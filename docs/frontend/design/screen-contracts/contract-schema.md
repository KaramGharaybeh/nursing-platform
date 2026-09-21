# Screen Contract Schema

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-SCHEMA
status: active-schema
owner: frontend-design-governance
updated: 2026-09-21
```

## Row Fields

Every routable screen or non-routable state entry must define these fields when applicable:

| Field | Meaning |
|---|---|
| `Screen ID` | Stable design/product screen identifier such as `AUTH-001`, `EXM-007`, or `SYS-001`. |
| `Route ID` | Canonical route ID from `frontend/src/app/core/routing/canonical-routes.ts`, or `NOT_ROUTABLE`. |
| `Path` | Canonical path from route source, or owning route/action when non-routable. |
| `Status` | Readiness/status value from this schema. |
| `Access` | `PUBLIC`, `ENTRY`, `AUTHENTICATED_ONLY`, role-only, or role-and-permission requirement. |
| `Authority` | Approved packet, registry, backend/OpenAPI, Stitch/design source, or explicit decision that owns the row. |
| `Required` | User-visible content, actions, state handling, and constraints that must be preserved. |
| `Allowed` | Optional visual or implementation expression permitted by authority. |
| `Forbidden` | Claims, controls, fields, or behaviors that must not appear. |
| `Notes` | Boundaries, gaps, implementation deferrals, or references to related contracts. |

## Status Vocabulary

| Status | Meaning |
|---|---|
| `CONTRACT_READY` | Enough authority is extracted here for a future agent to design or implement without inventing product behavior, subject to separate task authorization. |
| `PARTIAL` | Meaningful authority exists, but required contract detail or implementation reality remains incomplete. |
| `AUTHORITY_GAP` | Product, design, business, or screen decision is absent. |
| `BACKEND_BLOCKED` | Required backend/API authority does not exist or is insufficient for the screen/function. |
| `DEFERRED` | Intentionally postponed by explicit decision; do not implement or present as active behavior. |
| `NOT_ROUTABLE` | Use only in route fields for states/actions shown inside another route; status must still be one of the readiness statuses above. |
| `DESIGN_PROPOSED_FEATURE` | Useful design-discovered idea recorded in the DPF register; not product/implementation authority. |
| `UNSUPPORTED_CLAIM` | Claim must not be shown as factual user-facing content without separate verification and approval. |

## Required Evidence Rules

Route IDs and paths must come from `frontend/src/app/core/routing/canonical-routes.ts`.

Access rules must come from `route-classification.ts`, `route-permission-policy.ts`, and `navigation-permission-policy.ts`.

Screen IDs and screen decisions must come from `page-registry.md`, `route-permission-matrix.md`, screen approval packets, current explicit decisions, or a later approved replacement source.

Visual composition must come from `docs/frontend/design/stitch/DESIGN.md` and human-approved Stitch artifacts. A Stitch draft that has not passed human visual approval is evidence only.

Runtime behavior, DTOs, permissions, errors, validation, sensitive-data exposure, ownership, exam/payment/package rules, and security outcomes must come from backend source and generated Development OpenAPI.

## Forbidden Inferences

Do not create a screen because it appears in a design draft, Storybook story, route group label, navigation idea, or agent suggestion.

Do not promote a non-routable state into a route.

Do not treat `DESIGN_PROPOSED_FEATURE` items as implemented product behavior.

Do not show unsupported compliance, security, availability, auth-token, service-health, or accessibility claims as factual content.

Do not infer backend fields, enum values, validation, ownership, pricing, payment state, exam eligibility, entitlement rights, permissions, or report content from visual examples.

Do not use route names as user-facing labels unless copy authority exists.
