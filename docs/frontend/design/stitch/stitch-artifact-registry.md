# Stitch Artifact Registry

```yaml
document_id: NPS-DES-STITCH-ARTIFACT-REGISTRY
status: PHASE_2_SCREEN_GENERATION_ACTIVE
created_at: 2026-09-20
updated_at: 2026-09-22
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

## Authentication, Shared System, And Account Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated family-by-family from `CONTRACT_READY` screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `AUTH-009` | Reset Password Success | `projects/17116545761229201855/screens/ef4d555b00d54f68870318e12d251780` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found anonymous auth shell, required reset-success heading/copy, and `Sign in` action. No password/token/session data, unsupported claims, footer, fake identity, or authenticated navigation found. |
| `AUTH-010` | Session Expired | `projects/17116545761229201855/screens/43b0f224781949ad8be651df1991a257` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found required `Session expired` heading, body copy, `Sign in`, and safe `Go to home` action. No token/session/JWT details, protected-resource data, unsupported claims, footer, fake identity, or authenticated navigation found. |
| `AUTH-011` | Access Denied | `projects/17116545761229201855/screens/e869c0c2568444b5bc2638036f5fa8c0` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found privacy-safe denial copy and safe `Go to home`/`Sign in` actions. No role names, permission keys, protected-resource details, route IDs, tokens, support/help/notifications content actions, footer, or unsupported claims found. |
| `ONB-001` | Profile Onboarding | `projects/17116545761229201855/screens/741064daa5ec424c883f96a1caee3980` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `a4bdbce7fc104ceab34581f21e86a1b3`. Direct HTML validation found App Shell v3-aligned authenticated chrome, required `Complete your profile` heading/copy, `First name` and `Last name` required fields with max-length helper text and no placeholder/value drift, `Save`, and shell `Sign out`. No nurse/employer profile fields, tokens, raw IDs, `isProfileComplete`, footer, unsupported claims, or product-body changes found. |
| `ACC-001` | Account Overview | `projects/17116545761229201855/screens/320619ab3a3d43688c2372d25511c5b0` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `004ff3b651fb42aca5244b3fbd1d25e1`. Direct HTML validation found App Shell v3-aligned authenticated chrome and identity-only account overview with `Email`, `Username`, `First name`, `Last name`, `Email verification`, and `Edit personal details`. No password/security/session/settings/credentials wording, permissions, raw IDs, timestamps, `isProfileComplete`, footer, unsupported claims, or product-body changes found. |
| `ACC-002` | Personal Details Edit State | `projects/17116545761229201855/screens/0dd95bb8ced74c7e847975014b720e3d` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `f397d94810034593baa0793e87f36f15`. Direct HTML validation found App Shell v3-aligned authenticated chrome, same-route edit-state design with `Edit personal details`, `First name`, `Last name`, max-length helper text, empty inputs with no placeholder/value drift, `Save`, and `Cancel`. No email/username editing, password/security/session/device/notification/deletion controls, raw IDs, tokens, footer, unsupported claims, or product-body changes found. |

Rejected/superseded generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `ACC-001` | `projects/17116545761229201855/screens/09a6f96af6b9474cb95bfa236bf7a977` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Account Overview attempt used out-of-scope `credentials` wording in direct HTML. Superseded by accepted identity-only replacement `004ff3b651fb42aca5244b3fbd1d25e1`. |
| `ONB-001` | `projects/17116545761229201855/screens/a4bdbce7fc104ceab34581f21e86a1b3` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `741064daa5ec424c883f96a1caee3980`; product body remains unchanged. |
| `ONB-001` | `projects/17116545761229201855/screens/944359351fbf4882a4048e3cebbe1a10` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement added example placeholder text to the onboarding fields, creating product-body drift. Superseded by accepted replacement `741064daa5ec424c883f96a1caee3980`. |
| `ACC-001` | `projects/17116545761229201855/screens/004ff3b651fb42aca5244b3fbd1d25e1` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `320619ab3a3d43688c2372d25511c5b0`; product body remains unchanged. |
| `ACC-002` | `projects/17116545761229201855/screens/f397d94810034593baa0793e87f36f15` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `0dd95bb8ced74c7e847975014b720e3d`; product body remains unchanged. |
| `ACC-002` | `projects/17116545761229201855/screens/fb8efc4eba9b4b6381980e7de8169769` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement added placeholder text to the personal-details fields, creating product-body drift. Superseded by accepted replacement `0dd95bb8ced74c7e847975014b720e3d`. |

## Nurse Profile Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Nurse Profile `CONTRACT_READY` screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `NUR-001` | Profile Overview | `projects/17116545761229201855/screens/d2d3ea96cca0405a9622b73acaeb6e35` | SHELL_ALREADY_CONFORMANT_PENDING_HUMAN_REVIEW | Prior accepted artifact remains the authority for this screen. Direct shell comparison found it already materially aligned to the approved authenticated shell while preserving profile overview composition with professional identity, supported base-profile facts, section summaries, no completion score/status, no fake identity, and no unsupported profile/share/recruiter claims. |
| `NUR-002` | Profile Summary State | `projects/17116545761229201855/screens/875e554e97d64f958e97916ba8da891b` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `6d7f13d7750c47fd825e1836fdb3f110`. Direct HTML validation found App Shell v3-aligned authenticated chrome and the profile summary/first-time state aligned to the no-profile/sparse-profile contract, without completion percentage, unsupported profile status, fake identity, footer, or product-body drift. |
| `NUR-003` | Personal Information | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery pass found no acceptable artifact. Directly retrieved candidates continued to include material defects such as invented character limits, invented maximum years-of-experience rules, and unsupported verification-adjacent recruitment copy; the final replacement timed out and no acceptable recovered replacement was found. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `NUR-004` | Experience List | `projects/17116545761229201855/screens/92cec4b1916a460c96eed9dd2ca56642` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `06c2092a51bd4fd08b65aa74903dbc13`. Direct HTML validation found App Shell v3-aligned authenticated chrome, approved experience list content/actions, inline delete confirmation, no visible design annotations, no fake identity, footer, or product-body drift. |
| `NUR-005` | Experience Form State | `projects/17116545761229201855/screens/c46117e1b1b6447ba9c51a37302e9380` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalized accepted artifact. Direct HTML validation found approved experience form/delete-confirmation state with App Shell v3-aligned chrome and no unsupported uniqueness, employment-type, profile-completion, fake identity, footer, or product-body drift. |
| `NUR-006` | Education List | `projects/17116545761229201855/screens/ec3329c1812e4fc7aa36daeac5605bb4` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `6a0f2d2f5bae4a1e8ddfd370b46f765f`. Direct HTML validation found App Shell v3-aligned authenticated chrome, education records, approved date-line formats, Add/Edit/Delete actions, inline delete confirmation, and no state-example gallery, certificate content, completion score, raw IDs, unsupported status claims, footer, or product-body drift. |
| `NUR-007` | Education Form State | `projects/17116545761229201855/screens/563c6a3c987c45ce8a97002a3bbca21a` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalized accepted artifact. Direct HTML validation found education form fields, validation summary, end-date error, Save education, Cancel, App Shell v3-aligned chrome, and no state gallery, modal/delete confirmation, route IDs, fake identity, unrelated profile sections, footer, or product-body drift. |
| `NUR-008` | Certificates List | `projects/17116545761229201855/screens/d0b529ffcf104ef299e3c09c9568d854` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalized accepted artifact. Direct HTML validation found certificate list cards with plain issuer text, issue/optional expiration dates, credential ID, `Open credential link`, Edit/Delete, inline delete confirmation, App Shell v3-aligned chrome, and no verified/official/trusted status, Active/Expired/Valid/Expiring-soon labels, state examples, unrelated sections, footer, or product-body drift. |
| `NUR-009` | Certificate Form State | `projects/17116545761229201855/screens/abfbb16f30764777bca58a32bea76257` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `942bcff423aa4bdeac43b0ad6de1868b`. Direct HTML validation found App Shell v3-aligned authenticated chrome, certificate form fields, URL validation summary/error, Save certificate, Cancel, and no verification/status wording, modal/drawer, route IDs, fake identity, footer, unsupported claims, or product-body drift. |
| `NUR-010` | Skills | `projects/17116545761229201855/screens/3156d8c08fda437ab0c704b59e389213` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalized accepted artifact. Direct HTML validation found feature-local chip editor with skill input, duplicate validation, required rules helper, removable chips, Save skills, Cancel, App Shell v3-aligned chrome, and no taxonomy/autocomplete, drag/reorder, proficiency, route IDs, footer, unrelated sections, or product-body drift. |
| `NUR-011` | Languages | `projects/17116545761229201855/screens/ac90e46e18d540f5903a56d0fa9b8d3d` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `f77e083faedd42cea3a78b071fb489ec`. Direct HTML validation found App Shell v3-aligned authenticated chrome, row-based language/proficiency editor, exact proficiency options, duplicate-language validation, Add language, Save languages, Cancel, and no unsupported proficiency values, drag/reorder, unrelated sections, raw IDs, footer claims, or product-body drift. |
| `NUR-CONTACT` | Contact Requests | `projects/17116545761229201855/screens/8a354ffa137b46dd86eceaad3465d5d2` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization recovery replacement for prior artifact `31473847b8eb4c3185a0f58777e28c80`. Direct HTML validation found exactly one App Shell v3-aligned authenticated chrome, status filter, safe employer/request facts, plain-text statuses, pending Approve/Reject actions, inline Reject confirmation, terminal rows without actions, pagination, and integrated empty/no-results reference states. No duplicate shell, raw request IDs, employer private internals, messages/reasons/reply fields, permission keys, role claims, urgency/priority, semantic status badges, footer, unsupported claims, or product-body drift found. |

Rejected/superseded Nurse Profile generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `NUR-001` | `projects/17116545761229201855/screens/738fd82f37634b869eda8e7cf3889521` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Debug/reference content and unsupported validity wording. Superseded by accepted `d2d3ea96cca0405a9622b73acaeb6e35`. |
| `NUR-001` | `projects/17116545761229201855/screens/9202650b067e4a8599f91bc8adce1a3a` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Document-title/forbidden icon-ligature issue from prior validation. Superseded by accepted `d2d3ea96cca0405a9622b73acaeb6e35`. |
| `NUR-002` | `projects/17116545761229201855/screens/6d7f13d7750c47fd825e1836fdb3f110` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `875e554e97d64f958e97916ba8da891b`; product body remains unchanged. |
| `NUR-003` | `projects/17116545761229201855/screens/1cfe7b9fa79842bf9d47baf3e3baaad2` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Delayed recovered timeout artifact used unsupported `Max 120 characters` headline constraint and did not match authoritative optional/required semantics. |
| `NUR-003` | `projects/17116545761229201855/screens/64124d9dca29481bbed501d53e6aaca4` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Fresh replacement fixed the headline length but incorrectly marked optional profile fields/country controls as required. Second fresh retry timed out and did not recover, so `NUR-003` remains `STITCH_GENERATION_BLOCKED_TEMPORARY` for this pass. |
| `NUR-003` | `projects/17116545761229201855/screens/40914ae220c449e8a8ea132a1069beb1` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Targeted recovery candidate included unsupported `verified summary` recruitment helper copy. |
| `NUR-003` | `projects/17116545761229201855/screens/5c4ba45bdcfc4f8ca9f0df45becb43de` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Delayed recovered replacement still invented field character limits and a maximum years-of-experience rule. |
| `NUR-004` | `projects/17116545761229201855/screens/73822cb69a0d442bb9855e804dbe0bdb` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Visible design annotation labels. Superseded by accepted `06c2092a51bd4fd08b65aa74903dbc13`. |
| `NUR-004` | `projects/17116545761229201855/screens/06c2092a51bd4fd08b65aa74903dbc13` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `92cec4b1916a460c96eed9dd2ca56642`; product body remains unchanged. |
| `NUR-005` | `projects/17116545761229201855/screens/da2c04d28c8548be896dc897a5b0ae89` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `c46117e1b1b6447ba9c51a37302e9380`; product body remains unchanged. |
| `NUR-006` | `projects/17116545761229201855/screens/98b508ca88c948d5bb33772f2a94a411` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Delayed recovered timeout artifact rendered visible `Education State Examples` and separate state panels on the product screen. Superseded by accepted `6a0f2d2f5bae4a1e8ddfd370b46f765f`. |
| `NUR-006` | `projects/17116545761229201855/screens/6a0f2d2f5bae4a1e8ddfd370b46f765f` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `ec3329c1812e4fc7aa36daeac5605bb4`; product body remains unchanged. |
| `NUR-007` | `projects/17116545761229201855/screens/e64e103caf4942b68dc83a1043dcbf25` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `563c6a3c987c45ce8a97002a3bbca21a`; product body remains unchanged. |
| `NUR-008` | `projects/17116545761229201855/screens/fab0bd4e943d4acfaa00ea2980271db9` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Certificates List attempt used `verified_user` iconography beside issuing organizations, implying verification/trust despite the no-verified/official-badge rule. Superseded by accepted `f2ca29048bd14819814271042949e30b`. |
| `NUR-008` | `projects/17116545761229201855/screens/f2ca29048bd14819814271042949e30b` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `d0b529ffcf104ef299e3c09c9568d854`; product body remains unchanged. |
| `NUR-009` | `projects/17116545761229201855/screens/942bcff423aa4bdeac43b0ad6de1868b` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `abfbb16f30764777bca58a32bea76257`; product body remains unchanged. |
| `NUR-010` | `projects/17116545761229201855/screens/51eab55569ab49909c69279ec9fa706b` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `3156d8c08fda437ab0c704b59e389213`; product body remains unchanged. |
| `NUR-011` | `projects/17116545761229201855/screens/f77e083faedd42cea3a78b071fb489ec` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `ac90e46e18d540f5903a56d0fa9b8d3d`; product body remains unchanged. |
| `NUR-CONTACT` | `projects/17116545761229201855/screens/31473847b8eb4c3185a0f58777e28c80` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized recovery replacement `8a354ffa137b46dd86eceaad3465d5d2`; product body remains unchanged. |
| `NUR-CONTACT` | `projects/17116545761229201855/screens/30abf843c3c8467c9f1cbea90ce17693` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement duplicated the shell/header in direct HTML. Superseded by recovery replacement `8a354ffa137b46dd86eceaad3465d5d2`. |

## Exams Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Exams `CONTRACT_READY` screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `EXM-001` | Exam Catalog | `projects/17116545761229201855/screens/88c5c6b9fb244fc887e1f798360509fa` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `232c5af699c44991acee25ede383ce3a`. Direct HTML validation found App Shell v3-aligned authenticated chrome, filters, exam cards, analytics/history entries, pagination/no-results behavior, and no raw IDs, answers, correctness, purchase CTA, footer, unsupported claims, or product-body drift. |
| `EXM-002` | Exam Detail | `projects/17116545761229201855/screens/6ea22796476e4bd6a2c9c3e6daf0b5ba` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `10e1ba4fa8b04b6995e0e8ce97556eb4`. Direct HTML validation found App Shell v3-aligned authenticated chrome, exam detail facts, instructions summary treatment, Back to exams, View instructions, and no questions, answers, correctness, raw IDs, footer, unsupported claims, or product-body drift. |
| `EXM-004` | Exam Instructions | `projects/17116545761229201855/screens/148914b9eb4b464297bc349780b30374` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `2e8a32ace14c432e8289bb2314126158`. Direct HTML validation found App Shell v3-aligned authenticated chrome, generic backend-supplied instructions text, duration/question/passing facts, start/resume confirmation treatment, and no simultaneous empty fallback, invented instructions/rules, raw IDs, footer, unsupported claims, or product-body drift. |
| `EXM-005` | Exam Session | `projects/17116545761229201855/screens/cccc35faf40a4bed9a0e661d992cf5ff` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `cdc9aad819524ac09d0060855349aa09`. Direct HTML validation found App Shell v3-aligned authenticated chrome, one-question-at-a-time exam session with timer, near-expiry warning, save error/retry, option selection, Save answer, Previous/Next/Submit controls, and no correctness, answer key, explanations, result metrics, raw IDs, all-question navigator, footer, or product-body drift. |
| `EXM-006` | Submit Confirmation | `projects/17116545761229201855/screens/088376370f414da6830bb0558da88ffc` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `f8e10c91253f4540a0716f199e2f8271`. Direct HTML validation found App Shell v3-aligned authenticated chrome, submit confirmation with exam title, `Question 120 of 120`, total/answered/unanswered counts, cancellation action, disabled/loading `Submitting...` button state, and no question text, answer options, correctness, result metrics, visible state annotations, raw IDs, footer, unsupported claims, or product-body drift. |
| `EXM-007` | Exam Result | `projects/17116545761229201855/screens/c605f30a477041d980c81114cc8a046a` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `4b15edf6f4cb43fbbc27fa73b3a99632`. Direct HTML validation found App Shell v3-aligned authenticated chrome, aggregate finalized result with heading/context, terminal status, score/max/percentage/passed/correct answers/questions, Back to exams, Review answers, and no official/candidate-performance claims, provenance, timestamps, review content, analytics, system/sync language, raw IDs, footer, unsupported claims, or product-body drift. |
| `EXM-008` | Exam Analytics | `projects/17116545761229201855/screens/e427fe6cdcc549bea40b9e71bd81c125` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `b070909c744b4d1483042d2acb6d40e2`. Direct HTML validation found App Shell v3-aligned authenticated chrome, approved heading/copy, filters, Overview, Performance by exam, Performance by category, and Performance over time sections rendered as textual metrics/tables/lists. No charts, bands, recommendations, contradictory no-data notes, provenance, raw IDs, local-derivation claims, footer, or product-body drift found. |
| `EXM-009` | Answer Review | `projects/17116545761229201855/screens/76f3ac3096334dad807873d814f8f1ea` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `da514a9409434b03af140fbefe1ac91d`. Direct HTML validation found App Shell v3-aligned authenticated chrome, one-question finalized answer review with pager, factual status, read-only option rows, `Your answer`, `Correct answer`, explanation, points, Previous/Next, and Back to result. No active inputs, aggregate result metrics, analytics, raw IDs, package guidance, footer, unsupported claims, or product-body drift found. |
| `EXM-010` | Exam History | `projects/17116545761229201855/screens/40e4a6d17fce4e4c966978cc715d5667` | REGENERATED_NORMALIZED_AND_VALID_PENDING_HUMAN_REVIEW | Shell-normalization replacement for prior artifact `3e5f4653630446678736491c05b12117`. Direct HTML validation found App Shell v3-aligned authenticated chrome, status filter, attempt history rows with approved status/date/percentage/pass/action fields, and pagination. No optional state/reference section, analytics, score/max/correct counts, provenance, raw IDs, sync/system language, footer, unsupported claims, or product-body drift found. |

Rejected/superseded Exams generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `EXM-002` | `projects/17116545761229201855/screens/6b23383e4aec4a5aa6c8e2664619cc96` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Detail attempt invented unsupported exam instructions/rules. Superseded by accepted `10e1ba4fa8b04b6995e0e8ce97556eb4`. |
| `EXM-004` | `projects/17116545761229201855/screens/c51af566d4b044299821e81765649f9a` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Instructions attempt showed supplied instructions and empty fallback simultaneously. Superseded by accepted `2e8a32ace14c432e8289bb2314126158`. |
| `EXM-006` | `projects/17116545761229201855/screens/8f8d0a60978c4597a552e5aed0d94a7e` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Submit Confirmation attempt rendered a visible duplicate-submit preview annotation and active-question background content. Superseded by accepted `f8e10c91253f4540a0716f199e2f8271`. |
| `EXM-007` | `projects/17116545761229201855/screens/78d041cb77d143debaf3a1327e9e35d3` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Result attempt included unsupported blueprint/finalized-attempt/system status/synchronization wording. Superseded by accepted `4b15edf6f4cb43fbbc27fa73b3a99632`. |
| `EXM-010` | `projects/17116545761229201855/screens/a399ac9c21dd459fb32ec315a756fa00` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam History attempt rendered a visible `System status views` section and sync-related content. Superseded by accepted `3e5f4653630446678736491c05b12117`. |
| `EXM-001` | `projects/17116545761229201855/screens/232c5af699c44991acee25ede383ce3a` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `88c5c6b9fb244fc887e1f798360509fa`; product body remains unchanged. |
| `EXM-002` | `projects/17116545761229201855/screens/10e1ba4fa8b04b6995e0e8ce97556eb4` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `6ea22796476e4bd6a2c9c3e6daf0b5ba`; product body remains unchanged. |
| `EXM-004` | `projects/17116545761229201855/screens/2e8a32ace14c432e8289bb2314126158` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `148914b9eb4b464297bc349780b30374`; product body remains unchanged. |
| `EXM-004` | `projects/17116545761229201855/screens/f54b9eadd8794e98951e7baa92181495` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement invented exam instruction/rule copy. Superseded by accepted replacement `148914b9eb4b464297bc349780b30374`. |
| `EXM-005` | `projects/17116545761229201855/screens/cdc9aad819524ac09d0060855349aa09` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `cccc35faf40a4bed9a0e661d992cf5ff`; product body remains unchanged. |
| `EXM-006` | `projects/17116545761229201855/screens/f8e10c91253f4540a0716f199e2f8271` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `088376370f414da6830bb0558da88ffc`; product body remains unchanged. |
| `EXM-006` | `projects/17116545761229201855/screens/647890890bce41dca786012debc49ad7` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement exposed a visible state-annotation label. Superseded by accepted replacement `088376370f414da6830bb0558da88ffc`. |
| `EXM-007` | `projects/17116545761229201855/screens/4b15edf6f4cb43fbbc27fa73b3a99632` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `c605f30a477041d980c81114cc8a046a`; product body remains unchanged. |
| `EXM-007` | `projects/17116545761229201855/screens/29ebc258c5194b6db28803c3439134de` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement used unsupported `Official evaluation` wording. Superseded by accepted replacement `c605f30a477041d980c81114cc8a046a`. |
| `EXM-008` | `projects/17116545761229201855/screens/b070909c744b4d1483042d2acb6d40e2` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `e427fe6cdcc549bea40b9e71bd81c125`; product body remains unchanged. |
| `EXM-008` | `projects/17116545761229201855/screens/e2322f51873b4b05be53a0562bc43c69` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement included a contradictory no-data note while rows were present. Superseded by accepted replacement `e427fe6cdcc549bea40b9e71bd81c125`. |
| `EXM-009` | `projects/17116545761229201855/screens/da514a9409434b03af140fbefe1ac91d` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `76f3ac3096334dad807873d814f8f1ea`; product body remains unchanged. |
| `EXM-010` | `projects/17116545761229201855/screens/3e5f4653630446678736491c05b12117` | SUPERSEDED_BY_SHELL_NORMALIZATION | Prior contract-valid artifact superseded by shell-normalized replacement `40e4a6d17fce4e4c966978cc715d5667`; product body remains unchanged. |
| `EXM-010` | `projects/17116545761229201855/screens/5528b7f0a2924b819f97993eaf827e44` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First shell-normalized replacement used an ambiguous `Score` label in attempt history. Superseded by accepted replacement `40e4a6d17fce4e4c966978cc715d5667`. |

## Preparation Packages Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Preparation Packages `CONTRACT_READY` screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work. `PP-MATERIAL-READER` was not generated because its canonical status remains `BACKEND_BLOCKED`.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `PP-001` | Package Offers | `projects/17116545761229201855/screens/46142e7ae7e047d8b83ccb0607d254ed` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found the public anonymous shell, package-offer filters, two authorized offer cards, and pagination. No purchase/payment/entitlement/progress/material-reader/offline/status-reference/footer content found. |
| `PP-002` | Package Offer Detail | `projects/17116545761229201855/screens/cddd02cf617645d8be2ad0b4b9c04365` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found public package-offer detail facts, included content summary, access duration, and Back to offers actions only. No commerce/payment/material-reader/offline/start/practice/report-result/raw-ID/footer content found. |
| `PP-003` | My Preparation Packages | `projects/17116545761229201855/screens/3f464587aa57403c8e81d4d7a94f020e` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found authenticated Preparation Packages shell, owned entitlement cards, backend-style status/access/rights/attempt/report indicators, and pagination. No price/order/provider/raw-ID/material-reader/download/offline/reference/footer content found. |
| `PP-004` | Entitlement Detail | `projects/17116545761229201855/screens/0d56286fc5114969a4227c1d24b0759c` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found entitlement summary, rights, purchase snapshot dates, practice facts, package exam/report availability, and approved actions only. No invented duration/modules/report-analysis copy, price/payment/provider/raw IDs/material-reader/download/offline/percentage/reference/footer content found. |
| `PP-005` | Practice | `projects/17116545761229201855/screens/41f5fc1fd7db4ab4b5b70f73e946fd98` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found package practice progress counts, practice collections, active practice item, option selection, Save answer, and Back to entitlement. No material reader/download/offline/official-exam/answer-key/rationale/adaptive/percentage/report-result/reference/footer content found. |
| `PP-EXAM-START` | Package Exam Start Confirmation | `projects/17116545761229201855/screens/6f74c1d6b21d45ceaffe8a7dfe7dadb0` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found package exam start confirmation with package/exam context, attempt availability, attempt-consumption message, Cancel, Start package exam, Resume package exam, and Back to entitlement. No exam questions/answers/results/timer/payment/provider/raw-ID/system-status/reference/footer content found. |
| `PP-007` | Package Report | `projects/17116545761229201855/screens/3d75a0c5ad0b4d41aee81c41cb16ea77` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found Summary, Per-topic report, and Recommended package content with only approved metrics, rows, names, and types. No recommendation actions, question text, answers/answer keys, rationales, provenance, payment/provider/raw-ID/material-reader/download/offline/reference/footer content found. |

Rejected/superseded Preparation Packages generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `PP-001` | `projects/17116545761229201855/screens/46c2322d886442d699495f863e21ff63` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Package Offers attempt rendered visible `System Status Reference Views` and a `verified` icon ligature. Superseded by accepted `46142e7ae7e047d8b83ccb0607d254ed`. |
| `PP-004` | `projects/17116545761229201855/screens/9488cd80433344d69d7105cb7b4772a6` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Entitlement Detail attempt added unauthorized `Self-paced modules`, `NMC CBT Adult Nursing standard duration`, and report-analysis lock copy. Superseded by accepted `0d56286fc5114969a4227c1d24b0759c`. |
| `PP-007` | `projects/17116545761229201855/screens/0ac8a9848a624c88ba41828d6b57a3e8` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Package Report attempt added extra presentation labels/actions (`overall score`, `Start practice`, `View summary`) beyond the report contract. Superseded by accepted `3d75a0c5ad0b4d41aee81c41cb16ea77`. |

## Commerce Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Commerce `CONTRACT_READY` product discovery/detail screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize checkout/payment/order implementation work. `COM-003` through `COM-008` were not generated in this checkpoint.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `COM-001` | Product Catalog | `projects/17116545761229201855/screens/21862f11a45049cb9f95f4af1ad65361` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found authenticated Products shell, two product cards with safe title/description/type/availability/price/currency fields, View details actions, and pagination. No Purchase/Buy/Checkout/order/payment/provider/search/filter/raw-ID/reference/footer content found. |
| `COM-002` | Product Detail | `projects/17116545761229201855/screens/31c3beeed6ee4a6abf5a147b1c27880e` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found authenticated Products shell, product detail heading/context, safe product type/availability/name/description/price/currency facts, and Back to Products only. No Purchase/Buy/Checkout/order/payment/provider/raw-ID/invented-benefit/reference/footer content found. |

## Employer Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Employer `CONTRACT_READY` screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work. `EMP-005` was not generated because Candidate Detail remains deferred/backend-blocked.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `EMP-001` | Employer Home | `projects/17116545761229201855/screens/f8e292e2af1e4dccb75214f063680f97` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found focused Employer workspace entries for Candidate Search and Employer Requests plus factual profile readiness guidance. No metrics/KPIs/counts/activity-feed/private-candidate/reference/footer content found. |
| `EMP-002` | Candidate Search | `projects/17116545761229201855/screens/5c50950bee4243bf80f66486eb1ba07f` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found approved candidate search filters, Apply/Clear actions, and pagination only. No candidate cards, Candidate Detail/View profile/request action, sorting, counts, recommendations, private data, reference, or footer content found. |
| `EMP-003` | Candidate Results | `projects/17116545761229201855/screens/e0639590cf4e40b381aa00e5e0c83494` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement after one unrecovered timeout; direct HTML validation found safe candidate-list fields, Request contact actions, and pagination. No Candidate Detail/View profile/private data/raw IDs/scores/recommendations/counts/footer content found. |
| `EMP-004` | Candidate Empty / Filtered States | `projects/17116545761229201855/screens/27de589b1de64f3da5ff7dbc6bdfec81` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement after one unrecovered timeout; direct HTML validation found approved filters, filtered-empty copy, Clear filters action, and pagination. No hidden counts/recommendations/saved-search/alerts/Candidate Detail/private data/reference/footer content found. |
| `EMP-006` | Candidate Request | `projects/17116545761229201855/screens/797c6e47de2f42628ba1250cc3bbe723` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found one safe candidate summary and request confirmation with Cancel/Send request only. No extra candidate rows, success banners, message/note/reason fields, Candidate Detail/View profile/private data/raw IDs/footer content found. |
| `EMP-007` | Employer Requests | `projects/17116545761229201855/screens/faabbe70e39a4a1fb7ad181abbc57f3e` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found safe request-list fields, approved status filter, Pending/Approved/Cancelled statuses, View request, Pending-only Cancel request, and pagination. No private data, bulk/approve/reject actions, notes/messages/reasons/raw IDs/reference/footer content found. |
| `EMP-008` | Employer Request Detail | `projects/17116545761229201855/screens/11ab2f9c11b4435faf48ed4ef7c57e97` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found request detail fields, Pending status, Back to Requests, and cancel confirmation with Keep request/Cancel request. No Target Candidate label, Candidate Detail/View profile/private data/message/reason/raw IDs/reference/footer content found. |

Rejected/superseded Employer generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `EMP-002` | `projects/17116545761229201855/screens/24be350af13649b487e472fe58f45ce9` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Candidate Search attempt merged candidate result cards and `View profile`, implying deferred Candidate Detail. Superseded by accepted `5c50950bee4243bf80f66486eb1ba07f`. |
| `EMP-006` | `projects/17116545761229201855/screens/b9a2e404be26447090824205a8006eef` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Candidate Request attempt added an extra candidate row, invented `Candidate direct communication inquiry`, and split success action text into `View`. Superseded by accepted `797c6e47de2f42628ba1250cc3bbe723`. |
| `EMP-008` | `projects/17116545761229201855/screens/4f1c7d062266451793e640438c16ea57` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Request Detail attempt added an extra `Target Candidate` label beyond the approved field list. Superseded by accepted `11ab2f9c11b4435faf48ed4ef7c57e97`. |

## Administration Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Administration `CONTRACT_READY` screen contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work. `ADM-DASHBOARD`, `ADM-004`, `ADM-009`, and `ADM-010` were not generated because they remain `BACKEND_BLOCKED`/deferred. This checkpoint is a partial first Administration sub-batch; temporary Stitch generation blockers do not change the canonical screen-contract status of blocked rows.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `ADM-ENTRY` | Admin Workspace | `projects/17116545761229201855/screens/c89c44bfe7e543bb8448604533b89886` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found approved Admin workspace destination cards and permission-aware guidance. No dashboard metrics, KPI/stat/activity/system-health/recruitment/payment-order content, roles/permissions management, Admin payment orders, Admin recruitment, fake account identity, Help, Notifications, unsupported security/compliance/status claims, raw IDs, or debug annotations found. |
| `ADM-002` | Admin Users | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery directly retrieved two candidates, but both included unsupported `verified platform user records` copy beyond the safe user-list contract. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-003` | Admin User Detail | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery candidate still failed direct HTML validation because it added out-of-contract governance/policy claims about historical exam attempts and generated scoring records; the final replacement timed out and did not recover through delayed project metadata checks. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-005` | Exam Category Administration | `projects/17116545761229201855/screens/86190953d0384d1dae0b403534fe06bd` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found v1 Exam Category administration with Country, Category name, Slug, Description, Display order, Active state, create/update/archive/restore/delete actions, confirmations, and pagination. No generic reference-data console, unrelated reference entities, raw IDs, footer, Help, Notifications, unsupported security/compliance/status claims, or debug annotations found. |
| `ADM-006` | Admin Exams | `projects/17116545761229201855/screens/d81c321809974164a6f81938aeeb356d` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found Admin exams filters, dense table, approved lifecycle actions, pagination, and archive/delete confirmations. No learner catalog/session/result behavior, attempts, candidate enrollments/history, analytics, question/answer content, raw IDs, route IDs, permission keys, footer, Help, Notifications, or unsupported security/compliance/status claims found. |
| `ADM-007` | Exam Detail | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | First generated artifact failed direct HTML validation because it invented learner/test-taking instruction behavior and a URL-path-like slug helper. Replacement fixed those issues but added an unsupported audit-record claim, so it was also rejected. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-008` | Exam Versions | `projects/17116545761229201855/screens/20cea23835bc447580899a4339d0a5c2` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found exam context, draft validation panel, version lifecycle table, and publish/retire/delete confirmations. No learner version selection, attempts, candidate history, audit logs, analytics, question/answer content, raw IDs, route IDs, permission keys, footer, Help, Notifications, or unsupported security/compliance/status claims found. |
| `ADM-QUESTIONS` | Questions / Answer Options | `projects/17116545761229201855/screens/e62464c9bce04b019a60975cc0e0ab20` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found Admin question authoring form, backend-safe question type labels, answer-option management, question inventory, and deactivate/delete confirmations. No visible raw IDs, learner answer-review/test-taking UI, learner attempts, candidates, active exam attempt generation, randomization, unsupported question types, footer, Help, Notifications, or unsupported security/compliance/status claims found. |
| `ADM-PAY-PRODUCTS` | Admin Payment Products | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery directly retrieved candidates, but they included unsupported candidate purchase-token, learner catalog, curriculum verification, access-entitlement, and audit-preservation claims beyond the Admin Payment Products contract. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-PP-TOPICS` | Reporting Topics | `projects/17116545761229201855/screens/dd6fa35da96b460e9df55995f8091888` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found reporting-topic administration with Exam category, Name, Slug, Active state, list/create/update/archive/restore-style controls, filters, and archive confirmation. No diagnostic-report, learner-report, package-attempt, active-evaluation, URL/query-mapping, raw IDs, footer, Help, Notifications, unsupported security/compliance/status claims, or debug annotations found. |
| `ADM-PP-PROFILES` | Reporting Profiles | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery directly retrieved candidates, but they included unsupported auto-assignment, live synchronization, verification, candidate diagnostic-evaluation, and superseding claims beyond the reporting-profile contract. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-PP-MATERIALS` | Study Materials | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery directly retrieved candidates, but they included unsupported learner catalog/progress, active-learner access, interactive-summary/interactive-attachment, and storage/admin object-key claims beyond the study-material contract. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-PP-PRACTICE` | Practice Collections | `projects/17116545761229201855/screens/121e8a2ee2964b308020a6632a293463` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found practice collection/version/item/option authoring with collection title/slug/description, version lifecycle, item prompt/feedback/display order/reporting topic, answer options/correctness, publish/retire confirmations, and no learner practice attempt UI, adaptive practice, analytics, package enrollment, production-release, audit/history-log, footer, Help, Notifications, unsupported security/compliance/status claims, or debug annotations found. |
| `ADM-PP-DEFINITIONS` | Package Definitions | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery directly retrieved candidates, but they included unsupported URL/routing, learner/enrollment/purchase, audit-record, fake module-reference, and learner attempt/access claims beyond the package-definition contract. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `ADM-PP-OFFERS` | Package Offers | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Targeted recovery directly retrieved candidates, but they included unsupported assignment, official diagnostic mock, verification, historical-record, and storefront/catalog-adjacent claims beyond the package-offer contract. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |

Rejected/superseded Administration generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `ADM-003` | `projects/17116545761229201855/screens/d86e379d99834a948563fc626daca309` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Role-update copy mentioned `Session tokens`, violating token/session non-exposure. Replacement did not yield a retrievable accepted artifact. |
| `ADM-002` | `projects/17116545761229201855/screens/0410ce896a5e45808334d9c9dd163316` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved targeted-recovery candidate included unsupported `verified platform user records` copy beyond the safe Admin Users list contract. |
| `ADM-002` | `projects/17116545761229201855/screens/dd790b6f8fd34025beb37a9f5e7ccd69` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved replacement candidate still included unsupported `verified platform user records` copy beyond the safe Admin Users list contract. |
| `ADM-003` | `projects/17116545761229201855/screens/559577079ad442398edb08506cc96648` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Targeted recovery candidate added out-of-contract governance/policy claims about historical exam attempts and generated clinical scoring records. |
| `ADM-006` | `projects/17116545761229201855/screens/384f2e70f4b741b6b67a8c240b757569` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Admin Exams attempt referenced learner/candidate history and session integrity in lifecycle copy. Superseded by accepted `d81c321809974164a6f81938aeeb356d`. |
| `ADM-007` | `projects/17116545761229201855/screens/d61caead20754814b35ff38525e9717c` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Detail attempt invented learner/test-taking behavior, including question navigation, calculator, timer, automated submission, and URL-path-like slug helper text. |
| `ADM-007` | `projects/17116545761229201855/screens/1b0a93f57f594793ac13bf4e79e5f78e` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Replacement removed learner/test-taking behavior but added unsupported audit-record copy. No accepted `ADM-007` artifact remains from this checkpoint. |
| `ADM-008` | `projects/17116545761229201855/screens/f1bad88453474e5ba414b09e3204f9e0` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Versions attempt included learner attempts and audit-log claims in lifecycle confirmations. Superseded by accepted `20cea23835bc447580899a4339d0a5c2`. |
| `ADM-QUESTIONS` | `projects/17116545761229201855/screens/bec06f790b7b4593bf3fb5175727e051` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Questions attempt exposed a raw-looking question ID, learner/attempt/candidate-display wording, and unsupported question-type examples. Superseded by accepted `e62464c9bce04b019a60975cc0e0ab20`. |
| `ADM-PAY-PRODUCTS` | `projects/17116545761229201855/screens/124d0dea86c44772891c45d40f1fc19f` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved targeted-recovery candidate included unsupported candidate purchase-token, learner catalog, verification, access-entitlement, and audit-preservation claims. |
| `ADM-PAY-PRODUCTS` | `projects/17116545761229201855/screens/ae0557ad5764429da5ccc7369636cab2` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved replacement candidate still included unsupported candidate purchase-token, learner catalog, curriculum verification, access-entitlement, and audit-preservation claims. |
| `ADM-PP-TOPICS` | `projects/17116545761229201855/screens/148d0602c23841f3a223775cd3194553` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Reporting Topics attempt included diagnostic-report, learner-report, package-attempt, active-evaluation, and URL/query-mapping concepts beyond the topic CRUD contract. Replacement timed out and no accepted artifact remains from this checkpoint. |
| `ADM-PP-PROFILES` | `projects/17116545761229201855/screens/d9e9fdf128584f61bc91177f4704626d` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved targeted-recovery candidate included unsupported auto-assignment, verification, candidate diagnostic-evaluation, and superseding claims. |
| `ADM-PP-PROFILES` | `projects/17116545761229201855/screens/2d5c9c8e109e410fa0afeb430f7fbf46` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved replacement candidate still included unsupported live taxonomy synchronization, auto-assignment, verification, candidate diagnostic-evaluation, and superseding claims. |
| `ADM-PP-MATERIALS` | `projects/17116545761229201855/screens/fc652db84e92458dba084e6b1b6244e8` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved targeted-recovery candidate included unsupported learner study-log and object-key/storage UI claims. |
| `ADM-PP-MATERIALS` | `projects/17116545761229201855/screens/f820763c3bb146c8827f7ecd50694cd4` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved replacement candidate included unsupported learner catalog/progress, active-learner access, interactive-summary/interactive-attachment, and storage UI claims. |
| `ADM-PP-PRACTICE` | `projects/17116545761229201855/screens/49d1d94d2ed148ada9552abdbcbdc193` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Practice Collections attempt included routing/URI slug claims, enrolled-package/study-plan concepts, production-release language, and history-log/audit-like concepts. Replacement timed out and no accepted artifact remains from this checkpoint. |
| `ADM-PP-DEFINITIONS` | `projects/17116545761229201855/screens/d5095be8407c48cfb1bae66c70b9ad4d` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First retrievable Package Definitions attempt included learner/enrollment/purchase, audit-record, URL/routing, and entitlement-adjacent claims. Replacement timed out and no accepted artifact remains from this checkpoint. |
| `ADM-PP-DEFINITIONS` | `projects/17116545761229201855/screens/af6a1c60c4e4490d8d9e16ffc32d7ea8` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved targeted-recovery candidate included unsupported fake module-reference and URL/routing-adjacent identifier claims. |
| `ADM-PP-DEFINITIONS` | `projects/17116545761229201855/screens/d930bc8f2b444d7d8078874eb716ab02` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved replacement candidate still included unsupported URL/routing, learner/enrollment/purchase, audit-record, fake module-reference, and learner attempt/access claims. |
| `ADM-PP-OFFERS` | `projects/17116545761229201855/screens/635ae0b32c354fecb7e7fa6e97124008` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Package Offers attempt included catalog discovery/public catalog/storefront/routing, enrollment/learner access, transaction, and candidate-discoverability claims. Replacement timed out and no accepted artifact remains from this checkpoint. |
| `ADM-PP-OFFERS` | `projects/17116545761229201855/screens/9dcc5f1ba66c428fb0b122040cc5952f` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Directly retrieved replacement candidate included unsupported assignment, official diagnostic mock, verification, historical-record, and storefront/catalog-adjacent claims. |

## Shared/System Generated Candidates

Generation checkpoint: 2026-09-21. These artifacts were generated from the Shared/System `CONTRACT_READY` reusable state contracts in the active v2 Stitch workspace and directly validated through generated HTML. They are contract-validated design candidates pending human visual review. They do not mark Angular implementation complete, do not create backend/API behavior, do not change screen-contract statuses, and do not authorize implementation work. `SYS-004` and `SYS-005` were not generated because they remain deferred.

Active workspace used for these candidates:

| Field | Value |
|---|---|
| Stitch project | `projects/17116545761229201855` (`Nursing Platform — System Redesign v2`) |
| Stitch design system | `assets/6536256059106605307` (`Nursing Platform — Core Design System v2`) |
| Canonical design source | `docs/frontend/design/stitch/DESIGN.md` |

| Canonical screen ID | Screen name | Stitch screen | Validation status | Notes |
|---|---|---|---|---|
| `SYS-001` | Route Loading | `projects/17116545761229201855/screens/9dad6fd2167b407dbb0541d627b62657` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found authenticated shell chrome with `Loading page` and `Please wait while the page loads.` No fake progress, raw paths, route IDs, token/session/status claims, service-health/system-online claims, Help, Notifications, footer, compliance/security claims, or debug annotations found. |
| `SYS-002` | Not Found | `projects/17116545761229201855/screens/e526a2a6ae544a3ea4235ae6cb419f19` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found privacy-safe `Not found` state with required body copy and safe `Go to home`/`Back to previous page` actions. No raw paths, route IDs, hidden resource implication, protected-resource details, Help, Notifications, footer, unsupported claims, or debug annotations found. |
| `SYS-003` | Unexpected Error | `projects/17116545761229201855/screens/e1058447e57b4dcdb972e4275f075008` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found retryable `Something went wrong` state with required body copy and `Try again`/`Go to home` actions. No backend exception text, stack traces, request IDs, route IDs, protected-resource details, token/session/status claims, service-health/system-online claims, Help, Notifications, footer, compliance/security claims, or debug annotations found. |
| `SYS-006` | Empty | `projects/17116545761229201855/screens/d211ba5ad02945c6bb60b3eee557a714` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found reusable empty state with `Nothing here yet`, required body copy, and `Refresh` only. No create/add/new CTA, fake records, counts, owner-specific entity names, raw paths, route IDs, hidden data implication, Help, Notifications, footer, unsupported claims, or debug annotations found. |
| `SYS-007` | No Results | `projects/17116545761229201855/screens/9442d040634240c797d2a89eddd9575e` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found filtered-empty `No results found` state with required body copy, generic Search/Status criteria placeholders, and `Clear filters`/`Refresh` actions. No create/add/new CTA, fake records, counts, owner-specific entity names, raw paths, route IDs, hidden data implication, Help, Notifications, footer, unsupported claims, or debug annotations found. |

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
