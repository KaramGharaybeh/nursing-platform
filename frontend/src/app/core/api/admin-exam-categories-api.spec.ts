import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { AdminExamCategoriesApi } from './admin-exam-categories-api';
import { provideApiConfig } from './api-config';

const path = '/api/v1/admin/exam-categories';
const category = {
  id: 'category-1', countryId: 'country-1', countryName: 'United States', name: 'NCLEX',
  slug: 'nclex', description: null, displayOrder: 2, isActive: true,
};

describe('AdminExamCategoriesApi (T-FE-106)', () => {
  let api: AdminExamCategoriesApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()] });
    api = TestBed.inject(AdminExamCategoriesApi);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('lists categories with backend paging and exact optional filters', async () => {
    const result = firstValueFrom(api.list({ page: 2, pageSize: 20, countryId: 'country-1', isActive: false }));
    const req = http.expectOne(`${path}?page=2&pageSize=20&countryId=country-1&isActive=false`);
    expect(req.request.method).toBe('GET');
    req.flush({ items: [category], page: 2, pageSize: 20, totalCount: 21, totalPages: 2 });
    expect((await result).items[0].name).toBe('NCLEX');
  });

  it('gets one category through its typed owner endpoint', async () => {
    const result = firstValueFrom(api.get('category-1'));
    http.expectOne(`${path}/category-1`).flush(category);
    expect((await result).countryName).toBe('United States');
  });

  it('creates with the exact request and typed 201 body', async () => {
    const body = { countryId: 'country-1', name: 'NCLEX', slug: 'nclex', displayOrder: 2 };
    const result = firstValueFrom(api.create(body));
    const req = http.expectOne(path);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush(category, { status: 201, statusText: 'Created' });
    expect((await result).id).toBe('category-1');
  });

  it('updates by id and retains typed response', async () => {
    const body = { name: 'NCLEX', slug: 'nclex', displayOrder: 2, countryId: 'country-1' };
    const result = firstValueFrom(api.update('category-1', body));
    const req = http.expectOne(`${path}/category-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);
    req.flush(category);
    expect((await result).name).toBe('NCLEX');
  });

  it.each(['archive', 'restore'] as const)('%s returns server-backed category state', async (action) => {
    const result = firstValueFrom(api[action]('category-1'));
    const req = http.expectOne(`${path}/category-1/${action}`);
    expect(req.request.method).toBe('POST');
    req.flush({ ...category, isActive: action === 'restore' });
    expect((await result).isActive).toBe(action === 'restore');
  });

  it('deletes via 204 with no response body or automatic retry', async () => {
    const result = firstValueFrom(api.delete('category-1'));
    const req = http.expectOne(`${path}/category-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });
    expect(await result).toBeUndefined();
  });
});
