using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Enums;

namespace Eventity.Domain.Models;

public class Participation
{
    public Participation() { }
    
    public Participation(Guid id, Guid userId, Guid eventId, ParticipationRoleEnum role, ParticipationStatusEnum status)
    {
        Id = id;
        UserId = userId;
        EventId = eventId;
        Role = role;
        Status = status;
    }
    
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid EventId { get; set; }
    
    [EnumDataType(typeof(ParticipationRoleEnum))]
    public ParticipationRoleEnum Role { get; set; }
    
    [EnumDataType(typeof(ParticipationStatusEnum))]
    public ParticipationStatusEnum Status { get; set; }
}
