using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventity.Domain.Contracts;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.CoreService.Controllers;

[ApiController]
[Route("core/v1/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    public async Task<ActionResult<Event>> Add([FromBody] Event eventModel)
    {
        var created = await _eventService.AddEvent(
            eventModel.Title, eventModel.Description, eventModel.DateTime, eventModel.Address, eventModel.OrganizerId);
        return Ok(created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    public async Task<ActionResult<Event>> GetById(Guid id)
    {
        var eventModel = await _eventService.GetEventById(id);
        return Ok(eventModel);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll([FromQuery] string? title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            var byTitle = await _eventService.GetEventByTitle(title);
            return Ok(byTitle);
        }

        var events = await _eventService.GetAllEvents();
        return Ok(events);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    public async Task<ActionResult<Event>> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        var updated = await _eventService.UpdateEvent(
            id,
            request.Title,
            request.Description,
            request.DateTime,
            request.Address,
            request.Validation);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, [FromBody] ValidationRequest request)
    {
        await _eventService.RemoveEvent(id, request.Validation);
        return NoContent();
    }
}
