# Preparation Package Stage 4 — Package Analytical Report Generation, Recovery, Access, and Guidance

## Status

Approved — Stage 4 Specification

This specification is documentation only. It does not authorize implementation, implementation planning, source-code changes outside this file, database migrations, API implementation, tests, frontend or design work, staging, committing, pushing, deleting branches, or beginning any later stage.

---

## Problem Statement

Preparation Package Stages 1 through 3 created the backend foundation for package catalog composition, package purchase entitlements, dormant report eligibility rights, package-scoped exam attempt start, atomic attempt consumption, session source classification, and immutable package session provenance.

Stage 4 must define how a nurse receives the package analytical report promised by the purchased package after completing the package exam attempt. The report must be generated from the qualifying package exam session only, stored as an immutable nurse-owned snapshot, recoverable if first generation fails, readable after package expiry, and safe to expose through direct nurse-owned API access without leaking protected exam, payment, authorization, or identity data.

Existing exam analytics are mutable aggregate analytics over exam sessions. They are not package reports and must not be reused as the persistent package report snapshot.

---

## Goals

- Generate exactly one immutable analytical report snapshot per qualifying package exam session.
- Use lazy/on-demand generation in v1: the first valid report request after exam finalization generates the report if it does not already exist.
- Support idempotent recovery for finalized qualifying package sessions that do not yet have reports.
- Keep exam finalization independent from report generation failure.
- Use only exam-session evidence from the exact qualifying package attempt.
- Use the exact `ExamSessionProvenance` created by Stage 3 as the package/session/provenance source of truth.
- Return counts and percentages only; performance bands and labels are deferred.
- Include deterministic purchased-content guidance from both included study material versions and included practice collection version references.
- Keep practice progress excluded from report evidence.
- Keep reports nurse-owned and directly accessible after package expiry.
- Preserve existing standalone free/paid exam behavior, analytics behavior, payment behavior, package entitlement behavior, and package exam-session behavior.

---

## Non-Goals

Stage 4 v1 does not include:

- Report list endpoints.
- Frontend implementation, design files, wireframes, Angular components, or package report UI.
- Workspace/dashboard runtime.
- Employer report visibility or employer package data.
- Admin report access, admin report controls, support reset operations, or manual report regeneration tools.
- AI-generated, predictive, adaptive, or non-deterministic guidance.
- Performance bands, labels, weak/neutral/strong classifications, or launch-time classification thresholds.
- Practice-progress evidence in report scoring or topic percentages.
- Downloadable, printable, or export report formats beyond API JSON responses.
- Multiple package attempts per entitlement.
- Report sharing controls.
- Operational failure table for failed report generation.
- Production payment-provider, webhook, refund, reconciliation, subscription, cart, coupon, tax, invoice, wallet, payout, or sponsored-package behavior.

---

## Approved Decisions

1. Report generation trigger:
   - Stage 4 v1 uses lazy/on-demand generation.
   - The first valid direct report request after exam finalization generates the report if no report exists.
   - Idempotent recovery handles finalized qualifying package sessions without reports.
   - Exam finalization must not fail because report generation fails.
2. Report access shape:
   - Stage 4 v1 does not include a report list endpoint.
   - Stage 4 v1 uses direct report access only.
3. Performance output:
   - Performance bands and labels are deferred.
   - Reports store and return counts and percentages only.
4. Guidance:
   - Guidance is deterministic and restricted to purchased content.
   - Guidance includes study material versions and practice collection version references from the purchased package version.
   - Practice progress is not report evidence.
5. Failed generation tracking:
   - Stage 4 v1 does not add an operational failure table.
   - Recovery uses idempotent detection of finalized qualifying package sessions without reports.

---

## Authoritative Inputs and Existing Baseline

Stage 4 depends on the approved umbrella and Stage 1–3 preparation package specifications and completed Stage 3 implementation on branch `feature/preparation-package-foundation` at expected HEAD `798364e docs: mark preparation package stage 3 complete`.

Relevant existing baseline facts:

- `ExamScoringService` finalizes sessions from `ExamSessionQuestion`, `ExamSessionAnswerOption.IsCorrectSnapshot`, and submitted answers, then stores aggregate score fields on `ExamSession`.
- `ExamHandlerHelpers.FinalizeAsync` and `FinalizeIfExpiredAsync` perform existing finalization through `ExecuteExamSessionFinalizationAsync`.
- `SubmitExamSessionCommandHandler` finalizes submitted or expired sessions and returns the existing result DTO.
- Exam analytics load mutable session rows and compute aggregate metrics; analytics are not immutable package reports.
- `PackagePurchaseEntitlement` stores nurse ownership, purchased package/version/offer facts, included exam/version, reporting profile publication, practice collection version, and ordered study material version ids.
- `PackageBenefitRight` includes `ReportEligibility`, initially `Dormant`, and `ConsumedAt` currently supports package exam attempt consumption.
- `ExamSessionProvenance` stores the exact package purchase, benefit right, purchased snapshot, payment, package definition/version/offer, included exam/version, reporting-profile publication, practice collection version, and package access-window facts for package-attempt sessions.
- `StartPackageExamSessionCommandHandler` creates `PackageAttempt` sessions with provenance and consumes the package exam attempt right atomically.
- Current nurse-owned package endpoints live under `/api/v1/me/nurse-profile/preparation-packages/entitlements` in `PreparationPackageEndpointExtensions` and require authentication.

---

## Domain Model Proposal

Stage 4 should add report snapshot entities under the Preparation Packages domain area. Concrete implementation names remain subject to the later implementation plan, but the conceptual model is:

### Package Analytical Report

The report aggregate root represents one immutable generated report for one qualifying package exam session.

Required conceptual facts:

- report id;
- owning nurse profile id;
- exam session id;
- exam session provenance id;
- package purchase entitlement id;
- package order item snapshot id;
- payment order id and payment order item id;
- preparation package definition id;
- preparation package version id;
- preparation package offer id;
- included exam id;
- included exam version id;
- reporting profile publication id;
- practice collection version id;
- generated-at UTC timestamp;
- finalized session status, submitted/finalized timestamps, score, max score, percentage, correct count, and question count;
- package access-window start/end snapshot copied from provenance;
- immutable child topic results;
- immutable child guidance items.

The report root must not store question text, answer option text, correct-answer identifiers, answer keys, rationales, protected options, provider secrets, tokens, password hashes, or internal authorization state.

### Package Analytical Report Topic Result

Each topic result stores the immutable topic-level score snapshot for one reporting topic included in the reporting profile evidence.

Required conceptual facts:

- report id;
- reporting topic id;
- safe topic name/display snapshot;
- scored question count for the topic;
- correct answer count for the topic;
- earned points for the topic;
- available points for the topic;
- percentage for the topic.

Stage 4 v1 must not store or return performance bands or labels.

### Package Analytical Report Guidance Item

Each guidance item stores deterministic purchased-content guidance for one topic.

Required conceptual facts:

- report id;
- reporting topic id;
- guidance source type equivalent to `StudyMaterialVersion` or `PracticeCollectionVersion`;
- study material version id when the source is study material;
- practice collection version id when the source is practice collection;
- safe title/display snapshot;
- material type or practice reference type as safe metadata;
- deterministic sort order.

Guidance items must not store practice performance, exam question content, exam answer options, correct answers, or practice answer keys.

### Report Right Lifecycle

The Stage 2 `ReportEligibility` benefit right remains the right associated with report generation/access. Stage 4 may add report linkage and generated timestamp fields to the right or track linkage from the report table, but clients must never select or receive internal right ids.

If Stage 4 updates right status, the only normal successful transition approved by this specification is from dormant report eligibility to a generated/consumed report state after the report is persisted. Failed generation must not permanently consume the right.

---

## Application Behavior

Stage 4 should add Application-layer commands/queries/services that follow existing CQRS and `IApplicationDbContext` patterns without WebApi or Infrastructure dependencies.

### Direct Report Request

The primary v1 behavior is direct access by package exam session id:

```text
current authenticated nurse
    -> package exam session id
        -> verify session ownership
        -> verify source is PackageAttempt
        -> verify status is Submitted or Expired
        -> load exact ExamSessionProvenance
        -> verify provenance is for the same session
        -> verify selected package entitlement/right belongs to the same nurse/package provenance
        -> return existing report or generate one idempotently
        -> return safe report DTO
```

Direct access by report id may also be added if the implementation plan needs it for retrieval after generation, but Stage 4 v1 must not include list endpoints.

### Qualifying Sessions

Only sessions meeting all of the following qualify:

- `ExamSession.Source == PackageAttempt`.
- Session status is `Submitted` or `Expired` after existing scoring/finalization.
- The session has exactly one `ExamSessionProvenance` row.
- The provenance references the same included exam version as the session.
- The provenance references the purchased reporting-profile publication.
- The provenance references the package purchase entitlement that owns the dormant/report-eligible right.
- The current nurse owns the session and entitlement.

Non-qualifying sessions include:

- `Legacy`, `Free`, or `StandaloneGrant` sessions;
- in-progress sessions;
- abandoned sessions;
- package sessions without provenance;
- package sessions whose provenance does not match the session;
- standalone sessions taken by a nurse who also owns a package;
- sessions from another package entitlement.

### Report Generation

Report generation must:

- use finalized session snapshots and answers only;
- use `ReportingProfileQuestionAssignment` rows from the exact `ReportingProfilePublicationId` stored in `ExamSessionProvenance`;
- match assignments by original `ExamQuestionId` referenced by `ExamSessionQuestion` snapshots;
- calculate overall counts from finalized session fields and topic counts from session question/answer snapshots;
- calculate percentages with deterministic decimal rounding consistent with existing scoring precision where applicable;
- create the report and all topic/guidance children in one transaction;
- return an existing report if another request already generated it;
- never mutate exam session score fields;
- never require exam finalization to call report generation.

### Practice Progress Exclusion

The generator must not read practice progress tables, practice answer history, or practice correctness counters as evidence. Practice mappings may be used only to produce purchased-content guidance references.

---

## Persistence Behavior

Stage 4 implementation will require EF Core Code-First persistence and one migration during a later approved implementation task.

Required persistence invariants:

- At most one report exists for one `ExamSessionId`.
- Every report references one `ExamSessionProvenance`.
- Report/provenance/package/session relationships use restrictive delete behavior; cascade delete is forbidden for financial, entitlement, session, provenance, or report records.
- Report rows are immutable after creation, except for generic audit fields if existing infrastructure requires them and the implementation plan explicitly constrains their use.
- Topic result rows are immutable children of the report.
- Guidance rows are immutable children of the report.
- Report generated-at timestamps are UTC and server-owned.
- Database uniqueness must protect idempotency under concurrent direct requests.
- Persistence must support detecting finalized qualifying package sessions without reports by querying `ExamSessions`, `ExamSessionProvenances`, and report rows.

The report schema must not include protected exam content, correct-answer data, provider secrets, payment secrets, tokens, password hashes, or internal right ids in any public-facing DTO shape.

---

## WebApi Behavior

Stage 4 v1 exposes direct nurse-owned report access only.

Recommended endpoint:

```text
GET /api/v1/me/nurse-profile/preparation-packages/exam-sessions/{sessionId}/report
```

Behavior:

- Requires authentication.
- Resolves current nurse ownership in Application.
- Returns an existing immutable report if present.
- Lazily generates the report if the session is qualifying, finalized, and report does not exist.
- Returns a safe report DTO.
- Does not expose a report list route.
- Does not expose employer, admin, workspace/dashboard, retry-management, or guidance-only routes.

If direct access by generated report id is included, it must be limited to current-nurse-owned report detail retrieval and must not become a list/discovery endpoint.

The endpoint should be mapped in the existing preparation package endpoint group and follow the existing metadata, `ISender`, MediatR, Problem Details, and `.RequireAuthorization()` patterns.

---

## Authorization and Access Behavior

- Only the owning nurse may request or read a package analytical report in v1.
- Anonymous requests return `401 Unauthorized`.
- Cross-nurse report/session/entitlement access must not disclose another nurse's package purchase facts.
- Missing and non-owned report/session resources should be indistinguishable to the client, using the existing project pattern for hidden nurse-owned resources.
- Employer access is forbidden in v1.
- Admin report access is deferred.
- Report access remains allowed after package entitlement expiry when a report exists or when a qualifying finalized package session exists and can be generated from persisted provenance.
- Active materials/practice access is not required for reading the report after expiry.
- Live payment status must not be queried to authorize report generation or access.
- Standalone `ExamAccessGrant` must not authorize package report generation or access.
- One package's report right cannot be qualified by another package's session.

---

## Report Immutability Rules

- The generated report is a point-in-time snapshot.
- Report content must be based on the exact finalized package session and exact provenance available at generation time.
- Later changes to package definitions, package versions, offers, reporting topics, reporting profiles, material versions, practice collections, payment records, or entitlement status must not mutate existing reports.
- Later changes to exam question authoring records must not mutate existing reports.
- Report topic names/display values used in the response should be snapshotted or otherwise preserved so historical reports do not change unexpectedly when taxonomy display names change.
- Guidance items should snapshot safe display values and deterministic ordering to preserve historical report meaning.
- Report regeneration after successful generation is not part of v1.

---

## Recovery and Idempotency Rules

- The first valid direct report request after finalization generates the report if it is absent.
- Repeating the same request returns the same report.
- Concurrent first requests must converge to one report through application checks plus a database uniqueness constraint on `ExamSessionId`.
- If one transaction wins report creation, the losing transaction must reload and return the existing report when facts match.
- A failed generation attempt must not permanently consume the report right.
- A failed generation attempt must not invalidate exam scoring.
- A failed generation attempt must not require the nurse to retake the exam.
- Stage 4 v1 does not add a report-generation failure table.
- Recovery means querying for finalized qualifying package sessions without reports and running the same idempotent generation path.
- Recovery must use deterministic persisted data only; no AI or external recommendation service participates.
- Report generation must not run from exam finalization in a way that can cause finalization to fail.

---

## Guidance Rules

Guidance is deterministic purchased-content guidance, not AI recommendation.

Guidance must:

- derive topic needs from exam-only topic results;
- use only the `ReportingProfilePublicationId`, `PracticeCollectionVersionId`, and purchased study material version ids tied to the exact package provenance/entitlement;
- include study material version references whose topic mappings match report topics;
- include practice collection version references for topics represented by practice items in the purchased practice collection version;
- use deterministic ordering, such as topic order, then package material sort order, then stable title/id ordering;
- return safe references and display metadata only.

Guidance must not:

- use practice progress as report evidence;
- recommend live-catalog content outside the purchased package version;
- include protected exam question text;
- include exam answer option text;
- include correct-answer identifiers or answer keys;
- include rationales or per-question exam review;
- include practice answer keys unless a later approved practice runtime explicitly defines safe practice-only immediate-feedback responses outside report scope;
- call AI, model-based ranking, adaptive recommendation, or predictive services.

---

## Security and Privacy Exclusions

Report persistence, DTOs, logs, and Problem Details responses must not expose:

- question text;
- answer option text;
- correct answers or correct option identifiers;
- answer keys;
- rationales;
- protected options;
- per-question exam review;
- provider secrets;
- payment secrets;
- payment provider raw payloads;
- access tokens;
- refresh tokens;
- password hashes;
- internal authorization state;
- internal package benefit right ids;
- package provenance internals not intended for the nurse-facing API;
- EF/domain navigation objects;
- stack traces.

Raw JSON WebApi tests must inspect response strings for forbidden sensitive fields before deserializing.

---

## Error and Conflict Semantics

Stage 4 implementation must use existing exception middleware and Problem Details conventions.

Recommended deterministic outcomes:

- Missing or non-owned session/report: `404 Not Found`, indistinguishable for privacy.
- Session source is not `PackageAttempt`: `409 Conflict` with stable client-safe code `package-report-session-not-qualified`.
- Session is `InProgress`: `409 Conflict` with stable client-safe code `package-report-session-not-finalized`.
- Session is `Abandoned`: `409 Conflict` with stable client-safe code `package-report-session-not-qualified`.
- Package session provenance is missing or inconsistent: `409 Conflict` with stable client-safe code `package-report-provenance-invalid`.
- Report eligibility right is missing or incompatible with provenance: `409 Conflict` with stable client-safe code `package-report-right-missing`.
- Reporting profile assignments are missing/incomplete for the finalized session evidence: `409 Conflict` with stable client-safe code `package-report-profile-incomplete`.
- Concurrent generation detects another successful report: reload and return the existing report.
- Unexpected generation failure: return a safe server error through existing middleware without consuming report eligibility permanently and without exposing sensitive details.

Problem Details must not expose internal ids except public route ids already supplied by the client, internal right ids, provenance internals, stack traces, protected content, provider/payment secrets, tokens, or password hashes.

---

## Testing Requirements

Future Stage 4 implementation must include tests by layer.

### Domain Tests

- Report creation captures immutable session/provenance/package facts.
- Report creation rejects empty required ids.
- Topic result creation stores counts, points, and percentages without labels.
- Guidance item creation supports study material version and practice collection version references.
- Report immutability prevents mutation after generation.
- Report entities do not expose protected exam content or internal right ids through public properties intended for DTO projection.

### Application Tests

- Generates report lazily for a finalized submitted package session.
- Generates report lazily for an automatically finalized expired package session.
- Rejects in-progress sessions with `package-report-session-not-finalized`.
- Rejects abandoned sessions as not qualified.
- Rejects `Legacy`, `Free`, and `StandaloneGrant` sessions as not qualified.
- Uses exact `ExamSessionProvenance` and exact `ReportingProfilePublicationId`.
- Rejects missing or inconsistent provenance.
- Calculates topic counts/percentages from exam evidence only.
- Does not read or use practice progress for evidence.
- Includes only counts and percentages; no bands or labels.
- Includes deterministic guidance from purchased study material versions.
- Includes deterministic guidance from the purchased practice collection version references.
- Excludes live-catalog material/practice content not included in the purchased package version.
- Returns existing report idempotently on repeated requests.
- Concurrent generation converges to one report.
- Failed generation does not permanently consume report eligibility.
- Report remains accessible or generatable after package expiry when the qualifying session/provenance exists.
- Standalone sessions by nurses who own packages do not qualify package reports.
- One package entitlement cannot qualify another package's report.
- Existing exam finalization remains independent from report generation.

### Infrastructure Tests

- EF configuration enforces unique report per exam session.
- EF configuration enforces required report/provenance/session/package relationships.
- EF configuration uses restrictive delete behavior for report relationships.
- EF configuration persists UTC generated timestamps.
- EF configuration supports deterministic child ordering.
- PostgreSQL concurrent first requests produce one report and one set of children.
- EF pending-model check passes after the Stage 4 migration.

### WebApi Tests

- Direct report endpoint returns `401` without JWT.
- Valid finalized package session request returns report DTO.
- Repeated direct request returns the same report.
- Cross-nurse request returns hidden not found behavior without exposure.
- In-progress package session returns conflict Problem Details with exact code.
- Non-package session returns conflict Problem Details with exact code.
- Raw JSON response does not expose forbidden sensitive fields.
- Report remains accessible after entitlement expiry.
- Endpoint scope test proves no report list, employer report, workspace/dashboard, admin report, retry-management, AI guidance, or frontend/design route leakage.

### Compatibility Tests

- Existing `POST /api/v1/exams/{id}/sessions` free/standalone start behavior remains unchanged.
- Existing exam submit/result/review behavior remains unchanged and does not expose report-only fields.
- Existing exam analytics remain aggregate analytics and are not replaced by package reports.
- Existing payment/entitlement/package exam-session endpoints remain backward compatible.

---

## Implementation Slice Outline

This outline is not an implementation plan and does not authorize work. A separate implementation plan is required after this specification is reviewed and approved.

Recommended future slices:

1. Domain report snapshot model and domain tests.
2. Application report DTOs, validators, and generation service tests.
3. Persistence configuration, migration, uniqueness constraints, and infrastructure tests.
4. Lazy generation query/command with idempotent recovery behavior.
5. Deterministic guidance projection from purchased study material versions and practice collection version references.
6. WebApi direct report endpoint and Problem Details mapping.
7. Security, scope, compatibility, concurrency, EF, build, and final verification.

---

## Open Questions

No unresolved business questions block this draft specification.

Implementation-planning details deliberately deferred until after this specification is reviewed:

- Exact entity, table, property, DTO, command, query, and exception names.
- Whether direct access by generated report id is also included in addition to direct access by package exam session id.
- Exact decimal rounding rules for topic percentages if existing score precision is not sufficient.
- Exact safe display fields for study material and practice collection guidance references.
- Whether `ReportEligibility` right status gains a generated/consumed state or report linkage is represented only by the immutable report table.
- Exact stable Problem Details code names if implementation review chooses different but equivalent names.

These are implementation details, not missing business rules.

---

## Acceptance Criteria for This Specification

This specification is acceptable for review when it:

- stays within Stage 4 analytical report generation, recovery, direct access, and deterministic guidance scope;
- records lazy/on-demand generation as the v1 trigger;
- excludes report list endpoints from v1;
- defers performance bands and labels;
- requires exam-only report evidence;
- excludes practice progress from report evidence;
- requires use of exact `ExamSessionProvenance`;
- requires immutable report persistence;
- preserves report access after package expiry;
- includes purchased study material and purchased practice collection guidance references;
- forbids protected exam, payment, authorization, token, and password-hash exposure;
- forbids employer report access, workspace/dashboard runtime, frontend/design work, and AI guidance;
- does not update `CURRENT_TASK.md` or `TASKS.md`;
- does not modify backend runtime, tests, migrations, frontend/design files, or `.agent/goal-state.md`;
- does not stage, commit, push, or delete branches.
