# Model Orchestration

## Purpose

This document is the canonical central delegation/orchestration contract for this repository. It owns:

- model routing;
- risk routing;
- accuracy assessment;
- direct-execution vs delegation decisions;
- context routing;
- skill-routing integration;
- delegation packet;
- orchestrator preflight;
- worker preflight;
- evidence contract;
- reviewer contract;
- verifier contract;
- fallback;
- repair limits;
- escalation;
- context efficiency;
- safe worker handoff.

It exists to:

- Minimize shared OpenAI weekly-limit consumption.
- Use one OpenAI orchestrator only.
- Delegate repository work to approved workers through bounded, auditable packets.
- Reliably force fresh-context workers to read the correct authority, evaluate skills, perform preflight, respect scope, and return deterministic evidence.
- Require independent review and deterministic evidence.
- Preserve explicit user approval gates.

`AGENTS.md` remains the global AI/skill authority. Agent profile files reference this contract compactly and must not duplicate it.

## Authority

- User business decisions remain authoritative.
- Approved repository specifications remain authoritative.
- The orchestrator may route work but may not invent business rules.
- Subagents may execute or review but may not authorize phases, commits, or scope expansion.
- Delegated workers are executors and review helpers. They are NOT final authorities. No delegated worker self-approves project completion. Every delegated result returns to the OpenAI orchestrator for final review.
- Worker inference is never authoritative.

## Planes

CONTROL PLANE / FINAL AUTHORITY:

- `openai/gpt-5.5` — receives the user task, classifies, routes context/skills/models, builds packets, supervises, reviews, validates evidence, applies the final quality gate, and decides PASS / correction / STOP / escalation.

DELEGATED WORKER PLANE:

- Approved non-OpenAI worker pool only (see below). Executes bounded packets and returns evidence. Never final authority.

VERIFICATION / REVIEW PLANE:

- Delegated reviewers and verifiers check requirement compliance, scope, and deterministic evidence. They advise; `openai/gpt-5.5` decides.

## OpenAI Model Policy

`openai/gpt-5.5` is the project's OpenAI orchestrator. This regime is unchanged by ORCH-001.

No project agent other than `orchestrator` may use an `openai/*` model. Do NOT configure any other OpenAI model for project delegation or fallback — including, even if visible in the local OpenCode UI, GPT-5.4 mini, GPT-5.6, GPT-5.6 Luna/Sol/Terra, or any other OpenAI model.

## Approved Non-OpenAI Worker Pool

Exact IDs below were verified with read-only `opencode models` (193 models listed) during ORCH-001. Do NOT guess IDs from UI display names; re-verify with `opencode models` if routing fails.

| Priority | Display name | Exact model ID | Provider | Role use |
|---|---|---|---|---|
| 1 (preferred) | Muse Spark 1.3 | `opencode/muse-spark-1.3-contributor-free` | opencode | Default delegated worker when suitable and available |
| 2 | Big Pickle | `opencode/big-pickle` | opencode | Fallback worker; routine low-risk work; deterministic verification; git/scope guardianship |
| 3 | MiMo V2.5 | `opencode/mimo-v2.5-free` | opencode | Second fallback worker; independent review helper |

Default availability/fallback order: Muse Spark 1.3 → Big Pickle → MiMo V2.5.

This is NOT a blind capability order. The orchestrator may choose another approved pool worker when task risk, accuracy, capability, or reviewer independence makes it the better choice, and may execute directly. No new providers, no credential changes, no package installations to support routing.

## Direct Execution vs Delegation

The orchestrator must NOT blindly delegate every task. Before dispatch it decides:

- A. `openai/gpt-5.5` performs the task directly; or
- B. the task can safely be delegated to an approved worker.

Consider: RISK_LEVEL, ACCURACY_REQUIREMENT, TASK_TYPE, BUSINESS_CRITICALITY, SECURITY_IMPACT, ARCHITECTURAL_IMPACT, REVERSIBILITY, COMPLEXITY, WORKER_CAPABILITY, CURRENT_MODEL_AVAILABILITY.

Direct orchestrator execution/reasoning may be preferable for (non-exhaustive): architecture decisions, security-sensitive changes, authentication/authorization, payment/financial logic, destructive operations, ambiguous cross-module decisions, conflict resolution, final technical review, high-impact production fixes, and tasks where delegated output cannot safely be accepted without effectively redoing the task. The orchestrator owns the decision.

## Roles

1. Primary Orchestrator and Final Gate.
2. Repository Scout.
3. Architecture and Specification Agent.
4. Main Implementation Agent.
5. Routine Low-Risk Worker.
6. Deterministic Verification Agent.
7. Independent Deep Reviewer.
8. Documentation Agent.
9. Documentation Reviewer.
10. Git and Scope Guardian.

## Exact Routing

Routing mechanism: repository agent profiles define DEFAULT models in frontmatter (`model:`). Fresh OpenCode runs additionally support explicit runtime model override through `--agent` + `--model` (see Runtime Model Override and Executable Fallback). The orchestrator routes by selecting a profile and may re-dispatch the SAME bounded packet through the SAME role profile using the next approved worker model on availability failure. `repo-scout-fallback` remains a pre-existing named scout fallback profile, but it is NOT the general mechanism for the project-wide Muse → Big Pickle → MiMo fallback chain. Use only supported OpenCode syntax; do NOT invent unsupported configuration syntax.

| Role | Profile | Model |
|---|---|---|
| Primary Orchestrator and Final Gate | `orchestrator` | `openai/gpt-5.5` |
| Repository Scout | `repo-scout` | `opencode/muse-spark-1.3-contributor-free` |
| Repository Scout fallback | `repo-scout-fallback` | `opencode/big-pickle` |
| Architecture and Specification Agent | `architect` | `opencode/mimo-v2.5-free` |
| Main Implementation Agent | `main-implementer` | `opencode/muse-spark-1.3-contributor-free` |
| Independent Deep Reviewer, security reviewer, complex debugging, concurrency, transaction, authorization, and payment reasoning | `deep-reviewer` | `opencode/mimo-v2.5-free` |
| Routine Low-Risk Worker | `routine-worker` | `opencode/big-pickle` |
| Deterministic Verification Agent | `verifier` | `opencode/big-pickle` |
| Documentation Agent | `documentation` | `opencode/muse-spark-1.3-contributor-free` |
| Documentation Reviewer | `documentation-reviewer` | `opencode/mimo-v2.5-free` |
| Git and Scope Guardian | `git-guardian` | `opencode/big-pickle` |

Active delegated profile invariant: every REPOSITORY-DEFINED profile with DELEGATION_ELIGIBLE = YES uses exactly one of `opencode/muse-spark-1.3-contributor-free`, `opencode/big-pickle`, or `opencode/mimo-v2.5-free`. The only permitted exception is `orchestrator` on `openai/gpt-5.5`. No active delegated repository-owned profile uses Qwen, GLM, Nemotron, DeepSeek, north-mini, or any other model.

Routing history (replaced, not current): ORCH-001 round 1 initially routed `repo-scout` to `opencode/north-mini-code-free`, `repo-scout-fallback` to `opencode/deepseek-v4-flash-free`, and `deep-reviewer` to `nvidia/deepseek-ai/deepseek-v4-pro` (all unresolvable or unexecutable), and retained pre-existing Qwen/GLM/Nemotron specialist routes. Correction round 1 replaced all of them with approved-pool defaults above. History ends here; do not treat those IDs as current workers.

No project agent other than `orchestrator` may use an `openai/*` model.

## Reserve Pool (NOT APPROVED FOR CURRENT PROJECT DELEGATION)

The following models are historical benchmark candidates only. They are NOT part of the approved delegated worker pool and MUST NOT receive any delegated Nursing Platform task, FALLBACK_MODELS entry, or profile route unless the user explicitly changes the approved worker pool in a future decision:

- `opencode/laguna-s-2.1-free`
- `opencode/ling-3.0-flash-free`
- `nvidia/meta/llama-3.1-70b-instruct`

Reserve models must not receive architecture, security, payment, migration, or final-review authority.

## Risk Routing

Classify RISK_LEVEL per task. Payments/financial work defaults to high-risk unless existing authority explicitly supports another classification.

Low risk:

- Approved pool executor (default Muse Spark 1.3 when suitable).
- Different approved pool reviewer.
- Deterministic verification.
- Orchestrator receives only the concise evidence summary.

Medium risk:

- Repository scout.
- Architecture/planning when needed.
- Approved pool or main implementation agent.
- Independent review (different worker from implementer where available).
- Deterministic verification.
- Orchestrator final gate.

High risk:

- Repository scout.
- Architecture/invariant/specification analysis.
- Implementation according to task (orchestrator may execute directly).
- Independent security/technical review.
- Business-invariant review when business behavior is affected.
- Deterministic verification.
- Orchestrator final gate.
- Explicit user approval.

High-risk topics include:

- Authentication.
- Authorization.
- Permissions.
- JWT and refresh tokens.
- Payments.
- Fulfillment.
- Entitlements.
- Package attempts.
- Session provenance.
- Reports and report access.
- Concurrency.
- Transactions.
- Idempotency.
- Migrations.
- Sensitive personal data.
- File authorization.

ACCURACY_REQUIREMENT (Standard / High / High-Precision) is assessed alongside risk and may raise the handling tier or force direct orchestrator execution even for otherwise low-risk tasks.

## Context Routing, Skill Routing, Model Routing

Three separate concepts owned by the orchestrator. Do not conflate them.

- CONTEXT ROUTING: what authoritative repository information must the worker read?
- SKILL ROUTING: what existing `AGENTS.md` workflow skills must be evaluated/loaded?
- MODEL ROUTING: which approved model/role should execute the bounded task?

## Global Context Module Baseline

GLOBAL_CONTEXT_MODULES (always included by path reference, never pasted in full):

- `AGENTS.md`
- `PROJECT_RULES.md`
- `CURRENT_TASK.md`
- `PROGRESS.md`
- `docs/index.md`
- `docs/standards/engineering-standards.md`
- `docs/development/development-guide.md`
- `docs/development/model-orchestration.md`

These paths are context references, not text payloads. The packet provides the repository snapshot plus exact file paths; the worker reads repository authority directly. This preserves the existing `AGENTS.md` Required Reading Order (which additionally requires feature-implementation docs) and makes it more deterministic, not weaker.

## Task-Specific Context Routing

The orchestrator classifies TASK_TYPE / DOMAIN and adds only the additional relevant authority (paths/categories, never document contents). Do NOT create a separate context-registry file; this matrix is the registry.

| Domain | Add to global baseline |
|---|---|
| GENERAL / CROSS-CUTTING | `docs/product/vision.md`, `docs/architecture/system-architecture.md` |
| BACKEND | `docs/product/vision.md`, `docs/architecture/system-architecture.md`, `docs/backend/backend-architecture.md`, plus persistence/API authorities below when affected, plus relevant feature/business requirements |
| API | `docs/api/api-design.md`, `docs/backend/backend-architecture.md` as applicable |
| DATABASE | `docs/database/database-design.md`, `docs/architecture/system-architecture.md` as applicable; migrations require explicit user authorization |
| FRONTEND | `docs/product/vision.md`, `docs/architecture/system-architecture.md`, `docs/frontend/frontend-architecture.md`, `docs/frontend/frontend-project-rules.md`, relevant frontend execution/task authority |
| FRONTEND DESIGN | Frontend row plus source-authority/Penpot governance (`docs/frontend/design/governance/source-authority.md`), canonical OpenAPI/integration authority when integration is involved, and only the actually relevant design files. Never load all Penpot/design docs for unrelated frontend infrastructure work |
| AUTH / SECURITY | All relevant security constraints, API rules, backend/frontend architecture as applicable, business rules, permission/auth patterns, relevant contracts/tests |
| PAYMENTS / FINANCIAL | Payments/financial authority plus AUTH / SECURITY row; default high-risk |
| TESTING / VERIFICATION | Applicable test architecture/rules from `docs/standards/engineering-standards.md`, backend/frontend architecture as applicable, and the task's acceptance criteria |
| DOCUMENTATION / GOVERNANCE | The single authoritative document owning the fact plus `docs/index.md` |

Do not load unrelated frontend design-program documentation for backend work, or unrelated backend internals for visual design work. If required authority cannot be identified safely: STOP / ESCALATE.

## Skill-Routing Integration

`AGENTS.md` remains the sole skills authority: mandatory Superpowers workflow, skill-selection table, and selection order are unchanged and are NOT duplicated here.

For delegation, the orchestrator sets SKILLS_TO_EVALUATE (skill names from `AGENTS.md`, not instructions or paths) in the packet. The worker evaluates/loads per `AGENTS.md` and reports SKILLS_EVALUATED, SKILLS_LOADED, SKILL_REASONING. If required skill instructions cannot actually be accessed: STOP; never claim a skill was loaded when it was not.

## Mandatory Delegation Packet

Every delegation carries one compact, auditable packet. Required fields:

- PACKET_ID
- TASK_ID (or TASK_LABEL for non-roadmap governance work)
- TASK_PURPOSE
- TASK_TYPE / DOMAIN
- RISK_LEVEL
- ACCURACY_REQUIREMENT
- ASSIGNED_ROLE
- ASSIGNED_MODEL
- FALLBACK_MODELS
- REPOSITORY_SNAPSHOT
- WORKING_TREE_EXPECTATION
- ALLOWED_FILES
- FORBIDDEN_FILES
- GLOBAL_CONTEXT_MODULES
- TASK_CONTEXT_MODULES
- SKILLS_TO_EVALUATE
- KNOWN_CONSTRAINTS
- REQUIREMENTS
- ACCEPTANCE_CRITERIA
- VERIFICATION_REQUIRED
- FORBIDDEN_ASSUMPTIONS
- STOP_CONDITIONS
- SCOPE / BUDGET
- RESULT_SHAPE

Do NOT include full copies of repository documentation or complete conversation history. Fresh-context workers must be able to execute safely from packet + explicit requirements + repository paths + snapshot + constraints alone.

## Orchestrator Preflight

Before delegation, the orchestrator must:

1. Understand the user request.
2. Identify task/domain.
3. Evaluate risk.
4. Evaluate accuracy requirement.
5. Identify mandatory global context.
6. Select task-specific context.
7. Evaluate applicable skills.
8. Determine allowed files/scope.
9. Determine forbidden files/scope.
10. Identify requirements.
11. Define acceptance criteria.
12. Define required verification.
13. Define stop conditions.
14. Determine direct execution vs delegation.
15. Select worker/model if delegated.
16. Select valid fallback options.
17. Create the packet.
18. Validate packet completeness.
19. Dispatch only after validation.

If a safe complete packet cannot be constructed: DO NOT DISPATCH. STOP / ESCALATE.

## Worker Preflight

A delegated worker must:

1. Validate PACKET_ID/task.
2. Verify repository snapshot/status where applicable.
3. Verify allowed scope.
4. Verify forbidden scope.
5. Read GLOBAL_CONTEXT_MODULES.
6. Read TASK_CONTEXT_MODULES.
7. Evaluate/load skills according to `AGENTS.md`.
8. Distill applicable constraints/invariants.
9. Identify missing authority.
10. Identify conflicting authority.
11. Identify missing design/business/security decisions.
12. Report preflight evidence.
13. STOP on unresolved blocker.
14. Only then begin permitted execution/editing.

Workers must not immediately implement from a short task sentence.

## Incomplete Packet Rule

If mandatory packet information is missing: STOP. Return STATUS = BLOCKED with STOP_REASON = INCOMPLETE_DELEGATION_PACKET and list the missing fields. The worker must not guess the missing contract.

## Conflict / Missing-Authority Rule

ORCH-001 creates no second competing global authority hierarchy. Existing project precedence rules are preserved, including stronger scoped frontend authority (`docs/frontend/frontend-project-rules.md` consult order, `docs/frontend/design/governance/source-authority.md` hierarchy).

If authoritative sources conflict and existing precedence cannot resolve the conflict: STOP / ESCALATE TO ORCHESTRATOR. If a required business/design/security/architecture decision is absent: STOP / ESCALATE. Worker inference is not authoritative.

## Evidence Contract

Every delegated result must include:

- STATUS.
- TASK_ID.
- PACKET_ID.
- ASSIGNED_MODEL.
- FILES_READ.
- CONTEXT_MODULES_READ.
- CONSTRAINTS_APPLIED.
- SKILLS_EVALUATED.
- SKILLS_LOADED.
- SKILL_REASONING.
- FILES_CHANGED.
- REQUIREMENT_COVERAGE.
- VERIFICATION (commands executed, exact build/test results, exact Git status).
- OPEN_QUESTIONS.
- UNRESOLVED_QUESTIONS where applicable (alias of OPEN_QUESTIONS; either label accepted).
- ASSUMPTIONS.
- RISKS.
- STOP_REASON.
- EVIDENCE_LOCATIONS.

For implementation tasks, existing `AGENTS.md` command/evidence rules apply: full file contents and real command outputs where required; summary claims are not substitutes.

### Requirement Coverage

REQUIREMENT_COVERAGE must concisely map each supplied requirement, acceptance criterion, authoritative clause, or business/security invariant to its disposition (met / not met / blocked + evidence location). Tests passing alone is never sufficient evidence when requirements were not checked.

Every reviewer report must include findings ordered as:

- Critical.
- High.
- Medium.
- Low.

## Reviewer Contract

Delegated review must not be limited to style/lint. Reviewers must evaluate, as applicable: requirement compliance, architecture, security/privacy, business/functional invariants, contract correctness, scope drift, and verification evidence.

Each material finding must include:

- SEVERITY.
- REQUIREMENT / CONTRACT CLAUSE.
- FILE / SYMBOL where applicable.
- PROBLEM.
- IMPACT.
- REQUIRED CORRECTION.
- EVIDENCE.

Findings remain severity ordered. Keep review prose minimal.

## Verifier Contract

Verifier roles independently verify deterministic evidence where appropriate and never merely repeat implementer claims. Check as assigned: actual git scope, build/test outputs, file existence, dependency integrity, required evidence completeness, prohibited-file absence, and contract/acceptance conformance. The verifier is not the final approval authority; `openai/gpt-5.5` remains the final gate.

## Orchestrator Final Review

Every delegated result returns to `openai/gpt-5.5` for final review. The orchestrator evaluates: requirement compliance, context-module compliance, architecture compliance, security/privacy where applicable, business invariants where applicable, file/scope integrity, tests/verification, worker assumptions, risks, unresolved questions, and evidence completeness.

Final project verdicts reuse existing terminology where possible: PASS, PASS WITH FIXES, REWORK REQUIRED, BLOCKED / STOP.

## Independence Rules

- An executor must not be the sole reviewer of its own work.
- Do not intentionally use the same delegated worker as both implementation worker and its only independent delegated reviewer when another approved worker is available. If MiMo itself performed an implementation, it must not be treated as its own sole independent delegated reviewer.
- Architecture authored by one worker must be independently reviewed by a different worker where available.
- Implementation authored by one worker must be independently reviewed by a different worker where available.
- Implementation affecting business behavior must receive business-invariant review.
- Verification evidence must come from actual commands, not claims.
- The final OpenAI orchestrator must review summaries and critical evidence, not repeat all routine repository work.
- Regardless of delegated review, `openai/gpt-5.5` performs final review.

## Authorization Gates

Explicit user authorization is required separately for:

- Creating a staged specification.
- Creating an implementation plan.
- Starting implementation.
- Modifying the database.
- Creating migrations.
- Staging.
- Committing.
- Pushing.
- Beginning the next project stage.

Discussion, analysis, agreement, or clarification is not authorization.

## One-Repair Cap / Stall Protection

Default delegated correction behavior: one targeted repair attempt, then re-run required verification. Never silently broaden task scope during repair.

If the same material issue remains or critical evidence is still missing, STOP uncontrolled repair looping. The orchestrator chooses whether to switch approved worker, execute/take over directly, escalate, or stop.

Availability failure (unavailable model, access denied, provider unavailable, timeout, rate limit, removed model, transport error) may trigger fallback to another approved worker. Quality/execution failure (missed requirement, wrong implementation, architecture violation, scope drift, missing tests/evidence, ignored context, incorrect assumptions, security/business rule violation) returns to the orchestrator for review and does NOT blindly retry through the model pool.

## Runtime Model Override and Executable Fallback

Mechanism verdict: RUNTIME_MODEL_OVERRIDE_SUPPORTED.

Local evidence (read-only inspection, no state-changing test):

- `opencode run --help` exposes `-m/--model` ("model to use in the format of provider/model") and `--agent` ("agent to use") as independent options combinable in one invocation.
- Installed skill `opencode-delegate`, references/dispatch-and-poll.md documents the dispatch form `opencode run --format json --agent build -m provider/model < brief.txt` for a fresh run, with `--model` required and `--agent` independently selectable; a resumed run inherits its session's model.
- Installed skill `opencode-delegate`, SKILL.md states the relay requires `--model` on every fresh run and the orchestrator picks the model per task from the human-approved set.
- Project skill `nps-delegate` requires running OpenCode with an explicitly selected agent (`nps-isolated`) and an explicitly selected free model per delegation.

Executable fallback behavior: the SAME bounded delegation packet (brief) is re-dispatched through the same role profile with the next pool model, e.g. first `--agent <profile> --model opencode/muse-spark-1.3-contributor-free`, on availability failure `--agent <profile> --model opencode/big-pickle`, then `--agent <profile> --model opencode/mimo-v2.5-free`, each time carrying the current snapshot/diff context per Safe Worker Handoff. No extra generic fallback profiles are required for this; none were created (generic fallback profiles: NONE). The dedicated `repo-scout-fallback` profile is retained as the pre-existing scout fallback path.

Default selection vs fallback: Muse Spark 1.3 is preferred when candidates are otherwise similarly suitable — NOT a capability ranking. Deterministic verification may default to Big Pickle, independent deep review to MiMo, repository exploration to Muse, and high-risk architecture may stay with `openai/gpt-5.5` directly. The Muse → Big Pickle → MiMo sequence is the DEFAULT availability fallback only. Quality failure never triggers automatic fallback; it returns to `openai/gpt-5.5` under the one-repair cap.

## Canonical Delegation Mechanism and Dispatch Fence

The ONE supported Nursing Platform dispatch path is a fresh OpenCode run using `opencode run --agent <repository-profile> --model <approved-provider/model>` with the bounded packet as the brief, or the equivalent installed `opencode-delegate` / `nps-delegate` relay invocation that produces exactly this command.

The orchestrator must NOT dispatch Nursing Platform repository roles through a subagent-type/harness shortcut that does not honor repository-owned profile/model routing. Live verification proved such a shortcut can resolve a stale model (observed: a harness `repo-scout` dispatch resolving `opencode/north-mini-code-free`, which is not a repository route) while the canonical `opencode run --agent repo-scout --model opencode/muse-spark-1.3-contributor-free` path honors the repository profile. Dispatch-path STOP rule: if a dispatch mechanism resolves a model different from ASSIGNED_MODEL or outside the approved worker pool, STOP the dispatch, do not let it perform project work, and re-dispatch through the canonical mechanism.

## Assigned-Model Integrity

The orchestrator must verify that the actual dispatched model matches ASSIGNED_MODEL before accepting worker execution. For runtime fallback, the orchestrator creates/updates the attempt with the explicitly selected approved model. If actual execution reports ACTUAL_MODEL != ASSIGNED_MODEL, STOP / REJECT that attempt unless the orchestrator explicitly initiated an approved fallback attempt. No silent model substitution.

## Safe Worker Handoff

- Never run two write-capable agents concurrently on overlapping files in the same working tree. Parallel delegated implementation is permitted only for genuinely independent scopes.
- Before switching workers after partial implementation, inspect repository state and current diff.
- The next worker must explicitly receive current snapshot/diff context; never assume a fallback worker knows what a prior worker changed.
- Never silently discard prior changes.
- Never reset/clean/stash merely to switch workers unless user-authorized.
- Existing worktree/parallel-agent rules and applicable skills remain authoritative.

## Escalation Rules

Escalate when:

- Two agents disagree on a business invariant.
- Required evidence is missing.
- A model cannot follow file scope.
- Security or financial correctness is uncertain.
- Concurrency or transaction behavior remains unproved.
- Tests conflict with the approved specification.
- A required model is unavailable.
- Model routing cannot be determined safely (report MODEL_ROUTING_REQUIRES_TECHNICAL_LEAD_REVIEW and continue contract hardening where possible).
- The task requires an unapproved business decision.

The orchestrator must stop for the user instead of guessing.

## Context-Efficiency Rules

- Use repository scouts instead of having the orchestrator search broadly.
- Give subagents narrow, self-contained tasks.
- Reference global minimum + task-specific context by repository path, never paste full documents.
- Reference the repository snapshot instead of repeating unchanged context.
- Reuse this central contract instead of duplicating worker instructions.
- Do not paste entire historical conversations into subagent prompts.
- Reference committed authoritative files and exact paths.
- Return concise evidence summaries.
- Avoid rereading unchanged documents unless required.
- Never use another OpenAI model merely as a reviewer or fallback.

## Lifecycle Examples (Non-Normative)

Frontend lifecycle: user task → orchestrator classifies FRONTEND → selects frontend context rows → evaluates skill requirements → assesses risk/accuracy → decides direct vs delegated → selects worker → creates bounded packet → worker preflights → worker executes → worker returns evidence → verifier/reviewer if appropriate → orchestrator final review → PASS / correction / STOP.

Backend lifecycle: same orchestration flow, but task-specific context comes from the BACKEND / API / DATABASE / AUTH / PAYMENTS rows above. Examples are explanatory, not a second specification.
