using Eventity.DataAccess.Models.Mongo;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters.Mongo;

public static class NotificationConverter
{
    public static NotificationDb ToDb(this Notification notificationDomain)
    {
        return new NotificationDb
        {
            Id = notificationDomain.Id.ToString(),
            ParticipationId = notificationDomain.ParticipationId.ToString(),
            SentAt = notificationDomain.SentAt,
            Text = notificationDomain.Text,
            Type = notificationDomain.Type
        };
    }

    public static Notification ToDomain(this NotificationDb notificationDb)
    {
        return new Notification
        {
            Id = Guid.Parse(notificationDb.Id),
            ParticipationId = Guid.Parse(notificationDb.ParticipationId),
            SentAt = notificationDb.SentAt,
            Text = notificationDb.Text,
            Type = notificationDb.Type
        };
    }
}