# Preparation Package — Architecture Decisions (Umbrella Specification)

> Status: Approved. The Preparation Package umbrella architecture-decisions specification is reviewed and approved. DA1–DA10 and the reporting-profile transition remain approved. This approval authorizes the recorded architecture decisions only; it does not approve concrete entities, tables, properties, persistence, APIs, DTOs, routes, status codes, migrations, implementation, staged specifications, or implementation plans. Stage 1 remains a separate future specification requiring explicit authorization and review.

## Purpose

This document is the umbrella architecture-decisions specification for introducing a paid preparation package product on the Nursing Platform. It records the approved business invariants, approved architectural directions, explicitly deferred business features, design details reserved for later staged specifications, and launch-time configuration decisions.

It is not a concrete implementation specification. It does not approve concrete entities, tables, properties, persistence, APIs, DTOs, routes, status codes, migrations, implementation, staged specifications, or implementation plans. Any illustrative names used in this document are not approved implementation names.

This specification supersedes nothing. It records approved decisions only and points to staged specifications that will be separately reviewed before any of those areas are implemented.

---

## Scope And Posture

### In Scope

This specification records the architecture decisions required to introduce a paid preparation package product alongside the existing standalone paid mock-exam product and the existing `ExamAccessGrant` entitlement model.

It covers:

- The approved product model and package composition.
- Practice question bank separation and reusable collection direction.
- Managed study material direction.
- Access window, attempts, reports, and repurchase rules.
- Entitlement and fulfillment direction.
- Package catalog, immutable package versions, and offer lifecycle.
- Reporting taxonomy and a separate immutable reporting-profile publication.
- Explicit package-attempt session-start intent.
- Legacy `ExamAccessGrant` compatibility.
- Purchased-offer snapshot invariant.
- Transition and package-eligibility posture.
- Staged specification boundaries.
- Decisions required only before first commercial package publication.
- Explicitly deferred features.

### Out Of Scope For This Specification

- Concrete table designs.
- Concrete endpoint paths.
- DTO definitions.
- Migration design.
- Implementation tasks.
- Source-code, test, migration, configuration, frontend, or historical-specification changes.
- Creation of the staged specifications or implementation plans. Their boundaries are recorded here, but they are written and reviewed separately.

### Implementation Status

No part of the preparation package feature is implemented today. The current platform supports:

- Standalone paid mock-exam access through `PaymentProduct` of type `ExamAccess`, nurse-owned `PaymentOrder`, Sandbox checkout completion, and transactional `ExamAccessGrant` fulfillment.
- Free and grant-authorized exam session start.
- Nurse-owned exam analytics derived from completed session scores.

The preparation package is currently in the documentation/specification phase. This umbrella architecture-decisions specification is reviewed and approved. The underlying DA1–DA10 business and architecture decisions plus the reporting-profile transition remain approved decisions. This approval authorizes the recorded architecture decisions only; no preparation-package capability is implemented. Staged specifications and implementation plans remain separate and unapproved. Stage 1 remains a future specification requiring separate explicit authorization and review.

### Terminology Note

The names `PreparationPackage`, `PackageVersion`, `PackageOffer`, `PracticeCollection`, `StudyMaterial`, `ReportingProfileVersion`, `PackagePurchaseEntitlement`, and any other illustrative names used below are not approved implementation names. They are used in this umbrella specification only to make the approved decisions readable. Each staged specification will settle concrete names for its area, and those names are subject to separate review.

The existing term `ExamAccessGrant` is an existing implementation name and is intentionally preserved as-is.

---

## How To Read This Document

Every decision area below is classified into one of five categories. A decision area may contain statements from more than one category.

| Category | Meaning |
|----------|---------|
| Approved business invariant | A business rule that staged specifications and implementation must preserve. Not subject to reinterpretation unless a future reviewer explicitly reopens it. |
| Approved architectural direction | A directional choice that constrains design. Concrete persistence, API, entity, and naming decisions remain to be settled in the staged specification for that area. |
| Deferred business feature | A feature that the first commercial package launch will deliberately not include. |
| Design detail to be settled in a staged specification | A decision that this umbrella intentionally does not make. It is reserved for a separately reviewed staged specification. |
| Launch-time configuration decision | A decision that does not have to be made in this umbrella or in a staged specification. It is made at package publication time, subject to the business rules recorded here. |

---

## 1. Approved Product Model

### Approved business invariants

- The platform will sell a paid preparation package as a single commercial product in addition to the existing standalone paid mock-exam product.
- A preparation package is scoped to one exam's `Country` and `ExamCategory`. It is not a cross-country or cross-category bundle.
- A package has one package-level price. The package does not expose separate item-level prices to the buyer in v1.
- A package includes exactly one published mock exam in v1. Multi-exam bundles are deferred.
- Each package grants four logical benefit rights: study materials, practice question bank, one exam attempt, and one analytical report.
- The existing standalone paid-exam product and its `ExamAccessGrant` fulfillment path remain available and unchanged in observable behavior.
- A nurse may purchase the same package again only after any active entitlement for that package has ended. Repurchase is blocked while an entitlement is active.
- No multi-product shopping cart exists in v1. A checkout is for a single package offer.

### Approved architectural direction

- The package domain is additive at the architectural level. It does not replace the existing `ExamAccess`, `PaymentProduct`, `PaymentOrder`, `ExamAccessGrant`, or standalone exam-session paths.
- Existing code will require backward-compatible modification in the areas enumerated in section 13. This umbrella does not approve how those changes are persisted.

### Deferred business features

- Multi-exam bundles inside one package.
- Cross-country or cross-category bundles.
- Subscriptions and recurring access.
- Coupons, discounts, taxes, invoices, wallets, payouts.
- Item-level prices exposed to the buyer.
- A shopping cart that holds multiple products.

### Design details to be settled in a staged specification

- Concrete entity names, properties, and relationships for the package product.
- Concrete persistence structure.
- Concrete catalog and admin API contracts.
- Concrete pricing representation details.

---

## 2. Package Composition

### Approved business invariants

- A package is composed of: one managed study-materials benefit, one practice question-bank benefit, one mock-exam-attempt benefit, and one analytical-report benefit.
- The included mock exam is one published mock exam.
- The exam attempt is package-scoped: one attempt per active entitlement, consumed atomically with the qualifying exam session creation.
- The analytical report is generated once per qualifying session and is immutable.
- The four benefits are independently authorizable benefit rights, even though they are sold together as one product.

### Approved architectural direction

- Benefits are modeled as logical rights granted by one entitlement, not as four separate purchasable products. The staged commerce, fulfillment, entitlements, and benefit-rights specification (Stage 2) will settle how this is persisted and authorized.

### Deferred business features

- Choosing which benefits a package includes at purchase time (cherry-picking benefits).
- Buying benefits individually.
- Adding or removing benefits after purchase.

### Design details to be settled in a staged specification

- How the four benefit rights are represented and authorized at runtime.
- How each benefit right is checked when its corresponding nurse action is requested.
- How the package composition is validated at publication.

---

## 3. Practice Question Bank — Separation And Reusable Collection Rules

### Approved business invariants

- Practice content is a separate runtime experience from exam content. A nurse practicing will never be served an exam answer key, and exam sessions will never be served practice content.
- Practice items are managed independently. They are grouped into reusable immutable published Practice Collection versions. One published Practice Collection may be referenced by more than one package version.
- A shared authoring-source implementation is not required in v1. Each published Practice Collection version is itself the immutable reusable unit.
- Each practice item used by the Practice Collection included in a v1 package is assigned to exactly one Reporting Topic. This mapping is required for deterministic guidance even though practice performance is excluded from the v1 report (see section 9).
- Practice provides immediate feedback. The exam experience does not provide immediate feedback before completion.
- Exam answer keys are never exposed through the practice experience. Content-isolation checks at package publication must prevent a Practice Collection from leaking exam-only material.
- Basic practice progress distinguishes unanswered items from answered items. Answered progress further distinguishes correct answers from incorrect answers.
- Retry and retraining are allowed while the package access window is active. Practice retries never consume the package exam attempt.
- After package expiry, no further practice authorization is granted. Historical practice progress may remain visible after expiry.
- Practice runtime must never read or expose the included published Exam Version's protected question content, correct-answer identifiers, options, explanations, rationales, answer keys, or exam snapshots.
- Practice content included in the same Package Version must not reveal the included mock exam.
- A Practice Item is logically distinct from `ExamQuestion`, even if a future implementation reuses lower-level authoring infrastructure. Practice Items and exam questions are selected, published, accessed, and protected independently.
- Practice administration requires dedicated permissions separate from existing exam-content permissions.

### Approved architectural direction

- One published Practice Collection version per package version in v1. The staged content and package-catalog specification (Stage 1) will settle concrete authoring, versioning, and publication rules.
- Concrete persistence, counters, and aggregation for the approved practice-progress states are deferred to the staged content and package-catalog specification (Stage 1).

### Deferred business features

- Practice-driven adaptive sequencing.
- Standalone, adaptive, or live-catalog practice recommendation behavior outside the approved deterministic report mapping.
- Practice leaderboard or social features.
- Practice evidence in the analytical report (see section 6; the topic mapping remains required in v1).

### Design details to be settled in a staged specification

- Concrete Practice Collection entity, version lifecycle, and publication rules.
- Concrete practice-item-to-Reporting-Topic mapping model.
- Concrete practice progress shape.
- Concrete progress counters and persistence.
- Concrete content-isolation enforcement at publication.
- Concrete immediate-feedback contract.
- Concrete practice-administration permission names.

---

## 4. Managed Study Materials

### Approved business invariants

- Study materials are managed content in a managed content library, not free-text attached to a package.
- Materials have an independent publication lifecycle from packages.
- Materials are reusable across packages: one published material may be referenced by more than one package version.
- A published package version references an ordered list of specific published material versions, not a live pointer to mutable material.
- Each material version may map to one or more Reporting Topics. This mapping is required for deterministic report guidance.
- Report guidance is restricted to the material versions and Practice Collection version included in the purchased Package Version. The report does not recommend unrestricted live-catalog content.
- No interactive lessons, no per-material progress tracking, and no progress-gated material unlocking exist in v1.
- The supported v1 material types are File, External link, Video, and Formatted text.
- Draft material versions are editable. Published material versions are immutable, and revising published content creates a new material version.
- Retired material versions cannot be included in newly published Package Versions or newly sellable offers. Historical purchasers remain protected.
- Material authorization ends when the package access window ends.
- Offline material access is not included in v1.
- Draft material content is inaccessible to nurses and cannot be included when publishing a Package Version. Only published material versions may be included in a published Package Version.
- Materials support managed metadata, publication visibility, ordering, and reuse.
- Material administration requires dedicated permissions separate from existing exam-content permissions.

### Approved architectural direction

- A managed material identity has immutable published versions. The staged content and package-catalog specification (Stage 1) will settle concrete entity and versioning rules.
- No separate material collection concept is introduced in v1. The package version references an ordered list of material versions directly.

### Deferred business features

- Interactive lessons and quizzes embedded inside materials.
- Per-material progress tracking and completion certificates.
- Standalone, adaptive, or live-catalog material recommendation behavior and adaptive ordering outside the approved deterministic report mapping.
- Material authoring workflows beyond managed publication.
- Report guidance that points outside the purchased Package Version's material and Practice Collection versions.

### Design details to be settled in a staged specification

- Concrete material entity and version lifecycle.
- Concrete material-version-to-Reporting-Topic mapping model.
- Concrete material ordering and packaging rules inside a package version.
- Concrete material access authorization.
- Concrete storage, delivery, download, and file-provider policy.
- Concrete material metadata fields, publication-visibility representation, and material-administration permission names.

---

## 5. Access Window, One Package-Scoped Attempt, Repurchase

### Approved business invariants

- One package access window applies per entitlement. Access begins immediately at successful fulfillment, not at purchase time and not at first use.
- There is no first-use activation, no separate benefit clocks, and no choice between "fixed-length" and "rolling from fulfillment." The exact duration of the window is configured for the package offer.
- One package-scoped attempt is included per entitlement.
- The attempt is consumed atomically with the qualifying exam session creation. A failed session start does not consume the attempt; a successful session start does.
- The package access window must be active when the package session is successfully created. Later expiration of the package does not invalidate the already-created in-progress session.
- The nurse may resume the same in-progress session after package expiration while the exam session itself remains valid under its server-owned timer.
- The session may be submitted or automatically finalized after package expiration.
- It remains eligible for the package report.
- No additional attempt is consumed.
- Exactly one immutable analytical report is produced per qualifying session. A qualifying session is a finalized `Submitted` or `Expired` session created from this package attempt.
- The report persists after the entitlement expires. The report is owned by the nurse and remains readable.
- Active repurchase of the same stable package is blocked while an entitlement for that package is active. After the access window expires, repurchase is allowed and creates a new access window, a new attempt, and a new report right.
- Existing grant-based standalone exam access is not affected by these rules.

### Approved architectural direction

- Access duration is the only launch-time configuration decision for the access window. It is recorded as the configured duration of the package offer.
- The attempt-consumption contract (atomic with qualifying session creation) is an architectural invariant. The staged package-attempt specification (Stage 3) will settle how this is enforced and how conflict between attempt-start paths is resolved.

### Deferred business features

- Multiple attempts per package entitlement.
- Pause and resume of the access window.
- Transfer of an unused attempt to another nurse.
- Extension of the access window after purchase without repurchase.
- First-use activation.
- Separate benefit clocks that run independently per benefit right.

### Design details to be settled in a staged specification

- Concrete access-window representation and enforcement.
- Concrete attempt consumption and conflict rules between standalone and package-start paths.
- Concrete qualifying session definition and report trigger.

---

## 6. One Immutable Analytical Report

### Approved business invariants

- In v1, the analytical report is exam-only and diagnostic. It is generated from the one qualifying exam session taken through this package attempt.
- Exactly one report per qualifying session is generated, and it is immutable once generated.
- The report is nurse-owned and remains readable after the entitlement expires.
- Practice evidence is excluded from the v1 report. The report is based on the qualifying exam session only.
- The report references one compatible immutable reporting-profile publication (see section 9). It is not computed ad hoc from mutable topic assignments.
- Report guidance is restricted to the material versions and Practice Collection version included in the purchased Package Version. The report does not recommend unrestricted live-catalog content.
- The report must not expose question text, correct-answer identifiers, protected options, rationales, or per-question exam review.
- Deterministic content guidance is not an AI recommendation. It is a deterministic mapping from the qualifying session's scored-question topic outcomes to the material versions and Practice Collection version included in the purchased Package Version.
- Successful exam finalization and persisted scoring do not depend on successful report generation. If report generation fails, the exam result and qualifying session remain final and valid.
- The report right is not permanently consumed by a failed generation. Report generation remains recoverable and retryable. The nurse is not required to retake the exam.
- Submitted package sessions and automatically finalized and scored package sessions qualify for the report. Abandoned sessions do not qualify.
- The report benefit right exists after fulfillment but remains dormant until a qualifying package session exists.
- A generated report is an immutable snapshot of package-purchase and session provenance, scored topic results, classification labels, and mapped purchased material and practice content.
- When topic evidence is insufficient, the report may display the available evidence but must not assign a confident weak, neutral, or strong classification.
- Only the owning nurse may access the report in v1. Admin report access is deferred.
- Employer, peer, cohort, predictive, difficulty-based, AI, and adaptive analysis are not included in v1.

### Approved architectural direction

- The staging and lifecycle of the report generation is reserved for the staged reporting specification (Stage 4). This umbrella approves only the diagnostic-exam-only direction, the immutability invariant, the generation-failure isolation from exam finalization, and the deterministic-guidance direction.
- This umbrella does not select a concrete job, queue, synchronous, or asynchronous mechanism for report generation. That decision is settled in the staged reporting specification (Stage 4).

### Deferred business features

- Reports that combine practice evidence with exam evidence.
- Comparative reports across multiple attempts.
- AI-generated, predictive, or adaptive recommendations and non-deterministic remediation plans beyond the approved deterministic purchased-content guidance.
- Downloadable or printable report formats beyond the API response.

### Design details to be settled in a staged specification

- Concrete report entity and lifecycle (draft, generated, archived).
- Concrete report content shape and topic-level breakdown.
- Concrete generation trigger, idempotency, and recovery/retry rules.
- Concrete access authorization and nurse ownership rules.
- Concrete job/queue/synchronous/asynchronous generation mechanism.

---

## 7. Entitlement And Fulfillment Direction

### Approved business invariants

- An order item paid for a package offer produces exactly one package purchase entitlement.
- One package purchase entitlement grants four independently authorizable benefit rights: materials, practice, exam attempt, and report.
- `ExamAccessGrant` is preserved for standalone paid-exam access. It is not repurposed as the package's exam-attempt right.
- Fulfillment is idempotent. Paying twice for the same order item does not produce two entitlements.
- No free-text provenance is accepted. Every entitlement is traceable to a paid order item and a specific package offer snapshot.

### Approved architectural direction

- The approved fulfillment flow is:

  ```text
  Paid Order Item
      -> Idempotent Fulfillment
          -> Package Purchase Entitlement
              -> Independently Authorizable Benefit Rights
                    - Materials
                    - Practice
                    - Exam Attempt
                    - Report
  ```

- Concrete persistence of this flow is reserved for the staged commerce, fulfillment, entitlements, and benefit-rights specification (Stage 2).

### Deferred business features

- Self-service grant management by nurses.
- Manual admin grant adjustments outside the paid-fulfillment path.
- Entitlement sharing or transfer between nurses.

### Design details to be settled in a staged specification

- Concrete entitlement entity, ownership, and lifecycle.
- Concrete benefit-right representation and authorization checks.
- Concrete idempotency and transactional boundaries of fulfillment.
- Concrete provenance fields and what is stored for audit traceability.

---

## 8. Package Definition, Immutable Version, And Offer Lifecycle

### Approved business invariants

- A package definition is stable catalog identity and business metadata (analogue of `Exam` for exams).
- Package composition is selected, validated, and frozen when an immutable Package Version is published. The Package Version references:
  - One exact published exam version.
  - One compatible immutable reporting-profile publication for that exam version.
  - An ordered list of specific published material versions.
  - One specific practice collection version.
- Only published material versions and published Practice Collection versions may be included in a published Package Version.
- A Package Offer is the sellable thing. It references one already-published Package Version and configures price, currency, and access duration.
- An offer does not independently select exam versions, reporting-profile publications, materials, Practice Collections, or Reporting Topics. The Reporting Topic set is inherited from the immutable reporting-profile publication referenced by the Package Version.
- Package composition administration and offer administration are separate responsibilities.
- One active offer per package in v1. Multiple simultaneous active offers for the same package are deferred.
- Publication validation enforces that a package version's composition is internally consistent: the referenced exam version, reporting-profile publication, material versions, and practice collection version are all published and compatible.
- Administration of packages uses dedicated package permissions. Existing `Exams.*` permissions do not automatically cover package administration. Concrete permission names are settled in the staged content, package-catalog, and reporting-profile specification (Stage 1).

### Approved architectural direction

- Package definition, immutable package version, and offer are distinct concepts. They are not collapsed into a single mutable product record. The staged content, package-catalog, and reporting-profile specification (Stage 1) will settle concrete entity names and lifecycle transitions.
- The published exam version remains unchanged when a package references it. A package version only references the existing published exam version; it never mutates it.

### Deferred business features

- Multiple active offers per package.
- Country or region-specific offers under one package.
- Time-limited promotional offers.
- Offer-level discounts or coupons.

### Launch-time configuration decisions

- For each package offer, the business chooses:
  - The exact already-published Package Version.
  - Price.
  - Currency.
  - Exact access duration of the access window (access always begins at successful fulfillment and runs for this configured duration; no first-use activation, no separate benefit clocks, and no choice between fixed-length and rolling).

### Design details to be settled in a staged specification

- Concrete package definition, version, and offer entities.
- Concrete publication lifecycle transitions.
- Concrete publication validation rules and conflict mapping.
- Concrete package-administration permission names and seed updates.

---

## 9. Reporting Taxonomy And Separate Reporting-Profile Publication

### Approved business invariants

- In v1, the reporting taxonomy assigns exactly one reporting topic to every scored question required for package eligibility.
- A practice item used by the Practice Collection included in a v1 package is assigned to exactly one Reporting Topic. This mapping is required for deterministic guidance even though practice performance is excluded from the v1 report.
- A material version included in a v1 package may map to one or more Reporting Topics. This mapping is required for deterministic guidance.
- Every Reporting Topic belongs to exactly one existing `ExamCategory`. Country scope is inherited through that `ExamCategory`.
- V1 does not introduce global Reporting Topics, exam-specific Reporting Topic identities, cross-category topics, a separate skill taxonomy, or a separate difficulty taxonomy.
- Difficulty-based analysis remains outside the v1 report.
- The generated report is immutable.
- A published package version references one compatible immutable reporting-profile publication for its referenced exam version.
- The reporting-profile publication preserves exactly one reporting-topic assignment for every scored question required for package eligibility.
- Reporting-topic taxonomy authoring/publication and the separate immutable reporting-profile publication bound to one exact published exam version are prerequisites for package-version publication eligibility. They are part of Stage 1, not Stage 4.

### Approved architectural direction

- The reporting-profile publication is separate from the immutable published exam version. A package version references both:
  - One exact published exam version (unchanged).
  - One compatible immutable reporting-profile publication for that exam version.
- The published exam version is never modified to carry reporting-topic assignments. The reporting profile is its own immutable publication.
- "Compatible" means the reporting-profile publication is bound to the exact published exam version referenced by the package version. Concrete compatibility rules are settled in the Stage 1 staged specification.
- This umbrella does not approve concrete entity names, table names, properties, persistence structure, API contracts, or migration design for the reporting profile. Any name used here (for example, `ReportingProfileVersion`) is illustrative only.

### Deferred business features

- Multi-topic assignment per question.
- Reporting topics that span multiple categories.
- Practice-evidence topics in the report.
- Admin-curated weak-area taxonomies outside the per-question assignment model.

### Design details to be settled in a staged specification

- Concrete reporting-profile entity and immutable version lifecycle.
- Concrete compatibility rule binding a reporting profile to one exact published exam version.
- Concrete reporting-topic entity and category-scoping rule.
- Concrete practice-item-to-Reporting-Topic and material-version-to-Reporting-Topic mapping models.
- Concrete Stage 1 publication validation and conflict mapping that enforces reporting-topic taxonomy and reporting-profile publication as prerequisites for package-version publication.

---

## 10. Explicit Package-Attempt Session-Start Intent

### Approved business invariants

- Starting a package-attempt exam session is a separate operation from starting a standalone or free exam session.
- The client selects the Package Purchase identity or entitlement, not an internal attempt-right identifier. The backend resolves the corresponding attempt right. The system never silently selects an entitlement.
- The attempt is consumed atomically with the qualifying session creation. A failed start does not consume the attempt.
- Successful package start validates nurse ownership, exact exam and exam-version match, an active package access window at session creation time, an unused attempt, and immutable purchase provenance.
- A same-source request when the qualifying session is already in progress returns or resumes that same session idempotently. An attempt consumed by that same in-progress session resumes the same session.
- An attempt consumed by a terminal session returns a deterministic consumed outcome.
- One in-progress session per nurse per exam version is allowed regardless of access source. A nurse cannot have two in-progress sessions for the same exam version at the same time, even if one is standalone and one is package-scoped.
- When a session-start request from a different access source conflicts with an existing in-progress session for the same exam version, the system returns a deterministic conflict. It does not silently replace or silently reuse the in-progress session.
- The standalone or free start operation never consumes a package attempt.
- No silent entitlement selection, access-source switching, or provenance rewriting is allowed.
- Every new exam session has one immutable logical source: free, standalone grant-authorized, or one specific Package Purchase attempt. Session source and provenance cannot be rewritten after creation.
- Existing `isFree` and `canStart` semantics remain backward compatible for free and standalone access. Package availability and package-attempt eligibility are separate additive capability information.
- Existing `canStart` must not silently include, select, or consume package rights.

### Approved architectural direction

- The package-attempt start operation records provenance: which package purchase entitlement and which package offer snapshot produced the session.
- Existing free and standalone session creation for new sessions is preserved in observable behavior. A new session that does not originate from a package attempt continues to behave as today.
- Concrete session provenance fields and conflict semantics are settled in the staged package-attempt authorization, session provenance, concurrency, and legacy-compatibility specification (Stage 3).

### Deferred business features

- Switching an in-progress session between access sources.

### Design details to be settled in a staged specification

- Concrete package-attempt start operation and request shape.
- Concrete provenance fields on the session.
- Concrete conflict semantics and error code for source-mismatch.
- Concrete concurrency enforcement for one in-progress session per nurse per exam version.
- Concrete package-capability response fields, endpoint paths, DTOs, and status codes.

---

## 11. Legacy `ExamAccessGrant` Compatibility And Paid-Classification Boundary

### Approved business invariants

- `ExamAccessGrant` is preserved for standalone paid-exam access. It proves standalone authorization for the nurse.
- The package exam-attempt right is a distinct right from a standalone `ExamAccessGrant`. A package attempt does not create a second `ExamAccessGrant` for the same nurse and exam.
- A package attempt right proves authorization for the explicit package start operation. It is not interchangeable with a standalone `ExamAccessGrant`.
- The existing unique permanent-grant index on `ExamAccessGrant` (one permanent grant per `NurseProfileId` + `ExamId`) is not violated by package fulfillment. A package attempt is not a permanent grant.
- The standalone effective-paid classification rule remains: `Exam.IsFree == false OR active positive-price ExamAccess product exists`.
- Package offers, package entitlements, package attempts, and package benefit rights do not participate in this standalone paid-classification rule.
- Package availability and package-attempt eligibility remain separate additive capability information.
- `ExamAccessGrant` remains standalone authorization evidence and is not a classification input.
- Existing standalone `isFree` and `canStart` behavior remains backward compatible. Standalone session start, scoring, review, attempt history, and analytics remain observably unchanged for nurses who do not purchase a package.

### Approved business invariants — Coexistence And Independent Rights

- The same published exam may be sold standalone and included in a package. Both sales channels may coexist for the same exam.
- Owning standalone access does not block purchasing the package, and purchasing the package does not block or invalidate standalone access.
- The package still grants its own package-scoped attempt and report right, independent of any standalone grant.
- A standalone session does not consume or qualify the package attempt or package report right.
- Different packages containing the same exam may coexist. One package cannot consume or satisfy another package's attempt or report right.
- Active repurchase of the same stable package is blocked while an entitlement for that package is active.
- Repurchase after expiration of the access window creates a new access window, a new attempt, and a new report right.

### Approved architectural direction

- Concrete changes to `ExamAccessGrant`, its index, and surrounding authorization are reserved for the staged package-attempt specification (Stage 3). This umbrella only approves the direction: preserve the grant for standalone access, do not overload it for package attempts, and do not include it in the effective-paid classification rule.

### Design details to be settled in a staged specification

- Whether `ExamAccessGrant` acquires any new field, status, or source discriminator.
- How the staged specification represents package-attempt authorization distinct from grant-based authorization.
- Concrete independent-right enforcement rules and conflict mapping for the cases enumerated above.
- Concrete package-capability response fields, routes, DTOs, status codes, and persistence, which remain deferred to Stage 3.

---

## 12. Purchased-Offer Snapshot Invariant

### Approved business invariants

- An order item for a package offer snapshots the offer's identity, package version reference, price, currency, and package display fields at order creation time.
- Later changes to package definitions, package versions, offers, materials, practice collections, or reporting profiles do not mutate existing order item snapshots or existing entitlements.
- The package purchase entitlement references the offer snapshot that produced it. The entitlement is traceable to a specific package version through the snapshot, not through a live pointer to a mutable product record.

### Approved architectural direction

- Concrete snapshot fields are settled in the staged commerce, fulfillment, entitlements, and benefit-rights specification (Stage 2). This umbrella approves only the invariant and the traceability direction.

### Deferred business features

- Snapshots with rehydration (re-linking an old order to a newer package version).
- Order item migration across package versions.

### Design details to be settled in a staged specification

- Concrete snapshot fields and shape.
- Concrete relationship between snapshot and entitlement.
- Concrete behavior when the source package version is retired after purchase.

---

## 13. Transition And Package-Eligibility Posture

### Approved architectural direction

- The new package domain is additive at the architectural level. At the same time, existing code will require backward-compatible modification. This umbrella does not claim that existing handlers, entities, DTOs, tests, or persistence will not change.
- Likely areas of backward-compatible modification include:
  - Exam-session authorization provenance.
  - Existing free and standalone session creation for new sessions (observable behavior preserved).
  - A new package-attempt start operation.
  - Payment completion and package fulfillment.
  - Purchased-offer snapshots.
  - Catalog and API additions.
  - Admin validation and publication.
  - Existing and new integration tests.
- This umbrella does not pre-approve how those changes are persisted.

### Approved business invariants

- Package eligibility requires a published package version referencing a published exam version, a compatible immutable reporting-profile publication, an ordered list of published material versions, and one published practice collection version.
- A package cannot be published if any referenced artifact is missing, draft, retired, or incompatible. Concrete conflict mapping is settled in the staged content, package-catalog, and reporting-profile specification (Stage 1).
- Existing standalone paid-exam eligibility and free-exam eligibility are unchanged. The standalone effective-paid classification rule remains: `Exam.IsFree == false OR active positive-price ExamAccess product exists`.
- Package offers, package entitlements, package attempts, and package benefit rights do not participate in this standalone paid-classification rule. Package availability and package-attempt eligibility remain separate additive capability information.
- `ExamAccessGrant` remains standalone authorization evidence and is not a classification input. Existing standalone `isFree` and `canStart` behavior remains backward compatible.
- If a required exam version, reporting-profile publication, Practice Collection version, or material version becomes retired or otherwise ineligible, dependent offers become non-purchasable for new sales.
- Existing purchased-offer snapshots, package entitlements, completed reports, and historically purchased composition remain unchanged.
- Retirement does not mutate an immutable published Package Version.
- Historical access remains subject to the purchased entitlement's original access window and benefit rules.
- Existing immutable published Exam Versions remain valid and unchanged.
- An existing published Exam Version becomes package-eligible only after a compatible immutable reporting profile is published and the remaining package-publication requirements are satisfied.
- No mandatory mutation or backfill of all historical Exam Versions is approved.
- Exam Versions without a compatible published reporting profile remain usable through existing standalone behavior but are not package-eligible.

### Deferred business features

- Migrating historical standalone exam purchases into package entitlements.
- Mixing a package attempt with a standalone grant to gain additional attempts.

### Design details to be settled in a staged specification

- Concrete eligibility check order.
- Concrete conflict responses for missing or incompatible referenced artifacts.
- Concrete package-capability response fields, routes, DTOs, status codes, and persistence, which remain deferred to Stage 3.
- Concrete retirement propagation, recalculation, event, job, and database mechanisms.
- Concrete migration or historical backfill mechanics.

---

## 14. Staged Specification Boundaries

### Approved architectural direction

The preparation package will be specified and implemented through staged specifications and separately reviewed implementation plans. This umbrella records their boundaries. It does not create them.

| Stage | Staged specification scope | Separately reviewed plan covers |
|-------|------------------------------|---------------------------------|
| Stage 1 — Content, Package Catalog, and Reporting Profile | Reporting-topic taxonomy authoring/publication rules (including one-topic-per-scored-question requirement and `ExamCategory` scoping); separate immutable reporting-profile publication bound to one exact published exam version; managed study-material authoring and immutable published versions; reusable practice collection authoring and immutable published versions (including practice-item-to-Reporting-Topic mapping and material-version-to-Reporting-Topic mapping); package definition, immutable package version, offer lifecycle; package publication validation (referenced exam version, reporting-profile publication, material versions, practice collection version); package catalog administration; package catalog APIs. | Implementation plan for Stage 1. |
| Stage 2 — Commerce, Fulfillment, Entitlements, and Benefit Rights | Package offer snapshotting; payment completion for package offers; idempotent package fulfillment; package purchase entitlement; four benefit rights; benefit-right authorization hooks. | Implementation plan for Stage 2. |
| Stage 3 — Package-Attempt Authorization, Session Provenance, Concurrency, Legacy Compatibility | Package-attempt start operation; provenance on sessions; one in-progress session per nurse per exam version; source-mismatch conflict semantics; `ExamAccessGrant` compatibility (kept out of effective-paid classification); free and standalone session preservation. | Implementation plan for Stage 3. |
| Stage 4 — Analytical-Report Generation and Access | Immutable analytical-report generation; generation recovery and retry; report persistence; nurse-owned report access; deterministic guidance presentation restricted to the material versions and Practice Collection version included in the purchased Package Version. Reporting-topic taxonomy authoring/publication and the separate immutable reporting-profile publication are NOT in Stage 4; they are Stage 1 prerequisites. | Implementation plan for Stage 4. |

Each staged specification is reviewed on its own. A staged specification may be revised before its implementation plan is approved.

### Design details to be settled in a staged specification

- Anything enumerated as such in sections 1 through 13.
- Anything that this umbrella deliberately does not name concretely.

---

## 15. Decisions Required Before First Commercial Package Publication

The configuration and classification inputs below apply to the first commercial package. Per-offer configuration items apply per offer; the launch-readiness items apply once and must be set before the first commercial package is published. No values are invented in this document.

### Per-offer configuration decisions

Made by the business at each package offer's publication time:

- Price of the package offer.
- Currency of the package offer.
- Exact access duration of the package offer (the only configurable element of the access window; access always begins at successful fulfillment and runs for this configured duration; no first-use and no choice between fixed-length and rolling).
- Exact already-published Package Version. Its exam version, reporting-profile publication, ordered material versions, Practice Collection version, and inherited Reporting Topic set were selected, validated, and frozen when that Package Version was published; the offer does not select them independently.

### Required launch-readiness inputs before publishing the first commercial package

These items apply once across the first examination market and must exist before the first commercial package can be published:

- Initial Reporting Topics for the first examination market.
- Minimum scored-question representation required before a topic can be classified confidently.
- Weak / neutral / strong classification thresholds used by deterministic report guidance.
- The specific eligible published Package Version for the first commercial offer. Its exact composition must already satisfy the Package Version publication requirements.

These inputs are recorded here as launch-time configuration, not as architectural decisions. They are subject to the eligibility rules in section 13.

### Approved business invariants

- The first commercial package cannot be published until all four staged specifications and their implementation plans have been reviewed, the underlying capabilities implemented, and the launch-readiness inputs above exist.
- A package publication is a business action, not an engineering action. It uses already-approved capabilities.

---

## 16. Explicitly Deferred Features

The following are explicitly deferred and are not part of the first commercial package launch.

- Multi-exam bundles inside one package.
- Cross-country or cross-category packages.
- Subscriptions and recurring access.
- Coupons, discounts, taxes, invoices, wallets, payouts, refunds.
- A multi-product shopping cart.
- Item-level prices exposed to the buyer.
- Choosing benefits at purchase time.
- Adding or removing benefits after purchase.
- Multiple attempts per package entitlement.
- Pause and resume of the access window.
- Transfer of an unused attempt to another nurse.
- Extension of the access window without repurchase.
- Reports that combine practice evidence with exam evidence.
- Comparative reports across multiple attempts.
- AI-generated, predictive, or adaptive report recommendations and non-deterministic remediation plans beyond the approved deterministic purchased-content guidance.
- Downloadable or printable report formats beyond the API response.
- Multi-topic assignment per question.
- Reporting topics that span multiple categories.
- Admin-curated weak-area taxonomies outside the per-question assignment model.
- Practice-driven adaptive sequencing.
- Standalone, adaptive, or live-catalog practice recommendation behavior outside the approved deterministic report mapping.
- Practice leaderboard or social features.
- Interactive lessons and quizzes embedded inside materials.
- Per-material progress tracking and completion certificates.
- Standalone, adaptive, or live-catalog material recommendation behavior and adaptive ordering outside the approved deterministic report mapping.
- Self-service grant management by nurses.
- Manual admin grant adjustments outside the paid-fulfillment path.
- Entitlement sharing or transfer between nurses.
- Multiple active offers per package.
- Country or region-specific offers under one package.
- Time-limited promotional offers.
- Offer-level discounts or coupons.
- Snapshots with rehydration across package versions.
- Order item migration across package versions.
- Migrating historical standalone exam purchases into package entitlements.
- Mixing a package attempt with a standalone grant to gain additional attempts.
- Switching an in-progress session between access sources.
- Frontend implementation of package screens (frontend design for package screens is intentionally paused until the staged backend specifications are reviewed).

This list is illustrative of the approved deferrals, not exhaustive. Anything not explicitly approved in sections 1 through 15 is deferred by default.

---

## Cross-References

This umbrella refers to the following existing implemented capabilities as preserved baselines:

- Standalone paid-exam access via `PaymentProduct` of type `ExamAccess`, `PaymentOrder`, `PaymentOrderItem` snapshots, Sandbox checkout, atomic `PendingPayment` to `Paid`, and transactional `ExamAccessGrant` fulfillment (see `docs/superpowers/specs/2026-07-12-payment-products-orders-foundation.md` and `docs/superpowers/specs/2026-07-13-payment-checkout-provider-abstraction.md`).
- Free and grant-authorized exam session start, scoring, review, and attempt history (see `docs/superpowers/specs/2026-07-12-exams-module.md`).
- Nurse-owned exam analytics derived from completed session scores (see `docs/superpowers/specs/2026-07-12-exam-analytics.md`).
- Effective paid-access rule: `Exam.IsFree == false` OR active positive-price `ExamAccess` product exists (see `docs/api/api-design.md`).

These references are evidence only. Those historical specifications are not modified by this task.

---

## Review Status

Preparation Package umbrella architecture-decisions specification reviewed and approved.

The underlying DA1–DA10 business and architecture decisions, including the reporting-profile transition, remain approved. Umbrella approval authorizes the architecture decisions recorded by this document only. It does not approve concrete entities, tables, properties, persistence, APIs, DTOs, routes, status codes, migrations, implementation, staged specifications, or implementation plans.

Staged specifications and implementation plans are not approved by this document. Stage 1 remains a separate future specification requiring explicit authorization and review before it is created.

No implementation, migration, source-code, test, frontend, configuration, or historical-specification change is authorized by this document.
