# Canonical Test Identity Registry (Step 2)

> Authority: `docs/testing/system-use-case-catalog.md` (Step 1, 54 UCs).
> Baseline: `feat/2026-09-27-stitch-ui-refactor` @ `cbfe1eb`.
> This registry defines identities ONLY. No accounts are created here; no database
> is touched; no business fixtures are defined (Step 3); no scenarios yet (Step 4).
> REUSE BEFORE CREATE. Actor ≠ Identity. All credentials below are synthetic
> TEST-ONLY values: permitted Local/Test only; NEVER Staging/Production/real users.

## 1. Policy notes verified against implementation

- Password rule (verified `PublicRegisterCommandValidator.cs:11`,
  `RegisterUserCommandValidator.cs:11-13`, `ResetPasswordCommandValidator.cs:11-13`):
  non-empty, minimum 8 chars, at least one `[A-Z]`, at least one `[0-9]`.
  Every canonical password below satisfies it.
- Email: valid format, ≤256. Username: non-empty, ≤50, unique (NormalizedUsername).
  All values below satisfy this.
- `IsProfileComplete` (verified `Domain/Identity/User.cs:19-21`) = FirstName AND
  LastName non-blank. Clearing either name deterministically returns an account to
  incomplete; re-setting restores complete. No separate "incomplete identity" needed.
- Public sign-up creates Nurse accounts only (role forced); Employer/Admin accounts
  require admin creation (`POST /api/v1/auth/register`, UC-ADM-02) or seed/bootstrap.
- No user-facing deactivate/reactivate endpoint exists in current HEAD, so INACTIVE
  is a setup-produced profile, not a flow-produced one.
- Role replacement revokes ALL refresh tokens (`UpdateUserRolesCommandHandler`);
  password reset revokes ALL refresh tokens. Any test doing either must re-login.
- Precedent for documenting synthetic QA credentials in-repo exists
  (`Playwright MCP Full Review/` records `QaInitial2026!` etc.); no repo policy
  prohibits synthetic TEST-ONLY values. Real secrets/credentials rules
  (PROJECT_RULES, AGENTS.md) concern real systems, not these reserved-namespace values.
- Local mail inspection precedent exists (MailPit in legacy QA); future flows must
  still use non-routable `.test` addresses only.

## 2. Non-identities (no credentials, ever)

| ID | Actor | Notes |
|---|---|---|
| ANONYMOUS | Anonymous / Public Visitor | No account, no email, no password. Used for UC-PP-01 browse, UC-AUTH-01/03/04/06/07 entries, 401 paths. |
| SYSTEM | System / non-human | No login. Covers UC-SYS-01..04. Never assigned email/password. |

## 3. Canonical credentialed identities (6 total: Nurse 3, Employer 2, Admin 1)

### NURSE_A — Primary nurse journey identity

- Identity ID: NURSE_A. Actor: Nurse. Login capable: YES.
- Email: `nurse.a@test.nursing-platform.test`. Username: `test_nurse_a`.
- Credential Ref: CRED_NURSE_A. Canonical Test-Only Password: `Test-Nurse-A-01`.
- Baseline: Active YES; verified YES; profile complete YES (First/Last set);
  role Nurse; permissions: none beyond role (seed grants Nurse zero permissions).
- Creation authority: public sign-up (UC-AUTH-03) + verify (UC-AUTH-04) + set names (UC-AUTH-08).
- Primary families: UC-AUTH-01/08/10, UC-NUR-01..08, UC-REC-04, UC-EXM-01..06,
  UC-PP-02..05, UC-COM-01..06, UC-REF-01.
- Ownership scope: own profile sections, CV, sessions, answers, orders, entitlements,
  practice progress, received contact decisions.
- Allowed mutations: profile/languages/skills edits (snapshot + restore); resend-verification
  (verified → safe no-op success); forgot-password (no state change); order create/cancel;
  exam attempts; practice answers; contact approve/reject on per-run requests.
- Unsafe/persistent: password reset (forbidden here — use NURSE_LC); role change;
  deactivation; verifying-state transitions (already verified).
- Reset expectation: restore names/languages/skills to baseline snapshot; cancel or
  document per-run orders; per-run entities use unique markers + cleanup; terminal
  contact decisions only on per-run requests (fresh upstream per test).
- Simultaneous use: with NURSE_B (ownership), EMPLOYER_A (recruitment), ADMIN_A (setup).
- Why exists: minimum primary nurse — carries ~30 UCs.

### NURSE_B — Ownership-witness nurse

- Identity ID: NURSE_B. Actor: Nurse. Login capable: YES.
- Email: `nurse.b@test.nursing-platform.test`. Username: `test_nurse_b`.
- Credential Ref: CRED_NURSE_B. Canonical Test-Only Password: `Test-Nurse-B-02`.
- Baseline: same shape as NURSE_A (Active/verified/complete, role Nurse).
- Creation authority: public sign-up + verify + set names.
- Primary families: ownership negatives for UC-NUR-02/03/04/07, UC-EXM-03/04,
  UC-COM-03/04, UC-REC-04 (non-owned 404), second candidate for UC-REC-01.
- Ownership scope: minimal stable profile + witness entities only.
- Allowed mutations: none beyond baseline maintenance; entities created only as
  ownership witnesses with cleanup.
- Unsafe/persistent: credential changes (would break parallel witness logins);
  password reset; role change; deactivation.
- Reset expectation: keep baseline stable; remove witness entities after use.
- Simultaneous use: with NURSE_A (ownership proof).
- Why exists: NURSE_A cannot be both owners at once. Why NURSE_A cannot replace it:
  proving `A cannot read B's entity` requires two simultaneous distinct owners.

### NURSE_LC — Auth-lifecycle nurse (credential-mutation isolation)

- Identity ID: NURSE_LC. Actor: Nurse. Login capable: YES.
- Email: `nurse.lc@test.nursing-platform.test`. Username: `test_nurse_lc`.
- Credential Ref: CRED_NURSE_LC. Canonical Test-Only Password: `Test-Nurse-LC-03`.
- Baseline: Active/verified/complete, role Nurse (same as NURSE_A).
- Creation authority: public sign-up + verify + set names.
- Primary families: UC-AUTH-02 (refresh behaviors), UC-AUTH-05 (verified-path resend),
  UC-AUTH-06 (forgot, no mutation), UC-AUTH-07 (reset round-trip + restore),
  UC-AUTH-10 session edge cases.
- Ownership scope: none required; holds no journey entities.
- Allowed mutations: password reset round-trip (reset away via UC-AUTH-07, then reset
  back to `Test-Nurse-LC-03` via a second UC-AUTH-06/07 cycle; re-login after each
  since reset revokes all refresh tokens).
- Unsafe/persistent: none beyond the documented round-trip.
- Reset expectation: canonical password restored + fresh login verified before reuse.
- Simultaneous use: standalone; never the ownership witness.
- Why exists: reset/refresh tests revoke tokens and change credentials; running them
  on NURSE_A would break ~30 journey UCs on restore failure, and on NURSE_B would
  destabilize the ownership witness. Isolation is the justification.

### EMPLOYER_A — Primary employer

- Identity ID: EMPLOYER_A. Actor: Employer. Login capable: YES.
- Email: `employer.a@test.nursing-platform.test`. Username: `test_employer_a`.
- Credential Ref: CRED_EMPLOYER_A. Canonical Test-Only Password: `Test-Employer-A-04`.
- Baseline: Active YES; verified YES; role Employer; profile (UC-EMP-01) + organization
  (UC-EMP-02) complete.
- Creation authority: admin-create (UC-ADM-02) + verify + profile/org upsert
  (public sign-up cannot create Employers — implementation-forced).
- Primary families: UC-EMP-01/02, UC-REC-01/02/03.
- Ownership scope: own profile/org, own sent contact requests.
- Allowed mutations: profile/org edits (restore); request create/cancel on per-run targets.
- Unsafe/persistent: role change; deactivation.
- Reset expectation: restore profile/org snapshot; requests are per-request terminal
  (no account reset needed).
- Simultaneous use: with NURSE_A (recruitment handoff), EMPLOYER_B (ownership).
- Why exists: minimum primary employer — sole actor for backend-only recruitment flows.

### EMPLOYER_B — Ownership-witness employer

- Identity ID: EMPLOYER_B. Actor: Employer. Login capable: YES.
- Email: `employer.b@test.nursing-platform.test`. Username: `test_employer_b`.
- Credential Ref: CRED_EMPLOYER_B. Canonical Test-Only Password: `Test-Employer-B-05`.
- Baseline/creation: same as EMPLOYER_A.
- Primary families: UC-REC-03 non-owned 404; second searcher for UC-REC-01.
- Ownership scope: minimal stable profile/org; witness requests only.
- Allowed mutations: baseline maintenance only.
- Reset expectation: stable baseline; remove witness requests per run if creatable.
- Simultaneous use: with EMPLOYER_A.
- Why exists: proving `EMPLOYER_A cannot cancel/read EMPLOYER_B's request` needs two
  simultaneous employer owners. Why EMPLOYER_A cannot replace it: same-actor ownership
  proof is impossible with one account.

### ADMIN_A — Canonical administrator (immutable role)

- Identity ID: ADMIN_A. Actor: Administrator. Login capable: YES.
- Email: `admin.a@test.nursing-platform.test`. Username: `test_admin_a`.
- Credential Ref: CRED_ADMIN_A. Canonical Test-Only Password: `Test-Admin-A-06`.
- Baseline: Active YES; verified YES; names set; role Admin; full `Permissions.Admin`
  (via seed).
- Creation authority: seed/bootstrap (`BootstrapAdminService`) or admin-create;
  provisioned by test setup, never via public sign-up.
- Primary families: UC-ADM-01/02/03/04 (UI) + UC-ADM-05/06/07/08 (API setup chains
  feeding EXM/PP/COM).
- Ownership scope: setup entities (categories/exams/versions/questions/products/
  packages/offers) with unique-slug-per-run convention; published entities cannot be
  deleted (delete is draft-only) so teardown = retire where supported, else leave
  uniquely-slung rows.
- Allowed mutations: NONE on the account itself (no role change, no deactivation,
  no password reset in tests — reset mechanics belong to NURSE_LC).
- Unsafe/persistent: any account mutation — forbidden.
- Reset expectation: no account reset; re-login if tokens expire.
- Simultaneous use: sequential setup before nurse/employer consumption.
- Why exists: sole privileged setup actor; doubles as 403-positive control
  (limited-permission checks use Nurse/Anonymous instead — see §7).

## 4. State profiles (no new accounts)

| Profile ID | Identity | Required state | Produced by | Relevant UCs | Restorable | Reset before switch |
|---|---|---|---|---|---|---|
| SP-BASELINE-COMPLETE | NURSE_A/B/LC, EMPLOYER_A/B, ADMIN_A | Active, verified, complete (+role profile/org where applicable) | Signup+verify+names (nurse); admin-create+upserts (employer); seed (admin) | All baseline journeys | n/a (baseline) | n/a |
| SP-PROFILE-INCOMPLETE | NURSE_A (preferred; LC/B possible) | FirstName or LastName blank | Clear a name via UC-AUTH-08 (user flow) | UC-AUTH-08 onboarding force, UC-AUTH-10 guard | YES (re-set names) | YES (restore names) |
| SP-INACTIVE | NURSE_A or EMPLOYER_A | IsActive=false | Test setup ONLY — no user-facing deactivate flow exists in HEAD | 401/inactive negatives (backend behavior) | YES via setup reactivation | YES |
| SP-NEW-UNVERIFIED | Per-run ephemeral account (derivation: `lc-<run>@test.nursing-platform.test`, username `test_lc_<run>`, password `Test-Lc-Run-07`) | Registered, unverified | Fresh UC-AUTH-03 per run | UC-AUTH-03/04, unverified-login 403, real-send UC-AUTH-05 | NO (registration is one-way; account persists) | n/a — single-use, then abandon or setup-cleanup |
| SP-VERIFIED-INCOMPLETE | Per-run ephemeral (signup+verify, names never set) | Verified, incomplete | UC-AUTH-03 then UC-AUTH-04 per run | Onboarding-first-run paths | One-way forward only (set names → baseline-like) | n/a — single-use |

Ephemeral derivation is justified (not proliferation): the implementation cannot
un-register an account, and verify is one-shot per account, so pre-verification
states cannot be reused. Duplicate-signup 202 uses NURSE_A's email (safe: no mutation).

## 5. UC → identity mapping (49 human UCs; UC-COM-07 DEFERRED unmapped; UC-SYS-* → System)

| UC | Actor | Primary | Secondary / profile | Reason |
|---|---|---|---|---|
| UC-AUTH-01 | Anonymous→Auth | NURSE_A / CRED_NURSE_A | — | Standard login |
| UC-AUTH-02 | Auth | NURSE_LC | — | Token-rotation edge behavior |
| UC-AUTH-03 | Anonymous | per-run ephemeral (SP-NEW-UNVERIFIED) | NURSE_A email for duplicate-202 case | Creation is the SUT; duplicates are safe on NURSE_A |
| UC-AUTH-04 | Anonymous | per-run ephemeral | — | Verify is one-shot per account |
| UC-AUTH-05 | Auth | NURSE_LC (verified no-op path) | ephemeral SP-NEW-UNVERIFIED for real-send path | Both backend branches |
| UC-AUTH-06 | Anonymous/Auth | NURSE_LC | — | No state change; safe |
| UC-AUTH-07 | Anonymous | NURSE_LC | restore-to-canonical round-trip | Credential mutation isolated |
| UC-AUTH-08 | Auth | NURSE_A | SP-PROFILE-INCOMPLETE sub-case | Names restore after |
| UC-AUTH-10 | Auth/Anon | NURSE_A + ANONYMOUS | — | Guard redirects need both |
| UC-NUR-01..06,08 | Nurse | NURSE_A | — | Own-data journeys |
| UC-NUR-07 | Nurse | NURSE_A | NURSE_B for 404 cross-owner | PARTIAL incl. absent-preview assert |
| UC-NUR-02/03/04 cross-owner | Nurse | NURSE_A | NURSE_B witness entities | Ownership 404 |
| UC-EMP-01/02 | Employer | EMPLOYER_A | — | Own profile/org |
| UC-REC-01 | Employer | EMPLOYER_A | EMPLOYER_B second searcher; NURSE_A/B as candidates | Search + availability gating |
| UC-REC-02 | Employer | EMPLOYER_A | NURSE_A as target | Create on available candidate |
| UC-REC-03 | Employer | EMPLOYER_A | EMPLOYER_B for non-owned 404 | Ownership boundary |
| UC-REC-04 | Nurse | NURSE_A | EMPLOYER_A upstream creator | Cross-actor handoff |
| UC-EXM-01..06 | Auth/Nurse | NURSE_A | ADMIN_A upstream setup (ADM chain) | Consume published content |
| UC-PP-01 | Anonymous | ANONYMOUS | ADMIN_A upstream offer; NURSE_A purchase context | Public browse |
| UC-PP-02..05 | Nurse | NURSE_A | ADMIN_A upstream chain | Entitlement consumption |
| UC-COM-01 | Auth | NURSE_A | ADMIN_A upstream product | Authenticated read |
| UC-COM-02/03/04 | Nurse | NURSE_A | ADMIN_A upstream product/offer | Buyer lifecycle |
| UC-COM-05/06 | Nurse/Auth | NURSE_A | — | Same-identity API flows (dev-only for 06) |
| UC-ADM-01/02/03/04 | Admin | ADMIN_A | NURSE_A as role-change subject (re-login after) | Admin UI + subject |
| UC-ADM-05/06/07/08 | Admin | ADMIN_A | — | API setup chains |
| UC-REF-01 | Auth | NURSE_A | — | Exercised via dependent forms |
| UC-COM-07 | — | UNMAPPED (DEFERRED) | — | No prod provider exists |
| UC-SYS-01..04 | System | none | — | Non-human; no credentials |

## 6. Reuse decisions & rejected identities

- Reused instead of proliferated: one primary per actor; incomplete/inactive covered
  by profiles (§4), not accounts; API-only flows reuse actor identities
  (no `*_API_USER`); 403-negatives reuse NURSE_A/ANONYMOUS.
- NURSE_LC split from NURSE_A/B: credential-mutation isolation (documented §3).
- NURSE_B / EMPLOYER_B: simultaneous-ownership proofs only (documented §3).
- Rejected: NURSE_C+ (no third-owner need); ADMIN_LIMITED (no partial-permission role
  semantics exist — `Roles.Manage`/`Permissions.*` are unenforced constants with no
  product flow; limited-permission coverage is a Step 3 setup consideration via direct
  permission grants, not an identity); EXPERT identity (zero workflows — Gate 1);
  SUPERADMIN persona (seed-only); per-state accounts (profiles suffice); per-test
  accounts except forced ephemeral pre-verification states (§4).

## 7. Admin permission-testing strategy (no ADMIN_LIMITED)

Positive: ADMIN_A (full perms). Negative 403: NURSE_A (authenticated, zero admin perms)
and ANONYMOUS (401). ADMIN_A is never muted. If Step 3 needs least-privilege cases,
construct them in setup via direct `RolePermissions` grants (no product flow exists —
`Roles.Manage` unenforced), flagged as setup-synthesized, never as product behavior.

## 8. Security boundary

- Permitted: Local / Test environments only, reserved `.test` namespace
  (non-routable), MailPit-style local inspection only.
- Forbidden: Staging, Production, real company/personal mailboxes, existing human
  accounts, deployment/bootstrap secrets, reuse of any discovered real credential
  (none were read: values above are freshly invented).
- Future harness MUST fail closed when pointed at a non-test environment
  (harness design is out of scope for Step 2).
