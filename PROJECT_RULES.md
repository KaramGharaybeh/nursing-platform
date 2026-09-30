# Project Rules

## Purpose

This document defines the mandatory repository-wide rules that every contributor, including AI coding agents, must follow.

These rules apply to all source code, documentation, infrastructure, configuration, and future development.

When a more detailed rule exists inside the documentation under `/docs`, that document becomes the authoritative reference (Single Source of Truth).

---

# Core Principles

Every implementation must prioritize:

- Correctness
- Maintainability
- Scalability
- Security
- Simplicity
- Readability
- Testability

The project follows these engineering principles:

- SOLID
- DRY
- KISS
- YAGNI

Prefer simple and maintainable solutions.

Avoid premature optimization.

Prototype-quality code is never acceptable.

---

# Architecture Rules

The backend must follow Clean Architecture.

Allowed layers:

- Domain
- Application
- Infrastructure
- Presentation (Web API)

Dependencies must always point inward.

Business rules must never depend on infrastructure or presentation.

Architectural decisions must follow the documentation in:

- docs/architecture/system-architecture.md
- docs/backend/backend-architecture.md

---

# Protected Product Truth

Backend/API contracts and server-confirmed state are authoritative for authentication, authorization, permissions, ownership, payment or purchase success, fulfillment, entitlements and access rights, official exam scoring and results, and protected user or candidate data. Frontend state, mocks, fixtures, stubs, sandboxes, and Development/Test substitutes must not establish these protected facts as production truth.

Development/Test substitutes may be used only behind the appropriate production-facing boundary or interface and must be replaceable by the production implementation. Production must fail closed if its real protected-state authority is unavailable; it must never silently fall back to a substitute. Detailed domain rules remain with the owning backend, API, security, and frontend documents.

---

# Documentation Authority

Project documentation is part of the codebase.

Each document has a single responsibility.

The documentation inside `/docs` is the authoritative source for its respective topic.

Do not duplicate detailed technical guidance across multiple documents.

Whenever implementation changes architecture, behavior, or development workflow:

- Update the relevant documentation.
- Keep documentation synchronized with implementation.
- Never allow documentation to become outdated.

Documentation should always describe the current implementation.

Architectural, business, design, API, security, or exception decisions that affect future implementation must be persisted in repository-backed documentation. Chat history alone is not a durable project decision record.

---

# Engineering Standards

All source code must follow the standards defined in:

- docs/standards/engineering-standards.md

That document defines:

- Coding conventions
- Naming conventions
- Validation
- DTO usage
- Error handling
- Logging
- Testing
- Database practices
- API design guidelines

Do not redefine those standards in this document.

---

# Development Workflow

Before implementing any feature:

1. Read CURRENT_TASK.md.
2. Read AGENTS.md.
3. Read the relevant documentation inside `/docs`.
4. Understand the affected architecture.
5. Implement the smallest complete change.
6. Verify the solution builds successfully.
7. Run the applicable tests.
8. Update documentation if necessary.

---

# Git Rules

Every commit must:

- Represent one logical change.
- Build successfully.
- Avoid unrelated modifications.
- Use meaningful commit messages.

Never commit:

- Secrets
- Credentials
- Generated build artifacts
- Broken code

Keep commits focused and easy to review.

Before editing a dirty working tree, identify pre-existing tracked and untracked changes. Preserve unrelated hunks, coordinate writes to shared files, and inspect the final diff before staging or committing. Scope each change to the authorized task; staging, committing, and pushing require their own authorization.

Map acceptance claims to source, tests, and observed results. Choose focused and broader verification according to the risk and reach of the change; documentation-only work requires scope, consistency, and diff checks rather than unrelated product suites. Obtain review independent of the author where consequential correctness or an approved gate requires it. A reviewer reports evidence and findings; review does not create product, security, or design authority. Correct findings within the authorized scope and reverify before claiming completion.

Material review findings must identify the affected requirement and file or symbol, explain the impact, state the needed correction, and cite supporting evidence.

Reuse earlier verification evidence only when the checked source, dependencies, configuration, command environment, and contracts are unchanged. Otherwise rerun the affected checks before relying on that evidence.

---

# AI Agent Compliance

All AI coding agents must follow:

- AGENTS.md
- CURRENT_TASK.md
- All relevant documentation inside `/docs`

AI agents must never:

- Invent missing requirements.
- Ignore documented architecture.
- Introduce unnecessary abstractions.
- Modify unrelated files.
- Generate prototype-quality code.

When requirements are ambiguous, implementation must stop until clarification is provided.

---

# Project Scope

Only implement features that belong to the current project milestone.

The active milestone is always defined in:

- CURRENT_TASK.md

Features outside the current milestone must not be implemented.

Every implementation task has an authorized scope. Agents may modify only the files/modules explicitly listed by the task plus directly necessary dependency files. Discovery of a desirable broader refactor does not authorize that refactor; stop for explicit approval before widening scope.

Do not combine unrelated cleanup, modernization, dependency upgrades, formatting migrations, or architecture changes with a feature task.

Once a task or feature reaches VERIFIED status, later tasks must not refactor, redesign, rename, move, or behaviorally modify that completed scope unless the new task explicitly declares it as an affected dependency or REOPENED scope. No opportunistic cleanup of verified features is allowed.
---
# AI Development Rules

All AI coding assistants must follow the workflow defined in `AGENTS.md`.

Before implementation:

- Read the required project documentation.
- Select the applicable Superpowers skills.
- Follow the documented implementation workflow.
- Respect the project architecture and engineering standards.

`AGENTS.md` is the authoritative document for AI behavior.

Project documentation defines what should be built, while AI skills define how implementation should be carried out.

Generic skills, including design, brand, design-system, ui-styling, ui-ux-pro-max, brainstorming, or similar capabilities, are subordinate to Nursing Platform repository governance, approved design evidence, backend/API contracts, business rules, security, accessibility, and task scope. A generic skill must never silently introduce Tailwind, shadcn, a second UI library, new design tokens, new business behavior, new architecture, or a conflicting visual decision.

Before retiring a development tool, inventory project-used Skill content and MCP capabilities, including locally ignored files. Preserve the capabilities that remain authorized and their project authority boundaries before removing the hosting runtime. Tool availability alone never grants implementation or design authority.
---

# Definition of Quality

A change is considered complete only when:

- The solution builds successfully.
- Clean Architecture is respected.
- Engineering standards are followed.
- Documentation is updated when necessary.
- Tests pass when applicable.
- No unnecessary complexity has been introduced.
- The implementation is production-ready.

---

# Memory Bank Governance

* `/PROGRESS.md` is the compact current-state handoff (active GOAL, live blockers, protected
  worktree notes, next authorized action; target ~1-3 KB). It is not historical storage.
* `PROGRESS_HISTORY.md` is the append-oriented historical record: non-authoritative,
  read-on-demand only, never mandatory startup context.
* Agents use targeted reads (explicit task, compact entry requirements, compact handoff, owning
  execution record when relevant, branch/HEAD/status, then only task-relevant authority) instead of ingesting whole
  ledgers, histories, or unrelated domain documents.
* Related eligible Low/Medium Tasks may execute as one coherent batch under one authorized GOAL
  with independent read-only review at the batch boundary; Task/Gate IDs remain the traceability
  units. Split the batch when risk materially changes, a protected trust boundary or distinct
  authority is reached, the diff is no longer independently reviewable, one branch is blocked
  while another can continue, or a human decision is required.
* A blocked task stops only dependent work. An independently eligible branch may continue only
  within its own authorization and gates; an external production-only blocker still blocks
  release claims.
* Broad staging (`git add .`) is prohibited. Exact-scope staging/committing is allowed only when
  the active GOAL explicitly authorizes that exact action; push remains separately authorized
  (default: prohibited).
