using Allure.Xunit.Attributes;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using FluentAssertions;

namespace Eventity.Tests.Integration;

[AllureSuite("Integration Tests")]
[AllureSubSuite("Event Invitation Flow")]
[AllureFeature("Event Participation")]
public class CreateInviteSentIntegrationTests : IntegrationTestBase
{
    [Fact]
    [AllureFeature("Event Invitation")]
    [AllureStory("Complete Invitation Flow")]
    [AllureTag("Invitation")]
    [AllureTag("Notification")]
    public async Task CreateInviteSent_ShouldWorkCorrectly()
    {
        var authService = GetService<IAuthService>();
        var eventService = GetService<IEventService>();
        var participationService = GetService<IParticipationService>();
        var notificationService = GetService<INotificationService>();

        var organizerAuth = await authService.RegisterUser(
            "Event Organizer", "organizer@test.com", "eventorg", "password123", UserRoleEnum.User);
        var organizer = organizerAuth.User;

        var participantAuth = await authService.RegisterUser(
            "Event Participant", "participant@test.com", "eventpart", "password123", UserRoleEnum.User);
        var participant = participantAuth.User;

        var newEvent = await eventService.AddEvent(
            "Conference", "conference", 
            DateTime.UtcNow.AddDays(30), "Moscow", organizer.Id);

        var organizerValidation = new Validation(organizer.Id, false);
        var participation = await participationService.AddParticipation(
            participant.Id, newEvent.Id, ParticipationRoleEnum.Participant, 
            ParticipationStatusEnum.Invited, organizerValidation);

        var participantValidation = new Validation(participant.Id, false);
        var acceptedParticipation = await participationService.UpdateParticipation(
            participation.Id, ParticipationStatusEnum.Accepted, participantValidation);

        var notifications = await notificationService.AddNotification(
            newEvent.Id, NotificationTypeEnum.Reminder, organizerValidation);

        newEvent.Should().NotBeNull();
        newEvent.Title.Should().Be("Conference");

        participation.Should().NotBeNull();
        participation.Status.Should().Be(ParticipationStatusEnum.Invited);

        acceptedParticipation.Should().NotBeNull();
        acceptedParticipation.Status.Should().Be(ParticipationStatusEnum.Accepted);

        notifications.Should().NotBeEmpty();
        
        var notificationForParticipant = notifications
            .FirstOrDefault(n => n.ParticipationId == participation.Id);
        
        notificationForParticipant.Should().NotBeNull();
        notificationForParticipant.Type.Should().Be(NotificationTypeEnum.Reminder);
    }
}