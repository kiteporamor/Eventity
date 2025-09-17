using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Models;
using Eventity.UnitTests.DalUnitTests.Fabrics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eventity.Tests.Services;

public class ParticipationServiceTests : IClassFixture<ParticipationServiceTestFixture>
{
    private readonly ParticipationServiceTestFixture _fixture;

    public ParticipationServiceTests(ParticipationServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task AddParticipation_ShouldReturnParticipation_WhenParticipationIsCreated()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var role = ParticipationRoleEnum.Organizer;
        var status = ParticipationStatusEnum.Accepted;

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<Participation>()))
            .ReturnsAsync((Participation p) => p);

        var result = await _fixture.Service.AddParticipation(userId, eventId, role, status);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(role, result.Role);
        Assert.Equal(status, result.Status);

        _fixture.ParticipationRepoMock.Verify(repo => repo.AddAsync(
            It.IsAny<Participation>()), Times.Once);
    }

    [Fact]
    public async Task AddParticipation_ShouldThrowParticipationServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var role = ParticipationRoleEnum.Organizer;
        var status = ParticipationStatusEnum.Accepted;

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<Participation>()))
            .ThrowsAsync(new ParticipationRepositoryException("Repository error"));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.AddParticipation(userId, eventId, role, status));
    }

    [Fact]
    public async Task GetParticipationById_ShouldReturnParticipation_WhenParticipationExists()
    {
        var participation = ParticipationFactory.Create();

        _fixture.SetupParticipationExists(participation);

        var result = await _fixture.Service.GetParticipationById(participation.Id);

        Assert.NotNull(result);
        Assert.Equal(participation.Id, result.Id);
    }

    [Fact]
    public async Task GetParticipationById_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetParticipationById(participationId));
    }

    [Fact]
    public async Task GetParticipationsByEventId_ShouldReturnParticipations_WhenParticipationsExist()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            ParticipationFactory.WithSpecificIds(Guid.NewGuid(), eventId),
            ParticipationFactory.WithSpecificIds(Guid.NewGuid(), eventId)
        };

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _fixture.Service.GetParticipationsByEventId(eventId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetParticipationsByEventId_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        var eventId = Guid.NewGuid();

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetParticipationsByEventId(eventId));
    }

    [Fact]
    public async Task GetParticipationsByUserId_ShouldReturnParticipations_WhenParticipationsExist()
    {
        var userId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            ParticipationFactory.WithSpecificIds(userId, Guid.NewGuid()),
            ParticipationFactory.WithSpecificIds(userId, Guid.NewGuid())
        };

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(participations);

        var result = await _fixture.Service.GetParticipationsByUserId(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetParticipationsByUserId_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        var userId = Guid.NewGuid();

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetParticipationsByUserId(userId));
    }

    [Fact]
    public async Task GetOrganizerByEventId_ShouldReturnOrganizer_WhenOrganizerExists()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            ParticipationFactory.AcceptedParticipant(),
            ParticipationFactory.Organizer()
        };

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _fixture.Service.GetOrganizerByEventId(eventId);

        Assert.NotNull(result);
        Assert.Equal(ParticipationRoleEnum.Organizer, result.Role);
    }

    [Fact]
    public async Task GetOrganizerByEventId_ShouldThrowParticipationServiceException_WhenNoOrganizerFound()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            ParticipationFactory.AcceptedParticipant()
        };

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetOrganizerByEventId(eventId));
    }

    [Fact]
    public async Task GetAllParticipantsByEventId_ShouldReturnParticipants_WhenParticipantsExist()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            ParticipationFactory.AcceptedParticipant(),
            ParticipationFactory.Organizer()
        };

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _fixture.Service.GetAllParticipantsByEventId(eventId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, p => Assert.Equal(ParticipationRoleEnum.Participant, p.Role));
    }

    [Fact]
    public async Task GetAllParticipantsByEventId_ShouldThrowParticipationServiceException_WhenNoParticipantsFound()
    {
        var eventId = Guid.NewGuid();

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetAllParticipantsByEventId(eventId));
    }

    [Fact]
    public async Task GetAllLeftParticipantsByEventId_ShouldReturnLeftParticipants_WhenLeftParticipantsExist()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            ParticipationFactory.Create(role: ParticipationRoleEnum.Left),
            ParticipationFactory.AcceptedParticipant()
        };

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _fixture.Service.GetAllLeftParticipantsByEventId(eventId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, p => Assert.Equal(ParticipationRoleEnum.Left, p.Role));
    }

    [Fact]
    public async Task GetAllLeftParticipantsByEventId_ShouldThrowParticipationServiceException_WhenNoLeftParticipantsFound()
    {
        var eventId = Guid.NewGuid();

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetAllLeftParticipantsByEventId(eventId));
    }

    [Fact]
    public async Task GetAllParticipations_ShouldReturnParticipations_WhenParticipationsExist()
    {
        var participations = new List<Participation>
        {
            ParticipationFactory.Create(),
            ParticipationFactory.Create()
        };

        _fixture.SetupParticipationsList(participations);

        var result = await _fixture.Service.GetAllParticipations();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllParticipations_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        _fixture.SetupParticipationsList(new List<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetAllParticipations());
    }

    [Fact]
    public async Task UpdateParticipation_ShouldReturnUpdatedParticipation_WhenParticipationExists()
    {
        var participation = ParticipationFactory.Create();
        var updatedStatus = ParticipationStatusEnum.Rejected;

        _fixture.SetupParticipationExists(participation);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Participation>()))
            .ReturnsAsync((Participation p) => p);

        var result = await _fixture.Service.UpdateParticipation(participation.Id, updatedStatus);

        Assert.NotNull(result);
        Assert.Equal(updatedStatus, result.Status);
    }

    [Fact]
    public async Task UpdateParticipation_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.UpdateParticipation(participationId, ParticipationStatusEnum.Rejected));
    }

    [Fact]
    public async Task ChangeParticipationStatus_ShouldReturnUpdatedParticipation_WhenParticipationExists()
    {
        var participation = ParticipationFactory.Create();
        var updatedStatus = ParticipationStatusEnum.Invited;

        _fixture.SetupParticipationExists(participation);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Participation>()))
            .ReturnsAsync((Participation p) => p);

        var result = await _fixture.Service.ChangeParticipationStatus(participation.Id, updatedStatus);

        Assert.NotNull(result);
        Assert.Equal(updatedStatus, result.Status);
    }

    [Fact]
    public async Task ChangeParticipationStatus_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.ChangeParticipationStatus(participationId, ParticipationStatusEnum.Invited));
    }

    [Fact]
    public async Task ChangeParticipationRole_ShouldReturnUpdatedParticipation_WhenParticipationExists()
    {
        var participation = ParticipationFactory.Create();
        var updatedRole = ParticipationRoleEnum.Organizer;

        _fixture.SetupParticipationExists(participation);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Participation>()))
            .ReturnsAsync((Participation p) => p);

        var result = await _fixture.Service.ChangeParticipationRole(participation.Id, updatedRole);

        Assert.NotNull(result);
        Assert.Equal(updatedRole, result.Role);
    }

    [Fact]
    public async Task ChangeParticipationRole_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.ChangeParticipationRole(participationId, ParticipationRoleEnum.Organizer));
    }

    [Fact]
    public async Task RemoveParticipation_ShouldCallRemoveAsync_WhenParticipationExists()
    {
        var participationId = Guid.NewGuid();

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.RemoveAsync(participationId))
            .Returns(Task.CompletedTask);

        await _fixture.Service.RemoveParticipation(participationId);

        _fixture.ParticipationRepoMock.Verify(repo => repo.RemoveAsync(participationId), Times.Once);
    }

    [Fact]
    public async Task RemoveParticipation_ShouldThrowParticipationServiceException_WhenRepositoryFails()
    {
        var participationId = Guid.NewGuid();

        _fixture.ParticipationRepoMock
            .Setup(repo => repo.RemoveAsync(participationId))
            .ThrowsAsync(new ParticipationRepositoryException("Repository error"));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.RemoveParticipation(participationId));
    }
}