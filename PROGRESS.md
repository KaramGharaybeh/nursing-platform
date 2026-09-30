# Project Current State

> Compact operational handoff only (target ~1-3 KB). Completed history lives in
> `PROGRESS_HISTORY.md` (read-on-demand, non-authoritative). Task/Gate traceability lives in the
> execution ledgers. Do NOT accumulate completed reports, transcripts, or old handoffs here.

## Active Goal

- `MAX-FE-PROGRESS-2026-09-25` COMPLETE for authorized Low/Medium scope on
  `feat/2026-09-23-shared-shell-navigation`: Commerce orders COM-007/008 (`417371f`), Admin exam
  categories ADM-005 (`1a50fe5`), Admin exams ADM-006/007 (`e029733`). Each coherent batch has
  independent verifier PASS and exact-scope local commit. No push; see ledger/goal state for evidence.

## Live Blockers / Human Decisions

- `SECURITY_ROTATION_REQUIRED` (human action): revoke/rotate `ATRIA_API_KEY`,
  `PENPOT_MCP_USER_TOKEN`, `STITCH_API_KEY`; populate the environment; restart OpenCode. Blocks
  provider/MCP activation only.
- COM-004/005/006 remain deferred under their own gates; `T-FE-089` / `T-FE-137` remain
  external production-only.
- `T-FE-114` Reporting Topics needs category lookup but its approved `ReportingTopics.Manage`
  access alone does not grant the category-list API's `Exams.View`. No extra permission is inferred.
- Remaining exam version publication/retirement, protected question authoring, user roles/security,
  payment product, and package-authoring tasks cross High/consequential boundaries; Employer home
  depends on unmounted privacy-sensitive candidate/request routes. Separate authority required.

## Protected Worktree / Safety Notes

- Preserve unrelated dirty tracked: `.gitignore`, `docs/frontend/design/GOAL_STATE.md`,
  `docs/frontend/design/MASTER_PLAN.md`, `frontend/angular.json`; newly modified `opencode.jsonc`
  is external/runtime work and must remain untouched and unstaged.
- Preserve untracked: `.agent/`, `.opencode/skills/`, `.playwright-mcp/`, `.superpowers/`, `0`,
  `Playwright MCP Full Review/`, `docs/superpowers/plans/2026-09-24-opencode-orchestration-runtime.md`,
  `logs/`, `run-local.sh`.
- No push. Exact-scope staging only. See `.agent/goal-state.md` for the completed GOAL checkpoint.

## Next Authorized Action

- No further bounded Low/Medium product batch identified in the current DAG. Request explicit
  authorization/decision for a blocked or High-risk branch; do not push or touch protected state.

## Authority Pointers

- `AGENTS.md`, `PROJECT_RULES.md`, `CURRENT_TASK.md`
- `docs/development/model-orchestration.md` (canonical orchestration contract),
  `docs/development/opencode-agent-runtime.md`, `docs/index.md`
- `docs/frontend/execution/frontend-implementation-ledger.md` (Task/Gate authority),
  `.agent/goal-state.md` (completed GOAL state)
