# Release Plan

This document owns established release and milestone placement. It does not infer a release from implementation progress. [Current State](current-state.md) records repository implementation; [Roadmap](roadmap.md) records authorized future work. An implemented capability is not necessarily released, and an approved requirement is not necessarily scheduled.

## Established structure and limits

- The completed [Product requirements](../product/requirements.md) use **v1** for the current Preparation Package report and scope. That is an approved Product scope marker, not evidence of a dated deployment or a named release event. The first commercial Preparation Package boundary is also described in [Feature Catalog](../product/feature-catalog.md) and [Business Rules](../product/business-rules.md).
- The historical task list numbered development phases and a Preparation Package Stage 1→4 workstream. The [Frontend execution ledger](../frontend/execution/frontend-implementation-ledger.md) has M-FE-001..019 organizational milestones and Task/Gate dependencies. These are execution groupings, not independently verified production release identifiers.
- No authoritative calendar release dates, production launch date, whole-platform release membership, deployed release status, or person-specific release owner was established from the inspected current authority and repository state. Historical Product-vision statements about the “first production release,” preserved in Git, are not sufficient to assign current items to a release where later authority changed the scope or state.

## Release mapping

| Item or scope | Established scope/milestone | Release assignment and status |
|---|---|---|
| Preparation Package numeric analytical report without qualitative labels | Explicit current **v1 Product scope** in [Requirements](../product/requirements.md); current implementation evidence in [Current State](current-state.md). | No deployed v1 release or launch date established. `RELEASE_ASSIGNMENT_NOT_ESTABLISHED` for a shipping event. |
| Materials Access, practice, package exam attempt, and report | Approved four-benefit Preparation Package scope in [Product](../product/requirements.md). Backend implementation and learner-material gap are separated in [Current State](current-state.md). | `RELEASE_ASSIGNMENT_NOT_ESTABLISHED`; Product approval does not certify release readiness. |
| Preparation Package promotion administration | Approved Product capability, with mechanics unspecified and no inspected runtime implementation. | `RELEASE_ASSIGNMENT_NOT_ESTABLISHED`. |
| Account recovery | Approved Product capability with inspected API/UI implementation. | `RELEASE_ASSIGNMENT_NOT_ESTABLISHED`; implementation is not proof of a production release. |
| Production payment-provider integration | Explicitly deferred provider-neutral boundary in [Architecture Integrations](../architecture/integrations.md) and [Roadmap](roadmap.md). | `RELEASE_ASSIGNMENT_NOT_ESTABLISHED`; provider and production completion path are unselected. |
| Remaining Frontend Task/Gate and production-readiness categories | M-FE execution milestones and the migrated production-readiness category in [Roadmap](roadmap.md). | Execution grouping exists; `RELEASE_ASSIGNMENT_NOT_ESTABLISHED` for deployment or release membership. |
| Learner material reader, CI coverage gaps, and external monitoring | Current gaps in [Current State](current-state.md), with only bounded planning authority noted in [Roadmap](roadmap.md). | `RELEASE_ASSIGNMENT_NOT_ESTABLISHED`; a gap is not an automatic next-release commitment. |

## First commercial package and production-release decisions

[Product business rules](../product/business-rules.md) own per-offer price, currency, access duration, and published-package-version meaning. No chosen first-offer values or actual first commercial publication were established here. Historical task-list launch-readiness entries for qualitative weak/neutral/strong thresholds conflict with the later explicit human decision that current v1 reporting has **no qualitative performance bands**; those historical entries do not govern current v1 release readiness.

The active design-proposed-feature register requires disposition of its open design proposals before a first production release (`docs/frontend/design/stitch/design-proposed-features.md`). This is a decision gate for proposals, not approval of notifications or help/support as Product features and not proof that a release has been scheduled.

No item is called release-`BLOCKED` solely because its assignment is unknown. A specific approved release plan and unresolved prerequisite would be needed to make that claim. Dates, estimates, priorities, and release membership remain unassigned rather than invented.
