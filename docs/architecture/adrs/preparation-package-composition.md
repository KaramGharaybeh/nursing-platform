# Preparation Package Composition and Offers

Status: Accepted.

## Context and authority

The approved Preparation Package architecture decisions and Stage 1 catalog specification distinguish stable catalog identity, published composition, and sellable commercial facts. A package purchase must keep its original composition meaningful when catalog authoring changes later.

## Decision

A Preparation Package definition supplies stable catalog identity. A published package version freezes references to the exact included exam version, compatible reporting-profile publication, ordered material versions, and practice collection version. A package offer is a separate commercial relationship to a published package version. Purchase/order snapshots preserve the chosen version and commercial facts for subsequent fulfillment.

## Rationale and consequences

Separating mutable authoring identity from frozen published composition prevents later catalog edits from silently changing what was purchased. Separating the offer keeps commercial presentation from redefining package content. Consumers and reports follow the purchased version and snapshots instead of live catalog pointers. Product rules for package composition and offer availability remain in [Product business rules](../../product/business-rules.md).
