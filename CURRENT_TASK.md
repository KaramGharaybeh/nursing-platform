# Current Task

## Current Milestone

Phase 4C — Account Management Read APIs

Status:
Complete

---

## Objective

Implement read-only account management APIs: current user profile (GET /api/v1/me), paginated user list (GET /api/v1/users), and single user details (GET /api/v1/users/{id}).

---

## Current Focus

- [x] PaginatedResult, UserDetailDto, UserListItemDto
- [x] GetCurrentUserQuery + handler + tests
- [x] GetUserQuery + handler + validator + tests
- [x] ListUsersQuery + handler + validator + tests
- [x] GET /api/v1/me endpoint + integration tests
- [x] GET /api/v1/users and GET /api/v1/users/{id} endpoints + integration tests
- [x] Final build, test, EF migration verification

---

## Out of Scope

Do NOT implement:

- Email verification
- Password reset
- Activate/deactivate account
- Role assignment (admin)
- Nurse module
- Employer module
- Examination module
- Payments
- Recruitment
- Notifications
- Administration features

---

## Definition of Done

This milestone is complete when:

- PaginatedResult&lt;T&gt; shared model exists with computed TotalPages
- UserDetailDto and UserListItemDto exist with correct field exposure
- GetCurrentUserQuery returns authenticated user profile with roles and permissions
- GetUserQuery returns user by ID with roles and permissions
- ListUsersQuery supports pagination, search, isActive filter, role filter, and sort
- GET /api/v1/me is protected by RequireAuthorization only
- GET /api/v1/users and GET /api/v1/users/{id} are protected by RequirePermission(Permissions.Users.View)
- PasswordHash is never exposed in any response
- Integration tests cover 401, 403, and 200 for each protected endpoint
- Solution builds with zero warnings
- All 169 tests pass (12 Domain + 89 Application + 46 Infrastructure + 22 WebApi)

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- docs/standards/engineering-standards.md
- docs/database/database-design.md