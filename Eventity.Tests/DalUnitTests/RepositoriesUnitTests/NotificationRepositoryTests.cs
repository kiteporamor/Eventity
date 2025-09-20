using System;
using System.Linq;
using System.Threading.Tasks;
using Allure.XUnit.Attributes.Steps;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Eventity.DataAccess.Tests.Repositories;

public class NotificationRepositoryTests : IClassFixture<NotificationRepositoryFixture>
{
    private readonly NotificationRepositoryFixture _fixture;

    public NotificationRepositoryTests(NotificationRepositoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [AllureStep]
    public async Task AddAsync_ShouldSaveNotificationToDatabase()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new NotificationRepository(context, _fixture.Logger);

        var notification = new NotificationBuilder()
            .WithId(Guid.NewGuid())
            .WithParticipationId(Guid.NewGuid())
            .WithText("notification")
            .WithSentAt(DateTime.UtcNow)
            .Build();

        await repository.AddAsync(notification);
        var exists = await context.Notifications.AnyAsync(n => n.Text == "notification");

        Assert.True(exists);
    }

    [Fact]
    [AllureStep]
    public async Task GetByIdAsync_ShouldReturnNotification_WhenExists()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new NotificationRepository(context, _fixture.Logger);

        var notification = new NotificationBuilder()
            .WithId(Guid.NewGuid())
            .WithParticipationId(Guid.NewGuid())
            .WithText("notification")
            .WithSentAt(DateTime.UtcNow)
            .Build();

        await context.Notifications.AddAsync(notification.ToDb());
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(notification.Id);

        Assert.NotNull(result);
        Assert.Equal(notification.Text, result.Text);
    }

    [Fact]
    [AllureStep]
    public async Task RemoveAsync_ShouldDeleteNotificationFromDatabase()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new NotificationRepository(context, _fixture.Logger);

        var notification = new NotificationBuilder()
            .WithId(Guid.NewGuid())
            .WithParticipationId(Guid.NewGuid())
            .WithText("notification")
            .WithSentAt(DateTime.UtcNow)
            .Build();

        await context.Notifications.AddAsync(notification.ToDb());
        await context.SaveChangesAsync();

        await repository.RemoveAsync(notification.Id);
        var exists = await context.Notifications.AnyAsync(n => n.Id == notification.Id);

        Assert.False(exists);
    }
}