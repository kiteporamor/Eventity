using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class EventRepositoryException : RepositoryException
{
    public EventRepositoryException(string message) : base(message) { }
    public EventRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}