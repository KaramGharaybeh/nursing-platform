# Engineering Standards

## Purpose

This document owns shared repository-wide engineering conventions that every contributor and AI coding agent must follow. `docs/index.md` routes domain-specific Product, Architecture, Frontend, Backend, API, Security, Testing, Operations, and Delivery rules to their specialist owners.

These standards are mandatory and apply to every feature, bug fix, refactoring, and architectural change.

---

# General Principles

Every implementation must prioritize:

- Correctness
- Readability
- Maintainability
- Simplicity
- Scalability
- Security
- Testability

Code should be easy to understand before it is optimized.

---

# Clean Architecture

Follow the approved structure and rationale in [Architecture](../architecture/architecture-overview.md). Backend layer responsibilities and dependency implementation rules belong to [Backend Architecture](../backend/backend-architecture.md).

---

# SOLID Principles

All code should follow SOLID principles.

Especially:

- Single Responsibility Principle
- Dependency Inversion Principle

Avoid large classes with multiple responsibilities.

---

# Code Style

## Naming

Use clear and descriptive names.

Avoid abbreviations.

Prefer:

- NurseProfile
- EmployerSearchQuery
- SubmitExamCommand

Avoid:

- NP
- Emp
- DataManager

---

## Classes

A class should have one responsibility.

Large classes should be split into smaller components.

---

## Methods

Methods should:

- Perform one task.
- Be short.
- Have descriptive names.
- Avoid deep nesting.

Prefer early returns.

---

# Dependency Injection

Always use Dependency Injection.

Never instantiate services manually inside business logic.

Avoid service locators.

---

# Error Handling

Use exceptions only for exceptional situations.

Validation errors should not rely on exceptions.

HTTP status and error-response conventions belong to [API Guidelines and Errors](../api/api-guidelines-and-errors.md).

---

# Validation

Validate external input at the appropriate boundary. Backend validation placement and implementation belong to [Backend Architecture](../backend/backend-architecture.md); request and error representation belong to [API Guidelines and Errors](../api/api-guidelines-and-errors.md).

---

# DTOs

Keep transport, domain, and persistence representations separated according to [Backend Architecture](../backend/backend-architecture.md), [Backend Domain Model](../backend/domain-model.md), and the [OpenAPI contract](../api/openapi.yaml).

---

# Entity Framework Core

Persistence implementation, DbContext boundaries, query conventions, and provider-specific rules belong to [Storage and Database](../backend/storage-and-database.md).

---

# Asynchronous Programming

Prefer async/await.

Avoid blocking calls.

Avoid `.Result` and `.Wait()`.

---

# Logging

Use structured logging.

Never log:

- Passwords
- Tokens
- Secrets
- Personal sensitive information

Errors should include enough context for debugging.

---

# Security

Security controls and authentication/authorization enforcement belong to [Security](../security/security-overview.md). Follow [Data Protection](../security/data-protection.md) for sensitive-data and credential handling.

---

# Testing

Verification layers, determinism, isolation, and evidence expectations belong to the [Testing Strategy](../testing/testing-strategy.md).

---

# API Design

HTTP conventions and error behavior belong to [API Guidelines and Errors](../api/api-guidelines-and-errors.md); endpoint and schema specifics belong to [OpenAPI](../api/openapi.yaml).

---

# Database

Database implementation and migration conventions belong to [Storage and Database](../backend/storage-and-database.md). Operational database procedure belongs to [Operations](../operations/deployment-runbook.md).

---

# Git

Repository-wide Git constraints belong to [PROJECT_RULES.md](../../PROJECT_RULES.md); AI-agent Git behavior belongs to [AGENTS.md](../../AGENTS.md).

---

# Documentation

Update the authoritative owner routed by [docs/index.md](../index.md), cross-reference rather than duplicate, and update Delivery when implementation or status changes materially.

---

# AI Agent Requirements

AI-agent workflow, evidence discipline, skill use, task boundaries, and stop conditions belong to [AGENTS.md](../../AGENTS.md). Repository constraints remain in [PROJECT_RULES.md](../../PROJECT_RULES.md).

---

# Definition of Production Quality

Production-quality code should be:

- Readable
- Maintainable
- Secure
- Tested
- Modular
- Consistent
- Documented
- Easy to extend
