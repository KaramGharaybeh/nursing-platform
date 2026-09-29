# Exams Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-EXAMS
status: deep-extracted-family-contract
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Primary authority: `exams-screen-approval-packet.md`, `system-design-contract.md`, verified T-FE-069/071/072/073/074 evidence, backend/OpenAPI exam contracts, canonical routes and route permissions.

## EXM-001 Exam Catalog

| Field | Contract |
|---|---|
| Identity | `/exams`; authenticated-only. |
| Purpose | Browse startable exams. |
| List/card data | `title`, `description`, `country`, `category`, `duration`, `questionCount`, free/paid marker only when backend supplies. Order from backend. No raw IDs. |
| Filters/pagination | Country and category filters; backend paging. Empty and no-results distinct. |
| Actions | View details; View analytics; View history. No purchase CTA in this contract. |
| Status | `CONTRACT_READY`. |

## EXM-002 Exam Detail

| Field | Contract |
|---|---|
| Identity | `/exams/:examId`; route param `examId` internal. |
| Purpose | Understand exam and go to instructions. |
| Data | Title, description, country/category, duration, question count, passing score, requirements/instructions summary where backend supplies. |
| Actions | View instructions -> `EXAMS_INSTRUCTIONS`; Back to exams. |
| States | Loading, ready, purchase-required inline state `EXM-003`, not found, error. No questions/answers. |
| Status | `CONTRACT_READY`. |

## EXM-003 Purchase Required Inline State

| Field | Contract |
|---|---|
| Gap | Inline paid-access state is deferred from current exam screen implementation. It must not create checkout or purchase behavior. |
| Status | `DEFERRED`. |

## EXM-004 Exam Instructions

| Field | Contract |
|---|---|
| Data | Backend instructions, duration, question count, passing score. |
| Actions | Start/Resume opens a modal dialog above the unchanged instructions page; explicit confirm alone calls backend start/resume and routes to session. Cancel closes without mutation and restores trigger focus. |
| States | Ready, confirmation, start/resume loading, 409 conflict, not found/error. |
| Status | `CONTRACT_READY`. |

## EXM-005 Exam Session

| Field | Contract |
|---|---|
| Purpose | Answer one question at a time with automatic persistence, navigate previous/next or by Question Navigation, submit. |
| Data | Question text, points, ordered options, selected option, timer/expiry facts, question position. Before finalization no correctness, answers, explanations, or keys. |
| Form/action | Option selection auto-persists; Clear selection persists true unanswered; Flag/Unflag persists a session-scoped review marker independent of the answer; numbered Question Navigation exposes current/answered/unanswered/flagged states; Previous/Next/direct navigation wait for pending persistence; Submit opens the `EXM-006` modal. No Save answer button. Backend save/clear/flag/submit authoritative. |
| States | Loading, answering, saving/saved/save-failure with retry and retained selection, near-expiry, expired, submit modal, submitting, transient result, 409/not found/error. |
| Status | `CONTRACT_READY`. |

## EXM-006 Submit Confirmation

| Field | Contract |
|---|---|
| Identity | Non-routable state inside exam session. |
| Data | Total/answered/unanswered count from persisted session selections after pending mutations settle; optional flagged count from persisted session marker; no correctness. |
| Actions | Semantic modal dialog above unchanged session; Cancel closes without submit and restores focus; explicit Submit after pending persistence finalizes through backend; duplicate submit blocked; success enables result. |
| Status | `CONTRACT_READY`. |

## EXM-007 Exam Result

| Field | Contract |
|---|---|
| Purpose | View finalized aggregate result. |
| Report sections/order | Heading/context; aggregate summary; terminal status; actions. |
| Metrics | `score`, `maxScore`, `percentage`, `passed`, `correctCount`, `questionCount`, Submitted/Expired display. Backend result endpoint source only; no local derivation. |
| Null/zero | Unavailable/null facts show `Not available`; zero is shown as `0`, not missing. |
| Actions | Back to exams; Review answers when review route/contract applies. |
| Forbidden | Question details, explanations, analytics, timestamps/raw IDs. |
| Status | `CONTRACT_READY`. |

## EXM-008 Exam Analytics

| Field | Contract |
|---|---|
| Purpose | Review historical performance facts. |
| Sections/order | Overview; Performance by exam; Performance by category; Performance over time. |
| Source | Four `/me/nurse-profile/exam-analytics/*` operations. |
| Metrics | Counts, percentages, average/best/latest metrics, monthly trend points directly from DTO. Month bucket fixed by backend. |
| Filters/pagination | From date, to date, country, category; Apply/Clear; query params; by-exam/by-category page size 20; backend ordering. |
| Forbidden | Charts, performance bands, recommendations, local derivation, source/provenance, raw IDs. |
| Status | `CONTRACT_READY`. |

## EXM-009 Answer Review

| Field | Contract |
|---|---|
| Purpose | Review finalized answers one question at a time. |
| Sections/data | Question pager; question text; option rows; selected/correct indicators; explanation; points earned/points. |
| Source | Backend review endpoint; finalized Submitted/Expired only. |
| Actions | Back to result. Read-only; no answer mutation. |
| Forbidden | Active inputs, analytics, aggregate result duplication beyond context, raw IDs. |
| Status | `CONTRACT_READY`. |

## EXM-010 Exam History

| Field | Contract |
|---|---|
| Purpose | Review current/completed attempts. |
| List columns/items | Exam title; status; started date; expires-at for in-progress; percentage/pass when finalized and non-null. |
| Filters/pagination | Status filter `All`, `InProgress`, `Submitted`, `Expired`; invalid query normalizes; page size 20; backend page/order. |
| Row actions | Resume session for in-progress; View result/review for finalized; Back/catalog. |
| Forbidden | score/max/correct counts, source/provenance, raw IDs. |
| Status | `CONTRACT_READY`. |
