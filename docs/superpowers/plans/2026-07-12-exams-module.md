# Phase 7A Exam Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Phase 7A Exam Foundation: a secure nurse-facing mock exam flow from catalog to timed session, answer submission, scoring, result review, and attempt history without real payments.

**Architecture:** Exams are a new business module with Domain entities for catalog/content/session state, Application CQRS handlers for access, ownership, timer, scoring, and DTO projection, Infrastructure EF configuration/migration/indexes, and thin WebApi endpoint mappings. Phase 7A uses existing Country reference data, existing Nurse role guard patterns, and an entitlement-ready `ExamAccessGrant` model while deferring payment processors and full admin content management.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 7A Exam Foundation.
- Implement the approved spec exactly: `docs/superpowers/specs/2026-07-12-exams-module.md`.
- Do not implement full Phase 7 analytics dashboards.
- Do not add frontend.
- Do not add real payment processing, checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- Do not modify recruitment, contact requests, candidate search, employers, or nurse profile behavior except for reading nurse profile ownership through existing guards.
- Do not expose correct answers, `IsCorrect`, explanations, score, or pass/fail state before session completion.
- In-progress session DTOs may expose only the nurse's own saved selected answer option id per session question.
- Do not serialize EF entities, domain entities, navigation objects, account internals, roles, permissions, tokens, password hashes, payment provider ids, or payment state in DTOs.
- Use `.RequireAuthorization()` for Phase 7A nurse-facing endpoints; Application handlers enforce Nurse role and ownership.
- Use existing permissions only for future/admin boundaries. Do not add broad admin content endpoints unless a later approved task explicitly requires them.
- Phase 7A uses deterministic question and option order snapshots.
- Phase 7A supports single-best-answer multiple-choice questions only.
- Multiple attempts are allowed, but only one in-progress session per nurse per exam version is allowed.
- Enforce one `InProgress` session per `NurseProfileId + ExamVersionId` with a PostgreSQL/EF-compatible unique filtered index where `Status == 'InProgress'`; stop and report if this cannot be implemented cleanly.
- Starting a session requires valid published content: at least one active `SingleBestAnswer` question, at least two active options per question, exactly one correct active option per question, and positive points.
- `SaveExamSessionAnswers` is a partial upsert: provided answers are created/updated and omitted existing answers remain unchanged.
- Access without payments: published free exams are startable; non-free exams require an unexpired `ExamAccessGrant`.
- No source code, tests, migrations, tracking docs, staging, or commits happen until execution is explicitly approved.

---

## Planned File Structure

Domain:

- Create `backend/src/NursingPlatform.Domain/Exams/ExamStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamVersionStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSessionStatus.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamQuestionType.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamCategory.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/Exam.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamVersion.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamQuestion.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamAnswerOption.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamAccessGrant.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSession.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSessionQuestion.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSessionAnswerOption.cs`.
- Create `backend/src/NursingPlatform.Domain/Exams/ExamSessionAnswer.cs`.

Application:

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose exam DbSets and transition helper only if needed for atomic submit/finalize.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamCatalogItemDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamDetailDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionQuestionDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionAnswerOptionDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionResultDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamSessionReviewDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/DTOs/ExamAttemptDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Common/ExamMapping.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Common/ExamScoringService.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Commands/SaveExamSessionAnswers/SaveExamSessionAnswersRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Commands/SaveExamSessionAnswers/SaveExamSessionAnswerItemRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Queries/ListExams/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Queries/GetExam/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Commands/StartExamSession/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Queries/GetExamSession/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Commands/SaveExamSessionAnswers/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Commands/SubmitExamSession/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Queries/GetExamSessionResult/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Queries/GetExamSessionReview/*`.
- Create `backend/src/NursingPlatform.Application/Exams/Queries/ListMyExamAttempts/*`.

Infrastructure:

- Create EF configurations for all exam entities under `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Generate `AddExamFoundation` EF migration and update model snapshot through EF only.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` to map Phase 7A nurse-facing endpoints only.

Tests:

- Create `backend/tests/NursingPlatform.Domain.Tests/Exams/ExamEntityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Exams/ExamDtoSecurityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Exams/ExamValidatorTests.cs`.
- Create handler tests under `backend/tests/NursingPlatform.Application.Tests/Exams/`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ExamConfigurationTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ExamEndpointsTests.cs`.

Tracking:

- Modify `CURRENT_TASK.md` and `TASKS.md` only in Task 6 after final verification approval.

---

### Task 1: Domain and Application Contracts

**Goal:** Add exam domain types, safe DTO contracts, request/response contracts, validators, mapping shape, and DTO security tests without EF configuration, handlers, or WebApi endpoints.

**Files:**
- Create Domain files listed in Planned File Structure.
- Create Application DTO, command/query, validator, and mapping files listed above.
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`.
- Test `backend/tests/NursingPlatform.Domain.Tests/Exams/ExamEntityTests.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/ExamDtoSecurityTests.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/ExamValidatorTests.cs`.

**Tests to write first:**
- `Exam_DefaultStatus_IsDraft`
- `ExamVersion_DefaultStatus_IsDraft`
- `ExamSession_DefaultStatus_IsInProgress`
- `ExamSession_TerminalStatusHelpers_IdentifySubmittedExpiredAndAbandoned`
- `ExamSession_CalculatesExpiresAtFromStartedAtAndDuration`
- `ExamQuestion_SupportsOnlySingleBestAnswerInPhase7A`
- `ExamSessionDto_ShouldNotExposeCorrectAnswersExplanationsOrScoringBeforeCompletion`
- `ExamSessionDto_MayExposeOwnSelectedAnswerOptionIdForInProgressResume`
- `ExamSessionReviewDto_ShouldNotExposeAccountInternals`
- `Validate_ListExams_WithInvalidPagination_ShouldHaveError`
- `Validate_StartExamSession_WithEmptyExamId_ShouldHaveError`
- `Validate_SaveExamSessionAnswers_WithEmptyAnswers_ShouldHaveError`
- `Validate_SaveExamSessionAnswers_WithDuplicateExamSessionQuestionId_ShouldHaveError`
- `Validate_SaveExamSessionAnswers_WithEmptyGuidValues_ShouldHaveError`
- `Validate_SubmitExamSession_WithEmptyId_ShouldHaveError`

**Required implementation notes:**
- Status enum values:
  - `ExamStatus`: `Draft`, `Published`, `Archived`.
  - `ExamVersionStatus`: `Draft`, `Published`, `Retired`.
  - `ExamSessionStatus`: `InProgress`, `Submitted`, `Expired`, `Abandoned`.
  - `ExamQuestionType`: `SingleBestAnswer`.
- All aggregate roots inherit `AuditableEntity`.
- Use `Guid Id` for all entities.
- Do not add payment provider ids, checkout state, order ids, or payment status fields.
- Pre-completion session DTO includes `Id`, `ExamId`, `ExamTitle`, `Status`, `StartedAt`, `ExpiresAt`, `RemainingSeconds`, questions/options without correctness or explanations, and the nurse's own saved `SelectedExamSessionAnswerOptionId` per session question.
- `SaveExamSessionAnswersRequest` contains `Answers`.
- Each answer item contains `ExamSessionQuestionId` and `SelectedExamSessionAnswerOptionId`.
- Review DTO is separate from session DTO and may include correct answer/explanation only after completion.
- DTO security reflection tests must reject public properties named `UserId`, `Email`, `PasswordHash`, `Roles`, `Permissions`, `AccessToken`, `RefreshToken`, `TokenHash`, `IsCorrect`, `CorrectAnswerOptionId`, `Explanation`, `Score`, `Percentage`, `Passed`, `PaymentProviderId`, `PaymentIntentId`, `OrderId`, `User`, `NurseProfile`, `Exam`, `ExamVersion`, or `Questions` where they do not belong.
- Validators enforce pagination `Page >= 1`, `PageSize >= 1`, `PageSize <= 100`, non-empty route ids, non-empty answer sets, no duplicate `ExamSessionQuestionId` values, and no empty Guid values.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "Exam"
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ExamDto|ExamValidator|ExamSession"
dotnet build backend/NursingPlatform.slnx
git status --short --untracked-files=all
git diff --cached --stat
```

**Stop condition:** Stop for review after Task 1 tests and build pass if not in batch execution. Do not create EF configuration, migration, handlers, or endpoints in Task 1.

---

### Task 2: Infrastructure EF Configuration and Migration

**Goal:** Persist Phase 7A exam entities with correct relationships, enum storage, constraints, indexes, and migration.

**Files:**
- Create EF configuration files for all exam entities.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create generated migration `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/*_AddExamFoundation.cs`.
- Create generated migration designer.
- Modify model snapshot through EF generation only.
- Test `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ExamConfigurationTests.cs`.

**Tests to write first:**
- `ExamConfiguration_UsesExpectedTableNamesAndPrimaryKeys`
- `ExamConfiguration_StoresStatusesAsStringsWithMaxLength`
- `ExamConfiguration_ConfiguresCatalogIndexes`
- `ExamConfiguration_ConfiguresVersionAndQuestionOrderIndexes`
- `ExamConfiguration_ConfiguresSessionOwnershipAndAttemptIndexes`
- `ExamConfiguration_ConfiguresAnswerUniqueness`
- `ExamConfiguration_ConfiguresUniqueFilteredInProgressSessionIndex`
- `ExamConfiguration_ConfiguresRestrictDeleteForHistoricalIntegrity`

**Required implementation notes:**
- Store statuses as strings with max length 32.
- Configure max lengths:
  - titles/names: 200.
  - slugs: 160.
  - descriptions/instructions/explanations/question text: use bounded lengths; choose explicit values and test them.
  - option text: bounded and tested.
- Restrict deletes from Country, NurseProfile, Exam, ExamVersion, questions, and options when historical sessions exist.
- Use unique indexes:
  - `ExamCategories(CountryId, Slug)`.
  - `Exams(Slug)`.
  - `ExamVersions(ExamId, VersionNumber)`.
  - `ExamSessionAnswers(ExamSessionQuestionId)`.
- Add query indexes from the spec for catalog, grants, sessions, questions, options, and attempt history.
- Configure a unique filtered index on `ExamSessions(NurseProfileId, ExamVersionId)` where `Status == 'InProgress'`.
- If EF/PostgreSQL filtered unique index support cannot be implemented cleanly, stop and report instead of falling back to application-only duplicate prevention.
- Migration name: `AddExamFoundation`.
- Generate migration with EF only.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "ExamConfiguration"
dotnet build backend/NursingPlatform.slnx
dotnet ef migrations add AddExamFoundation --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short --untracked-files=all
git diff --cached --stat
```

**Stop condition:** Stop for review after EF model consistency is proven if not in batch execution. Do not implement Application handlers or WebApi endpoints in Task 2.

---

### Task 3: Application Handlers and Business Behavior

**Goal:** Implement catalog, start/resume, get session, save answers, submit, expiry finalization, result, review, and attempt history behavior.

**Files:**
- Create or modify handler files under `backend/src/NursingPlatform.Application/Exams/`.
- Create Application tests under `backend/tests/NursingPlatform.Application.Tests/Exams/`.

**Tests to write first:**
- `Handle_ListExams_ReturnsOnlyPublishedExamsWithPublishedVersions`
- `Handle_ListExams_ExcludesUnavailableNonFreeExamsWithoutGrant`
- `Handle_GetExam_ReturnsSafeMetadataOnly`
- `Handle_Start_WhenNurseProfileMissing_ThrowsForbiddenAccessException`
- `Handle_Start_WithFreePublishedExam_CreatesInProgressSnapshot`
- `Handle_Start_WithInvalidPublishedContent_ThrowsInvalidOperationException`
- `Handle_Start_WithNonFreeExamWithoutGrant_ThrowsForbiddenAccessException`
- `Handle_Start_WithExistingInProgressSession_ReturnsExistingSession`
- `Handle_Start_WithExpiredExistingInProgressSession_FinalizesExpiredAndCreatesNewSession`
- `Handle_GetSession_WhenOwnedInProgress_ReturnsQuestionsWithSelectedAnswerButWithoutCorrectAnswersExplanationsOrScoring`
- `Handle_GetSession_WhenExpiredByTime_FinalizesExpiredBeforeReturning`
- `Handle_SaveAnswers_ValidatesQuestionAndOptionBelongToSession`
- `Handle_SaveAnswers_PartialUpsert_LeavesOmittedExistingAnswersUnchanged`
- `Handle_SaveAnswers_WithDuplicateExamSessionQuestionId_ThrowsValidationException`
- `Handle_SaveAnswers_WhenExpired_FinalizesAndThrowsConflict`
- `Handle_Submit_WhenInProgress_ScoresImmediatelyAndReturnsResult`
- `Handle_Submit_WhenExpired_FinalizesAndReturnsExpiredResultWhenThisRequestWins`
- `Handle_Submit_WhenAlreadyTerminal_ThrowsInvalidOperationException`
- `Handle_GetResult_WhenOwnedCompleted_ReturnsScoreSummary`
- `Handle_GetReview_WhenInProgress_ThrowsInvalidOperationException`
- `Handle_GetReview_WhenCompleted_ReturnsCorrectAnswersAndExplanations`
- `Handle_ListAttempts_PaginatesOwnAttemptsAndProvesSkipAndTake`

**Required implementation notes:**
- Reuse `NurseRoleGuard.EnsureCurrentUserIsNurseAsync()`.
- Resolve the current `NurseProfile.Id` for every nurse workflow; missing profile throws `ForbiddenAccessException`.
- Non-owned sessions return `KeyNotFoundException`.
- Draft/archived/retired or inaccessible exams behave as not found or forbidden according to the spec.
- Start session:
  - find published exam and published current version.
  - enforce free/grant access.
  - validate published content before creating a session: at least one active `SingleBestAnswer` question, at least two active options per included question, exactly one correct active option per included question, and positive points.
  - return existing own `InProgress` session for the same exam version if present and not expired.
  - if an existing own `InProgress` session for the same exam version is expired by server time, finalize it as `Expired`, score saved answers, then create a new `InProgress` session.
  - expired `StartExamSession` resume attempts must finalize before creating a replacement session.
  - create deterministic snapshots ordered by `DisplayOrder`, `Id`.
  - set `StartedAt = DateTime.UtcNow`, `ExpiresAt = StartedAt + DurationMinutes`.
- Save answers:
  - reject terminal sessions.
  - finalize expired sessions before accepting changes.
  - partial upsert only the provided answers.
  - leave omitted existing answers unchanged.
  - reject duplicate `ExamSessionQuestionId` values and empty Guid values.
  - require every supplied question and option to belong to the same owned session snapshot.
- Submit:
  - atomically transition `InProgress` to `Submitted`.
  - score from session snapshots.
  - store immutable result fields.
  - competing submit/expiry attempts return `409 Conflict`.
  - when already expired by server time, finalize as `Expired` and return the expired result if this request won finalization; otherwise use one consistent behavior, either `409 Conflict` or the already finalized result, and lock it with tests.
- Expiry:
  - if now >= `ExpiresAt`, transition to `Expired` and score saved answers only.
  - do not trust client time.
- Get session:
  - owned `InProgress` sessions return saved selected answer option ids for resume.
  - owned `InProgress` sessions never return correct answer id, `IsCorrect`, explanation, score, percentage, or passed.
  - if server time is at or after `ExpiresAt`, finalize as `Expired` before returning.
- Review:
  - only for `Submitted` or `Expired`.
  - includes explanations and correct answers from snapshots.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.Application.Tests --filter "Exam"
dotnet build backend/NursingPlatform.slnx
git status --short --untracked-files=all
git diff --cached --stat
```

**Stop condition:** Stop for review after Application tests and build pass if not in batch execution. Do not add WebApi endpoints or tracking updates in Task 3.

---

### Task 4: WebApi Endpoints and Integration Tests

**Goal:** Add only the approved Phase 7A nurse-facing endpoints and prove auth, validation, ownership, conflict, binding, and raw JSON security.

**Files:**
- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ExamEndpointsTests.cs`.

**Endpoints to add:**
- `GET /api/v1/exams` with `.RequireAuthorization()` and `.WithName("ListExams")`.
- `GET /api/v1/exams/{id:guid}` with `.RequireAuthorization()` and `.WithName("GetExam")`.
- `POST /api/v1/exams/{id:guid}/sessions` with `.RequireAuthorization()` and `.WithName("StartExamSession")`.
- `GET /api/v1/exam-sessions/{id:guid}` with `.RequireAuthorization()` and `.WithName("GetExamSession")`.
- `PUT /api/v1/exam-sessions/{id:guid}/answers` with `.RequireAuthorization()` and `.WithName("SaveExamSessionAnswers")`.
- `POST /api/v1/exam-sessions/{id:guid}/submit` with `.RequireAuthorization()` and `.WithName("SubmitExamSession")`.
- `GET /api/v1/exam-sessions/{id:guid}/result` with `.RequireAuthorization()` and `.WithName("GetExamSessionResult")`.
- `GET /api/v1/exam-sessions/{id:guid}/review` with `.RequireAuthorization()` and `.WithName("GetExamSessionReview")`.
- `GET /api/v1/me/nurse-profile/exam-attempts` with `.RequireAuthorization()` and `.WithName("ListMyExamAttempts")`.

**Tests to write first:**
- `ExamEndpoints_WithoutJwt_ReturnUnauthorized`
- `ListExams_SendsQueryWithPaginationAndFilters`
- `ExamRoute_WithInvalidGuid_ReturnsBadRequestAndDoesNotSend`
- `StartExamSession_WithForbiddenAccess_ReturnsForbidden`
- `GetExamSession_WhenHidden_ReturnsNotFound`
- `SaveAnswers_WithValidationFailure_ReturnsValidationProblemDetails`
- `SubmitExamSession_WithInvalidTransition_ReturnsConflict`
- `GetExamSession_ReturnsInProgressJsonWithSelectedAnswerOptionId`
- `GetExamSession_ReturnsInProgressJsonWithoutCorrectAnswersExplanationsOrScore`
- `GetExamSessionReview_ReturnsCompletedJsonWithExplanationsOnlyAfterCompletion`
- `ListMyExamAttempts_ReturnsPaginatedSafeJson`
- `ExamEndpoints_UseRequireAuthorizationOnly_WithoutPermissionSetup`

**Required implementation notes:**
- Keep endpoints thin and delegate to MediatR.
- Do not add anonymous endpoints.
- Do not add payment endpoints.
- Do not add admin endpoints unless later approved.
- Raw JSON tests must inspect response text before deserialization.
- In-progress session JSON may include `"selectedExamSessionAnswerOptionId"`.
- Forbidden pre-completion patterns include `"isCorrect"`, `"correctAnswerOptionId"`, `"explanation"`, `"score"`, `"percentage"`, and `"passed"`.
- Global forbidden patterns include `"passwordHash"`, `"roles"`, `"permissions"`, `"accessToken"`, `"refreshToken"`, `"tokenHash"`, `"paymentProviderId"`, `"paymentIntentId"`, `"orderId"`, `"user"`, `"nurseProfile"`, `"examVersion"`, and EF navigation names.

**Verification commands:**
```bash
dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "Exam"
dotnet build backend/NursingPlatform.slnx
git status --short --untracked-files=all
git diff --cached --stat
```

**Stop condition:** Stop for review after focused WebApi tests and build pass if not in batch execution. Do not proceed to tracking updates until final verification is approved.

---

### Task 5: Final Verification and Documentation/Index Review

**Goal:** Verify Phase 7A end-to-end and confirm no out-of-scope behavior or unsafe DTO exposure was introduced.

**Files:**
- No file modifications expected unless verification exposes an approved documentation or index correction.

**Required implementation notes:**
- Run the full backend build.
- Run the full solution test suite.
- Run EF pending model check.
- Review `docs/api/api-design.md` for endpoint and Problem Details consistency.
- Review `docs/database/database-design.md` and EF configuration for index scope.
- Confirm no source response DTO exposes correct answers before completion.
- Confirm raw JSON WebApi tests inspect response text before DTO deserialization.
- Confirm no payment, frontend, recruitment, admin UI, import pipeline, analytics dashboard, or notification work was added.

**Verification commands:**
```bash
dotnet build backend/NursingPlatform.slnx
dotnet test backend/NursingPlatform.slnx
dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext
git status --short --untracked-files=all
git diff --cached --stat
git diff --stat
```

**Stop condition:** Stop for review after final verification evidence is reported if not in batch execution. Do not update tracking docs, stage, or commit until approved.

---

### Task 6: Tracking Documentation Update

**Goal:** Update project tracking only after Phase 7A implementation and final verification are approved.

**Files:**
- Modify `CURRENT_TASK.md`.
- Modify `TASKS.md`.

**Required implementation notes:**
- Mark only the implemented Phase 7A Exam Foundation work complete.
- Do not mark full Phase 7 analytics complete.
- Do not start Phase 8 payments.
- Do not add frontend, payment, recruitment, notification, or admin UI details.

**Verification commands:**
```bash
git diff -- CURRENT_TASK.md TASKS.md
git status --short --untracked-files=all
git diff --cached --stat
```

**Stop condition:** Stop for review after tracking diff and git status are reported if not in batch execution. Do not stage or commit.

---

### Task 7: Final Phase 7A Commit

**Goal:** Commit approved Phase 7A spec, plan, implementation, tests, migration, and tracking changes only after explicit reviewer approval.

**Files:**
- Stage only approved Phase 7A files explicitly.
- Include approved spec and plan.
- Include approved Phase 7A implementation, tests, migration, and tracking files.
- Do not stage `codex_res/codex_report.md`.

**Required implementation notes:**
- Inspect status and staged diff before committing.
- Use explicit `git add <path>` commands only.
- Never use `git add .`.
- Do not stage `AGENTS.md`, `PROJECT_RULES.md`, `.gitignore`, unrelated docs, report files, frontend files, payment files, recruitment files, generated artifacts outside approved migration files, or unapproved migrations.
- Commit message should be concise and scoped, for example `feat: add exam foundation`.

**Verification commands:**
```bash
git status --short --untracked-files=all
git diff --cached --name-only
git diff --cached --stat
git commit -m "feat: add exam foundation"
git status --short --untracked-files=all
git log -1 --oneline
```

**Stop condition:** Stop after reporting staged file list, staged diff stat, commit hash/message, post-commit status, and latest log line. Do not proceed to Phase 7B, payments, frontend, analytics dashboards, imports, notifications, or admin UI.

---

## Self-Review Checklist

- Spec coverage: Tasks cover domain/contracts, persistence/migration, Application behavior, WebApi endpoints, final verification, tracking, and commit.
- Scope check: The plan chooses Phase 7A, not full Phase 7.
- Payment check: No real payments are implemented; access uses free/mock and grant-based entitlement.
- Security check: Correct answers and explanations are hidden before completion and raw JSON tests are required.
- Lifecycle check: Exam, version, and session statuses match the spec.
- Placeholder scan: No unresolved placeholders are intentionally left in this plan.
