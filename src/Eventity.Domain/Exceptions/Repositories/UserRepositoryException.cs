using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class UserRepositoryException : RepositoryException
{
    public UserRepositoryException(string message) : base(message) { }
    public UserRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}
