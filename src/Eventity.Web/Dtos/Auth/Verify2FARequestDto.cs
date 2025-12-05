namespace Eventity.Web.Dtos;

public class Verify2FARequestDto
{
    public Guid UserId { get; set; }
    public string Code { get; set; }
}