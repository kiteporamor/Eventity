using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class ParticipationRepositoryException : RepositoryException
{
    public ParticipationRepositoryException(string message) : base(message) { }
    public ParticipationRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}