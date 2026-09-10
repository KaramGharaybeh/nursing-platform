# Frontend Route Access Contract

```yaml
document_id: NPS-DES-INV-ROUTE-PERMISSION-MATRIX
status: APPROVED_T_FE_030_AUTH_ROUTING_CONTRACT_AND_T_FE_031_ROUTE_UX_PERMISSION_CONTRACT
created_at: 2026-09-10
scope: documentation_contract_definition_only
implementation_authorization: false
source_route_authority: docs/frontend/design/inventory/page-registry.md
implemented_route_registry: frontend/src/app/core/routing/canonical-routes.ts
accepted_route_authority_commit: de6bc16 docs(frontend): approve canonical route contract
accepted_route_registry_commit: 47b38dc feat(frontend): add canonical route registry
```

## 1. Purpose and authority

This document is the approved route-access documentation owner for generic frontend route authentication behavior used by `T-FE-030`.

It consumes route identity and path/path-template authority from:

- `docs/frontend/design/inventory/page-registry.md`; and
- `frontend/src/app/core/routing/canonical-routes.ts`.

This document does not redefine, duplicate, or change canonical route IDs or paths. The canonical route registry remains the source for executable route identity/path values.

This checkpoint does not authorize Angular implementation. It does not create guards, does not modify `frontend/src/app/app.routes.ts`, does not add route activation, and does not authorize navigation, screens, or `T-FE-031`.

## 2. T-FE-030 classification model

For V1 `T-FE-030`, every exact canonical route has exactly one generic auth-routing classification:

1. `PUBLIC`
2. `ENTRY`
3. `AUTHENTICATED`

There is no anonymous-only route classification in `T-FE-030` V1.

### 2.1 PUBLIC

`PUBLIC` means:

- accessible when anonymous;
- accessible when authenticated; and
- `T-FE-030` performs no authenticated-user redirect away from it.

Do not introduce an anonymous-only guard for `PUBLIC` routes in `T-FE-030` V1. Do not redirect authenticated users away from sign-in, registration, verification, recovery, system terminal, or public preparation-package routes in `T-FE-030` V1.

### 2.2 ENTRY

`ENTRY` means:

- application entry identity;
- accessible regardless of authentication state; and
- no redirect behavior is implied by this classification.

### 2.3 AUTHENTICATED

`AUTHENTICATED` means:

- requires a resolved authenticated token-session; and
- does not imply role or permission authorization.

At the `T-FE-030` layer, an authenticated token-session satisfies the generic authentication requirement for every `AUTHENTICATED` canonical route, including `/nurse`, `/employer`, `/admin`, and their descendants. `T-FE-030` must not decide whether the authenticated user has the correct actor role or permission for the destination.

Backend authentication and authorization remain authoritative. Frontend route access is presentation/navigation behavior only.

## 3. Exact PUBLIC routes

The following 12 canonical route IDs are `PUBLIC`:

| Route ID | Canonical path/template |
|---|---|
| `AUTH_SIGN_IN` | `/auth/sign-in` |
| `AUTH_ROLE_SELECTION` | `/auth/role-selection` |
| `AUTH_REGISTER_NURSE` | `/auth/register/nurse` |
| `AUTH_REGISTER_EMPLOYER` | `/auth/register/employer` |
| `AUTH_VERIFY_EMAIL_REQUEST` | `/auth/verify-email` |
| `AUTH_VERIFY_EMAIL_CONFIRM` | `/auth/verify-email/confirm` |
| `AUTH_FORGOT_PASSWORD` | `/auth/forgot-password` |
| `AUTH_RESET_PASSWORD` | `/auth/reset-password` |
| `SYSTEM_SESSION_EXPIRED` | `/session-expired` |
| `SYSTEM_ACCESS_DENIED` | `/access-denied` |
| `PREPARATION_PACKAGES_OFFERS` | `/preparation-packages` |
| `PREPARATION_PACKAGES_OFFER_DETAIL` | `/preparation-packages/:offerSlug` |

## 4. ENTRY route

The following canonical route ID is `ENTRY`:

| Route ID | Canonical path/template |
|---|---|
| `ROOT_ENTRY` | `/` |

`ROOT_ENTRY` does not define anonymous root redirect, authenticated root redirect, role-home selection, nurse/employer/admin destination selection, or any other root redirect behavior in `T-FE-030` V1.

## 5. AUTHENTICATED classification rule

Every exact canonical route from the approved 62-route registry that is not one of the 12 `PUBLIC` route IDs and is not `ROOT_ENTRY` is classified `AUTHENTICATED`.

The expected count is 49 `AUTHENTICATED` canonical routes.

Do not maintain a manually divergent second exhaustive `AUTHENTICATED` route list in implementation when the classification can be represented deterministically from the canonical route registry plus the `PUBLIC`/`ENTRY` sets.

## 6. Anonymous access to AUTHENTICATED routes

When token-session resolution establishes that the user is anonymous and the requested canonical route is `AUTHENTICATED`, `T-FE-030` guard behavior is:

1. redirect to `AUTH_SIGN_IN`; and
2. preserve the original requested internal destination using the query parameter key `returnUrl`.

The redirect destination path is supplied by the canonical route registry and is currently `/auth/sign-in`. Do not hard-code a duplicate `/auth/sign-in` string in guard implementation when the canonical registry can supply it.

Conceptually:

```text
/auth/sign-in?returnUrl=<requested-internal-url>
```

Use Angular Router URL construction and serialization. Do not manually concatenate or manually percent-encode query parameters.

`SYSTEM_SESSION_EXPIRED` is not used for generic anonymous access to an `AUTHENTICATED` route.

`SYSTEM_ACCESS_DENIED` is not used for generic authentication-only failure.

## 7. Return intent and safe return

`T-FE-030` owns the generic safe-return contract.

The preserved return intent represents the original internal application URL requested before authentication. It preserves path plus query and fragment information to the extent represented by Angular Router state.

The generation source is Angular routing state, not an untrusted external URL supplied by application code.

The exact query key is:

```text
returnUrl
```

### 7.1 Safe-return validation

`T-FE-030` owns one reusable, pure safe-return validation helper for eventual post-auth consumption.

A candidate `returnUrl` is valid only when it represents an internal application path.

Accepted values:

- a normal internal absolute application path beginning with exactly one `/`;
- optional internal query string; and
- optional fragment.

Rejected values:

- protocol-relative values beginning `//`;
- absolute external URLs;
- URI schemes such as `http:`, `https:`, `javascript:`, `data:`, or equivalent scheme-bearing input;
- backslash-based path confusion;
- control-character input; and
- empty or non-path external-style values.

This helper must not perform role validation, permission validation, actor validation, backend calls, or `T-FE-031` policy checks.

If implementation later needs a fallback when a supplied `returnUrl` fails validation, use only fallback behavior that is explicitly authorized by the downstream login/navigation owner. Do not invent post-login destination behavior in `T-FE-030`.

### 7.2 Post-login consumption ownership

`T-FE-030` defines:

- returnUrl production;
- the `returnUrl` key; and
- the safe-return validation contract/helper.

Successful-login consumption/navigation belongs to the appropriate authentication screen/workflow when that screen is authorized. That consumer must use the `T-FE-030` safe-return validation contract rather than creating another independent validator.

## 8. AuthSessionBootstrap boundary

`T-FE-030` generic authentication guards depend on `AuthSessionBootstrap` and the resolved token-session state.

They do not read `TokenStorage` directly when the resolved session abstraction is available. They do not access `sessionStorage` or `localStorage` directly. They do not trigger a second competing bootstrap workflow and do not perform duplicate refresh orchestration.

If auth-session state is still `initializing`, the guard must not prematurely classify the user as anonymous. Navigation requiring an auth decision waits for `AuthSessionBootstrap` to resolve to `authenticated` or `anonymous` using the existing repository abstraction.

## 9. CurrentUserStore boundary

`CurrentUserStore` states do not control `T-FE-030` generic auth-only routing:

- `idle`
- `loading`
- `ready`
- `anonymous`
- `unavailable`

`CurrentUserState.unavailable` must not by itself redirect an otherwise token-session-authenticated user to sign-in. `T-FE-030` must not reinterpret current-user hydration or network failure as authentication failure.

`T-FE-031` may later consume `CurrentUserStore.ready` for role/permission UX decisions according to its own approved contract.

## 10. Session expired decision

`SYSTEM_SESSION_EXPIRED` remains a canonical `PUBLIC` route identity at `/session-expired`.

`T-FE-030` V1 does not automatically navigate to it.

Current generic session state does not provide an approved distinction between a first-time or normal anonymous user and a user whose previously authenticated session specifically expired.

Therefore:

```text
anonymous access to an AUTHENTICATED route -> AUTH_SIGN_IN + returnUrl
```

not:

```text
anonymous access to an AUTHENTICATED route -> SYSTEM_SESSION_EXPIRED
```

Do not infer session-expired semantics from `anonymous` alone. A future explicit session-expiry transition/reason contract may authorize navigation to `SYSTEM_SESSION_EXPIRED`.

## 11. Access denied and T-FE-031 boundary

`SYSTEM_ACCESS_DENIED` remains a canonical `PUBLIC` route identity at `/access-denied`.

`T-FE-030` does not navigate to it based on authentication alone.

`T-FE-030` owns only authenticated-versus-anonymous routing. It must not implement:

- role checks;
- permission checks;
- role-route mapping;
- permission-route mapping;
- Nurse/Employer/Admin route authorization;
- `/access-denied` decisions based on roles or permissions;
- navigation visibility;
- menu filtering;
- sidebar filtering; or
- backend permission mirroring.

`T-FE-031` remains a separate task for route-level UX permission policy. Backend authorization remains authoritative.

## 12. Entry and actor-family route boundary

`NURSE_ENTRY` at `/nurse` and `ADMIN_ENTRY` at `/admin` are classified `AUTHENTICATED`.

`EMPLOYER_HOME` at `/employer` and all other actor/domain routes classified `AUTHENTICATED` follow the same boundary: `T-FE-030` proves only that a resolved authenticated token-session exists.

`T-FE-030` does not assign role-specific semantics and does not implement:

- Nurse role checks on `/nurse`;
- Admin role checks on `/admin`;
- Employer role checks on `/employer`;
- redirects from `/nurse`;
- redirects from `/admin`;
- redirects from `/employer`; or
- role-home selection.

Those constraints belong to downstream route UX policy and feature/screen routing work.

## 13. Mechanical classification verification

This contract must be mechanically verified against `frontend/src/app/core/routing/canonical-routes.ts` before `T-FE-030` implementation begins.

Required counts:

```text
TOTAL_CANONICAL_ROUTES=62
PUBLIC=12
ENTRY=1
AUTHENTICATED=49
CLASSIFIED_TOTAL=62
```

Required invariants:

- `12 + 1 + 49 = 62`.
- Every exact canonical route appears in exactly one auth classification.
- No `BLOCKED` route is classified.
- No `DEFERRED` route is classified.
- No `NOT_ROUTABLE` destination is classified.
- No duplicate classification exists.
- No exact canonical route is missing.
- No canonical route ID or path is changed by this documentation contract.

## 14. T-FE-030 implementation checkpoint

T-FE-030 is implemented and verified in `a8dcf4a feat(frontend): add auth route guards`.

The generic `T-FE-030` route classification remains immutable for `T-FE-031`:

```text
PUBLIC=12
ENTRY=1
AUTHENTICATED=49
```

`T-FE-031` must not redesign generic authentication routing, anonymous redirects, `returnUrl`, safe-return validation, or the `AuthSessionBootstrap` boundary.

## 15. T-FE-031 route-level UX permission policy

This section is the approved `T-FE-031` route-level UX permission contract. It applies only to the 49 canonical routes classified as `AUTHENTICATED` by the immutable `T-FE-030` classification.

This section does not authorize Angular implementation. It does not create route guards, permission guards, route metadata, navigation filtering, screen behavior, backend authorization, backend error handling, or `T-FE-032` permission-aware navigation.

### 15.1 Source of roles and permissions

`T-FE-031` consumes `CurrentUserStore` after current-user hydration.

When `CurrentUserStore` is `ready`:

- `CurrentUser.roles` is the source for actor-context UX restrictions.
- `CurrentUser.permissions` is the source for capability-level UX restrictions.
- Both values come from `GET /api/v1/me`.

`T-FE-031` must not read JWT claims for roles or permissions. It must not duplicate backend authorization logic. Backend authorization remains authoritative; route-level frontend policy is UX only.

### 15.2 Allowed V1 policy forms

Every one of the 49 `AUTHENTICATED` canonical routes must have exactly one explicit `T-FE-031` policy.

Allowed V1 policy forms are:

| Policy form | Meaning |
|---|---|
| `AUTHENTICATED_ONLY` | No additional `T-FE-031` role or permission predicate after `T-FE-030` authentication. |
| `ROLE` | One or more accepted roles; any accepted role satisfies the role predicate. |
| `ROLE_AND_PERMISSION` | The role predicate must pass and the required permission predicate must also pass. |

For current V1 route policies, a route that names one permission requires that exact permission.

Do not create a generic authorization DSL. Do not add business-resource ownership, entitlement, exam eligibility, payment state, candidate visibility, account active state, or other feature/business rules to this generic route policy.

### 15.3 Multi-role semantics

Roles are not assumed mutually exclusive.

Role matching uses union semantics:

- If a route accepts one role, the current user must contain that role.
- If a future route accepts multiple roles, containing any accepted role satisfies the role predicate.

Example: `roles = [Admin, Nurse]` satisfies both a `Nurse` role requirement and an `Admin` role requirement.

There is no role precedence. `Admin` does not automatically override a missing required permission. For `ROLE_AND_PERMISSION`, the role predicate and permission predicate must both pass.

Use exact backend role and permission string values. Do not invent aliases.

### 15.4 CurrentUserStore state behavior

`T-FE-031` preserves separation from `T-FE-030` generic authentication routing.

| `CurrentUserStore` status | `T-FE-031` behavior |
|---|---|
| `idle` | Wait for the existing hydration lifecycle; do not evaluate permission policy prematurely. |
| `loading` | Wait; do not treat as denied. |
| `ready` | Evaluate the exact route policy. |
| `anonymous` | Perform no permission/access-denied redirect. Authentication routing remains `T-FE-030` responsibility. Do not duplicate `AUTH_SIGN_IN` behavior. |
| `unavailable` | Do not interpret as missing role/permission. Do not redirect to `SYSTEM_ACCESS_DENIED` or sign-in. Do not deny the route solely because current-user hydration is unavailable. Backend authorization remains authoritative and owning feature/error-state behavior handles data failure. |

Do not redesign `CurrentUserStore` as part of `T-FE-031`.

### 15.5 Access denied behavior

When all of the following are true:

1. generic authentication has already been satisfied;
2. `CurrentUserStore` is `ready`;
3. the route has an explicit `ROLE` or `ROLE_AND_PERMISSION` policy; and
4. the ready current user does not satisfy that policy;

`T-FE-031` route-level UX behavior is to redirect to canonical `SYSTEM_ACCESS_DENIED`, currently `/access-denied`, using the canonical route registry. Do not hard-code a duplicate path string.

No `returnUrl` is required for access-denied permission failure.

Do not use `SYSTEM_ACCESS_DENIED` for:

- anonymous authentication failure;
- `CurrentUserStore.unavailable`;
- backend/network failure;
- business-rule failure;
- entitlement failure;
- payment failure; or
- exam eligibility failure.

### 15.6 Backend 403 exclusion

HTTP `403` response handling is not owned by `T-FE-031`.

`T-FE-031` owns pre-navigation UX route policy only. It must not add:

- global `403` interceptor behavior;
- automatic HTTP `403` redirect; or
- backend-response routing behavior.

HTTP error handling remains with the appropriate existing or future HTTP, error-state, or feature workflow authority. Do not conflate pre-navigation permission UX with backend authorization response handling.

### 15.7 Missing-policy rule

There must be no silent fallback for an `AUTHENTICATED` canonical route missing from the `T-FE-031` policy registry.

All 49 routes must be covered explicitly. Implementation/tests must prove:

```text
POLICY_ROWS=49
UNIQUE_POLICY_ROUTE_IDS=49
MISSING_AUTHENTICATED_ROUTE_POLICIES=0
EXTRA_ROUTE_POLICIES=0
```

A future authenticated canonical route may not silently inherit `AUTHENTICATED_ONLY`. Its `T-FE-031` policy must be explicitly decided when that route is added.

If runtime lookup somehow receives an authenticated canonical route with no policy, treat that as an invalid/unresolved policy configuration rather than inventing permission semantics. Do not silently allow or infer from path prefix.

### 15.8 Explicit 49-route policy matrix

Counts:

```text
TOTAL_AUTHENTICATED_ROUTES=49
AUTHENTICATED_ONLY=11
NURSE_ROLE=19
EMPLOYER_ROLE=4
ADMIN_ENTRY_ROLE=1
ADMIN_ROLE_AND_PERMISSION=14
```

Arithmetic: `11 + 19 + 4 + 1 + 14 = 49`.

No `PUBLIC` route receives a `T-FE-031` policy. `ROOT_ENTRY` receives no `T-FE-031` policy.

| Route ID | Canonical path/template | T-FE-031 policy | Accepted roles | Required permission | Notes |
|---|---|---|---|---|---|
| `ACCOUNT_OVERVIEW` | `/account` | `AUTHENTICATED_ONLY` | — | — | No additional actor, ownership, account-active, or permission gate at route level. |
| `EXAMS_CATALOG` | `/exams` | `AUTHENTICATED_ONLY` | — | — | Exam eligibility remains backend/feature-owned. |
| `EXAMS_DETAIL` | `/exams/:examId` | `AUTHENTICATED_ONLY` | — | — | Exam access details remain backend/feature-owned. |
| `EXAMS_INSTRUCTIONS` | `/exams/:examId/instructions` | `AUTHENTICATED_ONLY` | — | — | Start eligibility remains backend/feature-owned. |
| `EXAMS_SESSION` | `/exams/:examId/sessions/:sessionId` | `AUTHENTICATED_ONLY` | — | — | Session ownership/access remains backend/feature-owned. |
| `EXAMS_RESULT` | `/exams/:examId/sessions/:sessionId/result` | `AUTHENTICATED_ONLY` | — | — | Result ownership/access remains backend/feature-owned. |
| `EXAMS_ANALYTICS` | `/exams/analytics` | `AUTHENTICATED_ONLY` | — | — | Analytics data access remains backend/feature-owned. |
| `EXAMS_REVIEW` | `/exams/:examId/sessions/:sessionId/review` | `AUTHENTICATED_ONLY` | — | — | Protected review content remains backend/feature-owned. |
| `EXAMS_HISTORY` | `/exams/history` | `AUTHENTICATED_ONLY` | — | — | Attempt ownership/access remains backend/feature-owned. |
| `COMMERCE_PRODUCTS` | `/commerce/products` | `AUTHENTICATED_ONLY` | — | — | Product availability/payment rules remain backend/feature-owned. |
| `COMMERCE_PRODUCT_DETAIL` | `/commerce/products/:productId` | `AUTHENTICATED_ONLY` | — | — | Product detail/payment rules remain backend/feature-owned. |
| `NURSE_ENTRY` | `/nurse` | `ROLE` | `Nurse` | — | Route-level actor UX boundary only. |
| `NURSE_PROFILE_OVERVIEW` | `/nurse/profile` | `ROLE` | `Nurse` | — | No profile ownership rule in generic policy. |
| `NURSE_PROFILE_PERSONAL_INFORMATION` | `/nurse/profile/personal-information` | `ROLE` | `Nurse` | — | No feature-specific profile rule in generic policy. |
| `NURSE_PROFILE_EXPERIENCE` | `/nurse/profile/experience` | `ROLE` | `Nurse` | — | No feature-specific profile rule in generic policy. |
| `NURSE_PROFILE_EDUCATION` | `/nurse/profile/education` | `ROLE` | `Nurse` | — | No feature-specific profile rule in generic policy. |
| `NURSE_PROFILE_CERTIFICATES` | `/nurse/profile/certificates` | `ROLE` | `Nurse` | — | No feature-specific profile rule in generic policy. |
| `NURSE_PROFILE_SKILLS` | `/nurse/profile/skills` | `ROLE` | `Nurse` | — | No feature-specific profile rule in generic policy. |
| `NURSE_PROFILE_LANGUAGES` | `/nurse/profile/languages` | `ROLE` | `Nurse` | — | No feature-specific profile rule in generic policy. |
| `NURSE_PROFILE_CV` | `/nurse/profile/cv` | `ROLE` | `Nurse` | — | CV behavior and file authorization remain backend/feature-owned. |
| `NURSE_CONTACT_REQUESTS` | `/nurse/contact-requests` | `ROLE` | `Nurse` | — | Contact-request ownership remains backend/feature-owned. |
| `PREPARATION_PACKAGES_ENTITLEMENTS` | `/nurse/preparation-packages` | `ROLE` | `Nurse` | — | Entitlement ownership remains backend/feature-owned. |
| `PREPARATION_PACKAGES_ENTITLEMENT_DETAIL` | `/nurse/preparation-packages/:entitlementId` | `ROLE` | `Nurse` | — | Entitlement/resource ownership remains backend/feature-owned. |
| `PREPARATION_PACKAGES_PRACTICE` | `/nurse/preparation-packages/:entitlementId/practice` | `ROLE` | `Nurse` | — | Practice entitlement rules remain backend/feature-owned. |
| `PREPARATION_PACKAGES_REPORT` | `/nurse/preparation-packages/reports/:sessionId` | `ROLE` | `Nurse` | — | Report access remains backend/feature-owned. |
| `COMMERCE_CHECKOUT` | `/checkout` | `ROLE` | `Nurse` | — | Order/payment-state checks remain backend/feature-owned. |
| `COMMERCE_PAYMENT_SUCCESS` | `/checkout/orders/:orderId/success` | `ROLE` | `Nurse` | — | URL alone is not proof of payment state. |
| `COMMERCE_PAYMENT_FAILURE` | `/checkout/orders/:orderId/failure` | `ROLE` | `Nurse` | — | URL alone is not proof of payment state. |
| `COMMERCE_ORDERS` | `/commerce/orders` | `ROLE` | `Nurse` | — | Order ownership remains backend/feature-owned. |
| `COMMERCE_ORDER_DETAIL` | `/commerce/orders/:orderId` | `ROLE` | `Nurse` | — | Order ownership remains backend/feature-owned. |
| `EMPLOYER_HOME` | `/employer` | `ROLE` | `Employer` | — | Route-level actor UX boundary only. |
| `EMPLOYER_CANDIDATES` | `/employer/candidates` | `ROLE` | `Employer` | — | Candidate visibility/disclosure remains backend/feature-owned. |
| `EMPLOYER_REQUESTS` | `/employer/requests` | `ROLE` | `Employer` | — | Request ownership/business rules remain backend/feature-owned. |
| `EMPLOYER_REQUEST_DETAIL` | `/employer/requests/:requestId` | `ROLE` | `Employer` | — | Request ownership/business rules remain backend/feature-owned. |
| `ADMIN_ENTRY` | `/admin` | `ROLE` | `Admin` | — | Does not authorize ADM-001 dashboard or role-home redirect behavior. |
| `ADMIN_USERS` | `/admin/users` | `ROLE_AND_PERMISSION` | `Admin` | `Users.View` | Minimum route entry capability; action controls remain feature-owned. |
| `ADMIN_USER_DETAIL` | `/admin/users/:userId` | `ROLE_AND_PERMISSION` | `Admin` | `Users.View` | Minimum route entry capability; sensitive exposure remains feature-owned. |
| `ADMIN_REFERENCE_DATA` | `/admin/reference-data` | `ROLE_AND_PERMISSION` | `Admin` | `Exams.View` | Minimum read/entry capability. |
| `ADMIN_EXAMS` | `/admin/exams` | `ROLE_AND_PERMISSION` | `Admin` | `Exams.View` | Does not require `Exams.Edit` merely to enter/read the route. |
| `ADMIN_EXAM_DETAIL` | `/admin/exams/:examId` | `ROLE_AND_PERMISSION` | `Admin` | `Exams.View` | Fine-grained edit/delete actions remain feature-owned. |
| `ADMIN_EXAM_VERSIONS` | `/admin/exams/:examId/versions` | `ROLE_AND_PERMISSION` | `Admin` | `Exams.View` | Version action permissions remain feature-owned. |
| `ADMIN_EXAM_QUESTIONS` | `/admin/exams/:examId/questions` | `ROLE_AND_PERMISSION` | `Admin` | `Questions.View` | Does not require `Questions.Manage` merely to enter/read. |
| `ADMIN_PAYMENT_PRODUCTS` | `/admin/payment-products` | `ROLE_AND_PERMISSION` | `Admin` | `Exams.View` | Payment product action controls remain feature-owned. |
| `ADMIN_PREPARATION_PACKAGE_TOPICS` | `/admin/preparation-packages/topics` | `ROLE_AND_PERMISSION` | `Admin` | `ReportingTopics.Manage` | Minimum approved route entry capability. |
| `ADMIN_PREPARATION_PACKAGE_PROFILES` | `/admin/preparation-packages/profiles` | `ROLE_AND_PERMISSION` | `Admin` | `ReportingProfiles.Manage` | Minimum approved route entry capability. |
| `ADMIN_PREPARATION_PACKAGE_MATERIALS` | `/admin/preparation-packages/materials` | `ROLE_AND_PERMISSION` | `Admin` | `StudyMaterials.Manage` | Minimum approved route entry capability. |
| `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS` | `/admin/preparation-packages/practice-collections` | `ROLE_AND_PERMISSION` | `Admin` | `PracticeCollections.Manage` | Minimum approved route entry capability. |
| `ADMIN_PREPARATION_PACKAGE_DEFINITIONS` | `/admin/preparation-packages/definitions` | `ROLE_AND_PERMISSION` | `Admin` | `PreparationPackages.View` | Does not require `PreparationPackages.Manage` or `PreparationPackages.Publish` merely to enter/read. |
| `ADMIN_PREPARATION_PACKAGE_OFFERS` | `/admin/preparation-packages/offers` | `ROLE_AND_PERMISSION` | `Admin` | `PreparationPackageOffers.Manage` | Minimum approved route entry capability. |

### 15.9 Explicit route IDs, not path-prefix authority

Although this contract intentionally assigns actor policies to the current Nurse, Employer, and Admin route sets, implementation must use explicit canonical route IDs.

Do not implement route UX permission policy with path-prefix matching such as:

```text
path.startsWith('/admin')
path.startsWith('/nurse')
path.startsWith('/employer')
```

Path prefixes remain URL organization, not authorization logic. The explicit route-policy registry is authority. Future routes under those prefixes require their own explicit policy entry.

### 15.10 T-FE-032 boundary

`T-FE-032` remains not authorized by this contract finalization.

The `T-FE-031` policy source may later be consumed by navigation presentation, but `T-FE-031` does not implement:

- menu hiding;
- sidebar hiding;
- navigation filtering;
- icon state;
- breadcrumb filtering;
- route-link visibility; or
- navigation ordering.

## 16. Implementation authorization

T-FE-031 IMPLEMENTATION IS NOT AUTHORIZED YET.

This contract finalization checkpoint authorizes documentation only. Separate authorization is required before creating permission guards, role guards, policy services, route metadata, route-policy tests, router integration, navigation behavior, screen behavior, or any Angular source change.
