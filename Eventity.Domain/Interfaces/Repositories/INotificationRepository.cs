using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification?> GetByParticipationIdAsync(Guid participationId);
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<Notification> UpdateAsync(Notification notification);
    Task RemoveAsync(Guid id);
}
