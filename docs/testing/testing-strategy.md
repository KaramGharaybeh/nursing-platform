# Testing Strategy

## Authority and scope

This document owns the verification model. [Product requirements](../product/requirements.md) and [business rules](../product/business-rules.md) define required behavior; [Architecture](../architecture/architecture-overview.md), [Frontend](../frontend/frontend-architecture.md), [Backend](../backend/backend-architecture.md), [API](../api/openapi.yaml), and [Security](../security/security-overview.md) own their technical contracts. Tests provide implementation evidence; they do not create these contracts. [Acceptance criteria](acceptance-criteria.md), [E2E scenarios](end-to-end-scenarios.md), [test data](test-data-catalog.md), and the [dependency graph](dependency-graph.md) own their respective verification details.

## Verification layers

| Layer | Responsibility | Current mechanism evidenced in repository |
|---|---|---|
| Domain and application | Isolated domain behavior, validation, state transitions, and failure paths against the approved owner. | xUnit Domain and Application test projects under `backend/tests/`. |
| Persistence and integration | Database mappings, constraints, migrations, and integration behavior where a real dependency matters. | Infrastructure test project; some PostgreSQL tests require a configured test connection. |
| API boundary | HTTP status, DTO/schema, validation, error response, and protected access against [OpenAPI](../api/openapi.yaml) and [API guidelines](../api/api-guidelines-and-errors.md). | WebApi test project. Its `WebApiTestFactory` substitutes the application sender and permission service and suppresses database initialization; those tests do not alone establish a full database journey. |
| Frontend unit and component | Component behavior, states, routing/guard UX, and rendered accessibility against [Frontend contracts](../frontend/frontend-architecture.md) and [screen contracts](../frontend/screen-contracts/screen-index.md). | Angular TestBed and Vitest `*.spec.ts` files. Frontend owns its test implementation conventions. |
| Cross-boundary E2E | Actor-visible workflows across browser, API, persistence, and external test substitutes, with independently established expected outcomes. | Playwright currently selects `auth-*.spec.ts` only. This is automation evidence, not the required scenario inventory. |
| Visual review | Inspect screen presentation and component states against approved visual authority. | Storybook stories are an available review surface. They do not establish Product or visual approval and do not replace behavior tests. |

[Security verification](../security/security-verification.md) owns authn/authz and sensitive-data control checks; those checks participate in applicable test layers. [Frontend accessibility](../frontend/accessibility.md) owns the implementation obligation. Frontend testing guidance calls for automated AXE checks and manual accessibility review; a rendered story alone does not verify accessibility.

## Evidence and isolation

For an acceptance claim, identify its authority, check, input/prerequisite, observed result, and limitations. A named test or modeled scenario is not a passing result. Verify critical workflows and edge cases at a boundary able to observe the claimed outcome. Use deterministic assertions and isolate mutable test state. Setup that is itself business behavior should be exercised through the real flow when that flow is the subject of verification. Data and prerequisite relationships are described in [test data](test-data-catalog.md) and the [dependency graph](dependency-graph.md).

The repository's current commands and CI wiring are implementation evidence, not this strategy's definition of completeness. Coverage and CI gaps belong to Delivery current state.
