# Project Execution Memory Bank & Progress State

> **CRITICAL AGENT INSTRUCTION:**
> 1. You MUST read this file in its entirety BEFORE executing any shell commands or code changes.
> 2. Whenever a new plan or task is agreed upon, add it to Section 2 BEFORE writing any code.
> 3. Update task statuses incrementally (`[x]` done, `[/]` in progress, `[ ]` pending) as you work.
> 4. Update Section 4 with handoff instructions BEFORE finishing your session.

---

## 1. Active Context & System State
* **Current Phase:** Phase 1A — Angular Scaffolding & Setup
* **Active Working Directory:** `./frontend` (All Angular CLI, linting, tests, and builds MUST run inside `./frontend`)
* **Framework Version:** Angular 22 (Standalone Architecture, Signals, SCSS, Zoneless)
* **Backend Baseline:** .NET WebApi (OpenAPI contract: `development-openapi-2026-08-16.json`)
* **Global System Status:** `[RESET_READY_FOR_PHASE_1A]`
* **Backend Stabilization:** `[STABILIZED]` — expired Package Exam Session entitlement test fixture repaired; backend build is 0 warnings / 0 errors; full backend suite is 1302/1302 passing with PostgreSQL integration-test configuration.
* **Next Technical Gate:** Safe OpenAPI Capture must be completed before Angular Phase 1A scaffolding.

---

## 2. Comprehensive Execution Checklist

### Phase 0: Governance & Architectural Remediation (COMPLETED)
- [x] **Touch Target Standard:** Fixed 44px (Desktop) / 48px (Mobile) via `@mixin touch-target`.
- [x] **Utilities & Grid Baseline:** Created `src/styles/_utilities.scss` (4px base grid, 2px precision, truncation, `u-visually-hidden`).
- [x] **Design Tokens Authority:** Codified Penpot tokens in `src/styles/_tokens.scss` (`#006B66`, `#173B57`, `#4F46B8`).
- [x] **Material 22 Theme Bridge:** Implemented `src/styles/_material-theme-bridge.scss`.
- [x] **Logical Properties Governance:** Enforced CSS Logical Properties via `.stylelintrc.json` (`postcss-scss`).

### Phase 1A: Angular Scaffolding & Setup (PENDING — NEXT STEP)
- [ ] **Safe OpenAPI Capture Gate:** Capture or validate a non-mutating Development OpenAPI contract before Angular scaffold.
- [ ] **Project Initialization:** Generate Angular 22 app via CLI into `./frontend`.
- [ ] **Subdirectory Isolation:** Ensure all Angular source and configuration lives inside `./frontend/`.
- [ ] **Root `.gitignore` Update:** Configure root `.gitignore` for `frontend/node_modules`, `frontend/dist`, and `frontend/.angular/cache`.
- [ ] **Initial Feature Scaffolding (`preparation-package`):**
  - [ ] Models: `filters`, `paginated-result`, `offer-detail`, `offer-list-item`, `payment-order`.
  - [ ] API Service: `PreparationPackageApiService`.
  - [ ] State Service: `PreparationPackageStateService`.
  - [ ] Presentational Component: `PriceDisplayComponent`.
- [ ] **Baseline Verification:** All unit tests passing, SCSS lint clean, prod build passing.

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
* **[ACTIVE CONSTRAINT]:** All frontend CLI commands MUST run from inside `./frontend`.
* **[ACTIVE BLOCKER]:** Penpot Page 07 Root Frame is 0.01x0.01 — requires manual Penpot UI resize.

---

## 4. Agent Handoff Instructions
> **Instructions for the Next Incoming Agent:**
> 1. System state wiped clean to resolve underlying DI & provider initialization issues. Ready for fresh Phase 1A scaffolding.
> 2. `./frontend` has been deleted. Recreate it via `ng new nursing-platform-frontend` inside a temp directory and relocate, or scaffold directly.
> 3. Backend baseline is stabilized: expired Package Exam Session test fixture repaired; full backend suite is 1302/1302 passing with PostgreSQL integration-test configuration.
> 4. Safe OpenAPI Capture is the next technical gate before Angular scaffold.
> 5. Root `.gitignore` is already configured for `frontend/` paths.
> 6. Phase 0 governance artifacts (tokens, mixins, utilities, theme bridge) were intentionally removed from repository-root `src/styles/`; review Git history and migrate/regenerate them under the future Angular workspace when Phase 1A begins.
> 7. Always update this `/PROGRESS.md` file incrementally as sub-tasks are completed.
> 8. Do NOT perform bulk staging using `git add .`. Stage modified files explicitly.
