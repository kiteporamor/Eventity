using Eventity.Domain.Models;
using Eventity.Web.Dtos;

namespace Eventity.Web.Converters;

public class AuthDtoConverter
{
    public AuthResponseDto ToResponseDto(AuthResult authResult)
    {
        return new AuthResponseDto
        {
            Id = authResult.User.Id,
            Email = authResult.User.Email,
            Name = authResult.User.Name,
            Login = authResult.User.Login,
            Role = authResult.User.Role,
            Token = authResult.Token
        };
    }
}