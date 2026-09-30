---
description: Cancel an active Nursing Platform GOAL while preserving completed work.
agent: dev-orchestrator
---

Load the installed `durable-goal` skill and read `.agent/goal-state.md`.

Record the current phase, persisted IDs, verified checkpoints, unresolved findings, and the user-supplied cancellation reason. Mark the state `Cancelled` without deleting the file or completed work. Do not stage, commit, push, or alter unrelated files.
