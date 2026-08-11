# Current Task

## Current Milestone

Preparation Package — Stage 1 Runtime Catalog Completion Verification and Frontend Handoff Ready

Status:
Preparation Package Stage 1 Runtime — Catalog Completion Verification is complete and stopped for review. Verification confirmed published exam versions require a compatible published reporting profile and remaining publication requirements for package eligibility; dependent offers are excluded from new sales when a required component becomes ineligible without mutating historical package/purchase facts; dedicated package permissions protect administration; safe anonymous catalog APIs expose active eligible offers only; the Stage 1 EF migration is present; and full backend validation passed while preserving existing free, standalone paid-exam, payment, entitlement, package exam-session, package analytical report, and exam analytics behavior.

Practice Progress implementation and docs/status completion on branch `feature/preparation-package-foundation` are represented by commits `a90074c`, `7f54e49`, `6c11e61`, `0454c0f`, `cffc846`, and `8b964ae`. The Practice Retry/Retraining Authorization status finalization is represented by commit `54c61a5`. Practice exam-content isolation evidence includes existing guard commit `695cfbf`. Dedicated administration permissions test coverage is represented by commit `17e38eb`. Reporting-topic taxonomy implementation evidence is represented by `3f3cd66`. Reporting-profile publication evidence is represented by `7cd06e9` and `16a3f4e`. Study-material and V1 material-type implementation evidence is represented by `2789e76`, `2ecc0e1`, `5ce67c4`, `8ba4c2a`, `9a82ee7`, `b85059c`, and `17e38eb`. Material lifecycle guard and regression evidence is represented by `1f31e88` and `a4b28bb`; draft-content package-publication guard evidence is represented by `8b46581`; material-version reuse evidence is represented by `849a453`; offer-deactivation evidence is represented by `e95545c`; direct package-version component-persistence evidence is represented by `0381045`. This Package Definition Version Offer Lifecycle Docs Status Finalization is documentation-only and does not authorize runtime changes.

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

The current authorized task is the documentation-only Stage 1 runtime catalog completion verification and frontend handoff status update. Verification confirmed checklist items 306–311 are satisfied by existing Stage 1 runtime implementation and regression coverage; no backend runtime code, tests, migrations, frontend/design files, staging, committing, pushing, or later-stage work is authorized by this update.

Known unrelated frontend/design worktree changes remain preserved and outside this Package Version Composition Administration completion update.

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

Verified Reporting Topic Taxonomy status:

- Completion is based on verification of existing implementation and tests, not new implementation.
- Relevant implementation evidence is `3f3cd66 feat: lock reporting topics to exam category`.
- Every Reporting Topic requires one `ExamCategoryId`; updates cannot change that category.
- Country scope is inherited through the `ExamCategory` relationship.
- Application create/update validators reject empty category references; handlers reject missing categories.
- Persistence requires `ReportingTopics.ExamCategoryId`, uses a restrictive `ExamCategories` foreign key, and scopes unique name/slug indexes to the category.
- Reporting-topic endpoints require `ReportingTopics.Manage`.
- V1 has no global, exam-specific, cross-category, separate-skill, or separate-difficulty taxonomy.
- Difficulty-based report analysis remains outside v1.
- Domain focused tests passed 62/62.
- Application focused tests passed 117/117.
- Infrastructure focused tests passed 42/42.
- WebApi focused tests passed 81/81.
- Build retry succeeded with 0 warnings and 0 errors.

Verified Reporting Profile Publication status:

- Completion is based on existing implementation plus committed WebApi authorization coverage, not new implementation.
- Relevant evidence includes `7cd06e9 feat: enforce reporting profile publication rules` and `16a3f4e test: cover reporting profile dedicated permission`.
- `ReportingProfilePublication` is separate from `ExamVersion`, bound to one exact immutable `ExamVersionId`, and requires a published ExamVersion.
- Duplicate published profiles for the same ExamVersion are rejected.
- Published and retired profiles reject assignment changes.
- Assignments map active scored exam-version questions to Reporting Topics and reject missing, foreign-version, wrong-category, and inactive-topic cases.
- Reporting-profile operations read the bound ExamVersion and do not mutate it.
- Persistence requires restrictive exact ExamVersion and assignment relationships, with unique profile-name-per-version and question-assignment-per-profile constraints.
- Reporting-profile endpoints require `ReportingProfiles.Manage`; `Exams.Edit` and `Questions.Manage` do not authorize them.
- Domain focused tests passed 63/63.
- Application focused tests passed 118/118.
- Infrastructure non-PostgreSQL fallback tests passed 43/43; the PostgreSQL-only test was blocked only by missing `NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING`.
- WebApi focused tests passed 86/86.
- Build succeeded with 0 warnings and 0 errors.

Verified Study Material Authoring status:

- Completion is based on verification of existing implementation and tests, not new implementation.
- Relevant evidence includes `2789e76`, `2ecc0e1`, `5ce67c4`, `8ba4c2a`, `9a82ee7`, `b85059c`, and `17e38eb`.
- Study materials have stable managed identities; versioned content and Reporting Topic mappings are separate.
- Published and retired material versions are immutable; revisions create the next draft without mutating published content.
- Validators enforce material-type content requirements and at least one existing active Reporting Topic mapping.
- Persistence has restrictive material/version/topic relationships and unique material-version and version-topic constraints.
- Material endpoints require `StudyMaterials.Manage`; `Exams.Edit` and `Questions.Manage` do not authorize them.
- Domain focused tests passed 62/62.
- Application focused tests passed 117/117.
- Infrastructure focused tests passed 42/42.
- WebApi focused tests passed 86/86.
- Build succeeded with 0 warnings and 0 errors.
- No material delivery/download, storage-provider, entitlement, package-composition, or workspace behavior is included in this item.

Verified Material Lifecycle status:

- Completion is based on the existing implementation plus `1f31e88 fix: reject offers for retired package materials` and `a4b28bb test: cover material lifecycle guards`.
- Draft material versions are editable; published and retired material versions are immutable.
- Revisions create the next draft version without mutating published content.
- Package publication rejects draft and retired material versions; offer activation rejects package versions that reference retired or otherwise non-published material versions.
- Material retirement does not mutate published package versions, purchased snapshots, package purchase entitlements, or benefit rights; historical purchaser protections remain unchanged.
- Material lifecycle endpoints require `StudyMaterials.Manage`; focused WebApi coverage proves `401` unauthenticated, `403` without that permission, authorized success, and denial for `Exams.Edit` and `Questions.Manage` on lifecycle updates.
- Domain focused tests passed 65/65; Application focused tests passed 120/120; WebApi focused tests passed 97/97; Application full tests passed 582/582; WebApi full tests passed 363/363; build succeeded with 0 warnings and 0 errors.
- No storage, delivery, entitlement-access, workspace, frontend, or unrelated package behavior is introduced or marked complete by this item.

Verified Draft Content Package Publication Guard status:

- Completion is based on the existing publication validator and catalog-route scope plus `8b46581 test: cover draft package content guards`.
- Package publication rejects draft material versions and draft Practice Collection versions; only published material and practice collection versions can be included in a published Package Version.
- Draft material content is not exposed through public catalog routes, and no nurse material-content route exists: the attempted nurse path returns `404` for both anonymous and authenticated requests.
- Application focused tests passed 121/121; WebApi focused tests passed 99/99; Application full tests passed 583/583; WebApi full tests passed 365/365; build succeeded with 0 warnings and 0 errors.
- No material delivery/download, storage-provider, file-provider, offline-access, entitlement-access, workspace, frontend, or unrelated package behavior is introduced or marked complete by this item.

Verified Material Authorization Access-Window status:

- Completion is based on verification of the existing package entitlement and `MaterialsAccess` benefit-right implementation and tests, not new implementation.
- `MaterialsAccess` has the same start/end window as the package entitlement; authorization allows access only while both windows are active and rejects expired access.
- Persistence requires entitlement and benefit-right access-window fields.
- No nurse material delivery/download route exists; the existing file-storage service is used only by nurse CV workflows. No storage-provider, file-provider, offline-access, workspace, frontend, or unrelated package behavior is introduced or marked complete.
- Domain focused tests passed 65/65; Application focused tests passed 86/86; Infrastructure configuration tests passed 42/42; WebApi focused tests passed 58/58; build succeeded with 0 warnings and 0 errors.

Verified Materials Metadata Visibility Ordering Reuse status:

- Completion is based on the existing material authoring/publication implementation plus `849a453 test: cover material version reuse`.
- Study materials are stable managed records with title, slug, and optional description. Material versions carry type-specific content, reporting-topic mappings, and draft/published/retired publication status.
- One published material version is reusable by two distinct published package versions without content duplication or mutation.
- Package material references require unique positive sort orders and return deterministic ordering.
- Concrete metadata fields and visibility representation remain deferred beyond the approved identity, authoring, and publication rules.
- Application focused tests passed 122/122; Application full tests passed 584/584; Infrastructure focused tests passed 42/42; WebApi focused tests passed 99/99; build succeeded with 0 warnings and 0 errors.
- No storage-provider, delivery, download, file-provider, offline-access, workspace, frontend, entitlement-access, or unrelated package behavior is introduced or marked complete by this item.

Verified Package Definition Version Offer Lifecycle status:

- Completion is based on the existing package definition/version/offer implementation plus `e95545c test: cover package offer deactivation` and `0381045 test: cover package version component persistence`.
- Package definitions are stable country/category-scoped catalog identities. Published package versions preserve exact references and reject composition mutation.
- Offers carry only package definition/version references and commercial configuration; draft update, activation, deactivation, and the one-active-offer-per-definition rule are enforced.
- Deactivation transitions an active offer to inactive without mutating the package definition or published package-version content.
- Package/version endpoints use `PreparationPackages.View`, `PreparationPackages.Manage`, and `PreparationPackages.Publish`; offer endpoints use `PreparationPackageOffers.Manage`.
- Domain focused tests passed 68/68; Application focused tests passed 134/134; Infrastructure focused tests passed 44/44; WebApi focused tests passed 105/105; build succeeded with 0 warnings and 0 errors.
- No payment, entitlement, purchase, workspace, frontend, or unrelated package behavior is introduced or marked complete by this item.

Verified Package Version Composition Administration status:

- Completion is based on the existing version-composition, publication-validation, persistence, and authorized admin-route implementation plus `0381045 test: cover package version component persistence`.
- The create command persists the exact selected `ExamVersionId`, `ReportingProfilePublicationId`, `PracticeCollectionVersionId`, and positive uniquely ordered `StudyMaterialVersionId` references. It does not duplicate or mutate material or practice content.
- Publication validates that the exact referenced exam, reporting profile, practice collection, and materials are published, eligible, and compatible; the reporting profile must be bound to the exact selected exam version.
- Published package versions reject composition mutation. Package-version create/validation routes require `PreparationPackages.Manage` or `PreparationPackages.View`, and publication requires `PreparationPackages.Publish`.
- Domain focused tests passed 66/66; Application focused tests passed 124/124; Infrastructure focused tests passed 44/44; WebApi focused tests passed 102/102; build succeeded with 0 warnings and 0 errors.
- No offer/payment, entitlement, purchase, workspace, frontend, or unrelated package behavior is introduced or marked complete by this item.

Verified V1 Material Types status:

- Completion is based on verification of existing implementation and tests, not new implementation.
- Relevant evidence includes `2789e76`, `2ecc0e1`, `5ce67c4`, `8ba4c2a`, `9a82ee7`, `b85059c`, and `17e38eb`.
- `StudyMaterialType` contains exactly `File`, `ExternalLink`, `Video`, and `FormattedText`; unsupported enum values are rejected.
- Validator and domain rules require exactly one matching content field: `FileStorageKey`, `ExternalUrl`, `VideoUrl`, or `FormattedTextContent`, and reject incompatible combinations.
- Persistence stores the type and the four bounded type-specific fields; admin DTOs, mappings, and endpoints support all four types.
- Material administration requires `StudyMaterials.Manage`; WebApi tests cover `401` unauthenticated, `403` without permission, success with the dedicated permission, and denial for `Exams.Edit` and `Questions.Manage`.
- Domain focused tests passed 62/62; Application focused tests passed 117/117; Infrastructure focused tests passed 42/42; WebApi focused tests passed 86/86; build succeeded with 0 warnings and 0 errors.
- No storage-provider, upload/download/delivery, entitlement-access, package-composition, workspace, or frontend behavior is included in this item.

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

## Reporting Topic Taxonomy Verification Snapshot

The following snapshot records the Reporting Topic Taxonomy verification evidence. No new implementation was added for this item.

- Implementation evidence: `3f3cd66 feat: lock reporting topics to exam category`.
- Domain focused tests: 62 passed, 0 failed.
- Application focused tests: 117 passed, 0 failed.
- Infrastructure focused tests: 42 passed, 0 failed.
- WebApi focused tests: 81 passed, 0 failed.
- Build retry: 0 warnings, 0 errors.
- Taxonomy boundaries: required immutable `ExamCategoryId`, inherited country scope, and no global, exam-specific, cross-category, separate-skill, or separate-difficulty taxonomy.
- Difficulty-based report analysis: outside v1.

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
