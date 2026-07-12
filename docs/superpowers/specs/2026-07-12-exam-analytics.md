# Phase 7C Exam Analytics Specification

## Objective

Define Phase 7C as backend-only exam analytics for authenticated nurses using the existing Phase 7A and Phase 7B examination data model.

Phase 7C gives a nurse insight into their own completed and expired exam performance without adding dashboards, frontend charts, payments, exports, recommendations, or platform-wide reporting. Analytics must be derived from existing `ExamSession`, `Exam`, `ExamCategory`, `Country`, and session snapshot data. It must not mutate exam sessions or expose answer keys.

This specification does not start implementation and does not modify source code, tests, migrations, tracking docs, frontend, payments, recruitment, or administration dashboards.

## Current Baseline

Phase 7A and Phase 7B already provide:

- Exam catalog and version entities.
- Timed nurse-owned exam sessions.
- Session score fields: `Score`, `MaxScore`, `Percentage`, `Passed`, `CorrectCount`, and `QuestionCount`.
- Session status values: `InProgress`, `Submitted`, `Expired`, and `Abandoned`.
- Immutable session question and answer option snapshots.
- Nurse role and ownership enforcement through existing Application guards.
- Nurse-facing attempt history under `/api/v1/me/nurse-profile/exam-attempts`.
- Admin/content management APIs for exam content.
- EF indexes for session ownership and attempt history by `NurseProfileId`, `StartedAt`, and `Id`.

Phase 7C should reuse that foundation and should not create analytics projections, persisted aggregates, or new taxonomy tables in this batch.

## In Scope

- Authenticated nurse-owned overall exam performance summary.
- Authenticated nurse-owned attempt status summary.
- Pass/fail counts and pass rate for counted sessions.
- Average, best, and latest score percentage for counted sessions.
- Performance breakdown by exam.
- Performance breakdown by country and category using existing exam metadata.
- Recent performance trend over deterministic time buckets.
- Completed and expired attempt metrics.
- Abandoned attempt count/rate as a status metric.
- DTOs, Application queries, validators, WebApi endpoints, and tests in the later implementation plan.

## Out of Scope

- Admin analytics.
- Platform-wide reports.
- Admin dashboards or reporting UI.
- Frontend charts.
- Payment analytics.
- Revenue analytics.
- Checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- Employer or recruitment analytics.
- CSV, Excel, or export endpoints.
- AI recommendations.
- Notifications or messaging.
- Weak-area analytics.
- Question taxonomy/tag schema.
- New persisted analytics projections.
- Materialized analytics tables.
- Mutating sessions, answers, or session snapshots.
- Changing nurse attempt, result, review, or exam-taking contracts.
- Phase 8 payments.
- Phase 9 dashboard/reporting work.

## Actors And Permissions

### Nurse

An authenticated user with the `Nurse` role and a `NurseProfile`.

Nurse may:

- View only their own exam analytics.
- Filter analytics by optional date range, country, category, or exam.
- View aggregate scores, pass rates, trends, and grouped performance summaries.

Nurse may not:

- View another nurse's analytics.
- View answer keys through analytics.
- View correct answers, `IsCorrect`, explanations, or selected answers for in-progress sessions through analytics.
- Access admin or platform-wide analytics.

### Admin / Content Manager

Admin and content-manager analytics are out of scope for Phase 7C. Existing Phase 7B admin content endpoints remain unchanged.

### Anonymous User

Anonymous users cannot access Phase 7C analytics endpoints.

## Data Sources

Phase 7C analytics must use existing persisted data:

- `ExamSessions` for attempt status, ownership, timing, scores, pass/fail, and finalization.
- `Exams` for exam title, country, category, duration, and stable exam identity.
- `ExamCategories` for category grouping and display names.
- `Countries` for country grouping and display names.
- `ExamSessionQuestions` only if needed for existing safe aggregate question counts already present in session-level scoring data.

Phase 7C must not read `ExamSessionAnswers` or `ExamSessionAnswerOptions` for core analytics. Weak-area analytics are deferred to a later approved phase/spec because they risk answer-level analysis and may need taxonomy/schema.

Phase 7C must not write to:

- `ExamSessions`
- `ExamSessionQuestions`
- `ExamSessionAnswerOptions`
- `ExamSessionAnswers`
- Exam content tables

## API Contract Proposal

All routes are under `/api/v1/me/nurse-profile/exam-analytics` and return DTOs, never EF/domain entities.

All endpoints require `.RequireAuthorization()`. Application handlers must enforce Nurse role and current nurse profile ownership.

### Summary

- `GET /api/v1/me/nurse-profile/exam-analytics/summary`
  - Name: `GetMyExamAnalyticsSummary`
  - Query filters: `from`, `to`, `countryId`, `categoryId`, `examId`
  - Returns overall nurse-owned performance metrics.

### By Exam

- `GET /api/v1/me/nurse-profile/exam-analytics/by-exam`
  - Name: `ListMyExamAnalyticsByExam`
  - Query filters: `from`, `to`, `countryId`, `categoryId`, `examId`, `page`, `pageSize`
  - Returns paginated grouped metrics per exam.

### By Category

- `GET /api/v1/me/nurse-profile/exam-analytics/by-category`
  - Name: `ListMyExamAnalyticsByCategory`
  - Query filters: `from`, `to`, `countryId`, `categoryId`, `page`, `pageSize`
  - Returns paginated grouped metrics per country/category pair.

### Trends

- `GET /api/v1/me/nurse-profile/exam-analytics/trends`
  - Name: `ListMyExamAnalyticsTrends`
  - Query filters: `from`, `to`, `countryId`, `categoryId`, `examId`, `bucket`
  - `bucket` values: `day`, `week`, `month`
  - Returns deterministic time-bucketed trend points.

### Recent Attempts

Do not add a Phase 7C `recent-attempts` endpoint unless implementation review shows the existing `/api/v1/me/nurse-profile/exam-attempts` endpoint cannot support the analytics screen. Current decision: omit this endpoint to avoid duplicating attempt history.

## DTO Proposal

### ExamAnalyticsSummaryDto

- `AttemptCount`
- `SubmittedCount`
- `ExpiredCount`
- `AbandonedCount`
- `InProgressCount`
- `CountedAttemptCount`
- `PassedCount`
- `FailedCount`
- `PassRatePercentage`
- `AverageScorePercentage`
- `BestScorePercentage`
- `LatestScorePercentage`
- `AverageScore`
- `AverageMaxScore`
- `AverageCorrectCount`
- `AverageQuestionCount`
- `FirstAttemptStartedAt`
- `LatestAttemptStartedAt`

### ExamAnalyticsByExamDto

- `ExamId`
- `ExamTitle`
- `CountryId`
- `CountryName`
- `CategoryId`
- `CategoryName`
- Same count and score metrics as summary where applicable.

### ExamAnalyticsByCategoryDto

- `CountryId`
- `CountryName`
- `CategoryId`
- `CategoryName`
- Same count and score metrics as summary where applicable.

### ExamAnalyticsTrendPointDto

- `BucketStart`
- `BucketEnd`
- `AttemptCount`
- `CountedAttemptCount`
- `PassedCount`
- `FailedCount`
- `PassRatePercentage`
- `AverageScorePercentage`
- `BestScorePercentage`

## DTO Security Rules

Analytics DTOs must not expose:

- `UserId`
- Account internals.
- Roles or permissions.
- Password hashes.
- Access tokens, refresh tokens, token hashes, or internal auth state.
- Payment provider ids or payment state.
- EF/domain entities.
- Navigation objects.
- Correct answer option ids.
- `IsCorrect` values.
- Explanations.
- Selected answer ids.
- Raw nurse answer rows.
- Session snapshot entities.

Analytics may expose aggregate scoring fields only for counted completed/expired sessions.

## Analytics Metric Definitions

### Attempt Count

`AttemptCount = SubmittedCount + ExpiredCount + AbandonedCount + InProgressCount`.

`AttemptCount` is the number of nurse-owned sessions matching filters, including `Submitted`, `Expired`, `Abandoned`, and `InProgress`. Summary score metrics must not use in-progress sessions.

### Counted Attempt Count

`CountedAttemptCount = SubmittedCount + ExpiredCount`.

`CountedAttemptCount` includes only `Submitted` and `Expired` sessions. These sessions count toward score, pass/fail, average, best, latest, trend score, and group score metrics.

### Submitted Count

Number of matching sessions with `Status == Submitted`.

### Expired Count

Number of matching sessions with `Status == Expired`.

### Abandoned Count

Number of matching sessions with `Status == Abandoned`. Abandoned sessions count in attempt-volume and abandoned-rate metrics only. They do not count in score, pass/fail, average, best, latest, trend score, or grouped score metrics.

### In Progress Count

Number of matching sessions with `Status == InProgress`.

In-progress sessions count only in `InProgressCount`, `AttemptCount`, and status/attempt-volume metrics. They must never count in score, pass/fail, average, best, latest, trend score, or grouped score metrics.

### Passed Count And Failed Count

For counted sessions:

- `PassedCount` is the number of counted sessions where `Passed == true`.
- `FailedCount` is the number of counted sessions where `Passed == false`.

### Pass Rate

`PassRatePercentage = PassedCount / CountedAttemptCount * 100`.

If `CountedAttemptCount == 0`, pass rate is `null`.

### Average Score Percentage

Average of `ExamSession.Percentage` over counted sessions.

If no counted sessions exist, average score percentage is `null`.

### Best Score Percentage

Maximum `ExamSession.Percentage` over counted sessions.

If no counted sessions exist, best score percentage is `null`.

### Latest Score Percentage

The `Percentage` from the latest counted session ordered by `StartedAt` descending, then `Id` descending.

If no counted sessions exist, latest score percentage is `null`.

### Recent Performance Trend

Trend points group matching sessions by server-stored UTC `StartedAt` date into `day`, `week`, or `month` buckets. Buckets must be deterministic and sorted by `BucketStart` ascending.

For each trend bucket:

- `AttemptCount`: all matching sessions in the bucket, including `Submitted`, `Expired`, `Abandoned`, and `InProgress`.
- `CountedAttemptCount`: only `Submitted` and `Expired` sessions in the bucket.
- Score/pass metrics are computed only from `CountedAttemptCount`.
- `InProgress` must never count in score/pass/average/best/latest/trend score metrics.

Bucket boundaries:

- Day bucket starts at 00:00:00 UTC.
- Week bucket starts Monday 00:00:00 UTC.
- Month bucket starts first day of month 00:00:00 UTC.
- `BucketEnd` is exclusive.

### Performance By Exam

Groups counted and status metrics by `ExamId` and safe exam display fields.

### Performance By Category

Groups counted and status metrics by `CountryId`, `CountryName`, `CategoryId`, and `CategoryName`. Exams without a category must be grouped under a null `CategoryId` with a null or empty `CategoryName`.

## Session Status Inclusion Rules

- `Submitted`: counts in `SubmittedCount`, `AttemptCount`, `CountedAttemptCount`, score, pass/fail, best, latest, trend score, by-exam, by-category, and summary metrics.
- `Expired`: counts in `ExpiredCount`, `AttemptCount`, `CountedAttemptCount`, score, pass/fail, best, latest, trend score, by-exam, by-category, and summary metrics because expiry finalization scores saved answers.
- `InProgress`: counts in `InProgressCount`, `AttemptCount`, and status/attempt-volume metrics only. It must never count in score/pass/average/best/latest/trend score metrics.
- `Abandoned`: counts in `AbandonedCount`, `AttemptCount`, and abandoned count/rate metrics only. It does not count in `CountedAttemptCount` or score/pass/average/best/latest/trend score metrics.

## Filters And Pagination Rules

Supported filters:

- `from`: optional UTC date/time lower bound on `StartedAt`, inclusive.
- `to`: optional UTC date/time upper bound on `StartedAt`, inclusive.
- `countryId`: optional exam country filter.
- `categoryId`: optional exam category filter.
- `examId`: optional exam filter.

Validation:

- `from <= to` when both are supplied.
- Guid filters must be valid GUID values through WebApi binding/model validation.
- `page >= 1`.
- `pageSize` is between `1` and `100`.
- `bucket` must be one of `day`, `week`, or `month`.

Pagination:

- `summary` is not paginated.
- `by-exam` is paginated.
- `by-category` is paginated.
- `trends` is not paginated in Phase 7C; validators must cap the date range to prevent unbounded daily buckets.

Default date range:

- If no date range is supplied, analytics use all available nurse-owned historical sessions.
- The implementation plan may choose a maximum trend date span for daily buckets and must lock it with validators/tests.

## Error Behavior

- Missing JWT: `401 Unauthorized`.
- Authenticated user without Nurse role or without a nurse profile: `403 Forbidden`.
- Invalid route/query GUID binding: `400 Bad Request`.
- Invalid request filters or pagination: `400 Bad Request` validation Problem Details.
- Filtered exam/category/country not found or not applicable to the nurse: return empty analytics rather than exposing existence details.
- Unexpected exceptions: existing `500` Problem Details behavior.

## Performance And Indexing Considerations

Phase 7C should use existing indexes first:

- `ExamSessions(NurseProfileId, StartedAt, Id)` supports owner-scoped attempt and trend queries.
- Existing exam catalog indexes support joins to exams, categories, and countries.
- Existing session status storage supports filtering by status after nurse ownership is applied.

Implementation should:

- Start all analytics queries from current nurse-owned `ExamSessions`.
- Join to exams/categories/countries only after ownership filtering.
- Do not read `ExamSessionAnswers` or `ExamSessionAnswerOptions` for Phase 7C core implementation.
- Avoid loading session snapshots for Phase 7C analytics.
- Avoid materialized analytics tables in Phase 7C.
- Avoid N+1 queries in grouped endpoints.
- Review generated LINQ for predictable server-side execution.

If evidence shows a missing index or query path, stop and report before adding a migration. No Phase 7C migration is expected.

## Migration Decision

No migration is expected for Phase 7C.

Existing Phase 7A schema contains the required analytics fields:

- `ExamSession.Status`
- `ExamSession.StartedAt`
- `ExamSession.FinalizedAt`
- `ExamSession.Score`
- `ExamSession.MaxScore`
- `ExamSession.Percentage`
- `ExamSession.Passed`
- `ExamSession.CorrectCount`
- `ExamSession.QuestionCount`
- `Exam.CountryId`
- `Exam.ExamCategoryId`
- `Exam.Title`
- `ExamCategory.Name`
- `Country.Name`

New persisted projections, materialized views, reporting tables, taxonomy/tag tables, or denormalized analytics columns are out of scope unless separately approved.

## Testing Requirements

Application tests must cover:

- Nurse role and nurse profile ownership enforcement.
- Summary counts for `Submitted`, `Expired`, `Abandoned`, and `InProgress`.
- `AttemptCount = SubmittedCount + ExpiredCount + AbandonedCount + InProgressCount`.
- `CountedAttemptCount = SubmittedCount + ExpiredCount`.
- `InProgress` is counted only in status/attempt volume and excluded from score/pass metrics.
- Score/pass metrics include `Submitted` and `Expired`.
- Score/pass metrics exclude `InProgress` and `Abandoned`.
- Empty data returns zero counts and null score rates where appropriate.
- Average, best, latest, pass rate, and grouped metrics are deterministic.
- By-exam filtering and pagination prove skip and take.
- By-category grouping handles null category.
- Date range filtering is inclusive.
- Country/category/exam filters apply after ownership.
- Trend buckets are sorted and deterministic.
- Trend buckets expose `AttemptCount` for all statuses and `CountedAttemptCount` for `Submitted` plus `Expired`.
- Trend score/pass metrics are computed only from `CountedAttemptCount`.
- A trend bucket containing `Submitted`, `Expired`, `Abandoned`, and `InProgress` reports all statuses in `AttemptCount` while excluding `Abandoned` and `InProgress` from score/pass metrics.
- Day buckets start at 00:00:00 UTC, week buckets start Monday 00:00:00 UTC, month buckets start first day of month 00:00:00 UTC, and `BucketEnd` is exclusive.
- Week and month bucket ordering is deterministic.
- Validators reject invalid date ranges, invalid pagination, and invalid bucket values.
- DTO reflection tests reject sensitive/internal fields.

WebApi integration tests must cover:

- Every analytics endpoint returns `401` without JWT.
- Endpoints use `.RequireAuthorization()` only and do not require admin permissions.
- Authenticated nurse requests send the expected Application queries.
- Invalid query values return `400`.
- Raw JSON does not expose forbidden fields, answer keys, explanations, selected answers, account internals, payment fields, roles, permissions, or EF/domain navigation objects.
- Existing in-progress session response secrecy remains unchanged.

Infrastructure tests are not expected unless EF configuration changes are proposed.

## Acceptance Criteria

- Spec and implementation plan exist for Phase 7C.
- No implementation starts during planning.
- Phase 7C is backend-only nurse-owned analytics.
- All analytics are scoped to the authenticated nurse.
- `Submitted` and `Expired` count in score/pass metrics.
- `InProgressCount` is explicit in summary metrics.
- `AttemptCount = SubmittedCount + ExpiredCount + AbandonedCount + InProgressCount`.
- `CountedAttemptCount = SubmittedCount + ExpiredCount`.
- `InProgress` and `Abandoned` do not count in score/pass metrics.
- Trend `AttemptCount` includes all statuses, while trend `CountedAttemptCount` and score/pass metrics include only `Submitted` and `Expired`.
- Deterministic bucket boundaries use UTC day, Monday-start week, first-day month, and exclusive `BucketEnd`.
- Analytics use existing Phase 7A/7B data.
- Analytics do not mutate sessions or snapshots.
- Analytics do not expose answer keys, explanations, selected answers, or account/payment internals.
- Analytics endpoints are under `/api/v1/me/nurse-profile/exam-analytics/...`.
- Analytics endpoints require `.RequireAuthorization()`.
- Application handlers enforce Nurse role and ownership.
- No migration is expected.
- No frontend, payments, exports, AI recommendations, admin analytics, or platform-wide dashboards are included.

## Reviewer Decisions Needed

None. This spec locks the safe Phase 7C defaults:

- Nurse-owned analytics only.
- No admin analytics.
- No migration.
- No recent-attempts duplication.
- Weak-area analytics are deferred; no weak-area taxonomy/schema in Phase 7C.
- Existing session data as the source of truth.
