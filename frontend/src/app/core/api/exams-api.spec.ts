import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { provideApiConfig } from './api-config';
import { ExamsApi } from './exams-api';

/**
 * T-FE-067 proving evidence: the thin exams facade delegates to the generated
 * exam operations with exact query/path parameters and adapts the untyped
 * generated bodies into explicit frontend-owned exam shapes. The catalog UI
 * sends page/pageSize plus optional countryId/categoryId only.
 */
describe('exams-api (T-FE-067)', () => {
  let api: ExamsApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    api = TestBed.inject(ExamsApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists exams with page/pageSize and no filter parameters by default', async () => {
    const result = firstValueFrom(api.listExams({ page: 1, pageSize: 20 }));
    const request = httpMock.expectOne('/api/v1/exams?page=1&pageSize=20');

    expect(request.request.method).toBe('GET');
    request.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
    await expect(result).resolves.toEqual({
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
    });
  });

  it('passes countryId and categoryId only when selected', async () => {
    const result = firstValueFrom(
      api.listExams({ page: 2, pageSize: 20, countryId: 'country-1', categoryId: 'cat-1' }),
    );
    const request = httpMock.expectOne(
      '/api/v1/exams?page=2&pageSize=20&countryId=country-1&categoryId=cat-1',
    );

    expect(request.request.method).toBe('GET');
    request.flush({ items: [], page: 2, pageSize: 20, totalCount: 0, totalPages: 0 });
    await expect(result).resolves.toEqual({
      items: [],
      page: 2,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
    });
  });

  it('adapts catalog items into the explicit frontend-owned shape', async () => {
    const result = firstValueFrom(api.listExams({ page: 1, pageSize: 20 }));
    const request = httpMock.expectOne('/api/v1/exams?page=1&pageSize=20');

    request.flush({
      items: [
        {
          id: 'exam-1',
          title: 'NCLEX Readiness',
          description: null,
          countryId: 'country-1',
          countryName: 'Jordan',
          categoryId: 'cat-1',
          categoryName: 'Licensure',
          durationMinutes: 120,
          questionCount: 75,
          passingScorePercentage: 70,
          isFree: true,
          canStart: true,
          extraUnknownField: 'ignored',
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    });

    await expect(result).resolves.toEqual({
      items: [
        {
          id: 'exam-1',
          title: 'NCLEX Readiness',
          description: null,
          countryId: 'country-1',
          countryName: 'Jordan',
          categoryId: 'cat-1',
          categoryName: 'Licensure',
          durationMinutes: 120,
          questionCount: 75,
          passingScorePercentage: 70,
          isFree: true,
          canStart: true,
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    });
  });

  it('gets one exam by id through the generated detail operation', async () => {
    const result = firstValueFrom(api.getExam('exam-1'));
    const request = httpMock.expectOne('/api/v1/exams/exam-1');

    expect(request.request.method).toBe('GET');
    request.flush({
      id: 'exam-1',
      title: 'NCLEX Readiness',
      description: 'Are you ready?',
      instructions: 'Answer every question.',
      countryId: 'country-1',
      countryName: 'Jordan',
      categoryId: null,
      categoryName: null,
      durationMinutes: 120,
      questionCount: 75,
      passingScorePercentage: 70,
      isFree: false,
      canStart: false,
    });
    await expect(result).resolves.toEqual({
      id: 'exam-1',
      title: 'NCLEX Readiness',
      description: 'Are you ready?',
      instructions: 'Answer every question.',
      countryId: 'country-1',
      countryName: 'Jordan',
      categoryId: null,
      categoryName: null,
      durationMinutes: 120,
      questionCount: 75,
      passingScorePercentage: 70,
      isFree: false,
      canStart: false,
    });
  });

  it('lists countries for the Country filter', async () => {
    const result = firstValueFrom(api.listCountries());
    const request = httpMock.expectOne('/api/v1/countries');

    expect(request.request.method).toBe('GET');
    request.flush([{ id: 'country-1', code: 'JO', name: 'Jordan' }]);
    await expect(result).resolves.toEqual([{ id: 'country-1', name: 'Jordan' }]);
  });
});
