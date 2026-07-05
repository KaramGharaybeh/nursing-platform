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

The architecture is intentionally designed to allow future modules without modifying existing business logic.

Examples include:

- AI Recommendations
- AI Exam Analysis
- Subscription Plans
- Mobile Applications
- Internationalization
- Notification Services

---

# Architecture Goals

Every architectural decision should improve at least one of the following:

- Maintainability
- Readability
- Performance
- Security
- Extensibility
- Developer Experience