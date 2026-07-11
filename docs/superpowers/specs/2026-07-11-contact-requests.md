# Phase 6D - Contact Requests Specification

## Objective

Define the Phase 6D Contact Requests workflow for Recruitment. Employers who meet the Phase 6 employer prerequisites may request recruitment contact with a visible nurse candidate, and nurses may approve or reject received requests. Phase 6D is security-first: candidate listing remains contact-free, and employer access to nurse contact information must never happen before nurse approval.

Phase 6D establishes request ownership, lifecycle, duplicate prevention, API contracts, data model expectations, validation, error behavior, and tests. This specification does not implement code, create migrations, update tracking docs, add frontend work, or start Phase 7.

## Current Baseline

Phase 6A provides:

- `EmployerProfile` owned by a single authenticated Employer user.
- `EmployerOrganization` linked one-to-one to an employer profile.
- Application-layer `EmployerRoleGuard` enforcing active authenticated users with the `Employer` role.
- Employer prerequisite behavior used by recruitment candidate search.

Phase 6B and Phase 6C provide:

- `GET /api/v1/recruitment/candidates`.
- `.RequireAuthorization()` plus Application-layer Employer role enforcement.
- Employer profile and organization prerequisites before candidate data is queried.
- Candidate eligibility limited to `NurseProfile.IsAvailableForRecruitment == true`, active nurse account, and verified nurse email.
- Safe candidate summary DTOs with no contact info, account internals, CV access, roles, permissions, tokens, or EF navigation objects.
- Pagination, deterministic sorting, approved filters, and raw JSON security tests.

Current domain shape:

- `NurseProfile` contains recruitment profile metadata, license country/current country ids, years of experience, and `IsAvailableForRecruitment`.
- `User` contains `Email`, legal name fields, `PasswordHash`, activity state, and email verification state.
- No nurse phone number or nurse-controlled recruitment contact field currently exists beyond `User.Email`.
- `User.Email` is account data and must not be exposed through candidate listing or contact requests without an explicit reviewer-approved contact-info design.

## In Scope

- Contact request workflow under Recruitment.
- Employer-created contact requests for a specific `nurseProfileId`.
- Employer listing and reading of only their own contact requests.
- Employer cancellation of only their own pending contact requests.
- Nurse listing of contact requests received for the authenticated nurse profile.
- Nurse approval or rejection of only their own pending received requests.
- Lifecycle status model: `Pending`, `Approved`, `Rejected`, `Cancelled`.
- Duplicate prevention for active employer+nurse request relationships.
- Application tests for lifecycle, ownership, prerequisites, duplicates, eligibility, validation, and no pre-approval contact exposure.
- WebApi integration tests for auth, validation, conflict, ownership hiding, route binding, and raw JSON security.
- EF model and indexing proposal for later implementation.
- Snapshotting of approved safe display fields at request creation so later profile/organization edits do not rewrite request history.
- Provider-neutral atomic transition behavior for approve, reject, and cancel.

## Out of Scope

- Frontend.
- Notifications.
- Chat, messaging, or threaded communication.
- Payments, subscriptions, or billing gates.
- Admin approval workflows.
- Candidate sorting changes.
- Candidate filtering changes.
- Candidate detail endpoints.
- CV download or CV access.
- Contact-info endpoint or contact-info response payload in Phase 6D.
- Broad PII exposure.
- Phase 7 exams or later-phase work.

## Actors, Permissions, and Ownership

### Employer Actor

An Employer is an authenticated user who:

- Has an active account.
- Has the `Employer` role.
- Owns an `EmployerProfile`.
- Has an `EmployerOrganization` linked to that employer profile.

Employer rules:

- May create a contact request for an eligible recruitment-visible nurse candidate.
- May list only requests where `EmployerProfileId` belongs to the authenticated employer.
- May get only one owned request by id.
- May cancel only one owned request while it is `Pending`.
- May not approve or reject requests.
- May not view another employer's requests.
- May not request contact with a nurse when employer profile or organization prerequisites are missing.

### Nurse Actor

A Nurse is an authenticated user who:

- Has an active account.
- Has the `Nurse` role.
- Owns a `NurseProfile`.

Nurse rules:

- May list only contact requests where `NurseProfileId` belongs to the authenticated nurse.
- May approve or reject only own received requests while they are `Pending`.
- May not create employer contact requests.
- May not cancel employer-created requests.
- May not view requests for another nurse profile.

### Candidate Eligibility

An employer may create a contact request only for a nurse profile that is currently eligible for recruitment search:

- `NurseProfile.IsAvailableForRecruitment == true`.
- Linked nurse `User.IsActive == true`.
- Linked nurse `User.EmailVerified == true`.

Ineligible nurses must behave as not found for employer create attempts so that employers cannot infer hidden, inactive, or unverified nurse profiles.

## Contact Request Lifecycle

Statuses:

- `Pending`: created by an eligible employer and awaiting nurse decision.
- `Approved`: nurse approved a pending request.
- `Rejected`: nurse rejected a pending request.
- `Cancelled`: employer cancelled a pending request before nurse response.

Allowed transitions:

| Actor | From | To | Notes |
|------|------|----|-------|
| Employer | none | Pending | Creates a new request when no active duplicate exists. |
| Nurse | Pending | Approved | Records nurse approval and `RespondedAt`. |
| Nurse | Pending | Rejected | Records nurse rejection and `RespondedAt`. |
| Employer | Pending | Cancelled | Records employer cancellation and `CancelledAt`. |

Terminal statuses:

- `Approved`, `Rejected`, and `Cancelled` are terminal in Phase 6D.
- No mutation after terminal statuses is allowed.
- Invalid transitions must return `409 Conflict`.
- Transition handlers must be atomic. If approve, reject, or cancel races with another transition, exactly one transition may succeed and every losing transition must return `409 Conflict`.
- The implementation plan must choose and justify a provider-compatible technical approach for atomic transitions: either a concurrency token compatible with current EF/PostgreSQL conventions, or an atomic conditional update scoped by `Id`, owner, and `Status == Pending` with affected-row verification.
- The implementation plan must not add reopen, reapprove, unreject, uncancel, expire, archive, or admin override behavior without reviewer approval.

## Duplicate Request Rules

Duplicate scope is the pair:

- `EmployerProfileId`
- `NurseProfileId`

Phase 6D must not allow duplicate active request relationships for the same employer profile and nurse profile.

Rules:

- Same employer requests the same nurse while a request is `Pending`: return `409 Conflict` and do not create a duplicate.
- Same employer requests the same nurse after a request is `Approved`: return `409 Conflict`; the approved relationship already exists.
- Same employer requests the same nurse after a request is `Rejected`: allow a new `Pending` request, preserving the rejected history.
- Same employer requests the same nurse after a request is `Cancelled`: allow a new `Pending` request, preserving the cancelled history.

Justification:

- `409 Conflict` for `Pending` and `Approved` is deterministic and avoids ambiguous POST idempotency.
- Allowing a later request after `Rejected` or `Cancelled` supports real recruitment workflows while keeping terminal history immutable.
- Database design should enforce active duplicate prevention with a unique filtered index for `Pending` and `Approved` if PostgreSQL/EF support is used during implementation.
- Abuse controls such as per-employer pending request limits, nurse-level rate limits, or anti-spam throttling are not part of Phase 6D and require reviewer approval before implementation.

## Privacy and Contact Info Decision

Phase 6D includes only the request workflow and status visibility. It must not include a post-approval contact-info endpoint.

Security baseline:

- Candidate listing must remain unchanged and must not expose contact info.
- Contact request list/detail responses must not expose nurse email, nurse phone, legal name, CV URLs, storage keys, account internals, roles, permissions, tokens, or password hashes.
- Phase 6D does not allow employer-authored messages, nurse-authored rejection reasons, or other cross-actor free-text fields because those fields could carry contact details that tests cannot reliably prove absent.
- Approval records consent state only; approval does not automatically return contact details in Phase 6D.
- Employer access to contact information must never happen before nurse approval.

Current data-model constraint:

- The current domain does not contain a nurse-controlled recruitment email, phone number, or contact preference field.
- The only obvious contact value is `User.Email`, which is account identity data.
- Phase 6D must not expose `User.Email` as contact info without explicit reviewer approval.

Reviewer decision needed for a future phase:

- Decide whether post-approval contact access should expose `User.Email`, require new nurse-controlled recruitment contact fields, or use a separate messaging/notification flow.
- If a future post-approval contact-info endpoint is approved, it must be separate from candidate listing, require Employer role, require employer ownership of an `Approved` request, return only explicitly approved contact fields, avoid user/account internals, and include forbidden pre-approval tests.

## Proposed API Contract

All Phase 6D endpoints are under `/api/v1` and require `.RequireAuthorization()`. Application handlers enforce role, prerequisites, ownership, and lifecycle rules.

### Employer Endpoints

| Method | Route | Name | Request DTO | Response DTO | Purpose |
|-------|-------|------|-------------|--------------|---------|
| POST | `/recruitment/contact-requests` | `CreateRecruitmentContactRequest` | `CreateContactRequestRequest` | `ContactRequestDto` | Employer requests contact with a nurse; returns `201 Created`. |
| GET | `/recruitment/contact-requests` | `ListMyRecruitmentContactRequests` | query params | `PaginatedResult<ContactRequestDto>` | Employer lists own sent requests. |
| GET | `/recruitment/contact-requests/{id:guid}` | `GetMyRecruitmentContactRequest` | route id | `ContactRequestDto` | Employer gets one own sent request. |
| POST | `/recruitment/contact-requests/{id:guid}/cancel` | `CancelRecruitmentContactRequest` | none | `ContactRequestDto` | Employer cancels own pending request. |

### Nurse Endpoints

| Method | Route | Name | Request DTO | Response DTO | Purpose |
|-------|-------|------|-------------|--------------|---------|
| GET | `/me/nurse-profile/contact-requests` | `ListReceivedContactRequests` | query params | `PaginatedResult<ReceivedContactRequestDto>` | Nurse lists received requests. |
| POST | `/me/nurse-profile/contact-requests/{id:guid}/approve` | `ApproveReceivedContactRequest` | none | `ReceivedContactRequestDto` | Nurse approves own pending request. |
| POST | `/me/nurse-profile/contact-requests/{id:guid}/reject` | `RejectReceivedContactRequest` | none | `ReceivedContactRequestDto` | Nurse rejects own pending request. |

Route design notes:

- Employer endpoints live under Recruitment because requests start from candidate search.
- Nurse endpoints live under `/me/nurse-profile` because the nurse is managing received requests for their own profile.
- Approve, reject, and cancel use subresource action routes to make state transitions explicit.
- Do not add sorting parameters in Phase 6D unless a later approved plan requires them.

### Query Parameters

List endpoints support only:

- `page`: optional integer, default `1`, minimum `1`.
- `pageSize`: optional integer, default `20`, minimum `1`, maximum `100`.
- `status`: optional status filter restricted to `Pending`, `Approved`, `Rejected`, `Cancelled`.

Default ordering:

1. `CreatedAt` descending.
2. `Id` ascending as a stable tie-breaker.

## DTO and Request Contracts

### CreateContactRequestRequest

| Property | Type | Rules |
|----------|------|-------|
| NurseProfileId | Guid | Required. Must identify an eligible recruitment-visible candidate. |

### ContactRequestDto

Employer-facing response:

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Contact request id. |
| NurseProfileId | Guid | Candidate nurse profile id. |
| Status | string | `Pending`, `Approved`, `Rejected`, or `Cancelled`. |
| CandidateHeadline | string? | Safe candidate summary snapshot captured at request creation. |
| CandidateLicenseCountryName | string? | Safe candidate summary snapshot captured at request creation. |
| CandidateCurrentCountryName | string? | Safe candidate summary snapshot captured at request creation. |
| CreatedAt | DateTime | UTC. |
| UpdatedAt | DateTime | UTC audit value. |
| RespondedAt | DateTime? | UTC when approved/rejected. |
| CancelledAt | DateTime? | UTC when cancelled. |

### ReceivedContactRequestDto

Nurse-facing response:

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Contact request id. |
| OrganizationName | string | Safe organization name snapshot captured at request creation. |
| JobTitle | string? | Safe `EmployerProfile.JobTitle` snapshot captured at request creation. |
| Department | string? | Safe `EmployerProfile.Department` snapshot captured at request creation. |
| Status | string | `Pending`, `Approved`, `Rejected`, or `Cancelled`. |
| CreatedAt | DateTime | UTC. |
| UpdatedAt | DateTime | UTC audit value. |
| RespondedAt | DateTime? | UTC when approved/rejected. |
| CancelledAt | DateTime? | UTC when cancelled. |

DTO security notes:

- Do not expose `UserId` in request DTOs or response DTOs.
- Do not expose nurse email or phone in Phase 6D responses.
- Do not expose employer user email or legal name unless explicitly approved by a later spec.
- Keep `EmployerProfileId`, `EmployerOrganizationId`, and `NurseProfileId` as entity/internal relationship fields. Only `NurseProfileId` is exposed on the employer-facing DTO because candidate listing already exposes it and the employer needs to understand the target candidate.
- Do not expose employer-authored messages, nurse-authored rejection reasons, or other cross-actor free text in Phase 6D.
- Do not expose concurrency tokens in API DTOs.
- Do not expose EF navigation objects, domain entities, or database entities.

## Data Model Proposal

Create a new Domain entity under Recruitment, likely `ContactRequest`, inheriting the existing `AuditableEntity` base.

Proposed fields:

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | Primary key. |
| EmployerProfileId | Guid | Required FK to `EmployerProfile`; restrict delete. |
| EmployerOrganizationId | Guid | Required FK to `EmployerOrganization`; captures organization at request time. |
| NurseProfileId | Guid | Required FK to `NurseProfile`; restrict delete. |
| Status | `ContactRequestStatus` enum persisted as string | Required, max 32: `Pending`, `Approved`, `Rejected`, `Cancelled`. |
| CandidateHeadlineSnapshot | string? | Safe candidate headline captured at request creation. |
| CandidateLicenseCountryNameSnapshot | string? | Safe candidate country name captured at request creation. |
| CandidateCurrentCountryNameSnapshot | string? | Safe candidate country name captured at request creation. |
| EmployerOrganizationNameSnapshot | string | Safe organization name captured at request creation. |
| JobTitleSnapshot | string? | Safe `EmployerProfile.JobTitle` captured at request creation. |
| DepartmentSnapshot | string? | Safe `EmployerProfile.Department` captured at request creation. |
| CreatedAt | DateTime | From `AuditableEntity`, UTC audit value. |
| CreatedBy | string? | From `AuditableEntity`. |
| UpdatedAt | DateTime | From `AuditableEntity`, UTC audit value. |
| UpdatedBy | string? | From `AuditableEntity`. |
| RespondedAt | DateTime? | Required when status is `Approved` or `Rejected`. |
| CancelledAt | DateTime? | Required when status is `Cancelled`. |

Relationship and index expectations:

- Table name should be `ContactRequests` unless an existing Recruitment naming pattern appears before implementation.
- Use Guid primary key and PascalCase columns.
- `EmployerProfileId`, `EmployerOrganizationId`, and `NurseProfileId` are required FKs.
- Delete behavior should be Restrict for employer profile, employer organization, and nurse profile to preserve request history.
- `Status` is represented in Domain as an enum and persisted as a string column with max length 32.
- Snapshot fields are persisted at creation and response DTOs read from snapshots, not live joins, for display history.
- No message, rejection reason, or other cross-actor free-text column is included in Phase 6D.
- Atomic approve/reject/cancel behavior is required, but the spec does not require a specific provider mechanism. Implementation may use a concurrency token if compatible with current EF/PostgreSQL conventions, or an atomic conditional update scoped by `Id`, owner, and `Status == Pending` with affected-row verification.
- Add indexes for:
  - `EmployerProfileId, CreatedAt, Id` for employer list.
  - `NurseProfileId, CreatedAt, Id` for nurse list.
  - `Status` only if query plans justify it.
- Add a unique filtered index on `EmployerProfileId, NurseProfileId` for active statuses `Pending` and `Approved` if supported during implementation.
- Do not create a migration during the specification phase.

## Application Behavior

Create request handler:

1. Enforce authenticated active Employer role.
2. Verify employer profile exists for current user.
3. Verify employer organization exists for that profile.
4. Verify target nurse profile is recruitment-visible, active, and email-verified.
5. Reject active duplicates according to duplicate rules.
6. Capture safe candidate and employer organization/profile snapshot fields.
7. Create `Pending` request.
8. Return safe `ContactRequestDto` with `201 Created`.

Employer list/get/cancel handlers:

- Enforce Employer role and employer prerequisites.
- Scope every query by authenticated employer profile id.
- Return `404 Not Found` when a request id does not belong to the employer.
- Cancel only `Pending` requests.
- Return `409 Conflict` for cancelling `Approved`, `Rejected`, or `Cancelled` requests.
- Use the implementation-plan-selected concurrency token or atomic conditional update approach so competing terminal transitions cannot both succeed.

Nurse list/approve/reject handlers:

- Enforce authenticated active Nurse role.
- Verify current nurse profile exists.
- Scope every query by authenticated nurse profile id.
- Return `404 Not Found` when a request id does not belong to the nurse.
- Approve/reject only `Pending` requests.
- Return `409 Conflict` for approving or rejecting `Approved`, `Rejected`, or `Cancelled` requests.
- Use the implementation-plan-selected concurrency token or atomic conditional update approach so competing terminal transitions cannot both succeed.

Projection requirements:

- Project explicitly to DTOs.
- Do not return entities.
- Do not materialize or serialize account internals.
- Include only safe candidate and organization summary fields listed in DTO contracts.

## Validation and Error Behavior

Authentication and authorization:

- Missing JWT returns `401 Unauthorized`.
- Authenticated wrong role returns `403 Forbidden`.
- Employer missing profile or organization returns `403 Forbidden`.
- Nurse missing nurse profile returns `403 Forbidden` for received-request workflows.

Ownership and hidden resources:

- If an employer reads/cancels a request not owned by their employer profile, return `404 Not Found`.
- If a nurse approves/rejects a request not received by their nurse profile, return `404 Not Found`.
- If an employer creates a request for an ineligible or nonexistent nurse profile, return `404 Not Found`.

Validation:

- Invalid GUID route/body values return `400 Bad Request`.
- `Guid.Empty` route ids return validation Problem Details with `400 Bad Request`.
- `NurseProfileId` is required for create and must not be `Guid.Empty`.
- Employer messages are not accepted in Phase 6D.
- Rejection reasons are not accepted in Phase 6D.
- `page >= 1`.
- `pageSize >= 1`.
- `pageSize <= 100`.
- `status`, when supplied, must be one of the approved lifecycle statuses.

Conflicts:

- Duplicate active `Pending` request returns `409 Conflict`.
- Duplicate active `Approved` relationship returns `409 Conflict`.
- Invalid state transitions return `409 Conflict`.
- Optimistic concurrency conflicts or losing competing terminal transitions return `409 Conflict`.
- Database unique-index conflicts for active duplicates must be translated to the same `409 Conflict` behavior if implementation uses a filtered unique index.

Problem Details:

- Use the existing WebApi Problem Details behavior.
- Do not expose stack traces or internal exception details.

## Security and DTO Requirements

Phase 6D must forbid exposing:

- Password hashes.
- Tokens, token hashes, access tokens, refresh tokens, reset tokens, verification tokens.
- Roles or permissions.
- `UserId` or account ownership internals.
- Nurse email before approval.
- Nurse phone before approval.
- Nurse email/phone after approval in Phase 6D because no contact-info endpoint is included.
- Employer user email or legal name unless separately approved.
- Employer profile ids and organization ids in API DTOs.
- Concurrency tokens in API DTOs.
- Employer-authored messages, nurse-authored rejection reasons, or other cross-actor free-text values.
- CV storage keys, CV URLs, file URLs, internal paths, or download links.
- License number.
- Email verification state or account active state.
- EF navigation objects.
- Domain entities or database entities directly.

Raw JSON WebApi tests must inspect response strings before DTO deserialization and assert forbidden field names and known sensitive values are absent.

## Testing Requirements

Application tests must cover:

- Create request success for an eligible employer and eligible nurse.
- Employer role guard rejects non-Employer users.
- Employer profile prerequisite failure.
- Employer organization prerequisite failure.
- Nurse candidate eligibility: unavailable, inactive, and unverified nurses cannot be requested.
- Duplicate pending behavior returns conflict and creates no duplicate.
- Duplicate approved behavior returns conflict.
- New request after rejected history is allowed.
- New request after cancelled history is allowed.
- Safe candidate, organization name, job title, and department snapshot fields are captured at creation and used for later responses.
- Employer lists only own requests.
- Employer gets only own request.
- Employer cancel success for own pending request.
- Nurse lists only own received requests.
- Nurse approve success for own pending request.
- Nurse reject success for own pending request.
- Invalid transition conflicts for terminal statuses.
- Simultaneous approve/reject/cancel attempts allow only one terminal transition and return conflict for losing transitions.
- Unauthorized ownership access returns not found.
- `Guid.Empty` validation for body and route ids.
- Employer messages and nurse rejection reasons are not accepted or returned.
- No pre-approval contact exposure in DTO projections.
- No Phase 6D post-approval contact-info response exists.

WebApi integration tests must cover:

- `401 Unauthorized` without JWT for every Phase 6D endpoint.
- `403 Forbidden` for authenticated wrong role or missing prerequisites.
- `400 Bad Request` validation Problem Details.
- `404 Not Found` for hidden ownership and ineligible/nonexistent nurse targets.
- `409 Conflict` duplicate and invalid transition behavior.
- Route binding for body ids, route ids, pagination, status filters, and transition endpoints.
- Raw JSON security assertions for employer-facing and nurse-facing responses.
- `.RequireAuthorization()` is present and no `.AllowAnonymous()` is used.
- No permission-service setup is required unless a later approved plan explicitly adds permissions.

Infrastructure tests must cover during implementation:

- EF configuration for required fields, max lengths, relationships, and delete behavior.
- Status enum string conversion with max length 32.
- Snapshot field persistence.
- Provider-compatible atomic transition approach: either compatible concurrency token behavior or atomic conditional update with affected-row verification.
- Indexes for employer list and nurse list.
- Active duplicate prevention index when supported by EF/PostgreSQL.
- Migration is created only during implementation, not during specification.

## Performance and Indexing Considerations

List endpoints must remain paginated and bounded.

Expected query paths:

- Employer list: filter by `EmployerProfileId`, optional `Status`, order by `CreatedAt desc`, `Id asc`.
- Nurse list: filter by `NurseProfileId`, optional `Status`, order by `CreatedAt desc`, `Id asc`.
- Duplicate check: filter by `EmployerProfileId`, `NurseProfileId`, and active statuses.
- Transition updates: filter by `Id`, owner profile id, and `Status == Pending`, with concurrency protection.

Recommended indexes for implementation planning:

- `IX_ContactRequests_EmployerProfileId_CreatedAt_Id`.
- `IX_ContactRequests_NurseProfileId_CreatedAt_Id`.
- Filtered unique index on `EmployerProfileId, NurseProfileId` where status is `Pending` or `Approved`.
- Status-leading indexes only if generated SQL and expected query volume justify them.

Do not add indexes or migrations in the spec phase.

## Acceptance Criteria

- A later Phase 6D implementation plan can be written from this spec without inventing lifecycle, ownership, duplicate, or privacy semantics.
- Candidate listing remains unchanged and contact-free.
- Contact request creation is limited to employers with profile and organization prerequisites.
- Contact request responses are scoped by ownership.
- Nurses control approval or rejection of received requests.
- No contact info is returned before approval.
- No contact info endpoint is introduced in Phase 6D.
- Duplicate active requests are prevented deterministically.
- Terminal statuses are immutable.
- Terminal transitions are atomic and competing transitions return conflict.
- Safe candidate and organization display fields are snapshots captured at creation.
- Validation, conflicts, ownership hiding, and auth behavior are testable through Application and WebApi tests.
- EF model changes are proposed but no migration is created in the spec phase.
- No frontend, notifications, messaging, payments, admin approval, candidate sorting/filtering changes, CV access, Phase 7, or broad PII exposure is included.

## Reviewer Decisions Needed

- Decide in a future spec whether approved requests should expose contact data through a separate endpoint, use new nurse-controlled recruitment contact fields, expose `User.Email`, or route communication through messaging/notifications.
- Decide in a future anti-abuse spec whether contact requests need rate limits, maximum pending counts, or moderation controls.
