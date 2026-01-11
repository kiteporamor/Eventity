using Eventity.Application.Services;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Eventity.Tests.Services;

public class EventServiceTestFixture
{
    public Mock<IEventRepository> EventRepoMock { get; }
    public Mock<IParticipationRepository> PartRepoMock { get; }
    public Mock<IUserRepository> UserRepoMock { get; }
    public Mock<ICalendarService> CalendarServiceMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }
    public Mock<ILogger<EventService>> LoggerMock { get; }
    public EventService Service { get; }

    public EventServiceTestFixture()
    {
        EventRepoMock = new Mock<IEventRepository>();
        PartRepoMock = new Mock<IParticipationRepository>();
        UserRepoMock = new Mock<IUserRepository>();
        CalendarServiceMock = new Mock<ICalendarService>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        LoggerMock = new Mock<ILogger<EventService>>();
        
        Service = new EventService(
            EventRepoMock.Object,
            PartRepoMock.Object,
            UserRepoMock.Object,
            CalendarServiceMock.Object,
            LoggerMock.Object,
            UnitOfWorkMock.Object);
    }

    public void ResetMocks()
    {
        EventRepoMock.Reset();
        PartRepoMock.Reset();
        UserRepoMock.Reset();
        CalendarServiceMock.Reset();
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
