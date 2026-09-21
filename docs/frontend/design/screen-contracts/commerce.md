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
| Known future list | Order/product title, created date, total amount/currency, status, View order; backend page/order. |
| Missing | Current implementation/screen authority and exact OpenAPI display contract not extracted/closed for implementation. |
| Status | `AUTHORITY_GAP`. |

## COM-008 Order Detail

| Field | Contract |
|---|---|
| Identity | `/commerce/orders/:orderId`; Nurse role. |
| Known future detail | Order facts/status/dates/items safe fields; cancel confirmation only if backend allows. |
| Missing | Exact fields, cancellation eligibility, conflict handling, and implementation authority remain incomplete. |
| Status | `AUTHORITY_GAP`. |
