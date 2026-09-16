# Account Screen Approval Packet — T-FE-092 / GATE-FE-T092

```yaml
document_id: NPS-DES-INV-ACCOUNT-SCREEN-APPROVAL-PACKET
status: HUMAN_APPROVED
created_at: 2026-09-16
prepared_by: T-FE-092 campaign
gate: GATE-FE-T092
gate_status: VERIFIED (closed by human-approved decisions HD-A1–HD-A5 below)
authorization: Decisions HD-A1–HD-A5 were approved
  by the human technical lead on 2026-09-16. ACC-001/002 are APPROVED as design
  authority; ACC-003..006 stay BLOCKED on backend gaps (T-FE-099..102 own those
  classifications). Approval does not itself start implementation; T-FE-097
  remains NOT STARTED until separately authorized.
```

## 1. Purpose

This is the T-FE-092 Account screen approval packet covering ACC-001..006. It records
per-screen backend contracts, the approved presentation authority for the
immediately-buildable scope (ACC-001/002), explicit non-scope for later self-service
screens, and the decisions deferred to owning gates. Per the frontend ledger,
GATE-FE-T092 requires "Account approval packet with decisions per screen".

## 2. Screen inventory (ACC-001..006)

| Screen | Route | Owner | Backend contract | Status | Approval |
|---|---|---|---|---|---|
| ACC-001 Account Overview | `/account` (APPROVED_CANONICAL, AUTHENTICATED_ONLY) | `T-FE-097` | `C-ME` (`GET /api/v1/me` + `CurrentUserStore`) | placeholder shell shipped | APPROVED (identity self-service; HD-A1) |
| ACC-002 Personal Details | NOT_ROUTABLE (owned inside `/account`) | `T-FE-097` | `C-ME` (`PUT /api/v1/me/profile` `{firstName, lastName}`) | NOT STARTED | APPROVED (same-route edit state; HD-A2) |
| ACC-003 Change password | — | `T-FE-099` | none found (backend gap) | NOT STARTED | BLOCKED (no contract) |
| ACC-004 Security/sessions | — | `T-FE-100` | none found (backend gap) | NOT STARTED | BLOCKED (no contract) |
| ACC-005 Notification preferences | — | `T-FE-101` | none found (backend gap) | NOT STARTED | BLOCKED (no contract) |
| ACC-006 Account status | — | `T-FE-102` | none found (backend gap) | NOT STARTED | BLOCKED (no contract) |

## 3. Current `/account` reality (evidence, not new design)

Route `ACCOUNT_OVERVIEW` is mounted with the three-guard pattern and
`AUTHENTICATED_ONLY` policy (actor-neutral: Nurse/Employer/Expert/Admin share it).
The shipped component is an intentional placeholder shell ("Account overview" card
only, from the QA-AUTH-001 sign-in fallback remediation). `T-FE-097` replaces/extends
this shell with ACC-001/002; no child routes exist and none are approved.

## 4. Account data contract (actor-neutral, verified from source)

`GET /api/v1/me` → `{id, email, username, firstName, lastName, isProfileComplete,
isActive, emailVerified, roles[], permissions[], createdAt, lastLoginAt?}`.
`PUT /api/v1/me/profile` accepts exactly `{firstName, lastName}` (both required,
≤100, trimmed server-side; response echoes username/names/`isProfileComplete`).
`POST /api/v1/auth/send-verification-email` exists (authenticated) but resend UX is
not in Account scope. Generated clients exist for all three (`get-current-user`,
`update-current-user-profile`, `send-verification-email`); `CurrentUserStore`
hydration is VERIFIED (`T-FE-138`).

Field classification: display — firstName, lastName, username, email,
emailVerified state; editable today — firstName, lastName only; security-sensitive —
none displayed beyond what `/me` already exposes to its owner; internal/not useful —
id, permissions[], createdAt/lastLoginAt (no display authority), `isProfileComplete`
(auth/onboarding property, never user-facing per NUR-013 closure).

## 5. HUMAN APPROVED DECISIONS (recorded 2026-09-16)

**HD-A1 — Overview purpose. APPROVED.** ACC-001 remains account/identity
self-service (identity summary + entry to Personal Details editing + only approved
Account actions). It is not a generic dashboard, navigation hub, or role home; no
dashboard model exists in authority.

**HD-A2 — Personal Details placement. APPROVED.** ACC-002 stays inside the canonical
`/account` route as a same-route view/edit state (view → edit → client validation →
submitting → success/failure → cancel). No `/account/personal-details` route exists
and none is approved.

**HD-A3 — Privacy boundary. APPROVED.** Do not expose raw permission keys, internal
user IDs, or generic `isProfileComplete` as user-facing account status. Roles are
namespaced facts about the account, not secrets, but no authority requires showing
them — default is identity-first display; role display needs no prohibition and no
mandate. `isProfileComplete` display is prohibited (auth/onboarding internals;
NUR-013 closure stands).

**HD-A4 — Self-service boundary. APPROVED.** No username/email/password editing,
account deletion, or sessions/devices management without explicit contracts. All are
absent today (T-FE-099..102 own the gap classifications). ACC-002 edits firstName +
lastName only; changing names flows through to `isProfileComplete` by existing
domain logic without any UI statement about completion.

**HD-A5 — Contact Requests entry. APPROVED AS DEFERRED.** A Nurse Contact Requests
entry may be added to Account ONLY if Account authority explicitly supports
actor-specific self-service navigation. It does not: `ACCOUNT_OVERVIEW` authority
says "Current-user account entry from `/me` contract" with no nav-hub language, and
no shell/dashboard/navigation authority exists. The condition is therefore unmet and
HD-R4 deferral stands — Contact Requests stays deep-link-only. Reopening needs
future account/navigation product authority, not a T-FE-097 stretch.

## 6. Approved states and composition

ACC-001: loading / loaded / error+retry via `T-FE-033`; no Empty/NoResults
pagination semantics needed. ACC-002: view / edit / client validation / submitting /
success / backend failure / cancel via `T-FE-034` + Announcer; no unsaved-changes
guard precedent exists — do not invent one. Email-verified state may be displayed as
fact; resend action is out of scope (display ≠ action). Responsive 1440/768/390,
logical properties, proper headings, labeled form controls, keyboard-safe actions —
existing foundations only.

## 7. What T-FE-097 needs from this packet

`T-FE-097` (deps `GATE-FE-T138` VERIFIED + `GATE-FE-T033` VERIFIED + `GATE-FE-T092`
VERIFIED after this closure) is now eligible. It builds ACC-001/002 into the mounted
`/account` shell per HD-A1/A2 within `C-ME` only. Its gate (`GATE-FE-T097`) expects
account overview tests + per-screen visual evidence.

## 8. Relationships explicitly not owned here

- T-FE-099..102 own ACC-003..006 gap classifications; this packet approves no
  password/session/notification/status behavior.
- Contact Requests navigation stays deferred (HD-A5/HD-R4); `T-FE-096` is untouched.
- No onboarding redesign (`isProfileComplete` semantics preserved), no NUR-013
  reopening, no permission-policy changes.

## 9. Approval record

- 2026-09-16: Human technical lead approved HD-A1–HD-A5 for the Account screen
  family. ACC-001/002 are APPROVED; ACC-003..006 stay BLOCKED on backend gaps.
  `T-FE-097` is thereby unblocked (its `GATE-FE-T138`/`GATE-FE-T033` dependencies
  were already VERIFIED). `T-FE-097` remains NOT STARTED pending separate
  authorization.
- GATE-FE-T092 is closed as VERIFIED on the strength of: every ACC screen having an
  explicit decision, contracts verified from source (not inferred), and self-service
  boundaries pinned to existing backend gaps — consistent with the
  T-FE-040/T-FE-052/T-FE-113 family-gate precedent.
