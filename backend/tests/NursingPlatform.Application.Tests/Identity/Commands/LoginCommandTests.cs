using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Common;
using NursingPlatform.Application.Identity.Commands.Login;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class LoginCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _roleId = Guid.NewGuid();
    private readonly User _activeUser;
    private readonly User _inactiveUser;
    private readonly List<Role> _roles;
    private readonly List<UserRole> _userRoles;
    private readonly LoginCommand _validCommand;

    public LoginCommandTests()
    {
        _activeUser = new User
        {
            Id = _userId,
            Email = "active@test.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe",
            IsActive = false
        };

        _roles = new List<Role> { new() { Id = _roleId, Name = "Nurse" } };

        _userRoles = new List<UserRole>
        {
            new() { UserId = _userId, RoleId = _roleId, Role = _roles[0] }
        };

        _validCommand = new LoginCommand
        {
            Email = "active@test.com",
            Password = "CorrectPass1"
        };
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResult()
    {
        var users = new List<User> { _activeUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var userRoles = _userRoles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);
        var tokens = new List<RefreshToken>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(tokens.Object);
        var accessTokenResult = new AccessTokenResult { Token = "access-token", ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
        _passwordHasherMock.Setup(p => p.Verify("CorrectPass1", "hash")).Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(_activeUser, new List<string> { "Nurse" })).Returns(accessTokenResult);
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

        var handler = new LoginCommandHandler(
            _contextMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);

        var result = await handler.Handle(_validCommand, default);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(accessTokenResult.ExpiresAt, result.ExpiresAt);
        var expectedHash = ComputeSha256Hash("refresh-token");
        _contextMock.Verify(c => c.RefreshTokens.Add(It.Is<RefreshToken>(rt =>
            rt.TokenHash == expectedHash
            && rt.UserId == _userId
            && rt.RevokedAt == null
            && rt.ExpiresAt > DateTime.UtcNow)));
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        var users = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var handler = new LoginCommandHandler(
            _contextMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);

        var command = new LoginCommand { Email = "nonexistent@test.com", Password = "Pass1" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var users = new List<User> { _inactiveUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var handler = new LoginCommandHandler(
            _contextMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);

        var command = new LoginCommand { Email = "inactive@test.com", Password = "Pass1" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var users = new List<User> { _activeUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        _passwordHasherMock.Setup(p => p.Verify("WrongPass1", "hash")).Returns(false);

        var handler = new LoginCommandHandler(
            _contextMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);

        var command = new LoginCommand { Email = "active@test.com", Password = "WrongPass1" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
    }

    [Fact]
    public void Validator_EmptyEmail_ReturnsError()
    {
        var v = new LoginCommandValidator();
        var c = new LoginCommand { Email = "", Password = "Pass1" };
        Assert.False(v.Validate(c).IsValid);
    }

    [Fact]
    public void Validator_EmptyPassword_ReturnsError()
    {
        var v = new LoginCommandValidator();
        var c = new LoginCommand { Email = "test@test.com", Password = "" };
        Assert.False(v.Validate(c).IsValid);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
