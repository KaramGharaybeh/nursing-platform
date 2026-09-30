# Preparation Package Stage 2 Fulfillment Entitlements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Plan the backend-only Stage 2 Preparation Package payment integration, purchased-offer snapshots, idempotent fulfillment, package purchase entitlements, and four benefit rights described in `docs/superpowers/specs/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md`.

**Architecture:** Stage 2 extends the existing payment/order/checkout/Sandbox fulfillment path additively while preserving standalone `PaymentProductType.ExamAccess`, `PaymentOrder`, `PaymentOrderItem`, and `ExamAccessGrant` behavior. New package entitlement and benefit-right behavior lives in Domain/Application/Infrastructure with thin Minimal API mappings; authorization for package benefits uses entitlement/right state, not live payment lookups.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core Code-First migrations, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi integration-test factory and JWT helpers.

---

## Status

Approved — Stage 2 Implementation Plan

## Purpose

This document is an implementation plan only. It organizes the approved Stage 2 Preparation Package fulfillment, entitlements, and benefit-rights specification into reviewable backend implementation slices, but it does not authorize execution, source-code changes, test changes, migrations, staging, committing, pushing, deployment, or beginning Stage 2 implementation.

Any implementation must be separately and explicitly approved after this plan is reviewed. This plan also does not authorize Stage 3 package-attempt start/session provenance, Stage 4 report generation/access, frontend work, carts, subscriptions, refunds, employer package access, or any future-roadmap feature.

## Inputs

Authoritative inputs for this plan:

- `docs/superpowers/specs/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md`
- `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`
- `docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md`
- `docs/superpowers/plans/2026-07-26-preparation-package-stage-1-catalog-authoring.md`
- Current Stage 1 implementation through `8aa5d50 docs(api): add preparation package openapi metadata`
- `docs/product/vision.md`
- `docs/architecture/system-architecture.md`
- `docs/backend/backend-architecture.md`
- `docs/frontend/frontend-architecture.md`
- `docs/database/database-design.md`
- `docs/api/api-design.md`
- `docs/standards/engineering-standards.md`
- `docs/development/model-orchestration.md`
- `PROJECT_RULES.md`
- `CURRENT_TASK.md`
- `TASKS.md`
- `README.md`
- `AGENTS.md`

The approved Stage 2 specification remains authoritative for Stage 2 scope. The approved umbrella and Stage 1 specifications remain authoritative for cross-stage invariants and Stage 1 baseline behavior. This plan must not introduce new business decisions.

## Scope Lock

- Execute only Stage 2 payment order item integration, purchased-offer snapshots, idempotent fulfillment, package purchase entitlements, four benefit rights, access-window rules, active entitlement/repurchase checks, historical buyer protection, Stage 2 nurse-owned entitlement visibility, and benefit-right authorization helpers.
- Implement the approved Stage 2 spec exactly: `docs/superpowers/specs/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md`.
- Preserve Stage 1 catalog/authoring behavior. Do not rework package definition, package version, offer, reporting-topic, reporting-profile, material, practice collection, or catalog semantics except where Stage 2 needs purchased-offer snapshot reads.
- Preserve existing standalone paid-exam, free-exam, grant-authorized session start, scoring, review, attempt history, analytics, `isFree`, and `canStart` behavior.
- Package fulfillment must not create `ExamAccessGrant` rows.
- Package offers, package entitlements, package attempts, and package benefit rights must not participate in the standalone effective-paid classification rule: `Exam.IsFree == false OR active positive-price ExamAccess product exists`.
- Extend `POST /api/v1/me/nurse-profile/payment/orders` additively to accept exactly one source: existing `productId` for standalone exam access or new `packageOfferId` for Preparation Package offers.
- Requests containing neither source or both sources are validation errors.
- Existing `productId` behavior and response fields remain backward compatible.
- Existing checkout start endpoint remains the checkout entry point for both standalone and package orders.
- Existing Development/Test Sandbox completion endpoint remains the completion entry point and may return additive package fulfillment summary fields while preserving `grantedExamIds`.
- Stage 2 may expose nurse-owned package entitlement list/detail/status endpoints only. Do not expose materials runtime, practice runtime, package-attempt start, attempt consumption, report generation, report access, report retry, report guidance, employer package data, carts, subscriptions, sponsored packages, adaptive practice, AI recommendations, refunds, coupons, taxes, invoices, wallets, payouts, or production provider reconciliation.
- Entitlement and right DTOs must not expose provider secrets, provider raw payloads, payment tokens, access tokens, refresh tokens, password hashes, protected exam content, answer identifiers, correct answers, answer keys, protected options, rationales, internal scoring logic, internal authorization state, EF/domain navigation objects, or stack traces.
- EF Core migrations are allowed only during a separately approved implementation session, not while authoring or reviewing this plan.
- Do not modify `CURRENT_TASK.md`, `TASKS.md`, approved specs, Stage 1 plan, frontend/design files, `.agent/goal-state.md`, `README`, or `CHANGELOG` unless separately and explicitly instructed.
- Do not stage files, commit, push, reset, clean, stash, or use `git add .` unless separately and explicitly authorized.

This plan proposes one potential backend feature batch split into the seven reviewable slices below, pending separate explicit implementation authorization. The planned batch is still not commercially launchable because package-attempt session creation and analytical reports remain Stage 3 and Stage 4 work.

## Existing Codebase Reconnaissance

Existing project patterns and paths to follow during implementation:

Domain:

- `backend/src/NursingPlatform.Domain/Common/AuditableEntity.cs` — existing auditable base entity convention.
- `backend/src/NursingPlatform.Domain/Payments/PaymentProduct.cs` — standalone exam-access product model.
- `backend/src/NursingPlatform.Domain/Payments/PaymentProductType.cs` — currently contains standalone product type semantics; do not overload `ExamAccess` for packages.
- `backend/src/NursingPlatform.Domain/Payments/PaymentOrder.cs` — nurse-owned order aggregate and `CreatePending(...)` pattern.
- `backend/src/NursingPlatform.Domain/Payments/PaymentOrderItem.cs` — existing standalone order-item snapshot pattern.
- `backend/src/NursingPlatform.Domain/Exams/ExamAccessGrant.cs` — standalone paid-exam authorization evidence to preserve.
- `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageDefinition.cs` — stable package identity used for same-package repurchase.
- `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageVersion.cs` — immutable composition source for purchased package snapshots.
- `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageOffer.cs` — Stage 1 sellable offer carrying price, currency, access duration, title, slug, summary, status.

Application:

- `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` — DbSet and transaction abstraction pattern.
- `backend/src/NursingPlatform.Application/Nurses/Common/NurseRoleGuard.cs` — current-nurse ownership resolution pattern.
- `backend/src/NursingPlatform.Application/Payments/Commands/CreateMyPaymentOrder/CreateMyPaymentOrderCommand.cs` — existing order creation and validation path.
- `backend/src/NursingPlatform.Application/Payments/Commands/StartMyPaymentCheckout/StartMyPaymentCheckoutCommand.cs` — checkout idempotency and request fingerprint path to preserve.
- `backend/src/NursingPlatform.Application/Payments/Commands/CompleteSandboxPaymentCheckout/CompleteSandboxPaymentCheckoutCommand.cs` — current atomic paid transition plus standalone fulfillment path to generalize item-by-item.
- `backend/src/NursingPlatform.Application/Payments/Common/PaymentHandlerHelpers.cs` — current nurse helper.
- `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs` — DTO mapping pattern.
- `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentOrderDto.cs`, `PaymentOrderItemDto.cs`, and `PaymentCompletionDto.cs` — additive response extensions.
- `backend/src/NursingPlatform.Application/PreparationPackages/Catalog/PreparationPackageCatalogOperations.cs` — package offer/catalog projection patterns.
- `backend/src/NursingPlatform.Application/PreparationPackages/DTOs/PreparationPackageDtos.cs` — Stage 1 DTO conventions and safe response patterns.
- `backend/src/NursingPlatform.Application/Authorization/Permissions.cs` — static permission constant pattern.

Infrastructure:

- `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs` — transaction, `ExecutePaymentOrderPaidTransitionAsync(...)`, unique-violation helper, DbSet exposure, and audit behavior.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs` — payment table/index/relationship style.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs` — Stage 1 package persistence style.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` — permission seed pattern.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` — EF migration location and naming pattern.

WebApi:

- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` — existing payment/nurse-profile route mappings, no-store headers, Development/Test Sandbox route gating.
- `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` — Stage 1 preparation package endpoint-group style and OpenAPI metadata helpers.
- `backend/src/NursingPlatform.WebApi/Extensions/AuthorizationExtensions.cs` — `RequirePermission(...)` extension.

Tests:

- `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs` — existing payment domain tests.
- `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PreparationPackageDomainTests.cs` — Stage 1 domain tests.
- `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs` — order, checkout, completion, idempotency, and grant-fulfillment handler tests.
- `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs` and `PaymentValidatorTests.cs` — DTO/validator patterns.
- `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageHandlerTests.cs` — Stage 1 package handler test setup.
- `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs` — payment EF configuration tests.
- `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs` — Stage 1 EF configuration tests.
- `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/SandboxPaymentCompletionPostgreSqlTests.cs` — PostgreSQL transaction/concurrency compatibility tests.
- `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs` — payment endpoint auth/security/no-store tests.
- `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PreparationPackageCatalogEndpointTests.cs` and `AdminPreparationPackageEndpointTests.cs` — Stage 1 endpoint patterns.

Planned file areas for Stage 2 implementation:

Domain:

- Modify `backend/src/NursingPlatform.Domain/Payments/PaymentOrderItem.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentOrderItemSourceType.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackagePurchaseEntitlement.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackagePurchaseEntitlementStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRight.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRightType.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRightStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageOrderItemSnapshot.cs` as the dedicated one-to-one purchased-offer snapshot entity for package order items.

Application:

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CreateMyPaymentOrder/CreatePaymentOrderRequest.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CreateMyPaymentOrder/CreateMyPaymentOrderCommand.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CompleteSandboxPaymentCheckout/CompleteSandboxPaymentCheckoutCommand.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentOrderItemDto.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentCompletionDto.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/DTOs/PackageEntitlementDtos.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/ListMyPackageEntitlements/ListMyPackageEntitlementsQuery.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/GetMyPackageEntitlement/GetMyPackageEntitlementQuery.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Fulfillment/PackageFulfillmentService.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Fulfillment/PackageFulfillmentResult.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Fulfillment/UnsafePackageFulfillmentException.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Authorization/PackageBenefitAuthorizationService.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Authorization/PackageBenefitAuthorizationResult.cs`.

Infrastructure:

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`.
- Create one Stage 2 migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` during approved implementation.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` for additive payment DTO metadata only if needed by existing route metadata conventions.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` to add only Stage 2 nurse-owned entitlement list/detail endpoints and OpenAPI metadata.

Tests:

- Modify `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs`.
- Modify `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PreparationPackageDomainTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentValidatorTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageEntitlementHandlerTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageFulfillmentServiceTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageBenefitAuthorizationTests.cs`.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs`.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PackageFulfillmentPostgreSqlTests.cs` or extend `SandboxPaymentCompletionPostgreSqlTests.cs` only if the shared PostgreSQL fixture/setup remains clearer in one file.
- Modify `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageEntitlementEndpointTests.cs`.
- Modify existing exam/payment WebApi or Application tests only to prove existing standalone behavior remains unchanged if implementation touches shared code.

## Proposed Implementation Slices

### Slice 1 — Domain Model and Domain Tests

**Objective:** Add Stage 2 domain concepts for package order item source typing, purchased-offer snapshots, package purchase entitlements, and four benefit rights without Application, Infrastructure, or WebApi behavior.

**Affected layer(s):** Domain and Domain.Tests only.

**Expected files or file areas:**

- Modify `backend/src/NursingPlatform.Domain/Payments/PaymentOrderItem.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentOrderItemSourceType.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackagePurchaseEntitlement.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackagePurchaseEntitlementStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRight.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRightType.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageBenefitRightStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PackageOrderItemSnapshot.cs` as the dedicated one-to-one purchased-offer snapshot entity.
- Modify `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs`.
- Modify `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PreparationPackageDomainTests.cs`.

**Tests to write first:**

- `PaymentOrderItem_CreateExamAccessSnapshot_PreservesExistingStandaloneSnapshotFields`
- `PaymentOrderItem_CreatePackageOfferSnapshot_CapturesImmutablePurchaseFacts`
- `PaymentOrderItem_CreatePackageOfferSnapshot_DoesNotSetExamAccessProductSemantics`
- `PackageEntitlement_CreateFromSnapshot_CapturesImmutablePurchaseFacts`
- `PackageEntitlement_CreateFromSnapshot_SetsAccessWindowFromFulfillmentTimeAndPurchasedDurationDays`
- `PackageEntitlement_IsActiveRequiresActiveStatusAndCurrentAccessWindow`
- `PackageBenefitRights_CreateDefaultSet_IncludesExactlyFourStage2RightTypes`
- `PackageBenefitRights_CreateDefaultSet_CreatesReportRightDormant`

**Implementation summary:**

- Add source typing equivalent to `PaymentOrderItemSourceType { ExamAccessProduct, PreparationPackageOffer }` while keeping existing standalone `ProductId`, `ProductNameSnapshot`, `ProductTypeSnapshot`, and `ExamIdSnapshot` valid for standalone order items.
- Preserve `PaymentOrderItem.CreateSnapshot(PaymentProduct product)` behavior for standalone exam access.
- Add only the additive source discriminator and source id fields needed on `PaymentOrderItem` to distinguish `ExamAccessProduct` from `PreparationPackageOffer` items without overloading existing standalone fields.
- Store package-specific purchased-offer snapshot details in the dedicated one-to-one `PackageOrderItemSnapshot` entity/table. Do not store package snapshot facts in nullable package-specific columns on `PaymentOrderItem`.
- Add package snapshot creation with immutable facts required by the Stage 2 spec: source package offer id, offer title/slug/summary, package definition id/title/slug, country id, exam category id, package version id/version number, included exam id, included exam version id/title, reporting-profile publication id, practice collection version id, ordered study material version ids, price amount minor, currency, access duration days, and order creation timestamp.
- Existing standalone fields such as `ProductId`, `ProductNameSnapshot`, `ProductTypeSnapshot`, and `ExamIdSnapshot` remain backward compatible for standalone exam order items and must not be overloaded for package purchased-offer snapshot facts.
- Do not store protected exam question text, answer identifiers, correct answers, answer keys, rationales, protected options, internal scoring logic, provider secrets, tokens, or internal authorization state in snapshots.
- Add `PackagePurchaseEntitlement` as nurse-owned aggregate root with source order/order item ids, purchased package ids, access window, fulfillment timestamp, status, and child rights.
- Add default benefit rights: `MaterialsAccess = Available`, `PracticeAccess = Available`, `PackageExamAttemptEligibility = Available`, `ReportEligibility = Dormant`.
- Stage 2 domain must not consume attempt rights, generate reports, persist sessions, or create `ExamAccessGrant`.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PaymentEntityTests|PreparationPackageDomainTests"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Immutable purchase facts, no `ExamAccessGrant` overload, exact four right types, report right dormant, active-window rule, and no Stage 3/4 behavior.

**Explicit non-scope:** No Application handlers, no EF mappings, no migrations, no endpoints, no package attempt consumption, no report generation, no frontend.

### Slice 2 — Payment Order Item Integration and Snapshot Contracts

**Objective:** Extend existing nurse order creation and payment DTO contracts to support package offer purchases while preserving standalone exam-access behavior.

**Affected layer(s):** Application and Application.Tests, consuming Domain types from Slice 1.

**Expected files or file areas:**

- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CreateMyPaymentOrder/CreatePaymentOrderRequest.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CreateMyPaymentOrder/CreateMyPaymentOrderCommand.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentOrderItemDto.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentOrderDto.cs` only if needed to carry additive item detail without breaking existing shape.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentValidatorTests.cs`.

**Tests to write first:**

- `Validate_CreateOrder_WithNeitherProductIdNorPackageOfferId_ShouldHaveError`
- `Validate_CreateOrder_WithBothProductIdAndPackageOfferId_ShouldHaveError`
- `Handle_CreateOrder_WithProductId_PreservesExistingExamAccessOrderSnapshot`
- `Handle_CreatePackageOrder_WithActiveSellableOffer_CreatesPendingOrderWithPurchasedOfferSnapshot`
- `Handle_CreatePackageOrder_WithMissingOffer_ThrowsKeyNotFoundException`
- `Handle_CreatePackageOrder_WithInactiveOffer_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithRetiredOffer_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithUnsellableOfferComponents_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_IgnoresClientSuppliedCommercialOrEntitlementFields`
- `Handle_CreatePackageOrder_DoesNotCreatePaymentProductOrExamAccessProductSnapshot`
- `Handle_CreatePackageOrder_WithActiveSamePackageEntitlement_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithExpiredSamePackageEntitlement_AllowsSeparateRepurchase`
- `Handle_CreatePackageOrder_WithDifferentPackageSharingSameExam_AllowsIndependentPurchase`
- `Handle_CreatePackageOrder_WithSameDefinitionDifferentOfferOrVersionAndActiveEntitlement_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithPackageOffer_DoesNotAffectStandalonePaidClassification`
- `PaymentOrderItemDto_ForPackageSnapshot_DoesNotExposeProtectedExamContentCorrectAnswersSecretsOrInternalAuthorizationState`

**Implementation summary:**

- Extend request DTO to `Guid? ProductId` and `Guid? PackageOfferId`.
- Validator enforces exactly one source.
- Existing `productId` path remains observably unchanged and continues to require active payment product linked to a published exam.
- New `packageOfferId` path resolves the current nurse through `PaymentHandlerHelpers.GetCurrentNurseProfileIdAsync(...)`, loads the active/sellable package offer, package definition, package version, included exam/version, reporting profile, ordered material versions, and practice collection version through explicit projections.
- The package offer sellability check must cover active offer status plus referenced package definition/version/component eligibility needed for a new sale; order creation must fail before checkout if required live components are no longer sellable for new purchases.
- The same-package repurchase check must use `PreparationPackageDefinitionId`, not package offer id or package version id.
- Before creating a package order, expire stale active same-package entitlements for the current nurse/package where `AccessEndsAt <= now`, save that lifecycle update, then block any remaining active entitlement for the same `PreparationPackageDefinitionId`.
- Create one pending order with one package-sourced order item snapshot. The server owns nurse identity, price, currency, offer/version/composition facts, access duration, and order totals.
- Add additive payment order item DTO fields: source discriminator plus package snapshot summary fields read from the dedicated one-to-one snapshot record. Do not remove or rename existing standalone fields.
- `PaymentOrderItem` itself receives only source discriminator/source id data needed to route fulfillment. Existing standalone fields remain for standalone exam access and must not be repurposed for package offer, package version, access duration, material, practice collection, or reporting-profile snapshot facts.
- Existing order list/detail handlers must include package snapshot data through explicit projection without querying live offer state to mutate historical meaning.
- Payment order DTO tests must assert specific approved package snapshot fields are present and raw JSON security tests must assert forbidden fields are absent before deserialization.
- Do not start checkout, mark paid, fulfill entitlement, create rights, or create `ExamAccessGrant` in order creation.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PaymentValidator|CreateOrder|PaymentDtoSecurity"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Exactly-one-source validation, active/sellable offer checks, immutable snapshot completeness, active repurchase block, DTO backward compatibility, safe raw JSON/DTO exposure, and no fulfillment at order creation.

**Explicit non-scope:** No checkout changes except DTO compatibility, no payment completion fulfillment, no migrations, no endpoints beyond existing request/response contracts, no package attempts, no reports.

### Slice 3 — Persistence Configuration, Migration, and Data Integrity

**Objective:** Persist Stage 2 source typing, package purchased-offer snapshots, entitlements, and benefit rights with EF Core constraints and one migration.

**Affected layer(s):** Infrastructure and Infrastructure.Tests, with Application DbSet exposure updates.

**Expected files or file areas:**

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`.
- Create one migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` during approved implementation.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs`.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs`.

**Tests to write first:**

- `PaymentOrderItemConfiguration_StoresSourceTypeAsStringWithMaxLength`
- `PaymentOrderItemConfiguration_PreservesStandaloneExamAccessSnapshotRelationships`
- `PaymentOrderItemConfiguration_ConfiguresSourceDiscriminatorAndSourceIdWithoutOverloadingStandaloneFields`
- `PackageOrderItemSnapshotConfiguration_ConfiguresOneToOnePackagePurchasedOfferSnapshotTable`
- `PackageEntitlementConfiguration_EnforcesUniqueEntitlementPerOrderItem`
- `PackageBenefitRightConfiguration_EnforcesOneRightPerTypePerEntitlement`
- `PackageEntitlementConfiguration_EnforcesOneActiveEntitlementPerNurseAndPackageDefinition`
- `PackageEntitlementConfiguration_UsesRestrictDeleteBehaviorForFinancialSnapshotAndRightRelationships`
- `PackageEntitlementConfiguration_PersistsUtcAccessWindowAndDurationDerivedEndTime`

**Implementation summary:**

- Expose DbSets for `PackagePurchaseEntitlement`, `PackageBenefitRight`, and the dedicated `PackageOrderItemSnapshot` entity.
- Configure enum discriminators as strings with bounded max length, following payment and Stage 1 package conventions.
- Add package snapshot persistence through the dedicated one-to-one snapshot table without forcing package offers into `PaymentProducts`, overloading `PaymentProductType.ExamAccess`, or overloading existing standalone `PaymentOrderItem` snapshot fields.
- Configure unique entitlement per `PaymentOrderItemId`.
- Configure unique right per `(PackagePurchaseEntitlementId, RightType)`.
- Configure one persisted `Active` entitlement per `(NurseProfileId, PreparationPackageDefinitionId)` using a filtered unique index equivalent to `Status = 'Active'`.
- Configure indexes for nurse-owned entitlement list/detail queries, source order/order item lookup, package definition/version lookup, and benefit-right authorization lookup.
- Configure all financial, snapshot, entitlement, and right relationships with `DeleteBehavior.Restrict` unless PostgreSQL migration generation requires an explicitly documented `NoAction`; cascade delete is forbidden.
- Generate exactly one Stage 2 migration with EF Core Code-First tooling only during approved implementation.
- Do not manually edit the database or bypass `ApplicationDbContext`.

**Validation commands:**

These are future separately authorized implementation-slice validation commands. They are not executed or authorized by this planning-correction task.

- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PaymentConfiguration|PreparationPackageConfiguration"`
- `dotnet ef migrations script --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --idempotent --output /tmp/opencode/preparation-package-stage2.sql`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Database uniqueness invariants, delete behavior, migration scope, backward-compatible payment item schema, historical buyer referential stability, and no Stage 3/4 tables.

**Explicit non-scope:** No package session provenance schema, no report tables, no material/practice runtime tables, no manual schema changes, no production provider schema.

### Slice 4 — Fulfillment Application Service and Idempotency

**Objective:** Implement idempotent package order item fulfillment and integrate it into Sandbox completion atomically with the paid transition while preserving standalone `ExamAccessGrant` fulfillment.

**Affected layer(s):** Application, Application.Tests, and PostgreSQL Infrastructure tests for transaction/concurrency behavior.

**Expected files or file areas:**

- Create `backend/src/NursingPlatform.Application/PreparationPackages/Fulfillment/PackageFulfillmentService.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Fulfillment/PackageFulfillmentResult.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Fulfillment/UnsafePackageFulfillmentException.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CompleteSandboxPaymentCheckout/CompleteSandboxPaymentCheckoutCommand.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentCompletionDto.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs` if completion mapping is extracted.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageFulfillmentServiceTests.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PackageFulfillmentPostgreSqlTests.cs` or extend `SandboxPaymentCompletionPostgreSqlTests.cs` with package-specific cases.

**Tests to write first:**

- `Handle_CompleteSandboxCheckout_ForPackageOrder_CreatesOneEntitlementAndFourBenefitRights`
- `Handle_FulfillPackageOrderItem_WhenRepeatedForSameOrderItem_ReturnsExistingEntitlement`
- `Handle_FulfillPackageOrderItem_WhenConcurrentForSameOrderItem_ConvergesToOneEntitlementAndFourRights`
- `Handle_CompleteSandboxCheckout_ForExamAccessProduct_PreservesExistingExamAccessGrantFulfillment`
- `Handle_CompleteSandboxCheckout_ForPackageOrder_DoesNotCreateExamAccessGrant`
- `Handle_FulfillPackageOrderItem_WithActiveSamePackageEntitlementFromDifferentOrderItem_ThrowsInvalidOperationException`
- `Handle_FulfillPackageOrderItem_WithExistingEntitlementSnapshotMismatch_ThrowsUnsafeFulfillmentConflict`
- `Handle_FulfillPackageOrderItem_WhenOfferRetiredAfterOrderCreation_UsesPurchasedSnapshot`
- `Handle_FulfillPackageOrderItem_WhenComponentsRetiredAfterFulfillment_DoesNotMutateHistoricalEntitlement`
- `Handle_FulfillPackageOrderItem_CreatesOneAccessWindowSharedByAllWindowLimitedRights`
- `CompleteSandboxCheckout_WhenPackageFulfillmentFailsBeforePaidCommit_RollsBackPaidTransitionAndEntitlement`
- `CompleteSandboxCheckout_TwoSimultaneousPackageHandlers_ConvergeToOnePaidOrderOneEntitlementAndFourRights`

**Implementation summary:**

- Keep checkout idempotency and fulfillment idempotency separate.
- Package fulfillment idempotency key is the paid package order item id plus source discriminator.
- Fulfillment service loads the immutable package order item snapshot, verifies nurse ownership and order/item consistency, checks for existing entitlement by source item, creates one entitlement and four rights when absent, and returns existing complete fulfillment on retry.
- If existing entitlement facts match and all four rights exist, return existing outcome.
- If existing entitlement facts match but one or more rights are missing, either repair missing rights idempotently in the same transaction or fail with `UnsafePackageFulfillmentException`; the implementation must choose one behavior and test it explicitly.
- If immutable purchased facts mismatch, throw `UnsafePackageFulfillmentException` and do not rewrite entitlement or snapshot facts.
- During fulfillment, expire stale active same-package entitlements for the same nurse/package whose `AccessEndsAt <= now`, then enforce no active same-package entitlement exists for a different order item.
- In the Development/Test Sandbox completion handler, process each order item by source type:
  - `ExamAccessProduct` items use existing `ExamAccessGrant` fulfillment.
  - `PreparationPackageOffer` items use package fulfillment and do not create grants.
- Paid transition, package entitlement creation, and benefit-right creation commit or roll back together in the existing transaction.
- Extend `PaymentCompletionDto` additively with package fulfillment summary fields that do not expose internal benefit right identifiers, while preserving `GrantedExamIds` for standalone purchases.
- Repeated completion after `Paid` must return the existing paid/fulfilled outcome for both standalone and package purchases.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "CompleteSandboxCheckout|PackageFulfillment"`
- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PackageFulfillment|SandboxPaymentCompletion"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Transactional atomicity, retry/idempotency, unique constraint convergence, stale entitlement expiry, active repurchase race protection, no duplicate rights, no `ExamAccessGrant` for packages, and no live catalog mutation of historical facts.

**Explicit non-scope:** No production webhook, no recovery worker, no package-attempt consumption, no session provenance, no report generation/retry/access.

### Slice 5 — API Endpoints and Integration Tests

**Objective:** Expose only the approved Stage 2 HTTP surface: additive package order creation through the existing payment endpoint, additive package completion summary through existing Sandbox endpoint, and nurse-owned package entitlement list/detail endpoints.

**Affected layer(s):** WebApi and WebApi.Tests, consuming Application handlers from earlier slices.

**Expected files or file areas:**

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` only where existing payment endpoint OpenAPI metadata or DTO binding requires additive Stage 2 metadata.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/DTOs/PackageEntitlementDtos.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/ListMyPackageEntitlements/ListMyPackageEntitlementsQuery.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/GetMyPackageEntitlement/GetMyPackageEntitlementQuery.cs`.
- Modify `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PackageEntitlementEndpointTests.cs`.

**Tests to write first:**

- `CreatePaymentOrder_WithPackageOfferId_Returns401WithoutJwt`
- `CreatePaymentOrder_WithPackageOfferId_SendsCommandWithPackageOfferId`
- `CreatePaymentOrder_WithBothProductIdAndPackageOfferId_ReturnsValidationProblem`
- `CompleteSandboxCheckout_ForPackageOrder_ReturnsAdditivePackageEntitlementSummaryAndNoStore`
- `ListMyPackageEntitlements_Returns401WithoutJwt`
- `ListMyPackageEntitlements_ReturnsOnlyCurrentNurseEntitlements`
- `GetMyPackageEntitlement_ForAnotherNurse_Returns404Or403WithoutExposure`
- `GetMyPackageEntitlement_RawJsonDoesNotContainForbiddenSensitiveFields`
- `PackageStage2EndpointScope_DoesNotMapPackageAttemptStartReportGenerationOrReportAccessRoutes`
- `PackageStage2EndpointScope_DoesNotMapEmployerPackagePurchaseReportOrPracticeProgressRoutes`
- `ExistingPaymentAndExamEndpoints_RemainBackwardCompatibleAfterPackageFulfillment`

**Implementation summary:**

- Existing `POST /api/v1/me/nurse-profile/payment/orders` remains the only nurse order creation endpoint; it now binds request DTO with either `productId` or `packageOfferId`.
- Existing `POST /api/v1/me/nurse-profile/payment/orders/{orderId}/checkout` remains unchanged as checkout entry point and continues setting `Cache-Control: no-store`.
- Existing Development/Test-only `POST /api/v1/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete` remains environment gated, authenticated, and no-store.
- Add nurse-owned entitlement endpoints under the preparation package group, proposed paths:
  - `GET /api/v1/me/nurse-profile/preparation-packages/entitlements`
  - `GET /api/v1/me/nurse-profile/preparation-packages/entitlements/{id}`
- Entitlement endpoints require `.RequireAuthorization()` through the existing authenticated nurse profile group and resolve current nurse ownership in Application handlers.
- Entitlement list/detail responses expose entitlement id, package definition/version/offer summary, purchased snapshot summary, access starts/ends, status, and benefit-right type/status summaries. They must not expose internal benefit right identifiers. Stage 3 must select entitlement id, not internal right id.
- Add OpenAPI metadata for new/extended endpoints, including response DTOs, validation problems, 401, 404 for hidden detail, and conflict where applicable.
- Do not expose admin/support diagnostics in Stage 2 through this plan.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PaymentEndpoints|PackageEntitlement"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PreparationPackageCatalog|AdminPreparationPackage"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Exact endpoint scope, auth behavior, no-store preservation, raw JSON secrecy, OpenAPI metadata, existing payment endpoint compatibility, and no Stage 3/4/employer routes.

**Explicit non-scope:** No package exam start route, no package purchase selection for exam start, no attempt consumption endpoint, no report endpoint, no workspace runtime, no employer package route, no frontend.

### Slice 6 — Authorization Helpers and Benefit Rights Read Models

**Objective:** Add application-level entitlement/right availability checks and nurse-owned read models needed by later stages while keeping all Stage 3, Stage 4, and workspace runtime actions unimplemented.

**Affected layer(s):** Application and Application.Tests, with endpoint query handlers if not completed in Slice 5.

**Expected files or file areas:**

- Create `backend/src/NursingPlatform.Application/PreparationPackages/Authorization/PackageBenefitAuthorizationService.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Authorization/PackageBenefitAuthorizationResult.cs`.
- Create or complete `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/DTOs/PackageEntitlementDtos.cs`.
- Create or complete `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/ListMyPackageEntitlements/ListMyPackageEntitlementsQuery.cs`.
- Create or complete `backend/src/NursingPlatform.Application/PreparationPackages/Entitlements/GetMyPackageEntitlement/GetMyPackageEntitlementQuery.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageBenefitAuthorizationTests.cs`.
- Create or modify `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PackageEntitlementHandlerTests.cs`.

**Tests to write first:**

- `Handle_AuthorizePackageBenefit_WithActiveRightAndWindow_AllowsAccessWithoutPaymentLookup`
- `Handle_AuthorizePackageBenefit_DoesNotQueryPaymentOrderStatus`
- `Handle_AuthorizePackageBenefit_WithExpiredWindow_DeniesAccess`
- `Handle_AuthorizePackageBenefit_WithMissingRight_DeniesAccess`
- `Handle_AuthorizePackageBenefit_WithDormantReportRight_DoesNotAuthorizeReportAccessInStage2`
- `Handle_AuthorizePackageBenefit_ForDifferentNurse_DeniesWithoutExposure`
- `Handle_AuthorizePackageBenefit_ForDifferentPackage_DeniesAccess`
- `Handle_AuthorizePackageBenefit_WithStandaloneExamAccessGrant_DoesNotSatisfyPackageRight`
- `Handle_AuthorizeStandaloneExamStart_WithPackageBenefitRight_DoesNotSatisfyExamAccessGrantRequirement`
- `Handle_AuthorizeReportEligibility_WithStandaloneOrDifferentPackageSession_DoesNotQualifyRight`
- `Handle_ListMyPackageEntitlements_ReturnsDeterministicallySortedNurseOwnedSummaries`
- `Handle_GetMyPackageEntitlement_ProjectsBenefitRightStatusesWithoutInternalNavigationObjects`

**Implementation summary:**

- Implement an Application-layer service/query pattern that checks only nurse ownership, entitlement status, access window, exact benefit right type, and right status.
- Authorization service must not query live payment status to authorize package benefits.
- Materials and practice rights authorize only when entitlement is active and corresponding right is `Available`.
- Package exam attempt eligibility helpers/read models may expose only entitlement/right availability status for later Stage 3 use; they must not implement package exam start, attempt consumption, session provenance, source-conflict behavior, or source switching.
- Report helpers/read models may expose only the dormant report right status for later Stage 4 use; they must not implement report generation, report access, report retry, report guidance, or report qualification.
- Slice 6 must not implement workspace runtime, material delivery, practice runtime, package exam start, attempt consumption, session provenance, report generation, or report access.
- Return deterministic, DTO-safe read models for entitlement list/detail.
- Handlers throw `KeyNotFoundException` for missing/foreign entitlement detail to preserve ownership privacy.
- Do not introduce WebApi dependencies in Application.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PackageBenefitAuthorization|PackageEntitlement"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Entitlement/right availability checks only, no payment live lookup, nurse ownership privacy, dormant report boundary, no Stage 3 package exam start/attempt/session behavior, no Stage 4 report behavior, no workspace runtime, deterministic projections, and DTO safety.

**Explicit non-scope:** No material delivery, no practice runtime/progress APIs, no package exam start, no attempt consumption, no session provenance, no report generation/access, no workspace runtime, no employer access.

### Slice 7 — Compatibility, Security, and Final Verification

**Objective:** Verify Stage 2 behavior, compatibility, security boundaries, migration health, documentation decision, and git hygiene.

**Affected layer(s):** Verification across Domain, Application, Infrastructure, WebApi, and documentation only if explicitly authorized.

**Expected files or file areas:**

- Modify `docs/api/api-design.md` only if explicitly authorized for documentation synchronization after implementation.
- Modify `docs/database/database-design.md` only if explicitly authorized for documentation synchronization after implementation.
- Do not modify `CURRENT_TASK.md`, `TASKS.md`, approved specs, Stage 1 plan, frontend/design files, `.agent/goal-state.md`, `README`, or `CHANGELOG` unless separately instructed.

**Tests to write first:**

- `StandaloneExamPurchaseJourney_OrderCheckoutSandboxCompletionGrantAndExamStart_RemainsSupported`
- `StartExamSession_ForFreeExamWithoutPaidProduct_StillAllowsStartWithoutGrant`
- `StartExamSession_ForStandalonePaidExamWithoutGrant_StillReturnsForbidden`
- `ListAndGetExams_WithPackageOffersAndEntitlements_DoNotChangeStandaloneIsFreeOrCanStart`
- `PackageStage2Compatibility_DoesNotExposeStage3OrStage4Routes`
- `PackageStage2Security_RawJsonDoesNotExposeForbiddenSensitiveFields`

If these behaviors are already covered by earlier slice tests, this slice may only run the existing tests and add no new tests. If review finds a missing Stage 2 acceptance criterion, add the smallest missing test in the appropriate earlier test area and re-run the relevant slice verification.

**Implementation summary:**

- Run the complete Stage 2 test set and compatibility checks.
- Confirm standalone payment order creation, checkout start, Sandbox completion, grant fulfillment, and exam start still work.
- Confirm package purchases do not create grants and do not alter `isFree`/`canStart` semantics.
- Confirm active same-package repurchase protection has both application and PostgreSQL concurrency coverage.
- Confirm no Stage 3 session provenance/start behavior and no Stage 4 report behavior was implemented.
- Confirm all API responses use DTOs and raw JSON tests check forbidden sensitive fields.
- Confirm migration script generation succeeds and no pending model changes remain after the Stage 2 migration.
- Confirm no protected or unrelated files changed.
- Confirm no files are staged and no commit is made unless separately authorized.

**Validation commands:**

These are future separately authorized implementation-slice validation commands. They are not executed or authorized by this planning-correction task.

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PreparationPackage|PaymentEntity"`
- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PreparationPackage|Payment"`
- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PreparationPackage|PaymentConfiguration|PackageFulfillment|SandboxPaymentCompletion"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PreparationPackage|PackageEntitlement|PaymentEndpoints"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ExamEndpoints|ExamAnalytics"`
- `dotnet build backend/NursingPlatform.slnx`
- `dotnet ef migrations script --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --idempotent --output /tmp/opencode/preparation-package-stage2.sql`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi`
- `git diff --check`
- `git status --short --untracked-files=all`
- `git diff -- CURRENT_TASK.md TASKS.md docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md docs/superpowers/specs/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md docs/superpowers/plans/2026-07-26-preparation-package-stage-1-catalog-authoring.md .agent/goal-state.md`

**Review focus:** Full verification evidence, compatibility preservation, security/privacy boundary, migration health, no unrelated changes, no staged files, no commit, and final evidence completeness.

**Explicit non-scope:** Do not proceed to Stage 3. Do not stage. Do not commit. Do not update task tracking unless separately and explicitly authorized. Do not add implementation beyond verified Stage 2 scope.

## Suggested Slice Order

1. **Slice 1 — Domain Model and Domain Tests** — dependency-safe first step because later layers need concrete entitlement, right, and source snapshot types.
2. **Slice 2 — Payment Order Item Integration and Snapshot Contracts** — depends on domain types and produces Application contracts for order creation and payment DTOs.
3. **Slice 3 — Persistence Configuration, Migration, and Data Integrity** — depends on domain and Application abstractions; creates constraints required for idempotent fulfillment and repurchase protection.
4. **Slice 4 — Fulfillment Application Service and Idempotency** — depends on domain, snapshots, persistence, and transaction abstractions; integrates package fulfillment with Sandbox completion.
5. **Slice 5 — API Endpoints and Integration Tests** — depends on Application handlers/DTOs and exposes only approved Stage 2 HTTP surface.
6. **Slice 6 — Authorization Helpers and Benefit Rights Read Models** — depends on entitlement/right persistence and produces reusable Stage 3/4 authorization hooks without implementing later-stage actions.
7. **Slice 7 — Compatibility, Security, and Final Verification** — dependency-safe final step because it verifies all earlier slices, compatibility behavior, documentation boundaries, migration health, and git hygiene.

This order keeps dependency direction intact: WebApi depends on Application, Infrastructure implements Application abstractions, and Domain remains independent of Infrastructure and WebApi.

## Permission and Authorization Plan

Stage 2 nurse purchase and entitlement endpoints require authentication and current nurse ownership. This plan does not include admin/support diagnostic endpoints or new admin/support permissions.

Authorization requirements:

- Existing payment product catalog remains `.RequireAuthorization()` and does not require permission service setup.
- Existing nurse order endpoints remain authenticated through the current nurse profile group.
- Package order creation uses current-nurse ownership resolved through `NurseRoleGuard`/current-user context.
- Package entitlement list/detail uses current-nurse ownership and hides non-owned entitlements with `404` or equivalent privacy-preserving response.
- Benefit authorization uses package entitlement and benefit-right rows, not live payment lookups.
- Standalone exam start continues to use `ExamAccessGrant` where the existing standalone policy requires a grant.
- A standalone `ExamAccessGrant` cannot satisfy a package benefit right.
- A package benefit right cannot satisfy standalone exam start.
- One package entitlement cannot satisfy another package's rights.
- Permission-protected admin/support diagnostics are not included in this Stage 2 implementation plan.

## Database and Migration Plan

A migration is required in a future separately approved implementation because Stage 2 introduces new persisted entitlement and benefit-right records and adds dedicated one-to-one package purchased-offer snapshot persistence. Generate exactly one Stage 2 migration after domain entities and EF configurations compile during that future approved implementation only. Do not hand-edit schema outside EF migrations.

The migration commands listed in this plan are future implementation-slice validation commands only. They must not be interpreted as commands executed, authorized, or required during this planning-correction task.

Migration constraints:

- EF Core Code-First migrations only.
- Generate migration only during a separately approved implementation session.
- Do not manually edit the database.
- Do not bypass `ApplicationDbContext`.
- Do not introduce package-attempt session provenance, report, report-access, material-delivery, practice-progress/runtime, subscription, cart, refund, employer package, adaptive-practice, AI, or frontend persistence.

Required invariants:

- Unique entitlement per package order item.
- Exactly one benefit right of each required type per entitlement.
- Entitlement references one nurse profile.
- Entitlement references source payment order and source payment order item.
- Entitlement references purchased package definition, package version, and package offer facts from the snapshot.
- Active same-package repurchase for the same nurse and package definition is prevented transactionally and by a filtered unique database invariant.
- Access-window timestamps are UTC and server-owned.
- Purchased snapshot fields are immutable after order creation.
- Entitlement purchase facts are immutable after fulfillment.
- EF relationships for financial, entitlement, right, and snapshot records use `DeleteBehavior.Restrict` unless an implementation plan correction explicitly justifies `NoAction`; cascade delete is forbidden.
- Historical buyer records remain referentially stable when live package catalog records are retired.

Migration verification command during approved implementation:

- `dotnet ef migrations script --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --idempotent --output /tmp/opencode/preparation-package-stage2.sql`

## API Plan

Implement only these Stage 2 API surfaces:

- Existing nurse payment order creation, extended additively:
  - `POST /api/v1/me/nurse-profile/payment/orders`
  - Request accepts exactly one of `productId` or `packageOfferId`.
- Existing nurse checkout start, unchanged as entry point:
  - `POST /api/v1/me/nurse-profile/payment/orders/{orderId}/checkout`
- Existing Development/Test-only Sandbox completion, additively extended response:
  - `POST /api/v1/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete`
- New nurse-owned package entitlement visibility:
  - `GET /api/v1/me/nurse-profile/preparation-packages/entitlements`
  - `GET /api/v1/me/nurse-profile/preparation-packages/entitlements/{id}`

Do not implement checkout alternatives, package exam start, package purchase selection for exam start, attempt consumption, session provenance mutation/source switching, conflict resolution endpoints, report generation/access/retry/guidance, materials runtime, practice runtime, carts, subscriptions, sponsored packages, employer package purchase/report/practice progress, adaptive practice, AI recommendation, or frontend endpoints in Stage 2.

## Test Strategy

Testing must follow TDD where practical: write failing tests first, implement the smallest production behavior needed to pass, then refactor while keeping tests green.

Domain tests:

- `PaymentOrderItem_CreateExamAccessSnapshot_PreservesExistingStandaloneSnapshotFields`
- `PaymentOrderItem_CreatePackageOfferSnapshot_CapturesImmutablePurchaseFacts`
- `PaymentOrderItem_CreatePackageOfferSnapshot_DoesNotSetExamAccessProductSemantics`
- `PackageEntitlement_CreateFromSnapshot_CapturesImmutablePurchaseFacts`
- `PackageEntitlement_CreateFromSnapshot_SetsAccessWindowFromFulfillmentTimeAndPurchasedDurationDays`
- `PackageEntitlement_IsActiveRequiresActiveStatusAndCurrentAccessWindow`
- `PackageBenefitRights_CreateDefaultSet_IncludesExactlyFourStage2RightTypes`
- `PackageBenefitRights_CreateDefaultSet_CreatesReportRightDormant`

Application tests:

- `Handle_CreatePackageOrder_WithActiveSellableOffer_CreatesPendingOrderWithPurchasedOfferSnapshot`
- `Handle_CreatePackageOrder_WithMissingOffer_ThrowsKeyNotFoundException`
- `Handle_CreatePackageOrder_WithInactiveOffer_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithRetiredOffer_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithUnsellableOfferComponents_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_IgnoresClientSuppliedCommercialOrEntitlementFields`
- `Handle_CreatePackageOrder_DoesNotCreatePaymentProductOrExamAccessProductSnapshot`
- `Handle_CreatePackageOrder_WithActiveSamePackageEntitlement_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithExpiredSamePackageEntitlement_AllowsSeparateRepurchase`
- `Handle_CreatePackageOrder_WithDifferentPackageSharingSameExam_AllowsIndependentPurchase`
- `Handle_CreatePackageOrder_WithSameDefinitionDifferentOfferOrVersionAndActiveEntitlement_ThrowsInvalidOperationException`
- `Handle_CreatePackageOrder_WithPackageOffer_DoesNotAffectStandalonePaidClassification`
- `Handle_CompleteSandboxCheckout_ForPackageOrder_CreatesOneEntitlementAndFourBenefitRights`
- `Handle_FulfillPackageOrderItem_WhenRepeatedForSameOrderItem_ReturnsExistingEntitlement`
- `Handle_FulfillPackageOrderItem_WhenConcurrentForSameOrderItem_ConvergesToOneEntitlementAndFourRights`
- `Handle_CompleteSandboxCheckout_ForExamAccessProduct_PreservesExistingExamAccessGrantFulfillment`
- `Handle_CompleteSandboxCheckout_ForPackageOrder_DoesNotCreateExamAccessGrant`
- `Handle_AuthorizePackageBenefit_WithActiveRightAndWindow_AllowsAccessWithoutPaymentLookup`
- `Handle_AuthorizePackageBenefit_DoesNotQueryPaymentOrderStatus`
- `Handle_AuthorizePackageBenefit_WithStandaloneExamAccessGrant_DoesNotSatisfyPackageRight`
- `Handle_AuthorizeStandaloneExamStart_WithPackageBenefitRight_DoesNotSatisfyExamAccessGrantRequirement`
- `Handle_FulfillPackageOrderItem_WithExistingEntitlementSnapshotMismatch_ThrowsUnsafeFulfillmentConflict`
- `Handle_FulfillPackageOrderItem_WhenOfferRetiredAfterOrderCreation_UsesPurchasedSnapshot`
- `Handle_FulfillPackageOrderItem_WhenComponentsRetiredAfterFulfillment_DoesNotMutateHistoricalEntitlement`
- `Handle_FulfillPackageOrderItem_WithActiveSamePackageEntitlementFromDifferentOrderItem_ThrowsInvalidOperationException`
- `Handle_FulfillPackageOrderItem_CreatesOneAccessWindowSharedByAllWindowLimitedRights`

Infrastructure tests:

- `PackageEntitlementConfiguration_EnforcesUniqueEntitlementPerOrderItem`
- `PackageBenefitRightConfiguration_EnforcesOneRightPerTypePerEntitlement`
- `PackageEntitlementConfiguration_EnforcesOneActiveEntitlementPerNurseAndPackageDefinition`
- `PackageEntitlementConfiguration_UsesRestrictDeleteBehaviorForFinancialSnapshotAndRightRelationships`
- `PackageEntitlementConfiguration_PersistsUtcAccessWindowAndDurationDerivedEndTime`
- PostgreSQL concurrency test proving active-same-package repurchase protection.
- PostgreSQL concurrency test proving repeated same order item fulfillment converges to one entitlement and four rights.

WebApi tests:

- `CreatePaymentOrder_WithPackageOfferId_Returns401WithoutJwt`
- `ListMyPackageEntitlements_ReturnsOnlyCurrentNurseEntitlements`
- `GetMyPackageEntitlement_ForAnotherNurse_Returns404Or403WithoutExposure`
- `GetMyPackageEntitlement_RawJsonDoesNotContainForbiddenSensitiveFields`
- `PackageStage2EndpointScope_DoesNotMapPackageAttemptStartReportGenerationOrReportAccessRoutes`
- `PackageStage2EndpointScope_DoesNotMapEmployerPackagePurchaseReportOrPracticeProgressRoutes`
- `ExistingPaymentAndExamEndpoints_RemainBackwardCompatibleAfterPackageFulfillment`

Compatibility tests:

- `StandaloneExamPurchaseJourney_OrderCheckoutSandboxCompletionGrantAndExamStart_RemainsSupported`
- `StartExamSession_ForFreeExamWithoutPaidProduct_StillAllowsStartWithoutGrant`
- `StartExamSession_ForStandalonePaidExamWithoutGrant_StillReturnsForbidden`
- `ListAndGetExams_WithPackageOffersAndEntitlements_DoNotChangeStandaloneIsFreeOrCanStart`

Final verification commands:

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PreparationPackage|PaymentEntity"`
- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PreparationPackage|Payment"`
- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PreparationPackage|PaymentConfiguration|PackageFulfillment|SandboxPaymentCompletion"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PreparationPackage|PackageEntitlement|PaymentEndpoints"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ExamEndpoints|ExamAnalytics"`
- `dotnet build backend/NursingPlatform.slnx`
- `dotnet ef migrations script --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --idempotent --output /tmp/opencode/preparation-package-stage2.sql`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi`
- `git diff --check`
- `git status --short --untracked-files=all`
- `git diff -- CURRENT_TASK.md TASKS.md docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md docs/superpowers/specs/2026-07-28-preparation-package-stage-2-fulfillment-entitlements.md docs/superpowers/plans/2026-07-26-preparation-package-stage-1-catalog-authoring.md .agent/goal-state.md`

## Security and Privacy Review Checklist

- Payment status is used to create entitlements; it is not queried live to authorize materials, practice, package attempt eligibility, or report eligibility.
- Clients are not trusted for nurse identity, entitlement ownership, price, currency, access duration, package version, package composition, right type, right status, or fulfillment status.
- Package order snapshots and entitlement outputs do not expose protected exam question text, answer identifiers, correct answers, answer keys, protected options, rationales, internal scoring logic, provider secrets, tokens, password hashes, or internal authorization state.
- DTOs do not expose EF/domain navigation objects.
- Raw JSON tests inspect sensitive responses before deserialization.
- Logs must not include provider secrets, tokens, protected exam content, or sensitive internal authorization state.
- Employer access to package purchase history, practice progress, reports, or entitlements is not introduced.
- Stage 2 does not expose internal benefit right identifiers for client selection of package attempt start.

## Stage 3 Handoff Constraints

Stage 3 receives from Stage 2:

- Package purchase entitlement id as the client-selectable package purchase identifier for package-attempt start.
- Internal package exam attempt eligibility right linked to the selected entitlement.
- Access-window start/end facts.
- Purchased exam id and exact exam version id.
- Purchased package version and package definition identity.
- Purchased-offer snapshot provenance.

Stage 2 must not pre-implement package attempt start, attempt consumption, session provenance, one-in-progress session conflict handling, or source switching.

## Stage 4 Handoff Constraints

Stage 4 receives from Stage 2:

- Dormant report eligibility right.
- Entitlement and purchased-offer snapshot provenance.
- Purchased reporting-profile publication id.
- Purchased material version ids and order.
- Purchased practice collection version id.
- Access-window and ownership facts.

Stage 2 must not pre-implement report generation, report persistence, report retry, report access, or report guidance.

## Documentation Decision

During implementation, update authoritative API/database documentation only if explicitly included in the approved implementation prompt. Do not modify `CURRENT_TASK.md`, `TASKS.md`, approved specifications, Stage 1 plan, frontend/design files, `.agent/goal-state.md`, `README`, or `CHANGELOG` without explicit instruction.

## Review and Approval Gates

- This plan requires independent review before approval.
- Implementation requires separate explicit user authorization after the reviewed plan is approved.
- Database modification and migration creation require separate explicit authorization as part of implementation approval.
- Staging, committing, pushing, or beginning Stage 3 require separate explicit authorization.
- If implementation discovers a conflict with the approved Stage 2 specification or a missing business rule, stop and request clarification instead of guessing.
