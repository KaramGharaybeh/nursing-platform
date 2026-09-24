import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { routes } from '../../app.routes';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder, CommerceProduct } from '../../core/api/commerce-payments-api';
import { CheckoutScreen } from './checkout';

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

function order(overrides: Partial<CommerceOrder> = {}): CommerceOrder {
  return {
    id: 'order-1',
    status: 'PendingPayment',
    currency: 'USD',
    totalAmountMinor: '4999',
    createdAt: '2026-02-01T00:00:00Z',
    items: [{ title: 'NCLEX Mock Exam', currency: 'USD', lineTotalAmountMinor: '4999' }],
    ...overrides,
  };
}

class CommercePaymentsApiStub {
  product: CommerceProduct = product();
  productError: unknown = undefined;
  created: CommerceOrder = order();
  createError: unknown = undefined;
  getCalls: string[] = [];
  createCalls: { productId?: string; packageOfferId?: string }[] = [];

  getProduct(productId: string) {
    this.getCalls.push(productId);
    if (this.productError !== undefined) {
      return throwError(() => this.productError);
    }
    return of(this.product);
  }

  createOrder(source: { productId?: string; packageOfferId?: string }) {
    this.createCalls.push(source);
    if (this.createError !== undefined) {
      return throwError(() => this.createError);
    }
    return of(this.created);
  }
}

async function setup(
  stub?: CommercePaymentsApiStub,
  query: Record<string, string> = { productId: 'product-1' },
): Promise<{
  fixture: ComponentFixture<CheckoutScreen>;
  api: CommercePaymentsApiStub;
  navigateSpy: Mock;
}> {
  const api = stub ?? new CommercePaymentsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [CheckoutScreen],
    providers: [
      provideRouter([]),
      { provide: CommercePaymentsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { queryParamMap: convertToParamMap(query) } },
      },
      Router,
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(CheckoutScreen);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<CheckoutScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<CheckoutScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<CheckoutScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

function clickByTestId(fixture: ComponentFixture<CheckoutScreen>, id: string): void {
  const button = byTestId(fixture, id) as HTMLButtonElement | null;
  if (button === null) {
    throw new Error(`Missing actionable element "${id}".`);
  }
  button.click();
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('Checkout screen (T-FE-084)', () => {
  it('loads the product context and shows the explicit order confirmation', async () => {
    const { fixture, api } = await setup();

    expect(api.getCalls).toEqual(['product-1']);
    expect(api.createCalls).toEqual([]);
    expect(byTestId(fixture, 'checkout-heading')?.textContent).toContain('Create order?');
    expect(text(fixture)).toContain('NCLEX Mock Exam');
    expect(text(fixture)).toContain('$49.99');
    expect(text(fixture)).toContain('An order will be created for this product.');
    expect(byTestId(fixture, 'checkout-create')).not.toBeNull();
    expect(byTestId(fixture, 'checkout-cancel')).not.toBeNull();
  });

  it('shows a no-context state without backend calls when no product is selected', async () => {
    const { fixture, api } = await setup(new CommercePaymentsApiStub(), {});

    expect(api.getCalls).toEqual([]);
    expect(api.createCalls).toEqual([]);
    expect(byTestId(fixture, 'checkout-no-context')).not.toBeNull();
    const back = byTestId(fixture, 'checkout-back') as HTMLAnchorElement | null;
    expect(back?.getAttribute('href')).toBe('/commerce/products');
  });

  it('shows a factual unavailable state when the product no longer exists', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.productError = { status: 404 };
    const { fixture, api } = await setup(stub);

    expect(api.createCalls).toEqual([]);
    expect(byTestId(fixture, 'checkout-unavailable')?.textContent).toContain(
      'no longer available',
    );
    expect(text(fixture)).not.toContain('product-1');
  });

  it('creates exactly one order on confirm and shows the created order facts', async () => {
    const { fixture, api } = await setup();

    clickByTestId(fixture, 'checkout-create');
    clickByTestId(fixture, 'checkout-create');
    await settle(fixture);

    expect(api.createCalls).toEqual([{ productId: 'product-1' }]);
    expect(byTestId(fixture, 'checkout-success-heading')?.textContent).toContain(
      'Order created.',
    );
    expect(text(fixture)).toContain('Pending payment');
    expect(text(fixture)).toContain('$49.99');
    expect(text(fixture)).not.toMatch(/View order|checkout\/orders/i);
  });

  it('maps an order conflict to a factual purchasability message', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.createError = { status: 409 };
    const { fixture } = await setup(stub);

    clickByTestId(fixture, 'checkout-create');
    await settle(fixture);

    expect(byTestId(fixture, 'checkout-create-error')?.textContent).toContain(
      'This product can no longer be ordered.',
    );
    expect(byTestId(fixture, 'checkout-success-heading')).toBeNull();
  });

  it('retries a generic creation failure only on explicit retry', async () => {
    const stub = new CommercePaymentsApiStub();
    stub.createError = { status: 500 };
    const { fixture, api } = await setup(stub);

    clickByTestId(fixture, 'checkout-create');
    await settle(fixture);
    expect(api.createCalls.length).toBe(1);
    expect(byTestId(fixture, 'checkout-create-error')?.textContent).toContain(
      "We couldn't create this order. Try again.",
    );

    stub.createError = undefined;
    clickByTestId(fixture, 'checkout-create-retry');
    await settle(fixture);

    expect(api.createCalls.length).toBe(2);
    expect(byTestId(fixture, 'checkout-success-heading')).not.toBeNull();
  });

  it('cancels back to the originating product detail without creating an order', async () => {
    const { fixture, api, navigateSpy } = await setup();

    clickByTestId(fixture, 'checkout-cancel');
    await settle(fixture);

    expect(api.createCalls).toEqual([]);
    expect(navigateSpy).toHaveBeenCalledWith('/commerce/products/product-1');
  });

  it('exposes no raw ids, provider, or entitlement content', async () => {
    const { fixture } = await setup();
    const body = text(fixture);

    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/entitled|practice|package report|card|CVV|billing|provider/i);
  });
});

describe('Commerce checkout route', () => {
  it('mounts /checkout with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'checkout');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'COMMERCE_CHECKOUT' });
  });
});
