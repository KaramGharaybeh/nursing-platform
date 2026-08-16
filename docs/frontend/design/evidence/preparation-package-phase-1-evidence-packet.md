# Preparation Package Phase 1 Evidence Packet

## 1. Purpose and authority

This packet records deterministic repository evidence for future frontend design discovery. It is not a page specification, route registry, Penpot approval or write authorization, Angular authorization, or backend change authorization. Current backend source and a generated Development OpenAPI artifact remain the runtime-contract authority; where that artifact is unavailable, this packet records the gap rather than inferring a contract.

## 2. Source snapshot

| Item | Evidence |
|---|---|
| Branch / HEAD | `feature/frontend-design-evidence-foundation` / `4073154 docs: record frontend design G0 acceptance` |
| Backend Preparation Package evidence baseline | `8439511` |
| G0 governance baseline | `4073154` |
| G0 scope | Accepted governance/re-entry only; Phase 1 evidence extraction is authorized, not implementation. |
| Endpoint source | `backend/src/NursingPlatform.WebApi/Extensions/PreparationPackageEndpointExtensions.cs` |
| Payment endpoint source | `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` |
| Current package test evidence | WebApi integration tests under `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/` and Application tests under `backend/tests/NursingPlatform.Application.Tests/PreparationPackages/` |

## 3. Evidence coverage summary

Implemented backend evidence covers public catalog offers, nurse-owned entitlements, package payment order creation and Development/Test Sandbox completion, entitlement benefit-right summaries, practice progress, package exam-session start, report read/generation, and permission-protected administration.

The Development OpenAPI document was captured from the authorized uncommitted contract-stabilization working tree based on `7768f27`: `http://localhost:5167/openapi/v1.json`, OpenAPI `3.1.1`, temporary artifact `/tmp/opencode/development-openapi-stabilized.json`, SHA-256 `c0c4f965215c0b58bcbf59d07db9e85c2fcc6b5a0dd91dc43f8bdd6b3f4e95a1`. The capture verifies Preparation Package paths, typed payment response schemas, payment response headers, and Bearer requirements for protected operations. Remaining gaps include Problem Details extensions, numeric schema unions, required-property metadata, no nurse package material delivery route, no workspace aggregate route, and no approved page, route, visual, or frontend runtime contract.

## 4. Implemented capability evidence

### 4.1 Public package catalog and offer discovery

- **Purpose / actor / status:** Public discovery of active, catalog-eligible offers; anonymous; **Implemented**.
- **Source:** `PreparationPackageEndpointExtensions.cs` `MapPublicCatalogEndpoints`; catalog operations in `Application/PreparationPackages/Catalog/PreparationPackageCatalogOperations.cs`; DTOs in `Application/PreparationPackages/DTOs/PreparationPackageDtos.cs`.
- **API:** `GET /api/v1/preparation-packages/offers` with optional `page`, `pageSize`, `countryId`, `examCategoryId`; anonymous; `200`, documented validation `400`. `GET /api/v1/preparation-packages/offers/{slug}`; anonymous; `200`, `400`, `404`.
- **Response evidence:** `PreparationPackageOfferListItemDto` and `PreparationPackageOfferDetailDto`; tests prove catalog JSON omits tokens, protected exam content, correct answers, scores, reports, internal authorization/provenance, and provider data.
- **Tests:** `PreparationPackageCatalogEndpointTests`.
- **Candidate implication:** Catalog and offer-detail candidate areas are **Ready for candidate registry after evidence review**, but need OpenAPI confirmation.

### 4.2 Package offer details

- **Purpose / actor / status:** Anonymous user reads one active, catalog-eligible offer by slug; **Implemented**.
- **Source/API:** `MapPublicCatalogEndpoints`; `GET /api/v1/preparation-packages/offers/{slug}`; `PreparationPackageOfferDetailDto` extends the list DTO with component summaries.
- **Evidence/gap:** `PreparationPackageCatalogEndpointTests` proves safe JSON and `200`; mapping documents `400` and `404`. Exact generated OpenAPI operation/schema remains open.

### 4.2 Package payment/order creation path

- **Purpose / actor / status:** Nurse creates one package-offer order using the existing payment order workflow; nurse; **Implemented**.
- **Source:** `ApplicationBuilderExtensions.cs` `CreateMyPaymentOrder`; `CreateMyPaymentOrderCommand.cs`; `CreatePaymentOrderRequest.cs`; `Payments/DTOs`.
- **API:** `POST /api/v1/me/nurse-profile/payment/orders`; authenticated nurse; returns `201`. The request validator requires exactly one of `productId` or `packageOfferId`; package ordering uses `packageOfferId`.
- **Business evidence:** Order creation requires an active offer, published/isolated package version, published matching exam/version/reporting profile/practice collection/material versions; it snapshots the purchased offer and composition.
- **Errors evidenced:** validator `400`; missing offer `404`; non-purchasable facts map through `InvalidOperationException` to `409`. Exact public OpenAPI schema/status matrix remains open.
- **Candidate implication:** Package purchase handoff is **Needs OpenAPI confirmation** and payment-operation evidence review.

### 4.3 Payment completion and fulfillment result

- **Purpose / actor / status:** Complete a Development/Test Sandbox checkout and fulfill package entitlements; nurse/system; **Implemented for Development/Test Sandbox only**.
- **Source:** `ApplicationBuilderExtensions.cs` `CompleteSandboxPaymentCheckout`; `CompleteSandboxPaymentCheckoutCommand.cs`; `Payments/Common/PackagePaymentFulfillmentService.cs`; `PaymentCompletionDto` and payment DTOs.
- **API:** `POST /api/v1/dev/sandbox/payment/checkout-sessions/{checkoutSessionId}/complete`; authenticated; mapped only in Development or Test; `200`, `Cache-Control: no-store`.
- **Business evidence:** Completion is transactional/idempotent for a paid order; package fulfillment creates entitlement/right evidence while standalone `ExamAccessGrant` behavior remains independent. `PaymentCompletionDto` includes package-entitlement summaries.
- **Errors evidenced:** empty checkout id validator `400`; missing/other-owner checkout `404`; expired/invalid provider/session/order state `409`; unsafe fulfillment `409`; exact OpenAPI remains open.
- **Candidate implication:** A Sandbox-only purchase outcome/handoff is **Needs OpenAPI confirmation**; no production payment screen is evidenced.

### 4.4 Nurse package entitlements and benefit-right read model

- **Purpose / actor / status:** Nurse reads owned package access windows, purchased snapshot, and four right summaries; nurse; **Implemented**.
- **Source:** endpoint extension `MapNurseEntitlementEndpoints`; `Entitlements/ListMyPackageEntitlements`, `Entitlements/GetMyPackageEntitlement`, and `Entitlements/DTOs/PackageEntitlementDtos.cs`.
- **API:** `GET /api/v1/me/nurse-profile/preparation-packages/entitlements` (`page`, `pageSize`) and `GET .../entitlements/{id}`; authenticated; `200`, `400`, `401`; detail documents `404`.
- **Response evidence:** `PackageEntitlementListItemDto`, `PackageEntitlementDetailDto`, `PackageBenefitRightSummaryDto`. Rights expose type, status, availability/dormancy, and access windows—not internal right identifiers.
- **Tests:** `PackageEntitlementEndpointTests` proves `401`, nurse ownership behavior, raw JSON protection, four benefit-right summaries, and no employer routes.
- **Candidate implication:** Entitlement list/detail is **Ready for candidate registry after evidence review**, subject to OpenAPI confirmation.

### 4.5 Package study-material access/read state

- **Purpose / actor / status:** Entitlement exposes `MaterialsAccess` right and purchased material-version identifiers; nurse; **Partially evidenced**.
- **Source:** entitlement DTOs and `PackageBenefitAuthorizationService.cs`; package material administration in `Admin/StudyMaterials`.
- **API evidence:** No nurse material content/delivery endpoint is mapped. The tested nurse material path returns `404` for anonymous and authenticated callers (`AdminPreparationPackageEndpointTests`).
- **Business evidence:** Materials access follows the active entitlement/right window; published material versions are included in snapshots. File, external-link, video, and formatted-text metadata exist in admin DTOs, but delivery/read behavior is not exposed through a nurse route.
- **Candidate implication:** A material-access state may be a future candidate only after exact API evidence; it is **Blocked by missing delivery/read contract**.

### 4.6 Package practice access and progress

- **Purpose / actor / status:** Nurse reads historical progress and records/replaces practice answers while access is active; nurse; **Implemented**.
- **Source:** endpoint extension; `PracticeProgress/GetPackagePracticeProgressQuery*`, `SubmitPackagePracticeAnswerCommand*`; DTOs under `PracticeProgress/DTOs`.
- **API:** `GET .../entitlements/{entitlementId}/practice-progress`; `POST .../practice-progress/items/{practiceItemId}/answer` with `SubmitPackagePracticeAnswerRequest { selectedPracticeAnswerOptionId }`; authenticated. `200`, `400`, `401`, `404`; answer submission also `409`.
- **Validation:** `SubmitPackagePracticeAnswerCommand.cs` requires non-empty entitlement, practice item, and selected option IDs.
- **Tests:** `PackagePracticeProgressEndpointTests` proves `401`, ownership-hidden `404`, historical read after expiry, expired write `409`, correct/incorrect/unanswered counters, and no official exam/internal-right exposure.
- **Candidate implication:** Practice-progress candidate area is **Ready for candidate registry after evidence review**, subject to OpenAPI confirmation of item-content routes and forms.

### 4.7 Package exam-session start/resume

- **Purpose / actor / status:** Nurse explicitly consumes/selects an entitlement-scoped package attempt or resumes the same qualifying session; nurse; **Implemented**.
- **Source:** endpoint extension; `ExamSessions/StartPackageExamSession/StartPackageExamSessionCommand*`; `PackageExamSessionStartDto.cs`; `PackageExamSessionConflictException.cs`.
- **API:** `POST /api/v1/me/nurse-profile/preparation-packages/entitlements/{entitlementId}/exam-session`; authenticated, no body; `200`, `400`, `401`, `404`, `409`.
- **Response evidence:** source, entitlement, included exam/version, offer, access end, and standard `ExamSessionDto`; no internal right/provenance/answer-key fields.
- **Conflict evidence:** `package-entitlement-inactive`, `package-attempt-right-missing`, `package-attempt-consumed`, `package-exam-version-unavailable`, and `exam-session-source-conflict` are `409` Problem Details codes. Same-entitlement retry returns the existing session safely.
- **Tests:** `PackageExamSessionEndpointTests`.
- **Candidate implication:** Exam start/resume candidate is **Ready for candidate registry after evidence review**; session-question UI requires the existing exam contract evidence separately.

### 4.8 Package analytical report read

- **Purpose / actor / status:** Nurse reads or lazily generates an immutable report for an owned qualifying finalized package session; nurse/system; **Implemented**.
- **Source:** endpoint extension; `Reports/GetPackageAnalyticalReport/*`; `Reports/Generation/PackageAnalyticalReportGenerator.cs`; report DTOs/mapping.
- **API:** `GET /api/v1/me/nurse-profile/preparation-packages/exam-sessions/{sessionId}/report`; authenticated; `200`, `400`, `401`, `404`, `409`.
- **Response evidence:** score/count/percentage, topic results, and deterministic purchased-content guidance (`PackageAnalyticalReportDto`, topic/guidance DTOs). No question text, answer keys, correct answers/options, rationale, entitlement/right IDs, provenance, or provider data.
- **State evidence:** repeated reads return the same report id; report is readable after entitlement expiry; qualifying report generation requires finalized valid package attempt evidence.
- **Conflict evidence:** `package-report-session-not-finalized`, `package-report-session-not-qualified`, `package-report-provenance-invalid`, `package-report-right-missing`, `package-report-profile-incomplete`.
- **Tests:** `PackageAnalyticalReportEndpointTests`; Application report contract/behavior tests.
- **Candidate implication:** Report read is **Ready for candidate registry after evidence review**, subject to OpenAPI confirmation.

### 4.9 Admin reporting topics and profiles

#### Admin reporting topics

- **Purpose / actor / status:** Admin manages Reporting Topics and reporting-profile publications used for package eligibility/reporting; admin; **Implemented**.
- **Source:** endpoint extension; `Admin/ReportingTopics/AdminReportingTopicOperations.cs`; `Admin/ReportingProfiles/AdminReportingProfileOperations.cs`; DTOs in `PreparationPackageDtos.cs`; publication validator.
- **API:** Reporting topics `GET/POST /admin/preparation-package/reporting-topics`, `PUT /{id}`, `POST /{id}/archive`; permission `ReportingTopics.Manage`. Reporting profiles `GET/POST /admin/preparation-package/reporting-profiles`, `GET /{id}`, `POST /{id}/publish`; permission `ReportingProfiles.Manage`.
- **Status/error evidence:** reads/updates `200`; creates `201`; documented `400/401/403`; detail/lifecycle `404`; archive/publish `409` where mapped. Profile publication requires a published exact exam version and assignments for every active scored question.
- **Tests:** `AdminPreparationPackageEndpointTests`, `PreparationPackageValidatorTests`, `PreparationPackagePermissionTests`.

#### Admin reporting profiles

- **Purpose / actor / status:** Admin creates, reads, and publishes immutable reporting-profile publications; admin; **Implemented**.
- **API / source:** `GET/POST /admin/preparation-package/reporting-profiles`, `GET /{id}`, `POST /{id}/publish`; `ReportingProfiles.Manage`; `Admin/ReportingProfiles/AdminReportingProfileOperations.cs`.
- **Evidence/gap:** The exact-exam-version and complete scored-question-assignment rules are enforced by `PreparationPackagePublicationValidator.cs`; `AdminPreparationPackageEndpointTests` proves dedicated authorization. OpenAPI remains required.

- **Candidate implication:** Both admin areas are **Ready for candidate registry after evidence review**, subject to OpenAPI confirmation.

### 4.10 Admin study-material metadata/version management

- **Purpose / actor / status:** Admin manages stable material identity and draft/published/retired versions; admin; **Implemented**.
- **Source:** endpoint extension; `Admin/StudyMaterials/AdminStudyMaterialOperations.cs`; material DTOs and validators.
- **API:** `GET/POST /admin/preparation-package/materials`; `POST /materials/{materialId}/versions`; `PUT /materials/{materialId}/versions/{versionId}`; publish/retire POST actions. Permission `StudyMaterials.Manage`.
- **Validation evidence:** Material type fields must be compatible; at least one Reporting Topic mapping is required. Types include File, ExternalLink, Video, FormattedText, but no storage/upload/delivery route is evidence.
- **Tests:** `AdminPreparationPackageEndpointTests`, `PreparationPackageValidatorTests`; exact dedicated permission is tested against `Exams.Edit`/`Questions.Manage` denial.
- **Candidate implication:** Admin metadata/version management is **Ready for candidate registry after evidence review**; nurse material delivery is deferred.

### 4.11 Admin practice collections

- **Purpose / actor / status:** Admin manages independent practice collections and versions; admin; **Implemented**.
- **Source:** endpoint extension; `Admin/PracticeCollections/AdminPracticeCollectionOperations.cs`; practice DTOs/validators.
- **API:** `GET/POST /admin/preparation-package/practice-collections`; version create/update/publish/retire routes beneath collection; permission `PracticeCollections.Manage`.
- **Validation evidence:** Items require a Reporting Topic and exactly one correct option; published/retired lifecycle actions expose documented `400/401/403/404/409` as applicable.
- **Tests:** `AdminPreparationPackageEndpointTests` proves dedicated permission and no official exam identifiers/snapshots in practice responses; `PreparationPackageValidatorTests`.
- **Candidate implication:** Admin practice collections is **Ready for candidate registry after evidence review**, subject to OpenAPI confirmation.

### 4.12 Admin package definitions, versions/composition, and offers

#### Admin package definitions

- **Purpose / actor / status:** Admin manages stable package definitions, immutable published composition, validation/publishing, and offer lifecycle; admin; **Implemented**.
- **Source:** endpoint extension; `Admin/PackageDefinitions`, `Admin/PackageVersions`, `Admin/PackageOffers`; `PreparationPackagePublicationValidator.cs`; package DTOs.
- **API:** Definitions: list/create/update under `/admin/preparation-package/packages`; versions: create, validation, publish, retire under `/packages/{packageId}/versions`; offers: list/create/update/activate/deactivate under `/offers`.
- **Permission evidence:** `PreparationPackages.View` lists/validates; `PreparationPackages.Manage` creates/updates definitions and versions; `PreparationPackages.Publish` publishes/retires versions; `PreparationPackageOffers.Manage` manages offers.
- **Business/validation evidence:** composition requires published matching exam/profile/practice/material components, unique positive material ordering, topic compatibility, and confirmed content isolation. An offer references a published version and its price/currency/access duration, not independently selected content.
- **Tests:** `AdminPreparationPackageEndpointTests`; `PreparationPackageValidatorTests`; `PreparationPackagePermissionTests`; Application handler tests.
- **Candidate implication:** Package definition/version/composition and offers are **Ready for candidate registry after evidence review**, subject to OpenAPI confirmation.

#### Admin package versions and composition

- **Purpose / actor / status:** Admin creates, validates, publishes, and retires exact package compositions; admin; **Implemented**.
- **API / source:** Version routes beneath `/admin/preparation-package/packages/{packageId}/versions`; `PreparationPackages.Manage` for create, `PreparationPackages.View` for validation, and `PreparationPackages.Publish` for publish/retire; `Admin/PackageVersions/AdminPreparationPackageVersionOperations.cs`.
- **Evidence/gap:** `PreparationPackagePublicationValidator.cs`, package DTOs, validators, and admin tests evidence exact published components, topic compatibility, ordering, and isolation; generated OpenAPI remains required.

#### Admin package offers

- **Purpose / actor / status:** Admin creates, updates, activates, and deactivates offers that reference already-published package versions; admin; **Implemented**.
- **API / source:** `GET/POST /admin/preparation-package/offers`, `PUT /offers/{id}`, `POST /offers/{id}/activate|deactivate`; `PreparationPackageOffers.Manage`; `Admin/PackageOffers/AdminPreparationPackageOfferOperations.cs`.
- **Evidence/gap:** Offer DTO/validator and admin authorization tests evidence the commercial fields/lifecycle; generated OpenAPI remains required.

## 5. API operation evidence table

| Capability | Actor | Method / path | Auth/permission | Request / response evidence | Success | Error evidence | Tests / open question |
|---|---|---|---|---|---|---|---|
| Catalog list/detail | Public | GET `/preparation-packages/offers`, `/{slug}` | Anonymous | Query; offer list/detail DTOs | 200 | 400; detail 404 | Catalog tests; OpenAPI capture required |
| Package order | Nurse | POST `/me/nurse-profile/payment/orders` | Authenticated nurse | `CreatePaymentOrderRequest.packageOfferId`; `PaymentOrderDto` | 201 | 400/404/409 evidenced in handlers | Payment command/tests; OpenAPI required |
| Sandbox completion | Nurse/system | POST `/dev/sandbox/payment/checkout-sessions/{id}/complete` | Authenticated; Dev/Test only | Path id; `PaymentCompletionDto` | 200 | 400/404/409 | Completion command; OpenAPI required |
| Entitlements | Nurse | GET `/me/nurse-profile/preparation-packages/entitlements`, `/{id}` | Authenticated | Pagination; entitlement DTOs | 200 | 400/401/404 | Entitlement tests; OpenAPI required |
| Practice progress | Nurse | GET progress; POST answer | Authenticated | Answer request/summary/submission DTOs | 200 | 400/401/404/409 | Practice tests; OpenAPI required |
| Package exam start | Nurse | POST `.../entitlements/{id}/exam-session` | Authenticated | No body; start DTO | 200 | 400/401/404/409 + codes | Session tests; OpenAPI required |
| Report read | Nurse | GET `.../exam-sessions/{id}/report` | Authenticated | Report DTO | 200 | 400/401/404/409 + codes | Report tests; OpenAPI required |
| Reporting topics/profiles | Admin | CRUD/publish routes in section 4.9 | Exact dedicated permission | Admin requests/DTOs | 200/201 | 400/401/403/404/409 as mapped | Admin tests; OpenAPI required |
| Materials/practice collections | Admin | Version lifecycle routes in sections 4.10–4.11 | Exact dedicated permission | Admin requests/DTOs | 200/201 | 400/401/403/404/409 as mapped | Admin/validator tests; OpenAPI required |
| Packages/versions/offers | Admin | Definition/version/offer routes in section 4.12 | Exact package/offer permission | Admin requests/DTOs | 200/201 | 400/401/403/404/409 as mapped | Admin/validator tests; OpenAPI required |

All paths above are relative to `/api/v1`.

## 6. Permission evidence table

| Capability | Permission | Source | Test evidence | Open question |
|---|---|---|---|---|
| Reporting topics | `ReportingTopics.Manage` | `Permissions.cs`; endpoint mapping | Admin endpoint 401/403/201 tests | Current OpenAPI capture |
| Reporting profiles | `ReportingProfiles.Manage` | same | Admin tests; exam/question permissions denied | Current OpenAPI capture |
| Materials | `StudyMaterials.Manage` | same | Admin tests; exam/question permissions denied | Delivery contract absent |
| Practice collections | `PracticeCollections.Manage` | same | Admin tests; independent content test | Current OpenAPI capture |
| Package definitions/validation | `PreparationPackages.View`, `PreparationPackages.Manage` | same | Permission constants/admin tests | Current OpenAPI capture |
| Package publish/retire | `PreparationPackages.Publish` | same | Publish 403/200 test | Current OpenAPI capture |
| Offers | `PreparationPackageOffers.Manage` | same | Activate/deactivate permission tests | Current OpenAPI capture |

## 7. Validation and Problem Details evidence

FluentValidation validators are present in payment commands, package admin operations, practice answer commands, and report query paths. Concrete tested examples include page >= 1 and page size 1–100; non-empty identifiers; reporting-topic category/name/slug; non-empty reporting-profile assignments; compatible material type/content with at least one topic; practice item topic and exactly one correct option; unique positive package material order; and non-negative price, uppercase currency, positive duration.

`ExceptionMiddleware.cs` maps validation to `400` with `errors`, missing resources to `404`, forbidden to `403`, unauthorized to `401`, package session/report and `InvalidOperationException` conflicts to `409`, provider unavailable to `503`, and unhandled failures to `500`. `422` behavior remains `OPEN-PH1-003`; it is not evidenced here. Rate limiting/`429` is not evidenced for these package endpoints.

## 8. Business state model evidence

- **Offer/catalog:** only active, catalog-eligible offers are public; sale revalidates published compatible package components and confirmed content isolation.
- **Fulfillment/entitlement:** successful package payment creates immutable purchased-offer evidence, one entitlement, and MaterialsAccess, PracticeAccess, PackageExamAttemptEligibility, and dormant ReportEligibility rights. Active same-package repurchase is blocked; expired repurchase forms a new window/right set.
- **Practice:** active access permits answer write/re-answer; progress stores answered correct/incorrect or derived unanswered; expiry blocks writes but permits historical owner read. Practice does not consume attempts or report evidence.
- **Attempt:** explicit entitlement package start consumes/uses a package attempt atomically; same-source in-progress retry resumes idempotently; inactive/missing/consumed/conflicting source states yield documented 409 codes.
- **Report:** a finalized qualifying package session can lazily produce one immutable report; repeated reads are idempotent and post-expiry owner read remains available. Guidance is restricted to purchased content and report data excludes protected exam review material.
- **Standalone/free compatibility:** package rights coexist with standalone/free behavior and are not a substitute for `ExamAccessGrant`.

## 9. Candidate screen areas

| Candidate area | Actor / goal | Evidence readiness | Blockers | Deferred exclusions | Candidate registry after review |
|---|---|---|---|---|---|
| Public package catalog / offer detail | Public user discovers eligible package | Needs OpenAPI confirmation | `OPEN-PH1-002`, `005` | No checkout completion claim | Yes |
| Package order / Sandbox handoff | Nurse starts order and receives Sandbox outcome | Needs OpenAPI/payment evidence | `OPEN-PH1-002`, `004`, `007` | No production provider/webhooks/refunds | Yes, after review |
| Entitlements / rights | Nurse sees purchased access and right availability | Needs OpenAPI confirmation | `OPEN-PH1-002`, `005` | No workspace aggregate | Yes |
| Materials access state | Nurse understands MaterialsAccess availability | Blocked by missing delivery/read route | Delivery contract absent | No storage/upload/download/delivery | No |
| Practice progress | Nurse reviews and updates practice progress | Needs OpenAPI confirmation | `OPEN-PH1-002`, `005` | No adaptive/retraining | Yes |
| Package exam start/resume | Nurse starts or resumes package attempt | Needs OpenAPI/exam-contract confirmation | `OPEN-PH1-002`, `005`, `OPEN-009` | No attempt reset | Yes, after review |
| Analytical report | Nurse reads diagnostic report | Needs OpenAPI confirmation | `OPEN-PH1-002`, `005` | No employer/admin report view | Yes |
| Admin package authoring | Admin manages topics, profiles, materials, practice, package composition, offers | Needs OpenAPI confirmation | `OPEN-PH1-002`, `005` | No material delivery/employer data | Yes |

## 10. Explicitly deferred or forbidden-as-implemented

- **Storage provider and upload/download/delivery:** no nurse material-content route is implemented; do not present it as usable package functionality.
- **Offline access:** deferred; no offline/sync contract exists.
- **Workspace/dashboard aggregation:** no package workspace aggregate route exists.
- **Adaptive practice and spaced repetition/retraining:** deferred; practice retry is not adaptive/retraining.
- **Employer visibility:** no employer package purchase, progress, or report routes exist.
- **Production payment provider/webhooks/refunds/subscriptions:** outside the evidenced Sandbox path.
- **Angular implementation, Penpot screens, page specifications, and final route registry:** outside this evidence packet's authority.

## 11. Open questions to carry forward

- Existing: `OPEN-PH1-001` runtime FluentValidation invocation; `OPEN-PH1-002` OpenAPI artifact/capture; `OPEN-PH1-003` `422`; `OPEN-PH1-004` administration payment permissions; `OPEN-PH1-005` package operation OpenAPI/source extract.
- Existing design foundation: `OPEN-001`–`OPEN-009` and applicable `DISC-PEN-*` / `DISC-REP-001`.
- Packet-local: A generated Development OpenAPI artifact is now captured for revision `7768f27`, but it is not sufficient for TypeScript generation while Problem Details extensions, numeric unions, and required/nullability metadata remain unresolved.
- Packet-local: Package material entitlement metadata is evidenced, but nurse content retrieval/delivery is not; do not design a material-reader screen as implemented.

## 12. Recommended next step

Authorize a bounded documentation-only API/OpenAPI and validation/error evidence-index task for this packet's operations. It should capture the current Development OpenAPI artifact or document the safe capture procedure, operation/DTO references, validation behavior, and Problem Details status/code evidence. It must not create page specifications, Penpot artifacts, Angular code, or a final route registry.
