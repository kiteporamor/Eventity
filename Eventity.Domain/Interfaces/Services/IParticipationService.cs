using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface IParticipationService
{
    Task<Participation> AddParticipation(Guid userId, Guid eventId, ParticipationRoleEnum participationRole, 
        ParticipationStatusEnum status, Validation validation);
    Task<Participation> GetParticipationById(Guid id);
    Task<IEnumerable<UserParticipationInfo>> GetUserParticipationsDetailed(
        string? organizer_login, string? event_title, Validation validation, Guid? user_id);
    Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByUserId(Guid userId);
    Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByEventTitle(Guid userId, string title);
    Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByOrganizerLogin(Guid userId, string name);
    Task<Participation> UpdateParticipation(Guid id, ParticipationStatusEnum? status, Validation validation);
    Task RemoveParticipation(Guid id, Validation validation);
    Task<IEnumerable<Participation>> GetAllParticipations();
    Task<IEnumerable<Participation>> GetParticipationsByUserId(Guid userId);
    Task<Participation> ChangeParticipationStatus(Guid id, ParticipationStatusEnum status);
    Task<Participation> ChangeParticipationRole(Guid id, ParticipationRoleEnum participationRole);
    Task<IEnumerable<Participation>> GetParticipationsByEventId(Guid eventId);
    Task<Participation> GetOrganizerByEventId(Guid eventId);
    Task<IEnumerable<Participation>> GetAllParticipantsByEventId(Guid eventId);
    Task<IEnumerable<Participation>> GetAllLeftParticipantsByEventId(Guid eventId);
    Task<Participation> GetParticipationByUserIdAndEventId(Guid userId, Guid eventId);
}
