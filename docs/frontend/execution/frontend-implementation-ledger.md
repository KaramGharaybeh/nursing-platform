# Frontend Implementation Ledger

This ledger is the authoritative detailed execution record for `GOAL-FE-001` frontend Goal → Milestone → Task → Subtask → Verification Gate work.

It is a governance and tracking document only. It does not authorize implementation by itself. Implementation may proceed only through an explicit task authorization or the Standing Implementation Authorization in this ledger and `docs/development/model-orchestration.md`.

## 1. Authority

Authoritative frontend rules remain in:

- `docs/frontend/frontend-project-rules.md`
- `docs/frontend/frontend-architecture.md`
- approved design specifications under `docs/frontend/design/specs/`
- canonical backend/OpenAPI contracts under `docs/frontend/design/integration/openapi/`

`PROGRESS.md` remains the concise current-state and handoff memory. This ledger is the detailed implementation roadmap authority.

### Execution authority precedence

When frontend sources conflict, use this precedence for implementation execution and status decisions:

1. Current explicit user or technical-lead decisions.
2. Current execution state and task/gate authority in this ledger, with `PROGRESS.md` as the concise current-session handoff.
3. Current frontend architecture and project rules in `docs/frontend/frontend-architecture.md` and `docs/frontend/frontend-project-rules.md`.
4. Approved design specifications and approved Penpot/design evidence.
5. Canonical backend/OpenAPI authority for API, validation, auth, authorization, security, and business behavior.
6. Historical planning, design inventory, evidence packets, trackers, and older prose.

Historical documents remain preserved as evidence, but they do not override newer verified execution state or current repository facts. Older statements such as “frontend workspace is not initialized” are stale for execution once the ledger and repository evidence show a later verified state. If the above precedence cannot resolve a conflict deterministically, STOP and escalate instead of guessing.

### Standing Implementation Authorization

Standing Implementation Authorization replaces the need for a separate user “start this task” message only for ordinary eligible Low/Medium frontend implementation Tasks where **all** of the following are true:

- every predecessor Gate required by the Task is `VERIFIED`;
- exact Task scope and expected file boundaries are established from current repository authority;
- no unresolved `DESIGN`, `BACKEND`, `CONTRACT_CLARIFICATION`, `SECURITY`, `DEPENDENCY`, `TOOLING_APPROVAL`, `SCOPE`, `EXTERNAL`, or runtime-deployment decision requires human authority;
- no missing screen-family approval, per-screen `APPROVED` decision, Penpot/design approval, or visual/source decision is required;
- no dependency installation/change, database change, migration, backend/OpenAPI mutation, Penpot mutation, CI/tooling approval, or external-provider decision is required;
- Task risk is Low or Medium under `docs/development/model-orchestration.md`;
- implementation can stay inside a bounded `ALLOWED_FILES` packet;
- focused tests/source-contract tests and the Gate's full deterministic evidence can be produced with existing approved tooling.

When those conditions are satisfied, the OpenAI Orchestrator may select the next eligible Task from the DAG, delegate repository-heavy exploration and implementation to the approved non-OpenAI worker pool, run required verification, perform the OpenAI final technical gate, update execution state, and proceed to the next eligible Task without a new per-task start message.

Standing authorization never authorizes staging, committing, pushing, destructive Git operations, dependency changes, database changes, migrations, backend/OpenAPI/Penpot mutation, business/product decisions, or High/High-Precision implementation escalation.

### Automatic Task selection

The Orchestrator must select Tasks by DAG eligibility, not by Task number alone. A Task is eligible only when all declared predecessor Gates are `VERIFIED`, the Standing Implementation Authorization conditions pass, and no blocker type applies. Blocked Tasks do not block unrelated eligible branches unless a declared dependency requires them. Range dependencies such as `GATE-FE-T008..T012` require every Gate in that inclusive range to be `VERIFIED`.

Example: `T-FE-013` must not run while any required predecessor Gate is not `VERIFIED`; in particular, `GATE-FE-T009` remains `BLOCKED`, so `T-FE-013` is not eligible.

### Mandatory human STOP conditions

Continuous execution MUST STOP and ask for user/technical-lead authority when any of these apply:

- a genuine product, business, architecture, UX, visual, or design decision is required;
- a design value, token, behavior, responsive rule, RTL rule, screen state, component contract, or page/screen approval is missing;
- screen-family approval is missing, a per-screen decision is not `APPROVED`, or visual/Penpot evidence is required but absent;
- dependency, package-manager, tooling, CI, E2E, AXE, browser, or external-service approval is required;
- security, authentication, authorization, payment, entitlement, exam/session/report, file-authorization, privacy, or production-hardening correctness is uncertain;
- backend source and canonical OpenAPI conflict, or a required API/DTO/error/permission contract cannot be established;
- database work, migration creation/application, backend mutation, OpenAPI mutation, or Penpot mutation is required;
- Task scope or `ALLOWED_FILES` cannot be bounded safely;
- High-risk or High-Precision implementation requires escalation under the orchestration policy;
- staging, committing, pushing, reset, clean, stash, checkout/restore, or repository-history alteration is requested;
- repository authority conflicts cannot be resolved deterministically.

Do not self-approve these stops.

### Testing and evidence invariant

Every implementation Task must satisfy its Gate evidence. Where behavior, logic, source contracts, styling contracts, routing, API mapping, security presentation, or error behavior is testable, focused unit/component/source-contract tests are mandatory and must verify behavior or relationships rather than weak string-presence checks. After focused verification, run the applicable full regression/quality/build verification required by the Task/Gate. The worker and Orchestrator must report exact command evidence; tests passing alone is insufficient unless requirement coverage is also checked.

### Usage-efficiency invariant

For normal Low/Medium eligible Tasks, use the efficient path: OpenAI Orchestrator performs compact classification, routing, packet validation, and final gate; Muse Spark 1.3 or the approved free fallback worker performs repository-heavy exploration, implementation, and deterministic verification; OpenAI reviews concise evidence and critical artifacts without repeating broad repository exploration unless a concrete risk, missing evidence, or finding justifies it.

## 2. Canonical Status and Blocker Model

Only these execution statuses are valid:

- `NOT STARTED`
- `IN PROGRESS`
- `BLOCKED`
- `READY FOR REVIEW`
- `VERIFIED`
- `REOPENED`

Blocker causes are separate from status. Valid blocker types:

- `DESIGN`
- `BACKEND`
- `CONTRACT_CLARIFICATION`
- `RUNTIME_DEPLOYMENT`
- `TOOLING_APPROVAL`
- `SECURITY`
- `DEPENDENCY`
- `SCOPE`
- `EXTERNAL`

Do not create status values such as `BLOCKED BY DESIGN`, `CONTRACT CLARIFICATION REQUIRED`, or `BLOCKED BY BACKEND`.

## 3. Approved Goal

### Goal `GOAL-FE-001` — Production-Quality Nursing Platform Angular Frontend

- status: `NOT STARTED`
- approval_reference: Phase 4A.2 technical-lead review, verdict `PASS WITH PERSISTENCE FIXES`
- product_scope: Angular 22 frontend implementation for Nursing Platform using approved backend/OpenAPI contracts and approved Penpot/design references.
- delivery_order: foundation/auth/shell/access/error states → nurse profile → exams/learning/preparation packages/commerce → employer recruitment → administration.
- production_readiness_limitations: Do not claim full platform production readiness while backend production dependencies remain unresolved, especially production payment provider/webhook/refund/subscription support.
- out_of_scope_until_explicitly_authorized: Angular scaffold, `frontend/` creation, dependency install, API client generation, backend changes, canonical OpenAPI changes, Penpot changes, staging, committing, pushing.
- canonical_openapi: `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` (`3.1.1`, 106 paths, 142 operations, 110 schemas, Bearer JWT, CV multipart field `file`).

### Angular Official AI Guidance Addendum

Official Angular documentation and Angular-maintained AI guidance are framework guidance only. They are subordinate to explicit Nursing Platform decisions, repository governance, backend/OpenAPI business and security contracts, approved Penpot visual evidence, and active task scope. Use official Angular v22-compatible guidance for Angular framework questions; do not silently adopt later-major APIs or behaviors.

Official Angular Agent Skills (`angular-developer`, `angular-new-app`) and Angular CLI MCP are optional advisory aids only when already available and explicitly allowed by the active Task. Do not install skills, add MCP configuration, run write-capable MCP operations, or widen Task scope because a tool suggests it. Absence of those tools is not a blocker.

The approved future `T-FE-001` scaffold command is:

```bash
npx -p @angular/cli@22.1.4 ng new nursing-platform-frontend \
  --directory frontend \
  --routing \
  --style scss \
  --test-runner vitest \
  --standalone true \
  --strict true \
  --zoneless \
  --ssr false \
  --package-manager npm \
  --prefix np \
  --skip-git \
  --commit false \
  --defaults \
  --ai-config=none \
  --file-name-style-guide=2025
```

Do not execute this command until `T-FE-001` is explicitly authorized. If pinned Angular CLI `22.1.4` is later shown not to support `--ai-config=none` or `--file-name-style-guide=2025`, STOP AND ESCALATE; do not remove the flags silently and do not substitute a newer CLI without explicit approval.

## 4. Milestone Registry

Milestones are organizational groupings. Task/Gate dependency declarations are authoritative and must remain a DAG.

| milestone_id | name | purpose | member_task_ids | true prerequisite Milestone/Gate dependencies | completion criteria |
|---|---|---|---|---|---|
| `M-FE-001` | Workspace baseline | Scaffold and verify empty Angular workspace/toolchain. | `T-FE-001..T-FE-003` | Explicit T-FE-001 authorization | `GATE-FE-T001..T003` `VERIFIED` |
| `M-FE-002` | Tooling/static analysis | Dependency guardrails, lint, stylelint, CI-local command, a11y tooling. | `T-FE-004..T-FE-007`, `T-FE-014` | `GATE-FE-T003` | member gates `VERIFIED` or approved `BLOCKED` with blocker type `TOOLING_APPROVAL` |
| `M-FE-003` | Design/runtime foundation | Tokens, Material theme, base a11y, direction, responsive, forms. | `T-FE-008..T-FE-013` | `GATE-FE-T003` | member gates `VERIFIED` |
| `M-FE-004` | API client foundation | Generator spike, generator approval, generated client. | `T-FE-015..T-FE-017` | `GATE-FE-T003` | member gates `VERIFIED` |
| `M-FE-005` | API infrastructure | API config, Problem Details mapping, DTO adapter boundary. | `T-FE-018..T-FE-020` | `GATE-FE-T017` | member gates `VERIFIED` |
| `M-FE-006` | Auth/session foundation | Auth transport, tokens, bootstrap, refresh, bearer, logout. | `T-FE-021..T-FE-026` | `GATE-FE-T018..T020` | member gates `VERIFIED` |
| `M-FE-007` | Shell/routing/permission architecture | Shell, loading state, route registry, guards, route UX permission policy, navigation. | `T-FE-027..T-FE-032` | task-level gates only | member gates `VERIFIED` |
| `M-FE-008` | Shared UX patterns | Loading/errors, validation, empty/restricted, feedback, upload, pagination, visual method. | `T-FE-033..T-FE-039` | task-level gates only | member gates `VERIFIED` |
| `M-FE-009` | Auth screens | Approval packet and auth screen implementations/classifications. | `T-FE-040..T-FE-055` | task-level gates only | family packet `VERIFIED`; applicable screen tasks `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-010` | Nurse profile | Nurse approval and nurse profile screens. | `T-FE-052`, `T-FE-056..T-FE-065` | task-level gates only | applicable member gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-011` | Exams/learning | Exams approval, exam catalog/session/result/review/history/analytics. | `T-FE-061`, `T-FE-066..T-FE-069`, `T-FE-071..T-FE-074` | task-level gates only | applicable member gates `VERIFIED` |
| `M-FE-012` | Preparation packages | PP approval, offers, entitlements, practice, package exam/report, material-reader classification. | `T-FE-070`, `T-FE-075..T-FE-080` | task-level gates only | applicable member gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-013` | Commerce/payment | Commerce approval, products, orders, checkout, generic outcomes, production payment limitation. | `T-FE-077`, `T-FE-081..T-FE-084`, `T-FE-086..T-FE-089` | task-level gates only | applicable member gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-014` | Employer recruitment | Employer approval, employer profile, candidate search, candidate detail/request, requests. | `T-FE-085`, `T-FE-090..T-FE-096` | task-level gates only; no Nurse UI milestone dependency | applicable member gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-015` | Account management | Account approval and account screens/classifications. | `T-FE-092`, `T-FE-097`, `T-FE-099..T-FE-102` | task-level gates only | applicable member gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-016` | Administration | Admin approval and admin users/reference/exams/payment/PP/classifications. | `T-FE-098`, `T-FE-103..T-FE-112`, `T-FE-114..T-FE-121` | task-level gates only | applicable member gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-017` | System screens | Shared system approval and system screen implementations/classifications. | `T-FE-028`, `T-FE-035`, `T-FE-113`, `T-FE-116`, `T-FE-117` | task-level gates only | applicable system gates `VERIFIED` or `BLOCKED` with blocker types |
| `M-FE-018` | Visual/E2E/CI | Per-screen visual verification template, Playwright setup, critical E2E, CI quality pipeline. | `T-FE-122..T-FE-125` | task-level gates only | member gates `VERIFIED` |
| `M-FE-019` | Production hardening | Security, dependency, config, performance, browser, observability, a11y/RTL/responsive, payment release gates. | `T-FE-126..T-FE-137` | relevant feature/build gates | member gates `VERIFIED` or release blockers explicitly recorded |

## 5. Contract Clarification Model

`CONTRACT_CLARIFICATION_GATE` is a reusable process invoked locally by the affected Task only. One domain's clarification must not block unrelated domains.

Process:
1. Inspect backend source.
2. Inspect endpoint authorization metadata.
3. Inspect request/response/error contract.
4. Compare source/runtime expectations with canonical OpenAPI.
5. Classify as `FRONTEND_PLAN_CORRECT`, `OPENAPI_METADATA_DEFECT`, `BACKEND_BEHAVIOR_MISSING`, or `DOCUMENTATION_STALE`.

If backend/OpenAPI conflict: STOP AND ESCALATE.

## 6. Contract Registry Authority Rule

The `C-*` registry below is a planning/navigation index only. The canonical OpenAPI snapshot and backend authority rules remain the source of truth.

Before any backend-consuming Task moves from `NOT STARTED` to `IN PROGRESS`, that Task must confirm current contract evidence from canonical OpenAPI and record, as applicable: HTTP method, route, operationId, request schema, response schema, success statuses, error statuses, Problem Details variants, Bearer/auth requirement, roles/permissions where documented, pagination, and multipart behavior.

If this registry summary and canonical OpenAPI differ, canonical OpenAPI/backend evidence wins and the discrepancy must be recorded. If the contract cannot be established, set `status: BLOCKED` and `blocker_types: [CONTRACT_CLARIFICATION]`.

## 7. Contract Registry

| contract_id | planning index summary |
|---|---|
| `C-AUTH-LOGIN` | `POST /api/v1/auth/login`, operationId `Login`, request `LoginCommand`, success `200`, auth unspecified; coded errors require local clarification. |
| `C-AUTH-REFRESH` | `POST /api/v1/auth/refresh`, `RefreshToken`, request `RotateRefreshTokenCommand`, success `200`, auth unspecified. |
| `C-AUTH-REGISTER` | `POST /api/v1/auth/register`, `RegisterUser`, request `RegisterUserRequest`, success `200`, error `401`, Bearer; local clarification required. |
| `C-AUTH-SEND-VERIFY` | `POST /api/v1/auth/send-verification-email`, `SendVerificationEmail`, success `200`, error `401`, Bearer; local clarification required. |
| `C-AUTH-VERIFY` | `POST /api/v1/auth/verify-email`, `VerifyEmail`, request `VerifyEmailRequest`, success `200`. |
| `C-AUTH-FORGOT` | `POST /api/v1/auth/forgot-password`, `ForgotPassword`, request `ForgotPasswordRequest`, success `200`. |
| `C-AUTH-RESET` | `POST /api/v1/auth/reset-password`, `ResetPassword`, request `ResetPasswordRequest`, success `200`. |
| `C-ME` | `GET /api/v1/me`, `GetCurrentUser`, success `200`, error `401`, Bearer; response shape/roles/permissions require local confirmation before permission policy. |
| `C-ERROR` | `ProblemDetails`, `ValidationProblemDetails`, `CodedProblemDetails`, `RetryableProblemDetails`. |
| `C-NUR-PROFILE` | `GET/PUT /api/v1/me/nurse-profile`, `GetCurrentNurseProfile`, `UpsertCurrentNurseProfile`, request `UpsertNurseProfileCommand`, `200/401`, Bearer. |
| `C-NUR-EXP` | `GET/POST /experiences`, `PUT/DELETE /experiences/{id}`, create/update request commands, `200/401`, Bearer. |
| `C-NUR-EDU` | `GET/POST /education`, `PUT/DELETE /education/{id}`, create/update request commands, `200/401`, Bearer. |
| `C-NUR-CERT` | `GET/POST /certificates`, `PUT/DELETE /certificates/{id}`, create/update request commands, `200/401`, Bearer. |
| `C-NUR-SKILLS-LANG` | `GET/PUT /skills`, `GET/PUT /languages`, update command schemas, `200/401`, Bearer. |
| `C-NUR-CV` | `GET/POST/DELETE /api/v1/me/nurse-profile/cv`, Bearer via nurse-profile group `RequireAuthorization()`, POST upload `multipart/form-data` field `file`; source/tests and corrected canonical OpenAPI agree DELETE success is `204 NoContent`. Accepted file constraints: `.pdf`, `.doc`, `.docx`; content types `application/pdf`, `application/msword`, `application/vnd.openxmlformats-officedocument.wordprocessingml.document`; max size `5 * 1024 * 1024` bytes; GET/POST return `NurseCvDocumentDto` metadata only. |
| `C-NUR-CONTACT` | `GET /contact-requests`, `POST /{id}/approve`, `POST /{id}/reject`, `200/401`, Bearer. |
| `C-EXAM-CATALOG` | `GET /api/v1/exams` `ListExams`; `GET /api/v1/exams/{id}` `GetExam`; `200/401`, Bearer; schemas need local clarification. |
| `C-EXAM-START` | `POST /api/v1/exams/{id}/sessions`, `StartExamSession`, `200/401`, Bearer. |
| `C-EXAM-SESSION` | `GET /exam-sessions/{id}`, `PUT /answers` request `SaveExamSessionAnswersRequest`, `POST /submit`, `200/401`, Bearer. |
| `C-EXAM-RESULT-REVIEW` | `GET /result`, `GET /review`, `200/401`, Bearer; disclosure requires local clarification. |
| `C-EXAM-ANALYTICS` | Nurse exam attempts and analytics summary/by-exam/by-category/trends, `200/401`, Bearer. |
| `C-PP-OFFERS` | `GET /api/v1/preparation-packages/offers`, `ListPreparationPackageOffers`, `PaginatedResultOfPreparationPackageOfferListItemDto`, `400 ValidationProblemDetails`; `GET /offers/{slug}`, `PreparationPackageOfferDetailDto`, `404 ProblemDetails`. |
| `C-PP-ENTITLEMENTS` | Entitlement list/detail, `PaginatedResultOfPackageEntitlementListItemDto`, `PackageEntitlementDetailDto`, `400/401/404`, Bearer. |
| `C-PP-PRACTICE` | Practice progress/answer, `PackagePracticeProgressSummaryDto`, `SubmitPackagePracticeAnswerRequest`, `PackagePracticeAnswerSubmissionDto`, `400/401/404/409`, Bearer. |
| `C-PP-EXAM-REPORT` | Package exam start/report, `PackageExamSessionStartDto`, `PackageAnalyticalReportDto`, `409 CodedProblemDetails`, Bearer. |
| `C-PAY-PRODUCTS` | `GET /api/v1/payment/products`, `GET /payment/products/{id}`, `200/401`, Bearer; schemas require local clarification. |
| `C-PAY-ORDERS` | Create/list/detail/cancel nurse payment orders; create request `CreatePaymentOrderRequest`; create success `201 PaymentOrderDto`; errors `400/401/404/409`. |
| `C-PAY-CHECKOUT` | `POST /orders/{orderId}/checkout`, request `StartPaymentCheckoutRequest`, success `PaymentCheckoutSessionDto`, `409 RetryableProblemDetails`, `503 ProblemDetails`; sandbox complete Development/Test only. |
| `C-EMP-PROFILE` | Employer profile and organization GET/PUT, update request schemas, `200/401`, Bearer. |
| `C-EMP-CANDIDATES` | `GET /api/v1/recruitment/candidates`, `ListRecruitmentCandidates`, Bearer, candidate search/list only; source/OpenAPI expose no candidate detail route such as `/api/v1/recruitment/candidates/{id}` and no candidate-detail DTO. List DTO is `CandidateListItemDto`; detail screen `EMP-005` is backend gap unless a future backend contract is added. |
| `C-EMP-REQUESTS` | Create/list/detail/cancel recruitment contact requests, create request `CreateContactRequestRequest`, `200/401`, Bearer. |
| `C-ADM-USERS` | `GET /api/v1/users`, `GET /api/v1/users/{id}`, `200/401`, Bearer; raw JSON sensitive-field verification required. |
| `C-ADM-EXAM-CATEGORIES` | Admin exam category list/create/get/update/delete/archive/restore, request `CreateAdminExamCategoryRequest`, `UpdateAdminExamCategoryRequest`, `200/401`, Bearer. |
| `C-ADM-EXAMS` | Admin exams lifecycle, versions, questions, answer options; request schemas include admin exam/question/option commands; `200/401`, Bearer; schemas/permissions need local checks. |
| `C-ADM-PAY-PRODUCTS` | Admin payment product list/create/get/update/archive/restore, request `CreateAdminPaymentProductRequest`, `UpdateAdminPaymentProductRequest`, `200/401`, Bearer. |
| `C-ADM-PP-TOPICS` | Admin PP reporting topics list/create/update/archive; paginated DTO; `400/401/403/404/409` variants. |
| `C-ADM-PP-PROFILES` | Admin PP reporting profiles list/create/get/publish; paginated/publication DTO; `400/401/403/404/409` variants. |
| `C-ADM-PP-MATERIALS` | Admin PP materials and material versions create/update/publish/retire; `400/401/403/404/409` variants. |
| `C-ADM-PP-PRACTICE` | Admin PP practice collections and versions create/update/publish/retire; `400/401/403/404/409` variants. |
| `C-ADM-PP-PACKAGES` | Admin PP packages/package versions/validation/publish/retire; `PackagePublicationValidationDto`; `400/401/403/404/409` variants. |
| `C-ADM-PP-OFFERS` | Admin PP offers list/create/update/activate/deactivate; paginated/admin offer DTO; `400/401/403/404/409` variants. |

## 8. Screen Approval Model

Family approval Tasks/Gates represent `SCREEN APPROVAL PACKET REVIEW`. A family approval gate becoming `VERIFIED` means the packet has been reviewed and every included screen has an explicit decision. It does not mean every screen is approved.

Allowed per-screen `approval_decision` values: `APPROVED`, `BLOCKED`, `DEFERRED`.

A screen implementation Task may begin only when: its family approval Gate is `VERIFIED`; the exact screen's `approval_decision` is `APPROVED`; all other prerequisites pass. No Agent may infer approval from the family gate alone or self-approve a screen.

## 9. Visual Verification Model

`T-FE-039` / `GATE-FE-T039` defines the reusable Visual/Penpot Verification method only: reference handling, viewport evidence, screenshot procedure, responsive comparison, RTL comparison, and intentional-difference documentation.

`T-FE-122` is a reusable execution/template mechanism, not a once-only global completion gate. Its state cannot substitute for individual screen verification. Each screen's own Gate remains authoritative and must attach per-screen visual evidence after implementation.

## 10. Routing / Permission Architecture

Routing concerns are separated:

- `T-FE-029` — Canonical Route Registry.
- `T-FE-030` — Authentication/Public Guards.
- `T-FE-031` — Route-Level Permission UX Policy.
- `T-FE-032` — Permission-Aware Navigation.

Rules:
- Backend is the security boundary.
- Route permission behavior is frontend UX enforcement only.
- Navigation visibility is not authorization.
- `/api/v1/me` approved roles/permissions contract is authoritative.
- Route configuration does not own business workflows.
- Product routes extend the canonical route registry through explicitly authorized Feature Tasks.
- Completed-feature protection must not prevent a future approved Feature Task from intentionally extending the route registry when that registry file is declared in the Task's authorized affected scope.

## 11. Task Registry

All Tasks have initial `status: NOT STARTED`.

| task_id | milestone_id | name | dependencies | scope/contract/design summary | blocker_types |
|---|---|---|---|---|---|
| `T-FE-001` | `M-FE-001` | Scaffold Angular workspace | explicit future authorization | exact scaffold command into `frontend/` with `--ai-config=none` and `--file-name-style-guide=2025`; no product code | `SCOPE`,`DEPENDENCY` |
| `T-FE-002` | `M-FE-001` | Pin Node/npm/toolchain | `GATE-FE-T001` | Node `22.23.1`, npm `11.6.0` | `DEPENDENCY` |
| `T-FE-003` | `M-FE-001` | Baseline install/build/test | `GATE-FE-T002` | generated workspace baseline only | `DEPENDENCY` |
| `T-FE-004` | `M-FE-002` | Dependency guardrails | `GATE-FE-T003` | block unapproved deps | `DEPENDENCY` |
| `T-FE-005` | `M-FE-002` | TypeScript/Angular lint setup | `GATE-FE-T003` | tooling approval if needed | `TOOLING_APPROVAL` |
| `T-FE-006` | `M-FE-002` | SCSS/stylelint setup | `GATE-FE-T003` | physical directional-property prohibition | `TOOLING_APPROVAL` |
| `T-FE-007` | `M-FE-002` | CI-local quality command | `GATE-FE-T005`,`GATE-FE-T006` | local aggregation only | `DEPENDENCY` |
| `T-FE-008` | `M-FE-003` | Runtime design tokens | `GATE-FE-T003` | approved tokens only | `DESIGN` |
| `T-FE-009` | `M-FE-003` | Angular Material theme bridge | `GATE-FE-T008` | single project theme | `DESIGN` |
| `T-FE-010` | `M-FE-003` | Base accessibility styles | `GATE-FE-T008` | focus/touch/SR utilities | `DESIGN` |
| `T-FE-011` | `M-FE-003` | Direction/locale foundation | `GATE-FE-T008` | `dir`, fonts, bidi; no full i18n | `DESIGN` |
| `T-FE-012` | `M-FE-003` | Responsive helpers | `GATE-FE-T008`,`GATE-FE-T011` | breakpoints/gutters/logical helpers | `DESIGN` |
| `T-FE-013` | `M-FE-003` | Standard form controls | `GATE-FE-T008..T012` | 64px/12px/48px; no compact | `DESIGN` |
| `T-FE-014` | `M-FE-002` | Accessibility automation setup | `GATE-FE-T003` | AXE-compatible tooling approval | `TOOLING_APPROVAL` |
| `T-FE-015` | `M-FE-004` | API generator spike | `GATE-FE-T003` | canonical OpenAPI; no integration | `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION` |
| `T-FE-016` | `M-FE-004` | API generator approval | `GATE-FE-T015` | exact version/config/location/Git policy | `TOOLING_APPROVAL` |
| `T-FE-017` | `M-FE-004` | Generate isolated API client | `GATE-FE-T016` | generated client only; no manual edits | `CONTRACT_CLARIFICATION` |
| `T-FE-018` | `M-FE-005` | API base/config | `GATE-FE-T017` | `/api/v1`; no secrets | `RUNTIME_DEPLOYMENT` |
| `T-FE-019` | `M-FE-005` | Problem Details mapping | `GATE-FE-T017` | `C-ERROR` | `CONTRACT_CLARIFICATION` |
| `T-FE-020` | `M-FE-005` | DTO adapter boundary | `GATE-FE-T017`,`GATE-FE-T019` | generated DTO adapter policy only | `CONTRACT_CLARIFICATION` |
| `T-FE-021` | `M-FE-006` | Auth transport | `GATE-FE-T018..T020` | `C-AUTH-LOGIN`,`C-AUTH-REFRESH`,`C-ME` | `CONTRACT_CLARIFICATION` |
| `T-FE-022` | `M-FE-006` | Token storage abstraction | `GATE-FE-T021` | MVP only; production posture later | `SECURITY` |
| `T-FE-023` | `M-FE-006` | Session bootstrap | `GATE-FE-T021`,`GATE-FE-T022` | `/me` bootstrap | `CONTRACT_CLARIFICATION` |
| `T-FE-024` | `M-FE-006` | Single-flight refresh | `GATE-FE-T022`,`GATE-FE-T023` | refresh coordination only | `SECURITY` |
| `T-FE-025` | `M-FE-006` | Bearer interceptor | `GATE-FE-T024` | bearer injection; no business logic | `SECURITY` |
| `T-FE-026` | `M-FE-006` | Local logout MVP | `GATE-FE-T023..T025` | no server revocation claim | `CONTRACT_CLARIFICATION` |
| `T-FE-027` | `M-FE-007` | Shell frame | `GATE-FE-T009..T012`,`GATE-FE-T023` | landmarks/router outlet | `DESIGN` |
| `T-FE-028` | `M-FE-017` | System loading route state | `GATE-FE-T027`,`GATE-FE-T113` | owns `SYS-001` | `DESIGN` |
| `T-FE-029` | `M-FE-007` | Canonical route registry | `GATE-FE-T027` | all product routes register here | `SCOPE` |
| `T-FE-030` | `M-FE-007` | Auth/public guards | `GATE-FE-T023..T025`,`GATE-FE-T029` | auth guard/public-only guard | `SECURITY` |
| `T-FE-031` | `M-FE-007` | Route-level UX permission policy | `GATE-FE-T023`,`GATE-FE-T029` | UX only; backend is security | `SECURITY`,`CONTRACT_CLARIFICATION` |
| `T-FE-032` | `M-FE-007` | Permission-aware navigation | `GATE-FE-T031` | presentation only | `DESIGN` |
| `T-FE-033` | `M-FE-008` | Loading/error/retry pattern | `GATE-FE-T019`,`GATE-FE-T010` | no feature copy | `DESIGN` |
| `T-FE-034` | `M-FE-008` | Form validation pattern | `GATE-FE-T013`,`GATE-FE-T019` | validation display only | `CONTRACT_CLARIFICATION` |
| `T-FE-035` | `M-FE-017` | Empty/no-results/restricted pattern | `GATE-FE-T031`,`GATE-FE-T113` | owns `SYS-006/007` | `DESIGN` |
| `T-FE-036` | `M-FE-008` | Confirmation/feedback/live-region | `GATE-FE-T009`,`GATE-FE-T019` | keyboard/focus/live region | `DESIGN` |
| `T-FE-037` | `M-FE-008` | File upload pattern | `GATE-FE-T034` | multipart helper | `CONTRACT_CLARIFICATION` |
| `T-FE-038` | `M-FE-008` | List/filter/pagination pattern | `GATE-FE-T033`,`GATE-FE-T035` | server pagination | `CONTRACT_CLARIFICATION` |
| `T-FE-039` | `M-FE-008` | Visual verification method | `GATE-FE-T012`,`GATE-FE-T014` | method only; not approval | `TOOLING_APPROVAL`,`DESIGN` |
| `T-FE-040` | `M-FE-009` | Auth screen approval packet | design/backend/state review | `AUTH-001..012` packet review | `DESIGN` |
| `T-FE-041` | `M-FE-009` | `AUTH-001` Sign In | `GATE-FE-T021..T025`,`GATE-FE-T030`,`GATE-FE-T034`,`GATE-FE-T040` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-042` | `M-FE-009` | Registration contract clarification | local only | `RegisterUser` auth/public behavior | `CONTRACT_CLARIFICATION` |
| `T-FE-043` | `M-FE-009` | `AUTH-002` Role Selection | `GATE-FE-T040`,`GATE-FE-T042` | screen implementation after exact approval | `DESIGN` |
| `T-FE-044` | `M-FE-009` | `AUTH-003` Nurse Registration | `GATE-FE-T040`,`GATE-FE-T042`,`GATE-FE-T034` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-045` | `M-FE-009` | `AUTH-004` Employer Registration | `GATE-FE-T040`,`GATE-FE-T042`,`GATE-FE-T034` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-046` | `M-FE-009` | Verification email contract clarification | local only | send/verify behavior | `CONTRACT_CLARIFICATION` |
| `T-FE-047` | `M-FE-009` | `AUTH-005/006` Email Verification journey | `GATE-FE-T040`,`GATE-FE-T046` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-048` | `M-FE-009` | `AUTH-007` Forgot Password | `GATE-FE-T040`,`GATE-FE-T034` | screen implementation after exact approval | `DESIGN` |
| `T-FE-049` | `M-FE-009` | `AUTH-008` Reset Password | `GATE-FE-T048`,`GATE-FE-T034` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-050` | `M-FE-009` | `AUTH-009` Reset Success | `GATE-FE-T049` | screen implementation after exact approval | `DESIGN` |
| `T-FE-051` | `M-FE-009` | `AUTH-010` Session Expired | `GATE-FE-T026`,`GATE-FE-T040` | screen implementation after exact approval | `DESIGN` |
| `T-FE-052` | `M-FE-010` | Nurse screen approval packet | design/backend/state review | `NUR-001..013` packet review | `DESIGN` |
| `T-FE-053` | `M-FE-009` | `AUTH-011` Access Denied | `GATE-FE-T031`,`GATE-FE-T040` | screen implementation after exact approval | `DESIGN` |
| `T-FE-054` | `M-FE-009` | Account inactive contract clarification | local only | login/current-user coded inactive state | `CONTRACT_CLARIFICATION` |
| `T-FE-055` | `M-FE-009` | `AUTH-012` Account Inactive | `GATE-FE-T040`,`GATE-FE-T054` | implement only if contract-backed | `BACKEND`,`CONTRACT_CLARIFICATION`,`DESIGN` |
| `T-FE-056` | `M-FE-010` | `NUR-001/002` read-only profile overview | `GATE-FE-T021..T025`,`GATE-FE-T030`,`GATE-FE-T033`,`GATE-FE-T052` | vertical slice A | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-057` | `M-FE-010` | `NUR-003` personal info | `GATE-FE-T056`,`GATE-FE-T034` | profile upsert | `DESIGN` |
| `T-FE-058` | `M-FE-010` | `NUR-004/005` experience CRUD | `GATE-FE-T056`,`GATE-FE-T034`,`GATE-FE-T036` | cohesive list+edit | `DESIGN` |
| `T-FE-059` | `M-FE-010` | `NUR-006/007` education CRUD | `GATE-FE-T056`,`GATE-FE-T034`,`GATE-FE-T036` | cohesive list+edit | `DESIGN` |
| `T-FE-060` | `M-FE-010` | `NUR-008/009` certificate CRUD | `GATE-FE-T056`,`GATE-FE-T034`,`GATE-FE-T036` | cohesive list+edit | `DESIGN` |
| `T-FE-061` | `M-FE-011` | Exams screen approval packet | design/backend/state review | `EXM-001..010` packet review | `DESIGN` |
| `T-FE-062` | `M-FE-010` | `NUR-010/011` skills/languages | `GATE-FE-T056`,`GATE-FE-T034` | paired profile update contracts | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-063` | `M-FE-010` | CV contract clarification | local only | file constraints only | `CONTRACT_CLARIFICATION` |
| `T-FE-064` | `M-FE-010` | `NUR-012` CV management | `GATE-FE-T056`,`GATE-FE-T037`,`GATE-FE-T052`,`GATE-FE-T063` | vertical slice B | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-065` | `M-FE-010` | `NUR-013` profile completion clarification/implementation | `GATE-FE-T056`,`GATE-FE-T052` | local clarification only | `BACKEND`,`CONTRACT_CLARIFICATION`,`DESIGN` |
| `T-FE-066` | `M-FE-011` | Exam catalog/detail contract clarification | local only | response schemas/access fields | `CONTRACT_CLARIFICATION` |
| `T-FE-067` | `M-FE-011` | `EXM-001/002` exam catalog/detail | `GATE-FE-T030`,`GATE-FE-T038`,`GATE-FE-T061`,`GATE-FE-T066` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-068` | `M-FE-011` | `EXM-003/004` purchase-required/instructions/start | `GATE-FE-T067` | commerce only as CTA dependency if needed | `DESIGN` |
| `T-FE-069` | `M-FE-011` | `EXM-005/006` exam session/save/submit | `GATE-FE-T068`,`GATE-FE-T036` | runtime/session | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-070` | `M-FE-012` | Preparation-package screen approval packet | design/backend/state review | PP screen packet review | `DESIGN` |
| `T-FE-071` | `M-FE-011` | `EXM-007` Exam Result | `GATE-FE-T069` | result only | `DESIGN` |
| `T-FE-072` | `M-FE-011` | `EXM-009` Answer Review | `GATE-FE-T069` | disclosure-sensitive review only | `SECURITY`,`DESIGN` |
| `T-FE-073` | `M-FE-011` | `EXM-010` Exam History | `GATE-FE-T069`,`GATE-FE-T038` | attempts/history | `DESIGN` |
| `T-FE-074` | `M-FE-011` | `EXM-008` Performance Analytics | `GATE-FE-T069` | analytics only | `DESIGN` |
| `T-FE-075` | `M-FE-012` | PP public offers | `GATE-FE-T018..T020`,`GATE-FE-T038`,`GATE-FE-T070` | no Exams dependency | `DESIGN` |
| `T-FE-076` | `M-FE-012` | PP entitlements | `GATE-FE-T021..T025`,`GATE-FE-T038`,`GATE-FE-T070` | auth/API only | `DESIGN` |
| `T-FE-077` | `M-FE-013` | Commerce screen approval packet | design/backend/state review | `COM-001..008` packet review | `DESIGN` |
| `T-FE-078` | `M-FE-012` | PP practice | `GATE-FE-T076`,`GATE-FE-T034` | entitlement + practice only | `DESIGN` |
| `T-FE-079` | `M-FE-012` | PP package exam/report | `GATE-FE-T076` | exact session primitive only if reused | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-080` | `M-FE-012` | PP material reader classification | local contract check | no implementation if backend absent | `BACKEND` |
| `T-FE-081` | `M-FE-013` | Payment product contract clarification | local only | product response schemas | `CONTRACT_CLARIFICATION` |
| `T-FE-082` | `M-FE-013` | `COM-001/002` payment products | `GATE-FE-T030`,`GATE-FE-T077`,`GATE-FE-T081` | screen implementation after exact approval | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-083` | `M-FE-013` | Payment order contract clarification | local only | order list/detail/cancel schemas | `CONTRACT_CLARIFICATION` |
| `T-FE-084` | `M-FE-013` | `COM-003` checkout/order creation | `GATE-FE-T082`,`GATE-FE-T083`,`GATE-FE-T034`,`GATE-FE-T077` | primary owner of COM-003 | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-085` | `M-FE-014` | Employer screen approval packet | design/backend/state review | `EMP-001..008` packet review | `DESIGN` |
| `T-FE-086` | `M-FE-013` | `COM-004` checkout processing | `GATE-FE-T084` | start checkout/retryable | `DESIGN` |
| `T-FE-087` | `M-FE-013` | `COM-005/006` generic outcome screens | `GATE-FE-T084`,`GATE-FE-T086`,`GATE-FE-T077` | generic server-backed only | `BACKEND`,`DESIGN` |
| `T-FE-088` | `M-FE-013` | `COM-007/008` orders list/detail | `GATE-FE-T084`,`GATE-FE-T038`,`GATE-FE-T077` | order history/detail | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-089` | `M-FE-013` | Production payment limitation | `GATE-FE-T086` | provider-specific release gate | `BACKEND`,`EXTERNAL` |
| `T-FE-090` | `M-FE-014` | `EMP-001` employer profile/home | `GATE-FE-T030`,`GATE-FE-T034`,`GATE-FE-T085` | no Nurse UI dependency | `DESIGN` |
| `T-FE-091` | `M-FE-014` | `EMP-002/003/004` candidate search/results | `GATE-FE-T090`,`GATE-FE-T038`,`GATE-FE-T085` | no candidate detail | `DESIGN` |
| `T-FE-092` | `M-FE-015` | Account screen approval packet | design/backend/state review | `ACC-001..006` packet review | `DESIGN` |
| `T-FE-093` | `M-FE-014` | Candidate detail contract clarification | local only | `EMP-005` only | `CONTRACT_CLARIFICATION`,`BACKEND` |
| `T-FE-094` | `M-FE-014` | `EMP-005/006` candidate profile/request | `GATE-FE-T091`,`GATE-FE-T093`,`GATE-FE-T034`,`GATE-FE-T085` | profile only if contract-backed | `BACKEND`,`DESIGN` |
| `T-FE-095` | `M-FE-014` | `EMP-007/008` employer requests | `GATE-FE-T094`,`GATE-FE-T038`,`GATE-FE-T085` | list/detail/cancel | `DESIGN` |
| `T-FE-096` | `M-FE-014` | Nurse received contact requests | `GATE-FE-T056`,`GATE-FE-T038` | not EMP canonical owner | `DESIGN` |
| `T-FE-097` | `M-FE-015` | `ACC-001/002` account overview/details | `GATE-FE-T023`,`GATE-FE-T033`,`GATE-FE-T092` | `/me` only | `DESIGN` |
| `T-FE-098` | `M-FE-016` | Admin screen approval packet | design/backend/state review | `ADM-001..010` packet review | `DESIGN` |
| `T-FE-099` | `M-FE-015` | `ACC-003` change password classification | local contract check | backend gap unless found | `BACKEND` |
| `T-FE-100` | `M-FE-015` | `ACC-004` sessions classification | local contract check | backend gap unless found | `BACKEND` |
| `T-FE-101` | `M-FE-015` | `ACC-005` notification preferences classification | local contract check | backend gap unless found | `BACKEND` |
| `T-FE-102` | `M-FE-015` | `ACC-006` account status classification | local contract check | coded state only if found | `BACKEND`,`CONTRACT_CLARIFICATION` |
| `T-FE-103` | `M-FE-016` | `ADM-001` dashboard classification | local contract check + approval | metrics absent | `BACKEND`,`DESIGN` |
| `T-FE-104` | `M-FE-016` | `ADM-002/003` users | `GATE-FE-T030`,`GATE-FE-T038`,`GATE-FE-T098` | raw JSON sensitive checks | `SECURITY`,`DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-105` | `M-FE-016` | `ADM-004` roles/permissions classification | local contract check | backend gap unless found | `BACKEND` |
| `T-FE-106` | `M-FE-016` | `ADM-005` exam categories/reference data | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | exam categories only | `DESIGN` |
| `T-FE-107` | `M-FE-016` | `ADM-006` admin exam lifecycle | `GATE-FE-T106`,`GATE-FE-T098` | split | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-108` | `M-FE-016` | Admin exam version lifecycle | `GATE-FE-T107` | split | `DESIGN` |
| `T-FE-109` | `M-FE-016` | `ADM-007` question lifecycle | `GATE-FE-T108` | split | `DESIGN` |
| `T-FE-110` | `M-FE-016` | Answer option lifecycle | `GATE-FE-T109` | split | `DESIGN` |
| `T-FE-111` | `M-FE-016` | `ADM-008` admin payment products | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | no orders | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `T-FE-112` | `M-FE-016` | `ADM-009/010` admin orders/recruitment classification | local contract check | backend gap unless found | `BACKEND` |
| `T-FE-113` | `M-FE-017` | Shared System screen approval packet | design/backend/runtime review | `SYS-001..007` packet review | `DESIGN` |
| `T-FE-114` | `M-FE-016` | Admin PP reporting topics | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | split | `DESIGN` |
| `T-FE-115` | `M-FE-016` | Admin PP reporting profiles | `GATE-FE-T114` | split | `DESIGN` |
| `T-FE-116` | `M-FE-017` | `SYS-004` Offline classification/implementation | `GATE-FE-T113` | runtime/deployment contract | `RUNTIME_DEPLOYMENT` |
| `T-FE-117` | `M-FE-017` | `SYS-005` Maintenance classification/implementation | `GATE-FE-T113` | runtime/deployment contract | `RUNTIME_DEPLOYMENT` |
| `T-FE-118` | `M-FE-016` | Admin PP materials | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | split | `DESIGN` |
| `T-FE-119` | `M-FE-016` | Admin PP practice collections | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | split | `DESIGN` |
| `T-FE-120` | `M-FE-016` | Admin PP packages/package versions | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | split | `DESIGN` |
| `T-FE-121` | `M-FE-016` | Admin PP offers | `GATE-FE-T034`,`GATE-FE-T038`,`GATE-FE-T098` | split | `DESIGN` |
| `T-FE-122` | `M-FE-018` | Per-screen visual verification execution template | each implemented screen gate | template only, not global substitute | `DESIGN` |
| `T-FE-123` | `M-FE-018` | Playwright setup | tooling approval | no install now | `TOOLING_APPROVAL` |
| `T-FE-124` | `M-FE-018` | Critical E2E journeys | `GATE-FE-T123` + feature gates | supported flows only | `EXTERNAL`,`DEPENDENCY` |
| `T-FE-125` | `M-FE-018` | CI quality pipeline | tooling gates | no CI edits until approved | `DEPENDENCY` |
| `T-FE-126` | `M-FE-019` | CSP/Trusted Types | feature/build gates | production hardening | `SECURITY` |
| `T-FE-127` | `M-FE-019` | XSRF/CSRF posture | auth/payment gates | security | `SECURITY` |
| `T-FE-128` | `M-FE-019` | Production auth/session posture | auth gates | security decision | `SECURITY` |
| `T-FE-129` | `M-FE-019` | Dependency audit | tooling gate | no auto-upgrade | `DEPENDENCY`,`SECURITY` |
| `T-FE-130` | `M-FE-019` | Source-map/config hardening | build/config gates | no secrets | `SECURITY` |
| `T-FE-131` | `M-FE-019` | Bundle budgets/performance | build gate | perf | `DEPENDENCY` |
| `T-FE-132` | `M-FE-019` | Browser matrix | test/build gates | human-approved matrix | `EXTERNAL` |
| `T-FE-133` | `M-FE-019` | Observability/redaction | error/auth/payment/exam gates | no sensitive logs | `SECURITY` |
| `T-FE-134` | `M-FE-019` | Final accessibility audit | implemented screens | manual + automation | `DESIGN` |
| `T-FE-135` | `M-FE-019` | Final RTL audit | direction + screens | no full translation claim | `DESIGN` |
| `T-FE-136` | `M-FE-019` | Final responsive audit | responsive + screens | breakpoints | `DESIGN` |
| `T-FE-137` | `M-FE-019` | Production payment release decision | `GATE-FE-T089` | no provider invention | `BACKEND`,`EXTERNAL` |

## 12. Subtask Registry

All Subtasks initially have `status: NOT STARTED`. Each Subtask inherits its parent Task's authorized scope, excluded scope, backend/OpenAPI reference, design/Penpot reference, acceptance criteria, verification evidence requirements, and STOP conditions unless a narrower scope is listed.

| subtask_id | parent_task | purpose |
|---|---|---|
| `ST-FE-001` | `T-FE-001` | Execute exact approved Angular scaffold command with `--ai-config=none` and `--file-name-style-guide=2025`; verify isolation, 2025 root filenames, no generated AI/MCP config, and no historical filename renames. |
| `ST-FE-002` | `T-FE-002` | Pin and verify Node/npm toolchain metadata. |
| `ST-FE-003` | `T-FE-003` | Run baseline install/test/build evidence. |
| `ST-FE-004` | `T-FE-004` | Add dependency approval/denylist guard. |
| `ST-FE-005` | `T-FE-005` | Approve/configure TypeScript/Angular lint command. |
| `ST-FE-006` | `T-FE-006` | Configure SCSS/stylelint and physical direction prohibition. |
| `ST-FE-007` | `T-FE-007` | Compose local quality command without CI mutation. |
| `ST-FE-008` | `T-FE-008` | Define runtime semantic tokens from approved foundation. |
| `ST-FE-009` | `T-FE-009` | Bridge tokens to single Angular Material theme. |
| `ST-FE-010` | `T-FE-010` | Establish focus/touch/screen-reader utilities. |
| `ST-FE-011` | `T-FE-011` | Implement document `dir`, direction state, font mapping, bidi checks. |
| `ST-FE-012` | `T-FE-012` | Implement breakpoint/gutter/logical layout helpers. |
| `ST-FE-013` | `T-FE-013` | Implement standard field/select/control foundation. |
| `ST-FE-014` | `T-FE-014` | Approve and wire component a11y automation. |
| `ST-FE-015` | `T-FE-015` | Spike generator against canonical OpenAPI critical contracts. |
| `ST-FE-016` | `T-FE-016` | Record exact generator/version/config approval. |
| `ST-FE-017` | `T-FE-017` | Generate isolated client and drift check. |
| `ST-FE-018` | `T-FE-018` | Configure API base URL/proxy/public config. |
| `ST-FE-019` | `T-FE-019` | Normalize backend Problem Details variants. |
| `ST-FE-020` | `T-FE-020` | Define generated DTO adapter boundary only. |
| `ST-FE-021` | `T-FE-021` | Implement typed login/refresh/current-user transport. |
| `ST-FE-022` | `T-FE-022` | Implement token storage abstraction. |
| `ST-FE-023` | `T-FE-023` | Implement session bootstrap state machine. |
| `ST-FE-024` | `T-FE-024` | Implement single-flight refresh coordination. |
| `ST-FE-025` | `T-FE-025` | Implement Bearer interceptor and exclusions. |
| `ST-FE-026` | `T-FE-026` | Implement local logout without server revocation claim. |
| `ST-FE-027` | `T-FE-027` | Implement accessible shell frame. |
| `ST-FE-028` | `T-FE-028` | Implement approved loading shell state. |
| `ST-FE-029` | `T-FE-029` | Implement canonical route registry entries/policy shape. |
| `ST-FE-030` | `T-FE-030` | Implement auth/public route guards. |
| `ST-FE-031` | `T-FE-031` | Implement route-level UX permission policy. |
| `ST-FE-032` | `T-FE-032` | Implement permission-aware navigation presentation. |
| `ST-FE-033` | `T-FE-033` | Implement loading/error/retry reusable pattern. |
| `ST-FE-034` | `T-FE-034` | Implement backend validation display pattern. |
| `ST-FE-035` | `T-FE-035` | Implement empty/no-results/restricted states. |
| `ST-FE-036` | `T-FE-036` | Implement confirmation/feedback/live-region pattern. |
| `ST-FE-037` | `T-FE-037` | Implement multipart file upload helper. |
| `ST-FE-038` | `T-FE-038` | Implement server list/filter/pagination helper. |
| `ST-FE-039` | `T-FE-039` | Define visual verification workflow/tooling method. |
| `ST-FE-040` | `T-FE-040` | Prepare Authentication screen approval packet. |
| `ST-FE-041` | `T-FE-041` | Build `AUTH-001` Sign In route/form after approval. |
| `ST-FE-042` | `T-FE-042` | Clarify `RegisterUser` auth/public behavior locally. |
| `ST-FE-043` | `T-FE-043` | Build `AUTH-002` Role Selection after approval. |
| `ST-FE-044` | `T-FE-044` | Build `AUTH-003` Nurse Registration after approval. |
| `ST-FE-045` | `T-FE-045` | Build `AUTH-004` Employer Registration after approval. |
| `ST-FE-046` | `T-FE-046` | Clarify send/verify email auth and error behavior. |
| `ST-FE-047` | `T-FE-047` | Build `AUTH-005/006` Email Verification journey. |
| `ST-FE-048` | `T-FE-048` | Build `AUTH-007` Forgot Password. |
| `ST-FE-049` | `T-FE-049` | Build `AUTH-008` Reset Password. |
| `ST-FE-050` | `T-FE-050` | Build `AUTH-009` Reset Success. |
| `ST-FE-051` | `T-FE-051` | Build `AUTH-010` Session Expired. |
| `ST-FE-052` | `T-FE-052` | Prepare Nurse screen approval packet. |
| `ST-FE-053` | `T-FE-053` | Build `AUTH-011` Access Denied. |
| `ST-FE-054` | `T-FE-054` | Clarify account inactive coded state. |
| `ST-FE-055` | `T-FE-055` | Build/classify `AUTH-012` Account Inactive. |
| `ST-FE-056` | `T-FE-056` | Build read-only `NUR-001/002` profile overview. |
| `ST-FE-057` | `T-FE-057` | Build `NUR-003` profile personal information upsert. |
| `ST-FE-058A` | `T-FE-058` | Build `NUR-004` experience list. |
| `ST-FE-058B` | `T-FE-058` | Build `NUR-005` add/edit experience form. |
| `ST-FE-059A` | `T-FE-059` | Build `NUR-006` education list. |
| `ST-FE-059B` | `T-FE-059` | Build `NUR-007` add/edit education form. |
| `ST-FE-060A` | `T-FE-060` | Build `NUR-008` certificate list. |
| `ST-FE-060B` | `T-FE-060` | Build `NUR-009` add/edit certificate form. |
| `ST-FE-061` | `T-FE-061` | Prepare Exams/Learning screen approval packet. |
| `ST-FE-062A` | `T-FE-062` | Build `NUR-010` skills management. |
| `ST-FE-062B` | `T-FE-062` | Build `NUR-011` languages management. |
| `ST-FE-063` | `T-FE-063` | Clarify CV file constraints locally. |
| `ST-FE-064` | `T-FE-064` | Build `NUR-012` CV management. |
| `ST-FE-065` | `T-FE-065` | Clarify/build `NUR-013` completion if contract-backed. |
| `ST-FE-066` | `T-FE-066` | Clarify exam catalog/detail response schemas. |
| `ST-FE-067A` | `T-FE-067` | Build `EXM-001` Exam Catalog. |
| `ST-FE-067B` | `T-FE-067` | Build `EXM-002` Exam Details. |
| `ST-FE-068A` | `T-FE-068` | Build `EXM-003` Purchase Required. |
| `ST-FE-068B` | `T-FE-068` | Build `EXM-004` Exam Instructions/start. |
| `ST-FE-069A` | `T-FE-069` | Build `EXM-005` Exam Session. |
| `ST-FE-069B` | `T-FE-069` | Build `EXM-006` Submit Confirmation. |
| `ST-FE-070` | `T-FE-070` | Prepare Preparation Package screen approval packet. |
| `ST-FE-071` | `T-FE-071` | Build `EXM-007` Exam Result. |
| `ST-FE-072` | `T-FE-072` | Build `EXM-009` Answer Review. |
| `ST-FE-073` | `T-FE-073` | Build `EXM-010` Exam History. |
| `ST-FE-074` | `T-FE-074` | Build `EXM-008` Performance Analytics. |
| `ST-FE-075` | `T-FE-075` | Build public PP offers list/detail. |
| `ST-FE-076` | `T-FE-076` | Build PP entitlements list/detail. |
| `ST-FE-077` | `T-FE-077` | Prepare Commerce screen approval packet. |
| `ST-FE-078` | `T-FE-078` | Build PP practice progress/answer flow. |
| `ST-FE-079` | `T-FE-079` | Build PP package exam/report flow. |
| `ST-FE-080` | `T-FE-080` | Classify PP material reader backend gap. |
| `ST-FE-081` | `T-FE-081` | Clarify payment product schemas. |
| `ST-FE-082A` | `T-FE-082` | Build `COM-001` Product Catalog. |
| `ST-FE-082B` | `T-FE-082` | Build `COM-002` Product Details. |
| `ST-FE-083` | `T-FE-083` | Clarify payment order schemas/lifecycle. |
| `ST-FE-084` | `T-FE-084` | Build `COM-003` Checkout/order creation. |
| `ST-FE-085` | `T-FE-085` | Prepare Employer screen approval packet. |
| `ST-FE-086` | `T-FE-086` | Build `COM-004` Payment Processing. |
| `ST-FE-087A` | `T-FE-087` | Build `COM-005` generic success state. |
| `ST-FE-087B` | `T-FE-087` | Build `COM-006` generic failure state. |
| `ST-FE-088A` | `T-FE-088` | Build `COM-007` Orders. |
| `ST-FE-088B` | `T-FE-088` | Build `COM-008` Order Details. |
| `ST-FE-089` | `T-FE-089` | Classify production payment limitation. |
| `ST-FE-090` | `T-FE-090` | Build `EMP-001` Employer Home/profile. |
| `ST-FE-091A` | `T-FE-091` | Build `EMP-002/003` Search and filters. |
| `ST-FE-091B` | `T-FE-091` | Build `EMP-004` Search Results. |
| `ST-FE-092` | `T-FE-092` | Prepare Account screen approval packet. |
| `ST-FE-093` | `T-FE-093` | Clarify candidate detail contract. |
| `ST-FE-094A` | `T-FE-094` | Build/classify `EMP-005` Candidate Profile. |
| `ST-FE-094B` | `T-FE-094` | Build `EMP-006` Recruitment Request. |
| `ST-FE-095A` | `T-FE-095` | Build `EMP-007` Requests List. |
| `ST-FE-095B` | `T-FE-095` | Build `EMP-008` Request Details. |
| `ST-FE-096` | `T-FE-096` | Build nurse received contact requests. |
| `ST-FE-097A` | `T-FE-097` | Build `ACC-001` Overview. |
| `ST-FE-097B` | `T-FE-097` | Build `ACC-002` Personal Details. |
| `ST-FE-098` | `T-FE-098` | Prepare Administration screen approval packet. |
| `ST-FE-099` | `T-FE-099` | Classify `ACC-003` Change Password. |
| `ST-FE-100` | `T-FE-100` | Classify `ACC-004` Security/Sessions. |
| `ST-FE-101` | `T-FE-101` | Classify `ACC-005` Notification Preferences. |
| `ST-FE-102` | `T-FE-102` | Classify `ACC-006` Account Status. |
| `ST-FE-103` | `T-FE-103` | Classify/build `ADM-001` dashboard/static landing. |
| `ST-FE-104A` | `T-FE-104` | Build `ADM-002` Users. |
| `ST-FE-104B` | `T-FE-104` | Build `ADM-003` User Details. |
| `ST-FE-105` | `T-FE-105` | Classify `ADM-004` Roles/Permissions. |
| `ST-FE-106` | `T-FE-106` | Build `ADM-005` exam categories/reference data. |
| `ST-FE-107` | `T-FE-107` | Build `ADM-006` admin exam lifecycle. |
| `ST-FE-108` | `T-FE-108` | Build admin exam version lifecycle. |
| `ST-FE-109` | `T-FE-109` | Build `ADM-007` question lifecycle. |
| `ST-FE-110` | `T-FE-110` | Build answer option lifecycle. |
| `ST-FE-111` | `T-FE-111` | Build `ADM-008` admin payment products. |
| `ST-FE-112` | `T-FE-112` | Classify `ADM-009/010` admin orders/recruitment. |
| `ST-FE-113` | `T-FE-113` | Prepare Shared System screen approval packet. |
| `ST-FE-114` | `T-FE-114` | Build admin PP reporting topics. |
| `ST-FE-115` | `T-FE-115` | Build admin PP reporting profiles. |
| `ST-FE-116` | `T-FE-116` | Classify/build `SYS-004` Offline. |
| `ST-FE-117` | `T-FE-117` | Classify/build `SYS-005` Maintenance. |
| `ST-FE-118` | `T-FE-118` | Build admin PP materials. |
| `ST-FE-119` | `T-FE-119` | Build admin PP practice collections. |
| `ST-FE-120` | `T-FE-120` | Build admin PP packages/package versions. |
| `ST-FE-121` | `T-FE-121` | Build admin PP offers. |
| `ST-FE-122` | `T-FE-122` | Run post-implementation visual verification for implemented screen. |
| `ST-FE-123` | `T-FE-123` | Approve/configure Playwright version/browser policy. |
| `ST-FE-124` | `T-FE-124` | Implement supported critical E2E journeys. |
| `ST-FE-125` | `T-FE-125` | Define CI pipeline sequence. |
| `ST-FE-126` | `T-FE-126` | Verify CSP/Trusted Types. |
| `ST-FE-127` | `T-FE-127` | Verify XSRF/CSRF posture. |
| `ST-FE-128` | `T-FE-128` | Approve production auth/session posture. |
| `ST-FE-129` | `T-FE-129` | Run dependency audit. |
| `ST-FE-130` | `T-FE-130` | Harden source-map/config policy. |
| `ST-FE-131` | `T-FE-131` | Verify bundle/performance budgets. |
| `ST-FE-132` | `T-FE-132` | Verify browser matrix. |
| `ST-FE-133` | `T-FE-133` | Verify observability redaction. |
| `ST-FE-134` | `T-FE-134` | Run final accessibility audit. |
| `ST-FE-135` | `T-FE-135` | Run final RTL audit. |
| `ST-FE-136` | `T-FE-136` | Run final responsive audit. |
| `ST-FE-137` | `T-FE-137` | Record production payment release decision. |

## 13. Verification Gate Registry

All Gates initially have `status_result: NOT STARTED`. Every Gate must use the canonical Gate template: `gate_id`, `parent_task`, `predecessor_gate_ids`, `task_specific_evidence`, `task_specific_blockers`, `applicable_cross_cutting_checks`, `status_result`, `blocker_types`.

| gate_id | parent_task | predecessor_gate_ids | task_specific_evidence | blocker_types |
|---|---|---|---|---|
| `GATE-FE-T001` | `T-FE-001` | explicit scaffold approval | CLI output, tree, no nested Git, no generated AI-config/rules files, no unauthorized MCP/Agent-Skill configuration, 2025 filename style evidence, generated files not renamed to historical Angular naming, existing Nursing Platform governance remains sole repository AI authority, status | `SCOPE`,`DEPENDENCY` |
| `GATE-FE-T002` | `T-FE-002` | `GATE-FE-T001` | Node/npm output | `DEPENDENCY` |
| `GATE-FE-T003` | `T-FE-003` | `GATE-FE-T002` | `npm ci`, test, prod build | `DEPENDENCY` |
| `GATE-FE-T004` | `T-FE-004` | `GATE-FE-T003` | dependency guard output | `DEPENDENCY` |
| `GATE-FE-T005` | `T-FE-005` | `GATE-FE-T003` | lint approval/config/output | `TOOLING_APPROVAL` |
| `GATE-FE-T006` | `T-FE-006` | `GATE-FE-T003` | stylelint + direction rule output | `TOOLING_APPROVAL` |
| `GATE-FE-T007` | `T-FE-007` | `GATE-FE-T005`, `GATE-FE-T006` | local quality command output | `DEPENDENCY` |
| `GATE-FE-T008` | `T-FE-008` | `GATE-FE-T003` | token/style evidence | `DESIGN` |
| `GATE-FE-T009` | `T-FE-009` | `GATE-FE-T008` | Material theme build/CSS evidence | `DESIGN` |
| `GATE-FE-T010` | `T-FE-010` | `GATE-FE-T008` | a11y utility tests | `DESIGN` |
| `GATE-FE-T011` | `T-FE-011` | `GATE-FE-T008` | dir/font/bidi tests | `DESIGN` |
| `GATE-FE-T012` | `T-FE-012` | `GATE-FE-T008`, `GATE-FE-T011` | responsive/logical CSS evidence | `DESIGN` |
| `GATE-FE-T013` | `T-FE-013` | `GATE-FE-T008..T012` | form dimension/a11y tests | `DESIGN` |
| `GATE-FE-T014` | `T-FE-014` | `GATE-FE-T003` | a11y tool approval/test output | `TOOLING_APPROVAL` |
| `GATE-FE-T015` | `T-FE-015` | `GATE-FE-T003` | generator spike report | `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T016` | `T-FE-016` | `GATE-FE-T015` | generator approval record | `TOOLING_APPROVAL` |
| `GATE-FE-T017` | `T-FE-017` | `GATE-FE-T016` | generation/build/drift evidence | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T018` | `T-FE-018` | `GATE-FE-T017` | config tests | `RUNTIME_DEPLOYMENT` |
| `GATE-FE-T019` | `T-FE-019` | `GATE-FE-T017` | Problem Details unit tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T020` | `T-FE-020` | `GATE-FE-T017`, `GATE-FE-T019` | adapter boundary tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T021` | `T-FE-021` | `GATE-FE-T018..T020` | auth API tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T022` | `T-FE-022` | `GATE-FE-T021` | token storage tests | `SECURITY` |
| `GATE-FE-T023` | `T-FE-023` | `GATE-FE-T021`, `GATE-FE-T022` | bootstrap state tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T024` | `T-FE-024` | `GATE-FE-T022`, `GATE-FE-T023` | refresh concurrency tests | `SECURITY` |
| `GATE-FE-T025` | `T-FE-025` | `GATE-FE-T024` | interceptor tests | `SECURITY` |
| `GATE-FE-T026` | `T-FE-026` | `GATE-FE-T023..T025` | logout tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T027` | `T-FE-027` | `GATE-FE-T009..T012`, `GATE-FE-T023` | shell/a11y tests | `DESIGN` |
| `GATE-FE-T028` | `T-FE-028` | `GATE-FE-T027`, `GATE-FE-T113` | loading state tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T029` | `T-FE-029` | `GATE-FE-T027` | route registry tests | `SCOPE` |
| `GATE-FE-T030` | `T-FE-030` | `GATE-FE-T023..T025`, `GATE-FE-T029` | guard/router tests | `SECURITY` |
| `GATE-FE-T031` | `T-FE-031` | `GATE-FE-T023`, `GATE-FE-T029` | UX permission tests | `SECURITY`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T032` | `T-FE-032` | `GATE-FE-T031` | nav visibility tests | `DESIGN` |
| `GATE-FE-T033` | `T-FE-033` | `GATE-FE-T019`, `GATE-FE-T010` | loading/error tests | `DESIGN` |
| `GATE-FE-T034` | `T-FE-034` | `GATE-FE-T013`, `GATE-FE-T019` | validation/form tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T035` | `T-FE-035` | `GATE-FE-T031`, `GATE-FE-T113` | empty/restricted tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T036` | `T-FE-036` | `GATE-FE-T009`, `GATE-FE-T019` | dialog/live-region tests | `DESIGN` |
| `GATE-FE-T037` | `T-FE-037` | `GATE-FE-T034` | multipart helper tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T038` | `T-FE-038` | `GATE-FE-T033`, `GATE-FE-T035` | pagination tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T039` | `T-FE-039` | `GATE-FE-T012`, `GATE-FE-T014` | visual method evidence | `TOOLING_APPROVAL`,`DESIGN` |
| `GATE-FE-T040` | `T-FE-040` | design/backend/state review | Auth approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T041` | `T-FE-041` | `GATE-FE-T021..T025`, `GATE-FE-T030`, `GATE-FE-T034`, `GATE-FE-T040` | Sign In tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T042` | `T-FE-042` | local source/OpenAPI inspection | Register clarification evidence | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T043` | `T-FE-043` | `GATE-FE-T040`, `GATE-FE-T042` | Role Selection tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T044` | `T-FE-044` | `GATE-FE-T040`, `GATE-FE-T042`, `GATE-FE-T034` | Nurse registration tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T045` | `T-FE-045` | `GATE-FE-T040`, `GATE-FE-T042`, `GATE-FE-T034` | Employer registration tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T046` | `T-FE-046` | local source/OpenAPI inspection | verification auth clarification | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T047` | `T-FE-047` | `GATE-FE-T040`, `GATE-FE-T046` | email verification tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T048` | `T-FE-048` | `GATE-FE-T040`, `GATE-FE-T034` | forgot password tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T049` | `T-FE-049` | `GATE-FE-T048`, `GATE-FE-T034` | reset password tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T050` | `T-FE-050` | `GATE-FE-T049` | reset success visual/state evidence | `DESIGN` |
| `GATE-FE-T051` | `T-FE-051` | `GATE-FE-T026`, `GATE-FE-T040` | session expired tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T052` | `T-FE-052` | design/backend/state review | Nurse approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T053` | `T-FE-053` | `GATE-FE-T031`, `GATE-FE-T040` | access denied tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T054` | `T-FE-054` | local source/OpenAPI inspection | inactive-state clarification | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T055` | `T-FE-055` | `GATE-FE-T040`, `GATE-FE-T054` | inactive implementation/classification evidence | `DESIGN`,`BACKEND` |
| `GATE-FE-T056` | `T-FE-056` | `GATE-FE-T021..T025`, `GATE-FE-T030`, `GATE-FE-T033`, `GATE-FE-T052` | nurse overview tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T057` | `T-FE-057` | `GATE-FE-T056`, `GATE-FE-T034` | personal info tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T058` | `T-FE-058` | `GATE-FE-T056`, `GATE-FE-T034`, `GATE-FE-T036` | experience CRUD tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T059` | `T-FE-059` | `GATE-FE-T056`, `GATE-FE-T034`, `GATE-FE-T036` | education CRUD tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T060` | `T-FE-060` | `GATE-FE-T056`, `GATE-FE-T034`, `GATE-FE-T036` | certificate CRUD tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T061` | `T-FE-061` | design/backend/state review | Exams approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T062` | `T-FE-062` | `GATE-FE-T056`, `GATE-FE-T034` | skills/languages tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T063` | `T-FE-063` | local source/OpenAPI inspection | CV constraints evidence | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T064` | `T-FE-064` | `GATE-FE-T056`, `GATE-FE-T037`, `GATE-FE-T052`, `GATE-FE-T063` | CV upload tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T065` | `T-FE-065` | `GATE-FE-T056`, `GATE-FE-T052` | completion formula evidence/tests | `BACKEND`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T066` | `T-FE-066` | local inspection | exam schema clarification | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T067` | `T-FE-067` | `GATE-FE-T030`, `GATE-FE-T038`, `GATE-FE-T061`, `GATE-FE-T066` | catalog/detail tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T068` | `T-FE-068` | `GATE-FE-T067` | access/start tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T069` | `T-FE-069` | `GATE-FE-T068`, `GATE-FE-T036` | session/save/submit tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T070` | `T-FE-070` | design/backend/state review | PP approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T071` | `T-FE-071` | `GATE-FE-T069` | result tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T072` | `T-FE-072` | `GATE-FE-T069` | answer-review disclosure tests + per-screen visual evidence | `SECURITY`,`DESIGN` |
| `GATE-FE-T073` | `T-FE-073` | `GATE-FE-T069`, `GATE-FE-T038` | history tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T074` | `T-FE-074` | `GATE-FE-T069` | analytics tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T075` | `T-FE-075` | `GATE-FE-T018..T020`, `GATE-FE-T038`, `GATE-FE-T070` | PP offer tests + visual evidence | `DESIGN` |
| `GATE-FE-T076` | `T-FE-076` | `GATE-FE-T021..T025`, `GATE-FE-T038`, `GATE-FE-T070` | entitlement tests + visual evidence | `DESIGN` |
| `GATE-FE-T077` | `T-FE-077` | design/backend/state review | Commerce approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T078` | `T-FE-078` | `GATE-FE-T076`, `GATE-FE-T034` | practice tests + visual evidence | `DESIGN` |
| `GATE-FE-T079` | `T-FE-079` | `GATE-FE-T076` | package exam/report tests + visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T080` | `T-FE-080` | local inspection | material reader classification | `BACKEND` |
| `GATE-FE-T081` | `T-FE-081` | local inspection | product schema evidence | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T082` | `T-FE-082` | `GATE-FE-T030`, `GATE-FE-T077`, `GATE-FE-T081` | product tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T083` | `T-FE-083` | local inspection | order schema evidence | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T084` | `T-FE-084` | `GATE-FE-T082`, `GATE-FE-T083`, `GATE-FE-T034`, `GATE-FE-T077` | checkout/order tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T085` | `T-FE-085` | design/backend/state review | Employer approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T086` | `T-FE-086` | `GATE-FE-T084` | processing/retry tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T087` | `T-FE-087` | `GATE-FE-T084`, `GATE-FE-T086`, `GATE-FE-T077` | generic outcome tests + per-screen visual evidence | `DESIGN`,`BACKEND` |
| `GATE-FE-T088` | `T-FE-088` | `GATE-FE-T084`, `GATE-FE-T038`, `GATE-FE-T077` | order list/detail tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T089` | `T-FE-089` | `GATE-FE-T086` | production limitation evidence | `BACKEND`,`EXTERNAL` |
| `GATE-FE-T090` | `T-FE-090` | `GATE-FE-T030`, `GATE-FE-T034`, `GATE-FE-T085` | employer profile tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T091` | `T-FE-091` | `GATE-FE-T090`, `GATE-FE-T038`, `GATE-FE-T085` | candidate search tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T092` | `T-FE-092` | design/backend/state review | Account approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T093` | `T-FE-093` | local inspection | candidate detail evidence | `CONTRACT_CLARIFICATION`,`BACKEND` |
| `GATE-FE-T094` | `T-FE-094` | `GATE-FE-T091`, `GATE-FE-T093`, `GATE-FE-T034`, `GATE-FE-T085` | profile/request tests + per-screen visual evidence | `DESIGN`,`BACKEND` |
| `GATE-FE-T095` | `T-FE-095` | `GATE-FE-T094`, `GATE-FE-T038`, `GATE-FE-T085` | employer requests tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T096` | `T-FE-096` | `GATE-FE-T056`, `GATE-FE-T038` | nurse received request tests | `DESIGN` |
| `GATE-FE-T097` | `T-FE-097` | `GATE-FE-T023`, `GATE-FE-T033`, `GATE-FE-T092` | account overview tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T098` | `T-FE-098` | design/backend/state review | Admin approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T099` | `T-FE-099` | local inspection | change-password classification | `BACKEND` |
| `GATE-FE-T100` | `T-FE-100` | local inspection | sessions classification | `BACKEND` |
| `GATE-FE-T101` | `T-FE-101` | local inspection | notification classification | `BACKEND` |
| `GATE-FE-T102` | `T-FE-102` | local inspection | account-status classification | `BACKEND`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T103` | `T-FE-103` | local inspection, `GATE-FE-T098` if UI | dashboard classification/visual | `BACKEND`,`DESIGN` |
| `GATE-FE-T104` | `T-FE-104` | `GATE-FE-T030`, `GATE-FE-T038`, `GATE-FE-T098` | user tests + raw JSON sensitive checks + visual | `SECURITY`,`DESIGN` |
| `GATE-FE-T105` | `T-FE-105` | local inspection | role CRUD classification | `BACKEND` |
| `GATE-FE-T106` | `T-FE-106` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | category tests + visual | `DESIGN` |
| `GATE-FE-T107` | `T-FE-107` | `GATE-FE-T106`, `GATE-FE-T098` | admin exam lifecycle tests + visual | `DESIGN` |
| `GATE-FE-T108` | `T-FE-108` | `GATE-FE-T107` | version lifecycle tests + visual | `DESIGN` |
| `GATE-FE-T109` | `T-FE-109` | `GATE-FE-T108` | question lifecycle tests + visual | `DESIGN` |
| `GATE-FE-T110` | `T-FE-110` | `GATE-FE-T109` | answer option tests + visual | `DESIGN` |
| `GATE-FE-T111` | `T-FE-111` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | payment product admin tests + visual | `DESIGN` |
| `GATE-FE-T112` | `T-FE-112` | local inspection | admin orders/recruitment classification | `BACKEND` |
| `GATE-FE-T113` | `T-FE-113` | design/backend/runtime review | System screen approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T114` | `T-FE-114` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | PP topic admin tests | `DESIGN` |
| `GATE-FE-T115` | `T-FE-115` | `GATE-FE-T114` | PP reporting profile tests | `DESIGN` |
| `GATE-FE-T116` | `T-FE-116` | `GATE-FE-T113` | offline runtime classification/visual if supported | `RUNTIME_DEPLOYMENT` |
| `GATE-FE-T117` | `T-FE-117` | `GATE-FE-T113` | maintenance runtime classification/visual if supported | `RUNTIME_DEPLOYMENT` |
| `GATE-FE-T118` | `T-FE-118` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | PP material admin tests | `DESIGN` |
| `GATE-FE-T119` | `T-FE-119` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | PP practice collection tests | `DESIGN` |
| `GATE-FE-T120` | `T-FE-120` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | PP package/version tests | `DESIGN` |
| `GATE-FE-T121` | `T-FE-121` | `GATE-FE-T034`, `GATE-FE-T038`, `GATE-FE-T098` | PP offer admin tests | `DESIGN` |
| `GATE-FE-T122` | `T-FE-122` | implemented screen gate + approved Penpot | reusable template evidence only; not global substitute | `DESIGN` |
| `GATE-FE-T123` | `T-FE-123` | tooling approval | Playwright config/browser policy | `TOOLING_APPROVAL` |
| `GATE-FE-T124` | `T-FE-124` | `GATE-FE-T123` + feature gates | E2E output | `EXTERNAL`,`DEPENDENCY` |
| `GATE-FE-T125` | `T-FE-125` | tooling/build/test gates | CI pipeline evidence | `DEPENDENCY` |
| `GATE-FE-T126` | `T-FE-126` | feature/build gates | CSP/Trusted Types evidence | `SECURITY` |
| `GATE-FE-T127` | `T-FE-127` | auth/payment gates | CSRF posture evidence | `SECURITY` |
| `GATE-FE-T128` | `T-FE-128` | auth gates | auth/session security approval | `SECURITY` |
| `GATE-FE-T129` | `T-FE-129` | dependency guard | audit output | `DEPENDENCY`,`SECURITY` |
| `GATE-FE-T130` | `T-FE-130` | build/config gates | source-map/config review | `SECURITY` |
| `GATE-FE-T131` | `T-FE-131` | production build | budget/perf evidence | `DEPENDENCY` |
| `GATE-FE-T132` | `T-FE-132` | build/test gates | browser matrix report | `EXTERNAL` |
| `GATE-FE-T133` | `T-FE-133` | error/auth/payment/exam gates | redaction tests | `SECURITY` |
| `GATE-FE-T134` | `T-FE-134` | implemented screens | a11y audit | `DESIGN` |
| `GATE-FE-T135` | `T-FE-135` | direction + screens | RTL audit | `DESIGN` |
| `GATE-FE-T136` | `T-FE-136` | responsive + screens | responsive audit | `DESIGN` |
| `GATE-FE-T137` | `T-FE-137` | `GATE-FE-T089` | payment release decision | `BACKEND`,`EXTERNAL` |

## 14. Screen Ownership Matrix

Every screen has exactly one primary owner Task. `approval_decision` is initially `BLOCKED` unless future screen approval changes it to `APPROVED` or `DEFERRED`.

| screen_id | primary_owner_task | family_approval_gate | approval_decision | execution_status | blocker_types | contract_dependency | post_implementation_visual_verification |
|---|---|---|---|---|---|---|---|
| `AUTH-001` | `T-FE-041` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-AUTH-LOGIN`,`C-ME` | required in owning gate evidence |
| `AUTH-002` | `T-FE-043` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-AUTH-REGISTER` | required in owning gate evidence |
| `AUTH-003` | `T-FE-044` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-AUTH-REGISTER` | required in owning gate evidence |
| `AUTH-004` | `T-FE-045` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-AUTH-REGISTER` | required in owning gate evidence |
| `AUTH-005` | `T-FE-047` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-AUTH-SEND-VERIFY`,`C-AUTH-VERIFY` | required in owning gate evidence |
| `AUTH-006` | `T-FE-047` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-AUTH-SEND-VERIFY`,`C-AUTH-VERIFY` | required in owning gate evidence |
| `AUTH-007` | `T-FE-048` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-AUTH-FORGOT` | required in owning gate evidence |
| `AUTH-008` | `T-FE-049` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-AUTH-RESET` | required in owning gate evidence |
| `AUTH-009` | `T-FE-050` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-AUTH-RESET` | required in owning gate evidence |
| `AUTH-010` | `T-FE-051` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ME` | required in owning gate evidence |
| `AUTH-011` | `T-FE-053` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ERROR` | required in owning gate evidence |
| `AUTH-012` | `T-FE-055` | `GATE-FE-T040` | `BLOCKED` | `NOT STARTED` | `CONTRACT_CLARIFICATION`,`BACKEND`,`DESIGN` | `C-AUTH-LOGIN`,`C-ME` | required if implemented |
| `NUR-001` | `T-FE-056` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-PROFILE` | required in owning gate evidence |
| `NUR-002` | `T-FE-056` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-PROFILE` | required in owning gate evidence |
| `NUR-003` | `T-FE-057` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-PROFILE` | required in owning gate evidence |
| `NUR-004` | `T-FE-058` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-EXP` | required in owning gate evidence |
| `NUR-005` | `T-FE-058` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-EXP` | required in owning gate evidence |
| `NUR-006` | `T-FE-059` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-EDU` | required in owning gate evidence |
| `NUR-007` | `T-FE-059` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-EDU` | required in owning gate evidence |
| `NUR-008` | `T-FE-060` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-CERT` | required in owning gate evidence |
| `NUR-009` | `T-FE-060` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-CERT` | required in owning gate evidence |
| `NUR-010` | `T-FE-062` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-SKILLS-LANG` | required in owning gate evidence |
| `NUR-011` | `T-FE-062` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-NUR-SKILLS-LANG` | required in owning gate evidence |
| `NUR-012` | `T-FE-064` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-NUR-CV` | required in owning gate evidence |
| `NUR-013` | `T-FE-065` | `GATE-FE-T052` | `BLOCKED` | `NOT STARTED` | `CONTRACT_CLARIFICATION`,`BACKEND`,`DESIGN` | `C-NUR-PROFILE` | required if implemented |
| `EMP-001` | `T-FE-090` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-PROFILE` | required in owning gate evidence |
| `EMP-002` | `T-FE-091` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-CANDIDATES` | required in owning gate evidence |
| `EMP-003` | `T-FE-091` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-CANDIDATES` | required in owning gate evidence |
| `EMP-004` | `T-FE-091` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-CANDIDATES` | required in owning gate evidence |
| `EMP-005` | `T-FE-094` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `CONTRACT_CLARIFICATION`,`BACKEND` | `C-EMP-CANDIDATES` | required if implemented |
| `EMP-006` | `T-FE-094` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-REQUESTS` | required in owning gate evidence |
| `EMP-007` | `T-FE-095` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-REQUESTS` | required in owning gate evidence |
| `EMP-008` | `T-FE-095` | `GATE-FE-T085` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EMP-REQUESTS` | required in owning gate evidence |
| `EXM-001` | `T-FE-067` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-EXAM-CATALOG` | required in owning gate evidence |
| `EXM-002` | `T-FE-067` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-EXAM-CATALOG` | required in owning gate evidence |
| `EXM-003` | `T-FE-068` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EXAM-START`,`C-PAY-PRODUCTS` | required in owning gate evidence |
| `EXM-004` | `T-FE-068` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EXAM-START` | required in owning gate evidence |
| `EXM-005` | `T-FE-069` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-EXAM-SESSION` | required in owning gate evidence |
| `EXM-006` | `T-FE-069` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EXAM-SESSION` | required in owning gate evidence |
| `EXM-007` | `T-FE-071` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EXAM-RESULT-REVIEW` | required in owning gate evidence |
| `EXM-008` | `T-FE-074` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EXAM-ANALYTICS` | required in owning gate evidence |
| `EXM-009` | `T-FE-072` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`SECURITY` | `C-EXAM-RESULT-REVIEW` | required in owning gate evidence |
| `EXM-010` | `T-FE-073` | `GATE-FE-T061` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-EXAM-ANALYTICS` | required in owning gate evidence |
| `COM-001` | `T-FE-082` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-PAY-PRODUCTS` | required in owning gate evidence |
| `COM-002` | `T-FE-082` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-PAY-PRODUCTS` | required in owning gate evidence |
| `COM-003` | `T-FE-084` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-PAY-ORDERS` | required in owning gate evidence |
| `COM-004` | `T-FE-086` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-PAY-CHECKOUT` | required in owning gate evidence |
| `COM-005` | `T-FE-087` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`BACKEND` | `C-PAY-ORDERS`,`C-PAY-CHECKOUT` | required in owning gate evidence |
| `COM-006` | `T-FE-087` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`BACKEND` | `C-PAY-ORDERS`,`C-PAY-CHECKOUT` | required in owning gate evidence |
| `COM-007` | `T-FE-088` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-PAY-ORDERS` | required in owning gate evidence |
| `COM-008` | `T-FE-088` | `GATE-FE-T077` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-PAY-ORDERS` | required in owning gate evidence |
| `ACC-001` | `T-FE-097` | `GATE-FE-T092` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ME` | required in owning gate evidence |
| `ACC-002` | `T-FE-097` | `GATE-FE-T092` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ME` | required in owning gate evidence |
| `ACC-003` | `T-FE-099` | `GATE-FE-T092` | `BLOCKED` | `NOT STARTED` | `BACKEND` | none found | required if implemented |
| `ACC-004` | `T-FE-100` | `GATE-FE-T092` | `BLOCKED` | `NOT STARTED` | `BACKEND` | none found | required if implemented |
| `ACC-005` | `T-FE-101` | `GATE-FE-T092` | `BLOCKED` | `NOT STARTED` | `BACKEND` | none found | required if implemented |
| `ACC-006` | `T-FE-102` | `GATE-FE-T092` | `BLOCKED` | `NOT STARTED` | `CONTRACT_CLARIFICATION`,`BACKEND` | `C-AUTH-LOGIN`,`C-ME` | required if implemented |
| `ADM-001` | `T-FE-103` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `BACKEND`,`DESIGN` | none found | required if implemented |
| `ADM-002` | `T-FE-104` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-ADM-USERS` | required in owning gate evidence |
| `ADM-003` | `T-FE-104` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-ADM-USERS` | required in owning gate evidence |
| `ADM-004` | `T-FE-105` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `BACKEND` | none found | required if implemented |
| `ADM-005` | `T-FE-106` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ADM-EXAM-CATEGORIES` | required in owning gate evidence |
| `ADM-006` | `T-FE-107` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-ADM-EXAMS` | required in owning gate evidence |
| `ADM-007` | `T-FE-109` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-ADM-EXAMS` | required in owning gate evidence |
| `ADM-008` | `T-FE-111` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `DESIGN`,`CONTRACT_CLARIFICATION` | `C-ADM-PAY-PRODUCTS` | required in owning gate evidence |
| `ADM-009` | `T-FE-112` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `BACKEND` | none found | required if implemented |
| `ADM-010` | `T-FE-112` | `GATE-FE-T098` | `BLOCKED` | `NOT STARTED` | `BACKEND` | none found | required if implemented |
| `SYS-001` | `T-FE-028` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `DESIGN` | none | required in owning gate evidence |
| `SYS-002` | `T-FE-031` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ERROR` | required in owning gate evidence |
| `SYS-003` | `T-FE-031` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ERROR` | required in owning gate evidence |
| `SYS-004` | `T-FE-116` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `RUNTIME_DEPLOYMENT` | runtime/deployment contract | required if implemented |
| `SYS-005` | `T-FE-117` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `RUNTIME_DEPLOYMENT` | runtime/deployment contract | required if implemented |
| `SYS-006` | `T-FE-035` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `DESIGN` | paginated/empty states | required in owning gate evidence |
| `SYS-007` | `T-FE-035` | `GATE-FE-T113` | `BLOCKED` | `NOT STARTED` | `DESIGN` | `C-ERROR`/403 | required in owning gate evidence |

## 15. Approved Vertical Slices

### Vertical Slice A

`AUTH-001` Sign In → session/bootstrap → protected shell → `GET /api/v1/me` → read-only Nurse Profile Overview.

This is a sequencing recommendation, not permission to implement. It remains subject to Task/Gate and screen-approval prerequisites.

### Vertical Slice B

`NUR-012` CV management → multipart/form-data `file`.

This is a sequencing recommendation, not permission to implement. It remains subject to Task/Gate and screen-approval prerequisites.

## 16. Genuine Human Decisions Remaining

- API generator selection after spike.
- Production auth/session posture.
- Production payment scope.
- Browser support matrix.
- Visual-regression tolerance/approval process.
- Whether confirmed backend-gap screens create backend backlog work or remain outside frontend scope.

## 17. Scope Protection Rules

- Do not begin `T-FE-001` without explicit authorization.
- Do not scaffold Angular, create `frontend/`, install dependencies, generate the API client, modify backend source/tests, modify canonical OpenAPI, modify Penpot, stage, commit, or push unless a later explicit approval says otherwise.
- Do not mark implementation Tasks `VERIFIED` because they are planned.
- Do not modify completed `VERIFIED` scope unless an approved Task explicitly marks it `REOPENED`.
- Do not use this ledger to expand task scope during implementation.
- Do not treat a `NOT STARTED` entry as approval to code.
- Permanent atomic checkpoint policy: reconcile and commit `PROGRESS.md` plus this ledger first as docs-only `docs(frontend): reconcile implementation ledger state`, then resume `T-FE-017` separately with `services: false` clean regeneration; never mix docs reconciliation with package/generated files in one commit; no push and no history rewrite.
- Next-step handoff: after the docs-only checkpoint commit, resume `T-FE-017` with `ng-openapi-gen@1.0.5` and `services: false` using the standard function-based generated client from clean generated output; paused partials (`frontend/package.json`, `frontend/package-lock.json`, `frontend/dependency-policy.json`, `frontend/eslint.config.js`, `frontend/src/app/core/api/generated/**`) stay uncommitted until that resume. `T-FE-015`/`T-FE-016` remain `VERIFIED`; no `T-FE-017` verification is claimed.

## 18. Execution Status Records

### `T-FE-001` — Scaffold Angular workspace

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_date: 2026-09-04
- prior_blocked_attempt: The exact approved Angular CLI command using `@angular/cli@22.1.4` accepted the approved options and generated the initial `frontend/` workspace files, but the scaffold workflow failed during its normal npm package installation step with `npm error Cannot read properties of null (reading 'edgesOut')`. The npm debug log path reported by npm was `/home/karam/.npm/_logs/2026-09-04T03_36_38_783Z-debug-0.log`.
- prior_scope_summary: Generated files were located under `frontend/`; `frontend/.git` was not created; `frontend/package-lock.json` was not created because package installation failed; no backend, canonical OpenAPI, Penpot, API client generation, Material/CDK setup, lint/stylelint setup, AXE setup, Playwright setup, auth implementation, or product implementation was performed.
- reopen_history: Previous status `BLOCKED`; previous blocker_type `DEPENDENCY`; root-cause classification `NPM_10_9_8_ARBORIST_DEFECT`; approved recovery `npm 11.6.0`. The exact generated package graph reproduced the npm `10.9.8` Arborist failure in isolation, npm `11.6.0` resolved the same graph successfully under Node `v22.23.1`, and diagnostic `--legacy-peer-deps` also bypassed the failure but is NOT an approved Nursing Platform install policy. This is an npm resolver defect classification, not an Angular dependency-graph conflict.
- retry_contract: Future clean retry must use Node `v22.23.1`, npm `11.6.0`, package manager `npm`, and the exact scaffold generator executable pinned through `npx -p @angular/cli@22.1.4 ng new ...` with the approved flags. Stock Angular CLI-generated dependency ranges must not be manually rewritten merely to force every installed Angular package to patch version `22.1.4`; `package-lock.json` will be the reproducibility authority for actual resolved package versions after successful scaffold installation.
- clean_retry_evidence: Clean retry completed with Node `v22.23.1`, npm `11.6.0`, package manager `npm`, and scaffold generator `@angular/cli@22.1.4`; packages installed successfully; `frontend/package.json`, `frontend/package-lock.json`, and `frontend/node_modules` exist; `frontend/.git` is absent; `package-lock.json` lockfileVersion is `3`; generated packageManager metadata is `npm@11.6.0`; generated application files use 2025 names (`app.ts`, `app.html`, `app.scss`, `app.spec.ts`, `app.config.ts`, `app.routes.ts`); no Angular-generated AI instruction/config files were found outside installed package internals; no SSR target/server source was generated; no `zone.js` dependency/lock entry or `provideZoneChangeDetection` configuration was generated; Router, SCSS, standalone bootstrap, Vitest, and prefix `np` are present; direct dependencies remain stock Angular CLI scaffold dependencies only.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `4a2fed1`; completion_commit_subject `feat(frontend): scaffold Angular workspace`; verified_toolchain Node `v22.23.1`, npm `11.6.0`; scaffold_generator `@angular/cli@22.1.4`; workspace `./frontend`; Angular major `22`; actual package versions are locked by `frontend/package-lock.json`; `SERVICE EXPORT CONFIRMED` for `@angular/core` `22.1.5`.

### `ST-FE-001` — Execute exact approved Angular scaffold command

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: The exact command was executed without changing CLI version or approved flags. Generation reached file creation but did not complete package installation. No additional install, upgrade, downgrade, cleanup, or guessed alternative command was run.
- clean_retry_evidence: Exact approved scaffold command was rerun from a clean absent `frontend/` path after npm `11.6.0` recovery. The command completed successfully including normal npm installation. No `--legacy-peer-deps`, `--skip-install`, alternate Angular CLI version, or manual dependency edit was used.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `4a2fed1 feat(frontend): scaffold Angular workspace`; subtask accepted as complete.

### `GATE-FE-T001`

- status_result: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Gate evidence is incomplete because package installation failed and the npm lockfile is missing. Observed partial positives: `frontend/` exists, no nested `frontend/.git` exists, generated app filenames use the 2025 style (`app.ts`, `app.html`, `app.scss`, `app.spec.ts`, `app.config.ts`, `app.routes.ts`), no generated Angular AI config files were found in the generated top-level workspace, and no unauthorized MCP/Agent-Skill repository configuration was added.
- clean_retry_evidence: Gate evidence is ready for technical-lead review after clean retry: exact command succeeded, package installation succeeded, npm `10.9.8` Arborist `edgesOut` failure did not recur under npm `11.6.0`, workspace exists only under `frontend/`, no nested Git exists, package-lock exists, lockfileVersion is `3`, packageManager metadata is `npm@11.6.0`, Router/SCSS/strict/standalone/zoneless-compatible no-zone scaffold/Vitest/prefix `np` are present, SSR is not configured, AI config generation is none, and 2025 filenames are present. The Agent has not marked the gate `VERIFIED`.
- closure_evidence: Technical-lead review accepted exact approved `ng new` invocation, workspace at `./frontend`, package installation success under npm `11.6.0`, committed `package-lock.json`, no nested Git, Router enabled, SCSS, strict configuration, standalone architecture, zoneless-compatible no-zone scaffold, SSR disabled, Vitest, prefix `np`, npm package manager, `--ai-config=none` honored, 2025 filename style honored, no unauthorized dependencies, no backend/OpenAPI/Penpot changes, and no `T-FE-002` execution.

### `T-FE-002` — Pin Node/npm/toolchain

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_date: 2026-09-04
- scope_summary: Added only root Node/npm metadata required for the already-scaffolded Angular workspace. Created repository-root `.nvmrc` with exact Node version `22.23.1`. Added exact `engines.node` value `22.23.1` and exact `engines.npm` value `11.6.0` to `frontend/package.json` while preserving `packageManager: npm@11.6.0`.
- exclusion_summary: Did not change package name, version, scripts, dependencies, devDependencies, Angular version ranges, `frontend/package-lock.json`, Angular source/configuration, backend files, canonical OpenAPI files, Penpot assets, or task-scope files for later frontend tasks. Did not run npm install/ci/update/upgrade, Angular CLI generation, build, tests, lint, stylelint, Playwright, or OpenAPI generation.
- review_note: `T-FE-003` remains `NOT STARTED` and requires later explicit authorization.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `7545c18`; completion_commit_subject `chore(frontend): pin Node and npm metadata`; verified_metadata `.nvmrc = 22.23.1`, `frontend/package.json packageManager = npm@11.6.0`, `frontend/package.json engines.node = 22.23.1`, `frontend/package.json engines.npm = 11.6.0`; dependency_integrity dependencies unchanged, devDependencies unchanged, Angular ranges unchanged, `frontend/package-lock.json` unchanged; toolchain_uniqueness no `frontend/.nvmrc`, no `.node-version`, no `frontend/.node-version`, no `.npmrc`, no `frontend/.npmrc`.

### `ST-FE-002` — Pin and verify Node/npm toolchain metadata

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Root `.nvmrc` and `frontend/package.json` engines now pin Node `22.23.1` and npm `11.6.0`. Existing generated `packageManager` metadata remains `npm@11.6.0`.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `7545c18 chore(frontend): pin Node and npm metadata`; subtask accepted as complete. `T-FE-003` remains `NOT STARTED`.

### `GATE-FE-T002`

- status_result: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Pre-task gate observed HEAD `b4af58a`, clean working tree, Node `v22.23.1`, npm `11.6.0`, and NVM tool paths. Final metadata verification observed `.nvmrc` content `22.23.1\n`, `engines.node` `22.23.1`, `engines.npm` `11.6.0`, and `packageManager` `npm@11.6.0`. `git diff -- frontend/package-lock.json` produced no output, confirming the lockfile was not modified. No install/build/test commands were run for this metadata-only task.
- closure_evidence: Technical-lead review accepted completion commit `7545c18 chore(frontend): pin Node and npm metadata`; committed file scope was `.nvmrc`, `frontend/package.json`, `PROGRESS.md`, and `docs/frontend/execution/frontend-implementation-ledger.md`; `frontend/package-lock.json`, Angular source/configuration, backend files, canonical OpenAPI, and Penpot/design source were not changed. Gate is closed as `VERIFIED`; `T-FE-003` remains `NOT STARTED` and requires explicit authorization.

### `T-FE-003` — Baseline install/build/test

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_date: 2026-09-04
- scope_summary: Executed the authorized generated-workspace baseline only: clean dependency installation from the committed lockfile, existing Angular build script, existing non-watch Angular/Vitest test command, and tracked drift checks.
- evidence_summary: Baseline HEAD `f163bb4`; Node `v22.23.1`; npm `11.6.0`; root `.nvmrc` `22.23.1`; `frontend/package.json` packageManager `npm@11.6.0`, engines.node `22.23.1`, engines.npm `11.6.0`. `npm ci` succeeded from `./frontend` using npm `11.6.0` with no legacy-peer-deps, force, install/update/upgrade workaround, `edgesOut`, ERESOLVE, or package-lock rewrite. Lockfile SHA-256 before, after `npm ci`, and final remained `0e5a064f6e9a8eb0f05f30f4fde41bfe12c2b826ba1246b60bd0696c6c8d3986`.
- dependency_baseline: Direct installed graph valid with `@angular/build 22.1.7`, `@angular/cli 22.1.7`, `@angular/common 22.1.5`, `@angular/compiler 22.1.5`, `@angular/compiler-cli 22.1.5`, `@angular/core 22.1.5`, `@angular/forms 22.1.5`, `@angular/platform-browser 22.1.5`, `@angular/router 22.1.5`, `jsdom 28.1.0`, `rxjs 7.8.2`, `typescript 6.0.3`, and `vitest 4.1.11`.
- build_test_evidence: `npm run build` succeeded using the existing generated `ng build` script; Angular reported application bundle generation complete in `5.298 seconds` and output location `frontend/dist/nursing-platform-frontend`. `npm test -- --watch=false` succeeded with Vitest `4.1.11`; `1` test file passed, `2` tests passed, `0` failed, duration `1.69s`.
- exclusion_summary: No tracked frontend package/config/source drift; no backend, canonical OpenAPI, or Penpot/design source changes; no lint/stylelint/AXE/Playwright/Material/CDK/API generation/dependency guardrail work; `T-FE-004` remains `NOT STARTED`.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `2b188b9`; completion_commit_subject `docs(frontend): record baseline build verification`; accepted clean-install/build/Vitest evidence preserved; `T-FE-004` remains `NOT STARTED` and requires explicit authorization.

### `ST-FE-003` — Run baseline install/test/build evidence

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Completed `npm ci`, installed direct dependency baseline, existing Angular build, existing non-watch Vitest baseline, and lockfile/source-scope checks under Node `v22.23.1` and npm `11.6.0`. Ready for technical-lead review; not self-marked `VERIFIED`.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `2b188b9 docs(frontend): record baseline build verification`; subtask accepted as complete. `T-FE-004` remains `NOT STARTED`.

### `GATE-FE-T003`

- status_result: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Gate evidence covers clean baseline at `f163bb4`, active approved Node/npm metadata, successful `npm ci` from committed lockfile without workaround flags, identical lockfile SHA-256 before/after/final, valid direct dependency graph, successful Angular build, successful non-watch Vitest run with zero failures, no tracked frontend source/config drift, no backend/OpenAPI/Penpot drift, no unrelated tooling installation/configuration, and `T-FE-004` remaining `NOT STARTED`.
- closure_evidence: Technical-lead review accepted completion commit `2b188b9 docs(frontend): record baseline build verification`; committed file scope was `PROGRESS.md` and `docs/frontend/execution/frontend-implementation-ledger.md`; no `frontend/**`, `.nvmrc`, backend files, canonical OpenAPI, or Penpot/design source were changed. Gate is closed as `VERIFIED`; `T-FE-004` remains `NOT STARTED` and requires explicit authorization.

### `T-FE-004` — Dependency guardrails

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_date: 2026-09-05
- baseline_head: `b4fa41f`
- scope_summary: Added a Node-stdlib-only direct dependency approval/denylist guard for the Angular workspace. Created `frontend/dependency-policy.json`, created `frontend/scripts/check-dependencies.mjs`, and added only the `check:dependencies` package script to `frontend/package.json`.
- policy_summary: `frontend/dependency-policy.json` snapshots the exact current direct runtime dependency declaration specifiers separately from exact current direct development dependency declaration specifiers. Unknown direct dependencies fail by default. Transitive dependencies are intentionally excluded from this direct guard and remain governed by `frontend/package-lock.json`.
- guard_summary: `frontend/scripts/check-dependencies.mjs` uses only Node standard-library imports, defaults to the real `frontend/package.json` and policy regardless of shell cwd, supports alternate manifest/policy inputs for safe temporary negative verification, exits `0` only when the current manifest exactly matches policy, and exits non-zero with actionable violations for policy/dependency failures. The script does not mutate files, install packages, or require registry/network access.
- verification_summary: Final post-review verification passed: `npm run check:dependencies` returned `POSITIVE_EXIT=0`; direct root invocation and unrelated temporary cwd invocation returned exit `0`; required negative cases rejected `primeng`, `@ngrx/store`, `tailwindcss`, `@angular/material`, `rxjs` specifier drift, runtime/dev category movement, unknown `left-pad`, removed approved `tslib`, and duplicate runtime/dev placement; invalid policy cases rejected runtime/dev duplicate approval, approved/denied overlap, and incompatible schema; unsupported `optionalDependencies` section was rejected.
- integrity_summary: `frontend/package.json` scope check confirmed `name`, `version`, `private`, `packageManager`, `engines`, `dependencies`, and `devDependencies` unchanged from `b4fa41f`; scripts changed only by adding `check:dependencies`. `frontend/package-lock.json` final SHA-256 and committed baseline SHA-256 both equal `0e5a064f6e9a8eb0f05f30f4fde41bfe12c2b826ba1246b60bd0696c6c8d3986`; `git diff --name-only -- frontend/package-lock.json` produced no output.
- exclusion_summary: No dependency was added, removed, installed, updated, or changed. No `npm install`, `npm ci`, `npm update`, `npm upgrade`, `npm uninstall`, Angular build/test/lint/stylelint/Playwright/AXE/OpenAPI generation, CI wiring, Git hook, backend, canonical OpenAPI, Penpot/design, Angular source, Angular config, `frontend/angular.json`, `frontend/tsconfig*.json`, `.nvmrc`, staging, commit, push, or later Task work occurred. The previous accidental `frontend/angular.json` analytics diff is absent.
- review_note: Previous deep-reviewer route was unavailable because its configured model is end-of-life; read-only verifier/routine-worker feedback was used as supporting evidence. Reviewer low findings were incorporated: cwd-independent manifest resolution, approved/denied disjointness validation, and unsupported dependency-section rejection. Final acceptance remains with the technical lead.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `0f75b43`; completion_commit_subject `chore(frontend): add dependency guardrails`; accepted dependency guard evidence preserved. `T-FE-005`, `T-FE-006`, and `T-FE-007` remain `NOT STARTED` and require explicit future authorization.

### `ST-FE-004` — Add dependency approval/denylist guard

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Implemented the direct dependency policy file, Node-only guard script, and package script. Final verification confirms exact direct dependency snapshot matching, required denylist behavior, unknown default-deny behavior, missing/specifier/category/duplicate rejection, malformed/inconsistent policy rejection, approved/denied overlap rejection, unsupported dependency-section rejection, no lockfile mutation, and no package installation.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `0f75b43 chore(frontend): add dependency guardrails`; subtask accepted as complete.

### `GATE-FE-T004`

- status_result: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Gate evidence is ready for technical-lead review. Final positive dependency guard output passed with `POSITIVE_EXIT=0`; cwd-independent invocations passed from repository root and unrelated temporary cwd; all required negative dependency cases and invalid policy cases exited non-zero with actionable messages; exact direct dependency declarations remain unchanged from `b4fa41f`; `frontend/package-lock.json` checksum is unchanged from committed baseline; source/config/backend/OpenAPI/Penpot drift checks produced no output; no files are staged; no commit or push was made. `T-FE-005`, `T-FE-006`, and `T-FE-007` remain `NOT STARTED`.
- closure_evidence: Technical-lead review accepted completion commit `0f75b43 chore(frontend): add dependency guardrails`; committed file scope was `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, `frontend/dependency-policy.json`, `frontend/package.json`, and `frontend/scripts/check-dependencies.mjs`. `frontend/package-lock.json`, `frontend/angular.json`, frontend source, `frontend/tsconfig*.json`, `.nvmrc`, backend files, canonical OpenAPI, and Penpot/design content were not changed. Gate is closed as `VERIFIED`; `T-FE-005`, `T-FE-006`, and `T-FE-007` remain `NOT STARTED`.

### `T-FE-005` — TypeScript/Angular lint setup

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_date: 2026-09-05
- baseline_head: `df688a1`
- tooling_approval: Technical lead approved exact `angular-eslint@22.1.0`, ESLint flat config, npm `11.6.0`, and the official schematic's direct lint dependency declarations: `@eslint/js: ^10.0.1`, `angular-eslint: 22.1.0`, `eslint: ^10.6.0`, and `typescript-eslint: 8.62.1`.
- scope_summary: The approved local Angular CLI schematic generated `frontend/eslint.config.js`, added only `lint: ng lint` and the four approved direct lint dev dependencies to `frontend/package.json`, added the `angular-eslint` schematic collection and `@angular-eslint/builder:lint` target for `src/**/*.ts` and `src/**/*.html` to `frontend/angular.json`, updated `frontend/package-lock.json`, and extended `frontend/dependency-policy.json` with the exact generated declarations.
- lockfile_semantics_note: Initial execution paused because the execution prompt incorrectly assumed schematic `--skip-install` prevents the outer `ng add` collection acquisition from changing `package-lock.json`. Technical-lead clarification accepted that early mutation as expected outer `ng add` behavior, not an angular-eslint incompatibility. Work resumed from the preserved generated artifacts without rerunning `ng add`. SHA-256 values: original `0e5a064f6e9a8eb0f05f30f4fde41bfe12c2b826ba1246b60bd0696c6c8d3986`; after `ng add` `6ec4d595017dc9831e37f1990b315689ab0ffcfa457be45de610446654f5c4b9`; final `b0b67e0b41ceed80d80bfa36b8cf30553a6282cd66668fd216873fcfb57316bd`.
- install_summary: Exact `npm install` under npm `11.6.0` added 32 packages, changed 4 packages, audited 623 packages, and reported 0 vulnerabilities; no `--legacy-peer-deps`, `--force`, update, upgrade, or audit-fix command was used.
- resolved_versions: `@eslint/js@10.0.1`, `angular-eslint@22.1.0`, `eslint@10.10.0`, and `typescript-eslint@8.62.1`. Existing direct framework/test/toolchain resolved versions remained unchanged: Angular build/CLI `22.1.7`; Angular packages/compiler-cli `22.1.5`; jsdom `28.1.0`; RxJS `7.8.2`; TypeScript `6.0.3`; Vitest `4.1.11`.
- verification_summary: `npm run check:dependencies` passed before and after install; temporary-manifest checks rejected `left-pad` before install and rejected `primeng`, `@ngrx/store`, `tailwindcss`, `@angular/material`, and `left-pad` after install, all with exit `1`. `npm run lint` exited `0` with `All files pass linting.` and no warnings or errors. Machine checks returned `PACKAGE_LOCK_SCOPE_PASS` and `ANGULAR_CONFIG_SCOPE_PASS`; lockfile root metadata exactly matches package declarations and lockfileVersion remains `3`.
- review_note: Independent deterministic verification and Git/scope audit returned `PASS`. The configured independent deep-review model was unavailable because the provider reports it as end-of-life; no unapproved technical-review model was substituted. Documentation review feedback to rewrite historical `T-FE-004` closure statements was rejected because those statements correctly preserve the state at the time `T-FE-004` closed. Final acceptance remains with the technical lead.
- exclusion_summary: No application source, TypeScript config, `.nvmrc`, backend, canonical OpenAPI, Penpot/design, stylelint, AXE setup, Playwright, Material/CDK, API generation, CI, Git hook, source lint fix, suppression, staging, commit, push, or later Task work occurred. `T-FE-006`, `T-FE-007`, and `T-FE-014` remain `NOT STARTED`.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `b6e51e9`; completion_commit_subject `chore(frontend): configure Angular lint tooling`; accepted T-FE-005 implementation and verification evidence preserved. `T-FE-006`, `T-FE-007`, and `T-FE-014` remain `NOT STARTED` and require explicit future authorization.

### `ST-FE-005` — Approve/configure TypeScript/Angular lint command

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_summary: Exact approved angular-eslint tooling is installed and configured with the stock flat ESLint baseline, Angular TypeScript and inline-template processing, HTML template recommended/accessibility rules, project selector rules, Angular CLI lint builder, `npm run lint`, exact dependency-policy approvals, passing dependency guards, and zero-error lint output.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `b6e51e9 chore(frontend): configure Angular lint tooling`; subtask accepted as complete.

### `GATE-FE-T005`

- status_result: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_summary: Technical-lead tooling approval is exact `angular-eslint@22.1.0`; generated flat config is `frontend/eslint.config.js`; Angular lint target and `lint: ng lint` are configured; only the four approved direct lint dev dependencies were added; normal npm `11.6.0` install completed with 0 vulnerabilities; existing direct versions did not drift; dependency policy and focused default-deny regressions passed; `npm run lint` passed with no warnings/errors; source/config/backend/OpenAPI/Penpot and later-Task exclusions passed; no files are staged and no commit or push was made.
- closure_evidence: Technical-lead review accepted completion commit `b6e51e9 chore(frontend): configure Angular lint tooling`; committed file scope was `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, `frontend/angular.json`, `frontend/dependency-policy.json`, `frontend/eslint.config.js`, `frontend/package-lock.json`, and `frontend/package.json`. No application source, TypeScript config, `.nvmrc`, backend, canonical OpenAPI, Penpot/design, or later-Task files were changed. Gate is closed as `VERIFIED`; `T-FE-006`, `T-FE-007`, and `T-FE-014` remain `NOT STARTED`.

### `T-FE-006` — SCSS/stylelint setup

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_date: 2026-09-05
- baseline_head: `79e7599`
- tooling_approval: Technical lead approved exact `stylelint@17.14.1`, `stylelint-config-standard-scss@17.0.0`, npm `11.6.0`, the `lint:styles` command, hand-authored ESM configuration, and explicit physical-direction property/value prohibitions.
- scope_summary: Add only the two approved direct dev dependencies, matching dependency-policy declarations, the approved npm script, `frontend/stylelint.config.mjs`, authorized lockfile resolution, and task evidence. No application source correction or suppression is authorized.
- install_summary: Normal `npm install` under npm `11.6.0` added 84 packages, audited 707 packages, and reported 0 vulnerabilities. Exact direct resolutions are `stylelint@17.14.1` and `stylelint-config-standard-scss@17.0.0`; all pre-existing direct resolved versions remained unchanged; dependency policy and lockfile root metadata checks passed.
- blocker_evidence: Required `npm run lint:styles` exited non-zero because generated `frontend/src/app/app.scss` is empty: `1:1 Empty source no-empty-source`. The inherited standard-SCSS rule is outside the approved direction-rule additions, while application source edits and suppressions are explicitly excluded. Technical-lead clarification is required.
- clarification: Technical lead explicitly authorized `no-empty-source: null` in `frontend/stylelint.config.mjs`, preserving the original six-file scope and avoiding an application-source edit. Execution resumed for the prescribed fixture and regression evidence.
- resolved_versions: `stylelint@17.14.1` and `stylelint-config-standard-scss@17.0.0`; transitive SCSS support resolves through `stylelint-config-recommended-scss@17.0.1`, `postcss-scss@4.0.9`, and `stylelint-scss@7.2.0`. Existing direct framework/test/toolchain versions remained unchanged: Angular build/CLI `22.1.7`; Angular packages/compiler-cli `22.1.5`; `@eslint/js@10.0.1`; `angular-eslint@22.1.0`; `eslint@10.10.0`; `jsdom@28.1.0`; Prettier `3.9.6`; RxJS `7.8.2`; TypeScript `6.0.3`; `typescript-eslint@8.62.1`; Vitest `4.1.11`.
- lockfile_summary: Initial SHA-256 `b0b67e0b41ceed80d80bfa36b8cf30553a6282cd66668fd216873fcfb57316bd`; final SHA-256 `bd50f8c4c67995cdaa9dc6161c41e24f9cefc95cab3bd541eff0521f8f759e4b`; lockfileVersion remains `3`, root package metadata exactly matches `frontend/package.json`, and lock changes are limited to approved Stylelint dependencies and their transitive closure.
- verification_summary: Real-source `npm run lint:styles` passed after the authorized clarification. The corrected temporary logical-property fixture passed with no output; the physical-direction fixture failed as required with exactly 10 errors covering all six prohibited properties and both prohibited values for `float` and `text-align`. Dependency negatives rejected `stylelint` version drift, preset version drift, and direct `postcss-scss`. `npm run check:dependencies`, `npm audit`, `npm run lint`, `npm test -- --watch=false` (2/2), `npm run build`, package-lock metadata, `git diff --check`, prohibited-area drift, and temporary-residue checks passed.
- focused_dependency_guard_evidence: Real manifest passed with exit `0`. Independent temporary manifests were rejected with exit `1` for `primeng`, `@ngrx/store`, `tailwindcss`, `@angular/material`, and arbitrary unknown dependency `left-pad`; the real `frontend/package.json` was not modified.
- direction_count_evidence: The exact temporary ten-case physical-direction fixture exited `2` with 10 targeted violations: `property-disallowed-list` = 6, `declaration-property-value-disallowed-list` = 4, unrelated rule findings = 0. The fixture was outside the repository and removed after verification.
- angular_config_hygiene: During gate-evidence resumption, `frontend/angular.json` was unexpectedly dirty. Textual inspection showed formatting expansion plus `analytics: false`; exact semantic JSON comparison against HEAD reported only `$.cli.analytics: ADDED = False`, classified `ANALYTICS_ONLY_ACCIDENTAL_MUTATION`. Under explicit technical-lead authorization, only `frontend/angular.json` was restored from committed HEAD; no T-FE-006 Angular configuration change remains.
- review_note: Independent documentation review, Git/scope audit, and deterministic verification returned `PASS`. The configured independent deep-review model was unavailable because its provider reports it as end-of-life; no unapproved review model was substituted. Final acceptance remains with the technical lead.
- exclusion_summary: No application source, Angular config, ESLint config, TypeScript config, dependency-guard script, `.nvmrc`, backend, canonical OpenAPI, Penpot/design, CI, AXE, Playwright, Material/CDK, runtime RTL/localization, API generation, Git hook, staging, commit, push, or later-Task work occurred. `T-FE-007` and `T-FE-014` remain `NOT STARTED`.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `d94cc45`; completion_commit_subject `chore(frontend): configure SCSS lint tooling`; accepted T-FE-006 implementation and verification evidence preserved. `T-FE-007` and `T-FE-014` remain `NOT STARTED` and require explicit future authorization.

### `ST-FE-006` — Configure SCSS/stylelint and physical direction prohibition

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_summary: Metadata, exact installation, dependency integrity, real-source lint, logical-property acceptance, ten-case physical-direction rejection, regression commands, drift checks, and independent audits passed after the technical lead authorized `no-empty-source: null` for generated empty component SCSS.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `d94cc45 chore(frontend): configure SCSS lint tooling`; subtask accepted as complete.

### `GATE-FE-T006`

- status_result: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_summary: Exact tooling approval and dependency integrity are satisfied; initial `no-empty-source` failure and technical-lead clarification are preserved; final real-source lint, logical/physical direction fixtures, dependency negatives, zero-vulnerability audit, Angular lint, 2/2 unit tests, production build, lockfile checks, six-file scope checks, no-staging check, and available independent reviews passed. This evidence was subsequently accepted by the technical lead.
- closure_evidence: Technical-lead review accepted completion commit `d94cc45 chore(frontend): configure SCSS lint tooling`; committed file scope was `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, `frontend/dependency-policy.json`, `frontend/package-lock.json`, `frontend/package.json`, and `frontend/stylelint.config.mjs`. No Angular configuration, application source, ESLint config, TypeScript config, `.nvmrc`, backend, canonical OpenAPI, Penpot/design, or later-Task files were changed. Gate is closed as `VERIFIED`; `T-FE-007` and `T-FE-014` remain `NOT STARTED`.

### `T-FE-007` — CI-local quality command

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_date: 2026-09-05
- baseline_head: `245de87`
- technical_lead_decision: Implementation shape is package.json script only. No new dependency, helper script, shell file, Node helper, dependency-policy change, CI mutation, or Git hook is approved. Approved script name is `quality`.
- scope_summary: Added only `scripts.quality` to `frontend/package.json`. The exact value is `npm run check:dependencies && npm run lint && npm run lint:styles && npm test -- --watch=false && npm run build`. All pre-existing package metadata, dependencies, devDependencies, and scripts are preserved.
- verification_summary: `npm run quality` from `frontend/` exited `0` and sequentially executed the dependency guard, Angular lint, Stylelint, non-watch unit tests, and production build. Component outcomes: dependency guard passed; Angular lint passed; Stylelint passed; unit tests passed with `1` test file and `2` tests; production build passed.
- integrity_summary: `frontend/package-lock.json` SHA-256 before and after T-FE-007 is `bd50f8c4c67995cdaa9dc6161c41e24f9cefc95cab3bd541eff0521f8f759e4b`. `frontend/package-lock.json` and `frontend/dependency-policy.json` have no diff. Machine package comparison against HEAD `245de87` confirmed only `scripts.quality` differs semantically.
- fail_fast_summary: Fail-fast command composition is structurally provided by shell `&&` between all five component commands; no destructive negative test was required or performed.
- exclusion_summary: No dependency installation, dependency declaration change, lockfile change, dependency-policy change, helper file, shell script, Node helper, CI file, Git hook, accessibility tooling, AXE, Playwright, application source change, quality-tool configuration change, backend change, canonical OpenAPI change, Penpot/design write, `T-FE-014`, `T-FE-008`, staging, commit, or push occurred.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `297dbdd`; completion_commit_subject `chore(frontend): add local quality command`; accepted T-FE-007 implementation and verification evidence preserved. `T-FE-014` and `T-FE-008` remain `NOT STARTED` and require explicit future authorization.

### `ST-FE-007` — Compose local quality command without CI mutation

- status: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Implemented the approved package.json-only local aggregate quality command with exact dependency guard, Angular lint, Stylelint, non-watch test, and production build sequence. Verification and integrity checks passed; no CI, hook, helper, dependency, lockfile, dependency-policy, source, tool-config, a11y, backend, OpenAPI, Penpot/design, or later-Task work occurred.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `297dbdd chore(frontend): add local quality command`; subtask accepted as complete.

### `GATE-FE-T007`

- status_result: `VERIFIED`
- blocker_types: `DEPENDENCY`
- evidence_summary: Technical-lead review accepted the existing gate evidence. Exact `quality` script exists in `frontend/package.json`; no dependency was added; lockfile checksum stayed `bd50f8c4c67995cdaa9dc6161c41e24f9cefc95cab3bd541eff0521f8f759e4b`; dependency policy is unchanged; no helper, CI, hook, or a11y tooling was created; accepted `npm run quality` evidence exited `0`; all five composed checks passed; fail-fast is provided by `&&`; source/tool-config/backend/OpenAPI/Penpot drift checks produced no output; `T-FE-014`, `T-FE-008`, and later Tasks were not started.
- closure_evidence: Technical-lead review accepted completion commit `297dbdd chore(frontend): add local quality command`; committed file scope was `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, and `frontend/package.json`. No package lock, dependency policy, application source, quality-tool configuration, `.nvmrc`, backend, canonical OpenAPI, Penpot/design, CI, hook, accessibility-tooling, or later-Task file was changed. Gate is closed as `VERIFIED`; `T-FE-014` and `T-FE-008` remain `NOT STARTED`.

### `T-FE-008` — Runtime design tokens

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-06
- baseline_head: `205f163`
- technical_lead_decision: T-FE-008 is authorized as the first runtime/frontend implementation task after tooling. Scope is exactly the approved concrete runtime token subset only, with no new dependency, no Material theme bridge, no accessibility style behavior, no direction/locale foundation, no responsive helpers, no form-control foundation, no AXE/Playwright/a11y automation, and no screen implementation.
- scope_summary: Create only `frontend/src/styles/_tokens.scss`; modify only `frontend/src/styles.scss` plus execution memory. Runtime representation is CSS custom properties emitted from the SCSS token source. `_tokens.scss` is the single source of truth for this task's runtime tokens.
- token_subset_summary: Implemented only 31 approved CSS custom properties: 3 brand foundation colors (`#006B66`, `#173B57`, `#4F46B8`), 3 focus foundation tokens (`2px`, `4px`, `var(--np-color-brand-1)`), 16 base spacing tokens (`4px` through `64px` in 4px increments), 8 optical spacing tokens (`2px`, `6px`, `10px`, `14px`, `18px`, `22px`, `26px`, `30px`), and scrim opacity `0.48`.
- implementation_summary: `frontend/src/styles/_tokens.scss` is the single token source for T-FE-008 and emits runtime CSS custom properties from `:root`. `frontend/src/styles.scss` only loads the token source with `@use 'styles/tokens';`. No duplicate SCSS variables, maps, JSON token files, TypeScript token files, Material theme files, theme classes, dark theme, runtime mutation service, mixins, utilities, component styles, page styles, or product-screen implementation were added.
- verification_summary: Pre-task baseline matched authorization: HEAD `205f163`, clean working tree, no staged files, Node `v22.23.1`, npm `11.6.0`. RED deterministic token assertion failed before `_tokens.scss` existed. Initial Stylelint run failed only for blank-line formatting (`custom-property-empty-line-before`); formatting was corrected without changing tokens. Final `npm run lint:styles` passed. Deterministic token assertion reported `TOKEN_COUNT=31`, `EXPECTED_COUNT=31`, duplicates `none`, unexpected `none`, missing `none`, mismatches `none`. `npm run quality` exited `0` and passed dependency guard, Angular lint, Stylelint, 1 test file / 2 tests, and production build. Compiled CSS under `frontend/dist/nursing-platform-frontend/browser/styles-IY5G5PMG.css` emitted representative runtime properties including `--np-color-brand-1: #006B66`, `--np-focus-ring-width`, `--np-space-4`, `--np-space-optical-2`, and `--np-scrim-opacity` normalized as `.48`.
- integrity_summary: No package, lockfile, dependency-policy, Angular config, ESLint config, Stylelint config, TypeScript config, `.nvmrc`, `frontend/src/app`, backend source/tests, canonical OpenAPI, Penpot/design source, Material/CDK bridge, accessibility automation, direction/locale, responsive helper, form-control foundation, or screen implementation changes occurred. Search of T-FE-008 changed source found no `#00796B`.
- deferred_summary: Neutral palette, typography scale, font loading/families, numeric elevation shadows, z-index scale, motion durations/easing, disabled-state tokens, component-local aliases, status colors, surface/background/text/border colors, breakpoints, gutters, border-radius/form-control tokens, control heights, 48x48 control target, RTL selectors, and locale state remain excluded/deferred to their owning future tasks or unresolved design decisions. `T-FE-009..T-FE-014` boundaries are preserved. SCREEN IMPLEMENTATION APPROVAL remains independently required.
- closure_evidence: Technical-lead verdict `PASS`; completion_commit `99e3739`; completion_commit_subject `feat(frontend): add runtime design tokens`; accepted T-FE-008 implementation and verification evidence preserved. `T-FE-009` and `T-FE-014` remain `NOT STARTED` and require explicit future authorization.

### `ST-FE-008` — Define runtime semantic tokens from approved foundation

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Defined runtime semantic tokens from the approved foundation subset only. The token source contains exactly 31 custom properties across 3 brand foundation colors, 3 focus foundation tokens, 16 base spacing tokens, 8 optical spacing tokens, and 1 scrim token. Deterministic assertion, Stylelint, aggregate quality, compiled CSS evidence, dependency/config/source/backend/OpenAPI/design integrity, and neighbor-task boundary checks passed.
- closure_evidence: Technical-lead verdict `PASS`; completion commit `99e3739 feat(frontend): add runtime design tokens`; subtask accepted as complete.

### `GATE-FE-T008`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Runtime token/style evidence is ready for technical-lead review. The approved subset only is implemented as 31 exact CSS custom properties in `frontend/src/styles/_tokens.scss`; `frontend/src/styles.scss` loads the source globally; no duplicate token truth or neighboring-task work was created. Final checks passed: deterministic token assertion, `npm run lint:styles`, `npm run quality`, compiled CSS token emission, package/dependency integrity, app/config/backend/OpenAPI/design integrity, `#00796B` absence from changed source, and final Git no-staging check.
- closure_evidence: Technical-lead review accepted completion commit `99e3739 feat(frontend): add runtime design tokens`; committed file scope was `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, `frontend/src/styles.scss`, and `frontend/src/styles/_tokens.scss`. No package, lockfile, dependency policy, Angular/ESLint/Stylelint/TypeScript config, `.nvmrc`, `frontend/src/app`, backend, canonical OpenAPI, Penpot/design, Material/CDK bridge, accessibility automation, direction/locale, responsive helper, form-control foundation, or screen implementation files were changed. Gate is closed as `VERIFIED`; `T-FE-009` and `T-FE-014` remain `NOT STARTED`.

### `T-FE-009` — Angular Material theme bridge

- status: `BLOCKED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-06
- baseline_head: `1145863`
- technical_decisions_resolved: Future T-FE-009 implementation is approved to use exact direct pins `@angular/material@22.1.5` and `@angular/cdk@22.1.5`; `@angular/animations` is not approved and not required; theme family remains M2 using `mat.m2-define-palette(...)` and `mat.m2-define-light-theme(...)`; scope is a single light theme only; bridge path remains `frontend/src/styles/_material-theme-bridge.scss`; future integration remains through `frontend/src/styles.scss`; custom Material typography and density remain deferred unless separately approved.
- design_blocker_summary: Implementation is blocked by missing approved design inputs, not by Angular, npm, Material/CDK version ambiguity, animations, or tooling. The unresolved inputs are Material primary/accent role mapping for `--np-color-brand-1`, `--np-color-brand-2`, and `--np-color-brand-3`; complete approved M2 hue/contrast palette data; and an approved warn/error palette or explicit Material fallback policy. Draft `#00796B` remains non-authoritative; the approved foundation value remains `#006B66` until explicit design authority supersedes it.
- exclusion_summary: No Material packages were installed, no dependency-policy change was made, no `_material-theme-bridge.scss` was created, no theme source was implemented, no build/lint/test/quality command was rerun, no frontend/backend/OpenAPI/Penpot/design source changed, and no `T-FE-010` or later task was started.

### `ST-FE-009` — Bridge tokens to single Angular Material theme

- status: `BLOCKED`
- blocker_types: `DESIGN`
- evidence_summary: Subtask is blocked until approved design authority provides either complete M2 primary/accent/warn palettes with hue and contrast data plus role assignment, or an explicit fallback policy naming the allowed Material defaults/fallback palettes and exact Nursing Platform brand-token role mapping.

### `GATE-FE-T009`

- status_result: `BLOCKED`
- blocker_types: `DESIGN`
- evidence_summary: Gate is not ready for Material theme build/CSS evidence because the required design inputs for safe M2 palette construction are absent. `GATE-FE-T009` is not `VERIFIED`.

### `T-FE-010` — Base accessibility styles

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-07
- implementation_summary: Implemented the bounded base accessibility foundation only: global tokenized `:focus-visible`, `.u-visually-hidden`, `touch-target($mobile: false)` mixin, focused source-contract tests, and global `styles.scss` wiring.
- verification_summary: OpenAI final-gate evidence passed: `npm test -- --watch=false` passed 2 files / 6 tests, `npm run lint:styles` passed, `npm run quality` passed, `git diff --check` passed, and `git diff --cached --name-status` was empty.
- integrity_summary: Changed Task files were limited to `frontend/src/styles.scss`, `frontend/src/styles/_accessibility.scss`, `frontend/src/styles/abstracts/_mixins.scss`, and `frontend/src/styles/accessibility.spec.ts`. No staging, commit, push, Material/CDK, accessibility tooling, dependency, backend, OpenAPI, Penpot, `T-FE-011+`, or screen work occurred.
- closure_evidence: Technical-lead verdict `PASS`; accepted T-FE-010 implementation and verification evidence preserved; Task is closed as `VERIFIED`. Completion commit `cc73fc8 feat(frontend): add accessibility style foundation` (atomic checkpoint; pre-commit "no staging/commit" wording is superseded for this slice).

### `ST-FE-010` — Establish focus/touch/screen-reader utilities

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Focus-visible styling, screen-reader utility, touch-target mixin, focused tests, Stylelint, quality, and diff hygiene passed under OpenAI final gate.
- closure_evidence: Technical-lead verdict `PASS`; subtask accepted as complete.

### `GATE-FE-T010`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Accessibility utility evidence exists and OpenAI final gate accepted it: focused and full frontend tests, Stylelint, quality/build, `git diff --check`, and no staged files.
- closure_evidence: Technical-lead review accepted T-FE-010 with verdict `PASS`; Gate is closed as `VERIFIED`. Committed scope `cc73fc8`: `frontend/src/styles.scss`, `frontend/src/styles/_accessibility.scss`, `frontend/src/styles/abstracts/_mixins.scss`, `frontend/src/styles/accessibility.spec.ts`.

### `T-FE-011` — Direction/locale foundation

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-07
- implementation_summary: Implemented the bounded direction/locale foundation only: initial `<html lang="en" dir="ltr">`, central signal-based locale/direction state, document `lang`/`dir` synchronization, `en -> ltr`, `ar -> rtl`, default `en/ltr`, and temporary `system-ui, sans-serif` font mapping for both locales.
- verification_summary: OpenAI final-gate evidence passed: focused locale-direction service tests passed 1 file / 10 tests; full frontend tests passed 3 files / 16 tests; `npm run lint:styles`, `npm run quality`, `git diff --check`, package/lock/dependency-policy diff, and `git diff --cached --name-status` passed.
- integrity_summary: Changed Task files were limited to `frontend/src/index.html`, `frontend/src/app/app.ts`, `frontend/src/app/core/locale/locale-direction.service.ts`, and `frontend/src/app/core/locale/locale-direction.service.spec.ts`. No persistence, CDK/Bidi, translations/i18n, selector UI, full RTL conversion, staging, commit, push, dependency, backend, OpenAPI, Penpot, or `T-FE-012+` work occurred.
- closure_evidence: Technical-lead verdict `PASS`; accepted T-FE-011 implementation and verification evidence preserved; Task is closed as `VERIFIED`. Completion commit `1b78732 feat(frontend): add locale direction foundation` (atomic checkpoint; pre-commit "no staging/commit" wording is superseded for this slice).

### `ST-FE-011` — Implement document `dir`, direction state, font mapping, bidi checks

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Locale/direction service, document synchronization, focused service tests, full tests, Stylelint, quality, package integrity, and diff hygiene passed under OpenAI final gate.
- closure_evidence: Technical-lead verdict `PASS`; subtask accepted as complete.

### `GATE-FE-T011`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Direction/font/bidi evidence exists and OpenAI final gate accepted it: focused service tests, full frontend tests, Stylelint, quality/build, package/dependency integrity, `git diff --check`, and no staged files.
- closure_evidence: Technical-lead review accepted T-FE-011 with verdict `PASS`; Gate is closed as `VERIFIED`. Committed scope `1b78732`: `frontend/src/index.html`, `frontend/src/app/app.ts`, `frontend/src/app/core/locale/locale-direction.service.ts`, `frontend/src/app/core/locale/locale-direction.service.spec.ts`.

### `T-FE-012` — Responsive helpers

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-07
- implementation_summary: Implemented the bounded minimal responsive foundation only: approved Sass breakpoint map, `respond-from($breakpoint)`, runtime `--np-page-gutter`, `page-gutter` logical mixin, focused source-contract tests, and global `styles.scss` wiring.
- verification_summary: OpenAI final-gate evidence passed: full frontend tests passed 4 files / 22 tests including `src/styles/responsive.spec.ts` with 6 tests; `npm run lint:styles` passed; `npm run quality` passed including production build; `git diff --check` passed; `git diff --cached --name-status` was empty. Focused file filtering with `--watch=false src/styles/responsive.spec.ts` is unsupported by the current Angular builder and failed before final full-suite verification.
- integrity_summary: Changed Task files were limited to `frontend/src/styles.scss`, `frontend/src/styles/_responsive.scss`, `frontend/src/styles/abstracts/_responsive.scss`, and `frontend/src/styles/responsive.spec.ts`. No unapproved breakpoint, extra gutter, container/readable max-width, broad utility framework, physical-direction CSS, dependency, backend, OpenAPI, Penpot, `T-FE-013`, `T-FE-014`, staging, commit, or push occurred.
- closure_evidence: Technical-lead verdict `PASS`; accepted T-FE-012 implementation and verification evidence preserved; Task is closed as `VERIFIED`. Completion commit `67dd5eb feat(frontend): implement responsive helpers` (atomic checkpoint; pre-commit "no staging/commit" wording is superseded for this slice).

### `ST-FE-012` — Implement breakpoint/gutter/logical layout helpers

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Breakpoint map, `respond-from`, page gutter custom property, `page-gutter`, source-contract tests, full tests, Stylelint, quality/build, and diff hygiene passed under OpenAI final gate.
- closure_evidence: Technical-lead verdict `PASS`; subtask accepted as complete.

### `GATE-FE-T012`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Responsive/logical CSS evidence exists and OpenAI final gate accepted it: full frontend tests, Stylelint, quality/build, source-contract tests for breakpoint/gutter/helper/wiring/physical-direction invariants, `git diff --check`, and no staged files.
- closure_evidence: Technical-lead review accepted T-FE-012 with verdict `PASS`; Gate is closed as `VERIFIED`. Committed scope `67dd5eb`: `frontend/src/styles.scss`, `frontend/src/styles/_responsive.scss`, `frontend/src/styles/abstracts/_responsive.scss`, `frontend/src/styles/responsive.spec.ts`.

### `T-FE-015` — API generator spike

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Completed bounded isolated generator spike only against canonical OpenAPI `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json`. Generated output stayed outside the repository at `/home/karam/.local/share/opencode/tool-output/t-fe-015-api-generator-spike/`; no generated client was integrated, no dependency was permanently installed, and no backend/OpenAPI/frontend package/config/source mutation was made for the spike.
- orchestration_note: Initial delegated Muse attempt was blocked by an OpenCode permission rejection for its compound shell inspection command. OpenAI performed the remaining direct, targeted tooling evidence collection under the permitted direct-execution exception for approved-worker permission blockage; repository-heavy implementation was not started.
- candidate_summary: Candidate tested was `ng-openapi-gen@1.0.5`. `--help` identifies it as an Angular 16+ OpenAPI 3.0/3.1 client generator; `--version` returned `1.0.5`. Generation command used `--services true --promises false --module false --index-file true --enum-style alias --use-temp-dir true`.
- generation_summary: Two isolated generation runs completed into `run1` and `run2`. Both emitted `269` TypeScript files and identical aggregate SHA-256 `89bf7a43026d1c21777646233e0f457d923f543d81542fedf8874b82ce2f1887`; `diff -qr run1 run2` produced no output. Generator warnings were limited to the canonical root `/.get` operation missing `operationId` (generator assumed `get`) and `HttpValidationProblemDetails` ignored as unused.
- contract_fit_summary: Generated output creates injectable Angular services plus functional request helpers and a root `ApiConfiguration`; CV multipart upload is represented as `body: { file: Blob }` with `multipart/form-data`; nullable optional OpenAPI fields are represented as optional properties with `(T | null)` where applicable; Problem Details variants generated as typed interfaces including `ProblemDetails`, `ValidationProblemDetails`, `CodedProblemDetails`, and `RetryableProblemDetails`. Bearer/security is not embedded in generated calls, which keeps auth header injection owned by future frontend interceptors. Numeric backend enums currently generate as numeric aliases such as `export type PaymentOrderStatus = number`, so user-facing enum label mapping remains an adapter/UI concern if this generator is approved.
- verification_summary: Help/version/generation/determinism commands were executed. An attempted isolated TypeScript syntax/type check failed inconclusively because the generated files live outside the Angular workspace and TypeScript module resolution could not resolve `@angular/*` and `rxjs` from that external path without copying/integrating generated files; no repository copy/integration was performed because T-FE-015 forbids adoption/integration.
- recommendation_for_t_fe_016: Candidate is viable for approval consideration only with an explicit T-FE-016 decision on exact version, generated-client destination, command/config file, root URL policy, generated-code lint policy, enum-label adapter policy, and auth/ProblemDetails adapter boundaries. Do not proceed to T-FE-016 without technical-lead tooling approval.
- exclusion_summary: No permanent dependency installation, no `frontend/package.json` or `frontend/package-lock.json` mutation, no `frontend/dependency-policy.json` mutation, no generated client integration, no backend or canonical OpenAPI mutation, no Penpot/design mutation, no staging, no commit, and no push occurred.
- closure_evidence: Technical-lead verdict `PASS` accepted the bounded isolated spike evidence (`ng-openapi-gen@1.0.5`, deterministic two-run generation, no integration/install/mutation); Task is closed as `VERIFIED`. No T-FE-016 approval scope was executed by this closure.

### `ST-FE-015` — Spike generator against canonical OpenAPI critical contracts

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION`
- evidence_summary: `ng-openapi-gen@1.0.5` generated deterministic isolated Angular TypeScript output for the canonical OpenAPI snapshot; critical evidence covers output determinism, Angular service/helper structure, multipart `file: Blob`, Problem Details interfaces, nullable fields, non-embedded Bearer auth, and numeric enum aliases. T-FE-016 must decide whether to approve/adopt this generator and its configuration.
- closure_evidence: Technical-lead verdict `PASS` on the parent `T-FE-015` spike; subtask accepted as complete and closed as `VERIFIED`.

### `GATE-FE-T015`

- status_result: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION`
- evidence_summary: Generator spike report is recorded for technical-lead review. The spike is isolated and reversible, deterministic across two runs, and identifies adoption decisions required before `T-FE-016`: exact generator/version/config/location, generated-client Git policy, enum adapter policy, auth interceptor boundary, Problem Details normalization boundary, and generated-code lint/format policy.
- closure_evidence: Technical-lead verdict `PASS` on the spike report; Gate is closed as `VERIFIED`. `T-FE-016` approval decisions are recorded below and remain subject to OpenAI final gate.

### `T-FE-016` — API generator approval

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_date: 2026-09-07
- approval_reference: Technical-lead tooling approval of exact preferred generator `ng-openapi-gen@1.0.5` via packet `FE-T016-CONTRACT-2026-09-07`; minimum targeted repository inspection only; no full spike rerun. Recorded for OpenAI final gate. Later correction approval supersedes the original `services: true` output-shape only; `T-FE-018` is not approved by this record.
- generator_version: Exact `ng-openapi-gen@1.0.5` only, as a `frontend` devDependency to be added by `T-FE-017` through the approved dependency-policy change process. No install, package, lockfile, or dependency-policy change was made by `T-FE-016`.
- canonical_input: `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` (repo-relative; verified present). `T-FE-017` must run from `frontend/` with input `../docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json`. No canonical OpenAPI mutation is authorized.
- output_location: `frontend/src/app/core/api/generated/` (repo-relative; the single isolated generated-code directory under Core API infrastructure, consistent with the `core`/`shared`/`features` ownership boundaries and the centralized-API rule). No other generated location is authorized. Existing paused generated output from the failed `services: true` attempt must be cleaned/replaced during the approved `services: false` resume; it is not verified and must not be manually edited.
- configuration_shape: Authoritative corrected CLI options for resume: `--services false --promises false --module false --index-file true --enum-style alias --use-temp-dir true`, plus explicit `--input`/`--output` above. This supersedes only the original spike-tested `--services true` option that produced uncompilable service facades; all other generator/version/input/output/manual-edit/drift/interceptor/adapter boundaries remain unchanged. If the generator schema rejects any key, `T-FE-017` must STOP rather than guess a substitute.
- npm_invocation: `T-FE-017` must add and use script `generate:api` run from `frontend/` as `npm run generate:api`, mapped to `ng-openapi-gen --input ../docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json --output src/app/core/api/generated --services false --promises false --module false --index-file true --enum-style alias --use-temp-dir true`. Pinned executable `ng-openapi-gen@1.0.5` only; npm is the only approved package manager.
- root_url_policy: Generation must not bake environment URLs or secrets. `ApiConfiguration.rootUrl` is set at runtime by `T-FE-018`; no hardcoded production URL in generated code or config.
- lint_policy: `T-FE-017` must record the generated-directory exclusion from `lint`/`lint:styles`/formatter scope; type-safety of generated output is proven via build (`npm run build`), not via lint of generated files.
- git_policy: Generated output under `frontend/src/app/core/api/generated/**` is tracked in Git (no `.gitignore` entry); regeneration from the same canonical snapshot must be byte-identical.
- regeneration_drift_policy: Regenerate only from the canonical snapshot above. `T-FE-017` gate must prove determinism (two-run `diff -qr` clean with matching aggregate checksum, or regen-to-temp diff against committed output with no differences). A canonical OpenAPI change requires a new approved task; never silently regenerate. Drift fails the gate.
- manual_edit_prohibition: Files under `frontend/src/app/core/api/generated/` must never be edited manually. Fixes go through generator config or thin adapters; a required hand-edit means STOP and escalate.
- integration_boundary: Generated services/helpers/models plus root `ApiConfiguration` stay isolated with no component, feature-state, interceptor, guard, or business-logic concerns. Bearer/token injection stays out of generated calls and belongs to the future functional interceptor chain (`T-FE-025`); Problem Details normalization belongs to `T-FE-019`; DTO adaptation belongs to thin feature adapters (`T-FE-020`); numeric enum aliases stay as generated, with user-facing labels mapped in adapters/UI. Generated output that misrepresents the canonical contract causes STOP AND ESCALATE.
- exclusion_summary: No dependency install, no `frontend/package.json`/`frontend/package-lock.json`/`frontend/dependency-policy.json` mutation, no generation run, no generated-client integration, no backend/OpenAPI mutation, no application feature implementation, no staging, no commit, and no push occurred.
- evidence_summary: Approval contract above is exact enough for `T-FE-017` to generate without guessing: generator/version, config shape, input, output, invocation, root-URL, lint, Git, drift, manual-edit, and interceptor/adapter boundaries are all fixed, with STOP rules for schema mismatch, drift, and contract misrepresentation. Muse/free-worker-first routing is preserved.
- correction_approval: Technical lead approved correction option 1 for the `T-FE-017` resume: revise generation from `services: true` to `services: false` and resume with the standard function-based client output. No backend/OpenAPI mutation, no custom templates, and no manual generated-file edits are authorized. The original `services: true` attempt remains historical evidence for the blocker; the corrected `services: false` configuration above is authoritative for resume. `T-FE-016` remains `VERIFIED`; `T-FE-017` remains `BLOCKED`/paused until resumed.

### `ST-FE-016` — Record exact generator/version/config approval

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_summary: Exact approval recorded under `T-FE-016`: `ng-openapi-gen@1.0.5`, corrected `services: false` CLI shape for resume, canonical input path, isolated output `frontend/src/app/core/api/generated/`, reproducible `generate:api` invocation, runtime root-URL policy, generated-code lint exclusion, tracked-Git with byte-identical drift policy, manual-edit prohibition, and interceptor/adapter boundaries for `T-FE-019`/`T-FE-020`/`T-FE-025`.
- closure_evidence: Approval record is complete and suitable for `T-FE-017` execution; subtask closed as `VERIFIED` subject to OpenAI final gate. No generation, install, or integration was performed.

### `GATE-FE-T016`

- status_result: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`
- evidence_summary: Generator approval record exists with exact version, configuration, locations, invocation, and policies sufficient for `T-FE-017` isolated generation without guessing and without manual edits.
- closure_evidence: Gate is closed as `VERIFIED` subject to OpenAI final gate; `T-FE-017` is currently `BLOCKED`/paused after the failed `services: true` attempt, with approved resume path `services: false`, and must enforce clean regeneration, determinism/drift, build, and no-manual-edit gates.

### `T-FE-017` — Generate isolated API client

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-08
- scope_summary: Completed isolated generated-client task after `T-FE-016` approval and the later technical-lead correction approval for `services: false`. Added exact approved `ng-openapi-gen@1.0.5` devDependency declaration, `generate:api` script, dependency-policy entry, package-lock resolution, generated output under `frontend/src/app/core/api/generated/`, and generated-directory ESLint ignore. No generated file was manually edited; no backend/OpenAPI mutation, API infrastructure, app feature implementation, custom template, staging outside the final atomic commit flow, push, or history rewrite occurred.
- orchestration_note: Muse and Big Pickle worker attempts hit permission limits (`node --version` / `npm install` denied). OpenAI performed the direct continuation under the model-orchestration permission-blockage exception because approved non-OpenAI routes could not complete the required dependency/install/generation commands; this continuation did not re-investigate routing.
- generation_summary: `npm run generate:api` completed twice from `frontend/` using canonical input `../docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` and authoritative `--services false --promises false --module false --index-file true --enum-style alias --use-temp-dir true`. Output contains `258` TypeScript files under the approved generated directory. Aggregate checksum after both generations was identical: `ba8235601f2ae6084bdca072d3cdbb8e6e34a205190325a40a49a41e49c48827`.
- contract_evidence: Generated output has no `services/` directory and no `services.ts`; function-based client output is present; Problem Details variants are generated; CV multipart is generated as `file: Blob`; nullable unions are present; generated code does not embed Authorization/Bearer token ownership; and generated files were not manually edited.
- verification_summary: `npm install` completed up to date with 0 vulnerabilities. Focused contract evidence passed. `npm test -- --watch=false` passed 4 files / 22 tests, `npm run lint` passed, and `npm run lint:styles` passed before this final checkpoint. Final gate verification then ran `npm run quality`, which passed dependency policy, Angular lint, Stylelint, non-watch tests (4 files / 22 tests), and production build. `git diff --check` was clean. `git diff --name-only` listed the tracked T-FE-017/doc files, while `git status --short` also showed the approved untracked generated API directory before staging.
- exclusion_summary: `T-FE-018` was not started. No API base/config runtime wiring, interceptors, adapters, guards, component/feature code, backend source, canonical OpenAPI, Penpot/design source, custom template, push, or history rewrite occurred.
- closure_evidence: Deterministic generation, generated-client contract evidence, full quality verification, diff whitespace check, and scope review are sufficient for OpenAI final gate; `T-FE-017` is closed as `VERIFIED` subject to the authorized atomic commit.

### `ST-FE-017` — Generate isolated client and drift check

- status: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION`
- evidence_summary: Clean regeneration with approved `services: false` completed twice from the canonical OpenAPI snapshot. Both runs produced `258` TypeScript files with aggregate SHA-256 `ba8235601f2ae6084bdca072d3cdbb8e6e34a205190325a40a49a41e49c48827`; generated output remains isolated under `frontend/src/app/core/api/generated/` and contains no service facades.
- closure_evidence: Subtask accepted as complete from deterministic regeneration, contract evidence, full frontend quality verification, and scope checks.

### `GATE-FE-T017`

- status_result: `VERIFIED`
- blocker_types: `TOOLING_APPROVAL`,`CONTRACT_CLARIFICATION`
- evidence_summary: Gate passed after approved `services: false` regeneration. Evidence covers exact pinned generator dependency/script/policy, isolated tracked generated output, two-run deterministic checksum (`258` TypeScript files, SHA-256 `ba8235601f2ae6084bdca072d3cdbb8e6e34a205190325a40a49a41e49c48827`), no generated service directory, function-based client output, Problem Details variants, CV multipart `file: Blob`, nullable unions, no generated Authorization/Bearer ownership, no manual generated edits, `npm run quality` pass, and clean `git diff --check`.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-018` is now the next API-infrastructure dependency path but must not start in this continuation and still requires its own eligible-task authorization and scope gate.

### `T-FE-018` — API base/config

- status: `VERIFIED`
- blocker_types: `RUNTIME_DEPLOYMENT`
- evidence_date: 2026-09-08
- scope_summary: Implemented the approved API base/config foundation only. Added a non-generated Core API config boundary in `frontend/src/app/core/api/api-config.ts`, focused behavior/config tests in `frontend/src/app/core/api/api-config.spec.ts`, Angular dev proxy config `frontend/proxy.conf.json`, app provider wiring in `frontend/src/app/app.config.ts`, and dev-server proxy wiring in `frontend/angular.json`. No generated files, backend source, canonical OpenAPI, package dependencies, Angular environment files, bearer/auth logic, Problem Details mapping, adapters, feature UI, T-FE-019, staging beyond the final atomic flow, push, or history rewrite occurred.
- runtime_config_summary: The API prefix constant is exactly `/api/v1`. Generated operation paths already include `/api/v1`, so the generated `ApiConfiguration.rootUrl` is supplied as `''` by default for same-origin relative API calls. Optional future public `apiOrigin` is normalized to origin-only by trimming trailing slashes; generated `RequestBuilder` then composes `rootUrl + generated operation path`, avoiding double prefixes and double slashes without editing generated output.
- proxy_summary: Angular dev-server proxy config maps exactly `/api/v1` to the repository-evidenced backend local target `http://localhost:5167` from `backend/src/NursingPlatform.WebApi/Properties/launchSettings.json`. No additional proxy routes are defined and no `pathRewrite` is configured, preserving the request path.
- tdd_summary: Initial focused TDD red test failed before `api-config.ts` existed. OpenAI final gate then identified a material double-prefix risk in the first implementation because generated paths already include `/api/v1`; a targeted Muse correction updated tests to prove generated `RequestBuilder` composition and the red check failed with `/api/v1/api/v1/exams` versus `/api/v1/exams`. The corrected implementation supplies generated root URL `''` by default and origin-only for configured origins; focused green verification passed 1 file / 11 tests.
- verification_summary: Worker verification passed `npm test -- --watch=false --include src/app/core/api/api-config.spec.ts` (1 file / 11 tests), `npm test -- --watch=false` (5 files / 33 tests), `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty `git diff --cached --name-status`, and final `git status --short` showing only the orchestrator-owned `PROGRESS.md` plus T-FE-018 implementation files before final evidence docs.
- closure_evidence: OpenAI final gate accepted the corrected generated-client integration boundary: generated files remain untouched, generated `ApiConfiguration` is provided through the generated provider, same-origin calls use generated paths directly, local development uses Angular proxy, public config has no credential/secret fields, and T-FE-019 remains not started. T-FE-018 is closed as `VERIFIED` subject to the authorized atomic commit.

### `ST-FE-018` — Configure API base URL/proxy/public config

- status: `VERIFIED`
- blocker_types: `RUNTIME_DEPLOYMENT`
- evidence_summary: Configures API prefix `/api/v1`, default same-origin generated root URL `''`, optional public origin normalization to origin-only, generated `ApiConfiguration` DI through a non-generated provider, and Angular dev proxy `/api/v1` -> `http://localhost:5167` with path preservation.
- closure_evidence: Focused behavior tests and full frontend quality verification passed; generated files were not edited.

### `GATE-FE-T018`

- status_result: `VERIFIED`
- blocker_types: `RUNTIME_DEPLOYMENT`
- evidence_summary: Gate evidence covers config tests for prefix/root/origin composition, generated `RequestBuilder` URL composition, generated `ApiConfiguration` DI, no public credential/secret fields, proxy target/path-preservation assertion, full frontend test/lint/style/quality/build verification, and git scope checks.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-019` must not start without its own authorization/eligibility and remains outside this run.

### `T-FE-042` — Registration contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Completed local source/OpenAPI inspection only for registration auth/public behavior. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- contract_summary: `POST /api/v1/auth/register` maps to operation/name `RegisterUser`, accepts `RegisterUserRequest` with `email`, `password`, `firstName`, `lastName`, and `roleIds`, sends `RegisterUserCommand`, and returns `200 OK` with `RegisterUserResponse { userId }` from backend source/tests. Runtime authorization is permission-protected through `RequirePermission(Permissions.Users.Create)`, not public: unauthenticated requests return `401`, authenticated requests without `Users.Create` return `403`, and authorized requests return `200`.
- validation_summary: Application validator requires non-empty valid email, password non-empty/min length 8/uppercase/digit, first/last name non-empty with max length 100, non-empty role IDs, and no duplicate role IDs.
- openapi_alignment: Canonical OpenAPI records `POST /api/v1/auth/register`, operationId `RegisterUser`, request schema `RegisterUserRequest`, `200`, `401`, and Bearer security. OpenAPI omits the runtime `403` permission response and omits the `200` response body schema; backend source and tests establish those details for frontend planning.
- verification_summary: Targeted read-only inspection covered `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`, registration request/command/validator/response files, `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/RegisterEndpointTests.cs`, and canonical OpenAPI `development-openapi-2026-09-03.json` registration operation/schema.
- closure_evidence: Local contract clarification evidence is deterministic and sufficient; `GATE-FE-T042` is closed as `VERIFIED`.

### `ST-FE-042` — Clarify RegisterUser auth/public behavior

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Registration is permission-protected by `Users.Create`, not anonymous/public. Source/tests prove `401` unauthenticated, `403` authenticated without permission, and `200` authorized with `RegisterUserResponse { userId }`. OpenAPI metadata omissions are recorded.
- closure_evidence: Subtask accepted as complete from local source/OpenAPI inspection evidence.

### `GATE-FE-T042`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Register clarification evidence is recorded: backend endpoint mapping, permission requirement, request/response contract, validation constraints, integration-test authorization behavior, and OpenAPI metadata comparison.
- closure_evidence: Gate is closed as `VERIFIED`; downstream registration screen tasks remain separately blocked by their own dependencies, design/screen approval gates, and form-validation prerequisites.

### `T-FE-046` — Verification email contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Completed local source/OpenAPI inspection only for send-verification-email and verify-email behavior. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- contract_summary: `POST /api/v1/auth/send-verification-email` maps to operation/name `SendVerificationEmail`, has no request body, is authenticated-only via `RequireAuthorization()`, returns `401` without a bearer token, and returns `200 OK` with `SendVerificationEmailResponse { message }` when authenticated. `POST /api/v1/auth/verify-email` maps to operation/name `VerifyEmail`, accepts `VerifyEmailRequest { token }`, is anonymous/public via `AllowAnonymous()`, and returns `200 OK` with `VerifyEmailResponse { message }` without exposing the raw token in success JSON.
- validation_error_summary: `VerifyEmailCommandValidator` requires non-empty `Token`; validation exceptions map to `400 Validation failed`. Send-verification unauthenticated/missing/inactive user throws `UnauthorizedAccessException` and maps to `401`. Send mail failure and invalid/used/expired verification tokens throw `InvalidOperationException` and map to `409 Conflict` through the current middleware.
- openapi_alignment: Canonical OpenAPI records `SendVerificationEmail` with Bearer security, `200`, and `401`, and records `VerifyEmail` with request schema and `200`. OpenAPI omits source/test response schemas for both endpoints, omits send-verification `409`, and omits verify-email validation/conflict error statuses; backend source and tests establish those details for frontend planning.
- verification_summary: Targeted read-only inspection covered `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`, send/verify request/command/validator/handler/response files, `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/SendVerificationEmailEndpointTests.cs`, `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/VerifyEmailEndpointTests.cs`, `backend/src/NursingPlatform.WebApi/Middleware/ExceptionMiddleware.cs`, and canonical OpenAPI `development-openapi-2026-09-03.json` send/verify operations/schemas.
- closure_evidence: Local contract clarification evidence is deterministic and sufficient; `GATE-FE-T046` is closed as `VERIFIED`.

### `ST-FE-046` — Clarify send/verify email behavior

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Send-verification-email is authenticated-only and returns `{ message }`; verify-email is anonymous/public, requires `token`, returns `{ message }`, maps empty token to `400`, maps invalid/used/expired token to `409`, and must not expose the raw token in success JSON. OpenAPI metadata omissions are recorded.
- closure_evidence: Subtask accepted as complete from local source/OpenAPI inspection evidence.

### `GATE-FE-T046`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Verification-email clarification evidence is recorded: backend endpoint mapping, auth requirements, request/response contracts, validation/error behavior, integration-test behavior, and OpenAPI metadata comparison.
- closure_evidence: Gate is closed as `VERIFIED`; downstream email-verification screen tasks remain separately blocked by their own dependencies, design/screen approval gates, and visual evidence prerequisites.

### `T-FE-054` — Account inactive contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Completed local source/OpenAPI inspection only for login/current-user inactive account state. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- contract_summary: No stable coded inactive-account contract exists for login or current-user. `POST /api/v1/auth/login` is anonymous/public via `AllowAnonymous()` and maps inactive users to generic `401 Unauthorized` with message `Invalid credentials.`, indistinguishable from missing user or wrong password. `GET /api/v1/me` is authenticated via `RequireAuthorization()` and does not reject inactive users in the source handler; `isActive` is a passive field in the `200 OK` `UserDetailDto` projection.
- error_summary: `UnauthorizedAccessException` maps to plain `401` Problem Details without a `code` field. `GET /api/v1/me` maps missing current-user identity to `401` and missing user row to `404`; no inactive-specific status or coded problem details are used for login/current-user.
- openapi_alignment: Canonical OpenAPI records login request and `200` only, and records `/api/v1/me` `200`/`401` Bearer only. OpenAPI omits login errors/response schema and `/me` response schema/`404`; backend source/tests establish the runtime details. No unresolvable source/OpenAPI conflict was found.
- verification_summary: Canonical Muse scout plus OpenAI final review inspected login/current-user endpoint mappings, login/current-user handlers and DTOs, exception middleware/problem details contracts, application and WebApi tests, domain `User.IsActive`, canonical OpenAPI login/`/me` sections, and negative searches for inactive/account-inactive coded contracts.
- closure_evidence: Local account-inactive clarification evidence is deterministic and sufficient; `GATE-FE-T054` is closed as `VERIFIED`. `T-FE-055` / `AUTH-012 Account Inactive` remains blocked by `BACKEND`, `CONTRACT_CLARIFICATION`, and `DESIGN` until a stable backend inactive-account contract and screen approval exist.

### `ST-FE-054` — Clarify account inactive coded state

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: No coded inactive-account state exists. Login inactive uses generic invalid-credentials `401`; current-user exposes passive `isActive` but does not reject inactive users. OpenAPI metadata omissions are recorded.
- closure_evidence: Subtask accepted as complete from local source/OpenAPI inspection evidence.

### `GATE-FE-T054`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Account-inactive clarification evidence is recorded: login/current-user endpoint mapping, auth requirements, runtime inactive behavior, error mapping, tests, negative coded-contract search, and OpenAPI metadata comparison.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-055` remains backend/design blocked and must not implement Account Inactive UX from generic `401` or passive `isActive` alone.

### `T-FE-063` — CV contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected CV source/tests/OpenAPI for local clarification. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- deterministic_source_summary: Backend source/tests establish `/api/v1/me/nurse-profile/cv` GET/POST/DELETE under authenticated nurse-profile scope, POST multipart field `file`, allowed content types `application/pdf`, `application/msword`, and `application/vnd.openxmlformats-officedocument.wordprocessingml.document`, allowed extensions `.pdf`, `.doc`, `.docx`, max file size `5 * 1024 * 1024` bytes, metadata DTO response for GET/POST, replace-on-upload semantics, no file-bytes download endpoint, and runtime handling for validation/authorization/not-found conflicts.
- openapi_correction: Technical lead authorized canonical OpenAPI correction after re-confirming backend source, endpoint metadata, success result, and automated tests. The affected operation only, DELETE `/api/v1/me/nurse-profile/cv`, was corrected from `200 OK` to `204 No Content` and committed via `7f07b26 docs(frontend): correct nurse CV OpenAPI delete status` (scope: `docs/frontend/design/integration/openapi/development-openapi-2026-09-03.json` only); OpenAPI JSON validation passed and diff was limited to that operation. No backend runtime change.
- closure_evidence: CV contract clarification is deterministic and sufficient after OpenAPI correction committed via `7f07b26`; `GATE-FE-T063` is closed as `VERIFIED`.

### `ST-FE-063` — Clarify CV file constraints

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Source/test file constraints were deterministically identified, and canonical OpenAPI now matches runtime/test DELETE `204 NoContent` after the authorized operation-only correction.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T063`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: CV constraints evidence is recorded and canonical OpenAPI now matches backend source/tests for DELETE success status (`204 NoContent`).
- closure_evidence: Gate is closed as `VERIFIED` after the authorized operation-only OpenAPI correction committed via `7f07b26`; downstream CV management remains separately subject to its own dependencies, screen approval, upload pattern gate, and visual evidence requirements.

### `T-FE-066` — Exam catalog/detail contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected exam catalog/detail source/tests/OpenAPI. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- contract_summary: `GET /api/v1/exams` maps to `ListExams` and returns `PaginatedResult<ExamCatalogItemDto>`; `GET /api/v1/exams/{id}` maps to `GetExam` and returns `ExamDetailDto`. Both endpoints use `RequireAuthorization()` only, with no permission requirement. List parameters are `page`, `pageSize`, `countryId`, and `categoryId`; validators enforce page `>= 1` and page size `1..100`.
- field_summary: Catalog items expose `id`, `title`, `description`, `countryId`, `countryName`, `categoryId`, `categoryName`, `durationMinutes`, `questionCount`, `passingScorePercentage`, `isFree`, and `canStart`. Detail adds `instructions`. Access/entitlement presentation fields are limited to `isFree` and `canStart`; no entitlement IDs, product pricing, attempt/session/report rights, questions, answers, correctness, rationales, explanations, or answer keys are exposed by catalog/detail DTOs.
- behavior_summary: Catalog filters to published exams with latest published versions and only returns startable items (`canStart` true); paid exams without a valid grant are omitted from the list while detail may return `isFree: false` and `canStart: false`. Ordering is deterministic by country/category/title/id; pagination applies after filtering.
- openapi_alignment: Canonical OpenAPI records routes, operationIds, parameters, Bearer security, and `200`/`401`; it omits response schemas and runtime `400`/`404` metadata. Backend source/tests provide deterministic source-authoritative schemas and behavior; no unresolvable backend/OpenAPI conflict was found.
- closure_evidence: Exam catalog/detail clarification evidence is deterministic and sufficient; `GATE-FE-T066` is closed as `VERIFIED`.

### `ST-FE-066` — Clarify exam catalog/detail response schemas and access fields

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Source/test-backed catalog/detail DTO fields, auth requirements, list filters, pagination behavior, access flags, sensitive-content non-exposure, and OpenAPI metadata omissions are recorded.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T066`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Exam catalog/detail schema clarification evidence is recorded: routes, authorization, request parameters, DTO fields, pagination/access behavior, status/error handling, sensitive-content boundary, tests, and OpenAPI metadata comparison.
- closure_evidence: Gate is closed as `VERIFIED`; downstream exam screens remain separately subject to route/pagination/screen approval and visual evidence gates.

### `T-FE-080` — PP material reader classification

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected preparation-package learner material reader/download/delivery contracts and admin material-management contracts. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: No contract-backed frontend material reader can be implemented now. Backend/OpenAPI expose nurse preparation-package entitlement, exam-session, practice-progress, answer submission, and report endpoints, but no learner material read/download/delivery endpoint. Admin study-material endpoints exist only for authoring/management and are protected by `StudyMaterials.Manage`; they do not authorize nurse learner delivery.
- evidence_summary: Canonical OpenAPI contains exactly the nurse preparation-package entitlement/exam/practice/report paths and admin preparation-package material-management paths, with no nurse material/content/download path. Backend tests explicitly assert attempted nurse/anonymous material content route returns `404`.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. Downstream material-reader UI remains out of scope until backend provides a learner material delivery contract and screen approval.

### `ST-FE-080` — Classify material reader backend availability

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Learner material read/download/delivery contract is absent; admin authoring endpoints and entitlement material IDs are not reader contracts.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T080`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Material-reader classification evidence is recorded: learner route absence, admin authoring distinction, entitlement DTO limitations, negative route test, OpenAPI consistency, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; no material reader is implemented.

### `T-FE-093` — Candidate detail contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse scout plus targeted OpenAI read-only review inspected recruitment candidate source/tests/OpenAPI. No screen implementation, frontend application code, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: Employer candidate search/list exists, but candidate detail is a backend gap. Source/OpenAPI expose `GET /api/v1/recruitment/candidates` only for candidates; no `CandidateDetail`, `GetCandidateDetail`, `GetCandidateById`, or `/api/v1/recruitment/candidates/{id}` route/DTO/test exists.
- evidence_summary: Candidate list response uses source DTO `CandidateListItemDto` and tests verify sensitive fields are not exposed in list JSON. Canonical OpenAPI records only the list path and parameters; grep for `recruitment/candidates/` returns no detail path.
- closure_evidence: Classification is complete and verified as backend gap / no detail implementation. `EMP-005` and downstream candidate profile/request work remain blocked until backend provides a candidate detail contract and screen approval.

### `ST-FE-093` — Clarify candidate detail availability

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`BACKEND`
- evidence_summary: Candidate detail route/DTO is absent; only candidate search/list is contract-backed.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T093`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`BACKEND`
- evidence_summary: Candidate detail classification evidence is recorded: source/OpenAPI route absence, list-only DTO evidence, negative detail-route search, sensitive-field list tests, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; candidate detail screen work remains blocked.

### `T-FE-099` — ACC-003 change password classification

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout plus targeted OpenAI read-only review inspected auth/account/me source/tests/OpenAPI for authenticated account change-password availability. No frontend implementation, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: ACC-003 authenticated change-password is a backend gap. Source/OpenAPI expose anonymous forgot/reset password flows, but no authenticated user change-password route/command/request exists.
- evidence_summary: `ApplicationBuilderExtensions` maps `/api/v1/auth/forgot-password` and `/api/v1/auth/reset-password` as anonymous auth recovery flows and maps `/api/v1/me` as authenticated current-user read only. `ResetPasswordRequest` requires `email`, `token`, and `newPassword`, proving reset-by-token rather than current-password change. Source grep found no `ChangePassword`, `CurrentPassword`, `/me/password`, `/account/password`, `change-password`, or `update-password` backend endpoint/command. Canonical OpenAPI contains `/api/v1/auth/reset-password` but no change-password/current-password route.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. `ACC-003` remains blocked until backend provides an authenticated change-password contract and account screen approval.

### `ST-FE-099` — Classify `ACC-003` Change Password

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Authenticated current-user change-password contract is absent; token-based reset-password is not an ACC-003 change-password contract.
- closure_evidence: Subtask accepted as complete from source/OpenAPI evidence.

### `GATE-FE-T099`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Change-password classification evidence is recorded: anonymous reset route distinction, authenticated `/me` read-only route, negative backend route/command search, OpenAPI route absence, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; change-password UI remains blocked.

### `T-FE-100` — ACC-004 sessions classification

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected auth/account/session/security/device source, tests, and canonical OpenAPI for account security/session-management availability. No frontend implementation, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: ACC-004 account security/sessions/device-management is a backend gap. No stable coded backend/OpenAPI contract exists for active sessions, user-visible session detail, session revocation, logout-all/revoke-all, trusted devices, device list/management, MFA, or account-security preferences.
- evidence_summary: `ApplicationBuilderExtensions` maps only auth primitives (`login`, `refresh`, `register`, `send-verification-email`, `verify-email`, `forgot-password`, `reset-password`) plus authenticated `GET /api/v1/me`; it does not map account session/device/security routes. `GetCurrentUser` returns identity/status/role/permission fields only. Refresh-token persistence/rotation/reuse revocation is server-internal token hygiene, not a user-visible session-management API. Canonical OpenAPI contains auth and `/api/v1/me` operations plus exam-session domain routes, but no `/api/v1/me/sessions`, `/api/v1/me/devices`, `/api/v1/me/security`, `/api/v1/account/*session*`, logout, revoke, active-sessions, trusted-devices, MFA, or account-security schema.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. `ACC-004` remains blocked until backend provides a session/security/device-management contract and account screen approval. Do not infer ACC-004 UX from refresh tokens, JWT claims, `/me` passive fields such as `LastLoginAt` or `IsActive`, generic `401/403`, or exam-session routes.

### `ST-FE-100` — Classify `ACC-004` Security/Sessions

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Account security/session/device-management contract is absent; refresh-token internals and exam-session routes are not ACC-004 contracts.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test search evidence.

### `GATE-FE-T100`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Sessions/security classification evidence is recorded: auth endpoint map limitation, current-user DTO limitation, internal-only refresh-token hygiene distinction, negative backend route/command/DTO/test search for session/device/security management, OpenAPI route/schema absence, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; ACC-004 account security/sessions UI remains blocked.

### `T-FE-101` — ACC-005 notification preferences classification

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical free-worker read-only scouting inspected account/current-user notification preference contracts in canonical OpenAPI and backend source/tests. No frontend implementation, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: ACC-005 notification preferences is a backend gap. No stable coded backend/OpenAPI contract exists for account/current-user notification preferences, account settings, communication preferences, email/SMS/push preferences, or notification subscription management.
- evidence_summary: Canonical OpenAPI has no `notification`, `preference`, `preferences`, `email notification`, `sms`, `push`, `communication`, `settings`, `NotificationPreference`, `AccountSetting`, or related schema/path hits. Auth paths are limited to login, refresh, register, verification-email, verify-email, forgot-password, and reset-password; `/api/v1/me*` paths cover current user and domain features such as nurse/employer profile, preparation packages, exam analytics/attempts, orders, contact requests, and CV, with no notification-preferences route. Backend searches found no `Preference` or `NotificationPreference` source/test contracts. The only notification-related backend code is transactional email sending (`IEmailService` / `EmailService`) for verification and password reset, not user-configurable preferences. WebApi source/tests contain no notification/preference/account-settings endpoint evidence.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. `ACC-005` remains blocked until backend provides a notification-preferences contract and account screen approval. Do not infer ACC-005 UX from `EmailVerified`, roles, permissions, identity fields, JWT claims, marketing copy, transactional email services, or screen-matrix approval.

### `ST-FE-101` — Classify `ACC-005` Notification Preferences

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Notification preferences/account settings contract is absent; transactional verification/reset email infrastructure is not a user preference contract.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test-search evidence, with final after-inspection git status supplied by the OpenAI final gate as a targeted missing-evidence check.

### `GATE-FE-T101`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Notification-preferences classification evidence is recorded: OpenAPI route/schema term absence, auth/current-user path limitation, backend source/test negative preference searches, transactional-email distinction, final git status no task mutation, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; ACC-005 notification preferences UI remains blocked.

### `T-FE-102` — ACC-006 account status classification

- status: `VERIFIED`
- blocker_types: `BACKEND`,`CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Approved free-worker read-only scout inspected account-status/login/current-user state contracts in canonical OpenAPI and backend source/tests, explicitly comparing against the already verified T-FE-054 inactive-account classification. No frontend implementation, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: ACC-006 account status is a backend gap. No stable coded account-status contract exists beyond passive current-user fields and generic inactive login/refresh rejection. There is no account-status enum/schema, coded inactive/suspended/locked status response, user activation/deactivation/suspension/lock management route, or account-status workflow contract.
- evidence_summary: Canonical OpenAPI exposes login, refresh, current-user, and admin user read routes but no account-status route/schema; login documents request fields only and no coded account-status error, while `/api/v1/me` has no schema metadata in the snapshot. Backend `User` has `IsActive`, `EmailVerified`, and `LastLoginAt` fields but no status enum. Login rejects missing or inactive users with the same generic `UnauthorizedAccessException("Invalid credentials.")`; refresh rotation rejects inactive users with generic invalid-refresh behavior. `GetCurrentUser` returns passive identity/status fields and does not reject inactive users. Exception mapping turns `UnauthorizedAccessException` into generic `401` without a stable status code. Tests cover inactive login as generic unauthorized and current-user DTO projection, with no inactive/status workflow test.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. `ACC-006` remains blocked until backend provides a stable account-status contract and account screen approval. Do not infer account-status UX from generic login `401`, passive `/me` fields such as `IsActive` or `LastLoginAt`, JWT claims, roles/permissions, admin-only `isActive` filtering, or frontend screen-matrix approval.

### `ST-FE-102` — Classify `ACC-006` Account Status

- status: `VERIFIED`
- blocker_types: `BACKEND`,`CONTRACT_CLARIFICATION`
- evidence_summary: Account-status workflow contract is absent; passive `/me` fields and generic inactive login/refresh rejection are not sufficient ACC-006 implementation contracts.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T102`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`,`CONTRACT_CLARIFICATION`
- evidence_summary: Account-status classification evidence is recorded: T-FE-054 consistency, OpenAPI route/schema limitations, backend `User.IsActive` passive field distinction, generic login/refresh unauthorized behavior, exception mapping without coded status, tests proving generic inactive login handling, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; ACC-006 account status UI remains blocked.

### `T-FE-103` — ADM-001 dashboard classification

- status: `VERIFIED`
- blocker_types: `BACKEND`,`DESIGN`
- evidence_date: 2026-09-07
- scope_summary: Approved free-worker read-only scouting inspected admin dashboard/metrics/summary/landing data contracts in canonical OpenAPI and backend source/tests. No ADM-001 UI implementation was authorized because `GATE-FE-T098` is not verified. No frontend implementation, backend/OpenAPI mutation, dependency/tooling change, staging, commit, or push occurred.
- classification_summary: ADM-001 admin dashboard/static landing is a backend gap for data-backed dashboard behavior. No stable coded backend/OpenAPI admin dashboard, metrics, statistics, overview, summary, analytics, chart, alert, activity, revenue, orders, users, exams, package, or recruitment aggregate contract exists.
- evidence_summary: Canonical OpenAPI dashboard/metrics/statistics/overview/chart/landing searches found only nurse-owned exam analytics under `/api/v1/me/nurse-profile/exam-analytics/*` and a public preparation-package catalog component summary DTO, neither of which is an admin dashboard contract. Admin OpenAPI paths are CRUD/list contracts for users, exam categories/exams, payment products, preparation-package administration, and recruitment candidate search; no `/api/v1/admin/dashboard`, metrics, overview, summary, statistics, analytics, or landing route exists. Backend source searches found no `Dashboard`, `Metrics`, `Statistics`, or `Overview` Application/Domain/Infrastructure contracts, and no admin dashboard WebApi group. Analytics source is nurse-owned `My*` exam analytics guarded to nurse profiles. Backend tests have no admin dashboard/metrics tests; existing dashboard strings are negative 404 scope guards for a non-existent nurse preparation-package workspace/dashboard route.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. `ADM-001` remains blocked until backend provides a dashboard/landing contract and Administration screen approval (`GATE-FE-T098`) is verified. Do not infer dashboard metrics from admin CRUD/list endpoints, nurse exam analytics, preparation-package catalog summaries, database table counts, revenue/payment/order lists, recruitment lists, or design expectations.

### `ST-FE-103` — Classify/build `ADM-001` dashboard/static landing

- status: `VERIFIED`
- blocker_types: `BACKEND`,`DESIGN`
- evidence_summary: Classification branch completed: admin dashboard/metrics contract is absent; UI build branch remains unauthorized because `GATE-FE-T098` is not verified.
- closure_evidence: Subtask accepted as complete for backend-gap classification from source/OpenAPI/test-search evidence.

### `GATE-FE-T103`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`,`DESIGN`
- evidence_summary: Admin dashboard classification evidence is recorded: OpenAPI absence of admin dashboard/metrics routes, distinction from nurse exam analytics and public package summary DTOs, backend negative dashboard/metrics contract searches, negative admin dashboard test searches, explicit no-UI relationship to unverified `GATE-FE-T098`, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; ADM-001 dashboard/static landing UI remains blocked.

### `T-FE-105` — ADM-004 roles/permissions classification

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected role/permission management contracts in canonical OpenAPI and backend identity/authorization source/tests. No frontend implementation, security/authorization policy decision, backend/OpenAPI mutation, dependency/tooling change, screen approval, staging, commit, or push occurred.
- classification_summary: ADM-004 roles/permissions management is a backend gap. No stable coded backend/OpenAPI contract exists for role CRUD, permission CRUD, role/permission listing, role assignment, permission assignment, assign/revoke operations, or role/permission management DTOs/commands/queries.
- evidence_summary: Canonical OpenAPI exposes `/api/v1/me`, `GET /api/v1/users`, and `GET /api/v1/users/{id}`, but no `/api/v1/roles`, `/api/v1/permissions`, `ListRoles`, `RoleDto`, `PermissionDto`, assign, revoke, or role/permission management operations. Backend `Permissions.cs` defines `Roles.View`, `Roles.Manage`, `Permissions.View`, and `Permissions.Manage` constants and reference-data seeding creates those permissions, but they are policy vocabulary/seeds only. WebApi user routes are protected by `Users.View`; no role/permission management route uses `Roles.Manage` or `Permissions.Manage`. Application source contains runtime authorization services and identity read projections only, not role/permission management commands/queries/DTOs. Existing `/me` and user detail/list role/permission fields are passive read-model projections.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. `ADM-004` remains blocked until backend provides explicit role/permission management contracts and Administration screen approval (`GATE-FE-T098`) is verified. Do not infer management APIs from permission constants, seeded permissions, `RequirePermission` metadata, JWT claims, `/me` roles/permissions, user list/detail projections, or `Permissions.All/Admin` arrays.

### `ST-FE-105` — Classify `ADM-004` Roles/Permissions

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Roles/permissions management contract is absent; constants, seeds, authorization metadata, and passive user projections are not management APIs.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test-search evidence.

### `GATE-FE-T105`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Role CRUD classification evidence is recorded: OpenAPI absence of role/permission management routes/schemas, backend distinction between constants/seeds/projections and management contracts, negative route/command/query/DTO/test searches, no security policy decision, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; ADM-004 roles/permissions UI remains blocked.

### `T-FE-081` — Payment product contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected payment product list/detail contracts in canonical OpenAPI, API documentation, backend endpoint mappings, Application DTO/query/mapping code, domain model, and existing tests. No payment production/provider/release decision, frontend implementation, backend/OpenAPI mutation, dependency/tooling change, screen approval, staging, commit, or push occurred.
- classification_summary: Payment product catalog list/detail has a stable coded source-backed contract, with OpenAPI metadata omissions recorded. Classification: `FRONTEND_PLAN_CORRECT` with `OPENAPI_METADATA_DEFECT` annotation, because source/tests establish DTOs/errors/filters while canonical OpenAPI omits response schemas and some error statuses.
- evidence_summary: Source routes are `GET /api/v1/payment/products` and `GET /api/v1/payment/products/{id:guid}`, both `.RequireAuthorization()` only with no permission or nurse-role requirement. List accepts `page`, `pageSize`, and optional `examId`, validates page bounds, returns `PaginatedResult<PaymentProductDto>`, filters to active products for published exams, supports optional exam filtering, and orders deterministically by `Name, Id`. Detail filters active products for published exams and maps missing/inactive/unpublished to `404`. `PaymentProductDto` exposes catalog fields only: `id`, `type`, `examId`, `examTitle`, `name`, optional `description`, normalized uppercase `currency`, `unitAmountMinor` serialized as string, `isActive`, `createdAt`, and `updatedAt`. Existing tests prove authenticated-only access, no permission-service/nurse-role requirement, active+published visibility, pagination validation, malformed GUID `400`, not-found behavior, Problem Details, and sensitive/internal field non-exposure.
- openapi_metadata_notes: Canonical OpenAPI records the two payment product routes, operationIds, Bearer security, list query params, detail path param, and `200/401` responses, but omits `200` response schemas, `400` validation/malformed-GUID responses, `404` detail response, and optional/nullability detail for `examId`. No OpenAPI mutation was authorized or performed.
- closure_evidence: Clarification is complete and verified for planning. Downstream payment product screens remain separately blocked by their route/screen/design dependencies. Do not infer order creation, checkout, provider, webhook, refund, subscription, fulfillment, entitlement, or production payment support from the catalog contract.

### `ST-FE-081` — Clarify payment product schemas

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Payment product list/detail source-backed DTO, filters, auth, statuses, exposure boundaries, and OpenAPI metadata omissions are recorded.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T081`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Product schema evidence is recorded: routes, authorization, list params, source DTO fields, active/published visibility behavior, detail not-found behavior, Problem Details/error metadata, sensitive-field boundary, OpenAPI omissions, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for contract clarification; payment product UI remains subject to downstream task/screen approval gates.

### `T-FE-083` — Payment order contract clarification

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected nurse payment order create/list/detail/cancel contracts in canonical OpenAPI, API/frontend authority, backend endpoint mappings, Application DTO/query/command/mapping code, domain order/status model, error middleware, and existing tests. No payment production/provider/release decision, frontend implementation, backend/OpenAPI mutation, dependency/tooling change, screen approval, staging, commit, or push occurred.
- classification_summary: Nurse payment order create/list/detail/cancel has a stable coded source-backed contract, with OpenAPI metadata omissions recorded. Classification: `FRONTEND_PLAN_CORRECT` with `OPENAPI_METADATA_DEFECT` annotation, because source/tests establish list/detail/cancel DTOs/errors/lifecycle while canonical OpenAPI omits response schemas and several error statuses for those operations.
- evidence_summary: Order routes live under authenticated `/api/v1/me/nurse-profile/payment/orders` and require Bearer authentication via the nurse-profile group. `POST /payment/orders` creates a `PendingPayment` order and returns `201 PaymentOrderDto` with `Location`; request has exactly one purchase source (`productId` or `packageOfferId`) and no idempotency key. `GET /payment/orders` returns owned orders as `PaginatedResult<PaymentOrderDto>` with `page`, `pageSize`, and optional `status`; it lazily expires past-due orders, filters by current nurse profile, optional status, orders by `CreatedAt DESC, Id`, and paginates. `GET /payment/orders/{id}` returns an owned order or `404`. `POST /payment/orders/{id}/cancel` cancels owned pending orders, expires past-due orders before cancel, blocks active checkout sessions with `409`, maps foreign/missing orders to `404`, and returns `PaymentOrderDto` on success. DTOs expose order/item/snapshot fields and string-serialized money amounts while tests guard against user, token, provider, webhook, payment-product/order entity, nurse-profile, grant, question, answer, key, rationale, and secret exposure.
- openapi_metadata_notes: Canonical OpenAPI records create with `201 PaymentOrderDto`, `Location`, `400`, `401`, `404`, and `409`, plus schemas for `CreatePaymentOrderRequest`, `PaymentOrderDto`, `PaymentOrderItemDto`, and integer `PaymentOrderStatus`. It records list/detail/cancel paths, Bearer security, and params, but omits `200` schemas for list/detail/cancel and omits some source/test-backed errors such as `400`, `403`, `404`, and `409` on applicable operations. No OpenAPI mutation was authorized or performed.
- closure_evidence: Clarification is complete and verified for planning. Downstream checkout/order UI remains separately blocked by its dependencies and screen approval. Do not infer checkout redirect/success, provider, webhook, refund, reconciliation, subscription, fulfillment, entitlement, grant, sandbox completion, or production payment support from order create/list/detail/cancel.

### `ST-FE-083` — Clarify payment order schemas/lifecycle

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Nurse payment order create/list/detail/cancel source-backed DTOs, lifecycle statuses, validation/conflict behavior, ownership, exposure boundaries, and OpenAPI metadata omissions are recorded.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test evidence.

### `GATE-FE-T083`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Order schema evidence is recorded: routes, authorization, create variants, list/detail/cancel ownership and lifecycle, DTO fields, status values, Problem Details/error mappings, sensitive-field boundary, OpenAPI omissions, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for contract clarification; payment order UI remains subject to downstream task/screen approval gates.

### `T-FE-112` — ADM-009/010 admin orders/recruitment classification

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_date: 2026-09-07
- scope_summary: Canonical Muse read-only scout inspected admin payment-order and admin recruitment management contracts in canonical OpenAPI, API/frontend authority, backend endpoint mappings, permissions, Application payment/recruitment source, and tests. No frontend implementation, backend/OpenAPI mutation, dependency/tooling change, screen approval, payment/security/admin policy decision, staging, commit, or push occurred.
- classification_summary: ADM-009 admin orders and ADM-010 admin recruitment management are backend gaps. No stable coded backend/OpenAPI admin order-management or admin recruitment-management contracts exist.
- evidence_summary: Canonical OpenAPI contains admin payment-product routes only for payment administration, and nurse-owned `/api/v1/me/nurse-profile/payment/orders*` routes for payment orders. It contains employer/search-owned recruitment routes (`/api/v1/recruitment/candidates`, `/api/v1/recruitment/contact-requests*`) and nurse-owned contact-request approve/reject routes, but no `/api/v1/admin/**/orders*`, `/api/v1/admin/recruitment*`, admin recruitment queue, admin candidate detail, moderation, assignment, status workflow, or admin order route. Backend admin endpoint mappings cover exam categories, exams/versions/questions/options, payment products, and preparation-package administration, but no admin payment orders or admin recruitment. `Permissions.cs` has no payments/orders/recruitment admin permission. Source/test searches found no `AdminPaymentOrder`, `AdminOrder`, `AdminRecruitment`, admin order/recruitment DTO/query/command, or admin route tests. Existing nurse payment orders, admin payment products, employer contact requests, nurse contact-request approvals, and candidate search/list are distinct non-admin-management contracts.
- closure_evidence: Classification is complete and verified as backend gap / no implementation. ADM-009/010 remain blocked until backend provides explicit admin order/recruitment contracts and Administration screen approval (`GATE-FE-T098`) is verified. Do not infer admin order management from nurse-owned payment orders or admin payment products, and do not infer admin recruitment management from candidate search/list, employer-owned contact requests, or nurse-owned contact request decisions.

### `ST-FE-112` — Classify `ADM-009/010` admin orders/recruitment

- status: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Admin payment-order and admin recruitment management contracts are absent; non-admin order/recruitment routes and product administration routes are not ADM-009/010 contracts.
- closure_evidence: Subtask accepted as complete from source/OpenAPI/test-search evidence.

### `GATE-FE-T112`

- status_result: `VERIFIED`
- blocker_types: `BACKEND`
- evidence_summary: Admin orders/recruitment classification evidence is recorded: OpenAPI absence of admin order/recruitment routes, backend admin-group limitation, permissions absence, distinction from nurse/employer/candidate-search routes, negative source/test searches, no policy decision, and no implementation performed.
- closure_evidence: Gate is closed as `VERIFIED` for backend-gap classification; ADM-009/010 UI remains blocked.
