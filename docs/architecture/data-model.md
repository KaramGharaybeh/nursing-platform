# Architectural Data Model

This is a conceptual ownership map. It does not define tables, indexes, EF mappings, DTOs, or migrations.

## Shared boundaries

- **Identity and authorization** hold account identity and access-control relationships. Nurse and employer profile data are distinct business information associated with users.
- **Examinations** own exam definitions/versions, question content, sessions, scoring evidence, and standalone access grants.
- **Payments** own commercial product/offer selection, orders, item snapshots, and checkout correlation. An order is not itself a package entitlement.
- **Preparation Packages** own stable package identity, frozen composition, the sellable offer relationship, purchased entitlement/rights, independent practice content, reporting taxonomy/profile, and package analytical reports.
- **Recruitment** owns candidate discovery and contact-request relationships; it does not acquire package report or payment ownership.

## Preparation Package relationships

```text
Package definition ── has ──> immutable package version
                              ├── exact published exam version
                              ├── compatible reporting-profile publication
                              ├── ordered published material versions
                              └── one published practice collection version
Package offer ── selects ────> published package version
Order item ── preserves ─────> purchased-offer snapshot
Fulfillment ── creates ──────> nurse-owned package entitlement ── owns benefit rights
Package exam session ────────> purchased-entitlement provenance
Qualifying session ──────────> one immutable analytical report
```

The reporting-profile publication is separate from the published exam version so package topic assignments do not mutate exam content. A practice collection's items are independent of exam questions; topic mappings connect them to report guidance without making practice answers part of exam scoring. Materials and practice collections are reusable published versions; a package version refers to exact versions, not mutable live content.

The purchased-offer snapshot preserves the composition and commercial facts used when the order was placed. Fulfillment yields an entitlement whose rights govern later package access without re-reading live payment state. The package session's source/provenance preserves which purchase supplied its attempt. The report preserves one finalized session and its topic/guidance results as a point-in-time record. These separations protect historical purchases and reports when catalog content changes. [Runtime Flows](runtime-flows.md) describes their interaction; the accepted [composition](adrs/preparation-package-composition.md), [entitlement](adrs/preparation-package-entitlement-provenance.md), and [report](adrs/preparation-package-report-snapshot.md) decisions explain the rationale.
