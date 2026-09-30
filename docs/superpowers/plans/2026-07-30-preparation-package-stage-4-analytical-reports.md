# Preparation Package Stage 4 — Package Analytical Reports Implementation Plan

> **For agentic workers:** Implementation is not authorized by this draft plan. Do not use reviewers, subagents, model-based review, or DeepSeek for this planning artifact. If implementation is later approved, follow the repository's current orchestration policy and obtain separate explicit approval for implementation, database changes, migrations, staging, committing, pushing, and any next stage.

**Status:** Complete — Stage 4 implemented and verified through Slice 7

**Completion note:** Slice 7 finalization used deterministic evidence only because the authorized Slice 7 prompt explicitly prohibited reviewers, subagents, deep review, and model-based review. No reviewer or subagent was used.

**Goal:** Add backend-only lazy package analytical report generation and direct nurse-owned report access for finalized qualifying package exam sessions.

**Architecture:** Add immutable report snapshot entities in the Preparation Packages domain, persist them with EF Core Code-First, generate them in the Application layer from finalized package exam-session snapshots and exact `ExamSessionProvenance`, and expose one authenticated nurse-owned WebApi endpoint. Existing exam finalization, exam analytics, payment, entitlement, and package exam-session behavior must remain unchanged.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Clean Architecture.

## Global Constraints

- Execute only Stage 4 analytical report generation, recovery, direct access, and deterministic guidance scope.
- Do not implement frontend, design files, wireframes, Angular components, workspace/dashboard runtime, employer report access, admin report access, report list endpoints, retry-management endpoints, exports, AI guidance, performance bands, labels, weak/neutral/strong classifications, or later-stage work.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless a future reviewer explicitly assigns a status-documentation slice.
- Do not delete branches.
- Do not begin implementation, stage, commit, push, create migrations, or modify the database without separate explicit approval for that action.
- Each slice requires separate explicit approval before implementation. Migration creation, staging, committing, and pushing each require their own separate explicit approval and are not authorized by this draft plan.
- Use exam-only report evidence from the exact finalized qualifying package attempt; practice progress must not be report evidence.
- Use the exact `ExamSessionProvenance` row created by Stage 3 as the package/session/provenance source of truth.
- Preserve report access after package entitlement expiry; active materials/practice access is not required for report reads.
- Persist one immutable report snapshot per qualifying package exam session; repeat and concurrent requests must converge to the same report.
- Never store or return question text, answer option text, correct-answer identifiers, answer keys, rationales, protected options, provider/payment secrets, tokens, password hashes, internal authorization state, internal package benefit right ids, EF/domain navigation objects, or stack traces.
- Return counts and percentages only; performance bands and labels are deferred.
- Guidance must be deterministic, purchased-content-only, and derived from study material versions plus practice collection version references tied to the exact package provenance/entitlement.
- Existing standalone free/paid exam behavior, existing exam submit/result/review behavior, existing mutable exam analytics, existing payment behavior, existing entitlement behavior, and existing package exam-session behavior must remain backward compatible.

---

## Scope Summary

Stage 4 builds backend persistence, Application generation/access logic, and one WebApi direct report endpoint:

```text
GET /api/v1/me/nurse-profile/preparation-packages/exam-sessions/{sessionId}/report
```

The endpoint requires authentication and resolves current nurse ownership in Application. It returns an existing immutable report or lazily generates one from a qualifying finalized `PackageAttempt` session. Missing/non-owned sessions use hidden not-found behavior. Non-qualifying or not-finalized sessions use stable conflict codes.

## Non-Scope

- Report list endpoint.
- Direct employer/admin/support/workspace/dashboard access.
- Frontend or design changes under `frontend/`, `docs/frontend/`, or `docs/design/`.
- AI/model-based recommendation, adaptive ranking, predictive guidance, or external recommendation services.
- Practice runtime/progress, practice scoring, answer history, or correctness counters as report evidence.
- Report regeneration after success, manual reset, failure table, operational retry endpoint, export/download/print formats.
- Changes to live payment-provider behavior, webhook behavior, refunds, subscriptions, carts, coupons, tax, invoices, wallets, payouts, or sponsored packages.

## Mandatory Stop Conditions

- Stop if practice collection version/topic mapping needed for guidance is not represented in the current Stage 1 data model.
- Stop if existing answer/session snapshots do not support topic-level counts without reading protected question text, answer option text, correct answers, answer keys, rationales, or other protected content.
- Stop if the direct endpoint cannot hide missing and non-owned sessions consistently with existing project patterns.
- Stop if the migration design would require broad payment/order/catalog foreign-key coupling instead of minimal hard relationships plus copied immutable scalar snapshot fields.

## Stage 1–3 Dependencies

- Stage 1 provides reporting topics, reporting profile publications, reporting-profile question assignments, study material versions/topics, practice collection versions/items, package definitions, versions, and offers.
- Stage 2 provides package purchase entitlements, purchased package facts, ordered purchased study material version ids, report eligibility benefit rights, package order item snapshots, and post-expiry report-right survivability.
- Stage 3 provides `ExamSession.Source == PackageAttempt`, atomic package attempt right consumption, exact `ExamSessionProvenance`, and package exam-session start behavior.
- Stage 4 must not reinterpret these facts from live catalog, live payment status, standalone exam grants, or practice progress.

## Proposed File Structure

### Domain

- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageAnalyticalReport.cs` — immutable report aggregate root and child collection ownership.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageAnalyticalReportTopicResult.cs` — immutable topic-level score snapshot.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageAnalyticalReportGuidanceItem.cs` — immutable deterministic guidance snapshot.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageReportGuidanceSourceType.cs` — enum with `StudyMaterialVersion` and `PracticeCollectionVersion`.
- Prefer leaving `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRight.cs` unchanged and using unique report-table existence as the v1 report linkage. Do not mutate `PackageBenefitRight` / report eligibility unless a separate explicit implementation decision approves it.

### Application

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` only when the Application behavior/model is stable enough to require report persistence abstractions, such as report DbSets and a unique-session violation helper used for idempotent recovery.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/DTOs/PackageAnalyticalReportDto.cs` — safe public report DTO.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/DTOs/PackageAnalyticalReportTopicResultDto.cs` — safe topic DTO.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/DTOs/PackageAnalyticalReportGuidanceItemDto.cs` — safe guidance DTO.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/GetPackageAnalyticalReport/GetPackageAnalyticalReportQuery.cs` — direct report query by session id.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/GetPackageAnalyticalReport/GetPackageAnalyticalReportQueryValidator.cs` — reject empty session id.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/GetPackageAnalyticalReport/GetPackageAnalyticalReportQueryHandler.cs` — ownership, qualification, existing-report load, lazy generation, and retry orchestration.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/Generation/PackageAnalyticalReportGenerator.cs` — pure Application generation service that computes topic results and guidance from persisted snapshots.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/Generation/PackageReportConflictException.cs` — stable conflict code exception if no existing package-report exception pattern exists.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/Mapping/PackageAnalyticalReportMapping.cs` — explicit safe DTO projection.

### Infrastructure

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs` — expose report DbSets and unique violation helper only after Application behavior and model shape are stable.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PackageAnalyticalReportConfiguration.cs` — table, keys, required properties, unique report-per-session index, child ordering, string enum conversion, and restrictive deletes for the minimal hard relationships.
- Create EF migration only after explicit migration approval and only after Application behavior/model are clarified, expected name: `AddPreparationPackageStage4AnalyticalReports`.
- Use minimal required hard relationships only, such as `ExamSessionId`, `ExamSessionProvenanceId`, and where needed `NurseProfileId` / `PackagePurchaseEntitlementId` for ownership and query integrity.
- Store purchased package/order/catalog/payment facts as copied immutable scalar snapshot fields when needed for report history; do not require broad foreign keys from reports to payment order, payment order item, package order item snapshot, package offer, package definition/version, included exam, included exam version, or other live catalog/payment/order tables.
- Copied payment/order/provenance/internal ids are internal persisted snapshot facts only and must not be exposed in nurse-facing DTOs or Problem Details unless separately approved.

### WebApi

- Modify `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` — add only the direct nurse report endpoint in the existing preparation package group and metadata helper updates if needed.
- Modify existing WebApi exception mapping only if the new conflict exception is not already covered by current middleware patterns.

### Tests

- Create/modify domain tests under `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/`.
- Create/modify Application tests under `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/Reports/`.
- Modify `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageDtoSecurityTests.cs` to include report DTOs.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs` to include report entities, indexes, enum conversion, minimal hard relationships, scalar snapshot columns, and delete behavior.
- Create PostgreSQL concurrency/persistence tests under `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/` after persistence is approved if existing test fixture supports package exam-session PostgreSQL tests.
- Modify/create WebApi tests under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageAnalyticalReportEndpointTests.cs`.

---

## Slice 1 — Domain Report Snapshot Model

**Objective:** Add immutable package analytical report domain entities and domain tests without persistence, Application, or WebApi behavior.

**Allowed files:**

- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageAnalyticalReport.cs`
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageAnalyticalReportTopicResult.cs`
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageAnalyticalReportGuidanceItem.cs`
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageReportGuidanceSourceType.cs`
- Create `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PackageAnalyticalReportDomainTests.cs`

**Forbidden files:**

- `backend/src/NursingPlatform.Application/**`
- `backend/src/NursingPlatform.Infrastructure/**`
- `backend/src/NursingPlatform.WebApi/**`
- `frontend/**`
- `docs/frontend/**`
- `docs/design/**`
- `CURRENT_TASK.md`
- `TASKS.md`

**Implementation notes:**

- `PackageAnalyticalReport` should be created through a factory such as `Create(...)` that requires all ids and UTC generated/finalized timestamps.
- It should expose read-only child collections and only domain methods needed at creation time, such as `AddTopicResult(...)` and `AddGuidanceItem(...)`, with duplicate/sort-order validation.
- Do not include navigation properties intended for public DTO projection.
- Do not include any question text, answer option text, correct answer ids, answer keys, rationales, provider secrets, tokens, password hashes, or internal benefit right ids.
- Prefer linkage to `PackageBenefitRight` only through provenance/entitlement/report existence, not by storing the right id on the report.

**Expected tests:**

- Report creation captures required nurse/session/provenance/package/session-score facts.
- Empty required ids are rejected.
- Non-UTC generated timestamp is rejected.
- Topic results store counts, points, and percentages without labels/bands.
- Guidance items support exactly `StudyMaterialVersion` and `PracticeCollectionVersion` references.
- Public properties do not include forbidden sensitive names.
- Child collections are read-only after creation.

**Validation commands:**

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests/NursingPlatform.Domain.Tests.csproj --filter "PackageAnalyticalReportDomainTests"
dotnet build backend/NursingPlatform.slnx
git status --short
```

**Commit boundary:** This slice does not authorize staging or committing. If a future reviewer explicitly approves a commit, stage only intended files by explicit path and commit only this logical slice.

---

## Slice 2 — Application DTO/Security Tests and Behavior Tests

**Objective:** Define the safe public DTO shape and behavior tests before persistence/migration design is finalized.

**Allowed files:**

- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/DTOs/PackageAnalyticalReportDto.cs`
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/DTOs/PackageAnalyticalReportTopicResultDto.cs`
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/DTOs/PackageAnalyticalReportGuidanceItemDto.cs`
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/GetPackageAnalyticalReport/GetPackageAnalyticalReportQuery.cs`
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/GetPackageAnalyticalReport/GetPackageAnalyticalReportQueryValidator.cs`
- Create behavior tests under `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/Reports/`
- Modify `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageDtoSecurityTests.cs`

**Forbidden files:**

- WebApi endpoint files.
- Infrastructure runtime, EF configuration, and migration files.
- Frontend/design files.

**Implementation notes:**

- DTOs must expose safe report, topic-result, and guidance data only.
- Public DTOs and Problem Details must not include `PackageBenefitRightId`, payment/order provider internals, raw provenance internals, copied payment/order scalar ids, question text, option text, correct answers, answer keys, rationales, tokens, password hashes, or stack traces unless separately approved.
- Behavior tests should define lazy generation, direct access, qualification, idempotency, post-expiry access, and deterministic guidance expectations before EF migration is introduced.
- Tests may use existing in-memory/mocked context patterns; do not create migration files in this slice.

**Expected tests:**

- DTO security test rejects forbidden public property names and sensitive terms.
- Query validator rejects an empty session id.
- Behavior tests specify submitted and expired finalized package-session generation.
- Behavior tests specify in-progress, abandoned, non-package, missing/non-owned, missing-provenance, inconsistent-provenance, missing-right, repeated-request, cross-package, post-expiry, and standalone-session rejection/access rules.
- Behavior tests specify exam-only topic calculations and purchased-content guidance without practice progress.

**Validation commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "PackageAnalyticalReport"
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "PreparationPackageDtoSecurityTests"
dotnet build backend/NursingPlatform.slnx
git status --short
```

**Commit boundary:** This slice does not authorize staging or committing. If a future reviewer explicitly approves a commit, stage only intended files by explicit path and commit only this logical slice.

---

## Slice 3 — Application Generation and Direct Access Implementation

**Objective:** Implement generation/query behavior against the stable Domain and DTO/test contract before creating EF migration files.

**Allowed files:**

- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/Generation/PackageAnalyticalReportGenerator.cs`
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/Mapping/PackageAnalyticalReportMapping.cs`
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/Generation/PackageReportConflictException.cs` if no existing exception pattern covers stable conflict codes.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Reports/GetPackageAnalyticalReport/GetPackageAnalyticalReportQueryHandler.cs`
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` only as needed to express report persistence abstractions used by Application tests.
- Modify Application report tests under `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/Reports/`

**Forbidden files:**

- WebApi endpoint files.
- EF migration files.
- Frontend/design files.
- Any exam finalization handler change that makes finalization call report generation.

**Implementation notes:**

- Get current nurse profile using the existing `NurseRoleGuard` and `ExamHandlerHelpers.GetCurrentNurseProfileIdAsync` pattern or equivalent existing nurse-owned resource pattern.
- Missing/non-owned session returns hidden not-found behavior.
- Qualify only `PackageAttempt` sessions in `Submitted` or `Expired` status.
- Reject `InProgress` with `package-report-session-not-finalized`.
- Reject `Legacy`, `Free`, `StandaloneGrant`, and `Abandoned` with `package-report-session-not-qualified`.
- Require exactly one provenance for the session and verify included exam/version/reporting profile/entitlement facts match.
- Verify the referenced package entitlement belongs to the same nurse.
- Verify report eligibility right exists and is compatible with the provenance; do not expose its id.
- Prefer unique report table existence as the v1 report linkage. Do not mutate `PackageBenefitRight` or report eligibility unless a separate explicit implementation decision approves it.
- If report exists, map and return it without regenerating.
- If report is absent, generate report and children in one transaction.
- If unique constraint loses a concurrent race, reload and return the existing report when facts match.
- Failed generation must not consume report eligibility permanently, invalidate scoring, or require retake.
- Report access/generation remains allowed after entitlement expiry when persisted finalized session/provenance qualifies.
- Load finalized `ExamSession`, `ExamSessionQuestion`, `ExamSessionAnswerOption`, and `ExamSessionAnswer` snapshot data only.
- Use `ReportingProfileQuestionAssignment` rows from `ExamSessionProvenance.ReportingProfilePublicationId` and match by `ExamSessionQuestion.ExamQuestionId`.
- Calculate topic earned/correct/available values from session snapshots and selected answers, not live answer-key authoring content.
- Use finalized overall fields from `ExamSession`: `Score`, `MaxScore`, `Percentage`, `Passed`, `CorrectCount`, `QuestionCount`.
- Use decimal percentage rounding consistent with existing `ExamScoringService` precision.
- Do not mutate exam-session scoring fields.
- Do not query practice progress tables, answer histories, or correctness counters.
- Study material guidance must use `PackagePurchaseEntitlement.StudyMaterialVersionIds` plus `StudyMaterialVersionTopic` mappings that match report topics.
- Practice guidance must use `ExamSessionProvenance.PracticeCollectionVersionId` and safe practice collection version references for topics represented by practice items in the purchased practice collection version.
- If practice collection version references cannot be safely mapped to report topics without using practice progress or protected answer content, Stage 4 must stop for clarification rather than inventing guidance behavior.
- Do not call AI/model services.

**Expected tests:**

- Submitted package session lazily generates and returns a report.
- Expired finalized package session lazily generates and returns a report.
- Repeated request returns the same report id and child ids.
- In-progress session returns exact conflict code.
- Abandoned and non-package sessions return exact conflict code.
- Missing/non-owned session returns hidden not found.
- Missing/inconsistent provenance returns exact conflict code.
- Missing/incompatible report right returns exact conflict code.
- One package entitlement cannot qualify another package's session.
- Standalone sessions by nurses who own packages do not qualify.
- Existing report remains readable/generatable after entitlement expiry.
- Simulated unique-constraint race reloads existing report.
- Existing exam finalization tests still pass and are not coupled to report generation.
- Includes purchased study material versions whose topic mappings match report topics.
- Includes purchased practice collection version references for matching represented topics, or stops for clarification if the mapping cannot be established safely.
- Excludes materials not in the purchased package and practice collections not referenced by the exact provenance.
- Orders guidance deterministically across repeated generation.
- Does not include question text, option text, correct answers, rationales, answer keys, or practice answer keys.

**Validation commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "PackageAnalyticalReport"
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "StartPackageExamSessionHandlerTests"
dotnet build backend/NursingPlatform.slnx
git status --short
```

**Commit boundary:** This slice does not authorize staging or committing. If a future reviewer explicitly approves a commit, stage only intended files by explicit path and commit only this logical slice.

---

## Slice 4 — Persistence Configuration and EF Migration

**Objective:** Persist the already-clarified report model with unique-session idempotency protection, minimal hard relationships, immutable scalar snapshots, restrictive delete behavior, and one EF Core migration after separate migration approval.

**Allowed files:**

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` only for final DbSet/unique-violation abstractions needed by the stable Application implementation.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PackageAnalyticalReportConfiguration.cs`
- Create migration files under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` only after explicit migration approval.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs`

**Forbidden files:**

- WebApi endpoint files.
- Frontend/design docs and runtime files.
- `CURRENT_TASK.md`, `TASKS.md`.

**Implementation notes:**

- Configure `PackageAnalyticalReports` with primary key `Id`.
- Configure unique index on `ExamSessionId`.
- Configure query-supporting index on `NurseProfileId, PackagePurchaseEntitlementId, ExamSessionId` if needed for current-nurse direct access.
- Use minimal hard relationships required for report ownership and idempotency, such as `ExamSessionId`, `ExamSessionProvenanceId`, and where needed `NurseProfileId` / `PackagePurchaseEntitlementId`.
- Do not require broad foreign keys from reports to payment order, payment order item, package order item snapshot, package offer, package definition/version, included exam, included exam version, or other live catalog/payment/order tables.
- Copy purchased package/order/catalog/payment facts into immutable scalar snapshot fields when needed for historical meaning. These copied facts are internal persisted snapshot facts only.
- Copied payment/order/provenance/internal ids must not be exposed in nurse-facing DTOs or Problem Details unless separately approved.
- Configure child tables `PackageAnalyticalReportTopicResults` and `PackageAnalyticalReportGuidanceItems` as immutable children with deterministic sort ordering.
- Use restrictive delete behavior for minimal hard report relationships; if cascade delete or broad payment/order/catalog FK coupling is considered for any report relationship, stop for review because the spec requires immutable historical report snapshots and forbids cascade delete for report records.
- Store `PackageReportGuidanceSourceType` as string with max length 32.
- Store percentages with the same precision used for exam percentages unless implementation confirms a narrower existing convention.
- Add provider-specific unique violation helper for the report `ExamSessionId` unique index.
- Migration must be generated by EF Core Code-First after separate explicit migration approval; never hand-edit schema outside migration files.

**Expected tests:**

- EF model contains all three report entities and expected table names.
- Unique index exists on `PackageAnalyticalReport.ExamSessionId`.
- Minimal required hard relationships use `DeleteBehavior.Restrict`.
- Broad payment/order/catalog tables are not required as hard report relationships.
- Internal copied scalar snapshot fields exist only where needed and are not part of DTO projections.
- Guidance enum stored as string with max length 32.
- `GeneratedAt` is required.
- Child guidance ordering columns are required and indexed as needed.
- EF migration class name includes `AddPreparationPackageStage4AnalyticalReports` after migration approval.

**Validation commands:**

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj --filter "PreparationPackageConfigurationTests"
dotnet ef migrations script --idempotent --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext --output /tmp/opencode/stage4-package-report-idempotent.sql
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
dotnet build backend/NursingPlatform.slnx
git status --short
```

**Commit boundary:** This slice does not authorize database changes, migrations, staging, or committing. Migration generation, staging, and committing each require separate explicit approval.

---

## Slice 5 — WebApi Direct Report Endpoint

**Objective:** Expose only the approved authenticated direct report endpoint and map it to the Application query.

**Allowed files:**

- Modify `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs`
- Modify existing exception/problem-details mapping only if necessary for `PackageReportConflictException`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageAnalyticalReportEndpointTests.cs`

**Forbidden files:**

- Any report list endpoint.
- Employer, admin, support, workspace/dashboard, retry-management, export, guidance-only, or report-id discovery routes unless separately approved.
- Frontend/design files.

**Implementation notes:**

- Add:

```text
GET /api/v1/me/nurse-profile/preparation-packages/exam-sessions/{sessionId:guid}/report
```

- Use `ISender` and send `GetPackageAnalyticalReportQuery`.
- Require `.RequireAuthorization()` exactly; do not add permission requirements.
- Add metadata in the existing preparation package endpoint style with 200, 400 validation, 401, 404, and 409 responses.
- Keep endpoint group organization under nurse-owned preparation package endpoints.
- Do not expose report list route or direct report-id route unless explicitly approved later.

**Expected tests:**

- `401 Unauthorized` without JWT.
- Authenticated valid finalized package session returns `200 OK` with safe DTO.
- Repeated direct request returns same report id.
- Cross-nurse request returns hidden `404` and does not leak package facts.
- In-progress package session returns `409` with exact code `package-report-session-not-finalized`.
- Non-package session returns `409` with exact code `package-report-session-not-qualified`.
- Raw JSON response inspection verifies forbidden terms are absent before deserialization.
- Report remains accessible after entitlement expiry.
- Endpoint scope test proves no list, employer, workspace/dashboard, admin, retry-management, AI guidance, or frontend/design route exists.

**Validation commands:**

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "PackageAnalyticalReportEndpointTests"
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "PackageExamSessionEndpointTests"
dotnet build backend/NursingPlatform.slnx
git status --short
```

**Commit boundary:** This slice does not authorize staging or committing. If a future reviewer explicitly approves a commit, stage only intended files by explicit path and commit only this logical slice.

---

## Slice 6 — PostgreSQL Concurrency, Integration, Security, and Compatibility Verification

**Objective:** Prove high-risk recovery, idempotency, concurrency, security, and compatibility invariants across layers.

**Allowed files:**

- Create/modify tests in `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/Reports/`.
- Create/modify tests in `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/`.
- Create/modify tests in `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageAnalyticalReportEndpointTests.cs`.
- Modify security test files that list DTOs or forbidden JSON fields.

**Forbidden files:**

- Runtime behavior changes unless a test exposes a real defect in the already-approved Stage 4 implementation; fix only the relevant defect.
- Frontend/design files.
- `CURRENT_TASK.md`, `TASKS.md`.

**Implementation notes:**

- Application-level recovery means a finalized qualifying package session without a report follows the same direct request generation path.
- PostgreSQL concurrency test should run two first requests/generation attempts and assert one report, one child set, and same returned report.
- Integration/security tests must verify copied payment/order/provenance/internal scalar snapshot fields are not exposed in nurse-facing DTOs or Problem Details.
- Finalization independence test must verify submit/finalize works without report generation and report generation failure does not roll back session scoring.
- Raw JSON security checks must use response body strings and `Assert.DoesNotContain(..., StringComparison.OrdinalIgnoreCase)` before DTO deserialization.

**Expected tests:**

- Recovery from finalized package session without report.
- Concurrent first requests converge to one persisted report.
- Failed generation attempt leaves report right unconsumed or otherwise retryable according to the chosen representation.
- Existing free/standalone exam start and submit behavior unchanged.
- Existing exam analytics remain aggregate analytics and do not use package report snapshot as a replacement.
- Existing payment/entitlement/package exam-session endpoint tests pass.
- Raw JSON security checks reject forbidden fields.

**Validation commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj --filter "PackageAnalyticalReport"
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj --filter "PackageAnalyticalReport"
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "PackageAnalyticalReportEndpointTests"
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj --filter "ExamEndpointsTests|ExamAnalyticsEndpointsTests|PackageExamSessionEndpointTests|PreparationPackage"
dotnet build backend/NursingPlatform.slnx
git status --short
```

**Commit boundary:** This slice does not authorize staging or committing. If a future reviewer explicitly approves a commit, stage only intended files by explicit path and commit only this logical slice.

---

## Slice 7 — Final Documentation/Status Update Review

**Objective:** After implementation is approved and complete, perform final deterministic verification and make only explicitly approved documentation/status updates.

**Allowed files:**

- Documentation files only if the implementation changed behavior and the reviewer explicitly requests updates.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.

**Forbidden files:**

- Any new runtime changes except minimal fixes required by failed verification.
- Frontend/design files unless explicitly instructed; Stage 4 currently forbids frontend/design implementation.
- Branch deletion.

**Required verification commands:**

```bash
dotnet build backend/NursingPlatform.slnx
dotnet test backend/tests/NursingPlatform.Domain.Tests/NursingPlatform.Domain.Tests.csproj
dotnet test backend/tests/NursingPlatform.Application.Tests/NursingPlatform.Application.Tests.csproj
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests/NursingPlatform.Infrastructure.Tests.csproj
dotnet test backend/tests/NursingPlatform.WebApi.Tests/NursingPlatform.WebApi.Tests.csproj
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
dotnet ef migrations script --idempotent --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext --output /tmp/opencode/stage4-package-report-idempotent.sql
git diff --name-only
git diff --cached --name-only
git status --short
```

**Security/privacy grep checks:**

Run repository searches against Stage 4 report DTOs, mappings, WebApi tests, and migrations for forbidden exposure. Investigate any hit rather than assuming it is safe.

```bash
rg -n "PasswordHash|passwordHash|RefreshToken|accessToken|clientSecret|providerSecret|PaymentProvider|QuestionText|OptionText|IsCorrect|CorrectAnswer|AnswerKey|Rationale|Explanation|PackageBenefitRightId" backend/src backend/tests
rg -n "PackageAnalyticalReport|package analytical report|package-report" backend/src backend/tests
rg -n "Map(Get|Post|Put|Delete).*report|/report|reports" backend/src/NursingPlatform.WebApi
```

Expected outcomes:

- Forbidden terms may appear in existing tests or negative security assertions, but must not appear as report DTO response fields, report public mapping output, Problem Details extensions, or migration columns that violate the approved schema.
- Only the approved direct route is added; no report list, employer, admin, workspace/dashboard, retry-management, or guidance-only route exists.

**Protected diff checks:**

```bash
git diff --name-only -- docs/frontend docs/design frontend CURRENT_TASK.md TASKS.md
git diff --cached --name-only
git status --short
```

Expected outcomes:

- No Stage 4 implementation changes under `frontend/**`, `docs/frontend/**`, or `docs/design/**`.
- No modifications to `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.
- No staged files unless staging was explicitly approved.
- No commit unless committing was explicitly approved.
- Known unrelated frontend/design worktree changes must remain untouched.

**Rollback requirements:**

- Runtime rollback is a single logical revert of Stage 4 commits if committed later with approval.
- Database rollback must be represented by EF migration `Down` method generated/reviewed with the migration; never manually edit production schema.
- If a report generation attempt fails before commit, no partial report or child rows should remain.
- If concurrent generation loses the unique report race, the loser must reload and return the winner rather than creating duplicates or surfacing a raw database error.

**Idempotency requirements:**

- One unique report per `ExamSessionId` at the database level.
- Query path checks for existing report before generation.
- Creation path catches the expected unique-session violation and reloads the report.
- Repeated requests return the same immutable report.

**Concurrency requirements:**

- Create report and topic/guidance children in one transaction.
- Database uniqueness on `ExamSessionId` protects first-request races.
- Do not lock or mutate exam session scoring fields for report generation.
- Do not mutate or consume report eligibility in v1 unless a separate approved implementation decision explicitly authorizes it; failed generation must remain retryable.

**Documentation/status update rule:**

- Update architecture/API/database docs only if explicitly requested by the reviewer after implementation.
- Do not update `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.

**Commit boundary:** Final verification and documentation review are not commits by default. If a future reviewer explicitly approves committing, inspect `git status`, `git diff`, and `git log --oneline -10`, stage only intended files by explicit path, and use one logical commit message consistent with repository history. Never use `git add .`.

---

## Final Acceptance Checklist

- [ ] Exactly one immutable report exists per qualifying package exam session.
- [ ] Lazy direct request creates missing report and returns existing report thereafter.
- [ ] Concurrent first requests converge to one report.
- [ ] Report generation uses exact `ExamSessionProvenance` and exact finalized `PackageAttempt` session only.
- [ ] Practice progress is not read or used as report evidence.
- [ ] Guidance uses only purchased study material versions and purchased practice collection version references.
- [ ] No protected exam content, answer keys, rationales, secrets, tokens, password hashes, internal right ids, or navigation objects are exposed.
- [ ] Endpoint requires authentication and no permission requirement.
- [ ] Cross-nurse access is hidden as not found.
- [ ] Reports remain accessible/generatable after entitlement expiry when persisted finalized session/provenance qualifies.
- [ ] Existing exam, analytics, payment, entitlement, and package exam-session behavior remain compatible.
- [ ] EF migration has no pending model changes after generation.
- [ ] No frontend/design files were touched.
- [ ] No branch deletion, staging, committing, pushing, or next-stage work occurred without explicit approval.
