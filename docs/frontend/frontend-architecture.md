# Frontend Architecture

## Purpose And Authority

This document is the authoritative source for permanent frontend engineering rules in the Nursing Platform repository.

It owns the Angular application structure, component separation, state/data-flow conventions, and frontend implementation boundaries. [Routing and permissions](routing-and-permissions.md) owns route and guard behavior; [Design system](design-system.md) owns the visual-system implementation authority chain; [Accessibility](accessibility.md) and [RTL/localization](rtl-localization.md) own their specialist frontend obligations. [Screen contracts](screen-contracts/README.md) map approved screen states and source boundaries. Product, Architecture, API, and Security remain the owners of their respective higher-level facts.

This document does not define temporary phase scope, visual design, acceptance criteria, execution steps, or phase task lists.

Separate documents own those decisions:

- The 2026-09-20 human decision starts a system-wide visual redesign in Google Stitch. For that redesign, approved Stitch screens own new visual composition after human approval; existing Penpot artifacts are legacy/reference evidence unless explicitly re-approved. Approved frontend visual foundations and approved design specifications remain visual constraints within the security, accessibility, and architecture constraints in this document.
- Storybook is the approved intended frontend visual development and review tooling model for production Angular components and production screen states. Storybook is not requirements authority; installation evidence alone does not authorize a screen or visual decision.
- `docs/frontend/design/frontend-design-foundation-reference.md` is the canonical textual implementation mapping of the approved frontend foundation and MUST be consulted before Angular UI, SCSS, Angular Material theme, component, or screen work.
- Figma is non-authoritative unless Karam explicitly records a future decision changing that rule.
- Future approved frontend design specifications own phase scope, behavior, and acceptance criteria.
- Future approved implementation plans own execution steps, task sequencing, verification commands, and commit strategy.
- `CURRENT_TASK.md` and bounded execution ledgers are updated only when explicitly approved.

Frontend work MUST follow this architecture unless a later reviewer-approved architecture decision explicitly changes it.

This document describes intended frontend implementation constraints. Current Angular source establishes current implementation only; neither a source file nor a package dependency independently approves new Product or visual behavior.

## Source Authority Policy

Implemented backend source and the verified [OpenAPI contract](../api/openapi.yaml) establish current runtime/API behavior for frontend integration. [Product](../product/requirements.md) owns approved behavior; [Security](../security/security-overview.md) owns protected enforcement. A code/prose disagreement must be reported and resolved within that ownership chain rather than guessed around.

Approved Stitch redesign artifacts own new visual-composition decisions only within their human approval scope. Legacy Penpot artifacts remain reference evidence unless explicitly re-approved. [Frontend routing](routing-and-permissions.md) owns client route identity and guard UX; OpenAPI owns HTTP paths, DTOs, and declared responses; Security owns protected enforcement. Backend code proves current behavior and trust-boundary implementation, not independent Product intent. A design example never creates backend or Product behavior.

Official Angular documentation and Angular-maintained AI guidance matching the Angular version pinned in the workspace are the preferred authority for Angular APIs, compatibility, defaults, tooling, and recommended patterns. Relevant official framework guidance includes `https://angular.dev/ai/develop-with-ai`, `https://angular.dev/ai/agent-skills`, `https://angular.dev/ai/mcp`, and `https://angular.dev/cli/new`.

Official Angular guidance is framework guidance only. It is subordinate to explicit Nursing Platform decisions, repository governance, backend/OpenAPI business and security contracts, approved visual evidence, and active task scope. Do not silently adopt APIs or behavior from a later Angular major. If current Angular documentation describes a patch-sensitive API or CLI behavior whose availability in the installed/pinned Angular version is uncertain, verify it against the actual installed/pinned version, do not upgrade Angular merely to use it, and STOP AND ESCALATE if incompatible.

Endpoint fields, validation limits, content types, status behavior, and other volatile contract details MUST be verified against the implemented backend and generated Development OpenAPI during each feature design and implementation phase.

This document owns frontend implementation response to trust boundaries. Permanent system boundary rationale belongs to Architecture; protected security and ownership enforcement belongs to Security; endpoint values and request/response shapes belong to API.

Historical reviews and cached documentation do not guarantee future compatibility. Every implementation phase MUST use current sources for the pinned workspace and current backend revision.

## Technology Foundation

The approved frontend foundation is:

- Angular 22.
- TypeScript.
- Angular standalone application architecture.
- Angular Signals.
- Signal Forms as the preferred Angular-native first choice for new forms when they satisfy the workflow's compatibility, accessibility, validation, backend-contract, and UX requirements.
- Strictly typed Reactive Forms as the documented fallback when Signal Forms are unsuitable, incompatible, or materially less reliable for a workflow.
- RxJS.
- Angular Material.
- Angular CDK where required by Angular Material or explicitly justified application behavior.
- SCSS as the approved styling language and stylesheet strategy.
- A project-owned Angular Material theme derived from approved design tokens.
- Angular Router.
- Feature-based architecture.
- Angular SPA for the initial product architecture.

SSR, SSG, hybrid rendering, PWA, and offline support are deferred unless explicitly approved later.

npm is the only approved package manager.

Angular strict mode and TypeScript strict mode MUST be enabled when the project is scaffolded.

Angular CLI MUST use the same major version as Angular.

Angular Material and Angular CDK versions MUST match the pinned Angular major and MUST be pinned through the approved npm dependency policy.

The exact Node.js version MUST be pinned during scaffold work after checking the official Angular 22 compatibility requirements. The chosen Node.js version MUST later be consistent across local development, `.nvmrc`, `package.json` engines, and CI.

Angular, TypeScript, Node.js, and RxJS versions MUST satisfy the official Angular 22 compatibility table. Exact executable versions MUST be selected only from those supported ranges.

The frontend MUST NOT introduce NgRx or another state-management library without a separate approved architecture decision.

Angular Material is the only currently approved general-purpose UI component library.

Any other external UI component library MUST NOT be introduced without a separate approved architecture decision.

An external toast library or package MUST NOT be introduced without a separate approved architecture decision.

## Angular 22 Platform Rules

Angular standalone APIs are the default application model. New components, directives, and pipes MUST NOT set redundant `standalone: true` metadata.

New components MUST NOT set redundant `ChangeDetectionStrategy.OnPush`; implementation MUST rely on Angular 22's zoneless-compatible defaults unless an approved compatibility exception establishes a concrete need.

The frontend MUST preserve Angular 22's zoneless model unless a future approved architecture decision explicitly changes it.

Code MUST follow the official Angular style guide unless a repository-specific rule is stricter.

Source files SHOULD keep one primary Angular concept per file.

Filenames MUST use hyphens between words. Tests MUST use the same base filename as the unit under test with the `.spec.ts` suffix.

The initial Angular CLI scaffold MUST use the 2025 file-name style guide. Expected generated root filenames may use forms such as `app.ts`, `app.html`, `app.scss`, and `app.spec.ts`. Do not rename generated files to older `app.component.ts` style merely from historical convention. Future generated Angular artifacts SHOULD follow the approved workspace naming convention unless repository architecture explicitly defines a different name.

Ordinary production Angular components under `frontend/src/app` MUST keep component TypeScript, rendered markup, and component styling separated into dedicated colocated files. The component class, imports, inputs/outputs, and metadata belong in the component `.ts` file; rendered component markup belongs in a colocated external `.html` file referenced through `templateUrl`; component styling belongs in a colocated external `.scss` file referenced through `styleUrl` or another explicitly approved external style metadata form. Focused component tests SHOULD remain colocated in a matching `.spec.ts` file whenever behavior or rendering is testable. Inline `template:`, inline `styles:`, `<style>` blocks inside component templates, and embedding component HTML or SCSS inside TypeScript merely because Angular permits it are prohibited for ordinary production components. A trivial template is not a default exception. Directives, pipes, services, guards, interceptors, and other non-rendering Angular code do not require artificial template or stylesheet files. Generated API code is not manually edited to satisfy component structure. Test-only host templates may remain inline in their `.spec.ts` fixture. An intentionally styleless production component may omit its SCSS file only with a task-recorded rationale; a production component rendering markup without an external HTML template requires explicit architecture or task approval. Do not invent other exceptions.

Code MUST be organized by feature or domain rather than global type-only folders. Vague catch-all files such as `helpers.ts`, `utils.ts`, and `common.ts` MUST NOT be introduced; files MUST communicate focused ownership.

New component APIs MUST use `input()`, `output()`, and `model()` rather than legacy decorator APIs. Angular-created input, output, model, and query properties SHOULD be declared `readonly`.

Dependency injection SHOULD use `inject()` rather than constructor injection for new code.

Signals MUST be preferred for local synchronous component state where appropriate. Derived read-only state MUST use `computed()`. Use `linkedSignal()` when writable/derived state must remain synchronized with reactive source changes and that behavior is semantically appropriate. Signal transformations MUST remain pure.

Signals MUST be changed with `set()` or `update()`. Legacy mutation patterns that modify signal-held state without notifying Angular MUST NOT be used.

Effects MUST be used sparingly and MUST NOT replace clear event-driven application flow.

New templates MUST use built-in control flow with `@if`, `@for`, and `@switch`. Every `@for` block MUST use a stable tracking expression.

Class and style bindings SHOULD be preferred over `ngClass` and `ngStyle`.

Host bindings and listeners MUST use component or directive `host` metadata rather than `@HostBinding` or `@HostListener`.

Templates MUST remain simple and MUST NOT contain business workflows or expensive transformations. Templates MUST NOT assume JavaScript globals such as `new Date()` are directly available; values needed by a template MUST be exposed explicitly by the component.

External component template and style paths MUST be relative to the component TypeScript file.

`NgOptimizedImage` MUST be used for appropriate supported static images. Do not apply it blindly to unsupported cases such as inline base64 images. Privacy and security rules still apply to user/private imagery. The frontend MUST NOT introduce `::ng-deep`.

Services MUST be focused and single-responsibility. Asynchronous template values SHOULD use `AsyncPipe` or deliberate signal interop rather than unmanaged subscriptions.

Current Angular v22 documentation recommends the new `@Service()` decorator for new singleton services. Do not make this a blindly applied project rule before the installed `@angular/core` version is verified after scaffold. After `T-FE-001` creates the Angular workspace, verify whether the installed Angular 22 package version exposes the official `Service` decorator. If confirmed compatible, prefer `@Service()` for new singleton services according to Angular v22 guidance. If not available in the installed locked version, use the supported documented singleton-service mechanism, do not upgrade Angular merely for `@Service`, and record the compatibility result.

Uncertain external boundaries MUST use `unknown` and explicit narrowing. `any` MUST be avoided and requires narrow, documented justification when unavoidable.

### Zoneless Rules

`zone.js`, `zone.js/testing`, and `provideZoneChangeDetection` MUST NOT be added without an approved compatibility exception.

Application code MUST notify Angular of relevant state changes through signals, inputs, template or host listeners, `AsyncPipe`, or another explicitly supported notification mechanism.

Tests MUST reflect Production zoneless behavior. Tests MUST NOT rely on repeated forced `fixture.detectChanges()` calls to conceal missing application notifications.

## Angular AI Tooling Rules

When agents use Angular AI assistance, they MUST prefer official Angular context sources over generic framework assumptions.

`https://angular.dev/llms.txt` is the official Angular AI context index. `https://angular.dev/assets/context/llms-full.txt` is supplementary compiled context. These resources are dynamic, and agents MUST consult their current versions when they are relevant.

Cached AI knowledge MUST NOT be assumed to match the Angular version pinned in the workspace. A vendored copy of Angular AI context MUST NOT become permanent authority without an explicit update and compatibility policy.

The official `angular-developer` and `angular-new-app` skills SHOULD be used by agents when they are already available in the active environment and relevant to Angular work. Do not install an Agent Skill as part of a normal implementation Task, and do not run `npx skills add` without explicit tooling/dependency authorization. Angular skills are advisory and subordinate to Nursing Platform governance; they cannot authorize architecture, dependency, UI-library, business-rule, backend-contract, or design changes. Absence of an Angular Agent Skill is not by itself a project blocker.

Angular CLI MCP MAY be used when it is already available to the Agent. Prefer read-only framework-information capabilities for Angular-specific questions, especially `get_best_practices`, `search_documentation`, and `list_projects`. Do not add MCP configuration files to the repository, install/configure MCP globally, run migration/modernization/write-capable MCP tools merely because they are available, or use MCP to widen Task scope unless explicitly authorized. If Angular MCP is unavailable, use official `angular.dev` documentation instead. MCP availability is not a default implementation blocker. MCP MUST run from the Angular CLI version pinned by the project and MUST NOT silently download or execute an unpinned future major version.

Read-only audits SHOULD use read-only MCP capabilities. MCP write operations MUST NOT bypass isolated-worktree, TDD, approval, review, or verification requirements.

AI-generated Angular code MUST still be verified against this document, the official Angular documentation, and the implemented backend source.

AI tools MUST NOT be treated as authority for backend contracts, security decisions, accessibility waivers, or product scope.

Generic skills, including design, brand, design-system, ui-styling, ui-ux-pro-max, brainstorming, or similar capabilities, are subordinate to Nursing Platform repository governance, approved Penpot evidence, backend/OpenAPI contracts, business rules, security, accessibility, and task scope. A generic skill MUST NOT silently introduce Tailwind, shadcn, a second UI library, new design tokens, new business behavior, new architecture, or a conflicting visual decision.

## Dependency And Lockfile Policy

Because npm is approved, `package-lock.json` MUST be committed after the frontend project is scaffolded.

CI MUST use `npm ci` for deterministic dependency installation.

Floating dependency declarations such as `latest` MUST NOT be committed.

New dependencies require a scoped justification and MUST NOT duplicate capabilities already provided by Angular, Angular Material, Angular CDK, TypeScript, RxJS, SCSS, or the platform.

Any new frontend dependency requires documented purpose, Angular/CDK alternative check, duplicate-capability check, maintenance/security impact, bundle impact where relevant, and explicit approval.

Lockfile changes MUST be attributable to an approved dependency or package-manager change. Normal lockfile updates caused by approved dependency changes are expected.

Wholesale deletion or regeneration of `package-lock.json` merely to bypass conflicts is forbidden.

Local development and CI MUST use the pinned Node.js and npm policy once those executable files are established. The current approved frontend toolchain pin is Node `v22.23.1` with npm `11.6.0`; npm `10.9.8` is rejected for this workspace because its Arborist peer-resolution path reproducibly throws `Cannot read properties of null (reading 'edgesOut')` for the stock Angular 22 scaffold dependency graph, while npm `11.6.0` resolves the same graph successfully under Node `v22.23.1`. This is not an Angular dependency-graph conflict. `--legacy-peer-deps` is not an approved normal install policy.

## Architecture Goals

Frontend implementation SHOULD optimize for:

- Maintainability.
- Scalability.
- Testability.
- Accessibility.
- Security.
- Predictability.
- Clear ownership boundaries.
- Long-term consistency with backend contracts.
- Performance that is measured and budgeted, not assumed.

Convenience MUST NOT override architecture, security, accessibility, or testability.

## Project Structure

The initial Angular application MUST use a feature-based structure under `src/app`.

The approved top-level application boundaries are:

```text
src/app/core/
src/app/shared/
src/app/features/
```

Major feature routes MUST use lazy loading.

The exact folder tree MAY evolve during approved implementation work, but the `core`, `shared`, and `features` ownership rules in this document MUST remain intact.

## Dependency Direction

Frontend dependencies MUST preserve clear ownership boundaries.

| Source | May Depend On | Must Not Depend On |
| --- | --- | --- |
| `core` | Angular platform APIs, shared business-neutral utilities | Feature implementations |
| `shared` | Angular platform APIs, business-neutral utilities | Core auth state, feature implementations, feature APIs |
| `features` | Core public services, shared public utilities, owning feature internals | Private internals of other features |
| generated API client | Generated model/client code only | Components, feature state, UI concerns |
| feature API adapters | Generated or handwritten API clients | Component implementation details from other features |

Business workflows belong in the owning feature or in explicit application services, not in low-level generated clients, interceptors, route files, or shared UI primitives.

Shared MUST NOT depend on Core or Features.

Core MUST NOT depend on Features.

Features MAY depend only on stable public Core and Shared APIs plus their own internals.

Each business workflow belongs to one owning feature. A feature MUST NOT absorb another feature's workflow merely for convenience.

Core MUST NOT become a dumping ground for feature workflows, feature state, feature data shaping, or business-specific helpers.

Shared MUST remain business-neutral and MUST NOT contain workflow policy, backend-resource ownership rules, permission decisions, payment/exam/entitlement behavior, or feature-specific state.

The root application shell and routing MAY compose public Feature entry points.

Barrel files MUST NOT expose private internals. Circular dependencies are forbidden.

These boundaries SHOULD be enforced with lint rules or architecture tests after the Angular workspace exists.

## Core Boundary

`src/app/core/` owns application-wide infrastructure.

Core MAY contain:

- authentication state;
- token lifecycle;
- HTTP interceptors;
- global error handling;
- route guards;
- application configuration;
- application-shell infrastructure.

Core MUST NOT contain business-feature implementation.

Core services SHOULD be singletons only when application-wide lifetime is required.

## Shared Boundary

`src/app/shared/` owns reusable, business-neutral building blocks.

Shared MAY contain:

- reusable UI primitives;
- directives;
- pipes;
- generic utilities;
- truly shared models.

Shared MUST NOT contain:

- feature business logic;
- feature-specific API calls;
- feature state;
- feature workflows.

Shared code MUST remain reusable without depending on private feature details.

## Feature Boundary

Each feature under `src/app/features/` owns its own:

- routes;
- pages;
- components;
- forms;
- feature state;
- feature models;
- API adapters;
- validators.

Features MUST NOT import private internals from other features.

Cross-feature communication MUST use stable public contracts or approved application-wide services.

Feature code SHOULD keep state and behavior as local as practical.

## Routing And Authorization

The frontend MUST use Angular Router.

Routes MUST be feature-oriented and lazy-loaded for major features.

Route guards MAY improve navigation, presentation, and user experience.

Route guards MUST NOT be treated as final security enforcement.

Frontend authorization is presentation and navigation behavior only. Backend authentication and authorization remain authoritative.

## State Management

Signals are the primary mechanism for synchronous UI and feature state.

RxJS is used for:

- HTTP;
- asynchronous coordination;
- cancellation;
- event streams;
- router-driven streams when appropriate.

State MUST remain as local as practical.

Auth state is application-wide and belongs in Core.

Feature state belongs to the owning feature.

The frontend MUST NOT create a global store without a demonstrated requirement and an approved architecture decision.

The frontend MUST NOT create global mutable state merely for convenience.

The frontend MUST NOT duplicate auth, API, cache, or state implementations. Shared cross-cutting behavior must have one clear owner.

Any new state-management library or architectural state pattern requires explicit architecture approval.

The frontend MUST NOT cache server data without explicit invalidation and refresh rules.

## RxJS Cancellation And Cleanup

Search, filtering, autocomplete, and route-driven requests SHOULD cancel stale requests when newer input supersedes them.

Component subscriptions MUST be lifecycle-safe and cleaned up automatically.

Angular lifecycle-aware cleanup such as `takeUntilDestroyed` SHOULD be used where applicable.

Composition operators such as `switchMap` SHOULD be preferred over nested subscriptions when newer emissions supersede older work.

Long-lived subscriptions require explicit ownership and cleanup documentation.

## Visual Design, Stitch, Legacy Penpot, And Storybook Boundary

Visual design authority is separated from implementation and review tooling. The active system-wide redesign uses Google Stitch as the visual-design workspace. Approved Stitch screens own new visual composition after human approval. Approved frontend visual foundations and approved design specifications remain constraints; legacy Penpot artifacts are reference evidence unless explicitly re-approved for a specific redesign element.

`docs/frontend/design/frontend-design-foundation-reference.md` records the canonical textual implementation mapping for the approved frontend foundation. If live Stitch redesign evidence, re-approved Penpot evidence, or this reference conflict, implementation MUST stop for design resolution and reference update instead of guessing or silently choosing either source.

Approved Stitch redesign artifacts, explicitly re-approved legacy Penpot artifacts, and approved design tokens become authoritative for:

- brand colors;
- typography;
- spacing;
- visual hierarchy;
- component appearance;
- dashboard layouts;
- responsive compositions;
- iconography;
- illustrations;
- interaction appearance.

Stitch and design decisions MUST operate within repository security, accessibility, performance, and architecture rules. Design artifacts MUST NOT override WCAG requirements, backend trust boundaries, token-handling rules, or sensitive-data handling.

When an approved design conflicts with accessibility, security, privacy, performance, backend contracts, or permanent architecture rules, implementation MUST stop and request explicit design and engineering review.

SCSS and the project-owned Angular Material theme are executable mappings of approved visual intent from the frontend foundation, approved Stitch redesign artifacts, and any explicitly re-approved legacy Penpot/design artifact. They are not independent sources of final visual-design decisions and MUST NOT silently override that intent.

Frontend implementation agents MUST NOT invent final visual design.

No current product screen is automatically implementation-approved merely because it exists in Penpot or the PDF export. This explicitly includes Sign In, Preparation Package Offers, Preparation Package Details, and Checkout.

Before a product screen is implemented, its task MUST identify the approved functional/page contract, route/access/API contracts, visual foundation or approved design reference, required states, responsive behavior, RTL behavior, accessibility acceptance criteria, and unresolved design/backend conflicts. If any required input is missing, implementation MUST stop and escalate.

Storybook is adopted as the intended primary visual development and review surface for production Angular components and production screen states after separate tooling authorization. Stories MUST import and render the same production Angular components used by the application. Storybook MUST NOT create Storybook-only component copies, duplicate markup, duplicate SCSS, parallel token definitions, alternate implementations, a second design system, or product requirements.

Storybook is not authoritative for business behavior, backend contracts, OpenAPI, DTOs, routes, authentication, roles, permissions, security, validation semantics, payment behavior, exam behavior, entitlement behavior, screen existence, page ownership, or product requirements. A story may demonstrate an already-authorized state; it may not invent a missing state or requirement. If functionality, UX behavior, or product intent is unresolved, implementation MUST stop for authority.

When a screen or component can be composed from an approved functional contract, approved route/access/API contracts, approved visual foundation, and approved existing component/layout patterns, a duplicate design artifact is not required solely for procedure. During the active redesign, materially new visual intent—such as a novel page layout, new navigation/layout concept, complex multi-step visual flow, new major interaction paradigm, new visual language not covered by existing foundations, or a cross-screen flow whose intended composition cannot be derived from approved patterns—requires explicit Stitch/design authority before implementation.

For novel design during the active redesign, the approved workflow is: Stitch/design exploration and human approval → production Angular implementation → Storybook production-state review. For routine compositions, the approved workflow is: approved contracts/foundations → production Angular implementation → Storybook production-state review. Storybook never replaces functional tests, component behavior tests, route/auth/permission tests, API verification, accessibility checks, lint, quality checks, or production build verification.

Storybook visual evidence may serve a future owning component/screen gate only after Storybook tooling is separately installed, configured, and verified. Storybook evidence never self-approves a component or screen, and it does not bypass screen-family approval, exact screen approval, task dependencies, implementation gates, or technical-lead approval. No currently blocked screen becomes implementation-authorized because of Storybook adoption.

Automated Storybook visual-regression or baseline testing is not approved by this architecture decision. Screenshot regression services, image snapshot frameworks, browser/DPR matrices, pixel tolerance policies, hosted visual-review services, or CI visual-regression integration require separate technical-lead/tooling approval.

Before approved Stitch/design screens exist, frontend work MAY use only neutral structural UI to verify:

- routing;
- authentication;
- forms;
- API communication;
- loading states;
- error states;
- accessibility behavior.

The initial foundation MUST NOT create a separate design-system package.

## Angular Material Usage Policy

Angular Material provides accessible behavioral and interaction primitives. Material visual defaults are not the final Nursing Platform design.

Material components MUST be imported selectively by standalone components or focused providers to preserve clear ownership and tree shaking.

The frontend MUST NOT create a broad global `MaterialModule` that imports or exports the entire library.

Features SHOULD use Angular Material components when they satisfy the required behavior and accessibility contract.

The project MUST NOT wrap every Material component automatically. A project-owned wrapper is justified only when it establishes a repeated Nursing Platform behavior, stable design contract, security rule, accessibility rule, or shared Penpot customization.

Application business logic MUST NOT be placed inside Material wrappers.

Angular Material internal DOM structure and private CSS class names MUST NOT be treated as stable APIs. Styles MUST NOT target undocumented internal selectors, including private `mat-mdc` implementation classes.

Angular Material customization MUST use supported theming APIs, design tokens, CSS custom properties where appropriate, documented component APIs, and narrowly owned application classes.

The existing prohibition on `::ng-deep` remains enforceable.

Approved visual intent and the project theme determine colors, typography, density, shape, spacing, and component appearance within accessibility and architecture constraints. Use the approved frontend foundation, approved Stitch redesign artifacts, and any explicitly re-approved legacy Penpot/design artifact; do not invent new values from Angular Material defaults or Storybook convenience.

Final colors, typography values, density values, elevations, breakpoints, and dark-mode behavior MUST be defined only through approved visual foundations, approved Stitch redesign artifacts, explicitly re-approved legacy Penpot/design artifacts, or later approved feature/foundation design scope.

## SCSS Architecture

The global stylesheet architecture MUST have clear ownership.

- `src/styles.scss` is the single root global stylesheet entry point.
- `src/styles/` owns only application-wide styling foundations.
- Component-specific SCSS remains colocated with its Angular component.
- Feature-specific styles remain inside the owning feature.
- A feature MUST NOT place private styling into the global stylesheet merely to avoid proper component ownership.

The future scaffold SHOULD organize global SCSS responsibilities around:

- design tokens;
- Angular Material theme integration;
- base document styles;
- typography;
- approved layout and accessibility utilities;
- theme variants when later approved.

The Frontend Foundation design specification and approved implementation plan MAY establish exact final filenames. The ownership boundaries in this section are permanent.

Global styles MUST remain small and intentional. Deep global selectors and page-specific global rules are forbidden.

CSS specificity MUST remain predictable. `!important` MUST NOT be used except for a narrowly documented exceptional requirement.

Repeated arbitrary colors, spacing, typography, radii, shadows, or z-index values MUST NOT be scattered across components. Approved design values MUST come from centralized design tokens.

Canonical keyboard focus ring: `focus.ring.width = 2px`, `focus.ring.offset = 4px`, and `focus.ring.shadow = 0 0 0 4 #006B66`. `border.focus = #4F46B8` is a distinct focused-border/accent token and MUST NOT be substituted for the keyboard focus ring unless a component contract explicitly uses it.

The canonical standard form-field/select foundation retains the documented `height = 64px` and minimum trailing action target `48 × 48px`. [Design System](design-system.md) owns the current visual radius from `DESIGN.md`: `8px` for standard inputs and selects. The 48px Preparation Package filter draft does not establish a compact control variant. A compact field/select variant may be created only through a later explicit design-system decision.

There are currently no approved canonical project motion duration/easing tokens. Do not invent or hard-code project-authored motion duration or easing values. Until canonical motion tokens are approved, prefer no custom decorative transition over arbitrary animation. Required state meaning must remain complete without animation. Framework-internal behavior is not visual authority and must not be copied into project tokens.

Do not invent a project numeric z-index scale. Angular CDK Overlay owns overlay stacking mechanics unless an evidenced implementation conflict requires an explicit project token. Project-owned elevation remains semantic: level-1 raised, level-2 menus/overlays, level-3 dialogs/modals, and scrim opacity `0.48`. Any custom z-index value requires an explicit architecture/design decision.

SCSS variables, maps, mixins, functions, and CSS custom properties MUST each have a clear purpose and MUST NOT duplicate the same source of truth.

Runtime theme values SHOULD use CSS custom properties where runtime switching or Angular Material integration requires them.

Compile-time SCSS helpers MUST remain deterministic and free from component business semantics.

Theme CSS MUST be emitted once at the correct application boundary. Angular Material theme mixins or equivalent generated theme output MUST NOT be included repeatedly by unrelated components.

Component styles MUST rely on Angular view encapsulation and MUST NOT assume global leakage.

Logical CSS properties such as `inset-inline-start`, `inset-inline-end`, `margin-inline`, and `padding-inline` MUST be used instead of physical directional equivalents. Physical properties (`margin-left`, `padding-right`, `left`, `right`) are banned. Stylelint enforces this through `property-disallowed-list`.

Responsive rules MUST use approved breakpoint tokens or mixins rather than repeated arbitrary media-query values.

Canonical breakpoint ranges are: mobile `<600`, tablet `600–959`, desktop `960–1279`, large `1280–1919`, and wide `1920+`. Canonical page gutters are: mobile `16px`, tablet `24px`, and desktop+ `32px`. Wide layouts additionally constrain readable/content width rather than expanding indefinitely.

Stylesheet optimization MUST NOT reduce accessibility, focus visibility, readable contrast, semantic behavior, or responsive correctness.

## Theme Architecture

The Nursing Platform owns one internal application theme system. The initial foundation MUST NOT create a separately published design-system npm package.

The theme maps approved visual foundation tokens, including Penpot-derived foundation values, into Angular Material theming and application-level SCSS tokens.

There MUST be one authoritative token source for each visual decision. Material theme configuration and application tokens MUST NOT drift into separate conflicting values.

Components MUST consume semantic tokens such as surface, primary action, critical text, spacing, and typography roles rather than arbitrary visual literals.

Theme tokens MUST use semantic names rather than names tied only to a temporary color value.

Theme changes MUST be reviewable and testable.

Future light, dark, high-contrast, brand, or tenant variants require approved design scope. This architecture does not promise dark mode.

Approved visual foundations, approved Stitch redesign artifacts, and explicitly re-approved legacy Penpot/design artifacts are the source of approved visual intent. The repository theme implementation becomes the executable mapping of that approved intent.

A design artifact MUST NOT override accessibility, security, privacy, performance, backend contracts, or permanent architecture rules.

## Token Bridge Architecture

The Nursing Platform enforces a strict token-to-theme pipeline. No ad-hoc overrides, raw color values, or direct Material palette manipulation is permitted outside this bridge.

### Token Source of Truth

`src/styles/_tokens.scss` is the **sole authoritative source** for all design tokens. It contains:

- Brand colors: Nursing Teal (`#006B66`), Professional Navy (`#173B57`), Exam Focus Indigo (`#4F46B8`)
- Neutral palette: 0 through 900 scale
- Typography: font family, font size scale
- Elevation: shadow definitions

All token values MUST originate from approved visual foundations, approved Stitch redesign artifacts, or explicitly re-approved legacy Penpot/design artifacts. Manual edits to `_tokens.scss` without a corresponding approved design decision are forbidden.

### Material Theme Bridge

`src/styles/_material-theme-bridge.scss` is the **mandatory intermediary** between `_tokens.scss` and Angular Material 22. It performs the following:

1. Imports `_tokens.scss` via `@use`.
2. Constructs Angular Material palettes using `mat.m2-define-palette()` with Penpot token values.
3. Constructs a Material theme using `mat.m2-define-light-theme()`.
4. Applies the theme via `@include mat.all-component-themes()`.

Components, features, and shared code MUST NOT directly import `@angular/material` theming APIs to create competing palettes, themes, or overrides. All Material visual behavior flows through this single bridge.

### Prohibited Patterns

The following are forbidden under this architecture:

- Component-local `mat.m2-define-palette()` or `mat.m2-define-light-theme()` calls.
- Direct `mat-*` CSS variable overrides on component selectors.
- Raw hex, rgb(), or hsl() color values in component SCSS.
- Custom Material theme files outside the designated bridge.
- Using `::ng-deep` to override Material component internal styles.

## Accessibility

WCAG 2.2 AA is the permanent accessibility target.

Frontend implementation MUST use:

- semantic HTML;
- keyboard navigation;
- visible focus states;
- accessible labels and names;
- screen-reader compatible structure;
- sufficient contrast;
- form errors associated with their fields;
- non-color-only communication;
- reduced-motion support when animation is introduced.

Actual interactive targets MUST be at least 44 × 44px. A 48 × 48px target is required for mobile/touch-screen viewports. A smaller visible icon or visual surface is permitted only when enclosed by an actual interactive target of at least 44 × 44px.

Accessibility MUST be designed into components and workflows from the beginning, not added only after implementation.

## Localization And RTL Readiness

Full Arabic localization and full RTL implementation are not part of the initial Frontend Foundation unless explicitly added to its future approved design specification.

The architecture MUST remain localization-ready and RTL-ready.

Frontend implementation MUST:

- avoid assumptions that make RTL conversion difficult;
- prefer logical layout concepts such as start/end over hardcoded left/right when practical;
- organize user-visible text so it can later move into translation resources.

The frontend MUST NOT introduce a complete i18n library until localization becomes approved implementation scope.

## Current Backend Authentication Contract

The current backend contract uses JWT Bearer authentication.

Login and refresh currently return `accessToken`, `refreshToken`, and `expiresAt` in JSON responses.

The frontend MUST use `GET /api/v1/me` to hydrate the authenticated user, roles, and permissions.

The `/api/v1/me` response is the frontend's current-user authority. JWT contents MAY support transport-level authentication but MUST NOT be treated as the authoritative source for profile, roles, or permissions in the UI.

Frontend authorization MUST remain presentation and navigation behavior only.

Backend authentication and authorization remain authoritative.

## Token Handling For The Local MVP

For the local frontend MVP:

- the access token MUST be kept in memory;
- the refresh token MUST be kept in `sessionStorage`;
- authentication tokens MUST NOT be stored in `localStorage`;
- all token access MUST be centralized behind an authentication abstraction;
- components and features MUST NOT access `sessionStorage` directly;
- tokens MUST NOT appear in logs;
- tokens MUST NOT appear in analytics;
- tokens MUST NOT appear in URLs or query strings;
- tokens MUST NOT appear in user-facing errors.

This is an interim local-MVP decision, not the final production security design.

Production MUST NOT automatically ship with the local-MVP refresh-token-in-`sessionStorage` design.

Final Production authentication requires dedicated written security approval. That review MUST cover Secure, HttpOnly, SameSite cookies; HTTPS; CSRF/XSRF; CORS; refresh rotation; revocation; server logout; multi-tab behavior; and compromised-session recovery.

Retaining `sessionStorage` for refresh tokens in Production requires explicit documented residual-risk approval.

Production release MUST remain blocked until refresh-token storage, CSRF posture, CORS posture, CSP, logging, and error-reporting behavior are reviewed together against the final deployment model.

## Authentication Bootstrap

Initial authentication state MUST be `initializing` or an equivalent explicit bootstrap state.

The application MUST NOT start by treating the user as prematurely authenticated or anonymous before bootstrap completes.

If a refresh token exists in `sessionStorage`, startup MUST attempt exactly one startup refresh through the central authentication abstraction.

If no refresh token exists in `sessionStorage`, bootstrap MUST resolve directly as anonymous.

After successful refresh, the access token MUST be held in memory and the returned refresh token MUST replace the previous local refresh token through the central token-storage abstraction.

Authentication bootstrap MUST NOT call `GET /api/v1/me` directly. Bootstrap establishes token-session validity only; current-user hydration is a separate downstream task after bearer interception exists.

The frontend MUST NOT call `/api/v1/me` without a valid access token.

Protected-route resolution MAY treat the user as authenticated or anonymous only after this bootstrap process completes.

Guards MUST wait for authentication bootstrap completion and MUST NOT redirect while initialization is still in progress.

Guards MUST also wait until anonymous bootstrap has completed when no refresh token exists.

Failed refresh MUST clear local authentication state, clear locally stored tokens, and resolve the user as anonymous.

A `/me` response of `401` after valid recovery MUST clear local authentication state, clear locally stored tokens, and resolve the user as anonymous.

A transient `/me` network failure or `5xx` response MUST NOT automatically be treated as invalid credentials and MUST NOT erase tokens. It MUST resolve through an explicit bootstrap-unavailable or recovery state with bounded user-visible recovery behavior.

Because `sessionStorage` is used, the local-MVP authentication session is tab-scoped.

A reload in the same tab MAY restore the session through refresh.

The local MVP officially supports one authenticated tab per session. Opening or duplicating tabs can create stale copies of a rotating refresh token.

Cross-tab refresh coordination is deferred. Reuse detected because another tab used a stale refresh token MUST resolve through deterministic local logout; the frontend MUST NOT bypass backend refresh-token reuse detection.

Full multi-tab authentication support requires a separate approved secure-coordination design.

## Authenticated Current User Hydration

Authenticated current-user hydration MUST occur only after bearer Authorization-header injection exists.

Current-user hydration MUST call `GET /api/v1/me` through the normal generated/client transport path and MUST rely on the central bearer interceptor for Authorization-header attachment.

Current-user hydration MUST establish frontend current-user/session identity state, including user identity, roles, and permissions.

Current-user hydration MUST NOT duplicate token refresh, token storage, or interceptor behavior.

## Refresh Coordination

Within one application and tab instance, the frontend MUST NOT allow more than one refresh request to be in flight at the same time.

Concurrent requests requiring refresh MUST wait for the same refresh operation.

After a successful refresh:

- the newly returned refresh token MUST atomically replace the previous refresh token in `sessionStorage`;
- the previous refresh token MUST NOT remain active in frontend storage;
- the new access token MUST be placed in memory;
- token replacement MUST complete before queued requests are released;
- queued requests MUST use the new access token.

Failure to persist the new local authentication state MUST be treated as refresh failure.

A request that received `401` MAY be replayed once only when the request was rejected by authentication before business processing.

If that guarantee cannot be established, a non-idempotent request MUST NOT be replayed automatically.

A second `401` after refresh MUST trigger deterministic logout and MUST NOT cause another refresh loop.

Infinite refresh or retry loops are forbidden.

Login and refresh requests MUST NOT trigger automatic refresh handling.

Server-provided expiry data controls token expiry behavior. Any client clock-skew allowance MUST be bounded, documented, and tested; it MUST NOT extend server authority.

Refresh failure MUST deterministically:

- clear authentication state;
- clear stored tokens;
- fail queued requests;
- redirect the user to login.

Components and features MUST remain unable to access refresh-token storage directly.

## Logout

The current backend does not expose an approved logout or refresh-token revocation endpoint for the frontend.

Until such an endpoint exists:

- logout MUST clear local authentication state;
- logout MUST clear locally held tokens;
- logout MUST redirect to login;
- the frontend MUST NOT claim that the server-side token was revoked.

Server-side logout or token revocation requires a separate backend task.

## API Integration

The API contract uses `/api/v1`.

Frontend requests SHOULD use relative API paths.

Local development SHOULD use an Angular development proxy.

Production SHOULD prefer a same-origin reverse proxy.

Backend CORS MUST NOT be added unless an approved deployment design requires separate origins.

Allowed origins, methods, headers, credentials, and environment boundaries MUST be explicit. Wildcard origins MUST NOT be combined with credentials. Development CORS configuration MUST NOT silently become Production configuration.

Frontend environment files MUST NOT contain secrets.

Public frontend configuration MUST be treated as visible to users.

All API communication MUST be:

- centralized;
- typed;
- consistently mapped;
- free from duplicated low-level request behavior.

Business logic MUST NOT be placed inside low-level HTTP clients.

Backend/OpenAPI transport DTOs MUST be represented accurately first. Frontend code MUST NOT invent response fields, silently change nullability, reinterpret enums, or alter date/time semantics. Feature adapters and view models MAY transform valid transport data for presentation, but presentation models MUST NOT pretend the backend supplied data it did not.

## HTTP Interceptors

HTTP interceptors MUST use Angular's functional interceptor style unless an approved exception is documented.

Interceptor behavior MUST be deterministic and ordered deliberately.

The final interceptor chain MUST account for:

- authentication header attachment;
- refresh coordination;
- one-time authentication replay where safe;
- Problem Details mapping;
- correlation and diagnostics where approved;
- request cancellation behavior.

Interceptors MUST NOT contain feature business logic.

Interceptors MUST NOT silently retry mutations.

## Date And Time Conventions

API timestamps MUST use the formats defined by the backend/OpenAPI contract, normally ISO 8601.

UTC timestamps MUST remain UTC during transport and state storage.

Conversion to the user's timezone belongs to presentation.

Locale-formatted date strings MUST NOT be sent to the API unless an endpoint explicitly requires them.

Date parsing and formatting MUST be centralized rather than duplicated across components.

Ambiguous timezone-free timestamps MUST NOT be silently interpreted without an explicit contract.

## OpenAPI Client Policy

The future **API CLIENT GENERATOR APPROVAL** gate MUST occur after workspace scaffold but before auth or feature API integration.

A generated Angular API client is preferred, but no generator tool is approved yet.

Future approved frontend design work MUST include a small OpenAPI generation spike using the real Development OpenAPI document.

The spike MUST verify:

- operation names;
- request and response DTOs;
- nullable fields;
- enums;
- pagination;
- multipart uploads;
- Problem Details;
- authentication configuration.

The spike MUST also verify Bearer-auth integration surface, CV multipart binary file handling, generated Angular/RxJS compatibility, output architecture/dependency footprint, generated-code Git policy, reproducible generation command, and regeneration determinism.

Only after that verification MAY one generator be approved.

If generation is adopted:

- generated files MUST live in a clearly isolated generated-code directory;
- generated files MUST NOT be edited manually;
- generation MUST use one repeatable command;
- features SHOULD use thin adapters when direct dependency on generated types would create excessive coupling.

Generator output that misrepresents the canonical OpenAPI contract causes STOP AND ESCALATE. Generated API code MUST remain isolated and MUST never be manually edited.

If generation is rejected, typed handwritten services are allowed only with the rejection reason documented in the approved frontend design specification.

Until the API CLIENT GENERATOR APPROVAL gate passes, feature/API integration is BLOCKED. Handwritten duplicate DTO contracts require explicit exception approval, and implementation using guessed handwritten backend DTOs is prohibited.

This architecture document does not select an OpenAPI generator.

## JSON Enums And Pagination

Enum handling MUST follow the actual OpenAPI/backend representation.

Components MUST NOT invent ad hoc enum serialization or parsing.

Unknown enum values MUST fail safely and remain diagnosable without crashing the application.

The current backend commonly exposes enum values as strings in response DTOs through server-side mapping. Query-bound enum values MUST still follow the OpenAPI/backend contract for the specific endpoint.

Pagination MUST use a shared typed contract matching the backend response shape: `items`, `page`, `pageSize`, `totalCount`, and `totalPages`.

Feature state owns page number, page size, filters, sorting, loading state, and total counts.

Changing filters or page size SHOULD reset the page when required to avoid invalid queries.

Generated or handwritten clients MUST preserve pagination metadata.

## Problem Details And Error Handling

RFC 7807 Problem Details is the standard backend error contract.

The frontend MUST handle standard fields including `type`, `title`, `status`, and `detail`.

The frontend MUST handle backend extensions including `traceId`, validation `errors`, `retryAfterSeconds`, and `Retry-After` response headers when present.

The frontend MUST handle:

- `400` validation errors by mapping field errors when possible;
- `401` through one refresh attempt or deterministic logout;
- `403` as authenticated but unauthorized;
- `404` as not found, including privacy-preserving not-found responses for resources hidden by ownership checks;
- `409` as a meaningful conflict without blind retry;
- `429` by respecting `Retry-After` when supplied;
- `503` as service unavailable with appropriate manual or bounded recovery.

The frontend MUST NOT show raw exception details.

Frontend behavior MUST NOT branch on human-readable `title` or `detail` text. Behavior MUST branch on HTTP status, documented structured extensions, and endpoint semantics.

Unknown Problem Details extensions MUST be tolerated without breaking error handling.

The `traceId` MAY be shown or copied for support purposes when doing so does not expose internal exception information.

Ownership-hidden `404` responses MUST NOT be converted into UI wording that reveals private resource existence.

Ownership-hidden `404` responses MUST remain generic in telemetry as well as the UI. Telemetry MUST NOT include private identifiers or resource contents merely because a hidden `404` occurred.

## User Feedback And Error Presentation

The frontend MUST NOT present every backend error as a snack bar. Feedback presentation MUST match the context, persistence, sensitivity, and recovery needs of the owning workflow.

- Field validation errors belong beside their fields and in the selected form state.
- Form-wide business or submission errors belong in a persistent form summary when appropriate.
- Page-loading failures belong in a page-level error state or persistent banner with a recovery action.
- `403` belongs in an unauthorized page or contextual authorization state.
- Privacy-preserving `404` belongs in a generic not-found page or contextual state.
- `409`, `429`, and `503` require contextual persistent feedback when the user must wait, retry, or make a decision.
- Payment, checkout, access-grant, exam-session, answer-persistence, submission, result, and security-sensitive states MUST remain visible inside the owning workflow.
- Snack bars are supplementary transient feedback and MUST NOT be the sole representation of critical workflow state.

UI behavior MUST branch on HTTP status, structured Problem Details fields, and endpoint semantics. Human-readable backend `title` or `detail` text MUST NOT be used directly as a programming contract.

Raw exception details MUST NOT be shown. Raw backend `detail` text MUST NOT be displayed automatically without a reviewed safe-message policy.

User-visible messages SHOULD come from an application-owned message catalog or intentional feature mapping. Validation errors MAY use safe structured field messages supplied by the backend.

The `traceId` MAY be displayed or copied for support when no sensitive data is exposed.

Tokens, private identifiers, payment data, uploaded-file data, exam answers, correct answers, rationales, and internal diagnostics MUST NOT appear in snack bars or other user feedback.

## Snack-Bar Architecture

Angular Material `MatSnackBar` is the approved notification rendering infrastructure. No separate npm toast library is approved.

Application feedback behavior and presentation MUST remain controlled by the project-owned feedback abstraction and custom snack-bar content component.

Core owns:

- the application-wide feedback service;
- typed feedback models;
- queueing;
- deduplication;
- lifecycle policy;
- the Angular Material snack-bar adapter.

Shared owns:

- the project-owned snack-bar presentation component;
- business-neutral visual feedback primitives.

The application shell owns the single application-level snack-bar or overlay presentation boundary where required by the final implementation.

Features:

- request feedback through the project-owned abstraction;
- MUST NOT depend directly on `MatSnackBar` for ordinary application feedback;
- MUST NOT create independent toast systems;
- MUST NOT duplicate notification queues.

The project-owned feedback API SHOULD support typed kinds such as success, information, warning, and error.

Final durations, positions, colors, icons, animation values, and responsive layout MUST remain deferred until approved Stitch/design-specification work.

Duplicate equivalent feedback SHOULD be deduplicated. Queue size and simultaneous visibility MUST be bounded.

Critical or actionable messages MUST NOT disappear before the user can perceive or act on them. Auto-dismiss duration MUST account for message length and accessibility.

Hover, keyboard focus, action interaction, and screen-reader behavior MUST be considered before dismissal. Snack-bar actions MUST be keyboard accessible.

Information and success announcements SHOULD use a polite live region. Urgent errors MAY use assertive announcement only when genuinely necessary.

A snack bar MUST NOT steal focus automatically. Reduced-motion preferences MUST be respected.

Untrusted HTML MUST NOT be rendered inside feedback messages.

Repeated server or polling failures MUST NOT flood the user with duplicate snack bars.

Logout or expired-session feedback MAY use a snack bar only as supplementary information; redirect and authentication-state transition remain authoritative.

The custom presentation component MUST be styled through the project SCSS theme and approved Angular Material customization APIs. It MUST NOT depend on undocumented Angular Material internal selectors.

## HTTP Retry And Idempotency

`POST`, `PUT`, `PATCH`, and `DELETE` requests MUST NOT be retried automatically.

Payment, checkout completion, access-grant, exam-start, and security-sensitive operations MUST NOT use blind retry.

Replay after successful refresh is authentication replay, not a general network retry.

Network failures, timeouts, `409`, and `503` MUST NOT trigger automatic replay of mutation requests.

Payment, checkout, exam-start, and other sensitive requests require their documented idempotency semantics.

Idempotent reads MAY use only bounded retry for explicitly approved transient failures.

Retry behavior MUST consider HTTP method, endpoint semantics, status code, and server guidance such as `Retry-After`.

An idempotency key represents one logical operation.

Browser-generated idempotency keys MUST use an approved cryptographically strong mechanism such as `crypto.randomUUID()`. `Math.random()` MUST NOT be used.

Keys are opaque. They MUST NOT contain user, order, exam, email, token, or other business or personal data, and they MUST NOT appear in URLs, logs, analytics, or user messages.

When generated by the frontend, an idempotency key MUST be generated by the owning feature workflow, not by a low-level interceptor.

The owning feature controls the key for the full lifetime of the logical operation. Re-rendering and rapid double-clicks MUST NOT create second logical operations.

A safe follow-up or replay of the same checkout, including recovery from an uncertain network outcome, MUST reuse the same idempotency key.

When a request with an idempotency key is safely replayed after authentication refresh, it MUST reuse the same key.

A genuinely new user-initiated logical operation MUST use a new idempotency key.

Components MUST NOT generate or replace payment idempotency keys independently of the owning feature workflow.

## Forms

Signal Forms are the preferred Angular-native first choice for new Nursing Platform forms when they satisfy Angular Material/control compatibility, required accessibility behavior, server-validation mapping, async validation behavior, typed backend-contract integration, and required UX behavior.

Do not force Signal Forms when a required Angular Material/custom-control workflow is incompatible or materially less reliable. Strictly typed Reactive Forms are the approved fallback for compatibility gaps or workflows where Signal Forms are unsuitable or risky. A feature selecting Reactive Forms MUST document that reason in its approved design specification or implementation plan.

Template-driven Forms MUST NOT be used for application workflows.

A single workflow MUST NOT mix form systems arbitrarily. The chosen form system MUST have clear ownership for value, validation, submission, and server-error state.

Before the first product form implementation, the owning form/pattern Task MUST verify the chosen form approach against the actual Angular 22 + Angular Material implementation.

Client validation improves user experience but never replaces backend validation.

Server validation remains authoritative.

Structured backend field errors MUST integrate into the selected form state and remain associated with the relevant controls or fields.

Forms MUST represent:

- submitting;
- validation-failure;
- server-failure;
- success states.

Server field errors SHOULD remain visible until the related value is changed or the request succeeds.

Complex business rules MUST NOT be duplicated in the frontend as an alternative source of truth.

## File Uploads

File uploads MUST use `multipart/form-data` with browser `FormData`.

Upload code MUST NOT manually set the multipart `Content-Type` boundary.

Client-side file-size and file-type validation improves user experience only; backend validation remains authoritative.

Each upload feature MUST verify current size, extension, content-type, and multipart-field requirements against the implemented backend and generated Development OpenAPI. Mutable upload validator values MUST remain in API documentation or the approved feature specification rather than being duplicated here as permanent authority.

File contents and sensitive file metadata MUST NOT be logged or placed in unrelated global state.

Upload progress MAY be exposed only when it provides real user value.

## Payments Trust Boundary

Price, currency, discounts if introduced, provider identity, payment and checkout status, `paidAt`, order totals, order-item snapshots, access grants, and entitlements are server-owned.

The frontend MUST NOT submit or trust client-computed amount, currency, paid status, provider, `paidAt`, access grant, or entitlement values.

Existing order-item snapshots returned by the backend are authoritative and MUST NOT be reconstructed from the current product catalog.

Integer minor units from the backend are authoritative. The frontend MUST preserve them for transport and state according to the current backend/OpenAPI contract.

Floating-point arithmetic MUST NOT be used as authoritative financial calculation.

Money display MUST use `Intl.NumberFormat` with the server-provided currency. Unsupported currencies or unsupported minor-unit assumptions MUST fail safely rather than silently invent formatting or arithmetic rules.

Checkout initialization can return `409` with `Retry-After` and `retryAfterSeconds` when initialization is already in progress. The UI MUST communicate this as a bounded wait/conflict state, not as a generic failure or blind retry trigger.

Checkout and Sandbox completion responses include `Cache-Control: no-store`; frontend code MUST treat these responses as sensitive and MUST NOT persist them outside the owning workflow state.

Development/Test Sandbox payment routes MUST NOT be enabled or called by Production frontend configuration.

Payment checkout URLs and server responses MUST be treated as server-owned data.

Checkout creation or navigation does not prove payment. Only backend order and fulfillment state prove payment and access.

Checkout URL validation MUST use parsing and positive allowlisting rather than only blacklist checks. Only `http` and `https` schemes MAY be accepted, and all other schemes MUST be rejected.

Production checkout navigation MUST use `https`, and Production provider origins MUST be explicitly approved. Development MAY use an explicitly configured local `http` origin.

## Sensitive Response No-Store Policy

Responses marked `Cache-Control: no-store` MUST NOT be copied into `localStorage`, `sessionStorage`, IndexedDB, a persistent application cache, or a service-worker cache.

Checkout URLs and Sandbox completion data MUST remain short-lived state owned by the active feature workflow.

Future caching layers MUST respect backend cache directives.

A generic no-store interceptor is not required unless an actual application cache requires centralized enforcement. Browser HTTP cache behavior and application-state persistence are separate concerns and MUST be reviewed separately.

## Exam Trust Boundary

Exam catalog and detail responses may expose `isFree` and `canStart` as UI hints.

The backend is authoritative for exam access. The frontend MUST NOT treat `isFree`, `canStart`, product state, local purchases, cached profile data, route guards, or JWT data as final permission to start an exam.

The frontend MUST NOT create, infer, consume, or mutate access grants. Payment-product existence and checkout navigation do not establish exam access.

`POST /api/v1/exams/{examId}/sessions` is the authoritative exam-session creation decision.

The backend owns exam timing, expiration, scoring, pass/fail, result availability, review availability, correct answers, and rationales.

Local timers are presentation only. Server session state and server timestamps are authoritative.

The frontend MUST accept server denial even when a cached `canStart` hint was previously `true`.

The frontend MUST NOT expose correct answers, rationales, scores, pass/fail state, or review data before the backend returns those fields through an approved review or result contract.

Local answer selection MAY update immediately for responsive user experience. Local selection MUST NOT be presented as server-saved or submitted before backend confirmation.

Saved, synchronized, submitted, scoring, and completion states are server-authoritative. Failed answer persistence MUST remain visible as an unsaved or failed state, and retry behavior MUST follow the mutation and idempotency rules in this document.

The frontend MUST NOT use optimistic updates for exam-session start, answer submission, final submission, results, or purchased exam access.

Ownership-hidden session responses MUST preserve privacy and MUST NOT reveal whether another user's exam session exists.

## Sensitive Operations

The frontend MUST NOT use optimistic updates for:

- payment status;
- checkout completion;
- access grants;
- paid-exam access;
- exam-session start;
- security-sensitive account operations.

Sensitive workflows MUST keep state scoped to the owning feature unless application-wide state is explicitly required.

Sensitive server responses MUST NOT be written to long-lived browser storage unless the storage behavior is explicitly approved.

## Testing Principles

Vitest is the approved unit and component test runner for the Angular 22 frontend foundation because it is the Angular CLI default for Angular 22.

Vitest MUST remain the sole unit and component test runner. The frontend MUST NOT introduce a competing unit-test runner.

Angular TestBed and official Angular testing utilities are approved.

Unit tests for Angular `HttpClient` services, interceptors, and adapters MUST use Angular's official HTTP testing providers and `HttpTestingController`.

Integration and E2E tests intentionally validating the real backend MUST NOT replace that backend with `HttpTestingController`.

Fast DOM emulation is appropriate for most unit and component tests. Browser-specific behavior MUST run in a real browser when DOM emulation is insufficient.

A Playwright-backed Vitest browser provider MAY be added when a feature documents and verifies the need for browser execution.

The architecture requires:

- component tests;
- service tests;
- auth-state tests;
- token-storage abstraction tests;
- interceptor tests;
- single-flight refresh tests;
- route-guard tests;
- Problem Details mapping tests;
- form validation tests;
- permission-aware presentation tests;
- payment trust-boundary tests;
- exam trust-boundary tests;
- feedback service tests;
- feedback queue and deduplication tests;
- Angular Material snack-bar adapter tests;
- contextual error-presentation policy tests;
- validation-error mapping tests;
- no-raw-server-error exposure tests;
- accessibility announcement tests;
- keyboard action and dismissal tests;
- reduced-motion behavior tests where animation exists;
- theme-token and Angular Material theme-integration smoke tests;
- checks preventing Tailwind dependencies or configuration from being introduced;
- accessibility-focused component tests where practical.

Automated accessibility testing MUST include AXE. AXE does not replace keyboard, focus, screen-reader, or manual accessibility review.

Playwright is the approved future E2E framework.

Cypress MUST NOT be introduced in parallel.

Critical browser journeys MUST eventually run against the real Development backend, including the real Sandbox payment path where applicable.

This architecture document MUST NOT establish a permanent repository-wide coverage percentage.

Coverage thresholds belong in phase specifications and CI decisions, while security-critical behavior MUST always have direct behavioral tests regardless of percentage.

Visual snapshot testing is not a permanent requirement and MUST NOT be adopted as one without separate approval.

## Frontend Task Definition of Done

A frontend Task may be marked `VERIFIED` only when all applicable requirements pass:

- requested business behavior implemented;
- backend/OpenAPI contract respected exactly;
- no guessed response fields/contracts;
- architecture/dependency rules respected;
- approved Stitch/design contract respected;
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
- task ledger updated with evidence (per-Task acceptance mapping retained inside a shared batch review);
- `PROGRESS.md` handoff updated only when current milestone/state changes, kept compact;
- completion commit recorded once committed (prefer one logical batch commit where the GOAL authorizes it).

A screenshot matching Penpot alone is never sufficient Definition of Done.

## Browser Policy

Angular 22's official Web Platform Baseline is the compatibility authority.

Internet Explorer is not supported.

The project browser configuration becomes the executable implementation of that policy after scaffolding. It MUST NOT expand or narrow support without approval and compatibility verification.

Polyfills require a demonstrated capability gap and tests. Polyfills MUST NOT be used to claim support for a browser that Angular 22 otherwise does not support.

## Browser Security

Untrusted content MUST NOT be injected through unsafe HTML APIs.

Production MUST use AOT compilation. JIT and runtime-compiled dynamic templates MUST NOT be used in Production.

Templates MUST NOT be constructed from user input.

Angular sanitization bypass APIs require narrow scope, documented justification, and explicit security review.

Third-party scripts, analytics SDKs, tag managers, embedded widgets, and remote executable content require explicit approval.

Content Security Policy MUST be designed before Production. The frontend MUST NOT introduce `unsafe-eval`, and `unsafe-inline` MUST NOT be broadly enabled without explicit security review.

Trusted Types MUST be evaluated and enabled when compatible. An exception requires documented security review.

Direct use of `document`, `ElementRef.nativeElement`, and third-party DOM manipulation MUST be minimized and narrowly owned.

Authentication data, payment data, private profile data, uploaded file content, exam answers, exam results, correct answers, rationales, and tokens MUST NOT be exposed through console logs, analytics, URLs, or client-side error reports.

Browser storage MUST be treated as user-visible and script-readable unless the storage mechanism explicitly provides stronger guarantees.

## Performance Rules

Major portal and feature routes MUST be lazy-loaded.

Production MUST define bundle budgets. CI MUST fail when approved error budgets are exceeded.

Performance decisions MUST be measured with Angular build output, browser tooling, or production-like telemetry when available.

Large datasets MUST use backend pagination, filtering, and sorting. The frontend MUST NOT download all candidates, users, exams, orders, sessions, or attempts and filter them entirely in the browser.

Polling MUST NOT be introduced without explicit interval, cancellation, backoff, and ownership rules.

Static supported images MUST use `NgOptimizedImage`. Images and other media MUST be optimized for size, responsive display, and accessibility.

Expensive derived UI state MUST use `computed()` rather than repeated template work.

Stale requests SHOULD be cancelled when newer input or navigation supersedes them.

Duplicate safe HTTP work MAY be shared only when ownership, lifetime, caching, and invalidation rules are explicit.

Network waterfalls SHOULD be avoided in route activation and dashboard loading when requests can be safely coordinated.

Performance optimizations MUST NOT weaken correctness, security, or accessibility.

Angular Material imports MUST remain selective to preserve tree shaking.

Global CSS and component-style budgets MUST be established during scaffold planning. Angular CLI `anyComponentStyle` or the appropriate supported style budget SHOULD be evaluated.

Duplicate theme generation MUST be prevented. Unused global selectors and unused SCSS utilities MUST NOT accumulate.

Feature deletion MUST allow its private SCSS to be removed with the feature.

A shared style abstraction requires demonstrated reuse. A one-off style MUST NOT be promoted into a global mixin or utility prematurely.

Styling optimization MUST be measured with Production builds and browser tooling.

Organization and reuse MUST NOT create excessive abstraction or a universal styling framework inside the project.

## Build And Deployment Configuration

Production builds MUST use Angular production optimizations and AOT compilation.

Development-only routes, providers, mocks, diagnostics, and Sandbox payment affordances MUST be excluded from Production configuration.

Frontend configuration MUST distinguish public configuration from secrets. Secrets MUST NOT be shipped in browser bundles or environment files.

Deployment topology decisions, including same-origin hosting or separate-origin CORS, MUST be made through approved deployment design.

## Production Diagnostics

Debug logging, development tooling, test hooks, verbose diagnostics, and Sandbox affordances MUST NOT ship enabled in Production.

Source-map generation and publication require an approved policy. Source maps MUST NOT be publicly exposed by default; private upload to an approved monitoring provider MAY be allowed.

Production monitoring and diagnostics MUST redact sensitive payloads.

Client-side error reporting, telemetry, analytics, and logging MUST redact or omit tokens, payment values where sensitive, uploaded file metadata, exam answers, exam results, correct answers, rationales, profile data, and raw API payloads unless an explicit privacy and security review approves the data.

Trace identifiers MAY be captured for support correlation when they do not expose sensitive details.

Diagnostic sampling, retention, provider selection, and user-consent behavior require explicit approval before Production.

## Agent And Implementation Rules

Frontend agents MUST NOT:

- change the approved stack without explicit approval;
- introduce a state library or UI library without explicit approval;
- invent visual design that belongs to Stitch/design authority;
- edit generated API code manually;
- modify backend code to make frontend work easier unless the task explicitly authorizes backend changes;
- mix unrelated features into a foundation task;
- refactor, redesign, rename, move, or behaviorally modify VERIFIED scope unless the new task explicitly declares it as an affected dependency or REOPENED scope;
- combine unrelated cleanup, modernization, dependency upgrades, formatting migrations, or architecture changes with a feature Task;
- expose tokens or secrets;
- implement Production access to Sandbox routes;
- stage, commit, merge, or push without explicit approval.

Every implementation phase MUST:

- have an approved design specification;
- have an approved implementation plan before code changes begin;
- use an isolated Git worktree unless an explicit reviewed exception exists;
- use TDD for behavior and logic;
- run focused tests;
- run the full applicable frontend test suite;
- run lint;
- run a production build;
- verify git status and staged files;
- stop for review before commit or merge.

Every implementation Task has an authorized scope. Agents may modify only the files/modules explicitly listed by the Task plus directly necessary dependency files. Discovery of a desirable broader refactor does not authorize that refactor. Agents MUST stop and escalate before widening scope.

If a completed area must change, the new task MUST record why it is being reopened, identify the dependent task/decision, mark the item `REOPENED`, define authorized affected files/modules, rerun its acceptance/verification gates, record the new verification evidence, and return it to `VERIFIED`. Opportunistic cleanup of verified features is forbidden.

Do not upgrade, replace, modernize, or rewrite an already approved frontend approach merely because another Angular pattern, dependency, API, or technique is newer or personally preferred. Changes to approved architecture require an explicit decision task.

## Deferred Decisions

The following decisions remain intentionally deferred:

- exact Node.js version;
- exact OpenAPI generator;
- final Stitch-approved colors, typography, spacing, layouts, and component appearance;
- SSR, SSG, hybrid rendering, PWA, and offline support;
- full Arabic localization and full RTL implementation;
- production refresh-token storage strategy;
- dedicated frontend deployment topology;
- permanent coverage thresholds.
