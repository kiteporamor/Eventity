using System;
using System.Collections.Generic;
using System.Linq;
using Eventity.Application.Services;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Services;
using Eventity.Web.Converters;
using Eventity.Web.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Eventity.Api.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventController> _logger;
    private readonly EventDtoConverter _dtoConverter;

    public EventController(IEventService eventService, ILogger<EventController> logger, EventDtoConverter dtoConverter)
    {
        _eventService = eventService;
        _logger = logger;
        _dtoConverter = dtoConverter;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> CreateEvent([FromBody] CreateEventRequestDto requestDto)
    {
        try
        {
            var newEvent = await _eventService.AddEvent(
                requestDto.Title,
                requestDto.Description,
                requestDto.DateTime,
                requestDto.Address,
                requestDto.OrganizerId);

            return CreatedAtAction(
                nameof(GetEventById), 
                new { id = newEvent.Id }, 
                _dtoConverter.ToResponseDto(newEvent));
        }
        catch (EventServiceException ex)
        {
            _logger.LogError(ex, "Failed to create event");
            return BadRequest(new ProblemDetails { 
                Title = "Event creation failed", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> GetEventById(Guid id)
    {
        try
        {
            var eventItem = await _eventService.GetEventById(id);
            return Ok(_dtoConverter.ToResponseDto(eventItem));
        }
        catch (EventServiceException ex)
        {
            _logger.LogWarning(ex, "Event not found: {EventId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Event not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEvents()
    {
        try
        {
            var events = await _eventService.GetAllEvents();
            return Ok(events.Select(_dtoConverter.ToResponseDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all events");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventRequestDto requestDto)
    {
        try
        {
            var updatedEvent = await _eventService.UpdateEvent(
                id,
                requestDto.Title,
                requestDto.Description,
                requestDto.DateTime,
                requestDto.Address);

            return Ok(_dtoConverter.ToResponseDto(updatedEvent));
        }
        catch (EventServiceException ex)
        {
            _logger.LogError(ex, "Failed to update event {EventId}", id);
            return BadRequest(new ProblemDetails { 
                Title = "Update failed", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        try
        {
            await _eventService.RemoveEvent(id);
            return NoContent();
        }
        catch (EventServiceException ex)
        {
            _logger.LogError(ex, "Failed to delete event {EventId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Event not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId);
    }
}
