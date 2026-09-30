# Canonical Executable Test Scenario Catalog (Step 5)

> WHAT to automate — not automation itself. Authority, in order: current
> implementation; `system-use-case-catalog.md` (54 UCs); `test-identity-registry.md`
> (6 identities); `test-data-catalog.md` (32 Data IDs); `dependency-graph.md`
> (24 edges, L0–L4). Legacy Playwright material is NOT authority. No scenario starts
> from fabricated workflow-derived state (Paid, grants, entitlements, sessions,
> results, requests, tokens, provenance). Layer: UI = real frontend journey;
> MIXED = UI journey + API setup; API_ONLY = no implemented frontend, API asserts only.

## AUTH family

### SCN-AUTH-SIGNUP-001 — Public nurse sign-up

- **Actor:** Anonymous. **Identity:** per-run ephemeral (SP-NEW-UNVERIFIED derivation).
- **UCs:** UC-AUTH-03. **Data:** none (creates User row). **Edges:** D-00a (seeded Nurse role), D-01. **Level:** L1.
- **Starting state / Preconditions:** clean run; `Nurse` role seeded; email unused.
- **Inputs:** unique `lc-<run>@test.nursing-platform.test`, username `test_lc_<run>`, password meeting min-8/upper/digit.
- **Actions:** open `/auth/sign-up` → fill → submit.
- **Expected UI:** redirect `/auth/verify-email` notice. **API:** `202 empty`.
- **Backend/domain:** User(IsActive, !verified, empty names) + UserRole(Nurse) + NurseProfile + 24h token.
- **Persisted:** `Users, UserRoles, NurseProfiles, EmailVerificationTokens` rows.
- **Downstream-visible:** account cannot log in yet (403 path in SCN-AUTH-LOGIN-001).
- **Negatives:** submit NURSE_A's email → still 202 with zero mutation (assert via API user count/unchanged row); weak/mismatched password → client + 400; retired `/auth/register/nurse` → 410.
- **Isolation:** unique email per run; abandon ephemeral account after.
- **Cleanup:** none required (or setup-cleanup per Step 7). **Layer:** UI.

### SCN-AUTH-VERIFY-001 — Email verification one-shot

- **Actor:** Anonymous. **Identity:** per-run ephemeral from SCN-AUTH-SIGNUP-001.
- **UCs:** UC-AUTH-04. **Data:** TOKENS_AUTH. **Edges:** D-01, D-00b. **Level:** L1.
- **Starting state:** fresh unverified account + valid token (via real sign-up, never hand-made).
- **Inputs:** `?token=` from issuance. **Actions:** open `/auth/verify-email/confirm?token=`.
- **Expected UI:** success state. **API:** 200. **Backend:** EmailVerified=true, token consumed.
- **Persisted:** `Users.EmailVerified`, token `UsedAt`. **Downstream:** login now succeeds.
- **Negatives:** replay same token → 409; garbage token → 409; empty token → missing guard view.
- **Isolation/Cleanup:** single-use account. **Layer:** UI.

### SCN-AUTH-LOGIN-001 — Login matrix

- **Actor:** Anonymous. **Identity:** NURSE_A (+ ephemeral unverified; STATE_INACTIVE_USER flag).
- **UCs:** UC-AUTH-01. **Data:** STATE_INACTIVE_USER (setup-synthesized, for inactive branch only). **Edges:** D-01, D-02. **Level:** L1.
- **Starting state:** NURSE_A baseline; one ephemeral unverified; one inactive-flagged setup account.
- **Inputs:** email + password variants. **Actions:** submit `/auth/sign-in` per case.
- **Expected UI:** verified → `/account`; unverified-correct → verify-required message; wrong/inactive → generic invalid-credentials, no session.
- **API:** 200 + AuthResult (success); 403 coded unverified; 401 indistinguishable otherwise.
- **Persisted:** success → RefreshToken row + LastLoginAt; failures → nothing.
- **Negatives:** covered as the matrix itself (401/403/400-empty).
- **Isolation:** read-only except success login (logout after). **Cleanup:** revoke/logout. **Layer:** UI.

### SCN-AUTH-SESSION-001 — Refresh, guards, session lifecycle

- **Actor:** Authenticated + Anonymous. **Identity:** NURSE_LC (+ ANONYMOUS for guard paths).
- **UCs:** UC-AUTH-02 (API_ONLY branch), UC-AUTH-10. **Data:** TOKENS_AUTH. **Edges:** D-05, D-02. **Level:** L1.
- **Starting state:** NURSE_LC session (login via real flow inside scenario).
- **Inputs:** none user-facing. **Actions:** call refresh API; reuse old token; visit `/nurse/profile` anonymous; visit `/onboarding/profile` complete; visit `/admin/users` as nurse.
- **Expected UI:** anon → `/auth/sign-in?returnUrl=`; complete → bounced from onboarding; nurse → `/access-denied`.
- **API:** rotation returns new pair; reuse → 401 and revokes all (re-login required).
- **Backend:** old RevokedAt set; reuse-revocation wipes user tokens. **Persisted:** token rows.
- **Downstream:** re-login restores session. **Negatives:** expired refresh → 401.
- **Isolation:** self-contained on NURSE_LC; re-login at end. **Cleanup:** logout. **Layer:** MIXED.

### SCN-AUTH-PASSWORD-001 — Forgot/reset round-trip with restore

- **Actor:** Anonymous. **Identity:** NURSE_LC (credential-mutation isolation).
- **UCs:** UC-AUTH-06, UC-AUTH-07. **Data:** TOKENS_AUTH. **Edges:** D-03. **Level:** L1.
- **Starting state:** NURSE_LC baseline, canonical password set.
- **Inputs:** NURSE_LC email; token from issuance; away-password then canonical `Test-Nurse-LC-03`.
- **Actions:** `/auth/forgot-password` submit (twice: known + unknown email) → `/auth/reset-password?token=` submit → login with new → repeat cycle back to canonical → login.
- **Expected UI:** generic success both emails; reset success view.
- **API:** forgot always 200-generic; reset 200; old password → 401 after; canonical works after restore.
- **Backend:** 1h token rows; hash replaced; ALL refresh revoked per reset.
- **Persisted:** PasswordHash changes; token consume rows. **Downstream:** fresh login verified.
- **Negatives:** unknown email indistinguishable; reused/expired token → 409; weak new password → 400.
- **Isolation/Cleanup:** restore canonical password + verify login before reuse. **Layer:** UI.

### SCN-AUTH-PROFILE-001 — View/update + forced onboarding

- **Actor:** Authenticated. **Identity:** NURSE_A.
- **UCs:** UC-AUTH-08. **Data:** none beyond identity. **Edges:** D-02. **Level:** L1.
- **Starting state:** baseline names set.
- **Inputs:** edited first/last (≤100); then blanked (SP-PROFILE-INCOMPLETE); then restored.
- **Actions:** `/account` edit → save; clear a name → revisit guarded nurse route → `/onboarding/profile` forced → refill → return.
- **Expected UI:** saved announce; forced onboarding redirect; return to `returnUrl||/account`.
- **API:** `PUT /me/profile` 200; `GET /me` reflects. **Persisted:** Users.First/LastName.
- **Downstream:** IsProfileComplete gates pass again. **Negatives:** empty → 400; anonymous → sign-in.
- **Cleanup:** restore baseline names. **Layer:** UI.

### SCN-AUTH-RESEND-001 — Resend verification (API-only)

- **Actor:** Authenticated. **Identity:** NURSE_LC (verified no-op) + ephemeral unverified (real send).
- **UCs:** UC-AUTH-05 (state B). **Data:** TOKENS_AUTH. **Edges:** D-01. **Level:** L1.
- **Actions:** POST send-verification-email authenticated. **API:** verified → 200 no new email; unverified → 200 + prior tokens invalidated + new 24h token.
- **Persisted:** token rows as described. **Negatives:** anonymous → 401.
- **Layer:** API_ONLY (no UI trigger exists — specs assert absence).

## NUR family (all NURSE_A primary; NURSE_B only where noted)

### SCN-NUR-PROFILE-001 — Personal information + overview

- **Actor:** Nurse. **Identity:** NURSE_A. **UCs:** UC-NUR-01, UC-NUR-08.
- **Data:** PROFILE_NURSE_A, REF_COUNTRY_US. **Edges:** D-02 baseline, D-21 (ref lookups). **Level:** L1.
- **Actions:** `/nurse/profile/personal-information` edit (headline≤160, years 0–80, available toggle, GB license country variant) → save → `/nurse/profile` reflects.
- **Expected UI/API/persisted:** DTO round-trip; overview cards update. **Negatives:** years 999 → 400; anonymous/Employer → guard/403.
- **Cleanup:** restore PROFILE_NURSE_A snapshot. **Layer:** UI.

### SCN-NUR-EXPERIENCE-001 — Experiences CRUD + ownership

- **Actor:** Nurse. **Identities:** NURSE_A (+ NURSE_B witness for 404).
- **UCs:** UC-NUR-02. **Data:** PROFILE_NURSE_A, REF_COUNTRY_US. **Level:** L1.
- **Actions:** list → create → edit → delete (TwoStep) on own; GET/PUT/DELETE NURSE_B's id → 404.
- **Expected:** 200/201/204; scoped rows; gone-notice on stale delete.
- **Negatives:** end<start → 400; cross-owner → 404. **Cleanup:** delete created rows. **Layer:** UI.

### SCN-NUR-EDUCATION-001 — Education CRUD + ownership

- Same contract as EXPERIENCE for **UC-NUR-03** (institution/degree/field/country/dates).
- **Cleanup:** delete created rows. **Layer:** UI.

### SCN-NUR-CERTIFICATES-001 — Certificates CRUD + ownership

- Same contract as EXPERIENCE for **UC-NUR-04** (name/org/issue/expiry≥issue/credential).
- **Cleanup:** delete created rows. **Layer:** UI.

### SCN-NUR-LANGUAGES-001 — Languages replace-all

- **Actor:** Nurse. **Identity:** NURSE_A. **UCs:** UC-NUR-05. **Data:** PROFILE_NURSE_A, REF_LANGUAGE_EN.
- **Actions:** replace set (EN/Fluent + AR/Intermediate) → save → list reflects → restore EN-only.
- **Negatives:** unknown languageId → 400; concurrent catalog change → 409 refresh path.
- **Cleanup:** restore baseline set. **Layer:** UI.

### SCN-NUR-SKILLS-001 — Skills replace-all

- **Actor:** Nurse. **Identity:** NURSE_A. **UCs:** UC-NUR-06. **Data:** PROFILE_NURSE_A.
- **Actions:** replace chips → save →dedup/case-fold asserted → restore wound-care+triage.
- **Negatives:** over-length/over-count → 400. **Cleanup:** restore. **Layer:** UI.

### SCN-NUR-CV-001 — CV upload/replace/delete (PARTIAL pin)

- **Actor:** Nurse. **Identities:** NURSE_A (+ NURSE_B 404). **UCs:** UC-NUR-07.
- **Data:** PROFILE_NURSE_A. **Actions:** upload pdf ≤5MB → metadata shown → replace → delete → empty.
- **Expected UI/API/persisted:** NurseCvDocumentDto lifecycle; file replaced on disk.
- **Negatives:** oversize/bad-ext/empty → client + 400; cross-owner → 404; preview/download asserted ABSENT (pins PARTIAL).
- **Cleanup:** delete test CV. **Layer:** UI.

## EMP / REC family

### SCN-EMP-PROFILE-001 — Employer profile + organization (API-only)

- **Actor:** Employer. **Identity:** EMPLOYER_A. **UCs:** UC-EMP-01, UC-EMP-02 (state B).
- **Data:** PROFILE_EMPLOYER_A. **Edges:** D-04. **Level:** L1.
- **Actions:** GET/PUT profile (title/dept) → GET/PUT organization (name req + contacts).
- **Expected API:** 200 DTOs. **Persisted:** 1:1 rows. **Negatives:** blank name → 400; nurse token → 403; anon → 401.
- **Cleanup:** restore snapshot. **Layer:** API_ONLY (no frontend routes exist).

### SCN-REC-DISCOVERY-001 — Candidate search gating (API-only)

- **Actor:** Employer. **Identities:** EMPLOYER_A (+ EMPLOYER_B second searcher).
- **UCs:** UC-REC-01 (state B). **Data:** PROFILE_NURSE_A, PROFILE_EMPLOYER_A, REF_COUNTRY_US/GB, REF_LANGUAGE_EN. **Edges:** D-17. **Level:** L3 (needs NURSE_A available baseline).
- **Actions:** search unfiltered → filter licenseCountry=US → minYears → skills=triage → language=EN.
- **Expected API:** NURSE_A present with safe projection only (no PII beyond approved fields); unavailable/incomplete nurses absent.
- **Negatives:** anon → 401. **Layer:** API_ONLY.

### SCN-REC-REQUEST-001 — Full request lifecycle, cross-actor

- **Actors:** Employer + Nurse. **Identities:** EMPLOYER_A (creator), NURSE_A (decider), EMPLOYER_B (non-owned 404).
- **UCs:** UC-REC-02 + UC-REC-03 (state B) + UC-REC-04 (state A). **Data:** EPH_CONTACT_REQUEST, PROFILE_NURSE_A (availability), PROFILE_EMPLOYER_A. **Edges:** D-17, D-18. **Level:** L3.
- **Actions (API):** EMPLOYER_A POST create → 201 + Pending; duplicate create → 409; EMPLOYER_B cancel other → 404; EMPLOYER_A list filtered.
- **Actions (UI):** NURSE_A opens `/nurse/contact-requests` → filters → Approve → terminal card.
- **Expected UI/API/persisted:** ReceivedContactRequestDto Approved + RespondedAt; employer view sees terminal.
- **Second pass (new request):** employer cancel while Pending → Cancelled; nurse double-decide → 409 gone/decided notice.
- **Negatives:** decide non-owned → 404; decide terminal → 409. **Cleanup:** per-row terminal, none needed. **Layer:** MIXED.

## EXM family

### SCN-EXM-FREE-001 — Free exam full journey

- **Actor:** Nurse (catalog reads: any authenticated). **Identity:** NURSE_A (+ ADMIN_A upstream setup, done once per L2).
- **UCs:** UC-EXM-01/02/03/04/05/06. **Data:** CAT_PRIMARY, EXAM_FREE_PRIMARY, QUESTION_SET_FREE, EPH_EXAM_SESSION, PROVENANCE_SESSION, SCORE_RESULT, REF_COUNTRY_US. **Edges:** D-06, D-07, D-08. **Level:** L3→L4.
- **Starting state:** published free exam + V1 + 3 questions (via UC-ADM-04/05/06 setup chain).
- **Actions (UI):** `/exams` filter → detail → instructions (TwoStep start) → session pager (answer QFREE-01/02 correct, QFREE-03 wrong) → premature result GET → 409 → confirm submit → result (2/4 deterministic) → review → history → analytics.
- **Expected UI:** countdown/unsaved badge; score 2/4; per-question correctness; history row; analytics aggregates move.
- **API:** session/result/review DTOs. **Backend:** InProgress→Submitted + ExamScoringService fields.
- **Persisted:** session + answers + score rows + provenance(Source=Free).
- **Downstream:** history/analytics reflect exactly one new finalized session.
- **Negatives:** premature result/review → 409; unpublished exam → invisible (setup assert).
- **Cleanup:** session terminal, none needed. **Layer:** MIXED.

### SCN-EXM-PAID-001 — Paid exam via sandbox grant (NURSE_B denied control)

- **Actor:** Nurse. **Identities:** NURSE_A (buyer/taker), NURSE_B (grantless denied control), ADMIN_A (setup).
- **UCs:** UC-EXM-02 paid branch (A); UC-COM-05 + UC-COM-06 (B); GRANT_STANDALONE via flow.
- **Data:** EXAM_PAID_PRIMARY, QUESTION_SET_PAID, PRODUCT_EXAM_PRIMARY, EPH_ORDER, EPH_EXAM_SESSION. **Edges:** D-09, D-10, D-11. **Level:** L3.
- **Actions:** NURSE_B attempts paid start → 403 requiresPurchase (UI path asserted, no session created) → NURSE_A order (UC-COM-02) → checkout init → sandbox complete → Paid + grant → start → answer all correctly → submit → 4/4 pass.
- **Expected API:** 201 order; checkout DTO; completion DTO with grantedExamIds; session/result DTOs.
- **Persisted:** PendingPayment→Paid; ExamAccessGrant row; session Submitted + provenance(Source=StandaloneGrant).
- **Negatives:** as above + double-submit → 409. **Cleanup:** finalize; orders historical. **Layer:** MIXED.

### SCN-EXM-OWNERSHIP-001 — Session ownership isolation (supplementary)

- **Actor:** Nurse. **Identities:** NURSE_A (owner), NURSE_B (intruder).
- **UCs:** none primary (supplementary to UC-EXM-03/04). **Data:** EPH_EXAM_SESSION (owned by A).
- **Actions:** B GETs/PUTs/submits A's session id → 404 each; A completes normally.
- **Layer:** MIXED (UI possible; API asserts suffice).

## COM family

### SCN-COM-ORDER-001 — Standalone order lifecycle

- **Actor:** Nurse. **Identity:** NURSE_A (+ ADMIN_A setup).
- **UCs:** UC-COM-01/02/03/04. **Data:** PRODUCT_EXAM_PRIMARY, EPH_ORDER. **Edges:** D-09, D-12. **Level:** L3.
- **Actions (UI):** `/commerce/products` → detail → `/checkout?productId=` → create → PendingPayment view → `/commerce/orders` lists → detail → confirm-cancel → Cancelled.
- **Expected UI/API/persisted:** money formatting; 201 + Location; order DTOs; PendingPayment→Cancelled.
- **Negatives:** missing productId → missingContext; unknown id → 404; both IDs → 400; double-cancel → 409; inactive-product 409 covered in SCN-ADM-PRODUCT-001 (canonical product must stay Active).
- **Cleanup:** cancelled terminal. **Layer:** MIXED.

## PP family

### SCN-PP-DISCOVERY-001 — Public offer browse

- **Actor:** Anonymous. **Identity:** ANONYMOUS (+ ADMIN_A setup).
- **UCs:** UC-PP-01. **Data:** OFFER_PRIMARY. **Edges:** D-13. **Level:** L3.
- **Actions:** `/preparation-packages` list → `/:offerSlug` detail → unknown slug → 404 view.
- **Expected:** active-only catalog; price/duration/counts rendered. **Layer:** UI.

### SCN-PP-JOURNEY-001 — Package purchase → practice → exam → report

- **Actor:** Nurse. **Identities:** NURSE_A (+ ADMIN_A setup).
- **UCs:** UC-PP-02/03/04/05 (A); UC-COM-02 package path + UC-COM-06 (B).
- **Data:** OFFER_PRIMARY, PACKAGE_VERSION_PRIMARY, EPH_ORDER, ENTITLEMENT_PACKAGE, COLLECTION_PRIMARY, PROGRESS_PRACTICE, EPH_EXAM_SESSION, SCORE_RESULT, PROFILE_PRIMARY, TOPIC_PRIMARY_A/B, MATERIAL_PRIMARY. **Edges:** D-13, D-14, D-15, D-16. **Level:** L3→L4.
- **Starting state:** OFFER_PRIMARY Active (full UC-ADM-08 chain validated).
- **Actions:** entitlements list (empty pre-purchase) → package order → sandbox complete → Active entitlement + 4 rights → practice wizard (answer PPRAC-01 correct, PPRAC-02 wrong; immediate feedback; progress 1/1) → package exam start (attempt Available→Consumed) → answer package session → submit → report (topic A/B breakdown + guidance limited to MATERIAL/COLLECTION versions; no question text/keys/rationales).
- **Expected UI/API/persisted:** entitlement DTO; submission DTOs; PackageExamSessionStartDto; immutable report row; provenance(Source=Package).
- **Negatives:** entitlement of other nurse → 404 (with NURSE_B); premature report → 409; double-consume attempt → 409. Expiry/consumed-window negatives: documented but NOT executable inside the 90-day window (assert Locked-state logic only if reachable; do not fake clocks).
- **Cleanup:** terminal states persist as facts; no reuse. **Layer:** MIXED.

### SCN-PP-OWNERSHIP-001 — Entitlement/report isolation (supplementary)

- **Identities:** NURSE_A (owner), NURSE_B (intruder). **Data:** ENTITLEMENT_PACKAGE, package EPH_EXAM_SESSION.
- **Actions:** B reads A's entitlement/report/session → 404. **Layer:** MIXED.

## ADM family (all ADMIN_A; ephemeral mutation targets)

### SCN-ADM-USERS-001 — User governance + safe role subject

- **Actor:** Administrator. **Identity:** ADMIN_A.
- **UCs:** UC-ADM-01 (A-UI) + UC-ADM-02 (B-API). **Edges:** D-04, D-19, D-20 (bootstrap→ADMIN_A). **Level:** L1–L2.
- **Actions:** `/admin/users` search/paginate → detail → (UI role view); API: create ephemeral user (unique email) → replace role → subject login reflects guards → restore/revoke as applicable.
- **Note:** NURSE_A is NOT the role subject (mutation would contaminate ~30 UCs); ephemeral subject justifies the deviation from the Step 2 mapping sketch.
- **Expected API:** 200/201; UserRoles replaced; all subject refresh revoked (re-login forced).
- **Negatives:** nurse token on admin routes → 403; invalid role → 400; SuperAdmin assignment rejected.
- **Cleanup:** ephemeral subject retained uniquely or removed per setup policy. **Layer:** MIXED.

### SCN-ADM-CATEGORY-001 — Category lifecycle on ephemeral target

- **UCs:** UC-ADM-03. **Data:** EPH_ADMIN_CATEGORY (CAT_PRIMARY is read-reference only). **Edges:** D-06. **Level:** L2.
- **Actions (UI):** create → edit → archive → restore → delete; duplicate slug → 409.
- **Expected:** 201+Location/200/204; IsActive flips. **Cleanup:** delete-if-legal else uniquely-slung remainder. **Layer:** UI.

### SCN-ADM-EXAM-001 — Exam/version/question lifecycle on ephemeral target

- **UCs:** UC-ADM-04/05/06 (B-API; UI does not exist for versions/questions).
- **Data:** EPH_ADMIN_EXAM. **Edges:** D-06. **Level:** L2.
- **Actions:** exam create → version draft → questions/options → validate clean → publish → retire; second draft → delete; write to published version → rejected (draft gate); publish invalid (no questions) → 409/issues.
- **Expected API/persisted:** Draft→Published→Retired; draft-only delete. **Layer:** API_ONLY.

### SCN-ADM-PRODUCT-001 — Product admin + inactive-order proof

- **UCs:** UC-ADM-07 (B). **Data:** EPH_ADMIN_EXAM as subject exam; PRODUCT_EXAM_PRIMARY read-reference. **Level:** L2.
- **Actions:** create product → archive → order attempt → 409 inactive → restore → order OK → 201.
- **Expected:** IsActive flips; gated by `Exams.*` (no commerce perm — pins contradiction #5).
- **Negatives:** nurse token → 403. **Layer:** API_ONLY.

### SCN-ADM-PACKAGE-001 — Full package authoring chain on ephemeral pattern

- **UCs:** UC-ADM-08 (B). **Data:** EPH_ADMIN_EXAM chain following canonical pattern (ephemeral topics→profile→material→collection→definition→version→offer). **Edges:** D-13. **Level:** L2.
- **Actions:** build chain → validation clean → publish → offer activate → public catalog shows it → negative variant: version publish with zero materials → issues/409.
- **Expected:** canonical chain (TOPIC_*/PROFILE_/MATERIAL_/COLLECTION_/PACKAGE_*/OFFER_PRIMARY) is the correctness oracle, never mutated.
- **Layer:** API_ONLY.

## SYS family

### SCN-SYS-HEALTH-001 — Liveness/readiness (API-only)

- **Actor:** Anonymous infrastructure. **UCs:** UC-SYS-04 (state B).
- **Actions:** GET `/`, `/health`, `/health/live`, `/health/ready` → 200 shapes; dev-only OpenAPI noted, no snapshot asserted.
- **Layer:** API_ONLY.

## UC coverage map (54 UCs — zero unexplained)

| UC(s) | State | Covering scenario(s) |
|---|---|---|
| UC-AUTH-01 | A | SCN-AUTH-LOGIN-001 |
| UC-AUTH-02 | B | SCN-AUTH-SESSION-001 (API branch) |
| UC-AUTH-03 | A | SCN-AUTH-SIGNUP-001 |
| UC-AUTH-04 | A | SCN-AUTH-VERIFY-001 |
| UC-AUTH-05 | B | SCN-AUTH-RESEND-001 |
| UC-AUTH-06/07 | A | SCN-AUTH-PASSWORD-001 |
| UC-AUTH-08 | A | SCN-AUTH-PROFILE-001 |
| UC-AUTH-10 | A | SCN-AUTH-SESSION-001 |
| UC-NUR-01/08 | A | SCN-NUR-PROFILE-001 |
| UC-NUR-02 | A | SCN-NUR-EXPERIENCE-001 |
| UC-NUR-03 | A | SCN-NUR-EDUCATION-001 |
| UC-NUR-04 | A | SCN-NUR-CERTIFICATES-001 |
| UC-NUR-05 | A | SCN-NUR-LANGUAGES-001 |
| UC-NUR-06 | A | SCN-NUR-SKILLS-001 |
| UC-NUR-07 | A | SCN-NUR-CV-001 (PARTIAL pinned) |
| UC-EMP-01/02 | B | SCN-EMP-PROFILE-001 |
| UC-REC-01 | B | SCN-REC-DISCOVERY-001 |
| UC-REC-02/03 | B | SCN-REC-REQUEST-001 (API steps) |
| UC-REC-04 | A | SCN-REC-REQUEST-001 (UI steps) |
| UC-EXM-01..06 | A | SCN-EXM-FREE-001 (all six); paid branch via SCN-EXM-PAID-001 |
| UC-PP-01 | A | SCN-PP-DISCOVERY-001 |
| UC-PP-02/03/04/05 | A | SCN-PP-JOURNEY-001 |
| UC-COM-01/02/03/04 | A | SCN-COM-ORDER-001 (standalone); package path in SCN-PP-JOURNEY-001 |
| UC-COM-05/06 | B | SCN-EXM-PAID-001 + SCN-PP-JOURNEY-001 (API steps; dev-only for 06) |
| UC-COM-07 | D | explicitly deferred — no scenario (no provider exists) |
| UC-ADM-01 | A | SCN-ADM-USERS-001 (UI steps) |
| UC-ADM-02 | B | SCN-ADM-USERS-001 (API steps) |
| UC-ADM-03 | A | SCN-ADM-CATEGORY-001 |
| UC-ADM-04 | A | SCN-ADM-EXAM-001 scope (exam create read via UI where exists; lifecycle API) — see note |
| UC-ADM-05/06/07/08 | B | SCN-ADM-EXAM-001 / PRODUCT-001 / PACKAGE-001 |
| UC-REF-01 | A | exercised inside SCN-NUR-PROFILE-001 + SCN-NUR-LANGUAGES-001 (lookup asserts) |
| UC-SYS-01 | C | provisioning premise of every scenario (boot/seed) |
| UC-SYS-02 | C | terminal/expiry behavior asserted in SCN-EXM-FREE-001, SCN-COM-ORDER-001, SCN-PP-JOURNEY-001 |
| UC-SYS-03 | C | token issuance/consumption asserted in SCN-AUTH-SIGNUP/VERIFY/PASSWORD/RESEND-001 |
| UC-SYS-04 | B | SCN-SYS-HEALTH-001 |

Note on UC-ADM-04: exam list/detail UI exists (`/admin/exams*`) while version/question
writes are API-only; SCN-ADM-EXAM-001 drives writes via API and reads via UI where
implemented — mapped A with that explicit split, no invented UI.

## Validation summary

- 30 Scenario IDs, all unique, family-prefixed (AUTH 7, NUR 7, EMP/REC 3, EXM 3, COM 1, PP 3, ADM 5, SYS 1).
- All UC/Identity/Data/edge references resolve to Steps 1–4 (no new identities, no new Data IDs; ephemeral entities reference existing EPH_* IDs with runtime production described).
- No BACKEND_ONLY UC carries UI steps; UC-COM-07 never a success path; expiry-inside-90-day-window explicitly marked non-executable, not faked.
- Ownership scenarios are supplementary (primary UC coverage sits in the named journey scenarios).
