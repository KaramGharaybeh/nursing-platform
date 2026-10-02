# Monitoring and Alerts

This document owns verified operational signals and their interpretation. [Security](../security/security-overview.md) owns protection of operational surfaces; [Deployment Architecture](../architecture/deployment-architecture.md) owns topology. An instrumented endpoint is not evidence that an external monitor, alert, or incident response process is deployed.

| Surface | Current evidence | Operational meaning / limit |
|---|---|---|
| Application logging | `Program.cs` configures Serilog from app configuration after a console bootstrap logger; `appsettings.json` configures console output; `ApplicationBuilderExtensions` enables request logging. | Console and request logs are available to the hosting process. No central collector or retention contract was established. Logs must be handled under [Data Protection](../security/data-protection.md); this document does not assert that all sensitive values are absent from logs. |
| Health checks | `ServiceCollectionExtensions` registers PostgreSQL and conditionally Redis; `ApplicationBuilderExtensions` maps `/health`, `/health/live`, `/health/ready` with JSON response writer. | `/health/ready` selects tagged dependency checks. `/health` and `/health/live` currently select all checks, including dependencies. The existence of endpoints does not establish a probe schedule or an external monitor. |
| Metrics | No application exporter, metrics collector, or metrics configuration was established in the inspected source/configuration. | Not established; no metric names or thresholds are defined here. |
| Distributed tracing | No tracing exporter or collector configuration was established in the inspected source/configuration. | Not established. |
| Alert rules | No alert definitions or threshold configuration was established in the inspected repository. | Not established; this is not a selected future alert design. |
| Dashboards | No dashboard definition or provider configuration was established in the inspected repository. | Not established. |
| Notification and escalation | No paging, notification, on-call, or escalation procedure was established in the inspected repository. | Not established. |

The legacy deployment guide discusses centralized logging, metrics, tracing, and alerts as prospective operations guidance. These statements are historical evidence, not proof of active instrumentation or an approved provider. [Delivery Current State](../delivery/current-state.md) owns current operational gaps; [Roadmap](../delivery/roadmap.md) owns only approved remaining work.
