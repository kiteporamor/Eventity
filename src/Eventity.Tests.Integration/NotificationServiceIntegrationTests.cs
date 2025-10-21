using Allure.Xunit.Attributes;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using FluentAssertions;

namespace Eventity.Tests.Integration;

[AllureSuite("Integration Tests")]
[AllureSubSuite("Notification Service")]
[AllureFeature("Notification Management")]
public class NotificationServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    [AllureFeature("Notification Creation")]
    [AllureStory("Create Notifications for Event Participants")]
    [AllureTag("Notification")]
    [AllureTag("Event")]
    public async Task AddNotification_ForEvent_ShouldCreateNotificationsForAllParticipants()
    {
        var notificationService = GetService<INotificationService>();
        var eventService = GetService<IEventService>();
        var userService = GetService<IUserService>();
        var participationService = GetService<IParticipationService>();

        var organizer = await userService.AddUser("Organizer", "org@test.com", "organizer", "pass", UserRoleEnum.User);
        var participant1 = await userService.AddUser("Participant1", "part1@test.com", "participant1", "pass", UserRoleEnum.User);
        var participant2 = await userService.AddUser("Participant2", "part2@test.com", "participant2", "pass", UserRoleEnum.User);
        
        var newEvent = await eventService.AddEvent("Test Event", "Desc", DateTime.UtcNow.AddDays(7), "Location", organizer.Id);

        var validation = new Validation(organizer.Id, false);
        
        await participationService.AddParticipation(participant1.Id, newEvent.Id, 
            ParticipationRoleEnum.Participant, ParticipationStatusEnum.Invited, validation);
        await participationService.AddParticipation(participant2.Id, newEvent.Id, 
            ParticipationRoleEnum.Participant, ParticipationStatusEnum.Accepted, validation);

        var notifications = await notificationService.AddNotification(
            newEvent.Id, NotificationTypeEnum.Reminder, validation);

        notifications.Should().NotBeEmpty();
        notifications.Count().Should().BeGreaterThan(0);
        notifications.Should().Contain(n => n.Type == NotificationTypeEnum.Reminder);
    }
}