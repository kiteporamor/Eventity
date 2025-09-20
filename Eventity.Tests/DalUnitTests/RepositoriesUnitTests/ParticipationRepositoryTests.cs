using System;
using System.Threading.Tasks;
using Allure.XUnit.Attributes.Steps;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.Fabrics;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventity.DataAccess.Tests.Repositories;

public class ParticipationRepositoryTests : IClassFixture<ParticipationRepositoryFixture>
{
    private readonly ParticipationRepositoryFixture _fixture;

    public ParticipationRepositoryTests(ParticipationRepositoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [AllureStep]
    public async Task AddAsync_ShouldAddParticipation()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var participation = ParticipationFactory.Create(
            status: ParticipationStatusEnum.Accepted
        );

        var result = await repository.AddAsync(participation);
        var savedParticipation = await context.Participations.FirstOrDefaultAsync(
            p => p.Id == participation.Id);

        Assert.NotNull(savedParticipation);
        Assert.Equal(participation.Id, savedParticipation.Id);
    }

    [Fact]
    [AllureStep]
    public async Task GetByIdAsync_ShouldReturnParticipation_WhenExist()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var participation = ParticipationFactory.Create();
        await repository.AddAsync(participation);

        var result = await repository.GetByIdAsync(participation.Id);

        Assert.NotNull(result);
        Assert.Equal(participation.Id, result.Id);
    }

    [Fact]
    [AllureStep]
    public async Task UpdateAsync_ShouldUpdateParticipation()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var participation = ParticipationFactory.Create(
            status: ParticipationStatusEnum.Invited
        );
        await repository.AddAsync(participation);

        var updatedParticipation = ParticipationFactory.Create(
            id: participation.Id,
            userId: participation.UserId,
            eventId: participation.EventId,
            role: participation.Role,
            status: ParticipationStatusEnum.Accepted
        );

        await repository.UpdateAsync(updatedParticipation);
        var savedParticipation = await context.Participations.FirstOrDefaultAsync(p => p.Id == participation.Id);

        Assert.Equal(ParticipationStatusEnum.Accepted, savedParticipation.Status);
    }

    [Fact]
    [AllureStep]
    public async Task RemoveAsync_ShouldRemoveParticipation()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var participation = ParticipationFactory.Create();
        await repository.AddAsync(participation);

        await repository.RemoveAsync(participation.Id);
        var removedParticipation = await context.Participations.FirstOrDefaultAsync(
            p => p.Id == participation.Id);

        Assert.Null(removedParticipation);
    }

    [Fact]
    [AllureStep]
    public async Task AddAsync_ShouldSaveOrganizerParticipation()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var participation = ParticipationFactory.Organizer();

        await repository.AddAsync(participation);
        var savedParticipation = await context.Participations.FirstOrDefaultAsync(
            p => p.Id == participation.Id);

        Assert.NotNull(savedParticipation);
        Assert.Equal(ParticipationRoleEnum.Organizer, savedParticipation.Role);
        Assert.Equal(ParticipationStatusEnum.Accepted, savedParticipation.Status);
    }

    [Fact]
    [AllureStep]
    public async Task AddAsync_ShouldSaveAcceptedParticipant()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var participation = ParticipationFactory.AcceptedParticipant();

        await repository.AddAsync(participation);
        var savedParticipation = await context.Participations.FirstOrDefaultAsync(
            p => p.Id == participation.Id);

        Assert.NotNull(savedParticipation);
        Assert.Equal(ParticipationRoleEnum.Participant, savedParticipation.Role);
        Assert.Equal(ParticipationStatusEnum.Accepted, savedParticipation.Status);
    }

    [Fact]
    [AllureStep]
    public async Task AddAsync_ShouldSaveParticipationWithSpecificIds()
    {
        var context = await _fixture.CreateContextAsync();
        var repository = new ParticipationRepository(context, _fixture.Logger);

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participation = ParticipationFactory.WithSpecificIds(userId, eventId);

        await repository.AddAsync(participation);
        var savedParticipation = await context.Participations.FirstOrDefaultAsync(
            p => p.Id == participation.Id);

        Assert.NotNull(savedParticipation);
        Assert.Equal(userId, savedParticipation.UserId);
        Assert.Equal(eventId, savedParticipation.EventId);
    }
}