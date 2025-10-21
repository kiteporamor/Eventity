using Allure.Xunit.Attributes;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using FluentAssertions;

namespace Eventity.Tests.Integration;

[AllureSuite("Integration Tests")]
[AllureSubSuite("Event Service")]
[AllureFeature("Event Management")]
public class EventServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    [AllureFeature("Event Creation")]
    [AllureStory("Create Event with Organizer")]
    [AllureTag("Event")]
    [AllureTag("Organizer")]
    public async Task CreateEvent_ShouldCreateEventAndOrganizerParticipation()
    {
        var eventService = GetService<IEventService>();
        var userService = GetService<IUserService>();
        var participationRepository = GetService<IParticipationRepository>();

        var organizer = await userService.AddUser("Organizer", "org@test.com", "organizer", "pass", UserRoleEnum.User);
        var organizerId = organizer.Id;

        var newEvent = await eventService.AddEvent(
            "Test Event", "Test Description", DateTime.UtcNow.AddDays(7), 
            "Test Location", organizerId);

        newEvent.Should().NotBeNull();
        newEvent.Title.Should().Be("Test Event");
        newEvent.OrganizerId.Should().Be(organizerId);

        var organizerParticipation = (await participationRepository.GetByEventIdAsync(newEvent.Id))
            .FirstOrDefault(p => p.UserId == organizerId);
        
        organizerParticipation.Should().NotBeNull();
        organizerParticipation.Role.Should().Be(ParticipationRoleEnum.Organizer);
        organizerParticipation.Status.Should().Be(ParticipationStatusEnum.Accepted);
    }

    [Fact]
    [AllureFeature("Event Retrieval")]
    [AllureStory("Get Event by ID")]
    [AllureTag("Event")]
    [AllureTag("Retrieval")]
    public async Task GetEventById_ShouldReturnEvent()
    {
        var eventService = GetService<IEventService>();
        var userService = GetService<IUserService>();
        
        var organizer = await userService.AddUser("Organizer", "org@test.com", "organizer", "pass", UserRoleEnum.User);
        var createdEvent = await eventService.AddEvent("Test Event", "Desc", DateTime.UtcNow.AddDays(1), "Location", organizer.Id);

        var retrievedEvent = await eventService.GetEventById(createdEvent.Id);

        retrievedEvent.Should().NotBeNull();
        retrievedEvent.Id.Should().Be(createdEvent.Id);
        retrievedEvent.Title.Should().Be("Test Event");
    }
}