using Eventity.DataAccess.Models.Postgres;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters.Postgres;

public static class ParticipationConverter
{
    public static ParticipationDb ToDb(this Participation participation) => new(
        participation.Id,
        participation.UserId,
        participation.EventId,
        participation.Role,
        participation.Status
    );

    public static Participation ToDomain(this ParticipationDb participationDb) => new(
        participationDb.Id,
        participationDb.UserId,
        participationDb.EventId,
        participationDb.Role,
        participationDb.Status
    );
}
