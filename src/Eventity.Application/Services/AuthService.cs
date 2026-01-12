using System;
using System.Threading.Tasks;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Eventity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository, 
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResult> AuthenticateUser(string login, string password)
    {
        _logger.LogInformation("Authentication attempt for login: {Login}", login);
        
        try
        {
            Console.WriteLine($"DEBUG: Looking for user with login: '{login}'");
            var user = await _userRepository.GetByLoginAsync(login);
            
            if (user == null)
            {
                Console.WriteLine($"DEBUG: User '{login}' NOT FOUND");
                _logger.LogWarning("User not found. Login: {Login}", login);
                throw new UserNotFoundException("User with this login is not found");
            }

            Console.WriteLine($"DEBUG: User found: {user.Login}");
            Console.WriteLine($"DEBUG: Expected password: '{user.Password}'");
            Console.WriteLine($"DEBUG: Provided password: '{password}'");
            Console.WriteLine($"DEBUG: Passwords match: {user.Password == password}");
            
            if (user.Password != password)
            {
                Console.WriteLine($"DEBUG: Password mismatch for user '{login}'");
                _logger.LogWarning("Invalid password for user. Login: {Login}", login);
                throw new InvalidPasswordException("Invalid password");
            }

            var token = _jwtService.GenerateToken(user);
            Console.WriteLine($"DEBUG: Authentication SUCCESS for user '{login}'");
            _logger.LogInformation("User authenticated successfully. UserId: {UserId}", user.Id);
            
            return new AuthResult { User = user, Token = token };
        }
        catch (UserNotFoundException ex)
        {
            Console.WriteLine($"DEBUG: UserNotFoundException: {ex.Message}");
            throw;
        }
        catch (InvalidPasswordException ex)
        {
            Console.WriteLine($"DEBUG: InvalidPasswordException: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Generic Exception: {ex.Message}");
            Console.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}");
            _logger.LogError(ex, "Authentication failed for login: {Login}", login);
            throw new AuthServiceException("Failed to authenticate user", ex);
        }
    }
        
    // public async Task<AuthResult> AuthenticateUser(string login, string password)
    // {
    //     _logger.LogInformation("Authentication attempt for login: {Login}", login);
        
    //     try
    //     {
    //         var user = await _userRepository.GetByLoginAsync(login);
    //         if (user == null)
    //         {
    //             _logger.LogWarning("User not found. Login: {Login}", login);
    //             throw new UserNotFoundException("User with this login is not found");
    //         }

    //         if (user.Password != password)
    //         {
    //             _logger.LogWarning("Invalid password for user. Login: {Login}", login);
    //             throw new InvalidPasswordException("Invalid password");
    //         }

    //         var token = _jwtService.GenerateToken(user);
    //         _logger.LogInformation("User authenticated successfully. UserId: {UserId}", user.Id);
            
    //         return new AuthResult { User = user, Token = token };
    //     }
    //     catch (UserNotFoundException)
    //     {
    //         throw;
    //     }
    //     catch (InvalidPasswordException)
    //     {
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Authentication failed for login: {Login}", login);
    //         throw new AuthServiceException("Failed to authenticate user", ex);
    //     }
    // }

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
            
            return new AuthResult { User = newUser, Token = token };
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
}