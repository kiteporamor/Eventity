using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Models.Postgres;

public class ParticipationDb
{
    internal ParticipationDb() { }
    
    public ParticipationDb(Guid id, Guid userId, Guid eventId, ParticipationRoleEnum role, ParticipationStatusEnum status)
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
    
    public virtual ICollection<NotificationDb> Notifications { get; set; } = new List<NotificationDb>();
    
    public virtual UserDb User { get; set; }
    
    public virtual EventDb Event { get; set; }
}
