# Phase 8B Checkout + Payment Provider Abstraction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement backend-only checkout sessions and provider-neutral payment-provider abstractions for existing nurse-owned pending payment orders.

**Architecture:** Application owns checkout use cases, DTOs, validation, idempotency, concurrency orchestration, and provider interfaces. Domain owns provider-neutral checkout-session state. Infrastructure owns EF persistence, database constraints, migrations, provider configuration, and future provider implementations. WebApi maps a thin authenticated nurse checkout endpoint only after the API contract is fixed and provider selection is approved.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, MediatR, FluentValidation, EF Core, PostgreSQL, xUnit, Moq, existing WebApi test factory/JWT helpers.

## Global Constraints

- Implement only the approved Phase 8B checkout/provider-abstraction scope in `docs/superpowers/specs/2026-07-13-payment-checkout-provider-abstraction.md`.
- Split sequencing into Foundation first, then provider adapter and public endpoint only after provider selection.
- Do not implement webhooks.
- Do not implement payment confirmation.
- Do not issue `ExamAccessGrant`.
- Do not mark orders `Paid` or `Failed` from checkout start.
- Do not implement provider-side cancellation; Phase 8B blocks local order cancellation while an active checkout exists and defers provider-side checkout cancellation/reconciliation to Phase 8C.
- Do not store card data.
- Do not store provider secrets, raw provider payloads, webhook payloads, checkout URL tokens, or client secrets.
- Checkout URLs may contain bearer-like tokens and must never be logged, emitted in telemetry tags, included in exception messages, or exposed in Problem Details.
- Checkout endpoint responses must include `Cache-Control: no-store`.
- Preserve immutable Phase 8A order and price snapshots.
- Do not let clients send amount, currency, quantity, provider name, callback URLs, card data, or provider metadata.
- Do not invent or name a real payment provider.
- Provider credentials and provider-specific API clients belong only in Infrastructure.
- Domain must not depend on EF, WebApi, provider SDKs, options, HTTP clients, or Infrastructure.
- WebApi must remain thin and return DTOs only.
- Use EF Core migrations for schema changes; do not edit schema manually.
- Do not modify `CURRENT_TASK.md` or `TASKS.md` unless a future reviewer explicitly requests tracking updates after implementation verification.
- Do not modify `AGENTS.md`, `PROJECT_RULES.md`, `.gitignore`, frontend files, recruitment files, contact-request files, candidate files, employer files, exports, dashboards, or Phase 9 files.
- Do not stage or commit unless explicitly instructed.

---

## Task Classification Legend

- `FREE_DELEGATE`: Low-risk isolated tests or narrow implementation with no lifecycle, persistence, endpoint, provider, financial, or security decisions. In Phase 8B, the only approved `FREE_DELEGATE` package is Application reflection/security tests after Application contracts exist.
- `MID_TIER`: Moderate complexity or cross-layer work requiring stronger review, including domain lifecycle, EF configuration, and endpoint implementation.
- `PREMIUM_ONLY`: Architecture/security/financial/payment boundary work that must be done or reviewed by the primary engineer, including provider adapters, migrations, model snapshots, EF verification, public endpoint approval, and final review.

## Planned File Structure

Domain:

- Create `backend/src/NursingPlatform.Domain/Payments/PaymentCheckoutSession.cs`.
- Create `backend/src/NursingPlatform.Domain/Payments/PaymentCheckoutSessionStatus.cs`.
- Avoid modifying `backend/src/NursingPlatform.Domain/Payments/PaymentOrder.cs` unless a reviewer approves a specific local lifecycle helper; `PaymentOrder.cs` is not part of any `FREE_DELEGATE` package.

Application:

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs` to expose `DbSet<PaymentCheckoutSession>`.
- Create `backend/src/NursingPlatform.Application/Payments/Abstractions/IPaymentCheckoutProvider.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Abstractions/CreatePaymentCheckoutProviderSessionRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Abstractions/CreatePaymentCheckoutProviderSessionResult.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Commands/StartMyPaymentCheckout/StartPaymentCheckoutRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Commands/StartMyPaymentCheckout/StartMyPaymentCheckoutCommand.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentCheckoutSessionDto.cs`.
- Create or extend `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs` for checkout DTO mapping.
- Create `backend/src/NursingPlatform.Application/Payments/Common/PaymentIdempotencyKey.cs` only if hashing/normalization is not better kept in the command handler.

Infrastructure:

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Modify or create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentCheckoutSessionConfiguration.cs` or extend `PaymentConfigurations.cs`.
- Generate EF migration `AddPaymentCheckoutSessions` only in a `PREMIUM_ONLY` task.
- Update `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` through EF generation only in a `PREMIUM_ONLY` task.
- Add provider configuration/options only after the reviewer chooses the real provider and approves timeout/retry values.

WebApi:

- Modify `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs` only after provider selection and public endpoint approval.

Tests:

- Modify `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentValidatorTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs`.
- Modify `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs` only in `MID_TIER` endpoint work after API contract is fixed and provider selection is approved; no WebApi endpoint tests are approved for `FREE_DELEGATE`.

---

## Foundation Sequence

Foundation tasks may proceed before real provider selection:

1. Domain checkout-session model and local lifecycle.
2. Application contracts, DTOs, validation, idempotency/concurrency contract, provider interface, and safe handler behavior against a fake provider.
3. EF configuration and persistence constraints.

Provider adapter and public endpoint tasks are blocked until reviewer provider selection:

1. Real Infrastructure provider adapter.
2. Provider options, timeout, and retry policy values.
3. Public checkout endpoint that returns provider redirect URLs.
4. Endpoint implementation beyond isolated tests.

---

## Task 1: Domain Checkout Session Model

**Classification:** `MID_TIER`

**Goal:** Add provider-neutral checkout-session domain state and tests without Application, Infrastructure, WebApi, provider, webhook, provider-side cancellation, or grant behavior.

**Allowed paths:**

- `backend/src/NursingPlatform.Domain/Payments/PaymentCheckoutSession.cs`
- `backend/src/NursingPlatform.Domain/Payments/PaymentCheckoutSessionStatus.cs`
- `backend/tests/NursingPlatform.Domain.Tests/Payments/PaymentEntityTests.cs`

**Forbidden paths:**

- `backend/src/NursingPlatform.Domain/Payments/PaymentOrder.cs` unless reviewer explicitly approves a specific local helper.
- `backend/src/NursingPlatform.Application/**`
- `backend/src/NursingPlatform.Infrastructure/**`
- `backend/src/NursingPlatform.WebApi/**`
- `backend/src/NursingPlatform.Domain/Exams/**`
- `backend/tests/NursingPlatform.Application.Tests/**`
- `backend/tests/NursingPlatform.Infrastructure.Tests/**`
- `backend/tests/NursingPlatform.WebApi.Tests/**`
- `docs/**`
- `CURRENT_TASK.md`
- `TASKS.md`

**Focused tests:**

- `PaymentCheckoutSession_Create_CopiesOrderOwnershipAmountCurrencyAndExpiry`
- `PaymentCheckoutSession_Create_CapsExpiryToOrderExpiry`
- `PaymentCheckoutSession_Created_AllowsNullProviderCheckoutSessionIdAndCheckoutUrl`
- `PaymentCheckoutSession_MarkProviderPending_StoresSafeProviderIdentifiersAndCheckoutUrl`
- `PaymentCheckoutSession_MarkProviderPending_RequiresProviderCheckoutSessionIdAndCheckoutUrl`
- `PaymentCheckoutSession_MarkProviderPending_RejectsNonHttpsCheckoutUrl`
- `PaymentCheckoutSession_MarkCreationRejected_SetsTerminalCreationRejectedStatus`
- `PaymentCheckoutSession_MarkCreationRejected_IsNotPaymentFailedState`
- `PaymentCheckoutSession_ExpirePastDueProviderPending_SetsExpiredStatus`
- `PaymentCheckoutSession_TerminalStates_AreNotReusable`

**Stop conditions:**

- Stop if any card, webhook, raw provider payload, provider secret, `ExamAccessGrant`, provider SDK, EF, WebApi, or Infrastructure dependency appears necessary.
- Stop if the domain design requires naming a real provider.
- Stop if order snapshots need mutation.
- Stop if lifecycle behavior requires modifying `PaymentOrder.cs` without reviewer approval.

**Implementation notes:**

- Use enum values `Created`, `ProviderPending`, `CreationRejected`, and `Expired` only.
- Keep provider ids opaque strings.
- Treat checkout URLs as sensitive and never log them.
- Do not add `Paid`, `Failed`, `Succeeded`, `Completed`, or grant statuses to checkout sessions.
- Do not add provider-side cancellation methods.
- Domain transitions enforce state-specific invariants: `ProviderCheckoutSessionId` and `CheckoutUrl` are nullable in `Created`, required in `ProviderPending`, and `ProviderPaymentIntentId` remains optional.
- `CreationRejected` is terminal and represents only definitive provider rejection where no provider checkout session was created.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Domain.Tests --filter "PaymentCheckoutSession|PaymentEntity"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 2: Application Contracts, DTOs, Validation, And Provider Interface

**Classification:** `PREMIUM_ONLY`

**Goal:** Define safe Application checkout contracts, DTOs, validation, idempotency key handling, request fingerprinting, and provider-neutral interfaces.

**Files:**

- Modify `backend/src/NursingPlatform.Application/Abstractions/Data/IApplicationDbContext.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Abstractions/IPaymentCheckoutProvider.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Abstractions/CreatePaymentCheckoutProviderSessionRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Abstractions/CreatePaymentCheckoutProviderSessionResult.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Commands/StartMyPaymentCheckout/StartPaymentCheckoutRequest.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/Commands/StartMyPaymentCheckout/StartMyPaymentCheckoutCommand.cs`.
- Create `backend/src/NursingPlatform.Application/Payments/DTOs/PaymentCheckoutSessionDto.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentValidatorTests.cs`.

**Tests to write first:**

- `PaymentCheckoutDto_ShouldNotExposeSensitiveFieldsCardDataSecretsRawPayloadsOrEntities`
- `StartCheckoutRequest_ShouldContainOnlyOptionalIdempotencyKey`
- `StartCheckoutRequestFingerprint_ShouldExcludeIdempotencyKey`
- `StartCheckoutRequestFingerprint_ShouldUseVersionedCanonicalOperationNurseOrderAndBusinessFields`
- `StartCheckoutValidator_WithTooLongIdempotencyKey_ShouldHaveError`
- `StartCheckoutValidator_WithEmptyOrderId_ShouldHaveError`
- `PaymentCheckoutProviderInterface_ShouldNotExposeCardDataSecretsRawPayloadsOrProviderNameInResult`
- `PaymentCheckoutProviderResult_ShouldContainProviderCheckoutIdOptionalPaymentIntentCheckoutUrlAndExpiryOnly`

**Implementation notes:**

- `StartPaymentCheckoutRequest` contains only `string? IdempotencyKey`.
- `StartMyPaymentCheckoutCommand` contains route `OrderId` plus request body.
- Provider request contains local ids, platform-generated client reference, amount/currency from the order, configured URLs, and expiry.
- Provider result contains safe provider ids, checkout URL, and expiry only; it must not contain `ProviderName`.
- Provider identity is read from `IPaymentCheckoutProvider.ProviderName` to prevent result/provider mismatches.
- DTO does not expose `NurseProfileId`, `UserId`, idempotency hash, request fingerprint hash, raw provider payloads, provider secrets, card fields, webhooks, or domain entities.
- DTO and provider contracts must not expose `ProviderCallLeaseId`, `ProviderCallLeaseExpiresAt`, internal authorization state, persistence details, or raw provider errors.
- Hash idempotency keys before persistence; do not log raw idempotency keys.
- Checkout URLs may contain bearer-like tokens and must never be logged.
- Request fingerprinting excludes `IdempotencyKey` and uses a versioned canonical fingerprint of operation name, `NurseProfileId`, `PaymentOrderId`, and business request fields.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PaymentCheckoutDto|StartCheckoutValidator|PaymentCheckoutProviderInterface"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 3: Application Checkout Handler, Concurrency, Recovery, And Idempotency

**Classification:** `PREMIUM_ONLY`

**Goal:** Implement checkout-start use case, nurse ownership, lazy expiry, session reuse, idempotency behavior, concurrency protection, crash recovery, provider abstraction call, timeout/retry behavior, HTTPS URL validation, and safe failure behavior.

**Files:**

- Modify `backend/src/NursingPlatform.Application/Payments/Commands/StartMyPaymentCheckout/StartMyPaymentCheckoutCommand.cs`.
- Modify `backend/src/NursingPlatform.Application/Payments/Common/PaymentMapping.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.

**Tests to write first:**

- `Handle_StartCheckout_WhenNurseProfileMissing_ThrowsForbiddenAccessException`
- `Handle_StartCheckout_WhenOrderNotOwned_ThrowsKeyNotFoundException`
- `Handle_StartCheckout_WhenOrderMissing_ThrowsKeyNotFoundException`
- `Handle_StartCheckout_WhenOrderNotPending_ThrowsInvalidOperationException`
- `Handle_StartCheckout_WhenOrderPastDue_ExpiresOrderAndThrowsInvalidOperationException`
- `Handle_StartCheckout_ExpiresPastDueCreatedAndProviderPendingSessionsBeforeActiveLookup`
- `Handle_StartCheckout_CreatesProviderPendingCheckoutSessionFromOrderSnapshotTotals`
- `Handle_StartCheckout_DoesNotCreateExamAccessGrant`
- `Handle_StartCheckout_DoesNotMarkOrderPaidOrFailed`
- `Handle_StartCheckout_WithExistingActiveSession_ReturnsExistingSessionWithoutCallingProvider`
- `Handle_StartCheckout_WithUnexpiredProviderCallLease_ReturnsCheckoutInitializationInProgressWithRetryAfter`
- `Handle_StartCheckout_AcquiresProviderCallLeaseBeforeCallingProvider`
- `Handle_StartCheckout_AfterLeaseExpiration_RecoversWithSameProviderClientReference`
- `Handle_StartCheckout_WithSameIdempotencyKeySameOrderSameFingerprint_ReturnsExistingSessionWithoutCallingProviderAgain`
- `Handle_StartCheckout_WithSameIdempotencyKeyOriginalExpired_ThrowsIdempotencyKeyAlreadyUsed`
- `Handle_StartCheckout_WithSameIdempotencyKeyOriginalCreationRejected_ThrowsIdempotencyKeyAlreadyUsed`
- `Handle_StartCheckout_WithSameIdempotencyKeyDifferentOrder_ThrowsConflict`
- `Handle_StartCheckout_WithSameIdempotencyKeySameOrderDifferentFingerprint_ThrowsConflict`
- `Handle_StartCheckout_RequestFingerprintExcludesIdempotencyKey`
- `Handle_StartCheckout_WithoutIdempotencyKey_ReusesActiveSessionForOrder`
- `Handle_StartCheckout_WithExpiredSession_CreatesNewSessionWhenOrderStillPending`
- `Handle_StartCheckout_ConcurrentRequestsForSameOrder_CreateOneActiveSessionAndOneProviderCall`
- `Handle_StartCheckout_ConcurrentRequestsWithSameIdempotencyKey_DoNotDuplicateProviderCalls`
- `Handle_StartCheckout_WithPersistedCreatedSessionAfterCrash_RecoversUsingSameProviderClientReference`
- `Handle_StartCheckout_ProviderTimeoutRetry_UsesSameProviderClientReference`
- `Handle_StartCheckout_ProviderTimeoutExhausted_LeavesRecoverableCreatedSessionOrSafeUnavailableError`
- `Handle_StartCheckout_DefinitiveProviderCreationRejected_MarksCreationRejectedWithoutFailingOrder`
- `Handle_StartCheckout_ProviderTimeoutNetworkFailureOrUnknownOutcome_DoesNotMarkCreationRejected`
- `Handle_StartCheckout_DuplicateProviderCheckoutId_ThrowsSafeConflictOrPersistenceException`
- `Handle_StartCheckout_InvalidCheckoutUrl_RejectsAndDoesNotLogUrl`
- `Handle_StartCheckout_NonHttpsCheckoutUrl_RejectsAndDoesNotLogUrl`
- `Handle_StartCheckout_PersistsProviderIdentifiersAndCheckoutUrl`
- `Handle_StartCheckout_ProviderNameComesFromProviderInstanceNotResult`

**Exact concurrency and recovery algorithm:**

1. Validate JWT-derived nurse context, route order id, and optional idempotency key.
2. Compute `RequestFingerprintHash` as a versioned canonical fingerprint from operation name, `NurseProfileId`, `PaymentOrderId`, and business request fields that affect the operation; exclude `IdempotencyKey` itself.
3. Hash the idempotency key if provided. Use `(NurseProfileId, IdempotencyKeyHash)` as the idempotency lookup scope.
4. Open a database transaction.
5. Load the order by `PaymentOrder.Id` and current `NurseProfileId`.
6. Apply lazy order expiry; if order is not `PendingPayment`, persist expiry if needed and return conflict.
7. If the same nurse/key exists for a different order, return conflict.
8. If the same nurse/key/order exists with a different request fingerprint, return conflict.
9. Before active lookup or insert, mark every `Created` or `ProviderPending` checkout session for the order with `ExpiresAt <= now` as `Expired` and persist those transitions inside the transaction.
10. If the same nurse/key's original session is `Expired` or `CreationRejected`, return `409 IdempotencyKeyAlreadyUsed`; the client must use a new idempotency key for a new operation.
11. Reuse an active `ProviderPending` session and return `200 OK` without provider call.
12. For an active `Created` session, atomically acquire the provider-call lease by setting `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` only when no unexpired lease exists.
13. If an active `Created` session has another caller's unexpired lease, commit without provider call and return `409 CheckoutInitializationInProgress` with `Retry-After`.
14. If no active session exists, create one `Created` session with stable `ProviderClientReference`, `ProviderName` from `IPaymentCheckoutProvider.ProviderName`, copied amount/currency, capped expiry, optional key hash, request fingerprint, and an acquired provider-call lease.
15. Commit before calling the provider.
16. Only the lease owner may call provider outside the transaction with a bounded timeout and only approved retries, always using the same `ProviderClientReference`.
17. On provider success, open a new transaction, reload the session, verify lease ownership, reject expired sessions, validate HTTPS checkout URL, persist provider ids, transition to `ProviderPending`, clear the lease, and commit.
18. On definitive provider creation rejection where the adapter guarantees no provider checkout session was created, transition to terminal `CreationRejected`, clear the lease, and return a safe conflict/provider-rejection result without marking the order `Failed`.
19. On timeout, network failure, crash, or unknown provider outcome, leave recoverable `Created` state and do not use `CreationRejected`; lease expiration allows sequential recovery with the same `ProviderClientReference`.
20. If another request already transitioned the session to `ProviderPending`, return that session with `200 OK`.

**Implementation notes:**

- Correctness must rely on database constraints/transactions, not in-memory locks.
- Query owned order by `OrderId` and current `NurseProfileId`.
- Apply Phase 8A lazy expiry before checkout creation.
- Expire stale `Created` and `ProviderPending` checkout sessions before active-session lookup or insert.
- Reuse active `Created` and `ProviderPending` sessions with `ExpiresAt > now`.
- Idempotency scope is nurse plus hashed idempotency key with request fingerprint behavior; terminal original sessions return `409 IdempotencyKeyAlreadyUsed` and do not free the key for a new operation.
- Generate a local checkout session and provider client reference before provider call.
- Set `ProviderName` from `IPaymentCheckoutProvider.ProviderName` when the `Created` session is first persisted before the provider call.
- Use `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` to ensure only one provider call is in flight at a time; concurrent callers with an unexpired lease return `409 CheckoutInitializationInProgress` with `Retry-After`.
- After lease expiration, recovery may acquire a new lease and sequentially retry with the same `ProviderClientReference`.
- Provider call must use amount/currency from `PaymentOrder`, not request body.
- Provider result stores required `ProviderCheckoutSessionId`, optional `ProviderPaymentIntentId`, required `CheckoutUrl`, and capped `ExpiresAt`; `ProviderName` comes from the provider instance and is already persisted on `Created`.
- Provider retries must use the same `ProviderClientReference`.
- Do not log checkout URLs.
- Do not create `ExamAccessGrant`.
- Do not transition order to `Paid` or `Failed`.
- Do not add webhook handling.
- Do not add provider-side cancellation.
- Do not include checkout URLs in logs, telemetry tags, exception messages, or Problem Details.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "StartCheckout|PaymentHandler"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 4: EF Configuration

**Classification:** `MID_TIER`

**Goal:** Configure checkout-session persistence with safe provider identifiers, idempotency hashes, request fingerprints, ownership indexes, active-session uniqueness, and Phase 8C correlation indexes. Do not generate migrations in this task.

**Files:**

- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Modify `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs` or create `backend/src/NursingPlatform.Infrastructure/Persistence/Configurations/PaymentCheckoutSessionConfiguration.cs`.
- Modify `backend/tests/NursingPlatform.Infrastructure.Tests/Persistence/PaymentConfigurationTests.cs`.

**Forbidden paths:**

- `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/**`
- `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`

**Tests to write first:**

- `PaymentCheckoutSessionConfiguration_UsesExpectedTableNameAndPrimaryKey`
- `PaymentCheckoutSessionConfiguration_StoresStatusAsStringWithMaxLength`
- `PaymentCheckoutSessionConfiguration_ConfiguresRequiredProviderNameCurrencyAndAmount`
- `PaymentCheckoutSessionConfiguration_DoesNotGloballyRequireProviderCheckoutSessionIdOrCheckoutUrl`
- `PaymentCheckoutSessionConfiguration_ConfiguresOrderAndNurseRestrictDeleteRelationships`
- `PaymentCheckoutSessionConfiguration_ConfiguresUniqueActiveCheckoutSessionPerOrder`
- `PaymentCheckoutSessionConfiguration_ConfiguresCreationRejectedAsNonActiveTerminalStatus`
- `PaymentCheckoutSessionConfiguration_ConfiguresUniqueProviderClientReference`
- `PaymentCheckoutSessionConfiguration_ConfiguresUniqueProviderNameAndCheckoutSessionIdWhenNotNull`
- `PaymentCheckoutSessionConfiguration_ConfiguresUniqueNurseAndIdempotencyKeyHashWhenNotNull`
- `PaymentCheckoutSessionConfiguration_ConfiguresNullableProviderCallLeaseFields`
- `PaymentCheckoutSessionConfiguration_ConfiguresRequestFingerprintHash`
- `PaymentCheckoutSessionConfiguration_DoesNotAddCardSecretRawPayloadWebhookProviderCancellationOrGrantColumns`

**Implementation notes:**

- Table name: `PaymentCheckoutSessions`.
- Store enum as string max length 32.
- Use max lengths for provider name, provider ids, client reference, idempotency hash, request fingerprint hash, URL, and currency.
- Configure restrict delete relationships to `PaymentOrders` and `NurseProfiles`.
- Add explicit database constraints listed in the spec.
- Unique active-session filter includes only `Created` and `ProviderPending`; `CreationRejected` and `Expired` are terminal/non-active.
- `ProviderCheckoutSessionId` and `CheckoutUrl` must be nullable in the database because `Created` sessions are persisted before provider success; domain transitions enforce they are present in `ProviderPending`.
- Add nullable `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt` for database-backed provider-call lease acquisition.
- Do not modify Phase 8A migration files.
- Do not add payment permission seed data.
- Do not create migrations manually or through EF in this task.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Infrastructure.Tests --filter "PaymentCheckoutSessionConfiguration|PaymentConfiguration"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 5: EF Migration, Model Snapshot, And EF Verification

**Classification:** `PREMIUM_ONLY`

**Goal:** Generate and verify the EF migration and model snapshot for checkout-session persistence after EF configuration is approved.

**Files:**

- Generate migration under `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/` named `AddPaymentCheckoutSessions`.
- Update `backend/src/NursingPlatform.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` through EF generation only.

**Review checklist:**

- Migration contains only checkout-session schema changes.
- Migration includes unique active checkout session per payment order.
- Migration unique active-session predicate includes only `Created` and `ProviderPending`.
- Migration includes unique `ProviderClientReference`.
- Migration includes unique `(ProviderName, ProviderCheckoutSessionId)` when checkout id is not null.
- Migration includes unique `(NurseProfileId, IdempotencyKeyHash)` when idempotency key hash is not null.
- Migration includes request fingerprint storage.
- Migration includes nullable `ProviderCallLeaseId` and `ProviderCallLeaseExpiresAt`.
- Migration keeps `ProviderCheckoutSessionId` and `CheckoutUrl` nullable globally.
- Migration does not add card, secret, raw payload, provider-side cancellation, webhook, grant, paid, or failed columns.
- Model snapshot matches approved EF configuration.

**Verification commands:**

- `dotnet ef migrations add AddPaymentCheckoutSessions --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 6: Application Contract And DTO Security Reflection Tests

**Classification:** `FREE_DELEGATE`

**Goal:** After Application contracts exist, add reflection/security tests proving checkout request, result, provider contracts, and DTOs expose no forbidden fields. Do not implement handlers, endpoints, EF, provider adapters, migrations, lifecycle changes, or endpoint integration tests in this package.

**Allowed paths:**

- `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`

**Forbidden paths:**

- `backend/src/NursingPlatform.WebApi/**`
- `backend/src/NursingPlatform.Domain/**`
- `backend/src/NursingPlatform.Application/**`
- `backend/src/NursingPlatform.Infrastructure/**`
- `backend/tests/NursingPlatform.Domain.Tests/**`
- `backend/tests/NursingPlatform.Infrastructure.Tests/**`
- `backend/tests/NursingPlatform.WebApi.Tests/**`
- `frontend/**`
- `docs/**`
- `CURRENT_TASK.md`
- `TASKS.md`

**Focused tests:**

- `PaymentCheckoutRequest_ShouldNotExposeForbiddenFields`
- `PaymentCheckoutDto_ShouldNotExposeForbiddenFields`
- `PaymentCheckoutProviderRequest_ShouldNotExposeCardSecretsRawPayloadsOrCheckoutUrlTokens`
- `PaymentCheckoutProviderResult_ShouldNotExposeProviderNameSecretsRawPayloadsOrInternalState`
- `PaymentCheckoutContracts_ShouldNotExposeProviderCallLeaseFields`
- `PaymentCheckoutContracts_ShouldNotExposeDomainOrEfEntities`

**Stop conditions:**

- Stop if Application request/result/provider contracts or DTOs do not exist yet.
- Stop if adding tests requires endpoint implementation, endpoint integration tests, handler implementation, provider configuration, Infrastructure changes, webhook routes, payment confirmation routes, grant routes, admin routes, or frontend changes.
- Stop if any tested contract exposes provider secrets, raw payloads, card fields, lease fields, internal authorization state, `NurseProfileId`, `UserId`, domain entities, EF entities, or raw idempotency keys.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PaymentCheckoutRequest|PaymentCheckoutDto|PaymentCheckoutProvider|PaymentCheckoutContracts"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 7: Provider Adapter

**Classification:** `PREMIUM_ONLY`

**Goal:** Implement the real provider adapter only after reviewer provider selection and timeout/retry values are approved.

**Blocking prerequisites:**

- Reviewer chooses the real payment provider.
- Reviewer approves provider timeout value.
- Reviewer approves provider retry policy.
- Reviewer approves provider configuration shape.

**Files:**

- Add provider-specific Infrastructure files only after paths are approved for the selected provider.
- Modify Infrastructure dependency injection only after provider selection.
- Add provider adapter tests for the selected provider abstraction boundary.

**Tests to write first:**

- Provider request maps local ids, amount, currency, URLs, expiry, and `ProviderClientReference`.
- Provider timeout uses the approved timeout value.
- Provider retry uses the same `ProviderClientReference`.
- Provider response maps checkout id, optional payment intent id, checkout URL, and expiry without returning `ProviderName`.
- Definitive provider creation rejection is only returned when the adapter can guarantee no provider checkout session was created.
- Timeout, network failure, and unknown provider outcomes are returned as recoverable failures and never mapped to `CreationRejected`.
- Provider adapter rejects invalid or non-HTTPS checkout URLs.
- Provider adapter never logs checkout URLs, secrets, raw payloads, or tokens.

**Stop conditions:**

- Stop if provider selection is still unresolved.
- Stop if credentials, checkout URLs, raw provider payloads, or bearer-like tokens would be logged.
- Stop if checkout URLs would appear in telemetry tags, exception messages, or Problem Details.
- Stop if implementation requires webhooks, payment confirmation, provider-side cancellation, paid/failed order transitions, or grants.

---

## Task 8: WebApi Checkout Endpoint Implementation

**Classification:** `MID_TIER` with `PREMIUM_ONLY` review

**Goal:** Add the single Phase 8B nurse checkout-start endpoint after provider selection and API contract approval, while keeping endpoint behavior thin.

**Blocking prerequisites:**

- API contract fixed.
- Provider selection approved.
- Provider adapter or approved test provider available.
- Premium review confirms public endpoint can return redirect URLs.

**Allowed paths:**

- `backend/src/NursingPlatform.WebApi/Extensions/ApplicationBuilderExtensions.cs`
- `backend/tests/NursingPlatform.WebApi.Tests/IntegrationTests/PaymentEndpointsTests.cs`

**Forbidden paths:**

- `backend/src/NursingPlatform.Domain/**`
- `backend/src/NursingPlatform.Application/**` except references already created by earlier tasks may be used, not modified.
- `backend/src/NursingPlatform.Infrastructure/**`
- `backend/src/NursingPlatform.WebApi/Program.cs`
- `backend/tests/NursingPlatform.Domain.Tests/**`
- `backend/tests/NursingPlatform.Application.Tests/**`
- `backend/tests/NursingPlatform.Infrastructure.Tests/**`
- `frontend/**`
- `docs/**`
- `CURRENT_TASK.md`
- `TASKS.md`

**Focused tests:**

- `PaymentCheckoutEndpoint_WithoutJwt_ReturnsUnauthorized`
- `StartCheckout_WithValidRequest_SendsCommandWithRouteOrderIdAndIdempotencyKey`
- `StartCheckout_WithInvalidGuid_ReturnsBadRequestAndSenderNotCalled`
- `StartCheckout_WhenApplicationConflict_ReturnsConflict`
- `StartCheckout_WhenInitializationInProgress_ReturnsConflictWithRetryAfter`
- `StartCheckout_NewSession_ReturnsOk`
- `StartCheckout_ReusedSession_ReturnsOk`
- `StartCheckout_SuccessResponse_UsesCacheControlNoStore`
- `PaymentCheckoutJson_DoesNotExposeSensitiveFieldsCardDataSecretsRawPayloadsOrEntities`

**Stop conditions:**

- Stop if adding this endpoint requires webhook routes, payment confirmation routes, provider-side cancellation routes, grant routes, admin routes, or frontend changes.
- Stop if endpoint needs `.RequirePermission(...)` or `.AllowAnonymous()`.
- Stop if response DTO exposes provider secrets, raw payloads, card fields, or `NurseProfileId`.

**Implementation notes:**

- Add only `POST /api/v1/me/nurse-profile/payment/orders/{orderId:guid}/checkout` under the existing nurse profile group.
- Use `.RequireAuthorization()` through the existing nurse profile group behavior if already inherited; otherwise add it consistently with neighboring payment order endpoints.
- Send `StartMyPaymentCheckoutCommand { OrderId = orderId, Request = request }`.
- Return `200 OK` for both newly created and reused checkout sessions.
- Add `Cache-Control: no-store` to checkout responses.
- Map checkout initialization in progress to `409` with `Retry-After`.
- Do not add webhook, paid, failed, refund, provider-admin, export, or dashboard endpoints.
- Do not include checkout URLs in logs, telemetry tags, exception messages, or Problem Details.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.WebApi.Tests --filter "PaymentCheckout|StartCheckout|PaymentEndpoints"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 9: Local Order Cancellation Guard

**Classification:** `MID_TIER`

**Goal:** Ensure local order cancellation is rejected when an active checkout session exists, without provider calls, checkout-session mutation, provider confirmation, provider-side cancellation, or webhook behavior.

**Files:**

- Modify `backend/src/NursingPlatform.Application/Payments/Commands/CancelMyPaymentOrder/CancelMyPaymentOrderCommand.cs`.
- Modify `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentHandlerTests.cs`.

**Tests to write first:**

- `Handle_CancelOrder_WithActiveCreatedCheckoutSession_ReturnsCheckoutInProgress`
- `Handle_CancelOrder_WithActiveProviderPendingCheckoutSession_ReturnsCheckoutInProgress`
- `Handle_CancelOrder_WithActiveCheckoutSession_DoesNotModifyOrderOrCheckoutSession`
- `Handle_CancelOrder_WithActiveCheckoutSession_DoesNotCallProviderCancellationWebhookPaymentConfirmationOrGrantLogic`
- `Handle_CancelOrder_WithExpiredCheckoutSession_AllowsLocalOrderCancellation`
- `Handle_CancelOrder_WithCreationRejectedCheckoutSession_AllowsLocalOrderCancellation`
- `Handle_CancelOrder_DuringProviderCallLease_ReturnsCheckoutInProgressAndDoesNotModifySession`

**Implementation notes:**

- Keep Phase 8A local order cancellation semantics only when no active checkout exists.
- Reject cancellation with `409 CheckoutInProgress` when an active `Created` or `ProviderPending` checkout session exists.
- Do not call the provider.
- Do not modify the active checkout session.
- Do not modify the `PaymentOrder` when cancellation is rejected.
- Allow cancellation when no active checkout exists, including when previous sessions are `Expired` or `CreationRejected`.
- Do not add provider-side cancellation interfaces or calls.
- Provider-side checkout cancellation and reconciliation remain deferred to Phase 8C.

**Verification commands:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "CancelOrder|PaymentCheckout"`
- `dotnet build backend/NursingPlatform.slnx`
- `git status --short --untracked-files=all`

---

## Task 10: Final Verification And Scope Review

**Classification:** `PREMIUM_ONLY`

**Goal:** Verify the implementation is complete, secure, and remains strictly inside Phase 8B.

**Review checklist:**

- Confirm no frontend files changed.
- Confirm no webhook endpoint, webhook handler, webhook signature validation, or webhook payload persistence was added.
- Confirm no payment confirmation logic was added.
- Confirm no handler transitions orders to `Paid` or `Failed`.
- Confirm no provider-side cancellation behavior was added.
- Confirm no `ExamAccessGrant` is created.
- Confirm no card data, provider secrets, raw provider payloads, webhook payloads, checkout URL tokens, or client secrets are persisted or exposed.
- Confirm checkout URLs are never logged.
- Confirm checkout URLs do not appear in telemetry tags, exception messages, or Problem Details.
- Confirm checkout endpoint responses include `Cache-Control: no-store`.
- Confirm order item snapshots remain immutable and are not recalculated from product data during checkout.
- Confirm checkout amount/currency come from `PaymentOrder` only.
- Confirm provider interface and persistence use provider-neutral names.
- Confirm provider result contract does not contain `ProviderName`.
- Confirm `ProviderName` is taken from `IPaymentCheckoutProvider.ProviderName`.
- Confirm `ProviderName` is persisted from `IPaymentCheckoutProvider.ProviderName` when `Created` is first persisted before the provider call.
- Confirm `ProviderCheckoutSessionId` and `CheckoutUrl` are nullable in `Created`, required by domain transition rules in `ProviderPending`, and not globally required in EF configuration.
- Confirm `ProviderPaymentIntentId` remains optional.
- Confirm stale `Created` and `ProviderPending` sessions are expired in the transaction before active-session lookup or insert.
- Confirm `CreationRejected` is terminal, distinct from payment failure, and used only for definitive provider rejection with guaranteed no provider checkout session creation.
- Confirm timeout, network failure, and unknown provider outcome leave `Created` recoverable and never use `CreationRejected`.
- Confirm provider-call lease fields serialize provider calls and concurrent unexpired leases return `409 CheckoutInitializationInProgress` with `Retry-After`.
- Confirm real provider was selected before provider adapter and public endpoint implementation.
- Confirm checkout endpoint is authenticated and nurse ownership is enforced in Application.
- Confirm success is `200 OK` for both new and reused checkout sessions.
- Confirm non-owned orders return `404`.
- Confirm idempotent retries reuse active sessions and do not duplicate provider calls.
- Confirm terminal original idempotency sessions return `409 IdempotencyKeyAlreadyUsed` and require a new idempotency key.
- Confirm request fingerprints exclude `IdempotencyKey` and use a versioned canonical operation/nurse/order/business-fields fingerprint.
- Confirm concurrency tests prove one active checkout session and one provider call for concurrent requests.
- Confirm crash-recovery tests prove persisted `Created` session reuse with stable `ProviderClientReference`.
- Confirm timeout/retry tests prove same provider reference and safe failure behavior.
- Confirm local order cancellation guard tests prove active `Created` and `ProviderPending` sessions return `409 CheckoutInProgress`, do not call the provider, and do not modify the order or checkout session.
- Confirm duplicate provider id tests prove persistence rejection or safe conflict mapping.
- Confirm non-HTTPS checkout URL tests prove rejection and no URL logging.
- Confirm EF migration includes only checkout-session schema and expected model snapshot updates.
- Confirm no files are staged unless explicitly instructed.
- Confirm no commit was made unless explicitly instructed.

**Verification commands:**

- `dotnet build backend/NursingPlatform.slnx`
- `dotnet test backend/NursingPlatform.slnx`
- `dotnet ef migrations has-pending-model-changes --project backend/src/NursingPlatform.Infrastructure --startup-project backend/src/NursingPlatform.WebApi --context ApplicationDbContext`
- `git status --short --untracked-files=all`
- `git diff --stat`
- `git diff --cached --stat`

**Stop condition:** Stop if build, tests, EF model check, security review, or scope review fails after two fix cycles.

---

## Delegation Packages

### Package A: FREE_DELEGATE Application Contract And DTO Security Tests

**Task:** Task 6.

**Allowed paths:**

- `backend/tests/NursingPlatform.Application.Tests/Payments/PaymentDtoSecurityTests.cs`

**Forbidden paths:**

- Any Domain, Application, Infrastructure, WebApi source, WebApi test, frontend, docs, migration, tracking, or config file.

**Focused tests:**

- `dotnet test backend/tests/NursingPlatform.Application.Tests --filter "PaymentCheckoutRequest|PaymentCheckoutDto|PaymentCheckoutProvider|PaymentCheckoutContracts"`

**Stop conditions:**

- Use this package only after Application contracts exist.
- Stop on endpoint implementation, endpoint integration tests, handler implementation, provider config, lifecycle changes, EF changes, webhook routes, payment confirmation routes, grant routes, admin routes, `.AllowAnonymous()`, `.RequirePermission(...)`, or sensitive DTO exposure.

There is no `FREE_DELEGATE` WebApi endpoint test package and no `FREE_DELEGATE` domain checkout package. Endpoint implementation and endpoint integration tests remain `MID_TIER` with `PREMIUM_ONLY` review. Domain checkout lifecycle work is `MID_TIER`, and `PaymentOrder.cs` is excluded unless a reviewer explicitly approves a specific local helper.

---

## Self-Review Checklist

- Spec coverage: The plan covers provider-neutral checkout architecture, checkout ownership/lifecycle, idempotency, concurrency, crash recovery, provider abstraction, persisted provider ids, secret/config boundaries, cancellation guard/expiration, security, and failure behavior.
- Scope check: Webhooks, payment confirmation, order paid/failed transitions, provider-side cancellation, and `ExamAccessGrant` issuance remain out of Phase 8B.
- Provider check: Real provider selection is a blocking reviewer decision before adapter and public endpoint implementation.
- API check: Success status is `200 OK` for both newly created and reused checkout sessions.
- Provider contract check: Provider result does not include `ProviderName`; provider identity comes from `IPaymentCheckoutProvider.ProviderName`.
- Database check: Unique active session, `ProviderClientReference`, provider checkout id, and nurse idempotency key constraints are explicit.
- Lease check: Database-backed provider-call lease allows only the lease owner to call the provider, returns `409 CheckoutInitializationInProgress` with `Retry-After` for concurrent unexpired leases, and allows sequential recovery retries with the same `ProviderClientReference` after lease expiration.
- Nullability check: `ProviderCheckoutSessionId` and `CheckoutUrl` are nullable in `Created`, required by domain transitions in `ProviderPending`, and not globally required by EF; `ProviderPaymentIntentId` remains optional.
- Idempotency check: Terminal original sessions return `409 IdempotencyKeyAlreadyUsed`; request fingerprints exclude `IdempotencyKey` and are versioned canonical operation/nurse/order/business-field fingerprints.
- CreationRejected check: `CreationRejected` is terminal, distinct from payment failure, and used only when no provider checkout session was created; unknown outcomes keep `Created` recoverable.
- Security check: Card data, provider secrets, raw payloads, webhook payloads, checkout URLs, and client secrets are excluded from logging, telemetry tags, exception messages, Problem Details, and exposure; checkout responses use `Cache-Control: no-store`.
- Snapshot check: Checkout uses existing immutable order totals and item snapshots; it does not recalculate from product data.
- Delegation check: Domain checkout is `MID_TIER`; WebApi endpoint implementation and endpoint integration tests are `MID_TIER` with `PREMIUM_ONLY` review; EF configuration is `MID_TIER`; migration/model snapshot/EF verification are `PREMIUM_ONLY`; `FREE_DELEGATE` may write only Application contract/DTO reflection security tests after Application contracts exist.
