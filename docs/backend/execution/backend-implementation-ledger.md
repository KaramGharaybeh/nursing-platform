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
| `T-BE-001` | Public self-registration and unverified-login contract | Phase 1 docs commit `33284dc docs(auth): define public registration contract` | `C-AUTH-PUBLIC-REGISTER-NURSE`, `C-AUTH-PUBLIC-REGISTER-EMPLOYER`, `C-AUTH-LOGIN`, `C-AUTH-REGISTER`, `C-AUTH-SEND-VERIFY`, `C-ERROR` | `VERIFIED` | — |

## 4. Backend Subtask Registry

| subtask_id | task_id | name | required behavior | status |
|---|---|---|---|---|
| `ST-BE-001` | `T-BE-001` | Public Nurse registration | Add `POST /api/v1/auth/register/nurse` with public anonymous access, request fields `email`, `password`, `firstName`, `lastName`, server-assigned `Nurse` role, `EmailVerified=false`, no token/session/current-user payload, initial verification-email dispatch for genuinely new accounts, and `202 Accepted` empty response for both new and duplicate syntactically/policy-valid requests. | `VERIFIED` |
| `ST-BE-002` | `T-BE-001` | Public Employer registration | Add `POST /api/v1/auth/register/employer` with public anonymous access, request fields `email`, `password`, `firstName`, `lastName`, server-assigned `Employer` role, no company/organization fields in V1, `EmailVerified=false`, no token/session/current-user payload, initial verification-email dispatch for genuinely new accounts, and `202 Accepted` empty response for both new and duplicate syntactically/policy-valid requests. | `VERIFIED` |
| `ST-BE-003` | `T-BE-001` | Safe public role assignment and duplicate handling | Ensure public clients cannot submit or influence `roleIds`, `roleId`, actor IDs, permission IDs, `Admin`, `SuperAdmin`, or privileged role selection; duplicate public registration must not disclose existence, mutate existing account fields, mutate existing roles, create a second account, issue tokens, establish a session, or implicitly resend verification email. Positively identified existing-email uniqueness races return the same `202 Accepted` empty public response. | `VERIFIED` |
| `ST-BE-004` | `T-BE-001` | Unverified-login coded rejection | For correct credentials on an otherwise authenticatable account with `EmailVerified=false`, return HTTP `403 Forbidden` using the existing Problem Details code mechanism with code `email_verification_required`; do not return `AuthResult`, access token, refresh token, session state, roles, permissions, or account detail. Wrong credentials must preserve the existing generic invalid-credentials behavior and must not return `email_verification_required`. | `VERIFIED` |
| `ST-BE-005` | `T-BE-001` | Administrative registration regression | Preserve existing `POST /api/v1/auth/register` administrative user creation with `Users.Create` authorization and existing role-assignment semantics; new public endpoints must not weaken or replace it. | `VERIFIED` |

## 5. Contract Registry

| contract_id | summary |
|---|---|
| `C-AUTH-PUBLIC-REGISTER-NURSE` | `POST /api/v1/auth/register/nurse`; anonymous/public; request fields only `email`, `password`, `firstName`, `lastName`; server assigns `Nurse`; success and duplicate syntactically/policy-valid outcomes are `202 Accepted` with no response body; no tokens/session/current-user payload; no public role IDs. |
| `C-AUTH-PUBLIC-REGISTER-EMPLOYER` | `POST /api/v1/auth/register/employer`; anonymous/public; request fields only `email`, `password`, `firstName`, `lastName`; server assigns `Employer`; no company/organization fields in V1; success and duplicate syntactically/policy-valid outcomes are `202 Accepted` with no response body; no tokens/session/current-user payload; no public role IDs. |
| `C-AUTH-LOGIN` | `POST /api/v1/auth/login`; successful verified-account behavior remains existing `AuthResult`; correct credentials with `EmailVerified=false` return HTTP `403 Forbidden` coded Problem Details code `email_verification_required` and no tokens; wrong credentials remain existing generic invalid-credentials behavior. |
| `C-AUTH-REGISTER` | Existing `POST /api/v1/auth/register` administrative registration remains permission-protected by `Users.Create` and preserves existing privileged/admin semantics. |
| `C-AUTH-SEND-VERIFY` | Existing verification-email dispatch workflow must be reused for genuinely new public registrations; public registration must not expose delivery guarantees, resend behavior, or resend timing. |
| `C-ERROR` | Existing repository Problem Details conventions and code/extension mechanism must be used; do not introduce a second error envelope. |
| `C-AUTH-PUBLIC-HARDENING-DEBT` | Anonymous registration abuse/rate limiting and verification-email delivery retry/resilience are deferred production hardening for V1 when no existing approved limiter/retry/outbox mechanism is available; do not add new infrastructure or dependency architecture in `T-BE-001`. |

## 6. Verification Gates

### `GATE-BE-T001`

- status_result: `VERIFIED`
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
- phase_2b_decision_resolution_date: 2026-09-13
- phase_2b_decision_summary: Technical lead approved the smallest repository-consistent coded `403` mechanism for `email_verification_required`; existing email comparison semantics; enumeration-safe `202 Accepted` empty handling for duplicates and positively identified existing-email unique races; log-and-continue `202 Accepted` behavior for verification-email delivery failure after account persistence; no new rate limiter/throttling infrastructure; seeded role-name lookup for `Nurse`/`Employer`; and `User` + `UserRole` creation only with no profile/company/onboarding rows.
- abuse_control_boundary: No existing backend limiter was found by the Phase 2B scout. Anonymous public-registration abuse/rate limiting is DEFERRED PRODUCTION HARDENING for V1. Do not claim the public registration endpoint is fully production-hardened against automated abuse.
- verification_email_resilience_boundary: No new queue/outbox/retry infrastructure is authorized in `T-BE-001`; verification-email delivery retry/resilience after persisted registration is DEFERRED PRODUCTION HARDENING unless an existing approved mechanism is available.
- phase_2b_backend_evidence: RED-first Application tests failed on missing public-register types before implementation; RED-first WebApi tests failed on missing public-register namespace before implementation. Final focused Application auth tests passed 34/34; focused WebApi auth/register tests passed 112/112; `dotnet build backend/NursingPlatform.slnx` passed with 0 warnings and 0 errors; full non-PostgreSQL backend test run passed Domain 143, Infrastructure 168, Application 603, WebApi 390. Full unfiltered backend test command was run and failed only because PostgreSQL integration-test environment variable `NURSING_PLATFORM_TEST_POSTGRES_CONNECTION_STRING` is not set for existing PostgreSQL-only tests.
- phase_2b_security_review: Independent Mimo security review `AUTH-V1-PHASE2B-SECURITY-REVIEW-001` returned PASS with no Critical/High/Medium findings; Low inactive+unverified test-hardening observation was corrected and focused/full non-PostgreSQL verification reran green.
