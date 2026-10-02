# Clean Architecture Boundaries

Status: Accepted.

## Context and authority

The repository's governing rules require the backend to follow the approved Clean Architecture boundaries. [Architecture Overview](../architecture-overview.md) owns the architectural direction, and [Backend Architecture](../../backend/backend-architecture.md) owns the layer and dependency implementation. The legacy system architecture described the same layering.

## Decision

Presentation invokes Application use cases. Application depends on Domain and defines interfaces needed from external systems. Infrastructure implements those interfaces. Domain must not depend on Presentation or Infrastructure; Presentation remains thin.

## Rationale and consequences

This boundary keeps domain behavior testable without an HTTP server or database adapter and permits external implementations to change without moving business rules. It constrains where new code can live. It does not assert that every current source file already complies; implementation conformance is reviewed in the technical domains.
