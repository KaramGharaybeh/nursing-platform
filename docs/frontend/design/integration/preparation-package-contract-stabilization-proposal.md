# Preparation Package Contract Stabilization Proposal

## 1. Purpose and authority

This report proposes contract-stabilization work based on backend source at `7768f27`, observed Development runtime behavior, and the generated Development OpenAPI document captured from `http://localhost:5167/openapi/v1.json` on 2026-08-13. It is not an implementation, page specification, route registry, API-client decision, Angular authorization, Penpot authorization, or governance decision.

The captured OpenAPI document is OpenAPI `3.1.1`, 234,498 bytes, with SHA-256 `e0b3a427c659db6a1a0f60ddff23a198268c5c953fa0910579098eef7bc96aba`. The temporary artifact is outside the repository at `/tmp/opencode/development-openapi.json`.

## 2. Classification and priority

Allowed action classifications are used exactly as requested:

- `BACKEND FIX REQUIRED`
- `OPENAPI FIX REQUIRED`
- `DOCUMENTATION FIX REQUIRED`
- `GOVERNANCE/BUSINESS DECISION REQUIRED`
- `NO CHANGE REQUIRED / INTENTIONAL BEHAVIOR`
- `COMBINATION OF THE ABOVE`

Priorities:

- `P0`: blocks safe frontend contract generation.
- `P1`: should be resolved before affected feature implementation.
- `P2`: important but does not block initial contract work.
- `P3`: documentation or observability improvement.

## 3. Executive blocker summary

| ID | Area | Classification | Priority | TypeScript generation impact |
|---|---|---|---|---|
| CST-001 | Payment order creation status | COMBINATION OF THE ABOVE | P0 | Status contract conflicts. |
| CST-002 | Payment response schemas | COMBINATION OF THE ABOVE | P0 | Payment response models cannot be generated from OpenAPI. |
| CST-003 | Protected-operation security metadata | OPENAPI FIX REQUIRED | P0 | Generated clients cannot reliably identify authenticated operations. |
| CST-004 | `401` response contract | COMBINATION OF THE ABOVE | P0 | OpenAPI body contract contradicts runtime. |
| CST-005 | Problem Details extensions | COMBINATION OF THE ABOVE | P0 | Structured runtime extensions are absent from OpenAPI. |
| CST-006 | Pagination and FluentValidation invocation | BACKEND FIX REQUIRED | P0 | Documented limits are not enforced at runtime. |
| CST-007 | Numeric OpenAPI unions | OPENAPI FIX REQUIRED | P0 | Generated numeric types conflict with runtime JSON. |
| CST-008 | Required/nullability metadata | COMBINATION OF THE ABOVE | P0 | Required TypeScript properties cannot be derived safely. |
| CST-009 | Exception/request logging status | BACKEND FIX REQUIRED | P2 | Client response is correct, but telemetry records false `500` events. |
| CST-010 | Rate limiting and abuse protection | GOVERNANCE/BUSINESS DECISION REQUIRED | P1 | No current `429` contract may be assumed. |

The API contract is not ready for TypeScript generation while any P0 item remains unresolved.

## 4. Discrepancy proposals

### CST-001: CreateMyPaymentOrder success status

1. **ID:** `CST-001`
2. **Area:** `CreateMyPaymentOrder` success status.
3. **Backend source evidence:** `ApplicationBuilderExtensions.cs`, `CreateMyPaymentOrder`, returns `Results.Created($"/api/v1/me/nurse-profile/payment/orders/{result.Id}", result)`. The command implements `IRequest<PaymentOrderDto>`.
4. **Runtime evidence:** An authenticated mutation was not executed because it would create business data. Source and existing endpoint tests are the available behavior evidence.
5. **OpenAPI evidence:** `POST /api/v1/me/nurse-profile/payment/orders` documents only `200` with no response content/schema.
6. **Existing frontend documentation evidence:** The Phase 1 packet, API validation/error index, and frontend contract baseline record `201` with `PaymentOrderDto`.
7. **Exact discrepancy:** Source and frontend evidence say `201 Created`; generated OpenAPI says `200 OK` and omits the response body type.
8. **Impact on Angular/frontend:** A generated or handwritten client cannot select the authoritative success status or response contract solely from OpenAPI.
9. **Recommended correction:** Preserve `201 Created` as the proposed authoritative behavior because it matches the explicit handler result. Add explicit endpoint metadata for `201` with `PaymentOrderDto`, including the `Location` response header where supported.
10. **Backend change required:** Yes, endpoint metadata only; no business behavior change.
11. **OpenAPI configuration change required:** Yes, operation metadata must expose `201` and the response schema.
12. **Documentation update required:** Yes, after regenerated OpenAPI and tests confirm the corrected contract; existing `201` wording then becomes verified rather than source-only.
13. **Karam approval required:** Yes, because backend/OpenAPI source and tests would change.
14. **Risk if unresolved:** Payment clients may treat a valid response incorrectly or generate no return type.
15. **Recommended priority:** `P0`.

**Classification:** `COMBINATION OF THE ABOVE`.

**Proposed files/symbols:**

- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`, `CreateMyPaymentOrder` mapping.
- Payment endpoint tests under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/`.

**Required tests:** Assert `201`, `Location`, raw JSON response shape, OpenAPI status, and `PaymentOrderDto` schema reference.

### CST-002: Payment response schemas

1. **ID:** `CST-002`
2. **Area:** Payment order creation, checkout initialization, and Development Sandbox completion response contracts.
3. **Backend source evidence:** `CreateMyPaymentOrder` returns `PaymentOrderDto`; `StartMyPaymentCheckout` returns the `PaymentCheckoutSessionDto` handler result and sets `Cache-Control: no-store`; `CompleteSandboxPaymentCheckout` returns `PaymentCompletionDto` and sets `Cache-Control: no-store`.
4. **Runtime evidence:** The operations were not invoked because they mutate orders, checkout sessions, or fulfillment. Startup/runtime source is authoritative until focused authorized tests execute them.
5. **OpenAPI evidence:** Payment operations document status-only responses. `PaymentOrderDto`, `PaymentCheckoutSessionDto`, and `PaymentCompletionDto` are absent from `components.schemas`.
6. **Existing frontend documentation evidence:** All three frontend evidence documents identify these DTOs, and the baseline records the no-store requirement.
7. **Exact discrepancy:** Backend handlers return typed DTO bodies, but generated OpenAPI omits their schemas and most operation-specific error/status metadata.
8. **Impact on Angular/frontend:** The required explicit payment TypeScript response models cannot be generated or checked against OpenAPI.
9. **Recommended correction:** Add explicit typed response metadata for all payment operations, success statuses, Problem Details responses, and no-store headers. Keep Development/Test Sandbox scope explicit.
10. **Backend change required:** Yes, presentation metadata and focused tests; no payment business behavior change.
11. **OpenAPI configuration change required:** Yes.
12. **Documentation update required:** Yes, after regeneration confirms the exact schemas/statuses/headers.
13. **Karam approval required:** Yes.
14. **Risk if unresolved:** Highest-risk financial and entitlement workflows remain untyped or require guessed contracts.
15. **Recommended priority:** `P0`.

**Classification:** `COMBINATION OF THE ABOVE`.

**Proposed files/symbols:**

- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`: `CreateMyPaymentOrder`, `StartMyPaymentCheckout`, `CompleteSandboxPaymentCheckout`, and related payment mappings.
- Existing payment endpoint/integration tests.

**Required tests:** Verify exact status, content type, DTO schema, no-store header, authenticated behavior, documented `400/401/404/409/503` where applicable, and absence of sensitive/provider-internal fields.

### CST-003: Protected-operation security metadata

1. **ID:** `CST-003`
2. **Area:** OpenAPI authentication requirements.
3. **Backend source evidence:** Nurse package operations call `.RequireAuthorization()`; admin operations call exact `.RequirePermission(...)`; Sandbox completion calls `.RequireAuthorization()`.
4. **Runtime evidence:** Anonymous nurse/admin GET probes returned `401` with `WWW-Authenticate: Bearer`.
5. **OpenAPI evidence:** `components.securitySchemes.Bearer` exists, but protected operations contain no `security` requirement. No global `security` requirement exists. Public and protected operations are indistinguishable by security metadata.
6. **Existing frontend documentation evidence:** The evidence packet/index/baseline correctly identify authenticated and permission-protected operations from source/tests.
7. **Exact discrepancy:** Runtime and endpoint metadata enforce authorization, while generated OpenAPI advertises no operation as requiring Bearer authentication.
8. **Impact on Angular/frontend:** Client generation and contract tooling cannot identify where credentials are required; security-aware integration checks cannot validate public/protected boundaries.
9. **Recommended correction:** Add an OpenAPI operation transformer that detects authorization metadata and applies the Bearer security requirement only to protected operations. Preserve `.AllowAnonymous()` operations without that requirement. Permissions remain descriptions/extensions, not client-side security enforcement.
10. **Backend change required:** No runtime authorization behavior change; OpenAPI transformer/test code changes are required.
11. **OpenAPI configuration change required:** Yes.
12. **Documentation update required:** Yes, after regenerated evidence confirms operation-level security.
13. **Karam approval required:** Yes.
14. **Risk if unresolved:** Generated clients may omit auth behavior and contract reviews may misclassify protected data as public.
15. **Recommended priority:** `P0`.

**Classification:** `OPENAPI FIX REQUIRED`.

**Proposed files/symbols:**

- `backend/src/NursingPlatform.WebApi/Extensions/ServiceCollectionExtensions.cs`, `AddOpenApi` configuration.
- OpenAPI-focused WebApi tests.

**Required tests:** Assert anonymous catalog operations have no Bearer requirement and representative nurse/admin/Sandbox operations do.

### CST-004: Runtime `401` response contract

1. **ID:** `CST-004`
2. **Area:** Authentication challenge response.
3. **Backend source evidence:** `UseAuthentication()` and `UseAuthorization()` run after custom exception and request logging middleware. JWT challenge responses are produced by authentication middleware, not by throwing `UnauthorizedAccessException` through `ExceptionMiddleware`.
4. **Runtime evidence:** Anonymous protected probes returned `401`, empty body, no Problem Details content type, and `WWW-Authenticate: Bearer`.
5. **OpenAPI evidence:** Protected package endpoints document `401 application/problem+json` using `ProblemDetails`.
6. **Existing frontend documentation evidence:** The index states middleware maps unauthorized exceptions and metadata declares `401`, but does not distinguish authentication challenges from exception-generated `401` responses. The baseline implies shared Problem Details behavior too broadly.
7. **Exact discrepancy:** OpenAPI promises a Problem Details body that the normal authentication challenge does not return.
8. **Impact on Angular/frontend:** A typed error parser may expect a body that is absent. Authentication handling must rely on status/header, not body fields.
9. **Recommended correction:** First make an explicit contract decision: either document empty challenge responses accurately, or intentionally configure JWT challenge/forbidden events to emit the approved Problem Details shape consistently. Do not change runtime security behavior without approval. The minimal correction is OpenAPI documentation matching current empty `401` plus `WWW-Authenticate`; consistent Problem Details is a separate security/presentation decision.
10. **Backend change required:** Only if consistent Problem Details challenge bodies are chosen.
11. **OpenAPI configuration change required:** Yes in either outcome.
12. **Documentation update required:** Yes.
13. **Karam approval required:** Yes, because the authoritative frontend-facing `401` contract must be selected.
14. **Risk if unresolved:** Error parsing failures, misleading generated types, and inconsistent authentication UX.
15. **Recommended priority:** `P0`.

**Classification:** `COMBINATION OF THE ABOVE`.

**Dependency order:** Governance/security contract decision, then runtime implementation if selected, then OpenAPI metadata, tests, and frontend evidence updates.

### CST-005: Problem Details extensions

1. **ID:** `CST-005`
2. **Area:** Structured error schema.
3. **Backend source evidence:** `ExceptionMiddleware` always writes `type`, `title`, `status`, `detail`, and `traceId`; validation adds `errors`; checkout-in-progress adds `retryAfterSeconds` and `Retry-After`; package exam/report conflicts add `code`.
4. **Runtime evidence:** Observed `400` and `404` bodies included `traceId`. Existing endpoint tests prove conflict `code` and retry-after extensions.
5. **OpenAPI evidence:** `ProblemDetails` includes only standard nullable fields; `HttpValidationProblemDetails` adds `errors`. Neither describes `traceId`, `code`, or `retryAfterSeconds`; response headers are incomplete.
6. **Existing frontend documentation evidence:** Frontend rules/architecture require explicit support for `traceId`, validation `errors`, `retryAfterSeconds`, `Retry-After`, and documented codes. The API index records these runtime extensions.
7. **Exact discrepancy:** Runtime structured extensions are not represented in generated schemas.
8. **Impact on Angular/frontend:** The required typed Problem Details hierarchy cannot be derived safely; code-based conflict handling would require casts or guessed fields.
9. **Recommended correction:** Define explicit API error DTO/schema types: base Problem Details, validation Problem Details, coded conflict Problem Details, and retryable conflict Problem Details. Map each endpoint response to the narrow applicable schema and document `Retry-After` where applicable.
10. **Backend change required:** Presentation/schema types and metadata; runtime payload values may remain unchanged.
11. **OpenAPI configuration change required:** Yes.
12. **Documentation update required:** Yes.
13. **Karam approval required:** Yes.
14. **Risk if unresolved:** Unsafe casts, branching on human-readable text, and incomplete support diagnostics.
15. **Recommended priority:** `P0`.

**Classification:** `COMBINATION OF THE ABOVE`.

**Proposed files/symbols:**

- WebApi error contract DTO location to be selected under the existing WebApi presentation boundary.
- `ExceptionMiddleware.cs` only if serialization is moved to typed response models.
- Preparation Package endpoint metadata helpers and payment endpoint metadata.
- Error/OpenAPI endpoint tests.

### CST-006: Pagination and FluentValidation invocation

1. **ID:** `CST-006`
2. **Area:** Request validation pipeline.
3. **Backend source evidence:** `Application.DependencyInjection.AddApplication` registers validators with `AddValidatorsFromAssembly`; no MediatR `IPipelineBehavior`, endpoint filter, or explicit validator invocation was found. Catalog and entitlement validators require `Page >= 1` and `PageSize` from 1 through 100.
4. **Runtime evidence:** Catalog `page=0` returned `200`; `pageSize=101` returned `200`; malformed `page=not-an-int` returned `400` from Minimal API binding.
5. **OpenAPI evidence:** Query parameters describe integer/string binding syntax but no minimum/maximum constraints; endpoint metadata advertises validation `400`.
6. **Existing frontend documentation evidence:** The evidence packet records validator rules; the API index explicitly left runtime invocation open; the baseline warns runtime behavior is unproven.
7. **Exact discrepancy:** Model binding executes, but registered FluentValidation rules do not execute in the request/MediatR path. Documented limits are therefore not runtime API behavior.
8. **Impact on Angular/frontend:** Frontend limits cannot be treated as authoritative; invalid pagination may reach handlers and create inconsistent state/load.
9. **Recommended correction:** Add one repository-standard validation pipeline mechanism, preferably a MediatR validation behavior covering commands/queries before handlers, with deterministic aggregation into `ValidationException`. Avoid duplicating validators in endpoint filters. Ensure OpenAPI constraints are emitted from an intentional source or explicit metadata.
10. **Backend change required:** Yes.
11. **OpenAPI configuration change required:** Yes, to expose enforced constraints accurately.
12. **Documentation update required:** Yes, after runtime tests prove invocation.
13. **Karam approval required:** Yes.
14. **Risk if unresolved:** Server validation is not authoritative despite project policy; mutation and paging contracts remain unsafe.
15. **Recommended priority:** `P0`.

**Classification:** `BACKEND FIX REQUIRED`.

**Proposed files/symbols:**

- `backend/src/NursingPlatform.Application/DependencyInjection.cs`, MediatR registration.
- New focused validation pipeline behavior under Application common behaviors, following Clean Architecture.
- Existing validators remain unchanged unless a proven defect exists.
- Application and WebApi tests for representative query, route-derived command, body command, and admin request validation.

**Required tests:** Prove `page=0` and `pageSize=101` return `400` with structured errors; malformed binding remains `400`; handlers are not invoked on validation failure; valid requests continue normally.

### CST-007: Numeric OpenAPI unions

1. **ID:** `CST-007`
2. **Area:** Numeric schema generation.
3. **Backend source evidence:** DTOs use C# `int`, `long`, and `decimal` properties, including pagination/counts, minor-unit prices, scores, and percentages.
4. **Runtime evidence:** Observed pagination JSON serialized `page`, `pageSize`, `totalCount`, and `totalPages` as JSON numbers.
5. **OpenAPI evidence:** Many `int`/`long` properties are emitted as `type: [integer, string]`; decimal percentages are emitted as `type: [number, string]`, with numeric patterns. Affected examples include offer counts/duration/`priceAmountMinor`, pagination fields, practice counters, report score/count/percentage, version numbers, sort orders, and admin offer amount/duration.
6. **Existing frontend documentation evidence:** Frontend architecture requires actual backend/OpenAPI enum and pagination representations and forbids guessing. The baseline identifies server-owned integer minor units.
7. **Exact discrepancy:** OpenAPI permits strings for C# numeric properties while observed System.Text.Json output is numeric.
8. **Impact on Angular/frontend:** A generator may create `number | string`, weakening end-to-end type safety and forcing normalization/casts not justified by runtime behavior.
9. **Recommended correction:** Investigate the Microsoft.OpenApi 2.7.5/.NET 10 schema-generation compatibility and numeric handling. Pin compatible package versions or add a schema transformer that emits the actual JSON number type for non-string numeric DTO properties. Verify large `Int64` JavaScript precision requirements separately; do not silently map financial minor units until the accepted transport representation is explicit.
10. **Backend change required:** No runtime serialization change is necessarily required; package/configuration or transformer code may be required.
11. **OpenAPI configuration change required:** Yes.
12. **Documentation update required:** Yes, after corrected artifact capture.
13. **Karam approval required:** Yes, especially for the `Int64`/JavaScript precision decision.
14. **Risk if unresolved:** Polluted generated models, precision risk, and arbitrary frontend coercion.
15. **Recommended priority:** `P0`.

**Classification:** `OPENAPI FIX REQUIRED`.

**Required tests:** OpenAPI schema assertions for representative `int`, `long`, and `decimal`; runtime JSON assertions; boundary tests for money values and an explicit JavaScript-safe transport decision.

### CST-008: Required and nullability metadata

1. **ID:** `CST-008`
2. **Area:** Required properties and nullable fields.
3. **Backend source evidence:** Nullable reference/value types use `?`; non-null strings often initialize to `string.Empty`; collections initialize to empty; some required response members use non-null initialization or `null!`; request requiredness is also enforced by validators.
4. **Runtime evidence:** Empty catalog responses prove pagination members are present, but do not prove every DTO member's presence/omission behavior. No serializer ignore policy was found in the reviewed OpenAPI evidence.
5. **OpenAPI evidence:** Schemas generally have no `required` arrays. Nullable properties are represented with `null` unions, but non-null properties are not marked required.
6. **Existing frontend documentation evidence:** The API index correctly states nullability/requiredness remained OpenAPI-unconfirmed; frontend architecture requires nullable fields to be verified before generator approval.
7. **Exact discrepancy:** OpenAPI distinguishes some nullable types but does not state which non-null properties are guaranteed present. Validator-required request fields are also not consistently reflected.
8. **Impact on Angular/frontend:** A generator may mark every property optional, preventing strict DTO-to-TypeScript fidelity and allowing invalid frontend state.
9. **Recommended correction:** Establish explicit API DTO requiredness using supported .NET/OpenAPI metadata. Prefer C# `required` members or constructor/record contracts for genuinely required API properties where compatible, explicit nullable properties for optional data, and schema transformation only where source metadata cannot express the contract. Request requiredness must match runtime validator behavior after `CST-006`.
10. **Backend change required:** Likely DTO declaration and tests; no business behavior change intended.
11. **OpenAPI configuration change required:** Likely yes.
12. **Documentation update required:** Yes after property-by-property verification.
13. **Karam approval required:** Yes.
14. **Risk if unresolved:** Generated TypeScript contracts become broadly optional and cannot enforce API guarantees.
15. **Recommended priority:** `P0`.

**Classification:** `COMBINATION OF THE ABOVE`.

**Dependency order:** Fix runtime validation, decide DTO requiredness, correct OpenAPI generation, capture artifact, then update frontend evidence.

### CST-009: Exception and request logging status

1. **ID:** `CST-009`
2. **Area:** Serilog request status for handled exceptions.
3. **Backend source evidence:** Pipeline order is `ExceptionMiddleware`, then `UseSerilogRequestLogging`, then endpoint/auth middleware. Exceptions propagate through Serilog request logging before the outer exception middleware converts them to final `400/404/409/...` responses.
4. **Runtime evidence:** Client responses were correct `400` and `404`, while Serilog logged those requests as `500` before `ExceptionMiddleware` wrote the final response.
5. **OpenAPI evidence:** OpenAPI documents the intended client statuses and is unaffected by log event status.
6. **Existing frontend documentation evidence:** Existing documents describe final response statuses but do not record false `500` logging.
7. **Exact discrepancy:** This is an observability discrepancy, not an HTTP contract discrepancy.
8. **Impact on Angular/frontend:** No direct response-shape effect; false server-error telemetry can obscure frontend/API diagnosis and inflate error alerts.
9. **Recommended correction:** Reorder request logging outside the exception-handling middleware so it observes the final response status, or configure an exception/status-level strategy that records the mapped status. Preserve exception logging once, avoiding duplicate error events.
10. **Backend change required:** Yes, middleware ordering/logging behavior.
11. **OpenAPI configuration change required:** No.
12. **Documentation update required:** Optional operational documentation after behavior is fixed.
13. **Karam approval required:** Yes.
14. **Risk if unresolved:** Misleading monitoring, false incident signals, and harder support correlation.
15. **Recommended priority:** `P2`.

**Classification:** `BACKEND FIX REQUIRED`.

**Proposed files/symbols:** `ApplicationBuilderExtensions.UseApplicationPipeline` and exception/request logging tests.

### CST-010: Rate limiting and abuse protection

1. **ID:** `CST-010`
2. **Area:** Rate limiting.
3. **Backend source evidence:** No `AddRateLimiter`, `UseRateLimiter`, `RequireRateLimiting`, limiter partition/policy, or endpoint-specific limiter was found. `Retry-After` and `retryAfterSeconds` are used only for checkout-in-progress `409`, not for rate limiting.
4. **Runtime evidence:** No rate-limiter behavior or `429` response was observed.
5. **OpenAPI evidence:** No inspected Preparation Package operation documents `429`.
6. **Existing frontend documentation evidence:** Frontend rules define handling only when documented; the API index correctly says package rate limiting is not evidenced.
7. **Exact discrepancy:** No source/runtime/OpenAPI contradiction exists. Abuse protection is absent or not evidenced, while future frontend architecture anticipates possible `429` handling.
8. **Impact on Angular/frontend:** No `429` UI may be claimed for current operations. Future behavior requires an explicit contract before implementation.
9. **Recommended correction:** Make a separate security/business decision and threat-model endpoint families before implementation. Recommended candidates are authentication login/refresh, verification/recovery, public catalog/search, checkout/order creation, Sandbox completion in non-production environments, expensive report generation/read paths, and file upload/download operations. Define partition keys, limits/windows, distributed deployment behavior, `Retry-After`, Problem Details schema, observability, and exemptions.
10. **Backend change required:** Not in this task; a later approved security implementation would be required.
11. **OpenAPI configuration change required:** Only after policies are implemented.
12. **Documentation update required:** Only after a decision/implementation establishes actual behavior.
13. **Karam approval required:** Yes, with security/product input.
14. **Risk if unresolved:** Abuse, credential attacks, resource exhaustion, payment/report amplification, and no stable frontend recovery contract.
15. **Recommended priority:** `P1` for authentication/payment/expensive operations; `P2` for lower-risk reads.

**Classification:** `GOVERNANCE/BUSINESS DECISION REQUIRED`.

## 5. Frontend documentation discrepancies

The following existing files require a separately authorized documentation reconciliation only after the relevant backend/OpenAPI decisions are implemented and verified:

| File | Current discrepancy | Proposed bounded update |
|---|---|---|
| `docs/frontend/design/evidence/preparation-package-phase-1-evidence-packet.md` | Says no generated artifact exists; records validator rules without live non-invocation evidence. | Record captured artifact revision/hash, runtime validation result, payment/OpenAPI gaps, security metadata gap, and actual `401` behavior. |
| `docs/frontend/design/integration/preparation-package-api-validation-error-index.md` | OpenAPI remains marked unavailable; `401` discussion conflates middleware exceptions with authentication challenges. | Replace unavailable status with captured evidence and preserve explicit blockers for schemas/security/errors/validation. |
| `docs/frontend/design/integration/preparation-package-frontend-contract-baseline.md` | Marks OpenAPI details unknown and broadly describes Problem Details; payment auth/status/schema evidence is incomplete. | Reclassify verified schemas and list exact live discrepancies without declaring contract readiness. |
| `docs/frontend/design/integration/development-openapi-capture-procedure.md` | Describes capture as blocked and proposes an unverified future path. | Record the authorized process-local startup/capture evidence only if a documentation task approves preserving that operational procedure and its security handling. |
| `docs/frontend/design/integration/development-openapi-capture-investigation.md` | Correctly records the earlier passive blocker but is now historical relative to the authorized runtime capture. | Preserve as historical investigation; add a superseding reference only if explicitly authorized. |
| `docs/frontend/design/governance/open-questions.md` | `OPEN-PH1-001/002/005` do not contain the newly captured evidence/status. | Governance task must decide whether evidence narrows or resolves each question; this proposal does not close them. |

No existing documentation was modified by this task.

## 6. Proposed authoritative contract strategy

The target chain remains:

```text
Backend DTO
-> runtime behavior
-> generated OpenAPI
-> explicit TypeScript response model
-> typed API adapter
-> typed feature state
-> typed component inputs
```

Proposed stabilization gates:

1. Backend runtime behavior and security rules are authoritative; tests must prove statuses, bodies, headers, validation, ownership, and sensitive-field exclusions.
2. OpenAPI must describe that proven behavior exactly, including response DTOs, operation security, error variants, numeric transport, and required/nullability metadata.
3. Capture and hash a fresh Development OpenAPI artifact from the exact verified backend revision.
4. Run a source/runtime/OpenAPI consistency audit with deterministic assertions.
5. Reconcile frontend evidence and governance questions through separately authorized documentation/governance tasks.
6. Only then run a generator evaluation and define explicit TypeScript response/error contracts. No `any`, `as any`, suppression, arbitrary cast, guessed field, or silently optional property is acceptable.
7. Angular remains an untrusted client; backend authentication, permissions, ownership, entitlement, payment, exam access, and sensitive-data boundaries remain authoritative.

## 7. Recommended implementation order

1. `CST-006`: establish runtime validation invocation and tests.
2. `CST-001` and `CST-002`: correct payment response metadata/status/schema coverage.
3. `CST-004`: decide the authoritative authentication challenge contract.
4. `CST-005`: define typed Problem Details variants and response headers.
5. `CST-003`: add operation-level OpenAPI Bearer requirements.
6. `CST-007` and `CST-008`: correct numeric and required/nullability schemas.
7. Re-capture OpenAPI and run deterministic contract comparison.
8. Reconcile authorized frontend evidence/governance documents.
9. Address `CST-009` observability independently.
10. Run a separately approved `CST-010` security/rate-limit design before implementation.

## 8. Final decision

The Preparation Package API contract is **not ready for TypeScript generation**. P0 blockers prevent trustworthy generation of payment responses, authentication requirements, error variants, validation behavior, numeric properties, and required properties.

Contract stabilization proposal complete; implementation remains blocked until the identified contract corrections are explicitly authorized and completed.
