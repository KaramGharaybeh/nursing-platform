# Phase 6C - Candidate Filtering Specification

## Objective

Extend the existing employer-facing candidate listing endpoint with safe optional filtering for recruitment-visible nurse candidates. Phase 6C must preserve the Phase 6B security model, DTO contract, pagination behavior, deterministic sorting, and raw JSON security guarantees while narrowing the eligible candidate set by explicitly approved filter parameters.

This is a read-only Recruitment enhancement within Phase 6. It must not introduce contact workflows, candidate communication, CV access, frontend work, or later-phase behavior.

## Current Baseline

Phase 6B already provides:

- `GET /api/v1/recruitment/candidates`
- JWT authentication through `.RequireAuthorization()`
- Application-layer Employer role enforcement through `EmployerRoleGuard`
- Employer profile and employer organization prerequisites before candidate data is queried
- Candidate eligibility filtering for `IsAvailableForRecruitment == true`, active user accounts, and verified user email
- Existing pagination defaults and limits through `ListCandidatesQuery`
- Deterministic default sorting by years of experience descending, profile creation time descending, and profile id ascending
- Recruitment-specific `CandidateListItemDto` and `CandidateLanguageDto`
- Raw JSON WebApi security tests that assert sensitive fields and values are absent

Phase 6C must build on this endpoint and query handler instead of adding a new endpoint or changing the route.

## In Scope

- Add optional candidate filters to the existing `ListCandidatesQuery`.
- Extend the existing endpoint query parameter binding for the approved filters only.
- Apply filters after employer role/prerequisite checks and after baseline nurse eligibility rules.
- Count only candidates that satisfy eligibility and filters.
- Preserve pagination compatibility: apply `CountAsync` after filters, then deterministic sorting, then `Skip` and `Take`.
- Preserve deterministic sorting compatibility with no new sort parameter.
- Preserve the existing candidate DTO shape and security posture.
- Add Application tests for filter behavior, filter combinations, validation, pagination, and sorting compatibility.
- Add WebApi integration tests for query binding, validation errors, response shape, and unchanged raw JSON security assertions.
- Review existing EF indexes and add indexing recommendations only when implementation evidence supports them.

## Out of Scope

- Contact requests.
- Messaging.
- CV access, CV download, CV URLs, or file storage keys.
- Nurse contact information.
- Saved searches.
- Notifications.
- Payments or subscriptions.
- Admin approval workflows.
- Frontend implementation.
- Phase 6D contact requests.
- Phase 7 examination module work.
- New candidate detail endpoints.
- New sorting parameters.

## API Contract

Extend the existing endpoint:

```http
GET /api/v1/recruitment/candidates
```

Approved optional query parameters:

- `page`: optional integer, default `1`, minimum `1`.
- `pageSize`: optional integer, default `20`, minimum `1`, maximum `100`.
- `licenseCountryId`: optional GUID. Matches `NurseProfile.LicenseCountryId`.
- `currentCountryId`: optional GUID. Matches `NurseProfile.CurrentCountryId`.
- `minimumYearsOfExperience`: optional integer. Includes candidates whose `YearsOfExperience` is greater than or equal to the supplied value.
- `skills`: optional repeated and/or comma-separated string values. Matches nurse skills by normalized skill name.
- `languageId`: optional GUID. Matches candidates with a `NurseLanguage.LanguageId`.

Phase 6C must support both skill filter forms:

```http
GET /api/v1/recruitment/candidates?skills=ICU&skills=Triage
GET /api/v1/recruitment/candidates?skills=ICU,Triage
```

Repeated and comma-separated skill filters must share one parsing and normalization pipeline:

1. Read all supplied `skills` entries.
2. Split comma-separated entries.
3. Trim whitespace from each parsed skill value.
4. Reject blank supplied skill values.
5. Normalize using the existing nurse skill normalization behavior.
6. Deduplicate by normalized name.
7. Match ALL supplied normalized skills.

## Query Model

Extend `ListCandidatesQuery` with:

- `Guid? LicenseCountryId`
- `Guid? CurrentCountryId`
- `int? MinimumYearsOfExperience`
- `IReadOnlyCollection<string> Skills`
- `Guid? LanguageId`

Keep `Page` and `PageSize` defaults unchanged.

Skill filter values must be normalized using the existing skill normalization behavior from nurse skill management:

- Trim and collapse whitespace for display-normalized input.
- Compare using the stored `NurseSkill.NormalizedName`.
- Match case-insensitively through the normalized comparison value.
- Ignore no values silently only when they are absent. Blank supplied skill values are invalid.
- De-duplicate normalized skill filters before applying the query.

Skills filtering must always use MATCH ALL semantics in Phase 6C. Every supplied normalized skill must be present on the candidate profile, and the implementation plan must not change this.

## Validation and Error Behavior

Keep existing pagination validation unchanged:

- `Page >= 1`
- `PageSize >= 1`
- `PageSize <= 100`

Add validation:

- `MinimumYearsOfExperience >= 0` when provided.
- Each supplied skill value must be non-empty after whitespace normalization.
- Phase 6C uses a maximum of 20 normalized skill names. The implementation plan must not change this.
- Each skill value must respect the existing nurse skill max length of `100` after display-name normalization.
- Duplicate normalized skill filter values are accepted only if they are de-duplicated before query execution; tests must prove duplicates do not change results.

Invalid filter values must return validation Problem Details with `400 Bad Request`.

Invalid GUID values for `licenseCountryId`, `currentCountryId`, or `languageId` must return `400 Bad Request` through WebApi binding/model validation behavior before the request reaches the Application handler.

Missing JWT must remain `401 Unauthorized`.

Authenticated non-Employer users and Employers without required profile/organization prerequisites must remain `403 Forbidden`.

Empty filtered result sets must return `200 OK` with an empty `items` array and accurate pagination metadata.

## Application Behavior

The query handler must preserve the Phase 6B ordering of responsibility:

1. Enforce authenticated Employer role.
2. Enforce employer profile and organization prerequisites.
3. Build the eligible candidate query only after prerequisites pass.
4. Apply baseline candidate eligibility.
5. Apply optional filters.
6. Count filtered candidates.
7. Apply deterministic sorting.
8. Apply pagination.
9. Project to the existing safe DTO shape.

Filter requirements:

- `licenseCountryId` filters profiles where `LicenseCountryId == licenseCountryId`.
- `currentCountryId` filters profiles where `CurrentCountryId == currentCountryId`.
- `minimumYearsOfExperience` filters profiles where `YearsOfExperience >= minimumYearsOfExperience`.
- `languageId` filters profiles with at least one matching `NurseLanguage`.
- `skills` filters profiles by stored normalized skill names and must avoid exposing `NormalizedName` in DTOs or API responses.
- Filters must compose with each other using AND semantics.
- Filters must not weaken the baseline visibility, active-account, or email-verification eligibility rules.
- Candidate data must not be queried when employer prerequisites fail.

## DTO and Security Requirements

The response DTOs must remain unchanged unless the reviewer explicitly approves a separate DTO change. Phase 6C must not expose:

- User ids.
- Email or phone values.
- Password hashes.
- Roles or permissions.
- Tokens or token hashes.
- Email verification or account active state.
- License numbers.
- CV storage keys, URLs, file paths, or download links.
- Full legal names.
- Skill normalized names.
- EF navigation objects.
- Domain or database entities.

Existing raw JSON security tests must remain in place and must still inspect the raw response string before deserialization. Phase 6C should add filtered-response coverage to the same security assertion path so new query behavior cannot regress DTO safety.

## Pagination and Sorting Compatibility

Filtering must happen before `TotalCount`, `TotalPages`, `Skip`, and `Take`.

Default sorting must remain:

1. `YearsOfExperience` descending.
2. `NurseProfile.CreatedAt` descending.
3. `NurseProfile.Id` ascending.

Tests must prove:

- Page 2 after filtering still proves both `Skip` and `Take`.
- `TotalCount` includes only filtered eligible candidates.
- `TotalPages`, `Page`, `PageSize`, first item, and last item are deterministic.
- Sorting tie-breakers still apply when filters are present.

## Testing Requirements

Application tests must cover:

- License country filter includes only matching eligible candidates.
- Current country filter includes only matching eligible candidates.
- Minimum years of experience includes candidates at the boundary and excludes lower values.
- Language filter includes candidates with the requested `LanguageId`.
- Skill filter normalizes input and matches stored `NormalizedName`.
- Multiple skill filters use AND semantics.
- Duplicate skill filters are de-duplicated or otherwise proven not to change results.
- Multiple filter types compose with AND semantics.
- Filters do not include unavailable, inactive, or unverified nurses.
- Employer prerequisite failures still avoid candidate set access where existing tests can prove it.
- Filtered pagination proves both `Skip` and `Take`.
- Deterministic sorting remains stable under filters.
- Validator rejects invalid page, page size, negative minimum experience, blank skills, too many skills, and over-length skills.

WebApi integration tests must cover:

- Query parameters are mapped into `ListCandidatesQuery`.
- Default pagination remains unchanged when filters are absent.
- Invalid filter validation returns `400` Problem Details.
- At least one invalid GUID filter value returns `400 Bad Request` through WebApi binding/model validation behavior.
- Filtered success response remains `200 OK` and uses `PaginatedResult<CandidateListItemDto>`.
- Raw JSON security checks are unchanged and run against filtered responses.
- No permission service setup is required because the endpoint remains `.RequireAuthorization()` only.

## Performance and Indexing Considerations

Existing helpful indexes:

- `NurseProfiles.UserId` unique.
- `NurseProfiles.LicenseCountryId`.
- `NurseProfiles.CurrentCountryId`.
- `NurseSkills.NurseProfileId`.
- `NurseSkills (NurseProfileId, NormalizedName)` unique.
- `NurseLanguages.NurseProfileId`.
- `NurseLanguages (NurseProfileId, LanguageId)` unique.

Implementation planning must evaluate whether additional indexes are justified by query shape and EF-generated SQL. Potential candidates:

- `NurseSkills (NormalizedName, NurseProfileId)` for skill-first filtering.
- `NurseLanguages (LanguageId, NurseProfileId)` for language-first filtering.
- A composite candidate listing index involving `IsAvailableForRecruitment`, `YearsOfExperience`, `CreatedAt`, and `Id`.
- A profile filter/sort composite that includes country filters only if evidence shows a real need.

Do not create migrations during the specification phase. During implementation, migrations must be created only if an approved plan requires EF model/index changes and verification confirms no pending unintended model drift.

## Acceptance Criteria

- A Phase 6C implementation plan can be written from this spec without inventing filter semantics.
- The existing candidate listing endpoint supports only the approved optional filters.
- Employer role and prerequisite enforcement remain unchanged.
- Baseline candidate eligibility remains unchanged.
- Filters compose safely before count, sorting, and pagination.
- Pagination and deterministic sorting behavior remain compatible with Phase 6B.
- DTO security remains unchanged, including raw JSON sensitive-field checks.
- Performance/indexing concerns are explicitly reviewed before implementation completion.
- No contact requests, messaging, CV access, contact info, saved searches, notifications, payments, admin approval, frontend, Phase 6D, or Phase 7 work is included.
