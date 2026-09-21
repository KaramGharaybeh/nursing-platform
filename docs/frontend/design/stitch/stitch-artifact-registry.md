# Stitch Artifact Registry

```yaml
document_id: NPS-DES-STITCH-ARTIFACT-REGISTRY
status: PHASE_2_SCREEN_GENERATION_ACTIVE
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
| `ONB-001` | Profile Onboarding | `projects/17116545761229201855/screens/a4bdbce7fc104ceab34581f21e86a1b3` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found authenticated shell, required `Complete your profile` heading/copy, `First name` and `Last name` required fields with max-length helper text, `Save`, and shell `Sign out`. No nurse/employer profile fields, tokens, raw IDs, `isProfileComplete`, footer, or unsupported claims found. |
| `ACC-001` | Account Overview | `projects/17116545761229201855/screens/004ff3b651fb42aca5244b3fbd1d25e1` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found identity-only account overview with `Email`, `Username`, `First name`, `Last name`, `Email verification`, `Edit personal details`, and approved shell chrome. No password/security/session/settings/credentials wording, permissions, raw IDs, timestamps, `isProfileComplete`, footer, or unsupported claims found. |
| `ACC-002` | Personal Details Edit State | `projects/17116545761229201855/screens/f397d94810034593baa0793e87f36f15` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found same-route edit-state design with `Edit personal details`, `First name`, `Last name`, max-length helper text, `Save`, and `Cancel`. No email/username editing, password/security/session/device/notification/deletion controls, raw IDs, tokens, footer, or unsupported claims found. |

Rejected/superseded generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `ACC-001` | `projects/17116545761229201855/screens/09a6f96af6b9474cb95bfa236bf7a977` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Account Overview attempt used out-of-scope `credentials` wording in direct HTML. Superseded by accepted identity-only replacement `004ff3b651fb42aca5244b3fbd1d25e1`. |

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
| `NUR-001` | Profile Overview | `projects/17116545761229201855/screens/d2d3ea96cca0405a9622b73acaeb6e35` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Previously accepted direct HTML validation found approved profile overview composition with professional identity, supported base-profile facts, section summaries, no completion score/status, no fake identity, and no unsupported profile/share/recruiter claims. Preserved without regeneration in this checkpoint. |
| `NUR-002` | Profile Summary State | `projects/17116545761229201855/screens/6d7f13d7750c47fd825e1836fdb3f110` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Previously accepted direct HTML validation found profile summary/first-time state aligned to the no-profile/sparse-profile contract, without completion percentage or unsupported profile status. Preserved without regeneration in this checkpoint. |
| `NUR-003` | Personal Information | Not accepted | STITCH_GENERATION_BLOCKED_TEMPORARY | Recovery pass exhausted the authorized two fresh attempts. Delayed recovered artifact and first fresh replacement both had material validation defects; the second fresh attempt timed out and did not recover through delayed direct checks. Canonical screen contract remains `CONTRACT_READY`; this is a temporary Stitch artifact-generation status only. |
| `NUR-004` | Experience List | `projects/17116545761229201855/screens/06c2092a51bd4fd08b65aa74903dbc13` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Previously accepted direct HTML validation found approved experience list content/actions and no visible design annotations after replacement. Preserved without regeneration in this checkpoint. |
| `NUR-005` | Experience Form State | `projects/17116545761229201855/screens/da2c04d28c8548be896dc897a5b0ae89` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Previously accepted direct HTML validation found approved experience form/delete-confirmation state with no unsupported uniqueness, employment-type, or profile-completion claims. Preserved without regeneration in this checkpoint. |
| `NUR-006` | Education List | `projects/17116545761229201855/screens/6a0f2d2f5bae4a1e8ddfd370b46f765f` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found Education list with three education records, approved date-line formats, Add/Edit/Delete actions, inline delete confirmation, and no state-example gallery, certificate content, completion score, raw IDs, or unsupported status claims. |
| `NUR-007` | Education Form State | `projects/17116545761229201855/screens/e64e103caf4942b68dc83a1043dcbf25` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found Education form state with institution, degree, field of study, country, optional dates, details/description, validation summary, end-date error, Save education, and Cancel. No state gallery, modal/delete confirmation, route IDs, fake identity, or unrelated profile sections found. |
| `NUR-008` | Certificates List | `projects/17116545761229201855/screens/f2ca29048bd14819814271042949e30b` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found certificate list cards with plain issuer text, issue/optional expiration dates, credential ID, `Open credential link`, Edit/Delete, and inline delete confirmation. No verified/official/trusted status, Active/Expired/Valid/Expiring-soon labels, state examples, or unrelated sections found. |
| `NUR-009` | Certificate Form State | `projects/17116545761229201855/screens/942bcff423aa4bdeac43b0ad6de1868b` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found certificate form fields, URL validation summary/error, Save certificate, and Cancel. No verification/status wording, modal/drawer, route IDs, fake identity, footer, or unsupported claims found. |
| `NUR-010` | Skills | `projects/17116545761229201855/screens/51eab55569ab49909c69279ec9fa706b` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found feature-local chip editor with skill input, duplicate validation, required rules helper, removable chips, Save skills, and Cancel. No taxonomy/autocomplete, drag/reorder, proficiency, route IDs, footer, or unrelated profile sections found. |
| `NUR-011` | Languages | `projects/17116545761229201855/screens/f77e083faedd42cea3a78b071fb489ec` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found row-based language/proficiency editor, exact proficiency options, duplicate-language validation, Add language, Save languages, and Cancel. No unsupported proficiency values, drag/reorder, unrelated sections, raw IDs, or footer claims found. |
| `NUR-CONTACT` | Contact Requests | `projects/17116545761229201855/screens/31473847b8eb4c3185a0f58777e28c80` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found status filter, safe employer/request facts, plain-text statuses, pending Approve/Reject actions, inline Reject confirmation, terminal rows without actions, and pagination. No raw request IDs, employer private internals, messages/reasons/reply fields, permission keys, role claims, urgency/priority, or semantic status badges found. |

Rejected/superseded Nurse Profile generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `NUR-001` | `projects/17116545761229201855/screens/738fd82f37634b869eda8e7cf3889521` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Debug/reference content and unsupported validity wording. Superseded by accepted `d2d3ea96cca0405a9622b73acaeb6e35`. |
| `NUR-001` | `projects/17116545761229201855/screens/9202650b067e4a8599f91bc8adce1a3a` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Document-title/forbidden icon-ligature issue from prior validation. Superseded by accepted `d2d3ea96cca0405a9622b73acaeb6e35`. |
| `NUR-003` | `projects/17116545761229201855/screens/1cfe7b9fa79842bf9d47baf3e3baaad2` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Delayed recovered timeout artifact used unsupported `Max 120 characters` headline constraint and did not match authoritative optional/required semantics. |
| `NUR-003` | `projects/17116545761229201855/screens/64124d9dca29481bbed501d53e6aaca4` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Fresh replacement fixed the headline length but incorrectly marked optional profile fields/country controls as required. Second fresh retry timed out and did not recover, so `NUR-003` remains `STITCH_GENERATION_BLOCKED_TEMPORARY` for this pass. |
| `NUR-004` | `projects/17116545761229201855/screens/73822cb69a0d442bb9855e804dbe0bdb` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Visible design annotation labels. Superseded by accepted `06c2092a51bd4fd08b65aa74903dbc13`. |
| `NUR-006` | `projects/17116545761229201855/screens/98b508ca88c948d5bb33772f2a94a411` | REJECTED_SUPERSEDED_NOT_AUTHORITY | Delayed recovered timeout artifact rendered visible `Education State Examples` and separate state panels on the product screen. Superseded by accepted `6a0f2d2f5bae4a1e8ddfd370b46f765f`. |
| `NUR-008` | `projects/17116545761229201855/screens/fab0bd4e943d4acfaa00ea2980271db9` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Certificates List attempt used `verified_user` iconography beside issuing organizations, implying verification/trust despite the no-verified/official-badge rule. Superseded by accepted `f2ca29048bd14819814271042949e30b`. |

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
| `EXM-001` | Exam Catalog | `projects/17116545761229201855/screens/232c5af699c44991acee25ede383ce3a` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found authenticated Exams catalog with filters, exam cards, analytics/history entries, pagination/no-results behavior, and no raw IDs, answers, correctness, purchase CTA, footer, or unsupported claims. |
| `EXM-002` | Exam Detail | `projects/17116545761229201855/screens/10e1ba4fa8b04b6995e0e8ce97556eb4` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found exam detail facts and instructions-preview/fallback treatment without invented unsupported instructions/rules, questions, answers, raw IDs, footer, or unsupported claims. |
| `EXM-004` | Exam Instructions | `projects/17116545761229201855/screens/2e8a32ace14c432e8289bb2314126158` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found backend-supplied instructions with duration/question/passing facts, start/resume confirmation treatment, and no simultaneous empty fallback, invented instructions, raw IDs, footer, or unsupported claims. |
| `EXM-005` | Exam Session | `projects/17116545761229201855/screens/cdc9aad819524ac09d0060855349aa09` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found one-question-at-a-time exam session with timer, near-expiry warning, save error/retry, option selection, Save answer, Previous/Next/Submit controls, and no correctness, answer key, explanations, result metrics, raw IDs, or all-question navigator. |
| `EXM-006` | Submit Confirmation | `projects/17116545761229201855/screens/f8e10c91253f4540a0716f199e2f8271` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found minimal submit confirmation with exam title, `Question 120 of 120`, total/answered/unanswered counts, cancellation action, disabled/loading `Submitting...` action, and no question text, answer options, correctness, result metrics, preview/state annotations, raw IDs, or unsupported claims. |
| `EXM-007` | Exam Result | `projects/17116545761229201855/screens/4b15edf6f4cb43fbbc27fa73b3a99632` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found aggregate finalized result with heading/context, terminal status, score/max/percentage/passed/correct answers/questions, Back to exams, and Review answers. No provenance, timestamps, review content, analytics, system/sync language, raw IDs, or unsupported claims found. |
| `EXM-008` | Exam Analytics | `projects/17116545761229201855/screens/b070909c744b4d1483042d2acb6d40e2` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found approved heading/copy, filters, Overview, Performance by exam, Performance by category, and Performance over time sections rendered as textual metrics/tables/lists. No charts, bands, recommendations, provenance, raw IDs, or local-derivation claims found. |
| `EXM-009` | Answer Review | `projects/17116545761229201855/screens/da514a9409434b03af140fbefe1ac91d` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Direct HTML validation found one-question finalized answer review with pager, factual status, read-only option rows, `Your answer`, `Correct answer`, explanation, points, Previous/Next, and Back to result. No active inputs, aggregate result metrics, analytics, raw IDs, package guidance, or unsupported claims found. |
| `EXM-010` | Exam History | `projects/17116545761229201855/screens/3e5f4653630446678736491c05b12117` | GENERATED_CONTRACT_VALIDATED_PENDING_HUMAN_REVIEW | Replacement direct HTML validation found status filter, attempt history rows with approved status/date/percentage/result/action fields, and pagination. No optional state/reference section, analytics, score/max/correct counts, provenance, raw IDs, sync/system language, footer, or unsupported claims found. |

Rejected/superseded Exams generation attempts from this checkpoint:

| Canonical screen ID | Stitch screen | Disposition | Reason |
|---|---|---|---|
| `EXM-002` | `projects/17116545761229201855/screens/6b23383e4aec4a5aa6c8e2664619cc96` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Detail attempt invented unsupported exam instructions/rules. Superseded by accepted `10e1ba4fa8b04b6995e0e8ce97556eb4`. |
| `EXM-004` | `projects/17116545761229201855/screens/c51af566d4b044299821e81765649f9a` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Instructions attempt showed supplied instructions and empty fallback simultaneously. Superseded by accepted `2e8a32ace14c432e8289bb2314126158`. |
| `EXM-006` | `projects/17116545761229201855/screens/8f8d0a60978c4597a552e5aed0d94a7e` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Submit Confirmation attempt rendered a visible duplicate-submit preview annotation and active-question background content. Superseded by accepted `f8e10c91253f4540a0716f199e2f8271`. |
| `EXM-007` | `projects/17116545761229201855/screens/78d041cb77d143debaf3a1327e9e35d3` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam Result attempt included unsupported blueprint/finalized-attempt/system status/synchronization wording. Superseded by accepted `4b15edf6f4cb43fbbc27fa73b3a99632`. |
| `EXM-010` | `projects/17116545761229201855/screens/a399ac9c21dd459fb32ec315a756fa00` | REJECTED_SUPERSEDED_NOT_AUTHORITY | First Exam History attempt rendered a visible `System status views` section and sync-related content. Superseded by accepted `3e5f4653630446678736491c05b12117`. |

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
