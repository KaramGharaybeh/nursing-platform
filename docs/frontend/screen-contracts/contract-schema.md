# Screen Contract Schema

## Row Fields

Every routable screen or non-routable state entry must define these fields when applicable:

| Field | Meaning |
|---|---|
| `Screen ID` | Stable screen/state identifier such as `AUTH-001`, `EXM-007`, or `SYS-001`; the identifier does not create Product scope. |
| `Route ID` | Canonical route ID from `frontend/src/app/core/routing/canonical-routes.ts`, or `NOT_ROUTABLE`. |
| `Path` | Canonical path from route source, or owning route/action when non-routable. |
| `Disposition` | Explicitly approved deferred or blocked concept when applicable; do not copy dated execution readiness. |
| `Access` | `PUBLIC`, `ENTRY`, `AUTHENTICATED_ONLY`, role-only, or role-and-permission requirement. |
| `Authority` | Approved packet, registry, backend/OpenAPI, Stitch/design source, or explicit decision that owns the row. |
| `Required` | User-visible content, actions, state handling, and constraints that must be preserved. |
| `Allowed` | Optional visual or implementation expression permitted by authority. |
| `Forbidden` | Claims, controls, fields, or behaviors that must not appear. |
| `Notes` | Boundaries, gaps, implementation deferrals, or references to related contracts. |

## Disposition vocabulary

| Value | Meaning |
|---|---|
| `BACKEND_BLOCKED` | An approved screen decision identifies insufficient backend/API authority for this concept. It is not a Product prohibition. |
| `DEFERRED` | Intentionally postponed by explicit decision; do not implement or present as active behavior. |
| `NOT_ROUTABLE` | Route-field value for a state/action shown inside another route; it is not a readiness status. |
| `DESIGN_PROPOSED_FEATURE` | A design-discovered idea recorded in the DPF register; it is not Product or implementation authority. |

Legacy `CONTRACT_READY`, `PARTIAL`, and `AUTHORITY_GAP` labels describe dated design/execution readiness. The permanent family contracts record approved presentation and explicit blocked/deferred boundaries without turning those labels into current delivery status.

## Required Evidence Rules

Route IDs and paths must come from `frontend/src/app/core/routing/canonical-routes.ts`.

Access rules must come from `route-classification.ts`, `route-permission-policy.ts`, and `navigation-permission-policy.ts`.

Screen IDs and screen decisions come from the [screen index](screen-index.md), applicable family contract, screen approval packets, and current explicit decisions. Route identity and guard UX belong to [Routing and Permissions](../routing-and-permissions.md).

Visual composition must come from `docs/frontend/design/stitch/DESIGN.md` and human-approved Stitch artifacts. A Stitch draft that has not passed human visual approval is evidence only.

Current runtime behavior, DTOs, errors, and validation must be checked against backend source and [OpenAPI](../../api/openapi.yaml). Product owns approved behavior and permission meaning; Security owns protected enforcement. Implementation alone does not create Product approval.

## Forbidden Inferences

Do not create a screen because it appears in a design draft, Storybook story, route group label, navigation idea, or agent suggestion.

Do not promote a non-routable state into a route.

Do not treat `DESIGN_PROPOSED_FEATURE` items as implemented product behavior.

Do not show unsupported compliance, security, availability, auth-token, service-health, or accessibility claims as factual content.

Do not infer backend fields, enum values, validation, ownership, pricing, payment state, exam eligibility, entitlement rights, permissions, or report content from visual examples.

Do not use route names as user-facing labels unless copy authority exists.
