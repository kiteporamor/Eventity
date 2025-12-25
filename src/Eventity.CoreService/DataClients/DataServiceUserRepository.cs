using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;

namespace Eventity.CoreService.DataClients;

public class DataServiceUserRepository : DataServiceClientBase, IUserRepository
{
    public DataServiceUserRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public async Task<User> AddAsync(User user)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("data/v1/users", user, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions) ?? user;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/users/{id}");
        return await ReadOrDefaultAsync<User>(response);
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"data/v1/users/by-login/{Uri.EscapeDataString(login)}");
        return await ReadOrDefaultAsync<User>(response);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("data/v1/users");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<User>>(JsonOptions) ?? Array.Empty<User>();
    }

    public async Task<User> UpdateAsync(User user)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"data/v1/users/{user.Id}", user, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions) ?? user;
    }

    public async Task RemoveAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"data/v1/users/{id}");
        response.EnsureSuccessStatusCode();
    }
}
