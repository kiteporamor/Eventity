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
[Route("core/v1/participations")]
[Produces("application/json")]
public class ParticipationsController : ControllerBase
{
    private readonly IParticipationService _participationService;

    public ParticipationsController(IParticipationService participationService)
    {
        _participationService = participationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> Add([FromBody] AddParticipationRequest request)
    {
        var participation = await _participationService.AddParticipation(
            request.UserId,
            request.EventId,
            request.Role,
            request.Status,
            request.Validation);
        return Ok(participation);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> GetById(Guid id)
    {
        var participation = await _participationService.GetParticipationById(id);
        return Ok(participation);
    }

    [HttpGet("by-event/{eventId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetByEvent(Guid eventId)
    {
        var participations = await _participationService.GetParticipationsByEventId(eventId);
        return Ok(participations);
    }

    [HttpGet("by-user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetByUser(Guid userId)
    {
        var participations = await _participationService.GetParticipationsByUserId(userId);
        return Ok(participations);
    }

    [HttpGet("by-user/{userId:guid}/event/{eventId:guid}")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> GetByUserAndEvent(Guid userId, Guid eventId)
    {
        var participation = await _participationService.GetParticipationByUserIdAndEventId(userId, eventId);
        return Ok(participation);
    }

    [HttpGet("organizer/{eventId:guid}")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> GetOrganizer(Guid eventId)
    {
        var organizer = await _participationService.GetOrganizerByEventId(eventId);
        return Ok(organizer);
    }

    [HttpGet("by-event/{eventId:guid}/participants")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetParticipants(Guid eventId)
    {
        var participants = await _participationService.GetAllParticipantsByEventId(eventId);
        return Ok(participants);
    }

    [HttpGet("by-event/{eventId:guid}/left")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetLeftParticipants(Guid eventId)
    {
        var left = await _participationService.GetAllLeftParticipantsByEventId(eventId);
        return Ok(left);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetAll()
    {
        var participations = await _participationService.GetAllParticipations();
        return Ok(participations);
    }

    [HttpGet("all-info")]
    [ProducesResponseType(typeof(IEnumerable<UserParticipationInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserParticipationInfo>>> GetAllInfos()
    {
        var infos = await _participationService.GetAllParticipationInfos();
        return Ok(infos);
    }

    [HttpPost("user-info")]
    [ProducesResponseType(typeof(IEnumerable<UserParticipationInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserParticipationInfo>>> GetUserInfos(
        [FromBody] ParticipationUserInfoRequest request)
    {
        var infos = await _participationService.GetUserParticipationsDetailed(
            request.OrganizerLogin,
            request.EventTitle,
            request.Validation,
            request.UserId);
        return Ok(infos);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> Update(Guid id, [FromBody] UpdateParticipationRequest request)
    {
        var updated = await _participationService.UpdateParticipation(id, request.Status, request.Validation);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> ChangeStatus(
        Guid id,
        [FromBody] ChangeParticipationStatusRequest request)
    {
        var updated = await _participationService.ChangeParticipationStatus(id, request.Status);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> ChangeRole(
        Guid id,
        [FromBody] ChangeParticipationRoleRequest request)
    {
        var updated = await _participationService.ChangeParticipationRole(id, request.Role);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, [FromBody] ValidationRequest request)
    {
        await _participationService.RemoveParticipation(id, request.Validation);
        return NoContent();
    }
}
