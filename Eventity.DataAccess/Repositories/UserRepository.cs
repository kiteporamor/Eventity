using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.DataAccess.Context;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Models;
using Eventity.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly EventityDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(EventityDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User> AddAsync(User user)
    {
        try
        {
            var userDb = user.ToDb();
            if (userDb.Login.Length > 30)
                throw new ArgumentException("Login cannot exceed 30 characters");
            await _context.Users.AddAsync(userDb);
            await _context.SaveChangesAsync();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            throw new UserRepositoryException("Failed to create user", ex);
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userDb is null)
            {
                _logger.LogWarning("User with Id {UserId} not found", id);
                throw new UserRepositoryException($"User with Id {id} not found");
            }

            return userDb.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with Id {UserId}", id);
            throw new UserRepositoryException("Failed to retrieve user", ex);
        }
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        try
        {
            var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (userDb is null)
            {
                _logger.LogWarning("User with login {Login} not found", login);
                return null;
            }

            return userDb.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with login {Login}", login);
            throw new UserRepositoryException("Failed to retrieve user", ex);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            var userDb = await _context.Users.ToListAsync();
            return userDb.Select(u => u.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all users");
            throw new UserRepositoryException("Failed to retrieve users", ex);
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userDb is null)
            {
                _logger.LogWarning("User with Id {UserId} not found for update", user.Id);
                throw new UserRepositoryException($"User with Id {user.Id} not found");
            }

            userDb.Name = user.Name;
            userDb.Email = user.Email;
            userDb.Login = user.Login;
            userDb.Password = user.Password;

            _context.Users.Update(userDb);
            await _context.SaveChangesAsync();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with Id {UserId}", user.Id);
            throw new UserRepositoryException("Failed to update user", ex);
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        try
        {
            var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userDb is null)
            {
                _logger.LogWarning("User with Id {UserId} not found for removal", id);
                throw new UserRepositoryException($"User with Id {id} not found");
            }

            _context.Users.Remove(userDb);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing user with Id {UserId}", id);
            throw new UserRepositoryException("Failed to remove user", ex);
        }
    }
}
