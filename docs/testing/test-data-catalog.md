# Canonical Test Data Catalog (Step 3)

> Authority: `docs/testing/system-use-case-catalog.md` (54 UCs),
> `docs/testing/test-identity-registry.md` (6 identities). Baseline
> `feat/2026-09-27-stitch-ui-refactor` @ `cbfe1eb`. Documentation only —
> nothing here is provisioned. REUSE BEFORE CREATE. Producer is Administrator
> (ADMIN_A) for all admin-authored setup — Expert owns nothing (Gate 1).
> Slugs below are globally unique per their scope and carry a `test-` prefix.

## 1. Classification legend

`SEED_REFERENCE` (existing seeder output) · `CANONICAL_REUSABLE_RESOURCE`
(provisioned once via real Admin API, reused, never mutated by consumers) ·
`PER_RUN_EPHEMERAL` (unique per run) · `MUST_CREATE_THROUGH_REAL_FLOW`
(business-critical, real workflow only) · `SYSTEM_GENERATED` (app-created
consequence) · `SETUP_SYNTHESIZED_TEST_STATE` (no product flow; justified,
Local/Test only) · `EXTERNAL_DEPENDENCY` (outside repo control).

Per-run namespace: run tag `<suite>-<utc-basic-timestamp>` (e.g.
`exm-20261001t080000z`); generated slugs `t-<resource>-<runtag>`, lowercase,
≤60 chars (slug ceilings are 160; keep short). Stable canonical slugs use the
exact values in §3. Runtime GUIDs are NEVER hard-coded (seed GUIDs exist but
lookup is by code/slug/email — implementation-backed unique indexes verified:
`Country.Code`, `Language.Code`, `Exam.Slug`, package/offer/material/collection
`Slug`, topic `(ExamCategoryId,Slug)`, `User.Email/NormalizedUsername`).

## 2. SEED_REFERENCE (3)

### REF_COUNTRY_US — Functional Area: reference — SEED_REFERENCE

- Purpose: primary country for categories, exams, nurse license/current country.
- Canonical values: Code `US`, Name `United States` (verified seeder).
- Lookup: `GET /api/v1/countries` by Code. State: Active. Mutable: NO.
- Dependent UCs: UC-NUR-01/02/03, UC-EXM-01, UC-ADM-03/04/08 chain, UC-REC-01.
- DIRECT_DB_CREATION_ALLOWED: n/a (existing seed). Cleanup: none (shared seed).

### REF_COUNTRY_GB — reference — SEED_REFERENCE

- Purpose: SECOND country value so filter tests can distinguish (license vs current,
  candidate search). Code `GB`, Name `United Kingdom`. Same mechanics as US.
- Why not ephemeral: both values are free from the existing seed; no fixture needed.
- Dependent UCs: UC-REC-01, UC-EXM-01 filters.

### REF_LANGUAGE_EN — reference — SEED_REFERENCE

- Purpose: primary language for nurse language tests. Code `EN`, Name `English`.
- Lookup: `GET /api/v1/languages` by Code. Dependent UCs: UC-NUR-05, UC-REC-01.

## 3. CANONICAL_REUSABLE_RESOURCE (16)

Shared rules: provisioned once via the cited API by the owning actor — ADMIN_A for
all admin-authored resources, NURSE_A for PROFILE_NURSE_A (via UC-NUR-01/05/06),
EMPLOYER_A for PROFILE_EMPLOYER_A (via UC-EMP-01/02); ordinary
consumer tests MUST NEVER mutate; lifecycle tested only on ephemeral copies (§4).
Published entities support retire/archive, NOT delete (delete is draft-only —
verified `EnsureDraftVersionAsync` gates + UC-ADM-04/05); offers active→inactive.

### CAT_PRIMARY — exam/category setup — CANONICAL_REUSABLE_RESOURCE

- Purpose: single root category for all canonical exams + reporting topics.
- Values: Country REF_COUNTRY_US, Name `Test Primary Category`, Slug `test-cat-primary`.
- Producer UC-ADM-03. Upstream: REF_COUNTRY_US. Identity: ADMIN_A.
- Lookup: admin list by slug. State: Active. Mutable in normal tests: NO.
- DIRECT_DB_CREATION_ALLOWED: NO (via UC-ADM-03). Dependents: all EXM/PP/COM/ADM-chain UCs.
- Cleanup: preserve baseline (reuse forever).

### EXAM_FREE_PRIMARY — free consumption — CANONICAL_REUSABLE_RESOURCE

- Purpose: free-exam learner journeys. Values: Country US, Category CAT_PRIMARY,
  Title `Test Free Primary Exam`, Slug `test-exam-free-primary`, IsFree=true,
  Duration 60, Passing 60. State: Published + published version V1 (immutable).
- Producer UC-ADM-04/05. Upstream: CAT_PRIMARY. Lookup: catalog by slug.
- Mutable: NO. DIRECT_DB: NO. Dependents: UC-EXM-01..06 (free path).
- Cleanup: preserve. Isolation: sessions are per-run ephemeral (see EPH_EXAM_SESSION).

### QUESTION_SET_FREE — exam content — CANONICAL_REUSABLE_RESOURCE

- Purpose: deterministic scored content for EXAM_FREE_PRIMARY V1 (max 4 pts).
- Content (original synthetic, NOT real exam-bank material):
  - QFREE-01 · 1pt · order 1 · "Which action best reduces germ transmission during
    routine patient contact?" · A) Alcohol-based hand rub before and after contact
    ★ · B) Wearing gloves instead · C) Rinsing hands with water only ·
    Explanation: "Hand hygiene is the single most effective transmission precaution."
  - QFREE-02 · 1pt · order 2 · "Which pulse site is routinely assessed in a stable
    adult?" · A) Carotid · B) Radial ★ · C) Femoral ·
    Explanation: "Radial is the routine peripheral site in stable adults."
  - QFREE-03 · 2pts · order 3 · "Right patient, right drug, right dose, right time:
    which 'right' is missing?" · A) Right room · B) Right chart · C) Right route ★ ·
    Explanation: "Route completes the five medication rights."
- Type SingleBestAnswer throughout. Producer UC-ADM-06. Upstream: EXAM_FREE_PRIMARY draft V1.
- Mutable: NO (version published → draft-gate locked). DIRECT_DB: NO.
- Dependents: UC-EXM-02/03/04 (free scoring proof: answer all correctly → 4/4 pass;
  miss QFREE-03 → 2/4 deterministic fail).

### EXAM_PAID_PRIMARY — paid + package-source consumption — CANONICAL_REUSABLE_RESOURCE

- Purpose: grant-gated standalone journeys AND package exam provenance (shared
  immutable published V1 — see reuse reasoning §8). Values: Slug
  `test-exam-paid-primary`, IsFree=false, Duration 60, Passing 60, else as FREE.
- Producer UC-ADM-04/05. Upstream: CAT_PRIMARY. Lookup: slug. Mutable: NO.
- DIRECT_DB: NO. Dependents: UC-EXM-01/02 (paid path), UC-COM-01/02, UC-PP-03/05,
  UC-ADM-08 chain (package version binds this exact version).
- Isolation constraint: no concurrent in-progress sessions on this version across
  sources for the same nurse (DA10 uniqueness); finalize before switching
  standalone↔package. Cleanup: preserve.

### QUESTION_SET_PAID — exam content — CANONICAL_REUSABLE_RESOURCE

- Purpose: deterministic scored content for EXAM_PAID_PRIMARY V1 (max 4 pts).
  - QPAID-01 · 1pt · order 1 · "Which intervention most directly prevents falls in an
    at-risk patient?" · A) Bedside hourly rounding ★ · B) Dim night lighting ·
    C) Raised bed rails alone · Expl: "Rounding with needs assessment prevents falls."
  - QPAID-02 · 1pt · order 2 · "A clean wound shows pink granulation tissue. Best
    next action?" · A) Apply antiseptic scrub · B) Protect with moist dressing ★ ·
    C) Leave open to air · Expl: "Moist environments support granulation."
  - QPAID-03 · 2pts · order 3 · "Which charting principle keeps documentation
    legally sound?" · A) Chart before care · B) Objective, timely entries ★ ·
    C) Copy prior shift notes · Expl: "Objective timely charting is defensible."
- Same mechanics as QUESTION_SET_FREE. Dependents: paid scoring + package report
  topic mapping (assignments in PROFILE_PRIMARY map these questions to topics).

### PRODUCT_EXAM_PRIMARY — commerce setup — CANONICAL_REUSABLE_RESOURCE

- Purpose: purchasable standalone access to EXAM_PAID_PRIMARY. Values:
  Type ExamAccess, Currency `USD`, UnitAmountMinor `4900`, IsActive=true.
- Producer UC-ADM-07. Upstream: EXAM_PAID_PRIMARY (published). Identity: ADMIN_A.
- Lookup: product list filtered by examId (unique per Type+ExamId — verified index).
- State: Active. Mutable: NO (archive/restore only via lifecycle tests on ephemeral
  copies). DIRECT_DB: NO. Dependents: UC-COM-01/02/03/04/05/06.
- Cleanup: preserve.

### TOPIC_PRIMARY_A / TOPIC_PRIMARY_B — reporting taxonomy — CANONICAL_REUSABLE_RESOURCE (×2)

- Purpose: two topics so reports prove per-topic breakdown (one topic cannot).
  Values: Category CAT_PRIMARY; A: Name `Test Topic A — Infection Control`,
  Slug `test-topic-a`; B: Name `Test Topic B — Safe Medication`,
  Slug `test-topic-b`. Lookup: `(ExamCategoryId,Slug)` unique.
- Producer: reporting-topics admin (UC-ADM-08). Mutable: NO (archive only).
- DIRECT_DB: NO. Dependents: PROFILE_PRIMARY, MATERIAL_PRIMARY, COLLECTION_PRIMARY,
  UC-PP-05 assertions. Cleanup: preserve.

### PROFILE_PRIMARY — reporting profile publication — CANONICAL_REUSABLE_RESOURCE

- Purpose: binds EXAM_PAID_PRIMARY V1 questions to topics (package-eligibility
  prerequisite). Values: Name `Test Primary Profile`; assignments:
  QPAID-01→A, QPAID-02→A, QPAID-03→B. State: Published (immutable).
- Producer: reporting-profiles admin (UC-ADM-08). Upstream: EXAM_PAID_PRIMARY V1 +
  TOPIC_*.. Lookup: admin list by examVersionId. Mutable: NO. DIRECT_DB: NO.
- Dependents: PACKAGE_VERSION_PRIMARY (required ref), UC-PP-05. Cleanup: preserve.

### MATERIAL_PRIMARY — study material — CANONICAL_REUSABLE_RESOURCE

- Purpose: report guidance target. Values: Title `Test Primary Material`,
  Slug `test-material-primary`; published version V1: MaterialType FormattedText,
  synthetic content, ReportingTopicIds [A, B]. State: identity Active + V1 Published.
- Producer: materials admin (UC-ADM-08). Mutable: NO (new versions only on ephemeral
  copies). DIRECT_DB: NO. Dependents: PACKAGE_VERSION_PRIMARY, UC-PP-05 guidance.
- Cleanup: preserve.

### COLLECTION_PRIMARY — practice collection — CANONICAL_REUSABLE_RESOURCE

- Purpose: practice bank with immediate feedback (2 items, original synthetic):
  - PPRAC-01 (Topic A, order 1): "When should hand hygiene occur around patient
    contact?" · A) Before and after ★ · B) Only after · C) Only when visibly soiled ·
    Feedback: "Both moments break transmission."
  - PPRAC-02 (Topic B, order 2): "Which check precedes every drug administration?" ·
    A) Room number · B) Two patient identifiers ★ · C) Meal timing ·
    Feedback: "Two identifiers confirm right patient."
- State: collection Active + version V1 Published. Producer: practice-collections
  admin (UC-ADM-08). Mutable: NO. DIRECT_DB: NO. Dependents: PACKAGE_VERSION_PRIMARY,
  UC-PP-04. Cleanup: preserve.

### PACKAGE_DEF_PRIMARY — package definition — CANONICAL_REUSABLE_RESOURCE

- Purpose: stable package identity. Values: Country US, Category CAT_PRIMARY,
  Title `Test Primary Package`, Slug `test-package-primary`.
- Producer: package-definitions admin (UC-ADM-08). Lookup: slug. Mutable: NO.
- DIRECT_DB: NO. Dependents: PACKAGE_VERSION_PRIMARY. Cleanup: preserve.

### PACKAGE_VERSION_PRIMARY — package version — CANONICAL_REUSABLE_RESOURCE

- Purpose: the purchasable content revision. Refs: ExamVersion EXAM_PAID_PRIMARY V1 +
  PROFILE_PRIMARY + COLLECTION_PRIMARY V1 + [MATERIAL_PRIMARY V1] + isolation
  confirmed. State: Published (immutable). Producer: package-versions admin + publish
  (UC-ADM-08; validation must be clean). Mutable: NO. DIRECT_DB: NO.
- Dependents: OFFER_PRIMARY, UC-PP-02/03/04/05. Cleanup: preserve.

### OFFER_PRIMARY — sellable offer — CANONICAL_REUSABLE_RESOURCE

- Purpose: what Anonymous browses and Nurse buys. Values: Title `Test Primary Offer`,
  Slug `test-offer-primary`, Price USD 9900, AccessDuration 90 days. State: Active.
- Producer: offers admin + activate (UC-ADM-08). Lookup: public catalog by slug.
- Mutable: NO (activate/deactivate only on ephemeral copies). DIRECT_DB: NO.
- Dependents: UC-PP-01/02, UC-COM-02 (package path), UC-COM-06. Cleanup: preserve.

### PROFILE_NURSE_A — nurse baseline content — CANONICAL_REUSABLE_RESOURCE

- Purpose: deterministic recruitable nurse state (identity-owned, provisioned via real
  flows at setup). Values: Headline `Test nurse profile`, Years 5,
  IsAvailableForRecruitment true, LicenseCountry US, Languages [EN/Fluent],
  Skills [`wound-care`, `triage`]. Identity: NURSE_A.
- Creation: UC-NUR-01/05/06 real flows during provisioning. Mutable during tests: YES
  with snapshot-restore (registry rule). Upstream: REF_COUNTRY_US, REF_LANGUAGE_EN.
  DIRECT_DB: NO. Dependents: UC-NUR-01..08 baseline, UC-REC-01/02 (availability source).
- Cleanup: snapshot-restore after mutation.

### PROFILE_EMPLOYER_A — employer baseline content — CANONICAL_REUSABLE_RESOURCE

- Purpose: deterministic employer state. Values: JobTitle `Test Hiring Manager`,
  Department `Test Nursing Recruitment`; Org Name `Test Care Hospital`,
  Country US, City `Test City`. Identity: EMPLOYER_A.
- Creation: UC-EMP-01/02 real flows during provisioning. Mutable: YES with restore.
  DIRECT_DB: NO. Dependents: UC-EMP-01/02, UC-REC-02/03 (request attribution).
- Cleanup: snapshot-restore.

## 4. PER_RUN_EPHEMERAL (5)

### EPH_ORDER — order — PER_RUN_EPHEMERAL

- Purpose: per-purchase order (product or package path). No stable values; created via
  UC-COM-02 real flow per run. Runtime: order ID captured from 201 response.
- Upstream: PRODUCT_EXAM_PRIMARY or OFFER_PRIMARY. Identity: NURSE_A.
- Mutable: terminal transitions only via UC-COM-04/05/06. DIRECT_DB: NO.
- Dependents: UC-COM-03/04/05/06. Cleanup: cancel if still pending; Paid rows remain
  (uniqueness per run, no collision). Reusable: NO.

### EPH_EXAM_SESSION — exam session — PER_RUN_EPHEMERAL

- Purpose: per-attempt session (standalone or package). Created via UC-EXM-02/PP-03
  per run; finalized via UC-EXM-04. Runtime: session ID from start response.
- Upstream: EXAM_FREE/PAID_PRIMARY version (or package entitlement). Identity: NURSE_A.
- Mutable: answers/autosave/submit only (normal flow). DIRECT_DB: NO.
- Dependents: UC-EXM-03/04/05/06, UC-PP-03/05. Cleanup: submit to terminal; expiry as
  backstop (system). Reusable: NO (one-shot attempt semantics).

### EPH_CONTACT_REQUEST — contact request — PER_RUN_EPHEMERAL

- Purpose: per-test recruitment request. Created via UC-REC-02 per run
  (EMPLOYER_A → NURSE_A). Runtime: request ID from 201 + Location.
- Upstream: PROFILE_NURSE_A availability. DIRECT_DB: NO.
- Dependents: UC-REC-03/04. Cleanup: none needed (terminal states are per-row).
- Reusable: NO (decisions are terminal).

### EPH_ADMIN_CATEGORY — category lifecycle target — PER_RUN_EPHEMERAL

- Purpose: isolated target for UC-ADM-03 create/edit/archive/restore/delete tests.
  Values per run: Name `Test Run Category <runtag>`, Slug `t-cat-<runtag>`, Country US.
- Producer UC-ADM-03 (the SUT itself). Identity: ADMIN_A. DIRECT_DB: NO.
- Dependents: UC-ADM-03. Cleanup: delete if still draft (legal); archived rows remain
  uniquely slugged. Reusable: NO (mutation is the test).

### EPH_ADMIN_EXAM — exam lifecycle target — PER_RUN_EPHEMERAL

- Purpose: isolated target for UC-ADM-04/05/06 (+ UC-ADM-07/08 publish-path variants):
  exam + draft version + questions created, validated, published/retired/deleted per run.
  Slugs `t-exm-<runtag>`. Covers D-requirement (§12D) without touching canonical exams.
- Producer: the admin UCs under test. Identity: ADMIN_A. DIRECT_DB: NO.
- Cleanup: delete-if-draft else retire; uniquely slugged rows may remain (Step 7
  Clean DB Contract owns final disposition — "leave garbage forever" is NOT the design).

## 5. MUST_CREATE_THROUGH_REAL_FLOW (2)

### GRANT_STANDALONE — ExamAccessGrant — MUST_CREATE_THROUGH_REAL_FLOW

- Purpose: standalone authorization proof for paid exams. Produced ONLY by UC-COM-06
  sandbox completion (fulfillment side-effect). Per purchase (tied to EPH_ORDER).
- Identity: NURSE_A. Upstream: EPH_ORDER + UC-COM-05 session.
- DIRECT_DB_CREATION_ALLOWED: NO (producing UC: UC-COM-06; direct creator is
  fulfillment (System) invoked by NURSE_A). Runtime: grant ID internal;
  tests assert via subsequent UC-EXM-02 success, not by reading the row.
- Dependents: UC-EXM-02 (paid path). Cleanup: none (grant rows are historical facts).

### ENTITLEMENT_PACKAGE — entitlement + 4 rights + snapshot — MUST_CREATE_THROUGH_REAL_FLOW

- Purpose: package access unit, created atomically by UC-COM-06 for package orders:
  `PackagePurchaseEntitlement(Active)` + 4 `PackageBenefitRights`
  (3 Available + report Dormant) + `PackageOrderItemSnapshot`. Per purchase.
- Identity: NURSE_A. Upstream: EPH_ORDER (package path) + OFFER_PRIMARY chain.
- DIRECT_DB_CREATION_ALLOWED: NO (producing UC: UC-COM-06; direct creator is
  fulfillment (System) invoked by NURSE_A). Tests consume via
  UC-PP-02/03/04/05; attempt consumption and expiry are asserted, never hand-set.
- Dependents: UC-PP-02/03/04/05. Cleanup: expiry is system-driven; rows are facts.

## 6. SYSTEM_GENERATED (4)

### TOKENS_AUTH — verification / reset / refresh token records — SYSTEM_GENERATED

- Purpose: one-shot token rows from UC-AUTH-02/03/05/06. Never static values; hashes
  unique per issuance (verified unique TokenHash indexes). Tests assert behavior
  (202/rotation/consume/reuse-revocation), never token literals.
- DIRECT_DB_CREATION_ALLOWED: NO. Supports: UC-AUTH-02/04/05/06/07 mechanics.

### PROGRESS_PRACTICE — practice progress rows — SYSTEM_GENERATED

- Purpose: per-item Correct/Incorrect rows from UC-PP-04 answering (overwrite-safe).
- Upstream: ENTITLEMENT_PACKAGE + COLLECTION_PRIMARY items. DIRECT_DB: NO.
- Supports: UC-PP-04 progress assertions.

### PROVENANCE_SESSION — session provenance rows — SYSTEM_GENERATED

- Purpose: Source + package snapshot chain written at session start (immutable).
- Upstream: EPH_EXAM_SESSION. DIRECT_DB: NO. Supports: UC-EXM-02, UC-PP-03/05
  source assertions.

### SCORE_RESULT — finalized score fields — SYSTEM_GENERATED

- Purpose: Score/MaxScore/Percentage/Passed written by atomic finalization
  (`ExamScoringService`) on submit/expiry. Never hand-set.
- Upstream: EPH_EXAM_SESSION + QUESTION_SET_*. DIRECT_DB: NO.
- Supports: UC-EXM-04/05/06, UC-PP-05.

## 7. SETUP_SYNTHESIZED_TEST_STATE (1)

### STATE_INACTIVE_USER — IsActive=false — SETUP_SYNTHESIZED_TEST_STATE

- Why: no deactivate-user flow exists in current HEAD (verified: no endpoint mutates
  `IsActive`; admin role-update replaces roles only). Inactive-login behavior (401
  indistinguishable) still needs coverage.
- Minimal change: flip `Users.IsActive` true→false on a dedicated setup-held account
  (never a canonical reusable identity). Security: Local/Test only; login attempts
  must fail (that IS the assertion). Reset: flip back + verify login works.
- DIRECT_DB_CREATION_ALLOWED: YES — sole justified exception (no workflow exists).
- Supports: inactive-account negatives. Cleanup: reactivate in setup teardown.

## 8. EXTERNAL_DEPENDENCY (1)

### EXT_PROD_PAYMENT — production provider completion data — EXTERNAL_DEPENDENCY

- Purpose: boundary marker. Production provider selection is DEFERRED (UC-COM-07);
  no provider IDs, checkout state, or webhook data may be fabricated.
- Sandbox path (UC-COM-06) is the only completable flow and is cataloged above.
- DIRECT_DB_CREATION_ALLOWED: NO. Supports: nothing today.

## 9. UC → DATA coverage matrix (38 data-requiring human UCs)

AUTH UCs (9) need no business data (identity-owned) — intentionally absent here.
UC-ADM-01/02 operate on identities, not business data — absent here.

| UC | Required Data IDs |
|---|---|
| UC-NUR-01/02/03 | PROFILE_NURSE_A (baseline), REF_COUNTRY_US |
| UC-NUR-04 | PROFILE_NURSE_A |
| UC-NUR-05 | PROFILE_NURSE_A, REF_LANGUAGE_EN |
| UC-NUR-06/07/08 | PROFILE_NURSE_A |
| UC-EMP-01/02 | PROFILE_EMPLOYER_A |
| UC-REC-01 | PROFILE_NURSE_A, PROFILE_EMPLOYER_A, REF_COUNTRY_US, REF_COUNTRY_GB, REF_LANGUAGE_EN |
| UC-REC-02/03/04 | EPH_CONTACT_REQUEST (+ PROFILE_NURSE_A availability, PROFILE_EMPLOYER_A attribution) |
| UC-EXM-01 | EXAM_FREE_PRIMARY or EXAM_PAID_PRIMARY, CAT_PRIMARY, REF_COUNTRY_US |
| UC-EXM-02 free | EXAM_FREE_PRIMARY, QUESTION_SET_FREE, EPH_EXAM_SESSION, PROVENANCE_SESSION |
| UC-EXM-02 paid | EXAM_PAID_PRIMARY, QUESTION_SET_PAID, GRANT_STANDALONE, EPH_EXAM_SESSION |
| UC-EXM-03/04 | EPH_EXAM_SESSION, QUESTION_SET_* (per exam), SCORE_RESULT |
| UC-EXM-05/06 | Prior EPH_EXAM_SESSIONs + SCORE_RESULTs |
| UC-PP-01 | OFFER_PRIMARY (Anonymous browse) |
| UC-PP-02 | OFFER_PRIMARY, ENTITLEMENT_PACKAGE |
| UC-PP-03 | ENTITLEMENT_PACKAGE, EXAM_PAID_PRIMARY version, EPH_EXAM_SESSION, PROVENANCE_SESSION |
| UC-PP-04 | ENTITLEMENT_PACKAGE, COLLECTION_PRIMARY, PROGRESS_PRACTICE |
| UC-PP-05 | Finalized package EPH_EXAM_SESSION, SCORE_RESULT, PROFILE_PRIMARY, TOPIC_PRIMARY_A/B, MATERIAL_PRIMARY, COLLECTION_PRIMARY |
| UC-COM-01 | PRODUCT_EXAM_PRIMARY |
| UC-COM-02 standalone | PRODUCT_EXAM_PRIMARY, EPH_ORDER |
| UC-COM-02 package | OFFER_PRIMARY, PACKAGE_VERSION_PRIMARY, EPH_ORDER |
| UC-COM-03/04 | EPH_ORDER |
| UC-COM-05 | EPH_ORDER (pending) |
| UC-COM-06 | EPH_ORDER, GRANT_STANDALONE and/or ENTITLEMENT_PACKAGE |
| UC-ADM-03 | EPH_ADMIN_CATEGORY (CAT_PRIMARY is the read-reference pattern, never the mutation target) |
| UC-ADM-04/05/06 | EPH_ADMIN_EXAM (EXAM_*_PRIMARY are read/consume references only) |
| UC-ADM-07 | EPH_ADMIN_EXAM as subject exam; PRODUCT_EXAM_PRIMARY as read-reference |
| UC-ADM-08 | EPH_ADMIN_EXAM chain following the canonical pattern (topics→profile→material→ collection→definition→version→offer); canonical chain (TOPIC_*/PROFILE_/MATERIAL_/ COLLECTION_/PACKAGE_*/OFFER_PRIMARY) is the correctness oracle, never the mutation target |
| UC-REF-01 | REF_COUNTRY_US/GB, REF_LANGUAGE_EN |

## 10. DATA REUSE MATRIX (anti-proliferation review)

| Data ID | UC families reusing | Mutable? | Per-run? | Why no second resource |
|---|---|---|---|---|
| REF_COUNTRY_US/GB, REF_LANGUAGE_EN | NUR, REC, EXM, ADM, REF (5) | NO | NO | Seed is free and stable; codes are lookup keys |
| CAT_PRIMARY | EXM, PP-chain, COM, ADM (4) | NO | NO | One root suffices; slug unique per country |
| EXAM_FREE_PRIMARY + QUESTION_SET_FREE | EXM free (1 family) | NO | NO | Single free exam covers availability/presentation/submission/scoring |
| EXAM_PAID_PRIMARY + QUESTION_SET_PAID | EXM paid, COM, PP exam/report (3) | NO | NO | Paid exam doubles as package source (immutable V1 shared; no-concurrent-in-progress constraint §3); a separate EXAM_PACKAGE was considered and REJECTED — sharing is safe, splitting would duplicate 6 questions + version for zero behavioral difference |
| PRODUCT_EXAM_PRIMARY | COM (1) | NO | NO | One active product proves browse/order/checkout/fulfill |
| TOPIC_PRIMARY_A/B | PP setup + report (1 chain) | NO | NO | Two is the minimum for per-topic breakdown; one topic could not prove it |
| PROFILE_PRIMARY | Package publish + report (1) | NO | NO | Single binding profile is the exact eligibility prerequisite |
| MATERIAL_/COLLECTION_PRIMARY | Package publish + practice/report (1) | NO | NO | One of each proves inclusion + guidance mapping |
| PACKAGE_DEF_/VERSION_/OFFER_PRIMARY | PP + package-COM (2) | NO | NO | One coherent chain; second package rejected (no multi-package behavior in v1 scope beyond coexistence, which per-run orders can prove) |
| PROFILE_NURSE_A / PROFILE_EMPLOYER_A | NUR/REC, EMP/REC (2 each) | Snapshot-restore only | NO | Identity-owned baselines, not duplicable fixtures |
| EPH_ORDER / EPH_EXAM_SESSION / EPH_CONTACT_REQUEST | COM, EXM/PP, REC | Flow-only | YES | One-shot/terminal semantics forbid reuse |
| EPH_ADMIN_CATEGORY / EPH_ADMIN_EXAM | ADM lifecycle | YES (the test) | YES | Draft-gated writes + draft-only delete force isolation |
| GRANT_STANDALONE / ENTITLEMENT_PACKAGE | EXM paid, PP | Consumed by flow | Per purchase | Fulfillment-derived by design; baselining them would bypass the SUT |
| TOKENS_AUTH | AUTH token mechanics (4 UCs) | NO (append-only rows) | Per-run rows | One record class, never static values |
| PROGRESS_PRACTICE / PROVENANCE_SESSION / SCORE_RESULT | PP-04, EXM-02/PP-03, EXM-04/PP-05 | Flow-only | Per session | App-written consequences |
| STATE_INACTIVE_USER | AUTH inactive negatives | Setup flip only | NO (reused flag) | Sole synthesized state; no product flow exists |
| EXT_PROD_PAYMENT | none today | n/a | n/a | Deferred boundary, fabrication forbidden |

Rejected duplicates: EXAM_PACKAGE_* (shared immutable version suffices); second
category/product/offer (no v1 behavior needs them); pre-paid order baseline
(orders are per-run by business model); seeded Approved/Rejected requests
(lifecycle must flow); per-test countries/languages (seed covers).

## 11. WORKFLOW-DERIVED STATE — DO NOT BASELINE-SEED

Verified against implementation: verification/reset/refresh token rows; payment
orders; checkout sessions; Paid transition; ExamAccessGrant; PackagePurchaseEntitlement
+ rights + snapshot; exam sessions + answers; finalized score/result; analytical
reports + topic/guidance rows; practice progress; provenance rows; contact-request
statuses (Pending→Approved/Rejected/Cancelled); ownership links (profile↔user,
request↔profiles, order↔nurse); `IsProfileComplete`/verified/active flags except
via their real flows (inactive is the §7 exception). Seeding any of these would
assert states the workflows were never proven to produce.

## 12. Step 2 bookkeeping resolution (§25)

Gate 2 reported "6 identities but 7 values": the seventh value (`Test-Lc-Run-07`)
is the documented per-run ephemeral derivation template for SP-NEW-UNVERIFIED /
SP-VERIFIED-INCOMPLETE (`test-identity-registry.md` §4) — it is NOT a seventh
canonical identity. No identity-model defect; NO change to the Step 2 registry
required or made.
