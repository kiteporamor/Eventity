namespace Eventity.Web.Dtos;

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}