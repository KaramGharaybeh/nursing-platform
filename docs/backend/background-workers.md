# Background Workers

## Current backend processing model

No `IHostedService`, `BackgroundService`, hosted-worker registration, scheduler, queue processor, or periodic timer was found in the inspected `backend/src` implementation. This is a bounded current-implementation observation, not a decision that background workers are prohibited in the future.

`DatabaseInitializer` runs as an awaited startup operation, not a hosted background service. The inspected exam-session expiry and payment-order expiry paths evaluate time during requests; the approved Preparation Package v1 report path generates on a qualifying direct request. None of those paths establishes a periodic cleanup worker.

A future worker design requires its own authority and must state its responsibility, trigger, persistence/transaction boundary, failure behavior, and ownership before being documented as approved here. Current delivery gaps belong to later `docs/delivery/current-state.md`, not this implementation contract.
