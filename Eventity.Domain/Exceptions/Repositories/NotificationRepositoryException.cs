using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class NotificationRepositoryException : RepositoryException
{
    public NotificationRepositoryException(string message) : base(message) { }
    public NotificationRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}