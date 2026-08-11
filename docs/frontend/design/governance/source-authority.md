# Source Authority and Live Evidence

```yaml
document_id: NPS-DES-GOV-SOURCE-001
version: 1.3
status: g0-accepted-governance-only
recorded_at: 2026-08-12
timezone: Asia/Amman
backend_evidence_commit: 8439511
design_governance_baseline: 1f17718
repository_branch: feature/frontend-design-evidence-foundation
penpot_file_id: 01813f71-6684-8025-8008-5d0437a49666
```

## Purpose

This document records the source hierarchy, live repository snapshot, and live Penpot inventory used by Phase 0. It is an evidence register, not visual approval, implementation authorization, or a page specification.

## Authority hierarchy

Use sources in this order:

1. Explicit current decisions made by Karam.
2. Approved current product and architecture decisions in the live repository.
3. Approved business requirements.
4. Verified live backend behavior at a recorded Git commit.
5. Approved shared frontend, design, and test contracts.
6. Approved page-family patterns and page specifications.
7. Penpot for approved visual geometry, composition, styling, and hierarchy.
8. Agent inference is never authoritative.

Penpot is the sole visual authority. Figma is non-authoritative unless Karam explicitly records a future decision changing that rule. Penpot cannot override repository security, accessibility, privacy, backend-contract, or architecture rules. A live Penpot artifact is evidence until it passes the applicable manager gate and Karam explicitly grants visual approval.

## Repository snapshot

| Item | Recorded value | Interpretation |
|---|---|---|
| Repository | `/home/karam/development/nursing-platform` | Live Nursing Platform repository |
| Branch | `feature/frontend-design-evidence-foundation` | Accepted governance/evidence-foundation branch |
| Backend evidence commit | `8439511` | Preparation Package Stage 1–4 backend handoff evidence baseline |
| Frontend governance conflict-resolution baseline | `1f17718` | Accepted G0 governance/re-entry baseline; not approval for Angular or Penpot writes |
| Remote relation | Not evaluated in this documentation-only re-entry task | Not required for frontend contract evidence |
| Frontend runtime | `frontend/` is uninitialized | Angular 22 is approved architecture, not implemented evidence |
| Active milestone | Preparation Package Stage 1 runtime complete; Stage 2–4 package runtime handoff remains implemented | Does not independently authorize frontend implementation |
| Roadmap | Administration and Frontend phases remain unstarted | Documentation-only Phase 0 is an explicit bounded governance exception, not roadmap completion |

The working tree already contained the following changes before the approved Phase 0 writes:

```text
 M docs/frontend/frontend-architecture.md
?? docs/design/screens/
?? docs/frontend/design-system-audit.md
?? docs/frontend/design/
```

These changes are uncommitted evidence. Phase 0 does not approve them automatically. The existing `docs/frontend/frontend-architecture.md` change reconciles prior historical Figma wording to Penpot-only authority and records the 44px actual-target minimum with a 48px preferred mobile/touch default; it remains an unstaged working-tree change pending normal review.

## Local runtime evidence

On 2026-08-12, the local Penpot frontend, backend, exporter, and MCP containers were observed running; the frontend is published at `http://localhost:9001`. This runtime availability is evidence only: it does not itself approve a Penpot artifact, authorize a write, or authorize Angular implementation. The observed library gaps (zero local components/colors, incomplete token coverage, and no token theme) remain unresolved evidence.

## G0 acceptance scope

Karam accepted G0 on 2026-08-12 as the governance/re-entry baseline for backend evidence `8439511` and frontend governance conflict resolution `1f17718` on `feature/frontend-design-evidence-foundation`. This acceptance authorizes only a separately scoped Phase 1 evidence-packet task. It does not authorize Angular implementation, Penpot writes, page specifications, a final route registry, backend changes, or visual approval.

## Preparation Package Backend Handoff

At `8439511`, future Phase 1 evidence work may inspect implemented contracts for admin reporting topics, reporting-profile publication, study materials, practice collections, package definition/version/composition, offers, nurse entitlement reads, practice progress, package exam-session start, and analytical-report reads. This is evidence availability only; it does not approve page specifications, Penpot work, Angular implementation, or a workspace design.

Storage provider, material upload/download/delivery, offline access, workspace/dashboard aggregation, adaptive practice, spaced repetition/retraining, and employer package progress/report visibility remain deferred and must not be represented as implemented behavior.

## Program ownership

| Concern | Owner | Status |
|---|---|---|
| Desktop design-documentation program | `docs/frontend/design/GOAL_STATE.md` | Authoritative program state |
| Program plan and gates | `docs/frontend/design/MASTER_PLAN.md` | Authoritative program plan |
| Legacy AUTH-001 durable state | `.agent/goal-state.md` | Untouched legacy evidence; not the owner of this program |
| Legacy AUTH-001 tracker | `docs/design/screens/authentication/auth-001-sign-in-tracker.md` | Untouched draft evidence |
| Page 09 review records | `docs/design/reviews/page-09-user-flows-audit.md` and `page-09-user-flows-fix-tracker.md` | Untouched legacy evidence with an open classification discrepancy |

## Penpot file inventory

The connected file is `Frontend Design Foundation`, ID `01813f71-6684-8025-8008-5d0437a49666`. Eleven actual pages exist, numbered continuously from `00` through `10`.

| No. | Page | Page ID | Principal live evidence | Phase 0 classification |
|---:|---|---|---|---|
| 00 | Cover | `7cd71457-8d32-8044-8008-55452790a701` | Cover board `70be19b8-cdda-80a2-8008-55545d07cc8a` | Designed foundation evidence; not newly approved |
| 01 | Getting Started | `70be19b8-cdda-80a2-8008-5553c1540c10` | Board `be0d98f3-029b-8045-8008-5a9eedd48c2c` | Populated but visually sparse; its Utilities-complete claim needs reconciliation |
| 02 | Colors | `70be19b8-cdda-80a2-8008-5553c1554a5a` | Board `c20c2ac0-03be-80c6-8008-5574598865ba` | Substantial foundation evidence; no local color assets |
| 03 | Typography | `70be19b8-cdda-80a2-8008-5553c155c42c` | Board `da460f46-d23b-804d-8008-5581182e457a` | Substantial foundation evidence; contains a stray root-level note |
| 04 | Spacing & Shape | `70be19b8-cdda-80a2-8008-5553c15666fe` | Board `19cc1e29-dc3c-802f-8008-5615aa528a68` | Substantial foundation evidence; one section alignment differs |
| 05 | Elevation & States | `70be19b8-cdda-80a2-8008-5553c156e3d0` | Board `38649993-a0ca-8049-8008-569753f5c2ef` | Substantial foundation evidence |
| 06 | Components | `70be19b8-cdda-80a2-8008-5553c1575666` | Board `1ea29b51-2ef7-80df-8008-58bce35091ae` | Extensive reference evidence; no local component assets |
| 07 | Utilities | `70be19b8-cdda-80a2-8008-5553c157daa3` | Three root boards overlap at `(0,0)` | Conflicted; legacy boards require classification |
| 08 | Product Map | `151d48c6-c8bb-8080-8008-5d154a789dcc` | Board `151d48c6-c8bb-8080-8008-5d170a42e8dc` | Populated planning inventory, not production screen design |
| 09 | User Flows | `5b727796-97b4-8084-8008-5d7f57f4a1bd` | Board `5b727796-97b4-8084-8008-5d7f58eeb357` | Legacy/draft evidence; classification remains open |
| 10 | Authentication Core | `9bd8100d-da35-8029-8008-5d963d808253` | Documentation and viewport boards | Incomplete relative to legacy package claims; Desktop evidence exists, while legacy responsive/RTL/state artifacts are defective or empty |

### Page 07 conflict

The following visible root boards share `(0,0)`:

| Board | ID | Evidence result |
|---|---|---|
| User Flows — Nursing Platform v1 | `151d48c6-c8bb-8080-8008-5d154b9ee63d` | Legacy board; sequential export rendered blank |
| Product Map — Nursing Platform v1 | `151d48c6-c8bb-8080-8008-5d154a81d104` | Legacy board; sequential export rendered almost blank |
| Utilities — Foundation v1 | `be0d98f3-029b-8045-8008-5aa0f62dcf9c` | Sparse text-only utility documentation; visually overlays the other boards |

No Page 07 artifact may be classified, relocated, deleted, or approved during Phase 0.

### Page 10 evidence

| Artifact | ID | Evidence result |
|---|---|---|
| Authentication Core — Batch A v1 | `9bd8100d-da35-8029-8008-5d9657a5d9c8` | 1920x3200; ten empty direct-child sections; blank export; later sections extend beyond clipped bounds |
| Desktop Default | `9bd8100d-da35-8029-8008-5de7f8b991a8` | Populated 1440x1024 sign-in draft; evidence only, not visually approved |
| Tablet Default | `9bd8100d-da35-8029-8008-5de7f8c0acf2` | Form renders; Brand Panel is outside parent bounds and clipped |
| Mobile Default | `5cca84cb-c69b-809e-8008-5e55e09742c8` | Form renders; Brand Panel is outside parent bounds and clipped; legacy tracker ID is stale |
| Desktop RTL | `9bd8100d-da35-8029-8008-5de7f8c9520e` | Empty board; blank export |
| Mobile RTL | `9bd8100d-da35-8029-8008-5de7f8ce22d0` | Empty board; blank export |

Two orphan root-level `Brand Abbreviation` text objects remain at `(0,0)`: `15ce6098-9d6b-800e-8008-5dfbb8d1e13b` and `15ce6098-9d6b-800e-8008-5dfc031263c9`.

### Local Penpot library

| Asset type | Count | Evidence implication |
|---|---:|---|
| Components | 0 | Component documentation is not backed by reusable local component assets |
| Colors | 0 | Color documentation is not backed by reusable local color assets |
| Typographies | 34 | Assets exist but use repeated generic names such as `Large`, `Medium`, and `Small` |
| Token sets | 2 | Spacing/shape and elevation/state tokens exist; no color or typography token set was reported |
| Token themes | 0 | No local token theme is established |

## Evidence boundaries

- Structural reads and exports prove only the observed live state at audit time.
- Geometry checks do not constitute visual approval.
- Export success does not prove accessibility, backend alignment, or production readiness.
- Completion markers in trackers do not override contradictory live evidence.
- Page 09's fresh full-board export rendered, but its historical audit, completion tracker, and current classification remain conflicting evidence until explicitly reconciled.
- No Penpot object was modified during Phase 0.
