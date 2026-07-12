# Phase 7B Exam Admin Content Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement backend-only Phase 7B exam admin/content management APIs for categories, exams, draft versions, questions, options, validation, publish, retire, and archive workflows.

**Architecture:** Phase 7B extends the existing Exams module with admin/content Application CQRS handlers, explicit admin DTOs, validators, and thin Minimal API endpoint mappings under `/api/v1/admin`. It reuses Phase 7A domain entities and EF mappings, protects published versions and historical session snapshots, and uses existing permission authorization infrastructure.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 7B Exam Admin Content Management.
- Implement the approved spec exactly: `docs/superpowers/specs/2026-07-12-exam-admin-content-management.md`.
- Do not implement Phase 7 analytics dashboards.
- Do not implement Phase 8 payments.
- Do not add frontend.
- Do not add checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- Do not implement exam access grant management.
- Do not implement question import, bulk upload, Excel/CSV import, AI generation, or translations.
- Do not modify recruitment, contact requests, candidates, employers, or nurse profile behavior.
- Do not mutate `ExamSession`, `ExamSessionQuestion`, `ExamSessionAnswerOption`, or `ExamSessionAnswer` except read-only checks for historical protection.
- Do not expose nurse answers or attempt details in admin content endpoints.
- Do not expose user account internals, roles, permissions, tokens, password hashes, payment provider ids, or payment state.
- Do not serialize EF/domain entities or navigation objects.
- Admin endpoints may expose correct answers and explanations only to authorized admin/content users.
- Existing nurse-facing pre-completion DTOs must remain free of correct answers, `IsCorrect`, explanations, score, percentage, and passed.
- Use existing permissions only: `Exams.View`, `Exams.Create`, `Exams.Edit`, `Exams.Delete`, `Questions.View`, `Questions.Manage`.
- All Phase 7B endpoints require `.RequirePermission(...)`; no `AllowAnonymous`.
- No migration is expected unless implementation proves a schema gap.

---

## Chosen Implementation Batch Scope

Phase 7B is one backend-only batch because it is a coherent content-management slice over the Phase 7A exam schema. It includes admin APIs for category, exam, draft version, question, option, validation, publish, retire, and archive workflows. It excludes payments, grants, analytics, imports, frontend, and nurse-taking behavior changes.

## Planned File Structure

Application:

- Create `backend/src/NursingPlatform.Application/Exams/Admin/DTOs/AdminExamCategoryDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/DTOs/AdminExamDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/DTOs/AdminExamVersionDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/DTOs/AdminExamQuestionDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/DTOs/AdminExamAnswerOptionDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/DTOs/AdminExamVersionValidationDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/Common/AdminExamMapping.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/Common/AdminExamContentValidator.cs`.
- Create category commands/queries under `backend/src/NursingPlatform.Application/Exams/Admin/Categories/`.
- Create exam commands/queries under `backend/src/NursingPlatform.Application/Exams/Admin/Exams/`.
- Create version commands/queries under `backend/src/NursingPlatform.Application/Exams/Admin/Versions/`.
- Create question commands/queries under `backend/src/NursingPlatform.Application/Exams/Admin/Questions/`.
- Create option commands/queries under `backend/src/NursingPlatform.Application/Exams/Admin/AnswerOptions/`.

Infrastructure:

- No schema files expected.
- Review `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`.
- Modify EF configuration only if the approved implementation discovers a necessary schema/index gap.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` to map admin exam endpoints.

Tests:

- Create `backend/tests/NursingPlatform.Application.Tests/Exams/Admin/AdminExamDtoSecurityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Exams/Admin/AdminExamValidatorTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Exams/Admin/AdminExamContentHandlerTests.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ExamAdminConfigurationTests.cs` only if infrastructure behavior or migration is touched.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AdminExamEndpointsTests.cs`.
- Update existing nurse exam endpoint tests only if needed to prove compatibility-safe pre-completion secrecy remains intact.

Tracking:

- Modify `CURRENT_TASK.md` and `TASKS.md` only after final verification passes and reviewer approves tracking update step.

## Migration Decision

No migration is expected. Phase 7A already created the required fields for Phase 7B content management:

- Category activation via `ExamCategory.IsActive`.
- Exam status via `Exam.Status`.
- Version status via `ExamVersion.Status`.
- Question activation via `ExamQuestion.IsActive`.
- Option activation via `ExamAnswerOption.IsActive`.
- Audit fields through `AuditableEntity`.

If implementation discovers a missing required field or index, stop and report before creating a migration unless the approved execution prompt explicitly allows migration creation.

## Task 1: Application Contracts, DTOs, Validators, And DTO Security Tests

**Goal:** Add admin request/response contracts and validators without implementing business handlers or endpoints.

**Files:**

- Create admin DTO files listed in Planned File Structure.
- Create request DTOs for category, exam, question, and answer option create/update operations plus draft version create/delete commands.
- Create command/query records/classes for all approved admin operations.
- Create validators for pagination, route ids, body fields, enum values, and content ranges.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/Admin/AdminExamDtoSecurityTests.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/Admin/AdminExamValidatorTests.cs`.

**Interfaces produced:**

- `AdminExamCategoryDto`
- `AdminExamDto`
- `AdminExamVersionDto`
- `AdminExamQuestionDto`
- `AdminExamAnswerOptionDto`
- `AdminExamVersionValidationDto`
- `CreateAdminExamCategoryRequest`
- `UpdateAdminExamCategoryRequest`
- `CreateAdminExamRequest`
- `UpdateAdminExamRequest`
- `UpsertAdminExamQuestionRequest`
- `UpsertAdminExamAnswerOptionRequest`

Do not create `UpsertAdminExamVersionRequest`. Phase 7A `ExamVersion` has no real editable metadata beyond system-managed fields, so Phase 7B must not create an empty/no-op request or a no-op draft version update endpoint.

**Tests to write first:**

- `AdminExamDtos_ShouldNotExposeAccountInternalsOrPaymentFields`
- `AdminExamQuestionDto_MayExposeIsCorrectAndExplanationForAdminOnly`
- `Validate_ListAdminExamCategories_WithInvalidPagination_ShouldHaveError`
- `Validate_UpsertCategory_WithInvalidFields_ShouldHaveError`
- `Validate_UpdateCategory_WithCountryIdChangeAttempt_ShouldHaveConflictTestCoverage`
- `Validate_UpsertExam_WithInvalidDurationOrPassingScore_ShouldHaveError`
- `Validate_CreateDraftVersion_WithEmptyExamId_ShouldHaveError`
- `Validate_NoUpsertAdminExamVersionRequest_IsCreated`
- `Validate_Question_WithUnsupportedQuestionType_ShouldHaveError`
- `Validate_Question_WithNonPositivePoints_ShouldHaveError`
- `Validate_AnswerOption_WithEmptyText_ShouldHaveError`
- `Validate_AllRouteIds_WithEmptyGuid_ShouldHaveError`

**Implementation notes:**

- Use explicit DTOs only.
- Do not expose `UserId`, `Email`, `PasswordHash`, `Roles`, `Permissions`, `AccessToken`, `RefreshToken`, `TokenHash`, `PaymentProviderId`, `PaymentIntentId`, `OrderId`, `User`, `NurseProfile`, `ExamSession`, `ExamSessionAnswer`, or navigation objects.
- Admin question/option DTOs may expose `IsCorrect` and `Explanation`.
- Validators enforce `Page >= 1`, `PageSize` between `1` and `100`, non-empty GUID route values, required names/titles/slugs/text, positive duration, passing score from `0` to `100`, positive question points, and `SingleBestAnswer` only.
- Category `CountryId` is immutable after creation. Update handlers must return `409 Conflict` if a request/command attempts to change `CountryId`.
- Exam `CountryId`, `ExamCategoryId`, `DurationMinutes`, and `PassingScorePercentage` are structural/scoring fields. Update validators/handlers must allow them only while the exam has no published versions, no retired versions, and no sessions.
- Exam safe display/access fields are `Title`, `Slug`, `Description`, `Instructions`, and `IsFree`; they may remain editable on non-archived exams when validation and uniqueness rules pass.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "AdminExamDto|AdminExamValidator"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after Task 1 if not in approved batch execution. Do not add handlers or endpoints in this task.

## Task 2: Domain/Application Content Behavior And Tests

**Goal:** Implement Application handlers for admin category, exam, version, question, option, validation, publish, retire, archive, restore, and safe delete behavior.

**Files:**

- Create or modify handler files under `backend/src/NursingPlatform.Application/Exams/Admin/`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/Common/AdminExamMapping.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Admin/Common/AdminExamContentValidator.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/Admin/AdminExamContentHandlerTests.cs`.

**Tests to write first:**

- `Handle_CreateCategory_CreatesActiveCategory`
- `Handle_UpdateCategory_WhenReferencedBySameCountry_UpdatesSafeFields`
- `Handle_UpdateCategory_WhenCountryIdChanges_ThrowsInvalidOperationException`
- `Handle_DeleteCategory_WhenReferencedByExam_ThrowsInvalidOperationException`
- `Handle_ArchiveCategory_SetsIsActiveFalse`
- `Handle_CreateExam_CreatesDraftExam`
- `Handle_UpdateExam_WhenArchived_ThrowsInvalidOperationException`
- `Handle_UpdateExam_WithStructuralOrScoringFieldChangeAndPublishedVersion_ThrowsInvalidOperationException`
- `Handle_UpdateExam_WithStructuralOrScoringFieldChangeAndSession_ThrowsInvalidOperationException`
- `Handle_UpdateExam_WithSafeDisplayFieldChangesAndPublishedVersion_UpdatesFields`
- `Handle_UpdateExam_WithCategoryCountryMismatch_ThrowsInvalidOperationException`
- `Handle_DeleteExam_WithVersionsOrSessions_ThrowsInvalidOperationException`
- `Handle_ArchiveExam_SetsStatusArchived`
- `Handle_CreateDraftVersion_AssignsNextVersionNumber`
- `Handle_DraftVersionUpdateEndpoint_IsNotImplementedWhenNoEditableFieldsExist`
- `Handle_ValidateDraftVersion_WithValidContent_ReturnsValidSummary`
- `Handle_ValidateDraftVersion_WithNoActiveQuestions_ReturnsErrors`
- `Handle_ValidateDraftVersion_WithInvalidOptions_ReturnsErrors`
- `Handle_PublishDraftVersion_WithInvalidContent_ThrowsInvalidOperationException`
- `Handle_PublishDraftVersion_WithValidContent_PublishesVersionAndExam`
- `Handle_RetirePublishedVersion_SetsRetiredAt`
- `Handle_CreateQuestion_WhenVersionNotDraft_ThrowsInvalidOperationException`
- `Handle_UpdateQuestion_WhenVersionDraft_UpdatesContent`
- `Handle_DeactivateQuestion_WhenVersionDraft_SetsIsActiveFalse`
- `Handle_DeleteQuestion_WhenReferencedBySessionSnapshot_ThrowsInvalidOperationException`
- `Handle_CreateAnswerOption_WhenVersionDraft_CreatesOption`
- `Handle_UpdateAnswerOption_WhenVersionNotDraft_ThrowsInvalidOperationException`
- `Handle_DeleteAnswerOption_WhenReferencedBySessionSnapshot_ThrowsInvalidOperationException`

**Implementation notes:**

- Use `IApplicationDbContext` only; no WebApi dependencies.
- Use existing exception mapping types: `KeyNotFoundException` for missing/hidden resources, `InvalidOperationException` for conflicts, FluentValidation for request validation.
- Parent-child mismatch returns `KeyNotFoundException`.
- Category `CountryId` is immutable after creation; attempted changes return `InvalidOperationException` mapped to `409 Conflict`.
- Exam structural/scoring fields are `CountryId`, `ExamCategoryId`, `DurationMinutes`, and `PassingScorePercentage`.
- Exam structural/scoring fields may change only while the exam has no published versions, no retired versions, and no sessions.
- After an exam has published/retired versions or any sessions, structural/scoring field changes return `InvalidOperationException` mapped to `409 Conflict`.
- Safe exam display/access fields `Title`, `Slug`, `Description`, `Instructions`, and `IsFree` remain editable on non-archived exams when validation and uniqueness rules pass.
- Invalid exam category/country mismatch returns `InvalidOperationException` mapped to `409 Conflict`.
- Published/retired versions are immutable.
- Do not implement draft version update unless the existing Phase 7A model has a real editable field. Current expected implementation omits it.
- Hard delete is draft/unused-only.
- Archive/deactivate is preferred over hard delete.
- Validation computes `QuestionCount` and `TotalPoints` from active draft content.
- Publish sets version `Status = Published`, `PublishedAt`, `QuestionCount`, `TotalPoints`, and parent exam `Status = Published` with `PublishedAt` when needed.
- Retire sets version `Status = Retired` and `RetiredAt`.
- Do not modify session snapshots.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "AdminExam"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after Task 2 if not in approved batch execution. Do not add WebApi endpoints before handler tests pass.

## Task 3: Infrastructure Review Or Migration Only If Needed

**Goal:** Confirm Phase 7A schema supports Phase 7B. Add no migration unless required and explicitly justified.

**Files:**

- Review `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`.
- Review `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`.
- Create `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/ExamAdminConfigurationTests.cs` only if new EF configuration or migration is needed.
- If a migration is approved, generate it through EF only.

**Tests to write first if infrastructure changes are needed:**

- `ExamAdminConfiguration_SupportsCategoryArchiveFlag`
- `ExamAdminConfiguration_SupportsQuestionAndOptionDeactivateFlags`
- `ExamAdminConfiguration_PreservesHistoricalRestrictDeletes`
- `ExamAdminConfiguration_NoUnexpectedSchemaChange`

**Implementation notes:**

- Default expected outcome is no source change and no migration.
- If no migration is needed, document that in the task report and continue.
- If migration is needed, use EF only and run pending model check.
- Do not manually edit database schema.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "ExamAdminConfiguration|ExamConfiguration"`
- `dotnet build backend/NursingPlatform.slnx`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if a migration appears necessary but was not approved by the execution prompt.

## Task 4: WebApi Admin Endpoints And Integration Tests

**Goal:** Add only approved Phase 7B admin/content endpoints under `/api/v1/admin` and prove permission behavior and raw JSON security.

**Files:**

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`.
- Test `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/AdminExamEndpointsTests.cs`.

**Endpoint groups to add:**

- `GET /api/v1/admin/exam-categories`
- `GET /api/v1/admin/exam-categories/{id:guid}`
- `POST /api/v1/admin/exam-categories`
- `PUT /api/v1/admin/exam-categories/{id:guid}`
- `POST /api/v1/admin/exam-categories/{id:guid}/archive`
- `POST /api/v1/admin/exam-categories/{id:guid}/restore`
- `DELETE /api/v1/admin/exam-categories/{id:guid}`
- `GET /api/v1/admin/exams`
- `GET /api/v1/admin/exams/{id:guid}`
- `POST /api/v1/admin/exams`
- `PUT /api/v1/admin/exams/{id:guid}`
- `POST /api/v1/admin/exams/{id:guid}/archive`
- `DELETE /api/v1/admin/exams/{id:guid}`
- `GET /api/v1/admin/exams/{examId:guid}/versions`
- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}`
- `POST /api/v1/admin/exams/{examId:guid}/versions`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/validate`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/publish`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/retire`
- `DELETE /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}`
- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions`
- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions`
- `PUT /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/deactivate`
- `DELETE /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}`
- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options`
- `PUT /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}/deactivate`
- `DELETE /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}`

**Tests to write first:**

- `AdminExamEndpoints_WithoutJwt_ReturnUnauthorized`
- `AdminExamEndpoints_WithoutPermission_ReturnForbidden`
- `AdminListExamCategories_WithViewPermission_SendsQuery`
- `AdminCreateExam_WithCreatePermission_ReturnsCreated`
- `AdminUpdateExam_WithEditPermission_SendsCommand`
- `AdminUpdateDraftVersionEndpoint_IsNotMapped_WhenNoEditableFieldsExist`
- `AdminDeleteExam_WithDeletePermission_SendsCommand`
- `AdminPublishDraftVersion_WithQuestionsManagePermission_SendsCommand`
- `AdminDeleteDraftVersion_WithExamsDeletePermission_SendsCommand`
- `AdminQuestionEndpoints_WithQuestionsViewOrManagePermission_Succeed`
- `AdminAnswerOptionEndpoints_WithQuestionsManagePermission_Succeed`
- `AdminContentValidationFailure_ReturnsValidationProblemDetails`
- `AdminContentConflict_ReturnsConflict`
- `AdminContentNotFound_ReturnsNotFound`
- `AdminContentJson_DoesNotExposeGlobalForbiddenFields`
- `AdminQuestionJson_MayExposeCorrectnessOnlyOnAdminRoutes`
- `NurseInProgressExamSessionJson_StillDoesNotExposeCorrectAnswersOrScoring`

**Implementation notes:**

- Keep endpoints thin.
- Use `.RequirePermission(...)` for every admin endpoint.
- Use existing route names from the spec.
- Do not add `AllowAnonymous`.
- Do not add nurse-facing endpoint changes except compatibility test coverage.
- Raw JSON tests must inspect response text before deserialization.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "AdminExam|ExamEndpoints"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after focused WebApi tests and build pass if not in approved batch execution.

## Task 5: Final Verification And Security Review

**Goal:** Verify Phase 7B end-to-end and confirm no out-of-scope behavior or unsafe exposure was introduced.

**Files:**

- No file modifications expected unless verification exposes an approved correction.

**Review checklist:**

- Confirm no frontend files changed.
- Confirm no payment/order/checkout/webhook files changed.
- Confirm no recruitment/contact-request/candidate/employer files changed.
- Confirm no nurse in-progress DTO exposes correct answers, correctness, explanations, score, percentage, or passed.
- Confirm admin DTOs do not expose account internals, payment state, session answers, or EF/domain navigation objects.
- Confirm admin endpoints require permissions and no `AllowAnonymous`.
- Confirm published/retired version mutation is blocked.
- Confirm unsafe hard delete returns conflict.
- Confirm historical session snapshots are not mutated.

**Verification commands:**

- `dotnet build backend/NursingPlatform.slnx`
- `dotnet test backend/NursingPlatform.slnx`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --stat`
- `git diff --cached --stat`

**Stop condition:** Stop if build, tests, security review, or EF model check fails after two fix cycles.

## Task 6: Tracking Documentation Update

**Goal:** Update project tracking only after Phase 7B implementation and final verification pass.

**Files:**

- Modify `CURRENT_TASK.md`.
- Modify `TASKS.md`.

**Implementation notes:**

- Mark only Phase 7B Exam Admin Content Management complete.
- Do not mark full Phase 7 analytics complete.
- Do not start Phase 8.
- Do not mention payments, frontend/admin UI, analytics dashboards, imports, or grants as implemented.

**Verification commands:**

- `git diff -- CURRENT_TASK.md TASKS.md`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after tracking diff and git status are reported if not in approved batch execution.

## Task 7: Final Commit

**Goal:** Commit approved Phase 7B implementation, tests, optional migration, and tracking files only after explicit reviewer approval or approved batch execution.

**Files:**

- Stage only approved Phase 7B files explicitly.
- Do not stage `codex_res/codex_report.md`.
- Do not stage `AGENTS.md`, `PROJECT_RULES.md`, `.gitignore`, frontend files, payment files, recruitment files, or unrelated docs.

**Commit strategy:**

- Planning commit, if requested separately: `docs: add exam admin content plan`.
- Implementation commit: `feat: add exam admin content management`.

**Verification commands:**

- `git status --short --untracked-files=all`
- `git diff --cached --name-only`
- `git diff --cached --stat`
- `git commit -m "feat: add exam admin content management"`
- `git status --short --untracked-files=all`
- `git log -1 --oneline`

**Stop condition:** Stop after reporting commit hash/message, post-commit status, and latest log line. Do not proceed to analytics, payments, frontend, imports, notifications, grants, or recruitment work.

## Explicit Out-of-Scope Guardrails

- No Phase 7 analytics dashboards.
- No Phase 8 payments.
- No frontend/admin UI.
- No checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- No exam access grant management.
- No imports or bulk upload.
- No AI question generation.
- No translations.
- No proctoring.
- No notifications or messaging.
- No recruitment changes.
- No nurse attempt/result/review contract changes except compatibility-safe tests.

## Self-Review Checklist

- Spec coverage: Plan covers categories, exams, draft versions, questions, options, validate, publish, retire, archive, delete/deactivate, permissions, DTO security, tests, verification, tracking, and commit.
- Scope check: Plan is Phase 7B backend content management only.
- Permission check: Plan uses existing `Exams.*` and `Questions.*` permissions.
- Migration check: Plan expects no migration and requires explicit stop if one becomes necessary without approval.
- Security check: Admin correctness exposure is restricted to permission-protected admin routes; nurse pre-completion secrecy remains unchanged.
- Placeholder scan: No unresolved placeholders are intentionally left in this plan.
