using System;
using System.Threading.Tasks;
using Allure.XUnit.Attributes.Steps;
using Eventity.DataAccess.Converters.Postgres;
using Eventity.DataAccess.Repositories.Postgres;
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
    [AllureStep]
    public async Task AddAsync_ShouldAddUser()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new UserRepository(context, _fixture.Logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "user@mail.ru",
            Login = "loginuser",
            Password = "woiejfwjf"
        };

        var result = await repository.AddAsync(user);
        var userInDb = await context.Users.FindAsync(user.Id);
        
        Assert.True(user.Login.Length <= 30);

        Assert.NotNull(userInDb);
        Assert.Equal(user.Name, userInDb.Name);
    }

    [Fact]
    [AllureStep]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new UserRepository(context, _fixture.Logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "a@mail.ru",
            Login = "wefknwlknf",
            Password = "wkenflwknf"
        };

        await context.Users.AddAsync(user.ToDb());
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Name, result.Name);
    }

    [Fact]
    [AllureStep]
    public async Task RemoveAsync_ShouldRemoveUser_WhenUserExists()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new UserRepository(context, _fixture.Logger);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice",
            Email = "alice@mail.ru",
            Login = "alice",
            Password = "password"
        };

        await context.Users.AddAsync(user.ToDb());
        await context.SaveChangesAsync();

        await repository.RemoveAsync(user.Id);
        var userInDb = await context.Users.FindAsync(user.Id);

        Assert.Null(userInDb);
    }
}
