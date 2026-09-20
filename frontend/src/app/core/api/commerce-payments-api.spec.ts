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
});
