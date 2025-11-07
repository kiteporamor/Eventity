using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}