using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using Eventity.Domain.Contracts;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;

namespace Eventity.Web.CoreClients;

public class CoreUserServiceClient : CoreServiceClientBase, IUserService
{
    public CoreUserServiceClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<User> AddUser(string name, string email, string login, string password, UserRoleEnum role)
    {
        var user = new User(Guid.NewGuid(), name, email, login, password, role);
        var response = await Client.PostAsJsonAsync("core/v1/users", user, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions) ?? user;
    }

    public async Task<User> GetUserById(Guid id)
    {
        var response = await Client.GetAsync($"core/v1/users/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions)
               ?? throw new InvalidOperationException("Empty user response.");
    }

    public async Task<User> GetUserByLogin(string login)
    {
        var response = await Client.GetAsync($"core/v1/users/by-login/{Uri.EscapeDataString(login)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions)
               ?? throw new InvalidOperationException("Empty user response.");
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        var response = await Client.GetAsync("core/v1/users");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<User>>(JsonOptions) ?? Array.Empty<User>();
    }

    public async Task<IEnumerable<User>> GetUsers(string? login)
    {
        var url = string.IsNullOrWhiteSpace(login)
            ? "core/v1/users"
            : $"core/v1/users?login={Uri.EscapeDataString(login)}";
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<User>>(JsonOptions) ?? Array.Empty<User>();
    }

    public async Task<User> UpdateUser(Guid id, string? name, string? email, string? login, string? password)
    {
        var request = new UpdateUserRequest
        {
            Name = name,
            Email = email,
            Login = login,
            Password = password
        };

        var response = await Client.PutAsJsonAsync($"core/v1/users/{id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions)
               ?? throw new InvalidOperationException("Empty user response.");
    }

    public async Task RemoveUser(Guid id)
    {
        var response = await Client.DeleteAsync($"core/v1/users/{id}");
        response.EnsureSuccessStatusCode();
    }
}
