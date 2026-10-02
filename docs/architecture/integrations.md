# Integration Boundaries

## Payment

The approved checkout design places provider-neutral contracts at the Application boundary and provider adapters in Infrastructure. Order and entitlement decisions remain platform-owned. A provider interaction supplies checkout information and later payment evidence through an authorized completion path; it does not directly create an access grant or package entitlement. The architecture preserves separate idempotency for checkout initiation and order-item fulfillment.

Provider-specific fields on the order and direct provider calls from the Web API were rejected in the checkout design because they couple commercial records or HTTP entry to a vendor. A real production provider, callback/webhook integration, and reconciliation behavior require separate authority. The Development/Test sandbox is a replaceable substitute and is not a production provider. This document selects no vendor.

## Email

Account workflows cross an email-delivery boundary. The server owns account and security decisions; an email service transports messages. The project architecture identifies SMTP as an external service, but this layer does not select a production vendor or prescribe templates, credentials, or delivery procedures.

## Data and cache

PostgreSQL is the primary data store. Redis is an infrastructure cache, not an alternate business authority. Persistence and cache mechanics belong to their later backend owners; environment configuration, monitoring, and recovery belong to Operations.

## Other potential services

Cloud object storage, a CDN, search service, and additional payment providers appear as future possibilities in legacy architecture/deployment material. No production selection or runtime dependency for them is established here. Adding one requires a separately approved design; absence of a selected provider is not permission to invent one.
