namespace Eventity.Domain.Models;

public class Validation
{
    public Validation()
    {
    }

    public Validation(Guid currentUserId, bool isAdmin)
    {
        CurrentUserId = currentUserId;
        IsAdmin = isAdmin;
    }
    public Guid CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }
}
