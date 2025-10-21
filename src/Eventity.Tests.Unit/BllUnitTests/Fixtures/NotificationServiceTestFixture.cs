using Eventity.Application.Services;
using Eventity.Domain.Models;
using Eventity.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Eventity.Tests.Services;

public class NotificationServiceTestFixture
{
    public Mock<INotificationRepository> NotificationRepoMock { get; }
    public Mock<IParticipationRepository> PartRepoMock { get; }
    public Mock<IUserRepository> UserRepoMock { get; }
    public Mock<IEventRepository> EventRepoMock { get; }
    public Mock<ILogger<NotificationService>> LoggerMock { get; }
    public NotificationService Service { get; }

    public NotificationServiceTestFixture()
    {
        NotificationRepoMock = new Mock<INotificationRepository>();
        PartRepoMock = new Mock<IParticipationRepository>();
        UserRepoMock = new Mock<IUserRepository>();
        EventRepoMock = new Mock<IEventRepository>();
        LoggerMock = new Mock<ILogger<NotificationService>>();
        
        Service = new NotificationService(
            NotificationRepoMock.Object,
            PartRepoMock.Object,
            UserRepoMock.Object,
            EventRepoMock.Object,
            LoggerMock.Object);
    }

    public void ResetMocks()
    {
        NotificationRepoMock.Reset();
        PartRepoMock.Reset();
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
        PartRepoMock.Setup(r => r.GetByIdAsync(participation.Id))
            .ReturnsAsync(participation);
    }

    public void SetupParticipationByEventId(Guid eventId, List<Participation> participations)
    {
        PartRepoMock.Setup(r => r.GetByEventIdAsync(eventId))
            .ReturnsAsync(participations);
    }

    public void SetupUserExists(Guid userId, User user)
    {
        UserRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
    }
    
    public void SetupEventExists(Guid eventId, Event eventItem)
    {
        EventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventItem);
    }
    
    public void SetupParticipationNotFound(Guid participationId)
    {
        PartRepoMock.Setup(r => r.GetByIdAsync(participationId))
            .ReturnsAsync((Participation)null);
    }
    
    public void SetupEventNotFound(Guid eventId)
    {
        EventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event)null);
    }
    
    public void SetupParticipationByEventIdNotFound(Guid eventId)
    {
        PartRepoMock.Setup(r => r.GetByEventIdAsync(eventId))
            .ReturnsAsync((IEnumerable<Participation>)null);
    }
    
    public void SetupNotificationUpdatedSuccessfully()
    {
        NotificationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);
    }
    
    public void SetupParticipationById(Guid participationId, Participation participation)
    {
        PartRepoMock.Setup(r => r.GetByIdAsync(participationId))
            .ReturnsAsync(participation);
    }
    
    public void SetupNotificationByParticipationId(Guid participationId, Notification notification)
    {
        NotificationRepoMock.Setup(r => r.GetByParticipationIdAsync(participationId))
            .ReturnsAsync(notification);
    }
    
    public void SetupNotificationByParticipationIdNotFound(Guid participationId)
    {
        NotificationRepoMock.Setup(r => r.GetByParticipationIdAsync(participationId))
            .ReturnsAsync((Notification)null);
    }
}