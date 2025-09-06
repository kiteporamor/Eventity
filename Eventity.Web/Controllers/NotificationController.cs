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
//TODO: Update убрать из сервиса? 

namespace Eventity.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;
    private readonly NotificationDtoConverter _dtoConverter;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger, 
        NotificationDtoConverter dtoConverter)
    {
        _notificationService = notificationService;
        _logger = logger;
        _dtoConverter = dtoConverter;
    }
    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> CreateNotification([FromBody] 
        NotificationRequestDto requestDto)
    {
        try
        {
            var newNotification = await _notificationService.AddNotification(
                requestDto.ParticipationId);

            return CreatedAtAction(
                nameof(GetNotificationId), 
                new { id = newNotification.Id }, 
                _dtoConverter.ToResponseDto(newNotification));
        }
        catch (NotificationServiceException ex)
        {
            _logger.LogError(ex, "Failed to create notification");
            return BadRequest(new ProblemDetails { 
                Title = "Notification creation failed", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> GetNotificationId(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationById(id);
            return Ok(_dtoConverter.ToResponseDto(notification));
        }
        catch (NotificationServiceException ex)
        {
            _logger.LogWarning(ex, "Notification not found: {NotificationId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Notification not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification by ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet("{participation_id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> GetNotificationByParticipationId(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationByParticipationId(id);
            return Ok(_dtoConverter.ToResponseDto(notification));
        }
        catch (NotificationServiceException ex)
        {
            _logger.LogWarning(ex, "Notification not found: {NotificationId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Notification not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification by ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetAllNotifications()
    {
        try
        {
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(notifications.Select(_dtoConverter.ToResponseDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all notifications");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        try
        {
            await _notificationService.RemoveNotification(id);
            return NoContent();
        }
        catch (NotificationServiceException ex)
        {
            _logger.LogError(ex, "Failed to delete notification {NotificationId}", id);
            return NotFound(new ProblemDetails { 
                Title = "Notification not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}
