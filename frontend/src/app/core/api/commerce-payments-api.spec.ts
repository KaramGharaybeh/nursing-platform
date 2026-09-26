import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { provideApiConfig } from './api-config';
import { CommercePaymentsApi } from './commerce-payments-api';

function productResponse(overrides: Record<string, unknown> = {}) {
  return {
    id: 'product-1',
    type: 'ExamAccess',
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    name: 'NCLEX Mock Exam',
    description: 'A full mock exam.',
    currency: 'USD',
    unitAmountMinor: '4999',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-02T00:00:00Z',
    ...overrides,
  };
}

describe('commerce-payments-api (T-FE-082)', () => {
  let api: CommercePaymentsApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    api = TestBed.inject(CommercePaymentsApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists products with page and fixed page size preserving backend order', async () => {
    const result = firstValueFrom(api.listProducts(1));
    const request = httpMock.expectOne('/api/v1/payment/products?page=1&pageSize=20');

    expect(request.request.method).toBe('GET');
    request.flush({
      items: [
        productResponse(),
        productResponse({ id: 'product-2', name: 'Second product', description: null }),
      ],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      totalPages: 1,
    });
    const adapted = await result;
    expect(adapted.page).toBe(1);
    expect(adapted.totalCount).toBe(2);
    expect(adapted.items.map((item) => item.name)).toEqual([
      'NCLEX Mock Exam',
      'Second product',
    ]);
    expect(adapted.items[0]).toEqual({
      id: 'product-1',
      name: 'NCLEX Mock Exam',
      description: 'A full mock exam.',
      examTitle: 'NCLEX Readiness',
      currency: 'USD',
      unitAmountMinor: '4999',
      isActive: true,
    });
  });

  it('gets one product by id with safe adaptation and inactive preserved', async () => {
    const result = firstValueFrom(api.getProduct('product-9'));
    const request = httpMock.expectOne('/api/v1/payment/products/product-9');

    expect(request.request.method).toBe('GET');
    request.flush(productResponse({ id: 'product-9', isActive: false, description: '  ' }));
    const adapted = await result;
    expect(adapted.id).toBe('product-9');
    expect(adapted.isActive).toBe(false);
    expect(adapted.description).toBeNull();
    expect(JSON.stringify(adapted)).not.toContain('createdAt');
    expect(JSON.stringify(adapted)).not.toContain('ExamAccess');
  });

  it('rejects products with missing identity instead of rendering broken content', async () => {
    const result = firstValueFrom(api.listProducts(1));
    httpMock
      .expectOne('/api/v1/payment/products?page=1&pageSize=20')
      .flush({
        items: [productResponse({ id: '' })],
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      });

    await expect(result).rejects.toThrow(
      'Product response did not include usable product content.',
    );
  });

  it('rejects products with malformed monetary data instead of guessing a price', async () => {
    const result = firstValueFrom(api.getProduct('product-1'));
    httpMock
      .expectOne('/api/v1/payment/products/product-1')
      .flush(productResponse({ unitAmountMinor: '49.99' }));

    await expect(result).rejects.toThrow(
      'Product response did not include usable product content.',
    );
  });

  function orderResponse(overrides: Record<string, unknown> = {}) {
    return {
      id: 'order-1',
      status: 'PendingPayment',
      currency: 'USD',
      totalAmountMinor: '4999',
      createdAt: '2026-02-01T00:00:00Z',
      updatedAt: '2026-02-01T00:00:00Z',
      cancelledAt: null,
      expiresAt: null,
      paidAt: null,
      items: [
        {
          id: 'item-1',
          productId: 'product-1',
          productName: 'NCLEX Mock Exam',
          productType: 'ExamAccess',
          examId: 'exam-1',
          currency: 'USD',
          unitAmountMinor: '4999',
          quantity: 1,
          lineTotalAmountMinor: '4999',
          sourceType: 'Product',
          sourceId: 'product-1',
          packageSnapshot: null,
        },
      ],
      ...overrides,
    };
  }

  it('creates a product order with the exact create path and adapts safe order content', async () => {
    const result = firstValueFrom(api.createOrder({ productId: 'product-1' }));
    const request = httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ productId: 'product-1' });
    request.flush(orderResponse());
    const adapted = await result;
    expect(adapted.id).toBe('order-1');
    expect(adapted.status).toBe('PendingPayment');
    expect(adapted.currency).toBe('USD');
    expect(adapted.totalAmountMinor).toBe('4999');
    expect(adapted.items.map((item) => item.title)).toEqual(['NCLEX Mock Exam']);
    expect(JSON.stringify(adapted)).not.toContain('packageSnapshot');
    expect(JSON.stringify(adapted)).not.toContain('ExamAccess');
  });

  it('creates a package order with only the package offer source', async () => {
    const result = firstValueFrom(api.createOrder({ packageOfferId: 'offer-1' }));
    const request = httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ packageOfferId: 'offer-1' });
    request.flush(orderResponse({ id: 'order-2' }));
    expect((await result).id).toBe('order-2');
  });

  it('rejects order creation without exactly one purchase source before any request', () => {
    expect(() => api.createOrder({ productId: '   ' })).toThrow(
      'Order creation requires exactly one purchase source.',
    );
    expect(() =>
      api.createOrder({ productId: 'product-1', packageOfferId: 'offer-1' }),
    ).toThrow('Order creation requires exactly one purchase source.');
  });

  it('rejects malformed order content instead of rendering a broken order', async () => {
    const result = firstValueFrom(api.createOrder({ productId: 'product-1' }));
    httpMock
      .expectOne('/api/v1/me/nurse-profile/payment/orders')
      .flush(orderResponse({ id: '', totalAmountMinor: '49.99' }));

    await expect(result).rejects.toThrow(
      'Order response did not include usable order content.',
    );
  });

  it('lists owned orders on the requested backend page without a status filter', async () => {
    const result = firstValueFrom(api.listOrders(2));
    const request = httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders?page=2&pageSize=20');
    expect(request.request.method).toBe('GET');
    request.flush({
      items: [orderResponse({ id: 'order-2' }), orderResponse()],
      page: 2,
      pageSize: 20,
      totalCount: 22,
      totalPages: 2,
    });
    const page = await result;
    expect(page).toMatchObject({ page: 2, pageSize: 20, totalCount: 22, totalPages: 2 });
    expect(page.items.map((item) => item.id)).toEqual(['order-2', 'order-1']);
    expect(JSON.stringify(page)).not.toContain('packageSnapshot');
  });

  it('retrieves an owned order by id and preserves safe payment dates', async () => {
    const result = firstValueFrom(api.getOrder('order-1'));
    const request = httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders/order-1');
    expect(request.request.method).toBe('GET');
    request.flush(orderResponse({ status: 'Paid', paidAt: '2026-02-02T00:00:00Z' }));
    expect(await result).toMatchObject({ status: 'Paid', paidAt: '2026-02-02T00:00:00Z' });
  });

  it('cancels once via the owner-scoped endpoint and adapts the resulting order', async () => {
    const result = firstValueFrom(api.cancelOrder('order-1'));
    const request = httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders/order-1/cancel');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(orderResponse({ status: 'Cancelled', cancelledAt: '2026-02-02T00:00:00Z' }));
    expect((await result).status).toBe('Cancelled');
  });

  it('rejects unusable list items instead of showing partial order history', async () => {
    const result = firstValueFrom(api.listOrders(1));
    httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders?page=1&pageSize=20').flush({
      items: [orderResponse({ items: [] })], page: 1, pageSize: 20, totalCount: 1, totalPages: 1,
    });
    await expect(result).rejects.toThrow('Order response did not include usable order content.');
  });

  it('rejects unknown payment statuses instead of displaying internal text', async () => {
    const result = firstValueFrom(api.getOrder('order-1'));
    httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders/order-1').flush(
      orderResponse({ status: 'InternalProviderStatus' }),
    );
    await expect(result).rejects.toThrow('Order response did not include usable order content.');
  });

  it('does not turn malformed pagination into a false empty order history', async () => {
    const result = firstValueFrom(api.listOrders(1));
    httpMock.expectOne('/api/v1/me/nurse-profile/payment/orders?page=1&pageSize=20')
      .flush({ items: [], page: 1, pageSize: 20 });
    await expect(result).rejects.toThrow('Order response did not include usable order history.');
  });
});
