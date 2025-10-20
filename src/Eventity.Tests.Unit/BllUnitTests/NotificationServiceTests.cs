using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;
using Microsoft.Extensions.Logging;
using Moq;
using Allure.Xunit;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Allure.XUnit.Attributes.Steps;
using Eventity.UnitTests.DalUnitTests.Fabrics;

namespace Eventity.Tests.Services;

public class NotificationServiceTests : IClassFixture<NotificationServiceTestFixture>
{
    private readonly NotificationServiceTestFixture _fixture;

    public NotificationServiceTests(NotificationServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task AddNotification_ShouldThrow_WhenParticipationNotFound()
    {
        var eventId = Guid.NewGuid();
        _fixture.PartRepoMock.Setup(x => x.GetByEventIdAsync(eventId))
                            .ReturnsAsync((IEnumerable<Participation>)null);

        var validation = new Validation(Guid.NewGuid(), false);
        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.AddNotification(eventId, NotificationTypeEnum.Invitation, validation));
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task AddNotification_ShouldThrow_WhenAccessDenied()
    {
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var participation = ParticipationFactory.Create(eventId: eventId);
        var eventInfo = new EventBuilder()
            .WithId(eventId)
            .WithOrganizerId(organizerId)
            .Build();
        
        _fixture.PartRepoMock.Setup(x => x.GetByEventIdAsync(eventId))
                            .ReturnsAsync(new List<Participation> { participation });
        _fixture.EventRepoMock.Setup(x => x.GetByIdAsync(eventId)).ReturnsAsync(eventInfo);

        var validation = new Validation(otherUserId, false);
        
        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.AddNotification(eventId, NotificationTypeEnum.Invitation, validation));
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task GetNotificationById_ShouldReturnNotification_WhenFound()
    {
        var notification = NotificationBuilder.Default();
        _fixture.SetupNotificationExists(notification);

        var result = await _fixture.Service.GetNotificationById(notification.Id);

        Assert.Equal(notification, result);
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task GetNotificationById_ShouldThrow_WhenNotFound()
    {
        var notificationId = Guid.NewGuid();
        _fixture.SetupNotificationNotFound(notificationId);

        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.GetNotificationById(notificationId));
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task GetNotificationByParticipationId_ShouldReturnNotification()
    {
        var participationId = Guid.NewGuid();
        var notification = NotificationBuilder.WithParticipation(participationId);
        
        _fixture.NotificationRepoMock.Setup(r => r.GetByParticipationIdAsync(participationId))
                                    .ReturnsAsync(notification);

        var result = await _fixture.Service.GetNotificationByParticipationId(participationId);

        Assert.Equal(participationId, result.ParticipationId);
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task GetNotificationByParticipationId_ShouldThrow_WhenNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.NotificationRepoMock.Setup(r => r.GetByParticipationIdAsync(participationId))
                                    .ReturnsAsync((Notification)null);

        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.GetNotificationByParticipationId(participationId));
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task GetAllNotifications_ShouldReturnList_WhenFound()
    {
        var notifications = new List<Notification> 
        { 
            NotificationBuilder.Default(),
            NotificationBuilder.WithSpecificText("new")
        };
        _fixture.SetupNotificationsList(notifications);

        var result = await _fixture.Service.GetAllNotifications();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task GetAllNotifications_ShouldThrow_WhenEmpty()
    {
        _fixture.SetupNotificationsList(new List<Notification>());

        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.GetAllNotifications());
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task GetNotifications_ShouldFilterByUserAndParticipation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var participationId = Guid.NewGuid();
        
        var notifications = new List<Notification>
        {
            NotificationBuilder.WithParticipation(participationId),
            NotificationBuilder.WithParticipation(Guid.NewGuid())
        };
        
        var participation = ParticipationFactory.Create(userId: userId, id: participationId);
        
        _fixture.SetupNotificationsList(notifications);
        _fixture.PartRepoMock.Setup(x => x.GetByIdAsync(participationId))
                            .ReturnsAsync(participation);
        _fixture.PartRepoMock.Setup(x => x.GetByIdAsync(It.Is<Guid>(id => id != participationId)))
                            .ReturnsAsync(ParticipationFactory.Create(userId: otherUserId));

        var validation = new Validation(userId, false);
        var result = await _fixture.Service.GetNotifications(participationId, validation);

        Assert.Single(result);
        Assert.Equal(participationId, result.First().ParticipationId);
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task UpdateNotification_ShouldUpdateFields()
    {
        var notification = NotificationBuilder.Default();
        var newText = "Updated text";

        _fixture.SetupNotificationExists(notification);
        _fixture.NotificationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
                                    .ReturnsAsync((Notification n) => n);

        var updated = await _fixture.Service.UpdateNotification(notification.Id, null, newText, null);

        Assert.Equal(newText, updated.Text);
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task UpdateNotification_ShouldThrow_WhenNotFound()
    {
        var notificationId = Guid.NewGuid();
        _fixture.SetupNotificationNotFound(notificationId);

        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.UpdateNotification(notificationId, null, "text", null));
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task RemoveNotification_ShouldCallRepository()
    {
        var notificationId = Guid.NewGuid();
        _fixture.NotificationRepoMock.Setup(r => r.RemoveAsync(notificationId))
                                    .Returns(Task.CompletedTask);

        await _fixture.Service.RemoveNotification(notificationId);

        _fixture.NotificationRepoMock.Verify(r => r.RemoveAsync(notificationId), Times.Once);
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task RemoveNotification_ShouldThrow_OnError()
    {
        var notificationId = Guid.NewGuid();
        _fixture.NotificationRepoMock.Setup(r => r.RemoveAsync(notificationId))
                                    .ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.RemoveNotification(notificationId));
    }
}
