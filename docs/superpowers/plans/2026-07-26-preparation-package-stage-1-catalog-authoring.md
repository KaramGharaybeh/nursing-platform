# Preparation Package Stage 1 Catalog and Authoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the backend-only Stage 1 Preparation Package catalog, authoring, package composition, reporting-topic/profile, offer, and safe catalog foundation described in `docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md`.

**Architecture:** Stage 1 adds a new additive Preparation Packages domain area without replacing existing Exams, Payments, `ExamAccessGrant`, standalone exam start, scoring, analytics, or checkout behavior. Domain entities live in `NursingPlatform.Domain`; CQRS handlers, DTOs, validators, and permission constants live in `NursingPlatform.Application`; EF Core configuration, migrations, and reference-data seeding live in `NursingPlatform.Infrastructure`; Minimal API endpoint mappings stay thin in `NursingPlatform.WebApi`.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core Code-First migrations, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi integration-test factory and JWT helpers.

---

## Status

Approved — Stage 1 Implementation Plan

## Purpose

This document is an implementation plan only. It organizes the approved Stage 1 Preparation Package catalog and authoring specification into reviewable backend implementation slices, but it does not authorize execution, source-code changes, test changes, migrations, staging, committing, pushing, deployment, or beginning Stage 1 implementation.

Any implementation must be separately and explicitly approved after this plan is reviewed. This plan also does not authorize Stage 2, Stage 3, Stage 4, frontend work, checkout, entitlement fulfillment, package attempts, report generation, or any future-roadmap feature.

## Inputs

Authoritative inputs for this plan:

- `docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md`
- `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`
- `docs/product/vision.md`
- `docs/architecture/system-architecture.md`
- `docs/backend/backend-architecture.md`
- `docs/database/database-design.md`
- `docs/api/api-design.md`
- `docs/development/model-orchestration.md`
- `PROJECT_RULES.md`
- `CURRENT_TASK.md`
- `TASKS.md`
- `AGENTS.md`

The approved Stage 1 specification remains authoritative for Stage 1 scope. The umbrella architecture-decisions specification remains authoritative for approved cross-stage invariants. This plan must not introduce new business decisions.

## Scope Lock

- Execute only Stage 1 catalog, authoring, package composition, reporting-topic/profile, package offer, safe catalog, and conceptual workspace foundation.
- Implement the approved Stage 1 spec exactly: `docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md`.
- Preserve the approved umbrella architecture decisions in `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`.
- Do not implement Stage 2 payment order processing, checkout behavior, idempotent fulfillment, entitlement persistence, benefit-right authorization, or runtime access-window enforcement.
- Do not implement Stage 3 package-attempt start, attempt consumption, session provenance, source conflict handling, or `ExamAccessGrant` compatibility changes.
- Do not implement Stage 4 report generation, report persistence, report access, analytical classification, or guidance presentation.
- Do not add frontend screens, frontend routes, design files, wireframes, Angular components, or visual layouts.
- Do not introduce subscriptions, carts, upgrade paths, refunds, package sharing, employer-sponsored packages, cohorts, team purchases, marketplace, creator marketplace, affiliate system, multi-vendor content, AI recommendations, adaptive practice, report sharing, cross-country packages, cross-category packages, multi-exam bundles, individually purchasable benefits, or purchase-time benefit selection.
- Preserve existing standalone paid-exam, free-exam, grant-authorized session start, scoring, review, attempt history, analytics, `isFree`, and `canStart` behavior.
- Package offers, package entitlements, package attempts, and package benefit rights must not participate in the standalone effective-paid classification rule: `Exam.IsFree == false OR active positive-price ExamAccess product exists`.
- Catalog responses must not expose protected exam questions, answer keys, protected answer identifiers, protected options, exam rationales, internal scoring logic, internal report logic, raw tokens, secrets, internal authorization state, or EF/domain navigation objects.
- Draft material and draft practice content are not nurse-accessible.
- Published package versions, published material versions, published practice collection versions, and published reporting profile publications are immutable.
- EF Core migrations are allowed only during a separately approved implementation session, not while authoring or reviewing this plan.
- Do not modify `CURRENT_TASK.md`, `TASKS.md`, approved specs, frontend/design files, `.agent/goal-state.md`, `README`, or `CHANGELOG` unless separately and explicitly instructed.
- Do not stage files, commit, push, reset, clean, stash, or use `git add .` unless separately and explicitly authorized.

Stage 1 should be implemented as one backend feature batch split into reviewable slices. The batch produces independently testable catalog/authoring capabilities, but it is not commercially launchable because payment fulfillment, entitlements, package-attempt launch, and reports remain later-stage work.

## Existing Codebase Reconnaissance

Existing project patterns and paths to follow during implementation:

Domain:

- `backend/src/NursingPlatform.Domain/Common/AuditableEntity.cs` — existing auditable base entity convention.
- `backend/src/NursingPlatform.Domain/Exams/` — existing exam aggregate/entity/status conventions.
- `backend/src/NursingPlatform.Domain/Payments/` — existing payment product/order and commercial configuration conventions.

Application:

- `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` — existing Application-layer DbSet exposure pattern.
- `backend/src/NursingPlatform.Application/Authorization/Permissions.cs` — existing static permission constant pattern.
- `backend/src/NursingPlatform.Application/Exams/Admin/` — existing admin CQRS, DTO, validator, mapping, and handler organization.
- `backend/src/NursingPlatform.Application/Exams/Queries/ListExams/ListExamsQueryHandler.cs` — existing safe catalog projection pattern.
- `backend/src/NursingPlatform.Application/Exams/Queries/ListExams/ListExamsQueryValidator.cs` — existing pagination validator pattern.

Infrastructure:

- `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs` — existing EF DbContext pattern.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs` — existing EF configuration style for exam entities.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs` — existing EF configuration style for price/commercial data.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` — existing role/permission seed pattern.
- `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` — existing EF migration location and naming pattern.

WebApi:

- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` — existing Minimal API endpoint registration style.
- `backend/src/NursingPlatform.WebApi/Extensions/AuthorizationExtensions.cs` — existing `RequirePermission(...)` extension.

Tests:

- `backend/tests/NursingPlatform.Domain.Tests/` — existing domain test project.
- `backend/tests/NursingPlatform.Application.Tests/` — existing handler, DTO, and validator test project.
- `backend/tests/NursingPlatform.Infrastructure.Tests/` — existing persistence/configuration test project.
- `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/` — existing endpoint integration test project, WebApi test factory, and JWT/permission test helpers.

Planned file areas for Stage 1 implementation:

Domain:

- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageDefinition.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageVersion.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageVersionStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageVersionMaterial.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageOffer.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PreparationPackageOfferStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/StudyMaterial.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/StudyMaterialVersion.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/StudyMaterialType.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PublicationStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/StudyMaterialVersionTopic.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeCollection.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeCollectionVersion.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeItem.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/PracticeAnswerOption.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/ReportingTopic.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/ReportingProfilePublication.cs`.
- Create `backend/src/NursingPlatform.Domain/PreparationPackages/ReportingProfileQuestionAssignment.cs`.

Application:

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose `DbSet<>` members for the new domain entities.
- Modify `backend/src/NursingPlatform.Application/Authorization/Permissions.cs` to add dedicated Stage 1 permissions only.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Common/PreparationPackageMapping.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Common/PreparationPackagePublicationValidator.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/DTOs/PreparationPackageDtos.cs`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/ReportingTopics/`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/ReportingProfiles/`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/StudyMaterials/`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/PracticeCollections/`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/PackageDefinitions/`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/PackageVersions/`.
- Create admin command/query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Admin/PackageOffers/`.
- Create public catalog query handler files under `backend/src/NursingPlatform.Application/PreparationPackages/Catalog/`.

Infrastructure:

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs` to expose `DbSet<>` members.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` to seed dedicated permissions.
- Create one EF Core migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` during approved implementation.

WebApi:

- Create `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` as a deliberate new endpoint-group extraction to avoid further growing the existing large `ApplicationBuilderExtensions.cs`; keep the registration call in `ApplicationBuilderExtensions.cs` and follow existing Minimal API conventions.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` to map the new endpoints.

Tests:

- Create `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PreparationPackageDomainTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageDtoSecurityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageValidatorTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageHandlerTests.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PreparationPackageCatalogEndpointTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AdminPreparationPackageEndpointTests.cs`.
- Modify existing exam/payment WebApi or Application tests only to prove existing standalone behavior remains unchanged if implementation touches shared code.

## Proposed Implementation Slices

### Slice 1: Domain Model and Domain Tests

**Objective:** Add Stage 1 domain entities and enums with no Application, Infrastructure, or WebApi behavior.

**Affected layer(s):** Domain and Domain.Tests only.

**Expected files or file areas:**

- Create all domain files listed in `Existing Codebase Reconnaissance` under `backend/src/NursingPlatform.Domain/PreparationPackages/`.
- Test `backend/tests/NursingPlatform.Domain.Tests/PreparationPackages/PreparationPackageDomainTests.cs`.

**Tests to write first:**

- `PublishedMaterialVersion_IsImmutableAfterPublish`
- `PublishedPracticeCollectionVersion_IsImmutableAfterPublish`
- `PublishedReportingProfile_IsImmutableAfterPublish`
- `PublishedPackageVersion_IsImmutableAfterPublish`
- `PackageVersion_ReferencesExactlyOneExamVersionOneReportingProfileAndOnePracticeCollectionVersion`
- `PackageVersion_MaterialsHaveDeterministicPositiveOrdering`
- `Offer_CarriesCommercialConfigurationWithoutComponentSelection`

**Implementation summary:**

- Produce these interfaces/types:
  - `PreparationPackageDefinition : AuditableEntity`
  - `PreparationPackageVersion : AuditableEntity`
  - `PreparationPackageVersionStatus { Draft, Published, Retired }`
  - `PreparationPackageVersionMaterial : AuditableEntity`
  - `PreparationPackageOffer : AuditableEntity`
  - `PreparationPackageOfferStatus { Draft, Active, Inactive, Retired }`
  - `StudyMaterial : AuditableEntity`
  - `StudyMaterialVersion : AuditableEntity`
  - `StudyMaterialType { File, ExternalLink, Video, FormattedText }`
  - `PublicationStatus { Draft, Published, Retired }`
  - `StudyMaterialVersionTopic : AuditableEntity`
  - `PracticeCollection : AuditableEntity`
  - `PracticeCollectionVersion : AuditableEntity`
  - `PracticeItem : AuditableEntity`
  - `PracticeAnswerOption : AuditableEntity`
  - `ReportingTopic : AuditableEntity`
  - `ReportingProfilePublication : AuditableEntity`
  - `ReportingProfileQuestionAssignment : AuditableEntity`
- Keep business behavior local to domain methods such as `Publish(...)`, `Retire(...)`, `Activate(...)`, `Deactivate(...)`, and update methods guarded by status.
- Use existing `AuditableEntity` conventions.
- Use explicit foreign-key ids and navigation collections consistent with existing `Exam` and payment entities.
- `PreparationPackageDefinition` stores stable catalog identity, `CountryId`, and `ExamCategoryId`.
- `PreparationPackageVersion` stores `PreparationPackageDefinitionId`, `ExamVersionId`, `ReportingProfilePublicationId`, `PracticeCollectionVersionId`, status, publication timestamp, and content-isolation confirmation fields.
- `PreparationPackageVersionMaterial` stores `PreparationPackageVersionId`, `StudyMaterialVersionId`, and one-based `SortOrder`.
- `PreparationPackageOffer` stores `PreparationPackageDefinitionId`, `PreparationPackageVersionId`, `Price`, `Currency`, `AccessDurationDays`, `Status`, and catalog display fields such as title, slug, summary, and published timestamps.
- `StudyMaterialVersion` supports File, External link, Video, and Formatted text through nullable type-specific content fields. It does not implement file storage or delivery.
- `PracticeItem` belongs to one `PracticeCollectionVersion`, has exactly one `ReportingTopicId`, and owns answer options and immediate-feedback text. It must not reference `ExamQuestion`, `ExamAnswerOption`, or exam snapshots.
- `ReportingTopic` belongs to exactly one `ExamCategoryId`.
- `ReportingProfilePublication` is bound to exactly one `ExamVersionId` and is separate from `ExamVersion`.
- `ReportingProfileQuestionAssignment` maps each required scored `ExamQuestionId` to exactly one `ReportingTopicId`.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PreparationPackage"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Domain immutability, additive model boundaries, exact component relationships, topic/profile separation from `ExamVersion`, and no leakage of later-stage entitlement/session/report concepts.

**Explicit non-scope:** No Application handlers, no EF mappings, no migrations, no WebApi endpoints, no checkout, no entitlements, no package attempts, no reports, no frontend.

### Slice 2: Application Contracts, DTO Security, Validators, and Permissions

**Objective:** Add Stage 1 permissions, DTOs, CQRS request records, validators, and DTO security tests without persistence configuration or endpoint mappings.

**Affected layer(s):** Application and Application.Tests only.

**Expected files or file areas:**

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Application/Authorization/Permissions.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/DTOs/PreparationPackageDtos.cs`.
- Create command/query records and validators under the Application folders listed in `Existing Codebase Reconnaissance`.
- Test `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageDtoSecurityTests.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageValidatorTests.cs`.

**Tests to write first:**

- `CatalogDtos_ShouldNotExposeProtectedExamContentOrInternalAuthorizationState`
- `AdminDtos_ShouldNotExposePasswordHashesTokensPaymentProviderIdsOrNavigationObjects`
- `Validate_Pagination_WithInvalidPageOrPageSize_ShouldHaveError`
- `Validate_CreateReportingTopic_WithEmptyNameOrCategoryId_ShouldHaveError`
- `Validate_PublishReportingProfile_WithMissingAssignments_ShouldHaveError`
- `Validate_MaterialVersion_WithUnsupportedMaterialTypeFields_ShouldHaveError`
- `Validate_MaterialVersion_WithNoTopicMappings_ShouldHaveError`
- `Validate_PracticeItem_WithNoTopicOrNoCorrectAnswer_ShouldHaveError`
- `Validate_PackageVersion_WithDuplicateMaterialSortOrder_ShouldHaveError`
- `Validate_PackageOffer_WithInvalidPriceCurrencyOrDuration_ShouldHaveError`

**Implementation summary:**

- Produce public DTOs: `PreparationPackageOfferListItemDto`, `PreparationPackageOfferDetailDto`, `PreparationPackageCatalogComponentSummaryDto`.
- Produce admin DTOs: `AdminReportingTopicDto`, `AdminReportingProfilePublicationDto`, `AdminStudyMaterialDto`, `AdminStudyMaterialVersionDto`, `AdminPracticeCollectionDto`, `AdminPracticeCollectionVersionDto`, `AdminPracticeItemDto`, `AdminPracticeAnswerOptionDto`, `AdminPreparationPackageDefinitionDto`, `AdminPreparationPackageVersionDto`, `AdminPreparationPackageOfferDto`, `PackagePublicationValidationDto`, `PackagePublicationValidationIssueDto`.
- Produce request DTOs and query/command records for each endpoint listed in `API Plan`.
- Validators enforce `Page >= 1`, `PageSize` between `1` and `100`, non-empty route ids, non-empty names/titles/slugs, price `>= 0`, three-letter uppercase currency, and `AccessDurationDays >= 1`.
- Material version validation enforces exactly the fields required by the selected material type and rejects incompatible field combinations.
- Material versions require at least one reporting topic before publication.
- Practice items require exactly one reporting topic and at least two options with exactly one correct option before collection publication.
- Reporting profile publication requires one assignment for every scored active question in the exact exam version.
- Public catalog DTOs expose only safe selling information: title, slug, country/category, included exam identity, material count/summary, practice item count/summary, access duration, price, and currency.
- Public catalog DTOs must not expose question text, answer keys, protected answer identifiers, options, rationales, scoring internals, report internals, internal authorization state, or draft content.
- Admin DTOs may expose authoring content but must not serialize EF/domain navigation graphs.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PreparationPackageDto|PreparationPackageValidator"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Dedicated permissions, safe DTO shape, validator boundaries, raw-sensitive-field protections, and no WebApi/Infrastructure dependencies in Application.

**Explicit non-scope:** No EF migrations, no endpoint mappings, no handler persistence behavior beyond contracts, no checkout, no entitlements, no package attempts, no reports, no frontend.

### Slice 3: EF Core Configuration, Migration, and Permission Seeding

**Objective:** Persist the Stage 1 domain model with EF Core configurations, one migration, and seeded dedicated permissions.

**Affected layer(s):** Infrastructure and Infrastructure.Tests, with Application/Domain types consumed from earlier slices.

**Expected files or file areas:**

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs`.
- Create one migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` during approved implementation.
- Test `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PreparationPackageConfigurationTests.cs`.

**Tests to write first:**

- `PreparationPackageConfiguration_CreatesRequiredUniqueIndexes`
- `PreparationPackageConfiguration_EnforcesOneActiveOfferPerPackageDefinition`
- `PreparationPackageConfiguration_EnforcesUniqueMaterialSortOrderPerPackageVersion`
- `PreparationPackageConfiguration_EnforcesOneTopicAssignmentPerReportingProfileQuestion`
- `ReferenceDataSeeder_SeedsDedicatedPreparationPackagePermissions`

**Implementation summary:**

- Configure table names with a consistent `PreparationPackage...` prefix unless existing conventions require otherwise.
- Configure required foreign keys to existing `Countries`, `ExamCategories`, `Exams`, `ExamVersions`, and `ExamQuestions` without modifying existing exam entity behavior.
- Configure decimal precision for offer price consistently with payment price precision.
- Configure a filtered unique index for one active offer per package definition.
- Configure unique indexes for slugs, reporting-topic names per category, reporting-profile assignment uniqueness, material sort order per package version, and package-version composition constraints.
- Configure delete behavior conservatively: restrict deletes for published or referenced content; prefer archive/retire application behavior.
- Generate the migration with `dotnet ef migrations add AddPreparationPackageStage1 --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi` only during the approved implementation session.
- Do not manually edit the database or bypass `ApplicationDbContext`.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PreparationPackageConfiguration|ReferenceDataSeeder"`
- `dotnet ef migrations script --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --idempotent --output /tmp/opencode/preparation-package-stage1.sql`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** EF constraints/indexes, migration scope, seed idempotency, one-active-offer enforcement, relationship delete behavior, and no modification to existing exam/payment semantics.

**Explicit non-scope:** No WebApi endpoints, no runtime fulfillment or entitlement tables, no package attempt/session provenance schema, no report tables, no manual schema edits.

### Slice 4: Application Handlers and Publication Validation

**Objective:** Implement CQRS handlers for Stage 1 admin authoring, publication, retirement, offers, and public safe catalog queries.

**Affected layer(s):** Application and Application.Tests.

**Expected files or file areas:**

- Create `backend/src/NursingPlatform.Application/PreparationPackages/Common/PreparationPackageMapping.cs`.
- Create `backend/src/NursingPlatform.Application/PreparationPackages/Common/PreparationPackagePublicationValidator.cs`.
- Create/complete handler files under all `backend/src/NursingPlatform.Application/PreparationPackages/` admin and catalog folders.
- Test `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/PreparationPackageHandlerTests.cs`.

**Tests to write first:**

- `Handle_CreateReportingTopic_CreatesActiveTopicForExistingExamCategory`
- `Handle_PublishReportingProfile_WhenAssignmentsCoverEveryScoredQuestion_PublishesImmutableProfile`
- `Handle_PublishReportingProfile_WhenQuestionMissingTopicAssignment_ThrowsInvalidOperationException`
- `Handle_PublishReportingProfile_WhenTopicCategoryMismatch_ThrowsInvalidOperationException`
- `Handle_PublishMaterialVersion_WhenTopicsPresent_PublishesVersion`
- `Handle_UpdateMaterialVersion_WhenPublished_ThrowsInvalidOperationException`
- `Handle_RetireMaterialVersion_DoesNotMutatePublishedPackageVersions`
- `Handle_PublishPracticeCollectionVersion_WhenItemsAreValid_PublishesVersion`
- `Handle_PublishPracticeCollectionVersion_WhenItemReferencesExamQuestion_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenAllComponentsCompatible_PublishesImmutableSnapshot`
- `Handle_PublishPackageVersion_WhenExamVersionNotPublished_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenReportingProfileBoundToDifferentExamVersion_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenMaterialRetired_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenPracticeCollectionNotPublished_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenMaterialTopicOutsideExamCategory_ThrowsInvalidOperationException`
- `Handle_ActivateOffer_WhenAnotherOfferActiveForSamePackageDefinition_ThrowsInvalidOperationException`
- `Handle_ActivateOffer_WhenPackageVersionNotPublished_ThrowsInvalidOperationException`
- `Handle_ListCatalogOffers_ReturnsOnlyActiveEligibleOffers`
- `Handle_GetCatalogOffer_DoesNotExposeProtectedExamOrPracticeAnswers`
- `Handle_StandaloneExamAccessQueries_RemainUnaffectedByPackageOffers`

**Implementation summary:**

- Use `IApplicationDbContext` only; no WebApi or Infrastructure dependencies in handlers.
- Use existing mapped exception types: `KeyNotFoundException` for missing/hidden resources, `InvalidOperationException` for conflicts, and FluentValidation for request validation.
- Package publication validation checks that the package definition, exam version, reporting profile publication, material versions, and practice collection version all exist, are published/eligible, and share the package definition's country/category context.
- Reporting profile compatibility is exact `ReportingProfilePublication.ExamVersionId == PreparationPackageVersion.ExamVersionId`.
- Practice collection content-isolation validation is structural: practice items must be dedicated practice entities, must not carry exam question ids or answer option ids, and must not reference exam snapshots. If semantic text-leak detection beyond structural isolation is required, stop for business clarification before implementing it.
- Material and practice retirement blocks new package publication and new offer activation, but does not mutate already published package versions.
- Offer activation re-validates sellability and enforces at most one active offer per package definition.
- Catalog queries return only active offers whose referenced package version and components remain eligible for new sales.
- Catalog queries must use explicit projection and must not include draft content, practice answer options, correct answer flags, protected exam content, or internal validation details.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PreparationPackageHandler"`
- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ExamAccess|ListExams|GetExam|StartExam"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Publication eligibility, immutable snapshots, content-isolation boundaries, safe projection, deterministic sorting, conflict mapping, and preservation of standalone exam/payment behavior.

**Explicit non-scope:** No workspace, no entitlement, no checkout, no package purchase, no practice runtime, no package exam attempt, no report handlers, no frontend.

### Slice 5: Minimal API Endpoint Mappings and Integration Tests

**Objective:** Expose only Stage 1 public catalog and admin endpoints through thin Minimal API mappings with exact authorization requirements.

**Affected layer(s):** WebApi and WebApi.Tests, consuming Application handlers from earlier slices.

**Expected files or file areas:**

- Create `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs`.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`.
- Test `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PreparationPackageCatalogEndpointTests.cs`.
- Test `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AdminPreparationPackageEndpointTests.cs`.

**Tests to write first:**

- `GetCatalogOffers_AllowsAnonymousAndReturnsSafeJson`
- `GetCatalogOffer_DoesNotExposePasswordHashTokensProtectedExamContentCorrectAnswersOrInternalAuthorizationState`
- `AdminReportingTopicCreate_Returns401WithoutJwt`
- `AdminReportingTopicCreate_Returns403WithoutReportingTopicsManagePermission`
- `AdminReportingTopicCreate_Returns201WithReportingTopicsManagePermission`
- `AdminPackageVersionPublish_Returns403WithoutPreparationPackagesPublishPermission`
- `AdminPackageVersionPublish_Returns200WithPreparationPackagesPublishPermission`
- `AdminOfferActivate_EnforcesPreparationPackageOffersManagePermission`
- `AdminEndpoints_DoNotAcceptEmployerPackageReportPracticeProgressOrPurchaseHistoryRoutes`
- `ExistingExamCatalogAndStartEndpoints_RemainBackwardCompatible`

**Implementation summary:**

- Follow existing endpoint mapping style from `ApplicationBuilderExtensions.cs` and `AuthorizationExtensions.cs`.
- Because `PreparationPackageEndpointExtensions.cs` is a deliberate new endpoint-group extraction, include the required imports for permission constants and the existing `RequirePermission` extension: `using NursingPlatform.Application.Authorization;` and `using NursingPlatform.WebApi.Extensions;`.
- Use `ISender`/MediatR from endpoints and keep endpoint bodies thin.
- Return DTOs only. Never return EF/domain entities.
- Use existing exception middleware and Problem Details mapping.
- Public catalog raw JSON tests must inspect the response string for forbidden fields before deserializing.
- Permission-protected endpoint tests must prove `401` unauthenticated, `403` authenticated without exact permission, and success with exact permission.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PreparationPackageCatalog|AdminPreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ExamEndpoints|PaymentEndpoints"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

**Review focus:** Endpoint scope, exact permissions, raw JSON secrecy, thin WebApi mapping, Problem Details consistency, and no unauthorized employer/package/later-stage routes.

**Explicit non-scope:** No route for checkout, payment, entitlement, workspace, package attempt, practice runtime, report status, report access, employer package data, frontend, or later stages.

### Slice 6: End-to-End Compatibility, Documentation Decision, and Final Verification

**Objective:** Verify Stage 1 behavior, confirm no out-of-scope changes, and update authoritative API/database documentation only if the approved implementation prompt explicitly includes documentation updates.

**Affected layer(s):** Verification across Domain, Application, Infrastructure, WebApi, and documentation only if explicitly authorized.

**Expected files or file areas:**

- Modify `docs/api/api-design.md` only if explicitly authorized for documentation synchronization after implementation.
- Modify `docs/database/database-design.md` only if explicitly authorized for documentation synchronization after implementation.
- Do not modify `CURRENT_TASK.md`, `TASKS.md`, approved specs, frontend/design files, `.agent/goal-state.md`, `README`, or `CHANGELOG` unless separately instructed.

**Tests to write first:**

- No new feature tests should be introduced in this final verification slice unless review discovers a missed Stage 1 acceptance criterion. If a gap is found, add the smallest missing test in the appropriate earlier test area and re-run the relevant slice verification.

**Implementation summary:**

- Run the complete Stage 1 test set and compatibility checks.
- Confirm no Stage 2, Stage 3, Stage 4, frontend, employer-facing package, checkout, entitlement, package attempt, report, AI, subscription, adaptive practice, or marketplace behavior has been added.
- Confirm no protected files or unrelated files changed.
- Confirm no files are staged and no commit is made unless separately authorized.

**Validation commands:**

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PreparationPackage|ReferenceDataSeeder"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ExamEndpoints|PaymentEndpoints|ExamAnalytics"`
- `dotnet build backend/NursingPlatform.slnx`
- `git diff --check`
- `git status --short --untracked-files=all`
- `git diff -- CURRENT_TASK.md TASKS.md docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md .agent/goal-state.md`

**Review focus:** Full verification evidence, source-of-truth documentation boundaries, no unrelated changes, no staged files, no commit, and final evidence completeness.

**Explicit non-scope:** Do not proceed to Stage 2. Do not stage. Do not commit. Do not update task tracking unless separately and explicitly authorized. Do not add implementation beyond verified Stage 1 scope.

## Suggested Slice Order

1. **Slice 1: Domain Model and Domain Tests** — dependency-safe first step because later layers need concrete domain types and invariants.
2. **Slice 2: Application Contracts, DTO Security, Validators, and Permissions** — depends on domain types and produces Application contracts required by handlers and endpoints.
3. **Slice 3: EF Core Configuration, Migration, and Permission Seeding** — depends on domain types and Application permission constants; produces persistence needed by handlers and integration tests.
4. **Slice 4: Application Handlers and Publication Validation** — depends on domain, Application contracts, validators, and persistence abstractions; produces use cases consumed by WebApi endpoints.
5. **Slice 5: Minimal API Endpoint Mappings and Integration Tests** — depends on handlers, DTOs, permissions, and persistence; exposes the approved Stage 1 HTTP surface.
6. **Slice 6: End-to-End Compatibility, Documentation Decision, and Final Verification** — dependency-safe final step because it verifies all earlier slices, compatibility behavior, documentation boundaries, and git hygiene.

This order keeps dependency direction intact: WebApi depends on Application, Infrastructure implements Application abstractions, and Domain remains independent of Infrastructure and WebApi.

## Permission and Authorization Plan

Add only these dedicated Stage 1 permissions:

- `PreparationPackages.View`
- `PreparationPackages.Manage`
- `PreparationPackages.Publish`
- `PreparationPackageOffers.Manage`
- `StudyMaterials.Manage`
- `PracticeCollections.Manage`
- `ReportingTopics.Manage`
- `ReportingProfiles.Manage`

Existing `Exams.*` and `Questions.*` permissions do not authorize the new package, material, practice, reporting-topic, reporting-profile, package-version publication, or offer/pricing administration endpoints.

Authorization requirements:

- Public catalog endpoints use `.AllowAnonymous()` and must return safe active-offer catalog data only.
- Admin list/detail endpoints use the matching `*.View` or `*.Manage` permission specified above.
- Admin write/publish/retire/activate/deactivate endpoints use the matching `*.Manage` or `PreparationPackages.Publish` permission specified above.
- Do not use existing `Exams.*` or `Questions.*` permissions for Stage 1 Preparation Package admin endpoints.
- Permission-protected endpoint tests must prove `401` unauthenticated, `403` authenticated without exact permission, and success with exact permission.
- Backend authorization remains authoritative; no frontend-only or client-trusted authorization is allowed.

## Database and Migration Plan

A migration is required in the approved implementation because Stage 1 introduces new persisted catalog, authoring, publication, and offer entities. Generate exactly one Stage 1 migration after domain entities and EF configurations compile. Do not hand-edit schema outside EF migrations.

Migration constraints:

- EF Core Code-First migrations only.
- Generate migration only during a separately approved implementation session.
- Do not manually edit the database.
- Do not bypass `ApplicationDbContext`.
- Do not introduce entitlement, benefit-right, package-attempt, report, report-access, fulfillment, checkout, subscription, AI, adaptive-practice, marketplace, employer-facing package, or frontend persistence.

Infrastructure files:

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PreparationPackageConfigurations.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs`.
- Create one migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/`.

Configuration requirements:

- Configure required foreign keys to existing `Countries`, `ExamCategories`, `Exams`, `ExamVersions`, and `ExamQuestions` without modifying existing exam entity behavior.
- Configure decimal precision for offer price consistently with payment price precision.
- Configure a filtered unique index for one active offer per package definition.
- Configure unique indexes for slugs, reporting-topic names per category, reporting-profile assignment uniqueness, material sort order per package version, and package-version composition constraints.
- Configure delete behavior conservatively: restrict deletes for published or referenced content; prefer archive/retire application behavior.

Migration verification command during approved implementation:

- `dotnet ef migrations script --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --idempotent --output /tmp/opencode/preparation-package-stage1.sql`

## API Plan

The endpoint paths below are proposed Stage 1 endpoint paths subject to implementation review. During implementation they must follow existing project Minimal API conventions, route grouping style, versioning conventions, DTO conventions, authorization extension patterns, and Problem Details behavior.

Implement only these proposed Stage 1 endpoints:

- Public catalog:
  - `GET /api/v1/preparation-packages/offers`
  - `GET /api/v1/preparation-packages/offers/{slug}`
- Admin reporting topics:
  - `GET /api/v1/admin/preparation-package/reporting-topics`
  - `POST /api/v1/admin/preparation-package/reporting-topics`
  - `PUT /api/v1/admin/preparation-package/reporting-topics/{id}`
  - `POST /api/v1/admin/preparation-package/reporting-topics/{id}/archive`
- Admin reporting profiles:
  - `GET /api/v1/admin/preparation-package/reporting-profiles`
  - `POST /api/v1/admin/preparation-package/reporting-profiles`
  - `GET /api/v1/admin/preparation-package/reporting-profiles/{id}`
  - `POST /api/v1/admin/preparation-package/reporting-profiles/{id}/publish`
- Admin study materials:
  - `GET /api/v1/admin/preparation-package/materials`
  - `POST /api/v1/admin/preparation-package/materials`
  - `POST /api/v1/admin/preparation-package/materials/{materialId}/versions`
  - `PUT /api/v1/admin/preparation-package/materials/{materialId}/versions/{versionId}`
  - `POST /api/v1/admin/preparation-package/materials/{materialId}/versions/{versionId}/publish`
  - `POST /api/v1/admin/preparation-package/materials/{materialId}/versions/{versionId}/retire`
- Admin practice collections:
  - `GET /api/v1/admin/preparation-package/practice-collections`
  - `POST /api/v1/admin/preparation-package/practice-collections`
  - `POST /api/v1/admin/preparation-package/practice-collections/{collectionId}/versions`
  - `PUT /api/v1/admin/preparation-package/practice-collections/{collectionId}/versions/{versionId}`
  - `POST /api/v1/admin/preparation-package/practice-collections/{collectionId}/versions/{versionId}/publish`
  - `POST /api/v1/admin/preparation-package/practice-collections/{collectionId}/versions/{versionId}/retire`
- Admin package definitions and versions:
  - `GET /api/v1/admin/preparation-package/packages`
  - `POST /api/v1/admin/preparation-package/packages`
  - `PUT /api/v1/admin/preparation-package/packages/{id}`
  - `POST /api/v1/admin/preparation-package/packages/{packageId}/versions`
  - `GET /api/v1/admin/preparation-package/packages/{packageId}/versions/{versionId}/validation`
  - `POST /api/v1/admin/preparation-package/packages/{packageId}/versions/{versionId}/publish`
  - `POST /api/v1/admin/preparation-package/packages/{packageId}/versions/{versionId}/retire`
- Admin offers:
  - `GET /api/v1/admin/preparation-package/offers`
  - `POST /api/v1/admin/preparation-package/offers`
  - `PUT /api/v1/admin/preparation-package/offers/{id}`
  - `POST /api/v1/admin/preparation-package/offers/{id}/activate`
  - `POST /api/v1/admin/preparation-package/offers/{id}/deactivate`

Do not implement checkout, purchase, entitlement, workspace, material delivery/download, practice runtime, package-attempt start, report status, report access, employer-facing package, or frontend endpoints in Stage 1.

## Test Strategy

Testing must follow TDD where practical: write failing tests first, implement the smallest production behavior needed to pass, then refactor while keeping tests green.

Domain tests:

- `PublishedMaterialVersion_IsImmutableAfterPublish`
- `PublishedPracticeCollectionVersion_IsImmutableAfterPublish`
- `PublishedReportingProfile_IsImmutableAfterPublish`
- `PublishedPackageVersion_IsImmutableAfterPublish`
- `PackageVersion_ReferencesExactlyOneExamVersionOneReportingProfileAndOnePracticeCollectionVersion`
- `PackageVersion_MaterialsHaveDeterministicPositiveOrdering`
- `Offer_CarriesCommercialConfigurationWithoutComponentSelection`

Application DTO and validator tests:

- `CatalogDtos_ShouldNotExposeProtectedExamContentOrInternalAuthorizationState`
- `AdminDtos_ShouldNotExposePasswordHashesTokensPaymentProviderIdsOrNavigationObjects`
- `Validate_Pagination_WithInvalidPageOrPageSize_ShouldHaveError`
- `Validate_CreateReportingTopic_WithEmptyNameOrCategoryId_ShouldHaveError`
- `Validate_PublishReportingProfile_WithMissingAssignments_ShouldHaveError`
- `Validate_MaterialVersion_WithUnsupportedMaterialTypeFields_ShouldHaveError`
- `Validate_MaterialVersion_WithNoTopicMappings_ShouldHaveError`
- `Validate_PracticeItem_WithNoTopicOrNoCorrectAnswer_ShouldHaveError`
- `Validate_PackageVersion_WithDuplicateMaterialSortOrder_ShouldHaveError`
- `Validate_PackageOffer_WithInvalidPriceCurrencyOrDuration_ShouldHaveError`

Infrastructure tests:

- `PreparationPackageConfiguration_CreatesRequiredUniqueIndexes`
- `PreparationPackageConfiguration_EnforcesOneActiveOfferPerPackageDefinition`
- `PreparationPackageConfiguration_EnforcesUniqueMaterialSortOrderPerPackageVersion`
- `PreparationPackageConfiguration_EnforcesOneTopicAssignmentPerReportingProfileQuestion`
- `ReferenceDataSeeder_SeedsDedicatedPreparationPackagePermissions`

Application handler tests:

- `Handle_CreateReportingTopic_CreatesActiveTopicForExistingExamCategory`
- `Handle_PublishReportingProfile_WhenAssignmentsCoverEveryScoredQuestion_PublishesImmutableProfile`
- `Handle_PublishReportingProfile_WhenQuestionMissingTopicAssignment_ThrowsInvalidOperationException`
- `Handle_PublishReportingProfile_WhenTopicCategoryMismatch_ThrowsInvalidOperationException`
- `Handle_PublishMaterialVersion_WhenTopicsPresent_PublishesVersion`
- `Handle_UpdateMaterialVersion_WhenPublished_ThrowsInvalidOperationException`
- `Handle_RetireMaterialVersion_DoesNotMutatePublishedPackageVersions`
- `Handle_PublishPracticeCollectionVersion_WhenItemsAreValid_PublishesVersion`
- `Handle_PublishPracticeCollectionVersion_WhenItemReferencesExamQuestion_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenAllComponentsCompatible_PublishesImmutableSnapshot`
- `Handle_PublishPackageVersion_WhenExamVersionNotPublished_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenReportingProfileBoundToDifferentExamVersion_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenMaterialRetired_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenPracticeCollectionNotPublished_ThrowsInvalidOperationException`
- `Handle_PublishPackageVersion_WhenMaterialTopicOutsideExamCategory_ThrowsInvalidOperationException`
- `Handle_ActivateOffer_WhenAnotherOfferActiveForSamePackageDefinition_ThrowsInvalidOperationException`
- `Handle_ActivateOffer_WhenPackageVersionNotPublished_ThrowsInvalidOperationException`
- `Handle_ListCatalogOffers_ReturnsOnlyActiveEligibleOffers`
- `Handle_GetCatalogOffer_DoesNotExposeProtectedExamOrPracticeAnswers`
- `Handle_StandaloneExamAccessQueries_RemainUnaffectedByPackageOffers`

WebApi integration tests:

- `GetCatalogOffers_AllowsAnonymousAndReturnsSafeJson`
- `GetCatalogOffer_DoesNotExposePasswordHashTokensProtectedExamContentCorrectAnswersOrInternalAuthorizationState`
- `AdminReportingTopicCreate_Returns401WithoutJwt`
- `AdminReportingTopicCreate_Returns403WithoutReportingTopicsManagePermission`
- `AdminReportingTopicCreate_Returns201WithReportingTopicsManagePermission`
- `AdminPackageVersionPublish_Returns403WithoutPreparationPackagesPublishPermission`
- `AdminPackageVersionPublish_Returns200WithPreparationPackagesPublishPermission`
- `AdminOfferActivate_EnforcesPreparationPackageOffersManagePermission`
- `AdminEndpoints_DoNotAcceptEmployerPackageReportPracticeProgressOrPurchaseHistoryRoutes`
- `ExistingExamCatalogAndStartEndpoints_RemainBackwardCompatible`

Final verification commands:

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PreparationPackage|ReferenceDataSeeder"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PreparationPackage"`
- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ExamEndpoints|PaymentEndpoints|ExamAnalytics"`
- `dotnet build backend/NursingPlatform.slnx`
- `git diff --check`
- `git status --short --untracked-files=all`
- `git diff -- CURRENT_TASK.md TASKS.md docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md docs/superpowers/specs/2026-07-26-preparation-package-stage-1-catalog-authoring.md .agent/goal-state.md`

## Security and Privacy Review Checklist

- Catalog responses must not expose protected exam questions, answer keys, protected answer identifiers, protected options, exam rationales, internal scoring logic, internal report logic, raw tokens, secrets, internal authorization state, or EF/domain navigation objects.
- Draft material and draft practice content are not nurse-accessible.
- Published package versions, published material versions, published practice collection versions, and published reporting profile publications are immutable.
- Public catalog DTOs expose only safe selling information: title, slug, country/category, included exam identity, material count/summary, practice item count/summary, access duration, price, and currency.
- Public catalog DTOs must not expose question text, answer keys, protected answer identifiers, options, rationales, scoring internals, report internals, internal authorization state, or draft content.
- Admin DTOs may expose authoring content but must not serialize EF/domain navigation graphs.
- Catalog queries must use explicit projection and must not include draft content, practice answer options, correct answer flags, protected exam content, or internal validation details.
- Return DTOs only. Never return EF/domain entities.
- Public catalog raw JSON tests must inspect the response string for forbidden fields before deserializing.
- Admin endpoint tests must include absence of employer package report, practice-progress, or purchase-history routes.
- Practice content must remain logically and structurally distinct from exam question content.
- Backend authorization remains authoritative for admin and catalog behavior.

## Review and Verification Gates

Before implementation begins:

- The plan must be reviewed and explicitly approved by the user.
- The plan must receive independent review confirmation that it stays within the approved Stage 1 specification.
- No source-code, test, migration, staging, or commit work may begin from this plan alone.

During approved implementation:

- Each slice must stop at its review gate unless the user explicitly approves batch continuation.
- Each slice must run its listed validation commands.
- Each slice must confirm no out-of-scope files were modified.
- Each slice must confirm no files were staged and no commit was made unless separately authorized.

Final verification gate:

- Run the complete final verification command set listed in `Test Strategy`.
- Run `git diff --check`.
- Run `git status --short --untracked-files=all`.
- Run the protected-file diff check for `CURRENT_TASK.md`, `TASKS.md`, approved specs, and `.agent/goal-state.md`.
- Paste real command outputs for review.
- Stop for review. Do not proceed to Stage 2. Do not stage. Do not commit.

Independent review requirements before implementation:

- Confirm the plan stays within the approved Stage 1 specification.
- Confirm the plan does not implement Stage 2 fulfillment, entitlements, benefit-right authorization, or access-window enforcement.
- Confirm the plan does not implement Stage 3 package-attempt start, session provenance, concurrency, or `ExamAccessGrant` changes.
- Confirm the plan does not implement Stage 4 report generation/access/guidance.
- Confirm the planned entities and endpoints protect protected exam content and sensitive fields.
- Confirm the planned tests prove permission behavior, DTO secrecy with raw JSON inspection, pagination where applicable, immutability, publication eligibility, and standalone compatibility.

## Risks and Mitigations

- Risk: Scope creep into Stage 2 fulfillment, entitlement persistence, benefit-right authorization, or access-window enforcement.
  - Mitigation: Keep checkout, payment, fulfillment, entitlement, benefit-right, and access-window runtime behavior explicitly out of every slice.
- Risk: Scope creep into Stage 3 package-attempt behavior, session provenance, concurrency, or `ExamAccessGrant` changes.
  - Mitigation: Preserve existing standalone start behavior and do not add package-attempt routes, session-source fields, attempt consumption, or grant compatibility changes.
- Risk: Scope creep into Stage 4 report generation, report persistence, report access, or guidance behavior.
  - Mitigation: Limit Stage 1 to reporting topics and reporting-profile publication eligibility only; do not add report runtime entities, jobs, endpoints, or classifications.
- Risk: Protected exam content leakage through catalog, materials, practice, DTOs, or JSON responses.
  - Mitigation: Use safe DTOs, explicit projection, structural practice/exam separation, and raw JSON secrecy tests.
- Risk: Incorrect authorization caused by reusing existing exam permissions.
  - Mitigation: Seed and require dedicated Stage 1 permissions; tests must prove 401, 403, and authorized success with exact permissions.
- Risk: Migration/schema drift or hand-edited database changes.
  - Mitigation: Use exactly one EF Core migration in an approved implementation session; verify with an idempotent migration script; do not manually modify schema.
- Risk: Existing standalone exam/payment behavior regression.
  - Mitigation: Run compatibility tests for exam catalog/start/access behavior and payment endpoints; package offers must not participate in standalone effective-paid classification.
- Risk: Endpoint-file pattern inconsistency.
  - Mitigation: Use `PreparationPackageEndpointExtensions.cs` deliberately as an endpoint-group extraction while keeping registration in `ApplicationBuilderExtensions.cs` and following existing Minimal API conventions.

## Explicit Later-Stage Hand-off

Stage 1 hands off catalog and authoring foundations only. It must not implement later-stage runtime behavior.

Stage 2 receives from Stage 1, after separately approved implementation:

- Package definitions and immutable package versions as approved catalog identities and composition snapshots.
- Package offers as sellable catalog concepts with price, currency, and access duration.
- Eligibility rules identifying when a package offer may be sold.
- Material and practice publication identities needed for future benefit-right authorization.
- Workspace concept inputs that require entitlement-backed access data later.

Stage 3 receives from Stage 1, after separately approved implementation:

- The exact published exam version referenced by the purchased package version.
- The package attempt entry concept from the workspace.
- The rule that standalone/free starts and package-attempt starts remain separate.
- The rule that package catalog and workspace surfaces must not silently consume package attempts.
- The later package-attempt design constraint that the system never silently selects an entitlement.
- The later session-concurrency design constraint that package exam attempt entry is subject to the one-in-progress-session-per-nurse-per-exam-version rule regardless of access source.

Stage 4 receives from Stage 1, after separately approved implementation:

- Reporting topics.
- Compatible reporting profile publications bound to exact published exam versions.
- Material-version-to-topic mappings.
- Practice-item-to-topic mappings.
- The rule that deterministic guidance is restricted to the purchased package's material versions and practice collection version.
- The rule that practice performance is not v1 report classification evidence.

Explicit exclusions from this plan and Stage 1 implementation:

- No Stage 2 payment order processing, checkout behavior, idempotent fulfillment, entitlement persistence, benefit-right authorization, or runtime access-window enforcement.
- No Stage 3 package-attempt start, attempt consumption, session provenance, source conflict handling, or `ExamAccessGrant` compatibility changes.
- No Stage 4 report generation, report persistence, report access, analytical classification, or guidance presentation.
- No frontend screens, frontend routes, design files, wireframes, Angular components, or visual layouts.
- No employer-facing package reports, package practice progress, or package purchase history.
- No subscriptions, carts, upgrade paths, refunds, package sharing, employer-sponsored packages, cohorts, team purchases, marketplace, creator marketplace, affiliate system, multi-vendor content, AI recommendations, adaptive practice, report sharing, cross-country packages, cross-category packages, multi-exam bundles, individually purchasable benefits, or purchase-time benefit selection.

Implementation requires explicit user approval after plan review. This plan alone does not authorize source-code changes, migrations, tests, staging, or commits.

## Acceptance Criteria for the Plan

This plan is acceptable for review when:

- It contains the required titled sections: Status, Purpose, Inputs, Scope Lock, Existing Codebase Reconnaissance, Proposed Implementation Slices, Suggested Slice Order, Permission and Authorization Plan, Database and Migration Plan, API Plan, Test Strategy, Security and Privacy Review Checklist, Review and Verification Gates, Risks and Mitigations, Explicit Later-Stage Hand-off, and Acceptance Criteria for the Plan.
- `Status` is exactly `Draft — Stage 1 Implementation Plan Pending Review`.
- `Purpose` states clearly that this is a plan only and does not authorize execution.
- The six implementation slices are under `Proposed Implementation Slices`.
- Each implementation slice explicitly includes objective, affected layer(s), expected files or file areas, tests to write first, implementation summary, validation commands, review focus, and explicit non-scope.
- The `Suggested Slice Order` explains why the order is dependency-safe.
- The `Security and Privacy Review Checklist` is a real titled section.
- The `Risks and Mitigations` content is a real titled section.
- The `Explicit Later-Stage Hand-off` content is a real titled section.
- The `API Plan` clarifies that listed endpoint paths are proposed Stage 1 endpoint paths subject to implementation review and must follow existing project conventions during implementation.
- It keeps all Stage 2, Stage 3, Stage 4, checkout, entitlement, package attempt, report generation, frontend, employer-facing package, AI, subscription, adaptive practice, and marketplace work explicitly out of scope.
- It does not add new business decisions.
- It does not modify approved specs, task docs, source code, tests, migrations, frontend/design files, or `.agent/goal-state.md`.
