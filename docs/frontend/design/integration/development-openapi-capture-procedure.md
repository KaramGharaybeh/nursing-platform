# Development OpenAPI Capture Procedure

## 1. Purpose and non-authority

This document records the canonical safe Development OpenAPI capture procedure for frontend design. It is not API-client generation, Angular authorization, a page specification, a route registry, a Penpot authorization, or an approval of any API contract beyond the cited runtime/source evidence.

## 2. Source snapshot

| Item | Evidence |
|---|---|
| Canonical snapshot | `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` |
| Previous snapshot retained | `docs/frontend/design/integration/openapi/development-openapi-2026-08-16.json` |
| Capture URL | `http://localhost:5167/openapi/v1.json` |
| Capture mode | Development-only `--capture-openapi` command-line sentinel |

## 3. Repository OpenAPI configuration evidence

- `backend/src/NursingPlatform.WebApi/Extensions/ServiceCollectionExtensions.cs` calls `services.AddEndpointsApiExplorer()` and `services.AddOpenApi(...)`. Its document transformer registers a JWT bearer security scheme.
- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` calls `app.MapOpenApi()` only when `app.Environment.IsDevelopment()`.
- The same extension maps API routes under `/api/v1`, including `MapPreparationPackageEndpoints()`.
- `backend/src/NursingPlatform.WebApi/Program.cs` detects the exact raw command-line sentinel `--capture-openapi`, removes only that sentinel before calling `WebApplication.CreateBuilder(...)`, and preserves other command-line arguments such as `--urls=http://localhost:5167`.
- Capture mode is allowed only in Development. If the sentinel is present outside Development, startup fails fast with `OpenAPI capture mode is Development-only.`
- In capture mode, startup skips only `InitializeDatabaseAsync()`. Normal startup without the sentinel still initializes the database exactly as before.

## 4. Local runtime prerequisites

### Development configuration

`backend/src/NursingPlatform.WebApi/appsettings.Development.json` configures:

- PostgreSQL: `Host=localhost;Port=5432;Database=nursing_platform;Username=nursing_admin;Password=ChangeMe_Development_2026`.
- Redis: `localhost:6379`.
- SMTP: `localhost:1025`.
- Sandbox payment public base URL: `https://localhost:5001/sandbox-payments`.

`backend/src/NursingPlatform.WebApi/Properties/launchSettings.json` declares Development HTTP `http://localhost:5167` and HTTPS `https://localhost:7059;http://localhost:5167`.

At inspection time, `nursing-postgres`, `nursing-redis`, and `nursing-mailpit` Docker containers were running; PostgreSQL and Redis were published on the expected host ports. .NET SDK `10.0.110` and ASP.NET Core runtime `10.0.10` were available.

### Startup safety

Normal WebApi startup calls `await app.InitializeDatabaseAsync()`. `InitializeDatabaseAsync` resolves `DatabaseInitializer`, whose `InitializeAsync` checks pending migrations, calls `MigrateAsync()` when any exist, runs reference-data seeding, and bootstraps an administrator.

The canonical capture command must use Development capture mode so database initialization is bypassed only for the capture process. Capture should not require a usable database connection; use a deliberately unusable local value to prove OpenAPI is reachable without migration, seeding, or bootstrap execution. Do not use valuable database credentials for capture verification.

## 5. Capture procedure

### Canonical safe capture command pattern

Use a fixed local URL for reproducibility and terminate the backend process immediately after retrieving the document.

```bash
ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__DefaultConnection="<deliberately-unusable-redacted-value>" \
dotnet run --no-build --project backend/src/NursingPlatform.WebApi -- \
  --capture-openapi \
  --urls=http://localhost:5167
```

Fetch the document from:

```bash
curl -f http://localhost:5167/openapi/v1.json \
  -o docs/frontend/design/integration/openapi/development-openapi-YYYY-MM-DD.json
```

After retrieval, stop the process and confirm no server remains running on the capture URL.

### Validation and metrics

Validate the captured JSON:

```bash
python3 -m json.tool \
  docs/frontend/design/integration/openapi/development-openapi-YYYY-MM-DD.json \
  > /tmp/development-openapi-validation.json
```

Record these metrics for every canonical snapshot:

- OpenAPI version.
- Path count.
- Operation count.
- Schema count.
- Security schemes.
- Any intentional semantic differences from the previous canonical snapshot.

Snapshot files must be dated and must not overwrite previous canonical snapshots.

## 6. Captured artifact

Current canonical artifact:

```text
docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json
```

The previous canonical snapshot remains available at:

```text
docs/frontend/design/integration/openapi/development-openapi-2026-08-16.json
```

## 7. Preparation Package coverage check

Preparation Package OpenAPI path presence must be verified from the canonical dated artifact before frontend API-client generation or page implementation. Source mapping remains in `PreparationPackageEndpointExtensions.cs`, but the generated Development OpenAPI artifact is the contract evidence for frontend work.

## 8. Risks and cautions

- Starting the WebApi without `--capture-openapi` can be mutating because normal startup initializes the database and may migrate, seed, and bootstrap an admin.
- `--capture-openapi` is not a generic skip-database setting and must not be represented in `appsettings`, environment variables, or production runtime configuration.
- A capture artifact is stale unless tied to an exact backend revision, environment, URL, and timestamp.
- Development OpenAPI is mapped only in Development; Production behavior must not be inferred.
- An artifact does not approve API-client generation, Angular implementation, page specifications, or Penpot work.
- Do not capture secrets or environment configuration alongside the public OpenAPI artifact.

## 9. Open questions impact

`OPEN-PH1-002` can be reviewed against the Development-only safe capture procedure and canonical dated artifact. `OPEN-PH1-005` should be closed only after the frontend technical lead accepts the generated artifact as the Preparation Package contract baseline.

## 10. Recommended next step

Use the canonical dated OpenAPI snapshot for technical-lead review before Angular Phase 1A. Do not proceed to API-client generation, page specifications, Penpot, or Angular work until that review gate is complete.
