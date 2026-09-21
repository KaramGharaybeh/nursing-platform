# Stitch Artifact Registry

```yaml
document_id: NPS-DES-STITCH-ARTIFACT-REGISTRY
status: PHASE_2_AUTHENTICATED_SHELL_APPROVED
created_at: 2026-09-20
updated_at: 2026-09-21
active_project: projects/17116545761229201855
active_design_system: assets/6536256059106605307
canonical_source: docs/frontend/design/stitch/DESIGN.md
```

## Superseded Artifacts

| Artifact | Name | Status | Notes |
|---|---|---|---|
| `projects/14739979548635957177` | Nursing Platform — System Redesign | SUPERSEDED_PENDING_VERIFICATION | Superseded by the clean v2 reset. Do not delete until separately approved by the human. |
| `assets/8866686723686557578` | Nursing Platform — Core Design System | SUPERSEDED_PENDING_VERIFICATION | Superseded by the clean v2 reset. Do not use as input for v2 generation. Do not delete until separately approved by the human. |

## Active Project

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` |
| Project title | `Nursing Platform — System Redesign v2` |
| Visibility | Private |
| Origin | Stitch |
| Status | ACTIVE_WITH_DESIGN_LED_FEATURE_DISCOVERY |
| Current screen count | `stitch_list_screens` has shown known inconsistencies. Shell artifacts and Authentication Batch 1 artifacts are directly retrievable/validated by IDs recorded below; list output must not be the only source of artifact verification. |

## Active Design System

| Field | Value |
|---|---|
| Stitch asset | `assets/6536256059106605307` |
| Display name | `Nursing Platform — Core Design System v2` |
| Repo source | `docs/frontend/design/stitch/DESIGN.md` |
| Source byte size | `11310` |
| Source character size | `11316` |
| Workflow used | `stitch_create_project` then `stitch_create_design_system` with complete canonical `theme.designMd` |
| Native upload/create-from-DESIGN.md status | Not used because `stitch_create_design_system_from_design_md` requires an existing selected screen instance and the reset project intentionally has no screens. |
| Full DESIGN.md transfer | YES |
| Shortening or summarization | NO |
| Parity verdict | FULL_CONTENT_PARITY |
| Missing design rules | None found. |

## Required Rule Parity Verification

| Rule | Stitch designMd status |
|---|---|
| No Arabic letter spacing | Present |
| Page section spacing 32px-64px | Present |
| Full-width exam-session exception | Present |
| Full-width admin dense-data exception | Present |
| Focus distinct from hover and selected | Present |
| Password visibility only for password fields | Present |
| Disabled vs read-only distinction | Present |
| Avoid unnecessary gradients | Present |
| Avoid toy-like excessive roundness | Present |
| Avoid dense visual noise | Present |
| Avoid low-contrast gray-on-gray UI | Present |
| Avoid excessive shadows | Present |
| Controlled wide-data overflow | Present |
| Critical content/actions never clipped | Present |
| Focus never clipped in overflow regions | Present |

## Phase 2 Screen Artifacts

The first generated product shell is the human-preferred visual direction for the authenticated Nurse shell. This visual preference does not approve every invented element as implemented product functionality. Content is classified below as authoritative shell/navigation, design-proposed feature, visual-only element, rejected fake/debug content, or unsupported factual claim.

| Approved artifact | Device | Stitch screen | Status | Notes |
|---|---|---|---|---|
| Shell / APP-SHELL / Nurse / Desktop | Desktop | `projects/17116545761229201855/screens/fbcef626cae7450fa6f5cedbdbeae8ea` | PREFERRED_VISUAL_BASELINE / NOT HUMAN APPROVED | Human prefers this visual direction for the authenticated Nurse desktop shell. Authoritative shell/navigation retained: brand, top app bar, Exams, Preparation Packages, Products, Profile, account affordance, main content region. Design-proposed features retained for backlog review only: Notifications (`DPF-001`) and Help / Support Access (`DPF-002`). Not accepted as product features: fake user identity, fake clinical title, debug/design annotations, route/debug labels, auth/token/status claims, online/system-health claims, compliance/security claims, and unsupported footer claims. This is not exact as-is human approval. |
| Shell / APP-SHELL / Nurse / Desktop v2 | Desktop | `projects/17116545761229201855/screens/96852332a6f048039bf76fc459d8f07e` | CONTRACT_VALID / READY_FOR_HUMAN_VISUAL_REVIEW / NOT HUMAN APPROVED / SECONDARY_REFERENCE | Generated exactly once from the prior strict manifest using `assets/6536256059106605307`. It remains a valid reduced reference, but it is not the preferred visual baseline. Human approval has not been granted. |
| Shell / APP-SHELL / Nurse / Desktop v3 — Human Review Candidate | Desktop | `projects/17116545761229201855/screens/764a9361e16948b0be831e4bf28f584b` | HUMAN_APPROVED_AUTHENTICATED_VISUAL_BASELINE | Human approved the authenticated App Shell visual chrome on 2026-09-21. Approved aspects: top application bar, Nursing Platform branding, primary navigation composition, active-navigation treatment, account affordance/dropdown, spacing/density, visual language, Notifications icon as `DPF-001`, and Help icon as `DPF-002`. Representative Exams content in the content region is non-authoritative and must not be treated as approved Exam behavior. DPF-001/002 remain design-proposed features and receive no implementation authority from this approval. |
| Shell / APP-SHELL / Nurse / Mobile | Mobile | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |
| Shell / APP-SHELL / Nurse / Desktop RTL | Desktop | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |
| Shell / APP-SHELL / Nurse / Mobile RTL | Mobile | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |
| Shell / APP-SHELL / Anonymous / Desktop | Desktop | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |
| Shell / APP-SHELL / Anonymous / Mobile | Mobile | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |
| Shell / APP-SHELL / Admin / Desktop | Desktop | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |
| Design System / CORE / Component Overview / Desktop | Desktop | Not created | PENDING_HUMAN_APPROVAL | Not attempted during foundation reset. |

## Authentication Batch 1 Human-Approved Visual References

Human approval checkpoint: 2026-09-21. The human visually reviewed Authentication Batch 1 and approved its visual direction, visual system, form/layout treatment, warning/error/state presentation, and family fit for Nursing Platform. These artifacts are approved visual references only. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize additional Authentication screens or later authenticated screen-family batches.

Active workspace used for these references:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Visual approval status | Notes |
|---|---|---|---|---|
| `AUTH-001` | Sign In | `projects/17116545761229201855/screens/6427cabcd8b549eb845c85d6b8a0413a` | HUMAN_APPROVED_VISUAL_REFERENCE | Accepted Authentication Batch 1 visual reference. Implementation remains separate. |
| `AUTH-002` | Sign Up | `projects/17116545761229201855/screens/300823b7306f4924a947eb3a80848e63` | HUMAN_APPROVED_VISUAL_REFERENCE | Accepted Authentication Batch 1 visual reference. Implementation remains separate. |
| `AUTH-005` | Check Email | `projects/17116545761229201855/screens/265d92edd39248269e867d6d72d44962` | HUMAN_APPROVED_VISUAL_REFERENCE | Accepted Authentication Batch 1 visual reference. Implementation remains separate. |
| `AUTH-006` | Verify Email | `projects/17116545761229201855/screens/76bae8f5c9a94284a919bf81b9c8e15c` | HUMAN_APPROVED_VISUAL_REFERENCE | Accepted Authentication Batch 1 visual reference. Implementation remains separate. |
| `AUTH-007` | Forgot Password | `projects/17116545761229201855/screens/55522993d9514ed9ac67586fb552cb1a` | HUMAN_APPROVED_VISUAL_REFERENCE | Accepted Authentication Batch 1 visual reference. Implementation remains separate. |
| `AUTH-008` | Reset Password | `projects/17116545761229201855/screens/5782187204dc4229a5970ddf2cc0012f` | HUMAN_APPROVED_VISUAL_REFERENCE | Accepted Authentication Batch 1 visual reference. Implementation remains separate. |

Rejected/superseded Authentication generation attempts from session/tool evidence:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `AUTH-001` | `projects/17116545761229201855/screens/35d14c2c1e614c4f9bb236227f86cb7b` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First generation attempt showed out-of-contract footer/support/legal/portal-claim content. Superseded by accepted `6427cabcd8b549eb845c85d6b8a0413a`. |
| `AUTH-006` | `projects/17116545761229201855/screens/f3ff01bf143449fa879bf13736c9d547` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First generation attempt showed a help/question-style icon in the missing-token state. Superseded by accepted `76bae8f5c9a94284a919bf81b9c8e15c`. |

## Tool Evidence

| Step | Result |
|---|---|
| `stitch_create_project` | Created `projects/17116545761229201855` named `Nursing Platform — System Redesign v2`. |
| `stitch_create_design_system` | Created `assets/6536256059106605307` named `Nursing Platform — Core Design System v2` with complete canonical `theme.designMd`. |
| `stitch_get_project` | Confirmed `projects/17116545761229201855` exists and is owned by the current user. |
| `stitch_list_design_systems` | Confirmed `assets/6536256059106605307` exists under the new project and returns the complete canonical `designMd`. |
| `stitch_list_screens` | Returned `{}` for the new project. |
| `stitch_generate_screen_from_text` | Generated exactly one screen: `projects/17116545761229201855/screens/fbcef626cae7450fa6f5cedbdbeae8ea`, titled `Shell / APP-SHELL / Nurse / Desktop`, using `assets/6536256059106605307`. |
| `stitch_get_screen` | Directly retrieved the generated screen by ID. |
| Generated HTML read-only inspection | Confirmed required top bar, brand, Exams, Preparation Packages, Products, Profile, user/account affordance, and main content region. Also found unauthorized invented Help, Notifications, named user/email, auth-token/status claims, clinical-system-online/compliance footer claims. |
| `stitch_edit_screens` on invalid shell | Reported successful DOM operations, but visible/browser artifact and downloaded HTML still retained prohibited original content. |
| `stitch_generate_screen_from_text` replacement | Generated exactly one replacement: `projects/17116545761229201855/screens/96852332a6f048039bf76fc459d8f07e`, titled `Shell / APP-SHELL / Nurse / Desktop v2`, using `assets/6536256059106605307`. |
| Replacement `stitch_get_screen` | Directly retrieved `projects/17116545761229201855/screens/96852332a6f048039bf76fc459d8f07e`, title `Shell / APP-SHELL / Nurse / Desktop v2`, width `2560`, height `2048`, device `DESKTOP`. |
| Replacement `stitch_list_screens` | Returned only the original invalid screen `fbcef626cae7450fa6f5cedbdbeae8ea`; replacement `96852332a6f048039bf76fc459d8f07e` was omitted despite direct retrieval. |
| Replacement visible text validation | Required labels present: `Nursing Platform`, `Exams`, `Preparation Packages`, `Products`, `Profile`, `Account`, `Sign out`, main content region placeholder. Prior forbidden visible items absent: Help, Notifications, fake user names/emails/role profile data, compliance/security/status/footer claims, metrics, fake exam data, invented product widgets. |
| Replacement validation-semantics correction | Closed-world validation now applies strictly to user-visible and accessibility-exposed product content/behavior, not unexposed raw implementation source. |
| Replacement implementation-internal findings | Raw HTML contains implementation internals such as HTML comments, Tailwind configuration, JavaScript scaffolding, Material icon ligature implementation, generated classes, and script/style internals. These are reported but not contract failures because they are not user-visible product UI and do not alter product behavior. |
| Replacement preview/document-metadata findings | The generated document title contains the screen name. This is preview/document metadata and is not a product-contract failure because it is not rendered as product content in the reviewed screen. |
| Replacement corrected contract validation | `CONTRACT_VALID`; required user-visible content exists; forbidden user-visible/accessibility-exposed product content is absent; no unlisted visible product feature/action/data was found. |
| Human design decision after replacement | First shell visual direction is preferred over the strictly reduced replacement. Do not delete or regenerate the first shell. |
| First shell design-proposed features | Notifications recorded as `DPF-001`; Help / Support Access recorded as `DPF-002` in `docs/frontend/design/stitch/design-proposed-features.md`. |
| First shell excluded content | Fake identity, fake professional title, debug annotations, route/debug labels, auth/session/status claims, online/system-health claims, compliance/security claims, and unsupported footer claims are not accepted as product features and are not backlog items. |
| Authentication Batch 1 generation | Generated and directly validated accepted visual references for `AUTH-001`, `AUTH-002`, `AUTH-005`, `AUTH-006`, `AUTH-007`, and `AUTH-008` in the active v2 workspace. |
| Authentication Batch 1 human approval | Human visually approved the six accepted Authentication artifacts on 2026-09-21 as visual references. Rejected attempts listed above are not authoritative. |
| App Shell v3 generation | Generated exactly one fresh shell candidate: `projects/17116545761229201855/screens/764a9361e16948b0be831e4bf28f584b`, titled `Shell / APP-SHELL / Nurse / Desktop v3 — Human Review Candidate`, using `assets/6536256059106605307`. |
| App Shell v3 direct validation | Direct artifact inspection found required brand, top application bar, Nurse primary navigation (`Exams`, `Preparation Packages`, `Products`, `Profile`), neutral `Account`, `Sign out`, DPF-001 Notifications icon, DPF-002 Help icon, and a clear content region. No fake user identity, unsupported system/security/compliance claims, service-health claims, token/session status, or permanent universal sidebar were found. |
| App Shell v3 human approval | Human approved App Shell v3 visual chrome as the authenticated visual baseline on 2026-09-21. Representative Exams content (`All modules`, `Active`, `Archived`, `Filter views...`, placeholder cards, `Destination Content Region`) is shell demonstration content only and is explicitly non-authoritative for Exam product behavior. |

## Design-Led Feature Discovery Protocol

All future Stitch generation must follow `docs/frontend/design/stitch/system-design-contract.md` Sections 2A through 2C. Stitch may surface useful product ideas, but generated functionality must be classified after generation as `AUTHORITATIVE_FEATURE`, `DESIGN_PROPOSED_FEATURE`, `VISUAL_ONLY_ELEMENT`, or `UNSUPPORTED_CLAIM`. Design-proposed features must be added to `docs/frontend/design/stitch/design-proposed-features.md` before a screen is fully reviewed. Unsupported factual claims must not remain as user-facing truth unless separately verified and authorized. Tool success alone is not acceptance.

## Replacement Manifest — Shell / APP-SHELL / Nurse / Desktop v2

| Field | Value |
|---|---|
| Screen ID / canonical name | `Shell / APP-SHELL / Nurse / Desktop v2` |
| Actor / audience | Authenticated Nurse |
| Route | Shell-level artifact; no route behavior is designed. Demonstrated active family is Exams. |
| Purpose | Persistent authenticated application shell and primary Nurse navigation. |
| REQUIRED | Nursing Platform brand/home affordance; persistent top application bar; Exams; Preparation Packages; Products; Profile; neutral account affordance; main content region. |
| ALLOWED | Page heading `Exams`; neutral non-semantic layout placeholder only if needed to demonstrate spacing; account menu action labels `Account` and `Sign out` under the account affordance, but the menu does not need to be shown open. |
| FORBIDDEN | Help; Notifications; invented user names; email addresses; role titles displayed as fake profile data; avatars representing a specific invented person; auth/token/session status; online/offline/service-health claims; HIPAA claims; GDPR claims; WCAG compliance claims; security claims; footer claims; support links; invented routes; invented widgets; metrics; exam counts; recommendations; analytics; fake feature data; internal screen IDs; route IDs; CSS/token/debug labels; viewport/debug labels; design annotations; implementation annotations. |
| Navigation | Primary navigation is exactly `Exams`, `Preparation Packages`, `Products`, `Profile`. Exams is the demonstrated active destination. No other destinations. |
| User-visible data | Brand text `Nursing Platform`; navigation labels; neutral account label `Account`; optional main heading `Exams`; optional neutral placeholder text that does not imply real data or behavior. |
| Actions | Brand/home affordance; primary navigation labels; account affordance; conceptual account actions `Account` and `Sign out`. No Help, Notifications, support, footer, status, or feature actions. |
| States | Static desktop shell with Exams active. No loading, empty, auth, online/offline, service-health, notification, or data states. |
| Responsive requirements | Desktop only for this artifact. Maintain professional desktop top-app-bar composition and content-region spacing. |
| RTL requirements | Not in scope for this artifact. Do not generate RTL variant. Preserve RTL-safe visual foundation only through design system. |
| Accessibility requirements | Visible text labels for primary navigation; active state must not rely on color alone; neutral account affordance has accessible label; main content region is visually clear; no internal annotations. |
| Explicit non-goals | Do not design a real Exams feature screen; do not add fake exam cards, counts, history, analytics, recommendations, notifications, dashboards, widgets, user identity, compliance/security/status claims, footer content, or additional routes. |

## Review Status

Authentication Batch 1 (`AUTH-001`, `AUTH-002`, `AUTH-005`, `AUTH-006`, `AUTH-007`, `AUTH-008`) is `HUMAN_APPROVED_VISUAL_REFERENCE` as of 2026-09-21. App Shell v3 (`projects/17116545761229201855/screens/764a9361e16948b0be831e4bf28f584b`) is `HUMAN_APPROVED_AUTHENTICATED_VISUAL_BASELINE` for authenticated shell chrome as of 2026-09-21. The representative Exams content inside that artifact is non-authoritative demonstration content only. Notifications and Help / Support Access remain `DESIGN_PROPOSED_FEATURE` only and are not implementation authority. Fake identity/debug content and unsupported security/compliance/status claims remain rejected. Do not treat visual approval as implementation completion.
