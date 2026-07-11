# Phase 6D Contact Requests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Phase 6D Contact Requests as a secure Recruitment workflow where employers request contact with eligible nurse candidates and nurses approve, reject, or employers cancel requests without exposing contact info.

**Architecture:** Contact Requests belong conceptually under Recruitment while reusing existing Employer and Nurse role guards. Domain owns the `ContactRequest` aggregate and status enum, Application owns CQRS handlers, validation, ownership, lifecycle, duplicate, eligibility, and safe DTO projection, Infrastructure owns EF persistence/migration/indexes, and WebApi remains thin Minimal API routing. The approved atomic transition approach is an EF Core atomic conditional update scoped by `Id`, owner id, and `Status == Pending` with affected-row verification; this avoids provider-specific `RowVersion` requirements while preserving one-winner terminal transitions.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 6D: Contact Requests.
- Implement the approved spec exactly: `docs/superpowers/specs/2026-07-11-contact-requests.md`.
- Do not add a contact-info endpoint.
- Do not modify candidate listing, candidate filtering, or candidate sorting.
- Do not expose nurse email or phone.
- Do not expose employer user email or legal name.
- Do not accept or return employer messages, nurse rejection reasons, or cross-actor free text.
- Do not expose `UserId`, account internals, roles, permissions, tokens, password hashes, CV storage keys, CV URLs, file URLs, internal paths, license numbers, EF navigation objects, domain entities, or database entities.
- Do not expose `EmployerProfileId` or `EmployerOrganizationId` in API DTOs.
- Employer-facing DTO may expose `NurseProfileId` only because candidate listing already exposes it and employers need to identify the target candidate.
- Do not expose concurrency tokens in API DTOs.
- Required statuses are `Pending`, `Approved`, `Rejected`, and `Cancelled`.
- Pending duplicate request for the same employer profile and nurse profile returns `409 Conflict`.
- Approved duplicate request for the same employer profile and nurse profile returns `409 Conflict`.
- Rejected request history allows a new `Pending` request.
- Cancelled request history allows a new `Pending` request.
- Approved, Rejected, and Cancelled are terminal and immutable.
- Approve, reject, and cancel transitions must be atomic: one competing terminal transition may win and losing competing transitions return `409 Conflict`.
- Use EF Core atomic conditional update for transitions: update only rows matching request id, owner id, and `Status == Pending`; verify affected row count is one.
- Use `.RequireAuthorization()` for every Phase 6D endpoint; do not add `.AllowAnonymous()` or `.RequirePermission(...)` unless a later approved task changes the requirement.
- Do not add frontend, notifications, messaging/chat, payments, admin approval, Phase 7, or later-phase behavior.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` until Task 6 and only after Task 5 approval.
- Do not stage or commit until explicitly instructed.

---

## Planned File Structure

Domain:

- Create `backend/src/NursingPlatform.Domain/Recruitment/ContactRequestStatus.cs`: enum with `Pending`, `Approved`, `Rejected`, and `Cancelled`.
- Create `backend/src/NursingPlatform.Domain/Recruitment/ContactRequest.cs`: `AuditableEntity` aggregate with internal FKs, status, safe snapshots, response timestamps, and no contact-info/free-text fields.

Application contracts and DTOs:

- Create `backend/src/NursingPlatform.Application/Recruitment/DTOs/ContactRequestDto.cs`: employer-facing safe response with `Id`, `NurseProfileId`, status, safe candidate snapshots, timestamps only.
- Create `backend/src/NursingPlatform.Application/Recruitment/DTOs/ReceivedContactRequestDto.cs`: nurse-facing safe response with `Id`, organization name, job title, department, status, timestamps only.
- Create `backend/src/NursingPlatform.Application/Recruitment/DTOs/ContactRequestStatusDto.cs` only if implementation needs a string conversion helper; otherwise keep status mapping private in handlers.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CreateContactRequest/CreateContactRequestRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CreateContactRequest/CreateContactRequestCommand.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CreateContactRequest/CreateContactRequestCommandValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CreateContactRequest/CreateContactRequestCommandHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CancelContactRequest/CancelContactRequestCommand.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CancelContactRequest/CancelContactRequestCommandValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/CancelContactRequest/CancelContactRequestCommandHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/ApproveReceivedContactRequest/ApproveReceivedContactRequestCommand.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/ApproveReceivedContactRequest/ApproveReceivedContactRequestCommandValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/ApproveReceivedContactRequest/ApproveReceivedContactRequestCommandHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/RejectReceivedContactRequest/RejectReceivedContactRequestCommand.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/RejectReceivedContactRequest/RejectReceivedContactRequestCommandValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Commands/RejectReceivedContactRequest/RejectReceivedContactRequestCommandHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListMyContactRequests/ListMyContactRequestsQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListMyContactRequests/ListMyContactRequestsQueryValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListMyContactRequests/ListMyContactRequestsQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/GetMyContactRequest/GetMyContactRequestQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/GetMyContactRequest/GetMyContactRequestQueryValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/GetMyContactRequest/GetMyContactRequestQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListReceivedContactRequests/ListReceivedContactRequestsQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListReceivedContactRequests/ListReceivedContactRequestsQueryValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Queries/ListReceivedContactRequests/ListReceivedContactRequestsQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Recruitment/Common/ContactRequestMapping.cs`: shared DTO projection helpers for snapshots and string status mapping.
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`: expose `DbSet<ContactRequest> ContactRequests`.

Infrastructure:

- Create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ContactRequestConfiguration.cs`: table, properties, relationships, string enum conversion, indexes, active duplicate unique filtered index.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`: expose `DbSet<ContactRequest> ContactRequests`.
- Create generated EF migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` named `AddContactRequests` during Task 2.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` through EF migration generation only.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`: map the seven approved Phase 6D endpoints only.

Tests:

- Create `backend/tests/NursingPlatform.Domain.Tests/Recruitment/ContactRequestTests.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ContactRequestConfigurationTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequestTestData.cs` if shared test builders keep handler tests readable.
- Create Application tests under `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ContactRequestEndpointsTests.cs`.

---

### Task 1: Domain and Application Contract Foundation

**Goal:** Add the Contact Request domain shape, status enum, safe DTOs, MediatR request contracts, validators, `IApplicationDbContext` surface, and contract-level tests without EF configuration, migrations, handlers, or WebApi endpoints.

**Files expected to create/modify:**

- Create: `backend/src/NursingPlatform.Domain/Recruitment/ContactRequestStatus.cs`
- Create: `backend/src/NursingPlatform.Domain/Recruitment/ContactRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/DTOs/ContactRequestDto.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/DTOs/ReceivedContactRequestDto.cs`
- Create: `backend/src/NursingPlatform.Application/Recruitment/Common/ContactRequestMapping.cs`
- Create: command/query contract and validator files listed in Planned File Structure.
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/Recruitment/ContactRequestTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/ContactRequestValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/ContactRequestDtoSecurityTests.cs`

**Tests to write first:**

- `ContactRequest_DefaultStatus_IsPending`
- `ContactRequest_CapturesSafeSnapshotsWithoutContactInfo`
- `ContactRequest_TerminalStatusHelpers_IdentifyApprovedRejectedAndCancelled`
- `Validate_CreateContactRequest_WithEmptyNurseProfileId_ShouldHaveError`
- `Validate_ListMyContactRequests_WithInvalidPagination_ShouldHaveError`
- `Validate_ListReceivedContactRequests_WithInvalidStatus_ShouldHaveError`
- `Validate_TransitionCommands_WithEmptyId_ShouldHaveError`
- `ContactRequestDto_ShouldNotExposeInternalOrSensitiveFields`
- `ReceivedContactRequestDto_ShouldNotExposeInternalOrSensitiveFields`

**Required implementation notes:**

- `ContactRequestStatus` enum values must be exactly `Pending`, `Approved`, `Rejected`, `Cancelled`.
- `ContactRequest` inherits `AuditableEntity` and has:
  - `Guid Id`
  - `Guid EmployerProfileId`
  - `Guid EmployerOrganizationId`
  - `Guid NurseProfileId`
  - `ContactRequestStatus Status`
  - `string? CandidateHeadlineSnapshot`
  - `string? CandidateLicenseCountryNameSnapshot`
  - `string? CandidateCurrentCountryNameSnapshot`
  - `string EmployerOrganizationNameSnapshot`
  - `string? JobTitleSnapshot`
  - `string? DepartmentSnapshot`
  - `DateTime? RespondedAt`
  - `DateTime? CancelledAt`
  - navigation properties only for EF: `EmployerProfile`, `EmployerOrganization`, `NurseProfile`
- Do not add message, rejection reason, nurse email, phone, contact fields, contact-info DTOs, or concurrency-token DTO fields.
- Add `DbSet<ContactRequest> ContactRequests { get; }` to `IApplicationDbContext`.
- `ContactRequestDto` includes only:
  - `Id`
  - `NurseProfileId`
  - `Status`
  - `CandidateHeadline`
  - `CandidateLicenseCountryName`
  - `CandidateCurrentCountryName`
  - `CreatedAt`
  - `UpdatedAt`
  - `RespondedAt`
  - `CancelledAt`
- `ReceivedContactRequestDto` includes only:
  - `Id`
  - `OrganizationName`
  - `JobTitle`
  - `Department`
  - `Status`
  - `CreatedAt`
  - `UpdatedAt`
  - `RespondedAt`
  - `CancelledAt`
- DTO tests must assert no public properties named `UserId`, `EmployerProfileId`, `EmployerOrganizationId`, `Email`, `Phone`, `PasswordHash`, `Roles`, `Permissions`, `AccessToken`, `RefreshToken`, `TokenHash`, `CvStorageKey`, `CvFileUrl`, `FileUrl`, `InternalPath`, `LicenseNumber`, `User`, `NurseProfile`, `EmployerProfile`, `EmployerOrganization`, `Message`, `RejectionReason`, `RowVersion`, or `ConcurrencyToken`.
- Validators:
  - reject `Guid.Empty` for create `NurseProfileId`.
  - reject `Guid.Empty` for route id commands/queries.
  - enforce list pagination `Page >= 1`, `PageSize >= 1`, `PageSize <= 100`.
  - enforce optional status is one of `Pending`, `Approved`, `Rejected`, `Cancelled`; prefer `ContactRequestStatus? Status` on query contracts so invalid WebApi strings fail binding or validation deterministically.
- Keep mapping helpers internal to Application and project explicit DTOs.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "ContactRequest"
```

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ContactRequestDto|ContactRequestValidator|CreateContactRequest|ListMyContactRequests|ListReceivedContactRequests|ApproveReceivedContactRequest|RejectReceivedContactRequest|CancelContactRequest|GetMyContactRequest"
```

```bash
dotnet build backend/NursingPlatform.slnx
```

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after domain/contract tests and build pass. Do not create EF configuration, migration, Application handlers, or WebApi endpoints in Task 1.

---

### Task 2: Infrastructure Persistence, EF Configuration, Migration, and Infrastructure Tests

**Goal:** Persist Contact Requests with EF Core, configure relationships/indexes/string status storage, generate the migration, and verify EF model consistency.

**Files expected to create/modify:**

- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ContactRequestConfiguration.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: generated migration `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/*_AddContactRequests.cs`
- Create: generated migration designer `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/*_AddContactRequests.Designer.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- Test: `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ContactRequestConfigurationTests.cs`

**Tests to write first:**

- `ContactRequestConfiguration_UsesExpectedTableNameAndPrimaryKey`
- `ContactRequestConfiguration_StoresStatusAsStringWithMaxLength`
- `ContactRequestConfiguration_ConfiguresSnapshotMaxLengths`
- `ContactRequestConfiguration_ConfiguresRestrictDeleteRelationships`
- `ContactRequestConfiguration_ConfiguresEmployerAndNurseListIndexes`
- `ContactRequestConfiguration_ConfiguresActiveDuplicateFilteredUniqueIndex`

**Required implementation notes:**

- Add `DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();` to `ApplicationDbContext`.
- Configure `ContactRequest` table as `ContactRequests`.
- Required properties and max lengths:
  - `Status`: required, string conversion, max length 32.
  - `CandidateHeadlineSnapshot`: max length 160.
  - `CandidateLicenseCountryNameSnapshot`: max length should match `Country.Name` practical storage; use max 200 unless existing Country config differs.
  - `CandidateCurrentCountryNameSnapshot`: same as license country snapshot.
  - `EmployerOrganizationNameSnapshot`: required, max length 200.
  - `JobTitleSnapshot`: max length 160.
  - `DepartmentSnapshot`: max length 160.
- Relationships:
  - `EmployerProfileId` required FK to `EmployerProfile`, delete behavior Restrict.
  - `EmployerOrganizationId` required FK to `EmployerOrganization`, delete behavior Restrict.
  - `NurseProfileId` required FK to `NurseProfile`, delete behavior Restrict.
- Indexes:
  - non-unique `EmployerProfileId, CreatedAt, Id`.
  - non-unique `NurseProfileId, CreatedAt, Id`.
  - filtered unique `EmployerProfileId, NurseProfileId` where `Status IN ('Pending', 'Approved')`.
  - status-leading index only if EF SQL/query evidence during implementation justifies it; do not add by default.
- Migration name: `AddContactRequests`.
- Generate migration using EF only. Do not manually edit migration output except for a reviewed provider-specific filtered-index expression if EF generation requires it.
- No `RowVersion` or provider-specific concurrency column is required by Phase 6D.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "ContactRequestConfiguration"
```

```bash
dotnet build backend/NursingPlatform.slnx
```

```bash
dotnet ef migrations add AddContactRequests --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
```

```bash
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
```

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after Infrastructure tests, build, migration generation, and EF pending-model verification. Do not implement Application behavior or WebApi endpoints in Task 2.

---

### Task 3: Application Behavior and Application Tests

**Goal:** Implement employer create/list/get/cancel and nurse received-list/approve/reject behavior with role guards, prerequisites, eligibility, ownership hiding, duplicate rules, immutable terminal statuses, atomic conditional transitions, safe snapshots, and Application tests.

**Files expected to create/modify:**

- Modify/create handler files listed in Task 1.
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` only if Task 3 needs a small transition helper surface; prefer EF Core `ExecuteUpdateAsync` in handlers if testable.
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/CreateContactRequestCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/ListMyContactRequestsQueryHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/GetMyContactRequestQueryHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/CancelContactRequestCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/ListReceivedContactRequestsQueryHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/ApproveReceivedContactRequestCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Recruitment/ContactRequests/RejectReceivedContactRequestCommandHandlerTests.cs`

**Tests to write first:**

- `Handle_Create_WithEligibleEmployerAndCandidate_CreatesPendingRequestWithSnapshots`
- `Handle_Create_WhenEmployerProfileMissing_ThrowsForbiddenAccessException`
- `Handle_Create_WhenEmployerOrganizationMissing_ThrowsForbiddenAccessException`
- `Handle_Create_WhenTargetNurseProfileMissingOrIneligible_ThrowsKeyNotFoundException`
- `Handle_Create_WithPendingDuplicate_ThrowsInvalidOperationException`
- `Handle_Create_WithApprovedDuplicate_ThrowsInvalidOperationException`
- `Handle_Create_WithRejectedHistory_CreatesNewPendingRequest`
- `Handle_Create_WithCancelledHistory_CreatesNewPendingRequest`
- `Handle_Create_DoesNotSetMessageOrContactFields`
- `Handle_ListMy_ReturnsOnlyCurrentEmployerRequestsWithPaginationAndStatusFilter`
- `Handle_GetMy_WhenOwned_ReturnsSafeDto`
- `Handle_GetMy_WhenNotOwned_ThrowsKeyNotFoundException`
- `Handle_Cancel_WhenOwnedPending_AtomicallyCancelsAndReturnsDto`
- `Handle_Cancel_WhenTerminal_ThrowsInvalidOperationException`
- `Handle_Cancel_WhenCompetingTransitionWins_ThrowsInvalidOperationException`
- `Handle_ListReceived_ReturnsOnlyCurrentNurseRequestsWithPaginationAndStatusFilter`
- `Handle_Approve_WhenOwnedPending_AtomicallyApprovesAndReturnsDto`
- `Handle_Approve_WhenTerminal_ThrowsInvalidOperationException`
- `Handle_Approve_WhenCompetingTransitionWins_ThrowsInvalidOperationException`
- `Handle_Reject_WhenOwnedPending_AtomicallyRejectsAndReturnsDto`
- `Handle_Reject_WhenTerminal_ThrowsInvalidOperationException`
- `Handle_Reject_WhenCompetingTransitionWins_ThrowsInvalidOperationException`
- `Handlers_DoNotExposeEmailPhoneUserIdOrInternalFksInDtos`

**Required implementation notes:**

- Reuse `EmployerRoleGuard.EnsureEmployerAsync()` for employer workflows.
- Reuse `NurseRoleGuard.EnsureCurrentUserIsNurseAsync()` for nurse workflows.
- Employer create flow:
  1. enforce Employer role.
  2. load employer profile for current user.
  3. load employer organization for that profile.
  4. load eligible nurse candidate by `NurseProfileId` with `IsAvailableForRecruitment`, active user, and verified email.
  5. project safe candidate snapshots and country names.
  6. reject active duplicate where same `EmployerProfileId`, same `NurseProfileId`, and status is Pending or Approved.
  7. create `Pending` `ContactRequest`.
  8. save and return `ContactRequestDto`.
- Missing employer profile/organization: throw `ForbiddenAccessException`.
- Missing/ineligible candidate: throw `KeyNotFoundException` to hide unavailable/inactive/unverified/nonexistent nurse profiles.
- Duplicate Pending or Approved: throw `InvalidOperationException` so existing middleware maps to `409 Conflict`.
- Rejected/Cancelled history must not block new Pending request.
- Employer list/get/cancel scope by employer profile id; non-owned id returns `KeyNotFoundException`.
- Nurse list/approve/reject scope by nurse profile id; non-owned id returns `KeyNotFoundException`.
- Missing nurse profile for nurse workflows: throw `ForbiddenAccessException`.
- Terminal status mutation attempts: throw `InvalidOperationException`.
- Atomic transition recommendation:
  - Prefer `ExecuteUpdateAsync` on `_context.ContactRequests.Where(r => r.Id == request.Id && r.EmployerProfileId == employerProfileId && r.Status == ContactRequestStatus.Pending)` for cancel.
  - Prefer `ExecuteUpdateAsync` on `_context.ContactRequests.Where(r => r.Id == request.Id && r.NurseProfileId == nurseProfileId && r.Status == ContactRequestStatus.Pending)` for approve/reject.
  - Set `Status`, `CancelledAt` or `RespondedAt`, and `UpdatedAt` in the update.
  - If affected rows is `0`, perform a scoped lookup by id+owner to distinguish not found from conflict; return `KeyNotFoundException` for not owned/not found and `InvalidOperationException` for non-pending or competing transition.
  - After a successful update, re-query and project the updated DTO.
- If `ExecuteUpdateAsync` cannot be unit-tested with current mock patterns, add focused Application tests around decision logic and use an Infrastructure-backed test for affected-row behavior; do not weaken the atomic behavior.
- List pagination must prove both `Skip` and `Take` by requesting page 2 in at least one employer-list or nurse-list test.
- Default list ordering: `CreatedAt` descending, `Id` ascending.
- Raw DTO security remains enforced by DTO reflection tests and WebApi raw JSON tests.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ContactRequest"
```

```bash
dotnet build backend/NursingPlatform.slnx
```

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after Application tests and build pass. Do not add WebApi endpoints or tracking updates in Task 3.

---

### Task 4: WebApi Endpoints and Integration Tests

**Goal:** Add only the seven approved Phase 6D endpoints and prove auth, validation, conflict, ownership, route binding, and raw JSON security behavior through WebApi integration tests.

**Files expected to create/modify:**

- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Test: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ContactRequestEndpointsTests.cs`

**Endpoints to add:**

- `POST /api/v1/recruitment/contact-requests` with `.RequireAuthorization()` and `.WithName("CreateRecruitmentContactRequest")`.
- `GET /api/v1/recruitment/contact-requests` with `.RequireAuthorization()` and `.WithName("ListMyRecruitmentContactRequests")`.
- `GET /api/v1/recruitment/contact-requests/{id:guid}` with `.RequireAuthorization()` and `.WithName("GetMyRecruitmentContactRequest")`.
- `POST /api/v1/recruitment/contact-requests/{id:guid}/cancel` with `.RequireAuthorization()` and `.WithName("CancelRecruitmentContactRequest")`.
- `GET /api/v1/me/nurse-profile/contact-requests` with `.RequireAuthorization()` and `.WithName("ListReceivedContactRequests")`.
- `POST /api/v1/me/nurse-profile/contact-requests/{id:guid}/approve` with `.RequireAuthorization()` and `.WithName("ApproveReceivedContactRequest")`.
- `POST /api/v1/me/nurse-profile/contact-requests/{id:guid}/reject` with `.RequireAuthorization()` and `.WithName("RejectReceivedContactRequest")`.

**Tests to write first:**

- `CreateContactRequest_WithoutJwt_ReturnsUnauthorized`
- `ListMyContactRequests_WithoutJwt_ReturnsUnauthorized`
- `GetMyContactRequest_WithoutJwt_ReturnsUnauthorized`
- `CancelContactRequest_WithoutJwt_ReturnsUnauthorized`
- `ListReceivedContactRequests_WithoutJwt_ReturnsUnauthorized`
- `ApproveReceivedContactRequest_WithoutJwt_ReturnsUnauthorized`
- `RejectReceivedContactRequest_WithoutJwt_ReturnsUnauthorized`
- `CreateContactRequest_WithInvalidBody_ReturnsValidationProblemDetails`
- `ContactRequestRoute_WithInvalidGuid_ReturnsBadRequest`
- `CreateContactRequest_WithForbiddenPrerequisite_ReturnsForbidden`
- `GetMyContactRequest_WhenHidden_ReturnsNotFound`
- `CreateContactRequest_WithDuplicate_ReturnsConflict`
- `ApproveReceivedContactRequest_WithInvalidTransition_ReturnsConflict`
- `CreateContactRequest_WithValidRequest_ReturnsCreatedAndSafeJson`
- `ListMyContactRequests_ReturnsPaginatedSafeJson`
- `ListReceivedContactRequests_ReturnsPaginatedSafeJson`
- `ContactRequestEndpoints_UseRequireAuthorizationOnly_WithoutPermissionSetup`

**Required implementation notes:**

- Keep endpoint handlers thin and delegate to MediatR.
- For create endpoint, bind `CreateContactRequestRequest`, send `CreateContactRequestCommand`, and return `Results.Created($"/api/v1/recruitment/contact-requests/{result.Id}", result)`.
- For list endpoints, bind only `int? page`, `int? pageSize`, and `ContactRequestStatus? status` or string status converted through Application validation if enum binding is unreliable.
- For get/cancel/approve/reject, bind route `Guid id` and send the matching command/query.
- Do not add a contact-info endpoint.
- Do not add candidate listing changes, candidate filters, sorting parameters, frontend, notifications, messaging, payments, or Phase 7 endpoints.
- Do not use `.AllowAnonymous()` or `.RequirePermission(...)`.
- WebApi tests must reset permission mock and prove no permission setup is required.
- Raw JSON tests must assert absence of forbidden names/values before deserializing:
  - `userId`, `employerProfileId`, `employerOrganizationId`, `email`, `phone`, `passwordHash`, `roles`, `permissions`, `accessToken`, `refreshToken`, `tokenHash`, `licenseNumber`, `cvStorageKey`, `cvFileUrl`, `storageKey`, `internalPath`, `fileUrl`, `firstName`, `lastName`, `user`, `nurseProfile`, `employerProfile`, `employerOrganization`, `message`, `rejectionReason`, `rowVersion`, `concurrencyToken`.
- Tests should mock `ISender.Send` outcomes using `ForbiddenAccessException`, `KeyNotFoundException`, `InvalidOperationException`, and `ValidationException` to verify mapped status codes.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ContactRequest|Recruitment"
```

```bash
dotnet build backend/NursingPlatform.slnx
```

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after focused WebApi tests and build pass. Do not proceed to final verification or tracking updates until approved.

---

### Task 5: Final Verification and Documentation/Index Review

**Goal:** Verify Phase 6D end-to-end, review docs/indexing implications, and confirm no out-of-scope behavior or unsafe exposure was introduced.

**Files expected to create/modify:**

- Modify docs only if implementation changed behavior, architecture, API contracts, migration details, or indexing beyond the approved spec/plan and reviewer approves.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` in Task 5.

**Tests to write first:**

- No new tests expected unless final verification exposes a missing Phase 6D requirement or regression gap.

**Required implementation notes:**

- Run the full backend build.
- Run the full solution test suite.
- Run EF pending model check.
- If EF reports pending model changes after migration, stop and report before changing anything.
- Review `docs/api/api-design.md` for endpoint/Problem Details consistency.
- Review `docs/database/database-design.md` and `ContactRequestConfiguration` for index scope.
- Confirm no response DTO exposes `EmployerProfileId`, `EmployerOrganizationId`, `UserId`, email, phone, legal names, role/permission internals, tokens, password hashes, CV data, contact info, messages, rejection reasons, or concurrency tokens.
- Confirm raw JSON WebApi tests inspect response text before DTO deserialization.
- Confirm `GET /api/v1/recruitment/candidates` was not modified for Phase 6D.
- Confirm no contact-info endpoint, frontend, notifications, messaging/chat, payments, admin approval, Phase 7, candidate sorting, or candidate filtering work was added.

**Verification commands:**

```bash
dotnet build backend/NursingPlatform.slnx
```

```bash
dotnet test backend/NursingPlatform.slnx
```

```bash
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
```

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --stat
```

```bash
git diff --stat
```

**Stop condition:** Stop for review after final verification and documentation/index review evidence is reported. Do not update tracking docs, stage, or commit.

---

### Task 6: Tracking Documentation Update

**Goal:** Update project tracking only after Phase 6D implementation and final verification are approved.

**Files expected to create/modify:**

- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

**Tests to write first:**

- No automated tests required for tracking-only edits.

**Required implementation notes:**

- Update `CURRENT_TASK.md` to Phase 6D Contact Requests completion status only after Task 5 approval.
- Update `TASKS.md` to mark only `Contact requests` complete.
- Do not mark Phase 7 work complete.
- Do not add Phase 7 implementation details.
- Do not start frontend, notifications, messaging, payments, or contact-info endpoint work.
- Do not stage or commit.

**Verification commands:**

```bash
git diff -- CURRENT_TASK.md TASKS.md
```

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --stat
```

**Stop condition:** Stop for review after tracking diff and git status are reported. Do not stage or commit.

---

### Task 7: Final Phase 6D Commit

**Goal:** Commit approved Phase 6D spec, plan, implementation, tests, migration, and tracking changes only after explicit reviewer approval.

**Files expected to create/modify:**

- Stage only explicitly approved Phase 6D files.
- Include approved spec: `docs/superpowers/specs/2026-07-11-contact-requests.md`.
- Include approved plan: `docs/superpowers/plans/2026-07-11-contact-requests.md`.
- Include approved Phase 6D implementation, tests, migration, and tracking files.
- Do not stage `codex_res/codex_report.md`.

**Tests to write first:**

- No new tests in commit-only task.

**Required implementation notes:**

- Before staging, inspect `git status --short --untracked-files=all`, `git diff --stat`, and `git log --oneline -10`.
- Use explicit `git add <path>` commands only.
- Never use `git add .`.
- Do not stage `AGENTS.md`, `PROJECT_RULES.md`, `.gitignore`, report files, unrelated docs, personal files, frontend files, Phase 7 files, generated artifacts outside approved migration files, or unapproved migrations.
- Before committing, run `git diff --cached --name-only` and `git diff --cached --stat`.
- Verify the staged list contains only approved Phase 6D files.
- Commit message should be concise and scoped, for example `feat: add recruitment contact requests`.

**Verification commands:**

```bash
git status --short --untracked-files=all
```

```bash
git diff --cached --name-only
```

```bash
git diff --cached --stat
```

```bash
git commit -m "feat: add recruitment contact requests"
```

```bash
git status --short --untracked-files=all
```

```bash
git log -1 --oneline
```

**Stop condition:** Stop after reporting staged file list, staged diff stat, commit hash/message, post-commit status, and latest log line. Do not proceed to Phase 7, frontend, contact-info endpoint, notifications, messaging, or payments.

---

## Self-Review Checklist

- Spec coverage: Task 1 covers domain/DTO/contracts/validators; Task 2 covers persistence/configuration/migration/indexes; Task 3 covers all Application lifecycle behavior; Task 4 covers all WebApi endpoints and integration/security tests; Task 5 covers full verification/docs/index review; Task 6 covers tracking; Task 7 covers final commit only.
- Scope check: The plan excludes contact-info endpoint, candidate listing changes, candidate filtering/sorting changes, frontend, notifications, messaging/chat, payments, admin approval, Phase 7, and broad PII exposure.
- Atomic transition decision: The plan recommends atomic conditional update with affected-row verification because it is provider-compatible with EF/PostgreSQL and avoids a required `RowVersion` schema token.
- DTO exposure check: Employer/nurse DTOs do not expose `EmployerProfileId`, `EmployerOrganizationId`, `UserId`, email, phone, legal names, messages, rejection reasons, contact fields, or concurrency tokens.
- Type consistency: The plan consistently uses `ContactRequest`, `ContactRequestStatus`, `ContactRequestDto`, `ReceivedContactRequestDto`, `CreateContactRequestCommand`, `ListMyContactRequestsQuery`, `GetMyContactRequestQuery`, `CancelContactRequestCommand`, `ListReceivedContactRequestsQuery`, `ApproveReceivedContactRequestCommand`, and `RejectReceivedContactRequestCommand`.
- Placeholder scan: No unresolved placeholders are intentionally left in this plan.
