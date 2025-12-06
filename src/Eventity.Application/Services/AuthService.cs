using System;
using System.Threading.Tasks;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eventity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ITwoFactorCodeService _twoFactorCodeService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        ITwoFactorCodeService twoFactorCodeService,
        IEmailService emailService,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _twoFactorCodeService = twoFactorCodeService;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResult> AuthenticateUser(string login, string password)
    {
        _logger.LogInformation("Authentication attempt for login: {Login}", login);
        
        try
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                _logger.LogWarning("User not found. Login: {Login}", login);
                throw new UserNotFoundException("User with this login is not found");
            }

            if (user.Password != password)
            {
                _logger.LogWarning("Invalid password for user. Login: {Login}", login);
                throw new InvalidPasswordException("Invalid password");
            }

            bool require2FA = _configuration.GetValue<bool>("Auth:Require2FA", false);
            
            if (require2FA)
            {
                var code = await _twoFactorCodeService.GenerateCodeAsync(user.Id);
                
                try
                {
                    var subject = "Your verification code";
                    var body = $"Your verification code is: {code}\n\nThis code will expire in 5 minutes.";
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                    
                    _logger.LogInformation("2FA email sent to {Email} for user {UserId}", user.Email, user.Id);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send 2FA email to {Email}", user.Email);
                }
                
                _logger.LogInformation("2FA required for user {UserId}. Code generated: {Code}", user.Id, code);
                
                return new AuthResult 
                { 
                    User = user, 
                    Requires2FA = true,
                    TwoFactorUserId = user.Id,
                    Token = null
                };
            }
            
            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("User authenticated successfully. UserId: {UserId}", user.Id);
            
            return new AuthResult 
            { 
                User = user, 
                Token = token,
                Requires2FA = false,
                TokenExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"]))
            };
        }
        catch (UserNotFoundException)
        {
            throw;
        }
        catch (InvalidPasswordException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for login: {Login}", login);
            throw new AuthServiceException("Failed to authenticate user", ex);
        }
    }

    public async Task<AuthResult> Verify2FA(Guid userId, string code)
    {
        _logger.LogInformation("Verifying 2FA code for user {UserId}", userId);
        
        try
        {
            var isValid = await _twoFactorCodeService.ValidateCodeAsync(userId, code);
            
            if (!isValid)
            {
                throw new Invalid2FACodeException("Invalid or expired verification code");
            }
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            var token = _jwtService.GenerateToken(user);
            
            _logger.LogInformation("2FA verification successful for user {UserId}", userId);
            
            return new AuthResult 
            { 
                User = user, 
                Token = token,
                Requires2FA = false,
                TokenExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"]))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "2FA verification failed for user {UserId}", userId);
            throw new AuthServiceException("Failed to verify 2FA code", ex);
        }
    }

    public async Task<AuthResult> RegisterUser(string name, string email, string login, string password, UserRoleEnum role)
    {
        _logger.LogInformation("Registration attempt. Login: {Login}, Email: {Email}", login, email);
        
        try
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user != null)
            {
                _logger.LogWarning("User already exists. Login: {Login}", login);
                throw new UserAlreadyExistsException("User with this login already exists");
            }

            var newUser = new User(Guid.NewGuid(), name, email, login, password, role);
            await _userRepository.AddAsync(newUser);
        
            var token = _jwtService.GenerateToken(newUser);
            _logger.LogInformation("User registered successfully. UserId: {UserId}", newUser.Id);
            
            return new AuthResult 
            { 
                User = newUser, 
                Token = token,
                Requires2FA = false,
                TokenExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"]))
            };
        }
        catch (UserAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for login: {Login}", login);
            throw new AuthServiceException("Failed to register user", ex);
        }
    }
    
    public async Task<ChangePasswordResult> ChangePassword(Guid userId, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Changing password for user {UserId}", userId);
        
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for password change. UserId: {UserId}", userId);
                return new ChangePasswordResult
                {
                    Success = false,
                    Message = "User not found",
                    ChangedAt = DateTime.UtcNow
                };
            }

            // Проверяем текущий пароль
            if (user.Password != currentPassword)
            {
                _logger.LogWarning("Invalid current password for user {UserId}", userId);
                return new ChangePasswordResult
                {
                    Success = false,
                    Message = "Current password is incorrect",
                    ChangedAt = DateTime.UtcNow
                };
            }

            // Проверяем сложность нового пароля (опционально)
            if (!IsPasswordValid(newPassword))
            {
                _logger.LogWarning("New password doesn't meet requirements for user {UserId}", userId);
                return new ChangePasswordResult
                {
                    Success = false,
                    Message = "New password doesn't meet security requirements",
                    ChangedAt = DateTime.UtcNow
                };
            }

            // Обновляем пароль
            user.Password = newPassword; // В реальном приложении здесь должно быть хеширование!
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Password successfully changed for user {UserId}", userId);
            
            return new ChangePasswordResult
            {
                Success = true,
                Message = "Password changed successfully",
                ChangedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change password for user {UserId}", userId);
            return new ChangePasswordResult
            {
                Success = false,
                Message = "Failed to change password due to server error",
                ChangedAt = DateTime.UtcNow
            };
        }
    }

    private bool IsPasswordValid(string password)
    {
        // Минимальные требования к паролю
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        if (password.Length < 6)
            return false;
        
        // Можно добавить дополнительные проверки:
        // - Наличие цифр
        // - Наличие букв в разных регистрах
        // - Наличие специальных символов
        
        return true;
    }
}