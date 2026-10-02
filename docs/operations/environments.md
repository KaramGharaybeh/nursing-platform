# Environments

This document owns verified operating-environment definitions and configuration boundaries. [Deployment Architecture](../architecture/deployment-architecture.md) owns conceptual topology; [Security](../security/security-overview.md) owns protected configuration policy. Configuration files and historical deployment guidance do not prove a running deployment.

| Environment designation | Evidence | Procedure / deployed-state evidence |
|---|---|---|
| Local Development | `backend/src/NursingPlatform.WebApi/Properties/launchSettings.json` selects `Development`; `appsettings.Development.json`, `run-local.sh`, and `run-local-with-mailpit.sh` configure local backend/frontend and optional mail capture. | [Local run procedure](deployment-runbook.md) is source-verified. A currently running instance was not established by these files. |
| Disposable E2E Test | `frontend/e2e/test-env-guard.mjs`, `global-setup.ts`, and `run-auth-e2e.sh` define a guarded local test environment for the AUTH suite. | Its runner creates/uses disposable dependencies; it does not establish a shared deployed test environment or general production-equivalent test stack. |
| Staging | Historical deployment guidance names Staging as an intended environment; that guidance remains recoverable in Git history. | No current Staging-specific configuration, deploy procedure, endpoint, or deployed instance was established from the inspected repository. |
| Production | Historical deployment guidance names Production; [Deployment Architecture](../architecture/deployment-architecture.md) owns conceptual topology. | No current production provider, orchestrator, deployed instance, or reproducible release procedure was established from the inspected repository. |

## Configuration boundaries

The Web API uses ASP.NET Core configuration for database, optional Redis, JWT, email, payment, and file-storage categories (`backend/src/NursingPlatform.WebApi/appsettings.json` and infrastructure registration). Local launch profiles use `Development`; the frontend's build configurations in `frontend/angular.json` do not establish separately deployed environments. The Angular API configuration derives its API root from the current client configuration. Do not copy values from local settings into deployed settings, publish secrets, or infer deployment from a checked-in template. Actual secret handling is owned by [Data Protection](../security/data-protection.md).

The verified local procedure and its prerequisites are in [Deployment Runbook](deployment-runbook.md). A Staging or Production operational contract must be established before those environments can have runnable instructions here.
