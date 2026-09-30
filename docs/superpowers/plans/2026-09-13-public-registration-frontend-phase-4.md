# Public Registration Frontend Phase 4 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the V1 public registration frontend flow for Role Selection, Nurse Registration, Employer Registration, Check Email, and the bounded Sign In `email_verification_required` regression.

**Architecture:** Keep all public-registration workflow behavior inside the owning Auth feature under `frontend/src/app/features/auth/`. Generated API remains isolated and is consumed only through feature-local adapters. Existing Core auth/session/token/current-user behavior is reused but not modified except for the Sign In coded-403 presentation regression if required.

**Tech Stack:** Angular 22 standalone components, Reactive Forms where consistent with existing Auth screens, Signals, RxJS, Angular Material, SCSS, Vitest, Storybook, generated `ng-openapi-gen@1.0.5` function-based client.

## Global Constraints

- Phase 3 is closed at `8b0ccf4 docs(frontend): update public registration api client`.
- Implement only `AUTH-002`, `AUTH-003`, `AUTH-004`, `AUTH-005`, plus bounded `AUTH-001` `email_verification_required` regression.
- `AUTH-012` remains DEFERRED / BLOCKED and must not be implemented.
- Do not push.
- Do not hand-edit files under `frontend/src/app/core/api/generated/**`.
- Public registration request fields are exactly `email`, `password`, `firstName`, and `lastName`.
- Public registration must not submit `roleIds`, company/organization fields, privileged role fields, tokens, or session/current-user data.
- Public registration success is `202` and navigates to canonical `/auth/verify-email`.
- Registration success does not authenticate: no `AuthResult`, access token, refresh token, `AuthSessionBootstrap`, `CurrentUserStore` hydration, or auto-login.
- `AUTH-005` is PUBLIC informational only: no backend call, resend, countdown/cooldown, account-existence disclosure, or session behavior.
- `AUTH-002` is PUBLIC and navigation-only: Nurse -> `/auth/register/nurse`, Employer -> `/auth/register/employer`.
- `AUTH-001` regression handles HTTP `403` coded Problem Details code `email_verification_required`; no tokens/session are established; wrong credentials behavior remains unchanged; no automatic resend; no invented redirect; no redesign.
- Reuse approved Auth-family visual language and existing Auth screen patterns.
- No new Penpot/human visual stop is required unless materially new visual language appears.
- Use Storybook render-ready desktop/tablet/mobile evidence and wait for target DOM readiness before screenshots.
- Use RED-first focused tests per screen/regression.
- Do not rerun the entire frontend suite after every tiny edit; run focused tests and relevant regressions for atomic closeouts, then complete integrated verification at the end.
- Ordinary Angular components must use external `.html` and `.scss` files via `templateUrl` and `styleUrl`.
- No dependency, backend, database, migration, OpenAPI, generated-client, Storybook configuration, route-contract, non-Auth product, or deferred screen changes.

---

### Task 1: AUTH-002 Role Selection

**Files:**
- Create: `frontend/src/app/features/auth/role-selection/role-selection.ts`
- Create: `frontend/src/app/features/auth/role-selection/role-selection.html`
- Create: `frontend/src/app/features/auth/role-selection/role-selection.scss`
- Create: `frontend/src/app/features/auth/role-selection/role-selection.spec.ts`
- Create: `frontend/src/app/features/auth/role-selection/role-selection.stories.ts`
- Modify: `frontend/src/app/app.routes.ts`
- Modify: `docs/frontend/execution/frontend-implementation-ledger.md`
- Modify: `PROGRESS.md`

**Interfaces:**
- Consumes: `canonicalRoutePath('AUTH_REGISTER_NURSE')`, `canonicalRoutePath('AUTH_REGISTER_EMPLOYER')`, `canonicalRoutePath('AUTH_ROLE_SELECTION')`.
- Produces: lazy public route at `/auth/role-selection` and a navigation-only component with visible Nurse and Employer choices.

- [ ] **Step 1: Write failing component/route tests**
  - Assert `app.routes.ts` lazily activates `auth/role-selection` without guards or redirects.
  - Assert Nurse action navigates to `/auth/register/nurse`.
  - Assert Employer action navigates to `/auth/register/employer`.
  - Assert no backend/auth/session/current-user service is injected or called.

- [ ] **Step 2: Run focused tests to verify RED**
  - Run: `npm test -- --watch=false --include=src/app/features/auth/role-selection/role-selection.spec.ts --include=src/app/features/auth/sign-in/sign-in.spec.ts`
  - Expected: FAIL because `role-selection` does not exist or route is absent.

- [ ] **Step 3: Implement minimal Role Selection**
  - Use a standalone Angular component with external template/style files.
  - Use existing Auth-family layout, tokenized SCSS, semantic heading, accessible controls, and Angular Material button primitives.
  - Add only the lazy route for `AUTH_ROLE_SELECTION`.

- [ ] **Step 4: Run focused/relevant verification**
  - Run focused tests for Role Selection and route regressions.
  - Run lint/style checks needed for this atomic screen.
  - Build Storybook and capture desktop/tablet/mobile render-ready evidence for the Role Selection story.

- [ ] **Step 5: Review, stage, commit**
  - Obtain bounded independent review or verifier evidence.
  - Stage exact files only.
  - Run git-guardian scope review.
  - Commit: `feat(frontend): implement role-selection screen`.

---

### Task 2: AUTH-003 Nurse Registration

**Files:**
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse-api.ts`
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse-api.spec.ts`
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse.ts`
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse.html`
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse.scss`
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse.spec.ts`
- Create: `frontend/src/app/features/auth/register-nurse/register-nurse.stories.ts`
- Modify: `frontend/src/app/app.routes.ts`
- Modify: `docs/frontend/execution/frontend-implementation-ledger.md`
- Modify: `PROGRESS.md`

**Interfaces:**
- Consumes: generated `publicRegisterNurse`, generated `PublicRegisterRequest`, `ApiConfiguration.rootUrl`, `canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST')`.
- Produces: feature-local `RegisterNurseApi.register(request: PublicRegisterRequest): Observable<void>` and public lazy route `/auth/register/nurse`.

- [ ] **Step 1: Write failing API adapter tests**
  - Assert adapter calls `POST /api/v1/auth/register/nurse` through generated operation.
  - Assert request body contains exactly `email`, `password`, `firstName`, `lastName`.
  - Assert body does not contain `roleIds`, `roles`, `company`, `organization`, tokens, or session/current-user fields.
  - Assert adapter returns `void` for `202`.

- [ ] **Step 2: Write failing component/route tests**
  - Assert public lazy route exists without guards or redirects.
  - Assert required validation for four fields, email format, and existing password rules consistent with backend/Auth precedents.
  - Assert successful `202` submits exact request and navigates to `/auth/verify-email`.
  - Assert success does not call `AuthSessionBootstrap`, `CurrentUserStore`, or token storage and does not auto-login.
  - Assert backend validation/problem details map to the form validation summary using existing pattern.

- [ ] **Step 3: Run focused tests to verify RED**
  - Run adapter and component focused tests.
  - Expected: FAIL because files/route do not exist.

- [ ] **Step 4: Implement minimal Nurse Registration**
  - Use feature-local adapter over generated API only.
  - Use strictly typed Reactive Form matching existing Auth screens.
  - Use external template/style files, existing form controls, validation summary, and approved Auth-family visual language.
  - Add only the lazy route for `AUTH_REGISTER_NURSE`.

- [ ] **Step 5: Run focused/relevant verification**
  - Focused tests: adapter, component, relevant route/Auth regressions.
  - Lint/style checks.
  - Storybook build and desktop/tablet/mobile render-ready screenshots for Nurse Registration.

- [ ] **Step 6: Review, stage, commit**
  - Obtain bounded independent review or verifier evidence.
  - Stage exact files only.
  - Run git-guardian scope review.
  - Commit: `feat(frontend): implement nurse-registration screen`.

---

### Task 3: AUTH-004 Employer Registration

**Files:**
- Create: `frontend/src/app/features/auth/register-employer/register-employer-api.ts`
- Create: `frontend/src/app/features/auth/register-employer/register-employer-api.spec.ts`
- Create: `frontend/src/app/features/auth/register-employer/register-employer.ts`
- Create: `frontend/src/app/features/auth/register-employer/register-employer.html`
- Create: `frontend/src/app/features/auth/register-employer/register-employer.scss`
- Create: `frontend/src/app/features/auth/register-employer/register-employer.spec.ts`
- Create: `frontend/src/app/features/auth/register-employer/register-employer.stories.ts`
- Modify: `frontend/src/app/app.routes.ts`
- Modify: `docs/frontend/execution/frontend-implementation-ledger.md`
- Modify: `PROGRESS.md`

**Interfaces:**
- Consumes: generated `publicRegisterEmployer`, generated `PublicRegisterRequest`, `ApiConfiguration.rootUrl`, `canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST')`.
- Produces: feature-local `RegisterEmployerApi.register(request: PublicRegisterRequest): Observable<void>` and public lazy route `/auth/register/employer`.

- [ ] **Step 1: Write failing API adapter tests**
  - Assert adapter calls `POST /api/v1/auth/register/employer` through generated operation.
  - Assert request body contains exactly `email`, `password`, `firstName`, `lastName`.
  - Assert body does not contain `roleIds`, `roles`, `company`, `organization`, tokens, or session/current-user fields.

- [ ] **Step 2: Write failing component/route tests**
  - Assert public lazy route exists without guards or redirects.
  - Assert validation and error mapping match Nurse Registration.
  - Assert successful `202` navigates to `/auth/verify-email` without auth/session/current-user/token behavior.
  - Assert no company/organization fields are rendered or submitted.

- [ ] **Step 3: Run focused tests to verify RED**
  - Run adapter/component focused tests.
  - Expected: FAIL because files/route do not exist.

- [ ] **Step 4: Implement minimal Employer Registration**
  - Use feature-local adapter over generated API only.
  - Match Nurse Registration structure while avoiding shared abstractions unless small and clearly Auth-owned.
  - Add only the lazy route for `AUTH_REGISTER_EMPLOYER`.

- [ ] **Step 5: Run focused/relevant verification**
  - Focused tests: adapter, component, route/Auth regressions.
  - Lint/style checks.
  - Storybook build and desktop/tablet/mobile render-ready screenshots for Employer Registration.

- [ ] **Step 6: Review, stage, commit**
  - Obtain bounded independent review or verifier evidence.
  - Stage exact files only.
  - Run git-guardian scope review.
  - Commit: `feat(frontend): implement employer-registration screen`.

---

### Task 4: AUTH-005 Check Email

**Files:**
- Create: `frontend/src/app/features/auth/check-email/check-email.ts`
- Create: `frontend/src/app/features/auth/check-email/check-email.html`
- Create: `frontend/src/app/features/auth/check-email/check-email.scss`
- Create: `frontend/src/app/features/auth/check-email/check-email.spec.ts`
- Create: `frontend/src/app/features/auth/check-email/check-email.stories.ts`
- Modify: `frontend/src/app/app.routes.ts`
- Modify: `docs/frontend/execution/frontend-implementation-ledger.md`
- Modify: `PROGRESS.md`

**Interfaces:**
- Consumes: `canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST')`, optionally `canonicalRoutePath('AUTH_SIGN_IN')` only if an existing Auth-family pattern already uses a neutral sign-in link and no new redirect/session behavior is introduced.
- Produces: public lazy route `/auth/verify-email` with static informational check-email content.

- [ ] **Step 1: Write failing component/route tests**
  - Assert route `/auth/verify-email` exists and is public with no guards or redirects.
  - Assert component renders check-email informational state.
  - Assert no backend API adapter/service is injected or called.
  - Assert no resend, cooldown/countdown, account-existence disclosure, token/session/current-user behavior, or automatic navigation exists.

- [ ] **Step 2: Run focused tests to verify RED**
  - Run Check Email focused tests and AUTH-006 route regression tests.
  - Expected: FAIL because the route/component is absent.

- [ ] **Step 3: Implement minimal Check Email screen**
  - Use external template/style files and existing Auth-family visual language.
  - Keep copy generic and enumeration-safe.
  - Add only the lazy route for `AUTH_VERIFY_EMAIL_REQUEST`; preserve `AUTH_VERIFY_EMAIL_CONFIRM`.

- [ ] **Step 4: Run focused/relevant verification**
  - Focused tests for Check Email and existing Verify Email confirmation regressions.
  - Lint/style checks.
  - Storybook build and desktop/tablet/mobile render-ready screenshots for Check Email.

- [ ] **Step 5: Review, stage, commit**
  - Obtain bounded independent review or verifier evidence.
  - Stage exact files only.
  - Run git-guardian scope review.
  - Commit: `feat(frontend): implement verification-email notice`.

---

### Task 5: AUTH-001 `email_verification_required` Regression

**Files:**
- Modify: `frontend/src/app/features/auth/sign-in/sign-in.ts`
- Modify: `frontend/src/app/features/auth/sign-in/sign-in.html`
- Modify: `frontend/src/app/features/auth/sign-in/sign-in.spec.ts`
- Modify: `frontend/src/app/features/auth/sign-in/sign-in.stories.ts` if needed for a production rendered state
- Modify: `docs/frontend/execution/frontend-implementation-ledger.md`
- Modify: `PROGRESS.md`

**Interfaces:**
- Consumes: normalized coded Problem Details from `normalizeProblemDetails`, backend code `email_verification_required`.
- Produces: Sign In presentation handling for unverified-email login failure without changing wrong-credentials behavior or session establishment.

- [ ] **Step 1: Write failing Sign In regression test**
  - Simulate login rejecting with HTTP `403` coded Problem Details code `email_verification_required`.
  - Assert no `AuthSessionBootstrap.establishAuthenticatedSession` call.
  - Assert no `CurrentUserStore.hydrate` call.
  - Assert no navigation/redirect is performed.
  - Assert user receives a safe verification-required message using the existing Sign In layout/pattern.
  - Assert wrong credentials behavior remains unchanged.

- [ ] **Step 2: Run focused test to verify RED**
  - Run Sign In focused tests.
  - Expected: FAIL because the coded `403` is not yet specially presented or covered.

- [ ] **Step 3: Implement minimal regression handling**
  - Detect normalized coded error by `code === 'email_verification_required'` and status `403` where available.
  - Present a safe message without auto-resend, redirect, or session changes.
  - Do not redesign Sign In.

- [ ] **Step 4: Run focused/relevant verification**
  - Run Sign In focused tests and relevant Auth registration regressions.
  - Lint/style checks.
  - Storybook build and desktop/tablet/mobile render-ready evidence if a story state is added or modified.

- [ ] **Step 5: Review, stage, commit**
  - Obtain bounded independent review or verifier evidence.
  - Stage exact files only.
  - Run git-guardian scope review.
  - Commit: `fix(frontend): handle unverified-email sign-in`.

---

### Task 6: Final Phase 4 Integration Verification

**Files:**
- Modify: `docs/frontend/execution/frontend-implementation-ledger.md`
- Modify: `PROGRESS.md`

**Interfaces:**
- Consumes: committed task outputs for Tasks 1-5.
- Produces: final Phase 4 Auth verification evidence and clean repository checkpoint.

- [ ] **Step 1: Run complete fresh frontend/Auth verification**
  - Run focused Auth suite.
  - Run `npm test -- --watch=false`.
  - Run `npm run lint`.
  - Run `npm run lint:styles`.
  - Run `npm run check:dependencies`.
  - Run `npm run quality`.
  - Run `npm run build-storybook`.
  - Run `git diff --check`.
  - Run `git status --short --untracked-files=all`.

- [ ] **Step 2: Final review**
  - Obtain final bounded review/verifier evidence for Phase 4 scope.
  - Verify `AUTH-002`, `AUTH-003`, `AUTH-004`, `AUTH-005`, and bounded `AUTH-001` regression are complete.
  - Verify generated files, backend files, dependencies, OpenAPI, database, non-Auth product files, and `AUTH-012` are untouched after Phase 3.

- [ ] **Step 3: Final status commit if needed**
  - If only ledger/PROGRESS final evidence remains, exact-stage those docs and commit as a bounded status checkpoint if consistent with repository atomic-commit policy.
  - Verify working tree is clean and staged area empty.
  - Do not push.
