using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Common;
using NursingPlatform.Application.Identity.Commands.RotateRefreshToken;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class RotateRefreshTokenCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _roleId = Guid.NewGuid();
    private readonly string _rawToken = "valid-raw-token";
    private readonly string _tokenHash;
    private readonly RefreshToken _validStoredToken;
    private readonly RefreshToken _expiredToken;
    private readonly RefreshToken _revokedToken;
    private readonly User _user;
    private readonly List<Role> _roles;
    private readonly List<UserRole> _userRoles;

    public RotateRefreshTokenCommandTests()
    {
        _tokenHash = ComputeSha256Hash(_rawToken);

        _user = new User
        {
            Id = _userId,
            Email = "test@test.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _roles = new List<Role> { new() { Id = _roleId, Name = "Nurse" } };

        _userRoles = new List<UserRole>
        {
            new() { UserId = _userId, RoleId = _roleId, Role = _roles[0] }
        };

        _validStoredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = _tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        _expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = ComputeSha256Hash("expired-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            RevokedAt = null
        };

        _revokedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = ComputeSha256Hash("revoked-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewAuthResult()
    {
        var tokens = new List<RefreshToken> { _validStoredToken }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(tokens.Object);
        var users = new List<User> { _user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var userRoles = _userRoles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        var accessTokenResult = new AccessTokenResult { Token = "new-access-token", ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(_user, new List<string> { "Nurse" })).Returns(accessTokenResult);
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");

        var handler = new RotateRefreshTokenCommandHandler(
            _contextMock.Object, _jwtServiceMock.Object);

        var result = await handler.Handle(
            new RotateRefreshTokenCommand { RefreshToken = _rawToken }, default);

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal(accessTokenResult.ExpiresAt, result.ExpiresAt);
        Assert.NotNull(_validStoredToken.RevokedAt);
        var expectedNewHash = ComputeSha256Hash("new-refresh-token");
        _contextMock.Verify(c => c.RefreshTokens.Add(It.Is<RefreshToken>(rt =>
            rt.TokenHash == expectedNewHash
            && rt.UserId == _userId
            && rt.RevokedAt == null
            && rt.ExpiresAt > DateTime.UtcNow)));
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        var tokens = new List<RefreshToken> { _expiredToken }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(tokens.Object);

        var handler = new RotateRefreshTokenCommandHandler(
            _contextMock.Object, _jwtServiceMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RotateRefreshTokenCommand { RefreshToken = "expired-token" }, default));
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorizedAccessException()
    {
        var tokens = new List<RefreshToken> { _revokedToken }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(tokens.Object);

        var handler = new RotateRefreshTokenCommandHandler(
            _contextMock.Object, _jwtServiceMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RotateRefreshTokenCommand { RefreshToken = "revoked-token" }, default));
    }

    [Fact]
    public async Task Handle_NonexistentToken_ThrowsUnauthorizedAccessException()
    {
        var tokens = new List<RefreshToken>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(tokens.Object);

        var handler = new RotateRefreshTokenCommandHandler(
            _contextMock.Object, _jwtServiceMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RotateRefreshTokenCommand { RefreshToken = "unknown-token" }, default));
    }

    [Fact]
    public async Task Handle_ReusedRevokedToken_RevokesAllUserTokens()
    {
        var otherToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TokenHash = ComputeSha256Hash("other-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        var tokens = new List<RefreshToken> { _revokedToken, otherToken }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(tokens.Object);

        var handler = new RotateRefreshTokenCommandHandler(
            _contextMock.Object, _jwtServiceMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RotateRefreshTokenCommand { RefreshToken = "revoked-token" }, default));

        Assert.NotNull(otherToken.RevokedAt);
    }

    [Fact]
    public void Validator_EmptyRefreshToken_ReturnsError()
    {
        var v = new RotateRefreshTokenCommandValidator();
        var c = new RotateRefreshTokenCommand { RefreshToken = "" };
        Assert.False(v.Validate(c).IsValid);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
