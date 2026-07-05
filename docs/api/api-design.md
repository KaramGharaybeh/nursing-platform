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