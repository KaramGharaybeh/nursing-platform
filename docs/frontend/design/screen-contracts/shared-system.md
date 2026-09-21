# Shared System Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-SYSTEM
status: deep-extracted-family-contract
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Primary authority: `system-screen-approval-packet.md`, `system-design-contract.md`, route/permission/navigation policies, DPF register, and Stitch artifact registry. Visual foundation remains `DESIGN.md`.

## App Shell

| Field | Contract |
|---|---|
| Identity | `APP-SHELL`; non-routable shared shell. |
| Purpose | Persistent application frame with top app bar, brand, actor-aware navigation, account affordance, sign-out affordance, main landmark, route loading. |
| Required | Skip link, brand/home affordance, primary navigation, account affordance, sign out, `main`, active route state. |
| Gap | Preferred Nurse desktop shell is a visual baseline, not exact human approval. Final production shell still requires human visual approval and Angular implementation task. |
| Status | `PARTIAL`. |

## NAV-PRIMARY Global Navigation

| Field | Contract |
|---|---|
| Identity | `NAV-PRIMARY`; non-routable shared navigation pattern. |
| Data/source | Uses `navigation-permission-policy.ts`, route IDs, current user roles/permissions. |
| Allowed Nurse primary labels | Exams, Preparation Packages, Products, Profile. Account belongs to account affordance. |
| Forbidden | Non-routable states, DPF items, blocked screens, raw route IDs, icon-only primary navigation. |
| Status | `PARTIAL`. |

## Account Affordance

| Field | Contract |
|---|---|
| Identity | `ACCOUNT-AFFORDANCE`; non-routable shell element. |
| Allowed actions | `Account`, `Sign out`, neutral account affordance. |
| Forbidden | Fake name, email, professional title, token/session status, verification badge, avatar representing invented person. |
| Status | `PARTIAL`. |

## Root Entry

| Field | Contract |
|---|---|
| Identity | `ROOT`; route `ROOT_ENTRY`; `/`; entry route. |
| Gap | State/actor-driven root behavior is approved conceptually, but final exact routing/content behavior is unresolved. No fake universal dashboard. |
| Status | `AUTHORITY_GAP`. |

## AUTH-010 Session Expired

| Field | Contract |
|---|---|
| Identity | `AUTH-010`; route `SYSTEM_SESSION_EXPIRED`; `/session-expired`; public terminal route. |
| Purpose | Explain explicit session-expired state and offer safe sign-in/account action. |
| Data | No backend data; no token details. |
| Actions | Sign in or safe account/root action per current auth flow. |
| Status | `CONTRACT_READY`. |

## AUTH-011 Access Denied

| Field | Contract |
|---|---|
| Identity | `AUTH-011`; route `SYSTEM_ACCESS_DENIED`; `/access-denied`; public terminal route for denied access. |
| Purpose | Privacy-safe restricted route state. |
| Data | No role/permission keys; no protected resource details. |
| Actions | Safe account/root action. |
| Status | `CONTRACT_READY`. |

## SYS-001 Route Loading

| Field | Contract |
|---|---|
| Identity | `SYS-001`; non-routable route/page loading. |
| Behavior | Composed route loading status; `role=status`; stable dimensions; hide on route end/cancel/error; no fake progress. |
| Status | `CONTRACT_READY`. |

## SYS-002 Not Found

| Field | Contract |
|---|---|
| Behavior | Unmatched route state; h1 Not found; preserve privacy; safe action only. Wildcard route is future, not a current canonical route. |
| Status | `CONTRACT_READY`. |

## SYS-003 Unexpected Error

| Field | Contract |
|---|---|
| Behavior | Unexpected retryable failure; contextual retry/safe action; no raw backend exception text. |
| Status | `CONTRACT_READY`. |

## SYS-004 Offline

| Field | Contract |
|---|---|
| Decision | Shared visual state may be designed factually per HD-STITCH-05, but offline/PWA/queued-write runtime behavior is deferred. |
| Forbidden | Offline storage, queued payment/exam actions, sync promises. |
| Status | `DEFERRED`. |

## SYS-005 Maintenance

| Field | Contract |
|---|---|
| Decision | Shared factual visual state may be designed, but maintenance workflow/runtime authority is deferred. |
| Forbidden | Scheduled availability, SLA, service-health guarantees. |
| Status | `DEFERRED`. |

## SYS-006 Empty

| Field | Contract |
|---|---|
| Behavior | Reusable absence state for no records in current owner/context. Optional CTA only when same-screen creation is authorized. |
| Status | `CONTRACT_READY`. |

## SYS-007 No Results

| Field | Contract |
|---|---|
| Behavior | Filter/search produced no matches; preserve filters; offer clear/reset where filters/search exist. |
| Status | `CONTRACT_READY`. |

## DPF-001 Notifications

| Field | Contract |
|---|---|
| Classification | `DESIGN_PROPOSED_FEATURE`; may appear in preferred shell baseline but is not implementation authority. |
| Missing | Product scope, backend event model, API, permissions/privacy/retention, frontend notification UI, release disposition. |
| Status | `DEFERRED`. |

## DPF-002 Help Support Access

| Field | Contract |
|---|---|
| Classification | `DESIGN_PROPOSED_FEATURE`; may appear in preferred shell baseline but is not implementation authority. |
| Missing | Product support scope, content source, backend/API if dynamic, privacy/accessibility/testing, release disposition. |
| Status | `DEFERRED`. |

## Unsupported Claims

Do not show System Online, Token Verified, HIPAA/GDPR/WCAG claims, service-health claims, clinical-system status, debug annotations, route IDs, token/session claims, security/compliance footer claims, or fake user identity without separate authority.
