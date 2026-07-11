# Candidate Filtering Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Phase 6C candidate filtering on the existing employer-facing candidate listing endpoint.

**Architecture:** Phase 6C extends the existing Recruitment query and endpoint without adding routes or changing the DTO response shape. WebApi remains thin and binds query parameters into `ListCandidatesQuery`; Application enforces Employer access, employer prerequisites, baseline nurse eligibility, optional filters, count, deterministic sorting, pagination, and safe projection. No Domain changes are expected, and no migrations are expected unless final verification proves an approved index/model change is required.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, xUnit, Moq, MockQueryable.Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 6C: Candidate Filtering.
- Extend only the existing endpoint: `GET /api/v1/recruitment/candidates`.
- Do not add a new endpoint.
- Do not add contact requests, messaging, CV access, nurse contact info, saved searches, notifications, payments, admin approval, frontend, Phase 6D, or Phase 7 behavior.
- Do not add sorting parameters.
- Keep the endpoint `.RequireAuthorization()` only; do not add `.RequirePermission(...)` or `.AllowAnonymous()`.
- Preserve Application-layer Employer role enforcement and employer profile/organization prerequisites before candidate queries.
- Preserve baseline candidate eligibility: `IsAvailableForRecruitment == true`, `User.IsActive == true`, and `User.EmailVerified == true`.
- Apply filters after employer prerequisites and baseline eligibility.
- Apply filters before `CountAsync`, deterministic sorting, `Skip`, and `Take`.
- Preserve default pagination: `Page = 1`, `PageSize = 20`, `PageSize <= 100`.
- Preserve default sorting: `YearsOfExperience` descending, `CreatedAt` descending, `Id` ascending.
- Preserve `CandidateListItemDto` and `CandidateLanguageDto` response shape.
- Preserve raw JSON security checks for forbidden fields and values.
- Support optional filters: `licenseCountryId`, `currentCountryId`, `minimumYearsOfExperience`, `skills`, and `languageId`.
- Support both repeated and comma-separated skill filters: `?skills=ICU&skills=Triage` and `?skills=ICU,Triage`.
- Use one skill parsing pipeline: read all supplied values, split comma-separated entries, trim whitespace, reject blanks, normalize with existing nurse skill normalization behavior, deduplicate by normalized name, and match ALL supplied normalized skills.
- Phase 6C maximum normalized skill filter count is 20. The implementation plan must not change this.
- Invalid GUID values for `licenseCountryId`, `currentCountryId`, or `languageId` must return `400 Bad Request` through WebApi binding/model validation behavior.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` until Task 4 and only after Task 3 approval.
- Do not stage or commit until explicitly instructed.

## Planned File Structure

- Modify `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQuery.cs`: add optional filter properties.
- Modify `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryValidator.cs`: validate minimum experience and normalized skill inputs.
- Modify `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryHandler.cs`: parse/normalize skills and apply filters before count/sort/pagination.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`: bind approved query parameters to `ListCandidatesQuery` on the existing endpoint only.
- Modify `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryHandlerTests.cs`: add Application filtering coverage.
- Modify `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryValidatorTests.cs`: add validation coverage.
- Modify `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/CandidateSearchEndpointsTests.cs`: add endpoint binding, invalid GUID, auth-shape, and raw JSON filtered-response coverage.
- Modify `CURRENT_TASK.md`: Task 4 only, after Task 3 approval, to mark Phase 6C complete.
- Modify `TASKS.md`: Task 4 only, after Task 3 approval, to mark only Candidate filtering complete.
- Optional only with approval/evidence: Infrastructure EF configuration and migration files if Task 3 proves a required index/model change.

---

### Task 1: Application Query, Validator, Filtering, and Application Tests

**Goal:** Add Phase 6C filter properties, validation, skill normalization, query filtering, and Application tests without changing the WebApi endpoint.

**Files expected to create/modify:**
- Modify: `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQuery.cs`
- Modify: `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryValidator.cs`
- Modify: `backend/src/NursingPlatform.Application/Recruitment/Queries/ListCandidates/ListCandidatesQueryHandler.cs`
- Modify: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryHandlerTests.cs`
- Modify: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ListCandidatesQueryValidatorTests.cs`

**Tests to write first:**
- `Validate_MinimumYearsOfExperienceLessThanZero_ShouldHaveError`
- `Validate_BlankSkill_ShouldHaveError`
- `Validate_MoreThanTwentyNormalizedSkills_ShouldHaveError`
- `Validate_OverLengthSkill_ShouldHaveError`
- `Handle_WithLicenseCountryIdFilter_ReturnsOnlyMatchingEligibleCandidates`
- `Handle_WithCurrentCountryIdFilter_ReturnsOnlyMatchingEligibleCandidates`
- `Handle_WithMinimumYearsOfExperienceFilter_IncludesBoundaryAndExcludesLowerValues`
- `Handle_WithLanguageIdFilter_ReturnsOnlyCandidatesWithLanguage`
- `Handle_WithSkillFilter_NormalizesInputAndMatchesStoredNormalizedName`
- `Handle_WithDuplicateSkillFilters_DoesNotChangeResult`
- `Handle_WithMultipleSkillFilters_RequiresAllSkills`
- `Handle_WithMultipleFilterTypes_ComposesWithAndSemantics`
- `Handle_WithFilters_ExcludesUnavailableInactiveAndUnverifiedNurses`
- `Handle_WithFilters_PaginationSkipsAndTakesAfterFiltering`
- `Handle_WithFilters_DefaultSortingRemainsDeterministic`

**Required implementation notes:**
- Extend `ListCandidatesQuery` with:
  - `Guid? LicenseCountryId`
  - `Guid? CurrentCountryId`
  - `int? MinimumYearsOfExperience`
  - `IReadOnlyCollection<string> Skills`, defaulting to `[]`
  - `Guid? LanguageId`
- Keep `Page` and `PageSize` defaults unchanged.
- Reuse `SkillNameNormalizer.NormalizeName` and `SkillNameNormalizer.NormalizeForComparison` from `NursingPlatform.Application.Nurses.Commands.UpdateNurseSkills`.
- Add a private helper in `ListCandidatesQueryHandler`, for example `NormalizeSkillFilters(IReadOnlyCollection<string> skills)`, that:
  - splits each supplied string on `,`;
  - trims each entry;
  - normalizes each entry with `SkillNameNormalizer.NormalizeName`;
  - converts to comparison form with `SkillNameNormalizer.NormalizeForComparison`;
  - de-duplicates with `Distinct()`;
  - returns a list of normalized names.
- Apply validator rules before the handler depends on normalized values:
  - `MinimumYearsOfExperience >= 0` when provided.
  - Each parsed skill entry must be non-blank after splitting and trimming.
  - Normalized skill count must be `<= 20`.
  - Each display-normalized skill name length must be `<= 100`.
- Keep validator and handler parsing rules aligned so Application tests prove the same semantics used by endpoint-bound requests.
- Preserve the current prerequisite order: call `EmployerRoleGuard.EnsureEmployerAsync`, check `EmployerProfiles`, check `EmployerOrganizations`, then build the candidate query.
- Apply baseline eligibility first:
  - `p.IsAvailableForRecruitment`
  - `p.User.IsActive`
  - `p.User.EmailVerified`
- Apply optional filters to `eligibleCandidates` before `CountAsync`:
  - `LicenseCountryId`: `p.LicenseCountryId == request.LicenseCountryId`
  - `CurrentCountryId`: `p.CurrentCountryId == request.CurrentCountryId`
  - `MinimumYearsOfExperience`: `p.YearsOfExperience >= request.MinimumYearsOfExperience`
  - `LanguageId`: `_context.NurseLanguages.Any(l => l.NurseProfileId == p.Id && l.LanguageId == request.LanguageId)`
  - `Skills`: require ALL normalized skill names by applying one `.Where(...)` per normalized skill name or an equivalent EF-translatable all-match pattern.
- Do not expose `NurseSkill.NormalizedName` in any DTO.
- Keep `totalCount` based on filtered eligible candidates only.
- Keep deterministic sorting before `Skip` and `Take`.
- Keep the existing projection and enrichment queries scoped to `profileIds` from the filtered page.
- Keep existing tests for employer prerequisites, baseline eligibility, pagination, sorting, and DTO safety.
- Do not modify WebApi files in Task 1.
- Do not create migrations in Task 1.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ListCandidates|CandidateSearch"
```
```bash
dotnet build backend/NursingPlatform.slnx
```
```bash
git status --short --untracked-files=all
```
```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after focused Application tests and build pass. Do not modify the WebApi endpoint in Task 1.

---

### Task 2: WebApi Query Binding and Integration Tests

**Goal:** Bind Phase 6C filters on the existing endpoint and prove endpoint behavior without adding new endpoints or changing authorization.

**Files expected to create/modify:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Modify: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/CandidateSearchEndpointsTests.cs`

**Tests to write first:**
- `ListCandidates_SendsQueryWithProvidedFilters`
- `ListCandidates_SendsQueryWithRepeatedSkills`
- `ListCandidates_SendsQueryWithCommaSeparatedSkills`
- `ListCandidates_WithInvalidFilterValidation_ReturnsValidationProblemDetails`
- `ListCandidates_WithInvalidGuidFilter_ReturnsBadRequest`
- `ListCandidates_WithFilteredEligibleEmployer_ReturnsPaginatedCandidateResponseWithoutSensitiveFields`
- `ListCandidates_UsesRequireAuthorizationOnly_WithoutPermissionSetup`

**Required implementation notes:**
- Modify only the existing `api.MapGet("/recruitment/candidates", ...)` endpoint.
- Keep `.WithName("ListRecruitmentCandidates")`.
- Keep `.RequireAuthorization()`.
- Do not add `.RequirePermission(...)`.
- Do not add `.AllowAnonymous()`.
- Do not add a new Recruitment route.
- Bind approved parameters only:
  - `int? page`
  - `int? pageSize`
  - `Guid? licenseCountryId`
  - `Guid? currentCountryId`
  - `int? minimumYearsOfExperience`
  - `string[]? skills`
  - `Guid? languageId`
  - `ISender sender`
- Build `ListCandidatesQuery` with defaults:
  - `Page = page ?? 1`
  - `PageSize = pageSize ?? 20`
  - `LicenseCountryId = licenseCountryId`
  - `CurrentCountryId = currentCountryId`
  - `MinimumYearsOfExperience = minimumYearsOfExperience`
  - `Skills = skills ?? []`
  - `LanguageId = languageId`
- Verify repeated skills remain separate entries in `ListCandidatesQuery.Skills`.
- Verify comma-separated skills are passed as the supplied raw string entry; Application validation/handler parsing owns splitting and normalization.
- Invalid GUID values such as `?licenseCountryId=not-a-guid` must return `400 Bad Request` through WebApi binding/model validation behavior before `ISender.Send` is called.
- Invalid Application-level filter validation, such as `minimumYearsOfExperience=-1` or blank skills, must return validation Problem Details with `400`.
- Keep raw JSON assertions before deserialization using the existing `AssertDoesNotExposeForbiddenCandidateFields` helper.
- Keep existing auth tests proving `401` without JWT and `403` from Application-layer forbidden behavior.
- Keep permission service mock reset behavior but do not configure permissions for this endpoint.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "CandidateSearch|ListCandidates|Recruitment"
```
```bash
dotnet build backend/NursingPlatform.slnx
```
```bash
git status --short --untracked-files=all
```
```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after focused WebApi tests and build pass. Do not proceed to final verification, tracking updates, staging, or commit until approved.

---

### Task 3: Final Verification and Documentation/Index Review

**Goal:** Verify Phase 6C end-to-end, review docs/indexing implications, and confirm no out-of-scope work was started.

**Files expected to create/modify:**
- Modify: no files expected.
- Optional only after explicit reviewer approval: docs or EF index/migration files if final verification proves an implementation-required change.

**Tests to write first:**
- No new tests expected unless Task 3 finds a missing Phase 6C requirement or a regression gap.

**Required implementation notes:**
- Run full backend build.
- Run full solution tests.
- Run EF pending model check.
- If EF reports pending model changes, stop and report before creating migrations.
- Review `docs/api/api-design.md` for endpoint and Problem Details consistency.
- Review `docs/database/database-design.md` and existing EF configurations for indexing implications.
- Existing helpful indexes include:
  - `NurseProfiles.UserId` unique.
  - `NurseProfiles.LicenseCountryId`.
  - `NurseProfiles.CurrentCountryId`.
  - `NurseSkills.NurseProfileId`.
  - `NurseSkills (NurseProfileId, NormalizedName)` unique.
  - `NurseLanguages.NurseProfileId`.
  - `NurseLanguages (NurseProfileId, LanguageId)` unique.
- Consider but do not add without evidence and approval:
  - `NurseSkills (NormalizedName, NurseProfileId)`.
  - `NurseLanguages (LanguageId, NurseProfileId)`.
  - candidate listing composite indexes for eligibility/sort.
- Confirm no DTO shape changes were introduced.
- Confirm raw JSON security tests still inspect response text before deserialization.
- Confirm no Phase 6D, Phase 7, contact request, frontend, sorting parameter, CV access, contact info, messaging, saved search, notification, payment, or admin work was started.

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
git status --short --untracked-files=all
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

**Files expected to create/modify:**
- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

**Tests to write first:**
- No automated tests required for tracking-only edits.

**Required implementation notes:**
- Update `CURRENT_TASK.md` to Phase 6C completion status only after final verification is approved.
- Update `TASKS.md` to mark only `Candidate filtering` complete.
- Do not mark `Contact requests` complete.
- Do not mark all Phase 6 complete unless explicitly instructed.
- Do not start Phase 6D, Phase 7, frontend, or implementation work outside approved Phase 6C tracking edits.

**Verification commands:**
```bash
git diff -- CURRENT_TASK.md TASKS.md
```
```bash
git status --short --untracked-files=all
```
```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after tracking diff and git status are reported. Do not stage or commit.

---

### Task 5: Final Phase 6C Commit

**Goal:** Commit approved Phase 6C spec, plan, implementation, tests, and tracking changes only after explicit reviewer approval.

**Files expected to create/modify:**
- Stage only explicitly approved Phase 6C files.
- Include approved spec: `docs/superpowers/specs/2026-07-11-candidate-filtering.md`.
- Include approved plan: `docs/superpowers/plans/2026-07-11-candidate-filtering.md`.
- Include approved Phase 6C implementation, tests, and tracking files.
- Do not stage `codex_res/codex_report.md`.

**Tests to write first:**
- No new tests in commit-only task.

**Required implementation notes:**
- Before staging, inspect `git status --short --untracked-files=all`, `git diff --stat`, and `git log --oneline -10`.
- Use explicit `git add <path>` commands only.
- Never use `git add .`.
- Do not stage `AGENTS.md`, unrelated docs, personal files, report files, Phase 6D/Phase 7 files, frontend files, generated artifacts, or unapproved migrations.
- Before committing, run `git diff --cached --name-only` and `git diff --cached --stat`.
- Verify the staged list contains only approved Phase 6C files.
- Commit message should be concise and scoped, for example `feat: add candidate filtering`.

**Verification commands:**
```bash
git status --short --untracked-files=all
```
```bash
git diff --cached --name-only
```
```bash
git diff --cached --stat
```
```bash
git commit -m "feat: add candidate filtering"
```
```bash
git status --short --untracked-files=all
```
```bash
git log -1 --oneline
```

**Stop condition:** Stop after reporting staged file list, staged diff stat, commit hash/message, post-commit status, and latest log line. Do not proceed to Phase 6D, Phase 7, contact requests, frontend, or sorting parameters.

---

## Self-Review Checklist

- Spec coverage: Task 1 covers Application query properties, validator rules, filtering, skill normalization, MATCH ALL semantics, pagination, sorting, and tests. Task 2 covers existing endpoint binding, invalid GUID behavior, auth shape, raw JSON security, and WebApi tests. Task 3 covers full verification and docs/index review. Task 4 covers tracking only. Task 5 covers commit only.
- Scope check: The plan excludes new endpoints, contact requests, messaging, CV access, contact info, saved searches, notifications, payments, admin approval, frontend, sorting parameters, Phase 6D, and Phase 7.
- Type consistency: Plan uses `ListCandidatesQuery`, `CandidateListItemDto`, `CandidateLanguageDto`, `PaginatedResult<CandidateListItemDto>`, `SkillNameNormalizer`, and existing Recruitment test files consistently.
- Placeholder scan: No unresolved placeholders are intentionally left in this plan.
