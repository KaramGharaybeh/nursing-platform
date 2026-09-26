# Project Current State

> Compact operational handoff only (target ~1-3 KB). Completed history lives in
> `PROGRESS_HISTORY.md` (read-on-demand, non-authoritative). Task/Gate traceability lives in the
> execution ledgers. Do NOT accumulate completed reports, transcripts, or old handoffs here.

## Active Goal

- `MAX-FE-PROGRESS-2026-09-25` (ACTIVE): implementation GOAL on
  `feat/2026-09-23-shared-shell-navigation`. Batch 1 COM-007/008 metadata/OpenAPI/typed client,
  facade, screens/routes/tests/stories and contract updates received independent verifier PASS
  (`BATCH1-COM0708-VERIFY-001`, including bounded doc rereview). Exact-scope local closure and
  DAG reassessment are in progress.

## Live Blockers / Human Decisions

- `SECURITY_ROTATION_REQUIRED` (human action): revoke/rotate `ATRIA_API_KEY`,
  `PENPOT_MCP_USER_TOKEN`, `STITCH_API_KEY`; populate the environment; restart OpenCode. Blocks
  provider/MCP activation only.
- Client regeneration permission was reloaded and the approved script succeeded. COM-004/005/006
  remain deferred under their own gates; `T-FE-089` / `T-FE-137` remain external production-only.

## Protected Worktree / Safety Notes

- Preserve unrelated dirty tracked: `.gitignore`, `docs/frontend/design/GOAL_STATE.md`,
  `docs/frontend/design/MASTER_PLAN.md`, `frontend/angular.json`.
- Preserve untracked: `.opencode/skills/`, `.playwright-mcp/`, `.superpowers/`, `0`,
  `Playwright MCP Full Review/`, `docs/superpowers/plans/2026-09-24-opencode-orchestration-runtime.md`,
  `logs/`, `run-local.sh`.
- No push. Exact-scope staging only. See `.agent/goal-state.md` for the active GOAL checkpoint.

## Next Authorized Action

- Complete Batch 1 exact-scope local commit, reassess DAG for another eligible Low/Medium batch,
  continue automatically where authorized. No push.

## Authority Pointers

- `AGENTS.md`, `PROJECT_RULES.md`, `CURRENT_TASK.md`
- `docs/development/model-orchestration.md` (canonical orchestration contract),
  `docs/development/opencode-agent-runtime.md`, `docs/index.md`
- `docs/frontend/execution/frontend-implementation-ledger.md` (Task/Gate authority),
  `.agent/goal-state.md` (active GOAL state)
