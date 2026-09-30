# Commerce Screen Approval Packet — T-FE-077 / GATE-FE-T077

```yaml
document_id: NPS-DES-INV-COMMERCE-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-19
prepared_by: T-FE-077 campaign
gate: GATE-FE-T077
gate_status: VERIFIED (closed by human-approved decisions HC-C1–HC-C15 below)
authorization: Decisions HC-C1–HC-C15 were approved
  by the human technical lead on 2026-09-19. COM-001/002/003/005/006/007/008
  are APPROVED as design authority; COM-004 stays DEFERRED pending
  provider-callback architecture. Approval does not itself start
  implementation; T-FE-082 and later Commerce tasks remain NOT STARTED until
  separately authorized.
```

## 1. Purpose

This is the T-FE-077 Commerce screen approval packet covering COM-001..008. It records
per-screen backend contracts verified from source (not inferred), the approved
presentation authority for the buildable scope, explicit non-scope and deferred
decisions, and the decisions deferred to owning gates. Per the frontend ledger,
GATE-FE-T077 requires "Commerce approval packet with decisions per screen".

## 2. Screen inventory (COM-001..008)

| Screen | Route | Owner | Backend contract | Status | Approval |
|---|---|---|---|---|---|
| Product catalog (COM-001) | `/commerce/products` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-082` | `GET /api/v1/payment/products` (page/pageSize/optional examId), `PaginatedResult<PaymentProductDto>`, active products for published exams, deterministic Name/Id order | NOT STARTED | APPROVED (HC-C4; browse-only, backend ordering/pagination, no search/sort/filter) |
| Product detail (COM-002) | `/commerce/products/:productId` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-082` | `GET /api/v1/payment/products/{id}` → `PaymentProductDto`, 404 when missing/inactive/unpublished | NOT STARTED | APPROVED (HC-C5; facts only, Purchase CTA bounded by HC-C5) |
| Checkout/order creation (COM-003) | `/checkout` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-084` | `POST /api/v1/me/nurse-profile/payment/orders` → `201 PaymentOrderDto` + Location; exact-one-source request (`productId` or `packageOfferId`) | NOT STARTED | APPROVED (HC-C6; explicit confirmation first, no implicit creation) |
| Checkout processing (COM-004) | NO ROUTE (DEFERRED) | `T-FE-086` | provider-callback architecture unresolved | DEFERRED | DEFERRED (HC-C7; no dedicated screen, no fake progress) |
| Payment success (COM-005) | `/checkout/orders/:orderId/success` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-087` | server-backed order truth via `GET /api/v1/me/nurse-profile/payment/orders/{id}` | NOT STARTED | APPROVED (HC-C8; generic server-truth state only) |
| Payment failure (COM-006) | `/checkout/orders/:orderId/failure` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-087` | server-backed order truth via `GET /api/v1/me/nurse-profile/payment/orders/{id}` | NOT STARTED | APPROVED (HC-C8; generic server-truth state only) |
| Order history (COM-007) | `/commerce/orders` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-088` | `GET /api/v1/me/nurse-profile/payment/orders` (page/pageSize/optional status), owned orders, CreatedAt DESC | NOT STARTED | APPROVED (HC-C9; backend ordering/pagination, no invented filters) |
| Order detail (COM-008) | `/commerce/orders/:orderId` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-088` | `GET /api/v1/me/nurse-profile/payment/orders/{id}` → `PaymentOrderDto`, owner-scoped 404; `POST .../cancel` when backend permits | NOT STARTED | APPROVED (HC-C10; facts + conditional cancel only) |

## 3. Backend contracts verified from source (evidence, not new design)

- Product catalog/detail: `GET /api/v1/payment/products` and `GET
  /api/v1/payment/products/{id:guid}`, `.RequireAuthorization()` only with no
  permission or nurse-role requirement (T-FE-081 VERIFIED). List accepts
  `page`, `pageSize`, and optional `examId`; returns
  `PaginatedResult<PaymentProductDto>` filtered to active products for published
  exams, ordered deterministically by Name/Id. Detail maps
  missing/inactive/unpublished to `404`.
- Order create/list/detail/cancel: authenticated `/api/v1/me/nurse-profile/payment/orders`
  group (nurse-profile authorization; T-FE-083 VERIFIED). Create returns `201
  PaymentOrderDto` with `Location`; request carries exactly one purchase source
  (`productId` or `packageOfferId`) and no idempotency key. List returns owned
  orders as `PaginatedResult<PaymentOrderDto>` with `page`/`pageSize`/optional
  `status`, lazily expires past-due orders, filters by current nurse profile,
  orders `CreatedAt DESC, Id`. Detail/cancel are owner-scoped (`404` for
  foreign/missing). Cancel expires past-due orders first, conflicts (`409`
  `CheckoutInProgress`) on active checkout sessions, then cancels.
- Checkout/sandbox transitions: `POST .../payment/orders/{orderId}/checkout`
  starts a checkout session (optional idempotency key); the
  `/dev/sandbox/payment/checkout-sessions/{id}/complete` endpoint is a
  development/test-fixture capability, not production screen authority.
- Money semantics verified from source: `PaymentProductDto` exposes `Currency`
  (code string) and `UnitAmountMinor` (`long`, JSON string-serialized);
  `PaymentOrderDto` exposes `Currency`, `TotalAmountMinor` (`long`, JSON
  string-serialized), `CreatedAt`/`UpdatedAt`, nullable
  `ExpiresAt`/`PaidAt`/`CancelledAt`, and `Items[]` (`PaymentOrderItemDto`
  with `ProductId`, `ProductName`, `ProductType`, `ExamId`, `Currency`,
  `UnitAmountMinor`, `Quantity`, `LineTotalAmountMinor`, `SourceType`,
  `SourceId`, optional package snapshot). Minor-unit semantics are therefore
  contractually established. Order lifecycle statuses are `PendingPayment`,
  `Paid`, `Failed`, `Cancelled`, `Expired`.

## 4. HUMAN APPROVED DECISIONS (recorded 2026-09-19)

**HC-C1 — Commerce domain boundary. APPROVED.** Commerce owns purchasable
product discovery, product detail, server-backed order creation, server-backed
order history/detail, supported payment/sandbox transition surfaces,
cancellation when backend permits it, and factual server-backed
success/failure states. Commerce does NOT own Nurse preparation-package
entitlement presentation, package practice, package exam consumption, package
analytical report, provider-specific payment claims not represented by
backend, card-number/payment-instrument collection unless an explicit
backend/provider contract exists, pricing recommendations, or
frontend-invented discount logic. T-FE-079 and the Preparation Package learner
surfaces remain separate.

**HC-C2 — Copy tone. APPROVED.** Use factual transactional language. Prefer
Buy, Purchase, Order, Payment, Total, Cancel order, Payment completed, Payment
failed. Avoid unsupported marketing language such as Best value, Recommended,
Limited time, Guaranteed, Secure checkout unless an authoritative
backend/product source explicitly supplies that claim. Never claim
payment-provider behavior the application cannot prove.

**HC-C3 — Price/currency rule. APPROVED.** Backend monetary fields are
authoritative. Render backend-supplied amount and currency using existing
project-safe monetary formatting. Never convert currencies, infer currency
symbols from country, recalculate totals, derive discounts, invent
tax/shipping/service-fee lines, or round using custom business logic. Minor
units use the repository/backend-defined conversion semantics only (minor-unit
semantics are contractually established per §3). Raw numeric/currency codes
may be formatted for display only when semantics are contractually clear. If a
future contract does not establish minor-vs-major-unit semantics, its
implementation task MUST stop with a bounded monetary-contract gap rather
than guess.

**HC-C4 — COM-001 product list. APPROVED.** COM-001 is the Commerce product
catalog letting an authenticated learner browse currently purchasable backend
products. Render only safe backend-supplied fields that exist in the
authoritative Product DTO. Approved semantic content: product name/title;
short description when non-empty; factual product type/category when backend
supplies a human-readable value; price; currency; availability/purchasability
state only when backend explicitly provides it. Primary row/card action is
View details. Never put provider checkout directly on the list. Never expose
raw product ids, internal version ids, provider ids, entitlement ids, or
debug/state-machine values. No Commerce search/filter/sort controls: the
initial list uses backend ordering and backend pagination where the endpoint
provides pagination; no client-side search, sorting, or product-type filter
unless a later task proves an explicit approved backend filter exists and this
packet is amended. Empty title is "No products available" with body "There
are no products available to purchase right now." and no empty-state purchase
CTA. Generic error is "We couldn't load products. Try again." with Retry.
Never expose raw backend errors.

**HC-C5 — COM-002 product detail and Purchase CTA. APPROVED.** COM-002 renders
only authoritative safe Product DTO fields: product name/title; description
when non-empty; factual product type/category when human-readable; price;
currency; any included-content summary ONLY if directly supplied in a
learner-safe backend field. Primary action is Purchase, shown only when
backend state/contracts indicate the product can currently be ordered.
Purchase means beginning the application's server-backed order-creation flow —
NOT immediate entitlement, payment success, direct provider completion, or
guaranteed fulfillment. Never label the action Pay now unless the actual
implementation contract directly initiates a payment-provider interaction and
that wording is separately approved; the initial approved label is Purchase.
Never invent package benefits, exam attempts, materials, expiration periods,
refund rules, or provider terms unless the Product DTO explicitly supplies
those facts.

**HC-C6 — COM-003 order creation, idempotency, and errors. APPROVED.** COM-003
owns the confirmation step before creating an order. Before the create-order
request, show an explicit confirmation containing product title,
backend price/currency, and a factual statement that an order will be
created. Confirmation heading is "Create order?"; primary action is "Create
order"; secondary action is Cancel. Never create the order merely by entering
the Product Detail screen; never make order creation implicit. If the
create-order backend contract requires an idempotency key, the frontend may
generate/manage it internally per existing backend/API conventions — it is
NOT user-editable UI and is never rendered. Duplicate-click protection is
required; never blindly create a second order after an ambiguous failure; use
existing backend/order reconciliation capability where available, otherwise
the future implementation must stop on that bounded contract gap.
Validation/conflict behavior stays factual: generic recoverable create error
is "We couldn't create this order. Try again."; backend conflicts proving an
existing/non-creatable order condition show a factual message derived from
the documented conflict meaning, never raw server text and never an invented
conflict taxonomy — implementation maps only documented backend
statuses/conditions.

**HC-C7 — Provider checkout boundary and COM-004 deferral. APPROVED.** Never
invent a payment-provider checkout screen: no card-number fields, expiry/CVV
fields, billing-address forms, provider logos, redirect instructions, or
external-payment buttons unless a later implementation contract explicitly
exposes and authorizes such a provider interaction. The frontend follows
backend-supported payment capabilities only. COM-004's canonical standalone
processing route remains DEFERRED: no dedicated processing screen is created
merely to fill the inventory slot. Processing is represented through factual
server-backed order status on the appropriate order-detail/success/failure
surface. No client-invented timer, fake progress percentage, or "Do not close
this page" claim. A future provider-callback architecture needing a dedicated
route requires separate design approval.

**HC-C8 — COM-005/006 server-truth states. APPROVED.** COM-005 is a generic
server-backed success state: heading "Payment completed" with supporting copy
"Your payment was completed successfully.", rendered only when authoritative
backend order/payment truth indicates successful completion. Approved actions
are View order and — only when the resulting learner entitlement/product
destination is safely known through existing application contracts — Continue;
the Continue destination comes from authoritative product/order fulfillment
data, never guessed from product type alone (e.g. no inferred Preparation
Package entitlement route); with no safe destination, omit Continue and keep
View order. COM-006 is a generic server-backed failed-payment state: heading
"Payment failed" with supporting copy "Your payment couldn't be completed."
Approved actions are contract-dependent: View order, plus Try again ONLY when
an existing backend contract explicitly supports retry/re-initiation — never
Try again by merely re-POSTing order creation, and never imply whether the
learner was charged unless backend truth explicitly establishes it. COM-005/006
are never selected from URL query text alone, navigation state alone, client
assumptions, or unverified provider-callback parameters; the screen always
reconciles/loads authoritative server-backed order truth, backend ownership
still decides availability for route-contained order ids, and
foreign/missing order information stays privacy-safe.

**HC-C9 — COM-007 order history. APPROVED.** COM-007 is the learner order
history displaying backend-owned orders only. Approved visible row fields,
only when provided by the Order DTO: product/order human-readable title,
created date/time, total amount, currency, factual order/payment status.
Primary action is View order. Never render raw order id, product id, provider
transaction id, idempotency key, entitlement id, or account/nurse id. Initial
history has NO frontend-invented search, date filter, product filter, status
filter, or sort selector; backend ordering applies with backend pagination
where supported; exposing an explicit server-side filter later requires
packet amendment. Empty title is "No orders yet" with body "Your orders will
appear here after you create one." and action Browse products. Generic error
is "We couldn't load your orders. Try again." with Retry.

**HC-C10 — COM-008 order detail, status, cancel, and back navigation.
APPROVED.** COM-008 is the authoritative learner-owned order detail rendering
only safe Order DTO fields: product/order title, created date/time, total
amount, currency, factual current status, payment status if separate and
human-meaningful, completed/finalized date when useful and backend-supplied;
never raw ids/provider internals. Status labels are factual mappings from
actual backend status values (verified lifecycle: PendingPayment, Paid,
Failed, Cancelled, Expired); the future implementation inspects the exact
backend enum/string contract and maps each reachable status to concise
user-facing copy; unknown/unusable status fails safely rather than exposing
raw internal text; no frontend state transition is ever invented. Show Cancel
order ONLY when the backend contract/state explicitly allows cancellation,
behind explicit confirmation (heading "Cancel order?", body "This order will
be cancelled if it is still eligible for cancellation.", actions Keep order /
Cancel order). Never optimistically claim cancellation before backend
success; after success reload/reconcile authoritative order detail. On
documented cancellation conflict (order can no longer be cancelled): factual
copy "This order can no longer be cancelled.", then reconcile/reload current
order state; never blind-retry, never expose backend exception text. Stable
navigation: Back to orders targets the canonical Order History route (never
browser history); Product Detail back action is Back to products targeting the
canonical Product List route; Success/failure screens use View order targeting
canonical Order Detail.

**HC-C11 — Route authority. APPROVED.** Use ONLY Commerce route identities
already present in the page registry and canonical route registry; no invented
routes. Copied from authority: COM-001 `COMMERCE_PRODUCTS` `/commerce/products`;
COM-002 `COMMERCE_PRODUCT_DETAIL` `/commerce/products/:productId`; COM-003
`COMMERCE_CHECKOUT` `/checkout`; COM-004 DEFERRED (no approved canonical
route — recorded as DEFERRED, not missing); COM-005
`COMMERCE_PAYMENT_SUCCESS` `/checkout/orders/:orderId/success`; COM-006
`COMMERCE_PAYMENT_FAILURE` `/checkout/orders/:orderId/failure`; COM-007
`COMMERCE_ORDERS` `/commerce/orders`; COM-008 `COMMERCE_ORDER_DETAIL`
`/commerce/orders/:orderId`. Outcome routes carry the registry's existing
caution: URL alone is never authoritative proof of payment state.

**HC-C12 — Authentication, ownership, and loading. APPROVED.** Commerce
learner screens are authenticated-only per existing backend/frontend
authority. Order history/detail/actions operate only on learner-owned orders;
frontend route visibility is never the security boundary; backend ownership
remains authoritative. Foreign/missing order uses privacy-safe unavailable
behavior with generic copy "This order isn't available.", never revealing
whether another learner owns it. Loading uses the existing verified shared
loading treatment; no Commerce-specific spinner language; loading is factual
and non-destructive with no fake payment progress.

**HC-C13 — Responsive, RTL, and accessibility. APPROVED.** All future Commerce
screens inherit the approved platform foundation: mobile-first safe stacking;
390px target with zero unintended horizontal overflow; logical CSS
properties; RTL-safe layout with money values readable in mixed-direction
content; semantic headings; semantic lists/details; correctly associated form
labels; visible focus; text status never color-only; >=44px interactive
targets where applicable; loading/error announcements; keyboard-complete
confirmations/actions; WCAG 2.2 AA target. No dedicated dashboard/sidebar is
authorized.

**HC-C14 — Preparation Package and sandbox boundaries. APPROVED.** Commerce
stops at transactional product/order/payment truth. Preparation Package
learner surfaces own entitlements, package access, practice, package exam,
package report, and package-specific guidance. Commerce may navigate to a
fulfilled destination only when backend/application authority safely
identifies it; never duplicate entitlement detail inside Order Detail. A
sandbox-complete endpoint existing for development/testing is NOT production
UI authority: never expose a learner-visible sandbox-completion action unless
an explicit environment/tooling/product decision separately authorizes it;
E2E/test tooling may use the supported sandbox endpoint where governance
permits. This packet distinguishes test-fixture capability from production
screen design.

**HC-C15 — Recorded OpenAPI defects and semantic-field rule. APPROVED.**
Recorded (not fixed in T-FE-077): response-metadata defects leave the
generated product-list, product-detail, order-list, order-detail, and cancel
operations void-typed/body-discarding; order-create is already
typed/body-preserving. Each later implementation task consuming a defective
operation must receive an explicitly authorized metadata prerequisite first.
Render only approved semantic fields that are directly supported by the
authoritative DTO; do not derive missing commerce facts. This prevents design
authority from becoming fabricated API authority.

## 5. Route authority (copied, not invented)

See HC-C11. Source registries: `docs/frontend/design/inventory/page-registry.md`
(COMMERCE_* rows) and `frontend/src/app/core/routing/canonical-routes.ts`.
COM-004 has no approved canonical route (DEFERRED, not missing). No paths
were invented in T-FE-077.

## 6. Relationships explicitly not owned here

- T-FE-079 Preparation Package analytical report and learner surfaces
  (entitlements, practice, package exam, package report, package guidance)
  remain separate and unchanged by this packet.
- Production payment-provider selection/release (T-FE-089, T-FE-137) remains an
  external/backend decision outside this packet.
- Admin payment-product management (future T-FE-111 scope) is not learner
  commerce and is not authorized here.

## 7. Approval record

- 2026-09-19: Human technical lead approved HC-C1–HC-C15 for the Commerce
  screen family. COM-001/002/003/005/006/007/008 are APPROVED; COM-004 stays
  DEFERRED pending provider-callback architecture. `T-FE-082` and later
  Commerce tasks remain NOT STARTED pending separate authorization (including
  the recorded OpenAPI metadata prerequisites where applicable).
- GATE-FE-T077 is closed as VERIFIED on the strength of: every COM screen
  having an explicit disposition, contracts verified from source (not
  inferred), provider/sandbox boundaries pinned, route identities copied from
  authority, known OpenAPI defects recorded without unauthorized fixes, and
  responsive/RTL/accessibility bindings included — consistent with the
  T-FE-040/T-FE-052/T-FE-061/T-FE-070/T-FE-092/T-FE-113 family-gate precedent.
