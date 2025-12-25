using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;

namespace Eventity.CoreService.DataClients;

public class DataServiceNotificationRepository : DataServiceClientBase, INotificationRepository
{
    public DataServiceNotificationRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("data/v1/notifications", notification, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Notification>(JsonOptions) ?? notification;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/notifications/{id}");
        return await ReadOrDefaultAsync<Notification>(response);
    }

    public async Task<Notification?> GetByParticipationIdAsync(Guid participationId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/notifications/by-participation/{participationId}");
        return await ReadOrDefaultAsync<Notification>(response);
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("data/v1/notifications");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Notification>>(JsonOptions)
               ?? Array.Empty<Notification>();
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"data/v1/notifications/{notification.Id}", notification, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Notification>(JsonOptions) ?? notification;
    }

    public async Task RemoveAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"data/v1/notifications/{id}");
        response.EnsureSuccessStatusCode();
    }
}
