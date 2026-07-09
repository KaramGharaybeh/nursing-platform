# Phase 4D — Identity Account Recovery & Verification

## Objective

Implement email verification and password reset flows for the Nursing Platform. These complete the deferred identity items from Phase 4A and close out the Identity & Security phase before moving to Phase 5 (Nurse Module).

The design follows all existing patterns: Clean Architecture, CQRS, token-based verification via cryptographically secure random tokens with SHA-256 hash storage, and SMTP email sending via MailKit.

## Approved Scope

| # | Endpoint | Description | Auth |
|---|----------|-------------|------|
| 1 | `POST /api/v1/auth/send-verification-email` | Send email verification link to authenticated user | `.RequireAuthorization()` |
| 2 | `POST /api/v1/auth/verify-email` | Verify email using token from email | `.AllowAnonymous()` |
| 3 | `POST /api/v1/auth/forgot-password` | Send password reset link to email | `.AllowAnonymous()` |
| 4 | `POST /api/v1/auth/reset-password` | Reset password using token from email | `.AllowAnonymous()` |

## Out of Scope

The following are explicitly deferred:

- Change password (authenticated, requires old password)
- Update own profile (`PATCH /api/v1/me`)
- Delete user
- Role CRUD management endpoints
- Permission CRUD management endpoints
- Activate/deactivate users
- Admin role assignment
- Frontend integration
- Email template customization
- Email send provider configuration UI

## Token Design

### Token Generation

All verification and reset tokens follow the same pattern used by `JwtService.GenerateRefreshToken`:

1. Generate 64 cryptographically secure random bytes via `System.Security.Cryptography.RandomNumberGenerator`
2. Encode as Base64 string → this is the raw token (sent via email, never stored)
3. Compute SHA-256 hash of the raw token → this is stored in the database
4. The raw token is never returned in any API response

### Token Persistence

Two new entities in `NursingPlatform.Domain.Identity`:

**EmailVerificationToken:**

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| UserId | Guid | FK to User, Restrict delete |
| TokenHash | string | SHA-256 hex, max 256 chars, unique index |
| ExpiresAt | DateTime | UTC, 24 hours from creation |
| CreatedAt | DateTime | UTC |
| UsedAt | DateTime? | Null until consumed |

**PasswordResetToken:**

Same structure as `EmailVerificationToken` with a 1-hour expiry.

Both entities follow the same persistence pattern as `RefreshToken`: `TokenHash` has a unique index, `UserId` has a non-unique index, `User` FK uses `DeleteBehavior.Restrict`.

### Token Invalidation Rules

1. **On new issuance:** Before creating a new token of the same type for the same user, mark all existing tokens where `UserId == userId && UsedAt == null` as used (`UsedAt = DateTime.UtcNow`).
2. **On consumption:** After successful verification or password reset, mark the token as used (`UsedAt = DateTime.UtcNow`).
3. **On expiry:** The handler checks `ExpiresAt >= DateTime.UtcNow`. Expired tokens are rejected with a clear error.

## Endpoint Contracts

### 1. `POST /api/v1/auth/send-verification-email`

**Auth:** Any authenticated user (`.RequireAuthorization()`)

**Request:** No body. Identity extracted from JWT via `ICurrentUserService`.

**Response 200:**
```json
{
  "message": "Verification email sent."
}
```

**Response 401:** Missing or invalid JWT.

**Behavior:**
- Read `UserId` from `ICurrentUserService`
- Load user. If not found → throw `UnauthorizedAccessException`
- If `user.EmailVerified == true` → return 200 with no-op (no token created, no email sent)
- Invalidate all existing `EmailVerificationToken` for this user where `UsedAt == null` → set `UsedAt = DateTime.UtcNow`
- Generate raw token, compute hash, store `EmailVerificationToken` (24h expiry)
- Call `IEmailService.SendVerificationEmailAsync(user.Email, rawToken)` — awaited
- If email service throws → throw `InvalidOperationException("Failed to send verification email.")`
- Return 200

### 2. `POST /api/v1/auth/verify-email`

**Auth:** Anonymous (`.AllowAnonymous()`) — token-based

**Request:**
```json
{
  "token": "base64-encoded-raw-token"
}
```

**Response 200:**
```json
{
  "message": "Email verified successfully."
}
```

**Response 400:** Validation failure (empty token).

**Response 409:** Invalid, expired, or already used token.

**Behavior:**
- Validate `token` is not empty
- Compute SHA-256 hash of submitted token
- Find `EmailVerificationToken` by `TokenHash`. If not found → throw `InvalidOperationException("Invalid verification token.")`
- If `UsedAt != null` → throw `InvalidOperationException("Verification token has already been used.")`
- If `ExpiresAt < DateTime.UtcNow` → throw `InvalidOperationException("Verification token has expired.")`
- Load the user. If not found or not active → throw `InvalidOperationException("User not found.")`
- Set `user.EmailVerified = true`
- Mark token as used (`UsedAt = DateTime.UtcNow`)
- SaveChangesAsync
- Return 200

### 3. `POST /api/v1/auth/forgot-password`

**Auth:** Anonymous (`.AllowAnonymous()`)

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response 200 (always):**
```json
{
  "message": "If the email exists, a password reset link has been sent."
}
```

**Response 400:** Validation failure (invalid email format).

**Behavior:**
- Validate email format
- Look up user by email where `IsActive == true`. If not found → skip to step 6 (return 200, no user existence leak)
- Invalidate all existing `PasswordResetToken` for this user where `UsedAt == null` → set `UsedAt = DateTime.UtcNow`
- Generate raw token, compute hash, store `PasswordResetToken` (1h expiry)
- Attempt to call `IEmailService.SendPasswordResetEmailAsync(user.Email, rawToken)` — awaited
- If email service throws → log the error (do NOT propagate to client)
- Always return 200 with the same message

### 4. `POST /api/v1/auth/reset-password`

**Auth:** Anonymous (`.AllowAnonymous()`) — token-based

**Request:**
```json
{
  "email": "user@example.com",
  "token": "base64-encoded-raw-token",
  "newPassword": "NewP@ss1"
}
```

**Response 200:**
```json
{
  "message": "Password has been reset successfully."
}
```

**Response 400:** Validation failure.

**Response 409:** Invalid, expired, or used token; token does not belong to submitted email.

**Behavior:**
- Validate all fields
- Look up user by email. If not found or not active → throw `InvalidOperationException("Invalid password reset request.")`
- Compute SHA-256 hash of submitted token. Find `PasswordResetToken` by `TokenHash` where `UserId == user.Id`. If not found → throw `InvalidOperationException("Invalid password reset request.")`
- If `UsedAt != null` → throw `InvalidOperationException("Password reset token has already been used.")`
- If `ExpiresAt < DateTime.UtcNow` → throw `InvalidOperationException("Password reset token has expired.")`
- Hash the new password using `IPasswordHashingService.Hash()`. Update `user.PasswordHash`.
- Revoke all active refresh tokens for this user: set `RevokedAt = DateTime.UtcNow` on all `RefreshToken` where `UserId == user.Id && RevokedAt == null`
- Mark token as used (`UsedAt = DateTime.UtcNow`)
- SaveChangesAsync
- Return 200

## Email Service Design

### Interface (Application Layer)

```csharp
namespace NursingPlatform.Application.Abstractions.Notifications;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default);
}
```

### Implementation (Infrastructure Layer)

Uses MailKit's `SmtpClient` to send emails via SMTP. Configuration is read from `EmailSettings` (already registered in DI):

- `SmtpHost`, `SmtpPort`, `Username`, `Password`, `FromAddress`, `FromName`, `UseSsl` — SMTP connection settings
- `ApplicationUrl` — base URL for building verification/reset links in email bodies

URLs constructed:
- Verification: `{ApplicationUrl}/verify-email?token={Uri.EscapeDataString(rawToken)}`
- Password reset: `{ApplicationUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}`

`ApplicationUrl` must be configured before building links. An empty value fails clearly with `InvalidOperationException("ApplicationUrl is not configured.")` instead of sending relative links.

Email bodies are simple HTML with the link. The URL path (`/verify-email`, `/reset-password`) is a frontend route — the backend serves only the API endpoints.

### Settings

Add `ApplicationUrl` to the existing `EmailSettings` class in `Infrastructure/Configuration/EmailSettings.cs`:

```csharp
public string ApplicationUrl { get; set; } = string.Empty;
```

Development value: `"ApplicationUrl": "http://localhost:5000"`

### MailKit Dependency

Add to `Infrastructure.csproj`:
```xml
<PackageReference Include="MailKit" Version="4.17.0" />
```

MimeKit is a transitive dependency of MailKit and does not need a separate package reference.

## Exception Mapping

All exceptions use the existing `ExceptionMiddleware` mapping:

| Exception | HTTP Status |
|-----------|-------------|
| `ValidationException` (FluentValidation) | 400 |
| `UnauthorizedAccessException` | 401 |
| `KeyNotFoundException` | 404 |
| `InvalidOperationException` | 409 |

## EF Migration

Adds two new tables: `EmailVerificationTokens` and `PasswordResetTokens`.

Generate via:
```bash
dotnet ef migrations add AddIdentityVerificationTokens \
  --project backend/src/NursingPlatform.Infrastructure \
  --startup-project backend/src/NursingPlatform.WebApi \
  --context ApplicationDbContext
```

## Modified Files

### New Files

| Layer | File |
|-------|------|
| Domain | `Identity/EmailVerificationToken.cs` |
| Domain | `Identity/PasswordResetToken.cs` |
| Application | `Abstractions/Notifications/IEmailService.cs` |
| Application | `Identity/Commands/SendVerificationEmail/SendVerificationEmailCommand.cs` |
| Application | `Identity/Commands/SendVerificationEmail/SendVerificationEmailCommandHandler.cs` |
| Application | `Identity/Commands/SendVerificationEmail/SendVerificationEmailResponse.cs` |
| Application | `Identity/Commands/VerifyEmail/VerifyEmailCommand.cs` |
| Application | `Identity/Commands/VerifyEmail/VerifyEmailCommandHandler.cs` |
| Application | `Identity/Commands/VerifyEmail/VerifyEmailCommandValidator.cs` |
| Application | `Identity/Commands/VerifyEmail/VerifyEmailRequest.cs` |
| Application | `Identity/Commands/VerifyEmail/VerifyEmailResponse.cs` |
| Application | `Identity/Commands/ForgotPassword/ForgotPasswordCommand.cs` |
| Application | `Identity/Commands/ForgotPassword/ForgotPasswordCommandHandler.cs` |
| Application | `Identity/Commands/ForgotPassword/ForgotPasswordCommandValidator.cs` |
| Application | `Identity/Commands/ForgotPassword/ForgotPasswordRequest.cs` |
| Application | `Identity/Commands/ForgotPassword/ForgotPasswordResponse.cs` |
| Application | `Identity/Commands/ResetPassword/ResetPasswordCommand.cs` |
| Application | `Identity/Commands/ResetPassword/ResetPasswordCommandHandler.cs` |
| Application | `Identity/Commands/ResetPassword/ResetPasswordCommandValidator.cs` |
| Application | `Identity/Commands/ResetPassword/ResetPasswordRequest.cs` |
| Application | `Identity/Commands/ResetPassword/ResetPasswordResponse.cs` |
| Infrastructure | `Notifications/EmailService.cs` |
| Infrastructure | `Persistence/Configurations/EmailVerificationTokenConfiguration.cs` |
| Infrastructure | `Persistence/Configurations/PasswordResetTokenConfiguration.cs` |

### Modified Files

| Layer | File | Change |
|-------|------|--------|
| Application | `Abstractions/Data/IApplicationDbContext.cs` | Add `DbSet<EmailVerificationToken> EmailVerificationTokens` and `DbSet<PasswordResetToken> PasswordResetTokens` |
| Infrastructure | `Infrastructure.csproj` | Add `MailKit` package reference |
| Infrastructure | `Configuration/EmailSettings.cs` | Add `ApplicationUrl` property |
| Infrastructure | `Persistence/ApplicationDbContext.cs` | Add `DbSet<EmailVerificationToken> EmailVerificationTokens` and `DbSet<PasswordResetToken> PasswordResetTokens` |
| Infrastructure | `DependencyInjection.cs` | Register `IEmailService` → `EmailService` |
| WebApi | `Extensions/ApplicationBuilderExtensions.cs` | Add 4 new endpoints |
| WebApi | `appsettings.json` | Add `Email:ApplicationUrl` |
| WebApi | `appsettings.Development.json` | Add `Email:ApplicationUrl` with `http://localhost:5000` default |

## Test Strategy

### Application Handler Tests

| Handler | Test Cases |
|---------|------------|
| `SendVerificationEmailHandlerTests` | (1) Already verified → no-op, no token created, returns success. (2) Not verified → sends email, creates token, invalidates old token. (3) Email service throws → propagates error. |
| `VerifyEmailHandlerTests` | (1) Valid token → marks verified, marks token used. (2) Invalid token hash → throws. (3) Expired token → throws. (4) Already used token → throws. |
| `ForgotPasswordHandlerTests` | (1) Existing active user → creates token, sends email, invalidates old token. (2) Non-existent email → returns success (no-op). (3) Inactive user → returns success (no-op). (4) Email service throws → still returns success, logs error. |
| `ResetPasswordHandlerTests` | (1) Valid token + matching email → hashes password, updates user, revokes refresh tokens, marks token used. (2) Invalid token → throws. (3) Expired token → throws. (4) Token doesn't belong to submitted email → throws. (5) Already used token → throws. |

### Validator Tests

| Validator | Test Cases |
|-----------|------------|
| `VerifyEmailCommandValidator` | (1) Empty token → invalid |
| `ForgotPasswordCommandValidator` | (1) Empty email → invalid. (2) Invalid email format → invalid. (3) Valid email → passes. |
| `ResetPasswordCommandValidator` | (1) Empty email → invalid. (2) Empty token → invalid. (3) Empty/weak password → invalid. (4) All valid → passes. |

### WebApi Integration Tests

| Endpoint | Test Cases |
|----------|------------|
| `POST /auth/send-verification-email` | 200 authenticated → success. 401 no token → unauthorized. |
| `POST /auth/verify-email` | 200 valid token → success. 400 missing token → validation error. |
| `POST /auth/forgot-password` | 200 with existing email → success. 200 with non-existent email → success (same message). 400 invalid email → validation error. |
| `POST /auth/reset-password` | 200 valid request → success. 400 missing fields → validation error. |

Integration tests mock `ISender` and do not verify `IEmailService` calls. `IEmailService` invocation is verified in Application handler tests.

## Risks and Mitigations

### Risk 1: Email Delivery Failure

If the SMTP server is unreachable, the email service throws. For verification (authenticated endpoint), the error propagates to the client with a clear message. For forgot-password (anonymous), the error is logged and 200 is returned to avoid leaking user existence. The user can retry.

### Risk 2: Token Collision

SHA-256 hash collision is cryptographically infeasible. The unique index on `TokenHash` provides an additional safety net at the database level.

### Risk 3: Token Reuse Detection

If a user has multiple valid tokens (possible if they request verification before the previous email arrives), only the most recently created token is usable after invalidation. The invalidation step marks all previous tokens as used, so only the latest token works.

### Risk 4: Refresh Token Revocation on Password Reset

After a password reset, all existing sessions are invalidated because refresh tokens are revoked. The user must log in again with the new password. This is the desired security behavior — a password reset should terminate all existing sessions.
