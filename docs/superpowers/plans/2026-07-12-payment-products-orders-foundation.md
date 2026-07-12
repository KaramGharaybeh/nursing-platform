# Phase 8A Payment Products & Orders Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement backend-only payment products and local nurse-owned pending-payment orders for purchasable exam access.

**Architecture:** Phase 8A adds a Payments module across Domain, Application, Infrastructure, and WebApi. Domain owns product/order entities and lifecycle helpers; Application owns CQRS handlers, validation, nurse ownership, admin product rules, and DTOs; Infrastructure owns EF configuration and migration; WebApi maps thin Minimal API endpoints with existing authorization patterns.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 8A Payment Products & Orders Foundation.
- Implement the approved spec exactly: `docs/superpowers/specs/2026-07-12-payment-products-orders-foundation.md`.
- Do not add frontend.
- Do not implement checkout.
- Do not integrate real payment providers.
- Do not implement webhooks.
- Do not issue `ExamAccessGrant` from Phase 8A orders.
- Do not add subscriptions, refunds, coupons, invoices, taxes, discounts, wallets, payouts, revenue reports, CSV exports, or Excel exports.
- Do not add Phase 9 dashboards or reports.
- Do not modify recruitment, contact-request, candidate, or employer workflows.
- Do not modify nurse exam-taking, result, review, or analytics contracts.
- Do not expose card data, provider ids, provider state, provider secrets, account internals, roles, permissions, tokens, password hashes, EF/domain entities, or navigation objects in DTOs.
- Store money as ISO 4217 currency plus integer minor-unit amount; do not use floating money values.
- Created nurse orders start as `PendingPayment`.
- New orders set `ExpiresAt = CreatedAt + 30 minutes`, `PaidAt = null`, and `CancelledAt = null`.
- Existing order item snapshots must not mutate when products change.
- Public product catalog endpoints are authenticated-only, not nurse-role-only.
- Public product catalog endpoints use `.RequireAuthorization()` only and do not require `.RequirePermission(...)` or `NurseRoleGuard`.
- Order endpoints under `/api/v1/me/nurse-profile/payment/orders` enforce Nurse role and current `NurseProfile` ownership in Application handlers.
- `PaymentProduct.Type` is immutable after creation.
- `PaymentProduct.ExamId` is immutable after creation.
- Admin product update may change only `Name`, `Description`, `Currency`, and `UnitAmountMinor`.
- Product active state changes only through archive/restore endpoints.
- `CreatePaymentOrderRequest` contains only `ProductId`; clients do not send quantity.
- Server creates exactly one order item with `Quantity = 1`.
- Admin product management uses `Permissions.Exams.View` for reads and `Permissions.Exams.Edit` for create/update/archive/restore.
- No new payment permissions or permission seed migration in Phase 8A.
- Migration expected: `AddPaymentProductsOrdersFoundation`.

---

## Chosen Implementation Batch Scope

Phase 8A is one backend-only batch because product, order, price snapshot, and local lifecycle behavior must land together to create a useful foundation for future checkout. It includes exam-access products, admin product management, authenticated product catalog, nurse-owned local pending-payment orders, cancellation, EF persistence, migration, and tests. It excludes checkout, providers, webhooks, grants, subscriptions, refunds, exports, dashboards, frontend, and recruitment work.

## Planned File Structure

Domain:

- Create `backend/src/NursingPlatform.Domain/Payments/PaymentProductType.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentOrderStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentProduct.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentOrder.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentOrderItem.cs`.

Application:

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose payment DbSets and any atomic order status helper if needed.
- Create `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentProductDto.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentOrderDto.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentOrderItemDto.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Common/PaymentMoneyValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Queries/ListPaymentProducts/*`.
- Create `backend/src/NursingPlatform.Application/Payments/Queries/GetPaymentProduct/*`.
- Create `backend/src/NursingPlatform.Application/Payments/Commands/CreateMyPaymentOrder/*`.
- Create `backend/src/NursingPlatform.Application/Payments/Queries/ListMyPaymentOrders/*`.
- Create `backend/src/NursingPlatform.Application/Payments/Queries/GetMyPaymentOrder/*`.
- Create `backend/src/NursingPlatform.Application/Payments/Commands/CancelMyPaymentOrder/*`.
- Create `backend/src/NursingPlatform.Application/Payments/Admin/Products/AdminPaymentProductOperations.cs`.

Infrastructure:

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs`.
- Generate migration `AddPaymentProductsOrdersFoundation`.
- Update model snapshot through EF generation only.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`.

Tests:

- Create `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentValidatorTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs`.

Tracking:

- Modify `CURRENT_TASK.md` and `TASKS.md` only after implementation verification passes.

## Domain Entities And Enums

### PaymentProductType

- `ExamAccess`

### PaymentOrderStatus

- `PendingPayment`
- `Paid`
- `Failed`
- `Cancelled`
- `Expired`

### PaymentProduct

- `Guid Id`
- `PaymentProductType Type`
- `Guid ExamId`
- `string Name`
- `string? Description`
- `string Currency`
- `long UnitAmountMinor`
- `bool IsActive`
- `Exam Exam`
- audit fields

### PaymentOrder

- `Guid Id`
- `Guid NurseProfileId`
- `PaymentOrderStatus Status`
- `string Currency`
- `long TotalAmountMinor`
- `DateTime? ExpiresAt`
- `DateTime? PaidAt`
- `DateTime? CancelledAt`
- `NurseProfile NurseProfile`
- `ICollection<PaymentOrderItem> Items`
- audit fields

### PaymentOrderItem

- `Guid Id`
- `Guid OrderId`
- `Guid ProductId`
- `string ProductNameSnapshot`
- `PaymentProductType ProductTypeSnapshot`
- `Guid ExamIdSnapshot`
- `string Currency`
- `long UnitAmountMinor`
- `int Quantity`
- `long LineTotalAmountMinor`
- `PaymentOrder Order`
- `PaymentProduct Product`
- audit fields

## Migration Plan

Expected migration name: `AddPaymentProductsOrdersFoundation`.

Expected tables:

- `PaymentProducts`
- `PaymentOrders`
- `PaymentOrderItems`

Expected EF configuration:

- Store enums as strings with max length 32.
- `PaymentProduct.Name` required max length 200.
- `PaymentProduct.Description` max length 1000.
- `PaymentProduct.Currency` required max length 3.
- `PaymentOrder.Currency` required max length 3.
- `PaymentOrderItem.ProductNameSnapshot` required max length 200.
- `PaymentOrderItem.Currency` required max length 3.
- All amount fields are `long`.
- Restrict delete relationships.
- Unique index on `PaymentProducts(Type, ExamId)`.
- Catalog index on `PaymentProducts(Type, IsActive, ExamId, Name, Id)`.
- Order indexes on `PaymentOrders(NurseProfileId, CreatedAt, Id)` and `PaymentOrders(NurseProfileId, Status, CreatedAt, Id)`.
- Item index on `PaymentOrderItems(OrderId, Id)`.

No payment permission seed changes are expected.

## Task 1: Domain Entities, Enums, And Domain Tests

**Goal:** Add payment domain types and prove lifecycle/money invariants without Application, EF, or WebApi changes.

**Files:**

- Create Domain files listed in Planned File Structure.
- Test `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs`.

**Tests to write first:**

- `PaymentProduct_DefaultType_IsExamAccess`
- `PaymentProduct_DefaultActiveState_IsTrue`
- `PaymentProduct_TypeAndExamId_AreImmutableAfterCreation`
- `PaymentProduct_UsesMinorUnitAmount`
- `PaymentOrder_CreatePending_SetsPendingPaymentStatusAndTotal`
- `PaymentOrder_CreatePending_SetsExpiresAtToCreatedAtPlusThirtyMinutes`
- `PaymentOrder_CancelPending_SetsCancelledStatusAndTimestamp`
- `PaymentOrder_CancelTerminalStatus_ThrowsInvalidOperationException`
- `PaymentOrder_ExpirePastDuePending_SetsExpiredStatus`
- `PaymentOrderItem_CreateSnapshot_CopiesProductFieldsAndLineTotal`
- `PaymentOrderItem_CreateSnapshot_UsesQuantityOneAndUnitAmountAsLineTotal`

**Implementation notes:**

- Add static factory helpers only if they reduce duplication and keep domain simple.
- Do not add provider fields.
- Do not add card fields.
- Do not add `ExamAccessGrant` references.
- Keep Domain free of EF and WebApi dependencies.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "Payment"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if the domain model appears to require provider state, checkout state, or access grant issuance.

## Task 2: Application DTOs, Requests, Queries, Validators, And DTO Security Tests

**Goal:** Add safe Application contracts and validation for products and orders.

**Files:**

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`.
- Create Application DTO, request, query, command, validator, and common files listed in Planned File Structure.
- Test `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentValidatorTests.cs`.

**Contracts to create:**

- `PaymentProductDto`
- `PaymentOrderDto`
- `PaymentOrderItemDto`
- `CreatePaymentOrderRequest` with only `ProductId`.
- `CreateMyPaymentOrderCommand`
- `ListPaymentProductsQuery`
- `GetPaymentProductQuery`
- `ListMyPaymentOrdersQuery`
- `GetMyPaymentOrderQuery`
- `CancelMyPaymentOrderCommand`
- Admin product requests/commands/queries in `AdminPaymentProductOperations.cs`.

**Tests to write first:**

- `PaymentDtos_ShouldNotExposeAccountInternalsProviderFieldsCardDataOrEntities`
- `PaymentDtos_ShouldExposeOnlySafeProductOrderAndSnapshotFields`
- `Validate_Product_WithInvalidCurrency_ShouldHaveError`
- `Validate_Product_WithNonPositiveAmount_ShouldHaveError`
- `Validate_Product_WithEmptyExamId_ShouldHaveError`
- `Validate_CreateOrder_WithEmptyProductId_ShouldHaveError`
- `Validate_CreateOrder_RequestContainsOnlyProductId`
- `Validate_ListProducts_WithInvalidPagination_ShouldHaveError`
- `Validate_ListOrders_WithInvalidPagination_ShouldHaveError`
- `Validate_ListOrders_WithInvalidStatus_ShouldHaveError`

**Implementation notes:**

- Currency normalization is uppercase invariant culture.
- Validators reject non-three-letter currency values.
- Validators reject amount `<= 0`.
- Validators reject empty GUIDs.
- Pagination uses `Page >= 1` and `PageSize` between `1` and `100`.
- Do not add arbitrary quantity validation because Phase 8A clients do not send quantity.
- DTO reflection tests must reject `UserId`, `PasswordHash`, `Roles`, `Permissions`, tokens, provider ids, provider state, card fields, `PaymentProduct`, `PaymentOrder`, `PaymentOrderItem`, `NurseProfile`, `Exam`, `ExamAccessGrant`, and navigation objects.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PaymentDto|PaymentValidator"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if validation requires checkout/provider semantics.

## Task 3: Application Product And Order Handlers

**Goal:** Implement product catalog, admin product management, nurse-owned order creation, order list/detail, cancellation, and lazy pending-order expiry.

**Files:**

- Create handler files under `backend/src/NursingPlatform.Application/Payments/`.
- Test `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.

**Tests to write first:**

- `Handle_ListProducts_ReturnsOnlyActiveProductsLinkedToPublishedExams`
- `Handle_GetProduct_WhenInactive_ReturnsNotFoundForNurseCatalog`
- `Handle_AdminCreateProduct_WithPublishedExam_CreatesProduct`
- `Handle_AdminCreateProduct_WithDraftOrArchivedExam_ThrowsInvalidOperationException`
- `Handle_AdminUpdateProduct_UpdatesSafeFieldsWithoutMutatingOrders`
- `Handle_AdminUpdateProduct_CannotMutateTypeOrExamId`
- `Handle_AdminUpdateProduct_DoesNotChangeIsActive`
- `Handle_AdminUpdateProduct_DoesNotMutateExistingOrderItemSnapshots`
- `Handle_AdminArchiveProduct_SetsInactive`
- `Handle_AdminRestoreProduct_WithPublishedExam_SetsActive`
- `Handle_CreateOrder_WhenNurseProfileMissing_ThrowsForbiddenAccessException`
- `Handle_CreateOrder_WithInactiveProduct_ThrowsInvalidOperationException`
- `Handle_CreateOrder_WithProductLinkedToUnpublishedExam_ThrowsInvalidOperationException`
- `Handle_CreateOrder_CreatesPendingPaymentOrderWithPriceSnapshot`
- `Handle_CreateOrder_CreatesOneItemWithQuantityOne`
- `Handle_CreateOrder_TotalEqualsProductUnitAmountSnapshot`
- `Handle_CreateOrder_SetsExpiresAtToCreatedAtPlusThirtyMinutes`
- `Handle_CreateOrder_DoesNotCreateExamAccessGrant`
- `Handle_ListOrders_ReturnsOnlyCurrentNurseOrders`
- `Handle_ListOrders_LazilyExpiresPastDuePendingOrders`
- `Handle_GetOrder_WhenOwned_ReturnsSnapshotItems`
- `Handle_GetOrder_WhenNotOwned_ThrowsKeyNotFoundException`
- `Handle_GetOrder_LazilyExpiresPastDuePendingOrder`
- `Handle_CancelOrder_WhenOwnedPending_SetsCancelled`
- `Handle_CancelOrder_WhenPastDuePending_ExpiresFirstAndThrowsInvalidOperationException`
- `Handle_CancelOrder_WhenTerminal_ThrowsInvalidOperationException`

**Implementation notes:**

- Use `NurseRoleGuard` for nurse order handlers.
- Resolve current `NurseProfile.Id`; missing profile throws `ForbiddenAccessException`.
- Admin product handlers rely on endpoint permissions and do not need role guards.
- Admin product update request must not include `Type`, `ExamId`, or `IsActive`, making Type/ExamId mutation impossible through update.
- Product active state must be changed only by archive/restore handlers.
- Product catalog handlers return active products linked to `ExamStatus.Published`.
- Order creation starts from current nurse ownership.
- Order creation request contains only `ProductId`.
- Order creation supports one product id, creates exactly one item with `Quantity = 1`, sets `LineTotalAmountMinor = UnitAmountMinor`, and sets `TotalAmountMinor = LineTotalAmountMinor`.
- Order creation sets `Status = PendingPayment`, `ExpiresAt = CreatedAt + 30 minutes`, `PaidAt = null`, and `CancelledAt = null`.
- Lazy expiry can be applied by list/detail/cancel handlers when `ExpiresAt <= DateTime.UtcNow`.
- Cancelling a past-due pending order applies lazy expiry first and returns conflict through `InvalidOperationException`.
- `Paid` and `Failed` remain enum values only for future provider/webhook phases; no Phase 8A handler transitions orders to `Paid` or `Failed`.
- Do not create or update `ExamAccessGrants`.
- Do not add a paid transition handler in Phase 8A.
- Do not expose non-owned order existence.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PaymentHandler|Payment"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if order behavior requires provider state, automatic grants, or checkout sessions.

## Task 4: Infrastructure EF Configuration, Migration, And Tests

**Goal:** Persist payment products, orders, and order items with correct constraints, indexes, enum conversion, and migration.

**Files:**

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs`.
- Generate EF migration `AddPaymentProductsOrdersFoundation`.
- Update model snapshot through EF generation only.
- Test `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs`.

**Tests to write first:**

- `PaymentConfiguration_UsesExpectedTableNamesAndPrimaryKeys`
- `PaymentConfiguration_StoresEnumsAsStringsWithMaxLength`
- `PaymentConfiguration_StoresMoneyAsLongMinorUnits`
- `PaymentConfiguration_ConfiguresProductCatalogIndexes`
- `PaymentConfiguration_ConfiguresUniqueExamAccessProductIndex`
- `PaymentConfiguration_ConfiguresOrderOwnershipIndexes`
- `PaymentConfiguration_ConfiguresOrderItemIndex`
- `PaymentConfiguration_ConfiguresRestrictDeleteRelationships`
- `PaymentConfiguration_DoesNotAddProviderColumns`

**Implementation notes:**

- Use `IEntityTypeConfiguration<T>` classes in `PaymentConfigurations.cs`.
- Use `.HasConversion<string>()` for enums.
- Use `.HasMaxLength(3)` for currency.
- Use `.HasMaxLength(200)` for names and snapshots.
- Use `.HasMaxLength(1000)` for descriptions.
- Configure all relationships with `DeleteBehavior.Restrict`.
- Generate migration with EF only.
- Do not add permission seed changes.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PaymentConfiguration"`
- `dotnet build backend/NursingPlatform.slnx`
- `dotnet ef migrations add AddPaymentProductsOrdersFoundation --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if EF migration introduces permission seed changes, provider fields, or unexpected schema beyond payment products/orders/items.

## Task 5: WebApi Endpoints And Integration Tests

**Goal:** Add approved Phase 8A endpoints and prove auth, permissions, query binding, errors, and raw JSON security.

**Files:**

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`.
- Test `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs`.

**Endpoints to add:**

- `GET /api/v1/payment/products`
- `GET /api/v1/payment/products/{id:guid}`
- `POST /api/v1/me/nurse-profile/payment/orders`
- `GET /api/v1/me/nurse-profile/payment/orders`
- `GET /api/v1/me/nurse-profile/payment/orders/{id:guid}`
- `POST /api/v1/me/nurse-profile/payment/orders/{id:guid}/cancel`
- `GET /api/v1/admin/payment/products`
- `GET /api/v1/admin/payment/products/{id:guid}`
- `POST /api/v1/admin/payment/products`
- `PUT /api/v1/admin/payment/products/{id:guid}`
- `POST /api/v1/admin/payment/products/{id:guid}/archive`
- `POST /api/v1/admin/payment/products/{id:guid}/restore`

**Authorization:**

- Public product catalog endpoints are authenticated-only, not nurse-role-only, and use `.RequireAuthorization()` only.
- Public product catalog endpoints do not require `.RequirePermission(...)`.
- Public product catalog endpoints do not require `NurseRoleGuard`.
- Order endpoints under `/api/v1/me/nurse-profile/payment/orders` use `.RequireAuthorization()` and rely on Application handlers for Nurse role and current `NurseProfile` ownership.
- Admin product reads use `.RequirePermission(Permissions.Exams.View)`.
- Admin product writes use `.RequirePermission(Permissions.Exams.Edit)`.
- Do not add `.AllowAnonymous()`.

**Tests to write first:**

- `PaymentEndpoints_WithoutJwt_ReturnUnauthorized`
- `PaymentProductCatalog_UseRequireAuthorizationOnly_WithoutPermissionSetup`
- `PaymentProductCatalog_DoesNotRequireNurseRole`
- `AdminPaymentProductRead_RequiresExamsViewPermission`
- `AdminPaymentProductWrite_RequiresExamsEditPermission`
- `CreateOrder_WithValidRequest_SendsCommand`
- `CreateOrder_RequestContainsOnlyProductId`
- `ListOrders_WithPaginationAndStatus_SendsQuery`
- `GetOrder_WhenHidden_ReturnsNotFound`
- `CancelOrder_WithConflict_ReturnsConflict`
- `PaymentValidationFailure_ReturnsValidationProblemDetails`
- `PaymentEndpoints_WithInvalidGuid_ReturnBadRequestAndSenderNotCalled`
- `PaymentJson_DoesNotExposeProviderFieldsCardDataAccountInternalsOrEntities`

**Implementation notes:**

- Keep WebApi thin.
- Bind route ids as `Guid`.
- Bind query status as `PaymentOrderStatus?` if existing enum binding is adequate; otherwise use string parsing and let Application validation handle invalid status.
- Raw JSON tests must inspect response content before DTO deserialization.
- Do not add checkout, paid, failed, refund, provider, grant, export, or dashboard endpoints.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "Payment"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if endpoint implementation requires provider state, checkout, grants, or new permissions.

## Task 6: Final Verification And Security Review

**Goal:** Verify Phase 8A end-to-end and confirm no out-of-scope behavior was added.

**Review checklist:**

- Confirm no frontend files changed.
- Confirm no provider, checkout, webhook, subscription, refund, export, revenue report, or dashboard files/endpoints were added.
- Confirm no recruitment/contact-request/candidate/employer files changed.
- Confirm no nurse exam-taking/result/review/analytics contracts changed.
- Confirm no `ExamAccessGrant` is created by order handlers.
- Confirm no provider ids/state or card data appear in DTOs.
- Confirm public product catalog endpoints are authenticated-only, not nurse-role-only.
- Confirm order endpoints enforce Nurse role and nurse profile ownership in Application handlers.
- Confirm admin update cannot mutate product `Type`, `ExamId`, or `IsActive`.
- Confirm create order uses `ProductId` only, creates `Quantity = 1`, and totals from the product unit amount snapshot.
- Confirm pending orders use `ExpiresAt = CreatedAt + 30 minutes` and lazy expiry behavior.
- Confirm `Paid` and `Failed` are future enum values only and no Phase 8A handler transitions to them.
- Confirm admin product endpoints use existing `Exams.View`/`Exams.Edit` only.
- Confirm migration includes only payment products/orders/items and expected model snapshot updates.

**Verification commands:**

- `dotnet build backend/NursingPlatform.slnx`
- `dotnet test backend/NursingPlatform.slnx`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --stat`
- `git diff --cached --stat`

**Stop condition:** Stop if build, tests, EF model check, security review, or scope review fails after two fix cycles.

## Task 7: Tracking Documentation Update

**Goal:** Update project tracking only after Phase 8A implementation and final verification pass.

**Files:**

- Modify `CURRENT_TASK.md`.
- Modify `TASKS.md`.

**Implementation notes:**

- Mark only Phase 8A products/orders foundation complete.
- In `TASKS.md`, mark Products and Orders complete only if the implementation covers them.
- Do not mark Checkout, Payment providers, or Webhooks complete.
- Do not start Phase 9.
- Do not mention frontend, provider integration, webhooks, subscriptions, refunds, exports, or dashboards as implemented.

**Verification commands:**

- `git diff -- CURRENT_TASK.md TASKS.md`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if tracking docs imply Phase 8 checkout/providers/webhooks or Phase 9 work is complete.

## Task 8: Final Commit

**Goal:** Commit approved Phase 8A implementation, migration, tests, and tracking files only after explicit approval or approved batch execution.

**Files:**

- Stage only approved Phase 8A files explicitly.
- Do not stage `codex_res/codex_report.md`.
- Do not stage `AGENTS.md`, `PROJECT_RULES.md`, `.gitignore`, frontend files, recruitment files, provider/checkout/webhook files, exports, dashboards, or unrelated docs.

**Commit strategy:**

- Planning commit, if requested separately: `docs: add payment products orders plan`.
- Implementation commit: `feat: add payment products orders foundation`.

**Verification commands:**

- `git status --short --untracked-files=all`
- `git diff --cached --name-only`
- `git diff --cached --stat`
- `git commit -m "feat: add payment products orders foundation"`
- `git status --short --untracked-files=all`
- `git log -1 --oneline`

**Stop condition:** Stop after reporting commit hash/message, post-commit status, and latest log line. Do not proceed to checkout, providers, webhooks, grants, refunds, frontend, exports, dashboards, recruitment, or Phase 9 work.

## Explicit Out-of-Scope Guardrails

- No frontend.
- No checkout.
- No real payment provider integration.
- No provider ids or provider state.
- No card data.
- No webhooks.
- No automatic exam access grant issuance.
- No subscriptions.
- No refunds.
- No coupons, invoices, taxes, discounts, wallets, payouts, or revenue reports.
- No CSV, Excel, or export endpoints.
- No Phase 9 dashboard/reporting UI.
- No recruitment changes.
- No nurse exam-taking/result/review/analytics contract changes.
- No new payment permissions or permission seed migration in Phase 8A.

## Self-Review Checklist

- Spec coverage: Plan covers products, orders, money, lifecycle, permissions, migration, tests, verification, tracking, and commit.
- Scope check: Plan is Phase 8A backend-only products and local orders foundation.
- Permission check: Plan reuses `Exams.View` and `Exams.Edit`; it does not add payment permissions.
- Migration check: Plan expects only payment products/orders/items schema and no permission seed changes.
- Security check: Plan excludes provider ids/state, card data, account internals, EF/domain serialization, and automatic grants.
- Placeholder scan: No unresolved placeholders are intentionally left in this plan.
