# Commerce Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-COMMERCE
status: deep-extracted-family-contract
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Primary authority: `commerce-screen-approval-packet.md`, `system-design-contract.md`, T-FE-081/082/083 evidence, payment backend/OpenAPI contracts, and T-FE-084 deferral. Checkout and payment-processing remain deferred.

## COM-001 Product Catalog

| Field | Contract |
|---|---|
| Identity | `/commerce/products`; authenticated-only. |
| Purpose | Browse purchasable backend products. |
| List/card data | Product title/name, description, product type/category, price/currency formatted with approved Intl/ECMA-402 minor-unit decision, availability. No raw product IDs/provider IDs. |
| Pagination/order | Backend page/order. Empty state if no products. |
| Actions | View details. No Purchase control here unless future owner approves. |
| Status | `CONTRACT_READY`. |

## COM-002 Product Detail

| Field | Contract |
|---|---|
| Identity | `/commerce/products/:productId`; route param internal. |
| Purpose | Understand purchasable product. |
| Data | Product safe fields from DTO; price/currency; content summary only if supplied. Unavailable/not found states privacy-safe. |
| Actions | Back to Products. Purchase/Checkout entry remains deferred to T-FE-084 and must not be displayed as active implementation authority. |
| Forbidden | Provider claims, card fields, payment security claims, invented benefits. |
| Status | `CONTRACT_READY`. |

## COM-003 Checkout

| Field | Contract |
|---|---|
| Identity | `/checkout`; Nurse role. |
| Current decision | Design approved but implementation intentionally deferred by T-FE-084 sequencing hold. |
| Allowed future shape | Confirmation/workflow with product title and price/currency; Create order/Cancel; duplicate activation prevention; ambiguous failure reconciliation. |
| Forbidden now | Do not implement checkout/order creation or active purchase controls while deferred. |
| Status | `DEFERRED`. |

## COM-004 Payment Processing

| Field | Contract |
|---|---|
| Identity | Non-routable/deferred processing state. |
| Gap | Provider/callback architecture and production payment decisions are not approved. |
| Status | `DEFERRED`. |

## COM-005 Payment Success

| Field | Contract |
|---|---|
| Identity | `/checkout/orders/:orderId/success`; Nurse role. |
| Known authority | Route exists; outcome must be server-truth order/payment state, not URL truth. |
| Missing | Exact success reconciliation contract, fields, actions, and provider-flow semantics remain incomplete. |
| Status | `AUTHORITY_GAP`. |

## COM-006 Payment Failure

| Field | Contract |
|---|---|
| Identity | `/checkout/orders/:orderId/failure`; Nurse role. |
| Missing | Exact failure reconciliation, retry/return behavior, and provider/backend semantics remain incomplete. |
| Status | `AUTHORITY_GAP`. |

## COM-007 Order History

| Field | Contract |
|---|---|
| Identity | `/commerce/orders`; Nurse role. |
| Source | Human-approved commerce packet HC-C9, HC-C11–C15; `PaymentOrderDto` and typed paginated order-list OpenAPI response. |
| List | Owner-scoped, backend `CreatedAt DESC, Id` order and backend page/pageSize (20) facts. Human-readable item title, created date/time, formatted total/currency and factual status, View order to `/commerce/orders/:orderId`. No search/filter/sort controls; raw identifiers/internal fields never shown. |
| States | Shared loading; empty `No orders yet` / `Your orders will appear here after you create one.` with Browse products to `/commerce/products`; error `We couldn't load your orders. Try again.` with Retry on the same page. |
| Responsive/accessibility | Mobile list cards reflow within the 390px viewport; semantic headings/lists, keyboard links and pagination, logical properties, readable mixed-direction money; shared focus and live status behavior. |
| Status | `CONTRACT_READY` per approved HC-C9 and typed response prerequisite; implementation review tracked by `T-FE-088` batch record. |

## COM-008 Order Detail

| Field | Contract |
|---|---|
| Identity | `/commerce/orders/:orderId`; Nurse role. |
| Source | Human-approved commerce packet HC-C10–C15; owner-scoped typed order-detail/cancel OpenAPI responses and backend `PaymentOrder.Cancel`/cancel handler. |
| Facts | Item titles, created date/time, formatted total/currency and status; paid date only for Paid when supplied. Factual statuses: Pending payment, Paid, Failed, Cancelled, Expired; unknown status fails safely. No raw ids, provider internals, or entitlement detail. |
| Cancellation | PendingPayment only, conditional behind inline explicit `Cancel order?` / `This order will be cancelled if it is still eligible for cancellation.` / Keep order / Cancel order confirmation. No optimistic success. One request per activation, reload owner-scoped detail after success and conflict; 409: `This order can no longer be cancelled.`; no blind mutation retry. Active checkout can produce 409 even for pending orders. |
| States/navigation | Shared factual loading/error with Retry; owner-hidden 404 `This order isn't available.`; Back to orders always `/commerce/orders`. Confirmation heading receives focus and Keep order returns focus; status text is not color-only. |
| Responsive/accessibility | Narrow-screen facts stack, logical layout, keyboard-complete controls with minimum target size, semantic headings/definition list, privacy-safe live error and status. |
| Status | `CONTRACT_READY` per approved HC-C10 and typed response prerequisite; implementation review tracked by `T-FE-088` batch record. |
