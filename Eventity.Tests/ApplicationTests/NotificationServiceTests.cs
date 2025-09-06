using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<IParticipationRepository> _participationRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IEventRepository> _eventRepoMock = new();
    private readonly Mock<ILogger<NotificationService>> _loggerMock = new();

    private NotificationService CreateService() =>
        new(_notificationRepoMock.Object, _participationRepoMock.Object, _userRepoMock.Object, _eventRepoMock.Object, _loggerMock.Object);
    

    [Fact]
    public async Task AddNotification_ShouldThrow_WhenParticipationNotFound()
    {
        var service = CreateService();
        _participationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).
            ReturnsAsync((Participation)null);

        await Assert.ThrowsAsync<NotificationServiceException>(() => service.AddNotification(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetNotificationById_ShouldReturnNotification_WhenFound()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var notification = new Notification(id, Guid.NewGuid(), "text", DateTime.UtcNow);
        _notificationRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(notification);

        var result = await service.GetNotificationById(id);

        Assert.Equal(notification, result);
    }

    [Fact]
    public async Task GetNotificationById_ShouldThrow_WhenNotFound()
    {
        var service = CreateService();
        _notificationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).
            ReturnsAsync((Notification)null);

        await Assert.ThrowsAsync<NotificationServiceException>(() => service.GetNotificationById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetNotificationByParticipationId_ShouldReturnNotification()
    {
        var service = CreateService();
        var participationId = Guid.NewGuid();
        var notification = new Notification(Guid.NewGuid(), participationId, "text", DateTime.UtcNow);
        _notificationRepoMock.Setup(r => r.GetByParticipationIdAsync(participationId)).
            ReturnsAsync(notification);

        var result = await service.GetNotificationByParticipationId(participationId);

        Assert.Equal(participationId, result.ParticipationId);
    }

    [Fact]
    public async Task GetAllNotifications_ShouldReturnList_WhenFound()
    {
        var service = CreateService();
        var list = new List<Notification> { new(Guid.NewGuid(), Guid.NewGuid(), "Test", 
            DateTime.UtcNow) };
        _notificationRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

        var result = await service.GetAllNotifications();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllNotifications_ShouldThrow_WhenEmpty()
    {
        var service = CreateService();
        _notificationRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Notification>());

        await Assert.ThrowsAsync<NotificationServiceException>(() => service.GetAllNotifications());
    }

    [Fact]
    public async Task UpdateNotification_ShouldUpdateFields()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var old = new Notification(id, Guid.NewGuid(), "OldText", DateTime.UtcNow);
        var newText = "Updated text";

        _notificationRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(old);
        _notificationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Notification>())).
            ReturnsAsync((Notification n) => n);

        var updated = await service.UpdateNotification(id, null, newText, null);

        Assert.Equal(newText, updated.Text);
    }

    [Fact]
    public async Task RemoveNotification_ShouldCallRepository()
    {
        var service = CreateService();
        var id = Guid.NewGuid();

        await service.RemoveNotification(id);

        _notificationRepoMock.Verify(r => r.RemoveAsync(id), Times.Once);
    }
}
