using System;
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
    // private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<ParticipationService> _logger;

    public ParticipationService(IParticipationRepository participationRepository,
        ILogger<ParticipationService> logger)
    {
        _participationRepository = participationRepository;
        // _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Participation> AddParticipation(Guid userId, Guid eventId, ParticipationRoleEnum participationRole, 
        ParticipationStatusEnum status)
    {
        _logger.LogDebug("Trying to add participation");
        try
        {
            var participationId = Guid.NewGuid();
            var participation = new Participation(participationId, userId, eventId, participationRole, status);

            await _participationRepository.AddAsync(participation);
            
            _logger.LogInformation("Participation created. ID: {ParticipationId}, " +
                                   "UserID: {UserId}, EventID: {EventId}", participationId, userId, eventId);
            return participation;
        }
        catch(ParticipationRepositoryException ex)
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
        catch (ParticipationRepositoryException ex)
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
        catch (ParticipationRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to get participations by event id");
            throw new ParticipationServiceException("Failed to get participations by event id", ex);
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
        catch (ParticipationRepositoryException ex)
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
        catch (ParticipationRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to find organizer for event id: {EventId}", eventId);
            throw new ParticipationServiceException("Failed to get organizer by event id", ex);
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
        catch (ParticipationRepositoryException ex)
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
        catch (ParticipationRepositoryException ex)
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
        catch (ParticipationRepositoryException ex)
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
                throw new ParticipationServiceException("Failed to retrieve participations or no participations found.");
            }
            
            _logger.LogInformation("All participations found");
            return participationDomainModels;
        }
        catch (ParticipationRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to get all participations");
            throw new ParticipationServiceException("Failed to get all participations", ex);
        }
    }
    
    public async Task<Participation> UpdateParticipation(Guid id, ParticipationStatusEnum? status)
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

            participation.Status = status ?? participation.Status;

            _logger.LogInformation("Participation updated: {Id}", id);
            return await _participationRepository.UpdateAsync(participation);
        }
        catch (ParticipationRepositoryException ex)
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
        catch(ParticipationRepositoryException ex)
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
        catch(ParticipationRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to change participation role: {Id}", id);
            throw new ParticipationServiceException("Failed to change participation role", ex);
        }
    }

    public async Task RemoveParticipation(Guid id)
    {
        _logger.LogDebug("Trying to remove participation");
        try
        {
            await _participationRepository.RemoveAsync(id);
            _logger.LogInformation("Participation removed: {Id}", id);
        }
        catch(ParticipationRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to remove participation: {Id}", id);
            throw new ParticipationServiceException("Failed to remove participation", ex);
        }
    }
}
