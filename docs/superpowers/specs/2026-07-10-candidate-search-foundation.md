# Phase 6B — Employer-facing Candidate Search Foundation Specification

## Objective

Implement a read-only candidate search foundation for authenticated employers. The feature lets eligible employers list recruitment-visible nurse candidates through a safe, paginated API response without exposing contact information, account internals, CV access, legal names, or domain/persistence objects.

Speed for Phase 6B comes from narrow scope only. Implementation quality, Clean Architecture boundaries, authorization, DTO safety, raw JSON security tests, pagination correctness, and documentation accuracy must remain production-grade.

## In Scope

- Employer-authenticated candidate listing endpoint.
- Recruitment module concept for candidate search code organization.
- Pagination using the existing `PaginatedResult<T>` shape.
- Deterministic default sorting.
- Minimal employer-facing candidate summary DTO.
- Enforcement of nurse recruitment visibility.
- Enforcement of active nurse account status.
- Enforcement of verified nurse email because `User.EmailVerified` exists in the current Identity model.
- Employer prerequisite checks requiring the authenticated Employer user to have both an `EmployerProfile` and an `EmployerOrganization` before searching.
- Raw JSON response tests proving sensitive/internal fields are not exposed.
- Application tests and WebApi integration tests.
- EF/indexing review and recommendations only; no migration implementation in the specification phase.

## Out of Scope

- Advanced filtering.
- Advanced sorting.
- Contact requests.
- CV download or CV file access.
- Nurse contact information.
- Employer messaging.
- Saved searches.
- Notifications.
- Payments or subscriptions.
- Admin approval workflow.
- Frontend work.
- Phase 6C, Phase 6D, Phase 7, or later-phase work.

## Architecture and Module Placement

Candidate search is conceptually part of Recruitment, not Employer Profile Management. Implement Phase 6B code under a Recruitment feature namespace/folder while keeping roadmap sequencing under Phase 6 because `TASKS.md` currently groups candidate search under the Employer Module.

Clean Architecture rules apply:

- WebApi maps HTTP only and delegates to Application through MediatR.
- Application owns the search query, validation, authorization/prerequisite checks, projection, pagination, and DTO shape.
- Domain entities remain persistence-ignorant and must not be exposed in API responses.
- Infrastructure remains responsible for EF mappings and migrations only when implementation later proves a schema/index change is needed.

## Existing Code Integration Points

- Reuse `IApplicationDbContext` for `Users`, `NurseProfiles`, `NurseExperiences`, `NurseEducation`, `NurseCertificates`, `NurseLanguages`, `NurseSkills`, `EmployerProfiles`, and `EmployerOrganizations`.
- Reuse `EmployerRoleGuard` for authenticated Employer role enforcement unless implementation proves a Recruitment-specific guard is needed to combine role and employer-prerequisite checks cleanly.
- Reuse `PaginatedResult<T>` for paginated responses.
- Follow existing `ListUsersQuery` pagination validation patterns: `Page >= 1`, `PageSize >= 1`, `PageSize <= 100`.
- Follow existing Minimal API endpoint mapping style in `ApplicationBuilderExtensions`.
- Do not reuse nurse self-service DTOs for employer-facing candidate responses. Create explicit Recruitment DTOs.

## Authorization Rules

- Endpoint must require JWT authentication.
- The authenticated user must have the `Employer` role.
- Authenticated non-Employer users must receive `403 Forbidden` through the existing mapped `ForbiddenAccessException` behavior.
- Unauthenticated requests must receive `401 Unauthorized`.
- Do not use admin permissions for Phase 6B.
- Do not use `.AllowAnonymous()`.

## Employer Prerequisites

Before returning candidates, the Application layer must verify that the current employer user has:

- An `EmployerProfile` owned by the current user.
- An `EmployerOrganization` linked to that employer profile.

If either prerequisite is missing, authenticated Employer users must receive `403 Forbidden` through the existing `ForbiddenAccessException` behavior. Candidate data must not be queried or returned when prerequisites are missing.

## Nurse Eligibility Rules

A nurse candidate is searchable only when all of the following are true:

- `NurseProfile.IsAvailableForRecruitment == true`.
- The linked `User.IsActive == true`.
- The linked `User.EmailVerified == true`.

Nurses that fail any eligibility rule must not appear in `Items`, must not contribute to `TotalCount`, and must not be inferable through response counts.

## API Contract

Endpoint:

```http
GET /api/v1/recruitment/candidates?page=1&pageSize=20
```

Authorization:

```text
RequireAuthorization() plus Application-layer Employer role and employer-prerequisite checks.
```

Query parameters:

- `page`: optional integer, default `1`, minimum `1`.
- `pageSize`: optional integer, default `20`, minimum `1`, maximum `100`.

Response:

```json
{
  "items": [
    {
      "nurseProfileId": "00000000-0000-0000-0000-000000000000",
      "headline": "Critical care nurse",
      "professionalSummary": "Experienced ICU nurse...",
      "licenseCountryName": "Canada",
      "currentCountryName": "Canada",
      "yearsOfExperience": 8,
      "skills": ["ICU", "Triage"],
      "languages": [
        { "name": "English", "code": "en", "proficiency": "Fluent" }
      ],
      "certificatesSummary": "2 certificates",
      "certificatesCount": 2,
      "latestExperienceTitle": "Senior ICU Nurse",
      "educationSummary": "Bachelor of Nursing"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

Default sorting must be deterministic. Recommended default order:

1. `YearsOfExperience` descending.
2. `NurseProfile.CreatedAt` descending.
3. `NurseProfile.Id` ascending as a stable tie-breaker.

Do not add advanced sorting parameters in Phase 6B.

## DTO Contract

Create Recruitment-specific DTOs, likely:

- `CandidateListItemDto`
- `CandidateLanguageDto`

Allowed candidate list fields:

- `NurseProfileId`
- `Headline`
- `ProfessionalSummary`, because it is already nurse-controlled profile content and guarded by `IsAvailableForRecruitment`.
- `LicenseCountryName`
- `CurrentCountryName`
- `YearsOfExperience`
- `Skills`, as display names only.
- `Languages`, as language name/code/proficiency only.
- `CertificatesSummary` and/or `CertificatesCount`.
- `LatestExperienceTitle`, preferring current experience title when available; otherwise latest by start date.
- `EducationSummary`, using degree/field summary only.

DTOs must not include EF navigation objects, domain entities, persistence-only fields, or self-service-only nurse DTOs.

## Forbidden Data Exposure

The candidate API must never expose:

- `UserId`
- Email
- Phone or contact details
- `PasswordHash`
- Roles
- Permissions
- Tokens, token hashes, access tokens, refresh tokens
- Email verification/internal account state
- License number
- CV storage key
- CV URL/download URL
- Internal file path
- Full legal name
- EF navigation objects such as `user`, `nurseProfile`, `licenseCountry`, `currentCountry`, `country`, or related domain objects
- Domain entities or database entities

WebApi integration tests must inspect the raw JSON response and assert these fields/terms are absent case-insensitively where applicable.

## Validation and Error Behavior

- Invalid `page` or `pageSize` values must return validation Problem Details with `400 Bad Request`.
- Missing JWT must return `401 Unauthorized`.
- Authenticated non-Employer users must return `403 Forbidden`.
- Authenticated Employer users without an employer profile or organization must not receive candidate data; use a consistent mapped error response defined during implementation planning.
- Empty eligible result sets must return `200 OK` with an empty `items` array and accurate pagination metadata.

## Application-layer Behavior

The query handler must:

- Enforce Employer role access before querying candidate data.
- Enforce employer profile and organization prerequisites before querying candidate data.
- Apply nurse eligibility rules before counting and pagination.
- Count only eligible nurses.
- Apply deterministic sorting before `Skip` and `Take`.
- Project explicitly to Recruitment DTOs.
- Avoid returning or materializing domain entities for API response use.
- Avoid N+1 query behavior where practical; use explicit projection and bounded nested summaries.
- Keep Phase 6B free of advanced filters and contact-request behavior.

## WebApi Behavior

- Add only the Phase 6B candidate listing endpoint.
- Place the endpoint under the existing `/api/v1` API version prefix.
- Use a Recruitment route group or route naming consistent with the module placement.
- Require authentication at the endpoint/group level.
- Do not add `.RequirePermission(...)` unless a later approved task changes authorization requirements.
- Do not add endpoints for filtering, contact requests, CV download, messaging, saved searches, notifications, admin approval, frontend, Phase 6C, Phase 6D, or Phase 7.

## Testing Requirements

Application tests must cover:

- Unauthenticated current user behavior through the guard/service path.
- Authenticated non-Employer user returns forbidden behavior.
- Employer without profile/organization cannot search.
- Only `IsAvailableForRecruitment == true` nurses are returned.
- Inactive nurse accounts are excluded.
- Unverified nurse accounts are excluded.
- Pagination proves both `Skip` and `Take` by requesting a page after the first page and asserting total count, total pages, page, page size, item count, first item, and last item.
- Deterministic default sorting including tie-breaker behavior.
- DTO projection excludes forbidden fields and includes only allowed summary fields.

WebApi integration tests must cover:

- `401` without JWT.
- `403` for authenticated non-Employer users.
- Error behavior for Employer users missing profile/organization prerequisites.
- `400` validation Problem Details for invalid pagination.
- `200` for an eligible Employer user.
- Raw JSON security checks for forbidden fields.
- Response shape matches `PaginatedResult<CandidateListItemDto>`.

## Performance and Indexing Considerations

Phase 6B implementation may work with existing indexes initially, but the implementation plan must evaluate query plans and EF model drift before final verification.

Existing helpful indexes include:

- `NurseProfiles.UserId` unique.
- `NurseProfiles.LicenseCountryId`.
- `NurseProfiles.CurrentCountryId`.
- Child-table `NurseProfileId` indexes.
- `NurseSkills (NurseProfileId, NormalizedName)` unique.
- `NurseLanguages (NurseProfileId, LanguageId)` unique.

Potential future indexes, if implementation evidence justifies them:

- Composite index on `NurseProfiles(IsAvailableForRecruitment, YearsOfExperience, CreatedAt, Id)` for default candidate listing.
- Index support for `Users(IsActive, EmailVerified)` or query shape alternatives if joins become expensive.
- Language/skill filter indexes are more relevant to Phase 6C and should not be added prematurely in Phase 6B unless required for the foundation query.

No migration should be created in the spec. During implementation, migrations must only be created if EF/indexing changes are explicitly required and verified.

## Security and Privacy Risks

- Candidate enumeration and scraping risk: keep pagination bounded and do not expose unnecessary identifiers beyond `NurseProfileId`.
- PII leakage risk: never expose email, legal names, contact details, license numbers, or account state.
- Authorization bypass risk: enforce role and employer prerequisites in Application, not only WebApi.
- Visibility leakage risk: exclude ineligible nurses before count and pagination.
- DTO regression risk: use raw JSON tests against sensitive/internal field names.
- Logging risk: do not log candidate PII or future search parameters.

## Acceptance Criteria

- A Phase 6B implementation plan can be written from this spec without inventing business rules.
- Candidate search is implemented conceptually under Recruitment while remaining sequenced under Phase 6.
- Only one read-only candidate listing endpoint is introduced.
- Endpoint requires authentication and Application-layer Employer role enforcement.
- Employer profile and organization prerequisites are enforced before returning candidates.
- Only recruitment-visible, active, email-verified nurse accounts are searchable.
- Response uses explicit Recruitment DTOs and existing pagination shape.
- Pagination, deterministic sorting, and safe projection are covered by automated tests.
- Raw JSON WebApi tests prove forbidden fields are absent.
- No advanced filtering, advanced sorting, contact requests, CV access, contact info, messaging, saved searches, notifications, payments, admin approval, frontend, Phase 6C, Phase 6D, or Phase 7 work is included.
