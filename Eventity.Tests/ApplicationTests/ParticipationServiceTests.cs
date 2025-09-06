using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Eventity.UnitTests;

public class ParticipationServiceTests
{
    private readonly Mock<IParticipationRepository> _mockParticipationRepository;
    private readonly ParticipationService _participationService;
    private readonly ILogger<ParticipationService> _logger;

    public ParticipationServiceTests()
    {
        _mockParticipationRepository = new Mock<IParticipationRepository>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ParticipationService>();
        _logger = logger;
        _participationService = new ParticipationService(_mockParticipationRepository.Object, _logger);
    }

    [Fact]
    public async Task AddParticipation_ShouldReturnParticipation_WhenParticipationIsCreated()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var role = ParticipationRoleEnum.Organizer;
        var status = ParticipationStatusEnum.Accepted;

        _mockParticipationRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Participation>()))
            .ReturnsAsync((Participation p) => p);

        var result = await _participationService.AddParticipation(userId, eventId, role, status);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(role, result.Role);
        Assert.Equal(status, result.Status);

        _mockParticipationRepository.Verify(repo => repo.AddAsync(
            It.IsAny<Participation>()), Times.Once);
    }

    [Fact]
    public async Task AddParticipation_ShouldThrowParticipationServiceException_WhenRepositoryFails()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var role = ParticipationRoleEnum.Organizer;
        var status = ParticipationStatusEnum.Accepted;

        _mockParticipationRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Participation>()))
            .ThrowsAsync(new ParticipationRepositoryException("Repository error"));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.AddParticipation(userId, eventId, role, status));
    }

    [Fact]
    public async Task GetParticipationById_ShouldReturnParticipation_WhenParticipationExists()
    {
        var participationId = Guid.NewGuid();
        var participation = new Participation(participationId, Guid.NewGuid(), Guid.NewGuid(), 
            ParticipationRoleEnum.Organizer, ParticipationStatusEnum.Accepted);

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(participation);

        var result = await _participationService.GetParticipationById(participationId);

        Assert.NotNull(result);
        Assert.Equal(participationId, result.Id);
    }

    [Fact]
    public async Task GetParticipationById_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(default(Participation));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetParticipationById(participationId));
    }

    [Fact]
    public async Task GetParticipationsByEventId_ShouldReturnParticipations_WhenParticipationsExist()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted),
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Organizer, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _participationService.GetParticipationsByEventId(eventId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetParticipationsByEventId_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        var eventId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetParticipationsByEventId(eventId));
    }

    [Fact]
    public async Task GetParticipationsByUserId_ShouldReturnParticipations_WhenParticipationsExist()
    {
        var userId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), userId, Guid.NewGuid(), ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted),
            new Participation(Guid.NewGuid(), userId, Guid.NewGuid(), ParticipationRoleEnum.Organizer, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(participations);

        var result = await _participationService.GetParticipationsByUserId(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetParticipationsByUserId_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        var userId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetParticipationsByUserId(userId));
    }

    [Fact]
    public async Task GetOrganizerByEventId_ShouldReturnOrganizer_WhenOrganizerExists()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted),
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Organizer, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _participationService.GetOrganizerByEventId(eventId);

        Assert.NotNull(result);
        Assert.Equal(ParticipationRoleEnum.Organizer, result.Role);
    }

    [Fact]
    public async Task GetOrganizerByEventId_ShouldThrowParticipationServiceException_WhenNoOrganizerFound()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetOrganizerByEventId(eventId));
    }

    [Fact]
    public async Task GetAllParticipantsByEventId_ShouldReturnParticipants_WhenParticipantsExist()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted),
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Organizer, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _participationService.GetAllParticipantsByEventId(eventId);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllParticipantsByEventId_ShouldThrowParticipationServiceException_WhenNoParticipantsFound()
    {
        var eventId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetAllParticipantsByEventId(eventId));
    }

    [Fact]
    public async Task GetAllLeftParticipantsByEventId_ShouldReturnLeftParticipants_WhenLeftParticipantsExist()
    {
        var eventId = Guid.NewGuid();
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Left, 
                ParticipationStatusEnum.Accepted),
            new Participation(Guid.NewGuid(), Guid.NewGuid(), eventId, ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);

        var result = await _participationService.GetAllLeftParticipantsByEventId(eventId);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllLeftParticipantsByEventId_ShouldThrowParticipationServiceException_WhenNoLeftParticipantsFound()
    {
        var eventId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByEventIdAsync(eventId))
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetAllLeftParticipantsByEventId(eventId));
    }

    [Fact]
    public async Task GetAllParticipations_ShouldReturnParticipations_WhenParticipationsExist()
    {
        var participations = new List<Participation>
        {
            new Participation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ParticipationRoleEnum.Participant, 
                ParticipationStatusEnum.Accepted),
            new Participation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ParticipationRoleEnum.Organizer, 
                ParticipationStatusEnum.Accepted)
        };

        _mockParticipationRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(participations);

        var result = await _participationService.GetAllParticipations();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllParticipations_ShouldThrowParticipationServiceException_WhenNoParticipationsFound()
    {
        _mockParticipationRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<Participation>());

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.GetAllParticipations());
    }

    [Fact]
    public async Task UpdateParticipation_ShouldReturnUpdatedParticipation_WhenParticipationExists()
    {
        var participationId = Guid.NewGuid();
        var participation = new Participation(participationId, Guid.NewGuid(), Guid.NewGuid(), 
            ParticipationRoleEnum.Participant, ParticipationStatusEnum.Accepted);
        var updatedStatus = ParticipationStatusEnum.Rejected;

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(participation);

        _mockParticipationRepository
            .Setup(repo => repo.UpdateAsync(participation))
            .ReturnsAsync(participation);

        var result = await _participationService.UpdateParticipation(participationId, updatedStatus);

        Assert.NotNull(result);
        Assert.Equal(updatedStatus, result.Status);
    }

    [Fact]
    public async Task UpdateParticipation_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(default(Participation));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.UpdateParticipation(participationId, ParticipationStatusEnum.Rejected));
    }

    [Fact]
    public async Task ChangeParticipationStatus_ShouldReturnUpdatedParticipation_WhenParticipationExists()
    {
        var participationId = Guid.NewGuid();
        var participation = new Participation(participationId, Guid.NewGuid(), Guid.NewGuid(), 
            ParticipationRoleEnum.Participant, ParticipationStatusEnum.Accepted);
        var updatedStatus = ParticipationStatusEnum.Invited;

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(participation);

        _mockParticipationRepository
            .Setup(repo => repo.UpdateAsync(participation))
            .ReturnsAsync(participation);

        var result = await _participationService.ChangeParticipationStatus(participationId, updatedStatus);

        Assert.NotNull(result);
        Assert.Equal(updatedStatus, result.Status);
    }

    [Fact]
    public async Task ChangeParticipationStatus_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(default(Participation));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.ChangeParticipationStatus(participationId, ParticipationStatusEnum.Invited));
    }

    [Fact]
    public async Task ChangeParticipationRole_ShouldReturnUpdatedParticipation_WhenParticipationExists()
    {
        var participationId = Guid.NewGuid();
        var participation = new Participation(participationId, Guid.NewGuid(), Guid.NewGuid(), 
            ParticipationRoleEnum.Participant, ParticipationStatusEnum.Accepted);
        var updatedRole = ParticipationRoleEnum.Organizer;

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(participation);

        _mockParticipationRepository
            .Setup(repo => repo.UpdateAsync(participation))
            .ReturnsAsync(participation);

        var result = await _participationService.ChangeParticipationRole(participationId, updatedRole);

        Assert.NotNull(result);
        Assert.Equal(updatedRole, result.Role);
    }

    [Fact]
    public async Task ChangeParticipationRole_ShouldThrowParticipationServiceException_WhenParticipationNotFound()
    {
        var participationId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.GetByIdAsync(participationId))
            .ReturnsAsync(default(Participation));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.ChangeParticipationRole(participationId, ParticipationRoleEnum.Organizer));
    }

    [Fact]
    public async Task RemoveParticipation_ShouldCallRemoveAsync_WhenParticipationExists()
    {
        var participationId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.RemoveAsync(participationId))
            .Returns(Task.CompletedTask);

        await _participationService.RemoveParticipation(participationId);

        _mockParticipationRepository.Verify(repo => repo.RemoveAsync(participationId), Times.Once);
    }

    [Fact]
    public async Task RemoveParticipation_ShouldThrowParticipationServiceException_WhenRepositoryFails()
    {
        var participationId = Guid.NewGuid();

        _mockParticipationRepository
            .Setup(repo => repo.RemoveAsync(participationId))
            .ThrowsAsync(new ParticipationRepositoryException("Repository error"));

        await Assert.ThrowsAsync<ParticipationServiceException>(() =>
            _participationService.RemoveParticipation(participationId));
    }
}