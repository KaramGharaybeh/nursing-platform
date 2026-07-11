# Current Task

## Current Milestone

Phase 6C — Candidate Filtering

Status:
Implementation complete — stopped for review before final commit.

---

## Objective

Complete the Phase 6C employer-facing candidate filtering enhancement for authenticated employers.

---

## Completion Summary

- [x] Task 1: Application candidate filtering query, validation, handler behavior, and Application tests
- [x] Task 2: WebApi query binding, invalid GUID handling, and integration tests
- [x] Task 3: Final verification and docs/index review
- [x] Task 4: Tracking documentation update

---

## Final Verification

- `dotnet build backend/NursingPlatform.slnx`: passed, 0 warnings, 0 errors
- `dotnet test backend/NursingPlatform.slnx`: passed, 402 tests
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`: no pending model changes
- EF design-time note: `HostAbortedException` is known non-blocking EF design-time host-resolution noise when EF still reports no pending model changes.

---

## Current Review State

Phase 6C implementation and final verification are complete.

Stopped for review before final commit.

Do NOT stage files.

Do NOT commit until explicitly instructed.

---

## Out of Scope

Do NOT implement:

- Contact requests
- CV download endpoint
- Certificate file attachments
- Frontend implementation
- Payments
- Exams
- Notifications
- Administration features

---

## Definition of Done

This milestone is complete when:

- Recruitment Application candidate filtering query, validator, handler behavior, and tests exist
- `GET /api/v1/recruitment/candidates` binds approved Phase 6C filters and requires authorization
- Candidate listing returns only recruitment-visible, active, email-verified nurses
- Candidate filtering supports license country, current country, minimum years of experience, skills, and language filters
- Employer role, employer profile, and employer organization prerequisites are enforced
- WebApi integration tests cover auth, validation, invalid GUID binding, pagination compatibility, response shape, and raw JSON sensitive-field exposure behavior
- Solution builds with zero warnings and zero errors
- All 402 tests pass
- EF pending model check reports no pending model changes
- Implementation is reviewed and explicitly approved before commit

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- docs/standards/engineering-standards.md
- docs/database/database-design.md
- docs/api/api-design.md
