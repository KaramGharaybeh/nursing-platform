# Phase 5 — Nurse Module

## 1. Objective

Implement the Phase 5 Profile Foundation for authenticated nurse self-service.

This phase gives nurse users a structured profile they can manage themselves, including profile details, experience, education, certificate metadata, languages, free-text skills, and CV upload metadata. The module must follow the existing Clean Architecture, CQRS, MediatR, FluentValidation, EF Core, and Minimal API patterns established in earlier phases.

Phase 5 does not expose nurse data to employers and does not implement recruitment search. It creates the nurse-owned data foundation required for later recruitment functionality.

## 2. Approved Scope

In scope:

- Nurse profile self read/upsert for authenticated nurse users.
- Experience CRUD for the authenticated nurse-owned profile.
- Education CRUD for the authenticated nurse-owned profile.
- Certificates CRUD, metadata only.
- Languages management using existing `Language` reference data.
- Skills management as nurse-owned free-text skills.
- CV upload with validation, storage abstraction, metadata persistence, and delete.
- Application tests and WebApi integration tests.
- EF Core migration during later implementation tasks.

Endpoint scope:

| Area | Endpoint | Method | Auth |
|------|----------|--------|------|
| Profile | `/api/v1/me/nurse-profile` | GET | `.RequireAuthorization()` |
| Profile | `/api/v1/me/nurse-profile` | PUT | `.RequireAuthorization()` |
| Experience | `/api/v1/me/nurse-profile/experiences` | GET | `.RequireAuthorization()` |
| Experience | `/api/v1/me/nurse-profile/experiences` | POST | `.RequireAuthorization()` |
| Experience | `/api/v1/me/nurse-profile/experiences/{id}` | PUT | `.RequireAuthorization()` |
| Experience | `/api/v1/me/nurse-profile/experiences/{id}` | DELETE | `.RequireAuthorization()` |
| Education | `/api/v1/me/nurse-profile/education` | GET | `.RequireAuthorization()` |
| Education | `/api/v1/me/nurse-profile/education` | POST | `.RequireAuthorization()` |
| Education | `/api/v1/me/nurse-profile/education/{id}` | PUT | `.RequireAuthorization()` |
| Education | `/api/v1/me/nurse-profile/education/{id}` | DELETE | `.RequireAuthorization()` |
| Certificates | `/api/v1/me/nurse-profile/certificates` | GET | `.RequireAuthorization()` |
| Certificates | `/api/v1/me/nurse-profile/certificates` | POST | `.RequireAuthorization()` |
| Certificates | `/api/v1/me/nurse-profile/certificates/{id}` | PUT | `.RequireAuthorization()` |
| Certificates | `/api/v1/me/nurse-profile/certificates/{id}` | DELETE | `.RequireAuthorization()` |
| Languages | `/api/v1/me/nurse-profile/languages` | GET | `.RequireAuthorization()` |
| Languages | `/api/v1/me/nurse-profile/languages` | PUT | `.RequireAuthorization()` |
| Skills | `/api/v1/me/nurse-profile/skills` | GET | `.RequireAuthorization()` |
| Skills | `/api/v1/me/nurse-profile/skills` | PUT | `.RequireAuthorization()` |
| CV | `/api/v1/me/nurse-profile/cv` | GET | `.RequireAuthorization()` |
| CV | `/api/v1/me/nurse-profile/cv` | POST | `.RequireAuthorization()` |
| CV | `/api/v1/me/nurse-profile/cv` | DELETE | `.RequireAuthorization()` |

## 3. Explicit Out of Scope

The following are explicitly out of scope for Phase 5:

- Admin nurse list/detail endpoints.
- Employer candidate search.
- Employer access to nurse data.
- Recruitment contact requests.
- Profile approval workflow.
- Admin skill/certificate/language/reference-data management.
- CV download endpoint.
- Certificate file attachments.
- Frontend implementation.
- Payments, exams, notifications, and Phase 6+ features.
- Global `Skill` reference data table.
- Certificate document storage.
- Public nurse profile pages.
- Profile moderation.
- Soft delete for nurse child records.

## 4. Entity Model

All new entities belong in `NursingPlatform.Domain.Nurses` and should use `Guid` primary keys. Aggregate-root and business child entities should inherit `AuditableEntity` unless there is a documented reason not to.

### NurseProfile

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| UserId | Guid | Required FK to `User`; unique index; delete behavior Restrict |
| Headline | string? | Max 160 |
| ProfessionalSummary | string? | Max 2000 |
| LicenseNumber | string? | Max 100 |
| LicenseCountryId | Guid? | Optional FK to `Country`; Restrict |
| CurrentCountryId | Guid? | Optional FK to `Country`; Restrict |
| YearsOfExperience | int | Required, default 0 |
| IsAvailableForRecruitment | bool | Required, default false |
| User | User | Required navigation |
| LicenseCountry | Country? | Optional navigation |
| CurrentCountry | Country? | Optional navigation |

Notes:

- One user can have at most one nurse profile.
- Profile personal identity fields such as first name, last name, and email remain on `User`; do not duplicate them in `NurseProfile`.
- Date of birth, phone number, nationality, and address are not included in Phase 5 to avoid introducing additional PII before explicit approval.

### NurseExperience

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| NurseProfileId | Guid | Required FK to `NurseProfile`; Cascade |
| FacilityName | string | Required, max 200 |
| JobTitle | string | Required, max 160 |
| CountryId | Guid? | Optional FK to `Country`; Restrict |
| StartDate | DateOnly | Required |
| EndDate | DateOnly? | Null when current |
| IsCurrent | bool | Required |
| Description | string? | Max 2000 |

### NurseEducation

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| NurseProfileId | Guid | Required FK to `NurseProfile`; Cascade |
| InstitutionName | string | Required, max 200 |
| Degree | string | Required, max 160 |
| FieldOfStudy | string? | Max 160 |
| CountryId | Guid? | Optional FK to `Country`; Restrict |
| StartDate | DateOnly? | Optional |
| EndDate | DateOnly? | Optional |
| Description | string? | Max 2000 |

### NurseCertificate

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| NurseProfileId | Guid | Required FK to `NurseProfile`; Cascade |
| Name | string | Required, max 200 |
| IssuingOrganization | string | Required, max 200 |
| IssueDate | DateOnly? | Optional |
| ExpirationDate | DateOnly? | Optional |
| CredentialId | string? | Max 160 |
| CredentialUrl | string? | Max 500, absolute URL when supplied |

Certificates are metadata only. Certificate file attachments are out of scope.

### NurseLanguage

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| NurseProfileId | Guid | Required FK to `NurseProfile`; Cascade |
| LanguageId | Guid | Required FK to existing `Language`; Restrict |
| Proficiency | string | Required enum string |

Allowed proficiency values:

- `Beginner`
- `Intermediate`
- `Advanced`
- `Fluent`
- `Native`

Add a unique index on `(NurseProfileId, LanguageId)`.

### NurseSkill

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| NurseProfileId | Guid | Required FK to `NurseProfile`; Cascade |
| Name | string | Required, max 100 |
| NormalizedName | string | Required, max 100, upper-invariant normalized value |

Skills are nurse-owned free-text rows in Phase 5. There is no global `Skill` reference table in this phase.

Normalization rules:

1. Trim leading and trailing whitespace.
2. Collapse internal whitespace to a single space.
3. Convert to upper invariant for `NormalizedName`.
4. Prevent duplicates per nurse using `(NurseProfileId, NormalizedName)`.

### NurseCvDocument

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | Primary key |
| NurseProfileId | Guid | Required FK to `NurseProfile`; Cascade |
| OriginalFileName | string | Sanitized display metadata only, max 255 |
| StorageKey | string | Required internal key, max 500; never returned by API |
| ContentType | string | Required, max 100 |
| FileSizeBytes | long | Required |
| UploadedAt | DateTime | Required UTC |

Only one active CV document is stored per nurse profile. Uploading a new CV replaces the previous metadata and deletes the previous stored file through the storage abstraction after successful new file storage.

## 5. API Endpoint Contracts

All endpoints are under `/api/v1` and require a valid JWT via `.RequireAuthorization()`.

### `GET /me/nurse-profile`

Returns the authenticated nurse user's profile. If the user has the `Nurse` role but no profile exists yet, return 404. If the authenticated user does not have the `Nurse` role, throw `ForbiddenAccessException` and return HTTP 403 Problem Details.

Response 200: `NurseProfileDto`.

### `PUT /me/nurse-profile`

Creates or updates the authenticated nurse user's profile. The handler must create the profile if it does not exist and update it if it does.

Request: `UpsertNurseProfileRequest`.

Response 200: `NurseProfileDto`.

### Experience Endpoints

- `GET /me/nurse-profile/experiences` returns all experience records for the authenticated nurse profile, sorted by `StartDate` descending and then `CreatedAt` descending.
- `POST /me/nurse-profile/experiences` creates one experience record.
- `PUT /me/nurse-profile/experiences/{id}` updates one owned experience record.
- `DELETE /me/nurse-profile/experiences/{id}` hard deletes one owned experience record and returns 204.

### Education Endpoints

- `GET /me/nurse-profile/education` returns all education records, sorted by `EndDate` descending nulls first, then `StartDate` descending.
- `POST /me/nurse-profile/education` creates one education record.
- `PUT /me/nurse-profile/education/{id}` updates one owned education record.
- `DELETE /me/nurse-profile/education/{id}` hard deletes one owned education record and returns 204.

### Certificate Endpoints

- `GET /me/nurse-profile/certificates` returns all certificate metadata records, sorted by `IssueDate` descending and then `CreatedAt` descending.
- `POST /me/nurse-profile/certificates` creates one certificate metadata record.
- `PUT /me/nurse-profile/certificates/{id}` updates one owned certificate metadata record.
- `DELETE /me/nurse-profile/certificates/{id}` hard deletes one owned certificate metadata record and returns 204.

### Language Endpoints

- `GET /me/nurse-profile/languages` returns the nurse's selected languages with language name/code and proficiency.
- `PUT /me/nurse-profile/languages` replaces the nurse's full language list with the request payload.

Replacement semantics are intentional so duplicate detection and deterministic ordering stay simple.

### Skill Endpoints

- `GET /me/nurse-profile/skills` returns the nurse's skills sorted by `Name` ascending.
- `PUT /me/nurse-profile/skills` replaces the nurse's full skill list with the request payload after normalizing and de-duplicating names.

Replacement semantics are intentional because Phase 5 skills are simple nurse-owned free-text values.

### CV Endpoints

- `GET /me/nurse-profile/cv` returns CV metadata when present; 404 when absent.
- `POST /me/nurse-profile/cv` accepts `multipart/form-data`, validates the uploaded file, stores it through `IFileStorageService`, persists metadata, and returns CV metadata.
- `DELETE /me/nurse-profile/cv` deletes the stored file through `IFileStorageService`, hard deletes metadata, and returns 204. If no CV exists, return 404.

CV download is out of scope. No endpoint may return file bytes, storage keys, or internal paths in Phase 5.

## 6. DTO / Request / Response Models

### NurseProfileDto

```json
{
  "id": "guid",
  "userId": "guid",
  "headline": "Registered Nurse",
  "professionalSummary": "Critical care nurse with ICU experience.",
  "licenseNumber": "RN-12345",
  "licenseCountryId": "guid-or-null",
  "licenseCountryName": "United Kingdom",
  "currentCountryId": "guid-or-null",
  "currentCountryName": "United Arab Emirates",
  "yearsOfExperience": 5,
  "isAvailableForRecruitment": false
}
```

### UpsertNurseProfileRequest

```json
{
  "headline": "Registered Nurse",
  "professionalSummary": "Critical care nurse with ICU experience.",
  "licenseNumber": "RN-12345",
  "licenseCountryId": "guid-or-null",
  "currentCountryId": "guid-or-null",
  "yearsOfExperience": 5,
  "isAvailableForRecruitment": false
}
```

### NurseExperienceDto / UpsertNurseExperienceRequest

Fields:

- `id` in response only.
- `facilityName`.
- `jobTitle`.
- `countryId`.
- `countryName` in response only.
- `startDate` as ISO date.
- `endDate` as ISO date or null.
- `isCurrent`.
- `description`.

### NurseEducationDto / UpsertNurseEducationRequest

Fields:

- `id` in response only.
- `institutionName`.
- `degree`.
- `fieldOfStudy`.
- `countryId`.
- `countryName` in response only.
- `startDate` as ISO date or null.
- `endDate` as ISO date or null.
- `description`.

### NurseCertificateDto / UpsertNurseCertificateRequest

Fields:

- `id` in response only.
- `name`.
- `issuingOrganization`.
- `issueDate` as ISO date or null.
- `expirationDate` as ISO date or null.
- `credentialId`.
- `credentialUrl`.

### NurseLanguageDto / UpdateNurseLanguagesRequest

Request shape:

```json
{
  "languages": [
    {
      "languageId": "guid",
      "proficiency": "Fluent"
    }
  ]
}
```

Response item fields:

- `id`.
- `languageId`.
- `languageName`.
- `languageCode`.
- `proficiency`.

### NurseSkillDto / UpdateNurseSkillsRequest

Request shape:

```json
{
  "skills": [
    {
      "name": "Critical Care"
    }
  ]
}
```

Response item fields:

- `id`.
- `name`.

Do not expose `NormalizedName`.

### NurseCvDocumentDto

Response fields:

- `id`.
- `fileName` from sanitized `OriginalFileName`.
- `contentType`.
- `fileSizeBytes`.
- `uploadedAt`.

Do not expose `StorageKey`, storage root, internal path, or any file URL.

## 7. Validation Rules

Global validation:

- Every request DTO must have a FluentValidation validator.
- Validation failures must return existing 400 Problem Details responses.
- The handler does not need to silently fix values rejected by validators.
- FluentValidation validators handle structural/request validation such as required fields, lengths, enum values, date ordering, duplicate IDs/names inside the request, file size, extension, and content type.
- Application handlers validate database-backed reference data such as active `Country` and active `Language` rows, unless an existing project pattern clearly does otherwise.

Profile validation:

- `headline`: max 160 when supplied.
- `professionalSummary`: max 2000 when supplied.
- `licenseNumber`: max 100 when supplied.
- `yearsOfExperience`: inclusive range 0 to 80.
- `licenseCountryId`: active country when supplied.
- `currentCountryId`: active country when supplied.

Experience validation:

- `facilityName`: required, max 200.
- `jobTitle`: required, max 160.
- `countryId`: active country when supplied.
- `startDate`: required.
- `endDate`: must be null when `isCurrent` is true.
- `endDate`: must be greater than or equal to `startDate` when supplied.
- `description`: max 2000 when supplied.

Education validation:

- `institutionName`: required, max 200.
- `degree`: required, max 160.
- `fieldOfStudy`: max 160 when supplied.
- `countryId`: active country when supplied.
- `endDate`: must be greater than or equal to `startDate` when both are supplied.
- `description`: max 2000 when supplied.

Certificate validation:

- `name`: required, max 200.
- `issuingOrganization`: required, max 200.
- `expirationDate`: must be greater than or equal to `issueDate` when both are supplied.
- `credentialId`: max 160 when supplied.
- `credentialUrl`: max 500 and absolute HTTP/HTTPS URL when supplied.

Language validation:

- `languageId`: required.
- Each language ID must refer to an active `Language` row.
- `proficiency`: required and one of `Beginner`, `Intermediate`, `Advanced`, `Fluent`, `Native`.
- Duplicate language IDs in the request are invalid.
- Maximum 20 languages per nurse.

Skill validation:

- `name`: required after trim, max 100.
- Normalized duplicate skill names in the same request are invalid.
- Maximum 50 skills per nurse.

CV upload validation:

- Request must be `multipart/form-data` with one file field named `file`.
- File must not be empty.
- Max file size: 5 MB.
- Supported file types: PDF, DOC, DOCX.
- Allowed content types:
  - `application/pdf`
  - `application/msword`
  - `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- Allowed extensions:
  - `.pdf`
  - `.doc`
  - `.docx`
- Never trust the client filename for storage location.

## 8. Authorization and Ownership Rules

Endpoint authorization:

- Every Phase 5 self-service endpoint uses `.RequireAuthorization()`.
- No Phase 5 endpoint uses `.RequirePermission(...)` because admin/employer access is out of scope.
- No Phase 5 endpoint uses `.AllowAnonymous()`.

Handler-level role rule:

- Only users with the `Nurse` role may create or manage a nurse profile.
- Handlers must load the authenticated user by `ICurrentUserService.UserId` and verify a role named `Nurse` through existing `UserRole` and `Role` data.
- If `ICurrentUserService.UserId` is null, throw the existing unauthenticated exception pattern.
- If the authenticated user does not have the `Nurse` role, throw `ForbiddenAccessException`.
- Add project-wide `ForbiddenAccessException` in the Application layer using namespace `NursingPlatform.Application.Common.Exceptions` unless an existing Application exception folder/namespace pattern exists at implementation time.
- Map `ForbiddenAccessException` to HTTP 403 Problem Details in the existing WebApi exception middleware.
- Keep unauthenticated users mapped to the existing 401 behavior.

Ownership rule:

- All self-service reads and writes must be scoped by the authenticated `UserId`.
- Child record commands must query through the authenticated user's `NurseProfile` and must not update/delete records by `id` alone.
- If a child record ID exists but belongs to another nurse profile, the response must not reveal ownership. Treat it the same as not found using the existing not-found pattern.

Security response rule:

- API responses must never expose password hashes, role internals, permission internals, `NurseSkill.NormalizedName`, `NurseCvDocument.StorageKey`, storage roots, internal paths, or file URLs.

## 9. File Storage / CV Upload Rules

Application abstraction:

```csharp
public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(Stream content, string extension, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
```

Infrastructure implementation:

- Implement local file storage in Infrastructure behind `IFileStorageService`.
- Local storage root must be configurable.
- Prefer a storage root outside the repository.
- Uploaded files must not be committed.
- If a development storage path is ever placed inside the repository, it must be ignored by git.
- Store files outside the application container path using configurable settings.
- Generate storage keys with `Guid.NewGuid()` and the validated extension.
- Do not use the client filename to generate the storage key.
- Create directories as needed.
- Delete should be idempotent for missing physical files.

CV upload behavior:

1. Validate the current user is authenticated and has the `Nurse` role.
2. Ensure a nurse profile exists before upload. If no profile exists, return not found using the existing not-found pattern.
3. Validate file size, extension, and content type.
4. Sanitize client filename for `OriginalFileName` metadata only.
5. Store file through `IFileStorageService`.
6. Persist `NurseCvDocument` metadata with internal `StorageKey`.
7. If replacing an existing CV, delete the old stored file after the new file is saved and metadata is updated.
8. Return `NurseCvDocumentDto` only.

## 10. Testing Requirements

Testing must follow the existing project patterns and use TDD where practical.

Domain tests:

- Entity defaults and simple property behavior.
- Skill normalization helper behavior if implemented outside handlers.

Application tests:

- Non-authenticated current user is rejected by handlers.
- Authenticated non-Nurse user throws `ForbiddenAccessException`.
- Nurse profile upsert creates a new profile when none exists.
- Nurse profile upsert updates the authenticated nurse's existing profile.
- Self read returns only the authenticated nurse profile.
- Experience, education, and certificate create/update/delete are scoped by authenticated nurse profile.
- Child update/delete treats records owned by another profile as not found.
- Language updates reject inactive/missing languages and duplicates.
- Skill updates normalize names and reject normalized duplicates.
- CV upload validates file type and size, stores metadata, and does not expose storage key.
- CV delete calls storage delete and removes metadata.

WebApi integration tests:

- Every endpoint returns 401 without JWT.
- Valid authenticated Nurse JWT succeeds for valid requests.
- Authenticated non-Nurse requests return 403 Problem Details.
- Raw JSON response checks confirm sensitive/internal fields are not exposed.
- Multipart CV upload rejects unsupported file type and oversized file.
- DELETE endpoints return 204 for owned records.

Verification commands for implementation tasks:

```bash
dotnet build backend/NursingPlatform.slnx
dotnet test backend/NursingPlatform.slnx
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short
git diff --cached --stat
```

## 11. EF Migration Considerations

Migration name:

```text
AddNurseModule
```

Expected tables:

- `NurseProfiles`
- `NurseExperiences`
- `NurseEducation`
- `NurseCertificates`
- `NurseLanguages`
- `NurseSkills`
- `NurseCvDocuments`

Expected relationship rules:

- `Users` to `NurseProfiles`: Restrict delete.
- `NurseProfiles` to owned child records: Cascade delete.
- `Countries` to nurse profile/experience/education rows: Restrict delete.
- `Languages` to `NurseLanguages`: Restrict delete.

Expected indexes:

- Unique index on `NurseProfiles.UserId`.
- Index on `NurseProfiles.CurrentCountryId`.
- Index on `NurseProfiles.LicenseCountryId`.
- Index on child `NurseProfileId` foreign keys.
- Unique index on `NurseLanguages(NurseProfileId, LanguageId)`.
- Unique index on `NurseSkills(NurseProfileId, NormalizedName)`.
- Unique index on `NurseCvDocuments.NurseProfileId` to enforce one CV per profile.

Application DbContext changes:

- Add `DbSet<Country> Countries` and `DbSet<Language> Languages` to `IApplicationDbContext` because handlers must validate active reference data.
- Add nurse module `DbSet` properties to both `IApplicationDbContext` and `ApplicationDbContext`.

No production seed data is required for Phase 5. Existing `Languages` and `Countries` seed data are reused.

## 12. Risks and Reviewer Checkpoints

### Checkpoint 1: Resolved Role / Forbidden Exception Strategy

The approved strategy is:

- Add a project-wide `ForbiddenAccessException` in the Application layer using namespace `NursingPlatform.Application.Common.Exceptions` unless an existing Application exception folder/namespace pattern exists at implementation time.
- Map `ForbiddenAccessException` to HTTP 403 Problem Details in the existing WebApi exception middleware.
- Use `ForbiddenAccessException` when the user is authenticated but does not have the `Nurse` role.
- Keep unauthenticated users mapped to the existing 401 behavior.
- Child record ownership violations still use the existing not-found pattern so ownership is not leaked.

### Checkpoint 2: CV Replacement Atomicity

File storage and database persistence cannot be fully atomic with local file storage. Recommended sequence is store new file, update DB metadata, then delete old file. If DB save fails after storage succeeds, implementation should best-effort delete the newly stored file. This behavior should be reviewed during implementation.

### Checkpoint 3: MIME Validation Strength

Phase 5 validates content type and extension. Deep file signature scanning and malware scanning are out of scope unless explicitly added later.

### Checkpoint 4: Profile Field Completeness

Phase 5 intentionally avoids additional PII fields such as date of birth, phone, full address, and nationality. Add them only after explicit reviewer approval.

### Checkpoint 5: Skills as Free Text

Skills are nurse-owned free-text rows in Phase 5. A global skill taxonomy can be introduced in a later administration/recruitment phase if needed.
