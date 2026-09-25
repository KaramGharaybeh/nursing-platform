# OpenCode Agent Runtime

## Scope and source-of-truth split

This document records the project runtime layout and operating lifecycle for the installed OpenCode `1.18.32` configuration family. The packet/evidence and project-governance contract remains `docs/development/model-orchestration.md`; `AGENTS.md` remains the global AI behavior/skill entry point. Agent profiles contain stable role instructions, while `opencode.jsonc` contains actual defaults, disable flags, and secret references.

Active logical roles:

| Role | Profile | Mode | Configured model | Worktree write |
|---|---|---|---|---|
| Goal owner, implementer, documentation and routine discovery | `dev-orchestrator` | `primary` | `opencode/muse-spark-1.3-contributor-free` | Yes; sole normal writer |
| Mandatory independent review and verification | `verifier` | `subagent` | `opencode/mimo-v2.6-flash-free` | No |
| Advisory consequential-reasoning escalation | `expert` | `subagent` | `openai/gpt-5.5` | No |

OpenCode built-ins (`build`, `plan`, `explore`, `general`, `compaction`, `summary`, `title`) remain built-in runtime capabilities. The project does not disable them. Historical custom agent files are retained for rollback but disabled in the project config after reference migration.

## Permission and delegation enforcement

- The primary’s `permission.task` is deny-by-default and allows only `verifier` and `expert`. It creates no implementation child.
- Both children have `task: deny`. Verifier has `edit: deny`, bash deny-by-default with only verification commands allowed, and explicit denial of `opencode *` and Git/package/destructive mutations. Expert has `edit: deny`, `task: deny`, and `bash: "*": deny`.
- The primary also denies shell-launched OpenCode child runs. Native Task is the only supported child route.
- These are OpenCode tool permissions, not an OS process sandbox. An allowed test/build command can run repository scripts; profiles do not claim stronger operating-system isolation.
- Task delegation begins only after the primary freezes the changed snapshot. No writer and reviewer run concurrently against a changing shared worktree.
- A Task target’s static agent profile selects its model. The observed native Task calls in OpenCode 1.18.32 accepted `description`, `prompt`, and `subagent_type`; this Task interface exposes no per-call model/variant override. A static agent `variant` is accepted by `opencode debug agent`, and CLI `opencode run --variant` controls a top-level run, not a native Task call. Do not inject unsupported Task fields or silently override the configured model. Use the fixed profile default and expert escalation when appropriate.

## Adaptive reasoning policy

The primary classifies risk and minimum reasoning before substantial work. This classification changes context depth, verification intensity, and expert escalation—not the ordinary free primary model.

- **LOW:** mechanical local change, rename, trivial documentation, or known local lint/test correction. Small packet and focused checks; verifier is still mandatory.
- **MEDIUM:** default for ordinary implementation/configuration, standard bug fixes, related-file changes, and normal test design. Select relevant task context and test scope.
- **HIGH:** architecture, security/auth/authorization, cross-module state, unclear root cause, complicated state, concurrency, consequential data behavior, or meaningful ambiguity/rework risk. Add domain authority and consider expert before implementation if the uncertainty itself is consequential.
- **XHIGH/MAX:** exceptional only when HIGH has not resolved consequential ambiguity and the provider/runtime supports the requested effort. Native Task variant support is not presumed. If unsupported, keep the configured profile model/variant and escalate to expert when criteria are met.

Task size alone never selects a tier: large mechanical work can remain MEDIUM, while a small security defect can require HIGH.

## Goal lifecycle

Project-local `/goal`, `/goal-resume`, `/goal-status`, and `/goal-cancel` commands target `dev-orchestrator`. `default_agent` also selects `dev-orchestrator` for ordinary project sessions. A direct initial session may be started with:

```bash
opencode run --agent dev-orchestrator --model opencode/muse-spark-1.3-contributor-free "<GOAL>"
```

Do not pass `--auto` for project work; it can approve ask-gated Git operations. The primary owns the GOAL through these normal transitions without asking the user to relay findings:

`GOAL → PREFLIGHT → IMPLEMENT → LOCAL VERIFY → NATIVE TASK VERIFIER → CORRECT/REREVIEW AS NEEDED → AUTHORIZED CLOSURE → TERMINAL REPORT`

Every coherent batch requires independent verifier review before completion: related eligible Low/Medium Tasks share one review at the batch boundary with per-Task acceptance mapping, while High/consequential changes are reviewed strictly at their own boundary. A Task ID is not automatically a separate session, verifier invocation, full-suite run, or commit. The verifier returns `PASS`, `FAIL`, or `BLOCKED`; it never repairs. The primary may perform two evidence-driven repair attempts for an unresolved verifier root finding. It may use expert earlier for qualifying consequential uncertainty. After the second failed repair it must consult expert; after an expert-informed correction the verifier rereviews. A remaining failure becomes `BLOCKED`.

Terminal statuses are `COMPLETE`, `BLOCKED`, `HUMAN_DECISION_REQUIRED`, and `SECURITY_ACTION_REQUIRED`. Ordinary implementation/review/checkpoint transitions are not terminal.

## Durable goal state

Use `.agent/goal-state.md` as concise operational state, not as a transcript. Include:

- goal and acceptance criteria;
- current task/ledger ID and status;
- branch and HEAD baseline;
- protected paths and decisions;
- completed implementation checkpoints;
- primary and verifier evidence;
- active findings and repair attempts;
- expert consultation and recommendation, if any;
- unresolved blockers and next action.

Update state after a verified checkpoint and before handoff. Preserve a pre-existing Active/Blocked goal rather than silently replacing it. A completed state is history; a new user GOAL may create a fresh state when repository policy permits.

## Packet validation and limitations

Before native Task, the primary serializes the full packet contract from `model-orchestration.md` as JSON and runs:

```bash
node .opencode/scripts/validate-delegation-packet.mjs .agent/delegation-packet.json
```

The dependency-free validator reads the required field names from the canonical packet section, checks required-key/value presence and the `TASK_ID`/`TASK_LABEL` alternative, and emits no packet contents. It is structural validation only: it cannot prove the truth of scope, risk, requirements, model assignment, or context rationale. Primary remains responsible for those semantics. Packet schema is not duplicated here.

## Credentials and provider requirements

Project configuration stores no literal provider credentials. It references these environment variables using the OpenCode-supported `{env:NAME}` syntax:

- `ATRIA_API_KEY` for the Atria OpenAI-compatible provider;
- `PENPOT_MCP_USER_TOKEN` for the Penpot MCP URL token;
- `STITCH_API_KEY` for the Stitch MCP header.

The previous values are treated as compromised. Revoke/rotate each affected credential with its provider, then set the new values in the process/environment secret store without pasting them into chat or project files. Until external rotation is confirmed and the environment is populated, report migration status `SECURITY_ROTATION_REQUIRED` (goal terminal state `SECURITY_ACTION_REQUIRED`); do not claim that MCP/provider authentication works. No credentials are stored in goal packets or `.agent/goal-state.md`.

## Background model policy

Project configuration explicitly routes `title` and `summary` built-ins to the validated free Muse model. Built-in `compaction` remains unmodified to protect state fidelity; its current effective model is `UNKNOWN` rather than inferred. OpenCode 1.18.32 accepts a static `agent.compaction.model` override in effective debug output, but no compaction quality evaluation was authorized here. Assess fidelity before assigning a cheaper compaction model.

## Legacy compatibility and rollback

Retained but project-disabled custom profiles: `orchestrator`, `free-orchestrator`, `deep-reviewer`, `architect`, `documentation`, `documentation-reviewer`, `git-guardian`, `main-implementer`, `routine-worker`, `repo-scout`, `repo-scout-fallback`, and the global `nps-isolated` profile. Their files and global skills are not deleted. The project-local commands override global goal commands by name; the global `durable-goal`, `opencode-delegate`, and `nps-delegate` assets remain installed but the latter two are not project dispatch authority.

Rollback without deleting compatibility assets: first preserve the current worktree diff and the human's unrelated dirty/untracked state. Restore only the migration-owned agent/command/governance changes against the known pre-migration commit `380b91d66c3dd7e4ec8675f6fea97a122c6681b6` after review; the previous agent profiles remain in `.opencode/agents/`. Rebuild `opencode.jsonc` as a **sanitized** config using the prior non-secret settings and new `{env:...}` references, changing only `default_agent`, `model`, and legacy `disable` flags as appropriate. Never restore the previously dirty credential-bearing config or any credential-bearing backup; those values were exposed and must be rotated. Re-enable old agents only after checking routes, existing worktree changes, and provider access. A rollback is a separately authorized migration, not an instruction to reset/restore the current dirty tree automatically.

## Runtime activation

OpenCode loads config/agents/commands at process start. After project configuration changes, close the current OpenCode session and start a new project session so the three-role configuration, default agent, commands, and permissions are loaded.
