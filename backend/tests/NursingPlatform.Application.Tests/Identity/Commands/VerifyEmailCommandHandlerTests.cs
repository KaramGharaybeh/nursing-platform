using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Commands.VerifyEmail;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();

    [Fact]
    public async Task Handle_ValidToken_VerifiesEmailMarksTokenUsedAndSaves()
    {
        var rawToken = "valid-token";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            IsActive = true,
            EmailVerified = false
        };
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };

        SetupUsers(user);
        SetupEmailVerificationTokens(verificationToken);
        var handler = new VerifyEmailCommandHandler(_contextMock.Object);

        var result = await handler.Handle(new VerifyEmailCommand { Token = rawToken }, default);

        Assert.Equal("Email verified successfully.", result.Message);
        Assert.True(user.EmailVerified);
        Assert.NotNull(verificationToken.UsedAt);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidOperationException()
    {
        SetupEmailVerificationTokens();
        var handler = new VerifyEmailCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new VerifyEmailCommand { Token = "invalid-token" }, default));

        Assert.Equal("Invalid verification token.", exception.Message);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidOperationException()
    {
        var rawToken = "expired-token";
        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        SetupEmailVerificationTokens(token);
        var handler = new VerifyEmailCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new VerifyEmailCommand { Token = rawToken }, default));

        Assert.Equal("Verification token has expired.", exception.Message);
    }

    [Fact]
    public async Task Handle_AlreadyUsedToken_ThrowsInvalidOperationException()
    {
        var rawToken = "used-token";
        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = ComputeSha256Hash(rawToken),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddHours(23),
            UsedAt = DateTime.UtcNow.AddMinutes(-30)
        };
        SetupEmailVerificationTokens(token);
        var handler = new VerifyEmailCommandHandler(_contextMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new VerifyEmailCommand { Token = rawToken }, default));

        Assert.Equal("Verification token has already been used.", exception.Message);
    }

    private void SetupUsers(params User[] users)
    {
        var dbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(dbSet.Object);
    }

    private void SetupEmailVerificationTokens(params EmailVerificationToken[] tokens)
    {
        var dbSet = tokens.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.EmailVerificationTokens).Returns(dbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
