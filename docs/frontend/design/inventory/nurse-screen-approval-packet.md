# Nurse Screen Approval Packet — T-FE-052 / GATE-FE-T052

```yaml
document_id: NPS-DES-INV-NURSE-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-16
prepared_by: Nurse Profile Technical Readiness campaign (feat/2026-09-16-nurse-profile-overview)
gate: GATE-FE-T052
gate_status: VERIFIED (closed by human-approved decisions D1-D10; extended by D11-D14, recorded 2026-09-16)
authorization: Decisions D1-D10 and the additional content authority below were approved
  by the human technical lead on 2026-09-16. NUR-001/NUR-002 design is APPROVED.
  NUR-003..012 remain BLOCKED with screen-specific design choices explicitly deferred
  to their owning implementation gates (permitted by the ledger gate structure).
  NUR-013 remains BLOCKED (no backend contract). Approval does not itself start
  implementation; T-FE-056 remains NOT STARTED until separately authorized.
```

## 1. Purpose

This is the T-FE-052 Nurse screen approval packet covering NUR-001..013. It establishes the family architecture, per-screen backend contracts, required states, and the explicit open design decisions. Per the frontend ledger, GATE-FE-T052 requires "Nurse approval packet with decisions per screen" — the decisions are collected in Section 5 and must be answered by the technical lead before any Nurse screen implementation (T-FE-056+) may start.

## 2. Family architecture established by repository authority

- Canonical routes (approved): `NURSE_ENTRY /nurse`, `NURSE_PROFILE_OVERVIEW /nurse/profile`, `NURSE_PROFILE_PERSONAL_INFORMATION /nurse/profile/personal-information`, `NURSE_PROFILE_EXPERIENCE /nurse/profile/experience`, `NURSE_PROFILE_EDUCATION /nurse/profile/education`, `NURSE_PROFILE_CERTIFICATES /nurse/profile/certificates`, `NURSE_PROFILE_SKILLS /nurse/profile/skills`, `NURSE_PROFILE_LANGUAGES /nurse/profile/languages`, `NURSE_PROFILE_CV /nurse/profile/cv`, `NURSE_CONTACT_REQUESTS /nurse/contact-requests`.
- Access: every Nurse route is AUTHENTICATED + ROLE `Nurse` (UX only; backend `NurseRoleGuard` is authoritative and returns 401 unauthenticated / 403 non-Nurse).
- Composition: overview page + separate section pages; add/edit modes remain inside owning sections (page-registry rationale). No route changes are proposed by this packet.
- Backend contracts: all nurse profile operations now publish typed success responses (OpenAPI + generated client corrected in this campaign; see Section 8).

## 3. Per-screen inventory (NUR-001..013)

Common state contract for every screen: loading / ready / error+retry (shared `np-loading-error-retry`), 401/403 handled by guards (route level) and problem-details display (in-session), responsive 1440/768/390 breakpoints with canonical gutters, WCAG 2.2 AA, RTL-ready logical properties, Storybook stories for approved states. Common backend authorization: `RequireAuthorization()` + `NurseRoleGuard` (401/403).

| Screen | Route | User goal | Backend contract | Behavior | States beyond common | Status | Approval |
|---|---|---|---|---|---|---|---|
| NUR-001/002 Overview | `/nurse/profile` | View professional profile summary + navigate to sections | `GET /me/nurse-profile` (+ section GETs) | READ-ONLY | first-time 404 empty state; per-section empty presentation; CV metadata (approved D1-D10) | NOT STARTED | APPROVED (design; implementation pending T-FE-056 authorization) |
| NUR-003 Personal information | `/nurse/profile/personal-information` | Edit base professional profile | `GET /me/nurse-profile`, `PUT` (upsert create-or-update) | READ + EDIT (single form) | validation states; 409 invalid-country problem-details | NOT STARTED | APPROVED (design; implementation pending T-FE-057 within the first Nurse vertical slice) |
| NUR-004/005 Experience | `/nurse/profile/experience` | Manage employment history | `GET/POST/PUT/DELETE /experiences` | LIST + CREATE + EDIT + DELETE | delete confirmation; date validation (End≥Start when provided, End null when IsCurrent — one-directional, End may be null when not current); empty list | NOT STARTED | APPROVED (design; implementation pending T-FE-058 authorization) |
| NUR-006/007 Education | `/nurse/profile/education` | Manage education | `GET/POST/PUT/DELETE /education` | LIST + CREATE + EDIT + DELETE | delete confirmation; date validation (End≥Start when both) | NOT STARTED | APPROVED (design; implementation pending T-FE-059 authorization) |
| NUR-008/009 Certificates | `/nurse/profile/certificates` | Manage certificates | `GET/POST/PUT/DELETE /certificates` | LIST + CREATE + EDIT + DELETE | delete confirmation; URL validation (absolute http/https); expiry validation | NOT STARTED | APPROVED (design; implementation pending T-FE-060 authorization) |
| NUR-010 Skills | `/nurse/profile/skills` | Manage free-text skill tags | `GET/PUT /skills` (full replace) | READ + EDIT (collection editor) | normalized duplicate rejection; ≤50 cap; empty collection | NOT STARTED | APPROVED (design; implementation pending T-FE-062 authorization) |
| NUR-011 Languages | `/nurse/profile/languages` | Manage languages + proficiency | `GET/PUT /languages` (full replace) | READ + EDIT (collection editor) | ≤20 cap; distinct LanguageId; proficiency enum (Beginner/Intermediate/Advanced/Fluent/Native) | NOT STARTED | APPROVED (design; implementation pending T-FE-062 authorization) |
| NUR-012 CV | `/nurse/profile/cv` | Upload/replace/delete CV document | `GET /cv` (metadata), `POST /cv` (multipart `file`), `DELETE /cv` (204) | READ + UPLOAD + DELETE | upload constraints (.pdf/.doc/.docx, ≤5MB); metadata-only display; 404 when none; delete confirmation | NOT STARTED | APPROVED (design; implementation pending T-FE-064 authorization) |
| NUR-013 Profile completion | — (NOT_ROUTABLE) | Professional profile completeness | NONE — no backend contract exists | n/a | n/a | BLOCKED (BACKEND + CONTRACT_CLARIFICATION + DESIGN) — must remain excluded until a backend contract is separately approved | BLOCKED (deferred) |
| (related) Contact requests | `/nurse/contact-requests` | Review/approve/reject employer contact requests | `GET /contact-requests` (paginated), `POST /{id}/approve`, `POST /{id}/reject` | READ + ACT | pagination; status filter | NOT STARTED (separate slice after profile family) | BLOCKED (design deferred to owning gate) |

## 4. Empty / first-time semantics (backend-authoritative, verified)

- Sign-up creates User + Nurse role only; no `NurseProfile` row exists.
- `GET /api/v1/me/nurse-profile` returns **404 Problem Details ("Nurse profile was not found.")** when no professional profile exists. Section endpoints behave the same (404) when the base profile row is missing; once the base profile exists, section lists return `200 []` when empty and CV GET returns 404 when no document exists.
- Frontend distinction: 404 on base GET = "no professional profile yet" (first-time empty state + create CTA → NUR-003); 200 with sparse/null fields = "exists but incomplete" (presentation only).
- `PUT /me/nurse-profile` is upsert: the first save creates the professional profile. T-FE-056 must design for the 404 first-time state; the semantics must NOT be changed to 200/null.

## 5. HUMAN APPROVED DECISIONS — NUR-001/002 Overview (recorded 2026-09-16)

The following decisions were approved by the human technical lead. They supersede the previous READY_FOR_HUMAN_DECISION placeholders and are binding design authority for T-FE-056.

**D1 — Overview composition. APPROVED.** One full-width Professional Identity / Base Profile summary area at the top, followed by concise section-summary cards. No tabs, no dashboard layout, no single giant editable profile form. Desktop: full-width identity area, key professional facts beneath it, profile section cards in a balanced two-column layout. Mobile: deliberate single-column layout.

**D2 — Sections shown on the overview. APPROVED.** The overview may summarize: Experience, Education, Certificates, Skills, Languages, CV. Base professional information remains the dominant top section.

**D3 — Summary depth. APPROVED.**
- Experience: show the current/latest item; show a factual remaining-record count when additional records exist.
- Education: show the most relevant/latest record; show remaining-record count when applicable.
- Certificates: show at most two certificates; show remaining count when applicable; do NOT invent verification status.
- Skills: show a concise wrapped tag/chip preview.
- Languages: show language name + backend-supported proficiency value only. Valid proficiency values are exactly: Beginner, Intermediate, Advanced, Fluent, Native.
- CV: metadata summary only.
- Do not display unsupported fields such as employment type.

**D4 — CV on overview. APPROVED.** Show CV metadata on the overview when a CV exists: file name, file size, uploaded date/time when available. Do NOT add Preview CV, Download CV, or a document reader — those capabilities do not exist in the current backend contract.

**D5 — First-time / 404 state. APPROVED.** `GET /me/nurse-profile` returning 404 represents "No professional Nurse Profile has been created yet." Render a calm first-time empty state. Do not treat this as a generic application error. Do not render all missing sections as warnings.

**D6 — First-time CTA. APPROVED.** Destination: `/nurse/profile/personal-information`. Preferred label: "Add personal information". T-FE-056 must NOT create a dead link: if NUR-003 / T-FE-057 is not yet implemented and mounted when T-FE-056 ships, render the first-time empty state without an actionable navigation button and add the CTA when NUR-003 becomes available. Do not invent `/nurse/profile/create`.

**D7 — Section navigation actions. APPROVED.** Do not expose Manage/View links to section routes until those routes are actually implemented and mounted. As each Nurse section ships, its overview navigation action may become visible. No dead navigation affordances.

**D8 — Profile completion. APPROVED.** Do NOT display: completion percentage, completion progress ring, profile score, profile strength, "Active Career Profile" status, or professional completion state. NUR-013 remains BLOCKED/deferred. The overview may display factual section information only, such as "3 positions" or "No certificates added yet", without deriving an overall completion score.

**D9 — Nurse entry route. APPROVED.** `/nurse` redirects to `/nurse/profile`. The Nurse Profile Overview is the initial Nurse-area landing page. Do not create a separate Nurse dashboard.

**D10 — Responsive behavior. APPROVED.** Desktop: full-width professional identity area, four key facts where space permits, two-column profile-section summary grid. Tablet: identity remains full width, key facts reflow to 2x2 as necessary, section grid adapts based on available width. Mobile: single-column content flow, key facts stack naturally, chips/tags wrap, no horizontal scrolling, touch targets remain accessible. All layouts must remain RTL-ready using logical layout behavior.

### 5.1 Additional content authority (approved)

Use only currently supported base-profile information: headline, professionalSummary, licenseNumber, licenseCountry, currentCountry, yearsOfExperience, isAvailableForRecruitment. Do NOT invent: degree/professional suffixes beside the user's name, employment type, credential verification badges, recruiter/profile view counts, ratings, profile status, privacy levels, professional completion percentage, or unsupported recruitment states. Recruitment availability may be shown clearly as "Available for recruitment" or "Not available for recruitment" because this maps directly to the existing boolean contract.

### 5.2 Additional human-approved decisions D11–D14 (recorded 2026-09-16, first Nurse vertical slice authorization)

**D11 — Country lookup. APPROVED: Option A.** A bounded read-only Country lookup endpoint `GET /api/v1/countries` is authorized: authenticated access (`RequireAuthorization()`), active countries only, deterministic sort by Name, response fields only `id`, `name`, `code`; no pagination, no Country CRUD, no Admin UI, no new permission model, and no hardcoded frontend country list. The endpoint exists only to provide authoritative reference data to authenticated frontend forms such as Nurse Personal Information.

**D12 — Recruitment availability control. APPROVED.** Use the existing shared checkbox control. Label: "Available for recruitment". Helper text explains accurately and concisely that enabling this allows the Nurse profile to appear in employer candidate search when the other backend eligibility conditions are satisfied. No switch component, visibility tiers, schedules, recruiter preferences, or privacy levels.

**D13 — Save success behavior. APPROVED.** Successful save of Personal Information navigates directly to `/nurse/profile`. No separate success page, no invented success workflow.

**D14 — Overview edit action. APPROVED.** Place "Edit personal information" inside the Professional Identity / base-profile area of the populated Nurse Profile Overview. It navigates to `/nurse/profile/personal-information`.

### 5.3 Additional human-approved decisions HD-1–HD-6 (recorded 2026-09-16, T-FE-058 Experience authorization)

**HD-1 — Date control. APPROVED.** Use a native `<input type="date">` wrapped in the existing shared form-control architecture as `NpDateControl`, following the same label/helper/error/describedBy conventions as the existing shared controls, with a string `YYYY-MM-DD` value contract. No Material Datepicker, no custom date parsing/masking, no new dependency. Mobile accessible, keyboard accessible, RTL-safe.

**HD-2 — List/form interaction. APPROVED.** One canonical route `/nurse/profile/experience`: NUR-004 = list/management state, NUR-005 = add/edit form state, with same-page view switching between list, create, and edit. Do NOT create `/experience/new` or `/experience/:id/edit`. Do NOT introduce modal/dialog/drawer architecture.

**HD-3 — Current role / End Date. APPROVED.** When `isCurrent = true`: disable End Date and clear End Date before submit (the previous UI value may be retained in memory so it can be restored if the user unchecks Current before saving). When `isCurrent = false`: End Date remains OPTIONAL because the backend contract allows null — do NOT invent a frontend requirement that End Date must exist; if End Date is provided, validate `endDate >= startDate`. Helper text for the current-role checkbox: "Mark this if this role is ongoing." Do NOT state "Only one position should be current" — the backend permits multiple current experiences, so the frontend must not invent a uniqueness rule.

**HD-4 — Delete confirmation. APPROVED.** Inline two-step confirmation inside the Experience card. First Delete action shows an inline confirmation such as: Delete "{JobTitle} at {FacilityName}"? This cannot be undone. Actions: Delete, Keep. Requirements: a second explicit action is required before the API call; no dialog, no drawer, no new confirmation service; accessible normal document flow, keyboard/screen-reader friendly, mobile-safe.

**HD-5 — Overview action. APPROVED.** After T-FE-058 ships, the Experience summary card on `/nurse/profile` gets exactly one navigation action: "Manage experience" → `/nurse/profile/experience`. Destructive Delete actions inside Experience use quiet danger-text styling; this is not approved as a global destructive-action standard outside this feature.

**HD-6 — Copy/content polish. APPROVED.** Do NOT add a recruiter-facing explanatory note to the Experience page; it is primarily for the Nurse managing their employment history. Use only factual field/helper content. For long descriptions: concise 2–3 line preview in list cards with an accessible Show more / Show less affordance when needed. Do not create a dedicated Experience detail page.

Backend-rule correction (binding): the actual rule is one-directional — `isCurrent = true` → End Date must be null. When `isCurrent = false`, End Date may still be null. Any shorthand implying "End Date is null iff IsCurrent" is stale and must not be enforced.

### 5.4 Additional human-approved decisions HD-E1–HD-E2 (recorded 2026-09-16, T-FE-059 Education authorization)

**HD-E1 — Education date-line rendering. APPROVED.** Education dates are optional. Render: startDate + endDate → "{start} → {end}"; startDate only → "{start} → Present"; endDate only → "{end}"; neither date → omit the date line entirely. Do NOT render "Dates not provided" or any warning/status implying missing dates are invalid. Use the same TZ-safe calendar-date formatting pattern already verified in Nurse Experience.

**HD-E2 — Overview education action. APPROVED.** After T-FE-059 ships, the Education summary card on `/nurse/profile` must expose "Manage education" → `/nurse/profile/education`. This is the same established CRUD-destination wording pattern as "Manage experience".

### 5.5 Additional human-approved decisions HD-C1–HD-C3 (recorded 2026-09-16, T-FE-060 Certificates authorization)

**HD-C1 — Expiration UX. APPROVED.** `expirationDate` remains a plain OPTIONAL field. Do NOT implement a "Does not expire" checkbox, Active/Expired/Valid/Expiring-soon status, renewal state, or any date-derived badge/color/status. The backend represents only `expirationDate: DateOnly | null`, and null means only that no expiration date was supplied. Approved helper text: "Optional. Add the expiration date if known." Do NOT use wording such as "Leave empty if it does not expire" because the backend does not distinguish "never expires" from "not provided".

**HD-C2 — Credential URL. APPROVED.** When `credentialUrl` is present, expose a safe clickable action with the accessible text "Open credential link" (or equivalently explicit wording). Do NOT display it as Verify/Verified/Official/trusted/validated credential — the system validates URL format only and does NOT verify the external credential. Requirements: http/https only per backend validation; normal accessible external link; `target="_blank"` with `rel="noopener noreferrer"` where a new tab matches app conventions; no unsafe schemes; no metadata fetching; no preview cards; no verification attempt. This is a nurse-facing convenience for the URL the Nurse entered.

**HD-C3 — Credential ID. APPROVED.** Show `credentialId` in Certificate list cards when present, as secondary metadata (example label: "Credential ID" followed by the value). Do not show any verification badge beside it.

### 5.6 Additional human-approved decisions HD-S1–HD-L2 (recorded 2026-09-16, T-FE-062 Skills + Languages authorization)

**HD-S1 — Skills editor. APPROVED.** Use a feature-local chip/tag editor under Nurse Skills: text input → Add button or Enter → removable skill chip; one page-level Save performs the full-replace PUT. No generic shared chip editor, no taxonomy, no autocomplete, no reference-data catalog, no drag/reorder UI. Keyboard accessible, mobile wrapping, each chip has an accessible Remove action. User ordering is not authoritative because the backend returns Skills ordered by Name.

**HD-S2 — Skills normalization/validation + contract correction. APPROVED.** Mirror backend normalization: trim, collapse internal whitespace runs, preserve casing, case-insensitive duplicate comparison on the normalized form; blank skills invalid; max 50 skills; max 100 characters per skill. Contract correction (binding): the `UpdateNurseSkills` validator lacks `MaxLength(100)` while EF and recruitment cap at 100 — fix the backend validator with RED-first boundary tests (100 accepted, 101 rejected with 400 behavior). No migration, no column widening, no semantic change.

**HD-L1 — Language lookup. APPROVED.** Implement authoritative `GET /api/v1/languages`: `RequireAuthorization()`, active-only, Name-ordered, no pagination, response `[{id, name, code}]`, typed 200 metadata; no CRUD/admin/permission model, no frontend hardcoding. Follow with focused backend tests, Development OpenAPI recapture, client regen, and a thin `LanguagesApi` facade.

**HD-L2 — Languages editor. APPROVED.** Row-based editing (Language select + Proficiency select + Remove); "Add language" creates one incomplete row with **no** preselected language and **no** default proficiency — both required before Save (placeholders "Select language"/"Select proficiency"). Exact proficiency values only: Beginner, Intermediate, Advanced, Fluent, Native. Duplicate Language IDs invalid; max 20 rows; empty collection valid and clears all; no reorder UI.

### 5.7 Additional human-approved decisions HD-V1–HD-V3 (recorded 2026-09-16, T-FE-064 CV authorization)

**HD-V1 — Upload trigger. APPROVED.** Explicit upload only: choose file → review filename/size → explicit Upload CV / Replace CV action → multipart upload. No auto-upload after selection, for first upload and replacement alike.

**HD-V2 — Success announcements. APPROVED.** First upload announces "CV uploaded."; replacement announces "CV replaced." via Announcer/live-region per shared patterns.

**HD-V3 — Replace entry. APPROVED.** Replace CV enters the select-file state directly with no pre-picker confirmation; the explicit Replace CV submit action is the deliberate confirmation. Delete CV keeps the established TwoStepConfirmation pattern.

## 6. Later-slice design dependencies (recorded, NOT solved here)

- Date input choice for experience/education/certificates: no date control exists in shared form-controls (`NpTextInputControl` types: text/email/password/search/number only). Options: native `<input type="date">` wrapper vs Material datepicker (not used anywhere yet). LATER DECISION (T-FE-058+).
- Skills chips/tag editor: no chips component exists. LATER DECISION (T-FE-062).
- Delete confirmation pattern: none exists (no dialogs in production yet). LATER DECISION (T-FE-058+).
- CV upload UX: first multipart in frontend; `uploadNurseCv({body:{file: Blob}})`; progress UI, error display, file picker. LATER DECISION (T-FE-064).
- Country lookup strategy: NO `/countries` endpoint exists (verified — 0 OpenAPI paths; seed data only). NUR-003/NUR-004/NUR-006 country pickers need either an approved new lookup API (backend work, separate authorization) or a deferral decision. Does NOT block read-only overview (DTOs carry `*CountryName` display strings).
- Language lookup strategy: same gap for NUR-011 (`LanguageId` Guid + proficiency; no `/languages` catalog endpoint). LATER DECISION.
- NUR-013 completion model: no backend contract; remains BLOCKED. No completion percentage/score/guard may be built.

## 7. Frontend architecture (validated against route authority; no production files created)

```
features/nurse/profile/
  overview/                    # NUR-001/002 routed container + section-summary presentational components
  personal-information/        # NUR-003 form
  experience/                  # NUR-004/005 list + form
  education/                   # NUR-006/007 list + form
  certificates/                # NUR-008/009 list + form
  skills/                      # NUR-010 editor
  languages/                   # NUR-011 editor
  cv/                          # NUR-012 management
core/api/nurse-profile-api.ts  # facade over generated nurse operations
```

Ownership boundaries: routed container pages own data loading + state (pattern: `admin-user-detail.ts` signals + `np-loading-error-retry`); overview section summaries are local presentational components (not shared — extract only after ≥2 consumers exist); section list/form components stay feature-local; shared reuse = `form-controls`, `form-validation`, `loading-error-retry`, `safe-return`; the API facade is the single boundary over generated fns; DTO→view-model mapping via `dto-adapters.ts` (`normalizeNullable`, `adaptDto`) where generated nullability (`| null`) needs normalization for templates. No premature generic abstractions.

## 8. API facade design (specification only)

`core/api/nurse-profile-api.ts` follows the established facade pattern (`profile-api.ts`, `admin-users-api.ts`): `@Injectable({providedIn:'root'})`, `inject(HttpClient)` + `inject(ApiConfiguration)`, wrap generated fn, `.pipe(map(r => r.body))`.

- First overview slice wraps: `getCurrentNurseProfile`, `listCurrentNurseExperiences`, `listCurrentNurseEducation`, `listCurrentNurseCertificates`, `listCurrentNurseSkills`, `listCurrentNurseLanguages`, `getCurrentNurseCv` (7 reads).
- Later CRUD slices add: `upsertCurrentNurseProfile`; 3× (create/update/delete) for experience/education/certificates; `updateNurseSkills`, `updateNurseLanguages`; `uploadNurseCv`, `deleteNurseCv`.
- After this campaign's typing fix, generated functions return real DTO types; DTO adapters are needed only for nullability normalization and view-model shaping, not for inventing fields.

## 9. Contract corrections completed by this campaign (backend evidence for the packet)

- All 21 nurse/recruitment response operations now publish typed success schemas in canonical OpenAPI (was: schema-less 200s); section DELETEs now correctly declare 204 only. New generated DTO models: `NurseProfileDto`, `NurseExperienceDto`, `NurseEducationDto`, `NurseCertificateDto`, `NurseLanguageDto`, `NurseSkillDto`, `NurseCvDocumentDto`, `CandidateListItemDto`, `CandidateLanguageDto`, `ReceivedContactRequestDto`, `ContactRequestDto` + 3 `PaginatedResultOf*` wrappers.
- Persistence widened to match the Application validation contract (JobTitle, Degree, FieldOfStudy, CredentialId 160→200) + migration `20260915224617_WidenNurseProfileTextConstraints`; validator boundary tests + configuration tests added.
- Recruitment privacy boundary pinned by test: `CandidateListItemDto` exposes only recruitment-safe fields (no userId/email/username/licenseNumber/country IDs/CV/availability flag). `IsAvailableForRecruitment` remains the only discoverability control.
- `/me.isProfileComplete` remains generic onboarding completion (first/last name) only; nurse professional profile does not affect it; no professional completion concept exists in backend.

## 10. Approval record

- 2026-09-16: Human technical lead approved decisions D1–D10 and the additional content authority (Section 5.1) for the Nurse Profile Overview (NUR-001/NUR-002). NUR-001/NUR-002 design is APPROVED; execution remains NOT STARTED pending separate T-FE-056 authorization.
- NUR-003..NUR-012 remain BLOCKED: family architecture and contracts are recorded in this packet, and screen-specific design choices (country/language lookup UX, date control, skills editor, delete confirmation, CV upload UX) are explicitly deferred to their owning implementation/design gates — permitted by the ledger structure because GATE-FE-T057..T064 each require their own per-screen visual evidence.
- NUR-013 remains BLOCKED (no backend completion contract exists).
- GATE-FE-T052 is closed as VERIFIED on the strength of: every NUR screen having an explicit decision, the human-approved overview design authority, backend contract evidence (Section 9), and the recorded per-screen states/dependencies — consistent with the T-FE-040 family-gate precedent.
