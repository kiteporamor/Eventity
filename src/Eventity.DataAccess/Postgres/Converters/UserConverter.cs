using Eventity.DataAccess.Models.Postgres;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters.Postgres;

public static class UserConverter
{
    public static UserDb ToDb(this User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.Login,
        user.Password,
        user.Role
    );

    public static User ToDomain(this UserDb userDb) => new(
        userDb.Id,
        userDb.Name,
        userDb.Email,
        userDb.Login,
        userDb.Password,
        userDb.Role
    );
}
