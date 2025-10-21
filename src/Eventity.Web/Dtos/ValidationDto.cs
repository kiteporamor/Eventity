namespace Eventity.Web.Dtos;

public class ValidationDto
{
    public Guid CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    public ValidationDto(Guid currentUserId, bool isAdmin)
    {
        CurrentUserId = currentUserId;
        IsAdmin = isAdmin;
    }
}