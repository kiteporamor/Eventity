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

public class ParticipationRepository : IParticipationRepository
{
    private readonly IMongoCollection<ParticipationDb> _collection;
    private readonly ILogger<ParticipationRepository> _logger;

    public ParticipationRepository(IMongoDatabase database, ILogger<ParticipationRepository> logger)
    {
        _collection = database.GetCollection<ParticipationDb>("participations");
        _logger = logger;
    }

    public async Task<Participation> AddAsync(Participation participation)
    {
        try
        {
            var participationDb = participation.ToDb();
            await _collection.InsertOneAsync(participationDb);
            return participation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating participation");
            throw new ParticipationRepositoryException("Failed to create participation", ex);
        }
    }

    public async Task<Participation?> GetByIdAsync(Guid id)
    {
        try
        {
            var participationDb = await _collection
                .Find(p => p.Id == id.ToString())
                .FirstOrDefaultAsync();
                
            return participationDb?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participation with Id {ParticipationId}", id);
            throw new ParticipationRepositoryException("Failed to retrieve participation", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var participations = await _collection
                .Find(p => p.UserId == userId.ToString())
                .ToListAsync();
            
            return participations.Select(p => p.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participations by user Id {UserId}", userId);
            throw new ParticipationRepositoryException("Failed to retrieve participations by user", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetByEventIdAsync(Guid eventId)
    {
        try
        {
            var participations = await _collection
                .Find(p => p.EventId == eventId.ToString())
                .ToListAsync();
            
            return participations.Select(p => p.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participations by event Id {EventId}", eventId);
            throw new ParticipationRepositoryException("Failed to retrieve participations by event", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetAllAsync()
    {
        try
        {
            var participations = await _collection
                .Find(_ => true)
                .ToListAsync();
                
            return participations.Select(p => p.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all participations");
            throw new ParticipationRepositoryException("Failed to retrieve all participations", ex);
        }
    }

    public async Task<Participation> UpdateAsync(Participation participation)
    {
        try
        {
            var filter = Builders<ParticipationDb>.Filter.Eq(p => p.Id, participation.Id.ToString());
            var update = Builders<ParticipationDb>.Update
                .Set(p => p.UserId, participation.UserId.ToString())
                .Set(p => p.EventId, participation.EventId.ToString())
                .Set(p => p.Status, participation.Status);

            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Participation with Id {ParticipationId} not found for update", participation.Id);
                throw new ParticipationRepositoryException($"Participation with Id {participation.Id} not found");
            }

            return participation;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating participation with Id {ParticipationId}", participation.Id);
            throw new ParticipationRepositoryException("Failed to update participation", ex);
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        try
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id.ToString());

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("Participation with Id {ParticipationId} not found for removal", id);
                throw new ParticipationRepositoryException($"Participation with Id {id} not found");
            }
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing participation with Id {ParticipationId}", id);
            throw new ParticipationRepositoryException("Failed to remove participation", ex);
        }
    }
}