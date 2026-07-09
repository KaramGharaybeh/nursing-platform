# Phase 4C — Account Management Read APIs

## Objective

Implement read-only account management APIs: current user profile, user listing, and user details. These provide the frontend with the authenticated user's identity (including roles and permissions) and allow administrators to browse the user directory.

The design follows all existing patterns: Clean Architecture, CQRS, no custom middleware, no ASP.NET Core Identity framework, permission-based authorization via `PermissionRequirement`.

## Approved Scope

| # | Endpoint | Description | Auth |
|---|----------|-------------|------|
| 1 | `GET /api/v1/me` | Current authenticated user profile | Authenticated only |
| 2 | `GET /api/v1/users` | Paginated user list | `Permissions.Users.View` |
| 3 | `GET /api/v1/users/{id}` | Single user details | `Permissions.Users.View` |

## Out of Scope

The following are explicitly deferred to later phases:

- Activate/deactivate users (`PUT /users/{id}/activate`, `PUT /users/{id}/deactivate`)
- Assign/remove roles (`POST /users/{id}/roles`, `DELETE /users/{id}/roles/{roleId}`)
- Email verification (send confirmation, verify email)
- Password reset (request reset, reset password)
- Change password
- Update own profile (`PATCH /api/v1/me`)
- Delete user
- Role CRUD management endpoints
- Permission CRUD management endpoints
- Frontend integration

## Endpoint Contracts

### 1. `GET /api/v1/me`

Returns the currently authenticated user's profile including assigned roles and permissions.

**Request:**
```
GET /api/v1/me
Authorization: Bearer <token>
```

**Response 200:**
```json
{
  "id": "3c4d5e6f-7081-492a-3b4c-5d6e7f8091a2",
  "email": "nurse@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "isActive": true,
  "emailVerified": false,
  "roles": ["Nurse"],
  "permissions": ["Exams.View", "Questions.View"],
  "createdAt": "2026-07-01T10:00:00Z",
  "lastLoginAt": "2026-07-08T08:30:00Z"
}
```

**Response 401:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

No permission requirement — any authenticated user can view their own profile. The authenticated user's `UserId` is obtained from `ICurrentUserService`.

### 2. `GET /api/v1/users`

Paginated list of users. Supports filtering and sorting for administrative browsing.

**Request:**
```
GET /api/v1/users?page=1&pageSize=20&search=john&isActive=true&role=Nurse&sort=-createdAt
Authorization: Bearer <token>
```

**Query Parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Page number (1-based) |
| `pageSize` | int | 20 | Items per page (max 100) |
| `search` | string | null | Search by email, first name, or last name (partial match) |
| `isActive` | bool | null | Filter by active/inactive status |
| `role` | string | null | Filter by role name (exact match, e.g. "Nurse") |
| `sort` | string | `"createdAt"` | Sort field. Prefix with `-` for descending (e.g. `-createdAt`). Supported: `email`, `firstName`, `lastName`, `createdAt`, `lastLoginAt` |

**Response 200:**
```json
{
  "items": [
    {
      "id": "3c4d5e6f-7081-492a-3b4c-5d6e7f8091a2",
      "email": "nurse@example.com",
      "firstName": "Jane",
      "lastName": "Smith",
      "isActive": true,
      "emailVerified": false,
      "roles": ["Nurse"],
      "createdAt": "2026-07-01T10:00:00Z",
      "lastLoginAt": "2026-07-08T08:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3
}
```

**Response 401:** Missing or invalid token.
**Response 403:** Authenticated but missing `Users.View` permission.

Requires `Permissions.Users.View`.

### 3. `GET /api/v1/users/{id}`

Single user details including roles and permissions.

**Request:**
```
GET /api/v1/users/3c4d5e6f-7081-492a-3b4c-5d6e7f8091a2
Authorization: Bearer <token>
```

**Response 200:**
```json
{
  "id": "3c4d5e6f-7081-492a-3b4c-5d6e7f8091a2",
  "email": "nurse@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "isActive": true,
  "emailVerified": false,
  "roles": ["Nurse"],
  "permissions": ["Exams.View", "Questions.View"],
  "createdAt": "2026-07-01T10:00:00Z",
  "lastLoginAt": "2026-07-08T08:30:00Z"
}
```

**Response 401:** Missing or invalid token.
**Response 403:** Authenticated but missing `Users.View` permission.
**Response 404:** User not found.

Requires `Permissions.Users.View`.

## Permission Mapping

| Endpoint | Permission | Rationale |
|----------|-----------|-----------|
| `GET /api/v1/me` | None (authenticated) | Own profile, always available to any logged-in user |
| `GET /api/v1/users` | `Users.View` | Admin user browsing |
| `GET /api/v1/users/{id}` | `Users.View` | Admin user detail viewing |

The `/me` endpoint does not require a specific permission because every authenticated user has a right to see their own identity. The existence of a valid JWT is sufficient.

## DTO Design

### UserDetailDto

Used for `GET /api/v1/me` and `GET /api/v1/users/{id}` responses. Includes permissions (resolved via role-permission relationships). `PasswordHash` is never exposed.

File: `Application/Identity/DTOs/UserDetailDto.cs`

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

### UserListItemDto

Used for paginated list items in `GET /api/v1/users`. Lighter than `UserDetailDto` — no permissions array (to keep payload small for lists).

File: `Application/Identity/DTOs/UserListItemDto.cs`

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

### PaginatedResult\<T\>

Generic paginated response. No existing implementation — must be created.

File: `Application/Common/Models/PaginatedResult.cs`

```csharp
namespace NursingPlatform.Application.Common.Models;

public class PaginatedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
```

### ListUsersRequest

Query parameters for the list endpoint. Implements `IRequest<PaginatedResult<UserListItemDto>>`.

File: `Application/Identity/Queries/ListUsers/ListUsersQuery.cs`

Properties mapped from query string: `Page`, `PageSize`, `Search`, `IsActive`, `Role`, `Sort`.

### GetUserQuery

Single user detail request. Implements `IRequest<UserDetailDto>`.

File: `Application/Identity/Queries/GetUser/GetUserQuery.cs`

Properties: `UserId` (Guid).

### GetCurrentUserQuery

Current user profile request. Implements `IRequest<UserDetailDto>`.

File: `Application/Identity/Queries/GetCurrentUser/GetCurrentUserQuery.cs`

No properties — handler reads `ICurrentUserService.UserId`.

## Query Design

### GetCurrentUserQueryHandler

```
Flow:
1. Read UserId from ICurrentUserService
2. If UserId is null, return/throw using the project's existing unauthorized error pattern. If no such pattern exists, the endpoint should be protected by RequireAuthorization and handler tests should verify null UserId behavior without relying on an unmapped exception returning 401.
3. Load User with UserRoles → Role → RolePermissions → Permission from DB
4. Map to UserDetailDto (include resolved permission names)
5. Return UserDetailDto
```

Permissions are resolved by including `UserRoles`, then `Role.RolePermissions`, then `Permission.Name`. The same resolution pattern used by `PermissionService` in Phase 4B is reused here.

No caching — per-request scope is sufficient for Phase 4C.

### GetUserQueryHandler

```
Flow:
1. Load User by UserId with UserRoles → Role → RolePermissions → Permission
2. If not found → throw NotFoundException (→ 404)
3. Map to UserDetailDto (include permissions)
4. Return UserDetailDto
```

### ListUsersQueryHandler

```
Flow:
1. Start with IQueryable<User> from DbContext (with UserRoles → Role include)
2. Apply search filter: WHERE (Email CONTAINS search) OR (FirstName CONTAINS search) OR (LastName CONTAINS search)
3. Apply isActive filter: WHERE IsActive == isActive
4. Apply role filter: WHERE UserRoles.Any(ur => ur.Role.Name == role)
5. Apply sorting (default: createdAt descending)
6. Apply pagination: Skip((page-1) * pageSize).Take(pageSize)
7. Execute count query for totalCount
8. Map items to UserListItemDto (roles only, no permissions)
9. Return PaginatedResult<UserListItemDto>
```

Filtering is case-insensitive (use EF.Functions.ILike for PostgreSQL or ToLower()).

Valid sort fields: `email`, `firstName`, `lastName`, `createdAt`, `lastLoginAt`. Default: `createdAt` descending.

Max page size: 100. Default page size: 20.

## Modified Files

| File | Change |
|------|--------|
| `Application/Common/Models/PaginatedResult.cs` | Create (generic paginated response) |
| `Application/Identity/DTOs/UserDetailDto.cs` | Create |
| `Application/Identity/DTOs/UserListItemDto.cs` | Create |
| `Application/Identity/Queries/GetCurrentUser/GetCurrentUserQuery.cs` | Create |
| `Application/Identity/Queries/GetCurrentUser/GetCurrentUserQueryHandler.cs` | Create |
| `Application/Identity/Queries/GetCurrentUser/GetCurrentUserQueryValidator.cs` | Create (if needed) |
| `Application/Identity/Queries/GetUser/GetUserQuery.cs` | Create |
| `Application/Identity/Queries/GetUser/GetUserQueryHandler.cs` | Create |
| `Application/Identity/Queries/GetUser/GetUserQueryValidator.cs` | Create |
| `Application/Identity/Queries/ListUsers/ListUsersQuery.cs` | Create |
| `Application/Identity/Queries/ListUsers/ListUsersQueryHandler.cs` | Create |
| `Application/Identity/Queries/ListUsers/ListUsersQueryValidator.cs` | Create |
| `WebApi/Extensions/ApplicationBuilderExtensions.cs` | Add 3 new endpoints with permissions |
| Test files | Create handler tests + integration tests |

No EF Core migration is needed — this phase adds no new entities or properties.

## Test Strategy

### Application Unit Tests

| Test Group | Tests | Count |
|-----------|-------|-------|
| `GetCurrentUserQueryHandlerTests` | User found → returns `UserDetailDto`; UserId null → throws; User not found → throws | 3 |
| `GetUserQueryHandlerTests` | User found → returns `UserDetailDto`; User not found → throws | 2 |
| `ListUsersQueryHandlerTests` | Pagination works; search filters correctly; isActive filter; role filter; sorting; empty results; pageSize capped at 100 | 7 |
| `GetCurrentUserQueryValidatorTests` | (if validator created) | 1 |
| `ListUsersQueryValidatorTests` | Page < 1 → invalid; pageSize < 1 → invalid; pageSize > 100 → invalid | 3 |
| `GetUserQueryValidatorTests` | Empty UserId → invalid | 1 |
| `PaginatedResultTests` | TotalPages calculation | 1 |

**Total estimated new unit tests: ~18**

### WebApi Integration Tests

| Test Group | Tests | Count |
|-----------|-------|-------|
| `GET /me` 200 (authenticated) | Valid token, returns user profile with permissions | 1 |
| `GET /me` 401 (no token) | No Authorization header | 1 |
| `GET /users` 200 (with Users.View) | Valid token + permission | 1 |
| `GET /users` 403 (no permission) | Valid token, no Users.View | 1 |
| `GET /users` 401 (no token) | No Authorization header | 1 |
| `GET /users/{id}` 200 (with Users.View) | Valid token + permission, existing user | 1 |
| `GET /users/{id}` 403 (no permission) | Valid token, no Users.View | 1 |
| `GET /users/{id}` 401 (no token) | No Authorization header | 1 |
| `GET /users/{id}` 404 (not found) | Valid token + permission, nonexistent ID | 1 |

**Total estimated new integration tests: ~9**

**Total new tests: ~27**

### Test Data Strategy

WebApi integration tests should follow the existing WebApi testing style. If endpoints use `ISender`, mock `ISender` responses for success/not-found paths, and mock `IPermissionService` for permission checks. Application handler tests should cover real query behavior against test `DbContext` / in-memory EF setup.

The existing `RegisterEndpointTests.CreateJwt()` helper can be reused to generate test tokens.

## Risks and Mitigations

### Risk 1: Permission Resolution Performance

Loading permissions for every `/me` and `/users/{id}` request involves a multi-level EF include: `UserRoles → Role → RolePermissions → Permission`.

**Mitigation:** Acceptable for Phase 4C. The user count during early adoption is small. If needed, permission caching (per-user, with invalidation on role change) can be added in a later phase without changing the API contract.

### Risk 2: Search Query Performance

The `search` parameter in `ListUsersQuery` will generate a `WHERE` clause with `OR` conditions across three string columns. On large datasets this may be slow without an index.

**Mitigation:** The existing `Email` column likely has a unique index from the `UserConfiguration`. Consider adding a composite index on `(FirstName, LastName)` if search performance becomes an issue. Not needed for Phase 4C.

### Risk 3: Case-Sensitive Search on PostgreSQL

PostgreSQL is case-sensitive by default for `LIKE`/`CONTAINS` queries. Using `ToLower()` on both sides prevents EF Core from using indexes.

**Mitigation:** For Phase 4C, a simple `ToLower()` approach on both sides is acceptable given expected low user counts. If needed, switch to `EF.Functions.ILike` (PostgreSQL case-insensitive operator, index-friendly) in a later optimization pass.

### Risk 4: No Existing `UserRoles → Role → RolePermissions` Navigation in Application Layer

The `PermissionService` already uses `UserRoles.SelectMany(ur => ur.Role.RolePermissions)` to resolve permissions. The same navigation path works for query handlers since `IApplicationDbContext` exposes `UserRoles`.

**Mitigation:** Reuse the same navigation. Verify that `RoleConfiguration` includes the `RolePermissions` collection navigation. If the include chain breaks, add the missing configuration.

## Open Questions

None. The approved scope is fully specified.
