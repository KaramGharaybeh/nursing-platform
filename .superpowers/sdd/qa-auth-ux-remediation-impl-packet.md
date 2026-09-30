# Historical execution packet — superseded; not current task authority.

PACKET_ID: QA-AUTH-UXNAV-A11Y-IMPL-001
TASK_LABEL: Consolidated Auth UX / Navigation / Accessibility remediation campaign
TASK_PURPOSE: Resolve remaining actionable Auth findings QA-AUTH-002 through QA-AUTH-006 from the Playwright MCP Full Review in one bounded frontend campaign, preserving QA-AUTH-001 regression safety and existing backend/security contracts.
TASK_TYPE / DOMAIN: FRONTEND + FRONTEND DESIGN + AUTH / SECURITY + TESTING / VERIFICATION
RISK_LEVEL: Medium-High. Auth UX/accessibility touches authentication screens but must not change backend/security/session contracts.
ACCURACY_REQUIREMENT: High.
ASSIGNED_ROLE: Main Implementation Agent
ASSIGNED_MODEL: opencode/muse-spark-1.3-contributor-free
FALLBACK_MODELS: opencode/big-pickle, opencode/mimo-v2.5-free for availability/reliability only.

REPOSITORY_SNAPSHOT:
- Repo: /home/karam/development/nursing-platform
- Branch: feat/2026-08-27-frontend-phase-1a-foundation
- Starting HEAD: 0d1a94e fix(frontend): repair successful sign-in navigation
- Baseline captured: git rev-parse --short HEAD => 0d1a94e; git diff --cached --name-status => no output; git status includes unrelated M frontend/angular.json and untracked .playwright-mcp/** plus Playwright MCP Full Review/**.

WORKING_TREE_EXPECTATION:
- Before edits, expect PROGRESS.md dirty from orchestrator campaign startup plus unrelated/excluded frontend/angular.json and untracked QA artifacts.
- Do not stage, commit, push, reset, clean, stash, or delete QA artifacts.
- Do not absorb frontend/angular.json, .playwright-mcp/**, Playwright MCP Full Review/**, or this scratch packet into the campaign commit.
- COMMAND CONSTRAINT: run simple commands only. Do not use compound shell chains (`&&`, `;`), pipes, redirects, `echo`, `head`, `tail`, `sed`, `awk`, or shell truncation. If you need several checks, run separate tool calls/commands. If a command is permission-blocked before edits, report BLOCKED rather than retrying broad shell syntax.

ALLOWED_FILES:
- frontend/src/app/features/auth/sign-in/sign-in.ts
- frontend/src/app/features/auth/sign-in/sign-in.html
- frontend/src/app/features/auth/sign-in/sign-in.scss
- frontend/src/app/features/auth/sign-in/sign-in.spec.ts
- frontend/src/app/features/auth/sign-in/sign-in.stories.ts
- frontend/src/app/features/auth/role-selection/role-selection.ts
- frontend/src/app/features/auth/role-selection/role-selection.html
- frontend/src/app/features/auth/role-selection/role-selection.scss
- frontend/src/app/features/auth/role-selection/role-selection.spec.ts
- frontend/src/app/features/auth/role-selection/role-selection.stories.ts
- frontend/src/app/features/auth/register-nurse/register-nurse.ts
- frontend/src/app/features/auth/register-nurse/register-nurse.html
- frontend/src/app/features/auth/register-nurse/register-nurse.scss
- frontend/src/app/features/auth/register-nurse/register-nurse.spec.ts
- frontend/src/app/features/auth/register-nurse/register-nurse.stories.ts
- frontend/src/app/features/auth/register-employer/register-employer.ts
- frontend/src/app/features/auth/register-employer/register-employer.html
- frontend/src/app/features/auth/register-employer/register-employer.scss
- frontend/src/app/features/auth/register-employer/register-employer.spec.ts
- frontend/src/app/features/auth/register-employer/register-employer.stories.ts
- frontend/src/app/features/auth/check-email/check-email.ts
- frontend/src/app/features/auth/check-email/check-email.html
- frontend/src/app/features/auth/check-email/check-email.scss
- frontend/src/app/features/auth/check-email/check-email.spec.ts
- frontend/src/app/features/auth/check-email/check-email.stories.ts
- frontend/src/app/features/auth/verify-email/verify-email.ts
- frontend/src/app/features/auth/verify-email/verify-email.html
- frontend/src/app/features/auth/verify-email/verify-email.scss
- frontend/src/app/features/auth/verify-email/verify-email.spec.ts
- frontend/src/app/features/auth/verify-email/verify-email.stories.ts
- frontend/src/app/features/auth/forgot-password/forgot-password.ts
- frontend/src/app/features/auth/forgot-password/forgot-password.html
- frontend/src/app/features/auth/forgot-password/forgot-password.scss
- frontend/src/app/features/auth/forgot-password/forgot-password.spec.ts
- frontend/src/app/features/auth/forgot-password/forgot-password.stories.ts
- frontend/src/app/features/auth/reset-password/reset-password.ts
- frontend/src/app/features/auth/reset-password/reset-password.html
- frontend/src/app/features/auth/reset-password/reset-password.scss
- frontend/src/app/features/auth/reset-password/reset-password.spec.ts
- frontend/src/app/features/auth/reset-password/reset-password.stories.ts
- frontend/src/app/features/auth/session-expired/session-expired.ts
- frontend/src/app/features/auth/session-expired/session-expired.html
- frontend/src/app/features/auth/session-expired/session-expired.scss
- frontend/src/app/features/auth/session-expired/session-expired.spec.ts
- frontend/src/app/features/auth/session-expired/session-expired.stories.ts
- frontend/src/app/features/auth/access-denied/access-denied.ts
- frontend/src/app/features/auth/access-denied/access-denied.html
- frontend/src/app/features/auth/access-denied/access-denied.scss
- frontend/src/app/features/auth/access-denied/access-denied.spec.ts
- frontend/src/app/features/auth/access-denied/access-denied.stories.ts
- frontend/src/app/app.routes.spec.ts
- frontend/src/app/core/routing/canonical-routes.spec.ts
- frontend/src/app/core/routing/route-permission-policy.spec.ts

FORBIDDEN_FILES:
- backend/**
- frontend/src/app/core/api/generated/**
- frontend/angular.json
- frontend/package.json
- frontend/package-lock.json
- frontend/dependency-policy.json
- .playwright-mcp/**
- Playwright MCP Full Review/**
- PROGRESS.md (orchestrator-owned; read only)
- docs/frontend/execution/frontend-implementation-ledger.md unless you can prove a required frontend-ledger status update is impossible for the orchestrator to do later; prefer not to edit it.
- Any dependency/tooling/configuration/database/migration/OpenAPI/Penpot files.

GLOBAL_CONTEXT_MODULES:
- AGENTS.md
- PROJECT_RULES.md
- CURRENT_TASK.md
- PROGRESS.md
- docs/index.md
- docs/standards/engineering-standards.md
- docs/development/development-guide.md
- docs/development/model-orchestration.md

TASK_CONTEXT_MODULES:
- docs/frontend/frontend-architecture.md
- docs/frontend/frontend-project-rules.md
- docs/frontend/design/frontend-design-foundation-reference.md
- docs/frontend/execution/frontend-implementation-ledger.md
- Playwright MCP Full Review/final-report.md
- Playwright MCP Full Review/pages/auth-sign-in/review.md
- Playwright MCP Full Review/pages/auth-role-selection/review.md
- Playwright MCP Full Review/pages/auth-register-nurse/review.md
- Playwright MCP Full Review/pages/auth-register-employer/review.md
- Playwright MCP Full Review/pages/auth-verify-email/review.md
- Playwright MCP Full Review/pages/auth-verify-email-confirm/review.md
- Playwright MCP Full Review/pages/auth-forgot-password/review.md
- Playwright MCP Full Review/pages/auth-reset-password/review.md
- Playwright MCP Full Review/pages/session-expired/review.md
- Playwright MCP Full Review/pages/access-denied/review.md
- Playwright MCP Full Review/journeys/nurse-journey.md
- Playwright MCP Full Review/journeys/employer-journey.md
- Playwright MCP Full Review/journeys/unverified-user-journey.md
- Playwright MCP Full Review/journeys/password-reset-journey.md
- Playwright MCP Full Review/journeys/duplicate-registration-journey.md

CONTEXT_SELECTION_RATIONALE:
- Global modules are required by repository governance and model orchestration.
- Frontend architecture/project rules/design foundation are required because this is Angular UI/SCSS/screen work with accessibility and responsive constraints.
- Frontend ledger preserves implemented Auth/screen/task constraints and verified session/token architecture.
- Playwright review files are the QA source of truth for QA-AUTH-002 through QA-AUTH-006 and route-level observations.
- Backend internals, database docs, payment docs, and unrelated feature docs are intentionally omitted because this campaign must not change backend/API/data/payment behavior.

SKILLS_TO_EVALUATE:
- using-superpowers
- brainstorming (user supplied approved bounded contract; do not stop for per-screen design approval)
- writing-plans (use this packet as implementation plan/contract; do not create a separate spec)
- test-driven-development
- subagent-driven-development or executing-plans
- systematic-debugging if tests/runtime behavior fail unexpectedly
- requesting-code-review
- verification-before-completion

KNOWN_CONSTRAINTS:
- QA-AUTH-001 is CLOSED by commit 0d1a94e; do not reopen/redesign it. Regression-test successful sign-in /account fallback.
- QA-ENV-001 and QA-ENV-002 are test-environment notes, not production defects.
- Preserve access token memory-only; refresh token sessionStorage-only; no localStorage token persistence; roles/permissions from /me; CurrentUserStore hydration; safe returnUrl; /account canonical fallback.
- No auto-login after registration/reset. No resend verification API/UI/action. No account enumeration. No backend behavior changes.
- Preserve existing visual language: cards, hierarchy, spacing tokens, colors/tokens, focus styles, responsive behavior. No hard-coded new colors when tokens exist. No material visual redesign.
- Ordinary production Angular components must retain external .html and .scss files and focused colocated specs.
- Use routerLink/router navigation following existing conventions.
- Do not weaken /account guards or route permission enforcement.

REQUIREMENTS:
1. Sign In: add Forgot password? -> /auth/forgot-password near password/form area; add New to Nursing Platform? Create an account -> /auth/role-selection below primary Sign In; add autocomplete email/current-password; render wrong credentials, unverified account, and generic submit failure exactly once as server/submission error presentation/live announcement; preserve successful verified login -> /account.
2. Role Selection: preserve Nurse/Employer choices and add/retain Already have an account? Sign in -> /auth/sign-in.
3. Nurse Registration: preserve exact request contract email/password/firstName/lastName only; add Already have an account? Sign in -> /auth/sign-in and Back to account type -> /auth/role-selection; add autocomplete email/new-password/given-name/family-name; do not add confirm password/profile/company/roleIds.
4. Employer Registration: same navigation/autocomplete architecture and same four-field public contract only; do not add organization/company fields.
5. Check Email: preserve privacy-safe informational behavior; add Go to sign in -> /auth/sign-in; no backend call/resend/countdown/delivery guarantee/account-existence signal.
6. Verify Email Confirm: preserve opaque token handling, no call when missing/empty token, invalid safe error, valid success; success shows Continue to sign in -> /auth/sign-in; missing/invalid show Back to sign in -> /auth/sign-in; never render/persist/log token; no resend.
7. Forgot Password: preserve anti-enumeration behavior and validation; if no clear secondary route exists, add Back to sign in -> /auth/sign-in; autocomplete=email.
8. Reset Password: preserve missing-token handling, Request a new reset link recovery, password validation, no auto-login, in-flow reset-success state; success adds Continue to sign in -> /auth/sign-in; invalid/backend reset failure renders safe reset failure once; new password autocomplete=new-password.
9. Session Expired: preserve PUBLIC terminal screen, add primary Sign in again -> /auth/sign-in; no automatic session mutation/redirect.
10. Access Denied: preserve PUBLIC permission-denied screen, add Go to account -> /account; existing /account guards authoritative; no role redirects/security weakening.
11. QA-AUTH-004: field validation may use linked validation summary + inline field errors; server/submission/auth errors must use ONE user-facing presentation/live announcement, avoiding simultaneous duplicate role=alert announcements.
12. QA-AUTH-006: add required autocomplete semantics without changing values/validation.
13. Update focused tests first (RED/GREEN) for changed behavior, including the minimum list in the user task.
14. Storybook stories may be updated only to represent authorized states/screens and support later screenshot evidence; do not invent behavior.

ACCEPTANCE_CRITERIA:
- QA-AUTH-002 closed by Sign In recovery/registration links and registration/role-selection route observations.
- QA-AUTH-003 closed by onward CTAs on check email, verify success/missing/invalid, reset success, and forgot/reset recovery consistency.
- QA-AUTH-004 closed by single server/submission/auth error announcement in Sign In and Reset invalid/backend failure.
- QA-AUTH-005 closed by recovery actions on Session Expired and Access Denied.
- QA-AUTH-006 closed by autocomplete attributes on all listed email/password/name fields.
- QA-AUTH-001 regression remains fixed: successful verified sign-in reaches /account and fallback works.
- No backend/API/generated/dependency/security/session contract change.
- Changed components preserve external templates/styles and pass component separation rules.

VERIFICATION_REQUIRED:
Run focused tests during development, then at completion run these from frontend/ unless stated otherwise:
- Focused affected Auth/routing specs covering all changed files. Include exact command(s) and results.
- npm test -- --watch=false
- npm run lint
- npm run lint:styles
- npm run check:dependencies
- npm run quality
- npm run build-storybook
- From repo root: git diff --check
- From repo root: git status --short --untracked-files=all
- From repo root: git diff --name-status
- From repo root: git diff --cached --name-status (should be empty; do not stage)

FORBIDDEN_ASSUMPTIONS:
- Do not assume a resend verification endpoint or UI exists.
- Do not treat QA-ENV-001/002 as app defects.
- Do not infer new product copy/visual system beyond explicit CTA labels and existing Auth language.
- Do not modify backend/OpenAPI/generated API to make UI easier.
- Do not add dependencies or tooling.

STOP_CONDITIONS:
- New product/security/API/data-model/dependency/material visual-language decision is required.
- Backend/OpenAPI contract evidence contradicts the user-provided backend boundary.
- Required implementation cannot stay inside ALLOWED_FILES.
- Tests reveal a real auth/session/token/security regression not addressed by this packet.
- Skills cannot be accessed/loaded per AGENTS.md.

SCOPE / BUDGET:
- One consolidated implementation pass. Do not stop after each page. Do not stage/commit/push.
- Keep edits minimal and localized. No broad refactor. No generated file edits. No runtime QA artifact edits.
- If a bounded test needs updating outside ALLOWED_FILES, stop and report exact requested file/why.
- Use only simple shell commands; no compound shell, pipes, redirects, `head`, `tail`, `sed`, `awk`, or `echo`.

RESULT_SHAPE:
- STATUS
- TASK_ID / PACKET_ID
- ASSIGNED_MODEL and ACTUAL_MODEL if available
- FILES_READ
- CONTEXT_MODULES_READ
- CONTEXT_SELECTION_RATIONALE
- SKILLS_EVALUATED / SKILLS_LOADED / SKILL_REASONING
- FILES_CHANGED
- REQUIREMENT_COVERAGE (map each QA-AUTH-002..006 and QA-AUTH-001 regression)
- TEST_FIRST_EVIDENCE (RED/GREEN summary)
- VERIFICATION (commands and exact pass/fail summaries)
- GIT_SCOPE (status, diff name-status, cached diff)
- OPEN_QUESTIONS / UNRESOLVED_QUESTIONS
- ASSUMPTIONS
- RISKS
- STOP_REASON
- EVIDENCE_LOCATIONS
