# Phase 4B — Authorization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement hybrid RBAC permission authorization using ASP.NET Core's `IAuthorizationHandler` pipeline.

**Architecture:** Dynamic `PermissionRequirement` + `PermissionAuthorizationHandler`. Static permission constants in Application layer. `ICurrentUserService` (Application interface, Infrastructure implementation). `IPermissionService` resolves user permissions via `IApplicationDbContext` in a single EF query.

**Tech Stack:** .NET 10, ASP.NET Core Authorization, MediatR, EF Core, Moq, xUnit

## Global Constraints

- Permission names must match existing seed data format: `"{Module}.{Action}"` with PascalCase groups (e.g. `"Users.Create"`, `"Roles.View"`).
- No custom authorization middleware. ASP.NET Core `IAuthorizationHandler` pipeline only.
- No named policies per permission — single dynamic `PermissionRequirement`.
- `PermissionAuthorizationHandler` is `AddScoped` (depends on scoped `ICurrentUserService` and `IPermissionService`).
- `CurrentUserService` returns safe defaults (null/empty) for unauthenticated requests — never throws.
- `CurrentUserService.UserId` reads `ClaimTypes.NameIdentifier` first, then falls back to `JwtRegisteredClaimNames.Sub` / `"sub"`. `JwtService` emits `"sub"`; the dual-lookup is robust regardless of inbound claim mapping configuration.
- Permission resolution is a single EF query via navigation properties: `UserRoles → Role → RolePermissions → Permission.Name`.
- `RegisterUserCommand` returns `Guid` (implements `IRequest<Guid>`). Endpoint wraps in `RegisterUserResponse { UserId = result }`.
- Integration tests generate real JWTs via `JwtService` with test `JwtSettings` and mock `IPermissionService` in `WebApiTestFactory`.
- All timestamps UTC. All entities Guid PK. Plural PascalCase table names.
- Follow existing project conventions (file placement, naming, dependency injection patterns).

---

### Task 1: Add Navigation Properties and Entity Configurations

**Files:**
- Modify: `backend/src/NursingPlatform.Domain/ReferenceData/Role.cs`
- Modify: `backend/src/NursingPlatform.Domain/ReferenceData/Permission.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RoleConfiguration.cs`

**Interfaces:**
- Consumes: Existing `Role`, `Permission`, `RolePermission` entities
- Produces: `Role.RolePermissions` (ICollection), `Permission.RolePermissions` (ICollection), EF config for both

- [ ] **Step 1: Add `RolePermissions` nav to Role.cs**

Add property after `UserRoles`:

```csharp
public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
```

- [ ] **Step 2: Add `RolePermissions` nav to Permission.cs**

```csharp
public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
```

- [ ] **Step 3: Add RolePermissions configuration to RoleConfiguration.cs**

Add after existing `UserRoles` config:

```csharp
builder.HasMany(r => r.RolePermissions)
    .WithOne(rp => rp.Role)
    .HasForeignKey(rp => rp.RoleId)
    .OnDelete(DeleteBehavior.Restrict);
```

- [ ] **Step 4: Create PermissionConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 6: Commit**

```bash
git add backend/src/NursingPlatform.Domain/ReferenceData/Role.cs backend/src/NursingPlatform.Domain/ReferenceData/Permission.cs backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RoleConfiguration.cs
git commit -m "feat: add RolePermissions navigation properties and EF configuration"
```

---

### Task 2: Expose Permissions and RolePermissions in IApplicationDbContext

**Files:**
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`

**Interfaces:**
- Consumes: `Permission` entity, `RolePermission` entity
- Produces: `IApplicationDbContext.Permissions`, `IApplicationDbContext.RolePermissions`

- [ ] **Step 1: Add DbSet properties to IApplicationDbContext**

Add two lines to the interface:

```csharp
DbSet<Permission> Permissions { get; }
DbSet<RolePermission> RolePermissions { get; }
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
git add backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs
git commit -m "feat: expose Permissions and RolePermissions in IApplicationDbContext"
```

---

### Task 3: Create Permission Constants

**Files:**
- Create: `backend/src/NursingPlatform.Application/Authorization/Permissions.cs`

**Interfaces:**
- Produces: `Permissions` static class with nested groups, `Permissions.All`, `Permissions.Admin`

- [ ] **Step 1: Write failing test for permission constants**

```csharp
// New test file: Application.Tests/Authorization/PermissionsTests.cs
using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionsTests
{
    [Fact]
    public void All_ShouldContainAllDefinedPermissions()
    {
        var expected = new HashSet<string>
        {
            "Users.Create", "Users.View", "Users.Edit", "Users.Delete",
            "Roles.View", "Roles.Manage",
            "Permissions.View", "Permissions.Manage",
            "Countries.View", "Countries.Manage",
            "Languages.View", "Languages.Manage",
            "Exams.View", "Exams.Create", "Exams.Edit", "Exams.Delete",
            "Questions.View", "Questions.Manage",
            "Nurses.View",
            "Employers.View"
        };

        Assert.True(expected.SetEquals(Permissions.All));
    }

    [Fact]
    public void Admin_ShouldContainAllPermissions()
    {
        Assert.Equal(Permissions.All, Permissions.Admin);
    }
}
```

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionsTests"` — Expected: FAIL

- [ ] **Step 2: Create Permissions.cs**

```csharp
namespace NursingPlatform.Application.Authorization;

public static class Permissions
{
    public static class Users
    {
        public const string Create = "Users.Create";
        public const string View = "Users.View";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Manage = "Roles.Manage";
    }

    public static class PermissionsGroup
    {
        public const string View = "Permissions.View";
        public const string Manage = "Permissions.Manage";
    }

    public static class Countries
    {
        public const string View = "Countries.View";
        public const string Manage = "Countries.Manage";
    }

    public static class Languages
    {
        public const string View = "Languages.View";
        public const string Manage = "Languages.Manage";
    }

    public static class Exams
    {
        public const string View = "Exams.View";
        public const string Create = "Exams.Create";
        public const string Edit = "Exams.Edit";
        public const string Delete = "Exams.Delete";
    }

    public static class Questions
    {
        public const string View = "Questions.View";
        public const string Manage = "Questions.Manage";
    }

    public static class Nurses
    {
        public const string View = "Nurses.View";
    }

    public static class Employers
    {
        public const string View = "Employers.View";
    }

    public static readonly string[] All =
    [
        Users.Create, Users.View, Users.Edit, Users.Delete,
        Roles.View, Roles.Manage,
        PermissionsGroup.View, PermissionsGroup.Manage,
        Countries.View, Countries.Manage,
        Languages.View, Languages.Manage,
        Exams.View, Exams.Create, Exams.Edit, Exams.Delete,
        Questions.View, Questions.Manage,
        Nurses.View,
        Employers.View
    ];

    public static readonly string[] Admin = All;
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionsTests"` — Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add backend/src/NursingPlatform.Application/Authorization/Permissions.cs backend/tests/NursingPlatform.Application.Tests/Authorization/PermissionsTests.cs
git commit -m "feat: add permission constants"
```

---

### Task 4: Create ICurrentUserService

**Files:**
- Create: `backend/src/NursingPlatform.Application/Abstractions/Auth/ICurrentUserService.cs`

**Interfaces:**
- Produces: `ICurrentUserService` interface with `UserId`, `Email`, `Roles`, `IsAuthenticated`

- [ ] **Step 1: Create ICurrentUserService**

```csharp
namespace NursingPlatform.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
```

- [ ] **Step 2: Commit**

```bash
git add backend/src/NursingPlatform.Application/Abstractions/Auth/ICurrentUserService.cs
git commit -m "feat: add ICurrentUserService interface"
```

---

### Task 5: Create PermissionRequirement

**Files:**
- Create: `backend/src/NursingPlatform.Application/Authorization/PermissionRequirement.cs`

**Interfaces:**
- Produces: `PermissionRequirement : IAuthorizationRequirement` with `Permission` property

- [ ] **Step 1: Write failing tests**

```csharp
// New test file: Application.Tests/Authorization/PermissionRequirementTests.cs
using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionRequirementTests
{
    [Fact]
    public void Constructor_ShouldSetPermission()
    {
        var requirement = new PermissionRequirement("Users.Create");
        Assert.Equal("Users.Create", requirement.Permission);
    }

    [Fact]
    public void Constructor_NullPermission_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new PermissionRequirement(null!));
    }
}
```

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionRequirement"` — Expected: FAIL

- [ ] **Step 2: Create PermissionRequirement**

```csharp
using Microsoft.AspNetCore.Authorization;

namespace NursingPlatform.Application.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionRequirement"` — Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add backend/src/NursingPlatform.Application/Authorization/PermissionRequirement.cs backend/tests/NursingPlatform.Application.Tests/Authorization/PermissionRequirementTests.cs
git commit -m "feat: add PermissionRequirement"
```

---

### Task 6: Create IPermissionService and PermissionService

**Files:**
- Create: `backend/src/NursingPlatform.Application/Authorization/IPermissionService.cs`
- Create: `backend/src/NursingPlatform.Application/Authorization/PermissionService.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext` (with `UserRoles`, `RolePermissions`, `Permission` exposed)
- Produces: `IPermissionService.GetUserPermissionsAsync(Guid userId, CancellationToken) -> Task<HashSet<string>>`

- [ ] **Step 1: Write failing test for PermissionService**

```csharp
// New test file: Application.Tests/Authorization/PermissionServiceTests.cs
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NursingPlatform.Application.Abstractions.Data;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionServiceTests
{
    [Fact]
    public async Task GetUserPermissionsAsync_ShouldReturnDistinctPermissions()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permCreate = new Permission { Id = Guid.NewGuid(), Name = "Users.Create" };
        var permView = new Permission { Id = Guid.NewGuid(), Name = "Users.View" };

        var userRoles = new List<UserRole>
        {
            new()
            {
                UserId = userId,
                RoleId = roleId,
                Role = new Role
                {
                    Id = roleId,
                    RolePermissions = new List<RolePermission>
                    {
                        new() { PermissionId = permCreate.Id, Permission = permCreate },
                        new() { PermissionId = permView.Id, Permission = permView }
                    }
                }
            }
        }.AsQueryable().BuildMockDbSet();

        var contextMock = new Mock<IApplicationDbContext>();
        contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = new PermissionService(contextMock.Object);
        var result = await service.GetUserPermissionsAsync(userId);

        Assert.Contains("Users.Create", result);
        Assert.Contains("Users.View", result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_UserWithNoRoles_ShouldReturnEmpty()
    {
        var userId = Guid.NewGuid();
        var userRoles = new List<UserRole>().AsQueryable().BuildMockDbSet();

        var contextMock = new Mock<IApplicationDbContext>();
        contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

        var service = new PermissionService(contextMock.Object);
        var result = await service.GetUserPermissionsAsync(userId);

        Assert.Empty(result);
    }
}
```

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionService"` — Expected: FAIL

- [ ] **Step 2: Create IPermissionService**

```csharp
namespace NursingPlatform.Application.Authorization;

public interface IPermissionService
{
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Create PermissionService**

```csharp
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Data;

namespace NursingPlatform.Application.Authorization;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<string>(permissions);
    }
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionService"` — Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add backend/src/NursingPlatform.Application/Authorization/IPermissionService.cs backend/src/NursingPlatform.Application/Authorization/PermissionService.cs backend/tests/NursingPlatform.Application.Tests/Authorization/PermissionServiceTests.cs
git commit -m "feat: add PermissionService"
```

---

### Task 7: Create PermissionAuthorizationHandler

**Files:**
- Create: `backend/src/NursingPlatform.Application/Authorization/PermissionAuthorizationHandler.cs`

**Interfaces:**
- Consumes: `ICurrentUserService`, `IPermissionService`
- Produces: `PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>`

- [ ] **Step 1: Write failing tests**

```csharp
// New test file: Application.Tests/Authorization/PermissionAuthorizationHandlerTests.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IPermissionService> _permissionServiceMock = new();
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _handler = new PermissionAuthorizationHandler(
            _currentUserMock.Object, _permissionServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_UnauthenticatedUser_ShouldNotSucceed()
    {
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(false);

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement("Users.Create")],
            new ClaimsPrincipal(new ClaimsIdentity()),
            null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserWithPermission_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(u => u.UserId).Returns(userId);
        _permissionServiceMock
            .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.Create" });

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement("Users.Create")],
            new ClaimsPrincipal(new ClaimsIdentity("test")),
            null);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserWithoutPermission_ShouldNotSucceed()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.SetupGet(u => u.IsAuthenticated).Returns(true);
        _currentUserMock.SetupGet(u => u.UserId).Returns(userId);
        _permissionServiceMock
            .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "Users.View" });

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement("Users.Create")],
            new ClaimsPrincipal(new ClaimsIdentity("test")),
            null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
```

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionAuthorizationHandler"` — Expected: FAIL

- [ ] **Step 2: Create PermissionAuthorizationHandler**

```csharp
using Microsoft.AspNetCore.Authorization;
using NursingPlatform.Application.Abstractions.Auth;

namespace NursingPlatform.Application.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(
        ICurrentUserService currentUser,
        IPermissionService permissionService)
    {
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return;

        var permissions = await _permissionService.GetUserPermissionsAsync(_currentUser.UserId.Value);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PermissionAuthorizationHandler"` — Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add backend/src/NursingPlatform.Application/Authorization/PermissionAuthorizationHandler.cs backend/tests/NursingPlatform.Application.Tests/Authorization/PermissionAuthorizationHandlerTests.cs
git commit -m "feat: add PermissionAuthorizationHandler"
```

---

### Task 8: Create CurrentUserService (Infrastructure Implementation)

**Files:**
- Create: `backend/src/NursingPlatform.Infrastructure/Authentication/CurrentUserService.cs`

**Interfaces:**
- Consumes: `IHttpContextAccessor`, `ICurrentUserService` interface
- Produces: `CurrentUserService : ICurrentUserService`

- [ ] **Step 1: Write failing tests**

```csharp
// New test file: Infrastructure.Tests/Authentication/CurrentUserServiceTests.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NursingPlatform.Infrastructure.Authentication;

namespace NursingPlatform.Infrastructure.Tests.Authentication;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_NoHttpContext_ReturnsNull()
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupGet(h => h.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.Null(service.UserId);
    }

    [Fact]
    public void IsAuthenticated_NoIdentity_ReturnsFalse()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupGet(h => h.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void UserId_WithNameIdentifierClaim_ReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "test");

        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupGet(h => h.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.Equal(userId, service.UserId);
    }

    [Fact]
    public void Email_WithEmailClaim_ReturnsValue()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@test.com")
        }, "test");

        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupGet(h => h.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.Equal("user@test.com", service.Email);
    }

    [Fact]
    public void Roles_WithRoleClaims_ReturnsList()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Nurse")
        }, "test");

        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupGet(h => h.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.Equal(2, service.Roles.Count);
        Assert.Contains("Admin", service.Roles);
        Assert.Contains("Nurse", service.Roles);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupGet(h => h.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.True(service.IsAuthenticated);
    }
}
```

Run: `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/ --filter "CurrentUserService"` — Expected: FAIL

- [ ] **Step 2: Create CurrentUserService**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NursingPlatform.Application.Abstractions.Auth;

namespace NursingPlatform.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return null;

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirst("sub");

            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public IReadOnlyList<string> Roles
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role);
            return claims?.Select(c => c.Value).ToList().AsReadOnly() ?? [];
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/ --filter "CurrentUserService"` — Expected: PASS

- [ ] **Step 4: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 5: Commit**

```bash
git add backend/src/NursingPlatform.Infrastructure/Authentication/CurrentUserService.cs backend/tests/NursingPlatform.Infrastructure.Tests/Authentication/CurrentUserServiceTests.cs
git commit -m "feat: add CurrentUserService implementation"
```

---

### Task 9: Register Services in Dependency Injection

**Files:**
- Modify: `backend/src/NursingPlatform.Application/DependencyInjection.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs`

**Interfaces:**
- Consumes: `IPermissionService`, `PermissionService`, `PermissionAuthorizationHandler`, `ICurrentUserService`, `CurrentUserService`
- Produces: Registered services

- [ ] **Step 1: Add Application layer registrations**

Edit `Application/DependencyInjection.cs`:

```csharp
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NursingPlatform.Application.Authorization;

namespace NursingPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
```

- [ ] **Step 2: Add Infrastructure layer registration**

Edit `Infrastructure/DependencyInjection.cs` — add after existing auth service registrations (line 75):

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 4: Commit**

```bash
git add backend/src/NursingPlatform.Application/DependencyInjection.cs backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs
git commit -m "feat: register authorization services in DI"
```

---

### Task 10: Seed RolePermission Join Records

**Files:**
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs`

**Interfaces:**
- Consumes: `ApplicationDbContext` with `Roles`, `Permissions`, `RolePermissions`
- Produces: Seeded `RolePermission` join records for Admin and SuperAdmin roles

**Important:** Do not duplicate existing seed data (countries, languages, roles, permissions). Only the snippet below is added to the existing file.

- [ ] **Step 1: Add RolePermission seeding after existing SaveChangesAsync**

Insert the following code after the existing `await context.SaveChangesAsync();` (currently line 75) that saves countries, languages, roles, and permissions:

```csharp
var adminRoleId = new Guid("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F");
var superAdminRoleId = new Guid("F8091A2B-3C4D-45E6-F708-192A3B4C5D6E");

var allPermissions = await context.Set<Permission>().ToListAsync();

foreach (var roleId in new[] { adminRoleId, superAdminRoleId })
{
    var existing = await context.Set<RolePermission>()
        .Where(rp => rp.RoleId == roleId)
        .Select(rp => rp.PermissionId)
        .ToListAsync();

    var newAssignments = allPermissions
        .Where(p => !existing.Contains(p.Id))
        .Select(p => new RolePermission { RoleId = roleId, PermissionId = p.Id })
        .ToList();

    if (newAssignments.Count > 0)
    {
        context.Set<RolePermission>().AddRange(newAssignments);
    }
}

await context.SaveChangesAsync();
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
git add backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs
git commit -m "feat: seed RolePermission join records for Admin and SuperAdmin roles"
```

---

### Task 11: Map Register Endpoint with PermissionRequirement

**Files:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`

**Interfaces:**
- Consumes: `RegisterUserCommand`, `RegisterUserRequest`, `RegisterUserResponse`, `PermissionRequirement`, `Permissions.Users.Create`
- Produces: `POST /api/v1/auth/register` endpoint protected by `PermissionRequirement`

- [ ] **Step 1: Add register endpoint to MapApiEndpoints**

Add using directives at the top of the file:

```csharp
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Identity.Commands.Register;
```

Add the register endpoint before `return app;` in `MapApiEndpoints`:

```csharp
api.MapPost("/auth/register", async (RegisterUserRequest request, ISender sender) =>
{
    var command = new RegisterUserCommand
    {
        Email = request.Email,
        Password = request.Password,
        FirstName = request.FirstName,
        LastName = request.LastName,
        RoleIds = request.RoleIds
    };

    var userId = await sender.Send(command);
    return Results.Ok(new RegisterUserResponse { UserId = userId });
})
.WithName("RegisterUser")
.RequireAuthorization(pb => pb.Requirements.Add(
    new PermissionRequirement(Permissions.Users.Create)));
```

Note: `RegisterUserCommand` is `IRequest<Guid>` (handler returns `Guid` directly). The endpoint wraps it in `RegisterUserResponse` for a consistent JSON shape.

- [ ] **Step 2: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
git add backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs
git commit -m "feat: map /auth/register endpoint with PermissionRequirement"
```

---

### Task 12: Add Integration Tests for Register Endpoint Authorization

**Files:**
- Modify: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/WebApiTestFactory.cs` (add mock IPermissionService)
- Modify: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AuthEndpointTests.cs` (add tests)

**Strategy:**
- `WebApiTestFactory` exposes a public `Mock<IPermissionService>` field, mocked via `ConfigureTestServices`.
- Tests generate real JWTs using `JwtService` constructed with test config values (available as environment variables).
- Three test scenarios: unauthenticated (401), authenticated without permission (403), authenticated with permission (200).

- [ ] **Step 1: Add IPermissionService mock to WebApiTestFactory**

Add using:

```csharp
using NursingPlatform.Application.Authorization;
```

Add field after `SenderMock`:

```csharp
public Mock<IPermissionService> PermissionServiceMock { get; } = new();
```

Add registration in `ConfigureTestServices`, after the existing `DatabaseInitializer` mock:

```csharp
services.AddScoped(_ => PermissionServiceMock.Object);
```

- [ ] **Step 2: Add register endpoint auth tests to AuthEndpointTests**

Add using at the top:

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Identity.Commands.Register;
using NursingPlatform.Domain.Identity;
using NursingPlatform.Infrastructure.Authentication;
using NursingPlatform.Infrastructure.Configuration;
```

Add a private helper to generate test JWTs:

```csharp
private static string GenerateTestJwt(Guid userId, string[] roles)
{
    var settings = Options.Create(new JwtSettings
    {
        Secret = "test-secret-key-that-is-at-least-32-characters-long",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpirationInMinutes = 60,
        RefreshTokenExpirationInDays = 7
    });

    var jwtService = new JwtService(settings);
    var user = new User
    {
        Id = userId,
        Email = "test@test.com",
        FirstName = "Test",
        LastName = "User",
        PasswordHash = "hash"
    };

    var result = jwtService.GenerateAccessToken(user, roles);
    return result.Token;
}
```

Add test methods to the `AuthEndpointTests` class:

```csharp
[Fact]
public async Task Register_WithoutAuthToken_Returns401()
{
    var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
        new { email = "new@test.com", password = "Password1", firstName = "New", lastName = "User", roleIds = new List<Guid>() });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}

[Fact]
public async Task Register_WithTokenButNoPermission_Returns403()
{
    var userId = Guid.NewGuid();
    var token = GenerateTestJwt(userId, ["SomeRole"]);

    _factory.PermissionServiceMock
        .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new HashSet<string>());

    var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register")
    {
        Content = JsonContent.Create(new { email = "new@test.com", password = "Password1", firstName = "New", lastName = "User", roleIds = new List<Guid> { Guid.NewGuid() } })
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await _client.SendAsync(request);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}

[Fact]
public async Task Register_WithTokenAndPermission_Returns200()
{
    var userId = Guid.NewGuid();
    var token = GenerateTestJwt(userId, ["Admin"]);

    var expectedUserId = Guid.NewGuid();
    _factory.PermissionServiceMock
        .Setup(p => p.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new HashSet<string> { "Users.Create" });

    _factory.SenderMock
        .Setup(s => s.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedUserId);

    var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register")
    {
        Content = JsonContent.Create(new { email = "new@test.com", password = "Password1", firstName = "New", lastName = "User", roleIds = new List<Guid> { Guid.NewGuid() } })
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await _client.SendAsync(request);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
    Assert.NotNull(body);
    Assert.Equal(expectedUserId, body.UserId);
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.WebApi.Tests/` — Expected: PASS (existing 6 tests + 3 new = 9 tests)

- [ ] **Step 4: Commit**

```bash
git add backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/WebApiTestFactory.cs backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AuthEndpointTests.cs
git commit -m "test: add register endpoint authorization tests"
```

---

### Task 13: Update Documentation

**Files:**
- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

- [ ] **Step 1: Update CURRENT_TASK.md to mark Phase 4B complete**

- [ ] **Step 2: Update TASKS.md to mark Phase 4B complete**

- [ ] **Step 3: Commit docs**

```bash
git add CURRENT_TASK.md TASKS.md
git commit -m "docs: update for Phase 4B authorization"
```

---

## Self-Review Checklist

- **Spec coverage** — All requirements from `docs/superpowers/specs/2026-07-08-authorization-design.md` covered:
  - Navigation properties on Role and Permission (Task 1)
  - PermissionConfiguration (Task 1)
  - IApplicationDbContext Permissions/RolePermissions (Task 2)
  - Permission constants matching seed data (Task 3)
  - ICurrentUserService (Task 4, 8)
  - PermissionRequirement (Task 5)
  - IPermissionService + PermissionService (Task 6)
  - PermissionAuthorizationHandler (Task 7)
  - DI registration as scoped (Task 9)
  - RolePermission seed without duplicating seed data (Task 10)
  - Register endpoint with RegisterUserResponse (Task 11)
  - Integration tests with real JWT + mock IPermissionService (Task 12)
- **Placeholder scan** — No "TBD", "TODO", "implement later", or similar patterns.
- **Type consistency** — `PermissionRequirement.Permission` is `string`; `IPermissionService.GetUserPermissionsAsync` returns `Task<HashSet<string>>`; constants match seed data PascalCase format; handler registered as scoped everywhere.
- **No duplicated GUIDs** — Task 10 only adds the delta (RolePermission joins), does not copy existing seed data.
