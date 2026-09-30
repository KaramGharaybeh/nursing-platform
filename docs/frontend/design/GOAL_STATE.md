# Nursing Platform — Penpot Desktop Goal State

```yaml
document_id: NPS-DES-STATE-001
version: 1.4
updated_at: 2026-08-12
timezone: Asia/Amman
manager_model: openai/gpt-6-sol
design_authority: approved visual foundations plus approved Stitch redesign artifacts; legacy Penpot/design artifacts only when explicitly re-approved
viewport_scope: Desktop browser
current_phase: PHASE-1-EVIDENCE-PACKET-AWAITING-AUTHORIZATION
current_batch: none
overall_status: g0-accepted-governance-only-phase-1-not-authorized
```

> The dated model allocation, phase status, next action, and resume instructions below are historical design-program state, not current execution authority. Current execution authority is in `PROJECT_RULES.md`, `AGENTS.md`, and the owning ledgers. Approved product/design decisions and human visual gates remain in force.

## Goal

Create evidence-backed Markdown specifications and, for the active system-wide redesign, human-approved Stitch designs for canonical Nursing Platform browser pages, with complete business, functional, non-functional, security, accessibility, API, permission, validation, functional-test, and visual-test traceability. Historical Penpot Desktop-program records remain preserved as evidence.

## Explicit exclusions

- No Tablet design in the current scope.
- No Mobile design in the current scope.
- No RTL or Arabic boards in the current scope; readiness remains required.
- No Angular implementation in the current scope.
- No automated test implementation in the current scope.
- Storybook is approved as the intended future production visual development/review surface after separate tooling authorization, but it is not a requirements authority and is not installed by this governance checkpoint.
- No alternate requirements authority is created by Storybook.
- No use of `claude-fable-5.md` as project evidence.

## Locked decisions

| ID | Decision | Status |
|---|---|---|
| DEC-001 | Penpot was the sole visual authority for the original Desktop design-documentation program; after the 2026-09-20 human decision, approved visual foundations constrain visual intent, approved Stitch redesign artifacts own new visual composition after human approval, and Storybook is the intended production visual development/review surface after separate tooling authorization | Superseded for current implementation governance; preserved as historical design-program baseline unless explicitly re-approved |
| DEC-002 | Desktop browser only for this design pass | Locked |
| DEC-003 | One Markdown file per canonical route-level page | Locked |
| DEC-004 | Page states stay inside the owning page spec unless independently routed | Locked |
| DEC-005 | Shared rules are centralized and referenced by stable IDs | Locked |
| DEC-006 | Angular 22 + Angular Material/CDK + SCSS + custom Material theme | Locked and present in the live frontend architecture working copy |
| DEC-007 | WCAG 2.2 AA | Locked |
| DEC-008 | Future Arabic and RTL readiness | Locked |
| DEC-009 | At the time this design decision was recorded, repository orchestration used `openai/gpt-6-sol` as sole orchestrator/final gate. A later three-role runtime superseded that arrangement and was itself retired during OpenCode independence cleanup; product/design approval authority remains unchanged. | Historical / superseded |
| DEC-010 | Karam/technical-lead approval remains required for final visual approval gates | Locked |
| DEC-011 | Storybook may provide future production component/screen visual evidence after separate tooling authorization but never self-approves screens or creates requirements | Locked |
| DEC-012 | The 2026-09-20 system-wide redesign uses Google Stitch as the active visual-design workspace. Human-approved Stitch screens own new visual composition; legacy Penpot/design artifacts remain reference evidence unless explicitly re-approved. | Locked |

## Phase 0 evidence reviewed

| Source | Coverage | Result |
|---|---|---|
| Live repository at `8439511` (`docs: mark preparation package stage 1 runtime complete`) | Required architecture, roadmap, frontend, design, legacy evidence, and completed Preparation Package runtime handoff | Reconciled |
| Live Git status and recent history | Branch, commit, remote relation, staged state, and pre-existing working-tree changes | Recorded in `governance/source-authority.md` |
| Live Penpot file `01813f71-6684-8025-8008-5d0437a49666` | Page list, root structures, representative exports, Page 07 conflict, Page 10 viewport/state evidence, local library | Read-only inventory complete |
| Backend endpoint, validator, error, permission, and test surfaces | Phase 0 freshness audit | Open contract questions assigned to Phase 1 |
| `claude-fable-5.md` | Explicitly excluded by decision | Not read or used |

## Current findings

1. The live repository is the evidence source; the earlier uploaded documentation dump is historical and insufficient for page specifications.
2. Current implementation governance uses separated authority: approved visual foundations constrain visual intent; approved Stitch redesign artifacts own new visual composition after human approval; legacy Penpot/design artifacts are reference evidence unless explicitly re-approved; Storybook is the intended future production visual development/review surface after separate tooling authorization; Figma is non-authoritative unless Karam records a future decision changing that rule.
3. Angular 22, Angular Material/CDK where required, Signals, RxJS, SCSS, and a project-owned Material theme are approved architecture; the frontend workspace is not initialized.
4. Karam accepted G0 on 2026-08-12 as a governance/re-entry baseline only. The current milestone and roadmap still do not authorize frontend implementation, Penpot writes, page specifications, or a route registry.
5. Eleven live Penpot pages exist from `00` through `10`. Their presence and exports are evidence, not approval.
6. Page 07 contains overlapping legacy boards that require classification.
7. Page 10 is incomplete relative to its legacy package claims: Desktop content exists, Tablet/Mobile branding is clipped, and RTL boards are empty. Tablet, Mobile, and RTL are not current program deliverables.
8. Page 09 has contradictory historical and completion classifications and remains open until resolved by evidence.
9. AUTH-001 and Page 09 remain untouched legacy/draft evidence.
10. Validator runtime behavior, OpenAPI capture, 422 mapping, and admin payment permissions are Phase 1 open questions.
11. Preparation Package backend areas are ready for future evidence work: reporting topics/profiles, materials, practice collections, package definition/version/composition, offers, entitlement reads, practice progress, package exam start, and analytical report reads.
12. Storage/delivery, offline access, workspace aggregation, adaptive practice, retraining, and employer package visibility remain deferred and must not be designed as implemented behavior.

## Live Penpot page inventory

| Page | Page ID | Phase 0 evidence classification | Current treatment |
|---|---|---|---|
| `00 — Cover` | `7cd71457-8d32-8044-8008-55452790a701` | Designed foundation evidence | Preserve; no approval granted |
| `01 — Getting Started` | `70be19b8-cdda-80a2-8008-5553c1540c10` | Populated but sparse; its Utilities-complete claim requires reconciliation | Preserve |
| `02 — Colors` | `70be19b8-cdda-80a2-8008-5553c1554a5a` | Substantial foundation evidence | Preserve; no colors approved |
| `03 — Typography` | `70be19b8-cdda-80a2-8008-5553c155c42c` | Substantial evidence with a stray root-level note | Preserve |
| `04 — Spacing & Shape` | `70be19b8-cdda-80a2-8008-5553c15666fe` | Substantial evidence with one alignment discrepancy | Preserve |
| `05 — Elevation & States` | `70be19b8-cdda-80a2-8008-5553c156e3d0` | Substantial foundation evidence | Preserve |
| `06 — Components` | `70be19b8-cdda-80a2-8008-5553c1575666` | Extensive reference evidence; no local component assets | Preserve |
| `07 — Utilities` | `70be19b8-cdda-80a2-8008-5553c157daa3` | Three overlapping root boards; classification unresolved | Preserve; classify in later evidence work |
| `08 — Product Map` | `151d48c6-c8bb-8080-8008-5d154a789dcc` | Planning inventory, not production screen design | Preserve as legacy evidence |
| `09 — User Flows` | `5b727796-97b4-8084-8008-5d7f57f4a1bd` | Detailed export renders; classification records conflict | Preserve untouched as legacy/draft evidence |
| `10 — Authentication Core` | `9bd8100d-da35-8029-8008-5d963d808253` | Incomplete legacy package; Desktop draft exists, while legacy responsive/RTL/state artifacts are defective or empty | Preserve untouched as legacy/draft evidence |

The detailed object IDs, library inventory, and discrepancy evidence are centralized in `governance/source-authority.md` and `governance/open-questions.md`.

## Phase ledger

| Phase | Status | Gate | Notes |
|---|---|---|---|
| Planning baseline | Complete | — | Master plan and durable goal state created |
| Phase 0 — Authority/live-repository reconciliation | Accepted | G0 | Karam accepted the governance/re-entry baseline on 2026-08-12; backend evidence baseline `8439511`; frontend governance conflict-resolution baseline `1f17718` |
| Phase 1 — Evidence packs | Not started; awaiting separate authorization | G1 | A bounded Preparation Package evidence-packet task is the next allowed work |
| Phase 2 — Shared design/test foundation | Not started | G2 | Existing decisions/assets require live verification |
| Phase 3 — Canonical page inventory | Not started | G3 | Provisional families only |
| Phase 4 — Two-page documentation pilot | Not started | G4 | Choose one simple and one high-risk page after G3 |
| Phase 5 — Page-spec production waves | Not started | G5 | No worker batches authorized |
| Phase 6 — Penpot foundation/family templates | Not started | G6 | No Penpot writes authorized |
| Phase 7 — Penpot Desktop page waves | Not started | G7 | No Penpot writes authorized |
| Phase 8 — Automation-ready handoff | Not started | G8 | Future within Desktop documentation program |

## Historical model allocation (retired)

| Role | Model | Qualification/status |
|---|---|---|
| Manager and final agent reviewer | `openai/gpt-6-sol` | Historical allocation; no current model requirement |
| Markdown author | Approved non-OpenAI worker at the time | Historical bounded-packet workflow |
| Independent reviewer | Approved non-OpenAI supporting reviewer at the time | Historical review workflow; no approval authority |
| Penpot executor | Separately authorized bounded executor under current repository orchestration | Not authorized until a future approved Penpot checkpoint |
| Big Pickle | Approved non-OpenAI worker only under the retired routing workflow | No approval, tracker, or Penpot-write authority |

## Usage state

```yaml
reported_usage_at_planning_start: 65_percent
soft_stop: 75_percent
new_manager_or_penpot_batch_stop: 80_percent
reserved_for_recovery_and_final_review: 20_percent
actual_pilot_usage: not-measured
```

## Open decisions before visual production

| ID | Decision needed | Recommendation | Blocks |
|---|---|---|---|
| OPEN-001 | Canonical Desktop content viewport | Start with `1440 × 1024` | Shared visual contract and Penpot pilot |
| OPEN-002 | Supported Desktop width range | Establish during Desktop layout foundation | Layout contract |
| OPEN-003 | Pinned browser/version, OS, DPR, fonts | Use one deterministic Chromium profile for pilot | Visual automation contract |
| OPEN-004 | Initial designed locale/timezone | English LTR and `Asia/Amman` for pilot | Fixtures and screenshots |
| OPEN-005 | Pixel-diff tolerance | Calibrate after deterministic pilot; one global value | Runtime visual regression |
| OPEN-006 | Quantitative performance budgets | Decide after current frontend/runtime baseline | Frontend NFR approval |
| OPEN-007 | Current release payment behavior | Derive from live code/docs and request product decision where inconsistent | Payment page inventory |
| OPEN-008 | Employer/organization ownership model | Derive and clarify from live sources | Employer pages |
| OPEN-009 | Exact exam timer, resume, attempt, and rationale rules | Derive and clarify from live sources | Exam pages |
| OPEN-010 | Current active milestone authorization for documentation-only frontend design | Resolved by explicit authorization of `NPS-DES-PH0-G0-RECONCILE`; implementation remains unauthorized | Resolved for Phase 0 |

## Current blockers

- G0 is accepted as a governance/re-entry baseline only; it does not authorize implementation or Penpot writes.
- No Phase 1 evidence-packet task is authorized yet.
- The canonical route-level page inventory remains a Phase 3 deliverable; the 11-page Penpot inventory is not a canonical page registry.
- Penpot findings remain evidence only and grant no visual approval.
- All unresolved source, Penpot, and contract discrepancies remain assigned in `governance/open-questions.md`.

## Historical next action (superseded)

Authorize a bounded Preparation Package Phase 1 Evidence Packet that records the current backend/OpenAPI contract evidence for implemented package areas only.

Do not create page specifications, modify Penpot, or change frontend/backend source until separately authorized.

## Historical resume instruction (superseded)

```text
Read docs/frontend/design/MASTER_PLAN.md and docs/frontend/design/GOAL_STATE.md.
Read the three files under docs/frontend/design/governance/.
Confirm the recorded G0 acceptance and the current source revision.
Require a separately approved Phase 1 task packet before evidence-pack work.
Do not modify Penpot, AUTH-001, Page 09, .agent/goal-state.md, CURRENT_TASK.md, or TASKS.md without explicit scope.
```

## Checkpoint history

| Date | Checkpoint | Evidence |
|---|---|---|
| 2026-07-23 | Penpot-only, Desktop-first, Markdown-per-page strategy locked | Current user decision |
| 2026-07-23 | Uploaded project documentation fully read and audited | 4,193-line source dump |
| 2026-07-23 | Historical manager model selected for original design-program plan | Superseded by current repository orchestration: `openai/gpt-6-sol` |
| 2026-07-23 | Master plan and initial goal-state ledger created | `NPS-DES-PLAN-001`, `NPS-DES-STATE-001` |
| 2026-07-23 | Read-only live repository and 11-page Penpot inventories completed | Commit `2c60554f`; Penpot file `01813f71-6684-8025-8008-5d0437a49666` |
| 2026-07-23 | Phase 0 documentation-only reconciliation authorized | `NPS-DES-PH0-G0-RECONCILE` |
| 2026-07-23 | Authority, decision, and discrepancy registers created | `NPS-DES-GOV-SOURCE-001`, `NPS-DES-GOV-DECISION-001`, `NPS-DES-GOV-OPEN-001` |
| 2026-07-23 | Phase 0 reconciliation stopped for review | Gate G0 not yet accepted |
| 2026-08-11 | Backend handoff re-entry reconciled | Backend evidence baseline advanced to `8439511`; G0 remains not accepted pending Karam review |
| 2026-08-12 | Conflict resolution and screen-continuation readiness mapping | Documentation-only reconciliation in review; design-governance baseline is `1c22b59`; G0 remains not accepted |
| 2026-08-12 | G0 governance/re-entry acceptance | Karam accepted G0 only for the backend evidence baseline `8439511` and frontend governance conflict-resolution baseline `1f17718`; Phase 1 remains separately authorized work |

## Proposed Phase 1 Preparation Package Evidence Packet

G0 is accepted. Do not create this packet or any page specification until a separate task authorizes Phase 1. The bounded packet must record the `8439511` source revision and extract only implemented Preparation Package contracts for: admin reporting topics, reporting-profile publication, study materials, practice collections, package definition/version/composition, offers, nurse entitlement reads, practice progress, package exam-session start, and analytical-report reads. It must explicitly exclude storage provider, material delivery, offline access, workspace/dashboard aggregation, adaptive practice, retraining, and employer package visibility.
