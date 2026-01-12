using Eventity.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Eventity.DataAccess.Models.Mongo;

public class NotificationDb
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ParticipationId { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public NotificationTypeEnum Type { get; set; }
}