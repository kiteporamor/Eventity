using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Eventity.Application.Services;

public class TwoFactorCodeService : ITwoFactorCodeService
{
    private readonly ConcurrentDictionary<Guid, (string Code, DateTime ExpiresAt)> _codes 
        = new ConcurrentDictionary<Guid, (string, DateTime)>();
    
    private readonly ILogger<TwoFactorCodeService> _logger;

    public TwoFactorCodeService(ILogger<TwoFactorCodeService> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateCodeAsync(Guid userId)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        var testCode = Environment.GetEnvironmentVariable("TEST_2FA_CODE");
        if (!string.IsNullOrEmpty(testCode))
        {
            _codes[userId] = (testCode, expiresAt);
            return Task.FromResult(testCode);
        }

        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        _codes[userId] = (code, expiresAt);

        CleanupExpiredCodes();

        _logger.LogInformation("Generated 2FA code for user {UserId}", userId);
        return Task.FromResult(code);
    }

    public Task<bool> ValidateCodeAsync(Guid userId, string code)
    {
        if (!_codes.TryGetValue(userId, out var storedData))
        {
            return Task.FromResult(false);
        }

        var (storedCode, expiresAt) = storedData;

        _logger.LogInformation("2FA code for user {UserId} is {code}, inputed: {storedCode}", userId, code, storedCode);
        if (storedCode == code)
        {
            _codes.TryRemove(userId, out _);
            _logger.LogInformation("2FA code validated for user {UserId}", userId);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private void CleanupExpiredCodes()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _codes.Where(kvp => kvp.Value.ExpiresAt <= now)
                                .Select(kvp => kvp.Key)
                                .ToList();

        foreach (var key in expiredKeys)
        {
            _codes.TryRemove(key, out _);
        }
    }
}