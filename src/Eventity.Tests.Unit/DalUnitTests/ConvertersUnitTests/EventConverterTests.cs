using Eventity.DataAccess.Converters.Postgres;
using Eventity.DataAccess.Models.Postgres;
using System;
using Allure.XUnit.Attributes.Steps;
using Xunit;

namespace Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

public class EventConverterTests
{
    [Fact]
    [AllureStep]
    public void ToDb_WhenValidEvent_ReturnsEventDb()
    {
        var eventDomain = new EventBuilder()
            .WithTitle("Test Event")
            .Build();

        var result = eventDomain.ToDb();

        Assert.NotNull(result);
        Assert.Equal(eventDomain.Id, result.Id);
        Assert.Equal(eventDomain.Title, result.Title);
        Assert.Equal(eventDomain.Description, result.Description);
        Assert.Equal(eventDomain.DateTime, result.DateTime);
        Assert.Equal(eventDomain.Address, result.Address);
        Assert.Equal(eventDomain.OrganizerId, result.OrganizerId);
    }

    [Fact]
    [AllureStep]
    public void ToDomain_WhenValidEventDb_ReturnsEvent()
    {
        var eventDb = new EventDb(
            Guid.NewGuid(),
            "Test Event",
            "Test Description",
            DateTime.Now.AddDays(1),
            "Test Address",
            Guid.NewGuid()
        );

        var result = eventDb.ToDomain();

        Assert.NotNull(result);
        Assert.Equal(eventDb.Id, result.Id);
        Assert.Equal(eventDb.Title, result.Title);
        Assert.Equal(eventDb.Description, result.Description);
        Assert.Equal(eventDb.DateTime, result.DateTime);
        Assert.Equal(eventDb.Address, result.Address);
        Assert.Equal(eventDb.OrganizerId, result.OrganizerId);
    }

    [Fact]
    [AllureStep]
    public void ToDb_WhenNullEvent_ThrowsArgumentNullException()
    {
        Event eventDomain = null;

        Assert.Throws<NullReferenceException>(() => eventDomain.ToDb());
    }
}