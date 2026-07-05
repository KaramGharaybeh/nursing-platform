# Documentation Index

## Purpose

This document serves as the entry point to the Nursing Platform documentation.

The documentation is organized by topic, with each document acting as the authoritative source for its respective area. Contributors and AI coding agents should consult the relevant documentation before making implementation decisions.

---

# Reading Order

When starting work on the project, review the documentation in the following order:

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

---

# Documentation Philosophy

Documentation is an integral part of the project.

Every architectural, implementation, or workflow decision should be reflected in the appropriate documentation to ensure consistency, maintainability, and long-term project sustainability.