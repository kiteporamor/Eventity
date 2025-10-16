using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Eventity.Application.Services;

public class ParticipationService : IParticipationService
{
    private readonly IParticipationRepository _participationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ParticipationService> _logger;

    public ParticipationService(IParticipationRepository participationRepository, 
        IEventRepository eventRepository, IUserRepository userRepository, 
        INotificationService notificationService, ILogger<ParticipationService> logger)
    {
        _participationRepository = participationRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Participation> AddParticipation(Guid userId, Guid eventId, ParticipationRoleEnum participationRole, 
        ParticipationStatusEnum status, Validation validation)
    {
        _logger.LogDebug("Trying to add participation");
        try
        {
            var eventDb = await _eventRepository.GetByIdAsync(eventId);
            if (eventDb.OrganizerId != validation.CurrentUserId && validation.IsAdmin)
            {
                throw new ParticipationServiceException("Access Denied.");
            }

            var participationId = Guid.NewGuid();
            var participation = new Participation(participationId, userId, eventId, participationRole, status);
            await _participationRepository.AddAsync(participation);

            _notificationService.AddNotification(participationId, NotificationTypeEnum.Invintation, validation);

            _logger.LogInformation("Participation created. ID: {ParticipationId}, " +
                                   "UserID: {UserId}, EventID: {EventId}", participationId, userId, eventId);
            return participation;
        }
        catch (ParticipationServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create participation");
            throw new ParticipationServiceException("Failed to create participation", ex);
        }
    }

    public async Task<Participation> GetParticipationById(Guid id)
    {
        _logger.LogDebug("Trying to get participation by id");
        try
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            if (participation == null)
            {
                _logger.LogWarning("Failed to find participation by id: {Id}", id);
                throw new ParticipationServiceException("Participation service: Failed to find participation by id.");
            }
            _logger.LogInformation("Failed to find participation by id: {Id}", id);
            return participation;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participation by id");
            throw new ParticipationServiceException("Failed to get participation by id", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetParticipationsByEventId(Guid eventId)
    {
        _logger.LogDebug("Trying to get participation by event id");
        try
        {
            var participations = await _participationRepository.GetByEventIdAsync(eventId);
            if (participations == null || !participations.Any())
            {
                _logger.LogWarning("Failed to find participation by event id: {EventId}", eventId);
                throw new ParticipationServiceException("Failed to find participations by event id.");
            }
            
            _logger.LogInformation("Participation found by event id: {EventId}", eventId);
            return participations;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participations by event id");
            throw new ParticipationServiceException("Failed to get participations by event id", ex);
        }
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationsDetailed(
        string? organizer_login, string? event_title, Validation validation, Guid? user_id)
    {
        _logger.LogDebug("Trying to get participation detailed by filters");
        try
        {
            Guid userId = validation.IsAdmin && user_id.HasValue 
                ? user_id.Value 
                : validation.CurrentUserId;
            
            if (string.IsNullOrEmpty(organizer_login) && string.IsNullOrEmpty(event_title))
            {
                return await GetUserParticipationInfoByUserId(userId);
            }

            var byOrganizer = !string.IsNullOrEmpty(organizer_login)
                ? await GetUserParticipationInfoByOrganizerLogin(userId, organizer_login)
                : Enumerable.Empty<UserParticipationInfo>();

            var byEvent = !string.IsNullOrEmpty(event_title)
                ? await GetUserParticipationInfoByEventTitle(userId, event_title)
                : Enumerable.Empty<UserParticipationInfo>();
            
            return !string.IsNullOrEmpty(organizer_login) && !string.IsNullOrEmpty(event_title)
                ? byEvent.Intersect(byOrganizer)
                : byEvent.Union(byOrganizer);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participation detailed by filters");
            throw new ParticipationServiceException("Failed to get participation detailed by filters", ex);
        }
    }
    
    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByUserId(Guid userId)
    {
        _logger.LogDebug("Trying to get participation by event id");
        try
        {
            List<UserParticipationInfo> userParticipationInfos = new List<UserParticipationInfo>();
            var participations = await _participationRepository.GetByUserIdAsync(userId);
            foreach (var participation in participations)
            {
                if (participation.Status == ParticipationStatusEnum.Accepted)
                {
                    var eventItem = await _eventRepository.GetByIdAsync(participation.EventId);
                    var organizer = await _userRepository.GetByIdAsync(eventItem.OrganizerId);
                    var userParticipationInfo = new UserParticipationInfo(eventItem, organizer.Id, organizer.Login);
                    userParticipationInfos.Add(userParticipationInfo);
                }
            }
            if (userParticipationInfos == null || !userParticipationInfos.Any())
            {
                _logger.LogWarning("Failed to find participation by user id: {UserId}", userId);
                throw new ParticipationServiceException("Failed to find participations by user id.");
            }
            
            _logger.LogInformation("Participation found by user id: {UserId}", userId);
            return userParticipationInfos;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participations by user id");
            throw new ParticipationServiceException("Failed to get participations by user id", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetParticipationsByUserId(Guid userId)
    {
        _logger.LogDebug("Trying to get participation by user id");
        try
        {
            var participations = await _participationRepository.GetByUserIdAsync(userId);
            if (participations == null || !participations.Any())
            {
                _logger.LogWarning("Failed to find participation by user id: {UserId}", userId);
                throw new ParticipationServiceException("Failed to find participations by user id.");
            }
            
            _logger.LogInformation("Participation found by user id: {UserId}", userId);
            return participations;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participations by user id");
            throw new ParticipationServiceException("Failed to get participations by user id", ex);
        }
    }

    public async Task<Participation> GetOrganizerByEventId(Guid eventId)
    {
        _logger.LogDebug("Trying to get organizer by event id");
        try
        {
            var participations = await _participationRepository.GetByEventIdAsync(eventId);
            if (participations == null || !participations.Any())
            {
                _logger.LogWarning("Organizer found by event id: {EventId}", eventId);
                throw new ParticipationServiceException("Failed to find any participations for event id.");
            }

            var organizer = participations.FirstOrDefault(p =>
                p.Role == ParticipationRoleEnum.Organizer);

            if (organizer == null)
            {
                _logger.LogWarning("Failed to find organizer for event id: {EventId}", eventId);
                throw new ParticipationServiceException("Failed to find organizer for event id.");
            }

            _logger.LogInformation("Organizer found for event id: {EventId}", eventId);
            return organizer;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find organizer for event id: {EventId}", eventId);
            throw new ParticipationServiceException("Failed to get organizer by event id", ex);
        }
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByEventTitle(Guid userId, string title)
    {
        try
        {
            List<UserParticipationInfo> userParticipationInfos = new List<UserParticipationInfo>();
            List<Event> events = new List<Event>();
            var participations = await _participationRepository.GetByUserIdAsync(userId);
            foreach (var participation in participations)
            {
                if (participation.Status == ParticipationStatusEnum.Accepted)
                {
                    var eventItem = await _eventRepository.GetByIdAsync(participation.EventId);
                    if (eventItem.Title == title)
                        events.Add(eventItem);
                }
            }
            foreach (var eventItem in events)
            {
                var organizer = await _userRepository.GetByIdAsync(eventItem.OrganizerId);
                var userParticipationInfo = new UserParticipationInfo(eventItem, organizer.Id, organizer.Login);
                userParticipationInfos.Add(userParticipationInfo);
            }

            return userParticipationInfos;
        }
        catch (Exception ex)
        {
            throw new ParticipationServiceException("Failed to get participation info by event title", ex);
        }
    }
    
    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByOrganizerLogin(Guid userId, string login)
    {
        try
        {
            List<UserParticipationInfo> userParticipationInfos = new List<UserParticipationInfo>();
            var participations = await _participationRepository.GetByUserIdAsync(userId);
            foreach (var participation in participations)
            {
                if (participation.Status == ParticipationStatusEnum.Accepted)
                {
                    var eventItem = await _eventRepository.GetByIdAsync(participation.EventId);
                    var organizer = await _userRepository.GetByIdAsync(eventItem.OrganizerId);
                    if (organizer.Login == login)
                    {
                        var userParticipationInfo = new UserParticipationInfo(eventItem, organizer.Id, organizer.Login);
                        userParticipationInfos.Add(userParticipationInfo);
                    }
                }
            }
            return userParticipationInfos;
        }
        catch (Exception ex)
        {
            throw new ParticipationServiceException("Failed to get participation info by event title", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetAllParticipantsByEventId(Guid eventId)
    {
        _logger.LogDebug("Trying to get all participations by event id");
        try
        {
            var participations = await _participationRepository.GetByEventIdAsync(eventId);
            if (participations == null || !participations.Any())
            {
                _logger.LogWarning("Failed to find participants by event id: {EventId}", eventId);
                throw new ParticipationServiceException("Failed to find participants by event id.");
            }

            _logger.LogInformation("Successfully got participants by event id: {EventId}", eventId);
            return participations.Where(p => p.Role == ParticipationRoleEnum.Participant);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participants by event id: {EventId}", eventId);
            throw new ParticipationServiceException("Failed to get participants by event id", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetAllLeftParticipantsByEventId(Guid eventId)
    {
        _logger.LogDebug("Trying to get all left participations by event id");
        try
        {
            var participations = await _participationRepository.GetByEventIdAsync(eventId);
            if (participations == null || !participations.Any())
            {
                _logger.LogWarning("Failed to find left participants by event id: {EventId}", eventId);
                throw new ParticipationServiceException("Failed to find left participants by event id.");
            }

            _logger.LogInformation("Successfully got left participants by event id: {EventId}", eventId);
            return participations.Where(p => p.Role == ParticipationRoleEnum.Left);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get left participants by event id: {EventId}", eventId);
            throw new ParticipationServiceException("Failed to get left participants by event id", ex);
        }
    }
    
    public async Task<Participation> GetParticipationByUserIdAndEventId(Guid userId, Guid eventId)
    {
        _logger.LogDebug("Trying to get participation by user id and event id");
        try
        {
            var participations = await _participationRepository.GetByUserIdAsync(userId);
            var participation = participations.FirstOrDefault(p => p.EventId == eventId);

            if (participation == null)
            {
                _logger.LogWarning("Failed to find participants by by user id {UserId} and event id {EventId}",
                    userId, eventId);
                throw new ParticipationServiceException("Participation not found");
            }

            _logger.LogInformation("Successfully got participants by user id {UserId} and event id {EventId}",
                userId, eventId);
            return participation;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participants by user id {UserId} and event id {EventId}",
                userId, eventId);
            throw new ParticipationServiceException("Failed to get participation", ex);
        }
    }
    
    public async Task<IEnumerable<Participation>> GetAllParticipations()
    {
        _logger.LogDebug("Trying to get all participations");
        try
        {
            var participations = await _participationRepository.GetAllAsync();
            var participationDomainModels = participations as Participation[] ?? participations.ToArray();
            if (participations == null || !participationDomainModels.Any())
            {
                _logger.LogWarning("Failed to retrieve participations or no participations found");
                throw new ParticipationServiceException(
                    "Failed to retrieve participations or no participations found.");
            }

            _logger.LogInformation("All participations found");
            return participationDomainModels;
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all participations");
            throw new ParticipationServiceException("Failed to get all participations", ex);
        }
    }
    
    public async Task<Participation> UpdateParticipation(Guid id, ParticipationStatusEnum? status, 
        Validation validation)
    {
        _logger.LogDebug("Trying to update participation");
        try
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            if (participation == null)
            {
                _logger.LogWarning("Failed to update participation, participation does not exist.");
                throw new ParticipationServiceException("Failed to update participation, " +
                                                        "participation does not exist.");
            }

            if (participation.UserId != validation.CurrentUserId && !validation.IsAdmin)
            {
                throw new ParticipationRepositoryException("Access denied.");
            }

            participation.Status = status ?? participation.Status;

            _logger.LogInformation("Participation updated: {Id}", id);
            return await _participationRepository.UpdateAsync(participation);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update participation {Id}", id);
            throw new ParticipationServiceException("Failed to update participation", ex);
        }
    }
    
    public async Task<Participation> ChangeParticipationStatus(Guid id, ParticipationStatusEnum status)
    {
        _logger.LogDebug("Trying to change participation status");
        try
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            if (participation == null)
            {
                _logger.LogWarning("Failed to change participation status, participation does not exist: {Id}", id);
                throw new ParticipationServiceException("Failed to change participation status, "
                                                        + "participation does not exist");
            }

            participation.Status = status;

            _logger.LogInformation("Participation status changed: {Id}", id);
            return await _participationRepository.UpdateAsync(participation);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change participation status: {Id}", id);
            throw new ParticipationServiceException("Failed to change participation status", ex);
        }
    }

    public async Task<Participation> ChangeParticipationRole(Guid id, ParticipationRoleEnum participationRole)
    {
        _logger.LogDebug("Trying to change participation role");
        try
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            if (participation == null)
            {
                _logger.LogWarning("Failed to change participation role, participation does not exist: {Id}", id);
                throw new ParticipationServiceException("Failed to change participation role, " +
                                                        "participation does not exist");
            }

            participation.Role = participationRole;
            
            _logger.LogInformation("Participation role changed: {Id}", id);
            return await _participationRepository.UpdateAsync(participation);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change participation role: {Id}", id);
            throw new ParticipationServiceException("Failed to change participation role", ex);
        }
    }

    public async Task RemoveParticipation(Guid id, Validation validation)
    {
        _logger.LogDebug("Trying to remove participation");
        try
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            if (participation.UserId != validation.CurrentUserId && !validation.IsAdmin)
            {
                throw new ParticipationRepositoryException("Access Denied.");
            }

            await _participationRepository.RemoveAsync(id);
            _logger.LogInformation("Participation removed: {Id}", id);
        }
        catch (ParticipationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove participation: {Id}", id);
            throw new ParticipationServiceException("Failed to remove participation", ex);
        }
    }
}
