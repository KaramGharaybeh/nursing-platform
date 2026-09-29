# Exams Screen Approval Packet — T-FE-061 / GATE-FE-T061

```yaml
document_id: NPS-DES-INV-EXAMS-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-18
prepared_by: T-FE-061 documentation closure campaign (feat/2026-09-16-nurse-profile-overview)
gate: GATE-FE-T061
gate_status: VERIFIED (closed by human-approved decisions E61-1–E61-13 below)
authorization: Decisions E61-1–E61-13 were approved
  by the human technical lead on 2026-09-18. EXM-001/002 (T-FE-067),
  EXM-003/004 (T-FE-068), and EXM-005/006 (T-FE-069, including the transient
  post-submit minimum) are APPROVED as design authority. Approval does not
  itself start implementation; T-FE-067 remains NOT STARTED until separately
  authorized, and T-FE-068/T-FE-069 remain gated behind their own gates.
  Decisions E71-1–E71-9 were approved by the human technical lead on
  2026-09-19 as EXM-007/T-FE-071 design/source-ownership authority. Approval
  does not itself start T-FE-071 implementation.
  Decisions E72-1–E72-14 were approved by the human technical lead on
  2026-09-19 as EXM-009/T-FE-072 design/source-ownership authority. Approval
  does not itself start T-FE-072 implementation.
  Decisions E74-1–E74-14 were approved by the human technical lead on
  2026-09-19 as EXM-008/T-FE-074 product/design/source-ownership authority.
  Approval does not itself start T-FE-074 implementation.
  Decisions E73-1–E73-12 were approved by the human technical lead on
  2026-09-19 as EXM-010/T-FE-073 design/source-ownership authority. Approval
  does not itself start T-FE-073 implementation.
```

## 1. Purpose

This is the T-FE-061 Exams screen approval packet covering EXM-001..006 plus
the explicitly bounded EXM-007 transient minimum. It establishes the family
architecture, per-screen backend contracts, required states, field and
non-exposure authority, visual-foundation binding, approved copy, and the
explicit open design decisions (none remain open: E61-1–E61-13 are all
HUMAN_APPROVED below). Per the frontend ledger, GATE-FE-T061 requires an
"Exams approval packet with decisions per screen".

## 2. Screen inventory

| Screen | Route | Owner | Backend contract | Status | Approval |
|---|---|---|---|---|---|
| Exam catalog (EXM-001) | `/exams` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-067` | `GET /api/v1/exams` (page/pageSize/countryId/categoryId), `PaginatedResult<ExamCatalogItemDto>`, startable-only, fixed order | NOT STARTED | APPROVED (E61-1, E61-2, E61-4; browse-only; no search/sort/commerce) |
| Exam detail (EXM-002) | `/exams/:examId` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-067` | `GET /api/v1/exams/{id}` → `ExamDetailDto` (+`Instructions`), 404 when absent/unstartable-context | NOT STARTED | APPROVED (E61-1, E61-3, E61-4, E61-5; metadata only; purchase CTA deferred to commerce) |
| Purchase required (EXM-003) | INLINE / DEFERRED STATE, NO ROUTE | `T-FE-068` | `IsFree`/`CanStart` truth + grant policy (backend) | NOT STARTED | APPROVED as inline factual state only (E61-8); no routable page, no purchase action |
| Exam instructions/start (EXM-004) | `/exams/:examId/instructions` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-068` | `ExamDetailDto` facts + `POST /api/v1/exams/{id}/sessions` (200/404/409) | NOT STARTED | APPROVED (E61-6, E61-7; confirmation + resume routing) |
| Exam session (EXM-005) | `/exams/:examId/sessions/:sessionId` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-069` | `GET/PUT/POST /api/v1/exam-sessions/{id}[...]` session contracts | NOT STARTED | APPROVED (E61-9–E61-11, E61-13; single-question pager, explicit save, countdown) |
| Submit confirmation (EXM-006) | TRANSIENT WITHIN EXM-005, NO ROUTE | `T-FE-069` | `POST /api/v1/exam-sessions/{id}/submit` → `ExamSessionResultDto` | NOT STARTED | APPROVED (E61-12; transient summary only) |
| Full result (EXM-007) | `/exams/:examId/sessions/:sessionId/result` (APPROVED_CANONICAL) | `T-FE-071` | `GET .../result` | NOT STARTED | HUMAN_APPROVED (E71-1–E71-9; source-agnostic aggregate result only; no review CTA) |
| Analytics (EXM-008) | `/exams/analytics` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-074` | `GET /me/nurse-profile/exam-analytics/{summary,by-exam,by-category,trends}` | NOT STARTED | HUMAN_APPROVED (E74-1–E74-14; historical learner analytics only; no charts/bands/recommendations) |
| Exam history (EXM-010) | `/exams/history` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-073` | `GET /me/nurse-profile/exam-attempts` (page/pageSize/status), `PaginatedResult<ExamAttemptDto>`, learner-owned, StartedAt DESC | NOT STARTED | HUMAN_APPROVED (E73-1–E73-12; source-agnostic attempt list only; no result/review/analytics duplication) |
| Answer review (EXM-009) | `/exams/:examId/sessions/:sessionId/review` (SECURITY-sensitive) | `T-FE-072` | `GET .../review` | NOT STARTED | HUMAN_APPROVED (E72-1–E72-14; source-agnostic finalized per-question review only; no analytics) |

## 3. Exam catalog/detail contract (verified from source, T-FE-066 scope)

List: `page`/`pageSize`/`countryId`/`categoryId` only (validators page `>= 1`,
size `1..100`), 1-based, deterministic order country/category/title/id,
server totals. List contains only startable items: `CanStart = IsFree OR
active grant`, filtered server-side; paid exams without a grant never appear.
Detail: single id lookup (`ExamDetailDto` = catalog fields + nullable
`Instructions`), 404 when absent. Both endpoints `RequireAuthorization()` only.
DTO display surface — `Title`, `Description?`, `CountryName`,
`CategoryName?`, `DurationMinutes`, `QuestionCount`,
`PassingScorePercentage`, `IsFree`, `CanStart`; hide — all Guid ids. No
questions, answers, correctness, rationales, or keys are exposed.
Generated clients exist (`list-exams`, `get-exam`).

## 4. Session contract summary (T-FE-069 scope, backend-verified)

`ExamSessionDto`: `Id/ExamId/ExamTitle/Status/Source/StartedAt/ExpiresAt/
RemainingSeconds/Items[]`; per question `Text/Points/DisplayOrder/
SelectedExamSessionAnswerOptionId/Options[]`; per option `Text/DisplayOrder`
only — structurally no correctness, key, or explanation pre-completion.
Explicit list-PUT saves (upsert, InProgress + unexpired only); submit is
non-idempotent and finalizes (`Submitted` before expiry, `Expired` at/past
expiry via lazy server finalization); result aggregates and review (correctness
+ explanations, post-completion only) are separate DTOs. Statuses:
InProgress/Submitted/Expired (+ enum-only Abandoned, unused in current flows).
Sessions never reopen. PackageAttempt sessions share this exact DTO and UI.

## 5. HUMAN APPROVED DECISIONS (recorded 2026-09-18)

**E61-1 — Catalog card contents. APPROVED (Option B).** Card shows `Title`;
`Description` only when non-empty; `CountryName`/`CategoryName` when
available; `DurationMinutes`; `QuestionCount`; `Free` marker only when
`IsFree == true`. Never raw IDs, never `CanStart` as technical text, never
"Requires purchase" on catalog rows (ungranted paid exams are backend-omitted).

**E61-2 — Catalog filters. APPROVED (Option B).** Exactly Country and Exam
Category filters bound strictly to `countryId`/`categoryId`. No search, no
sort, no invented filtering. Changing a filter resets pagination per shared
list/filter precedent.

**E61-3 — Paid without access. APPROVED (Option A).** Exam Detail shows factual
"Requires purchase" state when `IsFree == false` and `CanStart == false`.
No purchase implementation, no checkout action, no invented commerce route.

**E61-4 — Passing score. APPROVED (Option A).** `PassingScorePercentage` shown
on Exam Detail and Exam Instructions, never on catalog cards, always backend
truth, never derived or reinterpreted.

**E61-5 — Detail CTA. APPROVED (Option A).** For startable exams the primary
destination is "View instructions" → `EXAMS_INSTRUCTIONS`. Never create/start
directly from Exam Detail.

**E61-6 — Instructions content. APPROVED (Option A).** Instructions screen shows
backend `Instructions` verbatim when non-empty (calm factual fallback when
null/empty), plus `DurationMinutes`, `QuestionCount`, `PassingScorePercentage`.
Never invent clinical/exam instructions.

**E61-7 — Start + resume. APPROVED (Option A).** Starting requires confirmation
that makes the timed-session nature clear. Backend start stays authoritative
and idempotently returns an existing unexpired InProgress session. Approved
labels: "Start exam", "Resume exam". After success navigate canonical
`EXAMS_SESSION` (`/exams/:examId/sessions/:sessionId`) via the canonical route
builder.

**E61-8 — Purchase-required form. APPROVED (Option A).** EXM-003 remains an
INLINE STATE, never a routable page. No purchase-required route may be
created. No purchase CTA under T-FE-061/067/068.

**E61-9 — Session layout. APPROVED (Option A).** One question at a time, backend
item order, backend option order, position indicator "Question {current} of
{total}". Never a full-scroll all-questions layout.

**E61-10 — Save behavior. APPROVED (Option A).** Explicit per-question save,
label "Save answer", via the existing answer PUT contract. No autosave. No
silent save-on-navigation as the primary behavior. Persisted backend selection
is authoritative.

**E61-11 — Timer + expiry. APPROVED (Option A).** Visible countdown from backend
`RemainingSeconds` (local ticking between responses allowed; backend truth
authoritative). Near-expiry warning, factual expired state, transition out of
active answering on detected expiry. No pause behavior; wall-clock expiry
continues. The timer primitive may be created inside T-FE-069.

**E61-12 — Submit + transient result. APPROVED (Option A).** Submit requires a
confirmation surface showing total and unanswered counts; action "Submit exam";
submit is irreversible and backend-non-idempotent. After success T-FE-069 shows
ONLY the transient summary from the submit response (`Score`, `MaxScore`,
`Percentage`, `Passed`, `CorrectCount`, `QuestionCount`). Never per-question
correctness, answer key, explanations, or review content (T-FE-072 scope).
Full EXM-007 remains T-FE-071.

**E61-13 — Errors/404/retry/back. APPROVED (Option A).** Reuse T-FE-075/076/078
behavior: contextual non-revealing 404s, factual 409 notices, no raw backend
exception/code text, `LoadingErrorRetry` for recoverable failures,
context-preserving retry, global 401/session handling, canonical back links,
no screen-owned authorization invention.

### 2026-09-28 human interaction correction (new goal; prior flow closure preserved)

`EXAMS-INTERACTION-CORRECTIONS-2026-09-28` explicitly supersedes **E61-10's explicit Save answer/no autosave** and expands E61-9's one-question-at-a-time pager to include visible direct Question Navigation while retaining one displayed question and backend order. Selection/change auto-enters persistence; Next, Previous, direct navigation and Submit wait for pending saves, stay put on failure and retain the choice for retry. Clear selection must persist a genuinely unanswered state; Flag/Unflag is a persisted attempt-scoped review marker independent of answer/scoring. EXM-004 Start/Resume and EXM-006 Submit confirmation are modal dialogs, not inline panels. No change to E61-11 server timer, E61-12 scoring/submission truth, authorization, or pre-finalization non-exposure. Design evidence and candidate limitations: `docs/frontend/design/stitch/stitch-artifact-registry.md` (Exams Interaction Corrections section). This addendum authorizes only these interactions; it does not retroactively invalidate the verified original task.

## 6. Field and non-exposure authority

Display verbatim/safe: `Title`, non-empty `Description`/`Instructions`
(fallback when empty), `CountryName`/`CategoryName?`, `DurationMinutes`,
`QuestionCount`, `PassingScorePercentage`, `IsFree`/`CanStart` (as E61-1/E61-3
presentation, never raw booleans), session `Status/StartedAt/ExpiresAt/
RemainingSeconds`, question `Text/Points/DisplayOrder`, option `Text/
DisplayOrder`, persisted `SelectedExamSessionAnswerOptionId`, transient result
aggregates. Internal/never-display: all Guid ids, session internals beyond the
listed surface, review correctness/keys/explanations (outside T-FE-069).
Formatting approval granted inline above (absolute timestamps per shipped
`date:'medium'` precedent; counts as plain facts).

## 7. EXM-007 transient boundary (T-FE-069 may / T-FE-071 owns)

T-FE-069 may render transiently from the submit response only: `Score`,
`MaxScore`, `Percentage`, `Passed` ("Passed"/"Not passed" wording),
`CorrectCount`, `QuestionCount`, plus a back/continue action. It must not build
the `EXAMS_RESULT` route experience, historical results, comparisons, review
linkage, or per-question breakdowns. Full EXM-007 remains owned by T-FE-071.

## 7A. EXM-007 full result approval (T-FE-071 owns)

**E71-1 — Source ownership. HUMAN_APPROVED.** EXM-007 is a source-agnostic
aggregate exam-result screen. It may display an owned finalized session result
for either `Standalone` or `PackageAttempt` sessions when the existing result
endpoint authorizes the authenticated learner. The EXM-007 UI must not branch
by source/provenance and must not require source detection. T-FE-079 continues
to own the package-specific analytical report at
`/nurse/preparation-packages/reports/:sessionId`; EXM-007 is the generic
aggregate exam result, and the T-FE-079 report is the package-specific
topic/guidance analysis. They are complementary. EXM-007 must not replace the
T-FE-079 report.

**E71-2 — Heading and contextual title. HUMAN_APPROVED.** Page heading is
"Exam result". If the backend provides a non-empty safe exam title, display it
as contextual secondary information. Do not require the title for page identity.

**E71-3 — Display fields and terminal status. HUMAN_APPROVED.** Render only
these backend-provided aggregate values: `Score`, `MaxScore`, `Percentage`,
`Passed`/`Not passed`, `CorrectCount`, and `QuestionCount`. Render factual
terminal status as `Submitted` -> "Completed" and `Expired` -> "Time expired".
Do not display timestamps in T-FE-071, including `StartedAt` or `FinalizedAt`,
even when present in the DTO. Do not calculate any aggregate locally when the
backend supplies it.

**E71-4 — Submitted and expired behavior. HUMAN_APPROVED.** For backend result
status `Submitted`, show "Completed" and the six approved aggregate values;
`Passed`/`Not passed` comes only from backend `passed`, with no locally derived
thresholds. For backend result status `Expired`, show "Time expired" and, when
the finalized-result endpoint returns a valid `ExamSessionResultDto`, render the
backend aggregate values exactly as returned. Do not manufacture zero scores,
force "Not passed" locally, infer missing answers, or invent a separate expiry
result. Backend result truth is authoritative.

**E71-5 — T-FE-069 entry action. HUMAN_APPROVED.** T-FE-069's transient
aggregate result remains intact and is not replaced automatically. When T-FE-071
is implemented, add a navigation action to the existing transient successful
result state: "View full result" -> canonical `EXAMS_RESULT`. This action may
appear for both standalone and package-attempt sessions because EXM-007 is
source-agnostic. Do not auto-navigate immediately after submit; the transient
result remains visible first.

**E71-6 — Result navigation. HUMAN_APPROVED.** On EXM-007, the primary stable
navigation after viewing the result is "Back to exams" -> canonical
`EXAMS_CATALOG`. Do not depend on browser history. No package-specific back
action is required because EXM-007 is source-agnostic. T-FE-079's analytical
report keeps its own "Back to preparation packages" behavior.

**E71-7 — Review CTA boundary. HUMAN_APPROVED.** Do not show "Review answers"
in T-FE-071. T-FE-072 owns review design, route behavior, eligibility, and
security. When T-FE-072 is later implemented and verified, it may add an
appropriate review entry action. Until then, EXM-007 contains no review CTA.

**E71-8 — Direct URL, reload, and result mismatch. HUMAN_APPROVED.**
`EXAMS_RESULT` must work from route identity alone:
`/exams/:examId/sessions/:sessionId/result`. Load the result using `sessionId`.
Do not depend on transient submit state, router navigation state,
`localStorage`, `sessionStorage`, or browser history. The backend result endpoint
is authoritative. If the returned result `examId` conflicts with the route
`examId`, show the approved unavailable result state and expose neither raw id.

**E71-9 — Error copy and security boundary. HUMAN_APPROVED.** Approved copy:
404 / foreign / unavailable — "This exam result isn't available." 409 / session
not finalized — "Finish the exam before viewing the result." Generic
recoverable failure — "We couldn't load this exam result. Try again." Action:
"Retry". Retry uses the same route `sessionId`. Never expose raw HTTP
status/code/backend text. EXM-007 remains aggregate-only: never render or import
question text, user per-question answers, answer-option text, correct option,
answer key, per-question correctness, explanation/rationale, review DTO data,
or analytics. T-FE-072 owns review; T-FE-074 owns analytics.

## 7B. EXM-009 answer review approval (T-FE-072 owns)

**E72-1 — Source ownership. HUMAN_APPROVED.** EXM-009 Answer Review is
source-agnostic for an authenticated learner's owned finalized exam session. It
applies to both `Standalone` and `PackageAttempt` sessions, provided the backend
review endpoint authorizes the learner and the session is finalized. The review
UI must not branch by source/provenance. T-FE-079 remains the separate
package-specific analytical report at
`/nurse/preparation-packages/reports/:sessionId`; EXM-009 is per-question
finalized answer review and the T-FE-079 report is package-specific
analytical/topic/guidance reporting. Both may coexist for `PackageAttempt`
sessions. Neither replaces the other.

**E72-2 — Entry from T-FE-071. HUMAN_APPROVED.** When T-FE-072 is implemented,
add to the finalized T-FE-071 result screen a navigation-only CTA "Review
answers" -> canonical `EXAMS_REVIEW` via `buildExamsReviewPath(examId,
sessionId)`. This CTA is allowed for both `Submitted` and `Expired` finalized
results. T-FE-071 must not fetch review data and must not display correctness
inline on the result screen.

**E72-3 — Page heading. HUMAN_APPROVED.** Page heading is "Answer review". If
the backend provides a safe non-empty exam title, display it as contextual
secondary information. Do not require the title for route/page identity.

**E72-4 — Layout and navigation. HUMAN_APPROVED.** One question at a time.
Preserve backend question order. Show "Question {current} of {total}".
Navigation is "Previous" / "Next": Previous disabled at the first question,
Next disabled at the last question. Navigation changes the local review index
only, with no API request per navigation. No direct question-number navigator,
no flagged-question system, no audit-inspired question navigator.

**E72-5 — Question content. HUMAN_APPROVED.** For each reviewed question
display: question text; factual review status; answer options in backend order;
learner-selection indicator; correct-answer indicator; explanation when
non-empty; points as "Points: {pointsEarned} of {points}". Use backend values
verbatim. Never calculate points locally.

**E72-6 — Correctness status. HUMAN_APPROVED.** Explicit text, never color
alone: answered correctly -> "Correct"; answered incorrectly -> "Incorrect"; not
answered -> "Unanswered". Adapt authoritative backend truth (selected/correct
option linkage, option `IsCorrect` fields) into a frontend-safe model. Never
derive correctness from option IDs when the DTO provides authoritative
correctness.

**E72-7 — Option presentation. HUMAN_APPROVED.** Render all answer options in
backend order as plain review rows/list items, never as active radio inputs;
review is read-only. The learner-selected option carries the visible text "Your
answer". The correct option carries the visible text "Correct answer". An
option that is both may carry both labels. Never rely on color/check icons
alone. Answers cannot be changed in review.

**E72-8 — Unanswered presentation. HUMAN_APPROVED.** When
`SelectedExamSessionAnswerOptionId` is null, display "Unanswered". No option
receives "Your answer". The authoritative correct option is still displayed as
"Correct answer" because this is finalized review. Do not invent alternate
unanswered copy.

**E72-9 — Explanation. HUMAN_APPROVED.** When backend `Explanation` is non-null
and non-empty, show section heading "Explanation" and render the backend text
verbatim. When null/blank, omit the Explanation section entirely. Never invent
fallback explanation copy or derive rationale from correctness.

**E72-10 — Submitted and expired behavior. HUMAN_APPROVED.** For finalized
status `Submitted`, the review is available; render §§E72-4–E72-9 with no
additional Submitted banner and no duplicate of the aggregate T-FE-071 result
summary. For finalized status `Expired`, review is also available; display the
factual contextual status "Time expired", then render the finalized question
review exactly as the backend returns it. Unanswered questions remain
"Unanswered". Never force remaining questions incorrect locally, invent zero
points, hide correct answers because the session expired, or create a separate
Expired review design. Backend finalized review is authoritative.

**E72-11 — Back navigation. HUMAN_APPROVED.** Stable action "Back to result" ->
canonical `EXAMS_RESULT` (`/exams/:examId/sessions/:sessionId/result`) via the
canonical builder. Works for direct URL, reload, `Standalone`, and
`PackageAttempt`. Never depend on browser history. No package-specific back
action on EXM-009; T-FE-079 keeps its own package navigation.

**E72-12 — Direct URL and reload. HUMAN_APPROVED.** `EXAMS_REVIEW` loads from
route identity alone (`examId`, `sessionId`); the backend review GET uses
`sessionId`. Never depend on T-FE-071/T-FE-069 state, `localStorage`,
`sessionStorage`, or browser history. On backend review `examId` vs route
`examId` conflict, show the approved unavailable state and never show raw IDs.

**E72-13 — Error copy. HUMAN_APPROVED.** 404 / foreign / unavailable: "This
exam review isn't available." 409 / session not finalized: "Finish the exam
before reviewing answers." Generic recoverable failure: "We couldn't load this
exam review. Try again." Action: "Retry" using the same route `sessionId`.
Never expose raw HTTP status, backend exception/message, GUIDs, or internal
status/debug values.

**E72-14 — Field, raw-id, and analytics boundary. HUMAN_APPROVED.** T-FE-072
presentation may use only: `ExamId` internally for route reconciliation;
`ExamTitle`; `Status`; per question `DisplayOrder`, `Text`, `Explanation`,
`Points`, `PointsEarned`; internally `SelectedExamSessionAnswerOptionId`,
`CorrectAnswerOptionId`, option `Id`, option `DisplayOrder`, option `Text`,
option `IsCorrect`. Never display aggregate `Score`/`MaxScore`/`Percentage`/
`Passed` on EXM-009 (T-FE-071 owns them). Never visibly render any raw IDs
(review/session/exam/question/option/correct/selected/provenance). T-FE-074
owns analytics: no topic summaries, strengths/weaknesses, performance bands,
recommendations, charts, longitudinal data, or package guidance on EXM-009.

## 7C. EXM-008 exam analytics approval (T-FE-074 owns)

**E74-1 — Analytics ownership and boundaries. HUMAN_APPROVED.** T-FE-074 owns
HISTORICAL LEARNER EXAM ANALYTICS: the authenticated Nurse's exam-attempt
history summarized across multiple attempts, exams, categories, and time
buckets. It is not a single-session result screen, per-question answer review,
Preparation Package analytical report, recommendation engine, or answer-key
surface. T-FE-071 remains one finalized session aggregate result; T-FE-072
remains one finalized session per-question answer review; T-FE-079 remains one
`PackageAttempt` session's package-specific analytical report and
purchased-content guidance. T-FE-074 must not duplicate or absorb those
surfaces.

**E74-2 — Source ownership. HUMAN_APPROVED.** T-FE-074 is source-agnostic at
the historical learner-analytics level: it uses whatever owned learner exam
attempts the backend analytics contract legitimately includes (`Standalone`
and `PackageAttempt` attempts alike). Never branch UI by attempt source, show
source/provenance, add source filters, exclude `PackageAttempt` locally, or add
package guidance. The backend analytics query is authoritative for membership.
T-FE-079 remains the separate per-session package analysis.

**E74-3 — Heading and entry. HUMAN_APPROVED.** Page heading is "Exam
analytics" with supporting text "Review your exam performance over time." No
dashboard marketing copy and no performance-judgment copy. The entry lives on
the existing `EXAMS_CATALOG` screen as a "View analytics" navigation action to
canonical `EXAMS_ANALYTICS`. No Analytics links are added to T-FE-071 result,
T-FE-072 review, or the T-FE-079 package report.

**E74-4 — Operations and sections. HUMAN_APPROVED.** T-FE-074 owns all four
existing learner analytics operations: `GetMyExamAnalyticsSummary` (`GET
/me/nurse-profile/exam-analytics/summary`), `ListMyExamAnalyticsByExam`
(`.../by-exam`), `ListMyExamAnalyticsByCategory` (`.../by-category`), and
`ListMyExamAnalyticsTrends` (`.../trends`). No fifth endpoint is added. The
screen contains four bounded sections — Overview, Performance by exam,
Performance by category, Performance over time — rendered as accessible cards /
definition lists / lists / tables as appropriate. No charts are required or
authorized.

**E74-5 — Overview fields and nulls. HUMAN_APPROVED.** From
`ExamAnalyticsSummaryDto` render: Total attempts (`AttemptCount`), Submitted
(`SubmittedCount`), Expired (`ExpiredCount`), In progress (`InProgressCount`),
Passed (`PassedCount`), Failed (`FailedCount`), Pass rate
(`PassRatePercentage`), Average score (`AverageScorePercentage`), Best score
(`BestScorePercentage`), Latest score (`LatestScorePercentage`). Percentages
display as backend values with a `%` presentation suffix only, per existing
numeric-formatting precedent; never recompute locally. Never render
`AbandonedCount`, `CountedAttemptCount`, `AverageScore`, `AverageMaxScore`,
`AverageCorrectCount`, `AverageQuestionCount`, `FirstAttemptStartedAt`, or
`LatestAttemptStartedAt`. Nullable metrics with insufficient counted data
render as "Not available" — never `0%`, `N/A`, `—`, or an invented value.

**E74-6 — No performance bands. HUMAN_APPROVED.** T-FE-074 has no qualitative
performance bands: never show Excellent/Good/Poor/Weak/Strong/Needs
improvement/At risk/Mastered/grade/tier, and never define frontend thresholds.
Only backend numerical facts are shown.

**E74-7 — By-exam rows. HUMAN_APPROVED.** Visible semantic fields only, and
only when the authoritative DTO provides them directly: human-readable exam
title/name, attempt count, pass rate, average score percentage, best score
percentage, latest score percentage. Never derive missing metrics, never
display raw exam/version ids, preserve backend ordering, use the existing
pagination pattern with fixed page size 20 (subject to the endpoint contract),
no client-side sort.

**E74-8 — By-category rows. HUMAN_APPROVED.** Visible semantic fields only,
and only when directly supplied: human-readable category name, attempt count,
pass rate, average score percentage, best score percentage
(`LatestScorePercentage` is not required on category rows unless DTO/design
semantics clearly define it). Never invent or derive it, never display raw
category ids, existing pagination with fixed page size 20, no client-side
sort.

**E74-9 — Trends. HUMAN_APPROVED.** Approved bucket for the initial
implementation is Month; no Day/Week/Month selector is exposed — call the
backend with its existing monthly bucket value. Render trend points as an
accessible chronological list/table, never a chart, preserving backend
chronological ordering. Per point display only directly supplied safe fields
for period/month, attempt count, average score percentage, and pass rate;
omit any metric the DTO does not provide rather than deriving it. Use existing
safe date/month formatting precedent.

**E74-10 — Filters. HUMAN_APPROVED.** Initial filter set: From date, To date,
Country, Exam category. No examId, attempt-source, status, trend-bucket, or
sort controls. Reuse the existing Country and Exam Category lookup/filter
foundations from T-FE-067; never duplicate lookup services. All four sections
share one active filter set: Apply reloads the summary, resets by-exam and
by-category to page 1, reloads trends (bucket stays Month); Clear filters
clears from/to/country/category, returns pagination to page 1, and reloads
unfiltered. No API calls on keystrokes — explicit "Apply filters" and "Clear
filters" actions only. From/To are optional; when both exist and From is later
than To, show "From date must be on or before To date." and call no analytics
endpoints. No other local date constraints; backend remains server-side range
authority.

**E74-11 — Filter URL and reload. HUMAN_APPROVED.** Active applied filters
persist as query parameters `from`, `to`, `countryId`, `categoryId` so direct
reload preserves analytical context. Pagination state stays out of the route
unless existing list-page precedent clearly requires it; the fixed trend
bucket is never a query parameter. Raw ids may exist internally in query
parameters but must never render visibly. Invalid/unparseable query filter
input follows existing safe route/filter normalization precedent: never crash
and never reflect raw malformed values into visible error copy.

**E74-12 — Empty, sparse, error, and back behavior. HUMAN_APPROVED.** When the
authoritative summary indicates `AttemptCount = 0`, show the page-level empty
state titled "No exam analytics yet" with body "Complete an exam to see your
analytics." and action "Back to exams" to `EXAMS_CATALOG`; never render empty
metric cards with fabricated zeros and never call this an error. When
`AttemptCount > 0` but a subsection has no rows/points under current filters,
the Overview may still render and the empty subsection shows "No data is
available for these filters." Page-level load failure: "We couldn't load your
exam analytics. Try again." with Retry. Section-level failures after overview
is available: "We couldn't load exam-level analytics. Try again.", "We
couldn't load category analytics. Try again.", "We couldn't load
performance-over-time data. Try again.", each with section-level Retry. Never
expose HTTP status, backend exception text, GUIDs, permission keys, or
internal analytics/query details. Stable navigation is "Back to exams" to
canonical `EXAMS_CATALOG`, never browser history.

**E74-13 — Separation from result, review, and package report. HUMAN_APPROVED.**
T-FE-074 must not reproduce a selected session's Score/Max score block,
Passed/Not passed presentation, or CorrectCount/QuestionCount result card
except where historical backend analytics independently provides approved
history metrics; no session-specific result navigation is required from
analytics. It must never render question text, learner answers, correct
answers, explanations, per-question correctness/points, or any Review answers
functionality. It must never render package entitlement state,
provenance/source, purchased-content guidance, Recommended package content,
package topic guidance, or package-specific report links. T-FE-079 is never
altered.

**E74-14 — Presentation constraints. HUMAN_APPROVED.** Textual summary facts,
cards/dl, accessible rows/tables/lists, and pagination only. No charts, no
recommendations, no qualitative insights, no AI interpretation, no "You should
study..." copy, no dashboard/sidebar. Established light platform foundation;
desktop responsive grid for overview metrics with vertical stacking otherwise;
mobile single-column summary, stacked filters, wrapping metrics, retained
information, no horizontal overflow. Accessibility: semantic `h1` "Exam
analytics" plus semantic section headings, labeled filters with native date
inputs or established accessible date controls, textual percentages, no
color-only meaning, pagination labels, loading/error live announcements,
visible focus, ≥44px interactive targets, WCAG 2.2 AA target.

## 7D. EXM-010 exam history approval (T-FE-073 owns)

**E73-1 — History ownership and boundaries. HUMAN_APPROVED.** T-FE-073 owns
learner exam-attempt history: a concise per-attempt list only. It is not
historical analytics, answer review, a package analytical report, or another
exam catalog. It must not reproduce the full T-FE-071 result screen (no
Score/Max score detail, no Correct/Question counts, no result presentation),
must not fetch or render any T-FE-072 review content (no question/learner
answers/correct answers/explanations/correctness/points, no Review data
fetch), and must not render T-FE-074 analytics (no pass rate, averages,
best/latest metrics, breakdowns, trends, charts, bands).

**E73-2 — Source ownership. HUMAN_APPROVED.** Exam History is source-agnostic
over the learner-owned attempts returned by `ListMyExamAttempts`. Never
inspect/probe provenance, branch Standalone vs PackageAttempt UI, display
source, add a source filter, or locally exclude PackageAttempt attempts.
Package-specific analytical reporting remains T-FE-079; generic result/review
routes may be used for finalized package attempts.

**E73-3 — Heading, entry, and route. HUMAN_APPROVED.** Heading is "Exam
history" with supporting copy "Review your current and completed exam
attempts." Navigation-only entry "View history" lives on `EXAMS_CATALOG` and
targets canonical `EXAMS_HISTORY` (`/exams/history`, AUTHENTICATED_ONLY,
three guards, lazy screen, static-before-dynamic ordering). Never fetch
history from the catalog screen and never redesign the catalog.

**E73-4 — Visible fields and status labels. HUMAN_APPROVED.** Each row shows
exam title, factual status, and started date/time. Map statuses verbatim:
`InProgress` → "In progress", `Submitted` → "Completed", `Expired` → "Time
expired", reachable `Abandoned` → "Abandoned"; unknown/unusable status fails
safely and never displays raw enum text. For `InProgress` rows with a safe
backend `ExpiresAt`, also show "Ends" with the formatted expiry. For finalized
`Submitted`/`Expired` rows show backend-provided non-null values only:
Percentage, and Passed → "Passed" / false → "Not passed" / null → omit Result
entirely. Never calculate locally; never force `0%`/`Not passed` on Expired.
Never display raw Score/MaxScore, CorrectCount/QuestionCount, GUIDs,
provenance, review details, or explanations.

**E73-5 — Row actions. HUMAN_APPROVED.** `InProgress` → "Resume exam" to
canonical `EXAMS_SESSION` (existing session navigation only; never call
StartExamSession or Package Start; list stays factual to the attempts endpoint
and never locally rewrites status, polls per-row, or auto-finalizes).
`Submitted` → "View result" (`EXAMS_RESULT`) and "Review answers"
(`EXAMS_REVIEW`). `Expired` → "View result" and "Review answers". No
source-specific branching.

**E73-6 — Filter, pagination, and query params. HUMAN_APPROVED.** One Status
filter: All (no status parameter), In progress (`InProgress`), Completed
(`Submitted`), Time expired (`Expired`); no Abandoned/source/date/country/
category/search/sort controls. Abandoned rows, if any, stay visible under All.
Changing Status applies immediately, resets pagination to page 1, reloads with
the backend status, and preserves the filter. Route query state is `status`
(canonical backend identity) and 1-based `page`; invalid values normalize to
All/page 1; filter change writes page 1; pageSize stays out of the URL.
Pagination uses existing shared pagination at fixed pageSize 20 (subject to
endpoint bounds); page change preserves the filter, reloads history only, and
never navigates away.

**E73-7 — Empty, error, and back behavior. HUMAN_APPROVED.** Unfiltered zero
attempts: title "No exam attempts yet", body "Start an exam to see your
history.", action "Browse exams" to `EXAMS_CATALOG` (not an error; no fake
zero cards/tables). Filtered zero rows: title "No exam attempts match this
filter", body "Try another status.", action "Clear filter" (resets to All/page
1, normalized URL, unfiltered reload, no navigation away). Load failure: "We
couldn't load your exam history. Try again." with Retry on the same
status/page; never raw HTTP/backend/GUID/enum text. Stable "Back to exams"
targets canonical `EXAMS_CATALOG`, never browser history.

**E73-8 — Responsive, RTL, and accessibility. HUMAN_APPROVED.** Desktop, tablet,
and mobile: stacked filters, stacking rows/cards, wrapping long titles,
readable dates, safely wrapping actions/links, usable pagination, ≥44px
interactive targets where applicable, zero horizontal overflow. RTL: logical
properties only, preserved backend row order, stable dates/numerics,
appropriate directional controls. Accessibility: semantic h1 and list/table/
card structure, text status, associated Status filter label, accessible
pagination, loading/error announcements, visible focus, keyboard-complete
links/controls, no color-only meaning, WCAG 2.2 AA target.

**E73-9 — Attempt/session identity. HUMAN_APPROVED.** Backend source proves
`ExamAttemptDto.Id` is the `ExamSession.Id` row identity (same
`ExamSessions`-table row addressed by the session endpoints), so row actions
may navigate with existing canonical `EXAMS_SESSION`/`EXAMS_RESULT`/
`EXAMS_REVIEW` builders. Never fabricate session IDs.

**E73-10 — Status contract. HUMAN_APPROVED.** Domain statuses are
`InProgress`/`Submitted`/`Expired`/`Abandoned` serialized as backend strings;
the attempts endpoint accepts an optional status filter, defaults page 1 and
pageSize 20, orders StartedAt DESC then Id, nulls score facts for
non-terminal rows, and scopes rows to the current nurse profile. Adapt
reachable statuses to safe factual presentation only; never invent
transitions.

**E73-11 — T-FE-079 coexistence. HUMAN_APPROVED.** PackageAttempt rows appear
without source-specific UI; finalized package attempts may use the shared
result/review routes; never display package source, link to the package
analytical report from history, show package guidance, or infer
entitlement/provenance. T-FE-079 remains unchanged.

**E73-12 — Scope exclusions. HUMAN_APPROVED.** No global navigation/header,
question navigator, design-token overhaul, auth/profile redesign, admin-table
redesign, offline/maintenance framework, or global typography/container work.
T-FE-073 only.

## 8. EXM-008 approved in §7C above (EXM-009 approved in §7B above, EXM-010 approved in §7D above)

Analytics (T-FE-074) is approved in §7C above and keeps its own gate. Nothing
in T-FE-067/068/069 requires it. Answer review (T-FE-072) is approved in §7B
above and keeps its own gate. Exam history (T-FE-073) is approved in §7D above
and keeps its own gate.

## 9. Visual-foundation binding (all pages.pdf)

The approved visual foundation is the project/user-provided `all pages.pdf`
context (external to this repository; values below are recorded verbatim from
the human-approved authorization). Exam screens bind to it as follows —
background/app `#F6FBFA`; surface/default `#FFFFFF`; text/primary
Professional Navy `#173B57`; general primary actions Nursing Teal `#006B66`;
exam-context high-emphasis actions (Start exam, Resume exam, Save answer,
Submit exam, exam Next) Exam Focus Indigo `#4F46B8`; Success `#147A4B`;
Information `#0B63A3`; Warning `#8A4B00`; Error `#B3261E`. General platform
actions remain Nursing Teal unless another approved semantic role applies.
Never color alone for meaning. Typography/shape/spacing: Noto Sans (English),
Noto Sans Arabic (Arabic/RTL); logical start/end alignment; no Arabic text
shrinking; 4px spacing base with tokenized spacing only; inputs radius/sm;
buttons radius/md; cards radius/lg; dialogs radius/xl; touch targets ≥44×44
(48×48 preferred mobile); restrained elevation with border/spacing before
shadow; visible focus ring; no decorative dashboard clutter. Reuse existing
component/pattern decisions before creating new presentation.

## 10. Approved copy

Behavioral decisions E61-1–E61-13 are HUMAN_APPROVED. Core labels: Exams,
View instructions, Requires purchase, Start exam, Resume exam,
Question {current} of {total}, Save answer, Previous, Next, Submit exam,
Cancel, Passed, Not passed. Finalized concept wording (behavior-identical,
no new product meaning): empty catalog — "No exams available." /
"There are no exams to show right now. Please check back later."; filtered
no-results — "No exams match the selected filters."; missing instructions —
"No instructions were supplied for this exam."; timed confirmation —
"This is a timed exam session. Starting the exam begins the timer and the
attempt cannot be paused."; near-expiry — "Time is almost up. Submit your
exam soon."; expired — "The exam session has expired. Your submitted answers
were finalized."; unanswered warning — "{n} questions are unanswered.
Unanswered questions score zero." (count rendered from local selections);
submission success — "Exam submitted."; purchase notice — "This exam requires
purchase before it can be started."; 404s — "This {exam | instructions |
session} is no longer available. It may have been removed, or the link is
incorrect."; retry/error/back — shared component copy and canonical back
links ("Back to exams", "Back to exam", "Back to instructions").

## 11. Responsive / RTL / accessibility authority

Desktop/tablet/mobile required. Mobile: single-column content, 16px page
gutter, ≥44px targets (48px preferred), no horizontal page overflow,
stackable/wrappable actions. RTL: logical properties, correct source order,
directional icons mirror only when appropriate, status icons do not mirror,
numbers/percentages semantically stable, Noto Sans Arabic, no font-size
reduction to force fit. Accessibility (WCAG 2.2 AA target): semantic headings,
native radio semantics with fieldset/legend (or repository-equivalent
grouping), visible focus, keyboard-complete flow, no color-only
correctness/status, live announcements for save/submission/expiry changes,
reduced-motion safe, focus trap/restore where dialogs apply.

## 12. Task ownership boundaries

T-FE-061 approves presentation authority only (this packet). T-FE-067 owns
EXM-001/002 implementation after GATE-FE-T061. T-FE-068 owns EXM-003(state)/
EXM-004 after GATE-FE-T067. T-FE-069 owns EXM-005 + transient EXM-006 after
GATE-FE-T068 (timer primitive built there). T-FE-071/072/074 own result/
review/analytics/history. T-FE-073 owns EXM-010 implementation after GATE-FE-T073. T-FE-079 consumes the shared session destination; package
starts reuse it without a package-specific session UI. No global navigation
design is authorized here (separately deferred).

## 13. Downstream sequence

GATE-FE-T061 (this packet) → T-FE-067 (catalog/detail) → T-FE-068
(instructions/start, needs GATE-FE-T067) → T-FE-069 (session/submit + timer,
needs GATE-FE-T068) → T-FE-079 (package Start CTA + report, needs shared
session destination live).
