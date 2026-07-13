# Nursing Platform

## Overview

Nursing Platform is a production-ready SaaS application for the nursing industry.

The platform provides two primary capabilities:

- Realistic mock examinations for nursing licensing exams.
- A recruitment platform connecting qualified nurses with healthcare employers.

The project is designed with a strong focus on scalability, maintainability, security, and long-term sustainability.

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
- Tailwind CSS

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
├── TASKS.md
└── README.md
```

---

# Quick Start

Clone the repository.

```bash
git clone <repository-url>

cd nursing-platform
```

Start the development infrastructure.

```bash
docker compose up -d
```

Build the backend.

```bash
cd backend

dotnet restore

dotnet build
```

Run the Web API.

```bash
dotnet run --project src/NursingPlatform.WebApi
```

Run the frontend.

```bash
cd ../frontend

npm install

npm start
```

---

# Documentation

Project documentation is located in the `docs/` directory.

Start with:

| Document | Purpose |
|----------|---------|
| index.md | Documentation entry point and navigation |

Core documentation:

| Document | Purpose |
|----------|---------|
| product/vision.md | Product vision and business goals |
| architecture/system-architecture.md | Overall system architecture |
| backend/backend-architecture.md | Backend architecture |
| frontend/frontend-architecture.md | Frontend architecture |
| database/database-design.md | Database design |
| api/api-design.md | API standards |
| standards/engineering-standards.md | Engineering standards |
| development/development-guide.md | Local development workflow |
| deployment/deployment.md | Deployment strategy |

---

# Development Workflow

Before implementing any feature:

1. Read `docs/index.md`.
2. Read `CURRENT_TASK.md`.
3. Read `PROJECT_RULES.md`.
4. Read `AGENTS.md`.
5. Read any additional documents referenced by `docs/index.md`.

Only implement work that belongs to the current milestone.
---

# Current Status

Current status:

**Backend local MVP substantially implemented**

Implemented backend capabilities include:

- Identity, JWT authentication, refresh tokens, email verification, password reset, RBAC, and permission-based authorization.
- Nurse profiles, experience, education, certificates, skills, languages, and CV upload.
- Employer profiles, organization management, candidate search/filtering, and recruitment contact requests.
- Exams, admin content management, versions, questions, sessions, scoring, results, attempts, and analytics.
- Payment products, nurse-owned payment orders, immutable order snapshots, checkout sessions, and provider-neutral checkout abstraction.
- Development/Test Sandbox checkout and fulfillment from order checkout to `Paid` order and `ExamAccessGrant` issuance.
- Purchased exam access enforcement for paid exams before exam session start.

Frontend implementation has not started. The Sandbox payment provider is for Development/Test only and is not a production payment integration. A production payment provider has not been selected.

## Backend Verification

Latest synchronized backend verification after `1ec2efa feat: enforce purchased exam access`:

- Domain: 69 passed.
- Application: 434 passed.
- Infrastructure: 119 passed.
- WebApi: 252 passed.
- Total: 874 passed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes.
- PostgreSQL Sandbox tests: 6 passed, 0 skipped.

---

# Development Principles

The project follows these engineering principles:

- Clean Architecture
- SOLID
- DRY
- KISS
- YAGNI
- Domain-Driven Design (where appropriate)

Every implementation should prioritize:

- Correctness
- Maintainability
- Scalability
- Security
- Testability

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

This project is designed to be developed with AI coding assistants such as OpenCode, BigPickle, Claude, and GPT.

Before implementing any feature, AI agents must follow the workflow defined in:

- AGENTS.md
- PROJECT_RULES.md
- CURRENT_TASK.md

## Superpowers

The development workflow integrates the Superpowers skill system.

Available skills are loaded from the global Superpowers installation and are used automatically by compatible AI agents.

Typical workflow:

1. Select applicable skills.
2. Review project documentation.
3. Create or review the implementation plan.
4. Implement incrementally.
5. Verify the implementation.
6. Perform code review.
7. Complete the development branch.

The project documentation remains the single source of truth. Superpowers skills define *how* implementation is performed, while the project documentation defines *what* should be built.
