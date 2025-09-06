using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class UserServiceException : ServiceException
{
    public UserServiceException(string message) : base(message) { }
    public UserServiceException(string message, Exception innerException) : base(message, innerException) { }
}