# System Context

## Boundary

Nursing Platform comprises the client experience, server-side application/API, and platform-owned data. Nurses, healthcare employers, administrators, and visitors interact through the client or the public API boundary. Their approved business capabilities are defined in [Product roles and permissions](../product/roles-and-permissions.md); this document does not grant capabilities.

```text
Nurse / Employer / Administrator / Visitor
                 │
                 ▼
       Nursing Platform client
                 │ HTTPS / API
                 ▼
       Nursing Platform server ─── PostgreSQL
                 │                 Redis cache
                 ├─────────────── Email service boundary
                 └─────────────── Payment provider boundary
```

The client-to-server edge is a trust boundary. The server authenticates requests, applies authorization and ownership decisions, and owns protected payment, entitlement, and examination-result facts. Frontend navigation or locally held state does not cross that boundary as authority.

PostgreSQL holds primary persistent business data. Redis has a cache role and cannot become the source of truth. Email delivery and payment processing cross external integration boundaries. The architecture does not select a production email or payment vendor. [Integrations](integrations.md) owns those interaction boundaries; [deployment architecture](deployment-architecture.md) owns the conceptual hosting topology.

The system also depends on the browser/client runtime and HTTPS transport. This context does not assert a mobile client, CDN, cloud object store, production webhook, or separate background-worker service as an approved current container.
