# Preparation Package Screen Approval Packet — T-FE-070 / GATE-FE-T070

```yaml
document_id: NPS-DES-INV-PP-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-16
prepared_by: T-FE-070 campaign
gate: GATE-FE-T070
gate_status: VERIFIED (closed by human-approved decisions HD-PP1–HD-PP5 below)
authorization: Decisions HD-PP1–HD-PP5 were approved
  by the human technical lead on 2026-09-16. Entitlement list/detail
  (T-FE-076 scope) are APPROVED as design authority; offers list/detail,
  practice, exam-start, and report remain owned by T-FE-075/078/079 with only
  boundary decisions recorded here. Approval does not itself start
  implementation; T-FE-076 remains NOT STARTED until separately authorized.
```

## 1. Purpose

This is the T-FE-070 Preparation Package screen approval packet covering the
nurse-owned PP surfaces (offers browsing, entitlements, practice, exam/report).
It records per-screen backend contracts, the approved presentation authority for
the immediately-buildable scope (entitlement list/detail, T-FE-076), and explicit
boundaries for later-owned actions. Per the frontend ledger, GATE-FE-T070 requires
"PP approval packet with decisions per screen".

## 2. Screen inventory

| Screen | Route | Owner | Backend contract | Status | Approval |
|---|---|---|---|---|---|
| PP offers list | `/preparation-packages` (APPROVED_CANONICAL, public) | `T-FE-075` | `GET /preparation-packages/offers` (page/pageSize/countryId/examCategoryId), `PaginatedResult<PreparationPackageOfferListItemDto>` | NOT STARTED | APPROVED (browse-only; purchase CTA deferred to commerce §8) |
| PP offer detail | `/preparation-packages/:offerSlug` (APPROVED_CANONICAL, public) | `T-FE-075` | `GET /offers/{slug}` → detail DTO, 404 when absent | NOT STARTED | APPROVED (metadata only; no purchase flow in this packet) |
| PP entitlements list | `/nurse/preparation-packages` (APPROVED_CANONICAL, ROLE Nurse) | `T-FE-076` | `GET /me/nurse-profile/preparation-packages/entitlements` (page/pageSize only), `PaginatedResult<PackageEntitlementListItemDto>` | NOT STARTED | APPROVED (HD-PP1–PP5) |
| PP entitlement detail | `/nurse/preparation-packages/:entitlementId` (APPROVED_CANONICAL, ROLE Nurse) | `T-FE-076` | `GET /entitlements/{id}` → detail DTO (+ purchase snapshot), 404 when absent/foreign | NOT STARTED | APPROVED (information only; actions stay later-owned) |
| PP practice | `/nurse/preparation-packages/:entitlementId/practice` (APPROVED_CANONICAL, ROLE Nurse) | `T-FE-078` | `GET/POST practice-progress…` | NOT STARTED | BLOCKED (design deferred to owning gate) |
| PP exam start | NOT_ROUTABLE (action in entitlement context) | `T-FE-079` | `POST /entitlements/{id}/exam-session` (200/404/409) | NOT STARTED | BLOCKED (design deferred to owning gate) |
| PP report | `/nurse/preparation-packages/reports/:sessionId` (APPROVED_CANONICAL, ROLE Nurse) | `T-FE-079` | `GET /exam-sessions/{sessionId}/report` (200/404/409) | NOT STARTED | BLOCKED (design deferred to owning gate) |
| Material reader | — (no contract) | `T-FE-080` | none (backend gap, VERIFIED classification) | NOT STARTED | BLOCKED (no learner delivery contract) |

## 3. Entitlement contract (verified from source, T-FE-076 scope)

List: `page`/`pageSize` only (defaults 1/20, cap 100 via shared validator shape),
1-based, `OrderByDescending(AccessStartsAt)` + Id tiebreak, `TotalCount`/`TotalPages`
server-computed, `NurseRoleGuard` + profile ownership, missing profile → 403.
**No filter, search, or sort parameters exist** — list controls are pagination only.
Detail: single id lookup, same ownership, 404 when absent/foreign.
DTO (`PackageEntitlementListItemDto` + detail `PurchasedSnapshot`): display —
`PackageOfferTitle`, `PackageDefinitionTitle`, `IncludedExamTitle`, `AccessStartsAt`,
`AccessEndsAt`, `Status` (Active/Expired/Revoked, stored enum serialized verbatim),
per-right `{RightType, Status, IsAvailable, IsDormant, AccessStartsAt, AccessEndsAt?}`
(server-computed, display verbatim); hide — all Guid ids, `PriceAmountMinor`,
`Currency`, snapshot country/category/version ids, material version ids.
Generated clients exist (`list-my-package-entitlements`, `get-my-package-entitlement`).

## 4. HUMAN APPROVED DECISIONS (recorded 2026-09-16)

**HD-PP1 — Entitlement list/detail content. APPROVED.** List prioritizes package
identity (offer/definition/exam titles) + entitlement status/validity facts
(access window, per-right availability). Detail adds the same facts plus purchase
snapshot dates as information. No commerce actions (price/purchase/checkout),
no practice actions, no exam/report actions unless owned by the implementing task —
`T-FE-076` owns information display only.

**HD-PP2 — Status truth. APPROVED.** Use backend-provided status truth (`Status`,
`IsAvailable`, `IsDormant`) verbatim. Do not derive custom frontend
completion/urgency/validity scores. Plain-text status with factual access-window
dates; no semantic urgency colors.

**HD-PP3 — Filters/sorts. APPROVED.** Limited strictly to contract-supported values:
entitlement list has neither filters nor sorts, so it ships with pagination only.
No speculative search box, status filter, or sort select. (Offers list filters stay
with `T-FE-075` under its own contract: countryId/examCategoryId.)

**HD-PP4 — Empty state. APPROVED.** Empty entitlement state is factual and calm
("no preparation packages / entitlements yet" diction, `NpEmptyState` empty kind).
No purchase CTA linking to public offers — no cross-screen authority permits it.

**HD-PP5 — Internal hiding. APPROVED.** Do not expose raw Guid ids, order/payment
internals (price, currency, snapshot reference ids), or storage keys in entitlement
UI. Titles, dates, statuses, and availability facts are the display surface.

## 5. Boundaries explicitly not owned here

- Practice (T-FE-078): entitlement detail shows NO practice CTA until T-FE-078 ships
  (no dead buttons; benefit-right availability facts may render as information).
- Exam start/report (T-FE-079): no start action, no report link/summary on detail
  until T-FE-079 ships. Exam-start stays NOT_ROUTABLE as an entitlement-context action.
- Commerce (T-FE-077/082/084): price display, purchase CTA, and checkout handoff live
  entirely under commerce tasks. Offers screens show catalog metadata only in this packet.
- Material reader stays BLOCKED on the verified backend gap (T-FE-080).

## 6. Approved states and composition

List: loading / populated / empty / error+retry via `T-FE-033` + `NpEmptyState`
(empty kind only — no filter exists to produce no-results) + `NpPagination`
(fixed pageSize 20, page-1 discipline n/a without filters, retry preserves page,
hidden when ≤1 page). Detail: loading / loaded / 404-not-found / error+retry.
Dates absolute (`Intl` calendar formatting precedent; access instants via `date`
medium like CV `uploadedAt`). Responsive 1440/768/390, logical properties,
semantic headings, wrapping long titles, keyboard-safe links/buttons — existing
foundations only.

## 7. What T-FE-076 needs from this packet

`T-FE-076` (deps `GATE-FE-T021..T025` VERIFIED + `GATE-FE-T038` VERIFIED +
`GATE-FE-T070` VERIFIED after this closure) is now eligible. It builds the
entitlement list/detail per HD-PP1–PP5 within the contracts above. Its gate
(`GATE-FE-T076`) expects entitlement tests + visual evidence. `T-FE-075` is
likewise unblocked on the packet axis (its T018–020 + T038 deps already VERIFIED).

## 8. Approval record

- 2026-09-16: Human technical lead approved HD-PP1–HD-PP5 for the Preparation
  Package screen family. Entitlement list/detail and offers browse metadata are
  APPROVED; practice/exam-start/report stay BLOCKED to owning gates; material
  reader stays BLOCKED on the backend gap. `T-FE-076` (and packet-wise `T-FE-075`)
  are thereby unblocked. `T-FE-076` remains NOT STARTED pending separate authorization.
- GATE-FE-T070 is closed as VERIFIED on the strength of: every PP screen having an
  explicit decision, contracts verified from source (not inferred), and boundaries
  pinned to owning tasks — consistent with the T-FE-040/T-FE-052/T-FE-113/T-FE-092
  family-gate precedent.
