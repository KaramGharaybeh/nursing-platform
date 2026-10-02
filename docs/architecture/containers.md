# Containers and Responsibilities

“Container” here identifies a major runtime or logical boundary, not a one-to-one deployment unit or a repository folder.

| Boundary | Architectural responsibility | Communication |
|---|---|---|
| Angular single-page client | Present product workflows and server-confirmed state; handle client navigation and interaction. | Calls the versioned REST API over HTTPS. It does not authorize access or establish protected business facts. |
| ASP.NET Core Web API / application runtime | HTTP entry, use-case coordination, domain rules, authentication and authorization boundary, and external-interface orchestration. | Receives client requests; uses PostgreSQL, cache, and approved external integration interfaces. |
| PostgreSQL | Primary durable business store shared by the approved modular application architecture. | Accessed through the server's persistence boundary. |
| Redis | Distributed cache, never the authoritative business store. | Accessed through the server's infrastructure boundary. |

Inside the server boundary, Presentation maps HTTP; Application owns use cases and external contracts; Domain owns business concepts and rules; Infrastructure supplies persistence and external adapters. These are **layers within the server**, not four separately deployed services. See [Architecture Overview](architecture-overview.md) and [Data Model](data-model.md).

The approved documents identify email and payment as external dependencies. They are described in [Integrations](integrations.md), not as platform-owned containers. Production orchestration, a separate worker runtime, and a storage-provider container are not selected by the evidence used for this layer.
