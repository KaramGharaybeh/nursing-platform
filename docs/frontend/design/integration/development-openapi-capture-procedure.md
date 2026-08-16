# Development OpenAPI Capture Procedure

## 1. Purpose and non-authority

This document records OpenAPI capture evidence and a safe capture decision for frontend design. It is not API-client generation, Angular authorization, a page specification, a route registry, a Penpot authorization, or an approval of any API contract beyond the cited runtime/source evidence.

## 2. Source snapshot

| Item | Evidence |
|---|---|
| Branch / HEAD | `feature/frontend-design-evidence-foundation` / `0b5e645 docs: add preparation package API validation error index` |
| G0 acceptance | `4073154` |
| Phase 1 evidence packet | `fd19271` |
| API/validation/error index | `0b5e645` |
| Backend evidence baseline | `8439511` |
| Capture date/time context | 2026-08-12, `Asia/Amman` |

## 3. Repository OpenAPI configuration evidence

- `backend/src/NursingPlatform.WebApi/Extensions/ServiceCollectionExtensions.cs` calls `services.AddEndpointsApiExplorer()` and `services.AddOpenApi(...)`. Its document transformer registers a JWT bearer security scheme.
- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` calls `app.MapOpenApi()` only when `app.Environment.IsDevelopment()`.
- The same extension maps API routes under `/api/v1`, including `MapPreparationPackageEndpoints()`.
- The repository contains OpenAPI runtime assemblies in existing build output, but discovery found no generated Development OpenAPI JSON/YAML artifact and no documented capture command or recorded artifact revision.

## 4. Local runtime prerequisites

### Development configuration

`backend/src/NursingPlatform.WebApi/appsettings.Development.json` configures:

- PostgreSQL: `Host=localhost;Port=5432;Database=nursing_platform;Username=nursing_admin;Password=ChangeMe_Development_2026`.
- Redis: `localhost:6379`.
- SMTP: `localhost:1025`.
- Sandbox payment public base URL: `https://localhost:5001/sandbox-payments`.

`backend/src/NursingPlatform.WebApi/Properties/launchSettings.json` declares Development HTTP `http://localhost:5167` and HTTPS `https://localhost:7059;http://localhost:5167`.

At inspection time, `nursing-postgres`, `nursing-redis`, and `nursing-mailpit` Docker containers were running; PostgreSQL and Redis were published on the expected host ports. .NET SDK `10.0.110` and ASP.NET Core runtime `10.0.10` were available.

### Startup safety blocker

The existing `Program.cs` unconditionally calls `await app.InitializeDatabaseAsync()` before endpoint mapping. `InitializeDatabaseAsync` resolves `DatabaseInitializer`, whose `InitializeAsync` checks pending migrations, calls `MigrateAsync()` when any exist, runs reference-data seeding, and bootstraps an administrator.

Because this task forbids migrations, destructive/seeding changes, configuration changes, and source changes, starting the current WebApi process is not a safe read-only capture action. No backend process was started.

## 5. Capture procedure

### Current task result

No safe runtime capture was executed. The likely Development OpenAPI route must not be guessed from framework defaults; source confirms only `MapOpenApi()` without a configured route template.

### Preconditions for a future authorized capture task

1. Establish and approve a non-mutating startup/capture mechanism that bypasses `InitializeDatabaseAsync`, or obtain explicit authorization for the existing migration/seeding initialization behavior against the local database.
2. Start only with `ASPNETCORE_ENVIRONMENT=Development` and an explicit known local port from `launchSettings.json`.
3. Derive the actual OpenAPI URL from runtime endpoint output or an approved source configuration; do not assume an endpoint path.
4. Fetch the confirmed URL with `curl -f` into `docs/frontend/design/integration/openapi/development-openapi-YYYY-MM-DD.json`.
5. Stop the backend process, validate non-empty JSON with `python3 -m json.tool`, and record the backend commit and URL.

No command in the current repository documentation satisfies these prerequisites without triggering initialization.

## 6. Captured artifact

**No OpenAPI artifact captured.**

No artifact path, size, JSON validity result, endpoint URL, or backend runtime capture timestamp exists for this task.

## 7. Preparation Package coverage check

No captured artifact exists, so Preparation Package OpenAPI path presence cannot be confirmed. Endpoint source does map the package route group, but source mapping is not a substitute for a generated Development OpenAPI artifact. After safe capture, inspect the artifact for the `/api/v1/preparation-packages` and `/api/v1/me/nurse-profile/preparation-packages` path families and compare them to `PreparationPackageEndpointExtensions.cs`.

## 8. Risks and cautions

- Starting the current WebApi directly is mutating because database initialization can migrate, seed, and bootstrap an admin.
- A future capture artifact is stale unless tied to an exact backend revision, environment, URL, and timestamp.
- Development OpenAPI is mapped only in Development; Production behavior must not be inferred.
- An artifact does not approve API-client generation, Angular implementation, page specifications, or Penpot work.
- Do not capture secrets or environment configuration alongside the public OpenAPI artifact.

## 9. Open questions impact

`OPEN-PH1-002` remains open. The configuration evidence is sufficient to explain why capture is currently blocked, but not sufficient to close the question because no generated Development artifact or safe capture procedure has been verified. `OPEN-PH1-005` also remains open because no generated revision is available to evidence Preparation Package operations.

## 10. Recommended next step

Request an explicit decision on one of two bounded options: authorize the existing local initialization behavior for a capture-only run, or authorize a separate non-mutating backend startup/capture mechanism. After that decision, perform a dedicated read-only capture task that records the artifact revision and compares Preparation Package paths to source mappings. Do not proceed to API-client generation, page specifications, Penpot, or Angular work.
