# Project Current State

> Compact operational handoff only (target ~1-3 KB). Completed history lives in
> `PROGRESS_HISTORY.md` (read-on-demand, non-authoritative). Task/Gate traceability lives in the
> execution ledgers. Do NOT accumulate completed reports, transcripts, or old handoffs here.

## Active Goal

- `GOV-STREAMLINE-2026-09-25` (ACTIVE): governance-only migration to the streamlined agent
  execution model. Branch `feat/2026-09-23-shared-shell-navigation`. No feature Tasks in scope;
  T-FE-089 / T-FE-137 explicitly excluded.
- Prior: `T-FE-088` CLOSED classification-only at `1a711f5`; verifier PASS packet
  `T-FE-088-CLASSIFICATION-001` remains authoritative; COM-007/008 BLOCKED + NOT STARTED.

## Live Blockers / Human Decisions

- `SECURITY_ROTATION_REQUIRED` (human action): revoke/rotate `ATRIA_API_KEY`,
  `PENPOT_MCP_USER_TOKEN`, `STITCH_API_KEY`; populate the environment; restart OpenCode. Blocks
  provider/MCP activation only.
- COM-007/008 need metadata + implementation authority (`T-FE-089` needs its own gate). Do not start.

## Protected Worktree / Safety Notes

- Preserve unrelated dirty tracked: `.gitignore`, `docs/frontend/design/GOAL_STATE.md`,
  `docs/frontend/design/MASTER_PLAN.md`, `frontend/angular.json`.
- Preserve untracked: `.opencode/skills/`, `.playwright-mcp/`, `.superpowers/`, `0`,
  `Playwright MCP Full Review/`, `docs/superpowers/plans/2026-09-24-opencode-orchestration-runtime.md`,
  `logs/`, `run-local.sh`.
- No push. Exact-scope staging only. See `.agent/goal-state.md` for the active GOAL checkpoint.

## Next Authorized Action

- Finish `GOV-STREAMLINE-2026-09-25`: verify governance diff, obtain ONE independent verifier PASS,
  make the exact-scope local commit `chore(governance): streamline agent execution`, then STOP.

## Authority Pointers

- `AGENTS.md`, `PROJECT_RULES.md`, `CURRENT_TASK.md`
- `docs/development/model-orchestration.md` (canonical orchestration contract),
  `docs/development/opencode-agent-runtime.md`, `docs/index.md`
- `docs/frontend/execution/frontend-implementation-ledger.md` (Task/Gate authority),
  `.agent/goal-state.md` (active GOAL state)
