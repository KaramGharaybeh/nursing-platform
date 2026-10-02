# Roadmap

This document owns approved future-work status, not Product scope or a list of every implementation gap. [Current State](current-state.md) records what the inspected repository implements; [Release Plan](release-plan.md) owns release assignment. A requirement can be approved while its implementation plan, priority, and release remain unestablished. This document authorizes no implementation by itself.

## Authority used

The approved work recorded here was migrated from the historical task list and bounded execution records, then checked against current Product, Architecture, technical owners, source, and configuration. Those retired sources remain recoverable in Git history; their old checkboxes and “current” wording are not active authority. The [Frontend implementation ledger](../frontend/execution/frontend-implementation-ledger.md) retains its bounded Task/Gate eligibility role for `GOAL-FE-001`; its milestones are organizational groupings, not releases. The completed [Product requirements](../product/requirements.md) establish scope, not a delivery commitment.

## Approved planned or explicitly deferred work

| Work boundary | Classification and established dependency | Priority, date, release |
|---|---|---|
| Remaining eligible work under `GOAL-FE-001` | `APPROVED_PLANNED_WORK` as bounded by the Frontend ledger's Task/Gate DAG, standing authorization criteria, per-screen approval, and blocker rules. A `VERIFIED` classification gate does not prove that its screen is implemented. The ledger's M-FE-001..019 rows organize this work; they are not a release plan. | No global priority, date, or release assignment established here. Eligibility must be read at the specific Task/Gate. |
| Production payment-provider integration | `DEFERRED_APPROVED_SCOPE`. [Architecture Integrations](../architecture/integrations.md) keeps the provider boundary neutral; `CURRENT_TASK.md` records intentional deferral pending company country, bank-account jurisdiction, and provider selection. No vendor or callback design is selected. | No provider decision, schedule, or release assignment established. |
| Production-readiness work | `APPROVED_PLANNED_WORK` at category level, with bounded Frontend Task/Gate ownership in M-FE-018/019. The category covers quality/CI, operational hardening, and release checks without selecting a particular implementation solution or claiming a live deployment. | No priority, dates, provider, or release assignment established. Individual tasks retain their own gates. |
| Administration work not covered by current implementation | `APPROVED_PLANNED_WORK` at a broad category boundary, with specific Frontend task/screen states owned by M-FE-016 and its Task/Gate rows. Existing admin user/exam/package operations must not be relabeled unimplemented from historical checklist state. | No common schedule or release assignment established. |
| Preparation Package workspace runtime | `DEFERRED_APPROVED_SCOPE`: [Product requirements](../product/requirements.md) approve a purchaser workspace concept and `CURRENT_TASK.md` records workspace runtime as deferred relative to its earlier backend baseline. Screen/technical implementation details require their owning contracts. | No implementation date or release assignment established. |
| Public-registration abuse limiting and verification-email delivery resilience | `DEFERRED_APPROVED_SCOPE`, migrated from the historical `C-AUTH-PUBLIC-HARDENING-DEBT` decision. This entry does not choose limiter, queue, or outbox infrastructure; original execution provenance is recoverable in Git history. | No schedule, owner, or release assignment established by this roadmap. |

“Deferred” here records an explicit scope or decision boundary. It does not guarantee implementation or placement in a later release. `BLOCKED` is not assigned to an entire broad work category merely because a prerequisite is unknown; a specific Task/Gate must establish that its approved work cannot continue.

## Bounded blocker inventory

The [current Administration screen contract](../frontend/screen-contracts/administration.md) keeps `ADM-004`, `ADM-009`, and `ADM-010` blocked and forbids treating them as active destinations. The [Frontend execution ledger](../frontend/execution/frontend-implementation-ledger.md) maps classification Tasks `T-FE-105` and `T-FE-112` to those screen rows. The **screen rows**, not necessarily the classification Tasks, have approval decision `BLOCKED`, implementation status `NOT STARTED`, blocker type `BACKEND`, and no stable backend contract for full roles/permissions, admin payment-order, or admin recruitment management. These are bounded Frontend screen implementation blockers, not evidence that a broader Product feature or an entire milestone is blocked. Their implementation or release placement is not authorized by this roadmap. Other ledger `BLOCKED` labels require their specific Task/Gate evidence and current contract before reuse as current status.

## Verified gaps without an approved detailed work plan

The [current-state gap](current-state.md) for learner material delivery/reader has approved Product intent, but the [screen contract](../frontend/screen-contracts/preparation-packages.md) states that its learner interaction lacks technical authority. Preparation Package promotion administration is approved [Product scope](../product/requirements.md), while its mechanics remain unspecified and no current runtime implementation was found. The present CI workflow omits frontend/E2E/accessibility checks; local testing mechanisms exist, and the Frontend ledger has a broader CI pipeline task, but that does not assign each missing check a release or approve a specific CI design. Staging/Production procedures and external observability are not established; absence does not select a provider or create a scheduled task. These items remain `IMPLEMENTATION_GAP_WITHOUT_APPROVED_PLAN` where no exact approved work package was found, or bounded planned categories above where one exists.

## Historical and superseded planning boundaries

- The historical Product vision in Git says Preparation Packages are only planned and no capability implemented. [Current State](current-state.md) and inspected code show that status is stale; it is not a delivery status source.
- The retired Penpot master plan and goal-state records are preserved in Git history. Their dated phase, next, and resume instructions are historical and are not this Documentation Rearchitecture phase or a current software release.
- `PROGRESS_HISTORY.md` preserves past “next” messages; they do not supersede the current Task/Gate owner or this roadmap.
- The historical task list's launch-readiness text names weak/neutral/strong report classifications and thresholds. The later explicit human Product decision and [current v1 requirement](../product/requirements.md) exclude qualitative bands from required v1 output. Those historical threshold entries are not current roadmap or release blockers.

No source inspected establishes a platform-wide priority order beyond specific Task/Gate dependencies and the explicit Preparation Package Stage 1→4 historical workstream order. No date or person assignment is supplied by this document.
