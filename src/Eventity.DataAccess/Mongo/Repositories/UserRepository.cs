using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Eventity.DataAccess.Converters.Mongo;
using Eventity.DataAccess.Models.Mongo;
using Eventity.Domain.Exceptions;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Repositories.Mongo;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDb> _collection;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IMongoDatabase database, ILogger<UserRepository> logger)
    {
        _collection = database.GetCollection<UserDb>("users");
        _logger = logger;
    }

    public async Task<User> AddAsync(User user)
    {
        try
        {
            var userDb = user.ToDb();
            if (userDb.Login.Length > 30)
                throw new ArgumentException("Login cannot exceed 30 characters");
                
            await _collection.InsertOneAsync(userDb);
            return user;
        }
        catch (ArgumentException)
        {
            throw;
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
            var userDb = await _collection
                .Find(u => u.Id == id.ToString())
                .FirstOrDefaultAsync();

            if (userDb is null)
            {
                _logger.LogWarning("User with Id {UserId} not found", id);
                throw new UserRepositoryException($"User with Id {id} not found");
            }

            return userDb.ToDomain();
        }
        catch (UserRepositoryException)
        {
            throw;
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
            var userDb = await _collection
                .Find(u => u.Login == login)
                .FirstOrDefaultAsync();

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
            var usersDb = await _collection
                .Find(_ => true)
                .ToListAsync();
                
            return usersDb.Select(u => u.ToDomain());
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
            var filter = Builders<UserDb>.Filter.Eq(u => u.Id, user.Id.ToString());
            var update = Builders<UserDb>.Update
                .Set(u => u.Name, user.Name)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Login, user.Login)
                .Set(u => u.Password, user.Password);

            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("User with Id {UserId} not found for update", user.Id);
                throw new UserRepositoryException($"User with Id {user.Id} not found");
            }

            return user;
        }
        catch (UserRepositoryException)
        {
            throw;
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
            var result = await _collection.DeleteOneAsync(u => u.Id == id.ToString());

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("User with Id {UserId} not found for removal", id);
                throw new UserRepositoryException($"User with Id {id} not found");
            }
        }
        catch (UserRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing user with Id {UserId}", id);
            throw new UserRepositoryException("Failed to remove user", ex);
        }
    }
}