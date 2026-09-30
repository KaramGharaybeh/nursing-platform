import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder } from '../../core/api/commerce-payments-api';
import { OrderDetailScreen } from './order-detail';

const order: CommerceOrder = {
  id: '52bc1507-7c14-43b4-b23c-a23cc1a8930d', status: 'PendingPayment', currency: 'USD',
  totalAmountMinor: '4999', createdAt: '2026-02-01T00:00:00Z',
  items: [{ title: 'NCLEX Mock Exam', currency: 'USD', lineTotalAmountMinor: '4999' }],
};

class OrderStub {
  current = order;
  getError: unknown;
  cancelError: unknown;
  reloadError: unknown;
  pendingCancel: Subject<CommerceOrder> | undefined;
  getCalls: string[] = [];
  cancelCalls: string[] = [];
  getOrder(id: string) {
    this.getCalls.push(id);
    return this.getError === undefined ? of(this.current) : throwError(() => this.getError);
  }
  cancelOrder(id: string) {
    this.cancelCalls.push(id);
    if (this.pendingCancel) return this.pendingCancel.asObservable();
    if (this.cancelError === undefined && this.reloadError !== undefined) this.getError = this.reloadError;
    return this.cancelError === undefined ? of({ ...order, status: 'Cancelled' as const })
      : throwError(() => this.cancelError);
  }
}

async function setup(api = new OrderStub(), id = order.id) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [OrderDetailScreen],
    providers: [
      provideRouter([]),
      { provide: CommercePaymentsApi, useValue: api },
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ orderId: id }) } } },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(OrderDetailScreen);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<OrderDetailScreen>) {
  await fixture.whenStable();
  fixture.detectChanges();
}

function view(fixture: ComponentFixture<OrderDetailScreen>): HTMLElement {
  return fixture.nativeElement as HTMLElement;
}

function button(fixture: ComponentFixture<OrderDetailScreen>, testid: string) {
  return view(fixture).querySelector<HTMLButtonElement>(`[data-testid="${testid}"]`);
}

describe('COM-008 order detail', () => {
  it('shows safe owned facts, a factual status, and canonical back navigation', async () => {
    const { fixture, api } = await setup();
    expect(api.getCalls).toEqual([order.id]);
    expect(view(fixture).querySelector('h1')?.textContent).toBe('Order details');
    expect(view(fixture).textContent).toContain('NCLEX Mock Exam');
    expect(view(fixture).textContent).toContain('$49.99');
    expect(view(fixture).textContent).toContain('Pending payment');
    expect(view(fixture).textContent).not.toContain(order.id);
    expect(view(fixture).querySelector<HTMLAnchorElement>('[data-testid="back-to-orders"]')?.getAttribute('href'))
      .toBe('/commerce/orders');
  });

  it('renders each additional item with formatted currency, never minor units or identifiers', async () => {
    const api = new OrderStub();
    api.current = { ...order, items: [
      order.items[0],
      { title: 'Practice bundle', currency: 'USD', lineTotalAmountMinor: '1299' },
    ] };
    const { fixture } = await setup(api);
    expect(view(fixture).textContent).toContain('Practice bundle');
    expect(view(fixture).textContent).toContain('$12.99');
    expect(view(fixture).textContent).not.toContain('1299');
  });

  it.each([
    ['Paid', 'Paid'], ['Failed', 'Failed'], ['Cancelled', 'Cancelled'], ['Expired', 'Expired'],
  ] as const)('renders %s as factual %s with no cancel action', async (status, label) => {
    const api = new OrderStub();
    api.current = { ...order, status, paidAt: status === 'Paid' ? '2026-02-02T00:00:00Z' : null };
    const { fixture } = await setup(api);
    expect(view(fixture).textContent).toContain(label);
    expect(button(fixture, 'cancel-order')).toBeNull();
    if (status === 'Paid') expect(view(fixture).textContent).toContain('Paid on');
  });

  it('confirms before cancellation, prevents duplicate submissions, and reloads authoritative state', async () => {
    const api = new OrderStub();
    api.pendingCancel = new Subject<CommerceOrder>();
    const { fixture } = await setup(api);
    button(fixture, 'cancel-order')?.click();
    await settle(fixture);
    expect(view(fixture).textContent).toContain('Cancel order?');
    expect(view(fixture).textContent).toContain('This order will be cancelled if it is still eligible for cancellation.');
    expect(api.cancelCalls).toEqual([]);
    button(fixture, 'keep-order')?.click();
    await settle(fixture);
    expect(api.cancelCalls).toEqual([]);
    button(fixture, 'cancel-order')?.click();
    await settle(fixture);
    button(fixture, 'confirm-cancel')?.click();
    button(fixture, 'confirm-cancel')?.click();
    expect(api.cancelCalls).toEqual([order.id]);
    expect(view(fixture).textContent).not.toContain('Order cancelled.');
    api.current = { ...order, status: 'Cancelled' };
    api.pendingCancel.next(api.current);
    api.pendingCancel.complete();
    await settle(fixture);
    expect(api.getCalls).toEqual([order.id, order.id]);
    expect(view(fixture).textContent).toContain('Cancelled');
  });

  it('shows approved conflict copy and reconciles without blind retry', async () => {
    const api = new OrderStub();
    api.cancelError = { status: 409, error: { detail: 'Internal checkout session' } };
    const { fixture } = await setup(api);
    button(fixture, 'cancel-order')?.click();
    await settle(fixture);
    api.current = { ...order, status: 'Paid' };
    button(fixture, 'confirm-cancel')?.click();
    await settle(fixture);
    expect(api.cancelCalls).toEqual([order.id]);
    expect(api.getCalls).toEqual([order.id, order.id]);
    expect(view(fixture).textContent).toContain('This order can no longer be cancelled.');
    expect(view(fixture).textContent).not.toContain('Internal checkout session');
  });

  it('does not misreport a successful cancel as failed when the follow-up read is unavailable', async () => {
    const api = new OrderStub();
    api.reloadError = { status: 503 };
    const { fixture } = await setup(api);
    button(fixture, 'cancel-order')?.click();
    await settle(fixture);
    button(fixture, 'confirm-cancel')?.click();
    await settle(fixture);
    expect(api.cancelCalls).toEqual([order.id]);
    expect(view(fixture).textContent).toContain("We couldn't load this order. Try again.");
    expect(view(fixture).textContent).not.toContain("We couldn't cancel this order.");
  });

  it('hides foreign or missing orders behind privacy-safe unavailable copy', async () => {
    const api = new OrderStub();
    api.getError = { status: 404, error: { detail: 'Foreign owner' } };
    const { fixture } = await setup(api);
    expect(view(fixture).textContent).toContain("This order isn't available.");
    expect(view(fixture).textContent).not.toContain('Foreign owner');
    expect(button(fixture, 'cancel-order')).toBeNull();
  });

  it('shows a retryable safe error with no stale order facts', async () => {
    const api = new OrderStub();
    api.getError = { status: 503 };
    const { fixture } = await setup(api);
    expect(view(fixture).textContent).toContain("We couldn't load this order. Try again.");
    api.getError = undefined;
    view(fixture).querySelector<HTMLButtonElement>('.np-loading-error-retry-retry')?.click();
    await settle(fixture);
    expect(api.getCalls).toEqual([order.id, order.id]);
  });

  it('mounts canonical detail route with all three guards', () => {
    const route = routes.find((entry) => entry.path === 'commerce/orders/:orderId');
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'COMMERCE_ORDER_DETAIL' });
  });
});
