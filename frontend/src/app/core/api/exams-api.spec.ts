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

function sessionResponse(overrides: Record<string, unknown> = {}) {
  return {
    id: 'session-9',
    examId: 'exam-1',
    examTitle: 'Exam',
    status: 'InProgress',
    source: 'Free',
    startedAt: '2026-09-18T00:00:00Z',
    expiresAt: '2999-01-01T00:00:00Z',
    remainingSeconds: 3600,
    items: [
      {
        id: 'q-1',
        displayOrder: 1,
        text: 'First prompt',
        points: 1,
        selectedExamSessionAnswerOptionId: null,
        options: [
          { id: 'o-1a', displayOrder: 1, text: 'First option one' },
          { id: 'o-1b', displayOrder: 2, text: 'First option two' },
        ],
      },
      {
        id: 'q-2',
        displayOrder: 2,
        text: 'Second prompt',
        points: 1,
        selectedExamSessionAnswerOptionId: 'o-2a',
        options: [
          { id: 'o-2a', displayOrder: 1, text: 'Second option one' },
          { id: 'o-2b', displayOrder: 2, text: 'Second option two' },
        ],
      },
    ],
    ...overrides,
  };
}

/**
 * T-FE-069 proving evidence: the session facade delegates to the generated
 * exam-session operations with exact session ids and adapts bodies into
 * explicit frontend-owned shapes that carry no correctness/review fields.
 * Save posts the generated answer-item shape; submit exposes only the
 * aggregate transient result.
 */
describe('exams-api exam session (T-FE-069)', () => {
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

  it('gets a session by id with adapted questions, options, and persisted answers', async () => {
    const result = firstValueFrom(api.getExamSession('session-9'));
    const request = httpMock.expectOne('/api/v1/exam-sessions/session-9');

    expect(request.request.method).toBe('GET');
    request.flush(sessionResponse());
    const session = await result;

    expect(session.id).toBe('session-9');
    expect(session.status).toBe('InProgress');
    expect(session.remainingSeconds).toBe(3600);
    expect(session.items.map((item) => item.examSessionQuestionId)).toEqual(['q-1', 'q-2']);
    expect(session.items[1].selectedExamSessionAnswerOptionId).toBe('o-2a');
    expect(session.items[0].options.map((option) => option.text)).toEqual([
      'First option one',
      'First option two',
    ]);
    expect(JSON.stringify(session)).not.toContain('IsCorrect');
    expect(JSON.stringify(session)).not.toContain('CorrectAnswer');
  });

  it('saves answers with the exact generated request shape', async () => {
    const result = firstValueFrom(
      api.saveExamSessionAnswers('session-9', [
        { examSessionQuestionId: 'q-1', selectedExamSessionAnswerOptionId: 'o-1b' },
      ]),
    );
    const request = httpMock.expectOne('/api/v1/exam-sessions/session-9/answers');

    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      answers: [{ examSessionQuestionId: 'q-1', selectedExamSessionAnswerOptionId: 'o-1b' }],
    });
    request.flush(sessionResponse());
    await expect(result).resolves.toMatchObject({ id: 'session-9' });
  });

  it('submits a session with the exact id and adapts only aggregate result fields', async () => {
    const result = firstValueFrom(api.submitExamSession('session-9'));
    const request = httpMock.expectOne('/api/v1/exam-sessions/session-9/submit');

    expect(request.request.method).toBe('POST');
    request.flush({
      id: 'session-9',
      examId: 'exam-1',
      examTitle: 'Exam',
      status: 'Submitted',
      startedAt: '2026-09-18T00:00:00Z',
      expiresAt: '2999-01-01T00:00:00Z',
      submittedAt: '2026-09-18T00:30:00Z',
      finalizedAt: '2026-09-18T00:30:00Z',
      score: 1,
      maxScore: 2,
      percentage: 50,
      passed: false,
      correctCount: 1,
      questionCount: 2,
    });
    await expect(result).resolves.toEqual({
      score: 1,
      maxScore: 2,
      percentage: 50,
      passed: false,
      correctCount: 1,
      questionCount: 2,
    });
  });
});

/**
 * T-FE-071 proving evidence: the result facade delegates to the generated
 * GetExamSessionResult operation with the exact session id and adapts the
 * body into an explicit frontend-owned full-result shape carrying identity
 * (session/exam/title/status) plus backend-verbatim aggregates only.
 * Timestamps and review/per-question fields never enter the presentation model.
 */
describe('exams-api exam result (T-FE-071)', () => {
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

  function resultResponse(overrides: Record<string, unknown> = {}) {
    return {
      id: 'session-9',
      examId: 'exam-1',
      examTitle: 'NCLEX Readiness',
      status: 'Submitted',
      startedAt: '2026-09-18T00:00:00Z',
      expiresAt: '2026-09-18T01:00:00Z',
      submittedAt: '2026-09-18T00:30:00Z',
      finalizedAt: '2026-09-18T00:30:00Z',
      score: 68,
      maxScore: 75,
      percentage: 90.67,
      passed: true,
      correctCount: 68,
      questionCount: 75,
      ...overrides,
    };
  }

  it('gets a finalized result by exact session id with identity plus verbatim aggregates', async () => {
    const result = firstValueFrom(api.getExamSessionResult('session-9'));
    const request = httpMock.expectOne('/api/v1/exam-sessions/session-9/result');

    expect(request.request.method).toBe('GET');
    request.flush(resultResponse());
    await expect(result).resolves.toEqual({
      sessionId: 'session-9',
      examId: 'exam-1',
      examTitle: 'NCLEX Readiness',
      status: 'Submitted',
      score: 68,
      maxScore: 75,
      percentage: 90.67,
      passed: true,
      correctCount: 68,
      questionCount: 75,
    });
  });

  it('preserves an Expired status verbatim with backend aggregates unchanged', async () => {
    const result = firstValueFrom(api.getExamSessionResult('session-9'));
    httpMock
      .expectOne('/api/v1/exam-sessions/session-9/result')
      .flush(resultResponse({ status: 'Expired', passed: false, score: 40, percentage: 53.33 }));

    await expect(result).resolves.toMatchObject({ status: 'Expired', passed: false, score: 40 });
  });

  it('normalizes a blank exam title to null and never exposes timestamps or review fields', async () => {
    const result = firstValueFrom(api.getExamSessionResult('session-9'));
    httpMock
      .expectOne('/api/v1/exam-sessions/session-9/result')
      .flush(resultResponse({ examTitle: '   ' }));

    const adapted = await result;
    expect(adapted.examTitle).toBeNull();
    expect(JSON.stringify(adapted)).not.toContain('startedAt');
    expect(JSON.stringify(adapted)).not.toContain('finalizedAt');
    expect(JSON.stringify(adapted)).not.toContain('IsCorrect');
    expect(JSON.stringify(adapted)).not.toContain('explanation');
  });
});

describe('exams-api exam review (T-FE-072)', () => {
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

  function reviewResponse(overrides: Record<string, unknown> = {}) {
    return {
      id: 'session-9',
      examId: 'exam-1',
      examTitle: 'NCLEX Readiness',
      status: 'Submitted',
      score: 1,
      maxScore: 2,
      percentage: 50,
      passed: false,
      items: [
        {
          id: 'question-1',
          displayOrder: 2,
          text: 'Second question',
          explanation: 'Second why.',
          points: 3,
          pointsEarned: 0,
          selectedExamSessionAnswerOptionId: 'question-1-option-b',
          correctAnswerOptionId: 'question-1-option-a',
          options: [
            {
              id: 'question-1-option-a',
              displayOrder: 2,
              text: 'Second A',
              isCorrect: true,
            },
            {
              id: 'question-1-option-b',
              displayOrder: 1,
              text: 'Second B',
              isCorrect: false,
            },
          ],
        },
        {
          id: 'question-2',
          displayOrder: 1,
          text: 'First question',
          explanation: null,
          points: 1,
          pointsEarned: 1,
          selectedExamSessionAnswerOptionId: 'question-2-option-a',
          correctAnswerOptionId: 'question-2-option-a',
          options: [
            {
              id: 'question-2-option-a',
              displayOrder: 1,
              text: 'First A',
              isCorrect: true,
            },
          ],
        },
      ],
      ...overrides,
    };
  }

  it('gets finalized review by exact session id preserving backend order with selection mapping', async () => {
    const result = firstValueFrom(api.getExamSessionReview('session-9'));
    const request = httpMock.expectOne('/api/v1/exam-sessions/session-9/review');

    expect(request.request.method).toBe('GET');
    request.flush(reviewResponse());
    await expect(result).resolves.toEqual({
      examId: 'exam-1',
      examTitle: 'NCLEX Readiness',
      status: 'Submitted',
      items: [
        {
          displayOrder: 2,
          text: 'Second question',
          explanation: 'Second why.',
          points: 3,
          pointsEarned: 0,
          options: [
            { displayOrder: 2, text: 'Second A', isCorrect: true, isSelected: false },
            { displayOrder: 1, text: 'Second B', isCorrect: false, isSelected: true },
          ],
        },
        {
          displayOrder: 1,
          text: 'First question',
          explanation: null,
          points: 1,
          pointsEarned: 1,
          options: [
            { displayOrder: 1, text: 'First A', isCorrect: true, isSelected: true },
          ],
        },
      ],
    });
  });

  it('normalizes blank titles and explanations and exposes no aggregates or raw ids', async () => {
    const result = firstValueFrom(api.getExamSessionReview('session-9'));
    httpMock
      .expectOne('/api/v1/exam-sessions/session-9/review')
      .flush(reviewResponse({ examTitle: '   ' }));

    const adapted = await result;
    expect(adapted.examTitle).toBeNull();
    expect(adapted.examId).toBe('exam-1');
    const serialized = JSON.stringify(adapted);
    expect(serialized).not.toContain('score');
    expect(serialized).not.toContain('passed');
    expect(serialized).not.toContain('question-1');
    expect(serialized).not.toContain('question-1-option-a');
    expect(serialized).not.toContain('session-9');
  });

  it('maps an unanswered question with no selected option', async () => {
    const response = reviewResponse();
    const items = response.items as Record<string, unknown>[];
    items[0] = { ...items[0], selectedExamSessionAnswerOptionId: null };
    const result = firstValueFrom(api.getExamSessionReview('session-9'));
    httpMock.expectOne('/api/v1/exam-sessions/session-9/review').flush(response);

    const adapted = await result;
    expect(adapted.items[0].options.every((option) => !option.isSelected)).toBe(true);
    expect(adapted.items[0].options.find((option) => option.isCorrect)?.text).toBe('Second A');
  });

  it('rejects review content with empty question text', async () => {
    const response = reviewResponse();
    const items = response.items as Record<string, unknown>[];
    items[0] = { ...items[0], text: '  ' };
    const result = firstValueFrom(api.getExamSessionReview('session-9'));
    httpMock.expectOne('/api/v1/exam-sessions/session-9/review').flush(response);

    await expect(result).rejects.toThrow('Exam review response did not include usable review content.');
  });

  it('rejects review content with a selection matching no option', async () => {
    const response = reviewResponse();
    const items = response.items as Record<string, unknown>[];
    items[0] = { ...items[0], selectedExamSessionAnswerOptionId: 'missing-option' };
    const result = firstValueFrom(api.getExamSessionReview('session-9'));
    httpMock.expectOne('/api/v1/exam-sessions/session-9/review').flush(response);

    await expect(result).rejects.toThrow('Exam review response did not include usable review content.');
  });

  it('rejects review content with an empty option list', async () => {
    const response = reviewResponse();
    const items = response.items as Record<string, unknown>[];
    items[0] = { ...items[0], options: [] };
    const result = firstValueFrom(api.getExamSessionReview('session-9'));
    httpMock.expectOne('/api/v1/exam-sessions/session-9/review').flush(response);

    await expect(result).rejects.toThrow('Exam review response did not include usable review content.');
  });
});
