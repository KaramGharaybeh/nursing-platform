# Frontend Project Rules

## 1. Authority and scope

This is the operating contract for future frontend work. It consolidates task-level rules without replacing the source documents that own their subject matter. When sources conflict, stop work and resolve the conflict; agent inference is never authority.

For frontend task execution, consult the following in order, together with explicit current decisions from Karam:

1. `PROJECT_RULES.md`
2. `AGENTS.md`
3. `docs/frontend/frontend-architecture.md`
4. `docs/frontend/design/GOAL_STATE.md`
5. `docs/frontend/design/MASTER_PLAN.md`
6. `docs/frontend/design/governance/source-authority.md`
7. `docs/frontend/design/governance/decision-log.md`
8. `docs/frontend/design/governance/open-questions.md`
9. Approved Penpot evidence; Figma is non-authoritative unless Karam explicitly changes the recorded Penpot-only decision.
10. Implemented backend source and the generated Development OpenAPI contract for runtime API behavior.

Unresolved open questions block the affected implementation decision. Historical exports, trackers, prose, and uncommitted files are evidence only unless explicitly approved by the authority that owns the decision.

## 2. Pre-implementation gates

No Angular implementation, including scaffolding, may start until all of the following are explicitly satisfied in approved task scope:

- Karam has accepted G0.
- The current backend/API revision and generated Development OpenAPI artifact are captured.
- Request and response DTO contracts are identified from current implementation/OpenAPI.
- Problem Details, validation, and relevant `422` behavior are documented from evidence.
- A route registry and route/actor/permission matrix are drafted and approved for the intended slice.
- Design tokens and their source authority are accepted.
- Component contracts are drafted for the intended slice.
- Penpot source status and approval status are confirmed.
- The npm/package-manager policy and Angular project-creation command are explicitly approved.

G0 was accepted on 2026-08-12 as a governance/re-entry baseline only, as recorded in `docs/frontend/design/governance/decision-log.md` (DEC-PH0-016) and `docs/frontend/design/GOAL_STATE.md`. A later implementation approval must not be inferred from this document, a running Penpot container, a draft board, or backend readiness.

## 3. Angular stack rules

The approved planned stack is Angular 22, TypeScript strict mode, standalone architecture, Signals, RxJS, Angular Router, Angular Material, Angular CDK only where needed, SCSS, and one project-owned Angular Material theme mapped from approved Penpot tokens. The application must remain responsive for desktop, tablet, and mobile, preserve future Arabic/RTL readiness, and meet WCAG 2.2 AA.

- npm is the approved package manager; do not substitute another package manager.
- Angular CLI, Angular Material, and Angular CDK must match the pinned Angular major.
- Pin a Node.js version that the official Angular 22 compatibility table supports, consistently across local development, `.nvmrc`, `package.json` engines, and CI.
- Preserve Angular 22 zoneless-compatible defaults. Do not add `zone.js`, legacy global state libraries, another UI library, Tailwind, or a toast library without an approved architecture decision.
- Use official Angular guidance matching the pinned version. Do not rely on stale framework knowledge.

## 4. Project structure rules

The future workspace must be feature-based under `src/app` and preserve these ownership boundaries:

```text
src/app/core/       application-wide infrastructure
src/app/shared/     business-neutral reusable UI and utilities
src/app/features/   feature-owned routes, pages, state, forms, and API adapters
```

- `core` owns auth state, token lifecycle, functional HTTP interceptors, guards, global error/feedback infrastructure, configuration, and shell infrastructure; it must not own feature workflows.
- `shared` owns reusable business-neutral components, directives, pipes, and utilities; it must not depend on `core` or features, and must not own feature APIs, state, or business logic.
- Each feature owns its routes, lazy-loaded pages, components, forms, local state, models, validators, and API adapters. Features may use only stable public Core/Shared APIs and their own internals.
- Generated clients, if approved, remain isolated, are never manually edited, and contain no component or feature-state concerns. Feature adapters contain feature-specific workflow mapping.
- Layout and routing compose stable feature entry points. Route guards improve navigation only; they do not enforce security.
- Keep business workflows out of templates, generic components, Material wrappers, route definitions, interceptors, and low-level clients. Components remain presentation-focused; owning feature services/state coordinate business behavior.
- Use focused filenames, one primary Angular concept per file, hyphenated names, and colocated `.spec.ts` tests. Avoid catch-all `helpers`, `utils`, and `common` files, private barrel exposure, and circular dependencies.

This section defines rules only; it does not create the directory structure.

## 5. Security rules

- The backend is authoritative for authentication, authorization, ownership, exam access, scoring, payments, entitlements, and all protected data.
- For the approved local MVP posture, keep access tokens in memory and refresh tokens only in the centralized authentication abstraction backed by `sessionStorage`; do not use `localStorage` for tokens. Production token storage requires separate security approval.
- Never expose tokens, secrets, password data, payment data, private identifiers, uploaded files, protected exam content, or raw API payloads in logs, analytics, URLs, query strings, browser errors, snack bars, or user-facing error messages.
- Authentication starts in an explicit bootstrap state. A stored refresh token triggers one coordinated refresh; after success, hydrate user identity, roles, and permissions through `GET /api/v1/me`. Network/5xx bootstrap failure is not automatically an authentication failure.
- Permit at most one in-flight refresh per tab. A failed refresh or a second post-refresh `401` clears local state and resolves deterministic logout. Login/refresh requests do not self-trigger refresh; mutation requests are not blindly replayed.
- UI roles, permissions, guards, hidden controls, cached state, JWT claims, `isFree`, and `canStart` are visibility/navigation aids only. They never replace backend authorization or authorize a protected action.
- Do not expose official exam answers, correct options, rationales, answer keys, hidden scores, or review data until an approved backend contract returns permitted data. Analytical reports must not reveal protected exam content. Practice content and progress must remain isolated from official exam content and report evidence.
- Treat no-store responses, checkout data, and sensitive server results as short-lived owning-workflow state; do not persist them in browser storage or application caches.

## 6. Business rules

- Use only current API evidence for the preparation-package catalog, offers, and entitlement presentation. Do not infer availability, eligibility, price, currency, or access-window rules.
- An active same-package entitlement blocks a new purchase of that stable package; the backend decides and the UI must represent its documented conflict outcome. Different packages containing the same exam may coexist.
- Entitlement rights are independently backend-authorized. Materials, practice, package attempt, and report presentation must not imply rights the current contract does not provide.
- Practice answers may be re-answered only while package access is active; historical owner reads may remain after expiry. Retry must not be represented as consuming the package exam attempt.
- Package attempt start requires the nurse's explicit entitlement selection and is backend-controlled. An in-progress qualifying session may resume idempotently; source-conflict and consumed-attempt outcomes must remain visible and meaningful.
- Package analytical reports are nurse-owned, generated/read according to the backend contract, and remain readable after entitlement expiry when the backend permits it.
- Preserve free and standalone paid-exam behavior. A package purchase is not a standalone access grant, and standalone/free flows must not silently consume package rights.
- Do not design or implement employer package visibility, material storage/upload/download/delivery, offline access, workspace/dashboard aggregation, adaptive practice, spaced repetition/retraining, or cross-package progress behavior as implemented features. They remain deferred until separately approved.

## 7. API and error-handling rules

- Use generated clients only after generator approval; otherwise use confirmed typed contracts and feature adapters. Never guess DTO fields, enum values, validation limits, permission names, response shapes, or endpoint behavior.
- Record the backend revision and Development OpenAPI capture used by each implementation slice. If source, OpenAPI, and prose conflict, stop and resolve the discrepancy.
- Centralize HTTP behavior. Use functional interceptors with deliberate ordering; interceptors must not own feature business logic or silently retry mutations.
- Map RFC 7807 Problem Details through status, documented structured extensions, and endpoint semantics—not human-readable `title` or `detail`. Do not expose raw exception details.
- Map field validation errors to the relevant form controls and preserve form-level errors in a persistent summary when needed. Confirm the runtime validation and `422` mapping before implementation.
- Treat `401` through the single-flight refresh/logout policy; `403` as unauthorized; ownership-hidden `404` as generic not found; `409` as a meaningful conflict without blind retry; `429` with documented `Retry-After`; and `503` with contextual manual or approved bounded recovery.
- Provide explicit loading, ready, empty, validation, conflict, unauthorized, not-found, rate-limit, and error states. Do not silently swallow failures.
- `POST`, `PUT`, `PATCH`, and `DELETE` are never generally retried. Payment, checkout, access, and exam mutations have no blind retry. When an approved endpoint requires an idempotency key, the owning feature generates an opaque cryptographically strong key once per logical operation and safely reuses it only for that operation.

## 8. Design system rules

- Use approved design tokens before implementation. Do not introduce arbitrary colors, spacing, radii, shadows, typography values, z-indexes, breakpoints, or motion values.
- The project-owned Material theme and SCSS are executable mappings of approved Penpot intent, not alternate visual authorities. Emit theme CSS once at the application boundary.
- Customize Angular Material only through supported theming APIs, approved tokens, CSS custom properties where appropriate, documented component APIs, and narrowly owned application classes. Do not use `::ng-deep`, undocumented Material DOM/classes, or a global `MaterialModule`.
- Typography, spacing, radius, elevation, focus, disabled, and state values must follow accepted token contracts. Components use semantic token names rather than raw literals.
- Component contracts must cover purpose, anatomy, allowed variants/sizes, inputs/outputs, semantic/ARIA behavior, responsive and RTL behavior, and hover, focus, active, disabled, error, loading, empty, and success states as applicable.
- Implement only approved Penpot visual evidence. Any necessary deviation requires explicit design/engineering approval and a corresponding decision-log entry; do not silently alter visual intent.
- Current design-system evidence is not yet implementation-ready: Utilities, canonical token registry, breakpoint authority, local Penpot components/colors/themes, Arabic validation, elevation/z-index, patterns, and component implementation contracts remain unresolved or deferred.

### 8.1 Spacing Grid and Utilities Rules

All spacing MUST follow a **4px Base Grid** system. Component-level padding MAY use **2px Precision Increments** only for tight visual adjustments such as badges, chip inputs, and inline labels; this exception MUST NOT expand into general layout spacing.

- Base spacing tokens: 4px, 8px, 12px, 16px, 20px, 24px, 28px, 32px, 36px, 40px, 44px, 48px, 52px, 56px, 60px, 64px.
- Precision 2px tokens (optical only): 2px, 6px, 10px, 14px, 18px, 22px, 26px, 30px.
- No arbitrary spacing values outside this scale are permitted.
- All utility spacing classes MUST use the `!important` flag in `src/styles/_utilities.scss` to ensure consistent override behavior.

Text truncation and overflow MUST follow WCAG 2.2 AA requirements:

- Single-line truncation MUST use `text-overflow: ellipsis` with `white-space: nowrap` and `overflow: hidden`.
- Multi-line truncation MUST use `-webkit-line-clamp` with explicit line count.
- Truncated content MUST remain accessible to screen readers and keyboard focus.
- A `u-visually-hidden` utility class MUST be used for screen-reader-only content rather than `display: none`.

### 8.2 SCSS Architecture Files

The following SCSS files constitute the canonical global styling foundation. No component, feature, or shared code may redefine or duplicate the token or utility definitions within them:

| File | Ownership |
|------|-----------|
| `src/styles/_tokens.scss` | Penpot-derived canonical design tokens — **single source of truth** for all colors, typography, and elevation values. |
| `src/styles/_material-theme-bridge.scss` | Maps `_tokens.scss` values into Angular Material 22 component palettes. Prevents ad-hoc Material overrides. |
| `src/styles/abstracts/_mixins.scss` | Project-wide SCSS mixins including `touch-target($mobile)`. |
| `src/styles/_utilities.scss` | 4px grid spacing utilities, 2px precision helpers, text truncation, and WCAG accessibility helper classes. |

## 9. Accessibility rules

- WCAG 2.2 AA is mandatory from the first implementation slice.
- Use semantic landmarks, one clear page heading, logical reading/focus order, keyboard-complete interaction, visible focus, accessible names/descriptions, and non-color-only communication.
- Associate form labels, instructions, and field/form errors with their controls. Maintain meaningful focus movement and announce relevant status changes without stealing focus.
- Meet contrast requirements. **44 × 44px** is the absolute minimum for all desktop pointer interactive targets. **48 × 48px** is the absolute minimum for all mobile/touch-screen viewports. All interactive elements MUST comply from the first implementation slice. The mandatory SCSS implementation is `@mixin touch-target($mobile: false)` located in `src/styles/abstracts/_mixins.scss`.
- Respect reduced-motion preferences when animation exists. Dialogs, menus, overlays, tables, feedback, and errors require explicit keyboard, focus, screen-reader, and recovery behavior.
- Automated AXE coverage supplements—not replaces—manual keyboard, focus, screen-reader, contrast, zoom/reflow, and real-browser checks.
- Preserve Arabic/RTL readiness through logical layout properties, direction-aware iconography, script-aware content/typography, and text expansion tolerance. Full localization and RTL boards remain deferred unless approved.

## 10. Responsive and RTL rules

- Product implementation must support desktop, tablet, and mobile behavior; the current Penpot design-documentation program is Desktop-only and does not itself approve responsive boards.
- Do not build a desktop-only screen. Define and approve breakpoint tokens and behavior before implementing responsive layouts; do not invent media-query values.
- Use logical CSS properties and start/end alignment. Avoid physical left/right assumptions, fixed text widths, direction-insensitive spacing, and non-mirroring directional icons.
- Design and test for Arabic text expansion, mixed-direction numbers/punctuation, and content that changes density or line wrapping. Do not claim Arabic/RTL support based on empty or legacy boards.

### 10.1 CSS Directional Isolation Policy

Hardcoded physical CSS directional properties are **STRICTLY BANNED** from the first implementation slice, even when the current scope is English-only and desktop-only. This policy exists to prevent RTL conversion debt.

**Banned properties (MUST NOT appear in any SCSS or component styles):**

- `margin-left`, `margin-right`
- `padding-left`, `padding-right`
- `left`, `right` (for positioning)
- `float: left`, `float: right`
- `text-align: left`, `text-align: right`

**Required replacements (MUST be used instead):**

| Banned | Logical Replacement |
|--------|-------------------|
| `margin-left` | `margin-inline-start` |
| `margin-right` | `margin-inline-end` |
| `padding-left` | `padding-inline-start` |
| `padding-right` | `padding-inline-end` |
| `left` | `inset-inline-start` |
| `right` | `inset-inline-end` |
| `float: left` | `float: inline-start` |
| `text-align: left` | `text-align: start` |

Stylelint MUST be configured with `property-disallowed-list` to reject any code containing banned physical directional properties. Any exception requires an explicit architecture decision with justification and `stylelint-disable` annotation.

## 11. Penpot and MCP rules

- Penpot is the visual design authority only when its relevant evidence is approved. A running local service, export, geometry read, or tracker checkbox does not approve a design.
- Do not modify Penpot, create boards, import/export files, or use MCP write operations without explicit task authorization and the applicable design gate. Use read-only inspection only when the task permits it.
- Record a design discrepancy in the applicable governance open-question or decision-log process; do not correct it by inference. Penpot never supplies backend/API facts.
- Current observed local services are Penpot frontend at `http://localhost:9001`, Open Design at `127.0.0.1:7456`, and a running Penpot MCP container. These are local runtime facts, not approval or authorization.
- Penpot execution requires an approved page specification, frozen supporting contracts, explicit target IDs, a bounded task packet, and the applicable Penpot gate. Existing Page 07, Page 09, Page 10, and AUTH-001 discrepancies remain governed evidence, not implementation input.

## 12. Agent task rules

- Read the current task and all relevant authority, API, design, and business documentation before changing files. Load required skills before acting.
- Obey the current task boundary exactly; make no unrelated edits, preparatory scaffolding, or deferred-feature work.
- Use deterministic repository/runtime evidence. Report exact files changed and execute the validation requested by the task.
- Stop and ask for clarification when a route, permission, DTO, validation rule, business transition, visual decision, or security behavior is not evidenced.
- Do not stage, commit, push, delete branches, or alter repository history unless explicitly instructed. Never use `git add .`.
- Do not use reviewers, subagents, deep review, model-based review, or DeepSeek when the active task forbids them. Task-specific restrictions take precedence; no agent may delegate authorization or final acceptance.
- Do not claim completion without the requested verification evidence, clean scope confirmation, unstaged-file status, and explicit distinction between facts, decisions, and open questions.

## 13. Documentation and tracking rules

- Update the single authoritative document for a fact; do not duplicate detailed architecture, API, design, or status rules.
- Update `CURRENT_TASK.md` and `TASKS.md` only when the task explicitly authorizes it and only after verified scope completion. Never use them to imply a feature is implemented because a design or plan exists.
- Update design governance files only in an explicitly authorized governance task: `GOAL_STATE.md` owns program state, `MASTER_PLAN.md` owns gates/plan, `source-authority.md` owns evidence snapshots, `decision-log.md` owns approved decisions, and `open-questions.md` owns unresolved discrepancies.
- Treat screen trackers such as AUTH-001 as legacy/draft evidence unless the governing task explicitly authorizes reconciliation. Do not overwrite them to match an assumption.
- Reports record bounded audit evidence and do not supersede source documents. Maintain exact revision/source references where a report depends on live backend or Penpot evidence.
- Documentation/status changes must be logically scoped. If commits are later authorized, separate a documentation-only/status update from implementation when combining them would obscure review, provenance, or verification; do not create any commit without explicit instruction.

## 14. Testing rules

Future approved frontend work must include proportional tests for components and services, feature state, route guards, auth/token storage, single-flight refresh, interceptors, typed API adapters, Problem Details mapping, form validation, permission-aware presentation, payment/exam trust boundaries, feedback/error behavior, and design-token/Material-theme integration.

- Use Vitest and Angular TestBed/official testing utilities for unit and component tests. Use official HTTP testing providers and `HttpTestingController` for client/interceptor unit tests.
- Add accessibility checks, including AXE where applicable, plus manual keyboard, focus, screen-reader, responsive, and design-state verification.
- Cover loading, empty, validation, authorization, conflict, rate-limit, and failure states that the approved feature contract makes applicable.
- Do not add E2E tests, Playwright/browser execution, visual regression tooling, or real-backend integration tests unless explicitly authorized by the applicable task/specification. Do not introduce Cypress.

## 15. Explicitly forbidden without authorization

- Create or scaffold the Angular project.
- Install dependencies or change npm/package-manager policy.
- Generate an API client or select an OpenAPI generator.
- Modify Penpot or any Figma artifact (Figma is non-authoritative), import/export design files, or create boards/page specifications.
- Implement screens, components, routes, theme code, or frontend runtime behavior.
- Modify backend source, tests, API behavior, authentication, authorization, or security posture.
- Implement deferred storage/delivery, offline, workspace, adaptive, retraining, or employer package features.
- Stage, commit, push, delete branches, reset, clean, stash, or broadly stage files.

## 16. Next required decisions

Before frontend project creation, obtain explicit decisions for:

1. G0 acceptance by Karam.
2. The final approved Angular project-creation command and its npm assumptions.
3. The Angular-22-compatible Node.js/npm versions and whether SSR is excluded or separately approved.
4. The generated Development OpenAPI capture command, source revision, and API client-generation strategy.
5. The canonical route registry and initial approved design/implementation slice.
6. The accepted design-token source, token registry, Material-theme mapping, component contracts, breakpoints, and responsive/RTL rules.
7. The Penpot update/write policy after the required design gates.
8. The scoped review and commit strategy for the current uncommitted architecture, legacy tracker, design-system audit, and report files.

Until these decisions and pre-implementation gates are satisfied, frontend work remains documentation/evidence-only.
