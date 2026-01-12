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

public class EventRepository : IEventRepository
{
    private readonly IMongoCollection<EventDb> _collection;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(IMongoDatabase database, ILogger<EventRepository> logger)
    {
        _collection = database.GetCollection<EventDb>("events");
        _logger = logger;
    }

    public async Task<Event> AddAsync(Event eventDomain)
    {
        _logger.LogDebug("Attempting to add new event: {@Event}", eventDomain);

        try
        {
            var eventDb = eventDomain.ToDb();
            await _collection.InsertOneAsync(eventDb);
            
            _logger.LogInformation("Event added successfully: {EventId}", eventDb.Id);
            return eventDomain;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add event: {@Event}", eventDomain);
            throw new EventRepositoryException("Failed to create event", ex);
        }
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Retrieving event by Id: {EventId}", id);

        try
        {
            var eventDb = await _collection
                .Find(e => e.Id == id.ToString())
                .FirstOrDefaultAsync();

            if (eventDb is null)
            {
                _logger.LogWarning("Event not found: {EventId}", id);
                return null;
            }

            _logger.LogInformation("Event retrieved: {EventId}", id);
            return eventDb.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve event with Id: {EventId}", id);
            throw new EventRepositoryException("Failed to retrieve event", ex);
        }
    }

    public async Task<IEnumerable<Event>> GetByTitleAsync(string title)
    {
        _logger.LogDebug("Retrieving event by title: {Title}", title);

        try
        {
            var eventsDb = await _collection
                .Find(e => e.Title == title)
                .ToListAsync();

            _logger.LogInformation("Events retrieved by title: {Title}, count: {Count}", title, eventsDb.Count);
            return eventsDb.Select(e => e.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve event with title: {title}", title);
            throw new EventRepositoryException("Failed to retrieve event", ex);
        }
    }

    public async Task<IEnumerable<Event>> GetByOrganizerIdAsync(Guid id)
    {
        try
        {
            var eventsDb = await _collection
                .Find(e => e.OrganizerId == id.ToString())
                .ToListAsync();

            _logger.LogInformation("Events retrieved by organizer: {organizerId}, count: {Count}", id, eventsDb.Count);
            return eventsDb.Select(e => e.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve event with organizerId: {id}", id);
            throw new EventRepositoryException("Failed to retrieve event", ex);
        }
    }
    
    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        _logger.LogDebug("Retrieving all events");

        try
        {
            var eventsDb = await _collection
                .Find(_ => true)
                .ToListAsync();
                
            _logger.LogInformation("Retrieved {Count} events", eventsDb.Count);
            return eventsDb.Select(e => e.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events");
            throw new EventRepositoryException("Failed to retrieve events", ex);
        }
    }

    public async Task<Event> UpdateAsync(Event eventDomain)
    {
        _logger.LogDebug("Updating event: {@Event}", eventDomain);

        try
        {
            var filter = Builders<EventDb>.Filter.Eq(e => e.Id, eventDomain.Id.ToString());
            var update = Builders<EventDb>.Update
                .Set(e => e.Title, eventDomain.Title)
                .Set(e => e.Address, eventDomain.Address)
                .Set(e => e.Description, eventDomain.Description)
                .Set(e => e.DateTime, eventDomain.DateTime)
                .Set(e => e.OrganizerId, eventDomain.OrganizerId.ToString());

            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Event not found for update: {EventId}", eventDomain.Id);
                throw new EventRepositoryException($"Event with Id {eventDomain.Id} not found");
            }

            _logger.LogInformation("Event updated successfully: {EventId}", eventDomain.Id);
            return eventDomain;
        }
        catch (EventRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update event: {@Event}", eventDomain);
            throw new EventRepositoryException("Failed to update event", ex);
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        _logger.LogDebug("Removing event with Id: {EventId}", id);

        try
        {
            var result = await _collection.DeleteOneAsync(e => e.Id == id.ToString());

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("Event not found for removal: {EventId}", id);
                throw new EventRepositoryException($"Event with Id {id} not found");
            }

            _logger.LogInformation("Event removed: {EventId}", id);
        }
        catch (EventRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove event with Id: {EventId}", id);
            throw new EventRepositoryException("Failed to remove event", ex);
        }
    }
}