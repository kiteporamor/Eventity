using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.DataAccess.Context.Postgres;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Eventity.DataAccess.Converters.Postgres;
using Eventity.DataAccess.Models.Postgres;
using Eventity.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Repositories.Postgres;

public class EventRepository : IEventRepository
{
    private readonly EventityDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(EventityDbContext context, ILogger<EventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Event> AddAsync(Event eventDomain)
    {
        _logger.LogDebug("Attempting to add new event: {@Event}", eventDomain);

        try
        {
            var eventDb = eventDomain.ToDb();
            await _context.Events.AddAsync(eventDb);
            var isSave = await _context.SaveChangesAsync() > 0;

            if (isSave)
                _logger.LogInformation("Event added successfully: {EventId}", eventDb.Id);
            else
                _logger.LogWarning("No changes saved while adding event: {EventId}", eventDb.Id);

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
            var eventDb = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

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
            var eventDb = await _context.Events.Where(e => e.Title == title).ToListAsync();

            if (eventDb is null)
            {
                _logger.LogWarning("Event not found: {title}", title);
                return null;
            }

            _logger.LogInformation("Event retrieved: {title}", title);
            return eventDb.Select(e => e.ToDomain());
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
            var eventDb = await _context.Events.Where(e => e.OrganizerId == id).ToListAsync();

            if (eventDb is null)
            {
                _logger.LogWarning("Event not found: {organizerId}", id);
                return null;
            }

            _logger.LogInformation("Event retrieved: {organizerId}", id);
            return eventDb.Select(e => e.ToDomain());
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
            var eventsDb = await _context.Events.ToListAsync();
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
            var eventDb = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventDomain.Id);

            if (eventDb is null)
            {
                _logger.LogWarning("Event not found for update: {EventId}", eventDomain.Id);
                throw new EventRepositoryException($"Event with Id {eventDomain.Id} not found");
            }

            eventDb.Title = eventDomain.Title;
            eventDb.Address = eventDomain.Address;
            eventDb.Description = eventDomain.Description;
            eventDb.DateTime = eventDomain.DateTime;
            eventDb.OrganizerId = eventDomain.OrganizerId;

            _context.Events.Update(eventDb);
            await _context.SaveChangesAsync();

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
            var eventDb = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

            if (eventDb is null)
            {
                _logger.LogWarning("Event not found for removal: {EventId}", id);
                throw new EventRepositoryException($"Event with Id {id} not found");
            }

            _context.Events.Remove(eventDb);
            var isSave = await _context.SaveChangesAsync() > 0;

            if (isSave)
                _logger.LogInformation("Event removed: {EventId}", id);
            else
                _logger.LogWarning("No changes saved while removing event: {EventId}", id);
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
