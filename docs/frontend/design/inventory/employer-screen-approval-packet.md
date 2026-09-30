# Employer Screen Approval Packet — T-FE-085 / GATE-FE-T085

```yaml
document_id: NPS-DES-INV-EMPLOYER-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-21
prepared_by: T-FE-085 campaign
gate: GATE-FE-T085
gate_status: VERIFIED (closed by human-approved decisions HD-EMP-01..04)
authorization: HD-EMP-01..04 were approved by the human technical lead on
  2026-09-21. EMP-001/002/003/004/006/007/008 are APPROVED as v1 design
  contract authority. EMP-005 is explicitly DEFERRED/BACKEND_BLOCKED until a
  candidate-detail API/DTO/privacy contract exists. Approval does not itself
  start Angular, backend, OpenAPI, generated-client, database, Storybook, or
  Stitch implementation.
```

## 1. Purpose

This packet closes `T-FE-085` / `GATE-FE-T085` by recording the approved Employer
screen design contract for `EMP-001..008`. It separates design contract readiness
from implementation readiness: approved screens may be designed in Stitch and
implemented only through their later owning frontend tasks and only when backend
authority remains sufficient.

## 2. Family Authority

- Employer routes are role-only Employer routes in the canonical route registry
  and route permission policy.
- Candidate search uses the current source-backed `GET /api/v1/recruitment/candidates`
  contract only.
- Employer request management uses the current source-backed
  `/api/v1/recruitment/contact-requests` operations and `ContactRequestDto` only.
- Existing Angular implementation state is evidence only; it is not product or
  visual approval.
- Stitch generation is not authorized by this packet.

## 3. Approved Screen Inventory

| Screen | Route/routability | Design status | Implementation readiness | Approval |
|---|---|---|---|---|
| `EMP-001` Employer Home | `/employer` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-090` | HD-EMP-01 |
| `EMP-002` Candidate Search | `/employer/candidates` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-091` | HD-EMP-02 |
| `EMP-003` Candidate Results | `/employer/candidates` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-091` | HD-EMP-02 |
| `EMP-004` Candidate Empty / Filtered States | `/employer/candidates` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-091` | HD-EMP-02 |
| `EMP-005` Candidate Detail | NOT_ROUTABLE / deferred concept | DEFERRED / BACKEND_BLOCKED | NOT READY; backend candidate-detail contract absent | HD-EMP-03 |
| `EMP-006` Candidate Request | NOT_ROUTABLE action from candidate list/result | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-094` after predecessors | HD-EMP-03 |
| `EMP-007` Employer Requests | `/employer/requests` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-095` after predecessors | HD-EMP-04 |
| `EMP-008` Employer Request Detail | `/employer/requests/:requestId` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-095` after predecessors | HD-EMP-04 |

Common requirements for all approved Employer screens: authenticated Employer actor,
loading/ready/error+retry states, responsive 1440/768/390 behavior, no horizontal
overflow on mobile, RTL-safe logical layout, WCAG 2.2 AA intent, accessible status
and error messaging, and privacy-safe handling of missing/foreign resources.

## 4. Human-Approved Decisions

**HD-EMP-01 — Employer Home / Landing. APPROVED.** `EMP-001` is a focused
Employer recruiting workspace entry. Its primary purpose is to provide clear
entry to Candidate Search and Employer Requests. Do not invent dashboard metrics,
recruiting KPIs, analytics, activity feeds, organization statistics, candidate
counts, or request counts unless separately backed by authoritative API data.
Employer profile / organization readiness may be represented only where existing
authority proves it is a prerequisite or actionable state. Core recruiting
workflow remains primary.

**HD-EMP-02 — Candidate Search / Results / Empty States. APPROVED.**
`EMP-002..004` use current API-backed candidate search only. Approved filters are
pagination, license country, current country, minimum years of experience,
skills, and language. Do not invent sorting or additional filters. Approved
result data is limited to the verified safe candidate-list DTO fields: nurse
profile identifier internally for authorized actions, headline, professional
summary, license country name, current country name, years of experience, skills,
languages, certificates summary/count, latest experience title, and education
summary. The identifier does not need to be displayed. `EMP-004` must cover
initial/browse state where applicable, no-results state, filtered-empty state,
and clear/reset-filters action where appropriate.

**HD-EMP-03 — Candidate Detail / Contact Request. APPROVED.** `EMP-005` Candidate
Detail is deferred for v1 until an authoritative candidate-detail API/DTO/privacy
contract exists. Do not invent a candidate-detail screen from the list DTO and do
not expose additional candidate information for design completeness. `EMP-006`
Candidate Request may originate from an authorized candidate result/list item,
uses existing `nurseProfileId`-backed request authority internally, presents a
clear request/contact action, and requires a simple confirmation before creation.
The confirmation must not introduce additional form fields, message/note/reason
fields, or a Candidate Detail prerequisite. The design must distinguish eligible
request action, submission/loading, success, retryable failure, and duplicate or
conflict outcome. Duplicate/conflict UX communicates that an active/existing
request prevents another request and may provide a path to Employer Requests; it
must not invent backend transitions.

**HD-EMP-04 — Employer Requests. APPROVED.** `EMP-007` and `EMP-008` use the
current `ContactRequestDto` as sufficient v1 product/design authority. Use only
fields exposed by current authoritative DTOs. Approved Employer-facing statuses
are Pending, Approved, Rejected, and Cancelled. The design may provide readable
presentation of those statuses but must not invent additional states. `EMP-007`
includes request list, current safe candidate/request snapshot data, status,
timestamps where useful, status filtering where supported, backend pagination,
and empty/no-results/error states. `EMP-008` presents request detail using current
`ContactRequestDto` only; no richer candidate/private profile data. Ownership,
not-found, and restricted states follow existing authority. Cancel is exposed
only for Employer-owned Pending requests, requires confirmation, and is hidden for
terminal statuses. No additional request actions are approved.

## 5. Backend / API Authority

Candidate search authority:

- `GET /api/v1/recruitment/candidates`.
- Query authority: `page`, `pageSize`, `licenseCountryId`, `currentCountryId`,
  `minimumYearsOfExperience`, `skills`, `languageId`.
- Safe list DTO fields: `nurseProfileId`, `headline`, `professionalSummary`,
  `licenseCountryName`, `currentCountryName`, `yearsOfExperience`, `skills`,
  `languages`, `certificatesSummary`, `certificatesCount`,
  `latestExperienceTitle`, `educationSummary`.
- Candidate detail authority: absent. `T-FE-093` verified no candidate detail
  route/DTO/test/OpenAPI path for `EMP-005`.

Employer profile / organization authority:

- `GET/PUT /api/v1/me/employer-profile` with `EmployerProfileDto` (`id`,
  `userId`, `jobTitle`, `department`).
- `GET/PUT /api/v1/me/employer-profile/organization` with
  `EmployerOrganizationDto` (`id`, `employerProfileId`, `name`, `type`,
  `websiteUrl`, `countryId`, `countryName`, `city`, `addressLine1`,
  `addressLine2`, `postalCode`, `description`).
- These may support prerequisite/actionable readiness only; they do not approve
  dashboards, metrics, counts, or organization analytics.

Employer request authority:

- `POST /api/v1/recruitment/contact-requests` with `CreateContactRequestRequest`
  containing `nurseProfileId`.
- `GET /api/v1/recruitment/contact-requests` list with page/pageSize/status.
- `GET /api/v1/recruitment/contact-requests/{id}` detail.
- `POST /api/v1/recruitment/contact-requests/{id}/cancel` cancel.
- `ContactRequestDto` fields: `id`, `nurseProfileId`, `status`,
  `candidateHeadline`, `candidateLicenseCountryName`,
  `candidateCurrentCountryName`, `createdAt`, `updatedAt`, `respondedAt`,
  `cancelledAt`.
- Status vocabulary: `Pending`, `Approved`, `Rejected`, `Cancelled`.
- Cancel authority: Employer-owned Pending requests only; terminal statuses do
  not expose Cancel.

## 6. Privacy Boundaries

Do not expose candidate or request internals not present in approved DTOs.
Specifically forbidden: email, username, user ID, license number, CV/storage/file
data, private profile data, internal entities/navigation entities, roles,
permissions, password/token fields, provider/internal paths, raw backend errors,
message/note/reason fields, or any field not authorized by the safe recruitment
DTOs. `nurseProfileId` may be used internally for authorized request creation and
does not need to be displayed.

## 7. Per-Screen Contracts

### EMP-001 — Employer Home

- Purpose: focused Employer recruiting workspace entry.
- Actor/audience: authenticated Employer.
- Route/routability: `/employer`, `EMPLOYER_HOME`.
- Data: optional employer profile/organization prerequisite/readiness facts only
  where existing authority proves them; no counts or metrics.
- Actions: navigate to Candidate Search; navigate to Employer Requests; optional
  actionable profile/organization readiness link if required by backend
  prerequisite state.
- States: loading, ready, profile/organization prerequisite guidance where
  applicable, error+retry, access denied by role guard.
- Navigation: primary exits are `/employer/candidates` and `/employer/requests`.
- Privacy: do not expose internal employer profile IDs or organization internals.
- Responsive/RTL/accessibility: two-entry workspace may adapt from card/grid to
  single-column; logical layout; clear landmarks and accessible link/button names.
- Non-goals: dashboard, analytics, activity feed, organization statistics,
  candidate counts, request counts, KPIs.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-090` must verify current API
  and mounted routes.
- Remaining gaps: none for v1 design contract; metrics remain unapproved.

### EMP-002 — Candidate Search

- Purpose: let Employers search/browse candidates using current backend filters.
- Actor/audience: authenticated Employer.
- Route/routability: `/employer/candidates`, `EMPLOYER_CANDIDATES`.
- Data: approved filter inputs only: pagination, license country, current country,
  minimum years of experience, skills, language.
- Actions: apply filters; clear/reset filters; page through results.
- States: initial/browse, loading, results, no results, filtered-empty,
  validation/problem-details error+retry.
- Navigation: results remain on the same route; request action belongs to
  `EMP-006` and must not require Candidate Detail.
- Privacy: filters and results must not reveal forbidden candidate fields.
- Responsive/RTL/accessibility: filters reflow on tablet/mobile; controls remain
  keyboard accessible and labelled; no horizontal scrolling.
- Non-goals: sorting, additional filters, saved searches, recommendations,
  ranking, matching score.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-091` must verify generated
  contract and reference-data availability.
- Remaining gaps: no custom sort/additional filter authority.

### EMP-003 — Candidate Results

- Purpose: present safe candidate search results.
- Actor/audience: authenticated Employer.
- Route/routability: `/employer/candidates`, `EMPLOYER_CANDIDATES`.
- Data: safe list fields only: headline, professional summary, license country
  name, current country name, years of experience, skills, languages,
  certificates summary/count, latest experience title, education summary;
  `nurseProfileId` internal only for authorized actions.
- Actions: request contact via `EMP-006` when action is eligible; pagination.
- States: loading, populated results, empty/no-results, per-action loading where
  `EMP-006` is integrated, error+retry.
- Navigation: no Candidate Detail navigation in v1.
- Privacy: never display email, username, user ID, license number, CV/storage/file
  data, private profile fields, raw internal IDs, or internal entities.
- Responsive/RTL/accessibility: card/list composition may prioritize fields;
  hierarchy can be designed from approved fields only; actions meet touch targets.
- Non-goals: candidate detail drill-in, scores, recommendations, recruiter notes,
  private contact data.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-091` plus `T-FE-094` for
  request action integration.
- Remaining gaps: Candidate Detail remains deferred.

### EMP-004 — Candidate Empty / Filtered States

- Purpose: define initial, no-results, and filtered-empty states for candidate
  search.
- Actor/audience: authenticated Employer.
- Route/routability: `/employer/candidates`, `EMPLOYER_CANDIDATES`.
- Data: current filter state and pagination state only.
- Actions: clear/reset filters where filters are active; retry on load failure.
- States: initial/browse, no candidates available, filtered-empty, loading,
  recoverable error.
- Navigation: remain on Candidate Search; do not invent alternative destinations.
- Privacy: empty states must not imply hidden private candidate counts.
- Responsive/RTL/accessibility: empty-state action is reachable and labelled;
  logical text alignment.
- Non-goals: recommendations, saved-search prompts, alerts, candidate counts.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-091`.
- Remaining gaps: none for v1 design contract.

### EMP-005 — Candidate Detail

- Purpose: deferred v1 candidate detail concept.
- Actor/audience: authenticated Employer if reopened later.
- Route/routability: NOT_ROUTABLE; no approved path.
- Data: none approved beyond list DTO; list DTO must not be stretched into a
  detail profile.
- Actions: none in v1.
- States: deferred/backend-blocked only.
- Navigation: no Candidate Detail link in v1.
- Privacy: no additional candidate data may be exposed for design completeness.
- API/backend authority: missing candidate-detail route/DTO/privacy contract;
  `T-FE-093` verified the gap.
- Responsive/RTL/accessibility: not applicable until reopened.
- Non-goals: detailed profile, CV/license/contact/private candidate data.
- Stitch/design readiness: deferred; do not generate active detail screen.
- Implementation readiness: not ready.
- Remaining gaps: candidate-detail API, DTO, privacy boundary, and ownership
  rules.

### EMP-006 — Candidate Request

- Purpose: initiate contact/request from an authorized candidate result/list item.
- Actor/audience: authenticated Employer.
- Route/routability: NOT_ROUTABLE action/state within candidate search/results.
- Data: internal `nurseProfileId` from selected safe candidate result; safe
  visible candidate summary only.
- Actions: open simple confirmation; confirm create request; cancel confirmation;
  navigate to Employer Requests after duplicate/conflict or success where useful.
- States: eligible action, confirmation, submitting/loading, success, retryable
  failure, duplicate/conflict existing-active-request outcome.
- Navigation: may link to `/employer/requests`; does not require Candidate Detail.
- Privacy: confirmation must not introduce message/note/reason fields or expose
  private candidate/contact data.
- API/backend authority: `POST /api/v1/recruitment/contact-requests` with
  `nurseProfileId`; duplicate/conflict exists as backend evidence and is shown as
  a high-level existing-active-request outcome only.
- Responsive/RTL/accessibility: confirmation uses normal accessible document flow
  or approved shared confirmation pattern; keyboard reachable; no hidden form
  fields exposed.
- Non-goals: message forms, recruiter notes, scheduling, detail prerequisite,
  backend transition invention.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-094` must respect `T-FE-093`
  and avoid Candidate Detail dependency.
- Remaining gaps: exact implementation mapping for conflict Problem Details must
  be verified during implementation.

### EMP-007 — Employer Requests

- Purpose: list Employer-owned contact requests.
- Actor/audience: authenticated Employer.
- Route/routability: `/employer/requests`, `EMPLOYER_REQUESTS`.
- Data: current `ContactRequestDto` safe fields only: candidate headline, license
  country snapshot, current country snapshot, status, timestamps, IDs for routing
  only.
- Actions: status filter where supported; pagination; view request detail; cancel
  Pending request where backend authority permits and after confirmation.
- States: loading, populated list, empty list, filtered-empty, error+retry,
  cancelling, cancelled/reconciled after backend success.
- Navigation: detail link to `/employer/requests/:requestId`; optional path back
  to Candidate Search when empty if approved by shell/navigation authority later.
- Privacy: no private candidate data, employer internal IDs, or message/reason
  fields.
- API/backend authority: list endpoint with page/pageSize/status; statuses
  Pending, Approved, Rejected, Cancelled.
- Responsive/RTL/accessibility: list/table/card transformation as appropriate;
  filters labelled; status text readable without color dependence.
- Non-goals: additional statuses, bulk actions, approve/reject actions, notes,
  internal ownership display.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-095` after predecessors.
- Remaining gaps: implementation must verify generated status typing and error
  mapping.

### EMP-008 — Employer Request Detail

- Purpose: show one Employer-owned contact request using current DTO only.
- Actor/audience: authenticated Employer.
- Route/routability: `/employer/requests/:requestId`, `EMPLOYER_REQUEST_DETAIL`.
- Data: current `ContactRequestDto` only: request ID for routing, nurse profile ID
  internal only where needed, safe candidate snapshots, status, timestamps.
- Actions: Cancel only for Employer-owned Pending requests; confirmation required;
  back to request list.
- States: loading, ready, not-found/foreign/missing as privacy-safe not found,
  cancelling, cancelled/reconciled, terminal status without Cancel, error+retry.
- Navigation: back to Employer Requests; no Candidate Detail link in v1.
- Privacy: no richer candidate/private profile data, no message/rejection reason,
  no internal profile/entity data.
- API/backend authority: detail endpoint returns `ContactRequestDto`; cancel uses
  pending Employer-owner transition.
- Responsive/RTL/accessibility: detail sections stack on mobile; confirmation is
  accessible; status text not color-only.
- Non-goals: richer candidate profile, additional actions, backend transition
  invention, private contact information.
- Stitch/design readiness: ready after this packet.
- Implementation readiness: not started; later `T-FE-095` after predecessors.
- Remaining gaps: none for v1 design contract; richer detail requires new backend
  authority.

## 8. Backend Gap Disposition

| Gap | Disposition after HD-EMP decisions |
|---|---|
| Candidate detail API / DTO / privacy boundary | Still unresolved. `EMP-005` is explicitly deferred/backend-blocked, so this does not block the approved v1 Employer design flow. |
| Candidate request initiation contract | Product UX resolved for v1: candidate list/result action → simple confirmation → create request. Implementation still verifies current generated/API error details. |
| Duplicate-request semantics | Existing backend conflict evidence is documented as high-level duplicate/existing-active-request UX. No backend transition is invented. |
| Request status vocabulary and cancellation rules | V1 status labels and pending-only cancel visibility are approved subject to existing backend authority. |
| Request-detail DTO sufficiency | Current `ContactRequestDto` is explicitly accepted as sufficient for `EMP-008` v1. |
| Candidate-list filters/pagination finality | Current API-backed filters and pagination are explicitly accepted for v1; no custom sorting/additional filters are required. |

## 9. Approval Record

- 2026-09-21: Human technical lead approved HD-EMP-01..04.
- `EMP-001`, `EMP-002`, `EMP-003`, `EMP-004`, `EMP-006`, `EMP-007`, and
  `EMP-008` are design-contract ready for v1.
- `EMP-005` is deferred/backend-blocked and must not be generated or implemented
  as an active v1 candidate-detail screen.
- `GATE-FE-T085` is verified by this packet because every `EMP-001..008` row now
  has an explicit approved/deferred decision, backend authority boundaries,
  privacy constraints, and remaining gaps recorded.
