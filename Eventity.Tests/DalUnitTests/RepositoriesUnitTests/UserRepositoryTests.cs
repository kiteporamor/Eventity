using System;
using System.Threading.Tasks;
using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Xunit;

namespace Eventity.DataAccess.Tests.Repositories;

public class UserRepositoryTests : IClassFixture<UserRepositoryFixture>
{
    private readonly UserRepositoryFixture _fixture;

    public UserRepositoryTests(UserRepositoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new UserRepository(context, _fixture.Logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com",
            Login = "johndoe",
            Password = "securepassword"
        };

        var result = await repository.AddAsync(user);
        var userInDb = await context.Users.FindAsync(user.Id);

        Assert.NotNull(userInDb);
        Assert.Equal(user.Name, userInDb.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new UserRepository(context, _fixture.Logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            Email = "jane@example.com",
            Login = "janedoe",
            Password = "securepassword"
        };

        await context.Users.AddAsync(user.ToDb());
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Name, result.Name);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveUser_WhenUserExists()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new UserRepository(context, _fixture.Logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice",
            Email = "alice@example.com",
            Login = "alice",
            Password = "securepassword"
        };

        await context.Users.AddAsync(user.ToDb());
        await context.SaveChangesAsync();

        await repository.RemoveAsync(user.Id);
        var userInDb = await context.Users.FindAsync(user.Id);

        Assert.Null(userInDb);
    }
}
