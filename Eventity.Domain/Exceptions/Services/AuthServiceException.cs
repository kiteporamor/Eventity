using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class AuthServiceException : ServiceException
{
    public AuthServiceException(string message) : base(message) { }
    public AuthServiceException(string message, Exception innerException) : base(message, innerException) { }
}

public class UserNotFoundException : AuthServiceException
{
    public UserNotFoundException(string message) : base(message) { }
}

public class InvalidPasswordException : AuthServiceException
{
    public InvalidPasswordException(string message) : base(message) { }
}

public class UserAlreadyExistsException : AuthServiceException
{
    public UserAlreadyExistsException(string message) : base(message) { }
}