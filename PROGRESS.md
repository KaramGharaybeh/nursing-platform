# Project Execution Memory Bank & Progress State

> **CRITICAL AGENT INSTRUCTION:**
> 1. You MUST read this file in its entirety BEFORE executing any shell commands or code changes.
> 2. Whenever a new plan or task is agreed upon, add it to Section 2 BEFORE writing any code.
> 3. Update task statuses incrementally (`[x]` done, `[/]` in progress, `[ ]` pending) as you work.
> 4. Update Section 4 with handoff instructions BEFORE finishing your session.

---

## 1. Active Context & System State
* **Current Phase:** GOAL-FE-001 / M-FE-001 — T-FE-003 clean install/build/unit-test baseline VERIFIED.
* **Active Working Directory:** repository root; Angular workspace now exists at `./frontend`.
* **Framework Version:** Angular 22 (Standalone Architecture, Signals, SCSS, Zoneless)
* **Backend Baseline:** .NET WebApi (OpenAPI contract: `development-openapi-2026-09-03.json`)
* **Global System Status:** `[GOAL_FE_001_ROADMAP_PERSISTED_READY_FOR_T_FE_001_AUTHORIZATION]`
* **Backend Stabilization:** `[STABILIZED]` — expired Package Exam Session entitlement test fixture repaired; backend build is 0 warnings / 0 errors; full backend suite is 1302/1302 passing with PostgreSQL integration-test configuration.
* **Next Technical Gate:** Explicit technical-lead authorization for `T-FE-004`; do not begin `T-FE-004` without later explicit authorization.
* **Safe OpenAPI Capture:** `[COMPLETED]` — Development-only `--capture-openapi` skips only database initialization; normal database initialization remains unchanged; CV multipart OpenAPI description corrected; canonical snapshot `development-openapi-2026-09-03.json` verified as OpenAPI 3.1.1 with 106 paths, 142 operations, 110 schemas, and Bearer security scheme; backend build is 0 warnings / 0 errors; full backend suite is 1307/1307 passing.
* **Phase 2C Scope Correction:** `[COMPLETED]` — Non-Development capture rejection sets non-zero exit only on the rejected `--capture-openapi` path; generic fatal startup catch behavior restored; backend build is 0 warnings / 0 errors; full backend suite remains 1307/1307 passing.

---

## 2. Comprehensive Execution Checklist

### Phase 0: Governance & Architectural Remediation (COMPLETED)
- [x] **Touch Target Standard:** Fixed 44px (Desktop) / 48px (Mobile) via `@mixin touch-target`.
- [x] **Utilities & Grid Baseline:** Created `src/styles/_utilities.scss` (4px base grid, 2px precision, truncation, `u-visually-hidden`).
- [x] **Design Tokens Authority:** Codified Penpot tokens in `src/styles/_tokens.scss` (`#006B66`, `#173B57`, `#4F46B8`).
- [x] **Material 22 Theme Bridge:** Implemented `src/styles/_material-theme-bridge.scss`.
- [x] **Logical Properties Governance:** Enforced CSS Logical Properties via `.stylelintrc.json` (`postcss-scss`).

### Phase 1A: Angular Scaffolding & Setup (PENDING — NEXT STEP)
- [x] **Safe OpenAPI Capture Gate:** Implemented Development-only `--capture-openapi`, fixed CV multipart OpenAPI contract metadata, captured `development-openapi-2026-09-03.json`, and verified before Angular scaffold.
- [ ] **Project Initialization:** Generate Angular 22 app via CLI into `./frontend`.
- [ ] **Subdirectory Isolation:** Ensure all Angular source and configuration lives inside `./frontend/`.
- [ ] **Root `.gitignore` Update:** Configure root `.gitignore` for `frontend/node_modules`, `frontend/dist`, and `frontend/.angular/cache`.
- [ ] **Initial Feature Scaffolding (`preparation-package`):**
  - [ ] Models: `filters`, `paginated-result`, `offer-detail`, `offer-list-item`, `payment-order`.
  - [ ] API Service: `PreparationPackageApiService`.
  - [ ] State Service: `PreparationPackageStateService`.
  - [ ] Presentational Component: `PriceDisplayComponent`.
- [ ] **Baseline Verification:** All unit tests passing, SCSS lint clean, prod build passing.

### Phase 3B: Frontend Governance Closure (DOCUMENTATION-ONLY — IN PROGRESS)
- [x] **Governance Guardrails:** Persist approved frontend decision rules for Penpot authority, backend/OpenAPI authority, generic-skill subordination, no-silent-modernization, scope containment, and completed-feature protection.
- [x] **Design Foundation Decisions:** Record approved focus ring, form foundation, motion-token absence, overlay/elevation semantics, responsive breakpoints/gutters, and screen-approval gate rules.
- [x] **Execution Ledger:** Create `docs/frontend/execution/frontend-implementation-ledger.md` as the detailed frontend Goal → Milestone → Task → Subtask execution record template only, without implementation tasks.
- [x] **OpenAPI Reference Reconciliation:** Active/current frontend planning references point to `development-openapi-2026-09-03.json`; historical `2026-08-16` references are preserved as superseded evidence.
- [/] **Final Verification:** Ledger vocabulary/template correction identified during final verification; rerun final consistency checks, then paste `git status --short` and `git diff --stat` in the Phase 3B handoff response.

### Phase 4: Frontend Implementation Roadmap (PLANNING — COMPLETED)
- [x] **Phase 4A Technical Planning:** Complete `GOAL-FE-001 — Production-Quality Nursing Platform Angular Frontend` roadmap reviewed through Phase 4A.2.
- [x] **Phase 4B Persistence:** Detailed Goal → Milestone → Task → Subtask → Verification Gate roadmap persisted to `docs/frontend/execution/frontend-implementation-ledger.md`.
- [x] **Vertical Slice Sequencing:** Recorded approved sequencing: Slice A (`AUTH-001` → session/bootstrap → protected shell → `GET /api/v1/me` → read-only Nurse Profile Overview), then Slice B (`NUR-012` CV management → multipart `file`).
- [x] **Angular Official AI Guidance Addendum:** Reconciled Nursing Platform frontend governance with official Angular v22 AI guidance before scaffold; `T-FE-001` remains not started.
- [x] **T-FE-001 Angular Workspace Scaffold:** VERIFIED by technical-lead PASS; completion commit `4a2fed1 feat(frontend): scaffold Angular workspace`. Angular workspace exists at `./frontend`; approved toolchain is Node `v22.23.1` / npm `11.6.0`; `T-FE-002` remains NOT STARTED and requires explicit technical-lead authorization.
- [x] **T-FE-002 Node/npm/toolchain metadata:** VERIFIED by technical-lead PASS; completion commit `7545c18 chore(frontend): pin Node and npm metadata`. Repository metadata pins Node `22.23.1` and npm `11.6.0`; `frontend/package-lock.json` remains the dependency-resolution authority. `T-FE-003` remains NOT STARTED and requires explicit technical-lead authorization.
- [x] **T-FE-003 Clean install/build/unit-test baseline:** VERIFIED by technical-lead PASS; completion commit `2b188b9 docs(frontend): record baseline build verification`. Accepted evidence: clean install from committed lockfile, Angular build, and non-watch Vitest baseline passed from `./frontend` with Node `v22.23.1` / npm `11.6.0`. `T-FE-004` remains NOT STARTED and requires explicit technical-lead authorization.

### Phase 1B: Core Infrastructure & Network Interceptors (PENDING)
- [ ] **HTTP Interceptors (`frontend/src/app/core/interceptors/`):**
  - [ ] `authInterceptor`: Bearer token injection via `AuthStateService`.
  - [ ] `tenantHeaderInterceptor`: `Accept-Language` + `X-Client-Version` headers.
  - [ ] `errorHandlerInterceptor`: Centralized 401/403 error handling.
  - [ ] `loadingInterceptor`: Signal-backed pending request counter.
- [ ] **Interceptor Registration:** Chain in `app.config.ts`.

### Phase 1C: Router Architecture, Guards & Feature Page Shells (PENDING)
- [ ] **Functional Route Guards (`frontend/src/app/core/guards/`):**
  - [ ] `authGuard`: Redirect unauthenticated to `/auth/login`.
  - [ ] `publicOnlyGuard`: Redirect authenticated away from `/auth/*`.
- [ ] **Feature Lazy Routes:** Preparation package and auth routes.
- [ ] **Page Shell Components:** OffersList, OfferDetail, CheckoutOrder.

### Phase 1D: Core Auth Infrastructure & Login Shell (PENDING)
- [ ] **Core Auth Services (`frontend/src/app/core/auth/`):**
  - [ ] `AuthApiService` with `providedIn: 'root'`.
  - [ ] `AuthStateService` with `providedIn: 'root'`, signals, localStorage persistence.
- [ ] **Interceptor & Guard Refactor:** Wire to `AuthStateService`.
- [ ] **Auth Feature Module:** Login page shell, auth routes with `publicOnlyGuard`.

---

## 3. Known Technical Constraints & Active Blockers
* **[ACTIVE CONSTRAINT]:** `T-FE-003` is VERIFIED. Do not run additional project dependency installation, build, tests, lint, generate an API client, modify backend source, modify Penpot, stage files, commit, push, or begin `T-FE-004` until later explicit approval authorizes the specific Task.
* **[ACTIVE CONSTRAINT]:** After future scaffold approval, all frontend CLI commands MUST run from inside `./frontend`.
* **[ACTIVE CONSTRAINT]:** `PROGRESS.md` remains the high-level canonical session/handoff memory; detailed frontend Goal/Milestone/Task/Subtask history belongs in `docs/frontend/execution/frontend-implementation-ledger.md`.
* **[ACTIVE CONSTRAINT]:** `docs/frontend/execution/frontend-implementation-ledger.md` is the detailed authority for `GOAL-FE-001`; `PROGRESS.md` must not duplicate the 137 Tasks or 74-screen matrix.
* **[ACTIVE CONSTRAINT]:** Official Angular guidance, Agent Skills, and Angular CLI MCP are framework guidance only and remain subordinate to Nursing Platform governance, backend/OpenAPI contracts, approved Penpot evidence, and task scope.
* **[ACTIVE BLOCKER]:** Production backend dependencies remain unresolved for production payment provider/webhook/refund/subscription flows.
* **[ACTIVE BLOCKER]:** Penpot Page 07 Root Frame is 0.01x0.01 — requires manual Penpot UI resize.

---

## 4. Agent Handoff Instructions
> **Instructions for the Next Incoming Agent:**
> 1. `GOAL-FE-001` frontend implementation roadmap is persisted in `docs/frontend/execution/frontend-implementation-ledger.md`; use that ledger as the detailed authority.
> 2. `T-FE-001` is VERIFIED by technical-lead PASS; completion commit `4a2fed1 feat(frontend): scaffold Angular workspace`. Angular workspace exists at `./frontend`. The first attempt with npm `10.9.8` failed during normal npm install with the reproducible npm Arborist `edgesOut` defect; preserve that reopen history in the ledger.
> 2a. `T-FE-002` is VERIFIED by technical-lead PASS; completion commit `7545c18 chore(frontend): pin Node and npm metadata`. Repository metadata pins Node `22.23.1` and npm `11.6.0`; `frontend/package-lock.json` remains the dependency-resolution authority.
> 2b. `T-FE-003` is VERIFIED by technical-lead PASS; completion commit `2b188b9 docs(frontend): record baseline build verification`. Accepted evidence confirms clean install, Angular build, and non-watch Vitest baseline passed from `./frontend` with Node `v22.23.1` / npm `11.6.0`. Do not proceed to `T-FE-004` without explicit authorization.
> 3. Backend baseline is stabilized: expired Package Exam Session test fixture repaired; full backend suite is 1302/1302 passing with PostgreSQL integration-test configuration.
> 4. Active frontend planning must use canonical OpenAPI snapshot `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json`; `2026-08-16` is historical/superseded for implementation planning.
> 5. Keep `PROGRESS.md` high-level and update it only for session/current-state handoff; do not duplicate the detailed roadmap here.
> 6. Root `.gitignore` is already configured for `frontend/` paths.
> 7. Phase 0 governance artifacts (tokens, mixins, utilities, theme bridge) were intentionally removed from repository-root `src/styles/`; review Git history and migrate/regenerate them under the future Angular workspace when Phase 1A begins.
> 8. Vertical Slice A/B sequencing is recorded in the ledger and above; both remain subject to their Task/Gate and screen-approval prerequisites.
> 9. Known production backend blockers remain, especially production payment provider/webhook/refund/subscription support.
> 10. Do NOT perform bulk staging using `git add .`. Stage modified files explicitly only if explicitly instructed.
