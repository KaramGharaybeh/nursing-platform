# Preparation Package Entitlement and Attempt Provenance

Status: Accepted.

## Context and authority

The approved Preparation Package umbrella, Stage 2 purchase, and Stage 3 examination specifications require a package purchase to yield identifiable benefits and an exam attempt to retain its purchase source. Standalone exam access already has a different grant path.

## Decision

Successful order-item fulfillment uses the purchased snapshot to establish a nurse-owned package entitlement with independently authorizable benefit rights. Each right is a persisted child of the entitlement rather than a computed-only value. Package exam start is a separate interaction from free and standalone starts; it selects the owned entitlement, uses its purchased exam version, and records immutable package source and purchase provenance for the session in a one-to-one provenance record. A standalone exam access grant does not become a package entitlement.

## Rationale and consequences

An explicit entitlement and separate start path prevent a session from silently switching among access sources. Provenance makes later reporting and audit traceable to the exact purchase and version. Persisted right rows support atomic exam-attempt consumption with session creation and a separate report-right lifecycle without changing purchase facts; the entitlement/right-type uniqueness constraint prevents duplicate rights of one type. Fulfillment idempotency and attempt consumption protect against duplicate rights or sessions across retries. The approved access and attempt invariants are owned by [Product business rules](../../product/business-rules.md); this record owns the boundary and provenance rationale.

The approved Stage 3 attempt design uses a transaction, database uniqueness, and state reload to converge a concurrent same-source start or return a source conflict. Its specification did not select PostgreSQL row locking and required separate evidence and review before adopting it. The current handler implements the transaction and unique-violation reload path; this record makes no claim that an EF concurrency token exists.
