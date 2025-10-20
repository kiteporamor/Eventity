using System;
using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class ParticipationResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public ParticipationRoleEnum Role { get; set; }
    public ParticipationStatusEnum Status { get; set; }
}