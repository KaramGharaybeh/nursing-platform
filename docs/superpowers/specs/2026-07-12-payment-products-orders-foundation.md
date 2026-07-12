# Phase 8A Payment Products & Orders Foundation Specification

## Objective

Define Phase 8A as a backend-only payment catalog and local order foundation for purchasable exam access.

Phase 8A introduces durable payment products, price snapshots, and nurse-owned local orders without checkout sessions, payment provider integration, webhooks, automatic access grants, subscriptions, refunds, invoices, taxes, discounts, wallets, payouts, dashboards, exports, or frontend work.

This specification does not start implementation and does not modify source code, tests, migrations, tracking docs, frontend, recruitment, exam-taking, exam result, exam review, or exam analytics contracts.

## Current Baseline

The platform currently provides:

- Identity, JWT authentication, current-user service, roles, permissions, and `.RequirePermission(...)`.
- Existing `NurseRoleGuard` and nurse profile ownership patterns.
- Existing `Exams.*` permissions for exam content administration.
- Existing exam catalog/content/session/access-grant entities.
- Existing `ExamAccessGrant` entitlement model for non-free exam access.
- Existing WebApi Problem Details behavior for validation, forbidden, unauthorized, not found, conflict, and bad request errors.
- EF Core Code-First migrations and PostgreSQL as the source of truth.

The platform does not currently provide:

- Payment product entities.
- Payment order entities.
- Checkout sessions.
- External payment provider ids or state.
- Webhook processing.
- Automatic exam access grant issuance from payments.
- Payment-specific admin permissions.

## In Scope

- Backend-only payment products for exam access.
- `PaymentProductType` enum with only `ExamAccess` implemented in Phase 8A.
- Product pricing with ISO 4217 currency code and integer minor-unit amount.
- Product active/inactive lifecycle.
- Admin/content product management endpoints needed to create and maintain purchasable exam products.
- Nurse-facing active product catalog endpoints.
- Nurse-owned local order creation.
- Order line item snapshots.
- Nurse-owned order list/detail endpoints.
- Nurse-owned order cancellation while pending payment.
- Order status lifecycle without real payment providers.
- EF entities, configurations, migration, Application handlers, WebApi endpoints, and tests in the later implementation plan.

## Out of Scope

- Frontend.
- Checkout sessions.
- Real payment provider integration.
- Payment provider ids.
- Provider payment state.
- Webhooks.
- Automatic `ExamAccessGrant` issuance.
- Marking orders paid through nurse, admin, test-only, or hidden endpoints.
- Subscriptions.
- Refund workflows.
- `Refunded` status in Phase 8A implementation.
- Coupons, discounts, taxes, invoices, wallets, payouts, or revenue reports.
- CSV, Excel, or export endpoints.
- Phase 9 admin dashboards/reports.
- Recruitment, contact-request, candidate, or employer workflows.
- Nurse exam-taking, result, review, or analytics contract changes.

## Actors And Permissions

### Authenticated Product Catalog User

The public payment product catalog is authenticated-only, not nurse-role-only.

Authenticated users may:

- List active purchasable payment products through `GET /api/v1/payment/products`.
- View active purchasable payment product details through `GET /api/v1/payment/products/{id}`.

Catalog endpoint rules:

- Public product catalog endpoints require `.RequireAuthorization()`.
- Public product catalog endpoints do not require `.RequirePermission(...)`.
- Public product catalog endpoints do not require `NurseRoleGuard`.
- Anonymous users cannot access public product catalog endpoints.

### Nurse

An authenticated user with the `Nurse` role and a `NurseProfile`.

Nurse may:

- Create a local pending-payment order for an active product.
- View only their own orders.
- Cancel only their own pending-payment orders.

Order endpoint rules:

- Order endpoints under `/api/v1/me/nurse-profile/payment/orders` require `.RequireAuthorization()`.
- Order Application handlers must enforce Nurse role through `NurseRoleGuard`.
- Order Application handlers must enforce current `NurseProfile` ownership.

Nurse may not:

- View inactive products from the public product catalog.
- Create orders for inactive products.
- Create orders for unpublished, archived, or missing exams.
- View another nurse's orders.
- Mark an order paid.
- Trigger an `ExamAccessGrant`.
- Submit card data or provider tokens.

### Admin / Content Manager

Phase 8A includes backend-only product management because products must exist before future checkout can sell them.

Admin/content manager may:

- List payment products.
- View payment product details.
- Create exam-access products.
- Update safe product display/pricing fields.
- Archive/deactivate products.
- Restore/reactivate products when the linked exam is still valid.

Permissions decision:

- Use existing `Exams.View` for admin product read endpoints.
- Use existing `Exams.Edit` for create, update, archive, and restore of exam-linked payment products.
- Do not add new payment permissions in Phase 8A.
- Do not modify permission seed data in Phase 8A.

Reason: Phase 8A products are exam access products attached to existing exam catalog content. Dedicated payment permissions can be introduced in a later broader payment-administration phase if needed.

### Anonymous User

Anonymous users cannot access Phase 8A product or order endpoints.

## Domain / Model Impact

Phase 8A likely requires a migration because it introduces new persisted payment tables.

### PaymentProduct

Fields:

- `Id`
- `Type`
- `ExamId`
- `Name`
- `Description`
- `Currency`
- `UnitAmountMinor`
- `IsActive`
- audit fields

Rules:

- `Type == ExamAccess` is the only implemented type in Phase 8A.
- `ExamId` is required for `ExamAccess` products.
- `PaymentProduct.Type` is immutable after creation.
- `PaymentProduct.ExamId` is immutable after creation.
- Product name is required and bounded.
- Currency is required, uppercase ISO 4217, exactly three letters.
- `UnitAmountMinor` must be greater than zero.
- Active products are purchasable only when the linked exam is published and not archived.
- Inactive products remain readable to admin and historical orders.
- Admin product update may change only `Name`, `Description`, `Currency`, and `UnitAmountMinor`.
- Product active state must change only through archive/restore endpoints.
- `AdminUpdatePaymentProductRequest` must not include `Type`, `ExamId`, or `IsActive`, so Type/ExamId mutation is impossible through the update endpoint.
- Products are not hard-deleted in Phase 8A.

### PaymentProductType

Values:

- `ExamAccess`

Future-compatible enum values may be added later, but Phase 8A handlers must reject unsupported types.

### PaymentOrder

Fields:

- `Id`
- `NurseProfileId`
- `Status`
- `Currency`
- `TotalAmountMinor`
- `CreatedAt`
- `UpdatedAt`
- `ExpiresAt`
- `PaidAt`
- `CancelledAt`
- audit fields

Rules:

- Created nurse orders start as `PendingPayment`.
- New Phase 8A orders set `Status = PendingPayment`, `ExpiresAt = CreatedAt + 30 minutes`, `PaidAt = null`, and `CancelledAt = null`.
- `Draft` is not implemented in Phase 8A to avoid cart semantics.
- `PaidAt` remains null in Phase 8A because no payment confirmation exists.
- Pending orders may expire through server-side query/handler logic.
- Only pending orders may be cancelled by the owning nurse.
- `Paid` and `Failed` remain enum values only for future provider/webhook phases; no Phase 8A handler transitions orders to either status.

### PaymentOrderStatus

Values:

- `PendingPayment`
- `Paid`
- `Failed`
- `Cancelled`
- `Expired`

`Refunded` is deferred because refunds are out of scope for Phase 8A.

### PaymentOrderItem

Fields:

- `Id`
- `OrderId`
- `ProductId`
- `ProductNameSnapshot`
- `ProductTypeSnapshot`
- `ExamIdSnapshot`
- `Currency`
- `UnitAmountMinor`
- `Quantity`
- `LineTotalAmountMinor`
- audit fields

Rules:

- Phase 8A order creation supports a single product line item with quantity `1`.
- Server always creates exactly one order item with `Quantity = 1`.
- `LineTotalAmountMinor = UnitAmountMinor`.
- `TotalAmountMinor = LineTotalAmountMinor`.
- Snapshot fields preserve the purchased product and price at order creation time.
- Later product name, price, currency, active state, or linked exam changes must not mutate existing order items.

## API Contract Proposal

All endpoints return DTOs, never EF/domain entities.

### Nurse Product Catalog

- `GET /api/v1/payment/products`
  - Name: `ListPaymentProducts`
  - Authorization: `.RequireAuthorization()`
  - Actor rule: authenticated-only, not nurse-role-only.
  - Does not require `.RequirePermission(...)`.
  - Does not require `NurseRoleGuard`.
  - Query: `page`, `pageSize`, `examId`
  - Returns active purchasable products only.

- `GET /api/v1/payment/products/{id:guid}`
  - Name: `GetPaymentProduct`
  - Authorization: `.RequireAuthorization()`
  - Actor rule: authenticated-only, not nurse-role-only.
  - Does not require `.RequirePermission(...)`.
  - Does not require `NurseRoleGuard`.
  - Returns active purchasable product detail or `404`.

### Nurse Orders

- `POST /api/v1/me/nurse-profile/payment/orders`
  - Name: `CreateMyPaymentOrder`
  - Authorization: `.RequireAuthorization()`
  - Request: `CreatePaymentOrderRequest`
  - `CreatePaymentOrderRequest` contains only `ProductId`.
  - Creates a local `PendingPayment` order for the authenticated nurse.
  - Returns `201 Created`.

- `GET /api/v1/me/nurse-profile/payment/orders`
  - Name: `ListMyPaymentOrders`
  - Authorization: `.RequireAuthorization()`
  - Query: `page`, `pageSize`, optional `status`
  - Returns paginated current-nurse orders.

- `GET /api/v1/me/nurse-profile/payment/orders/{id:guid}`
  - Name: `GetMyPaymentOrder`
  - Authorization: `.RequireAuthorization()`
  - Returns one current-nurse order or `404`.

- `POST /api/v1/me/nurse-profile/payment/orders/{id:guid}/cancel`
  - Name: `CancelMyPaymentOrder`
  - Authorization: `.RequireAuthorization()`
  - Cancels an owned pending-payment order.

### Admin Product Management

- `GET /api/v1/admin/payment/products`
  - Name: `AdminListPaymentProducts`
  - Authorization: `.RequirePermission(Permissions.Exams.View)`

- `GET /api/v1/admin/payment/products/{id:guid}`
  - Name: `AdminGetPaymentProduct`
  - Authorization: `.RequirePermission(Permissions.Exams.View)`

- `POST /api/v1/admin/payment/products`
  - Name: `AdminCreatePaymentProduct`
  - Authorization: `.RequirePermission(Permissions.Exams.Edit)`

- `PUT /api/v1/admin/payment/products/{id:guid}`
  - Name: `AdminUpdatePaymentProduct`
  - Authorization: `.RequirePermission(Permissions.Exams.Edit)`

- `POST /api/v1/admin/payment/products/{id:guid}/archive`
  - Name: `AdminArchivePaymentProduct`
  - Authorization: `.RequirePermission(Permissions.Exams.Edit)`

- `POST /api/v1/admin/payment/products/{id:guid}/restore`
  - Name: `AdminRestorePaymentProduct`
  - Authorization: `.RequirePermission(Permissions.Exams.Edit)`

No checkout, provider, webhook, paid, refund, subscription, export, or dashboard endpoints are included.

## DTO Security Rules

Payment DTOs must not expose:

- `UserId`
- Account internals.
- Roles or permissions.
- Password hashes.
- Access tokens, refresh tokens, token hashes, or internal auth state.
- Card data.
- Payment provider ids.
- Provider payment state.
- Provider secrets.
- Webhook payloads.
- EF/domain entities.
- Navigation objects.
- Nurse profile entities.
- Exam entities.
- `ExamAccessGrant` entities.

Payment DTOs may expose:

- Product ids and safe product display fields.
- Exam id and safe exam title where needed.
- Currency.
- Minor-unit amounts.
- Order id, status, totals, timestamps, and safe item snapshots.

## Money / Currency Rules

- Store currency as uppercase ISO 4217 code.
- Store all amounts in integer minor units.
- Do not store floating-point money values.
- Use `long` for minor-unit amounts in Domain/Application/EF.
- `Currency` must be exactly three ASCII letters after normalization.
- All order items in one order must share one currency.
- `CreatePaymentOrderRequest` contains only `ProductId`; clients do not send `Quantity` in Phase 8A.
- Server always creates exactly one order item with `Quantity = 1`.
- `LineTotalAmountMinor = UnitAmountMinor`.
- `TotalAmountMinor = LineTotalAmountMinor`.

## Product Lifecycle Rules

- New products may be created active or inactive, but only active products are purchasable.
- Product archive/deactivate sets `IsActive = false`.
- Product restore/reactivate sets `IsActive = true` only if the linked exam is still published.
- Existing orders retain product snapshots even when a product is updated, archived, or restored.
- Product hard delete is not implemented in Phase 8A.
- Product creation/update must validate that exam-access products link to an existing exam.
- Public catalog excludes inactive products and products linked to non-published exams.
- `PaymentProduct.Type` is immutable after creation.
- `PaymentProduct.ExamId` is immutable after creation.
- Admin product update may change only `Name`, `Description`, `Currency`, and `UnitAmountMinor`.
- Admin product update must not include `Type`, `ExamId`, or `IsActive`.
- Product active state must change only through archive/restore endpoints.

## Order Lifecycle Rules

- Created nurse orders start as `PendingPayment`.
- Created nurse orders set `ExpiresAt = CreatedAt + 30 minutes`.
- Created nurse orders set `PaidAt = null` and `CancelledAt = null`.
- `PendingPayment` may transition to `Cancelled` by the owning nurse.
- Lazy expiry: list/detail/cancel handlers may convert owned pending orders with `ExpiresAt <= now` to `Expired`.
- Cancelling an already expired pending order returns `409 Conflict` after lazy expiry is applied.
- `Paid` and `Failed` remain enum values only for future provider/webhook phases; no Phase 8A handler transitions orders to `Paid` or `Failed`.
- Terminal statuses are immutable in Phase 8A: `Paid`, `Failed`, `Cancelled`, and `Expired`.
- Cancelling a terminal order returns `409 Conflict`.
- Reading another nurse's order returns `404` to avoid existence disclosure.

## Price Snapshot Rules

- Order creation copies product name, type, exam id, currency, unit amount, quantity, and line total into `PaymentOrderItem`.
- Order totals are computed from item snapshots.
- Later product edits do not change existing orders.
- Product id remains stored for traceability, but order display must use snapshots.

## Exam Access Grant Decision

Phase 8A must not issue `ExamAccessGrant`.

Rationale:

- A local order is not proof of payment.
- Future access should be granted only after provider-confirmed payment, expected in a later checkout/webhook phase.
- Phase 8A may reference `ExamAccessGrant` only as the existing entitlement model that future Phase 8C payment confirmation can create.

## Checkout / Provider / Webhook Deferral

Phase 8A explicitly defers:

- Checkout sessions.
- Provider customers.
- Provider payment intents.
- Provider session ids.
- Provider status mapping.
- Webhook signature validation.
- Webhook event processing.
- Automatic order paid/failed transitions from providers.
- Automatic access grant issuance.

The model should remain future-compatible by preserving local product/order identities and price snapshots.

## Idempotency Decision

Phase 8A does not implement persisted idempotency keys.

Rationale:

- No external provider call occurs in Phase 8A.
- Duplicate local order creation is low risk because orders remain unpaid and do not grant access.
- A later checkout/provider phase should introduce idempotency alongside provider session creation.

The implementation plan must include a note that provider checkout work must revisit idempotency.

## Error Behavior

- Missing JWT: `401 Unauthorized`.
- Authenticated user without Nurse role or without nurse profile for nurse order operations: `403 Forbidden`.
- Admin product endpoints without required permission: `403 Forbidden`.
- Invalid request validation: `400 Bad Request`.
- Invalid GUID route/query binding: `400 Bad Request` through WebApi binding.
- Product not found: `404 Not Found`.
- Inactive product in nurse catalog/detail: `404 Not Found`.
- Inactive product used for order creation: `409 Conflict`.
- Product linked to missing, draft, or archived exam for order creation: `409 Conflict`.
- Product currency mismatch in order creation: `409 Conflict`.
- Cancelling non-owned order: `404 Not Found`.
- Cancelling non-pending order: `409 Conflict`.
- Unexpected exceptions: existing `500` Problem Details behavior.

## Migration Decision

Phase 8A is expected to create an EF migration named `AddPaymentProductsOrdersFoundation`.

Expected new Domain/EF types:

- `PaymentProduct`
- `PaymentProductType`
- `PaymentOrder`
- `PaymentOrderStatus`
- `PaymentOrderItem`

Expected new tables:

- `PaymentProducts`
- `PaymentOrders`
- `PaymentOrderItems`

Expected indexes:

- `PaymentProducts(Type, IsActive, ExamId, Name, Id)` for active product catalog queries.
- Unique `PaymentProducts(Type, ExamId)` for one exam-access product per exam in Phase 8A.
- `PaymentOrders(NurseProfileId, CreatedAt, Id)` for nurse order listing.
- `PaymentOrders(NurseProfileId, Status, CreatedAt, Id)` for status-filtered nurse order listing.
- `PaymentOrderItems(OrderId, Id)` for order detail loading.

Expected relationships:

- `PaymentProduct.ExamId -> Exams.Id` with restrict delete.
- `PaymentOrder.NurseProfileId -> NurseProfiles.Id` with restrict delete.
- `PaymentOrderItem.OrderId -> PaymentOrders.Id` with restrict delete.
- `PaymentOrderItem.ProductId -> PaymentProducts.Id` with restrict delete.

No permission seed migration is expected because Phase 8A reuses existing `Exams.*` permissions.

## Testing Requirements

Domain tests must cover:

- Default product type and active state.
- Money fields use integer minor units.
- Order total equals line item totals.
- Pending order cancellation and terminal immutability.

Application tests must cover:

- DTO reflection security.
- Validators for currency, amounts, ids, pagination, and status.
- Public product catalog returns only active products linked to published exams.
- Public product catalog is authenticated-only and does not require Nurse role, `NurseRoleGuard`, or permissions.
- Product detail hides inactive products from nurse catalog.
- Admin product create/update/archive/restore behavior.
- Admin product update request does not allow `Type`, `ExamId`, or `IsActive` mutation.
- Admin product update does not mutate existing order item snapshots.
- Archive/restore controls `IsActive`.
- Order creation requires Nurse role and nurse profile.
- Order creation rejects inactive products.
- Order creation rejects products linked to draft/archived/missing exams.
- Order creation snapshots price and product fields.
- Order creation accepts only `ProductId` from the client.
- Order creation creates one item with `Quantity = 1`.
- Order creation sets order total equal to the product unit amount snapshot.
- Order creation sets `ExpiresAt` to `CreatedAt + 30 minutes`.
- Order list/detail are nurse-owned.
- Cancel allows only owned pending orders.
- Expired pending orders are treated as expired by read/list/cancel workflows.
- List/detail lazily expire past-due pending orders.
- Cancel on a past-due pending order applies lazy expiry first and returns `409 Conflict`.
- No handler creates `ExamAccessGrant`.

Infrastructure tests must cover:

- Table names and primary keys.
- Enum conversion to string with max length.
- Required currency and amount column shapes.
- Precision-free integer money storage.
- Unique product index for exam-access product per exam.
- Nurse order listing indexes.
- Restrict delete relationships.
- Migration/model consistency.

WebApi integration tests must cover:

- Product/order endpoints require authentication.
- Public product catalog endpoints use `.RequireAuthorization()` only.
- Public product catalog endpoints do not require `.RequirePermission(...)`, `NurseRoleGuard`, or nurse role setup.
- Admin product endpoints require exact existing `Exams.View` or `Exams.Edit` permissions.
- Nurse product endpoints do not require admin permissions.
- Query binding and route binding.
- Validation Problem Details.
- Forbidden and conflict behavior.
- Raw JSON does not expose forbidden fields, provider ids/state, card data, account internals, or EF/domain navigation objects.

## Acceptance Criteria

- Spec and implementation plan exist for Phase 8A.
- No implementation starts during planning.
- Phase 8A is backend-only payment products and local orders foundation.
- Exam-access products are linked to existing exams.
- Money is stored as ISO currency plus integer minor-unit amounts.
- Active products only are purchasable.
- Nurses can create and view only own local pending-payment orders.
- Order items snapshot product and price fields.
- No checkout/provider/webhook behavior is implemented.
- No automatic `ExamAccessGrant` is issued.
- Admin product management uses existing `Exams.View` and `Exams.Edit` permissions.
- A migration is expected for payment product/order tables.
- No frontend, subscriptions, refunds, exports, revenue reports, Phase 9 dashboards, recruitment changes, or nurse exam contract changes are included.

## Reviewer Decisions Needed

None. This spec locks the safe Phase 8A defaults:

- Exam-access products only.
- Local pending-payment orders only.
- Existing `Exams.*` permissions for product management.
- No provider integration.
- No automatic exam access grants.
- Expected migration: `AddPaymentProductsOrdersFoundation`.
