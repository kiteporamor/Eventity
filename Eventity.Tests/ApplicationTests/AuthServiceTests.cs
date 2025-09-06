using System;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eventity.Tests.ApplicationTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _logger = new LoggerFactory().CreateLogger<AuthService>();
        _authService = new AuthService(_userRepositoryMock.Object, _jwtServiceMock.Object, _logger);
    }

    [Fact]
    public async Task AuthenticateUser_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var login = "testuser";
        var password = "password";
        var user = new User(Guid.NewGuid(), "Test", "test@email.com", login, password, UserRoleEnum.User);
        var token = "fake-jwt-token";

        _userRepositoryMock.Setup(r => r.GetByLoginAsync(login)).ReturnsAsync(user);
        _jwtServiceMock.Setup(s => s.GenerateToken(user)).Returns(token);

        // Act
        var result = await _authService.AuthenticateUser(login, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user, result.User);
        Assert.Equal(token, result.Token);
    }

    [Fact]
    public async Task AuthenticateUser_UserNotFound_ThrowsUserNotFoundException()
    {
        // Arrange
        var login = "missing";
        _userRepositoryMock.Setup(r => r.GetByLoginAsync(login)).ReturnsAsync((User)null!);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _authService.AuthenticateUser(login, "anyPassword"));
    }

    [Fact]
    public async Task AuthenticateUser_InvalidPassword_ThrowsInvalidPasswordException()
    {
        // Arrange
        var login = "testuser";
        var user = new User(Guid.NewGuid(), "Test", "test@email.com", login, "correct", UserRoleEnum.User);
        _userRepositoryMock.Setup(r => r.GetByLoginAsync(login)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidPasswordException>(() =>
            _authService.AuthenticateUser(login, "wrong"));
    }

    [Fact]
    public async Task RegisterUser_NewUser_ReturnsAuthResult()
    {
        // Arrange
        var login = "newuser";
        var password = "1234";
        var name = "New";
        var email = "new@email.com";
        var role = UserRoleEnum.User;

        _userRepositoryMock.Setup(r => r.GetByLoginAsync(login)).ReturnsAsync((User)null!);
        _jwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

        // Act
        var result = await _authService.RegisterUser(name, email, login, password, role);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.Equal("token", result.Token);
        Assert.Equal(name, result.User.Name);
        Assert.Equal(email, result.User.Email);
    }

    [Fact]
    public async Task RegisterUser_UserAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var login = "existing";
        var user = new User(Guid.NewGuid(), "Existing", "exist@email.com", login, "pwd", UserRoleEnum.User);
        _userRepositoryMock.Setup(r => r.GetByLoginAsync(login)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _authService.RegisterUser("New", "new@email.com", login, "1234", UserRoleEnum.User));
    }
}
