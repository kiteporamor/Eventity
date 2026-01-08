using Eventity.Domain.Models;

namespace Eventity.Domain.Interfaces.Services;

public interface ITwoFactorCodeService
{
    Task<string> GenerateCodeAsync(Guid userId);
    Task<TwoFactorValidationResult> ValidateCodeAsync(Guid userId, string code);
}
