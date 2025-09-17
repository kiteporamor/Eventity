using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Models;
using Eventity.Domain.Enums;
using Eventity.UnitTests.DalUnitTests.Fabrics;
using System;
using Xunit;

namespace Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

public class UserConverterTests
{
    [Fact]
    public void ToDb_WhenValidUser_ReturnsUserDb()
    {
        var user = UserFactory.CreateUser();

        var result = user.ToDb();

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Login, result.Login);
        Assert.Equal(user.Password, result.Password);
        Assert.Equal(user.Role, result.Role);
    }

    [Fact]
    public void ToDomain_WhenValidUserDb_ReturnsUser()
    {
        var userDb = new UserDb(
            Guid.NewGuid(),
            "A a",
            "A@a.com",
            "a",
            "password123",
            UserRoleEnum.User
        );

        var result = userDb.ToDomain();

        Assert.NotNull(result);
        Assert.Equal(userDb.Id, result.Id);
        Assert.Equal(userDb.Name, result.Name);
        Assert.Equal(userDb.Email, result.Email);
        Assert.Equal(userDb.Login, result.Login);
        Assert.Equal(userDb.Password, result.Password);
        Assert.Equal(userDb.Role, result.Role);
    }

    [Fact]
    public void ToDb_WhenNullUser_ThrowsNullReferenceException()
    {
        User user = null;

        Assert.Throws<NullReferenceException>(() => user.ToDb());
    }
}