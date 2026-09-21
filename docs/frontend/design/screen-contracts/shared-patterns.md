# Shared Screen Patterns

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-SHARED-PATTERNS
status: active-pattern-contract
owner: frontend-design-governance
updated: 2026-09-21
visual_authority: docs/frontend/design/stitch/DESIGN.md
```

## Authority

These patterns are executable guidance for future Stitch, Storybook, and Angular screen work. They do not create product behavior. Business-specific fields, endpoints, copy, and actions remain in family contracts or higher authority.

All visual styling, typography, color, spacing, shape, elevation, focus, and token values come from `docs/frontend/design/stitch/DESIGN.md` and the approved frontend design foundation. Do not copy token tables into screen contracts.

## App Shell

Required shell anatomy for authenticated layouts:

| Element | Contract |
|---|---|
| Skip link | First keyboard-reachable control; jumps to `main`. |
| Top app bar | Hybrid top-app-bar architecture approved by HD-STITCH-01; mobile may collapse navigation into an accessible menu/drawer. |
| Brand/home affordance | Displays `Nursing Platform`; must not invent root redirect semantics. |
| Primary navigation | Actor-aware destinations filtered by `navigation-permission-policy.ts`; visible text labels required; icons may support labels but must not replace them. |
| Account affordance | Authenticated shell exposes account access and sign-out affordance; no fake identity, fake title, or invented avatar. |
| Main landmark | One `main` region; owning screen provides one `h1`. |
| Active state | Use text/shape/`aria-current` where applicable; never color alone. |

Navigation is a visibility aid only. Backend authorization remains authoritative.

## Navigation

Primary family destinations:

| Actor | Allowed primary families |
|---|---|
| Anonymous | Sign in, Sign up, public Preparation Packages offers/detail, safe terminal/system states when routed. |
| Nurse | Exams, Preparation Packages, Products/Commerce when eligible, Profile. Account remains an account affordance, not primary product navigation. |
| Employer | Employer home, Candidates, Requests only after family contracts mature. |
| Admin | Admin entry, Users, Exams/reference/payment/preparation-package admin areas only according to exact permission policy. |

Do not generate global navigation entries for non-routable states, detail-only pages, deferred concepts, DPF items, or blocked screens.

## Account Affordance

Allowed visible actions: `Account`, `Sign out`, and a neutral account label. Do not show a hard-coded name, email, role title, credential, token/session status, verification badge, or compliance/security claim.

## Forms

Every form contract must specify fields in the owning family file. Shared form behavior:

| Concern | Required behavior |
|---|---|
| Labels | Every control has visible label and programmatic label. |
| Help/error | Help and error text use described-by relationships when implemented. |
| Required/optional | Required state comes from backend/API/packet authority; do not infer from visual mocks. |
| Validation | Client validation mirrors approved backend constraints only; backend remains source of truth. |
| Error summary | Use persistent summary when multiple fields or submission errors need orientation. |
| Submit | Primary action label must describe the operation; duplicate activation is prevented for mutations. |
| Cancel/back | Returns to the owning route/section listed in the family contract; do not rely on browser history unless approved. |
| Disabled/read-only | Disabled and read-only are visually and semantically distinct. |
| Password fields | Password visibility is allowed only for password controls. |
| Hidden/internal fields | Tokens, IDs, ownership keys, and route params are never displayed as form values unless explicitly approved. |

## Validation And Errors

Use endpoint-aware Problem Details mapping. Do not expose raw exception messages, raw backend detail where privacy-sensitive, stack traces, internal IDs, permission keys, or storage keys. `401`, `403`, `404`, `409`, `422`, `429`, and `503` must retain their approved semantic handling from frontend architecture and family contracts.

## Lists And Tables

Every list/table family contract must specify item identity, secondary metadata, actions, filters, pagination, and empty/no-results behavior.

Shared rules:

| Concern | Required behavior |
|---|---|
| Item identity | First visible text identifies the row/card from user-meaningful data, not raw IDs. |
| Column/order | Desktop table columns appear in contract order. |
| Row actions | Row actions are listed explicitly; no hidden bulk actions unless approved. |
| Search/filter/sort | Only available when approved by backend/API/screen authority. |
| Pagination | Show current page and item availability using backend paging facts when provided; page reset on filter change where approved. |
| Empty | No records exist for the current owner/context. |
| No results | Filters/search produced no matching results; preserve filters and offer clear/reset when approved. |
| Mobile | Transform dense tables to cards/lists unless approved horizontal wide-data handling exists. |
| Wide data | Critical content/actions never clip; horizontal overflow must preserve keyboard focus visibility. |

## Search, Filters, Sorting, Pagination

Search must name the searched field set. Filters must list allowed fields/options. Sorting must not be added unless backend/API or approved client behavior defines order. Pagination must include page, page size, total count/total pages only when backend supplies those facts or a verified contract defines them.

## Reports And Analytics

Reports must present backend-provided facts only. Family contracts must list ordered sections, metric names, types, units, null/zero behavior, source endpoints, filters, and forbidden derivations. Charts are not approved unless a family contract explicitly authorizes them. Tables are preferred for exact diagnostic data unless future design authority approves charts.

## Destructive Confirmations

Confirmation contracts must define the exact action, precondition, copy intent, safe action, confirm action, focus behavior, duplicate prevention, success, failure, and reconciliation path. Inline confirmation is preferred where the action belongs to an item row and a modal is not required.

## Loading, Empty, No-Results, Restricted, Unavailable

| State | Shared contract |
|---|---|
| Loading | Identify the task; use `role=status` where meaningful; no fake progress. |
| Section loading | Preserve surrounding layout; do not block unrelated sections. |
| Empty | Explain absence in current owner/context; optional CTA only when same-screen creation is authorized. |
| No results | Preserve filters/search; offer clear/reset only when filters/search exist. |
| Restricted | Generic access-denied copy; no permission keys or protected-resource disclosure. |
| Unavailable/not found | Privacy-safe; do not expose raw IDs or backend exception detail. |
| Saved/success | Non-color status; route changes only when contract says so. |

## Responsive Containers

Use constrained content widths and useful columns on desktop, reflow to readable single-column mobile when needed, and preserve all critical actions without horizontal page overflow. Detail/report pages may use multi-column desktop summary layouts only when reading order remains clear.

## Dense Data Mobile Transformation

Admin and wide tabular screens require explicit authority for desktop table and mobile cards. Mobile cards must preserve identity, essential metadata, row action, permission-sensitive controls, and status text. Do not hide critical columns without an accessible detail path.

## RTL

Use logical start/end and inline/block spacing. Mirror directional affordances only when semantically directional. Preserve brand marks, status icons, numbers, dates, percentages, currency, route tokens, and mixed English/Arabic terms. Arabic uses the approved Arabic font from `DESIGN.md`.

## Accessibility

All screens target WCAG 2.2 AA intent: semantic landmarks, one page `h1`, ordered headings, keyboard-complete operation, visible focus, no color-only meaning, 44x44 minimum targets and 48x48 preferred mobile targets, live-region announcements for meaningful loading/success/error, and reduced-motion-safe behavior.

## Design Classification

Every visible functional element in future Stitch output must be classified as:

| Classification | Meaning |
|---|---|
| `AUTHORITATIVE_FEATURE` | Already supported by route/product/backend/frontend authority. |
| `DESIGN_PROPOSED_FEATURE` | Recorded in `design-proposed-features.md`; not implementation authority. |
| `VISUAL_ONLY_ELEMENT` | Decorative/compositional only. |
| `UNSUPPORTED_CLAIM` | Must not be shown as factual truth without separate verification. |

Known DPFs: `DPF-001` Notifications and `DPF-002` Help / Support Access.

## Unsupported Claim Ban

Forbidden unless separately verified and approved for the exact context: HIPAA/GDPR/WCAG certification claims, `System Online`, `Token Verified`, service-health claims, clinical-system status, bank-grade/security guarantees, encrypted-by-default claims, provider/payment security promises, fake user identity, fake professional title, debug labels, route IDs, token/session internals, and unsupported footer claims.
