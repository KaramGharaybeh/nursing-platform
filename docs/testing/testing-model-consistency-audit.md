# Testing Model Consistency Audit (Step 8)

> Read-only audit of the 7 canonical artifacts against each other and against the
> committed baseline (`cbfe1eb`). No artifact modified; nothing provisioned, built,
> or deleted. One new uncommitted worktree drift found (§9) — reported, not repaired.

## 1. Reconciled counts (recalculated, not copied)

| Dimension | Count | Evidence |
|---|---|---|
| Canonical UCs | 54 | `^### UC-` in catalog; all resolve |
| Credentialed identities | 6 | NURSE_A/B/LC, EMPLOYER_A/B, ADMIN_A; no 7th (ephemeral `lc-<run>` is a derivation template) |
| Data IDs | 32 | 31 `###` headers (TOPIC_A/B share one, marked ×2); all 32 names verified present |
| Dependency edges / families | 24 / 7 | unique D-IDs D-00a/b/c + D-01..D-21; F0–F6 |
| Scenario IDs | 30 | unique `^### SCN-`; AUTH 7, NUR 7, EMP/REC 3, EXM 3, COM 1, PP 3, ADM 5, SYS 1 |
| Provisioning profiles | 30 | 23 table rows (2 grouped: SIGNUP/VERIFY, NUR-×7); 2+7+21=30 |
| LEG IDs | 26 | 001–011 (11) + 012–021 table (10, verified 10 rows) + 022–026 (5) |
| Executable legacy tests / manual intents | 0 / 15 | no specs/config/runner found repo-wide |
| UC closure | 54/54 | A 35 / B 15 / C 3 / D 1 (COM-07); every header appears in scenario map incl. range rows |
| Orphans (identity/data/edge/scenario/UC/profile) | 0 | all 6 identities used; all 32 IDs referenced; all 24 edges have downstream coverage; legacy NO_MAPPING (3) is by-design stale/non-test |

Producer totals re-verified: Admin 16 / Nurse 3 / Employer 2 / System 6 / Seed 3 /
Synth 1 / External 1 = 32. Gate 3 wording defect does NOT recur (grep clean).

## 2. Section results

- UC integrity (§4): PASS — unique IDs, valid actors/statuses/testability; coherent
  UC→Identity→Data→Dependency→Scenario→Provisioning paths; 401/403/404/409 +
  unverified/inactive/token/unpublished/grant/entitlement/transition/ownership
  negatives present (8/8/13/15 mentions + ownership/expiry).
- Identity (§5): PASS — exactly 6; B-identities ownership-only; LC quarantines
  credential mutation; ADMIN_A never mutated (ephemeral role subject);
  Employer/Admin creation via UC-ADM-02 consistent; no legacy persona leakage
  (remaining gmail mentions are explicit prohibitions/historical notes).
- Data (§6): PASS — 32/32 fields complete; no runtime GUIDs; no real emails;
  synthetic questions only; no workflow-state baselining (single justified exception).
- Workflow state (§7): PASS — grants/entitlements/sessions/scores/reports/tokens/
  provenance/requests all flow-produced; no hidden shortcuts in Steps 3/4/5/7.
- Dependencies (§8): PASS — 24 unique edges, valid endpoints, L0–L4 acyclic
  (D-13→D-06 ordering explicit); no missing producers, no flattening, no bypass.
- Scenarios (§9): PASS — 30 valid contracts; no order-dependence; no fabricated
  state; BACKEND_ONLY without UI; COM-07 never a success path; 90-day expiry
  honestly marked non-executable.
- Coverage (§10): PASS — 54/54 in exactly one state; range rows verified genuine.
- Negatives (§11): PASS for implemented behavior; no invented codes.
- Ownership (§12): PASS — both directions for nurse/employer/sessions/entitlements/
  reports/requests; no persisted-ID hard-coding (registry contract).
- Auth (§13): PASS — LC round-trip+restore sufficient; MailPit/Gmail excluded.
- Exam (§14): PASS **except drift §9** — free/paid separation, grant gate, provenance,
  premature-409s, immutability all coherent at baseline.
- Commerce (§15): PASS — Nurse owns EPH_ORDER, System owns fulfillment, pending-only
  cancel, sandbox-only, COM-07 external.
- Package (§16): PASS — 8-ID chain lines up; entitlement flow-generated; report
  immutable; expiry not faked.
- Recruitment (§17): PASS — Employer creates, Nurse decides, dup/transition/ownership
  coherent, EMPLOYER_B minimal, terminal never seeded.
- Admin (§18): PASS — ADMIN_A canonical; ephemeral subject is not an identity;
  admin writes serialized per Step 7; published never recycled; ADMIN_A stable.
- System (§19): PASS — never credentialed; consequences via actor flows; SYS-04 API-only.
- Provisioning (§20): PASS — 6/6, 32/32, 30/30; L0–L4 aligned; no per-scenario
  rebuilds; collision avoidance via run tags; abort recovery sufficient.
- Cleanup (§21): PASS — all classes respect immutability/history/audit invariants;
  every state class has a policy.
- Time (§22): PASS — 24h/1h/7d/60m-dev/30m verified in code (Step 7); no secrets
  exposed; long boundaries not faked; env variance (dev vs Test) documented.
- Env safety (§23): PASS — Development/Test gates verified in code
  (`IsDevelopment` OpenAPI/dev; Development|Test sandbox); opt-in + allowlist +
  unknown⇒REFUSE; host/db-only preflight; implementable from repo reality.
- Legacy (§24): PASS — 26 IDs accounted; 0 executable; 15 intents NO_REPLACEMENT_YET;
  0/0 replaced; mapping≠automation preserved; deletion never immediate; no stale
  leakage into Steps 1–7 (retired routes/forms/employer-reg explicitly excluded).
- Duplication (§26): PASS — no dup identities/data/scenarios/edges; near-misses
  (FREE vs PAID exams, A vs B witnesses) each justified by distinct lifecycle.
- Terminology (§27): PASS — Actor≠Identity, mapping≠replacement, sandbox≠prod,
  BACKEND_ONLY≠missing, DEFERRED≠failed used consistently.
- Step 9 readiness (§28): all 8 families have scenarios+identities+data+provisioning+
  cleanup; no unresolved external blocker (sandbox suffices; prod deferred by design).

## 3. Findings

- **P2-01 — Uncommitted exam-interaction drift (BASELINE_CONFLICT).** Dirty worktree
  (17 files, +524/−45, still uncommitted, HEAD unchanged at `cbfe1eb`) adds
  `IsFlagged` + `DELETE .../answers/{questionId}` (clear) + `PUT .../flag`
  endpoints + regenerated client + facade methods + rewritten session spec.
  This is the Step 1 §12-anticipated interaction-correction goal landing.
  Impact if committed: UC-EXM-03's recorded statement ("no flag/unflag or
  clear-selection in HEAD") becomes false; SCN-EXM-FREE-001/PAID-001 and
  SCN-PP-JOURNEY-001 session-action lists under-cover the product; EPH_EXAM_SESSION
  gains a clear transition. Bounded workaround: at Step 9 entry, EITHER land +
  update UC-EXM-03/scenarios first, OR revert the drift and proceed as documented;
  all other 51 UCs unaffected either way. Not a defect in the 7 artifacts (they are
  HEAD-accurate); not repaired here per Step 8 rules.
- **P3-01** — `###LEG-PW-001` missing heading space (renders as text, ID still
  greppable). Bookkeeping only.
- **P3-02** — Range-notation LEG IDs (012..021, 023/024/025) complicate mechanical
  parsing; counts reconcile manually to 26. Bookkeeping only.

P0: 0. P1: 0. P2: 1. P3: 2.

## 4. Verdict

**STEP_8_PASS.** The 7 artifacts describe one coherent, implementable system for the
committed baseline. **READY_FOR_STEP_9_IMPLEMENTATION: CONDITIONAL** — proceed for
all families on `cbfe1eb`; EXM-session action automation (SCN-EXM-*/PP-JOURNEY
answer/flag/clear steps) requires P2-01 resolution first (land-and-update or revert).
