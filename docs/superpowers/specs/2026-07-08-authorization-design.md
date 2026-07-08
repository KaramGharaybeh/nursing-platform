# Phase 4B — Authorization Design

## Overview

Implement a hybrid RBAC (Role-Based Access Control) permission authorization system using ASP.NET Core's built-in `IAuthorizationHandler` pipeline. Permissions are assigned to roles; users inherit permissions through their role memberships. Enforcement happens at the endpoint level via `PermissionRequirement`.

The design follows all existing patterns established in Phase 4A: Clean Architecture, CQRS, no custom middleware, no ASP.NET Core Identity framework.

## Sub-Phase Breakdown

- Phase 4A — Core Identity (done): User entity, password hashing, JWT, login, refresh tokens, admin-only user creation
- **Phase 4B — Authorization (current):** Permission model, RolePermission seeding, permission-checking service, authorization handler, endpoint enforcement
- Phase 4C — Account Management: Email verification, password reset, remaining lifecycle features

---

## Architecture

### Approach

Dynamic `PermissionRequirement` + `PermissionAuthorizationHandler` pattern:

```
Endpoint (RequireAuthorization with PermissionRequirement)
    ↓
ASP.NET Core AuthorizationMiddleware
    ↓
PermissionAuthorizationHandler (scoped)
    ↓                    ↓
ICurrentUserService   PermissionService (resolves user permissions from DB/context)
    ↓
IApplicationDbContext
```

### Key Decisions

- **No custom authorization middleware.** Use ASP.NET Core `IAuthorizationHandler` pipeline only.
- **Static Permission constants.** Defined in the Application layer as a flat `Permissions` class with nested groups. Constants match existing seed data exactly (PascalCase format, e.g. `"Users.Create"`).
- **Per-request permission resolution.** `PermissionAuthorizationHandler` resolves the user's effective permission set once per request via `PermissionService` (scoped).
- **Handler registered as scoped.** `PermissionAuthorizationHandler` depends on `ICurrentUserService` and `IPermissionService` (both scoped). `AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()`.
- **No named policies per permission.** A single dynamic `PermissionRequirement` carries the permission name. No policy registration ceremony per permission.
- **ICurrentUserService** in Application layer (interface), implemented in Infrastructure layer via `IHttpContextAccessor` — same pattern as `IJwtService`.
- **Permission seeding.** `ReferenceDataSeeder` extended to assign permissions to roles via `RolePermission` join records. Admin role gets all permissions. SuperAdmin role also gets all permissions.
- **Claim fallback:** `JwtService` emits the user ID as `JwtRegisteredClaimNames.Sub` (`"sub"`). `CurrentUserService` reads `ClaimTypes.NameIdentifier` first, then `JwtRegisteredClaimNames.Sub` as fallback. This is robust whether or not ASP.NET Core's inbound claim mapping is active.

---

## Domain Layer

### Permission Entity (existing, no changes)

```
Permission : AuditableEntity
  - Id : Guid
  - Name : string         // e.g. "Users.Create"
  - Description : string?

  Navigation:
  - RolePermissions : ICollection<RolePermission>  ← ADD if missing
```

Already exists in `Domain/Authorization/Permission.cs`. Add `RolePermissions` navigation if absent.

### RolePermission Entity (existing, no changes)

```
RolePermission
  - RoleId : Guid (PK component, FK → Role)
  - PermissionId : Guid (PK component, FK → Permission)

  Navigation:
  - Role : Role
  - Permission : Permission
```

Already exists in `Domain/Authorization/RolePermission.cs`. No changes needed.

### Role Entity — Add RolePermissions Navigation

Currently `Role.cs` has `UserRoles` but is missing `RolePermissions` navigation:

```
Role : AuditableEntity
  - Id : Guid
  - Name : string
  - Description : string?

  Navigation:
  - UserRoles : ICollection<UserRole>
  - RolePermissions : ICollection<RolePermission>   ← ADD
```

---

## Application Layer

### Permission Constants

New file: `Application/Authorization/Permissions.cs`

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

- Constants use PascalCase (e.g. `"Users.Create"`) matching existing `Permission.Name` values in seed data.
- `PermissionGroup` is named that way because `Permissions` would conflict with the containing class name.
- `All` and `Admin` arrays provide convenience for seeding and testing.

### ICurrentUserService

New file: `Application/Abstractions/Auth/ICurrentUserService.cs`

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

- Minimal set of properties. Permissions are resolved by `PermissionService`, not exposed here.
- Return empty/zero-value defaults when the user is not authenticated (never throws).

### PermissionService

New file: `Application/Authorization/PermissionService.cs`

```csharp
namespace NursingPlatform.Application.Authorization;

public interface IPermissionService
{
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
}
```

- Returns a flat deduplicated set of permission names for a given user.
- Resolution logic: query `RolePermissions` via `UserRoles` → collect distinct `Permission.Name` values.
- Single query, no caching (per-request scope means in-memory within the handler).

### IApplicationDbContext — Add Permission Query Access

The existing `IApplicationDbContext` interface exposes `Roles` but is missing `Permissions` and `RolePermissions`:

```csharp
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }       // ← ADD
    DbSet<RolePermission> RolePermissions { get; }  // ← ADD
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### Authorization Requirement

New file: `Application/Authorization/PermissionRequirement.cs`

```csharp
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

- Simple marker with the permission name string.
- Placed in the Application layer because authorization requirements belong to the Application layer (per backend-architecture.md).

### Authorization Handler

New file: `Application/Authorization/PermissionAuthorizationHandler.cs`

```csharp
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

- If user is not authenticated, does not call `Succeed` (falls through to failure).
- Resolves permissions once per request.
- Registered as scoped because it depends on scoped services.

### PermissionService Implementation

`PermissionService` is implemented in the Application layer because it depends only on `IApplicationDbContext`, an Application-layer interface.

```csharp
public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(ct);

        return new HashSet<string>(permissions);
    }
}
```

- Single EF Core query via navigation properties.
- No case-insensitive comparison needed — permission names are consistently PascalCase.

---

## Infrastructure Layer

### CurrentUserService Implementation

New file: `Infrastructure/Authentication/CurrentUserService.cs`

```csharp
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

- Uses `ClaimTypes.NameIdentifier` first, then falls back to `JwtRegisteredClaimNames.Sub` (`"sub"`). `JwtService` emits `"sub"`. The JWT handler usually maps `"sub"` → `ClaimTypes.NameIdentifier`, but relying solely on implicit mapping is fragile if `MapInboundClaims` changes. The dual-lookup makes `CurrentUserService` robust regardless of claim mapping configuration.

### Seed Data — RolePermission Join Records

Extend `ReferenceDataSeeder.SeedAsync()` to assign permissions to roles. Only the role-permission mapping code is added; the existing permission seed data is not duplicated:

```csharp
// After the existing SaveChangesAsync that saves roles and permissions:

var adminRoleId = new Guid("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F");
var superAdminRoleId = new Guid("F8091A2B-3C4D-45E6-F708-192A3B4C5D6E");

var adminPermissions = await context.Set<Permission>().ToListAsync();

foreach (var roleId in new[] { adminRoleId, superAdminRoleId })
{
    var existing = await context.Set<RolePermission>()
        .Where(rp => rp.RoleId == roleId)
        .Select(rp => rp.PermissionId)
        .ToListAsync();

    var newAssignments = adminPermissions
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

- Idempotent: checks for existing assignments before adding.
- Both Admin and SuperAdmin roles get all permissions.
- Uses the same existing `adminRoleId` GUID from the existing seeder.

### Dependency Injection

#### Infrastructure Layer (`Infrastructure/DependencyInjection.cs`)

Add registrations:

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();
```

#### Application Layer (`Application/DependencyInjection.cs`)

Add registrations:

```csharp
services.AddScoped<IPermissionService, PermissionService>();
services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

(Handler is `AddScoped` because it depends on `ICurrentUserService` and `IPermissionService`, both scoped.)

### Authorization Policy Registration (`WebApi/Extensions/ServiceCollectionExtensions.cs`)

No changes needed — `services.AddAuthorization()` is already called in `AddPresentation`.

---

## WebApi Layer

### Endpoint Authorization

Apply `RequireAuthorization` with `PermissionRequirement` to protected endpoints:

#### `POST /api/v1/auth/register` (Admin-only user creation)

```csharp
group.MapPost("/auth/register", async (RegisterUserRequest request, ISender sender) =>
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
.RequireAuthorization(pb => pb.Requirements.Add(new PermissionRequirement(Permissions.Users.Create)));
```

- `RegisterUserCommand` implements `IRequest<Guid>` — the handler returns `Guid`, not `RegisterUserResponse`. The endpoint wraps the result in `RegisterUserResponse` for a consistent JSON response shape.

#### Future endpoints will follow the same pattern:

```csharp
.RequireAuthorization(pb => pb.Requirements.Add(new PermissionRequirement(Permissions.Users.View)));
```

### Pipeline Order (unchanged from Phase 4A)

```
ExceptionMiddleware
SerilogRequestLogging
OpenAPI (dev only)
HttpsRedirection
Authentication
Authorization       ← now has PermissionAuthorizationHandler wired in
Health Checks
Endpoints
```

No middleware changes needed — `UseAuthorization()` already exists in the pipeline.

---

## Permission Inventory (Phase 4B)

| Permission Name | Description | Protected Endpoint |
|---|---|---|
| `Users.Create` | Create new user accounts | `POST /api/v1/auth/register` |
| `Users.View` | View user list/details | Future |
| `Users.Edit` | Edit user profiles | Future |
| `Users.Delete` | Delete/disable users | Future |
| `Roles.View` | View roles | Future |
| `Roles.Manage` | Manage roles | Future |
| `Permissions.View` | View permissions | Future |
| `Permissions.Manage` | Manage permissions | Future |
| `Countries.View` | View countries | Future |
| `Countries.Manage` | Manage countries | Future |
| `Languages.View` | View languages | Future |
| `Languages.Manage` | Manage languages | Future |
| `Exams.View` | View exams | Future |
| `Exams.Create` | Create exams | Future |
| `Exams.Edit` | Edit exams | Future |
| `Exams.Delete` | Delete exams | Future |
| `Questions.View` | View questions | Future |
| `Questions.Manage` | Manage questions | Future |
| `Nurses.View` | View nurses | Future |
| `Employers.View` | View employers | Future |

---

## Modified Files

| File | Change |
|---|---|
| `Domain/ReferenceData/Role.cs` | Add `RolePermissions` navigation property |
| `Domain/ReferenceData/Permission.cs` | Add `RolePermissions` navigation property |
| `Application/Authorization/Permissions.cs` | Create (permission constants) |
| `Application/Authorization/PermissionRequirement.cs` | Create (IAuthorizationRequirement) |
| `Application/Authorization/PermissionAuthorizationHandler.cs` | Create (AuthorizationHandler) |
| `Application/Authorization/IPermissionService.cs` | Create (interface) |
| `Application/Authorization/PermissionService.cs` | Create (implementation) |
| `Application/Abstractions/Auth/ICurrentUserService.cs` | Create (interface) |
| `Application/Abstractions/Data/IApplicationDbContext.cs` | Add `Permissions`, `RolePermissions` DbSets |
| `Application/DependencyInjection.cs` | Register `IPermissionService`, `PermissionAuthorizationHandler` |
| `Infrastructure/Authentication/CurrentUserService.cs` | Create (implementation) |
| `Infrastructure/DependencyInjection.cs` | Register `ICurrentUserService`, `IHttpContextAccessor` |
| `Infrastructure/Persistence/Configurations/RoleConfiguration.cs` | Add `RolePermissions` navigation config |
| `Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` | Create (EF configuration) |
| `Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` | Add RolePermission seeding |
| `WebApi/Extensions/ServiceCollectionExtensions.cs` | No changes (AddAuthorization() already exists) |
| `WebApi/Extensions/ApplicationBuilderExtensions.cs` | Add register endpoint with PermissionRequirement |

---

## Testing

### Unit Tests (Application)

- Permission constants: `Permissions.All` contains expected list of all permissions matching seed data.
- `PermissionRequirement` constructor throws on null.
- `PermissionAuthorizationHandler`: unauthenticated user → requirement not succeeded; user with permission → succeeded; user without permission → not succeeded.
- `PermissionService`: mock `IApplicationDbContext` with known UserRoles/RolePermissions data; verify correct set returned.

### Unit Tests (Infrastructure)

- `CurrentUserService`: mock `IHttpContextAccessor` with various principal states (null, authenticated, unauthenticated, with/without claims).

### Integration Tests (WebApi)

Strategy for testing protected endpoints:
- `WebApiTestFactory` exposes a mock `IPermissionService` that can be configured per test.
- Tests generate real JWTs using `JwtService` constructed with test `JwtSettings`.
- Three scenarios:
  1. `POST /api/v1/auth/register` without auth token → `401 Unauthorized`
  2. `POST /api/v1/auth/register` with JWT but no `Users.Create` permission → `403 Forbidden`
  3. `POST /api/v1/auth/register` with JWT and `Users.Create` permission → `200 OK` with `RegisterUserResponse.UserId`
- `POST /api/v1/auth/login` and `POST /api/v1/auth/refresh` (AllowAnonymous) still work without auth.

---

## Migration

No new EF Core migration needed — tables (`Roles`, `Permissions`, `RolePermissions`) already exist from Phase 3. Only seed data changes.

However, if `Role.RolePermissions` and `Permission.RolePermissions` navigation properties were not previously configured, EF Core may detect a model change requiring a migration. Generate migration iff `dotnet ef migrations add` reports a pending change.

---

## Constraints

- Permission names use PascalCase format matching existing seed data (e.g. `"Users.Create"`, `"Roles.View"`).
- All permission checks go through the `AuthorizationMiddleware` — no manual `User.IsInRole()` checks in endpoints.
- `PermissionAuthorizationHandler` is the single handler for all `PermissionRequirement` instances.
- Handler registered as scoped (depends on scoped `ICurrentUserService` and `IPermissionService`).
- `CurrentUserService` returns safe defaults (null/empty) for unauthenticated requests — never throws.
- `PermissionService` resolves via navigation properties in a single EF query.
- `IHttpContextAccessor` registered via `AddHttpContextAccessor()` in Infrastructure DI.

## Out of Scope

The following are explicitly deferred to later phases:

- Role CRUD management endpoints
- Permission CRUD management endpoints
- User management full CRUD (list, edit, delete users)
- Frontend auth/authorization
- Email verification
- Password reset
- Nurse, Employer, Examination, Payments, Recruitment modules
- Custom admin dashboard
- Audit logging of authorization decisions
- Permission caching beyond per-request scope
- Deny/negative permissions
- Resource-based authorization (e.g., "can edit this specific exam")
