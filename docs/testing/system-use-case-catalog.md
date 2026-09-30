# System Use-Case Catalog (Canonical — Implementation Truth)

> Step 1 of the real use-case-driven test model. Baseline: branch
> `feat/2026-09-27-stitch-ui-refactor` @ `cbfe1eb89b15e7d1ab86c1cbc553df9d32077838`.
> Authority order: backend behavior → API/OpenAPI → frontend behavior →
> auth/routing policy → persistence model → verified product contracts (context) →
> automated tests (supporting only). Legacy Playwright material, Stitch sample
> data, and stale docs are NOT authority. `IMPLEMENTED_END_TO_END` means the
> frontend-through-backend surface exists; it does NOT mean runtime E2E
> verification (no automated E2E suite exists yet).

## 1. Actor model (implementation-derived)

| Actor | Human | Authenticates | Role/permission basis | Frontend | Backend | Real test persona later |
|---|---|---|---|---|---|---|
| Anonymous / Public Visitor | yes | no | `AllowAnonymous` routes; PUBLIC routes | yes | yes | yes (no credentials) |
| Authenticated user (capability scope, any JWT) | yes | yes | `RequireAuthorization`; exams/commerce-reads/account/countries/languages | yes | yes | no — covered by role identities; record as scope, not persona |
| Nurse | yes | yes | Seeded `Nurse` role + `NurseRoleGuard` (active + DB role check, claims NOT trusted) | yes (`/nurse/**`, `/checkout`, `/commerce/orders*`) | yes | yes |
| Employer | yes | yes | Seeded `Employer` role + `EmployerRoleGuard` | NO — zero mounted routes, no `features/employer/**` | yes | yes (API-driven until UI exists) |
| Administrator | yes | yes | `Admin` role + `RequirePermission(...)`; `ROLE Admin` routes | partial (`/admin`, users, reference-data, exams) | yes | yes |
| System / non-human | NO | no | Seeder, bootstrap, expiry/finalization, token/email issuance, sandbox provider | n/a | yes | NO credentials ever |
| Expert | yes | yes | Seeded role, **zero permissions, zero guards, zero workflows** | no | no (role row only) | NO — not a product actor |
| SuperAdmin | yes | yes | Seed-only all-permission role; excluded from `UpdateUserRoles` (`BusinessRoleNames` = Admin/Employer/Expert/Nurse) | no distinct surface | seed only | NO — Admin seed variant only |

Unverified / inactive / incomplete are **user states, not actors**.

## 2. Status / testability legend

`IMPLEMENTED_END_TO_END` = reachable frontend entry + working API/backend.
`BACKEND_ONLY` = API exists, no reachable production frontend surface
(generated API clients do NOT count as frontend). `PARTIAL` = working subset
with an important deferred slice. `DEFERRED` = explicitly deferred boundary.
Testability: `REAL_UI_JOURNEY` (UI-only from clean seed), `MIXED_SETUP_UI_JOURNEY`
(UI journey + API setup), `API_ONLY`, `EXTERNAL_BLOCKED`, `NOT_CURRENTLY_TESTABLE`.

## 3. Authentication & session (Anonymous / Authenticated / Admin / System)

Admin user creation via API is catalogued once as UC-ADM-02 (no create UI exists).

### UC-AUTH-01 — Login — Anonymous — IMPLEMENTED_END_TO_END

- Purpose: Authenticate with email + password and establish a session.
- Entry: UI `/auth/sign-in` (PUBLIC) → submit; API `POST /api/v1/auth/login`
  (`AllowAnonymous`). Auth: none.
- Preconditions: registered active verified user (unverified → coded 403 path).
- Inputs: `email` (string, req, backend revalidated, sensitive: no);
  `password` (string, req, backend revalidated, SENSITIVE).
- Actions: submit; follow forgot/sign-up links; safe `?returnUrl` redirect.
- Results: UI → `returnUrl||/account`, or verify-required message on 403
  `email_verification_required`; API → `200 AuthResult{accessToken,refreshToken,expiresAt}`;
  persisted → `RefreshTokens` row + `Users.LastLoginAt`.
- Failures: 400 validation; 401 indistinguishable (missing/inactive/wrong password);
  403 unverified-correct; ProblemDetails summary + field errors.
- UPSTREAM_DEPENDENCY: YES — System/Anonymous sign-up + verification
  (UC-AUTH-03/04) or Admin create (UC-ADM-02) must have produced the user.
- Traceability: FE `features/auth/sign-in/sign-in.ts` → `core/api/auth-transport.ts`;
  BE `ApplicationBuilderExtensions.cs:154` → `Identity/Commands/Login/LoginCommandHandler`;
  entities `Users, RefreshTokens`. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-AUTH-02 — Refresh token rotation — Authenticated infrastructure — BACKEND_ONLY

- Purpose: Rotate the refresh token without re-login (transport-level).
- Entry: no screen; automatic via interceptor/`refresh-coordinator`;
  API `POST /api/v1/auth/refresh` (`AllowAnonymous` with token body).
- Preconditions: unexpired unused refresh token.
- Inputs: `refreshToken` (opaque, req, SENSITIVE).
- Actions: automatic rotate.
- Results: API → new pair; persisted → old `RevokedAt`, new row.
- Failures: generic 401 (unknown/revoked-reuse/expired/inactive); reuse revokes ALL user tokens.
- UPSTREAM_DEPENDENCY: YES — UC-AUTH-01/03 session. Traceability:
  `core/auth/refresh-coordinator.ts` → `RotateRefreshTokenCommandHandler`;
  entity `RefreshTokens`. TESTABLE_LATER_AS: API_ONLY.

### UC-AUTH-03 — Public sign-up (Nurse only) — Anonymous — IMPLEMENTED_END_TO_END

- Purpose: Self-register a Nurse account.
- Entry: UI `/auth/sign-up` (PUBLIC; `auth/role-selection`, `auth/register/nurse|employer`
  redirect here); API `POST /api/v1/auth/sign-up` (`AllowAnonymous`, role forced `Nurse`).
- Preconditions: `Nurse` role seeded.
- Inputs: `email` (req, ≤256, email format); `username` (req, ≤50);
  `password` (req, 8+, upper+digit, SENSITIVE); `confirmPassword` client-only must-match.
  Backend revalidates all; no role input exists.
- Actions: submit.
- Results: UI → `/auth/verify-email` notice; API → `202 empty` (duplicate email/username
  also 202, anti-enumeration); persisted → `Users(IsActive,!EmailVerified,empty names)` +
  `UserRole(Nurse)` + `NurseProfile` + 24h `EmailVerificationToken` (+ email attempt).
- Failures: 400 validation; retired `POST /auth/register/nurse|employer` → `410 Gone`.
- UPSTREAM_DEPENDENCY: YES — System seed (`Nurse` role). Traceability:
  `features/auth/sign-up/sign-up.ts` → `core/api/sign-up-api.ts` →
  `PublicRegisterCommandHandler`; entities `Users,UserRoles,NurseProfiles,EmailVerificationTokens`.
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-AUTH-04 — Verify email — Anonymous — IMPLEMENTED_END_TO_END

- Purpose: Activate email verification via token link.
- Entry: UI `/auth/verify-email` (static notice) + `/auth/verify-email/confirm?token=`
  (auto-call); API `POST /api/v1/auth/verify-email` (`AllowAnonymous`).
- Preconditions: unconsumed unexpired token (from UC-AUTH-03/05).
- Inputs: `token` (opaque query/body, req, SENSITIVE). Backend revalidates + expiry/consume checks.
- Actions: automatic verify on page init.
- Results: UI → missing/loading/success/failure; API → 200;
  persisted → `Users.EmailVerified=true`, token `UsedAt`.
- Failures: 409 (missing/used/expired/inactive user).
- UPSTREAM_DEPENDENCY: YES — sign-up/resend token issuance. Traceability:
  `features/auth/verify-email/verify-email.ts` → `VerifyEmailCommandHandler`.
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-AUTH-05 — Resend verification email — Authenticated — BACKEND_ONLY

- Purpose: Issue a fresh verification token. No production UI trigger exists
  (specs assert `resend-verification` absent; generated client only).
- Entry: API `POST /api/v1/auth/send-verification-email` (`RequireAuthorization`).
- Preconditions: authenticated unverified user.
- Inputs: none (current user). Actions: request.
- Results: API → 200; already-verified → success without email; else prior unused
  tokens invalidated + new 24h token + send (send failure → 409).
- Failures: 401; 409 send failure.
- UPSTREAM_DEPENDENCY: NONE (self-service on own account).
  Traceability: `SendVerificationEmailCommandHandler`; entity `EmailVerificationTokens`.
  TESTABLE_LATER_AS: API_ONLY.

### UC-AUTH-06 — Forgot password — Anonymous — IMPLEMENTED_END_TO_END

- Purpose: Request a password-reset token (enumeration-safe).
- Entry: UI `/auth/forgot-password`; API `POST /api/v1/auth/forgot-password` (`AllowAnonymous`).
- Preconditions: none. Inputs: `email` (req, email format, backend revalidated).
- Actions: submit. Results: UI → generic success message always; API → 200 always;
  persisted → 1h `PasswordResetToken` for active users only (failures swallowed/logged).
- Failures: 400 validation only. UPSTREAM_DEPENDENCY: NONE.
  Traceability: `features/auth/forgot-password/` → `ForgotPasswordCommandHandler`.
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-AUTH-07 — Reset password — Anonymous — IMPLEMENTED_END_TO_END

- Purpose: Set a new password via reset token.
- Entry: UI `/auth/reset-password?token=` (blocked when token empty);
  API `POST /api/v1/auth/reset-password` (`AllowAnonymous`).
- Preconditions: valid token from UC-AUTH-06.
- Inputs: `email` + `token` (req, SENSITIVE) + `newPassword` (req, 8+ upper+digit, SENSITIVE).
  Backend revalidates.
- Actions: submit. Results: UI → success/error; API → 200;
  persisted → `Users.PasswordHash` replaced, token consumed, ALL refresh tokens revoked.
- Failures: 409 (mismatch/used/expired/inactive); 400 validation.
- UPSTREAM_DEPENDENCY: YES — UC-AUTH-06 token. Traceability:
  `features/auth/reset-password/` → `ResetPasswordCommandHandler`.
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-AUTH-08 — Current user profile view/update + onboarding — Authenticated — IMPLEMENTED_END_TO_END

- Purpose: View own identity and complete/maintain first/last name (incl. forced onboarding).
- Entry: UI `/account` (inline edit) + `/onboarding/profile` (forced when `!isProfileComplete`
  via `profileCompletionGuard`; complete users bounced away); API `GET /api/v1/me`,
  `PUT /api/v1/me/profile` (`RequireAuthorization`).
- Preconditions: authenticated. Inputs: `firstName,lastName` (req, ≤100, editable,
  backend revalidated); username/email/verified/roles read-only.
- Actions: view, edit, save, cancel.
- Results: UI → inline saved announce / onboarding → `returnUrl||/account`;
  API → 200; persisted → `Users.FirstName/LastName`.
- Failures: 400 validation; 401; safe-`returnUrl` enforcement.
- UPSTREAM_DEPENDENCY: NONE. Traceability: `features/account/account.ts` +
  `personal-details-form.ts` + `features/onboarding/profile/` → `core/api/profile-api.ts` →
  `GetCurrentUser`/`UpdateCurrentUserProfile`; entity `Users`.
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-AUTH-10 — Session lifecycle (bootstrap/bearer/logout/expired/denied) — Authenticated — IMPLEMENTED_END_TO_END

- Purpose: Maintain, attach, and terminate sessions; route anonymous/denied users safely.
- Entry: app bootstrap (`AuthSessionBootstrap`, `CurrentUserStore.hydrate`),
  `bearerInterceptor` (JWT on `/api/*` except anonymous allowlist),
  shell sign-out (`LocalLogout`), `/session-expired`, `/access-denied` (guard target).
- Preconditions: none/token. Inputs: none (system-managed tokens).
- Actions: bootstrap, auto-attach, logout, guard redirects with safe `returnUrl`.
- Results: UI → anonymous sent to `/auth/sign-in?returnUrl=`, denied to `/access-denied`;
  API → `WWW-Authenticate: Bearer` 401s; persisted → token rows unchanged (logout is local).
- Failures: expired → session-expired notice; permission-deny → access-denied.
- UPSTREAM_DEPENDENCY: NONE. Traceability: `core/auth/*`, `core/routing/*`.
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

## 4. Nurse profile (Nurse)

All API under `/api/v1/me/nurse-profile*` (`RequireAuthorization` + `NurseRoleGuard`);
all UI under `/nurse/profile*` (`ROLE Nurse` triple guard). Common failures:
401, 403 non-Nurse, 400 validation, 404 non-owned/gone.

### UC-NUR-01 — Personal information get/upsert — IMPLEMENTED_END_TO_END

- Purpose: Maintain the nurse's headline, summary, license, location, experience years, availability.
- Entry: UI `/nurse/profile/personal-information`; API `GET /`, `PUT /` (upsert, creates if missing).
- Preconditions: Nurse role (profile auto-created at sign-up).
- Inputs: `headline≤160, professionalSummary≤2000, licenseNumber≤100` (optional);
  `licenseCountryId?,currentCountryId?` (lookup `GET /countries`);
  `yearsOfExperience` req 0–80; `isAvailableForRecruitment` bool. Editable; backend revalidates.
- Actions: view, save (create-vs-edit title). Results: UI → `/nurse/profile`;
  API → `NurseProfileDto`; persisted → `NurseProfiles` row.
- UPSTREAM_DEPENDENCY: NONE (own data). Traceability:
  `features/nurse/profile/personal-information/` → `core/api/nurse-profile-api.ts` →
  `UpsertNurseProfile`. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-NUR-02 — Experiences CRUD — IMPLEMENTED_END_TO_END

- Purpose: Maintain employment history.
- Entry: UI `/nurse/profile/experience` (list + create/edit views);
  API `GET|POST /experiences`, `PUT|DELETE /experiences/{id}` (204 on delete).
- Preconditions: own profile. Inputs: `facilityName, jobTitle` req; `countryId?` (lookup);
  `startDate` req DateOnly; `endDate?`; `isCurrent` bool; `description?`.
  Constraint: EndDate≥Start unless current. Backend revalidates. Owner-scoped.
- Actions: list, create, edit, delete (`TwoStepConfirmation`), expand.
- Results: UI → reload + saved/deleted announce, 404 gone-notice;
  API → DTOs; persisted → `NurseExperiences` rows.
- UPSTREAM_DEPENDENCY: NONE. Traceability: `nurse-experience*.ts` →
  `Create|Update|DeleteNurseExperience`. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-NUR-03 — Education CRUD — IMPLEMENTED_END_TO_END

- Purpose: Maintain education history. Same shape as UC-NUR-02.
- Entry: UI `/nurse/profile/education`; API `/education*`.
- Inputs: `institutionName, degree` req; `fieldOfStudy?,countryId?,startDate?,endDate?,description?`.
- Actions/results/failures: as UC-NUR-02; entity `NurseEducation`.
- UPSTREAM_DEPENDENCY: NONE. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-NUR-04 — Certificates CRUD — IMPLEMENTED_END_TO_END

- Purpose: Maintain professional certificates. Same shape as UC-NUR-02.
- Entry: UI `/nurse/profile/certificates`; API `/certificates*`.
- Inputs: `name, issuingOrganization` req; `issueDate?`; `expirationDate?` (≥issue);
  `credentialId?,credentialUrl?`.
- Actions/results: as UC-NUR-02; entity `NurseCertificates`.
- UPSTREAM_DEPENDENCY: NONE. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-NUR-05 — Languages replace-all — IMPLEMENTED_END_TO_END

- Purpose: Replace the full language set.
- Entry: UI `/nurse/profile/languages` (dynamic rows, max 20);
  API `GET /languages`, `PUT /languages` (full replace).
- Inputs: `languages[]:{languageId` req (lookup `GET /languages`), `proficiency` req ∈
  {Beginner,Intermediate,Advanced,Fluent,Native}}. Backend revalidates + dedupes.
- Actions: add/remove rows, save, cancel. Results: UI → saved announce, 409 → catalog
  refresh message, 404 profileGone; API → list DTOs; persisted → `NurseLanguages` replaced.
- UPSTREAM_DEPENDENCY: NONE. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-NUR-06 — Skills replace-all — IMPLEMENTED_END_TO_END

- Purpose: Replace the full skill set (free-text chips).
- Entry: UI `/nurse/profile/skills`; API `GET /skills`, `PUT /skills` (full replace).
- Inputs: `skills:string[]` free text, trim/dedupe, ≤100 chars, ≤50 items. Backend revalidates.
- Actions: add/remove chips, save, cancel. Results: persisted `NurseSkills` replaced.
- UPSTREAM_DEPENDENCY: NONE. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-NUR-07 — CV get/upload/delete — Nurse — PARTIAL

- Purpose: Store one CV document (upload/replace/delete work; preview/download absent).
- Entry: UI `/nurse/profile/cv` (file picker); API `GET /cv`, `POST /cv` (multipart),
  `DELETE /cv` (204, replace-on-upload).
- Inputs: `file` binary req; accept `.pdf/.doc/.docx`, ≤5MB, single;
  metadata server-read. Backend revalidates (`UploadNurseCvCommandValidator`).
- Actions: pick, upload, delete (confirm). Results: UI → metadata + announce, 404 → empty;
  API → `NurseCvDocumentDto`; persisted → `NurseCvDocuments` + file storage.
- Failures: client+400 on empty/oversize/bad-ext; 404; 401/403.
- UPSTREAM_DEPENDENCY: NONE. Traceability: `nurse-cv.ts` → `Upload|DeleteNurseCv` +
  `IFileStorageService(LocalFileStorageService)`. TESTABLE_LATER_AS: REAL_UI_JOURNEY
  (assert preview/download absent to pin PARTIAL).

### UC-NUR-08 — Profile overview dashboard — IMPLEMENTED_END_TO_END

- Purpose: Read-only rollup of all profile sections with navigation to subpages.
- Entry: UI `/nurse/profile` (parallel per-section calls, per-section swallow,
  404 profile → empty ready). No dedicated API (composes UC-NUR-01..07 reads).
- Preconditions: Nurse role. Inputs: none. Actions: view, navigate.
- Results: UI → cards, latest experience/education, certificate previews + counts,
  availability label. No direct persistence.
- UPSTREAM_DEPENDENCY: NONE. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

## 5. Employer & recruitment (Employer / Nurse)

Employer side has NO frontend routes (verified: no `features/employer/**`, no mounted
`/employer*` paths) — all Employer UCs are BACKEND_ONLY via direct API.

### UC-EMP-01 — My employer profile get/upsert — Employer — BACKEND_ONLY

- Purpose: Maintain job title/department. Entry: API `GET|PUT /api/v1/me/employer-profile/`
  (`RequireAuthorization` + `EmployerRoleGuard`). Preconditions: Employer role.
- Inputs: `jobTitle?,department?` (optional, backend revalidated). Actions: view, save.
- Results: API → `EmployerProfileDto`; persisted → `EmployerProfiles` (1:1).
- Failures: 401/403/400. UPSTREAM_DEPENDENCY: YES — Admin-created Employer user
  (UC-ADM-02; public sign-up is Nurse-only). TESTABLE_LATER_AS: API_ONLY.

### UC-EMP-02 — My organization get/upsert — Employer — BACKEND_ONLY

- Purpose: Maintain the employing organization. Entry: API `GET|PUT /.../organization`.
- Preconditions: UC-EMP-01 profile. Inputs: `name` req + `type?,websiteUrl?(url),
  countryId?,city?,addressLine1/2?,postalCode?,description?`. Backend revalidates.
- Actions/results: as UC-EMP-01; entity `EmployerOrganizations`.
- UPSTREAM_DEPENDENCY: YES — UC-EMP-01. TESTABLE_LATER_AS: API_ONLY.

### UC-REC-01 — Browse/search candidates — Employer — BACKEND_ONLY

- Purpose: Discover available nurses via safe projection (no PII beyond approved fields).
- Entry: API `GET /api/v1/recruitment/candidates` (`RequireAuthorization`).
- Preconditions: only `IsAvailableForRecruitment` profiles are visible.
- Inputs (all optional + pagination): `licenseCountryId?,currentCountryId?` (lookups),
  `minimumYearsOfExperience?` int, `skills?` csv, `languageId?`. Backend revalidates.
- Actions: search, filter, paginate. Results: API → `PaginatedResult<CandidateListItemDto>`;
  no persistence.
- Failures: 401/400; empty vs filtered no-results.
- UPSTREAM_DEPENDENCY: YES — Nurse UC-NUR-01/05/06 availability + profile data.
  Traceability: `Recruitment/Queries/ListCandidates`. TESTABLE_LATER_AS: API_ONLY.

### UC-REC-02 — Create contact request — Employer — BACKEND_ONLY

- Purpose: Request contact with a nurse (`Pending`).
- Entry: API `POST /api/v1/recruitment/contact-requests` (201+Location).
- Preconditions: target exists + available; no duplicate pending.
- Inputs: `nurseProfileId` guid req (existing entity). Backend checks all guards.
- Actions: create. Results: API → 201 `ContactRequestDto(Pending)`;
  persisted → `ContactRequests` row.
- Failures: 401/404/409 duplicate-or-unavailable/400.
- UPSTREAM_DEPENDENCY: YES — UC-REC-01 discovery state. TESTABLE_LATER_AS: API_ONLY.

### UC-REC-03 — List/get/cancel my sent requests — Employer — BACKEND_ONLY

- Purpose: Track and cancel own requests. Entry: API `GET /...` (status filter),
  `GET /.../{id}`, `POST /.../{id}/cancel` (owner only, `Pending→Cancelled`).
- Preconditions: UC-REC-02 request. Inputs: `{id}` path; `status?` filter.
- Actions: list, view, cancel. Results: API → DTOs; persisted → `CancelledAt/RespondedAt`.
- Failures: non-owned → 404; illegal transition → 409.
- UPSTREAM_DEPENDENCY: YES — UC-REC-02. TESTABLE_LATER_AS: API_ONLY.

### UC-REC-04 — List received / approve / reject — Nurse — IMPLEMENTED_END_TO_END

- Purpose: Decide on incoming employer requests.
- Entry: UI `/nurse/contact-requests` (filter All/Pending/Approved/Rejected/Cancelled,
  20/pg, per-card Approve/Reject); API `GET /api/v1/me/nurse-profile/contact-requests`,
  `POST .../{id}/approve|reject` (atomic `Pending→Approved/Rejected`).
- Preconditions: pending request from UC-REC-02.
- Inputs: `{id}` path; `status?` filter. Actions: filter, paginate, approve, reject.
- Results: UI → terminal card + notice, 404/409 gone/decided notice + reload,
  empty-page fallback; API → `ReceivedContactRequestDto`; persisted → status + `RespondedAt`.
- Failures: 404 non-owned; 409 already-decided.
- UPSTREAM_DEPENDENCY: YES — Employer UC-REC-02 produces `Pending`.
  Traceability: `nurse-contact-requests.ts` → `ContactRequestsApi` →
  `Approve|RejectReceivedContactRequest`. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

## 6. Exams learner (Authenticated / Nurse)

Catalog chain is `AUTHENTICATED_ONLY` (any JWT); session writes owner-scoped;
paid starts gated by `ExamAccessPolicy` (free → `Free` source; paid → unexpired
`ExamAccessGrant` else coded 403). State machine: `InProgress→Submitted|Expired`
(atomic finalization via `ExamScoringService`).

### UC-EXM-01 — List exams / exam detail — IMPLEMENTED_END_TO_END

- Purpose: Browse published exams. Entry: UI `/exams` (country/category filters,
  pagination) + `/exams/:examId`; API `GET /api/v1/exams`, `GET /api/v1/exams/{id}`.
- Preconditions: published exam + published version (Admin chain).
- Inputs: `countryId?,categoryId?` lookups + pagination. Actions: filter, paginate, view.
- Results: UI → cards, `canStart` vs `requiresPurchase(!isFree&&!canStart)`;
  API → catalog/detail DTOs; no persistence.
- Failures: 401; 404 unpublished/missing.
- UPSTREAM_DEPENDENCY: YES — Administrator UC-ADM-04/05 (publish).
  Traceability: `exams-list.ts`+`exam-detail.ts` → `core/api/exams-api.ts` →
  `ListExams/GetExam`. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-EXM-02 — Start exam session — IMPLEMENTED_END_TO_END

- Purpose: Begin an `InProgress` attempt (free or grant/package-authorized).
- Entry: UI `/exams/:examId/instructions` (`TwoStepConfirmation` + resume probe);
  API `POST /api/v1/exams/{id}/sessions`.
- Preconditions: published version; paid → grant (UC-COM-06) or package attempt right
  (UC-PP-03); one in-progress session per nurse per version (DA10).
- Inputs: `{examId}` path. Actions: start, resume-if-exists.
- Results: UI → session route or 409 reconcile → reload; API → `ExamSessionDto` with items;
  persisted → `ExamSessions(InProgress)` + snapshot `Questions/AnswerOptions/Provenance{Source}`.
- Failures: 403 paid-without-grant; 409 different-source conflict; 404.
- UPSTREAM_DEPENDENCY: YES — Admin publish; grant/entitlement for paid/package.
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-EXM-03 — Session answer autosave — IMPLEMENTED_END_TO_END

- Purpose: Answer with pager + autosave (no scoring until submit).
- Entry: UI `/exams/:examId/sessions/:sessionId` (select/save/prev/next/goto, 1s countdown,
  5-min warning, unsaved badge, unanswered count);
  API `GET /api/v1/exam-sessions/{id}`, `PUT .../answers`.
- Preconditions: own `InProgress` session. Inputs: `answers[]:{questionId,optionId}` req
  (existing entities). Actions: select, save, navigate, (no flag/unflag or clear-selection
  in current HEAD — verified absent).
- Results: UI → local-vs-persisted state; API → session DTO; persisted → `ExamSessionAnswers`.
- Failures: 401/404 non-owned; 409 terminal; expiry → auto-reconcile.
- UPSTREAM_DEPENDENCY: YES — UC-EXM-02 (same-actor prior session).
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-EXM-04 — Submit / result / review — IMPLEMENTED_END_TO_END

- Purpose: Finalize scoring and review correctness.
- Entry: UI same session screen (confirm submit) + `.../result` + `.../review`;
  API `POST .../submit`, `GET .../result`, `GET .../review` (review adds
  `isCorrect/pointsEarned/explanation/correctOptionId`, post-finalize only).
- Preconditions: own `InProgress` session. Inputs: none.
- Actions: submit (confirm), view result, page review.
- Results: UI → score/passed, `Submitted/Expired` label; API → result/review DTOs;
  persisted → `Submitted` + score fields (+`SubmittedAt`).
- Failures: 404 unavailable; 409 notFinalized/premature.
- UPSTREAM_DEPENDENCY: YES — UC-EXM-02/03. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-EXM-05 — My exam attempts history — IMPLEMENTED_END_TO_END

- Purpose: List own attempts with resume/result/review links.
- Entry: UI `/exams/history` (status filter + page in queryParams);
  API `GET /api/v1/me/nurse-profile/exam-attempts`.
- Inputs: `status?` ∈ {InProgress,Submitted,Expired}; pagination.
- Results: UI → rows (`Abandoned` label); API → paginated attempts; no persistence.
- UPSTREAM_DEPENDENCY: YES — prior sessions (same actor).
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-EXM-06 — Exam analytics — IMPLEMENTED_END_TO_END

- Purpose: Aggregate performance (summary/by-exam/by-category/trends).
- Entry: UI `/exams/analytics` (draft from/to/country/category, `from>to` client error,
  per-section retry, zero-attempt short-circuit); API `.../exam-analytics/summary|by-exam|
  by-category|trends` (`bucket?=Month` ∈ {Day,Week,Month}).
- Inputs: `from?,to?,countryId?,categoryId?,examId?` + pagination. Backend validates.
- Results: UI → section loading/ready/error; API → aggregate DTOs; read-only (no persistence).
- UPSTREAM_DEPENDENCY: YES — finalized sessions (same actor).
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

## 7. Preparation packages (Anonymous / Nurse / Admin-setup)

Public catalog anonymous; nurse area `RequireAuthorization` +
`PackageBenefitAuthorizationService` (Active entitlement + window + right Available);
admin `RequirePermission(...)`. Rights: `MaterialsAccess/PracticeAccess/
PackageExamAttemptEligibility/ReportEligibility` (Available/Dormant/Consumed/Expired).

### UC-PP-01 — Public offer catalog list/detail — Anonymous — IMPLEMENTED_END_TO_END

- Purpose: Browse purchasable package offers. Entry: UI `/preparation-packages`
  (20/pg, no filters) + `/:offerSlug`; API `GET /api/v1/preparation-packages/offers[/{slug}]`
  (`AllowAnonymous`, active eligible only).
- Preconditions: active eligible offer (full Admin chain UC-ADM-08).
- Inputs: `countryId?,examCategoryId?` + pagination (API); slug path.
- Results: UI → cards/detail, 404 notFound; API → catalog/detail DTOs (price/currency/
  duration/counts/components); no persistence.
- UPSTREAM_DEPENDENCY: YES — Administrator UC-ADM-08. Traceability:
  `offers-list.ts`+`offer-detail.ts` → `preparation-package-offers-api.ts` →
  `PreparationPackageEndpointExtensions.cs` catalog ops. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-PP-02 — My package entitlements list/detail — Nurse — IMPLEMENTED_END_TO_END

- Purpose: View owned package access + benefit rights + purchase snapshot.
- Entry: UI `/nurse/preparation-packages` + `/:entitlementId`;
  API `GET /api/v1/me/nurse-profile/preparation-packages/entitlements[/{id}]`.
- Preconditions: `Active` entitlement from package purchase flow.
- Inputs: pagination; `{id}` path. Actions: view, follow practice gate.
- Results: UI → status (Active/Expired/Locked), rights, snapshot; API → entitlement DTOs;
  no new persistence (reads).
- Failures: 404 non-owned. UPSTREAM_DEPENDENCY: YES — UC-COM-02(package)+UC-COM-06
  (fulfillment). TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-PP-03 — Package exam session start/state — Nurse — IMPLEMENTED_END_TO_END

- Purpose: Consume one package attempt and take the included mock exam.
- Entry: UI entitlement-detail `package-exam-section`;
  API `POST .../entitlements/{eid}/exam-session` (atomic consume) +
  `GET .../{eid}/exam-session` (state probe, no consume).
- Preconditions: attempt-eligibility right Available + access window.
- Inputs: `{entitlementId}` path. Actions: start, probe state.
- Results: API → `PackageExamSessionStartDto` / state DTO;
  persisted → right `Available→Consumed`, session `Source=Package` + provenance.
- Failures: 404; coded 409 ineligible/consumed/expired.
- UPSTREAM_DEPENDENCY: YES — UC-PP-02 entitlement. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-PP-04 — Practice bank progress/answer — Nurse — IMPLEMENTED_END_TO_END

- Purpose: Practice with immediate feedback (never exposes keys/rationales at rest).
- Entry: UI `/:entitlementId/practice` wizard (select/submit/prev/next/goto,
  resume-first-unanswered); API `GET .../practice-progress/items` (learner-safe),
  `GET .../practice-progress`, `POST .../items/{pid}/answer`.
- Preconditions: practice-access right + window.
- Inputs: `selectedPracticeAnswerOptionId` guid req. Backend revalidates.
- Actions: select, submit, navigate. Results: UI → immediate feedback + progress;
  API → content/summary/submission DTOs; persisted → `PackagePracticeProgress`
  (Correct/Incorrect overwrite).
- Failures: 404; 409 accessEnded. UPSTREAM_DEPENDENCY: YES — UC-PP-02.
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-PP-05 — Package analytical report — Nurse — IMPLEMENTED_END_TO_END

- Purpose: View immutable diagnostic report (topic scores + guidance restricted to
  included versions; no question text/keys/rationales).
- Entry: UI `/nurse/preparation-packages/reports/:sessionId`;
  API `GET .../exam-sessions/{sid}/report` (lazily generated once).
- Preconditions: finalized package session + report-eligibility right.
- Inputs: `{sessionId}` path. Actions: view.
- Results: API → report DTO (topics + guidance items); persisted → immutable
  `PackageAnalyticalReports(+TopicResults+GuidanceItems)`; report failure does NOT
  consume the right nor require retake (DA9); report persists after expiry.
- Failures: 404 unavailable; 409 notFinalized.
- UPSTREAM_DEPENDENCY: YES — UC-PP-03 session + UC-EXM-04 finalization.
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

## 8. Commerce (Authenticated / Nurse)

Sandbox-only fulfillment (`SandboxPaymentCheckoutProvider`); production provider
deferred. Order: `PendingPayment→Paid|Failed|Cancelled|Expired`.

### UC-COM-01 — Payment product list/detail — Authenticated — IMPLEMENTED_END_TO_END

- Purpose: Browse purchasable exam-access products. Entry: UI `/commerce/products` +
  `/:productId`; API `GET /api/v1/payment/products[/{id}]` (`RequireAuthorization`,
  active + published-exam only).
- Preconditions: active product (UC-ADM-07). Inputs: pagination; `examId?` filter.
- Results: UI → money-formatted cards; API → product DTOs; no persistence.
- UPSTREAM_DEPENDENCY: YES — Administrator UC-ADM-07. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-COM-02 — Create order — Nurse — IMPLEMENTED_END_TO_END

- Purpose: Create a `PendingPayment` order (order-create ONLY; no card/provider/billing
  UI — asserted absent in specs). Entry: UI `/checkout?productId=` (create/retryCreate/cancel);
  API `POST /api/v1/me/nurse-profile/payment/orders` (201+Location, exactly-one of
  `productId|packageOfferId`).
- Preconditions: active product / active offer. Inputs: `productId? xor packageOfferId?`
  (existing entity); money server-owned; checkout `idempotencyKey?`.
- Actions: create, retry-create, back-out. Results: UI → missingContext/unavailable(404)/
  conflict-or-generic + pending order view; API → order DTO; persisted → `PaymentOrders
  (PendingPayment)` + items (+ package snapshot for package path).
- Failures: 400 (both/neither IDs); 404; 409 inactive/unpublished.
- UPSTREAM_DEPENDENCY: YES — UC-ADM-07 / UC-ADM-08 offer. Traceability:
  `features/commerce/checkout.ts` → `CommercePaymentsApi.createOrder`.
  TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-COM-03 — My orders list/detail — Nurse — IMPLEMENTED_END_TO_END

- Purpose: Track own orders. Entry: UI `/commerce/orders` + `/:orderId`;
  API `GET .../payment/orders`, `GET .../{id}` (non-owned → 404).
- Inputs: pagination; `status?` (API). Results: status labels + totals; read-only.
- UPSTREAM_DEPENDENCY: YES — UC-COM-02 (same actor). TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-COM-04 — Cancel pending order — Nurse — IMPLEMENTED_END_TO_END

- Purpose: Cancel own `PendingPayment` order (focus-trapped confirm, pending-only).
- Entry: UI order-detail confirm dialog; API `POST .../orders/{id}/cancel`.
- Preconditions: owned pending order without active checkout.
- Inputs: `{id}` path. Results: UI → reload + focus restore; API → order DTO;
  persisted → `PendingPayment→Cancelled`.
- Failures: 404; 409 cancelConflict (non-pending/active-checkout).
- UPSTREAM_DEPENDENCY: YES — UC-COM-02. TESTABLE_LATER_AS: MIXED_SETUP_UI_JOURNEY.

### UC-COM-05 — Start provider checkout session — Nurse — BACKEND_ONLY

- Purpose: Initialize the provider payment session (no frontend pay button exists).
- Entry: API `POST .../orders/{orderId}/checkout` (`Cache-Control:no-store`).
- Preconditions: owned pending order. Inputs: `{idempotencyKey?}`.
- Results: API → checkout-session DTO; persisted → `PaymentCheckoutSessions(Created→
  ProviderPending)` with lease handling.
- Failures: 400/404; 409 retryable + `Retry-After` on concurrency; 503 provider down.
- UPSTREAM_DEPENDENCY: YES — UC-COM-02. TESTABLE_LATER_AS: API_ONLY.

### UC-COM-06 — Sandbox checkout completion (dev/test only) — Authenticated — BACKEND_ONLY

- Purpose: Complete payment in Development/Test and fulfill (absent in Prod).
- Entry: API `POST /api/v1/dev/sandbox/payment/checkout-sessions/{id}/complete`.
- Preconditions: pending order + checkout session.
- Inputs: `{checkoutSessionId}` path. Actions: complete (idempotent replay safe).
- Results: API → `PaymentCompletionDto{grantedExamIds, package entitlements}`;
  persisted → `PendingPayment→Paid` + `ExamAccessGrant` and/or
  `PackagePurchaseEntitlement+Rights+Snapshot`.
- Failures: 409 expired/illegal state; 503.
- UPSTREAM_DEPENDENCY: YES — UC-COM-02 + UC-COM-05. TESTABLE_LATER_AS: API_ONLY
  (dev-only; never assert in Prod).

### UC-COM-07 — Production provider payment completion — n/a — DEFERRED

- Boundary record: company country/bank/provider selection unfinalized per
  `CURRENT_TASK.md`; no prod provider code paths exist. UPSTREAM_DEPENDENCY: NONE.
  TESTABLE_LATER_AS: EXTERNAL_BLOCKED.

## 9. Administration (Administrator)

### UC-ADM-01 — User list/detail/role administration — IMPLEMENTED_END_TO_END

- Purpose: Find users and replace business role (revokes sessions).
- Entry: UI `/admin` hub + `/admin/users` (search, 10/pg) + `/admin/users/:userId`
  (role select Nurse/Employer/Expert/Admin, no SuperAdmin);
  API `GET /api/v1/users*` (`Users.View`), `PUT /api/v1/admin/users/{userId}/role` (`Users.Edit`).
- Preconditions: admin with perms. Inputs: `search?,isActive?,role?,sort?` + pagination;
  `roleName` req ∈ {Admin,Employer,Expert,Nurse}.
- Actions: search, paginate, view, save role. Results: UI → updated status;
  API → DTOs; persisted → `UserRoles` replaced + ALL refresh tokens revoked (re-login).
- Failures: 401/403 (incl. limited admin); 404; 400.
- UPSTREAM_DEPENDENCY: NONE. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-ADM-02 — Admin create user — Administrator — BACKEND_ONLY

- Purpose: Create a user with explicit roles (incl. Employer/Admin onboarding, since
  public sign-up is Nurse-only). No create UI (only list/detail/role UI).
- Entry: API `POST /api/v1/auth/register` (`RequirePermission(Users.Create)`).
- Preconditions: admin with `Users.Create`; valid role IDs.
- Inputs: `email,username,password,firstName,lastName` (req),
  `roleIds?` (existing roles). Backend revalidates.
- Actions: create. Results: API → 200 `{userId}`;
  persisted → `User(IsActive,!EmailVerified)` + `UserRoles`.
- Failures: 401/403; 409 duplicates/bad roles; 400 validation.
- UPSTREAM_DEPENDENCY: NONE. Traceability: `RegisterUserCommandHandler`;
  entities `Users,UserRoles`. TESTABLE_LATER_AS: API_ONLY.

### UC-ADM-03 — Exam category administration — IMPLEMENTED_END_TO_END

- Purpose: CRUD + archive/restore categories (reference-data surface).
- Entry: UI `/admin/reference-data` (country/isActive filters, per-field validation,
  confirm archive/delete, restore); API `/api/v1/admin/exam-categories*`
  (`Exams.View/Create/Edit/Delete`).
- Inputs: `countryId` req (lookup), `name≤200`, `slug≤160`, `description?≤1000`,
  `displayOrder` int req. Backend revalidates.
- Actions: create/edit/archive/restore/delete + per-button perm gates.
- Results: API → 201+Location/200/204; persisted → `ExamCategories` (+`IsActive` flips).
- Failures: 409 conflict; 401/403/404/400.
- UPSTREAM_DEPENDENCY: NONE (root setup; downstream: UC-ADM-04/08, UC-EXM-01).
  TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-ADM-04 — Exam administration — IMPLEMENTED_END_TO_END

- Purpose: CRUD + archive exams (list/detail UI only; versions/questions API-only).
- Entry: UI `/admin/exams` + `/admin/exams/:examId` (edit blocked if Archived;
  archive if not Archived; delete Draft only); API `/api/v1/admin/exams*`.
- Inputs: `countryId` req, `examCategoryId?`, `title/slug` req, `description?`,
  `instructions?`, `durationMinutes` int req, `passingScorePercentage` req, `isFree=true`.
- Results: persisted `Exams` (`Draft→Published→Archived`; delete Draft only).
- UPSTREAM_DEPENDENCY: YES — UC-ADM-03 category. TESTABLE_LATER_AS: REAL_UI_JOURNEY.

### UC-ADM-05 — Exam version lifecycle — Administrator — BACKEND_ONLY

- Purpose: Draft → validate → publish → retire versions (no UI routes).
- Entry: API `GET .../exams/{examId}/versions[/{versionId}]`,
  `POST .../versions` (draft), `POST .../{v}/validate` (`Questions.View` →
  `AdminExamVersionValidationDto`), `POST .../{v}/publish` (`Questions.Manage`),
  `POST .../{v}/retire` (`Exams.Edit`), `DELETE` draft only.
- Results: persisted `Draft→Published→Retired`; delete draft only.
- Failures: 404/409. UPSTREAM_DEPENDENCY: YES — UC-ADM-04.
  TESTABLE_LATER_AS: API_ONLY.

### UC-ADM-06 — Question / answer-option administration — Administrator — BACKEND_ONLY

- Purpose: Author version content (no UI routes).
- Entry: API `.../versions/{v}/questions[/{qid}]` + `.../questions/{qid}/options[/{oid}]`
  (list/get/create/update/deactivate/delete; `Questions.View/Manage`).
- Inputs: question `questionText` req, `explanation?`, `questionType=SingleBestAnswer`,
  `points=1`, `displayOrder` int, `isActive=true`; option `optionText` req,
  `displayOrder` int, `isCorrect` bool, `isActive=true`. Backend revalidates.
- Results: persisted `ExamQuestions/ExamAnswerOptions` (+`IsActive` flips).
- UPSTREAM_DEPENDENCY: YES — UC-ADM-05 draft version. TESTABLE_LATER_AS: API_ONLY.

### UC-ADM-07 — Payment product administration — Administrator — BACKEND_ONLY

- Purpose: CRUD + archive/restore products (no `/admin/payment-products` route;
  gated by `Exams.*`, no commerce perm — confirm intentional, see contradictions).
- Entry: API `/api/v1/admin/payment/products*`.
- Inputs: `examId` req, `name` req, `description?`, `currency` req, `unitAmountMinor` long req,
  `isActive=true`. Results: persisted `PaymentProducts` (+`IsActive` flips).
- UPSTREAM_DEPENDENCY: YES — UC-ADM-04 published exam. TESTABLE_LATER_AS: API_ONLY.

### UC-ADM-08 — Preparation-package administration — Administrator — BACKEND_ONLY

- Purpose: Full authoring chain (no `/admin/preparation-packages/**` routes).
- Entry (all `/api/v1/admin/preparation-package/`): reporting-topics
  (`ReportingTopics.Manage`: CRUD + archive; `examCategoryId` immutable);
  reporting-profiles (`ReportingProfiles.Manage`: create bound to exact exam version +
  per-question topic assignments + publish); materials (`StudyMaterials.Manage`: identity
  CRUD + version create/update + publish/retire; type ∈ {FormattedText,File,ExternalLink,Video}
  with exactly-one content field + `reportingTopicIds[]`); practice-collections
  (`PracticeCollections.Manage`: collection + version items with correct option + publish/retire);
  package definitions/versions (`PreparationPackages.View/Manage/Publish`: definition CRUD,
  version create `{examVersionId,reportingProfilePublicationId,practiceCollectionVersionId,
  materials[]}`, validation → `PackagePublicationValidationDto`, publish needs ≥1 material +
  isolation + compatible profile, retire); offers (`PreparationPackageOffers.Manage`:
  create `{definitionId,versionId,title,slug,price,currency,accessDurationDays}` + update +
  activate/deactivate; public catalog shows Active only).
- Results: `201+Location` creates; `Draft→Published→Retired` / `Draft→Active→Inactive`
  transitions; 404/409 + validation issues on illegal moves.
- UPSTREAM_DEPENDENCY: YES — UC-ADM-03/04/05/06 chain (category → exam → version+questions).
  Downstream: UC-PP-01, UC-COM-02(package). TESTABLE_LATER_AS: API_ONLY.

## 10. Reference & system

### UC-REF-01 — Countries / languages lookup — Authenticated — IMPLEMENTED_END_TO_END

- Purpose: Back every country/language select. Entry: used inside nurse forms/exam
  filters; API `GET /api/v1/countries|languages` (`RequireAuthorization`).
  Manage permissions exist as unenforced constants only.
- Results: `CountryListItemDto/LanguageListItemDto{id,name,code}` (seeded 10/7); read-only.
- UPSTREAM_DEPENDENCY: YES — System seed. TESTABLE_LATER_AS: REAL_UI_JOURNEY
  (exercised via dependent forms; API-assertable standalone).

### UC-SYS-01 — Seed & bootstrap — System — BACKEND_ONLY — HUMAN_ACTOR: NO

- Purpose: Guarantee roles/permissions/countries/languages + initial Admin at startup
  (`ReferenceDataSeeder`, `BootstrapAdminService`, `DatabaseInitializer` + migrations).
- Results: persisted seed rows. UPSTREAM_DEPENDENCY: NONE.
  TESTABLE_LATER_AS: NOT_CURRENTLY_TESTABLE
  (implicitly proven by every boot/integration run; never assert as a journey).

### UC-SYS-02 — Expiry & finalization — System — BACKEND_ONLY — HUMAN_ACTOR: NO

- Purpose: Lazy expiry (`ExpireIfPastDue`: tokens/orders/sessions/entitlements/rights)
  and atomic finalization/provenance (exam submit, contact-request transition,
  fulfillment). Entry: inside user flows + clock. Results: terminal states + timestamps.
- UPSTREAM_DEPENDENCY: NONE (triggered by clock/domain rules, not business state).
  TESTABLE_LATER_AS: API_ONLY (via windowed fixtures, never hand-edited impossible states).

### UC-SYS-03 — Email token issuance — System — BACKEND_ONLY — HUMAN_ACTOR: NO

- Purpose: Create/send verification + reset tokens (`IEmailService`); send failures are
  swallowed to generic success by design. Assert token row + 202/generic, never inbox content.
- UPSTREAM_DEPENDENCY: NONE (triggered by user auth flows, no business entity required).
  TESTABLE_LATER_AS: API_ONLY.

### UC-SYS-04 — Health / root — Anonymous infrastructure — BACKEND_ONLY

- Purpose: Liveness/readiness (`GET /,/health,/health/live|ready`) + dev-only OpenAPI
  (`GET /openapi/v1.json`; no checked-in snapshot). UPSTREAM_DEPENDENCY: NONE.
  TESTABLE_LATER_AS: API_ONLY.

## 11. Current implementation contradictions (recorded, not fixed)

1. Retired registration: `docs/api/api-design.md` documents active
   `POST /auth/register/nurse|employer` — implementation returns `410 Gone`; live route is
   Nurse-only `POST /api/v1/auth/sign-up`. Impact: tests must use sign-up; doc needs update.
2. Package API: `docs/api/api-design.md` states no preparation-package API exists —
   ~42 endpoints exist. Impact: doc unusable for packages; implementation governs.
3. Employer readiness: `screen-contracts/employer.md` = `CONTRACT_READY` but zero mounted
   employer routes exist. Impact: employer journeys are API-only until UI lands.
4. Checkout terminology: order-create UI (`/checkout`) exists while contracts mark checkout
   DEFERRED and no provider-pay UI exists. Impact: test order-create, not provider payment.
5. Product-admin permission: payment products gated by `Exams.*` (no commerce perm) —
   confirm intentional. Impact: permission tests must use `Exams.*`.
6. Screen inventory drift: older README prose counts vs `screen-index.md` (64 IDs / 101 rows);
   main-API operations lack `WithTags`; no checked-in `openapi.json` (regen drift risk).
7. Expert: seeded role, zero permissions/workflows; exam authoring is Admin-owned
   (`Questions.Manage`, `Exams.*`). Any Expert-authoring future is a PRODUCT CHANGE, not catalog.
