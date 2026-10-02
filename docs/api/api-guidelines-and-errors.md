# API Guidelines and Errors

## Contract authority

[OpenAPI](openapi.yaml) is the current machine-readable HTTP contract for paths, operations, request/response schemas, and declared security/response metadata. It was converted without semantic changes from the September 28 Development OpenAPI snapshot after a fresh WebApi build and live capture matched every non-server field. Its `http://localhost:5167/` server entry is the snapshot's Development capture address, not a production deployment claim. Endpoint mappings, DTOs, validators, and runtime middleware remain implementation evidence. [Product](../product/requirements.md) owns behavior; [Security](../security/authentication-authorization.md) owns protected enforcement.

## Current implementation conventions

The Web API groups application routes under `/api/v1`. Minimal API mappings declare request/response types and authorization metadata. The OpenAPI generator adds Bearer security requirements for protected operations and documents their `401` challenge; an ASP.NET authentication challenge may have no Problem Details body. Do not infer a permission requirement from a Bearer marker alone; inspect endpoint metadata and the server authorization service.

Application handlers return DTOs rather than domain or database entities. Requests are validated by registered FluentValidation validators and endpoint constraints. List operations have endpoint-specific paging, filtering, and sorting shapes; OpenAPI owns each operation's actual parameters rather than a universal rule being assumed here.

## Errors

`ExceptionMiddleware` emits `application/problem+json` for mapped exceptions. Current mappings include `400` for malformed requests or validation, `401` for unauthorized access, `403` for forbidden access and email verification required, `404` for missing resources, `409` for state conflicts, `503` for an unavailable checkout provider, and `500` for unexpected errors. Validation responses include field-error arrays; some conflicts and verification failures include a stable code; a checkout-in-progress conflict can carry `Retry-After` and `retryAfterSeconds`. Unexpected failures receive a generic public detail. The exact response variants are represented in OpenAPI and the middleware contracts.

The legacy API design document mentions `422`, but no `422` mapping or declaration was found in the verified current contract. This document does not establish `422` as a current convention. Missing and non-owned protected resources may intentionally share a safe response; the owning Security and endpoint contracts determine that behavior.

## Versioning and generation boundary

`/api/v1` is the implemented route prefix. The Development-only `--capture-openapi` mode exposes `/openapi/v1.json` while skipping database initialization; it is not a production endpoint or permission to run normal startup against an unreviewed database. The YAML target is a checked-in representation of the verified current capture, not authority for unimplemented future endpoints. When implementation changes, regenerate and compare the contract rather than editing schemas from prose.

For a fresh capture, build the Web API, start it with `ASPNETCORE_ENVIRONMENT=Development` and the `--capture-openapi` argument, fetch `/openapi/v1.json` from its bound local URL, then stop that process. `Program.cs` removes only that sentinel from application arguments and skips `InitializeDatabaseAsync` in capture mode; without it, normal startup may apply migrations and seed data. Verify the JSON, compare paths, operations, schemas, security, and response metadata against the checked-in contract, and record the source revision. Do not treat an older dated frontend snapshot as the current API contract.
