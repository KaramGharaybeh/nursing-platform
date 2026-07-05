# Database Design

## Purpose

This document defines the database architecture, design principles, naming conventions, and data persistence strategy for the Nursing Platform.

It serves as the authoritative reference for all database-related decisions throughout the project lifecycle.

---

# Database Technology

The platform uses:

- PostgreSQL
- Entity Framework Core
- Code-First approach
- EF Core Migrations

PostgreSQL is the system's primary persistent data store.

Redis is used only as a distributed cache and must never become the source of truth.

Database schema changes must always be introduced through EF Core Migrations.

Manual schema modifications are prohibited.

---

# Design Goals

The database design prioritizes:

- Data Integrity
- Scalability
- Maintainability
- Performance
- Security
- Consistency
- Extensibility

The schema should support future business expansion without requiring major redesign.

Optimization should never compromise correctness or maintainability.

---

# Architectural Principles

The database is considered an implementation detail of the Infrastructure layer.

Business rules must never depend directly on database structures.

The Application layer communicates through repositories and abstractions.

The Domain layer must remain persistence-ignorant.

All persistence concerns belong exclusively to the Infrastructure layer.

---

# Database Organization

The initial version of the platform uses a single PostgreSQL database.

Business data is organized logically by business modules rather than physical separation.

Examples include:

- Identity
- Nurses
- Employers
- Examinations
- Recruitment
- Administration
- Payments

Future database partitioning or service decomposition should not require redesigning the domain model.

---

# Entity Design Principles

Entities represent business concepts rather than database implementation details.

Each entity should have:

- A single responsibility
- A stable primary key
- Clearly defined relationships
- Explicit ownership rules

Favor clarity over unnecessary optimization.

Avoid overly complex table designs.

---

# Primary Keys

All entities should use:

- UUID (Guid)

Advantages include:

- Globally unique identifiers
- Better support for distributed systems
- Easier future integration with external services

Primary keys must never contain business meaning.

---

# Foreign Keys

Relationships must be explicitly defined.

Foreign key constraints must enforce referential integrity.

Cascade delete should only be enabled when it accurately reflects business rules.

Otherwise, deletion should be restricted.

---

# Audit Fields

Every aggregate root should include audit information unless there is a documented reason not to.

Typical audit fields include:

- CreatedAt
- CreatedBy
- UpdatedAt
- UpdatedBy

These fields support traceability, diagnostics, and operational maintenance.

---

# Soft Delete

Soft deletion should only be used for business entities where historical records must be preserved.

Typical fields include:

- IsDeleted
- DeletedAt
- DeletedBy

Soft-deleted records should be excluded by default using Entity Framework Core Global Query Filters.

Soft delete should not be applied universally.

---

# Timestamps

All timestamps must be stored in UTC.

Time zone conversion belongs exclusively to the presentation layer.

Never store local server time.

---

# Naming Conventions

## Tables

Use plural PascalCase names.

Examples:

- Users
- Roles
- Permissions
- NurseProfiles
- Employers
- Exams

---

## Columns

Use PascalCase.

Examples:

- FirstName
- CreatedAt
- UpdatedAt

---

## Primary Keys

Use:

- Id

---

## Foreign Keys

Use:

- RelatedEntityId

Examples:

- UserId
- EmployerId
- CountryId

---

## Database Objects

Indexes:

- IX_Table_Column

Foreign Keys:

- FK_Table_RelatedTable

Unique Constraints:

- UQ_Table_Column

---

# Relationships

Relationships should be explicit.

Prefer:

- One-to-Many
- Many-to-One

Many-to-Many relationships should use explicit join entities whenever future business requirements may introduce additional metadata.

---

# Constraints

Database constraints should enforce:

- Required fields
- Foreign keys
- Unique values
- Referential integrity

Business validation remains the responsibility of the Application layer.

---

# Indexing Strategy

Indexes should be created only when supported by real query requirements.

Typical candidates include:

- Email
- License Number
- Country
- Foreign Keys
- Frequently filtered columns

Avoid excessive indexing.

Every index increases write cost and maintenance complexity.

---

# Transactions

Business operations that modify multiple aggregates should execute within database transactions.

Transaction coordination is initiated by the Application layer and implemented by the Infrastructure layer.

Transaction boundaries should remain explicit.

---

# Concurrency

Optimistic concurrency should be preferred.

Concurrency tokens may be introduced where concurrent updates are expected.

Concurrency handling should remain transparent to business logic.

---

# Migrations

All schema changes must be created using EF Core Migrations.

Migration history must remain under version control.

Existing migrations should never be modified after they have been shared or deployed.

Always create a new migration instead.

---

# Seed Data

Seed data should be limited to stable reference information.

Examples include:

- Countries
- Languages
- Roles
- Permissions
- Examination Categories

Business data should never be seeded automatically in production.

---

# Performance

Database performance should prioritize:

- Efficient indexing
- Appropriate pagination
- Minimal unnecessary joins
- Asynchronous data access
- Query optimization when justified

Premature optimization should be avoided.

Read-heavy scenarios may introduce dedicated read models in the future as part of the CQRS strategy.

---

# Security

Sensitive information must never be stored in plain text.

Examples include:

- Passwords
- Tokens
- Secrets

Passwords must always be securely hashed.

Personally identifiable information (PII) should be encrypted whenever appropriate.

---

# Backup Strategy

Production environments must support:

- Automated backups
- Point-in-time recovery
- Disaster recovery planning

Backup implementation details are defined in the deployment documentation.

---

# Future Considerations

The database architecture should support future enhancements including:

- Multi-country expansion
- Multi-language support
- Additional payment providers
- AI-driven analytics
- Reporting
- Audit history
- Read replicas
- Database partitioning
- Multi-tenancy (if adopted)
- Horizontal scaling

---

# Design Philosophy

The database exists to support the business domain.

Every database design decision should improve one or more of the following:

- Correctness
- Simplicity
- Consistency
- Maintainability
- Scalability
- Reliability

The schema should remain understandable, predictable, and production-ready throughout the lifetime of the project.