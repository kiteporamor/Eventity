using Eventity.DataAccess.Models;
using Eventity.Domain.Models;

namespace Eventity.DataAccess.Converters;

public static class NotificationConverter
{
    public static NotificationDb ToDb(this Notification notification) => new(
        notification.Id,
        notification.ParticipationId,
        notification.Text,
        notification.SentAt
    );

    public static Notification ToDomain(this NotificationDb notificationDb) => new(
        notificationDb.Id,
        notificationDb.ParticipationId,
        notificationDb.Text,
        notificationDb.SentAt
    );
}
