# Business Feature Delta Report

## 1. Comparison scope

- **Base ref:** `origin/main` (`ac6d7c02e18bd79efdcb528f309fef5fc9ca36cb`)
- **Feature ref:** `origin/feature/preparation-package-foundation`
- **Feature-side mode:** remote feature branch
- **Local HEAD:** `1c22b59 docs: reconcile frontend design re-entry baseline`
- **Comparison commits:** 92
- **Diff size:** 185 files changed, 49,487 insertions, 628 deletions.

The remote feature branch exists, so this is a remote-to-remote comparison rather than a local fallback.

## 2. Executive summary

Since `origin/main`, the platform gained an end-to-end backend Preparation Package capability. Administrators can author and publish the package catalog and sellable offers; nurses can purchase package offers through the existing Sandbox payment path, receive time-bounded package rights, practice package content, start a package-scoped exam attempt, and read a package analytical report. Existing free and standalone paid-exam behavior is explicitly preserved. Frontend implementation has not been added.

## 3. Business features added

### Preparation Package catalog and administration — Backend complete / frontend pending

- **Purpose:** Let administrators manage reporting topics/profiles, materials, practice collections, immutable package composition, and offers.
- **Roles:** Admin; public catalog visitors.
- **Evidence:** `2789e76`, `2ecc0e1`, `5ce67c4`, `8ba4c2a`, `9a82ee7`, `3f3cd66`, `7cd06e9`, `a65da22`, `8439511`; `TASKS.md` Stage 1 items 288–311 are checked.
- **API readiness:** Public offer catalog plus dedicated admin endpoint families for topics, profiles, materials, practice collections, package definitions/versions, and offers.

### Study materials and practice collections — Backend complete / frontend pending

- **Purpose:** Provide reusable, versioned preparation content linked to Reporting Topics.
- **Roles:** Admin authors; nurses receive package-scoped practice access.
- **Evidence:** `b85059c`, `b311f1f`, `695cfbf`, `b6b3d2e`, `90af444`, `47f661e`, `8b46581`, `a4b28bb`.
- **Business controls:** Published versions are immutable; draft/retired content is excluded from new package publication or sale; practice content remains separate from protected official exam content.

### Offers, payment, fulfillment, and entitlements — Backend complete / frontend pending

- **Purpose:** Sell a package offer and convert successful payment into a package entitlement with materials, practice, attempt, and report rights.
- **Roles:** Nurse, system, admin.
- **Evidence:** `118d4d8`, `137e5d2`, `68fc266`, `155c1d5`, `56de439`, `e9b4fc1`; `TASKS.md` Stage 2 checklist is checked.
- **Business controls:** Immutable purchased-offer snapshots, idempotent fulfillment, active same-package repurchase blocking, and independent coexistence with standalone exam access.

### Package practice progress — Backend complete / frontend pending

- **Purpose:** Let nurses answer and re-answer package practice items while access is active, then retain historical owner reads after expiry.
- **Roles:** Nurse.
- **Evidence:** `a90074c`, `7f54e49`, `6c11e61`, `0454c0f`, `cffc846`, `8b964ae`.
- **Business controls:** Correct/incorrect/unanswered distinction; retries do not consume the package exam attempt; practice progress is excluded from analytical report evidence.

### Package exam attempt and session provenance — Backend complete / frontend pending

- **Purpose:** Give each entitlement a package-scoped exam attempt without changing free or standalone exam flows.
- **Roles:** Nurse, system.
- **Evidence:** `921224c`, `9286927`, `ed6d30f`, `b25986b`, `66efaeb`, `798364e`.
- **Business controls:** Explicit entitlement selection, atomic attempt consumption, idempotent in-progress resume, source-conflict handling, and one in-progress session per nurse/exam version.

### Analytical reports — Backend complete / frontend pending

- **Purpose:** Provide a nurse-owned, immutable diagnostic report for qualifying package exam sessions.
- **Roles:** Nurse, system.
- **Evidence:** `b1a9c5b`, `3a8b84d`, `32451ec`, `12d247d`, `3058604`, `84ca844`.
- **Business controls:** Lazy/idempotent generation, recovery/concurrency protection, post-expiry reading, and deterministic purchased-content guidance without protected question/answer exposure.

### Authorization and API readiness — Backend complete / frontend pending

- **Purpose:** Protect administration with dedicated permissions and expose nurse-owned package APIs safely.
- **Roles:** Admin, nurse, anonymous catalog visitor.
- **Evidence:** `17e38eb`, `16a3f4e`, `9a82ee7`, `8aa5d50`; Application and WebApi tests in the comparison range.

### Frontend/design governance — Documentation only

- **Purpose:** Establish Penpot authority and design-program governance for later frontend work.
- **Roles:** Design/engineering governance.
- **Evidence:** `1c22b59`; `docs/frontend/design/*`.
- **Status:** Frontend runtime remains uninitialized; governance does not constitute Angular implementation.

## 4. What is completed

Repository status evidence identifies Preparation Package Stages 1–4 as complete for the approved backend scope. “Complete” means the relevant `TASKS.md` checklists are checked and the current-task/status records describe implementation, persistence, WebApi behavior, tests, and compatibility preservation. It does not mean a commercial launch configuration or frontend product experience exists.

## 5. What is still incomplete or deferred

- Angular/frontend UI implementation is not started.
- Material storage provider, upload/download/delivery, and offline access are deferred.
- Workspace/dashboard aggregation is deferred unless separately completed.
- Adaptive practice and spaced repetition/retraining are deferred.
- Employers cannot view package practice progress, reports, or package purchase history in v1.
- `TASKS.md` still shows broader roadmap gaps including production payment provider/webhook/reconciliation/refunds/subscriptions, Administration Phase items, Frontend Phase items, and Production Readiness items.

## 6. Backend/API readiness

### Ready for Figma/design

Admin reporting topics/profiles/materials/practice collections/package composition/offers; nurse entitlement reads and practice progress; package exam-session start; package analytical-report read.

### Ready for Angular implementation after DTO/OpenAPI confirmation

The same implemented endpoint families are suitable for Angular planning once the current generated OpenAPI and exact DTO, validation, permission, and Problem Details contracts are confirmed.

### Not ready / deferred

Storage and material delivery, offline access, workspace aggregation, adaptive/retraining flows, and employer package visibility.

## 7. Risks and cautions before push or merge

- The local branch has no configured upstream, although the matching remote feature branch now exists.
- Current worktree files are uncommitted: `docs/frontend/frontend-architecture.md`, `docs/design/screens/authentication/auth-001-sign-in-tracker.md`, `docs/frontend/design-system-audit.md`, and two local report files.
- Reports are local-only until intentionally committed.
- Backend completion does not authorize or prove frontend completion.
- Frontend design governance remains in a re-entry/G0 review state; current contracts must be confirmed before Angular work.

## 8. Commit summary

- **Stage 1 catalog:** `2789e76`, `2ecc0e1`, `5ce67c4`, `8ba4c2a`, `9a82ee7`.
- **Stage 1 safeguards/status:** `3f3cd66`, `7cd06e9`, `b85059c`, `b311f1f`, `a65da22`, `8439511`.
- **Stage 2 commerce/rights:** `2ed74ef`, `118d4d8`, `137e5d2`, `68fc266`, `155c1d5`, `56de439`, `e9b4fc1`.
- **Stage 3 package attempts:** `921224c`, `9286927`, `ed6d30f`, `b25986b`, `66efaeb`, `798364e`.
- **Stage 4 reports:** `b1a9c5b`, `3a8b84d`, `32451ec`, `12d247d`, `3058604`, `84ca844`.
- **Practice progress:** `a90074c`, `7f54e49`, `6c11e61`, `0454c0f`, `cffc846`, `8b964ae`.
- **Design governance:** `1c22b59`.

## 9. Changed areas

- **Backend:** Domain, Application, Infrastructure migrations/configuration, WebApi mappings/middleware.
- **Tests:** Domain, Application, Infrastructure including PostgreSQL concurrency, and WebApi integration tests.
- **Docs/status/specs/plans:** Preparation Package Stage 1–4 records, architecture/API/database documentation, task status.
- **Frontend/design governance:** architecture/design-governance records; no Angular application code.
- **Reports:** local untracked Git-push delta report and this business report.

## 10. Final conclusion

The branch adds a complete approved-scope backend Preparation Package journey: catalog authoring, sale/fulfillment, entitlement-scoped preparation, package attempt, and nurse report access. The Preparation Package backend is complete for the recorded Stage 1–4 scope; frontend work is not complete. Next, obtain explicit frontend/design authorization, confirm current API/DTO/OpenAPI contracts, and proceed through the design governance gates before Angular implementation.

## Current uncommitted worktree

The files listed in Section 7 are uncommitted and not included in the remote feature comparison.
