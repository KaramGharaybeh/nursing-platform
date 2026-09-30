import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { routes } from '../../app.routes';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceProduct } from '../../core/api/commerce-payments-api';
import { ProductListScreen } from './product-list';

function product(overrides: Partial<CommerceProduct> = {}): CommerceProduct {
  return {
    id: 'product-1',
    name: 'NCLEX Mock Exam',
    description: 'A full mock exam.',
    examTitle: 'NCLEX Readiness',
    currency: 'USD',
    unitAmountMinor: '4999',
    isActive: true,
    ...overrides,
  };
}

class CommercePaymentsApiStub {
  items: CommerceProduct[] = [
    product(),
    product({ id: 'product-2', name: 'Second product', description: null }),
  ];
  error: unknown = undefined;
  calls: { page: number }[] = [];

  listProducts(page: number) {
    this.calls.push({ page });
    if (this.error !== undefined) {
      return throwError(() => this.error);
    }
    return of({ items: this.items, page, pageSize: 20, totalCount: 2, totalPages: 1 });
  }

  getProduct() {
    return throwError(() => ({ status: 404 }));
  }
}

async function setup(
  stub?: CommercePaymentsApiStub,
): Promise<{
  fixture: ComponentFixture<ProductListScreen>;
  api: CommercePaymentsApiStub;
  navigateSpy: Mock;
}> {
  const api = stub ?? new CommercePaymentsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ProductListScreen],
    providers: [
      provideRouter([]),
      { provide: CommercePaymentsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({}) } },
      },
      Router,
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ProductListScreen);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<ProductListScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ProductListScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<ProductListScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

function allByTestId(fixture: ComponentFixture<ProductListScreen>, id: string): HTMLElement[] {
  return [
    ...(fixture.nativeElement as HTMLElement).querySelectorAll(
      `[data-testid="${id}"]`,
    ),
  ] as HTMLElement[];
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ProductList screen (T-FE-082)', () => {
  it('loads products in backend order with heading, names, and formatted prices', async () => {
    const { fixture, api } = await setup();

    expect(api.calls).toEqual([{ page: 1 }]);
    expect(byTestId(fixture, 'product-list-heading')?.textContent).toContain('Products');
    const rows = allByTestId(fixture, 'product-row');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('NCLEX Mock Exam');
    expect(rows[0].textContent).toContain('A full mock exam.');
    expect(rows[0].textContent).toContain('$49.99');
    expect(rows[1].textContent).toContain('Second product');
  });

  it('links each row to View details with the canonical href and no purchase action', async () => {
    const { fixture } = await setup();
    const link = byTestId(fixture, 'product-details-link') as HTMLAnchorElement | null;

    expect(link?.textContent).toContain('View details');
    expect(link?.getAttribute('href')).toBe('/commerce/products/product-1');
    expect(text(fixture)).not.toMatch(/Purchase|Pay now|Checkout/i);
  });

  it('reloads only the list on page change', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      loadProductsPage(page: number): void;
    };
    const before = api.calls.length;

    component.loadProductsPage(2);
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls.slice(before)).toEqual([{ page: 2 }]);
  });

  it('shows the approved empty state for zero products', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.items = [];
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'product-list-empty')?.textContent).toContain('No products available');
    expect(text(fixture)).toContain('There are no products available to purchase right now.');
  });

  it('shows the approved error copy with same-page retry', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.error = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load products. Try again.");

    stub.error = undefined;
    const retry = (fixture.nativeElement as HTMLElement).querySelector(
      '.np-loading-error-retry-retry',
    ) as HTMLButtonElement | null;
    retry?.click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls).toEqual([{ page: 1 }, { page: 1 }]);
  });

  it('exposes no raw ids, provider UI, or order side effects', async () => {
    const { fixture, api } = await setup();
    const body = text(fixture);

    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/card|CVV|billing|provider|checkout/i);
    expect(api.calls.every((call) => Object.keys(call).every((key) => key === 'page'))).toBe(
      true,
    );
  });
});

describe('Commerce product list route', () => {
  it('mounts /commerce/products with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'commerce/products');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'COMMERCE_PRODUCTS' });
  });
});
