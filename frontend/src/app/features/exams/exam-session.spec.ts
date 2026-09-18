import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamSession, ExamSessionResult } from '../../core/api/exams-api';
import { ExamSessionScreen } from './exam-session';

function question(
  id: string,
  order: number,
  selected: string | null,
  options: { id: string; text: string }[] = [
    { id: `${id}-a`, text: `${id} option one` },
    { id: `${id}-b`, text: `${id} option two` },
  ],
) {
  return {
    examSessionQuestionId: id,
    text: `${id} prompt`,
    points: 1,
    displayOrder: order,
    selectedExamSessionAnswerOptionId: selected,
    options: options.map((option, index) => ({
      examSessionAnswerOptionId: option.id,
      text: option.text,
      displayOrder: index + 1,
    })),
  };
}

function session(overrides: Partial<ExamSession> = {}): ExamSession {
  return {
    id: 'session-9',
    examId: 'exam-1',
    examTitle: 'Exam',
    status: 'InProgress',
    startedAt: '2026-09-18T00:00:00Z',
    expiresAt: '2999-01-01T00:00:00Z',
    remainingSeconds: 3600,
    items: [
      question('q-1', 1, null),
      question('q-2', 2, 'q-2-a'),
    ],
    ...overrides,
  };
}

class ExamsApiStub {
  current: ExamSession = session();
  sessionError: unknown = undefined;
  saved: { sessionId: string; answers: { examSessionQuestionId: string; selectedExamSessionAnswerOptionId: string }[] }[] = [];
  submitCalls: string[] = [];
  submitError: unknown = undefined;
  submitResult: ExamSessionResult = {
    score: 1,
    maxScore: 2,
    percentage: 50,
    passed: false,
    correctCount: 1,
    questionCount: 2,
  };

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
    if (this.sessionError !== undefined) {
      return throwError(() => this.sessionError);
    }
    if (this.submitCalls.length > 0) {
      return of({ ...this.current, status: 'Submitted', remainingSeconds: 0 });
    }
    return of(this.current);
  }

  saveExamSessionAnswers(
    sessionId: string,
    answers: { examSessionQuestionId: string; selectedExamSessionAnswerOptionId: string }[],
  ) {
    this.saved.push({ sessionId, answers });
    const [answer] = answers;
    this.current = {
      ...this.current,
      items: this.current.items.map((item) =>
        item.examSessionQuestionId === answer.examSessionQuestionId
          ? { ...item, selectedExamSessionAnswerOptionId: answer.selectedExamSessionAnswerOptionId }
          : item,
      ),
    };
    return of(this.current);
  }

  submitExamSession(sessionId: string) {
    this.submitCalls.push(sessionId);
    if (this.submitError !== undefined) {
      return throwError(() => this.submitError);
    }
    return of(this.submitResult);
  }
}

async function setup(stub?: ExamsApiStub): Promise<{
  fixture: ComponentFixture<ExamSessionScreen>;
  api: ExamsApiStub;
}> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamSessionScreen],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: { paramMap: convertToParamMap({ examId: 'exam-1', sessionId: 'session-9' }) },
        },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamSessionScreen);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<ExamSessionScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamSessionScreen>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<ExamSessionScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamSession screen (T-FE-069)', () => {
  it('loads by sessionId and resumes at the first unanswered question', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'session-position')?.textContent).toContain('Question 1 of 2');
    expect(byTestId(fixture, 'session-prompt')?.textContent).toContain('q-1 prompt');
  });

  it('restores persisted answers when navigating', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as { next(): void };

    component.next();
    fixture.detectChanges();
    await settle(fixture);

    const checked = (fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="session-option"] input:checked',
    ) as HTMLInputElement | null;
    expect(checked?.value).toBe('q-2-a');
  });

  it('renders one question with backend-ordered options and no correctness leakage', async () => {
    const { fixture } = await setup();
    const element = fixture.nativeElement as HTMLElement;
    const options = [...element.querySelectorAll('[data-testid="session-option"]')].map((o) =>
      o.textContent?.trim(),
    );

    expect(options).toEqual(['q-1 option one', 'q-1 option two']);
    expect(element.querySelectorAll('fieldset').length).toBe(1);
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
    expect(text(fixture)).not.toMatch(/correct answer|answer key|rationale|explanation/i);
  });

  it('selecting an option does not call Save', async () => {
    const { fixture, api } = await setup();

    ((fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="session-option"] input',
    ) as HTMLInputElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.saved).toEqual([]);
    expect(byTestId(fixture, 'session-save')).not.toBeNull();
  });

  it('explicit Save persists the current answer from the authoritative response', async () => {
    const { fixture, api } = await setup();

    ((fixture.nativeElement as HTMLElement).querySelectorAll(
      '[data-testid="session-option"] input',
    )[1] as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'session-save') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.saved).toEqual([
      { sessionId: 'session-9', answers: [{ examSessionQuestionId: 'q-1', selectedExamSessionAnswerOptionId: 'q-1-b' }] },
    ]);
    expect(text(fixture)).toContain('Answer saved.');
  });

  it('re-answer then Save overwrites via the backend contract', async () => {
    const stub = new ExamsApiStub();
    stub.current = session({
      items: [question('q-1', 1, 'q-1-a'), question('q-2', 2, null)],
    });
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { previous(): void };

    expect(byTestId(fixture, 'session-position')?.textContent).toContain('Question 2 of 2');

    component.previous();
    fixture.detectChanges();
    await settle(fixture);

    ((fixture.nativeElement as HTMLElement).querySelectorAll(
      '[data-testid="session-option"] input',
    )[1] as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'session-save') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.saved).toEqual([
      { sessionId: 'session-9', answers: [{ examSessionQuestionId: 'q-1', selectedExamSessionAnswerOptionId: 'q-1-b' }] },
    ]);
  });

  it('Previous and Next change only the displayed question without saving', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as { next(): void; previous(): void };

    component.next();
    fixture.detectChanges();
    await settle(fixture);
    expect(byTestId(fixture, 'session-position')?.textContent).toContain('Question 2 of 2');

    component.previous();
    fixture.detectChanges();
    await settle(fixture);
    expect(byTestId(fixture, 'session-position')?.textContent).toContain('Question 1 of 2');
    expect(api.saved).toEqual([]);
    expect((byTestId(fixture, 'session-prev') as HTMLButtonElement).disabled).toBe(true);
  });

  it('initializes the countdown from RemainingSeconds without announcing every tick', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'session-timer')?.textContent).toContain('60:00');
  });

  it('shows the near-expiry warning immediately when loaded at or below 300 seconds', async () => {
    const stub = new ExamsApiStub();
    stub.current = session({ remainingSeconds: 299 });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'session-expiry-warning')?.textContent).toContain(
      'Time is almost up. Submit your exam soon.',
    );
  });

  it('decrements the displayed countdown on tick without backend calls', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as { tick(): void };
    let loads = 0;
    const original = api.getExamSession.bind(api);
    api.getExamSession = () => {
      loads += 1;
      return original();
    };

    component.tick();
    fixture.detectChanges();

    expect(byTestId(fixture, 'session-timer')?.textContent).toContain('59:59');
    expect(loads).toBe(0);
  });

  it('reconciles through Get on zero without auto-submit and leaves answering on expiry', async () => {
    const stub = new ExamsApiStub();
    let calls = 0;
    const fresh = stub.current;
    stub.getExamSession = () => {
      calls += 1;
      if (calls === 1) {
        return of({ ...fresh, remainingSeconds: 1 });
      }
      return of({ ...fresh, status: 'Expired', remainingSeconds: 0 });
    };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { tick(): void };

    component.tick();
    fixture.detectChanges();
    await settle(fixture);

    expect(calls).toBe(2);
    expect(byTestId(fixture, 'session-terminal-notice')?.textContent).toContain(
      'The exam session has expired.',
    );
    expect(byTestId(fixture, 'session-save')).toBeNull();
    expect(byTestId(fixture, 'session-submit-open')).toBeNull();
    expect(api.submitCalls).toEqual([]);
  });

  it('cleans up the timer on destroy', async () => {
    const cleared: unknown[] = [];
    const originalClear = globalThis.clearInterval;
    globalThis.clearInterval = ((handle?: unknown) => {
      cleared.push(handle);
      return undefined;
    }) as typeof clearInterval;
    try {
      const { fixture } = await setup();
      fixture.destroy();
      expect(cleared.length).toBeGreaterThan(0);
    } finally {
      globalThis.clearInterval = originalClear;
    }
  });

  it('reconciles an expired save without claiming success', async () => {
    const stub = new ExamsApiStub();
    let loads = 0;
    stub.saveExamSessionAnswers = () => throwError(() => ({ status: 409 }));
    stub.getExamSession = () => {
      loads += 1;
      if (loads === 1) {
        return of(stub.current);
      }
      return of(session({ status: 'Expired', remainingSeconds: 0 }));
    };
    const { fixture } = await setup(stub);

    ((fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="session-option"] input',
    ) as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'session-save') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(byTestId(fixture, 'session-terminal-notice')).not.toBeNull();
    expect(text(fixture)).not.toContain('Answer saved.');
  });

  it('reconciles expiry to the factual expired state without auto-submit', async () => {
    const stub = new ExamsApiStub();
    stub.current = session({ status: 'Expired', remainingSeconds: 0 });
    const { fixture, api } = await setup(stub);

    expect(byTestId(fixture, 'session-terminal-notice')).not.toBeNull();
    expect(byTestId(fixture, 'session-save')).toBeNull();
    expect(byTestId(fixture, 'session-submit-open')).toBeNull();
    expect(api.saved).toEqual([]);
    expect(api.submitCalls).toEqual([]);
  });

  it('submit confirmation counts only persisted answers and Cancel sends zero POST', async () => {
    const { fixture, api } = await setup();

    ((fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="session-option"] input',
    ) as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'session-submit-open') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(byTestId(fixture, 'session-confirm')).not.toBeNull();
    expect(text(fixture)).toContain('1 questions are unanswered');

    (byTestId(fixture, 'session-confirm-cancel') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.submitCalls).toEqual([]);
  });

  it('confirming submit posts once and renders backend transient aggregates', async () => {
    const { fixture, api } = await setup();

    (byTestId(fixture, 'session-submit-open') as HTMLButtonElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'session-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.submitCalls).toEqual(['session-9']);
    expect(byTestId(fixture, 'session-result')).not.toBeNull();
    expect(text(fixture)).toContain('Not passed');
    expect(text(fixture)).not.toMatch(/correct answer|answer key|rationale|explanation/i);
  });

  it('shows a contextual missing notice on 404', async () => {
    const stub = new ExamsApiStub();
    stub.sessionError = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'session-missing-notice')).not.toBeNull();
    expect(byTestId(fixture, 'session-back-link')).not.toBeNull();
  });

  it('renders a factual terminal state for a directly loaded submitted session', async () => {
    const stub = new ExamsApiStub();
    stub.current = session({ status: 'Submitted', remainingSeconds: 0 });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'session-terminal-notice')).not.toBeNull();
    expect(byTestId(fixture, 'session-save')).toBeNull();
    expect(byTestId(fixture, 'session-submit-open')).toBeNull();
    expect(byTestId(fixture, 'session-result')).toBeNull();
    expect(byTestId(fixture, 'session-timer')).toBeNull();
  });
});

describe('ExamSession route', () => {
  it('mounts /exams/:examId/sessions/:sessionId with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/:examId/sessions/:sessionId');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_SESSION' });
  });
});
