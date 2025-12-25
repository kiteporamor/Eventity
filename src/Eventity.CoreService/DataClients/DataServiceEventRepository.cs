using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;

namespace Eventity.CoreService.DataClients;

public class DataServiceEventRepository : DataServiceClientBase, IEventRepository
{
    public DataServiceEventRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public async Task<Event> AddAsync(Event eventDomain)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("data/v1/events", eventDomain, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Event>(JsonOptions) ?? eventDomain;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/events/{id}");
        return await ReadOrDefaultAsync<Event>(response);
    }

    public async Task<IEnumerable<Event>> GetByTitleAsync(string title)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/events/by-title/{Uri.EscapeDataString(title)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Event>>(JsonOptions) ?? Array.Empty<Event>();
    }

    public async Task<IEnumerable<Event>> GetByOrganizerIdAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/events/by-organizer/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Event>>(JsonOptions) ?? Array.Empty<Event>();
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("data/v1/events");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Event>>(JsonOptions) ?? Array.Empty<Event>();
    }

    public async Task<Event> UpdateAsync(Event eventDomain)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"data/v1/events/{eventDomain.Id}", eventDomain, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Event>(JsonOptions) ?? eventDomain;
    }

    public async Task RemoveAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"data/v1/events/{id}");
        response.EnsureSuccessStatusCode();
    }
}
