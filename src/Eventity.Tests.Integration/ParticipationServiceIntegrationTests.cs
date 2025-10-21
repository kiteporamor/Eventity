using Allure.Xunit.Attributes;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using FluentAssertions;

namespace Eventity.Tests.Integration;

[AllureSuite("Integration Tests")]
[AllureSubSuite("Participation Service")]
[AllureFeature("Participation Management")]
public class ParticipationServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    [AllureFeature("Participation Creation")]
    [AllureStory("Add Participant to Event")]
    [AllureTag("Participation")]
    [AllureTag("Event")]
    public async Task AddParticipation_ShouldCreateParticipationAndNotification()
    {
        var participationService = GetService<IParticipationService>();
        var eventService = GetService<IEventService>();
        var userService = GetService<IUserService>();
        var notificationRepository = GetService<INotificationRepository>();

        var organizer = await userService.AddUser("Organizer", "org@test.com", "organizer", "pass", UserRoleEnum.User);
        var participant = await userService.AddUser("Participant", "part@test.com", "participant", "pass", UserRoleEnum.User);
        
        var newEvent = await eventService.AddEvent("Test Event", "Desc", DateTime.UtcNow.AddDays(7), "Location", organizer.Id);

        var validation = new Validation(organizer.Id, false);

        var participation = await participationService.AddParticipation(
            participant.Id, newEvent.Id, ParticipationRoleEnum.Participant, 
            ParticipationStatusEnum.Invited, validation);

        participation.Should().NotBeNull();
        participation.UserId.Should().Be(participant.Id);
        participation.EventId.Should().Be(newEvent.Id);
        participation.Status.Should().Be(ParticipationStatusEnum.Invited);

        var notifications = await notificationRepository.GetAllAsync();
        notifications.Should().NotBeEmpty();
        
        var notificationForParticipation = notifications.FirstOrDefault(n => n.ParticipationId == participation.Id);
        notificationForParticipation.Should().NotBeNull();
        notificationForParticipation.Type.Should().Be(NotificationTypeEnum.Invitation);
    }

    [Fact]
    [AllureFeature("Participation Update")]
    [AllureStory("Update Participation Status")]
    [AllureTag("Participation")]
    [AllureTag("Update")]
    public async Task UpdateParticipationStatus_ShouldUpdateStatus()
    {
        var participationService = GetService<IParticipationService>();
        var eventService = GetService<IEventService>();
        var userService = GetService<IUserService>();

        var organizer = await userService.AddUser("Organizer", "org@test.com", "organizer", "pass", UserRoleEnum.User);
        var participant = await userService.AddUser("Participant", "part@test.com", "participant", "pass", UserRoleEnum.User);
        
        var newEvent = await eventService.AddEvent("Test Event", "Desc", DateTime.UtcNow.AddDays(7), "Location", organizer.Id);

        var validation = new Validation(organizer.Id, false);
        var participation = await participationService.AddParticipation(
            participant.Id, newEvent.Id, ParticipationRoleEnum.Participant, 
            ParticipationStatusEnum.Invited, validation);

        var userValidation = new Validation(participant.Id, false);

        var updatedParticipation = await participationService.UpdateParticipation(
            participation.Id, ParticipationStatusEnum.Invited, userValidation);

        updatedParticipation.Should().NotBeNull();
        updatedParticipation.Status.Should().Be(ParticipationStatusEnum.Invited);
    }
}