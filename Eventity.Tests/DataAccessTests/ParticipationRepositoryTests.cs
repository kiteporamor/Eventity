using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Tests.Repositories
{
    public class ParticipationRepositoryTests
    {
        private readonly EventityDbContext _context;
        private readonly ParticipationRepository _repository;
        private readonly ILogger<ParticipationRepository> _logger;

        public ParticipationRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<EventityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EventityDbContext(options);
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<ParticipationRepository>();
            _logger = logger;
            _repository = new ParticipationRepository(_context, _logger);
        }

        [Fact]
        public async Task AddAsync_ShouldAddParticipation()
        {
            var participation = new Participation
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = ParticipationStatusEnum.Accepted
            };

            var result = await _repository.AddAsync(participation);

            var savedParticipation = await _context.Participations.FirstOrDefaultAsync(
                p => p.Id == participation.Id);
            Assert.NotNull(savedParticipation);
            Assert.Equal(participation.Id, savedParticipation.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnParticipation_WhenExist()
        {
            var participation = new Participation
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = ParticipationStatusEnum.Accepted
            };
            await _repository.AddAsync(participation);

            var result = await _repository.GetByIdAsync(participation.Id);

            Assert.NotNull(result);
            Assert.Equal(participation.Id, result.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateParticipation()
        {
            var participation = new Participation
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = ParticipationStatusEnum.Accepted
            };
            await _repository.AddAsync(participation);

            participation.Status = ParticipationStatusEnum.Accepted;
            var updatedParticipation = await _repository.UpdateAsync(participation);

            var savedParticipation = await _context.Participations.FirstOrDefaultAsync(p => p.Id == participation.Id);
            Assert.Equal(ParticipationStatusEnum.Accepted, savedParticipation.Status);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveParticipation()
        {
            var participation = new Participation
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = ParticipationStatusEnum.Accepted
            };
            await _repository.AddAsync(participation);

            await _repository.RemoveAsync(participation.Id);

            var removedParticipation = await _context.Participations.FirstOrDefaultAsync(
                p => p.Id == participation.Id);
            Assert.Null(removedParticipation);
        }
    }
}
