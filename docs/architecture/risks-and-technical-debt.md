# Architectural Risks and Technical Debt

This document records only evidence-supported architecture limits. It does not assign roadmap priority, owners to people, release dates, or remediation schedules.

| Evidenced limit | Architectural consequence | Owning boundary |
|---|---|---|
| A real production payment provider has not been selected in the approved provider-neutral checkout design. | The provider interface is stable as an architectural boundary, but no vendor-specific adapter or production callback topology can be treated as approved architecture. | [Integrations](integrations.md); [Delivery Current State](../delivery/current-state.md) records implementation status. |
| The deployment guide names production components but does not select a production orchestration platform. | Container placement and cloud-specific topology must not be inferred from the conceptual diagram. | [Deployment Architecture](deployment-architecture.md); [Operations](../operations/environments.md) owns procedure and environment detail. |

These are explicit decision limits, not findings that the existing implementation violates architecture. The inspected sources did not establish an additional accepted architectural technical-debt item suitable for permanent migration. New debt should be recorded here only with concrete evidence of the decision or deviation and its architectural consequence.
