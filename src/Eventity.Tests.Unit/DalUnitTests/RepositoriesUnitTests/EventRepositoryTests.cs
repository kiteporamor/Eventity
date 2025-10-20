using System;
using System.Linq;
using System.Threading.Tasks;
using Allure.XUnit.Attributes.Steps;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Tests.Repositories;

public class EventRepositoryTests : IClassFixture<EventRepositoryFixture>
{
    private readonly EventRepositoryFixture _fixture;

    public EventRepositoryTests(EventRepositoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [AllureStep]
    public async Task AddAsync_ShouldSaveEvent()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new EventRepository(context, _fixture.Logger);

        var eventToAdd = new EventBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("Birthday")
            .WithDescription("Mari's Birthday")
            .WithAddress("adress")
            .WithDateTime(DateTime.UtcNow)
            .Build();

        await repository.AddAsync(eventToAdd);
        var retrievedEvent = await repository.GetByIdAsync(eventToAdd.Id);

        Assert.NotNull(retrievedEvent);
        Assert.Equal(eventToAdd.Id, retrievedEvent.Id);
        Assert.Equal(eventToAdd.Description, retrievedEvent.Description);
        Assert.Equal(eventToAdd.Title, retrievedEvent.Title);
    }

    [Fact]
    [AllureStep]
    public async Task UpdateAsync_ShouldModifyExistingEvent()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new EventRepository(context, _fixture.Logger);

        var eventToUpdate = new EventBuilder()
            .WithId(Guid.NewGuid())
            .WithTitle("title")
            .WithDescription("description")
            .WithAddress("adress")
            .WithDateTime(DateTime.UtcNow)
            .Build();

        await repository.AddAsync(eventToUpdate);

        var newDescription = "new descr";
        var newAddress = "new addr";
        var updatedEvent = new EventBuilder()
            .WithId(eventToUpdate.Id)
            .WithTitle(eventToUpdate.Title)
            .WithDescription(newDescription)
            .WithAddress(newAddress)
            .WithDateTime(eventToUpdate.DateTime)
            .Build();

        await repository.UpdateAsync(updatedEvent);
        var result = await repository.GetByIdAsync(eventToUpdate.Id);

        Assert.Equal(newDescription, result.Description);
        Assert.Equal(newAddress, result.Address);
    }

    [Fact]
    [AllureStep]
    public async Task RemoveAsync_ShouldDeleteEvent()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new EventRepository(context, _fixture.Logger);

        var eventToDelete = new EventBuilder()
            .WithId(Guid.NewGuid())
            .Build();

        await repository.AddAsync(eventToDelete);
        await repository.RemoveAsync(eventToDelete.Id);

        var deletedEvent = await repository.GetByIdAsync(eventToDelete.Id);
        Assert.Null(deletedEvent);
    }
}
