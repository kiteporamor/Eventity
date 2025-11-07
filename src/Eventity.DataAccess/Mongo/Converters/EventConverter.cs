using Eventity.DataAccess.Models.Mongo;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters.Mongo;

public static class EventConverter
{
    public static EventDb ToDb(this Event eventDomain)
    {
        return new EventDb
        {
            Id = eventDomain.Id.ToString(),
            Title = eventDomain.Title,
            Description = eventDomain.Description,
            DateTime = eventDomain.DateTime,
            Address = eventDomain.Address,
            OrganizerId = eventDomain.OrganizerId.ToString(),
            Participations = new List<ParticipationDb>()
        };
    }

    public static Event ToDomain(this EventDb eventDb)
    {
        return new Event
        {
            Id = Guid.Parse(eventDb.Id),
            Title = eventDb.Title,
            Description = eventDb.Description,
            DateTime = eventDb.DateTime,
            Address = eventDb.Address,
            OrganizerId = Guid.Parse(eventDb.OrganizerId)
        };
    }
}