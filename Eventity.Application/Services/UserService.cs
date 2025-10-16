using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> AddUser(string name, string email, string login, string password, UserRoleEnum role)
    {
        _logger.LogDebug("Trying to add user");
        try
        {
            var userId = Guid.NewGuid();
            var user = new User(userId, name, email, login, password, role);

            await _userRepository.AddAsync(user);
            _logger.LogInformation("User created successfully. ID: {UserId}, Login: {Login}, Role: {Role}", userId, login, role);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user. Login: {Login}, Email: {Email}", login, email);
            throw new UserServiceException("User service: Failed to create user", ex);
        }
    }

    public async Task<User> GetUserById(Guid id)
    {
        _logger.LogDebug("Trying to get user by id");
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found. ID: {UserId}", id);
                throw new UserServiceException("User service: Failed to find user by id.");
            }

            _logger.LogInformation("Retrieved user successfully. ID: {UserId}, Login: {Login}", id, user.Login);
            return user;
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by ID: {UserId}", id);
            throw new UserServiceException("User service: Failed to get user by id", ex);
        }
    }

    public async Task<User> GetUserByLogin(string login)
    {
        _logger.LogDebug("Trying to get user by login");
        try
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                _logger.LogWarning("User not found. Login: {Login}", login);
                throw new UserServiceException("User service: Failed to find user by login.");
            }

            _logger.LogInformation("Retrieved user successfully. Login: {Login}, ID: {UserId}", login, user.Id);
            return user;
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by login: {Login}", login);
            throw new UserServiceException("User service: Failed to get user by login", ex);
        }
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        _logger.LogDebug("Trying to get all users");
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userList = users as User[] ?? users.ToArray();

            if (userList.Length == 0)
            {
                _logger.LogWarning("No users found");
                throw new UserServiceException("User service: No users found.");
            }

            _logger.LogInformation("Retrieved {UserCount} users successfully", userList.Length);
            return userList;
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users");
            throw new UserServiceException("User service: Failed to get all users", ex);
        }
    }

    public async Task<IEnumerable<User>> GetUsers(string? login)
    {
        _logger.LogDebug("Trying to get all users");
        try
        {
            IEnumerable<User> users = new List<User>();
            if (string.IsNullOrEmpty(login))
            {
                users = await _userRepository.GetAllAsync();
            }
            else
            {
                var user = await _userRepository.GetByLoginAsync(login);
                users.Append(user);
            }
            var userList = users as User[] ?? users.ToArray();

            if (userList.Length == 0)
            {
                _logger.LogWarning("No users found");
                throw new UserServiceException("User service: No users found.");
            }
            _logger.LogInformation("Retrieved {UserCount} users successfully", userList.Length);
            return userList;
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users");
            throw new UserServiceException("User service: Failed to get all users", ex);
        }
    }

    public async Task<User> UpdateUser(Guid id, string? name, string? email, string? login, string? password)
    {
        _logger.LogDebug("Trying to update user");
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for update. ID: {UserId}", id);
                throw new UserServiceException("User service: Failed to update user, user does not exist.");
            }

            user.Name = name ?? user.Name;
            user.Email = email ?? user.Email;
            user.Login = login ?? user.Login;
            user.Password = password ?? user.Password;

            var updatedUser = await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User updated successfully. ID: {UserId}, New Login: {Login}", id, updatedUser.Login);
            return updatedUser;
        }
        catch (UserServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user. ID: {UserId}", id);
            throw new UserServiceException("User service: Failed to update user", ex);
        }
    }

    public async Task RemoveUser(Guid id)
    {
        _logger.LogDebug("Trying to remove user");
        try
        {
            await _userRepository.RemoveAsync(id);
            _logger.LogInformation("User removed successfully. ID: {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove user. ID: {UserId}", id);
            throw new UserServiceException("User service: Failed to remove user", ex);
        }
    }
}
