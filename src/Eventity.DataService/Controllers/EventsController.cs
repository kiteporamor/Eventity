using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.DataService.Controllers;

[ApiController]
[Route("data/v1/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;

    public EventsController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    public async Task<ActionResult<Event>> Add([FromBody] Event eventModel)
    {
        var created = await _eventRepository.AddAsync(eventModel);
        return Ok(created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> GetById(Guid id)
    {
        var eventModel = await _eventRepository.GetByIdAsync(id);
        if (eventModel == null)
        {
            return NotFound();
        }

        return Ok(eventModel);
    }

    [HttpGet("by-title/{title}")]
    [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetByTitle(string title)
    {
        var events = await _eventRepository.GetByTitleAsync(title);
        return Ok(events ?? Array.Empty<Event>());
    }

    [HttpGet("by-organizer/{organizerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetByOrganizer(Guid organizerId)
    {
        var events = await _eventRepository.GetByOrganizerIdAsync(organizerId);
        return Ok(events ?? Array.Empty<Event>());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll()
    {
        var events = await _eventRepository.GetAllAsync();
        return Ok(events);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    public async Task<ActionResult<Event>> Update(Guid id, [FromBody] Event eventModel)
    {
        eventModel.Id = id;
        var updated = await _eventRepository.UpdateAsync(eventModel);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _eventRepository.RemoveAsync(id);
        return NoContent();
    }
}
