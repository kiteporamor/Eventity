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
    public async Task AddUser_ShouldReturnUser_WhenUserIsCreated()
    {
        var name = "John Doe";
        var email = "john.doe@example.com";
        var login = "johndoe";
        var password = "password123";
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
    public async Task AddUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var name = "John Doe";
        var email = "john.doe@example.com";
        var login = "johndoe";
        var password = "password123";
        var role = UserRoleEnum.User;

        _fixture.UserRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.AddUser(name, email, login, password, role));
    }
    
    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "johndoe", 
            "password123", UserRoleEnum.User);

        _fixture.SetupUserExists(user);

        var result = await _fixture.Service.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);

        _fixture.UserRepoMock.Verify(repo => repo.GetByIdAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task GetUserById_ShouldThrowUserServiceException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _fixture.SetupUserNotFound(userId);

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetUserById(userId));
    }

    [Fact]
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
    public async Task GetAllUsers_ShouldReturnUsers_WhenUsersExist()
    {
        var users = new List<User>
        {
            new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "johndoe", 
                "password123", UserRoleEnum.User),
            new User(Guid.NewGuid(), "Jane Doe", "jane.doe@example.com", "janedoe", 
                "password456", UserRoleEnum.User)
        };

        _fixture.SetupUsersList(users);

        var result = await _fixture.Service.GetAllUsers();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        _fixture.UserRepoMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_ShouldThrowUserServiceException_WhenNoUsersFound()
    {
        _fixture.SetupUsersList(new List<User>());

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetAllUsers());
    }

    [Fact]
    public async Task GetAllUsers_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        _fixture.UserRepoMock
            .Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.GetAllUsers());
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnUpdatedUser_WhenUserExists()
    {
        var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", "johndoe", 
            "password123", UserRoleEnum.User);

        _fixture.SetupUserExists(user);
        _fixture.UserRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var result = await _fixture.Service.UpdateUser(user.Id, "Jane Doe", null, null, null);

        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("john.doe@example.com", result.Email);

        _fixture.UserRepoMock.Verify(repo => repo.GetByIdAsync(user.Id), Times.Once);
        _fixture.UserRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowUserServiceException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _fixture.SetupUserNotFound(userId);

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.UpdateUser(userId, "Jane Doe", null, null, null));
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var user = new User(Guid.NewGuid(), "John Doe", "john.doe@example.com", 
            "johndoe", "password123", UserRoleEnum.User);

        _fixture.SetupUserExists(user);
        _fixture.UserRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _fixture.Service.UpdateUser(user.Id, "Jane Doe", null, null, null));
    }
    
    [Fact]
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