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
using Eventity.Domain.Interfaces;

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

        var event1 = new EventBuilder().WithId(event1Id).WithTitle("Event 1").WithOrganizerId(organizerId).Build();
        var event2 = new EventBuilder().WithId(event2Id).WithTitle("Event 2").WithOrganizerId(organizerId).Build();
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
        Assert.All(result, x => Assert.Equal(organizer.Login, x.OrganizerLogin));
    }

    [Fact]
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
    public async Task GetUserParticipationInfoByUserId_ShouldReturnEmpty_WhenNoAcceptedParticipations()
    {
        var userId = Guid.NewGuid();

        var participations = new List<Participation>
        {
            ParticipationFactory.Create(userId: userId, status: ParticipationStatusEnum.Invited),
            ParticipationFactory.Create(userId: userId, status: ParticipationStatusEnum.Rejected)
        };

        _fixture.ParticipationRepoMock.Setup(r => r.GetByUserIdAsync(userId))
                                    .ReturnsAsync(participations);

        var result = await _fixture.Service.GetUserParticipationInfoByUserId(userId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [AllureSuite("ParticipationServiceSuccess")]
    [AllureStep]
    public async Task GetUserParticipationInfoByUserId_ShouldReturnEmpty_WhenNoParticipationsFound()
    {
        var userId = Guid.NewGuid();

        _fixture.ParticipationRepoMock.Setup(r => r.GetByUserIdAsync(userId))
                                    .ReturnsAsync(new List<Participation>());

        var result = await _fixture.Service.GetUserParticipationInfoByUserId(userId);

        Assert.NotNull(result);
        Assert.Empty(result);
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
        var validation = new Validation(userId, false);

        var eventDb = new EventBuilder().WithId(eventId).WithOrganizerId(userId).Build();
        _fixture.EventRepoMoch.Setup(r => r.GetByIdAsync(eventId))
                             .ReturnsAsync(eventDb);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<Participation>()))
            .ThrowsAsync(new Exception("Repository error"));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.AddParticipation(userId, eventId, role, status, validation));
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
    [AllureSuite("ParticipationServiceSuccess")]
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
        var validation = new Validation(participation.UserId, false);

        _fixture.SetupParticipationExists(participation);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Participation>()))
            .ReturnsAsync((Participation p) => p);

        var result = await _fixture.Service.UpdateParticipation(participation.Id, updatedStatus, validation);

        Assert.NotNull(result);
        Assert.Equal(updatedStatus, result.Status);
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task UpdateParticipation_ShouldThrow_WhenAccessDenied()
    {
        var participation = ParticipationFactory.Create();
        var updatedStatus = ParticipationStatusEnum.Rejected;
        var validation = new Validation(Guid.NewGuid(), false);

        _fixture.SetupParticipationExists(participation);

        await Assert.ThrowsAsync<ParticipationRepositoryException>(() =>
            _fixture.Service.UpdateParticipation(participation.Id, updatedStatus, validation));
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task UpdateParticipation_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();
        var validation = new Validation(Guid.NewGuid(), false);
        _fixture.SetupParticipationNotFound(participationId);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.UpdateParticipation(participationId, ParticipationStatusEnum.Rejected, validation));
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
        var participation = ParticipationFactory.Create(id: participationId);
        var eventDomain = new EventBuilder().WithOrganizerId(participation.UserId).Build();
        var validation = new Validation(participation.UserId, false);

        _fixture.SetupParticipationExists(participation);
        _fixture.EventRepoMoch.Setup(r => r.GetByIdAsync(participation.EventId))
                             .ReturnsAsync(eventDomain);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.RemoveAsync(participationId))
            .Returns(Task.CompletedTask);

        await _fixture.Service.RemoveParticipation(participationId, validation);

        _fixture.ParticipationRepoMock.Verify(repo => repo.RemoveAsync(participationId), Times.Once);
    }

    [Fact]
    [AllureSuite("ParticipationServiceError")]
    [AllureStep]
    public async Task RemoveParticipation_ShouldThrowParticipationServiceException_WhenRepositoryFails()
    {
        var participationId = Guid.NewGuid();
        var participation = ParticipationFactory.Create(id: participationId);
        var eventDomain = new EventBuilder().WithOrganizerId(participation.UserId).Build();
        var validation = new Validation(participation.UserId, false);

        _fixture.SetupParticipationExists(participation);
        _fixture.EventRepoMoch.Setup(r => r.GetByIdAsync(participation.EventId))
                             .ReturnsAsync(eventDomain);
        _fixture.ParticipationRepoMock
            .Setup(repo => repo.RemoveAsync(participationId))
            .ThrowsAsync(new Exception("Repository error"));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _fixture.Service.RemoveParticipation(participationId, validation));
    }
}