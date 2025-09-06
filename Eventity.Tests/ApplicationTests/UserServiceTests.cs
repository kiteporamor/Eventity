using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Eventity.UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserService _userService;
    private readonly ILogger<UserService> _logger;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserService>();
        _logger = logger;
        _userService = new UserService(_mockUserRepository.Object, logger);
    }

    [Fact]
    public async Task AddUser_ShouldReturnUser_WhenUserIsCreated()
    {
        var name = "John Doe";
        var email = "john.doe@example.com";
        var login = "johndoe";
        var password = "password123";
        var userId = Guid.NewGuid();
        var role = UserRoleEnum.User;
        var user = new User(userId, name, email, login, password, role);

        _mockUserRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        var result = await _userService.AddUser(name, email, login, password, role);

        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(email, result.Email);
        Assert.Equal(login, result.Login);
        Assert.Equal(password, result.Password);

        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task AddUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var name = "John Doe";
        var email = "john.doe@example.com";
        var login = "johndoe";
        var password = "password123";
        var role = UserRoleEnum.User;

        _mockUserRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.AddUser(name, email, login, password, role));
    }
    
    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new User(userId, "John Doe", "john.doe@example.com", "johndoe", 
            "password123", UserRoleEnum.User);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _userService.GetUserById(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);

        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserById_ShouldThrowUserServiceException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(default(User?));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.GetUserById(userId));
    }

    [Fact]
    public async Task GetUserById_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.GetUserById(userId));
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

        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsers();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        _mockUserRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_ShouldThrowUserServiceException_WhenNoUsersFound()
    {
        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<User>());

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.GetAllUsers());
    }

    [Fact]
    public async Task GetAllUsers_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.GetAllUsers());
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnUpdatedUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new User(userId, "John Doe", "john.doe@example.com", "johndoe", 
            "password123", UserRoleEnum.User);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(user))
            .ReturnsAsync(user);

        var result = await _userService.UpdateUser(userId, "Jane Doe", null, null, null);

        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("john.doe@example.com", result.Email);

        _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowUserServiceException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(default(User));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.UpdateUser(userId, "Jane Doe", null, null, null));
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();
        var user = new User(userId, "John Doe", "john.doe@example.com", 
            "johndoe", "password123", UserRoleEnum.User);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(user))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.UpdateUser(userId, "Jane Doe", null, null, null));
    }
    
    [Fact]
    public async Task RemoveUser_ShouldCallRemoveAsync_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(repo => repo.RemoveAsync(userId))
            .Returns(Task.CompletedTask);

        await _userService.RemoveUser(userId);

        _mockUserRepository.Verify(repo => repo.RemoveAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RemoveUser_ShouldThrowUserServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(repo => repo.RemoveAsync(userId))
            .ThrowsAsync(new UserRepositoryException("Repository error"));

        await Assert.ThrowsAsync<UserServiceException>(() => 
            _userService.RemoveUser(userId));
    }
}