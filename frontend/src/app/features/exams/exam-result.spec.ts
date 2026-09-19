import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamFullResult } from '../../core/api/exams-api';
import { ExamResultScreen } from './exam-result';

function fullResult(overrides: Partial<ExamFullResult> = {}): ExamFullResult {
  return {
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
    ...overrides,
  };
}

class ExamsApiStub {
  result: ExamFullResult = fullResult();
  error: unknown = undefined;
  calls: string[] = [];

  listExams() {
    return throwError(() => ({ status: 500 }));
  }

  getExam() {
    return throwError(() => ({ status: 404 }));
  }

  listCountries() {
    return of([]);
  }

  getExamSession() {
    return throwError(() => ({ status: 404 }));
  }

  saveExamSessionAnswers() {
    return throwError(() => ({ status: 409 }));
  }

  submitExamSession() {
    return throwError(() => ({ status: 409 }));
  }

  getExamSessionResult(sessionId: string) {
    this.calls.push(sessionId);
    if (this.error !== undefined) {
      return throwError(() => this.error);
    }
    return of(this.result);
  }
}

async function setup(
  stub?: ExamsApiStub,
  params: Record<string, string> = { examId: 'exam-1', sessionId: 'session-9' },
): Promise<{
  fixture: ComponentFixture<ExamResultScreen>;
  api: ExamsApiStub;
}> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamResultScreen],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap(params) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamResultScreen);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<ExamResultScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamResultScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<ExamResultScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamResult screen (T-FE-071)', () => {
  it('loads the result by route sessionId from route identity alone', async () => {
    const { fixture, api } = await setup();

    expect(api.calls).toEqual(['session-9']);
    expect(byTestId(fixture, 'exam-result-heading')?.textContent).toContain('Exam result');
  });

  it('renders the backend exam title as secondary context without requiring it', async () => {
    const stub = new ExamsApiStub();
    stub.result = fullResult({ examTitle: null });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-result-heading')?.textContent).toContain('Exam result');
    expect(byTestId(fixture, 'exam-result-title')).toBeNull();
  });

  it('renders a Submitted result as Completed with backend-verbatim aggregates', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'exam-result-status')?.textContent).toContain('Completed');
    expect(byTestId(fixture, 'exam-result-score')?.textContent).toContain('68');
    expect(byTestId(fixture, 'exam-result-score')?.textContent).toContain('75');
    expect(byTestId(fixture, 'exam-result-percentage')?.textContent).toContain('90.67');
    expect(byTestId(fixture, 'exam-result-outcome')?.textContent).toContain('Passed');
    expect(byTestId(fixture, 'exam-result-correct')?.textContent).toContain('68');
    expect(byTestId(fixture, 'exam-result-questions')?.textContent).toContain('75');
  });

  it('renders backend Not passed without a local threshold', async () => {
    const stub = new ExamsApiStub();
    stub.result = fullResult({ passed: false });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-result-outcome')?.textContent).toContain('Not passed');
    expect(byTestId(fixture, 'exam-result-outcome')?.textContent).not.toContain('Passed.');
  });

  it('renders an Expired result as Time expired with backend aggregates unchanged', async () => {
    const stub = new ExamsApiStub();
    stub.result = fullResult({ status: 'Expired', passed: false, score: 40, percentage: 53.33 });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-result-status')?.textContent).toContain('Time expired');
    expect(byTestId(fixture, 'exam-result-score')?.textContent).toContain('40');
    expect(byTestId(fixture, 'exam-result-percentage')?.textContent).toContain('53.33');
    expect(byTestId(fixture, 'exam-result-outcome')?.textContent).toContain('Not passed');
  });

  it('shows the approved unavailable state when the result examId conflicts with the route', async () => {
    const stub = new ExamsApiStub();
    stub.result = fullResult({ examId: 'other-exam' });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-result-unavailable')?.textContent).toContain(
      "This exam result isn't available.",
    );
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
    expect(text(fixture)).not.toContain('other-exam');
  });

  it('shows the approved unavailable copy on 404', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-result-unavailable')?.textContent).toContain(
      "This exam result isn't available.",
    );
  });

  it('shows the approved not-finalized copy on 409', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 409 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-result-not-finalized')?.textContent).toContain(
      'Finish the exam before viewing the result.',
    );
  });

  it('retries a generic failure with the same route sessionId', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load this exam result. Try again.");

    stub.error = undefined;
    const retryButton = (fixture.nativeElement as HTMLElement).querySelector(
      '.np-loading-error-retry-retry',
    ) as HTMLButtonElement | null;
    expect(retryButton?.textContent).toContain('Retry');
    retryButton?.click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls).toEqual(['session-9', 'session-9']);
    expect(byTestId(fixture, 'exam-result-status')).not.toBeNull();
  });

  it('links Back to exams without browser-history dependence', async () => {
    const { fixture } = await setup();
    const back = byTestId(fixture, 'exam-result-back') as HTMLAnchorElement | null;

    expect(back?.textContent).toContain('Back to exams');
    expect(back?.getAttribute('href')).toBe('/exams');
  });

  it('never renders ids, timestamps, review content, or a review CTA', async () => {
    const { fixture } = await setup();
    const body = text(fixture);

    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/startedAt|finalizedAt|20\d\d-/);
    expect(body).not.toMatch(/correct answer|answer key|rationale|explanation|Review answers/i);
  });
});

describe('ExamResult route', () => {
  it('mounts /exams/:examId/sessions/:sessionId/result with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/:examId/sessions/:sessionId/result');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_RESULT' });
  });
});
