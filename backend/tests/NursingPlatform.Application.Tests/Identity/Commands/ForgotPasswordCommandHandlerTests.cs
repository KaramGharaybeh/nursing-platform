using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Application.Identity.Commands.ForgotPassword;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private const string SuccessMessage = "If the email exists, a password reset link has been sent.";

    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ExistingActiveUser_CreatesHashedTokenInvalidatesOldTokensAndSendsEmail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "active@test.com",
            IsActive = true
        };
        var activeOldToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-active-hash",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
        var usedOldToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-used-hash",
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            ExpiresAt = DateTime.UtcNow.AddMinutes(20),
            UsedAt = DateTime.UtcNow.AddHours(-1)
        };

        SetupUsers(user);
        var tokenDbSet = SetupPasswordResetTokens([activeOldToken, usedOldToken]);
        string? rawToken = null;
        PasswordResetToken? addedToken = null;
        tokenDbSet
            .Setup(t => t.Add(It.IsAny<PasswordResetToken>()))
            .Callback<PasswordResetToken>(token => addedToken = token);
        _emailServiceMock
            .Setup(s => s.SendPasswordResetEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, token, _) => rawToken = token)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var result = await handler.Handle(new ForgotPasswordCommand { Email = user.Email }, default);

        Assert.Equal(SuccessMessage, result.Message);
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
        Assert.True((addedToken.ExpiresAt - addedToken.CreatedAt) >= TimeSpan.FromMinutes(59));
        _emailServiceMock.Verify(
            s => s.SendPasswordResetEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonexistentEmail_ReturnsSuccessWithoutCreatingTokenOrSendingEmail()
    {
        SetupUsers();
        var tokenDbSet = SetupPasswordResetTokens([]);
        var handler = CreateHandler();

        var result = await handler.Handle(new ForgotPasswordCommand { Email = "missing@test.com" }, default);

        Assert.Equal(SuccessMessage, result.Message);
        tokenDbSet.Verify(t => t.Add(It.IsAny<PasswordResetToken>()), Times.Never);
        _emailServiceMock.Verify(
            s => s.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsSuccessWithoutCreatingTokenOrSendingEmail()
    {
        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            IsActive = false
        };
        SetupUsers(inactiveUser);
        var tokenDbSet = SetupPasswordResetTokens([]);
        var handler = CreateHandler();

        var result = await handler.Handle(new ForgotPasswordCommand { Email = inactiveUser.Email }, default);

        Assert.Equal(SuccessMessage, result.Message);
        tokenDbSet.Verify(t => t.Add(It.IsAny<PasswordResetToken>()), Times.Never);
        _emailServiceMock.Verify(
            s => s.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailServiceThrows_LogsErrorAndReturnsSuccess()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "active@test.com",
            IsActive = true
        };
        SetupUsers(user);
        SetupPasswordResetTokens([]);
        _emailServiceMock
            .Setup(s => s.SendPasswordResetEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp failed"));
        var handler = CreateHandler();

        var result = await handler.Handle(new ForgotPasswordCommand { Email = user.Email }, default);

        Assert.Equal(SuccessMessage, result.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private ForgotPasswordCommandHandler CreateHandler()
    {
        return new ForgotPasswordCommandHandler(
            _contextMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private void SetupUsers(params User[] users)
    {
        var dbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(dbSet.Object);
    }

    private Mock<DbSet<PasswordResetToken>> SetupPasswordResetTokens(
        IEnumerable<PasswordResetToken> tokens)
    {
        var dbSet = tokens.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.PasswordResetTokens).Returns(dbSet.Object);
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
