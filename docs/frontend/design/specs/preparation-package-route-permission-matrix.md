# Preparation Package Route Registry & Permission Matrix

```yaml
document_id: NPS-DES-SPEC-PP-002
version: 1.0
status: draft-openapi-derived
recorded_at: 2026-08-16
timezone: Asia/Amman
openapi_source: docs/frontend/design/integration/openapi/development-openapi-2026-08-16.json
openapi_sha256: aa96da71259c9fc8c91d54b42e17741b6988fdaa7e5308128e3da26b08497f99
cst_commit: 77cc3b7
page_spec_source: docs/frontend/design/specs/preparation-package-page-specs.md
page_spec_document_id: NPS-DES-SPEC-PP-001
```

## Policy

This document is derived from the captured Development OpenAPI artifact and verified backend source code. It is a read-only draft for frontend design discovery. It does not authorize Angular implementation, Penpot writes, or backend changes. All route paths, operation IDs, and authorization requirements match the OpenAPI schema and backend endpoint registration exactly.

---

## Route Registry

| Stable ID | Route Path | HTTP Method | Operation ID | Page Spec Ref | Auth Level |
|-----------|------------|-------------|--------------|---------------|------------|
| `PP-OFFERS-LIST` | `/api/v1/preparation-packages/offers` | `GET` | `ListPreparationPackageOffers` | `NPS-DES-SPEC-PP-001` Screen 1 | Anonymous |
| `PP-OFFER-DETAIL` | `/api/v1/preparation-packages/offers/{slug}` | `GET` | `GetPreparationPackageOffer` | `NPS-DES-SPEC-PP-001` Screen 2 | Anonymous |
| `PP-CHECKOUT-ORDER` | `/api/v1/me/nurse-profile/payment/orders` | `POST` | `CreateMyPaymentOrder` | `NPS-DES-SPEC-PP-001` Screen 3 | Bearer (JWT) |

### API Route Notes

- The routes above represent the full API path (`/api/v1/...`) as defined in the OpenAPI artifact.
- Angular frontend routes will differ from API routes (frontend routes are not yet defined).

---

## Permission Matrix

| Stable ID | Anonymous (no JWT) | Bearer (with JWT) | Required Permissions | Backend Authorization Source |
|-----------|---------------------|--------------------|---------------------|------------------------------|
| `PP-OFFERS-LIST` | ✓ Allowed | ✗ N/A | None | `.AllowAnonymous()` — `PreparationPackageEndpointExtensions.cs:155` |
| `PP-OFFER-DETAIL` | ✓ Allowed | ✗ N/A | None | `.AllowAnonymous()` — `PreparationPackageEndpointExtensions.cs:167` |
| `PP-CHECKOUT-ORDER` | ✗ Rejected (401) | ✓ Allowed | None (any authenticated user) | `.RequireAuthorization()` on parent group — `ApplicationBuilderExtensions.cs:805-806` |

---

## Protection Level Details

### PP-OFFERS-LIST — Anonymous

- No JWT required
- No permission required
- `.AllowAnonymous()` explicitly defined on the endpoint
- OpenAPI: no `security` field on the operation
- Source: `PreparationPackageEndpointExtensions.cs:155`

### PP-OFFER-DETAIL — Anonymous

- No JWT required
- No permission required
- `.AllowAnonymous()` explicitly defined on the endpoint
- OpenAPI: no `security` field on the operation
- Source: `PreparationPackageEndpointExtensions.cs:167`

### PP-CHECKOUT-ORDER — Bearer (Any Authenticated User)

- Valid JWT required
- No specific permission required — any authenticated user can create an order
- `.RequireAuthorization()` on parent group `/me/nurse-profile` — `ApplicationBuilderExtensions.cs:805-806`
- No `.RequirePermission(...)` on the endpoint or any parent group
- OpenAPI: `"security": [{"Bearer": []}]` — `development-openapi-2026-08-16.json:7464-7468`
- Source: `ApplicationBuilderExtensions.cs:1100-1110`

---

## Unused Permissions for Target Screens

The following permissions are NOT used in the three target screens:

| Permission String | Namespace | Actual Usage |
|-------------------|-----------|--------------|
| `PreparationPackages.View` | `Permissions.PreparationPackages` | Admin endpoints only |
| `PreparationPackages.Manage` | `Permissions.PreparationPackages` | Admin endpoints only |
| `PreparationPackages.Publish` | `Permissions.PreparationPackages` | Admin endpoints only |
| `PreparationPackageOffers.Manage` | `Permissions.PreparationPackageOffers` | Admin endpoints only |
| `Exams.View` | `Permissions.Exams` | Admin payment product endpoints only |
| `Exams.Edit` | `Permissions.Exams` | Admin payment product endpoints only |

**Important:** The `Permissions.cs` file defines 25 permissions, but none are required for the third screen (PP-CHECKOUT-ORDER). The only protection mechanism is a valid JWT.

---

## Screen Navigation Flow

```
PP-OFFERS-LIST (Anonymous)
    │
    │ via slug
    ▼
PP-OFFER-DETAIL (Anonymous)
    │
    │ via packageOfferId
    ▼
PP-CHECKOUT-ORDER (Bearer JWT)
```

---

## Verification Sources

| Source | File | Line/Reference |
|--------|------|----------------|
| OpenAPI artifact | `docs/frontend/design/integration/openapi/development-openapi-2026-08-16.json` | Lines 25-96 (offers list), 97-146 (offer detail), 7393-7468 (create order) |
| Page specifications | `docs/frontend/design/specs/preparation-package-page-specs.md` | Full document (296 lines) |
| Catalog endpoints | `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` | Lines 126-168 (public catalog) |
| Payment endpoints | `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` | Lines 805-806 (group auth), 1100-1110 (create order) |
| Permissions | `backend/src/NursingPlatform.Application/Authorization/Permissions.cs` | Lines 61-71 (PreparationPackages, PreparationPackageOffers) |
| CST commit | `77cc3b7` | Contract stabilization baseline |
