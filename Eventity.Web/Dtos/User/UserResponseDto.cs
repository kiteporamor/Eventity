using System;
using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Login { get; set; }
    public UserRoleEnum Role { get; set; }
}