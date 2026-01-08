namespace Eventity.Domain.Models;

public class TwoFactorValidationResult
{
    public bool IsValid { get; init; }
    public bool IsLockedOut { get; init; }
    public int RemainingAttempts { get; init; }
    public DateTime? LockoutUntil { get; init; }
}
