namespace Eventity.Domain.Models;

public class ChangePasswordResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime ChangedAt { get; set; }
}