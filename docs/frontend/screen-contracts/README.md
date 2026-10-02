# Frontend Screen Contracts

This directory is the permanent Frontend owner for screen identity, route mapping, approved presentation states, explicit blocked/deferred concepts, and shared screen patterns. [Screen contract schema](contract-schema.md) defines evidence and disposition rules; [shared patterns](shared-patterns.md) record reusable screen interaction patterns; [screen index](screen-index.md) maps screen identity to the canonical route source.

## Authority chain

1. [Product](../../product/requirements.md) owns required behavior and [Architecture](../../architecture/architecture-overview.md) owns system boundaries.
2. [Security](../../security/security-overview.md) and [OpenAPI](../../api/openapi.yaml) own protected enforcement and current HTTP contracts.
3. [Frontend routing](../routing-and-permissions.md) owns route identities, guard UX, and navigation source links.
4. Human-approved screen packets own approved screen presentation and states within those higher constraints.
5. `docs/frontend/design/stitch/DESIGN.md` and human-approved Stitch artifacts own visual intent within their exact approval scope.

A screen contract maps those sources. It cannot create Product behavior, API fields, permissions, route activation, visual approval, or implementation authorization. Current Angular code proves implementation only. Dated execution and visual-review status belong to Delivery or their review records, not a screen's permanent behavior contract.

## Family contracts

| Family | Contract |
|---|---|
| Authentication | [authentication.md](authentication.md) |
| Account | [account.md](account.md) |
| Nurse Profile | [nurse-profile.md](nurse-profile.md) |
| Exams | [exams.md](exams.md) |
| Preparation Packages | [preparation-packages.md](preparation-packages.md) |
| Commerce | [commerce.md](commerce.md) |
| Employer | [employer.md](employer.md) |
| Administration | [administration.md](administration.md) |
| Shared/System | [shared-system.md](shared-system.md) |

These family contracts are the permanent screen-contract owners. Approved screen-presentation detail remains in the linked human-approved packets where a family contract cites one; current implementation status belongs to [Delivery](../../delivery/current-state.md).
