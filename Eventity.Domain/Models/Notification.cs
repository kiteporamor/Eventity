using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace Eventity.Domain.Models;

public class Notification
{    
    public Notification() { }
    
    public Notification(Guid id, Guid participationId, string text, DateTime sentAt)
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
}
