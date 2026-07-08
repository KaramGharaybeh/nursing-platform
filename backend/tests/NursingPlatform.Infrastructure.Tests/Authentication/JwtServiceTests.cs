using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Infrastructure.Authentication;
using NursingPlatform.Infrastructure.Configuration;

namespace NursingPlatform.Infrastructure.Tests.Authentication;

public class JwtServiceTests
{
    private readonly JwtSettings _settings = new()
    {
        Secret = "this-is-a-very-long-secret-key-that-is-at-least-32-chars",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpirationInMinutes = 60,
        RefreshTokenExpirationInDays = 7
    };

    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        Email = "test@test.com",
        FirstName = "John",
        LastName = "Doe",
        IsActive = true
    };

    private readonly IList<string> _roles = new[] { "Nurse", "Admin" };
    private readonly IJwtService _sut;

    public JwtServiceTests()
    {
        _sut = new JwtService(Options.Create(_settings));
    }

    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaims()
    {
        var result = _sut.GenerateAccessToken(_user, _roles);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Equal(_user.Id.ToString(), token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(_user.Email, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(_user.FirstName, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.GivenName).Value);
        Assert.Equal(_user.LastName, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.FamilyName).Value);
        Assert.Equal($"{_user.FirstName} {_user.LastName}", token.Claims.First(c => c.Type == "name").Value);
        Assert.Contains("Nurse", token.Claims.Where(c => c.Type == "role").Select(c => c.Value));
        Assert.Contains("Admin", token.Claims.Where(c => c.Type == "role").Select(c => c.Value));
        Assert.NotNull(token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
        Assert.NotNull(token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat));
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAtMatchesSettings()
    {
        var result = _sut.GenerateAccessToken(_user, _roles);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var expectedExpiration = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes);
        Assert.Equal(expectedExpiration.Minute, result.ExpiresAt.Minute);
        Assert.Equal(expectedExpiration.Hour, result.ExpiresAt.Hour);
        Assert.Equal(expectedExpiration.Day, result.ExpiresAt.Day);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAtPropertyMatchesTokenExpiration()
    {
        var result = _sut.GenerateAccessToken(_user, _roles);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var tokenExp = token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value;
        var expUnix = long.Parse(tokenExp);
        var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

        var diff = (result.ExpiresAt - expDateTime).Duration();
        Assert.True(diff.TotalSeconds <= 1,
            $"ExpiresAt ({result.ExpiresAt:O}) differs from token exp ({expDateTime:O}) by {diff.TotalSeconds}s");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateRefreshToken_MultipleCalls_ReturnsUniqueValues()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ValidExpiredToken_ReturnsPrincipal()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var expiredToken = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, _user.Id.ToString())],
            notBefore: DateTime.UtcNow.AddMinutes(-2),
            expires: DateTime.UtcNow.AddMinutes(-1),
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        var tokenString = handler.WriteToken(expiredToken);

        var principal = _sut.GetPrincipalFromExpiredToken(tokenString);

        Assert.NotNull(principal);
        Assert.Equal(_user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_InvalidToken_ReturnsNull()
    {
        var principal = _sut.GetPrincipalFromExpiredToken("invalid-token");

        Assert.Null(principal);
    }
}
