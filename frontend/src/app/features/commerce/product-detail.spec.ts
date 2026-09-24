import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { routes } from '../../app.routes';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceProduct } from '../../core/api/commerce-payments-api';
import { ProductDetailScreen } from './product-detail';

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
  detail: CommerceProduct = product();
  error: unknown = undefined;
  calls: string[] = [];

  listProducts() {
    return throwError(() => ({ status: 500 }));
  }

  getProduct(productId: string) {
    this.calls.push(productId);
    if (this.error !== undefined) {
      return throwError(() => this.error);
    }
    return of(this.detail);
  }
}

async function setup(
  stub?: CommercePaymentsApiStub,
  params: Record<string, string> = { productId: 'product-1' },
): Promise<{
  fixture: ComponentFixture<ProductDetailScreen>;
  api: CommercePaymentsApiStub;
  navigateSpy: Mock;
}> {
  const api = stub ?? new CommercePaymentsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ProductDetailScreen],
    providers: [
      provideRouter([]),
      { provide: CommercePaymentsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap(params) } },
      },
      Router,
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ProductDetailScreen);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<ProductDetailScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ProductDetailScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<ProductDetailScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ProductDetail screen (T-FE-082)', () => {
  it('loads the product by route productId with facts and formatted price', async () => {
    const { fixture, api } = await setup();

    expect(api.calls).toEqual(['product-1']);
    expect(byTestId(fixture, 'product-detail-heading')?.textContent).toContain('NCLEX Mock Exam');
    expect(text(fixture)).toContain('A full mock exam.');
    expect(text(fixture)).toContain('NCLEX Readiness');
    expect(text(fixture)).toContain('$49.99');
  });

  it('omits the description block when absent and offers Purchase for active products', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.detail = product({ description: null });
    const { fixture, navigateSpy } = await setup(stub);

    expect(byTestId(fixture, 'product-detail-description')).toBeNull();
    const purchase = byTestId(fixture, 'product-detail-purchase') as HTMLButtonElement | null;
    expect(purchase?.textContent).toContain('Purchase');

    purchase?.click();
    expect(navigateSpy).toHaveBeenCalledWith(['/checkout'], {
      queryParams: { productId: 'product-1' },
    });
  });

  it('shows a factual unavailable state for inactive products without actions', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.detail = product({ isActive: false });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'product-detail-unavailable')?.textContent).toContain('Unavailable');
    expect(text(fixture)).not.toMatch(/Purchase|Pay now/i);
  });

  it('shows a privacy-safe unavailable state on 404 without raw ids', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.error = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'product-detail-unavailable')?.textContent).toContain(
      'no longer available',
    );
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
    expect(text(fixture)).not.toContain('product-1');
  });

  it('retries a generic failure with the same route productId', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.error = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load this product. Try again.");

    stub.error = undefined;
    const retry = (fixture.nativeElement as HTMLElement).querySelector(
      '.np-loading-error-retry-retry',
    ) as HTMLButtonElement | null;
    retry?.click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls).toEqual(['product-1', 'product-1']);
    expect(byTestId(fixture, 'product-detail-heading')).not.toBeNull();
  });

  it('links Back to products without browser-history dependence', async () => {
    const { fixture } = await setup();
    const back = byTestId(fixture, 'product-detail-back') as HTMLAnchorElement | null;

    expect(back?.textContent).toContain('Back to products');
    expect(back?.getAttribute('href')).toBe('/commerce/products');
  });

  it('exposes no raw ids, entitlement, package, or provider content', async () => {
    const { fixture } = await setup();
    const body = text(fixture);

    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/entitled|practice|package report|report|card|CVV|billing|provider|checkout/i);
  });
});

describe('Commerce product detail route', () => {
  it('mounts /commerce/products/:productId with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'commerce/products/:productId');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'COMMERCE_PRODUCT_DETAIL' });
  });

  it('registers static commerce routes where dynamic shadowing cannot occur', () => {
    const paths = routes.map((entry) => entry.path);

    expect(paths).toContain('commerce/products');
    expect(paths).toContain('commerce/products/:productId');
  });
});
