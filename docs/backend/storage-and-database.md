# Storage and Database

## Ownership

This document owns current backend persistence implementation. [Architectural Data Model](../architecture/data-model.md) owns conceptual relationships; Operations later owns backup, restore, deployment, and incident procedures.

## Current implementation

`NursingPlatform.Infrastructure.Persistence.ApplicationDbContext` is the EF Core context for the implemented modules. Infrastructure configures Npgsql using `Database:ConnectionString` and the Infrastructure migrations assembly. Entity configurations are applied from the assembly. Tracked EF Core migrations represent the schema history; the presence of a migration file does not prove it has been applied to any environment.

The WebApi startup path calls `DatabaseInitializer`, which checks and applies pending migrations and seeds reference/admin data; Development exam seed data is environment-specific. The OpenAPI capture mode skips this initializer. Schema changes are made through EF Core migrations under repository rules, not by ad hoc manual database alteration.

Current configurations express durable constraints where applicable: unique active package offer per package definition, package-version component ordering and references, package entitlement relationships, effective standalone exam grant and in-progress session uniqueness, and payment-checkout idempotency/correlation uniqueness. Consult the configuration classes for exact filtered predicates; this summary does not redefine business rules or claim every conceptual relationship has a database constraint.

`RolePermissionConfiguration` maps the pure role/permission junction with composite `(RoleId, PermissionId)` primary key and restrict-delete foreign keys, without a separate identity or audit columns. `PackageAnalyticalReportConfiguration` uses unique session identity to prevent duplicate report rows. These are current persistence contracts; they do not by themselves establish new Product policy.

`PackagePracticeProgressConfiguration` stores answered rows with a unique `(PackagePurchaseEntitlementId, PracticeItemId)` index and an index for nurse/entitlement/collection-version reads. `Unanswered` is represented by an absent progress row in the current read model; summary counters are calculated by the query handler rather than persisted as aggregate rows.

PostgreSQL is primary persistence. Redis is an optional distributed cache in the current Infrastructure registration; when its connection string is absent, a no-op cache adapter is registered. Redis cannot establish business truth. Current file uploads use a local file-storage adapter; this observation does not approve a production storage provider. Transaction and concurrency behavior must be read from the owning handlers/configurations for the affected workflow; no universal optimistic-concurrency token policy is established here.
