# Phase 4D — Identity Account Recovery & Verification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement email verification and password reset flows — the two deferred identity items from Phase 4A.

**Architecture:** Four new CQRS commands (`SendVerificationEmailCommand`, `VerifyEmailCommand`, `ForgotPasswordCommand`, `ResetPasswordCommand`) with handlers that use `IApplicationDbContext`, `ICurrentUserService`, `IPasswordHashingService`, and `IEmailService`. Token persistence via two new entities (`EmailVerificationToken`, `PasswordResetToken`). Email sending via MailKit using existing `EmailSettings` configuration.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, EF Core, MailKit, Moq, xUnit

**Spec:** `docs/superpowers/specs/2026-07-09-identity-account-recovery-verification.md`

## Global Constraints

- Token generation: 64 random bytes via `RandomNumberGenerator`, Base64 encoded (raw token), SHA-256 hash stored.
- Raw token never returned in API responses. Only sent via email.
- Raw Base64 tokens must be URL-encoded with `Uri.EscapeDataString(token)` before being placed in email query strings.
- Previous active tokens of the same type for the same user invalidated at issuance (`UsedAt = DateTime.UtcNow`).
- `EmailVerificationToken` expiry: 24 hours. `PasswordResetToken` expiry: 1 hour.
- `ApplicationUrl` from `EmailSettings` used to build email URLs. Development default: `http://localhost:5000`.
- Empty `ApplicationUrl` fails clearly before building links.
- Forgot-password always returns 200 with same message (no user existence leak). Email failures logged, not propagated.
- Send-verification-email propagates email failures (authenticated user, safe to surface).
- Already verified user → send-verification-email returns 200 no-op, no token created.
- Reset password validates token belongs to submitted email (lookup by UserId + TokenHash).
- Reset password revokes all active refresh tokens for the user.
- MailKit package added to Infrastructure. MimeKit comes transitively.
- Follow existing project conventions (file placement, naming, SHA-256 hashing pattern from `LoginCommandHandler`, dependency injection patterns).
- `ComputeSha256Hash` static method duplicated in each handler (consistent with existing `LoginCommandHandler` and `RotateRefreshTokenCommandHandler`).

---

### Task 1: Create Domain Entities + EF Configurations + DbContext + Migration

**Files:**
- Create: `backend/src/NursingPlatform.Domain/Identity/EmailVerificationToken.cs`
- Create: `backend/src/NursingPlatform.Domain/Identity/PasswordResetToken.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/EmailVerificationTokenConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PasswordResetTokenConfiguration.cs`
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`

**Interfaces:**
- Produces: `EmailVerificationToken` entity, `PasswordResetToken` entity, EF configurations, updated `IApplicationDbContext`

- [ ] **Step 1: Create EmailVerificationToken.cs**

```csharp
namespace NursingPlatform.Domain.Identity;

public class EmailVerificationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public User User { get; set; } = null!;
}
```

- [ ] **Step 2: Create PasswordResetToken.cs**

```csharp
namespace NursingPlatform.Domain.Identity;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public User User { get; set; } = null!;
}
```

- [ ] **Step 3: Create EmailVerificationTokenConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("EmailVerificationTokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(256);
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 4: Create PasswordResetTokenConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(256);
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 5: Update IApplicationDbContext.cs**

Add before the closing brace:
```csharp
DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
DbSet<PasswordResetToken> PasswordResetTokens { get; }
```

Add using for `NursingPlatform.Domain.Identity` (already imported).

- [ ] **Step 6: Update ApplicationDbContext.cs**

Add before the closing brace of properties:
```csharp
public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
```

Add using for `NursingPlatform.Domain.Identity` (already imported).

- [ ] **Step 7: Build and generate migration**

```bash
dotnet build backend/NursingPlatform.slnx
```

Expected: 0 errors, 0 warnings.

```bash
dotnet ef migrations add AddIdentityVerificationTokens \
  --project backend/src/NursingPlatform.Infrastructure \
  --startup-project backend/src/NursingPlatform.WebApi \
  --context ApplicationDbContext
```

Expected: Migration created with `Up` creating `EmailVerificationTokens` and `PasswordResetTokens` tables, `Down` dropping them.

- [ ] **Step 8: Run existing tests to confirm no regressions**

```bash
dotnet test backend/NursingPlatform.slnx
```

Expected: ALL PASS (169 tests).

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 2: Create IEmailService Interface + EmailService Implementation + DI Registration + Config Updates

**Files:**
- Create: `backend/src/NursingPlatform.Application/Abstractions/Notifications/IEmailService.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Notifications/EmailService.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Configuration/EmailSettings.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/NursingPlatform.Infrastructure.csproj`
- Modify: `backend/src/NursingPlatform.WebApi/appsettings.json`
- Modify: `backend/src/NursingPlatform.WebApi/appsettings.Development.json`

**Interfaces:**
- Consumes: `EmailSettings` (expanded with `ApplicationUrl`)
- Produces: `IEmailService` interface, `EmailService` implementation, DI registration

- [ ] **Step 1: Create IEmailService.cs**

```csharp
namespace NursingPlatform.Application.Abstractions.Notifications;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Add `ApplicationUrl` to EmailSettings.cs**

Add property:
```csharp
public string ApplicationUrl { get; set; } = string.Empty;
```

- [ ] **Step 3: Add MailKit package to Infrastructure.csproj**

```xml
<PackageReference Include="MailKit" Version="4.17.0" />
```

- [ ] **Step 4: Create EmailService.cs**

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Infrastructure.Configuration;

namespace NursingPlatform.Infrastructure.Notifications;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApplicationUrl))
            throw new InvalidOperationException("ApplicationUrl is not configured.");

        var encodedToken = Uri.EscapeDataString(token);
        var url = $"{_settings.ApplicationUrl.TrimEnd('/')}/verify-email?token={encodedToken}";
        var body = $"<p>Click <a href=\"{url}\">here</a> to verify your email address.</p>";

        _logger.LogInformation("Sending verification email to {Email}", to);
        await SendEmailAsync(to, "Verify your email address", body, cancellationToken);
        _logger.LogInformation("Verification email sent to {Email}", to);
    }

    public async Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApplicationUrl))
            throw new InvalidOperationException("ApplicationUrl is not configured.");

        var encodedToken = Uri.EscapeDataString(token);
        var url = $"{_settings.ApplicationUrl.TrimEnd('/')}/reset-password?token={encodedToken}";
        var body = $"<p>Click <a href=\"{url}\">here</a> to reset your password.</p>";

        _logger.LogInformation("Sending password reset email to {Email}", to);
        await SendEmailAsync(to, "Reset your password", body, cancellationToken);
        _logger.LogInformation("Password reset email sent to {Email}", to);
    }

    private async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        if (!_settings.UseSsl)
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
        else
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, true, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
```

- [ ] **Step 5: Update DependencyInjection.cs**

Add after the `services.AddScoped<IJwtService, JwtService>();` line:
```csharp
services.AddScoped<IEmailService, EmailService>();
```

Add to using block:
```csharp
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Infrastructure.Notifications;
```

- [ ] **Step 6: Update appsettings.json**

Add `ApplicationUrl` to the Email section:
```json
"Email": {
    "ApplicationUrl": "",
    "SmtpHost": "",
    "SmtpPort": 0,
    "Username": "",
    "Password": "",
    "FromAddress": "",
    "FromName": "",
    "UseSsl": false
}
```

- [ ] **Step 7: Update appsettings.Development.json**

```json
"Email": {
    "ApplicationUrl": "http://localhost:5000",
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "Username": "",
    "Password": "",
    "FromAddress": "noreply@nursingplatform.dev",
    "FromName": "Nursing Platform",
    "UseSsl": false
}
```

- [ ] **Step 8: Build and run tests**

```bash
dotnet build backend/NursingPlatform.slnx
```
Expected: 0 errors, 0 warnings.

```bash
dotnet test backend/NursingPlatform.slnx
```
Expected: ALL PASS.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 3: Create SendVerificationEmailCommand + Handler + Tests + Endpoint

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/SendVerificationEmail/SendVerificationEmailCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/SendVerificationEmail/SendVerificationEmailCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/SendVerificationEmail/SendVerificationEmailResponse.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/SendVerificationEmailCommandHandlerTests.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`

**Interfaces:**
- Consumes: `ICurrentUserService`, `IApplicationDbContext`, `IEmailService`
- Produces: `SendVerificationEmailCommand : IRequest<SendVerificationEmailResponse>`, handler, endpoint

- [ ] **Step 1: Create SendVerificationEmailCommand.cs**

```csharp
using MediatR;

namespace NursingPlatform.Application.Identity.Commands.SendVerificationEmail;

public class SendVerificationEmailCommand : IRequest<SendVerificationEmailResponse>
{
}
```

- [ ] **Step 2: Create SendVerificationEmailResponse.cs**

```csharp
namespace NursingPlatform.Application.Identity.Commands.SendVerificationEmail;

public class SendVerificationEmailResponse
{
    public string Message { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Create SendVerificationEmailCommandHandler.cs**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.SendVerificationEmail;

public class SendVerificationEmailCommandHandler : IRequestHandler<SendVerificationEmailCommand, SendVerificationEmailResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public SendVerificationEmailCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<SendVerificationEmailResponse> Handle(SendVerificationEmailCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found.");

        if (user.EmailVerified)
            return new SendVerificationEmailResponse { Message = "Verification email sent." };

        // Invalidate existing active verification tokens
        var existingTokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in existingTokens)
            token.UsedAt = DateTime.UtcNow;

        // Generate new token
        var rawToken = GenerateToken();
        var tokenHash = ComputeSha256Hash(rawToken);

        _context.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationEmailAsync(user.Email, rawToken, cancellationToken);

        return new SendVerificationEmailResponse { Message = "Verification email sent." };
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

- [ ] **Step 4: Write handler tests**

File: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/SendVerificationEmailCommandHandlerTests.cs`

Test cases:
1. Already verified user → no token created, no email sent, returns success
2. Not verified → creates token, invalidates old tokens, sends email, returns success
3. Email service throws → propagates exception
4. User not found → throws UnauthorizedAccessException
5. UserId null → throws UnauthorizedAccessException

Use `Mock<IApplicationDbContext>` with `MockQueryable.Moq`, `Mock<ICurrentUserService>`, `Mock<IEmailService>`.

- [ ] **Step 5: Add endpoint to ApplicationBuilderExtensions.cs**

Add after existing register endpoint block:
```csharp
api.MapPost("/auth/send-verification-email", async (ISender sender) =>
{
    var result = await sender.Send(new SendVerificationEmailCommand());
    return Results.Ok(result);
})
.WithName("SendVerificationEmail")
.RequireAuthorization();
```

- [ ] **Step 6: Run tests**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "SendVerificationEmail"
```
Expected: ALL PASS.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 4: Create VerifyEmailCommand + Handler + Validator + Tests + Endpoint

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/VerifyEmail/VerifyEmailCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/VerifyEmail/VerifyEmailCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/VerifyEmail/VerifyEmailCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/VerifyEmail/VerifyEmailRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/VerifyEmail/VerifyEmailResponse.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/VerifyEmailCommandHandlerTests.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/VerifyEmailCommandValidatorTests.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`
- Produces: `VerifyEmailCommand`, handler, validator, endpoint

- [ ] **Step 1: Create VerifyEmailCommand.cs**

```csharp
using MediatR;

namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<VerifyEmailResponse>
{
    public string Token { get; init; } = string.Empty;
}
```

- [ ] **Step 2: Create VerifyEmailRequest.cs**

```csharp
namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Create VerifyEmailCommandValidator.cs**

```csharp
using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
```

- [ ] **Step 4: Create VerifyEmailCommandHandler.cs**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly IApplicationDbContext _context;

    public VerifyEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VerifyEmailResponse> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256Hash(command.Token);

        var storedToken = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
            throw new InvalidOperationException("Invalid verification token.");

        if (storedToken.UsedAt is not null)
            throw new InvalidOperationException("Verification token has already been used.");

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Verification token has expired.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidOperationException("User not found.");

        user.EmailVerified = true;
        storedToken.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse { Message = "Email verified successfully." };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

Create VerifyEmailResponse.cs:
```csharp
namespace NursingPlatform.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailResponse
{
    public string Message { get; init; } = string.Empty;
}
```

- [ ] **Step 5: Write handler tests**

Test cases:
1. Valid token → sets EmailVerified = true, marks token used
2. Invalid token hash → throws InvalidOperationException
3. Expired token → throws InvalidOperationException
4. Already used token → throws InvalidOperationException

- [ ] **Step 6: Write validator tests**

Test cases:
1. Empty token → invalid (validation error)

- [ ] **Step 7: Add endpoint to ApplicationBuilderExtensions.cs**

```csharp
api.MapPost("/auth/verify-email", async (VerifyEmailRequest request, ISender sender) =>
{
    var command = new VerifyEmailCommand { Token = request.Token };
    var result = await sender.Send(command);
    return Results.Ok(result);
})
.WithName("VerifyEmail")
.AllowAnonymous();
```

Add using for `NursingPlatform.Application.Identity.Commands.VerifyEmail`.

- [ ] **Step 8: Run tests**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "VerifyEmail"
```
Expected: ALL PASS.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 5: Create ForgotPasswordCommand + Handler + Validator + Tests + Endpoint

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ForgotPassword/ForgotPasswordCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ForgotPassword/ForgotPasswordCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ForgotPassword/ForgotPasswordCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ForgotPassword/ForgotPasswordRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ForgotPassword/ForgotPasswordResponse.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/ForgotPasswordCommandHandlerTests.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/ForgotPasswordCommandValidatorTests.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`, `IEmailService`
- Produces: `ForgotPasswordCommand`, handler, validator, endpoint

- [ ] **Step 1: Create ForgotPasswordCommand.cs**

```csharp
using MediatR;

namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
{
    public string Email { get; init; } = string.Empty;
}
```

- [ ] **Step 2: Create ForgotPasswordRequest.cs**

```csharp
namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Create ForgotPasswordCommandValidator.cs**

```csharp
using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

- [ ] **Step 4: Create ForgotPasswordCommandHandler.cs**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Abstractions.Notifications;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email && u.IsActive, cancellationToken);

        if (user is not null)
        {
            // Invalidate existing active password reset tokens
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.UsedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in existingTokens)
                token.UsedAt = DateTime.UtcNow;

            // Generate new token
            var rawToken = GenerateToken();
            var tokenHash = ComputeSha256Hash(rawToken);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, rawToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            }
        }

        return new ForgotPasswordResponse
        {
            Message = "If the email exists, a password reset link has been sent."
        };
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

Create ForgotPasswordResponse.cs:
```csharp
namespace NursingPlatform.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordResponse
{
    public string Message { get; init; } = string.Empty;
}
```

- [ ] **Step 5: Write handler tests**

Test cases:
1. Existing active user → creates token, invalidates old tokens, sends email, returns success
2. Non-existent email → returns success (no-op, no token created, no email sent)
3. Inactive user → returns success (no-op)
4. Email service throws → returns success (logged, not propagated)

- [ ] **Step 6: Write validator tests**

Test cases:
1. Empty email → invalid
2. Invalid email format → invalid
3. Valid email → passes

- [ ] **Step 7: Add endpoint to ApplicationBuilderExtensions.cs**

```csharp
api.MapPost("/auth/forgot-password", async (ForgotPasswordRequest request, ISender sender) =>
{
    var command = new ForgotPasswordCommand { Email = request.Email };
    var result = await sender.Send(command);
    return Results.Ok(result);
})
.WithName("ForgotPassword")
.AllowAnonymous();
```

Add using for `NursingPlatform.Application.Identity.Commands.ForgotPassword`.

- [ ] **Step 8: Run tests**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "ForgotPassword"
```
Expected: ALL PASS.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 6: Create ResetPasswordCommand + Handler + Validator + Tests + Endpoint

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ResetPassword/ResetPasswordCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ResetPassword/ResetPasswordCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ResetPassword/ResetPasswordCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ResetPassword/ResetPasswordRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/ResetPassword/ResetPasswordResponse.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/ResetPasswordCommandHandlerTests.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/ResetPasswordCommandValidatorTests.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`, `IPasswordHashingService`
- Produces: `ResetPasswordCommand`, handler, validator, endpoint

- [ ] **Step 1: Create ResetPasswordCommand.cs**

```csharp
using MediatR;

namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<ResetPasswordResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
```

- [ ] **Step 2: Create ResetPasswordRequest.cs**

```csharp
namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordRequest
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Create ResetPasswordResponse.cs**

```csharp
namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordResponse
{
    public string Message { get; init; } = string.Empty;
}
```

- [ ] **Step 4: Create ResetPasswordCommandValidator.cs**

```csharp
using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z\d]").WithMessage("Password must contain at least one special character.");
    }
}
```

- [ ] **Step 5: Create ResetPasswordCommandHandler.cs**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHashingService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Invalid password reset request.");

        var tokenHash = ComputeSha256Hash(command.Token);

        var storedToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UserId == user.Id, cancellationToken);

        if (storedToken is null)
            throw new InvalidOperationException("Invalid password reset request.");

        if (storedToken.UsedAt is not null)
            throw new InvalidOperationException("Password reset token has already been used.");

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Password reset token has expired.");

        user.PasswordHash = _passwordHasher.Hash(command.NewPassword);
        storedToken.UsedAt = DateTime.UtcNow;

        // Revoke all active refresh tokens for this user
        var activeRefreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
            refreshToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResponse { Message = "Password has been reset successfully." };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

- [ ] **Step 6: Write handler tests**

Test cases:
1. Valid token + matching email → hashes password, updates user, revokes all refresh tokens, marks token used
2. Invalid token → throws InvalidOperationException
3. Expired token → throws InvalidOperationException
4. Token doesn't belong to submitted email → throws InvalidOperationException
5. Already used token → throws InvalidOperationException
6. User not found / inactive → throws InvalidOperationException

- [ ] **Step 7: Write validator tests**

Test cases:
1. Empty email → invalid
2. Empty token → invalid
3. Empty password → invalid
4. Password too short → invalid
5. Password missing uppercase → invalid
6. Password missing digit → invalid
7. All valid → passes

- [ ] **Step 8: Add endpoint to ApplicationBuilderExtensions.cs**

```csharp
api.MapPost("/auth/reset-password", async (ResetPasswordRequest request, ISender sender) =>
{
    var command = new ResetPasswordCommand
    {
        Email = request.Email,
        Token = request.Token,
        NewPassword = request.NewPassword
    };
    var result = await sender.Send(command);
    return Results.Ok(result);
})
.WithName("ResetPassword")
.AllowAnonymous();
```

Add using for `NursingPlatform.Application.Identity.Commands.ResetPassword`.

- [ ] **Step 9: Run tests**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "ResetPassword"
```
Expected: ALL PASS.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 7: WebApi Integration Tests for All 4 Endpoints

**Files:**
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AccountRecoveryEndpointTests.cs`

**Interfaces:**
- Consumes: `WebApiTestFactory`, JWT helper, mock `ISender`
- Produces: Integration tests for all 4 Phase 4D endpoints

- [ ] **Step 1: Write AccountRecoveryEndpointTests.cs**

```csharp
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NursingPlatform.Application.Identity.Commands.ForgotPassword;
using NursingPlatform.Application.Identity.Commands.ResetPassword;
using NursingPlatform.Application.Identity.Commands.SendVerificationEmail;
using NursingPlatform.Application.Identity.Commands.VerifyEmail;
using NursingPlatform.WebApi.IntegrationTests;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class AccountRecoveryEndpointTests
{
    private readonly WebApiTestFactory _factory;

    public AccountRecoveryEndpointTests(WebApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SendVerificationEmail_WithValidToken_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            _factory.CreateJwt(Guid.NewGuid()));

        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<SendVerificationEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendVerificationEmailResponse { Message = "Verification email sent." });

        // Act
        var response = await client.PostAsync("/api/v1/auth/send-verification-email", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<SendVerificationEmailResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal("Verification email sent.", body.Message);
    }

    [Fact]
    public async Task SendVerificationEmail_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/v1/auth/send-verification-email", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_Returns200()
    {
        var client = _factory.CreateClient();
        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifyEmailResponse { Message = "Email verified successfully." });

        var request = new { Token = "valid-token" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/verify-email", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<VerifyEmailResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal("Email verified successfully.", body.Message);
    }

    [Fact]
    public async Task VerifyEmail_WithEmptyToken_Returns400()
    {
        var client = _factory.CreateClient();
        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure("Token", "'Token' must not be empty.") }));

        var request = new { Token = "" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/verify-email", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_Returns200()
    {
        var client = _factory.CreateClient();
        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForgotPasswordResponse { Message = "If the email exists, a password reset link has been sent." });

        var request = new { Email = "user@example.com" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_Returns400()
    {
        var client = _factory.CreateClient();
        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure("Email", "'Email' is not a valid email address.") }));

        var request = new { Email = "not-an-email" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_Returns200()
    {
        var client = _factory.CreateClient();
        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetPasswordResponse { Message = "Password has been reset successfully." });

        var request = new { Email = "user@example.com", Token = "valid-token", NewPassword = "NewP@ss1" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithMissingFields_Returns400()
    {
        var client = _factory.CreateClient();
        var senderMock = _factory.Services.GetRequiredService<Mock<ISender>>();
        senderMock.Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException(
                new[]
                {
                    new FluentValidation.Results.ValidationFailure("Email", "'Email' must not be empty."),
                    new FluentValidation.Results.ValidationFailure("Token", "'Token' must not be empty."),
                    new FluentValidation.Results.ValidationFailure("NewPassword", "'New Password' must not be empty.")
                }));

        var request = new { Email = "", Token = "", NewPassword = "" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
```

- [ ] **Step 2: Run integration tests**

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests/ --filter "AccountRecoveryEndpoint"
```
Expected: ALL PASS.

- [ ] **Step 3: Run all tests**

```bash
dotnet test backend/NursingPlatform.slnx
```
Expected: ALL PASS.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 8: Final Build, Full Test Suite, EF Migration Verification

**Files:**
- No source changes. Only verification.

- [ ] **Step 1: Full solution build**

```bash
dotnet build backend/NursingPlatform.slnx
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 2: Run all tests**

```bash
dotnet test backend/NursingPlatform.slnx
```
Expected: ALL PASS. Total test count: ~199 (169 existing + ~30 new).

- [ ] **Step 3: EF Core migration verification**

Generate a no-op migration to confirm no model changes remain:
```bash
dotnet ef migrations add VerifyPhase4DNoModelChanges \
  --project backend/src/NursingPlatform.Infrastructure \
  --startup-project backend/src/NursingPlatform.WebApi \
  --context ApplicationDbContext
```

Inspect the generated files. If `Up` and `Down` are empty, the migration is a no-op. Delete the generated migration files and restore `ApplicationDbContextModelSnapshot.cs` if changed. If not empty, paste the generated migration for review.

- [ ] **Step 4: Verify git status**

```bash
git status --short
```
Expected: Only Phase 4D source, test, config, doc, and migration files. No unintended modifications.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 9: Update CURRENT_TASK.md and TASKS.md + Final Commit

**Files:**
- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

- [ ] **Step 1: Update CURRENT_TASK.md**

Set Current Milestone to "Phase 4D — Identity Account Recovery & Verification" with status Complete.

- [ ] **Step 2: Update TASKS.md**

Mark the following items as complete in Phase 4A:
- `[x] Email verification`
- `[x] Password reset`

Add and mark complete a Phase 4D section:
```markdown
## Phase 4D — Identity Account Recovery & Verification ✅

- [x] Email verification tokens and persistence
- [x] Password reset tokens and persistence
- [x] Email service (MailKit)
- [x] POST /api/v1/auth/send-verification-email
- [x] POST /api/v1/auth/verify-email
- [x] POST /api/v1/auth/forgot-password
- [x] POST /api/v1/auth/reset-password
- [x] Application handler tests
- [x] Integration tests
- [x] EF migration
- [x] Final build, test, and verification
```

- [ ] **Step 3: Paste diffs for review**

```bash
git diff CURRENT_TASK.md TASKS.md
```
Paste the output for review.

**Do not commit until explicitly approved.**

---

### Task 10: Final Commit (Only After Explicit Approval)

- [ ] **Step 1: Review all changes**

```bash
git log --oneline -5
git diff --stat
git status
```

- [ ] **Step 2: Stage all intended files**

Stage source files, test files, spec, plan, migration, config updates, CURRENT_TASK.md, TASKS.md. Exclude AGENTS.md and TODO.

- [ ] **Step 3: Commit**

```bash
git commit -m "feat: add email verification and password reset (Phase 4D)"
```

---

### WebApi Integration Test Guidance

Validation error tests for anonymous endpoints (verify-email, forgot-password, reset-password) follow the existing `AuthEndpointTests.Login_ValidationError_Returns400WithErrors` pattern:

- Set up `_senderMock.Send()` to throw `new FluentValidation.ValidationException(new[] { new ValidationFailure("Field", "error") })`
- POST the invalid request body
- Assert `HttpStatusCode.BadRequest`
- Assert the response contains the validation error details in the `errors` dictionary

This works because the integration test replaces the real MediatR pipeline with a mock. The mock throws the validation exception, and the `ExceptionMiddleware` catches it and returns the ProblemDetails response with validation errors.

Do NOT attempt to trigger FluentValidation through real request binding — the ISender mock bypasses the actual handler pipeline.

## Self-Review Checklist

- **Spec coverage** — All requirements from `docs/superpowers/specs/2026-07-09-identity-account-recovery-verification.md` covered:
  - Email verification: send + verify endpoints
  - Password reset: forgot + reset endpoints
  - Token generation: 64 random bytes, SHA-256 hash stored, raw token never in API response
  - Token invalidation: previous active tokens invalidated on new issuance (UsedAt check, not RevokedAt)
  - Refresh token revocation on password reset
  - Token-user binding: reset validates token belongs to submitted email
  - Already-verified no-op: send-verification-email returns success for verified users
  - Configurable base URL: `EmailSettings.ApplicationUrl` drives email links
  - Forgot-password: always returns same 200, logs email failures
  - MailKit: single package, MimeKit transitive
- **No fire-and-forget** — all `IEmailService` calls are awaited
- **Token not returned in API response** — verified by integration tests via raw JSON assertions
- **Integration tests mock ISender** — do not verify IEmailService calls (handled in Application tests)
- **Placeholder scan** — No "TBD", "TODO", "implement later", or similar patterns in production code
- **Type consistency** — `ComputeSha256Hash` signature consistent with existing handlers
