# Phase 0 Decision Log

```yaml
document_id: NPS-DES-GOV-DECISION-001
version: 1.0
status: phase-0-in-review
recorded_at: 2026-07-23
timezone: Asia/Amman
```

## Decision policy

Only explicit decisions from Karam or approved repository authorities may be recorded as locked. This log does not convert draft Penpot artifacts, uncommitted files, tracker checkboxes, or agent findings into approval.

## Locked decisions

| ID | Decision | Evidence | Consequence |
|---|---|---|---|
| DEC-PH0-001 | Authorize `NPS-DES-PH0-G0-RECONCILE` as documentation-only governance and reconciliation work. | Karam, 2026-07-23 | The approved five-file write set may be updated; no implementation phase is authorized. |
| DEC-PH0-002 | Penpot is the sole visual authority. | Karam, 2026-07-23 | Figma and any other design tool are non-authoritative for this program. Penpot remains constrained by repository architecture, security, accessibility, privacy, and backend contracts. |
| DEC-PH0-003 | Current scope is Desktop browser only. | Karam, 2026-07-23 | Tablet and Mobile boards are not deliverables in the current design-documentation program. Their live defects remain evidence, not current repair scope. |
| DEC-PH0-004 | RTL is future scope. | Karam, 2026-07-23 | Current Desktop work preserves readiness but creates or repairs no RTL boards. |
| DEC-PH0-005 | AUTH-001 and Page 09 remain legacy/draft evidence and must not be modified. | Karam, 2026-07-23 | Existing boards, trackers, audits, and completion claims are preserved for later evidence reconciliation. |
| DEC-PH0-006 | `docs/frontend/design/GOAL_STATE.md` owns this design-documentation program. | Karam, 2026-07-23 | Resume and checkpoint decisions for this program come from the in-repository design goal state. |
| DEC-PH0-007 | `.agent/goal-state.md` remains untouched legacy state. | Karam, 2026-07-23 | It is not synchronized, corrected, or used to expand current scope. |
| DEC-PH0-008 | The live Penpot inventory is evidence, not visual approval. | Karam, 2026-07-23 | No page or board receives approved status from Phase 0 inspection. |
| DEC-PH0-009 | Validator runtime behavior, OpenAPI capture, 422 mapping, and admin payment permissions are Phase 1 open questions. | Karam, 2026-07-23 | Phase 0 records but does not answer or implement these contract questions. |
| DEC-PH0-010 | The existing active milestone and roadmap files remain unchanged. | Approved task boundary, 2026-07-23 | Explicit Phase 0 authorization is a bounded documentation exception and does not mark Administration, Frontend, or any implementation phase complete. |
| DEC-PH0-011 | `claude-fable-5.md` is excluded. | Karam, 2026-07-23 | It is not read, cited, or used as Nursing Platform evidence. |
| DEC-PH0-012 | No Phase 1 work begins automatically after this reconciliation. | Stop-for-review requirement, 2026-07-23 | Gate G0 and the next task packet require explicit review and authorization. |
| DEC-PH0-013 | Reconcile the frontend/design evidence baseline to backend handoff commit `8439511`. | Current authorized task, 2026-08-11 | Records Preparation Package backend-ready and deferred boundaries without authorizing Phase 1, page specifications, Penpot, or Angular work. |
| DEC-PH0-014 | G0 is not accepted on existing evidence alone. | G0 requires explicit Karam acceptance; no acceptance record exists | Human review remains the sole route to accept or reject G0. |

## Reconciled architecture direction

The live working copy of `docs/frontend/frontend-architecture.md` specifies Angular 22, standalone architecture, Signals, RxJS, Angular Material, Angular CDK where justified, SCSS, and a future project-owned Angular Material theme that will derive from Penpot tokens only after those tokens are approved. The frontend workspace remains uninitialized, so these are approved architectural constraints rather than implemented capabilities or approval of current Penpot tokens.

## Approval boundaries

This decision log does not approve:

- any Penpot page, board, color, token, component, or layout;
- AUTH-001 or Page 09 as production-ready;
- a Desktop viewport or visual-regression baseline;
- a canonical page inventory;
- a page specification;
- Angular or backend implementation;
- any current uncommitted file for commit or merge.
