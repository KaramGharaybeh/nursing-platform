import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { routes } from '../../app.routes';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamAttemptDto } from '../../core/api/generated/models/exam-attempt-dto';
import type { ExamDetail } from '../../core/api/exams-api';
import { ExamInstructions } from './exam-instructions';

const EXAM: ExamDetail = {
  id: 'exam-1',
  title: 'NCLEX Readiness',
  description: 'Are you ready?',
  instructions: 'Answer every question carefully.',
  countryId: 'country-1',
  countryName: 'Jordan',
  categoryId: 'cat-1',
  categoryName: 'Licensure',
  durationMinutes: 120,
  questionCount: 75,
  passingScorePercentage: 70,
  isFree: true,
  canStart: true,
};

const PAID_LOCKED: ExamDetail = { ...EXAM, isFree: false, canStart: false };

function attempt(): ExamAttemptDto {
  return {
    id: 'sess-1',
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'InProgress',
    startedAt: '2026-09-18T00:00:00Z',
    expiresAt: '2999-01-01T00:00:00Z',
    finalizedAt: null,
    score: null,
    maxScore: null,
    percentage: null,
    passed: null,
  };
}

class ExamsApiStub {
  exam: ExamDetail = EXAM;
  examError: unknown = undefined;
  resumable: ExamAttemptDto | undefined = undefined;
  attemptsError: unknown = undefined;
  attemptsCalls = 0;
  session = { sessionId: 'session-9', examId: 'exam-1' };
  startError: unknown = undefined;
  startCalls: { examId: string }[] = [];

  listExams() {
    return throwError(() => ({ status: 500 }));
  }

  getExam() {
    if (this.examError !== undefined) {
      return throwError(() => this.examError);
    }
    return of(this.exam);
  }

  listCountries() {
    return of([]);
  }

  findResumableAttempt() {
    this.attemptsCalls += 1;
    if (this.attemptsError !== undefined) {
      return Promise.reject(this.attemptsError);
    }
    return Promise.resolve(this.resumable);
  }

  startExamSession(examId: string) {
    this.startCalls.push({ examId });
    if (this.startError !== undefined) {
      return throwError(() => this.startError);
    }
    return of(this.session);
  }
}

async function setup(stub?: ExamsApiStub): Promise<{
  fixture: ComponentFixture<ExamInstructions>;
  api: ExamsApiStub;
  navigateSpy: Mock;
}> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamInstructions],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({ examId: 'exam-1' }) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamInstructions);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<ExamInstructions>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamInstructions>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<ExamInstructions>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamInstructions (T-FE-068)', () => {
  it('loads by examId and renders title, verbatim instructions, and facts', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('NCLEX Readiness');
    expect(content).toContain('Answer every question carefully.');
    expect(content).toContain('120 minutes');
    expect(content).toContain('75 questions');
    expect(content).toContain('70');
    expect(content).not.toMatch(UUID_PATTERN);
  });

  it('shows the packet fallback when instructions are empty', async () => {
    const stub = new ExamsApiStub();
    stub.exam = { ...EXAM, instructions: null };
    const { fixture } = await setup(stub);

    expect(text(fixture)).toContain('No instructions were supplied for this exam.');
  });

  it('shows Requires purchase with no action for a paid inaccessible exam', async () => {
    const stub = new ExamsApiStub();
    stub.exam = PAID_LOCKED;
    const { fixture, api } = await setup(stub);
    const content = text(fixture);

    expect(byTestId(fixture, 'instructions-requires-purchase')?.textContent).toContain(
      'Requires purchase',
    );
    expect(byTestId(fixture, 'instructions-start')).toBeNull();
    expect(byTestId(fixture, 'instructions-resume')).toBeNull();
    expect(api.attemptsCalls).toBe(0);
    expect(content).not.toMatch(/checkout|buy now|pay now|price|cart/i);
  });

  it('shows Start exam when startable with no resumable attempt', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'instructions-start')?.textContent).toContain('Start exam');
    expect(byTestId(fixture, 'instructions-resume')).toBeNull();
  });

  it('shows Resume exam only from authoritative attempt truth', async () => {
    const stub = new ExamsApiStub();
    stub.resumable = attempt();
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'instructions-resume')?.textContent).toContain('Resume exam');
    expect(byTestId(fixture, 'instructions-start')).toBeNull();
  });

  it('does not flash Start while resume detection is loading', async () => {
    const stub = new ExamsApiStub();
    let resolveAttempts!: (value: ExamAttemptDto | undefined) => void;
    stub.findResumableAttempt = () => {
      stub.attemptsCalls += 1;
      return new Promise((resolve) => {
        resolveAttempts = resolve;
      });
    };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'instructions-start')).toBeNull();
    expect(byTestId(fixture, 'instructions-resume')).toBeNull();
    expect(byTestId(fixture, 'instructions-resume-loading')).not.toBeNull();

    resolveAttempts(undefined);
    await settle(fixture);

    expect(byTestId(fixture, 'instructions-start')).not.toBeNull();
  });

  it('shows a retryable state when attempt detection fails instead of downgrading to Start', async () => {
    const stub = new ExamsApiStub();
    stub.attemptsError = { status: 500 };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retryAttempts(): Promise<void> };

    expect(byTestId(fixture, 'instructions-start')).toBeNull();
    expect(byTestId(fixture, 'instructions-resume-error')).not.toBeNull();

    api.attemptsError = undefined;
    await component.retryAttempts();
    await settle(fixture);

    expect(api.attemptsCalls).toBe(2);
    expect(byTestId(fixture, 'instructions-start')).not.toBeNull();
  });

  it('opens Start confirmation and Cancel sends zero POST', async () => {
    const { fixture, api } = await setup();

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(byTestId(fixture, 'instructions-confirm')).not.toBeNull();
    expect(text(fixture)).toContain('timed exam session');

    (byTestId(fixture, 'instructions-cancel') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual([]);
    expect(byTestId(fixture, 'instructions-confirm')).toBeNull();
  });

  it('confirming Start posts once and navigates with the POST session id', async () => {
    const { fixture, api, navigateSpy } = await setup();

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'instructions-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual([{ examId: 'exam-1' }]);
    expect(navigateSpy).toHaveBeenCalledWith('/exams/exam-1/sessions/session-9');
  });

  it('blocks repeated activation while the start request is pending', async () => {
    const stub = new ExamsApiStub();
    const pending = new Subject<{ sessionId: string; examId: string }>();
    stub.startExamSession = (examId: string) => {
      stub.startCalls.push({ examId });
      return pending.asObservable();
    };
    const { fixture, api } = await setup(stub);

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    const go = byTestId(fixture, 'instructions-confirm-go') as HTMLButtonElement;
    go.click();
    go.click();
    fixture.detectChanges();

    expect(api.startCalls).toEqual([{ examId: 'exam-1' }]);

    pending.next({ sessionId: 'session-9', examId: 'exam-1' });
    pending.complete();
    await settle(fixture);

    expect(api.startCalls).toEqual([{ examId: 'exam-1' }]);
  });

  it('confirming Resume uses the same StartExamSession operation', async () => {
    const stub = new ExamsApiStub();
    stub.resumable = attempt();
    const { fixture, api, navigateSpy } = await setup(stub);

    (byTestId(fixture, 'instructions-resume') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(text(fixture)).toContain('Resuming the exam');

    (byTestId(fixture, 'instructions-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual([{ examId: 'exam-1' }]);
    expect(navigateSpy).toHaveBeenCalledWith('/exams/exam-1/sessions/session-9');
  });

  it('shows a safe missing notice with back link on 404', async () => {
    const stub = new ExamsApiStub();
    stub.examError = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'instructions-missing-notice')).not.toBeNull();
    expect(byTestId(fixture, 'instructions-back-link')).not.toBeNull();
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
  });

  it('reconciles safely when start reports a conflict', async () => {
    const stub = new ExamsApiStub();
    stub.startError = { status: 409 };
    const { fixture, api } = await setup(stub);

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'instructions-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls.length).toBe(1);
    expect(byTestId(fixture, 'instructions-start-error')).not.toBeNull();
    expect(text(fixture)).not.toMatch(/409|package-attempt-consumed/i);
  });

  it('exposes no restricted learner content', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).not.toMatch(UUID_PATTERN);
    expect(content).not.toMatch(/correct answer|answer key|rationale|explanation/i);
  });

  it('renders the accepted context, three backend fact cards and instructions panel', async () => {
    const { fixture } = await setup();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('h1')?.textContent).toContain('Instructions');
    expect(root.querySelector('.np-exam-instructions-context')?.textContent).toContain('NCLEX Readiness');
    expect(root.querySelectorAll('.np-exam-instructions-facts > div')).toHaveLength(3);
    expect(root.querySelector('.np-exam-instructions-panel h2')?.textContent).toContain('Candidate instructions');
    expect(root.querySelector('.np-exam-instructions-panel')?.textContent).toContain('Answer every question carefully.');
    expect(root.querySelector('[data-testid="instructions-confirm"]')).toBeNull();
  });

  it('shows the resume notice and only the backend-authorized resume action', async () => {
    const stub = new ExamsApiStub();
    stub.resumable = attempt();
    const { fixture } = await setup(stub);
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('.np-exam-instructions-resume-notice')).not.toBeNull();
    expect(byTestId(fixture, 'instructions-resume')).not.toBeNull();
    expect(byTestId(fixture, 'instructions-start')).toBeNull();
  });

  it('shows the timed inline confirmation only after action, without duplicate Start and Resume actions', async () => {
    const { fixture } = await setup();
    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('.np-exam-instructions-confirmation')).not.toBeNull();
    expect(byTestId(fixture, 'instructions-start')).toBeNull();
    expect(byTestId(fixture, 'instructions-confirm-go')).not.toBeNull();
  });

  it('opens Start confirmation as a modal dialog above inert background with zero POST', async () => {
    const { fixture, api } = await setup();

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    const dialog = byTestId(fixture, 'instructions-confirm') as HTMLElement;
    expect(dialog.getAttribute('role')).toBe('dialog');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(dialog.getAttribute('aria-labelledby')).toBe('instructions-confirm-title');
    expect(byTestId(fixture, 'instructions-body')?.hasAttribute('inert')).toBe(true);
    expect(byTestId(fixture, 'instructions-confirm-backdrop')).not.toBeNull();
    expect(api.startCalls).toEqual([]);
  });

  it('Cancel closes the modal without mutation and restores Start trigger focus', async () => {
    const { fixture, api } = await setup();

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);
    expect(document.activeElement).toBe(byTestId(fixture, 'instructions-cancel'));

    (byTestId(fixture, 'instructions-cancel') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual([]);
    expect(byTestId(fixture, 'instructions-confirm')).toBeNull();
    expect(byTestId(fixture, 'instructions-body')?.hasAttribute('inert')).toBe(false);
    expect(document.activeElement).toBe(byTestId(fixture, 'instructions-start'));
  });

  it('Escape closes the modal without submitting', async () => {
    const { fixture, api } = await setup();

    (byTestId(fixture, 'instructions-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);
    expect(byTestId(fixture, 'instructions-confirm')).not.toBeNull();

    (byTestId(fixture, 'instructions-confirm') as HTMLElement).dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }),
    );
    fixture.detectChanges();
    await settle(fixture);

    expect(byTestId(fixture, 'instructions-confirm')).toBeNull();
    expect(api.startCalls).toEqual([]);
    expect(document.activeElement).toBe(byTestId(fixture, 'instructions-start'));
  });

  it('Resume opens the modal with resume copy and Cancel restores the Resume trigger', async () => {
    const stub = new ExamsApiStub();
    stub.resumable = attempt();
    const { fixture, api } = await setup(stub);

    (byTestId(fixture, 'instructions-resume') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    const dialog = byTestId(fixture, 'instructions-confirm') as HTMLElement;
    expect(dialog.getAttribute('role')).toBe('dialog');
    expect(text(fixture)).toContain('Resuming the exam');
    expect(api.startCalls).toEqual([]);

    (byTestId(fixture, 'instructions-cancel') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(byTestId(fixture, 'instructions-confirm')).toBeNull();
    expect(api.startCalls).toEqual([]);
    expect(document.activeElement).toBe(byTestId(fixture, 'instructions-resume'));
  });
});

describe('ExamInstructions route', () => {
  it('mounts /exams/:examId/instructions with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/:examId/instructions');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_INSTRUCTIONS' });
  });
});
