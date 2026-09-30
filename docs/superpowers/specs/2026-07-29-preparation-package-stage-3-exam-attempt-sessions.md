# Preparation Package Stage 3 — Package Exam Attempt / Session Authorization and Provenance

## Status

Approved — Stage 3 Specification

This specification defines the Stage 3 product and architecture requirements for package exam attempt start, attempt consumption, exam session source distinction, and immutable session provenance. It does not authorize implementation, implementation planning, source-code changes beyond this specification file, database migrations, API implementation, tests, frontend work, staging, committing, pushing, deleting branches, or beginning Stage 4.

---

## Purpose

Stage 3 connects the Stage 2 package entitlement and benefit-right foundation to the existing exam session runtime without changing the existing free or standalone paid exam-start behavior.

The goal is to allow a nurse to explicitly start the exam included in one owned package purchase, consume the package's `PackageExamAttemptEligibility` right atomically with qualifying session creation, and record immutable session provenance so Stage 4 can later generate reports from the correct package purchase and session.

---

## Authoritative Inputs

This specification is based on:

- `AGENTS.md`
- `PROJECT_RULES.md`
- `CURRENT_TASK.md`
- `TASKS.md`
- `docs/superpowers/specs/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md`
- `docs/superpowers/plans/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md`
- Existing exam/session implementation in `backend/src/NursingPlatform.Domain/Exams/`, `backend/src/NursingPlatform.Application/Exams/`, and `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Stage 2 package entitlement and benefit-right implementation in `backend/src/NursingPlatform.Domain/PreparationPackages/` and `backend/src/NursingPlatform.Application/PreparationPackages/`
- The approved Stage 3 planning review for package exam attempt/session authorization and provenance.

If this specification conflicts with an approved umbrella or earlier staged preparation-package specification, the already-approved document remains authoritative until the conflict is explicitly resolved by the user.

---

## Existing Baseline

The existing exam start endpoint is:

```text
POST /api/v1/exams/{id}/sessions
```

It sends `StartExamSessionCommand` and returns an `ExamSessionDto`. Its current behavior includes:

- resolving the current nurse profile;
- loading the requested published exam;
- selecting the latest published exam version;
- authorizing free or standalone paid access through `ExamAccessPolicy`;
- checking for an existing in-progress session for the nurse and selected exam version;
- returning the existing session when it is still in progress and not expired;
- finalizing an expired in-progress session before creating a new standalone/free session;
- creating session question and answer-option snapshots;
- returning an exam session DTO that does not expose correct answers.

The existing standalone paid authorization evidence is `ExamAccessGrant`. The existing effective-paid classification rule is based on exam/payment-product state only:

```text
Exam.IsFree == false OR active positive-price ExamAccess product exists
```

The existing database already enforces one in-progress session per nurse and exam version through a filtered unique index over `(NurseProfileId, ExamVersionId)` where session status is `InProgress`.

Stage 3 must preserve this baseline and add package-attempt behavior as a separate source.

---

## Stage 2 Foundation Received by Stage 3

Stage 3 receives these implemented Stage 2 capabilities:

- `PackagePurchaseEntitlement` as the nurse-owned package purchase aggregate.
- `PackageBenefitRight` rows owned by the entitlement.
- `PackageBenefitRightType.PackageExamAttemptEligibility` as the right authorizing package exam attempt start.
- `PackageBenefitRightStatus.Consumed` reserved for Stage 3 attempt consumption.
- Entitlement fields for `IncludedExamId`, `IncludedExamVersionId`, `PreparationPackageDefinitionId`, `PreparationPackageVersionId`, `PreparationPackageOfferId`, `PurchasedOfferSnapshotId`, access-window dates, and ownership.
- `PackageBenefitAuthorizationService` for current-nurse package benefit authorization by entitlement/right state.
- `PackagePaymentFulfillmentService` creating package entitlements and the four Stage 2 rights after successful package payment fulfillment.
- Nurse-owned entitlement list/detail APIs exposing entitlement identity, included exam identity, snapshot facts, access window, and benefit-right statuses without exposing internal right ids.

Payment orders, checkout sessions, and payment status are fulfillment history. Stage 3 authorization must use entitlement and benefit-right state, not live payment status.

---

## Stage 3 Scope

Stage 3 includes only:

1. A package-specific exam start operation where the client explicitly selects a package purchase entitlement id.
2. Backend resolution of the internal `PackageExamAttemptEligibility` right for the selected entitlement.
3. Authorization checks for package attempt start.
4. Atomic consumption of the package attempt right with qualifying exam session creation.
5. Immutable logical session source recording for new sessions.
6. Immutable package session provenance recording.
7. Idempotent same-source retry behavior.
8. Deterministic consumed/terminal-session behavior.
9. Deterministic cross-source in-progress-session conflict behavior.
10. Preservation of the existing one in-progress session per nurse/exam-version rule across free, standalone, and package sources.
11. Preservation of existing free and standalone paid session start behavior.
12. Tests proving authorization, atomicity, idempotency, concurrency, security, and compatibility.

Stage 3 may introduce the domain concepts, application services/commands, persistence schema, API route, DTOs, and tests necessary for these requirements only after a separate implementation plan is reviewed and approved.

---

## Out of Scope

Stage 3 does not implement or authorize:

- report generation;
- report access;
- report analytics;
- report retry operations;
- report guidance;
- activation or consumption of the dormant `ReportEligibility` right;
- package workspace runtime;
- material delivery/download runtime;
- practice runtime or practice progress APIs;
- employer package purchase, session, report, or practice-progress data;
- frontend or design work;
- production payment-provider changes;
- webhooks, refunds, reconciliation, subscriptions, carts, sponsored packages, coupons, taxes, invoices, wallets, or payouts;
- manual admin/support attempt consumption reset;
- package session provenance mutation;
- source switching between free, standalone, and package sessions.

Stage 3 must not expose Stage 4 or workspace behavior through routes, DTOs, database tables, or hidden placeholder fields.

---

## Existing Standalone and Free Behavior That Must Remain Unchanged

The existing `POST /api/v1/exams/{id}/sessions` endpoint remains the free/standalone start operation.

Preserved behavior:

- Free exam start remains authorized by existing free-exam rules.
- Standalone paid exam start remains authorized by `ExamAccessGrant` when the existing paid classification requires a grant.
- Standalone paid exam purchase and Sandbox completion continue to create `ExamAccessGrant` rows.
- Package fulfillment continues not to create `ExamAccessGrant` rows.
- Package entitlements and package benefit rights do not participate in standalone effective-paid classification.
- Existing `isFree` and `canStart` semantics remain backward-compatible and must not silently include package-attempt eligibility.
- Existing session retrieval, answer save, submit, result, and review behavior remains governed by current exam-session ownership and lifecycle rules.
- Existing DTOs must not begin exposing correct answers, answer identifiers, answer keys, internal scoring logic, protected options, rationales, provider secrets, tokens, password hashes, or internal authorization state.

Stage 3 may add internal source/provenance persistence for free and standalone sessions, but this must not change whether a free or standalone session can be started.

---

## Package-Specific Exam Start Flow

Stage 3 introduces a separate package-attempt start operation:

```text
POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session
```

The client supplies only the package purchase entitlement id in the route. The backend resolves all other facts from authoritative state.

The client must not supply:

- internal benefit right id;
- nurse profile id;
- exam id;
- exam version id;
- package definition id;
- package version id;
- package offer id;
- purchased snapshot id;
- access-window dates;
- source/provenance facts;
- right status;
- scoring/report facts.

Required package start flow:

```text
current authenticated nurse
    -> selected PackagePurchaseEntitlement id
        -> verify nurse ownership
        -> resolve PackageExamAttemptEligibility right internally
        -> verify entitlement active at session creation time
        -> verify right Available and within its access window
        -> verify exact included exam and included exam version are startable
        -> check existing in-progress session for nurse/exam-version across all sources
        -> create package-sourced ExamSession and ExamSessionProvenance
        -> consume PackageExamAttemptEligibility right
        -> persist session, provenance, question snapshots, option snapshots, and right consumption atomically
        -> return safe session DTO
```

The package start operation must use the entitlement's purchased `IncludedExamId` and `IncludedExamVersionId`. It must not select the latest published exam version when starting a package attempt.

---

## Why the Existing Exam Start Endpoint Must Not Be Silently Extended

The existing endpoint must not silently start package attempts because package start has a different authorization model and requires explicit entitlement selection.

Reasons:

- A package attempt consumes a scarce package-specific right; free and standalone starts do not.
- The package attempt must be tied to one selected `PackagePurchaseEntitlement`.
- The backend must resolve the internal `PackageExamAttemptEligibility` right; the client must not select it.
- The package attempt must use the purchased exact exam version, not necessarily the latest published version.
- `ExamAccessGrant` and package rights are not interchangeable.
- Silent entitlement selection would make it ambiguous which package is consumed when multiple package entitlements include the same exam.
- Silent extension would risk changing `canStart` semantics and breaking standalone/free compatibility.

The existing endpoint may only be updated to record source metadata for sessions it already creates. It must not read package entitlements, consume package rights, infer package source, or switch an existing free/standalone session into a package session.

---

## Source Distinction

Every stored exam session has one immutable logical source:

- `Legacy` — historical/backfill-only value for pre-Stage-3 `ExamSession` rows created before source/provenance existed.
- `Free` — started through the existing exam start operation where the exam does not require paid authorization under current policy.
- `StandaloneGrant` — started through the existing exam start operation and authorized by standalone `ExamAccessGrant`.
- `PackageAttempt` — started through the new package-specific start operation and authorized by one selected package purchase entitlement's `PackageExamAttemptEligibility` right.

The source is server-owned and selected by the start operation or by an approved migration backfill, not by the client.

Rules:

- `Legacy` is allowed only as a migration backfill value for existing pre-Stage-3 rows.
- New application-created sessions after Stage 3 must never use `Legacy`.
- New free sessions must use `Free`.
- New standalone paid sessions must use `StandaloneGrant`.
- New package sessions must use `PackageAttempt`.
- `Legacy` does not authorize package behavior.
- `Legacy` does not create package provenance.
- `Legacy` must not be used for report qualification.
- Existing historical sessions with `Legacy` remain resumable/reviewable only under existing session ownership and lifecycle rules.
- A session source cannot be changed after creation.
- A free or standalone session cannot become a package session.
- A package session cannot become a free or standalone session.
- A standalone `ExamAccessGrant` cannot satisfy package attempt start.
- A package benefit right cannot satisfy standalone paid exam start.
- Package start must not create or mutate `ExamAccessGrant`.

---

## Session Provenance Model

Session provenance is the durable, immutable record of why a session was authorized and which package purchase, if any, produced it.

Stage 3 should introduce a session source discriminator on `ExamSession` and a one-to-one `ExamSessionProvenance` persistence model for package-specific provenance.

The source discriminator belongs to the session lifecycle. Package provenance belongs to an owned one-to-one provenance record associated with the session.

### Recommended Storage

Use a one-to-one `ExamSessionProvenance` table rather than many nullable package columns on `ExamSession`.

Reasons:

- Keeps the core `ExamSession` entity focused on exam runtime state.
- Avoids growing `ExamSession` with many nullable columns that apply only to package attempts.
- Makes package provenance reviewable as a cohesive model with a single responsibility.
- Allows strong one-to-one constraints and package-specific indexes without obscuring existing session fields.
- Gives Stage 4 a stable package-session provenance join without adding report concerns to `ExamSession`.
- Preserves clearer DTO boundaries: session runtime fields remain separate from internal package authorization evidence.

The provenance record must be created in the same transaction as the package session. It must not be created later by a background process.

### Provenance Required for Package Sessions

For `PackageAttempt` sessions, the provenance record must store these server-owned immutable facts:

- `ExamSessionId`.
- `PackagePurchaseEntitlementId`.
- internal `PackageBenefitRightId` for the resolved `PackageExamAttemptEligibility` right.
- `PackageOrderItemSnapshotId` or equivalent purchased-offer snapshot id.
- `PaymentOrderId`.
- `PaymentOrderItemId`.
- `PreparationPackageDefinitionId`.
- `PreparationPackageVersionId`.
- `PreparationPackageOfferId`.
- `IncludedExamId`.
- `IncludedExamVersionId`.
- `ReportingProfilePublicationId`.
- `PracticeCollectionVersionId`.
- package access-window start and end as observed at session creation.
- `StartedAt` or provenance creation timestamp in UTC.

The provenance record must not store protected exam question text, answer identifiers, correct answers, answer keys, protected options, rationales, internal scoring logic, report evidence, report analytics, provider secrets, tokens, password hashes, or free-text provenance.

For `Legacy`, `Free`, and `StandaloneGrant` sessions, package provenance fields are not present. The session source discriminator is sufficient for Stage 3. If a one-to-one row is used for all source types, legacy, free, and standalone rows must not contain package ids or internal package right ids.

---

## PackageExamAttemptEligibility Authorization Rules

Package-attempt authorization must check all of the following:

1. The request is authenticated.
2. The current user has a nurse profile.
3. The selected `PackagePurchaseEntitlement` exists for the current nurse.
4. The entitlement is active at the session creation timestamp: `Status == Active`, `AccessStartsAt <= now`, and `now < AccessEndsAt`.
5. The entitlement contains exactly one `PackageExamAttemptEligibility` right.
6. The attempt right status is `Available`.
7. The attempt right access window contains the session creation timestamp.
8. The selected entitlement's `IncludedExamId` exists and remains valid for session creation.
9. The selected entitlement's exact `IncludedExamVersionId` exists and remains startable for the historical package attempt.
10. The package start operation uses the entitlement's included exam and exam version; it does not accept client-supplied exam or version ids.
11. There is no conflicting in-progress session for the same nurse and exam version from a different source or different package entitlement.

Authorization must not query live payment status to decide package attempt eligibility.

Authorization must not be satisfied by standalone `ExamAccessGrant`.

Authorization must not be satisfied by a different entitlement, another package's right, a materials right, a practice right, or a dormant report right.

---

## Right Consumption

The `PackageExamAttemptEligibility` right is consumed only when qualifying package session creation succeeds.

Consumption means the right transitions from `Available` to `Consumed` and records a server-owned timestamp in a dedicated nullable `ConsumedAt` business timestamp on the package attempt right. Stage 3 implementation must not rely only on generic audit timestamps to prove attempt consumption.

Rules:

- A failed package start must not consume the right.
- A validation failure must not consume the right.
- A source conflict must not consume the right.
- A database failure before transaction commit must not consume the right.
- A retry after successful commit must not consume the right a second time.
- A consumed right cannot authorize a new package session.
- The package attempt right is independent from the dormant `ReportEligibility` right.
- Stage 3 must not reset, revoke, or manually adjust consumed rights.

---

## Atomicity Requirements

Package attempt consumption and session creation must be atomic.

Required invariant:

```text
No consumed package attempt right without the qualifying package session.
No package attempt session without the consumed package attempt right.
```

The same transaction must persist:

- the `ExamSession` with source `PackageAttempt`;
- the one-to-one `ExamSessionProvenance` record;
- exam session question snapshots;
- exam session answer-option snapshots;
- the `PackageExamAttemptEligibility` right transition to `Consumed`.

If any part fails before commit, the entire operation rolls back.

If another concurrent transaction wins session creation or right consumption, the losing transaction must reload authoritative state and return either an idempotent same-source result or a deterministic conflict. Stage 3 uses optimistic concurrency plus database uniqueness and reload behavior. PostgreSQL row locking must not be used unless later implementation evidence proves it necessary and a separate review approves that change.

---

## Idempotency Behavior

Package start idempotency is based on:

```text
current nurse profile id + package purchase entitlement id + included exam version id
```

Required behavior:

- Repeating the same package start request after the first request committed but before the client received the response returns the same in-progress package session.
- Repeating the same package start request after a rollback treats the request as a fresh attempt because the right remains `Available`.
- Repeating the same package start request must never create duplicate sessions for the same nurse/exam version.
- Repeating the same package start request must never consume the same right more than once.
- Idempotency is scoped to the selected entitlement. Another package entitlement that includes the same exam is a different package source and must not silently reuse the first entitlement's right.

Stage 3 does not require a client-supplied idempotency key for package start. The natural entitlement-scoped key is sufficient for v1.

---

## Cross-Source In-Progress Session Conflict Behavior

The existing concurrency rule remains:

```text
One in-progress session per nurse per exam version across all access sources.
```

Conflict rules:

- If a free or standalone in-progress session exists for the nurse and exam version, package start for that same exam version returns a deterministic source-conflict outcome.
- If a package in-progress session exists for the same entitlement and exam version, package start returns the existing session idempotently.
- If a package in-progress session exists for a different package entitlement but the same nurse and exam version, package start returns a deterministic source-conflict outcome.
- If package start creates an in-progress session first, a later free or standalone start for the same nurse and exam version must return a deterministic source-conflict outcome rather than silently resuming the package session as standalone/free.
- Database unique constraint violations for the in-progress nurse/exam-version index must be translated to the same deterministic conflict behavior after reloading state.

Different packages containing the same exam may coexist as entitlements. They may not create simultaneous in-progress sessions for the same nurse and exam version.

---

## Same-Source Retry Behavior

A same-source package retry means the same nurse repeats package start for the same package purchase entitlement and included exam version.

Required same-source behavior:

- If the matching package session is still `InProgress` and the session timer has not expired, return/resume that same session.
- If the matching package session is `InProgress` but the session timer has expired, finalize it using existing session finalization behavior, then return the deterministic consumed outcome. A new package session must not be created from the already-consumed right.
- If the matching package session is terminal, return a deterministic consumed outcome.
- Same-source retry must not re-check entitlement access-window validity after session creation when returning the already-created in-progress session. The entitlement only had to be valid at creation time.

---

## Consumed and Terminal Session Behavior

When a `PackageExamAttemptEligibility` right has been consumed by a terminal session, package start for the same entitlement must return a deterministic consumed outcome.

The deterministic consumed outcome should be represented as a client-safe Problem Details response in WebApi, with a stable code selected during implementation planning. It must not expose internal right ids, stack traces, internal authorization state, or protected exam content.

Terminal statuses follow the existing exam session lifecycle, including submitted, expired, and abandoned states.

Stage 3 does not create a second package attempt after terminal completion. Repurchase-after-expiry creates a separate entitlement and separate attempt right through the already-approved Stage 2 commerce/fulfillment behavior.

---

## Expiry Behavior

The package entitlement and package attempt right must be valid at package session creation time.

After successful package session creation:

- package entitlement expiry does not invalidate the already-created in-progress session;
- resume remains allowed while the session itself is valid under the server-owned exam timer;
- answer saving, submit, scoring, and automatic finalization continue to use session ownership and session lifecycle rules;
- no additional package attempt is consumed;
- report qualification remains possible for Stage 4 based on the session and provenance, not on the entitlement still being active at report time.

If the entitlement expires before package session creation, package start is denied and the attempt right is not consumed.

Materials and practice access continue to end with the package access window under Stage 2 rules. Stage 3 does not alter those benefits.

---

## API Boundary

Stage 3 adds only the package-attempt start endpoint:

```text
POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session
```

Authorization:

- Requires authentication.
- Requires current nurse ownership of the selected entitlement.
- Does not require an admin permission.
- Does not allow anonymous access.

Response boundary:

- Returns an additive package-start DTO wrapping safe session data and safe package/source/entitlement summary.
- Exposes session source as a safe nurse-facing field with values `Free`, `StandaloneGrant`, and `PackageAttempt`.
- Must not expose internal package benefit right ids in public DTOs.
- Must not expose package provenance internals in nurse-facing DTOs.
- Must not expose protected exam content beyond the existing safe session question/option presentation.
- Must not expose correct answers, answer identifiers, answer keys, protected options, rationales, report evidence, provider secrets, tokens, password hashes, stack traces, or internal authorization state.

The existing `POST /api/v1/exams/{id}/sessions` endpoint remains the free/standalone operation and must not accept package entitlement ids.

---

## Security and Privacy Requirements

- Backend authorization is authoritative.
- Client input is limited to selecting the package purchase entitlement id for package start.
- The backend resolves nurse ownership, entitlement, right, exam id, exam version id, package ids, purchased snapshot id, and provenance facts.
- Internal `PackageBenefitRight` ids are never client-selected.
- Public/nurse-facing DTOs must expose only safe source and entitlement summary fields; they must not expose internal right ids or package provenance internals.
- Package right status and provenance must not be writable by the client.
- No source switching or provenance rewriting endpoint may exist.
- Package attempts must not create, mutate, or rely on `ExamAccessGrant`.
- `ExamAccessGrant` must not authorize package attempt start.
- Logs must not include provider secrets, payment tokens, access tokens, refresh tokens, password hashes, protected exam content, answer identifiers, correct answers, answer keys, rationales, report evidence, or internal authorization state.
- Cross-nurse entitlement probing must not expose another nurse's package purchase facts.
- Raw JSON endpoint tests must verify forbidden sensitive fields are absent.

---

## Data Integrity Requirements

Future implementation must use EF Core Code-First migrations and PostgreSQL constraints. Manual schema changes are forbidden.

Required invariants:

- Every session has exactly one immutable source value.
- Existing pre-Stage-3 sessions are backfilled to `Legacy`; new application-created sessions must use explicit non-`Legacy` source values.
- Every package-attempt session has exactly one provenance row.
- Every package-attempt provenance row references one exam session.
- A package-attempt provenance row references the selected package entitlement and resolved package exam attempt right.
- A package-attempt provenance row references the package purchase snapshot and package composition facts needed by Stage 4.
- Package provenance relationships use restrictive delete behavior; cascade delete of financial, entitlement, right, session, or provenance records is forbidden.
- One in-progress session per nurse/exam version remains enforced at the database level across all sources.
- Attempt consumption and package session/provenance creation are committed atomically.
- Source and provenance cannot be rewritten after creation.
- Package attempt right consumption stores a dedicated nullable `ConsumedAt` timestamp.

---

## Error and Conflict Semantics

Stage 3 must define deterministic client-safe outcomes for:

- entitlement not found or not owned by current nurse: return `404 Not Found`; missing and non-owned entitlements must be indistinguishable to the client;
- entitlement inactive or outside access window at creation: return `409 Conflict` with stable client-safe code `package-entitlement-inactive`;
- attempt right missing: return `409 Conflict` with stable client-safe code `package-attempt-right-missing`;
- attempt right not available because it was consumed by a terminal package session: return `409 Conflict` with stable client-safe code `package-attempt-consumed`;
- exact included exam/exam-version mismatch or unavailable historical exam version: return `409 Conflict` with stable client-safe code `package-exam-version-unavailable`;
- different-source in-progress session conflict: return `409 Conflict` with stable client-safe code `exam-session-source-conflict`;
- same exam version in progress from a different package entitlement: return `409 Conflict` with stable client-safe code `exam-session-source-conflict`;
- concurrency conflict where another transaction creates the in-progress session or consumes the right first: reload authoritative state and return either the same-source idempotent session result or the appropriate `409 Conflict` stable code above.

Problem Details responses must remain client-safe and must not expose internal right ids, package provenance internals, stack traces, protected exam content, report evidence, provider secrets, tokens, password hashes, or internal authorization state.

---

## Required Tests for Future Implementation

Future Stage 3 implementation must include tests by layer.

### Domain Tests

- `ExamSession_CreateFreeSource_RecordsImmutableFreeSource`
- `ExamSession_CreateStandaloneGrantSource_RecordsImmutableStandaloneGrantSource`
- `ExamSession_CreatePackageAttemptSource_RecordsImmutablePackageSource`
- `ExamSession_CreateLegacySource_RecordsImmutableLegacySourceForBackfillOnly`
- `ExamSessionProvenance_CreateForPackageAttempt_CapturesRequiredPackageFacts`
- `ExamSessionProvenance_CreateForPackageAttempt_RejectsMissingEntitlementRightOrSnapshotIds`
- `PackageBenefitRight_ConsumePackageExamAttempt_TransitionsAvailableToConsumed`
- `PackageBenefitRight_ConsumePackageExamAttempt_SetsConsumedAt`
- `PackageBenefitRight_ConsumePackageExamAttempt_WhenNotAvailable_Throws`
- `PackageBenefitRight_Expire_DoesNotOverwriteConsumedAttemptRight`

### Application Tests

- `Handle_StartPackageAttempt_WithActiveEntitlementAndAvailableRight_CreatesSessionProvenanceAndConsumesRight`
- `Handle_StartPackageAttempt_WithExpiredEntitlement_DeniesWithoutConsumingRight`
- `Handle_StartPackageAttempt_WithForeignEntitlement_DeniesWithoutExposingEntitlementFacts`
- `Handle_StartPackageAttempt_WithMissingAttemptRight_DeniesWithoutCreatingSession`
- `Handle_StartPackageAttempt_WithConsumedRightAndInProgressMatchingSession_ReturnsSameSession`
- `Handle_StartPackageAttempt_WithConsumedRightAndTerminalSession_ReturnsConsumedOutcome`
- `Handle_StartPackageAttempt_WithDifferentSourceInProgress_ReturnsSourceConflictWithoutConsumingRight`
- `Handle_StartPackageAttempt_WithDifferentPackageEntitlementInProgressForSameExamVersion_ReturnsSourceConflict`
- `Handle_StartPackageAttempt_UsesEntitlementIncludedExamVersion_NotLatestPublishedVersion`
- `Handle_StartPackageAttempt_DoesNotReadOrCreateExamAccessGrant`
- `Handle_StartExamSession_ForFreeExam_RecordsFreeSourceAndDoesNotConsumePackageRight`
- `Handle_StartExamSession_ForStandalonePaidExam_RecordsStandaloneGrantSourceAndDoesNotConsumePackageRight`
- `Handle_StartExamSession_NeverCreatesLegacySource`
- `Handle_StartExamSession_WithPackageEntitlementOnly_StillRequiresStandaloneGrantWhenPaidPolicyRequiresGrant`
- `Handle_StartPackageAttempt_WithLegacySessionSource_DoesNotSatisfyPackageStart`
- `Handle_StartPackageAttempt_WhenSessionCreationFails_DoesNotConsumeRight`
- `Handle_StartPackageAttempt_WhenRightConsumptionFails_DoesNotPersistSession`
- `Handle_StartPackageAttempt_AfterEntitlementExpiryButSessionInProgress_ReturnsExistingSessionWithoutRecheckingWindow`

### Infrastructure Tests

- `ExamSessionConfiguration_PersistsSessionSourceAsRequiredString`
- `ExamSessionMigration_BackfillsExistingRowsToLegacySource`
- `ExamSessionProvenanceConfiguration_UsesOneToOneSessionRelationship`
- `ExamSessionProvenanceConfiguration_UsesRestrictDeleteBehavior`
- `ExamSessionProvenanceConfiguration_IndexesPackageEntitlementAndBenefitRight`
- `ExamSessionConfiguration_EnforcesOneInProgressSessionPerNurseAndExamVersionAcrossSources`
- `PackageBenefitRightConfiguration_PersistsConsumedAttemptStatus`
- `PackageAttemptStart_WithConcurrentSameSourceRequests_ConvergesToOneSessionAndOneConsumedRight`
- `PackageAttemptStart_WithConcurrentDifferentSourceRequests_AllowsOneWinnerAndReturnsDeterministicConflictForLoser`
- `PackageAttemptStart_WhenTransactionRollsBack_DoesNotLeaveConsumedRightWithoutSession`

### WebApi Tests

- `StartPackageExamSession_Returns401WithoutJwt`
- `StartPackageExamSession_WithForeignEntitlement_ReturnsNotFoundOrForbiddenWithoutExposure`
- `StartPackageExamSession_WithValidEntitlement_ReturnsSessionAndNoInternalRightId`
- `StartPackageExamSession_WithValidEntitlement_ReturnsSafePackageStartDtoWithSourceAndEntitlementSummary`
- `StartPackageExamSession_WithDifferentSourceInProgress_ReturnsConflictProblemDetails`
- `StartPackageExamSession_WithConsumedTerminalAttempt_ReturnsConsumedProblemDetails`
- `StartPackageExamSession_RawJsonDoesNotExposeSensitiveFields`
- `StartPackageExamSession_WithLegacySessionSource_DoesNotAuthorizePackageBehavior`
- `ExistingStartExamSession_ForFreeExam_RemainsBackwardCompatible`
- `ExistingStartExamSession_ForStandalonePaidExam_RemainsBackwardCompatible`
- `ExistingStartExamSession_DoesNotAcceptOrConsumePackageEntitlement`
- `ExamCatalogAndDetail_IsFreeAndCanStart_DoNotSilentlyIncludePackageRights`
- `Stage3EndpointScope_DoesNotExposeReportWorkspaceEmployerOrAttemptResetRoutes`

---

## Stage 4 and Later Handoff

Stage 4 receives immutable package session provenance from Stage 3. Stage 4 must use that provenance to determine whether a submitted or finalized package session qualifies for the dormant report right.

Deferred to Stage 4 or later:

- report generation;
- report access;
- report persistence;
- report retry;
- report guidance;
- report analytics;
- report-right activation or consumption;
- workspace runtime;
- practice progress runtime;
- materials runtime;
- employer package data;
- admin report controls;
- support reset of consumed package attempts.

Stage 3 must not implement placeholder report tables or routes.

---

## Resolved Open Questions

These items were open questions in the review draft and are now resolved by user approval before implementation planning:

1. Problem Details use `409 Conflict` with stable client-safe codes: `exam-session-source-conflict`, `package-attempt-consumed`, `package-entitlement-inactive`, `package-attempt-right-missing`, and `package-exam-version-unavailable`.
2. Package start success returns an additive package-start DTO wrapping safe session data and safe package/source/entitlement summary. It must not expose internal benefit right ids.
3. Nurse-facing session DTOs may expose session source as a safe field with values `Legacy`, `Free`, `StandaloneGrant`, and `PackageAttempt`. `Legacy` is historical/backfill-only and must not be created by new application flows. DTOs must not expose package provenance internals.
4. Missing and non-owned package entitlements return `404 Not Found` and must be indistinguishable to the client.
5. The package attempt right receives a dedicated nullable `ConsumedAt` business timestamp during implementation. Generic audit timestamps are not sufficient for attempt-consumption evidence.
6. Concurrency uses optimistic concurrency plus database uniqueness and reload behavior. PostgreSQL row locking is not part of the approved Stage 3 design unless later implementation evidence proves it necessary and a separate review approves it.
7. If same-source package retry finds an expired in-progress package session, the handler finalizes it using existing session finalization behavior, then returns the deterministic consumed outcome. It must not create a replacement package session from the already-consumed right.

---

## Decisions Summary

- Stage 3 uses a separate package-specific start endpoint: `POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session`.
- Package start success returns an additive package-start DTO wrapping safe session data and safe package/source/entitlement summary.
- The client selects an entitlement id only.
- The backend resolves the current nurse, package entitlement, included exam/version, internal `PackageExamAttemptEligibility` right, and all provenance facts.
- The client must not send internal right id, exam version id, package version id, or provenance facts.
- Public DTOs expose safe session source values `Legacy`, `Free`, `StandaloneGrant`, and `PackageAttempt`, but do not expose internal benefit right ids or package provenance internals.
- `Legacy` is a safe historical/backfill-only source value for pre-Stage-3 sessions. It does not authorize package behavior, does not create package provenance, must not qualify for reports, and must never be used for new application-created sessions.
- Missing and non-owned package entitlements return indistinguishable `404 Not Found` responses.
- Stage 3 conflict outcomes use `409 Conflict` with stable client-safe codes: `exam-session-source-conflict`, `package-attempt-consumed`, `package-entitlement-inactive`, `package-attempt-right-missing`, and `package-exam-version-unavailable`.
- Package right authorization and standalone grant authorization remain separate.
- Package rights do not satisfy standalone exam start.
- `ExamAccessGrant` does not satisfy package attempt start.
- Existing free/standalone endpoint behavior remains backward-compatible.
- Every session receives one immutable source: `Legacy`, `Free`, `StandaloneGrant`, or `PackageAttempt`; only migration-backfilled historical sessions may use `Legacy`.
- Package provenance is immutable once created.
- Recommended storage is a one-to-one `ExamSessionProvenance` table rather than many nullable `ExamSession` columns.
- Package attempt right consumption happens only when package session creation succeeds.
- Package attempt right consumption records a dedicated nullable `ConsumedAt` business timestamp.
- The system must never persist a consumed package attempt right without its qualifying session, or a package attempt session without the consumed right.
- Retry must not double-consume package rights.
- Concurrency uses optimistic concurrency plus database uniqueness and reload behavior; PostgreSQL row locking is not approved unless later evidence and review require it.
- Same-source retry for an expired in-progress package session finalizes the session through existing finalization behavior and returns consumed outcome without creating a replacement session.
- One in-progress session per nurse/exam-version across free, standalone, and package sources remains the concurrency rule.
- Different packages containing the same exam can coexist as entitlements but cannot create simultaneous in-progress sessions for the same nurse/exam-version.
- Stage 4 report generation/access, analytics, workspace runtime, and employer package data remain deferred.
