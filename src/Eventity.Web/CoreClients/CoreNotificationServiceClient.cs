using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using Eventity.Domain.Contracts;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;

namespace Eventity.Web.CoreClients;

public class CoreNotificationServiceClient : CoreServiceClientBase, INotificationService
{
    public CoreNotificationServiceClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<Notification>> AddNotification(Guid eventId, NotificationTypeEnum type, Validation validation)
    {
        var request = new NotificationCreateRequest
        {
            EventId = eventId,
            Type = type,
            Validation = validation
        };
        var response = await Client.PostAsJsonAsync("core/v1/notifications", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Notification>>(JsonOptions)
               ?? Array.Empty<Notification>();
    }

    public async Task<Notification> GetNotificationById(Guid id)
    {
        var response = await Client.GetAsync($"core/v1/notifications/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Notification>(JsonOptions)
               ?? throw new InvalidOperationException("Empty notification response.");
    }

    public async Task<Notification> GetNotificationByParticipationId(Guid participationId)
    {
        var response = await Client.GetAsync($"core/v1/notifications/by-participation/{participationId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Notification>(JsonOptions)
               ?? throw new InvalidOperationException("Empty notification response.");
    }

    public async Task<IEnumerable<Notification>> GetAllNotifications()
    {
        var response = await Client.GetAsync("core/v1/notifications");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Notification>>(JsonOptions)
               ?? Array.Empty<Notification>();
    }

    public async Task<IEnumerable<Notification>> GetNotifications(Guid? participation_id, Validation validation)
    {
        var request = new NotificationFilterRequest
        {
            ParticipationId = participation_id,
            Validation = validation
        };
        var response = await Client.PostAsJsonAsync("core/v1/notifications/filter", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Notification>>(JsonOptions)
               ?? Array.Empty<Notification>();
    }

    public async Task<Notification> UpdateNotification(Guid id, Guid? participationId, string? text, DateTime? sentAt)
    {
        var request = new NotificationUpdateRequest
        {
            ParticipationId = participationId,
            Text = text,
            SentAt = sentAt
        };
        var response = await Client.PutAsJsonAsync($"core/v1/notifications/{id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Notification>(JsonOptions)
               ?? throw new InvalidOperationException("Empty notification response.");
    }

    public async Task RemoveNotification(Guid id)
    {
        var response = await Client.DeleteAsync($"core/v1/notifications/{id}");
        response.EnsureSuccessStatusCode();
    }
}
