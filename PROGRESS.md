# Project Execution Memory Bank & Progress State

> **CRITICAL AGENT INSTRUCTION:**
> 1. You MUST read this file in its entirety BEFORE executing any shell commands or code changes.
> 2. Whenever a new plan or task is agreed upon, add it to Section 2 BEFORE writing any code.
> 3. Update task statuses incrementally (`[x]` done, `[/]` in progress, `[ ]` pending) as you work.
> 4. Update Section 4 with handoff instructions BEFORE finishing your session.

---

## 1. Active Context & System State
* **Current Phase:** Phase 3B — Frontend Governance Closure (documentation-only)
* **Active Working Directory:** repository root for documentation-only governance edits; `./frontend` MUST remain absent until a later explicit scaffold approval.
* **Framework Version:** Angular 22 (Standalone Architecture, Signals, SCSS, Zoneless)
* **Backend Baseline:** .NET WebApi (OpenAPI contract: `development-openapi-2026-09-03.json`)
* **Global System Status:** `[READY_FOR_PHASE_3B_REVIEW]`
* **Backend Stabilization:** `[STABILIZED]` — expired Package Exam Session entitlement test fixture repaired; backend build is 0 warnings / 0 errors; full backend suite is 1302/1302 passing with PostgreSQL integration-test configuration.
* **Next Technical Gate:** Review Phase 3B governance closure before building the complete Goal → Milestone → Task → Subtask frontend implementation plan. Angular scaffolding remains unauthorized.
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
* **[ACTIVE CONSTRAINT]:** Do not scaffold Angular, create `frontend/`, install dependencies, generate an API client, modify backend source, modify Penpot, stage files, commit, or push during Phase 3B.
* **[ACTIVE CONSTRAINT]:** After future scaffold approval, all frontend CLI commands MUST run from inside `./frontend`.
* **[ACTIVE CONSTRAINT]:** `PROGRESS.md` remains the high-level canonical session/handoff memory; detailed frontend Goal/Milestone/Task/Subtask history belongs in `docs/frontend/execution/frontend-implementation-ledger.md`.
* **[ACTIVE BLOCKER]:** Penpot Page 07 Root Frame is 0.01x0.01 — requires manual Penpot UI resize.

---

## 4. Agent Handoff Instructions
> **Instructions for the Next Incoming Agent:**
> 1. Phase 3B frontend governance closure documentation patch is ready for review after final command-output evidence is pasted. Do not begin the frontend implementation plan until reviewer approval.
> 2. Do not scaffold Angular or create `./frontend` until a later explicit approval. The approved scaffold contract is documented in `docs/frontend/frontend-project-rules.md` for future use only.
> 3. Backend baseline is stabilized: expired Package Exam Session test fixture repaired; full backend suite is 1302/1302 passing with PostgreSQL integration-test configuration.
> 4. Active frontend planning must use canonical OpenAPI snapshot `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json`; `2026-08-16` is historical/superseded for implementation planning.
> 5. The detailed frontend execution ledger is `docs/frontend/execution/frontend-implementation-ledger.md`; keep `PROGRESS.md` high-level and update it only for session/current-state handoff.
> 6. Root `.gitignore` is already configured for `frontend/` paths.
> 7. Phase 0 governance artifacts (tokens, mixins, utilities, theme bridge) were intentionally removed from repository-root `src/styles/`; review Git history and migrate/regenerate them under the future Angular workspace when Phase 1A begins.
> 8. Always update this `/PROGRESS.md` file incrementally as sub-tasks are completed.
> 9. Do NOT perform bulk staging using `git add .`. Stage modified files explicitly only if explicitly instructed.
