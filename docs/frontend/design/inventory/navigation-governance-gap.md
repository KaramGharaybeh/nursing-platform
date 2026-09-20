# Navigation Governance Gap — Application Navigation / Navigation Toolbar

> Scope: governance/task-authority record only. No design decisions, no implementation.
> Recorded: 2026-09-20 from accepted HEAD `a60e3c9`. Human priority: Navigation Toolbar work is the next
> human-selected campaign after the `T-FE-084` deferral. Do NOT start implementation from this document.

## 1. Product/usability requirement

The application currently lacks a practical user-facing navigation mechanism: implemented screens are reachable
in production only by manually entering canonical URLs. The current visual experience needs a dedicated
Navigation Toolbar / app navigation design pass. Individual feature screens MUST NOT invent navigation
independently (sidebar/header/navbar/sidebar per screen was never authorized for any feature task).

## 2. Existing navigation authority (verified against current repository state, not assumed)

- `T-FE-032` / `ST-FE-032` / `GATE-FE-T032` are `VERIFIED` for reusable non-visual navigation-policy logic only
  (`frontend/src/app/core/routing/navigation-permission-policy.ts`: per-route eligibility + caller-supplied
  candidate filtering over the `T-FE-031` route permission policy). Its record explicitly excludes: actual
  navigation UI, navigation inventory, menu/sidebar/header/drawer/mobile components, templates, SCSS/CSS,
  labels/copy, icons, groups, order, active visual state, responsive/RTL presentation, breadcrumbs, layouts,
  Storybook, Penpot governance, and route activation.
- Route authority deliberately excludes navigation metadata: `page-registry.md` §2.13 forbids sidebar labels,
  icons, menu ordering, and breadcrumbs in the canonical route registry unless a later task requires it.
- Production shell reality: `frontend/src/app/app.html` renders only the `route-loading` treatment plus
  `<router-outlet />` — there is NO visual navigation consumer in the shell.
- Result: permission-aware visibility logic exists with no inventory to filter and no surface to render into.

## 3. NAVIGATION_GOVERNANCE_GAP (no existing task owns visual navigation)

No ledger task, subtask, gate, design packet, or page/shell authority owns visual application navigation:

- `T-FE-032` stopped short at logic-only by explicit technical-lead `LOGIC-ONLY` scope decision; its subtask
  record states concrete visual navigation presentation "requires separate product/design/visual-workflow
  authority before any UI work begins."
- `M-FE-007` (Shell/routing/permission architecture) contains no visual-navigation task; all member gates are
  `VERIFIED` for logic/registry/guard/policy work only.
- No screen-approval packet (Auth, Nurse, Exams, Commerce, Employer, Account, System) authorizes global
  navigation UI; Commerce packet `HC-C*` decisions cover `COM-001..008` screens only.

## 4. Why the current logic is insufficient for user-facing navigation

`getNavigationEligibility` / `filterEligibleNavigationCandidates` answer "may this caller-supplied route ID be
shown" — they supply no product inventory (which routes appear), no labels/icons/groups/order, no shell
placement or responsive/mobile pattern, no active-route treatment, and no breadcrumbs. There is additionally no
approved visual authority to derive these from: no Penpot global-navigation design, no toolbar spec, and (per
the campaign boundary) no sidebar-vs-toolbar, breakpoint, hamburger, menu-item, role-grouping, icon, or
styling decisions have been made.

## 5. Where a new task would insert (no task number assigned)

A new task number MUST NOT be invented here: repository governance defines no deterministic self-allocation
mechanism. Placement for human authorization:

- Milestone: `M-FE-007` (Shell/routing/permission architecture) is the natural home; predecessor
  `GATE-FE-T032` (`VERIFIED`) plus the verified shell/registry/guard/policy foundations would anchor it.
- Likely shape (for the human to approve, not decided here): a design-first subtask or packet producing the
  navigation inventory + toolbar visual authority (possibly Penpot), followed by an implementation task with
  per-screen visual evidence under its own gate — mirroring the packet-then-build precedent of `T-FE-077` →
  `T-FE-082`.
- Blocker axes to carry: `DESIGN` (inventory + visual authority missing) at minimum.

## 6. Human decisions needed to authorize the next campaign

1. Allocate the ledger task number(s) for navigation design and implementation work.
2. Confirm scope: Navigation Toolbar / app navigation design pass + implementation, sequenced BEFORE the
   deferred `T-FE-084` checkout work.
3. Confirm the design surface for navigation intent (dedicated Penpot/design pass vs. derivation from approved
   foundations + existing component patterns), resolving the §11 boundary items (sidebar vs toolbar, desktop vs
   mobile pattern, menu items, role grouping, icons, breakpoints, styling, active-route treatment).
4. Confirm whether the page-registry §2.13 exclusion must be revisited (i.e., whether navigation metadata
   belongs in the canonical registry or lives with the owning navigation task).

## 7. Design/source-of-truth notes relevant to the Navigation campaign (reported, not fixed here)

- `GOAL_STATE.md` / `MASTER_PLAN.md` / `screen-continuation-readiness.md` are frozen Phase-0 historical
  program documents (self-declared superseded/historical); they are not current execution authority and were
  intentionally left untouched. The ledger remains the execution authority.
- Screen Ownership Matrix rows for already-implemented exam screens still read `BLOCKED` / `NOT STARTED`
  (pre-existing systemic staleness across post-packet families, not caused by this campaign); only the
  Commerce rows (`COM-001/002/003`) were reconciled here. A future reconciliation should address the exam
  rows rather than leaving the matrix split.
- Implemented routes (Auth, Account, Nurse, Exams, Preparation Packages, Commerce products) have no
  navigation-model entries because no navigation model exists — that absence is this gap itself.
