using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using Allure.Xunit.Attributes;
using Eventity.Domain.Enums;
using Eventity.Web.Dtos;
using FluentAssertions;
using Xunit;

namespace Eventity.Tests.E2E;

[AllureSuite("E2E Tests")]
[AllureSubSuite("E2E-tests")]
[AllureFeature("Event Management")]
public class E2ETests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly HttpClient _calendarClient;
    private readonly string _calendarMode;
    private string _organizerToken;
    private string _participantToken;
    private Guid _createdEventId;
    private Guid _participationId;

    public E2ETests()
    {
        var baseUrl = Environment.GetEnvironmentVariable("EVENTITY_API_URL") ?? "http://eventity-app:5001";
        var calendarUrl = Environment.GetEnvironmentVariable("CALENDAR_API_URL") ?? "http://calendar-mock:8080";
        _calendarMode = Environment.GetEnvironmentVariable("CALENDAR_MODE") ?? "Mock";
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _calendarClient = new HttpClient
        {
            BaseAddress = new Uri(calendarUrl)
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client?.Dispose();
        _calendarClient?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    [AllureFeature("End to End")]
    [AllureStory("Scenario")]
    public async Task CompleteEventScenario_ShouldWorkEndToEnd()
    {
        var timestamp = DateTime.Now.Ticks;

        var organizerRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Иван",
            email = $"organizer{timestamp}@eventity.com",
            login = $"eventorg{timestamp}",
            password = "password123",
            role = UserRoleEnum.Admin
        });

        organizerRegisterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var organizerAuth = await organizerRegisterResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        organizerAuth.Should().NotBeNull();
        organizerAuth!.Token.Should().NotBeNullOrEmpty();
        _organizerToken = organizerAuth.Token;

        var participantRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Петр",
            email = $"participant{timestamp}@eventity.com",
            login = $"eventpart{timestamp}",
            password = "password123",
            role = UserRoleEnum.Admin
        });

        participantRegisterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var participantAuth = await participantRegisterResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        participantAuth.Should().NotBeNull();
        participantAuth!.Token.Should().NotBeNullOrEmpty();
        _participantToken = participantAuth.Token;

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_organizerToken}");
        
        var createEventResponse = await _client.PostAsJsonAsync("/api/events", new
        {
            title = $"День рождения {timestamp}",
            description = "День рождения",
            dateTime = DateTime.UtcNow.AddDays(30),
            address = "Москва"
        });

        await AssertCreatedOrDumpAsync(createEventResponse);
        var newEvent = await createEventResponse.Content.ReadFromJsonAsync<EventResponseDto>();
        newEvent.Should().NotBeNull();
        newEvent!.Title.Should().Be($"День рождения {timestamp}");
        _createdEventId = newEvent.Id;

        var createParticipationResponse = await _client.PostAsJsonAsync("/api/participations", new
        {
            userId = participantAuth.Id,
            eventId = newEvent.Id,
            role = ParticipationRoleEnum.Participant,
            status = ParticipationStatusEnum.Invited
        });

        createParticipationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var participation = await createParticipationResponse.Content.ReadFromJsonAsync<ParticipationResponseDto>();
        participation.Should().NotBeNull();
        participation!.Status.Should().Be(ParticipationStatusEnum.Invited);
        _participationId = participation.Id;

        var createNotificationResponse = await _client.PostAsJsonAsync("/api/notifications", new
        {
            eventId = newEvent.Id,
            type = NotificationTypeEnum.Invitation
        });

        createNotificationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_participantToken}");

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_organizerToken}");
        
        var createReminderResponse = await _client.PostAsJsonAsync("/api/notifications", new
        {
            eventId = newEvent.Id,
            type = NotificationTypeEnum.Reminder
        });

        createReminderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getEventResponse = await _client.GetAsync($"/api/events/{newEvent.Id}");
        getEventResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var eventDetails = await getEventResponse.Content.ReadFromJsonAsync<EventResponseDto>();
        eventDetails.Should().NotBeNull();
        eventDetails!.Title.Should().Be($"День рождения {timestamp}");

        var getParticipantsResponse = await _client.GetAsync($"/api/participations?event_title=День рождения {timestamp}");
        getParticipantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var eventParticipants = await getParticipantsResponse.Content.ReadFromJsonAsync<List<UserParticipationInfoResponseDto>>();
        eventParticipants.Should().NotBeEmpty();

        organizerAuth.Name.Should().Be("Иван");
        participantAuth.Name.Should().Be("Петр");
        eventDetails.DateTime.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    [AllureFeature("End to End Multiple participants")]
    [AllureStory("Multiple participants")]
    public async Task EventCreationAndManagement_ShouldHandleMultipleParticipants()
    {
        var timestamp = DateTime.Now.Ticks;

        var organizerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Алексей",
            email = $"a{timestamp}@eventity.com",
            login = $"a{timestamp}",
            password = "password123",
            role = UserRoleEnum.User
        });

        organizerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var organizer = await organizerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {organizer!.Token}");
        
        var createEventResponse = await _client.PostAsJsonAsync("/api/events", new
        {
            title = $"Технический митап {timestamp}",
            description = "Обсуждение новых технологий",
            dateTime = DateTime.UtcNow.AddDays(45),
            address = "Санкт-Петербург"
        });

        await AssertCreatedOrDumpAsync(createEventResponse);
        var techEvent = await createEventResponse.Content.ReadFromJsonAsync<EventResponseDto>();

        var participants = new List<AuthResponseDto>();
        for (int i = 1; i <= 3; i++)
        {
            var participantResponse = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                name = $"Участник {i}",
                email = $"participant{timestamp}_{i}@eventity.com",
                login = $"part{timestamp}_{i}",
                password = "password123",
                role = UserRoleEnum.User
            });

            participantResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var participant = await participantResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            participants.Add(participant!);

            var participationResponse = await _client.PostAsJsonAsync("/api/participations", new
            {
                userId = participant!.Id,
                eventId = techEvent!.Id,
                role = ParticipationRoleEnum.Participant,
                status = ParticipationStatusEnum.Invited
            });

            participationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var getParticipantsResponse = await _client.GetAsync($"/api/participations?event_title=Технический митап {timestamp}");
        getParticipantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var allParticipants = await getParticipantsResponse.Content.ReadFromJsonAsync<List<UserParticipationInfoResponseDto>>();
        
        allParticipants!.Count.Should().Be(1);
    }

    [Fact]
    [AllureFeature("End to End Registration")]
    [AllureStory("Registration")]
    public async Task UserRegistrationAndLogin_ShouldWorkCorrectly()
    {
        var timestamp = DateTime.Now.Ticks;

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Тестовый пользователь",
            email = $"test{timestamp}@eventity.com",
            login = $"testuser{timestamp}",
            password = "password123",
            role = UserRoleEnum.User
        });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            login = $"testuser{timestamp}",
            password = "password123"
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        loginResult.Should().NotBeNull();
        loginResult!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [AllureFeature("Calendar Integration")]
    [AllureStory("Event creation adds calendar entries and reminders")]
    public async Task EventCreation_ShouldSyncCalendarAndReminders()
    {
        var timestamp = DateTime.Now.Ticks;

        var organizerRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Calendar User",
            email = $"calendar{timestamp}@eventity.com",
            login = $"calendar{timestamp}",
            password = "password123",
            role = UserRoleEnum.User
        });

        organizerRegisterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var organizerAuth = await organizerRegisterResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        organizerAuth.Should().NotBeNull();
        organizerAuth!.Token.Should().NotBeNullOrEmpty();

        _organizerToken = organizerAuth.Token;
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_organizerToken}");

        var createEventResponse = await _client.PostAsJsonAsync("/api/events", new
        {
            title = $"Calendar Event {timestamp}",
            description = "Calendar event description",
            dateTime = DateTime.UtcNow.AddDays(10),
            address = "Calendar Address"
        });

        await AssertCreatedOrDumpAsync(createEventResponse);
        var newEvent = await createEventResponse.Content.ReadFromJsonAsync<EventResponseDto>();
        newEvent.Should().NotBeNull();

        var reminderResponse = await _client.PostAsJsonAsync("/api/notifications", new
        {
            eventId = newEvent!.Id,
            type = NotificationTypeEnum.Reminder
        });

        reminderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        if (_calendarMode.Equals("Mock", StringComparison.OrdinalIgnoreCase))
        {
            var eventsCount = await GetWireMockRequestCountAsync("/api/calendar/events");
            eventsCount.Should().BeGreaterThan(0);

            var remindersCount = await GetWireMockRequestCountAsync("/api/calendar/reminders");
            remindersCount.Should().BeGreaterThan(0);
        }
        else
        {
            var accessToken = Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_ACCESS_TOKEN");
            var calendarId = Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_ID") ?? "primary";
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return;
            }

            _calendarClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var query =
                $"?privateExtendedProperty=eventityEventId={newEvent.Id}&fields=items(id,reminders)";
            var encodedCalendarId = Uri.EscapeDataString(calendarId);
            var eventsResponse = await _calendarClient
                .GetFromJsonAsync<GoogleEventsListDto>($"/calendars/{encodedCalendarId}/events{query}");

            eventsResponse.Should().NotBeNull();
            var googleEvent = eventsResponse!.Items?.FirstOrDefault();
            googleEvent.Should().NotBeNull();
            googleEvent!.Id.Should().NotBeNullOrEmpty();
            googleEvent.Reminders.Should().NotBeNull();
        }
    }

    private async Task<int> GetWireMockRequestCountAsync(string path)
    {
        var response = await _calendarClient.GetAsync("/__admin/requests");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        if (!doc.RootElement.TryGetProperty("requests", out var requests))
        {
            return 0;
        }

        return requests.EnumerateArray()
            .Count(entry =>
            {
                var request = entry.GetProperty("request");
                var url = request.GetProperty("url").GetString() ?? string.Empty;
                return url.StartsWith(path, StringComparison.OrdinalIgnoreCase);
            });
    }

    private async Task AssertCreatedOrDumpAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Created)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new Xunit.Sdk.XunitException(
            $"Expected 201 Created, got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
    }

    private sealed record GoogleEventsListDto(GoogleEventDto[]? Items);

    private sealed record GoogleEventDto(string Id, GoogleRemindersDto? Reminders);

    private sealed record GoogleRemindersDto(bool UseDefault, GoogleReminderOverrideDto[]? Overrides);

    private sealed record GoogleReminderOverrideDto(string Method, int Minutes);
}
