# Data Layer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create the EF Core data layer with reference data entities, initial migration, seed data, and database initialization as required by Phase 3 (Data Layer) of the project roadmap.

**Architecture:** Reference data entities live in the Domain layer. Entity type configurations live in `Infrastructure/Persistence/Configurations/`. Seed data and database initialization logic live in `Infrastructure/Persistence/`. All database access is managed through EF Core code-first within the Infrastructure layer.

**Tech Stack:** .NET 10, EF Core 10, Npgsql 10

## Global Constraints

- EF Core Code-First approach only — no manual SQL (CURRENT_TASK.md §46)
- All entities use Guid (UUID) primary keys (database-design.md §99-103)
- Plural PascalCase table names (database-design.md §170-182)
- PascalCase column names (database-design.md §186-193)
- `AuditableEntity` abstract base class for **aggregate root** entities only (database-design.md §127-138)
- UTC timestamps — never local time (database-design.md §159-166)
- Foreign key constraints must enforce referential integrity (database-design.md §115-125)
- No cascade delete — use `DeleteBehavior.Restrict` unless business rules require otherwise (database-design.md §121-123)
- Seed only stable reference data (database-design.md §311-325)
- Migration assembly = `NursingPlatform.Infrastructure` (configured in existing code)
- Domain entities in `Domain/`, EF configurations in `Infrastructure/Persistence/Configurations/`
- Entity configurations use `IEntityTypeConfiguration<T>` — not data annotations
- `IAuditableEntity.CreatedAt` and `CreatedBy` marked `IsModified = false` on updates (existing behavior)
- No `ValidateOnStart()` for options — placeholder `ValidateDataAnnotations()` only

---
## Scope

### In Scope (CURRENT_TASK.md §18-24)

1. Domain entities: `Country`, `Language`, `Role`, `Permission`, `RolePermission`
2. Entity type configurations for all five entities
3. `DbSet` properties and configuration application in `ApplicationDbContext`
4. Seed data for Countries, Languages, Roles, and Permissions
5. `DatabaseInitializer` that runs on application startup (CURRENT_TASK.md §51)
6. Initial EF Core migration (`InitialCreate`)
7. Unit tests for domain entity constructors and properties
8. Documentation updates (`CURRENT_TASK.md`, `TASKS.md`)

### Not in Current Focus (TASKS.md Phase 3, deferred)

- Repository implementations (TASKS.md §86)
- Unit of Work — if adopted (TASKS.md §87)

These are Phase 3 items listed in TASKS.md but are NOT in CURRENT_TASK.md's Current Focus or Definition of Done. They will be implemented when CURRENT_TASK.md is updated to include them.

### Out of Scope (CURRENT_TASK.md §28-39)

- Identity module
- Nurse module
- Employer module
- Examination module
- Payments
- Recruitment
- Notifications
- Administration features

---
## Implementation Order

Each task builds on the previous one. Tasks must be implemented in order. Every task includes its own verification step.

1. Domain entities (`Country`, `Language`, `Role`, `Permission`, `RolePermission`) with unit tests
2. Entity type configurations (`IEntityTypeConfiguration<T>`)
3. `ApplicationDbContext` updates (`DbSet` properties, `ApplyConfigurationsFromAssembly`)
4. Initial EF Core migration (`InitialCreate`) — generated from the finalized model
5. Seed data — `ReferenceDataSeeder`
6. Database initialization — `DatabaseInitializer`
7. Documentation updates

---
## Task Breakdown

### Task 1: Domain Entities

**Files to create:**
- `backend/src/NursingPlatform.Domain/ReferenceData/Country.cs`
- `backend/src/NursingPlatform.Domain/ReferenceData/Language.cs`
- `backend/src/NursingPlatform.Domain/ReferenceData/Role.cs`
- `backend/src/NursingPlatform.Domain/ReferenceData/Permission.cs`
- `backend/src/NursingPlatform.Domain/ReferenceData/RolePermission.cs`

**Files to test:**
- `backend/tests/NursingPlatform.Domain.Tests/ReferenceData/ReferenceDataEntitiesTests.cs`

**Entity design rationale:**

- `Country`, `Language`, `Role`, `Permission` are aggregate roots → extend `AuditableEntity` (database-design.md §127-138)
- `RolePermission` is a pure junction table for the many-to-many relationship between Role and Permission (database-design.md §244). It is **not** an aggregate root. It uses a composite primary key `(RoleId, PermissionId)` — no separate `Id` column. The composite key inherently enforces uniqueness. `DeleteBehavior.Restrict` is used on both foreign keys (database-design.md §121-123). Audit fields are omitted (only aggregate roots require them per database-design.md §127-129).

**Entity definitions:**

```
Country : AuditableEntity
  Id: Guid
  Name: string
  Code: string (2-letter ISO)
  IsActive: bool

Language : AuditableEntity
  Id: Guid
  Name: string
  Code: string (2-letter ISO)
  IsActive: bool

Role : AuditableEntity
  Id: Guid
  Name: string
  Description: string?

Permission : AuditableEntity
  Id: Guid
  Name: string
  Description: string?

RolePermission (no base class — pure junction table, not aggregate root)
  RoleId: Guid (composite PK part 1)
  PermissionId: Guid (composite PK part 2)
  Role: Role (navigation)
  Permission: Permission (navigation)
```

- [ ] **Step 1: Write failing tests**

```csharp
// tests/NursingPlatform.Domain.Tests/ReferenceData/ReferenceDataEntitiesTests.cs
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Domain.Tests.ReferenceData;

public class ReferenceDataEntitiesTests
{
    [Fact]
    public void Country_Should_Set_Properties()
    {
        var id = Guid.NewGuid();
        var country = new Country
        {
            Id = id,
            Name = "United States",
            Code = "US",
            IsActive = true
        };

        Assert.Equal(id, country.Id);
        Assert.Equal("United States", country.Name);
        Assert.Equal("US", country.Code);
        Assert.True(country.IsActive);
    }

    [Fact]
    public void Language_Should_Set_Properties()
    {
        var language = new Language
        {
            Id = Guid.NewGuid(),
            Name = "English",
            Code = "EN",
            IsActive = true
        };

        Assert.Equal("English", language.Name);
        Assert.Equal("EN", language.Code);
    }

    [Fact]
    public void Role_Should_Set_Properties()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "System administrator"
        };

        Assert.Equal("Admin", role.Name);
        Assert.Equal("System administrator", role.Description);
    }

    [Fact]
    public void Permission_Should_Set_Properties()
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "ManageUsers",
            Description = "Can manage users"
        };

        Assert.Equal("ManageUsers", permission.Name);
    }

    [Fact]
    public void RolePermission_Should_Set_Properties()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        Assert.Equal(roleId, rolePermission.RoleId);
        Assert.Equal(permissionId, rolePermission.PermissionId);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

Command: `dotnet test backend/tests/NursingPlatform.Domain.Tests --no-restore`

Expected: FAIL — `Country`, `Language`, `Role`, `Permission`, `RolePermission` types not found.

- [ ] **Step 3: Create entity classes**

```csharp
// src/NursingPlatform.Domain/ReferenceData/Country.cs
using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.ReferenceData;

public class Country : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

```csharp
// src/NursingPlatform.Domain/ReferenceData/Language.cs
using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.ReferenceData;

public class Language : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

```csharp
// src/NursingPlatform.Domain/ReferenceData/Role.cs
using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.ReferenceData;

public class Role : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

```csharp
// src/NursingPlatform.Domain/ReferenceData/Permission.cs
using NursingPlatform.Domain.Common;

namespace NursingPlatform.Domain.ReferenceData;

public class Permission : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

```csharp
// src/NursingPlatform.Domain/ReferenceData/RolePermission.cs
namespace NursingPlatform.Domain.ReferenceData;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
```

- [ ] **Step 4: Run tests — verify they pass**

Command: `dotnet test backend/tests/NursingPlatform.Domain.Tests --no-restore`

Expected: PASS, 5/5

- [ ] **Step 5: Build to verify**

Command: `dotnet build --no-restore`

Expected: Build succeeds, 0 errors

**Completion criteria for Task 1:**
- 5 entity classes created in Domain/ReferenceData/
- Unit tests pass (5/5)
- Build succeeds with 0 errors
- No Infrastructure or WebApi dependency in Domain project

---

### Task 2: Entity Type Configurations

**Files to create:**
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/CountryConfiguration.cs`
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/LanguageConfiguration.cs`
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RoleConfiguration.cs`
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs`
- `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/RolePermissionConfiguration.cs`

- [ ] **Step 1: Create CountryConfiguration**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/Configurations/CountryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex(c => c.Code)
            .IsUnique();
    }
}
```

- [ ] **Step 2: Create LanguageConfiguration**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/Configurations/LanguageConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex(l => l.Code)
            .IsUnique();
    }
}
```

- [ ] **Step 3: Create RoleConfiguration**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/Configurations/RoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);
    }
}
```

- [ ] **Step 4: Create PermissionConfiguration**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);
    }
}
```

- [ ] **Step 5: Create RolePermissionConfiguration**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/Configurations/RolePermissionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

- [ ] **Step 6: Build to verify**

Command: `dotnet build --no-restore`

Expected: Build succeeds, 0 errors

**Completion criteria for Task 2:**
- 5 configuration classes created in Infrastructure/Persistence/Configurations/
- Build succeeds with 0 errors
- All configurations use `IEntityTypeConfiguration<T>` (no data annotations)
- `RolePermission` uses composite PK `(RoleId, PermissionId)`, `DeleteBehavior.Restrict` on both FKs

---

### Task 3: Update ApplicationDbContext

**Files to modify:**
- `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`

- [ ] **Step 1: Add DbSet properties and apply configurations via `ApplyConfigurationsFromAssembly`**

The `OnModelCreating` override calls `base.OnModelCreating(modelBuilder)` first, then discovers and applies all `IEntityTypeConfiguration<T>` implementations in the Infrastructure assembly via `modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)`.

```csharp
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.Common;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker
            .Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    break;
            }
        }
    }
}
```

- [ ] **Step 2: Build to verify**

Command: `dotnet build --no-restore`

Expected: Build succeeds, 0 errors

**Completion criteria for Task 3:**
- 5 `DbSet<>` properties added (`Countries`, `Languages`, `Roles`, `Permissions`, `RolePermissions`)
- `OnModelCreating` calls `base.OnModelCreating(modelBuilder)` then `ApplyConfigurationsFromAssembly`
- All 5 entity type configurations discovered and applied automatically
- Audit field handling preserved (unchanged from existing behavior)
- Build succeeds with 0 errors

---

### Task 4: Initial EF Core Migration

- [ ] **Step 1: Model review — verify all entities before generating migration**

Confirm the following before generating:

| Check | Entity | Detail |
|-------|--------|--------|
| PK | Country | `Id` (Guid), configured in `CountryConfiguration` |
| PK | Language | `Id` (Guid), configured in `LanguageConfiguration` |
| PK | Role | `Id` (Guid), configured in `RoleConfiguration` |
| PK | Permission | `Id` (Guid), configured in `PermissionConfiguration` |
| PK | RolePermission | Composite `(RoleId, PermissionId)`, configured in `RolePermissionConfiguration` |
| FK | RolePermission→Role | `RoleId`, `DeleteBehavior.Restrict` |
| FK | RolePermission→Permission | `PermissionId`, `DeleteBehavior.Restrict` |
| Index | Country | Unique on `Code` |
| Index | Language | Unique on `Code` |
| Index | RolePermission | Unique composite PK (inherent) |
| Naming | All tables | Plural PascalCase: Countries, Languages, Roles, Permissions, RolePermissions |
| Naming | All columns | PascalCase |
| Audit | Aggregate roots | `Country`, `Language`, `Role`, `Permission` extend `AuditableEntity` |
| Audit | Junction table | `RolePermission` is not an aggregate root — no audit fields |

- [ ] **Step 2: Generate the migration**

Running PostgreSQL is **not** required. Migration generation uses the EF Core model only.

```
dotnet ef migrations add InitialCreate \
    --project backend/src/NursingPlatform.Infrastructure \
    --startup-project backend/src/NursingPlatform.WebApi \
    --output-dir Persistence/Migrations
```

Expected: Migration files created under `Infrastructure/Persistence/Migrations/`

- [ ] **Step 3: Build to verify**

Command: `dotnet build --no-restore`

Expected: Build succeeds, 0 errors

**Completion criteria for Task 4:**
- Migration files created in `Infrastructure/Persistence/Migrations/`
- Migration creates tables: Countries, Languages, Roles, Permissions, RolePermissions
- Build succeeds with 0 errors

---

### Task 5: Seed Data

**Files to create:**
- `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs`

**Design decision:** Seed data uses deterministic Guid values to ensure migrations and seed data are stable across environments.

- [ ] **Step 1: Create ReferenceDataSeeder**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Domain.ReferenceData;

namespace NursingPlatform.Infrastructure.Persistence.Seed;

public static class ReferenceDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Set<Country>().AnyAsync())
            return;

        var countries = new List<Country>
        {
            new() { Id = new Guid("A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D"), Name = "United States", Code = "US", IsActive = true },
            new() { Id = new Guid("B2C3D4E5-F6A7-4B8C-9D0E-1F2A3B4C5D6E"), Name = "United Kingdom", Code = "GB", IsActive = true },
            new() { Id = new Guid("C3D4E5F6-A7B8-4C9D-0E1F-2A3B4C5D6E7F"), Name = "Canada", Code = "CA", IsActive = true },
            new() { Id = new Guid("D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F80"), Name = "Australia", Code = "AU", IsActive = true },
            new() { Id = new Guid("E5F6A7B8-C9D0-4E1F-2A3B-4C5D6E7F8091"), Name = "United Arab Emirates", Code = "AE", IsActive = true },
            new() { Id = new Guid("F6A7B8C9-D0E1-4F2A-3B4C-5D6E7F8091A2"), Name = "Saudi Arabia", Code = "SA", IsActive = true },
            new() { Id = new Guid("A7B8C9D0-E1F2-4A3B-4C5D-6E7F8091A2B3"), Name = "Qatar", Code = "QA", IsActive = true },
            new() { Id = new Guid("B8C9D0E1-F2A3-4B4C-5D6E-7F8091A2B3C4"), Name = "Oman", Code = "OM", IsActive = true },
            new() { Id = new Guid("C9D0E1F2-A3B4-4C5D-6E7F-8091A2B3C4D5"), Name = "Kuwait", Code = "KW", IsActive = true },
            new() { Id = new Guid("D0E1F2A3-B4C5-4D6E-7F80-91A2B3C4D5E6"), Name = "Bahrain", Code = "BH", IsActive = true },
        };

        var languages = new List<Language>
        {
            new() { Id = new Guid("E1F2A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7"), Name = "English", Code = "EN", IsActive = true },
            new() { Id = new Guid("F2A3B4C5-D6E7-4F80-91A2-B3C4D5E6F708"), Name = "Arabic", Code = "AR", IsActive = true },
            new() { Id = new Guid("A3B4C5D6-E7F8-4091-A2B3-C4D5E6F70819"), Name = "French", Code = "FR", IsActive = true },
            new() { Id = new Guid("B4C5D6E7-F809-41A2-B3C4-D5E6F708192A"), Name = "Spanish", Code = "ES", IsActive = true },
            new() { Id = new Guid("C5D6E7F8-091A-42B3-C4D5-E6F708192A3B"), Name = "Hindi", Code = "HI", IsActive = true },
            new() { Id = new Guid("D6E7F809-1A2B-43C4-D5E6-F708192A3B4C"), Name = "Urdu", Code = "UR", IsActive = true },
            new() { Id = new Guid("E7F8091A-2B3C-44D5-E6F7-08192A3B4C5D"), Name = "Tagalog", Code = "TL", IsActive = true },
        };

        var roles = new List<Role>
        {
            new() { Id = new Guid("F8091A2B-3C4D-45E6-F708-192A3B4C5D6E"), Name = "SuperAdmin", Description = "Full system access" },
            new() { Id = new Guid("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F"), Name = "Admin", Description = "Administrative access" },
            new() { Id = new Guid("1A2B3C4D-5E6F-4708-192A-3B4C5D6E7F80"), Name = "Nurse", Description = "Nurse user" },
            new() { Id = new Guid("2B3C4D5E-6F70-4819-2A3B-4C5D6E7F8091"), Name = "Employer", Description = "Employer user" },
        };

        var permissions = new List<Permission>
        {
            new() { Id = new Guid("3C4D5E6F-7081-492A-3B4C-5D6E7F8091A2"), Name = "Users.View", Description = "View users" },
            new() { Id = new Guid("4D5E6F70-8192-4A3B-4C5D-6E7F8091A2B3"), Name = "Users.Create", Description = "Create users" },
            new() { Id = new Guid("5E6F7081-92A3-4B4C-5D6E-7F8091A2B3C4"), Name = "Users.Edit", Description = "Edit users" },
            new() { Id = new Guid("6F708192-A3B4-4C5D-6E7F-8091A2B3C4D5"), Name = "Users.Delete", Description = "Delete users" },
            new() { Id = new Guid("708192A3-B4C5-4D6E-7F80-91A2B3C4D5E6"), Name = "Roles.View", Description = "View roles" },
            new() { Id = new Guid("8192A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7"), Name = "Roles.Manage", Description = "Manage roles" },
            new() { Id = new Guid("92A3B4C5-D6E7-4F80-91A2-B3C4D5E6F708"), Name = "Permissions.View", Description = "View permissions" },
            new() { Id = new Guid("A3B4C5D6-E7F8-4091-A2B3-C4D5E6F70819"), Name = "Permissions.Manage", Description = "Manage permissions" },
            new() { Id = new Guid("B4C5D6E7-F809-41A2-B3C4-D5E6F708192A"), Name = "Countries.View", Description = "View countries" },
            new() { Id = new Guid("C5D6E7F8-091A-42B3-C4D5-E6F708192A3B"), Name = "Countries.Manage", Description = "Manage countries" },
            new() { Id = new Guid("D6E7F809-1A2B-43C4-D5E6-F708192A3B4C"), Name = "Languages.View", Description = "View languages" },
            new() { Id = new Guid("E7F8091A-2B3C-44D5-E6F7-08192A3B4C5D"), Name = "Languages.Manage", Description = "Manage languages" },
            new() { Id = new Guid("F8091A2B-3C4D-45E6-F708-192A3B4C5D6E"), Name = "Exams.View", Description = "View exams" },
            new() { Id = new Guid("091A2B3C-4D5E-46F7-0819-2A3B4C5D6E7F"), Name = "Exams.Create", Description = "Create exams" },
            new() { Id = new Guid("1A2B3C4D-5E6F-4708-192A-3B4C5D6E7F80"), Name = "Exams.Edit", Description = "Edit exams" },
            new() { Id = new Guid("2B3C4D5E-6F70-4819-2A3B-4C5D6E7F8091"), Name = "Exams.Delete", Description = "Delete exams" },
            new() { Id = new Guid("3C4D5E6F-7081-492A-3B4C-5D6E7F8091A2"), Name = "Questions.View", Description = "View questions" },
            new() { Id = new Guid("4D5E6F70-8192-4A3B-4C5D-6E7F8091A2B3"), Name = "Questions.Manage", Description = "Manage questions" },
            new() { Id = new Guid("5E6F7081-92A3-4B4C-5D6E-7F8091A2B3C4"), Name = "Nurses.View", Description = "View nurses" },
            new() { Id = new Guid("6F708192-A3B4-4C5D-6E7F-8091A2B3C4D5"), Name = "Employers.View", Description = "View employers" },
        };

        context.Set<Country>().AddRange(countries);
        context.Set<Language>().AddRange(languages);
        context.Set<Role>().AddRange(roles);
        context.Set<Permission>().AddRange(permissions);

        await context.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Build to verify**

Command: `dotnet build --no-restore`

Expected: Build succeeds, 0 errors

**Completion criteria for Task 5:**
- `ReferenceDataSeeder` created in Infrastructure/Persistence/Seed/
- Uses deterministic Guids for all seed entities
- Guards against duplicate seeding (`AnyAsync` check)
- Build succeeds with 0 errors

---

### Task 6: Database Initialization

**Files to create:**
- `backend/src/NursingPlatform.Infrastructure/Persistence/DatabaseInitializer.cs`

**Files to modify:**
- `backend/src/NursingPlatform.Infrastructure/DependencyInjection.cs`
- `backend/src/NursingPlatform.WebApi/Program.cs`

**Design rationale:** CURRENT_TASK.md §51 requires "Database initialization runs on application startup." The simplest approach is calling `DatabaseInitializer.InitializeAsync()` from `Program.cs` after the pipeline is built. This runs in **all environments** per the requirement (no environment guard is specified). Seeding is idempotent: `ReferenceDataSeeder` checks `context.Set<Country>().AnyAsync()` before inserting, so repeated calls are safe.

- [ ] **Step 1: Create DatabaseInitializer**

```csharp
// src/NursingPlatform.Infrastructure/Persistence/DatabaseInitializer.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NursingPlatform.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            await Seed.ReferenceDataSeeder.SeedAsync(context);
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }
}
```

- [ ] **Step 2: Register DatabaseInitializer in DependencyInjection.cs**

Append to the `AddInfrastructure` method:

```csharp
services.AddScoped<DatabaseInitializer>();
```

- [ ] **Step 3: Create extension method for database initialization**

Add to `ApplicationBuilderExtensions.cs`:

```csharp
public static async Task InitializeDatabaseAsync(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}
```

- [ ] **Step 4: Wire extension method in Program.cs**

Call the extension method after `UseApplicationPipeline()` and before `app.Run()`:

```csharp
await app.InitializeDatabaseAsync();
```

This follows the existing pattern in `ApplicationBuilderExtensions.cs` (e.g., `UseApplicationPipeline()`) and keeps `Program.cs` minimal.

- [ ] **Step 5: Build to verify**

Command: `dotnet build --no-restore`

Expected: Build succeeds, 0 errors

**Completion criteria for Task 6:**
- `DatabaseInitializer` created with `InitializeAsync` method
- `IServiceProvider` creates scope, resolves `ApplicationDbContext`
- Migration applied via `context.Database.MigrateAsync()`
- Seed called after migration
- Registered as scoped service in `DependencyInjection.cs`
- Invoked from `Program.cs` at startup
- Build succeeds with 0 errors

---

### Task 7: Documentation Updates

**Files to modify:**
- `backend/CURRENT_TASK.md`
- `backend/TASKS.md`

- [ ] **Step 1: Update CURRENT_TASK.md**

Mark all Phase 3 Current Focus items as complete. Move milestone to next phase.

- [ ] **Step 2: Update TASKS.md**

Mark Phase 3 completed checklist items:
- [x] ApplicationDbContext
- [x] Initial migration
- [x] Database initialization
- [x] Countries
- [x] Languages
- [x] Roles
- [x] Permissions

Leave unchecked (deferred):
- [ ] Repository implementations
- [ ] Unit of Work (if adopted)

**Completion criteria for Task 7:**
- CURRENT_TASK.md reflects completed state
- TASKS.md checkboxes updated
- Documentation committed to git

---
## Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| EF Core tool not installed | Cannot generate migration | `dotnet tool install --global dotnet-ef` |

## Verification Steps (Final)

1. **Build:** `dotnet build --no-restore` — must succeed with 0 errors
2. **Domain tests:** `dotnet test backend/tests/NursingPlatform.Domain.Tests --no-build` — must pass 5/5
3. **Architecture compliance:** No Infrastructure or WebApi references in Domain project; Domain is persistence-ignorant
4. **Documentation:** CURRENT_TASK.md and TASKS.md updated
