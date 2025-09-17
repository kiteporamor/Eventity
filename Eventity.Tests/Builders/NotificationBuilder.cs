namespace Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

public class NotificationBuilder
{
    private Notification _notification;

    public NotificationBuilder()
    {
        _notification = new Notification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Default notification text",
            DateTime.UtcNow
        );
    }

    public NotificationBuilder With(Action<Notification> configure)
    {
        configure(_notification);
        return this;
    }

    public NotificationBuilder WithId(Guid id) => With(n => n.Id = id);
    public NotificationBuilder WithParticipationId(Guid participationId) => With(n => n.ParticipationId = participationId);
    public NotificationBuilder WithText(string text) => With(n => n.Text = text);
    public NotificationBuilder WithSentAt(DateTime sentAt) => With(n => n.SentAt = sentAt);

    public Notification Build() => _notification;

    public static Notification Default() => new NotificationBuilder().Build();
    public static Notification WithSpecificText(string text) => new NotificationBuilder().WithText(text).Build();
    public static Notification WithParticipation(Guid participationId) => new NotificationBuilder().WithParticipationId(participationId).Build();
}
