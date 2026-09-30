---
description: Start a Nursing Platform GOAL through the project dev-orchestrator.
agent: dev-orchestrator
---

Treat the supplied text as the authoritative GOAL:

$ARGUMENTS

Load the installed `durable-goal` skill. Read `AGENTS.md`, `PROJECT_RULES.md`, `CURRENT_TASK.md`, the compact `PROGRESS.md` handoff, and `docs/development/model-orchestration.md`. Inspect `.agent/goal-state.md` and the Git baseline before planning or editing. Use targeted reads thereafter; `PROGRESS_HISTORY.md` and full ledgers are on-demand evidence only.

If an Active or Blocked goal already exists, preserve it and ask whether to resume it or explicitly replace it. Do not overwrite completed history or protected work. Otherwise persist a concise `.agent/goal-state.md` with the full goal or a lossless prompt reference, acceptance criteria, branch/HEAD, protected paths, bounded phases, current step, verification, unresolved findings, and resume instructions.

Own the goal through implementation, focused checks, mandatory independent native-Task verification, bounded repair/rereview, and governance-authorized closure. Do not create implementation workers. Do not stage, commit, or push unless the goal and project governance explicitly authorize the exact action. Never use shell-launched `opencode run` for child delegation. Stop only at the documented terminal states or genuine human decision/security/access blockers.
