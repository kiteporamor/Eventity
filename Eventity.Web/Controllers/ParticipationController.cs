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
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

//TODO: использование ErrorResponseDto
namespace Eventity.Api.Controllers;

[ApiController]
[Route("api/participations")]
[Produces("application/json")]
public class ParticipationController : ControllerBase
{
    private readonly IParticipationService _participationService;
    private readonly ILogger<ParticipationController> _logger;
    private readonly ParticipationDtoConverter _dtoConverter;

    public ParticipationController(IParticipationService participationService, ILogger<ParticipationController> logger, 
        ParticipationDtoConverter dtoConverter)
    {
        _participationService = participationService;
        _logger = logger;
        _dtoConverter = dtoConverter;
    }
    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ParticipationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParticipationResponseDto>> CreateParticipation([FromBody] 
        ParticipationRequestDto requestDto)
    {
        try
        {
            var newParticipation = await _participationService.AddParticipation(
                requestDto.UserId,
                requestDto.EventId,
                requestDto.Role,
                requestDto.Status);

            return CreatedAtAction(
                nameof(GetParticipationById), 
                new { id = newParticipation.Id }, 
                _dtoConverter.ToResponseDto(newParticipation));
        }
        catch (ParticipationServiceException ex)
        {
            _logger.LogError(ex, "Failed to create participation");
            return BadRequest(new ProblemDetails { 
                Title = "Participation creation failed", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating participation");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ParticipationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParticipationResponseDto>> GetParticipationById(Guid id)
    {
        try
        {
            var participation = await _participationService.GetParticipationById(id);
            return Ok(_dtoConverter.ToResponseDto(participation));
        }
        catch (ParticipationServiceException ex)
        {
            _logger.LogWarning(ex, "Participation not found: {ParticipationId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Participation not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participation by ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserParticipationInfoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserParticipationInfoResponseDto>>> GetParticipationsDetailed(
        string? organizer_login, string? event_title)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID format");
            }

            IEnumerable<UserParticipationInfo> participations = new List<UserParticipationInfo>();
            if (organizer_login == null && event_title == null)
            {
                participations = await _participationService
                    .GetUserParticipationInfoByUserId(userId);
            }
            else
            {
                IEnumerable<UserParticipationInfo> participations_by_organizer = new List<UserParticipationInfo>();
                IEnumerable<UserParticipationInfo> participations_by_title = new List<UserParticipationInfo>();
                if (organizer_login != null)
                {
                    participations_by_organizer = await _participationService
                        .GetUserParticipationInfoByOrganizerLogin(userId, organizer_login);
                }
                if (event_title != null)
                {
                    participations_by_title = await _participationService
                        .GetUserParticipationInfoByEventTitle(userId, event_title);
                }
                if (organizer_login != null && event_title != null)
                {
                    participations = participations_by_organizer.Intersect(participations_by_title);
                }
                else
                {
                    participations = participations_by_organizer.Union(participations_by_title);
                }
            }
            return Ok(participations.Select(_dtoConverter.ToResponseDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all participations by organizer login");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpPatch("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ParticipationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParticipationResponseDto>> UpdateParticipation(Guid id,
        [FromBody] UpdateParticipationRequestDto requestDto)
    {
        try
        {
            var updatedParticipation = await _participationService.UpdateParticipation(
                id,
                requestDto.Status);

            return Ok(_dtoConverter.ToResponseDto(updatedParticipation));
        }
        catch (ParticipationServiceException ex)
        {
            _logger.LogError(ex, "Failed to update participation {ParticipationId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Update failed",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participation {ParticipationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteParticipation(Guid id)
    {
        try
        {
            await _participationService.RemoveParticipation(id);
            return NoContent();
        }
        catch (ParticipationServiceException ex)
        {
            _logger.LogError(ex, "Failed to delete participation {ParticipationId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Participation not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting participation {ParticipationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}
