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
    [ProducesResponseType(typeof(IEnumerable<ParticipationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParticipationResponseDto>>> GetAllParticipations()
    {
        try
        {
            var participations = await _participationService.GetAllParticipations();
            return Ok(participations.Select(_dtoConverter.ToResponseDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all participations");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet("{user_id}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ParticipationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParticipationResponseDto>>> GetAllUserParticipations(Guid userId)
    {
        try
        {
            var participations = await _participationService.GetParticipationsByUserId(userId);
            return Ok(participations.Select(_dtoConverter.ToResponseDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all participations by userId");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpPut("{id}")]
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
