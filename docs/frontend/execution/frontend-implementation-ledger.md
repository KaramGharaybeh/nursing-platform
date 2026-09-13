# Frontend Implementation Ledger

This ledger is the authoritative detailed execution record for `GOAL-FE-001` frontend Goal → Milestone → Task → Subtask → Verification Gate work.

It is a governance and tracking document only. It does not authorize implementation by itself. Implementation may proceed only through an explicit task authorization or the Standing Implementation Authorization in this ledger and `docs/development/model-orchestration.md`.

## 1. Authority

Authoritative frontend rules remain in:

- `docs/frontend/frontend-project-rules.md`
- `docs/frontend/frontend-architecture.md`
- `docs/frontend/design/frontend-design-foundation-reference.md`
- approved Storybook visual workflow governance in active frontend architecture/project rules; Storybook tooling is not installed until separately authorized
- approved design specifications under `docs/frontend/design/specs/`
- canonical backend/OpenAPI contracts under `docs/frontend/design/integration/openapi/`

`PROGRESS.md` remains the concise current-state and handoff memory. This ledger is the detailed implementation roadmap authority.

### Execution authority precedence

When frontend sources conflict, use this precedence for implementation execution and status decisions:

1. Current explicit user or technical-lead decisions.
2. Current execution state and task/gate authority in this ledger, with `PROGRESS.md` as the concise current-session handoff.
3. Current frontend architecture and project rules in `docs/frontend/frontend-architecture.md` and `docs/frontend/frontend-project-rules.md`.
4. `docs/frontend/design/frontend-design-foundation-reference.md`, approved visual foundations, approved design specifications, task-required approved Penpot/design evidence, and approved Storybook workflow governance.
5. Canonical backend/OpenAPI authority for API, validation, auth, authorization, security, and business behavior.
6. Historical planning, design inventory, evidence packets, trackers, and older prose.

Historical documents remain preserved as evidence, but they do not override newer verified execution state or current repository facts. Older statements such as “frontend workspace is not initialized” are stale for execution once the ledger and repository evidence show a later verified state. If the above precedence cannot resolve a conflict deterministically, STOP and escalate instead of guessing.

### Standing Implementation Authorization

Standing Implementation Authorization replaces the need for a separate user “start this task” message only for ordinary eligible Low/Medium frontend implementation Tasks where **all** of the following are true:

- every predecessor Gate required by the Task is `VERIFIED`;
- exact Task scope and expected file boundaries are established from current repository authority;
- no unresolved `DESIGN`, `BACKEND`, `CONTRACT_CLARIFICATION`, `SECURITY`, `DEPENDENCY`, `TOOLING_APPROVAL`, `SCOPE`, `EXTERNAL`, or runtime-deployment decision requires human authority;
- no missing screen-family approval, per-screen `APPROVED` decision, required Penpot/design approval, required Storybook visual evidence, or visual/source decision is required;
- no dependency installation/change, database change, migration, backend/OpenAPI mutation, Penpot mutation, Storybook installation/configuration, CI/tooling approval, or external-provider decision is required;
- Task risk is Low or Medium under `docs/development/model-orchestration.md`;
- implementation can stay inside a bounded `ALLOWED_FILES` packet;
- focused tests/source-contract tests and the Gate's full deterministic evidence can be produced with existing approved tooling.

When those conditions are satisfied, the OpenAI Orchestrator may select the next eligible Task from the DAG, delegate repository-heavy exploration and implementation to the approved non-OpenAI worker pool, run required verification, perform the OpenAI final technical gate, update execution state, and proceed to the next eligible Task without a new per-task start message.

Standing authorization never authorizes staging, committing, pushing, destructive Git operations, dependency changes, database changes, migrations, backend/OpenAPI/Penpot mutation, Storybook installation/configuration, business/product decisions, or High/High-Precision implementation escalation.

### Automatic Task selection

The Orchestrator must select Tasks by DAG eligibility, not by Task number alone. A Task is eligible only when all declared predecessor Gates are `VERIFIED`, the Standing Implementation Authorization conditions pass, and no blocker type applies. Blocked Tasks do not block unrelated eligible branches unless a declared dependency requires them. Range dependencies such as `GATE-FE-T008..T012` require every Gate in that inclusive range to be `VERIFIED`.

Example: `T-FE-013` must not run while any required predecessor Gate is not `VERIFIED`; after `GATE-FE-T009` closes, it still requires every predecessor in `GATE-FE-T008..T012` to remain `VERIFIED` before eligibility.

### Mandatory human STOP conditions

Continuous execution MUST STOP and ask for user/technical-lead authority when any of these apply:

- a genuine product, business, architecture, UX, visual, or design decision is required;
- a design value, token, behavior, responsive rule, RTL rule, screen state, component contract, or page/screen approval is missing;
- screen-family approval is missing, a per-screen decision is not `APPROVED`, or required visual/Penpot/Storybook evidence is absent;
- dependency, package-manager, tooling, CI, E2E, AXE, browser, or external-service approval is required;
- security, authentication, authorization, payment, entitlement, exam/session/report, file-authorization, privacy, or production-hardening correctness is uncertain;
- backend source and canonical OpenAPI conflict, or a required API/DTO/error/permission contract cannot be established;
- database work, migration creation/application, backend mutation, OpenAPI mutation, Penpot mutation, or Storybook tooling/configuration is required;
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

Official Angular documentation and Angular-maintained AI guidance are framework guidance only. They are subordinate to explicit Nursing Platform decisions, repository governance, backend/OpenAPI business and security contracts, approved visual evidence, and active task scope. Use official Angular v22-compatible guidance for Angular framework questions; do not silently adopt later-major APIs or behaviors.

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
| `M-FE-006` | Auth/session foundation | Auth transport, tokens, token-session bootstrap, refresh, bearer, current-user hydration, logout. | `T-FE-021..T-FE-026`, `T-FE-138` | `GATE-FE-T018..T020` | member gates `VERIFIED` |
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
| `C-AUTH-LOGIN` | `POST /api/v1/auth/login`, operationId `Login`, request `LoginCommand`, success `200`, auth unspecified; unverified-email failures must not issue tokens and must return coded Problem Details code `email_verification_required` after backend implementation. |
| `C-AUTH-REFRESH` | `POST /api/v1/auth/refresh`, `RefreshToken`, request `RotateRefreshTokenCommand`, success `200`, auth unspecified. |
| `C-AUTH-REGISTER` | `POST /api/v1/auth/register`, `RegisterUser`, request `RegisterUserRequest`, success `200`, error `401`, Bearer; permission-protected administrative user-registration operation. It must not be repurposed as public self-registration. |
| `C-AUTH-REGISTER-NURSE` | Planned V1 public self-registration endpoint `POST /api/v1/auth/register/nurse`; request fields `email`, `password`, `firstName`, `lastName`; server assigns `Nurse`; no public `roleIds`; no `AuthResult`, tokens, session bootstrap, current-user hydration, or auto-login; `EmailVerified=false`; backend initiates initial verification email; duplicate email outcome must be enumeration-safe/generic. Backend implementation and OpenAPI regeneration pending. |
| `C-AUTH-REGISTER-EMPLOYER` | Planned V1 public self-registration endpoint `POST /api/v1/auth/register/employer`; request fields `email`, `password`, `firstName`, `lastName`; server assigns `Employer`; no public `roleIds`, company/org fields, or privileged role assignment; no `AuthResult`, tokens, session bootstrap, current-user hydration, or auto-login; `EmailVerified=false`; backend initiates initial verification email; duplicate email outcome must be enumeration-safe/generic. Backend implementation and OpenAPI regeneration pending. |
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

Storybook is adopted by technical-lead decision on 2026-09-10 as the intended future primary visual development and review surface for production Angular components and production screen states. This governance decision does not install or configure Storybook, add dependencies/scripts, create stories, modify Angular source, authorize navigation UI, approve any screen, or approve automated visual regression.

After Storybook tooling is separately installed and verified, reviewed Storybook rendering of production Angular components/screens may serve as visual implementation/review evidence for an owning component or screen Gate. Storybook evidence does not self-approve a Gate; the owning Gate and technical-lead decision remain authoritative.

Storybook stories must render production Angular source. They must not create Storybook-only component copies, duplicate markup, duplicate SCSS, parallel token definitions, alternate Storybook implementations, a second design system, product requirements, route/navigation inventory, labels/groups/order/icons, security behavior, backend contracts, validation semantics, payment/exam/entitlement behavior, or screen existence.

Penpot is no longer mandatory as a procedural intermediate artifact for every component or routine screen composition. Penpot/design authority remains required when materially new visual intent is unresolved, including novel page layouts, new navigation/layout concepts, complex multi-step visual flows, new major interaction paradigms, new visual language outside approved foundations, cross-screen flows whose composition cannot be derived from approved patterns, or explicit design tasks.

Approved future screen workflow:

1. Establish screen/task eligibility.
2. Approve the screen's functional/page contract: purpose, route, API/data, access, states, validation/business behavior, and ownership boundaries.
3. Determine whether materially new visual design is unresolved.
4. If yes, create/approve the required Penpot/design artifact before implementation; if no, reuse approved visual foundations/components/patterns without a duplicate Penpot artifact.
5. Implement or reuse production Angular components.
6. Render appropriate production component/screen states in Storybook after tooling authorization.
7. Review interaction, loading/error/empty, responsive, RTL/LTR, and accessibility-relevant states.
8. Perform the owning screen/component verification and approval Gate.
9. Commit only after all applicable verification passes.

Automated Storybook visual-regression/baseline testing remains unapproved. Screenshot regression services, image snapshot frameworks, browser/DPR matrices, pixel tolerance policies, hosted visual-review services, and CI visual-regression integration require separate technical-lead/tooling approval.

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

## 10.1 Angular Component File Separation Governance

The frontend architecture now explicitly requires ordinary production Angular components under `frontend/src/app` to separate component TypeScript, rendered markup, and component styling into colocated `.ts`, `.html`, and `.scss` files, with focused colocated `.spec.ts` coverage where behavior or rendering is testable. Inline component `template:`, inline component `styles:`, and template-local `<style>` blocks are prohibited for ordinary production components; Angular allowing these forms is not project authority to use them. The canonical architecture rule lives in `docs/frontend/frontend-architecture.md`, concrete enforcement and exception boundaries live in `docs/frontend/frontend-project-rules.md`, and compact agent enforcement lives in `AGENTS.md`.

This governance correction is forward-looking and does not retroactively invalidate already-VERIFIED task evidence. Existing source requiring later bounded structural remediation under the newly explicit rule was recorded as:

- `frontend/src/app/shared/ui/loading-error-retry.ts` — inline production component template from `T-FE-033`.
- `frontend/src/app/app.html` — scaffold placeholder template contains an embedded `<style>` block while `frontend/src/app/app.scss` exists.

Do not refactor these files opportunistically. Remediate them only in a separately authorized bounded architecture-remediation task or in an explicitly declared reopened/touched scope with focused verification.

T-FE-139 later completed the bounded structural remediation for exactly those two recorded targets. T-FE-033 remains historically `VERIFIED`; the later component-separation remediation is recorded separately and does not retroactively invalidate the accepted loading/error/retry behavior or evidence.

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
| `T-FE-023` | `M-FE-006` | Session bootstrap | `GATE-FE-T021`,`GATE-FE-T022` | token-session bootstrap only; no `/me`, no manual Authorization header | `CONTRACT_CLARIFICATION`,`SECURITY` |
| `T-FE-024` | `M-FE-006` | Single-flight refresh | `GATE-FE-T022`,`GATE-FE-T023` | refresh coordination only | `SECURITY` |
| `T-FE-025` | `M-FE-006` | Bearer interceptor | `GATE-FE-T024` | bearer injection; no business logic | `SECURITY` |
| `T-FE-026` | `M-FE-006` | Local logout MVP | `GATE-FE-T023..T025` | no server revocation claim | `CONTRACT_CLARIFICATION` |
| `T-FE-027` | `M-FE-007` | Shell frame | `GATE-FE-T009..T012`,`GATE-FE-T023` | landmarks/router outlet | `DESIGN` |
| `T-FE-028` | `M-FE-017` | System loading route state | `GATE-FE-T027`,`GATE-FE-T113` | owns `SYS-001` | `DESIGN` |
| `T-FE-029` | `M-FE-007` | Canonical route registry | `GATE-FE-T027` | all product routes register here | `SCOPE` |
| `T-FE-030` | `M-FE-007` | Auth/public guards | `GATE-FE-T023..T025`,`GATE-FE-T029`,`GATE-FE-T138` | auth guard/public guard using the generic auth-routing contract in `docs/frontend/design/inventory/route-permission-matrix.md` | `SECURITY` |
| `T-FE-031` | `M-FE-007` | Route-level UX permission policy | `GATE-FE-T023`,`GATE-FE-T029`,`GATE-FE-T138` | UX only; backend is security | `SECURITY`,`CONTRACT_CLARIFICATION` |
| `T-FE-032` | `M-FE-007` | Permission-aware navigation | `GATE-FE-T031` | presentation only | `DESIGN` |
| `T-FE-033` | `M-FE-008` | Loading/error/retry pattern | `GATE-FE-T019`,`GATE-FE-T010` | no feature copy | `DESIGN` |
| `T-FE-034` | `M-FE-008` | Form validation pattern | `GATE-FE-T013`,`GATE-FE-T019` | validation display only | `CONTRACT_CLARIFICATION` |
| `T-FE-035` | `M-FE-017` | Empty/no-results/restricted pattern | `GATE-FE-T031`,`GATE-FE-T113` | owns `SYS-006/007` | `DESIGN` |
| `T-FE-036` | `M-FE-008` | Confirmation/feedback/live-region | `GATE-FE-T009`,`GATE-FE-T019` | keyboard/focus/live region | `DESIGN` |
| `T-FE-037` | `M-FE-008` | File upload pattern | `GATE-FE-T034` | multipart helper | `CONTRACT_CLARIFICATION` |
| `T-FE-038` | `M-FE-008` | List/filter/pagination pattern | `GATE-FE-T033`,`GATE-FE-T035` | server pagination | `CONTRACT_CLARIFICATION` |
| `T-FE-039` | `M-FE-008` | Visual verification method | `GATE-FE-T012`,`GATE-FE-T014` | method only; not approval | `TOOLING_APPROVAL`,`DESIGN` |
| `T-FE-040` | `M-FE-009` | Auth screen approval packet | design/backend/state review | `AUTH-001..012` packet review | `DESIGN` |
| `T-FE-041` | `M-FE-009` | `AUTH-001` Sign In | `GATE-FE-T021..T025`,`GATE-FE-T030`,`GATE-FE-T034`,`GATE-FE-T040` | screen implementation after exact approval | — |
| `T-FE-042` | `M-FE-009` | Registration contract clarification | local only | `RegisterUser` auth/public behavior | `CONTRACT_CLARIFICATION` |
| `T-FE-043` | `M-FE-009` | `AUTH-002` Role Selection | `GATE-FE-T040`,`GATE-FE-T042` | approved pre-registration UI; implement only in V1 self-registration Phase 4 after backend/OpenAPI prerequisites are complete | — |
| `T-FE-044` | `M-FE-009` | `AUTH-003` Nurse Registration | `GATE-FE-T040`,`GATE-FE-T042`,`GATE-FE-T034` | approved Nurse public-registration UI; implement only in V1 self-registration Phase 4 after `C-AUTH-REGISTER-NURSE` backend/OpenAPI prerequisites are complete | `BACKEND` |
| `T-FE-045` | `M-FE-009` | `AUTH-004` Employer Registration | `GATE-FE-T040`,`GATE-FE-T042`,`GATE-FE-T034` | approved Employer public-registration UI; implement only in V1 self-registration Phase 4 after `C-AUTH-REGISTER-EMPLOYER` backend/OpenAPI prerequisites are complete | `BACKEND` |
| `T-FE-046` | `M-FE-009` | Verification email contract clarification | local only | send/verify behavior | `CONTRACT_CLARIFICATION` |
| `T-FE-047` | `M-FE-009` | `AUTH-005/006` Email Verification journey | `GATE-FE-T040`,`GATE-FE-T046` | `AUTH-005` approved as public informational check-your-email screen; `AUTH-006` verified confirmation-link screen | — |
| `T-FE-048` | `M-FE-009` | `AUTH-007` Forgot Password | `GATE-FE-T040`,`GATE-FE-T034` | screen implementation after exact approval | — |
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
| `T-FE-097` | `M-FE-015` | `ACC-001/002` account overview/details | `GATE-FE-T138`,`GATE-FE-T033`,`GATE-FE-T092` | `/me` only | `DESIGN` |
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
| `T-FE-138` | `M-FE-006` | Authenticated current-user hydration | `GATE-FE-T025` | `GET /api/v1/me` via normal transport; relies on bearer interceptor; current-user/session identity state only | `CONTRACT_CLARIFICATION`,`SECURITY` |
| `T-FE-139` | `M-FE-008` | Component separation remediation | component-separation governance acceptance, `GATE-FE-T033` | structural-only remediation for recorded inline template/style violations; no UI redesign | `SCOPE` |

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
| `ST-FE-023` | `T-FE-023` | Implement token-session bootstrap state machine without `/me` hydration. |
| `ST-FE-024` | `T-FE-024` | Implement single-flight refresh coordination. |
| `ST-FE-025` | `T-FE-025` | Implement Bearer interceptor and exclusions. |
| `ST-FE-026` | `T-FE-026` | Implement local logout without server revocation claim. |
| `ST-FE-138` | `T-FE-138` | Hydrate current user through `GET /api/v1/me` after bearer injection exists. |
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
| `ST-FE-139` | `T-FE-139` | Move recorded inline production component template and template-local style block to colocated external files without behavior or visual redesign. |

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
| `GATE-FE-T023` | `T-FE-023` | `GATE-FE-T021`, `GATE-FE-T022` | token-session bootstrap tests proving initial state, no-token anonymous path, one startup refresh attempt, token-material persistence through `TokenStorage`, invalid refresh clear/anonymous path, transient failure semantics, no `/me`, no manual Authorization header, no single-flight/interceptor/guard/UI behavior, no direct `sessionStorage`, and no generated-file edits | `CONTRACT_CLARIFICATION`,`SECURITY` |
| `GATE-FE-T024` | `T-FE-024` | `GATE-FE-T022`, `GATE-FE-T023` | refresh concurrency tests | `SECURITY` |
| `GATE-FE-T025` | `T-FE-025` | `GATE-FE-T024` | interceptor tests | `SECURITY` |
| `GATE-FE-T026` | `T-FE-026` | `GATE-FE-T023..T025` | logout tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T027` | `T-FE-027` | `GATE-FE-T009..T012`, `GATE-FE-T023` | shell/a11y tests | `DESIGN` |
| `GATE-FE-T028` | `T-FE-028` | `GATE-FE-T027`, `GATE-FE-T113` | loading state tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T029` | `T-FE-029` | `GATE-FE-T027` | route registry tests | `SCOPE` |
| `GATE-FE-T030` | `T-FE-030` | `GATE-FE-T023..T025`, `GATE-FE-T029`, `GATE-FE-T138` | guard/router tests | `SECURITY` |
| `GATE-FE-T031` | `T-FE-031` | `GATE-FE-T023`, `GATE-FE-T029`, `GATE-FE-T138` | UX permission tests | `SECURITY`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T032` | `T-FE-032` | `GATE-FE-T031` | nav visibility tests | `DESIGN` |
| `GATE-FE-T033` | `T-FE-033` | `GATE-FE-T019`, `GATE-FE-T010` | loading/error tests | `DESIGN` |
| `GATE-FE-T034` | `T-FE-034` | `GATE-FE-T013`, `GATE-FE-T019` | validation/form tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T035` | `T-FE-035` | `GATE-FE-T031`, `GATE-FE-T113` | empty/restricted tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T036` | `T-FE-036` | `GATE-FE-T009`, `GATE-FE-T019` | dialog/live-region tests | `DESIGN` |
| `GATE-FE-T037` | `T-FE-037` | `GATE-FE-T034` | multipart helper tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T038` | `T-FE-038` | `GATE-FE-T033`, `GATE-FE-T035` | pagination tests | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T039` | `T-FE-039` | `GATE-FE-T012`, `GATE-FE-T014` | visual method evidence | `TOOLING_APPROVAL`,`DESIGN` |
| `GATE-FE-T040` | `T-FE-040` | design/backend/state review | Auth approval packet with decisions per screen | `DESIGN` |
| `GATE-FE-T041` | `T-FE-041` | `GATE-FE-T021..T025`, `GATE-FE-T030`, `GATE-FE-T034`, `GATE-FE-T040` | Sign In tests + per-screen visual evidence | — |
| `GATE-FE-T042` | `T-FE-042` | local source/OpenAPI inspection | Register clarification evidence | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T043` | `T-FE-043` | `GATE-FE-T040`, `GATE-FE-T042` | Role Selection tests + per-screen visual evidence | `DESIGN` |
| `GATE-FE-T044` | `T-FE-044` | `GATE-FE-T040`, `GATE-FE-T042`, `GATE-FE-T034` | Nurse registration tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T045` | `T-FE-045` | `GATE-FE-T040`, `GATE-FE-T042`, `GATE-FE-T034` | Employer registration tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T046` | `T-FE-046` | local source/OpenAPI inspection | verification auth clarification | `CONTRACT_CLARIFICATION` |
| `GATE-FE-T047` | `T-FE-047` | `GATE-FE-T040`, `GATE-FE-T046` | email verification tests + per-screen visual evidence | `DESIGN`,`CONTRACT_CLARIFICATION` |
| `GATE-FE-T048` | `T-FE-048` | `GATE-FE-T040`, `GATE-FE-T034` | forgot password tests + per-screen visual evidence | — |
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
| `GATE-FE-T097` | `T-FE-097` | `GATE-FE-T138`, `GATE-FE-T033`, `GATE-FE-T092` | account overview tests + per-screen visual evidence | `DESIGN` |
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
| `GATE-FE-T138` | `T-FE-138` | `GATE-FE-T025` | current-user hydration tests proving `GET /api/v1/me` is called through normal auth transport/generation path, bearer injection is supplied by `T-FE-025`, current-user/session identity state is established, and no token refresh/storage/interceptor behavior is duplicated | `CONTRACT_CLARIFICATION`,`SECURITY` |
| `GATE-FE-T139` | `T-FE-139` | component-separation governance acceptance, `GATE-FE-T033` | structural compliance audit, focused component tests, full frontend quality, git scope evidence | `SCOPE` |

## 14. Screen Ownership Matrix

Every screen has exactly one primary owner Task. `approval_decision` is initially `BLOCKED` unless future screen approval changes it to `APPROVED` or `DEFERRED`.

| screen_id | primary_owner_task | family_approval_gate | approval_decision | execution_status | blocker_types | contract_dependency | post_implementation_visual_verification |
|---|---|---|---|---|---|---|---|
| `AUTH-001` | `T-FE-041` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-AUTH-LOGIN`,`C-ME` | implementation, automated verification, visual calibration, and human visual approval complete |
| `AUTH-002` | `T-FE-043` | `GATE-FE-T040` | `APPROVED` | `NOT STARTED` | — | `C-AUTH-REGISTER-NURSE`,`C-AUTH-REGISTER-EMPLOYER` | public pre-registration role-selection UI only; Nurse -> `/auth/register/nurse`, Employer -> `/auth/register/employer`; no backend call, account creation, role/session/token/current-user behavior |
| `AUTH-003` | `T-FE-044` | `GATE-FE-T040` | `APPROVED` | `NOT STARTED` | `BACKEND` | `C-AUTH-REGISTER-NURSE` | approved contract; implementation waits for backend endpoint and regenerated generated client evidence |
| `AUTH-004` | `T-FE-045` | `GATE-FE-T040` | `APPROVED` | `NOT STARTED` | `BACKEND` | `C-AUTH-REGISTER-EMPLOYER` | approved contract; implementation waits for backend endpoint and regenerated generated client evidence |
| `AUTH-005` | `T-FE-047` | `GATE-FE-T040` | `APPROVED` | `NOT STARTED` | — | `C-AUTH-REGISTER-NURSE`,`C-AUTH-REGISTER-EMPLOYER`,`C-AUTH-VERIFY` | public informational check-your-email screen at `/auth/verify-email`; no backend call, resend, countdown/cooldown, session, or account-existence disclosure |
| `AUTH-006` | `T-FE-047` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-AUTH-VERIFY` | implementation, automated verification, and render-ready Storybook evidence complete |
| `AUTH-007` | `T-FE-048` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-AUTH-FORGOT` | implementation, automated verification, render-ready Storybook evidence, and human visual approval complete |
| `AUTH-008` | `T-FE-049` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-AUTH-RESET` | implementation, automated verification, render-ready Storybook evidence complete |
| `AUTH-009` | `T-FE-050` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-AUTH-RESET` | implementation, automated verification, render-ready Storybook state evidence complete |
| `AUTH-010` | `T-FE-051` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-ME` | implementation, automated verification, render-ready Storybook evidence complete |
| `AUTH-011` | `T-FE-053` | `GATE-FE-T040` | `APPROVED` | `VERIFIED` | — | `C-ERROR` | implementation, automated verification, and render-ready Storybook evidence complete |
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
- Storybook tooling installation/configuration checkpoint.
- Browser support matrix.
- Visual-regression tolerance/approval process.
- Whether confirmed backend-gap screens create backend backlog work or remain outside frontend scope.

## 17. Scope Protection Rules

- Do not begin `T-FE-001` without explicit authorization.
- Do not scaffold Angular, create `frontend/`, install dependencies, generate the API client, modify backend source/tests, modify canonical OpenAPI, modify Penpot, install/configure Storybook, create stories, stage, commit, or push unless a later explicit approval says otherwise.
- Do not mark implementation Tasks `VERIFIED` because they are planned.
- Do not modify completed `VERIFIED` scope unless an approved Task explicitly marks it `REOPENED`.
- Do not use this ledger to expand task scope during implementation.
- Do not treat a `NOT STARTED` entry as approval to code.
- Permanent atomic checkpoint policy: reconcile and commit `PROGRESS.md` plus this ledger first as docs-only `docs(frontend): reconcile implementation ledger state`, then resume `T-FE-017` separately with `services: false` clean regeneration; never mix docs reconciliation with package/generated files in one commit; no push and no history rewrite.
- Next-step handoff: after the docs-only checkpoint commit, resume `T-FE-017` with `ng-openapi-gen@1.0.5` and `services: false` using the standard function-based generated client from clean generated output; paused partials (`frontend/package.json`, `frontend/package-lock.json`, `frontend/dependency-policy.json`, `frontend/eslint.config.js`, `frontend/src/app/core/api/generated/**`) stay uncommitted until that resume. `T-FE-015`/`T-FE-016` remain `VERIFIED`; no `T-FE-017` verification is claimed.

## 18. Execution Status Records

### Storybook visual workflow governance decision — no T-FE task ID assigned

- status: `VERIFIED` once committed by documentation-only checkpoint `docs(frontend): adopt storybook visual workflow`
- decision_date: 2026-09-10
- accepted_baseline: `75e7a4c feat(frontend): add permission-aware navigation logic`
- closed_predecessor_context: `T-FE-029`, `T-FE-030`, `T-FE-031`, and `T-FE-032` are closed; actual navigation UI and product screens remain unimplemented.
- governance_summary: Storybook is adopted as the intended future primary visual development/review surface for production Angular components and production screen states, but this checkpoint authorizes documentation only. Storybook is not business, backend, API, route, auth, permission, validation, payment, exam, entitlement, screen-existence, page-ownership, or product-requirements authority.
- implementation_source_boundary: Production Angular source remains the single implementation source. Future stories must import/render production components and must not create Storybook-only copies, duplicate markup, duplicate SCSS, alternate implementations, parallel token definitions, or a second design system.
- Penpot_boundary: Penpot is no longer mandatory as a procedural intermediate artifact for every component or routine screen composition. Penpot/design authority remains required when materially new visual intent is unresolved or explicitly designated by a design task.
- screen_boundary: Screen-family eligibility, exact screen approval, task dependencies, implementation gates, technical-lead approval, functional/accessibility/testing requirements, and current screen `BLOCKED`/`NOT STARTED` statuses remain unchanged. No screen becomes implementation-authorized by Storybook adoption.
- tooling_boundary: No existing ledger task currently owns Storybook installation/configuration. `T-FE-039` may later own visual verification method updates and `T-FE-122` remains the per-screen visual verification template, but actual Storybook tooling/setup still requires separate technical-lead authorization or an explicitly authorized execution checkpoint. Do not invent or activate a new DAG task without authority.
- visual_regression_boundary: Automated Storybook visual regression, screenshot services, image snapshots, browser/DPR matrix, pixel tolerance policy, hosted visual review, and CI integration remain unapproved future decisions.
- explicit_exclusions: No Storybook installation/configuration, package changes, `angular.json` changes, lockfile changes, stories, Angular source changes, navigation UI, screen implementation, backend changes, OpenAPI/generated API changes, Penpot changes, push, or screen gate verification occurred in this governance checkpoint.

### Angular patch-alignment checkpoint before Storybook tooling — no T-FE task ID assigned

- status: `VERIFIED` once committed by dependency-alignment checkpoint `chore(frontend): align angular patch versions`
- decision_date: 2026-09-11
- checkpoint_purpose: Resolve the npm Angular peer patch-resolution blocker discovered before Storybook installation without installing Storybook or weakening zoneless architecture.
- preserved_Storybook_history: The initial `zone.js` concern was resolved by official `@storybook/angular-vite` metadata/docs showing optional `zone.js` and default zoneless behavior. The second blocker was npm Angular patch skew during Storybook install attempts. This checkpoint aligns Angular patches only; it does not mark Storybook tooling installed or verified.
- alignment_strategy: Official npm metadata and Angular-supported update mechanics selected Angular framework/Material/CDK `22.1.6` and CLI/build `22.1.8` as the compatible Angular `22.1` patch set. Framework packages that have exact same-patch peer constraints now resolve together at `22.1.6`; Material/CDK remain exact direct pins at `22.1.6`; CLI/build follow their own compatible patch cadence at `22.1.8`.
- dependency_scope: `frontend/package.json`, `frontend/package-lock.json`, and `frontend/dependency-policy.json` record the aligned direct dependency policy. `frontend/src/styles/material-theme-bridge.spec.ts` updates only the existing Material/CDK version contract assertions from `22.1.5` to `22.1.6` after the first full test run failed on the old expected patch.
- zoneless_integrity: `@angular/animations`, `zone.js`, `zone.js/testing`, and `provideZoneChangeDetection` remain absent; no zone compatibility exception was introduced.
- explicit_exclusions: No Storybook packages, `.storybook/**`, `*.stories.*`, production Angular source, Angular config, backend, canonical OpenAPI/generated API, navigation UI, product screens, visual-regression tooling, overrides, `--force`, or `--legacy-peer-deps` were introduced.
- verification_summary: Normal `npm install` passed; `npm ls` shows Angular framework packages at `22.1.6`, Material/CDK at `22.1.6`, CLI/build at `22.1.8`, and no invalid Angular peer graph; `npm test -- --watch=false` passed 25 files / 276 tests after the intentional version-contract test update; `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, and `npm run quality` including production build passed. Lockfile changed only Angular/Angular-devkit/Schematics patch packages.
- Storybook_status: Storybook tooling remains pending and was not installed in this checkpoint.

### Storybook tooling/configuration checkpoint — no T-FE task ID assigned

- status: `VERIFIED` once committed by tooling checkpoint `chore(frontend): configure storybook`
- decision_date: 2026-09-11
- accepted_baseline: Angular alignment commit `0f79849` and Storybook governance commit `19ee376`.
- checkpoint_purpose: Install and configure the smallest official Storybook Angular-Vite tooling surface needed to render a production Angular component in Storybook while preserving the existing Angular 22 zoneless architecture.
- dependency_scope: Added direct dev dependencies only: `storybook@10.6.0` and `@storybook/angular-vite@10.6.0`, recorded in `frontend/package.json`, `frontend/package-lock.json`, and `frontend/dependency-policy.json`. No direct `@angular/animations` dependency was added; npm resolves `@angular/animations@22.1.6` only as an official peer/transitive dependency required by `@storybook/angular-vite` / Angular platform packages. No `zone.js` dependency was added.
- configuration_scope: Added minimal `.storybook/main.ts`, `.storybook/preview.ts`, and `.storybook/tsconfig.json`; imported the existing production `frontend/src/styles.scss`; configured SCSS load paths for existing project styles; added `/storybook-static` to `frontend/.gitignore`; and excluded `src/**/*.stories.ts` from production `tsconfig.app.json` while allowing the Storybook tsconfig to include stories.
- story_scope: Added exactly one smoke story, `frontend/src/app/shared/ui/loading-error-retry.stories.ts`, which imports and renders the existing production `LoadingErrorRetry` component. No Storybook-only component copy, duplicate markup, duplicate SCSS, alternate design system, route/navigation inventory, labels/groups/icons/order, product requirement, backend contract, validation/auth/payment/exam/entitlement behavior, screen implementation, or additional story was added.
- zoneless_integrity: Official `@storybook/angular-vite@10.6.0` metadata supports Angular `>=21 <23`; `zone.js` is an optional peer; Storybook Angular-Vite docs state zoneless is default and `zone.js` imports occur only when `zoneless: false`. This checkpoint did not add `zone.js`, `zone.js/testing`, or `provideZoneChangeDetection`, and did not enable a zone compatibility mode.
- verification_summary: Normal Storybook install and subsequent `npm install` completed without `--force`, `--legacy-peer-deps`, overrides, or vulnerabilities. `npm run storybook -- --ci --smoke-test --no-open` passed. `npm run build-storybook` completed successfully and discovered the smoke story bundle. `npm test -- --watch=false` passed 25 files / 276 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, and `npm run quality` including production build passed. Dependency and source scans found no `zone.js/testing`, no `provideZoneChangeDetection`, and no visual-regression tooling.
- explicit_exclusions: No Angular version/patch changes, production component behavior changes, product screens, navigation UI, route activation, visual-regression tooling, Playwright/AXE/CI tooling, backend source, canonical OpenAPI, generated API, Penpot mutation, database/migration work, push, or next frontend DAG task occurred.

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

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-09
- baseline_head: `1145863`
- technical_decisions_resolved: Future T-FE-009 implementation is approved to use exact direct pins `@angular/material@22.1.5` and `@angular/cdk@22.1.5`; `@angular/animations` is not approved and not required; theme family remains M2 using `mat.m2-define-palette(...)` and `mat.m2-define-light-theme(...)`; scope is a single light theme only; bridge path remains `frontend/src/styles/_material-theme-bridge.scss`; future integration remains through `frontend/src/styles.scss`; custom Material typography and density remain deferred unless separately approved.
- design_reference_checkpoint: Phase A created `docs/frontend/design/frontend-design-foundation-reference.md` as the canonical textual implementation mapping for the live Penpot `Frontend Design Foundation` under the then-current Penpot visual source-of-truth wording; that wording is superseded for current implementation governance by the 2026-09-10 Storybook visual workflow decision while preserving the approved foundation values. Backend/OpenAPI remains behavior/security authority. Live Penpot MCP inspection verified the file name, page set, `0` local colors, `34` local typographies, `1` local component, token sets `Spacing Shape` and `Elevation States`, approved color values, spacing/radius values, elevation/focus/state tokens, and documented focus evidence conflict.
- design_decisions_resolved: Technical lead resolved the T-FE-009 Material role mapping as Material M2 primary = Nursing Teal `#006B66`, accent = Professional Navy `#173B57`, warn = Error `#B3261E`; Warning `#8A4B00` remains a separate recoverable-warning semantic; Exam/Focus Indigo `#4F46B8` remains a separate exam/focus semantic. The implementation must not synthesize 50–900 ramps, must not treat Angular default brand palettes as visual authority, and must stop if Angular Material Sass requires invented hue/contrast data.
- known_focus_conflict: Colors authority says `border/focus = #4F46B8`; Components examples use indigo/blue focus treatment; Elevation & States token authority says `focus.ring.shadow = 0 0 0 4px #006B66` with `focus.ring.offset = 4px`; existing VERIFIED frontend focus-token behavior remains unchanged until explicit reconciliation. `T-FE-009` must not opportunistically change focus behavior.
- exclusion_summary: Phase A is documentation/governance only. No Material packages are installed in Phase A, no dependency-policy change is made in Phase A, no `_material-theme-bridge.scss` is created in Phase A, no theme source is implemented in Phase A, no backend/OpenAPI/Penpot source is changed, and no `T-FE-013`, routing, screen, or later task is started.
- implementation_summary: Phase A was committed as `e243976 docs(frontend): add canonical design foundation reference`. Phase B installed exact direct dependencies `@angular/material@22.1.5` and `@angular/cdk@22.1.5` with no direct `@angular/animations`; updated dependency-policy approvals for only those pins; added centralized `frontend/src/styles/_material-theme-bridge.scss`; and integrated it only from `frontend/src/styles.scss`. The bridge uses Angular Material M2 Sass APIs (`mat.m2-define-palette(...)`, `mat.m2-define-light-theme(...)`) with one light theme only, no dark theme, no custom Material typography, and no custom Material density.
- palette_strategy: The bridge uses flat custom M2 palettes because Penpot did not provide approved 50-900 hue ramps. Each Material role repeats only the approved role color across required M2 hue keys: Primary `#006B66`, Accent `#173B57`, Warn/Error `#B3261E`; all contrast entries use approved `#FFFFFF`. No synthetic shade values, Angular built-in palettes, `#005A56`, semantic Warning `#8A4B00`, or Exam/Focus Indigo `#4F46B8` were introduced into the Material bridge.
- verification_summary: TDD RED was observed before implementation. Focused Material bridge source-contract tests passed 1 file / 11 tests. Full frontend tests passed 16 files / 160 tests. `npm run lint` passed. `npm run lint:styles` passed. `npm run quality` passed, including dependency guard, lint, stylelint, full tests, and production build. `git diff --check` passed and staged area was empty before final documentation update.
- integrity_summary: Changed T-FE-009 Phase B implementation/config/test scope is limited to `frontend/package.json`, `frontend/package-lock.json`, `frontend/dependency-policy.json`, `frontend/src/styles.scss`, `frontend/src/styles/_material-theme-bridge.scss`, and `frontend/src/styles/material-theme-bridge.spec.ts`, plus final evidence updates in `PROGRESS.md` and this ledger. Git-guardian scope review returned PASS for exact Phase B scope and confirmed no backend, canonical OpenAPI, generated API, Angular components, focus/accessibility foundation, responsive/RTL foundation, screens, routing, `T-FE-013+`, or canonical design-reference mutation. OpenAI direct fallback was used only after Muse/BigPickle worker attempts were blocked by repository agent command permissions (`npm install` and compound shell checks), and only within the bounded approved Phase B scope.

### `ST-FE-009` — Bridge tokens to single Angular Material theme

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Phase A records the required Penpot foundation reference and technical-lead Material role mapping. Phase B installed exact Material/CDK `22.1.5` pins, implemented the centralized M2 single-light-theme bridge, compiled successfully through `npm run quality`, verified approved primary/accent/warn values, preserved warning/exam/focus semantics, and avoided invented hue ramps by using flat approved-color M2 palettes.

### `GATE-FE-T009`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Gate evidence is complete. Phase A reference commit is `e243976 docs(frontend): add canonical design foundation reference`. Phase B supplies exact dependency evidence for `@angular/material@22.1.5` and `@angular/cdk@22.1.5` with no direct `@angular/animations`; centralized Material M2 bridge source checks; one-light-theme/no-dark/no-typography/no-density checks; flat approved-color palette evidence; focused test result 1 file / 11 tests; full frontend result 16 files / 160 tests; `npm run lint`, `npm run lint:styles`, and `npm run quality` including production build; `git diff --check`; empty staged area before final evidence update; and git-guardian PASS for exact atomic T-FE-009 Phase B scope.

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

### `T-FE-013` — Standard form controls

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-09
- scope_summary: Implemented the bounded shared standard form-control foundation only under `frontend/src/app/shared/ui/form-controls/`. Added standalone Angular Material wrapper components for standard text-like inputs (`text`, `email`, `password`, `search`, `number`), textarea, select, checkbox, and radio group, plus a barrel export and focused source/behavior tests. No switch, autocomplete, date picker, file upload, password visibility toggle, search clear button, feature form, screen, route, validation policy, backend/OpenAPI/generated API, package/dependency, or Material theme bridge changes were made.
- design_contract_summary: The implementation consumes the verified runtime tokens, Material bridge, accessibility foundation, direction/locale foundation, and responsive helpers. Field styling records the approved 64px field foundation, 12px radius, 16px logical inline padding, stable support row, logical start alignment, explicit 44px touch-target baseline, and 48px mobile selection-control target. The stylesheet uses existing approved tokens only and no hardcoded parallel brand color values.
- accessibility_summary: Controls keep visible labels; placeholders remain supplementary. Helper and structural error text are associated through stable IDs and `aria-describedby` without defining timing, business validation, backend field mapping, or Problem Details behavior. Required, optional, disabled, and readonly semantics are preserved distinctly where applicable. Selection controls rely on Angular Material checkbox/radio primitives for native keyboard-focusable inputs.
- component_separation_summary: All new production Angular components use the project-required external `templateUrl: './standard-form-controls.html'` and `styleUrl: './standard-form-controls.scss'` metadata, with no inline component templates, no inline component styles, and no template-local `<style>` block. Focused colocated tests live in `standard-form-controls.spec.ts`.
- orchestration_note: Initial T-FE-013 implementation routing used Muse/free-worker-first attempts that stopped before edits because of repository command permission failure; Big Pickle fallback also stopped before edits because of command permission failure. The focused TDD red failed before `standard-form-controls.ts` existed (`Could not resolve "./standard-form-controls"` / `TS2307`), and focused green passed after implementation and targeted compile/test fixes. Resumed run used free-route verification only: Big Pickle verifier API failure, MiMo timeout, Muse verifier PASS; no direct OpenAI implementation or verification fallback was needed in this resumed run.
- verification_summary: Classification A; Muse scout classification. Focused verification passed `npm test -- --watch=false --include=src/app/shared/ui/form-controls/standard-form-controls.spec.ts` with 1 file / 9 tests PASS. Full frontend verification passed `npm test -- --watch=false` with 17 files / 169 tests PASS, `npm run lint` PASS, `npm run lint:styles` PASS, and `npm run quality` PASS including dependency guard, Angular lint, Stylelint, full tests, and production build. `git diff --check` PASS, `git diff --cached --name-status` empty (cached diff empty), structural audit PASS, and `git status --short` showed only orchestrator-owned `PROGRESS.md`, this ledger after evidence update, and the new `frontend/src/app/shared/ui/form-controls/` task directory.
- closure_evidence: Gate evidence is sufficient for the standard form-control foundation: standard controls render through Angular Material primitives, visible labels/placeholders/support text and selected/checked/value semantics are covered, required/optional/disabled/readonly distinctions are covered, logical dimensions/touch-target/source-boundary tests pass, component separation is enforced, and excluded advanced controls/validation/forms/routing/screens/dependencies/backend/generated/API work are absent. T-FE-013 is closed as `VERIFIED`; T-FE-014, T-FE-027, T-FE-034, T-FE-036, routing, and screens remain separate tasks and were not started.

### `ST-FE-013` — Implement standard field/select/control foundation

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: The shared standard form-control foundation exists as standalone Angular Material components with external template/style files, focused tests, approved dimensions, logical layout, helper/error association, and no advanced/later controls or validation policy.
- closure_evidence: Focused standard form-control tests and full frontend quality/build verification passed; no out-of-scope source, dependency, backend, OpenAPI, generated API, route, screen, or Material theme bridge changes were made.

### `GATE-FE-T013`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Gate evidence covers focused form dimension/a11y/source-boundary tests, full frontend tests, Angular lint, Stylelint, aggregate quality/build, clean whitespace diff, empty staged area, and implementation scope limited to the new shared form-controls task files plus orchestrator-owned evidence docs.
- closure_evidence: Gate is closed as `VERIFIED`; downstream form validation (`T-FE-034`), shell/routing (`T-FE-027`), feedback/live-region (`T-FE-036`), accessibility automation (`T-FE-014`), screens, and feature forms remain separate tasks and must not start in this checkpoint.

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

### `T-FE-019` — Problem Details mapping

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-08
- scope_summary: Implemented the approved non-generated frontend Problem Details mapping boundary only. Added `frontend/src/app/core/api/problem-details.ts` and `frontend/src/app/core/api/problem-details.spec.ts`; did not edit generated client files, backend source, canonical OpenAPI, packages, environment files, auth/interceptors/guards/routing, feature UI, DTO adapters, or T-FE-020 scope.
- contract_summary: Backend source, backend Problem Details OpenAPI contract tests, generated DTOs, and canonical OpenAPI agree for `ProblemDetails`, `ValidationProblemDetails`, `CodedProblemDetails`, and `RetryableProblemDetails`. Base fields are `type`, `title`, `status`, `detail`, and `traceId`; validation adds `errors: Record<string,string[]>`; coded adds `code`; retryable adds `retryAfterSeconds`. OpenAPI uses flat schemas rather than inheritance composition. Runtime status mirrors HTTP status; backend emits `application/problem+json`; backend does not define `instance`, UI copy, fallback business codes, auth/logout behavior, or retry orchestration in these contracts.
- mapping_summary: The mapper normalizes unknown generated/error payloads into readonly frontend categories `generic`, `validation`, `coded`, and `retryable`, preserving authoritative fields and extensions where present while safely degrading malformed, nullish, partial, empty-code, and invalid retry metadata payloads without throwing or inventing business semantics. Optional `instance` is tolerated as pass-through only if a payload carries a non-empty string.
- tdd_summary: Focused TDD red test failed before `problem-details.ts` existed with `Could not resolve "./problem-details"` / `TS2307`; focused green verification then passed 1 file / 13 tests.
- verification_summary: Muse/free-worker verification passed `npm test -- --watch=false --include='**/problem-details.spec.ts'` (1 file / 13 tests), `npm test -- --watch=false` (6 files / 46 tests), `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty `git diff --cached --name-status`, and `git status --short` showing only orchestrator-owned `PROGRESS.md` plus the two new T-FE-019 files before final evidence docs. OpenAI final gate performed targeted inspection of the new mapper/test files and git scope.
- closure_evidence: Gate evidence is sufficient for the C-ERROR contract family: generic, validation, coded, retryable, optional/nullish/unknown handling, source DTO immutability, generated-file boundary, and absence of UI/business behavior are covered. T-FE-019 is closed as `VERIFIED`; T-FE-020 remains not started and requires separate eligibility/authorization.

### `ST-FE-019` — Normalize backend Problem Details variants

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Normalized frontend error model and mapper preserve authoritative base fields, validation errors, backend codes, and retry metadata while degrading incomplete/unknown payloads safely. No UI copy, toast/routing/auth/logout, retry orchestration, or feature-specific business behavior is embedded.
- closure_evidence: Focused Problem Details unit tests and full frontend quality verification passed; generated files were not edited.

### `GATE-FE-T019`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Gate evidence covers source/OpenAPI contract clarification for all four Problem Details variants, focused unit tests for actual mapping outputs and safety behavior, full frontend test/lint/style/quality/build verification, empty staged area, clean whitespace diff, and generated-file scope protection.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-020` must not start in this run.

### `T-FE-020` — DTO adapter boundary

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-08
- scope_summary: Implemented the approved minimum non-generated DTO adapter boundary only. Added `frontend/src/app/core/api/dto-adapters.ts` and `frontend/src/app/core/api/dto-adapters.spec.ts`; did not edit generated client files, backend source, canonical OpenAPI, packages, environment files, auth/session/interceptors/guards/routing, feature-specific adapters, UI behavior, or T-FE-021 scope.
- clarification_summary: Repository authority is unambiguous: generated DTOs may be consumed directly only at generated-client/transport boundaries or for pure pass-through, while adapters are required when data crosses into frontend-owned application/domain-facing shapes, when unknown generated fields must be excluded, when nullability is normalized for a frontend shape, when generated pagination wrappers are exposed to callers, or when enum/date values need explicit future feature policy. Reusable boundary helpers live under non-generated `frontend/src/app/core/api/`; feature-specific adapters remain future work. Problem Details mapping remains separate in `problem-details.ts`; generated files remain read-only.
- mapping_summary: The boundary exposes `normalizeNullable`, `adaptDto`, structural `PaginatedSource`/`PaginatedView`, and `adaptPaginatedResult`. Caller-provided mappers define the frontend-owned output shape, so generated extra fields do not leak by object spreading. Null and undefined normalize deterministically to `undefined`; enum/value and date/time transport strings are preserved verbatim unless a later feature-specific adapter explicitly maps them.
- verification_summary: Muse/free-worker verification passed focused adapter tests with `npm test -- --watch=false --include='**/dto-adapters.spec.ts'` (1 file / 10 tests), full frontend tests (7 files / 56 tests), `npm run lint` after fixing the reported array-type lint issue, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty `git diff --cached --name-status`, `git status --short`, and generated-directory diff checks. OpenAI final gate performed targeted inspection of the adapter implementation/test files and git scope.
- closure_evidence: Gate evidence is sufficient for the DTO adapter boundary policy: generated DTO input maps to frontend-owned output, source DTOs are not mutated, nullable/optional values are deterministic, unknown fields do not leak, the boundary does not depend on generated implementation internals, Problem Details mapping remains separate, and generated files were not edited. T-FE-020 is closed as `VERIFIED`; T-FE-021 remains not started.

### `ST-FE-020` — Establish generated DTO adapter boundary

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: A small reusable adapter utility pattern exists for later API/auth work without creating a broad domain-model framework. It uses explicit caller-owned mappers, null normalization, pagination-wrapper mapping, readonly/frozen outputs, and no generated internals or Problem Details coupling.
- closure_evidence: Focused adapter boundary tests and full frontend quality verification passed; generated files were not edited.

### `GATE-FE-T020`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Gate evidence covers adapter boundary tests for generated DTO input to frontend-owned output, source immutability, optional/null handling, unknown-field non-leakage, generated-internals independence, Problem Details separation, generated-file protection, full frontend test/lint/style/quality/build verification, empty staged area, and clean whitespace diff.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-021` must not start in this run.

### `T-FE-021` — Auth transport

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-08
- scope_summary: T-FE-021 auth transport implementation was initially blocked before non-generated transport files were created because generated auth/current-user functions returned `StrictHttpResponse<void>` for endpoints whose backend source/tests return typed bodies. The authorized correction checkpoint changed only canonical OpenAPI metadata for the affected success responses plus generated files produced by the approved generator. The final implementation adds only non-generated `frontend/src/app/core/api/auth-transport.ts` and `frontend/src/app/core/api/auth-transport.spec.ts`; no backend runtime/source mutation, package/dependency change, generated-file manual edit, token storage, session/bootstrap, refresh coordination, bearer interceptor/header injection, logout, guards, UI/forms, T-FE-022+, staging, push, amend, rebase, or squash occurred.
- contract_comparison: Backend source/test verification established `POST /api/v1/auth/login` returns `Results.Ok(AuthResult)`, `POST /api/v1/auth/refresh` returns `Results.Ok(AuthResult)`, and `GET /api/v1/me` returns `Results.Ok(UserDetailDto)`. The prior canonical OpenAPI `Login`, `RefreshToken`, and `GetCurrentUser` 200 responses had only `description: OK` and no `application/json` schema, causing generated `login`, `refreshToken`, and `getCurrentUser` functions to clone `body: undefined` and return `StrictHttpResponse<void>`.
- correction_classification: The mismatch is classified as `OPENAPI_METADATA_DEFECT`, not frontend DTO invention and not backend behavior missing, because accepted backend source/tests unambiguously confirm typed response bodies while canonical OpenAPI omitted only response-schema metadata.
- openapi_correction: Canonical OpenAPI `development-openapi-2026-09-03.json` now declares `POST /api/v1/auth/login` 200 as `AuthResult`, `POST /api/v1/auth/refresh` 200 as `AuthResult`, and `GET /api/v1/me` 200 as `UserDetailDto`; canonical `AuthResult` and `UserDetailDto` component schemas were added. Two unintended temporary insertions for root `/` and `SendVerificationEmail` were removed and are not part of the final correction.
- generated_client_summary: Regeneration used the approved `ng-openapi-gen@1.0.5` function-based `services: false` configuration. Generated outputs now expose `login(...): Observable<StrictHttpResponse<AuthResult>>`, `refreshToken(...): Observable<StrictHttpResponse<AuthResult>>`, and `getCurrentUser(...): Observable<StrictHttpResponse<UserDetailDto>>`; generated `AuthResult` and `UserDetailDto` model exports were added.
- correction_verification_summary: Deterministic generation passed twice with `260` TypeScript files and SHA-256 `424bee7bb44f6f3b10ff83efd65eace8ed883ffebb6559095b3b9c60acdaa482`. `npm run quality` passed, including dependency guard, Angular lint, Stylelint, full frontend tests (`7` files / `56` tests), and production build. OpenAPI JSON parse passed and `git diff --check` passed. The OpenAPI blocker is resolved and T-FE-021 implementation is ready to resume after the atomic correction commit `docs(frontend): correct auth response schemas`.
- implementation_summary: `AuthTransport` is an injectable transport boundary that delegates `login(LoginCommand)`, `refresh(RotateRefreshTokenCommand)`, and `getCurrentUser()` to the corrected generated functions with injected `HttpClient` and `ApiConfiguration.rootUrl`, then maps `StrictHttpResponse.body` to typed `AuthResult`, `AuthResult`, and `UserDetailDto` results. It intentionally contains no token persistence, session/bootstrap state, refresh coordination, bearer header injection/interceptor logic, logout, guards, routing, UI, or error conversion.
- implementation_verification_summary: Muse/free-worker TDD red failed before `auth-transport.ts` existed with `Could not resolve "./auth-transport"` / `TS2307`; focused green passed `npm test -- --watch=false --include='**/auth-transport.spec.ts'` with 1 file / 8 tests. Full verification passed `npm test -- --watch=false` (8 files / 64 tests), `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty staged area, `git diff --name-only -- frontend/src/app/core/api/generated` with no output, and git status showing only orchestrator-owned `PROGRESS.md` plus the two new T-FE-021 files before final docs evidence. OpenAI final gate inspected `auth-transport.ts`, `auth-transport.spec.ts`, and git scope.
- closure_evidence: Gate evidence is sufficient for transport-only auth API scope: login/refresh/current-user delegate to the generated operations with generated request DTOs, typed `AuthResult` and `UserDetailDto` bodies are returned, generated `HttpErrorResponse` errors propagate without UI/session conversion, no token storage/session/bootstrap/bearer/header/logout/guard behavior is introduced, generated files remain untouched, and full frontend verification passed. T-FE-021 is closed as `VERIFIED`; T-FE-022 must not start without separate authorization.

### `ST-FE-021` — Implement typed login/refresh/current-user transport

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: The typed login/refresh/current-user transport boundary exists in `frontend/src/app/core/api/auth-transport.ts`, consumes corrected generated API functions and generated request/response types, maps response bodies to typed observables, and preserves generated errors unchanged.
- closure_evidence: Focused auth transport tests and full frontend verification passed; generated files were not edited during implementation and no T-FE-022+ responsibilities were added.

### `GATE-FE-T021`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Gate evidence records OpenAPI blocker resolution, corrected generated typed auth/current-user outputs, focused auth transport tests for delegation/request DTOs/typed bodies/error propagation/no auth-state behavior, full frontend test/lint/style/quality/build verification, clean whitespace diff, empty staged area before docs update, generated-dir diff protection, and OpenAI final-gate source/scope inspection.
- closure_evidence: Gate is closed as `VERIFIED`; downstream T-FE-022 remains a separate token-storage task and must not start in this checkpoint.

### `T-FE-022` — Token storage abstraction

- status: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_date: 2026-09-08
- scope_summary: Implemented only the approved local-MVP token-storage abstraction in `frontend/src/app/core/auth/token-storage.ts` with focused tests in `frontend/src/app/core/auth/token-storage.spec.ts`. No backend, canonical OpenAPI, generated client, package/dependency, T-FE-021 auth transport, interceptor, guard, session bootstrap, refresh coordination, logout, UI/forms, or T-FE-023+ files were modified.
- security_contract_summary: The accepted T-FE-022 security review established the repository contract: access token and access-token expiry are memory-only, refresh token is stored only in `sessionStorage`, `localStorage` is forbidden, cookies are not part of the current backend contract, and all token access must be centralized behind the auth/token-storage abstraction.
- implementation_summary: `TokenStorage` is an injectable root service with an injectable `AUTH_SESSION_STORAGE` backend. It exposes `getTokenState()`, `getAccessToken()`, `getAccessTokenExpiresAt()`, `getRefreshToken()`, `setTokenMaterial(...)`, and `clear()`. `setTokenMaterial` stores only access token/expiry in service memory and persists only the refresh token under `np.auth.refreshToken`; `clear` removes in-memory access material and removes the persisted refresh token. A new service instance with the same storage backing restores only the refresh token.
- boundary_summary: The abstraction does not call login, refresh, or `/me`; does not inject `HttpClient` or Authorization headers; does not coordinate refresh; does not bootstrap session/user state; does not implement logout workflow; does not synchronize tabs; does not persist `AuthResult`, user profile, roles/permissions, Problem Details, arbitrary generated DTOs, Authorization headers, or credentials other than the approved refresh token; and contains no token logging/error/URL/analytics path.
- tdd_summary: Focused TDD red failed before `token-storage.ts` existed with `Could not resolve "./token-storage"` / `TS2307`; focused green passed `npm test -- --watch=false --include='**/token-storage.spec.ts'` with 1 file / 8 tests, then strengthened security-boundary coverage passed with 1 file / 9 tests.
- verification_summary: Full verification passed `npm test -- --watch=false` (9 files / 73 tests), `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty `git diff --cached --name-status`, `git status --short`, and forbidden-scope diff checks proving no generated/backend/OpenAPI/package/auth-transport files changed. OpenAI final gate inspected `token-storage.ts`, `token-storage.spec.ts`, and git scope.
- closure_evidence: Gate evidence is sufficient for T-FE-022 token-storage-only scope: initial empty state, existing refresh token recovery, memory-only access/expiry, sessionStorage-only refresh persistence, clear/removal, same-tab reload semantics, missing storage safety, replacement semantics, no localStorage, no unrelated DTO persistence, no Authorization/header/interceptor/session/bootstrap/refresh/logout/guard/UI behavior, and generated/backend/OpenAPI integrity are covered. T-FE-022 is closed as `VERIFIED`; T-FE-023 must not start without separate authorization.

### `ST-FE-022` — Establish local-MVP token storage abstraction

- status: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_summary: The local-MVP token storage abstraction centralizes access to refresh-token `sessionStorage` while keeping access token and expiry in memory only. Focused tests prove storage, replacement, clear, reload, missing-storage, and forbidden-behavior boundaries.
- closure_evidence: Subtask accepted as complete from focused tests, full frontend verification, and final-gate security/scope review.

### `GATE-FE-T022`

- status_result: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_summary: Gate evidence covers token-storage tests, memory-only access token/expiry, sessionStorage-only refresh token, no localStorage, no user/profile/role/permission/Problem Details/AuthResult DTO persistence, no Authorization header/interceptor/bootstrap/refresh/logout/guard behavior, no generated/backend/OpenAPI/package changes, full frontend verification, and git integrity checks.
- closure_evidence: Gate is closed as `VERIFIED`; downstream T-FE-023 remains a separate session-bootstrap task and must not start in this checkpoint.

### `T-FE-023` — Session bootstrap

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- logout_safe_support_correction_date: 2026-09-09
- logout_safe_support_correction_summary: Narrow T-FE-026 support correction added `AuthSessionBootstrap.resolveAnonymous()` as an explicitly authorized anonymous reset API. It updates only the existing token-session state signal to `anonymous` and performs no transport, refresh, token storage, current-user, logout, navigation, route guard, or UI behavior.
- governance_correction_date: 2026-09-09
- governance_correction_summary: Technical lead accepted the DAG/session-bootstrap order conflict and explicitly rejected a manual bearer-header exception inside `T-FE-023`. `T-FE-023` now owns token-session bootstrap only: unknown/bootstrap state, refresh-token availability through the `T-FE-022` token-storage abstraction, anonymous completion without network calls when no refresh token exists, one startup refresh attempt through the `T-FE-021` auth transport when a refresh token exists, successful token-material persistence through token storage, invalid/expired/revoked refresh clearing local token material and resolving anonymous, and no `/api/v1/me`, no manual Authorization header, no general single-flight refresh coordination, no interceptor behavior, no route guards, and no UI behavior.
- downstream_hydration_owner: `T-FE-138` owns authenticated current-user hydration after `GATE-FE-T025` so `/me` calls rely on normal bearer interceptor behavior.
- evidence_date: 2026-09-09
- scope_summary: Implemented only revised token-session bootstrap in `frontend/src/app/core/auth/auth-session-bootstrap.ts`, focused tests in `frontend/src/app/core/auth/auth-session-bootstrap.spec.ts`, and minimal Angular startup registration in `frontend/src/app/app.config.ts`. No backend, canonical OpenAPI, generated client, package/dependency, auth transport, token storage, interceptor, guard, route, UI/component, logout, current-user hydration, T-FE-024, T-FE-025, T-FE-026, or T-FE-138 implementation files were modified.
- implementation_summary: `AuthSessionBootstrap` is an injectable root service with `state` signal initialized to `initializing`. `bootstrap()` reads the refresh token through `TokenStorage.getRefreshToken()` only. Without a refresh token it resolves `anonymous` without auth network calls. With a refresh token it performs one startup `AuthTransport.refresh({ refreshToken })` attempt. Successful refresh stores token material through `TokenStorage.setTokenMaterial({ accessToken, accessTokenExpiresAt: expiresAt, refreshToken })` and resolves `authenticated`. Any refresh error clears token material through `TokenStorage.clear()` and resolves `anonymous`. `provideAuthSessionBootstrap()` registers the bootstrap through Angular `provideAppInitializer` so startup waits for the bootstrap observable.
- boundary_summary: The implementation does not call `GET /api/v1/me` or `AuthTransport.getCurrentUser`, does not attach Authorization headers manually, does not inject `HttpClient`, does not use direct `sessionStorage`, `localStorage`, or `globalThis` storage access, does not implement single-flight/general refresh coordination, request queues, replay, or interceptor behavior, and does not implement route guards, UI, logout workflow, user profile, roles, permissions, or current-user identity state.
- tdd_summary: Initial TDD RED failed before `auth-session-bootstrap.ts` existed with unresolved `./auth-session-bootstrap` / `TS2307`. A targeted final-gate wiring correction added a startup-provider test; RED failed before `provideAuthSessionBootstrap` existed with `TS2724` plus the private initializer-runner type issue, then the implementation added `provideAppInitializer` wiring and used a narrow test cast to run Angular initializers.
- verification_summary: Accepted final verification passed `npm test -- --watch=false --include='**/auth-session-bootstrap.spec.ts'` with 1 file / 10 tests, `npm test -- --watch=false` with 10 files / 83 tests, `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty `git diff --cached --name-status`, `git status --short`, and `git diff --name-only -- frontend/src/app/core/api/generated` with no generated-file changes.
- closure_evidence: Gate evidence is sufficient for revised T-FE-023 token-session bootstrap scope: initial state, no-refresh-token anonymous/no-network path, stored-refresh-token one startup refresh attempt, successful refresh token-material persistence through `TokenStorage`, invalid refresh clear/anonymous path, transient/network refresh failure clear/anonymous path, no `/me`, no manual Authorization header, no direct storage access, no generated-file edits, no single-flight/general refresh coordination, no interceptor/guard/UI/logout/current-user hydration behavior, and Angular startup initializer registration are covered. T-FE-023 is closed as `VERIFIED`; T-FE-024 remains not started and must not begin in this checkpoint.

### `ST-FE-023` — Implement token-session bootstrap state machine without `/me` hydration

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- logout_safe_support_evidence: Focused bootstrap tests prove `resolveAnonymous()` moves token-session state to `anonymous`, makes no `/auth/refresh` or `/me` network call, does not access browser storage directly, and introduces no current-user/logout/navigation/guard/UI behavior.
- governance_correction_summary: Subtask scope is token-session bootstrap only and must not hydrate current user.
- evidence_summary: Subtask implemented a minimal token-session bootstrap state machine and startup initializer. It uses `TokenStorage` and `AuthTransport.refresh` only, resolves authenticated/anonymous token-session state, and preserves `/me` hydration, bearer injection, refresh coordination, logout, guards, and UI as later-task responsibilities.
- closure_evidence: Subtask accepted as complete from focused bootstrap tests, full frontend verification, and scope/boundary evidence.

### `GATE-FE-T023`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- governance_correction_summary: Gate evidence must cover token-session bootstrap state and boundary tests, including no `/me`, no manual Authorization header, no direct `sessionStorage`, no generated-file edits, no single-flight/general refresh coordination, and no interceptor/guard/UI behavior.
- evidence_summary: Gate evidence covers token-session bootstrap tests for initial state, no-token anonymous/no-network path, stored refresh token one startup refresh attempt, successful token material persistence through `TokenStorage`, invalid/expired/revoked refresh clear/anonymous behavior, transient/network refresh failure clear/anonymous behavior, no current-user endpoint calls, startup provider registration, and source-boundary checks excluding manual Authorization headers, `HttpClient`, direct browser storage, refresh coordination, interceptor, guard, router, logout, replay, and queue behavior.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-024` remains a separate single-flight refresh coordination task, `T-FE-025` remains a separate bearer-interceptor task, and `T-FE-138` remains a separate authenticated current-user hydration task.

### `T-FE-024` — Single-flight refresh

- status: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_date: 2026-09-09
- logout_safe_support_correction_summary: Narrow T-FE-026 support correction added `RefreshCoordinator.invalidate()` using a generation check. Invalidation advances the active refresh generation and clears the in-flight observable reference. A late response from an earlier invalidated generation cannot write returned token material and invalidated callers do not receive a successful restored `AuthResult`; in-flight cleanup remains bounded to the matching observable, and a later explicit refresh can start fresh when valid token material exists. No bearer, navigation, UI, logout workflow, current-user, `/me`, retry/backoff, direct browser storage, generated/backend/OpenAPI/package behavior was added.
- scope_summary: Implemented only single-flight refresh coordination in `frontend/src/app/core/auth/refresh-coordinator.ts` with focused tests in `frontend/src/app/core/auth/refresh-coordinator.spec.ts`. No backend, canonical OpenAPI, generated client, package/dependency, auth transport, token storage, session bootstrap, bearer interceptor, guard, route, UI/component, logout, current-user hydration, T-FE-025, T-FE-026, or T-FE-138 implementation files were modified.
- implementation_summary: `RefreshCoordinator` is an injectable root service. `refresh()` reuses one in-flight observable per app/tab instance, reads the refresh token through `TokenStorage.getRefreshToken()`, calls only `AuthTransport.refresh({ refreshToken })`, writes returned token material through `TokenStorage.setTokenMaterial({ accessToken, accessTokenExpiresAt: expiresAt, refreshToken })`, shares the same result/error with concurrent callers, clears token material through `TokenStorage.clear()` on refresh/no-token/storage failure, and resets its in-flight state after success or failure so later refresh attempts start a new backend request.
- boundary_summary: The implementation has no retry/backoff policy, no bearer header injection, no interceptor behavior, no failed-request replay queue, no manual Authorization header, no direct `HttpClient`, no `/api/v1/me` or current-user hydration, no route/guard/UI/logout behavior, no direct `sessionStorage`/`localStorage`/`globalThis` storage access, no token logging, and no generated/backend/OpenAPI/package changes.
- tdd_summary: Focused TDD RED was observed before implementation. Accepted focused verification then passed `npm test -- --watch=false --include=src/app/core/auth/refresh-coordinator.spec.ts` with 1 file / 8 tests.
- verification_summary: Accepted full frontend verification passed `npm test -- --watch=false` with 11 files / 91 tests, `npm run lint`, `npm run lint:styles`, `npm run quality`, `git diff --check`, `git diff --cached --name-status`, and `git status --short`. Current continuation re-read the persisted implementation and final checks preserved scope.
- closure_evidence: Gate evidence is sufficient for T-FE-024 refresh-coordination-only scope: one refresh in flight per app/tab instance, concurrent callers share one refresh operation, successful refresh updates token storage before callers receive success, rotated refresh token replaces old token, backend/no-token/storage failure clears token material and errors callers, in-flight state resets after success/failure, later refresh starts a new backend request, no retry/backoff, no bearer/interceptor/replay behavior, no `/me` hydration, no route/UI/logout behavior, no direct browser storage access, and no generated/backend/OpenAPI/package changes. T-FE-024 is closed as `VERIFIED`; T-FE-025 remains not started and must not begin in this checkpoint.

### `ST-FE-024` — Implement single-flight refresh coordination

- status: `VERIFIED`
- blocker_types: `SECURITY`
- logout_safe_support_evidence: Focused refresh-coordinator tests prove refresh starts, logout-style invalidation before completion prevents a late successful refresh response from repopulating token material, invalidated waiting callers do not receive `AuthResult` success, coordinator state resets cleanly, a later refresh starts a fresh backend request when valid token material remains, and forbidden integration checks still exclude direct browser storage, logout/navigation/UI, current-user, `/me`, bearer, guard, retry/backoff, generated/backend/OpenAPI/package behavior.
- evidence_summary: Subtask implemented a minimal refresh coordinator over `AuthTransport.refresh` and `TokenStorage`, proving shared concurrent refresh, token-material update before release, rotated refresh-token replacement, failure clearing semantics, and in-flight reset for later attempts.
- closure_evidence: Subtask accepted as complete from focused refresh-coordinator tests, full frontend verification, final-gate source/scope review, and git-guardian scope review.

### `GATE-FE-T024`

- status_result: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_summary: Gate evidence covers refresh concurrency tests and source-boundary checks for shared in-flight operation, token storage update ordering, refresh-token rotation, backend/no-token/storage failure clearing, later retry after success/failure, no retry/backoff, no bearer/interceptor/replay, no `/me`, no route/UI/logout, no direct browser storage, no generated/backend/OpenAPI/package changes, full frontend verification, and git integrity checks.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-025` remains a separate bearer-interceptor task and must not start in this checkpoint.

### `T-FE-025` — Bearer interceptor

- status: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_date: 2026-09-09
- scope_summary: Implemented only the approved bearer-auth interceptor in `frontend/src/app/core/auth/bearer-interceptor.ts`, focused tests in `frontend/src/app/core/auth/bearer-interceptor.spec.ts`, and minimal Angular HTTP provider registration in `frontend/src/app/app.config.ts`. No backend, canonical OpenAPI, generated client, package/dependency, auth transport, token storage, refresh coordinator, session bootstrap, route guard, UI/component, logout, current-user hydration, T-FE-026, T-FE-030, or T-FE-138 files were modified.
- implementation_summary: `bearerInterceptor` is an Angular functional interceptor. It preserves existing `Authorization` headers, reads the current memory access token only through `TokenStorage.getAccessToken()`, matches eligible application API requests by relative `/api/v1` paths or absolute URLs whose origin matches the configured `ApiConfiguration.rootUrl` and whose path is under `/api/v1`, skips the exact anonymous allowlist, and clones eligible requests with `Authorization: Bearer <accessToken>` only when an access token exists.
- anonymous_allowlist_summary: Bearer injection is skipped for `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/verify-email`, `POST /api/v1/auth/forgot-password`, `POST /api/v1/auth/reset-password`, `GET /api/v1/preparation-packages/offers`, and `GET /api/v1/preparation-packages/offers/{slug}`. Bearer remains allowed for `POST /api/v1/auth/register`, `POST /api/v1/auth/send-verification-email`, `GET /api/v1/me`, and other protected API requests when an access token exists.
- boundary_summary: The interceptor does not call or import `RefreshCoordinator`, `AuthTransport`, or `HttpClient`; does not handle `401` responses; does not retry or replay requests; does not clear token storage; does not navigate, redirect, or implement logout; does not call `/me` or hydrate current user/roles/permissions; does not implement route guards or UI behavior; does not access `sessionStorage`, `localStorage`, or `globalThis` directly; and does not log token values.
- tdd_summary: Focused TDD RED failed before implementation with `Could not resolve "./bearer-interceptor"` / `TS2307`, proving the focused spec targeted missing behavior. Focused GREEN passed `npm test -- --watch=false --include=src/app/core/auth/bearer-interceptor.spec.ts` with 1 file / 19 tests.
- verification_summary: Full frontend verification passed `npm test -- --watch=false` with 12 files / 110 tests, `npm run lint`, `npm run lint:styles`, and `npm run quality` including dependency guard and production build. One lint issue in the new interceptor (`ReadonlyArray<AnonymousRoute>` style) was fixed and `npm run lint` reran clean. Git integrity checks passed: `git diff --check`, empty `git diff --cached --name-status`, `git status --short`, and forbidden-scope diff checks proving no generated/backend/OpenAPI/package/refresh-coordinator/bootstrap/token-storage/auth-transport changes.
- closure_evidence: Gate evidence is sufficient for T-FE-025 bearer-interceptor-only scope: eligible relative and configured absolute API requests receive bearer when a token exists; arbitrary external absolute URLs containing `/api/v1` remain untouched; no-token requests pass unchanged; anonymous endpoints remain unauthenticated; register/send-verification-email/`/me` receive bearer when token exists; existing `Authorization` is preserved exactly; non-API requests are untouched; no direct browser storage, refresh coordination, 401 handling, replay/retry, `/me` hydration, route/UI/logout behavior, or generated/backend/OpenAPI/package mutation exists. T-FE-025 is closed as `VERIFIED`; T-FE-138 and T-FE-026 remain not started and must not begin in this checkpoint.

### `ST-FE-025` — Implement Bearer interceptor and exclusions

- status: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_summary: Subtask implemented a functional bearer interceptor with exact API matching, exact anonymous allowlist, missing-token passthrough, existing-header preservation, and provider registration. Focused tests prove request behavior and security boundaries.
- closure_evidence: Subtask accepted as complete from focused interceptor tests, full frontend verification, final-gate security/scope review, and git-guardian scope review.

### `GATE-FE-T025`

- status_result: `VERIFIED`
- blocker_types: `SECURITY`
- evidence_summary: Gate evidence covers interceptor tests for bearer attachment on eligible protected API requests, relative and configured absolute API URL matching, third-party absolute URL exclusion, missing-token passthrough, exact anonymous endpoint exclusions, register/send-verification-email/`/me` bearer behavior, existing `Authorization` preservation, non-API passthrough, provider registration, no direct browser storage, no refresh/401/retry/replay/logout/route/UI/current-user behavior, no generated/backend/OpenAPI/package changes, full frontend verification, and git integrity checks.
- closure_evidence: Gate is closed as `VERIFIED`; downstream `T-FE-138` current-user hydration and `T-FE-026` local logout remain separate tasks and must not start in this checkpoint.

### `T-FE-026` — Local logout MVP

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- evidence_date: 2026-09-09
- support_correction_summary: Prerequisite support correction committed via `79f054f fix(frontend): make auth session logout-safe`. `RefreshCoordinator.invalidate()` prevents an earlier in-flight refresh from repopulating tokens or delivering successful `AuthResult` after logout-style invalidation, and `AuthSessionBootstrap.resolveAnonymous()` updates only token-session state to `anonymous`.
- scope_summary: Implemented only local logout in `frontend/src/app/core/auth/local-logout.ts` with focused tests in `frontend/src/app/core/auth/local-logout.spec.ts` and this ledger evidence. Local logout invalidates refresh, clears token material through `TokenStorage.clear()`, resolves `AuthSessionBootstrap` anonymous, and resolves `CurrentUserStore` anonymous. It performs no backend logout call, server-side revocation claim, navigation, route guard, UI/toast, refresh call, manual bearer behavior, `/me` call, generated/backend/OpenAPI/package/dependency mutation, or screen implementation.
- verification_summary: Focused TDD RED failed before `local-logout.ts` existed with unresolved `./local-logout` / `TS2307`; focused GREEN passed 1 file / 6 tests. Full verification evidence covers token clearing, current-user/bootstrap anonymous state, active-refresh invalidation before token clearing, late refresh success not resurrecting auth state, repeated logout idempotency, no backend logout/revoke/refresh/`/me` request, source-boundary checks excluding direct browser storage, navigation, route/UI behavior, server revocation claim, manual bearer behavior, generated-file mutation, and no forbidden scope.
- closure_evidence: Technical-lead accepted and closed T-FE-026 after atomic commit `ffe47af feat(frontend): add local logout`. Committed scope is `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, `frontend/src/app/core/auth/local-logout.ts`, and `frontend/src/app/core/auth/local-logout.spec.ts`. Auth/session foundation ownership remains preserved: T-FE-023 token-session bootstrap, T-FE-024 refresh coordination, T-FE-025 bearer injection, T-FE-138 authenticated current-user hydration, and T-FE-026 local logout.

### `ST-FE-026` — Implement local logout without server revocation claim

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- evidence_summary: Subtask implemented the minimal `LocalLogout` service. `logout()` is synchronous and idempotent, invalidates refresh first, clears token storage, resets token-session bootstrap state anonymous, and clears current-user state anonymous.

### `GATE-FE-T026`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- evidence_summary: Gate evidence proves access token, access-token expiry, and refresh token clearing; CurrentUserStore and AuthSessionBootstrap anonymous resolution; active-refresh invalidation before late refresh success; no stale refresh resurrection; repeated logout idempotency; no backend logout/revoke/refresh/`/me` request; no direct `sessionStorage`/`localStorage`; no navigation, route guard, UI/toast, server revocation claim, generated/backend/OpenAPI/package mutation, or screen implementation.
- closure_evidence: Gate is closed as `VERIFIED` and committed via `ffe47af feat(frontend): add local logout`; no route guard, shell, route registry, screen, generated, backend, OpenAPI, package, dependency, or product-source work is included by this gate.

### `T-FE-027` — Shell frame

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-09
- predecessor_summary: Required predecessor gates from the authoritative DAG are `GATE-FE-T009..T012` and `GATE-FE-T023`; all were confirmed `VERIFIED` before implementation. The shell task was selected by DAG eligibility, not by task number alone.
- contract_summary: T-FE-027 owns only the shared application shell frame contract recorded as `landmarks/router outlet`, `ST-FE-027` accessible shell frame, and `GATE-FE-T027` shell/a11y tests. The bounded interpretation is a neutral app-root structural shell with a single focusable main content target and the Angular RouterOutlet. T-FE-027 does not own route registry entries, route guards, permission policy, permission-aware navigation, role/product navigation, loading route state, feedback/live-region behavior, screen implementation, feature components, auth/session behavior, backend/OpenAPI/generated API changes, dependencies/tooling, or unapproved visual design decisions. Skip-link/header/nav/sidebar/toolbar/mobile-menu UI is deferred because no navigation/header block is owned by this task.
- scope_summary: Implemented only `frontend/src/app/app.ts`, `frontend/src/app/app.html`, `frontend/src/app/app.scss`, and `frontend/src/app/app.spec.ts`. The Angular scaffold placeholder logo, links, social content, title, and scaffold styles were removed. The app template now renders `.np-app-shell` with one `main#main-content` carrying `tabindex="-1"` and containing `<router-outlet />`. `app.ts` preserves the T-FE-011 locale/direction service initialization and external `templateUrl`/`styleUrl` metadata. `app.scss` uses logical sizing and `padding-inline: var(--np-page-gutter)`, with approved text token `var(--np-color-brand-2)` and no hardcoded colors, oklch scaffold values, custom breakpoints, motion, radius, elevation, z-index, or physical directional properties.
- verification_summary: Muse/free-worker TDD RED first failed with 5 expected focused shell-test failures against the scaffold placeholder: missing `main-content` id/focusability, missing RouterOutlet inside main, scaffold SVG/links still present, product/navigation links present, and scaffold SCSS lacking shell logical/token contract. Focused GREEN passed `npm test -- --watch=false --include=src/app/app.spec.ts` with 1 file / 9 tests. Full verification passed `npm test -- --watch=false` with 18 files / 184 tests, `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty staged area, and git status/scope checks. Independent Mimo/free-worker review returned PASS for scope, design authority, accessibility/responsive/RTL, separation, tests, and git scope. OpenAI final gate performed targeted inspection of the changed app-root files and git scope.

### `ST-FE-027` — Implement accessible shell frame

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: The subtask implemented a small accessible shell frame at the application root with one focusable main content landmark and RouterOutlet, neutral tokenized logical styling, scaffold placeholder removal, and focused shell/a11y tests. It does not create navigation, routes, guards, permission behavior, loading route state, screens, or feature workflows.

### `GATE-FE-T027`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Gate evidence covers shell/a11y focused tests for one main landmark, stable focus target, RouterOutlet placement, scaffold placeholder removal, absence of navigation/product links, component separation, no embedded template styles, logical tokenized CSS, full frontend tests, lint, stylelint, aggregate quality/build, git diff checks, independent review, and scope confirmation that no route registry/guard/permission/navigation/screen/auth/generated/backend/OpenAPI/dependency work was pulled forward.

### `T-FE-029` — Canonical route registry

- status: `VERIFIED`
- blocker_types: `SCOPE`
- evidence_date: 2026-09-10
- predecessor_summary: Required predecessor gate `GATE-FE-T027` was already `VERIFIED`; implementation was explicitly authorized by the technical lead from approved route-authority commit `de6bc16 docs(frontend): approve canonical route contract`.
- contract_summary: T-FE-029 owns only stable canonical frontend route IDs, the exact approved canonical paths/path templates, and minimal deterministic dynamic path construction. The implementation consumes `docs/frontend/design/inventory/page-registry.md` as the approved route authority and does not reopen route-design decisions, infer URLs from backend/Penpot/Angular conventions, or implement route access behavior.
- scope_summary: Implemented only the pure Core route registry in `frontend/src/app/core/routing/canonical-routes.ts` with focused source-contract tests in `frontend/src/app/core/routing/canonical-routes.spec.ts`. `frontend/src/app/app.routes.ts` remains unchanged and empty. No Angular route activation, placeholders, guards, redirects, return-url validation, safe-return behavior, role/permission/access policy, navigation, breadcrumbs, route titles/copy, feature query-state logic, screen/component implementation, backend/OpenAPI/generated API, auth/session/interceptor/current-user/logout, shared UI, styling, dependency, or package changes were introduced.
- verification_summary: TDD RED was captured before implementation with unresolved `./canonical-routes` / `TS2307` plus expected compile failures from the missing registry. Focused GREEN passed `npm test -- --watch=false --include=src/app/core/routing/canonical-routes.spec.ts` with 1 file / 13 tests. Full frontend verification passed `npm test -- --watch=false` with 19 files / 197 tests, `npm run lint`, `npm run lint:styles`, and `npm run quality` including dependency guard and production build. Deterministic contract checks confirmed 62 implemented canonical entries, 62 unique IDs, 62 unique templates, exact match to the approved page-registry rows, no forbidden route/path leakage, `NURSE_ENTRY`/`ADMIN_ENTRY` present, `NURSE_HOME`/`ADMIN_HOME` absent, `app.routes.ts` unchanged, and no backend/OpenAPI/generated/package/auth/shared/features/styles diff. Independent Big Pickle review returned PASS with no Critical/High/Medium findings.

### `ST-FE-029` — Implement canonical route registry entries/policy shape

- status: `VERIFIED`
- blocker_types: `SCOPE`
- evidence_summary: Subtask implemented the route ID/path template registry and deterministic dynamic builders for every approved dynamic canonical route. It includes no route metadata for guards, roles, permissions, redirects, titles, labels, icons, breadcrumbs, menus, or feature workflow state.

### `GATE-FE-T029`

- status_result: `VERIFIED`
- blocker_types: `SCOPE`
- evidence_summary: Gate evidence covers exact 62/62 approved canonical route representation, duplicate-free IDs/templates, full mechanical contract comparison against `docs/frontend/design/inventory/page-registry.md`, path hygiene, non-routable/blocked/deferred exclusions, dynamic builder correctness and template immutability, `app.routes.ts` unchanged/empty, full frontend tests/lint/stylelint/quality/build, independent review PASS, git diff checks, and forbidden-scope confirmation. Downstream `T-FE-030`, `T-FE-031`, and `T-FE-032` remain separate tasks and were not started.

### `T-FE-030` — Auth/public guards

- status: `VERIFIED`
- blocker_types: `SECURITY`,`CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-10
- contract_authority_checkpoint: Technical-lead auth-routing decisions were accepted for documentation/contract finalization and persisted in `docs/frontend/design/inventory/route-permission-matrix.md`, then a separate technical-lead continuation explicitly authorized T-FE-030 implementation from accepted contract commit `35ad4e6 docs(frontend): define auth routing contract` and canonical route registry commit `47b38dc feat(frontend): add canonical route registry`.
- implementation_summary: Implemented only reusable generic T-FE-030 primitives: canonical-registry-based route auth classification (`PUBLIC`, `ENTRY`, derived `AUTHENTICATED`), pure internal-only safe-return validation with decoded-path hardening, and an AuthSessionBootstrap-based authenticated route guard that returns Angular UrlTree redirects for anonymous access to authenticated routes using canonical `AUTH_SIGN_IN` and exact query key `returnUrl`. Guard primitives remain unattached because `frontend/src/app/app.routes.ts` has no approved route targets and remains empty.
- verification_summary: Focused RED evidence first failed on missing T-FE-030 modules, then targeted safe-return hardening RED failed on slash-prefixed scheme and encoded path-confusion cases. Final focused tests passed 3 files / 31 tests, proving exact route classification counts (`TOTAL=62`, `PUBLIC=12`, `ENTRY=1`, `AUTHENTICATED=49`), public/entry/authenticated examples, anonymous redirect with preserved path/query/fragment, canonical sign-in destination, exact `returnUrl` key, initializing wait, no duplicate bootstrap/refresh, no CurrentUserStore dependency, PUBLIC access while authenticated, ROOT_ENTRY unrestricted, safe-return positive/negative/security cases, and no role/permission behavior. Full frontend verification passed 22 files / 228 tests, `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty staged-area checks, and scope checks.
- independent_security_review: MiMo deep-reviewer read-only review and targeted safe-return re-review passed with no Critical/High/Medium findings. The initial Low encoded-path hardening observation was resolved by validating the decoded path portion while preserving safe encoded query values.
- explicit_exclusions: No `frontend/src/app/app.routes.ts` route population or placeholders, no route activation, no fake lazy routes, no anonymous-only guard, no authenticated-user redirect away from PUBLIC routes, no post-login navigation, no role checks, no permission checks, no actor-family authorization, no navigation/menu/sidebar visibility, no screen components, no session-expired automatic navigation, no access-denied decision, no backend/OpenAPI/generated/dependency/package/tooling changes, no T-FE-031, and no push.

### `ST-FE-030` — Implement auth/public guard primitives

- status: `VERIFIED`
- blocker_types: `SECURITY`,`CONTRACT_CLARIFICATION`
- evidence_summary: Verified reusable auth/public guard primitives are present under `frontend/src/app/core/routing/` and `frontend/src/app/core/auth/`: route classification, safe-return validation, authenticated-route guard, and focused tests. `app.routes.ts` remains empty and guard attachment is deferred until an authorized route/screen integration task supplies real route targets.

### `GATE-FE-T030`

- status_result: `VERIFIED`
- blocker_types: `SECURITY`,`CONTRACT_CLARIFICATION`
- evidence_summary: Gate evidence satisfies guard/router tests and security review: focused tests passed 3 files / 31 tests; full frontend tests passed 22 files / 228 tests; lint, stylelint, quality/build, diff checks, scope checks, and independent MiMo security review/re-review passed. Newly unblocked tasks may consume only the verified generic auth/public guard primitives according to their own prerequisites and approval gates; T-FE-031/navigation/screens remain separate and were not started by this gate.

### `T-FE-031` — Route-level UX permission policy

- status: `VERIFIED`
- blocker_types: `SECURITY`,`CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-10
- contract_authority_checkpoint: Technical-lead accepted the read-only T-FE-031 extraction and supplied the missing product/permission decisions for documentation finalization only. The accepted contract is persisted in `docs/frontend/design/inventory/route-permission-matrix.md`.
- contract_summary: `T-FE-031` applies only to the 49 canonical `AUTHENTICATED` routes. It consumes `CurrentUserStore` after hydration: roles from `/api/v1/me` are the actor-context UX source, permissions from `/api/v1/me` are the capability UX source, JWT claims are not used, and backend authorization remains authoritative. Allowed V1 policies are `AUTHENTICATED_ONLY`, `ROLE`, and `ROLE_AND_PERMISSION`; role matching uses union semantics and `ROLE_AND_PERMISSION` requires both role and permission predicates. `idle`/`loading` wait, `ready` evaluates, `anonymous` remains `T-FE-030`, and `unavailable` is not denial/sign-in/access-denied. Ready authenticated users failing an explicit role or role+permission policy redirect to canonical `SYSTEM_ACCESS_DENIED`; backend HTTP `403` handling is outside `T-FE-031`. Every authenticated route requires an explicit policy row; there is no silent default or path-prefix inference.
- policy_counts: `AUTHENTICATED_ONLY=11`, `NURSE_ROLE=19`, `EMPLOYER_ROLE=4`, `ADMIN_ENTRY_ROLE=1`, `ADMIN_ROLE_AND_PERMISSION=14`, `TOTAL=49`.
- implementation_summary: Implemented reusable route-level UX permission primitives only: a pure 49-row explicit canonical route policy registry/evaluator in `frontend/src/app/core/routing/route-permission-policy.ts`, plus a small functional route permission guard primitive in `frontend/src/app/core/routing/route-permission.guard.ts` that consumes `CurrentUserStore`, waits for `idle`/`loading`, evaluates only `ready`, passes through `anonymous` and `unavailable`, and redirects ready users failing explicit role/permission policy to canonical `SYSTEM_ACCESS_DENIED` with no `returnUrl`.
- verification_summary: Focused TDD RED evidence failed before `route-permission-policy.ts` and `route-permission.guard.ts` existed. Focused GREEN passed `route-permission-policy.spec.ts` 1 file / 10 tests and `route-permission.guard.spec.ts` 1 file / 15 tests. Full frontend verification passed 24 files / 253 tests, `npm run lint`, `npm run lint:styles`, and `npm run quality` including dependency guard and production build. Matrix verification proved `TOTAL_AUTHENTICATED_ROUTES=49`, `POLICY_ROWS=49`, `AUTHENTICATED_ONLY=11`, `NURSE_ROLE=19`, `EMPLOYER_ROLE=4`, `ADMIN_ENTRY_ROLE=1`, `ADMIN_ROLE_AND_PERMISSION=14`, `DUPLICATE_ROUTE_POLICIES=0`, `MISSING_ROUTE_POLICIES=0`, `EXTRA_ROUTE_POLICIES=0`, `PUBLIC_ROUTE_POLICIES=0`, and `ROOT_ENTRY_POLICY=false`. Independent MiMo review PASS reported no Critical/High/Medium findings.
- explicit_exclusions: No route attachment, route activation, placeholder route/component, navigation/sidebar/menu/breadcrumb/link filtering, screen behavior, backend/OpenAPI/generated/package/dependency changes, HTTP `403` handling, business ownership/entitlement/payment/exam/account lifecycle rules, `T-FE-030` behavior redesign, `T-FE-032`, or push occurred.

### `ST-FE-031` — Implement route-level UX permission policy

- status: `VERIFIED`
- blocker_types: `SECURITY`,`CONTRACT_CLARIFICATION`
- evidence_summary: Implemented and verified the reusable route-level UX permission policy registry, evaluator, and guard primitive from the accepted contract. Focused tests prove exact 49-policy coverage/counts, AUTHENTICATED_ONLY behavior, ROLE any-match union behavior, ROLE_AND_PERMISSION AND behavior, no Admin permission bypass, `idle`/`loading` wait, `anonymous` and `unavailable` pass-through without T-FE-030 duplication, canonical access-denied redirect only for ready explicit-policy failure, missing-policy invariant failure, backend-403 exclusion, path-prefix exclusion, and no T-FE-032 navigation leakage.

### `GATE-FE-T031`

- status_result: `VERIFIED`
- blocker_types: `SECURITY`,`CONTRACT_CLARIFICATION`
- evidence_summary: Gate evidence satisfies UX permission tests and independent review: production registry covers exactly the 49 current `AUTHENTICATED` canonical routes once, with no PUBLIC/ENTRY policies and count split `11/19/4/1/14`; exact backend permission spelling is implemented, including `ADMIN_PAYMENT_PRODUCTS` with `Exams.View`; evaluator tests prove multi-role union semantics, `ROLE_AND_PERMISSION` AND semantics, and no Admin bypass; guard tests prove `idle`/`loading` wait, `anonymous` pass-through, `unavailable` pass-through, ready-state evaluation, and canonical `SYSTEM_ACCESS_DENIED` redirect only for ready explicit-policy failure; source/scope tests prove no path-prefix matching authority, no backend `403` handling, no business-rule leakage, no route attachment, and no T-FE-032 navigation leakage. Full frontend verification and independent MiMo review passed.

### `T-FE-032` — Permission-aware navigation

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-10
- scope_decision: Technical lead accepted the read-only T-FE-032 boundary review and authorized `LOGIC-ONLY` implementation. T-FE-032 in this checkpoint owns only reusable non-visual navigation-policy logic for caller-supplied canonical route IDs. It does not define the actual product navigation inventory, labels, icons, groups, order, layout, breadcrumbs, sidebar/header/drawer/mobile navigation, Angular Material navigation UI, Storybook workflow, Penpot governance, route activation, or screens.
- implementation_summary: Added `frontend/src/app/core/routing/navigation-permission-policy.ts` as a small pure Core routing module. `getNavigationEligibility(routeId, user)` returns explicit per-route `eligible`, `ineligible`, `unresolved`, or `unsupported` results. `filterEligibleNavigationCandidates(candidates, user)` accepts only caller-supplied canonical route IDs, preserves caller order and eligible duplicates in ready-state filtering, introduces no route IDs absent from the caller input, excludes ineligible/unsupported routes from the resolved eligible output, and returns an explicit `unresolved` aggregate result with preserved caller candidates for `idle`, `loading`, `anonymous`, and `unavailable` states. The implementation reuses T-FE-031 `getRoutePermissionPolicy` and `evaluateRoutePermission`; it does not duplicate route-policy registry, role matching, permission matching, access-denied redirect, backend `403`, path-prefix, route-family, JWT, API, or security-enforcement behavior.
- verification_summary: Genuine TDD RED was captured before the production module existed with unresolved `./navigation-permission-policy` / `TS2307`. A targeted final-gate correction then added explicit aggregate unresolved semantics; its RED failed against the old array-return API with `TS2339` for missing `status` / `eligible` / `candidates`, and final GREEN passed `npm test -- --watch=false --include=src/app/core/routing/navigation-permission-policy.spec.ts` with 1 file / 23 tests. Full frontend verification passed `npm test -- --watch=false` with 25 files / 276 tests, `npm run lint`, `npm run lint:styles`, and `npm run quality` including dependency guard and production build. Independent MiMo read-only review returned PASS with 16/16 checklist items passing and no Critical/High/Medium/Low findings.
- explicit_exclusions: No actual navigation UI, navigation inventory, menu/sidebar/header/drawer/mobile components, Angular templates, SCSS/CSS, labels/copy, icons, groups, order, active visual state, responsive or RTL navigation presentation, breadcrumbs, page layouts, Storybook, Penpot governance, package/dependency changes, backend/OpenAPI/generated API changes, `frontend/src/app/app.routes.ts` changes, route activation, screen implementation, T-FE-031 redesign, T-FE-030 redesign, access-denied navigation, sign-in redirect, JWT reads, backend API calls, or push occurred.

### `ST-FE-032` — Implement permission-aware navigation presentation

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Subtask is verified for the authorized logic-only presentation primitive boundary. It delivers reusable permission-aware navigation visibility logic only: per-route eligibility and caller-supplied candidate filtering based on T-FE-031 route permission policy and CurrentUser-compatible states. Concrete visual navigation presentation remains unimplemented and requires separate product/design/visual-workflow authority before any UI work begins.

### `GATE-FE-T032`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Gate evidence satisfies the authorized logic-only nav visibility requirement. Focused tests prove ready-state `AUTHENTICATED_ONLY`, `ROLE`, and `ROLE_AND_PERMISSION` eligibility, missing-role and missing-permission ineligibility, inherited multi-role union semantics, no Admin permission bypass, caller-supplied candidate filtering, order preservation, duplicate preservation for eligible duplicates, no injected route IDs, explicit unresolved results for `idle`, `loading`, `anonymous`, and `unavailable`, explicit unsupported results for PUBLIC/ENTRY/unknown route IDs, T-FE-031 evaluator/policy reuse, no path-prefix inference, no access-denied/sign-in routing, no UI behavior, and `app.routes.ts` unchanged/empty. Full frontend verification and independent MiMo review passed.

### `T-FE-138` — Authenticated current-user hydration

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- evidence_date: 2026-09-09
- contract_summary: Technical-lead decision approved T-FE-138 ownership of authenticated `GET /api/v1/me` hydration and a separate frontend-owned current-user identity state boundary. Approved states are `idle`, `loading`, `ready`, `anonymous`, and `unavailable`; this state must not repurpose T-FE-023 token-session state. Startup must resolve anonymous without `/me` after anonymous token bootstrap, and must trigger exactly one current-user hydration after authenticated token bootstrap through `AuthTransport.getCurrentUser()` while relying exclusively on T-FE-025 bearer injection. Generated `UserDetailDto` must be adapted to immutable frontend-owned `CurrentUser`; roles/permissions come from `/me`, not JWT; CurrentUser must not be persisted. `/me` `401` clears local token material through `TokenStorage` and resolves anonymous without logout UX/navigation; transient/network/5xx and unexpected non-401 failures preserve tokens, clear exposed current user for the failed hydration attempt, and resolve `unavailable`; no automatic retry, refresh coordination, manual bearer injection, logout, navigation, guards, UI, generated/backend/OpenAPI/dependency changes, or T-FE-026 behavior is authorized.
- scope_summary: Implemented only the dedicated `CurrentUser` model/adapter, `CurrentUserStore`, focused tests, minimal ordered startup integration in `app.config.ts`, and governance evidence. No generated files, backend/OpenAPI, token-storage/auth-session-bootstrap/refresh-coordinator/bearer-interceptor internals, route guards, logout, UI, package/dependency files, or unrelated DAG history were modified.
- verification_summary: Focused TDD RED failures were captured for missing current-user files, the initial independent-app-initializer startup race, and stale exposed-user clearing. Final focused T-FE-138 tests passed 1 file / 21 tests. Full frontend verification passed 13 files / 131 tests, `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, empty staged-area check, final git status scope review, forbidden-path diff checks, and git-guardian no-shell scope review PASS.

### `ST-FE-138` — Hydrate current user through `GET /api/v1/me` after bearer injection exists

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- implementation_summary: Verified frontend-owned immutable `CurrentUser` adapter/state boundary and ordered startup hydration after token-session bootstrap. Anonymous token bootstrap resolves CurrentUser anonymous without `/me`; authenticated token bootstrap triggers exactly one `/me` hydration through `AuthTransport.getCurrentUser()` and relies on T-FE-025 for bearer injection.

### `GATE-FE-T138`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`,`SECURITY`
- evidence_summary: Gate is closed as `VERIFIED`. Evidence proves `/me` is called through the normal auth transport/generation path after authenticated token bootstrap, bearer injection comes from T-FE-025 with no manual header, current-user identity state is established separately from token-session state, generated `UserDetailDto` is adapted to immutable frontend-owned `CurrentUser`, roles/permissions are preserved from `/me`, CurrentUser is not persisted, `/me` `401` clears token material and resolves anonymous without logout/navigation, transient/network/5xx/unexpected non-401 failures preserve token material and resolve unavailable, no automatic retry/refresh behavior exists, stale exposed user is cleared for a new hydration attempt, and no generated/backend/OpenAPI/package/guard/logout/UI scope is modified.

### `T-FE-033` — Loading/error/retry pattern

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-09
- contract_summary: T-FE-033 owns only a reusable shared loading/error/retry presentation foundation. The supported state model is `ready`, `loading`, and `error`; empty/no-results/restricted states remain owned by `T-FE-035`. Loading uses explicit visible text plus `role="status"`, `aria-live="polite"`, and `aria-busy="true"`. Error presentation consumes the normalized frontend `NormalizedProblemDetails` shape from T-FE-019, not raw generated Problem Details DTOs, and presents only title/detail plus caller-provided generic fallback labels. Retry is an explicit user action emitted through the component output; no automatic retry policy exists.
- scope_summary: Implemented only the standalone shared component `frontend/src/app/shared/ui/loading-error-retry.ts`, its SCSS `frontend/src/app/shared/ui/loading-error-retry.scss`, focused tests `frontend/src/app/shared/ui/loading-error-retry.spec.ts`, and governance evidence. Styling uses existing runtime tokens and the T-FE-010 touch-target mixin only. No feature-specific copy, screen-specific layout, business-specific error interpretation, routing/navigation, auth/logout/session behavior, generated/backend/OpenAPI/package/dependency mutation, Angular Material dependency, shell/route registry/guard work, screen implementation, or screen approval task was introduced.
- verification_summary: Muse/free-worker repository clarification loaded the required authority but was blocked by directory-creation permission before code changes; OpenAI used a narrow direct fallback for the pre-authorized file scope. Focused TDD RED failed before `loading-error-retry.ts` existed with unresolved `./loading-error-retry` / `TS2307`. Focused GREEN passed 1 file / 7 tests. Full verification passed 15 files / 148 tests, `npm run lint`, `npm run lint:styles` after kebab-case class-selector correction, and `npm run quality` including dependency guard and production build. Focused tests prove loading semantics, error rendering from normalized error input, missing detail fallback behavior, explicit retry output with no automatic invocation, native keyboard-usable button, no empty-state behavior, no feature-specific copy, no auth/routing/business/generated behavior, and no timer-based automatic retry.

### `ST-FE-033` — Implement loading/error/retry reusable pattern

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Subtask implemented a small standalone Angular shared component with an explicit `LoadingErrorRetryState`, caller-owned labels, normalized error input, accessible loading/error semantics, and explicit retry output. It does not create a broad state-management framework.

### `GATE-FE-T033`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Gate evidence covers loading/error tests, accessibility semantics, explicit retry-only behavior, normalized frontend error input, absence of empty-state ownership, absence of feature/business copy, no routing/navigation/auth/logout behavior, no generated/backend/OpenAPI/package changes, full frontend verification, stylelint/lint/quality/build, git integrity checks, and scope review.

### `T-FE-034` — Form validation pattern

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_date: 2026-09-09
- contract_summary: T-FE-034 owns only a reusable shared validation presentation/interaction pattern over the frontend-owned `NormalizedProblemDetails` shape from T-FE-019 and the standard form-control foundation from T-FE-013. It may present normalized validation errors with caller-owned safe field labels/control IDs and caller-owned generic fallback copy. It must not invent validation timing, business rules, regex/password/email-domain rules, feature-specific mappings, auth forms, routing, screens, backend/OpenAPI behavior, generated API behavior, or feedback/live-region behavior owned by separate tasks.
- scope_summary: Implemented only `frontend/src/app/shared/ui/form-validation/` with pure mapping helpers, a standalone `np-form-validation-summary` component, external `.html`/`.scss`, a colocated focused spec, and barrel exports. Derived labels remove unsafe markup characters, generated control IDs are constrained to safe anchor characters, non-validation errors use caller-provided fallback text, and raw backend `traceId`, `code`, `title`, and `detail` are not displayed. `fieldKey` is retained only as an internal mapping/track key and is not rendered to users.
- verification_summary: Muse/free-worker implementation completed the bounded scope; independent Mimo/free-worker review returned PASS for scope, security/sanitization, accessibility, and component separation, including the `fieldKey` retention question. Fresh OpenAI final-gate verification passed focused form-validation tests (1 file / 8 tests), full frontend tests (18 files / 177 tests), `npm run lint`, `npm run lint:styles`, `npm run quality` including dependency guard and production build, `git diff --check`, and structural component-separation audit. No generated/backend/OpenAPI/package/dependency/routing/screen/auth form/T-FE-027/T-FE-036 files were changed.

### `ST-FE-034` — Implement backend validation display pattern

- status: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Subtask implemented a small reusable shared validation display foundation with deterministic field-error item mapping, caller-supplied labels/control IDs, sanitized derived fallback labels/control IDs, safe generic non-validation fallback presentation, accessible summary semantics, and programmatic focus. It does not create form timing policy or feature-specific validators.

### `GATE-FE-T034`

- status_result: `VERIFIED`
- blocker_types: `CONTRACT_CLARIFICATION`
- evidence_summary: Gate evidence covers validation/form focused tests, normalized Problem Details input handling, sanitization/no raw server trace-code-detail display, accessible persistent summary and field anchors, component separation with external template/style/spec, absence of timing/business/auth/routing/generated behavior, full frontend verification, lint/stylelint/quality/build, git diff checks, and scope review.

### `T-FE-040` — Authentication screen approval packet

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_date: 2026-09-11
- scope_summary: Completed the screen-family approval packet only. No Angular screen components, routes, navigation UI, Storybook stories/configuration, Penpot work, backend/OpenAPI mutation, dependency/package changes, screen implementation, push, or runtime behavior change occurred.
- contract_summary: `T-FE-040` owns the `AUTH-001..012` approval packet review. The family gate may close when every included screen has an explicit `approval_decision`; per-screen approval does not authorize implementation, and blocked screens may remain blocked with explicit reasons. Screen implementation remains governed by each screen's own task dependencies and gate evidence.
- approved_screens: `AUTH-001` Sign In, `AUTH-006` Verify email link, `AUTH-007` Forgot password, `AUTH-008` Reset password, `AUTH-009` Reset success state, `AUTH-010` Session expired, and `AUTH-011` Access denied are approved for later separately authorized implementation because route or non-routable state identity, access semantics, functional/API behavior, error/status handling, prerequisite clarification evidence, and existing visual foundations are sufficient. Penpot is not required before these screens merely as procedural duplication; post-implementation screen gates still require per-screen visual evidence. `AUTH-001` approval was added by the 2026-09-12 blocker-resolution contract below; it does not authorize implementation.
- v1_self_registration_contract: The technical lead resolved the public self-registration blockers on 2026-09-13 for `AUTH-002`, `AUTH-003`, `AUTH-004`, and `AUTH-005`. Public self-registration is approved only for `Nurse` and `Employer`; `Admin`, `SuperAdmin`, and privileged roles must not be anonymously assignable. Existing `POST /api/v1/auth/register` remains the permission-protected administrative user-registration operation and must not be repurposed. The backend must add separate public endpoints `POST /api/v1/auth/register/nurse` and `POST /api/v1/auth/register/employer`; public requests include only `email`, `password`, `firstName`, and `lastName`; public clients never submit `roleIds`; the server assigns `Nurse` or `Employer` from the endpoint. Registration success does not authenticate: no `AuthResult`, access token, refresh token, `AuthSessionBootstrap`, `CurrentUserStore` hydration, or auto-login. New accounts start with `EmailVerified = false`, and the backend initiates the initial verification email. Login for unverified accounts must not issue tokens and must return coded Problem Details code `email_verification_required`. Public duplicate-email outcomes must be enumeration-safe/generic. `AUTH-005` is a public informational check-your-email screen at `/auth/verify-email` with no backend call, resend, countdown/cooldown, session behavior, or account-existence disclosure. Backend implementation, canonical OpenAPI update, generated-client regeneration, and frontend implementation remain later phases and are not authorized by this docs-only checkpoint.
- approved_screens_update: `AUTH-002` Role Selection, `AUTH-003` Nurse Registration, `AUTH-004` Employer Registration, and `AUTH-005` Verify Email request/check-your-email are now approved for later sequential implementation in the V1 self-registration campaign after their backend/OpenAPI prerequisites are satisfied. This approval does not mark them implemented or verified.
- blocked_screens: `AUTH-012` Account Inactive remains blocked because `T-FE-054` found no stable coded inactive-account contract beyond generic login `401` and passive `/me.isActive`.
- auth_001_blocker_resolution_contract: The technical lead resolved the only remaining AUTH-001 blockers on 2026-09-12. Successful Sign In consumes the existing backend `AuthResult` (`accessToken`, `refreshToken`, `expiresAt`) from `C-AUTH-LOGIN`; the Sign In screen must not write `TokenStorage`, `sessionStorage`, or browser storage directly. It must hand the already successful `AuthResult` to the existing auth-session authority, which owns the smallest repository-consistent operation necessary to establish an authenticated token session. That operation stores token material through `TokenStorage`, preserves the current memory-only access token/expiry and session-storage refresh-token contract, transitions the resolved token-session state to `authenticated`, performs no refresh call, and does not duplicate startup bootstrap. If current implementation requires a small API extension, the behavior above is the contract and this checkpoint intentionally does not invent a method name.
- auth_001_current_user_contract: After token-session establishment succeeds, Sign In must trigger the existing `CurrentUserStore` hydration workflow to obtain canonical `/api/v1/me` identity, roles, and permissions for same-session downstream `T-FE-031` and `T-FE-032` behavior. Sign In must not parse JWT roles or permissions, call `/me` through a second custom implementation, duplicate `CurrentUserStore`, or own current-user state. Ordering is: backend sign-in succeeds; establish authenticated token session through the existing session authority; hydrate `CurrentUserStore` through its existing workflow; resolve the approved post-login destination; navigate using Angular Router. If token-session establishment succeeds but `/me` hydration reaches existing `unavailable` due to a non-authentication failure, preserve the authenticated token session, do not clear valid token material merely because `/me` is temporarily unavailable, do not fabricate roles/permissions, and do not classify the failure as invalid credentials.
- auth_001_return_url_contract: After successful session establishment and current-user hydration, a present `returnUrl` query parameter may be used only when it passes the existing `T-FE-030` safe-return contract/helper. Sign In must not duplicate URL validation, manually trust the raw query parameter, or allow external, protocol-relative, scheme-bearing, backslash-confused, control-character, empty, or non-path destinations. If `returnUrl` is absent, empty, invalid, or rejected, the canonical V1 fallback is `ACCOUNT_OVERVIEW`, currently `/account`, obtained from the canonical route registry. `/account` is selected because it is an approved `AUTHENTICATED` route, has `T-FE-031` `AUTHENTICATED_ONLY` policy, is actor-neutral, requires no Nurse/Employer/Admin role choice, does not introduce role-home logic, and does not reinterpret `ROOT_ENTRY`. AUTH-001 V1 must not infer role-specific fallbacks such as Admin -> `/admin`, Nurse -> `/nurse`, Employer -> `/employer`, must not route successful login to `/auth/role-selection` unless future product authority requires it, and must not make `ROOT_ENTRY` own role-home selection.
- auth_001_invalid_credentials_contract: Backend authentication failure must not establish a local session. On unsuccessful Sign In, do not persist returned or partial token material, do not transition `AuthSessionBootstrap` to `authenticated`, do not hydrate `CurrentUserStore` as if login succeeded, and remain in the Sign In workflow while presenting backend-authorized error behavior through the existing Auth API / Problem Details contract. Do not invent backend error codes or messages.
- auth_001_route_visual_contract: `AUTH_SIGN_IN` remains `PUBLIC` under `T-FE-030`; no anonymous-only guard or authenticated-user redirect away from Sign In is approved in V1. Existing T-FE-040 visual-readiness evidence is sufficient for AUTH-001 after this functional/security contract resolution. Legacy Page 10/Auth material remains reusable historical/reference evidence only, not active approved visual authority, and must not override current tokens, accessibility, responsive, or backend/access contracts. No materially new visual intent or separate unresolved material visual decision is recorded for AUTH-001.
- visual_readiness_summary: Existing approved visual foundations, Material theme bridge, standard form controls, form validation pattern, loading/error/retry pattern, responsive helpers, RTL/LTR direction foundation, accessibility rules, and reusable authentication/core visual precedent are sufficient for the approved routine Auth screens including AUTH-001 after the 2026-09-12 blocker-resolution contract. Legacy Page 10/Auth material remains reusable historical/reference evidence only; it is not active approved visual authority and must not override current tokens, accessibility, responsive, or backend/access contracts. No materially new visual intent was found for the approved routine screens. Remaining blocked screens are blocked by functional/security/product contract gaps, not by a mandatory Penpot recreation requirement.
- boundary_summary: `T-FE-030` remains the owner of `PUBLIC`/`ENTRY`/`AUTHENTICATED`, anonymous redirects to `AUTH_SIGN_IN`, `returnUrl`, and safe-return validation. `T-FE-031` remains the owner of route-level role/permission UX and canonical `SYSTEM_ACCESS_DENIED` navigation for ready policy failure. `T-FE-040` does not redesign those foundations. PUBLIC auth/system routes remain accessible to authenticated users under V1; `SYSTEM_SESSION_EXPIRED` remains PUBLIC without automatic expiry routing; `SYSTEM_ACCESS_DENIED` remains PUBLIC.
- first_screen_recommendation: `AUTH-001` is now screen-approved for later implementation authorization after the 2026-09-12 blocker-resolution contract. This approval does not implement or authorize implementation of the Sign In screen, but it removes the prior recommendation to skip AUTH-001 solely because login-success handoff and fallback were unresolved.

### `ST-FE-040` — Prepare Authentication screen approval packet

- status: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Subtask completed the explicit Authentication family decision packet. All `AUTH-001..012` screens now have explicit approval decisions in the Screen Ownership Matrix; approved screens and blocked screens have evidence-backed rationale, visual/Penpot requirements are classified, and no screen implementation or visual-tooling/source mutation occurred. The later 2026-09-12 AUTH-001 blocker-resolution contract updates AUTH-001 from BLOCKED to APPROVED without changing other Auth screen decisions.

### `GATE-FE-T040`

- status_result: `VERIFIED`
- blocker_types: `DESIGN`
- evidence_summary: Gate evidence satisfies the Auth approval packet requirement: every included Auth/System screen has an explicit decision; approved screens have route/state identity, access semantics, functional/API contracts, status/error behavior, and sufficient existing visual authority; blocked screens preserve explicit blockers without invented behavior; Storybook is not treated as authority; draft Penpot/Auth evidence is not silently promoted; and implementation remains unauthorized until a separate screen task is approved. The later 2026-09-12 AUTH-001 blocker-resolution contract supplies the missing successful-login token-session handoff, current-user hydration, safe-return consumption, and `ACCOUNT_OVERVIEW` `/account` fallback authority; therefore AUTH-001 is APPROVED for future implementation authorization.

### `T-FE-041` — `AUTH-001` Sign In

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-12
- scope_summary: AUTH-001 Sign In implementation and visual calibration are complete and accepted. Human visual review was granted as APPROVED on 2026-09-12, closing the outstanding visual-review requirement. Current closure scope is limited to the approved AUTH-001 implementation/visual files, the minimal route/session/test support files, approved app-shell/body overflow fixes, this ledger, and `PROGRESS.md`. No other Auth screen, navigation UI, backend/OpenAPI/generated/package/dependency file, or Storybook configuration change is included.
- functional_freeze_summary: Functional behavior remains frozen during visual calibration. The verified successful-login order remains `AuthTransport.login(LoginCommand)` -> `AuthSessionBootstrap.establishAuthenticatedSession(AuthResult)` -> `CurrentUserStore.hydrate()` -> safe navigation; failed login remains in the Sign In workflow without establishing a session; `returnUrl`, `ACCOUNT_OVERVIEW` fallback, route/access policy, TokenStorage semantics, CurrentUserStore semantics, and `/me` flow were preserved.
- visual_calibration_summary: Desktop/tablet/mobile visual calibration is complete using approved foundations and authoritative text identity `Nursing Platform`. The pass improved desktop composition, product hierarchy, typography, CTA treatment, background treatment, responsive single-column behavior, and form/error spacing without introducing an invented logo/mark/tagline.
- overflow_evidence: Production-route desktop, tablet, and mobile measurements all reported `hasHorizontal: false` and `hasVertical: false`. The app-shell horizontal overflow root cause was corrected through the approved `.np-app-shell { box-sizing: border-box; }` change, and unnecessary vertical overflow from browser-default body margin was corrected through the approved `body { margin: 0; }` change. No `overflow-x: hidden` or broad global reset was added.
- verification_summary: Automated verification passed for focused Sign In tests (8/8), auth-session-bootstrap tests (13/13), full frontend tests (26 files / 285 tests), dependency guard, lint, stylelint, `npm run quality` with only the known initial bundle budget warning, production build, `npm run build-storybook`, production-route desktop/tablet/mobile overflow metrics, and `git diff --check`. Human visual review is APPROVED; no T-FE-041 blocker remains.
- closure_summary: `T-FE-041` is closed as `VERIFIED` for AUTH-001 only. Minor future polish opportunities are non-blocking and not part of this closure scope. Do not infer authorization for sibling Auth screens, redesign, unrelated polish, push, or next-task work from this closure.

### `ST-FE-041` — Build `AUTH-001` Sign In route/form after approval

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Implementation and visual calibration are complete with automated functional, quality, Storybook, and production overflow evidence recorded. Human visual review is APPROVED, closing the subtask for AUTH-001 only.

### `GATE-FE-T041`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate evidence covers Sign In focused behavior, auth session handoff, CurrentUser hydration ordering, safe navigation contract preservation, per-screen desktop/tablet/mobile screenshots, clean desktop/tablet/mobile overflow metrics, global overflow root-cause fixes, full frontend quality verification, Storybook static build, Angular CLI drift absence, scope review, and human visual review APPROVED. `GATE-FE-T041` is closed as `VERIFIED`; no T-FE-041 blocker remains.

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

### `T-FE-047` — `AUTH-006` Verify Email Link

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-13
- scope_summary: AUTH-006 Verify Email Link implementation is complete at worker level for AUTH-006 only. Scope is limited to the AUTH-006 feature files, minimal lazy route activation, focused route/test expectation updates, and this ledger. At the time of AUTH-006 implementation, AUTH-005 Send verification email was BLOCKED and NOT STARTED; no send-verification-email route/component/story/adapter was implemented. The later 2026-09-13 V1 self-registration contract supersedes only AUTH-005's approval status and does not change the historical AUTH-006 implementation scope. No sibling Auth screen implementation, navigation UI, auth/session foundation behavior, CurrentUserStore behavior, TokenStorage behavior, backend/OpenAPI/generated API mutation, dependency change, package change, or Storybook configuration change occurred.
- functional_contract_summary: AUTH-006 uses `C-AUTH-VERIFY`: `POST /api/v1/auth/verify-email`, operation `VerifyEmail`, request `VerifyEmailRequest { token }`, anonymous/public success `200`. The feature-local `VerifyEmailApi` adapter delegates to the generated `verifyEmail` function without editing generated files. The screen reads the exact `token` query parameter as opaque input, trims only for missing/empty detection, never persists/decodes/displays/logs the token, blocks the backend call when it is absent or empty/whitespace, and calls the adapter exactly once for a non-empty token. Success renders the safe status `Email verified successfully.` with `role="status"`; missing token renders `This verification link is invalid or missing.` with `role="alert"`; backend failures render authoritative detail or `Email verification failed.` with `role="alert"`. No automatic sign-in/session/current-user/token/logout/navigation, no resend/retry/recovery CTA, and no invented copy/CTA/navigation was added.
- route_summary: `AUTH_VERIFY_EMAIL_CONFIRM` is activated at canonical `/auth/verify-email/confirm` via lazy `loadComponent`. PUBLIC route semantics are preserved: no `canActivate`, no `redirectTo`, no guard, no permission metadata, and no AUTH-005 `/auth/verify-email` route activation.
- visual_summary: AUTH-006 reuses the approved AUTH-007/AUTH-008 Auth-family visual language as visual/pattern precedent only. Storybook evidence renders the production component states through a mocked `VerifyEmailApi` provider with no Storybook configuration change. Render-ready desktop/tablet/mobile screenshots were captured from the static Storybook Default story after DOM readiness verified `np-verify-email`, visible `Verify email`, visible source-authoritative success status `Email verified successfully.`, no displayed opaque token, and no component CTA/link/button. Current artifacts are `/tmp/opencode/auth-006-verify-email-desktop.png`, `/tmp/opencode/auth-006-verify-email-tablet.png`, and `/tmp/opencode/auth-006-verify-email-mobile.png`.
- verification_summary: RED evidence first failed before production implementation with missing `VerifyEmail` / `VerifyEmailApi` modules. Focused GREEN passed AUTH-006 component/API tests plus route-regression tests (3 files / 21 tests). Full frontend tests passed 34 files / 319 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, `npm run quality` (including production build with `verify-email` lazy chunk), `npm run build-storybook` (including `verify-email.stories` chunk), backend-source success-message check, DOM readiness validation, Storybook screenshot capture, and `git diff --check` passed.

### `ST-FE-047` — Build `AUTH-006` Verify Email Link

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Subtask implementation is complete for AUTH-006 only. The production component uses external `.html` and `.scss`, a colocated focused spec, feature-local generated-API adapter, and bounded Storybook stories rendering the production component. Tests cover missing/empty token blocking without backend calls, exact opaque request DTO with single call, success status without token exposure, safe error without token exposure, PUBLIC lazy route activation without guard/redirect, absence of AUTH-005/send-verification behavior, and absence of auth/session/token-storage/CurrentUserStore/navigation behavior. AUTH-005 was blocked and not started during AUTH-006 implementation; later V1 self-registration approval does not retroactively add AUTH-005 behavior to AUTH-006.

### `GATE-FE-T047`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate is closed as `VERIFIED` for AUTH-006 only. Evidence includes AUTH-006 functionality, PUBLIC lazy route activation, API adapter verification, route regression without AUTH-005 activation, full frontend verification, lint/style/dependency/quality/build/build-storybook, source-authoritative success message confirmation, DOM readiness validation, render-ready desktop/tablet/mobile screenshot artifact capture, and `git diff --check`. AUTH-005 was blocked by CONTRACT_CLARIFICATION/SECURITY and explicitly out of scope for AUTH-006 at that time; the later 2026-09-13 V1 self-registration contract now approves AUTH-005 as a separate not-started screen.

### `T-FE-048` — `AUTH-007` Forgot Password

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-12
- scope_summary: AUTH-007 Forgot Password implementation is complete, automated verification passed, narrowed Big Pickle read-only review passed with no Critical/High/Medium findings, valid render-ready desktop/tablet/mobile Storybook visual evidence exists, and human visual review is APPROVED. Scope is limited to the AUTH-007 feature files, minimal lazy route activation, focused route/test expectation updates, this ledger, and `PROGRESS.md`. No AUTH-008 Reset Password, sibling Auth screen implementation, navigation UI, auth/session foundation behavior, backend/OpenAPI/generated API mutation, dependency change, or Storybook configuration change occurred.
- functional_contract_summary: AUTH-007 uses `C-AUTH-FORGOT`: `POST /api/v1/auth/forgot-password`, operation `ForgotPassword`, request `ForgotPasswordRequest { email }`, and success `200`. The feature-local API adapter delegates to the generated `forgotPassword` function without editing generated files. The screen sends only the email field, performs only required/email-format form validation, shows the approved generic no-account-enumeration success message (`If the email exists, a password reset link has been sent.`), remains in the workflow for recoverable errors, and does not navigate to AUTH-008 or claim account existence/email delivery.
- route_summary: `AUTH_FORGOT_PASSWORD` is activated at canonical `/auth/forgot-password` via lazy `loadComponent`. PUBLIC route semantics are preserved: no anonymous-only guard, no authenticated-user redirect, no `canActivate`, no route-permission policy change, and no auth/session/current-user behavior.
- visual_summary: AUTH-007 reuses the approved AUTH-001 Auth-family visual language as a visual/pattern precedent only: full-page Auth composition, card proportions, restrained branded context panel, typography hierarchy, spacing rhythm, primary CTA treatment, responsive behavior, logical CSS, and accessibility patterns. Penpot was not required. Initial desktop/tablet Storybook screenshots captured the loader race and are superseded/not accepted as evidence. Regenerated Storybook screenshots were captured only after DOM readiness verified visible `Forgot password` heading, visible email field, visible `Send reset link` primary action, and no visible Storybook loader/spinner. Current visual evidence artifacts are `/tmp/opencode/auth-007-desktop.png`, `/tmp/opencode/auth-007-tablet.png`, and `/tmp/opencode/auth-007-mobile.png`; desktop, tablet, and mobile are rendered and human-approved. Human visual approval: `APPROVED`.
- verification_summary: RED evidence first failed before production implementation with missing `ForgotPassword` / `ForgotPasswordApi` modules. Focused GREEN passed AUTH-007 component tests (1 file / 7 tests) and feature-local API tests (1 file / 1 test). Focused route regression passed `canonical-routes.spec.ts` (1 file / 13 tests). AUTH-001 focused regression passed `sign-in.spec.ts` (1 file / 8 tests). Full frontend tests passed 28 files / 293 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, `npm run quality`, `npm run build`, and `npm run build-storybook` passed. Initial `npm run quality` exposed a new AUTH-007 component-style budget warning; the warning was resolved by removing nonessential decorative orbit styling from AUTH-007 only and rerunning focused tests plus quality/build successfully. Independent review status: MiMo review timed out before verdict; first Big Pickle attempt failed because it attempted an unapproved command; narrowed Big Pickle read-only review passed with no Critical/High/Medium findings.

### `ST-FE-048` — Build `AUTH-007` Forgot Password

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Subtask implementation is complete and human visual approval is APPROVED. The production component uses external `.html` and `.scss`, a colocated focused spec, feature-local generated-API adapter, and a bounded Storybook story rendering the production component. Tests cover rendering, validation, exact request DTO, duplicate-submit prevention, success, failure, route activation, PUBLIC semantics, and absence of auth/session/reset-password behavior.

### `GATE-FE-T048`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate is closed as `VERIFIED` for AUTH-007 only. Evidence includes AUTH-007 functionality, route activation, Storybook rendering, route regression, AUTH-001 regression, full frontend verification, lint/style/dependency/quality/build/build-storybook, narrowed Big Pickle read-only review PASS, render-ready desktop/tablet/mobile screenshot artifact capture, and human visual review APPROVED. Earlier loader-only desktop/tablet captures were superseded and are not accepted evidence.

### `T-FE-049` — `AUTH-008` Reset Password

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-12
- scope_summary: AUTH-008 Reset Password implementation is complete and verified. Scope is limited to the AUTH-008 feature files, minimal lazy route activation, focused route/test expectation updates, this ledger, and `PROGRESS.md`. No AUTH-009 Reset Success route/component/story/redirect/success-state UI, sibling Auth screen implementation, navigation UI, auth/session foundation behavior, CurrentUserStore behavior, backend/OpenAPI/generated API mutation, dependency change, package change, or Storybook configuration change occurred.
- functional_contract_summary: AUTH-008 uses `C-AUTH-RESET`: `POST /api/v1/auth/reset-password`, operation `ResetPassword`, request `ResetPasswordRequest { email, token, newPassword }`, and success `200`. The feature-local API adapter delegates to the generated `resetPassword` function without editing generated files. The screen reads the exact `token` query parameter as opaque reset-flow input, trims only for missing/empty detection, never stores it, blocks submission and backend calls when it is absent or empty, and offers recovery through the canonical `AUTH_FORGOT_PASSWORD` route. For non-empty tokens, backend remains authoritative for invalid, expired, consumed, revoked, or otherwise rejected tokens. The form fields are exactly email and new password; no confirm-password field or additional password rule is introduced. Client validation mirrors the backend validator only: email required/format, newPassword required/minimum length 8/uppercase/digit.
- auth_009_boundary_summary: T-FE-049 implements only reset-password submission and in-workflow blocking/error behavior. `AUTH-009` / `T-FE-050` remains a separate reset-success state after `GATE-FE-T049`; no `AUTH_RESET_PASSWORD_SUCCESS` route, `/auth/reset-password/success`, reset-success component, reset-success story, success redirect, or AUTH-009 UI/copy/title was implemented in T-FE-049.
- route_summary: `AUTH_RESET_PASSWORD` is activated at canonical `/auth/reset-password` via lazy `loadComponent`. PUBLIC route semantics are preserved: no anonymous-only guard, no authenticated-user redirect, no `canActivate`, no route-permission policy change, and no auth/session/current-user behavior.
- visual_summary: AUTH-008 reuses the approved AUTH-001/AUTH-007 Auth-family visual language as visual/pattern precedent only: full-page Auth composition, card proportions adapted for two form controls, restrained branded context panel, typography hierarchy, spacing rhythm, primary CTA treatment, responsive behavior, logical CSS, and accessibility patterns. Penpot was not required. Storybook evidence renders the production component. Render-ready desktop/tablet/mobile screenshots were captured only after DOM readiness verified visible `Reset password` heading, visible `Email address` control, visible `New password` control, `<body>` in `sb-show-main`, and no `<body>` `sb-show-preparing-story` or `sb-show-errordisplay`. Current artifacts are `/tmp/opencode/auth-008-desktop.png`, `/tmp/opencode/auth-008-tablet.png`, and `/tmp/opencode/auth-008-mobile.png`.
- verification_summary: RED evidence first failed before production implementation with missing `ResetPassword` / `ResetPasswordApi` modules. Focused GREEN passed AUTH-008 component/API tests (2 files / 11 tests). Focused route and AUTH-001/AUTH-007 regressions passed (4 files / 29 tests). Full frontend tests passed 30 files / 304 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, `npm run quality`, `npm run build`, `npm run build-storybook`, and `git diff --check` passed. An initial component-style budget warning on AUTH-008 SCSS was resolved within AUTH-008 scope and rerun clean. Independent MiMo review passed with no Critical/High/Medium findings; two Low cleanup items were addressed by merging duplicate SCSS selector blocks and adding a production-component missing-token Storybook state.

### `ST-FE-049` — Build `AUTH-008` Reset Password

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Subtask implementation is complete. The production component uses external `.html` and `.scss`, a colocated focused spec, feature-local generated-API adapter, and bounded Storybook stories rendering the production component. Tests cover rendering, exact token query behavior, missing/empty token blocking without backend calls, authoritative validation, exact request DTO, duplicate-submit prevention, failure handling, route activation, PUBLIC semantics, and absence of auth/session/token-storage/CurrentUserStore/AUTH-009 behavior.

### `GATE-FE-T049`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate is closed as `VERIFIED` for AUTH-008 only. Evidence includes AUTH-008 functionality, route activation, API adapter verification, route regression, AUTH-001/AUTH-007 regression, full frontend verification, lint/style/dependency/quality/build/build-storybook, independent MiMo review PASS with no Critical/High/Medium findings, and render-ready desktop/tablet/mobile screenshot artifact capture. `AUTH-009` remains separate and is not implemented by this gate.

### `T-FE-050` — `AUTH-009` Reset Success

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-12
- scope_summary: AUTH-009 Reset Success State implementation is complete and verified. Scope is limited to the existing reset-password workflow production component/template/styles/spec/story, this ledger, and `PROGRESS.md`. No independent `AUTH_RESET_PASSWORD_SUCCESS` route, `/auth/reset-password/success`, success redirect, sign-in CTA, automatic sign-in, auth/session foundation behavior, CurrentUserStore behavior, TokenStorage behavior, sibling Auth screen implementation, navigation UI, backend/OpenAPI/generated API mutation, dependency change, package change, or Storybook configuration change occurred.
- functional_contract_summary: AUTH-009 uses the already-verified `C-AUTH-RESET` success path from AUTH-008. After the existing reset-password API submission succeeds, the screen renders the exact success message `Password has been reset successfully.` as an in-workflow state. The message is not used as a route, session/auth token, redirect trigger, or additional API call. The reset token remains opaque reset-flow input only and is not stored or interpreted.
- route_summary: `AUTH_RESET_PASSWORD_SUCCESS` remains `NOT_ROUTABLE` and is presented inside the `AUTH_RESET_PASSWORD` workflow. Repository route inspection and focused tests confirm no `/auth/reset-password/success` route exists and no navigation occurs on success.
- accessibility_summary: The success message is rendered through the production template with `role="status"`. No CTA or focus-moving behavior was invented.
- visual_summary: AUTH-009 reuses the approved AUTH-001/AUTH-007/AUTH-008 Auth-family visual language as visual/pattern precedent only. Storybook evidence renders the existing production component Success state by filling and submitting the real reset-password form through the existing story fixture; no Storybook addon/config/tooling expansion and no artificial component API were added. Render-ready desktop/tablet/mobile screenshots were captured only after DOM readiness verified visible `Password has been reset successfully.`, `role="status"`, `<body>` class `sb-show-main`, no `<body>` preparing/error class, and no success route. Current artifacts are `/tmp/opencode/auth-009-success-desktop.png`, `/tmp/opencode/auth-009-success-tablet.png`, and `/tmp/opencode/auth-009-success-mobile.png`.
- verification_summary: RED evidence first failed before production implementation because the success message was absent. Focused GREEN passed AUTH-008/AUTH-009 reset workflow component/API tests (2 files / 12 tests). Relevant Auth/route regressions passed (5 files / 40 tests). Full frontend tests passed 30 files / 305 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, `npm run quality`, `npm run build`, `npm run build-storybook`, and `git diff --check` passed. Independent MiMo review passed with no Critical/High/Medium findings; three Low informational notes required no correction for AUTH-009 closure.

### `ST-FE-050` — Build `AUTH-009` Reset Success

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Subtask implementation is complete. The production reset-password component now renders the non-routable reset-success state after successful reset submission. Tests cover the exact success message, `role="status"`, no navigation, no success route, exact request DTO preservation, and absence of auth/session/token-storage/CurrentUserStore behavior.

### `GATE-FE-T050`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate is closed as `VERIFIED` for AUTH-009. Evidence includes reset-success state behavior, non-routable route preservation, accessibility state evidence, bounded Storybook production-component Success state, full frontend verification, lint/style/dependency/quality/build/build-storybook, independent MiMo review PASS with no Critical/High/Medium findings, and render-ready desktop/tablet/mobile screenshot artifact capture.

### `T-FE-051` — `AUTH-010` Session Expired

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-12
- scope_summary: AUTH-010 Session Expired implementation is complete and verified. Scope is limited to the static `SessionExpired` production component, external template and styles, colocated focused spec and Storybook story, canonical `/session-expired` lazy route activation, focused route-regression assertions, this ledger, and `PROGRESS.md`. No automatic session-expiry routing, route guard, redirect, token clearing, logout behavior, TokenStorage behavior, AuthSessionBootstrap mutation, CurrentUserStore behavior, backend 401/403 interception logic, permission logic, navigation UI, sibling Auth screen implementation, backend/OpenAPI/generated API mutation, dependency change, package change, or Storybook configuration change occurred.
- route_summary: `SYSTEM_SESSION_EXPIRED` is activated at canonical `/session-expired` via lazy `loadComponent`. PUBLIC semantics are preserved: no `canActivate`, no `redirectTo`, no permission metadata, and no automatic routing trigger was added.
- copy_summary: The only screen-specific visible content is the authoritative screen identity `Session expired`. No explanatory text, sign-in CTA, recovery instruction, secondary action, or destination was invented. The visible `Nursing Platform` label and decorative context treatment reuse the approved Auth-family visual pattern as brand treatment only.
- visual_summary: AUTH-010 reuses the approved AUTH-001/AUTH-007/AUTH-008/AUTH-009 Auth-family visual language as visual/pattern precedent. Storybook evidence renders the production component without interaction tooling or config changes. Render-ready desktop/tablet/mobile screenshots were captured only after DOM readiness verified `np-session-expired`, visible `Session expired`, `<body>` class `sb-show-main`, no `<body>` preparing/error class, and no component CTA/link/button. Current artifacts are `/tmp/opencode/auth-010-session-expired-desktop.png`, `/tmp/opencode/auth-010-session-expired-tablet.png`, and `/tmp/opencode/auth-010-session-expired-mobile.png`.
- verification_summary: RED evidence first failed before production implementation with missing `SessionExpired` module/template. Focused GREEN passed AUTH-010 component and route-regression tests (2 files / 16 tests). Relevant Auth/route regressions passed 8 files / 76 tests. Full frontend tests passed 31 files / 308 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, `npm run quality`, `npm run build`, `npm run build-storybook`, and `git diff --check` passed after targeted mechanical lint/style fixes. Initial MiMo review timed out without verdict; fallback Big Pickle read-only review passed with no Critical/High/Medium findings and one informational Low note requiring no correction.

### `ST-FE-051` — Build `AUTH-010` Session Expired

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Subtask implementation is complete. The production component renders the static terminal `Session expired` screen, route activation uses canonical `/session-expired`, focused tests prove no CTA/link/button/routerLink, no guards/redirects, and no auth/session/token/current-user/logout/API behavior.

### `GATE-FE-T051`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate is closed as `VERIFIED` for AUTH-010. Evidence includes session-expired focused tests, route activation and PUBLIC/no-guard/no-redirect preservation, per-screen Storybook visual evidence, full frontend verification, lint/style/dependency/quality/build/build-storybook, fallback Big Pickle read-only review PASS with no Critical/High/Medium findings, and render-ready desktop/tablet/mobile screenshot artifact capture.

### `T-FE-053` — `AUTH-011` Access Denied

- status: `VERIFIED`
- blocker_types: —
- evidence_date: 2026-09-13
- scope_summary: AUTH-011 Access Denied implementation is complete and verified at worker level. Scope is limited to the static `AccessDenied` production component, external template and styles, colocated focused spec and Storybook story, canonical `/access-denied` lazy route activation, focused route-regression assertion updates, and this ledger. No permission checks, route-policy edits, route guard attachment, backend 403 interception, automatic redirects, auth/session/token/current-user/logout behavior, navigation UI, sibling Auth screen implementation, backend/OpenAPI/generated API mutation, dependency change, package change, or Storybook configuration change occurred.
- route_summary: `SYSTEM_ACCESS_DENIED` is activated at canonical `/access-denied` via lazy `loadComponent`. PUBLIC semantics are preserved: no `canActivate`, no `redirectTo`, no permission metadata, and no automatic routing trigger was added.
- copy_summary: The only screen-specific visible content is the authoritative screen identity `Access denied`. No CTA, link, button, routerLink, sign-in/retry/support instruction, or extra product copy was invented. The visible `Nursing Platform` label and decorative context treatment reuse the approved Auth-family visual pattern as brand treatment only.
- visual_summary: AUTH-011 reuses the approved AUTH-010 Session Expired Auth-family visual language as visual/pattern precedent. Storybook evidence renders the production component only with fullscreen layout and no config changes. Render-ready desktop/tablet/mobile screenshots were captured from the static Storybook Default story after DOM readiness verified `np-access-denied`, visible `Access denied`, rendered `#storybook-root` content, and no component CTA/link/button. Current artifacts are `/tmp/opencode/auth-011-access-denied-desktop.png`, `/tmp/opencode/auth-011-access-denied-tablet.png`, and `/tmp/opencode/auth-011-access-denied-mobile.png`.
- verification_summary: RED evidence first failed before production implementation with `TS2307: Cannot find module './access-denied'`. Focused GREEN passed AUTH-011 component and route-regression tests (2 files / 16 tests). Full frontend tests passed 32 files / 311 tests. `npm run lint`, `npm run lint:styles`, `npm run check:dependencies`, `npm run quality` (including production build with `access-denied` lazy chunk), `npm run build-storybook` (including `access-denied.stories` chunk), DOM readiness validation, Storybook screenshot capture, and `git diff --check` passed.

### `ST-FE-053` — Build `AUTH-011` Access Denied

- status: `VERIFIED`
- blocker_types: —
- evidence_summary: Subtask implementation is complete. The production component renders the static terminal `Access denied` screen, route activation uses canonical `/access-denied`, focused tests prove no CTA/link/button/routerLink, no guards/redirects, and no auth/session/token/current-user/logout/API behavior.

### `GATE-FE-T053`

- status_result: `VERIFIED`
- blocker_types: —
- evidence_summary: Gate is closed as `VERIFIED` for AUTH-011. Evidence includes access-denied focused tests, route activation and PUBLIC/no-guard/no-redirect preservation, per-screen Storybook production-component story, full frontend verification, lint/style/dependency/quality/build/build-storybook, DOM readiness validation, and render-ready desktop/tablet/mobile screenshot artifact capture. No independent delegated review was invoked for this bounded screen because the implementation is a direct static-screen analogue of AUTH-010 and final-gate targeted review found no material issue.

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

### `T-FE-139` — Component separation remediation

- status: `VERIFIED`
- blocker_types: `SCOPE`
- evidence_date: 2026-09-09
- scope_summary: Structural-only remediation for the two existing Angular component-separation violations recorded after the governance rule was strengthened: `frontend/src/app/shared/ui/loading-error-retry.ts` inline production template and `frontend/src/app/app.html` template-local `<style>` block. No UI redesign, behavior change, shell/routing/guard/screen work, backend/OpenAPI/generated-client/package/dependency mutation, staging, commit, or push occurred.
- implementation_summary: `LoadingErrorRetry` now uses `templateUrl: './loading-error-retry.html'` with the former inline markup extracted to colocated `frontend/src/app/shared/ui/loading-error-retry.html`; the existing external `styleUrl: './loading-error-retry.scss'` remains in use. `frontend/src/app/app.html` no longer contains a template-local `<style>` block; the moved scaffold/app styles now live in colocated `frontend/src/app/app.scss` with only stylelint-required normalization. Focused structural coverage was added to `frontend/src/app/shared/ui/loading-error-retry.spec.ts`.
- preservation_summary: T-FE-033 remains historically `VERIFIED`; T-FE-139 does not retroactively invalidate that task. Loading, error, retry, ready-state projection, accessibility semantics, native-button retry behavior, normalized Problem Details usage, and no feature/business/routing/auth/generated coupling remain preserved by the focused component tests.
- verification_summary: Focused LoadingErrorRetry test first failed RED because `loading-error-retry.html` did not exist, then passed GREEN with 1 file / 8 tests. Full frontend verification passed 15 files / 149 tests. `npm run lint`, `npm run lint:styles`, `npm run quality` including production build, and `git diff --check` passed. Targeted structural audit found no inline production Angular `template:`, no inline production `styles:`, no template-local `<style>` block in the remediated production files or broader `frontend/src/app` production scope excluding `.spec.ts`; `LoadingErrorRetry` external HTML/SCSS metadata was confirmed. Staged area remained empty.
- scope_evidence: Git-guardian review `T-FE-139-GIT-GUARDIAN-SKILL-CORRECTION-2026-09-09` returned `PASS` with compliant skill preflight. Dirty scope was limited to `PROGRESS.md`, `docs/frontend/execution/frontend-implementation-ledger.md`, `frontend/src/app/app.html`, `frontend/src/app/app.scss`, `frontend/src/app/shared/ui/loading-error-retry.spec.ts`, `frontend/src/app/shared/ui/loading-error-retry.ts`, and new untracked `frontend/src/app/shared/ui/loading-error-retry.html`. No backend, OpenAPI, generated API, package/dependency, routing, shell, screen, unrelated component, database/migration, or `.opencode` paths changed. `docs.zip` remained untracked and outside project scope.
- closure_evidence: T-FE-139 is closed as `VERIFIED` and ready for atomic commit `refactor(frontend): separate component templates and styles`; do not push or start another frontend task.

### `ST-FE-139` — Move recorded inline/template-local component content to external files

- status: `VERIFIED`
- blocker_types: `SCOPE`
- evidence_summary: Recorded inline production component template and template-local app style block were moved to colocated external files without behavior, accessibility, routing, shell, screen, generated-client, backend, OpenAPI, package, or dependency changes.
- closure_evidence: Subtask accepted as structurally complete from focused RED/GREEN component tests, full frontend verification, structural source audit, and git-guardian PASS.

### `GATE-FE-T139`

- status_result: `VERIFIED`
- blocker_types: `SCOPE`
- evidence_summary: Gate evidence is recorded: structural component-separation audit PASS, focused LoadingErrorRetry 1 file / 8 tests PASS, full frontend 15 files / 149 tests PASS, lint/stylelint/quality/build PASS, `git diff --check` PASS, staged area empty, git-guardian scope review PASS, and `docs.zip` remained untracked/out of scope.
- closure_evidence: Gate is closed as `VERIFIED`; T-FE-033 remains historically `VERIFIED` with no retroactive invalidation. Stop for review; do not stage, commit, push, or start the next task without explicit instruction.
