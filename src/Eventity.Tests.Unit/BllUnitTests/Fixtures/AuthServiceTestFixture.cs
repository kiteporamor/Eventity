using Eventity.Application.Services;
using Eventity.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Eventity.Tests.Services;

public class AuthServiceTestFixture
{
    public Mock<IUserRepository> UserRepositoryMock { get; }
    public Mock<IJwtService> JwtServiceMock { get; }
    public Mock<ITwoFactorCodeService> TwoFactorCodeServiceMock { get; }
    public Mock<IEmailService> EmailServiceMock { get; }
    public Mock<ILogger<AuthService>> LoggerMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public AuthService Service { get; }

    public AuthServiceTestFixture()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        JwtServiceMock = new Mock<IJwtService>();
        TwoFactorCodeServiceMock = new Mock<ITwoFactorCodeService>();
        EmailServiceMock = new Mock<IEmailService>();
        LoggerMock = new Mock<ILogger<AuthService>>();
        ConfigurationMock = new Mock<IConfiguration>();

        Service = new AuthService(
            UserRepositoryMock.Object,
            JwtServiceMock.Object,
            TwoFactorCodeServiceMock.Object,
            EmailServiceMock.Object,
            LoggerMock.Object,
            ConfigurationMock.Object);
    }

    public void ResetMocks()
    {
        UserRepositoryMock.Reset();
        JwtServiceMock.Reset();
        TwoFactorCodeServiceMock.Reset();
        EmailServiceMock.Reset();
        LoggerMock.Reset();
        ConfigurationMock.Reset();
    }
}