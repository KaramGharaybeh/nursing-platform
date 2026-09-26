import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { AdminExamsApi } from './admin-exams-api';
import { provideApiConfig } from './api-config';

const path = '/api/v1/admin/exams';
const exam = {
  id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
  categoryName: 'NCLEX', title: 'Nursing Exam', slug: 'nursing-exam', description: null,
  instructions: null, durationMinutes: 90, passingScorePercentage: 70, status: 'Draft',
  isFree: true, publishedAt: null,
};
const body = {
  countryId: 'country-1', examCategoryId: 'category-1', title: 'Nursing Exam', slug: 'nursing-exam',
  durationMinutes: 90, passingScorePercentage: 70, isFree: true,
};

describe('AdminExamsApi (T-FE-107)', () => {
  let api: AdminExamsApi;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()] });
    api = TestBed.inject(AdminExamsApi);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('lists backend page with exact optional filters and typed facts', async () => {
    const result = firstValueFrom(api.list({ page: 2, countryId: 'country-1', isFree: false, status: 0 }));
    const request = http.expectOne(`${path}?page=2&pageSize=20&countryId=country-1&status=0&isFree=false`);
    expect(request.request.method).toBe('GET');
    request.flush({ items: [exam], page: 2, pageSize: 20, totalCount: 21, totalPages: 2 });
    expect((await result).items[0].title).toBe('Nursing Exam');
  });

  it('gets detail with the owner route only', async () => {
    const result = firstValueFrom(api.get('exam-1'));
    const request = http.expectOne(`${path}/exam-1`);
    expect(request.request.method).toBe('GET');
    request.flush(exam);
    expect((await result).status).toBe('Draft');
  });

  it('creates with only supported request fields and accepts typed 201', async () => {
    const result = firstValueFrom(api.create(body));
    const request = http.expectOne(path);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush(exam, { status: 201, statusText: 'Created' });
    expect((await result).id).toBe('exam-1');
  });

  it('updates through the existing endpoint and returns server truth', async () => {
    const result = firstValueFrom(api.update('exam-1', body));
    const request = http.expectOne(`${path}/exam-1`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush(exam);
    expect((await result).countryName).toBe('United States');
  });

  it('archives through the exact route without automatic retry', async () => {
    const result = firstValueFrom(api.archive('exam-1'));
    const request = http.expectOne(`${path}/exam-1/archive`);
    expect(request.request.method).toBe('POST');
    request.flush({ ...exam, status: 'Archived' });
    expect((await result).status).toBe('Archived');
  });

  it('deletes with 204 and no invented response body', async () => {
    const result = firstValueFrom(api.delete('exam-1'));
    const request = http.expectOne(`${path}/exam-1`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null, { status: 204, statusText: 'No Content' });
    expect(await result).toBeUndefined();
  });
});
