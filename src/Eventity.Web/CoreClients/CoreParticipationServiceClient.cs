using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using Eventity.Domain.Contracts;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;

namespace Eventity.Web.CoreClients;

public class CoreParticipationServiceClient : CoreServiceClientBase, IParticipationService
{
    public CoreParticipationServiceClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<Participation> AddParticipation(Guid userId, Guid eventId, ParticipationRoleEnum participationRole, ParticipationStatusEnum status, Validation validation)
    {
        var request = new AddParticipationRequest
        {
            UserId = userId,
            EventId = eventId,
            Role = participationRole,
            Status = status,
            Validation = validation
        };
        var response = await Client.PostAsJsonAsync("core/v1/participations", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task<Participation> GetParticipationById(Guid id)
    {
        var response = await Client.GetAsync($"core/v1/participations/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task<IEnumerable<Participation>> GetParticipationsByEventId(Guid eventId)
    {
        var response = await Client.GetAsync($"core/v1/participations/by-event/{eventId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationsDetailed(string? organizer_login, string? event_title, Validation validation, Guid? user_id)
    {
        var request = new ParticipationUserInfoRequest
        {
            OrganizerLogin = organizer_login,
            EventTitle = event_title,
            UserId = user_id,
            Validation = validation
        };
        var response = await Client.PostAsJsonAsync("core/v1/participations/user-info", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserParticipationInfo>>(JsonOptions)
               ?? Array.Empty<UserParticipationInfo>();
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByUserId(Guid userId)
    {
        var request = new ParticipationUserInfoRequest
        {
            UserId = userId,
            Validation = new Validation { CurrentUserId = userId, IsAdmin = false }
        };
        var response = await Client.PostAsJsonAsync("core/v1/participations/user-info", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserParticipationInfo>>(JsonOptions)
               ?? Array.Empty<UserParticipationInfo>();
    }

    public async Task<IEnumerable<Participation>> GetParticipationsByUserId(Guid userId)
    {
        var response = await Client.GetAsync($"core/v1/participations/by-user/{userId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<Participation> GetOrganizerByEventId(Guid eventId)
    {
        var response = await Client.GetAsync($"core/v1/participations/organizer/{eventId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByEventTitle(Guid userId, string title)
    {
        var response = await Client.PostAsJsonAsync("core/v1/participations/user-info", new ParticipationUserInfoRequest
        {
            EventTitle = title,
            UserId = userId,
            Validation = new Validation { CurrentUserId = userId, IsAdmin = false }
        }, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserParticipationInfo>>(JsonOptions)
               ?? Array.Empty<UserParticipationInfo>();
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetUserParticipationInfoByOrganizerLogin(Guid userId, string login)
    {
        var response = await Client.PostAsJsonAsync("core/v1/participations/user-info", new ParticipationUserInfoRequest
        {
            OrganizerLogin = login,
            UserId = userId,
            Validation = new Validation { CurrentUserId = userId, IsAdmin = false }
        }, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserParticipationInfo>>(JsonOptions)
               ?? Array.Empty<UserParticipationInfo>();
    }

    public async Task<IEnumerable<Participation>> GetAllParticipantsByEventId(Guid eventId)
    {
        var response = await Client.GetAsync($"core/v1/participations/by-event/{eventId}/participants");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<IEnumerable<Participation>> GetAllLeftParticipantsByEventId(Guid eventId)
    {
        var response = await Client.GetAsync($"core/v1/participations/by-event/{eventId}/left");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<Participation> GetParticipationByUserIdAndEventId(Guid userId, Guid eventId)
    {
        var response = await Client.GetAsync($"core/v1/participations/by-user/{userId}/event/{eventId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task<IEnumerable<Participation>> GetAllParticipations()
    {
        var response = await Client.GetAsync("core/v1/participations/all");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Participation>>(JsonOptions)
               ?? Array.Empty<Participation>();
    }

    public async Task<IEnumerable<UserParticipationInfo>> GetAllParticipationInfos()
    {
        var response = await Client.GetAsync("core/v1/participations/all-info");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<UserParticipationInfo>>(JsonOptions)
               ?? Array.Empty<UserParticipationInfo>();
    }

    public async Task<Participation> UpdateParticipation(Guid id, ParticipationStatusEnum? status, Validation validation)
    {
        var request = new UpdateParticipationRequest
        {
            Status = status,
            Validation = validation
        };
        var response = await Client.PutAsJsonAsync($"core/v1/participations/{id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task<Participation> ChangeParticipationStatus(Guid id, ParticipationStatusEnum status)
    {
        var request = new ChangeParticipationStatusRequest { Status = status };
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"core/v1/participations/{id}/status")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task<Participation> ChangeParticipationRole(Guid id, ParticipationRoleEnum participationRole)
    {
        var request = new ChangeParticipationRoleRequest { Role = participationRole };
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"core/v1/participations/{id}/role")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Participation>(JsonOptions)
               ?? throw new InvalidOperationException("Empty participation response.");
    }

    public async Task RemoveParticipation(Guid id, Validation validation)
    {
        var request = new ValidationRequest { Validation = validation };
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"core/v1/participations/{id}")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        });
        response.EnsureSuccessStatusCode();
    }

}
