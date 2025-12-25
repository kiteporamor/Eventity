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
[Route("core/v1/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<Notification>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Notification>>> Add([FromBody] NotificationCreateRequest request)
    {
        var notifications = await _notificationService.AddNotification(
            request.EventId, request.Type, request.Validation);
        return Ok(notifications);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    public async Task<ActionResult<Notification>> GetById(Guid id)
    {
        var notification = await _notificationService.GetNotificationById(id);
        return Ok(notification);
    }

    [HttpGet("by-participation/{participationId:guid}")]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    public async Task<ActionResult<Notification>> GetByParticipation(Guid participationId)
    {
        var notification = await _notificationService.GetNotificationByParticipationId(participationId);
        return Ok(notification);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Notification>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Notification>>> GetAll()
    {
        var notifications = await _notificationService.GetAllNotifications();
        return Ok(notifications);
    }

    [HttpPost("filter")]
    [ProducesResponseType(typeof(IEnumerable<Notification>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Notification>>> GetFiltered([FromBody] NotificationFilterRequest request)
    {
        var notifications = await _notificationService.GetNotifications(
            request.ParticipationId, request.Validation);
        return Ok(notifications);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    public async Task<ActionResult<Notification>> Update(Guid id, [FromBody] NotificationUpdateRequest request)
    {
        var updated = await _notificationService.UpdateNotification(
            id, request.ParticipationId, request.Text, request.SentAt);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _notificationService.RemoveNotification(id);
        return NoContent();
    }
}
