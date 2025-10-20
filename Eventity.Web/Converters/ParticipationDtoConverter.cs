using Eventity.Domain.Models;
using Eventity.Web.Dtos;

namespace Eventity.Web.Converters;

public class ParticipationDtoConverter
{
    public ParticipationResponseDto ToResponseDto(Participation participation)
    {
        return new ParticipationResponseDto
        {
            Id = participation.Id,
            UserId = participation.UserId,
            EventId = participation.EventId,
            Role = participation.Role,
            Status = participation.Status
        };
    }
    
    public UserParticipationInfoResponseDto ToResponseDto(UserParticipationInfo userParticipationInfo)
    {
        return new UserParticipationInfoResponseDto
        {
            EventId = userParticipationInfo.EventItem.Id,
            Title = userParticipationInfo.EventItem.Title,
            Address = userParticipationInfo.EventItem.Address,
            DateTime = userParticipationInfo.EventItem.DateTime,
            Description = userParticipationInfo.EventItem.Description,
            OrganizerId = userParticipationInfo.EventItem.OrganizerId,
            OrganizerLogin = userParticipationInfo.OrganizerLogin
        };
    }
}