# AI Agent Instructions

## Purpose

This document defines the mandatory operating rules for all AI coding agents contributing to the Nursing Platform project.

These instructions apply to every AI coding assistant contributing to this repository.

The objective is to ensure every contribution is:

- Production-ready
- Architecturally correct
- Fully testable
- Maintainable
- Secure
- Scalable
- Consistent across the entire codebase

These project instructions take precedence over convenience. Prototype implementations, shortcuts, and temporary solutions are not acceptable.

---

# RULE 0: COMPACT HANDOFF & TARGETED READING PROTOCOL

- **Startup:** Read the compact `PROGRESS.md` current-state handoff (target ~1-3 KB). Do NOT read it
  in its entirety as a ritual, and NEVER load `PROGRESS_HISTORY.md` at startup: it is historical,
  append-oriented, read-on-demand evidence, non-authoritative for current execution.
- **Current-state ownership:** The task owner keeps `PROGRESS.md` concise when an authorized
  task requires a handoff update (current work, live blockers, protected worktree notes, next
  authorized action). Completed Task reports, transcripts, and old handoffs belong in the owning
  execution ledger or `PROGRESS_HISTORY.md` as on-demand history, not in `PROGRESS.md`. Prefer one
  durable evidence location plus references.
- **Planning:** Record the authorized batch plan in the owning execution record when required
  before writing code; do not infer authority from a local runtime state file.
- **Progress:** Record minimal per-increment evidence as work proceeds
  (inspect → implement → focused verify → continue) and stop at a coherent batch boundary for
  independent review. Do not emit intermediate human-facing essays for ordinary Low/Medium work.
- **Bounded review:** A reviewer reads only task-relevant context, reports evidence and findings,
  and does not write to the reviewed worktree or `PROGRESS.md` unless the human-authorized task
  explicitly permits a separate edit. Do not bulk-read histories, full ledgers, or unrelated domains.
- **Handoff:** When a handoff update is authorized, leave `PROGRESS.md` compact and current.

---

# Mandatory AI Workflow

This project adopts the Superpowers workflow for structured software development.

Before performing any task, the AI agent must first determine which development skills are applicable.

Implementation must never begin immediately.

The agent must first:

1. Load the `using-superpowers` skill.
2. Determine which additional skills apply.
3. Load those skills.
4. Read the required project documentation.
5. Produce an implementation plan.
6. Implement incrementally.
7. Verify the implementation.
8. Update documentation if necessary.

The AI must never skip applicable skills.

Discover applicable installed skills through the available skill catalog. Do not guess or hard-code installation paths.

Every fresh delegated worker must, before execution:

1. Read `AGENTS.md` first.
2. Discover and evaluate applicable installed skills through the available skill catalog.
3. Load `using-superpowers`.
4. Evaluate additionally applicable skills against this file and the authorized task scope.
5. Load every required skill before execution.
6. Report `SKILLS_EVALUATED`, `SKILLS_LOADED`, and `SKILL_REASONING`.
7. STOP if a required skill cannot actually be accessed; never claim an inaccessible skill was loaded.

Before implementation, the AI must explicitly report:

- Which Superpowers skills were loaded.
- Why each selected skill applies.
- Which project documents were reviewed.
- Which project constraints affect the current task.

Implementation must not begin until this report is complete.

---

# Required Skills

The following Superpowers skills are mandatory whenever their triggering conditions apply.

| Situation | Required Skill |
|------------|----------------|
| Every new conversation | using-superpowers |
| New features or architecture discussions | brainstorming |
| Multi-step implementations | writing-plans |
| Executing an approved implementation plan | executing-plans (inline; project orchestration does not create implementation workers) |
| Independent parallel tasks | dispatching-parallel-agents |
| New feature implementation | test-driven-development |
| Debugging unexpected behavior | systematic-debugging |
| Before requesting merge approval | requesting-code-review |
| After receiving review feedback | receiving-code-review |
| Isolated feature development | using-git-worktrees |
| Before declaring work complete | verification-before-completion |
| Finishing implementation | finishing-a-development-branch |

Not every task requires every skill.

However, the AI must always evaluate whether a skill applies before proceeding.

---

# Skill Selection Rules

Before starting any task, the AI must determine which Superpowers skills apply.

The following order must always be respected:

1. using-superpowers
2. Process skills
   - brainstorming
   - systematic-debugging
3. Planning skills
   - writing-plans
4. Execution skills
   - subagent-driven-development
   - executing-plans
5. Quality skills
   - requesting-code-review
   - receiving-code-review
   - verification-before-completion
6. Branch management
   - finishing-a-development-branch

The AI must never begin implementation before selecting the applicable skills.

Skill instructions do not change repository authority. If a generic skill suggests commits, staging, extra implementation writers, altered review gates, or broader scope, the explicit human task, `PROJECT_RULES.md`, this file, and the owning execution ledger govern. Loading a skill never authorizes another writer. Each coherent batch receives the independent read-only review required by its current Task/Gate authority, with per-Task acceptance mapping; consequential work is reviewed at its own boundary. All applicable non-conflicting skill steps remain mandatory.

---

# Repository Context

This repository contains project documentation, backend code, frontend code, scripts, and infrastructure.

The AI must always treat the repository documentation as the primary source of truth.

Task work follows the explicit human authorization, `PROJECT_RULES.md`, this file, and the relevant owning ledger and domain contracts. Only one writer may edit a shared worktree at a time; independent review is read-only unless separately authorized. The `GOAL-FE-001` Frontend Standing Implementation Authorization remains limited by its current ledger criteria.

When multiple documents exist:

- `docs/*` defines architecture and implementation rules.
- `PROJECT_RULES.md` defines repository-wide constraints.
- `CURRENT_TASK.md` defines the active milestone.
- `TASKS.md` defines the long-term roadmap.
- `README.md` provides project overview.
- `AGENTS.md` defines AI behavior.

Never duplicate documentation.

Always update the authoritative document instead.

---

# Bounded Delegated Review

When a task requires independent review, provide the reviewer with the bounded scope, current source and authority, acceptance criteria, protected worktree state, and relevant verification evidence. The reviewer reports requirement coverage, findings, and actual checks against a stable snapshot; review does not create product, design, security, or closure authority.

Reviewers read `AGENTS.md` and only the task-relevant authority, remain within the authorized scope, and stop on missing evidence or unresolved authority. Do not run concurrent writers against a shared worktree. A reviewer does not repair the work under review or approve a human-owned decision.

---

# Project Overview

Nursing Platform is a production-ready SaaS platform built using modern engineering practices.

## Backend

- .NET 10
- ASP.NET Core Minimal APIs
- Clean Architecture
- CQRS
- MediatR
- Entity Framework Core
- PostgreSQL
- Redis

## Frontend

- Angular 22
- Standalone Components
- Signals
- RxJS
- Angular Material
- Angular CDK where required
- SCSS
- Project-owned Angular Material theme

Frontend agents must follow the styling, theming, and user-feedback rules in `docs/frontend/frontend-architecture.md`.

Frontend agents must consult `docs/frontend/design/frontend-design-foundation-reference.md` before Angular UI, SCSS, Angular Material theme, component, or screen work. Approved visual foundations and task-required approved Penpot/design artifacts own visual intent; Storybook is the intended future production visual development/review surface after separate tooling authorization, not requirements authority. If live Penpot contradicts the reference for a Penpot-required scope, stop for design resolution instead of guessing.

Frontend screen, Stitch, Storybook, and Angular UI agents must also consult `docs/frontend/design/screen-contracts/` before creating, editing, reviewing, or implementing screen-level work. Screen contracts map route IDs, screen IDs, access rules, approved states, blocked/deferred concepts, and design-proposed features. They do not create backend behavior, product authority, visual approval, or implementation authorization. Backend/OpenAPI/security/privacy/accessibility contracts, canonical route/permission source, approved screen packets, `docs/frontend/design/stitch/DESIGN.md`, and human-approved Stitch artifacts remain higher authority. If a screen contract conflicts with those sources, stop and resolve the conflict instead of guessing.

Canonical screen/design source map for future agents:

- Visual authority: `docs/frontend/design/stitch/DESIGN.md`.
- System-level design contract: `docs/frontend/design/stitch/system-design-contract.md`.
- Master screen map: `docs/frontend/design/screen-contracts/screen-index.md`.
- Screen contract schema: `docs/frontend/design/screen-contracts/contract-schema.md`.
- Shared patterns: `docs/frontend/design/screen-contracts/shared-patterns.md`.
- Family contracts: `docs/frontend/design/screen-contracts/authentication.md`, `account.md`, `nurse-profile.md`, `exams.md`, `preparation-packages.md`, `commerce.md`, `employer.md`, `administration.md`, and `shared-system.md`.
- Design-proposed feature register: `docs/frontend/design/stitch/design-proposed-features.md`.
- Stitch artifact mapping: `docs/frontend/design/stitch/stitch-artifact-registry.md`.
- Governance/source authority: `docs/frontend/design/governance/`.

Required future-agent screen workflow: find the target in `screen-index.md`; read the exact family screen section; read `shared-patterns.md`; read `DESIGN.md`; check `system-design-contract.md` when system/flow context is needed; check DPF references; check route, permission, API, and backend authority; stop on unresolved authority gaps; never invent missing business behavior.

Active Stitch v2 mapping: active project `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`); active design system `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`); preferred visual baseline `projects/17116545761229201855/screens/fbcef626cae7450fa6f5cedbdbeae8ea` (`Shell / APP-SHELL / Nurse / Desktop`) is preferred visual direction but not exact as-is human approval. Earlier Stitch project `projects/14739979548635957177` and design system `assets/8866686723686557578` are superseded and are not active authority.

Frontend agents must also enforce the Angular component separation rule in `docs/frontend/frontend-architecture.md` and `docs/frontend/frontend-project-rules.md`. Angular allowing inline templates or inline styles is not project authority to use them. For every new or modified ordinary production Angular component, workers and final-gate review must verify: no inline component `template:`, no inline component `styles:`, no template-local `<style>` block, a colocated external `.html` referenced by `templateUrl`, a colocated external `.scss` referenced by approved external style metadata unless an approved styleless-component exception is recorded, and a focused colocated `.spec.ts` where behavior or rendering is testable. If an exception is not covered by the frontend project rules, stop instead of inventing one.

---

# Primary Objective

Every implementation must be:

- Production-ready
- Maintainable
- Testable
- Secure
- Scalable

Never optimize for development speed at the expense of architecture or maintainability.

---

# Required Context Loading

Select context from the explicit human task, `PROJECT_RULES.md`, this file, `PROGRESS.md`, the owning execution ledger, and the affected domain contracts and source. A reviewer reads `AGENTS.md` first, then only the authority needed for the bounded review.

Prefer targeted reads over whole-document ingestion: at task start or resumption, load the explicit human request, compact entry rules and `PROGRESS.md` handoff, and current Git branch/HEAD/status; then read only the relevant Task/Gate/predecessors, architecture, screen or family contract, approved visual authority, and affected source/tests. Do not bulk-read full ledgers, entire historical design programs, every screen contract, all OpenAPI JSON, or `PROGRESS_HISTORY.md` unless the task truly requires them.

Historical documents (`PROGRESS_HISTORY.md`, old `GOAL_STATE.md`/`MASTER_PLAN.md` design-program files, superseded review records) are on-demand evidence only and never create current STOP conditions merely because old text says "next". If the required authority cannot be safely identified or listed sources conflict without a resolvable precedence rule, STOP and report the blocker rather than guessing.

---

# Implementation Workflow

For every task:

1. Determine the applicable Superpowers skills.
2. Read `CURRENT_TASK.md`.
3. Identify the affected modules.
4. Review all relevant documentation.
5. Produce an implementation plan.
6. Request approval if architectural changes are required.
7. Implement incrementally.
8. Build the solution.
9. Run all applicable tests.
10. Update documentation when implementation changes behavior or architecture.
11. Perform final verification before declaring completion.

If requirements are unclear:

- Stop implementation.
- Explain the ambiguity.
- Request clarification.

Never invent requirements.

---

# Architecture Rules

Always respect Clean Architecture.

Dependency direction:

```text
WebApi
    ↓
Application
    ↓
Domain

Infrastructure
    ↓
Application
    ↓
Domain
```

Mandatory rules:

- Domain must never depend on Infrastructure.
- Domain must never depend on WebApi.
- Infrastructure implements interfaces defined by the Application layer.
- Business logic belongs only in Application and Domain.
- Presentation must remain thin.
- Keep dependencies flowing inward.

Never violate dependency direction.

---

# Coding Standards

Generated code must be:

- Small
- Modular
- Readable
- Strongly typed
- SOLID compliant
- Easy to test
- Easy to maintain

Prefer:

- Composition over inheritance
- Explicit code over clever code
- Dependency injection
- Immutable objects where practical

Avoid:

- God classes
- Duplicate code
- Tight coupling
- Static mutable state
- Magic strings
- Premature optimization
- Hidden side effects

---

# Database Rules

Always use:

- Entity Framework Core
- Code-First
- EF Core Migrations

Never:

- Modify the schema manually.
- Bypass DbContext.
- Execute ad-hoc schema changes.
- Couple business logic to persistence details.

---

# API Rules

Always:

- Return DTOs.
- Validate every request.
- Use consistent HTTP status codes.
- Return Problem Details for errors.
- Follow the API design document.

Never expose:

- Domain entities
- Database entities
- Internal implementation details
- Password hashes
- Secrets
- Internal authorization state not intended for the API response

---

# Testing Policy

Testing is mandatory.

Every significant feature should include appropriate automated tests.

Business-critical logic must always be testable.

Pay particular attention to:

- Business workflows
- Validation rules
- Authentication
- Authorization
- Examination scoring
- Financial calculations
- Edge cases
- Pagination
- Filtering and sorting
- Sensitive-field exposure

Whenever practical, follow Test-Driven Development:

1. Write a failing test.
2. Implement the smallest possible solution.
3. Refactor while keeping tests green.

Never leave failing tests.

---

# Verification Policy

Never claim work is complete without verification.

Before declaring completion, the AI must:

- Build the solution where a build exists for the changed surface.
- Run focused verification for each changed testable behavior, plus the broader
  build/integration/full checks appropriate to the combined batch risk (risk-based, not
  full-regression-after-every-micro-Task; documentation/classification-only work needs no
  unrelated product suites).
- Review generated changes.
- Verify documentation.
- Confirm no unrelated files were modified.
- Confirm no files were staged and no commit was made unless the active GOAL explicitly
  authorizes that exact action.

Never assume success.

Always verify.

---

# Documentation Policy

Documentation is part of the implementation.

Whenever behavior, architecture, APIs, workflows, or design changes:

- Update the relevant documentation when explicitly required by the task.
- Keep markdown files synchronized with implementation.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.

Documentation must never become outdated.

---

# Communication Guidelines

When presenting work:

- Explain assumptions.
- Describe important trade-offs.
- Mention risks when applicable.
- Distinguish facts from assumptions.
- Keep explanations concise.
- Provide evidence, not only claims.

Never fabricate requirements.

Never guess business rules.

---

# Git Rules

Every commit must:

- Represent one logical change.
- Build successfully.
- Keep documentation synchronized.
- Use meaningful commit messages.

Never commit:

- Secrets
- Credentials
- Temporary code
- Generated artifacts
- Experimental code
- Debug-only changes
- Personal notes such as `TODO`, unless explicitly instructed

Never use:

```bash
git add .
```

Use explicit paths only when staging is explicitly approved.

---

# Definition of Done

A task is complete only when:

- The implementation is production-ready.
- Clean Architecture is respected.
- Engineering standards are followed.
- The solution builds successfully.
- Applicable tests pass.
- Documentation has been updated when required.
- Final verification has been completed.
- All applicable Superpowers skills have been followed.
- The full requested evidence has been pasted for review.
- The stable batch snapshot has the independent review required by its owning Task/Gate,
  with per-Task acceptance mapping, before governance-authorized closure.

---

# Agent Operating Rules — Nursing Platform

These rules exist because this project is executed in a strictly reviewed, task-by-task workflow. They are mandatory.

## 1. Task Boundary Rules

- Execute only the task explicitly assigned.
- Do not start, prepare, scaffold, or partially implement the next task.
- Do not modify files outside the requested scope.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.
- Do not modify endpoint groups that belong to later tasks.
- Do not commit unless the active GOAL explicitly authorizes the exact commit action.
- Do not stage files unless the active GOAL explicitly authorizes the exact staging action.
- Task/Gate IDs are traceability units, not automatic session/review/commit boundaries:
  related eligible Low/Medium Tasks may execute as one coherent batch under one authorized GOAL
  subject to `PROJECT_RULES.md` and the owning ledger.
- Never use `git add .`.

## 2. Stop-for-Review Rule

When the assigned task is complete:

- A bounded task without an explicitly authorized enclosing GOAL stops for review.
- The owner of an explicitly authorized multi-step GOAL continues through required independent review, bounded corrections, rereview, and governance-authorized closure until a terminal GOAL status.
- A GOAL does not authorize beginning a separate later roadmap task or expanding into unrelated scope.
- Do not suggest that a separate next task is starting.
- Do not write speculative next-step implementation notes.
- When no enclosing GOAL owns another authorized lifecycle step, end with this exact status sentence:

```text
Stopped for review. Do not proceed. Do not commit.
```

Do not write misleading phrases like:

- “Awaiting Task 5” if Task 5 was not assigned yet.
- “Next: implementing users endpoint” unless explicitly instructed.
- “Ready to continue” without review.
- “I will now proceed” after completing the task.

For an active GOAL, keep intermediate evidence in the owning execution record and end only at `COMPLETE`, `BLOCKED`, `HUMAN_DECISION_REQUIRED`, or `SECURITY_ACTION_REQUIRED`.

## 3. Full File Output Rule

When asked to paste full file contents, paste the actual full file contents using `cat`.

Summaries are not acceptable substitutes.

Bad:

- “File pasted above.”
- “See Read tool output.”
- “Key changes are...”
- “The file contains...”
- “The important section is...”
- “Lines 40-60 show the change.”

Good:

```bash
cat path/to/file.cs
```

Then paste the full command output.

If a file was corrected after a build error, paste the final corrected full file, not the earlier failed version.

## 4. Command Output Rule

When asked to run build, tests, or git status, paste the real command and the real output.

Required format:

```bash
dotnet build backend/NursingPlatform.slnx
```

Then paste the real output.

```bash
dotnet test path/or/project --filter "SomeFilter"
```

Then paste the real output.

```bash
git status --short
```

Then paste the real output.

Do not replace command output with only:

- “Build passed.”
- “Tests passed.”
- “Only untracked files.”
- “No issues.”

A short summary may be included after the real output.

## 5. Evidence Before Approval Rule

A task is not complete until the reviewer receives all requested evidence:

- Full contents of all requested created files.
- Full contents of all requested modified files.
- Build output.
- Test output.
- `git status --short` output.
- Confirmation that no commit was made.
- Confirmation that no files were staged unless explicitly instructed.
- Confirmation that no out-of-scope files were changed.

If any requested item is missing, the task remains pending review.

## 6. No Summary-Only Completion Rule

A completion message that only says the following is not enough:

- Files were created.
- Tests passed.
- Build passed.
- Git status is acceptable.
- Key changes were made.

Always include the requested evidence.

## 7. Final Corrected File Rule

If the first build or test run fails:

1. Paste the failure.
2. Fix only the relevant issue.
3. Re-run the required build/tests.
4. Paste the final corrected full files.
5. Paste final command outputs.
6. Stop for review.

Do not hide the failure.

Do not paste only snippets after fixing.

Do not say “already shown above” instead of printing the final file again.

## 8. Test Name Accuracy Rule

Test names must match what the test actually proves.

Bad:

```csharp
public async Task Handle_DuplicateRoles_AreDeDuplicated()
```

if the test uses two different role names:

```text
Admin
Nurse
```

That proves sorting or multiple-role handling, not de-duplication.

Good:

```csharp
public async Task Handle_MultipleRoles_AreSortedDeterministically()
public async Task Handle_DuplicateRoleNames_AreDeDuplicated()
```

Every required behavior must be proven with explicit assertions.

## 9. Pagination Test Rule

Pagination tests must prove both `Skip` and `Take`.

Do not only use:

```csharp
Page = 1
PageSize = 10
```

That does not prove skipping.

A proper pagination test should:

- Use enough records to span multiple pages.
- Request a page after the first page, such as `Page = 2`.
- Use deterministic sorting.
- Assert `TotalCount`.
- Assert `TotalPages`.
- Assert `Page`.
- Assert `PageSize`.
- Assert item count.
- Assert the first and last returned items.

## 10. Security Response Rule

When testing that a response must not expose sensitive fields, inspect the raw JSON response.

Deserializing into a DTO is not enough because deserialization can ignore unexpected fields.

Required pattern:

```csharp
var json = await response.Content.ReadAsStringAsync();

Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);

var body = JsonSerializer.Deserialize<ResponseDto>(
    json,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
```

Apply this rule to fields such as:

- `passwordHash`
- secrets
- internal tokens
- internal authorization state
- any field explicitly marked as not exposed

## 11. Endpoint Scope Rule

Respect endpoint task boundaries exactly.

For every endpoint task:

- Implement only the endpoints explicitly assigned in the current task.
- Do not add endpoints from later tasks.
- Do not modify unrelated endpoint groups.
- Do not combine separate endpoint tasks unless explicitly instructed.
- Follow the auth/permission requirement exactly as stated in the task.

If the endpoint scope is unclear, stop and ask for clarification before modifying endpoint mappings.

## 12. Authorization Rule

Use the exact authorization requirement from the task and existing project patterns.

- If the task says `.AllowAnonymous()`, use anonymous access for that endpoint.
- If the task says `.RequireAuthorization()`, require authentication only and do not add permission requirements.
- If the task says `.RequirePermission(...)`, use the exact permission specified.
- Do not substitute a different permission.
- Do not add stricter or looser authorization than the approved task requires.

Endpoint tests must prove the required auth behavior for the assigned endpoint:

- Anonymous endpoints: success without JWT when the request is valid.
- Authenticated endpoints: 401 without JWT, success with JWT when the request is valid.
- Permission-protected endpoints: 401 unauthenticated, 403 authenticated without permission, 200 authorized when the request is valid.

## 13. Query and Handler Rule

Application handlers must:

- Use existing project patterns.
- Throw existing mapped exception types.
- Avoid exposing sensitive properties.
- De-duplicate roles and permissions where required.
- Sort collections deterministically where required.
- Keep projection explicit.
- Avoid endpoint implementation inside Application tasks.
- Avoid WebApi dependencies inside Application.

## 14. Validator and Handler Responsibility Rule

If the validator rejects a value, the handler does not need to silently fix or cap it unless explicitly required.

Example:

If the validator rejects:

```text
PageSize > 100
```

then the handler should not cap `PageSize` to `100` unless the task explicitly says to do so.

## 15. DTO Exposure Rule

API and Application DTOs must not expose:

- `PasswordHash`
- internal tokens
- infrastructure details
- persistence-only fields
- domain entities
- navigation entities

List DTOs must not include detail-only fields unless explicitly required.

Example:

- `UserListItemDto` may include roles.
- `UserListItemDto` must not include permissions unless explicitly required.
- `UserDetailDto` may include roles and permissions when required.

## 16. Permission Service Test Rule

For endpoints protected by `RequirePermission(...)`:

- Tests must include unauthenticated `401`.
- Tests must include authenticated-but-forbidden `403`.
- Tests must include authenticated-and-authorized `200`.
- Tests must configure the permission service for the exact permission required.
- Tests must not configure unrelated permissions.

For endpoints protected only by `.RequireAuthorization()`:

- Do not require permission service setup.
- Include a test proving no permission setup is needed when requested.

## 17. Git Hygiene Rule

At the end of every task, run:

```bash
git status --short
```

Expected during uncommitted task work:

- New task files may be untracked.
- Modified task files may appear.
- No staged files.
- No commit unless explicitly instructed.

Never touch personal or unrelated files such as:

```text
TODO
```

unless explicitly instructed.

## 18. Out-of-Scope Change Rule

If the task requires changing a file that could affect unrelated behavior, make the smallest possible change.

Do not refactor unrelated code.

Do not reorganize files.

Do not rename unrelated classes.

Do not “clean up” unrelated code while performing the assigned task.

## 19. Existing Pattern Rule

Before adding new implementation or tests, inspect existing patterns and follow them.

Examples:

- Existing endpoint mapping style.
- Existing JWT helper style in tests.
- Existing permission test setup.
- Existing exception mapping.
- Existing validator test style.
- Existing mock DbSet style.

Do not invent a different pattern unless the task explicitly requires it.

## 20. Reviewer Correction Rule

When the reviewer asks for a correction:

- Apply only the requested correction.
- Re-run the requested verification.
- Paste the requested full files and outputs.
- Do not argue that a summary is enough.
- Do not proceed to the next task.
- Do not commit.

---

# Strict Task Prompt Header

When receiving a task from the reviewer, treat the following rules as always active even if they are not repeated:

```text
Strict execution rules:
- Execute only this task.
- Do not proceed to the next task.
- Do not commit.
- Do not stage files unless explicitly instructed.
- Never use git add .
- Do not modify CURRENT_TASK.md or TASKS.md unless explicitly instructed.
- Do not modify files outside the requested scope.
- Paste full file contents, not summaries, when requested.
- Paste real build/test/git outputs, not summaries.
- If you fix a file after an error, paste the final corrected full file.
- Stop for review.
```

---

# Final Goal

Every contribution should move the Nursing Platform closer to a production-ready SaaS platform while preserving:

- Architectural integrity
- Code quality
- Maintainability
- Scalability
- Security
- Developer experience

When in doubt, prioritize correctness, maintainability, and long-term quality over implementation speed.
