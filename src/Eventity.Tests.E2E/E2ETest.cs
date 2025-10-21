using System.Net;
using System.Net.Http.Json;
using Allure.Xunit.Attributes;
using DataAccess;
using Eventity.Application.Services;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eventity.Tests.E2E;

public class E2ETests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly ServiceProvider _serviceProvider;
    private readonly EventityDbContext _dbContext;
    private string _organizerToken;
    private string _participantToken;
    private Guid _createdEventId;
    private Guid _participationId;

    public E2ETests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
        
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "PtmYHJnq6UhhjMUw510vZd546amBNgqWSDROkhOgkyQ=",
                ["Jwt:Issuer"] = "Eventity.Web.Test",
                ["Jwt:Audience"] = "http://localhost:5001",
                ["Jwt:ExpireMinutes"] = "120",
                ["ConnectionStrings:DataBaseConnect"] = "User ID=postgres;Password=postgres;Host=test-db;Database=EventityTest;Port=5432"
            })
            .Build();
            
        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<EventityDbContext>(options =>
            options.UseNpgsql("User ID=postgres;Password=postgres;Host=test-db;Database=EventityTest;Port=5432"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<EventityDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _dbContext.Database.EnsureDeletedAsync();
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    [AllureFeature("Event Management")]
    [AllureStory("Complete Event Scenario")]
    [AllureTag("E2E")]
    public async Task CompleteEventScenario_ShouldWorkEndToEnd()
    {
        var authService = _serviceProvider.GetRequiredService<IAuthService>();
        var eventService = _serviceProvider.GetRequiredService<IEventService>();
        var participationService = _serviceProvider.GetRequiredService<IParticipationService>();
        var notificationService = _serviceProvider.GetRequiredService<INotificationService>();

        var organizerAuth = await authService.RegisterUser(
            "Иван", "organizer@eventity.com", "eventorg", "password123", UserRoleEnum.User);
        var participantAuth = await authService.RegisterUser(
            "Петр", "participant@eventity.com", "eventpart", "password123", UserRoleEnum.User);

        organizerAuth.Should().NotBeNull();
        participantAuth.Should().NotBeNull();
        organizerAuth.Token.Should().NotBeNullOrEmpty();
        participantAuth.Token.Should().NotBeNullOrEmpty();

        _organizerToken = organizerAuth.Token;
        _participantToken = participantAuth.Token;

        var newEvent = await eventService.AddEvent(
            "День рождения", 
            "День рождения",
            DateTime.UtcNow.AddDays(30), 
            "Москва",
            organizerAuth.User.Id);

        newEvent.Should().NotBeNull();
        newEvent.Title.Should().Be("День рождения");
        newEvent.OrganizerId.Should().Be(organizerAuth.User.Id);
        _createdEventId = newEvent.Id;

        var organizerValidation = new Validation(organizerAuth.User.Id, false);
        var participation = await participationService.AddParticipation(
            participantAuth.User.Id, 
            newEvent.Id, 
            ParticipationRoleEnum.Participant, 
            ParticipationStatusEnum.Invited, 
            organizerValidation);

        participation.Should().NotBeNull();
        participation.Status.Should().Be(ParticipationStatusEnum.Invited);
        _participationId = participation.Id;

        var invitationNotifications = await notificationService.AddNotification(
            newEvent.Id, 
            NotificationTypeEnum.Invitation, 
            organizerValidation);

        invitationNotifications.Should().NotBeEmpty();
        invitationNotifications.Should().Contain(n => n.Type == NotificationTypeEnum.Invitation);

        var participantValidation = new Validation(participantAuth.User.Id, false);
        var acceptedParticipation = await participationService.UpdateParticipation(
            participation.Id, 
            ParticipationStatusEnum.Accepted, 
            participantValidation);

        acceptedParticipation.Should().NotBeNull();
        acceptedParticipation.Status.Should().Be(ParticipationStatusEnum.Accepted);

        var reminderNotifications = await notificationService.AddNotification(
            newEvent.Id, 
            NotificationTypeEnum.Reminder, 
            organizerValidation);

        reminderNotifications.Should().NotBeEmpty();
        reminderNotifications.Should().Contain(n => n.Type == NotificationTypeEnum.Reminder);

        var eventDetails = await eventService.GetEventById(newEvent.Id);

        eventDetails.Should().NotBeNull();
        eventDetails.Title.Should().Be("День рождения");
        eventDetails.Address.Should().Be("Москва");

        var eventParticipants = await participationService.GetParticipationsByEventId(newEvent.Id);

        eventParticipants.Should().NotBeEmpty();
        eventParticipants.Should().Contain(p => p.UserId == participantAuth.User.Id);
        eventParticipants.Should().Contain(p => p.UserId == organizerAuth.User.Id);

        var userParticipations = await participationService.GetUserParticipationInfoByUserId(participantAuth.User.Id);

        userParticipations.Should().NotBeEmpty();
        userParticipations.Should().Contain(up => up.EventItem.Title == "День рождения");

        organizerAuth.User.Name.Should().Be("Иван");
        participantAuth.User.Name.Should().Be("Петр");
        eventDetails.DateTime.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    [AllureFeature("Event Management")]
    [AllureStory("Multiple Participants Scenario")]
    [AllureTag("E2E")]
    public async Task EventCreationAndManagement_ShouldHandleMultipleParticipants()
    {
        var authService = _serviceProvider.GetRequiredService<IAuthService>();
        var eventService = _serviceProvider.GetRequiredService<IEventService>();
        var participationService = _serviceProvider.GetRequiredService<IParticipationService>();

        var organizer = await authService.RegisterUser(
            "Алексей", "a@eventity.com", "a", "password123", UserRoleEnum.User);

        var techEvent = await eventService.AddEvent(
            "День рождения", 
            "ДР",
            DateTime.UtcNow.AddDays(45), 
            "Санкт-Петербург",
            organizer.User.Id);

        var participants = new List<AuthResult>();
        for (int i = 1; i <= 3; i++)
        {
            var participant = await authService.RegisterUser(
                $"Участник {i}", $"participant{i}@eventity.com", $"part{i}", "password123", UserRoleEnum.User);
            participants.Add(participant);
        }

        var organizerValidation = new Validation(organizer.User.Id, false);
        foreach (var participant in participants)
        {
            var part = await participationService.AddParticipation(
                participant.User.Id, 
                techEvent.Id, 
                ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Invited, 
                organizerValidation);

            part.Should().NotBeNull();
        }

        var allParticipants = await participationService.GetParticipationsByEventId(techEvent.Id);
        allParticipants.Count(p => p.Role == ParticipationRoleEnum.Participant).Should().Be(3);
    }
}