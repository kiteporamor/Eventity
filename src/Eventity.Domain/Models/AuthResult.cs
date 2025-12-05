namespace Eventity.Domain.Models;

public class AuthResult
{
    public User User { get; set; }
    public string Token { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public bool Requires2FA { get; set; } 
    public Guid? TwoFactorUserId { get; set; }
}

public class Init2FAResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
    public DateTime CodeExpiresAt { get; set; }
}

public class Verify2FAResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public AuthResult? AuthResult { get; set; }
}