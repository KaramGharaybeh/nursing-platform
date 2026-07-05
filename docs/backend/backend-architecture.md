# Backend Architecture

## Overview

The backend is implemented using Clean Architecture.

The solution is organized into four primary projects:

```
NursingPlatform.Domain
NursingPlatform.Application
NursingPlatform.Infrastructure
NursingPlatform.WebApi
```

Each project has a clearly defined responsibility.

---

# Solution Structure

```
backend/

├── src/
│
├── NursingPlatform.Domain
├── NursingPlatform.Application
├── NursingPlatform.Infrastructure
└── NursingPlatform.WebApi
│
└── tests/
    ├── NursingPlatform.Domain.Tests
    └── NursingPlatform.Application.Tests
```

---

# Domain Layer

The Domain project represents the business core.

It contains only business concepts.

## Contains

- Entities
- Value Objects
- Enumerations
- Domain Events (future)
- Domain Exceptions
- Domain Interfaces (only when required)

## Must NOT contain

- Entity Framework
- ASP.NET Core
- HTTP
- Database code
- Logging
- File system
- External services

The Domain must be completely independent.

---

# Application Layer

The Application project contains all business use cases.

## Contains

- Commands
- Queries
- CQRS Handlers
- DTOs
- Interfaces
- Validators
- Application Services
- Mapping
- Authorization Requirements

## Responsibilities

- Execute business workflows
- Coordinate Domain objects
- Validate requests
- Return DTOs

The Application layer depends only on the Domain.

---

# Infrastructure Layer

Infrastructure contains all external implementations.

## Contains

- EF Core
- PostgreSQL
- Redis
- Authentication
- Email
- File Storage
- Repository implementations
- External APIs

Infrastructure implements interfaces defined inside the Application layer.

Business rules never belong here.

---

# Web API Layer

The Web API project is responsible only for HTTP.

## Contains

- Program.cs
- Endpoint registration
- Middleware
- Dependency Injection
- Authentication configuration
- Authorization configuration
- Health Checks
- Swagger/OpenAPI (when enabled)

The Web API should never contain business logic.

---

# Dependency Rules

Allowed dependencies:

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

Forbidden:

- Domain → Infrastructure
- Domain → WebApi
- Application → WebApi
- Domain → EF Core
- Domain → ASP.NET Core

---

# CQRS Organization

Each feature should be organized by feature instead of technical type.

Example:

```
Application/

Identity/

    Commands/

        Register/

            RegisterCommand.cs

            RegisterCommandHandler.cs

            RegisterCommandValidator.cs

    Queries/

        Login/

            LoginQuery.cs

            LoginQueryHandler.cs

Exam/

Recruitment/

Employer/

Nurse/
```

Feature-based organization is preferred over large shared folders.

---

# Dependency Injection

All services must be registered through extension methods.

Example:

```
builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddPresentation();
```

Program.cs should remain small.

---

# Validation

Validation should be implemented using FluentValidation.

Validation must execute before business logic.

Business rules are not validation rules.

---

# Mapping

DTO mapping should be centralized.

Avoid manual mapping spread across the project.

Mapping strategy will be defined later.

---

# Error Handling

Global exception handling should be implemented.

Endpoints should return consistent error responses.

Unexpected exceptions should be logged.

---

# Logging

Logging should use Microsoft's ILogger abstraction.

Sensitive information must never be logged.

Logs should be structured.

---

# Testing Strategy

Domain

- Unit Tests

Application

- Unit Tests
- Integration Tests (future)

Infrastructure

- Integration Tests (future)

Web API

- Endpoint Tests (future)

---

# Future Modularization

As the platform grows, each business module will evolve independently.

Examples:

- Identity
- Nurses
- Employers
- Exams
- Recruitment
- Administration

The architecture should allow adding new modules without changing existing modules.

---

# Backend Goals

The backend should always remain:

- Modular
- Testable
- Secure
- Easy to extend
- Easy to maintain
- Production-ready