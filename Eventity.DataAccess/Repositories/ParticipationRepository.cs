using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.DataAccess.Context;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Models;
using Eventity.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Repositories;

public class ParticipationRepository : IParticipationRepository
{
    private readonly EventityDbContext _context;
    private readonly ILogger<ParticipationRepository> _logger;

    public ParticipationRepository(EventityDbContext context, ILogger<ParticipationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Participation> AddAsync(Participation participation)
    {
        try
        {
            var participationDb = participation.ToDb();
            await _context.Participations.AddAsync(participationDb);
            await _context.SaveChangesAsync();

            return participation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating participation");
            throw new ParticipationRepositoryException("Failed to create participation", ex);
        }
    }

    public async Task<Participation?> GetByIdAsync(Guid id)
    {
        try
        {
            var participationDb = await _context.Participations.FirstOrDefaultAsync(p => p.Id == id);
            return participationDb?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participation with Id {ParticipationId}", id);
            throw new ParticipationRepositoryException("Failed to retrieve participation", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var participations = await _context.Participations
                .Where(p => p.UserId == userId)
                .ToListAsync();
            
            return participations.Select(p => p.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participations by user Id {UserId}", userId);
            throw new ParticipationRepositoryException("Failed to retrieve participations by user", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetByEventIdAsync(Guid eventId)
    {
        try
        {
            var participations = await _context.Participations
                .Where(p => p.EventId == eventId)
                .ToListAsync();
            
            return participations.Select(p => p.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving participations by event Id {EventId}", eventId);
            throw new ParticipationRepositoryException("Failed to retrieve participations by event", ex);
        }
    }

    public async Task<IEnumerable<Participation>> GetAllAsync()
    {
        try
        {
            var participations = await _context.Participations.ToListAsync();
            return participations.Select(p => p.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all participations");
            throw new ParticipationRepositoryException("Failed to retrieve all participations", ex);
        }
    }

    public async Task<Participation> UpdateAsync(Participation participation)
    {
        try
        {
            var participationDb = await _context.Participations.FirstOrDefaultAsync(p => p.Id == participation.Id);

            if (participationDb is null)
            {
                _logger.LogWarning("Participation with Id {ParticipationId} not found for update", participation.Id);
                throw new ParticipationRepositoryException($"Participation with Id {participation.Id} not found");
            }

            participationDb.UserId = participation.UserId;
            participationDb.EventId = participation.EventId;
            participationDb.Status = participation.Status;

            _context.Participations.Update(participationDb);
            await _context.SaveChangesAsync();

            return participation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating participation with Id {ParticipationId}", participation.Id);
            throw new ParticipationRepositoryException("Failed to update participation", ex);
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        try
        {
            var participationDb = await _context.Participations.FirstOrDefaultAsync(p => p.Id == id);

            if (participationDb is null)
            {
                _logger.LogWarning("Participation with Id {ParticipationId} not found for removal", id);
                throw new ParticipationRepositoryException($"Participation with Id {id} not found");
            }

            _context.Participations.Remove(participationDb);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing participation with Id {ParticipationId}", id);
            throw new ParticipationRepositoryException("Failed to remove participation", ex);
        }
    }
}
