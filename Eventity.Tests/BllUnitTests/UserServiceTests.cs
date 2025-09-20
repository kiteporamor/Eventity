using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Allure.Xunit;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Allure.XUnit.Attributes.Steps;

namespace Eventity.Tests.Services;

public class UserServiceTests : IClassFixture<UserServiceTestFixture>
{
    private readonly UserServiceTestFixture _fixture;

    public UserServiceTests(UserServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    [AllureSuite("UserServiceSuccess")]
    [AllureStep]
    public async Task AddUser_ShouldReturnUser_WhenUserIsCreated()
    {
        var name = "New user";
        var email = "user@mail.ru";
        var login = "login";
        var password = "passworduniq";
        var role = UserRoleEnum.User;

        _fixture.UserRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var result = await _fixture.Service.AddUser(name, email, login, password, role);

        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(email, result.Email);
        Assert.Equal(login, result.Login);
        Assert.Equal(password, result.Password);

        _fixture.UserRepoMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task AddUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var name = "New user";
        var email = "user@mail.ru";
        var login = "login";
        var password = "passworduniq";
        var role = UserRoleEnum.User;

        _fixture.UserRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.AddUser(name, email, login, password, role));
    }
    
    [Fact]
    [AllureSuite("UserServiceSuccess")]
    [AllureStep]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var user = new User(Guid.NewGuid(), "Name", "name@mail.ru", "login111", 
            "password", UserRoleEnum.User);

        _fixture.SetupUserExists(user);

        var result = await _fixture.Service.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);

        _fixture.UserRepoMock.Verify(repo => repo.GetByIdAsync(user.Id), Times.Once);
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task GetUserById_ShouldThrowUserServiceException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _fixture.SetupUserNotFound(userId);

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetUserById(userId));
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task GetUserById_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();

        _fixture.UserRepoMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetUserById(userId));
    }
    
    [Fact]
    [AllureSuite("UserServiceSuccess")]
    [AllureStep]
    public async Task GetAllUsers_ShouldReturnUsers_WhenUsersExist()
    {
        var users = new List<User>
        {
            new User(Guid.NewGuid(), "A A", "a.a@mail.ru", "aaa", 
                "aaa111", UserRoleEnum.User),
            new User(Guid.NewGuid(), "B B", "b.b@mail.ru", "bbb", 
                "bbb111", UserRoleEnum.User)
        };

        _fixture.SetupUsersList(users);

        var result = await _fixture.Service.GetAllUsers();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        _fixture.UserRepoMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task GetAllUsers_ShouldThrowUserServiceException_WhenNoUsersFound()
    {
        _fixture.SetupUsersList(new List<User>());

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetAllUsers());
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task GetAllUsers_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        _fixture.UserRepoMock
            .Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetAllUsers());
    }
    
    [Fact]
    [AllureSuite("UserServiceSuccess")]
    [AllureStep]
    public async Task UpdateUser_ShouldReturnUpdatedUser_WhenUserExists()
    {
        var user = new User(Guid.NewGuid(), "name", "name@mail.ru", "sjdneck", 
            "ejoijf", UserRoleEnum.User);

        _fixture.SetupUserExists(user);
        _fixture.UserRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var result = await _fixture.Service.UpdateUser(user.Id, "name", null, null, null);

        Assert.NotNull(result);
        Assert.Equal("name", result.Name);
        Assert.Equal("name@mail.ru", result.Email);

        _fixture.UserRepoMock.Verify(repo => repo.GetByIdAsync(user.Id), Times.Once);
        _fixture.UserRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task UpdateUser_ShouldThrowUserServiceException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _fixture.SetupUserNotFound(userId);

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.UpdateUser(userId, "BB", null, null, null));
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task UpdateUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var user = new User(Guid.NewGuid(), "A", "a@mail.ru", 
            "iqefiq", "woefjof", UserRoleEnum.User);

        _fixture.SetupUserExists(user);
        _fixture.UserRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.UpdateUser(user.Id, "A", null, null, null));
    }
    
    [Fact]
    [AllureSuite("UserServiceSuccess")]
    [AllureStep]
    public async Task RemoveUser_ShouldCallRemoveAsync_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        _fixture.UserRepoMock
            .Setup(repo => repo.RemoveAsync(userId))
            .Returns(Task.CompletedTask);

        await _fixture.Service.RemoveUser(userId);

        _fixture.UserRepoMock.Verify(repo => repo.RemoveAsync(userId), Times.Once);
    }

    [Fact]
    [AllureSuite("UserServiceError")]
    [AllureStep]
    public async Task RemoveUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();

        _fixture.UserRepoMock
            .Setup(repo => repo.RemoveAsync(userId))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.RemoveUser(userId));
    }
}