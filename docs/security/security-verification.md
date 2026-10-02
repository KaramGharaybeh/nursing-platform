# Security Verification

This document owns focused verification of security controls. `docs/testing/testing-strategy.md` is the current Testing owner for general test methodology.

## Authentication and authorization checks

For an anonymous operation, test valid access without a JWT. For an authenticated operation, test `401` without a JWT and valid access with one. For a permission-protected operation, test `401` unauthenticated, `403` authenticated without the exact required permission, and authorized success. These are repository agent testing rules and match current endpoint patterns. Verify the endpoint's declared authorization metadata and server resource-ownership checks separately; a frontend guard test cannot replace them.

Current examples include `AuthEndpointTests`, `MeEndpointTests`, `PackageEntitlementEndpointTests`, and `PackageAnalyticalReportEndpointTests` under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/`. Application-level permission tests cover `PermissionService`, `PermissionRequirement`, and `PermissionAuthorizationHandler`.

## Sensitive-response checks

Inspect the **raw JSON response** for forbidden properties or values before deserializing it into a DTO. A DTO parse alone can silently ignore unexpected fields. Current `MeEndpointTests`, `PaymentEndpointsTests`, `PackageEntitlementEndpointTests`, and `PackageAnalyticalReportEndpointTests` contain examples. Focused recovery tests assert that raw reset/verification token material does not appear in responses.

## Boundary checks

Test non-owned resource requests without exposing another user's existence or protected facts. For purchase/entitlement/attempt/report flows, verify the server does not accept a client-side success assertion or substitute one access source for another. For Development/Test provider substitutes, verify that the production path cannot silently fall back to the substitute. The test evidence must be tied to the actual endpoint or handler being claimed; this document does not claim every possible boundary is already covered.
