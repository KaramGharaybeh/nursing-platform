# Preparation Package Stage 1 Practice Progress Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add nurse-owned practice progress tracking for preparation package practice collections without coupling practice activity to official exam scoring, attempts, or analytical report evidence.

**Architecture:** Implement progress as a new Stage 1 runtime capability scoped to one nurse profile, one package purchase entitlement, one practice collection version, and one practice item. Keep business rules in Domain/Application, persistence in Infrastructure, and routes thin in WebApi. Reuse existing `PracticeAccess` entitlement authorization for active writes while allowing owning nurses to read historical progress after expiry.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, Clean Architecture, CQRS/MediatR patterns, FluentValidation, Entity Framework Core code-first migrations, PostgreSQL, xUnit integration and application tests.

## Global Constraints

- Execute only practice progress runtime work described by this plan.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless an explicitly approved documentation/status finalization task says to do so.
- Do not stage, commit, push, reset, clean, or stash unless explicitly authorized by the reviewer.
- Do not create frontend/design changes.
- Do not modify Stage 4 analytical report evidence semantics; package analytical reports remain exam-session based.
- Practice progress is nurse-owned and scoped to one package purchase entitlement.
- Practice progress is isolated per entitlement and package practice collection version.
- Practice progress is not an official exam score, is not employer-visible in v1, and is not Stage 4 report evidence.
- Practice progress writes require active `PracticeAccess` and an active entitlement window.
- Practice progress historical reads are allowed after expiry for the owning nurse.
- Practice actions must never consume package exam attempts.
- Persist answered practice items only; unanswered state may be derived from the absence of a progress row.
- Key progress by nurse profile id, package purchase entitlement id, practice collection version id, and practice item id.
- Do not key or reference official exam session, question, or answer identifiers.
- Re-answering while active overwrites latest answer, correctness, and timestamp.
- Derived counters must not be stored.
- No employer/admin practice progress routes in v1.
- Stop for clarification if implementation requires changing Stage 4 report evidence semantics.
- Stop for clarification if `PracticeAccess` authorization cannot be reused safely.
- Stop for clarification if persistence would require FK coupling to official exam question, answer, or session tables.
- Stop for clarification if route ownership cannot hide non-owned resources safely.

## Deferred Out of Scope

- Retry history.
- Spaced repetition or retraining workflows.
- Adaptive practice.
- Workspace/dashboard aggregation.
- Employer visibility.
- Practice progress in analytical report evidence.
- Offline sync.
- Progress export.
- Cross-package progress merging.

## Planned File Structure

- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeProgressState.cs` for explicit answered-state values: `AnsweredCorrect` and `AnsweredIncorrect`; `Unanswered` remains derived from missing rows.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeProgress.cs` for the aggregate/entity that enforces entitlement/version/item/nurse identity, latest selected option, correctness, and timestamp updates.
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose `DbSet<PracticeProgress> PracticeProgresses`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs` to register the new set.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs` to map table, required fields, uniqueness, indexes, and relationships to package entitlement, practice collection version, practice item, and selected answer option.
- Create an EF Core migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` for the practice progress table.
- Create Application DTOs under `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/DTOs/` for summary, item progress, and update responses.
- Create Application commands/queries and handlers under `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/`.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` only to add nurse-owned practice progress endpoints.
- Add or modify backend tests under the existing Domain, Application, and WebApi test projects following established naming, authorization, and fixture patterns.

---

### Task 1: Domain model and domain tests

**Files:**
- Create: `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeProgressState.cs`
- Create: `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeProgress.cs`
- Test: matching domain test file in the existing Domain test project and namespace used for preparation package domain tests.

**Interfaces:**
- Produces: `PracticeProgress.Create(Guid nurseProfileId, Guid packagePurchaseEntitlementId, Guid practiceCollectionVersionId, Guid practiceItemId, Guid selectedAnswerOptionId, bool isCorrect, DateTimeOffset answeredAt)`.
- Produces: `PracticeProgress.UpdateAnswer(Guid selectedAnswerOptionId, bool isCorrect, DateTimeOffset answeredAt)`.
- Produces: read-only properties for nurse profile id, entitlement id, collection version id, item id, selected answer option id, state, correctness, answered timestamp, created timestamp, and updated timestamp according to existing entity timestamp conventions.

- [ ] Write domain tests proving creation stores all scope identifiers and sets `AnsweredCorrect` when `isCorrect` is true.
- [ ] Write domain tests proving creation stores all scope identifiers and sets `AnsweredIncorrect` when `isCorrect` is false.
- [ ] Write domain tests proving `UpdateAnswer` overwrites selected answer option, correctness, state, and answered timestamp without changing scope identifiers.
- [ ] Write domain tests proving invalid empty identifiers are rejected consistently with existing domain guard patterns.
- [ ] Run the focused domain tests and confirm the new tests fail before implementation.
- [ ] Implement the enum and entity using existing preparation package domain patterns.
- [ ] Re-run the focused domain tests and confirm they pass.

### Task 2: Application contracts, validators, and tests

**Files:**
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/DTOs/PracticeProgressItemDto.cs`
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/DTOs/PracticeProgressSummaryDto.cs`
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/DTOs/PracticeProgressUpdateResponseDto.cs`
- Create: query, command, and validator files under `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/` following existing Application naming patterns.
- Test: matching Application unit test files in the existing Application test project.

**Interfaces:**
- Consumes: `PracticeProgress` from Task 1.
- Produces: a query for nurse-owned progress summary by entitlement id.
- Produces: a command for recording/updating one practice item answer by entitlement id, practice item id, and selected answer option id.
- Produces: validators requiring non-empty route/body identifiers.

- [ ] Write validator tests for empty entitlement id, practice item id, and selected answer option id.
- [ ] Write handler tests proving historical reads return progress for the owning nurse after entitlement expiry.
- [ ] Write handler tests proving progress reads hide or reject non-owned entitlements using existing not-found/authorization exception patterns.
- [ ] Write handler tests proving write attempts require active `PracticeAccess` through `PackageBenefitAuthorizationService` or its existing abstraction.
- [ ] Write handler tests proving selected answer correctness is evaluated from the package practice answer option, not from exam answer data.
- [ ] Run focused Application tests and confirm the new tests fail before implementation.

### Task 3: Application implementation

**Files:**
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`
- Create/modify: handler files under `backend/src/NursingPlatform.Application/PreparationPackages/PracticeProgress/`
- Test: focused Application tests from Task 2.

**Interfaces:**
- Consumes: query, command, validators, DTOs, and `PracticeProgress` from earlier tasks.
- Produces: read model with derived totals: total practice items, answered count, correct count, incorrect count, and per-item state where missing rows are surfaced as `Unanswered` without persisting them.
- Produces: update command behavior that inserts a row for first answer and overwrites the existing row on re-answer while active.

- [ ] Implement explicit projections that do not expose password hashes, internal tokens, domain entities, persistence entities, or official exam identifiers.
- [ ] Implement read ownership checks against the nurse profile and package purchase entitlement.
- [ ] Implement write authorization using existing active `PracticeAccess` entitlement authorization.
- [ ] Implement selected option validation so the answer option belongs to the requested practice item and package practice collection version.
- [ ] Implement upsert/re-answer logic without storing derived counters.
- [ ] Re-run focused Application tests and confirm they pass.

### Task 4: Persistence mapping and migration

**Files:**
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`
- Create: EF Core migration files under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/`
- Test: persistence/application tests that exercise EF model validation if such tests already exist.

**Interfaces:**
- Consumes: `PracticeProgress` and `IApplicationDbContext.PracticeProgresses`.
- Produces: `PracticeProgresses` table mapped with a uniqueness constraint on `(NurseProfileId, PackagePurchaseEntitlementId, PracticeCollectionVersionId, PracticeItemId)`.

- [ ] Add EF mapping for required scalar fields and enum storage consistent with existing project conventions.
- [ ] Add relationships to `PackagePurchaseEntitlement`, `PracticeCollectionVersion`, `PracticeItem`, and selected `PracticeAnswerOption`.
- [ ] Add indexes supporting lookup by nurse profile and entitlement.
- [ ] Generate an EF Core migration using the project’s documented migration command.
- [ ] Review the generated migration to confirm it does not create or reference official exam question, answer, or session foreign keys.
- [ ] Run the focused persistence/model tests or build if no focused tests exist.

### Task 5: WebApi nurse-owned endpoints

**Files:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs`
- Test: matching WebApi integration tests under the existing WebApi test project.

**Interfaces:**
- Consumes: Application query/command from Tasks 2 and 3.
- Produces: nurse-owned endpoints under `/api/v1/me/nurse-profile/preparation-packages/...` only.

- [ ] Add endpoint for reading practice progress summary for one package entitlement.
- [ ] Add endpoint for recording or updating one practice item answer.
- [ ] Use `.RequireAuthorization()` or the existing nurse-profile route authorization pattern exactly as established for `/me/nurse-profile` routes; do not add employer/admin routes.
- [ ] Write integration tests proving unauthenticated requests return `401`.
- [ ] Write integration tests proving authenticated non-owner access is hidden or rejected according to existing route ownership patterns.
- [ ] Write integration tests proving valid owner read succeeds without exposing official exam identifiers or sensitive fields in raw JSON.
- [ ] Write integration tests proving valid owner write succeeds and does not consume package exam attempts.
- [ ] Run focused WebApi tests and confirm they pass.

### Task 6: Integration, security, and compatibility verification

**Files:**
- Test-only modifications as needed in existing backend test projects.
- No frontend/design files.
- No Stage 4 report behavior changes.

**Interfaces:**
- Consumes: completed implementation from Tasks 1 through 5.
- Produces: evidence that practice progress is isolated from official exam attempts and analytical reports.

- [ ] Add or run tests proving two entitlements for the same nurse do not share progress.
- [ ] Add or run tests proving two nurses do not share progress for the same package content.
- [ ] Add or run tests proving re-answer overwrites latest progress rather than appending retry history.
- [ ] Add or run tests proving expired entitlement reads are allowed for the owner.
- [ ] Add or run tests proving expired entitlement writes are rejected.
- [ ] Add or run tests proving Stage 4 analytical report generation still reads exam-session evidence only.
- [ ] Run backend build for the solution.
- [ ] Run all relevant backend tests for Domain, Application, Infrastructure/Persistence, and WebApi coverage.
- [ ] Run `git diff --check`.
- [ ] Run `git status --short --untracked-files=all` and confirm no files are staged unless explicitly authorized.

### Task 7: Documentation and status finalization

**Files:**
- Modify only authoritative documentation if the implementation changes API behavior, runtime semantics, or task status and the reviewer explicitly authorizes those documentation/status edits.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` without explicit instruction.

**Interfaces:**
- Consumes: verified implementation and test evidence from Task 6.
- Produces: synchronized documentation and final review evidence.

- [ ] Update API documentation only if the new endpoints are implemented and documentation changes are explicitly authorized.
- [ ] Update backend/database documentation only if the new persistence model needs source-of-truth documentation and documentation changes are explicitly authorized.
- [ ] Paste full contents of requested created/modified files when asked.
- [ ] Paste real build output, real test output, and real `git status --short` output.
- [ ] Confirm no commit was made unless explicitly instructed.
- [ ] Confirm no files were staged unless explicitly instructed.
- [ ] Stop for review and do not proceed to any next task.

## Self-Review Notes

- Spec coverage: the plan covers nurse-owned entitlement-scoped progress, active-write authorization, historical reads after expiry, overwrite-on-reanswer behavior, derived unanswered/counters, endpoint scope, persistence boundaries, and Stage 4 compatibility.
- Deferred scope is explicitly listed and excluded from implementation tasks.
- Stop conditions are copied into Global Constraints so implementers must stop rather than guess on report semantics, authorization reuse, exam FK coupling, or ownership hiding.
- The plan intentionally avoids implementation code and does not authorize staging, committing, or beginning execution.
