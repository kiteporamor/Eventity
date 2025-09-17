using Eventity.Domain.Enums;

namespace Eventity.UnitTests.DalUnitTests.Fabrics;

public static class ParticipationFactory
{
    public static Participation Create(
        Guid? id = null,
        Guid? userId = null,
        Guid? eventId = null,
        ParticipationRoleEnum role = ParticipationRoleEnum.Participant,
        ParticipationStatusEnum status = ParticipationStatusEnum.Invited)
    {
        return new Participation(
            id ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            eventId ?? Guid.NewGuid(),
            role,
            status
        );
    }

    public static Participation Organizer() => Create(
        role: ParticipationRoleEnum.Organizer,
        status: ParticipationStatusEnum.Accepted
    );

    public static Participation RejectedParticipant() => Create(
        role: ParticipationRoleEnum.Participant,
        status: ParticipationStatusEnum.Rejected
    );

    public static Participation AcceptedParticipant() => Create(
        role: ParticipationRoleEnum.Participant,
        status: ParticipationStatusEnum.Accepted
    );

    public static Participation WithSpecificIds(Guid userId, Guid eventId) => Create(
        userId: userId,
        eventId: eventId
    );
}
