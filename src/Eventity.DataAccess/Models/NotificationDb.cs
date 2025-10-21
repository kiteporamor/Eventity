using System;
using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Enums;

namespace Eventity.DataAccess.Models;

public class NotificationDb
{
    internal NotificationDb() { }
    
    public NotificationDb(Guid id, Guid participationId, string text, DateTime sentAt, NotificationTypeEnum type)
    {
        Id = id;
        ParticipationId = participationId;
        Text = text;
        SentAt = sentAt;
        Type = type;
    }
    
    public Guid Id { get; set; }
    
    [Required]
    public Guid ParticipationId { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public NotificationTypeEnum Type { get; set; }
    
    public virtual ParticipationDb Participation { get; set; }
}
