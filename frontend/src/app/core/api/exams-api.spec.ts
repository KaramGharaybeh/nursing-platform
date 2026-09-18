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

function attemptResponse(items: Record<string, unknown>[], page = 1, totalPages = 1) {
  return { items, page, pageSize: 100, totalCount: items.length, totalPages };
}

function inProgressAttempt(examId: string, id = 'sess-1', expiresAt = '2999-01-01T00:00:00Z') {
  return {
    id,
    examId,
    examTitle: 'Exam',
    status: 'InProgress',
    startedAt: '2026-09-18T00:00:00Z',
    expiresAt,
    finalizedAt: null,
    score: null,
    maxScore: null,
    percentage: null,
    passed: null,
  };
}

/**
 * T-FE-068 proving evidence: resume detection traverses the paginated
 * attempts contract with status=InProgress and matches only unexpired
 * attempts for the current exam. Expired, foreign-exam, malformed, and
 * non-InProgress rows never produce Resume. Start posts the exact exam id
 * and exposes the returned session identity for canonical navigation.
 */
describe('exams-api resume detection (T-FE-068)', () => {
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

  it('requests InProgress attempts with the contract page size', async () => {
    const result = api.findResumableAttempt('exam-1');
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/exam-attempts?page=1&pageSize=100&status=0',
    );

    expect(request.request.method).toBe('GET');
    request.flush(attemptResponse([]));
    await expect(result).resolves.toBeUndefined();
  });

  it('accepts a matching unexpired InProgress attempt', async () => {
    const expected = inProgressAttempt('exam-1');
    const result = api.findResumableAttempt('exam-1');
    httpMock
      .expectOne('/api/v1/me/nurse-profile/exam-attempts?page=1&pageSize=100&status=0')
      .flush(attemptResponse([expected]));

    await expect(result).resolves.toEqual(expected);
  });

  it('ignores attempts for other exams, expired attempts, and malformed expiry', async () => {
    const result = api.findResumableAttempt('exam-1');
    httpMock
      .expectOne('/api/v1/me/nurse-profile/exam-attempts?page=1&pageSize=100&status=0')
      .flush(
        attemptResponse([
          inProgressAttempt('other-exam'),
          inProgressAttempt('exam-1', 'expired', '2020-01-01T00:00:00Z'),
          { ...inProgressAttempt('exam-1', 'malformed'), expiresAt: 'not-a-date' },
        ]),
      );

    await expect(result).resolves.toBeUndefined();
  });

  it('traverses pages until a match and stops immediately', async () => {
    const expected = inProgressAttempt('exam-1');
    const result = api.findResumableAttempt('exam-1');
    httpMock
      .expectOne('/api/v1/me/nurse-profile/exam-attempts?page=1&pageSize=100&status=0')
      .flush({ items: [inProgressAttempt('other')], page: 1, pageSize: 100, totalCount: 2, totalPages: 2 });
    await new Promise((resolve) => setTimeout(resolve, 0));
    httpMock
      .expectOne('/api/v1/me/nurse-profile/exam-attempts?page=2&pageSize=100&status=0')
      .flush({ items: [expected], page: 2, pageSize: 100, totalCount: 2, totalPages: 2 });

    await expect(result).resolves.toEqual(expected);
    httpMock.expectNone('/api/v1/me/nurse-profile/exam-attempts?page=3&pageSize=100&status=0');
  });

  it('stops at the final page when no match exists', async () => {
    const result = api.findResumableAttempt('exam-1');
    httpMock
      .expectOne('/api/v1/me/nurse-profile/exam-attempts?page=1&pageSize=100&status=0')
      .flush({ items: [inProgressAttempt('other')], page: 1, pageSize: 100, totalCount: 1, totalPages: 1 });

    await expect(result).resolves.toBeUndefined();
    httpMock.expectNone('/api/v1/me/nurse-profile/exam-attempts?page=2&pageSize=100&status=0');
  });

  it('starts an exam session with the exact exam id and exposes the session identity', async () => {
    const result = firstValueFrom(api.startExamSession('exam-1'));
    const request = httpMock.expectOne('/api/v1/exams/exam-1/sessions');

    expect(request.request.method).toBe('POST');
    request.flush({
      id: 'session-9',
      examId: 'exam-1',
      examTitle: 'Exam',
      status: 'InProgress',
      source: 'Free',
      startedAt: '2026-09-18T00:00:00Z',
      expiresAt: '2999-01-01T00:00:00Z',
      remainingSeconds: 3600,
      items: [],
    });
    await expect(result).resolves.toEqual({ sessionId: 'session-9', examId: 'exam-1' });
  });

  it('rejects a session response without session identity', async () => {
    const result = firstValueFrom(api.startExamSession('exam-1'));
    httpMock.expectOne('/api/v1/exams/exam-1/sessions').flush({ id: null, examId: 'exam-1' });

    await expect(result).rejects.toThrow('Exam session response did not include a session identity.');
  });
});
