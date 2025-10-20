using Eventity.Domain.Models;
using Eventity.Web.Dtos;

namespace Eventity.Web.Converters;

public class NotificationDtoConverter
{
    public NotificationResponseDto ToResponseDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            ParticipationId = notification.ParticipationId,
            Text = notification.Text,
            SentAt = notification.SentAt,
            Type = notification.Type
        };
    }
}