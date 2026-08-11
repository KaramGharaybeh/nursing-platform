# Screen Continuation Readiness

## 1. Purpose and non-authority

This is a readiness map for continuing the design program. It is not a page approval, canonical route registry, page specification, Penpot authorization, visual approval, or Angular implementation authorization. It records current evidence and blockers only. G0 remains not accepted.

## 2. Current gates

| Gate | Status | Meaning |
|---|---|---|
| G0 — authority reconciled | In review; not accepted | Karam must explicitly accept or reject the reconciled authority, status, and source evidence. |
| G1 — evidence ready | Not started | Routes, actors, API, permissions, validation, errors, and state evidence must be captured or explicitly blocked. |
| G2 — shared contracts frozen | Not started | Tokens, theme, component, accessibility, NFR, fixture, and visual contracts must be approved. |
| G3 — inventory approved | Not started | Canonical route/page boundaries, actors, permissions, risk, and batches must be approved. |

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
| Authentication | Sign-in, account recovery, verification; anonymous users | Backend/auth architecture and legacy AUTH-001 notes | G0, current OpenAPI/error capture, legacy-board reconciliation | Candidate after G0 | No | No |
| Account | Current-user profile and security actions; authenticated users | `/me` architecture contract and backend roadmap | G0, routes/DTOs/error states | Candidate after G0 | No | No |
| Nurse | Nurse profile and professional data; nurses | Roadmap/backend modules | G0, route/permission/contract extraction | Candidate after G0 | No | No |
| Employer | Organization and candidate workflows; employers | Roadmap/backend modules | G0 and employer ownership evidence | Candidate after G0 | No | No |
| Recruitment | Search, contact-request, disclosure workflows; nurses/employers | Roadmap/backend modules | G0 and ownership/state evidence | Candidate after G0 | No | No |
| Examinations | Catalog, sessions, results, analytics; nurses/admin | Implemented backend and current architecture | G0, OpenAPI, timer/resume/rationale evidence | Candidate after G0 | No | No |
| Payments | Orders, checkout, outcomes; nurses/admin | Sandbox/payment backend baseline | G0, release/payment contract and exact permissions | Candidate after G0 | No | No |
| Administration | Administrative maintenance; admins | Roadmap plus implemented backend surfaces | G0, exact route/permission evidence | Candidate after G0 | No | No |
| Shared system | Shell, forbidden, not found, session expiry; all actors | Frontend architecture | G0, route registry and shared contracts | Candidate after G0 | No | No |
| Preparation Package | Package discovery, rights, practice, attempts, reports, administration; visitors/nurses/admins | Backend Stage 1–4 handoff baseline `8439511` | G0 and Phase 1 operation/DTO/permission/error capture | First proposed packet after G0 | No | No |

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

1. Finish review of this conflict-resolution task.
2. Karam explicitly accepts or rejects G0.
3. If G0 is accepted, authorize a bounded Preparation Package Phase 1 Evidence Packet.
4. Build the required evidence indexes.
5. Draft a candidate route/page registry.
6. Freeze shared foundation contracts.
7. Only then create page specifications.
8. Only after approved page specifications, authorize Penpot work.
9. Treat Angular scaffold approval as a separate later task.

## 9. Remaining open questions

This map does not close open questions. See `docs/frontend/design/governance/open-questions.md`, including `OPEN-G0-001`, `OPEN-PH1-001` through `OPEN-PH1-005`, `OPEN-001` through `OPEN-009`, and the unresolved `DISC-PEN-*` and `DISC-REP-001` records.
