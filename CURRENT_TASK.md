# Current Task

## Current Milestone

Preparation Package — Stage 1 Runtime Dedicated Administration Permissions Docs Status Finalization Complete

Status:
Preparation Package Stage 1 Runtime — Dedicated Administration Permissions Docs Status Finalization is complete and stopped for review. Verification confirmed the existing implementation and added WebApi test coverage already satisfy the Stage 1 runtime dedicated practice/material administration permission item while preserving existing free, standalone paid-exam, payment, entitlement, package exam-session, package analytical report, and exam analytics behavior.

Practice Progress implementation and docs/status completion on branch `feature/preparation-package-foundation` are represented by commits `a90074c`, `7f54e49`, `6c11e61`, `0454c0f`, `cffc846`, and `8b964ae`. The Practice Retry/Retraining Authorization status finalization is represented by commit `54c61a5`. Practice exam-content isolation evidence includes existing guard commit `695cfbf`. Dedicated administration permissions test coverage is represented by commit `17e38eb`. This Dedicated Administration Permissions Docs Status Finalization is documentation-only and does not authorize runtime changes.

---

## Completed Baseline — Backend Local MVP (Payment Fulfillment and Purchased Exam Access)

The backend local MVP for payment fulfillment and purchased exam access is complete and preserved as the compatibility baseline. It is not the current task. Its behavior must remain observably unchanged by preparation-package work.

The full Development/Test Sandbox journey exists:

```text
product -> order -> checkout -> Sandbox completion -> Paid -> ExamAccessGrant -> authorized exam start
```

Production payment-provider selection remains intentionally deferred (company country, bank-account jurisdiction, and provider selection are not finalized). This deferral is preserved.

---

## Preparation Package Architecture Decisions

Decisions 1 through 10 (DA1–DA10) plus the reporting-profile transition are approved decisions and recorded in the approved umbrella specification at `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`.

Approved decisions summary:

- DA1 — Product and Package Model: standalone exam plus preparation package with one package-level price, scoped to one exam's Country and ExamCategory, one published mock exam included, no multi-product cart in v1.
- DA2 — Practice Question Bank: practice items managed independently and grouped into reusable immutable published Practice Collection versions; shared authoring-source implementation not required in v1; each practice item used by the included Practice Collection is assigned to exactly one Reporting Topic; separate runtime content from exam content; immediate feedback for practice only; exam answer keys never exposed through practice.
- DA3 — Study Materials: managed content library, immutable published versions; each material version may map to one or more Reporting Topics; materials reusable across packages; no interactive lessons or progress tracking in v1.
- DA4 — Attempts, Access Duration, Reports, Repurchase: one package access window; access begins immediately at successful fulfillment and runs for the configured duration; no first-use and no choice between fixed-length and rolling; one package-scoped attempt consumed atomically with qualifying session creation; one immutable report per qualifying session; report persists after expiry; active repurchase of the same stable package is blocked while entitlement is active; repurchase after expiry creates a new access window, attempt, and report right; package expiration after successful session creation does not invalidate the in-progress session (resume, submit, automatic finalization, and report qualification remain allowed without consuming an additional attempt).
- DA5 — Entitlement and Fulfillment Architecture: paid order item to idempotent fulfillment to package purchase entitlement to four independently authorizable benefit rights; `ExamAccessGrant` preserved as standalone authorization evidence; no free-text provenance.
- DA6 — Package Catalog and Lifecycle: stable package definition; immutable published package versions referencing one exact published exam version and one compatible immutable reporting-profile publication; sellable offer with price/currency; one active offer in v1; publication validation (reporting-topic taxonomy and reporting-profile publication are Stage 1 prerequisites for package-version publication); dedicated package administration permissions.
- DA7 — Practice-Bank Authoring: reusable immutable published Practice Collection versions; one Practice Collection version per package version in v1; each practice item mapped to exactly one Reporting Topic; basic practice progress required in v1; content-isolation checks at package publication.
- DA8 — Study-Material Authoring: managed material identity with immutable published versions; ordered list of specific material versions per package version; each material version may map to one or more Reporting Topics; no separate collection concept in v1.
- DA9 — Analytical Report and Reporting Taxonomy: exam-only diagnostic report in v1; one Reporting Topic per scored question required for package eligibility; topics scoped to ExamCategory; practice performance excluded from v1 report evidence while deterministic practice guidance remains supported via the practice-item-to-Reporting-Topic mapping; immutable generated report; separate immutable reporting-profile publication bound to one exact published exam version; report guidance restricted to the material versions and Practice Collection version included in the purchased Package Version; report must not expose question text, correct-answer identifiers, protected options, rationales, or per-question exam review; deterministic guidance is not an AI recommendation; report-generation failure does not invalidate scoring, does not permanently consume the report right, and does not require an exam retake.
- DA10 — Exam Access Source and Session Authorization: separate package-attempt start operation; explicit package purchase selection; atomic consumption; session provenance recorded; one in-progress session per nurse per exam version regardless of source; different-source conflict returns deterministic error.
- Reporting-profile transition: separate immutable reporting-profile publication bound to one exact published exam version; published exam version remains unchanged; reporting-topic taxonomy authoring/publication and the reporting-profile publication are Stage 1 prerequisites for package-version publication; concrete entity, table, properties, persistence, API contracts, and migration design are deliberately not approved at this stage.

### Coexistence And Independent Rights

- The same published exam may be sold standalone and included in a package. Both sales channels may coexist.
- Owning standalone access does not block purchasing the package, and purchasing the package does not block or invalidate standalone access.
- The package grants its own package-scoped attempt and report right, independent of any standalone grant.
- A standalone session does not consume or qualify the package attempt or report right.
- Different packages containing the same exam may coexist. One package cannot consume or satisfy another package's attempt or report right.
- Active repurchase of the same stable package is blocked while an entitlement for that package is active.
- Repurchase after the access window expires creates a new access window, a new attempt, and a new report right.

### Paid-Classification Boundary

- `ExamAccessGrant` is the nurse's proof of standalone authorization. It is not part of the rule determining whether an exam requires payment.
- The effective-paid classification rule is based on exam/payment-product state only.
- A package attempt right proves authorization for the explicit package start operation. It is not interchangeable with a standalone `ExamAccessGrant`.

### Current Authorized Task

The current authorized task is the documentation-only Dedicated Administration Permissions completion status update. Verification confirmed this Stage 1 runtime item is already satisfied by the existing Stage 1 runtime implementation and committed WebApi test coverage; no backend runtime code, tests, migrations, frontend/design files, staging, committing, pushing, or later-stage work is authorized by this update.

Known unrelated frontend/design worktree changes remain preserved and outside this Dedicated Administration Permissions completion update.

### Stage 1 Runtime Practice Progress Implementation Status

Stage 1 Runtime — Practice Progress is complete through seven slices:

- Slice 1 — domain model and domain tests.
- Slice 2 — Application contracts, DTOs, validators, and contract tests.
- Slice 3 — Application nurse-owned read/write workflow.
- Slice 4 — persistence configuration, EF migration, and configuration tests.
- Slice 5 — WebApi nurse-owned practice progress endpoints and integration tests.
- Slice 6 — integration, security, and compatibility verification.
- Slice 7 — documentation/status finalization.

Implemented Practice Progress capabilities:

- Nurse-owned package practice progress for one package purchase entitlement and its purchased practice collection version.
- Answered rows distinguish `AnsweredCorrect` and `AnsweredIncorrect`; `Unanswered` is derived from missing rows.
- Re-answering while active overwrites latest selected practice answer option, correctness state, and answered timestamp.
- Historical owner reads remain available after entitlement expiry.
- Writes require active entitlement access and active `PracticeAccess` authorization.
- Derived counters include total, answered, unanswered, correct, and incorrect counts; no aggregate counter table is stored.
- Practice progress is isolated from official exam sessions, official exam questions, official exam answer options, official snapshots, package exam attempt consumption, and Stage 4 analytical report evidence.
- Nurse-owned endpoints are available at `GET /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/practice-progress` and `POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/practice-progress/items/{practiceItemId}/answer`.
- No employer or admin practice-progress endpoints exist in v1.

Verified Practice Retry/Retraining Authorization status:

- Re-answer/retry while package access is active is allowed through the existing submit-answer workflow.
- Re-answering overwrites the latest answer, correctness state, and answered timestamp.
- Expired submit/re-answer/write attempts are rejected.
- Expired owners can still read historical practice progress.
- Practice retry/re-answer does not consume the package exam attempt.
- No retry history is stored in v1.
- No spaced repetition/retraining workflow is implemented in v1.
- No adaptive practice workflow is implemented in v1.
- No employer or admin practice-progress routes exist in v1.
- Stage 4 analytical reports do not read practice progress.

Verified Practice Exam-Content Isolation status:

- Practice runtime and package-publication isolation prevent practice content from reading, exposing, or revealing the included published Exam Version's protected question content, answer identifiers, options, explanations, rationales, answer keys, or snapshots.
- The item was completed by verification of existing implementation/test evidence, not by new implementation.
- Existing guard evidence includes `695cfbf test: guard practice item independence from exam content`.
- Relevant Practice Progress implementation evidence includes commits `a90074c`, `7f54e49`, `6c11e61`, `0454c0f`, and `cffc846`.
- Relevant status evidence includes commits `8b964ae` and `54c61a5`.
- Domain focused tests passed 62/62.
- Application focused tests passed 117/117.
- Infrastructure focused tests passed 42/42.
- WebApi focused tests passed 33/33.
- Build succeeded with 0 warnings and 0 errors.
- Stage 4 reports do not read `PracticeProgress`.
- Practice runtime/package-publication isolation grep found no defect and showed only existing guard tests, official exam/report paths, migrations/model snapshots, and unrelated payment/package snapshot references.

Verified Dedicated Administration Permissions status:

- Practice administration and material administration use dedicated permissions separate from existing exam-content permissions.
- Completion is based on existing implementation plus added WebApi test coverage in `17e38eb test: cover package administration dedicated permissions`.
- `StudyMaterials.Manage` and `PracticeCollections.Manage` are dedicated permissions.
- Dedicated permissions are seeded in reference data.
- Study material admin endpoints require `StudyMaterials.Manage`.
- Practice collection admin endpoints require `PracticeCollections.Manage`.
- `Exams.Edit` and `Questions.Manage` do not authorize study material or practice collection administration.
- WebApi focused permission tests passed 81/81.
- Infrastructure permission/seeding tests passed 11/11.
- Application focused tests passed 140/140.
- Build succeeded with 0 warnings and 0 errors.

Deferred Practice Progress items remain outside v1:

- retry history;
- spaced repetition or retraining algorithms;
- adaptive practice;
- workspace or dashboard aggregation;
- employer visibility;
- practice progress as analytical-report evidence;
- offline sync;
- progress export;
- cross-package progress merging.

### Stage 4 Implementation Status

Stage 4 — Package Analytical Reports is complete through seven slices:

- Slice 1 — domain report snapshot model and domain tests.
- Slice 2 — Application DTO, security, and behavior contracts.
- Slice 3 — Application lazy generation/direct access workflow.
- Slice 4 — persistence configuration, EF migration, and configuration tests.
- Slice 5 — WebApi direct nurse-owned report endpoint and integration tests.
- Slice 6 — PostgreSQL concurrency/recovery verification.
- Slice 7 — final verification and Stage 4 completion status update.

Implemented Stage 4 capabilities:

- Lazy/on-demand package analytical report generation.
- Direct nurse-owned endpoint: `GET /api/v1/me/nurse-profile/preparation-packages/exam-sessions/{sessionId}/report`.
- No report list endpoint in v1.
- Report generation only for finalized package attempt sessions.
- Existing reports are returned idempotently.
- Concurrent first requests converge to one report.
- Reports remain accessible after package entitlement expiry.
- Report evidence comes from exam session snapshots only.
- Practice progress is excluded from report evidence.
- Guidance references purchased study material versions and the purchased practice collection version.
- The report right remains dormant/unconsumed in v1.
- No protected question, answer, correct option, rationale, answer key, provider secret, token, password hash, internal authorization state, or internal benefit-right id exposure.

### Stage 3 Implementation Status

Stage 3 — Package Exam Attempt Sessions is complete through eight tasks:

- Task 1 — domain source, provenance, and attempt consumption model.
- Task 2 — persistence schema, EF configuration, migration, and data integrity tests.
- Task 3 — existing free/standalone start compatibility and source recording.
- Task 4 — package start application workflow with atomicity, idempotency, and conflicts.
- Task 5 — infrastructure PostgreSQL concurrency and rollback coverage.
- Task 6 — package exam-session WebApi endpoint, Problem Details codes, and response security.
- Task 7 — existing endpoint/API compatibility and scope guard tests.
- Task 8 — final verification and Stage 3 completion status update.

Implemented Stage 3 capabilities:

- `ExamSessionSource` values: `Legacy`, `Free`, `StandaloneGrant`, and `PackageAttempt`.
- Historical sessions are backfilled to `Legacy`; new application-created sessions do not use `Legacy`.
- Existing free starts record `Free`; existing standalone paid/grant starts record `StandaloneGrant`.
- Package exam-session Application workflow uses the selected package purchase entitlement and exact included exam version.
- Package session provenance is recorded in one-to-one `ExamSessionProvenance` rows.
- Package attempt rights are consumed atomically with qualifying session creation and record `ConsumedAt`.
- Package start persists session, provenance, snapshots, and attempt consumption in one transaction.
- PostgreSQL concurrency/idempotency coverage verifies one in-progress session per nurse/exam-version across sources.
- WebApi exposes only `POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session` for package exam start.
- Stable 409 Problem Details codes are implemented for `exam-session-source-conflict`, `package-attempt-consumed`, `package-entitlement-inactive`, `package-attempt-right-missing`, and `package-exam-version-unavailable`.
- Compatibility and scope guards protect existing free/standalone endpoint behavior and prevent Stage 4/report/workspace/employer route leakage.

### Stage 2 Implementation Status

Stage 2 — Package Fulfillment and Entitlements is complete through seven reviewed slices:

- Slice 1 — domain model for package order item source typing, purchased-offer snapshots, package purchase entitlements, and benefit rights.
- Slice 2 — payment order contracts for package offer order creation while preserving existing standalone product order behavior.
- Slice 3 — persistence and migration for package snapshots, entitlements, and benefit rights.
- Slice 4 — idempotent package fulfillment integrated with Sandbox payment completion.
- Slice 5 — Stage 2 package entitlement APIs and integration tests.
- Slice 6 — benefit-right authorization helpers and safe entitlement read models.
- Slice 7 — final compatibility, security, EF, and git hygiene verification.

Implemented Stage 2 package order/payment behavior:

- Authenticated nurse package offer order creation through the existing payment order endpoint.
- Development/Test Sandbox package payment completion through the existing Sandbox completion endpoint.
- Immutable purchased-offer snapshot usage for package fulfillment and read models.
- Package purchase entitlement creation after successful payment completion.
- Four benefit rights per entitlement: `MaterialsAccess`, `PracticeAccess`, `PackageExamAttemptEligibility`, and dormant `ReportEligibility`.
- Idempotent package fulfillment for repeated completion of the same paid package order item.
- Active same-package entitlement blocking based on the stable package definition.
- Different package definitions containing the same exam can coexist independently.

Preserved standalone behavior:

- Standalone exam access product completion still creates `ExamAccessGrant` authorization evidence.
- Package fulfillment does not create `ExamAccessGrant` rows and does not participate in standalone effective-paid classification.

Explicitly still deferred to later stages:

- Package exam start.
- Package attempt consumption.
- Exam session provenance.
- Report generation and report access.
- Package workspace runtime.
- Employer package data.

Next recommended work is Stage 3 planning/review only. Stage 3 implementation is not authorized until its staged specification and implementation plan are explicitly reviewed and approved.

---

## Completed Backend Capabilities

- [x] Payment products and immutable order snapshots.
- [x] Nurse-owned order create/list/detail/cancel behavior.
- [x] Checkout session foundation and lifecycle.
- [x] Provider-neutral checkout abstraction.
- [x] Sandbox provider available only in Development/Test.
- [x] Sandbox checkout initialization.
- [x] Development/Test-only Sandbox completion endpoint.
- [x] Atomic `PendingPayment` -> `Paid` transition.
- [x] Server-persisted `PaidAt`.
- [x] Transactional and idempotent `ExamAccessGrant` fulfillment.
- [x] PostgreSQL concurrency and rollback coverage.
- [x] Purchased exam access enforcement.
- [x] Effective paid rule: `Exam.IsFree == false` OR active positive-price `ExamAccess` product exists.
- [x] Exam catalog/detail `IsFree` and `CanStart` consistency.
- [x] Complete local Sandbox purchase-to-exam-start journey.
- [x] Preparation Package Stage 2 package offer order creation.
- [x] Preparation Package Stage 2 purchased-offer snapshots.
- [x] Preparation Package Stage 2 package payment completion through Sandbox.
- [x] Preparation Package Stage 2 package purchase entitlements and four benefit rights.
- [x] Preparation Package Stage 2 idempotent package fulfillment.
- [x] Preparation Package Stage 2 nurse-owned entitlement list/detail APIs.
- [x] Preparation Package Stage 2 benefit-right authorization/read models.
- [x] Preparation Package Stage 2 final compatibility, security, EF, and git hygiene verification.
- [x] Preparation Package Stage 3 exam-session source values and historical `Legacy` backfill.
- [x] Preparation Package Stage 3 package session provenance.
- [x] Preparation Package Stage 3 package attempt consumption with `ConsumedAt`.
- [x] Preparation Package Stage 3 package exam-session Application workflow.
- [x] Preparation Package Stage 3 package exam-session WebApi endpoint.
- [x] Preparation Package Stage 3 PostgreSQL concurrency/idempotency coverage.
- [x] Preparation Package Stage 3 compatibility and scope guards.
- [x] Preparation Package Stage 4 package analytical report domain model.
- [x] Preparation Package Stage 4 lazy/on-demand report generation workflow.
- [x] Preparation Package Stage 4 report persistence and uniqueness protection.
- [x] Preparation Package Stage 4 direct nurse-owned report endpoint.
- [x] Preparation Package Stage 4 PostgreSQL concurrency/recovery coverage.
- [x] Preparation Package Stage 1 Runtime practice progress domain model, Application workflow, persistence, and nurse-owned WebApi endpoints.

---

## Baseline Verification Snapshot (Backend Local MVP)

The following snapshot reflects the completed baseline at the time it was finalized. It is preserved as evidence of the compatibility baseline. The current task is the preparation-package umbrella approval-status closeout, not the baseline itself.

- Domain: 69 passed.
- Application: 434 passed.
- Infrastructure: 119 passed.
- WebApi: 252 passed.
- Total: 874 passed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes.
- PostgreSQL Sandbox tests: 6 passed, 0 skipped.

## Stage 2 Final Verification Snapshot

The following snapshot records the final Stage 2 Slice 7 verification evidence:

- Domain filtered tests: 66 passed, 0 failed.
- Application filtered tests: 100 passed, 0 failed.
- Infrastructure filtered tests: 59 passed, 0 failed.
- WebApi filtered tests: 72 passed, 0 failed.
- Full Application tests: 496 passed, 0 failed.
- Full WebApi tests: 284 passed, 0 failed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes.
- Final idempotent migration script generated at `/tmp/opencode/preparation-package-stage2-final.sql` with size 91141 bytes.
- Infrastructure migration/configuration diff: clean.
- Backend files: no remaining modified backend files after Stage 2 verification.
- Worktree: only known unrelated frontend/design changes remained.

## Stage 3 Final Verification Snapshot

The following snapshot records the final Stage 3 Task 8 verification evidence:

- Domain filtered tests: 21 passed, 0 failed.
- Application filtered tests: 97 passed, 0 failed.
- WebApi filtered tests: 35 passed, 0 failed.
- Infrastructure filtered tests with PostgreSQL connection string: 15 passed, 0 failed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes. The known design-time `HostAbortedException` appeared, and EF still completed the intended check with `No changes have been made to the model since the last migration.`
- Final idempotent migration script generated at `/tmp/opencode/preparation-package-stage3-final.sql` with size 100156 bytes.
- `git diff --check`: clean.
- Stage leakage grep: report/workspace/employer terms appear only in existing negative/scope tests and migration-name assertions.
- Legacy grep: `Legacy` appears only in negative tests/assertions in Application/WebApi test scopes.
- Sensitive/internal-field grep: matches are existing auth/payment/security code/tests and package endpoint negative forbidden-pattern assertions; Stage 3 public DTOs do not expose package benefit right ids or provenance internals.
- Worktree: only known unrelated frontend/design changes plus this Stage 3 status update remained after verification.

## Stage 4 Final Verification Snapshot

The following snapshot records the final Stage 4 Slice 7 verification evidence:

- WebApi package report tests: passed.
- Broader WebApi package tests: passed.
- Application package/session tests: passed.
- Infrastructure package report/configuration tests: passed.
- PostgreSQL package report/concurrency tests: passed.
- Domain report/right/session/provenance tests: passed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes. The known design-time `HostAbortedException` may appear when EF still completes the intended check with `No changes have been made to the model since the last migration.`
- Final idempotent migration script generated successfully.
- `git diff --check`: clean.
- Worktree: only known unrelated frontend/design changes plus this Stage 4 status update remained after verification.

## Practice Progress Final Verification Snapshot

The following snapshot records the final Practice Progress Slice 6 verification evidence:

- Domain full tests: 139 passed, 0 failed.
- Application full tests: 579 passed, 0 failed.
- Infrastructure focused practice-progress/configuration tests: 42 passed, 0 failed.
- Infrastructure non-PostgreSQL tests: 168 passed, 0 failed.
- WebApi full tests: 337 passed, 0 failed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes. The known design-time `HostAbortedException` appeared, and EF still completed the intended check with `No changes have been made to the model since the last migration.`
- Stage 4 compatibility: analytical report generator/query code does not read practice progress, and practice progress is not report classification evidence.

## Practice Retry/Retraining Authorization Verification Snapshot

The following snapshot records the Practice Retry/Retraining Authorization verification evidence. No new implementation was added for this item.

- Application focused PracticeProgress tests: 20 passed, 0 failed.
- WebApi focused PracticeProgress tests: 23 passed, 0 failed.
- Domain focused PackagePracticeProgress tests: 9 passed, 0 failed.
- Build: 0 warnings, 0 errors.
- Stage 4 compatibility: package analytical reports do not read practice progress.
- Attempt compatibility: practice retry/re-answer does not consume the package exam attempt.

## Practice Exam-Content Isolation Verification Snapshot

The following snapshot records the Practice Exam-Content Isolation verification evidence. No new implementation was added for this item.

- Domain focused tests: 62 passed, 0 failed.
- Application focused tests: 117 passed, 0 failed.
- Infrastructure focused tests: 42 passed, 0 failed.
- WebApi focused tests: 33 passed, 0 failed.
- Build: 0 warnings, 0 errors.
- Stage 4 compatibility: package analytical reports do not read `PracticeProgress`.
- Isolation grep: practice runtime/package-publication isolation grep found no defect.

## Dedicated Administration Permissions Verification Snapshot

The following snapshot records the Dedicated Administration Permissions verification evidence.

- WebApi focused permission tests: 81 passed, 0 failed.
- Infrastructure permission/seeding tests: 11 passed, 0 failed.
- Application focused tests: 140 passed, 0 failed.
- Build: 0 warnings, 0 errors.
- Dedicated permissions: `StudyMaterials.Manage` and `PracticeCollections.Manage`.
- Isolation: `Exams.Edit` and `Questions.Manage` do not authorize study material or practice collection administration.

---

## Deferred Work

The following work remains deferred relative to the baseline. Do not mark these items complete until they are explicitly implemented and verified. None of these are blockers for the current preparation-package umbrella approval-status closeout.

- Production payment provider.
- Production webhook/signature verification.
- Refunds and reconciliation.
- Production-grade object storage.
- Operational/production hardening.
- Frontend implementation.
- Preparation Package workspace runtime.
- Employer package purchase/report/practice-progress data.

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- TASKS.md
- README.md
- docs/product/vision.md
- docs/architecture/system-architecture.md
- docs/backend/backend-architecture.md
- docs/frontend/frontend-architecture.md
- docs/database/database-design.md
- docs/api/api-design.md
- docs/standards/engineering-standards.md
