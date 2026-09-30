# Historical execution packet — superseded; not current task authority.

PACKET_ID: QA-AUTH-UXNAV-A11Y-IMPL-002
TASK_LABEL: Continuation for consolidated Auth UX / Navigation / Accessibility remediation campaign
TASK_PURPOSE: Continue and finish the same bounded campaign from QA-AUTH-UXNAV-A11Y-IMPL-001 after the prior Muse worker stopped at max steps. Complete remaining QA-AUTH-002..006 implementation, focused tests, and full automated verification. Do not stage/commit/push.
TASK_TYPE / DOMAIN: FRONTEND + FRONTEND DESIGN + AUTH / SECURITY + TESTING / VERIFICATION
RISK_LEVEL: Medium-High. Auth UX/accessibility touches authentication screens but must not change backend/security/session contracts.
ACCURACY_REQUIREMENT: High.
ASSIGNED_ROLE: Main Implementation Agent
ASSIGNED_MODEL: opencode/big-pickle
FALLBACK_MODELS: opencode/mimo-v2.5-free for availability/reliability only.

REPOSITORY_SNAPSHOT:
- Repo: /home/karam/development/nursing-platform
- Branch: feat/2026-08-27-frontend-phase-1a-foundation
- Starting campaign HEAD: 0d1a94e fix(frontend): repair successful sign-in navigation
- Current partial implementation exists from previous worker. Current dirty implementation files: sign-in .ts/.html/.scss/.spec.ts and role-selection .ts/.html/.scss/.spec.ts. PROGRESS.md is orchestrator-owned dirty. frontend/angular.json is unrelated/excluded dirty. Staged area is empty.
- Original complete packet is .superpowers/sdd/qa-auth-ux-remediation-impl-packet.md. Read it first; it remains binding unless this continuation explicitly narrows/updates current state.

WORKING_TREE_EXPECTATION:
- Preserve prior partial Sign In and Role Selection changes; do not discard or rewrite broadly.
- Before continuation, expect dirty: PROGRESS.md, frontend/angular.json, sign-in files, role-selection files, plus untracked QA artifacts and packet files.
- Do not stage, commit, push, reset, clean, stash, or delete QA artifacts.
- Do not absorb frontend/angular.json, .playwright-mcp/**, Playwright MCP Full Review/**, or .superpowers/sdd/*.md scratch packets into the campaign commit.
- COMMAND CONSTRAINT: run simple commands only. Do not use compound shell chains (`&&`, `;`), pipes, redirects, `echo`, `head`, `tail`, `sed`, `awk`, or shell truncation. If several checks are needed, run separate commands.
- TEST COMMAND CONSTRAINT: use only repository package scripts for frontend tests. Do not run `npx`, `vitest` directly, or ad-hoc test runners. Focused tests must use `npm test -- --watch=false --include=<path>` from `frontend/` exactly like the original packet evidence.

ALLOWED_FILES:
- Same as QA-AUTH-UXNAV-A11Y-IMPL-001 ALLOWED_FILES.
- For this continuation, continue from remaining work: register-nurse, register-employer, check-email, verify-email, forgot-password, reset-password, session-expired, access-denied, relevant stories/specs, and routing specs if needed. You may also adjust prior sign-in/role-selection changes only if tests/lint require it or requirement coverage is incomplete.

FORBIDDEN_FILES:
- Same as QA-AUTH-UXNAV-A11Y-IMPL-001 FORBIDDEN_FILES, plus do not modify frontend/angular.json and do not modify .superpowers/sdd/*.md.

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
- .superpowers/sdd/qa-auth-ux-remediation-impl-packet.md
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
- Original packet contains the complete requirements and source QA contract.
- This continuation adds current diff context after a max-step worker stop; task remains frontend Auth UX/a11y with high accuracy because security/session behavior must not change.
- Backend internals, database docs, payment docs, and unrelated feature docs remain intentionally omitted because the campaign must not change backend/API/data/payment behavior.

SKILLS_TO_EVALUATE:
- using-superpowers
- brainstorming (approved user contract; no per-screen design approval stop)
- writing-plans (original packet is plan/contract; no separate spec)
- test-driven-development
- subagent-driven-development or executing-plans
- systematic-debugging if tests/runtime behavior fail unexpectedly
- requesting-code-review
- verification-before-completion

KNOWN_COMPLETED_BY_PREVIOUS WORKER:
- Sign In RED then GREEN focused spec: 14/14 passed. Added Forgot password, Create account, single server/submission alert, autocomplete email/current-password, preserved successful /account regression.
- Role Selection RED then GREEN focused spec: 6/6 passed. Added Already have an account? Sign in.
- No staging/commit/push occurred.

REMAINING REQUIREMENTS TO FINISH:
1. Nurse registration: Sign in link, Back to account type link, autocomplete email/new-password/given-name/family-name, 4-field request contract unchanged.
2. Employer registration: same as nurse; no company/org fields.
3. Check Email: Go to sign in CTA; no backend call/resend/countdown/delivery guarantee/account signal.
4. Verify Email Confirm: success Continue to sign in; missing/invalid Back to sign in; no call without token; token never rendered; no resend.
5. Forgot Password: Back to sign in if not already clear; autocomplete email; anti-enumeration unchanged.
6. Reset Password: success Continue to sign in; invalid/backend reset failure rendered once; Request new reset link preserved; autocomplete new-password; no auto-login.
7. Session Expired: Sign in again -> /auth/sign-in; no automatic session side effect.
8. Access Denied: Go to account -> /account; no permission/security weakening.
9. Routing specs if needed for QA-AUTH-001 / route observations.
10. Full verification suite from original packet.

ACCEPTANCE_CRITERIA:
- Same as QA-AUTH-UXNAV-A11Y-IMPL-001. Must provide requirement coverage for QA-AUTH-001 regression and QA-AUTH-002 through QA-AUTH-006.

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
- Same as original packet: no resend, no QA-ENV remediation, no invented visual system/copy beyond explicit CTAs, no backend/OpenAPI/generated/dependency changes.

STOP_CONDITIONS:
- Same as original packet.
- Also STOP if completion requires files outside original ALLOWED_FILES or changing shared form-controls to support autocomplete.

SCOPE / BUDGET:
- Finish the consolidated implementation and verification. Do not stop after each page. Do not stage/commit/push.
- Keep edits minimal and localized. No broad refactor. No generated file edits. No runtime QA artifact edits.
- Use simple shell commands only.
- Use only `npm test -- --watch=false --include=<path>` for focused tests. Do not use `npx` or direct `vitest` commands.

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
