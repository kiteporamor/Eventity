using MailingManager.Core.Exceptions.Repositories;

namespace Eventity.Domain.Exceptions;

public class CalendarServiceException : ServiceException
{
    public CalendarServiceException(string message) : base(message)
    {
    }

    public CalendarServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
