using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface IUserService
{
    Task<User> AddUser(string name, string email, string login, string password, UserRoleEnum role);
    Task<User> GetUserById(Guid id);
    Task<User> GetUserByLogin(string login);
    Task<IEnumerable<User>> GetAllUsers();
    Task<IEnumerable<User>> GetUsers(string? login);
    Task<User> UpdateUser(Guid id, string? name, string? email, string? login, string? password);
    Task RemoveUser(Guid id);
}
