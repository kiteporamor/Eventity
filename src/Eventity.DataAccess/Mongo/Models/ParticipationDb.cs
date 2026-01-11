using Eventity.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Eventity.DataAccess.Models.Mongo;

public class ParticipationDb
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; }
    public string EventId { get; set; }
    public ParticipationRoleEnum Role { get; set; }
    public ParticipationStatusEnum Status { get; set; }
    public List<NotificationDb> Notifications { get; set; } = new();
}