# Screen Continuation Readiness

## 1. Purpose and non-authority

This is a readiness map for continuing the design program. It is not a page approval, canonical route registry, page specification, Penpot authorization, visual approval, or Angular implementation authorization. G0 is accepted as governance/re-entry baseline only; all candidate screens still require Phase 1 evidence and separate later approvals.

## 2. Current gates

| Gate | Status | Meaning |
|---|---|---|
| G0 — authority reconciled | Accepted, governance-only | Karam accepted the baseline on 2026-08-12; this does not authorize screens, Penpot writes, or Angular work. |
| G1 — evidence ready | Not started; awaits separate Phase 1 task | Routes, actors, API, permissions, validation, errors, and state evidence must be captured or explicitly blocked. |
| G2 — shared contracts frozen | Not started | Tokens, theme, component, accessibility, NFR, fixture, and visual contracts must be approved. |
| G3 — inventory approved | Not started | Canonical route/page boundaries, actors, permissions, risk, and batches must be approved. |

Current frontend execution after the 2026-09-11 `T-FE-040` packet, the 2026-09-12 AUTH-001 blocker-resolution contract, and the 2026-09-13 V1 public self-registration contract has superseded the earlier Authentication-family row below for implementation selection: the detailed screen decisions now live in `docs/frontend/execution/frontend-implementation-ledger.md` under `T-FE-040`, `ST-FE-040`, `GATE-FE-T040`, and the Screen Ownership Matrix. This readiness map remains historical/governance context and is not the approval ledger.

## 3. Approved-to-preserve screen/design evidence

The following Penpot evidence must be preserved but is not visually approved or implementation-ready:

- Foundation pages: Cover, Getting Started, Colors, Typography, Spacing & Shape, Elevation & States, and Components.
- AUTH-001 / Authentication Core: legacy/draft evidence only.
- Product Map: planning inventory, not production screen design.
- User Flows: legacy/draft evidence with unresolved classification.
- Utilities: a preserved conflict; overlapping boards and substantively incomplete utilities remain unresolved.

## 4. Candidate screen families

All families below are candidates only and cannot enter page-spec production or Penpot work now.

| Family | Business purpose / likely actors | Evidence available | Blockers | Phase 1 evidence packet | Page-spec production now | Penpot now |
|---|---|---|---|---|---|---|
| Authentication | Sign-in, account recovery, verification; anonymous users | Backend/auth architecture, route/access contracts, auth/session foundation, shared form/validation/error foundations, V1 public self-registration contract, and legacy AUTH-001 notes | `T-FE-040` resolved the family approval packet; later contracts resolved AUTH-001 successful-login handoff/fallback blockers and approved AUTH-002/AUTH-003/AUTH-004/AUTH-005 for the V1 self-registration flow; AUTH-012 remains blocked/deferred in the frontend ledger | Completed in `T-FE-040`; use the frontend ledger for exact screen decisions | No | No |
| Account | Current-user profile and security actions; authenticated users | `/me` architecture contract and backend roadmap | Routes/DTOs/error states | Eligible for separately authorized Phase 1 evidence | No | No |
| Nurse | Nurse profile and professional data; nurses | Roadmap/backend modules, typed nurse/recruitment OpenAPI + generated client contracts (2026-09-16 readiness campaign), T-FE-052 approval packet `nurse-screen-approval-packet.md` with human-approved D1–D10 decisions | NUR-003..012 screen-specific design deferred to owning gates; NUR-013 backend gap | `GATE-FE-T052` VERIFIED — NUR-001/002 APPROVED (T-FE-056 awaiting separate start authorization) | No | No |
| Employer | Organization and candidate workflows; employers | Roadmap/backend modules | Employer ownership evidence | Eligible for separately authorized Phase 1 evidence | No | No |
| Recruitment | Search, contact-request, disclosure workflows; nurses/employers | Roadmap/backend modules | Ownership/state evidence | Eligible for separately authorized Phase 1 evidence | No | No |
| Examinations | Catalog, sessions, results, analytics; nurses/admin | Implemented backend and current architecture | OpenAPI, timer/resume/rationale evidence | Eligible for separately authorized Phase 1 evidence | No | No |
| Payments | Orders, checkout, outcomes; nurses/admin | Sandbox/payment backend baseline | Release/payment contract and exact permissions | Eligible for separately authorized Phase 1 evidence | No | No |
| Administration | Administrative maintenance; admins | Roadmap plus implemented backend surfaces | Exact route/permission evidence | Eligible for separately authorized Phase 1 evidence | No | No |
| Shared system | Shell, forbidden, not found, session expiry; all actors | Frontend architecture | Route registry and shared contracts | Eligible for separately authorized Phase 1 evidence | No | No |
| Preparation Package | Package discovery, rights, practice, attempts, reports, administration; visitors/nurses/admins | Backend Stage 1–4 handoff baseline `8439511` | Phase 1 operation/DTO/permission/error capture | First eligible separately authorized packet | No | No |

## 5. Preparation Package candidate screen areas

The following are backend-ready candidates only. Each is **Candidate only**, **Needs Phase 1 evidence**, **No page spec yet**, and **No Penpot write yet**.

| Candidate area | Scope boundary |
|---|---|
| Public package catalog and offer discovery | Active eligible offers only, according to captured API evidence. |
| Package offer detail | Use confirmed offer/package DTOs only. |
| Checkout/package purchase handoff | Only the current Sandbox/payment evidence; no production payment behavior is implied. |
| Nurse package entitlements list/detail | Nurse-owned entitlement/read contracts only. |
| Package study-material access/read state | Excludes storage, upload, download, and delivery behavior not implemented. |
| Package practice progress | Entitlement-scoped progress and active-write/historical-read boundaries only. |
| Package exam-session start/resume | Explicit entitlement selection and documented source-conflict/idempotency outcomes only. |
| Package analytical report read | Nurse-owned read after eligibility/expiry behavior confirmed; no protected exam content. |
| Admin reporting topics/profiles | Exact administration permissions and lifecycle rules require evidence capture. |
| Admin study-material metadata/version management | Metadata/version administration only; no delivery implementation. |
| Admin practice collections | Independent practice content only; no official exam-content leakage. |
| Admin package definitions/versions/composition | Immutable composition and publication constraints require evidence capture. |
| Admin package offers | Offer lifecycle and commercial configuration require evidence capture. |

## 6. Screens/features explicitly not allowed yet

- Storage, upload, download, or delivery as implemented package features.
- Offline access or sync.
- Workspace/dashboard aggregation.
- Adaptive practice or spaced repetition/retraining.
- Employer package purchase, progress, or report visibility.
- Production payment provider, public webhook, refunds, reconciliation, or subscriptions unless separately implemented and evidenced.
- Any Angular implementation.

## 7. Required evidence before screen specs

- Current generated Development OpenAPI capture and source revision.
- Operation index, DTO index, permission index, validation index, and error-response index.
- Route/actor matrix and evidenced business state transitions.
- Accepted design tokens, component contracts, breakpoint/viewport contract, and accessibility/RTL contract.
- Classification of relevant Penpot discrepancies and explicit confirmation of approved visual source status.

## 8. Recommended next design sequence

1. Authorize a bounded Preparation Package Phase 1 Evidence Packet.
2. Build the required evidence indexes.
3. Draft a candidate route/page registry.
4. Freeze shared foundation contracts.
5. Only then create page specifications.
6. Only after approved page specifications, authorize Penpot work.
7. Treat Angular scaffold approval as a separate later task.

## 9. Remaining open questions

This map does not close Phase 1 or design-foundation questions. See `docs/frontend/design/governance/open-questions.md`, including `OPEN-PH1-001` through `OPEN-PH1-005`, `OPEN-001` through `OPEN-009`, and the unresolved `DISC-PEN-*` and `DISC-REP-001` records.
