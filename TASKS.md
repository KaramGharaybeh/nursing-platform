# Nursing Platform Roadmap

This document defines the implementation roadmap for the Nursing Platform.

Development must follow the phases in order.

Do not start work on a later phase until the current phase is complete.

---

# Phase 1 — Project Foundation ✅

## Documentation

- [x] Product vision
- [x] System architecture
- [x] Backend architecture
- [x] Frontend architecture
- [x] Database design
- [x] API design
- [x] Engineering standards
- [x] Development guide
- [x] Deployment guide

## Repository

- [x] Git repository
- [x] Project standards
- [x] AI agent instructions
- [x] Project roadmap

## Backend

- [x] .NET solution
- [x] Clean Architecture projects
- [x] Project references
- [x] Initial Web API

## Infrastructure

- [x] Docker Compose
- [x] PostgreSQL
- [x] Redis
- [x] MailPit

---

# Phase 2 — Platform Foundation ✅

## API

- [x] Global exception handling
- [x] Problem Details (RFC 7807)
- [x] Health checks
- [x] API versioning
- [x] Swagger / OpenAPI

## Configuration

- [x] Configuration management
- [x] Options pattern
- [x] Environment configuration
- [x] Secret management

## Dependency Injection

- [x] Application registration
- [x] Infrastructure registration
- [x] Presentation registration

## Logging

- [x] Serilog
- [x] Structured logging
- [ ] Request correlation

---

# Phase 3 — Data Layer

## Entity Framework Core

- [x] ApplicationDbContext
- [x] Initial migration
- [x] Database initialization
- [ ] Repository implementations
- [ ] Unit of Work (if adopted)

## Reference Data

- [x] Countries
- [x] Languages
- [x] Roles
- [x] Permissions

---

# Phase 4 — Identity & Security

## Phase 4A — Core Identity ✅

- [x] User management (admin create + register done, full CRUD deferred)
- [x] Authentication (login, JWT pipeline)
- [x] Authorization (completed in 4B)
- [x] JWT (issuance, validation, services)
- [x] Refresh tokens (rotation, revocation detection)
- [x] Email verification
- [x] Password reset
- [x] Role management (admin bootstrap + seed done, full CRUD deferred)
- [x] Permission management (seed done, full CRUD deferred)

## Phase 4B — Authorization ✅

- [x] Permission authorization handler and requirement
- [x] Permission service
- [x] Current user service
- [x] Reference data entities (Permission, Role, RolePermission)
- [x] EF Core configurations for reference data
- [x] Reference data seeder (idempotent, testable)
- [x] RequirePermission extension method for Minimal API
- [x] Register endpoint protected with Users.Create permission
- [x] IPermissionService mocked in WebApi tests
- [x] JWT KeyId fix for JsonWebTokenHandler compatibility
- [x] Integration tests (register 401/403/200, login/refresh no-auth)
- [x] Unit tests (handler, requirement, service, permissions)

## Phase 4C — Account Management Read APIs ✅

- [x] PaginatedResult, UserDetailDto, UserListItemDto
- [x] GetCurrentUserQuery + handler + tests
- [x] GetUserQuery + handler + validator + tests
- [x] ListUsersQuery + handler + validator + tests
- [x] GET /api/v1/me endpoint + integration tests
- [x] GET /api/v1/users and GET /api/v1/users/{id} endpoints + integration tests
- [x] Final build, test, EF migration verification

## Phase 4D — Identity Account Recovery & Verification ✅

- [x] Email verification tokens and persistence
- [x] Password reset tokens and persistence
- [x] Email service / MailKit
- [x] POST /api/v1/auth/send-verification-email
- [x] POST /api/v1/auth/verify-email
- [x] POST /api/v1/auth/forgot-password
- [x] POST /api/v1/auth/reset-password
- [x] Application handler tests
- [x] WebApi integration tests
- [x] EF migration
- [x] Final build, test, and EF verification

---

# Phase 5 — Nurse Module ✅

- [x] Nurse profile
- [x] Experience
- [x] Education
- [x] Certificates
- [x] Skills
- [x] Languages
- [x] CV upload

---

# Phase 6 — Employer Module

- [ ] Employer profile
- [ ] Organization management
- [ ] Candidate search
- [ ] Candidate filtering
- [ ] Contact requests

---

# Phase 7 — Examination Module

- [ ] Countries
- [ ] Categories
- [ ] Question bank
- [ ] Mock exams
- [ ] Exam sessions
- [ ] Timer
- [ ] Auto scoring
- [ ] Results
- [ ] Analytics

---

# Phase 8 — Payments

- [ ] Products
- [ ] Orders
- [ ] Checkout
- [ ] Payment providers
- [ ] Webhooks

---

# Phase 9 — Administration

- [ ] Dashboard
- [ ] User management
- [ ] Question management
- [ ] Category management
- [ ] Reports
- [ ] Audit logs
- [ ] System settings

---

# Phase 10 — Frontend

- [ ] Angular application
- [ ] Authentication
- [ ] Dashboard
- [ ] Nurse portal
- [ ] Employer portal
- [ ] Administration portal
- [ ] Shared component library

---

# Phase 11 — Production Readiness

## Quality

- [ ] Unit tests
- [ ] Integration tests
- [ ] End-to-end tests

## DevOps

- [ ] CI/CD pipeline
- [ ] Docker production images
- [ ] Monitoring
- [ ] Health monitoring
- [ ] Backup strategy
- [ ] Security hardening

## Deployment

- [ ] Production deployment
- [ ] Performance testing
- [ ] Load testing
- [ ] Disaster recovery validation

---

# Development Rules

- Complete phases sequentially.
- Do not implement features outside the current phase.
- Keep documentation synchronized with implementation.
- Every completed feature must compile successfully.
- Run applicable tests before marking work as complete.
- No task is considered complete until its documentation is updated.
- Production quality is required for every implementation.
