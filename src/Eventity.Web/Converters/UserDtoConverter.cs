using Eventity.Domain.Models;
using Eventity.Web.Dtos;

namespace Eventity.Web.Converters;

public class UserDtoConverter
{
    public UserResponseDto ToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Login = user.Login,
            Role = user.Role
        };
    }
}