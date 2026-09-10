# Frontend Route Access Contract

```yaml
document_id: NPS-DES-INV-ROUTE-PERMISSION-MATRIX
status: APPROVED_T_FE_030_AUTH_ROUTING_CONTRACT
created_at: 2026-09-10
scope: documentation_contract_definition_only
implementation_authorization: false
source_route_authority: docs/frontend/design/inventory/page-registry.md
implemented_route_registry: frontend/src/app/core/routing/canonical-routes.ts
accepted_route_authority_commit: de6bc16 docs(frontend): approve canonical route contract
accepted_route_registry_commit: 47b38dc feat(frontend): add canonical route registry
```

## 1. Purpose and authority

This document is the approved route-access documentation owner for generic frontend route authentication behavior used by `T-FE-030`.

It consumes route identity and path/path-template authority from:

- `docs/frontend/design/inventory/page-registry.md`; and
- `frontend/src/app/core/routing/canonical-routes.ts`.

This document does not redefine, duplicate, or change canonical route IDs or paths. The canonical route registry remains the source for executable route identity/path values.

This checkpoint does not authorize Angular implementation. It does not create guards, does not modify `frontend/src/app/app.routes.ts`, does not add route activation, and does not authorize navigation, screens, or `T-FE-031`.

## 2. T-FE-030 classification model

For V1 `T-FE-030`, every exact canonical route has exactly one generic auth-routing classification:

1. `PUBLIC`
2. `ENTRY`
3. `AUTHENTICATED`

There is no anonymous-only route classification in `T-FE-030` V1.

### 2.1 PUBLIC

`PUBLIC` means:

- accessible when anonymous;
- accessible when authenticated; and
- `T-FE-030` performs no authenticated-user redirect away from it.

Do not introduce an anonymous-only guard for `PUBLIC` routes in `T-FE-030` V1. Do not redirect authenticated users away from sign-in, registration, verification, recovery, system terminal, or public preparation-package routes in `T-FE-030` V1.

### 2.2 ENTRY

`ENTRY` means:

- application entry identity;
- accessible regardless of authentication state; and
- no redirect behavior is implied by this classification.

### 2.3 AUTHENTICATED

`AUTHENTICATED` means:

- requires a resolved authenticated token-session; and
- does not imply role or permission authorization.

At the `T-FE-030` layer, an authenticated token-session satisfies the generic authentication requirement for every `AUTHENTICATED` canonical route, including `/nurse`, `/employer`, `/admin`, and their descendants. `T-FE-030` must not decide whether the authenticated user has the correct actor role or permission for the destination.

Backend authentication and authorization remain authoritative. Frontend route access is presentation/navigation behavior only.

## 3. Exact PUBLIC routes

The following 12 canonical route IDs are `PUBLIC`:

| Route ID | Canonical path/template |
|---|---|
| `AUTH_SIGN_IN` | `/auth/sign-in` |
| `AUTH_ROLE_SELECTION` | `/auth/role-selection` |
| `AUTH_REGISTER_NURSE` | `/auth/register/nurse` |
| `AUTH_REGISTER_EMPLOYER` | `/auth/register/employer` |
| `AUTH_VERIFY_EMAIL_REQUEST` | `/auth/verify-email` |
| `AUTH_VERIFY_EMAIL_CONFIRM` | `/auth/verify-email/confirm` |
| `AUTH_FORGOT_PASSWORD` | `/auth/forgot-password` |
| `AUTH_RESET_PASSWORD` | `/auth/reset-password` |
| `SYSTEM_SESSION_EXPIRED` | `/session-expired` |
| `SYSTEM_ACCESS_DENIED` | `/access-denied` |
| `PREPARATION_PACKAGES_OFFERS` | `/preparation-packages` |
| `PREPARATION_PACKAGES_OFFER_DETAIL` | `/preparation-packages/:offerSlug` |

## 4. ENTRY route

The following canonical route ID is `ENTRY`:

| Route ID | Canonical path/template |
|---|---|
| `ROOT_ENTRY` | `/` |

`ROOT_ENTRY` does not define anonymous root redirect, authenticated root redirect, role-home selection, nurse/employer/admin destination selection, or any other root redirect behavior in `T-FE-030` V1.

## 5. AUTHENTICATED classification rule

Every exact canonical route from the approved 62-route registry that is not one of the 12 `PUBLIC` route IDs and is not `ROOT_ENTRY` is classified `AUTHENTICATED`.

The expected count is 49 `AUTHENTICATED` canonical routes.

Do not maintain a manually divergent second exhaustive `AUTHENTICATED` route list in implementation when the classification can be represented deterministically from the canonical route registry plus the `PUBLIC`/`ENTRY` sets.

## 6. Anonymous access to AUTHENTICATED routes

When token-session resolution establishes that the user is anonymous and the requested canonical route is `AUTHENTICATED`, `T-FE-030` guard behavior is:

1. redirect to `AUTH_SIGN_IN`; and
2. preserve the original requested internal destination using the query parameter key `returnUrl`.

The redirect destination path is supplied by the canonical route registry and is currently `/auth/sign-in`. Do not hard-code a duplicate `/auth/sign-in` string in guard implementation when the canonical registry can supply it.

Conceptually:

```text
/auth/sign-in?returnUrl=<requested-internal-url>
```

Use Angular Router URL construction and serialization. Do not manually concatenate or manually percent-encode query parameters.

`SYSTEM_SESSION_EXPIRED` is not used for generic anonymous access to an `AUTHENTICATED` route.

`SYSTEM_ACCESS_DENIED` is not used for generic authentication-only failure.

## 7. Return intent and safe return

`T-FE-030` owns the generic safe-return contract.

The preserved return intent represents the original internal application URL requested before authentication. It preserves path plus query and fragment information to the extent represented by Angular Router state.

The generation source is Angular routing state, not an untrusted external URL supplied by application code.

The exact query key is:

```text
returnUrl
```

### 7.1 Safe-return validation

`T-FE-030` owns one reusable, pure safe-return validation helper for eventual post-auth consumption.

A candidate `returnUrl` is valid only when it represents an internal application path.

Accepted values:

- a normal internal absolute application path beginning with exactly one `/`;
- optional internal query string; and
- optional fragment.

Rejected values:

- protocol-relative values beginning `//`;
- absolute external URLs;
- URI schemes such as `http:`, `https:`, `javascript:`, `data:`, or equivalent scheme-bearing input;
- backslash-based path confusion;
- control-character input; and
- empty or non-path external-style values.

This helper must not perform role validation, permission validation, actor validation, backend calls, or `T-FE-031` policy checks.

If implementation later needs a fallback when a supplied `returnUrl` fails validation, use only fallback behavior that is explicitly authorized by the downstream login/navigation owner. Do not invent post-login destination behavior in `T-FE-030`.

### 7.2 Post-login consumption ownership

`T-FE-030` defines:

- returnUrl production;
- the `returnUrl` key; and
- the safe-return validation contract/helper.

Successful-login consumption/navigation belongs to the appropriate authentication screen/workflow when that screen is authorized. That consumer must use the `T-FE-030` safe-return validation contract rather than creating another independent validator.

## 8. AuthSessionBootstrap boundary

`T-FE-030` generic authentication guards depend on `AuthSessionBootstrap` and the resolved token-session state.

They do not read `TokenStorage` directly when the resolved session abstraction is available. They do not access `sessionStorage` or `localStorage` directly. They do not trigger a second competing bootstrap workflow and do not perform duplicate refresh orchestration.

If auth-session state is still `initializing`, the guard must not prematurely classify the user as anonymous. Navigation requiring an auth decision waits for `AuthSessionBootstrap` to resolve to `authenticated` or `anonymous` using the existing repository abstraction.

## 9. CurrentUserStore boundary

`CurrentUserStore` states do not control `T-FE-030` generic auth-only routing:

- `idle`
- `loading`
- `ready`
- `anonymous`
- `unavailable`

`CurrentUserState.unavailable` must not by itself redirect an otherwise token-session-authenticated user to sign-in. `T-FE-030` must not reinterpret current-user hydration or network failure as authentication failure.

`T-FE-031` may later consume `CurrentUserStore.ready` for role/permission UX decisions according to its own approved contract.

## 10. Session expired decision

`SYSTEM_SESSION_EXPIRED` remains a canonical `PUBLIC` route identity at `/session-expired`.

`T-FE-030` V1 does not automatically navigate to it.

Current generic session state does not provide an approved distinction between a first-time or normal anonymous user and a user whose previously authenticated session specifically expired.

Therefore:

```text
anonymous access to an AUTHENTICATED route -> AUTH_SIGN_IN + returnUrl
```

not:

```text
anonymous access to an AUTHENTICATED route -> SYSTEM_SESSION_EXPIRED
```

Do not infer session-expired semantics from `anonymous` alone. A future explicit session-expiry transition/reason contract may authorize navigation to `SYSTEM_SESSION_EXPIRED`.

## 11. Access denied and T-FE-031 boundary

`SYSTEM_ACCESS_DENIED` remains a canonical `PUBLIC` route identity at `/access-denied`.

`T-FE-030` does not navigate to it based on authentication alone.

`T-FE-030` owns only authenticated-versus-anonymous routing. It must not implement:

- role checks;
- permission checks;
- role-route mapping;
- permission-route mapping;
- Nurse/Employer/Admin route authorization;
- `/access-denied` decisions based on roles or permissions;
- navigation visibility;
- menu filtering;
- sidebar filtering; or
- backend permission mirroring.

`T-FE-031` remains a separate task for route-level UX permission policy. Backend authorization remains authoritative.

## 12. Entry and actor-family route boundary

`NURSE_ENTRY` at `/nurse` and `ADMIN_ENTRY` at `/admin` are classified `AUTHENTICATED`.

`EMPLOYER_HOME` at `/employer` and all other actor/domain routes classified `AUTHENTICATED` follow the same boundary: `T-FE-030` proves only that a resolved authenticated token-session exists.

`T-FE-030` does not assign role-specific semantics and does not implement:

- Nurse role checks on `/nurse`;
- Admin role checks on `/admin`;
- Employer role checks on `/employer`;
- redirects from `/nurse`;
- redirects from `/admin`;
- redirects from `/employer`; or
- role-home selection.

Those constraints belong to downstream route UX policy and feature/screen routing work.

## 13. Mechanical classification verification

This contract must be mechanically verified against `frontend/src/app/core/routing/canonical-routes.ts` before `T-FE-030` implementation begins.

Required counts:

```text
TOTAL_CANONICAL_ROUTES=62
PUBLIC=12
ENTRY=1
AUTHENTICATED=49
CLASSIFIED_TOTAL=62
```

Required invariants:

- `12 + 1 + 49 = 62`.
- Every exact canonical route appears in exactly one auth classification.
- No `BLOCKED` route is classified.
- No `DEFERRED` route is classified.
- No `NOT_ROUTABLE` destination is classified.
- No duplicate classification exists.
- No exact canonical route is missing.
- No canonical route ID or path is changed by this documentation contract.

## 14. Implementation authorization

T-FE-030 IMPLEMENTATION IS NOT AUTHORIZED YET.

This contract finalization checkpoint authorizes documentation only. Separate authorization is required before creating guards, safe-return helper source, guard tests, router integration, or any Angular source change.
