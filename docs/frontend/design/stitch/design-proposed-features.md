# Design-Proposed Features

These features originated from design exploration. Their presence in a Stitch design does not mean they are implemented. They must not silently become production functionality.

Before the first production release, every open item must be reviewed. Each item must end in one of:

- `IMPLEMENT_BEFORE_RELEASE`
- `HIDE_BEFORE_RELEASE`
- `DEFER_POST_RELEASE`
- `REJECT`

Implementation requires normal product, documentation, backend/API, frontend, permission/security, accessibility, and testing authorization and planning. This file is a design-to-product backlog, not an implementation ledger, and it does not create frontend task numbers.

## Notifications

- ID: `DPF-001`
- Status: `DESIGN_PROPOSED`
- Origin: `Shell / APP-SHELL / Nurse / Desktop`; `projects/17116545761229201855/screens/fbcef626cae7450fa6f5cedbdbeae8ea`
- Actor(s): Nurse initially; future applicability to Employer/Admin/Public requires separate review.
- Design intent: The preferred shell visual direction includes a compact notification affordance in the authenticated top bar. The intent is to support timely awareness without displacing the primary Nurse workflows.
- Proposed behavior:
  1. Provide an authenticated notification entry point.
  2. Show a notification list or inbox.
  3. Support read/unread state if later authorized.
  4. Link notifications only to authorized product destinations.
  5. Define backend event/source model before implementation.
  6. Define permissions/privacy and retention requirements.
  7. Decide delivery channels separately; do not assume email, push, or SMS.
- Current authority:
  - Documentation: No approved notification-center product contract for this shell. Account notification preferences are classified as a backend gap in current frontend planning evidence.
  - Frontend: No implemented notification center or notification entry point is authorized by the current Angular shell/navigation implementation.
  - Backend: No stable current-user notification inbox/event-source contract is recorded for this feature.
  - API: No approved notification list/read/unread API contract is recorded for this feature.
  - Permissions/security: No notification visibility, privacy, retention, or destination-linking policy is approved.
  - Notifications/events: Event sources, delivery semantics, and retention are undefined.
- Missing implementation:
  - Product decision for notification scope and actors.
  - Notification source/event model.
  - Backend persistence/query/update contracts.
  - API schemas and error behavior.
  - Frontend notification entry, inbox/list, unread/read behavior, routing, and states.
  - Permission/privacy/retention policy.
  - Accessibility, testing, and release acceptance criteria.
- Production-release decision: `PENDING`
- Release rule: If still unimplemented at production-readiness review, decide explicitly whether to hide, defer, reject, or implement it.
- Dependencies / notes: Must remain secondary to core Nurse shell navigation and workflows. Do not infer email, push, SMS, browser push, real-time sockets, or cross-actor notifications from this design affordance.

## Help / Support Access

- ID: `DPF-002`
- Status: `DESIGN_PROPOSED`
- Origin: `Shell / APP-SHELL / Nurse / Desktop`; `projects/17116545761229201855/screens/fbcef626cae7450fa6f5cedbdbeae8ea`
- Actor(s): Nurse initially; future applicability to Employer/Admin/Public requires separate review.
- Design intent: The preferred shell visual direction includes a small Help affordance in the authenticated top bar. The intent is to offer user assistance without turning the product into a support portal or distracting from primary workflows.
- Proposed behavior:
  1. Provide an authenticated help entry point.
  2. Surface approved help or support resources.
  3. Route users to contextual support content where product authority exists.
  4. Do not invent live chat, ticketing, phone support, or knowledge-base systems unless separately approved.
  5. Define ownership and content source before implementation.
- Current authority:
  - Documentation: No approved help/support product contract is recorded for this shell.
  - Frontend: No implemented help center, support menu, or contextual support route is authorized by the current Angular shell/navigation implementation.
  - Backend: No stable support-content, ticket, chat, or help-resource backend contract is recorded.
  - API: No approved help/support API contract is recorded.
  - Permissions/security: No support access policy, privacy boundary, escalation model, or content governance is approved.
  - Notifications/events: Not applicable unless later tied to support workflows by separate authorization.
- Missing implementation:
  - Product decision for support/help scope and actors.
  - Content ownership and source of truth.
  - Backend/API contracts if dynamic content, tickets, or support interactions are later approved.
  - Frontend help entry point, content surface, routing, and states.
  - Permission/privacy/accessibility/testing/release acceptance criteria.
- Production-release decision: `PENDING`
- Release rule: If still unimplemented at production-readiness review, decide explicitly whether to hide, defer, reject, or implement it.
- Dependencies / notes: Must not imply live chat, ticketing, phone support, emergency support, or knowledge-base availability without explicit authority. Must remain secondary to core product workflows.
