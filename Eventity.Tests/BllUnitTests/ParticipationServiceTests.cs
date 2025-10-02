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
using Allure.Xunit;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Allure.XUnit.Attributes.Steps;
using Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
    public async Task GetUserParticipationInfoByUserId_ShouldReturnUserParticipationInfos_WhenValidAcceptedParticipations()
    {
        var userId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var event1Id = Guid.NewGuid();
        var event2Id = Guid.NewGuid();

        var participations = new List<Participation>
        {
            ParticipationFactory.Create(userId: userId, eventId: event1Id, status: ParticipationStatusEnum.Accepted),
            ParticipationFactory.Create(userId: userId, eventId: event2Id, status: ParticipationStatusEnum.Accepted),
            ParticipationFactory.Create(userId: userId, eventId: Guid.NewGuid(), status: ParticipationStatusEnum.Invited) 
        };

        var event1 = new EventBuilder().WithId(event1Id).WithTitle("Event 1").Build();
        event1.OrganizerId = organizerId; 
        
        var event2 = new EventBuilder().WithId(event2Id).WithTitle("Event 2").Build();
        event2.OrganizerId = organizerId;
        
        var organizer = UserFactory.CreateUser(id: organizerId, login: "organizer1");

        _fixture.ParticipationRepoMock.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(participations);
        _fixture.EventRepoMoch.Setup(r => r.GetByIdAsync(event1Id))
            .ReturnsAsync(event1);
        _fixture.EventRepoMoch.Setup(r => r.GetByIdAsync(event2Id))
            .ReturnsAsync(event2);
        _fixture.UserRepoMoch.Setup(r => r.GetByIdAsync(organizerId))
            .ReturnsAsync(organizer);

        var result = await _fixture.Service.GetUserParticipationInfoByUserId(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); 
        Assert.Contains(result, x => x.EventItem.Id == event1Id); 
        Assert.Contains(result, x => x.EventItem.Id == event2Id); 
        Assert.All(result, x => Assert.Equal(organizerId, x.OrganizerId));
        Assert.All(result, x => Assert.Equal(organizer.Login, x.OrganizerLogin));
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task GetUserParticipationInfoByUserId_ShouldThrow_WhenNoAcceptedParticipations()
    {
        var userId = Guid.NewGuid();

        var participations = new List<Participation>
        {
            ParticipationFactory.Create(userId: userId, status: ParticipationStatusEnum.Invited),
            ParticipationFactory.Create(userId: userId, status: ParticipationStatusEnum.Rejected)
        };

        _fixture.ParticipationRepoMock.Setup(r => r.GetByUserIdAsync(userId))
                                    .ReturnsAsync(participations);

        var exception = await Assert.ThrowsAsync<ParticipationServiceException>(() => 
            _fixture.Service.GetUserParticipationInfoByUserId(userId));
        
        Assert.Equal("Failed to find participations by user id.", exception.Message);
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task GetUserParticipationInfoByUserId_ShouldThrow_WhenNoParticipationsFound()
    {
        var userId = Guid.NewGuid();

        _fixture.ParticipationRepoMock.Setup(r => r.GetByUserIdAsync(userId))
                                    .ReturnsAsync(new List<Participation>());

        var exception = await Assert.ThrowsAsync<ParticipationServiceException>(() => 
            _fixture.Service.GetUserParticipationInfoByUserId(userId));
        
        Assert.Equal("Failed to find participations by user id.", exception.Message);
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
    public async Task GetParticipationById_ShouldReturnParticipation_WhenParticipationExists()
    {
        var participation = ParticipationFactory.Create();

        _fixture.SetupParticipationExists(participation);

        var result = await _fixture.Service.GetParticipationById(participation.Id);

        Assert.NotNull(result);
        Assert.Equal(participation.Id, result.Id);
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task GetParticipationById_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetParticipationById(participationId));
    }

    [Fact]
    [AllureSuite("ParticipationServicSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task GetAllParticipations_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        _fixture.SetupParticipationsList(new List<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.GetAllParticipations());
    }

    [Fact]
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task UpdateParticipation_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.UpdateParticipation(participationId, ParticipationStatusEnum.Rejected));
    }

    [Fact]
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task ChangeParticipationStatus_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.ChangeParticipationStatus(participationId, ParticipationStatusEnum.Invited));
    }

    [Fact]
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task ChangeParticipationRole_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.ChangeParticipationRole(participationId, ParticipationRoleEnum.Organizer));
    }

    [Fact]
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
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
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
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