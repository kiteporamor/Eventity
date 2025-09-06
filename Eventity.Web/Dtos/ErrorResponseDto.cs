namespace Eventity.Web.Dtos;

public class ErrorResponseDto
{
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Детали ошибки
    /// </summary>
    public string Details { get; set; }
}
