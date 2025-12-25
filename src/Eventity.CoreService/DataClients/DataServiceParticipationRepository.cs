using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;

namespace Eventity.CoreService.DataClients;

public class DataServiceParticipationRepository : DataServiceClientBase, IParticipationRepository
{
    public DataServiceParticipationRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public async Task<Participation> AddAsync(Participation participation)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("data/v1/participations", participation, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions) ?? participation;
    }

    public async Task<Participation?> GetByIdAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/participations/{id}");
        return await ReadOrDefaultAsync<Participation>(response);
    }

    public async Task<IEnumerable<Participation>> GetByUserIdAsync(Guid userId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/participations/by-user/{userId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<IEnumerable<Participation>> GetByEventIdAsync(Guid eventId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/participations/by-event/{eventId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<IEnumerable<Participation>> GetAllAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("data/v1/participations");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<Participation> UpdateAsync(Participation participation)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"data/v1/participations/{participation.Id}", participation, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions) ?? participation;
    }

    public async Task RemoveAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"data/v1/participations/{id}");
        response.EnsureSuccessStatusCode();
    }
}
