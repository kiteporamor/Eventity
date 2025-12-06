namespace Eventity.Web.Dtos;

public class ChangePasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime ChangedAt { get; set; }
}