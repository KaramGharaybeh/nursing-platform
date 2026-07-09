# Current Task

## Current Milestone

Phase 4D — Identity Account Recovery & Verification

Status:
Complete

---

## Objective

Complete the deferred identity account recovery and verification work before Phase 5 by implementing email verification and password reset APIs.

---

## Current Focus

- [x] Email verification tokens and persistence
- [x] Password reset tokens and persistence
- [x] Email service / MailKit
- [x] POST /api/v1/auth/send-verification-email
- [x] POST /api/v1/auth/verify-email
- [x] POST /api/v1/auth/forgot-password
- [x] POST /api/v1/auth/reset-password
- [x] Application handler tests
- [x] WebApi integration tests
- [x] EF migration
- [x] Final build, test, and EF verification

---

## Final Verification

- build: 0 warnings / 0 errors
- tests: 219 passed
- EF pending model check: no pending model changes
- EF design-time note: `dotnet ef migrations has-pending-model-changes` logs a known non-blocking `HostAbortedException` during design-time host resolution, then reports no pending model changes.

---

## Out of Scope

Do NOT implement:

- Change password (authenticated, requires old password)
- Update own profile
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

- EmailVerificationToken and PasswordResetToken persistence exists
- Email service abstraction and MailKit implementation exist
- Email verification request and verification endpoints are implemented
- Forgot-password and reset-password endpoints are implemented
- Tokens are generated securely and only token hashes are stored
- Password reset revokes active refresh tokens
- Raw tokens are never returned in API responses
- Solution builds with zero warnings
- All 219 tests pass
- EF pending model check reports no pending model changes

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- docs/standards/engineering-standards.md
- docs/database/database-design.md
