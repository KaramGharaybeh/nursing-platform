# Shared System Screen Approval Packet — T-FE-113 / GATE-FE-T113

```yaml
document_id: NPS-DES-INV-SYSTEM-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-16
prepared_by: T-FE-113 campaign
gate: GATE-FE-T113
gate_status: VERIFIED (closed by human-approved decisions HD-SYS1–HD-SYS4 below)
authorization: Decisions HD-SYS1–HD-SYS4 were approved
  by the human technical lead on 2026-09-16. SYS-001/002/003/006/007 and the
  restricted pattern are APPROVED as design authority; SYS-004/005 stay
  DEFERRED pending runtime authority (T-FE-116/117). Approval does not itself
  start implementation; owning tasks (T-FE-028, T-FE-035) remain NOT STARTED
  until separately authorized.
```

## 1. Purpose

This is the T-FE-113 Shared System screen approval packet covering SYS-001..007 plus the
permission-restricted presentation. It establishes the family semantics, per-state backend
contracts, required distinctions, and the explicit design decisions. Per the frontend ledger,
GATE-FE-T113 requires "System screen approval packet with decisions per screen" — the
decisions are collected in Section 5 and were answered by the technical lead before any
T-FE-035/T-FE-028 implementation may start.

## 2. Scope owned by this packet

| Screen | Owner | Route | Status | Approval |
|---|---|---|---|---|
| SYS-001 Loading route state | `T-FE-028` | NOT_ROUTABLE | NOT STARTED | APPROVED (compose verified `T-FE-033` pattern; no loading route) |
| SYS-002 Not found presentation | `T-FE-031` family | NOT_ROUTABLE | contract exists, no component | APPROVED (presentation contract ratified; wildcard wiring later) |
| SYS-003 Unexpected error presentation | `T-FE-031` family | NOT_ROUTABLE | contract exists | APPROVED (compose verified `T-FE-033` error presentation) |
| SYS-004 Offline | `T-FE-116` | DEFERRED | needs runtime authority | DEFERRED (not solvable here) |
| SYS-005 Maintenance | `T-FE-117` | DEFERRED | needs runtime authority | DEFERRED (not solvable here) |
| SYS-006 Empty state | `T-FE-035` | NOT_ROUTABLE | NOT STARTED | APPROVED (HD-SYS1 semantics; implement in `T-FE-035`) |
| SYS-007 No search results | `T-FE-035` | NOT_ROUTABLE | NOT STARTED | APPROVED (HD-SYS1 semantics; implement in `T-FE-035`) |
| Restricted (access-denied) | `T-FE-031` policy | `/access-denied` route exists | SHIPPED + VERIFIED | APPROVED (HD-SYS3; existing screen is the pattern) |

Out of scope for this packet: pagination/filter mechanics (owned by `T-FE-038`),
validation display (`T-FE-034`), loading/error/retry foundation (`T-FE-033`,
referenced not redefined), guards (`T-FE-030`), Announcer/live-region (`T-FE-036`),
global shell, and all product screens.

## 3. Existing implementations reused (evidence, not new design)

- Loading/error/retry: `frontend/src/app/shared/ui/loading-error-retry.*` (`T-FE-033`
  VERIFIED) — explicit loading text with `role="status"`, error with `role="alert"`,
  explicit retry output, no automatic retry.
- Restricted: `frontend/src/app/features/auth/access-denied/access-denied.*`
  (`T-FE-053`/`GATE-FE-T053` VERIFIED) — generic "Access denied" heading, "Go to
  account" safe action, no permission internals exposed.
- Empty states: seven Nurse Profile sections + CV + Skills/Languages empty cards
  (e.g. "No positions added yet.", "No CV uploaded yet.") — calm heading + factual
  copy + optional same-page CTA, verified across T-FE-056..064 with E2E.
- Empty vs no-results distinction: `admin-users.html` already renders "No users are
  available yet." (empty, no query) vs "No users match your search." (query present,
  context preserved) — the exact SYS-006/SYS-007 split in production code.
- Contextual stale handling: Experience/Education/Certificates/CV delete and update
  flows render calm inline "no longer exists / already removed" notices with list
  refresh — local handling, no global 404 page involved.

## 4. Semantic boundaries (binding)

- EMPTY (SYS-006): the collection genuinely has zero records. May carry a primary
  CTA to create first data on the same page. Must not imply records exist.
- NO RESULTS (SYS-007): records may exist, but the current search/filter yields zero
  matches. Must preserve query/filter context, must offer Clear filters / Reset, must
  NOT claim no records exist globally, must NOT offer create-data CTA as primary.
- RESTRICTED: authenticated user lacks role/permission. Generic privacy-safe copy
  only; safe navigation action; no role/permission keys, no status-code jargon.
- NOT FOUND (SYS-002): unmatched route (future wildcard, URL preserved, no
  redirect) or stale reference. Inline contextual "no longer exists" handling stays
  local to owning features; it is not replaced by a global screen.
- ERROR (SYS-003): unexpected failure where retry may succeed. Compose the verified
  `T-FE-033` presentation; no independently navigable V1 destination.

## 5. HUMAN APPROVED DECISIONS — Shared System states (recorded 2026-09-16)

**HD-SYS1 — Empty vs no-results distinction. APPROVED.** EMPTY and NO RESULTS remain
distinct semantic states per Section 4. `T-FE-035` implements both; lists with
search/filter must select the state from query presence exactly as the admin-users
precedent does (blank query + zero rows = empty; non-blank query + zero rows =
no-results with context preserved and reset offered).

**HD-SYS2 — Illustrations/icons. APPROVED.** Shared states do NOT require decorative
illustrations or icons; they are optional presentation, never semantic requirements.
No shipped state uses artwork; none is mandated.

**HD-SYS3 — Restricted copy. APPROVED.** Restricted/access-denied copy remains generic
and privacy-safe ("Access denied" + safe navigation such as "Go to account"). Do not
expose internal roles, permission keys, or status-code terminology. The shipped
access-denied screen satisfies this and is the pattern.

**HD-SYS4 — Stale/deleted-resource handling. APPROVED.** Existing contextual
stale/deleted-resource handling remains local to owning features (calm inline notice
+ refresh). Do NOT force every missing resource into a global 404 screen. The future
wildcard may render SYS-002 for unmatched routes only, preserving the URL.

## 6. What T-FE-035 needs from this packet

`T-FE-035` (deps `GATE-FE-T031` VERIFIED + `GATE-FE-T113` VERIFIED after this closure)
is now eligible. It implements the SYS-006/SYS-007 reusable pattern using HD-SYS1
semantics, HD-SYS2 presentation freedom, and existing evidence above. Its gate
(`GATE-FE-T035`) expects empty/restricted tests + per-screen visual evidence — the
restricted half is already proven by the shipped access-denied screen; the empty and
no-results halves are greenfield pattern work.

## 7. Relationships explicitly not owned here

- `T-FE-038` owns list/filter/pagination mechanics, server-pagination behavior, and
  its own tests/visual evidence; it consumes SYS-006/007 for list states and
  `T-FE-033` for loading/error. This packet approves no pagination behavior.
- Future Nurse Contact Requests (`T-FE-096`) needs: no-requests (SYS-006), no
  filter matches (SYS-007 with filter preserved + clear), permission denial (guard
  → access-denied, HD-SYS3), load failure (SYS-003 via `T-FE-033`). All are covered
  by the semantics above; no conflict and no scope expansion results.
- SYS-004/005 stay DEFERRED to `T-FE-116`/`T-FE-117` runtime authority.

## 8. Responsive / RTL / accessibility (cross-cutting, foundations reused)

Shared states follow established breakpoints with canonical gutters, logical
properties only, no horizontal overflow, correct heading order, keyboard-reachable
actions, `role="status"`/`role="alert"` semantics where the composed patterns
already provide them, and no color-only distinction. No new foundation is created.

## 9. HUMAN APPROVED DECISIONS — Shared pagination pattern (recorded 2026-09-16)

**HD-G1 — Pagination UI. APPROVED.** Use the established Previous / Page X of Y ·
N total / Next pattern inside `<nav>` with an accessible pagination label and native
buttons. Backend/page contract remains 1-based. No numbered-page buttons, ellipsis,
MatPaginator, jump-to-page, or infinite scroll.

**HD-G2 — Reload composition. APPROVED.** Page change, filter/search submission, and
filter reset use the existing full loading-state swap (`T-FE-033` / Admin Users
precedent); stale content is not retained behind an overlay. When `totalPages <= 1`,
pagination navigation does not render (including zero-result and single-page sets).

**HD-G3 — Page size. APPROVED.** Page-size value is consumer-owned; `T-FE-038`
provides no shared selector and invents no 10/25/50 options. Each product contract
chooses its own fixed/default pageSize within its backend cap.

**HD-G4 — URL query state. APPROVED.** `T-FE-038` does not synchronize page, filters,
search, or pageSize to URL query parameters and introduces no router coupling.
URL synchronization is consumer-specific future scope only if explicitly authorized.

## 10. Approval record

- 2026-09-16: Human technical lead approved HD-SYS1–HD-SYS4 for the Shared System
  screen family. SYS-001/002/003/006/007 and the restricted pattern are APPROVED;
  SYS-004/005 stay DEFERRED. `T-FE-035` is thereby unblocked on the design axis
  (its `GATE-FE-T031` dependency was already VERIFIED). Owning implementation tasks
  (`T-FE-028`, `T-FE-035`) remain NOT STARTED pending separate authorization.
- GATE-FE-T113 is closed as VERIFIED on the strength of: every SYS screen having an
  explicit decision, ratification (not reinvention) of verified foundations, and the
  recorded per-screen states/dependencies — consistent with the T-FE-040/T-FE-052
  family-gate precedent.
