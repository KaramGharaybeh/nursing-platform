# Phase 4A — Core Identity Design

## Overview

Implement a custom authentication and identity system aligned with the existing Clean Architecture, domain model, and naming conventions. No ASP.NET Core Identity framework is used.

## Sub-Phase Breakdown

- **Phase 4A** — Core Identity (current): User entity, password hashing, JWT, login, refresh tokens, admin-only user creation
- Phase 4B — Authorization: Roles, permissions, authorization policies/middleware
- Phase 4C — Account Management: Email verification, password reset, remaining lifecycle features

Each sub-phase has its own plan, build, test, review, and commit cycle.

---

## Architecture

### Approach

Custom identity implementation following Clean Architecture:

```
WebApi (endpoints, middleware, DI)
    ↓
Application (commands, validators, DTOs, interfaces)
    ↓
Domain (User, UserRole, RefreshToken entities)
```

```
Infrastructure (JwtService, PasswordHashingService, persistence config)
    ↓
Application (commands, validators, DTOs, interfaces)
    ↓
Domain (User, UserRole, RefreshToken entities)
```

### Key Decisions

- **No ASP.NET Core Identity.** Own domain entities, own services.
- **CQRS via MediatR.** Authentication operations are all Commands (LoginCommand, RotateRefreshTokenCommand, RegisterUserCommand) — they modify application state.
- **FluentValidation from the start.** Validators for every command, handlers focused on business logic.
- **Symmetric JWT (HMAC-SHA256).** Configured via existing `JwtSettings.Secret`.
- **Admin-only user creation.** First admin seeded during database initialization. After that, only authenticated admins can create users.

---

## Domain Layer

### User Entity

```
User : AuditableEntity
  - Id : Guid
  - Email : string
  - PasswordHash : string
  - FirstName : string
  - LastName : string
  - IsActive : bool
  - EmailVerified : bool
  - LastLoginAt : DateTime? (UTC)

  Navigation:
  - UserRoles : ICollection<UserRole>
```

- Inherits `AuditableEntity` (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- No profile fields beyond basics — phone, avatar, address deferred to later phases

### UserRole Entity

```
UserRole
  - UserId : Guid (PK component, FK → User)
  - RoleId : Guid (PK component, FK → Role)

  Navigation:
  - User : User
  - Role : Role
```

- Composite primary key `(UserId, RoleId)`
- No separate `Id` property
- Does NOT inherit `AuditableEntity` (junction entity, not aggregate root)
- `DeleteBehavior.Restrict` on both foreign keys

### Updated Role Entity

```
Role : AuditableEntity
  - Id : Guid
  - Name : string
  - Description : string?

  Navigation:
  - UserRoles : ICollection<UserRole>  (NEW)
  - RolePermissions : ICollection<RolePermission>  (existing)
```

- Add `UserRoles` navigation property to existing `Role.cs`
- All other properties unchanged

### RefreshToken Entity

```
RefreshToken
  - Id : Guid
  - UserId : Guid (FK → User)
  - TokenHash : string (SHA-256 of the token)
  - ExpiresAt : DateTime (UTC)
  - CreatedAt : DateTime (UTC)
  - RevokedAt : DateTime? (UTC)

  Navigation:
  - User : User
```

- Does NOT inherit `AuditableEntity` (child entity of User aggregate)
- `DeleteBehavior.Restrict` on FK to User
- Token itself never stored — only its SHA-256 hash
- `CreatedAt` is a plain timestamp field (not audit metadata), `RevokedAt` is a business timestamp

---

## Application Layer

### Interfaces

```
Application/Abstractions/Auth/
  IJwtService.cs
  IPasswordHashingService.cs
```

#### IJwtService

```csharp
public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    RefreshTokenValidationResult ValidateRefreshToken(string refreshToken);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
```

```csharp
public class RefreshTokenValidationResult
{
    public bool IsValid { get; init; }
    public Guid? UserId { get; init; }
    public string? FailureReason { get; init; } // "expired", "revoked", "not_found"

    public static RefreshTokenValidationResult Success(Guid userId) => new() { IsValid = true, UserId = userId };
    public static RefreshTokenValidationResult Failure(string reason) => new() { IsValid = false, FailureReason = reason };
}
```

- `GenerateAccessToken`: creates JWT with sub (userId), email, given_name, family_name, name, roles (as string list), jti (GUID), iat, exp
- `GenerateRefreshToken`: returns a cryptographically secure random string
- `ValidateRefreshToken`: checks the hash against DB, returns `RefreshTokenValidationResult`
- `GetPrincipalFromExpiredToken`: extracts ClaimsPrincipal from an expired token (for refresh flow) without validating lifetime

#### IPasswordHashingService

```csharp
public interface IPasswordHashingService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

- Implemented with `Microsoft.AspNetCore.Identity.PasswordHasher<User>` internally
- No ASP.NET Core Identity framework dependency — just the password hasher component

### Commands

Organized by feature under `Application/Identity/Commands/`:

```
Commands/Register/
  RegisterUserCommand.cs
  RegisterUserCommandHandler.cs
  RegisterUserCommandValidator.cs
  RegisterUserRequest.cs        (transport DTO)
  RegisterUserResponse.cs

Commands/Login/
  LoginCommand.cs
  LoginCommandHandler.cs
  LoginCommandValidator.cs
  LoginRequest.cs
  LoginResponse.cs

Commands/RotateRefreshToken/
  RotateRefreshTokenCommand.cs
  RotateRefreshTokenCommandHandler.cs
  RotateRefreshTokenCommandValidator.cs
  RotateRefreshTokenRequest.cs
  RotateRefreshTokenResponse.cs
```

#### RegisterUserCommand

- **Request:** Email, Password, FirstName, LastName, RoleIds (List<Guid>)
- **Response:** UserId (Guid)
- **Handler logic:**
  1. Check email is not already taken
  2. Hash password
  3. Create User entity
  4. Verify all RoleIds exist in DB
  5. Create UserRole records
  6. Save to DB
  7. Return UserId

#### LoginCommand

- **Request:** Email, Password
- **Response:** AccessToken, RefreshToken, ExpiresAt
- **Handler logic:**
  1. Look up user by email
  2. Verify user is active
  3. Verify password hash
  4. Load user roles
  5. Generate JWT access token
  6. Generate and persist refresh token (SHA-256 hash)
  7. Update LastLoginAt
  8. Return tokens

#### RotateRefreshTokenCommand

- **Request:** RefreshToken (string)
- **Response:** AccessToken, RefreshToken, ExpiresAt
- **Handler logic:**
  1. Hash the provided refresh token
  2. Look up hash in DB
  3. Verify not expired and not revoked
  4. Revoke old refresh token
  5. Load user and roles
  6. Generate new JWT access token
  7. Generate and persist new refresh token
  8. Return tokens

### Validators (FluentValidation)

- **RegisterUserCommandValidator:**
  - Email: not empty, valid format
  - Password: min 8 chars, at least 1 uppercase, 1 digit
  - FirstName: not empty, max 100
  - LastName: not empty, max 100
  - RoleIds: not empty, must contain at least one role, no duplicate entries

- **LoginCommandValidator:**
  - Email: not empty, valid format
  - Password: not empty

- **RotateRefreshTokenCommandValidator:**
  - RefreshToken: not empty

### DTOs

- `RegisterUserRequest` / `RegisterUserResponse`
- `LoginRequest` / `LoginResponse`
- `RotateRefreshTokenRequest` / `RotateRefreshTokenResponse`

Transport DTOs are separate from Command objects. Endpoints map HTTP request → Command.

---

## Infrastructure Layer

### JwtService

- Located at `Infrastructure/Authentication/JwtService.cs`
- Uses `System.IdentityModel.Tokens.Jwt` and `Microsoft.IdentityModel.Tokens`
- Reads config from `JwtSettings` (already registered via IOptions)
- Access token claims: sub, email, name, roles (as string list), jti (GUID), iat, exp
- Refresh tokens: 32-byte cryptographically random string via `RandomNumberGenerator`
- Token hashing: SHA-256

### PasswordHashingService

- Located at `Infrastructure/Authentication/PasswordHashingService.cs`
- Wraps `Microsoft.AspNetCore.Identity.PasswordHasher<User>` as internal implementation
- No dependency on ASP.NET Core Identity framework — just the hasher component
- `Hash()` returns the Identity v3 hash format
- `Verify()` returns true/false

### Bootstrap Administrator

- During `ReferenceDataSeeder.SeedAsync()`, after seeding roles and permissions, create the first SuperAdmin user if no users exist
- Credentials sourced from dedicated `BootstrapAdminSettings` configuration section
- Default dev values in `appsettings.Development.json`: Email = `admin@nursingplatform.com`, Password overridable
- Assign `SuperAdmin` role
- This is a development-only bootstrap — production deployments will manage admin accounts separately

```csharp
public class BootstrapAdminSettings
{
    public const string SectionName = "BootstrapAdmin";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### New/Modified Configuration Files

| File | Change |
|---|---|
| `Infrastructure/Authentication/JwtService.cs` | Create |
| `Infrastructure/Authentication/PasswordHashingService.cs` | Create |
| `Infrastructure/Persistence/Configurations/UserConfiguration.cs` | Create |
| `Infrastructure/Persistence/Configurations/UserRoleConfiguration.cs` | Create |
| `Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | Create |
| `Infrastructure/Persistence/Configurations/RoleConfiguration.cs` | Modify (add UserRoles nav) |
| `Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` | Modify (add admin bootstrap) |
| `Infrastructure/DependencyInjection.cs` | Modify (register auth services, JWT bearer) |
| `Infrastructure/Configuration/JwtSettings.cs` | No changes needed (already exists) |

---

## WebApi Layer

### Pipeline Order

```
ExceptionMiddleware
SerilogRequestLogging
OpenAPI (dev only)
HttpsRedirection
Authentication      ← NEW
Authorization       ← NEW
Health Checks       ← these remain public
Endpoints
```

### Endpoints (mapped on `/api/v1`)

| Method | Path | Command | Auth |
|---|---|---|---|
| POST | `/api/v1/auth/login` | `LoginCommand` | Public |
| POST | `/api/v1/auth/refresh` | `RotateRefreshTokenCommand` | Public |
| POST | `/api/v1/auth/register` | `RegisterUserCommand` | Admin only |

- All endpoints return Problem Details (RFC 7807) for errors
- Auth endpoints return `200 OK` on success, `401 Unauthorized` on invalid credentials, `400 Bad Request` on validation errors

### Modified Files

| File | Change |
|---|---|
| `WebApi/Program.cs` | Add `app.UseAuthentication()` / `app.UseAuthorization()` |
| `WebApi/Extensions/ApplicationBuilderExtensions.cs` | Add auth middleware to pipeline |
| `WebApi/Extensions/ServiceCollectionExtensions.cs` | Add authentication + authorization service registration |
| `WebApi/Program.cs` or new file | Map auth endpoints under `/api/v1` group |

---

## NuGet Dependencies

| Package | Version | Project |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.x | WebApi |
| `System.IdentityModel.Tokens.Jwt` | latest stable | Infrastructure |
| `Microsoft.Extensions.Identity.Core` | latest stable | Infrastructure |

---

## Migration

- New EF Core migration: `AddIdentityTables`
- Creates tables: `Users`, `UserRoles`, `RefreshTokens`
- Updates `Roles` table if needed (no schema change expected)
- `UserRoles` uses composite primary key `(UserId, RoleId)` with `DeleteBehavior.Restrict` on both foreign keys
- `Users.RefreshTokens` relation uses `DeleteBehavior.Restrict`

---

## Testing

### Unit Tests (Domain)

- User creation sets expected defaults (IsActive = true, EmailVerified = false)
- UserRole links User and Role correctly
- RefreshToken enforces expiration correctly

### Unit Tests (Application — via mocked dependencies)

- RegisterUserCommand: duplicate email returns error, invalid RoleId returns error, duplicate RoleIds rejected, success creates user
- LoginCommand: valid credentials return tokens, invalid password returns 401, inactive user returns 401
- RotateRefreshTokenCommand: valid token issues new tokens, expired token rejected, revoked token rejected

### Unit Tests (Infrastructure)

- PasswordHashingService: hash produces non-empty result, verify matches original password, verify rejects wrong password, same password produces different hashes each time

### Integration Tests

Test project: `NursingPlatform.WebApi.IntegrationTests` (or extend existing test infrastructure)

- POST `/api/v1/auth/login` returns JWT and refresh token
- Invalid credentials return 401 Unauthorized
- POST `/api/v1/auth/refresh` issues a new access token
- Expired/revoked refresh token is rejected
- POST `/api/v1/auth/register` requires authorization (returns 401/403 without valid admin token)
- Refresh token reuse after rotation is rejected (using the old refresh token after a successful rotation returns an error)

---

## Constraints

- All timestamps in UTC
- All entities use Guid primary keys
- Plural PascalCase table names
- No ASP.NET Core Identity framework in Domain layer
- JWT secret from configuration (dev secret in `appsettings.Development.json`)
- Refresh tokens stored as SHA-256 hashes only
- `DeleteBehavior.Restrict` on all foreign keys for consistency
- Health check endpoints remain public (no auth)
- Rate limiting is intentionally deferred to a later phase

---

## Out of Scope

The following features are explicitly deferred to later phases and must NOT be implemented in Phase 4A:

- Email verification
- Password reset / forgot password
- Multi-factor authentication (2FA)
- External / social login providers (OAuth, Google, Facebook, etc.)
- Account lockout after failed login attempts
- "Remember me" persistent sessions
- Server-side session management
- Rate limiting on auth endpoints
- Public self-registration
- User profile management (avatar, phone, address, bio)
- Account deletion / deactivation workflows
