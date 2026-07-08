using Microsoft.AspNetCore.Identity;
using NursingPlatform.Application.Abstractions.Auth;

namespace NursingPlatform.Infrastructure.Authentication;

public sealed class PasswordHashingService : IPasswordHashingService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(null!, hash, password);
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
