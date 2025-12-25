using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Eventity.Api.Controllers;

[ApiController]
[Route("api/v1/participations")]
[Produces("application/json")]
public class ParticipationController : ControllerBase
{
    private readonly IParticipationService _participationService;
    private readonly ILogger<ParticipationController> _logger;
    private readonly ParticipationDtoConverter _dtoConverter;
    private readonly ValidationDtoConverter _validationDtoConverter;

    public ParticipationController(IParticipationService participationService, ILogger<ParticipationController> logger, 
        ParticipationDtoConverter dtoConverter, ValidationDtoConverter validationDtoConverter)
    {
        _participationService = participationService;
        _logger = logger;
        _dtoConverter = dtoConverter;
        _validationDtoConverter = validationDtoConverter;
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(ParticipationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParticipationResponseDto>> CreateParticipation([FromBody] 
        ParticipationRequestDto requestDto)
    {
        try
        {
            var validation = _validationDtoConverter.ToDomain(new ValidationDto(GetCurrentUserId(), IsAdmin()));
            var newParticipation = await _participationService.AddParticipation(
                requestDto.UserId,
                requestDto.EventId,
                requestDto.Role,
                requestDto.Status,
                validation);

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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(IEnumerable<UserParticipationInfoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserParticipationInfoResponseDto>>> GetParticipationsDetailed(
        string? organizer_login, string? event_title, Guid? user_id)
    {
        try
        {
            var validation = _validationDtoConverter.ToDomain(new ValidationDto(GetCurrentUserId(), IsAdmin()));

            var participations = await _participationService
                    .GetUserParticipationsDetailed(organizer_login, event_title, validation, user_id);
            
            return Ok(participations.Select(_dtoConverter.ToResponseDto));
        }
        catch (ParticipationServiceException ex)
        {
            _logger.LogError(ex, "Failed to get participations");
            return BadRequest(new ProblemDetails
            {
                Title = "Get failed",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all participations by organizer login");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(ParticipationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParticipationResponseDto>> UpdateParticipation(Guid id,
        [FromBody] UpdateParticipationRequestDto requestDto)
    {
        try
        {
            var validation = _validationDtoConverter.ToDomain(new ValidationDto(GetCurrentUserId(), IsAdmin()));
            var updatedParticipation = await _participationService.UpdateParticipation(
                id, requestDto.Status, validation);

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
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteParticipation(Guid id)
    {
        try
        {
            var validation = _validationDtoConverter.ToDomain(new ValidationDto(GetCurrentUserId(), IsAdmin()));
            await _participationService.RemoveParticipation(id, validation);
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
    
    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId);
    }

    private string GetCurrentUserRole()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(role))
            throw new UnauthorizedAccessException("Role not found in token");
        return role;
    }
    
    private bool IsAdmin()
    {
        var role = GetCurrentUserRole();
        if (role == "Admin")
            return true;
        return false;
    }
}
