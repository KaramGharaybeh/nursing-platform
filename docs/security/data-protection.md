# Data Protection

## Protected information

Repository rules and current contracts identify password hashes; raw access, refresh, verification, and reset tokens; secrets and provider payloads; payment and entitlement internals; nurse/employer profile data; and protected examination questions, options, answer keys, and results as sensitive at their respective boundaries. Product capability definitions remain in Product, and exact public response schemas remain in [OpenAPI](../api/openapi.yaml).

## Current storage and transit controls

The current password service uses ASP.NET Core Identity's password hasher. Refresh, verification, and reset token handlers store SHA-256 hashes rather than raw token values. The API configures HTTPS redirection and validates JWT signatures and lifetime. These are current implementation controls; this document does not claim encryption-at-rest, a particular production secret store, or a legal retention period without supporting authority.

`PROJECT_RULES.md` prohibits frontend state and Development/Test substitutes from establishing protected server truth. The payment checkout specification forbids logging card data, secrets, raw provider payloads, webhook payloads, tokens, password hashes, raw idempotency keys, or checkout URLs. Public DTOs must not expose domain/persistence objects, password hashes, secrets, internal authorization state, or protected exam content. Current endpoint tests inspect raw JSON to catch fields a typed DTO deserializer might ignore.

## Ownership and response boundaries

Server-side ownership checks protect nurse-specific resources. Missing and non-owned package report/session responses are designed to avoid revealing another nurse's purchase facts; the current report endpoint tests exercise this behavior. Package reports exclude protected exam question and answer content. Exact response fields and status codes belong to the API contract, while [Security Verification](security-verification.md) owns how these exclusions are checked.

Preparation Package material and practice content do not gain authority to disclose the included exam's protected questions, answers, options, or rationales merely by being packaged. The approved Stage 1 content boundary allows exam-related material only when it is independently approved as material content and does not reveal protected included-exam content. The report has its own protected-content exclusion above.

Current recruitment candidate search projects a limited summary after employer-profile/organization checks and excludes unavailable, inactive-account, and unverified-email nurse profiles from both rows and counts. Current contact-request responses likewise avoid the nurse's account email and CV. Approval state alone must not be treated as proof that a contact-information response endpoint exists; the protected response shape is owned by [OpenAPI](../api/openapi.yaml).
