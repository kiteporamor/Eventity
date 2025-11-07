using Eventity.DataAccess.Models.Mongo;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters.Mongo;

public static class UserConverter
{
    public static UserDb ToDb(this User user)
    {
        return new UserDb
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Login = user.Login,
            Password = user.Password,
            Role = user.Role
        };
    }

    public static User ToDomain(this UserDb userDb)
    {
        return new User
        {
            Id = Guid.Parse(userDb.Id),
            Name = userDb.Name,
            Email = userDb.Email,
            Login = userDb.Login,
            Password = userDb.Password,
            Role = userDb.Role
        };
    }
}