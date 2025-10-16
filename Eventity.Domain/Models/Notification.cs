using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Eventity.Domain.Enums;

namespace Eventity.Domain.Models;

public class Notification
{    
    public Notification() { }
    
    public Notification(Guid id, Guid participationId, string text, DateTime sentAt, NotificationTypeEnum type)
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
}
