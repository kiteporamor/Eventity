using Microsoft.Extensions.Logging;

namespace Eventity.Tests.Services;

public class UserServiceTestFixture
{
    public Mock<IUserRepository> UserRepoMock { get; }
    public Mock<ILogger<UserService>> LoggerMock { get; }
    public UserService Service { get; }

    public UserServiceTestFixture()
    {
        UserRepoMock = new Mock<IUserRepository>();
        LoggerMock = new Mock<ILogger<UserService>>();
        
        Service = new UserService(
            UserRepoMock.Object,
            LoggerMock.Object);
    }

    public void ResetMocks()
    {
        UserRepoMock.Reset();
        LoggerMock.Reset();
    }

    public void SetupUserExists(User user)
    {
        UserRepoMock.Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
    }

    public void SetupUserNotFound(Guid userId)
    {
        UserRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User)null);
    }

    public void SetupUsersList(List<User> users)
    {
        UserRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);
    }
}