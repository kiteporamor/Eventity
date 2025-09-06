using Eventity.DataAccess.Models;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters;
    
public static class EventConverter
{
    public static EventDb ToDb(this Event eventDomain) => new(
        eventDomain.Id,
        eventDomain.Title,
        eventDomain.Description,
        eventDomain.DateTime,
        eventDomain.Address,
        eventDomain.OrganizerId
    );

    public static Event ToDomain(this EventDb eventDb) => new()
    {
        Id = eventDb.Id,
        Title = eventDb.Title,
        Description = eventDb.Description,
        DateTime = eventDb.DateTime,
        Address = eventDb.Address,
        OrganizerId = eventDb.OrganizerId
    };
}
