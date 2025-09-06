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

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IParticipationRepository _participationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IParticipationRepository participationRepository,
        IUserRepository userRepository,
        IEventRepository eventRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _participationRepository = participationRepository;
        _userRepository = userRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }
    
    public async Task<Notification> AddNotification(Guid participationId)
    {
        _logger.LogDebug("Trying to add notification");
        try
        {
            var participation = await _participationRepository.GetByIdAsync(participationId) 
                ?? throw new NotificationServiceException("Participation not found");
                
            if (participation.Status != ParticipationStatusEnum.Invited)
                throw new NotificationServiceException("Cannot create invitation for non-invited participation");

            var user = await _userRepository.GetByIdAsync(participation.UserId) 
                ?? throw new NotificationServiceException("User not found");
                
            var eventInfo = await _eventRepository.GetByIdAsync(participation.EventId) 
                ?? throw new NotificationServiceException("Event not found");

            var notification = new Notification(
                Guid.NewGuid(),
                participationId,
                GenerateInvitationText(user, eventInfo),
                DateTime.UtcNow);

            await _notificationRepository.AddAsync(notification);
            
            _logger.LogInformation("Notification created successfully. ID: {NotificationId}, Participation: {ParticipationId}, User: {UserId}, Event: {EventId}", 
                notification.Id, participationId, user.Id, eventInfo.Id);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification for participation {ParticipationId}", participationId);
            throw new NotificationServiceException("Failed to create notification", ex);
        }
    }

    public async Task<Notification> GetNotificationById(Guid id)
    {
        _logger.LogDebug("Trying to get notification by id");
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                _logger.LogWarning("Notification not found. ID: {NotificationId}", id);
                throw new NotificationServiceException("Notification not found");
            }
            
            _logger.LogInformation("Notification retrieved successfully. ID: {NotificationId}, Participation: {ParticipationId}", 
                id, notification.ParticipationId);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification. ID: {NotificationId}", id);
            throw new NotificationServiceException("Failed to get notification", ex);
        }
    }

    public async Task<Notification> GetNotificationByParticipationId(Guid participationId)
    {
        _logger.LogDebug("Trying to get notification by participation id");
        try
        {
            var notification = await _notificationRepository.GetByParticipationIdAsync(participationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification not found for participation. ParticipationID: {ParticipationId}", participationId);
                throw new NotificationServiceException("Notification not found");
            }
            
            _logger.LogInformation("Notification retrieved by participation successfully. ID: {NotificationId}, Participation: {ParticipationId}", 
                notification.Id, participationId);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification for participation {ParticipationId}", participationId);
            throw new NotificationServiceException("Failed to get notification", ex);
        }
    }

    public async Task<IEnumerable<Notification>> GetAllNotifications()
    {
        _logger.LogDebug("Trying to get all notifications");
        try
        {
            var notifications = await _notificationRepository.GetAllAsync();
            if (notifications?.Any() != true)
            {
                _logger.LogWarning("No notifications found in repository");
                throw new NotificationServiceException("No notifications found");
            }
            
            _logger.LogInformation("Retrieved {NotificationCount} notifications successfully", notifications.Count());
            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notifications");
            throw new NotificationServiceException("Failed to get notifications", ex);
        }
    }

    public async Task<Notification> UpdateNotification(Guid id, Guid? participationId, string? text, DateTime? sentAt)
    {
        _logger.LogDebug("Trying to update notification");
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                _logger.LogWarning("Notification not found for update. ID: {NotificationId}", id);
                throw new NotificationServiceException("Notification not found");
            }

            notification.ParticipationId = participationId ?? notification.ParticipationId;
            notification.Text = text ?? notification.Text;
            notification.SentAt = sentAt ?? notification.SentAt;

            var updatedNotification = await _notificationRepository.UpdateAsync(notification);
            
            _logger.LogInformation("Notification updated successfully. ID: {NotificationId}, New text length: {TextLength}", 
                id, updatedNotification.Text?.Length ?? 0);
            return updatedNotification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notification. ID: {NotificationId}", id);
            throw new NotificationServiceException("Failed to update notification", ex);
        }
    }

    public async Task RemoveNotification(Guid id)
    {
        _logger.LogDebug("Trying to remove notification");
        try
        {
            await _notificationRepository.RemoveAsync(id);
            _logger.LogInformation("Notification removed successfully. ID: {NotificationId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove notification. ID: {NotificationId}", id);
            throw new NotificationServiceException("Failed to remove notification", ex);
        }
    }
    
    private string GenerateInvitationText(User user, Event eventInfo)
    {
        return $"Dear {user.Name}! You are invited to the \"{eventInfo.Title}\" event, " +
               $"which will be held at \"{eventInfo.Address}\", " +
               $"at {eventInfo.DateTime:yyyy-MM-dd HH:mm}.\n" +
               $"Notification sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm}.";
    }
}
