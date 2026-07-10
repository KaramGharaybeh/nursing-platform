# Phase 6A — Employer Profile Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement authenticated employer self-service profile and organization foundation without recruitment search or nurse data exposure.

**Architecture:** Use existing Clean Architecture and CQRS patterns. Domain entities live under `NursingPlatform.Domain.Employers`, use cases live under `NursingPlatform.Application.Employers`, EF Core configuration lives in Infrastructure, and Minimal API endpoint mappings remain thin in WebApi.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq

**Spec:** `docs/superpowers/specs/2026-07-10-employer-module.md`

## Global Constraints

- Planning document only until implementation is explicitly approved.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.
- Do not stage or commit unless explicitly instructed.
- Do not implement candidate search, filtering, contact requests, employer access to nurse data, messaging, admin employer management, frontend work, payments, exams, notifications, or Phase 7+ work.
- All Phase 6A endpoints use `.RequireAuthorization()`.
- Do not use `.RequirePermission(...)` in Phase 6A.
- Only authenticated users with the `Employer` role may create or manage employer profile and organization data.
- Enforce the `Employer` role and ownership in Application handlers using existing project patterns.
- Reuse `ForbiddenAccessException` for authenticated non-Employer users.
- Keep unauthenticated users mapped to existing 401 behavior.
- One user can have at most one `EmployerProfile` in Phase 6A.
- One employer profile can have at most one `EmployerOrganization` in Phase 6A.
- Keep employer personal identity fields on `User`; do not duplicate first name, last name, or email in `EmployerProfile`.
- Organization data is business metadata only.
- Use existing `Country` reference data for organization country when supplied.
- Validate database-backed reference data in handlers, not FluentValidation, unless the Phase 5 Country validation pattern clearly differs at implementation time.
- Missing or inactive `CountryId` must follow the same Application-layer pattern used in Phase 5 Country validation.
- Do not invent a new exception type or middleware mapping for Country validation in Phase 6A.
- If the Phase 5 Country validation pattern is unclear at implementation time, stop and ask for review.
- Do not expose nurse data to employers.
- No implementation task may proceed past its stop condition without explicit review approval.

---

## File Structure Map

Domain files:

- Create `backend/src/NursingPlatform.Domain/Employers/EmployerProfile.cs` for the employer profile aggregate root.
- Create `backend/src/NursingPlatform.Domain/Employers/EmployerOrganization.cs` for the single employer-owned organization profile.

Application files:

- Create `backend/src/NursingPlatform.Application/Employers/DTOs/EmployerProfileDto.cs`.
- Create `backend/src/NursingPlatform.Application/Employers/DTOs/EmployerOrganizationDto.cs`.
- Create `backend/src/NursingPlatform.Application/Employers/Common/EmployerRoleGuard.cs`.
- Create employer profile read/upsert queries and commands under `backend/src/NursingPlatform.Application/Employers/Queries/GetMyEmployerProfile/` and `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerProfile/`.
- Create organization read/upsert queries and commands under `backend/src/NursingPlatform.Application/Employers/Queries/GetMyEmployerOrganization/` and `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerOrganization/`.
- Modify `backend/src/NursingPlatform.Application/DependencyInjection.cs` to register `EmployerRoleGuard` following the existing `NurseRoleGuard` registration pattern.
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose `EmployerProfiles`, `EmployerOrganizations`, and existing `Countries` if not already exposed.

Infrastructure files:

- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/EmployerProfileConfiguration.cs`.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/EmployerOrganizationConfiguration.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create generated EF migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` named `AddEmployerModule` during implementation only after code changes exist.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` through EF migration generation only.

WebApi files:

- Modify the existing endpoint registration file used for grouped Minimal API mappings, expected `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` unless implementation-time inspection shows employer endpoints belong in a different existing endpoint file.

Test files:

- Create Domain tests under `backend/tests/NursingPlatform.Domain.Tests/Employers/EmployerEntitiesTests.cs`.
- Create Application tests under `backend/tests/NursingPlatform.Application.Tests/Employers/*`.
- Create WebApi integration tests under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/EmployerProfileEndpointsTests.cs`.

---

### Task 1: Domain Entities, EF Configurations, DbContext, and Migration

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Domain/Employers/EmployerProfile.cs`
- Create: `backend/src/NursingPlatform.Domain/Employers/EmployerOrganization.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/EmployerProfileConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/EmployerOrganizationConfiguration.cs`
- Create: generated EF migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` named `AddEmployerModule`
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/Employers/EmployerEntitiesTests.cs`

**Tests to write first:**

- `EmployerProfile_CanBeCreatedForUserWithoutDuplicatingIdentityFields`
- `EmployerOrganization_StoresBusinessMetadataOnly`
- `EmployerOrganization_CountryReference_IsOptional`

**Implementation requirements:**

- Add `EmployerProfile` exactly from the spec.
- Add `EmployerOrganization` exactly from the spec.
- `EmployerProfile.UserId` must have a unique index.
- `EmployerOrganization.EmployerProfileId` must have a unique index.
- `User` to `EmployerProfile` uses `DeleteBehavior.Restrict`.
- `EmployerProfile` to `EmployerOrganization` uses `DeleteBehavior.Cascade`.
- `Country` to `EmployerOrganization` uses `DeleteBehavior.Restrict`.
- Add `DbSet<EmployerProfile> EmployerProfiles` and `DbSet<EmployerOrganization> EmployerOrganizations` to `IApplicationDbContext` and `ApplicationDbContext`.
- Ensure `IApplicationDbContext` exposes `Countries` for handler reference-data validation if it is not already exposed.
- Generate migration only after entities, configurations, and DbContext changes are complete.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "Employer"
dotnet build backend/NursingPlatform.slnx
dotnet ef migrations add AddEmployerModule --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short
git diff --cached --stat
```

Expected: employer domain tests pass, build has 0 errors and 0 warnings, EF reports no pending model changes after migration generation, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after migration generation and verification output. Do not proceed to Task 2. Do not stage or commit until explicitly approved.

---

### Task 2: Employer Role Guard and Profile Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Employers/Common/EmployerRoleGuard.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/DTOs/EmployerProfileDto.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerProfile/UpsertMyEmployerProfileCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerProfile/UpsertMyEmployerProfileCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerProfile/UpsertMyEmployerProfileCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Queries/GetMyEmployerProfile/GetMyEmployerProfileQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Queries/GetMyEmployerProfile/GetMyEmployerProfileQueryHandler.cs`
- Modify: `backend/src/NursingPlatform.Application/DependencyInjection.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Employers/EmployerRoleGuardTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Employers/UpsertMyEmployerProfileCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Employers/GetMyEmployerProfileQueryHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/DependencyInjectionTests.cs` if existing Application DI tests use this file; otherwise create the matching Application DI test file used by existing patterns.

**Tests to write first:**

- `EnsureEmployerAsync_WithEmployerRole_AllowsAccess`
- `EnsureEmployerAsync_WithoutEmployerRole_ThrowsForbiddenAccessException`
- `AddApplication_ShouldRegisterEmployerRoleGuard`
- `EmployerRoleGuard_ShouldBeScoped`
- `Handle_WhenProfileDoesNotExist_ReturnsNotFound`
- `Handle_WhenProfileDoesNotExist_CreatesProfileForCurrentUser`
- `Handle_WhenProfileExists_UpdatesExistingProfileWithoutDuplicate`
- `Handle_TrimsProfileFieldsBeforeSaving`

**Implementation requirements:**

- Use the existing current-user abstraction and role access pattern established in the Application layer.
- Enforce exactly the `Employer` role name.
- Throw `ForbiddenAccessException` for authenticated non-Employer users.
- Do not accept user id or profile id from the request.
- Project only DTO properties listed in the spec.
- Do not expose `PasswordHash`, internal tokens, permissions, roles, or EF navigation objects.
- Register `EmployerRoleGuard` in Application DI, following the existing `NurseRoleGuard` registration pattern.
- Validator enforces max lengths for `JobTitle` and `Department`.
- Handler trims optional string fields before persistence and response mapping.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "EmployerRoleGuard|EmployerProfile"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: relevant Application tests pass, build has 0 errors and 0 warnings, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after employer role guard and profile Application verification. Do not proceed to Task 3. Do not stage or commit until explicitly approved.

---

### Task 3: Organization Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Employers/DTOs/EmployerOrganizationDto.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerOrganization/UpsertMyEmployerOrganizationCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerOrganization/UpsertMyEmployerOrganizationCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Commands/UpsertMyEmployerOrganization/UpsertMyEmployerOrganizationCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Queries/GetMyEmployerOrganization/GetMyEmployerOrganizationQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Employers/Queries/GetMyEmployerOrganization/GetMyEmployerOrganizationQueryHandler.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Employers/UpsertMyEmployerOrganizationCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Employers/GetMyEmployerOrganizationQueryHandlerTests.cs`

**Tests to write first:**

- `Handle_WhenOrganizationDoesNotExist_ReturnsNotFound`
- `Handle_WhenProfileDoesNotExist_CreatesProfileAndOrganizationForCurrentUser`
- `Handle_WhenOrganizationExists_UpdatesExistingOrganizationWithoutDuplicate`
- `Handle_TrimsOrganizationFieldsBeforeSaving`
- `Handle_WithActiveCountryId_SavesCountryReference`
- `Handle_WithMissingCountryId_FollowsPhase5CountryValidationPattern`
- `Handle_WithInactiveCountryId_FollowsPhase5CountryValidationPattern`

**Implementation requirements:**

- Reuse `EmployerRoleGuard` from Task 2.
- Create the employer profile automatically when a valid Employer user upserts organization data and no profile exists.
- Keep only one organization row per employer profile.
- Validator enforces required `Name`, max lengths, and absolute HTTP/HTTPS URL format for `WebsiteUrl` when supplied.
- Non-HTTP/HTTPS absolute URLs are invalid.
- Handler validates `CountryId` against existing active `Country` rows.
- Missing or inactive `CountryId` must follow the same Application-layer pattern used in Phase 5 Country validation.
- Do not invent a new exception type or middleware mapping for this in Phase 6A.
- If the Phase 5 pattern is unclear at implementation time, stop and ask for review.
- Handler trims optional and required string fields before persistence and response mapping.
- Response includes `CountryName` when a country is available.
- Do not expose nurse data, membership data, invitations, or authorization internals.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "EmployerOrganization"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: relevant Application tests pass, build has 0 errors and 0 warnings, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after organization Application verification. Do not proceed to Task 4. Do not stage or commit until explicitly approved.

---

### Task 4: WebApi Endpoints and Integration Tests

**Files expected to be created/modified:**

- Modify: existing endpoint registration file for Minimal API mappings, expected `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` unless implementation-time inspection shows a more specific existing endpoint file.
- Test: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/EmployerProfileEndpointsTests.cs`

**Endpoints to add:**

- `GET /api/v1/me/employer-profile` with `.RequireAuthorization()`.
- `PUT /api/v1/me/employer-profile` with `.RequireAuthorization()`.
- `GET /api/v1/me/employer-profile/organization` with `.RequireAuthorization()`.
- `PUT /api/v1/me/employer-profile/organization` with `.RequireAuthorization()`.

**Tests to write first:**

- `GetEmployerProfile_WithoutJwt_ReturnsUnauthorized`
- `PutEmployerProfile_WithoutJwt_ReturnsUnauthorized`
- `GetEmployerOrganization_WithoutJwt_ReturnsUnauthorized`
- `PutEmployerOrganization_WithoutJwt_ReturnsUnauthorized`
- `PutEmployerProfile_WithAuthenticatedNonEmployer_ReturnsForbidden`
- `PutEmployerOrganization_WithAuthenticatedNonEmployer_ReturnsForbidden`
- `PutEmployerProfile_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields`
- `GetEmployerProfile_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields`
- `PutEmployerOrganization_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields`
- `GetEmployerOrganization_WithEmployerJwt_ReturnsOkAndDoesNotExposeSensitiveFields`
- `PutEmployerOrganization_WithInvalidPayload_ReturnsValidationProblemDetails`

**Implementation requirements:**

- Endpoint handlers must be thin and call MediatR commands/queries.
- Do not add endpoints outside the four approved Phase 6A endpoints.
- Use `.RequireAuthorization()` exactly; do not use `.AllowAnonymous()` or `.RequirePermission(...)`.
- Follow existing route group, versioning, DTO binding, and response conventions.
- Integration tests must use existing JWT helper and WebApi factory patterns.
- Tests for sensitive fields must inspect raw JSON before deserializing.
- Endpoint tests for `.RequireAuthorization()` must not configure or depend on permission-service setup.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "EmployerProfile|EmployerOrganization"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: relevant WebApi tests pass, build has 0 errors and 0 warnings, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after endpoint and integration-test verification. Do not proceed to Task 5. Do not stage or commit until explicitly approved.

---

### Task 5: Final Verification and Documentation Review

**Files expected to be created/modified:**

- Modify docs only if implementation changed behavior, architecture, API contracts, or migration details beyond this spec.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.

**Review requirements:**

- Confirm no candidate search, filtering, contact requests, nurse profile exposure, messaging, admin employer management, frontend, payment, exam, notification, or Phase 7+ work was added.
- Confirm no `.RequirePermission(...)` employer endpoint was added.
- Confirm no out-of-scope endpoint groups were modified.
- Confirm no response exposes `PasswordHash`, internal tokens, internal authorization state, storage keys, EF navigation objects, or domain entities.
- Confirm migration is generated through EF Core only.
- Confirm no files are staged and no commit was made unless explicitly instructed.

**Verification commands:**

```bash
dotnet build backend/NursingPlatform.slnx
dotnet test backend/NursingPlatform.slnx
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short
git diff --cached --stat
```

Expected: build has 0 errors and 0 warnings, all tests pass, EF reports no pending model changes, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after pasting full required evidence. Do not proceed to Phase 6B. Do not stage or commit until explicitly approved.

---

### Task 6: Update CURRENT_TASK.md and TASKS.md

**Files expected to be created/modified:**

- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

**Prerequisites:**

- Only begin this task after implementation and final verification are approved.
- Do not perform this task during Phase 6A implementation unless the reviewer explicitly approves documentation status updates.

**Implementation requirements:**

- Mark Phase 6A complete in the authoritative task-tracking documents.
- Keep updates factual and limited to Phase 6A completion status.
- Do not add Phase 6B or Phase 7 implementation details.
- Do not stage or commit.
- Paste diffs for review.

**Verification commands:**

```bash
git diff -- CURRENT_TASK.md TASKS.md
git status --short
git diff --cached --stat
```

Expected: diffs contain only approved Phase 6A task-tracking documentation updates, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after pasting diffs and git status evidence. Do not proceed to Task 7. Do not stage or commit until explicitly approved.

---

### Task 7: Final Phase 6A Commit

**Files expected to be staged/committed:**

- Stage intended Phase 6A implementation files only after explicit reviewer approval.
- Do not use `git add .`.
- Do not stage `AGENTS.md`.
- Do not stage docs/superpowers files unless explicitly instructed.

**Prerequisites:**

- Only begin this task after explicit reviewer approval to commit.
- Confirm the approved commit message before committing.

**Implementation requirements:**

- Stage intended Phase 6A files explicitly by path.
- Verify the staged file list before committing.
- Commit with the approved message.
- Report post-commit status.
- Do not proceed to Phase 6B or Phase 7.

**Verification commands:**

```bash
git status --short
git diff --cached --name-only
git commit -m "<approved message>"
git status --short
```

Expected: only approved Phase 6A files are staged before commit, commit succeeds, post-commit status has no staged files and no unexpected modifications.

**Stop condition:** Stop for review after reporting the commit hash and post-commit status. Do not proceed to Phase 6B or Phase 7.

---

## Implementation Order

1. Task 1: Domain entities, EF configurations, DbContext, and migration.
2. Task 2: Employer role guard and profile Application layer.
3. Task 3: Organization Application layer.
4. Task 4: WebApi endpoints and integration tests.
5. Task 5: Final verification and documentation review.
6. Task 6: Update `CURRENT_TASK.md` and `TASKS.md`.
7. Task 7: Final Phase 6A commit.

Each task is independently reviewable and must stop at its stop condition. Do not batch tasks together without explicit reviewer approval.

## Plan Self-Review

- The plan removes personal phone handling from `EmployerProfile`, profile DTOs, request validation, and profile-handler requirements.
- `WebsiteUrl` validation is constrained to absolute HTTP/HTTPS URLs; non-HTTP/HTTPS absolute URLs are invalid.
- `CountryId` validation is assigned to handlers and must reuse the Phase 5 Country validation pattern without new exception or middleware mapping.
- `EmployerRoleGuard` DI registration and scoped lifetime regression tests are included in Task 2.
- Final task-tracking documentation updates and the final commit are separated into Tasks 6 and 7 and require explicit reviewer approval.
