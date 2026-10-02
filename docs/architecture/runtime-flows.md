# Runtime Flows

These flows describe approved boundary crossings. [Product requirements](../product/requirements.md) and [business rules](../product/business-rules.md) own the behavior and invariants; endpoint and handler details belong to later API/backend documentation.

## Identity and protected requests

The client sends account/authentication requests to the API boundary. Server-side identity services establish authenticated identity. For subsequent protected requests, the server applies authorization and ownership checks before returning protected data or changing state. The client may adjust presentation from the result but is not the final authorization authority. [System Context](system-context.md) owns the trust edge.

## Commercial order and fulfillment

```text
Client selection → server-owned commercial facts → order-item snapshot
→ checkout/provider boundary → authorized order completion
→ item-based fulfillment → access grant or package entitlement
```

The provider boundary is separate from order and entitlement decisions. Checkout initialization and fulfillment are distinct idempotency concerns. Standalone exam purchases produce standalone access grants; Preparation Package purchases produce a package entitlement and independently authorizable rights. An order-item snapshot preserves the commercial and package-version facts used by fulfillment; live catalog changes do not rewrite the purchased facts. A Development/Test sandbox is a substitute behind the payment boundary, not production payment authority. [Integrations](integrations.md) owns the provider separation; [Data Model](data-model.md) owns conceptual relationships.

## Preparation Package access and exam attempt

An owned package entitlement is the access root for its purchased benefits. Material/practice access is evaluated against the entitlement and relevant right. The package examination start is a separate server operation from free or standalone exam start. It uses the explicitly selected entitlement and its purchased exam version, records immutable session source and package provenance, and consumes the package attempt right with successful session creation. A standalone exam grant cannot substitute for a package attempt right. The architectural reason for separate start paths is recorded in [Package entitlement and provenance](adrs/preparation-package-entitlement-provenance.md).

## Examination and package report

The examination runtime holds a server-owned session timer and finalized scoring. A qualifying package session retains provenance to the purchased package. The current approved v1 report path uses the finalized package session and its compatible reporting profile, not practice-progress evidence. The first valid direct report request can create an immutable report snapshot; later requests retrieve that report. Generation failure does not undo exam finalization. Purchased material/practice version references supply deterministic guidance. The report's current approved output is numeric; [Product requirements](../product/requirements.md) owns its required behavior. See [Package report snapshot](adrs/preparation-package-report-snapshot.md).

No separate report worker or queue is prescribed by the approved v1 report decision. Later background processing or external callback flows require their own approved architecture before being stated here as a target flow.
