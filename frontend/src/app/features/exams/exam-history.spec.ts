import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { routes } from '../../app.routes';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamHistoryAttempt } from '../../core/api/exams-api';
import { ExamHistoryScreen } from './exam-history';

function inProgressAttempt(): ExamHistoryAttempt {
  return {
    sessionId: 'session-7',
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'InProgress',
    startedAt: '2026-09-18T00:00:00Z',
    expiresAt: '2026-09-18T01:00:00Z',
    score: null,
    maxScore: null,
    percentage: null,
    passed: null,
  };
}

function submittedAttempt(): ExamHistoryAttempt {
  return {
    sessionId: 'session-9',
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'Submitted',
    startedAt: '2026-09-17T00:00:00Z',
    expiresAt: '2026-09-17T01:00:00Z',
    score: 68,
    maxScore: 75,
    percentage: 90.67,
    passed: true,
  };
}

function expiredAttempt(): ExamHistoryAttempt {
  return {
    sessionId: 'session-8',
    examId: 'exam-2',
    examTitle: 'Second exam',
    status: 'Expired',
    startedAt: '2026-09-16T00:00:00Z',
    expiresAt: '2026-09-16T01:00:00Z',
    score: 40,
    maxScore: 75,
    percentage: 53.33,
    passed: false,
  };
}

class ExamsApiStub {
  items: ExamHistoryAttempt[] = [inProgressAttempt(), submittedAttempt(), expiredAttempt()];
  error: unknown = undefined;
  calls: { status?: number; page: number }[] = [];

  listExams() {
    return throwError(() => ({ status: 500 }));
  }

  getExam() {
    return throwError(() => ({ status: 404 }));
  }

  listCountries() {
    return of([]);
  }

  listExamHistory(filters: { status?: number }, page: number) {
    this.calls.push({ ...filters, page });
    if (this.error !== undefined) {
      return throwError(() => this.error);
    }
    return of({ items: this.items, page, pageSize: 20, totalCount: 3, totalPages: 1 });
  }
}

async function setup(
  stub?: ExamsApiStub,
  queryParams: Record<string, string> = {},
): Promise<{
  fixture: ComponentFixture<ExamHistoryScreen>;
  api: ExamsApiStub;
  navigateSpy: Mock;
}> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamHistoryScreen],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: {
            paramMap: convertToParamMap({}),
            queryParamMap: convertToParamMap(queryParams),
          },
        },
      },
      Router,
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamHistoryScreen);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<ExamHistoryScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamHistoryScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<ExamHistoryScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

function allByTestId(fixture: ComponentFixture<ExamHistoryScreen>, id: string): HTMLElement[] {
  return [
    ...(fixture.nativeElement as HTMLElement).querySelectorAll(
      `[data-testid="${id}"]`,
    ),
  ] as HTMLElement[];
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamHistory screen (T-FE-073)', () => {
  it('loads unfiltered history on init with heading and supporting copy', async () => {
    const { fixture, api } = await setup();

    expect(api.calls).toEqual([{ page: 1 }]);
    expect(byTestId(fixture, 'exam-history-heading')?.textContent).toContain('Exam history');
    expect(text(fixture)).toContain('Review your current and completed exam attempts.');
  });

  it('renders rows in backend order with title, status, and started date', async () => {
    const { fixture } = await setup();
    const rows = allByTestId(fixture, 'exam-history-row');

    expect(rows.length).toBe(3);
    expect(rows[0].textContent).toContain('NCLEX Readiness');
    expect(rows[0].textContent).toContain('In progress');
    expect(rows[0].textContent).toContain('2026');
    expect(rows[1].textContent).toContain('Completed');
    expect(rows[2].textContent).toContain('Time expired');
  });

  it('shows Ends with expiry for InProgress and percentage/result for finalized rows', async () => {
    const { fixture } = await setup();
    const rows = allByTestId(fixture, 'exam-history-row');

    expect(rows[0].textContent).toContain('Ends');
    expect(rows[1].textContent).toContain('90.67%');
    expect(rows[1].textContent).toContain('Passed');
    expect(rows[2].textContent).toContain('Not passed');
    expect(rows[0].textContent).not.toContain('Passed');
  });

  it('links InProgress rows to Resume exam without start POSTs', async () => {
    const { fixture, api } = await setup();
    const resume = byTestId(fixture, 'exam-history-resume') as HTMLAnchorElement | null;

    expect(resume?.textContent).toContain('Resume exam');
    expect(resume?.getAttribute('href')).toBe('/exams/exam-1/sessions/session-7');
    expect(api.calls).toEqual([{ page: 1 }]);
  });

  it('links Submitted rows to View result and Review answers', async () => {
    const { fixture } = await setup();
    const rows = allByTestId(fixture, 'exam-history-row');

    const resultLink = rows[1].querySelector(
      '[data-testid="exam-history-result"]',
    ) as HTMLAnchorElement | null;
    const reviewLink = rows[1].querySelector(
      '[data-testid="exam-history-review"]',
    ) as HTMLAnchorElement | null;
    expect(resultLink?.getAttribute('href')).toBe('/exams/exam-1/sessions/session-9/result');
    expect(reviewLink?.getAttribute('href')).toBe('/exams/exam-1/sessions/session-9/review');
  });

  it('links Expired rows to View result and Review answers', async () => {
    const { fixture } = await setup();
    const rows = allByTestId(fixture, 'exam-history-row');

    expect(
      rows[2]
        .querySelector('[data-testid="exam-history-result"]')
        ?.getAttribute('href'),
    ).toBe('/exams/exam-2/sessions/session-8/result');
    expect(
      rows[2]
        .querySelector('[data-testid="exam-history-review"]')
        ?.getAttribute('href'),
    ).toBe('/exams/exam-2/sessions/session-8/review');
  });

  it('applies the Completed filter immediately resetting pagination and URL', async () => {
    const { fixture, api, navigateSpy } = await setup();
    const component = fixture.componentInstance as unknown as {
      applyStatusFilter(value: string): void;
    };

    component.applyStatusFilter('Submitted');
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls.at(-1)).toEqual({ status: 1, page: 1 });
    expect(navigateSpy.mock.calls.length).toBeGreaterThan(0);
    expect(JSON.stringify(navigateSpy.mock.calls.at(-1))).toContain('Submitted');
  });

  it('initializes filter and page from query params', async () => {
    const { fixture, api } = await setup(new ExamsApiStub(), {
      status: 'Expired',
      page: '2',
    });

    expect(api.calls[0]).toEqual({ status: 2, page: 2 });
    expect(text(fixture)).toContain('Time expired');
  });

  it('normalizes invalid query status and page', async () => {
    const { api } = await setup(new ExamsApiStub(), { status: 'Bogus', page: '0' });

    expect(api.calls[0]).toEqual({ page: 1 });
  });

  it('reloads only history on page change preserving the filter', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      applyStatusFilter(value: string): void;
      loadHistoryPage(page: number): void;
    };

    component.applyStatusFilter('InProgress');
    fixture.detectChanges();
    await settle(fixture);
    const before = api.calls.length;
    component.loadHistoryPage(3);
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls.slice(before)).toEqual([{ status: 0, page: 3 }]);
  });

  it('shows the page-level empty state for unfiltered zero attempts', async () => {
    const stub = new ExamsApiStub();
    stub.items = [];
    const { fixture, api } = await setup(stub);

    expect(byTestId(fixture, 'exam-history-empty')?.textContent).toContain('No exam attempts yet');
    expect(text(fixture)).toContain('Start an exam to see your history.');
    expect(api.calls).toEqual([{ page: 1 }]);
  });

  it('shows the filtered empty state with Clear filter restoring All', async () => {
    const stub = new ExamsApiStub();
    stub.items = [];
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as {
      applyStatusFilter(value: string): void;
      clearFilter(): void;
    };

    component.applyStatusFilter('Expired');
    fixture.detectChanges();
    await settle(fixture);
    expect(text(fixture)).toContain('No exam attempts match this filter');

    component.clearFilter();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls.at(-1)).toEqual({ page: 1 });
    expect(text(fixture)).toContain('No exam attempts yet');
  });

  it('shows the exact error copy with same-state retry', async () => {
    const stub = new ExamsApiStub();
    stub.error = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load your exam history. Try again.");

    stub.error = undefined;
    const retry = (fixture.nativeElement as HTMLElement).querySelector(
      '.np-loading-error-retry-retry',
    ) as HTMLButtonElement | null;
    retry?.click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls).toEqual([{ page: 1 }, { page: 1 }]);
  });

  it('links Back to exams and exposes no raw ids, source, or review/analytics content', async () => {
    const { fixture } = await setup();
    const body = text(fixture);
    const back = byTestId(fixture, 'exam-history-back') as HTMLAnchorElement | null;

    expect(back?.textContent).toContain('Back to exams');
    expect(back?.getAttribute('href')).toBe('/exams');
    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/Your answer|Correct answer|Explanation|Recommended|Pass rate|Average score/i);
  });
});

describe('ExamHistory route', () => {
  it('mounts /exams/history with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/history');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_HISTORY' });
  });

  it('registers static /exams/history before the dynamic :examId route', () => {
    const paths = routes.map((entry) => entry.path);

    expect(paths).toContain('exams/history');
    expect(paths.indexOf('exams/history')).toBeLessThan(paths.indexOf('exams/:examId'));
  });
});
