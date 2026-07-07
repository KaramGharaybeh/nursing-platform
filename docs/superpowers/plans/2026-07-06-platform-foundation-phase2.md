# Platform Foundation (Phase 2) — Implementation Plan

> **For agentic workers:** Executed inline with review checkpoints. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the production-ready backend infrastructure — DI, configuration, logging, EF Core wiring, Redis caching, error handling, health checks, and CI.

**Architecture:** Clean Architecture with 4 layers (Domain ← Application ← Infrastructure/WebApi). Each layer gets an `Add{Layer}()` extension method.

**Tech Stack:** .NET 10, EF Core + Npgsql, StackExchange.Redis, Serilog, FluentValidation, MediatR, Health Checks, GitHub Actions.

## Global Constraints

- No migrations, no schema, no repositories, no seed data in this phase
- No docker-compose.yml in this repository (managed externally)
- Infrastructure services (PostgreSQL, Redis, Mailpit) provided by external environment
- All timestamps in UTC
- Primary keys use Guid
- Dependencies always point inward

---

### Task 1: NuGet Packages & Dependency Injection Registration

**Files:**
- Modify: `src/NursingPlatform.Application/NursingPlatform.Application.csproj`
- Modify: `src/NursingPlatform.Infrastructure/NursingPlatform.Infrastructure.csproj`
- Modify: `src/NursingPlatform.WebApi/NursingPlatform.WebApi.csproj`
- Create: `src/NursingPlatform.Application/DependencyInjection.cs`
- Create: `src/NursingPlatform.Infrastructure/DependencyInjection.cs`
- Modify: `src/NursingPlatform.WebApi/Extensions/ServiceCollectionExtensions.cs`
- Modify: `src/NursingPlatform.WebApi/Program.cs`

**Interfaces:**
- Produces: `IServiceCollection.AddApplication()`, `IServiceCollection.AddInfrastructure()`, `IServiceCollection.AddPresentation()` extension methods

- [ ] **Step 1: Add NuGet packages to Application project**

  ```xml
  <PackageReference Include="FluentValidation" Version="11.11.0" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
  <PackageReference Include="MediatR" Version="12.4.1" />
  ```

- [ ] **Step 2: Add NuGet packages to Infrastructure project**

  ```xml
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="10.0.0" />
  ```

- [ ] **Step 3: Add NuGet packages to WebApi project**

  ```xml
  <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
  <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="9.0.0" />
  ```

- [ ] **Step 4: Create `Application/DependencyInjection.cs`**

  ```csharp
  using FluentValidation;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;

  namespace NursingPlatform.Application;

  public static class DependencyInjection
  {
      public static IServiceCollection AddApplication(
          this IServiceCollection services)
      {
          services.AddMediatR(config =>
              config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

          services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

          return services;
      }
  }
  ```

- [ ] **Step 5: Create `Infrastructure/DependencyInjection.cs`**

  ```csharp
  using Microsoft.Extensions.DependencyInjection;

  namespace NursingPlatform.Infrastructure;

  public static class DependencyInjection
  {
      public static IServiceCollection AddInfrastructure(
          this IServiceCollection services)
      {
          return services;
      }
  }
  ```

- [ ] **Step 6: Update `WebApi/Extensions/ServiceCollectionExtensions.cs`**

  ```csharp
  using NursingPlatform.Application;
  using NursingPlatform.Infrastructure;
  using NursingPlatform.WebApi.Extensions;

  namespace NursingPlatform.WebApi.Extensions;

  public static class ServiceCollectionExtensions
  {
      public static IServiceCollection AddApplicationServices(
          this IServiceCollection services)
      {
          services.AddEndpointsApiExplorer();
          services.AddApplication();
          services.AddInfrastructure();
          services.AddPresentation();

          return services;
      }

      public static IServiceCollection AddPresentation(
          this IServiceCollection services)
      {
          return services;
      }
  }
  ```

- [ ] **Step 7: Run `dotnet restore` and `dotnet build` to verify**

  Run: `dotnet restore && dotnet build --no-restore`
  Expected: Build succeeds

- [ ] **Step 8: Run tests**

  Run: `dotnet test`
  Expected: Tests pass (1 placeholder test)

---

### Task 2: Configuration System (Options Pattern)

**Files:**
- Create: `src/NursingPlatform.Infrastructure/Configuration/JwtSettings.cs`
- Create: `src/NursingPlatform.Infrastructure/Configuration/DatabaseSettings.cs`
- Create: `src/NursingPlatform.Infrastructure/Configuration/RedisSettings.cs`
- Create: `src/NursingPlatform.Infrastructure/Configuration/EmailSettings.cs`
- Modify: `src/NursingPlatform.Infrastructure/DependencyInjection.cs`
- Modify: `src/NursingPlatform.WebApi/appsettings.json`
- Modify: `src/NursingPlatform.WebApi/appsettings.Development.json`

---

### Task 3: Serilog & Structured Logging

**Files:**
- Modify: `src/NursingPlatform.WebApi/Program.cs`
- Modify: `src/NursingPlatform.WebApi/appsettings.json`

---

### Task 4: EF Core & Database Foundation (Infrastructure Only)

**Files:**
- Create: `src/NursingPlatform.Domain/Common/IAuditableEntity.cs`
- Create: `src/NursingPlatform.Domain/Common/AuditableEntity.cs`
- Create: `src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify: `src/NursingPlatform.Infrastructure/DependencyInjection.cs`
- Modify: `src/NursingPlatform.WebApi/appsettings.json`
- Modify: `src/NursingPlatform.WebApi/appsettings.Development.json`

---

### Task 5: Redis Cache Integration

**Files:**
- Create: `src/NursingPlatform.Application/Abstractions/Caching/ICacheService.cs`
- Create: `src/NursingPlatform.Infrastructure/Caching/RedisCacheService.cs`
- Modify: `src/NursingPlatform.Infrastructure/DependencyInjection.cs`

---

### Task 6: Error Handling, Health Checks, Swagger & CI

**Files:**
- Create: `src/NursingPlatform.WebApi/Middleware/ExceptionMiddleware.cs`
- Create: `src/NursingPlatform.WebApi/Health/HealthChecks.cs`
- Modify: `src/NursingPlatform.WebApi/Program.cs`
- Modify: `src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- Create: `.github/workflows/ci.yml`
