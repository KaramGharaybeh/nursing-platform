# Preparation Package — Stage 1 Catalog and Authoring Specification

## 1. Status

Approved — Stage 1 Specification

This specification is a staged product and architecture specification only. It does not authorize implementation, implementation planning, source-code changes, database migrations, API implementation, tests, frontend screens, staging, committing, pushing, or beginning Stage 2, Stage 3, or Stage 4.

---

## 2. Purpose

Stage 1 defines the catalog and authoring foundation required before later Preparation Package commerce, fulfillment, entitlement, package-attempt, and analytical-report work can be specified or implemented.

The approved umbrella architecture established the Preparation Package as an additive paid product composed of managed study materials, an independent practice bank, one package-scoped mock exam attempt, and one analytical diagnostic report. Stage 1 translates only the catalog, authoring, package composition, reporting-topic, reporting-profile, package offer, and safe catalog/workspace concepts from that umbrella into a focused staged specification.

Stage 1 exists so later stages can rely on immutable published package composition and compatible reporting-topic/profile inputs without inventing content, catalog, or eligibility rules during fulfillment, attempt consumption, or report generation.

---

## 3. Authoritative Inputs

The authoritative inputs for this specification are:

- `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`, the approved umbrella architecture-decisions specification.
- Accepted conversation-only intake decisions recorded before this specification:
  - Package Workspace/Dashboard is accepted as a v1 user-facing concept.
  - Package workspace conceptually includes materials, practice, package exam attempt entry, report status/availability, and access-expiry information.
  - Employers cannot view package reports, package practice progress, or package purchase history in v1.
  - Future expansions such as AI study recommendations, subscriptions, employer-sponsored preparation, adaptive practice, and report-sharing controls must not be designed into v1 unless explicitly approved.
- Existing product and architecture documentation:
  - `docs/product/vision.md`
  - `docs/architecture/system-architecture.md`
  - `docs/backend/backend-architecture.md`
  - `docs/database/database-design.md`
  - `docs/api/api-design.md`
  - `PROJECT_RULES.md`
  - `CURRENT_TASK.md`
  - `TASKS.md`
- The model routing and approval-gate policy in `docs/development/model-orchestration.md`.

If this Stage 1 draft appears to conflict with the approved umbrella specification, the umbrella specification remains authoritative until the conflict is explicitly resolved by the user.

---

## 4. Stage 1 Scope

Stage 1 includes only the business and architecture rules for:

- Stable package definition.
- Immutable published package version.
- Package composition.
- Package offer and pricing concept.
- Material authoring model.
- Material published versions.
- Practice collection authoring model.
- Practice collection published versions.
- Reporting topic taxonomy.
- Reporting profile publication eligibility.
- Package catalog eligibility rules.
- Package administration responsibilities.
- Package workspace conceptual entry only, without fulfillment implementation.
- V1 boundaries and non-goals.

Stage 1 may settle conceptual lifecycle and eligibility rules needed to publish and safely sell a package offer later. Stage 1 must not define payment fulfillment, runtime entitlement enforcement, exam-session start behavior, or report generation mechanics.

---

## 5. Stage 1 Non-Scope

Stage 1 excludes Stage 2, Stage 3, Stage 4, and future roadmap implementation details.

Stage 1 does not specify implementation for:

- Payment order processing.
- Checkout or payment-provider behavior.
- Idempotent fulfillment mechanics.
- Entitlement persistence.
- Benefit-right consumption or runtime authorization hooks.
- Package exam session creation.
- Attempt consumption.
- Session concurrency.
- `ExamAccessGrant` compatibility changes beyond preserving the approved boundary.
- Report generation jobs.
- Report persistence implementation.
- Report access implementation.
- Frontend screen design.
- Employer access implementation.
- AI recommendations.
- Subscriptions.
- Adaptive practice.
- Employer-sponsored packages.
- Cohorts.
- Report-sharing controls.
- Marketplace, creator marketplace, affiliate system, or multi-vendor content.

Stage 1 does not define concrete database schemas, DTOs, endpoint paths, status codes, class names, migrations, test cases, or file-by-file implementation steps. Those remain subject to a separately authorized implementation plan after this staged specification is reviewed and approved.

---

## 6. Business Concepts

### Package Definition

A Package Definition is the stable catalog identity for a Preparation Package. It represents the business product identity that repurchase rules will later use to determine whether the same package is active for a nurse. It is scoped to one exam country/category context and is separate from any specific sellable offer.

### Published Package Version

A Published Package Version is an immutable frozen composition of one exact published exam version, one exact compatible reporting profile publication, one ordered set of exact published material versions, and one exact complete published practice collection version. It is the package content snapshot referenced by an offer.

### Package Offer

A Package Offer is the sellable catalog concept. It references one already-published package version and carries commercial configuration such as price, currency, and access duration. It does not independently select component content.

### Material

A Material is a stable managed content identity in the study-material library. It is not free text attached directly to a package.

### Material Version

A Material Version is a concrete revision of a material. Draft material versions may be edited. Published material versions are immutable and reusable across package versions.

### Practice Collection

A Practice Collection is the stable authoring identity for a reusable independent practice-bank collection. It is separate from the exam question bank.

### Practice Collection Version

A Practice Collection Version is an immutable published set of independent practice items and their immediate-feedback content. In v1, a package version references exactly one complete published practice collection version and does not directly select individual practice items.

### Reporting Topic

A Reporting Topic is a managed taxonomy item used for deterministic diagnostic classification and guidance. Each topic belongs to exactly one existing exam category, with country context inherited through the exam/category context.

### Reporting Profile Publication

A Reporting Profile Publication is a separate immutable publication bound to one exact published exam version. It records the reporting-topic assignment needed for package eligibility and later report generation. It does not mutate the published exam version.

### Package Catalog Eligibility

Package Catalog Eligibility is the rule set that determines whether a package version and offer may be safely published, activated, displayed as purchasable, and later sold. Eligibility requires compatible published components and safe catalog exposure.

### Package Workspace Concept

The Package Workspace is a v1 user-facing post-purchase concept that will eventually gather the package's materials access, practice access, package exam attempt entry, report status/availability, and access-expiry information. In Stage 1 it is conceptual only; runtime entitlement authorization, package-attempt launch, and report generation/access belong to later stages.

---

## 7. Package Definition and Versioning Rules

- A package definition is the stable identity for the preparation package and is the identity later used for same-package active-repurchase blocking.
- A package definition is scoped to one exam's country/category context. V1 packages are not cross-country, cross-category, or multi-exam bundles.
- A published package version is immutable after publication.
- A published package version references one exact published exam version. It does not mutate that exam version and does not copy protected exam question content into package content.
- A published package version references one exact compatible reporting profile publication bound to that exact published exam version.
- A published package version references an exact ordered list of published material versions.
- A published package version references exactly one complete published practice collection version.
- V1 package composition does not directly select individual practice items. Practice items are selected through the referenced complete practice collection version.
- V1 does not introduce a material collection abstraction. Ordered material versions are referenced directly by the package version.
- Package version publication requires validation that every referenced component is published, eligible, and compatible.
- Package version publication requires content-isolation validation that the referenced practice collection version does not leak protected exam content from the referenced exam version.
- Package version publication is a business action, not an engineering action. It uses already-approved capabilities and is performed through authorized administration, not through source-code deployment.
- Retirement or ineligibility of a referenced component must not mutate an already published package version.

---

## 8. Material Authoring Rules

- Materials have stable managed identities and independently versioned content.
- Draft material versions are editable and inaccessible to nurses.
- Published material versions are immutable.
- Revising published material creates a new material version rather than mutating the published version.
- V1 material types are File, External link, Video, and Formatted text.
- Published material versions may be reused across package versions.
- A package version includes material versions as an ordered list of exact published versions.
- Retired material versions cannot be included in newly published package versions or newly sellable offers.
- Historical buyers remain protected: retirement does not rewrite historical package composition, purchased snapshots, entitlements, or reports.
- V1 does not require per-material progress tracking.
- V1 does not include offline material access.
- V1 does not include interactive lessons, embedded quizzes, completion certificates, or progress-gated material unlocking.
- Material administration requires dedicated permissions separate from existing exam-content permissions.
- Material metadata and visibility rules should support safe catalog and authoring workflows, but concrete fields remain Stage 1 design detail for later implementation planning.

---

## 9. Practice Authoring Rules

- Practice collections have stable managed identities and independently versioned published collection versions.
- Published practice collection versions are immutable and reusable across package versions.
- Practice items are logically distinct from exam questions, even if future lower-level authoring infrastructure is shared.
- Practice items and exam questions are selected, published, accessed, and protected independently.
- Practice content must never reveal the included published exam version's protected question text, answer identifiers, options, explanations, rationales, answer keys, or exam snapshots.
- Practice content included in the same package version must not reveal the included mock exam.
- Practice immediate feedback and rationales may be shown only from independent practice items and their own approved feedback content.
- Every practice item included in a published practice collection version maps to exactly one reporting topic.
- Retry and retraining are allowed while the package access window is active. Runtime authorization for active access belongs to Stage 2 and later benefit-right enforcement.
- Practice retries never consume or affect the package exam attempt.
- Practice performance does not affect the official package exam score.
- Practice performance is excluded from v1 analytical report classification, although practice-topic mapping remains required for deterministic guidance.
- Basic practice progress distinguishes unanswered items from answered items. Answered progress further distinguishes correct answers from incorrect answers. Concrete persistence and counters are deferred to later implementation planning and must remain isolated from Stage 4 report classification evidence.
- After package expiry, no further practice authorization is granted. Historical practice progress may remain visible after expiry.
- Practice administration requires dedicated permissions separate from existing exam-content permissions.

---

## 10. Reporting Topics and Profile Eligibility

- Reporting topics are managed taxonomy items.
- Each reporting topic belongs to exactly one existing exam category.
- Country context is inherited through the package's exam/category context rather than through a separate cross-category or global topic model.
- V1 does not introduce global topics, exam-specific topic identities, cross-category topics, separate skill taxonomies, or separate difficulty taxonomies.
- Every scored exam question required for package eligibility maps to exactly one reporting topic through a compatible reporting profile publication.
- Every practice item included in the package's practice collection version maps to exactly one reporting topic.
- Each material version included in a package version maps to one or more reporting topics.
- Package eligibility requires a compatible reporting profile publication bound to the exact published exam version referenced by the package version.
- The reporting profile publication is separate from the published exam version and must not mutate it.
- The reporting profile publication is immutable once published.
- No mandatory mutation or backfill of historical exam versions is required. Exam versions without a compatible published reporting profile remain usable through existing standalone behavior but are not package-eligible.
- No AI guidance is included in v1.
- V1 report guidance is deterministic and may only use the purchased package's included material versions and practice collection version. Stage 1 supplies the topic mappings that later make deterministic guidance possible; Stage 4 defines report generation and guidance presentation.

---

## 11. Package Offer and Catalog Eligibility

- A package offer references one exact published package version.
- A package offer carries price and currency.
- A package offer carries access duration as a commercial/catalog configuration, while runtime access-window enforcement belongs to Stage 2 and later entitlement work.
- At most one active offer per package definition exists in v1.
- The offer does not independently select exam versions, reporting profiles, materials, practice collections, or reporting topics.
- The catalog exposes safe selling information only, such as title, country/category, included exam identity, material count or summary, practice scope, access duration, and price.
- The catalog must not expose protected exam questions, answer keys, protected answer identifiers, exam rationales, protected options, internal scoring logic, internal report logic, or implementation-only eligibility internals.
- Package availability and package-attempt eligibility are separate additive capability information. They do not participate in the standalone effective-paid classification rule.
- Package offers, package entitlements, package attempts, and package benefit rights do not participate in the existing standalone paid-classification rule: `Exam.IsFree == false OR active positive-price ExamAccess product exists`.
- The same published exam may be sold standalone and included in a package. Both sales channels may coexist for the same exam.
- Owning standalone access does not block purchasing the package, and purchasing the package does not block or invalidate standalone access.
- Different packages containing the same exam may coexist. One package cannot consume or satisfy another package's attempt or report right.
- Required components must be published and eligible before a package version can be published or an offer can be treated as sellable.
- Required components include the exact published exam version, compatible reporting profile publication, published material versions, published practice collection version, and active offer.
- If a required component is retired or otherwise becomes ineligible, new purchases of dependent offers must be blocked or made ineligible according to future catalog rules.
- Existing buyers remain protected when components are retired or made ineligible for new purchases.
- The exact block/ineligible mechanism is deferred until it is needed for Stage 1 implementation planning. The observable rule is that new sales are blocked while historical buyer rights are preserved.

---

## 12. Package Workspace Concept

- Package Workspace/Dashboard is accepted as a v1 user-facing concept.
- The workspace conceptually contains materials access, practice access, package exam attempt entry, report status/availability, and access-expiry information.
- The workspace aggregates the four package benefit rights: materials, practice, exam attempt, and report. Each eventual workspace access point is backed by an independently authorizable benefit right.
- Stage 1 records the workspace as a product and architecture concept only.
- Actual entitlement authorization for materials, practice, exam attempt, and report belongs to Stage 2.
- Package attempt launch and session provenance belong to Stage 3.
- Report generation, report persistence, and report access implementation belong to Stage 4.
- Stage 1 does not design frontend screens, wireframes, routes, components, or visual layouts.
- The workspace must not imply package attempt consumption or report generation before those later stages are specified and approved.

---

## 13. Admin Responsibility Boundaries

Stage 1 separates content/composition administration from offer/pricing administration.

Content and composition administration covers:

- Reporting topic taxonomy management.
- Reporting profile publication management.
- Material authoring and publication.
- Practice collection authoring and publication.
- Package definition management.
- Package version composition and publication validation.

Offer and pricing administration covers:

- Selecting one already-published package version for an offer.
- Setting offer price and currency.
- Setting offer access duration.
- Activating or deactivating sellable offers within the v1 one-active-offer-per-package rule.

Offer administration must not independently select component content or reporting topics. This specification does not design a full admin UI and does not settle concrete permission names, endpoint paths, or DTOs.

Package, material, practice, reporting-topic, and reporting-profile administration require dedicated permissions. Existing `Exams.*` permissions do not automatically cover package administration, material administration, practice administration, reporting-topic administration, reporting-profile administration, package-version publication, or offer/pricing administration. Concrete permission names remain deferred to the separately authorized Stage 1 implementation plan.

---

## 14. Security and Privacy Boundaries

- Backend authorization remains authoritative for all admin, catalog, workspace, material, practice, package attempt, and report access decisions.
- Employers cannot view package reports in v1.
- Employers cannot view package practice progress in v1.
- Employers cannot view package purchase history in v1.
- Stage 1 must not introduce employer-facing package report, practice-progress, or package-purchase-history access.
- Catalog responses must not expose protected exam content, answer keys, protected answer identifiers, protected options, exam rationales, internal scoring logic, internal report logic, raw tokens, secrets, or internal authorization state.
- Practice content must not leak protected exam content.
- Materials must not be used to expose protected exam content unless that content is independently approved as material content and does not reveal the included mock exam's protected questions, answers, or rationales.
- Report guidance in later stages must not expose question text, correct-answer identifiers, protected options, rationales, or per-question exam review.
- Draft material and draft practice content are not nurse-accessible.
- Existing standalone paid-exam and free-exam behavior remains protected and observably unchanged.
- Existing standalone `isFree` and `canStart` behavior remains backward compatible. Standalone session start, scoring, review, attempt history, and analytics remain observably unchanged for nurses who do not purchase a package.

---

## 15. V1 Business Boundaries

The first Preparation Package implementation excludes:

- Subscriptions.
- Carts.
- Upgrade paths.
- Partial refunds.
- Package sharing.
- Employer-sponsored packages.
- Cohorts.
- Team purchases.
- Marketplace, creator marketplace, affiliate system, or multi-vendor content.
- AI recommendations.
- Adaptive practice.
- Report sharing controls.
- Cross-country or cross-category packages.
- Multi-exam bundles.
- Buying benefits individually.
- Choosing or changing benefits at purchase time.
- Report guidance outside the purchased package's own materials and practice collection.

Future expansions must not be designed into v1 unless explicitly approved.

---

## 16. Later Stage Hand-off

Stage 2 receives from Stage 1:

- Package definitions and immutable package versions as approved catalog identities and composition snapshots.
- Package offers as sellable catalog concepts with price, currency, and access duration.
- Eligibility rules that identify when a package offer may be sold.
- Material and practice publication identities needed for benefit-right authorization.
- Workspace concepts that require entitlement-backed access data later.

Stage 3 receives from Stage 1:

- The exact published exam version referenced by the purchased package version.
- The package attempt entry concept from the workspace.
- The rule that standalone/free starts and package-attempt starts remain separate.
- The rule that package catalog and workspace surfaces must not silently consume package attempts.
- The later package-attempt design constraint that the system never silently selects an entitlement. The client selects the package purchase identity or entitlement, and the backend resolves the corresponding benefit right in the later authorized stage.
- The later session-concurrency design constraint that package exam attempt entry is subject to the one-in-progress-session-per-nurse-per-exam-version rule regardless of access source.

Stage 4 receives from Stage 1:

- Reporting topics.
- Compatible reporting profile publications bound to exact published exam versions.
- Material-version-to-topic mappings.
- Practice-item-to-topic mappings.
- The rule that deterministic guidance is restricted to the purchased package's material versions and practice collection version.
- The rule that practice performance is not v1 report classification evidence.

No later stage may treat this specification as authorization to implement without a separately reviewed implementation plan and explicit user authorization.

A package must not be commercially launchable until the later fulfillment, entitlement, package-attempt, and report capabilities needed for that launch have been specified, implemented, verified, and explicitly approved.

---

## 17. Open Questions and Deferred Details

No unresolved business questions block this draft specification.

The following details are deliberately deferred because they are implementation or later-stage concerns:

- Concrete entity names, table names, properties, indexes, constraints, and migrations.
- Concrete endpoint paths, DTOs, status codes, request/response shapes, and OpenAPI contracts.
- Concrete permission names and seed-data deltas.
- Concrete file storage, delivery, download, and file-provider policy for material files.
- Concrete retirement propagation mechanism, such as synchronous validation, events, background jobs, or recalculation.
- Concrete package catalog block/ineligible mechanism for retired or incompatible components.
- Concrete practice progress persistence and counters.
- Concrete workspace contract details and any fields whose semantics depend on Stage 2, Stage 3, or Stage 4.
- Payment order snapshots, fulfillment, entitlement persistence, benefit-right enforcement, package attempt start, session provenance, report generation, report persistence, and report access.

These deferred details must be settled only in the appropriate future staged specification or separately authorized implementation plan.

---

## 18. Acceptance Criteria for This Specification

This specification is acceptable for review when:

- It stays within Stage 1 catalog, authoring, package composition, reporting-topic, reporting-profile, package offer, safe catalog, and conceptual workspace scope.
- It preserves the approved umbrella architecture decisions.
- It captures the accepted intake decisions about package workspace, employer non-visibility, and v1 future-expansion boundaries.
- It does not authorize implementation, implementation planning, source-code changes, migrations, tests, frontend screens, staging, committing, pushing, or later stages.
- It does not define Stage 2 payment, fulfillment, entitlement, or benefit-right implementation details.
- It does not define Stage 3 package-attempt start, session provenance, concurrency, or `ExamAccessGrant` implementation details.
- It does not define Stage 4 report generation, report persistence, report access, or guidance implementation details.
- It preserves v1 business boundaries and excludes deferred roadmap features.
- It protects against protected exam-content leakage through catalog, practice, materials, workspace, or later report guidance.
- It preserves existing standalone paid-exam, free-exam, and grant-authorized session behavior as the compatibility baseline.
- It does not modify `CURRENT_TASK.md`, `TASKS.md`, `AGENTS.md`, `docs/index.md`, the approved umbrella specification, source code, tests, migrations, frontend/design files, or `.agent/goal-state.md`.
- It preserves existing unrelated frontend/design worktree changes exactly.
