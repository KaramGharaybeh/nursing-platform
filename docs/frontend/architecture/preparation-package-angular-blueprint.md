# Angular Implementation Blueprint — Preparation Package Screens

## 0. Metadata

```yaml
document_id: NPS-DES-BLUEPRINT-PP-001
version: 1.0
status: draft
recorded_at: 2026-08-16
page_spec_source: NPS-DES-SPEC-PP-001
route_matrix_source: NPS-DES-SPEC-PP-002
evidence_packet_source: preparation-package-phase-1-evidence-packet.md
architecture_source: frontend-architecture.md
```

## 1. Preconditions & Authority

- This blueprint is documentation-only. It does not authorize Angular scaffolding, code creation, or file modification.
- All component, service, guard, and routing decisions below derive from the committed OpenAPI contract (`development-openapi-2026-08-16.json`), page specs (`NPS-DES-SPEC-PP-001`), and route matrix (`NPS-DES-SPEC-PP-002`).
- The Angular frontend does not yet exist. The `frontend/` directory is empty. All decisions reference the approved `frontend-architecture.md` and `frontend-project-rules.md` as the implementation authority.
- Pre-implementation gates from `frontend-project-rules.md` §2 remain unsatisfied. Angular code may not begin until all gates are met.

---

## 2. Feature Boundary

```
src/app/features/preparation-package/
├── routes/
│   └── preparation-package.routes.ts
├── pages/
│   ├── offers-list/
│   │   ├── offers-list-page.component.ts
│   │   ├── offers-list-page.component.html
│   │   ├── offers-list-page.component.scss
│   │   └── offers-list-page.component.spec.ts
│   ├── offer-detail/
│   │   ├── offer-detail-page.component.ts
│   │   ├── offer-detail-page.component.html
│   │   ├── offer-detail-page.component.scss
│   │   └── offer-detail-page.component.spec.ts
│   └── checkout-order/
│       ├── checkout-order-page.component.ts
│       ├── checkout-order-page.component.html
│       ├── checkout-order-page.component.scss
│       └── checkout-order-page.component.spec.ts
├── components/
│   ├── offer-card/
│   │   ├── offer-card.component.ts
│   │   ├── offer-card.component.html
│   │   ├── offer-card.component.scss
│   │   └── offer-card.component.spec.ts
│   ├── offer-filters/
│   │   ├── offer-filters.component.ts
│   │   ├── offer-filters.component.html
│   │   ├── offer-filters.component.scss
│   │   └── offer-filters.component.spec.ts
│   ├── package-component-list/
│   │   ├── package-component-list.component.ts
│   │   ├── package-component-list.component.html
│   │   ├── package-component-list.component.scss
│   │   └── package-component-list.component.spec.ts
│   ├── order-summary-card/
│   │   ├── order-summary-card.component.ts
│   │   ├── order-summary-card.component.html
│   │   ├── order-summary-card.component.scss
│   │   └── order-summary-card.component.spec.ts
│   └── price-display/
│       ├── price-display.component.ts
│       ├── price-display.component.html
│       ├── price-display.component.scss
│       └── price-display.component.spec.ts
├── services/
│   ├── preparation-package-api.service.ts
│   ├── preparation-package-api.service.spec.ts
│   ├── preparation-package-state.service.ts
│   └── preparation-package-state.service.spec.ts
├── models/
│   ├── preparation-package-offer-list-item.model.ts
│   ├── preparation-package-offer-detail.model.ts
│   ├── preparation-package-paginated-result.model.ts
│   ├── payment-order.model.ts
│   └── preparation-package-filters.model.ts
└── index.ts
```

---

## 3. Component Tree & Standalone Components Hierarchy

### 3.1 Screen: PP-OFFERS-LIST (`/preparation-packages/offers`)

```
AppShell
└── OffersListPageComponent              ← route: lazy-loaded
    ├── OfferFiltersComponent            ← child: filter inputs
    │   ├── CountryFilter (MatSelect)    ← inline or child
    │   └── ExamCategoryFilter (MatSelect)
    ├── @if (loading)
    │   └── MatProgressBar (indeterminate)
    ├── @if (offers().length === 0 && !loading())
    │   └── EmptyState                   ← inline or shared component
    ├── @for (offer of offers(); track offer.id)
    │   └── OfferCardComponent           ← child: reusable card
    │       ├── PriceDisplayComponent    ← shared: formatted price
    │       └── <a [routerLink]="['/preparation-packages/offers', offer.slug]"
    │              routerLinkActive="active">View Details</a>
    └── MatPaginator                     ← pagination controls
```

**Component responsibilities:**

| Component | Owner | Inputs | Outputs | Notes |
|-----------|-------|--------|---------|-------|
| `OffersListPageComponent` | feature | — | — | Page shell; owns filter state, pagination state, data fetching |
| `OfferFiltersComponent` | feature | `countries`, `examCategories`, `selectedFilters` | `filtersChange` | Filter dropdowns; emits updated filter signal |
| `OfferCardComponent` | feature | `offer: PreparationPackageOfferListItemDto` | — | Card display; navigation link via `routerLink` |
| `PriceDisplayComponent` | shared | `amountMinor: string`, `currency: string` | — | Formats minor-unit string to display currency using `Intl.NumberFormat` |

### 3.2 Screen: PP-OFFER-DETAIL (`/preparation-packages/offers/{slug}`)

```
AppShell
└── OfferDetailPageComponent             ← route: lazy-loaded, resolves :slug
    ├── @if (loading)
    │   └── MatProgressBar (indeterminate)
    ├── @if (offer(); as offer)
    │   ├── MatCard                      ← main offer card
    │   │   ├── Title + Summary
    │   │   ├── PriceDisplayComponent    ← shared
    │   │   ├── Access Duration
    │   │   └── Material/Practice counts
    │   ├── PackageComponentListComponent ← child: component breakdown
    │   │   └── @for (comp of offer.components; track comp.name)
    │   │       └── MatListItem           ← component name + count
    │   └── <button (click)="initiatePurchase()"
    │          routerLink="/me/nurse-profile/payment/orders"
    │          [queryParams]="{ packageOfferId: offer.id }">
    │          Purchase
    │        </button>
    └── @if (error status === 404)
        └── NotFoundPage                  ← shared route or component
```

**Component responsibilities:**

| Component | Owner | Inputs | Outputs | Notes |
|-----------|-------|--------|---------|-------|
| `OfferDetailPageComponent` | feature | — | — | Page shell; resolves `slug` from route params; owns fetch state |
| `PackageComponentListComponent` | feature | `components: PreparationPackageCatalogComponentSummaryDto[]` | — | Renders component breakdown list |
| `PriceDisplayComponent` | shared | `amountMinor`, `currency` | — | Reused from offers list |

### 3.3 Screen: PP-CHECKOUT-ORDER (`/me/nurse-profile/payment/orders`)

```
AppShell
└── CheckoutOrderPageComponent           ← route: lazy-loaded, auth-guarded
    ├── @if (loading)
    │   └── MatProgressBar (indeterminate)
    ├── @if (orderCreated(); as order)
    │   ├── OrderSummaryCardComponent    ← child: order confirmation
    │   │   ├── Order Status
    │   │   ├── Order Items (table or list)
    │   │   ├── Total Amount (PriceDisplayComponent)
    │   │   └── Expiration (if present)
    │   └── Navigation actions
    ├── @if (error)
    │   ├── @if (errorStatus === 401)
    │   │   └── Redirect to login
    │   ├── @if (errorStatus === 404)
    │   │   └── Offer not found message
    │   ├── @if (errorStatus === 409)
    │   │   └── Conflict message (pending order exists)
    │   └── @else
    │       └── Generic error with retry
    └── Confirm Order Button (MatButton) ← triggers POST
```

**Component responsibilities:**

| Component | Owner | Inputs | Outputs | Notes |
|-----------|-------|--------|---------|-------|
| `CheckoutOrderPageComponent` | feature | — | — | Page shell; reads `packageOfferId` from query params; owns order creation state |
| `OrderSummaryCardComponent` | feature | `order: PaymentOrderDto` | — | Displays created order summary |

---

## 4. Services & State Management

### 4.1 API Service: `PreparationPackageApiService`

**Location:** `src/app/features/preparation-package/services/preparation-package-api.service.ts`

**Responsibility:** Typed HTTP calls to backend endpoints. No business logic. No state ownership.

```typescript
@Injectable({ providedIn: 'root' })
export class PreparationPackageApiService {
  private readonly http = inject(HttpClient);

  // GET /api/v1/preparation-packages/offers
  listOffers(params: OfferListQueryParams): Observable<PaginatedResult<PreparationPackageOfferListItemDto>> { ... }

  // GET /api/v1/preparation-packages/offers/{slug}
  getOfferDetail(slug: string): Observable<PreparationPackageOfferDetailDto> { ... }

  // POST /api/v1/me/nurse-profile/payment/orders
  createPaymentOrder(body: CreatePaymentOrderRequest): Observable<PaymentOrderDto> { ... }
}
```

**Key patterns:**
- Uses `inject(HttpClient)` per `frontend-architecture.md` §4
- Returns `Observable` — no signal wrapping at service level
- API paths are relative (`/api/v1/...`) per `frontend-architecture.md` §API Integration
- No caching without explicit invalidation rules
- Error handling deferred to callers (Problem Details mapping via interceptor)

### 4.2 State Service: `PreparationPackageStateService`

**Location:** `src/app/features/preparation-package/services/preparation-package-state.service.ts`

**Responsibility:** Feature-scoped state using Angular Signals. Owns filter state, pagination state, loading/error states, and data signals.

```typescript
@Injectable()
export class PreparationPackageStateService {
  private readonly api = inject(PreparationPackageApiService);

  // --- Filter State (Signals) ---
  readonly selectedCountryId = signal<string | null>(null);
  readonly selectedExamCategoryId = signal<string | null>(null);
  readonly currentPage = signal<number>(1);
  readonly pageSize = signal<number>(10);

  // --- Derived Filter Params (computed) ---
  readonly queryParams = computed(() => ({
    page: this.currentPage(),
    pageSize: this.pageSize(),
    countryId: this.selectedCountryId(),
    examCategoryId: this.selectedExamCategoryId(),
  }));

  // --- Data State (Signals) ---
  readonly offers = signal<PreparationPackageOfferListItemDto[]>([]);
  readonly totalCount = signal<number>(0);
  readonly totalPages = signal<number>(0);
  readonly isLoading = signal<boolean>(false);
  readonly error = signal<ProblemDetails | null>(null);

  // --- Actions ---
  loadOffers(): void { ... }
  resetFilters(): void { ... }
}
```

**Key patterns:**
- Signals for all synchronous state (per `frontend-architecture.md` §State Management)
- `computed()` for derived values (pure transformations)
- No global store (no NgRx) — feature state only (per architecture rules)
- RxJS `Observable` from API service bridged to signals via subscription in the page component
- State service is `@Injectable()` (not `providedIn: 'root'`) — scoped to feature

### 4.3 Data Flow Summary

```
Route Activation (slug or query params)
       │
       ▼
Page Component (reads route params/signals)
       │
       ▼
PreparationPackageStateService (owns filter + pagination signals)
       │
       ▼
PreparationPackageApiService (HTTP calls → Observable)
       │
       ▼
Angular HTTP Interceptor Chain (auth header, refresh, Problem Details)
       │
       ▼
Backend API (/api/v1/...)
```

### 4.4 Models

| Model | Location | Source |
|-------|----------|--------|
| `PreparationPackageOfferListItemDto` | `models/preparation-package-offer-list-item.model.ts` | OpenAPI `PreparationPackageOfferListItemDto` |
| `PreparationPackageOfferDetailDto` | `models/preparation-package-offer-detail.model.ts` | OpenAPI `PreparationPackageOfferDetailDto` |
| `PaginatedResult<T>` | `models/preparation-package-paginated-result.model.ts` | OpenAPI `PaginatedResult` generic wrapper |
| `PaymentOrderDto` | `models/payment-order.model.ts` | OpenAPI `PaymentOrderDto` |
| `OfferListQueryParams` | `models/preparation-package-filters.model.ts` | Page spec query parameters |
| `CreatePaymentOrderRequest` | `models/payment-order.model.ts` | OpenAPI request body |

**CST-003 note:** All monetary amounts (`priceAmountMinor`, `totalAmountMinor`, `unitAmountMinor`, `lineTotalAmountMinor`) are `string` in the TypeScript models. `PriceDisplayComponent` parses them for display. No `number` conversion for financial logic.

---

## 5. Route Guards

### 5.1 Guard Design

| Guard | Location | Purpose | Auth Level |
|-------|----------|---------|------------|
| `authGuard` | `src/app/core/guards/auth.guard.ts` | Redirects unauthenticated users to login | Bearer JWT |
| `anonymousGuard` | `src/app/core/guards/anonymous.guard.ts` | Redirects authenticated users away from public-only pages | No JWT |
| `authBootstrapGuard` | `src/app/core/guards/auth-bootstrap.guard.ts` | Waits for auth bootstrap to complete before route activation | — |

### 5.2 Route-to-Guard Mapping

| Route Path | Stable ID | Guards Required |
|------------|-----------|-----------------|
| `/preparation-packages/offers` | `PP-OFFERS-LIST` | `authBootstrapGuard` only (anonymous access) |
| `/preparation-packages/offers/:slug` | `PP-OFFER-DETAIL` | `authBootstrapGuard` only (anonymous access) |
| `/me/nurse-profile/payment/orders` | `PP-CHECKOUT-ORDER` | `authBootstrapGuard` + `authGuard` (Bearer JWT required) |

### 5.3 Guard Behavior Specifications

**`authBootstrapGuard`:**
- Waits for authentication bootstrap state to resolve (`initializing` → `authenticated` | `anonymous`).
- MUST NOT redirect while bootstrap is in progress.
- MUST wait until anonymous bootstrap completes when no refresh token exists.
- If bootstrap fails with network/5xx error, resolves as bootstrap-unavailable (bounded recovery, not automatic logout).

**`authGuard`:**
- Checks if user is authenticated (after bootstrap).
- If not authenticated, redirects to `/login` with `returnUrl` query parameter preserving the original navigation target.
- Does NOT check permissions — `PP-CHECKOUT-ORDER` has no permission requirement.
- MUST wait for `authBootstrapGuard` to resolve first.

**`anonymousGuard`:**
- Not required for these three screens (they are not login/register pages).
- Included for completeness — future use if login/register routes need to redirect authenticated users.

### 5.4 Route Definition

```typescript
// src/app/features/preparation-package/routes/preparation-package.routes.ts
export const routes: Routes = [
  {
    path: 'offers',
    canActivate: [authBootstrapGuard],
    loadComponent: () => import('../pages/offers-list/offers-list-page.component')
      .then(m => m.OffersListPageComponent),
  },
  {
    path: 'offers/:slug',
    canActivate: [authBootstrapGuard],
    loadComponent: () => import('../pages/offer-detail/offer-detail-page.component')
      .then(m => m.OfferDetailPageComponent),
  },
  {
    path: 'payment/orders',
    canActivate: [authBootstrapGuard, authGuard],
    loadComponent: () => import('../pages/checkout-order/checkout-order-page.component')
      .then(m => m.CheckoutOrderPageComponent),
  },
];
```

**Root-level lazy loading:**

```typescript
// src/app/app.routes.ts (root)
{
  path: 'preparation-packages',
  loadChildren: () => import('./features/preparation-package/routes/preparation-package.routes')
    .then(m => m.routes),
},
{
  path: 'me/nurse-profile',
  loadChildren: () => import('./features/preparation-package/routes/preparation-package.routes')
    .then(m => m.routes.filter(r => r.path === 'payment/orders')),
},
```

---

## 6. Angular Patterns Summary

| Aspect | Decision | Source |
|--------|----------|--------|
| Component model | Standalone (Angular 22 default) | `frontend-architecture.md` §Platform Rules |
| `standalone: true` | Not set redundantly | Angular 22 default |
| `ChangeDetectionStrategy.OnPush` | Not set redundantly | Angular 22 zoneless default |
| Template control flow | `@if`, `@for`, `@switch` | `frontend-architecture.md` §Platform Rules |
| Signals | Primary state mechanism | `frontend-architecture.md` §State Management |
| `computed()` | Derived state | `frontend-architecture.md` §State Management |
| `effect()` | Used sparingly | `frontend-architecture.md` §State Management |
| DI | `inject()` not constructor injection | `frontend-architecture.md` §Platform Rules |
| HTTP | `HttpClient` via `inject()` | `frontend-architecture.md` §API Integration |
| Angular Material | Selective standalone imports | `frontend-architecture.md` §Material Usage |
| Styling | SCSS, colocated with component | `frontend-architecture.md` §SCSS Architecture |
| Testing | Vitest + Angular TestBed | `frontend-architecture.md` §Testing Principles |
| Forms | Signal Forms (default) or Reactive Forms (fallback) | `frontend-architecture.md` §Forms |
| Routing | Angular Router, lazy-loaded features | `frontend-architecture.md` §Routing |
| Zone.js | Not included (zoneless) | `frontend-architecture.md` §Zoneless Rules |

---

## 7. Error Handling & UI States

### 7.1 PP-OFFERS-LIST

| State | Condition | UI |
|-------|-----------|-----|
| Loading | `isLoading() === true` | Indeterminate `MatProgressBar` |
| Empty | `offers().length === 0 && !isLoading()` | Empty-state message |
| Success | `offers().length > 0` | Offer cards + `MatPaginator` |
| 400 | `error()?.status === 400` | Field-level errors from `ValidationProblemDetails.errors` |

### 7.2 PP-OFFER-DETAIL

| State | Condition | UI |
|-------|-----------|-----|
| Loading | `isLoading() === true` | Indeterminate `MatProgressBar` |
| 404 | `error()?.status === 404` | NotFoundPage or contextual not-found state |
| Success | `offer() !== null` | Full offer card + components + Purchase CTA |
| 400 | `error()?.status === 400` | Invalid slug message |

### 7.3 PP-CHECKOUT-ORDER

| State | Condition | UI |
|-------|-----------|-----|
| Loading | `isLoading() === true` | Indeterminate `MatProgressBar` |
| 401 | `error()?.status === 401` | Redirect to login (via `authGuard`) |
| 404 | `error()?.status === 404` | Offer not found message |
| 409 | `error()?.status === 409` | Conflict: "A pending order already exists" |
| Success (201) | `orderCreated() !== null` | Order summary card |

### 7.4 Monetary Display

- All monetary amounts are `string` in the API contract (CST-003).
- `PriceDisplayComponent` uses `Intl.NumberFormat` with server-provided `currency`.
- `parseInt(amountMinor, 10)` converts string to integer for formatting.
- No floating-point arithmetic for financial values.

---

## 8. Task Breakdown (Atomic Tasks)

### Phase A: Foundation (Prerequisite for all screen tasks)

| Task ID | Description | Dependencies | Verification |
|---------|-------------|--------------|--------------|
| A-01 | Create feature directory structure under `src/app/features/preparation-package/` | Angular scaffold complete | Directory exists |
| A-02 | Define TypeScript interfaces for all DTOs (`PreparationPackageOfferListItemDto`, `PreparationPackageOfferDetailDto`, `PaymentOrderDto`, `PaginatedResult<T>`, filter params, request bodies) | A-01 | Files compile, types match OpenAPI |
| A-03 | Implement `PreparationPackageApiService` with typed HTTP methods | A-02 | Service injectable, method signatures correct |
| A-04 | Implement `PreparationPackageStateService` with signal-based state | A-03 | State service injectable, signals initialize correctly |
| A-05 | Implement `PriceDisplayComponent` (shared) with `Intl.NumberFormat` | A-02 | Component renders formatted price, handles string amounts |

### Phase B: PP-OFFERS-LIST

| Task ID | Description | Dependencies | Verification |
|---------|-------------|--------------|--------------|
| B-01 | Implement `OfferFiltersComponent` with `MatSelect` for country and exam category | A-01 | Component renders, emits filter changes |
| B-02 | Implement `OfferCardComponent` with price display and navigation link | A-05 | Component renders offer data, link navigates correctly |
| B-03 | Implement `OffersListPageComponent` with data fetching, loading/empty/success states | A-03, A-04, B-01, B-02 | Page loads data, renders cards, handles pagination |
| B-04 | Implement `MatPaginator` integration with page/size changes | B-03 | Pagination triggers API calls, updates state |
| B-05 | Write unit tests for `OfferFiltersComponent` | B-01 | Tests pass |
| B-06 | Write unit tests for `OfferCardComponent` | B-02 | Tests pass |
| B-07 | Write unit tests for `OffersListPageComponent` (loading, empty, success, 400 states) | B-03 | Tests pass |
| B-08 | Write integration test for offers list data flow (filters → API → state → render) | B-03, B-04 | Test passes |

### Phase C: PP-OFFER-DETAIL

| Task ID | Description | Dependencies | Verification |
|---------|-------------|--------------|--------------|
| C-01 | Implement `PackageComponentListComponent` | A-01 | Component renders component list |
| C-02 | Implement `OfferDetailPageComponent` with slug resolution, data fetching, loading/success/404 states | A-03, A-04, C-01 | Page loads detail, renders components, handles 404 |
| C-03 | Implement Purchase CTA navigation to checkout page | C-02 | Button navigates with correct `packageOfferId` query param |
| C-04 | Write unit tests for `PackageComponentListComponent` | C-01 | Tests pass |
| C-05 | Write unit tests for `OfferDetailPageComponent` (loading, success, 404, 400 states) | C-02 | Tests pass |
| C-06 | Write integration test for offer detail data flow (route param → API → state → render) | C-02 | Test passes |

### Phase D: PP-CHECKOUT-ORDER

| Task ID | Description | Dependencies | Verification |
|---------|-------------|--------------|--------------|
| D-01 | Implement `OrderSummaryCardComponent` | A-05 | Component renders order details |
| D-02 | Implement `CheckoutOrderPageComponent` with query param resolution, order creation, loading/success/409/401/404 states | A-03, A-04, D-01 | Page creates order, handles all states |
| D-03 | Implement Confirm Order button with POST trigger | D-02 | Button triggers order creation, shows success/conflict |
| D-04 | Write unit tests for `OrderSummaryCardComponent` | D-01 | Tests pass |
| D-05 | Write unit tests for `CheckoutOrderPageComponent` (loading, 201, 401, 404, 409 states) | D-02 | Tests pass |
| D-06 | Write integration test for checkout flow (query param → POST → state → render) | D-02, D-03 | Test passes |

### Phase E: Routing & Guards

| Task ID | Description | Dependencies | Verification |
|---------|-------------|--------------|--------------|
| E-01 | Implement `authBootstrapGuard` (waits for bootstrap completion) | Core auth state exists | Guard blocks until bootstrap resolves |
| E-02 | Implement `authGuard` (redirects unauthenticated to login) | E-01 | Guard redirects correctly |
| E-03 | Define `preparation-package.routes.ts` with lazy-loaded routes and guards | E-01, E-02, B-03, C-02, D-02 | Routes resolve, guards execute |
| E-04 | Write unit tests for `authBootstrapGuard` | E-01 | Tests pass |
| E-05 | Write unit tests for `authGuard` | E-02 | Tests pass |
| E-06 | Write integration test for route navigation (anonymous → offers list → offer detail → login required → checkout) | E-03 | Test passes |

### Phase F: Verification & Polish

| Task ID | Description | Dependencies | Verification |
|---------|-------------|--------------|--------------|
| F-01 | Run full Vitest suite for preparation-package feature | All Phase B–E tasks | All tests pass |
| F-02 | Run Angular production build | All tasks | Build succeeds, no errors |
| F-03 | Verify `git status --short` shows only intended files | All tasks | Clean scope |
| F-04 | Accessibility audit (keyboard nav, focus states, ARIA labels, contrast) | All components | WCAG 2.2 AA compliance |

---

## 9. Dependency Graph

```
A-01 → A-02 → A-03 → A-04
                    ↓
                    A-05 → B-02, C-01, D-01
                              ↓
A-04 → B-01, C-02, D-02
         ↓       ↓       ↓
        B-03    C-03    D-03
         ↓       ↓       ↓
        B-04    —       —
         ↓
        E-01 → E-02 → E-03
                        ↓
                       F-01 → F-02 → F-03
```

---

## 10. Risks & Open Items

| Risk/Item | Impact | Mitigation |
|-----------|--------|------------|
| Angular scaffold not yet authorized | Blocks all tasks | Await pre-implementation gate approval |
| OpenAPI generator not approved | May affect API service implementation | Handwritten typed services approved as fallback |
| Design tokens not finalized | May affect SCSS/theming | Use neutral structural UI initially; apply tokens when approved |
| `authBootstrapGuard` depends on Core auth state | Core auth must exist first | Implement Core auth before Phase E |
| `PaymentOrderDto` items array structure | Complex nested DTO | Verify against OpenAPI during implementation |
| 409 Conflict handling for pending orders | UX decision needed | Show persistent conflict message, not snack bar |
| `packageOfferId` as UUID in query param | URL encoding considerations | Use standard query param encoding |

---

**End of Angular Implementation Blueprint.**
