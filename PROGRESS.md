# Project Current State

> Compact operational handoff only (target ~1-3 KB). Completed history lives in
> `PROGRESS_HISTORY.md` (read-on-demand, non-authoritative). Task/Gate traceability lives in the
> execution ledgers. Do NOT accumulate completed reports, transcripts, or old handoffs here.

## Active Goal

- `MAX-FE-PROGRESS-2026-09-25` (ACTIVE) on `feat/2026-09-23-shared-shell-navigation`.
  Batch 1 COM-007/008 and Batch 2 ADM-005 passed independent review and were committed locally
  as `417371f` and `1a50fe5` (no push). Batch 3 `T-FE-107` ADM-006/007 Admin Exams is in progress:
  typed response metadata/OpenAPI/client, facade, guarded list/detail/forms, tests/stories are
  uncommitted; independent verifier PASS `BATCH3-ADM006007-VERIFY-001`; exact-scope commit and
  DAG reassessment pending.

## Live Blockers / Human Decisions

- `SECURITY_ROTATION_REQUIRED` (human action): revoke/rotate `ATRIA_API_KEY`,
  `PENPOT_MCP_USER_TOKEN`, `STITCH_API_KEY`; populate the environment; restart OpenCode. Blocks
  provider/MCP activation only.
- COM-004/005/006 remain deferred under their own gates; `T-FE-089` / `T-FE-137` remain
  external production-only. Credential rotation remains a separate human action.
- `T-FE-114` Reporting Topics needs category lookup but its approved `ReportingTopics.Manage`
  access alone does not grant the category-list API's `Exams.View`. No extra permission is inferred.

## Protected Worktree / Safety Notes

- Preserve unrelated dirty tracked: `.gitignore`, `docs/frontend/design/GOAL_STATE.md`,
  `docs/frontend/design/MASTER_PLAN.md`, `frontend/angular.json`; newly modified `opencode.jsonc`
  is external/runtime work and must remain untouched and unstaged.
- Preserve untracked: `.opencode/skills/`, `.playwright-mcp/`, `.superpowers/`, `0`,
  `Playwright MCP Full Review/`, `docs/superpowers/plans/2026-09-24-opencode-orchestration-runtime.md`,
  `logs/`, `run-local.sh`.
- No push. Exact-scope staging only. See `.agent/goal-state.md` for the active GOAL checkpoint.

## Next Authorized Action

- Complete Batch 3 exact-scope local commit and reassess DAG for eligible Low/Medium work. No push.

## Authority Pointers

- `AGENTS.md`, `PROJECT_RULES.md`, `CURRENT_TASK.md`
- `docs/development/model-orchestration.md` (canonical orchestration contract),
  `docs/development/opencode-agent-runtime.md`, `docs/index.md`
- `docs/frontend/execution/frontend-implementation-ledger.md` (Task/Gate authority),
  `.agent/goal-state.md` (active GOAL state)
