# Frontend Design System

## Authority chain

This is the frontend implementation entry point for approved visual foundations, themes, and reusable patterns. `docs/frontend/design/stitch/DESIGN.md` is the active visual token and style source; `docs/frontend/design/stitch/system-design-contract.md` supplies system-level visual/flow context; `docs/frontend/design/stitch/stitch-artifact-registry.md` distinguishes human-approved screens from preferred or proposed artifacts. `docs/frontend/design/frontend-design-foundation-reference.md` is the repository textual implementation mapping of the approved foundation. The active Stitch v2 project and design-system identifiers are recorded in the artifact registry. Earlier Stitch and Penpot programs are historical/reference evidence unless explicitly re-approved.

A human-approved screen artifact owns its approved presentation scope only. It cannot authorize a Product feature, API behavior, route, or protected access. The preferred Nurse desktop shell artifact is not exact as-is approval; the registry separately records an approved authenticated shell visual baseline. Design-proposed Notifications and Help remain proposals, even when visible in an approved visual composition.

## Angular implementation boundary

Angular Material and CDK supply permitted component primitives. The project-owned Material theme, SCSS tokens, and component styles translate approved visual intent into executable UI. `frontend/src/styles/_tokens.scss` is the current implementation token source; `_material-theme-bridge.scss` maps it into the Angular Material theme. The active DESIGN source owns approved visual values, while SCSS proves their current implementation. Ordinary production components use external HTML and SCSS files under the [Frontend Architecture](frontend-architecture.md) component separation rule.

The active DESIGN source specifies palette roles, Noto Sans/Noto Sans Arabic typography, a 4px spacing rhythm, responsive gutters, visual states, focus treatment, and RTL-sensitive presentation. Reusable screen patterns are mapped by [Screen Contracts](screen-contracts/README.md). Storybook is a visual development/review surface, never Product or design approval authority.

## Standard field shape

The current visual authority, `docs/frontend/design/stitch/DESIGN.md`, specifies an **8px radius for standard inputs and selects**. This rule concerns visual intent. Existing SCSS values prove current implementation only and should be checked against it during relevant UI work.
