# AI Agent Instructions

## Purpose

This document defines the mandatory operating rules for all AI coding agents contributing to the Nursing Platform project.

These instructions apply to OpenCode, BigPickle, ChatGPT, Claude, Gemini, Codex, and any future AI coding assistant.

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
| Executing an approved implementation plan | subagent-driven-development (preferred) or executing-plans |
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

---

# Repository Context

This repository contains project documentation, backend code, frontend code, scripts, and infrastructure.

The AI must always treat the repository documentation as the primary source of truth.

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
- Tailwind CSS

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

# Required Reading Order

Before implementing any feature, the AI must review the following documents in order.

1. `docs/product/vision.md`
2. `docs/architecture/system-architecture.md`
3. `docs/backend/backend-architecture.md`
4. `docs/frontend/frontend-architecture.md`
5. `docs/database/database-design.md`
6. `docs/api/api-design.md`
7. `docs/standards/engineering-standards.md`
8. `PROJECT_RULES.md`
9. `CURRENT_TASK.md`

Implementation must not begin until these documents are understood.

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

- Build the solution.
- Run all relevant tests.
- Review generated changes.
- Verify documentation.
- Confirm no unrelated files were modified.
- Confirm no files were staged unless explicitly instructed.
- Confirm no commit was made unless explicitly instructed.

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
- The reviewer has approved the task.

---

# Agent Operating Rules — Nursing Platform

These rules exist because this project is executed in a strictly reviewed, task-by-task workflow. They are mandatory.

## 1. Task Boundary Rules

- Execute only the task explicitly assigned.
- Do not start, prepare, scaffold, or partially implement the next task.
- Do not modify files outside the requested scope.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless explicitly instructed.
- Do not modify endpoint groups that belong to later tasks.
- Do not commit unless explicitly instructed.
- Do not stage files unless explicitly instructed.
- Never use `git add .`.

## 2. Stop-for-Review Rule

When the assigned task is complete:

- Stop immediately.
- Do not proceed to the next task.
- Do not suggest that you are starting the next task.
- Do not write speculative next-step implementation notes.
- End with this exact status sentence:

```text
Stopped for review. Do not proceed. Do not commit.
```

Do not write misleading phrases like:

- “Awaiting Task 5” if Task 5 was not assigned yet.
- “Next: implementing users endpoint” unless explicitly instructed.
- “Ready to continue” without review.
- “I will now proceed” after completing the task.

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
