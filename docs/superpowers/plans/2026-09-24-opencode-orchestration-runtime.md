# OpenCode Orchestration Runtime Implementation Plan

> **Historical / superseded plan — not current execution authority.** The runtime described here has been retired. Its commands, paths, models, and next-action language record the 2026-09-24 plan only.

> **For agentic workers:** Execute inline in this session with bounded verification checkpoints. Do not spawn implementation workers or call the OpenAI expert during setup.

**Goal:** Replace project routing complexity with three active project-local logical roles—`dev-orchestrator`, `verifier`, and `expert`—using native Task delegation, mandatory independent verification, secure credential references, and durable goal commands.

**Architecture:** The project config sets `dev-orchestrator` as the primary/default agent and assigns validated free models to routine execution and verification. The primary is the sole shared-worktree writer and invokes only `verifier` or advisory `expert` through native Task permissions; explicit bash denies prevent child CLI dispatch. The canonical packet and evidence contract stays in `model-orchestration.md`; a dependency-free validator derives required packet fields from that document. Credentials move to `{env:...}` interpolation and require external rotation.

**Tech Stack:** OpenCode 1.18.32 JSONC/frontmatter configuration, native Task tool, Node.js built-ins (`node:test`), Markdown governance.

## Global Constraints

- Preserve branch `feat/2026-09-23-shared-shell-navigation`, starting HEAD `380b91d66c3dd7e4ec8675f6fea97a122c6681b`, and all pre-existing dirty/untracked state.
- Do not modify Nursing Platform application/product source.
- Do not print, test, or retain credential values; replace literals with supported `{env:NAME}` references.
- Do not call the OpenAI expert or use OpenAI child agents during setup.
- Do not stage or commit without explicit authorization; never push or use destructive Git operations.
- Retain legacy agent files and global compatibility assets; disable redundant project roles only after references and runtime tests pass.
- Treat credential revocation/rotation as an external human action; report `SECURITY_ROTATION_REQUIRED` until confirmed.

---

### Task 1: Validate installed runtime and baseline

**Files:** None.

- Confirm branch, HEAD, staged state, dirty/untracked inventory.
- Confirm candidate free models via `opencode models`; do not generate model calls yet.
- Probe `default_agent`, named `permission.task` patterns, and `{env:...}` interpolation through `OPENCODE_CONFIG_CONTENT` and `opencode debug config`, filtering output so values are never printed.
- Preserve the completed `.agent/goal-state.md`; do not replace it.
- Inspect exact current provider/MCP credential fields through a redacting runtime-config summary only.

### Task 2: Add packet validation test-first

**Files:**
- Create: `.opencode/scripts/validate-delegation-packet.mjs`
- Create: `.opencode/scripts/validate-delegation-packet.test.mjs`
- Read: `docs/development/model-orchestration.md`

- Derive mandatory field names from the canonical “Mandatory Delegation Packet” section rather than maintaining a second list.
- RED: tests prove a complete packet is accepted, missing fields are rejected, `TASK_ID`/`TASK_LABEL` is an either/or, and empty required values fail.
- GREEN: implement a built-in-only validator that reads one JSON packet file and emits only concise pass/failure field names, never field values.
- Run `node --test .opencode/scripts/validate-delegation-packet.test.mjs` after RED and GREEN.

### Task 3: Configure the three active roles and secret-safe project config

**Files:**
- Create: `.opencode/agents/dev-orchestrator.md`
- Replace: `.opencode/agents/verifier.md`
- Create: `.opencode/agents/expert.md`
- Update: `opencode.jsonc`

- Configure `dev-orchestrator` as primary/default with `opencode/muse-spark-1.3-contributor-free`.
- Configure `verifier` as `subagent`, non-writing/non-delegating, with `opencode/muse-spark-1.3-contributor-free`.
- Configure `expert` as `subagent`, non-writing/non-delegating, with `openai/gpt-6-sol`; no expert invocation during setup.
- Grant the primary native `task` access only to `verifier` and `expert`; deny shell-launched OpenCode dispatch.
- Disable redundant legacy custom agents via project configuration, retain their files, and leave OpenCode built-ins enabled.
- Set `default_agent` and project default model only after role files exist.
- Replace provider/MCP inline secrets with `ATRIA_API_KEY`, `PENPOT_MCP_USER_TOKEN`, and `STITCH_API_KEY` environment references; preserve non-secret endpoints and model/server configuration.

### Task 4: Migrate governance, durable-goal commands, and runtime guide

**Files:**
- Update: `docs/development/model-orchestration.md`
- Create: `docs/development/opencode-agent-runtime.md`
- Update: `AGENTS.md`
- Create: `.opencode/commands/goal.md`
- Create: `.opencode/commands/goal-resume.md`
- Create: `.opencode/commands/goal-status.md`
- Create: `.opencode/commands/goal-cancel.md`

- Keep packet schema/evidence contract canonical in `model-orchestration.md`; replace current OpenAI-final-gate/CLI-dispatch/many-agent requirements with the authorized three-role native-Task policy.
- Document adaptive reasoning tiers, fixed native subagent model/variant behavior as proven at runtime, mandatory verifier fail→repair→rereview limits, expert escalation, writer boundary, exact stop conditions, and status outcomes.
- Integrate `.agent/goal-state.md` with bounded checkpoints without overwriting a completed goal or storing transcripts.
- Ensure project-local `/goal` explicitly targets `dev-orchestrator`; project-local resume/status/cancel commands preserve durable-goal safety semantics.
- Do not edit global configuration, global skills, or compatibility files outside the repository.

### Task 5: Validate loaded config and effective permissions

**Files:** No additional changes unless a validation defect requires a bounded correction.

- Check `opencode debug config` with a redacting parser, `opencode agent list`, and `opencode debug agent` for the three active agents.
- Prove default agent, model IDs, native Task target allowlist, source edits denied for verifier/expert, task recursion denied, and `opencode run` bash spawning denied.
- Prove no inline secret values remain, using only boolean/presence summaries and a redacted content scan.
- Record hidden internal background model fields and avoid changing compaction unless the installed runtime exposes safe, verifiable control.

### Task 6: Run disposable free-model and native-Task integration tests

**Files:** Temporary workspace under `/tmp/opencode`; no application source changes.

- Verify the routine free model can make a tiny bounded change, run a local test, and report accurately.
- Verify the free verifier detects an intentionally defective version and passes the corrected version.
- Run one complete native Task lifecycle: primary implements, automatically calls verifier, receives FAIL, repairs once, reruns local checks, calls verifier again, receives PASS, and reports COMPLETE.
- Capture actual free provider/model from runtime session/event evidence; model self-report is insufficient.
- Do not invoke expert; report `EXPERT_RUNTIME_ROUTING_NOT_INVOKED_DURING_SETUP`.

### Task 7: Final scope and security review

**Files:** No additional changes unless bounded defects are found.

- Verify project application-source paths are unchanged and only orchestration/governance files were newly changed.
- Verify final staged area remains empty, all original dirty/untracked state remains, no commit/push occurred, and no credentials are staged.
- Retain `SECURITY_ROTATION_REQUIRED` until the user/provider confirms revocation/rotation and supplies new environment values out of band.
- Report exact model, config, delegation, closure, rollback, and restart evidence.
