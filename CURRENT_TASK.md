# Current Task

## Current Milestone

Preparation Package — Stage 2 Package Fulfillment and Entitlements Complete

Status:
Preparation Package Stage 2 — Package Fulfillment and Entitlements is implemented, verified, and stopped for review. Stage 2 extends the existing payment/order/checkout/Sandbox fulfillment path with package offer purchase, purchased-offer snapshots, package purchase entitlements, and benefit-right authorization/read models while preserving standalone paid-exam behavior.

Stage 2 verification completed on branch `feature/preparation-package-foundation` at `56de439 feat: add package benefit authorization read models`. Slice 7 was verification-only and produced no commit.

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

The current authorized task is the documentation-only Stage 2 completion status update. Stage 2 implementation and verification are complete; no code, tests, migrations, frontend/design files, staging, committing, pushing, or Stage 3 work is authorized by this update.

Known unrelated frontend/design worktree changes remain preserved and outside this backend Stage 2 completion update.

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

---

## Deferred Work

The following work remains deferred relative to the baseline. Do not mark these items complete until they are explicitly implemented and verified. None of these are blockers for the current preparation-package umbrella approval-status closeout.

- Production payment provider.
- Production webhook/signature verification.
- Refunds and reconciliation.
- Production-grade object storage.
- Operational/production hardening.
- Frontend implementation.
- Preparation Package Stage 3 package exam start, attempt consumption, and exam session provenance.
- Preparation Package Stage 4 report generation and report access.
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
