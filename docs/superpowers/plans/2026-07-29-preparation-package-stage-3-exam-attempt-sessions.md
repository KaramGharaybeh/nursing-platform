# Preparation Package Stage 3 Exam Attempt Sessions Implementation Plan

**Status:** Complete — Stage 3 implemented and verified through Task 8

**Completion note:** Task 8 finalization used deterministic evidence only because the authorized Task 8 prompt explicitly prohibited reviewers, subagents, deep review, and model-based review. No reviewer or subagent was used.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add package-specific exam start, atomic attempt consumption, immutable session source/provenance, and deterministic retry/conflict behavior while preserving existing free and standalone exam start behavior.

**Architecture:** Stage 3 keeps Clean Architecture boundaries: domain entities enforce source/provenance/right state, application handlers orchestrate authorization and atomic workflows through `IApplicationDbContext`, Infrastructure persists EF Core configuration/migrations and PostgreSQL constraints, and WebApi exposes only the package-specific route and client-safe DTO/Problem Details responses. Package attempt authorization remains separate from standalone `ExamAccessGrant`; the existing exam start endpoint may only record `Free` or `StandaloneGrant` source for sessions it already creates.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, EF Core Code-First migrations, PostgreSQL, xUnit, FluentAssertions-style existing assertions where already used, project-owned exception/problem-details patterns.

## Global Constraints

- **Risk classification:** High risk under `docs/development/model-orchestration.md` because this touches authorization, entitlements, package attempts, session provenance, concurrency, transactions, idempotency, migrations, and sensitive response boundaries.
- **Executor/reviewer routing:** Use Main Implementation Agent (`nvidia/qwen/qwen3-coder-480b-a35b-instruct`) for implementation after explicit approval, an available non-OpenAI independent reviewer for security/authorization/concurrency review, Documentation Reviewer (`opencode/nemotron-3-ultra-free`) if docs are updated, Deterministic Verification Agent (`opencode/big-pickle`) for command evidence, and OpenAI Primary Orchestrator (`openai/gpt-5.5`) only as final gate. Before using any reviewer/subagent model, run a tiny availability probe asking it to `Reply with exactly: READY`. If a model hangs, returns empty output, is cancelled, rate-limited, blocked by permissions, or fails, do not wait and do not retry more than once. Do not hard-depend on `deepseek-v4-pro`, because it has previously been observed freezing/non-responsive. If reviewer evidence is unavailable, use deterministic evidence instead: tests, build, EF checks, grep checks, protected diff, and git status.
- **Approval gates:** This plan does not authorize implementation, database changes, migrations, staging, committing, pushing, branch deletion, or Stage 4. Obtain explicit approval before each gated action.
- **Task boundary:** Implement only Stage 3 package exam attempt/session authorization and provenance. Do not implement report generation/access, workspace runtime, employer package data, package attempt reset, frontend/design, production payment-provider changes, webhooks, refunds, subscriptions, carts, coupons, taxes, invoices, wallets, payouts, or Stage 4 placeholders.
- **Existing behavior:** Preserve `POST /api/v1/exams/{id}/sessions` as the free/standalone start operation. It must not accept package entitlement ids, infer package starts, read package rights for authorization, consume package rights, or switch existing session source.
- **New endpoint:** Add only `POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session`, requiring authentication and current-nurse ownership; no admin permission and no anonymous access.
- **Client input:** The package start route supplies only `entitlementId`. The client must not supply internal benefit right id, nurse profile id, exam id, exam version id, package ids, snapshot ids, access-window dates, source/provenance facts, right status, scoring facts, or report facts.
- **Session source values:** Every stored session has one immutable source: `Legacy`, `Free`, `StandaloneGrant`, or `PackageAttempt`. `Legacy` is historical/backfill-only for pre-Stage-3 rows and must never be used by new application-created sessions.
- **Provenance storage:** Use a one-to-one `ExamSessionProvenance` table for package provenance rather than many nullable columns on `ExamSession`.
- **Problem Details codes:** Use stable client-safe `409 Conflict` codes: `exam-session-source-conflict`, `package-attempt-consumed`, `package-entitlement-inactive`, `package-attempt-right-missing`, and `package-exam-version-unavailable`.
- **Not-found behavior:** Missing and non-owned package entitlements must both return indistinguishable `404 Not Found`.
- **Attempt consumption:** Consume `PackageExamAttemptEligibility` only when qualifying package session creation succeeds; set a dedicated nullable `ConsumedAt` UTC business timestamp.
- **Atomicity invariant:** Never persist a consumed package attempt right without the qualifying package session; never persist a package attempt session without the consumed package attempt right.
- **Concurrency:** Use optimistic concurrency plus existing/new database uniqueness and reload behavior. PostgreSQL row locking is not approved unless separate review approves it later.
- **Expired retry:** Same-source retry for an expired in-progress package session finalizes it with existing finalization behavior, then returns the deterministic consumed outcome without creating a replacement package session.
- **Security:** Public DTOs and raw JSON responses must not expose internal benefit right ids, package provenance internals, correct answers, answer identifiers, answer keys, protected options, rationales, report evidence, provider secrets, tokens, password hashes, stack traces, or internal authorization state.
- **Git hygiene:** Do not stage or commit unless explicitly authorized. Never use `git add .`. Do not modify `CURRENT_TASK.md`, `TASKS.md`, `.agent/goal-state.md`, frontend docs/design files, or unrelated endpoint groups unless separately approved.

---

## Files and Responsibilities

### Domain

- Modify `backend/src/NursingPlatform.Domain/Exams/ExamSession.cs`: add immutable `Source` property and factory overload/source-aware creation.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSessionSource.cs`: enum/string-backed source values `Legacy`, `Free`, `StandaloneGrant`, `PackageAttempt`, with `Legacy` reserved for migration backfill only.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSessionProvenance.cs`: immutable one-to-one package provenance entity with required package facts.
- Modify `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRight.cs`: add `ConsumedAt`, package-attempt consumption method, and concurrency token if not already present.
- Modify `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRightStatus.cs`: preserve existing statuses and ensure `Consumed` is used for Stage 3 attempts only where specified.

### Application

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`: add `DbSet<ExamSessionProvenance>`, unique-constraint detection methods for package session/provenance/right concurrency if needed.
- Modify `backend/src/NursingPlatform.Application/Exams/Common/ExamAccessPolicy.cs`: expose a way for existing start flow to know whether access was `Free` or `StandaloneGrant` without reading package rights.
- Modify `backend/src/NursingPlatform.Application/Exams/Commands/StartExamSession/StartExamSessionCommandHandler.cs`: set source to `Free` or `StandaloneGrant`, and return deterministic source conflict when a package in-progress session exists for the latest published version.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/StartPackageExamSession/StartPackageExamSessionCommand.cs`: MediatR command carrying only entitlement id.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/StartPackageExamSession/StartPackageExamSessionCommandHandler.cs`: package start orchestration, ownership/authorization, idempotency, transaction, snapshot creation, right consumption, reload-on-conflict behavior.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/DTOs/PackageExamSessionStartDto.cs`: additive success DTO wrapping safe `ExamSessionDto`, safe source, and safe entitlement/package summary.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/Exceptions/PackageExamSessionConflictException.cs`: application exception with stable code and message for WebApi Problem Details.
- Modify `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionDto.cs`: add safe `Source` field with values `Legacy`, `Free`, `StandaloneGrant`, `PackageAttempt`; do not add provenance/right ids. New application flows must not create `Legacy`.
- Modify `backend/src/NursingPlatform.Application/Exams/Common/ExamMapping.cs`: map safe source and continue hiding correct-answer snapshots.

### Infrastructure

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`: expose new DbSet and unique-constraint detection helpers.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`: configure required `ExamSession.Source`, one-to-one `ExamSessionProvenance`, restrictive delete behavior, indexes, and source/provenance constraints.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`: configure `PackageBenefitRight.ConsumedAt` and concurrency token/row version pattern.
- Create EF Core migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/`: add session source with safe backfill for existing rows, provenance table, consumed timestamp, indexes, constraints, and restrictive foreign keys.

### WebApi

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`: map only the new package start endpoint in the existing v1/me/nurse package area; require authentication exactly.
- Modify `backend/src/NursingPlatform.WebApi/Middleware/ExceptionMiddleware.cs`: include stable `code` extension for `PackageExamSessionConflictException`; keep responses client-safe.

### Tests

- Add/modify domain tests under `backend/tests/NursingPlatform.Domain.Tests/Exams/` and `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/`.
- Add application tests under `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageExamSessions/` and existing exam command test locations.
- Add infrastructure tests under `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/`.
- Add WebApi integration tests under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageExamSessionEndpointTests.cs` and compatibility tests in existing exam endpoint tests.

---

## Implementation Tasks

### Task 1: Domain source, provenance, and attempt consumption model

**Files:**
- Create: `backend/src/NursingPlatform.Domain/Exams/ExamSessionSource.cs`
- Create: `backend/src/NursingPlatform.Domain/Exams/ExamSessionProvenance.cs`
- Modify: `backend/src/NursingPlatform.Domain/Exams/ExamSession.cs`
- Modify: `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRight.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/Exams/ExamSessionSourceTests.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/Exams/ExamSessionProvenanceTests.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PackageBenefitRightConsumptionTests.cs`

**Interfaces:**
- Produces: `ExamSessionSource` values `Legacy`, `Free`, `StandaloneGrant`, `PackageAttempt`, with `Legacy` available only for migration/backfill representation and not selected by application start handlers.
- Produces: `ExamSession.Create(..., ExamSessionSource source)` returning an `ExamSession` whose source is set once at creation.
- Produces: `ExamSessionProvenance.CreateForPackageAttempt(...)` with required package facts.
- Produces: `PackageBenefitRight.ConsumePackageExamAttempt(DateTime consumedAtUtc)` setting `Status = Consumed` and `ConsumedAt = consumedAtUtc` only when the right is `PackageExamAttemptEligibility` and `Available`.

- [x] **Step 1: Write failing source tests**

Create tests proving all four source values are recorded at creation and cannot be changed through a public source mutation method. The assertion must check the actual `Source` property value for `Legacy`, `Free`, `StandaloneGrant`, and `PackageAttempt`. The `Legacy` test must name and document that the value exists for migration/backfill representation only.

- [x] **Step 2: Run domain source tests and verify they fail**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests/NursingPlatform.Domain.Tests.csproj --filter "ExamSession_Create"
```

Expected: fails because `ExamSessionSource` and source-aware creation do not exist yet.

- [x] **Step 3: Add source enum and source-aware session creation**

Implement `ExamSessionSource` and update `ExamSession.Create` to require a source for all new code paths. Preserve a compatibility overload only if needed temporarily by tests, and remove it before final verification if all call sites can be updated.

- [x] **Step 4: Write failing provenance tests**

Create tests named exactly:

- `ExamSessionProvenance_CreateForPackageAttempt_CapturesRequiredPackageFacts`
- `ExamSessionProvenance_CreateForPackageAttempt_RejectsMissingEntitlementRightOrSnapshotIds`

The tests must assert every required package fact listed in the approved specification and must assert that default `Guid.Empty` ids for entitlement, right, snapshot, order, order item, package definition, package version, offer, included exam, included exam version, reporting profile publication, or practice collection version are rejected.

- [x] **Step 5: Add provenance entity**

Implement `ExamSessionProvenance` with a package-only factory. Do not include report evidence, report status, answer ids, correct answer keys, rationales, free-text provenance, provider secrets, tokens, or password hashes.

- [x] **Step 6: Write failing attempt-consumption tests**

Create tests named exactly:

- `PackageBenefitRight_ConsumePackageExamAttempt_TransitionsAvailableToConsumed`
- `PackageBenefitRight_ConsumePackageExamAttempt_SetsConsumedAt`
- `PackageBenefitRight_ConsumePackageExamAttempt_WhenNotAvailable_Throws`
- `PackageBenefitRight_Expire_DoesNotOverwriteConsumedAttemptRight`

- [x] **Step 7: Add attempt consumption behavior**

Add nullable `ConsumedAt` and a package-attempt-specific consumption method. The method must reject non-attempt rights and non-available rights. Existing expiry behavior must not overwrite consumed attempt rights.

- [x] **Step 8: Run domain tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests/NursingPlatform.Domain.Tests.csproj --filter "ExamSession|PackageBenefitRight_ConsumePackageExamAttempt|PackageBenefitRight_Expire_DoesNotOverwriteConsumedAttemptRight"
```

Expected: all new and existing matching domain tests pass.

- [x] **Step 9: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 2: Persistence schema, EF configuration, migration, and data integrity tests

**Files:**
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/<timestamp>_AddExamSessionSourceAndPackageProvenance.cs`
- Create/modify: migration designer/model snapshot generated by EF Core
- Test: `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ExamSessionProvenanceConfigurationTests.cs`
- Test: `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs`

**Interfaces:**
- Consumes: `ExamSessionSource`, `ExamSessionProvenance`, `PackageBenefitRight.ConsumedAt` from Task 1.
- Produces: `IApplicationDbContext.ExamSessionProvenances`.
- Produces: one-to-one provenance mapping and restrictive foreign keys.
- Produces: source/provenance and consumed-at columns through EF migration.

- [x] **Step 1: Write failing EF configuration tests**

Create tests named exactly:

- `ExamSessionConfiguration_PersistsSessionSourceAsRequiredString`
- `ExamSessionMigration_BackfillsExistingRowsToLegacySource`
- `ExamSessionProvenanceConfiguration_UsesOneToOneSessionRelationship`
- `ExamSessionProvenanceConfiguration_UsesRestrictDeleteBehavior`
- `ExamSessionProvenanceConfiguration_IndexesPackageEntitlementAndBenefitRight`
- `ExamSessionConfiguration_EnforcesOneInProgressSessionPerNurseAndExamVersionAcrossSources`
- `PackageBenefitRightConfiguration_PersistsConsumedAttemptStatus`

The tests must inspect EF metadata and verify required string conversion/max length, unique one-to-one `ExamSessionId`, indexes for entitlement/right lookup, restrict delete behavior, and the existing filtered unique in-progress session index remains present.

- [x] **Step 2: Run infrastructure tests and verify they fail**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj --filter "ExamSessionConfiguration|ExamSessionProvenanceConfiguration|PackageBenefitRightConfiguration_PersistsConsumedAttemptStatus"
```

Expected: fails because mappings and DbSet are not complete.

- [x] **Step 3: Configure EF model**

Add `ExamSessionProvenances` DbSet. Configure `ExamSession.Source` as required string max length 32. Configure `ExamSessionProvenance` as its own table with required columns, one-to-one relationship to `ExamSession`, restrictive foreign keys to package entitlement/right/snapshot/order/order item/package facts where navigations exist, and indexes for `PackagePurchaseEntitlementId`, `PackageBenefitRightId`, and `IncludedExamVersionId`.

- [x] **Step 4: Add unique-constraint detection helpers if required**

If application retry handling needs provider-specific detection, add narrowly named helpers such as `IsInProgressExamSessionUniqueViolation(DbUpdateException exception)` and `IsExamSessionProvenanceUniqueViolation(DbUpdateException exception)`. Do not add generic persistence helpers that expose Infrastructure details to Domain.

- [x] **Step 5: Generate EF migration**

After explicit user approval for migrations, run the project’s existing migration command pattern for Infrastructure. The migration must:

- Add required `Source` to `ExamSessions` using the approved `Legacy` historical backfill strategy: add `Source`, backfill all existing pre-Stage-3 rows to `Legacy`, then make `Source` required. Future inserted rows must use explicit non-`Legacy` source values from application code. Do not classify historical sessions as `Free`, `StandaloneGrant`, or `PackageAttempt` because the authorization branch used at historical creation time cannot be reconstructed deterministically.
- Add `ConsumedAt` nullable timestamp to `PackageBenefitRights`.
- Add `ExamSessionProvenances` with required package facts.
- Add one-to-one unique index on `ExamSessionId`.
- Preserve filtered unique index on `(NurseProfileId, ExamVersionId)` where status is `InProgress`.
- Use restrictive delete behavior.

- [x] **Step 6: Run EF metadata tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj --filter "ExamSessionConfiguration|ExamSessionProvenanceConfiguration|PackageBenefitRightConfiguration_PersistsConsumedAttemptStatus"
```

Expected: all matching infrastructure tests pass.

- [x] **Step 7: Verify migration/model snapshot**

Run the repository’s EF pending-model check command used in prior stages. Expected: no pending model changes after migration generation.

- [x] **Step 8: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 3: Existing free/standalone start compatibility and source recording

**Files:**
- Modify: `backend/src/NursingPlatform.Application/Exams/Common/ExamAccessPolicy.cs`
- Modify: `backend/src/NursingPlatform.Application/Exams/Commands/StartExamSession/StartExamSessionCommandHandler.cs`
- Modify: `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionDto.cs`
- Modify: `backend/src/NursingPlatform.Application/Exams/Common/ExamMapping.cs`
- Test: existing application exam session command tests or new `backend/tests/NursingPlatform.Application.Tests/Exams/StartExamSessionSourceTests.cs`

**Interfaces:**
- Consumes: `ExamSessionSource` from Task 1.
- Produces: existing start handler records `Free` or `StandaloneGrant` source without package-right authorization.
- Produces: safe `ExamSessionDto.Source` string/enum field.

- [x] **Step 1: Write failing compatibility tests**

Create tests named exactly:

- `Handle_StartExamSession_ForFreeExam_RecordsFreeSourceAndDoesNotConsumePackageRight`
- `Handle_StartExamSession_ForStandalonePaidExam_RecordsStandaloneGrantSourceAndDoesNotConsumePackageRight`
- `Handle_StartExamSession_NeverCreatesLegacySource`
- `Handle_StartExamSession_WithPackageEntitlementOnly_StillRequiresStandaloneGrantWhenPaidPolicyRequiresGrant`

The tests must prove the existing endpoint does not consume package rights, package entitlement alone does not satisfy standalone paid exam start, and new existing-endpoint sessions never use `Legacy`.

- [x] **Step 2: Run application compatibility tests and verify they fail**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "Handle_StartExamSession_ForFreeExam|Handle_StartExamSession_ForStandalonePaidExam|Handle_StartExamSession_NeverCreatesLegacySource|Handle_StartExamSession_WithPackageEntitlementOnly"
```

Expected: fails because source recording is not implemented.

- [x] **Step 3: Make access policy return source classification**

Refactor the existing exam access policy minimally so the start handler can distinguish `Free` from `StandaloneGrant` using current paid classification and grant authorization. Do not read or write package entitlements/rights in this policy for the existing endpoint.

- [x] **Step 4: Record source in existing start handler**

Pass `ExamSessionSource.Free` or `ExamSessionSource.StandaloneGrant` into `ExamSession.Create`. Never pass `ExamSessionSource.Legacy` from application start code. If the existing in-progress session source is `PackageAttempt`, return `PackageExamSessionConflictException` with code `exam-session-source-conflict` rather than silently resuming it as free/standalone. Historical `Legacy` sessions remain resumable/reviewable only under existing session ownership/lifecycle rules and do not authorize package behavior.

- [x] **Step 5: Map source in safe DTO**

Add `Source` to `ExamSessionDto` and `ExamMapping.ToSessionDto`. Do not add entitlement id, benefit right id, provenance id, correct answer flags, answer keys, or rationales.

- [x] **Step 6: Run compatibility tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "Handle_StartExamSession_ForFreeExam|Handle_StartExamSession_ForStandalonePaidExam|Handle_StartExamSession_NeverCreatesLegacySource|Handle_StartExamSession_WithPackageEntitlementOnly"
```

Expected: all matching tests pass.

- [x] **Step 7: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 4: Package start application workflow with atomicity, idempotency, and conflicts

**Files:**
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/StartPackageExamSession/StartPackageExamSessionCommand.cs`
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/StartPackageExamSession/StartPackageExamSessionCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/DTOs/PackageExamSessionStartDto.cs`
- Create: `backend/src/NursingPlatform.Application/PreparationPackages/ExamSessions/Exceptions/PackageExamSessionConflictException.cs`
- Modify as needed: `backend/src/NursingPlatform.Application/PreparationPackages/Authorization/PackageBenefitAuthorizationService.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageExamSessions/StartPackageExamSessionHandlerTests.cs`

**Interfaces:**
- Consumes: provenance model, source model, EF DbSet, existing `ExamHandlerHelpers`, existing package entitlement/right models.
- Produces: `StartPackageExamSessionCommand(Guid EntitlementId)`.
- Produces: `PackageExamSessionStartDto` containing `ExamSessionDto Session`, `string Source`, and safe package/entitlement summary fields only.
- Produces: deterministic exceptions with approved stable codes.

- [x] **Step 1: Write failing happy-path application test**

Create `Handle_StartPackageAttempt_WithActiveEntitlementAndAvailableRight_CreatesSessionProvenanceAndConsumesRight`. Assert:

- session source is `PackageAttempt`;
- session uses entitlement `IncludedExamId` and exact `IncludedExamVersionId`, not latest published version;
- one provenance row is created with required package facts;
- right status becomes `Consumed`;
- right `ConsumedAt` is non-null and equals the operation timestamp within existing test clock tolerance;
- question and option snapshots are created;
- response DTO hides internal benefit right id and provenance internals.

- [x] **Step 2: Run happy-path test and verify it fails**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "Handle_StartPackageAttempt_WithActiveEntitlementAndAvailableRight"
```

Expected: fails because command/handler do not exist.

- [x] **Step 3: Implement package command, DTO, and exception shell**

Add the command, success DTO, and conflict exception with stable `Code` property. Do not expose internal right ids in DTOs.

- [x] **Step 4: Implement authorization and happy path inside one transaction**

Handler sequence must be:

1. Resolve current nurse profile.
2. Load entitlement by `entitlementId` and current nurse id; throw `KeyNotFoundException` for missing or non-owned.
3. Validate entitlement `Active`, `AccessStartsAt <= now`, and `now < AccessEndsAt`; otherwise code `package-entitlement-inactive`.
4. Resolve exactly one `PackageExamAttemptEligibility` right; otherwise code `package-attempt-right-missing`.
5. Validate right `Available` and right access window; otherwise reload same-source state if consumed, then return consumed/source conflict as appropriate.
6. Load entitlement included exam and exact included exam version; if unavailable/start-invalid, code `package-exam-version-unavailable`.
7. Check existing in-progress session for nurse/exam version across all sources.
8. If matching same entitlement package session exists and is not expired, return it idempotently without rechecking entitlement window.
9. If different source or different package entitlement exists, code `exam-session-source-conflict` without consuming.
10. Create package `ExamSession`, provenance, question snapshots, option snapshots, and consume right in the same transaction.
11. Save and commit once.

- [x] **Step 5: Write failing negative authorization tests**

Create tests named exactly:

- `Handle_StartPackageAttempt_WithExpiredEntitlement_DeniesWithoutConsumingRight`
- `Handle_StartPackageAttempt_WithForeignEntitlement_DeniesWithoutExposingEntitlementFacts`
- `Handle_StartPackageAttempt_WithMissingAttemptRight_DeniesWithoutCreatingSession`
- `Handle_StartPackageAttempt_UsesEntitlementIncludedExamVersion_NotLatestPublishedVersion`
- `Handle_StartPackageAttempt_DoesNotReadOrCreateExamAccessGrant`

- [x] **Step 6: Implement negative authorization outcomes**

Use approved codes and `404` behavior through exception mapping. Do not query live payment status. Do not satisfy package start with `ExamAccessGrant`.

- [x] **Step 7: Write failing idempotency/conflict/expiry tests**

Create tests named exactly:

- `Handle_StartPackageAttempt_WithConsumedRightAndInProgressMatchingSession_ReturnsSameSession`
- `Handle_StartPackageAttempt_WithConsumedRightAndTerminalSession_ReturnsConsumedOutcome`
- `Handle_StartPackageAttempt_WithDifferentSourceInProgress_ReturnsSourceConflictWithoutConsumingRight`
- `Handle_StartPackageAttempt_WithLegacySessionSource_DoesNotSatisfyPackageStart`
- `Handle_StartPackageAttempt_WithDifferentPackageEntitlementInProgressForSameExamVersion_ReturnsSourceConflict`
- `Handle_StartPackageAttempt_AfterEntitlementExpiryButSessionInProgress_ReturnsExistingSessionWithoutRecheckingWindow`

- [x] **Step 8: Implement same-source retry and deterministic conflicts**

Implement idempotency based on current nurse profile id + entitlement id + included exam version id. `Legacy` must not satisfy package start, must not create package provenance, and must not authorize package behavior. For expired matching in-progress package session, call existing finalization behavior, then throw/return consumed outcome `package-attempt-consumed`; do not create a second session.

- [x] **Step 9: Write failing rollback tests**

Create tests named exactly:

- `Handle_StartPackageAttempt_WhenSessionCreationFails_DoesNotConsumeRight`
- `Handle_StartPackageAttempt_WhenRightConsumptionFails_DoesNotPersistSession`

- [x] **Step 10: Implement rollback-safe transaction behavior**

Use `BeginTransactionAsync`, commit only after session, provenance, snapshots, and right consumption are persisted. Roll back on failure. Do not set consumed state before all validations pass.

- [x] **Step 11: Run package application tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "Handle_StartPackageAttempt"
```

Expected: all package attempt application tests pass.

- [x] **Step 12: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 5: Infrastructure concurrency and rollback integration tests

**Files:**
- Test: `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PackageExamSessionConcurrencyTests.cs`
- Modify if needed: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify if needed: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`

**Interfaces:**
- Consumes: package start handler, EF mappings, unique indexes, transaction behavior.
- Produces: verified optimistic-concurrency/database-uniqueness behavior without PostgreSQL row locking.

- [x] **Step 1: Write failing concurrent same-source test**

Create `PackageAttemptStart_WithConcurrentSameSourceRequests_ConvergesToOneSessionAndOneConsumedRight`. Use real PostgreSQL infrastructure test pattern from existing suite. Assert one session, one provenance row, one consumed right, same session returned/reloaded for same entitlement.

- [x] **Step 2: Write failing concurrent different-source test**

Create `PackageAttemptStart_WithConcurrentDifferentSourceRequests_AllowsOneWinnerAndReturnsDeterministicConflictForLoser`. Assert one in-progress session exists and loser receives `exam-session-source-conflict` after reload.

- [x] **Step 3: Write failing rollback integration test**

Create `PackageAttemptStart_WhenTransactionRollsBack_DoesNotLeaveConsumedRightWithoutSession`. Force a save failure using an existing test fixture pattern or a deliberate unique/provenance violation inside the transaction. Assert no consumed right without session remains.

- [x] **Step 4: Run concurrency tests and verify failure if support code is incomplete**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj --filter "PackageAttemptStart_WithConcurrent|PackageAttemptStart_WhenTransactionRollsBack"
```

- [x] **Step 5: Implement reload-on-conflict handling**

On `DbUpdateConcurrencyException` or provider-specific unique violations for the in-progress session/provenance/right indexes, reload authoritative state and return either same-source session or the approved conflict/consumed code. Do not introduce PostgreSQL row locking.

- [x] **Step 6: Run infrastructure concurrency tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj --filter "PackageAttemptStart_WithConcurrent|PackageAttemptStart_WhenTransactionRollsBack"
```

Expected: all matching tests pass.

- [x] **Step 7: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 6: WebApi route, Problem Details codes, and response security

**Files:**
- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Middleware/ExceptionMiddleware.cs`
- Test: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageExamSessionEndpointTests.cs`
- Test: existing exam endpoint integration test file for compatibility cases

**Interfaces:**
- Consumes: `StartPackageExamSessionCommand`, `PackageExamSessionStartDto`, `PackageExamSessionConflictException`.
- Produces: route `POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session` requiring authentication.
- Produces: Problem Details with stable `code` extension for approved conflict outcomes.

- [x] **Step 1: Write failing auth and success endpoint tests**

Create tests named exactly:

- `StartPackageExamSession_Returns401WithoutJwt`
- `StartPackageExamSession_WithForeignEntitlement_ReturnsNotFoundOrForbiddenWithoutExposure`
- `StartPackageExamSession_WithValidEntitlement_ReturnsSessionAndNoInternalRightId`
- `StartPackageExamSession_WithValidEntitlement_ReturnsSafePackageStartDtoWithSourceAndEntitlementSummary`

The foreign-entitlement test must assert the final approved behavior: `404 Not Found`, indistinguishable from missing entitlement.

- [x] **Step 2: Write failing conflict Problem Details tests**

Create tests named exactly:

- `StartPackageExamSession_WithDifferentSourceInProgress_ReturnsConflictProblemDetails`
- `StartPackageExamSession_WithConsumedTerminalAttempt_ReturnsConsumedProblemDetails`

Assert HTTP 409, `application/problem+json`, and exact `code` values.

- [x] **Step 3: Write failing raw JSON security test**

Create `StartPackageExamSession_RawJsonDoesNotExposeSensitiveFields`. Read raw JSON string before deserializing and assert it does not contain `passwordHash`, `benefitRightId`, `packageBenefitRightId`, `examSessionProvenance`, `correct`, `answerKey`, `rationale`, `providerSecret`, `accessToken`, `refreshToken`, `reportEvidence`, or `internalAuthorizationState` case-insensitively. Also create `StartPackageExamSession_WithLegacySessionSource_DoesNotAuthorizePackageBehavior` proving a historical/backfilled `Legacy` session cannot be treated as package provenance or package authorization.

- [x] **Step 4: Map endpoint**

Add only the package start route. Use `.RequireAuthorization()` and do not add admin permission. Do not add report, workspace, employer, or reset routes.

- [x] **Step 5: Add Problem Details code mapping**

Map `PackageExamSessionConflictException` to 409 with safe detail and stable `code`. Preserve existing handling for validation, not found, forbidden, unauthorized, checkout-in-progress, and invalid operation.

- [x] **Step 6: Run package endpoint tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "StartPackageExamSession"
```

Expected: all package endpoint tests pass.

- [x] **Step 7: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 7: Existing endpoint/API compatibility and scope guard tests

**Files:**
- Test: existing WebApi exam endpoint integration test file
- Test: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageExamSessionEndpointTests.cs`
- Modify only if needed: application/WebApi mapping touched by prior tasks

**Interfaces:**
- Consumes: source-aware DTOs and endpoint mappings from Tasks 3 and 6.
- Produces: compatibility proof that Stage 3 did not silently broaden existing endpoint behavior or expose Stage 4 routes.

- [x] **Step 1: Write compatibility tests**

Create tests named exactly:

- `ExistingStartExamSession_ForFreeExam_RemainsBackwardCompatible`
- `ExistingStartExamSession_ForStandalonePaidExam_RemainsBackwardCompatible`
- `ExistingStartExamSession_DoesNotAcceptOrConsumePackageEntitlement`
- `ExamCatalogAndDetail_IsFreeAndCanStart_DoNotSilentlyIncludePackageRights`
- `Stage3EndpointScope_DoesNotExposeReportWorkspaceEmployerOrAttemptResetRoutes`

The endpoint-scope test must attempt representative forbidden routes and assert they are not mapped by Stage 3.

- [x] **Step 2: Run compatibility tests and verify failure if behavior is incomplete**

Run:

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "ExistingStartExamSession|ExamCatalogAndDetail_IsFreeAndCanStart|Stage3EndpointScope"
```

- [x] **Step 3: Correct only compatibility regressions**

If a compatibility test fails, make the smallest correction to preserve existing semantics. Do not refactor unrelated endpoint groups.

- [x] **Step 4: Run compatibility tests again**

Run:

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "ExistingStartExamSession|ExamCatalogAndDetail_IsFreeAndCanStart|Stage3EndpointScope"
```

Expected: all compatibility tests pass.

- [x] **Step 5: Stop for review**

Do not stage or commit unless the user explicitly authorizes staging/committing for this task.

---

### Task 8: Final verification, documentation check, and review package

**Files:**
- Modify documentation only if implementation changed documented current behavior and the user explicitly authorizes documentation updates.
- Do not modify: `CURRENT_TASK.md`, `TASKS.md`, `.agent/goal-state.md`, frontend/design files, or unrelated backend files unless explicitly authorized.

**Interfaces:**
- Consumes: all prior task deliverables.
- Produces: deterministic evidence for final gate review.

- [x] **Step 1: Run full backend build**

Run:

```bash
dotnet build backend/NursingPlatform.slnx
```

Expected: build succeeds with 0 errors.

- [x] **Step 2: Run domain tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests/NursingPlatform.Domain.Tests.csproj
```

Expected: all tests pass.

- [x] **Step 3: Run application tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj
```

Expected: all tests pass.

- [x] **Step 4: Run infrastructure tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj
```

Expected: all tests pass, including PostgreSQL-backed concurrency/rollback tests where applicable.

- [x] **Step 5: Run WebApi tests**

Run:

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj
```

Expected: all tests pass.

- [x] **Step 6: Run EF pending-model verification**

Run the repository’s established EF pending-model check command. Expected: no pending model changes.

- [x] **Step 7: Inspect diffs for scope**

Run:

```bash
git diff --check
git diff --name-only
git status --short --untracked-files=all
```

Expected: no whitespace errors; only approved Stage 3 files changed; no staged files unless explicitly authorized; no commits unless explicitly authorized.

- [x] **Step 8: Request independent review** — Superseded for Task 8 by explicit user instruction forbidding reviewers/model-based review; deterministic evidence was used instead.

Use `requesting-code-review` and route high-risk review to the Independent Deep Reviewer. Review must focus on authorization, transaction atomicity, retry/idempotency, source switching prevention, sensitive-field exposure, EF constraints, and endpoint scope.

- [x] **Step 9: Correct review findings only after review-feedback workflow** — No reviewer was used by explicit Task 8 instruction, so there were no review findings to correct.

If findings arrive, use `receiving-code-review`, apply only required corrections, rerun affected tests and final verification, and paste corrected full file contents if requested.

- [x] **Step 10: Stop for review**

End with the required stop status. Do not proceed to Stage 4. Do not stage, commit, push, or delete branches unless explicitly authorized.

---

## Required Evidence for Final Implementation Review

- Full list of files changed.
- Full contents of all requested created/modified files.
- Real output from `dotnet build backend/NursingPlatform.slnx`.
- Real output from all applicable `dotnet test` commands.
- Real output from EF pending-model verification.
- Real output from `git diff --check`.
- Real output from `git diff --name-only`.
- Real output from `git status --short --untracked-files=all`.
- Confirmation that no frontend/design files were touched.
- Confirmation that `CURRENT_TASK.md`, `TASKS.md`, and `.agent/goal-state.md` were not modified unless explicitly authorized.
- Confirmation that no files were staged unless explicitly authorized.
- Confirmation that no commit, push, or branch deletion occurred unless explicitly authorized.

## Self-Review Against Approved Specification

- Separate package start endpoint is covered in Tasks 4 and 6.
- One-to-one `ExamSessionProvenance` is covered in Tasks 1 and 2.
- Source values `Legacy`, `Free`, `StandaloneGrant`, and `PackageAttempt` are covered in Tasks 1 and 3; `Legacy` migration backfill and non-creation by application flows are covered in Tasks 2, 3, 4, and 6.
- Stable 409 Problem Details codes are covered in Tasks 4 and 6.
- Additive package-start DTO and safe source exposure are covered in Tasks 3, 4, and 6.
- `404 Not Found` for missing/non-owned entitlement is covered in Tasks 4 and 6.
- Dedicated nullable `ConsumedAt` is covered in Tasks 1 and 2.
- Optimistic concurrency plus database uniqueness/reload is covered in Tasks 2, 4, and 5.
- Expired same-source retry finalization followed by consumed outcome is covered in Task 4.
- Existing free/standalone behavior remains covered in Tasks 3 and 7.
- Atomicity and rollback behavior are covered in Tasks 4 and 5.
- Security raw JSON response checks are covered in Task 6.
- Report generation/access, workspace runtime, employer package data, package attempt reset, frontend/design, branch deletion, and Stage 4 are explicitly excluded in Global Constraints and Task 8.
