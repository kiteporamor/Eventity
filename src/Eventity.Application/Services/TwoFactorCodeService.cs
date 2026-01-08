using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eventity.Application.Services;

public class TwoFactorCodeService : ITwoFactorCodeService
{
    private readonly ConcurrentDictionary<Guid, TwoFactorState> _states 
        = new ConcurrentDictionary<Guid, TwoFactorState>();
    
    private readonly ILogger<TwoFactorCodeService> _logger;
    private readonly int _maxAttempts;
    private readonly TimeSpan _lockoutDuration;

    public TwoFactorCodeService(ILogger<TwoFactorCodeService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _maxAttempts = configuration.GetValue<int>("Auth:TwoFactorMaxAttempts", 3);
        var lockoutSeconds = configuration.GetValue<int>("Auth:TwoFactorLockoutSeconds", 300);
        _lockoutDuration = TimeSpan.FromSeconds(lockoutSeconds);
    }

    public Task<string> GenerateCodeAsync(Guid userId)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        _states.TryGetValue(userId, out var existingState);
        var now = DateTime.UtcNow;
        var activeLockout = existingState?.LockoutUntil != null && existingState.LockoutUntil > now;
        var testCode = Environment.GetEnvironmentVariable("TEST_2FA_CODE");
        if (!string.IsNullOrEmpty(testCode))
        {
            _states[userId] = new TwoFactorState(
                testCode,
                expiresAt,
                activeLockout ? existingState!.FailedAttempts : 0,
                activeLockout ? existingState!.LockoutUntil : null);
            return Task.FromResult(testCode);
        }

        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        _states[userId] = new TwoFactorState(
            code,
            expiresAt,
            activeLockout ? existingState!.FailedAttempts : 0,
            activeLockout ? existingState!.LockoutUntil : null);

        CleanupExpiredCodes();

        _logger.LogInformation("Generated 2FA code for user {UserId}", userId);
        return Task.FromResult(code);
    }

    public Task<TwoFactorValidationResult> ValidateCodeAsync(Guid userId, string code)
    {
        if (!_states.TryGetValue(userId, out var storedData))
        {
            return Task.FromResult(new TwoFactorValidationResult
            {
                IsValid = false,
                IsLockedOut = false,
                RemainingAttempts = _maxAttempts
            });
        }

        var now = DateTime.UtcNow;
        if (storedData.LockoutUntil.HasValue && storedData.LockoutUntil.Value > now)
        {
            return Task.FromResult(new TwoFactorValidationResult
            {
                IsValid = false,
                IsLockedOut = true,
                RemainingAttempts = 0,
                LockoutUntil = storedData.LockoutUntil
            });
        }

        if (storedData.ExpiresAt <= now)
        {
            _states.TryRemove(userId, out _);
            return Task.FromResult(new TwoFactorValidationResult
            {
                IsValid = false,
                IsLockedOut = false,
                RemainingAttempts = _maxAttempts
            });
        }

        _logger.LogInformation("2FA code for user {UserId} is {code}, inputed: {storedCode}", userId, code, storedData.Code);
        if (storedData.Code == code)
        {
            _states.TryRemove(userId, out _);
            _logger.LogInformation("2FA code validated for user {UserId}", userId);
            return Task.FromResult(new TwoFactorValidationResult
            {
                IsValid = true,
                IsLockedOut = false,
                RemainingAttempts = _maxAttempts
            });
        }

        var failedAttempts = storedData.FailedAttempts + 1;
        var lockoutUntil = failedAttempts >= _maxAttempts ? now.Add(_lockoutDuration) : (DateTime?)null;
        var remainingAttempts = Math.Max(0, _maxAttempts - failedAttempts);

        _states[userId] = storedData with
        {
            FailedAttempts = failedAttempts,
            LockoutUntil = lockoutUntil
        };

        return Task.FromResult(new TwoFactorValidationResult
        {
            IsValid = false,
            IsLockedOut = lockoutUntil.HasValue,
            RemainingAttempts = remainingAttempts,
            LockoutUntil = lockoutUntil
        });
    }

    private void CleanupExpiredCodes()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _states.Where(kvp => kvp.Value.ExpiresAt <= now)
                                .Select(kvp => kvp.Key)
                                .ToList();

        foreach (var key in expiredKeys)
        {
            _states.TryRemove(key, out _);
        }
    }

    private sealed record TwoFactorState(string Code, DateTime ExpiresAt, int FailedAttempts, DateTime? LockoutUntil);
}
