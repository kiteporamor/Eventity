using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using Eventity.Domain.Contracts;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;

namespace Eventity.Web.CoreClients;

public class CoreEventServiceClient : CoreServiceClientBase, IEventService
{
    public CoreEventServiceClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<Event> AddEvent(string title, string description, DateTime dateTime, string address, Guid organizerId)
    {
        var eventModel = new Event(Guid.NewGuid(), title, description, dateTime, address, organizerId);
        var response = await Client.PostAsJsonAsync("core/v1/events", eventModel, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Event>(JsonOptions) ?? eventModel;
    }

    public async Task<Event> GetEventById(Guid id)
    {
        var response = await Client.GetAsync($"core/v1/events/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Event>(JsonOptions)
               ?? throw new InvalidOperationException("Empty event response.");
    }

    public async Task<IEnumerable<Event>> GetEventByTitle(string title)
    {
        var response = await Client.GetAsync($"core/v1/events?title={Uri.EscapeDataString(title)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Event>>(JsonOptions) ?? Array.Empty<Event>();
    }

    public async Task<IEnumerable<Event>> GetAllEvents()
    {
        var response = await Client.GetAsync("core/v1/events");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Event>>(JsonOptions) ?? Array.Empty<Event>();
    }

    public async Task<Event> UpdateEvent(Guid id, string? title, string? description, DateTime? dateTime, string? address, Validation validation)
    {
        var request = new UpdateEventRequest
        {
            Title = title,
            Description = description,
            DateTime = dateTime,
            Address = address,
            Validation = validation
        };

        var response = await Client.PutAsJsonAsync($"core/v1/events/{id}", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Event>(JsonOptions)
               ?? throw new InvalidOperationException("Empty event response.");
    }

    public async Task RemoveEvent(Guid id, Validation validation)
    {
        var request = new ValidationRequest { Validation = validation };
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"core/v1/events/{id}")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        });
        response.EnsureSuccessStatusCode();
    }
}
