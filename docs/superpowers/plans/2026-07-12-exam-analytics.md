# Phase 7C Exam Analytics Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement backend-only nurse-owned exam analytics over existing Phase 7A/7B exam sessions and catalog data.

**Architecture:** Phase 7C adds Application CQRS queries, explicit analytics DTOs, validators, aggregation helpers, and thin Minimal API endpoints under `/api/v1/me/nurse-profile/exam-analytics`. Handlers start from the authenticated nurse profile, aggregate existing `ExamSession` data, join safe exam/category/country metadata, and never mutate session snapshots or expose answer keys.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, MockQueryable.Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Execute only Phase 7C Exam Analytics.
- Implement the approved spec exactly: `docs/superpowers/specs/2026-07-12-exam-analytics.md`.
- Do not implement admin analytics.
- Do not implement platform-wide reports.
- Do not implement Phase 8 payments.
- Do not implement Phase 9 dashboards or reporting UI.
- Do not add frontend charts.
- Do not add checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- Do not implement CSV, Excel, or export endpoints.
- Do not implement AI recommendations.
- Do not implement weak-area analytics in Phase 7C.
- Do not implement notifications or messaging.
- Do not modify recruitment, contact requests, candidates, employers, or nurse profile behavior.
- Do not modify nurse exam-taking, attempt, result, or review contracts except compatibility-safe tests.
- Do not mutate `ExamSession`, `ExamSessionQuestion`, `ExamSessionAnswerOption`, or `ExamSessionAnswer`.
- Do not expose answer keys, correct answer ids, `IsCorrect`, explanations, selected answers, raw answers, account internals, roles, permissions, tokens, password hashes, payment provider ids, payment state, EF entities, domain entities, or navigation objects.
- Do not read `ExamSessionAnswers` or `ExamSessionAnswerOptions` for Phase 7C core implementation.
- Use existing `NurseRoleGuard` and current nurse profile ownership.
- All Phase 7C endpoints require `.RequireAuthorization()` only.
- Do not add `RequirePermission`, `AllowAnonymous`, or new permissions.
- No migration is expected. Stop and report if implementation requires schema/index changes.

---

## Chosen Implementation Batch Scope

Phase 7C is one backend-only batch because the endpoints share the same nurse-owned analytics filter model, status rules, metric definitions, and DTO security requirements. It includes summary, by-exam, by-category, and trend analytics. It excludes recent-attempt duplication, weak-area analytics, weak-area taxonomy, admin analytics, dashboards, exports, frontend, payments, imports, notifications, and recruitment work.

## Planned File Structure

Application:

- Create `backend/src/NursingPlatform.Application/Exams/Analytics/DTOs/ExamAnalyticsSummaryDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/DTOs/ExamAnalyticsByExamDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/DTOs/ExamAnalyticsByCategoryDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/DTOs/ExamAnalyticsTrendPointDto.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Common/ExamAnalyticsFilters.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Common/ExamAnalyticsMetricCalculator.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/GetMyExamAnalyticsSummary/GetMyExamAnalyticsSummaryQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/GetMyExamAnalyticsSummary/GetMyExamAnalyticsSummaryQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/GetMyExamAnalyticsSummary/GetMyExamAnalyticsSummaryQueryValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsByExam/ListMyExamAnalyticsByExamQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsByExam/ListMyExamAnalyticsByExamQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsByExam/ListMyExamAnalyticsByExamQueryValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsByCategory/ListMyExamAnalyticsByCategoryQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsByCategory/ListMyExamAnalyticsByCategoryQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsByCategory/ListMyExamAnalyticsByCategoryQueryValidator.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsTrends/ListMyExamAnalyticsTrendsQuery.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsTrends/ListMyExamAnalyticsTrendsQueryHandler.cs`.
- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/ListMyExamAnalyticsTrends/ListMyExamAnalyticsTrendsQueryValidator.cs`.

Infrastructure:

- Review `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`.
- Do not modify EF configuration or migrations unless a reviewer-approved schema/index gap is discovered.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` to map Phase 7C nurse analytics endpoints.

Tests:

- Create `backend/tests/NursingPlatform.Application.Tests/Exams/Analytics/ExamAnalyticsDtoSecurityTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Exams/Analytics/ExamAnalyticsValidatorTests.cs`.
- Create `backend/tests/NursingPlatform.Application.Tests/Exams/Analytics/ExamAnalyticsHandlerTests.cs`.
- Create `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ExamAnalyticsEndpointsTests.cs`.
- Update existing nurse exam endpoint tests only if needed to prove compatibility-safe pre-completion secrecy remains unchanged.

Tracking:

- Modify `CURRENT_TASK.md` and `TASKS.md` only after final verification passes and reviewer approves tracking update step.

## Migration Decision

No migration is expected. Phase 7C should compute analytics from existing Phase 7A/7B tables and indexes:

- `ExamSessions`
- `Exams`
- `ExamCategories`
- `Countries`

If implementation evidence shows a required new index, projection table, materialized view, taxonomy table, or denormalized analytics column, stop and report. Do not create a migration in Phase 7C without explicit approval.

## Task 1: Application Analytics DTOs, Query Contracts, Validators, And DTO Security Tests

**Goal:** Add analytics contracts and validators without implementing aggregation handlers or WebApi endpoints.

**Files:**

- Create Application DTO, filter, query, and validator files listed in Planned File Structure.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/Analytics/ExamAnalyticsDtoSecurityTests.cs`.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/Analytics/ExamAnalyticsValidatorTests.cs`.

**Interfaces produced:**

- `ExamAnalyticsSummaryDto`
  - Includes `InProgressCount`.
  - Defines `AttemptCount = SubmittedCount + ExpiredCount + AbandonedCount + InProgressCount`.
  - Defines `CountedAttemptCount = SubmittedCount + ExpiredCount`.
- `ExamAnalyticsByExamDto`
- `ExamAnalyticsByCategoryDto`
- `ExamAnalyticsTrendPointDto`
  - Includes `AttemptCount` for all bucket sessions.
  - Includes `CountedAttemptCount` for `Submitted` and `Expired` bucket sessions only.
- `ExamAnalyticsFilters`
- `ExamAnalyticsBucket` with values `Day`, `Week`, `Month`
- `GetMyExamAnalyticsSummaryQuery`
- `ListMyExamAnalyticsByExamQuery`
- `ListMyExamAnalyticsByCategoryQuery`
- `ListMyExamAnalyticsTrendsQuery`

**Tests to write first:**

- `ExamAnalyticsDtos_ShouldNotExposeAccountInternalsPaymentFieldsOrAnswerKeys`
- `ExamAnalyticsDtos_ShouldExposeAggregateMetricsOnly`
- `Validate_Summary_WithFromAfterTo_ShouldHaveError`
- `Validate_ByExam_WithInvalidPagination_ShouldHaveError`
- `Validate_ByCategory_WithInvalidPagination_ShouldHaveError`
- `Validate_Trends_WithInvalidBucket_ShouldHaveError`
- `Validate_Trends_WithTooLargeDailyRange_ShouldHaveError`
- `Validate_Filters_WithEmptyGuidValues_ShouldHaveError`

**Implementation notes:**

- DTOs expose aggregate numbers and safe exam/category/country display fields only.
- DTOs must not expose `UserId`, `Email`, `PasswordHash`, `Roles`, `Permissions`, `AccessToken`, `RefreshToken`, `TokenHash`, `PaymentProviderId`, `PaymentIntentId`, `OrderId`, `User`, `NurseProfile`, `ExamSession`, `ExamSessionAnswer`, `ExamSessionQuestion`, `ExamSessionAnswerOption`, `SelectedExamSessionAnswerOptionId`, `CorrectAnswerOptionId`, `IsCorrect`, `Explanation`, or navigation objects.
- Shared filters include nullable `From`, `To`, `CountryId`, `CategoryId`, and `ExamId`.
- `ByExam` and `ByCategory` queries include `Page` and `PageSize`.
- `Trends` includes `Bucket`, defaulting to `Month`.
- Validators enforce `Page >= 1`, `PageSize` between `1` and `100`, `From <= To`, non-empty optional GUID filters, and valid bucket values.
- Lock a daily trend maximum range of 366 days in validators/tests to prevent unbounded daily buckets.
- Do not add `WeakAreaSummaryDto` in Phase 7C; weak-area analytics are deferred to a later approved phase/spec because they risk answer-level analysis and may need taxonomy/schema.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ExamAnalyticsDto|ExamAnalyticsValidator"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after Task 1 if not in approved batch execution. Do not add aggregation handlers or endpoints in this task.

## Task 2: Application Analytics Handlers And Tests

**Goal:** Implement nurse-owned analytics aggregation over existing exam sessions and safe exam metadata.

**Files:**

- Create `backend/src/NursingPlatform.Application/Exams/Analytics/Common/ExamAnalyticsMetricCalculator.cs`.
- Create handler files under `backend/src/NursingPlatform.Application/Exams/Analytics/Queries/`.
- Test `backend/tests/NursingPlatform.Application.Tests/Exams/Analytics/ExamAnalyticsHandlerTests.cs`.

**Tests to write first:**

- `Handle_Summary_WhenNurseProfileMissing_ThrowsForbiddenAccessException`
- `Handle_Summary_IncludesOnlyCurrentNurseSessions`
- `Handle_Summary_CountsSubmittedExpiredAbandonedAndInProgressStatuses`
- `Handle_Summary_AttemptCountEqualsSubmittedExpiredAbandonedAndInProgress`
- `Handle_Summary_CountedAttemptCountEqualsSubmittedAndExpired`
- `Handle_Summary_ScoreMetricsIncludeSubmittedAndExpiredOnly`
- `Handle_Summary_ExcludesInProgressAndAbandonedFromScoreAndPassMetrics`
- `Handle_Summary_InProgressCountsOnlyInStatusAndAttemptVolume`
- `Handle_Summary_WithNoCountedAttempts_ReturnsZeroCountsAndNullRates`
- `Handle_Summary_ComputesAverageBestLatestAndPassRate`
- `Handle_Summary_AppliesInclusiveDateRange`
- `Handle_Summary_AppliesCountryCategoryAndExamFiltersAfterOwnership`
- `Handle_ByExam_GroupsByExamWithSafeMetadata`
- `Handle_ByExam_PaginatesAndProvesSkipAndTake`
- `Handle_ByCategory_GroupsNullCategorySafely`
- `Handle_Trends_GroupsByMonthInAscendingBucketOrder`
- `Handle_Trends_GroupsByWeekDeterministically`
- `Handle_Trends_UsesMondayUtcWeekStartAndExclusiveBucketEnd`
- `Handle_Trends_UsesFirstDayUtcMonthStartAndExclusiveBucketEnd`
- `Handle_Trends_GroupsByDayWithinAllowedRange`
- `Handle_Trends_BucketWithAllStatusesCountsAttemptAndCountedAttemptSeparately`
- `Handle_Analytics_DoesNotReadOrMutateSessionSnapshotAnswersForCoreMetrics`

**Implementation notes:**

- Reuse `ExamHandlerHelpers.GetCurrentNurseProfileIdAsync()` or the same `NurseRoleGuard` and nurse-profile resolution pattern.
- Start queries from `_context.ExamSessions.Where(s => s.NurseProfileId == currentNurseProfileId)`.
- Apply date and exam filters to sessions before grouping.
- Join `Exams`, `ExamCategories`, and `Countries` after ownership filtering.
- Counted sessions are `Submitted` and `Expired`.
- `AttemptCount = SubmittedCount + ExpiredCount + AbandonedCount + InProgressCount`.
- `CountedAttemptCount = SubmittedCount + ExpiredCount`.
- `InProgress` counts only in `InProgressCount`, `AttemptCount`, and status/attempt-volume metrics.
- `InProgress` must never count in score/pass/average/best/latest/trend score metrics.
- `Abandoned` counts only in `AbandonedCount`, `AttemptCount`, and abandoned count/rate metrics.
- `Abandoned` is excluded from `CountedAttemptCount` and score/pass metrics.
- `Abandoned` is included only in `AbandonedCount`, `AttemptCount`, and status/attempt-volume metrics.
- Empty aggregates return zero counts and null rate/score percentages.
- Latest score uses counted sessions ordered by `StartedAt` descending, then `Id` descending.
- Trend bucket sorting is by `BucketStart` ascending.
- Day bucket starts at 00:00:00 UTC.
- Week bucket starts Monday 00:00:00 UTC.
- Month bucket starts first day of month 00:00:00 UTC.
- `BucketEnd` is exclusive.
- Trend `AttemptCount` includes all matching `Submitted`, `Expired`, `Abandoned`, and `InProgress` sessions in the bucket.
- Trend `CountedAttemptCount` includes only `Submitted` and `Expired` sessions in the bucket.
- Trend score/pass metrics are computed only from `CountedAttemptCount`.
- Avoid loading `ExamSessionAnswers` or `ExamSessionAnswerOptions` for core metrics.
- Throw `ForbiddenAccessException` when a current nurse profile is missing.
- Do not expose existence of another nurse's attempts through filters; filters with no owned data return empty analytics.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "ExamAnalytics"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after Task 2 if not in approved batch execution. Do not add WebApi endpoints before Application tests pass.

## Task 3: Infrastructure And Index Review

**Goal:** Confirm existing Phase 7A/7B schema and indexes support Phase 7C analytics without a migration.

**Files:**

- Review `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/ExamConfigurations.cs`.
- Review `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`.
- Create no files unless a reviewer-approved infrastructure change is required.

**Tests to write first if infrastructure changes are proposed:**

- `ExamAnalyticsConfiguration_UsesExistingSessionOwnershipAttemptIndex`
- `ExamAnalyticsConfiguration_DoesNotRequireAnalyticsProjectionTable`

**Implementation notes:**

- Default outcome is no infrastructure change and no migration.
- Existing `ExamSessions(NurseProfileId, StartedAt, Id)` supports owner-scoped attempt/trend queries.
- Existing exam/category/country relationships support safe metadata grouping.
- If a new index, projection, or schema object appears necessary, stop and report before modifying infrastructure.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "ExamConfiguration"`
- `dotnet build backend/NursingPlatform.slnx`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop if a migration or EF configuration change appears necessary but was not explicitly approved.

## Task 4: WebApi Endpoints And Integration Tests

**Goal:** Add only the approved Phase 7C nurse analytics endpoints and prove auth, query binding, and raw JSON security.

**Files:**

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`.
- Test `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/ExamAnalyticsEndpointsTests.cs`.

**Endpoint groups to add:**

- `GET /api/v1/me/nurse-profile/exam-analytics/summary`
- `GET /api/v1/me/nurse-profile/exam-analytics/by-exam`
- `GET /api/v1/me/nurse-profile/exam-analytics/by-category`
- `GET /api/v1/me/nurse-profile/exam-analytics/trends`

**Route names:**

- `GetMyExamAnalyticsSummary`
- `ListMyExamAnalyticsByExam`
- `ListMyExamAnalyticsByCategory`
- `ListMyExamAnalyticsTrends`

**Tests to write first:**

- `ExamAnalyticsEndpoints_WithoutJwt_ReturnUnauthorized`
- `ExamAnalyticsEndpoints_UseRequireAuthorizationOnly_WithoutPermissionSetup`
- `GetSummary_WithFilters_SendsQuery`
- `ListByExam_WithPaginationAndFilters_SendsQuery`
- `ListByCategory_WithPaginationAndFilters_SendsQuery`
- `ListTrends_WithBucketAndFilters_SendsQuery`
- `ExamAnalyticsEndpoints_WithInvalidGuidFilter_ReturnBadRequestAndSenderNotCalled`
- `ExamAnalyticsValidationFailure_ReturnsValidationProblemDetails`
- `ExamAnalyticsForbiddenAccess_ReturnsForbidden`
- `ExamAnalyticsJson_DoesNotExposeForbiddenFields`
- `NurseInProgressExamSessionJson_StillDoesNotExposeCorrectAnswersOrScoring`

**Implementation notes:**

- Add endpoints under the existing `nurseProfile` group if practical.
- Use `.RequireAuthorization()` inherited from the existing `/me/nurse-profile` group.
- Do not add `.RequirePermission(...)`.
- Do not add `.AllowAnonymous()`.
- Do not add admin routes.
- Query parameters: `from`, `to`, `countryId`, `categoryId`, `examId`, `page`, `pageSize`, and `bucket` where applicable.
- Invalid GUID query values should use existing Minimal API/WebApi binding and BadHttpRequestException behavior.
- Raw JSON tests must inspect response content before DTO deserialization.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "ExamAnalytics|ExamEndpoints"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after focused WebApi tests and build pass if not in approved batch execution.

## Task 5: Final Verification And Security/Performance Review

**Goal:** Verify Phase 7C end-to-end and confirm no out-of-scope behavior or unsafe exposure was introduced.

**Files:**

- No file modifications expected unless verification exposes an approved correction.

**Review checklist:**

- Confirm no frontend files changed.
- Confirm no payment/order/checkout/webhook files changed.
- Confirm no recruitment/contact-request/candidate/employer files changed.
- Confirm no admin analytics or dashboard/reporting endpoints were added.
- Confirm no export endpoints were added.
- Confirm analytics DTOs do not expose answer keys, selected answers, explanations, account internals, payment state, or EF/domain navigation objects.
- Confirm analytics handlers start from current nurse ownership.
- Confirm `Submitted` and `Expired` count in score/pass metrics.
- Confirm `AttemptCount = SubmittedCount + ExpiredCount + AbandonedCount + InProgressCount`.
- Confirm `CountedAttemptCount = SubmittedCount + ExpiredCount`.
- Confirm `InProgress` and `Abandoned` do not count in score/pass metrics.
- Confirm `InProgress` is counted only in status/attempt volume.
- Confirm trend buckets count all statuses in `AttemptCount`, count only `Submitted` and `Expired` in `CountedAttemptCount`, and compute score/pass metrics only from `CountedAttemptCount`.
- Confirm day/week/month bucket boundaries are deterministic, UTC-based, and use exclusive `BucketEnd`.
- Confirm session snapshots are not mutated.
- Confirm no migration was created.

**Verification commands:**

- `dotnet build backend/NursingPlatform.slnx`
- `dotnet test backend/NursingPlatform.slnx`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --stat`
- `git diff --cached --stat`

**Stop condition:** Stop if build, tests, security review, performance review, or EF model check fails after two fix cycles.

## Task 6: Tracking Documentation Update

**Goal:** Update project tracking only after Phase 7C implementation and final verification pass.

**Files:**

- Modify `CURRENT_TASK.md`.
- Modify `TASKS.md`.

**Implementation notes:**

- Mark only Phase 7C Exam Analytics complete.
- Phase 7 analytics may be marked complete only if TASKS.md has no other open Phase 7 analytics items.
- Do not start Phase 8.
- Do not mention payments, frontend/admin UI, exports, AI recommendations, admin dashboards, platform-wide reports, or recruitment analytics as implemented.

**Verification commands:**

- `git diff -- CURRENT_TASK.md TASKS.md`
- `git status --short --untracked-files=all`
- `git diff --cached --stat`

**Stop condition:** Stop after tracking diff and git status are reported if not in approved batch execution.

## Task 7: Final Commit

**Goal:** Commit approved Phase 7C implementation, tests, and tracking files only after explicit reviewer approval or approved batch execution.

**Files:**

- Stage only approved Phase 7C files explicitly.
- Do not stage `codex_res/codex_report.md`.
- Do not stage `AGENTS.md`, `PROJECT_RULES.md`, `.gitignore`, frontend files, payment files, recruitment files, admin dashboard/reporting files, migrations, or unrelated docs.

**Commit strategy:**

- Planning commit, if requested separately: `docs: add exam analytics plan`.
- Implementation commit: `feat: add exam analytics`

**Verification commands:**

- `git status --short --untracked-files=all`
- `git diff --cached --name-only`
- `git diff --cached --stat`
- `git commit -m "feat: add exam analytics"`
- `git status --short --untracked-files=all`
- `git log -1 --oneline`

**Stop condition:** Stop after reporting commit hash/message, post-commit status, and latest log line. Do not proceed to Phase 8 payments, Phase 9 dashboards/reports, frontend charts, exports, AI recommendations, notifications, or recruitment work.

## Explicit Out-of-Scope Guardrails

- No admin analytics.
- No platform-wide reports.
- Weak-area analytics are deferred to a later approved phase/spec.
- No Phase 8 payments.
- No Phase 9 dashboard/reporting UI.
- No frontend charts.
- No checkout, orders, payment providers, subscriptions, refunds, or webhooks.
- No CSV, Excel, or export endpoints.
- No AI recommendations.
- No notifications or messaging.
- No exam access grant management.
- No imports or bulk upload.
- No recruitment changes.
- No nurse exam-taking contract changes except compatibility-safe tests.
- No answer-key exposure.
- No session snapshot mutation.
- No migration unless explicitly approved after a stop report.

## Self-Review Checklist

- Spec coverage: Plan covers summary, by-exam, by-category, trends, filters, status rules, DTO security, ownership, tests, verification, tracking, and commit.
- Scope check: Plan is Phase 7C backend nurse analytics only.
- Permission check: Plan uses `.RequireAuthorization()` and Application nurse ownership, not admin permissions.
- Migration check: Plan expects no migration and requires a stop if schema/index changes become necessary.
- Security check: Analytics exposes aggregates only and does not expose answer keys, selected answers, explanations, account internals, payment state, or EF/domain objects.
- Weak-area check: weak-area analytics are deferred, and Phase 7C does not read `ExamSessionAnswers` or `ExamSessionAnswerOptions` for core implementation.
- Placeholder scan: No unresolved placeholders are intentionally left in this plan.
