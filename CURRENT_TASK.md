# Current Task

## Current Milestone

Phase 4B — Authorization

Status:
Complete

---

## Objective

Implement the authorization layer: permission-based authorization pipeline, current user context, reference data seeding, and protected API endpoints.

---

## Current Focus

- [x] Permission authorization handler and requirement
- [x] Permission service
- [x] Current user service
- [x] Reference data entities (Permission, Role, RolePermission)
- [x] EF Core configurations for reference data
- [x] Reference data seeder (idempotent, testable)
- [x] RequirePermission extension method for Minimal API
- [x] Register endpoint protected with Users.Create permission
- [x] IPermissionService mocked in WebApi tests
- [x] JWT KeyId fix for JsonWebTokenHandler compatibility
- [x] Integration tests (register 401/403/200, login/refresh no-auth)
- [x] Unit tests (handler, requirement, service, permissions)
- [x] Final build, warnings cleanup, verification

---

## Out of Scope

Do NOT implement:

- Email verification (Phase 4C)
- Password reset (Phase 4C)
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

- Permission authorization pipeline exists (handler + requirement + service)
- Current user service extracts user identity from HTTP context
- Reference data roles/permissions seed correctly and idempotently
- Register endpoint is protected with RequirePermission(Permissions.Users.Create)
- Login and refresh remain AllowAnonymous
- WebApi integration tests cover register (401 unauthenticated, 403 no permission, 200 with permission)
- JwtService tokens include kid header matching JWT bearer validation key
- Solution builds with zero warnings
- All 104 tests pass (12 Domain + 37 Application + 46 Infrastructure + 9 WebApi)

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- docs/standards/engineering-standards.md
- docs/database/database-design.md