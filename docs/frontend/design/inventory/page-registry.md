# Frontend Page Registry and Canonical Route Contract

```yaml
document_id: NPS-DES-INV-PAGE-REGISTRY
status: APPROVED_CANONICAL_ROUTE_AUTHORITY
created_at: 2026-09-10
scope: documentation_contract_definition_only
implementation_authorization: false
```

## 1. Purpose and authority

This document is the approved canonical documentation authority for frontend page and route identity decisions. It records:

1. the **approved route design policy** supplied by the technical lead for documentation and contract-definition work; and
2. the **approved canonical route inventory** accepted by the technical lead.

The policy in Section 2 is approved. The exact route table in Section 4 is now the **APPROVED CANONICAL ROUTE AUTHORITY** for route IDs and path/path-template identity. This contract approval does not authorize Angular implementation.

This document does not implement Angular routes. `frontend/src/app/app.routes.ts` remains the current runtime route configuration and still exports an empty `Routes` array. `T-FE-029`, `ST-FE-029`, and `GATE-FE-T029` must not be marked `VERIFIED` from this documentation checkpoint.

## 2. Approved route design policy

### 2.1 URL philosophy

Frontend URLs must be semantic, human-readable, stable, based on durable product/user concepts, and independent of Angular component names, Penpot object IDs, internal screen IDs, and backend API endpoint paths.

### 2.2 Product-domain grouping

Canonical routes should use stable product-domain families where product evidence supports a family. The currently evidenced families are `auth`, `nurse`, `employer`, `exams`, `preparation-packages`, `commerce`, `account`, `admin`, and `system`.

### 2.3 Path naming

Canonical path segments must use lowercase kebab-case, avoid implementation terminology, avoid component/class names, avoid internal task or screen identifiers, and avoid arbitrary abbreviations.

### 2.4 Hierarchy

Prefer shallow route hierarchies. Nest only where the parent/child relationship is stable and meaningful to the user/product model. Do not reproduce component trees, sidebar structure, temporary workflow steps, or backend resource nesting inside frontend URLs without product justification.

### 2.5 Stable URLs

A canonical URL should remain stable when Angular components are renamed, layout/shell structure changes, Penpot boards change, backend endpoint structure changes, or implementation internals are refactored.

### 2.6 Dynamic parameters

Use dynamic path parameters only for real independently addressable resources. Parameter names must be semantic, such as `:candidateId`, `:requestId`, `:examId`, and `:orderId`. Do not derive parameter names mechanically from backend DTOs, and do not use a generic `:id` when the domain identity is known.

### 2.7 Query parameters

Use query parameters for view state over the same route-level resource, including filters, search, sorting, pagination, and one-time link tokens where the route-level user goal is unchanged. Do not create separate routes merely for temporary filter/search state.

### 2.8 Locale

For current V1 route authority, locale changes must not change the canonical application path. Canonical routes remain language-neutral stable English identifiers. Do not introduce `/en`, `/ar`, translated Arabic URLs, or locale-dependent route identity unless separately approved later.

### 2.9 Trailing slash

Use one canonical no-trailing-slash representation for application route definitions. Do not create duplicate slash/non-slash aliases.

### 2.10 Registry responsibility

`T-FE-029` owns the canonical route identity/path contract only:

- stable route identifier;
- canonical path or path template;
- minimal deterministic path construction when dynamic parameters require it.

The registry must not become a broad application metadata object.

### 2.11 Explicit separation from T-FE-030

`T-FE-029` must not implement authentication guards, authorization guards, permission enforcement, role enforcement, redirect execution, unauthenticated behavior, unauthorized behavior, safe-return execution, return-url validation, or access-denied decisions. Those concerns belong downstream to `T-FE-030` and related route-access tasks unless the authoritative ledger later says otherwise. Frontend authorization remains UX behavior only; backend authorization remains authoritative.

### 2.12 Safe-return and redirect ownership

Canonical destination constants may later be consumed by redirect logic. Redirect policy, redirect timing, return-url validation, and access routing behavior are not `T-FE-029` implementation responsibilities.

### 2.13 Navigation metadata

Do not put sidebar labels, navigation labels, icons, menu ordering, breadcrumbs, or product copy into the canonical route registry unless a later task explicitly requires it.

### 2.14 Page titles

Page and route titles are separate from canonical URL identity. Do not use translated/display titles as route identifiers. Title ownership may remain with route/page specifications or later Angular route configuration.

### 2.15 Not found

Angular wildcard/catch-all mechanics are implementation concerns. `SYS-002` is an approved presentation contract without an approved canonical visible not-found path; a wildcard implementation must not by itself imply one.

### 2.16 Canonical documentation ownership

This `docs/frontend/design/inventory/page-registry.md` document owns the frontend page and canonical route inventory contract. `docs/frontend/design/inventory/route-permission-matrix.md` owns generic route authentication classification for `T-FE-030` and later route/actor/access/permission relationships for downstream work. Do not duplicate access or guard responsibility here.

## 3. Route identifier and table conventions

- Route IDs use semantic `UPPER_SNAKE_CASE`.
- Route IDs are internal source identifiers only and must not appear as public URL segments.
- Approved paths are lowercase, kebab-case, no trailing slash, and contain no `/api/v1`, `/en`, or `/ar` prefixes.
- `—` in the path column means the destination is intentionally non-routable or blocked/deferred until more authority exists.
- Access classification is intentionally absent from the canonical route table. Generic authentication classification and later actor/access/permission mapping belong in the separate route-permission matrix and `T-FE-030`/`T-FE-031` work.

## 4. Approved canonical route inventory

Every route with an exact path in this table is part of the **APPROVED CANONICAL ROUTE AUTHORITY**. Rows with `—` are explicitly non-routable, blocked, or deferred as recorded in their status and rationale. This is route-contract authority only; Angular source implementation remains unauthorized until a separate `T-FE-029` implementation approval.

| Route ID | Screen/Page ID | Destination | Approved path/template | Family | Dynamic params | Evidence reference | Status | Rationale |
|---|---|---|---|---|---|---|---|---|
| `ROOT_ENTRY` | — | Application entry | `/` | root | — | `T-FE-027`, `T-FE-029`, shell/router outlet evidence | APPROVED_CANONICAL | Canonical application entry identity only; not a content/dashboard claim. Eventual redirect/destination behavior belongs to `T-FE-030`. |
| `AUTH_SIGN_IN` | `AUTH-001` | Sign in | `/auth/sign-in` | auth | — | Ledger `T-FE-041`, contract `C-AUTH-LOGIN` | APPROVED_CANONICAL | Independent public authentication entry. |
| `AUTH_ROLE_SELECTION` | `AUTH-002` | Role selection | `/auth/role-selection` | auth | — | Ledger `T-FE-043`, `T-FE-042` registration clarification | APPROVED_CANONICAL | Distinct route-level registration user goal in the V1 frontend contract. |
| `AUTH_REGISTER_NURSE` | `AUTH-003` | Nurse registration | `/auth/register/nurse` | auth | — | Ledger `T-FE-044`, `T-FE-042` | APPROVED_CANONICAL | Distinct route-level registration user goal for nurse registration. |
| `AUTH_REGISTER_EMPLOYER` | `AUTH-004` | Employer registration | `/auth/register/employer` | auth | — | Ledger `T-FE-045`, `T-FE-042` | APPROVED_CANONICAL | Distinct route-level registration user goal for employer registration. |
| `AUTH_VERIFY_EMAIL_REQUEST` | `AUTH-005` | Send verification email | `/auth/verify-email` | auth | — | Ledger `T-FE-047`, `T-FE-046`, `C-AUTH-SEND-VERIFY` | APPROVED_CANONICAL | Verification request destination; no token in path. |
| `AUTH_VERIFY_EMAIL_CONFIRM` | `AUTH-006` | Verify email link | `/auth/verify-email/confirm` | auth | query token | Ledger `T-FE-047`, `T-FE-046`, `C-AUTH-VERIFY` | APPROVED_CANONICAL | Deep-link destination; token remains query state, not route identity. |
| `AUTH_FORGOT_PASSWORD` | `AUTH-007` | Forgot password | `/auth/forgot-password` | auth | — | Ledger `T-FE-048`, `C-AUTH-FORGOT` | APPROVED_CANONICAL | Independent password recovery entry. |
| `AUTH_RESET_PASSWORD` | `AUTH-008` | Reset password | `/auth/reset-password` | auth | query token | Ledger `T-FE-049`, `C-AUTH-RESET` | APPROVED_CANONICAL | `AUTH_RESET_PASSWORD` remains the canonical reset workflow route; reset token is not exposed as a path segment. |
| `AUTH_RESET_PASSWORD_SUCCESS` | `AUTH-009` | Reset success | — | auth | — | Ledger `T-FE-050` | NOT_ROUTABLE | `AUTH-009` remains traceable as the successful completion state/screen presented within the `AUTH_RESET_PASSWORD` workflow. `/auth/reset-password/success` is not part of the V1 canonical route contract; a future explicit product decision may reopen this boundary. |
| `SYSTEM_SESSION_EXPIRED` | `AUTH-010` | Session expired | `/session-expired` | system | — | Ledger `T-FE-051`, local logout/session evidence | APPROVED_CANONICAL | Approved canonical route-level terminal session destination. This establishes route identity only; token clearing, redirects, guards, and when navigation occurs belong to `T-FE-030` or later session-routing behavior. |
| `SYSTEM_ACCESS_DENIED` | `AUTH-011` | Access denied | `/access-denied` | system | — | Ledger `T-FE-053`, routing permission architecture | APPROVED_CANONICAL | Approved canonical route-level access-denied destination. This does not decide authorization; `T-FE-030`/`T-FE-031` own guard/access behavior and backend authorization remains authoritative. |
| `SYSTEM_ACCOUNT_INACTIVE` | `AUTH-012` | Account inactive | — | system | — | `T-FE-054`, `T-FE-055` backend/design blocker | BLOCKED | No stable coded inactive-account contract; do not create behavior or canonical path yet. |
| `ACCOUNT_OVERVIEW` | `ACC-001` | Account overview | `/account` | account | — | Ledger `T-FE-097`, contract `C-ME` | APPROVED_CANONICAL | Current-user account entry from `/me` contract. |
| `ACCOUNT_DETAILS` | `ACC-002` | Personal details | — | account | — | Ledger `T-FE-097`, contract `C-ME` | NOT_ROUTABLE | `ACC-002` remains a distinct screen/section/form responsibility. In V1 it is owned inside the canonical `/account` destination; `/account/details` is not part of the approved canonical route contract. This does not change the backend `/me` contract or `ACC-002` implementation scope. |
| `ACCOUNT_CHANGE_PASSWORD` | `ACC-003` | Change password | — | account | — | `T-FE-099` backend gap | BLOCKED | No authenticated current-user change-password backend contract. |
| `ACCOUNT_SECURITY_SESSIONS` | `ACC-004` | Security and sessions | — | account | — | `T-FE-100` backend gap | BLOCKED | No active-session/session-management contract. |
| `ACCOUNT_NOTIFICATION_PREFERENCES` | `ACC-005` | Notification preferences | — | account | — | `T-FE-101` backend gap | BLOCKED | No notification preferences contract. |
| `ACCOUNT_STATUS` | `ACC-006` | Account status | — | account | — | `T-FE-102` backend gap | BLOCKED | No stable account-status workflow contract. |
| `NURSE_ENTRY` | — | Nurse entry | `/nurse` | nurse | — | Ledger role-home/slice sequencing, `C-ME` roles evidence | APPROVED_CANONICAL | Stable Nurse family entry. Does not imply a separately implemented Nurse Home dashboard/page; content/redirect selection and role redirect logic are downstream and not `T-FE-029`. |
| `NURSE_PROFILE_OVERVIEW` | `NUR-001`, `NUR-002` | Profile overview | `/nurse/profile` | nurse | — | Ledger `T-FE-056`, contract `C-NUR-PROFILE` | APPROVED_CANONICAL | Approved route-level profile overview destination; edit mode can remain component state unless later approved. |
| `NURSE_PROFILE_PERSONAL_INFORMATION` | `NUR-003` | Personal information | `/nurse/profile/personal-information` | nurse | — | Ledger `T-FE-057`, `C-NUR-PROFILE` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. |
| `NURSE_PROFILE_EXPERIENCE` | `NUR-004`, `NUR-005` | Experience | `/nurse/profile/experience` | nurse | — | Ledger `T-FE-058`, `C-NUR-EXP` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. Add/edit modes remain inside this owning section unless separately approved later. |
| `NURSE_PROFILE_EDUCATION` | `NUR-006`, `NUR-007` | Education | `/nurse/profile/education` | nurse | — | Ledger `T-FE-059`, `C-NUR-EDU` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. Add/edit modes remain inside this owning section unless separately approved later. |
| `NURSE_PROFILE_CERTIFICATES` | `NUR-008`, `NUR-009` | Certificates | `/nurse/profile/certificates` | nurse | — | Ledger `T-FE-060`, `C-NUR-CERT` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. Add/edit modes remain inside this owning section unless separately approved later. |
| `NURSE_PROFILE_SKILLS` | `NUR-010` | Skills | `/nurse/profile/skills` | nurse | — | Ledger `T-FE-062`, `C-NUR-SKILLS-LANG` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. |
| `NURSE_PROFILE_LANGUAGES` | `NUR-011` | Languages | `/nurse/profile/languages` | nurse | — | Ledger `T-FE-062`, `C-NUR-SKILLS-LANG` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. |
| `NURSE_PROFILE_CV` | `NUR-012` | CV management | `/nurse/profile/cv` | nurse | — | Ledger `T-FE-064`, `T-FE-063`, `C-NUR-CV` | APPROVED_CANONICAL | Approved distinct profile section route-level user task. |
| `NURSE_PROFILE_COMPLETION` | `NUR-013` | Profile completion | — | nurse | — | Ledger `T-FE-065` blocker | BLOCKED | Backend/design authority insufficient for canonical route. |
| `NURSE_CONTACT_REQUESTS` | — | Received contact requests | `/nurse/contact-requests` | nurse | — | Ledger `T-FE-096`, `C-NUR-CONTACT` | APPROVED_CANONICAL | Nurse-side employer contact request workflow. |
| `EMPLOYER_HOME` | `EMP-001` | Employer home | `/employer` | employer | — | Ledger `T-FE-090`, `C-EMP-PROFILE` | APPROVED_CANONICAL | Employer role landing/profile destination. |
| `EMPLOYER_CANDIDATES` | `EMP-002`, `EMP-003`, `EMP-004` | Candidate search and results | `/employer/candidates` | employer | query filters/search/page/sort | Ledger `T-FE-091`, `C-EMP-CANDIDATES` | APPROVED_CANONICAL | Search, filters, and results share one route; filters/results are query/view state. |
| `EMPLOYER_CANDIDATE_DETAIL` | `EMP-005` | Candidate profile | — | employer | `:candidateId` deferred | `T-FE-093`, `T-FE-094` backend gap | BLOCKED | No backend candidate detail contract; do not propose exact path until contract exists. |
| `EMPLOYER_CANDIDATE_REQUEST` | `EMP-006` | Recruitment request | — | employer | `:candidateId` deferred | `T-FE-093`, `T-FE-094`, `C-EMP-REQUESTS` | BLOCKED | Depends on candidate detail/profile contract; not independently safe yet. |
| `EMPLOYER_REQUESTS` | `EMP-007` | Recruitment requests | `/employer/requests` | employer | query filters/page/sort | Ledger `T-FE-095`, `C-EMP-REQUESTS` | APPROVED_CANONICAL | Employer-owned request list. |
| `EMPLOYER_REQUEST_DETAIL` | `EMP-008` | Recruitment request detail | `/employer/requests/:requestId` | employer | `:requestId` | Ledger `T-FE-095`, `C-EMP-REQUESTS` | APPROVED_CANONICAL | Contact request is independently addressable. |
| `EXAMS_CATALOG` | `EXM-001` | Exam catalog | `/exams` | exams | query filters/page/sort | Ledger `T-FE-067`, `T-FE-066`, `C-EXAM-CATALOG` | APPROVED_CANONICAL | Startable exam catalog. |
| `EXAMS_DETAIL` | `EXM-002` | Exam detail | `/exams/:examId` | exams | `:examId` | Ledger `T-FE-067`, `T-FE-066`, `C-EXAM-CATALOG` | APPROVED_CANONICAL | Exam is independently addressable. |
| `EXAMS_PURCHASE_REQUIRED` | `EXM-003` | Purchase required | — | exams | `:examId` deferred | Ledger `T-FE-068`, commerce dependency | DEFERRED | Treat as state/CTA unless later screen approval requires route. |
| `EXAMS_INSTRUCTIONS` | `EXM-004` | Exam instructions/start | `/exams/:examId/instructions` | exams | `:examId` | Ledger `T-FE-068`, `C-EXAM-START` | APPROVED_CANONICAL | Pre-session route-level user goal. |
| `EXAMS_SESSION` | `EXM-005`, `EXM-006` | Exam session | `/exams/:examId/sessions/:sessionId` | exams | `:examId`, `:sessionId` | Ledger `T-FE-069`, `C-EXAM-SESSION` | APPROVED_CANONICAL | Active/resumable exam session is independently addressable; submit confirmation remains transient. |
| `EXAMS_RESULT` | `EXM-007` | Exam result | `/exams/:examId/sessions/:sessionId/result` | exams | `:examId`, `:sessionId` | Ledger `T-FE-071`, `C-EXAM-RESULT-REVIEW` | APPROVED_CANONICAL | Result is route-level post-session destination. |
| `EXAMS_ANALYTICS` | `EXM-008` | Performance analytics | `/exams/analytics` | exams | — | Ledger `T-FE-074`, `C-EXAM-ANALYTICS` | APPROVED_CANONICAL | Aggregate analytics destination. |
| `EXAMS_REVIEW` | `EXM-009` | Answer review | `/exams/:examId/sessions/:sessionId/review` | exams | `:examId`, `:sessionId` | Ledger `T-FE-072`, `C-EXAM-RESULT-REVIEW` | APPROVED_CANONICAL | Security-sensitive review route only if backend contract permits. |
| `EXAMS_HISTORY` | `EXM-010` | Exam history | `/exams/history` | exams | query filters/page/sort | Ledger `T-FE-073`, `C-EXAM-ANALYTICS` | APPROVED_CANONICAL | Historical attempts list. |
| `PREPARATION_PACKAGES_OFFERS` | PP offers list | Package offers | `/preparation-packages` | preparation-packages | query filters/page/sort | Ledger `T-FE-075`, PP specs/blueprint as draft evidence | APPROVED_CANONICAL | Canonical frontend product family root. Older/draft `/offers` shapes remain historical/draft evidence only. |
| `PREPARATION_PACKAGES_OFFER_DETAIL` | PP offer detail | Package offer detail | `/preparation-packages/:offerSlug` | preparation-packages | `:offerSlug` | Ledger `T-FE-075`, PP specs/blueprint as draft evidence | APPROVED_CANONICAL | Canonical offer-detail shape. Older/draft `/offers` shapes remain historical/draft evidence only. |
| `PREPARATION_PACKAGES_ENTITLEMENTS` | — | My preparation packages | `/nurse/preparation-packages` | preparation-packages | query filters/page/sort | Ledger `T-FE-076`, `C-PP-ENTITLEMENTS` | APPROVED_CANONICAL | Nurse-owned entitlement list; avoids backend `/me/nurse-profile` path coupling. |
| `PREPARATION_PACKAGES_ENTITLEMENT_DETAIL` | — | Preparation package entitlement detail | `/nurse/preparation-packages/:entitlementId` | preparation-packages | `:entitlementId` | Ledger `T-FE-076`, `C-PP-ENTITLEMENTS` | APPROVED_CANONICAL | Entitlement is independently addressable. |
| `PREPARATION_PACKAGES_PRACTICE` | — | Package practice | `/nurse/preparation-packages/:entitlementId/practice` | preparation-packages | `:entitlementId` | Ledger `T-FE-078`, `C-PP-PRACTICE` | APPROVED_CANONICAL | Practice workflow belongs under selected entitlement. |
| `PREPARATION_PACKAGES_EXAM_START` | — | Package exam start/resume action | — | preparation-packages | — | Ledger `T-FE-079`, `C-PP-EXAM-REPORT` | NOT_ROUTABLE | Package exam start/resume is an action initiated from the selected package-entitlement context. The action may create or recover the qualifying exam session according to backend authority. Once a session destination exists, navigation may use the canonical shared exam-session route. The POST endpoint is not frontend route authority; do not create a second package-specific exam-session URL merely because package provenance exists. |
| `PREPARATION_PACKAGES_REPORT` | — | Package analytical report | `/nurse/preparation-packages/reports/:sessionId` | preparation-packages | `:sessionId` | Ledger `T-FE-079`, `C-PP-EXAM-REPORT` | APPROVED_CANONICAL | Report is independently addressable after package exam session. |
| `PREPARATION_PACKAGES_MATERIAL_READER` | — | Material reader | — | preparation-packages | `:entitlementId`, `:materialId` deferred | `T-FE-080` backend gap | BLOCKED | No learner material reader/download/delivery contract exists. |
| `COMMERCE_PRODUCTS` | `COM-001` | Payment products | `/commerce/products` | commerce | query filters/page/sort | Ledger `T-FE-082`, `T-FE-081`, `C-PAY-PRODUCTS` | APPROVED_CANONICAL | Product catalog for commerce. |
| `COMMERCE_PRODUCT_DETAIL` | `COM-002` | Payment product detail | `/commerce/products/:productId` | commerce | `:productId` | Ledger `T-FE-082`, `T-FE-081`, `C-PAY-PRODUCTS` | APPROVED_CANONICAL | Product is independently addressable. |
| `COMMERCE_CHECKOUT` | `COM-003` | Checkout | `/checkout` | commerce | query product/order context | Ledger `T-FE-084`, `T-FE-083`, `C-PAY-ORDERS` | APPROVED_CANONICAL | Route-level checkout user goal; order creation behavior is feature-owned. |
| `COMMERCE_PAYMENT_PROCESSING` | `COM-004` | Payment processing state | — | commerce | — | Ledger `T-FE-086`, `C-PAY-CHECKOUT` | DEFERRED | `COM-004` remains a checkout/payment processing state. Production-provider callback/return behavior is unresolved; no canonical processing URL is reserved from hypothetical provider behavior. |
| `COMMERCE_PAYMENT_SUCCESS` | `COM-005` | Payment success | `/checkout/orders/:orderId/success` | commerce | `:orderId` | Ledger `T-FE-087`, `C-PAY-ORDERS` | APPROVED_CANONICAL | Approved generic payment outcome route. URL alone is not authoritative proof of payment state; feature implementation must use authoritative backend order/payment state. No production-provider-specific claim is made. |
| `COMMERCE_PAYMENT_FAILURE` | `COM-006` | Payment failure | `/checkout/orders/:orderId/failure` | commerce | `:orderId` | Ledger `T-FE-087`, `C-PAY-ORDERS` | APPROVED_CANONICAL | Approved generic payment outcome route. URL alone is not authoritative proof of payment state; feature implementation must use authoritative backend order/payment state. No production-provider-specific claim is made. |
| `COMMERCE_ORDERS` | `COM-007` | Orders | `/commerce/orders` | commerce | query filters/page/sort | Ledger `T-FE-088`, `T-FE-083`, `C-PAY-ORDERS` | APPROVED_CANONICAL | Nurse-owned order history, not backend-path-coupled. |
| `COMMERCE_ORDER_DETAIL` | `COM-008` | Order detail | `/commerce/orders/:orderId` | commerce | `:orderId` | Ledger `T-FE-088`, `T-FE-083`, `C-PAY-ORDERS` | APPROVED_CANONICAL | Order is independently addressable. |
| `COMMERCE_PRODUCTION_PAYMENT_RELEASE` | — | Production payment release decision | — | commerce | — | `T-FE-089`, `T-FE-137` | BLOCKED | External/backend provider decision, not a route. |
| `ADMIN_ENTRY` | — | Administration entry | `/admin` | admin | — | Ledger admin milestone, `T-FE-098` screen approval packet | APPROVED_CANONICAL | Stable Admin family entry. Does not imply `ADM-001` Admin Dashboard; `ADMIN_DASHBOARD` remains blocked under `T-FE-103`. What `/admin` resolves to and redirect behavior are downstream and not `T-FE-029`. |
| `ADMIN_DASHBOARD` | `ADM-001` | Admin dashboard | — | admin | — | `T-FE-103` backend gap, `T-FE-098` approval dependency | BLOCKED | No stable admin dashboard/metrics contract. |
| `ADMIN_USERS` | `ADM-002` | Users | `/admin/users` | admin | query filters/page/sort | Ledger `T-FE-104`, `C-ADM-USERS` | APPROVED_CANONICAL | Admin user list/management destination. |
| `ADMIN_USER_DETAIL` | `ADM-003` | User detail | `/admin/users/:userId` | admin | `:userId` | Ledger `T-FE-104`, `C-ADM-USERS` | APPROVED_CANONICAL | User is independently addressable; sensitive-field rules remain feature-owned. |
| `ADMIN_ROLES_PERMISSIONS` | `ADM-004` | Roles and permissions | — | admin | — | `T-FE-105` backend gap | BLOCKED | No role/permission management contract. |
| `ADMIN_REFERENCE_DATA` | `ADM-005` | Reference data | `/admin/reference-data` | admin | — | Ledger `T-FE-106`, `C-ADM-EXAM-CATEGORIES` | APPROVED_CANONICAL | Stable admin reference-data concept; exam categories are current evidence. |
| `ADMIN_EXAMS` | `ADM-006` | Exams | `/admin/exams` | admin | query filters/page/sort | Ledger `T-FE-107`, `C-ADM-EXAMS` | APPROVED_CANONICAL | Admin exam lifecycle root. |
| `ADMIN_EXAM_DETAIL` | — | Admin exam detail | `/admin/exams/:examId` | admin | `:examId` | Ledger `T-FE-107`, `C-ADM-EXAMS` | APPROVED_CANONICAL | Exam resource is independently addressable. |
| `ADMIN_EXAM_VERSIONS` | — | Admin exam versions | `/admin/exams/:examId/versions` | admin | `:examId` | Ledger `T-FE-108`, `C-ADM-EXAMS` | APPROVED_CANONICAL | Versions are stable children of an exam. |
| `ADMIN_EXAM_QUESTIONS` | `ADM-007` | Admin exam questions | `/admin/exams/:examId/questions` | admin | `:examId` | Ledger `T-FE-109`, `C-ADM-EXAMS` | APPROVED_CANONICAL | Shallow question management route avoids exposing version/component nesting unless later required. |
| `ADMIN_PAYMENT_PRODUCTS` | `ADM-008` | Admin payment products | `/admin/payment-products` | admin | query filters/page/sort | Ledger `T-FE-111`, `C-ADM-PAY-PRODUCTS` | APPROVED_CANONICAL | Admin payment product management. |
| `ADMIN_PAYMENT_ORDERS` | `ADM-009` | Admin payment orders | — | admin | — | `T-FE-112` backend gap | BLOCKED | No admin payment-order management contract. |
| `ADMIN_RECRUITMENT` | `ADM-010` | Admin recruitment | — | admin | — | `T-FE-112` backend gap | BLOCKED | No admin recruitment-management contract. |
| `ADMIN_PREPARATION_PACKAGE_TOPICS` | — | Preparation package reporting topics | `/admin/preparation-packages/topics` | admin | query filters/page/sort | Ledger `T-FE-114`, `C-ADM-PP-TOPICS` | APPROVED_CANONICAL | Admin PP subdomain under admin family. |
| `ADMIN_PREPARATION_PACKAGE_PROFILES` | — | Preparation package reporting profiles | `/admin/preparation-packages/profiles` | admin | query filters/page/sort | Ledger `T-FE-115`, `C-ADM-PP-PROFILES` | APPROVED_CANONICAL | Admin PP subdomain. |
| `ADMIN_PREPARATION_PACKAGE_MATERIALS` | — | Preparation package materials | `/admin/preparation-packages/materials` | admin | query filters/page/sort | Ledger `T-FE-118`, `C-ADM-PP-MATERIALS` | APPROVED_CANONICAL | Admin material authoring/management, not learner delivery. |
| `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS` | — | Preparation package practice collections | `/admin/preparation-packages/practice-collections` | admin | query filters/page/sort | Ledger `T-FE-119`, `C-ADM-PP-PRACTICE` | APPROVED_CANONICAL | Admin PP subdomain. |
| `ADMIN_PREPARATION_PACKAGE_DEFINITIONS` | — | Preparation package definitions | `/admin/preparation-packages/definitions` | admin | query filters/page/sort | Ledger `T-FE-120`, `C-ADM-PP-PACKAGES` | APPROVED_CANONICAL | Admin package/version/composition management root. |
| `ADMIN_PREPARATION_PACKAGE_OFFERS` | — | Preparation package offers | `/admin/preparation-packages/offers` | admin | query filters/page/sort | Ledger `T-FE-121`, `C-ADM-PP-OFFERS` | APPROVED_CANONICAL | Admin offer lifecycle management. |
| `SYSTEM_NOT_FOUND` | `SYS-002` | Not found presentation | — | system | — | System screen family `T-FE-113`, not-found requirement | NOT_ROUTABLE | `SYS-002` remains the Not Found presentation/screen contract. Angular unmatched-route handling may later render `SYS-002` via `**` while preserving the original unmatched browser URL. Do not redirect merely to manufacture a canonical not-found path; Angular wildcard configuration is implementation detail, not a canonical user-facing path. |
| `SYSTEM_UNEXPECTED_ERROR` | `SYS-003` | Unexpected error presentation | — | system | — | System screen family `T-FE-113`, `T-FE-033` error pattern | NOT_ROUTABLE | `SYS-003` remains a shared/system unexpected-error presentation contract. Page/global error handling may consume it later; no independently navigable error destination is approved in V1. A future runtime/product requirement may reopen this decision. |
| `SYSTEM_OFFLINE` | `SYS-004` | Offline | — | system | — | `T-FE-116` runtime/deployment contract | DEFERRED | Runtime/deployment authority required before canonical route. |
| `SYSTEM_MAINTENANCE` | `SYS-005` | Maintenance | — | system | — | `T-FE-117` runtime/deployment contract | DEFERRED | Runtime/deployment authority required before canonical route. |
| `SYSTEM_EMPTY_STATE` | `SYS-006` | Empty state | — | system | — | `T-FE-035` | NOT_ROUTABLE | Reusable component/pattern state inside owning pages. |
| `SYSTEM_NO_RESULTS` | `SYS-007` | No search results | — | system | — | `T-FE-035` | NOT_ROUTABLE | Query/list state inside owning search/list page. |
| `SYSTEM_RESTRICTED_STATE` | — | Permission restricted state | — | system | — | `T-FE-031`, `T-FE-035` | NOT_ROUTABLE | Permission UX state/presentation is downstream, not route identity. |

## 5. Routable versus transient decisions

| Destination/state | Decision | Reason |
|---|---|---|
| Loading shell (`SYS-001`) | NOT_ROUTABLE | `T-FE-028` owns loading route state; current registry must not invent loading-route behavior. |
| Candidate filters (`EMP-004`) | NOT_ROUTABLE | Filters are query/view state on `EMPLOYER_CANDIDATES`. |
| Candidate results (`EMP-003`) | Not separate from candidate search | Results are the rendered outcome of search/filter query state. |
| Submit confirmation (`EXM-006`) | Not separate from exam session | Confirmation can be transient within `EXAMS_SESSION` unless a later screen approval requires an independent URL. |
| Purchase required (`EXM-003`) | DEFERRED | May be route or state; commerce/screen approval must decide. |
| Reset success (`AUTH-009`) | NOT_ROUTABLE | Successful completion state/screen within the `AUTH_RESET_PASSWORD` workflow; no separate V1 canonical route. |
| Account details (`ACC-002`) | NOT_ROUTABLE | Distinct section/form responsibility owned inside `ACCOUNT_OVERVIEW` at `/account`. |
| Not Found (`SYS-002`) | NOT_ROUTABLE | Catch-all presentation may render later via Angular wildcard while preserving the original unmatched URL; no canonical not-found path. |
| Unexpected error (`SYS-003`) | NOT_ROUTABLE | Shared/system presentation contract only; no independently navigable V1 destination. |
| Package exam start/resume | NOT_ROUTABLE | Entitlement-scoped backend action; resulting session navigation uses the shared `EXAMS_SESSION` route once a session destination exists. |
| Reset/verification tokens | Query state | Tokens are not stable route identity and must not become path segments. |
| Validation errors | NOT_ROUTABLE | `T-FE-034` owns validation display pattern inside forms. |
| Generic loading/error/retry | NOT_ROUTABLE by default | `T-FE-033` owns reusable state pattern, not global page routes. |
| Empty/no-results/restricted | NOT_ROUTABLE by default | `T-FE-035` owns reusable presentation states. |
| Dialogs/drawers/modals | NOT_ROUTABLE by default | No current authority makes them independent browser destinations. |
| Payment processing (`COM-004`) | DEFERRED | Checkout/payment processing remains state; production-provider callback/return behavior is unresolved and no processing URL is reserved. |
| Payment provider callback internals | NOT_ROUTABLE in V1 contract | No production provider decision; success/failure outcome routes remain generic and backend-state-backed only. |

## 6. Blocked and deferred destination register

| Destination | Status | Authority |
|---|---|---|
| Account inactive | BLOCKED | `T-FE-054` found no stable inactive-account contract; `T-FE-055` remains backend/design blocked. |
| Account change password | BLOCKED | `T-FE-099` backend gap. |
| Account security/sessions | BLOCKED | `T-FE-100` backend gap. |
| Account notification preferences | BLOCKED | `T-FE-101` backend gap. |
| Account status workflow | BLOCKED | `T-FE-102` backend gap. |
| Admin dashboard | BLOCKED | `T-FE-103` backend gap and screen approval dependency. |
| Admin roles/permissions | BLOCKED | `T-FE-105` backend gap. |
| Admin payment orders/recruitment management | BLOCKED | `T-FE-112` backend gap. |
| Candidate detail/request | BLOCKED | `T-FE-093` found no candidate detail contract; dependent request UI cannot assume candidate detail. |
| Nurse profile completion | BLOCKED | `T-FE-065` backend/design blocker. |
| Preparation package material reader | BLOCKED | `T-FE-080` found no learner material reader/download/delivery contract. |
| Production payment release/provider-specific outcomes | BLOCKED | `T-FE-089` and `T-FE-137` remain backend/external blockers. |
| Offline and maintenance | DEFERRED | `T-FE-116`/`T-FE-117` require runtime/deployment classification. |
| Payment processing route | DEFERRED | `COM-004` processing state is not a canonical route until production-provider return/callback behavior is resolved. |
| Purchase required | DEFERRED | `EXM-003` remains state/CTA until commerce/screen approval decides. |

## 7. Dynamic parameter policy and review

| Parameter | Approved use | Status | Rationale |
|---|---|---|---|
| `:examId` | Exam detail, instructions, sessions, result, review, admin exam detail | APPROVED_CANONICAL | Exam is an independently addressable product/admin resource. |
| `:sessionId` | Exam session/result/review, package report | APPROVED_CANONICAL | Session/report destinations are independently addressable after creation. |
| `:offerSlug` | Preparation package offer detail | APPROVED_CANONICAL | Offer detail is public and uses the approved `/preparation-packages/:offerSlug` normalization. |
| `:entitlementId` | Nurse preparation package entitlement detail and package practice | APPROVED_CANONICAL | Entitlement is the user-owned package access unit for approved entitlement/detail/practice routes. It is not used by a package exam-start route in V1. |
| `:productId` | Commerce product detail | APPROVED_CANONICAL | Payment product is independently addressable. |
| `:orderId` | Checkout success/failure outcomes and order detail | APPROVED_CANONICAL | Order is independently addressable after backend creation. Processing is not an approved order outcome path in V1. |
| `:requestId` | Employer request detail | APPROVED_CANONICAL | Recruitment/contact request is independently addressable. |
| `:userId` | Admin user detail | APPROVED_CANONICAL | Admin user detail is independently addressable; sensitive exposure checks stay feature-owned. |
| `:candidateId` | Candidate detail/request | BLOCKED | Parameter name is reserved as the semantic convention, but route is blocked by missing backend candidate detail. |
| `:materialId` | Material reader | BLOCKED | Parameter name is reserved as the semantic convention, but material reader is backend blocked. |

Query parameters are approved for filters, search, sorting, pagination, return-url intent, and link tokens. Return-url validation and authentication redirect behavior are owned by `docs/frontend/design/inventory/route-permission-matrix.md` for `T-FE-030`; post-login consumption is owned by the later authorized authentication screen/workflow.

## 8. Special route decisions

- **Root `/`:** approved as a canonical application entry identity only. It is not a content/dashboard claim; eventual redirect/destination behavior belongs to `T-FE-030`.
- **Auth routes:** approved as route identities only. Anonymous-only behavior and authenticated-user redirects are deferred to `T-FE-030`.
- **Registration:** role selection plus nurse/employer registration are approved independent route-level registration user goals and must not be collapsed in V1.
- **Verification/reset deep links:** approved deep-link routes use query tokens; tokens are not part of canonical path identity.
- **Reset success:** `AUTH-009` remains a traceable successful completion state/screen within the reset-password workflow, not a separate V1 canonical route. A future explicit product decision may reopen that boundary.
- **Role entries:** `/nurse`, `/employer`, and `/admin` are stable family entry destinations. `NURSE_ENTRY` and `ADMIN_ENTRY` do not imply separately implemented dashboards/pages; role inference, content selection, and redirect behavior are downstream.
- **Session expired:** approved as a route-level terminal session destination; any token clearing, redirect timing, guard, or navigation trigger remains downstream.
- **Inactive account:** remains blocked; no path is approved or proposed until backend contract exists.
- **Forbidden/access denied:** approved as a visible system destination; deciding when to navigate there is downstream, and backend authorization remains authoritative.
- **Not found:** `SYS-002` is a Not Found presentation/screen contract. Angular unmatched-route handling may later render it via `**` while preserving the original unmatched browser URL. Do not redirect merely to manufacture a canonical not-found URL.
- **Unexpected error:** `SYS-003` is a shared/system unexpected-error presentation contract for page/global handling; no independently navigable V1 error destination is approved.
- **Offline/maintenance:** deferred pending runtime/deployment authority.
- **Search/filter:** query state on owning list/search routes, not separate canonical routes.
- **Checkout/payment outcomes:** checkout and payment success/failure routes are approved without production-provider claims. URL alone is not authoritative proof of payment state; feature implementation must use authoritative backend order/payment state. Payment processing remains deferred as state pending provider-return/callback decisions.
- **Account details:** `ACC-002` remains a distinct personal-details screen/section/form responsibility inside `/account`; it is not an independent canonical route in V1.
- **Nurse profile sections:** `/nurse/profile`, `/nurse/profile/personal-information`, `/nurse/profile/experience`, `/nurse/profile/education`, `/nurse/profile/certificates`, `/nurse/profile/skills`, `/nurse/profile/languages`, and `/nurse/profile/cv` are intentionally approved route-level user tasks. Add/edit modes remain inside their owning section unless separately approved later.
- **Preparation package offer normalization:** `/preparation-packages` and `/preparation-packages/:offerSlug` are the approved canonical frontend product family shapes. Older/draft `/offers` shapes remain historical/draft evidence only.
- **Package exam start/resume:** package exam start/resume is an action from selected entitlement context, not a canonical route. Once a session destination exists, navigation may use the canonical shared exam-session route; exact transition behavior is downstream feature/routing work.

## 9. T-FE-029 and downstream ownership boundary

`T-FE-029` may consume this approved document only after a separate implementation authorization to implement the canonical route ID/path/path-builder source. It must not implement guard execution, permission checks, redirects, return-url validation, navigation menus, page titles, breadcrumbs, screen components, or feature workflows.

`T-FE-030` consumes approved route identities plus `docs/frontend/design/inventory/route-permission-matrix.md` as the generic authentication/public guard contract. `T-FE-031` consumes approved route identities plus later access/permission matrix decisions for UX permission policy. `T-FE-032` consumes those later decisions for navigation presentation.

Screen implementation tasks consume approved route identities only after their family approval gates and per-screen approvals pass.

## 10. Documentation verification checklist

- Each route row has an evidence reference.
- No approved path uses `/api/v1` or backend endpoint nesting such as `/me/nurse-profile`.
- No approved path is derived from Angular component names, Penpot object IDs, or screen IDs alone.
- All exact paths are lowercase and use kebab-case static segments.
- No exact path has a trailing slash.
- No duplicate exact path templates are present.
- No duplicate route IDs are present.
- Dynamic parameters use semantic domain names, not generic `:id`.
- Guard/auth/permission execution is excluded and deferred to downstream tasks.
- Navigation labels, icons, menu ordering, and breadcrumbs are excluded.
- Transient states are not automatically promoted to routes.
- Known backend/design/runtime blockers remain explicit.
- Preparation Package draft route examples are historical/draft evidence only; approved canonical offer paths use `/preparation-packages` and `/preparation-packages/:offerSlug`.
- `frontend/src/app/app.routes.ts` remains unchanged by this documentation contract and still exports an empty `Routes` array until `T-FE-029` implementation is separately authorized and verified.

## 11. Implementation authorization

T-FE-029 is implemented and verified in `47b38dc feat(frontend): add canonical route registry`. T-FE-030 IMPLEMENTATION IS NOT AUTHORIZED YET by this documentation update.
