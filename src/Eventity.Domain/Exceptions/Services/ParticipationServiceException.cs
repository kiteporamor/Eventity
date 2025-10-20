using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class ParticipationServiceException : ServiceException
{
    public ParticipationServiceException(string message) : base(message) { }
    public ParticipationServiceException(string message, Exception innerException) : base(message, innerException) { }
}