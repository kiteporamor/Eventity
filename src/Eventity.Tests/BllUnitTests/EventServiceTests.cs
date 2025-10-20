using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;
using Microsoft.Extensions.Logging;
using Moq;
using Allure.Xunit;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Allure.XUnit.Attributes.Steps;

namespace Eventity.Tests.Services;

public class EventServiceTests : IClassFixture<EventServiceTestFixture>
{
    private readonly EventServiceTestFixture _fixture;

    public EventServiceTests(EventServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    [AllureSuite("EventServiceSuccess")]
    [AllureStep]
    public async Task AddEvent_Should_Add_Event_And_Participation()
    {
        var title = "Event";
        var desc = "Description";
        var date = DateTime.Now.AddDays(1);
        var addr = "Address";
        var organizerId = Guid.NewGuid();

        _fixture.UnitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _fixture.UnitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        var result = await _fixture.Service.AddEvent(title, desc, date, addr, organizerId);

        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(desc, result.Description);
        Assert.Equal(addr, result.Address);
        Assert.Equal(organizerId, result.OrganizerId);
        
        _fixture.EventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>()), Times.Once);
        _fixture.PartRepoMock.Verify(x => x.AddAsync(It.IsAny<Participation>()), Times.Once);
        _fixture.UnitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _fixture.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _fixture.UnitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceSuccess")]
    [AllureStep]
    public async Task GetEventById_Should_Return_Event_If_Exists()
    {
        var expectedEvent = EventBuilder.Default();
        _fixture.SetupEventExists(expectedEvent);

        var result = await _fixture.Service.GetEventById(expectedEvent.Id);

        Assert.Equal(expectedEvent, result);
        _fixture.EventRepoMock.Verify(x => x.GetByIdAsync(expectedEvent.Id), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task GetEventById_Should_Throw_If_Not_Exists()
    {
        var eventId = Guid.NewGuid();
        _fixture.SetupEventNotFound(eventId);

        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.GetEventById(eventId));
        
        _fixture.EventRepoMock.Verify(x => x.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceSuccess")]
    [AllureStep]
    public async Task GetEventByTitle_Should_Return_Events_If_Exists()
    {
        var expectedEvents = new List<Event> { EventBuilder.Default() };
        _fixture.EventRepoMock.Setup(x => x.GetByTitleAsync(It.IsAny<string>()))
                             .ReturnsAsync(expectedEvents);

        var result = await _fixture.Service.GetEventByTitle("Test");

        Assert.Single(result);
        _fixture.EventRepoMock.Verify(x => x.GetByTitleAsync("Test"), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task GetEventByTitle_Should_Throw_If_Not_Exists()
    {
        _fixture.EventRepoMock.Setup(x => x.GetByTitleAsync(It.IsAny<string>()))
                             .ReturnsAsync((IEnumerable<Event>)null);

        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.GetEventByTitle("NonExistent"));
        
        _fixture.EventRepoMock.Verify(x => x.GetByTitleAsync("NonExistent"), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceSuccess")]
    [AllureStep]
    public async Task GetAllEvents_Should_Return_List_If_Any()
    {
        var events = new List<Event> 
        { 
            EventBuilder.Default(),
            new EventBuilder().WithTitle("Second Event").Build()
        };
        _fixture.SetupEventsList(events);

        var result = await _fixture.Service.GetAllEvents();

        Assert.Equal(2, result.Count());
        _fixture.EventRepoMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task GetAllEvents_Should_Throw_If_Empty()
    {
        _fixture.SetupEventsList(new List<Event>());

        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.GetAllEvents());
        
        _fixture.EventRepoMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceSuccess")]
    [AllureStep]
    public async Task UpdateEvent_Should_Update_Existing_Event()
    {
        var originalEvent = EventBuilder.Default();
        _fixture.SetupEventExists(originalEvent);
        
        _fixture.EventRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
                             .ReturnsAsync((Event e) => e);

        var validation = new Validation(originalEvent.OrganizerId, false);
        var result = await _fixture.Service.UpdateEvent(
            originalEvent.Id, 
            "NewTitle", 
            null, 
            null, 
            "NewAddress",
            validation);

        Assert.Equal("NewTitle", result.Title);
        Assert.Equal("NewAddress", result.Address);
        Assert.Equal(originalEvent.Description, result.Description);
        Assert.Equal(originalEvent.DateTime, result.DateTime);
        
        _fixture.EventRepoMock.Verify(x => x.GetByIdAsync(originalEvent.Id), Times.Once);
        _fixture.EventRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task UpdateEvent_Should_Throw_If_Not_Exists()
    {
        var eventId = Guid.NewGuid();
        _fixture.SetupEventNotFound(eventId);

        var validation = new Validation(Guid.NewGuid(), false);
        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.UpdateEvent(eventId, "T", "D", DateTime.Now, "A", validation));
        
        _fixture.EventRepoMock.Verify(x => x.GetByIdAsync(eventId), Times.Once);
        _fixture.EventRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task UpdateEvent_Should_Throw_If_Access_Denied()
    {
        var originalEvent = EventBuilder.Default();
        _fixture.SetupEventExists(originalEvent);

        var validation = new Validation(Guid.NewGuid(), false);
        
        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.UpdateEvent(originalEvent.Id, "NewTitle", null, null, null, validation));
        
        _fixture.EventRepoMock.Verify(x => x.GetByIdAsync(originalEvent.Id), Times.Once);
        _fixture.EventRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    [AllureSuite("EventServiceSuccess")]
    [AllureStep]
    public async Task RemoveEvent_Should_Call_Repository_When_Authorized()
    {
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var existingEvent = new Event(eventId, "Test", "Desc", DateTime.Now.AddDays(1), "Addr", organizerId);
        
        _fixture.SetupEventExists(existingEvent);
        _fixture.EventRepoMock.Setup(x => x.RemoveAsync(eventId))
                             .Returns(Task.CompletedTask);

        var validation = new Validation(organizerId, false); 
        await _fixture.Service.RemoveEvent(eventId, validation);

        _fixture.EventRepoMock.Verify(x => x.RemoveAsync(eventId), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task RemoveEvent_Should_Throw_On_Exception()
    {
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var existingEvent = new Event(eventId, "Test", "Desc", DateTime.Now.AddDays(1), "Addr", organizerId);
        
        _fixture.SetupEventExists(existingEvent);
        _fixture.EventRepoMock.Setup(x => x.RemoveAsync(eventId))
                             .ThrowsAsync(new Exception("DB error"));

        var validation = new Validation(organizerId, false);
        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.RemoveEvent(eventId, validation));
        
        _fixture.EventRepoMock.Verify(x => x.RemoveAsync(eventId), Times.Once);
    }

    [Fact]
    [AllureSuite("EventServiceError")]
    [AllureStep]
    public async Task RemoveEvent_Should_Throw_If_Access_Denied()
    {
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var existingEvent = new Event(eventId, "Test", "Desc", DateTime.Now.AddDays(1), "Addr", organizerId);
        
        _fixture.SetupEventExists(existingEvent);

        var validation = new Validation(Guid.NewGuid(), false);
        
        await Assert.ThrowsAsync<EventServiceException>(() => 
            _fixture.Service.RemoveEvent(eventId, validation));
        
        _fixture.EventRepoMock.Verify(x => x.RemoveAsync(eventId), Times.Never);
    }
}