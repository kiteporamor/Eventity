using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Repositories;

public interface IParticipationRepository
{
    Task<Participation> AddAsync(Participation participation);
    Task<Participation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Participation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Participation>> GetByEventIdAsync(Guid eventId);
    Task<IEnumerable<Participation>> GetAllAsync();
    Task<Participation> UpdateAsync(Participation participation);
    Task RemoveAsync(Guid id);
}
