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

Current milestone:

**Project Foundation**

The current focus is establishing the production-ready foundation of the platform.

Business modules such as Authentication, Nurse Management, Employer Management, Examinations, Recruitment, and Payments have not yet been implemented.

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