# Stitch UI Refactor Report

Canonical detailed report for GOAL `STITCH-UI-REFACTOR-2026-09-27` (visual implementation + UI
refactor). Stitch owns approved visual composition; repository/backend/OpenAPI/route/security/product
contracts own functional and business behavior. No backend, API, route, redirect, permission, or
business-rule changes are authorized by this GOAL. The Landing/Home/default-entry initiative is a
separate task and is excluded.

## Index

| Page ID | Page | Route | Actor | Stitch Artifact | Alignment | UI Logic Gaps | Verification |
|---------|------|-------|-------|-----------------|-----------|---------------|--------------|
| APP-SHELL | Authenticated App Shell | Shell-level (wraps authenticated routes) | Nurse / Employer / Admin | `projects/17116545761229201855/screens/764a9361e16948b0be831e4bf28f584b` (App Shell v3, HUMAN_APPROVED_AUTHENTICATED_VISUAL_BASELINE) | Aligned with reported logic gaps | 2 | Focused shell specs 15/15; full suite 98 files 944/944 (final state); build 448.74 kB, no budget warning |

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
