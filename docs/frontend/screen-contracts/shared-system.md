# Shared System Screen Contracts

## Family Authority

Primary authority: `system-screen-approval-packet.md`, `system-design-contract.md`, route/permission/navigation policies, DPF register, and Stitch artifact registry. Visual foundation remains `DESIGN.md`.

## App Shell

| Field | Contract |
|---|---|
| Identity | `APP-SHELL`; non-routable shared shell. |
| Purpose | Persistent application frame with top app bar, brand, actor-aware navigation, account affordance, sign-out affordance, main landmark, route loading. |
| Required | Skip link, brand/home affordance, primary navigation, account affordance, sign out, `main`, active route state. |
| Visual authority | The App Shell v3 visual chrome is human approved for the authenticated shell; its representative Exams content is not screen behavior authority. Earlier Nurse desktop shell remains a preferred historical direction, not the current approved shell baseline. |

## NAV-PRIMARY Global Navigation

| Field | Contract |
|---|---|
| Identity | `NAV-PRIMARY`; non-routable shared navigation pattern. |
| Data/source | Uses `navigation-permission-policy.ts`, route IDs, current user roles/permissions. |
| Allowed Nurse primary labels | Exams, Preparation Packages, Products, Profile. Account belongs to account affordance. |
| Forbidden | Non-routable states, DPF items, blocked screens, raw route IDs, icon-only primary navigation. |

## Account Affordance

| Field | Contract |
|---|---|
| Identity | `ACCOUNT-AFFORDANCE`; non-routable shell element. |
| Allowed actions | `Account`, `Sign out`, neutral account affordance. |
| Forbidden | Fake name, email, professional title, token/session status, verification badge, avatar representing invented person. |

## Root Entry

| Field | Contract |
|---|---|
| Identity | `ROOT`; route `ROOT_ENTRY`; `/`; entry route. |
| Boundary | HD-STITCH-04 approves a state/actor-driven root concept and rejects a fake universal dashboard. The canonical registry names `/`, but inspected `app.routes.ts` has no root route mapping. The exact redirect/content behavior is not established; do not imply an implemented redirect. |

## AUTH-010 Session Expired

| Field | Contract |
|---|---|
| Identity | `AUTH-010`; route `SYSTEM_SESSION_EXPIRED`; `/session-expired`; public terminal route. |
| Purpose | Explain explicit session-expired state and offer safe sign-in/account action. |
| Data | No backend data; no token details. |
| Actions | Sign in or safe account/root action per current auth flow. |

## AUTH-011 Access Denied

| Field | Contract |
|---|---|
| Identity | `AUTH-011`; route `SYSTEM_ACCESS_DENIED`; `/access-denied`; public terminal route for denied access. |
| Purpose | Privacy-safe restricted route state. |
| Data | No role/permission keys; no protected resource details. |
| Actions | Safe account/root action. |

## SYS-001 Route Loading

| Field | Contract |
|---|---|
| Identity | `SYS-001`; non-routable route/page loading. |
| Behavior | Composed route loading status; `role=status`; stable dimensions; hide on route end/cancel/error; no fake progress. |

## SYS-002 Not Found

| Field | Contract |
|---|---|
| Behavior | Unmatched route state; h1 Not found; preserve privacy; safe action only. Wildcard route is future, not a current canonical route. |

## SYS-003 Unexpected Error

| Field | Contract |
|---|---|
| Behavior | Unexpected retryable failure; contextual retry/safe action; no raw backend exception text. |

## SYS-004 Offline

| Field | Contract |
|---|---|
| Decision | Shared visual state may be designed factually per HD-STITCH-05, but offline/PWA/queued-write runtime behavior is deferred. |
| Forbidden | Offline storage, queued payment/exam actions, sync promises. |

## SYS-005 Maintenance

| Field | Contract |
|---|---|
| Decision | Shared factual visual state may be designed, but maintenance workflow/runtime authority is deferred. |
| Forbidden | Scheduled availability, SLA, service-health guarantees. |

## SYS-006 Empty

| Field | Contract |
|---|---|
| Behavior | Reusable absence state for no records in current owner/context. Optional CTA only when same-screen creation is authorized. |

## SYS-007 No Results

| Field | Contract |
|---|---|
| Behavior | Filter/search produced no matches; preserve filters; offer clear/reset where filters/search exist. |

## DPF-001 Notifications

| Field | Contract |
|---|---|
| Classification | `DESIGN_PROPOSED_FEATURE`; may appear in preferred shell baseline but is not implementation authority. |
| Missing | Product scope, backend event model, API, permissions/privacy/retention, frontend notification UI, release disposition. |

## DPF-002 Help Support Access

| Field | Contract |
|---|---|
| Classification | `DESIGN_PROPOSED_FEATURE`; may appear in preferred shell baseline but is not implementation authority. |
| Missing | Product support scope, content source, backend/API if dynamic, privacy/accessibility/testing, release disposition. |

## Unsupported Claims

Do not show System Online, Token Verified, HIPAA/GDPR/WCAG claims, service-health claims, clinical-system status, debug annotations, route IDs, token/session claims, security/compliance footer claims, or fake user identity without separate authority.
