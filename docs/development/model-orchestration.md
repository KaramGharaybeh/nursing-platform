# Model Orchestration

## Purpose and authority

This is the canonical project delegation contract. It owns the mandatory packet, context and skill routing, risk/accuracy routing, verifier/expert boundaries, model evidence, repair limits, and delegation results. `docs/development/opencode-agent-runtime.md` documents how the installed OpenCode runtime implements this contract; role files contain only stable agent instructions.

- User decisions and approved repository specifications remain authoritative. Agents route and execute; they do not invent product behavior or authorization.
- The project has exactly three active logical orchestration roles: `dev-orchestrator`, `verifier`, and `expert`.
- `dev-orchestrator` owns user goals, preflight, implementation, documentation, bounded repository discovery, primary verification, routing, correction, and authorized closure. It is the only normal writer in the shared project worktree. It creates no implementation workers.
- `verifier` is an independent, mandatory, read-only subagent for every task, including trivial changes. It returns PASS, FAIL, or BLOCKED; it never repairs or closes a gate.
- `expert` is an advisory OpenAI subagent used only when consequential uncertainty, a qualifying escalation, or unresolved verifier findings justify it. It never writes, delegates, or approves completion.
- OpenAI is not used for ordinary execution or as an automatic final reviewer. The configured expert model is `openai/gpt-5.5`; expert calls are usage-gated by the primary and the user’s goal.

## Active model routes

| Logical role | Runtime agent | Model | Use |
|---|---|---|---|
| Goal owner / implementer / coordinator | `dev-orchestrator` | `opencode/muse-spark-1.3-contributor-free` | Default routine and ordinary goal execution; no silent OpenAI fallback. |
| Independent verifier | `verifier` | `opencode/mimo-v2.6-flash-free` | Mandatory independent review and deterministic verification. |
| Advisory expert | `expert` | `openai/gpt-5.5` | Justified high-consequence reasoning only. |

These identifiers are explicit project profile defaults and are verified against the installed runtime. A native Task call targets a profile and uses that profile’s configured model. Do not claim an actual dispatched model solely from a model’s self-report; use runtime/session evidence where available. If a configured route is unavailable, stop and report the exact failure or use only a separately authorized, explicit free-model fallback attempt. Never silently substitute or retry after a quality failure.

## Native delegation and recursive-delegation boundary

- Native OpenCode Task is the only child-delegation mechanism for project goals. The parent exposes only named task targets `verifier` and `expert`; wildcard delegation is denied.
- `verifier` and `expert` deny Task. Both deny bash-based OpenCode CLI launching; the expert denies bash entirely. Verifier bash is deny-by-default with only bounded verification commands allowed.
- Do not use shell-launched `opencode run --agent ...` for child tasks, the generic harness task shortcut, or `--auto` to bypass ask/deny controls.
- Tool permissions are runtime enforcement, not an OS-level sandbox. Denying direct OpenCode commands blocks the supported recursive routes; permitted test/build commands can still execute project scripts, so do not describe this as a process sandbox.
- No two write-capable actors may operate concurrently against the shared worktree. Verifier starts only after the primary freezes the relevant snapshot.

## Context routing

### Mandatory global context

Every project task packet includes these path references:

- `AGENTS.md`
- `PROJECT_RULES.md`
- `CURRENT_TASK.md`
- `PROGRESS.md`
- `docs/index.md`
- `docs/standards/engineering-standards.md`
- `docs/development/development-guide.md`
- `docs/development/model-orchestration.md`
- `docs/development/opencode-agent-runtime.md`

Workers read the current files from the repository snapshot; packets do not paste complete documents or conversation history.

### Task-specific context routing

The primary adds only task-relevant authority. Do not load unrelated domain internals. If relevant adjacent authority is intentionally omitted, explain that in `CONTEXT_SELECTION_RATIONALE`.

| Domain | Add to global context |
|---|---|
| GENERAL / CROSS-CUTTING | `docs/product/vision.md`, `docs/architecture/system-architecture.md` |
| BACKEND | Product/system architecture, backend architecture, persistence/API authority where affected, and feature/business requirements |
| API | `docs/api/api-design.md`, `docs/backend/backend-architecture.md` as applicable |
| DATABASE | `docs/database/database-design.md`, system architecture as applicable; migration requires explicit authorization |
| FRONTEND | Product/system architecture, frontend architecture/project rules, and task/ledger authority |
| FRONTEND DESIGN | Frontend context plus source-authority/Penpot governance and only relevant design artifacts |
| AUTH / SECURITY | Relevant security, API, business, permission/auth, and architecture contracts/tests |
| PAYMENTS / FINANCIAL | Payment authority plus AUTH / SECURITY context; normally HIGH risk |
| TESTING / VERIFICATION | Applicable test architecture/rules and the task’s acceptance criteria |
| DOCUMENTATION / GOVERNANCE | The authoritative document owning the fact plus `docs/index.md` |

## Skill routing

`AGENTS.md` remains the sole authority for Superpowers skill selection/order. The primary writes `SKILLS_TO_EVALUATE` in each child packet. A fresh child reads `AGENTS.md`, discovers skills using the installed OpenCode skill mechanism, loads all applicable required skills, and reports `SKILLS_EVALUATED`, `SKILLS_LOADED`, and `SKILL_REASONING`. Missing required skill access or unresolved authority means BLOCKED; never claim an inaccessible skill was loaded.

Legacy global `opencode-delegate` and `nps-delegate` skills remain compatibility assets, not this project’s dispatch path. Project-local `/goal` commands select `dev-orchestrator`; it must use native Task and must not invoke those CLI relays for Nursing Platform delegation.

## Mandatory Delegation Packet

Before invoking either child, create one compact packet containing every field below. `TASK_ID` is required for a roadmap task; otherwise supply `TASK_LABEL`. Empty `FALLBACK_MODELS`, `ALLOWED_FILES`, or other collection values are permitted only when that emptiness is intentional and explained.

- `PACKET_ID`
- `TASK_ID` (or `TASK_LABEL` for non-roadmap governance work)
- `TASK_PURPOSE`
- `TASK_TYPE / DOMAIN`
- `RISK_LEVEL`
- `ACCURACY_REQUIREMENT`
- `ASSIGNED_ROLE`
- `ASSIGNED_MODEL`
- `FALLBACK_MODELS`
- `REPOSITORY_SNAPSHOT`
- `WORKING_TREE_EXPECTATION`
- `ALLOWED_FILES`
- `FORBIDDEN_FILES`
- `GLOBAL_CONTEXT_MODULES`
- `TASK_CONTEXT_MODULES`
- `CONTEXT_SELECTION_RATIONALE`
- `SKILLS_TO_EVALUATE`
- `KNOWN_CONSTRAINTS`
- `REQUIREMENTS`
- `ACCEPTANCE_CRITERIA`
- `VERIFICATION_REQUIRED`
- `FORBIDDEN_ASSUMPTIONS`
- `STOP_CONDITIONS`
- `SCOPE / BUDGET`
- `RESULT_SHAPE`

The primary validates the packet before dispatch with the project-local dependency-free command:

```bash
node .opencode/scripts/validate-delegation-packet.mjs .agent/delegation-packet.json
```

Dispatch only on `PACKET_VALID`. The validator derives field names from this section rather than keeping a second schema list. It verifies structure/presence, not semantic truth; the primary remains responsible for correctness and context selection.

Do not paste complete files or long histories. Include only necessary goals, authority references, snapshot/diff facts, constraints, acceptance criteria, and evidence. A verifier rereview gets prior findings, correction diff, and regression scope. An expert gets only the unresolved reasoning problem and minimum relevant authority.

## Primary preflight and adaptive reasoning

Before substantial work, `dev-orchestrator` determines task eligibility/authority, task/domain, risk, accuracy requirement, minimal relevant context, applicable skills, scope, acceptance criteria, verification evidence, stop conditions, and whether expert input is justified.

Reasoning tiers and the control limitations are specified in `docs/development/opencode-agent-runtime.md`. In brief: LOW covers mechanical local work; MEDIUM is the ordinary default; HIGH covers consequential uncertainty, architecture/security, cross-module state, concurrency, or substantial rework risk; XHIGH/MAX is exceptional. Task size alone never selects a tier. Native Task model and variant use the target profile configuration; per-call variant override must not be assumed. When a stronger reasoning mode is actually needed, consult the expert rather than inventing a native Task parameter.

Consult expert before implementation when uncertainty is itself consequential, including architecture, security/auth/authorization, destructive data behavior, concurrency, cross-module state, irreversible migrations, or integrity-sensitive behavior. Otherwise, primary decides and implements.

## Mandatory verification, correction, and escalation lifecycle

Each bounded task follows:

`GOAL → GOVERNANCE PREFLIGHT → ACCEPTANCE CRITERIA → RISK / REASONING CLASSIFICATION → IMPLEMENT → PRIMARY SELF-VERIFICATION → VERIFIER → PASS → GOVERNANCE-AUTHORIZED CLOSURE → FINAL REPORT`

1. Primary implements and runs focused local verification, then freezes the relevant diff/snapshot.
2. Primary sends a complete packet and exact acceptance/evidence contract to verifier.
3. Verifier independently checks requirement coverage, scope, relevant source/security/business invariants, and actual verification evidence, returning PASS, FAIL, or BLOCKED.
4. FAIL triggers a bounded evidence-driven correction by the primary, local verification, and verifier rereview. After two evidence-driven repair attempts against the same unresolved root problem, consult expert. A material HIGH-risk uncertainty may call expert before implementation instead of waiting for failures.
5. Primary applies expert recommendations; primary performs local verification and verifier rereviews. If that rereview still fails, stop BLOCKED.
6. Only verifier PASS plus satisfied task/ledger/user gates permits primary closure. The verifier and expert never close their own gates.
7. Do not return ordinary intermediate phase completion to the user. Continue automatically until COMPLETE, BLOCKED, HUMAN_DECISION_REQUIRED, or SECURITY_ACTION_REQUIRED.

HIGH-risk topics include authentication, authorization, permissions, payments/financial behavior, fulfillment, entitlements, concurrency, transactions, idempotency, migrations, sensitive personal data, session provenance, reports, and file authorization. Follow stronger domain-specific project gates.

## Authorization and project gates

Explicit user authorization remains separately required where repository governance requires it: staged specifications/plans when applicable, implementation, database/migration changes, staging, commits, pushes, or starting a subsequent project stage. The `GOAL-FE-001` Frontend Standing Implementation Authorization exception remains limited to its existing ledger criteria; it does not authorize staging, commits, push, backend/API/database changes, or a next stage.

Use the least powerful justified reasoning/model. Expert is not a default reviewer. The configured expert is `openai/gpt-5.5` and may only be invoked by the primary on an allowed named Task route. Never configure another OpenAI project role or use OpenAI as a routine fallback.

Git rules remain exact: preserve unrelated state; never broadly stage; no push; no reset, clean, stash, restore, checkout, rebase, or amend. `git add`/`git commit` require the explicit authorization and configured permission action. Confirm staged files and worktree state at closure.

## Evidence contract

Every child result includes:

- `STATUS` (`PASS`, `FAIL`, or `BLOCKED` for verifier; expert uses advisory `PASS`/`BLOCKED` and is never a gate);
- `TASK_ID` or `TASK_LABEL`, `PACKET_ID`, `ASSIGNED_MODEL`;
- `FILES_READ`, `CONTEXT_MODULES_READ`, `CONTEXT_SELECTION_RATIONALE`, `CONSTRAINTS_APPLIED`;
- `SKILLS_EVALUATED`, `SKILLS_LOADED`, `SKILL_REASONING`;
- `FILES_CHANGED` (verifier/expert must report none);
- `REQUIREMENT_COVERAGE` with evidence locations;
- `VERIFICATION` with exact commands/results and Git state;
- `OPEN_QUESTIONS` / `UNRESOLVED_QUESTIONS`, `ASSUMPTIONS`, `RISKS`, `STOP_REASON`, and `EVIDENCE_LOCATIONS`.

Verifier findings are severity-ordered Critical, High, Medium, Low. Every material finding gives the requirement/contract clause, file/symbol, problem, impact, required correction, and evidence. Expert results additionally give diagnosis, recommendation, affected paths, risks, and verification criteria.

The primary’s final report maps every acceptance criterion to evidence, lists changed files and verification, notes remaining uncertainty, and reports the terminal GOAL status. Actual commands—not claims—are required.
