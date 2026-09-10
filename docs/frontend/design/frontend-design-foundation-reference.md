# Frontend Design Foundation Reference

## Purpose and authority

This document is the canonical textual implementation reference for the approved Penpot file `Frontend Design Foundation` (`01813f71-6684-8025-8008-5d0437a49666`).

This Markdown file records the repository-owned textual mapping for the approved Penpot-derived frontend foundation that Angular workers must consult before frontend UI, SCSS, Angular Material, component, or screen work. After the 2026-09-10 visual workflow decision, approved visual foundations and task-required approved Penpot/design artifacts own visual intent; Storybook is the intended future production visual development/review surface after separate tooling authorization, not requirements authority.

If this reference conflicts with the live approved Penpot file, implementation must STOP, record the discrepancy, obtain Penpot/design resolution, and update this reference before coding. Do not silently choose either source.

Backend source and canonical OpenAPI remain authoritative for routes, DTOs, nullability, validation, authentication, authorization, permissions, status codes, server errors, business behavior, sensitive-data exposure, and payment/exam/entitlement trust boundaries. A Penpot example never creates backend behavior.

## Verified Penpot source snapshot

- File: `Frontend Design Foundation`
- File ID: `01813f71-6684-8025-8008-5d0437a49666`
- Verified by: Penpot MCP read-only inspection on 2026-09-09
- Pages inspected: `01 — Getting Started`, `02 — Colors`, `03 — Typography`, `04 — Spacing & Shape`, `05 — Elevation & States`, `06 — Components`, `07 — Utilities`, `10 — Authentication Core`, and `11 — Preparation Package Screens`
- Local library snapshot: `0` colors, `34` typographies, `1` component
- Token sets present: `Spacing Shape` and `Elevation States`
- Color tokens are not present in the Penpot token catalog; color values are documented as page swatches/mappings.

## Global implementation rules from Penpot

- Angular 22 owns executable implementation.
- Angular Material and CDK provide implementation primitives.
- Project-owned Angular Material theme, SCSS, tokens, components, and patterns translate approved Penpot decisions.
- Do not copy arbitrary visual values.
- Resolve design/implementation disagreements before implementation, never silently.
- WCAG 2.2 AA is the accessibility baseline.
- Actual interactive targets are at least `44 × 44px`; `48 × 48px` is preferred on mobile when layout and density permit.
- Layouts must support desktop, tablet, and mobile through reflow and wrapping rather than shrink-only behavior.
- Use logical start/end and locale-aware alignment. Mirror directional icons selectively; never mirror brand or status icons.
- Arabic remains native/editable. Do not use pseudo-Arabic or shrink text to force fit.

## Color foundation

### Core brand and semantic swatches

| Role | Hex | Usage | Recommended text |
|---|---:|---|---|
| Nursing Teal | `#006B66` | Primary brand, primary action, focus accents, and active navigation states. | `#FFFFFF` (`6.37:1 AA`) |
| Professional Navy | `#173B57` | Primary text, app chrome, secondary actions, and trustworthy professional framing. | `#FFFFFF` (`11.67:1 AA`) |
| Exam Focus Indigo | `#4F46B8` | Exam-preparation emphasis, assessment context, focus border, and study states. | `#FFFFFF` (`7.24:1 AA`) |
| Success | `#147A4B` | Saved, completed, passed, verified, or positive confirmation states. | `#FFFFFF` (`5.36:1 AA`) |
| Information | `#0B63A3` | Neutral guidance, explanatory banners, system notices, and helpful status details. | `#FFFFFF` (`6.31:1 AA`) |
| Warning | `#8A4B00` | Attention-needed states, expiring sessions, incomplete setup, and recoverable risk. | `#FFFFFF` (`6.80:1 AA`) |
| Error | `#B3261E` | Validation errors, destructive risk, failed actions, and blocking issues. | `#FFFFFF` (`6.54:1 AA`) |

### Neutral swatches

| Role | Hex | Usage | Recommended text |
|---|---:|---|---|
| Neutral 0 | `#FFFFFF` | Default surface, cards, modals, popovers, and form containers. | `#173B57` (`11.67:1 AA`) |
| Neutral 50 | `#F6FBFA` | App background and high-comfort reading canvas. | `#173B57` (`11.17:1 AA`) |
| Neutral 100 | `#F3F6F8` | Subtle surfaces, table headers, grouped content, and secondary panels. | `#173B57` (`10.75:1 AA`) |
| Neutral 200 | `#E4EAEE` | Dividers, disabled backgrounds, quiet separators, and nested outlines. | `#173B57` (`9.61:1 AA`) |
| Neutral 300 | `#D1DCE2` | Default dividers, low-emphasis borders, and non-interactive outlines. | `#173B57` (`8.36:1 AA`) |
| Neutral 500 | `#758894` | Disabled text/icons with supporting non-color affordance. | `#FFFFFF` (`3.68:1 Not AA`) |
| Neutral 700 | `#4F6473` | Secondary text and readable metadata. | `#FFFFFF` (`6.17:1 AA`) |
| Neutral 900 | `#102A3A` | Highest contrast text when navy is not strong enough. | `#FFFFFF` (`14.85:1 AA`) |

Do not use `#FFFFFF` text on Neutral 500 for normal-size accessible text because the Penpot contrast note explicitly marks it `Not AA`.

### Semantic color mappings

| Mapping | Hex | Usage |
|---|---:|---|
| `background/app` | `#F6FBFA` | Main light-theme canvas and app shell background. |
| `background/subtle` | `#EEF7F5` | Subtle panels, callouts, and quiet grouped areas. |
| `surface/default` | `#FFFFFF` | Cards, forms, dialogs, menus, and elevated surfaces. |
| `surface/subtle` | `#F3F6F8` | Nested cards, table headers, and secondary content areas. |
| `surface/strong` | `#DDEDEA` | Selected rows, emphasized blocks, and strong section fills. |
| `text/primary` | `#173B57` | Primary headings, body text, and important labels. |
| `text/secondary` | `#4F6473` | Supporting copy, helper text, and secondary metadata. |
| `text/muted` | `#5F7280` | Low-emphasis metadata that must remain readable. |
| `text/inverse` | `#FFFFFF` | Text placed on verified dark brand or semantic fills. |
| `border/default` | `#C9DDDA` | Default component outlines, cards, dividers, and form borders. |
| `border/strong` | `#8FA9A5` | Stronger separators and inactive-but-visible boundaries. |
| `border/focus` | `#4F46B8` | Keyboard focus ring and high-visibility focus outline in the Colors page mapping. |
| `action/primary` | `#006B66` | Primary calls to action and most important confirmation actions. |
| `action/primary-hover` | `#005A56` | Hover, active, or pressed primary action state. |
| `action/secondary` | `#173B57` | Secondary buttons, links, and professional navigation actions. |
| `action/exam` | `#4F46B8` | Exam-preparation actions, study context, and assessment emphasis. |
| `feedback/success` | `#147A4B` | Successful completion, saved state, and positive confirmation. |
| `feedback/information` | `#0B63A3` | Informational banners, neutral guidance, and status details. |
| `feedback/warning` | `#8A4B00` | Warnings that require attention but are not destructive. |
| `feedback/error` | `#B3261E` | Errors, destructive risk, failed validation, and blocking issues. |

Do not communicate pass/fail, warning, error, or selected state with color alone. Add text, icons, shape, placement, accessible names, or other non-color cues.

## T-FE-009 Angular Material theme bridge decisions

Technical-lead decisions for `T-FE-009` are:

- Install exactly `@angular/material@22.1.5`.
- Install exactly `@angular/cdk@22.1.5`.
- Do not add `@angular/animations`.
- Use Angular Material M2 APIs only.
- Use `mat.m2-define-palette(...)` and `mat.m2-define-light-theme(...)`.
- Emit one single light theme only.
- Bridge file: `frontend/src/styles/_material-theme-bridge.scss`.
- Integrate through `frontend/src/styles.scss`.
- Custom Material typography is deferred.
- Custom Material density is deferred.
- Do not synthesize 50–900 color ramps from single colors.
- Do not treat Angular default brand palettes as visual authority.

Approved Material role mapping:

| Material M2 role | Nursing Platform value |
|---|---:|
| Primary | Nursing Teal `#006B66` |
| Accent | Professional Navy `#173B57` |
| Warn | Error `#B3261E` |

Warning `#8A4B00` remains a separate recoverable-warning semantic. Exam/Focus Indigo `#4F46B8` remains a separate exam/focus semantic. `T-FE-009` must not opportunistically convert warning to Material warn and must not change existing focus-token behavior.

If Angular Material's Sass API cannot compile without complete invented hue/contrast maps, implementation must STOP instead of fabricating palette data.

## Typography foundation

- English UI uses `Noto Sans`.
- Arabic and RTL content use `Noto Sans Arabic`.
- Avoid letter spacing in Arabic.
- Use semantic roles for Angular implementation rather than raw scale names.
- Preserve line height for exam reading, recruitment forms, tables, dashboards, and long-form content.
- Use start/end alignment in implementation and preserve punctuation, mixed-direction numbers, and Arabic field ordering.
- Responsive typography breakpoints in Penpot are mobile below `600`, tablet `600–959`, desktop `960–1279`, large `1280–1919`, and wide `1920+`.

The Penpot local library contains `34` typography assets across English and Arabic semantic examples. Until a task explicitly maps those assets into executable Angular typography tokens, workers must not invent custom Material typography configuration.

## Spacing and shape token foundation

Penpot token set `Spacing Shape` is active and contains these verified values:

| Token | Value |
|---|---:|
| `space.0` | `0px` |
| `space.0_5` | `2px` |
| `space.1` | `4px` |
| `space.2` | `8px` |
| `space.3` | `12px` |
| `space.4` | `16px` |
| `space.5` | `20px` |
| `space.6` | `24px` |
| `space.8` | `32px` |
| `space.10` | `40px` |
| `space.12` | `48px` |
| `space.16` | `64px` |
| `space.20` | `80px` |
| `space.24` | `96px` |
| `space.32` | `128px` |
| `inset.xs` | `8px` |
| `inset.sm` | `12px` |
| `inset.md` | `16px` |
| `inset.lg` | `24px` |
| `inset.xl` | `32px` |
| `stack.xs` | `4px` |
| `stack.sm` | `8px` |
| `stack.md` | `16px` |
| `stack.lg` | `24px` |
| `stack.xl` | `32px` |
| `inline.xs` | `4px` |
| `inline.sm` | `8px` |
| `inline.md` | `12px` |
| `inline.lg` | `16px` |
| `section.sm` | `32px` |
| `section.md` | `48px` |
| `section.lg` | `64px` |
| `page.mobile` | `16px` |
| `page.tablet` | `24px` |
| `page.desktop` | `32px` |

| Radius token | Value | Typical usage evidenced in Penpot |
|---|---:|---|
| `radius.none` | `0px` | Strict square alignment only. |
| `radius.xs` | `4px` | Tiny badges. |
| `radius.sm` | `8px` | Inputs, selects, compact controls. |
| `radius.md` | `12px` | Buttons and standard form-field foundation. |
| `radius.lg` | `16px` | Cards. |
| `radius.xl` | `24px` | Dialogs. |
| `radius.full` | `9999px` | Badges, chips, intentional pill-like controls. |

Component examples in Penpot include: button padding `inset/sm + inline/md`, input padding `inset/md`, card padding `inset/lg`, modal padding `inset/xl`, table cell padding `inset/sm / inset-md`, navigation spacing `inline/md + inset/sm`, alert padding `inset/lg`, form group spacing `stack/lg`, exam question spacing `section/sm + stack/md`, and profile section spacing `section/md`.

## Responsive foundation

| Breakpoint range | Page gutter | Section spacing | Card padding | Density note |
|---|---:|---|---|---|
| Mobile `<600` | `page/mobile 16px` | `section/sm 32px` | `inset/md` | Comfort density; `44px+` targets. |
| Tablet `600–959` | `page/tablet 24px` | `section/sm` to `section/md` | `inset/lg` | Stable controls; wider gutters. |
| Desktop `960–1279` | `page/desktop 32px` | `section/md 48px` | `inset/lg` | Default density and reading width. |
| Large `1280–1919` | `page/desktop 32px` | `section/md` to `section/lg` | `inset/lg` | Use columns before whitespace. |
| Wide `1920+` | `page/desktop 32px` plus content-width constraints | Design-specific | Design-specific | Do not expand readable content indefinitely. |

## Elevation, focus, and state foundation

Penpot token set `Elevation States` is active and contains these verified values:

| Token | Value |
|---|---|
| `elevation.none` | no shadow |
| `elevation.level1` | `0 2px 6px 0 #173B5724` |
| `elevation.level2` | `0 6px 14px 0 #173B57` |
| `elevation.level3` | `0 12px 28px 0 #173B57` |
| `focus.ring.width` | `2px` |
| `focus.ring.offset` | `4px` |
| `focus.ring.shadow` | `0 0 0 4px #006B66` |
| `state.disabled.opacity` | `0.42` |
| `state.scrim.opacity` | `0.48` |

State rules evidenced in Penpot:

- Use the lowest elevation. Prefer borders or spacing for stable grouping; reserve shadows for floating relationships.
- Do not use elevation for permissions, validity, focus, selection, validation, or disabled state.
- Hover, pressed, focus-visible, disabled, loading, selected, read-only, error, and success states must remain visually and semantically distinct.
- Loading preserves dimensions and avoids layout jump. Do not show fake percentages.
- Read-only is distinct from disabled and remains readable.
- Error and success use icons, messages, text, border, or other non-color cues.

### Known focus authority conflict

There is a known focus evidence conflict that is intentionally not resolved by `T-FE-009`:

- Colors authority says `border/focus = #4F46B8`.
- Components examples use indigo/blue focus treatment.
- Elevation & States token authority says `focus.ring.shadow = 0 0 0 4px #006B66` and `focus.ring.offset = 4px`.
- Existing verified frontend focus-token behavior uses `--np-focus-ring-color: var(--np-color-brand-1)`.

Until an explicit reconciliation task changes it, the existing verified frontend focus-token behavior remains unchanged. `T-FE-009` must not opportunistically change focus behavior.

## Component and utility evidence

Penpot component guidance establishes project-owned Angular Material patterns, not automatic screen approval. Components must preserve labels, validation, loading geometry, responsive behavior, accessible interaction states, and future RTL readiness.

Evidenced component requirements include:

- Buttons: `48px` control examples, `16/24px` inline padding, stable loading width, focus-visible distinct from hover/pressed, icon-only buttons require documented accessible names, actual targets at least `44 × 44px` and preferred `48 × 48px` on mobile.
- Button anatomy: optional `20px` icon, `16px` icon-to-label gap, `24px` logical inline padding, `12px` component radius.
- Inputs: standard foundation remains `64px` field height, `12px` radius, `16px` inline padding, stable support row, distinct read-only/disabled/validation/loading states.
- Textareas: support message uses logical inline-start; counter uses logical inline-end; stack safely on narrow layouts without overlap.
- Selection controls: selected, checked, indeterminate, current, and active are distinct and must not rely on color alone.

Utilities page evidence lists expected future patterns such as authentication screen states, product-map entries, and workflow descriptions. These entries do not approve product screen implementation and do not override backend/OpenAPI authority.

## STOP conditions for frontend workers

STOP and escalate instead of guessing when:

- Live Penpot contradicts a copied value in this document.
- A Material theme implementation requires invented hue/contrast data or synthesized palettes.
- A task would change warning, exam, focus, or other approved visual semantics outside its scope.
- A new design decision is required.
- A Penpot product screen, utility description, or example conflicts with backend/OpenAPI behavior.
- A screen lacks explicit approval, state contract, responsive contract, RTL contract, accessibility contract, or backend contract.
- A worker needs to add dependencies, modify Penpot, change backend/OpenAPI behavior, stage, commit, push, or alter repository history without explicit approval.
