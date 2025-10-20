using System;
using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class NotificationResponseDto
{
    public Guid Id { get; set; }
    public Guid ParticipationId { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public NotificationTypeEnum Type { get; set; }
}