using Eventity.DataAccess.Models.Mongo;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters.Mongo;

public static class ParticipationConverter
{
    public static ParticipationDb ToDb(this Participation participationDomain)
    {
        return new ParticipationDb
        {
            Id = participationDomain.Id.ToString(),
            EventId = participationDomain.EventId.ToString(),
            UserId = participationDomain.UserId.ToString(),
            Role = participationDomain.Role,
            Status = participationDomain.Status,
            Notifications = new List<NotificationDb>()
        };
    }

    public static Participation ToDomain(this ParticipationDb participationDb)
    {
        return new Participation
        {
            Id = Guid.Parse(participationDb.Id),
            EventId = Guid.Parse(participationDb.EventId),
            UserId = Guid.Parse(participationDb.UserId),
            Role = participationDb.Role,
            Status = participationDb.Status
        };
    }
}