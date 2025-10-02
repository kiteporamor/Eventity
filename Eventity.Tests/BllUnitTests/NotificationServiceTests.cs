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
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task AddNotification_ShouldCreateNotification_WhenValidParticipation()
    {
        var participationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
    
        var participation = ParticipationFactory.Create(
            id: participationId,
            userId: userId,
            eventId: eventId,
            status: ParticipationStatusEnum.Invited
        );
    
        var user = UserFactory.CreateUser(id: userId, name: "John Doe", email: "john@example.com");
        var eventInfo = new EventBuilder()
            .WithId(eventId)
            .WithTitle("Test Event")
            .WithAddress("Test Address")
            .WithDateTime(DateTime.UtcNow.AddDays(1))
            .Build();
    
        _fixture.SetupParticipationExists(participation);
        _fixture.SetupUserExists(userId, user);
        _fixture.SetupEventExists(eventId, eventInfo);
        _fixture.SetupNotificationAddedSuccessfully();

        var result = await _fixture.Service.AddNotification(participationId);

        Assert.NotNull(result);
        Assert.Equal(participationId, result.ParticipationId);
        Assert.Contains(user.Name, result.Text);
        Assert.Contains(eventInfo.Title, result.Text);
        Assert.Contains(eventInfo.Address, result.Text);
    
        _fixture.NotificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    [AllureSuite("NotificationServiceSuccess")]
    [AllureStep]
    public async Task AddNotification_ShouldGenerateCorrectInvitationText()
    {
        var participationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        
        var participation = ParticipationFactory.Create(
            id: participationId,
            userId: userId,
            eventId: eventId,
            status: ParticipationStatusEnum.Invited
        );
        
        var user = UserFactory.CreateUser(id: userId, name: "Name", email: "email@mail.ru");
        var eventDateTime = new DateTime(2024, 12, 25, 18, 0, 0);
        var eventInfo = new EventBuilder()
            .WithId(eventId)
            .WithTitle("Event")
            .WithAddress("Adress")
            .WithDateTime(eventDateTime)
            .Build();
        
        _fixture.SetupParticipationExists(participation);
        _fixture.SetupUserExists(userId, user);
        _fixture.SetupEventExists(eventId, eventInfo);
        _fixture.SetupNotificationAddedSuccessfully();

        var result = await _fixture.Service.AddNotification(participationId);

        var expectedText = $"Dear {user.Name}! You are invited to the \"{eventInfo.Title}\" event, " +
                          $"which will be held at \"{eventInfo.Address}\", " +
                          $"at {eventInfo.DateTime:yyyy-MM-dd HH:mm}.\n" +
                          $"Notification sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm}.";
        
        Assert.Equal(expectedText, result.Text);
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task AddNotification_ShouldThrow_WhenParticipationStatusNotInvited()
    {
        var participationId = Guid.NewGuid();

        var confirmedParticipation = ParticipationFactory.AcceptedParticipant();
        confirmedParticipation = new Participation(participationId, confirmedParticipation.UserId,
            confirmedParticipation.EventId, confirmedParticipation.Role, confirmedParticipation.Status);

        _fixture.SetupParticipationExists(confirmedParticipation);

        var exception = await Assert.ThrowsAsync<NotificationServiceException>(() =>
            _fixture.Service.AddNotification(participationId));

        Assert.Equal("Failed to create notification", exception.Message);
    }

    [Fact]
    [AllureSuite("NotificationServiceError")]
    [AllureStep]
    public async Task AddNotification_ShouldThrow_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<NotificationServiceException>(() => 
            _fixture.Service.AddNotification(participationId));
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
}
