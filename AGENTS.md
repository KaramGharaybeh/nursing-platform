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

- docs/* defines architecture and implementation rules.
- PROJECT_RULES.md defines repository-wide constraints.
- CURRENT_TASK.md defines the active milestone.
- TASKS.md defines the long-term roadmap.
- README.md provides project overview.
- AGENTS.md defines AI behavior.

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

1. docs/product/vision.md
2. docs/architecture/system-architecture.md
3. docs/backend/backend-architecture.md
4. docs/frontend/frontend-architecture.md
5. docs/database/database-design.md
6. docs/api/api-design.md
7. docs/standards/engineering-standards.md
8. PROJECT_RULES.md
9. CURRENT_TASK.md

Implementation must not begin until these documents are understood.

---

# Implementation Workflow

For every task:

1. Determine the applicable Superpowers skills.
2. Read CURRENT_TASK.md.
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

```
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

---

# Testing Policy

Testing is mandatory.

Every significant feature should include appropriate automated tests.

Business-critical logic must always be testable.

Pay particular attention to:

- Business workflows
- Validation rules
- Authorization
- Examination scoring
- Financial calculations
- Edge cases

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

Never assume success.

Always verify.

---

# Documentation Policy

Documentation is part of the implementation.

Whenever behavior, architecture, APIs, workflows, or design changes:

- Update the relevant documentation.
- Keep markdown files synchronized with implementation.

Documentation must never become outdated.

---

# Communication Guidelines

When presenting work:

- Explain assumptions.
- Describe important trade-offs.
- Mention risks when applicable.
- Distinguish facts from assumptions.
- Keep explanations concise.

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

---

# Definition of Done

A task is complete only when:

- The implementation is production-ready.
- Clean Architecture is respected.
- Engineering standards are followed.
- The solution builds successfully.
- Applicable tests pass.
- Documentation has been updated.
- Final verification has been completed.
- All applicable Superpowers skills have been followed.

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