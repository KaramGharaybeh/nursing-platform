# Platform Foundation (Phase 2) — Design

> **Status:** Approved  
> **Date:** 2026-07-06  
> **Milestone:** Project Foundation (Phase 2)

## Overview

Complete the production-ready backend infrastructure for the Nursing Platform. This phase establishes the foundational plumbing — DI, configuration, logging, database wiring, caching, error handling, health checks, and CI — so that business modules can be built on a solid base.

## Scope

Build, configure, and integrate the following infrastructure concerns:

### Dependency Injection
- One `Add{Layer}()` extension method per Clean Architecture layer
- `Program.cs` chains: `AddApplication()` → `AddInfrastructure()` → `AddPresentation()`

### Configuration System
- Options Pattern with strongly-typed settings classes
- Environment-specific `appsettings.{Environment}.json` loading
- Secret management via .NET User Secrets (development) / environment variables (production)

### Logging
- Serilog as the logging provider
- Structured JSON output (development: console; production: console + file)
- Request correlation via `TraceId`

### EF Core & Database Wiring
- NuGet packages for EF Core + Npgsql
- `ApplicationDbContext` defined (with entity configurations stubs)
- PostgreSQL connection string configured
- DbContext registered in DI
- **No migrations, no schema, no repositories, no seed data** — these belong to the Data Layer phase

### Redis Caching
- `ICacheService` abstraction (Application layer)
- `RedisCacheService` implementation (Infrastructure layer)
- Redis connection string configured

### Error Handling
- `GlobalExceptionMiddleware` catching unhandled exceptions
- RFC 7807 Problem Details responses
- Structured error logging with context

### Health Checks
- Endpoints: `/health`, `/health/live`, `/health/ready`
- PostgreSQL and Redis health probes

### API Documentation
- Swagger / OpenAPI via `Microsoft.AspNetCore.OpenApi`

### CI/CD Foundation
- GitHub Actions workflow: restore → build → test on push/PR

## Out of Scope
- Docker Compose (managed externally in `~/development/dev-infrastructure/`)
- Database migrations / schema
- Entity configurations beyond stubs
- Persistence logic / repositories
- Seed data
- Any business module (Identity, Nurses, Employers, Exams, etc.)

## Decisions

| Decision | Choice |
|---|---|
| EF Core setup now | Infrastructure wiring only. No migrations, schema, or persistence. |
| Docker Compose | External repository (`~/development/dev-infrastructure/`). Only app-level Dockerfile if needed. |
| Logging provider | Serilog with structured output. |
| Cache abstraction | `ICacheService` interface in Application, `RedisCacheService` in Infrastructure. |
| Health checks | ASP.NET Core built-in health checks with PostgreSQL + Redis probes. |
| CI | GitHub Actions. |
| API documentation | Swagger/OpenAPI via `Microsoft.AspNetCore.OpenApi` (already referenced). |

## Implementation Order

1. NuGet packages + DI registration (layers wired together)
2. Configuration system (Options classes, environment config)
3. Serilog logging (structured logging, request correlation)
4. EF Core foundation (DbContext, PostgreSQL wiring)
5. Redis cache integration (interface + implementation)
6. Error handling, health checks, Swagger, CI workflow
