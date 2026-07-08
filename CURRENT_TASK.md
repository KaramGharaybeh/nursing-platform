# Current Task

## Current Milestone

Phase 4A — Core Identity

Status:
Complete

---

## Objective

Implement the core identity and authentication layer: domain entities, CQRS commands, infrastructure services, WebApi auth pipeline, and integration tests.

---

## Current Focus

- [x] Domain entities (User, UserRole, RefreshToken)
- [x] EF Core configurations + migration (AddIdentityTables)
- [x] Application interfaces (IJwtService, IPasswordHashingService, IApplicationDbContext)
- [x] RegisterUserCommand + handler + validator
- [x] LoginCommand + RotateRefreshTokenCommand + handlers + validators
- [x] Infrastructure services (JwtService, PasswordHashingService)
- [x] Bootstrap admin settings + DatabaseInitializer integration
- [x] WebApi auth pipeline (ExceptionMiddleware, JWT auth, OpenAPI scheme, /auth endpoints)
- [x] Integration tests (WebApi auth endpoint tests)
- [x] Final build, warnings cleanup, documentation

---

## Out of Scope

Do NOT implement:

- Authorization policies (Phase 4B)
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

- Identity entities exist and are mapped correctly
- Login, refresh token, and user registration commands work end-to-end
- JWT and password services are implemented and tested
- Bootstrap admin creates admin user/role on startup
- Auth endpoints return correct HTTP status codes and response shapes
- Integration tests cover login (valid, invalid, validation error) and refresh (valid, invalid)
- Solution builds with zero warnings
- All 60 tests pass (12 Domain + 18 Application + 24 Infrastructure + 6 WebApi)

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- docs/standards/engineering-standards.md
- docs/database/database-design.md