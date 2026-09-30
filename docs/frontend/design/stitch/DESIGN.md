# Nursing Platform — Core DESIGN.md

```yaml
document_id: NPS-DES-STITCH-DESIGN-MD
status: PHASE_2_CANONICAL_STITCH_DESIGN_SOURCE
source_contract: docs/frontend/design/stitch/system-design-contract.md
created_at: 2026-09-20
stitch_design_system_name: Nursing Platform — Core Design System
```

## Product Character

Nursing Platform is a professional, clinical, calm, trustworthy, modern, readable, task-oriented healthcare application. The interface prioritizes clarity, direct task completion, factual status, accessibility, and confidence over decoration.

Avoid excessive decoration, giant marketing typography in workflows, unnecessary gradients, toy-like roundness, dense visual noise, low-contrast gray-on-gray UI, excessive shadows, and dashboard decoration without information value.

## Core Palette

Use one calm light palette. Do not introduce a competing palette.

| Role | Value | Use |
|---|---:|---|
| Application background | `#F6FBFA` | App canvas and page background. |
| Surface | `#FFFFFF` | Cards, app bar, menus, dialogs, form panels. |
| Subtle surface | `#F3F6F8` | Table headers, nested panels, quiet sections. |
| Subtle teal surface | `#EEF7F5` | Active navigation background and quiet brand-tinted panels. |
| Primary text / navy | `#173B57` | Headings, app chrome, primary text. |
| High contrast text | `#102A3A` | Dense text where maximum contrast is needed. |
| Secondary text | `#4F6473` | Metadata and helper copy. |
| Muted text | `#5F7280` | Low-emphasis text only on approved light backgrounds. |
| Border default | `#C9DDDA` | Cards, controls, separators. |
| Border strong | `#8FA9A5` | Strong separators and active indicators. |
| Primary interactive teal | `#006B66` | Primary app actions, brand mark, primary nav affordances. |
| Primary hover | `#005A56` | Hover/pressed primary states. |
| Exam emphasis indigo | `#4F46B8` | Exam-specific high-emphasis actions and focus accents where approved. |
| Success | `#147A4B` | Completed/saved/success states. |
| Info | `#0B63A3` | Informational notices. |
| Warning | `#8A4B00` | Recoverable attention states. |
| Error | `#B3261E` | Errors and destructive risk. |
| Overlay / scrim | `rgba(16, 42, 58, 0.48)` | Modal drawer/dialog scrim. |

Never communicate status, active state, selected state, or correctness by color alone. Pair color with text, weight, indicator shape, border, icon, or placement.

## Typography

- English UI uses `Noto Sans`.
- Arabic and RTL content uses `Noto Sans Arabic`.
- Avoid letter spacing in Arabic.
- Use semantic type roles rather than arbitrary sizes.

Recommended hierarchy:

| Role | Intent |
|---|---|
| Display | Rare product/system overview headings only; not for routine app workflows. |
| Page heading | One clear `h1` per screen. |
| Section heading | Distinct section grouping in main content. |
| Body | Comfortable reading text. |
| Label | Form labels, table headers, nav labels. |
| Helper | Field help, secondary instructions. |
| Status | Compact factual status text with non-color affordance. |
| Numeric / metric | Stable direction, readable in RTL/mixed English/Arabic contexts. |

## Spacing, Containers, Density

Use a 4px-based spacing system. Prefer consistent rhythm over manual offsets.

- Mobile gutter: `16px`.
- Tablet gutter: `24px`.
- Desktop gutter: `32px`.
- Standard card padding: `24px` desktop/tablet, `16px` mobile when needed.
- Page sections: `32px` to `64px` depending on density and hierarchy.
- Navigation item hit areas: minimum `44 × 44px`; `48 × 48px` preferred on mobile/touch.
- Full-width workflow exceptions are allowed for exam session and admin dense data when content demands it.
- Wide-data exceptions may use larger containers or controlled horizontal overflow only when cross-column comparison requires it.

## Shape

- Inputs/selects: `8px` radius.
- Buttons: `12px` radius.
- Cards/surfaces: `16px` radius.
- Dialogs/drawers: `24px` radius where appropriate.
- Pills/badges: full radius.
- Avoid over-rounded toy-like cards.

## Elevation And Overlays

Use borders and spacing before shadows. Shadows are for floating relationships only.

- Cards: border or very subtle level-1 elevation.
- Menus/drawers: clear overlay elevation and scrim for modal mobile navigation.
- Dialogs: strongest elevation, scrim, focus management.
- Do not use elevation to indicate validity, permission, focus, selection, or status.

## Focus And Accessibility

- Visible keyboard focus is mandatory.
- Use a high-contrast 2px focus ring with offset; do not hide focus.
- Focus treatment must be distinct from hover and selected state.
- Skip-to-content link appears on focus and targets the main content region.
- Menu buttons expose clear focus, expanded/collapsed state, and accessible names.
- Drawer focus enters the drawer on open and returns to trigger on close.
- Escape closes modal navigation.
- Background page must not remain interactable while modal navigation is open.
- WCAG 2.2 AA is the target.

## Motion And Reduced Motion

Motion is supportive, not decorative. Use short, calm transitions only for menus/drawers and state changes when needed. Reduced-motion mode must remove or minimize movement without losing state meaning. Do not invent fake progress, spinning decorative effects, or arbitrary easing as product identity.

## Iconography

Icons support labels; they are not primary identity.

- Primary navigation keeps visible text labels.
- Icon-only primary navigation is not approved.
- Use one consistent project-owned line icon style.
- Standard icon size: `20px` inside navigation/items; `24px` for larger utility controls when needed.
- Icon-only utility controls need accessible names and compliant hit areas.
- Directional icons mirror in RTL.
- Status icons do not mirror.
- Decorative icons are not interactive and must not carry unique meaning.

## Navigation System

The approved shell is hybrid: persistent top application bar on desktop, compact top bar with collapsible accessible menu/drawer on mobile. Do not use a permanent global sidebar for every user.

Authenticated shell includes:

- Brand/home affordance.
- Primary actor-aware navigation by user goals/product families.
- User/account affordance.
- Sign-out access.
- Main content region.
- Route-level loading treatment.
- Optional contextual/secondary navigation only where a product family truly needs it.

Nurse primary navigation demonstrates:

- Exams.
- Preparation Packages.
- Products when available.
- Profile.

Account actions live in the user/account affordance, not as competing primary product navigation. Contextual screens such as Exam Session, Exam Result, Answer Review, package report, detail screens, confirmations, and transient states are not global primary menu items.

Active route state must use text emphasis plus a visible indicator/background/border and future `aria-current` semantics. It must not rely on color alone.

Navigation visibility is not authorization. It represents eligibility only; backend and router guards remain security authority.

## Anonymous Shell

Anonymous/public shell must not show authenticated product navigation. It supports only authoritative public/entry behavior: brand/home, authentication entry, and public Preparation Package offers where appropriate. Do not invent a marketing website.

## Mobile Navigation

At approximately 390px:

- Top bar remains compact.
- Navigation trigger is visible and accessible.
- Drawer/menu uses readable labels and compliant targets.
- No clipped labels, no horizontal overflow, no nested scrolling.
- Drawer closes after successful route change.
- Close control is visible.
- Current destination is clear.
- Account and sign out are reachable.
- Background interaction is blocked while drawer is open.

Do not use bottom navigation unless separately justified and approved. Current approval is top-app-bar plus collapsible mobile navigation.

## RTL And Mixed-Direction Content

- Use logical start/end layout, spacing, and alignment.
- Preserve semantic navigation order.
- Drawer opens from the logical/navigation-appropriate side.
- Directional icons mirror; status icons do not.
- Numbers, percentages, dates, English abbreviations, and currency remain readable and stable.
- Arabic expansion must not clip labels.
- Monetary/numeric content should be wrapped and aligned for mixed-direction readability.

## Controls

Controls must include default, hover, focus, active, disabled, loading, selected, error, and success states where applicable.

- Primary button: teal fill, white text, clear focus, stable loading width.
- Secondary button: navy or outline treatment, calm emphasis.
- Destructive button: error role, confirmation when consequential.
- Text link: visible underline/focus affordance; not color-only.
- Icon utility button: accessible name, visible focus, 44px target.
- Inputs/selects: label, helper/error row, readable border, disabled/read-only distinction.
- Password input: visibility control allowed for password fields only.
- Checkbox/radio: visible label and target area.
- Textarea: logical helper/counter behavior, safe wrapping.

## Content Components

- Cards: grouped content with clear heading and facts; avoid decorative clutter.
- Metric blocks: factual only, no unsupported scoring/bands.
- Definition lists: preferred for identity/status facts.
- Lists: responsive and accessible; maintain full content.
- Tables: preferred for genuine dense admin data on desktop. Mobile transforms to cards/lists when columns cannot reflow meaningfully. Horizontal scrolling is allowed only when comparison genuinely requires table structure.
- Pagination: Previous / Page X of Y / Next pattern with accessible labels.
- Dialogs/confirmations: clear heading, consequence summary, safe default action, focus entry/return.
- Banners/notices: info/warning/error/success roles with text and icon/shape, not color alone.

## System States

- Loading: factual text, layout stability, announced, no fake percentages.
- Empty: calm, describes zero records, optional same-screen create CTA only when authorized.
- Filtered empty/no-results: preserves query/filter context and offers reset.
- Restricted/forbidden: generic privacy-safe language, no role/permission keys.
- Unavailable/not found: privacy-safe and contextual, no raw IDs.
- Offline/maintenance: factual connectivity/service-state language only. Do not claim exam answers are safely stored offline, payments are safe to continue offline, transactions resume, arbitrary writes are queued, or data synchronizes unless runtime/backend contracts prove it.

## Application Shell Screens For Phase 2

Generate only these Phase-2 design surfaces:

- Shell / APP-SHELL / Nurse / Desktop.
- Shell / APP-SHELL / Nurse / Mobile.
- Shell / APP-SHELL / Nurse / Desktop RTL.
- Shell / APP-SHELL / Nurse / Mobile RTL.
- Shell / APP-SHELL / Anonymous / Desktop.
- Shell / APP-SHELL / Anonymous / Mobile.
- Shell / APP-SHELL / Admin / Desktop if generated as a representative variant.
- Design System / CORE / Component Overview / Desktop if Stitch represents the design system as a screen/artifact.

Do not generate feature screens such as Exam Catalog, Exam Detail, Exam Session, Commerce Product pages, Checkout, Nurse Profile pages, reports, forms, or Admin feature screens in Phase 2.
