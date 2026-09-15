# Nurse Screen Approval Packet — T-FE-052 / GATE-FE-T052

```yaml
document_id: NPS-DES-INV-NURSE-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-16
prepared_by: Nurse Profile Technical Readiness campaign (feat/2026-09-16-nurse-profile-overview)
gate: GATE-FE-T052
gate_status: VERIFIED (closed by human-approved decisions D1-D10, recorded 2026-09-16)
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
| NUR-003 Personal information | `/nurse/profile/personal-information` | Edit base professional profile | `GET /me/nurse-profile`, `PUT` (upsert create-or-update) | READ + EDIT (single form) | validation states; 409 invalid-country problem-details | NOT STARTED | BLOCKED (design deferred to GATE-FE-T057) |
| NUR-004/005 Experience | `/nurse/profile/experience` | Manage employment history | `GET/POST/PUT/DELETE /experiences` | LIST + CREATE + EDIT + DELETE | delete confirmation; date validation (End≥Start, End null iff IsCurrent); empty list | NOT STARTED | BLOCKED (design deferred to GATE-FE-T058) |
| NUR-006/007 Education | `/nurse/profile/education` | Manage education | `GET/POST/PUT/DELETE /education` | LIST + CREATE + EDIT + DELETE | delete confirmation; date validation (End≥Start when both) | NOT STARTED | BLOCKED (design deferred to GATE-FE-T059) |
| NUR-008/009 Certificates | `/nurse/profile/certificates` | Manage certificates | `GET/POST/PUT/DELETE /certificates` | LIST + CREATE + EDIT + DELETE | delete confirmation; URL validation (absolute http/https); expiry validation | NOT STARTED | BLOCKED (design deferred to GATE-FE-T060) |
| NUR-010 Skills | `/nurse/profile/skills` | Manage free-text skill tags | `GET/PUT /skills` (full replace) | READ + EDIT (collection editor) | normalized duplicate rejection; ≤50 cap; empty collection | NOT STARTED | BLOCKED (design deferred to GATE-FE-T062) |
| NUR-011 Languages | `/nurse/profile/languages` | Manage languages + proficiency | `GET/PUT /languages` (full replace) | READ + EDIT (collection editor) | ≤20 cap; distinct LanguageId; proficiency enum (Beginner/Intermediate/Advanced/Fluent/Native) | NOT STARTED | BLOCKED (design deferred to GATE-FE-T062) |
| NUR-012 CV | `/nurse/profile/cv` | Upload/replace/delete CV document | `GET /cv` (metadata), `POST /cv` (multipart `file`), `DELETE /cv` (204) | READ + UPLOAD + DELETE | upload constraints (.pdf/.doc/.docx, ≤5MB); metadata-only display; 404 when none; delete confirmation | NOT STARTED | BLOCKED (CV contract clarified by GATE-FE-T063; upload UX design deferred to GATE-FE-T064) |
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
