using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface ICalendarService
{
    Task AddEventToCalendarAsync(User user, Event eventInfo, CancellationToken cancellationToken = default);
    Task SendReminderAsync(User user, Event eventInfo, string message, DateTime remindAt,
        CancellationToken cancellationToken = default);
}
