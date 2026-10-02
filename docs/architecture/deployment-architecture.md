# Deployment Architecture

## Conceptual topology

The approved architecture identifies an Angular browser client communicating over HTTPS with the ASP.NET Core Web API. The production deployment guide identifies a reverse-proxy/TLS edge, a Web API application service, PostgreSQL for durable data, and Redis for cache. These describe intended relationships, not proof of a live production deployment.

```text
Browser / Angular client
          │ HTTPS
          ▼
Reverse-proxy / TLS boundary
          │
          ▼
ASP.NET Core Web API ─── PostgreSQL (primary persistence)
          │
          └────────────── Redis (cache only)
```

The application service is intended to be stateless; persistent business data stays outside application containers. Docker Compose is identified for local development/testing. The deployment guide calls for containerized application services but does not select Kubernetes, Docker Swarm, or a particular production cloud orchestrator. Email and payment services cross the external boundaries described in [Integrations](integrations.md). No separate production report worker, object-storage provider, CDN, or search container is approved by this topology.

Deployment sequence, migrations, secrets configuration, rollback, health checks, monitoring, and backups are operational concerns. They belong to [Operations](../operations/environments.md), not this topology description. Detailed authentication and authorization enforcement belongs to [Security](../security/security-overview.md); the client-to-API and API-to-external-service boundaries remain architectural trust edges.
