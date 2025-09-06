using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface IParticipationService
{
    Task<Participation> AddParticipation(Guid userId, Guid eventId, ParticipationRoleEnum participationRole, 
        ParticipationStatusEnum status);
    Task<Participation> GetParticipationById(Guid id);
    Task<IEnumerable<Participation>> GetParticipationsByUserId(Guid userId);
    Task<IEnumerable<Participation>> GetParticipationsByEventId(Guid eventId);
    Task<Participation> GetOrganizerByEventId(Guid eventId);
    Task<IEnumerable<Participation>> GetAllParticipantsByEventId(Guid eventId);
    Task<IEnumerable<Participation>> GetAllLeftParticipantsByEventId(Guid eventId);
    Task<Participation> GetParticipationByUserIdAndEventId(Guid userId, Guid eventId);
    Task<IEnumerable<Participation>> GetAllParticipations();
    Task<Participation> UpdateParticipation(Guid id, ParticipationStatusEnum? status);
    Task<Participation> ChangeParticipationStatus(Guid id, ParticipationStatusEnum status);
    Task<Participation> ChangeParticipationRole(Guid id, ParticipationRoleEnum participationRole);
    Task RemoveParticipation(Guid id);
}
