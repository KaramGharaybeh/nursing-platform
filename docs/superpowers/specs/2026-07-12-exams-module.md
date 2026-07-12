# Phase 7 - Exams Module Specification

## Objective

Define the Phase 7 Exams Module safely without starting implementation. Phase 7 introduces the examination foundation for nurses: country-specific mock exam catalog, versioned exam forms, timed sessions, answer submission, scoring, result review, and basic attempt history.

This specification chooses **Phase 7A - Exam Foundation** as the first implementation batch. Full Phase 7 is too large for one safe batch because it spans content administration, payments, analytics dashboards, imports, advanced randomization, and future commercial access rules. Phase 7A must create a production-quality foundation that can later support paid exams and richer admin workflows without pretending payment processing exists today.

This specification does not modify source code, tests, migrations, tracking docs, frontend, recruitment, or payments.

## Current Baseline

The platform currently provides:

- Identity, JWT authentication, roles, permissions, and current-user services.
- Nurse profile ownership and `NurseRoleGuard`.
- Existing `Country` reference data.
- Existing permissions for `Exams.*` and `Questions.*`.
- Clean Architecture with Domain, Application, Infrastructure, and WebApi.
- EF Core Code-First migrations with PostgreSQL.
- WebApi Problem Details exception behavior.

The platform does not currently provide:

- Exam domain entities.
- Exam catalog endpoints.
- Exam session state.
- Payment provider integration.
- Admin content-management UI.
- Exam analytics dashboards.

## Chosen First Batch Scope

Phase 7A - Exam Foundation is in scope.

Reasons:

- It delivers a complete nurse-facing mock exam loop without depending on payments.
- It establishes durable domain and persistence boundaries for later paid access and admin tooling.
- It keeps correct-answer secrecy testable before adding richer content management.
- It avoids mixing content authoring, payment processing, analytics dashboards, and exam-taking workflows in one high-risk batch.
- It fits the existing backend-only task workflow.

## In Scope

- Exam country context using existing `Country` reference data.
- Exam categories for grouping exams within a country or licensing context.
- Exam catalog listing for published, active mock exams.
- Exam details that show safe metadata before a session starts.
- Exam versions/forms so content changes do not mutate historical attempts.
- Questions and answer options stored in the domain and database.
- Correct answer and explanation storage that is never exposed before completion.
- Foundation access model without real payments.
- Nurse starts an accessible mock exam session.
- Nurse retrieves in-progress session questions without correct answers or explanations, while seeing their own saved selected answer option id per session question.
- Nurse saves or submits answers.
- Timed session expiry rules.
- Immediate scoring on submit or auto-expiry finalization.
- Result summary after completion.
- Review after completion with submitted answer, correct answer, and explanation.
- Basic attempt history for the authenticated nurse.
- Admin/content management boundaries for later work.
- Domain, Application, Infrastructure, WebApi, and tests in the future implementation plan.

## Out of Scope

- Full Phase 7 analytics dashboards.
- Frontend implementation.
- Real payment processor integration.
- Checkout, orders, products, subscriptions, invoices, refunds, or webhooks.
- Recruitment, contact requests, candidate search, or employer workflows.
- Question import pipeline.
- Rich admin UI.
- Bulk content authoring.
- Adaptive exams.
- AI recommendations.
- Certificates of completion.
- Proctoring, browser lockdown, or anti-cheat monitoring.
- Multi-language question translations.
- Partial-credit scoring.
- Free-text, essay, audio, or file-upload questions.
- Messaging, notifications, or email reminders.
- Public anonymous exam attempts.

## Actors and Permissions

### Nurse

An authenticated active user with the `Nurse` role and a `NurseProfile`.

Nurse may:

- List published exams available through Phase 7A access rules.
- View safe exam metadata.
- Start an accessible exam session.
- Resume an in-progress own session.
- Save answers for an in-progress own session.
- Submit an in-progress own session.
- View own completed result and review details.
- List own attempt history.

Nurse may not:

- See correct answers or explanations before completion.
- Modify exams, exam versions, questions, options, scoring rules, or access entitlements.
- View another nurse's sessions, answers, results, or attempt history.
- Submit answers after completion, expiration finalization, cancellation, or abandonment.

### Admin / Content Manager

Phase 7A defines boundaries but does not require full admin content endpoints unless the implementation plan explicitly includes seed-only or minimal internal setup. Existing permissions should be reused later:

- `Exams.View`
- `Exams.Create`
- `Exams.Edit`
- `Exams.Delete`
- `Questions.View`
- `Questions.Manage`

Phase 7A implementation may seed or create test content through EF/test fixtures, but must not build a full admin UI or broad admin content-management API unless separately approved.

### Anonymous User

Anonymous users may not start sessions, submit answers, or view results. Catalog visibility may remain authenticated-only in Phase 7A to keep authorization simple and consistent with nurse ownership.

## Required Decisions

- First implementation batch: **Phase 7A Exam Foundation**, not full Phase 7.
- Exam lifecycle: `Draft`, `Published`, `Archived`.
- Exam version lifecycle: `Draft`, `Published`, `Retired`.
- Session lifecycle: `InProgress`, `Submitted`, `Expired`, `Abandoned`.
- Question order in Phase 7A: deterministic snapshot order.
- Answer option order in Phase 7A: deterministic snapshot order.
- Nurse can resume an in-progress session and see only their own saved selected answer option ids.
- Timer expiry: expired sessions can no longer accept answer changes; finalization scores saved answers and marks the session `Expired`.
- Scoring happens immediately on submit and on expiry finalization.
- Review shows correct answers and explanations only after `Submitted` or `Expired`.
- Multiple attempts are allowed, but only one in-progress session per nurse per exam version is allowed and must be protected by a provider-compatible unique filtered index.
- Access without payments: Phase 7A uses free/mock access plus an entitlement-ready model. Published exams may be `IsFree == true`; non-free exams require an `ExamAccessGrant` created by admin/data setup. No payment processor is implemented.
- Admin/content scope: domain and persistence support content; full admin CRUD is deferred.

## Domain Model Proposal

### ExamCategory

- `Id`
- `CountryId`
- `Name`
- `Slug`
- `Description`
- `DisplayOrder`
- `IsActive`
- audit fields

Purpose: group exams by country/licensing context.

### Exam

- `Id`
- `CountryId`
- `ExamCategoryId`
- `Title`
- `Slug`
- `Description`
- `Instructions`
- `DurationMinutes`
- `PassingScorePercentage`
- `Status`
- `IsFree`
- `PublishedAt`
- audit fields

Purpose: stable catalog identity and business metadata.

Lifecycle:

- `Draft`: not visible to nurses.
- `Published`: visible to eligible nurses.
- `Archived`: hidden from new starts, historical sessions remain readable by owners.

### ExamVersion

- `Id`
- `ExamId`
- `VersionNumber`
- `Status`
- `QuestionCount`
- `TotalPoints`
- `PublishedAt`
- `RetiredAt`
- audit fields

Purpose: immutable published form used by sessions. Historical attempts must point at the version taken.

Lifecycle:

- `Draft`: content can be edited before publishing.
- `Published`: can be started when parent exam is published and access rules pass.
- `Retired`: no new sessions, historical attempts remain readable.

### ExamQuestion

- `Id`
- `ExamVersionId`
- `QuestionText`
- `Explanation`
- `QuestionType`
- `Points`
- `DisplayOrder`
- `IsActive`
- audit fields

Phase 7A supports only single-best-answer multiple-choice questions.

### ExamAnswerOption

- `Id`
- `ExamQuestionId`
- `OptionText`
- `DisplayOrder`
- `IsCorrect`
- audit fields

Correctness is stored internally and must not be exposed before completion.

### ExamAccessGrant

- `Id`
- `NurseProfileId`
- `ExamId`
- `GrantedAt`
- `ExpiresAt`
- `Reason`
- audit fields

Purpose: entitlement-ready model for paid or admin-granted future access. In Phase 7A, free exams do not require a grant; non-free exams require a grant but no payment creates it.

### ExamSession

- `Id`
- `NurseProfileId`
- `ExamId`
- `ExamVersionId`
- `Status`
- `StartedAt`
- `ExpiresAt`
- `SubmittedAt`
- `FinalizedAt`
- `Score`
- `MaxScore`
- `Percentage`
- `Passed`
- `CorrectCount`
- `QuestionCount`
- audit fields

Purpose: one nurse attempt against one immutable exam version.

### ExamSessionQuestion

- `Id`
- `ExamSessionId`
- `ExamQuestionId`
- `DisplayOrder`
- `QuestionTextSnapshot`
- `ExplanationSnapshot`
- `Points`

Purpose: snapshot question order/content for a session.

### ExamSessionAnswerOption

- `Id`
- `ExamSessionQuestionId`
- `ExamAnswerOptionId`
- `DisplayOrder`
- `OptionTextSnapshot`
- `IsCorrectSnapshot`

Purpose: snapshot options and correctness for scoring/review. `IsCorrectSnapshot` must not be projected before completion.

### ExamSessionAnswer

- `Id`
- `ExamSessionQuestionId`
- `SelectedExamSessionAnswerOptionId`
- `AnsweredAt`

Purpose: the nurse's selected answer for each question.

## API Contract Proposal

All Phase 7A endpoints are under `/api/v1` and require `.RequireAuthorization()`. Application handlers enforce Nurse role and ownership for nurse workflows.

### Nurse-Facing Endpoints

| Method | Route | Name | Purpose |
|-------|-------|------|---------|
| GET | `/exams` | `ListExams` | List published accessible exam catalog items. |
| GET | `/exams/{id:guid}` | `GetExam` | Get safe exam metadata. |
| POST | `/exams/{id:guid}/sessions` | `StartExamSession` | Start or return own in-progress session for the current published version. |
| GET | `/exam-sessions/{id:guid}` | `GetExamSession` | Get own session with questions/options and own saved selected option ids, but no correctness/explanations/scoring before completion. |
| PUT | `/exam-sessions/{id:guid}/answers` | `SaveExamSessionAnswers` | Save selected answers for own in-progress session. |
| POST | `/exam-sessions/{id:guid}/submit` | `SubmitExamSession` | Submit own in-progress session and score immediately. |
| GET | `/exam-sessions/{id:guid}/result` | `GetExamSessionResult` | Get own completed/expired result summary. |
| GET | `/exam-sessions/{id:guid}/review` | `GetExamSessionReview` | Get own completed/expired review with correct answers and explanations. |
| GET | `/me/nurse-profile/exam-attempts` | `ListMyExamAttempts` | List own attempt history. |

### Query Parameters

Catalog:

- `page`: default `1`, minimum `1`.
- `pageSize`: default `20`, minimum `1`, maximum `100`.
- `countryId`: optional GUID.
- `categoryId`: optional GUID.

Attempt history:

- `page`: default `1`, minimum `1`.
- `pageSize`: default `20`, minimum `1`, maximum `100`.
- `status`: optional `InProgress`, `Submitted`, `Expired`, or `Abandoned`.

No sorting parameters are required in Phase 7A. Default catalog ordering is country name, category display order, exam title, and id. Attempt ordering is `StartedAt` descending and `Id` ascending.

### SaveExamSessionAnswersRequest

The save answers endpoint uses an explicit request DTO:

- `SaveExamSessionAnswersRequest`
  - `Answers`: collection of answer items.
- Each answer item:
  - `ExamSessionQuestionId`: required `Guid`.
  - `SelectedExamSessionAnswerOptionId`: required `Guid`.

Save behavior is a partial upsert:

- Only answer items supplied in the request are created or updated.
- Existing saved answers for questions omitted from the request remain unchanged.
- The request must contain at least one answer.
- A single request must not contain duplicate `ExamSessionQuestionId` values.
- Empty Guid values are invalid.
- Every supplied question id and selected option id must belong to the same owned session snapshot.

## DTO Security Rules

DTOs must be explicit and must not serialize EF/domain entities directly.

Before completion, session DTOs may expose:

- The nurse's own saved `SelectedExamSessionAnswerOptionId` per session question.

Before completion, session DTOs must not expose:

- Correct answer option id.
- `IsCorrect` flags.
- Explanations.
- Score.
- Percentage.
- Passing outcome.
- Internal answer-option correctness snapshots.

All Phase 7A DTOs must not expose:

- User account internals.
- Roles or permissions.
- Password hashes.
- Tokens or token hashes.
- Email verification state.
- Nurse profile internals not required by the API.
- EF navigation objects.
- Domain entities or database entities.
- Payment provider ids or payment state.

After completion, review DTOs may expose:

- Selected answer option id.
- Correct answer option id.
- Option correctness.
- Explanation.
- Per-question points earned.

Review DTOs must be available only for the authenticated nurse who owns the session and only after session status is `Submitted` or `Expired`.

## State Transitions

### Exam

Allowed:

- `Draft -> Published`
- `Published -> Archived`
- `Archived` is terminal for new starts in Phase 7A.

### ExamVersion

Allowed:

- `Draft -> Published`
- `Published -> Retired`
- `Retired` blocks new starts but preserves history.

### ExamSession

Allowed:

- none -> `InProgress` when nurse starts an accessible exam.
- `InProgress -> Submitted` when nurse submits before expiry.
- `InProgress -> Expired` when current time is at or after `ExpiresAt` and the session is finalized by a read/submit/save path or future background job.
- `InProgress -> Abandoned` is reserved for future cleanup/admin policy and should not be exposed in Phase 7A unless explicitly implemented.

Terminal:

- `Submitted`
- `Expired`
- `Abandoned`

Invalid transitions return `409 Conflict`. Non-owned sessions return `404 Not Found` to avoid leaking ids.

Concurrent start protection:

- Phase 7A must enforce one `InProgress` session per `NurseProfileId + ExamVersionId`.
- The required technical approach is a PostgreSQL/EF-compatible unique filtered index on `NurseProfileId` and `ExamVersionId` where `Status == 'InProgress'`.
- If EF/PostgreSQL filtered unique index support cannot be implemented cleanly, implementation must stop and report instead of falling back to weak application-only duplicate prevention.

## Timer and Expiry Rules

- `ExpiresAt = StartedAt + DurationMinutes`.
- Server time is authoritative.
- In-progress session reads include remaining time seconds calculated server-side.
- If `StartExamSession` finds an existing own `InProgress` session for the current exam version and it is not expired, return that session.
- If `StartExamSession` finds an existing own `InProgress` session for the current exam version and it is expired by server time, finalize it as `Expired`, score saved answers, then create a new `InProgress` session.
- Expired `StartExamSession` resume attempts must finalize the expired session before creating the replacement session.
- `GetExamSession` for an expired-by-time `InProgress` session must finalize it as `Expired` before returning the session state.
- Saving answers after expiry returns `409 Conflict` after finalizing the session as `Expired`.
- Submitting after expiry finalizes and returns the expired result if this request won finalization; if another request already finalized it, the handler must consistently return either `409 Conflict` or the already finalized result, and tests must lock the chosen behavior.
- Expiry finalization scores saved answers only.
- No client-supplied elapsed time is trusted.

## Published Content Validity

Starting a session requires the published exam version to contain valid startable content:

- At least one active `SingleBestAnswer` question.
- Every included question has at least two active answer options.
- Every included question has exactly one correct active answer option.
- Every included question has positive points.

Invalid published content must return `409 Conflict` because it is a content/configuration problem, not a nurse problem.

## Scoring Rules

- Phase 7A supports single-best-answer multiple choice only.
- Each question has a positive integer `Points`; default is `1`.
- A question earns full points only when the selected option is correct.
- Unanswered questions earn `0`.
- `Score` is the sum of earned points.
- `MaxScore` is the sum of question points in the session snapshot.
- `Percentage = Score / MaxScore * 100`, rounded to two decimal places for DTOs.
- `Passed = Percentage >= PassingScorePercentage`.
- Scoring runs immediately on submit and on expiry finalization.
- Once scored, result values are immutable.

## Access Rules Without Payments

Phase 7A must not implement real payment.

Access decision:

- Published free exams are startable by authenticated nurses with a nurse profile.
- Published non-free exams are startable only when an unexpired `ExamAccessGrant` exists for the nurse profile and exam.
- No checkout endpoint, order, payment provider, webhook, or subscription logic exists in Phase 7A.
- The entitlement model is intentionally payment-ready for later Phase 8 work.

## Validation and Error Behavior

- Missing JWT: `401 Unauthorized`.
- Authenticated non-nurse for nurse exam workflows: `403 Forbidden`.
- Nurse without `NurseProfile`: `403 Forbidden`.
- Draft/archived/retired/nonexistent exam start: `404 Not Found`.
- No access grant for non-free exam: `403 Forbidden`.
- Non-owned session id: `404 Not Found`.
- Invalid route GUID binding: `400 Bad Request`.
- Invalid request body or pagination: `400 Bad Request` validation Problem Details.
- Starting an exam with an existing own in-progress session for the same exam version returns the existing session instead of creating a duplicate.
- Starting an exam with an expired own in-progress session for the same exam version finalizes the expired session and starts a new one.
- Attempting to save/submit terminal sessions returns `409 Conflict`.
- Attempting to review in-progress sessions returns `409 Conflict`.

## Testing Requirements

Domain tests must cover:

- Exam/session default statuses.
- Exam/session terminal status helpers.
- Question/option correctness remains internal to domain.
- Session expiry calculation.

Application tests must cover:

- Catalog lists only published exams with published versions.
- Catalog access excludes non-free exams without grants or marks access accurately according to DTO decision.
- Start requires Nurse role and nurse profile.
- Start rejects invalid published content with `409 Conflict` when there are no active questions, fewer than two active options, not exactly one correct active option, or non-positive points.
- Start creates a session snapshot from the published version.
- Start returns existing own in-progress session for the same exam version.
- Start finalizes an expired own in-progress session and creates a new in-progress session.
- Session questions may include the nurse's saved selected answer option id before completion.
- Session questions exclude correct answers, correctness flags, explanations, score, percentage, and passed fields before completion.
- Save answers performs a partial upsert and leaves omitted existing answers unchanged.
- Save answers validates no duplicate `ExamSessionQuestionId`, no empty Guid values, and question/option ownership within the same owned session snapshot.
- Submit scores immediately and prevents later mutation.
- Expired sessions finalize with saved answers only.
- Get session finalizes expired-by-time in-progress sessions before returning.
- Result and review are scoped to the owner.
- Review exposes explanations only after completion.
- Attempt history pagination proves `Skip` and `Take`.
- DTO reflection tests reject sensitive/internal fields.

WebApi tests must cover:

- All endpoints require JWT.
- Nurse-only workflows return `403` through Application role guard behavior for non-nurses.
- Invalid route GUID returns `400` before `ISender.Send`.
- Validation Problem Details for invalid body/pagination.
- Conflict mapping for invalid transitions.
- Raw JSON security checks before deserialization for session, result, and review responses.
- Correct answers/explanations absent from in-progress session JSON.
- In-progress session JSON may include selected answer option id for the nurse's own saved answer.
- In-progress session JSON must not include correctness, explanation, score, percentage, or passed fields.
- Correct answers/explanations present only in completed review JSON.
- No permission setup is required for nurse-facing endpoints unless an admin endpoint is explicitly approved later.

Infrastructure tests must cover:

- Table names and primary keys.
- Required relationships and delete behavior.
- Status enum string storage.
- Max lengths for title, slug, text, and explanation fields.
- Unique indexes for slugs/version numbers.
- Indexes for catalog, access grant, session ownership, and attempt history queries.

## Performance and Indexing Considerations

Expected indexes:

- `ExamCategories(CountryId, DisplayOrder, Id)`
- `Exams(Status, CountryId, ExamCategoryId, Title, Id)`
- unique `Exams(Slug)`
- `ExamVersions(ExamId, Status, VersionNumber)`
- unique `ExamVersions(ExamId, VersionNumber)`
- `ExamQuestions(ExamVersionId, DisplayOrder, Id)`
- `ExamAnswerOptions(ExamQuestionId, DisplayOrder, Id)`
- `ExamAccessGrants(NurseProfileId, ExamId, ExpiresAt)`
- unique filtered `ExamSessions(NurseProfileId, ExamVersionId)` where `Status == 'InProgress'`
- `ExamSessions(NurseProfileId, StartedAt, Id)`
- `ExamSessionQuestions(ExamSessionId, DisplayOrder, Id)`
- `ExamSessionAnswers(ExamSessionQuestionId)` unique

The implementation plan must use EF migrations only and must not manually change schema.

## Acceptance Criteria

- Spec and plan choose Phase 7A Exam Foundation as first batch.
- No implementation starts during planning.
- No source, test, tracking, migration, frontend, payment, or recruitment files are modified during planning.
- Future implementation can create exam catalog/session/result/review workflows without exposing correct answers before completion.
- Access rules do not pretend real payment exists.
- State transitions, timer expiry, scoring, review, attempts, and admin/content boundaries are explicit.
- DTO security rules are testable through reflection and raw JSON tests.

## Reviewer Decisions Needed

None required before writing the Phase 7A implementation plan. The spec intentionally locks the safe default decisions needed for the first implementation batch.
