# Administration Screen Contracts

## Family Authority

The canonical Administration approval packet is
`docs/frontend/design/inventory/admin-screen-approval-packet.md`. That packet
records human-approved decisions `HD-ADM-01..06` and is the detailed design
contract authority for Admin screens. This family file is the screen-index-facing
summary.

Admin screen contracts are bounded by the approved packet, [current route contract](../routing-and-permissions.md), [Product capability meaning](../../product/roles-and-permissions.md), [Security enforcement](../../security/authentication-authorization.md), and [OpenAPI](../../api/openapi.yaml). Screen readiness does not establish implementation completion or authorize additional behavior.

The human-approved Admin packet leaves `ADM-DASHBOARD`, `ADM-004`, `ADM-009`, and `ADM-010` blocked; their concept entries below do not authorize active destinations.

Preparation-package Admin frontend routes use
`/admin/preparation-packages/...`; the current HTTP paths are represented in [OpenAPI](../../api/openapi.yaml). Frontend route names do not create alternate backend paths.

## ADM-ENTRY Admin Entry

| Field | Contract |
|---|---|
| Identity | `/admin`; route ID `ADMIN_ENTRY`; Admin role. |
| Purpose | Functional Admin workspace/navigation hub for authorized v1 Admin capabilities. |
| Data/actions | Static/grouped entry points only. Destination visibility must respect each destination route/permission policy. |
| Non-goals | No dashboard metrics, KPI cards, statistics, activity feeds, system health, recruitment metrics, payment/order metrics, or inferred counts. `ADM-DASHBOARD` owns any future metric concept. |

## ADM-002 Admin Users

| Field | Contract |
|---|---|
| Identity | `/admin/users`; route ID `ADMIN_USERS`; Admin + `Users.View`. |
| Purpose | Find users. |
| Data | `UserListItemDto`: email, username, first/last name, active/email-verified facts, roles, created/last-login timestamps. |
| Search/pagination | `page`, `pageSize`, `search`, `isActive`, `role`, `sort`; no unapproved filters or bulk actions. |
| Table/list | Desktop table; mobile cards preserve identity, roles/status, and View detail action. |
| Actions | View detail -> `ADM-003`. |
| Privacy | No `passwordHash`, secrets, tokens, internal authorization state; raw JSON sensitive-field tests required. |

## ADM-003 Admin User Detail

| Field | Contract |
|---|---|
| Identity | `/admin/users/:userId`; route ID `ADMIN_USER_DETAIL`; Admin + `Users.View`. |
| Purpose | View safe user detail and update a user's single business role through current backend authority. |
| Data | `UserDetailDto`: identity, active/email/profile facts, roles, permissions, timestamps; no password/token fields. |
| Forms/actions | Role update uses `PUT /api/v1/admin/users/{userId}/role`; action requires `Users.Edit`. Authoritative role choices are `Admin`, `Employer`, `Expert`, `Nurse` from `UpdateUserRolesCommandValidator`/handler. Do not include seeded `SuperAdmin`; it is not supported by the update command. |
| States | Loading, ready, not found, validation, saving, saved, backend failure, forbidden/restricted. |
| Non-goals | Full roles/permissions management, permission assignment, user deletion/deactivation, session management. |

## ADM-005 Reference Data / Exam Categories

| Field | Contract |
|---|---|
| Identity | `/admin/reference-data`; route ID `ADMIN_REFERENCE_DATA`; Admin + `Exams.View` for read. |
| Purpose | V1 Exam Category administration only despite broad route name. |
| Data | Country, category name, slug, description, display order, active state. |
| Forms/actions | List, get, create, update, archive, restore, delete exam categories using `/api/v1/admin/exam-categories`; mutations require exact backend permissions `Exams.Create`, `Exams.Edit`, `Exams.Delete`. |
| Non-goals | Generic reference-data CRUD console, country/language management, unrelated reference entities. |

## ADM-006 Admin Exams

| Field | Contract |
|---|---|
| Identity | `/admin/exams`; route ID `ADMIN_EXAMS`; Admin + `Exams.View`. |
| Purpose | Admin exam list and lifecycle entry. |
| Data | Country/category/title/slug/description/instructions, duration, passing score, status, free/paid state, published timestamp. |
| Filters/actions | List filters `page`, `pageSize`, `countryId`, `categoryId`, `status`, `isFree`; actions list/get/create/update/archive/delete per backend permissions. |
| Non-goals | Learner exam catalog/session/result behavior, attempts, analytics, unsupported authoring features. |

## ADM-007 Admin Exam Detail

| Field | Contract |
|---|---|
| Identity | `/admin/exams/:examId`; route ID `ADMIN_EXAM_DETAIL`; Admin + `Exams.View`. |
| Purpose | Present/edit one Admin exam and expose supported lifecycle actions. |
| Data/forms/actions | Use `AdminExamDto` and create/update/archive/delete backend authority only. Destructive actions require confirmation. |
| Navigation | May link to versions and questions where user is eligible. |
| Non-goals | Learner preview, attempts, analytics, package report behavior. |

## ADM-008 Admin Exam Versions

| Field | Contract |
|---|---|
| Identity | `/admin/exams/:examId/versions`; route ID `ADMIN_EXAM_VERSIONS`; Admin + `Exams.View`. |
| Purpose | Manage exam versions for one Admin exam. |
| Data | Version number, status, question count, total points, published/retired timestamps, validation errors. |
| Actions | List/get, create draft, validate draft, publish draft, retire, delete draft. Permissions split exactly as backend metadata declares: `Exams.View`, `Exams.Edit`, `Questions.View`, `Questions.Manage`, `Exams.Delete`. |
| Confirmations | Publish, retire, and delete draft require confirmations. |

## ADM-QUESTIONS Admin Exam Questions

| Field | Contract |
|---|---|
| Identity | `/admin/exams/:examId/questions`; route ID `ADMIN_EXAM_QUESTIONS`; Admin + `Questions.View`. |
| Purpose | Manage questions and answer options for an Admin exam version. |
| Data | Question text, explanation, question type, points, display order, active state; answer option text, display order, correctness, active state. |
| Actions | List/get/create/update/deactivate/delete questions; list/create/update/deactivate/delete answer options. Mutations require `Questions.Manage`. |
| Route/backend caveat | Current backend question/option APIs require `versionId`; implementation must resolve approved version context without inventing a new canonical route. |
| Privacy | Protected question/answer content appears only to authorized Admin users and must not leak into learner routes. |

## ADM-PAY-PRODUCTS Admin Payment Products

| Field | Contract |
|---|---|
| Identity | `/admin/payment-products`; route ID `ADMIN_PAYMENT_PRODUCTS`; Admin + `Exams.View`. |
| Purpose | Manage exam-access payment products. |
| Data | Type, exam relation/title, name, description, currency, unit amount minor, active state, created/updated timestamps. |
| Forms/actions | List/get/create/update/archive/restore via `/api/v1/admin/payment/products`; mutations use `Exams.Edit`. Create type is limited to `ExamAccess`. |
| Money | Use canonical money/design rules; do not hand-wave minor units. |
| Non-goals | Admin payment orders, transactions, refunds, provider controls, payment analytics. |

## ADM-PP-TOPICS Reporting Topics

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/topics`; route ID `ADMIN_PREPARATION_PACKAGE_TOPICS`; Admin + `ReportingTopics.Manage`. |
| Purpose | Manage preparation-package reporting topics. |
| Data/actions | Exam category, name, slug, active state; list/create/update/archive. |
| Backend authority | `/api/v1/admin/preparation-package/reporting-topics`. |

## ADM-PP-PROFILES Reporting Profiles

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/profiles`; route ID `ADMIN_PREPARATION_PACKAGE_PROFILES`; Admin + `ReportingProfiles.Manage`. |
| Purpose | Manage reporting profile publications and question-topic assignments. |
| Data/actions | Exam version, name, status, published timestamp, assignments; list/create/get/publish. |
| Backend authority | `/api/v1/admin/preparation-package/reporting-profiles`. |

## ADM-PP-MATERIALS Study Materials

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/materials`; route ID `ADMIN_PREPARATION_PACKAGE_MATERIALS`; Admin + `StudyMaterials.Manage`. |
| Purpose | Manage study materials and versions. |
| Data/actions | Material title/slug/description; version number, material type, status, formatted text, file storage key, external URL, video URL, published timestamp, reporting topic IDs; list/create material, create/update/publish/retire versions. |
| Exposure boundary | `fileStorageKey` is Admin DTO data, not a learner delivery URL or an approved storage UI. Do not present it as public material access. |
| Non-goals | Learner material reader, offline delivery, storage UI. |
| Backend authority | `/api/v1/admin/preparation-package/materials`. |

## ADM-PP-PRACTICE Practice Collections

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/practice-collections`; route ID `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS`; Admin + `PracticeCollections.Manage`. |
| Purpose | Manage practice collections and versions/items/options. |
| Data/actions | Collection title/slug/description; version status/published timestamp; item prompt/feedback/display order/reporting topic; options/correctness; list/create collection, create/update/publish/retire versions. |
| Non-goals | Learner practice attempt UI, adaptive practice, analytics. |
| Backend authority | `/api/v1/admin/preparation-package/practice-collections`. |

## ADM-PP-DEFINITIONS Package Definitions

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/definitions`; route ID `ADMIN_PREPARATION_PACKAGE_DEFINITIONS`; Admin + `PreparationPackages.View`. |
| Purpose | Manage package definitions and package versions. |
| Data/actions | Country, exam category, title, slug, description; version exam version/reporting profile/practice collection/content isolation/materials/status; list/create/update definitions, create/validate/publish/retire versions. |
| Permissions | Read/validation `PreparationPackages.View`; create/update `PreparationPackages.Manage`; publish/retire `PreparationPackages.Publish`. |
| Backend authority | `/api/v1/admin/preparation-package/packages`. |

## ADM-PP-OFFERS Package Offers

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/offers`; route ID `ADMIN_PREPARATION_PACKAGE_OFFERS`; Admin + `PreparationPackageOffers.Manage`. |
| Purpose | Manage preparation-package offers. |
| Data/actions | Definition/version relation, title, slug, summary, price amount minor, currency, access duration days, status; list/create/update/activate/deactivate. |
| Non-goals | Learner catalog, checkout, order management, entitlement management. |
| Backend authority | `/api/v1/admin/preparation-package/offers`. |

## ADM-DASHBOARD

| Field | Contract |
|---|---|
| Gap | No stable dashboard/metrics/statistics data contract. Do not infer metrics from CRUD/list endpoints, database counts, payment/order data, recruitment data, or design expectations. |
| Disposition | Separate future concept; must not appear as active v1 Admin destination. |

## ADM-004 Roles And Permissions

| Field | Contract |
|---|---|
| Gap | No stable role/permission management backend contract. Permission constants, seeded permissions, `/me` roles/permissions, user detail projections, and `ADM-003` role update do not authorize a full management UI. |
| Disposition | Separate future concept; must not appear as active v1 Admin destination. |

## ADM-009 Admin Payment Orders

| Field | Contract |
|---|---|
| Gap | No admin payment-order management contract. Nurse-owned payment order routes and Admin payment product routes do not authorize Admin order UI. |
| Disposition | Separate future concept; must not appear as active v1 Admin destination. |

## ADM-010 Admin Recruitment

| Field | Contract |
|---|---|
| Gap | No admin recruitment-management contract. Employer contact-request/candidate routes and nurse contact-request decision routes do not authorize Admin recruitment UI. |
| Disposition | Separate future concept; must not appear as active v1 Admin destination. |
