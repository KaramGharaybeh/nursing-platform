# Development Guide

## Purpose

This document describes the recommended development workflow for the Nursing Platform project.

It explains how to set up the local development environment, run the application, follow project standards, and contribute safely without breaking the architecture.

This guide should be read by every developer before making changes to the codebase.

---

# Prerequisites

The following software must be installed before working on the project.

## Required

- Git
- Docker Desktop or Docker Engine
- Docker Compose
- .NET 10 SDK
- Node.js LTS
- npm
- PostgreSQL client (optional)
- Redis CLI (optional)

---

# Repository Structure

```
nursing-platform/

├── backend/
├── frontend/
├── docs/
├── scripts/
├── .github/
├── AGENTS.md
├── CURRENT_TASK.md
├── PROJECT_RULES.md
├── TASKS.md
└── README.md
```

---

# Initial Setup

Clone the repository.

```bash
git clone <repository-url>

cd nursing-platform
```

---

# Start Development Infrastructure

Start all required development services.

```bash
docker compose up -d
```

The development environment includes:

- PostgreSQL
- Redis
- MailPit

Verify the containers are running.

```bash
docker compose ps
```

---

# Backend Setup

Navigate to the backend project.

```bash
cd backend
```

Restore packages.

```bash
dotnet restore
```

Build the solution.

```bash
dotnet build
```

Run the Web API.

```bash
cd src/NursingPlatform.WebApi

dotnet run
```

Verify the application is running.

```
GET /
```

Expected response:

```json
{
  "application": "Nursing Platform API",
  "version": "v1",
  "status": "Running"
}
```

---

# Database

Database schema changes must always be created using Entity Framework Core migrations.

Never modify the database schema manually.

Typical workflow:

1. Create migration.
2. Review generated migration.
3. Apply migration.
4. Commit migration files.

---

# Frontend Setup

Navigate to the frontend project.

```bash
cd frontend
```

Install dependencies.

```bash
npm install
```

Run the development server.

```bash
npm start
```

---

# Development Workflow

Before starting any implementation:

1. Read `CURRENT_TASK.md`.
2. Read `PROJECT_RULES.md`.
3. Read `AGENTS.md`.
4. Read the relevant documentation inside `docs/`.

Do not implement features outside the current milestone.

---

# Branch Strategy

Create a dedicated branch for each logical change.

Example:

```
feature/health-checks

feature/authentication

fix/login-validation
```

Keep branches focused on a single purpose.

---

# Build Verification

Every completed change should pass:

```bash
dotnet build
```

and

```bash
dotnet test
```

No change should be committed if the solution does not build successfully.

---

# Documentation Workflow

Documentation is part of the implementation.

Whenever architecture, behavior, or workflows change:

1. Update the relevant documentation.
2. Verify documentation matches the implementation.
3. Commit documentation together with the code.

Documentation should never become outdated.

---

# Coding Standards

All implementations must follow:

- Clean Architecture
- SOLID
- DRY
- KISS
- YAGNI

Business logic belongs in the Application layer.

The Domain layer must remain independent.

Presentation should remain thin.

---

# Testing

Critical business logic requires automated tests.

Unit tests should cover:

- Domain rules
- Business workflows
- Validation
- Financial calculations
- Exam scoring

Tests should be deterministic and independent.

---

# Logging

Use structured logging.

Never log:

- Passwords
- Tokens
- Secrets
- Sensitive personal information

Logs should provide enough context for troubleshooting.

---

# Git Guidelines

Before committing:

- Ensure the solution builds successfully.
- Ensure tests pass.
- Remove temporary code.
- Remove debugging statements.
- Update documentation if necessary.

Each commit should represent one logical change.

---

# Definition of Done

A development task is considered complete only when:

- The solution builds successfully.
- Tests pass.
- Documentation is updated when required.
- Clean Architecture boundaries are respected.
- No unnecessary complexity has been introduced.
- The implementation is production-ready.