# Frontend Accessibility

[Product requirements](../product/requirements.md) own required accessibility outcomes. This document owns frontend implementation obligations supported by the approved frontend foundation, [Frontend Architecture](frontend-architecture.md), and the active visual design source. It does not establish an additional WCAG target.

The documented baseline is WCAG 2.2 AA. Frontend components and screens use semantic structure, keyboard reachability, visible focus, accessible names and labels, field-associated errors, sufficient contrast, non-color-only states, and reduced-motion treatment where animation exists. Screen contracts require a skip link and one main landmark in the authenticated shell. The project rules require external component templates and styles so accessibility can be inspected alongside behavior and rendering tests.

The frontend architecture and project rules require at least 44 × 44px actual interactive targets and at least 48 × 48px on mobile/touch viewports. The active `DESIGN.md` also prefers 48 × 48px on mobile/touch; that preference does not prohibit the stricter minimum. A smaller visible icon may sit inside a compliant actual interactive target.

Current screen/component tests and visual review are evidence of particular implementations; neither a passing test nor a design image replaces an approved accessibility requirement. [Security Verification](../security/security-verification.md) addresses protected behavior, while the [Testing Strategy](../testing/testing-strategy.md) owns broad verification methodology.
