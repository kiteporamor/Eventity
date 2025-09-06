using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class EventServiceException : ServiceException
{
    public EventServiceException(string message) : base(message) { }
    public EventServiceException(string message, Exception innerException) : base(message, innerException) { }
}