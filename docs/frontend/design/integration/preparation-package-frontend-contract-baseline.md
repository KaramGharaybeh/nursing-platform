# Preparation Package Frontend Contract Baseline

## 1. Purpose and authority

This is a documentation-only baseline for Preparation Package frontend candidate areas. It consolidates static repository evidence at backend handoff baseline `8439511`; it is not a generated OpenAPI contract, page specification, route registry, API-client decision, Angular authorization, Penpot authorization, or governance decision.

Implemented backend source and a generated Development OpenAPI document are the runtime-contract authorities. This baseline records source and test evidence where a generated document cannot safely be captured. It does not replace the missing artifact.

## 2. Evidence classification

| Classification | Meaning |
|---|---|
| **VERIFIED** | Directly traceable to endpoint mapping, DTO, validator, middleware, or existing automated test source. |
| **INFERRED / SAFE DESIGN ASSUMPTION** | A frontend safety rule from `frontend-project-rules.md` or `frontend-architecture.md`; it is not a backend contract claim. |
| **UNKNOWN / BLOCKED** | Requires generated Development OpenAPI, runtime evidence, a Karam decision, or a separately approved evidence task. |

All listed paths are relative to `/api/v1`. Endpoint response/status entries below are explicit endpoint metadata or handler results, not generated OpenAPI output.

## 3. Shared verified contract evidence

### VERIFIED

- `PreparationPackageEndpointExtensions.cs` maps public catalog, nurse entitlement, and administration endpoints beneath the `/api/v1` API group.
- Nurse package endpoints use `.RequireAuthorization()`. Administration endpoints use exact `RequirePermission(...)` calls. Public catalog endpoints use `.AllowAnonymous()`.
- `ExceptionMiddleware.cs` writes `application/problem+json` with `type`, `title`, `status`, `detail`, and `traceId`. A thrown `ValidationException` additionally has `errors`; package exam-session and package-report conflict exceptions additionally have `code`.
- The middleware maps `ValidationException` to `400`, missing resources to `404`, forbidden access to `403`, unauthorized access to `401`, package conflict exceptions and `InvalidOperationException` to `409`, provider unavailable to `503`, and unhandled exceptions to `500`.
- Tests establish generic ownership-preserving `404` behavior for nurse-owned entitlements, practice progress, exam sessions, and reports; UI must not disclose whether another user's resource exists.
- `WithNurseEntitlementMetadata` explicitly documents `200`, validation `400`, `401`, and optional `404`/`409`; `WithAdminMetadata` documents `200`, validation `400`, `401`, `403`, and optional `404`/`409`; created admin endpoints document `201`, validation `400`, `401`, `403`, and optional `404`.

### INFERRED / SAFE DESIGN ASSUMPTION

- A network-backed read needs a pending/loading state, a successful populated state, and a failure state; an empty state is shown only if the documented response shape can contain an empty collection.
- A mutation needs a visible submitting state and a persistent contextual result for `400`, `401`, `403`, `404`, `409`, `500`, or `503` where evidenced. Mutations must not be blindly retried.
- `400` field errors may be mapped only when the runtime `errors` keys and JSON naming are confirmed. User-facing behavior must not branch on Problem Details `title` or `detail` text.
- `401` follows the centralized authentication policy; `403` is an unauthorized presentation state; ownership-hidden `404` is presented generically; `409` is persistent contextual conflict feedback.

### UNKNOWN / BLOCKED

- `OPEN-PH1-001`: runtime FluentValidation invocation is proven for public catalog pagination only; other validated Minimal API operations remain unproven.
- `OPEN-PH1-002`: a Development OpenAPI artifact was captured from the uncommitted contract-stabilization working tree based on `7768f27` at `http://localhost:5167/openapi/v1.json` with SHA-256 `c0c4f965215c0b58bcbf59d07db9e85c2fcc6b5a0dd91dc43f8bdd6b3f4e95a1`; governance closure remains separate.
- `OPEN-PH1-003`: no `422` mapping or package endpoint evidence exists.
- `OPEN-PH1-004`: no package-specific administration payment endpoint or permission is evidenced.
- `OPEN-PH1-005`: the generated revision evidences listed operation paths, protected-operation Bearer metadata, and payment success contracts; unresolved schema discrepancies still block frontend generation.
- Exact OpenAPI operation IDs, generated schemas, JSON property casing, nullable/required representation, enum serialization, content types, response headers other than source-tested cases, and complete response matrices remain unverified.

## 4. Candidate-area contract matrix

### 4.1 Public catalog

| Item | Baseline |
|---|---|
| **VERIFIED endpoint** | `GET /preparation-packages/offers`; anonymous. Optional source-bound query parameters: `page`, `pageSize`, `countryId`, `examCategoryId`. |
| **VERIFIED response/status** | `200` with `PaginatedResult<PreparationPackageOfferListItemDto>`; endpoint metadata declares validation `400`. |
| **VERIFIED response evidence** | Offer DTO source identifies id, title, slug, optional summary, country/category/exam identity and names, material/practice counts, access duration, minor-unit price, and currency. Catalog tests prove protected exam content, authorization/provenance, provider data, tokens, scores, and reports are absent from catalog JSON. |
| **VERIFIED validation/error evidence** | `ListPreparationPackageOffersQueryValidator` requires page at least 1 and page size 1-100. The endpoint invokes it before `ISender.Send`; focused tests prove the handler is not called on invalid input. Live `page=0` and `pageSize=101` requests return `400`. |
| **Justified states** | Loading, populated, empty collection, `400`, and generic unexpected failure. |
| **UNKNOWN / BLOCKED** | OpenAPI does not yet encode the pagination minimum/maximum constraints; numeric and required/nullability schema issues remain. |
| **Readiness** | Static evidence is sufficient for bounded candidate analysis; blocked for page specification or implementation until OpenAPI/runtime gates are resolved. |

### 4.2 Package detail

| Item | Baseline |
|---|---|
| **VERIFIED endpoint** | `GET /preparation-packages/offers/{slug}`; anonymous. |
| **VERIFIED response/status** | `200` with `PreparationPackageOfferDetailDto`; endpoint metadata declares `400` and `404`. |
| **VERIFIED response evidence** | Detail DTO inherits the catalog offer DTO and adds `Components`, each with name, count, and optional summary. Catalog tests prove safe response JSON. |
| **Justified states** | Loading, populated, generic not-found, `400`, and unexpected failure. |
| **UNKNOWN / BLOCKED** | Runtime slug validation/normalization, generated schema, and OpenAPI representation. |
| **Readiness** | Static evidence is sufficient for bounded candidate analysis; blocked for page specification or implementation. |

### 4.3 Payment and Development/Test Sandbox handoff

| Item | Baseline |
|---|---|
| **VERIFIED endpoints** | `POST /me/nurse-profile/payment/orders` accepts `CreatePaymentOrderRequest`; source/runtime tests return `201` with `PaymentOrderDto` and `Location`, and OpenAPI now documents that contract. `POST /me/nurse-profile/payment/orders/{orderId}/checkout` returns `200` with `PaymentCheckoutSessionDto`; `POST /dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete` is Development/Test-only and returns `200` with `PaymentCompletionDto`. Both responses set and document `Cache-Control: no-store`. |
| **VERIFIED request/response evidence** | `CreatePaymentOrderRequest` has nullable `ProductId` and `PackageOfferId`; its validator requires exactly one selected non-empty source. Package flow uses `PackageOfferId`. `PaymentOrderDto`, `PaymentCheckoutSessionDto`, and `PaymentCompletionDto` exist; completion includes package-entitlement summaries. |
| **VERIFIED error evidence** | Static evidence records validator `400`, missing/other-owner resources `404`, and conflict conditions `409` for package order/completion paths. Provider-unavailable middleware mapping is `503`; operation-specific runtime coverage is incomplete. |
| **Justified states** | Submitting, server-confirmed order/checkout result, generic ownership-preserving not-found, persistent conflict, service-unavailable where confirmed, and no-store short-lived completion state. No optimistic paid/access state. |
| **UNKNOWN / BLOCKED** | Generated OpenAPI now marks protected payment operations with Bearer requirements and provides the three success schemas. Problem Details extensions, numeric unions, required-property metadata, production provider behavior, and the complete payment error contract remain unresolved. Sandbox completion must not be treated as a Production flow. |
| **Readiness** | Candidate evidence only. Blocked for frontend contract finalization by `OPEN-PH1-002`, `OPEN-PH1-004`, `OPEN-PH1-005`, and production payment scope. |

### 4.4 Entitlements and rights

| Item | Baseline |
|---|---|
| **VERIFIED endpoints** | `GET /me/nurse-profile/preparation-packages/entitlements` with optional `page`/`pageSize`; `GET /me/nurse-profile/preparation-packages/entitlements/{id:guid}`; authenticated. |
| **VERIFIED response/status** | List returns `200` with `PaginatedResult<PackageEntitlementListItemDto>`; detail returns `200` with `PackageEntitlementDetailDto`. Metadata declares `400`, `401`, and detail `404`. |
| **VERIFIED response evidence** | DTOs expose entitlement/package identity, access window/status, and benefit-right summaries. Detail adds a purchased snapshot. Rights expose type, status, availability, dormancy, and access windows. Tests prove `401`, ownership behavior, four right summaries, and exclusion of internal identifiers, tokens, provider data, and protected content. |
| **Justified states** | Loading, populated, empty list, generic ownership-preserving not-found for detail, `401`, `400`, and unexpected failure. Right availability is backend-authoritative display information, not a client authorization decision. |
| **UNKNOWN / BLOCKED** | Runtime validation and exact JSON/OpenAPI representation. There is no package workspace aggregate contract. |
| **Readiness** | Static candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.5 Practice progress

| Item | Baseline |
|---|---|
| **VERIFIED endpoints** | `GET /me/nurse-profile/preparation-packages/entitlements/{entitlementId:guid}/practice-progress`; `POST /me/nurse-profile/preparation-packages/entitlements/{entitlementId:guid}/practice-progress/items/{practiceItemId:guid}/answer`; authenticated. |
| **VERIFIED request/response/status** | Read returns `200` with `PackagePracticeProgressSummaryDto`; submission accepts `SubmitPackagePracticeAnswerRequest` with `SelectedPracticeAnswerOptionId` and returns `200` with `PackagePracticeAnswerSubmissionDto`. Metadata declares `400`, `401`, `404`; answer submission also declares `409`. |
| **VERIFIED behavior** | Tests prove unauthenticated `401`, ownership-hidden `404`, historical read after expiry, expired write `409`, and correct/incorrect/unanswered counters. Tests also prove no official exam/internal-right exposure. |
| **Justified states** | Loading, populated progress, submitting answer, visible unsaved/failed answer result, generic not-found, persistent `409` expiry conflict, `401`, `400`, and unexpected failure. Do not represent practice as consuming an exam attempt. |
| **UNKNOWN / BLOCKED** | Runtime validation/error-key mapping, generated schema, exact item-content contract, and OpenAPI representation. Adaptive practice and retraining are not evidenced. |
| **Readiness** | Static candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.6 Package exam start/resume

| Item | Baseline |
|---|---|
| **VERIFIED endpoint** | `POST /me/nurse-profile/preparation-packages/entitlements/{entitlementId:guid}/exam-session`; authenticated; no request body. |
| **VERIFIED response/status** | `200` with `PackageExamSessionStartDto`; metadata declares `400`, `401`, `404`, and `409`. |
| **VERIFIED response/behavior** | The DTO exposes session, source, entitlement, included exam/version, offer, and access-end evidence. Tests prove generic `404`, response-sensitive-field exclusion, same-entitlement retry returning `200`, and `409` codes: `package-entitlement-inactive`, `package-attempt-right-missing`, `package-attempt-consumed`, `package-exam-version-unavailable`, and `exam-session-source-conflict`. |
| **Justified states** | Submitting, server-confirmed start/resume result, generic not-found, persistent conflict keyed only by the documented `code`, `401`, `400`, and unexpected failure. No optimistic session-start state or blind retry. |
| **UNKNOWN / BLOCKED** | The nested `ExamSessionDto` contract, session-question experience, timer/resume/rationale rules, OpenAPI schema, and runtime validation remain outside this baseline. `OPEN-009` remains open. |
| **Readiness** | Static start/resume candidate evidence is strong; blocked for page specification or implementation. |

### 4.7 Analytical report

| Item | Baseline |
|---|---|
| **VERIFIED endpoint** | `GET /me/nurse-profile/preparation-packages/exam-sessions/{sessionId:guid}/report`; authenticated. |
| **VERIFIED response/status** | `200` with `PackageAnalyticalReportDto`; metadata declares `400`, `401`, `404`, and `409`. |
| **VERIFIED response/behavior** | DTO source exposes report/session identity, generation/finalization evidence, score/count/percentage/pass values, topic results, and guidance items. Tests prove unauthenticated `401`, generic `404`, conflict `409`, repeated successful reads, post-expiry read behavior, and exclusion of question text, answer keys, correct answers/options, rationales, entitlement/right identifiers, provenance, and provider data. Documented conflict codes are `package-report-session-not-finalized`, `package-report-session-not-qualified`, `package-report-provenance-invalid`, `package-report-right-missing`, and `package-report-profile-incomplete`. |
| **Justified states** | Loading/generating read, populated immutable report, generic not-found, persistent documented-code conflict, `401`, `400`, and unexpected failure. Do not expose protected exam-review content. |
| **UNKNOWN / BLOCKED** | OpenAPI schema/enum serialization, runtime validation, and detailed report presentation contract remain unverified. |
| **Readiness** | Static candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.8 Admin reporting topics

| Item | Baseline |
|---|---|
| **VERIFIED endpoints and permission** | `GET/POST /admin/preparation-package/reporting-topics`, `PUT /admin/preparation-package/reporting-topics/{id:guid}`, and `POST /admin/preparation-package/reporting-topics/{id:guid}/archive`; each requires `ReportingTopics.Manage`. |
| **VERIFIED request/response/status** | List returns `200` `PaginatedResult<AdminReportingTopicDto>`; create accepts `CreateAdminReportingTopicRequest`, returns `201` `AdminReportingTopicDto`; update accepts `UpdateAdminReportingTopicRequest`, returns `200`; archive returns `200`. Metadata declares `400`, `401`, `403`; update/archive declare `404`; archive declares `409`. |
| **VERIFIED validation evidence** | Static validators exist for list/create/update/archive. Existing evidence identifies category, name, and slug validation, but not runtime invocation. |
| **Justified states** | Loading, populated, empty list, submitting mutation, `401`, `403`, generic `404`, `400`, archive conflict, and unexpected failure. |
| **UNKNOWN / BLOCKED** | Request-field JSON schema and error-key mapping; generated OpenAPI. |
| **Readiness** | Static candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.9 Admin reporting profiles

| Item | Baseline |
|---|---|
| **VERIFIED endpoints and permission** | `GET/POST /admin/preparation-package/reporting-profiles`, `GET /admin/preparation-package/reporting-profiles/{id:guid}`, and `POST /admin/preparation-package/reporting-profiles/{id:guid}/publish`; each requires `ReportingProfiles.Manage`. |
| **VERIFIED request/response/status** | List returns `200` `PaginatedResult<AdminReportingProfilePublicationDto>`; create accepts `CreateAdminReportingProfileRequest`, returns `201`; detail returns `200`; publish accepts `PublishAdminReportingProfileRequest`, returns `200`. Metadata declares `400`, `401`, `403`; detail/publish declare `404`; publish declares `409`. |
| **VERIFIED behavior** | Publication validation requires a published exact exam version and assignments for every active scored question. Dedicated permission tests exist. |
| **Justified states** | Loading, populated, empty list, submitting, `401`, `403`, generic `404`, `400`, persistent publish conflict, and unexpected failure. |
| **UNKNOWN / BLOCKED** | Runtime validation invocation, request schemas/error-key mapping, and generated OpenAPI. |
| **Readiness** | Static candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.10 Admin materials

| Item | Baseline |
|---|---|
| **VERIFIED endpoints and permission** | `GET/POST /admin/preparation-package/materials`; `POST /admin/preparation-package/materials/{materialId:guid}/versions`; `PUT /admin/preparation-package/materials/{materialId:guid}/versions/{versionId:guid}`; `POST` publish/retire lifecycle actions under the same version path. Each requires `StudyMaterials.Manage`. |
| **VERIFIED request/response/status** | List returns `200` `PaginatedResult<AdminStudyMaterialDto>`; create accepts `CreateAdminStudyMaterialRequest`, returns `201`; create-version accepts `CreateAdminStudyMaterialVersionRequest`, returns `201` `AdminStudyMaterialVersionDto`; update accepts `UpdateAdminStudyMaterialVersionRequest`, returns `200`; publish/retire return `200`. Metadata declares `400`, `401`, `403`; version creation/update/lifecycle declare `404`; update/lifecycle declare `409`. |
| **VERIFIED validation evidence** | Static validators cover material/version operations. Material-type content must be compatible and at least one reporting-topic mapping is required. |
| **Justified states** | Loading, populated, empty list, submitting, `401`, `403`, generic `404`, `400`, persistent lifecycle conflict, and unexpected failure. |
| **UNKNOWN / BLOCKED** | No nurse material-content/delivery/read route is mapped. `FileStorageKey`, URLs, and formatted text in admin DTOs do not establish public or nurse delivery behavior. Request schemas/error keys and OpenAPI remain unverified. |
| **Readiness** | Administration metadata/version candidate only; nurse material-reader design is blocked. Admin page specification or implementation remains blocked by OpenAPI/runtime gates. |

### 4.11 Admin practice collections

| Item | Baseline |
|---|---|
| **VERIFIED endpoints and permission** | `GET/POST /admin/preparation-package/practice-collections`; version create/update/publish/retire actions beneath `/admin/preparation-package/practice-collections/{collectionId:guid}/versions`; each requires `PracticeCollections.Manage`. |
| **VERIFIED request/response/status** | List returns `200` `PaginatedResult<AdminPracticeCollectionDto>`; create accepts `CreateAdminPracticeCollectionRequest`, returns `201`; version create/update use their named request DTOs and return `201`/`200` `AdminPracticeCollectionVersionDto`; publish/retire return `200`. Metadata declares `400`, `401`, `403`; relevant version operations declare `404`, and update/lifecycle declare `409`. |
| **VERIFIED validation and sensitivity evidence** | Static validation requires a reporting topic and exactly one correct option for each practice item. Admin DTOs include `IsCorrect` for authoring; tests establish that nurse practice responses do not expose official exam content. |
| **Justified states** | Loading, populated, empty list, submitting, `401`, `403`, generic `404`, `400`, persistent lifecycle conflict, and unexpected failure. |
| **UNKNOWN / BLOCKED** | Request schemas/error-key mapping, OpenAPI, and frontend disclosure rules for authoring-only correct-option data require a later approved admin feature specification. |
| **Readiness** | Static administration candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.12 Admin package composition and versioning

| Item | Baseline |
|---|---|
| **VERIFIED endpoints and permissions** | Definitions: `GET/POST /admin/preparation-package/packages`, `PUT /admin/preparation-package/packages/{id:guid}`. Versions: `POST /admin/preparation-package/packages/{packageId:guid}/versions`, `GET /admin/preparation-package/packages/{packageId:guid}/versions/{versionId:guid}/validation`, `POST` publish/retire actions under the same version path. List/validation require `PreparationPackages.View`; definition/version create/update require `PreparationPackages.Manage`; publish/retire require `PreparationPackages.Publish`. |
| **VERIFIED request/response/status** | Definition list returns `200` `PaginatedResult<AdminPreparationPackageDefinitionDto>`; definition create returns `201`; update returns `200`. Version create accepts `CreateAdminPreparationPackageVersionRequest`, returns `201` `AdminPreparationPackageVersionDto`; validation returns `200` `PackagePublicationValidationDto`; publish/retire return `200`. Metadata declares `400`, `401`, `403` where applicable, `404` for identified operations, and `409` for version publish/retire. |
| **VERIFIED validation/business evidence** | Published composition requires matching published exam/profile/practice/material components, unique positive material ordering, topic compatibility, and confirmed content isolation. Validation DTO has `IsValid` and a list of code/message issues. |
| **Justified states** | Loading, populated, empty list, submitting, validation-result display, `401`, `403`, generic `404`, `400`, persistent publish/retire conflict, and unexpected failure. |
| **UNKNOWN / BLOCKED** | Request schemas/error-key mapping, publication issue-code catalogue/semantics, OpenAPI, and runtime validation behavior remain unverified. |
| **Readiness** | Static administration candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

### 4.13 Admin offers

| Item | Baseline |
|---|---|
| **VERIFIED endpoints and permission** | `GET/POST /admin/preparation-package/offers`, `PUT /admin/preparation-package/offers/{id:guid}`, and `POST /admin/preparation-package/offers/{id:guid}/activate|deactivate`; each requires `PreparationPackageOffers.Manage`. |
| **VERIFIED request/response/status** | List returns `200` `PaginatedResult<AdminPreparationPackageOfferDto>`; create accepts `CreateAdminPreparationPackageOfferRequest`, returns `201`; update accepts `UpdateAdminPreparationPackageOfferRequest`, returns `200`; activate/deactivate return `200`. Metadata declares `400`, `401`, `403`; create/update/activate/deactivate declare `404`; update/activate declare `409`. |
| **VERIFIED validation/business evidence** | Static validators cover list/create/update/activate/deactivate. An offer references a published package version and carries price, currency, and access duration rather than independently selected content. |
| **Justified states** | Loading, populated, empty list, submitting, `401`, `403`, generic `404`, `400`, persistent update/activation conflict, and unexpected failure. |
| **UNKNOWN / BLOCKED** | Request schemas/error-key mapping, currency/amount display rules beyond server-owned minor units, OpenAPI, and runtime validation behavior remain unverified. |
| **Readiness** | Static administration candidate evidence is strong; blocked for page specification or implementation by OpenAPI/runtime gates. |

## 5. Cross-area exclusions

The following are not evidenced as implemented frontend candidate behavior and must not be designed as such from this baseline:

- Nurse material storage, upload, download, delivery, or reader functionality.
- Offline access, workspace/dashboard aggregation, adaptive practice, spaced repetition, or retraining.
- Employer package purchase, progress, or report visibility.
- Production payment-provider, webhook, refund, reconciliation, subscription, or administration-payment behavior.
- Angular implementation, API-client generation, page specifications, route registry, Penpot boards, or visual approval.

## 6. OpenAPI-dependent completion gaps

The following cannot safely be finalized until a generated Development OpenAPI document is captured from a verified backend revision without violating startup restrictions:

- Actual document URL, capture timestamp, artifact path, content hash, and document revision.
- Generated operation IDs, tags, request/response schemas, nullable/required properties, JSON names, enum serialization, content types, and complete status matrices.
- Exact request contract for checkout initialization and runtime presence/shape of validation `errors`.
- Confirmation that all static endpoint metadata appears as expected in the generated document.
- A source-to-OpenAPI comparison for every Preparation Package operation, required by `OPEN-PH1-005`.

## 7. Governance status

This baseline records evidence only. It does not update governance files or close questions.

- `OPEN-PH1-001` is narrowed: public catalog pagination validation is proven; other Minimal API validator invocation remains unproven.
- `OPEN-PH1-002` has capture evidence at revision `7768f27`; governance closure remains separate.
- `OPEN-PH1-003` remains open: no `422` evidence exists.
- `OPEN-PH1-004` remains open: package administration payment scope/permission is not evidenced.
- `OPEN-PH1-005` has generated path/security/payment evidence, but unresolved schema discrepancies still block contract finalization and governance closure.

## 8. Conclusion

The repository contains strong static evidence for the listed public, nurse, and administration candidate operations, including mapped paths, DTO types, explicit authorization/permission requirements, handler response metadata, middleware behavior, validators, and focused tests. It supports bounded contract discovery only.

No candidate area is ready for page specification, Angular implementation, API-client generation, or Penpot work. The missing Development OpenAPI capture and the remaining runtime/governance questions are blocking contract-finalization evidence, not gaps this document may infer away.
