# Current Task

## Current Milestone

Phase 6B — Candidate Search Foundation

Status:
Complete — final verification passed.

---

## Objective

Complete the Phase 6B employer-facing candidate search foundation for authenticated employers.

---

## Completion Summary

- [x] Task 1: Recruitment Application query, DTOs, validator, handler, and Application tests
- [x] Task 2: WebApi candidate listing endpoint and integration tests
- [x] Task 3: Final verification and documentation review
- [x] Task 4: Tracking documentation update

---

## Final Verification

- `dotnet build backend/NursingPlatform.slnx`: passed, 0 warnings, 0 errors
- `dotnet test backend/NursingPlatform.slnx`: passed, 375 tests
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`: no pending model changes
- EF design-time note: `HostAbortedException` is known non-blocking EF design-time host-resolution noise when EF still reports no pending model changes.

---

## Current Review State

Phase 6B Tasks 1-3 have passed final verification and review.

Do NOT proceed to Task 5 until Task 4 is reviewed and explicitly approved.

Do NOT stage files.

Do NOT commit until explicitly instructed.

---

## Out of Scope

Do NOT implement:

- Recruitment filtering
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

- Recruitment Application candidate search query, DTOs, validator, handler, and tests exist
- `GET /api/v1/recruitment/candidates` exists and requires authorization
- Candidate search returns only recruitment-visible, active, email-verified nurses
- Employer role, employer profile, and employer organization prerequisites are enforced
- WebApi integration tests cover auth, validation, forbidden access, pagination, response shape, and raw JSON sensitive-field exposure behavior
- Solution builds with zero warnings and zero errors
- All 375 tests pass
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
