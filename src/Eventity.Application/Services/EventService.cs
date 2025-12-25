using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Eventity.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IParticipationRepository _participationRepository;
    private readonly ILogger<EventService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(
        IEventRepository eventRepository,
        IParticipationRepository participationRepository,
        ILogger<EventService> logger,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _participationRepository = participationRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Event> AddEvent(string title, string description, DateTime dateTime, string address, 
        Guid organizerId)
    {
        _logger.LogInformation("Trying to add event");
        try
        {
            var eventId = Guid.NewGuid();
            var eventDomain = new Event(eventId, title, description, dateTime, address, organizerId);
            var participation = new Participation(Guid.NewGuid(), organizerId, eventId, 
                ParticipationRoleEnum.Organizer, ParticipationStatusEnum.Accepted);

            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                await _eventRepository.AddAsync(eventDomain);
                await _participationRepository.AddAsync(participation);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("Event created successfully. ID: {EventId}, Title: {Title}, Organizer: {OrganizerId}", 
                    eventId, title, organizerId);
                return eventDomain;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create event. Transaction rolled back.");
                throw;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to create event. Title: {Title}, Organizer: {OrganizerId}", title, organizerId);
            throw new EventServiceException("Failed to create event", ex);
        }
    }

    public async Task<Event> GetEventById(Guid id)
    {
        _logger.LogInformation("Trying to get event by id");
        try
        {
            var eventDomain = await _eventRepository.GetByIdAsync(id);
            if (eventDomain == null)
            {
                _logger.LogWarning("Event not found. ID: {EventId}", id);
                throw new EventServiceException("Event not found");
            }
            
            _logger.LogInformation("Retrieved event successfully. ID: {EventId}, Title: {Title}", 
                id, eventDomain.Title);
            return eventDomain;
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get event. ID: {EventId}", id);
            throw new EventServiceException("Failed to get event", ex);
        }
    }
    
    public async Task<IEnumerable<Event>> GetEventByTitle(string title)
    {
        _logger.LogInformation("Trying to get event by title");
        try
        {
            var eventDomains = await _eventRepository.GetByTitleAsync(title);
            if (eventDomains == null)
            {
                _logger.LogWarning("Event not found. title: {title}", title);
                throw new EventServiceException("Event not found");
            }

            return eventDomains;
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get event. title: {title}", title);
            throw new EventServiceException("Failed to get event", ex);
        }
    }

    public async Task<IEnumerable<Event>> GetAllEvents()
    {
        _logger.LogInformation("Trying to get add events");
        try
        {
            var events = await _eventRepository.GetAllAsync();
            if (events == null || !events.Any()) 
            {
                _logger.LogWarning("No events found");
                throw new EventServiceException("No events found");
            }
            
            _logger.LogInformation("Retrieved {EventCount} events successfully", events.Count());
            return events;
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events");
            throw new EventServiceException("Failed to get events", ex);
        }
    }
    
    public async Task<Event> UpdateEvent(Guid id, string? title, string? description, DateTime? dateTime, 
        string? address, Validation validation)
    {
        _logger.LogInformation("Trying to update event");
        try
        {
            var eventDomain = await _eventRepository.GetByIdAsync(id);
            if (eventDomain == null)
            {
                _logger.LogWarning("Event not found for update. ID: {EventId}", id);
                throw new EventServiceException("Event not found");
            }

            if (validation.CurrentUserId != eventDomain.OrganizerId && !validation.IsAdmin)
            {
                _logger.LogWarning("Access denied. ID: {EventId}", id);
                throw new EventServiceException("Access denied.");
            }

            eventDomain.Title = title ?? eventDomain.Title;
            eventDomain.Description = description ?? eventDomain.Description;
            eventDomain.DateTime = dateTime ?? eventDomain.DateTime;
            eventDomain.Address = address ?? eventDomain.Address;

            var updatedEvent = await _eventRepository.UpdateAsync(eventDomain);

            _logger.LogInformation("Event updated successfully. ID: {EventId}, New title: {Title}",
                id, updatedEvent.Title);
            return updatedEvent;
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update event. ID: {EventId}", id);
            throw new EventServiceException("Failed to update event", ex);
        }
    }

    public async Task RemoveEvent(Guid id, Validation validation)
    {
        _logger.LogInformation("Trying to remove event");
        try
        {
            var eventDomain = await _eventRepository.GetByIdAsync(id);
            if (validation.CurrentUserId != eventDomain.OrganizerId && !validation.IsAdmin)
            {
                _logger.LogWarning("Access denied. ID: {EventId}", id);
                throw new EventServiceException("Access denied.");
            }
            await _eventRepository.RemoveAsync(id);
            _logger.LogInformation("Event removed successfully. ID: {EventId}", id);
        }
        catch (EventServiceException)
        {
            throw;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to remove event. ID: {EventId}", id);
            throw new EventServiceException("Failed to remove event", ex);
        }
    }
}