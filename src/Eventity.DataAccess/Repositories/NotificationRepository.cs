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

public class NotificationRepository : INotificationRepository
{
    private readonly EventityDbContext _context;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(EventityDbContext context, ILogger<NotificationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        try
        {
            _logger.LogInformation("Adding new notification with Id {Id}", notification.Id);
            var notificationDb = notification.ToDb();
            await _context.Notifications.AddAsync(notificationDb);
            var isSave = await _context.SaveChangesAsync() > 0;

            _logger.LogDebug("SaveChanges result: {IsSave}", isSave);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification with Id {Id}", notification.Id);
            throw new NotificationRepositoryException("Failed to create notification", ex);
        }
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting notification by Id: {Id}", id);
            var notificationDb = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

            if (notificationDb is null)
            {
                _logger.LogWarning("Notification with Id {Id} not found", id);
                return null;
            }

            _logger.LogDebug("Notification found: {Notification}", notificationDb);
            return notificationDb.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notification with Id {Id}", id);
            throw new NotificationRepositoryException("Failed to retrieve notification", ex);
        }
    }

    public async Task<Notification?> GetByParticipationIdAsync(Guid participationId)
    {
        try
        {
            _logger.LogInformation("Getting notification by ParticipationId: {ParticipationId}", participationId);
            var notificationDb = await _context.Notifications.FirstOrDefaultAsync(
                n => n.ParticipationId == participationId);

            if (notificationDb is null)
            {
                _logger.LogWarning("Notification with ParticipationId {ParticipationId} not found", participationId);
                throw new NotificationRepositoryException(
                    $"Notification with ParticipationId {participationId} not found");
            }

            _logger.LogDebug("Notification found: {Notification}", notificationDb);
            return notificationDb.ToDomain();
        }
        catch (NotificationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notification with ParticipationId {ParticipationId}", participationId);
            throw new NotificationRepositoryException("Failed to retrieve notification", ex);
        }
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Getting all notifications");
            var notificationsDb = await _context.Notifications.ToListAsync();

            _logger.LogDebug("Total notifications retrieved: {Count}", notificationsDb.Count);
            return notificationsDb.Select(n => n.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notifications");
            throw new NotificationRepositoryException("Failed to retrieve notifications", ex);
        }
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        try
        {
            _logger.LogInformation("Updating notification with Id {Id}", notification.Id);
            var notificationDb = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);

            if (notificationDb is null)
            {
                _logger.LogWarning("Notification with Id {Id} not found", notification.Id);
                throw new NotificationRepositoryException($"Notification with Id {notification.Id} not found");
            }

            notificationDb.ParticipationId = notification.ParticipationId;
            notificationDb.Text = notification.Text;
            notificationDb.SentAt = notification.SentAt;

            _context.Notifications.Update(notificationDb);
            var isSave = await _context.SaveChangesAsync() > 0;

            _logger.LogDebug("SaveChanges result: {IsSave}", isSave);
            return notification;
        }
        catch (NotificationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notification with Id {Id}", notification.Id);
            throw new NotificationRepositoryException("Failed to update notification", ex);
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Removing notification with Id {Id}", id);
            var notificationDb = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

            if (notificationDb is null)
            {
                _logger.LogWarning("Notification with Id {Id} not found", id);
                throw new NotificationRepositoryException($"Notification with Id {id} not found");
            }

            _context.Notifications.Remove(notificationDb);
            var isSave = await _context.SaveChangesAsync() > 0;
            _logger.LogDebug("SaveChanges result: {IsSave}", isSave);
        }
        catch (NotificationRepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove notification with Id {Id}", id);
            throw new NotificationRepositoryException("Failed to remove notification", ex);
        }
    }
}
