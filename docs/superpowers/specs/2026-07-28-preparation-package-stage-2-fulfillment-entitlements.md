# Preparation Package Stage 2 — Fulfillment, Entitlements, and Benefit Rights

## Status

Approved — Stage 2 Specification

This specification is a staged product and architecture specification only. It does not authorize implementation, implementation planning, source-code changes, database migrations, API implementation, tests, frontend screens, staging, committing, pushing, or beginning Stage 3 or Stage 4.

---

## Purpose

Stage 2 defines how a paid Preparation Package offer becomes a durable package purchase entitlement and a set of independently authorizable benefit rights after successful payment fulfillment.

Stage 1 produced the backend catalog, authoring, package composition, package offers, and safe public/admin endpoints. Stage 2 treats Stage 1 as the foundation and does not rework package definition, package version, offer, reporting-topic, reporting-profile, material, practice collection, or catalog behavior.

The goal of Stage 2 is to settle commerce, purchased-offer snapshot, entitlement, benefit-right, access-window, repurchase, failure, retry, and authorization boundaries so later stages can implement package attempts and reports without inventing fulfillment rules.

---

## Inputs and Dependencies

Authoritative inputs:

- `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`
- `docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md`
- `docs/superpowers/plans/2026-07-26-preparation-package-stage-1-catalog-authoring.md`
- Current Stage 1 implementation on branch `feature/preparation-package-foundation`, including commit `8aa5d50 docs(api): add preparation package openapi metadata`
- Existing payment/order/checkout/fulfillment implementation for standalone exam access
- Existing exam access implementation based on `ExamAccessGrant`
- `docs/product/vision.md`
- `docs/architecture/system-architecture.md`
- `docs/backend/backend-architecture.md`
- `docs/database/database-design.md`
- `docs/api/api-design.md`
- `PROJECT_RULES.md`
- `CURRENT_TASK.md`
- `TASKS.md`
- `docs/development/model-orchestration.md`

Stage 2 depends on Stage 1 entities and concepts already implemented:

- `PreparationPackageDefinition`
- `PreparationPackageVersion`
- `PreparationPackageOffer`
- `StudyMaterialVersion`
- `PracticeCollectionVersion`
- `ReportingProfilePublication`
- public package-offer catalog queries
- package offer activation/sellability rules

Stage 2 also depends on existing payment concepts:

- `PaymentProduct` currently supports standalone `ExamAccess` products only.
- `PaymentOrder` is nurse-owned and starts as `PendingPayment`.
- `PaymentOrderItem` stores immutable snapshots for the purchased item.
- Checkout start uses idempotency keys and request fingerprints.
- Sandbox completion atomically transitions an order to `Paid` and fulfills standalone `ExamAccessGrant` rows.

If this Stage 2 draft conflicts with the approved umbrella specification or approved Stage 1 specification, the approved specification remains authoritative until the conflict is explicitly resolved by the user.

---

## Stage 1 Baseline

Stage 1 is complete and is not reopened by this specification.

The Stage 1 baseline provides:

- Stable package definitions scoped to one country/category context.
- Immutable package versions referencing one exact published exam version, one compatible reporting-profile publication, one ordered list of material versions, and one practice collection version.
- Package offers carrying price, currency, access duration, catalog display fields, and lifecycle status.
- One-active-offer-per-package-definition behavior for v1.
- Reporting topics and reporting-profile publication compatibility rules.
- Material and practice authoring/publication foundations.
- Public catalog endpoints that expose safe active package offers only.
- Admin endpoints protected by dedicated Preparation Package permissions.
- OpenAPI metadata for Stage 1 endpoints.

Stage 2 must use these facts as inputs. Stage 2 must not revise Stage 1 catalog or authoring rules except by defining how a purchased offer is snapshotted and fulfilled.

---

## Scope

Stage 2 includes only:

1. Payment order item integration for package offers.
2. Purchased-offer snapshot rules.
3. Idempotent package fulfillment from paid order item to package purchase entitlement.
4. Package purchase entitlement model.
5. Benefit rights model.
6. Access window start and expiry rules.
7. Active entitlement rules.
8. Repurchase rules.
9. Historical buyer protection.
10. Authorization by entitlement and benefit rights, not live payment lookups.
11. Standalone exam compatibility.
12. Failure and retry behavior for payment completion and fulfillment.
13. Admin/support visibility boundaries.
14. Stage 2 API boundaries.
15. Stage 3 and Stage 4 handoff constraints.

Stage 2 may specify future commands, queries, DTO concepts, persistence invariants, and tests required for a later implementation plan, but this document does not authorize that plan or implementation.

---

## Non-Goals

Stage 2 does not specify or authorize:

- Package exam session creation.
- Package attempt consumption.
- Package purchase selection for session start.
- Exam session provenance fields.
- Package-vs-standalone source conflict behavior.
- Report generation.
- Report persistence.
- Report access implementation.
- Report retry behavior after a qualifying package session.
- Package workspace runtime.
- Material delivery/download runtime.
- Practice runtime or practice progress APIs.
- Employer access to package purchases, reports, or practice progress.
- Subscriptions.
- Carts.
- Sponsored packages.
- Package sharing.
- Adaptive practice.
- AI recommendations.
- Report sharing.
- Refunds, coupons, taxes, invoices, wallets, payouts, or production provider reconciliation.
- Frontend or design work.

Stage 2 must not create `ExamAccessGrant` rows for package purchases.

---

## Terminology

### Order

An Order is the nurse-owned payment aggregate representing a pending, paid, cancelled, expired, or failed commercial transaction. Existing implementation name: `PaymentOrder`.

### Order Item

An Order Item is one immutable purchased item snapshot inside an Order. For v1, checkout remains a single-product purchase, but the order/item separation remains important for idempotent fulfillment and traceability. Existing implementation name: `PaymentOrderItem`.

### Payment Product

A Payment Product is the existing standalone exam-access product record used by the current payment flow. It is not the same concept as a Preparation Package offer. Stage 2 may extend payment order creation to accept package offers, but package offers must not be forced into the existing `ExamAccess` semantics.

### Package Offer

A Package Offer is the Stage 1 sellable Preparation Package catalog concept. It references one already-published package version and carries price, currency, access duration, title, slug, and summary. It is the source of commercial facts for a package purchase.

### Package Version

A Package Version is the immutable Stage 1 composition snapshot referenced by a Package Offer. It identifies the exact published exam version, compatible reporting-profile publication, material versions, and practice collection version included in the package.

### Purchased-Offer Snapshot

A Purchased-Offer Snapshot is the immutable order-item-level record of what was bought at order creation time. It preserves package offer identity, package definition identity, package version identity, commercial facts, display facts, and included composition identities needed to fulfill and protect the purchase later.

### Package Entitlement

A Package Entitlement is the durable nurse-owned aggregate created by idempotent fulfillment of one paid package order item. It owns the access window and is the aggregate root for the purchase's benefit rights.

### Benefit Right

A Benefit Right is an independently authorizable right granted by a Package Entitlement. Stage 2 defines four right types: materials access, practice access, package exam attempt eligibility, and report eligibility/dormant report right.

---

## Core Model

The Stage 2 aggregate root is `PackagePurchaseEntitlement` or an equivalently named package purchase entitlement aggregate selected during implementation planning.

The entitlement aggregate owns:

- stable identity;
- owning nurse profile id;
- source payment order id;
- source payment order item id;
- purchased-offer snapshot id or embedded immutable snapshot data;
- preparation package definition id;
- preparation package version id;
- package offer id as purchased;
- access-window start and end;
- fulfillment timestamp;
- status;
- benefit rights;
- audit fields.

The entitlement aggregate is the authorization root for Stage 2 and later package benefit checks. Payment order and checkout records are financial history and fulfillment inputs. They are not queried live for benefit authorization.

Allowed mutable entitlement facts are limited to:

- status transitions required for lifecycle management, such as Active to Expired by computed status or persisted lifecycle update;
- fulfillment/retry diagnostics that do not change purchased facts;
- future Stage 3/4 references added by separately approved specifications, such as attempt/session/report linkage.

Immutable entitlement facts include:

- nurse owner;
- source order and order item;
- package definition;
- package version;
- purchased offer;
- purchased price/currency;
- purchased access duration;
- access-window start;
- access-window end;
- included exam version;
- included reporting-profile publication;
- included material version ids and ordering;
- included practice collection version id.

---

## Payment Integration

Stage 2 extends the payment order creation and fulfillment path additively.

The existing standalone path remains:

```text
PaymentProduct(ExamAccess)
    -> PaymentOrderItem snapshot
        -> Checkout
            -> Paid order
                -> ExamAccessGrant fulfillment
```

The package path becomes:

```text
PreparationPackageOffer
    -> Package order item purchased-offer snapshot
        -> Checkout
            -> Paid order
                -> Idempotent package fulfillment
                    -> Package purchase entitlement
                        -> Benefit rights
```

Payment order integration must preserve these rules:

- The server owns nurse identity, price, currency, package offer selection, package version selection, access duration, and package composition facts.
- Clients must not supply price, currency, access duration, nurse id, package version id, entitlement id, right ids, or fulfillment status.
- Package offer purchase requires the offer to be active and sellable at order creation time.
- A payment order item for a package offer must carry a package-specific snapshot rather than relying on live offer data during fulfillment.
- Existing `PaymentProductType.ExamAccess` behavior remains backward compatible.
- Package offers must not participate in standalone exam paid classification.

Stage 2 selects the second strategy: generalize order item source typing while preserving the existing standalone product path.

Required direction:

- `PaymentOrder` remains the order aggregate.
- `PaymentOrderItem` remains the order-item concept.
- Order items gain a source discriminator with Stage 2 meanings equivalent to `ExamAccessProduct` and `PreparationPackageOffer`.
- Existing standalone fields and behavior remain valid for `ExamAccessProduct` items.
- Package items store package-offer snapshot facts either directly on package-specific nullable order-item fields or in a one-to-one package order-item snapshot table owned by the order item.
- `PaymentProductType.ExamAccess`, `ExamIdSnapshot`, and `ExamAccessGrant` semantics must not be overloaded to represent package purchases.
- The package order creation API uses package offer identity as input and creates a package-sourced order item with a purchased-offer snapshot.

The implementation plan must not collapse package offers into standalone exam-access products if that would overload `ExamIdSnapshot`, `PaymentProductType.ExamAccess`, or `ExamAccessGrant` semantics.

---

## Purchased-Offer Snapshot

At order creation time, the package order item must snapshot all facts needed to fulfill, authorize, audit, and protect the buyer even if live catalog data later changes.

Required immutable snapshot facts:

- source package offer id;
- package offer title;
- package offer slug;
- package offer summary or display description used at purchase time;
- package definition id;
- package definition title;
- package definition slug;
- country id;
- exam category id;
- package version id;
- package version number or stable display version;
- included exam id;
- included exam version id;
- included exam title;
- included reporting-profile publication id;
- included practice collection version id;
- included study material version ids in purchased order;
- price amount minor;
- currency;
- access duration days;
- order creation timestamp.

Material count and practice item count are not required immutable snapshot facts. They may be computed for display from the purchased package version and referenced component identities, provided doing so does not read mutable live composition in a way that changes historical purchase meaning.

The snapshot must not include protected exam question text, answer identifiers, correct answers, answer keys, rationales, protected options, internal scoring logic, raw report logic, provider secrets, tokens, or internal authorization state.

The entitlement references the purchased-offer snapshot. Fulfillment must not re-resolve mutable package composition from the live offer after payment completes except to verify identity consistency and detect corruption.

---

## Fulfillment Flow

Package fulfillment starts only after a payment order is successfully marked paid by an authorized payment completion path.

Required flow:

```text
Paid package order item
    -> load immutable purchased-offer snapshot
    -> verify nurse ownership and order/item consistency
    -> verify idempotency by source order item
    -> block duplicate active same-package entitlement for new order creation or fulfillment
    -> create one package purchase entitlement
    -> create four benefit rights
    -> persist all changes transactionally
    -> return completion result without exposing internal right identifiers unless explicitly required by a later approved API
```

Stage 2 v1 fulfillment must be atomic with the paid transition for the existing Development/Test Sandbox completion path, matching the current standalone payment completion posture. The paid transition, package entitlement creation, and benefit-right creation must commit or roll back together when handled by the same command.

The fulfillment logic must also be written as an idempotent order-item-based operation so a future production webhook or recovery worker can retry fulfillment for an already-paid order item without creating duplicates. This retryability requirement does not relax the Stage 2 v1 atomic Sandbox transaction requirement.

If a payment order contains both standalone and package items in a future extension, fulfillment must be item-based and independent per order item. V1 does not introduce carts or multi-product checkout, so this is a schema/handler robustness constraint only, not a cart feature.

---

## Idempotency Rules

The idempotency key for package fulfillment is the source payment order item id plus the package item source type.

Checkout idempotency and fulfillment idempotency are separate and compose as follows:

- Checkout idempotency uses the existing checkout session idempotency key and request fingerprint to prevent duplicate checkout initialization for the same nurse/order request.
- Fulfillment idempotency uses the paid package order item id and source discriminator to prevent duplicate entitlements and duplicate rights after payment succeeds.
- Reusing a checkout idempotency key must never be treated as proof that fulfillment occurred.
- Re-running fulfillment must never create a second entitlement for the same package order item.

The uniqueness invariant is:

```text
At most one package purchase entitlement may exist for one package order item.
```

Required idempotency behavior:

- Re-running fulfillment for the same paid package order item returns the existing entitlement outcome.
- Re-running payment completion after the order is already `Paid` returns the existing paid and fulfilled outcome when entitlement and rights are complete.
- If the entitlement exists and all required benefit rights exist with matching snapshot facts, fulfillment is complete.
- If the entitlement exists but one or more required benefit rights are missing, fulfillment must either repair the missing rights idempotently in the same transaction or fail with a deterministic unsafe-fulfillment error that can be corrected by support/admin tooling in a later approved scope.
- If the entitlement exists but purchased snapshot facts mismatch the source order item, fulfillment must stop with a deterministic unsafe-fulfillment error. It must not mutate immutable purchased facts to force a match.
- If another transaction wins entitlement creation for the same order item, the losing transaction must reload and return the converged existing entitlement outcome when facts match.
- Duplicate entitlement creation must be prevented by a database uniqueness constraint in addition to application checks.

Payment checkout idempotency remains separate from fulfillment idempotency. Checkout idempotency uses checkout request keys and fingerprints. Fulfillment idempotency uses paid order-item provenance.

---

## Entitlement Model

The package entitlement represents a single purchase of a package definition by a nurse.

Required entitlement fields conceptually include:

- `Id`
- `NurseProfileId`
- `PaymentOrderId`
- `PaymentOrderItemId`
- `PurchasedOfferSnapshotId` or embedded snapshot reference
- `PreparationPackageDefinitionId`
- `PreparationPackageVersionId`
- `PreparationPackageOfferId`
- `Status`
- `FulfilledAt`
- `AccessStartsAt`
- `AccessEndsAt`
- `CreatedAt`
- `UpdatedAt`

Persisted entitlement statuses:

- `Active`
- `Expired`
- `Revoked` only if a separately approved support/refund/revocation policy later exists

For Stage 2 v1, `Status` is persisted and must be consistent with the access window. Order creation and fulfillment paths must treat stale active entitlements whose `AccessEndsAt <= now` as expired before applying same-package repurchase checks. An entitlement is active for authorization only when the nurse owns it, `Status == Active`, and `AccessStartsAt <= now < AccessEndsAt`.

The entitlement must be nurse-owned. Employer, peer, public, and cross-nurse access is forbidden.

---

## Benefit Rights Model

Benefit rights should be represented as separate persisted child rows/entities of the entitlement, not only as computed value objects.

Rationale:

- Stage 3 must atomically consume the exam-attempt eligibility right with session creation.
- Stage 4 must track report eligibility/dormancy, generated report linkage, and retry-safe report lifecycle without changing purchase facts.
- Independent rows make authorization, status, and audit checks explicit.
- Database constraints can enforce exactly one right of each type per entitlement.

Required benefit right types:

1. `MaterialsAccess`
2. `PracticeAccess`
3. `PackageExamAttemptEligibility`
4. `ReportEligibility`

The right type must be represented by a stable discriminator equivalent to a `BenefitRightType` enum with exactly those four values for Stage 2.

Each package entitlement must have exactly one right of each type.

Persisted benefit right statuses for Stage 2:

- `Available` — the right exists and may authorize its corresponding action when the entitlement access-window rule also allows it.
- `Dormant` — the right exists but is waiting for a later qualifying condition; Stage 2 uses this for the report right.
- `Consumed` — reserved for the Stage 3 package exam attempt right after successful qualifying session creation.
- `Expired` — the right can no longer authorize access-window-limited actions.
- `Revoked` — reserved for a separately approved support/refund/revocation policy.

Initial right statuses at fulfillment:

- `MaterialsAccess`: `Available`
- `PracticeAccess`: `Available`
- `PackageExamAttemptEligibility`: `Available`
- `ReportEligibility`: `Dormant`

Stage 2 may write only `Available`, `Dormant`, and `Expired` during normal package fulfillment and access-window evaluation. `Consumed` is reserved for Stage 3 attempt consumption. `Revoked` is reserved for a separately approved support/refund/revocation policy.

Common right facts:

- stable right id;
- entitlement id;
- right type;
- status;
- access starts at;
- access ends at, when applicable;
- created at;
- updated at.

Materials and practice rights authorize access only while the entitlement access window is active.

The package exam attempt eligibility right authorizes a later Stage 3 package-attempt start only while the entitlement access window is active and the attempt is unused. Stage 2 must not consume it.

The report eligibility right exists immediately after fulfillment but is dormant until a qualifying package exam session exists. Stage 2 must not generate, persist, or expose reports.

Internal benefit right identifiers should not be selected by clients for package-attempt start. Stage 3 receives the package purchase entitlement identity and resolves the internal attempt right.

---

## Access Window Rules

The access window starts at successful package fulfillment.

`AccessStartsAt` is the server-owned timestamp at which the package entitlement and benefit rights are successfully fulfilled, not the checkout start time, not the order creation time, not the provider authorization time, and not first use.

For the Stage 2 v1 Sandbox completion path, `AccessStartsAt` must equal the persisted `PaymentOrder.PaidAt` because the paid transition and fulfillment commit atomically. If a future production provider separates paid marking from fulfillment retry, `AccessStartsAt` remains the fulfillment timestamp, and any difference from `PaidAt` must be intentional, persisted, and test-covered.

The access duration unit for Stage 2 v1 is whole days, copied from the purchased package offer snapshot. `AccessEndsAt` is calculated as:

```text
AccessStartsAt + TimeSpan.FromDays(purchased access duration days)
```

The duration comes from the purchased-offer snapshot, not the live package offer.

Rules:

- One package access window applies to all access-window-limited benefits.
- There is no first-use activation.
- There are no separate clocks per benefit.
- Materials access ends when the access window ends.
- Practice access ends when the access window ends.
- Package exam attempt start eligibility requires an active access window at session creation time.
- If Stage 3 later creates a package exam session before expiry, later package expiry does not invalidate that already-created in-progress session.
- Report eligibility is dormant after fulfillment and not made impossible merely because no qualifying session exists yet.

Stage 2 must not expose runtime workspace behavior based on the access window. It may define entitlement/read models for future APIs only within Stage 2 boundaries.

---

## Repurchase Rules

Same-package repurchase is based on the stable `PreparationPackageDefinitionId`.

Rules:

- A nurse cannot create or fulfill a new package purchase for the same package definition while that nurse has an active entitlement for that package definition.
- The check must not use package offer id or package version id as the same-package identity because offers and versions can change while the stable package definition remains the same product.
- Repurchase after the access window expires is allowed.
- Repurchase after expiry creates a separate order, separate purchased-offer snapshot, separate entitlement, separate access window, separate package exam attempt eligibility right, and separate dormant report eligibility right.
- Different packages using the same exam are independent purchases and may coexist.
- Owning standalone exam access does not block purchasing a package.
- Owning a package does not block purchasing standalone exam access.

The active-repurchase block must be enforced before payment when creating the package order. Fulfillment must also re-check and enforce the invariant transactionally to protect against races.

Concurrency control requirement:

- Persisted active entitlements must be protected by a database uniqueness invariant equivalent to one `Active` entitlement per `(NurseProfileId, PreparationPackageDefinitionId)`.
- Before creating a new package order, the application must expire stale active entitlements for the same nurse/package whose `AccessEndsAt <= now`, then check for a remaining active entitlement.
- During fulfillment, the same invariant must be enforced inside the transaction. If another active entitlement exists for a different order item, fulfillment must fail with a deterministic conflict rather than creating a second active entitlement.

If two concurrent package order creations or completions attempt to create active same-package entitlements, only one may succeed. The other must receive a deterministic conflict or converge to its own already-existing fulfilled entitlement only if it is retrying the same order item.

---

## Historical Buyer Protection

Historical buyers remain protected after purchase.

After fulfillment, the buyer's entitlement and purchased-offer snapshot must not be mutated when any of these live records later change or retire:

- package definition;
- package version;
- package offer;
- material;
- material version;
- practice collection;
- practice collection version;
- reporting topic;
- reporting-profile publication;
- exam version.

Historical protection means:

- The entitlement continues to authorize the purchased material versions and practice collection version during its original access window.
- The package exam attempt eligibility remains tied to the purchased exam version and purchased package version.
- The dormant report right remains tied to the purchased reporting-profile publication and purchased content mappings.
- New sales must be blocked or made ineligible when required live components are retired or otherwise no longer sellable, but existing entitlements are not rewritten.

Historical protection does not mean access continues after expiry for materials, practice, or package exam attempt start eligibility.

---

## Authorization Rules

Authorization must use package entitlement and benefit rights, not live payment lookups.

Rules:

- Payment status is used to create entitlements; it is not queried live to authorize materials, practice, package attempt start, or report eligibility.
- A user must own the nurse profile associated with the package entitlement.
- Each benefit access path must check the corresponding benefit right type.
- Materials access must check `MaterialsAccess` and active access window.
- Practice access must check `PracticeAccess` and active access window.
- Package attempt start eligibility must later check `PackageExamAttemptEligibility`, active access window at session creation, and unused attempt state for the selected package entitlement's exact package definition/version.
- Report generation/access must later check `ReportEligibility`, package session qualification, report lifecycle, and nurse ownership.
- Authorization for package exam attempt must check the `PackageExamAttemptEligibility` right on the selected package entitlement for the specific package definition/version and purchased exam version.
- Authorization for standalone exam start must check `ExamAccessGrant` for the exam when the existing standalone policy requires a grant.
- A standalone `ExamAccessGrant` cannot satisfy a package benefit right.
- A package benefit right cannot satisfy standalone exam start.
- One package entitlement cannot satisfy another package's rights.
- One package's report right cannot be qualified by another package's session or by a standalone session.

Stage 2 should define application-level authorization services or query patterns later used by Stage 3/4, but it must not implement Stage 3 start or Stage 4 report access in this specification.

---

## Standalone Exam Compatibility

Existing standalone exam behavior must remain backward compatible.

Preserved behavior:

- `ExamAccessGrant` remains standalone paid-exam authorization evidence.
- Existing standalone payment completion continues to create `ExamAccessGrant` rows for standalone exam-access order items.
- Package fulfillment must not create `ExamAccessGrant` rows.
- Package offers, package entitlements, package attempts, and package benefit rights do not participate in the standalone effective-paid classification rule:

```text
Exam.IsFree == false OR active positive-price ExamAccess product exists
```

- Existing `isFree` and `canStart` semantics for standalone/free exam catalog and detail responses remain observably unchanged.
- Existing standalone/free exam session start must not silently select or consume package rights.
- Standalone sessions do not consume package attempt rights and do not qualify package report rights.
- Different packages that include the same exam remain independent.

Stage 2 must not modify session provenance or one-in-progress-session behavior. Those concerns belong to Stage 3.

---

## Failure and Retry Behavior

Payment and fulfillment failures must be safe, deterministic, and retryable where possible.

Rules:

- If checkout or provider completion is retried for an already-paid order, the handler must return the existing paid/fulfilled outcome when fulfillment is complete.
- If order paid transition succeeds and package fulfillment succeeds, retries must not create duplicate entitlements or duplicate rights.
- If package fulfillment fails before the paid transition commits in a shared transaction, the paid transition should roll back with fulfillment.
- If a future provider/webhook flow can mark payment paid before fulfillment completes, fulfillment must be retryable from the paid order item until it converges or fails with a deterministic unsafe state.
- Fulfillment must not partially grant access without an entitlement root.
- A report generation impossibility at Stage 2 time is not a fulfillment failure. The report right is created dormant and remains awaiting a future qualifying package session.
- If a package offer is retired after order creation but before payment completion, fulfillment uses the order item's purchased snapshot. The purchase remains valid if the order was created while the offer was active and sellable.
- If package components retire after order creation or fulfillment, existing purchased snapshots and entitlements remain valid under historical buyer protection.
- If snapshot mismatch is detected between the order item and existing entitlement, fulfillment must fail safely and not rewrite immutable facts. Mismatch detection covers every immutable purchased fact listed in `Purchased-Offer Snapshot` that is stored both on the source order item snapshot and on, or through, the entitlement. A mismatch is a non-retryable unsafe-fulfillment conflict until corrected by a separately approved support process.
- Unsafe fulfillment errors must not expose internal provider secrets, stack traces, protected exam content, or internal authorization state.

---

## Admin and Support Visibility

Stage 2 may define admin/support visibility boundaries but should not create broad admin operations without explicit implementation approval.

Allowed visibility concepts:

- Admin/support may need read-only visibility into package purchase entitlement status, payment order linkage, fulfillment state, access-window dates, and benefit-right statuses for diagnostics.
- Support views must not expose provider secrets, raw tokens, protected exam content, answer keys, protected options, rationales, or internal scoring details.
- Support views must not allow manual grant adjustment in Stage 2.
- Support views must not expose package reports, practice progress, or employer-facing purchase history.

Forbidden in Stage 2:

- Employer access to package purchases.
- Employer access to package reports.
- Employer access to package practice progress.
- Manual admin creation of entitlements outside paid fulfillment.

Explicitly deferred to Stage 3 or Stage 4 and still forbidden in Stage 2:

- Manual admin consumption/reset of package attempt rights.
- Manual report generation controls.

---

## API Boundary

Stage 2 may later expose only payment, purchase, entitlement, and benefit-right status surfaces required for fulfillment and nurse-owned entitlement visibility.

Allowed Stage 2 API concepts for future implementation planning:

- Create a payment order for a package offer.
- Include package purchased-offer snapshot fields in nurse-owned payment order responses.
- Complete existing Development/Test Sandbox checkout for package orders through the existing sandbox completion surface, returning package entitlement fulfillment summary in a backward-compatible way.
- List current nurse's package purchase entitlements.
- Get current nurse's package purchase entitlement details.
- Expose entitlement access-window and benefit-right status summaries needed by a future workspace.

Concrete Stage 2 API direction:

- Extend the existing authenticated nurse order creation endpoint, `POST /api/v1/me/nurse-profile/payment/orders`, with a request shape that accepts exactly one purchase source: existing `productId` for standalone exam access or new `packageOfferId` for a Preparation Package offer.
- Requests containing neither source or both sources are validation errors.
- Existing `productId` behavior and response fields remain backward compatible.
- Payment order item responses add a source discriminator and nullable package snapshot fields for package items without removing existing standalone fields.
- The existing authenticated checkout start endpoint, `POST /api/v1/me/nurse-profile/payment/orders/{orderId}/checkout`, remains the checkout entry point for both standalone and package orders.
- The existing Development/Test Sandbox completion endpoint remains the Sandbox completion entry point. Its response may be extended additively with package entitlement summary fields, such as fulfilled package entitlement ids, while preserving existing standalone `grantedExamIds` behavior for standalone purchases.

Stage 2 endpoint authorization:

- Nurse purchase and entitlement endpoints require authentication and current nurse ownership.
- Admin/support read-only diagnostics, if included later, require dedicated admin/support permissions defined by an approved implementation plan.

Explicitly forbidden until Stage 3:

- Package exam start endpoint.
- Package purchase selection for exam start.
- Attempt consumption endpoint.
- Session provenance mutation or source switching endpoint.
- Package-vs-standalone conflict resolution endpoint.

Explicitly forbidden until Stage 4:

- Report generation endpoint.
- Report access endpoint.
- Report retry endpoint.
- Report guidance endpoint.

Explicitly forbidden in v1 Stage 2:

- Cart endpoints.
- Subscription endpoints.
- Sponsored package endpoints.
- Employer package purchase, report, or practice progress endpoints.
- Adaptive practice or AI recommendation endpoints.

Concrete route paths, DTOs, status codes, OpenAPI metadata, and permission names remain for the separately authorized Stage 2 implementation plan.

Any new permissions introduced by a future Stage 2 implementation plan should follow the dedicated Preparation Package permission pattern established in Stage 1 and must not reuse `Exams.*` or `Questions.*` permissions for package entitlement or benefit-right administration.

---

## Security and Privacy Requirements

- Backend authorization is authoritative.
- Clients must not be trusted for nurse identity, entitlement identity ownership, price, currency, access duration, package version, package composition, right type, right status, or fulfillment status.
- API responses must not expose provider secrets, provider raw payloads, payment tokens, access tokens, refresh tokens, password hashes, internal authorization state, or stack traces.
- Entitlement and right DTOs must not expose domain navigation objects or EF entities.
- Purchased-offer snapshots and entitlement outputs must not expose protected exam question text, answer identifiers, correct answers, answer keys, protected options, rationales, or internal scoring logic.
- Employer access to package purchase history, practice progress, or reports is forbidden in v1.
- Authorization checks must be based on nurse ownership and the specific benefit right, not on whether an order appears paid.
- Nurse ownership must follow the existing current-nurse resolution pattern used by payment handlers, equivalent to resolving the current nurse profile through `NurseRoleGuard`/current-user context before loading or mutating nurse-owned orders, entitlements, or rights.
- Logs must not include provider secrets, tokens, protected exam content, or sensitive internal authorization state.

---

## Data Integrity Requirements

Future implementation must use EF Core Code-First migrations and PostgreSQL constraints. No manual schema changes are allowed.

Required data integrity invariants:

- Unique entitlement per package order item.
- Exactly one benefit right of each required type per entitlement.
- Entitlement references one nurse profile.
- Entitlement references its source payment order and payment order item.
- Entitlement references the purchased package definition, package version, and package offer facts from the snapshot.
- Active same-package repurchase for the same nurse and package definition is prevented transactionally.
- Access-window timestamps are UTC and server-owned.
- Purchased snapshot fields are immutable after order creation.
- Entitlement purchase facts are immutable after fulfillment.
- EF relationships for financial, entitlement, right, and snapshot records must use `DeleteBehavior.Restrict` unless an implementation plan explicitly justifies `NoAction` for provider-specific migration behavior. Cascade delete is forbidden for these records.
- Historical buyer records must remain referentially stable even when live package catalog records are retired.

The implementation plan must prove active-same-package repurchase protection with concurrency tests.

---

## Required Tests for Future Implementation

Future Stage 2 implementation must include tests proving:

### Domain tests

- `PackageEntitlement_CreateFromSnapshot_CapturesImmutablePurchaseFacts`
- `PackageEntitlement_CreateFromSnapshot_SetsAccessWindowFromFulfillmentTimeAndPurchasedDurationDays`
- `PackageEntitlement_ActiveAuthorizationRequiresActiveStatusAndCurrentAccessWindow`
- `PackageBenefitRights_CreateDefaultSet_IncludesExactlyFourStage2RightTypes`
- `PackageBenefitRights_CreateDefaultSet_CreatesReportRightDormantAndUnconsumed`

### Application tests

- `Handle_CreatePackageOrder_WithActiveSellableOffer_CreatesPendingOrderWithPurchasedOfferSnapshot`
- `Handle_CreatePackageOrder_WithInactiveRetiredMissingOrUnsellableOffer_ThrowsInvalidOperationExceptionOrNotFound`
- `Handle_CreatePackageOrder_WithActiveSamePackageEntitlement_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithExpiredSamePackageEntitlement_AllowsSeparateRepurchase`
- `Handle_CreatePackageOrder_WithDifferentPackageSharingSameExam_AllowsIndependentPurchase`
- `Handle_CompleteSandboxCheckout_ForPackageOrder_CreatesOneEntitlementAndFourBenefitRights`
- `Handle_FulfillPackageOrderItem_WhenRepeatedForSameOrderItem_ReturnsExistingEntitlement`
- `Handle_FulfillPackageOrderItem_WhenConcurrentForSameOrderItem_ConvergesToOneEntitlementAndFourRights`
- `Handle_CompleteSandboxCheckout_ForExamAccessProduct_PreservesExistingExamAccessGrantFulfillment`
- `Handle_CompleteSandboxCheckout_ForPackageOrder_DoesNotCreateExamAccessGrant`
- `Handle_AuthorizePackageBenefit_WithActiveRightAndWindow_AllowsAccessWithoutPaymentLookup`
- `Handle_FulfillPackageOrderItem_WithExistingEntitlementSnapshotMismatch_ThrowsUnsafeFulfillmentConflict`
- `Handle_FulfillPackageOrderItem_WhenOfferRetiredAfterOrderCreation_UsesPurchasedSnapshot`
- `Handle_FulfillPackageOrderItem_WhenComponentsRetiredAfterFulfillment_DoesNotMutateHistoricalEntitlement`

### Infrastructure tests

- `PackageEntitlementConfiguration_EnforcesUniqueEntitlementPerOrderItem`
- `PackageBenefitRightConfiguration_EnforcesOneRightPerTypePerEntitlement`
- `PackageEntitlementConfiguration_EnforcesOneActiveEntitlementPerNurseAndPackageDefinition`
- `PackageEntitlementConfiguration_UsesRestrictDeleteBehaviorForFinancialSnapshotAndRightRelationships`
- `PackageEntitlementConfiguration_PersistsUtcAccessWindowAndDurationDerivedEndTime`

### WebApi tests

- `CreatePaymentOrder_WithPackageOfferId_Returns401WithoutJwt`
- `ListMyPackageEntitlements_ReturnsOnlyCurrentNurseEntitlements`
- `GetMyPackageEntitlement_ForAnotherNurse_Returns404Or403WithoutExposure`
- `GetMyPackageEntitlement_DoesNotExposeProviderSecretsTokensPasswordHashProtectedExamContentCorrectAnswersOrInternalAuthorizationState`
- `PackageStage2Endpoints_DoNotExposePackageAttemptStartReportGenerationOrReportAccessRoutes`
- `PackageStage2Endpoints_DoNotExposeEmployerPackagePurchaseReportOrPracticeProgressRoutes`
- `ExistingPaymentAndExamEndpoints_RemainBackwardCompatibleAfterPackageFulfillment`

### Compatibility tests

- `StandaloneExamPurchaseJourney_OrderCheckoutSandboxCompletionGrantAndExamStart_RemainsSupported`
- `StartExamSession_ForFreeExamWithoutPaidProduct_StillAllowsStartWithoutGrant`
- `StartExamSession_ForStandalonePaidExamWithoutGrant_StillReturnsForbidden`
- `ListAndGetExams_WithPackageOffersAndEntitlements_DoNotChangeStandaloneIsFreeOrCanStart`

---

## Stage 3 Handoff

Stage 3 receives from Stage 2:

- Package purchase entitlement id as the client-selectable package purchase identifier for package-attempt start.
- Internal package exam attempt eligibility right linked to the selected entitlement.
- Access-window start/end facts.
- Purchased exam id and exact exam version id.
- Purchased package version and package definition identity.
- Purchased-offer snapshot provenance.

Stage 3 must preserve these constraints:

- The client selects the package purchase entitlement, not an internal right id.
- The backend resolves the package exam attempt eligibility right.
- Stage 3 consumes the attempt atomically with qualifying session creation.
- Stage 3 records immutable session provenance.
- Stage 3 enforces one in-progress session per nurse per exam version across sources.
- Stage 3 keeps standalone/free start behavior backward compatible.

Stage 2 must not pre-implement package attempt start or attempt consumption.

---

## Stage 4 Handoff

Stage 4 receives from Stage 2:

- Dormant report eligibility right.
- Entitlement and purchased-offer snapshot provenance.
- Purchased reporting-profile publication id.
- Purchased material version ids and order.
- Purchased practice collection version id.
- Access-window and ownership facts.

Stage 4 must preserve these constraints:

- Report generation waits for a qualifying package exam session from Stage 3.
- Report failure does not invalidate scoring, does not permanently consume the report right, and does not require a retake.
- Generated report guidance is restricted to the purchased package's material versions and practice collection version.
- Report output must not expose protected exam question text, correct-answer identifiers, protected options, rationales, answer keys, or per-question exam review.
- Nurse-owned report access remains the v1 boundary.

Stage 2 must not pre-implement report generation, report persistence, report retry, or report access.

---

## Open Questions

No unresolved business questions block this Stage 2 draft.

Implementation-planning details deliberately deferred until after this specification is reviewed:

- Exact entity, table, and property names.
- Whether package order creation is a separate command/endpoint or a generalized extension of existing order creation.
- Exact payment completion DTO shape for mixed standalone/package fulfillment summaries.
- Exact admin/support read-only diagnostics scope and permissions, if any.
- Exact route paths, status codes, OpenAPI metadata, and permission constants.

These are implementation details, not missing business rules.

---

## Decisions Summary

- Stage 2 aggregate root is a nurse-owned package purchase entitlement.
- Package entitlement is separate from payment order, order item, package offer, package version, and standalone `ExamAccessGrant`.
- Stage 2 generalizes payment order items with a source discriminator and package-offer snapshot support while preserving the existing standalone `ExamAccessProduct` path.
- Package order items require immutable purchased-offer snapshots.
- The existing nurse order creation endpoint is extended additively to accept exactly one of `productId` or `packageOfferId`; existing checkout and Sandbox completion endpoints remain the entry points.
- Fulfillment idempotency key is the source package order item id plus item source type.
- Stage 2 v1 Sandbox package fulfillment is atomic with the paid transition and also retry-safe by paid order item.
- Fulfillment creates one entitlement and four benefit rights.
- Benefit rights are separate persisted child rows/entities of the entitlement.
- Right types are materials access, practice access, package exam attempt eligibility, and report eligibility.
- Access starts at successful fulfillment; for Stage 2 v1 Sandbox completion it equals persisted `PaidAt`, and it ends after the purchased duration days from the snapshot.
- Report right is dormant after fulfillment until a qualifying package session exists.
- Same package definition cannot be repurchased while active.
- One active same-package entitlement per nurse/package definition is enforced by a database uniqueness invariant over persisted `Active` entitlements plus transactional stale-expiry handling.
- Same package definition can be repurchased after expiry and creates a separate entitlement.
- Different packages sharing an exam are independent purchases.
- Historical buyers remain protected after live catalog/content retirement.
- Authorization uses entitlement and benefit rights, not live payment lookups.
- Package fulfillment must not create or overload `ExamAccessGrant`.
- Stage 2 does not create sessions, consume attempts, generate reports, implement workspace runtime, or expose employer package data.
