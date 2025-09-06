using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;

public class UserRepositoryTests
{
    private readonly EventityDbContext _context;
    private readonly UserRepository _repository;
    private readonly ILogger<UserRepository> _logger;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EventityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EventityDbContext(options);
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserRepository>();
        _logger = logger;
        
        _repository = new UserRepository(_context, logger);
    }
    
    private async Task<EventityDbContext> GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<EventityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new EventityDbContext(options);
        await databaseContext.Database.EnsureCreatedAsync();
        return databaseContext;
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        var context = await GetDatabaseContext();
        var repository = new UserRepository(context, _logger);
        var user = new User { Id = Guid.NewGuid(), 
            Name = "John Doe", Email = "john@example.com", Login = "johndoe", Password = "securepassword" };
        
        var result = await repository.AddAsync(user);
        var userInDb = await context.Users.FindAsync(user.Id);
        
        Assert.NotNull(userInDb);
        Assert.Equal(user.Name, userInDb.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var context = await GetDatabaseContext();
        var repository = new UserRepository(context, _logger);
        var user = new User { Id = Guid.NewGuid(), 
            Name = "Jane Doe", Email = "jane@example.com", Login = "janedoe", Password = "securepassword" };
        await context.Users.AddAsync(user.ToDb());
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Name, result.Name);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveUser_WhenUserExists()
    {
        var context = await GetDatabaseContext();
        var repository = new UserRepository(context, _logger);
        var user = new User { Id = Guid.NewGuid(), 
            Name = "Alice", Email = "alice@example.com", Login = "alice", Password = "securepassword" };
        await context.Users.AddAsync(user.ToDb());
        await context.SaveChangesAsync();

        await repository.RemoveAsync(user.Id);
        var userInDb = await context.Users.FindAsync(user.Id);

        Assert.Null(userInDb);
    }
}
