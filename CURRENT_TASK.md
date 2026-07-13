# Current Task

## Current Milestone

Backend Local MVP — Payment Fulfillment and Purchased Exam Access Complete

Status:
Backend local MVP is complete enough for frontend development.

---

## Objective

Record the completed local backend MVP, preserve the verified payment-to-exam-access boundary, and await explicit selection of the next implementation phase.

The full Development/Test Sandbox journey exists:

```text
product -> order -> checkout -> Sandbox completion -> Paid -> ExamAccessGrant -> authorized exam start
```

Production payment-provider selection is intentionally deferred because company country, bank-account jurisdiction, and provider selection are not finalized.

---

## Completed Backend Capabilities

- [x] Payment products and immutable order snapshots.
- [x] Nurse-owned order create/list/detail/cancel behavior.
- [x] Checkout session foundation and lifecycle.
- [x] Provider-neutral checkout abstraction.
- [x] Sandbox provider available only in Development/Test.
- [x] Sandbox checkout initialization.
- [x] Development/Test-only Sandbox completion endpoint.
- [x] Atomic `PendingPayment` -> `Paid` transition.
- [x] Server-persisted `PaidAt`.
- [x] Transactional and idempotent `ExamAccessGrant` fulfillment.
- [x] PostgreSQL concurrency and rollback coverage.
- [x] Purchased exam access enforcement.
- [x] Effective paid rule: `Exam.IsFree == false` OR active positive-price `ExamAccess` product exists.
- [x] Exam catalog/detail `IsFree` and `CanStart` consistency.
- [x] Complete local Sandbox purchase-to-exam-start journey.

---

## Latest Verified Results

- Domain: 69 passed.
- Application: 434 passed.
- Infrastructure: 119 passed.
- WebApi: 252 passed.
- Total: 874 passed.
- Build: 0 warnings, 0 errors.
- EF: no pending model changes.
- PostgreSQL Sandbox tests: 6 passed, 0 skipped.

---

## Current Review State

Backend local MVP payment fulfillment and purchased exam access are complete for local frontend development.

Next status: awaiting explicit selection of the next phase.

---

## Deferred Work

The following work is deferred and is not a current blocker for local frontend development:

- Production payment provider.
- Production webhook/signature verification.
- Refunds and reconciliation.
- Production-grade object storage.
- Operational/production hardening.
- Frontend implementation.

Do not mark these deferred items complete until they are explicitly implemented and verified.

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- TASKS.md
- README.md
- docs/product/vision.md
- docs/architecture/system-architecture.md
- docs/backend/backend-architecture.md
- docs/frontend/frontend-architecture.md
- docs/database/database-design.md
- docs/api/api-design.md
- docs/standards/engineering-standards.md
