# Preparation Package Page Specifications

```yaml
document_id: NPS-DES-SPEC-PP-001
version: 1.0
status: draft-openapi-derived
recorded_at: 2026-08-16
timezone: Asia/Amman
openapi_source: docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json
openapi_sha256: 6ceeb0551d43bd320100f25fea3073874ad1a7df0c10f04752e16cd7b534292b
cst_commit: 77cc3b7
```

## Policy

These specifications are derived directly from the current canonical captured Development OpenAPI artifact. They are read-only drafts for frontend design discovery. They do not authorize Angular implementation, Penpot writes, or backend changes. All field names, types, and error contracts match the OpenAPI schema exactly. Earlier `development-openapi-2026-08-16.json` references are historical and superseded for active implementation planning.

---

## Screen 1: Offers List

| Item | Value |
|------|-------|
| **Stable ID** | `PP-OFFERS-LIST` |
| **Route Path** | `/preparation-packages/offers` |
| **Operation ID** | `ListPreparationPackageOffers` |
| **Method** | `GET` |
| **Auth** | Anonymous (no JWT required) |

### Inputs (Query Parameters)

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| `page` | `int32 \| null` | No | Page number |
| `pageSize` | `int32 \| null` | No | Items per page |
| `countryId` | `uuid` | No | Filter by country |
| `examCategoryId` | `uuid` | No | Filter by exam category |

### Outputs

| Status | Schema | Description |
|--------|--------|-------------|
| `200` | `PaginatedResultOfPreparationPackageOfferListItemDto` | Filtered, paginated offer list |
| `400` | `ValidationProblemDetails` | Invalid query parameters |

### Response DTO: `PreparationPackageOfferListItemDto`

All fields are required unless noted.

| Field | Type | Notes |
|-------|------|-------|
| `id` | `uuid` | Offer identifier |
| `title` | `string` | Display title |
| `slug` | `string` | URL slug; used for detail navigation |
| `summary` | `string \| null` | Optional short description |
| `countryId` | `uuid` | Country identifier |
| `countryName` | `string` | Country display label |
| `examCategoryId` | `uuid` | Exam category identifier |
| `examCategoryName` | `string` | Exam category display label |
| `examId` | `uuid` | Associated exam identifier |
| `examTitle` | `string` | Exam display title |
| `materialCount` | `int32` | Number of included study materials |
| `practiceItemCount` | `int32` | Number of included practice items |
| `accessDurationDays` | `int32` | Access window in days |
| `priceAmountMinor` | `string` (numeric pattern) | Price in minor currency units; transported as string (CST-003) |
| `currency` | `string` | ISO currency code |

### Pagination Wrapper: `PaginatedResult`

| Field | Type |
|-------|------|
| `items` | `PreparationPackageOfferListItemDto[]` |
| `page` | `int32` |
| `pageSize` | `int32` |
| `totalCount` | `int32` |
| `totalPages` | `int32` |

### UI States

| State | Description |
|-------|-------------|
| **Loading** | Skeleton/spinner while first page loads |
| **Empty** | No matching offers; display empty-state message |
| **Success** | Offer list with pagination controls and filter inputs |
| **400 Validation Error** | `ValidationProblemDetails.errors` — field-level errors grouped by property name |

---

## Screen 2: Package Details

| Item | Value |
|------|-------|
| **Stable ID** | `PP-OFFER-DETAIL` |
| **Route Path** | `/preparation-packages/offers/{slug}` |
| **Operation ID** | `GetPreparationPackageOffer` |
| **Method** | `GET` |
| **Auth** | Anonymous |

### Inputs (Path Parameter)

| Parameter | Type | Required |
|-----------|------|----------|
| `slug` | `string` | Yes |

### Outputs

| Status | Schema | Description |
|--------|--------|-------------|
| `200` | `PreparationPackageOfferDetailDto` | Full offer detail |
| `400` | `ValidationProblemDetails` | Invalid slug |
| `404` | `ProblemDetails` | Offer not found or inactive |

### Response DTO: `PreparationPackageOfferDetailDto`

Extends the list DTO with component summaries. All fields required unless noted.

| Field | Type | Notes |
|-------|------|-------|
| `id` | `uuid` | Offer identifier |
| `title` | `string` | Display title |
| `slug` | `string` | URL slug |
| `summary` | `string \| null` | Optional short description |
| `countryId` | `uuid` | Country identifier |
| `countryName` | `string` | Country display label |
| `examCategoryId` | `uuid` | Exam category identifier |
| `examCategoryName` | `string` | Exam category display label |
| `examId` | `uuid` | Associated exam identifier |
| `examTitle` | `string` | Exam display title |
| `materialCount` | `int32` | Number of included study materials |
| `practiceItemCount` | `int32` | Number of included practice items |
| `accessDurationDays` | `int32` | Access window in days |
| `priceAmountMinor` | `string` (numeric pattern) | Price in minor units (CST-003) |
| `currency` | `string` | ISO currency code |
| `components` | `PreparationPackageCatalogComponentSummaryDto[]` | Package component summaries |

### `PreparationPackageCatalogComponentSummaryDto`

| Field | Type | Required |
|-------|------|----------|
| `name` | `string` | Yes |
| `count` | `int32` | Yes |
| `summary` | `string \| null` | No |

### UI States

| State | Description |
|-------|-------------|
| **Loading** | Skeleton while detail loads |
| **404 Not Found** | Offer not found or inactive; dedicated 404 page |
| **Success** | Full offer card: title, summary, price, duration, components, "Purchase" CTA |
| **400 Validation Error** | Invalid slug format |

---

## Screen 3: Checkout / Order

| Item | Value |
|------|-------|
| **Stable ID** | `PP-CHECKOUT-ORDER` |
| **Route Path** | `/me/nurse-profile/payment/orders` |
| **Operation ID** | `CreateMyPaymentOrder` |
| **Method** | `POST` |
| **Auth** | Bearer (JWT required) |

### Inputs (Request Body)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `productId` | `uuid \| null` | No | One of `productId` or `packageOfferId` required |
| `packageOfferId` | `uuid \| null` | No | For packages: the offer identifier |

### Outputs

| Status | Schema | Description |
|--------|--------|-------------|
| `201` | `PaymentOrderDto` | Order created successfully |
| `400` | `ValidationProblemDetails` | Must specify `productId` or `packageOfferId` |
| `401` | — | `WWW-Authenticate: Bearer` |
| `404` | `ProblemDetails` | Offer not found |
| `409` | `ProblemDetails` | Conflict (pending order exists) |

### Response DTO: `PaymentOrderDto`

All fields required unless noted.

| Field | Type | Notes |
|-------|------|-------|
| `id` | `uuid` | Order identifier |
| `status` | `string` | Order status |
| `currency` | `string` | ISO currency code |
| `totalAmountMinor` | `string` (numeric pattern) | Total in minor units (CST-003) |
| `createdAt` | `datetime` | Creation timestamp |
| `updatedAt` | `datetime` | Last update timestamp |
| `expiresAt` | `datetime \| null` | Expiration timestamp |
| `paidAt` | `datetime \| null` | Payment timestamp |
| `cancelledAt` | `datetime \| null` | Cancellation timestamp |
| `items` | `PaymentOrderItemDto[]` | Order line items |

### `PaymentOrderItemDto`

All fields required unless noted.

| Field | Type | Notes |
|-------|------|-------|
| `id` | `uuid` | Item identifier |
| `productId` | `uuid` | Product identifier |
| `productName` | `string` | Product display name |
| `productType` | `string` | Product type |
| `examId` | `uuid` | Associated exam identifier |
| `currency` | `string` | ISO currency code |
| `unitAmountMinor` | `string` (numeric) | Unit price in minor units (CST-003) |
| `quantity` | `int32` | Quantity |
| `lineTotalAmountMinor` | `string` (numeric) | Line total in minor units (CST-003) |
| `sourceType` | `string` | Source type |
| `sourceId` | `uuid` | Source identifier |
| `packageSnapshot` | `PaymentPackageSnapshotDto \| null` | Package snapshot at order time |

### `PaymentPackageSnapshotDto`

| Field | Type |
|-------|------|
| `packageOfferId` | `uuid` |
| `packageOfferTitle` | `string` |
| `packageOfferSlug` | `string` |
| `packageOfferSummary` | `string \| null` |
| `packageDefinitionId` | `uuid` |
| `packageDefinitionTitle` | `string` |
| `packageDefinitionSlug` | `string` |
| `countryId` | `uuid` |
| `examCategoryId` | `uuid` |
| `packageVersionId` | `uuid` |
| `packageVersionNumber` | `int32` |
| `includedExamId` | `uuid` |
| `includedExamVersionId` | `uuid` |
| `includedExamTitle` | `string` |
| `reportingProfilePublicationId` | `uuid` |
| `practiceCollectionVersionId` | `uuid` |
| `studyMaterialVersionIds` | `uuid[]` |
| `priceAmountMinor` | `string` (numeric) |
| `currency` | `string` |
| `accessDurationDays` | `int32` |
| `orderCreatedAt` | `datetime` |

### UI States

| State | Description |
|-------|-------------|
| **401 Unauthorized** | Redirect to login page |
| **Success (201)** | Order created; redirect to order summary or payment page |
| **400 Validation Error** | `ValidationProblemDetails.errors` — must specify one of `productId` or `packageOfferId` |
| **404 Not Found** | Offer does not exist |
| **409 Conflict** | Pending order already exists for this offer |

---

## Shared Error Schemas

### `ValidationProblemDetails` (400)

| Field | Type | Required |
|-------|------|----------|
| `errors` | `{ [key: string]: string[] }` | Yes |
| `type` | `string` | Yes |
| `title` | `string` | Yes |
| `status` | `int32` | Yes |
| `detail` | `string` | Yes |
| `traceId` | `string` | Yes |

### `ProblemDetails` (404, 409)

| Field | Type | Required |
|-------|------|----------|
| `type` | `string` | Yes |
| `title` | `string` | Yes |
| `status` | `int32` | Yes |
| `detail` | `string` | Yes |
| `traceId` | `string` | Yes |

### `CodedProblemDetails` (409 — conflict codes)

Extends `ProblemDetails` with an additional `code` field.

| Field | Type | Required |
|-------|------|----------|
| `code` | `string` | Yes |

---

## Cross-Screen Notes

- All monetary amounts (`priceAmountMinor`, `totalAmountMinor`, `unitAmountMinor`, `lineTotalAmountMinor`) are transported as **numeric strings** per CST-003. Frontend must parse them for display, not treat them as native numbers.
- `ValidationProblemDetails.errors` is a dictionary of `{ fieldName: string[] }` — errors are grouped by property name.
- `CodedProblemDetails` is used only in `409 Conflict` responses with defined conflict codes (e.g., `package-entitlement-inactive`).
- No `422` status exists in the backend — all validation errors return `400`.
- Anonymous screens (`PP-OFFERS-LIST`, `PP-OFFER-DETAIL`) require no authentication. `PP-CHECKOUT-ORDER` requires Bearer JWT.
- Navigation: `PP-OFFERS-LIST` → `PP-OFFER-DETAIL` (via `slug`) → `PP-CHECKOUT-ORDER` (via `packageOfferId`).
