# Nursing Platform

## Overview

Nursing Platform supports nursing licensing-examination preparation and healthcare recruitment. Approved scope is owned by [Product Overview](docs/product/product-overview.md); current implementation and operational gaps are owned by [Delivery Current State](docs/delivery/current-state.md).

---

# Technology Stack

## Backend

- .NET 10
- ASP.NET Core Minimal APIs
- Clean Architecture
- CQRS
- MediatR
- Entity Framework Core
- PostgreSQL
- Redis
- FluentValidation

## Frontend

- Angular 22
- Standalone Components
- Angular Signals
- RxJS
- Angular Material
- SCSS

## Infrastructure

- Docker
- Docker Compose
- PostgreSQL
- Redis
- MailPit

---

# Repository Structure

```
nursing-platform/

├── backend/
├── frontend/
├── docs/
├── scripts/
├── .github/
│
├── AGENTS.md
├── CURRENT_TASK.md
├── PROJECT_RULES.md
└── README.md
```

---

# Quick Start

Use the [Deployment Runbook](docs/operations/deployment-runbook.md) for the verified local backend and frontend startup procedure and its prerequisites. The repository does not currently contain a runnable Docker Compose deployment procedure. Current implementation and operational status are owned by [Delivery Current State](docs/delivery/current-state.md).

---

# Documentation

Project documentation is located in the `docs/` directory.

Start with:

| Document | Purpose |
|----------|---------|
| [docs/index.md](docs/index.md) | Documentation entry point and navigation |

Core documentation:

| Document | Purpose |
|----------|---------|
| [docs/product/product-overview.md](docs/product/product-overview.md) | Product purpose and scope |
| [docs/architecture/architecture-overview.md](docs/architecture/architecture-overview.md) | Approved system architecture |
| [docs/backend/backend-architecture.md](docs/backend/backend-architecture.md) | Backend implementation architecture |
| [docs/frontend/frontend-architecture.md](docs/frontend/frontend-architecture.md) | Frontend implementation architecture |
| [docs/backend/storage-and-database.md](docs/backend/storage-and-database.md) | Storage and database implementation |
| [docs/api/openapi.yaml](docs/api/openapi.yaml) | Current HTTP contract |
| [docs/api/api-guidelines-and-errors.md](docs/api/api-guidelines-and-errors.md) | API conventions and errors |
| [docs/standards/engineering-standards.md](docs/standards/engineering-standards.md) | Engineering standards |
| [docs/operations/environments.md](docs/operations/environments.md) | Verified environment boundaries |
| [docs/operations/deployment-runbook.md](docs/operations/deployment-runbook.md) | Verified local deployment procedure |

---

# Development Workflow

Before implementing any feature:

1. Read `AGENTS.md`.
2. Read `PROJECT_RULES.md`.
3. Read `docs/index.md`.
4. Read `docs/delivery/current-state.md` when current implementation matters.
5. Read only the task-specific owners routed by `docs/index.md`.

Authorization and bounded task rules are owned by `AGENTS.md` and `PROJECT_RULES.md`.
---

# Current Status

Current repository implementation and verified gaps are owned by [docs/delivery/current-state.md](docs/delivery/current-state.md). Historical test totals and milestone statements in Git history do not replace that snapshot.

---

# Development Principles

Shared conventions are owned by [Engineering Standards](docs/standards/engineering-standards.md). Architecture and domain-specific implementation rules are routed through [docs/index.md](docs/index.md).

---

# Contributing

Before submitting changes:

- Ensure the solution builds successfully.
- Run applicable tests.
- Keep documentation synchronized with implementation.
- Follow the standards defined in `PROJECT_RULES.md` and `docs/standards/engineering-standards.md`.

---

# License

This project is licensed under a private license.
---

# AI Development Workflow

This project can be developed with AI coding assistants that follow the current project rules and task-specific authorities.

Before implementing any feature, AI agents must follow the workflow defined in:

- AGENTS.md
- PROJECT_RULES.md
- docs/index.md
- docs/delivery/current-state.md when current implementation matters
- the relevant `CURRENT_TASK.md` scope only when repository governance requires it for bounded feature work

## Superpowers

The development workflow integrates the Superpowers skill system. Agents discover and load applicable installed skills through the available skill mechanism as `AGENTS.md` requires; repository and human authority remain controlling.

Typical workflow:

1. Select applicable skills.
2. Review project documentation.
3. Create or review the implementation plan.
4. Implement incrementally.
5. Verify the implementation.
6. Perform code review.
7. Complete the development branch.

The project documentation remains the single source of truth. Superpowers skills define *how* implementation is performed, while the project documentation defines *what* should be built.
