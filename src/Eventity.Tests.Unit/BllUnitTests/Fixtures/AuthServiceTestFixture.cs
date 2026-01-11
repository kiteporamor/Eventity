using Eventity.Application.Services;
using Eventity.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Eventity.Tests.Services;

public class AuthServiceTestFixture
{
    public Mock<IUserRepository> UserRepositoryMock { get; }
    public Mock<IJwtService> JwtServiceMock { get; }
    public Mock<ILogger<AuthService>> LoggerMock { get; }
    public AuthService Service { get; }

    public AuthServiceTestFixture()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        JwtServiceMock = new Mock<IJwtService>();
        LoggerMock = new Mock<ILogger<AuthService>>();
        
        Service = new AuthService(
            UserRepositoryMock.Object, 
            JwtServiceMock.Object, 
            LoggerMock.Object);
    }

    public void ResetMocks()
    {
        UserRepositoryMock.Reset();
        JwtServiceMock.Reset();
        LoggerMock.Reset();
    }
}