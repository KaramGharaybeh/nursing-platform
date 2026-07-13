# API Design

## Purpose

This document defines the API design standards, conventions, and architectural principles for the Nursing Platform.

All HTTP endpoints must follow the rules defined in this document to ensure consistency, maintainability, and a predictable developer experience.

---

# API Style

The platform exposes a RESTful HTTP API.

Design principles include:

- Resource-oriented endpoints
- Stateless communication
- Predictable URLs
- Consistent request and response structures
- Proper use of HTTP methods
- Standard HTTP status codes

---

# API Versioning

All public endpoints must be versioned.

Initial version:

```
/api/v1
```

Examples:

```
GET /api/v1/health

POST /api/v1/auth/login

GET /api/v1/nurses
```

Future versions must coexist without breaking existing clients.

---

# Content Type

Requests and responses use:

```
application/json
```

File uploads use:

```
multipart/form-data
```

---

# Endpoint Naming

Use nouns rather than verbs.

Good examples:

```
GET /users

GET /nurses

POST /employers

GET /exams
```

Avoid:

```
/getUsers

/createExam

/deleteEmployer
```

HTTP methods express the action.

---

# HTTP Methods

Use standard REST conventions.

| Method | Purpose |
|---------|---------|
| GET | Retrieve resources |
| POST | Create resources |
| PUT | Replace resources |
| PATCH | Partially update resources |
| DELETE | Remove resources |

---

# URL Conventions

Use:

- Lowercase
- Hyphen-separated resource names when necessary
- Stable resource identifiers

Examples:

```
/api/v1/nurses

/api/v1/exams

/api/v1/exam-sessions
```

Avoid:

```
/GetNurses

/ExamList

/DoLogin
```

---

# Request Models

Every request should use dedicated request DTOs.

Never bind directly to:

- Domain entities
- EF Core entities

Validation belongs to the Application layer.

---

# Response Models

Every endpoint should return response DTOs.

Never expose:

- Domain entities
- Database entities
- Internal implementation details

Responses should remain stable even if internal models evolve.

---

# HTTP Status Codes

Use standard HTTP status codes.

Examples:

| Status | Meaning |
|---------|---------|
| 200 | Success |
| 201 | Resource created |
| 204 | No content |
| 400 | Validation error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Resource not found |
| 409 | Conflict |
| 422 | Business rule violation (when applicable) |
| 500 | Unexpected server error |

---

# Error Responses

All errors should follow a consistent structure.

Example:

```json
{
  "type": "...",
  "title": "...",
  "status": 400,
  "detail": "...",
  "traceId": "..."
}
```

The platform uses RFC 7807 Problem Details.

Internal exception information must never be exposed.

---

# Pagination

Collection endpoints should support pagination.

Typical parameters:

```
?page=1

?pageSize=20
```

Responses should include:

- Current page
- Page size
- Total items
- Total pages

---

# Filtering

Filtering should use query parameters.

Example:

```
GET /api/v1/nurses?country=UK&language=English
```

---

# Sorting

Sorting should use query parameters.

Example:

```
?sort=lastName

?sort=-createdAt
```

A leading minus sign indicates descending order.

---

# Searching

Search endpoints should remain predictable.

Example:

```
GET /api/v1/nurses?search=john
```

Avoid creating dedicated search endpoints unless justified.

---

# Authentication

Protected endpoints require JWT Bearer authentication.

Public endpoints should remain explicitly documented.

Authentication configuration is defined separately.

---

# Authorization

Authorization must be policy-based.

Business permissions belong in the Application layer.

Endpoints should declare authorization requirements explicitly.

---

# Validation

All incoming requests must be validated.

Validation should use FluentValidation.

Validation errors should return consistent Problem Details responses.

---

# Idempotency

PUT and DELETE operations should be idempotent whenever possible.

POST operations create new resources unless explicitly documented otherwise.

---

# File Uploads

File upload endpoints should:

- Validate file type
- Validate file size
- Store files outside the application container
- Never trust client-provided filenames

---

# OpenAPI

The API should expose OpenAPI documentation.

Generated documentation must accurately reflect the implemented endpoints.

Manual documentation should be minimized.

---

# Health Endpoints

The platform should expose health endpoints.

Examples:

```
GET /health

GET /health/live

GET /health/ready
```

These endpoints are intended for monitoring and orchestration.

---

# Payment And Exam Access APIs

This section documents the backend payment and purchased-exam-access contracts currently implemented for local frontend development. Sandbox payment behavior is Development/Test-only and must not be presented as a production payment provider.

## Payment Product Catalog

Payment product catalog endpoints require JWT authentication with `.RequireAuthorization()` and do not require an additional permission policy.

```http
GET /api/v1/payment/products?page=1&pageSize=20&examId={examId}
GET /api/v1/payment/products/{id}
```

`GET /api/v1/payment/products` returns a paginated result. `page` defaults to `1`, `pageSize` defaults to `20`, `pageSize` must be between `1` and `100`, and `examId` is optional.

Payment product response fields:

- `id`
- `type`
- `examId`
- `examTitle`
- `name`
- `description`
- `currency`
- `unitAmountMinor`
- `isActive`
- `createdAt`
- `updatedAt`

The authenticated catalog returns active products for published exams. Unauthenticated requests return `401`. Missing products return `404` Problem Details.

## Nurse Payment Orders

Nurse payment order endpoints are under the authenticated current nurse profile group.

```http
POST /api/v1/me/nurse-profile/payment/orders
GET /api/v1/me/nurse-profile/payment/orders?page=1&pageSize=20&status={status}
GET /api/v1/me/nurse-profile/payment/orders/{id}
POST /api/v1/me/nurse-profile/payment/orders/{id}/cancel
```

Create order request fields:

- `productId`

Order response fields:

- `id`
- `status`
- `currency`
- `totalAmountMinor`
- `createdAt`
- `updatedAt`
- `expiresAt`
- `paidAt`
- `cancelledAt`
- `items`

Order item response fields:

- `id`
- `productId`
- `productName`
- `productType`
- `examId`
- `currency`
- `unitAmountMinor`
- `quantity`
- `lineTotalAmountMinor`

Orders are nurse-owned. Clients supply only `productId` when creating an order. The server resolves the authenticated nurse profile, product, exam, currency, amount, immutable item snapshot, expiration, and order status. Client-supplied nurse ids, amount, currency, exam entitlement, `PaidAt`, and grant ids are not trusted.

Expected payment order behavior:

- New orders start as `PendingPayment` and contain one immutable snapshot item with `quantity = 1`.
- Inactive products and unpublished exam products return safe Problem Details through conflict handling.
- Missing or non-owned orders return `404` Problem Details.
- Cancel is available only for owned pending orders without active checkout in progress.
- Active checkout cancellation conflicts return `409` Problem Details.
- Unauthenticated requests return `401`.

## Nurse Checkout Start

Checkout start is authenticated under the current nurse profile group.

```http
POST /api/v1/me/nurse-profile/payment/orders/{orderId}/checkout
```

Checkout start request fields:

- `idempotencyKey`

Checkout session response fields:

- `id`
- `paymentOrderId`
- `status`
- `providerName`
- `checkoutUrl`
- `currency`
- `amountMinor`
- `expiresAt`
- `createdAt`
- `updatedAt`

The response sets `Cache-Control: no-store`.

Checkout start behavior:

- Only owned `PendingPayment` orders can start checkout.
- Missing or non-owned orders return `404` Problem Details.
- Expired orders, non-pending orders, mismatched idempotency keys, terminal idempotency reuse, and provider rejection return safe Problem Details according to the mapped exception.
- An unavailable configured checkout provider returns `503` Problem Details.
- Concurrent checkout initialization can return `409` Problem Details with `Retry-After` and `retryAfterSeconds`.
- The server owns provider selection, checkout session creation, amount, currency, checkout expiration, and provider references.

## Development/Test Sandbox Completion

Sandbox completion is mapped only when the ASP.NET Core environment is Development or `Test`. This route is absent in Production.

```http
POST /api/v1/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete
```

The endpoint requires JWT authentication and sets `Cache-Control: no-store` on the response.

Sandbox completion response fields:

- `paymentOrderId`
- `orderStatus`
- `paidAt`
- `grantedExamIds`

Sandbox completion behavior:

- Only owned Sandbox checkout sessions in `ProviderPending` state can be completed.
- The server atomically transitions the order from `PendingPayment` to `Paid` and persists `PaidAt`.
- Fulfillment transactionally and idempotently issues effective `ExamAccessGrant` rows for purchased exam-access order items.
- Missing or non-owned sessions return `404` Problem Details.
- First completion of a `PendingPayment` order requires the owned Sandbox `ProviderPending` checkout session to be unexpired.
- A first completion attempt using an expired session returns `409` Problem Details.
- After the order is already `Paid`, repeating completion through the same owned and associated Sandbox `ProviderPending` session returns idempotent `200` even if the session has subsequently expired.
- Repeated completion preserves the original persisted `PaidAt` and does not create duplicate `ExamAccessGrant` rows.
- Invalid session state, non-Sandbox sessions, and unsafe fulfillment outcomes return `409` Problem Details.
- No client-supplied payment success, amount, currency, nurse id, exam entitlement, `PaidAt`, or grant id is trusted.

## Exam Start And Paid Access

Exam session start is authenticated.

```http
POST /api/v1/exams/{id}/sessions
```

The exam catalog/detail endpoints include `isFree` and `canStart` fields.

```http
GET /api/v1/exams
GET /api/v1/exams/{id}
```

Effective paid-access rule:

```text
Exam.IsFree == false OR active positive-price ExamAccess product exists
```

When the effective paid-access rule applies, the authenticated nurse must have an active `ExamAccessGrant` for the exam before starting a session. Without a grant, exam start returns `403` Problem Details with a safe forbidden response. Missing exams continue to return `404` Problem Details where applicable. Unauthenticated requests return `401`.

`isFree` and `canStart` are kept consistent in catalog/detail responses with the same access rule used by exam start.

---

# API Evolution

Changes should remain backward compatible whenever possible.

Breaking changes require:

- A new API version
- Updated documentation
- Migration guidance

---

# Security

The API must enforce:

- HTTPS
- Authentication
- Authorization
- Input validation
- Output encoding where appropriate

Sensitive information must never be returned in API responses.

---

# Design Philosophy

The API should always be:

- Consistent
- Predictable
- Secure
- Well documented
- Easy to consume
- Easy to evolve

A well-designed API should minimize surprises for both frontend developers and external consumers.
