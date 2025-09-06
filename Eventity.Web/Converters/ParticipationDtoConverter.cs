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
}