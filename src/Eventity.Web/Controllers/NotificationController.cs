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

namespace Eventity.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;
    private readonly NotificationDtoConverter _dtoConverter;
    private readonly ValidationDtoConverter _validationDtoConverter;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger, 
        NotificationDtoConverter dtoConverter, ValidationDtoConverter validationDtoConverter)
    {
        _notificationService = notificationService;
        _logger = logger;
        _dtoConverter = dtoConverter;
        _validationDtoConverter = validationDtoConverter;
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> CreateNotifications([FromBody] 
        NotificationRequestDto requestDto)
    {
        try
        {
            var validation = _validationDtoConverter.ToDomain(new ValidationDto(GetCurrentUserId(), IsAdmin()));
            
            var newNotifications = await _notificationService.AddNotification(
                requestDto.EventId, requestDto.Type, validation);

            return StatusCode(StatusCodes.Status201Created, newNotifications);
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
    [Authorize(Roles = "Admin")]
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
    
    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> GetAllNotifications(Guid? participation_id)
    {
        try
        {
            var validation = _validationDtoConverter.ToDomain(new ValidationDto(GetCurrentUserId(), IsAdmin()));
            var notifications = await _notificationService.GetNotifications(participation_id, validation);

            return Ok(notifications.Select(_dtoConverter.ToResponseDto));
        }
        catch (NotificationServiceException ex)
        {
            _logger.LogWarning(ex, "Notification not found: {NotificationId}", participation_id);
            return NotFound(new ProblemDetails { 
                Title = "Notification not found", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification by ID {Id}", participation_id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
