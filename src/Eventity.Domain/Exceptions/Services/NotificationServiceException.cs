using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class NotificationServiceException : ServiceException
{
    public NotificationServiceException(string message) : base(message) { }
    public NotificationServiceException(string message, Exception innerException) : base(message, innerException) { }
}