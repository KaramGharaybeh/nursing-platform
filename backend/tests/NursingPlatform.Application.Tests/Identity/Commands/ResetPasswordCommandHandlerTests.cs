using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Commands.ResetPassword;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();

    [Fact]
    public async Task Handle_ValidTokenForSubmittedEmail_UpdatesPasswordMarksTokenUsedAndRevokesRefreshTokens()
    {
        var rawToken = "valid-token";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "active@test.com",
            IsActive = true,
            PasswordHash = "old-hash"
        };
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(50)
        };
        var activeRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "refresh-active",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        var revokedRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "refresh-revoked",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };

        SetupUsers(user);
        SetupPasswordResetTokens(resetToken);
        SetupRefreshTokens(activeRefreshToken, revokedRefreshToken);
        _passwordHasherMock.Setup(h => h.Hash("NewPass1!")).Returns("new-hash");
        var handler = CreateHandler();

        var result = await handler.Handle(new ResetPasswordCommand
        {
            Email = user.Email,
            Token = rawToken,
            NewPassword = "NewPass1!"
        }, default);

        Assert.Equal("Password has been reset successfully.", result.Message);
        Assert.Equal("new-hash", user.PasswordHash);
        Assert.NotNull(resetToken.UsedAt);
        Assert.NotNull(activeRefreshToken.RevokedAt);
        Assert.NotNull(revokedRefreshToken.RevokedAt);
        _passwordHasherMock.Verify(h => h.Hash("NewPass1!"), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserMissing_ThrowsInvalidPasswordResetRequest()
    {
        SetupUsers();
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResetPasswordCommand
            {
                Email = "missing@test.com",
                Token = "raw-token",
                NewPassword = "NewPass1!"
            }, default));

        Assert.Equal("Invalid password reset request.", exception.Message);
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsInvalidPasswordResetRequest()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            IsActive = false
        };
        SetupUsers(user);
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResetPasswordCommand
            {
                Email = user.Email,
                Token = "raw-token",
                NewPassword = "NewPass1!"
            }, default));

        Assert.Equal("Invalid password reset request.", exception.Message);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidPasswordResetRequest()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "active@test.com", IsActive = true };
        SetupUsers(user);
        SetupPasswordResetTokens();
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResetPasswordCommand
            {
                Email = user.Email,
                Token = "invalid-token",
                NewPassword = "NewPass1!"
            }, default));

        Assert.Equal("Invalid password reset request.", exception.Message);
    }

    [Fact]
    public async Task Handle_TokenBelongsToDifferentUser_ThrowsInvalidPasswordResetRequest()
    {
        var rawToken = "valid-other-user-token";
        var submittedUser = new User { Id = Guid.NewGuid(), Email = "submitted@test.com", IsActive = true };
        var otherUserToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(50)
        };
        SetupUsers(submittedUser);
        SetupPasswordResetTokens(otherUserToken);
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResetPasswordCommand
            {
                Email = submittedUser.Email,
                Token = rawToken,
                NewPassword = "NewPass1!"
            }, default));

        Assert.Equal("Invalid password reset request.", exception.Message);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidOperationException()
    {
        var rawToken = "expired-token";
        var user = new User { Id = Guid.NewGuid(), Email = "active@test.com", IsActive = true };
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        SetupUsers(user);
        SetupPasswordResetTokens(resetToken);
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResetPasswordCommand
            {
                Email = user.Email,
                Token = rawToken,
                NewPassword = "NewPass1!"
            }, default));

        Assert.Equal("Password reset token has expired.", exception.Message);
    }

    [Fact]
    public async Task Handle_AlreadyUsedToken_ThrowsInvalidOperationException()
    {
        var rawToken = "used-token";
        var user = new User { Id = Guid.NewGuid(), Email = "active@test.com", IsActive = true };
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(50),
            UsedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        SetupUsers(user);
        SetupPasswordResetTokens(resetToken);
        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResetPasswordCommand
            {
                Email = user.Email,
                Token = rawToken,
                NewPassword = "NewPass1!"
            }, default));

        Assert.Equal("Password reset token has already been used.", exception.Message);
    }

    private ResetPasswordCommandHandler CreateHandler()
    {
        return new ResetPasswordCommandHandler(
            _contextMock.Object,
            _passwordHasherMock.Object);
    }

    private void SetupUsers(params User[] users)
    {
        var dbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(dbSet.Object);
    }

    private void SetupPasswordResetTokens(params PasswordResetToken[] tokens)
    {
        var dbSet = tokens.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.PasswordResetTokens).Returns(dbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupRefreshTokens(params RefreshToken[] tokens)
    {
        var dbSet = tokens.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.RefreshTokens).Returns(dbSet.Object);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
