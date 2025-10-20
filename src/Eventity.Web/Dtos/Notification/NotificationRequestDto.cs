using System;
using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class NotificationRequestDto
{
    public Guid EventId { get; set; }
    public NotificationTypeEnum Type { get; set; }
}