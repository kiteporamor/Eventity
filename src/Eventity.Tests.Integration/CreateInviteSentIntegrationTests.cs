using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;
using FluentAssertions;

namespace Eventity.Tests.Integration;

public class CreateInviteSentIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateInviteSent_ShouldWorkCorrectly()
    {
        var authService = GetService<AuthService>();
        var eventService = GetService<EventService>();
        var participationService = GetService<ParticipationService>();
        var notificationService = GetService<NotificationService>();

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
        notifications.First().Type.Should().Be(NotificationTypeEnum.Reminder);
        
        var participationForNotification = (await participationService.GetParticipationsByEventId(newEvent.Id))
            .First(p => p.UserId == participant.Id && p.Status == ParticipationStatusEnum.Accepted);
        
        notifications.First().ParticipationId.Should().Be(participationForNotification.Id);
    }
}