using System.Security.Claims;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Abstractions.Auth;

public interface IJwtService
{
    AccessTokenResult GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
