# Frontend Project Rules

## 1. Authority and scope

This is the operating contract for future frontend work. It consolidates task-level rules without replacing the source documents that own their subject matter. When sources conflict, stop work and resolve the conflict; agent inference is never authority.

For frontend task execution, consult the following in order, together with explicit current decisions from Karam:

1. `PROJECT_RULES.md`
2. `AGENTS.md`
3. Current execution state and task/gate authority in `docs/frontend/execution/frontend-implementation-ledger.md`, with `PROGRESS.md` as concise current-session handoff.
4. `docs/frontend/frontend-architecture.md`
5. `docs/frontend/design/frontend-design-foundation-reference.md` for the canonical textual implementation mapping of the approved frontend foundation; the 2026-09-20 human decision makes Google Stitch the active visual-design workspace for the system-wide redesign. Approved Stitch screens own new visual composition after human approval; existing Penpot artifacts are legacy/reference evidence unless explicitly re-approved. Storybook is the intended future production visual development/review surface, not requirements authority, and is not installed until a separate tooling authorization.
6. Implemented backend source and the generated Development OpenAPI contract for runtime API behavior.
7. `docs/frontend/design/GOAL_STATE.md`
8. `docs/frontend/design/MASTER_PLAN.md`
9. `docs/frontend/design/governance/source-authority.md`
10. `docs/frontend/design/governance/decision-log.md`
11. `docs/frontend/design/governance/open-questions.md`

Unresolved open questions block the affected implementation decision. Historical exports, trackers, prose, and uncommitted files are evidence only unless explicitly approved by the authority that owns the decision. Historical planning/design documents remain preserved but do not override newer verified execution state or current repository facts merely because they contain older statements such as “frontend workspace is not initialized.” If precedence cannot resolve a conflict deterministically, STOP AND ESCALATE.

Architectural, business, design, API, security, or exception decisions that affect future implementation MUST be persisted in repository-backed documentation. Chat history alone is not a durable project decision record.

Generic skills, including design, brand, design-system, ui-styling, ui-ux-pro-max, brainstorming, or similar capabilities, are subordinate to Nursing Platform repository governance, approved visual evidence, backend/OpenAPI contracts, business rules, security, accessibility, and task scope. A generic skill MUST NOT silently introduce Tailwind, shadcn, a second UI library, new design tokens, new business behavior, new architecture, or a conflicting visual decision.

Official Angular documentation and Angular-maintained AI guidance are framework guidance only. They are subordinate to explicit Nursing Platform decisions, repository governance, backend/OpenAPI business and security contracts, approved visual evidence, and active task scope. For Angular framework implementation questions, prefer current official Angular documentation and official Angular-maintained AI guidance compatible with the project's locked Angular major/version, including:

- `https://angular.dev/ai/develop-with-ai`
- `https://angular.dev/ai/agent-skills`
- `https://angular.dev/ai/mcp`
- `https://angular.dev/cli/new`

Do not treat generic web tutorials, StackOverflow answers, generic coding skills, or model memory as higher authority than official Angular documentation. The project targets Angular 22. Do not silently adopt APIs or behavior from a later Angular major. If current Angular documentation describes a patch-sensitive API or CLI behavior whose availability in the installed/pinned Angular version is uncertain, verify it against the actual installed/pinned version, do not upgrade Angular merely to use it, and STOP AND ESCALATE if the desired guidance is incompatible with the locked project version.

## 2. Pre-implementation gates

No Angular implementation, including scaffolding, may start until all of the following are explicitly satisfied in approved task scope:

- Karam has accepted G0.
- The current backend/API revision and generated Development OpenAPI artifact are captured.
- Request and response DTO contracts are identified from current implementation/OpenAPI.
- Problem Details, validation, and relevant `422` behavior are documented from evidence.
- A route registry and route/actor/permission matrix are drafted and approved for the intended slice.
- Design tokens and their source authority are accepted.
- Component contracts are drafted for the intended slice.
- Stitch/design source status and approval status are confirmed when materially new visual intent is unresolved; routine work may instead cite approved visual foundations and approved existing component/layout patterns.
- The npm/package-manager policy and Angular project-creation command are explicitly approved.

G0 was accepted on 2026-08-12 as a governance/re-entry baseline only, as recorded in `docs/frontend/design/governance/decision-log.md` (DEC-PH0-016) and `docs/frontend/design/GOAL_STATE.md`. A later implementation approval must not be inferred from this document, a running Penpot container, a draft board, or backend readiness.

### 2.1 Standing Implementation Authorization

Standing Implementation Authorization for ordinary eligible Low/Medium frontend Tasks is defined in `docs/frontend/execution/frontend-implementation-ledger.md` and `docs/development/model-orchestration.md`. It may replace a separate per-task start message only when all declared predecessor Gates are `VERIFIED`, exact scope is established, no unresolved blocker/approval/security/business/design/tooling/dependency/backend/OpenAPI/Penpot/Storybook/database/migration decision is required, risk remains Low/Medium, implementation can stay within bounded `ALLOWED_FILES`, and required focused plus full Gate evidence can be produced. It never authorizes staging, committing, pushing, dependency changes, database changes, migrations, backend/OpenAPI/Penpot mutation, Storybook installation/configuration, screen work without screen approval, or human-owned product/business decisions.

## 3. Angular stack rules

The approved planned stack is Angular 22, TypeScript strict mode, standalone architecture, Signals, RxJS, Angular Router, Angular Material, Angular CDK only where needed, SCSS, and one project-owned Angular Material theme mapped from approved design tokens. The application must remain responsive for desktop, tablet, and mobile, preserve future Arabic/RTL readiness, and meet WCAG 2.2 AA.

- npm is the approved package manager; do not substitute another package manager.
- Angular CLI, Angular Material, and Angular CDK must match the pinned Angular major.
- Pin a Node.js version that the official Angular 22 compatibility table supports, consistently across local development, `.nvmrc`, `package.json` engines, and CI.
- Preserve Angular 22 zoneless-compatible defaults. Do not add `zone.js`, legacy global state libraries, another UI library, Tailwind, or a toast library without an approved architecture decision.
- Use official Angular guidance matching the pinned version. Do not rely on stale framework knowledge.
- Use standalone Angular architecture. Do not explicitly write redundant `standalone: true` metadata inside new component/directive decorators for Angular 22. Do not introduce NgModules for new feature architecture unless a concrete compatibility requirement is documented and approved.
- Do not mechanically add `changeDetection: ChangeDetectionStrategy.OnPush` to every new component when Angular 22 already supplies the intended default behavior. Do not perform change-detection modernization outside active Task scope.
- Prefer `inject()` over constructor injection for new Angular code. Keep services narrowly responsible.
- Prefer signal-based component APIs: `input()`, `output()`, and `model()`. Use `model()` for genuine component two-way binding instead of manually pairing an input and output.
- Use signals for local/component state where appropriate. Use `computed()` for derived read-only state and `linkedSignal()` when writable/derived state must remain synchronized with reactive source changes and that behavior is semantically appropriate. Do not use signal `mutate`; use `set()` and `update()` with predictable, pure state transformations. Do not introduce a global state library merely because signals exist.
- Use Angular native control flow (`@if`, `@for`, `@switch`) for new templates when appropriate. Keep business/workflow logic out of templates. Use `async` pipe for Observable template consumption where appropriate rather than unmanaged manual subscriptions. Do not assume JavaScript globals are directly available inside Angular templates.
- Prefer Angular class/style bindings (`[class...]`, `[style...]`) for new code. Do not use `ngClass` or `ngStyle` as the default approach when normal bindings express the behavior.
- Use component/directive `host` metadata for host bindings and listeners in new code rather than `@HostBinding` or `@HostListener` by default.
- Feature routes should be lazy-loaded by default when they represent independently navigable feature boundaries. Do not lazy-load merely to satisfy a rule where it makes route architecture worse. Canonical Nursing Platform route registry and permission governance remain authoritative.
- Use `NgOptimizedImage` for appropriate static images. Do not apply it blindly to unsupported cases such as inline base64 images. Privacy/security rules still apply to user/private imagery.
- Do not upgrade, replace, modernize, or rewrite an already approved frontend approach merely because another Angular pattern, dependency, API, or technique is newer or personally preferred. Changes to approved architecture require an explicit decision task.

### 3.1 Official Angular Agent Skills and CLI MCP

Angular publishes official Agent Skills including `angular-developer` and `angular-new-app`. If an official Angular-maintained Agent Skill is already available in the execution environment, the Agent should use it when relevant to Angular work. Do not install an Agent Skill as part of a normal implementation Task, and do not run `npx skills add` without explicit tooling/dependency authorization. Angular skills are advisory and subordinate to Nursing Platform governance; they cannot authorize architecture, dependency, UI-library, business-rule, backend-contract, or design changes. Absence of an Angular Agent Skill is not by itself a project blocker.

Angular provides an official Angular CLI MCP server. When Angular CLI MCP is already available to the Agent, prefer its read-only framework-information capabilities for Angular-specific questions, especially `get_best_practices`, `search_documentation`, and `list_projects`. For normal Nursing Platform work, read-only usage is preferred unless the active Task explicitly authorizes a write-capable MCP operation. Do not add MCP configuration files to the repository, install/configure MCP globally, run migration/modernization/write-capable MCP tools merely because they are available, or use MCP to widen Task scope unless explicitly authorized. If Angular MCP is unavailable, use official `angular.dev` documentation instead. MCP availability is not a default implementation blocker.

### 3.2 `@Service()` Compatibility Check

Current Angular v22 documentation recommends the new `@Service()` decorator for new singleton services. Do not make this a blindly applied project rule before the actual installed `@angular/core` version is verified after scaffold. After `T-FE-001` creates the Angular workspace, verify whether the installed Angular 22 package version exposes the official `Service` decorator. If confirmed compatible, prefer `@Service()` for new singleton services according to Angular v22 guidance. If not available in the installed locked version, use the supported documented singleton-service mechanism, do not upgrade Angular merely for `@Service`, and record the compatibility result. This check belongs to later Angular coding/toolchain verification, not governance-only documentation work.

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
- Each business workflow belongs to one owning feature. Features MUST NOT import another feature's internal files. Cross-feature behavior MUST go through an intentionally stable public, shared, or core abstraction. Shared MUST remain business-neutral, Core MUST NOT become a dumping ground, and generic catch-all `utils/` directories are forbidden.
- Generated clients, if approved, remain isolated, are never manually edited, and contain no component or feature-state concerns. Feature adapters contain feature-specific workflow mapping.
- Layout and routing compose stable feature entry points. Route guards improve navigation only; they do not enforce security.
- Keep business workflows out of templates, generic components, Material wrappers, route definitions, interceptors, and low-level clients. Components remain presentation-focused; owning feature services/state coordinate business behavior.
- Use focused filenames, one primary Angular concept per file, hyphenated names, and colocated `.spec.ts` tests. Avoid catch-all `helpers`, `utils`, and `common` files, private barrel exposure, and circular dependencies.
- Ordinary production Angular components under `frontend/src/app` MUST use separated colocated component files by default: component TypeScript logic and metadata in the component `.ts` file, rendered markup in a colocated external `.html` file referenced via `templateUrl`, component styling in a colocated external `.scss` file referenced via `styleUrl` or another explicitly approved external style metadata form, and a focused colocated `.spec.ts` file when behavior or rendering is testable. Inline `template:`, inline `styles:`, `<style>` blocks inside component templates, and embedding component HTML or SCSS inside TypeScript merely because Angular permits it are prohibited for ordinary production components. A trivial template is not a default exception.
- Do not create artificial template or style files for Angular concepts that do not render component UI, including directives, pipes, services, interceptors, guards, and other purely programmatic infrastructure. Generated API code remains governed by generated-code isolation and MUST NOT be manually edited to satisfy component file-structure rules. Test-only host templates MAY remain inline inside `.spec.ts` files when they are local test fixtures. A production component that intentionally has no stylesheet MAY omit the SCSS file only when the task records an explicit styleless-component rationale. Any production component that renders markup but proposes no external HTML template requires explicit architecture or task approval. If a proposed exception is not covered here, STOP AND ESCALATE rather than inventing one.

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
- Backend/OpenAPI transport DTOs MUST be represented accurately first. Do not invent response fields, silently change nullability, reinterpret enums, or alter date/time semantics. Feature adapters and view models MAY transform valid transport data for presentation, but presentation models MUST NOT pretend the backend supplied data it did not.
- Generated API code MUST remain isolated and MUST never be manually edited. Generator output that misrepresents the canonical OpenAPI contract causes STOP AND ESCALATE. Handwritten duplicate DTO contracts require explicit exception approval.
- Record the backend revision and Development OpenAPI capture used by each implementation slice. If source, OpenAPI, and prose conflict, stop and resolve the discrepancy.
- Centralize HTTP behavior. Use functional interceptors with deliberate ordering; interceptors must not own feature business logic or silently retry mutations.
- Map RFC 7807 Problem Details through status, documented structured extensions, and endpoint semantics—not human-readable `title` or `detail`. Do not expose raw exception details.
- Map field validation errors to the relevant form controls and preserve form-level errors in a persistent summary when needed. Confirm the runtime validation and `422` mapping before implementation.
- Treat `401` through the single-flight refresh/logout policy; `403` as unauthorized; ownership-hidden `404` as generic not found; `409` as a meaningful conflict without blind retry; `429` with documented `Retry-After`; and `503` with contextual manual or approved bounded recovery.
- Provide explicit loading, ready, empty, validation, conflict, unauthorized, not-found, rate-limit, and error states. Do not silently swallow failures.
- `POST`, `PUT`, `PATCH`, and `DELETE` are never generally retried. Payment, checkout, access, and exam mutations have no blind retry. When an approved endpoint requires an idempotency key, the owning feature generates an opaque cryptographically strong key once per logical operation and safely reuses it only for that operation.

## 8. Design system rules

- Use approved design tokens before implementation. Do not introduce arbitrary colors, spacing, radii, shadows, typography values, z-indexes, breakpoints, or motion values.
- Before Angular UI, SCSS, Angular Material theme, component, or screen work, consult `docs/frontend/design/frontend-design-foundation-reference.md`. If live Stitch redesign evidence, re-approved Penpot evidence, or that reference contradict each other, STOP and obtain design resolution before implementation.
- Canonical keyboard focus ring: `focus.ring.width = 2px`, `focus.ring.offset = 4px`, and `focus.ring.shadow = 0 0 0 4 #006B66`. `border.focus = #4F46B8` is a distinct focused-border/accent token and MUST NOT be substituted for the keyboard focus ring unless a component contract explicitly uses it.
- Canonical standard form-field/select foundation: `height = 64px`, `radius = 12px`, and minimum trailing action target `48 × 48px`. The 48px Preparation Package filter draft does not establish a compact control variant. A compact field/select variant may be created only through a later explicit design-system decision.
- There are currently no approved canonical project motion duration/easing tokens. Do not invent or hard-code project-authored motion duration or easing values. Until canonical motion tokens are approved, prefer no custom decorative transition over arbitrary animation. Required state meaning must remain complete without animation. Framework-internal behavior is not visual authority and must not be copied into project tokens.
- Do not invent a project numeric z-index scale. Angular CDK Overlay owns overlay stacking mechanics unless an evidenced implementation conflict requires an explicit project token. Project-owned elevation remains semantic: level-1 raised, level-2 menus/overlays, level-3 dialogs/modals, and scrim opacity `0.48`. Any custom z-index value requires an explicit architecture/design decision.
- The project-owned Material theme and SCSS are executable mappings of approved visual intent, including approved visual foundations, approved Stitch redesign artifacts, and explicitly re-approved legacy Penpot/design artifacts, not alternate visual authorities. Emit theme CSS once at the application boundary.
- Customize Angular Material only through supported theming APIs, approved tokens, CSS custom properties where appropriate, documented component APIs, and narrowly owned application classes. Do not use `::ng-deep`, undocumented Material DOM/classes, or a global `MaterialModule`.
- Typography, spacing, radius, elevation, focus, disabled, and state values must follow accepted token contracts. Components use semantic token names rather than raw literals.
- Component contracts must cover purpose, anatomy, allowed variants/sizes, inputs/outputs, semantic/ARIA behavior, responsive and RTL behavior, and hover, focus, active, disabled, error, loading, empty, and success states as applicable.
- Implement only approved visual intent from approved visual foundations, approved design specifications, approved Stitch redesign artifacts, and any explicitly re-approved legacy Penpot/design artifacts. Any necessary deviation requires explicit design/engineering approval and a corresponding decision-log entry; do not silently alter visual intent.
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
| `src/styles/_tokens.scss` | Approved visual-foundation design tokens, including Penpot-derived foundation values — **single source of truth** for all colors, typography, and elevation values. |
| `src/styles/_material-theme-bridge.scss` | Maps `_tokens.scss` values into Angular Material 22 component palettes. Prevents ad-hoc Material overrides. |
| `src/styles/abstracts/_mixins.scss` | Project-wide SCSS mixins including `touch-target($mobile)`. |
| `src/styles/_utilities.scss` | 4px grid spacing utilities, 2px precision helpers, text truncation, and WCAG accessibility helper classes. |

### 8.3 Storybook Visual Development and Review Rules

Storybook is adopted as the intended frontend visual development and review tooling model for production Angular components and production screen states. Storybook installation, configuration, dependencies, scripts, stories, and visual-regression integration are not authorized until a separate tooling implementation authorization explicitly approves them.

- Stories MUST render the same production Angular components used by the application. Do not create Storybook-only component copies, duplicate markup, duplicate SCSS, alternate implementations, parallel token definitions, or a second design system.
- Storybook is not authoritative for business behavior, backend contracts, OpenAPI, DTOs, routes, authentication, roles, permissions, security, validation semantics, payment behavior, exam behavior, entitlement behavior, screen existence, page ownership, or product requirements.
- A story may demonstrate an already-authorized component or screen state. It must not invent missing functionality, UX behavior, product intent, copy, route inventory, labels/groups/order/icons, navigation layout, or screen states.
- Reusable production components may later expose meaningful approved states in Storybook, including default, hover, focus, disabled, loading, error, empty, success, validation, interaction, responsive, and RTL/LTR states where applicable. Story fixtures may provide inputs needed to demonstrate states but do not create business authority.
- Production screen components may later have stories for approved screen states when it improves review. Screen stories may only represent states authorized by the screen's functional/design contract.
- If a component or screen can be composed from approved functional contracts, route/access/API contracts, visual foundations, and existing component/layout patterns, a duplicate design artifact is not required solely as a procedural duplicate. If materially new visual intent is unresolved, STOP for visual authority, normally through an explicit Stitch/design decision during the active redesign program.
- Storybook visual evidence may satisfy a future owning component/screen gate only after Storybook tooling is installed and verified. The owning gate and technical lead still approve; Storybook never self-approves.
- Storybook does not replace unit tests, Angular component behavior tests, route/auth/permission tests, API/integration verification, accessibility checks, lint, quality checks, or production build verification.
- Automated visual regression remains unapproved. Do not add screenshot regression services, image snapshot frameworks, browser/DPR matrices, pixel tolerance policy, hosted visual-review services, or CI visual-regression integration without separate technical-lead/tooling approval.

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
- Canonical breakpoint ranges are: mobile `<600`, tablet `600–959`, desktop `960–1279`, large `1280–1919`, and wide `1920+`. Canonical page gutters are: mobile `16px`, tablet `24px`, and desktop+ `32px`. Wide layouts additionally constrain readable/content width rather than expanding indefinitely.
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

## 11. Stitch, Legacy Penpot, and MCP rules

- Stitch is the active visual-design workspace for the system-wide redesign. A Stitch screen becomes visual-composition authority only after human approval. Existing Penpot artifacts are legacy/reference evidence unless a specific element is explicitly re-approved. A running service, export, geometry read, generated screen, or tracker checkbox does not approve a design.
- Approved Stitch redesign artifacts are authoritative for visual decisions only. Backend source and the canonical OpenAPI are authoritative for routes, DTOs, nullability, enums, validation, authentication, authorization, permissions, status codes, server errors, business behavior, sensitive-data exposure, and payment/exam/entitlement trust boundaries. A design example never creates backend business behavior.
- No current product screen is automatically implementation-approved merely because it exists in Penpot or the PDF export. This explicitly includes Sign In, Preparation Package Offers, Preparation Package Details, and Checkout.
- Before a product screen is implemented, its task MUST identify the approved Stitch/design reference (or explicitly re-approved legacy Penpot/design reference), backend/OpenAPI contract, required states, responsive behavior, RTL behavior, accessibility acceptance criteria, and unresolved design/backend conflicts. If any required input is missing, STOP AND ESCALATE.
- Do not modify Stitch, Penpot, create boards/projects/screens, import/export files, or use MCP write operations without explicit task authorization and the applicable design gate. Use read-only inspection only when the task permits it.
- Record a design discrepancy in the applicable governance open-question or decision-log process; do not correct it by inference. Penpot never supplies backend/API facts.
- Current observed local services are Penpot frontend at `http://localhost:9001`, Open Design at `127.0.0.1:7456`, and a running Penpot MCP container. These are local runtime facts, not approval or authorization.
- Penpot execution requires an approved page specification, frozen supporting contracts, explicit target IDs, a bounded task packet, and the applicable Penpot gate. Existing Page 07, Page 09, Page 10, and AUTH-001 discrepancies remain governed evidence, not implementation input.

## 12. Agent task rules

- Read the current task and all relevant authority, API, design, and business documentation before changing files. Load required skills before acting.
- Obey the current task boundary exactly; make no unrelated edits, preparatory scaffolding, or deferred-feature work. Every implementation Task has an authorized scope. The Agent may modify only the files/modules explicitly listed by the Task plus directly necessary dependency files. Discovery of a desirable broader refactor does not authorize that refactor. STOP AND ESCALATE before widening scope.
- Do not combine unrelated cleanup, modernization, dependency upgrades, formatting migrations, or architecture changes with a feature Task.
- Once a Task or Feature reaches VERIFIED status, later tasks MUST NOT refactor, redesign, rename, move, or behaviorally modify that completed scope unless the new task explicitly declares it as an affected dependency or REOPENED scope. If a completed area must change, record why it is being reopened, identify the dependent task/decision, mark the item REOPENED, define authorized affected files/modules, rerun its acceptance/verification gates, record the new verification evidence, and return it to VERIFIED. No opportunistic cleanup of verified features.
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
- Detailed frontend Task/Subtask history belongs in `docs/frontend/execution/frontend-implementation-ledger.md`. `PROGRESS.md` remains the high-level canonical session and handoff memory: current Goal, current Milestone, active Task, latest verified checkpoint, blockers, next approved gate, important cross-cutting decisions, and a reference to the detailed ledger.

## 14. Testing rules

Future approved frontend work must include proportional tests for components and services, feature state, route guards, auth/token storage, single-flight refresh, interceptors, typed API adapters, Problem Details mapping, form validation, permission-aware presentation, payment/exam trust boundaries, feedback/error behavior, and design-token/Material-theme integration.

- Use Vitest and Angular TestBed/official testing utilities for unit and component tests. Use official HTTP testing providers and `HttpTestingController` for client/interceptor unit tests.
- Add accessibility checks, including AXE where applicable, plus manual keyboard, focus, screen-reader, responsive, and design-state verification.
- Cover loading, empty, validation, authorization, conflict, rate-limit, and failure states that the approved feature contract makes applicable.
- Do not add E2E tests, Playwright/browser execution, visual regression tooling, or real-backend integration tests unless explicitly authorized by the applicable task/specification. Do not introduce Cypress.

## 14.1 Angular Forms Policy

For new Nursing Platform forms, Signal Forms are the preferred Angular-native option when they satisfy Angular Material/control compatibility, required accessibility behavior, server-validation mapping, async validation behavior, typed backend-contract integration, and required UX behavior.

Do not force Signal Forms when a required Angular Material/custom-control workflow is incompatible or materially less reliable. When Signal Forms are not appropriate, prefer strictly typed Reactive Forms. Template-driven Forms are not the default architecture for production feature forms.

Before the first product form implementation, the owning form/pattern Task must verify the chosen form approach against the actual Angular 22 + Angular Material implementation. Do not introduce mixed form architectures casually. Do not duplicate backend validation rules as frontend business truth.

## 15. Frontend Task Definition of Done

A frontend Task may be marked `VERIFIED` only when all applicable requirements pass:

- requested business behavior implemented;
- backend/OpenAPI contract respected exactly;
- no guessed response fields/contracts;
- architecture/dependency rules respected;
- approved visual foundation and any task-required Stitch/design contract respected;
- relevant loading/error/empty/disabled/success states implemented;
- desktop/tablet/mobile verified where applicable;
- RTL behavior verified where applicable;
- keyboard/focus behavior verified;
- WCAG requirements addressed;
- no sensitive/raw backend/internal data leakage;
- relevant tests pass;
- lint/stylelint pass;
- production build passes when applicable;
- generated-client drift check passes when applicable;
- no unrelated repository regression;
- no unauthorized scope change;
- task ledger updated with evidence;
- `PROGRESS.md` updated when milestone/current-state changes;
- completion commit recorded once committed.

A screenshot matching Penpot alone is never sufficient Definition of Done.

## 16. Explicitly forbidden without authorization

- Create or scaffold the Angular project.
- Install dependencies or change npm/package-manager policy.
- Generate an API client or select an OpenAPI generator.
- Modify Penpot or any Figma artifact (Figma is non-authoritative), import/export design files, or create boards/page specifications.
- Implement screens, components, routes, theme code, or frontend runtime behavior.
- Modify backend source, tests, API behavior, authentication, authorization, or security posture.
- Implement deferred storage/delivery, offline, workspace, adaptive, retraining, or employer package features.
- Stage, commit, push, delete branches, reset, clean, stash, or broadly stage files.

## 17. Approved initial scaffold contract

The approved initial scaffold contract is recorded for a future scaffold/toolchain Task only. It MUST NOT be executed until that Task is explicitly authorized.

| Item | Decision |
|---|---|
| Angular major | 22 |
| Angular CLI | 22.1.4 |
| Node | 22.23.1 |
| npm | 11.6.0 |
| Package manager | npm |
| Workspace | `frontend/` |
| Routing | yes |
| Styles | SCSS |
| Strict | yes |
| Standalone | yes |
| SSR | no |
| Zoneless | yes |
| Test runner | Vitest |
| Prefix | `np` |
| Nested Git | no |
| Interactive scaffold | no |
| AI config generation | none |
| File name style guide | 2025 |

Preferred reproducible invocation pins the CLI instead of relying on whichever global CLI happens to be installed:

```bash
npx -p @angular/cli@22.1.4 ng new nursing-platform-frontend \
  --directory frontend \
  --routing \
  --style scss \
  --test-runner vitest \
  --standalone true \
  --strict true \
  --zoneless \
  --ssr false \
  --package-manager npm \
  --prefix np \
  --skip-git \
  --commit false \
  --defaults \
  --ai-config=none \
  --file-name-style-guide=2025
```

The scaffold generator executable is pinned exactly to `@angular/cli@22.1.4` through `npx -p @angular/cli@22.1.4 ng new ...`. Stock Angular CLI-generated `package.json` dependency ranges MUST NOT be manually rewritten merely to force every installed Angular package to patch version `22.1.4`; legitimate stock ranges may include entries such as `@angular/cli: ^22.1.4`, `@angular/build: ^22.1.4`, and `@angular/core: ^22.1.0`. After successful scaffold installation, `package-lock.json` is the reproducibility authority for actual resolved package versions, and CI later uses `npm ci`. Angular major remains locked to 22 unless separately approved.

The current approved npm pin is `11.6.0`. npm `10.9.8` is rejected for this workspace because its Arborist peer-resolution path reproducibly throws `Cannot read properties of null (reading 'edgesOut')` for the stock Angular 22 scaffold dependency graph. npm `11.6.0` was locally isolated and verified to resolve the same graph successfully under Node `v22.23.1`. This is an npm resolver defect classification, not an Angular dependency-graph conflict.

`--legacy-peer-deps` is NOT an approved Nursing Platform install policy. Its successful diagnostic result helped isolate the npm `10.9.8` resolver defect only. Future normal installs MUST NOT rely on it unless a separate technical-lead decision explicitly authorizes an exception.

The explicit `--ai-config=none` setting prevents Angular CLI from generating additional AI-tool rule/config files that could duplicate or conflict with `AGENTS.md`, `PROJECT_RULES.md`, frontend architecture/rules, or the implementation ledger. Official Angular AI guidance is incorporated intentionally into Nursing Platform governance instead.

The explicit `--file-name-style-guide=2025` setting preserves Angular's current CLI naming style. Expected generated root filenames may use forms such as `app.ts`, `app.html`, `app.scss`, and `app.spec.ts`. Do not rename generated files to older `app.component.ts` style merely from historical Angular convention. Future generated Angular artifacts should follow the approved workspace naming convention unless repository architecture explicitly defines a different name.

If the exact pinned Angular CLI `22.1.4` is later shown not to support `--ai-config=none` or `--file-name-style-guide=2025`, STOP AND ESCALATE. Do not remove either flag silently and do not substitute a newer CLI without explicit approval.

Future `T-FE-001` verification must confirm that no Angular scaffold-generated AI config/rules files were created, 2025 filename style is used, generated files were not renamed to historical Angular naming, no unauthorized MCP/Agent-Skill configuration was generated, and existing Nursing Platform governance remains the only repository AI authority.

Node/npm project pin files are created as part of the scaffold/toolchain Task, not manually before the workspace exists unless the future plan explicitly says otherwise.

## 18. Future API client generator approval gate

Do not select a generator during governance. The future **API CLIENT GENERATOR APPROVAL** gate MUST occur after workspace scaffold but before auth or feature API integration. It must:

- test candidate generator against canonical `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json`;
- verify nullable/required semantics;
- verify enums;
- verify Problem Details;
- verify pagination;
- verify Bearer-auth integration surface;
- verify CV multipart binary file;
- verify generated Angular/RxJS compatibility;
- inspect output architecture/dependency footprint;
- choose exact generator/version/config;
- decide generated-code Git policy;
- document reproducible generation command;
- verify regeneration determinism;
- prohibit implementation using guessed handwritten backend DTOs while this gate is unresolved.

Until that gate passes, feature/API integration is BLOCKED.

## 19. Future screen implementation approval gate

Before implementing a product screen, the future **SCREEN IMPLEMENTATION APPROVAL** gate MUST record:

- screen ID;
- status upgraded from draft/planned as appropriate;
- approved design/Penpot reference;
- all required states;
- backend/OpenAPI mapping;
- responsive contract;
- RTL contract;
- accessibility contract;
- resolved design/backend conflicts;
- acceptance criteria.

No agent may upgrade a Penpot screen's approval status by itself.

## 20. Next required decisions

`GOAL-FE-001` and the detailed Goal → Milestone → Task → Subtask → Verification Gate roadmap are persisted in `docs/frontend/execution/frontend-implementation-ledger.md`. Before frontend project creation or implementation work proceeds, obtain only the next explicit task authorization and any task-local approvals required by that ledger.

Known future decisions include:

1. Explicit authorization to begin `T-FE-001`.
2. API client-generation strategy through the API CLIENT GENERATOR APPROVAL gate.
3. Screen-specific implementation approvals through the applicable SCREEN APPROVAL PACKET REVIEW gates.
4. Production auth/session posture.
5. Production payment scope.
6. Browser support matrix.
7. Visual-regression tolerance and approval process.
8. Whether confirmed backend-gap screens create backend backlog work or remain outside frontend scope.

Until the applicable decision and pre-implementation gates are satisfied, frontend work remains documentation/evidence-only.
