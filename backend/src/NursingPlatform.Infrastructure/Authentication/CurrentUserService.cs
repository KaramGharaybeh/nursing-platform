using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NursingPlatform.Application.Abstractions.Auth;

namespace NursingPlatform.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return null;

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirst("sub");

            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public IReadOnlyList<string> Roles
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role);
            return claims?.Select(c => c.Value).ToList().AsReadOnly() ?? [];
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
