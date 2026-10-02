# Documentation Index

This index routes questions to their authoritative owner. It does not define Product behavior, technical contracts, implementation status, or project governance.

## Start here

For a new contribution, follow this reading path:

1. [AGENTS.md](../AGENTS.md) — AI-agent operating rules and task workflow.
2. [PROJECT_RULES.md](../PROJECT_RULES.md) — repository governance and hard constraints.
3. This index — choose the owner for the task.
4. [Current State](delivery/current-state.md) — read when the task depends on what the repository implements now.
5. Read only the relevant owner documents below, then inspect source, tests, and configuration as needed to verify the task.

Do not read every document by default. An active task's explicit human instructions remain essential context; this index does not authorize implementation.

## Find the authoritative owner

| Question or task | Start with | Specialist owners |
|---|---|---|
| What is the approved Product purpose or feature scope? | [Product Overview](product/product-overview.md) | [Feature Catalog](product/feature-catalog.md), [Glossary](product/glossary.md) |
| What must the product do, and under which business or actor rules? | [Requirements](product/requirements.md) | [Business Rules](product/business-rules.md), [Roles and Permissions](product/roles-and-permissions.md) |
| How is the approved system structured, and why? | [Architecture Overview](architecture/architecture-overview.md) | [System Context](architecture/system-context.md), [Containers](architecture/containers.md), [Runtime Flows](architecture/runtime-flows.md), [Data Model](architecture/data-model.md), [Integrations](architecture/integrations.md), [Deployment Architecture](architecture/deployment-architecture.md), [Quality Attributes](architecture/quality-attributes.md), [Risks and Technical Debt](architecture/risks-and-technical-debt.md), and the overview's ADR inventory. For Preparation Package composition rationale, use the [composition ADR](architecture/adrs/preparation-package-composition.md). |
| How does the Angular client work? | [Frontend Architecture](frontend/frontend-architecture.md) | [Routing and Permissions](frontend/routing-and-permissions.md), [Design System](frontend/design-system.md), [Accessibility](frontend/accessibility.md), [RTL and Localization](frontend/rtl-localization.md) |
| Which current screen contract applies? | [Screen Index](frontend/screen-contracts/screen-index.md) | [Screen Contracts](frontend/screen-contracts/README.md); for package reporting, the [Preparation Package family](frontend/screen-contracts/preparation-packages.md) |
| How is the backend implemented? | [Backend Architecture](backend/backend-architecture.md) | [Domain Model](backend/domain-model.md), [Storage and Database](backend/storage-and-database.md), [Background Workers](backend/background-workers.md) |
| What is the HTTP operation or schema? | [OpenAPI](api/openapi.yaml) | [API Guidelines and Errors](api/api-guidelines-and-errors.md) for conventions and error behavior |
| How are protected access and data handled? | [Security Overview](security/security-overview.md) | [Authentication and Authorization](security/authentication-authorization.md), [Threat Model](security/threat-model.md), [Data Protection](security/data-protection.md), [Security Verification](security/security-verification.md). Frontend guards are UX controls; protected server enforcement belongs to Security and server/API contracts. |
| How is required behavior or a contract verified? | [Testing Strategy](testing/testing-strategy.md) | [Acceptance Criteria](testing/acceptance-criteria.md), [End-to-End Scenarios](testing/end-to-end-scenarios.md), [Test Data Catalog](testing/test-data-catalog.md), [Verification Dependency Graph](testing/dependency-graph.md). Testing does not define the behavior it verifies. |
| How are environments and the system operated? | [Environments](operations/environments.md) | [Deployment Runbook](operations/deployment-runbook.md) for verified procedures, [Monitoring and Alerts](operations/monitoring-and-alerts.md) for established signals. These documents do not establish a runnable Staging or Production procedure where one is unverified. |
| What exists or remains incomplete now? | [Current State](delivery/current-state.md) | Follow its links to the affected domain owner for detail. |
| What approved work remains, or what is blocked? | [Roadmap](delivery/roadmap.md) | Read the relevant task's authority only when executing that task. A current gap is not automatically planned work. |
| Which release or milestone contains the work? | [Release Plan](delivery/release-plan.md) | `RELEASE_ASSIGNMENT_NOT_ESTABLISHED` is the recorded answer where authority establishes no assignment. |
| What conventions apply across technical domains? | [Engineering Standards](standards/engineering-standards.md) | Domain-specific rules stay with their Frontend, Backend, API, Security, Testing, or other specialist owner. |

## Keep authority and status separate

[Product](product/product-overview.md) owns what the system must do; [Architecture](architecture/architecture-overview.md) owns approved structure and rationale. [Frontend](frontend/frontend-architecture.md), [Backend](backend/backend-architecture.md), [API](api/openapi.yaml), and [Security](security/security-overview.md) own technical contracts and enforcement. [Testing](testing/testing-strategy.md) owns verification, and [Operations](operations/environments.md) owns operating knowledge. [Delivery](delivery/current-state.md) owns current implementation status; its [Roadmap](delivery/roadmap.md) owns approved remaining work and its [Release Plan](delivery/release-plan.md) owns release placement. [Engineering Standards](standards/engineering-standards.md) owns shared engineering conventions. [PROJECT_RULES.md](../PROJECT_RULES.md) owns repository governance; [AGENTS.md](../AGENTS.md) owns AI-agent behavior.

Update the document that owns a fact and link to it from other domains. Keep approved target behavior separate from current implementation status. Existing code establishes implementation evidence, not Product intent; historical text is evidence, not automatically current authority. Preserve verified architectural rationale with its Architecture owner or ADR. Update Delivery when implementation or status changes materially. If two sources of equal authority conflict without an established rule, seek the human owner's decision rather than selecting one here.

## Historical and bounded governance material

Historical plans, reports, retired design programs, and deleted duplicate documentation remain recoverable in Git history. They are evidence, not normal active authority. The bounded execution and handoff files that remain are read only when an explicit task or repository rule requires them; their dated “current,” “next,” or “blocked” statements do not override the owners above. Normal reading starts with this router and the active owner.
