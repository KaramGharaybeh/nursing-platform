# Current Task

## Current Milestone

Phase 8A — Payment Products & Orders Foundation

Status:
Implementation complete — batch execution final verification passed.

---

## Objective

Complete backend-only payment products and local nurse-owned pending-payment orders for purchasable exam access.

---

## Completion Summary

- [x] Payment product domain entities and lifecycle rules
- [x] Payment order and order item snapshot domain entities
- [x] Authenticated product catalog endpoints
- [x] Nurse-owned order create/list/detail/cancel endpoints
- [x] Admin product create/update/archive/restore endpoints
- [x] EF configuration and AddPaymentProductsOrdersFoundation migration
- [x] Domain, Application, Infrastructure, and WebApi payment tests
- [x] Raw JSON DTO security tests
- [x] Tracking documentation update

---

## Final Verification

- `dotnet build backend/NursingPlatform.slnx`: passed, 0 warnings, 0 errors
- `dotnet test backend/NursingPlatform.slnx`: passed, 723 tests
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`: no pending model changes
- EF design-time note: `HostAbortedException` is known non-blocking EF design-time host-resolution noise when EF still reports no pending model changes.

---

## Current Review State

Phase 8A implementation and final verification are complete.

---

## Out of Scope

Do NOT implement:

- Phase 8B checkout
- Phase 8C payment providers/webhooks
- Phase 9 dashboards/reports
- Frontend/admin UI
- Real checkout, payment providers, subscriptions, refunds, or webhooks
- Exam access grant management
- Question import pipeline, bulk upload, Excel/CSV import, AI generation, or translations
- Admin analytics or platform-wide reports
- Exports, CSV, or Excel endpoints
- AI recommendations
- Weak-area analytics
- Notifications or messaging
- Recruitment, contact-request, candidate, or employer changes

---

## Definition of Done

This milestone is complete when:

- Product catalog endpoints require authorization only
- Product catalog endpoints do not require permissions or NurseRoleGuard
- Nurse order Application handlers enforce Nurse role and current nurse profile ownership
- Admin product reads use existing `Exams.View`
- Admin product writes/archive/restore use existing `Exams.Edit`
- Products and orders store money as ISO currency plus integer minor-unit amount
- Product Type and ExamId are immutable after creation
- Admin product update cannot change Type, ExamId, or IsActive
- Order creation creates one item with `Quantity = 1`
- Order item snapshots are not mutated by later product changes
- New orders start `PendingPayment` and expire at `CreatedAt + 30 minutes`
- Lazy expiry applies for owned pending orders in list/detail/cancel
- No checkout/provider/webhook behavior is implemented
- No automatic `ExamAccessGrant` is issued
- Solution builds with zero warnings and zero errors
- All 723 tests pass
- EF pending model check reports no pending model changes
- Implementation is committed only after final verification

---

## References

Before implementing anything, read:

- PROJECT_RULES.md
- AGENTS.md
- docs/standards/engineering-standards.md
- docs/database/database-design.md
- docs/api/api-design.md
- docs/superpowers/specs/2026-07-12-payment-products-orders-foundation.md
- docs/superpowers/plans/2026-07-12-payment-products-orders-foundation.md
