using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using FluentAssertions;

namespace Eventity.Tests.Integration;

public class AuthServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task RegisterUser_ShouldCreateUserAndReturnToken()
    {
        var authService = GetService<AuthService>();
        var userRepository = GetService<IUserRepository>();

        var result = await authService.RegisterUser(
            "Test User", "test@email.com", "testuser", "password123", UserRoleEnum.User);

        result.Should().NotBeNull();
        result.User.Name.Should().Be("Test User");
        result.User.Login.Should().Be("testuser");
        result.Token.Should().NotBeNullOrEmpty();

        var userFromDb = await userRepository.GetByLoginAsync("testuser");
        userFromDb.Should().NotBeNull();
        userFromDb.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task AuthenticateUser_WithValidCredentials_ShouldReturnToken()
    {
        var authService = GetService<AuthService>();
        await authService.RegisterUser("Test User", "test@email.com", "testuser", "password123", UserRoleEnum.User);

        var result = await authService.AuthenticateUser("testuser", "password123");

        result.Should().NotBeNull();
        result.User.Login.Should().Be("testuser");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthenticateUser_WithInvalidPassword_ShouldThrowException()
    {
        var authService = GetService<AuthService>();
        await authService.RegisterUser("Test User", "test@email.com", "testuser", "password123", UserRoleEnum.User);

        await Assert.ThrowsAsync<InvalidPasswordException>(() =>
            authService.AuthenticateUser("testuser", "wrongpassword"));
    }
}