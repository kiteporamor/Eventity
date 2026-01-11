using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;

namespace Eventity.Tests.Integration.Fakes;

public class CalendarServiceFake : ICalendarService
{
    public Task AddEventToCalendarAsync(User user, Event eventInfo, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendReminderAsync(User user, Event eventInfo, string message, DateTime remindAt,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
