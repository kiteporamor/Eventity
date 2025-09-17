using Eventity.Application.Services;
using Eventity.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Eventity.Tests.Services;

public class EventServiceTestFixture
{
    public Mock<IEventRepository> EventRepoMock { get; }
    public Mock<IParticipationRepository> PartRepoMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }
    public Mock<ILogger<EventService>> LoggerMock { get; }
    public EventService Service { get; }

    public EventServiceTestFixture()
    {
        EventRepoMock = new Mock<IEventRepository>();
        PartRepoMock = new Mock<IParticipationRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        LoggerMock = new Mock<ILogger<EventService>>();
        
        Service = new EventService(
            EventRepoMock.Object,
            PartRepoMock.Object,
            LoggerMock.Object,
            UnitOfWorkMock.Object);
    }

    public void ResetMocks()
    {
        EventRepoMock.Reset();
        PartRepoMock.Reset();
        UnitOfWorkMock.Reset();
        LoggerMock.Reset();
    }

    public void SetupEventExists(Event eventObj)
    {
        EventRepoMock.Setup(x => x.GetByIdAsync(eventObj.Id))
            .ReturnsAsync(eventObj);
    }

    public void SetupEventNotFound(Guid eventId)
    {
        EventRepoMock.Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync((Event)null);
    }

    public void SetupEventsList(List<Event> events)
    {
        EventRepoMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(events);
    }
}