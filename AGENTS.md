# AI Agent Instructions

## Purpose

This document defines how AI coding agents (OpenCode, BigPickle, GPT, Claude, or any future coding assistant) must operate while contributing to the Nursing Platform project.

The objective is to ensure every contribution is production-ready, consistent with the project architecture, and aligned with long-term maintainability.

---

# Project Overview

Nursing Platform is a production-ready SaaS application built using:

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

Prototype implementations, shortcuts, and temporary solutions are not acceptable.

---

# Required Reading Order

Before implementing any feature, always review the following documents in order:

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

1. Read CURRENT_TASK.md.
2. Identify the affected modules.
3. Review the relevant documentation.
4. Explain the implementation plan.
5. Request approval if architectural changes are required.
6. Implement incrementally.
7. Verify the solution builds successfully.
8. Run applicable tests.
9. Update documentation if implementation changes behavior or architecture.

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

Rules:

- Domain must never depend on Infrastructure.
- Domain must never depend on WebApi.
- Business logic belongs in the Application and Domain layers.
- Infrastructure implements interfaces defined by the Application layer.
- Presentation must remain thin.

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

Avoid:

- God classes
- Duplicate code
- Magic strings
- Static mutable state
- Tight coupling
- Premature optimization

---

# Database Rules

Always use:

- Entity Framework Core
- Code-First
- EF Core Migrations

Never:

- Modify the database schema manually.
- Bypass DbContext.
- Execute ad-hoc schema changes in production.

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

---

# Testing

Every significant feature should include appropriate tests.

Business-critical logic must always be testable.

Particular attention should be given to:

- Business workflows
- Financial calculations
- Examination scoring
- Validation logic

Do not leave failing tests.

---

# Documentation

Documentation is part of the implementation.

Whenever behavior or architecture changes:

- Update the relevant documentation.
- Keep markdown files synchronized with the implementation.

Never allow documentation to become outdated.

---

# Communication

When presenting work:

- Explain assumptions.
- Mention important trade-offs.
- Highlight risks when applicable.
- Keep explanations concise.

If requirements are unclear:

- Stop implementation.
- Explain the ambiguity.
- Request clarification.

Never invent requirements.

---

# Git Rules

Every commit should:

- Represent one logical change.
- Build successfully.
- Avoid unrelated modifications.
- Use meaningful commit messages.

Never commit:

- Secrets
- Credentials
- Temporary code
- Generated build artifacts

---

# Definition of Done

A task is complete only when:

- The solution builds successfully.
- Applicable tests pass.
- Clean Architecture is respected.
- Engineering standards are followed.
- Documentation is updated when necessary.
- The implementation is production-ready.

---

# Final Goal

Every contribution should move the Nursing Platform closer to a production-ready SaaS platform while preserving:

- Architectural integrity
- Code quality
- Maintainability
- Scalability
- Security
- Developer experience