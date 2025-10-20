using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class AuthResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Login { get; set; }
    public UserRoleEnum Role { get; set; }
    public string Token { get; set; }
}