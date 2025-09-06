using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

// TODO: исправить во всех схемах organizerName на organizerId
public interface IEventService
{
    Task<Event> AddEvent(string title, string description, DateTime dateTime, string address, Guid organizerId);
    Task<Event> GetEventById(Guid id);
    Task<IEnumerable<Event>> GetAllEvents();
    Task<Event> UpdateEvent(Guid id, string? title, string? description, DateTime? dateTime, string? address);
    Task RemoveEvent(Guid id);
}
