# Engineering Standards

## Purpose

This document defines the engineering standards that every contributor and AI coding agent must follow.

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

The backend follows Clean Architecture.

Layers:

- Domain
- Application
- Infrastructure
- Presentation (Web API)

Dependencies always point inward.

Outer layers must never introduce business rules.

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

API responses should return meaningful HTTP status codes.

---

# Validation

All external input must be validated.

Use FluentValidation.

Business validation belongs inside the Application layer.

---

# DTOs

Never expose database entities.

Always expose DTOs.

Separate:

- Request models
- Response models
- Domain models

---

# Entity Framework Core

Always use:

- EF Core
- LINQ
- Async methods

Never:

- Build SQL manually unless absolutely necessary.
- Expose DbContext outside Infrastructure.

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

Passwords:

- Hash only.
- Never store plain text.

Authentication:

- JWT

Authorization:

- Policy/Permission based.

Always validate user input.

---

# Testing

Business logic must be unit tested.

Critical workflows require integration tests.

Tests should be deterministic.

Avoid flaky tests.

---

# API Design

Use RESTful principles.

Endpoints should be predictable.

Return consistent response structures.

HTTP status codes should follow standards.

---

# Database

Database changes must use EF Core migrations.

Never edit production schema manually.

Foreign keys should enforce integrity.

---

# Git

Commit often.

Each commit should represent one logical change.

Commit messages should be meaningful.

Never commit generated files unnecessarily.

---

# Documentation

Architecture changes require documentation updates.

Documentation should evolve together with implementation.

---

# AI Agent Requirements

Before writing code, the AI agent must read:

- README.md
- PROJECT_RULES.md
- AGENTS.md
- CURRENT_TASK.md
- Relevant documentation inside `/docs`

The AI must never invent requirements.

If requirements are incomplete, it must stop and request clarification.

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