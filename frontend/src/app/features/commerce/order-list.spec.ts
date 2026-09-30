import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder, CommerceOrderPage } from '../../core/api/commerce-payments-api';
import { OrderListScreen } from './order-list';

const order: CommerceOrder = {
  id: '52bc1507-7c14-43b4-b23c-a23cc1a8930d', status: 'PendingPayment', currency: 'USD',
  totalAmountMinor: '4999', createdAt: '2026-02-01T00:00:00Z',
  items: [{ title: 'NCLEX Mock Exam', currency: 'USD', lineTotalAmountMinor: '4999' }],
};

class OrdersStub {
  page: CommerceOrderPage = { items: [order], page: 1, pageSize: 20, totalCount: 21, totalPages: 2 };
  error: unknown;
  calls: number[] = [];
  listOrders(page: number) {
    this.calls.push(page);
    return this.error === undefined ? of({ ...this.page, page }) : throwError(() => this.error);
  }
}

async function setup(api = new OrdersStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [OrderListScreen],
    providers: [provideRouter([]), { provide: CommercePaymentsApi, useValue: api }],
  }).compileComponents();
  const fixture = TestBed.createComponent(OrderListScreen);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<OrderListScreen>) {
  await fixture.whenStable();
  fixture.detectChanges();
}

function root(fixture: ComponentFixture<OrderListScreen>): HTMLElement {
  return fixture.nativeElement as HTMLElement;
}

describe('COM-007 order history', () => {
  it('renders backend order and paging facts with a canonical View order link and no filters', async () => {
    const { fixture, api } = await setup();
    const view = root(fixture);
    expect(api.calls).toEqual([1]);
    expect(view.querySelector('h1')?.textContent).toBe('Orders');
    expect(view.textContent).toContain('NCLEX Mock Exam');
    expect(view.textContent).toContain('$49.99');
    expect(view.textContent).toContain('Pending payment');
    expect(view.querySelector<HTMLAnchorElement>('[data-testid="order-view"]')?.getAttribute('href'))
      .toBe('/commerce/orders/52bc1507-7c14-43b4-b23c-a23cc1a8930d');
    expect(view.textContent).not.toContain(order.id);
    expect(view.querySelector('input, select')).toBeNull();
    expect(view.textContent).not.toMatch(/sandbox|provider|card number/i);
    const next = view.querySelector<HTMLButtonElement>('[data-testid="pagination-next"]');
    next?.click();
    await settle(fixture);
    expect(api.calls).toEqual([1, 2]);
  });

  it('renders approved empty copy and Browse products action', async () => {
    const api = new OrdersStub();
    api.page = { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 };
    const { fixture } = await setup(api);
    const view = root(fixture);
    expect(view.textContent).toContain('No orders yet');
    expect(view.textContent).toContain('Your orders will appear here after you create one.');
    expect(view.querySelector<HTMLAnchorElement>('[data-testid="browse-products"]')?.getAttribute('href'))
      .toBe('/commerce/products');
  });

  it('shows privacy-safe error and retries the same page', async () => {
    const api = new OrdersStub();
    api.error = { status: 500, error: { title: 'Secret server detail' } };
    const { fixture } = await setup(api);
    expect(root(fixture).textContent).toContain("We couldn't load your orders. Try again.");
    expect(root(fixture).textContent).not.toContain('Secret server detail');
    api.error = undefined;
    root(fixture).querySelector<HTMLButtonElement>('.np-loading-error-retry-retry')?.click();
    await settle(fixture);
    expect(api.calls).toEqual([1, 1]);
  });

  it('mounts only the canonical history route behind all three guards', () => {
    const route = routes.find((entry) => entry.path === 'commerce/orders');
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'COMMERCE_ORDERS' });
  });
});
