# System Architecture

## Overview

The Nursing Platform is a modular, production-grade SaaS application built using Clean Architecture.

The system is designed around independent business modules that communicate through clearly defined application boundaries.

The architecture prioritizes:

- Scalability
- Maintainability
- Security
- Testability
- Extensibility

---

# High-Level Architecture

The platform consists of the following major components:

```
                    Internet
                        │
                        │
                 Angular Frontend
                        │
                    HTTPS / REST
                        │
                ASP.NET Core Web API
                        │
        ┌───────────────┴────────────────┐
        │                                │
   Application Layer              Infrastructure Layer
        │                                │
     Domain Layer                 PostgreSQL / Redis
```

---

# Architectural Style

The backend follows:

- Clean Architecture
- Domain-Oriented Design
- CQRS
- Dependency Injection

Dependencies always point toward the Domain.

Business rules never depend on infrastructure.

---

# Primary Modules

The platform is divided into independent business modules.

## Identity

Responsible for:

- Registration
- Login
- JWT Authentication
- Refresh Tokens
- Roles
- Permissions

---

## Nurse Management

Responsible for:

- Nurse Profiles
- Education
- Experience
- Certifications
- Skills
- Languages
- CV Management

---

## Employer Management

Responsible for:

- Employer Profiles
- Company Information
- Organization Details

---

## Examination

Responsible for:

- Mock Exams
- Question Bank
- Exam Sessions
- Grading
- Results
- Performance Analytics

---

## Preparation Packages (Planned)

Responsible for the planned paid preparation package product and its four logical benefit rights:

- Managed study materials (managed content library with immutable published versions).
- Practice question bank (separate runtime content from exam content, with immediate feedback).
- Package-scoped exam attempt (one attempt per active entitlement, consumed atomically with the qualifying session creation).
- Analytical report (one immutable diagnostic report per qualifying session).

This module is currently in the documentation/specification phase. The umbrella architecture-decisions specification is reviewed and approved, and the underlying DA1–DA10 business and architecture decisions plus the reporting-profile transition remain approved. This approval covers the recorded architecture decisions only; no preparation-package capability is implemented. Staged specifications and implementation plans remain separate and unapproved, and Stage 1 has not begun. The Examination module continues to own standalone paid and free mock-exam behavior unchanged. See `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md` for the recorded decisions, deferred features, and staged-specification boundaries.

Approved architectural direction for the preparation package module:

- The package domain is additive at the architectural level. Existing handlers, entities, DTOs, tests, and persistence will require backward-compatible modification, but existing standalone-exam behavior is preserved.
- One package purchase entitlement grants four independently authorizable benefit rights.
- A package version references one exact published exam version, one compatible immutable reporting-profile publication, an ordered list of specific published material versions, and one specific practice collection version.
- A package-attempt exam session start is a separate operation from a standalone or free exam session start, records provenance, and consumes the attempt atomically with the qualifying session creation.
- The analytical report references one compatible immutable reporting-profile publication that is separate from the immutable published exam version.
- `ExamAccessGrant` is preserved for standalone paid-exam access and is not repurposed as the package's exam-attempt right.

---

## Recruitment

Responsible for:

- Candidate Search
- Filtering
- Contact Requests
- Candidate Discovery

---

## Administration

Responsible for:

- User Management
- Countries
- Categories
- Skills
- Languages
- Reports
- Discounts
- System Configuration

---

# Backend Layers

## Domain

Contains:

- Entities
- Value Objects
- Domain Rules
- Enumerations

The Domain contains no infrastructure code.

---

## Application

Contains:

- Use Cases
- Commands
- Queries
- DTOs
- Interfaces
- Validation

Business logic lives here.

---

## Infrastructure

Contains:

- EF Core
- Database
- Authentication
- Redis
- File Storage
- Email
- External Services

Infrastructure implements interfaces defined by the Application layer.

---

## Presentation

Contains:

- Minimal APIs
- Endpoint Mapping
- Authentication Middleware
- Dependency Registration

Presentation should remain thin.

---

# External Services

The platform communicates with:

- PostgreSQL
- Redis
- SMTP Server
- Future Payment Providers
- Future Cloud Storage

All integrations should be isolated inside Infrastructure.

---

# Communication Flow

A typical request follows this path:

```
Client

↓

Minimal API Endpoint

↓

Application Command / Query

↓

Domain Rules

↓

Infrastructure

↓

Database

↓

Response DTO

↓

Client
```

---

# Design Principles

The architecture follows these principles:

- Separation of Concerns
- Single Responsibility
- Low Coupling
- High Cohesion
- Explicit Dependencies
- Modular Design

---

# Scalability

The architecture should support:

- Multiple countries
- Multiple licensing systems
- Additional payment providers
- Future mobile applications
- Horizontal scaling

without major architectural changes.

---

# Security

Security is considered at every layer.

Examples include:

- JWT Authentication
- Role-based Authorization
- Permission-based Access
- Request Validation
- Secure Password Hashing

---

# Future Expansion

Future modules should be introduced through backward-compatible extension so existing observable behavior remains stable. Necessary changes to existing modules are allowed when explicitly specified, reviewed, and tested. Unnecessary redesign and breaking changes should be avoided.

Examples include:

- AI Recommendations
- AI Exam Analysis
- Subscription Plans
- Mobile Applications
- Internationalization
- Notification Services
- Preparation Package extensions beyond the first commercial launch (multi-exam bundles, cross-country and cross-category packages, practice-evidence reports, subscriptions, and additional benefits). These extensions are explicitly deferred by the approved umbrella architecture-decisions specification.

---

# Architecture Goals

Every architectural decision should improve at least one of the following:

- Maintainability
- Readability
- Performance
- Security
- Extensibility
- Developer Experience
