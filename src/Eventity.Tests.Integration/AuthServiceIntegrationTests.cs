using Allure.Xunit.Attributes;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using FluentAssertions;

namespace Eventity.Tests.Integration;

[AllureSuite("Integration Tests")]
[AllureSubSuite("Authentication Service")]
[AllureFeature("User Authentication")]
public class AuthServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    [AllureFeature("User Registration")]
    [AllureStory("Successful User Registration")]
    [AllureTag("Authentication")]
    public async Task RegisterUser_ShouldCreateUserAndReturnToken()
    {
        var authService = GetService<IAuthService>();
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
    [AllureFeature("User Authentication")]
    [AllureStory("Successful Login")]
    [AllureTag("Authentication")]
    public async Task AuthenticateUser_WithValidCredentials_ShouldReturnToken()
    {
        var authService = GetService<IAuthService>();
        await authService.RegisterUser("Test User", "test@email.com", "testuser", "password123", UserRoleEnum.User);

        var result = await authService.AuthenticateUser("testuser", "password123");

        result.Should().NotBeNull();
        result.User.Login.Should().Be("testuser");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [AllureFeature("User Authentication")]
    [AllureStory("Failed Login - Invalid Password")]
    [AllureTag("Authentication")]
    public async Task AuthenticateUser_WithInvalidPassword_ShouldThrowException()
    {
        var authService = GetService<IAuthService>();
        await authService.RegisterUser("Test User", "test@email.com", "testuser", "password123", UserRoleEnum.User);

        await Assert.ThrowsAsync<InvalidPasswordException>(() =>
            authService.AuthenticateUser("testuser", "wrongpassword"));
    }
}