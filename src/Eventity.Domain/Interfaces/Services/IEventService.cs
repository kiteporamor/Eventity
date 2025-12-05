using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface IEventService
{
    Task<Event> AddEvent(string title, string description, DateTime dateTime, string address, Guid organizerId);
    Task<Event> GetEventById(Guid id);
    Task<IEnumerable<Event>> GetEventByTitle(string title);
    Task<IEnumerable<Event>> GetAllEvents();
    Task<Event> UpdateEvent(Guid id, string? title, string? description, DateTime? dateTime, string? address,
        Validation validation);
    Task RemoveEvent(Guid id, Validation validation);
}