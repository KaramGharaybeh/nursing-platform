# Phase 8B Checkout + Payment Provider Abstraction Specification

## Objective

Define Phase 8B as a backend-only checkout-session layer and provider-neutral payment-provider abstraction for nurse-owned Phase 8A payment orders.

Phase 8B lets an authenticated nurse start checkout for an existing owned `PendingPayment` order and receive a provider checkout redirect response through an Application-defined abstraction. It persists provider-neutral checkout session metadata and provider identifiers needed to correlate future provider callbacks, while keeping payment confirmation, webhook processing, order `Paid`/`Failed` transitions, provider-side cancellation, and `ExamAccessGrant` issuance deferred to later phases.

This specification and plan only create documentation. They do not implement source code, tests, migrations, configuration changes, endpoint changes, provider integrations, or tracking-doc changes.

## Baseline

Expected HEAD: `43969bf feat: add payment products orders foundation`.

Phase 8A already provides:

- `PaymentProduct`, `PaymentOrder`, and `PaymentOrderItem` domain entities.
- Nurse-owned local payment orders created as `PendingPayment`.
- Immutable order item snapshots for product name, product type, exam id, currency, unit amount, quantity, and line total.
- Order expiry at `CreatedAt + 30 minutes` with lazy expiry in owned order list/detail/cancel workflows.
- Authenticated product catalog endpoints.
- Nurse-owned create/list/detail/cancel order endpoints.
- Admin product management endpoints.
- No checkout sessions, provider ids, provider state, card data, webhooks, paid transitions, failed transitions, provider-side cancellation, or automatic access grants.

## In Scope

- Backend-only checkout session model for existing nurse-owned `PendingPayment` orders.
- Provider-neutral checkout architecture with Application interfaces and Infrastructure implementations.
- A nurse endpoint to create or reuse checkout for an existing owned order, only after the API contract and provider-selection blocker are resolved.
- Persisted provider identifiers required for future webhook correlation.
- Idempotent checkout-start behavior for client retries and provider retry safety.
- Checkout session ownership tied to the existing `PaymentOrder.NurseProfileId`.
- Checkout session lifecycle states independent from payment confirmation.
- Local order cancellation guard and checkout-session expiration behavior.
- Secret and configuration boundaries for future real payment providers.
- Security, DTO exposure, logging, and failure behavior.
- EF configuration and EF migration planning, with migration generation and model snapshot verification reserved for premium review.

## Out of Scope

- Frontend.
- Card entry or card data storage.
- Real provider choice such as Stripe, PayPal, Adyen, or any named vendor until a reviewer makes the provider-selection decision.
- Provider selection UX or runtime provider switching rules.
- Webhook endpoints.
- Webhook signature validation.
- Payment confirmation.
- Order `Paid` or `Failed` transitions from provider results.
- Provider-side cancellation.
- `ExamAccessGrant` issuance.
- Refunds, subscriptions, invoices, taxes, coupons, discounts, wallets, payouts, exports, dashboards, or Phase 9 administration reports.
- Changes to product creation, product pricing, or order item snapshot semantics.
- Modification of `CURRENT_TASK.md` or `TASKS.md` during planning.

## Chosen Approach

Use a provider-neutral checkout-session aggregate plus Application-owned provider interfaces.

The Application layer owns checkout business rules, idempotency semantics, DTOs, provider contracts, and concurrency orchestration. Infrastructure implements persistence, constraints, transactions, EF configuration, and provider adapters only after provider selection. WebApi remains thin and maps only the nurse checkout-start endpoint after the API contract is fixed. Domain can hold provider-neutral checkout-session entities and enums because local checkout lifecycle is business state, while external API calls, retries, timeouts, provider SDKs, and secrets remain outside Domain.

Rejected alternatives:

- Provider-specific fields on `PaymentOrder`: rejected because it couples orders to a vendor and weakens immutable order snapshots.
- Direct provider API calls from WebApi handlers: rejected because it violates Clean Architecture and makes tests brittle.
- Webhooks in Phase 8B: rejected because payment confirmation and grant issuance belong to Phase 8C.
- Optional provider-side cancellation in Phase 8B: rejected because Phase 8B blocks local order cancellation while an active checkout exists, and provider-side cancellation/reconciliation belongs with provider callbacks/confirmation in Phase 8C.

## Blocking Reviewer Decision

Real provider selection is a blocking reviewer decision before implementing either:

- Any real Infrastructure provider adapter.
- The public checkout endpoint that can return a provider redirect URL.

Until this decision is made, implementation may proceed only through Foundation work: domain model, Application contracts, and persistence design. Endpoint tests may be written in isolation only after the API request/response contract is fixed.

## Actors And Authorization

### Nurse

An authenticated user with the `Nurse` role and a current `NurseProfile` may start checkout for an order they own.

Endpoint authorization:

- `POST /api/v1/me/nurse-profile/payment/orders/{orderId:guid}/checkout`
- Uses `.RequireAuthorization()`.
- Application handler enforces Nurse role through existing `NurseRoleGuard`.
- Application handler enforces current nurse profile ownership through `PaymentOrder.NurseProfileId`.

Nurse may not:

- Start checkout for another nurse's order.
- Start checkout for an expired, cancelled, paid, or failed order.
- Send card data, card tokens, raw provider payloads, provider secrets, amount, currency, or line items.
- Override provider choice.

### Anonymous User

Anonymous users receive `401 Unauthorized` for the checkout endpoint.

### Admin / Content Manager

Phase 8B does not add admin checkout operations.

## Checkout Session Lifecycle

### PaymentCheckoutSession

Expected fields:

- `Id`
- `PaymentOrderId`
- `NurseProfileId`
- `Status`
- `ProviderName`
- `ProviderCheckoutSessionId`
- `ProviderPaymentIntentId`
- `ProviderClientReference`
- `CheckoutUrl`
- `ProviderCallLeaseId`
- `ProviderCallLeaseExpiresAt`
- `Currency`
- `AmountMinor`
- `IdempotencyKeyHash`
- `RequestFingerprintHash`
- `ExpiresAt`
- `CreatedAt`
- `UpdatedAt`
- audit fields

Rules:

- `PaymentOrderId` links to one Phase 8A order with restrict delete.
- `NurseProfileId` duplicates ownership for efficient filtering and defense-in-depth; it must match the linked order's `NurseProfileId`.
- `Currency` and `AmountMinor` are copied from the immutable `PaymentOrder` at checkout-session creation.
- Session amount and currency must never be taken from the client.
- `ProviderName` is obtained from `IPaymentCheckoutProvider.ProviderName`, not from provider result data or client input, and is set when the `Created` session is first persisted before any provider call.
- `ProviderCheckoutSessionId` stores the provider's checkout/session id when returned by the provider abstraction; it is nullable in `Created` and required by domain transition rules in `ProviderPending`.
- `ProviderPaymentIntentId` stores a provider payment intent/payment id only if the abstraction returns one.
- `ProviderClientReference` is generated by the platform, is stable for the local checkout session, and is sent to the provider abstraction as the provider idempotency/correlation reference.
- `CheckoutUrl` is the redirect URL returned by the provider abstraction and must be validated as HTTPS; it is nullable in `Created` and required by domain transition rules in `ProviderPending`.
- `ProviderPaymentIntentId` remains optional in all Phase 8B states.
- `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` are nullable database-backed lease fields used to serialize provider calls for an active `Created` session.
- Checkout URLs may contain bearer-like tokens or secrets and must never be logged, even when validation fails.
- Checkout sessions do not store card number, CVC, expiry date, billing card details, payment method details, customer secrets, client secrets, raw provider payloads, or webhook payloads.

### PaymentCheckoutSessionStatus

Expected values:

- `Created`
- `ProviderPending`
- `CreationRejected`
- `Expired`

Rules:

- `Created` is persisted local pre-provider state used for concurrency and crash recovery while provider creation is in progress or unknown after a process crash.
- `ProviderPending` means the provider checkout session was created and the nurse can be redirected.
- `CreationRejected` means the provider adapter returned a definitive rejection and guarantees that no provider checkout session was created. It is terminal, it is not a payment `Failed` state, and it must not be used for timeout, network failure, process crash, or any unknown provider outcome.
- `Expired` means the local checkout session is no longer usable because either the session expiry or linked order expiry has passed.
- Phase 8B must not introduce `Paid`, `Failed`, `Succeeded`, `Completed`, or grant-related checkout states. Provider-confirmed outcomes belong to Phase 8C.

### Lifecycle Transitions

- None -> `Created`: start checkout creates a local checkout session after validating order ownership and order status.
- `Created` -> `ProviderPending`: provider abstraction returns a checkout session successfully and the persisted session is updated with provider ids and checkout URL.
- `Created` -> `CreationRejected`: provider abstraction returns a definitive rejection where the adapter guarantees no provider checkout session was created.
- `Created` -> `Expired`: recovery determines the linked order/session expired before a provider response was persisted.
- `ProviderPending` -> `ProviderPending`: idempotent retry returns the existing unexpired session.
- `ProviderPending` -> `Expired`: lazy expiry when the checkout session `ExpiresAt <= now` or linked order is expired.

No Phase 8B transition may mark an order as `Paid` or `Failed`.
Domain transitions must enforce state-specific invariants: `ProviderCheckoutSessionId` and `CheckoutUrl` are absent or nullable in `Created`, required in `ProviderPending`, and not globally required by persistence configuration.

## Exact Concurrency And Crash-Recovery Algorithm

The implementation must use database-backed concurrency plus stable provider idempotency. Application code may use a transaction and optimistic concurrency tokens or equivalent PostgreSQL row locking through Infrastructure abstractions, but correctness must not rely on in-memory locks.

Checkout start algorithm:

1. Validate the JWT, Nurse role, current nurse profile, route order id, and optional idempotency key.
2. Compute `RequestFingerprintHash` as a versioned canonical fingerprint from the API operation name `StartPaymentCheckout`, authenticated `NurseProfileId`, route `PaymentOrderId`, and business request fields that affect the operation. `IdempotencyKey` itself is excluded from the fingerprint. In Phase 8B no body field affects the operation, so the fingerprint must not include amount, currency, provider, URLs, idempotency key, or mutable order display data.
3. If `IdempotencyKey` is provided, hash it and use the idempotency scope `(NurseProfileId, IdempotencyKeyHash)`.
4. Start a database transaction.
5. Load the owned order by both `PaymentOrder.Id` and `NurseProfileId`; missing or non-owned orders return `404`.
6. Apply Phase 8A lazy expiry. If the order is no longer `PendingPayment`, persist expiry if needed and return `409`.
7. If the idempotency key already exists for the same nurse with a different `PaymentOrderId`, return `409 Conflict`.
8. If the idempotency key already exists for the same nurse and same order with a different `RequestFingerprintHash`, return `409 Conflict`.
9. Before looking for or inserting a new active session, mark every owned session for the order in `Created` or `ProviderPending` with `ExpiresAt <= now` as `Expired`, persist those transitions inside the transaction, then apply the unique active-session constraint.
10. If the idempotency key already exists for the same nurse and original session is `Expired` or `CreationRejected`, return `409 IdempotencyKeyAlreadyUsed`; the key remains bound to its original operation and the client must supply a new idempotency key for a new operation.
11. Find an active checkout session for the order in `Created` or `ProviderPending` status with `ExpiresAt > now`. A unique active-session constraint must make more than one impossible.
12. If an active `ProviderPending` session exists, commit and return it with `200 OK` without calling the provider.
13. If an active `Created` session exists, attempt to atomically acquire the provider-call lease by setting a new `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` only when no unexpired lease exists.
14. If no active session exists, create and save one `Created` session with a generated stable `ProviderClientReference`, `ProviderName` from `IPaymentCheckoutProvider.ProviderName`, copied order amount/currency, capped expiry, optional `IdempotencyKeyHash`, `RequestFingerprintHash`, and an acquired provider-call lease; commit. The unique active-session and idempotency constraints protect concurrent inserts.
15. A caller that finds an active `Created` session with an unexpired provider-call lease owned by another caller must commit without calling the provider and return `409 CheckoutInitializationInProgress` with `Retry-After` based on the lease expiry.
16. Only the caller that atomically acquires the lease may call the provider. Only one provider call may be in flight for a local session at a time. After lease expiration, recovery may acquire a new lease and retry sequentially with the same `ProviderClientReference`; sequential recovery retries are allowed, concurrent provider calls are not.
17. Call the provider outside the database transaction using `ProviderClientReference` as the stable provider idempotency/reference value.
18. On successful provider response, start a new transaction, reload the same session, verify it is still `Created`, active, and owned by the lease holder, validate provider identifiers and HTTPS checkout URL, persist provider ids, set status `ProviderPending`, clear the lease, and commit.
19. If the provider returns a definitive creation rejection where the adapter guarantees no provider checkout session was created, reload the same session, verify lease ownership, set `CreationRejected`, clear the lease, and return a safe conflict/provider-rejection response without marking the payment order `Failed`.
20. If the provider times out, a network failure occurs, the process crashes, or the provider outcome is unknown, do not use `CreationRejected`; leave the session `Created` and recoverable until expiry, with the lease expiring naturally or being cleared only when safe.
21. If another concurrent request already advanced the session to `ProviderPending`, return the persisted session with `200 OK` without overwriting it.
22. If the order or session expired while the provider call was in flight, do not reactivate the session; return `409 Conflict` for the local state.

Provider timeout and retry behavior:

- Use a configured provider-call timeout. The initial implementation plan should require reviewer approval for the exact value before adapter implementation and must never allow an infinite wait.
- Provider calls may be retried only when the failure is transient or timeout-related and only with the same `ProviderClientReference`.
- Retries must never create a second local active checkout session for the same order.
- Retries must never use a new provider idempotency reference for the same local session.
- Provider-call retries must acquire the database-backed lease first; a concurrent request that observes an unexpired lease must return `409 CheckoutInitializationInProgress` with `Retry-After` and must not call the provider.
- If all retries fail or timeout, leave the local session in `Created` for recovery unless the local order/session expired.

Crash recovery for persisted `Created` sessions:

- A persisted active `Created` session means the process may have crashed before, during, or after the provider call.
- A retry for the same order or same idempotency key must reuse the existing `Created` session and its existing `ProviderClientReference`.
- The handler must atomically acquire an expired or absent provider-call lease before calling the provider using that same `ProviderClientReference`; provider idempotency should return the same provider session if the previous provider call succeeded.
- If the provider reports an existing session for that reference, persist it and return `200 OK`.
- If the provider creates it for the first time, persist it and return `200 OK`.
- If the local order/session expired before recovery, mark the local session `Expired` and return `409 Conflict` without creating a new provider session.

## Checkout Session Ownership

- Checkout sessions are owned by the same nurse profile that owns the linked order.
- Starting checkout for a non-owned order returns `404 Not Found` to avoid existence disclosure.
- The Application handler must query by both `PaymentOrder.Id` and current `NurseProfile.Id`.
- Returned DTOs must not expose `NurseProfileId` or `UserId`.

## Idempotency Behavior

Phase 8B uses nurse-scoped operation idempotency keys plus request fingerprints.

Request contract:

- `StartPaymentCheckoutRequest`
- Contains only optional `IdempotencyKey`.
- Does not contain product id, amount, currency, quantity, provider name, card data, callback URLs, or metadata.

Rules:

- `IdempotencyKey` is optional to avoid forcing clients into a new contract before frontend exists.
- If provided, it must be bounded to a safe length and stored hashed; raw client-provided idempotency key values must never be logged.
- The idempotency scope is `(NurseProfileId, IdempotencyKeyHash)` plus `RequestFingerprintHash`.
- `RequestFingerprintHash` is a versioned canonical fingerprint of operation name, `NurseProfileId`, `PaymentOrderId`, and business request fields; it excludes the `IdempotencyKey` itself.
- Reusing the same key for the same nurse, same operation, same order, and same fingerprint returns the existing active checkout session response with `200 OK`.
- Reusing the same key with another order returns `409 Conflict`.
- Reusing the same key for the same order with a different request fingerprint returns `409 Conflict`.
- The same idempotency key remains bound to its original operation even after the original session becomes terminal.
- If the original session for the same key is `Expired` or `CreationRejected`, return `409 IdempotencyKeyAlreadyUsed`; the client must supply a new idempotency key for a new operation.
- If no idempotency key is provided, the handler must still reuse an existing active `Created` or `ProviderPending` checkout session for the same order instead of creating duplicate provider checkout sessions.
- Expired or creation-rejected sessions are not reused.
- Provider requests must include `ProviderClientReference` so Phase 8C can correlate provider callbacks without relying on mutable order display data.

## Provider Abstraction Interfaces

Application owns provider contracts. Infrastructure implements them.

Expected interface shape:

```csharp
public interface IPaymentCheckoutProvider
{
    string ProviderName { get; }

    Task<CreatePaymentCheckoutProviderSessionResult> CreateCheckoutSessionAsync(
        CreatePaymentCheckoutProviderSessionRequest request,
        CancellationToken cancellationToken);
}
```

Expected provider request fields:

- `PaymentOrderId`
- `CheckoutSessionId`
- `ProviderClientReference`
- `Currency`
- `AmountMinor`
- `Description`
- `SuccessUrl`
- `CancelUrl`
- `ExpiresAt`

Expected provider result fields:

- `ProviderCheckoutSessionId`
- `ProviderPaymentIntentId`
- `CheckoutUrl`
- `ExpiresAt`

Rules:

- Provider result must not include `ProviderName`; provider identity comes from `IPaymentCheckoutProvider.ProviderName`.
- The handler must persist only the injected provider's `ProviderName`, preventing provider-result mismatches.
- Provider interfaces must not accept card data.
- Provider interfaces must not expose provider secrets.
- Provider interfaces must not return raw provider payloads to Application DTOs.
- Application may log local ids and opaque provider ids but must not log checkout URLs, secrets, raw payloads, tokens, idempotency key raw values, or card data.
- Provider implementation errors are mapped to safe Application exceptions and WebApi Problem Details.
- The abstraction must be provider-neutral and must not reference a concrete vendor in interface names, DTO names, table names, or endpoint names.

## Provider Identifiers To Persist

Persist only safe correlation identifiers:

- Opaque `ProviderName` from `IPaymentCheckoutProvider.ProviderName`.
- Opaque `ProviderCheckoutSessionId` returned by the provider abstraction.
- Opaque `ProviderPaymentIntentId` returned by the provider abstraction when available.
- Platform-generated `ProviderClientReference`.

Do not persist:

- Card number.
- CVC.
- Expiry date.
- Payment method fingerprint.
- Billing card details.
- Provider API keys.
- Provider webhook secrets.
- Provider client secrets intended only for a browser SDK.
- Raw provider request or response payloads.
- Raw webhook payloads.

## Secret And Configuration Boundaries

- Provider credentials live only in Infrastructure configuration and environment-specific secret stores.
- Secrets must never be checked into source control.
- Application receives only provider-neutral results from `IPaymentCheckoutProvider`.
- Domain must not reference configuration, options, HTTP clients, or provider SDKs.
- WebApi must not bind provider secrets from requests.
- Checkout success and cancel URL templates come from configuration, not request bodies.
- The plan must not choose a real provider; concrete provider selection remains a blocking reviewer decision before real provider adapter or public checkout endpoint implementation.

## Cancellation Guard And Expiration Behavior

- Starting checkout applies the same lazy order expiry rule as Phase 8A: if the owned pending order is past due, mark it `Expired` and return `409 Conflict`.
- Checkout session `ExpiresAt` must be no later than the linked order `ExpiresAt`.
- If the provider abstraction returns a later expiry than the order expiry, the local session expiry is capped to the order expiry.
- If the provider abstraction returns an earlier expiry, the local session uses the earlier provider expiry.
- Reusing checkout returns only active `Created` or `ProviderPending` sessions with `ExpiresAt > now` and linked order still `PendingPayment`.
- The existing cancel-order operation must reject local order cancellation with `409 CheckoutInProgress` when an active `Created` or `ProviderPending` checkout session exists for that order.
- The failed cancellation must not change the `PaymentOrder` or the active `PaymentCheckoutSession`.
- The cancel-order operation must not call the provider, add provider-side cancellation interfaces, add provider-side cancellation calls, or perform best-effort provider cancellation in Phase 8B.
- Cancellation remains allowed when no active checkout exists, including when previous checkout sessions are `Expired` or `CreationRejected`.
- Provider-side checkout cancellation and reconciliation remain deferred to Phase 8C.

## API Contract

### Start My Payment Checkout

- `POST /api/v1/me/nurse-profile/payment/orders/{orderId:guid}/checkout`
- Name: `StartMyPaymentCheckout`
- Authorization: `.RequireAuthorization()`
- Request: `StartPaymentCheckoutRequest`
- Response: `PaymentCheckoutSessionDto`
- Success status: `200 OK` for both newly created provider checkout sessions and reused checkout sessions.

`StartPaymentCheckoutRequest` fields:

- `IdempotencyKey` optional string.

`PaymentCheckoutSessionDto` fields:

- `Id`
- `PaymentOrderId`
- `Status`
- `ProviderName`
- `CheckoutUrl`
- `Currency`
- `AmountMinor`
- `ExpiresAt`
- `CreatedAt`
- `UpdatedAt`

DTO must not expose:

- `NurseProfileId`
- `UserId`
- Idempotency key hash.
- Request fingerprint hash.
- Provider payment intent id unless explicitly approved for client use.
- Raw provider payloads.
- Provider secrets.
- Card data.
- Webhook data.
- Domain or EF entities.

## Error Behavior

- Missing JWT: `401 Unauthorized`.
- Authenticated user without Nurse role or nurse profile: `403 Forbidden`.
- Non-owned order: `404 Not Found`.
- Missing order: `404 Not Found`.
- Invalid route GUID: `400 Bad Request` through WebApi binding.
- Invalid idempotency key: `400 Bad Request`.
- Same nurse reuses the same idempotency key for a different order: `409 Conflict`.
- Same nurse reuses the same idempotency key and order with a different request fingerprint: `409 Conflict`.
- Order not `PendingPayment`: `409 Conflict`.
- Order expired before checkout starts: lazy-expire order then `409 Conflict`.
- Existing checkout session expired: mark local session expired and create a new one only if the order is still pending, not expired, and idempotency semantics permit a new operation.
- Existing active `Created` session has an unexpired provider-call lease held by another caller: `409 CheckoutInitializationInProgress` with `Retry-After`; do not call the provider.
- Same nurse reuses the same idempotency key after the original session is `Expired` or `CreationRejected`: `409 IdempotencyKeyAlreadyUsed`; the client must send a new idempotency key for a new operation.
- Provider configuration missing or disabled: `503 Service Unavailable` or existing mapped infrastructure-unavailable error if the project has one at implementation time.
- Provider definitive creation rejection where the adapter guarantees no provider checkout session was created: transition the local session to terminal `CreationRejected` and return safe conflict/provider-rejection Problem Details without marking the order `Failed`.
- Provider unavailable, timeout, network failure, or unknown outcome: leave local `Created` state recoverable and return safe unavailable Problem Details; do not use `CreationRejected`.
- Provider timeout after bounded retries: safe provider-unavailable Problem Details; do not expose provider internals or checkout URLs.
- Unexpected exceptions: existing `500` Problem Details behavior.

## Database Decision

Phase 8B is expected to require an EF migration because it adds a `PaymentCheckoutSessions` table. EF configuration is `MID_TIER`; migration generation, model snapshot changes, and EF verification are `PREMIUM_ONLY`.

Expected table:

- `PaymentCheckoutSessions`

Required constraints and indexes:

- Unique active checkout session per payment order. Only one `Created` or `ProviderPending` session may exist for a `PaymentOrderId` at a time; `CreationRejected` and `Expired` historical sessions may coexist.
- Unique `ProviderClientReference`.
- Unique `(ProviderName, ProviderCheckoutSessionId)` when `ProviderCheckoutSessionId` is not null.
- Unique `(NurseProfileId, IdempotencyKeyHash)` when `IdempotencyKeyHash` is not null. The handler uses this with `RequestFingerprintHash` to return the existing session for the same order/fingerprint and return `409` for a different order or different fingerprint.
- `IX_PaymentCheckoutSessions_PaymentOrderId_Status_ExpiresAt` for reuse and expiry checks.
- `IX_PaymentCheckoutSessions_NurseProfileId_PaymentOrderId` for ownership checks.
- `IX_PaymentCheckoutSessions_ProviderName_ProviderPaymentIntentId` for Phase 8C webhook lookup when payment intent id exists.
- `ProviderCheckoutSessionId` and `CheckoutUrl` are nullable columns because `Created` sessions exist before provider success; EF configuration must not mark either property globally required.
- Domain transition methods, not global database nullability, enforce that `ProviderPending` has `ProviderCheckoutSessionId` and `CheckoutUrl`.
- `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` are nullable columns used for database-backed provider-call lease acquisition and recovery.

Relationships:

- `PaymentCheckoutSession.PaymentOrderId -> PaymentOrders.Id` with restrict delete.
- `PaymentCheckoutSession.NurseProfileId -> NurseProfiles.Id` with restrict delete.

## Security Requirements

- Do not store card data.
- Do not log card data, secrets, raw provider payloads, webhook payloads, access tokens, refresh tokens, password hashes, idempotency key raw values, or checkout URLs.
- Treat checkout URLs as sensitive because they may contain bearer-like tokens.
- Validate every request.
- Return DTOs only.
- Preserve ownership boundaries and return `404` for non-owned orders.
- Do not allow clients to influence amount, currency, provider, success URL, cancel URL, provider metadata, or product snapshots.
- Enforce HTTPS-only redirect URLs through validation when provider results are persisted.
- Reject invalid or non-HTTPS checkout URLs.
- Keep provider credentials in Infrastructure configuration and secret stores only.
- Checkout endpoint responses must include `Cache-Control: no-store`.
- Checkout URLs must not appear in logs, telemetry tags, exception messages, or Problem Details.

## Testing Requirements For Implementation

Domain tests must cover:

- Checkout session creation copies order id, nurse profile id, amount, currency, and expiry.
- Checkout session expiry cannot exceed order expiry.
- Checkout session terminal states are not reused.
- `CreationRejected` is terminal and distinct from payment failure.
- `Created` permits null provider checkout session id and null checkout URL.
- `ProviderPending` requires provider checkout session id and checkout URL.
- Cancel-order guard returns `409 CheckoutInProgress` when an active `Created` or `ProviderPending` checkout session exists.

Application tests must cover:

- Start checkout requires Nurse role and current nurse profile.
- Non-owned order returns not found.
- Non-pending order returns conflict.
- Past-due order is lazily expired before checkout and returns conflict.
- Before active-session lookup or insert, all `Created` or `ProviderPending` sessions with `ExpiresAt <= now` are marked `Expired` and persisted in the transaction.
- Existing active session is reused without calling provider again.
- Active `Created` session with another caller's unexpired provider-call lease returns `409 CheckoutInitializationInProgress` with `Retry-After` and does not call provider.
- Only the caller that atomically acquires the provider-call lease calls the provider.
- Lease-expired recovery can acquire a new lease and reuse the same `ProviderClientReference`.
- Same idempotency key returns the same active session for the same nurse, order, and request fingerprint.
- Same idempotency key remains bound to the original operation after terminal state and returns `409 IdempotencyKeyAlreadyUsed` when the original session is `Expired` or `CreationRejected`.
- Same idempotency key with another order returns conflict.
- Same idempotency key and order with a different request fingerprint returns conflict.
- `RequestFingerprintHash` excludes `IdempotencyKey` and uses a versioned canonical operation/order/nurse/business-fields fingerprint.
- No idempotency key still reuses active session for the order.
- Concurrent requests for one order produce one active checkout session and one provider call.
- Concurrent requests with the same idempotency key do not duplicate provider calls.
- Persisted `Created` session after process crash is recovered using the same `ProviderClientReference`.
- Provider timeout retry uses the same `ProviderClientReference` and does not create duplicate provider calls beyond configured retry policy.
- Provider timeout exhaustion leaves safe recoverable local state or returns safe unavailable errors as specified.
- Definitive provider creation rejection transitions to `CreationRejected` only when the adapter guarantees no provider checkout session was created.
- Timeout, network failure, or unknown provider outcome leaves `Created` recoverable and never transitions to `CreationRejected`.
- Local order cancellation during an active `Created` provider call lease returns `409 CheckoutInProgress`, does not call the provider, and does not modify the order or checkout session.
- Duplicate provider checkout ids are rejected by persistence and mapped safely.
- Invalid or non-HTTPS checkout URLs are rejected and never logged.
- Provider result ids and checkout URL are persisted after validation, and `ProviderName` is persisted from `IPaymentCheckoutProvider.ProviderName` before the provider call when the `Created` session is first saved.
- Amount and currency come from `PaymentOrder`, not the request.
- `ProviderName` is taken from `IPaymentCheckoutProvider.ProviderName`, not provider result.
- No handler creates `ExamAccessGrant`.
- No handler marks orders `Paid` or `Failed`.
- DTO reflection tests reject card, secret, webhook, raw provider payload, account, and entity fields.

Infrastructure tests must cover:

- Table name, keys, relationships, delete behavior, enum conversion, required fields, nullable provider id/URL fields, max lengths, and indexes.
- Unique active checkout session per order for `Created` and `ProviderPending` only; `CreationRejected` and `Expired` are terminal/non-active.
- Unique `ProviderClientReference`.
- Unique `(ProviderName, ProviderCheckoutSessionId)` when provider checkout id is not null.
- Unique `(NurseProfileId, IdempotencyKeyHash)` when idempotency key hash is not null.
- Nullable `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` fields.
- No card columns, secret columns, raw payload columns, provider-side cancellation columns, webhook columns, or grant columns are added.

WebApi tests must cover:

- Checkout endpoint returns `401` without JWT.
- Authenticated request sends command with route order id and request body idempotency key.
- Invalid GUID returns `400` and does not call sender.
- Application conflict maps to `409`.
- Success returns `200 OK` for both new and reused checkout sessions.
- Checkout initialization in progress maps to `409` with `Retry-After`.
- Checkout endpoint responses include `Cache-Control: no-store`.
- Raw JSON response does not expose forbidden fields.

## Implementation Sequencing

Foundation may proceed first:

- Domain checkout-session model and local lifecycle.
- Application contracts, DTOs, validation, idempotency/concurrency contract, and provider interface.
- EF configuration and persistence constraints.

Provider adapter and public endpoint must wait until provider selection:

- Real Infrastructure provider adapter.
- Provider configuration/options and timeout/retry values.
- Public checkout endpoint returning redirect URLs.
- Endpoint implementation beyond isolated tests based on a fixed API contract.

## Acceptance Criteria

- Phase 8B implementation plan exists and is limited to checkout/provider abstraction.
- Specification explicitly defers webhooks, payment confirmation, order paid/failed transitions, provider-side cancellation, and `ExamAccessGrant` issuance to later phases.
- Provider-neutral Application interfaces are defined in the design.
- Provider result contract does not contain `ProviderName`.
- Checkout session lifecycle, ownership, concurrency, crash recovery, and idempotency are defined.
- API success contract is `200 OK` for both new and reused checkout sessions.
- Persisted provider identifiers are defined without storing card data or secrets.
- Required database constraints are explicit.
- Secret/configuration boundaries are defined.
- Cancellation guard, expiration, security, and failure behavior are defined.
- Real provider selection is marked as a blocking reviewer decision before adapter and public endpoint implementation.
- No source code, tests, migrations, staging, or commits are performed during this planning task.

## Reviewer Decisions Needed

- Choose the real payment provider before implementing any Infrastructure provider adapter.
- Choose the real payment provider before implementing the public checkout endpoint that returns a redirect URL.
- Approve exact provider timeout and retry policy values before adapter implementation.
- Decide exact unavailable-provider HTTP mapping if the existing exception middleware does not already support `502` or `503` provider errors.
