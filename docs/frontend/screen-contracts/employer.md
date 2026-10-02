# Employer Screen Contracts

## Family Authority

The Employer v1 screen contract is approved by `T-FE-085` / `GATE-FE-T085` and
the human-approved decisions `HD-EMP-01..04`. This family covers Employer Home,
Candidate Search/Results/Empty states, Candidate Request initiation, and Employer
Request List/Detail. Candidate Detail is explicitly deferred/backend-blocked.

Common constraints: Employer role only; no Stitch generation or Angular
implementation is authorized by this file; approved DTO fields and backend/API
privacy boundaries remain authoritative; do not expose candidate/private/internal
fields outside the approved DTOs.

## EMP-001 Employer Home

| Field | Contract |
|---|---|
| Identity | `/employer`; route ID `EMPLOYER_HOME`; Employer role. |
| Purpose | Focused Employer recruiting workspace entry. |
| Required | Clear entry to Candidate Search and Employer Requests. Employer profile/organization readiness may appear only where current authority proves it is a prerequisite or actionable state. |
| Forbidden | Dashboard metrics, recruiting KPIs, analytics, activity feeds, organization statistics, candidate counts, request counts, or unsupported dashboard claims. |

## EMP-002 Candidate Search

| Field | Contract |
|---|---|
| Identity | `/employer/candidates`; route ID `EMPLOYER_CANDIDATES`; Employer role. |
| Purpose | Search/browse candidates using current backend-supported filters. |
| Required | Approved filters only: pagination, license country, current country, minimum years of experience, skills, language. Include apply, clear/reset where appropriate, loading, initial/browse, and error+retry states. |
| Forbidden | Custom sorting, additional filters, saved searches, recommendations, ranking, or matching scores. |

## EMP-003 Candidate Results

| Field | Contract |
|---|---|
| Identity | Same route as `EMP-002`: `/employer/candidates`; route ID `EMPLOYER_CANDIDATES`; Employer role. |
| Required | Use only verified safe `CandidateListItemDto` fields: headline, professional summary, license country name, current country name, years of experience, skills, languages, certificates summary/count, latest experience title, education summary. `nurseProfileId` may be used internally for authorized actions and need not be displayed. |
| Actions | Contact/request action may originate from a result item per `EMP-006`; no Candidate Detail route is required. |
| Forbidden | Email, username, user ID, license number, CV/storage/file data, private profile data, raw internal IDs, internal entities, scores, recommendations, recruiter notes, private contact data. |

## EMP-004 Candidate Empty Filtered States

| Field | Contract |
|---|---|
| Identity | Same route as `EMP-002`: `/employer/candidates`; route ID `EMPLOYER_CANDIDATES`; Employer role. |
| Required | Cover initial/browse state where applicable, no-results state, filtered-empty state, clear/reset-filters action where filters are active, loading, and recoverable error+retry. |
| Forbidden | Empty states must not imply hidden private candidate counts, recommendations, alerts, or saved-search features. |

## EMP-005 Candidate Detail

| Field | Contract |
|---|---|
| Identity | `NOT_ROUTABLE`; candidate detail concept. |
| Decision | Deferred for v1 until an authoritative candidate-detail API/DTO/privacy contract exists. |
| Required | Do not create an active Candidate Detail screen from list DTO fields. Do not expose additional candidate information for design completeness. |
| Forbidden | Detail route, CV/license/contact/private fields, richer profile data, or inferred candidate biography. |

## EMP-006 Candidate Request

| Field | Contract |
|---|---|
| Identity | `NOT_ROUTABLE`; action/state originating from authorized candidate result/list item. |
| Purpose | Initiate contact/request using existing `nurseProfileId`-backed request authority. |
| Required | Clear contact/request action, simple confirmation before creation, no extra form fields, submission/loading, success, retryable failure, and duplicate/conflict outcome. Duplicate/conflict copy communicates an active/existing request prevents another and may link to Employer Requests. |
| Forbidden | Message/note/reason fields, Candidate Detail prerequisite, scheduling, recruiter notes, or invented backend transitions. |

## EMP-007 Employer Requests

| Field | Contract |
|---|---|
| Identity | `/employer/requests`; route ID `EMPLOYER_REQUESTS`; Employer role. |
| Purpose | List Employer-owned contact requests. |
| Required | Use current `ContactRequestDto` fields only: safe candidate snapshots, status, timestamps where useful, routing/detail IDs internally. Include status filtering where supported, backend pagination, empty/no-results/error states, detail navigation, and pending-only Cancel where backend authority permits. |
| Status vocabulary | Pending, Approved, Rejected, Cancelled. |
| Forbidden | Additional statuses, bulk actions, approve/reject actions, notes, private candidate data, internal ownership display, message/reason fields. |

## EMP-008 Employer Request Detail

| Field | Contract |
|---|---|
| Identity | `/employer/requests/:requestId`; route ID `EMPLOYER_REQUEST_DETAIL`; Employer role. |
| Purpose | Present one Employer-owned contact request using current `ContactRequestDto` only. |
| Required | Show safe candidate snapshots, status, timestamps, privacy-safe not-found/ownership behavior, Back to Requests, and Cancel only for Employer-owned Pending requests with confirmation. Terminal statuses must not expose Cancel. |
| Forbidden | Richer candidate/private profile data, Candidate Detail link, message/rejection reason, additional actions, backend transition invention. |
