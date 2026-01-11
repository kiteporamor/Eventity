using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eventity.Application.Services;

public class CalendarConfiguration
{
    public const string CalendarSection = "Calendar";

    public string Mode { get; set; } = "Mock";
    public string MockBaseUrl { get; set; } = "http://calendar-mock:8080";
    public string Provider { get; set; } = "Google";
    public string GoogleBaseUrl { get; set; } = "https://www.googleapis.com/calendar/v3";
    public string GoogleCalendarId { get; set; } = "primary";
    public string GoogleAccessToken { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
}

public class CalendarService : ICalendarService
{
    private readonly HttpClient _httpClient;
    private readonly CalendarConfiguration _calendarConfig;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(HttpClient httpClient, IOptions<CalendarConfiguration> calendarOptions,
        ILogger<CalendarService> logger)
    {
        _httpClient = httpClient;
        _calendarConfig = calendarOptions.Value;
        _logger = logger;
    }

    public async Task AddEventToCalendarAsync(User user, Event eventInfo, CancellationToken cancellationToken = default)
    {
        if (IsMockMode())
        {
            var request = new CalendarEventRequest
            {
                EventId = eventInfo.Id,
                UserId = user.Id,
                UserEmail = user.Email,
                Title = eventInfo.Title,
                Description = eventInfo.Description,
                StartAt = eventInfo.DateTime,
                Address = eventInfo.Address
            };

            var response = await SendWithRetryAsync(
                ct => _httpClient.PostAsJsonAsync("/api/calendar/events", request, ct),
                "Calendar event creation failed",
                cancellationToken);
            await EnsureSuccessAsync(response, "Calendar event creation failed", cancellationToken);
            return;
        }

        EnsureGoogleProvider();
        EnsureGoogleAuthorization();

        var calendarId = ResolveCalendarId(user);
        var googleEvent = new GoogleEventInsertRequest
        {
            Summary = eventInfo.Title,
            Description = eventInfo.Description,
            Location = eventInfo.Address,
            Start = new GoogleEventTime(eventInfo.DateTime),
            End = new GoogleEventTime(eventInfo.DateTime.AddHours(1)),
            ExtendedProperties = new GoogleExtendedProperties
            {
                Private = new GooglePrivateProperties
                {
                    EventityEventId = eventInfo.Id.ToString()
                }
            }
        };

        var path = $"/calendars/{Uri.EscapeDataString(calendarId)}/events";
        var googleResponse = await SendWithRetryAsync(
            ct => _httpClient.PostAsJsonAsync(path, googleEvent, ct),
            "Google Calendar event creation failed",
            cancellationToken);
        await EnsureSuccessAsync(googleResponse, "Google Calendar event creation failed", cancellationToken);
    }

    public async Task SendReminderAsync(User user, Event eventInfo, string message, DateTime remindAt,
        CancellationToken cancellationToken = default)
    {
        if (IsMockMode())
        {
            var request = new CalendarReminderRequest
            {
                EventId = eventInfo.Id,
                UserId = user.Id,
                UserEmail = user.Email,
                Message = message,
                RemindAt = remindAt
            };

            var response = await SendWithRetryAsync(
                ct => _httpClient.PostAsJsonAsync("/api/calendar/reminders", request, ct),
                "Calendar reminder failed",
                cancellationToken);
            await EnsureSuccessAsync(response, "Calendar reminder failed", cancellationToken);
            return;
        }

        EnsureGoogleProvider();
        EnsureGoogleAuthorization();

        var calendarId = ResolveCalendarId(user);
        var eventId = await FindGoogleEventIdAsync(calendarId, eventInfo.Id, cancellationToken);

        var minutesBefore = Math.Max(0, (int)Math.Round((eventInfo.DateTime - remindAt).TotalMinutes));
        var patch = new GoogleEventPatchRequest
        {
            Description = $"{eventInfo.Description}\nReminder: {message}",
            Reminders = new GoogleReminders
            {
                UseDefault = false,
                Overrides = new[]
                {
                    new GoogleReminderOverride
                    {
                        Method = "popup",
                        Minutes = minutesBefore
                    }
                }
            }
        };

        var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"),
            $"/calendars/{Uri.EscapeDataString(calendarId)}/events/{Uri.EscapeDataString(eventId)}")
        {
            Content = JsonContent.Create(patch)
        };
        var patchResponse = await SendWithRetryAsync(
            ct => _httpClient.SendAsync(patchRequest, ct),
            "Google Calendar reminder update failed",
            cancellationToken);
        await EnsureSuccessAsync(patchResponse, "Google Calendar reminder update failed", cancellationToken);
    }

    private bool IsMockMode()
    {
        return _calendarConfig.Mode.Equals("Mock", StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureGoogleProvider()
    {
        if (!_calendarConfig.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
        {
            throw new CalendarServiceException($"Calendar provider '{_calendarConfig.Provider}' is not supported.");
        }
    }

    private void EnsureGoogleAuthorization()
    {
        if (string.IsNullOrWhiteSpace(_calendarConfig.GoogleAccessToken))
        {
            throw new CalendarServiceException("Google Calendar access token is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _calendarConfig.GoogleAccessToken);
    }

    private string ResolveCalendarId(User user)
    {
        if (string.IsNullOrWhiteSpace(_calendarConfig.GoogleCalendarId))
        {
            throw new CalendarServiceException("Google Calendar ID is not configured.");
        }

        if (_calendarConfig.GoogleCalendarId.Equals("{user.email}", StringComparison.OrdinalIgnoreCase))
        {
            return user.Email;
        }

        return _calendarConfig.GoogleCalendarId;
    }

    private async Task<string> FindGoogleEventIdAsync(string calendarId, Guid eventityEventId,
        CancellationToken cancellationToken)
    {
        var query =
            $"?privateExtendedProperty=eventityEventId={Uri.EscapeDataString(eventityEventId.ToString())}";
        var url = $"/calendars/{Uri.EscapeDataString(calendarId)}/events{query}";
        var response = await _httpClient.GetFromJsonAsync<GoogleEventsListResponse>(url, cancellationToken);

        var googleEventId = response?.Items?.FirstOrDefault()?.Id;
        if (string.IsNullOrWhiteSpace(googleEventId))
        {
            throw new CalendarServiceException("Google Calendar event not found for reminder.");
        }

        return googleEventId;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string errorMessage,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("{Message}: {StatusCode} {Body}", errorMessage, response.StatusCode, body);
            throw new CalendarServiceException(errorMessage);
        }
    }

    private static async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;
        const int delayMs = 200;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var response = await sendAsync(cancellationToken);
                if (response.IsSuccessStatusCode || attempt == maxAttempts)
                {
                    return response;
                }
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
            }
            catch (TaskCanceledException) when (attempt < maxAttempts)
            {
            }

            await Task.Delay(delayMs, cancellationToken);
        }

        throw new CalendarServiceException(errorMessage);
    }

    private sealed class CalendarEventRequest
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    private sealed class CalendarReminderRequest
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime RemindAt { get; set; }
    }

    private sealed class GoogleEventInsertRequest
    {
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public GoogleEventTime Start { get; set; } = null!;
        public GoogleEventTime End { get; set; } = null!;
        public GoogleExtendedProperties ExtendedProperties { get; set; } = null!;
    }

    private sealed class GoogleEventPatchRequest
    {
        public string Description { get; set; } = string.Empty;
        public GoogleReminders Reminders { get; set; } = null!;
    }

    private sealed class GoogleEventTime
    {
        public GoogleEventTime(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public DateTime DateTime { get; set; }
        public string TimeZone { get; set; } = "UTC";
    }

    private sealed class GoogleExtendedProperties
    {
        public GooglePrivateProperties Private { get; set; } = null!;
    }

    private sealed class GooglePrivateProperties
    {
        public string EventityEventId { get; set; } = string.Empty;
    }

    private sealed class GoogleReminders
    {
        public bool UseDefault { get; set; }
        public GoogleReminderOverride[] Overrides { get; set; } = Array.Empty<GoogleReminderOverride>();
    }

    private sealed class GoogleReminderOverride
    {
        public string Method { get; set; } = string.Empty;
        public int Minutes { get; set; }
    }

    private sealed class GoogleEventsListResponse
    {
        public GoogleEventItem[] Items { get; set; } = Array.Empty<GoogleEventItem>();
    }

    private sealed class GoogleEventItem
    {
        public string Id { get; set; } = string.Empty;
    }
}
