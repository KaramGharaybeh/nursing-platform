# Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-README
status: active-contract-index
owner: frontend-design-governance
updated: 2026-09-21
```

## Purpose

This directory defines the canonical screen-contract map for future Stitch, Storybook, and Angular implementation work. It bridges route authority, product/page decisions, backend/API constraints, and visual-design authority without replacing any of those sources.

Use these files to answer four questions before generating, reviewing, or implementing a screen:

1. Which canonical route or non-routable state owns this screen?
2. Which route, role, permission, backend/API, and security constraints apply?
3. Which approved screen packet or governance source authorizes the screen?
4. Which details remain blocked, deferred, visual-only, or design-proposed rather than implemented product behavior?

## Authority

Screen contracts are mapping documents. They do not create routes, backend behavior, permissions, product features, visual approval, or implementation authorization.

Higher authority remains:

1. Current explicit decisions from Karam.
2. Backend source and generated Development OpenAPI for runtime/API/security behavior.
3. Canonical route and permission source under `frontend/src/app/core/routing/`.
4. Approved screen approval packets and page registry under `docs/frontend/design/inventory/`.
5. `docs/frontend/design/stitch/DESIGN.md` and human-approved Stitch artifacts for visual composition.
6. Angular source after implementation.

If these files conflict with higher authority, stop and resolve the conflict instead of guessing.

## Files

| File | Ownership |
|---|---|
| `contract-schema.md` | Required fields, statuses, allowed evidence, and forbidden inferences for every contract row. |
| `screen-index.md` | Canonical route-to-screen map across all 64 current route IDs plus individually traceable non-routable states, shared shell states, and design-proposed feature rows. |
| `shared-patterns.md` | Shared design, state, accessibility, security, and responsive patterns used by family files. |
| `authentication.md` | Auth, registration, verification, password, and onboarding-adjacent auth states. |
| `account.md` | Account overview, same-route account edit state, and blocked account settings states. |
| `nurse-profile.md` | Nurse profile sections, CV, and contact requests. |
| `exams.md` | Exam catalog, detail, session, result, review, analytics, history, and transient exam states. |
| `preparation-packages.md` | Package offers, entitlements, practice, package report, and package-owned actions. |
| `commerce.md` | Payment products, checkout, payment outcomes, order history, and deferred processing state. |
| `employer.md` | Employer routes and known contract gaps. |
| `administration.md` | Admin routes, current ready subset, permission-protected future surfaces, and blocked admin concepts. |
| `shared-system.md` | Root, session/access terminal routes, loading/error/empty/offline/maintenance shared states. |

## Status Values

Use the status vocabulary in `contract-schema.md`. Do not replace `PARTIAL`, `AUTHORITY_GAP`, `BACKEND_BLOCKED`, `DEFERRED`, or `DESIGN_PROPOSED_FEATURE` with implementation-ready language.

## Current Route Baseline

The current route source contains 64 route IDs:

| Classification | Count |
|---|---:|
| Entry route | 1 |
| Public routes | 13 |
| Authenticated routes | 50 |
| Total | 64 |

Older prose that says `62`, `12`, or `49` is stale for this contract directory. Use the route source and `screen-index.md` instead.

The current expanded screen/state map contains 101 represented rows: 70 routable screen rows, including justified shared-route state rows, and 31 non-routable/transient/deferred/design-proposed rows.
