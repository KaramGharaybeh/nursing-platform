# Frontend Page Registry and Proposed Route Contract

```yaml
document_id: NPS-DES-INV-PAGE-REGISTRY
status: PROPOSED_FOR_TECHNICAL_LEAD_REVIEW
created_at: 2026-09-10
scope: documentation_contract_definition_only
implementation_authorization: false
```

## 1. Purpose and authority

This document is the canonical documentation location for frontend page and route identity decisions. It records:

1. the **approved route design policy** supplied by the technical lead for documentation and contract-definition work; and
2. a **proposed canonical route inventory** for technical-lead approval.

The policy in Section 2 is approved. The exact route table in Section 4 is not yet implementation authority. Every proposed path is labelled `PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL` until the technical lead accepts this document.

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

Angular wildcard/catch-all mechanics are implementation concerns. A canonical visible system destination can be proposed separately from Angular `**` behavior; a wildcard implementation must not by itself imply a visible `/not-found` URL.

### 2.16 Canonical documentation ownership

This `docs/frontend/design/inventory/page-registry.md` document owns the frontend page and canonical route inventory proposal. A future `docs/frontend/design/inventory/route-permission-matrix.md`, if explicitly authorized, owns route/actor/access/permission relationships and is downstream input to `T-FE-030`. Do not duplicate permission/guard responsibility here.

## 3. Route identifier and table conventions

- Route IDs use semantic `UPPER_SNAKE_CASE`.
- Route IDs are internal source identifiers only and must not appear as public URL segments.
- Proposed paths are lowercase, kebab-case, no trailing slash, and contain no `/api/v1`, `/en`, or `/ar` prefixes.
- `—` in the path column means the destination is intentionally non-routable or blocked/deferred until more authority exists.
- Access classification is intentionally absent from the canonical route table. Actor/access/permission mapping belongs in a separate route-permission matrix and `T-FE-030`/`T-FE-031` work.

## 4. Proposed canonical route inventory

Every route with an exact path in this table is `PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL`.

| Route ID | Screen/Page ID | Destination | Proposed path/template | Family | Dynamic params | Evidence reference | Status | Rationale |
|---|---|---|---|---|---|---|---|---|
| `ROOT_ENTRY` | — | Application entry | `/` | root | — | `T-FE-027`, `T-FE-029`, shell/router outlet evidence | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Stable application entry point only; redirect behavior is deferred to `T-FE-030`. |
| `AUTH_SIGN_IN` | `AUTH-001` | Sign in | `/auth/sign-in` | auth | — | Ledger `T-FE-041`, contract `C-AUTH-LOGIN` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Independent public authentication entry. |
| `AUTH_ROLE_SELECTION` | `AUTH-002` | Role selection | `/auth/role-selection` | auth | — | Ledger `T-FE-043`, `T-FE-042` registration clarification | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct registration journey decision point evidenced by screen ID. |
| `AUTH_REGISTER_NURSE` | `AUTH-003` | Nurse registration | `/auth/register/nurse` | auth | — | Ledger `T-FE-044`, `T-FE-042` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct registration destination for nurse actor. |
| `AUTH_REGISTER_EMPLOYER` | `AUTH-004` | Employer registration | `/auth/register/employer` | auth | — | Ledger `T-FE-045`, `T-FE-042` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct registration destination for employer actor. |
| `AUTH_VERIFY_EMAIL_REQUEST` | `AUTH-005` | Send verification email | `/auth/verify-email` | auth | — | Ledger `T-FE-047`, `T-FE-046`, `C-AUTH-SEND-VERIFY` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Verification request destination; no token in path. |
| `AUTH_VERIFY_EMAIL_CONFIRM` | `AUTH-006` | Verify email link | `/auth/verify-email/confirm` | auth | query token | Ledger `T-FE-047`, `T-FE-046`, `C-AUTH-VERIFY` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Deep-link destination; token remains query state, not route identity. |
| `AUTH_FORGOT_PASSWORD` | `AUTH-007` | Forgot password | `/auth/forgot-password` | auth | — | Ledger `T-FE-048`, `C-AUTH-FORGOT` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Independent password recovery entry. |
| `AUTH_RESET_PASSWORD` | `AUTH-008` | Reset password | `/auth/reset-password` | auth | query token | Ledger `T-FE-049`, `C-AUTH-RESET` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Deep-link destination; reset token is not exposed as a path segment. |
| `AUTH_RESET_PASSWORD_SUCCESS` | `AUTH-009` | Reset success | `/auth/reset-password/success` | auth | — | Ledger `T-FE-050` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Reviewable proposal because the ledger models it as a separate screen task. |
| `SYSTEM_SESSION_EXPIRED` | `AUTH-010` | Session expired | `/session-expired` | system | — | Ledger `T-FE-051`, local logout/session evidence | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Terminal user-facing session state; clearing/redirect behavior deferred. |
| `SYSTEM_ACCESS_DENIED` | `AUTH-011` | Access denied | `/access-denied` | system | — | Ledger `T-FE-053`, routing permission architecture | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Visible forbidden/access-denied destination; access decision deferred. |
| `SYSTEM_ACCOUNT_INACTIVE` | `AUTH-012` | Account inactive | — | system | — | `T-FE-054`, `T-FE-055` backend/design blocker | BLOCKED | No stable coded inactive-account contract; do not create behavior or canonical path yet. |
| `ACCOUNT_OVERVIEW` | `ACC-001` | Account overview | `/account` | account | — | Ledger `T-FE-097`, contract `C-ME` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Current-user account entry from `/me` contract. |
| `ACCOUNT_DETAILS` | `ACC-002` | Personal details | `/account/details` | account | — | Ledger `T-FE-097`, contract `C-ME` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct account detail destination proposed from account screen split. |
| `ACCOUNT_CHANGE_PASSWORD` | `ACC-003` | Change password | — | account | — | `T-FE-099` backend gap | BLOCKED | No authenticated current-user change-password backend contract. |
| `ACCOUNT_SECURITY_SESSIONS` | `ACC-004` | Security and sessions | — | account | — | `T-FE-100` backend gap | BLOCKED | No active-session/session-management contract. |
| `ACCOUNT_NOTIFICATION_PREFERENCES` | `ACC-005` | Notification preferences | — | account | — | `T-FE-101` backend gap | BLOCKED | No notification preferences contract. |
| `ACCOUNT_STATUS` | `ACC-006` | Account status | — | account | — | `T-FE-102` backend gap | BLOCKED | No stable account-status workflow contract. |
| `NURSE_HOME` | — | Nurse home | `/nurse` | nurse | — | Ledger role-home/slice sequencing, `C-ME` roles evidence | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Role landing destination only; role redirect behavior deferred. |
| `NURSE_PROFILE_OVERVIEW` | `NUR-001`, `NUR-002` | Profile overview | `/nurse/profile` | nurse | — | Ledger `T-FE-056`, contract `C-NUR-PROFILE` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Combines read/edit modes under one routable profile overview; edit mode can be component state unless later approved. |
| `NURSE_PROFILE_PERSONAL_INFORMATION` | `NUR-003` | Personal information | `/nurse/profile/personal-information` | nurse | — | Ledger `T-FE-057`, `C-NUR-PROFILE` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct profile section with form workflow. |
| `NURSE_PROFILE_EXPERIENCE` | `NUR-004`, `NUR-005` | Experience | `/nurse/profile/experience` | nurse | — | Ledger `T-FE-058`, `C-NUR-EXP` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | List/add/edit can remain within section; no per-experience route proposed yet. |
| `NURSE_PROFILE_EDUCATION` | `NUR-006`, `NUR-007` | Education | `/nurse/profile/education` | nurse | — | Ledger `T-FE-059`, `C-NUR-EDU` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | List/add/edit can remain within section; no per-education route proposed yet. |
| `NURSE_PROFILE_CERTIFICATES` | `NUR-008`, `NUR-009` | Certificates | `/nurse/profile/certificates` | nurse | — | Ledger `T-FE-060`, `C-NUR-CERT` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | List/add/edit can remain within section; no per-certificate route proposed yet. |
| `NURSE_PROFILE_SKILLS` | `NUR-010` | Skills | `/nurse/profile/skills` | nurse | — | Ledger `T-FE-062`, `C-NUR-SKILLS-LANG` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct profile section. |
| `NURSE_PROFILE_LANGUAGES` | `NUR-011` | Languages | `/nurse/profile/languages` | nurse | — | Ledger `T-FE-062`, `C-NUR-SKILLS-LANG` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Distinct profile section. |
| `NURSE_PROFILE_CV` | `NUR-012` | CV management | `/nurse/profile/cv` | nurse | — | Ledger `T-FE-064`, `T-FE-063`, `C-NUR-CV` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Vertical Slice B route-level destination. |
| `NURSE_PROFILE_COMPLETION` | `NUR-013` | Profile completion | — | nurse | — | Ledger `T-FE-065` blocker | BLOCKED | Backend/design authority insufficient for canonical route. |
| `NURSE_CONTACT_REQUESTS` | — | Received contact requests | `/nurse/contact-requests` | nurse | — | Ledger `T-FE-096`, `C-NUR-CONTACT` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Nurse-side employer contact request workflow. |
| `EMPLOYER_HOME` | `EMP-001` | Employer home | `/employer` | employer | — | Ledger `T-FE-090`, `C-EMP-PROFILE` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Employer role landing/profile destination. |
| `EMPLOYER_CANDIDATES` | `EMP-002`, `EMP-003`, `EMP-004` | Candidate search and results | `/employer/candidates` | employer | query filters/search/page/sort | Ledger `T-FE-091`, `C-EMP-CANDIDATES` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Search, filters, and results share one route; filters/results are query/view state. |
| `EMPLOYER_CANDIDATE_DETAIL` | `EMP-005` | Candidate profile | — | employer | `:candidateId` deferred | `T-FE-093`, `T-FE-094` backend gap | BLOCKED | No backend candidate detail contract; do not propose exact path until contract exists. |
| `EMPLOYER_CANDIDATE_REQUEST` | `EMP-006` | Recruitment request | — | employer | `:candidateId` deferred | `T-FE-093`, `T-FE-094`, `C-EMP-REQUESTS` | BLOCKED | Depends on candidate detail/profile contract; not independently safe yet. |
| `EMPLOYER_REQUESTS` | `EMP-007` | Recruitment requests | `/employer/requests` | employer | query filters/page/sort | Ledger `T-FE-095`, `C-EMP-REQUESTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Employer-owned request list. |
| `EMPLOYER_REQUEST_DETAIL` | `EMP-008` | Recruitment request detail | `/employer/requests/:requestId` | employer | `:requestId` | Ledger `T-FE-095`, `C-EMP-REQUESTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Contact request is independently addressable. |
| `EXAMS_CATALOG` | `EXM-001` | Exam catalog | `/exams` | exams | query filters/page/sort | Ledger `T-FE-067`, `T-FE-066`, `C-EXAM-CATALOG` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Startable exam catalog. |
| `EXAMS_DETAIL` | `EXM-002` | Exam detail | `/exams/:examId` | exams | `:examId` | Ledger `T-FE-067`, `T-FE-066`, `C-EXAM-CATALOG` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Exam is independently addressable. |
| `EXAMS_PURCHASE_REQUIRED` | `EXM-003` | Purchase required | — | exams | `:examId` deferred | Ledger `T-FE-068`, commerce dependency | DEFERRED | Treat as state/CTA unless later screen approval requires route. |
| `EXAMS_INSTRUCTIONS` | `EXM-004` | Exam instructions/start | `/exams/:examId/instructions` | exams | `:examId` | Ledger `T-FE-068`, `C-EXAM-START` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Pre-session route-level user goal. |
| `EXAMS_SESSION` | `EXM-005`, `EXM-006` | Exam session | `/exams/:examId/sessions/:sessionId` | exams | `:examId`, `:sessionId` | Ledger `T-FE-069`, `C-EXAM-SESSION` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Active/resumable exam session is independently addressable; submit confirmation remains transient. |
| `EXAMS_RESULT` | `EXM-007` | Exam result | `/exams/:examId/sessions/:sessionId/result` | exams | `:examId`, `:sessionId` | Ledger `T-FE-071`, `C-EXAM-RESULT-REVIEW` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Result is route-level post-session destination. |
| `EXAMS_ANALYTICS` | `EXM-008` | Performance analytics | `/exams/analytics` | exams | — | Ledger `T-FE-074`, `C-EXAM-ANALYTICS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Aggregate analytics destination. |
| `EXAMS_REVIEW` | `EXM-009` | Answer review | `/exams/:examId/sessions/:sessionId/review` | exams | `:examId`, `:sessionId` | Ledger `T-FE-072`, `C-EXAM-RESULT-REVIEW` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Security-sensitive review route only if backend contract permits. |
| `EXAMS_HISTORY` | `EXM-010` | Exam history | `/exams/history` | exams | query filters/page/sort | Ledger `T-FE-073`, `C-EXAM-ANALYTICS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Historical attempts list. |
| `PREPARATION_PACKAGES_OFFERS` | PP offers list | Package offers | `/preparation-packages` | preparation-packages | query filters/page/sort | Ledger `T-FE-075`, PP specs/blueprint as draft evidence | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Public offer catalog; deliberately normalizes draft `/offers` example to the product family root for review. |
| `PREPARATION_PACKAGES_OFFER_DETAIL` | PP offer detail | Package offer detail | `/preparation-packages/:offerSlug` | preparation-packages | `:offerSlug` | Ledger `T-FE-075`, PP specs/blueprint as draft evidence | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Offer slug is independently addressable; proposal is not authority until approved. |
| `PREPARATION_PACKAGES_ENTITLEMENTS` | — | My preparation packages | `/nurse/preparation-packages` | preparation-packages | query filters/page/sort | Ledger `T-FE-076`, `C-PP-ENTITLEMENTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Nurse-owned entitlement list; avoids backend `/me/nurse-profile` path coupling. |
| `PREPARATION_PACKAGES_ENTITLEMENT_DETAIL` | — | Preparation package entitlement detail | `/nurse/preparation-packages/:entitlementId` | preparation-packages | `:entitlementId` | Ledger `T-FE-076`, `C-PP-ENTITLEMENTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Entitlement is independently addressable. |
| `PREPARATION_PACKAGES_PRACTICE` | — | Package practice | `/nurse/preparation-packages/:entitlementId/practice` | preparation-packages | `:entitlementId` | Ledger `T-FE-078`, `C-PP-PRACTICE` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Practice workflow belongs under selected entitlement. |
| `PREPARATION_PACKAGES_EXAM_START` | — | Package exam start | `/nurse/preparation-packages/:entitlementId/exam` | preparation-packages | `:entitlementId` | Ledger `T-FE-079`, `C-PP-EXAM-REPORT` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Package exam is entitlement-scoped. |
| `PREPARATION_PACKAGES_REPORT` | — | Package analytical report | `/nurse/preparation-packages/reports/:sessionId` | preparation-packages | `:sessionId` | Ledger `T-FE-079`, `C-PP-EXAM-REPORT` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Report is independently addressable after package exam session. |
| `PREPARATION_PACKAGES_MATERIAL_READER` | — | Material reader | — | preparation-packages | `:entitlementId`, `:materialId` deferred | `T-FE-080` backend gap | BLOCKED | No learner material reader/download/delivery contract exists. |
| `COMMERCE_PRODUCTS` | `COM-001` | Payment products | `/commerce/products` | commerce | query filters/page/sort | Ledger `T-FE-082`, `T-FE-081`, `C-PAY-PRODUCTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Product catalog for commerce. |
| `COMMERCE_PRODUCT_DETAIL` | `COM-002` | Payment product detail | `/commerce/products/:productId` | commerce | `:productId` | Ledger `T-FE-082`, `T-FE-081`, `C-PAY-PRODUCTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Product is independently addressable. |
| `COMMERCE_CHECKOUT` | `COM-003` | Checkout | `/checkout` | commerce | query product/order context | Ledger `T-FE-084`, `T-FE-083`, `C-PAY-ORDERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Route-level checkout user goal; order creation behavior is feature-owned. |
| `COMMERCE_PAYMENT_PROCESSING` | `COM-004` | Payment processing | `/checkout/orders/:orderId/processing` | commerce | `:orderId` | Ledger `T-FE-086`, `C-PAY-CHECKOUT` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Processing can be addressable for provider return UX; production provider remains blocked. |
| `COMMERCE_PAYMENT_SUCCESS` | `COM-005` | Payment success | `/checkout/orders/:orderId/success` | commerce | `:orderId` | Ledger `T-FE-087`, `C-PAY-ORDERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Generic outcome route; no provider-specific claim. |
| `COMMERCE_PAYMENT_FAILURE` | `COM-006` | Payment failure | `/checkout/orders/:orderId/failure` | commerce | `:orderId` | Ledger `T-FE-087`, `C-PAY-ORDERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Generic outcome route; no provider-specific claim. |
| `COMMERCE_ORDERS` | `COM-007` | Orders | `/commerce/orders` | commerce | query filters/page/sort | Ledger `T-FE-088`, `T-FE-083`, `C-PAY-ORDERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Nurse-owned order history, not backend-path-coupled. |
| `COMMERCE_ORDER_DETAIL` | `COM-008` | Order detail | `/commerce/orders/:orderId` | commerce | `:orderId` | Ledger `T-FE-088`, `T-FE-083`, `C-PAY-ORDERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Order is independently addressable. |
| `COMMERCE_PRODUCTION_PAYMENT_RELEASE` | — | Production payment release decision | — | commerce | — | `T-FE-089`, `T-FE-137` | BLOCKED | External/backend provider decision, not a route. |
| `ADMIN_HOME` | — | Administration home | `/admin` | admin | — | Ledger admin milestone, `T-FE-098` screen approval packet | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin role landing destination only. |
| `ADMIN_DASHBOARD` | `ADM-001` | Admin dashboard | — | admin | — | `T-FE-103` backend gap, `T-FE-098` approval dependency | BLOCKED | No stable admin dashboard/metrics contract. |
| `ADMIN_USERS` | `ADM-002` | Users | `/admin/users` | admin | query filters/page/sort | Ledger `T-FE-104`, `C-ADM-USERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin user list/management destination. |
| `ADMIN_USER_DETAIL` | `ADM-003` | User detail | `/admin/users/:userId` | admin | `:userId` | Ledger `T-FE-104`, `C-ADM-USERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | User is independently addressable; sensitive-field rules remain feature-owned. |
| `ADMIN_ROLES_PERMISSIONS` | `ADM-004` | Roles and permissions | — | admin | — | `T-FE-105` backend gap | BLOCKED | No role/permission management contract. |
| `ADMIN_REFERENCE_DATA` | `ADM-005` | Reference data | `/admin/reference-data` | admin | — | Ledger `T-FE-106`, `C-ADM-EXAM-CATEGORIES` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Stable admin reference-data concept; exam categories are current evidence. |
| `ADMIN_EXAMS` | `ADM-006` | Exams | `/admin/exams` | admin | query filters/page/sort | Ledger `T-FE-107`, `C-ADM-EXAMS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin exam lifecycle root. |
| `ADMIN_EXAM_DETAIL` | — | Admin exam detail | `/admin/exams/:examId` | admin | `:examId` | Ledger `T-FE-107`, `C-ADM-EXAMS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Exam resource is independently addressable. |
| `ADMIN_EXAM_VERSIONS` | — | Admin exam versions | `/admin/exams/:examId/versions` | admin | `:examId` | Ledger `T-FE-108`, `C-ADM-EXAMS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Versions are stable children of an exam. |
| `ADMIN_EXAM_QUESTIONS` | `ADM-007` | Admin exam questions | `/admin/exams/:examId/questions` | admin | `:examId` | Ledger `T-FE-109`, `C-ADM-EXAMS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Shallow question management route avoids exposing version/component nesting unless later required. |
| `ADMIN_PAYMENT_PRODUCTS` | `ADM-008` | Admin payment products | `/admin/payment-products` | admin | query filters/page/sort | Ledger `T-FE-111`, `C-ADM-PAY-PRODUCTS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin payment product management. |
| `ADMIN_PAYMENT_ORDERS` | `ADM-009` | Admin payment orders | — | admin | — | `T-FE-112` backend gap | BLOCKED | No admin payment-order management contract. |
| `ADMIN_RECRUITMENT` | `ADM-010` | Admin recruitment | — | admin | — | `T-FE-112` backend gap | BLOCKED | No admin recruitment-management contract. |
| `ADMIN_PREPARATION_PACKAGE_TOPICS` | — | Preparation package reporting topics | `/admin/preparation-packages/topics` | admin | query filters/page/sort | Ledger `T-FE-114`, `C-ADM-PP-TOPICS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin PP subdomain under admin family. |
| `ADMIN_PREPARATION_PACKAGE_PROFILES` | — | Preparation package reporting profiles | `/admin/preparation-packages/profiles` | admin | query filters/page/sort | Ledger `T-FE-115`, `C-ADM-PP-PROFILES` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin PP subdomain. |
| `ADMIN_PREPARATION_PACKAGE_MATERIALS` | — | Preparation package materials | `/admin/preparation-packages/materials` | admin | query filters/page/sort | Ledger `T-FE-118`, `C-ADM-PP-MATERIALS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin material authoring/management, not learner delivery. |
| `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS` | — | Preparation package practice collections | `/admin/preparation-packages/practice-collections` | admin | query filters/page/sort | Ledger `T-FE-119`, `C-ADM-PP-PRACTICE` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin PP subdomain. |
| `ADMIN_PREPARATION_PACKAGE_DEFINITIONS` | — | Preparation package definitions | `/admin/preparation-packages/definitions` | admin | query filters/page/sort | Ledger `T-FE-120`, `C-ADM-PP-PACKAGES` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin package/version/composition management root. |
| `ADMIN_PREPARATION_PACKAGE_OFFERS` | — | Preparation package offers | `/admin/preparation-packages/offers` | admin | query filters/page/sort | Ledger `T-FE-121`, `C-ADM-PP-OFFERS` | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin offer lifecycle management. |
| `SYSTEM_NOT_FOUND` | `SYS-002` | Not found | `/not-found` | system | — | System screen family `T-FE-113`, not-found requirement | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Visible not-found destination; Angular wildcard mechanics are separate. |
| `SYSTEM_UNEXPECTED_ERROR` | `SYS-003` | Unexpected error | `/error` | system | — | System screen family `T-FE-113`, `T-FE-033` error pattern | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Visible route-level unexpected-error destination; component error states remain local. |
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
| Reset/verification tokens | Query state | Tokens are not stable route identity and must not become path segments. |
| Validation errors | NOT_ROUTABLE | `T-FE-034` owns validation display pattern inside forms. |
| Generic loading/error/retry | NOT_ROUTABLE by default | `T-FE-033` owns reusable state pattern, not global page routes. |
| Empty/no-results/restricted | NOT_ROUTABLE by default | `T-FE-035` owns reusable presentation states. |
| Dialogs/drawers/modals | NOT_ROUTABLE by default | No current authority makes them independent browser destinations. |
| Payment provider callback internals | NOT_ROUTABLE in V1 proposal | No production provider decision; outcome routes remain generic proposals only. |

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

## 7. Dynamic parameter policy and review

| Parameter | Proposed use | Status | Rationale |
|---|---|---|---|
| `:examId` | Exam detail, instructions, sessions, result, review, admin exam detail | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Exam is an independently addressable product/admin resource. |
| `:sessionId` | Exam session/result/review, package report | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Session/report destinations are independently addressable after creation. |
| `:offerSlug` | Preparation package offer detail | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Offer detail is public and slug-backed in draft PP evidence; approval still required. |
| `:entitlementId` | Nurse preparation package entitlement detail/practice/exam | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Entitlement is the user-owned package access unit. |
| `:productId` | Commerce product detail | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Payment product is independently addressable. |
| `:orderId` | Checkout outcomes/order detail | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Order is independently addressable after backend creation. |
| `:requestId` | Employer request detail | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Recruitment/contact request is independently addressable. |
| `:userId` | Admin user detail | PROPOSED — AWAITING TECHNICAL-LEAD APPROVAL | Admin user detail is independently addressable; sensitive exposure checks stay feature-owned. |
| `:candidateId` | Candidate detail/request | BLOCKED | Parameter name is reserved as the semantic convention, but route is blocked by missing backend candidate detail. |
| `:materialId` | Material reader | BLOCKED | Parameter name is reserved as the semantic convention, but material reader is backend blocked. |

Query parameters are proposed for filters, search, sorting, pagination, return-url intent, and link tokens. Return-url validation and redirect execution are explicitly outside this document.

## 8. Special route decisions

- **Root `/`:** proposed as a canonical application entry only. Any role-based or anonymous redirect execution is deferred to `T-FE-030`.
- **Auth routes:** proposed as route identities only. Anonymous-only behavior and authenticated-user redirects are deferred to `T-FE-030`.
- **Registration:** role selection plus nurse/employer registration are proposed because the ledger contains distinct auth screen tasks.
- **Verification/reset deep links:** proposed routes use query tokens; tokens are not part of canonical path identity.
- **Role homes:** `/nurse`, `/employer`, and `/admin` are proposed stable role-home destinations. Role inference/redirect behavior is deferred.
- **Session expired:** proposed as a route-level terminal state; any clearing/redirect behavior remains downstream.
- **Inactive account:** remains blocked; no path is approved or proposed until backend contract exists.
- **Forbidden/access denied:** proposed as a visible system destination; deciding when to navigate there is downstream.
- **Not found:** `/not-found` is proposed as a visible destination; Angular `**` wildcard mechanics are implementation-only and deferred.
- **Unexpected error:** `/error` is proposed for route-level unexpected errors; local component errors stay with `T-FE-033` patterns.
- **Offline/maintenance:** deferred pending runtime/deployment authority.
- **Search/filter:** query state on owning list/search routes, not separate canonical routes.
- **Checkout/payment outcomes:** generic checkout and outcome routes are proposed without production-provider claims; production release remains blocked.

## 9. T-FE-029 and downstream ownership boundary

`T-FE-029` may consume an approved version of this document to implement the canonical route ID/path/path-builder source. It must not implement guard execution, permission checks, redirects, return-url validation, navigation menus, page titles, breadcrumbs, screen components, or feature workflows.

`T-FE-030` consumes approved route identities as destinations for authentication/public guard behavior. `T-FE-031` consumes approved route identities plus a future access/permission matrix for UX permission policy. `T-FE-032` consumes those later decisions for navigation presentation.

Screen implementation tasks consume approved route identities only after their family approval gates and per-screen approvals pass.

## 10. Documentation verification checklist

- Each proposed route row has an evidence reference.
- No proposed path uses `/api/v1` or backend endpoint nesting such as `/me/nurse-profile`.
- No proposed path is derived from Angular component names, Penpot object IDs, or screen IDs alone.
- All exact paths are lowercase and use kebab-case static segments.
- No exact path has a trailing slash.
- No duplicate exact path templates are present.
- No duplicate route IDs are present.
- Dynamic parameters use semantic domain names, not generic `:id`.
- Guard/auth/permission execution is excluded and deferred to downstream tasks.
- Navigation labels, icons, menu ordering, and breadcrumbs are excluded.
- Transient states are not automatically promoted to routes.
- Known backend/design/runtime blockers remain explicit.
- Preparation Package draft route examples are used only as evidence for a proposal and are not promoted as authority until technical-lead approval.

## 11. Implementation authorization

T-FE-029 IMPLEMENTATION IS NOT AUTHORIZED YET.
