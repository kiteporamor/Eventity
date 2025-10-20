using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> AddNotification(Guid eventId, NotificationTypeEnum type,
        Validation validation);
    Task<Notification> GetNotificationById(Guid id);
    Task<Notification> GetNotificationByParticipationId(Guid participationId);
    Task<IEnumerable<Notification>> GetAllNotifications();
    Task<IEnumerable<Notification>> GetNotifications(Guid? participation_id, Validation validation);
    Task<Notification> UpdateNotification(Guid id, Guid? participationId, string? text, DateTime? sentAt);
    Task RemoveNotification(Guid id);
}