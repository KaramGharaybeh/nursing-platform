# Commerce Screen Contracts

These contracts own Commerce screen presentation within [Product](../../product/requirements.md), [Frontend routing](../routing-and-permissions.md), [OpenAPI](../../api/openapi.yaml), and [Security](../../security/security-overview.md) authority. The human-approved Commerce screen packet owns the screen-specific visual and interaction decisions. Screen approval does not create payment success, entitlement, or provider authority.

## COM-001 Product Catalog

| Field | Contract |
|---|---|
| Identity | `COM-001`; `COMMERCE_PRODUCTS`; `/commerce/products`; authenticated. |
| Purpose | Browse backend-supplied purchasable product facts. |
| Presentation | Safe product name, nonempty description, human-readable type/category when supplied, price/currency, and backend-supplied availability. Backend ordering and pagination; no invented search/filter/sort. |
| Action and states | View details; loading, empty, and retryable error. The list does not start provider checkout. |
| Forbidden | Raw product/provider/entitlement IDs, unsupported benefits, payment or security guarantees. |

## COM-002 Product Detail

| Field | Contract |
|---|---|
| Identity | `COM-002`; `COMMERCE_PRODUCT_DETAIL`; `/commerce/products/:productId`; authenticated. |
| Purpose | Explain the selected backend product with safe factual fields. |
| Action | `Purchase` may enter the approved server-backed order-creation flow only when backend state indicates the product is currently orderable. It does not mean payment succeeded or access was granted. Back to Products remains available. |
| Forbidden | Provider checkout, card fields, invented package benefits or refund/expiry claims. |

## COM-003 Checkout and order creation

| Field | Contract |
|---|---|
| Identity | `COM-003`; `COMMERCE_CHECKOUT`; `/checkout`; Nurse frontend role guard. |
| Purpose | Explicitly confirm creation of a server-backed order. |
| Required | Show product title and backend price/currency before the request. Heading `Create order?`; actions `Create order` and `Cancel`. Prevent duplicate activation; after an ambiguous failure, do not blindly submit another order. Show only factual returned order state and recoverable mapped errors. |
| Forbidden | Implicit order creation on product-detail entry, direct payment-success or entitlement claim, editable idempotency key, raw provider data. |

## COM-004 Payment processing

`COM-004` remains a non-routable, `DEFERRED` standalone processing screen. The approved packet forbids invented card/CVV fields, provider logos, fake progress, or a client timer. A backend-supported provider interaction and separate screen approval would be needed for further presentation.

## COM-005 and COM-006 Payment outcomes

`COM-005` (`COMMERCE_PAYMENT_SUCCESS`, `/checkout/orders/:orderId/success`) and `COM-006` (`COMMERCE_PAYMENT_FAILURE`, `/checkout/orders/:orderId/failure`) are approved generic server-truth outcome states. Both must load and reconcile authoritative order/payment truth; a URL or client state alone never proves the outcome. The approved success state may show `Payment completed` and `View order`; show `Continue` only when existing fulfillment data identifies a safe learner destination. The approved failure state may show `Payment failed` and `View order`; show `Try again` only when an existing backend contract supports retry/re-initiation. Never re-POST order creation merely to retry, infer a provider flow, or reveal foreign/missing order details.

## COM-007 Order History and COM-008 Order Detail

`COM-007` (`COMMERCE_ORDERS`, `/commerce/orders`) presents nurse-owned, backend-ordered and paginated order facts with a view-detail action, empty state, and retryable error. `COM-008` (`COMMERCE_ORDER_DETAIL`, `/commerce/orders/:orderId`) presents one nurse-owned order and only backend-supported actions such as an eligible cancel action. Their presentation is bounded by the approved Commerce packet. Neither screen may infer payment success or entitlement from a local route, cached catalog item, or client calculation. The API contract owns exact DTO fields, status codes, and action availability.
