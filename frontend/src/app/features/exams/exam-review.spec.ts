import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamReview } from '../../core/api/exams-api';
import { ExamReviewScreen } from './exam-review';

function incorrectQuestion() {
  return {
    displayOrder: 1,
    text: 'First question',
    explanation: 'Because safety comes first.',
    points: 3,
    pointsEarned: 0,
    options: [
      { displayOrder: 1, text: 'First A', isCorrect: true, isSelected: false },
      { displayOrder: 2, text: 'First B', isCorrect: false, isSelected: true },
    ],
  };
}

function unansweredQuestion() {
  return {
    displayOrder: 2,
    text: 'Second question',
    explanation: null,
    points: 1,
    pointsEarned: 0,
    options: [
      { displayOrder: 1, text: 'Second A', isCorrect: true, isSelected: false },
      { displayOrder: 2, text: 'Second B', isCorrect: false, isSelected: false },
    ],
  };
}

function correctQuestion() {
  return {
    displayOrder: 1,
    text: 'Only question',
    explanation: null,
    points: 2,
    pointsEarned: 2,
    options: [{ displayOrder: 1, text: 'Only A', isCorrect: true, isSelected: true }],
  };
}

function review(overrides: Partial<ExamReview> = {}): ExamReview {
  return {
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'Submitted',
    items: [incorrectQuestion(), unansweredQuestion()],
    ...overrides,
  };
}

class ExamsApiStub {
  review: ExamReview = review();
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

  getExamSessionResult() {
    return throwError(() => ({ status: 404 }));
  }

  getExamSessionReview(sessionId: string) {
    this.calls.push(sessionId);
    if (this.error !== undefined) {
      return throwError(() => this.error);
    }
    return of(this.review);
  }
}

async function setup(
  stub?: ExamsApiStub,
  params: Record<string, string> = { examId: 'exam-1', sessionId: 'session-9' },
): Promise<{
  fixture: ComponentFixture<ExamReviewScreen>;
  api: ExamsApiStub;
}> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamReviewScreen],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap(params) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamReviewScreen);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<ExamReviewScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamReviewScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<ExamReviewScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

function allByTestId(fixture: ComponentFixture<ExamReviewScreen>, id: string): HTMLElement[] {
  return [
    ...(fixture.nativeElement as HTMLElement).querySelectorAll(
      `[data-testid="${id}"]`,
    ),
  ] as HTMLElement[];
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamReview screen (T-FE-072)', () => {
  it('loads the review by route sessionId from route identity alone', async () => {
    const { fixture, api } = await setup();

    expect(api.calls).toEqual(['session-9']);
    expect(byTestId(fixture, 'exam-review-heading')?.textContent).toContain('Answer review');
  });

  it('renders the backend exam title as secondary context without requiring it', async () => {
    const stub = new ExamsApiStub();
    stub.review = review({ examTitle: null });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-review-heading')?.textContent).toContain('Answer review');
    expect(byTestId(fixture, 'exam-review-title')).toBeNull();
  });

  it('renders exactly one question with backend order and a position indicator', async () => {
    const { fixture } = await setup();

    expect(allByTestId(fixture, 'exam-review-question')).toHaveLength(1);
    expect(byTestId(fixture, 'exam-review-position')?.textContent).toContain('Question 1 of 2');
    expect(byTestId(fixture, 'exam-review-prompt')?.textContent).toContain('First question');
  });

  it('navigates locally without extra API calls and honors edge disablement', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as { next(): void; previous(): void };

    expect((byTestId(fixture, 'exam-review-prev') as HTMLButtonElement).disabled).toBe(true);

    component.next();
    fixture.detectChanges();
    await settle(fixture);

    expect(byTestId(fixture, 'exam-review-position')?.textContent).toContain('Question 2 of 2');
    expect(byTestId(fixture, 'exam-review-prompt')?.textContent).toContain('Second question');
    expect((byTestId(fixture, 'exam-review-next') as HTMLButtonElement).disabled).toBe(true);
    expect(api.calls).toEqual(['session-9']);

    component.previous();
    fixture.detectChanges();
    await settle(fixture);

    expect(byTestId(fixture, 'exam-review-position')?.textContent).toContain('Question 1 of 2');
  });

  it('marks an incorrect answer with text labels on both options', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'exam-review-status')?.textContent).toContain('Incorrect');
    expect(text(fixture)).toContain('Your answer');
    expect(text(fixture)).toContain('Correct answer');
  });

  it('marks a correct answer with both labels on the same option', async () => {
    const stub = new ExamsApiStub();
    stub.review = review({ items: [correctQuestion()] });
    const { fixture } = await setup(stub);
    const option = byTestId(fixture, 'exam-review-option');

    expect(byTestId(fixture, 'exam-review-status')?.textContent).toContain('Correct');
    expect(option?.textContent).toContain('Your answer');
    expect(option?.textContent).toContain('Correct answer');
  });

  it('marks an unanswered question without a Your answer label', async () => {
    const stub = new ExamsApiStub();
    stub.review = review({ items: [unansweredQuestion()] });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-review-status')?.textContent).toContain('Unanswered');
    expect(text(fixture)).not.toContain('Your answer');
    expect(text(fixture)).toContain('Correct answer');
  });

  it('renders the backend explanation verbatim and omits the section when blank', async () => {
    const { fixture } = await setup();
    expect(byTestId(fixture, 'exam-review-explanation')?.textContent).toContain(
      'Because safety comes first.',
    );

    const stub = new ExamsApiStub();
    stub.review = review({ items: [unansweredQuestion()] });
    const rerender = await setup(stub);
    expect(byTestId(rerender.fixture, 'exam-review-explanation')).toBeNull();
    expect(text(rerender.fixture)).not.toContain('Explanation');
  });

  it('renders backend points exactly', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'exam-review-points')?.textContent).toContain('Points: 0 of 3');
  });

  it('renders Expired review with Time expired context and normal content', async () => {
    const stub = new ExamsApiStub();
    stub.review = review({ status: 'Expired' });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-review-expired')?.textContent).toContain('Time expired');
    expect(byTestId(fixture, 'exam-review-prompt')?.textContent).toContain('First question');
    expect(byTestId(fixture, 'exam-review-status')?.textContent).toContain('Incorrect');
  });

  it('shows the approved unavailable state on examId mismatch without raw ids', async () => {
    const stub = new ExamsApiStub();
    stub.review = review({ examId: 'other-exam' });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-review-unavailable')?.textContent).toContain(
      "This exam review isn't available.",
    );
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
    expect(text(fixture)).not.toContain('other-exam');
  });

  it('shows the approved unavailable copy on 404', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-review-unavailable')?.textContent).toContain(
      "This exam review isn't available.",
    );
  });

  it('shows the approved not-finalized copy on 409', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 409 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-review-not-finalized')?.textContent).toContain(
      'Finish the exam before reviewing answers.',
    );
    expect(text(fixture)).not.toMatch(/Correct|Incorrect|Your answer|Correct answer/);
  });

  it('retries a generic failure with the same route sessionId', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load this exam review. Try again.");

    stub.error = undefined;
    const retryButton = (fixture.nativeElement as HTMLElement).querySelector(
      '.np-loading-error-retry-retry',
    ) as HTMLButtonElement | null;
    expect(retryButton?.textContent).toContain('Retry');
    retryButton?.click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls).toEqual(['session-9', 'session-9']);
    expect(byTestId(fixture, 'exam-review-status')).not.toBeNull();
  });

  it('links Back to result without browser-history dependence', async () => {
    const { fixture } = await setup();
    const back = byTestId(fixture, 'exam-review-back') as HTMLAnchorElement | null;

    expect(back?.textContent).toContain('Back to result');
    expect(back?.getAttribute('href')).toBe('/exams/exam-1/sessions/session-9/result');
  });

  it('never renders raw ids, aggregates, analytics, or interactive inputs', async () => {
    const { fixture } = await setup();
    const body = text(fixture);
    const element = fixture.nativeElement as HTMLElement;

    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/Score|Percentage|Passed|Strong|Weak|performance band/i);
    expect(element.querySelectorAll('input[type="radio"]')).toHaveLength(0);
    expect(element.querySelectorAll('input')).toHaveLength(0);
  });

  it('renders finalized review in a single card with backend position, status, option labels and explanation', async () => {
    const { fixture } = await setup();
    const root = fixture.nativeElement as HTMLElement;

    expect(root.querySelector('.np-exam-review-header [data-testid="exam-review-title"]')?.textContent).toContain('NCLEX Readiness');
    expect(root.querySelector('.np-exam-review-card-header [data-testid="exam-review-position"]')?.textContent).toContain('Question 1 of 2');
    expect(root.querySelector('.np-exam-review-card-header [data-testid="exam-review-status"]')?.textContent).toContain('Incorrect');
    expect(root.querySelector('.np-exam-review-card-header [data-testid="exam-review-points"]')?.textContent).toContain('0 of 3');
    expect([...root.querySelectorAll('.np-exam-review-options > li')].map((option) => option.textContent?.trim())).toEqual([
      expect.stringContaining('First A'), expect.stringContaining('First B'),
    ]);
    expect(root.querySelector('.np-exam-review-explanation [data-testid="exam-review-explanation"]')?.textContent).toContain('Because safety comes first.');
    expect(root.querySelector('.np-exam-review-card-footer [data-testid="exam-review-next"]')).not.toBeNull();
    expect(root.querySelectorAll('.np-exam-review-card input')).toHaveLength(0);
  });

  it('does not mount review cards or correctness labels for an unfinished 409 session', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 409 };
    const { fixture } = await setup(stub);
    expect(byTestId(fixture, 'exam-review-not-finalized')).not.toBeNull();
    expect((fixture.nativeElement as HTMLElement).querySelector('.np-exam-review-card')).toBeNull();
    expect(text(fixture)).not.toMatch(/Correct answer|Your answer|Explanation/);
  });
});

describe('ExamReview route', () => {
  it('mounts /exams/:examId/sessions/:sessionId/review with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/:examId/sessions/:sessionId/review');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_REVIEW' });
  });
});
