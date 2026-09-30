# Preparation Package API, Validation, and Error Evidence Index

## 1. Purpose and non-authority

This is a repository evidence index for frontend-design integration. It is not a page specification, final route registry, API-client generation decision, Angular authorization, Penpot authorization, or backend change authorization. Source paths and tests describe the current implemented evidence; a generated Development OpenAPI artifact is still required before implementation contracts can be frozen.

## 2. Source snapshot

| Item | Evidence |
|---|---|
| Branch / HEAD | `feature/frontend-design-evidence-foundation` / `fd19271 docs: add preparation package frontend evidence packet` |
| G0 acceptance commit | `4073154` |
| Phase 1 evidence-packet commit | `fd19271` |
| Backend handoff baseline | `8439511` |
| Endpoint mappings | `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs`; payment mappings in `ApplicationBuilderExtensions.cs` |
| Contract/validator source | `backend/src/NursingPlatform.Application/PreparationPackages/**`; `backend/src/NursingPlatform.Application/Payments/**` |
| Error source | `backend/src/NursingPlatform.WebApi/Middleware/ExceptionMiddleware.cs` |
| Test source | WebApi integration, Application, Domain, and Infrastructure package tests under `backend/tests/` |

## 3. OpenAPI status

`ServiceCollectionExtensions.cs` calls `AddOpenApi`; `ApplicationBuilderExtensions.cs` calls `MapOpenApi()` only when `app.Environment.IsDevelopment()`. The authorized Development capture from the uncommitted contract-stabilization working tree based on `7768f27` produced OpenAPI `3.1.1` from `http://localhost:5167/openapi/v1.json`, temporary artifact `/tmp/opencode/development-openapi-stabilized.json`, SHA-256 `c0c4f965215c0b58bcbf59d07db9e85c2fcc6b5a0dd91dc43f8bdd6b3f4e95a1`.

The captured document verifies the Preparation Package operation paths, typed payment success schemas, `CreateMyPaymentOrder` as `201` with `Location`, checkout/Sandbox `Cache-Control`, and operation-level Bearer requirements for protected endpoints. It does not resolve Problem Details extensions, numeric schema unions, or required-property metadata. Governance status for `OPEN-PH1-002` and `OPEN-PH1-005` must be updated only in a separately authorized governance task.

## 4. API operation index

All paths are relative to `/api/v1`. Endpoint status declarations are from the mapping metadata; test status evidence is cited where available.

| Capability | Actor | Method | Path | Endpoint source | Auth / permission | Request / response source | Success | Error evidence | Tests | Open questions |
|---|---|---|---|---|---|---|---|---|---|---|
| Catalog list | Public | GET | `/preparation-packages/offers` | `MapPublicCatalogEndpoints` | Anonymous | Query: page/pageSize/countryId/examCategoryId; `PreparationPackageOfferListItemDto` | 200 | 400 | `PreparationPackageCatalogEndpointTests` | `OPEN-PH1-002`, `005` |
| Offer detail | Public | GET | `/preparation-packages/offers/{slug}` | same | Anonymous | `PreparationPackageOfferDetailDto` | 200 | 400, 404 | same | `OPEN-PH1-002`, `005` |
| Package order | Nurse | POST | `/me/nurse-profile/payment/orders` | `CreateMyPaymentOrder` | Authenticated nurse | `CreatePaymentOrderRequest`; `PaymentOrderDto` | 201 | 400, 404, 409 from validator/handler/middleware | `PaymentHandlerTests`, payment integration tests | `OPEN-PH1-002`, `004`, `005` |
| Sandbox completion | Nurse / system | POST | `/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete` | `CompleteSandboxPaymentCheckout` | Authenticated; Development/Test only | Path id; `PaymentCompletionDto` | 200 | 400, 404, 409; 500 fallback | payment integration/application tests | `OPEN-PH1-002`, `005` |
| Entitlement list | Nurse | GET | `/me/nurse-profile/preparation-packages/entitlements` | `MapNurseEntitlementEndpoints` | Authenticated | page/pageSize; `PaginatedResult<PackageEntitlementListItemDto>` | 200 | 400, 401, 403 handler path | `PackageEntitlementEndpointTests` | `OPEN-PH1-001`, `002`, `005` |
| Entitlement detail | Nurse | GET | `.../entitlements/{id}` | same | Authenticated | Path id; `PackageEntitlementDetailDto` | 200 | 400, 401, 403, 404 | same | `OPEN-PH1-001`, `002`, `005` |
| Practice progress | Nurse | GET | `.../entitlements/{entitlementId}/practice-progress` | same | Authenticated | Path id; `PackagePracticeProgressSummaryDto` | 200 | 400, 401, 404 | `PackagePracticeProgressEndpointTests` | `OPEN-PH1-001`, `002`, `005` |
| Practice answer | Nurse | POST | `.../practice-progress/items/{practiceItemId}/answer` | same | Authenticated | `SubmitPackagePracticeAnswerRequest`; `PackagePracticeAnswerSubmissionDto` | 200 | 400, 401, 404, 409 | same | `OPEN-PH1-001`, `002`, `005` |
| Package exam start/resume | Nurse | POST | `.../entitlements/{entitlementId}/exam-session` | same | Authenticated | No body; `PackageExamSessionStartDto` | 200 | 400, 401, 404, 409 with code | `PackageExamSessionEndpointTests`; handler/concurrency tests | `OPEN-PH1-002`, `005`, `OPEN-009` |
| Package report read | Nurse | GET | `/me/nurse-profile/preparation-packages/exam-sessions/{sessionId}/report` | same | Authenticated | Path id; `PackageAnalyticalReportDto` | 200 | 400, 401, 404, 409 with code | `PackageAnalyticalReportEndpointTests`; report contract/persistence tests | `OPEN-PH1-001`, `002`, `005` |
| Reporting topics | Admin | GET/POST/PUT/POST | `/admin/preparation-package/reporting-topics`, `/{id}`, `/{id}/archive` | `MapReportingTopicEndpoints` | `ReportingTopics.Manage` | Admin topic requests / `AdminReportingTopicDto` | 200/201 | 400, 401, 403, 404; archive 409 | `AdminPreparationPackageEndpointTests`, validator tests | `OPEN-PH1-001`, `002`, `005` |
| Reporting profiles | Admin | GET/POST/GET/POST | `/admin/preparation-package/reporting-profiles`, `/{id}`, `/{id}/publish` | `MapReportingProfileEndpoints` | `ReportingProfiles.Manage` | Admin profile requests / `AdminReportingProfilePublicationDto` | 200/201 | 400, 401, 403, 404, 409 | admin/validator tests | `OPEN-PH1-001`, `002`, `005` |
| Study materials | Admin | GET/POST/POST/PUT/POST | `/admin/preparation-package/materials` and version lifecycle routes | `MapStudyMaterialEndpoints` | `StudyMaterials.Manage` | Material requests / admin material DTOs | 200/201 | 400, 401, 403, 404, 409 | admin/validator tests | `OPEN-PH1-001`, `002`, `005` |
| Practice collections | Admin | GET/POST/POST/PUT/POST | `/admin/preparation-package/practice-collections` and version lifecycle routes | `MapPracticeCollectionEndpoints` | `PracticeCollections.Manage` | Collection requests / admin collection DTOs | 200/201 | 400, 401, 403, 404, 409 | admin/validator tests | `OPEN-PH1-001`, `002`, `005` |
| Package definitions | Admin | GET/POST/PUT | `/admin/preparation-package/packages`, `/{id}` | `MapPackageDefinitionAndVersionEndpoints` | View for list; Manage for create/update | Definition requests / `AdminPreparationPackageDefinitionDto` | 200/201 | 400, 401, 403, 404 | admin/validator/permission tests | `OPEN-PH1-001`, `002`, `005` |
| Package versions/composition | Admin | POST/GET/POST/POST | `/packages/{id}/versions`, validation, publish, retire | same | Manage/View/Publish as mapped | Version request / version DTO + validation DTO | 200/201 | 400, 401, 403, 404, 409 | admin/validator/publication tests | `OPEN-PH1-001`, `002`, `005` |
| Package offers | Admin | GET/POST/PUT/POST/POST | `/admin/preparation-package/offers`, `/{id}`, activate/deactivate | `MapOfferEndpoints` | `PreparationPackageOffers.Manage` | Offer requests / `AdminPreparationPackageOfferDto` | 200/201 | 400, 401, 403, 404, 409 | admin/validator/permission tests | `OPEN-PH1-001`, `002`, `005` |

## 5. DTO and field evidence

| DTO / source | Fields evidenced | Required / nullable evidence | Sensitivity / unknowns |
|---|---|---|---|
| `PreparationPackageOfferListItemDto`, `PreparationPackageOfferDetailDto` in `PreparationPackageDtos.cs` | Id, title, slug, optional summary, country/category/exam ids and names, material/practice counts, duration days, price minor, currency; detail components | `Summary` nullable; strings initialized but runtime requiredness not OpenAPI-confirmed | Public tests prohibit protected exam, authorization, provider, token, score/report fields. |
| `CreatePaymentOrderRequest` in `Payments/Commands/CreateMyPaymentOrder` | nullable `ProductId`, nullable `PackageOfferId` | Validator requires exactly one and non-empty selected source | Package flow uses `PackageOfferId`; field-level JSON/OpenAPI naming remains unconfirmed. |
| `PaymentCompletionDto`, `PaymentPackageEntitlementSummaryDto` in `Payments/DTOs` | order id/status/paid-at, granted exam ids, package entitlement summary fields/windows/status | `PaidAt` nullable | No-store response; Production provider contract is excluded. |
| Entitlement DTOs in `Entitlements/DTOs/PackageEntitlementDtos.cs` | identity, offer/definition/version/exam, access window/status, purchased snapshot, benefit right type/status/availability/dormancy/window | Offer summary and right access end are nullable | Raw JSON tests exclude nurse profile, order item, snapshot/right internal ids, tokens, provider fields, and protected content. |
| `PackagePracticeProgressSummaryDto`, item/submission DTOs in `PracticeProgress/DTOs` | entitlement/practice version, counters, item state, selected option, timestamp | State lists initialized; individual nullable fields must follow DTO source/OpenAPI | Tests exclude official exam IDs/snapshots, correct answers, rationales, internal right/ownership data. |
| `PackageExamSessionStartDto` | `Session`, source, entitlement, included exam/version, offer, access end | `Session` non-null initialized with null-forgiving operator; exact response schema depends on `ExamSessionDto` | Tests exclude internal right/provenance and answer-key fields. |
| `PackageAnalyticalReportDto`, topic/guidance DTOs | generated/session/finalization times, score/count/percentage/pass, topic results, guidance source metadata | several timestamps nullable; list defaults shown in source | Tests exclude protected answers/rationales, provenance, entitlement/right identifiers, and provider fields. |
| Admin DTOs in `PreparationPackageDtos.cs` | topic/profile/material/version/practice/package/version/offer fields | Explicit nullable fields are `?`; request-field requiredness is validator-specific | Admin practice DTOs contain `IsCorrect` for authoring only; never use as nurse/practice runtime evidence. |

## 6. Validation evidence

Validators are registered with `services.AddValidatorsFromAssembly(...)` in `Application/DependencyInjection.cs`. The public Preparation Package catalog list endpoint now resolves `IValidator<ListPreparationPackageOffersQuery>` and invokes it before `ISender.Send`. Focused endpoint tests prove invalid pagination does not invoke the handler. Live verification proves `page=0` and `pageSize=101` return `400 application/problem+json`, while valid pagination returns `200`.

| Operation group | Validator / source | Rules evidenced | Frontend implication / gap |
|---|---|---|---|
| Catalog and entitlement paging | `ListPreparationPackageOffersQueryValidator`; `ListMyPackageEntitlementsQueryValidator` | Page >= 1; page size 1–100 | Use validated pagination only after runtime/OpenAPI confirmation. |
| Package order | `CreateMyPaymentOrderCommandValidator` | Exactly one purchase source; selected id non-empty | Do not infer server field error keys/casing. |
| Sandbox completion | `CompleteSandboxPaymentCheckoutCommandValidator` | Checkout id non-empty | Exact runtime response remains unproven. |
| Practice answer | `SubmitPackagePracticeAnswerRequestValidator`, command validator | entitlement/item/selected option ids non-empty | Map only confirmed field errors after runtime evidence. |
| Report/entitlement detail | `GetPackageAnalyticalReportQueryValidator`, `GetMyPackageEntitlementQueryValidator` | Path ids non-empty | Route constraints may reject malformed values before validator; exact outcome is not established. |
| Admin authoring | validators in `Admin/**Operations.cs`, plus `PreparationPackageValidatorTests` | Topic fields, assignments, material type/content/topics, practice item/topic/correct option, package ordering, offer price/currency/duration | Admin form contracts require per-operation extraction before UI. |

Runtime FluentValidation invocation is verified for the public Preparation Package catalog pagination operation only. Other Minimal API command/query validators remain outside this bounded stabilization and must not be inferred as active.

## 7. Error and Problem Details evidence

`ExceptionMiddleware.cs` produces `application/problem+json` with `type`, `title`, `status`, `detail`, and `traceId`; thrown FluentValidation errors additionally create `errors` keyed by validator property name.

| Status | Evidence / shape | Package-specific evidence | Frontend implication / gap |
|---|---|---|---|
| 400 | `ValidationException` maps to title `Validation failed` and `errors`; endpoint metadata declares validation problem | Mapped metadata on catalog, nurse, and admin routes | Field mapping is possible only after runtime invocation/JSON naming evidence. |
| 401 | Middleware maps `UnauthorizedAccessException`; nurse/admin metadata declares 401 | Nurse endpoint tests prove unauthenticated 401 | Use auth bootstrap/refresh policy; do not retry mutations blindly. |
| 403 | Middleware maps `ForbiddenAccessException`; admin metadata declares 403 | Admin tests prove missing exact permission 403; nurse role guard can throw 403 | Present unauthorized state; UI checks are not security. |
| 404 | `KeyNotFoundException` maps to resource-not-found | Owned/missing nurse entitlement, practice, session, report tests prove privacy-preserving 404; detail metadata declares it | Use generic not-found wording; do not reveal ownership. |
| 409 | `InvalidOperationException`, package session/report conflict exceptions map to conflict | Practice expiry; package exam codes; report codes; package lifecycle actions | Persistent contextual conflict state; branch on documented code only. |
| 422 | No mapping found in middleware or endpoint tests | None evidenced | **`OPEN-PH1-003` remains open.** |
| 429 | Only checkout-in-progress exception supplies `retryAfterSeconds` and `Retry-After`; no package endpoint rate-limit evidence | No package rate-limit route/test evidence | Do not invent rate-limit UI for package operations. |
| 500 | Middleware fallback suppresses exception detail | Applicable fallback only | Show safe generic recovery state. |
| 503 | `PaymentCheckoutProviderUnavailableException` maps to service unavailable | Package Sandbox completion may encounter payment infrastructure; no package-specific endpoint test inspected | Use only if current operation evidence confirms it. |

Conflict codes confirmed by WebApi tests: package exam start uses `package-entitlement-inactive`, `package-attempt-right-missing`, `package-attempt-consumed`, `package-exam-version-unavailable`, and `exam-session-source-conflict`; report read uses `package-report-session-not-finalized`, `package-report-session-not-qualified`, `package-report-provenance-invalid`, `package-report-right-missing`, and `package-report-profile-incomplete`.

## 8. Permission evidence

| Operation group | Permission / source | Endpoint usage | Test evidence | Frontend implication / gap |
|---|---|---|---|---|
| Reporting topics | `ReportingTopics.Manage` in `Application/Authorization/Permissions.cs` | All mapped topic routes | Admin tests prove 401/403/201 | Visibility helper only; exact permission remains backend authority. |
| Reporting profiles | `ReportingProfiles.Manage` | All mapped profile routes | 401/403/201 and denial for Exam/Question permissions | Same. |
| Materials | `StudyMaterials.Manage` | Material/version lifecycle routes | 401/403/200/201; Exam/Question denial | No nurse delivery permission is evidenced. |
| Practice collections | `PracticeCollections.Manage` | Collection/version lifecycle routes | 401/403/201 and content-isolation tests | Same. |
| Package definitions/versions | `PreparationPackages.View`, `.Manage`, `.Publish` | list/validation, create/update, publish/retire respectively | Permission constants and 403/200 publish tests | Use exact action-level permission evidence. |
| Package offers | `PreparationPackageOffers.Manage` | Offer lifecycle routes | activate/deactivate 401/403/200 tests | Same. |
| Payment administration | No package-specific admin payment route/permission in this scope | None | None | **`OPEN-PH1-004` remains open** for current or planned administration payment operations. |

Permission constants are also tested by `PreparationPackagePermissionTests`; seeding evidence is in `Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` and its tests. No frontend authority follows from possession of a UI permission indicator.

## 9. Frontend design implications

Future candidate screens may rely on the documented endpoint paths, actor boundaries, known DTO source fields, and tested 401/403/404/409 patterns. They must represent no-store Sandbox completion as sensitive short-lived state; generic ownership-hidden 404s; persistent conflicts for attempt/report/expiry outcomes; and backend-authoritative right availability.

Page specifications remain blocked by a current OpenAPI capture, proven runtime validation behavior, exact admin payment scope, and the design foundation/registry gates. Material delivery, workspace aggregation, adaptive/retraining, employer visibility, production payments, Angular, and Penpot screens are not implemented screen evidence.

## 10. Open questions carried forward

- `OPEN-PH1-001`: narrowed; runtime invocation is proven for public catalog pagination only, not repository-wide.
- `OPEN-PH1-002`: generated Development artifact captured; governance closure is not performed by this implementation task.
- `OPEN-PH1-003`: remains open; no `422` mapping/evidence was found.
- `OPEN-PH1-004`: remains open; administration payment operations are outside this package endpoint scope.
- `OPEN-PH1-005`: generated revision now evidences package operation paths/security and payment success contracts; governance closure remains separate because other schema discrepancies persist.
- Packet-local: Verify the current Development OpenAPI endpoint/artifact procedure, exact request JSON naming/nullability, and runtime validation pipeline before any page specification.

This task evaluates evidence only; it does not update or close governance questions.

## 11. Recommended next step

Authorize a bounded, read-only Development OpenAPI capture/procedure task. It must record the generated artifact revision for this backend, compare the package operations against endpoint code, and verify runtime validation behavior through existing tests or a separately approved test-only investigation. It must not generate clients, create page specs, write Penpot, or create Angular code.
