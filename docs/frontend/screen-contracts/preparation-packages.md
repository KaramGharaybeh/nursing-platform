# Preparation Package Screen Contracts

## Family Authority

Primary authority: `preparation-package-screen-approval-packet.md`, `system-design-contract.md`, current Preparation Package backend/OpenAPI evidence, canonical routes, and verified frontend implementation evidence. Public offers are public; owned package screens require Nurse role.

The approved packet establishes bounded offer, entitlement, exam-entry, and report presentation decisions. Practice has later owning authority; Product material access remains approved while a learner reader/delivery screen contract is blocked pending its own technical authority.

## PP-001 Package Offers

| Field | Contract |
|---|---|
| Identity | Route `PREPARATION_PACKAGES_OFFERS`; `/preparation-packages`; public. |
| Purpose | Browse active eligible package offers. |
| List/card data | Offer title, package definition title, exam title/category/country facts, safe price/metadata only when backend supplies. |
| Filters/pagination | Backend-supported filters/page only; empty/no-results distinct. |
| Actions | View detail. No purchase/order behavior in this screen. |

## PP-002 Package Offer Detail

| Field | Contract |
|---|---|
| Identity | `/preparation-packages/:offerSlug`; route param `offerSlug`. |
| Purpose | Understand public package offer facts. |
| Data | Backend offer detail fields only: package/exam/category/country, included benefits/content summaries, access/attempt/report facts only when supplied. |
| Actions | Back to offers; commerce/purchase only when separately approved. |
| Forbidden | Invented benefits, material delivery, offline access, guarantees, payment/order behavior. |

## PP-003 My Preparation Packages

| Field | Contract |
|---|---|
| Identity | `/nurse/preparation-packages`; Nurse role. |
| Purpose | View owned package entitlements. |
| List/card data | Package identity, exam title, entitlement status, access window dates, rights availability, attempt/report indicators from backend. |
| Pagination | Backend page only. Empty means current nurse owns no entitlements. |
| Actions | View entitlement detail. |
| Forbidden | Price/currency/order/provider internals. |

## PP-004 Entitlement Detail

| Field | Contract |
|---|---|
| Identity | `/nurse/preparation-packages/:entitlementId`; route param internal; Nurse role. |
| Purpose | View owned entitlement facts and eligible package actions. |
| Data | Entitlement status, access window, rights, purchase snapshot safe titles/dates, exam attempt/report availability, practice access facts. |
| Actions | Practice -> `PP-005`; package exam start/resume -> `PP-EXAM-START`; view report -> `PP-007` when backend permits; back to entitlements. |
| States | Ready, not found/ownership-hidden, expired/no actions, loading, error. |

## PP-005 Practice

| Field | Contract |
|---|---|
| Identity | `/nurse/preparation-packages/:entitlementId/practice`; Nurse role. |
| Purpose | Practice package content and progress while preserving exam-content isolation. |
| Data/list | Practice progress and collection facts returned by backend; answered/correct/incorrect/unanswered counts only when supplied; practice item content must be independent from official exam protected content. |
| Actions | Answer/re-answer practice items while active if backend allows; back to entitlement. |
| States | Loading, empty, progress states, validation/error, expired write rejected but historical read where backend permits. |
| Forbidden in this practice screen | Material reader/download, official exam answer keys, rationales, adaptive/spaced repetition claims. Approved Product material access is owned separately by the package benefit. |

## PP-EXAM-START Package Exam Start

| Field | Contract |
|---|---|
| Identity | Non-routable action from entitlement detail to shared `EXAMS_SESSION`. |
| Purpose | Start/resume package-scoped exam attempt using explicit package purchase entitlement. |
| Confirmation | Required. Copy must state single package attempt consumption where applicable; safe action Cancel; confirm action Start/Resume. |
| Backend authority | Package start endpoint/session authorization; conflicts reconcile with backend truth. |

## PP-007 Package Report

| Field | Contract |
|---|---|
| Identity | `/nurse/preparation-packages/reports/:sessionId`; Nurse role. |
| Purpose | View package-specific post-exam analytical report. |
| Report sections/order | Summary; per-topic report; recommended package content. |
| Metrics/data | Correct/question/percentage summary; topic counts/percentage; recommended content title/type from package-included content only. |
| Source/privacy | Backend report endpoint; must not reveal exam question text, answers, answer keys, rationales, provenance/payment internals. |
| States | Ready, unavailable, unfinished 409, no recommendations, error. |

## PP-MATERIAL-READER

| Field | Contract |
|---|---|
| Gap | [Product requirements](../../product/requirements.md) approve learner material access. The inspected screen contract and current HTTP/Angular implementation do not yet establish a complete learner material reader/download interaction. Admin material authoring endpoints do not define that learner UI. |
