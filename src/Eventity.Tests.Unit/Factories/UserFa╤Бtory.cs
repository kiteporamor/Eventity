using Eventity.Domain.Enums;

namespace Eventity.UnitTests.DalUnitTests.Fabrics;

public static class UserFactory
{
    public static User CreateUser(
        Guid? id = null,
        string name = "Test User",
        string email = "test@example.com",
        string login = "testuser",
        string password = "Password123!",
        UserRoleEnum role = UserRoleEnum.User)
    {
        return new User(
            id ?? Guid.NewGuid(),
            name ?? throw new ArgumentNullException(nameof(name)),
            email ?? throw new ArgumentNullException(nameof(email)),
            login ?? throw new ArgumentNullException(nameof(login)),
            password ?? throw new ArgumentNullException(nameof(password)),
            role
        );
    }

    public static User AdminUser() => CreateUser(
        name: "Admin User",
        email: "admin@example.com",
        login: "admin",
        password: "AdminPass123!",
        role: UserRoleEnum.User
    );

    public static User OrganizerUser() => CreateUser(
        name: "Organizer User",
        email: "organizer@example.com",
        login: "organizer",
        password: "OrganizerPass123!",
        role: UserRoleEnum.Admin
    );

    public static User RegistratedUser() => CreateUser(
        name: "New User",
        email: "newuser@example.com",
        login: "newuser",
        password: "NewPassword123!"
    );
}