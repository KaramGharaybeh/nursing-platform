# Phase 5 — Nurse Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Profile Foundation self-service nurse module: nurse profile, experience, education, certificate metadata, languages, free-text skills, and CV upload metadata.

**Architecture:** Use existing Clean Architecture and CQRS patterns. Domain entities live under `NursingPlatform.Domain.Nurses`, use cases live under `NursingPlatform.Application.Nurses`, EF Core configuration and local file storage live in Infrastructure, and Minimal API endpoint mappings remain thin in WebApi.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq

**Spec:** `docs/superpowers/specs/2026-07-09-nurse-module.md`

## Global Constraints

- Planning document only until implementation is explicitly approved.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.
- Do not stage or commit unless explicitly instructed.
- All Phase 5 self-service endpoints use `.RequireAuthorization()`.
- Only users with the `Nurse` role may create or manage a nurse profile.
- Enforce role and ownership in Application handlers using existing project patterns.
- Add project-wide `ForbiddenAccessException` in the Application layer using namespace `NursingPlatform.Application.Common.Exceptions` unless an existing Application exception folder/namespace pattern exists at implementation time.
- Map `ForbiddenAccessException` to HTTP 403 Problem Details in the existing WebApi exception middleware.
- Use `ForbiddenAccessException` when the user is authenticated but does not have the `Nurse` role.
- Keep unauthenticated users mapped to the existing 401 behavior.
- Child record ownership violations use the existing not-found pattern so ownership is not leaked.
- Skills are nurse-owned free-text rows, not a global reference table.
- Store normalized skill names for duplicate prevention per nurse; do not expose normalized names in API responses.
- Languages use existing `Language` reference data.
- CV upload supports PDF/DOC/DOCX only, with max file size 5 MB.
- Never trust client filenames for storage location.
- Store generated internal storage keys; never expose storage keys or internal paths through the API.
- CV download and certificate file attachments are out of scope.
- Child record deletion is hard delete.
- User to `NurseProfile` delete behavior is Restrict.
- `NurseProfile` to owned child records may use Cascade.

---

## File Structure Map

Domain files:

- Create `backend/src/NursingPlatform.Domain/Nurses/NurseProfile.cs` for the nurse profile aggregate root.
- Create `backend/src/NursingPlatform.Domain/Nurses/NurseExperience.cs` for employment history.
- Create `backend/src/NursingPlatform.Domain/Nurses/NurseEducation.cs` for education history.
- Create `backend/src/NursingPlatform.Domain/Nurses/NurseCertificate.cs` for certificate metadata.
- Create `backend/src/NursingPlatform.Domain/Nurses/NurseLanguage.cs` for nurse-language selections.
- Create `backend/src/NursingPlatform.Domain/Nurses/NurseSkill.cs` for free-text skills.
- Create `backend/src/NursingPlatform.Domain/Nurses/NurseCvDocument.cs` for CV file metadata.

Application files:

- Create `backend/src/NursingPlatform.Application/Nurses/DTOs/*` for response DTOs.
- Create `backend/src/NursingPlatform.Application/Common/Exceptions/ForbiddenAccessException.cs` for project-wide forbidden errors.
- Create `backend/src/NursingPlatform.Application/Nurses/Common/NurseRoleGuard.cs` for authenticated Nurse role enforcement.
- Create commands and queries under `backend/src/NursingPlatform.Application/Nurses/Commands/*` and `backend/src/NursingPlatform.Application/Nurses/Queries/*` following existing feature-folder conventions.
- Create `backend/src/NursingPlatform.Application/Abstractions/Storage/IFileStorageService.cs` and storage DTOs for CV upload.
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose reference data and nurse module DbSets.

Infrastructure files:

- Create EF configurations under `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/*`.
- Create `backend/src/NursingPlatform.Infrastructure/Storage/LocalFileStorageService.cs`.
- Create `backend/src/NursingPlatform.Infrastructure/Configuration/FileStorageSettings.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs`.
- Modify `backend/src/NursingPlatform.WebApi/appsettings.json` and `backend/src/NursingPlatform.WebApi/appsettings.Development.json` for file storage settings.

WebApi files:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` to map Phase 5 endpoints only.

Test files:

- Create Domain tests under `backend/tests/NursingPlatform.Domain.Tests/Nurses/*`.
- Create Application tests under `backend/tests/NursingPlatform.Application.Tests/Nurses/*`.
- Create WebApi integration tests under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/*Nurse*Tests.cs`.

---

### Task 1: Forbidden Exception Foundation

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Common/Exceptions/ForbiddenAccessException.cs`
- Modify: `backend/src/NursingPlatform.WebApi/Middleware/ExceptionMiddleware.cs`
- Test: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ExceptionMiddlewareTests.cs` if an existing middleware test location exists; otherwise cover 403 behavior through the first Phase 5 non-Nurse endpoint integration test in Task 9.

**Tests to write first:**

- `ForbiddenAccessException_Returns403ProblemDetails`
- If middleware tests do not exist, defer this assertion to Task 9 as `PutProfile_WithAuthenticatedNonNurse_ReturnsForbidden`.

**Steps:**

- [ ] Create project-wide `ForbiddenAccessException` in the Application layer using namespace `NursingPlatform.Application.Common.Exceptions` unless an existing Application exception folder/namespace pattern exists at implementation time.
- [ ] Map `ForbiddenAccessException` to HTTP 403 Problem Details in `ExceptionMiddleware`.
- [ ] Keep `UnauthorizedAccessException` mapped to HTTP 401.
- [ ] Do not change existing validation, not-found, conflict, or unexpected-error mappings.
- [ ] Add or defer the 403 test according to the available WebApi test pattern.

**Verification commands:**

```bash
dotnet build backend/NursingPlatform.slnx
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "Forbidden|Nurse"
git status --short
git diff --cached --stat
```

Expected: build has 0 errors and 0 warnings, relevant WebApi tests pass, no staged files, no unrelated modifications.

**Stop condition:** Stop for review after the forbidden exception foundation is verified. Do not proceed to Task 2. Do not commit until explicitly approved.

---

### Task 2: Domain Entities, EF Configurations, DbContext, and Migration

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseProfile.cs`
- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseExperience.cs`
- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseEducation.cs`
- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseCertificate.cs`
- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseLanguage.cs`
- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseSkill.cs`
- Create: `backend/src/NursingPlatform.Domain/Nurses/NurseCvDocument.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseProfileConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseExperienceConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseEducationConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseCertificateConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseLanguageConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseSkillConfiguration.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/NurseCvDocumentConfiguration.cs`
- Create: generated EF migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` named `AddNurseModule`
- Modify: `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- Test: `backend/tests/NursingPlatform.Domain.Tests/Nurses/NurseEntitiesTests.cs`

**Tests to write first:**

- `NurseSkill_NormalizedName_IsStoredSeparatelyFromName`
- `NurseCvDocument_StorageKey_IsSeparateFromOriginalFileName`
- `NurseProfile_DefaultRecruitmentVisibility_IsFalse`

**Implementation requirements:**

- Add all entities from the spec exactly.
- `NurseProfile.UserId` unique index.
- `User` to `NurseProfile` uses `DeleteBehavior.Restrict`.
- `NurseProfile` to owned child records uses `DeleteBehavior.Cascade`.
- `Country` and `Language` relationships use `DeleteBehavior.Restrict`.
- Add `DbSet<Country> Countries` and `DbSet<Language> Languages` to `IApplicationDbContext` because handlers must validate active reference data.
- Add all nurse module DbSets to `IApplicationDbContext` and `ApplicationDbContext`.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "Nurse"
dotnet build backend/NursingPlatform.slnx
dotnet ef migrations add AddNurseModule --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short
git diff --cached --stat
```

Expected: domain nurse tests pass, build has 0 errors and 0 warnings, EF reports no pending model changes after migration generation, no staged files.

**Stop condition:** Stop for review after migration generation and verification output. Do not proceed to Task 3. Do not commit until explicitly approved.

---

### Task 3: File Storage Abstraction and Local Storage Infrastructure

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Abstractions/Storage/IFileStorageService.cs`
- Create: `backend/src/NursingPlatform.Application/Abstractions/Storage/StoredFileResult.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Configuration/FileStorageSettings.cs`
- Create: `backend/src/NursingPlatform.Infrastructure/Storage/LocalFileStorageService.cs`
- Create: `backend/tests/NursingPlatform.Infrastructure.Tests/Storage/LocalFileStorageServiceTests.cs`
- Modify: `backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs`
- Modify: `backend/src/NursingPlatform.WebApi/appsettings.json`
- Modify: `backend/src/NursingPlatform.WebApi/appsettings.Development.json`

**Tests to write first:**

- `SaveAsync_GeneratesStorageKeyWithoutClientFileName`
- `SaveAsync_WritesContentToConfiguredRoot`
- `DeleteAsync_MissingFile_DoesNotThrow`

**Implementation requirements:**

- `IFileStorageService.SaveAsync(Stream content, string extension, string contentType, CancellationToken cancellationToken = default)` returns `StoredFileResult` with `StorageKey`, `ContentType`, and `FileSizeBytes`.
- `IFileStorageService.DeleteAsync(string storageKey, CancellationToken cancellationToken = default)` deletes by internal storage key only.
- `LocalFileStorageService` stores files under configurable `FileStorageSettings.RootPath`.
- Prefer a storage root outside the repository.
- Uploaded files must not be committed.
- If a development storage path is ever placed inside the repository, add it to git ignore before using it.
- Storage keys are generated from `Guid.NewGuid()` and validated extension only.
- Do not use client filenames in storage paths.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "LocalFileStorageService"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: storage tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after storage abstraction and local implementation verification. Do not proceed to Task 4. Do not commit until explicitly approved.

---

### Task 4: Nurse Profile Self Read and Upsert Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseProfileDto.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpsertNurseProfile/UpsertNurseProfileCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpsertNurseProfile/UpsertNurseProfileCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpsertNurseProfile/UpsertNurseProfileCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpsertNurseProfile/UpsertNurseProfileRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/GetCurrentNurseProfile/GetCurrentNurseProfileQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/GetCurrentNurseProfile/GetCurrentNurseProfileQueryHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Common/NurseRoleGuard.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UpsertNurseProfileCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UpsertNurseProfileCommandValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Queries/GetCurrentNurseProfileQueryHandlerTests.cs`

**Tests to write first:**

- `Handle_NurseWithoutProfile_CreatesProfile`
- `Handle_NurseWithProfile_UpdatesProfile`
- `Handle_NonNurse_ThrowsForbiddenAccessException`
- `Handle_MissingCurrentUser_ThrowsUnauthorizedAccessException`
- `Handle_InactiveCountry_ThrowsInvalidOperationException`
- `Validate_YearsOfExperienceGreaterThan80_IsInvalid`
- `GetCurrentNurseProfile_ExistingProfile_ReturnsDtoWithoutSensitiveFields`
- `GetCurrentNurseProfile_NoProfile_ThrowsKeyNotFoundException`

**Implementation requirements:**

- Query and command handlers must load the current user via `ICurrentUserService.UserId`.
- Role enforcement must throw `ForbiddenAccessException` for authenticated users who do not have the `Nurse` role.
- Country validation must check `Countries` where `IsActive == true`.
- DTO projection must be explicit and must not expose user password, role internals, or permission internals.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "NurseProfile"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: profile application tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after profile application layer verification. Do not proceed to Task 5. Do not commit until explicitly approved.

---

### Task 5: Experience CRUD Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseExperienceDto.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/CreateNurseExperience/CreateNurseExperienceCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/CreateNurseExperience/CreateNurseExperienceCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/CreateNurseExperience/CreateNurseExperienceCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/CreateNurseExperience/UpsertNurseExperienceRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseExperience/UpdateNurseExperienceCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseExperience/UpdateNurseExperienceCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/DeleteNurseExperience/DeleteNurseExperienceCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/DeleteNurseExperience/DeleteNurseExperienceCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseExperiences/ListCurrentNurseExperiencesQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseExperiences/ListCurrentNurseExperiencesQueryHandler.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/NurseExperienceCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/NurseExperienceCommandValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Queries/ListCurrentNurseExperiencesQueryHandlerTests.cs`

**Tests to write first:**

- `Create_ValidExperience_AddsRecordForCurrentNurseProfile`
- `Create_EndDateBeforeStartDate_IsInvalid`
- `Create_CurrentExperienceWithEndDate_IsInvalid`
- `Update_RecordOwnedByAnotherProfile_ThrowsKeyNotFoundException`
- `Delete_RecordOwnedByCurrentProfile_RemovesRecord`
- `List_ReturnsRecordsSortedByStartDateDescending`

**Implementation requirements:**

- All operations must resolve the authenticated nurse profile first.
- Update and delete must query by both child `Id` and current `NurseProfileId`.
- Country validation must check active country when `CountryId` is supplied.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "NurseExperience"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: experience tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after experience application layer verification. Do not proceed to Task 6. Do not commit until explicitly approved.

---

### Task 6: Education and Certificate CRUD Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseEducationDto.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseCertificateDto.cs`
- Create: education command/query folders under `backend/src/NursingPlatform.Application/Nurses/Commands/*NurseEducation*` and `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseEducation/`
- Create: certificate command/query folders under `backend/src/NursingPlatform.Application/Nurses/Commands/*NurseCertificate*` and `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseCertificates/`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/NurseEducationCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/NurseEducationCommandValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Queries/ListCurrentNurseEducationQueryHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/NurseCertificateCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/NurseCertificateCommandValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Queries/ListCurrentNurseCertificatesQueryHandlerTests.cs`

**Tests to write first:**

- `CreateEducation_ValidRequest_AddsRecordForCurrentNurseProfile`
- `UpdateEducation_RecordOwnedByAnotherProfile_ThrowsKeyNotFoundException`
- `DeleteEducation_RecordOwnedByCurrentProfile_RemovesRecord`
- `CreateEducation_EndDateBeforeStartDate_IsInvalid`
- `CreateCertificate_ValidRequest_AddsMetadataOnlyRecord`
- `CreateCertificate_ExpirationBeforeIssueDate_IsInvalid`
- `CreateCertificate_InvalidCredentialUrl_IsInvalid`
- `DeleteCertificate_RecordOwnedByCurrentProfile_RemovesRecord`

**Implementation requirements:**

- Education and certificate records are metadata only.
- Certificate file attachments are not added.
- All child operations must scope by authenticated nurse profile.
- Education country validation must check active country when supplied.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "NurseEducation|NurseCertificate"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: education and certificate tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after education and certificate application layer verification. Do not proceed to Task 7. Do not commit until explicitly approved.

---

### Task 7: Languages and Skills Management Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseLanguageDto.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseSkillDto.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseLanguages/UpdateNurseLanguagesCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseLanguages/UpdateNurseLanguagesCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseLanguages/UpdateNurseLanguagesCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseLanguages/UpdateNurseLanguagesRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseLanguages/ListCurrentNurseLanguagesQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseLanguages/ListCurrentNurseLanguagesQueryHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseSkills/UpdateNurseSkillsCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseSkills/UpdateNurseSkillsCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseSkills/UpdateNurseSkillsCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UpdateNurseSkills/UpdateNurseSkillsRequest.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseSkills/ListCurrentNurseSkillsQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/ListCurrentNurseSkills/ListCurrentNurseSkillsQueryHandler.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UpdateNurseLanguagesCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UpdateNurseLanguagesCommandValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UpdateNurseSkillsCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UpdateNurseSkillsCommandValidatorTests.cs`

**Tests to write first:**

- `UpdateLanguages_ValidRequest_ReplacesExistingLanguages`
- `UpdateLanguages_DuplicateLanguageIds_IsInvalid`
- `UpdateLanguages_InactiveLanguage_ThrowsInvalidOperationException`
- `UpdateSkills_ValidRequest_ReplacesExistingSkills`
- `UpdateSkills_NormalizedDuplicateNames_IsInvalid`
- `UpdateSkills_StoresNormalizedNamesAndReturnsDisplayNamesOnly`
- `ListSkills_ReturnsNamesSortedAscending`

**Implementation requirements:**

- Language update uses full replacement semantics.
- Skill update uses full replacement semantics.
- Language reference rows must exist and be active.
- Skill normalized names must be generated according to the spec.
- API/application DTOs must not expose `NormalizedName`.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "NurseLanguage|NurseSkill"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: language and skill tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after language and skill application layer verification. Do not proceed to Task 8. Do not commit until explicitly approved.

---

### Task 8: CV Metadata, Upload, and Delete Application Layer

**Files expected to be created/modified:**

- Create: `backend/src/NursingPlatform.Application/Nurses/DTOs/NurseCvDocumentDto.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UploadNurseCv/UploadNurseCvCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UploadNurseCv/UploadNurseCvCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/UploadNurseCv/UploadNurseCvCommandValidator.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/DeleteNurseCv/DeleteNurseCvCommand.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Commands/DeleteNurseCv/DeleteNurseCvCommandHandler.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/GetCurrentNurseCv/GetCurrentNurseCvQuery.cs`
- Create: `backend/src/NursingPlatform.Application/Nurses/Queries/GetCurrentNurseCv/GetCurrentNurseCvQueryHandler.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UploadNurseCvCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/UploadNurseCvCommandValidatorTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Commands/DeleteNurseCvCommandHandlerTests.cs`
- Test: `backend/tests/NursingPlatform.Application.Tests/Nurses/Queries/GetCurrentNurseCvQueryHandlerTests.cs`

**Tests to write first:**

- `Upload_ValidPdf_StoresFileAndPersistsMetadata`
- `Upload_UnsupportedContentType_IsInvalid`
- `Upload_FileGreaterThanFiveMegabytes_IsInvalid`
- `Upload_ReplacesExistingCv_DeletesOldStoredFile`
- `Upload_ResponseDoesNotExposeStorageKey`
- `GetCv_ExistingCv_ReturnsMetadataOnly`
- `DeleteCv_ExistingCv_DeletesStoredFileAndMetadata`
- `DeleteCv_NoCv_ThrowsKeyNotFoundException`

**Implementation requirements:**

- CV upload command accepts stream, original filename, content type, and length from WebApi.
- Validator enforces PDF/DOC/DOCX and 5 MB max size.
- Handler stores file through `IFileStorageService` and persists metadata.
- Handler must not return `StorageKey` or internal path.
- If replacing an existing CV, delete the old stored file after new metadata is saved.
- If DB save fails after storing the new file, best-effort delete the new stored file before rethrowing.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "NurseCv|UploadNurseCv|DeleteNurseCv"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: CV application tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after CV application layer verification. Do not proceed to Task 9. Do not commit until explicitly approved.

---

### Task 9: WebApi Endpoints and Integration Tests

**Files expected to be created/modified:**

- Modify: `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseProfileEndpointTests.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseExperienceEndpointTests.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseEducationEndpointTests.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseCertificateEndpointTests.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseLanguageEndpointTests.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseSkillEndpointTests.cs`
- Create: `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/NurseCvEndpointTests.cs`

**Tests to write first:**

- `GetProfile_WithoutJwt_ReturnsUnauthorized`
- `PutProfile_WithNurseJwt_ReturnsOk`
- `PutProfile_WithAuthenticatedNonNurse_ReturnsForbidden`
- `CreateExperience_WithoutJwt_ReturnsUnauthorized`
- `DeleteExperience_WithNurseJwt_ReturnsNoContent`
- `UpdateLanguages_WithDuplicateLanguageIds_ReturnsBadRequest`
- `UpdateSkills_WithNormalizedDuplicateNames_ReturnsBadRequest`
- `UploadCv_WithUnsupportedFileType_ReturnsBadRequest`
- `UploadCv_WithOversizedFile_ReturnsBadRequest`
- `GetCv_ResponseJson_DoesNotContainStorageKeyOrInternalPath`

**Implementation requirements:**

- Add only Phase 5 self-service nurse endpoints.
- Every endpoint uses `.RequireAuthorization()` exactly.
- Do not add admin nurse endpoints.
- Do not add employer endpoints.
- CV upload endpoint uses `multipart/form-data` and maps the uploaded `IFormFile` to `UploadNurseCvCommand`.
- Endpoints must return DTOs only.

**Verification commands:**

```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "Nurse"
dotnet build backend/NursingPlatform.slnx
git status --short
git diff --cached --stat
```

Expected: Nurse WebApi integration tests pass, build has 0 errors and 0 warnings, no staged files.

**Stop condition:** Stop for review after endpoint and integration test verification. Do not proceed to Task 10. Do not commit until explicitly approved.

---

### Task 10: Final Phase 5 Verification and Documentation Review

**Files expected to be created/modified:**

- Modify: `docs/api/api-design.md` only if implementation introduces behavior that must be documented beyond existing API conventions.
- Modify: `docs/database/database-design.md` only if implementation changes database rules rather than adding normal module tables through EF Core.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.

**Tests to write first:**

- No new tests in this task. This task verifies all tests created in Tasks 2 through 9.

**Verification commands:**

```bash
dotnet build backend/NursingPlatform.slnx
dotnet test backend/NursingPlatform.slnx
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short
git diff --cached --stat
```

Expected:

- Build: 0 warnings, 0 errors.
- Tests: all pass.
- EF pending model check: no pending model changes.
- Git status: only intended Phase 5 files modified or untracked.
- Cached diff: no staged files unless reviewer explicitly approved staging.

**Stop condition:** Stop for review with full command outputs and full requested file contents. Do not proceed to Task 11 until explicitly approved. Do not commit until explicitly approved.

---

### Task 11: Update CURRENT_TASK.md and TASKS.md

**Files expected to be created/modified:**

- Modify: `CURRENT_TASK.md`
- Modify: `TASKS.md`

**Tests to write first:**

- No automated tests. This is a documentation status update after implementation and final verification are approved.

**Implementation requirements:**

- Only run this task after implementation and final verification from Task 10 are approved.
- Mark Phase 5 complete in `CURRENT_TASK.md`.
- Mark Phase 5 Nurse Module items complete in `TASKS.md`.
- Do not start Phase 6.
- Do not stage or commit.
- Paste diffs for review.

**Verification commands:**

```bash
git diff -- CURRENT_TASK.md TASKS.md
git status --short
git diff --cached --stat
```

Expected: diffs contain only intended Phase 5 completion documentation changes, no staged files.

**Stop condition:** Stop for review after pasting documentation diffs and status. Do not proceed to Task 12. Do not commit until explicitly approved.

---

### Task 12: Final Phase 5 Commit

**Files expected to be created/modified:**

- Create: none.
- Modify: none.
- Stage: intended Phase 5 files only after explicit reviewer approval.

**Tests to write first:**

- No new tests. This task commits already verified and approved work.

**Implementation requirements:**

- Only run this task after explicit reviewer approval.
- Stage intended Phase 5 files only.
- Do not use `git add .`.
- Do not stage unrelated files.
- Verify staged file list before committing.
- Commit with an approved message.
- Report post-commit status.
- Do not proceed to Phase 6.

**Verification commands:**

```bash
git diff --cached --name-only
git diff --cached --stat
git commit -m "<approved message>"
git status --short
git log -1 --oneline
```

Expected: staged file list contains only intended Phase 5 files; commit succeeds; post-commit status is clean or contains only explicitly approved unrelated local files.

**Stop condition:** Stop for review after reporting staged file list, commit hash/message, and post-commit status. Do not proceed to Phase 6.

---

## Plan Self-Review

Spec coverage:

- Objective: Task 1 through Task 10 implement the approved profile foundation.
- Approved scope: Covered by Tasks 2 through 9.
- Explicit out-of-scope list: Preserved in Global Constraints and endpoint task restrictions.
- Entity model: Task 2.
- API endpoint contracts: Task 9.
- DTO/request/response models: Tasks 4 through 9.
- Validation rules: Tasks 4 through 9.
- Authorization and ownership rules: Task 1 and Tasks 4 through 9.
- File storage/CV upload rules: Tasks 3 and 8.
- Testing requirements: Every implementation task lists tests to write first.
- EF migration considerations: Task 2 and Task 10.
- Documentation completion: Task 11.
- Final approved commit: Task 12.
- Risks/reviewer checkpoints: Task 8.

No commit occurs before Task 12. Every implementation task stops for review and explicitly says not to commit until approved.
