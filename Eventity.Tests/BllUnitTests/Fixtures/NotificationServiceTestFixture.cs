using Eventity.Application.Services;
using Microsoft.Extensions.Logging;

namespace Eventity.Tests.Services;

public class NotificationServiceTestFixture
{
    public Mock<INotificationRepository> NotificationRepoMock { get; }
    public Mock<IParticipationRepository> ParticipationRepoMock { get; }
    public Mock<IUserRepository> UserRepoMock { get; }
    public Mock<IEventRepository> EventRepoMock { get; }
    public Mock<ILogger<NotificationService>> LoggerMock { get; }
    public NotificationService Service { get; }

    public NotificationServiceTestFixture()
    {
        NotificationRepoMock = new Mock<INotificationRepository>();
        ParticipationRepoMock = new Mock<IParticipationRepository>();
        UserRepoMock = new Mock<IUserRepository>();
        EventRepoMock = new Mock<IEventRepository>();
        LoggerMock = new Mock<ILogger<NotificationService>>();
        
        Service = new NotificationService(
            NotificationRepoMock.Object,
            ParticipationRepoMock.Object,
            UserRepoMock.Object,
            EventRepoMock.Object,
            LoggerMock.Object);
    }

    public void ResetMocks()
    {
        NotificationRepoMock.Reset();
        ParticipationRepoMock.Reset();
        UserRepoMock.Reset();
        EventRepoMock.Reset();
        LoggerMock.Reset();
    }

    public void SetupNotificationExists(Notification notification)
    {
        NotificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);
    }

    public void SetupNotificationNotFound(Guid notificationId)
    {
        NotificationRepoMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification)null);
    }

    public void SetupNotificationsList(List<Notification> notifications)
    {
        NotificationRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(notifications);
    }

    public void SetupParticipationExists(Participation participation)
    {
        ParticipationRepoMock.Setup(r => r.GetByIdAsync(participation.Id))
            .ReturnsAsync(participation);
    }

    public void SetupUserExists(Guid userId, User user)
    {
        UserRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
    }
    
    public void SetupEventExists(Guid eventInfo, Event eventItem)
    {
        EventRepoMock.Setup(r => r.GetByIdAsync(eventItem.Id))
            .ReturnsAsync(eventItem);
    }
    
    public void SetupParticipationNotFound(Guid participationId)
    {
        ParticipationRepoMock.Setup(r => r.GetByIdAsync(participationId))
            .ReturnsAsync((Participation)null);
    }
    
    public void SetupNotificationAddedSuccessfully()
    {
        NotificationRepoMock.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);
    }
}