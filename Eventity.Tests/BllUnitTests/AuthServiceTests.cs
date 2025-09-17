using System;
using System.Linq;
using System.Threading.Tasks;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.Fabrics;
using Microsoft.EntityFrameworkCore;
using Allure.Xunit;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;

namespace Eventity.Tests.Services;

public class AuthServiceTests : IClassFixture<AuthServiceTestFixture>
{
    private readonly AuthServiceTestFixture _fixture;

    public AuthServiceTests(AuthServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    [AllureFeature("Authentication")]
    [AllureStory("User authentication")]
    [AllureSuite("Auth")]
    public async Task AuthenticateUser_ValidCredentials_ReturnsAuthResult()
    {
        var user = UserFactory.CreateUser();
        var token = "fake-jwt-token";

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login))
            .ReturnsAsync(user);
        _fixture.JwtServiceMock.Setup(s => s.GenerateToken(user))
            .Returns(token);

        var result = await _fixture.Service.AuthenticateUser(user.Login, user.Password);

        Assert.NotNull(result);
        Assert.Equal(user, result.User);
        Assert.Equal(token, result.Token);
    }

    [Fact]
    [AllureFeature("Authentication UserNotFound")]
    [AllureStory("Unknown user authentication")]
    [AllureSuite("AuthError")]
    public async Task AuthenticateUser_UserNotFound_ThrowsUserNotFoundException()
    {
        var login = "missing";
        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(login))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _fixture.Service.AuthenticateUser(login, "anyPassword"));
    }

    [Fact]
    public async Task AuthenticateUser_InvalidPassword_ThrowsInvalidPasswordException()
    {
        var user = UserFactory.CreateUser(password: "correct");
        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<InvalidPasswordException>(() =>
            _fixture.Service.AuthenticateUser(user.Login, "wrong"));
    }

    [Fact]
    public async Task RegisterUser_NewUser_ReturnsAuthResult()
    {
        var newUser = UserFactory.RegistratedUser();
        var token = "generated-token";

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(newUser.Login))
            .ReturnsAsync((User)null!);
        _fixture.JwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        var result = await _fixture.Service.RegisterUser(
            newUser.Name, 
            newUser.Email, 
            newUser.Login, 
            newUser.Password, 
            newUser.Role);

        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.Equal(token, result.Token);
        Assert.Equal(newUser.Name, result.User.Name);
        Assert.Equal(newUser.Email, result.User.Email);
        Assert.Equal(newUser.Login, result.User.Login);
        Assert.Equal(newUser.Role, result.User.Role);
    }

    [Fact]
    public async Task RegisterUser_UserAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        var existingUser = UserFactory.CreateUser(login: "existing");
        var newUserDetails = UserFactory.CreateUser(
            name: "New", 
            email: "new@email.com", 
            login: "existing",
            password: "1234"
        );

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(existingUser.Login))
            .ReturnsAsync(existingUser);

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _fixture.Service.RegisterUser(
                newUserDetails.Name,
                newUserDetails.Email,
                newUserDetails.Login,
                newUserDetails.Password,
                newUserDetails.Role));
    }

    [Fact]
    public async Task RegisterUser_AdminRole_CreatesAdminUserSuccessfully()
    {
        var adminUser = UserFactory.AdminUser();
        var token = "admin-token";

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(adminUser.Login))
            .ReturnsAsync((User)null!);
        _fixture.JwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        var result = await _fixture.Service.RegisterUser(
            adminUser.Name,
            adminUser.Email,
            adminUser.Login,
            adminUser.Password,
            adminUser.Role);

        Assert.NotNull(result);
        Assert.Equal(UserRoleEnum.User, result.User.Role);
        Assert.Equal("admin", result.User.Login);
    }

    [Fact]
    public async Task RegisterUser_OrganizerRole_CreatesOrganizerUserSuccessfully()
    {
        var organizerUser = UserFactory.OrganizerUser();
        var token = "organizer-token";

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(organizerUser.Login))
            .ReturnsAsync((User)null!);
        _fixture.JwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        var result = await _fixture.Service.RegisterUser(
            organizerUser.Name,
            organizerUser.Email,
            organizerUser.Login,
            organizerUser.Password,
            organizerUser.Role);

        Assert.NotNull(result);
        Assert.Equal(UserRoleEnum.Admin, result.User.Role);
        Assert.Equal("organizer", result.User.Login);
    }

    [Fact]
    public async Task AuthenticateUser_AdminUser_ReturnsAuthResultWithAdminRole()
    {
        var adminUser = UserFactory.AdminUser();
        var token = "admin-jwt-token";

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(adminUser.Login))
            .ReturnsAsync(adminUser);
        _fixture.JwtServiceMock.Setup(s => s.GenerateToken(adminUser))
            .Returns(token);

        var result = await _fixture.Service.AuthenticateUser(adminUser.Login, adminUser.Password);

        Assert.NotNull(result);
        Assert.Equal(adminUser, result.User);
        Assert.Equal(token, result.Token);
        Assert.Equal(UserRoleEnum.User, result.User.Role);
    }

    [Fact]
    public async Task AuthenticateUser_OrganizerUser_ReturnsAuthResultWithOrganizerRole()
    {
        var organizerUser = UserFactory.OrganizerUser();
        var token = "organizer-jwt-token";

        _fixture.UserRepositoryMock.Setup(r => r.GetByLoginAsync(organizerUser.Login))
            .ReturnsAsync(organizerUser);
        _fixture.JwtServiceMock.Setup(s => s.GenerateToken(organizerUser))
            .Returns(token);

        var result = await _fixture.Service.AuthenticateUser(organizerUser.Login, organizerUser.Password);

        Assert.NotNull(result);
        Assert.Equal(organizerUser, result.User);
        Assert.Equal(token, result.Token);
        Assert.Equal(UserRoleEnum.Admin, result.User.Role);
    }
}
