using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.DataService.Controllers;

[ApiController]
[Route("data/v1/participations")]
[Produces("application/json")]
public class ParticipationsController : ControllerBase
{
    private readonly IParticipationRepository _participationRepository;

    public ParticipationsController(IParticipationRepository participationRepository)
    {
        _participationRepository = participationRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> Add([FromBody] Participation participation)
    {
        var created = await _participationRepository.AddAsync(participation);
        return Ok(created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Participation>> GetById(Guid id)
    {
        var participation = await _participationRepository.GetByIdAsync(id);
        if (participation == null)
        {
            return NotFound();
        }

        return Ok(participation);
    }

    [HttpGet("by-user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetByUser(Guid userId)
    {
        var participations = await _participationRepository.GetByUserIdAsync(userId);
        return Ok(participations);
    }

    [HttpGet("by-event/{eventId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetByEvent(Guid eventId)
    {
        var participations = await _participationRepository.GetByEventIdAsync(eventId);
        return Ok(participations);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Participation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Participation>>> GetAll()
    {
        var participations = await _participationRepository.GetAllAsync();
        return Ok(participations);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Participation), StatusCodes.Status200OK)]
    public async Task<ActionResult<Participation>> Update(Guid id, [FromBody] Participation participation)
    {
        participation.Id = id;
        var updated = await _participationRepository.UpdateAsync(participation);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _participationRepository.RemoveAsync(id);
        return NoContent();
    }
}
