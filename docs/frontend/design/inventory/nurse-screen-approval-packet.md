# Nurse Screen Approval Packet — T-FE-052 / GATE-FE-T052

```yaml
document_id: NPS-DES-INV-NURSE-SCREEN-APPROVAL-PACKET
status: READY_FOR_HUMAN_DECISION
created_at: 2026-09-16
prepared_by: Nurse Profile Technical Readiness campaign (feat/2026-09-16-nurse-profile-overview)
gate: GATE-FE-T052
authorization: This packet is preparation evidence only. It does not approve screens.
  All approval_decision values below remain BLOCKED pending human decisions.
  GATE-FE-T052 is NOT VERIFIED by this document.
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
| NUR-001/002 Overview | `/nurse/profile` | View professional profile summary + navigate to sections | `GET /me/nurse-profile` (+ section GETs) | READ-ONLY | first-time 404 empty state; per-section empty presentation; CV metadata presence (D1–D7) | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-003 Personal information | `/nurse/profile/personal-information` | Edit base professional profile | `GET /me/nurse-profile`, `PUT` (upsert create-or-update) | READ + EDIT (single form) | validation states; 409 invalid-country problem-details | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-004/005 Experience | `/nurse/profile/experience` | Manage employment history | `GET/POST/PUT/DELETE /experiences` | LIST + CREATE + EDIT + DELETE | delete confirmation; date validation (End≥Start, End null iff IsCurrent); empty list | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-006/007 Education | `/nurse/profile/education` | Manage education | `GET/POST/PUT/DELETE /education` | LIST + CREATE + EDIT + DELETE | delete confirmation; date validation (End≥Start when both) | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-008/009 Certificates | `/nurse/profile/certificates` | Manage certificates | `GET/POST/PUT/DELETE /certificates` | LIST + CREATE + EDIT + DELETE | delete confirmation; URL validation (absolute http/https); expiry validation | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-010 Skills | `/nurse/profile/skills` | Manage free-text skill tags | `GET/PUT /skills` (full replace) | READ + EDIT (collection editor) | normalized duplicate rejection; ≤50 cap; empty collection | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-011 Languages | `/nurse/profile/languages` | Manage languages + proficiency | `GET/PUT /languages` (full replace) | READ + EDIT (collection editor) | ≤20 cap; distinct LanguageId; proficiency enum (Beginner/Intermediate/Advanced/Fluent/Native) | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-012 CV | `/nurse/profile/cv` | Upload/replace/delete CV document | `GET /cv` (metadata), `POST /cv` (multipart `file`), `DELETE /cv` (204) | READ + UPLOAD + DELETE | upload constraints (.pdf/.doc/.docx, ≤5MB); metadata-only display; 404 when none; delete confirmation | NOT STARTED | BLOCKED → READY_FOR_HUMAN_DECISION |
| NUR-013 Profile completion | — (NOT_ROUTABLE) | Professional profile completeness | NONE — no backend contract exists | n/a | n/a | BLOCKED (BACKEND + CONTRACT_CLARIFICATION + DESIGN) — must remain excluded until a backend contract is separately approved | BLOCKED (deferred) |
| (related) Contact requests | `/nurse/contact-requests` | Review/approve/reject employer contact requests | `GET /contact-requests` (paginated), `POST /{id}/approve`, `POST /{id}/reject` | READ + ACT | pagination; status filter | NOT STARTED (separate slice after profile family) | BLOCKED → READY_FOR_HUMAN_DECISION |

## 4. Empty / first-time semantics (backend-authoritative, verified)

- Sign-up creates User + Nurse role only; no `NurseProfile` row exists.
- `GET /api/v1/me/nurse-profile` returns **404 Problem Details ("Nurse profile was not found.")** when no professional profile exists. Section endpoints behave the same (404) when the base profile row is missing; once the base profile exists, section lists return `200 []` when empty and CV GET returns 404 when no document exists.
- Frontend distinction: 404 on base GET = "no professional profile yet" (first-time empty state + create CTA → NUR-003); 200 with sparse/null fields = "exists but incomplete" (presentation only).
- `PUT /me/nurse-profile` is upsert: the first save creates the professional profile. T-FE-056 must design for the 404 first-time state; the semantics must NOT be changed to 200/null.

## 5. HUMAN DECISION REQUIRED — NUR-001/002 Overview (decision packet)

Repository evidence fixed: read-only overview; Nurse-only; separate canonical section routes; 404 first-time state; data domain = base profile + 6 sections + CV metadata. Recommendations below are defaults, not requirements.

**D1. Overall layout/composition.** Evidence: no nurse visual authority exists; global foundations (cards, kicker, h1) apply. Options: A) stacked summary cards per section under a profile header (recommended default — matches card pattern of onboarding/admin screens, scales down to mobile naturally); B) two-column desktop grid; C) tabbed sections (conflicts with separate canonical section routes). TRADE-OFF: A is simplest + reuses existing card language; B denser but needs new layout decisions; C contradicts route authority. **HUMAN DECISION REQUIRED.**

**D2. Which sections appear.** Options: A) all six sections + CV (recommended default — mirrors backend data domain; nothing forces hiding); B) base-profile fields only + section links; C) subset chosen per product priority. **HUMAN DECISION REQUIRED.**

**D3. Summary depth per section.** Options: A) count-only ("3 experiences") + link (recommended default — cheapest, no new presentational patterns, no privacy risk); B) latest/top item preview per section (needs "primary item" definition per section — a product decision); C) richer multi-item preview (duplicates section pages, hurts overview scannability). **HUMAN DECISION REQUIRED.**

**D4. CV metadata on overview.** Options: A) show CV status card (file name, size, uploaded date + link to CV page) (recommended default — metadata-only DTO makes this trivial and it is recruitment-relevant); B) omit CV from overview until NUR-012 ships (link only); C) omit entirely. **HUMAN DECISION REQUIRED.**

**D5. First-time 404 empty-state presentation.** Options: A) dedicated empty-state composition with headline + explanation + CTA (recommended default); B) render section scaffold with all-empty sections; C) auto-redirect to personal-information form. TRADE-OFF: C changes navigation flow and presumes NUR-003 exists in the first slice (it does not). **HUMAN DECISION REQUIRED.**

**D6. Empty-state CTA text and destination.** Options: A) "Create your professional profile" → `/nurse/profile/personal-information` (recommended default, matches canonical route; requires NUR-003 to exist before or with overview, OR CTA hidden until it does); B) CTA → onboarding-style inline creation on the overview itself (invents new UX); C) no CTA, informational only. Sub-decision if D6-A: show CTA before NUR-003 is implemented? **HUMAN DECISION REQUIRED.**

**D7. Section navigation CTAs before section pages exist.** First slice = overview only; section routes mount later. Options: A) render section entries as non-navigating summaries in slice 1, add links as each section page ships (recommended default — avoids dead links/404s); B) link all sections immediately (dead routes in slice 1); C) ship overview only after all sections exist (delays first user value). **HUMAN DECISION REQUIRED.**

**D8. Factual "sections completed" summary.** No percentage/score (NUR-013 has no backend contract — must not be invented). Options: A) no completion summary in first slice (recommended default); B) factual per-section filled/empty indication (count already covers this implicitly in D3-A); C) anything weighted/scored — NOT AVAILABLE without new backend contract. **HUMAN DECISION REQUIRED.**

**D9. `/nurse` entry behavior.** Options: A) redirect `/nurse` → `/nurse/profile` when overview ships (recommended default — registry says NURSE_ENTRY is family entry, not a dashboard; redirect behavior is owned downstream per page-registry, so this decision must be explicit here); B) leave `/nurse` unmounted until a Nurse home exists; C) mount a stub page. **HUMAN DECISION REQUIRED.**

**D10. Desktop/tablet/mobile composition.** Default recommendation: single column stacked cards at mobile (390) and tablet (768); optional two-column arrangement desktop (1440) if D1-B chosen; canonical gutters 16/24/32; all layouts RTL-ready via logical properties. Specific visual composition beyond tokens requires either approval of this default or a Penpot/design artifact. **HUMAN DECISION REQUIRED.**

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

No screen is approved by this packet. On human answers to D1–D10 (+ family-level D2 scope for later screens), the technical lead's decisions should be recorded in the frontend ledger (T-FE-052/GATE-FE-T052 rows and the NUR matrix) — only then does GATE-FE-T052 become VERIFIED and T-FE-056 become eligible.
