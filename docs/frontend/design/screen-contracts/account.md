# Account Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-ACCOUNT
status: deep-extracted-family-contract
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Primary authority: `account-screen-approval-packet.md`, `system-design-contract.md`, auth/current-user/profile implementation evidence, backend/OpenAPI `/me` and profile update contracts, canonical routes and permission policy.

## ONB-001 Profile Onboarding

| Field | Contract |
|---|---|
| Identity | `ONB-001`; route `ONBOARDING_PROFILE`; `/onboarding/profile`; authenticated-only; routable. |
| Purpose | Capture minimum first/last name profile facts when onboarding is required. Non-goals: nurse/employer profile creation, account settings, status management. |
| Navigation | Entry from auth bootstrap/onboarding guard; success exits to Account/root approved destination. Cancel is not default unless current workflow authorizes skipping. |
| Form fields | `firstName` label `First name`, text, string, required, max 100; `lastName` label `Last name`, text, string, required, max 100. Server trimming authoritative. |
| Actions | `Save`: update profile contract; success hydrates current user/profile and navigates per owning auth flow. |
| States | Loading current user, ready, validation, saving, saved, backend validation/error. |
| Privacy | Do not show `isProfileComplete` internals, raw user ID, token/session state. |
| Status | `CONTRACT_READY`. |

## ACC-001 Account Overview

| Field | Contract |
|---|---|
| Identity | `ACC-001`; route `ACCOUNT_OVERVIEW`; `/account`; authenticated-only; routable. |
| Purpose | Show current actor-neutral account identity and personal details. |
| User-visible data | `email` label `Email`, string, required when backend supplies; `username` label `Username`, string; `firstName`/`lastName` labels `First name`/`Last name`, nullable/missing shown as `Not provided`; `emailVerified` may be shown as factual verification state if approved; roles may appear only as non-secret account facts when screen packet permits. |
| Navigation | Entry from shell account affordance and auth fallbacks; exits to same-route edit state, sign out, and actor areas through global nav. |
| Actions | `Edit personal details` opens `ACC-002`; `Sign out` belongs to shell/account affordance, not the account data contract. |
| States | Loading, ready, sparse/missing optional names, edit mode, save success, backend error, restricted via auth guard. |
| Forbidden | Password hashes, refresh/access tokens, permission keys, raw authorization payloads, raw user IDs, `isProfileComplete`, internal timestamps unless separately approved. |
| Status | `CONTRACT_READY`. |

## ACC-002 Personal Details Edit State

| Field | Contract |
|---|---|
| Identity | `ACC-002`; non-routable same-route state inside `/account`. |
| Purpose | Edit first and last name. |
| Form fields | `firstName`, text, string, required, max 100, default current first name; `lastName`, text, string, required, max 100, default current last name. |
| Actions | `Save`: profile update; success returns to overview with status. `Cancel`: discard local changes and return to overview. |
| States | Editing, validation, saving, saved, backend validation/error. |
| Status | `CONTRACT_READY`. |

## ACC-003 Change Password

| Field | Contract |
|---|---|
| Gap | No authenticated current-password change backend/OpenAPI contract exists. Anonymous reset-password does not authorize an account change-password screen. |
| Status | `BACKEND_BLOCKED`. |

## ACC-004 Security And Sessions

| Field | Contract |
|---|---|
| Gap | No stable account security/session/device-management backend contract exists. Refresh-token internals, JWT claims, generic `401/403`, and exam-session routes are not authority. |
| Status | `BACKEND_BLOCKED`. |

## ACC-005 Notification Preferences

| Field | Contract |
|---|---|
| Gap | No current-user notification preference backend/product contract exists. `DPF-001` Notifications remains design-proposed and does not authorize settings. |
| Status | `BACKEND_BLOCKED`. |

## ACC-006 Account Status

| Field | Contract |
|---|---|
| Gap | No account-status workflow contract exists beyond passive fields and generic inactive login/refresh rejection. Do not infer from admin-only user state. |
| Status | `BACKEND_BLOCKED`. |
