# Cross-Actor & Scenario Dependency Graph (Step 4)

> Authority: `system-use-case-catalog.md` (54 UCs), `test-identity-registry.md`
> (6 identities), `test-data-catalog.md` (32 Data IDs); implementation is ultimate
> authority. Documentation only — no provisioning, no scenarios (Step 5), no tests.
> Edge format: Upstream Actor → Upstream UC → Produced Entity/State →
> Downstream Actor → Downstream UC → Missing-Prerequisite Behavior.
> Identity/Data IDs are references to Steps 2/3, not new definitions.

## F0. Seed foundation (System → everyone)

| Edge | Upstream → UC → Produced state → Downstream → UC → Missing behavior |
|---|---|
| D-00a | System → UC-SYS-01 → roles/permissions (REF seed), REF_COUNTRY_*, REF_LANGUAGE_EN, bootstrap Admin → All actors → all UCs → Missing: boot/seed absent breaks everything (sign-up throws without Nurse role; guards 403 without grants) |
| D-00b | System → UC-SYS-03 → EmailVerificationToken / PasswordResetToken rows → Anonymous → UC-AUTH-04 / UC-AUTH-07 → Missing: absent/used/expired token → 409; no inbox assertion allowed |
| D-00c | System → UC-SYS-02 → lazy expiry + atomic finalization (tokens/orders/sessions/entitlements/rights) → All consumers → lifecycle UCs → Missing/expired window → 409 paths (order Expired, session auto-finalize, entitlement Locked) |

## F1. Authentication / account lifecycle → protected workflows

| Edge | Chain → Missing behavior |
|---|---|
| D-01 | Anonymous → UC-AUTH-03 → User(Nurse, active, !verified) + NurseProfile + token → Anonymous → UC-AUTH-04 → EmailVerified → UC-AUTH-01 → session (NURSE_A/LC/B) → Missing: correct-creds-but-unverified → 403 `email_verification_required`; duplicate sign-up → silent 202, no mutation |
| D-02 | Nurse → UC-AUTH-01 session → UC-AUTH-08 names → IsProfileComplete → UC-AUTH-10 guards unlock `/nurse/**`, `/checkout`, `/commerce/orders*`, `/account` → Missing: incomplete → forced `/onboarding/profile`; anonymous → `/auth/sign-in?returnUrl=`; denied → `/access-denied` |
| D-03 | Nurse → UC-AUTH-06 → reset token → UC-AUTH-07 → new hash + ALL refresh revoked → UC-AUTH-01 re-login (NURSE_LC round-trip, restore canonical) → Missing: reused/expired token → 409; old password → 401 |
| D-04 | Administrator → UC-ADM-02 → User + Employer/Admin roles → Employer → UC-EMP-01/02 + UC-REC-*; Administrator → UC-ADM-* (public sign-up is Nurse-only, so this is the ONLY Employer/Admin path) → Missing: wrong role → 403 from Nurse/EmployerRoleGuard; SuperAdmin not assignable |
| D-05 | Auth → UC-AUTH-02 rotation sustains UC-AUTH-10 sessions → Missing: refresh reuse → revoke-all → forced re-login |

## F2. Exam setup (Admin) → Nurse consumption

| Edge | Chain → Missing behavior |
|---|---|
| D-06 | Administrator → UC-ADM-03 → CAT_PRIMARY(Active) → UC-ADM-04 → Exam(Draft) → UC-ADM-05 → Version(Draft) → UC-ADM-06 → QUESTION_SET_* → validate clean → publish → Version(Published, immutable) → Missing: validation issues → publish blocked 409; writes to non-draft → rejected (draft gate); delete allowed draft-only |
| D-07 | Administrator → published version → Nurse → UC-EXM-01 catalog/detail (published-only filter) → Missing: unpublished → invisible / 404 |
| D-08 | Nurse → UC-EXM-02 start → EPH_EXAM_SESSION(InProgress) + snapshot + PROVENANCE_SESSION → UC-EXM-03 autosave → UC-EXM-04 submit → SCORE_RESULT(Submitted) → UC-EXM-05/06 history/analytics → Missing: paid without GRANT_STANDALONE → 403 requiresPurchase; cross-source in-progress on same version → 409 (DA10); premature result/review → 409; no flag/unflag or clear-selection exists in HEAD |

## F3. Commerce → fulfillment → paid access

| Edge | Chain → Missing behavior |
|---|---|
| D-09 | Administrator → UC-ADM-07 → PRODUCT_EXAM_PRIMARY(Active, published exam) → Nurse → UC-COM-01 browse → UC-COM-02 → EPH_ORDER(PendingPayment) → Missing: inactive/unpublished product → 409; unknown product → 404; both/neither product+offer IDs → 400 |
| D-10 | Nurse → EPH_ORDER → UC-COM-05 checkout init → session(Created→ProviderPending) → UC-COM-06 sandbox complete (dev/test only) → Order(Paid) → Missing: concurrent init → 409 + Retry-After; provider down → 503; non-pending → 409 |
| D-11 | System → UC-COM-06 fulfillment → GRANT_STANDALONE (invoked by NURSE_A) → Nurse → UC-EXM-02 paid start → Missing: grant absent → 403; grant is fulfillment-derived — never seeded |
| D-12 | Nurse → EPH_ORDER → UC-COM-04 cancel → Cancelled (pending-only, no active checkout) → Missing: non-pending/active-checkout → 409 cancelConflict |

## F4. Package setup → entitlement → package workflows

| Edge | Chain → Missing behavior |
|---|---|
| D-13 | Administrator → UC-ADM-08 chain: TOPIC_PRIMARY_A/B → PROFILE_PRIMARY (bound to EXAM_PAID_PRIMARY V1) → MATERIAL_PRIMARY V1 → COLLECTION_PRIMARY V1 → PACKAGE_DEF_PRIMARY → PACKAGE_VERSION_PRIMARY (isolation confirmed, validation clean) → publish → OFFER_PRIMARY activate → Anonymous → UC-PP-01 visible → Missing: validation issues → publish blocked 409; inactive offer → invisible/404 |
| D-14 | Nurse → UC-COM-02 (packageOfferId) → UC-COM-06 → ENTITLEMENT_PACKAGE (Active + 4 rights + snapshot) → Nurse → UC-PP-02 view → Missing: no entitlement → 404; expired window → Locked / 409 |
| D-15 | Nurse → attempt right (Available) → UC-PP-03 start (consume → Consumed) → package EPH_EXAM_SESSION → UC-EXM-04 finalize → UC-PP-05 immutable report (persists post-expiry; failure ≠ retake, ≠ consumed right) → Missing: consumed/expired → 409; premature report → 409 notFinalized |
| D-16 | Nurse → practice right → UC-PP-04 items (learner-safe) / answer → immediate feedback + PROGRESS_PRACTICE → Missing: accessEnded → 409 |

## F5. Recruitment cross-actor loop

| Edge | Chain → Missing behavior |
|---|---|
| D-17 | Nurse → UC-NUR-01/05/06 → PROFILE_NURSE_A (available + content) → Employer → UC-REC-01 search (available-only filter) → Missing: unavailable/incomplete → invisible to search |
| D-18 | Employer → UC-REC-02 → EPH_CONTACT_REQUEST(Pending, EMPLOYER_A → NURSE_A) → Nurse → UC-REC-04 approve/reject → terminal → Employer → UC-REC-03 terminal view (or employer Pending→Cancelled) → Missing: duplicate pending → 409; illegal transition → 409; non-owned → 404 |

## F6. Role administration → downstream access

| Edge | Chain → Missing behavior |
|---|---|
| D-19 | Administrator → UC-ADM-01 list/detail → role replace (UC-ADM-01/02 subjects, e.g. NURSE_A) → guard access changes + ALL refresh tokens revoked → forced re-login → Missing: SuperAdmin not assignable via product; stale tokens → 401 |
| D-20 | System → UC-SYS-01 bootstrap → ADMIN_A → all F2/F3/F4 setup chains (D-06/D-09/D-13) → Missing: no Admin → no setup possible |

## Reference-data edge

| Edge | Chain |
|---|---|
| D-21 | System seed → UC-REF-01 lookups → UC-NUR-01/02/03/05 selects, UC-EXM-01/REC-01 filters (codes US/GB/EN as stable keys) |

## Topological order for Step 5 scenario sequencing

- L0: D-00a/b/c (seed, token mechanics available, expiry rules known).
- L1: D-01/D-04 (obtain authenticated actors) → D-02 (complete profiles) → baselines PROFILE_NURSE_A / PROFILE_EMPLOYER_A.
- L2: D-06 + D-09 + D-13 (admin setup chains: exam, product, package) — mutually order-independent except D-13 needs D-06's published version.
- L3: consumer flows D-07/D-08 (free exams), D-10/D-11 (purchase→grant→paid exam), D-14/D-15/D-16 (package), D-17/D-18 (recruitment), D-12/D-19 (cancel, role admin).
- L4 (needs finalized sessions): UC-EXM-05/06, UC-PP-05, UC-COM-03 history views.
- Negatives attach to their edge (missing-behavior column) — no separate phase.

## Coverage check

Every cross-actor UC from the catalog appears as a downstream at least once:
UC-AUTH-01/04/07/08 (D-01..03), UC-EMP-01/02 + UC-REC-01/02/03 (D-04/D-17/D-18),
UC-REC-04 (D-18), UC-EXM-01/02 (D-07/D-08/D-11), UC-PP-01/02/03/04/05
(D-13/D-14/D-15/D-16), UC-COM-01/02/03/05/06 (D-09/D-10/D-14), UC-ADM-03/04/05/06/07/08
as upstreams (D-06/D-09/D-13). Same-actor chains (EXM-03→04→05/06, COM-02→03/04)
are ordered inside their edges. UC-COM-07 (DEFERRED) and UC-SYS-* have no
downstream by design. No orphan edges; no invented flows.
