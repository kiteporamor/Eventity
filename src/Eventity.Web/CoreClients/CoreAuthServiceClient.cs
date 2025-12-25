using System.Net.Http.Json;
using Eventity.Domain.Contracts;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;

namespace Eventity.Web.CoreClients;

public class CoreAuthServiceClient : CoreServiceClientBase, IAuthService
{
    public CoreAuthServiceClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<AuthResult> AuthenticateUser(string login, string password)
    {
        var request = new AuthLoginRequest { Login = login, Password = password };
        var response = await Client.PostAsJsonAsync("core/v1/auth/login", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions)
               ?? throw new InvalidOperationException("Empty auth response.");
    }

    public async Task<AuthResult> RegisterUser(string name, string email, string login, string password, Eventity.Domain.Enums.UserRoleEnum role)
    {
        var request = new AuthRegisterRequest
        {
            Name = name,
            Email = email,
            Login = login,
            Password = password,
            Role = role
        };

        var response = await Client.PostAsJsonAsync("core/v1/auth/register", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions)
               ?? throw new InvalidOperationException("Empty auth response.");
    }
}
