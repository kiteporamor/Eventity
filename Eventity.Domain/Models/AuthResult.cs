namespace Eventity.Domain.Models;

public class AuthResult
{
    public User User { get; set; }
    public string Token { get; set; }
}