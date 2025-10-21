using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class RegisterRequestDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public UserRoleEnum Role { get; set; }
}