# Stitch UI Refactor Report

Canonical detailed report for GOAL `STITCH-UI-REFACTOR-2026-09-27` (visual implementation + UI
refactor). Stitch owns approved visual composition; repository/backend/OpenAPI/route/security/product
contracts own functional and business behavior. No backend, API, route, redirect, permission, or
business-rule changes are authorized by this GOAL. The Landing/Home/default-entry initiative is a
separate task and is excluded.

## Index

Batch 2 public-header navigation (AUTH-001/002/005/006/007/008/010/011): the visual
`Preparation Packages` link reuses the already mounted anonymous catalog route
`PREPARATION_PACKAGES_OFFERS` (`/preparation-packages`) through `canonicalRoutePath`. The only
TypeScript additions across these eight components are readonly `publicOffersPath` properties
and, where the existing page did not already link to Sign up, readonly `signUpPath` properties;
no authentication/API/workflow logic changed and no route was created. Both are pre-existing public
destinations surfaced in approved Stitch auth headers. Brand remains non-navigating because
the root/default-entry policy is frozen. AUTH-009 inherits the same header as AUTH-008.

Batch 2 boundary evidence (current working snapshot, before independent verifier):
`npm test -- --watch=false --include='src/app/features/auth/**/*.spec.ts'` — 11 files,
75/75 tests; `npm test -- --watch=false` — 98 files, 954/954 tests; `npm run lint`,
`npm run lint:styles`, and `npm run check:dependencies` passed; `npm run build` passed,
initial 448.74 kB without budget warning. Browser inspection of all eight routes at 390px,
1280px and then 320px found one visible h1 per route, no document overflow, and the mounted
anonymous catalog link at the expected canonical path; at 390px with `dir="rtl"`, no document
overflow across all eight routes. The observed pages used existing routes only; no authenticated
data, token, role, or permission was displayed. The browser session had zero console errors.

| Page ID | Page | Route | Actor | Stitch Artifact | Alignment | UI Logic Gaps | Verification |
|---------|------|-------|-------|-----------------|-----------|---------------|--------------|
| APP-SHELL | Authenticated App Shell | Shell-level (wraps authenticated routes) | Nurse / Employer / Admin | `projects/17116545761229201855/screens/764a9361e16948b0be831e4bf28f584b` (App Shell v3, HUMAN_APPROVED_AUTHENTICATED_VISUAL_BASELINE) | Aligned with reported logic gaps | 2 | Focused shell specs 15/15; full suite 98 files 944/944 (final state); build 448.74 kB, no budget warning |
| AUTH-001 | Sign in | `/auth/sign-in` | Public | `projects/17116545761229201855/screens/6427cabcd8b549eb845c85d6b8a0413a` | Partial alignment: transport/visual field conflict | 1 | Focused spec 16/16; stylelint passed; batch verifier pending |
| AUTH-002 | Sign up | `/auth/sign-up` | Public | `projects/17116545761229201855/screens/300823b7306f4924a947eb3a80848e63` | Partial alignment: required confirm-password field | 1 | Focused spec 6/6; stylelint passed; batch verifier pending |
| AUTH-005 | Check Email | `/auth/verify-email` | Public | `projects/17116545761229201855/screens/265d92edd39248269e867d6d72d44962` | Partial alignment: delivery-certainty copy conflict | 0 | Focused spec 7/7; stylelint passed; batch verifier pending |
| AUTH-006 | Verify Email | `/auth/verify-email/confirm` | Public | `projects/17116545761229201855/screens/76bae8f5c9a94284a919bf81b9c8e15c` | Partial alignment: illustrative state selector omitted | 1 | Focused spec 10/10; stylelint passed; batch verifier pending |
| AUTH-007 | Forgot Password | `/auth/forgot-password` | Public | `projects/17116545761229201855/screens/55522993d9514ed9ac67586fb552cb1a` | Partial alignment: illustrative state selector omitted | 1 | Focused spec 10/10; stylelint passed; batch verifier pending |
| AUTH-008 | Reset Password | `/auth/reset-password` | Public | `projects/17116545761229201855/screens/5782187204dc4229a5970ddf2cc0012f` | Partial alignment: illustrative state gallery omitted | 1 | Focused spec 15/15; stylelint passed; batch verifier pending |
| AUTH-009 | Reset Password Success (state) | `/auth/reset-password` (non-routable state) | Public | `projects/17116545761229201855/screens/ef4d555b00d54f68870318e12d251780` | Partial alignment: form retained after success | 0 | Focused reset spec 15/15; no new route |
| AUTH-010 | Session Expired | `/session-expired` | Public | `projects/17116545761229201855/screens/43b0f224781949ad8be651df1991a257` | Aligned with reported logic gap | 1 | Focused terminal specs 8/8; stylelint passed; batch verifier pending |
| AUTH-011 | Access Denied | `/access-denied` | Public | `projects/17116545761229201855/screens/e869c0c2568444b5bc2638036f5fa8c0` | Aligned with reported logic gaps | 2 | Focused terminal specs 8/8; stylelint passed; batch verifier pending |

---

# APP-SHELL — Authenticated App Shell

## 1. Identity

- Page ID: APP-SHELL
- Page name: Authenticated App Shell
- Actor/user: Nurse / Employer / Admin (actor-aware `navigationItems` input)
- Canonical route: Shell-level artifact; no route behavior of its own. Wraps mounted authenticated routes.
- Actual Angular route: n/a (shell component rendered by `app.html` around `<router-outlet>`)
- Angular component path: `frontend/src/app/core/shell/authenticated-app-shell.ts` (+ `.html`, `.scss`,
  `.spec.ts`); navigation items from `frontend/src/app/core/shell/primary-navigation.ts`
- Accepted Stitch artifact ID: `projects/17116545761229201855/screens/764a9361e16948b0be831e4bf28f584b`
  ("Shell / APP-SHELL / Nurse / Desktop v3 — Human Review Candidate")
- Stitch artifact status: HUMAN_APPROVED_AUTHENTICATED_VISUAL_BASELINE (authenticated chrome only;
  representative Exams content inside the artifact is explicitly non-authoritative and was ignored)

## 2. Page Goal

Per current product authority, the shell provides persistent authenticated chrome: brand/home affordance,
actor-aware primary navigation with active-state, neutral account access, sign-out, skip link, route-level
loading presentation, and an accessible mobile navigation drawer. Navigation visibility is eligibility-only;
router guards remain security authority.

## 3. Refactor Goal

Align the implemented shell chrome with approved App Shell v3: brand mark + wordmark, icon + label primary
navigation (Exams, Preparation Packages, Products, Profile), account dropdown menu (Account / Sign out),
skip link, mobile drawer parity, token-based calm surfaces, 44px targets, logical (RTL-safe) CSS.

## 4. Goals Achieved

- Verified the implemented shell already matches approved chrome for: top application bar composition,
  text wordmark brand/home link, actor-aware text-label primary navigation, active state via border +
  background + `aria-current` (not color alone), direct Account and Sign out actions, skip link to
  `#main-content`, mobile drawer with focus trap, Escape handling, focus entry/return, backdrop/scrim,
  header `inert` while open, close-on-route-change, token-driven calm surfaces, 44px targets, logical CSS
  properties, 600px breakpoint drawer switch.
- Evaluated every v3 delta; applied none as code (see §14 for measured/documented reasons).
- No source change required; no behavior changed.

## 5. Complete Existing Feature Behavior

Verified operational (unchanged): brand/home navigation; actor-aware primary navigation rendering with
single `aria-current`; account navigation to `ACCOUNT_OVERVIEW`; sign-out through `LocalLogout` + navigation
to `AUTH_SIGN_IN`; mobile drawer open/close/Escape/focus-trap/focus-return/inert-backdrop/route-change-close;
skip link; route-level loading presentation via `app.html`. All covered by existing specs (15 shell tests).

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|-----------|-------------|--------------------|-----------------|-------------|
| Brand/home | `/` | Yes, unchanged | Canonical routes (`'/'` home affordance) | `AuthenticatedAppShell` template |
| Primary nav item | `item.path` (actor-aware) | Yes, unchanged | `primary-navigation.ts` + canonical routes | `getPrimaryNavigationItems` |
| Account | `ACCOUNT_OVERVIEW` path | Yes, unchanged | `canonicalRoutePath('ACCOUNT_OVERVIEW')` | `AuthenticatedAppShell.accountPath` |
| Sign out | `AUTH_SIGN_IN` after local logout | Yes, unchanged | `canonicalRoutePath('AUTH_SIGN_IN')` | `AuthenticatedAppShell.signOut` via `LocalLogout` |
| Mobile drawer links | Same destinations | Yes, unchanged | Same as desktop | Same owners |

No route behavior was created, changed, or removed by this GOAL.

## 7. Stitch → Angular Visual Changes

None applied. Implementation inspection found the current shell already expresses the approved chrome with
project tokens (`--np-color-brand-1 #006B66`, `--np-color-brand-2 #173B57`,
`--np-color-active-surface #EEF7F5`, 4px spacing scale, 44px targets, `rgb(16 42 58 / 0.48)` scrim).
A CDK Menu account-dropdown probe was implemented, measured, and reverted (see §14, deviation 3).

## 8. TypeScript / Logic Mapping

| Stitch element | Classification | Owner / evidence |
|---|---|---|
| Primary navigation items | EXISTING_LOGIC_REUSED | `getPrimaryNavigationItems` (`primary-navigation.ts`); labels/routes/active-family unchanged |
| Brand/home affordance | EXISTING_LOGIC_REUSED | Shell template `routerLink="/"` |
| Account action | EXISTING_LOGIC_REUSED | `accountPath` → `ACCOUNT_OVERVIEW` |
| Sign out action | EXISTING_LOGIC_REUSED | `signOut()` → `LocalLogout` + `AUTH_SIGN_IN` |
| Mobile drawer open/close/Escape/focus | EXISTING_LOGIC_REUSED | Shell component signals + `CdkTrapFocus`; specs unchanged |
| Notifications icon (DPF-001) | UI_LOGIC_GAP — VISUAL_ONLY | No TypeScript/route/service/API authority; not rendered (see §9) |
| Help icon (DPF-002) | UI_LOGIC_GAP — VISUAL_ONLY | No TypeScript/route/service/API authority; not rendered (see §9) |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Notifications icon (DPF-001) | Top bar, after primary nav | Utility affordance | NO (not rendered) | NO | NO | Intentionally not rendered: established shell spec (`authenticated-app-shell.spec.ts`) asserts absence of DPF controls, and the DPF register (`design-proposed-features.md`) denies implementation authority to DPF-001. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |
| Help icon (DPF-002) | Top bar, after notifications | Utility affordance | NO (not rendered) | NO | NO | Same rationale as DPF-001; DPF-002 remains design-proposed only. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

UI "Notifications" and UI "Help" in page "Authenticated App Shell" exist in the approved Stitch design and
were deliberately not rendered as controls because current TypeScript/component/route/facade/service/backend
authority plus the established shell spec forbid DPF controls, and the DPF register grants them no
implementation authority. No behavior was invented. (Deviation from the render-visually default in GOAL
`STITCH-UI-REFACTOR-2026-09-27` §13 is documented here and in §14, deviation 4.)

## 10. Existing Functionality Missing From Stitch

None. All established shell behavior (navigation eligibility, account, sign-out, drawer, skip link, loading
presentation) is representable within approved chrome; nothing was removed.

## 11. Contracts Preserved

- Business rules: unchanged (shell owns none).
- API behavior / backend truth: untouched (no API-adjacent code in batch).
- Auth/authentication: unchanged (`LocalLogout` flow intact, spec-verified).
- Authorization/permissions/ownership: unchanged (navigation eligibility pipeline untouched).
- Route identity/guards/navigation behavior: unchanged (§6).
- Validation: n/a (no forms in shell).
- Sensitive-data rules: unchanged (no identity details rendered; spec asserts absence).
- Payment/entitlement/exam truth: n/a, untouched.

## 12. Files Changed

- `docs/frontend/design/stitch/STITCH_UI_REFACTOR_REPORT.md` (new; this file, APP-SHELL section only).
- No production source, test, style, route, or config file changed (CDK Menu probe fully reverted;
  `git diff` on `frontend/src/app/core/shell/` is empty).

## 13. Verification

- Focused shell specs: final reverted state 15/15 passed (`npm test -- --watch=false
  --include='src/app/core/shell/*.spec.ts'`); menu-probe cycle reached 17/17 before revert.
- Full frontend unit suite on final state: 98 files, 944/944 passed (`npm test -- --watch=false`).
  (Menu-probe cycle measured 946/946 with 2 additional probe tests before revert.)
- ESLint (`npm run lint`): all files pass.
- Stylelint on shell SCSS: clean after one shorthand-order fix (probe cycle).
- Production build: reverted state measured at baseline worktree build — initial total 448.74 kB,
  no budget warning (`maximumWarning 500kB`, `maximumError 1MB`). Probe build measured 524.37 kB
  (new 500kB warning) and was rejected (see §14).
- Baseline comparison build performed in a disposable worktree (`/tmp/opencode/shell-baseline`, removed
  afterwards) with symlinked `node_modules`; no installs, no repo mutation.
- Verifier packet/result at batch boundary: see `PACKET_ID PKT-STITCH-UI-REFACTOR-B1` (pending).

## 14. Visual Deviations From Stitch

1. Text wordmark instead of shield mark + wordmark. Reason: no established project icon mechanism exists
   (no icon font, no SVG precedent, `mat-icon` unused in production); introducing a webfont/SVG icon system
   is a new global mechanism beyond this visual GOAL. Wordmark preserves approved branding text.
2. Text-only primary navigation labels instead of icon + label pairs. Reason: same as (1); DESIGN.md
   normative rule ("Primary navigation keeps visible text labels") is satisfied.
3. Direct desktop Account link + Sign out button instead of an account dropdown menu. Reason: implemented
   CDK Menu probe and measured initial bundle 448.74 kB → 524.37 kB (+75.6 kB raw, +15.7 kB transfer),
   introducing a new 500kB-budget warning on a previously clean baseline. Identical destinations and
   behavior are preserved with zero bundle cost, consistent with the mobile drawer's direct actions.
   Probe fully reverted; no residual change.
4. DPF-001/DPF-002 icons not rendered (see §9). Reason: established shell spec forbids DPF controls and the
   DPF register grants no implementation authority; rendering operable-looking icons would imply
   functionality and contradict test authority.
5. Skip link text "Skip to content" vs Stitch "Skip to main content". Reason: equivalent behavior and target
   semantics (`#main-content`); copy churn unjustified.

## 15. Remaining Gaps

- None for APP-SHELL. (Icon mechanism, if ever approved as its own scoped initiative, could revisit
  deviations 1–2; explicitly out of this GOAL.)

## 16. Page Verdict

STITCH_ALIGNED_WITH_REPORTED_LOGIC_GAPS

---

# AUTH-001 — Sign In

## 1. Identity

- Page ID/name/actor: AUTH-001, Sign in, public/unauthenticated.
- Canonical and actual route: `AUTH_SIGN_IN`, `/auth/sign-in` (unchanged).
- Component: `frontend/src/app/features/auth/sign-in/sign-in.ts` (external `.html` / `.scss`).
- Accepted artifact: `projects/17116545761229201855/screens/6427cabcd8b549eb845c85d6b8a0413a`, `HUMAN_APPROVED_VISUAL_REFERENCE`.

## 2. Page/State Goal

Authenticate using the existing email/password login request, hydrate the current user, and navigate to a safe existing return URL or account overview. Present validation, generic backend failure, and email-verification-required states without exposing tokens.

## 3. Refactor Goal

Translate the accepted centered white auth card, calm public header, headline/subtitle, recovery link adjacent to the password field, primary submit, and separated account-creation row into the existing Angular form without altering auth transport or route behavior.

## 4. Goals Achieved

Removed the unrelated decorative two-column marketing panel and gradient. Added the simple public header, centered bordered single-column card, subtitle, password-adjacent recovery link and divider before account-creation copy; kept the Material controls and token-based button. A submitting state now uses the existing `isSubmitting()` signal for button text.

## 5. Complete Existing Feature Behavior

Focused tests verify required fields/validation, exact `LoginCommand` email/password payload, session bootstrap before current-user hydration, safe `returnUrl` with account fallback, public route, failure/error handling, email-verification-required message, no role redirect or token exposure, autocomplete, and recovery links. Auth workflow and API code are unchanged; a canonical public catalog path property was added for the approved header.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Sign in | Safe `returnUrl` or `/account` | Yes; unchanged | `isSafeReturnUrl`, `ACCOUNT_OVERVIEW` | `SignIn.submit()` |
| Forgot password? | `/auth/forgot-password` | Yes; unchanged | `AUTH_FORGOT_PASSWORD` | `SignIn.forgotPasswordPath` |
| Create an account / header Sign up | `/auth/sign-up` | Existing destination; header shares the already-authorized link | `AUTH_SIGN_UP` | `SignIn.signUpPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `SignIn.publicOffersPath` |
| Header Sign in | Current page label; no new navigation | No action | `AUTH_SIGN_IN` | Presentational current-page text |
| Header brand | Visible wordmark; no new route | No action | Root/default policy frozen | Presentational brand text |

## 7. Stitch → Angular Visual Changes

Single-column centered 448px card and neutral background replace marketing art; white 64px public bar; navy heading, secondary subtitle, `#C9DDDA` border, approved 16px card radius, 12px actions, approved 4px spacing scale and responsive gutter token. Logical CSS properties keep future RTL reflow. Error and validation presentations remain conditional and retain their accessible announcements rather than showing Stitch's illustrative simultaneous error and verification banners.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Email address and Password fields, Sign in, submitting label | EXISTING_LOGIC_VISUALLY_REFACTORED | `SignIn.form`, `submit()`, existing `NpTextInputControl` / `AuthTransport` |
| Forgot password, Create account, header Sign up | EXISTING_LOGIC_REUSED | Existing canonical paths and `RouterLink` |
| Header brand, current-page Sign in label, subtitle | PRESENTATIONAL_ONLY | Template/SCSS |
| Password-visibility icon shown by Stitch | UI_LOGIC_GAP — VISUAL_ONLY | See §9; static decorative SVG |
| Email-or-username form field displayed by Stitch | STITCH_FUNCTIONALITY_GAP | Existing transport only sends `email`; see §§10,14 |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Password visibility eye | Password input trailing edge | Visual indication of password field | YES — decorative, `aria-hidden`, non-focusable, no event/route binding | NO | NO | Rendered an inline SVG beside the existing shared Material password field; password remains masked. Requires authorized accessible local visibility control to become functional; TypeScript: NONE, route: NONE, service/facade: NONE, API/backend: NONE. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

UI “Password visibility” in AUTH-001 exists in Stitch and was implemented visually as a non-operational icon, but it has no authoritative functionality in the current TypeScript/component/route/facade/service/backend contract. No behavior was invented.

## 10. Existing Functionality Missing From Stitch

No existing functional action was removed. The real field is email-only (`LoginCommand.email`); Stitch's “Email or username” label promises broader input than the implemented transport supports. This visual/functional mismatch is preserved as an exact deviation, not turned into new auth logic. Existing server-backed validation summary and safe error semantics are retained even when Stitch illustrates static alerts.

## 11. Contracts Preserved

Exact login payload, token/session ownership, current-user hydration, safe return URL and fallback, backend Problem Details, required-field validation, route access, email-verification-required handling, authentication/authorization, and sensitive-field secrecy are unchanged.

## 12. Files Changed

`frontend/src/app/features/auth/sign-in/sign-in.ts` (public route-path property only), `sign-in.html`, `sign-in.scss`, `sign-in.spec.ts`; this report. No auth workflow logic, route, generated client, backend, or OpenAPI change.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/sign-in/sign-in.spec.ts'` — 16/16 passed (visual-structure and decorative-eye tests red before implementation); `npm run lint:styles` — passed. Scoped lint, production build, browser/responsive verification and verifier pending batch boundary.

## 14. Visual Deviations From Stitch

- “Email or username” remains “Email address”: current `LoginCommand.email` and frontend login validation are email-only; no username support inferred from artwork. This is an affected visual decision requiring explicit auth/UX authority for exact copy equivalence.
- The password eye is a non-operational presentational SVG, not Stitch's working toggle button; the existing password input remains masked (see §9).
- Illustrative simultaneous error and verification alerts become truthful conditional states from the existing Problem Details mapping. Placeholder/institutional-email helper, spinner and separate disabled button are not fabricated as real states; existing submit button changes label and disabled state based on actual submission.
- Stitch brand `href="#"` becomes non-interactive wordmark; root/default-route behavior is frozen. The public header includes existing Sign up, public Preparation Packages, and current-page Sign in.

## 15. Remaining Gaps

Visual field-label decision is HUMAN_DECISION_REQUIRED for exact copy parity; functional password-eye behavior requires separate authority. Browser responsive/RTL review completed (batch evidence above); verifier review pending.

## 16. Page Verdict

PARTIAL_ALIGNMENT (bounded AUTH-001 visual decisions described above; functional contract preserved).

---

# AUTH-002 — Sign Up

## 1. Identity

- AUTH-002, Sign up, public; canonical and actual route `AUTH_SIGN_UP` `/auth/sign-up`.
- `frontend/src/app/features/auth/sign-up/sign-up.ts` with external `.html`/`.scss`.
- `projects/17116545761229201855/screens/300823b7306f4924a947eb3a80848e63`, `HUMAN_APPROVED_VISUAL_REFERENCE`.

## 2. Page/State Goal

Submit the existing public registration request, handle validation/error state, then navigate to Check Email without creating a session or choosing a role.

## 3. Refactor Goal

Apply the approved centered account card and neutral public header/typography/field rhythm while preserving the real fourth confirmation field omitted by Stitch.

## 4. Goals Achieved

Added public header with Sign in/current Sign up, centered white card, Stitch title/subtitle, approved password-rule helper text, neutral page canvas and token spacing. Removed unsupported role-assignment marketing claim and heavy shadow. Existing Material form controls and validation retained.

## 5. Complete Existing Feature Behavior

Focused specs verify four fields, autocomplete, confirm-password mismatch rejection, exact public registration payload (email/username/password only), and Check Email navigation. Form, errors, and auth service are untouched.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Create account | `/auth/verify-email` after accepted submit | Yes, unchanged | `AUTH_VERIFY_EMAIL_REQUEST` | `SignUp.submit()` |
| Sign in (header and footer) | `/auth/sign-in` | Existing route reused | `AUTH_SIGN_IN` | `SignUp.signInPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `SignUp.publicOffersPath` |
| Sign up (header) | Current page only, no new navigation | n/a | `AUTH_SIGN_UP` | Presentational text |
| Brand | Static wordmark, no root redirect | n/a | Root freeze | Presentational text |

## 7. Stitch → Angular Visual Changes

New 64px public bar, centered 448px card, light approved background, navy heading, teal Material submit, white surface with approved border/radius; 16px mobile and approved tablet gutters, logical CSS for RTL. The server-driven error summary remains conditional rather than rendering illustrative banners simultaneously.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Email, Username, Password, Create account | EXISTING_LOGIC_VISUALLY_REFACTORED | `SignUp.form`, `submit()`, `SignUpApi` |
| Sign in links | EXISTING_LOGIC_REUSED | `SignUp.signInPath` |
| Confirm password | STITCH_FUNCTIONALITY_GAP | Existing validation in `SignUp.form` and `clientValidationFailure()` |
| Password eye visual | UI_LOGIC_GAP — VISUAL_ONLY | Decorative SVG only; no control behavior |
| Brand, current-page Sign up, description and approved rule helper | PRESENTATIONAL_ONLY | Angular template |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Password visibility eye | Password input trailing edge | Visual indication of password field | YES — decorative, `aria-hidden`, non-focusable SVG | NO | NO | Rendered as non-operational SVG; masked input remains. TypeScript/route/service/facade/API/backend authority: NONE. Accessible reveal-control semantics require separate authority and a shared-control slot. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

UI “Password visibility” in AUTH-002 exists in Stitch and was implemented visually, but it has no authoritative functionality in the current TypeScript/component/route/facade/service/backend contract. No behavior was invented.

## 10. Existing Functionality Missing From Stitch

**STITCH_FUNCTIONALITY_GAP:** Confirm password is a required existing form control and must be retained. It remains in the same form stack with matching field styling. Exact Stitch three-field composition is intentionally extended by one real field.

## 11. Contracts Preserved

Public sign-up payload and 202 acceptance handoff, password/confirm validation, backend-safe errors, no role selector or automatic login, canonical navigation, auth/session and sensitive-field protections unchanged. No backend/API/DTO/generated client edits.

## 12. Files Changed

`frontend/src/app/features/auth/sign-up/sign-up.ts` (public route-path property only), `sign-up.html`, `sign-up.scss`, `sign-up.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/sign-up/sign-up.spec.ts'`: 6/6 pass (new structure and decorative-eye tests failed before implementation); `npm run lint:styles`: pass before the final decorative-eye addition; rerun at batch boundary. Batch regression, browser, production build, verifier pending.

## 14. Visual Deviations From Stitch

Required Confirm password retained (§10); no illustrative simultaneously visible failure/verification banners; no fake response state or root-brand link. Public package nav reuses the mounted catalog route. Password eye is non-operational rather than Stitch's working toggle; current masked field remains. Exact body wording is kept neutral and consistent with current backend truth.

## 15. Remaining Gaps

Functional password-eye behavior, plus batch-wide responsive/browser checks and verifier review remain. No new auth behavior authorized.

## 16. Page Verdict

PARTIAL_ALIGNMENT (real required field is absent from Stitch and preserved).

---

# AUTH-005 — Check Email

## 1. Identity

- AUTH-005; Check Email; public; `AUTH_VERIFY_EMAIL_REQUEST` `/auth/verify-email`; `frontend/src/app/features/auth/check-email/check-email.ts` with external `.html`/`.scss`.
- Accepted artifact `projects/17116545761229201855/screens/265d92edd39248269e867d6d72d44962`; `HUMAN_APPROVED_VISUAL_REFERENCE`.

## 2. Page/State Goal

Show a generic accepted-registration verification instruction without claiming delivery for any specific account; offer existing Sign in navigation only. No resend, countdown, token inspection or extra backend call.

## 3. Refactor Goal

Use the accepted centered public confirmation card, simple auth header, envelope icon and prominent Sign in action, subject to the existing enumeration-safe copy contract.

## 4. Goals Achieved

Removed the previous two-column marketing graphic and gradient; added a calm white bordered centered card, public wordmark/sign-in header, decorative envelope and teal sign-in action. Existing generic instructional wording remains.

## 5. Complete Existing Feature Behavior

Focused specs verify conditional generic copy (“If registration was successful”), canonical sign-in link, no resend/countdown/delivery guarantee, public route activation, preservation of verify-confirmation route, and absence of backend/token/session handling. All remain unchanged.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Card `Go to sign in`, header `Sign in` | `/auth/sign-in` | Existing destination reused | `AUTH_SIGN_IN` | `CheckEmail.signInPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `CheckEmail.publicOffersPath` |
| Header brand | No navigation | No | Root/default freeze | Presentational template |

## 7. Stitch → Angular Visual Changes

Applied public-header/card geometry, neutral approved canvas, teal envelope treatment, centered text and 48px sign-in visual button using existing border/spacing/radius tokens and logical CSS. Heading and copy remain safe and readable at mobile widths.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Sign in header/card links | EXISTING_LOGIC_VISUALLY_REFACTORED | Existing `signInPath` / `RouterLink` |
| Brand and envelope | PRESENTATIONAL_ONLY | Template/SCSS |
| “We sent a link” status | PRESENTATIONAL_ONLY, rejected literal claim | No delivery-confirmation authority; generic instruction retained |

## 9. UI Without Logic

No `UI_LOGIC_GAP — VISUAL_ONLY` was found. The catalog header link reuses the already mounted anonymous route; no fake control, resend or API behavior was created.

## 10. Existing Functionality Missing From Stitch

No existing action removed. The conditional, enumeration-safe wording of the implemented page differs from Stitch's unconditional delivery claim; it remains in place to preserve truthful backend behavior.

## 11. Contracts Preserved

Public route and sign-in destination, conditional copy, zero token/backend use, no resend or account-existence disclosure, authentication/security and privacy semantics unchanged.

## 12. Files Changed

`frontend/src/app/features/auth/check-email/check-email.ts` (public route-path property only), `check-email.html`, `check-email.scss`, `check-email.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/check-email/check-email.spec.ts'`: 7/7 passed (new layout test first failed); `npm run lint:styles`: passed. Browser and verifier pending batch boundary.

## 14. Visual Deviations From Stitch

“Verification link sent / We sent” banner omitted: an unconditional delivery claim conflicts with the existing enumeration-safe “If registration was successful” text. Retained old explanatory copy rather than falsely showing a confirmed email address or delivery. Header uses existing Sign in and public catalog destinations, with wordmark non-interactive because root policy is frozen.

## 15. Remaining Gaps

Exact delivery-status copy requires backend/product authority; responsive/browser and batch verifier review pending.

## 16. Page Verdict

PARTIAL_ALIGNMENT (content-safety deviation preserved).

---

# AUTH-006 — Verify Email

## 1. Identity

- AUTH-006; Verify Email; public token-query route `AUTH_VERIFY_EMAIL_CONFIRM` `/auth/verify-email/confirm`; component `frontend/src/app/features/auth/verify-email/verify-email.ts` with external `.html`/`.scss`.
- Artifact `projects/17116545761229201855/screens/76bae8f5c9a94284a919bf81b9c8e15c`, `HUMAN_APPROVED_VISUAL_REFERENCE`.

## 2. Page/State Goal

Verify an opaque query token through the existing API and present exactly one truthful current state (missing, loading, success, error) with safe sign-in exit. Never display token or pretend an arbitrary status was returned.

## 3. Refactor Goal

Use the approved public top bar, calm centered status card, navy heading and visually distinct state banners/teal recovery CTA while retaining the actual backend-driven state machine.

## 4. Goals Achieved

Removed marketing graphic/gradient; added compact public header and centered white card, subtitle, token-driven status styling, and clear sign-in recovery actions. Existing observable states and API logic untouched.

## 5. Complete Existing Feature Behavior

Focused spec 10/10 verifies opaque token never rendered, missing/empty token blocks API, success calls exact verify request, backend failure remains safe, correct Sign in recovery labels and canonical route. No API/session/navigation logic changed.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Continue/Back to sign in | `/auth/sign-in` | Yes; unchanged | `AUTH_SIGN_IN` | `VerifyEmail.signInPath` |
| Header Sign in | `/auth/sign-in` | Existing canonical destination reused | `AUTH_SIGN_IN` | Same path |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `VerifyEmail.publicOffersPath` |
| Brand | Non-navigating | No | Root/default freeze | Presentational text |

## 7. Stitch → Angular Visual Changes

Single centered 448px border card, 64px public bar, white surface, approved light canvas, responsive logical CSS and calm token-based state banner styles replace decorative two-column panel. No simultaneous success/failure/example states.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Loading, success, missing-token, backend-failure banners | EXISTING_LOGIC_VISUALLY_REFACTORED | `VerifyEmail` signals + API + Problem Details |
| Sign in links | EXISTING_LOGIC_REUSED | `signInPath` / `RouterLink` |
| Public brand and subtitle | PRESENTATIONAL_ONLY | Angular template |
| Stitch state tabs (Loading / Success / Missing Link / Expired or Invalid / Temporary Error) | UI_LOGIC_GAP — VISUAL_ONLY | Design-preview selector has no runtime authority |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| State-preview tabs | Above example states | Switch illustrated state variants | NO — a fake switcher would falsify verification truth | NO | NO | Kept real status card only. TypeScript preview state: NONE; route/service/facade/API/backend authority: NONE. A separately approved design-preview tooling state would be needed. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

Stitch's state-preview tabs are not a production workflow element; representing them as working tabs would let visitors manufacture a false verified state. This conflicts with auth truth, so the state selector is not rendered. No behavior was invented.

## 10. Existing Functionality Missing From Stitch

No existing action removed. Actual missing-token short-circuit, opaque-token API verification, state exclusivity, error detail mapping, and established recovery labels remain even though Stitch depicts a static gallery of multiple states.

## 11. Contracts Preserved

Verify-email request and token secrecy, route/query semantics, loading/success/failure/invalid handling, Problem Details, safe Sign in navigation and public access unchanged. Only a public route-path property was added to TS; no auth workflow, generated client, backend or OpenAPI changes.

## 12. Files Changed

`frontend/src/app/features/auth/verify-email/verify-email.ts` (public route-path property only), `verify-email.html`, `verify-email.scss`, `verify-email.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/verify-email/verify-email.spec.ts'`: 10/10 passed (new visual-structure test failed before implementation); `npm run lint:styles`: passed. Batch browser/RTL and verifier pending.

## 14. Visual Deviations From Stitch

State gallery/tabs omitted to prevent fabricated verification states; only the actual resolved state renders. No invented Try again action; existing safe sign-in recovery retained. Stitch's literal status copy is not substituted for backend-safe existing wording. Header uses existing Sign in and public catalog destinations; no root-brand link.

## 15. Remaining Gaps

The illustrative state-selector design decision is BLOCKED_CONTRACT_CONFLICT with auth truth; browser responsive/RTL review completed (batch evidence above); verifier review pending.

## 16. Page Verdict

PARTIAL_ALIGNMENT (illustrative multi-state gallery conflicts with runtime truth).

---

# AUTH-007 — Forgot Password

## 1. Identity

- AUTH-007; Forgot Password; public route `AUTH_FORGOT_PASSWORD` `/auth/forgot-password`; component `frontend/src/app/features/auth/forgot-password/forgot-password.ts` with external `.html`/`.scss`.
- Artifact `projects/17116545761229201855/screens/55522993d9514ed9ac67586fb552cb1a`, `HUMAN_APPROVED_VISUAL_REFERENCE`.

## 2. Page/State Goal

Request a password-reset email using existing API with a single email input and enumeration-safe accepted response. Preserve validation, submitting, backend error, and safe sign-in exit.

## 3. Refactor Goal

Apply the accepted centered public-header/card geometry and calm form presentation, keeping actual backend-driven state exclusivity and neutral success text.

## 4. Goals Achieved

Replaced two-column decorative panel/gradient with white bordered centered card and public wordmark/sign-in header; retained email field, validation summary, real success/error status, primary submit and Back to sign in.

## 5. Complete Existing Feature Behavior

Focused spec 10/10 verifies required/email-format validation, exact request payload, enumeration-safe status, backend failure handling, public route and canonical sign-in exit; no TypeScript/API change.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Send reset link | No route change; existing request | Yes | `AUTH_FORGOT_PASSWORD` | `ForgotPassword.submit()` / `ForgotPasswordApi` |
| Back to sign in / header Sign in | `/auth/sign-in` | Yes; reused | `AUTH_SIGN_IN` | `ForgotPassword.signInPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `ForgotPassword.publicOffersPath` |
| Brand | Static, no route | No | Root freeze | Presentational template |

## 7. Stitch → Angular Visual Changes

Single centered 448px white card, 64px top bar, approved border/radius/neutral background and responsive logical spacing replace heavy-shadow gradient layout. Conditional real status messaging remains in its original location rather than rendering mock simultaneous states.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Email field, Send reset link, validation/status | EXISTING_LOGIC_VISUALLY_REFACTORED | `ForgotPassword.form`, `submit()`, `ForgotPasswordApi` |
| Sign in links | EXISTING_LOGIC_REUSED | `signInPath` |
| Brand, instructional heading | PRESENTATIONAL_ONLY | Template |
| Stitch preview state tabs | UI_LOGIC_GAP — VISUAL_ONLY | No runtime state selector contract |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Initial / Validation error / Submitting / Accepted / Backend error tabs | Above card | Design-preview state switcher | NO — preview-only and would falsify request state | NO | NO | Only actual backend-driven state is shown. TypeScript/route/facade/service/API preview-state authority: NONE. Future functionality would need explicit non-production preview/tooling authority. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

## 10. Existing Functionality Missing From Stitch

No working action removed; current enumeration-safe success wording, concrete validation summary and actual error mapping are retained even where the artifact illustrates copy/state examples.

## 11. Contracts Preserved

Exact reset-request payload, validation, enumeration safety, API/error truth, public route, sign-in destination, token/account privacy unchanged. No backend/API/generated client edits.

## 12. Files Changed

`frontend/src/app/features/auth/forgot-password/forgot-password.ts` (public route-path property only), `forgot-password.html`, `forgot-password.scss`, `forgot-password.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/forgot-password/forgot-password.spec.ts'`: 10/10 pass (layout test red before implementation); `npm run lint:styles`: pass. Batch browser/build/verifier pending.

## 14. Visual Deviations From Stitch

Stitch preview-state tabs and simultaneous accepted/error panels are omitted to preserve actual state truth; generic accepted copy is maintained instead of any guaranteed email delivery. Brand remains a non-navigating wordmark; public catalog nav uses the existing route.

## 15. Remaining Gaps

Preview-state switcher design decision is BLOCKED_CONTRACT_CONFLICT with request truth; browser/responsive review completed (batch evidence above); verifier pending.

## 16. Page Verdict

PARTIAL_ALIGNMENT (preview controls conflict with runtime request truth).

---

# AUTH-008 — Reset Password

## 1. Identity

- AUTH-008; Reset Password; public `AUTH_RESET_PASSWORD` `/auth/reset-password` (opaque token query); `frontend/src/app/features/auth/reset-password/reset-password.ts` with external `.html`/`.scss`.
- Artifact `projects/17116545761229201855/screens/5782187204dc4229a5970ddf2cc0012f`, `HUMAN_APPROVED_VISUAL_REFERENCE`.

## 2. Page/State Goal

Use an opaque reset token and email/new password to submit the existing reset request; show validation/error/missing-token/accepted success, then offer safe sign-in. AUTH-009 is the same component's non-routable success state.

## 3. Refactor Goal

Apply approved calm public header, centered card and two-field arrangement, password-rule helper and real conditional status presentation without changing the reset transaction or route.

## 4. Goals Achieved

Removed decorative gradient/two-column panel; centered bordered form card, added public wordmark/sign-in bar and password guidance consistent with existing validation, styled actual success as a distinct icon/status panel. No token or API handling changed.

## 5. Complete Existing Feature Behavior

Focused spec 15/15 verifies exact ResetPasswordRequest, token handling, email/password validation, missing token blocking, success state on same route, backend error and safe recovery. Current success flow keeps the original form and submit interaction; it was not silently removed.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Reset password | Same route; existing API call | Yes | `AUTH_RESET_PASSWORD` | `ResetPassword.submit()` |
| Request new reset link | `/auth/forgot-password` | Yes | `AUTH_FORGOT_PASSWORD` | `forgotPasswordPath` |
| Continue to sign in / header Sign in | `/auth/sign-in` | Yes; destination reused | `AUTH_SIGN_IN` | `signInPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `ResetPassword.publicOffersPath` |
| Brand | No route | n/a | Root freeze | Presentational wordmark |

## 7. Stitch → Angular Visual Changes

White centered 448px card, neutral canvas, 64px public bar, approved card/button geometry and 4px spacing rhythm; actual success icon, error/validation banners and form remain in logical document order with responsive gutters.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Email, New password, Reset password, validation/status | EXISTING_LOGIC_VISUALLY_REFACTORED | `ResetPassword.form`, `submit()`, `ResetPasswordApi` |
| Forgot password and Sign in | EXISTING_LOGIC_REUSED | Existing canonical paths |
| Brand, helper text, success icon | PRESENTATIONAL_ONLY | Template/SCSS; helper matches existing validation |
| State-preview tabs / example panels | UI_LOGIC_GAP — VISUAL_ONLY | No preview-state runtime contract |
| Form remaining in success state | STITCH_FUNCTIONALITY_GAP | Existing form remains usable in current component |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Initial / Validation error / Submitting / Success / Invalid / Missing / Retryable preview tabs | Above example cards | Design-preview state selector | NO (would falsify reset truth) | NO | NO | Rendered only actual existing runtime state. TypeScript/route/service/facade/API/backend preview authority: NONE. Future functionality would need separately approved preview tooling. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

## 10. Existing Functionality Missing From Stitch

STITCH_FUNCTIONALITY_GAP: The current Reset Password form remains rendered and available after a successful request; Stitch's isolated success screen does not show it. No existing field or submit capability was removed; visual parity of this residual form needs a separate UX decision if a success-only replacement is desired.

## 11. Contracts Preserved

Opaque-token secrecy, exact reset payload/validation, accepted success on existing route, backend-safe errors, sign-in/forgot links, auth and privacy semantics unchanged. No backend/API/DTO/generated-client change.

## 12. Files Changed

`frontend/src/app/features/auth/reset-password/reset-password.ts` (public route-path property only), `reset-password.html`, `reset-password.scss`, `reset-password.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/reset-password/reset-password.spec.ts'`: 15/15 pass (new structure test red before implementation); `npm run lint:styles`: pass. Browser and batch verifier pending.

## 14. Visual Deviations From Stitch

No fake state tabs or simultaneous state cards, no unauthorised Try again or extra route; actual conditional statuses replace demo content. Form stays visible after success to preserve existing behavior (§10). No root-brand link; public catalog nav reuses the mounted route. Password remains masked; no unsupported toggle behavior.

## 15. Remaining Gaps

State-preview selector design decision is BLOCKED_CONTRACT_CONFLICT with reset truth. Success-state residual form is a visual discrepancy marked HUMAN_DECISION_REQUIRED for a success-only replacement; browser/responsive review completed (batch evidence above); verifier pending.

## 16. Page Verdict

PARTIAL_ALIGNMENT (state gallery omitted and existing success-state form retained).

---

# AUTH-009 — Reset Password Success (non-routable state)

## 1. Identity

- AUTH-009; Reset Password Success; public, **non-routable transient state** within `AUTH_RESET_PASSWORD` `/auth/reset-password`.
- Component is the existing `frontend/src/app/features/auth/reset-password/reset-password.ts`, not a new component/route.
- Artifact `projects/17116545761229201855/screens/ef4d555b00d54f68870318e12d251780`; `GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW`.

## 2. Page/State Goal

Confirm a successful reset without exposing token/password data and direct the user to the existing Sign in route.

## 3. Refactor Goal

Apply the artifact's icon/status hierarchy and prominent Sign in presentation inside the existing AUTH-008 success branch, with no new route or altered reset behavior.

## 4. Goals Achieved

Success now uses a separate visually distinct icon/status panel within the centered card. Existing live-status semantics and safe Sign in link are retained.

## 5. Complete Existing Feature Behavior

Success appears only after the existing reset API resolves; status text remains `Password has been reset successfully.` (`role="status"`). Route remains `/auth/reset-password`; sign-in link still navigates to `/auth/sign-in` (focused reset spec 15/15). Existing form remains rendered afterward.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Continue to sign in | `/auth/sign-in` | Yes | `AUTH_SIGN_IN` | `ResetPassword.signInPath` |
| Success state itself | No route | Yes; same workflow | `AUTH-009` non-routable contract | `ResetPassword.successMessage` |

## 7. Stitch → Angular Visual Changes

Added teal-tinted success icon beside the actual success status; centered card and responsive token-based geometry are shared with AUTH-008. No second page or duplicate production component.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Success message and Sign in | EXISTING_LOGIC_VISUALLY_REFACTORED | `ResetPassword.successMessage` + `signInPath` |
| Success icon | PRESENTATIONAL_ONLY | Template/SCSS |
| Form remaining below success | STITCH_FUNCTIONALITY_GAP | Existing form/submission behavior |

## 9. UI Without Logic

No `UI_LOGIC_GAP — VISUAL_ONLY` was found in the AUTH-009 artifact; the Sign in control already has an authoritative route.

## 10. Existing Functionality Missing From Stitch

STITCH_FUNCTIONALITY_GAP: The existing form remains present after success, unlike the standalone Stitch success frame. It is preserved; removing/replacing it would change the current component's behavior and requires a separate UX/behavior decision.

## 11. Contracts Preserved

Exact reset success criterion, status announcement, token/password secrecy, canonical route and existing Sign in destination unchanged. No new route/component, backend, API, DTO or auth flow.

## 12. Files Changed

Same `reset-password.html`, `reset-password.scss`, `reset-password.spec.ts` as AUTH-008; this report. No independent AUTH-009 source file.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/reset-password/reset-password.spec.ts'`: 15/15 pass, including success-state and no-new-route assertions. Batch browser and verifier pending.

## 14. Visual Deviations From Stitch

The success frame is rendered as a conditional state of the existing route; the input form remains after success, and the exact existing backend-safe success copy is preserved. No standalone route or automatic redirect was created.

## 15. Remaining Gaps

Success-only replacement design would need an explicit decision on preserving/disabling post-success form behavior.

## 16. Page Verdict

PARTIAL_ALIGNMENT (existing same-route success form preserved).

---

# AUTH-010 — Session Expired

## 1. Identity

- AUTH-010; Session Expired; public terminal route `SYSTEM_SESSION_EXPIRED` `/session-expired`; `frontend/src/app/features/auth/session-expired/session-expired.ts` + external `.html`/`.scss`.
- Artifact `projects/17116545761229201855/screens/43b0f224781949ad8be651df1991a257`, `GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW`.

## 2. Page/State Goal

Present the explicit session-expired terminal state without exposing tokens or account/session diagnostics; offer the already-established Sign in recovery.

## 3. Refactor Goal

Adopt Stitch's calm public header, centered terminal card, compact status icon, body description and clear primary sign-in action without introducing root/default routing behavior.

## 4. Goals Achieved

Removed gradient/two-column art; added public brand bar, centered bordered card with status icon, neutral state description, teal Sign in CTA and visually present non-operational Go to home text.

## 5. Complete Existing Feature Behavior

Existing Sign in again link still uses `signInPath`, public terminal route stays unguarded, no auth/session/token/API dependency added. Focused terminal tests 8/8 including both screens.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Sign in again | `/auth/sign-in` | Yes, unchanged | `AUTH_SIGN_IN` | `SessionExpired.signInPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `SessionExpired.publicOffersPath` |
| Go to home appearance | None | No; visual only | Root behavior unresolved | No route/logic |
| Brand | None | No | Root freeze | Presentational wordmark |

## 7. Stitch → Angular Visual Changes

White center card, light approved canvas, teal status treatment, 64px header, approved border/radius and responsive logical gutters; preserved one `h1` and safe terminal copy.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Sign in again | EXISTING_LOGIC_VISUALLY_REFACTORED | `SessionExpired.signInPath` / `RouterLink` |
| Brand, clock icon, description | PRESENTATIONAL_ONLY | Template/SCSS |
| Go to home | UI_LOGIC_GAP — VISUAL_ONLY | No authorized root behavior |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Go to home | Below Sign in | Secondary destination appearance | YES — static visible text | NO | NO | No routerLink, href, handler, or fake success. TypeScript: NONE; route: unresolved root/default behavior; service/facade: NONE; API/backend: NONE. Future functionality requires human-authorized root-entry policy. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

UI “Go to home” in AUTH-010 exists in Stitch and was implemented visually, but has no authoritative current route/functionality; no behavior was invented.

## 10. Existing Functionality Missing From Stitch

None; Sign in recovery preserved.

## 11. Contracts Preserved

Public route, sign-in destination, terminal state, no token/session detail, no redirect or auth policy change, no backend/API access.

## 12. Files Changed

`frontend/src/app/features/auth/session-expired/session-expired.ts` (public route-path property only), `session-expired.html`, `session-expired.scss`, `session-expired.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/{session-expired,access-denied}/*.spec.ts'`: 8/8 pass (both new layout tests red before implementation); `npm run lint:styles`: pass. Batch browser/build/verifier pending.

## 14. Visual Deviations From Stitch

The Go to home affordance is non-operational rather than linked (root route/default redirect not authorized). Header uses the existing public catalog destination; the existing primary label “Sign in again” is retained.

## 15. Remaining Gaps

Root-entry policy decision required for Go to home; batch visual verification pending.

## 16. Page Verdict

STITCH_ALIGNED_WITH_REPORTED_LOGIC_GAPS.

---

# AUTH-011 — Access Denied

## 1. Identity

- AUTH-011; Access Denied; public terminal route `SYSTEM_ACCESS_DENIED` `/access-denied`; `frontend/src/app/features/auth/access-denied/access-denied.ts` + external `.html`/`.scss`.
- Artifact `projects/17116545761229201855/screens/e869c0c2568444b5bc2638036f5fa8c0`, `GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW`.

## 2. Page/State Goal

Offer privacy-safe restricted-route feedback and an existing safe Account destination without exposing roles, permissions, or protected-resource details.

## 3. Refactor Goal

Align public header, icon, centered status card, body hierarchy and safe actions with accepted Stitch composition while retaining the account route and not inventing root/sign-in navigation on this page.

## 4. Goals Achieved

Replaced decorative two-column gradient with approved neutral header/card, lock icon, generic denial copy, prominent existing Go to account action and non-operational Go to home visual label.

## 5. Complete Existing Feature Behavior

`Go to account` still uses `accountPath`; terminal route remains public and unguarded; no token/session/auth-service/API code added. Focused terminal specs 8/8 pass.

## 6. Routes / Redirects / Navigation

| UI action | Destination | Existing behavior? | Route authority | Logic owner |
|---|---|---|---|---|
| Go to account | `/account` | Yes; unchanged | `ACCOUNT_OVERVIEW` | `AccessDenied.accountPath` |
| Header Preparation Packages | `/preparation-packages` | Existing mounted public catalog | `PREPARATION_PACKAGES_OFFERS` | `AccessDenied.publicOffersPath` |
| Go to home appearance | None | No; visual only | Root behavior unresolved | No route/logic |
| Brand | None | No | Root freeze | Presentational wordmark |

## 7. Stitch → Angular Visual Changes

Centered 448px white card, light approved background, 64px header, status icon, quiet supporting copy, teal primary action, responsive logical gutters and approved card radius/border.

## 8. TypeScript / Logic Mapping

| Element | Classification | Owner |
|---|---|---|
| Go to account | EXISTING_LOGIC_VISUALLY_REFACTORED | `AccessDenied.accountPath` |
| Brand, icon, privacy-safe copy | PRESENTATIONAL_ONLY | Template/SCSS |
| Go to home | UI_LOGIC_GAP — VISUAL_ONLY | Root policy unresolved |
| Stitch Sign in | UI_LOGIC_GAP — VISUAL_ONLY | Not an existing action of this restricted-route state |

## 9. UI Without Logic

| Stitch UI element | Location | Visual role | UI implemented? | Logic exists? | Functionality wired? | Action taken | Classification |
|---|---|---|---|---|---|---|---|
| Go to home | Secondary action below Account | Safe destination appearance | YES — static visible text | NO | NO | No routerLink/href/handler. TypeScript: NONE; route: unresolved root; service/facade/API: NONE. Requires separately authorized root-entry policy. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |
| Stitch Sign in | Secondary action area | Sign-in destination appearance | YES — static visible text | NO for this screen | NO | Preserved existing Account action; no new navigation/handler. TypeScript/service/facade/API: NONE. Requires page-level navigation approval. No behavior invented. | UI_LOGIC_GAP — VISUAL_ONLY |

## 10. Existing Functionality Missing From Stitch

STITCH_FUNCTIONALITY_GAP: Current safe Go to account action is absent from Stitch (which shows Go to home + Sign in). Preserved as the primary action rather than silently deleted.

## 11. Contracts Preserved

Generic privacy-safe denial, existing `/account` action, public terminal route, no role/permission/resource disclosure, no auth/API/backend/route behavior changes.

## 12. Files Changed

`frontend/src/app/features/auth/access-denied/access-denied.ts` (public route-path property only), `access-denied.html`, `access-denied.scss`, `access-denied.spec.ts`; this report.

## 13. Verification

`npm test -- --watch=false --include='src/app/features/auth/{session-expired,access-denied}/*.spec.ts'`: 8/8 pass; `npm run lint:styles`: pass. Batch browser/build/verifier pending.

## 14. Visual Deviations From Stitch

Existing Go to account remains the actionable primary destination; Stitch's Sign in and Go to home are static appearances because exact route behavior for this screen/root remains unresolved. No role, permission or protected-resource detail added.

## 15. Remaining Gaps

Human route/UX authority needed to replace Account with Stitch's Sign in or activate Go to home; batch browser/verifier pending.

## 16. Page Verdict

PARTIAL_ALIGNMENT (existing Account navigation preserved; root/sign-in deviations reported).

---

## Visual Fidelity Re-audit (Correction Pass, GOAL STITCH-UI-REFACTOR-2026-09-27)

Human review found Batch 2 (commit `6d4aa6c`) at 0/9 STITCH_ALIGNED_VERIFIED with material
composition drift. Each artifact below was re-opened directly (full HTML, not text extracts) and
every difference classified: REQUIRED_CONTRACT_DEVIATION (behavior/copy/state truth),
REQUIRED_ACCESSIBILITY_DEVIATION, REQUIRED_SECURITY_DEVIATION, UI_LOGIC_GAP — VISUAL_ONLY, or
UNJUSTIFIED_VISUAL_DRIFT (corrected in this pass). No business logic, routes, APIs, or auth behavior
were changed; only HTML/SCSS presentation plus minimal selector-scoping test updates.

Cross-page corrections applied everywhere: 32px teal brand-mark tile in all 8 public headers;
per-page header action treatment (current-page underline vs solid Sign-up pill vs outline pill);
composition padding 48px/64px (terminal pages 48px/64px); card widths per artifact (448/460/480/
500/512/576px); 24px primary-action radius (except AUTH-011, which specifies 12px); arrow affordances
on Stitch-arrowed CTAs. The former shared Material field geometry was subsequently corrected in the
AUTH Form Field Fidelity Follow-up below; it is NOT a required deviation. Retained deviations: Noto Sans system
stack (no webfont mechanism), text wordmark treatment inside the new mark tile, 44px minimum targets
where Stitch draws 40-42px chrome (project 44px rule governs), `tablet` (768px) standing in for
Stitch `sm` (640px) scale steps (approved project breakpoints only).

### AUTH-001 — Sign In (`6427cabcd8b549eb845c85d6b8a0413a`)

- Drift found/corrected: text-only brand → mark tile; uniform header links → per-page treatment
  (teal underlined current Sign in + outline pill Sign up); submit radius 12px → 24px, min-height
  52px → 48px; composition padding 24/48px → 48/64px.
- REQUIRED_CONTRACT_DEVIATION retained: email-only field/label vs “Email or username”; conditional
  Problem Details states vs illustrative simultaneous banners.
- Drift remaining: no webfont, 44px targets. Field geometry corrected in the follow-up below.
- Rendered verification: desktop/mobile/RTL snapshots + measurement (card 448px, one h1, no
  overflow, zero console errors).

### AUTH-002 — Sign Up (`300823b7306f4924a947eb3a80848e63`)

- Drift found/corrected: brand mark tile; header → outline Sign-in pill + solid current Sign-up pill;
  card 448px → 512px, desktop padding 32px → 40px; form gap 16px → 20px; helper 0.875rem → 0.75rem;
  h1 → 1.5rem/1.875rem desktop; submit radius 24px + decorative arrow + semibold label.
- REQUIRED_CONTRACT_DEVIATION retained: Confirm password field (STITCH_FUNCTIONALITY_GAP, Stitch-styled).
- Drift remaining: no webfont, 44px targets. Field geometry corrected in the follow-up below.
- Rendered verification: desktop + mobile screenshots reviewed; measurement (card 512px, no overflow).

### AUTH-005 — Check Email (`265d92edd39248269e867d6d72d44962`)

- Drift found/corrected: brand mark tile; header → plain links + solid Sign-up pill; card 448px →
  500px, tablet padding 32px → 40px; header block mb-8 with start alignment at tablet; subtitle
  0.875rem/1rem responsive scale; CTA arrow + 24px radius; composition 48/64px.
- REQUIRED_CONTRACT_DEVIATION retained: enumeration-safe conditional copy vs “We sent” banner
  (no delivery authority); no resend/countdown.
- Drift remaining: no delivery banner visual, shared tokens/breakpoints, 44px targets.
- Rendered verification: measured (card 500px, one h1, no overflow incl. RTL).

### AUTH-006 — Verify Email (`76bae8f5c9a94284a919bf81b9c8e15c`)

- Drift found/corrected: brand mark tile; solid Sign-up pill; screen-level centered header block
  (h1 1.875rem + subtitle) moved above card per artifact; card 448px → 576px, tablet padding 40px;
  56px decorative state icon tiles per branch (static loader, check, info, error); recovery CTAs
  changed from full-width bars to centered auto-width (min 140px, 44px) actions.
- UI_LOGIC_GAP — VISUAL_ONLY retained: state-preview tab gallery not rendered (would falsify truth).
- REQUIRED_CONTRACT_DEVIATION retained: mutually exclusive backend-driven states only.
- Drift remaining: no animated progress sweep (reduced-motion safety), shared tokens.
- Rendered verification: measured (card 576px, one h1, no overflow incl. RTL).

### AUTH-007 — Forgot Password (`55522993d9514ed9ac67586fb552cb1a`)

- Drift found/corrected: brand mark tile; solid Sign-up pill; card 448px → 460px; h1 centered
  1.5rem + centered secondary instruction; form gap 20px; submit 44px + arrow + 24px radius;
  error/success banners icon + title/detail geometry (p-4, title/detail text); footer rebuilt as
  mt-6/pt-6/border divider with back-arrow Back-to-sign-in.
- UI_LOGIC_GAP — VISUAL_ONLY retained: preview-state tab strip not rendered.
- REQUIRED_CONTRACT_DEVIATION retained: enumeration-safe accepted copy; conditional states only.
- Drift remaining: tablet (not sm) scale steps, 44px targets. Field geometry corrected in the follow-up below.
- Rendered verification: measured (card 460px, one h1, no overflow incl. RTL).

### AUTH-008 — Reset Password (`5782187204dc4229a5970ddf2cc0012f`)

- Drift found/corrected: brand mark tile; solid Sign-up pill; card 448px → 480px, tablet padding
  40px; centered header + 0.875rem instruction; form gap 20px; helper 0.75rem; submit 48px + arrow
  + 24px radius; success icon → 56px circle with check SVG; missing-token block centered with
  outline secondary Back-to-sign-in action; new footer row (arrow Back-to-sign-in + Forgot password,
  mt-8/pt-6/border).
- UI_LOGIC_GAP — VISUAL_ONLY retained: preview-state tab strip not rendered.
- REQUIRED_CONTRACT_DEVIATION retained: opaque-token flow, residual post-success form
  (STITCH_FUNCTIONALITY_GAP), backend-safe copy.
- Drift remaining: tablet scale steps, 44px targets. Field geometry corrected in the follow-up below.
- Rendered verification: desktop screenshot reviewed; measured (card 480px, no overflow incl. RTL).

### AUTH-009 — Reset Password Success, non-routable (`ef4d555b00d54f68870318e12d251780)

- Drift found/corrected: success presentation rebuilt as centered 56px emerald circle icon + status
  + full teal Continue-to-sign-in CTA with arrow (was unstyled link); inherits AUTH-008 header/card.
- REQUIRED_CONTRACT_DEVIATION retained: same-route conditional state (no new route/component);
  existing success copy; residual form preserved (STITCH_FUNCTIONALITY_GAP).
- Rendered verification: covered by AUTH-008 page evidence (same component/route).

### AUTH-010 — Session Expired (`43b0f224781949ad8be651df1991a257`)

- Drift found/corrected: brand mark tile; header → plain links + solid Sign-up pill; 64px warning
  circle badge (was 48px tile); h1 1.5rem; copy 0.875rem/1rem with mt-3/mb-8 rhythm; primary 44px +
  arrow + 24px radius; Go-to-home de-styled from bordered pill to plain muted text (no false
  affordance); composition 48/64px; card tablet padding 40px.
- UI_LOGIC_GAP — VISUAL_ONLY retained: Go-to-home static (root policy unresolved).
- Drift remaining: 64px (not 80px) terminal padding (no 80px project token), shared tokens.
- Rendered verification: measured (card 448px, one h1, no overflow incl. RTL).

### AUTH-011 — Access Denied (`e869c0c2568444b5bc2638036f5fa8c0`)

- Drift found/corrected: brand mark tile; header → plain links + solid 12px Sign-up pill (per
  artifact); 64px error circle badge (was 48px tile); h1 margin rhythm; body fixed 1rem centered;
  static Go-to-home/Sign-in de-styled to plain muted text; card tablet padding 40px; composition
  48/64px; primary Account action kept at artifact-specified 12px radius.
- UI_LOGIC_GAP — VISUAL_ONLY retained: static Go-to-home + Sign-in appearances (root/page-route
  authority missing).
- STITCH_FUNCTIONALITY_GAP retained: existing Go-to-account primary action.
- Drift remaining: shared tokens, 44px targets.
- Rendered verification: measured (card 448px, one h1, no overflow incl. RTL).

### Correction change distribution

- Production HTML files changed: 8 (all auth pages: headers, icons, arrows, footers, verify
  restructure, reset token/success blocks).
- Production SCSS files changed: 8 (geometry, spacing, radius, icon tiles, responsive scale).
- Production TypeScript files changed: 1 (`access-denied.ts`: one readonly `signInPath`
  canonical-path property for the Stitch-specified header link; same established pattern, no logic).
- Test files changed: 2, selector-scoping only (session-expired card-action scope after header gained
  a same-destination link; verify-email h1 scope after approved screen-header move). No assertion
  weakened; no layout accepted as-is.
- Report/docs files changed: 1 (this chapter; per-page §§1–16 factual claims unchanged).
- Visual correction is carried by production HTML/SCSS; tests only track the new structure.

### Remaining deviations and budgets

- REQUIRED deviations catalogued per page above; UNJUSTIFIED_VISUAL_DRIFT remaining: none known —
  every inventoried difference is either corrected or explicitly justified.
- `reset-password.scss` (most state-dense auth page) now compiles to 5.21 kB vs the 4.00 kB
  `anyComponentStyle` warning budget (error budget 8.00 kB; build green). Reduced from 6.02 kB by
  merging duplicated rules; further reduction would require removing fidelity-required state
  coverage or a shared-component refactor, both out of scope for this pass. Initial bundle unchanged
  at 448.74 kB, no initial-budget warning.
- Verdicts after correction: AUTH-001/002/005/006/007/008/009/011 remain PARTIAL_ALIGNMENT only
  insofar as their explicitly justified contract gaps persist; composition fidelity itself is now
  materially faithful. AUTH-010 is STITCH_ALIGNED_WITH_REPORTED_LOGIC_GAPS. No page is called
  STITCH_ALIGNED_VERIFIED while contract-bound copy/state differences remain by design.

## AUTH Form Field Fidelity Follow-up (after `7b78919`)

The previous re-audit erroneously classified 64px floating Material fields as a required deviation.
They were visually inherited, not required by an auth contract. The exact accepted Stitch HTML for
AUTH-001 `6427cabcd8b549eb845c85d6b8a0413a`, AUTH-002
`300823b7306f4924a947eb3a80848e63`, AUTH-007 `55522993d9514ed9ac67586fb552cb1a`,
and AUTH-008 `5782187204dc4229a5970ddf2cc0012f` was inspected at input/label/helper/error
element level. AUTH-009 is a success state in AUTH-008 and inherits its real fields. AUTH-005/006/010/011
contain no form fields; their earlier state/action presentation is unchanged by this follow-up.

| Screen/field | Exact Stitch DOM and geometry | Angular correction | Remaining authority-bound difference |
|---|---|---|---|
| AUTH-001 Email | Persistent uppercase 12px label above native outlined input, required star, 8px radius, ~44px height, 14px inline padding, helper below | Auth-owned persistent label + native 44px/8px input and 6px label gap; approved email-only placeholder | Email-only transport means label stays `Email address`; Stitch's email-or-username and institutional-email helper claim are not adopted. |
| AUTH-001 Password | Persistent uppercase label beside Forgot password link; password input and trailing eye | Label/control 6px gap; Forgot password returned to the password label row; masked native input, decorative non-operational eye centered at control end | Eye is UI_LOGIC_GAP — VISUAL_ONLY; no disclosure behavior invented. |
| AUTH-002 Email | Persistent 12–14px label, 8px outlined input, `name@example.com` placeholder, required star | Native 44px input with persistent label and placeholder | Existing label `Email address` preserved; copy-only difference. |
| AUTH-002 Username | Label above native input, `Choose a username` placeholder | Same treatment as email, independently associated label and error | Backend username validation unchanged. |
| AUTH-002 Password | Label above input, eye in trailing edge, rule helper immediately below field | Native masked input, decorative eye, rule text moved into field support row (8px below control) and associated via `aria-describedby` | Eye not operable (UI_LOGIC_GAP); rule remains current V1 validation copy. |
| AUTH-002 Confirm Password | Not present in Stitch | Existing required field retained; **same persistent-label 44px/8px native input**, placeholder, error row and spacing as Password | REQUIRED_CONTRACT_DEVIATION / STITCH_FUNCTIONALITY_GAP: existing required match validation unchanged. |
| AUTH-007 Email | Persistent label, outlined 44px/8px input, `name@example.com`, inline error beneath field | Matching auth-owned native input; backend error text in field support row, label and error IDs preserved | `Email address` copy and enumeration-safe accepted status retained. |
| AUTH-008 Email | Persistent label, 44px/8px input and placeholder | Matching auth-owned native input + error support row | Required token remains opaque and separate from fields. |
| AUTH-008 New password | Persistent label, password input + trailing eye, helper immediately below, red inline validation state | Masked native input + visual-only eye + helper text/validation row associated with input | No invented toggle; backend-safe validation messages retained. |

Implementation: `frontend/src/app/features/auth/auth-text-field/` is an Authentication-owned,
external-template/SCSS component with a direct label/input relationship, 44px min-height, 8px
radius, 14px inline padding, approved border, visible 2px focus treatment, readable disabled and
red-border error states; helper/error text appears below and is linked by `aria-describedby`. It
accepts the same visual field inputs and emits entered string values; owning auth components retain
their original form controls, exact API payloads, submission flow, autocomplete assignment,
validation and error mapping. The globally shared Material form control remains untouched for
other features. Password-eye SVGs are decorative, `aria-hidden`, non-operational; no route, service
or API behavior was added.

**Verification without Playwright MCP (explicitly prohibited for this GOAL):** test-first field
component tests initially failed because the component was absent, then passed 2/2; per-page
persistent-label tests failed on floating Material markup, then auth family passed 81/81 across 12
files. Full frontend unit suite passed 960/960 across 99 files. `npm run lint` and
`npm run lint:styles` passed. `npm run build` succeeded with initial bundle 448.74 kB; existing
reset-password style warning is 5.40 kB against 4 kB warning / 8 kB error limit. JSDOM
component tests verified actual rendered DOM/label/error/disabled/masked-input structure, but do
not constitute pixel/browser visual verification; CSS/token/DOM comparison with exact Stitch HTML
is the non-Playwright evidence. No Playwright MCP was used.

**Disposition:** prior “shared-field geometry” justification is withdrawn for AUTH-001/002/007/008.
Remaining differences above are copy/backend authority or visual-only password affordance, not
permission to retain the old layout. AUTH-009 form-preservation and other previous contract-bound
page verdicts remain as reported; no page is newly marked STITCH_ALIGNED_VERIFIED solely from tests.
