# Phase 4A — Core Identity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement custom authentication and identity for the Nursing Platform — User entity, JWT, login, refresh tokens, and admin-only user creation.

**Architecture:** Clean Architecture with CQRS (MediatR). All authentication operations modeled as Commands. Domain entities remain framework-independent. Infrastructure implements Application interfaces for JWT and password hashing.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core + Npgsql, MediatR, FluentValidation, JWT Bearer, xUnit, Moq

## Global Constraints

- All entities use `Guid` primary keys
- All timestamps in UTC
- Plural PascalCase table names (e.g., `Users`, `UserRoles`)
- `DeleteBehavior.Restrict` on ALL foreign keys (no Cascade)
- No ASP.NET Core Identity framework in Domain layer
- Domain never depends on Infrastructure or WebApi
- Refresh tokens stored as SHA-256 hashes only (never plaintext)
- JWT claims: `sub`, `email`, `given_name`, `family_name`, `name`, `roles`, `jti`, `iat`, `exp`
- Access token lifetime: from `JwtSettings.ExpirationInMinutes` (default 15 min)
- Refresh token lifetime: from `JwtSettings.RefreshTokenExpirationInDays` (default 7 days)
- Follow existing code patterns (AuditableEntity, IEntityTypeConfiguration, DbSets in ApplicationDbContext)
- Transport DTOs are separate from MediatR Command objects
- Application layer defines `IApplicationDbContext` with `DbSet<T>` (pragmatic EF Core dependency at interface level) — `ApplicationDbContext` implements it in Infrastructure

---
### Task 1: Domain Entities (User, UserRole, RefreshToken) + Role Update

**Files:**
- Create: `backend/src/NursingPlatform.Domain/Identity/User.cs`
- Create: `backend/src/NursingPlatform.Domain/Identity/UserRole.cs`
- Create: `backend/src/NursingPlatform.Domain/Identity/RefreshToken.cs`
- Modify: `backend/src/NursingPlatform.Domain/ReferenceData/Role.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/Identity/IdentityEntitiesTests.cs`

**Interfaces:**
- Consumes: `AuditableEntity` (existing), `Role` (existing)
- Produces: `User`, `UserRole`, `RefreshToken`, updated `Role`

- [ ] **Step 1: Write failing domain tests**

```csharp
// IdentityEntitiesTests.cs
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Tests.Identity;

public class IdentityEntitiesTests
{
    [Fact]
    public void User_Creation_SetsDefaultValues()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe"
        };

        Assert.True(user.IsActive);
        Assert.False(user.EmailVerified);
        Assert.Null(user.LastLoginAt);
    }

    [Fact]
    public void UserRole_LinksUserAndRole()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRole = new UserRole { UserId = userId, RoleId = roleId };

        Assert.Equal(userId, userRole.UserId);
        Assert.Equal(roleId, userRole.RoleId);
    }

    [Fact]
    public void RefreshToken_EnforcesExpiration()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        };

        Assert.False(token.ExpiresAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UserRole_DoesNotInheritAuditableEntity()
    {
        Assert.False(typeof(UserRole).IsSubclassOf(typeof(NursingPlatform.Domain.Common.AuditableEntity)));
    }

    [Fact]
    public void RefreshToken_DoesNotInheritAuditableEntity()
    {
        Assert.False(typeof(RefreshToken).IsSubclassOf(typeof(NursingPlatform.Domain.Common.AuditableEntity)));
    }

    [Fact]
    public void Role_HasUserRolesNavigation()
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "TestRole" };
        var userRole = new UserRole { RoleId = role.Id };
        role.UserRoles = new List<UserRole> { userRole };

        Assert.Single(role.UserRoles);
        Assert.Equal(role.Id, role.UserRoles.First().RoleId);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/NursingPlatform.Domain.Tests --filter "IdentityEntitiesTests" -v`
Expected: FAIL — 6 tests fail (types not found)

- [ ] **Step 3: Create domain entities**

```csharp
// Domain/Identity/User.cs
using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.Identity;

public class User : AuditableEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
```

```csharp
// Domain/Identity/UserRole.cs
namespace NursingPlatform.Domain.Identity;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
```

```csharp
// Domain/Identity/RefreshToken.cs
namespace NursingPlatform.Domain.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public User User { get; set; } = null!;
}
```

- [ ] **Step 4: Modify Role.cs**

Add to existing `Role.cs`:
```csharp
public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
```
Add `using NursingPlatform.Domain.Identity;`.

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test tests/NursingPlatform.Domain.Tests --filter "IdentityEntitiesTests" -v`
Expected: PASS — all 6 tests pass

- [ ] **Step 6: Commit**

```bash
git add backend/src/NursingPlatform.Domain/Identity/ backend/src/NursingPlatform.Domain/ReferenceData/Role.cs backend/tests/NursingPlatform.Domain.Tests/Identity/
git commit -m "feat: add User, UserRole, RefreshToken domain entities"
```

---
### Task 2: EF Core Configurations + ApplicationDbContext + Migration

**Files:**
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/UserRoleConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RoleConfiguration.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Auto-generate: EF Core migration `AddIdentityTables`

**Interfaces:**
- Consumes: `User`, `UserRole`, `RefreshToken` from Domain
- Produces: EF configurations, updated DbContext, migration

- [ ] **Step 1: Create UserConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.EmailVerified).IsRequired();
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 2: Create UserRoleConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 3: Create RefreshTokenConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(256);
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 4: Modify RoleConfiguration.cs**

Add to `Configure` after existing property configurations:
```csharp
builder.HasMany(r => r.UserRoles)
    .WithOne(ur => ur.Role)
    .HasForeignKey(ur => ur.RoleId)
    .OnDelete(DeleteBehavior.Restrict);
```

- [ ] **Step 5: Modify ApplicationDbContext.cs**

Add DbSet properties:
```csharp
public DbSet<User> Users => Set<User>();
public DbSet<UserRole> UserRoles => Set<UserRole>();
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
```
Add `using NursingPlatform.Domain.Identity;`.

- [ ] **Step 6: Build**

Run: `dotnet build` — Expected: 0 errors

- [ ] **Step 7: Generate migration**

Run from WebApi project dir:
```bash
dotnet ef migrations add AddIdentityTables --project ../NursingPlatform.Infrastructure --startup-project .
```
Expected: Migration with `Users`, `UserRoles`, `RefreshTokens` tables.

- [ ] **Step 8: Build verification**

Run: `dotnet build` — Expected: 0 errors

- [ ] **Step 9: Commit**

```bash
git add backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/UserConfiguration.cs backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/UserRoleConfiguration.cs backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RoleConfiguration.cs backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/
git commit -m "feat: add EF configurations, DbSets, and AddIdentityTables migration"
```

---
### Task 3: NuGet Packages + Application Interfaces + IApplicationDbContext

**Files:**
- Add packages to Application, Infrastructure, and WebApi .csproj files
- Add `MockQueryable.Moq` and `Moq` to Application.Tests.csproj
- Create: `backend/src/NursingPlatform.Application/Abstractions/Auth/IJwtService.cs`
- Create: `backend/src/NursingPlatform.Application/Abstractions/Auth/IPasswordHashingService.cs`
- Create: `backend/src/NursingPlatform.Application/Abstractions/Auth/RefreshTokenValidationResult.cs`
- Create: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`

**Note:** The Application project needs `Microsoft.EntityFrameworkCore` for `DbSet<T>` in `IApplicationDbContext`. This is a pragmatic compromise — the interface provides Entity Framework's `DbSet<T>` which Infrastructure implements, a standard pattern in .NET Clean Architecture.

- [ ] **Step 1: Add NuGet packages**

Application.csproj:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.9" />
```

Infrastructure.csproj:
```xml
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.4.0" />
<PackageReference Include="Microsoft.Extensions.Identity.Core" Version="10.0.9" />
```

WebApi.csproj:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.9" />
```

Application.Tests.csproj:
```xml
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="MockQueryable.Moq" Version="7.0.3" />
```

- [ ] **Step 2: Create IJwtService.cs**

```csharp
using System.Security.Claims;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Abstractions.Auth;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    RefreshTokenValidationResult ValidateRefreshToken(string refreshToken);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
```

- [ ] **Step 3: Create IPasswordHashingService.cs**

```csharp
namespace NursingPlatform.Application.Abstractions.Auth;

public interface IPasswordHashingService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

- [ ] **Step 4: Create RefreshTokenValidationResult.cs**

```csharp
namespace NursingPlatform.Application.Abstractions.Auth;

public class RefreshTokenValidationResult
{
    public bool IsValid { get; init; }
    public Guid? UserId { get; init; }
    public string? FailureReason { get; init; }

    public static RefreshTokenValidationResult Success(Guid userId) =>
        new() { IsValid = true, UserId = userId };

    public static RefreshTokenValidationResult Failure(string reason) =>
        new() { IsValid = false, FailureReason = reason };
}
```

- [ ] **Step 5: Create IApplicationDbContext.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 6: Have ApplicationDbContext implement IApplicationDbContext**

Modify `ApplicationDbContext.cs`:
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
```

- [ ] **Step 7: Build**

Run: `dotnet build` — Expected: 0 errors

- [ ] **Step 8: Commit**

```bash
git add backend/src/NursingPlatform.Application/NursingPlatform.Application.csproj backend/src/NursingPlatform.Infrastructure/NursingPlatform.Infrastructure.csproj backend/src/NursingPlatform.WebApi/NursingPlatform.WebApi.csproj backend/src/NursingPlatform.Application/Abstractions/ backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj
git commit -m "feat: add NuGet packages, auth interfaces, and IApplicationDbContext"
```

---
### Task 4: RegisterUserCommand + Handler + Validator + DTOs

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/Register/RegisterUserCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/Register/RegisterUserCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/Register/RegisterUserCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/Register/RegisterUserRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/Register/RegisterUserResponse.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/RegisterUserCommandTests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`, `IPasswordHashingService`
- Produces: RegisterUserCommand, handler, validator, DTOs

- [ ] **Step 1: Write failing tests**

```csharp
// RegisterUserCommandTests.cs
using Microsoft.EntityFrameworkCore;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Commands.Register;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Identity.Commands;

public class RegisterUserCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsException()
    {
        var existingUser = new User { Email = "existing@test.com" };
        var users = new List<User> { existingUser }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        var handler = new RegisterUserCommandHandler(_contextMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand { Email = "existing@test.com" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_InvalidRoleId_ThrowsException()
    {
        _contextMock.Setup(c => c.Users).Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Roles).Returns(new List<Role>().AsQueryable().BuildMockDbSet().Object);
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hash");
        var handler = new RegisterUserCommandHandler(_contextMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand { Email = "new@test.com", Password = "Password1", FirstName = "John", LastName = "Doe", RoleIds = new List<Guid> { Guid.NewGuid() } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserAndReturnsId()
    {
        var roleId = Guid.NewGuid();
        var roles = new List<Role> { new() { Id = roleId, Name = "Nurse" } }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Roles).Returns(roles.Object);
        _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hash");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new RegisterUserCommandHandler(_contextMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand { Email = "new@test.com", Password = "Password1", FirstName = "John", LastName = "Doe", RoleIds = new List<Guid> { roleId } };
        var result = await handler.Handle(command, default);
        Assert.NotEqual(Guid.Empty, result);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_EmptyEmail_ReturnsError()
    {
        var v = new RegisterUserCommandValidator();
        var c = new RegisterUserCommand { Email = "", Password = "Password1", FirstName = "John", LastName = "Doe", RoleIds = new List<Guid> { Guid.NewGuid() } };
        Assert.False(v.Validate(c).IsValid);
    }

    [Fact]
    public void Validator_DuplicateRoleIds_ReturnsError()
    {
        var id = Guid.NewGuid();
        var v = new RegisterUserCommandValidator();
        var c = new RegisterUserCommand { Email = "t@t.com", Password = "Password1", FirstName = "J", LastName = "D", RoleIds = new List<Guid> { id, id } };
        Assert.False(v.Validate(c).IsValid);
    }

    [Fact]
    public void Validator_PasswordTooShort_ReturnsError()
    {
        var v = new RegisterUserCommandValidator();
        var c = new RegisterUserCommand { Email = "t@t.com", Password = "short", FirstName = "J", LastName = "D", RoleIds = new List<Guid> { Guid.NewGuid() } };
        Assert.False(v.Validate(c).IsValid);
    }
}
```

- [ ] **Step 2: Run to verify fail**

Run: `dotnet test tests/NursingPlatform.Application.Tests --filter "RegisterUserCommandTests" -v`
Expected: FAIL

- [ ] **Step 3: Create RegisterUserCommand.cs**

```csharp
using MediatR;

namespace NursingPlatform.Application.Identity.Commands.Register;

public class RegisterUserCommand : IRequest<Guid>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<Guid> RoleIds { get; init; } = new();
}
```

- [ ] **Step 4: Create RegisterUserCommandHandler.cs**

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;

    public RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHashingService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{command.Email}' is already registered.");

        var validRoleIds = await _context.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var invalidRoleIds = command.RoleIds.Except(validRoleIds).ToList();
        if (invalidRoleIds.Count != 0)
            throw new InvalidOperationException($"Role(s) not found: {string.Join(", ", invalidRoleIds)}");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = _passwordHasher.Hash(command.Password),
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = true,
            EmailVerified = false
        };

        foreach (var roleId in validRoleIds)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
```

- [ ] **Step 5: Create RegisterUserCommandValidator.cs**

```csharp
using FluentValidation;

namespace NursingPlatform.Application.Identity.Commands.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleIds).NotEmpty()
            .Must(ids => ids.Count == ids.Distinct().Count()).WithMessage("RoleIds must not contain duplicate entries.");
    }
}
```

- [ ] **Step 6: Create DTOs**

```csharp
// RegisterUserRequest.cs
namespace NursingPlatform.Application.Identity.Commands.Register;
public class RegisterUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<Guid> RoleIds { get; init; } = new();
}

// RegisterUserResponse.cs
namespace NursingPlatform.Application.Identity.Commands.Register;
public class RegisterUserResponse
{
    public Guid UserId { get; init; }
}
```

- [ ] **Step 7: Run tests to verify pass**

Run: `dotnet test tests/NursingPlatform.Application.Tests --filter "RegisterUserCommandTests" -v`
Expected: PASS

- [ ] **Step 8: Commit**

```bash
git add backend/src/NursingPlatform.Application/Identity/Commands/Register/ backend/tests/NursingPlatform.Application.Tests/Identity/Commands/
git commit -m "feat: add RegisterUser command, handler, validator, and DTOs"
```

---
### Task 5: LoginCommand + RotateRefreshTokenCommand + Validators + DTOs

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Common/AuthResult.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/Login/*` (5 files)
- Create: `backend/src/NursingPlatform.Application/Identity/Commands/RotateRefreshToken/*` (5 files)
- Test: LoginCommandTests.cs, RotateRefreshTokenCommandTests.cs

**Interfaces:**
- Consumes: `IApplicationDbContext`, `IPasswordHashingService`, `IJwtService`
- Produces: LoginCommand, RotateRefreshTokenCommand, handlers, validators, DTOs

- [ ] **Step 1: Write failing tests** (full test content in plan file, run to verify fail)

- [ ] **Step 2: Create AuthResult shared response type**

```csharp
// Identity/Common/AuthResult.cs
namespace NursingPlatform.Application.Identity.Common;
public class AuthResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
```

- [ ] **Step 3: Create Login commands**

```csharp
// LoginCommand.cs
using MediatR;
using NursingPlatform.Application.Identity.Common;
namespace NursingPlatform.Application.Identity.Commands.Login;
public class LoginCommand : IRequest<AuthResult>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
```

```csharp
// LoginCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Identity.Common;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Application.Identity.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IApplicationDbContext context, IPasswordHashingService passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var tokenHash = ComputeSha256Hash(refreshToken);

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow
        });

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResult { AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = DateTime.UtcNow.AddMinutes(15) };
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

```csharp
// LoginCommandValidator.cs, LoginRequest.cs, LoginResponse.cs
// (standard FluentValidation and DTO classes mirroring spec)
```

- [ ] **Step 4: Create RotateRefreshToken commands** (mirror spec — handler validates via IJwtService, revokes old, issues new)

- [ ] **Step 5: Build + test**

Run: `dotnet build && dotnet test` — Expected: 0 errors, all tests pass

- [ ] **Step 6: Commit**

---
### Task 6: Infrastructure Services (JwtService, PasswordHashingService)

**Files:**
- Create: `backend/src/NursingPlatform.Infrastructure/Authentication/JwtService.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Authentication/PasswordHashingService.cs`
- Test: PasswordHashingService unit tests (hash/verify correctness)

- [ ] **Step 1: Write failing PasswordHashingService tests**

```csharp
// Tests in Domain.Tests or new Infrastructure.Tests project
[Fact] public void Hash_ProducesNonEmptyResult() { ... }
[Fact] public void Verify_CorrectPassword_ReturnsTrue() { ... }
[Fact] public void Verify_WrongPassword_ReturnsFalse() { ... }
[Fact] public void Hash_SamePassword_DifferentHashes() { ... }
```

- [ ] **Step 2: Create PasswordHashingService.cs**

```csharp
using Microsoft.AspNetCore.Identity;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Domain.Identity;

namespace NursingPlatform.Infrastructure.Authentication;

public class PasswordHashingService : IPasswordHashingService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password) => _passwordHasher.HashPassword(null!, password);

    public bool Verify(string password, string hash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
```

- [ ] **Step 3: Create JwtService.cs** (full implementation per spec — inject IOptions<JwtSettings> and IApplicationDbContext for ValidateRefreshToken)

- [ ] **Step 4: Build + test**

- [ ] **Step 5: Commit**

---
### Task 7: BootstrapAdminSettings + ReferenceDataSeeder Update + DI Registration

**Files:**
- Create: `backend/src/NursingPlatform.Infrastructure/Configuration/BootstrapAdminSettings.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/DatabaseInitializer.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Create BootstrapAdminSettings.cs**

```csharp
namespace NursingPlatform.Infrastructure.Configuration;
public class BootstrapAdminSettings
{
    public const string SectionName = "BootstrapAdmin";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Update ReferenceDataSeeder.cs**

Add bootstrap admin user creation after seeding roles/permissions. Update `SeedAsync` signature to accept `IPasswordHashingService` and `BootstrapAdminSettings`. Create first SuperAdmin user if no users exist.

- [ ] **Step 3: Update DatabaseInitializer.cs**

Inject `IPasswordHashingService` and `IOptions<BootstrapAdminSettings>`, pass to `ReferenceDataSeeder.SeedAsync()`.

- [ ] **Step 4: Update DependencyInjection.cs**

```csharp
services.AddOptions<BootstrapAdminSettings>()
    .Bind(configuration.GetSection(BootstrapAdminSettings.SectionName))
    .ValidateDataAnnotations();

services.AddScoped<IPasswordHashingService, PasswordHashingService>();
services.AddScoped<IJwtService, JwtService>();
```

- [ ] **Step 5: Build**

Run: `dotnet build` — Expected: 0 errors

- [ ] **Step 6: Commit**

---
### Task 8: WebApi — Authentication Pipeline + Endpoints

**Files:**
- Create: `backend/src/NursingPlatform.WebApi/Endpoints/AuthEndpoints.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ServiceCollectionExtensions.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Modify: `backend/src/NursingPlatform.WebApi/appsettings.Development.json`

- [ ] **Step 1: Create AuthEndpoints.cs**

Map `/api/v1/auth/login` (AllowAnonymous), `/api/v1/auth/refresh` (AllowAnonymous), `/api/v1/auth/register` (RequireAuthorization). Map HTTP requests to Commands via `ISender`.

- [ ] **Step 2: Update ServiceCollectionExtensions.cs**

Add JWT Bearer authentication and authorization services. Read config from `JwtSettings` section.

- [ ] **Step 3: Update ApplicationBuilderExtensions.cs**

Add `app.UseAuthentication()` and `app.UseAuthorization()` after `UseHttpsRedirection()`. Wire `MapApiEndpoints` to include auth endpoints.

- [ ] **Step 4: Update appsettings.Development.json**

Add BootstrapAdmin section:
```json
"BootstrapAdmin": {
    "Email": "admin@nursingplatform.com",
    "Password": "Admin123!"
}
```

- [ ] **Step 5: Build**

Run: `dotnet build` — Expected: 0 errors

- [ ] **Step 6: Commit**

---
### Task 9: Integration Tests

**Files:**
- Create: `tests/NursingPlatform.WebApi.IntegrationTests/NursingPlatform.WebApi.IntegrationTests.csproj`
- Create: `tests/NursingPlatform.WebApi.IntegrationTests/Authentication/AuthEndpointsTests.cs`

- [ ] **Step 1: Create Integration Tests project** (xUnit + Microsoft.AspNetCore.Mvc.Testing + Testcontainers.PostgreSql)

- [ ] **Step 2: Write integration tests**

- Login with valid credentials returns JWT and refresh token (200)
- Invalid credentials return 401
- Register without auth returns 401
- Refresh token rotation works end-to-end
- Old refresh token rejected after rotation

- [ ] **Step 3: Run tests**

Run: `dotnet test tests/NursingPlatform.WebApi.IntegrationTests -v` — Expected: PASS

- [ ] **Step 4: Commit**

---
### Task 10: Final Build + Test + Documentation

- [ ] **Step 1: Full build**

Run: `dotnet build` — Expected: 0 errors

- [ ] **Step 2: All tests**

Run: `dotnet test` — Expected: All pass

- [ ] **Step 3: Update CURRENT_TASK.md**

Set milestone to "Phase 4A — Core Identity", mark items complete.

- [ ] **Step 4: Commit**

```bash
git add CURRENT_TASK.md
git commit -m "docs: update CURRENT_TASK.md for Phase 4A completion"
```
