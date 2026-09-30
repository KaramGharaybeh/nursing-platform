# Documentation Index

## Purpose

This document serves as the entry point to the Nursing Platform documentation.

The documentation is organized by topic, with each document acting as the authoritative source for its respective area. Contributors and AI coding agents should consult the relevant documentation before making implementation decisions.

---

# Reading Order

When starting work on the project, use targeted reads instead of ingesting every document:
load the explicit human task, `PROJECT_RULES.md`, `AGENTS.md`, the compact `PROGRESS.md` handoff,
and current Git branch/HEAD/status; then read only the task-relevant authority from this index
and the owning execution ledger. The historical record
(`PROGRESS_HISTORY.md`, old design-program files, superseded packets) is on-demand evidence only.

The topic map below identifies current authorities and explicitly labeled historical references:

1. Product Vision
2. System Architecture
3. Backend Architecture
4. Frontend Architecture
5. Database Design
6. API Design
7. Engineering Standards
8. Development Guide
9. Deployment Guide

---

# Documentation Structure

## Product

### vision.md

Defines the product vision, business goals, target users, and long-term roadmap.

Location:

```
docs/product/vision.md
```

---

## Architecture

### system-architecture.md

Provides the high-level architecture of the platform, system boundaries, modules, and overall design principles.

Location:

```
docs/architecture/system-architecture.md
```

---

## Backend

### backend-architecture.md

Defines the backend architecture, Clean Architecture implementation, project structure, dependency rules, and application organization.

Location:

```
docs/backend/backend-architecture.md
```

### execution/backend-implementation-ledger.md

Defines backend Task → Subtask → Verification Gate ownership and execution status for backend implementation work.

Location:

```
docs/backend/execution/backend-implementation-ledger.md
```

---

## Frontend

### frontend-architecture.md

Defines the Angular application architecture, feature organization, state management, routing, and frontend design principles.

Location:

```
docs/frontend/frontend-architecture.md
```

---

## Database

### database-design.md

Defines the database architecture, persistence strategy, entity design principles, naming conventions, migrations, and performance considerations.

Location:

```
docs/database/database-design.md
```

---

## API

### api-design.md

Defines REST API conventions, endpoint design, versioning, validation, authentication, response models, and error handling.

Location:

```
docs/api/api-design.md
```

---

## Standards

### engineering-standards.md

Defines the mandatory engineering standards for coding style, architecture, testing, logging, validation, security, and documentation.

Location:

```
docs/standards/engineering-standards.md
```

---

## Development

### development-guide.md

Explains how to set up the local development environment, build the solution, run the application, and follow the recommended development workflow.

Location:

```
docs/development/development-guide.md
```

---

## Deployment

### deployment.md

Defines the deployment strategy, infrastructure, environments, monitoring, backups, CI/CD pipeline, and operational practices.

Location:

```
docs/deployment/deployment.md
```

---

## Specifications

### preparation-package-architecture-decisions.md

Approved umbrella architecture-decisions specification for the planned paid preparation package product. The umbrella approval covers the recorded architecture decisions, including DA1–DA10 and the reporting-profile transition, but does not implement any preparation-package capability. Staged specifications and implementation plans remain separate and unapproved, and Stage 1 has not begun. The specification records approved business invariants and architectural directions, explicitly deferred features, design details reserved for staged specifications, and launch-time configuration decisions.

Location:

```
docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md
```

---

# Documentation Principles

Every document in this directory has a single responsibility.

When implementation changes:

- Update the relevant documentation.
- Keep documentation synchronized with the codebase.
- Avoid duplicating detailed information across multiple documents.
- Treat each document as the single source of truth for its topic.

---

# Related Repository Documents

The following repository-level documents complement the documentation in this directory:

| Document | Purpose |
|----------|---------|
| `README.md` | Project overview and entry point |
| `PROJECT_RULES.md` | Repository-wide development rules |
| `AGENTS.md` | Instructions for AI coding agents |
| `CURRENT_TASK.md` | Active implementation milestone |
| `TASKS.md` | Long-term project roadmap |
| `PROGRESS.md` | Compact current-state / session handoff (not history) |
| `PROGRESS_HISTORY.md` | Historical evidence, read-on-demand, non-authoritative |

---

# Documentation Philosophy

Documentation is an integral part of the project.

Every architectural, implementation, or workflow decision should be reflected in the appropriate documentation to ensure consistency, maintainability, and long-term project sustainability.
