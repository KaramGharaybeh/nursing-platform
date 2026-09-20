# Open Questions and Discrepancy Register

```yaml
document_id: NPS-DES-GOV-OPEN-001
version: 1.2
status: g0-accepted-phase-1-contract-questions-open
recorded_at: 2026-08-12
timezone: Asia/Amman
```

## Policy

Every unresolved conflict remains explicit. No agent may infer a route, permission, validation boundary, error status, business transition, page classification, or visual approval to close an item. `Open` and `Deferred` items are not Phase 0 failures when they have an owner and cannot change the authorized Phase 0 workflow.

## Phase 0 discrepancies

| ID | Discrepancy | Evidence | Owner / target | Status |
|---|---|---|---|---|
| DISC-PEN-001 | Page 07 contains three root boards at `(0,0)` and mixes Utilities with legacy Product Map and User Flows artifacts. | Live Penpot structure and sequential exports | Manager, Phase 1 evidence classification before G1 | Open |
| DISC-PEN-002 | Page 07 Utilities is sparse while Page 01 describes Utilities as complete; the design-system audit describes it as substantively incomplete. | Pages 01 and 07; `design-system-audit.md` | Manager, Phase 1 evidence and Phase 2 foundation | Open |
| DISC-PEN-003 | Page 09 has conflicting classifications. Its superseded audit says incomplete, its fix tracker says complete with no unresolved findings, and current export evidence renders successfully. | Both Page 09 review records and live export | Manager with Karam review, Phase 1 | Open |
| DISC-PEN-004 | Page 10 is not a complete production-ready package. The main documentation board is blank, all ten sections are empty, and multiple sections exceed clipped bounds. | Live Page 10 structure and export | Manager, preserve as legacy until Phase 6 re-evaluation | Deferred |
| DISC-PEN-005 | Page 10 Tablet and Mobile Brand Panels are outside parent bounds and clipped. | Live geometry and viewport exports | Future responsive scope owner | Deferred; outside Desktop scope |
| DISC-PEN-006 | Page 10 Desktop RTL and Mobile RTL boards are empty. | Zero child counts and blank exports | Future RTL scope owner | Deferred; outside current scope |
| DISC-PEN-007 | AUTH-001 records a stale Mobile board ID and completion language that conflicts with live clipping, empty state sections, and empty RTL boards. | Legacy tracker and live Page 10 inventory | Manager, Phase 1 evidence classification | Open; legacy file remains untouched |
| DISC-PEN-008 | Page 10 contains two orphan root-level `Brand Abbreviation` texts at `(0,0)`. | Live root structure | Penpot foundation owner, Phase 6 | Deferred |
| DISC-PEN-009 | Page 03 contains the unrelated root-level text `How I can avoid timeout that from MCP?`. | Live Page 03 root structure | Penpot foundation owner, Phase 6 | Deferred |
| DISC-PEN-010 | Page 04's `Section — Semantic Radius Usage` uses `x=27` while peer sections use `x=96`. | Live Page 04 geometry | Foundation reviewer, Phase 2 then Phase 6 | Open |
| DISC-PEN-011 | Page 02 documents primary teal `#006B66`, while the existing AUTH-001 Desktop draft uses `#00796B`. | Live Page 02 and Page 10 evidence | Foundation owner, Phase 2 | Open; no color is approved by Phase 0 |
| DISC-PEN-012 | The local Penpot library has zero components and zero colors despite extensive component and color documentation. | Live local library inventory | Foundation owner, Phase 2 and Phase 6 | Open |
| DISC-PEN-013 | Thirty-four typography assets use repeated generic names, and no typography token set was reported. | Live local library inventory | Foundation owner, Phase 2 and Phase 6 | Open |
| DISC-PEN-014 | Only spacing/shape and elevation/state token sets were reported; no color token set, typography token set, or token theme exists. | Live token inventory | Foundation owner, Phase 2 | Open |
| DISC-REP-001 | Historical Phase 0 Figma/Penpot wording has been superseded for the active Stitch redesign by DEC-PH0-020. | `docs/frontend/design/governance/decision-log.md` DEC-PH0-020; active governance reconciliation | Karam/reviewer during repository integration | Superseded for active Stitch redesign; historical evidence preserved |
| DISC-REP-002 | `CURRENT_TASK.md` awaits selection of the next implementation phase and `TASKS.md` keeps Frontend at Phase 10, while this design-documentation program is active. | Live milestone and roadmap | Karam | Resolved for Phase 0 only by explicit bounded authorization; implementation remains unauthorized |
| DISC-REP-003 | `.agent/goal-state.md` describes active responsive/RTL AUTH-001 work, conflicting with the Desktop-only program goal. | Legacy and program goal-state files | Karam | Ownership resolved; legacy file intentionally untouched |
| DISC-REP-004 | The frontend architecture is approved, but `frontend/` is uninitialized. | README and live filesystem audit | Future frontend implementation owner | Not a defect; implementation remains not started |

## Phase 1 contract questions

These questions are explicitly owned by Phase 1. Phase 0 does not answer them.

| ID | Question | Required evidence | Blocks | Status | Diagnostic finding |
|---|---|---|---|---|---|
| OPEN-PH1-001 | Where and how is FluentValidation invoked at runtime for Minimal API requests? | Live request pipeline, endpoint behavior, and focused tests | Validation index and form error contracts | Open | FluentValidation is invoked via **explicit manual injection only**, not via MediatR Pipeline Behaviors or ASP.NET Core Filters. `AddValidatorsFromAssembly` registers all `AbstractValidator<T>` subclasses in DI (`Application/DependencyInjection.cs:20`). The **only** runtime invocation is a single `ValidateAndThrowAsync` call in `PreparationPackageEndpointExtensions.cs:146` for `ListPreparationPackageOffersQuery`. All other endpoints (~50+ validators) are registered but **never invoked at runtime**. No `IPipelineBehavior`, no `ValidationBehavior<TRequest, TResponse>`, no `IActionFilter` for validation exists in the codebase. |
| OPEN-PH1-002 | What exact Development OpenAPI artifact or capture procedure will be the recorded contract revision? | Generated Development OpenAPI from the verified application revision | API operation index and client-generation spike | **Resolved** | Current canonical artifact for active frontend planning is `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` (OpenAPI 3.1.1, SHA-256 `6ceeb0551d43bd320100f25fea3073874ad1a7df0c10f04752e16cd7b534292b`). Earlier artifact `development-openapi-2026-08-16.json` (SHA-256 `aa96da71259c9fc8c91d54b42e17741b6988fdaa7e5308128e3da26b08497f99`) is historical/superseded for active implementation planning. Capture procedure documented in `development-openapi-capture-procedure.md`. CST-001–008 stabilization committed at `77cc3b7`. |
| OPEN-PH1-003 | Which implemented exception or business-rule paths return `422`, and how are they mapped to Problem Details? | Exception middleware, handlers, endpoint tests, and generated OpenAPI | Error-response index and page state contracts | Open | **No `422` mapping exists.** `ExceptionMiddleware.cs:45` maps `FluentValidation.ValidationException` to **400 Bad Request** (not 422). The middleware uses `ValidationProblemDetailsContract` with an `errors` dictionary grouped by property name. No `ValidationExceptionHandler`, `ProblemDetailsExceptionMapping`, `IExceptionHandler`, or `UseExceptionHandler()` exists. `ExceptionMiddleware` is the **sole exception handling mechanism** in the pipeline (registered first/outermost at `ApplicationBuilderExtensions.cs:90`). The `Status422UnprocessableEntity` constant is never referenced anywhere in the codebase. |
| OPEN-PH1-004 | Which exact permissions protect current or planned administration payment operations? | Endpoint mappings, seeded permission names, authorization tests, and roadmap status | Administration/payment page candidates | Open | Two-tier authorization: **(1) Admin payment endpoints** (`/admin/payment/products/*`) use `RequirePermission(...)` with `Permissions.Exams.View` (reads) and `Permissions.Exams.Edit` (writes) — enforced via `PermissionAuthorizationHandler` → `PermissionService` → DB query (`UserRoles → Role → RolePermissions → Permission.Name`). **(2) Consumer/nurse payment endpoints** (`/api/v1/payment/products/*`, `/api/v1/me/nurse-profile/payment/orders/*`) use **only** `.RequireAuthorization()` — no permission check; any authenticated user passes. 25 distinct permission strings defined in `Permissions.cs`. No role-based authorization (`[Authorize(Roles)]`) exists anywhere. |
| OPEN-PH1-005 | What generated OpenAPI revision and source extract will evidence each Preparation Package operation at `8439511`? | Generated Development OpenAPI, endpoint mappings, DTOs, validators, permission tests, and Problem Details behavior | Proposed Preparation Package evidence packet | **Resolved** | CST-001–008 stabilization committed at `77cc3b7` (typed payment responses, Bearer security, Problem Details variants, numeric string transport, required/nullable metadata, exception middleware fixes, 6 new test files). Captured artifact reflects stabilized contracts. Frontend contract baseline (`preparation-package-frontend-contract-baseline.md`) authored at `8439511` handoff — sections 8–12 classify unresolved items pending OpenAPI capture, now resolved. |

## Existing program decisions still open

| ID | Decision needed | Owner / target | Status |
|---|---|---|---|
| OPEN-001 | Canonical Desktop content viewport | Karam, Phase 2 | Open; `1440x1024` is precedent only |
| OPEN-002 | Supported Desktop width range | Karam, Phase 2 | Open |
| OPEN-003 | Pinned browser/version, OS, DPR, and fonts | Karam, Phase 2 | Open |
| OPEN-004 | Initial locale and timezone | Karam, Phase 2 | Open; English LTR and `Asia/Amman` remain recommendations only |
| OPEN-005 | One calibrated pixel-diff tolerance | Karam after deterministic pilot | Open |
| OPEN-006 | Quantitative frontend performance budgets | Frontend architecture owner after runtime baseline | Open |
| OPEN-007 | Release-specific payment page behavior | Manager, Phase 1 evidence | Open |
| OPEN-008 | Employer and organization ownership model | Manager, Phase 1 evidence | Open |
| OPEN-009 | Exact exam timer, resume, attempt, and rationale behavior | Manager, Phase 1 evidence | Open |

## Phase 2 Stitch questions

| ID | Question | Owner / target | Status |
|---|---|---|---|
| OPEN-STITCH-001 | Can the Stitch generation timeout for the first Phase 2 app-shell artifact be retried, or should generation proceed through another approved Stitch path? | Karam / Stitch tooling | Open; `stitch_generate_screen_from_text` timed out for `Shell / APP-SHELL / Nurse / Desktop`, repeated `stitch_list_screens` checks returned no screen IDs, and the tool instruction says not to retry a timeout without recovery. |
| OPEN-STITCH-002 | Should the rejected `stitch_update_design_system` endpoint be investigated separately, or is the created design system sufficient because `stitch_create_design_system` stored the canonical DESIGN.md content? | Karam / Stitch tooling | Open; create succeeded and list confirmed stored content, but the required immediate update endpoint returned `Request contains an invalid argument`. |
| OPEN-STITCH-003 | Why did `stitch_edit_screens` report successful DOM operations on the first Nurse desktop shell while the visible/browser artifact and downloaded HTML retained prohibited original content? | Karam / Stitch tooling | Open; edit-tool success is not accepted as persistence proof. Future recovery uses closed-world regeneration plus HTML/content validation rather than treating edit success as approval. |
| OPEN-STITCH-004 | Should strict Stitch contract validation treat non-visible raw HTML comments, document titles, Tailwind configuration, JavaScript scaffolding, and material icon ligature scaffolding as prohibited internal annotations, or only visible/rendered content? | Karam / Stitch tooling | Resolved by human browser review evidence and validation-semantics correction: the product contract applies to user-visible and accessibility-exposed product content/behavior. Implementation-internal raw source scaffolding and preview metadata are reported but do not fail the product contract unless exposed to users, interactive, security-sensitive, or behavior-changing. |
| OPEN-STITCH-005 | Why does `stitch_get_screen` retrieve replacement screen `projects/17116545761229201855/screens/96852332a6f048039bf76fc459d8f07e` while `stitch_list_screens` omits it and lists only the original invalid screen? | Karam / Stitch tooling | Open; direct screen retrieval and list API are inconsistent after the single authorized replacement generation. |

## Phase 2 Stitch decisions resolved

| ID | Decision | Evidence | Status |
|---|---|---|---|
| HD-STITCH-01 | Hybrid app shell: persistent top app bar, no permanent global sidebar, mobile accessible drawer/menu. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-02 | Navigation by user goals/product families; Nurse primary families are Exams, Preparation Packages, Commerce when available, and Profile. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-03 | Icons support labels; no icon-only primary navigation; directional icons mirror in RTL and status icons do not. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-04 | Root `/` is actor/state-driven; no fake universal dashboard. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-05 | Offline/maintenance visual states may be factual only and must not claim offline write/payment/exam safety without runtime proof. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-06 | Admin dense desktop data prefers semantic/native tables; mobile cards/lists unless cross-column comparison requires overflow. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-07 | Phase 2 design system defines missing token roles for icons, focus, motion, states, overlays, navigation, mobile spacing, and mixed-direction numeric/monetary content. | `docs/frontend/design/governance/decision-log.md` | Resolved |
| HD-STITCH-08 | Closed-world Stitch generation baseline and validation-semantics correction. | `docs/frontend/design/governance/decision-log.md` | Superseded for exploratory generation by HD-STITCH-09; retained as historical strict-mode evidence. |
| HD-STITCH-09 | Design-led feature discovery: unlisted generated functionality is classified, useful proposals enter a DPF backlog, unsupported factual claims remain prohibited, and first Nurse desktop shell is the preferred visual baseline. | `docs/frontend/design/governance/decision-log.md` | Resolved |

## Phase 0 review question

| ID | Question | Owner | Status |
|---|---|---|---|
| OPEN-G0-001 | Does the five-file re-entry reconciliation accurately record authority, scope, `8439511` handoff status, Penpot evidence, and discrepancy ownership? | Karam acceptance, 2026-08-12; DEC-PH0-016 | Resolved; G0 governance/re-entry baseline accepted. Does not authorize Phase 1 without a separate task. |

## Proposed Phase 1 Preparation Package Evidence Packet

G0 is accepted. A separately authorized Phase 1 packet may extract routes, exact permissions, DTOs, validation, Problem Details, ownership, and tested states for the implemented Preparation Package backend-ready areas: reporting topics/profiles, materials, practice collections, package composition/offers, entitlement reads, practice progress, package exam start, and analytical report reads. It must not create page specifications or design artifacts, and it must exclude storage/delivery, offline access, workspace aggregation, adaptive practice, retraining, and employer package visibility.
