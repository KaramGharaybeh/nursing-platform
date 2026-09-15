using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Application.Identity.Commands.PublicRegister;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class PublicRegisterCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<PublicRegisterCommandHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ValidRequest_CreatesUnverifiedActiveUserWithOnlyNurseRoleAndSendsVerificationEmail()
    {
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var usersDbSet = SetupUsers();
        var emailTokensDbSet = SetupEmailVerificationTokens();
        SetupRoles(nurseRole);
        _passwordHasherMock.Setup(p => p.Hash("Password1!")).Returns("hashed-password");
        string? rawToken = null;
        EmailVerificationToken? addedToken = null;
        User? addedUser = null;
        usersDbSet.Setup(u => u.Add(It.IsAny<User>())).Callback<User>(user => addedUser = user);
        emailTokensDbSet
            .Setup(t => t.Add(It.IsAny<EmailVerificationToken>()))
            .Callback<EmailVerificationToken>(token => addedToken = token);
        _emailServiceMock
            .Setup(s => s.SendVerificationEmailAsync("new-user@test.com", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, token, _) => rawToken = token)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("new-user@test.com", "new-user"), default);

        Assert.NotNull(addedUser);
        Assert.Equal("new-user@test.com", addedUser.Email);
        Assert.Equal("new-user", addedUser.Username);
        Assert.Equal("NEW-USER", addedUser.NormalizedUsername);
        Assert.Equal("hashed-password", addedUser.PasswordHash);
        Assert.True(addedUser.IsActive);
        Assert.False(addedUser.EmailVerified);
        Assert.Single(addedUser.UserRoles);
        Assert.Equal(nurseRole.Id, addedUser.UserRoles.Single().RoleId);
        Assert.NotNull(rawToken);
        Assert.NotEmpty(rawToken);
        Assert.NotNull(addedToken);
        Assert.Equal(addedUser.Id, addedToken.UserId);
        Assert.Equal(ComputeSha256Hash(rawToken), addedToken.TokenHash);
        Assert.NotEqual(rawToken, addedToken.TokenHash);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync("new-user@test.com", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_DoesNotRequireFirstOrLastName()
    {
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var usersDbSet = SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(nurseRole);
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        User? addedUser = null;
        usersDbSet.Setup(u => u.Add(It.IsAny<User>())).Callback<User>(user => addedUser = user);
        _emailServiceMock
            .Setup(s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("noname@test.com", "noname-user"), default);

        Assert.NotNull(addedUser);
        Assert.Equal(string.Empty, addedUser.FirstName);
        Assert.Equal(string.Empty, addedUser.LastName);
    }

    [Fact]
    public async Task Handle_UsernameNormalization_IsAppliedAndEnforced()
    {
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = "Nurse" };
        var usersDbSet = SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(nurseRole);
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        User? addedUser = null;
        usersDbSet.Setup(u => u.Add(It.IsAny<User>())).Callback<User>(user => addedUser = user);
        _emailServiceMock
            .Setup(s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("case@test.com", "  MixedCase.User-1  "), default);

        Assert.NotNull(addedUser);
        Assert.Equal("MixedCase.User-1", addedUser.Username);
        Assert.Equal("MIXEDCASE.USER-1", addedUser.NormalizedUsername);
    }

    [Fact]
    public async Task Handle_ExistingNormalizedUsername_ReturnsWithoutCreatingUserOrSendingVerificationEmail()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "other@test.com",
            Username = "Taken.Name",
            NormalizedUsername = "TAKEN.NAME",
            PasswordHash = "existing-hash",
            IsActive = true,
            EmailVerified = true
        };
        var usersDbSet = SetupUsers(existingUser);
        var emailTokensDbSet = SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("brand-new@test.com", "taken.name"), default);

        usersDbSet.Verify(u => u.Add(It.IsAny<User>()), Times.Never);
        emailTokensDbSet.Verify(t => t.Add(It.IsAny<EmailVerificationToken>()), Times.Never);
        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ReturnsWithoutMutatingUserOrSendingVerificationEmail()
    {
        var existingRoleId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            Username = "existing-user",
            NormalizedUsername = "EXISTING-USER",
            PasswordHash = "existing-hash",
            IsActive = true,
            EmailVerified = true
        };
        existingUser.UserRoles.Add(new UserRole { UserId = existingUser.Id, RoleId = existingRoleId });
        var usersDbSet = SetupUsers(existingUser);
        var emailTokensDbSet = SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });

        var handler = CreateHandler();

        await handler.Handle(CreateCommand(existingUser.Email, "some-other-username"), default);

        Assert.Equal("existing-hash", existingUser.PasswordHash);
        Assert.True(existingUser.EmailVerified);
        Assert.Single(existingUser.UserRoles);
        Assert.Equal(existingRoleId, existingUser.UserRoles.Single().RoleId);
        usersDbSet.Verify(u => u.Add(It.IsAny<User>()), Times.Never);
        emailTokensDbSet.Verify(t => t.Add(It.IsAny<EmailVerificationToken>()), Times.Never);
        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingNurseRole_FailsClosed()
    {
        SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Employer" });

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(CreateCommand("new@test.com", "new-user"), default));
    }

    [Fact]
    public async Task Handle_EmailUniqueRace_ReturnsWithoutSendingVerificationEmailWhenRaceIsRecognized()
    {
        SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("duplicate email", new InvalidOperationException("unique")));
        _contextMock
            .Setup(c => c.IsUniqueUserEmailViolation(It.IsAny<DbUpdateException>()))
            .Returns(true);
        _contextMock
            .Setup(c => c.IsUniqueUsernameViolation(It.IsAny<DbUpdateException>()))
            .Returns(false);

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("race@test.com", "race-user"), default);

        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UsernameUniqueRace_ReturnsWithoutSendingVerificationEmailWhenRaceIsRecognized()
    {
        SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        _contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("duplicate username", new InvalidOperationException("unique")));
        _contextMock
            .Setup(c => c.IsUniqueUserEmailViolation(It.IsAny<DbUpdateException>()))
            .Returns(false);
        _contextMock
            .Setup(c => c.IsUniqueUsernameViolation(It.IsAny<DbUpdateException>()))
            .Returns(true);

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("new-race@test.com", "race-user"), default);

        _emailServiceMock.Verify(
            s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UnrelatedSaveFailure_IsNotSwallowed()
    {
        SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        var failure = new DbUpdateException("connection failed");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(failure);
        _contextMock.Setup(c => c.IsUniqueUserEmailViolation(failure)).Returns(false);
        _contextMock.Setup(c => c.IsUniqueUsernameViolation(failure)).Returns(false);

        var handler = CreateHandler();

        var thrown = await Assert.ThrowsAsync<DbUpdateException>(() =>
            handler.Handle(CreateCommand("new@test.com", "new-user"), default));
        Assert.Same(failure, thrown);
    }

    [Fact]
    public async Task Handle_EmailDeliveryFailureAfterPersistence_LogsAndReturnsWithoutDeletingAccount()
    {
        var usersDbSet = SetupUsers();
        SetupEmailVerificationTokens();
        SetupRoles(new Role { Id = Guid.NewGuid(), Name = "Nurse" });
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        User? addedUser = null;
        usersDbSet.Setup(u => u.Add(It.IsAny<User>())).Callback<User>(user => addedUser = user);
        _emailServiceMock
            .Setup(s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp failed"));

        var handler = CreateHandler();

        await handler.Handle(CreateCommand("new@test.com", "new-user"), default);

        Assert.NotNull(addedUser);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_NonNurseRoleName_IsRejected()
    {
        var validator = new PublicRegisterCommandValidator();
        var command = new PublicRegisterCommand
        {
            Email = "new@test.com",
            Username = "new-user",
            Password = "Password1!",
            RoleName = "Employer"
        };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_UsernameRequired()
    {
        var validator = new PublicRegisterCommandValidator();
        var command = new PublicRegisterCommand
        {
            Email = "new@test.com",
            Username = string.Empty,
            Password = "Password1!",
            RoleName = "Nurse"
        };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validator_WhitespaceUsername_ReturnsError()
    {
        var validator = new PublicRegisterCommandValidator();
        var command = new PublicRegisterCommand
        {
            Email = "new@test.com",
            Username = "   ",
            Password = "Password1!",
            RoleName = "Nurse"
        };

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void PublicRequest_DoesNotExposeRoleOrAccountTypeFields()
    {
        var propertyNames = typeof(PublicRegisterRequest)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        Assert.Contains("Email", propertyNames);
        Assert.Contains("Username", propertyNames);
        Assert.Contains("Password", propertyNames);
        Assert.DoesNotContain(
            propertyNames,
            name => name.Contains("Role", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            propertyNames,
            name => name.Contains("AccountType", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            propertyNames,
            name => name.Contains("ActorType", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("FirstName", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("LastName", StringComparison.OrdinalIgnoreCase));
    }

    private PublicRegisterCommandHandler CreateHandler()
    {
        return new PublicRegisterCommandHandler(
            _contextMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    private static PublicRegisterCommand CreateCommand(string email, string username)
    {
        return new PublicRegisterCommand
        {
            Email = email,
            Username = username,
            Password = "Password1!",
            RoleName = "Nurse"
        };
    }

    private Mock<DbSet<User>> SetupUsers(params User[] users)
    {
        var dbSet = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(dbSet.Object);
        return dbSet;
    }

    private void SetupRoles(params Role[] roles)
    {
        var dbSet = roles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(dbSet.Object);
    }

    private Mock<DbSet<EmailVerificationToken>> SetupEmailVerificationTokens(
        params EmailVerificationToken[] tokens)
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
