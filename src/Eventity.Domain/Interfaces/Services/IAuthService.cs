using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> AuthenticateUser(string login, string password);
    Task<AuthResult> RegisterUser(string name, string email, string login, string password, UserRoleEnum role);
}