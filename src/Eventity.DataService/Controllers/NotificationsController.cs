using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.DataService.Controllers;

[ApiController]
[Route("data/v1/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    public async Task<ActionResult<Notification>> Add([FromBody] Notification notification)
    {
        var created = await _notificationRepository.AddAsync(notification);
        return Ok(created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Notification>> GetById(Guid id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    [HttpGet("by-participation/{participationId:guid}")]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Notification>> GetByParticipation(Guid participationId)
    {
        var notification = await _notificationRepository.GetByParticipationIdAsync(participationId);
        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Notification>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Notification>>> GetAll()
    {
        var notifications = await _notificationRepository.GetAllAsync();
        return Ok(notifications);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Notification), StatusCodes.Status200OK)]
    public async Task<ActionResult<Notification>> Update(Guid id, [FromBody] Notification notification)
    {
        notification.Id = id;
        var updated = await _notificationRepository.UpdateAsync(notification);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _notificationRepository.RemoveAsync(id);
        return NoContent();
    }
}
