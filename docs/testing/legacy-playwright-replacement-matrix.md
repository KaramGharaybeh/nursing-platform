# Legacy Playwright Replacement Matrix (Step 6)

> Read-only inventory + model-level mapping only. NOTHING deleted, moved, or
> edited; NO automation implemented. Authority: implementation > Steps 1–5;
> legacy material is historical coverage evidence only. Headline finding:
> the repository contains ZERO executable legacy Playwright tests — no config,
> no specs, no runner, no fixtures, no page objects. Legacy = manual MCP-driven
> QA narratives (Sept 2026) + tooling-generated snapshots/logs/screenshots.
> The OpenCode host-config entries below describe the Step 6 snapshot; that config
> was subsequently removed. They are not current tooling or removal instructions.

## 1. Inventory summary (586 files + 1 config block, 26 LEG IDs)

| Group | Location | Files | Nature |
|---|---|---|---|
| Raw MCP snapshots | `.playwright-mcp/page-*.yml` (309) + `batch3-*-review.yml` (3) + `console-*.log` (178) + `*.png` (41) | 531, untracked, tooling-generated | Generated output, no assertions |
| Manual QA campaign | `Playwright MCP Full Review/` (17 md + 35 png) | 52, untracked, 2026-09-14 Auth campaign @ `d3f8e43` | Manual narratives + evidence |
| Adjacent reports | `reports/*.md` | 3 | Delta/design/ops docs, NOT Playwright workflow artifacts |
| Tooling config | `opencode.jsonc` `playwright` MCP block (`npx @playwright/mcp`) | 1 block | Host config at the Step 6 snapshot, not a test; later removed |

Out of scope (not legacy, not inventoried as LEG): frontend `*.spec.ts` unit/component
tests, `*.stories.ts` Storybook files, backend `*.Tests` — current tooling.

## 2. Artifact matrix

Fields: Type (DOC/MANUAL/OUTPUT/CONFIG) · Mapping (COMPLETE/PARTIAL/NO/NON-TEST) ·
Replace (applies to MANUAL only; all NO_REPLACEMENT_YET — Steps 0–5 changed no test
code and Step 9 hasn't begun) · Delete (NO / FUTURE_CANDIDATE; nothing YES).

###LEG-PW-001 · `.playwright-mcp/page-*.yml` ×309 · OUTPUT · NO_SCENARIO_MAPPING · Delete: FUTURE_CANDIDATE

- Raw MCP DOM dumps (sample: bare `main` region, no assertions). No testable behavior
  recorded; screens depicted unknown. No UC/scenario map possible. No dependencies.
  Replacement target: none (not a test). Notes: largest deletion candidate by volume
  once automation exists; carries zero behavioral authority.

### LEG-PW-002 · `.playwright-mcp/batch3-admin-exam*-review.yml` ×3 · OUTPUT · PARTIAL_SCENARIO_MAPPING · Delete: FUTURE_CANDIDATE

- Depicts: admin exams filters (country/category/status/price-type), Add-exam button,
  `No exams yet` empty state, load-error and detail-unavailable states.
- Intent maps conceptually to SCN-ADM-EXAM-001 / UC-ADM-04 (empty/error/unavailable
  views only — visual, no assertions, no lifecycle). Canonical scenarios cover the
  behavior; screenshots add nothing executable.

### LEG-PW-003 · `.playwright-mcp/console-*.log` ×178 · OUTPUT · NON_TEST_SUPPORT_ARTIFACT · Delete: FUTURE_CANDIDATE

- Browser console noise; the campaign's own report calls it expected noise
  (401/403/409 resource errors, MailPit srcdoc logs). No behavior. No mapping.

### LEG-PW-004 · `.playwright-mcp/*.png` ×41 · OUTPUT · PARTIAL_SCENARIO_MAPPING · Delete: FUTURE_CANDIDATE

- Visual-only snapshots naming: t-fe-028 route-loading; 067 exams list/detail; 068
  instructions; 069 session; 071 result; 072 review; 073 history; 074 analytics;
  075 offers; 078 practice; 079 report; 082 products; corr-signup/reset; auth-sign-up.
- Conceptual homes: SCN-EXM-FREE-001, SCN-COM-ORDER-001 (products only),
  SCN-PP-DISCOVERY-001, SCN-PP-JOURNEY-001 (practice/report only). Zero assertions;
  viewport variants (mobile/rtl) have no canonical scenario dimension.

### LEG-PW-005 · `Playwright MCP Full Review/README.md` · DOC · NON_TEST_SUPPORT_ARTIFACT · Delete: FUTURE_CANDIDATE

- Campaign method record (env URLs, viewports, 5 gmail personas, severity scale,
  no-fix statement). Persona table is the identity-mismatch source (see §5).

### LEG-PW-006 · `.../final-report.md` · DOC · COMPLETE_SCENARIO_MAPPING · Replace: NO_REPLACEMENT_YET · Delete: FUTURE_CANDIDATE

- 10/10 Auth routes, 5 personas, 8 issues (QA-AUTH-001..006 + ENV-001/002).
- Every functional finding has a canonical home: registration/verify/login/reset
  mechanics → SCN-AUTH-SIGNUP/VERIFY/LOGIN/PASSWORD-001; terminal screens →
  SCN-AUTH-SESSION-001. QA-AUTH-001 (post-login UI failure) is SUPERSEDED —
  current sign-in navigates via guards/returnUrl (automation must prove it).
- API observations (202/403-coded/200/409/duplicate-202) already match current
  implementation — usable as cross-checks, never as authority.

### LEG-PW-007 · `journeys/nurse-journey.md` · MANUAL · PARTIAL_SCENARIO_MAPPING · Replace: NO_REPLACEMENT_YET · Delete: FUTURE_CANDIDATE

- Intent: role-select → register → validate → 202 → unverified-403 → MailPit verify →
  login (backend OK, UI stuck QA-AUTH-001). Personas: gmail nurse, `QaInitial2026!`.
- Maps to SCN-AUTH-SIGNUP/VERIFY/LOGIN-001 for verify/login mechanics; STALE:
  `POST /api/v1/auth/register/nurse` now 410 (live: Nurse-only `/auth/sign-up`
  with username, no names); role-selection now redirect-only; QA-AUTH-001 superseded.
- Setup sins: gmail mailbox, MailPit-dependent token retrieval, per-persona account.

### LEG-PW-008 · `journeys/employer-journey.md` · MANUAL · PARTIAL_SCENARIO_MAPPING · Replace: NO_REPLACEMENT_YET · Delete: FUTURE_CANDIDATE

- Intent: identical chain via `/auth/register/employer` → Employer role, parity checks.
- STALE core: public employer registration retired (410); canonical employer
  provisioning is UC-ADM-02 (SCN-ADM-USERS-001). Registration-mechanics observations
  partially inform SCN-AUTH-SIGNUP-001; employer-product intent has NO UI journey
  (backend-only REC/EMP). Same setup sins as 007.

### LEG-PW-009 · `journeys/unverified-user-journey.md` · MANUAL · COMPLETE_SCENARIO_MAPPING · Replace: NO_REPLACEMENT_YET · Delete: FUTURE_CANDIDATE

- Intent: fresh account → correct-creds login → 403 coded, safe message, no session,
  kept unverified. Fully covered by SCN-AUTH-LOGIN-001's 403 branch (ephemeral
  SP-NEW-UNVERIFIED). Same setup sins; resend-absence note is now covered by
  SCN-AUTH-RESEND-001 (API-only by design).

### LEG-PW-010 · `journeys/password-reset-journey.md` · MANUAL · COMPLETE_SCENARIO_MAPPING · Replace: NO_REPLACEMENT_YET · Delete: FUTURE_CANDIDATE

- Intent: client validation matrix → enumeration-safe 200s → MailPit link →
  weak-reject → reset 200 → old-401/new-backend-OK(+QA-AUTH-001 UI).
- Fully covered by SCN-AUTH-PASSWORD-001 (incl. 409-invalid-token, already in report
  §API Observations). MailPit mechanics are method, not behavior.

### LEG-PW-011 · `journeys/duplicate-registration-journey.md` · MANUAL · COMPLETE_SCENARIO_MAPPING · Replace: NO_REPLACEMENT_YET · Delete: FUTURE_CANDIDATE

- Intent: same email twice → identical 202s, exactly one email (no flood/signal).
- Fully covered by SCN-AUTH-SIGNUP-001's duplicate negative (NURSE_A email, row-unchanged assert).

### LEG-PW-012..021 · `pages/*/review.md` ×10 · MANUAL/DOC · Delete: FUTURE_CANDIDATE · Replace: NO_REPLACEMENT_YET (all)

| ID | Page | Legacy intent | Canonical home | Mapping | Stale note |
|---|---|---|---|---|---|
| 012 | auth-sign-in | validation matrix, 403-unverified, 200-stuck (QA-AUTH-001) | SCN-AUTH-LOGIN-001 (UC-AUTH-01) | COMPLETE | QA-AUTH-001 superseded |
| 013 | auth-role-selection | choose Nurse/Employer | none — screen is redirect-only | NO_SCENARIO_MAPPING | screen no longer exists as a choice |
| 014 | auth-register-nurse | 4-field form, empty/malformed validation, 202 | SCN-AUTH-SIGNUP-001 (UC-AUTH-03) | PARTIAL | live form is email+username+password (no names) |
| 015 | auth-register-employer | employer self-registration | none (public employer reg retired) | NO_SCENARIO_MAPPING | superseded by UC-ADM-02 (SCN-ADM-USERS-001) |
| 016 | auth-verify-email | check-inbox notice | SCN-AUTH-SIGNUP-001 end-state | COMPLETE | CTA-gap notes are design-gated, not behavior |
| 017 | auth-verify-email-confirm | valid/invalid token states | SCN-AUTH-VERIFY-001 (UC-AUTH-04) | COMPLETE | — |
| 018 | auth-forgot-password | empty/malformed/generic-200 | SCN-AUTH-PASSWORD-001 (UC-AUTH-06) | COMPLETE | — |
| 019 | auth-reset-password | weak-reject, 200, 409-invalid | SCN-AUTH-PASSWORD-001 (UC-AUTH-07) | COMPLETE | — |
| 020 | session-expired | static notice | SCN-AUTH-SESSION-001 (UC-AUTH-10) | COMPLETE | — |
| 021 | access-denied | static notice | SCN-AUTH-SESSION-001 (UC-AUTH-10) | COMPLETE | — |

### LEG-PW-022 · Full Review 35 pngs · OUTPUT · PARTIAL_SCENARIO_MAPPING · Delete: FUTURE_CANDIDATE

- Desktop/tablet/mobile per route + defect shots. Evidence for 012–021; responsive
  dimension has no canonical scenario (by design — behavior truth over pixels).

### LEG-PW-023/024/025 · `reports/business-feature-delta-report.md`, `frontend-design-penpot-readiness-audit.md`, `github-push-delta-report.md` · DOC · NON_TEST_SUPPORT_ARTIFACT · Delete: NO

- Branch-delta, design-readiness, and push-delta analyses — NOT Playwright workflow
  artifacts. No scenario mapping needed; deletion out of Step 6 scope entirely.

### LEG-PW-026 · `opencode.jsonc` playwright MCP block · CONFIG · NON_TEST_SUPPORT_ARTIFACT · Delete: NO

- Tooling enabled at the Step 6 snapshot (`npx @playwright/mcp`), not a test.
  `Delete: NO` records Step 6 scope only; the host config was removed later.

## 3. Stale / superseded legacy behavior (8 items)

1. Active `POST /api/v1/auth/register/nurse|employer` — now `410 Gone` (LEG-007/008/014/015).
2. 4-field registration (email/password/names, no username) — live: email+username+password (007/008/014).
3. `/auth/role-selection` as a real choice screen — now redirect-only (007/008/013).
4. Public employer self-registration — retired; employers are admin-created (008/015).
5. QA-AUTH-001 post-login UI failure — superseded by current guard/returnUrl navigation; automation must prove, not assume (006/007/008/010/012).
6. MailPit-dependent token retrieval as test method — canonical scenarios capture runtime IDs from real flows; no external mailbox (007–011).
7. Gmail personas + shared `QaInitial2026!` + 5-accounts-for-auth — replaced by Step 2 identities (NURSE_A/LC, ephemeral derivations); legacy pattern must NOT be copied (005/006/007–011).
8. Per-persona account proliferation as method — violates REUSE BEFORE CREATE (same sources).

No genuine implementation-vs-canonical contradiction found; Steps 1–5 stand unchanged.

## 4. Identity / data mapping notes

- Legacy nurse/unverified/reset/duplicate personas → NURSE_A (standard), ephemeral
  SP-NEW-UNVERIFIED derivations (one-shot states), NURSE_LC (reset round-trip).
- Legacy employer persona → EMPLOYER_A for provisioning intent only; its UI journey
  has no canonical equivalent (correctly — no employer UI exists).
- Legacy QA passwords/MailPit/gmail → no canonical equivalent by design (synthetic
  `.test` credentials + flow-captured tokens replace them).
- No legacy fixture maps to Step 3 Data IDs (legacy never reached business data —
  blocked at QA-AUTH-001); t-fe/batch3 pngs depict screens whose data is now
  CAT_PRIMARY/EXAM_*/OFFER chains, but depictions are not fixtures.

## 5. Reverse view — Scenario → legacy coverage

- Manual-approx full (behavior narrated, never automated): SCN-AUTH-SIGNUP-001,
  SCN-AUTH-VERIFY-001, SCN-AUTH-LOGIN-001, SCN-AUTH-PASSWORD-001 (4).
- Partial (visual/static fragments only): SCN-AUTH-SESSION-001 (terminal screens),
  SCN-EXM-FREE-001 (067–074 pngs), SCN-COM-ORDER-001 (082 products pngs only),
  SCN-PP-DISCOVERY-001 (075), SCN-PP-JOURNEY-001 (078/079), SCN-ADM-EXAM-001
  (batch3 ymls) (6).
- Zero legacy coverage (20): all NUR/EMP/REC scenarios, SESSION/RESEND/PROFILE auth,
  EXM-PAID/OWNERSHIP, PP-OWNERSHIP, ADM-USERS/CATEGORY/PRODUCT/PACKAGE, SYS-HEALTH.
  Zero is NOT a defect — the new model exceeds old coverage by design.

## 6. Reconciliation counts

- Inventoried: 26 LEG IDs = 586 files + 1 config block.
- Executable legacy tests: **0** (no specs/config/runner/fixtures/page-objects found).
- Manual coverage intents: 15 (5 journeys + 10 page reviews).
- Generated outputs: 5 groups (001, 002, 003, 004, 022) = 572 files.
- Documentation: 5 (005, 006, 023, 024, 025). Config: 1 (026). [15+5+5+1 = 26 ✓]
- COMPLETE_SCENARIO_MAPPING: 11 (006, 009, 010, 011, 012, 016, 017, 018, 019, 020, 021).
- PARTIAL: 6 (002, 004, 007, 008, 014, 022). NO_MAPPING: 3 (001, 013, 015).
- NON_TEST_SUPPORT: 6 (003, 005, 023, 024, 025, 026). [11+6+3+6 = 26 ✓]
- FULLY_REPLACED: 0. PARTIALLY_REPLACED: 0. NO_REPLACEMENT_YET: 15 (all manual intents; outputs/docs/config need no behavioral replacement).
- Deletion: FUTURE_CANDIDATE 22 groups (001–022); NO 4 (023–026); YES_AFTER_VERIFIED_REPLACEMENT 0 (nothing deleted in Step 6).
- Stale/superseded behaviors: 8 (§3). Identity mismatches: 7 artifacts (005, 006, 007–011).
- Non-canonical setup assumptions: 6 (006 + 007–011: MailPit, gmail, proliferation).
- Scenarios: full-manual-approx 4 / partial 6 / zero 20 (= 30 ✓).
