using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Repositories;

public interface IEventRepository
{
    Task<Event> AddAsync(Event eventDomain);
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event> UpdateAsync(Event eventDomain);
    Task RemoveAsync(Guid id);
}
