# Page 09 — User Flows Fix Tracker

> Recommended in-project path: `docs/design/reviews/page-09-user-flows-fix-tracker.md`

## Status Legend
- [ ] Not started
- [~] In progress
- [x] Complete
- [!] Blocked / needs review

---

## Goal
Bring **Page 09 — User Flows** to a reviewable, export-verified, design-complete state.

---

## Review Sources
- `User Flows — Nursing Platform v1.pdf`
- exported page image
- `page-09-user-flows-audit.md`

---

## Task Checklist

### A. Cleanup and baseline
- [x] Remove visible internal marker `USER-FLOW-BUILD-COMPLETE:1`.
- [x] Freeze the approved visual grammar for flow diagrams (cards, arrows, branch labels, direction, spacing).
- [x] Confirm `Authentication Entry` as the reference section after cleanup.

### B. Section-by-section rebuild
- [x] Section 1 — Flow Conventions reviewed.
- [x] Section 2 — Authentication Entry reviewed; standardize readability and visual grammar.
- [x] Section 3 — Nurse Registration: rebuild as actual directional flow.
- [x] Section 4 — Employer Registration: rebuild as actual directional flow.
- [x] Section 5 — Email Verification: rebuild as actual directional flow.
- [x] Section 6 — Password Recovery: rebuild as actual directional flow and attach branch labels.
- [x] Section 7 — Session and Access Failures: rebuild as actual directional flow.
- [x] Section 8 — Authenticated Entry Routing: rebuild as actual directional flow.
- [x] Section 9 — Authentication Screen Set: simplify and remove duplication/clutter.
- [x] Section 10 — First Design Batch: verify after section 9 cleanup.

### C. Logic verification
- [x] Confirm no frontend-only behavior contradicts backend authority.
- [x] Confirm no permission keys, raw tokens, or unsafe details are exposed.
- [x] Confirm no misleading success path is shown before real verification/confirmation.
- [x] Confirm shared flows are either fully detailed or clearly delegated.

### D. Export verification
- [x] Export page 09 at scale 1.
- [x] Verify all sections are visible.
- [x] Verify all flow content is readable.
- [x] Verify all arrows/branch labels are visually attached.
- [x] Verify no overlap/overlay/clipping/containment issues.
- [x] Verify no stray internal markers or temporary text remain.

### E. Completion gate
- [x] Mark page 09 complete only after all section-level and export-level checks pass.

---

## Notes for Agent
1. Do **not** trust completion markers alone.
2. Trust only the combination of:
   - structural inspection,
   - visual export,
   - and business-logic review.
3. If a section is only a card inventory, it is **not complete**.
4. If branch labels exist without visible branches, it is **not complete**.
5. If a section routes into a shared flow, make that delegation explicit.

---

## Final Independent PDF Review Corrections

- Rebuilt Nurse Registration, Employer Registration, Email Verification, Password Recovery, Session and Access Failures, and Authenticated Entry Routing as explicit card-and-text-arrow flows with decision labels adjacent to their destinations.
- Corrected Existing Email paths to route to Sign In only; removed the incorrect Authenticated Employer Entry to Sign In route and added an explicit shared Email Verification delegation into Authenticated Employer Entry.
- Rebuilt Password Recovery and Session and Access Failures branch areas to remove the reviewed overlap and incorrect refresh/access relationships.
- Rebuilt role-routing branches so each originates from Role Resolution without floating labels or outcome chains.
- Historical correction pass completed on the deleted pre-recreation board. That board and its export are superseded by the later `Page 09 Recreation From Scratch` evidence below.

---

## Page 09 Recreation From Scratch

**Date:** 2026-07-22
**Page ID:** `5b727796-97b4-8084-8008-5d7f57f4a1bd`
**Main board ID:** `5b727796-97b4-8084-8008-5d7f58eeb357`

### Section Checkpoints

- [x] Flow Conventions — ID `5b727796-97b4-8084-8008-5d80b404ae35`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Authentication Entry — ID `9bd8100d-da35-8029-8008-5d85d9e66d38`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Nurse Registration — ID `9bd8100d-da35-8029-8008-5d86259b94bd`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Employer Registration — ID `9bd8100d-da35-8029-8008-5d86d25c339b`; structural verification passed after correcting the shared-success outcome label; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Email Verification — ID `9bd8100d-da35-8029-8008-5d872c345045`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Password Recovery — ID `9bd8100d-da35-8029-8008-5d876562e514`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Session and Access Failures — ID `9bd8100d-da35-8029-8008-5d87a5e74feb`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Authenticated Entry Routing — ID `9bd8100d-da35-8029-8008-5d880b7f3b16`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] Authentication Screen Set — ID `9bd8100d-da35-8029-8008-5d886506a108`; structural verification passed after replacing multiline inventory text with individual native text lines; inline PNG export reviewed at scale 1 with no unresolved visual findings.
- [x] First Design Batch — ID `9bd8100d-da35-8029-8008-5d88b2d0292c`; structural verification passed; inline PNG export reviewed at scale 1 with no unresolved visual findings.

### Final Verification

- [x] Page 09 exists exactly once at index 9, immediately after `08 — Product Map` at index 8.
- [x] Main board is `1920 × 8280`; ten 1728px-wide sections are aligned at x=96 with consistent 64px gaps and 80px final bottom padding.
- [x] Final full-board PNG export (`1920 × 8280`, scale 1) reviewed with no unresolved visual findings.
- [x] Final structural counts: Page 09 `1`; main board `1`; required/direct-child sections `10/10`; missing/duplicate sections `0/0`; blank cards/text/whitespace-only text `0/0/0`; paths/raw SVG/SVG text/outlined text/non-Noto text `0/0/0/0/0`; detached arrows/outcome labels `0/0`; overlaps/containment violations/clipped visible text `0/0/0`; internal markers `0`; missing/duplicate AUTH IDs `0/0`; product screen boards `0`; raw token values `0`; backend permission keys `0`; unsupported business rules `0`.
- [x] Protected pages 00–08 remained unchanged: final shape counts match the pre-mutation baseline for all nine pages; modified protected-page shape count `0`.
- [x] Unresolved findings: none.
