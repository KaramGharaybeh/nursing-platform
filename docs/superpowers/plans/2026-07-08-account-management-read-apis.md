# Phase 4C — Account Management Read APIs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement read-only account management APIs: current user profile (`GET /api/v1/me`), paginated user list (`GET /api/v1/users`), and single user details (`GET /api/v1/users/{id}`).

**Architecture:** Three CQRS queries (`GetCurrentUserQuery`, `GetUserQuery`, `ListUsersQuery`) with handlers that use `IApplicationDbContext` and `ICurrentUserService`. Endpoints protected by `PermissionRequirement` via `RequirePermission` extension. No custom middleware.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, EF Core, Moq, xUnit

**Spec:** `docs/superpowers/specs/2026-07-08-account-management-read-apis.md`

## Global Constraints

- No EF Core migration needed — this phase adds no new entities or properties.
- `PasswordHash` must never be exposed in any DTO.
- `UserDetailDto` includes `Permissions` (resolved from role-permission relationships). `UserListItemDto` does not include `Permissions`.
- Permission resolution follows the same navigation path as `PermissionService`: `UserRoles → Role → RolePermissions → Permission.Name`.
- `GetCurrentUserQueryHandler` reads `UserId` from `ICurrentUserService`. If null, use the project's existing unauthorized error pattern. Do not assume `UnauthorizedAccessException` maps to 401.
- `ListUsersQueryHandler` supports pagination with max `pageSize` of 100. Validator rejects `PageSize > 100` (handler does not cap). Search (email/firstName/lastName partial match), `isActive` filter, `role` filter, and sort.
- `PaginatedResult<T>` created as a shared generic model under `Application/Common/Models/`.
- All timestamps UTC. All entities Guid PK. Plural PascalCase table names.
- Follow existing project conventions (file placement, naming, dependency injection patterns).
- Integration tests in WebApi project follow existing style: mock `ISender` for success/not-found, mock `IPermissionService` for permission checks. Application handler tests use real query behavior against in-memory EF setup.

---

### Task 1: Create Shared DTOs and PaginatedResult

**Files:**
- Create: `backend/src/NursingPlatform.Application/Common/Models/PaginatedResult.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/DTOs/UserDetailDto.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/DTOs/UserListItemDto.cs`

**Interfaces:**
- Produces: `PaginatedResult<T>` generic model, `UserDetailDto`, `UserListItemDto`

- [ ] **Step 1: Create PaginatedResult.cs**

```csharp
namespace NursingPlatform.Application.Common.Models;

public class PaginatedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

- [ ] **Step 2: Create UserDetailDto.cs**

```csharp
namespace NursingPlatform.Application.Identity.DTOs;

public class UserDetailDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailVerified { get; init; }
    public List<string> Roles { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
```

- [ ] **Step 3: Create UserListItemDto.cs**

```csharp
namespace NursingPlatform.Application.Identity.DTOs;

public class UserListItemDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailVerified { get; init; }
    public List<string> Roles { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
```

- [ ] **Step 4: Write unit test for PaginatedResult**

```csharp
// New test file: Application.Tests/Common/Models/PaginatedResultTests.cs
using NursingPlatform.Application.Common.Models;

namespace NursingPlatform.Application.Tests.Common.Models;

public class PaginatedResultTests
{
    [Fact]
    public void TotalPages_ShouldBeCalculatedCorrectly()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 25
        };

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithExactDivision_ShouldNotRoundUp()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 20
        };

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithZeroItems_ShouldBeZero()
    {
        var result = new PaginatedResult<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        Assert.Equal(0, result.TotalPages);
    }
}
```

- [ ] **Step 5: Build and run tests**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings
Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "PaginatedResult"` — Expected: PASS

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 2: Create GetCurrentUserQuery + Handler + Validator + Tests

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/GetCurrentUser/GetCurrentUserQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/GetCurrentUser/GetCurrentUserQueryHandler.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/GetCurrentUser/GetCurrentUserQueryHandlerTests.cs`

**No validator needed** — the query has no properties.

**Interfaces:**
- Consumes: `ICurrentUserService`, `IApplicationDbContext`
- Produces: `GetCurrentUserQuery : IRequest<UserDetailDto>`, handler

- [ ] **Step 1: Create GetCurrentUserQuery.cs**

```csharp
using MediatR;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserDetailDto>
{
}
```

- [ ] **Step 2: Write failing handler tests**

Test cases:
1. User found → returns `UserDetailDto` with roles and permissions
2. UserId null → throws or returns using project's unauthorized error pattern
3. User not found → throws `NotFoundException`

Use a real `DbContext` with `UseInMemoryDatabase` or similar approach, or mock `IApplicationDbContext` with `MockQueryable.Moq` / `BuildMockDbSet`. Choose the pattern that matches existing handler tests in the project.

- [ ] **Step 3: Create GetCurrentUserQueryHandler.cs**

Key behavior:
- Read `UserId` from `ICurrentUserService`
- If `UserId` is null, return/throw using the project's existing unauthorized error pattern
- Load `User` with `UserRoles → Role → RolePermissions → Permission` includes
- If not found, throw `NotFoundException` (→ 404)
- Map to `UserDetailDto` with resolved roles (role names) and permissions (permission names)
- Return `UserDetailDto`

- [ ] **Step 4: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "GetCurrentUser"` — Expected: PASS

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 3: Create GetUserQuery + Handler + Validator + Tests

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/GetUser/GetUserQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/GetUser/GetUserQueryHandler.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/GetUser/GetUserQueryHandlerTests.cs`

**Validator:**
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/GetUser/GetUserQueryValidator.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/GetUser/GetUserQueryValidatorTests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`
- Produces: `GetUserQuery : IRequest<UserDetailDto>`, handler, validator

- [ ] **Step 1: Create GetUserQuery.cs**

```csharp
using MediatR;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.GetUser;

public class GetUserQuery : IRequest<UserDetailDto>
{
    public Guid UserId { get; init; }
}
```

- [ ] **Step 2: Create GetUserQueryValidator.cs**

Validates that `UserId` is not empty.

- [ ] **Step 3: Write failing handler tests**

Test cases:
1. User found → returns `UserDetailDto` with roles and permissions
2. User not found → throws `NotFoundException`

- [ ] **Step 4: Write failing validator tests**

Test cases:
1. Empty `UserId` → invalid

- [ ] **Step 5: Create GetUserQueryHandler.cs**

Key behavior:
- Load `User` by `UserId` with `UserRoles → Role → RolePermissions → Permission` includes
- If not found, throw `NotFoundException` (→ 404)
- Map to `UserDetailDto` with roles and permissions
- Return `UserDetailDto`

- [ ] **Step 6: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "GetUserQuery"` — Expected: PASS

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 4: Create ListUsersQuery + Handler + Validator + Tests

**Files:**
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/ListUsers/ListUsersQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/ListUsers/ListUsersQueryHandler.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/ListUsers/ListUsersQueryHandlerTests.cs`

**Validator:**
- Create: `backend/src/NursingPlatform.Application/Identity/Queries/ListUsers/ListUsersQueryValidator.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/ListUsers/ListUsersQueryValidatorTests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`
- Produces: `ListUsersQuery : IRequest<PaginatedResult<UserListItemDto>>`, handler, validator

- [ ] **Step 1: Create ListUsersQuery.cs**

```csharp
using MediatR;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Identity.DTOs;

namespace NursingPlatform.Application.Identity.Queries.ListUsers;

public class ListUsersQuery : IRequest<PaginatedResult<UserListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public string? Role { get; init; }
    public string? Sort { get; init; }
}
```

- [ ] **Step 2: Create ListUsersQueryValidator.cs**

Validates:
- `Page` >= 1
- `PageSize` >= 1 and <= 100
- `Sort` is one of the allowed values: `email`, `firstName`, `lastName`, `createdAt`, `lastLoginAt` (with optional `-` prefix)

- [ ] **Step 3: Write failing handler tests**

Test cases:
1. Pagination works correctly (returns correct page of items)
2. Search filters by email (partial match)
3. Search filters by firstName
4. Search filters by lastName
5. `isActive` filter (true/false)
6. `role` filter (exact role name match)
7. Sorting ascending by email, firstName, lastName, createdAt, lastLoginAt
8. Sorting descending (prefixed with `-`)
9. Empty results returns empty list
10. Default sort is `createdAt` descending

- [ ] **Step 4: Write failing validator tests**

Test cases:
1. `Page` < 1 → invalid
2. `PageSize` < 1 → invalid
3. `PageSize` > 100 → invalid
4. Invalid `Sort` field → invalid
5. Valid query → passes validation

- [ ] **Step 5: Create ListUsersQueryHandler.cs**

Key behavior:
- Start with `IQueryable<User>` from `DbContext` with `UserRoles → Role` include
- Apply search filter: `WHERE (Email.Contains(search) OR FirstName.Contains(search) OR LastName.Contains(search))` — case-insensitive
- Apply `isActive` filter if provided
- Apply `role` filter if provided: `WHERE UserRoles.Any(ur => ur.Role.Name == role)`
- Apply sorting (default: `createdAt` descending)
- Apply pagination: `Skip((page - 1) * pageSize).Take(pageSize)`
- Execute count query for `TotalCount`
- Map items to `UserListItemDto` (roles only, no permissions)
- Return `PaginatedResult<UserListItemDto>`

- [ ] **Step 6: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.Application.Tests/ --filter "ListUsersQuery"` — Expected: PASS

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 5: Map GET /api/v1/me Endpoint + Integration Tests

**Files:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Modify: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/WebApiTestFactory.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/MeEndpointTests.cs`

**Interfaces:**
- Consumes: `GetCurrentUserQuery`, `UserDetailDto`
- Produces: `GET /api/v1/me` endpoint with integration tests

- [ ] **Step 1: Add GET /me endpoint to MapApiEndpoints**

Add after existing register endpoint:

```csharp
api.MapGet("/me", async (ISender sender) =>
{
    var user = await sender.Send(new GetCurrentUserQuery());
    return Results.Ok(user);
})
.WithName("GetCurrentUser")
.RequireAuthorization();
```

Note: Uses `.RequireAuthorization()` (no specific permission) — any authenticated user can access `/me`.

- [ ] **Step 2: Write integration tests**

Test cases:
1. `GET /me` with valid token → 200, returns `UserDetailDto` with roles and permissions
2. `GET /me` without token → 401

Integration tests follow existing WebApi testing style: mock `ISender` responses, generate real JWT via helper, use `WebApiTestFactory`.

- [ ] **Step 3: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.WebApi.Tests/ --filter "MeEndpoint"` — Expected: PASS

- [ ] **Step 4: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

**Stop for review. Do not proceed to the next task. Do not commit.**

Note: If `WebApiTestFactory.cs` needs modification for the `/me` endpoint (e.g., additional mocks), include it in the review.

---

### Task 6: Map GET /api/v1/users and GET /api/v1/users/{id} Endpoints + Integration Tests

**Files:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/UsersEndpointTests.cs`

**Interfaces:**
- Consumes: `ListUsersQuery`, `GetUserQuery`, `Permissions.Users.View`
- Produces: `GET /api/v1/users`, `GET /api/v1/users/{id}` endpoints with integration tests

- [ ] **Step 1: Add GET /users endpoint to MapApiEndpoints**

```csharp
api.MapGet("/users", async ([AsParameters] ListUsersQuery query, ISender sender) =>
{
    var result = await sender.Send(query);
    return Results.Ok(result);
})
.WithName("ListUsers")
.RequirePermission(Permissions.Users.View);
```

- [ ] **Step 2: Add GET /users/{id} endpoint to MapApiEndpoints**

```csharp
api.MapGet("/users/{id:guid}", async (Guid id, ISender sender) =>
{
    var user = await sender.Send(new GetUserQuery { UserId = id });
    return Results.Ok(user);
})
.WithName("GetUser")
.RequirePermission(Permissions.Users.View);
```

- [ ] **Step 3: Write integration tests**

Test cases for `GET /api/v1/users`:
1. 200 with `Users.View` permission
2. 403 without `Users.View` permission
3. 401 without token

Test cases for `GET /api/v1/users/{id}`:
1. 200 with `Users.View` permission, existing user
2. 403 without `Users.View` permission
3. 401 without token
4. 404 with valid token + permission, nonexistent user ID

Integration tests follow existing style: mock `ISender` for success/not-found, mock `IPermissionService` for permission checks.

- [ ] **Step 4: Run tests**

Run: `dotnet test backend/tests/NursingPlatform.WebApi.Tests/ --filter "UsersEndpoint"` — Expected: PASS

- [ ] **Step 5: Build and verify**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 7: Final Build, Test, and EF Migration Check

**Files:**
- No source changes. Only verification and cleanup.

- [ ] **Step 1: Full solution build**

Run: `dotnet build backend/NursingPlatform.slnx` — Expected: 0 errors, 0 warnings

- [ ] **Step 2: Run all tests**

Run: `dotnet test backend/NursingPlatform.slnx` — Expected: ALL PASS

Check test count is appropriate (existing tests + new tests). Note the exact count.

- [ ] **Step 3: EF Core migration check**

Generate a migration to detect any model changes:
```bash
dotnet ef migrations add VerifyAccountReadApisNoModelChanges \
  --project backend/src/NursingPlatform.Infrastructure \
  --startup-project backend/src/NursingPlatform.WebApi \
  --context ApplicationDbContext
```

Inspect the generated migration files:
- If `Up` and `Down` methods are empty, the migration is a no-op. Delete the generated migration files and restore `ApplicationDbContextModelSnapshot.cs` if it was changed.
- If `Up` or `Down` is not empty, stop and paste the generated migration for review.

- [ ] **Step 4: Verify git status**

Run: `git status` — Confirm only intended files are modified/untracked. No accidental changes.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 8: Update CURRENT_TASK.md and TASKS.md

**Files:**
- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

- [ ] **Step 1: Update CURRENT_TASK.md**

Set Current Milestone to "Phase 4C — Account Management Read APIs" and mark all tasks complete:
- [x] PaginatedResult, UserDetailDto, UserListItemDto
- [x] GetCurrentUserQuery + handler + tests
- [x] GetUserQuery + handler + validator + tests
- [x] ListUsersQuery + handler + validator + tests
- [x] GET /api/v1/me endpoint + integration tests
- [x] GET /api/v1/users and GET /api/v1/users/{id} endpoints + integration tests
- [x] Final build, test, EF migration verification

- [ ] **Step 2: Update TASKS.md**

Mark Phase 4C items as complete in the roadmap. Keep deferred items (email verification, password reset, activate/deactivate, role assignment) in their deferred positions.

- [ ] **Step 3: Paste diffs for review**

Run: `git diff CURRENT_TASK.md TASKS.md`

Paste the output for review. Do not commit.

**Stop for review. Do not proceed to the next task. Do not commit.**

---

### Task 9: Final Audit and Commit (Only After Explicit Approval)

**Files:**
- All files from previous tasks (staged or unstaged — depends on prior approvals)

- [ ] **Step 1: Review all changes**

Run `git log --oneline -10` (if prior commits exist) or `git diff --stat` to see all changed files.

- [ ] **Step 2: Verify no unintended files**

Run `git status` — ensure no unintended modifications, no secrets, no temporary code, no generated artifacts.

- [ ] **Step 3: Verify docs and spec are in sync**

Quick-read the spec and confirm implementation matches:
- `GET /api/v1/me` exists with correct shape
- `GET /api/v1/users` exists with pagination/filtering
- `GET /api/v1/users/{id}` exists with correct shape
- Permission mapping matches spec
- Out-of-scope items are not implemented

- [ ] **Step 4: Stage all intended files**

Run `git add` for source, test, and doc files only. Never stage generated artifacts, bin/obj, or temporary files.

- [ ] **Step 5: Create meaningful commit**

Commit with a message summarizing Phase 4C scope, e.g.:
```bash
git commit -m "feat: add account management read APIs (GET /me, /users, /users/{id})"
```

- [ ] **Step 6: Only after explicit approval**

Do not commit until the reviewer explicitly approves the final audit output. Paste `git status`, `git diff --cached --stat`, and `git log --oneline -3` for review before committing.

---

## Self-Review Checklist

- **Spec coverage** — All requirements from `docs/superpowers/specs/2026-07-08-account-management-read-apis.md` covered:
  - `PaginatedResult<T>` created as shared model (Task 1)
  - `UserDetailDto` and `UserListItemDto` created (Task 1)
  - `GetCurrentUserQuery` + handler reads from `ICurrentUserService` (Task 2)
  - `GetUserQuery` + handler loads user by ID with roles/permissions (Task 3)
  - `ListUsersQuery` + handler supports pagination, search, filters, sort (Task 4)
  - `GET /api/v1/me` endpoint without permission requirement (Task 5)
  - `GET /api/v1/users` and `GET /api/v1/users/{id}` endpoints with `Users.View` (Task 6)
  - Integration tests for auth/permission behavior (Tasks 5, 6)
  - Application handler tests against in-memory EF (Tasks 2, 3, 4)
- **No activation/deactivation endpoints** — confirmed out of scope
- **No role assignment endpoints** — confirmed out of scope
- **No activation/deactivation handlers** — confirmed out of scope
- **No EF migration** — confirmed no new entities/properties
- **PageSize validation** — Validator rejects `PageSize > 100`; handler does not cap
- **Placeholder scan** — No "TBD", "TODO", "implement later", or similar patterns in production code
- **Type consistency** — `UserDetailDto` includes `Permissions` list; `UserListItemDto` does not; `PaginatedResult<T>` uses generic type parameter
