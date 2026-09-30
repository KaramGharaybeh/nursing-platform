# Development OpenAPI Capture Investigation

## Scope and non-authority

This is a read-only investigation of whether the current backend can safely yield its existing Development OpenAPI document without source, configuration, runtime, database, cache, frontend, or Penpot changes. It is not an API-client generation decision, Angular authorization, a page specification, route registry, Penpot authorization, or a governance decision.

## 1. Environment inspected

### Verified evidence

| Item | Observation |
|---|---|
| Repository branch and HEAD | `feature/frontend-design-evidence-foundation` / `7768f27` at investigation start |
| WebApi process | Passive process inspection found no `NursingPlatform.WebApi` process and no `dotnet run` process for this application. |
| Configured Development listeners | `backend/src/NursingPlatform.WebApi/Properties/launchSettings.json` declares HTTP `http://localhost:5167` and HTTPS `https://localhost:7059;http://localhost:5167`. |
| Configured-port listeners | Passive listener inspection found no listener on ports `5167` or `7059`. |
| Running Nursing Platform containers | `nursing-postgres`, `nursing-redis`, and `nursing-mailpit` were running. No Nursing Platform WebApi container was present. |
| Existing generated artifact | Repository and backend build-output searches found no generated OpenAPI JSON/YAML artifact. |

No HTTP request was made because passive evidence did not establish an already-running Nursing Platform WebApi. No process, container, database, cache, mail service, or application endpoint was started or changed by this investigation.

## 2. OpenAPI registration evidence

### Verified evidence

- `backend/src/NursingPlatform.WebApi/Extensions/ServiceCollectionExtensions.cs` registers `AddOpenApi(...)` and adds a bearer JWT security scheme through a document transformer.
- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` calls `app.MapOpenApi()` only when `app.Environment.IsDevelopment()`.
- `backend/src/NursingPlatform.WebApi/NursingPlatform.WebApi.csproj` references `Microsoft.AspNetCore.OpenApi` version `10.0.9` and `Microsoft.OpenApi` version `2.7.5`.
- `ApplicationBuilderExtensions.MapApiEndpoints` maps the `/api/v1` group and calls `MapPreparationPackageEndpoints()`.
- `PreparationPackageEndpointExtensions.MapPreparationPackageEndpoints` maps public catalog, nurse entitlement, and administration endpoint groups.

### Evidence boundary

The source confirms Development-only OpenAPI registration and Preparation Package route mapping. It does not provide a generated document, a configured OpenAPI route template, a recorded document URL, or a capture revision.

## 3. Possible capture mechanisms investigated

| Mechanism | Evidence investigated | Safe/non-mutating under this task | Result |
|---|---|---|---|
| Fetch an already-running WebApi document | Passive process and listener inspection for the configured Development ports | Yes only if the WebApi is already running and the request is a read-only GET | Not available: no WebApi process or listener was present, so no request was made. |
| Start the WebApi with an existing Development launch profile | `Program.cs`, launch settings, application initialization, and database initializer | No | Prohibited: startup unconditionally calls database initialization before endpoint mapping. |
| Capture a checked-in/generated artifact | Repository, backend source, scripts/tooling, and build-output searches | Yes if an existing artifact were present | Not available: no JSON/YAML artifact, capture command, or generator/tooling configuration was found. |
| Derive and fetch a framework-default OpenAPI URL | `MapOpenApi()` source call and OpenAPI configuration | Not safe as evidence | Rejected: no source-configured route template or live runtime metadata established the actual URL; it must not be guessed. |
| Add or alter a non-mutating startup/capture mode | Current startup behavior | Outside task scope | Rejected: it would modify source or application behavior. |

## 4. Startup safety evidence

### Verified evidence

- `backend/src/NursingPlatform.WebApi/Program.cs` calls `app.UseApplicationPipeline()`, then unconditionally awaits `app.InitializeDatabaseAsync()`, and only afterwards maps API endpoints and runs the application.
- `ApplicationBuilderExtensions.InitializeDatabaseAsync` resolves `DatabaseInitializer` and invokes `InitializeAsync()`.
- `backend/src/NursingPlatform.Infrastructure/Persistence/DatabaseInitializer.cs` checks pending migrations and calls `MigrateAsync()` when any exist, then always invokes `ReferenceDataSeeder.SeedAsync(context)` and `BootstrapAdminService.BootstrapAsync()`.
- `appsettings.Development.json` configures the local PostgreSQL database, Redis, SMTP, and Development admin bootstrap credentials used by normal Development startup.

### Conclusion

Starting the existing WebApi for OpenAPI capture is not a safe read-only action under this task. It can migrate the database and always invokes seeding and administrator bootstrap behavior. The investigation did not start it.

## 5. Exact command/procedure if a safe mechanism exists

### Conclusion

No safe capture command or procedure exists in the current task environment.

A future task may use a read-only HTTP GET only after passive evidence establishes that a WebApi was already started independently, is listening on a known Development URL, and exposes a confirmed OpenAPI route. That future procedure must record the exact backend revision, environment, URL, timestamp, artifact path, and JSON-validity result. This investigation cannot supply the URL or artifact because neither was established without startup.

## 6. Actual OpenAPI artifact capture

### Verified evidence

No Development OpenAPI artifact was captured. There is no artifact URL, file path, content hash, size, timestamp, JSON validation result, generated operation, schema, or runtime metadata from this task.

## 7. Preparation Package endpoint/path evidence discovered

### Verified source evidence

- The API root group is `/api/v1` in `ApplicationBuilderExtensions.MapApiEndpoints`.
- That group invokes `MapPreparationPackageEndpoints()`.
- `PreparationPackageEndpointExtensions.MapPreparationPackageEndpoints` invokes `MapPublicCatalogEndpoints`, `MapNurseEntitlementEndpoints`, and `MapAdminEndpoints`.
- The nurse entitlement mapping creates groups under `/me/nurse-profile/preparation-packages/entitlements` and `/me/nurse-profile/preparation-packages/exam-sessions`, relative to the API root.

### Evidence boundary

These are endpoint-source facts only. Without a generated Development OpenAPI document, the presence, generated operation metadata, schemas, JSON field names, nullable/required representation, and status-response representation of Preparation Package paths are not verified from OpenAPI.

## 8. Impact on `OPEN-PH1-002`

### Conclusion

`OPEN-PH1-002` must remain open. This investigation established why a generated Development artifact cannot be captured safely in the current environment, but it did not produce the required generated artifact or a verified safe capture procedure.

`OPEN-PH1-005` also remains open because no generated OpenAPI revision exists to evidence Preparation Package operations.

No governance file was changed and no open question was closed by this report.

## 9. Remaining blockers for frontend contract/design work

- No generated Development OpenAPI artifact, recorded capture URL, revision, or content hash exists.
- Normal WebApi startup can invoke migration, reference-data seeding, and administrator bootstrap, so it is not permitted for this read-only task.
- No already-running WebApi was available for a passive, non-mutating document GET.
- Preparation Package OpenAPI operation and schema evidence remains unavailable; source mapping does not replace generated contract evidence.
- The existing independent blockers remain governed elsewhere, including runtime validation invocation (`OPEN-PH1-001`), `422` mapping (`OPEN-PH1-003`), and administration payment permissions (`OPEN-PH1-004`).

## 10. Recommended next action

Obtain explicit authorization for one bounded option before attempting capture:

1. Authorize the existing Development initialization behavior against the designated local environment for a capture-only run; or
2. Authorize a separately designed non-mutating WebApi startup/capture mechanism.

After authorization, capture the actual Development document, validate the artifact as JSON, record its exact source revision and URL, and compare the generated Preparation Package paths and operations against the endpoint-source evidence. Do not begin API-client generation, page specifications, Penpot work, or Angular implementation from this report.
