# Frontend Routing and Permissions

## Authority chain

[Product roles and permissions](../product/roles-and-permissions.md) own capability meaning. [Security authentication and authorization](../security/authentication-authorization.md) own protected server enforcement. [OpenAPI](../api/openapi.yaml) owns operation security metadata. This document owns frontend route identity, navigation/guard behavior, and UX access handling. A frontend guard or hidden link never secures an API resource.

This document is the permanent owner of frontend route identity and UX access policy. The retained `docs/frontend/design/inventory/page-registry.md` and `route-permission-matrix.md` are structured contract fixtures consumed by source-contract tests; they do not form a second prose authority. Current `canonical-routes.ts` provides executable route IDs and path templates, while `app.routes.ts` defines activated Angular routes. `route-classification.ts` and `route-permission-policy.ts` provide current executable access classifications and role/permission UX policy. `navigation-permission-policy.ts` filters visible destinations. [Screen contracts](screen-contracts/README.md) map screen IDs and approved states. A route entry does not itself approve a screen or Product capability.

## Approved route identity policy

The technical-lead-approved route policy uses semantic, stable, human-readable URLs based on product/user concepts. Paths use lowercase kebab-case and shallow domain grouping; they do not encode component names, design IDs, screen/task IDs, or backend endpoint structure. A dynamic parameter names a genuinely addressable resource (`:examId`, for example); query parameters carry same-resource view state such as filters, pagination, and one-time link tokens. The approved V1 path identity is locale-neutral, uses no translated or `/en`/`/ar` path prefix, and has no duplicate trailing-slash alias. Route IDs are internal `UPPER_SNAKE_CASE` identifiers, not URL segments. The canonical registry owns path identity and construction; navigation labels, icons, ordering, breadcrumbs, titles, redirects, and permission decisions have separate owners. A wildcard route does not establish a canonical visible not-found URL.

## Current route registry

The table below transcribes the 64 current canonical route IDs and path templates and their executable generic classification. Policy entries describe **frontend UX** checks only. `PUBLIC` is reachable anonymously and when authenticated; `ENTRY` is the root identity without an implied redirect; remaining routes are `AUTHENTICATED`. `AUTHENTICATED_ONLY` adds no role check. The executable sources remain the tie-breaker for current route wiring; discrepancies with approved screen intent require resolution, not silent promotion of code to Product authority.

| Route ID | Path template | Generic classification | Frontend UX policy |
|---|---|---|---|
| `ROOT_ENTRY` | `/` | ENTRY | — |
| `AUTH_SIGN_IN` | `/auth/sign-in` | PUBLIC | — |
| `AUTH_SIGN_UP` | `/auth/sign-up` | PUBLIC | — |
| `AUTH_ROLE_SELECTION` | `/auth/role-selection` | PUBLIC | — |
| `AUTH_REGISTER_NURSE` | `/auth/register/nurse` | PUBLIC | — |
| `AUTH_REGISTER_EMPLOYER` | `/auth/register/employer` | PUBLIC | — |
| `AUTH_VERIFY_EMAIL_REQUEST` | `/auth/verify-email` | PUBLIC | — |
| `AUTH_VERIFY_EMAIL_CONFIRM` | `/auth/verify-email/confirm` | PUBLIC | — |
| `AUTH_FORGOT_PASSWORD` | `/auth/forgot-password` | PUBLIC | — |
| `AUTH_RESET_PASSWORD` | `/auth/reset-password` | PUBLIC | — |
| `SYSTEM_SESSION_EXPIRED` | `/session-expired` | PUBLIC | — |
| `SYSTEM_ACCESS_DENIED` | `/access-denied` | PUBLIC | — |
| `ONBOARDING_PROFILE` | `/onboarding/profile` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `ACCOUNT_OVERVIEW` | `/account` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `NURSE_ENTRY` | `/nurse` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_OVERVIEW` | `/nurse/profile` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_PERSONAL_INFORMATION` | `/nurse/profile/personal-information` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_EXPERIENCE` | `/nurse/profile/experience` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_EDUCATION` | `/nurse/profile/education` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_CERTIFICATES` | `/nurse/profile/certificates` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_SKILLS` | `/nurse/profile/skills` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_LANGUAGES` | `/nurse/profile/languages` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_PROFILE_CV` | `/nurse/profile/cv` | AUTHENTICATED | ROLE; Nurse |
| `NURSE_CONTACT_REQUESTS` | `/nurse/contact-requests` | AUTHENTICATED | ROLE; Nurse |
| `EMPLOYER_HOME` | `/employer` | AUTHENTICATED | ROLE; Employer |
| `EMPLOYER_CANDIDATES` | `/employer/candidates` | AUTHENTICATED | ROLE; Employer |
| `EMPLOYER_REQUESTS` | `/employer/requests` | AUTHENTICATED | ROLE; Employer |
| `EMPLOYER_REQUEST_DETAIL` | `/employer/requests/:requestId` | AUTHENTICATED | ROLE; Employer |
| `EXAMS_CATALOG` | `/exams` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_DETAIL` | `/exams/:examId` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_INSTRUCTIONS` | `/exams/:examId/instructions` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_SESSION` | `/exams/:examId/sessions/:sessionId` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_RESULT` | `/exams/:examId/sessions/:sessionId/result` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_ANALYTICS` | `/exams/analytics` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_REVIEW` | `/exams/:examId/sessions/:sessionId/review` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `EXAMS_HISTORY` | `/exams/history` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `PREPARATION_PACKAGES_OFFERS` | `/preparation-packages` | PUBLIC | — |
| `PREPARATION_PACKAGES_OFFER_DETAIL` | `/preparation-packages/:offerSlug` | PUBLIC | — |
| `PREPARATION_PACKAGES_ENTITLEMENTS` | `/nurse/preparation-packages` | AUTHENTICATED | ROLE; Nurse |
| `PREPARATION_PACKAGES_ENTITLEMENT_DETAIL` | `/nurse/preparation-packages/:entitlementId` | AUTHENTICATED | ROLE; Nurse |
| `PREPARATION_PACKAGES_PRACTICE` | `/nurse/preparation-packages/:entitlementId/practice` | AUTHENTICATED | ROLE; Nurse |
| `PREPARATION_PACKAGES_REPORT` | `/nurse/preparation-packages/reports/:sessionId` | AUTHENTICATED | ROLE; Nurse |
| `COMMERCE_PRODUCTS` | `/commerce/products` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `COMMERCE_PRODUCT_DETAIL` | `/commerce/products/:productId` | AUTHENTICATED | AUTHENTICATED_ONLY |
| `COMMERCE_CHECKOUT` | `/checkout` | AUTHENTICATED | ROLE; Nurse |
| `COMMERCE_PAYMENT_SUCCESS` | `/checkout/orders/:orderId/success` | AUTHENTICATED | ROLE; Nurse |
| `COMMERCE_PAYMENT_FAILURE` | `/checkout/orders/:orderId/failure` | AUTHENTICATED | ROLE; Nurse |
| `COMMERCE_ORDERS` | `/commerce/orders` | AUTHENTICATED | ROLE; Nurse |
| `COMMERCE_ORDER_DETAIL` | `/commerce/orders/:orderId` | AUTHENTICATED | ROLE; Nurse |
| `ADMIN_ENTRY` | `/admin` | AUTHENTICATED | ROLE; Admin |
| `ADMIN_USERS` | `/admin/users` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Users.View |
| `ADMIN_USER_DETAIL` | `/admin/users/:userId` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Users.View |
| `ADMIN_REFERENCE_DATA` | `/admin/reference-data` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Exams.View |
| `ADMIN_EXAMS` | `/admin/exams` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Exams.View |
| `ADMIN_EXAM_DETAIL` | `/admin/exams/:examId` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Exams.View |
| `ADMIN_EXAM_VERSIONS` | `/admin/exams/:examId/versions` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Exams.View |
| `ADMIN_EXAM_QUESTIONS` | `/admin/exams/:examId/questions` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Questions.View |
| `ADMIN_PAYMENT_PRODUCTS` | `/admin/payment-products` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; Exams.View |
| `ADMIN_PREPARATION_PACKAGE_TOPICS` | `/admin/preparation-packages/topics` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; ReportingTopics.Manage |
| `ADMIN_PREPARATION_PACKAGE_PROFILES` | `/admin/preparation-packages/profiles` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; ReportingProfiles.Manage |
| `ADMIN_PREPARATION_PACKAGE_MATERIALS` | `/admin/preparation-packages/materials` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; StudyMaterials.Manage |
| `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS` | `/admin/preparation-packages/practice-collections` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; PracticeCollections.Manage |
| `ADMIN_PREPARATION_PACKAGE_DEFINITIONS` | `/admin/preparation-packages/definitions` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; PreparationPackages.View |
| `ADMIN_PREPARATION_PACKAGE_OFFERS` | `/admin/preparation-packages/offers` | AUTHENTICATED | ROLE_AND_PERMISSION; Admin; PreparationPackageOffers.Manage |

## Guard and denial boundaries

`authenticated-route.guard.ts` handles authentication navigation; `route-permission.guard.ts` evaluates resolved user roles and permissions through the route policy. `profile-completion.guard.ts` applies its separate onboarding-navigation condition. Guard and navigation policy tests cover classifications and access behavior. The server still authenticates, authorizes, and checks resource ownership for every protected operation.

For an anonymous request to an `AUTHENTICATED` route, the authentication guard redirects to canonical sign-in and preserves the requested internal path, query, and fragment under `returnUrl`. It waits for `AuthSessionBootstrap` to resolve before deciding; `CurrentUserStore.unavailable` alone is not anonymous token-session evidence. The shared safe-return helper accepts internal single-slash paths with optional query/fragment and rejects external/scheme-bearing, protocol-relative, backslash, control-character, and malformed encoded-path input. It does not grant role, permission, or resource access. Generic anonymous navigation does not infer a session-expired reason. The role/permission UX guard consumes the hydrated `/me` user roles and permissions, not JWT claims; its `AUTHENTICATED_ONLY`, `ROLE`, and `ROLE_AND_PERMISSION` policy forms do not encode resource ownership or entitlement rules. A resolved `unavailable` current-user state does not itself cause a role-denial redirect. These are frontend navigation contracts; protected server truth remains with [Security](../security/authentication-authorization.md).

The legacy route matrix contains inconsistent authenticated-route totals and dated statements about implementation authorization. The source table above is current executable evidence; screen approval and delivery status are not derived from it. The later human-authorized [Commerce screen contract](screen-contracts/commerce.md) owns the COM-003 presentation scope; generic payment processing and outcome states remain separately bounded there.
