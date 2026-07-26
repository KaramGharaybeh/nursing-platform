# Current Task

## Current Milestone

Preparation Package — Umbrella Architecture Accepted

Status:
The Preparation Package umbrella architecture-decisions specification is reviewed and approved. DA1–DA10 plus the reporting-profile transition remain approved decisions. This approval authorizes the recorded architecture decisions only; no preparation-package capability, staged specification, or implementation plan is approved or implemented by this milestone.

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

The current authorized task is the umbrella approval-status closeout only. The umbrella architecture-decisions specification has been reviewed and approved, and the authoritative documentation records the approved decisions, staged-specification boundaries, and accepted status.

No application code, tests, migrations, frontend files, or historical specifications have been modified.

No staged specification or implementation plan has been created or approved. The next possible work item is the Stage 1 — Content, Package Catalog, and Reporting Profile staged specification, but umbrella approval does not authorize that work; it requires a separate explicit instruction and review. The remaining staged specifications and all implementation plans likewise remain separate and unapproved.

Existing backend and frontend worktree changes remain preserved and outside this approval-status closeout.

### Implementation Status

No preparation-package capability is implemented today. The preparation package is in the documentation/specification phase. The completed backend MVP (standalone paid-exam access, free-exam access, grant-authorized session start, scoring, review, attempt history, and nurse-owned exam analytics) remains the compatibility baseline and is unchanged.

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

---

## Deferred Work

The following work remains deferred relative to the baseline. Do not mark these items complete until they are explicitly implemented and verified. None of these are blockers for the current preparation-package umbrella approval-status closeout.

- Production payment provider.
- Production webhook/signature verification.
- Refunds and reconciliation.
- Production-grade object storage.
- Operational/production hardening.
- Frontend implementation.

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
