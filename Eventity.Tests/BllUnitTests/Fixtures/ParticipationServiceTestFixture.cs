using Eventity.Application.Services;
using Microsoft.Extensions.Logging;

namespace Eventity.Tests.Services;

public class ParticipationServiceTestFixture
{
    public Mock<IParticipationRepository> ParticipationRepoMock { get; }
    public Mock<ILogger<ParticipationService>> LoggerMock { get; }
    public ParticipationService Service { get; }

    public ParticipationServiceTestFixture()
    {
        ParticipationRepoMock = new Mock<IParticipationRepository>();
        LoggerMock = new Mock<ILogger<ParticipationService>>();
        
        Service = new ParticipationService(
            ParticipationRepoMock.Object,
            LoggerMock.Object);
    }

    public void ResetMocks()
    {
        ParticipationRepoMock.Reset();
        LoggerMock.Reset();
    }

    public void SetupParticipationExists(Participation participation)
    {
        ParticipationRepoMock.Setup(r => r.GetByIdAsync(participation.Id))
            .ReturnsAsync(participation);
    }

    public void SetupParticipationNotFound(Guid participationId)
    {
        ParticipationRepoMock.Setup(r => r.GetByIdAsync(participationId))
            .ReturnsAsync((Participation)null);
    }

    public void SetupParticipationsList(List<Participation> participations)
    {
        ParticipationRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(participations);
    }
}