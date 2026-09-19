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
| Analytics (EXM-008) | `/exams/analytics` | `T-FE-074` | aggregate contracts | NOT STARTED | OUT OF SCOPE for this packet |
| Answer review (EXM-009) | `/exams/:examId/sessions/:sessionId/review` (SECURITY-sensitive) | `T-FE-072` | `GET .../review` | NOT STARTED | OUT OF SCOPE for this packet |

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

## 8. EXM-008 / EXM-009 exclusions

Analytics (T-FE-074) and SECURITY-sensitive answer review (T-FE-072) are
outside this packet's minimum approved scope and keep their own gates. Nothing
in T-FE-067/068/069 requires them. (Their contracts, routes, and policies
already exist; only their presentations await their owning approvals.)

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
review/analytics. T-FE-079 consumes the shared session destination; package
starts reuse it without a package-specific session UI. No global navigation
design is authorized here (separately deferred).

## 13. Downstream sequence

GATE-FE-T061 (this packet) → T-FE-067 (catalog/detail) → T-FE-068
(instructions/start, needs GATE-FE-T067) → T-FE-069 (session/submit + timer,
needs GATE-FE-T068) → T-FE-079 (package Start CTA + report, needs shared
session destination live).
