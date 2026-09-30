# Authentication Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-AUTH
status: deep-extracted-family-contract
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Primary authority: `system-design-contract.md` Sections 6-9, 16, V1 self-registration/auth evidence in `PROGRESS.md`, canonical routes, route classification, backend/OpenAPI auth contracts, and current verified Auth implementation evidence. Public route status does not authorize arbitrary session or account behavior.

Shared rules: `shared-patterns.md`; visual foundation: `docs/frontend/design/stitch/DESIGN.md`.

## AUTH-001 Sign In

| Field | Contract |
|---|---|
| Identity | Screen ID `AUTH-001`; canonical name `Sign in`; family Authentication; route `AUTH_SIGN_IN`; path `/auth/sign-in`; public; routable. |
| Purpose | Authenticate returning users with email/username and password. Non-goals: registration, role selection, token inspection, account diagnostics. |
| Navigation | Entry from anonymous shell, auth redirects, expired/session flows, and protected-route `returnUrl`; exits to safe return URL, account/root, forgot password, or sign up. |
| User-visible data | No backend data before submit. Display generic auth errors only through Problem Details mapping. Never display tokens, refresh state, role claims, or raw error internals. |
| Form fields | `usernameOrEmail` label `Email or username`, text/email-capable input, string, required; `password` label `Password`, password control with visibility toggle, string, required, never logged/displayed. |
| Actions | `Sign in`: submit credentials to auth transport; success hydrates session/current user and navigates by safe `returnUrl` or approved fallback. `Forgot password`: `/auth/forgot-password`. `Create account`: `/auth/sign-up`. |
| States | Initial, validating, submitting/loading, invalid credentials/backend error, email-verification-required backend state, success navigation. |
| Responsive/RTL/a11y | Auth shell, one column, visible labels, error summary/status, keyboard submit, password visibility accessible name. |
| Design classification | All form controls/actions are `AUTHORITATIVE_FEATURE`; no DPFs. |
| Status | `CONTRACT_READY`. |

## AUTH-002 Sign Up

| Field | Contract |
|---|---|
| Identity | `AUTH-002`; route `AUTH_SIGN_UP`; `/auth/sign-up`; public; routable. |
| Purpose | Create public V1 account. Non-goals: role-specific profile capture, automatic login, employer/nurse profile completion, role assignment UI. |
| Navigation | Entry from Sign in/Create account and legacy auth redirects; accepted submit exits to `AUTH_VERIFY_EMAIL_REQUEST`; secondary exit to Sign in. |
| Form fields | `email` label `Email`, email input, string, required, email format; `username` label `Username`, text, string, required, backend validation authoritative; `password` label `Password`, password, string, required, min 8, uppercase, digit per V1 evidence. |
| Actions | `Create account`: submit exact public register request; success is `202`/accepted-style handoff and navigates to check-email; errors map validation/Problem Details. |
| States | Initial, validation errors, submitting, accepted, backend duplicate/validation without unsafe account enumeration. |
| Security | Do not expose role internals, token/session state, password, duplicate-account diagnostics beyond backend-approved copy. |
| Status | `CONTRACT_READY`. |

## AUTH-LEGACY-ROLE Selection Redirect

| Field | Contract |
|---|---|
| Identity | `AUTH-LEGACY-ROLE`; route `AUTH_ROLE_SELECTION`; `/auth/role-selection`; public; routable redirect-only legacy route. |
| Purpose | Preserve old links and redirect to `/auth/sign-up`. No user-facing role-selection screen is authorized. |
| Actions/states | Immediate redirect or minimal transitional loading; no role options, no forms. |
| Status | `CONTRACT_READY` for redirect behavior only. |

## AUTH-LEGACY-NURSE Registration Redirect

| Field | Contract |
|---|---|
| Identity | `AUTH-LEGACY-NURSE`; route `AUTH_REGISTER_NURSE`; `/auth/register/nurse`; public; redirect-only. |
| Purpose | Preserve old links and redirect to `/auth/sign-up`; no nurse-specific registration screen. |
| Status | `CONTRACT_READY` for redirect behavior only. |

## AUTH-LEGACY-EMPLOYER Registration Redirect

| Field | Contract |
|---|---|
| Identity | `AUTH-LEGACY-EMPLOYER`; route `AUTH_REGISTER_EMPLOYER`; `/auth/register/employer`; public; redirect-only. |
| Purpose | Preserve old links and redirect to `/auth/sign-up`; no employer/company registration fields. |
| Status | `CONTRACT_READY` for redirect behavior only. |

## AUTH-005 Check Email

| Field | Contract |
|---|---|
| Identity | `AUTH-005`; route `AUTH_VERIFY_EMAIL_REQUEST`; `/auth/verify-email`; public; routable confirmation. |
| Purpose | Tell accepted registrants to verify email. Non-goals: resend, change email, support, notification preferences. |
| User-visible data | Registration email only if safely carried by the owning flow; otherwise generic copy. |
| Actions | Primary `Sign in`; optional email-client guidance is static only. No backend call/resend/cooldown unless separately approved. |
| States | Ready confirmation only. |
| Status | `CONTRACT_READY`. |

## AUTH-006 Verify Email

| Field | Contract |
|---|---|
| Identity | `AUTH-006`; route `AUTH_VERIFY_EMAIL_CONFIRM`; `/auth/verify-email/confirm`; public; token query route. |
| Purpose | Confirm opaque verification token from email link. |
| User-visible data | Token is internal and never displayed, decoded, copied, logged, or exposed. |
| Actions | On token present, call backend verify-email contract; success offers Sign in; failure offers safe Sign in / request flow only where approved. |
| States | Missing token, verifying/loading, success, invalid/expired/failure, retryable error. |
| Status | `CONTRACT_READY`. |

## AUTH-007 Forgot Password

| Field | Contract |
|---|---|
| Identity | `AUTH-007`; route `AUTH_FORGOT_PASSWORD`; `/auth/forgot-password`; public; routable form. |
| Purpose | Request password reset email. |
| Form fields | `email` label `Email`, email input, string, required, email format. |
| Actions | `Send reset link`: submit backend request; accepted state must not reveal whether account exists beyond backend-approved behavior. `Back to sign in`: `/auth/sign-in`. |
| States | Initial, validation, submitting, accepted status, backend error. |
| Status | `CONTRACT_READY`. |

## AUTH-008 Reset Password

| Field | Contract |
|---|---|
| Identity | `AUTH-008`; route `AUTH_RESET_PASSWORD`; `/auth/reset-password`; public; routable form with token query. |
| Purpose | Reset password from opaque reset link. |
| Form fields | `email` label `Email`, email input, string, required, email format; `newPassword` label `New password`, password, string, required, min 8, uppercase, digit; `token` from URL/query, required internally, never visible. |
| Actions | `Reset password`: call reset contract; success transitions to `AUTH-009`; `Back to sign in` or `Forgot password` safe exits. |
| States | Missing token, initial, validation, submitting, success, invalid/expired token, retryable error. |
| Status | `CONTRACT_READY`. |

## AUTH-009 Reset Password Success

| Field | Contract |
|---|---|
| Identity | `AUTH-009`; non-routable transient success state inside `AUTH_RESET_PASSWORD`. |
| Purpose | Confirm password reset succeeded and direct user to Sign in. |
| Data/actions | No password/token data. Action: `Sign in` -> `/auth/sign-in`. |
| Status | `CONTRACT_READY`. |

## AUTH-012 Account Inactive

| Field | Contract |
|---|---|
| Identity | `AUTH-012`; non-routable concept. |
| Gap | No stable coded inactive-account route/product/backend contract exists. Generic `401/403`, passive `/me.isActive`, or admin user state does not authorize an inactive-account UX. |
| Blocks | Stitch design and frontend implementation. Backend/product authority needed first. |
| Status | `BACKEND_BLOCKED`. |
