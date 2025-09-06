using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface INotificationService
{
    Task<Notification> AddNotification(Guid participationId);
    Task<Notification> GetNotificationById(Guid id);
    Task<Notification> GetNotificationByParticipationId(Guid participationId);
    Task<IEnumerable<Notification>> GetAllNotifications();
    Task<Notification> UpdateNotification(Guid id, Guid? participationId, string? text, DateTime? sentAt);
    Task RemoveNotification(Guid id);
}