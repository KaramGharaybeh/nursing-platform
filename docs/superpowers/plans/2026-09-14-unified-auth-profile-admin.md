# Unified Auth Profile Admin Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace role-specific public registration with unified server-authoritative sign-up, add backend-owned username/profile-onboarding state, and implement protected Admin role management with verified backend, OpenAPI, generated-client, frontend, and E2E coverage.

**Architecture:** Backend remains authoritative for identity, roles, permissions, username uniqueness, and minimum-profile state. Frontend consumes generated API contracts through non-generated feature adapters and never sends public role/account-type input or stores token/role authority in `localStorage`. Work is split into backend/domain/database, OpenAPI/generated-client, frontend UI/routing, and final verification commits.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, Clean Architecture, MediatR, EF Core code-first migrations, PostgreSQL, Angular 22 standalone components/signals, Angular Material, SCSS, ng-openapi-gen 1.0.5.

## Global Constraints

- Branch remains `feat/2026-08-27-frontend-phase-1a-foundation`; starting baseline is `c2a203c`.
- Do not push. Do not amend prior commits `0d1a94e` or `c2a203c`.
- Do not stage or commit excluded artifacts: `frontend/angular.json`, `.playwright-mcp/**`, `.superpowers/sdd/qa-auth-ux-remediation-*.md`, `Playwright MCP Full Review/**`.
- Public Sign In remains `/auth/sign-in`; public Sign Up is `/auth/sign-up`.
- Public users never see or choose role/account type; frontend sends no `role`, `roleId`, `roleIds`, `accountType`, or `actorType` fields.
- Backend assigns the seeded `Nurse` role server-side for all public sign-ups; public self-assignment of `Employer`, `Expert`, `Admin`, or future privileged roles must be impossible.
- Username is required, unique, normalized, server validated, and database protected.
- Sign In remains current authorized credential identifier unless existing contracts explicitly support username login.
- First/last name belong to authenticated minimum profile onboarding at `/onboarding/profile`.
- Backend `/me` owns username, roles, permissions, and profile/onboarding state.
- Token invariants remain: access token memory-only, refresh token `sessionStorage`-only, no `localStorage` token/role authority.
- Existing safe `returnUrl`, `/account` fallback, no auto-login after registration/reset, and `/me` role/permission authority must be preserved.
- Ordinary Angular production components must use external `.html` and `.scss`, no inline `template`, inline `styles`, or template-local `<style>` blocks.

---

## File Structure

### Backend files expected to change or be created

- Modify `backend/src/NursingPlatform.Domain/Identity/User.cs`: add `Username`, `NormalizedUsername`, optional `FirstName`/`LastName`, and a derived/profile-completion helper if consistent with existing entity style.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/UserConfiguration.cs`: configure username lengths, required normalized username, unique normalized username index, and nullable first/last-name columns.
- Create one EF migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` using timestamp naming and update `ApplicationDbContextModelSnapshot.cs` through EF tooling.
- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` only if new DbSet/query surfaces are required; otherwise preserve existing boundary.
- Modify `backend/src/NursingPlatform.Application/Identity/Commands/PublicRegister/*`: replace public role-specific request/command shape with unified sign-up fields `Email`, `Username`, `Password`; server-resolve seeded Nurse by name.
- Modify `backend/src/NursingPlatform.Application/Identity/Commands/Register/*`: keep admin-created-user role assignment protected and add username support if the admin endpoint remains the create-user API.
- Create `backend/src/NursingPlatform.Application/Identity/Commands/UpdateCurrentUserProfile/` with command, request, response/DTO if needed, validator, and handler for first/last name onboarding.
- Create `backend/src/NursingPlatform.Application/Identity/Commands/UpdateUserRoles/` or equivalent admin role-change folder with request, command, validator, and transactional handler.
- Modify `backend/src/NursingPlatform.Application/Identity/DTOs/UserDetailDto.cs` and `UserListItemDto.cs`: add username and minimum profile/onboarding state fields without sensitive data.
- Modify `backend/src/NursingPlatform.Application/Identity/Queries/GetCurrentUser/*`, `GetUser/*`, and `ListUsers/*`: explicit projections include username and deterministic roles/permissions.
- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`: expose `POST /api/v1/auth/sign-up`, profile update endpoint under `/api/v1/me`, and protected admin role-management route under existing user/admin grouping conventions.
- Modify WebApi endpoint metadata/ProblemDetails declarations where existing patterns require OpenAPI response schema/status accuracy.

### Backend tests expected to change or be created

- Modify/add `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/PublicRegisterCommandTests.cs`.
- Modify/add `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PublicRegisterEndpointTests.cs`.
- Modify/add `backend/tests/NursingPlatform.Application.Tests/Identity/Commands/RegisterUserCommandTests.cs` and `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/RegisterEndpointTests.cs` if admin create-user keeps `/auth/register`.
- Modify/add `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/GetCurrentUser/GetCurrentUserQueryHandlerTests.cs`.
- Modify/add `backend/tests/NursingPlatform.Application.Tests/Identity/Queries/ListUsers/ListUsersQueryHandlerTests.cs` and `GetUser/GetUserQueryHandlerTests.cs`.
- Modify/add `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/UsersEndpointTests.cs` for `/me`, list/detail, and protected role change.
- Add infrastructure configuration/migration tests near existing persistence tests if the project has matching patterns.

### OpenAPI/generated-client files expected to change

- Modify canonical OpenAPI snapshot `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` only through approved capture/correction process.
- Regenerate `frontend/src/app/core/api/generated/**` using existing `npm run generate:api` workflow; do not hand-edit generated files.

### Frontend files expected to change or be created

- Modify `frontend/src/app/core/routing/canonical-routes.ts` and specs for `/auth/sign-up`, redirects/legacy route builders, and `/onboarding/profile` if current route registry lacks them.
- Modify `frontend/src/app/app.routes.ts` to mount `/auth/sign-up`, legacy role-registration redirects, `/onboarding/profile`, and Admin user-management route(s) using existing guard/policy patterns.
- Modify `frontend/src/app/core/auth/current-user-store.ts` and adapter/model tests so username and profile/onboarding state come from `/me`.
- Add or modify non-generated API adapters under feature/core boundaries for sign-up, current-user profile update, and admin role changes.
- Replace role-specific public registration screens under `frontend/src/app/features/auth/**` with unified Sign Up while preserving sign-in/recovery UX from `c2a203c`.
- Create `frontend/src/app/features/onboarding/profile/*` with external template/style/spec for minimum first/last-name onboarding.
- Create `frontend/src/app/features/admin/users/*` or the existing admin feature location discovered by the frontend worker, with search/detail/role-change UI backed by protected APIs.
- Add Storybook stories only for new visible components if current Storybook governance requires visual evidence.

---

## Task 1: Backend identity contract and database migration

**Interfaces:**
- Produces backend persisted fields `Username`, `NormalizedUsername`, nullable `FirstName`, nullable `LastName`, and database uniqueness on normalized username.
- Consumes existing `User`, EF `UserConfiguration`, and existing PostgreSQL integration-test patterns.

- [ ] Write failing domain/application/persistence tests proving username is required, normalized username uniqueness is enforced, public registration no longer requires first/last name, and duplicate username cannot create a second user.
- [ ] Run focused backend tests and confirm RED failures reference missing username/profile behavior.
- [ ] Modify `User` and EF configuration minimally.
- [ ] Generate one EF migration and snapshot update through EF tooling; include backfill for existing users using deterministic existing data such as email local-part plus stable suffix where required by uniqueness.
- [ ] Run focused application/infrastructure tests until GREEN.
- [ ] Run `dotnet build backend/NursingPlatform.slnx` and relevant focused backend suites.

## Task 2: Unified public sign-up API

**Interfaces:**
- Produces `POST /api/v1/auth/sign-up` anonymous endpoint accepting only `Email`, `Username`, and `Password`.
- Preserves email verification flow and no auto-login.
- Deprecates or redirects/removes public role-specific registration routes according to WebApi route compatibility patterns, without allowing role input.

- [ ] Write WebApi tests proving anonymous valid sign-up returns accepted/success per current public-register convention, sends no tokens, creates user with only seeded `Nurse` role, and rejects duplicate email/username according to existing privacy/error policy.
- [ ] Write raw JSON/security tests proving role/account-type fields are not accepted or not present in response, and no sensitive fields leak.
- [ ] Implement request/command/validator/handler changes with server-side Nurse role lookup by name, no hardcoded role ID.
- [ ] Preserve email verification token generation and MailPit/development email link behavior.
- [ ] Verify focused public-register and auth endpoint tests.

## Task 3: Current-user profile state and onboarding API

**Interfaces:**
- Produces `/api/v1/me` response fields for username and profile/onboarding state.
- Produces authenticated profile update endpoint for minimum first/last name completion.

- [ ] Write application tests for `/me` projection: username included, roles/permissions sorted/deduplicated, profile incomplete when first/last is missing, complete when both are present, no sensitive fields.
- [ ] Write WebApi tests for profile update: `401` unauthenticated; authenticated valid update persists first/last name and returns updated state; validation rejects empty/overlength values.
- [ ] Implement DTO/query/command/handler and route wiring using existing exception/ProblemDetails conventions.
- [ ] Verify focused current-user/profile tests.

## Task 4: Protected Admin user role management

**Interfaces:**
- Produces protected backend role-change capability under existing users/admin conventions.
- Consumes seeded role names/IDs only through database lookup.
- Produces transactional role replacement and refresh-token revocation recommendation/implementation.

- [ ] Write tests for admin role change authorization: `401` unauthenticated, `403` authenticated without exact permission, success with exact user-management permission.
- [ ] Write application tests proving requested role IDs exist, duplicate role IDs are de-duplicated, unrelated roles are not invented, role replacement is transactional, and changing roles revokes that user's active refresh tokens.
- [ ] Implement command/validator/handler/endpoint with EF transaction if existing DbContext exposes transaction support; otherwise use existing SaveChanges atomicity and document limitation in evidence.
- [ ] Verify list/detail/search include username and roles without permissions in list DTO unless existing detail DTO requires permissions.

## Task 5: Backend OpenAPI capture and generated client

**Interfaces:**
- Produces canonical OpenAPI matching runtime backend endpoints and regenerated generated TypeScript client.

- [ ] Run backend OpenAPI capture using the repository-approved command/pattern.
- [ ] Validate JSON and inspect only auth/me/admin-user operations for expected request/response schemas/status/security.
- [ ] Run `npm run generate:api` from `frontend/`.
- [ ] Verify generated files changed only as expected and no generated file was hand-edited.
- [ ] Run frontend dependency guard and focused generated-client compile checks through `npm run quality` or narrower approved commands first if failures need debugging.

## Task 6: Frontend unified Sign Up and route migration

**Interfaces:**
- Consumes generated sign-up API and non-generated adapter.
- Produces public `/auth/sign-up` screen and legacy route behavior preserving auth navigation UX.

- [ ] Write focused frontend tests proving Sign In Create Account links to `/auth/sign-up`, sign-up form has Email/Username/Password/Confirm password only, and submitted payload excludes all role/account-type fields.
- [ ] Write route tests proving role-selection and role-specific public registration are not user-choice surfaces and route to unified Sign Up or other approved compatibility behavior.
- [ ] Implement external-template/style Angular component and feature adapter.
- [ ] Verify no auto-login after registration and success/check-email behavior remains compatible with email verification.

## Task 7: Frontend onboarding profile route and guard behavior

**Interfaces:**
- Consumes CurrentUserStore `/me` state with username and profile/onboarding state.
- Produces `/onboarding/profile` screen for authenticated users missing minimum profile.

- [ ] Write tests proving authenticated incomplete users route to `/onboarding/profile`, completed users are not trapped, anonymous users still route to Sign In via existing auth guard, and safe returnUrl behavior remains intact.
- [ ] Implement profile onboarding screen with first/last name only, generated API adapter, and CurrentUserStore refresh/update.
- [ ] Verify external component files and accessibility roles/status for loading/error/success states.

## Task 8: Frontend Admin Users role management UI

**Interfaces:**
- Consumes protected backend list/detail/role-change APIs and CurrentUserStore permissions.
- Produces Admin user-management UI under approved Admin route(s) without frontend-only role switching.

- [ ] Write focused tests proving unauthorized users cannot access the UI route, authorized Admin can search/select user, roles are shown from backend, and role change submits protected backend role IDs/names per generated contract.
- [ ] Implement external-template/style components following existing shared form controls, validation summary, loading/error/retry, and Angular Material theme rules.
- [ ] Verify UI does not expose password hashes, internal tokens, or localStorage role authority.

## Task 9: Integrated verification, review, and atomic commits

**Interfaces:**
- Produces final evidence and local commits only; no push.

- [ ] Run backend build and relevant/full backend tests required by changed areas.
- [ ] Run frontend focused suites, `npm run quality`, and `npm run build-storybook` if stories/UI changed.
- [ ] Run `git diff --check`, staged-area checks, generated-file hand-edit checks, and excluded-artifact scope checks.
- [ ] Run Playwright MCP journeys: public sign-up + email verification + onboarding; sign-in completed user; public cannot choose privileged role; admin role change; regression for token storage/no localStorage.
- [ ] Dispatch independent deep review for security/auth/authorization/data migration findings.
- [ ] Apply at most one bounded correction round for material findings.
- [ ] Update `PROGRESS.md` and `.agent/goal-state.md` with exact final evidence.
- [ ] Create authorized atomic commits in logical order: backend/domain/API/migration, OpenAPI/generated-client, frontend UI/routing, final verification/docs if needed. Use exact-file staging only; never `git add .`; do not push.

## Plan Self-Review

- Spec coverage: Product decisions and security invariants are mapped across Tasks 1-9.
- Placeholder scan: No `TBD`, `TODO`, or unspecified future work remains.
- Type consistency: Backend fields and frontend state use `Username`/`NormalizedUsername` and minimum profile/onboarding state consistently; exact DTO names may follow existing repository naming during implementation but must be reported in worker evidence.
- Risk controls: Backend scout incompleteness is explicitly handled by requiring implementation worker targeted verification for migrations/tests/DbContext/OpenAPI before code changes.
