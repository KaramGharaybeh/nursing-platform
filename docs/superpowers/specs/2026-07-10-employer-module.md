# Phase 6A — Employer Profile Foundation

## 1. Objective

Implement the Phase 6A Employer Profile Foundation for authenticated employer self-service.

This phase gives employer users a minimal, structured business profile they can manage themselves, including employer profile metadata and a single organization profile. The module must follow the existing Clean Architecture, CQRS, MediatR, FluentValidation, EF Core, and Minimal API patterns established in earlier phases.

Phase 6A does not expose nurse data to employers and does not implement candidate search, filtering, contact requests, recruitment approval, messaging, or administration. It creates the employer-owned data foundation required for later recruitment functionality.

## 2. Approved Scope

In scope:

- Employer profile self read/upsert for authenticated employer users.
- Organization profile/metadata self read/upsert for the authenticated employer-owned profile.
- Employer role guard using the existing `Employer` role name.
- Application tests for role enforcement, ownership, validation, reference-data checks, creation, and update behavior.
- WebApi integration tests for authentication, forbidden access, validation, successful read/upsert, and sensitive-field exposure behavior.
- EF Core migration during later implementation tasks, not during planning.

Endpoint scope:

| Area | Endpoint | Method | Auth |
|------|----------|--------|------|
| Employer profile | `/api/v1/me/employer-profile` | GET | `.RequireAuthorization()` |
| Employer profile | `/api/v1/me/employer-profile` | PUT | `.RequireAuthorization()` |
| Organization | `/api/v1/me/employer-profile/organization` | GET | `.RequireAuthorization()` |
| Organization | `/api/v1/me/employer-profile/organization` | PUT | `.RequireAuthorization()` |

## 3. Explicit Out of Scope

The following are explicitly out of scope for Phase 6A:

- Candidate search.
- Candidate filtering.
- Contact requests.
- Employer access to nurse profile details.
- Public nurse profile pages.
- Recruitment approval workflow.
- Messaging.
- Admin employer management.
- Multi-user organization membership.
- Organization invitations.
- Frontend implementation.
- Payments, exams, notifications, and Phase 7+ work.
- Employer profile approval or moderation workflow.
- Organization logo or document upload.
- Public employer profile pages.
- Permission-protected employer endpoints using `.RequirePermission(...)`.

## 4. Entity Model

All new entities belong in `NursingPlatform.Domain.Employers` and should use `Guid` primary keys. Aggregate-root and business child entities should inherit `AuditableEntity` unless there is a documented reason not to.

### EmployerProfile

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| UserId | Guid | Required FK to `User`; unique index; delete behavior Restrict |
| JobTitle | string? | Employer user's business title, max 160 |
| Department | string? | Department or team, max 160 |
| Organization | EmployerOrganization? | Optional one-to-one organization profile |
| User | User | Required navigation |

Notes:

- One user can have at most one `EmployerProfile` in Phase 6A.
- Profile personal identity fields such as first name, last name, and email remain on `User`; do not duplicate them in `EmployerProfile`.
- Avoid additional personal PII unless explicitly approved.
- Employer profile creation is self-service for authenticated users with the `Employer` role; no admin approval is required.

### EmployerOrganization

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| EmployerProfileId | Guid | Required FK to `EmployerProfile`; unique index; delete behavior Cascade |
| Name | string | Required, max 200 |
| Type | string? | Organization type, max 100 |
| WebsiteUrl | string? | Absolute HTTP/HTTPS URL when supplied, max 500 |
| CountryId | Guid? | Optional FK to existing `Country`; Restrict |
| City | string? | Max 120 |
| AddressLine1 | string? | Business address line, max 200 |
| AddressLine2 | string? | Business address line, max 200 |
| PostalCode | string? | Max 40 |
| Description | string? | Business metadata, max 2000 |
| Country | Country? | Optional navigation |
| EmployerProfile | EmployerProfile | Required navigation |

Notes:

- One employer profile can have at most one organization in Phase 6A.
- Organization data is business metadata only.
- Use existing `Country` reference data for organization country when supplied.
- Do not model multi-user organization membership, invitations, organization roles, or delegated access in Phase 6A.

## 5. API Endpoint Contracts

All endpoints are under `/api/v1` and require a valid JWT via `.RequireAuthorization()`.

Application handlers must enforce that the authenticated user has the `Employer` role. If an authenticated user does not have the `Employer` role, throw `ForbiddenAccessException` and return HTTP 403 Problem Details through the existing WebApi exception mapping.

### `GET /me/employer-profile`

Returns the authenticated employer user's profile. If the user has the `Employer` role but no profile exists yet, return 404. If the authenticated user does not have the `Employer` role, return 403 Problem Details.

Response 200: `EmployerProfileDto`.

### `PUT /me/employer-profile`

Creates or replaces the authenticated employer user's profile metadata. If the profile already exists, update the existing row instead of creating another row. If the authenticated user does not have the `Employer` role, return 403 Problem Details.

Response 200: `EmployerProfileDto`.

### `GET /me/employer-profile/organization`

Returns the organization profile for the authenticated employer user. If the employer profile or organization does not exist, return 404. If the authenticated user does not have the `Employer` role, return 403 Problem Details.

Response 200: `EmployerOrganizationDto`.

### `PUT /me/employer-profile/organization`

Creates or replaces the authenticated employer user's single organization profile. If an employer profile does not exist yet, create it before creating the organization. If the organization already exists, update the existing row instead of creating another row. If the authenticated user does not have the `Employer` role, return 403 Problem Details.

Response 200: `EmployerOrganizationDto`.

## 6. DTO, Request, and Response Models

### EmployerProfileDto

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Employer profile id |
| UserId | Guid | Owner user id |
| JobTitle | string? | Business title |
| Department | string? | Department or team |
| Organization | EmployerOrganizationDto? | Included when available |

Do not expose `PasswordHash`, internal tokens, authorization internals, persistence details, or domain/entity navigation objects.

### UpsertEmployerProfileRequest

| Property | Type | Notes |
|----------|------|-------|
| JobTitle | string? | Trimmed, max 160 |
| Department | string? | Trimmed, max 160 |

### EmployerOrganizationDto

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Organization id |
| EmployerProfileId | Guid | Owning employer profile id |
| Name | string | Organization name |
| Type | string? | Organization type |
| WebsiteUrl | string? | Website URL |
| CountryId | Guid? | Country id |
| CountryName | string? | Country name when available |
| City | string? | City |
| AddressLine1 | string? | Business address line |
| AddressLine2 | string? | Business address line |
| PostalCode | string? | Postal code |
| Description | string? | Organization description |

### UpsertEmployerOrganizationRequest

| Property | Type | Notes |
|----------|------|-------|
| Name | string | Required, trimmed, max 200 |
| Type | string? | Trimmed, max 100 |
| WebsiteUrl | string? | Trimmed, absolute HTTP/HTTPS URL when supplied, max 500 |
| CountryId | Guid? | Existing active `Country` when supplied |
| City | string? | Trimmed, max 120 |
| AddressLine1 | string? | Trimmed, max 200 |
| AddressLine2 | string? | Trimmed, max 200 |
| PostalCode | string? | Trimmed, max 40 |
| Description | string? | Trimmed, max 2000 |

## 7. Validation Rules

Use FluentValidation for request-shape validation in the Application layer.

Employer profile validation:

- `JobTitle` is optional, trimmed before persistence, and must be at most 160 characters.
- `Department` is optional, trimmed before persistence, and must be at most 160 characters.

Organization validation:

- `Name` is required after trimming and must be at most 200 characters.
- `Type` is optional, trimmed before persistence, and must be at most 100 characters.
- `WebsiteUrl` is optional, trimmed before persistence, must be at most 500 characters, and must be an absolute HTTP/HTTPS URL when supplied.
- Non-HTTP/HTTPS absolute URLs are invalid.
- `City` is optional, trimmed before persistence, and must be at most 120 characters.
- `AddressLine1` and `AddressLine2` are optional, trimmed before persistence, and must be at most 200 characters.
- `PostalCode` is optional, trimmed before persistence, and must be at most 40 characters.
- `Description` is optional, trimmed before persistence, and must be at most 2000 characters.
- `CountryId` is optional. Validate database-backed reference data in handlers, not FluentValidation, unless the Phase 5 Country validation pattern clearly differs at implementation time.

Reference-data validation:

- Handlers validate `CountryId` against active `Country` rows.
- Missing or inactive `CountryId` must follow the same Application-layer pattern used in Phase 5 Country validation.
- Do not invent a new exception type or middleware mapping for this in Phase 6A.
- If the Phase 5 pattern is unclear at implementation time, stop and ask for review.

## 8. Authorization and Ownership Rules

- All Phase 6A endpoints use `.RequireAuthorization()` only.
- Do not use `.RequirePermission(...)` in Phase 6A.
- Unauthenticated requests must return 401 through existing authentication behavior.
- Authenticated users without the `Employer` role must receive 403 via `ForbiddenAccessException`.
- Only authenticated users with the `Employer` role may create or manage employer profile and organization data.
- Every handler must operate only on the current authenticated user's `UserId` from the existing current-user abstraction.
- A user can only read or update their own `EmployerProfile` and `EmployerOrganization`.
- Do not accept `UserId`, `EmployerProfileId`, or ownership identifiers from client request bodies for self-service endpoints.
- If an owned row is not found for the current user, use the existing not-found pattern without leaking other users' data.
- Do not expose nurse data to employers in Phase 6A.

## 9. Testing Requirements

Application tests must cover:

- Employer role guard allows authenticated users with the `Employer` role.
- Employer role guard rejects authenticated users without the `Employer` role using `ForbiddenAccessException`.
- `GET` employer profile returns not found when no profile exists for an employer user.
- `PUT` employer profile creates a profile for the current employer user.
- `PUT` employer profile updates the existing profile instead of creating duplicates.
- Profile fields are trimmed before persistence and response projection.
- `GET` organization returns not found when no organization exists.
- `PUT` organization creates the employer profile when needed and creates the organization.
- `PUT` organization updates the existing organization instead of creating duplicates.
- Organization fields are trimmed before persistence and response projection.
- Supplied `CountryId` must exist and be active.
- Missing or inactive `CountryId` follows the same Application-layer pattern used in Phase 5 Country validation.

WebApi integration tests must cover:

- Each endpoint returns 401 without JWT.
- Each endpoint returns 403 for an authenticated non-Employer user.
- Valid Employer JWT can upsert and read employer profile.
- Valid Employer JWT can upsert and read organization profile.
- Invalid request payloads return validation Problem Details.
- Raw JSON responses do not contain sensitive fields such as `passwordHash`, internal tokens, or internal authorization state.

Testing constraints:

- Endpoint tests for `.RequireAuthorization()` must not configure or depend on permission-service setup.
- Tests must follow existing JWT helper and WebApi factory patterns.
- Test names must exactly describe the behavior proven.
- Security response tests must inspect raw JSON before deserializing DTOs.

## 10. EF Migration Considerations

The later implementation task should create an EF Core migration named `AddEmployerModule` after domain entities, EF configurations, and DbContext updates are implemented.

Required schema characteristics:

- Table names use plural PascalCase names, expected `EmployerProfiles` and `EmployerOrganizations`.
- Columns use PascalCase.
- Primary keys are `Guid`.
- `EmployerProfiles.UserId` has a unique index.
- `EmployerOrganizations.EmployerProfileId` has a unique index.
- `User` to `EmployerProfile` delete behavior is Restrict.
- `EmployerProfile` to `EmployerOrganization` delete behavior is Cascade.
- `Country` to `EmployerOrganization` delete behavior is Restrict.
- Audit fields should be included through `AuditableEntity` unless an existing entity pattern requires otherwise.
- No manual schema changes are allowed.

Migration verification during implementation:

```bash
dotnet ef migrations add AddEmployerModule --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
```

## 11. Risks and Reviewer Checkpoints

Risks:

- Accidentally implementing recruitment or nurse data exposure during employer foundation work.
- Duplicating personal identity fields that already belong to `User`.
- Adding permission-protected endpoints when the approved requirement is `.RequireAuthorization()` plus Application-layer role enforcement.
- Introducing multi-user organization membership before the product model is approved.
- Treating reference-data validation as request-shape validation instead of handler/database validation.

Reviewer checkpoints:

- Confirm Phase 6A remains foundation-only and does not expose nurse data.
- Confirm endpoint scope is limited to the four approved `/me/employer-profile` endpoints.
- Confirm `Employer` role enforcement happens in Application handlers.
- Confirm no `.RequirePermission(...)` endpoint is added.
- Confirm no admin, recruitment, search, contact-request, messaging, frontend, or migration work is performed during planning.
- Confirm implementation tasks stop for review and do not proceed automatically to later Phase 6 work.
