---
description: Report the active Nursing Platform GOAL without changing work.
agent: dev-orchestrator
---

Load the installed `durable-goal` skill. Read `.agent/goal-state.md` and inspect `git status --short` without changing files.

Report the goal ID/title, status, current phase, last verified checkpoint, next incomplete checkpoint, branch/HEAD, persisted IDs, verifier findings, blockers, completion-gate progress, and staged/commit status. If no state file exists, report that no durable goal is active.
