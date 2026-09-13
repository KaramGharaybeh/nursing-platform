# Backend Implementation Ledger

This ledger is the authoritative detailed execution record for backend Task → Subtask → Verification Gate work.

It is a governance and tracking document only. It does not authorize implementation by itself. Implementation may proceed only through explicit technical-lead authorization, the task/gate ownership recorded here, and the repository-wide rules in `AGENTS.md`, `PROJECT_RULES.md`, and `docs/development/model-orchestration.md`.

`PROGRESS.md` remains the concise current-state and handoff memory.

## 1. Authority

Authoritative backend rules remain in:

- `docs/backend/backend-architecture.md`
- `docs/api/api-design.md`
- `docs/architecture/system-architecture.md`
- `docs/standards/engineering-standards.md`
- `PROJECT_RULES.md`
- `AGENTS.md`

When backend sources conflict, use this precedence for backend execution and status decisions:

1. Current explicit user or technical-lead decisions.
2. Current execution state and task/gate authority in this ledger, with `PROGRESS.md` as the concise current-session handoff.
3. Current API/backend architecture rules in `docs/api/api-design.md` and `docs/backend/backend-architecture.md`.
4. Product/system architecture documents and approved feature specifications.
5. Historical planning, evidence packets, trackers, and older prose.

If the above precedence cannot resolve a conflict deterministically, STOP and escalate instead of guessing.

## 2. Canonical Status and Blocker Model

Only these execution statuses are valid:

- `NOT STARTED`
- `IN PROGRESS`
- `BLOCKED`
- `READY FOR REVIEW`
- `VERIFIED`
- `REOPENED`

Blocker causes are separate from status. Valid blocker types:

- `DESIGN`
- `BACKEND`
- `CONTRACT_CLARIFICATION`
- `RUNTIME_DEPLOYMENT`
- `TOOLING_APPROVAL`
- `SECURITY`
- `DEPENDENCY`
- `SCOPE`
- `EXTERNAL`

## 3. Backend Task Registry

Backend task identifiers use `T-BE-###`. Subtasks use `ST-BE-###`. Verification gates use `GATE-BE-T###`, matching the owning task number.

| task_id | name | dependencies | contract_dependencies | status | blocker_types |
|---|---|---|---|---|---|
| `T-BE-001` | Public self-registration and unverified-login contract | Phase 1 docs commit `33284dc docs(auth): define public registration contract` | `C-AUTH-PUBLIC-REGISTER-NURSE`, `C-AUTH-PUBLIC-REGISTER-EMPLOYER`, `C-AUTH-LOGIN`, `C-AUTH-REGISTER`, `C-AUTH-SEND-VERIFY`, `C-ERROR` | `IN PROGRESS` | — |

## 4. Backend Subtask Registry

| subtask_id | task_id | name | required behavior | status |
|---|---|---|---|---|
| `ST-BE-001` | `T-BE-001` | Public Nurse registration | Add `POST /api/v1/auth/register/nurse` with public anonymous access, request fields `email`, `password`, `firstName`, `lastName`, server-assigned `Nurse` role, `EmailVerified=false`, no token/session/current-user payload, initial verification-email dispatch for genuinely new accounts, and `202 Accepted` empty response for both new and duplicate syntactically/policy-valid requests. | `NOT STARTED` |
| `ST-BE-002` | `T-BE-001` | Public Employer registration | Add `POST /api/v1/auth/register/employer` with public anonymous access, request fields `email`, `password`, `firstName`, `lastName`, server-assigned `Employer` role, no company/organization fields in V1, `EmailVerified=false`, no token/session/current-user payload, initial verification-email dispatch for genuinely new accounts, and `202 Accepted` empty response for both new and duplicate syntactically/policy-valid requests. | `NOT STARTED` |
| `ST-BE-003` | `T-BE-001` | Safe public role assignment and duplicate handling | Ensure public clients cannot submit or influence `roleIds`, `roleId`, actor IDs, permission IDs, `Admin`, `SuperAdmin`, or privileged role selection; duplicate public registration must not disclose existence, mutate existing account fields, mutate existing roles, create a second account, issue tokens, establish a session, or implicitly resend verification email. | `NOT STARTED` |
| `ST-BE-004` | `T-BE-001` | Unverified-login coded rejection | For correct credentials on an otherwise authenticatable account with `EmailVerified=false`, return HTTP `403 Forbidden` using the existing Problem Details code mechanism with code `email_verification_required`; do not return `AuthResult`, access token, refresh token, session state, roles, permissions, or account detail. Wrong credentials must preserve the existing generic invalid-credentials behavior and must not return `email_verification_required`. | `NOT STARTED` |
| `ST-BE-005` | `T-BE-001` | Administrative registration regression | Preserve existing `POST /api/v1/auth/register` administrative user creation with `Users.Create` authorization and existing role-assignment semantics; new public endpoints must not weaken or replace it. | `NOT STARTED` |

## 5. Contract Registry

| contract_id | summary |
|---|---|
| `C-AUTH-PUBLIC-REGISTER-NURSE` | `POST /api/v1/auth/register/nurse`; anonymous/public; request fields only `email`, `password`, `firstName`, `lastName`; server assigns `Nurse`; success and duplicate syntactically/policy-valid outcomes are `202 Accepted` with no response body; no tokens/session/current-user payload; no public role IDs. |
| `C-AUTH-PUBLIC-REGISTER-EMPLOYER` | `POST /api/v1/auth/register/employer`; anonymous/public; request fields only `email`, `password`, `firstName`, `lastName`; server assigns `Employer`; no company/organization fields in V1; success and duplicate syntactically/policy-valid outcomes are `202 Accepted` with no response body; no tokens/session/current-user payload; no public role IDs. |
| `C-AUTH-LOGIN` | `POST /api/v1/auth/login`; successful verified-account behavior remains existing `AuthResult`; correct credentials with `EmailVerified=false` return HTTP `403 Forbidden` coded Problem Details code `email_verification_required` and no tokens; wrong credentials remain existing generic invalid-credentials behavior. |
| `C-AUTH-REGISTER` | Existing `POST /api/v1/auth/register` administrative registration remains permission-protected by `Users.Create` and preserves existing privileged/admin semantics. |
| `C-AUTH-SEND-VERIFY` | Existing verification-email dispatch workflow must be reused for genuinely new public registrations; public registration must not expose delivery guarantees, resend behavior, or resend timing. |
| `C-ERROR` | Existing repository Problem Details conventions and code/extension mechanism must be used; do not introduce a second error envelope. |

## 6. Verification Gates

### `GATE-BE-T001`

- status_result: `NOT STARTED`
- required_evidence:
  - genuine RED-first tests before backend source implementation;
  - focused public Nurse registration tests;
  - focused public Employer registration tests;
  - focused duplicate-registration enumeration-safety tests;
  - focused unverified-login `403` coded Problem Details tests;
  - admin registration authorization regression tests;
  - relevant auth/security regression tests;
  - backend build and full repository-required backend test suite;
  - `git diff --check`;
  - independent security review covering anonymous privilege escalation, role injection, account enumeration, duplicate mutation, token issuance, unverified-account authentication, and admin endpoint authorization preservation.

## 7. `T-BE-001` Execution Notes

- phase_2a_governance_decision_date: 2026-09-13
- phase_2a_commit_dependency: `33284dc docs(auth): define public registration contract`
- implementation_scope: Backend only. No frontend components/routes/stories, canonical OpenAPI, generated frontend API, package files, migrations, database schema changes, or push.
- abuse_control_boundary: Inspect existing backend rate-limit/abuse-control infrastructure during implementation. Reuse an existing approved mechanism if one applies to anonymous auth endpoints. Do not add a new third-party dependency or materially new infrastructure architecture without approval. Public resend remains out of scope for V1.
