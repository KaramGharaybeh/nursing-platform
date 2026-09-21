# Screen Index

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-INDEX
status: active-route-screen-map
owner: frontend-design-governance
updated: 2026-09-21
route_source: frontend/src/app/core/routing/canonical-routes.ts
```

## Counts

| Count | Value |
|---|---:|
| Canonical route IDs | 64 |
| Routable screen rows | 70 |
| Non-routable/transient/deferred rows | 31 |
| Total represented screen/state IDs | 101 |

Notes:

- Route coverage remains exactly `64/64`; every canonical route appears once as route ownership.
- Some approved screen IDs share one route by design, such as `NUR-001` and `NUR-002` on `/nurse/profile`, form/list state pairs on Nurse section routes, and `EMP-002` through `EMP-004` on `/employer/candidates`.
- Non-routable rows must not be promoted into routes without separate authority.

## Master Map

| Family | Screen ID | Route ID | Path / owner | Status | Contract section |
|---|---|---|---|---|---|
| Shared App Shell / Navigation | APP-SHELL | NOT_ROUTABLE | application shell | PARTIAL | `shared-system.md#app-shell` |
| Shared App Shell / Navigation | NAV-PRIMARY | NOT_ROUTABLE | global navigation | PARTIAL | `shared-system.md#nav-primary-global-navigation` |
| Shared App Shell / Navigation | ACCOUNT-AFFORDANCE | NOT_ROUTABLE | shell account/sign-out area | PARTIAL | `shared-system.md#account-affordance` |
| Shared/System | ROOT | `ROOT_ENTRY` | `/` | AUTHORITY_GAP | `shared-system.md#root-entry` |
| Authentication | AUTH-001 | `AUTH_SIGN_IN` | `/auth/sign-in` | CONTRACT_READY | `authentication.md#auth-001-sign-in` |
| Authentication | AUTH-002 | `AUTH_SIGN_UP` | `/auth/sign-up` | CONTRACT_READY | `authentication.md#auth-002-sign-up` |
| Authentication | AUTH-LEGACY-ROLE | `AUTH_ROLE_SELECTION` | `/auth/role-selection` | CONTRACT_READY | `authentication.md#auth-legacy-role-selection-redirect` |
| Authentication | AUTH-LEGACY-NURSE | `AUTH_REGISTER_NURSE` | `/auth/register/nurse` | CONTRACT_READY | `authentication.md#auth-legacy-nurse-registration-redirect` |
| Authentication | AUTH-LEGACY-EMPLOYER | `AUTH_REGISTER_EMPLOYER` | `/auth/register/employer` | CONTRACT_READY | `authentication.md#auth-legacy-employer-registration-redirect` |
| Authentication | AUTH-005 | `AUTH_VERIFY_EMAIL_REQUEST` | `/auth/verify-email` | CONTRACT_READY | `authentication.md#auth-005-check-email` |
| Authentication | AUTH-006 | `AUTH_VERIFY_EMAIL_CONFIRM` | `/auth/verify-email/confirm` | CONTRACT_READY | `authentication.md#auth-006-verify-email` |
| Authentication | AUTH-007 | `AUTH_FORGOT_PASSWORD` | `/auth/forgot-password` | CONTRACT_READY | `authentication.md#auth-007-forgot-password` |
| Authentication | AUTH-008 | `AUTH_RESET_PASSWORD` | `/auth/reset-password` | CONTRACT_READY | `authentication.md#auth-008-reset-password` |
| Authentication | AUTH-009 | NOT_ROUTABLE | reset-password success state | CONTRACT_READY | `authentication.md#auth-009-reset-password-success` |
| Authentication | AUTH-012 | NOT_ROUTABLE | inactive-account concept | BACKEND_BLOCKED | `authentication.md#auth-012-account-inactive` |
| Shared/System | AUTH-010 | `SYSTEM_SESSION_EXPIRED` | `/session-expired` | CONTRACT_READY | `shared-system.md#auth-010-session-expired` |
| Shared/System | AUTH-011 | `SYSTEM_ACCESS_DENIED` | `/access-denied` | CONTRACT_READY | `shared-system.md#auth-011-access-denied` |
| Account | ONB-001 | `ONBOARDING_PROFILE` | `/onboarding/profile` | CONTRACT_READY | `account.md#onb-001-profile-onboarding` |
| Account | ACC-001 | `ACCOUNT_OVERVIEW` | `/account` | CONTRACT_READY | `account.md#acc-001-account-overview` |
| Account | ACC-002 | NOT_ROUTABLE | `/account` edit state | CONTRACT_READY | `account.md#acc-002-personal-details-edit-state` |
| Account | ACC-003 | NOT_ROUTABLE | change-password concept | BACKEND_BLOCKED | `account.md#acc-003-change-password` |
| Account | ACC-004 | NOT_ROUTABLE | security/sessions concept | BACKEND_BLOCKED | `account.md#acc-004-security-and-sessions` |
| Account | ACC-005 | NOT_ROUTABLE | notification preferences concept | BACKEND_BLOCKED | `account.md#acc-005-notification-preferences` |
| Account | ACC-006 | NOT_ROUTABLE | account status concept | BACKEND_BLOCKED | `account.md#acc-006-account-status` |
| Nurse Profile | NUR-ENTRY | `NURSE_ENTRY` | `/nurse` | AUTHORITY_GAP | `nurse-profile.md#nur-entry-nurse-entry` |
| Nurse Profile | NUR-001 | `NURSE_PROFILE_OVERVIEW` | `/nurse/profile` | CONTRACT_READY | `nurse-profile.md#nur-001-profile-overview` |
| Nurse Profile | NUR-002 | `NURSE_PROFILE_OVERVIEW` | `/nurse/profile` | CONTRACT_READY | `nurse-profile.md#nur-002-profile-summary-state` |
| Nurse Profile | NUR-003 | `NURSE_PROFILE_PERSONAL_INFORMATION` | `/nurse/profile/personal-information` | CONTRACT_READY | `nurse-profile.md#nur-003-personal-information` |
| Nurse Profile | NUR-004 | `NURSE_PROFILE_EXPERIENCE` | `/nurse/profile/experience` | CONTRACT_READY | `nurse-profile.md#nur-004-experience-list` |
| Nurse Profile | NUR-005 | `NURSE_PROFILE_EXPERIENCE` | `/nurse/profile/experience` | CONTRACT_READY | `nurse-profile.md#nur-005-experience-form-state` |
| Nurse Profile | NUR-006 | `NURSE_PROFILE_EDUCATION` | `/nurse/profile/education` | CONTRACT_READY | `nurse-profile.md#nur-006-education-list` |
| Nurse Profile | NUR-007 | `NURSE_PROFILE_EDUCATION` | `/nurse/profile/education` | CONTRACT_READY | `nurse-profile.md#nur-007-education-form-state` |
| Nurse Profile | NUR-008 | `NURSE_PROFILE_CERTIFICATES` | `/nurse/profile/certificates` | CONTRACT_READY | `nurse-profile.md#nur-008-certificates-list` |
| Nurse Profile | NUR-009 | `NURSE_PROFILE_CERTIFICATES` | `/nurse/profile/certificates` | CONTRACT_READY | `nurse-profile.md#nur-009-certificate-form-state` |
| Nurse Profile | NUR-010 | `NURSE_PROFILE_SKILLS` | `/nurse/profile/skills` | CONTRACT_READY | `nurse-profile.md#nur-010-skills` |
| Nurse Profile | NUR-011 | `NURSE_PROFILE_LANGUAGES` | `/nurse/profile/languages` | CONTRACT_READY | `nurse-profile.md#nur-011-languages` |
| Nurse Profile | NUR-012 | `NURSE_PROFILE_CV` | `/nurse/profile/cv` | PARTIAL | `nurse-profile.md#nur-012-cv` |
| Nurse Profile | NUR-013 | NOT_ROUTABLE | profile preview/share concept | DEFERRED | `nurse-profile.md#nur-013-profile-preview-share` |
| Nurse Profile | NUR-CONTACT | `NURSE_CONTACT_REQUESTS` | `/nurse/contact-requests` | CONTRACT_READY | `nurse-profile.md#nur-contact-contact-requests` |
| Exams | EXM-001 | `EXAMS_CATALOG` | `/exams` | CONTRACT_READY | `exams.md#exm-001-exam-catalog` |
| Exams | EXM-002 | `EXAMS_DETAIL` | `/exams/:examId` | CONTRACT_READY | `exams.md#exm-002-exam-detail` |
| Exams | EXM-003 | NOT_ROUTABLE | purchase-required inline state | DEFERRED | `exams.md#exm-003-purchase-required-inline-state` |
| Exams | EXM-004 | `EXAMS_INSTRUCTIONS` | `/exams/:examId/instructions` | CONTRACT_READY | `exams.md#exm-004-exam-instructions` |
| Exams | EXM-005 | `EXAMS_SESSION` | `/exams/:examId/sessions/:sessionId` | CONTRACT_READY | `exams.md#exm-005-exam-session` |
| Exams | EXM-006 | NOT_ROUTABLE | submit confirmation inside session | CONTRACT_READY | `exams.md#exm-006-submit-confirmation` |
| Exams | EXM-007 | `EXAMS_RESULT` | `/exams/:examId/sessions/:sessionId/result` | CONTRACT_READY | `exams.md#exm-007-exam-result` |
| Exams | EXM-008 | `EXAMS_ANALYTICS` | `/exams/analytics` | CONTRACT_READY | `exams.md#exm-008-exam-analytics` |
| Exams | EXM-009 | `EXAMS_REVIEW` | `/exams/:examId/sessions/:sessionId/review` | CONTRACT_READY | `exams.md#exm-009-answer-review` |
| Exams | EXM-010 | `EXAMS_HISTORY` | `/exams/history` | CONTRACT_READY | `exams.md#exm-010-exam-history` |
| Preparation Packages | PP-001 | `PREPARATION_PACKAGES_OFFERS` | `/preparation-packages` | CONTRACT_READY | `preparation-packages.md#pp-001-package-offers` |
| Preparation Packages | PP-002 | `PREPARATION_PACKAGES_OFFER_DETAIL` | `/preparation-packages/:offerSlug` | CONTRACT_READY | `preparation-packages.md#pp-002-package-offer-detail` |
| Preparation Packages | PP-003 | `PREPARATION_PACKAGES_ENTITLEMENTS` | `/nurse/preparation-packages` | CONTRACT_READY | `preparation-packages.md#pp-003-my-preparation-packages` |
| Preparation Packages | PP-004 | `PREPARATION_PACKAGES_ENTITLEMENT_DETAIL` | `/nurse/preparation-packages/:entitlementId` | CONTRACT_READY | `preparation-packages.md#pp-004-entitlement-detail` |
| Preparation Packages | PP-005 | `PREPARATION_PACKAGES_PRACTICE` | `/nurse/preparation-packages/:entitlementId/practice` | CONTRACT_READY | `preparation-packages.md#pp-005-practice` |
| Preparation Packages | PP-EXAM-START | NOT_ROUTABLE | package exam start action | CONTRACT_READY | `preparation-packages.md#pp-exam-start-package-exam-start` |
| Preparation Packages | PP-007 | `PREPARATION_PACKAGES_REPORT` | `/nurse/preparation-packages/reports/:sessionId` | CONTRACT_READY | `preparation-packages.md#pp-007-package-report` |
| Preparation Packages | PP-MATERIAL-READER | NOT_ROUTABLE | material reader concept | BACKEND_BLOCKED | `preparation-packages.md#pp-material-reader` |
| Commerce | COM-001 | `COMMERCE_PRODUCTS` | `/commerce/products` | CONTRACT_READY | `commerce.md#com-001-product-catalog` |
| Commerce | COM-002 | `COMMERCE_PRODUCT_DETAIL` | `/commerce/products/:productId` | CONTRACT_READY | `commerce.md#com-002-product-detail` |
| Commerce | COM-003 | `COMMERCE_CHECKOUT` | `/checkout` | DEFERRED | `commerce.md#com-003-checkout` |
| Commerce | COM-004 | NOT_ROUTABLE | payment processing state | DEFERRED | `commerce.md#com-004-payment-processing` |
| Commerce | COM-005 | `COMMERCE_PAYMENT_SUCCESS` | `/checkout/orders/:orderId/success` | AUTHORITY_GAP | `commerce.md#com-005-payment-success` |
| Commerce | COM-006 | `COMMERCE_PAYMENT_FAILURE` | `/checkout/orders/:orderId/failure` | AUTHORITY_GAP | `commerce.md#com-006-payment-failure` |
| Commerce | COM-007 | `COMMERCE_ORDERS` | `/commerce/orders` | AUTHORITY_GAP | `commerce.md#com-007-order-history` |
| Commerce | COM-008 | `COMMERCE_ORDER_DETAIL` | `/commerce/orders/:orderId` | AUTHORITY_GAP | `commerce.md#com-008-order-detail` |
| Employer | EMP-001 | `EMPLOYER_HOME` | `/employer` | CONTRACT_READY | `employer.md#emp-001-employer-home` |
| Employer | EMP-002 | `EMPLOYER_CANDIDATES` | `/employer/candidates` | CONTRACT_READY | `employer.md#emp-002-candidate-search` |
| Employer | EMP-003 | `EMPLOYER_CANDIDATES` | `/employer/candidates` | CONTRACT_READY | `employer.md#emp-003-candidate-results` |
| Employer | EMP-004 | `EMPLOYER_CANDIDATES` | `/employer/candidates` | CONTRACT_READY | `employer.md#emp-004-candidate-empty-filtered-states` |
| Employer | EMP-005 | NOT_ROUTABLE | candidate detail concept | DEFERRED | `employer.md#emp-005-candidate-detail` |
| Employer | EMP-006 | NOT_ROUTABLE | candidate request concept | CONTRACT_READY | `employer.md#emp-006-candidate-request` |
| Employer | EMP-007 | `EMPLOYER_REQUESTS` | `/employer/requests` | CONTRACT_READY | `employer.md#emp-007-employer-requests` |
| Employer | EMP-008 | `EMPLOYER_REQUEST_DETAIL` | `/employer/requests/:requestId` | CONTRACT_READY | `employer.md#emp-008-employer-request-detail` |
| Administration | ADM-ENTRY | `ADMIN_ENTRY` | `/admin` | AUTHORITY_GAP | `administration.md#adm-entry-admin-entry` |
| Administration | ADM-002 | `ADMIN_USERS` | `/admin/users` | CONTRACT_READY | `administration.md#adm-002-admin-users` |
| Administration | ADM-003 | `ADMIN_USER_DETAIL` | `/admin/users/:userId` | CONTRACT_READY | `administration.md#adm-003-admin-user-detail` |
| Administration | ADM-005 | `ADMIN_REFERENCE_DATA` | `/admin/reference-data` | AUTHORITY_GAP | `administration.md#adm-005-reference-data` |
| Administration | ADM-006 | `ADMIN_EXAMS` | `/admin/exams` | AUTHORITY_GAP | `administration.md#adm-006-admin-exams` |
| Administration | ADM-007 | `ADMIN_EXAM_DETAIL` | `/admin/exams/:examId` | AUTHORITY_GAP | `administration.md#adm-007-admin-exam-detail` |
| Administration | ADM-008 | `ADMIN_EXAM_VERSIONS` | `/admin/exams/:examId/versions` | AUTHORITY_GAP | `administration.md#adm-008-admin-exam-versions` |
| Administration | ADM-QUESTIONS | `ADMIN_EXAM_QUESTIONS` | `/admin/exams/:examId/questions` | AUTHORITY_GAP | `administration.md#adm-questions-admin-exam-questions` |
| Administration | ADM-PAY-PRODUCTS | `ADMIN_PAYMENT_PRODUCTS` | `/admin/payment-products` | AUTHORITY_GAP | `administration.md#adm-pay-products-admin-payment-products` |
| Administration | ADM-PP-TOPICS | `ADMIN_PREPARATION_PACKAGE_TOPICS` | `/admin/preparation-packages/topics` | AUTHORITY_GAP | `administration.md#adm-pp-topics-reporting-topics` |
| Administration | ADM-PP-PROFILES | `ADMIN_PREPARATION_PACKAGE_PROFILES` | `/admin/preparation-packages/profiles` | AUTHORITY_GAP | `administration.md#adm-pp-profiles-reporting-profiles` |
| Administration | ADM-PP-MATERIALS | `ADMIN_PREPARATION_PACKAGE_MATERIALS` | `/admin/preparation-packages/materials` | AUTHORITY_GAP | `administration.md#adm-pp-materials-study-materials` |
| Administration | ADM-PP-PRACTICE | `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS` | `/admin/preparation-packages/practice-collections` | AUTHORITY_GAP | `administration.md#adm-pp-practice-practice-collections` |
| Administration | ADM-PP-DEFINITIONS | `ADMIN_PREPARATION_PACKAGE_DEFINITIONS` | `/admin/preparation-packages/definitions` | AUTHORITY_GAP | `administration.md#adm-pp-definitions-package-definitions` |
| Administration | ADM-PP-OFFERS | `ADMIN_PREPARATION_PACKAGE_OFFERS` | `/admin/preparation-packages/offers` | AUTHORITY_GAP | `administration.md#adm-pp-offers-package-offers` |
| Administration | ADM-DASHBOARD | NOT_ROUTABLE | admin dashboard concept | BACKEND_BLOCKED | `administration.md#adm-dashboard` |
| Administration | ADM-004 | NOT_ROUTABLE | roles/permissions management concept | BACKEND_BLOCKED | `administration.md#adm-004-roles-and-permissions` |
| Administration | ADM-009 | NOT_ROUTABLE | admin payment orders concept | BACKEND_BLOCKED | `administration.md#adm-009-admin-payment-orders` |
| Administration | ADM-010 | NOT_ROUTABLE | admin recruitment concept | BACKEND_BLOCKED | `administration.md#adm-010-admin-recruitment` |
| Shared/System | SYS-001 | NOT_ROUTABLE | route loading | CONTRACT_READY | `shared-system.md#sys-001-route-loading` |
| Shared/System | SYS-002 | NOT_ROUTABLE | not found | CONTRACT_READY | `shared-system.md#sys-002-not-found` |
| Shared/System | SYS-003 | NOT_ROUTABLE | unexpected error | CONTRACT_READY | `shared-system.md#sys-003-unexpected-error` |
| Shared/System | SYS-004 | NOT_ROUTABLE | offline state | DEFERRED | `shared-system.md#sys-004-offline` |
| Shared/System | SYS-005 | NOT_ROUTABLE | maintenance state | DEFERRED | `shared-system.md#sys-005-maintenance` |
| Shared/System | SYS-006 | NOT_ROUTABLE | empty state | CONTRACT_READY | `shared-system.md#sys-006-empty` |
| Shared/System | SYS-007 | NOT_ROUTABLE | no-results state | CONTRACT_READY | `shared-system.md#sys-007-no-results` |
| Shared/System | DPF-001 | NOT_ROUTABLE | Notifications | DEFERRED | `shared-system.md#dpf-001-notifications` |
| Shared/System | DPF-002 | NOT_ROUTABLE | Help / Support Access | DEFERRED | `shared-system.md#dpf-002-help-support-access` |
