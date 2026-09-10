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
| DEC-PH0-002 | Penpot is the sole visual authority for the original Phase 0 design-documentation baseline. | Karam, 2026-07-23 | Superseded for current implementation governance by DEC-PH0-018. Figma and any other design tool remain non-authoritative unless Karam explicitly records a future decision changing that rule. Penpot remains constrained by repository architecture, security, accessibility, privacy, and backend contracts. |
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
| DEC-PH0-014 | Existing evidence alone did not satisfy G0. | G0 required explicit Karam acceptance; no acceptance record existed at the time | Historical prerequisite later satisfied by DEC-PH0-016. |
| DEC-PH0-015 | Authorize documentation-only frontend/design conflict resolution and screen-continuation readiness mapping. | Karam, 2026-08-12 | May reconcile documented authority, target-size, scope, baseline, and readiness wording from recorded evidence; does not accept G0 or authorize page specifications, Penpot writes, Angular implementation, or backend changes. |
| DEC-PH0-016 | Accept G0 as a frontend design governance/re-entry baseline only. | Karam, 2026-08-12 | Authorizes only a separately scoped Phase 1 evidence-packet task. It does not authorize Angular creation or implementation, Penpot writes, page specifications, a final route registry, backend changes, deferred features, or visual approval of any screen. |
| DEC-PH0-017 | Adopt Storybook as the intended frontend visual development/review tooling model. | Technical lead, 2026-09-10 | Storybook becomes the intended primary visual development/review surface for production Angular components and production screen states after separate tooling authorization. It is not requirements authority, not installed by this decision, and cannot self-approve components or screens. |
| DEC-PH0-018 | Update Penpot's current implementation-governance role. | Technical lead, 2026-09-10 | Penpot is no longer mandatory as a procedural intermediate artifact for every component or routine screen composition. Penpot remains the approved design-exploration/specification tool when materially new visual intent is unresolved. Historical Penpot-only decisions remain preserved as their original design-program baseline. |
| DEC-PH0-019 | Preserve screen approval and testing gates under Storybook. | Technical lead, 2026-09-10 | Storybook visual evidence may support future owning gates after installation/configuration is separately approved, but screen-family eligibility, exact screen approval, dependencies, implementation gates, functional tests, accessibility checks, quality/build, and technical-lead approval remain required. Automated visual regression remains unapproved. |

## Reconciled architecture direction

The live working copy of `docs/frontend/frontend-architecture.md` specifies Angular 22, standalone architecture, Signals, RxJS, Angular Material, Angular CDK where justified, SCSS, and a future project-owned Angular Material theme that will derive from Penpot tokens only after those tokens are approved. The frontend workspace remains uninitialized, so these are approved architectural constraints rather than implemented capabilities or approval of current Penpot tokens.

## Approval boundaries

This decision log does not approve:

- any Penpot page, board, color, token, component, or layout;
- AUTH-001 or Page 09 as production-ready;
- a Desktop viewport or visual-regression baseline;
- Storybook installation, configuration, dependencies, scripts, stories, screenshot regression, image snapshots, hosted visual review, browser/DPR matrix, or pixel tolerance policy;
- any Storybook-only component implementation, duplicate design system, duplicate markup, duplicate SCSS, route/navigation inventory, product state, or business requirement;
- a canonical page inventory;
- a page specification;
- Angular or backend implementation;
- any current uncommitted file for commit or merge.
