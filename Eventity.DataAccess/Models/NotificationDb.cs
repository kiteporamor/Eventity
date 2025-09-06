using System;
using System.ComponentModel.DataAnnotations;

namespace Eventity.DataAccess.Models;

public class NotificationDb
{
    internal NotificationDb() { }
    
    public NotificationDb(Guid id, Guid participationId, string text, DateTime sentAt)
    {
        Id = id;
        ParticipationId = participationId;
        Text = text;
        SentAt = sentAt;
    }
    
    public Guid Id { get; set; }
    
    [Required]
    public Guid ParticipationId { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    
    public virtual ParticipationDb Participation { get; set; }
}
