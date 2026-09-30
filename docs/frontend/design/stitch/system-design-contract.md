# System-Wide Stitch Design Contract

```yaml
document_id: NPS-DES-STITCH-SYSTEM-DESIGN-CONTRACT
status: PHASE_2_SHELL_NAVIGATION_GENERATION_ACTIVE
created_at: 2026-09-20
scope: system_wide_visual_redesign_input_contract
stitch_write_authorization: phase_2_shell_navigation_only
implementation_authorization: false
```

## 1. Authority Model

This contract is the canonical input contract for future Google Stitch generation. It does not authorize Stitch writes, Angular implementation, backend changes, OpenAPI changes, generated-client changes, Storybook work, database changes, commits beyond the approved documentation commits, or screen generation.

Authority order:

1. Explicit current human decisions, including the 2026-09-20 decision that Google Stitch is the active visual-design workspace for the system-wide redesign.
2. Current approved product and design decisions.
3. Current implementation ledger and approved screen packets.
4. Current canonical routes.
5. Route-permission/access policy.
6. Backend/OpenAPI/generated DTO contracts.
7. Verified current frontend behavior only when it reveals implemented states; existing Angular layout/styling is implementation evidence, not visual authority.
8. Approved design foundation / All Pages rules.
9. UI/UX audit findings as improvement evidence.
10. Agent inference is never authority.

Visual authority for this redesign:

- Stitch is the active visual-design workspace.
- Stitch owns new visual composition/screens only after the future generated screens pass human approval.
- Existing Penpot artifacts are legacy/reference evidence only unless a specific element is explicitly re-approved.
- Existing Angular UI is implementation evidence and state evidence only, not visual authority.
- Storybook remains implementation/component verification, not independent product authority.
- Backend/API/security/privacy/accessibility contracts remain authoritative and cannot be overridden by visual examples.

## 2. Stitch Capability Discovery And Usage Plan

Observed Stitch MCP capability surface in the current OpenCode session exposes 15 tools:

| Capability | Exposed tool | Later-phase use | Phase 1 use |
|---|---|---|---|
| List projects | `stitch_list_projects` | Verify project existence/access before generation. | Read-only discovery only. |
| Get project | `stitch_get_project` | Inspect generated project metadata and screen instances. | Not required because no projects exist. |
| Create project | `stitch_create_project` | Phase 2 creates one canonical Nursing Platform Stitch project. | Forbidden in Phase 1. |
| List screens | `stitch_list_screens` | Audit generated screen coverage and naming. | Not useful without project. |
| Get screen | `stitch_get_screen` | Inspect individual generated screen metadata. | Not useful without project. |
| Generate screen from text | `stitch_generate_screen_from_text` | Generate contract-scoped screens family-by-family. | Forbidden in Phase 1. |
| Edit screens | `stitch_edit_screens` | Apply contract-comparison corrections after review. | Forbidden in Phase 1. |
| Generate variants | `stitch_generate_variants` | Create responsive, RTL, role, and major-state variants only when warranted. | Forbidden in Phase 1. |
| List design systems | `stitch_list_design_systems` | Audit available systems; verify one canonical system is active. | Read-only allowed, not required. |
| Create design system | `stitch_create_design_system` | Create the shared design system after DESIGN.md is prepared. | Forbidden in Phase 1. |
| Upload DESIGN.md | `stitch_upload_design_md` | Upload canonical design-system guidance. | Forbidden in Phase 1. |
| Create design system from DESIGN.md | `stitch_create_design_system_from_design_md` | Create system from uploaded DESIGN.md when Phase 2 begins. | Forbidden in Phase 1. |
| Apply design system | `stitch_apply_design_system` | Apply the approved system to generated screens. | Forbidden in Phase 1. |
| Update design system | `stitch_update_design_system` | Revise system after human-approved token changes. | Forbidden in Phase 1. |
| Delete project | `stitch_delete_project` | Cleanup only after explicit destructive approval. | Forbidden in Phase 1. |

No explicit prototype, link-screen, interaction-flow, or click-through wiring MCP capability is exposed. The screen flow graph in this document remains repository authority for relationships even if Stitch cannot encode links directly.

Phase 2 strategy:

1. Create one Stitch project.
2. Upload/create one canonical DESIGN.md from this contract's Design System section.
3. Create one shared Stitch design system.
4. Generate design system + app shell + global navigation first.
5. Generate screens family-by-family.
6. Generate variants only under the rules in Section 24.
7. Compare generated screens back to this contract before edits.
8. Use `stitch_edit_screens` only for contract-comparison corrections.

## 2A. Design-Led Feature Discovery Protocol

Stitch may help discover useful product ideas during visual exploration, but design-discovered functionality must not silently become production functionality. Core Nursing Platform workflows remain the primary authority and visual focus.

Future Stitch prompts may allow reasonable product-design exploration. After each generated screen, the agent must classify every user-visible or accessibility-exposed functional element into one of these categories:

| Category | Definition | Governance effect |
|---|---|---|
| `AUTHORITATIVE_FEATURE` | Already supported by current product contracts, documentation, API, or implementation. | May be represented normally in designs. |
| `DESIGN_PROPOSED_FEATURE` | Useful product functionality discovered or introduced during design. | May remain visible in the design, but is not implemented, backend/API authority, frontend implementation authority, or release approval. Must be recorded in `docs/frontend/design/stitch/design-proposed-features.md` before the screen is considered fully reviewed. |
| `VISUAL_ONLY_ELEMENT` | Decorative or compositional element with no product functionality implied. | Does not require implementation planning unless it later gains behavior. |
| `UNSUPPORTED_CLAIM` | Security, compliance, runtime, business, availability, state, or status claim that cannot be asserted from authoritative system evidence. | Must not be presented as factual user-facing truth unless separately verified and authorized. Do not convert unsupported factual claims into design-proposed features. |

Examples of `UNSUPPORTED_CLAIM` include `HIPAA compliant`, `GDPR compliant`, `WCAG compliant`, `System Online`, `Token Verified`, security guarantees, service-health guarantees, and similar factual assurances.

Design-proposed features are secondary. They must not redesign the application around speculative functionality, replace authoritative workflows, obscure core actions/data/navigation, distort screen purpose, or become dependencies for core workflows.

Required per-screen manifest fields:

| Field | Requirement |
|---|---|
| Screen ID / canonical name | Exact repository-approved screen identity and generated screen name. |
| Actor / audience | Exact actor or audience; no additional audience inference. |
| Route | Exact route if screen is routed; `not applicable` only when no route exists. |
| Purpose | One bounded user/product purpose. |
| REQUIRED | User-visible elements that must appear. |
| ALLOWED | User-visible elements that may appear. |
| FORBIDDEN | User-visible elements, data, actions, claims, and behaviors that must not appear. |
| Navigation | Exact global/contextual destinations allowed. |
| User-visible data | Exact data fields/copy allowed to appear. |
| Actions | Exact actions allowed. |
| States | Exact states allowed. |
| Responsive requirements | Exact viewport/responsive requirements for the requested artifact. |
| RTL requirements | Exact RTL requirements or `not in scope for this artifact`. |
| Accessibility requirements | Exact accessibility requirements inherited from this contract and any screen-specific requirements. |
| Explicit non-goals | Explicit exclusions and adjacent screens/behaviors that must not be designed. |

Manifest semantics:

- `REQUIRED` = must appear.
- `ALLOWED` = may appear as an authoritative or visual element.
- `FORBIDDEN` = must not appear.
- `UNLISTED` = classify after generation as `AUTHORITATIVE_FEATURE`, `DESIGN_PROPOSED_FEATURE`, `VISUAL_ONLY_ELEMENT`, or `UNSUPPORTED_CLAIM`; do not automatically treat every unlisted design element as forbidden.

Before calling Stitch for any screen, the agent must construct the exact manifest from repository authority. The Stitch prompt must be derived from that manifest and must not replace the manifest with a vague creative prompt.

When a prompt is intended to be strict rather than exploratory, it must say so explicitly. Otherwise future Stitch prompts may allow bounded design exploration while requiring post-generation classification.

Strict prompts may include this sentence before the manifest:

> Closed-world rule: use only the user-visible elements and behaviors explicitly authorized below. Do not fill perceived gaps. Do not invent realistic sample content. Anything not listed is forbidden.

Do not paste the full `DESIGN.md` into screen-generation prompts. The active design system owns visual language and design rules.

## 2B. Post-Generation Classification And Validation Gate

Generation success is not acceptance. After Stitch generates a screen, the agent must inspect the resulting screen metadata and available rendered/browser content artifacts before requesting human visual review. Raw source may be inspected to detect exposed product content, but the validator must distinguish rendered product UI from implementation source.

After each generated screen, the agent must identify:

1. authoritative features;
2. newly proposed functional features;
3. visual-only additions;
4. unsupported factual claims.

Any new `DESIGN_PROPOSED_FEATURE` must be added to `docs/frontend/design/stitch/design-proposed-features.md` before the screen is considered fully reviewed. Do not implement the proposed feature merely because Stitch generated it.

Validation content categories:

| Category | Definition | Contract effect |
|---|---|---|
| `USER_VISIBLE_PRODUCT_CONTENT` | Rendered visible text; visible icons with product meaning; buttons; links; navigation destinations; form controls; visible status indicators; visible user data; visible footer/header content; tooltips or labels exposed to the user; interactive behaviors available to the user. | Must obey `REQUIRED`, `ALLOWED`, `FORBIDDEN`, and `UNLISTED` rules. |
| `ACCESSIBILITY_EXPOSED_CONTENT` | Accessible names, `aria-label`s, alt text, accessible descriptions, and semantic interactive roles exposed to assistive technology. | Must obey the product contract and must not expose invented product functionality or misleading content. |
| `IMPLEMENTATION_INTERNAL` | HTML comments, Tailwind configuration, CSS implementation, JavaScript scaffolding, framework boilerplate, Material icon ligature implementation, generated class names, script/style internals, and non-rendered developer scaffolding. | Does not constitute a closed-world contract violation unless it becomes user-visible, interactive, security-sensitive, or changes product behavior. |
| `PREVIEW_DOCUMENT_METADATA` | Generated document title, preview metadata, and Stitch-specific artifact metadata. | Report separately when useful, but do not fail the user-visible screen contract solely because of preview/tool metadata unless it leaks prohibited user-visible product information. |

Validation requirements:

1. Every `REQUIRED` element exists.
2. No `FORBIDDEN` element exists.
3. Every unlisted user-visible or accessibility-exposed product element is classified as `AUTHORITATIVE_FEATURE`, `DESIGN_PROPOSED_FEATURE`, `VISUAL_ONLY_ELEMENT`, or `UNSUPPORTED_CLAIM`.
4. No fake PII or invented user identity appears.
5. No unsupported security, compliance, availability, authentication, online/offline, or service-health claim appears.
6. No internal design, route, CSS, token, viewport, debug, implementation, or developer annotation is rendered or exposed to the user.
7. No invented navigation destination appears as implemented/authoritative unless it is already supported by repository authority; useful new destinations must be tracked as `DESIGN_PROPOSED_FEATURE`.
8. Raw implementation scaffolding alone must not cause `CONTRACT_VIOLATION` when it remains implementation-internal and does not alter product behavior.
9. Before first production release, every open design-proposed feature must receive one final disposition: `IMPLEMENT_BEFORE_RELEASE`, `HIDE_BEFORE_RELEASE`, `DEFER_POST_RELEASE`, or `REJECT`.

Allowed validation statuses:

| Status | Meaning |
|---|---|
| `CONTRACT_VALID` | Required content appears, forbidden content is absent, useful unimplemented functional additions are tracked as design-proposed features, and unsupported factual claims are excluded or explicitly marked not accepted. |
| `CONTRACT_VIOLATION` | Any required item is missing, forbidden item appears, fake identity/debug content is accepted as product content, an unsupported factual claim remains presented as truth, or an unimplemented proposed feature is treated as implemented authority. |
| `READY_FOR_HUMAN_VISUAL_REVIEW` | The artifact is `CONTRACT_VALID` and ready for human visual review. |
| `HUMAN_APPROVED` | A human explicitly approves the artifact after visual review. |

`CONTRACT_VALID` is required before `READY_FOR_HUMAN_VISUAL_REVIEW`. Stitch tool success does not imply `CONTRACT_VALID`. Human visual approval is always separate. If validation fails, stop downstream generation; do not edit, regenerate, or generate variants unless explicitly authorized.

## 2C. Production Release Gate For Design-Proposed Features

Before the first production release, every open item in `docs/frontend/design/stitch/design-proposed-features.md` must be reviewed. No `DESIGN_PROPOSED` feature may accidentally ship as a partially functional or misleading control.

Each item must receive one final disposition:

- `IMPLEMENT_BEFORE_RELEASE`
- `HIDE_BEFORE_RELEASE`
- `DEFER_POST_RELEASE`
- `REJECT`

If a feature remains visible in production, it must have the necessary product, frontend, backend/API, permission/security, accessibility, and testing authority appropriate to that feature.

## 3. Application Information Architecture

| Family | Purpose | Primary actors | Entry routes | Primary screens | Major flows | Relationships | Status | Stitch generation |
|---|---|---|---|---|---|---|---|---|
| Authentication | Public identity access, registration, verification, recovery, terminal auth states. | Anonymous, authenticated return users. | `/auth/sign-in`, `/auth/sign-up`, `/auth/verify-email`, `/auth/forgot-password`, `/auth/reset-password` | Sign in, sign up, check email, verify email, forgot/reset password, session expired, access denied. | Sign in -> account/root; sign up -> check email; verify link; forgot -> reset link; reset -> success state. | Feeds Account and onboarding; produces token session. | Implemented for many screens; visual authority reset for Stitch. | READY_FOR_STITCH except account inactive is BACKEND_BLOCKED. |
| Account | Actor-neutral current-user identity and personal-name edit. | Authenticated users. | `/account`, `/onboarding/profile` | Account overview, personal details same-route edit, onboarding profile. | View identity -> edit first/last names -> save/cancel. | Auth fallback, profile completion, navigation account affordance. | ACC-001/002 approved; other account screens blocked. | READY_FOR_STITCH for ACC-001/002; backend-blocked for ACC-003..006. |
| Nurse profile | Nurse professional profile and recruitment-facing self-management. | Nurse. | `/nurse`, `/nurse/profile`, profile section routes. | Overview, personal info, experience, education, certificates, skills, languages, CV, contact requests. | Create base profile; manage CRUD sections; upload/delete CV; approve/reject contact requests. | Employer candidate discovery; preparation packages; account identity. | Approved packet; most implemented; visual redesign treats Angular as evidence only. | READY_FOR_STITCH except NUR-013 DEFERRED. |
| Exams | Browse/start/take/review/analyze exams. | Authenticated learner, usually Nurse but route policy is authenticated-only. | `/exams`, `/exams/history`, `/exams/analytics` | Catalog, detail, instructions, session, result, review, analytics, history. | Catalog -> detail -> instructions -> session -> submit -> result -> review; catalog -> history/analytics. | Preparation package exams reuse shared session/result/review; commerce may gate paid access. | Approved and implemented across current chain. | READY_FOR_STITCH. |
| Preparation Packages | Public package offer discovery; nurse-owned entitlements, practice, package exam, report. | Anonymous/public offer browsers; Nurse learners. | `/preparation-packages`, `/nurse/preparation-packages` | Offers, offer detail, entitlements, entitlement detail, practice, package report. | Browse offer; view owned entitlement; practice; start package exam; view package report. | Commerce purchase creates entitlements; exams use shared exam routes. | Many implemented; material reader backend-blocked. | READY_FOR_STITCH for evidenced screens; material reader BACKEND_BLOCKED. |
| Commerce | Product discovery, order creation, payment outcomes, order history/detail. | Authenticated learners; checkout routes Nurse role for later order screens. | `/commerce/products`, `/checkout`, `/commerce/orders` | Products, product detail, checkout, success/failure, orders, order detail. | Product -> detail -> checkout -> order -> payment outcome -> order detail/history. | Preparation package offers/products; payments; T-FE-084 intentionally deferred. | COM-001/002 verified; COM-003 remains deferred. | READY_FOR_STITCH for catalog/detail; COM-003 DEFERRED; COM-004 DEFERRED; COM-005/006 CONTRACT_INCOMPLETE; COM-007/008 READY_FOR_STITCH. |
| Employer | Employer landing/profile, candidate discovery, contact requests. | Employer. | `/employer`, `/employer/candidates`, `/employer/requests` | Home, candidate search/results, request list/detail. | Browse candidates -> request contact; manage recruitment requests. | Nurse recruitment availability and contact requests. | Routes approved; candidate detail/request backend-blocked. | CONTRACT_INCOMPLETE for current approved routes; candidate detail/request BACKEND_BLOCKED. |
| Administration | Admin management for users, exams, reference data, payments, preparation packages. | Admin plus exact permissions. | `/admin`, `/admin/users`, `/admin/exams`, `/admin/preparation-packages/*` | User list/detail; reference data; exams; exam detail/versions/questions; payment products; PP admin lists. | Admin list/detail workflows; manage definitions/materials/offers. | Backend admin APIs; route permissions. | Some user screens implemented; many future routes approved but not implemented. | CONTRACT_INCOMPLETE where backend/screen packet lacks detail; blocked dashboard/roles/payment orders/recruitment. |
| Shared/System | Shell, navigation, loading/errors, empty/no-results, restricted, not found, offline, maintenance. | All actors. | `/`, `/session-expired`, `/access-denied`, wildcard future. | App shell, global navigation, route loading, access denied, not found, unexpected error, empty/no-results. | Route transitions; restricted/404/error; offline/maintenance future. | Every family consumes these patterns. | System packet approved; navigation gap newly first-class. | App shell/navigation HUMAN_DECISION_REQUIRED; shared states READY_FOR_STITCH except offline/maintenance DEFERRED. |

## 4. Global Navigation Contract

Navigation is a first-class design requirement because the current shell has route logic but no practical visual navigation. Do not duplicate authorization logic; frontend navigation eligibility must reuse `navigation-permission-policy.ts` over explicit route IDs and `route-permission-policy.ts`. Backend authorization remains authoritative.

Required model:

- Actor-aware navigation uses authenticated user roles and permissions from `GET /api/v1/me` / `CurrentUserStore.ready`.
- `anonymous` shell exposes public/auth destinations only: sign in, sign up, public preparation package offers, and safe terminal/system states when routed.
- `authenticated` shell exposes account/profile affordance and a sign-out affordance.
- Approved app shell architecture is hybrid: persistent top application bar on desktop, compact top bar plus accessible menu/drawer on mobile. Do not use a permanent global sidebar for every user; dense families such as Administration may use contextual secondary side navigation when justified.
- Navigation is organized by user goals/product families, not raw route tree.
- Nurse primary families: Exams, Preparation Packages, Commerce when applicable destinations are available, and Profile. Account belongs to the user/account affordance.
- Employer primary families: Employer home, candidate search, recruitment requests.
- Admin primary families: Admin entry, users, exams/reference/payment/PP admin areas according to exact permission policies.
- Secondary/contextual destinations include detail pages, form/edit substates, result/review/report states, checkout outcome states, and direct resource routes.
- Active route behavior must reflect exact route identity/ancestor family without path-prefix authorization decisions.
- Sign-out placement is required in authenticated shell; exact placement is HUMAN_DECISION_REQUIRED.
- Profile/account access is required in authenticated shell; exact grouping/order is HUMAN_DECISION_REQUIRED.
- Mobile access requires a reachable navigation mechanism with route-change close behavior, focus entry/return, background interaction blocked while modal navigation is open, and no hidden keyboard traps.
- RTL must use logical layout, mirrored directional affordances only where appropriate, stable numbers/dates/currency, and unchanged brand/status icons.
- Skip link must jump to main content.
- Route change closes transient mobile navigation and restores focus to a stable post-navigation target.

Resolved Phase 2 navigation decisions:

- HD-STITCH-01 resolves hybrid shell architecture.
- HD-STITCH-02 resolves primary family grouping for Nurse shell and excludes contextual/detail/transient routes from global primary navigation.
- HD-STITCH-03 resolves icon role: supporting icons with visible text labels; icon-only primary navigation is not approved.
- HD-STITCH-04 resolves root behavior boundary: state/actor-driven entry, no fake universal dashboard.

Remaining later decisions: exact final product copy for every future actor menu item, optional breadcrumb policy beyond shell placeholder, and future implementation task/gate numbering.

## 5. App Shell Contract

Required shell elements:

- Skip link to `main`.
- Brand/home affordance that does not invent root redirect semantics.
- Anonymous navigation for public/auth destinations.
- Authenticated actor-aware navigation filtered by route policy.
- User/account affordance and sign-out affordance.
- Optional breadcrumbs/context only after human approval; do not infer from URL segments.
- Main content landmark with single page `h1` owned by screen.
- Route-level loading composed from verified `SYS-001` pattern.
- Mobile navigation mechanism with explicit open/close control, keyboard trap only if modal drawer architecture is chosen, escape/route-change close behavior, and focus return.
- RTL mirroring through logical properties.
- Active route state with text/shape/aria-current, not color alone.

Viable architectures for Phase 2 review:

| Option | Description | Tradeoff | Status |
|---|---|---|---|
| Top application bar + responsive drawer | Header contains brand, primary navigation, account/sign-out; mobile collapses to accessible drawer/menu. | Approved for Phase 2 Shell / App Navigation. | APPROVED_BY_HD-STITCH-01 |
| Contextual secondary navigation | Optional inside dense families such as Administration, not global for every user. | Keeps learner shell light while supporting admin density. | APPROVED_WHEN_JUSTIFIED |
| Permanent global sidebar | Sidebar present for every actor and page. | Consumes horizontal space and makes every page feel like admin dashboard. | NOT_APPROVED |

Architecture is locked for Phase 2 shell generation by HD-STITCH-01; later Angular implementation still requires a separate task/gate.

## 6. Role Screen Matrix

All canonical paths from `canonical-routes.ts` are represented. Public/entry routes receive no role policy; authenticated routes use `route-permission-policy.ts`.

| Screen ID | Route ID | Path/template | Family | Audience | Auth | Role | Permission | Ownership/resource | Entry points | Exit/next | Direct URL | Impl status | Design authority | Stitch readiness |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| ROOT | ROOT_ENTRY | `/` | Shared | All | Entry | — | — | State/actor-driven root; no fake dashboard | Browser/root | Public entry or actor/product entry according to route/product rules | YES | Shell exists | HD-STITCH-04 | READY_FOR_STITCH |
| AUTH-001 | AUTH_SIGN_IN | `/auth/sign-in` | Auth | Anonymous/authenticated | Public | — | — | Token/session | Direct, auth redirects | Account/returnUrl/root | YES | Implemented | Legacy approved, redesign reset | READY_FOR_STITCH |
| AUTH-002 | AUTH_SIGN_UP | `/auth/sign-up` | Auth | Anonymous | Public | — | — | Registration contract | Sign in/public | Check email | YES | Implemented | V1 contract | READY_FOR_STITCH |
| AUTH-LEGACY-ROLE | AUTH_ROLE_SELECTION | `/auth/role-selection` | Auth | Legacy | Public | — | — | Redirect only | Old links | Sign up | YES | Redirect | No screen | NOT_APPLICABLE |
| AUTH-LEGACY-NURSE | AUTH_REGISTER_NURSE | `/auth/register/nurse` | Auth | Legacy | Public | — | — | Redirect only | Old links | Sign up | YES | Redirect | No screen | NOT_APPLICABLE |
| AUTH-LEGACY-EMPLOYER | AUTH_REGISTER_EMPLOYER | `/auth/register/employer` | Auth | Legacy | Public | — | — | Redirect only | Old links | Sign up | YES | Redirect | No screen | NOT_APPLICABLE |
| AUTH-005 | AUTH_VERIFY_EMAIL_REQUEST | `/auth/verify-email` | Auth | New registrants | Public | — | — | Email verification handoff | Sign up | Sign in/email client | YES | Implemented | V1 contract | READY_FOR_STITCH |
| AUTH-006 | AUTH_VERIFY_EMAIL_CONFIRM | `/auth/verify-email/confirm` | Auth | Email-link users | Public | — | — | Opaque query token | Email link | Sign in | YES | Implemented | V1 contract | READY_FOR_STITCH |
| AUTH-007 | AUTH_FORGOT_PASSWORD | `/auth/forgot-password` | Auth | Users needing recovery | Public | — | — | Email submission | Sign in | Check email/out-of-band | YES | Implemented | V1 contract | READY_FOR_STITCH |
| AUTH-008 | AUTH_RESET_PASSWORD | `/auth/reset-password` | Auth | Reset-link users | Public | — | — | Email + opaque token | Email link | Success state/sign in | YES | Implemented | V1 contract | READY_FOR_STITCH |
| AUTH-010 | SYSTEM_SESSION_EXPIRED | `/session-expired` | System | All | Public | — | — | Session terminal state | Future session transition | Sign in/account safe action TBD | YES | Implemented | V1 contract | READY_FOR_STITCH |
| AUTH-011 | SYSTEM_ACCESS_DENIED | `/access-denied` | System | Authenticated denied users | Public | — | — | Route permission failure | Guards | Account | YES | Implemented | V1 contract | READY_FOR_STITCH |
| ONB-001 | ONBOARDING_PROFILE | `/onboarding/profile` | Account | Authenticated | Authenticated | — | — | Current-user profile | Sign-up/auth bootstrap | Account/root | YES | Implemented | V1 contract | READY_FOR_STITCH |
| ACC-001/002 | ACCOUNT_OVERVIEW | `/account` | Account | Authenticated | Authenticated | — | — | Current user only | Shell/account | Edit state, sign out, actor areas | YES | Placeholder/partial | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-ENTRY | NURSE_ENTRY | `/nurse` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse | Shell/root | Nurse profile | YES | Redirect | Route contract | READY_FOR_STITCH |
| NUR-001/002 | NURSE_PROFILE_OVERVIEW | `/nurse/profile` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Nurse entry/shell | Profile sections | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-003 | NURSE_PROFILE_PERSONAL_INFORMATION | `/nurse/profile/personal-information` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-004/005 | NURSE_PROFILE_EXPERIENCE | `/nurse/profile/experience` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-006/007 | NURSE_PROFILE_EDUCATION | `/nurse/profile/education` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-008/009 | NURSE_PROFILE_CERTIFICATES | `/nurse/profile/certificates` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-010 | NURSE_PROFILE_SKILLS | `/nurse/profile/skills` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-011 | NURSE_PROFILE_LANGUAGES | `/nurse/profile/languages` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| NUR-012 | NURSE_PROFILE_CV | `/nurse/profile/cv` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse profile | Overview | Overview | YES | Route exists, component path may be absent in current file tree | HUMAN_APPROVED | CONTRACT_INCOMPLETE |
| NUR-CONTACT | NURSE_CONTACT_REQUESTS | `/nurse/contact-requests` | Nurse | Nurse | Authenticated | Nurse | — | Current nurse requests | Direct/future nav | Stay/list refresh | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EMP-001 | EMPLOYER_HOME | `/employer` | Employer | Employer | Authenticated | Employer | — | Employer profile/home | Future shell | Candidates/requests | YES | Future/not mounted | Approved route only | CONTRACT_INCOMPLETE |
| EMP-002/003/004 | EMPLOYER_CANDIDATES | `/employer/candidates` | Employer | Employer | Authenticated | Employer | — | Candidate visibility backend | Employer home | Candidate request/detail blocked | YES | Future/not mounted | Route contract | CONTRACT_INCOMPLETE |
| EMP-007 | EMPLOYER_REQUESTS | `/employer/requests` | Employer | Employer | Authenticated | Employer | — | Employer-owned requests | Employer home | Request detail | YES | Future/not mounted | Route contract | CONTRACT_INCOMPLETE |
| EMP-008 | EMPLOYER_REQUEST_DETAIL | `/employer/requests/:requestId` | Employer | Employer | Authenticated | Employer | — | Request ownership | Request list | Request list | YES | Future/not mounted | Route contract | CONTRACT_INCOMPLETE |
| EXM-001 | EXAMS_CATALOG | `/exams` | Exams | Authenticated learner | Authenticated | — | — | Backend exam eligibility | Shell/direct | Detail/history/analytics | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-002 | EXAMS_DETAIL | `/exams/:examId` | Exams | Authenticated learner | Authenticated | — | — | Startable/detail backend | Catalog | Instructions/catalog | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-004 | EXAMS_INSTRUCTIONS | `/exams/:examId/instructions` | Exams | Authenticated learner | Authenticated | — | — | Exam start backend | Detail | Session/detail | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-005/006 | EXAMS_SESSION | `/exams/:examId/sessions/:sessionId` | Exams | Authenticated learner | Authenticated | — | — | Session ownership/backend | Instructions/history/package start | Result/transient | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-007 | EXAMS_RESULT | `/exams/:examId/sessions/:sessionId/result` | Exams | Authenticated learner | Authenticated | — | — | Session result ownership | Submit/history | Review/catalog | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-008 | EXAMS_ANALYTICS | `/exams/analytics` | Exams | Authenticated learner | Authenticated | — | — | Analytics backend membership | Catalog | Catalog | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-009 | EXAMS_REVIEW | `/exams/:examId/sessions/:sessionId/review` | Exams | Authenticated learner | Authenticated | — | — | Finalized review ownership | Result/history | Result | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| EXM-010 | EXAMS_HISTORY | `/exams/history` | Exams | Authenticated learner | Authenticated | — | — | Attempt ownership | Catalog | Session/result/review/catalog | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| PP-001 | PREPARATION_PACKAGES_OFFERS | `/preparation-packages` | Prep packages | Public | Public | — | — | Offer visibility | Public/shell | Offer detail | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| PP-002 | PREPARATION_PACKAGES_OFFER_DETAIL | `/preparation-packages/:offerSlug` | Prep packages | Public | Public | — | — | Offer visibility | Offers | Offers/commerce later | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| PP-003 | PREPARATION_PACKAGES_ENTITLEMENTS | `/nurse/preparation-packages` | Prep packages | Nurse | Authenticated | Nurse | — | Current nurse entitlements | Shell/direct | Entitlement detail | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| PP-004 | PREPARATION_PACKAGES_ENTITLEMENT_DETAIL | `/nurse/preparation-packages/:entitlementId` | Prep packages | Nurse | Authenticated | Nurse | — | Entitlement ownership | Entitlements | Practice/session/report | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| PP-005 | PREPARATION_PACKAGES_PRACTICE | `/nurse/preparation-packages/:entitlementId/practice` | Prep packages | Nurse | Authenticated | Nurse | — | Entitlement practice access | Entitlement detail | Entitlement detail | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| PP-007 | PREPARATION_PACKAGES_REPORT | `/nurse/preparation-packages/reports/:sessionId` | Prep packages | Nurse | Authenticated | Nurse | — | Package session/report ownership | Entitlement detail | Entitlements | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| COM-001 | COMMERCE_PRODUCTS | `/commerce/products` | Commerce | Authenticated learner | Authenticated | — | — | Product availability | Shell/direct | Product detail | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| COM-002 | COMMERCE_PRODUCT_DETAIL | `/commerce/products/:productId` | Commerce | Authenticated learner | Authenticated | — | — | Product availability | Products | Products/checkout future | YES | Implemented | HUMAN_APPROVED | READY_FOR_STITCH |
| COM-003 | COMMERCE_CHECKOUT | `/checkout` | Commerce | Nurse | Authenticated | Nurse | — | Order create eligibility | Product detail | Success/failure/order | YES | Deferred | HUMAN_APPROVED but deferred | DEFERRED |
| COM-005 | COMMERCE_PAYMENT_SUCCESS | `/checkout/orders/:orderId/success` | Commerce | Nurse | Authenticated | Nurse | — | Order ownership/payment truth | Provider/backend return | Order detail | YES | Future | HUMAN_APPROVED | CONTRACT_INCOMPLETE |
| COM-006 | COMMERCE_PAYMENT_FAILURE | `/checkout/orders/:orderId/failure` | Commerce | Nurse | Authenticated | Nurse | — | Order ownership/payment truth | Provider/backend return | Order detail | YES | Future | HUMAN_APPROVED | CONTRACT_INCOMPLETE |
| COM-007 | COMMERCE_ORDERS | `/commerce/orders` | Commerce | Nurse | Authenticated | Nurse | — | Owned orders | Shell/account/future | Order detail/products | YES | Future | HUMAN_APPROVED | CONTRACT_INCOMPLETE |
| COM-008 | COMMERCE_ORDER_DETAIL | `/commerce/orders/:orderId` | Commerce | Nurse | Authenticated | Nurse | — | Order ownership | Orders/outcomes | Orders | YES | Future | HUMAN_APPROVED | CONTRACT_INCOMPLETE |
| ADM-ENTRY | ADMIN_ENTRY | `/admin` | Admin | Admin | Authenticated | Admin | — | Admin entry | Shell | Admin family workspace without fake dashboard metrics | YES | Future | HD-STITCH-06 for shell/dense-data treatment | READY_FOR_PHASE_2_REPRESENTATIVE_SHELL |
| ADM-002 | ADMIN_USERS | `/admin/users` | Admin | Admin | Authenticated | Admin | Users.View | Backend users | Admin entry | User detail | YES | Implemented | Route/API | READY_FOR_STITCH |
| ADM-003 | ADMIN_USER_DETAIL | `/admin/users/:userId` | Admin | Admin | Authenticated | Admin | Users.View | User detail | Users | Users | YES | Implemented | Route/API | READY_FOR_STITCH |
| ADM-005 | ADMIN_REFERENCE_DATA | `/admin/reference-data` | Admin | Admin | Authenticated | Admin | Exams.View | Reference data | Admin entry | Admin entry | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-006 | ADMIN_EXAMS | `/admin/exams` | Admin | Admin | Authenticated | Admin | Exams.View | Exams backend | Admin entry | Exam detail | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-EXAM-DETAIL | ADMIN_EXAM_DETAIL | `/admin/exams/:examId` | Admin | Admin | Authenticated | Admin | Exams.View | Exam backend | Admin exams | Versions/questions | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-EXAM-VERSIONS | ADMIN_EXAM_VERSIONS | `/admin/exams/:examId/versions` | Admin | Admin | Authenticated | Admin | Exams.View | Exam versions | Exam detail | Exam detail | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-007 | ADMIN_EXAM_QUESTIONS | `/admin/exams/:examId/questions` | Admin | Admin | Authenticated | Admin | Questions.View | Exam questions | Exam detail | Exam detail | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-008 | ADMIN_PAYMENT_PRODUCTS | `/admin/payment-products` | Admin | Admin | Authenticated | Admin | Exams.View | Payment products admin | Admin entry | Product admin detail TBD | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-PP-TOPICS | ADMIN_PREPARATION_PACKAGE_TOPICS | `/admin/preparation-packages/topics` | Admin | Admin | Authenticated | Admin | ReportingTopics.Manage | PP topics | Admin entry | Same list | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-PP-PROFILES | ADMIN_PREPARATION_PACKAGE_PROFILES | `/admin/preparation-packages/profiles` | Admin | Admin | Authenticated | Admin | ReportingProfiles.Manage | PP profiles | Admin entry | Same list | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-PP-MATERIALS | ADMIN_PREPARATION_PACKAGE_MATERIALS | `/admin/preparation-packages/materials` | Admin | Admin | Authenticated | Admin | StudyMaterials.Manage | PP materials | Admin entry | Same list | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-PP-PRACTICE | ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS | `/admin/preparation-packages/practice-collections` | Admin | Admin | Authenticated | Admin | PracticeCollections.Manage | PP practice collections | Admin entry | Same list | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-PP-DEFINITIONS | ADMIN_PREPARATION_PACKAGE_DEFINITIONS | `/admin/preparation-packages/definitions` | Admin | Admin | Authenticated | Admin | PreparationPackages.View | PP definitions | Admin entry | Same list | YES | Future | Route only | CONTRACT_INCOMPLETE |
| ADM-PP-OFFERS | ADMIN_PREPARATION_PACKAGE_OFFERS | `/admin/preparation-packages/offers` | Admin | Admin | Authenticated | Admin | PreparationPackageOffers.Manage | PP offers admin | Admin entry | Same list | YES | Future | Route only | CONTRACT_INCOMPLETE |

## 7. Complete Screen Inventory And Per-Screen Contracts

The matrix above is the role/security index. This section adds the screen design contract detail required for Stitch generation. All screens inherit the global responsive, RTL, accessibility, security, and design-system rules in Sections 12, 19, and 20 unless a row narrows them.

Legend for page type: `list`, `detail`, `form`, `report`, `workflow`, `system state`, `confirmation`, `transient state`.

| screen_id | screen_name | Family | Route | Page type | User goal/business purpose | Data and visible fields | Layout hierarchy and actions | States | Navigation | Security/non-exposure | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|
| AUTH-001 | Sign in | Auth | `/auth/sign-in` | form | Authenticate with email/username and password. | Credentials only; token response never displayed. | Auth shell, h1 Sign in, credential fields, submit, recovery/sign-up links. | loading, validation, backend error, success navigation. | returnUrl safe return; forgot/sign-up links. | Never show token, raw auth errors, password. | READY_FOR_STITCH |
| AUTH-002 | Sign up | Auth | `/auth/sign-up` | form | Create public V1 account. | email, username, password per registration contract. | Auth shell, h1 Sign up, fields, submit, sign-in link. | validation, submitting, accepted -> check email, backend error. | Check email, sign in. | Never expose role assignment internals. | READY_FOR_STITCH |
| AUTH-005 | Check email | Auth | `/auth/verify-email` | confirmation | Tell accepted registrant to verify email. | registration email only if safely available; otherwise generic copy. | h1 Check your email, guidance, sign-in link. | ready only. | Sign in. | No resend unless contract-authorized. | READY_FOR_STITCH |
| AUTH-006 | Verify email | Auth | `/auth/verify-email/confirm?token=` | system state | Confirm opaque token. | token internal only; success text from backend authority. | h1 Verify email, status region. | missing token, verifying, success, failure. | Sign in. | Never display/store/decode token. | READY_FOR_STITCH |
| AUTH-007 | Forgot password | Auth | `/auth/forgot-password` | form | Request reset email. | email. | h1 Forgot password, email field, submit, sign-in link. | validation, submitting, accepted status, backend error. | Sign in/reset out-of-band. | Do not reveal account existence beyond backend copy. | READY_FOR_STITCH |
| AUTH-008/AUTH-009 | Reset password and success | Auth | `/auth/reset-password?token=` | form + transient state | Reset password from opaque link and show success state. | email, token internal, newPassword. | h1 Reset password, fields, submit; inline success status. | missing token, validation, submitting, success, error. | Forgot password, sign in. | Never show token or password. | READY_FOR_STITCH |
| AUTH-010 | Session expired | System | `/session-expired` | system state | Terminal session state when explicitly routed. | no backend data. | h1 Session expired, explanatory copy, safe action. | ready only. | Sign in or account safe action per future shell. | Do not infer expiry from anonymous alone. | READY_FOR_STITCH |
| AUTH-011 | Access denied | System | `/access-denied` | system state | Privacy-safe restricted route state. | no permission keys. | h1 Access denied, generic copy, safe action. | ready only. | Account/safe route. | Never show role/permission keys. | READY_FOR_STITCH |
| ONB-001 | Profile onboarding | Account | `/onboarding/profile` | form | Capture first/last name for minimum profile. | firstName, lastName. | h1 Complete your profile, two fields, save. | validation, saving, saved/error. | Account/root after save. | Do not expose `isProfileComplete` internals. | READY_FOR_STITCH |
| ACC-001/002 | Account overview and personal details | Account | `/account` | detail + form | View identity facts and edit first/last names. | email, username, firstName, lastName, emailVerified; optional roles as non-secret facts only if approved. | h1 Account, identity summary, personal details section, edit form same route. | loading, ready, edit, validation, saving, saved, error. | Account stays; actor areas through global nav. | Do not show raw user ID, permission keys, `isProfileComplete`, internal timestamps unless re-approved. | READY_FOR_STITCH |
| NUR-001/002 | Nurse profile overview | Nurse | `/nurse/profile` | detail | View professional identity summary and section summaries. | headline, summary, license, countries, years, recruitment availability, section counts/previews, CV metadata. | h1 Nurse profile, professional identity, facts, summary cards. | first-time 404 empty, loaded sparse, error. | Personal info and implemented section pages only. | No completion %, profile score, recruiter counts, raw IDs. | READY_FOR_STITCH |
| NUR-003 | Personal information | Nurse | `/nurse/profile/personal-information` | form | Create/update base professional profile. | headline, professionalSummary, licenseNumber, licenseCountry/currentCountry selectors, yearsOfExperience, isAvailableForRecruitment. | h1 Personal information, grouped professional fields, save/cancel. | 404 create, edit, validation, saving, saved, 409 country, error. | Back/save to overview. | No unsupported privacy tiers/employment type. | READY_FOR_STITCH |
| NUR-004/005 | Experience | Nurse | `/nurse/profile/experience` | list + form | Manage employment history. | jobTitle, facilityName, country, startDate, endDate, isCurrent, description. | h1 Experience, list cards, add/edit same page, inline delete confirmation. | empty, list, create, edit, validation, delete confirm, stale, error. | Back/overview. | No employer verification/status invention. | READY_FOR_STITCH |
| NUR-006/007 | Education | Nurse | `/nurse/profile/education` | list + form | Manage education. | institution, degree, fieldOfStudy, country, optional dates. | h1 Education, cards, add/edit same page, delete confirmation. | empty/list/form/validation/delete/error. | Back/overview. | No degree verification invention. | READY_FOR_STITCH |
| NUR-008/009 | Certificates | Nurse | `/nurse/profile/certificates` | list + form | Manage certificates. | name, issuingOrganization, issueDate, expirationDate?, credentialId?, credentialUrl?. | h1 Certificates, cards, add/edit form, credential link, delete confirmation. | empty/list/form/url validation/delete/error. | Back/overview. | No verified/official/trusted claim. | READY_FOR_STITCH |
| NUR-010 | Skills | Nurse | `/nurse/profile/skills` | form/list editor | Manage free-text skill tags. | normalized skill names only. | h1 Skills, chip editor, save. | empty, editing, validation duplicates/cap, saving, error. | Back/overview. | No taxonomy/autocomplete unless future contract. | READY_FOR_STITCH |
| NUR-011 | Languages | Nurse | `/nurse/profile/languages` | form/list editor | Manage languages and proficiency. | language select, proficiency enum Beginner/Intermediate/Advanced/Fluent/Native. | h1 Languages, editable rows, add/remove, save. | empty, validation duplicate/required/cap, saving, error. | Back/overview. | Option IDs internal only. | READY_FOR_STITCH |
| NUR-012 | CV | Nurse | `/nurse/profile/cv` | workflow | Upload/replace/delete CV. | file name, size, uploaded date metadata; selected file review. | h1 CV, current metadata, choose file, upload/replace/delete confirmation. | no CV, selected file, uploading, uploaded/replaced, delete confirm, error. | Back/overview. | No preview/download unless backend re-authorizes. | CONTRACT_INCOMPLETE |
| NUR-CONTACT | Contact requests | Nurse | `/nurse/contact-requests` | list/table | Review employer contact requests. | requester/employer facts provided by DTO, status, created date. | h1 Contact requests, status filter, paginated cards/table, approve/reject actions. | empty, filtered empty, loading, approve success, reject confirmation, error. | Same screen; future global nav. | No raw request IDs, no permission keys; reject irreversible confirmation. | READY_FOR_STITCH |
| EMP-001 | Employer home | Employer | `/employer` | detail/dashboard placeholder | Employer role landing/profile. | Existing contract incomplete. | h1 Employer, primary next links TBD. | loading/error TBD. | Candidates/requests. | Employer-only route UX; backend authoritative. | CONTRACT_INCOMPLETE |
| EMP-002/003/004 | Candidate search/results | Employer | `/employer/candidates` | list/filter | Search visible nurse candidates. | Recruitment-safe candidate fields only; no email/license/user IDs/CV. | h1 Candidates, filters, results list/cards. | empty, no results, pagination, error. | Candidate detail/request blocked. | Backend disclosure authoritative. | CONTRACT_INCOMPLETE |
| EMP-007/008 | Employer requests | Employer | `/employer/requests`, `/employer/requests/:requestId` | list + detail | Track recruitment/contact requests. | request status, candidate safe summary, dates. | list/detail with status filters if contract supports. | empty, filtered empty, not found, error. | list <-> detail. | Owner-scoped; no foreign disclosure. | CONTRACT_INCOMPLETE |
| EXM-001 | Exam catalog | Exams | `/exams` | list | Browse startable exams. | title, non-empty description, country/category, duration, question count, free marker. | h1 Exams, Country/Category filters, cards, analytics/history entries. | empty, no-results, pagination, error. | Detail, analytics, history. | No raw IDs, answers, correctness, purchase CTA. | READY_FOR_STITCH |
| EXM-002 | Exam detail | Exams | `/exams/:examId` | detail | Understand exam and go to instructions. | title, description, country/category, duration, question count, passing score, requirements. | h1 Exam detail, facts, instructions preview/fallback, action View instructions. | loading, ready, purchase-required state, not found, error. | Instructions/back to exams. | No questions/answers/IDs. | READY_FOR_STITCH |
| EXM-004 | Exam instructions | Exams | `/exams/:examId/instructions` | workflow | Review instructions and start/resume. | backend instructions, duration, count, passing score. | h1 Instructions, facts, modal confirmation for timed start/resume. | ready, confirmation, start loading, 409, error. | Session/detail/catalog. | No invented instructions. | READY_FOR_STITCH |
| EXM-005/006 | Exam session and submit confirmation | Exams | `/exams/:examId/sessions/:sessionId` | workflow + modal confirmation | Answer one question at a time, autosave, clear/flag, navigate, submit. | question text/points/options/order, persisted selected option, flagged marker, timer; transient submit aggregate only. | h1 Exam session, timer, visible numbered Question Navigation, options, Clear selection, Flag/Unflag, Prev/Next, Submit modal; no Save button. | loading, answering, saving/saved/save-error, near-expiry, expired, submit modal, submitting, transient result, errors. | Result, instructions/catalog. | Before finalized: no correctness, keys, explanations; clear and flag have no scoring effect. | READY_FOR_STITCH |
| EXM-007 | Exam result | Exams | `/exams/:examId/sessions/:sessionId/result` | report/detail | View finalized aggregate result. | score, maxScore, percentage, passed, correctCount, questionCount, terminal status. | h1 Exam result, aggregate summary, Back to exams, Review answers when T-FE-072 verified. | submitted, expired, unavailable, 409 unfinished, error. | Review/catalog. | No question/review/analytics; raw IDs hidden. | READY_FOR_STITCH |
| EXM-009 | Answer review | Exams | `/exams/:examId/sessions/:sessionId/review` | report/workflow | Review finalized answers one question at a time. | question, options, selected/correct indicators, explanation, points. | h1 Answer review, question pager, read-only option rows, explanation. | submitted, expired, unavailable, 409 unfinished, error. | Back to result. | Correct answers only post-finalized; no raw IDs/analytics. | READY_FOR_STITCH |
| EXM-008 | Exam analytics | Exams | `/exams/analytics` | report | Review historical performance facts. | summary metrics, by-exam, by-category, monthly trends from backend only. | h1 Exam analytics, filters, four sections in approved order. | no attempts, sparse sections, section errors, filters invalid, error. | Back to exams. | No charts, bands, recommendations, local derivation, raw IDs. | READY_FOR_STITCH |
| EXM-010 | Exam history | Exams | `/exams/history` | list | Review current/completed attempts. | exam title, status, started date, expiry for in-progress, finalized percentage/pass facts when non-null. | h1 Exam history, status filter, paginated rows/cards, row actions. | empty, filtered empty, loading, error. | Resume session/result/review/catalog. | No source/provenance, score/max details, raw IDs. | READY_FOR_STITCH |
| PP-001 | Package offers | Prep packages | `/preparation-packages` | list | Browse public preparation packages. | offer title, definition/exam/category/country facts, safe metadata. | h1 Preparation packages, contract filters, cards, view detail. | empty, no-results, pagination, error. | Offer detail. | No purchase/order behavior. | READY_FOR_STITCH |
| PP-002 | Package offer detail | Prep packages | `/preparation-packages/:offerSlug` | detail | Understand offer facts. | backend offer detail fields only. | h1 Package offer, sections for factual contents, back. | ready, not found, error. | Offers; commerce only when approved. | No invented benefits/terms. | READY_FOR_STITCH |
| PP-003 | My preparation packages | Prep packages | `/nurse/preparation-packages` | list | View owned entitlements. | offer/definition/exam titles, access windows, statuses, right availability. | h1 My preparation packages, paginated cards/table. | empty, loading, error. | Entitlement detail. | No price/currency/order internals. | READY_FOR_STITCH |
| PP-004 | Entitlement detail | Prep packages | `/nurse/preparation-packages/:entitlementId` | detail | View entitlement facts and eligible actions. | entitlement status, rights, access windows, purchase snapshot safe titles/dates. | h1 Package detail, rights sections, package exam section, practice/report links when authorized. | ready, not found, expired/no actions, error. | Practice/session/report/list. | Hide IDs/payment internals. | READY_FOR_STITCH |
| PP-005 | Practice | Prep packages | `/nurse/preparation-packages/:entitlementId/practice` | workflow/list | Practice package content. | practice progress and collection facts from backend. | h1 Practice, sections/actions per contract. | loading, empty, progress states, error. | Entitlement detail. | No material reader/download invention. | READY_FOR_STITCH |
| PP-007 | Package report | Prep packages | `/nurse/preparation-packages/reports/:sessionId` | report | View package-specific post-exam report. | summary counts/percentage, topic rows, recommended package content titles/types. | h1 Exam report, summary, per-topic report, recommendations section. | ready, unavailable, unfinished 409, no recommendations, error. | Back to preparation packages. | No question/answer/key/provenance/payment internals. | READY_FOR_STITCH |
| COM-001 | Product catalog | Commerce | `/commerce/products` | list | Browse purchasable backend products. | product title/name, description, type/category, price/currency. | h1 Products, paginated cards, View details. | empty, loading, error. | Product detail. | No provider IDs/raw product IDs. | READY_FOR_STITCH |
| COM-002 | Product detail | Commerce | `/commerce/products/:productId` | detail | Understand purchasable product. | product safe fields, price/currency, content summary only if DTO supplies. | h1 Product detail, facts, Purchase action only when ordering approved. | ready, unavailable, error. | Products/checkout future. | No invented benefits/provider claims. | READY_FOR_STITCH |
| COM-003 | Checkout | Commerce | `/checkout` | confirmation/workflow | Confirm before server-backed order creation. | product title, price/currency. | h1 Checkout/Create order?, confirmation facts, Create order/Cancel. | confirming, creating, ambiguous failure, conflict, success route. | Product/outcome/order. | Duplicate activation prevention; no card fields. | DEFERRED |
| COM-005/006 | Payment outcome | Commerce | `/checkout/orders/:orderId/success|failure` | system state/detail | Show server-truth outcome. | order status/payment truth, safe order facts. | h1 Payment completed/failed, View order, conditional Continue/Try again. | loading, reconciled success/failure, unavailable, error. | Order detail/products. | URL alone not proof; no provider internals. | CONTRACT_INCOMPLETE |
| COM-007 | Order history | Commerce | `/commerce/orders` | list | Review owned orders. | order/product title, created date, total amount/currency, status. | h1 Orders, paginated rows/cards, View order. | empty, loading, error. | Order detail/products. | No raw order/product/provider/idempotency IDs. | READY_FOR_STITCH |
| COM-008 | Order detail | Commerce | `/commerce/orders/:orderId` | detail + confirmation | View order truth and cancel if eligible. | order facts/status/dates/items safe fields. | h1 Order details, summary, item list, Cancel order confirmation. | ready, unavailable, cancel confirm, cancellation conflict, error. | Orders. | Owner-scoped, no provider internals. | READY_FOR_STITCH |
| ADM-002 | Admin users | Admin | `/admin/users` | table/list | Find users. | user list DTO safe fields: email/username/name/roles/status facts. | h1 Users, search/filter where implemented, table/cards, pagination. | empty, no-results, loading, error. | User detail. | No passwordHash/secrets; inspect raw JSON in tests. | READY_FOR_STITCH |
| ADM-003 | Admin user detail | Admin | `/admin/users/:userId` | detail/form | View user and manage allowed role/detail actions. | user detail DTO roles/permissions as authorized, no passwordHash. | h1 User detail, identity, roles/permissions facts, forms only where implemented. | loading, ready, not found, validation, save/error. | Users. | Hide passwordHash/tokens; permission-gated. | READY_FOR_STITCH |
| Admin future screens | ADM-005..ADM-PP-OFFERS | Admin | approved admin paths | list/detail/form | Admin manage respective domains. | Exact DTOs/contracts to be extracted in owning tasks. | Dense admin table/list patterns with mobile-card authority required. | loading, empty, no-results, validation, error. | Admin entry/family details. | Permissions exact; no sensitive internals. | CONTRACT_INCOMPLETE |
| SYS-001 | Route loading | System | non-routable | transient state | Indicate route transition loading. | loading text only. | Shell-level status. | entering/exiting routes. | None. | No fake progress. | READY_FOR_STITCH |
| SYS-002 | Not found | System | wildcard future, no canonical path | system state | Present unmatched route. | no backend data. | h1 Not found, safe copy, safe action. | ready. | Safe route/account/home TBD. | Preserve unmatched URL; no resource disclosure. | READY_FOR_STITCH |
| SYS-003 | Unexpected error | System | non-routable | system state | Present unexpected retryable failure. | error copy only. | h1/error heading, retry/safe action. | error/retry. | Contextual. | No raw backend text. | READY_FOR_STITCH |
| SYS-004/005 | Offline/Maintenance | System | non-routable/deferred | system state | Runtime/service state presentation only. | Runtime/deployment contract incomplete; HD-STITCH-05 allows factual shared visual states without workflow claims. | Calm factual notice, no queued-write or payment/exam safety claims. | offline/maintenance. | Safe retry/back where contract permits. | Do not imply offline storage, queued writes, payment continuation, or sync. | READY_FOR_SHARED_VISUAL_PATTERN_ONLY |
| SYS-006/007 | Empty/No results | System | non-routable | system state | Reusable list absence states. | title/body/action per owning screen. | calm state; optional CTA for empty only, reset for no-results. | empty/no-results. | Contextual. | No false data claims. | READY_FOR_STITCH |

## 8. Form Contracts

Global form rules: all fields require visible labels, programmatic labels, described-by links for help/error text, validation summary or field-level error copy where established, visible focus, keyboard operation, logical RTL layout, mobile single column, desktop columns only where they preserve reading order, explicit submit/cancel hierarchy, disabled/submitting states, and no hidden backend-only IDs as visible values.

| Form | Fields | Validation/source | Layout/action rules | Status |
|---|---|---|---|---|
| Sign in | username/email text, password password | Auth contract; required; password not logged/displayed. | One column; Sign in primary; forgot/sign-up links secondary. | READY_FOR_STITCH |
| Sign up | email, username, password | Public registration contract; backend validation authoritative. | One column; submit then check email. | READY_FOR_STITCH |
| Forgot password | email | Required/email format. | One column; accepted status does not reveal account existence beyond backend copy. | READY_FOR_STITCH |
| Reset password | email, newPassword, opaque token internal | email required/format, newPassword required/min 8/uppercase/digit per contract; token required but never visible. | One column; success state inline/non-routable. | READY_FOR_STITCH |
| Onboarding profile | firstName, lastName | required, max 100 per profile contract. | Two fields desktop optional 2-col; mobile single column; Save primary. | READY_FOR_STITCH |
| Account personal details | firstName, lastName | required, max 100, server trims. | Same-route edit state; Save/Cancel; saved status. | READY_FOR_STITCH |
| Nurse personal information | headline, professionalSummary, licenseNumber, licenseCountry, currentCountry, yearsOfExperience, isAvailableForRecruitment | Backend nurse profile validators; countries from `GET /api/v1/countries`; recruitment checkbox helper approved. | Semantic sections; save navigates overview; cancel back. | READY_FOR_STITCH |
| Nurse experience | jobTitle, facilityName, country, startDate, endDate, isCurrent, description | Date: if end provided then end >= start; isCurrent clears/disables end; no uniqueness rule. | Same page list/form; native date controls; delete inline confirmation. | READY_FOR_STITCH |
| Nurse education | institution, degree, fieldOfStudy, country, startDate?, endDate? | Optional dates; if both, end >= start. | Same page list/form; delete confirmation. | READY_FOR_STITCH |
| Nurse certificates | name, issuingOrganization, issueDate, expirationDate?, credentialId?, credentialUrl? | URL absolute http/https; expiration optional; no does-not-expire. | Same page list/form; external credential link display. | READY_FOR_STITCH |
| Nurse skills | skill input, removable chips | trim/collapse whitespace, duplicate case-insensitive, max 50, max 100 chars. | Chip editor; Save full replace; no taxonomy/autocomplete. | READY_FOR_STITCH |
| Nurse languages | language select, proficiency select, rows | language options from `GET /api/v1/languages`; proficiency exact enum; duplicate invalid; max 20. | Row editor; no preselected language/proficiency; Save full replace. | READY_FOR_STITCH |
| CV upload | file input | .pdf/.doc/.docx <=5MB. | Choose -> review -> explicit Upload/Replace; delete confirmation. | CONTRACT_INCOMPLETE |
| Contact request reject | two-step confirmation | Pending only; irreversible. | Inline confirmation, Reject/Keep; no modal. | READY_FOR_STITCH |
| Exam start/resume | modal confirmation only | Backend start/recover authoritative. | Timed attempt copy; Cancel/Start or Cancel/Resume. | READY_FOR_STITCH |
| Exam answer autosave | radio/list option selection, Clear selection | One selected option or none; selection/change and clear persist through the backend. | One question at a time; navigation and Submit wait for pending saves and stay put on failure with the choice retained for retry; no Save button. | READY_FOR_STITCH |
| Exam submit | modal confirmation | Show total/unanswered count from persisted session answers after pending saves settle; backend submit and scoring authoritative. | Irreversible confirmation; duplicate activation prevention. | READY_FOR_STITCH |
| Analytics filters | from date, to date, country, category | from <= to; country/category lookup reused; explicit Apply/Clear. | Shared filters across sections; query params for applied filters. | READY_FOR_STITCH |
| Exam history filter | status select | All/InProgress/Submitted/Expired; invalid query normalizes. | Immediate apply; page reset. | READY_FOR_STITCH |
| Commerce checkout | create order confirmation | Product/order contract; duplicate protection; reconciliation for ambiguous failure. | Create order/Cancel; no card fields. | DEFERRED |
| Order cancel | confirmation | Only when backend allows. | Keep order/Cancel order; reload after success/conflict. | CONTRACT_INCOMPLETE |
| Admin user forms | user detail/role forms where implemented | Existing admin API validators; exact permissions. | Dense admin form/table; mobile-card authority needed. | READY_FOR_STITCH for existing users; future admin forms incomplete. |

## 9. Report And Analytics Contracts

| Report | Audience | Source | Ordered sections | Metrics/fields | Filters/pagination | Forbidden |
|---|---|---|---|---|---|---|
| Exam result | Authenticated learner | `GET /exam-sessions/{sessionId}/result` | Heading/context, aggregate summary, status, actions. | score, maxScore, percentage, passed, correctCount, questionCount, Submitted/Expired display. | None. | Local derivation, question/review fields, timestamps, raw IDs. |
| Answer review | Authenticated learner | `GET /exam-sessions/{sessionId}/review` | Question pager, question, options, explanation, points. | correctness text, selected/correct labels, pointsEarned/points. | Local question index only. | Active inputs, changing answers, analytics, aggregate result duplication, raw IDs. |
| Exam analytics | Authenticated learner | Four `/me/nurse-profile/exam-analytics/*` operations | Overview, Performance by exam, Performance by category, Performance over time. | counts, percentages, average/best/latest metrics, monthly trend points directly from DTO. | filters from/to/country/category; by-exam/by-category pageSize 20; backend ordering. | Charts, performance bands, recommendations, local derivation, raw IDs, source/provenance. |
| Exam history | Authenticated learner | `GET /me/nurse-profile/exam-attempts` | Filter/status, attempt list, pagination. | title, status, started, expiresAt for in-progress, percentage/pass when non-null. | status + page query; pageSize 20. | score/max/correct counts, source, review details. |
| Package exam report | Nurse | `GET /exam-sessions/{sessionId}/report` | Summary, per-topic, recommended package content. | correct/question/percentage, topic counts/percentage, content title/type. | None unless backend later adds. | Question/answer/key/rationale, provenance, payment internals, bands. |

## 10. List And Table Contracts

Responsive rule: desktop may use table when dense comparisons are needed; tablet/mobile must transform to cards/lists unless horizontal overflow is explicitly justified and all content remains accessible. Admin dense data requires explicit native-table vs mobile-card design authority per screen before generation.

| Screen | Row/card identity | Secondary metadata | Actions | Pagination/filter/sort | Empty/no-results |
|---|---|---|---|---|---|
| Exam catalog | Exam title | country/category, duration, count, free marker | View details | page, country, category; backend order | Empty/no-results distinct. |
| Exam history | Exam title + status | started/ends/result facts | Resume/View result/Review | status, page, backend order | Empty/no-results distinct. |
| Nurse sections | Item title (role, degree, cert, skill, language, CV) | dates, country, metadata | edit/delete/open link/remove | backend/order-specific; no extra filters | Section empty states. |
| Contact requests | Requester/employer identity | status/date | approve/reject | status filter/page where contract supports | Empty/filtered empty. |
| PP offers | Offer title | exam/category/country facts | View detail | backend filters/page | Empty/no-results. |
| PP entitlements | Package identity | status/access window/rights | View detail | page only | Empty. |
| Products | Product title | price/currency/type | View details | backend page/order | Empty. |
| Orders | Order/product title | date/amount/status | View order | backend page/order | Empty. |
| Admin users | User identity | email/roles/status | View detail | implemented search/pagination | Empty/no-results. |
| Future admin lists | Domain identity | status/metadata per backend | View/manage according to permissions | backend authority only | Empty/no-results. |

## 11. Detail Contracts

Detail screens use: title identity, primary summary facts, section order matching the business workflow, status facts as text, primary action after summary, secondary actions after primary content, contextual relationships, privacy-safe 404/unavailable copy, long-content wrapping/clamping with accessible full content, and explicit back target.

Back targets:

- Exam detail -> Exam catalog.
- Instructions -> Exam detail.
- Result -> Exam catalog; Review -> Result.
- Nurse profile sections -> Nurse profile overview.
- Entitlement detail -> Entitlements list; package report -> Entitlements list.
- Product detail -> Product catalog.
- Order detail -> Order history.
- Admin user detail -> Admin users.
- Employer request detail -> Employer requests.

Do not depend on browser history unless an owning screen explicitly approves it.

## 12. Confirmation And Destructive Action Contract

| Action | Confirmation | Heading/body | Safe action | Confirm action | Focus/activation | Failure/reconcile |
|---|---|---|---|---|---|---|
| Exam start | Required | Timed exam copy. | Cancel | Start exam | Focus confirmation heading; duplicate blocked. | 409 factual, reconcile route/session. |
| Exam resume | Required where approved | Timer continued copy. | Cancel | Resume exam | Same. | 409 factual. |
| Exam submit | Required modal | total/unanswered count from persisted answers after pending saves settle; irreversible. | Cancel | Submit exam | Prevent duplicate submit. | Generic retry/error, no blind duplicate. |
| Package exam start | Required | single-attempt consumption copy. | Cancel | Start exam | Prevent duplicate POST. | 409 reconcile state. |
| Package exam resume | Required | resume existing timed session copy. | Cancel | Resume exam | Prevent duplicate POST. | 409 reconcile state. |
| Nurse item delete | Required inline | Delete item? cannot be undone. | Keep | Delete | Inline flow, focus remains in document. | Stale/deleted notice and refresh. |
| CV delete | Required | Delete CV? | Keep | Delete CV | Inline/two-step. | Stale/error safe. |
| Contact request reject | Required inline | Reject this contact request? cannot be undone. | Keep | Reject | Normal document flow. | Refetch server truth. |
| Contact request approve | No confirmation V1 | Immediate pending-only approve. | n/a | Approve | Duplicate blocked. | Refetch server truth. |
| Commerce create order | Required | Create order? order will be created. | Cancel | Create order | Duplicate blocked. | Reconcile if ambiguous; stop if no contract. |
| Order cancel | Required | Cancel order? eligibility caveat. | Keep order | Cancel order | Duplicate blocked. | Conflict copy and reload. |

## 13. System State Contract

| State | Pattern | Copy source | Requirements |
|---|---|---|---|
| Route/page loading | SYS-001 + T-FE-033 | Shared factual loading text. | `role=status`, stable dimensions, no fake progress. |
| Section loading | Local section status | Owning section. | Preserve layout where possible, announce if meaningful. |
| Empty | SYS-006 | Owning screen. | No global records exist; optional create CTA if same-screen creation is authorized. |
| Filtered empty/no-results | SYS-007 | Owning screen. | Preserve filters; offer clear/reset; do not suggest no records exist globally. |
| Unavailable/not found | Contextual or SYS-002 | Owning screen. | Privacy-safe; no raw IDs/status/backend text. |
| Forbidden/restricted | Access denied | System packet. | Generic copy, no role/permission keys. |
| Retryable error | T-FE-033 | Owning screen. | Retry on same route/context, no raw exceptions. |
| Offline | SYS-004 | Runtime authority missing. | HUMAN_DECISION_REQUIRED; do not imply offline safety. |
| Maintenance | SYS-005 | Runtime authority missing. | HUMAN_DECISION_REQUIRED; do not imply scheduled availability. |
| Successful save | Announcer/status | Owning screen approved copy. | Non-color status; no navigation unless approved. |
| No-access/expired-access | Feature-specific | Backend truth. | Factual copy; no entitlement/payment inference beyond backend. |

## 14. Design System Contract

Design language: professional, clinical, calm, readable, trustworthy, and task-oriented. Avoid decorative complexity.

Color roles:

| Role | Foundation value/usage |
|---|---|
| Application background | `#F6FBFA` |
| Card/surface | `#FFFFFF` |
| Subtle surface | `#F3F6F8` / `#EEF7F5` |
| Primary text | Professional Navy `#173B57` / high contrast `#102A3A` |
| Secondary text | `#4F6473` |
| Muted text | `#5F7280` only on approved backgrounds/pairings |
| Borders/dividers | `#C9DDDA`, strong `#8FA9A5` |
| Primary interactive | Nursing Teal `#006B66`; hover `#005A56` |
| High-emphasis domain role | Exam Focus Indigo `#4F46B8` for exam-context primary/high-emphasis actions only where approved |
| Success | `#147A4B` |
| Warning | `#8A4B00` |
| Error | `#B3261E` |
| Info | `#0B63A3` |
| Disabled | disabled opacity `0.42` plus non-color affordance; do not rely on inaccessible Neutral 500 foreground pairing |
| Overlay/scrim | opacity `0.48` |
| Focus | Existing verified frontend focus behavior unless reconciled; visible 2px+ offset/ring; no color-only focus |

Typography:

- English UI: Noto Sans.
- Arabic/RTL UI: Noto Sans Arabic; avoid letter spacing.
- Display/page heading/section heading/body/label/helper/status roles must be semantic, not arbitrary raw sizes.
- Numeric/metric treatment: stable direction in RTL, tabular alignment only where supported by foundation, percentages/currency presented textually.
- Readable line lengths: constrain long prose; do not expand text indefinitely on wide screens.

Spacing/shape/elevation:

- 4px base system; page gutters mobile 16, tablet 24, desktop 32.
- Stack/cluster/container spacing roles from approved foundation.
- Radius: inputs 8, buttons 12, cards 16, dialogs 24, pills full.
- Elevation restrained; use borders/spacing first; menus/dialogs use clear overlay hierarchy.

Controls and states:

- Buttons, links, inputs, password controls, selects, checkbox/radio, textarea, pagination, navigation items, status badges, progress, tables, cards, dialogs, banners, notices must define default/hover/focus/active/disabled/loading/selected/error/success states.
- Password visibility is a shared auth/form pattern requirement from audit evidence; product behavior remains bounded to password fields only.
- Target sizes: 44x44 minimum, 48x48 preferred mobile.
- Reduced motion: no custom decorative motion tokens yet; use reduced-motion-safe behavior and avoid arbitrary durations/easing.

## 15. UI/UX Audit Findings Classification

| Finding | Classification | Contract action |
|---|---|---|
| Global shell/navigation absent | SHARED_PATTERN_REQUIREMENT + HUMAN_DECISION_REQUIRED | App shell/navigation is first Stitch batch; exact architecture unresolved. |
| Shared auth shell consistency | SHARED_PATTERN_REQUIREMENT | Auth screens use one calm auth shell; no current Angular layout authority. |
| Password visibility | DESIGN_SYSTEM_REQUIREMENT | Approved as control pattern for password fields; no product semantic change. |
| Validation focus flow | IMPLEMENTATION_ONLY_LATER + DESIGN_SYSTEM_REQUIREMENT | Define focus/error pattern; implement later in Angular tasks. |
| Profile completion/navigation | HUMAN_DECISION_REQUIRED / NOT_SUPPORTED_BY_PRODUCT_CONTRACT | NUR-013 intentionally deferred; no profile completion score. |
| Long-content overflow | DESIGN_SYSTEM_REQUIREMENT | Critical content never clipped; truncation accessible. |
| Exam question navigation | NOT_SUPPORTED_BY_PRODUCT_CONTRACT where it adds navigator | One-question Previous/Next only; no question-number navigator. |
| Timer urgency | SCREEN_REQUIREMENT | Near-expiry warning, factual expired state; no pause. |
| Saving/saved answer status | SCREEN_REQUIREMENT | Explicit save answer status. |
| Submit confirmation | CONFIRMATION_REQUIREMENT | Required; irreversible copy. |
| Package/practice progress | SCREEN_REQUIREMENT where backend supplies | No unsupported progress claims. |
| Admin table semantics | SHARED_PATTERN_REQUIREMENT + HUMAN_DECISION_REQUIRED | Need native table vs mobile-card authority for dense admin. |
| Offline/maintenance states | HUMAN_DECISION_REQUIRED | Runtime/deployment authority missing. |
| Shared responsive container | DESIGN_SYSTEM_REQUIREMENT | Global container/gutter rules. |
| RTL behavior | DESIGN_SYSTEM_REQUIREMENT | Logical properties, stable numbers, Arabic font. |
| Focus visibility | DESIGN_SYSTEM_REQUIREMENT | Visible focus required; focus token conflict remains known. |
| Typography | DESIGN_SYSTEM_REQUIREMENT | Noto Sans / Noto Sans Arabic; semantic scale. |
| Token coverage | DESIGN_SYSTEM_REQUIREMENT | DESIGN.md must include tokens; missing icon/motion/density deferred. |
| Contrast | DESIGN_SYSTEM_REQUIREMENT | WCAG 2.2 AA target; no color-only meaning. |
| Reduced motion | DESIGN_SYSTEM_REQUIREMENT | No arbitrary motion; reduce/disable decorative motion. |

## 16. Screen Flow Graph

| From | Action | To | Condition | Route/path | Owner |
|---|---|---|---|---|---|
| Root | Resolve entry | Sign in / actor area | HUMAN_DECISION_REQUIRED | `/` -> TBD | Shell/navigation |
| Sign in | Submit success | returnUrl/account/root | Auth success | safe return | Auth |
| Sign in | Forgot password | Forgot password | user action | `/auth/forgot-password` | Auth |
| Sign in | Create account | Sign up | user action | `/auth/sign-up` | Auth |
| Sign up | Accepted | Check email | backend accepted | `/auth/verify-email` | Auth |
| Email link | Verify token | Verify email result | token present | `/auth/verify-email/confirm` | Auth |
| Forgot password | Request reset | Out-of-band/check email | backend accepted | same screen/status | Auth |
| Reset link | Submit reset | Reset success state | backend success | non-routable state | Auth |
| Account | Edit personal details | Account edit state | user action | `/account` | Account |
| Nurse entry | Redirect | Nurse profile | Nurse role | `/nurse/profile` | Nurse |
| Nurse profile | Edit personal information | Personal information | route implemented | `/nurse/profile/personal-information` | Nurse |
| Nurse profile | Manage experience/education/certs/skills/languages/CV | Section route | section implemented | profile section paths | Nurse |
| Nurse contact requests | Approve | Refreshed list | pending row | same route | Nurse |
| Nurse contact requests | Reject | Inline confirmation -> refreshed list | pending row | same route | Nurse |
| Exams catalog | View details | Exam detail | exam row | `/exams/:examId` | Exams |
| Exams catalog | View analytics | Exam analytics | action | `/exams/analytics` | Exams |
| Exams catalog | View history | Exam history | action | `/exams/history` | Exams |
| Exam detail | View instructions | Exam instructions | can start/purchase state handled | `/exams/:examId/instructions` | Exams |
| Exam instructions | Start/Resume confirmed | Exam session | backend returns session | `/exams/:examId/sessions/:sessionId` | Exams |
| Exam session | Select/change or clear answer (autosave) | Same question/session | pending mutation persists before navigation or Submit | same route | Exams |
| Exam session | Submit confirmed | Transient result | backend success | same route state | Exams |
| Transient result | View full result | Exam result | T-FE-071 implemented | `/exams/:examId/sessions/:sessionId/result` | Exams |
| Exam result | Review answers | Answer review | T-FE-072 implemented/finalized | `/exams/:examId/sessions/:sessionId/review` | Exams |
| Answer review | Back to result | Exam result | user action | result route | Exams |
| Exam history | Resume exam | Exam session | InProgress row | session route | Exams |
| Exam history | View result/review | Result/review | finalized row | result/review routes | Exams |
| Package offers | View detail | Offer detail | offer row | `/preparation-packages/:offerSlug` | PP |
| My packages | View detail | Entitlement detail | entitlement row | `/nurse/preparation-packages/:entitlementId` | PP |
| Entitlement detail | Practice | Practice | right available | `/nurse/preparation-packages/:entitlementId/practice` | PP |
| Entitlement detail | Start/Resume package exam | Exam session | backend package start returns session | shared session route | PP/Exams |
| Entitlement detail | View exam report | Package report | finalized package session | `/nurse/preparation-packages/reports/:sessionId` | PP |
| Products | View details | Product detail | product row | `/commerce/products/:productId` | Commerce |
| Product detail | Purchase | Checkout | T-FE-084 reopened | `/checkout` | Commerce |
| Checkout | Create order | Outcome/order flow | backend order | outcome/order routes | Commerce |
| Payment success/failure | View order | Order detail | order available | `/commerce/orders/:orderId` | Commerce |
| Orders | View order | Order detail | order row | `/commerce/orders/:orderId` | Commerce |
| Order detail | Cancel order | Reconciled detail | backend allows | same route | Commerce |
| Admin users | View user | User detail | row action | `/admin/users/:userId` | Admin |
| Admin future lists | View/manage | Detail/edit states | owning contract | admin routes | Admin |
| Employer candidates | Request contact | Request flow/detail | backend permits | blocked/no route yet | Employer |
| Employer requests | View request | Request detail | row action | `/employer/requests/:requestId` | Employer |

Complete major workflow coverage: Auth, Account, Nurse profile, Exams, Preparation Packages, Commerce, Employer, Admin, Shared/System. Flow graph remains repository authority even without a Stitch linking capability.

## 17. Stitch Naming And Organization Plan

Naming scheme:

```text
[Family] / [Screen ID] / [Screen Name] / [Variant]
```

Examples:

- `Shell / APP-001 / Authenticated Shell / Desktop`
- `Shell / NAV-001 / Global Navigation / Mobile Drawer`
- `Auth / AUTH-001 / Sign In / Desktop`
- `Nurse / NUR-001 / Profile Overview / RTL`
- `Exams / EXM-005 / Session / Warning`
- `Exams / EXM-006 / Submit Confirmation / Mobile`
- `Commerce / COM-003 / Checkout / Confirmation`

Variant rules:

- Base screen: Desktop LTR ready/default state.
- Responsive variants: Mobile required when composition materially changes; Tablet only when distinct from desktop/mobile.
- RTL variants: required for app shell/navigation and complex forms/reports/tables; simple screens may use a representative RTL variant plus component-level RTL rules.
- State variants: loading/error/empty/no-results/confirmation/terminal only when composition materially changes.
- Role variants: only when actor content or navigation changes materially.
- Do not duplicate screens for data value changes, different IDs, or simple copy swaps.
- Prefer design-system components for repeated controls/states instead of generating one-off variants.

## 18. Stitch Variant Strategy

Use `stitch_generate_variants` intentionally for:

- Mobile navigation and shell.
- RTL shell/navigation and high-density forms/reports.
- Major states: empty, no-results, forbidden, not found, submit confirmation, destructive confirmation, terminal outcomes.
- Role navigation differences when layout actually changes.
- Admin dense-table mobile-card transformations.

Do not use variants for:

- Different backend records or IDs.
- Simple status text changes that the same component handles.
- Styling exploration outside approved design system.
- Provider/payment behavior that is not contract-authorized.

## 19. Responsive, RTL, Accessibility, And Security Rules

Responsive:

- Desktop uses constrained content widths and useful columns before empty whitespace.
- Tablet reflows facts/cards/forms without truncating critical content.
- Mobile single-column by default; actions wrap; no unintended horizontal page overflow.
- Tables transform to cards/lists unless explicit horizontal overflow is justified.

RTL:

- Use logical start/end, inline/block spacing, and mirrored directional icons only when directional.
- Preserve status icons, brand marks, numbers, dates, percentages, currency, and mixed English/Arabic terms.
- Arabic content uses Noto Sans Arabic and must not be shrunk to force fit.

Accessibility:

- WCAG 2.2 AA target.
- Semantic landmarks, one screen `h1`, section headings in order.
- Visible focus, keyboard-complete operation, no color-only meaning.
- 44x44 minimum targets, 48x48 preferred mobile.
- Loading/status announcements through appropriate live regions.
- Dialog/confirmation focus entry/return if modal architecture is chosen; inline confirmations remain in document flow.
- Reduced-motion safe behavior; no arbitrary decorative transitions.

Security/non-exposure:

- Never display password hashes, secrets, tokens, provider IDs, idempotency keys, raw GUIDs, internal permission keys, storage keys, internal authorization state, answer keys before finalization, payment-provider internals, or backend exception text.
- Raw IDs may exist in routes/query/internal state when canonical route requires them but must not render visibly.
- Backend ownership/authorization remains authoritative; frontend navigation is UX only.

## 20. Human Decision Required, Contract Incomplete, Backend Blocked, Deferred

HUMAN_DECISION_REQUIRED:

- Future implementation task number/gate for Angular shell/navigation.
- Final optional breadcrumb policy beyond Phase 2 shell placeholder.
- Workflow-specific offline/maintenance behavior beyond factual shared visual-state treatment.
- Future actor-specific Employer shell once Employer screen contracts are complete.

CONTRACT_INCOMPLETE screens:

- Employer home/candidates/requests/detail.
- Admin reference data/exams/exam detail/versions/questions/payment products/preparation-package admin screens.
- Commerce outcomes/orders/order detail until implementation prerequisites close.
- Nurse CV if the current route/component gap remains in implementation reality.

BACKEND_BLOCKED / DEFERRED:

- AUTH-012 Account inactive: backend/design blocked.
- ACC-003 Change password, ACC-004 Security/sessions, ACC-005 Notification preferences, ACC-006 Account status: backend gaps.
- NUR-013 Professional profile completion: intentionally not implemented/deferred.
- EMP-005 Candidate profile and EMP-006 Recruitment request: candidate detail backend gap.
- PREPARATION_PACKAGES_MATERIAL_READER: no learner material delivery contract.
- COM-003/T-FE-084 checkout remains intentionally deferred by current human decision.
- COM-004 payment processing route deferred pending provider-callback architecture.
- Production payment release/provider-specific outcomes blocked by external/backend decision.
- ADM-001 Dashboard, ADM-004 roles/permissions, ADM-009 payment orders, ADM-010 recruitment management: backend gaps.
- SYS-004 Offline, SYS-005 Maintenance: shared visual states may be designed factually per HD-STITCH-05; workflow/runtime behavior remains deferred.

## 21. Phase 2 Batch Plan

First future Stitch batch after Phase 1 approval:

1. Design system from this contract.
2. App shell.
3. Global navigation.

Rationale: the current application has route logic but no practical user-facing navigation, and individual feature screens must not invent navigation independently.

Subsequent family batches:

1. Authentication + Account.
2. Nurse profile.
3. Exams.
4. Preparation Packages.
5. Commerce, excluding deferred checkout until reopened.
6. Employer incomplete contracts only after human decisions/backends mature.
7. Administration screens with admin table/card authority.
8. Shared system states and variants.

No Stitch write operation may execute until this contract is reviewed and approved.
