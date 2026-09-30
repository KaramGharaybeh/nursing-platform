# Test Environment Provisioning Contract (Step 7)

> CONTRACT/DESIGN ONLY — no DB touched, nothing provisioned, no code written.
> Authority: implementation > migrations/seeds/config > Steps 1–6. Baseline HEAD
> `cbfe1eb`. Secret values are NEVER pasted here — config keys named, values omitted.

## 1. Clean baseline definition

A clean test database =(Local/Test Postgres, disposable database name):

- A. **Schema:** all 17 EF Core migrations applied via `DatabaseInitializer`
  (`MigrateAsync`) — Code-First only, never manual DDL.
- B. **Implementation-owned seed/bootstrap** (product-owned, NOT fixtures):
  `ReferenceDataSeeder` (10 countries, 7 languages, 5 roles, 28 permissions +
  Admin grants) + `BootstrapAdminService` (initial Admin from `Admin__*` config).
- C. **Testing-owned provisioning** (this contract): 6 identities + 16 reusable
  resources, created exclusively through real product flows (§5, §8).
- D. **Per-scenario runtime state:** created inside scenarios via real flows,
  referenced through the runtime registry (§8), never pre-seeded.

## 2. Baseline fabrication ban (Step 3 rule preserved)

The baseline MUST NOT contain: Paid status, provider completion, ExamAccessGrant,
entitlements/rights/snapshots, sessions/answers/scores/reports, practice progress,
provenance rows, request lifecycle states, auth/refresh tokens, ownership links set
outside flows. Legitimate producer paths: UC-COM-06 fulfillment (grant/entitlement),
UC-EXM-02/PP-03 (session+provenance), UC-EXM-04 submit (scores), UC-PP-04 (progress),
UC-PP-05 first GET (report), UC-REC-02 (Pending), UC-AUTH-03/06 + System issuance
(tokens). Sole exception: STATE_INACTIVE_USER (setup flip, no product flow exists).

## 3. Identity provisioning contracts (6/6)

| Identity | Producer path (real flows only) | Baseline state | Creds | Survives suites | Mutating scenarios | Reset |
|---|---|---|---|---|---|---|
| NURSE_A/B/LC | UC-AUTH-03 sign-up → UC-AUTH-04 verify → UC-AUTH-08 names | Active/verified/complete, Nurse | Deterministic per registry; harness reads from Step 2 doc, never hard-codes into scenarios | YES | A: profile/content edits (snapshot-restore); LC: AUTH-02/05/06/07 incl. reset round-trip + re-login | Restore names/languages/skills; LC restore canonical password + fresh login |
| EMPLOYER_A/B | UC-ADM-02 admin-create (public sign-up is Nurse-only) → verify → UC-EMP-01/02 | Active/verified, profile+org complete | Deterministic per registry | YES | Profile/org edits (restore); requests are per-run rows, not account mutation | Restore profile/org snapshot |
| ADMIN_A | Seed/bootstrap (`Admin__*` config) — never public sign-up | Active/verified/named, full perms | Deterministic per registry; 전용 setup use | YES | NONE on the account (no role/deactivate/reset in tests) | Re-login on token expiry only |

NURSE_LC isolation is load-bearing: AUTH-07 revokes ALL refresh tokens and changes
the credential — running it on NURSE_A/B would break journey/witness logins.

## 4. Credential & environment safety contract

- Local/Test only; `.test` namespace; no real users; no committed personal secrets;
  no legacy gmail personas; no MailPit dependence (SMTP catcher at configured
  `Email__SmtpHost/Port` is the mechanism — token IDs come from real-flow API
  responses/DB reads in setup, never from inbox scraping as authority).
- **Fail-closed guard (mandatory for Step 9 harness):** provisioning/reset code MUST
  refuse unless ALL hold: environment name ∈ {`Development`, `Test`} (the only names
  the implementation itself gates dev/test behavior on: `IsDevelopment()` for
  OpenAPI/dev endpoints; `Development|Test` for sandbox completion); connection
  targets a disposable database (name allow-list owned by harness config, e.g.
  `*_test`); an explicit opt-in marker (harness-owned, e.g. `NURSING_TEST_MODE=true`).
  Unknown environment ⇒ REFUSE. Never log secrets; preflight prints host/db-name only.

## 5. Data provisioning (32/32 — category summary; details live in Step 3)

- **Seed (3):** REF_COUNTRY_US/GB, REF_LANGUAGE_EN — from B, lookup by Code.
- **Reusable via Admin API (14):** CAT_PRIMARY → EXAM_* (UC-ADM-04) → QUESTION_SET_*
  (UC-ADM-06, draft V1) → validate → publish (UC-ADM-05); PRODUCT_EXAM_PRIMARY
  (UC-ADM-07); TOPIC_* → PROFILE_PRIMARY → MATERIAL/COLLECTION V1 → PACKAGE_DEF →
  PACKAGE_VERSION (isolation+validation) → publish → OFFER activate (UC-ADM-08).
  Order matters (Step 4 D-06/D-09/D-13); each step asserts its API result before
  continuing. Never mutated by consumers (immutable-on-publish enforced by draft
  gates — verified `EnsureDraftVersionAsync`; offers Active locked).
- **Reusable identity-owned (2):** PROFILE_NURSE_A (UC-NUR-01/05/06), PROFILE_EMPLOYER_A
  (UC-EMP-01/02); snapshot-restore on mutation.
- **Per-run (5):** EPH_ORDER (UC-COM-02) / EPH_EXAM_SESSION (UC-EXM-02/PP-03) /
  EPH_CONTACT_REQUEST (UC-REC-02) / EPH_ADMIN_CATEGORY+EXAM (admin UCs under test);
  unique slugs `t-<resource>-<runtag>`, emails/usernames run-suffixed; runtime IDs
  captured from 201/start responses into the registry (§8).
- **Workflow-generated (6: 2 MUST_FLOW + 4 SYSTEM):** GRANT/ENTITLEMENT via UC-COM-06;
  TOKENS/PROGRESS/PROVENANCE/SCORE as operation consequences — asserted, never inserted.
- **Synthesized (1):** STATE_INACTIVE_USER flip + reactivate (Local/Test only).
- **External (1):** EXT_PROD_PAYMENT — sandbox (`Payment:Checkout:Provider=Sandbox`
  in dev config) is the only invokable path; production NEVER simulated as authority.

## 6. Immutable/published strategy

Published versions, consumed rights, finalized sessions/reports, terminal requests,
Paid orders are facts: RETAIN_IMMUTABLE / TERMINAL_DISCARD — never edited to
recycle. Draft-only delete and retire/archive are the only legal removals; published
accumulation uses unique-slug-per-run until a future governed purge (out of scope).
Mutation detection: reusable setup re-validated by slug-lookup + status asserts at
suite start (RECREATE_BASELINE only on mismatch).

## 7. Runtime-object registry (conceptual)

`EPH_ORDER→OrderId (UC-COM-02 201)`, `EPH_EXAM_SESSION→SessionId (start)`,
`EPH_CONTACT_REQUEST→RequestId (201+Location)`, `GRANT→implied (next paid-start
success)`, `ENTITLEMENT→EntitlementId (list)`, admin-created IDs (per-create
responses), token values (issuance responses, single-use). Symbolic names in
scenarios; runtime GUIDs never hard-coded. (Design only — implemented in Step 9.)

## 8. Reset levels & cleanup classes

- Environment reset: drop+recreate disposable DB → L0 (migrate+seed+bootstrap).
- Suite reset: verify reusable slugs/statuses (recreate on mismatch); leave history rows.
- Family reset: restore profile snapshots (NURSE_A, EMPLOYER_A); LC canonical-password check + login.
- Scenario reset: per-run uniques make most scenarios self-isolating; cancel stale
  pending orders/sessions owned by the scenario identity (recovery-safe: identifiable
  by run tag).
- Cleanup classes in play: DELETE_SAFE (drafts, owned CRUD rows, test CVs),
  RESET_TO_BASELINE (profiles, LC credential), RETAIN_IMMUTABLE (published/final/Paid),
  TERMINAL_DISCARD (submitted sessions, decided requests, consumed attempts),
  EXTERNAL_NO_CLEANUP (prod provider: nothing to clean).

## 9. Independence, sharing, parallelism

- Order independence: scenarios compose their own upstream chain (L0–L4 topology);
  NEVER assume another test ran. Shared safely: seed, published reusables, stable
  profiles. Never shared: LC credential state, active sessions, mutating orders,
  transitioning requests, consumable rights, tokens.
- Parallel: SAFE — read-only/browse, independent per-run flows with unique slugs;
  SAFE-WITH-RUN-SCOPED-DATA — all EPH_* flows; SERIALIZED — anything touching
  NURSE_LC credential, ADMIN_A setup writes, shared-witness (NURSE_B/EMPLOYER_B)
  assertions.
- Abort recovery: run-tagged artifacts are discoverable (pending orders/sessions/
  requests per identity+tag) → next run discards/completes-then-discards; LC
  canonical-password check runs before LC scenarios.

## 10. Time & externals

- Deterministic windows (verified): verify/resend 24h, reset 1h, refresh 7d,
  access JWT 60m (dev), order 30m, analytics weekly buckets. Exercisable: short
  windows via real waits only where cheap; premature-result/report 409s need no
  clock control.
- NON-EXECUTABLE boundaries (no fake clocks): 90-day entitlement expiry/consumed-window
  assertions beyond terminal-state logic; provider SLAs. Covered by unit/integration
  tests as supporting evidence, marked in scenarios — never simulated.
- Externals: prod provider DEFERRED (no fabrication); email via local SMTP catcher
  (localhost parity in dev config), content-agnostic asserts.

## 11. Scenario → provisioning profile (30/30)

Each scenario assumes L0 (migrated+seeded+bootstrap) and composes the rest:

| Scenario(s) | Reusable prerequisites | Identity/profile | Runtime-created | Isolation/reset |
|---|---|---|---|---|
| SCN-AUTH-SIGNUP/VERIFY-001 | Nurse role (seed) | ephemeral SP-NEW-UNVERIFIED | User+token rows | unique email; abandon |
| SCN-AUTH-LOGIN-001 | NURSE_A baseline | NURSE_A + ephemeral + inactive flag (synth) | Refresh row on success | logout |
| SCN-AUTH-SESSION-001 | NURSE_LC | NURSE_LC (+ANONYMOUS paths) | token rotations | re-login |
| SCN-AUTH-PASSWORD-001 | NURSE_LC | NURSE_LC | reset tokens | restore canonical + login |
| SCN-AUTH-PROFILE-001 | NURSE_A | NURSE_A (+incomplete excursion) | — | restore names |
| SCN-AUTH-RESEND-001 | NURSE_LC + ephemeral | as listed | token rows | — |
| SCN-NUR-* (7) | PROFILE_NURSE_A + REF_* | NURSE_A (+NURSE_B witness) | owned rows | delete/restore |
| SCN-EMP-PROFILE-001 | PROFILE_EMPLOYER_A | EMPLOYER_A | — | restore snapshot |
| SCN-REC-DISCOVERY-001 | both profiles + REFs | EMPLOYER_A/B | — | read-only |
| SCN-REC-REQUEST-001 | both profiles | EMPLOYER_A + NURSE_A (+B witness) | EPH_CONTACT_REQUEST | terminal rows persist |
| SCN-EXM-FREE-001 | CAT/EXAM_FREE/QSET_FREE | NURSE_A | EPH_EXAM_SESSION + score | submit terminal |
| SCN-EXM-PAID-001 | +EXAM_PAID/QSET_PAID/PRODUCT | NURSE_A (+B denied) + ADMIN setup | EPH_ORDER + GRANT + session | facts persist |
| SCN-EXM-OWNERSHIP-001 | session owned by A | NURSE_A/B | — | read-only 404s |
| SCN-COM-ORDER-001 | PRODUCT | NURSE_A | EPH_ORDER | cancel terminal |
| SCN-PP-DISCOVERY-001 | OFFER_PRIMARY | ANONYMOUS | — | read-only |
| SCN-PP-JOURNEY-001 | full package chain | NURSE_A | EPH_ORDER + ENTITLEMENT + session + report | facts persist |
| SCN-PP-OWNERSHIP-001 | entitlement owned by A | NURSE_A/B | — | read-only 404s |
| SCN-ADM-USERS-001 | ADMIN_A | ADMIN_A (+ephemeral subject) | ephemeral user | unique email |
| SCN-ADM-CATEGORY-001 | ADMIN_A | ADMIN_A | EPH_ADMIN_CATEGORY | delete-if-legal |
| SCN-ADM-EXAM-001 | ADMIN_A | ADMIN_A | EPH_ADMIN_EXAM + drafts | delete/retire |
| SCN-ADM-PRODUCT-001 | ADMIN_A + subject exam | ADMIN_A | ephemeral product | archive/restore |
| SCN-ADM-PACKAGE-001 | ADMIN_A + ephemeral chain | ADMIN_A | ephemeral chain | retire/leave tagged |
| SCN-SYS-HEALTH-001 | running env | none | — | — |

Topological provisioning order: L0 schema/seed/bootstrap → L1 identities+baselines
(signup/verify/names; admin-create+upserts) → L2 admin definitions (category, topics,
materials, collections) → L3 published products/exams/packages (validate→publish→
activate asserts) → L4 scenario runtime state. No unexplained starting states.
