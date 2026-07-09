using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Application.Identity.Commands.SendVerificationEmail;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class SendVerificationEmailCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<SendVerificationEmailCommandHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_AlreadyVerifiedUser_ReturnsSuccessWithoutCreatingTokenOrSendingEmail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "verified@test.com",
            IsActive = true,
            EmailVerified = true
        };

        SetupUsers(user);
        var tokenDbSet = SetupEmailVerificationTokens([]);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id);

        var handler = CreateHandler();

        var result = await handler.Handle(new SendVerificationEmailCommand(), default);

        Assert.Equal("Verification email sent.", result.Message);
        tokenDbSet.Verify(t => t.Add(It.IsAny<EmailVerificationToken>()), Times.Never);
        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NotVerifiedUser_CreatesHashedTokenInvalidatesOldTokensAndSendsEmail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "notverified@test.com",
            IsActive = true,
            EmailVerified = false
        };
        var activeOldToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-active-hash",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(22)
        };
        var usedOldToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-used-hash",
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            ExpiresAt = DateTime.UtcNow.AddHours(21),
            UsedAt = DateTime.UtcNow.AddHours(-1)
        };

        SetupUsers(user);
        var tokenDbSet = SetupEmailVerificationTokens([activeOldToken, usedOldToken]);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id);
        string? rawToken = null;
        EmailVerificationToken? addedToken = null;
        tokenDbSet
            .Setup(t => t.Add(It.IsAny<EmailVerificationToken>()))
            .Callback<EmailVerificationToken>(token => addedToken = token);
        _emailServiceMock
            .Setup(s => s.SendVerificationEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, token, _) => rawToken = token)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var result = await handler.Handle(new SendVerificationEmailCommand(), default);

        Assert.Equal("Verification email sent.", result.Message);
        Assert.NotNull(rawToken);
        Assert.NotEmpty(rawToken);
        Assert.NotNull(addedToken);
        Assert.Equal(user.Id, addedToken.UserId);
        Assert.Equal(ComputeSha256Hash(rawToken), addedToken.TokenHash);
        Assert.NotEqual(rawToken, addedToken.TokenHash);
        Assert.Null(addedToken.UsedAt);
        Assert.NotNull(activeOldToken.UsedAt);
        Assert.NotNull(usedOldToken.UsedAt);
        Assert.True(addedToken.ExpiresAt > addedToken.CreatedAt);
        Assert.True((addedToken.ExpiresAt - addedToken.CreatedAt) >= TimeSpan.FromHours(23));
        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailServiceThrows_LogsAndThrowsPlannedInvalidOperationException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "notverified@test.com",
            IsActive = true,
            EmailVerified = false
        };

        SetupUsers(user);
        SetupEmailVerificationTokens([]);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id);
        _emailServiceMock
            .Setup(s => s.SendVerificationEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp failed"));

        var handler = CreateHandler();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new SendVerificationEmailCommand(), default));

        Assert.Equal("Failed to send verification email.", exception.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new SendVerificationEmailCommand(), default));
    }

    [Fact]
    public async Task Handle_MissingUser_ThrowsUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        SetupUsers();

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new SendVerificationEmailCommand(), default));
    }

    private SendVerificationEmailCommandHandler CreateHandler()
    {
        return new SendVerificationEmailCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private void SetupUsers(params User[] users)
    {
        var dbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(dbSet.Object);
    }

    private Mock<DbSet<EmailVerificationToken>> SetupEmailVerificationTokens(
        IEnumerable<EmailVerificationToken> tokens)
    {
        var dbSet = tokens.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.EmailVerificationTokens).Returns(dbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return dbSet;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
