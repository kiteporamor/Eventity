using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Models;
using System;
using Xunit;

namespace Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

public class NotificationConverterTests
{
    [Fact]
    public void ToDb_WhenValidNotification_ReturnsNotificationDb()
    {
        var notification = new NotificationBuilder()
            .WithText("Test notification message")
            .Build();

        var result = notification.ToDb();

        Assert.NotNull(result);
        Assert.Equal(notification.Id, result.Id);
        Assert.Equal(notification.ParticipationId, result.ParticipationId);
        Assert.Equal(notification.Text, result.Text);
        Assert.Equal(notification.SentAt, result.SentAt);
    }

    [Fact]
    public void ToDomain_WhenValidNotificationDb_ReturnsNotification()
    {
        var sentAt = DateTime.UtcNow;
        var notificationDb = new NotificationDb(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test notification message",
            sentAt
        );

        var result = notificationDb.ToDomain();

        Assert.NotNull(result);
        Assert.Equal(notificationDb.Id, result.Id);
        Assert.Equal(notificationDb.ParticipationId, result.ParticipationId);
        Assert.Equal(notificationDb.Text, result.Text);
        Assert.Equal(notificationDb.SentAt, result.SentAt);
    }

    [Fact]
    public void ToDb_WhenNullNotification_ThrowsNullReferenceException()
    {
        Notification notification = null;

        Assert.Throws<NullReferenceException>(() => notification.ToDb());
    }

    [Fact]
    public void ToDomain_WhenNullNotificationDb_ThrowsNullReferenceException()
    {
        NotificationDb notificationDb = null;

        Assert.Throws<NullReferenceException>(() => notificationDb.ToDomain());
    }

    [Fact]
    public void ToDb_WithEmptyText_ConvertsCorrectly()
    {
        var notification = new NotificationBuilder()
            .WithText(string.Empty)
            .Build();

        var result = notification.ToDb();

        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void ToDomain_WithLongText_ConvertsCorrectly()
    {
        var longText = new string('A', 1000);
        var notificationDb = new NotificationDb(
            Guid.NewGuid(),
            Guid.NewGuid(),
            longText,
            DateTime.UtcNow
        );

        var result = notificationDb.ToDomain();

        Assert.Equal(longText, result.Text);
        Assert.Equal(1000, result.Text.Length);
    }
}
