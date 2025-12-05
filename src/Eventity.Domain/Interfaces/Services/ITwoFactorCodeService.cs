namespace Eventity.Domain.Interfaces.Services;

public interface ITwoFactorCodeService
{
    Task<string> GenerateCodeAsync(Guid userId);
    Task<bool> ValidateCodeAsync(Guid userId, string code);
}