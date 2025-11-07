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

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<NotificationDb> _collection;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(IMongoDatabase database, ILogger<NotificationRepository> logger)
    {
        _collection = database.GetCollection<NotificationDb>("notifications");
        _logger = logger;
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        try
        {
            _logger.LogInformation("Adding new notification with Id {Id}", notification.Id);
            var notificationDb = notification.ToDb();
            await _collection.InsertOneAsync(notificationDb);
            
            _logger.LogDebug("Notification added successfully");
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
            var notificationDb = await _collection
                .Find(n => n.Id == id.ToString())
                .FirstOrDefaultAsync();

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
            var notificationDb = await _collection
                .Find(n => n.ParticipationId == participationId.ToString())
                .FirstOrDefaultAsync();

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
            var notificationsDb = await _collection
                .Find(_ => true)
                .ToListAsync();

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
            var filter = Builders<NotificationDb>.Filter.Eq(n => n.Id, notification.Id.ToString());
            var update = Builders<NotificationDb>.Update
                .Set(n => n.ParticipationId, notification.ParticipationId.ToString())
                .Set(n => n.Text, notification.Text)
                .Set(n => n.SentAt, notification.SentAt);

            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Notification with Id {Id} not found", notification.Id);
                throw new NotificationRepositoryException($"Notification with Id {notification.Id} not found");
            }

            _logger.LogDebug("Notification updated successfully");
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
            var result = await _collection.DeleteOneAsync(n => n.Id == id.ToString());

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("Notification with Id {Id} not found", id);
                throw new NotificationRepositoryException($"Notification with Id {id} not found");
            }
            
            _logger.LogDebug("Notification removed successfully");
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