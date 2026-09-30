# Admin Screen Approval Packet — T-FE-098 / GATE-FE-T098

```yaml
document_id: NPS-DES-INV-ADMIN-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-21
prepared_by: T-FE-098 campaign
gate: GATE-FE-T098
gate_status: VERIFIED (closed by human-approved decisions HD-ADM-01..06)
authorization: HD-ADM-01..06 were approved by the human technical lead on
  2026-09-21. ADM-ENTRY, ADM-002, ADM-003, ADM-005, ADM-006, ADM-007,
  ADM-008, ADM-QUESTIONS, ADM-PAY-PRODUCTS, and ADM-PP-* are approved as v1
  design contract authority where backed by current API/source evidence.
  ADM-DASHBOARD, ADM-004, ADM-009, and ADM-010 remain BACKEND_BLOCKED. Approval
  does not itself start Angular, backend, OpenAPI, generated-client, database,
  Storybook, or Stitch implementation.
```

## 1. Purpose

This packet closes `T-FE-098` / `GATE-FE-T098` by recording the approved Admin
screen design contracts and the explicit backend-blocked Admin capabilities. It
separates design-contract readiness from implementation readiness: approved Admin
screens may be designed in Stitch or implemented only through their later owning
frontend tasks, after those tasks re-check current OpenAPI/backend authority,
route mounting, generated clients, and predecessor gates.

## 2. Family Authority

- Canonical frontend Admin routes and permission policies exist for the screens
  listed below.
- Current Angular implementation evidence is limited to Admin Users and Admin
  User Detail; existing implementation is evidence only, not product authority.
- Backend/API authority exists for user list/detail/role update, exam category
  administration, admin exam/version/question/answer-option lifecycles, admin
  payment products, and preparation-package administration.
- Backend/API authority does not exist for Admin dashboard metrics, full
  roles/permissions management, Admin payment-order management, or Admin
  recruitment management.
- Stitch generation is not authorized by this packet.

## 3. Approved Screen Inventory

| Screen | Route/routability | Design status | Implementation readiness | Approval |
|---|---|---|---|---|
| `ADM-ENTRY` Admin Workspace | `/admin` | APPROVED / CONTRACT_READY | NOT STARTED; implementation task must use functional hub only | HD-ADM-01 |
| `ADM-002` Admin Users | `/admin/users` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-104` | extraction + HD-ADM approval |
| `ADM-003` Admin User Detail | `/admin/users/:userId` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-104` | extraction + HD-ADM approval |
| `ADM-005` Exam Category Administration | `/admin/reference-data` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-106` | HD-ADM-02 |
| `ADM-006` Admin Exams | `/admin/exams` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-107` | HD-ADM-03 |
| `ADM-007` Exam Detail | `/admin/exams/:examId` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-107` | HD-ADM-03 |
| `ADM-008` Exam Versions | `/admin/exams/:examId/versions` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-108` | HD-ADM-03 |
| `ADM-QUESTIONS` Questions / Answer Options | `/admin/exams/:examId/questions` | APPROVED / CONTRACT_READY | NOT STARTED; owning tasks `T-FE-109`/`T-FE-110` | HD-ADM-03 |
| `ADM-PAY-PRODUCTS` Admin Payment Products | `/admin/payment-products` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-111` | HD-ADM-04 |
| `ADM-PP-TOPICS` Reporting Topics | `/admin/preparation-packages/topics` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-114` | HD-ADM-05 |
| `ADM-PP-PROFILES` Reporting Profiles | `/admin/preparation-packages/profiles` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-115` | HD-ADM-05 |
| `ADM-PP-MATERIALS` Study Materials | `/admin/preparation-packages/materials` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-118` | HD-ADM-05 |
| `ADM-PP-PRACTICE` Practice Collections | `/admin/preparation-packages/practice-collections` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-119` | HD-ADM-05 |
| `ADM-PP-DEFINITIONS` Package Definitions | `/admin/preparation-packages/definitions` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-120` | HD-ADM-05 |
| `ADM-PP-OFFERS` Package Offers | `/admin/preparation-packages/offers` | APPROVED / CONTRACT_READY | NOT STARTED; owning task `T-FE-121` | HD-ADM-05 |
| `ADM-DASHBOARD` Dashboard Metrics | NOT_ROUTABLE / future concept | BACKEND_BLOCKED / DEFERRED | NOT READY; no backend contract | HD-ADM-06 / BG-ADM-01 |
| `ADM-004` Roles / Permissions Management | NOT_ROUTABLE / future concept | BACKEND_BLOCKED | NOT READY; no management contract | HD-ADM-06 / BG-ADM-02 |
| `ADM-009` Admin Payment Orders | NOT_ROUTABLE / future concept | BACKEND_BLOCKED | NOT READY; no admin order contract | HD-ADM-06 / BG-ADM-03 |
| `ADM-010` Admin Recruitment | NOT_ROUTABLE / future concept | BACKEND_BLOCKED | NOT READY; no admin recruitment contract | HD-ADM-06 / BG-ADM-04 |

Common requirements for all approved Admin screens: authenticated Admin actor,
route permission policy enforcement, loading/ready/error+retry/restricted states,
privacy-safe not-found/unavailable states, desktop dense tables where data is
genuinely tabular, mobile card/list transformations, RTL-safe logical layout,
WCAG 2.2 AA intent, accessible status text not conveyed by color alone, and no
raw backend error/ID exposure in user-facing copy.

## 4. Human-Approved Decisions

**HD-ADM-01 — Admin Home / Workspace. APPROVED.** `ADM-ENTRY` is a functional
Admin workspace/navigation hub. It provides clear entry into currently authorized
Admin capabilities. It must not show dashboard metrics, KPI cards, statistics,
activity feeds, system-health metrics, recruitment metrics, payment/order metrics,
or any aggregate/count not backed by separate authoritative API/product contracts.
`ADM-DASHBOARD` remains a separate backend-blocked future concept.

**HD-ADM-02 — Reference Data. APPROVED.** For v1, `ADM-005` covers Exam Category
administration only. Do not create a generic cross-domain Reference Data CRUD
console because the canonical route is `/admin/reference-data`. Other future
reference-data entities require separate authority.

**HD-ADM-03 — Exam Administration. APPROVED.** Use the current route/domain
decomposition: `ADM-006` Admin Exams, `ADM-007` Exam Detail, `ADM-008` Exam
Versions, and `ADM-QUESTIONS` Questions / Answer Options. Designs must represent
actual supported lifecycle actions: create/update, validation, publishing,
archive/restore/delete where supported, version draft/publish/retire/delete-draft,
question and answer-option lifecycles, and destructive confirmations. Do not derive
Admin authoring behavior from learner Exam screens, and do not expose protected
question/answer content beyond current Admin authority.

**HD-ADM-04 — Admin Payment Products. APPROVED.** `ADM-PAY-PRODUCTS` uses current
Admin Payment Product backend authority for list, get/detail where applicable,
create, update, archive, and restore. Approved fields are product type, exam
relation, name, description, currency, unit amount minor, active/archive state,
and source DTO timestamps where useful. Money presentation follows canonical money
rules. Do not infer payment orders, transaction management, refunds, provider
controls, or payment analytics. `ADM-009` remains backend-blocked.

**HD-ADM-05 — Preparation Package Administration. APPROVED.** Keep domain-specific
Admin surfaces separate: topics, profiles, materials, practice collections,
definitions, and offers. Do not collapse them into one generic CRUD screen. Each
screen preserves its own backend/API lifecycle rules for fields, relations,
validation, versions, publish/retire behavior, activate/deactivate/archive behavior,
ordering, dependencies, and destructive constraints only where those behaviors
exist. Shared Admin navigation may group them under Preparation Packages Admin,
but the screen contracts remain separate.

**HD-ADM-06 — Backend-Blocked Admin Capabilities. APPROVED.** `ADM-DASHBOARD`,
`ADM-004`, `ADM-009`, and `ADM-010` remain outside v1 Admin design/implementation
until backend/product authority exists. Do not invent placeholder functionality.
They may be documented as `DEFERRED` / `BACKEND_BLOCKED` future capabilities and
must not appear as active functional v1 Admin navigation destinations unless later
explicitly authorized.

## 5. Backend / API Authority

### User Administration

- `GET /api/v1/users` accepts `page`, `pageSize`, `search`, `isActive`, `role`,
  and `sort`; requires `Users.View`; returns `PaginatedResult<UserListItemDto>`.
- `GET /api/v1/users/{id}` requires `Users.View`; returns `UserDetailDto`.
- `PUT /api/v1/admin/users/{userId}/role` accepts `UpdateUserRolesRequest` with
  `roleName`; requires `Users.Edit`; returns `UpdateUserRolesResponse`.
- `UserListItemDto` fields: `id`, `email`, `username`, `firstName`, `lastName`,
  `isActive`, `emailVerified`, `roles[]`, `createdAt`, `lastLoginAt?`.
- `UserDetailDto` fields: list fields plus `isProfileComplete` and `permissions[]`.
- Authoritative role-update vocabulary exists in `UpdateUserRolesCommandValidator`
  and handler: `Admin`, `Employer`, `Expert`, `Nurse`. Seeded roles also include
  `SuperAdmin`, but the update command explicitly limits user role management to
  the four business roles above.

### Exam Category / Reference Data

- Backend group: `/api/v1/admin/exam-categories`.
- Operations: list, get, create, update, archive, restore, delete.
- Permissions: read uses `Exams.View`; create uses `Exams.Create`; update/archive/
  restore use `Exams.Edit`; delete uses `Exams.Delete`.
- DTO fields: `id`, `countryId`, `countryName`, `name`, `slug`, `description?`,
  `displayOrder`, `isActive`.

### Admin Exams, Versions, Questions, Answer Options

- Admin exams: list/get/create/update/archive/delete under `/api/v1/admin/exams`.
  Filters include `page`, `pageSize`, `countryId`, `categoryId`, `status`, and
  `isFree`; fields include country/category/title/slug/description/instructions,
  `durationMinutes`, `passingScorePercentage`, `status`, `isFree`, `publishedAt`.
- Exam versions: list/get/create draft/validate/publish/retire/delete draft under
  `/api/v1/admin/exams/{examId}/versions`; fields include `versionNumber`,
  `status`, `questionCount`, `totalPoints`, `publishedAt`, `retiredAt`, and
  validation errors.
- Questions: list/get/create/update/deactivate/delete under
  `/api/v1/admin/exams/{examId}/versions/{versionId}/questions`; fields include
  question text, explanation, question type, points, display order, active state,
  and answer options.
- Answer options: list/create/update/deactivate/delete under question option paths;
  fields include option text, display order, correctness, and active state.
- Permissions split between `Exams.*` and `Questions.*` exactly as endpoint metadata
  declares; implementation must not substitute looser or stricter permissions.

### Admin Payment Products

- Backend group: `/api/v1/admin/payment/products`.
- Operations: list, get, create, update, archive, restore.
- List filters: `page`, `pageSize`, `examId`, `isActive`.
- Create request fields: `type` (only `ExamAccess` accepted), `examId`, `name`,
  `description?`, `currency`, `unitAmountMinor`, `isActive`.
- Update request fields: `name`, `description?`, `currency`, `unitAmountMinor`.
- Response fields use `PaymentProductDto`: `id`, `type`, `examId`, `examTitle`,
  `name`, `description?`, `currency`, `unitAmountMinor`, `isActive`, `createdAt`,
  `updatedAt`.
- Creation requires a published exam and rejects duplicate exam-access products.

### Preparation Package Administration

Backend source currently maps preparation-package Admin APIs under
`/api/v1/admin/preparation-package/...` while frontend canonical screen routes use
`/admin/preparation-packages/...`. This naming/path difference is not product
authority to collapse screens; downstream implementation tasks must map the
frontend route to the correct generated/backend operations and must not invent
alternate backend paths.

- Reporting topics: list/create/update/archive; filter by `examCategoryId`; fields
  include exam category, name, slug, and active state; permission
  `ReportingTopics.Manage`.
- Reporting profiles: list/create/get/publish; filter by `examVersionId`; fields
  include exam version, name, status, published timestamp, and question-topic
  assignments; permission `ReportingProfiles.Manage`.
- Study materials: list/create material, create/update/publish/retire versions;
  fields include title, slug, description, version number, material type, status,
  formatted text, file storage key, external URL, video URL, published timestamp,
  and reporting topic IDs; permission `StudyMaterials.Manage`.
- Practice collections: list/create collection, create/update/publish/retire
  versions; fields include title, slug, description, version number, status,
  published timestamp, items, prompts, feedback, display order, options, and
  correctness; permission `PracticeCollections.Manage`.
- Package definitions: list/create/update definitions, create versions, validate,
  publish, retire versions; fields include country, exam category, title, slug,
  description, exam version, reporting profile, practice collection, content
  isolation confirmation, materials, sort order, and validation issues; permissions
  `PreparationPackages.View`, `PreparationPackages.Manage`, and
  `PreparationPackages.Publish`.
- Offers: list/create/update/activate/deactivate; fields include package
  definition/version, title, slug, summary, price amount minor, currency, access
  duration days, and status; permission `PreparationPackageOffers.Manage`.

## 6. Privacy And Non-Exposure Boundaries

- Never expose `passwordHash`, refresh/access tokens, secrets, provider secrets,
  internal entities/navigation entities, storage implementation details, or raw
  internal authorization state.
- Admin Users tests must inspect raw JSON to prove sensitive fields such as
  `passwordHash` are absent; DTO deserialization alone is not sufficient.
- Restricted/forbidden copy must be generic and must not reveal permission keys,
  role internals, or backend policy details to unauthorized users.
- Not-found/unavailable states must be privacy-safe and must not expose raw GUIDs
  or imply hidden records.
- Permission constants and seeded role/permission data are policy vocabulary; they
  do not create management UI authority except where explicit endpoints exist.
- Payment product administration does not expose provider controls, transactions,
  refunds, checkout sessions, orders, or payment analytics.
- Preparation-package material fields such as file storage keys may exist in Admin
  DTOs; user-facing presentation must avoid exposing storage implementation as a
  public download promise unless a material delivery contract supports it.

## 7. Per-Screen Contracts

### ADM-ENTRY — Admin Workspace

- Purpose: functional workspace/navigation hub for authorized Admin capabilities.
- Actor/audience: authenticated Admin.
- Route/routability: `/admin`, route ID `ADMIN_ENTRY`.
- Permissions: Admin role per route policy; destination cards/links must respect
  each destination's own permission policy.
- Data/fields: no dashboard data; only navigation affordances, descriptive labels,
  and static guidance for approved Admin capability families.
- Forms: none.
- Tables/lists: optional list/grid of approved Admin destinations only.
- Actions: navigate to approved destinations such as Users, Exam Categories, Exams,
  Payment Products, and Preparation Package Admin surfaces when eligible.
- Lifecycle/state rules: no metrics lifecycle; no health/activity/status feeds.
- Confirmations/destructive behavior: none.
- States: loading for route/session readiness, ready, restricted, unavailable/error
  with retry where existing shared pattern applies.
- Privacy/non-exposure: no counts or hidden-resource implications.
- Backend/API authority: none required for the v1 hub beyond route/permission state.
- Responsive behavior: desktop may use grouped destination cards/sections; mobile
  stacks destinations with touch targets at least 44px.
- RTL: logical ordering and mirrored directional icons only.
- Accessibility: one `h1`, landmarks, accessible link/button names, visible focus,
  no color-only state.
- Explicit non-goals: dashboard metrics, KPIs, statistics, activity feeds, system
  health, recruitment/payment/order metrics.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; must not implement metrics.
- Remaining gaps: no active metrics dashboard; `ADM-DASHBOARD` remains blocked.

### ADM-002 — Admin Users

- Purpose: find and review platform users.
- Actor/audience: authenticated Admin with `Users.View`.
- Route/routability: `/admin/users`, route ID `ADMIN_USERS`.
- Permissions: Admin + `Users.View`.
- Data/fields: `UserListItemDto` fields only: email, username, names, active and
  email-verified facts, roles, created/last-login timestamps where useful.
- Forms: search and authorized filters only: `search`, `isActive`, `role`, `sort`,
  pagination.
- Tables/lists: desktop table prioritizing identity, email/username, roles/status,
  timestamps, and View detail action; mobile cards preserve identity/status/action.
- Actions: apply/clear filters, paginate, navigate to `ADM-003`.
- Lifecycle/state rules: backend pagination; deterministic ordering comes from
  backend/source task preflight.
- Confirmations/destructive behavior: none in v1 list.
- States: loading, populated, empty, filtered-empty, error+retry, restricted.
- Privacy/non-exposure: raw JSON sensitive-field tests for password/token/internal
  state; no permission detail in list unless future authority requires it.
- Backend/API authority: `GET /api/v1/users`.
- Responsive behavior: table-to-card transformation at mobile widths.
- RTL: logical table/card alignment and stable dates/numeric content.
- Accessibility: labelled search/filter controls, announced result changes,
  accessible pagination and row actions.
- Explicit non-goals: bulk actions, delete/deactivate users, permission management,
  unsupported filters.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-104` still owns implementation.
- Remaining gaps: none for v1 design contract.

### ADM-003 — Admin User Detail

- Purpose: view safe user detail and update one user's business role through the
  existing role-update endpoint.
- Actor/audience: authenticated Admin with `Users.View`; role update requires
  `Users.Edit`.
- Route/routability: `/admin/users/:userId`, route ID `ADMIN_USER_DETAIL`.
- Permissions: Admin + `Users.View` for detail; role update action must require
  `Users.Edit` server-side and be UX-gated where current user permissions allow.
- Data/fields: `UserDetailDto` fields only: email, username, names, profile-active
  facts, active/email-verified state, roles, permissions, timestamps.
- Forms: role update form accepts only authoritative role values `Admin`, `Employer`,
  `Expert`, `Nurse` from `UpdateUserRolesCommandValidator`/handler. Do not include
  `SuperAdmin` even though it is seeded, because the update command does not support
  it.
- Tables/lists: definition lists for identity/status; permissions may be grouped as
  read-only facts if shown and must not be used to imply permission management.
- Actions: save role update, cancel/reset role form, back to Admin Users.
- Lifecycle/state rules: loading, ready, validation, saving, saved, backend error,
  not found, restricted.
- Confirmations/destructive behavior: no destructive action; role update should make
  the consequence clear and may use a confirmation if implementation chooses.
- Privacy/non-exposure: no password hash, tokens, refresh sessions, internal auth
  state, or raw IDs in user-facing copy.
- Backend/API authority: `GET /api/v1/users/{id}` and
  `PUT /api/v1/admin/users/{userId}/role`.
- Responsive behavior: details/forms stack on mobile; actions remain reachable.
- RTL: logical form and definition-list layout.
- Accessibility: labelled controls, validation messages tied to controls, status
  announcement for saved/failure states, visible focus.
- Explicit non-goals: full roles/permissions management, creating roles, assigning
  permissions, user deletion/deactivation, session management.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-104` still owns implementation.
- Remaining gaps: none for role-choice source; it is source-backed by validator and
  handler. Full role/permission management remains `ADM-004` backend-blocked.

### ADM-005 — Exam Category Administration

- Purpose: manage exam categories under the broad canonical route named Reference
  Data.
- Actor/audience: authenticated Admin with exam category permissions.
- Route/routability: `/admin/reference-data`, route ID `ADMIN_REFERENCE_DATA`.
- Permissions: Admin + `Exams.View` for read; create/edit/delete actions require
  the exact backend permissions `Exams.Create`, `Exams.Edit`, `Exams.Delete`.
- Data/fields: country, category name, slug, description, display order, active
  state.
- Forms: create/update exam category only; no generic reference-data entity form.
- Tables/lists: category list with country, name/slug, display order, active state,
  and supported actions.
- Actions: list, view, create, update, archive, restore, delete.
- Lifecycle/state rules: active/archive state only where backend supports it.
- Confirmations/destructive behavior: archive/delete require confirmation; restore
  is explicit and reversible if backend allows.
- States: loading, populated, empty, filtered-empty where filters apply, validation,
  saving, saved, conflict/error+retry, restricted.
- Privacy/non-exposure: no internal entities or unsupported country/reference data
  management claims.
- Backend/API authority: `/api/v1/admin/exam-categories` operations.
- Responsive behavior: desktop table; mobile cards preserve category identity,
  active state, and actions.
- RTL: logical ordering; slugs/codes remain readable.
- Accessibility: labelled fields, accessible confirmations, focus return after
  dialogs, accessible pagination.
- Explicit non-goals: generic reference-data CRUD console, countries/languages admin,
  unrelated reference entities.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-106` still owns implementation.
- Remaining gaps: route name remains broader than v1 scope; implementation must label
  the screen as Exam Categories or equivalent to avoid overclaiming.

### ADM-006 — Admin Exams

- Purpose: manage exam records at list/lifecycle entry level.
- Actor/audience: authenticated Admin with exam permissions.
- Route/routability: `/admin/exams`, route ID `ADMIN_EXAMS`.
- Permissions: Admin + `Exams.View`; create/edit/archive/delete actions require
  matching backend permissions.
- Data/fields: country, category, title, slug, description/instructions where
  useful, duration, passing score, status, free/paid state, published timestamp.
- Forms: create/update exam using backend request authority.
- Tables/lists: exams table with filters for page/pageSize/country/category/status/
  isFree.
- Actions: list, detail, create, update, archive, delete, navigate to versions and
  questions where authorized.
- Lifecycle/state rules: status and publish/archive behavior must match backend;
  do not invent learner exam states.
- Confirmations/destructive behavior: archive/delete require confirmation and clear
  consequence text.
- States: loading, populated, empty, filtered-empty, validation, saving, conflict or
  backend error, restricted.
- Privacy/non-exposure: no question/answer content on the exam list beyond current
  DTO authority.
- Backend/API authority: `/api/v1/admin/exams` operations.
- Responsive behavior: desktop table; mobile cards preserve title/status/actions.
- RTL: logical table/card layout and stable numbers/percentages.
- Accessibility: labelled filters/forms, accessible pagination and action menus.
- Explicit non-goals: learner exam catalog behavior, attempts/session analytics,
  scoring visualization beyond fields supplied.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-107` still owns implementation.
- Remaining gaps: none for v1 design contract; implementation must re-check current
  generated API metadata.

### ADM-007 — Exam Detail

- Purpose: present and edit one Admin exam using supported fields and lifecycle
  actions.
- Actor/audience: authenticated Admin with exam permissions.
- Route/routability: `/admin/exams/:examId`, route ID `ADMIN_EXAM_DETAIL`.
- Permissions: Admin + `Exams.View`; mutation actions use `Exams.Edit` or
  `Exams.Delete` as endpoint metadata declares.
- Data/fields: same `AdminExamDto` field set as list/detail authority.
- Forms: update exam fields supported by backend request; no unsupported scoring or
  authoring extras.
- Tables/lists: detail sections; optional related navigation to versions/questions.
- Actions: update, archive, delete, navigate back/to versions/to questions.
- Lifecycle/state rules: preserve backend exam status semantics.
- Confirmations/destructive behavior: archive/delete require confirmation.
- States: loading, ready, not found, validation, saving, saved, conflict/error,
  restricted.
- Privacy/non-exposure: no protected question/answer body unless navigating to the
  authorized question screen.
- Backend/API authority: `GET/PUT/POST archive/DELETE /api/v1/admin/exams/{id}`.
- Responsive behavior: detail sections stack on mobile; actions remain accessible.
- RTL: logical definition-list/form layout.
- Accessibility: one `h1`, form labels, status messages, focus-safe confirmations.
- Explicit non-goals: learner preview, analytics, attempts, package report behavior.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-107` still owns implementation.
- Remaining gaps: none for v1 design contract.

### ADM-008 — Exam Versions

- Purpose: manage versions for one Admin exam.
- Actor/audience: authenticated Admin with exam/question permissions.
- Route/routability: `/admin/exams/:examId/versions`, route ID
  `ADMIN_EXAM_VERSIONS`.
- Permissions: Admin + `Exams.View`; create/retire uses `Exams.Edit`, validate uses
  `Questions.View`, publish uses `Questions.Manage`, delete draft uses
  `Exams.Delete`.
- Data/fields: version number, status, question count, total points, published and
  retired timestamps, validation errors.
- Forms: create draft has no request body; version mutation forms only where backend
  request exists.
- Tables/lists: version table/list with status and lifecycle actions.
- Actions: list/get, create draft, validate draft, publish draft, retire version,
  delete draft, navigate to questions.
- Lifecycle/state rules: distinguish draft, published, retired, validation result,
  and delete-draft behavior; do not invent duplicate version states.
- Confirmations/destructive behavior: publish, retire, and delete draft require
  confirmation with consequence summary.
- States: loading, empty, populated, validating, publishing, retiring, deleting,
  validation-failed, conflict/error, restricted.
- Privacy/non-exposure: validation messages may be shown; no raw internal IDs in
  visible copy.
- Backend/API authority: `/api/v1/admin/exams/{examId}/versions` operations.
- Responsive behavior: desktop table; mobile cards preserve version/status/actions.
- RTL: logical layout and stable numbers.
- Accessibility: action controls labelled with version context; confirmations manage
  focus entry/return.
- Explicit non-goals: learner version selection, exam attempts, analytics.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-108` still owns implementation.
- Remaining gaps: none for v1 design contract.

### ADM-QUESTIONS — Questions / Answer Options

- Purpose: manage questions and answer options for an Admin exam version.
- Actor/audience: authenticated Admin with question permissions.
- Route/routability: `/admin/exams/:examId/questions`, route ID
  `ADMIN_EXAM_QUESTIONS`; implementation must preserve the backend `versionId`
  relationship even though the canonical frontend route carries only `examId`.
- Permissions: Admin + `Questions.View` for reads; `Questions.Manage` for question
  and option mutations.
- Data/fields: question text, explanation, question type, points, display order,
  active state, answer option text, option display order, correctness, option active
  state.
- Forms: create/update question; create/update answer option.
- Tables/lists: question list with nested/related option management where usable.
- Actions: list/get/create/update/deactivate/delete questions; list/create/update/
  deactivate/delete answer options.
- Lifecycle/state rules: active/deactivated and delete behavior only as supported;
  correctness is Admin-authoring data, not learner feedback.
- Confirmations/destructive behavior: deactivate/delete question or option requires
  confirmation.
- States: loading, empty, populated, validation, saving, saved, conflict/error,
  restricted.
- Privacy/non-exposure: protected content appears only to authorized Admin users;
  do not expose this screen through learner routes or generic previews.
- Backend/API authority: question/option endpoints under
  `/api/v1/admin/exams/{examId}/versions/{versionId}/questions`.
- Responsive behavior: dense desktop list/table; mobile cards or grouped forms.
- RTL: logical layout; question/option text wraps safely.
- Accessibility: labelled rich text/textarea controls, clear option correctness
  labels, accessible destructive confirmations.
- Explicit non-goals: learner answer review UI, rationale disclosure rules for
  learners, analytics, unsupported question types beyond backend values.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-109`/`T-FE-110` own implementation.
- Remaining gaps: implementation must resolve version selection/context using
  approved route/back-end evidence without inventing a new canonical route.

### ADM-PAY-PRODUCTS — Admin Payment Products

- Purpose: manage exam-access payment products.
- Actor/audience: authenticated Admin with payment-product permissions.
- Route/routability: `/admin/payment-products`, route ID `ADMIN_PAYMENT_PRODUCTS`.
- Permissions: Admin + `Exams.View` for list/detail; create/update/archive/restore
  use `Exams.Edit` per current backend metadata.
- Data/fields: product type, exam ID/title, name, description, currency, unit amount
  minor, active state, created/updated timestamps.
- Forms: create and update product fields from backend requests; create type is
  limited to `ExamAccess`.
- Tables/lists: products table with exam, name, price, active state, and actions.
- Actions: list, get/detail, create, update, archive, restore.
- Lifecycle/state rules: creation requires a published exam; duplicate exam-access
  products are rejected; archive/restore controls active state.
- Confirmations/destructive behavior: archive requires confirmation; restore is
  explicit.
- States: loading, empty, populated, validation, saving, saved, conflict/error,
  restricted.
- Privacy/non-exposure: no orders, transactions, provider/session/refund data, or
  analytics.
- Backend/API authority: `/api/v1/admin/payment/products` operations.
- Responsive behavior: desktop table; mobile cards preserve product/exam/price/state
  and actions.
- RTL: canonical money/mixed-direction numeric display rules.
- Accessibility: labelled money/currency fields, accessible action menus and
  confirmations.
- Explicit non-goals: Admin payment orders, transaction management, refunds,
  provider controls, payment analytics.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-111` still owns implementation.
- Remaining gaps: none for v1 design contract; implementation must use canonical
  money utility/rules.

### ADM-PP-TOPICS — Reporting Topics

- Purpose: manage preparation-package reporting topics.
- Actor/audience: authenticated Admin with `ReportingTopics.Manage`.
- Route/routability: `/admin/preparation-packages/topics`, route ID
  `ADMIN_PREPARATION_PACKAGE_TOPICS`.
- Permissions: Admin + `ReportingTopics.Manage`.
- Data/fields: exam category, topic name, slug, active state.
- Forms: create/update topic.
- Tables/lists: topic list with exam category/name/slug/state/actions.
- Actions: list, create, update, archive.
- Lifecycle/state rules: archive only where backend supports it; archived topics are
  not generic deleted records.
- Confirmations/destructive behavior: archive requires confirmation.
- States: loading, empty, populated, validation, saving, saved, conflict/error,
  restricted.
- Privacy/non-exposure: no package analytics or learner performance data.
- Backend/API authority: `/api/v1/admin/preparation-package/reporting-topics`.
- Responsive behavior: table to cards.
- RTL: logical layout.
- Accessibility: labelled fields, pagination, action confirmations.
- Explicit non-goals: reporting profile publishing, material/practice authoring.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-114` still owns implementation.
- Remaining gaps: frontend route/backend path naming differs; implementation must
  bind generated operations correctly.

### ADM-PP-PROFILES — Reporting Profiles

- Purpose: manage reporting profile publications and question-topic assignments.
- Actor/audience: authenticated Admin with `ReportingProfiles.Manage`.
- Route/routability: `/admin/preparation-packages/profiles`, route ID
  `ADMIN_PREPARATION_PACKAGE_PROFILES`.
- Permissions: Admin + `ReportingProfiles.Manage`.
- Data/fields: exam version, name, status, published timestamp, assignments of exam
  question IDs to reporting topic IDs/names.
- Forms: create profile; publish profile with backend request authority.
- Tables/lists: profile publication list/detail with status and assignment summary.
- Actions: list, create, get/detail, publish.
- Lifecycle/state rules: draft/publication status only as backend returns; publishing
  may return conflict/problem details.
- Confirmations/destructive behavior: publish requires confirmation.
- States: loading, empty, populated, validation, publishing, published,
  conflict/error, restricted.
- Privacy/non-exposure: no learner scores or analytics.
- Backend/API authority: `/api/v1/admin/preparation-package/reporting-profiles`.
- Responsive behavior: assignment summaries compress on mobile.
- RTL: logical layout and stable IDs hidden from visible copy where possible.
- Accessibility: accessible assignment controls/summaries and publish confirmation.
- Explicit non-goals: reporting-topic CRUD, package analytics reports.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-115` still owns implementation.
- Remaining gaps: frontend route/backend path naming differs; implementation must
  bind generated operations correctly.

### ADM-PP-MATERIALS — Study Materials

- Purpose: manage study materials and their versions.
- Actor/audience: authenticated Admin with `StudyMaterials.Manage`.
- Route/routability: `/admin/preparation-packages/materials`, route ID
  `ADMIN_PREPARATION_PACKAGE_MATERIALS`.
- Permissions: Admin + `StudyMaterials.Manage`.
- Data/fields: material title/slug/description; version number, material type,
  status, formatted text, file storage key, external URL, video URL, published
  timestamp, reporting topic IDs.
- Forms: create material; create/update material version.
- Tables/lists: material list and version sections.
- Actions: list, create material, create version, update version, publish version,
  retire version.
- Lifecycle/state rules: version publish/retire only as backend supports; material
  type controls which content field is relevant.
- Confirmations/destructive behavior: publish/retire require confirmation.
- States: loading, empty, populated, validation, saving, publishing, retiring,
  conflict/error, restricted.
- Privacy/non-exposure: file storage key is Admin DTO data but must not be presented
  as public delivery capability without delivery authority.
- Backend/API authority: `/api/v1/admin/preparation-package/materials`.
- Responsive behavior: forms stack; version list cards on mobile.
- RTL: logical layout and safe wrapping for URLs/text.
- Accessibility: labelled content fields, status announcements, accessible
  confirmations.
- Explicit non-goals: learner material reader, offline material delivery, storage UI.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-118` still owns implementation.
- Remaining gaps: frontend route/backend path naming differs; delivery/reader remains
  outside this Admin contract.

### ADM-PP-PRACTICE — Practice Collections

- Purpose: manage practice collections and their versions/items/options.
- Actor/audience: authenticated Admin with `PracticeCollections.Manage`.
- Route/routability: `/admin/preparation-packages/practice-collections`, route ID
  `ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS`.
- Permissions: Admin + `PracticeCollections.Manage`.
- Data/fields: collection title/slug/description; version number/status/published
  timestamp; item reporting topic, prompt, immediate feedback, display order;
  option text, display order, correctness.
- Forms: create collection; create/update collection version and item/option content
  as backend request supports.
- Tables/lists: collection list and version/item composition sections.
- Actions: list, create collection, create version, update version, publish version,
  retire version.
- Lifecycle/state rules: version status and publish/retire only as backend supports.
- Confirmations/destructive behavior: publish/retire require confirmation.
- States: loading, empty, populated, validation, saving, publishing, retiring,
  conflict/error, restricted.
- Privacy/non-exposure: Admin answer correctness is authoring data only; do not
  expose through learner routes.
- Backend/API authority: `/api/v1/admin/preparation-package/practice-collections`.
- Responsive behavior: dense composition uses desktop sections and mobile cards.
- RTL: prompt/feedback/option text wraps logically.
- Accessibility: labelled item/option fields, ordered controls, accessible status.
- Explicit non-goals: learner practice attempt UI, adaptive practice, analytics.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-119` still owns implementation.
- Remaining gaps: frontend route/backend path naming differs.

### ADM-PP-DEFINITIONS — Package Definitions

- Purpose: manage preparation package definitions and versions.
- Actor/audience: authenticated Admin with preparation-package permissions.
- Route/routability: `/admin/preparation-packages/definitions`, route ID
  `ADMIN_PREPARATION_PACKAGE_DEFINITIONS`.
- Permissions: Admin + `PreparationPackages.View` for list/validation;
  `PreparationPackages.Manage` for create/update; `PreparationPackages.Publish` for
  publish/retire.
- Data/fields: country, exam category, title, slug, description; version exam
  version, reporting profile publication, practice collection version, content
  isolation confirmation, materials with sort order, status, published timestamp,
  validation issues.
- Forms: create/update definition; create version; compose materials/order where
  backend request supports.
- Tables/lists: definition list; version composition/validation sections.
- Actions: list, create definition, update definition, create version, validate,
  publish version, retire version.
- Lifecycle/state rules: validation precedes publish UX; publish/retire only where
  backend permits.
- Confirmations/destructive behavior: publish/retire require confirmation.
- States: loading, empty, populated, validation, validation-failed, saving,
  publishing, retiring, conflict/error, restricted.
- Privacy/non-exposure: no learner entitlements or purchase/order state.
- Backend/API authority: `/api/v1/admin/preparation-package/packages`.
- Responsive behavior: desktop composition table/sections; mobile grouped cards.
- RTL: logical layout and stable ordering numbers.
- Accessibility: validation issues announced/listed accessibly; confirmations focus
  safely.
- Explicit non-goals: catalog offer browsing, checkout, entitlement grant.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-120` still owns implementation.
- Remaining gaps: frontend route/backend path naming differs.

### ADM-PP-OFFERS — Package Offers

- Purpose: manage preparation package offers.
- Actor/audience: authenticated Admin with `PreparationPackageOffers.Manage`.
- Route/routability: `/admin/preparation-packages/offers`, route ID
  `ADMIN_PREPARATION_PACKAGE_OFFERS`.
- Permissions: Admin + `PreparationPackageOffers.Manage`.
- Data/fields: package definition/version, title, slug, summary, price amount minor,
  currency, access duration days, status.
- Forms: create/update offer.
- Tables/lists: offer list with package relation, title/slug, price, duration,
  status, actions.
- Actions: list, create, update, activate, deactivate.
- Lifecycle/state rules: activation/deactivation only as backend supports; do not
  infer purchase/order effects.
- Confirmations/destructive behavior: deactivate requires confirmation; activate is
  explicit and may surface backend conflicts.
- States: loading, empty, populated, validation, saving, activating, deactivating,
  conflict/error, restricted.
- Privacy/non-exposure: no buyer/order/payment transaction data.
- Backend/API authority: `/api/v1/admin/preparation-package/offers`.
- Responsive behavior: desktop table; mobile cards preserve title/price/status.
- RTL: canonical money/mixed-direction numeric rules.
- Accessibility: labelled money/currency/duration fields, accessible confirmations.
- Explicit non-goals: learner package catalog design, checkout, order management,
  entitlement management.
- DESIGN_CONTRACT readiness: `CONTRACT_READY`.
- Implementation readiness: not started; `T-FE-121` still owns implementation.
- Remaining gaps: frontend route/backend path naming differs.

### ADM-DASHBOARD — Dashboard Metrics

- Purpose: deferred future Admin metrics/dashboard concept.
- Actor/audience: Admin if reopened later.
- Route/routability: NOT_ROUTABLE; not an active v1 destination.
- Permissions: none approved beyond future backend/product contract.
- Data/fields/forms/tables/actions/lifecycle: none approved.
- States: backend-blocked only.
- Privacy/non-exposure: no inferred counts/metrics from CRUD endpoints.
- Backend/API authority: absent; `T-FE-103` verified no stable dashboard/metrics
  contract.
- Responsive/RTL/accessibility: not applicable until reopened.
- Explicit non-goals: metrics, KPIs, activity feeds, system health, payment/order,
  recruitment, user, exam, package aggregates.
- DESIGN_CONTRACT readiness: `BACKEND_BLOCKED` / `DEFERRED`.
- Implementation readiness: not ready.
- Remaining gaps: BG-ADM-01.

### ADM-004 — Roles / Permissions Management

- Purpose: deferred full roles/permissions management.
- Actor/audience: Admin if reopened later.
- Route/routability: NOT_ROUTABLE; not an active v1 destination.
- Permissions: role/permission constants exist but no management API exists.
- Data/fields/forms/tables/actions/lifecycle: none approved.
- States: backend-blocked only.
- Privacy/non-exposure: `/me`, user detail projections, permission constants, and
  seeded permissions do not authorize management UI.
- Backend/API authority: absent; `T-FE-105` verified no role/permission CRUD,
  listing, assign/revoke, or management DTO/command/query contract.
- Responsive/RTL/accessibility: not applicable until reopened.
- Explicit non-goals: full role CRUD, permission CRUD, assign/revoke permissions.
- DESIGN_CONTRACT readiness: `BACKEND_BLOCKED`.
- Implementation readiness: not ready.
- Remaining gaps: BG-ADM-02. `ADM-003` role update does not resolve this gap.

### ADM-009 — Admin Payment Orders

- Purpose: deferred Admin payment-order management.
- Actor/audience: Admin if reopened later.
- Route/routability: NOT_ROUTABLE; not an active v1 destination.
- Permissions/data/forms/tables/actions/lifecycle: none approved.
- States: backend-blocked only.
- Privacy/non-exposure: nurse-owned payment orders and Admin payment products do not
  authorize Admin order management.
- Backend/API authority: absent; `T-FE-112` verified no admin order-management
  contract.
- Responsive/RTL/accessibility: not applicable until reopened.
- Explicit non-goals: transaction management, refunds, provider controls,
  reconciliation, payment analytics.
- DESIGN_CONTRACT readiness: `BACKEND_BLOCKED`.
- Implementation readiness: not ready.
- Remaining gaps: BG-ADM-03.

### ADM-010 — Admin Recruitment

- Purpose: deferred Admin recruitment-management capability.
- Actor/audience: Admin if reopened later.
- Route/routability: NOT_ROUTABLE; not an active v1 destination.
- Permissions/data/forms/tables/actions/lifecycle: none approved.
- States: backend-blocked only.
- Privacy/non-exposure: employer candidate search/contact requests and nurse-owned
  contact-request decisions do not authorize Admin recruitment management.
- Backend/API authority: absent; `T-FE-112` verified no admin recruitment-management
  contract.
- Responsive/RTL/accessibility: not applicable until reopened.
- Explicit non-goals: candidate moderation, admin candidate detail, assignment,
  recruitment queue, employer/nurse request override.
- DESIGN_CONTRACT readiness: `BACKEND_BLOCKED`.
- Implementation readiness: not ready.
- Remaining gaps: BG-ADM-04.

## 8. Backend Gap Disposition

| Gap | Disposition |
|---|---|
| BG-ADM-01 | Still open. Dashboard/metrics backend authority remains missing. |
| BG-ADM-02 | Still open. Full role/permission management remains missing; `ADM-003` role update is not a management console. |
| BG-ADM-03 | Still open. Admin payment-order management remains missing. |
| BG-ADM-04 | Still open. Admin recruitment management remains missing. |

## 9. Relationships Explicitly Not Owned Here

- No Stitch generation, Stitch editing, or screen variants are authorized.
- No Angular route mounting, component implementation, Storybook story, backend,
  OpenAPI, generated-client, database, or package changes are authorized.
- `T-FE-084` remains deferred and is not affected by Admin payment-product approval.
- `ADM-DASHBOARD`, `ADM-004`, `ADM-009`, and `ADM-010` remain blocked until future
  backend/product authority exists.
- Downstream implementation tasks remain subject to their own gates, generated API
  checks, route mounting, tests, visual evidence, and explicit implementation
  authorization.

## 10. Approval Record

- 2026-09-21: Human technical lead accepted Admin authority extraction and approved
  HD-ADM-01..06.
- `GATE-FE-T098` is closed as `VERIFIED` on the strength of: explicit decisions for
  every Admin screen/concept, backend/API authority recorded for approved v1 Admin
  screens, unresolved backend gaps preserved without invention, and implementation
  readiness kept separate from design-contract readiness.
