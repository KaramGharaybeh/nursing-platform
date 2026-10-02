# Authentication and Authorization

## Enforcement responsibility

[Product roles and permissions](../product/roles-and-permissions.md) own who should have each capability. The server owns authentication, authorization, and resource ownership enforcement. API endpoint metadata states whether an operation is anonymous, authenticated, or permission-protected; [OpenAPI](../api/openapi.yaml) owns the current HTTP representation. [Frontend routing](../frontend/routing-and-permissions.md) may improve navigation and denial UX but cannot grant server access.

## Current authentication implementation

The Web API configures JWT Bearer authentication. It validates issuer, audience, lifetime, and signing key; startup rejects missing JWT secret, issuer, or audience. The request pipeline performs authentication before authorization. The current token issuer signs access tokens with HMAC-SHA256 and includes subject, email, name, token id, issuance time, and role claims. The current account model uses project-owned `User`/`UserRole` entities and authentication services; its password service uses ASP.NET Core Identity's password hasher without adopting the framework's account entity model. These are current implementation facts from `ServiceCollectionExtensions`, `ApplicationBuilderExtensions`, `JwtService`, `User`, and `PasswordHashingService`, not a new requirement to retain those mechanics forever.

The current login path checks credentials and account state; valid credentials for an unverified account do not issue tokens. Refresh tokens are random, stored by hash, expire, and rotate. Reuse of a revoked refresh token revokes other active tokens for the user. Current account recovery issues a random password-reset token, stores its hash, marks earlier unused reset tokens used, and sends the raw token through the email boundary. Reset checks ownership, use, and expiry; it changes the password hash and revokes active refresh tokens. Email-verification tokens are likewise stored by hash and checked for use and expiry. See the Identity command handlers for exact current mechanics. [Product requirements](../product/requirements.md) own the approved recovery capability.

## Current authorization implementation

The Application permission service obtains a user's permission names through assigned roles and role-permission relationships. The current `RequirePermission` endpoint helper adds a `PermissionRequirement` to an ASP.NET Core authorization policy; `PermissionAuthorizationHandler` succeeds it only for an authenticated user with the required permission. Endpoint mappings apply explicit authentication or permission metadata. Permission symbols are defined in `Permissions.cs`; the API contract owns per-operation security metadata. Resource-specific Application checks further enforce nurse ownership and Preparation Package benefit rights. A role claim or frontend route guard alone does not establish ownership of an order, entitlement, session, or report.

The server's current permission symbols and seeded assignments are implementation contracts. Product owns the meaning and approval of capabilities; this document does not expand them or treat a newly observed symbol as Product approval.
