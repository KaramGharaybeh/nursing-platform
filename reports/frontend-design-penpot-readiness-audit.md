# Frontend Design and Penpot Readiness Audit

## 1. Repository state

- Branch: `feature/preparation-package-foundation`.
- HEAD: `1c22b59 docs: reconcile frontend design re-entry baseline`.
- The frontend runtime is uninitialized; no Angular application exists.
- Preparation Package backend Stages 1–4 are recorded as complete for their approved backend scope. Frontend implementation remains pending.
- Existing unstaged/untracked design work is preserved: `docs/frontend/frontend-architecture.md`, `docs/design/screens/authentication/auth-001-sign-in-tracker.md`, and `docs/frontend/design-system-audit.md`.
- Other pre-existing untracked reports are outside this audit and were not changed.

## 2. Current frontend/design files

`docs/frontend/frontend-architecture.md` is a coherent, detailed engineering foundation for Angular 22, standalone APIs, Signals, RxJS, Angular Material/CDK, SCSS, a project-owned Material theme derived from Penpot tokens, WCAG 2.2 AA, responsive behavior, and future RTL/Arabic readiness. It intentionally defers executable token values, exact Node version, OpenAPI generator, and final visual details.

The design-governance files consistently describe a Desktop-only, specification-first design program. They do not claim that an Angular workspace, approved page specifications, approved Penpot boards, or a frozen design foundation already exists.

The uncommitted architecture wording change correctly makes Penpot the sole visual authority and resolves the 44px compliance minimum / 48px preferred mobile target policy. It still needs normal review before an independent commit decision.

`docs/frontend/design-system-audit.md` is an evidence-rich pre-development audit, not an implementation-ready design-system contract. `docs/design/screens/authentication/auth-001-sign-in-tracker.md` is explicitly legacy/draft evidence and conflicts with current program scope and live Penpot evidence.

## 3. Design governance status

**G0 was accepted on 2026-08-12 as a governance/re-entry baseline only, as recorded in `docs/frontend/design/governance/decision-log.md` (DEC-PH0-016).** The decision log, `GOAL_STATE.md`, and `MASTER_PLAN.md` confirm that this acceptance authorizes only a separately scoped Phase 1 evidence-packet task. It does not authorize Angular implementation, Penpot writes, page specifications, a route registry, or visual approval.

The governance program is internally coherent about authority and sequencing: Penpot is the sole visual authority, current design-documentation scope is Desktop only, and no Penpot writes, page specs, or Angular implementation may begin before the required gates. The architecture remains responsive and RTL-ready, which is compatible with the narrower current Desktop design-documentation scope.

Open governance/design blockers include unresolved Page 07 and Page 09 classifications, incomplete Page 10 evidence, absent local Penpot component/color assets and token theme, incomplete token sets, and no approved canonical page inventory or shared design/test contracts.

## 4. AUTH-001 status

AUTH-001 is **not the current next design slice**. It is a legacy/draft tracker that must remain untouched under the recorded decision. Its completion markers are not authoritative: current live evidence reports a blank main documentation board, empty/clipped legacy responsive/RTL artifacts, orphan objects, a stale Mobile board ID, and pending backend/accessibility review.

Its login contract notes are useful future evidence, but it is not a current approved page specification, approved Penpot design, or Angular implementation authorization.

## 5. Preparation Package design readiness

The backend is ready to supply a future evidence packet for reporting topics/profiles, study materials, practice collections, package definition/version/composition, offers, entitlement reads, practice progress, package exam-session start, and analytical-report reads.

Preparation Package design evidence should be the first new evidence slice **after** G0 acceptance and a separately approved Phase 1 task packet. It must exclude unimplemented/deferred storage and delivery, offline access, workspace/dashboard aggregation, adaptive practice, retraining, and employer package visibility. It is not ready for page specs or Penpot boards today because the required OpenAPI/DTO/validation/error/permission capture remains open.

## 6. Penpot/Docker runtime status

The Penpot Docker stack is running from `/home/karam/development/penpot/docker-compose.yaml`:

| Service | Container | Image | Status / access |
|---|---|---|---|
| Frontend | `penpot-penpot-frontend-1` | `penpotapp/frontend:2.16` | Up; `0.0.0.0:9001->8080/tcp`; likely URL `http://localhost:9001` |
| Backend | `penpot-penpot-backend-1` | `penpotapp/backend:2.16` | Up; internal Docker-network access only |
| Exporter | `penpot-penpot-exporter-1` | `penpotapp/exporter:2.16` | Up; internal Docker-network access only |
| MCP | `penpot-penpot-mcp-1` | `penpotapp/mcp:2.16` | Up; internal Docker-network access only |
| PostgreSQL | `penpot-penpot-postgres-1` | `postgres:15` | Up and healthy |
| Valkey | `penpot-penpot-valkey-1` | `valkey/valkey:8.1` | Up and healthy |
| Mailcatch | `penpot-penpot-mailcatch-1` | `sj26/mailcatcher:latest` | Up; `http://localhost:1080` |

The compose configuration declares `PENPOT_PUBLIC_URI: http://localhost:9001` and enables MCP. Containers demonstrate service availability; no login, browser check, Penpot read/write, or export was performed. Backend, exporter, and MCP have no published host port in `docker ps`, so direct host availability is not established by this audit.

## 7. Whether Penpot needs update

**No immediate Penpot update is authorized or required before project creation assessment.** Penpot is available, but existing boards are evidence only and contain documented foundation and legacy defects. A Penpot update should occur only after G0 acceptance, Phase 1 evidence capture, Phase 2 shared contracts, approved page inventory/specification, and the applicable Penpot write gate. Immediate writes would prematurely encode unresolved tokens, components, breakpoints, accessibility, and page-contract decisions.

## 8. Whether Angular project creation is safe now

**No. Angular project creation should remain blocked.** The technology direction is sufficiently defined to formulate a later scaffold command, but the repository architecture requires an approved design specification and implementation plan before code changes. The design program additionally blocks work at G0, then requires evidence, foundation, and inventory gates before page/visual work.

Creating a neutral structural scaffold would still be frontend implementation and would create a new source-of-truth boundary before the required design/API contracts are frozen. This audit does not authorize it.

## 9. Required blockers before Angular project creation

1. Karam explicitly accepts or rejects G0 after reviewing the Phase 0 reconciliation evidence and current working-tree status.
2. Resolve the commit disposition of the three existing design files through a separately approved scoped review; do not bundle draft legacy evidence automatically.
3. Approve a Phase 1 Preparation Package/API evidence packet and capture current generated Development OpenAPI, DTOs, exact permissions, validators, and Problem Details/error mappings.
4. Complete and approve the shared foundation: canonical token registry, project Material-theme mapping, breakpoints, elevation/z-index, motion, icon policy, typography/font loading, component contracts, patterns, feedback decision tree, and accessibility/RTL test contracts.
5. Resolve or explicitly defer the design-system audit's Priority 0 issues, especially incomplete Utilities, token authority, Arabic validation, and remaining source discrepancies.
6. Approve the canonical route registry, route/permission matrix, initial design slice, page specification, and frontend foundation implementation plan.
7. Pin Angular-22-compatible Node.js/npm versions and decide the OpenAPI generation approach in the approved scaffold plan.

## 10. Recommended next action

**Block Angular project creation and request explicit G0 acceptance/rejection.** If accepted, authorize one bounded Phase 1 Preparation Package evidence-capture task. Do not update Penpot, do not create Angular code, and do not treat AUTH-001 as the next approved design slice. Review the uncommitted architecture update separately; retain the legacy tracker and design-system audit as draft/audit evidence until their scope and disposition are explicitly approved.

## 11. Exact next prompt recommendation

```text
Review the Phase 0 frontend-design reconciliation and explicitly accept or reject Gate G0. Do not create Angular code or modify Penpot. If G0 is accepted, authorize a documentation-only Phase 1 Preparation Package API evidence packet at the current backend revision. The packet must capture generated Development OpenAPI, endpoint mappings, DTOs, validators, exact permissions, and Problem Details behavior for implemented package areas only; it must not create page specs, alter existing design files, or include storage/delivery, offline access, workspace, adaptive practice, retraining, or employer package features.
```
