# AI Agent Operating Contract

This file governs AI-agent behavior in the Nursing Platform repository. Follow the explicit human authorization for the bounded task, [PROJECT_RULES.md](PROJECT_RULES.md), this contract, and the task's authoritative documentation. A tool or skill never grants project, product, design, security, Git, or scope authority by itself.

## Start with the owner

1. Read this file, then [PROJECT_RULES.md](PROJECT_RULES.md).
2. Use [docs/index.md](docs/index.md) to find the owner of the question or task. Read [docs/delivery/current-state.md](docs/delivery/current-state.md) when current repository implementation or status matters.
3. Read only the task-specific Product, Architecture, technical, Testing, Operations, Delivery, or [Engineering Standards](docs/standards/engineering-standards.md) owner needed for the work. Inspect affected source, tests, contracts, and configuration to verify implementation claims.
4. Establish the authorized scope, applicable acceptance criteria, branch, HEAD, and working-tree status before consequential edits. Preserve unrelated tracked and untracked work.

Do not routinely bulk-read `CURRENT_TASK.md`, `PROGRESS.md`, `PROGRESS_HISTORY.md`, legacy task lists, old master plans, historical Superpowers plans/specifications, reports, execution ledgers, or retired design programs. Read bounded portions only when an explicit task, current owner, or [PROJECT_RULES.md](PROJECT_RULES.md) establishes their relevance. For feature work, read the relevant `CURRENT_TASK.md` scope as PROJECT_RULES.md requires; it is not the owner of general current repository status. Dated “current,” “next,” or “blocked” statements in legacy files do not replace the permanent owners. The active task's human instructions are essential context; this reading path does not authorize implementation.

One fact has one authoritative owner. [docs/index.md](docs/index.md) routes Product behavior, Architecture rationale, Frontend and screen contracts, Backend representation, API contracts, Security controls, Testing methodology, Operations procedures, Delivery status and plans, and shared engineering conventions. Update the owner and cross-reference it; do not copy domain rules into this file. Keep approved target truth separate from current implementation and delivery status. Preserve verified architectural rationale in its Architecture owner or ADR.

### Bounded execution-ledger exception

The [Frontend implementation ledger](docs/frontend/execution/frontend-implementation-ledger.md) retains a specific `GOAL-FE-001` Task/Gate eligibility and execution-governance role. For work actually authorized under that goal, inspect only the relevant Task, predecessor Gates, approval and blocker rows, and Gate criteria. The ledger does not authorize work by its mere existence, define general Product or Architecture truth, or replace [Delivery current state](docs/delivery/current-state.md). Do not generalize this exception to another ledger without verified active authority.

## Select and use skills

Before execution, discover available installed skills through the current skill catalog or mechanism; do not guess local installation paths. Load `using-superpowers`, evaluate additional skills against the authorized task, and load every applicable required skill before work. Follow their procedures within the human and repository authority boundary. Never claim an inaccessible skill was loaded; stop and report when a mandatory skill cannot be accessed.

| Established trigger | Skill to evaluate and load when applicable |
|---|---|
| Every conversation or task start | `using-superpowers` |
| New feature or architecture discussion | `brainstorming` |
| Unexpected behavior or failing check | `systematic-debugging` |
| Multi-step implementation planning | `writing-plans` |
| Inline execution of an approved implementation plan | `executing-plans` |
| New feature implementation | `test-driven-development` |
| Authorized independent parallel work | `dispatching-parallel-agents` |
| Isolated feature development | `using-git-worktrees` |
| Review requested or consequential work nearing a review gate | `requesting-code-review` |
| Review feedback received | `receiving-code-review` |
| Before any completion or pass claim | `verification-before-completion` |
| Finishing a branch when integration is authorized | `finishing-a-development-branch` |

Applicable skill steps remain mandatory unless they conflict with the explicit task or repository authority. A generic skill cannot authorize extra files, another writer, staging, commits, pushes, dependency changes, or a project decision. Report the skills loaded, why they apply, the project authority read, and the task constraints before implementation. A delegated worker must read this file and establish its own applicable skills and scope before execution.

## Decide from evidence

Inspect before editing. Distinguish explicit human decisions, approved target authority, current implementation evidence, historical evidence, and unresolved questions. Code, tests, configuration, and contracts establish implementation facts; they do not automatically establish intended Product behavior or approved Architecture. Historical text does not become current authority by being copied. Do not invent requirements, business rules, technical policy, or missing facts.

When a material conflict or missing decision cannot be resolved by established authority, stop that decision path and report `HUMAN_DECISION_REQUIRED`: name the exact issue, evidence inspected, conflict or gap, affected owner, supported alternatives, and exact human decision needed. Continue independent authorized work where possible. Never silently choose between equally authoritative sources.

## Work within the task boundary

Execute only authorized work and modify only authorized files. Do not start or prepare the next task, perform opportunistic cleanup, or rewrite a completed owner because an adjacent defect was noticed; report out-of-scope defects. Match the exact scope and access requirement of an assigned endpoint or other protected operation. If a task needs broader scope or an architectural, Product, security, or UX decision, obtain explicit authority rather than guessing. Inspect existing patterns before changing implementation.

Only one writer may edit a shared worktree at a time unless the human explicitly authorizes safely isolated work. Delegation does not transfer decision authority. A read-only reviewer must not repair the reviewed worktree or approve a human-owned decision.

For an explicitly authorized multi-step GOAL, follow its owning Task/Gate eligibility, acceptance, blocker, and review rules. Task/Gate IDs are traceability units, not automatic session, commit, or review boundaries. A bounded task without an authorized enclosing GOAL stops for review when its assigned work is complete. Neither a GOAL nor a skill authorizes unrelated later work.

## Verify, review, and report

Plan the bounded work before implementation; implement in reviewable increments. Run checks appropriate to the changed surface and task risk, read their actual outputs, and distinguish checks run from checks merely available. Test claims must prove the requested behavior, including relevant authorization and sensitive-response boundaries; use the owning [Testing Strategy](docs/testing/testing-strategy.md) and [Security Verification](docs/security/security-verification.md) for methodology. Documentation-only work needs scope, consistency, link, and diff checks, not unrelated application suites.

Before claiming completion, inspect the final diff and Git status, confirm the authorized file scope, and provide the requested file contents and actual command outputs. Do not hide failed checks; repair only within scope, rerun affected checks, and report the final evidence. If asked for full files, print the final contents rather than substituting a summary. Do not claim a build, test, or review passed without its evidence.

Where the current task or Gate requires independent review, give the reviewer bounded scope, source authority, acceptance criteria, protected worktree state, and relevant verification evidence. Reviewers report findings against a stable snapshot and remain read-only unless repair authority is explicit. Resolve supported findings within the task boundary and reverify; review does not create Product, design, security, or human approval.

## Git and handoff safety

Inspect branch, HEAD, and status before consequential work. Preserve unrelated changes and coordinate shared-file writes. Do not stage, commit, push, reset, clean, stash, restore, or otherwise perform destructive/history-changing Git actions without authority for the exact action. Never use `git add .`. Inspect the cached diff before any authorized staging or commit. [PROJECT_RULES.md](PROJECT_RULES.md) owns repository-wide Git constraints.

Update the authoritative documentation when an authorized change materially alters behavior, design, contracts, or status. Do not edit handoff, task, or ledger files unless the task authorizes that edit. Keep any authorized handoff concise, current, and limited to live blockers, protected worktree notes, and the next authorized action; retain historical evidence with its proper owner.

Report what changed, evidence and checks, limitations or unresolved decisions, and Git scope. After a bounded task with no authorized enclosing continuation, end with:

```text
Stopped for review. Do not proceed. Do not commit.
```

For an active GOAL, follow its established review and closure authority; stop at a genuine human decision, security action, or other established gate. Do not announce or begin a separate next task.
