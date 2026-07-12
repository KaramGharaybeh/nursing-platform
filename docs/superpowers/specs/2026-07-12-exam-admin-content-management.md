# Phase 7B Exam Admin Content Management Specification

## Objective

Define Phase 7B as backend-only administration and content management for the exam foundation created in Phase 7A.

Phase 7B lets authorized admin/content users manage exam categories, exam catalog records, draft exam versions, questions, answer options, and publish/retire/archive workflows. It must protect historical attempts, keep published versions immutable, and preserve Phase 7A nurse-facing answer secrecy.

This specification does not start implementation and does not modify source code, tests, migrations, tracking docs, frontend, recruitment, or payments.

## Current Baseline

Phase 7A already provides:

- Exam domain entities: `ExamCategory`, `Exam`, `ExamVersion`, `ExamQuestion`, `ExamAnswerOption`, `ExamAccessGrant`, `ExamSession`, `ExamSessionQuestion`, `ExamSessionAnswerOption`, and `ExamSessionAnswer`.
- Exam lifecycle enums: `ExamStatus`, `ExamVersionStatus`, `ExamSessionStatus`, and `ExamQuestionType`.
- EF persistence, indexes, and the `AddExamFoundation` migration.
- Nurse-facing exam catalog/session/result/review endpoints.
- Published content validation before nurse session start.
- Immutable session snapshots for historical attempts.
- Existing permission constants and seeded permissions for `Exams.*` and `Questions.*`.

Phase 7A intentionally deferred admin content-management APIs. Phase 7B fills that backend gap without adding payments, analytics, imports, or frontend UI.

## In Scope

- Backend-only admin/content API for exam categories.
- Backend-only admin/content API for exams.
- Backend-only admin/content API for draft exam versions.
- Backend-only admin/content API for questions in draft versions.
- Backend-only admin/content API for answer options in draft-version questions.
- Draft-version content validation endpoint.
- Publish workflow for draft exam versions.
- Retire workflow for published exam versions.
- Archive workflow for exams and categories.
- Safe deletion/deactivation semantics that protect published content and historical attempts.
- DTOs that may expose correct answers only to authorized admin/content users.
- Permission-protected Minimal API endpoints using existing authorization infrastructure.
- Tests for permission behavior, DTO security, content validation, version immutability, and historical-attempt protection.

## Out of Scope

- Frontend/admin UI.
- Real payments.
- Checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- Exam access grant management.
- Nurse performance analytics.
- Analytics dashboards.
- Question import pipeline.
- Bulk upload.
- Excel/CSV import.
- AI question generation.
- Translations.
- Proctoring.
- Notifications or messaging.
- Recruitment, contact-request, candidate, or employer changes.
- Modifying Phase 7A nurse session/result/review contracts except compatibility-safe reuse.
- Mutating session snapshots or nurse attempts from admin content endpoints.

## Actors and Permissions

### Admin

An authenticated user with admin-level permissions. Admin may manage categories, exams, versions, questions, options, publish/retire/archive content, and perform hard delete only when explicitly allowed by draft/unused rules.

### Content Manager

An authenticated user with exam/question permissions. Content managers may manage the parts allowed by their permissions. They are not a separate role in Phase 7B; permission checks determine access.

### Nurse

Nurse-facing Phase 7A behavior remains unchanged. Nurses cannot access Phase 7B admin endpoints.

### Anonymous User

Anonymous users cannot access any Phase 7B endpoint.

## Permission Mapping

Phase 7B must reuse the existing permission constants:

- `Exams.View`
- `Exams.Create`
- `Exams.Edit`
- `Exams.Delete`
- `Questions.View`
- `Questions.Manage`

All Phase 7B endpoints require authentication and permission checks. Endpoints must use `.RequirePermission(...)`, which already composes authorization with the project permission policy. Do not use `.AllowAnonymous()`.

Mapping:

- Read category/exam/version metadata: `Exams.View`.
- Create categories: `Exams.Create`.
- Update/archive categories: `Exams.Edit`.
- Delete draft/unused categories when allowed: `Exams.Delete`.
- Create exams: `Exams.Create`.
- Update/archive exams: `Exams.Edit`.
- Delete draft/unused exams when allowed: `Exams.Delete`.
- Create draft versions: `Exams.Edit`.
- Read draft/published/retired versions: `Exams.View`.
- Delete draft/unused versions: `Exams.Delete`.
- Publish draft versions: `Questions.Manage`. This is the final Phase 7B endpoint permission decision because publishing depends on validated question/option content and the existing `RequirePermission` extension accepts one permission. Do not invent new multi-permission authorization infrastructure in Phase 7B.
- Retire versions: `Exams.Edit`.
- Validate draft version content: `Questions.View`.
- List/get questions/options: `Questions.View`.
- Create/update/deactivate/delete questions/options: `Questions.Manage`.

Reviewer decision locked: Phase 7B uses existing permissions and does not create new permission names.

## Domain And Model Impact

Phase 7B should use the Phase 7A schema by default. No migration is expected if the existing fields are enough:

- `ExamCategory.IsActive` supports category archive/deactivation.
- `Exam.Status` supports `Draft`, `Published`, and `Archived`.
- `ExamVersion.Status` supports `Draft`, `Published`, and `Retired`.
- `ExamQuestion.IsActive` supports question deactivation.
- `ExamAnswerOption.IsActive` supports option deactivation.
- Existing audit fields support basic audit expectations.

Phase 7A `ExamVersion` has no editable metadata beyond system-managed fields such as `VersionNumber`, `Status`, `QuestionCount`, `TotalPoints`, `PublishedAt`, and `RetiredAt`. Phase 7B must not add a no-op draft version update endpoint and must not create an empty/no-op `UpsertAdminExamVersionRequest`.

Do not implement `PUT /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}` unless the existing Phase 7A model has a real editable field. Current decision: no such field exists, so omit this endpoint from Phase 7B implementation.

Do not add a migration just to support version metadata in Phase 7B. If implementation discovers that Phase 7A entities lack a required field for other approved Phase 7B behavior, the implementation must stop or justify the smallest EF migration in the plan. Planning decision: no schema changes are expected.

No content endpoint may write `ExamSession`, `ExamSessionQuestion`, `ExamSessionAnswerOption`, or `ExamSessionAnswer` except read-only existence checks used to protect deletion and immutability.

## API Contract Proposal

All routes are under `/api/v1/admin` and return DTOs, never EF/domain entities.

### Categories

- `GET /api/v1/admin/exam-categories`
  - Name: `AdminListExamCategories`
  - Permission: `Exams.View`
  - Query: `page`, `pageSize`, `countryId`, `isActive`
- `GET /api/v1/admin/exam-categories/{id:guid}`
  - Name: `AdminGetExamCategory`
  - Permission: `Exams.View`
- `POST /api/v1/admin/exam-categories`
  - Name: `AdminCreateExamCategory`
  - Permission: `Exams.Create`
- `PUT /api/v1/admin/exam-categories/{id:guid}`
  - Name: `AdminUpdateExamCategory`
  - Permission: `Exams.Edit`
- `POST /api/v1/admin/exam-categories/{id:guid}/archive`
  - Name: `AdminArchiveExamCategory`
  - Permission: `Exams.Edit`
- `POST /api/v1/admin/exam-categories/{id:guid}/restore`
  - Name: `AdminRestoreExamCategory`
  - Permission: `Exams.Edit`
- `DELETE /api/v1/admin/exam-categories/{id:guid}`
  - Name: `AdminDeleteExamCategory`
  - Permission: `Exams.Delete`
  - Only allowed when no exam references the category.

### Exams

- `GET /api/v1/admin/exams`
  - Name: `AdminListExams`
  - Permission: `Exams.View`
  - Query: `page`, `pageSize`, `countryId`, `categoryId`, `status`, `isFree`
- `GET /api/v1/admin/exams/{id:guid}`
  - Name: `AdminGetExam`
  - Permission: `Exams.View`
- `POST /api/v1/admin/exams`
  - Name: `AdminCreateExam`
  - Permission: `Exams.Create`
- `PUT /api/v1/admin/exams/{id:guid}`
  - Name: `AdminUpdateExam`
  - Permission: `Exams.Edit`
  - Updates catalog metadata only. Does not edit published version content.
- `POST /api/v1/admin/exams/{id:guid}/archive`
  - Name: `AdminArchiveExam`
  - Permission: `Exams.Edit`
  - Blocks new nurse starts but preserves history.
- `DELETE /api/v1/admin/exams/{id:guid}`
  - Name: `AdminDeleteExam`
  - Permission: `Exams.Delete`
  - Only allowed for draft exams with no versions and no sessions. Otherwise archive.

### Exam Versions

- `GET /api/v1/admin/exams/{examId:guid}/versions`
  - Name: `AdminListExamVersions`
  - Permission: `Exams.View`
- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}`
  - Name: `AdminGetExamVersion`
  - Permission: `Exams.View`
- `POST /api/v1/admin/exams/{examId:guid}/versions`
  - Name: `AdminCreateDraftExamVersion`
  - Permission: `Exams.Edit`
  - Creates a new draft version with the next `VersionNumber`.
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/validate`
  - Name: `AdminValidateDraftExamVersion`
  - Permission: `Questions.View`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/publish`
  - Name: `AdminPublishDraftExamVersion`
  - Permission: `Questions.Manage`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/retire`
  - Name: `AdminRetireExamVersion`
  - Permission: `Exams.Edit`
- `DELETE /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}`
  - Name: `AdminDeleteDraftExamVersion`
  - Permission: `Exams.Delete`
  - Draft-only and unused-only. Otherwise keep the version and use retire/archive workflows where applicable.

### Questions

- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions`
  - Name: `AdminListExamQuestions`
  - Permission: `Questions.View`
- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}`
  - Name: `AdminGetExamQuestion`
  - Permission: `Questions.View`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions`
  - Name: `AdminCreateExamQuestion`
  - Permission: `Questions.Manage`
- `PUT /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}`
  - Name: `AdminUpdateExamQuestion`
  - Permission: `Questions.Manage`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/deactivate`
  - Name: `AdminDeactivateExamQuestion`
  - Permission: `Questions.Manage`
- `DELETE /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}`
  - Name: `AdminDeleteExamQuestion`
  - Permission: `Questions.Manage`
  - Draft-only and unused-only. Otherwise deactivate.

### Answer Options

- `GET /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options`
  - Name: `AdminListExamAnswerOptions`
  - Permission: `Questions.View`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options`
  - Name: `AdminCreateExamAnswerOption`
  - Permission: `Questions.Manage`
- `PUT /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}`
  - Name: `AdminUpdateExamAnswerOption`
  - Permission: `Questions.Manage`
- `POST /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}/deactivate`
  - Name: `AdminDeactivateExamAnswerOption`
  - Permission: `Questions.Manage`
- `DELETE /api/v1/admin/exams/{examId:guid}/versions/{versionId:guid}/questions/{questionId:guid}/options/{optionId:guid}`
  - Name: `AdminDeleteExamAnswerOption`
  - Permission: `Questions.Manage`
  - Draft-only and unused-only. Otherwise deactivate.

## Request DTOs

Category create:

- `CountryId`
- `Name`
- `Slug`
- `Description`
- `DisplayOrder`

Category update:

- `Name`
- `Slug`
- `Description`
- `DisplayOrder`

`ExamCategory.CountryId` is immutable after creation. Category update may change only `Name`, `Slug`, `Description`, and `DisplayOrder`. If a request or command attempts to change `CountryId`, the handler must return `409 Conflict`; do not silently ignore the attempted country change.

Exam create:

- `CountryId`
- `ExamCategoryId`
- `Title`
- `Slug`
- `Description`
- `Instructions`
- `DurationMinutes`
- `PassingScorePercentage`
- `IsFree`

Exam update:

- `CountryId`
- `ExamCategoryId`
- `Title`
- `Slug`
- `Description`
- `Instructions`
- `DurationMinutes`
- `PassingScorePercentage`
- `IsFree`

Exam update rules are field-level:

- `CountryId`, `ExamCategoryId`, `DurationMinutes`, and `PassingScorePercentage` are structural/scoring fields.
- Structural/scoring fields may be changed only while the exam has no published versions, no retired versions, and no sessions.
- After an exam has published/retired versions or any sessions, changing structural/scoring fields must return `409 Conflict`.
- Safe catalog/display fields may still be updated with `Exams.Edit`: `Title`, `Description`, `Instructions`, `IsFree`, and `Slug` when uniqueness rules pass and route behavior remains stable.
- `IsFree` is safe because it affects future access only and does not mutate historical sessions.
- Updating archived exams remains blocked; restore must happen first if a later phase adds restore behavior.
- Invalid category/country mismatch must return `409 Conflict` consistently.

Draft version create:

- `QuestionCount` and `TotalPoints` are system-calculated on validation/publish, not client-authored.
- Do not create `UpsertAdminExamVersionRequest`.
- Do not create an empty/no-op draft version update request.
- Do not add a migration just to support version metadata in Phase 7B.

Question create/update:

- `QuestionText`
- `Explanation`
- `Points`
- `DisplayOrder`
- `IsActive`
- `QuestionType` must be `SingleBestAnswer` in Phase 7B.

Answer option create/update:

- `OptionText`
- `DisplayOrder`
- `IsCorrect`
- `IsActive`

## DTO Security Rules

Admin content DTOs may expose correct answers, `IsCorrect`, and explanations only to authorized admin/content endpoints.

Admin content DTOs must not expose:

- User account internals.
- `UserId`.
- Roles or permissions.
- Password hashes.
- Access tokens, refresh tokens, token hashes, or internal auth state.
- Nurse answers, selected answer ids, attempt details, or session review data.
- Payment provider ids or payment state.
- EF/domain entities or navigation objects.

Nurse-facing DTO contracts from Phase 7A must remain compatible and must not gain correct-answer or explanation exposure before completion.

Raw JSON WebApi tests must inspect response text before deserialization for sensitive field leaks.

## State Transitions

### ExamCategory

- Active category can be archived by setting `IsActive = false`.
- Archived category can be restored by setting `IsActive = true`.
- `CountryId` is immutable after creation; attempted changes return `409 Conflict`.
- Category update may change only `Name`, `Slug`, `Description`, and `DisplayOrder`.
- Hard delete is allowed only when no exam references the category.

### Exam

- `Draft -> Published` happens indirectly when the first draft version is published and exam metadata is valid.
- `Published -> Archived` archives an exam and blocks new starts.
- `Archived` exams remain readable by admin/content users and historical nurse owners through Phase 7A session/result/review rules.
- `CountryId`, `ExamCategoryId`, `DurationMinutes`, and `PassingScorePercentage` are draft-only structural/scoring fields.
- Structural/scoring fields may be changed only while the exam has no published versions, no retired versions, and no sessions.
- After an exam has published/retired versions or any sessions, structural/scoring field changes return `409 Conflict`.
- Safe display/access fields `Title`, `Slug`, `Description`, `Instructions`, and `IsFree` may be updated when the exam is not archived and validation/uniqueness rules pass.
- Hard delete is allowed only when the exam has no versions and no sessions.

### ExamVersion

- New versions are created as `Draft`.
- `Draft -> Published` is allowed only after content validation passes.
- `Published -> Retired` blocks future starts but preserves historical attempts.
- `Retired` is terminal for new starts.
- Published and retired versions are immutable through content-management endpoints.

### ExamQuestion And ExamAnswerOption

- Create/update/delete is allowed only when the parent version is `Draft`.
- Deactivate is allowed only when the parent version is `Draft`.
- Published/retired version content cannot be mutated.
- Historical session snapshots cannot be mutated.

## Content Validation Rules

Publishing a draft version requires:

- Parent exam exists.
- Parent exam is not archived.
- Existing `CountryId` is valid.
- If `ExamCategoryId` is supplied, category exists, belongs to the same country, and is active.
- Exam title and slug are non-empty.
- Exam duration is positive and within a bounded maximum chosen in implementation validators.
- Passing score is between `0` and `100`.
- At least one active `SingleBestAnswer` question.
- Every active question has at least two active options.
- Every active question has exactly one correct active option.
- Every active question has positive points.
- Display order values are deterministic; duplicate display orders may be allowed only if sorting falls back to `Id`, but plan should prefer validation against duplicate display order per parent where practical.

Validation endpoint returns a DTO with:

- `IsValid`.
- `Errors` collection.
- `QuestionCount`.
- `TotalPoints`.

Publish endpoint must reject invalid content with `409 Conflict`.

## Publishing And Versioning Rules

- Editing a published version is blocked.
- Editing a retired version is blocked.
- To change published content, admin/content user creates a new draft version.
- A new draft version gets the next `VersionNumber` for the exam.
- Published version content remains immutable so historical attempts keep their meaning.
- Publishing a draft version sets `QuestionCount`, `TotalPoints`, and `PublishedAt`.
- Parent exam becomes `Published` on successful version publish unless it is already published.
- Phase 7B may allow multiple published versions over time, but Phase 7A nurse start behavior chooses the highest published version. Retiring old versions is supported and recommended.
- Retiring a published version sets `RetiredAt` and blocks new starts of that version.
- Retiring the only published version does not delete historical attempts.

## Deletion And Archive Rules

- Prefer archive/deactivate over hard delete for content that might have business meaning.
- Hard delete is allowed only for draft and unused content.
- Categories referenced by exams cannot be hard-deleted.
- Exams with versions or sessions cannot be hard-deleted.
- Draft versions can be deleted only when they have no sessions. Published/retired versions cannot be hard-deleted.
- Draft questions/options can be hard-deleted only when not referenced by session snapshot records. Because sessions are only created from published versions, this should usually be true for draft content.
- Published/retired questions/options cannot be hard-deleted or edited.
- Historical `ExamSession*` snapshot rows are never changed by Phase 7B content endpoints.

## Error Behavior

- Missing JWT: `401 Unauthorized`.
- Authenticated user without required permission: `403 Forbidden`.
- Invalid route GUID binding: `400 Bad Request`.
- Invalid request body or pagination: `400 Bad Request` validation Problem Details.
- Nonexistent category/exam/version/question/option: `404 Not Found`.
- Parent-child mismatch, such as a version not belonging to an exam: `404 Not Found`.
- Attempting to mutate published or retired content: `409 Conflict`.
- Attempting to publish invalid content: `409 Conflict`.
- Attempting unsafe hard delete: `409 Conflict`.
- Attempting to change `ExamCategory.CountryId`: `409 Conflict`.
- Attempting to change exam structural/scoring fields after published/retired versions or sessions exist: `409 Conflict`.
- Invalid exam category/country mismatch: `409 Conflict`.
- Unexpected exceptions: existing `500` Problem Details behavior.

## Testing Requirements

Application tests must cover:

- Permission-independent handlers enforce content business rules.
- Category create/update/archive/restore/delete rules.
- Category `CountryId` immutability returns conflict when a change is attempted.
- Exam create/update/archive/delete rules.
- Changing structural/scoring fields on a published/used exam returns conflict.
- Changing safe exam display/access fields remains allowed when the exam is not archived and validation passes.
- Invalid category/country mismatch returns conflict consistently.
- Draft version creation uses next version number.
- No draft version update endpoint or `UpsertAdminExamVersionRequest` is implemented unless a real editable Phase 7A model field exists.
- Publish validates content and computes `QuestionCount` and `TotalPoints`.
- Publish rejects no questions, fewer than two active options, zero/multiple correct options, non-positive points, invalid duration, and invalid passing score.
- Retire blocks new starts but preserves historical attempts.
- Published/retired version mutation is rejected.
- Question/option create/update/delete/deactivate is draft-only.
- Unsafe hard deletes return conflict.
- DTO reflection tests reject account internals, nurse attempt data, payment fields, EF/domain entities, and navigation objects.

WebApi integration tests must cover:

- Every admin endpoint returns `401` without JWT.
- Every admin endpoint returns `403` for authenticated users without required permission.
- Representative endpoints return success when required permission is present.
- No admin endpoint uses `AllowAnonymous`.
- Admin endpoints use expected route names.
- Raw JSON for admin content responses does not expose forbidden global fields.
- Admin question/option responses may expose `isCorrect` and `explanation` only on admin routes.
- Existing nurse-facing in-progress session JSON still does not expose `isCorrect`, `correctAnswerOptionId`, `explanation`, `score`, `percentage`, or `passed`.

Infrastructure tests must cover:

- No migration is expected if Phase 7A model supports the behavior.
- If a migration is needed, verify EF model and indexes remain compatible with Phase 7A.
- Existing historical integrity delete restrictions remain intact.

## Performance And Indexing Considerations

Use Phase 7A indexes where possible:

- Category listing by country and active state.
- Exam listing by status, country, category, title, and id.
- Version lookup by exam, status, and version number.
- Question and option ordering by parent, display order, and id.

No new indexes are planned in Phase 7B unless implementation evidence shows a missing query path. If a new index is needed, it requires an EF migration and explicit justification.

## Acceptance Criteria

- Spec and implementation plan exist for Phase 7B.
- No implementation starts during planning.
- No source files, tests, migrations, tracking docs, frontend, payments, recruitment, or nurse-taking behavior are modified during planning.
- Phase 7B scope is backend-only admin/content management.
- Existing permissions are reused.
- Admin endpoints require authorization and permission checks.
- Published versions are immutable.
- Historical attempts and session snapshots are protected.
- Publish workflow enforces content validity.
- Hard delete is blocked for published, retired, used, or referenced content.
- Correct answers are exposed only to authorized admin/content endpoints and completed nurse review endpoints.
- Nurse-facing pre-completion answer secrecy remains unchanged.

## Reviewer Decisions Needed

None. This spec locks the safe default decisions for Phase 7B:

- Reuse existing permissions.
- Use `/api/v1/admin/...` route prefix.
- Use `.RequirePermission(...)` on admin endpoints.
- Prefer archive/deactivate over delete.
- Block mutation of published/retired versions.
- Expect no migration unless implementation proves a necessary schema gap.
