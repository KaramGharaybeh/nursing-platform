# Deployment Runbook

This runbook records source-verified operating steps. [Deployment Architecture](../architecture/deployment-architecture.md) owns topology, and [Environments](environments.md) owns environment boundaries. Source inspection verifies commands and sequencing in code; it does not prove that an unexecuted command succeeded in a particular environment.

## Local Development run

From the repository root, ensure the required .NET and Node/npm tools and a reachable local PostgreSQL instance are available. `run-local.sh` checks `dotnet`, `npm`, `curl`, `frontend/node_modules`, and PostgreSQL at `localhost:5432`; it does not install packages or start PostgreSQL. The optional `run-local-with-mailpit.sh` additionally requires or launches Mailpit and checks its web and SMTP ports. The exact local configuration must be supplied safely according to [Environments](environments.md) and [Data Protection](../security/data-protection.md).

```bash
./run-local.sh
```

For the local mail-capture variant:

```bash
./run-local-with-mailpit.sh
```

The scripts start Web API via the `http` launch profile and Angular via `npm run start -- --port 4200`. They probe the public package-offers API on `http://localhost:5167` and the frontend root on `http://localhost:4200`; the Mailpit variant also probes its local web/SMTP endpoints. Logs are written under `./logs/`. Use `Ctrl+C` to stop services started by the script. Review those log files if a prerequisite or readiness check fails. These commands were inspected, not executed during documentation migration.

## Startup database sequence

Current `backend/src/NursingPlatform.WebApi/Program.cs` calls `InitializeDatabaseAsync` before `app.Run()` outside OpenAPI capture mode. `DatabaseInitializer.InitializeAsync` applies pending EF migrations, seeds reference data, adds a Development-only exam seed when applicable, and runs bootstrap-admin initialization. This is the **current application startup behavior**. The historical deployment guide describes a migration-before-serving policy, but no separate release migration job or production coordination procedure was established. Do not treat local startup behavior as an approved multi-instance release sequence. Review migration and startup logs when diagnosing failed local startup.

## Validation surfaces

The Web API maps `/`, `/health`, `/health/live`, and `/health/ready` (`ApplicationBuilderExtensions`). `/health/ready` selects checks tagged `ready`: PostgreSQL, plus Redis when configured. `/health` and `/health/live` both select all registered checks in current code, so `/health/live` is not a dependency-free process probe. A successful probe verifies the checked conditions at that moment; it does not validate an entire user workflow. [Monitoring and Alerts](monitoring-and-alerts.md) owns observability details.

## Staging and Production release boundary

The repository does not establish a verified runnable Staging/Production deployment sequence, provider/orchestrator, reverse-proxy/TLS commands, backup/restore procedure, or rollback procedure. Historical deployment guidance recoverable in Git discusses these topics as intended guidance; no corresponding deploy scripts, Dockerfile, Compose file, release workflow, or tested recovery commands were found. Its `docker compose up -d` instructions are not runnable from the currently inspected repository because no Compose file was found. Do not use this local runbook as a production release or recovery instruction. [Delivery Current State](../delivery/current-state.md) owns the implementation status; its [Roadmap](../delivery/roadmap.md) records only approved remaining work.
