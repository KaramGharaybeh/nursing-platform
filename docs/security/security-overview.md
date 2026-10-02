# Security Overview

## Authority chain

[Product roles and permissions](../product/roles-and-permissions.md) define the business meaning of actor capabilities. [System context](../architecture/system-context.md) identifies the client, server, data, and external-service trust boundaries. This Security layer owns protected enforcement and data-protection rules. [Authentication and authorization](authentication-authorization.md), [data protection](data-protection.md), [threat model](threat-model.md), and [security verification](security-verification.md) own their respective detail.

The server, rather than frontend state or navigation, decides authentication, authorization, ownership, payment and fulfillment truth, entitlements, and official examination results. Development/Test substitutes must remain behind replaceable production-facing interfaces; production must fail closed when its real protected-state authority is unavailable. These constraints come from `PROJECT_RULES.md`. Frontend guards are presentation and navigation controls, as described in [Frontend routing](../frontend/routing-and-permissions.md); they do not secure API resources.

## Protected boundaries

- A client request crosses the API authentication and authorization boundary before protected data is returned or changed.
- Nurse-owned profiles, purchases, Preparation Package entitlements, attempts, and reports require server-side ownership checks; a route or client-held identifier does not confer ownership.
- Payment-provider and email-delivery interactions cross external boundaries. [Integrations](../architecture/integrations.md) owns their system relationship; this layer owns the associated trust and sensitive-data restrictions.
- PostgreSQL persists protected business data. Redis is a cache, never an independent source of protected truth.

The documented controls below distinguish governing constraints from current implementation. An implementation detail does not by itself approve a future security policy.
