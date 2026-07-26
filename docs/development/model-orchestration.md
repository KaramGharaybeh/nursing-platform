# Model Orchestration

## Purpose

This document is the authoritative policy for model routing and multi-agent operation in this repository.

- Minimize shared OpenAI weekly-limit consumption.
- Use one OpenAI orchestrator only.
- Delegate repository work to specialized OpenCode-native and Nvidia models.
- Require independent review and deterministic evidence.
- Preserve explicit user approval gates.

## Authority

- User business decisions remain authoritative.
- Approved repository specifications remain authoritative.
- The orchestrator may route work but may not invent business rules.
- Subagents may execute or review but may not authorize phases, commits, or scope expansion.

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

All model identifiers below are exact identifiers returned by `opencode models` for OpenCode 1.18.5.

| Role | Model |
|---|---|
| Primary Orchestrator and Final Gate | `openai/gpt-5.5` |
| Repository Scout | `opencode/north-mini-code-free` |
| Repository Scout fallback | `opencode/deepseek-v4-flash-free` |
| Architecture and Specification Agent | `nvidia/z-ai/glm-5.2` |
| Main Implementation Agent | `nvidia/qwen/qwen3-coder-480b-a35b-instruct` |
| Independent Deep Reviewer, security reviewer, complex debugging, concurrency, transaction, authorization, and payment reasoning | `nvidia/deepseek-ai/deepseek-v4-pro` |
| Routine Low-Risk Worker | `opencode/big-pickle` |
| Deterministic Verification Agent | `opencode/big-pickle` |
| Verification fallback | `opencode/deepseek-v4-flash-free` |
| Documentation Agent | `nvidia/z-ai/glm-5.2` |
| Documentation Reviewer | `opencode/nemotron-3-ultra-free` |
| Git and Scope Guardian | `opencode/big-pickle` |

No project agent other than `orchestrator` may use an `openai/*` model.

## Reserve Pool

These models are reserve-only until benchmarked:

- `opencode/laguna-s-2.1-free`
- `opencode/ling-3.0-flash-free`
- `opencode/mimo-v2.5-free`
- `nvidia/meta/llama-3.1-70b-instruct`

Reserve models must not receive architecture, security, payment, migration, or final-review authority.

## Risk Routing

Low risk:

- Free executor.
- Different free reviewer.
- Deterministic verification.
- Orchestrator receives only the concise evidence summary.

Medium risk:

- Repository scout.
- GLM architecture/planning when needed.
- Qwen implementation.
- DeepSeek independent review.
- Big-pickle verification.
- Orchestrator final gate.

High risk:

- Repository scout.
- GLM invariant/specification analysis.
- Qwen or DeepSeek implementation according to task.
- DeepSeek security/technical review.
- GLM business-invariant review.
- Big-pickle deterministic verification.
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

## Independence Rules

- An executor must not be the sole reviewer of its own work.
- GLM-authored architecture must be independently reviewed by DeepSeek.
- Qwen-authored code must be independently reviewed by DeepSeek.
- DeepSeek implementation must receive GLM invariant review when business behavior is affected.
- Verification evidence must come from actual commands, not claims.
- The final OpenAI orchestrator must review summaries and critical evidence, not repeat all routine repository work.

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

## Evidence Contract

Every executor report must include:

- Assigned scope.
- Files read.
- Files changed.
- Assumptions.
- Unresolved questions.
- Commands executed.
- Exact build/test results.
- Exact Git status.
- Risks.
- Evidence locations.

Every reviewer report must include findings ordered as:

- Critical.
- High.
- Medium.
- Low.

Each finding must include:

- File and symbol or line.
- Problem.
- Impact.
- Required correction.
- Supporting evidence.

## Escalation Rules

Escalate when:

- Two agents disagree on a business invariant.
- Required evidence is missing.
- A model cannot follow file scope.
- Security or financial correctness is uncertain.
- Concurrency or transaction behavior remains unproved.
- Tests conflict with the approved specification.
- A required model is unavailable.
- The task requires an unapproved business decision.

The orchestrator must stop for the user instead of guessing.

## Context-Efficiency Rules

- Use repository scouts instead of having the orchestrator search broadly.
- Give subagents narrow, self-contained tasks.
- Do not paste entire historical conversations into subagent prompts.
- Reference committed authoritative files and exact paths.
- Return concise evidence summaries.
- Avoid rereading unchanged documents unless required.
- Never use another OpenAI model merely as a reviewer or fallback.
