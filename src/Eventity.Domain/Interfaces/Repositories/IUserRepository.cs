using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByLoginAsync(string login);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> UpdateAsync(User user);
    Task RemoveAsync(Guid id);
}