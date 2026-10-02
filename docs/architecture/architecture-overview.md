# Architecture Overview

## Scope and reading path

This layer records approved system structure and architectural rationale. [Product requirements](../product/requirements.md), [business rules](../product/business-rules.md), and [roles and permissions](../product/roles-and-permissions.md) own product intent. The documents below own architectural boundaries; they do not report delivery status or prescribe endpoint, component, or persistence implementation.

1. [System context](system-context.md) — system and trust boundaries.
2. [Containers](containers.md) — major client, server, and data responsibilities.
3. [Runtime flows](runtime-flows.md) — interactions across those boundaries.
4. [Data model](data-model.md) — conceptual ownership and durable relationships.
5. [Integrations](integrations.md) and [deployment architecture](deployment-architecture.md) — external and hosting boundaries.
6. [Quality attributes](quality-attributes.md) and [risks and technical debt](risks-and-technical-debt.md) — approved design responses and evidenced limits.

## Architectural style

The system uses an Angular single-page client and a REST-facing ASP.NET Core Web API. The backend follows Clean Architecture: Presentation invokes Application use cases; Application depends on Domain; Infrastructure implements Application-defined external interfaces. Domain does not depend on Infrastructure or Presentation. Presentation remains thin, and business logic belongs in Application and Domain. Commands and queries organize backend use cases. These are architectural constraints, not a claim that every current file complies.

The system is organized by business boundaries, including identity, nurse and employer information, examinations, recruitment, payments, and Preparation Packages. Modules may share the approved application and persistence architecture; the documentation does not assert independently deployed services for each module. The package domain is additive: it connects to payment and examination boundaries while preserving distinct standalone examination access.

## Governing principles

- Server-confirmed state owns protected authentication, authorization, payment, entitlement, and official result facts. The client presents these facts but cannot establish them.
- PostgreSQL is the primary persistent store. Redis is a cache, not a source of business truth.
- External provider interactions are separated from business use cases through Application-owned contracts and Infrastructure implementations. A production payment provider is not selected by this architecture.
- Published Preparation Package composition and purchased-offer facts are preserved as specific versions/snapshots. Package attempts retain their purchase provenance; package reports retain their qualifying session evidence.

The detailed boundaries and rationale are owned by the linked documents and accepted ADRs below. Repository-wide coding and documentation conventions remain with `docs/standards/engineering-standards.md`.

## ADR inventory

| Decision | Status | Record |
|---|---|---|
| Keep backend dependencies directed inward through Clean Architecture | Accepted | [Clean Architecture boundaries](adrs/clean-architecture-boundaries.md) |
| Separate Preparation Package identity, immutable composition, and sellable offer | Accepted | [Package composition and offers](adrs/preparation-package-composition.md) |
| Keep package entitlement and attempt provenance distinct from standalone exam grants | Accepted | [Package entitlement and provenance](adrs/preparation-package-entitlement-provenance.md) |
| Persist one immutable report snapshot from a qualifying package session | Accepted for current v1 numeric report scope | [Package report snapshot](adrs/preparation-package-report-snapshot.md) |

The payment provider-neutral boundary is described in [Integrations](integrations.md). No ADR here selects a real provider or converts a staged implementation plan into permanent architectural authority.
