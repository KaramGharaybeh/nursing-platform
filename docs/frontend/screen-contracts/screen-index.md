# Screen Index

This index maps screen/state identity to the current 64-ID canonical route registry and the owning family contracts. A row does not establish that its screen is implemented, active in `app.routes.ts`, or approved Product behavior. Read the owning family contract for approved presentation and explicit blocked/deferred boundaries.

[Frontend routing](../routing-and-permissions.md) owns route identity and guard behavior. [Contract schema](contract-schema.md) defines evidence and disposition rules. [Family contracts](README.md#family-contracts) own screen presentation. The former screen-contract copies remain recoverable in Git history and are not a second permanent owner.

| Family | Screen/state ID | Route ID | Path or non-routable owner |
|---|---|---|---|
| Shared App Shell / Navigation | APP-SHELL | NOT_ROUTABLE | application shell |
| Shared App Shell / Navigation | NAV-PRIMARY | NOT_ROUTABLE | global navigation |
| Shared App Shell / Navigation | ACCOUNT-AFFORDANCE | NOT_ROUTABLE | shell account/sign-out area |
| Shared/System | ROOT | `ROOT_ENTRY` | `/` |
| Authentication | AUTH-001 | `AUTH_SIGN_IN` | `/auth/sign-in` |
| Authentication | AUTH-002 | `AUTH_SIGN_UP` | `/auth/sign-up` |
| Authentication | AUTH-LEGACY-ROLE | `AUTH_ROLE_SELECTION` | `/auth/role-selection` |
| Authentication | AUTH-LEGACY-NURSE | `AUTH_REGISTER_NURSE` | `/auth/register/nurse` |
| Authentication | AUTH-LEGACY-EMPLOYER | `AUTH_REGISTER_EMPLOYER` | `/auth/register/employer` |
| Authentication | AUTH-005 | `AUTH_VERIFY_EMAIL_REQUEST` | `/auth/verify-email` |
| Authentication | AUTH-006 | `AUTH_VERIFY_EMAIL_CONFIRM` | `/auth/verify-email/confirm` |
| Authentication | AUTH-007 | `AUTH_FORGOT_PASSWORD` | `/auth/forgot-password` |
| Authentication | AUTH-008 | `AUTH_RESET_PASSWORD` | `/auth/reset-password` |
| Authentication | AUTH-009 | NOT_ROUTABLE | reset-password success state |
| Authentication | AUTH-012 | NOT_ROUTABLE | inactive-account concept |
| Shared/System | AUTH-010 | `SYSTEM_SESSION_EXPIRED` | `/session-expired` |
| Shared/System | AUTH-011 | `SYSTEM_ACCESS_DENIED` | `/access-denied` |
| Account | ONB-001 | `ONBOARDING_PROFILE` | `/onboarding/profile` |
| Account | ACC-001 | `ACCOUNT_OVERVIEW` | `/account` |
| Account | ACC-002 | NOT_ROUTABLE | `/account` edit state |
| Account | ACC-003 | NOT_ROUTABLE | change-password concept |
| Account | ACC-004 | NOT_ROUTABLE | security/sessions concept |
| Account | ACC-005 | NOT_ROUTABLE | notification preferences concept |
| Account | ACC-006 | NOT_ROUTABLE | account status concept |
| Nurse Profile | NUR-ENTRY | `NURSE_ENTRY` | `/nurse` |
| Nurse Profile | NUR-001 | `NURSE_PROFILE_OVERVIEW` | `/nurse/profile` |
| Nurse Profile | NUR-002 | `NURSE_PROFILE_OVERVIEW` | `/nurse/profile` |
| Nurse Profile | NUR-003 | `NURSE_PROFILE_PERSONAL_INFORMATION` | `/nurse/profile/personal-information` |
| Nurse Profile | NUR-004 | `NURSE_PROFILE_EXPERIENCE` | `/nurse/profile/experience` |
| Nurse Profile | NUR-005 | `NURSE_PROFILE_EXPERIENCE` | `/nurse/profile/experience` |
| Nurse Profile | NUR-006 | `NURSE_PROFILE_EDUCATION` | `/nurse/profile/education` |
| Nurse Profile | NUR-007 | `NURSE_PROFILE_EDUCATION` | `/nurse/profile/education` |
| Nurse Profile | NUR-008 | `NURSE_PROFILE_CERTIFICATES` | `/nurse/profile/certificates` |
| Nurse Profile | NUR-009 | `NURSE_PROFILE_CERTIFICATES` | `/nurse/profile/certificates` |
| Nurse Profile | NUR-010 | `NURSE_PROFILE_SKILLS` | `/nurse/profile/skills` |
| Nurse Profile | NUR-011 | `NURSE_PROFILE_LANGUAGES` | `/nurse/profile/languages` |
| Nurse Profile | NUR-012 | `NURSE_PROFILE_CV` | `/nurse/profile/cv` |
| Nurse Profile | NUR-013 | NOT_ROUTABLE | deferred professional-profile completion concept |
| Nurse Profile | NUR-CONTACT | `NURSE_CONTACT_REQUESTS` | `/nurse/contact-requests` |
| Exams | EXM-001 | `EXAMS_CATALOG` | `/exams` |
| Exams | EXM-002 | `EXAMS_DETAIL` | `/exams/:examId` |
| Exams | EXM-003 | NOT_ROUTABLE | purchase-required inline state |
| Exams | EXM-004 | `EXAMS_INSTRUCTIONS` | `/exams/:examId/instructions` |
| Exams | EXM-005 | `EXAMS_SESSION` | `/exams/:examId/sessions/:sessionId` |
| Exams | EXM-006 | NOT_ROUTABLE | submit confirmation inside session |
| Exams | EXM-007 | `EXAMS_RESULT` | `/exams/:examId/sessions/:sessionId/result` |
| Exams | EXM-008 | `EXAMS_ANALYTICS` | `/exams/analytics` |
| Exams | EXM-009 | `EXAMS_REVIEW` | `/exams/:examId/sessions/:sessionId/review` |
| Exams | EXM-010 | `EXAMS_HISTORY` | `/exams/history` |
| Preparation Packages | PP-001 | `PREPARATION_PACKAGES_OFFERS` | `/preparation-packages` |
| Preparation Packages | PP-002 | `PREPARATION_PACKAGES_OFFER_DETAIL` | `/preparation-packages/:offerSlug` |
| Preparation Packages | PP-003 | `PREPARATION_PACKAGES_ENTITLEMENTS` | `/nurse/preparation-packages` |
| Preparation Packages | PP-004 | `PREPARATION_PACKAGES_ENTITLEMENT_DETAIL` | `/nurse/preparation-packages/:entitlementId` |
| Preparation Packages | PP-005 | `PREPARATION_PACKAGES_PRACTICE` | `/nurse/preparation-packages/:entitlementId/practice` |
| Preparation Packages | PP-EXAM-START | NOT_ROUTABLE | package exam start action |
| Preparation Packages | PP-007 | `PREPARATION_PACKAGES_REPORT` | `/nurse/preparation-packages/reports/:sessionId` |
| Preparation Packages | PP-MATERIAL-READER | NOT_ROUTABLE | material reader concept |
| Commerce | COM-001 | `COMMERCE_PRODUCTS` | `/commerce/products` |
| Commerce | COM-002 | `COMMERCE_PRODUCT_DETAIL` | `/commerce/products/:productId` |
| Commerce | COM-003 | `COMMERCE_CHECKOUT` | `/checkout` |
| Commerce | COM-004 | NOT_ROUTABLE | payment processing state |
| Commerce | COM-005 | `COMMERCE_PAYMENT_SUCCESS` | `/checkout/orders/:orderId/success` |
| Commerce | COM-006 | `COMMERCE_PAYMENT_FAILURE` | `/checkout/orders/:orderId/failure` |
| Commerce | COM-007 | `COMMERCE_ORDERS` | `/commerce/orders` |
| Commerce | COM-008 | `COMMERCE_ORDER_DETAIL` | `/commerce/orders/:orderId` |
| Employer | EMP-001 | `EMPLOYER_HOME` | `/employer` |
| Employer | EMP-002 | `EMPLOYER_CANDIDATES` | `/employer/candidates` |
| Employer | EMP-003 | `EMPLOYER_CANDIDATES` | `/employer/candidates` |
| Employer | EMP-004 | `EMPLOYER_CANDIDATES` | `/employer/candidates` |
| Employer | EMP-005 | NOT_ROUTABLE | candidate detail concept |
| Employer | EMP-006 | NOT_ROUTABLE | candidate request concept |
| Employer | EMP-007 | `EMPLOYER_REQUESTS` | `/employer/requests` |
| Employer | EMP-008 | `EMPLOYER_REQUEST_DETAIL` | `/employer/requests/:requestId` |
| Administration | ADM-ENTRY | `ADMIN_ENTRY` | `/admin` |
| Administration | ADM-002 | `ADMIN_USERS` | `/admin/users` |
| Administration | ADM-003 | `ADMIN_USER_DETAIL` | `/admin/users/:userId` |
| Administration | ADM-005 | `ADMIN_REFERENCE_DATA` | `/admin/reference-data` |
| Administration | ADM-006 | `ADMIN_EXAMS` | `/admin/exams` |
| Administration | ADM-007 | `ADMIN_EXAM_DETAIL` | `/admin/exams/:examId` |
| Administration | ADM-008 | `ADMIN_EXAM_VERSIONS` | `/admin/exams/:examId/versions` |
| Administration | ADM-QUESTIONS | `ADMIN_EXAM_QUESTIONS` | `/admin/exams/:examId/questions` |
| Administration | ADM-PAY-PRODUCTS | `ADMIN_PAYMENT_PRODUCTS` | `/admin/payment-products` |
| Administration | ADM-PP-TOPICS | `ADMIN_PREPARATION_PACKAGE_TOPICS` | `/admin/preparation-packages/topics` |
| Administration | ADM-PP-PROFILES | `ADMIN_PREPARATION_PACKAGE_PROFILES` | `/admin/preparation-packages/profiles` |
| Administration | ADM-PP-MATERIALS | `ADMIN_PREPARATION_PACKAGE_MATERIALS` | `/admin/preparation-packages/materials` |
| Administration | ADM-PP-PRACTICE | `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS` | `/admin/preparation-packages/practice-collections` |
| Administration | ADM-PP-DEFINITIONS | `ADMIN_PREPARATION_PACKAGE_DEFINITIONS` | `/admin/preparation-packages/definitions` |
| Administration | ADM-PP-OFFERS | `ADMIN_PREPARATION_PACKAGE_OFFERS` | `/admin/preparation-packages/offers` |
| Administration | ADM-DASHBOARD | NOT_ROUTABLE | admin dashboard concept |
| Administration | ADM-004 | NOT_ROUTABLE | roles/permissions management concept |
| Administration | ADM-009 | NOT_ROUTABLE | admin payment orders concept |
| Administration | ADM-010 | NOT_ROUTABLE | admin recruitment concept |
| Shared/System | SYS-001 | NOT_ROUTABLE | route loading |
| Shared/System | SYS-002 | NOT_ROUTABLE | not found |
| Shared/System | SYS-003 | NOT_ROUTABLE | unexpected error |
| Shared/System | SYS-004 | NOT_ROUTABLE | offline state |
| Shared/System | SYS-005 | NOT_ROUTABLE | maintenance state |
| Shared/System | SYS-006 | NOT_ROUTABLE | empty state |
| Shared/System | SYS-007 | NOT_ROUTABLE | no-results state |
| Shared/System | DPF-001 | NOT_ROUTABLE | Notifications |
| Shared/System | DPF-002 | NOT_ROUTABLE | Help / Support Access |
