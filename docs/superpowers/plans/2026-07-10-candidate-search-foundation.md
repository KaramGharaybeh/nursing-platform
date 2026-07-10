# Employer-facing Candidate Search Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Phase 6B read-only candidate search foundation for authenticated employers.

**Architecture:** Candidate search is implemented conceptually under the Recruitment Application feature while remaining sequenced under Phase 6. WebApi remains thin and delegates to MediatR; Application enforces Employer access, employer prerequisites, nurse eligibility, pagination, deterministic sorting, and safe DTO projection. No Domain entity changes or migrations are expected for Phase 6B.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, xUnit, Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 6B: Employer-facing Candidate Search Foundation.
- Do not implement Phase 6C advanced filtering or sorting.
- Do not implement Phase 6D contact requests.
- Do not implement Phase 7, frontend, payments, subscriptions, notifications, admin approval, saved searches, messaging, CV download, or nurse contact info.
- Add exactly one read-only endpoint: `GET /api/v1/recruitment/candidates?page=1&pageSize=20`.
- Use conceptual module placement `Recruitment`; keep roadmap sequencing under Phase 6.
- Require authenticated user with Employer role.
- Require the authenticated Employer user to have both `EmployerProfile` and `EmployerOrganization` before searching.
- Missing `EmployerProfile` or `EmployerOrganization` must throw `ForbiddenAccessException` and return `403 Forbidden`.
- Candidate data must not be queried or returned when employer prerequisites are missing.
- Searchable nurses must satisfy `IsAvailableForRecruitment == true`, `User.IsActive == true`, and `User.EmailVerified == true`.
- Use existing `PaginatedResult<T>`.
- Do not expose `DisplayLabel`, full legal name, contact info, CV access, `UserId`, account internals, roles, permissions, tokens, or EF navigation objects.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` until Task 4 and only after Task 3 is approved.
- Do not stage or commit until explicitly instructed.

## Planned File Structure

- Create `backend/src/NursingPlatform.Application/Recruitment/DTOs/CandidateLanguageDto.cs`: Safe language summary for candidate list items.
- Create `backend/src/NursingPlatform.Application/Recruitment/DTOs/CandidateListItemDto.cs`: Safe employer-facing candidate summary DTO.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQuery.cs`: Request object returning `PaginatedResult<CandidateListItemDto>`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryValidator.cs`: Pagination validation.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryHandler.cs`: Employer prerequisite checks, eligibility filtering, sorting, pagination, projection.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`: Map one Recruitment candidate listing endpoint.
- Create `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryHandlerTests.cs`: Application handler coverage.
- Create `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryValidatorTests.cs`: Pagination validation coverage.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/CandidateSearchEndpointsTests.cs`: Endpoint auth, validation, response, and raw JSON security tests.
- Modify `CURRENT_TASK.md`: Task 4 only, after Task 3 approval.
- Modify `TASKS.md`: Task 4 only, after Task 3 approval.

---

### Task 1: Application-layer Candidate Search

**Goal:** Add Recruitment Application query, DTOs, validator, handler, and Application tests. No WebApi endpoint yet.

**Files:**
- Create: `backend/src/NursingPlatform.Application/Recruitment/DTOs/CandidateLanguageDto.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/DTOs/CandidateListItemDto.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryHandler.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryHandlerTests.cs`
- Create: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryValidatorTests.cs`

**Tests to write first:**
- Validator rejects `Page < 1`.
- Validator rejects `PageSize < 1`.
- Validator rejects `PageSize > 100`.
- Handler throws `ForbiddenAccessException` for authenticated non-Employer user.
- Handler throws `ForbiddenAccessException` when Employer profile is missing.
- Handler throws `ForbiddenAccessException` when Employer organization is missing.
- Handler does not query candidate data when profile or organization is missing, if testable with existing mocks/fakes.
- Handler returns only nurses with `IsAvailableForRecruitment == true`.
- Handler excludes inactive nurse accounts.
- Handler excludes unverified nurse accounts.
- Handler pagination proves both `Skip` and `Take` using enough eligible nurses to span pages and request `Page = 2`.
- Handler sorts deterministically by `YearsOfExperience` descending, `CreatedAt` descending, then `Id` ascending.
- Handler projects only allowed candidate fields.

**Required implementation notes:**
- Define `ListCandidatesQuery : IRequest<PaginatedResult<CandidateListItemDto>>` with `Page = 1` and `PageSize = 20` defaults.
- Define `ListCandidatesQueryValidator` using existing pagination messages and limits from `ListUsersQueryValidator`.
- Reuse `EmployerRoleGuard.EnsureEmployerAsync()` for Employer role enforcement.
- After role guard returns `userId`, check `EmployerProfiles` for `UserId == userId`.
- Check `EmployerOrganizations` for the returned employer profile id.
- Throw `ForbiddenAccessException` when either prerequisite is missing.
- Build the eligible candidate query only after prerequisite checks pass.
- Eligibility filter must include `NurseProfile.IsAvailableForRecruitment`, linked `User.IsActive`, and linked `User.EmailVerified`.
- Apply `CountAsync` after eligibility filters and before pagination.
- Apply deterministic sort before `Skip` and `Take`.
- Project explicitly to Recruitment DTOs; do not reuse Nurse self-service DTOs.
- DTO allowed fields: `NurseProfileId`, `Headline`, `ProfessionalSummary`, `LicenseCountryName`, `CurrentCountryName`, `YearsOfExperience`, `Skills`, `Languages`, `CertificatesSummary`, `CertificatesCount`, `LatestExperienceTitle`, `EducationSummary`.
- DTO forbidden fields: `UserId`, `Email`, `Phone`, `PasswordHash`, roles, permissions, tokens, `EmailVerified`, `IsActive`, `LicenseNumber`, CV storage/file fields, `FirstName`, `LastName`, `DisplayLabel`, navigation objects, domain entities.
- No migrations are expected in Task 1.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ListCandidates|CandidateSearch"
```
```bash
dotnet build backend/NursingPlatform.slnx
```
```bash
git status --short
```
```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after Application tests and build pass. Do not create the WebApi endpoint in Task 1.

---

### Task 2: WebApi Endpoint and Integration Tests

**Goal:** Add only the candidate listing endpoint and WebApi integration tests.

**Files:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/CandidateSearchEndpointsTests.cs`

**Tests to write first:**
- `GET /api/v1/recruitment/candidates` returns `401 Unauthorized` without JWT.
- Authenticated non-Employer returns `403 Forbidden` through mocked `ForbiddenAccessException`.
- Authenticated Employer missing profile/organization returns `403 Forbidden` through mocked `ForbiddenAccessException`.
- Invalid pagination returns `400 Bad Request` Problem Details.
- Eligible Employer returns `200 OK` with paginated candidate response.
- Raw JSON response does not contain forbidden fields: `userId`, `email`, `phone`, `passwordHash`, `roles`, `permissions`, `accessToken`, `refreshToken`, `tokenHash`, `emailVerified`, `isActive`, `licenseNumber`, `cvStorageKey`, `cvFileUrl`, `storageKey`, `internalPath`, `fileUrl`, `firstName`, `lastName`, `displayLabel`, `user`, `nurseProfile`, `licenseCountry`, `currentCountry`, `country`.
- Response shape matches `PaginatedResult<CandidateListItemDto>`.

**Required implementation notes:**
- Add only one endpoint: `GET /api/v1/recruitment/candidates`.
- Endpoint parameters are `int? page`, `int? pageSize`, and `ISender sender`.
- Endpoint builds `ListCandidatesQuery` with `Page = page ?? 1` and `PageSize = pageSize ?? 20`.
- Endpoint returns `Results.Ok(result)`.
- Use `.RequireAuthorization()` only; do not use `.AllowAnonymous()` or `.RequirePermission(...)`.
- Do not add filtering, sorting, contact request, CV, messaging, saved search, notification, admin, frontend, Phase 6C, Phase 6D, or Phase 7 endpoints.
- Follow existing WebApi test setup and JWT helper style.
- Raw JSON tests must inspect the string response before deserialization.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "CandidateSearch|ListCandidates|Recruitment"
```
```bash
dotnet build backend/NursingPlatform.slnx
```
```bash
git status --short
```
```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after focused WebApi tests and build pass. Do not proceed to final verification or tracking updates until approved.

---

### Task 3: Final Verification and Documentation Review

**Goal:** Verify Phase 6B end-to-end and decide whether authoritative docs need updates.

**Files:**
- Modify: no files expected.
- Optional docs only if implementation behavior requires it and reviewer approves scope.

**Tests to write first:**
- No new tests expected in Task 3 unless final verification exposes a gap.

**Required implementation notes:**
- Run full backend build and full solution tests.
- Run EF pending model check.
- Do not create migrations unless EF verification proves model drift; if model drift appears, stop and report before changing anything.
- Review `docs/api/api-design.md` and `docs/database/database-design.md`; do not edit docs if existing conventions already cover Phase 6B.
- Confirm no Phase 6C, Phase 6D, Phase 7, frontend, contact request, advanced filtering/sorting, payment, notification, saved search, admin, messaging, or CV access work was started.
- Confirm raw JSON security tests cover all forbidden fields.

**Verification commands:**
```bash
dotnet build backend/NursingPlatform.slnx
```
```bash
dotnet test backend/NursingPlatform.slnx
```
```bash
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
```
```bash
git status --short
```
```bash
git diff --cached --stat
```
```bash
git diff --stat
```

**Stop condition:** Stop for review after final verification evidence is reported. Do not update `CURRENT_TASK.md` or `TASKS.md` in Task 3.

---

### Task 4: Tracking Documentation Update

**Goal:** Update project tracking only after Task 3 is approved.

**Files:**
- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

**Tests to write first:**
- No automated tests required for tracking-only edits.

**Required implementation notes:**
- Update `CURRENT_TASK.md` to Phase 6B completion status only after final verification is approved.
- Update `TASKS.md` to mark only `Candidate search` complete.
- Do not mark `Candidate filtering` complete.
- Do not mark `Contact requests` complete.
- Do not mark all Phase 6 complete unless explicitly instructed.
- Do not start Phase 6C, Phase 6D, Phase 7, frontend, or implementation work.

**Verification commands:**
```bash
git diff -- CURRENT_TASK.md TASKS.md
```
```bash
git status --short
```
```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after tracking diff and git status are reported. Do not stage or commit.

---

### Task 5: Final Phase 6B Commit

**Goal:** Commit approved Phase 6B implementation, tests, spec, plan, and tracking changes only after explicit reviewer approval.

**Files:**
- Stage only explicitly approved Phase 6B files.
- Include approved spec: `docs/superpowers/specs/2026-07-10-candidate-search-foundation.md`.
- Include approved plan: `docs/superpowers/plans/2026-07-10-candidate-search-foundation.md`.
- Include Phase 6B implementation/test/tracking files approved in Tasks 1-4.

**Tests to write first:**
- No new tests in commit-only task.

**Required implementation notes:**
- Before staging, inspect `git status --short`, `git diff --stat`, and `git log --oneline -10`.
- Use explicit `git add <path>` commands only.
- Never use `git add .`.
- Do not stage `AGENTS.md`, unrelated docs, personal files, Phase 6C/6D/7 files, frontend files, or generated artifacts outside approved scope.
- Before committing, run `git diff --cached --name-only` and `git diff --cached --stat` and verify the staged list is only approved Phase 6B files.
- Commit message should be concise and scoped, for example `feat: add employer candidate search foundation`.

**Verification commands:**
```bash
git diff --cached --name-only
```
```bash
git diff --cached --stat
```
```bash
git commit -m "feat: add employer candidate search foundation"
```
```bash
git status --short
```
```bash
git log -1 --oneline
```

**Stop condition:** Stop after reporting staged file list, staged diff stat, commit hash/message, post-commit status, and latest log line. Do not proceed to Phase 6C, Phase 6D, Phase 7, or frontend.

---

## Self-Review Checklist

- Spec coverage: Task 1 covers Application query, DTOs, validation, eligibility, prerequisites, pagination, deterministic sorting, and safe projection. Task 2 covers the single WebApi endpoint and integration/security tests. Task 3 covers final build/test/EF/docs verification. Task 4 covers tracking only. Task 5 covers commit only.
- Scope check: The plan excludes advanced filtering, advanced sorting, contact requests, CV access, contact info, messaging, saved searches, notifications, payments, admin approval, frontend, Phase 6C, Phase 6D, and Phase 7.
- Type consistency: Plan uses `ListCandidatesQuery`, `CandidateListItemDto`, `CandidateLanguageDto`, `PaginatedResult<CandidateListItemDto>`, and `ForbiddenAccessException` consistently.
- Placeholder scan: No placeholder implementation steps are intentionally left unresolved.
