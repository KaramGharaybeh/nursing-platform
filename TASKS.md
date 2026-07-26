# Nursing Platform Roadmap

This document defines the implementation roadmap for the Nursing Platform.

Development must follow the phases in order.

Do not start work on a later phase until the current phase is complete.

The numbered phases remain historically accurate. The Preparation Package Workstream below is an explicitly authorized additive workstream; starting it does not mark, skip, or complete Phases 9–11. Within that workstream, Stage 1 -> Stage 2 -> Stage 3 -> Stage 4 order is mandatory, and no later preparation-package stage may start before the earlier stage is completed and approved.

---

# Phase 1 — Project Foundation ✅

## Documentation

- [x] Product vision
- [x] System architecture
- [x] Backend architecture
- [x] Frontend architecture
- [x] Database design
- [x] API design
- [x] Engineering standards
- [x] Development guide
- [x] Deployment guide

## Repository

- [x] Git repository
- [x] Project standards
- [x] AI agent instructions
- [x] Project roadmap

## Backend

- [x] .NET solution
- [x] Clean Architecture projects
- [x] Project references
- [x] Initial Web API

## Infrastructure

- [x] Docker Compose
- [x] PostgreSQL
- [x] Redis
- [x] MailPit

---

# Phase 2 — Platform Foundation ✅

## API

- [x] Global exception handling
- [x] Problem Details (RFC 7807)
- [x] Health checks
- [x] API versioning
- [x] Swagger / OpenAPI

## Configuration

- [x] Configuration management
- [x] Options pattern
- [x] Environment configuration
- [x] Secret management

## Dependency Injection

- [x] Application registration
- [x] Infrastructure registration
- [x] Presentation registration

## Logging

- [x] Serilog
- [x] Structured logging
- [ ] Request correlation

---

# Phase 3 — Data Layer

## Entity Framework Core

- [x] ApplicationDbContext
- [x] Initial migration
- [x] Database initialization
- [x] Direct EF Core `ApplicationDbContext` persistence; repository/unit-of-work abstractions intentionally not adopted

## Reference Data

- [x] Countries
- [x] Languages
- [x] Roles
- [x] Permissions

---

# Phase 4 — Identity & Security

## Phase 4A — Core Identity ✅

- [x] User management (admin create + register done, full CRUD deferred)
- [x] Authentication (login, JWT pipeline)
- [x] Authorization (completed in 4B)
- [x] JWT (issuance, validation, services)
- [x] Refresh tokens (rotation, revocation detection)
- [x] Email verification
- [x] Password reset
- [x] Role management (admin bootstrap + seed done, full CRUD deferred)
- [x] Permission management (seed done, full CRUD deferred)

## Phase 4B — Authorization ✅

- [x] Permission authorization handler and requirement
- [x] Permission service
- [x] Current user service
- [x] Reference data entities (Permission, Role, RolePermission)
- [x] EF Core configurations for reference data
- [x] Reference data seeder (idempotent, testable)
- [x] RequirePermission extension method for Minimal API
- [x] Register endpoint protected with Users.Create permission
- [x] IPermissionService mocked in WebApi tests
- [x] JWT KeyId fix for JsonWebTokenHandler compatibility
- [x] Integration tests (register 401/403/200, login/refresh no-auth)
- [x] Unit tests (handler, requirement, service, permissions)

## Phase 4C — Account Management Read APIs ✅

- [x] PaginatedResult, UserDetailDto, UserListItemDto
- [x] GetCurrentUserQuery + handler + tests
- [x] GetUserQuery + handler + validator + tests
- [x] ListUsersQuery + handler + validator + tests
- [x] GET /api/v1/me endpoint + integration tests
- [x] GET /api/v1/users and GET /api/v1/users/{id} endpoints + integration tests
- [x] Final build, test, EF migration verification

## Phase 4D — Identity Account Recovery & Verification ✅

- [x] Email verification tokens and persistence
- [x] Password reset tokens and persistence
- [x] Email service / MailKit
- [x] POST /api/v1/auth/send-verification-email
- [x] POST /api/v1/auth/verify-email
- [x] POST /api/v1/auth/forgot-password
- [x] POST /api/v1/auth/reset-password
- [x] Application handler tests
- [x] WebApi integration tests
- [x] EF migration
- [x] Final build, test, and EF verification

---

# Phase 5 — Nurse Module ✅

- [x] Nurse profile
- [x] Experience
- [x] Education
- [x] Certificates
- [x] Skills
- [x] Languages
- [x] CV upload

---

# Phase 6 — Employer Module ✅

- [x] Employer profile
- [x] Organization management
- [x] Candidate search
- [x] Candidate filtering
- [x] Contact requests

---

# Phase 7 — Examination Module ✅

- [x] Countries
- [x] Categories
- [x] Exam-version question bank (exam-version question content; distinct from the planned Practice Bank)
- [x] Mock exams
- [x] Exam sessions
- [x] Timer
- [x] Auto scoring
- [x] Results
- [x] Admin content management
- [x] Analytics

---

# Phase 8 — Payments

- [x] Payment products
- [x] Payment orders
- [x] Order snapshots
- [x] Nurse order APIs
- [x] Checkout session foundation
- [x] Provider abstraction
- [x] Checkout idempotency/concurrency core
- [x] Sandbox provider for Development/Test
- [x] Sandbox checkout initialization
- [x] Sandbox completion endpoint
- [x] Atomic Paid transition
- [x] ExamAccessGrant issuance
- [x] Purchased exam access enforcement
- [x] PostgreSQL payment concurrency/rollback tests
- [x] Sandbox purchase-to-exam-start integration test
- [ ] Production payment provider
- [ ] Public production webhook
- [ ] Signature verification
- [ ] Provider reconciliation
- [ ] Refunds
- [ ] Subscriptions
- [ ] Coupons, taxes, invoices, wallets, and payouts
- [ ] Production payment reporting/admin operations

---

# Phase 9 — Administration

- [ ] Dashboard
- [ ] User management
- [ ] Question management
- [ ] Category management
- [ ] Reports
- [ ] Audit logs
- [ ] System settings

---

# Phase 10 — Frontend

- [ ] Angular application
- [ ] Authentication
- [ ] Dashboard
- [ ] Nurse portal
- [ ] Employer portal
- [ ] Administration portal
- [ ] Shared component library

---

# Phase 11 — Production Readiness

## Quality

- [ ] Unit tests
- [ ] Integration tests
- [ ] End-to-end tests

## DevOps

- [ ] CI/CD pipeline
- [ ] Docker production images
- [ ] Monitoring
- [ ] Health monitoring
- [ ] Backup strategy
- [ ] Security hardening

## Deployment

- [ ] Production deployment
- [ ] Performance testing
- [ ] Load testing
- [ ] Disaster recovery validation

---

# Preparation Package Workstream (Planned)

This workstream introduces the paid preparation package product described in `docs/superpowers/specs/2026-07-25-preparation-package-architecture-decisions.md`. It is an explicitly authorized additive workstream and is currently in the specification/documentation phase. Starting it does not mark, skip, or complete Phases 9–11. No preparation-package capability is implemented today. Stage 1 -> Stage 2 -> Stage 3 -> Stage 4 order is mandatory; no later stage may start before the earlier stage is completed and approved. Each stage requires a separately reviewed staged specification and implementation plan before implementation begins.

Existing standalone paid-exam, free-exam, and grant-authorized exam session behavior must be preserved throughout this workstream.

## Specification Phase

- [x] Umbrella architecture-decisions draft created
- [x] Umbrella architecture-decisions specification reviewed and approved
- [ ] Stage 1 — Content, Package Catalog, and Reporting Profile staged specification
- [ ] Stage 2 — Commerce, Fulfillment, Entitlements, and Benefit Rights staged specification
- [ ] Stage 3 — Package-Attempt Authorization, Session Provenance, Concurrency, and Legacy Compatibility staged specification
- [ ] Stage 4 — Analytical-Report Generation and Access staged specification

## Stage 1 — Content, Package Catalog, and Reporting Profile

- [ ] Stage 1 staged specification reviewed and approved
- [ ] Stage 1 implementation plan reviewed and approved
- [ ] Reporting-topic taxonomy authoring/publication rules: every Reporting Topic belongs to exactly one existing ExamCategory and inherits country scope through it; no global, exam-specific, cross-category, separate-skill, or separate-difficulty taxonomy in v1; difficulty-based report analysis remains outside v1
- [ ] Separate immutable reporting-profile publication bound to one exact published exam version
- [ ] Managed study-material authoring and immutable published versions (including material-version-to-Reporting-Topic mapping)
- [ ] Reusable Practice Collection authoring and immutable published versions (including practice-item-to-Reporting-Topic mapping)
- [ ] Practice Items remain logically distinct from ExamQuestion and are selected, published, accessed, and protected independently, even if lower-level authoring infrastructure is later reused
- [ ] Basic practice progress distinguishes unanswered from answered items and, for answered items, correct from incorrect; concrete persistence and counters remain deferred to Stage 1 design
- [ ] Practice retry/retraining allowed only while package access is active; retries never consume the package exam attempt; after expiry no further practice authorization is granted while historical progress may remain visible
- [ ] Practice runtime and package-publication isolation prevent practice content from reading, exposing, or revealing the included published Exam Version's protected question content, answer identifiers, options, explanations, rationales, answer keys, or snapshots
- [ ] Practice administration and material administration use dedicated permissions separate from existing exam-content permissions; concrete permission names remain deferred to Stage 1 design
- [ ] V1 material types: File, External link, Video, and Formatted text
- [ ] Material lifecycle: Draft versions editable; Published versions immutable; revisions create new versions; Retired versions excluded from newly published Package Versions and newly sellable offers while historical purchasers remain protected
- [ ] Material authorization ends with the package access window; no offline access in v1; storage, delivery, download, and file-provider policy remain deferred to Stage 1 design
- [ ] Draft material content is inaccessible to nurses and cannot be included in package publication; only published material versions and published Practice Collection versions may be included in a published Package Version
- [ ] Materials support managed metadata, publication visibility, ordering, and reuse; concrete metadata fields and visibility representation remain deferred to Stage 1 design
- [ ] Package definition, immutable package version, and offer lifecycle
- [ ] Package Version composition administration selects, validates, and freezes one exact published Exam Version, one compatible immutable reporting-profile publication, one ordered list of exact published material versions, and one exact published Practice Collection version
- [ ] Offer administration is separate from composition administration: each offer references one already-published Package Version and configures only that version, price, currency, and access duration; the offer does not independently select component content or Reporting Topics
- [ ] Package publication validation: reporting-topic taxonomy and reporting-profile publication are prerequisites for package-version publication, plus referenced exam version, material versions, and Practice Collection version; the Reporting Topic set is inherited from the reporting-profile publication
- [ ] Existing published Exam Versions opt into package eligibility only through a compatible published reporting profile plus the remaining publication requirements; no mandatory mutation or historical backfill; versions without a profile retain standalone use but are not package-eligible
- [ ] Retirement and ineligibility make dependent offers non-purchasable for new sales without mutating immutable Package Versions, purchased snapshots, entitlements, reports, or historically purchased composition; historical access follows the original entitlement window and benefit rules
- [ ] Package catalog administration (dedicated package permissions)
- [ ] Package catalog APIs
- [ ] Stage 1 migration
- [ ] Stage 1 tests

## Stage 2 — Commerce, Fulfillment, Entitlements, and Benefit Rights

- [ ] Stage 2 staged specification reviewed and approved
- [ ] Stage 2 implementation plan reviewed and approved
- [ ] Package offer snapshotting
- [ ] Payment completion for package offers
- [ ] Idempotent package fulfillment (paid order item to package purchase entitlement)
- [ ] Four benefit rights (materials, practice, exam attempt, report) with independent authorization
- [ ] Benefit-right authorization hooks
- [ ] Stage 2 migration
- [ ] Stage 2 tests

## Stage 3 — Package-Attempt Authorization, Session Provenance, Concurrency, Legacy Compatibility

- [ ] Stage 3 staged specification reviewed and approved
- [ ] Stage 3 implementation plan reviewed and approved
- [ ] Package-attempt start operation where the client selects the Package Purchase identity/entitlement and the backend resolves the corresponding internal attempt right; no internal right identifier is client-selected
- [ ] Atomic attempt consumption with qualifying session creation
- [ ] Package start validates ownership, exact exam and exam-version match, active package window at creation, unused attempt, and immutable purchase provenance
- [ ] Same-source retries return/resume the same qualifying in-progress session idempotently, including when that session consumed the attempt; an attempt consumed by a terminal session returns a deterministic consumed outcome
- [ ] One package access window; access begins immediately at successful fulfillment; only duration configurable
- [ ] Package expiration after successful session creation does not invalidate the in-progress session; resume, submit, and automatic finalization remain allowed and the session remains report-eligible
- [ ] Session provenance recording
- [ ] Every new session has one immutable logical source (free, standalone grant-authorized, or one specific Package Purchase attempt); source and provenance cannot be rewritten after creation
- [ ] Existing isFree/canStart semantics remain backward compatible for free and standalone access; package availability and package-attempt eligibility are separate additive capability information, and canStart never silently includes or consumes package rights
- [ ] One in-progress session per nurse per exam version (across access sources)
- [ ] Source-mismatch conflict semantics
- [ ] Standalone/free start never consumes a package attempt; no silent entitlement selection, source switching, or provenance rewriting
- [ ] `ExamAccessGrant` kept as standalone authorization evidence; not part of effective-paid classification
- [ ] Standalone exam access and package rights coexist independently (same exam may be sold standalone and in a package; neither blocks the other; a standalone session does not consume or qualify the package attempt or report right; different packages containing the same exam coexist; one package cannot satisfy another package's rights; active repurchase of the same stable package is blocked; repurchase after expiration creates a new access window, attempt, and report right)
- [ ] Free and standalone session start behavior preserved
- [ ] Stage 3 migration
- [ ] Stage 3 tests

## Stage 4 — Analytical-Report Generation and Access

- [ ] Stage 4 staged specification reviewed and approved
- [ ] Stage 4 implementation plan reviewed and approved
- [ ] Submitted and automatically finalized/scored package sessions qualify for the report; abandoned sessions do not qualify
- [ ] Report benefit right exists after fulfillment but remains dormant until a qualifying package session exists
- [ ] Immutable analytical-report snapshot captures package-purchase/session provenance, scored topic results, classification labels, and mapped purchased material/practice content
- [ ] Generation recovery and retry (report-generation failure does not invalidate scoring, does not permanently consume the report right, and does not require an exam retake)
- [ ] Insufficient topic evidence may be displayed but must not receive a confident weak/neutral/strong classification
- [ ] Report persistence after entitlement expiry (nurse-owned, remains readable)
- [ ] Nurse-owned report access only in v1; admin report access is deferred
- [ ] Deterministic guidance presentation restricted to the material versions and Practice Collection version included in the purchased Package Version (no question text, correct-answer identifiers, protected options, rationales, or per-question exam review exposed; deterministic, not an AI recommendation)
- [ ] Stage 4 migration
- [ ] Stage 4 tests

## Launch-Readiness Configuration

These items must exist before the first commercial package can be published. They are split into per-offer configuration (chosen at each offer's publication) and required-once launch-readiness inputs (apply once across the first examination market).

### Per-offer Configuration

Made by the business at each package offer's publication time:

- [ ] Price of the package offer
- [ ] Currency of the package offer
- [ ] Exact access duration of the package offer (the only configurable element of the access window; access always begins at successful fulfillment and runs for this configured duration; no first-use and no choice between fixed-length and rolling)
- [ ] Exact already-published Package Version; its component composition and inherited Reporting Topic set are already frozen and are not independently selected by the offer

### Required Before Publishing the First Commercial Package (Launch-Readiness Inputs)

Apply once across the first examination market. No values invented in this document:

- [ ] Initial Reporting Topics for the first examination market
- [ ] Minimum scored-question representation required before a topic can be classified confidently
- [ ] Weak / neutral / strong classification thresholds used by deterministic report guidance
- [ ] The specific eligible published Package Version for the first commercial offer; its exact composition already satisfies Package Version publication requirements

### Approved Business Invariants

- The first commercial package cannot be published until all four staged specifications and their implementation plans have been reviewed, the underlying capabilities implemented, and the launch-readiness inputs above exist.
- A package publication is a business action, not an engineering action. It uses already-approved capabilities.

---

# Development Rules

- Complete phases sequentially.
- Do not implement features outside the current phase.
- Keep documentation synchronized with implementation.
- Every completed feature must compile successfully.
- Run applicable tests before marking work as complete.
- No task is considered complete until its documentation is updated.
- Production quality is required for every implementation.
