import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { vi, type Mock } from 'vitest';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { CountriesApi } from '../../core/api/countries-api';
import { ExamsApi } from '../../core/api/exams-api';
import type {
  ExamAnalyticsByCategoryItem,
  ExamAnalyticsByExamItem,
  ExamAnalyticsSummary,
  ExamAnalyticsTrendPoint,
} from '../../core/api/exams-api';
import { ExamAnalyticsScreen } from './exam-analytics';

function summary(overrides: Partial<ExamAnalyticsSummary> = {}): ExamAnalyticsSummary {
  return {
    attemptCount: 3,
    submittedCount: 2,
    expiredCount: 1,
    inProgressCount: 0,
    passedCount: 2,
    failedCount: 1,
    passRate: 66.67,
    averageScorePercentage: 80,
    bestScorePercentage: 100,
    latestScorePercentage: null,
    ...overrides,
  };
}

function byExamRow(overrides: Partial<ExamAnalyticsByExamItem> = {}): ExamAnalyticsByExamItem {
  return {
    examTitle: 'Second exam',
    attemptCount: 2,
    passRate: 50,
    averageScorePercentage: 70,
    bestScorePercentage: 90,
    latestScorePercentage: 60,
    ...overrides,
  };
}

function byCategoryRow(
  overrides: Partial<ExamAnalyticsByCategoryItem> = {},
): ExamAnalyticsByCategoryItem {
  return {
    categoryId: 'cat-1',
    categoryName: 'Licensure',
    attemptCount: 3,
    passRate: 66.67,
    averageScorePercentage: 82,
    bestScorePercentage: 100,
    ...overrides,
  };
}

function trendPoint(overrides: Partial<ExamAnalyticsTrendPoint> = {}): ExamAnalyticsTrendPoint {
  return {
    bucketStart: '2026-01-01T00:00:00Z',
    bucketEnd: '2026-02-01T00:00:00Z',
    attemptCount: 1,
    averageScorePercentage: 90,
    passRate: 100,
    ...overrides,
  };
}

class ExamsApiStub {
  summaryResult: ExamAnalyticsSummary = summary();
  examItems: ExamAnalyticsByExamItem[] = [
    byExamRow(),
    byExamRow({ examTitle: 'First exam', attemptCount: 1, passRate: 100 }),
  ];
  categoryItems: ExamAnalyticsByCategoryItem[] = [byCategoryRow()];
  trendPoints: ExamAnalyticsTrendPoint[] = [
    trendPoint(),
    trendPoint({ bucketStart: '2026-02-01T00:00:00Z', bucketEnd: '2026-03-01T00:00:00Z' }),
  ];
  summaryError: unknown = undefined;
  examError: unknown = undefined;
  categoryError: unknown = undefined;
  trendsError: unknown = undefined;
  calls: { op: string; filters: unknown; page?: number }[] = [];

  listExams() {
    return throwError(() => ({ status: 500 }));
  }

  getExam() {
    return throwError(() => ({ status: 404 }));
  }

  listCountries() {
    return of([]);
  }

  getExamAnalyticsSummary(filters: unknown) {
    this.calls.push({ op: 'summary', filters });
    if (this.summaryError !== undefined) {
      return throwError(() => this.summaryError);
    }
    return of(this.summaryResult);
  }

  listExamAnalyticsByExam(filters: unknown, page: number) {
    this.calls.push({ op: 'by-exam', filters, page });
    if (this.examError !== undefined) {
      return throwError(() => this.examError);
    }
    return of({ items: this.examItems, page, pageSize: 20, totalCount: 3, totalPages: 2 });
  }

  listExamAnalyticsByCategory(filters: unknown, page: number) {
    this.calls.push({ op: 'by-category', filters, page });
    if (this.categoryError !== undefined) {
      return throwError(() => this.categoryError);
    }
    return of({ items: this.categoryItems, page, pageSize: 20, totalCount: 1, totalPages: 1 });
  }

  listExamAnalyticsTrends(filters: unknown) {
    this.calls.push({ op: 'trends', filters });
    if (this.trendsError !== undefined) {
      return throwError(() => this.trendsError);
    }
    return of(this.trendPoints);
  }
}

class CountriesApiStub {
  calls = 0;

  list() {
    this.calls += 1;
    return of([{ id: 'c-1', name: 'Jordan', code: 'JO' }]);
  }
}

async function setup(
  stub?: ExamsApiStub,
  queryParams: Record<string, string> = {},
): Promise<{
  fixture: ComponentFixture<ExamAnalyticsScreen>;
  api: ExamsApiStub;
  navigateSpy: Mock;
}> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamAnalyticsScreen],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      { provide: CountriesApi, useValue: new CountriesApiStub() },
      Router,
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: {
            paramMap: convertToParamMap({}),
            queryParamMap: convertToParamMap(queryParams),
          },
        },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamAnalyticsScreen);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<ExamAnalyticsScreen>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamAnalyticsScreen>): string {
  return (fixture.nativeElement as HTMLElement).textContent ?? '';
}

function byTestId(fixture: ComponentFixture<ExamAnalyticsScreen>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamAnalytics screen (T-FE-074)', () => {
  it('loads all four sections on init with heading and supporting copy', async () => {
    const { fixture, api } = await setup();

    expect(api.calls.map((call) => call.op)).toEqual([
      'summary',
      'by-exam',
      'by-category',
      'trends',
    ]);
    expect(byTestId(fixture, 'exam-analytics-heading')?.textContent).toContain('Exam analytics');
    expect(text(fixture)).toContain('Review your exam performance over time.');
  });

  it('renders overview counts verbatim with null metrics as Not available', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'analytics-total-attempts')?.textContent).toContain('3');
    expect(byTestId(fixture, 'analytics-submitted')?.textContent).toContain('2');
    expect(byTestId(fixture, 'analytics-pass-rate')?.textContent).toContain('66.67%');
    expect(byTestId(fixture, 'analytics-latest-score')?.textContent).toContain('Not available');
  });

  it('shows the page-level empty state when attemptCount is 0 without section tables', async () => {
    const stub = new ExamsApiStub();
    stub.summaryResult = summary({ attemptCount: 0 });
    const { fixture, api } = await setup(stub);

    expect(byTestId(fixture, 'analytics-empty')?.textContent).toContain('No exam analytics yet');
    expect(text(fixture)).toContain('Complete an exam to see your analytics.');
    expect(api.calls.map((call) => call.op)).toEqual(['summary']);
  });

  it('renders by-exam rows in backend order with pagination metadata', async () => {
    const { fixture } = await setup();
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll(
      '[data-testid="analytics-exam-row"]',
    );

    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Second exam');
    expect(byTestId(fixture, 'analytics-exam-pagination')).not.toBeNull();
  });

  it('reloads only by-exam on page change keeping filters and other sections', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as { loadExamPage(page: number): void };
    const before = api.calls.length;

    component.loadExamPage(2);
    fixture.detectChanges();
    await settle(fixture);

    const after = api.calls.slice(before);
    expect(after).toEqual([{ op: 'by-exam', filters: {}, page: 2 }]);
  });

  it('renders trends chronologically without charts or bands', async () => {
    const { fixture } = await setup();
    const points = (fixture.nativeElement as HTMLElement).querySelectorAll(
      '[data-testid="analytics-trend-point"]',
    );

    expect(points.length).toBe(2);
    expect(byTestId(fixture, 'analytics-trends')).not.toBeNull();
    expect(text(fixture)).not.toMatch(/Excellent|Good|Weak|Strong|Mastered|grade|tier/i);
    expect(
      byTestId(fixture, 'analytics-trends')?.querySelectorAll('svg,canvas').length ?? -1,
    ).toBe(0);
  });

  it('changing draft filters causes zero analytics reloads until Apply', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      setDraftFilters(filters: Record<string, string>): void;
    };
    const before = api.calls.length;

    component.setDraftFilters({ from: '2026-01-01', countryId: 'c-1' });
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls.length).toBe(before);
  });

  it('shows the exact date-range error with zero analytics calls', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      setDraftFilters(filters: Record<string, string>): void;
      applyFilters(): void;
    };
    const before = api.calls.length;

    component.setDraftFilters({ from: '2026-03-01', to: '2026-01-01' });
    component.applyFilters();
    fixture.detectChanges();
    await settle(fixture);

    expect(text(fixture)).toContain('From date must be on or before To date.');
    expect(api.calls.length).toBe(before);
  });

  it('Apply reloads all sections, resets pages, and updates query params', async () => {
    const { fixture, api, navigateSpy } = await setup();
    const component = fixture.componentInstance as unknown as {
      setDraftFilters(filters: Record<string, string>): void;
      applyFilters(): void;
    };

    component.setDraftFilters({ from: '2026-01-01', countryId: 'c-1' });
    component.applyFilters();
    fixture.detectChanges();
    await settle(fixture);

    const ops = api.calls.map((call) => call.op);
    expect(ops.slice(-4)).toEqual(['summary', 'by-exam', 'by-category', 'trends']);
    expect(api.calls.filter((call) => call.op === 'by-exam').at(-1)).toEqual({
      op: 'by-exam',
      filters: { from: '2026-01-01', countryId: 'c-1' },
      page: 1,
    });
    expect(navigateSpy.mock.calls.length).toBeGreaterThan(0);
    expect(JSON.stringify(navigateSpy.mock.calls.at(-1))).toContain('countryId');
  });

  it('initializes draft and applied filters from query params', async () => {
    const { fixture, api } = await setup(new ExamsApiStub(), {
      from: '2026-01-01',
      categoryId: 'cat-1',
    });

    expect(text(fixture)).toContain('Licensure');
    expect(api.calls[0]).toEqual({
      op: 'summary',
      filters: { from: '2026-01-01', categoryId: 'cat-1' },
    });
  });

  it('shows section sparse copy without failing the page', async () => {
    const stub = new ExamsApiStub();
    stub.examItems = [];
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'analytics-total-attempts')?.textContent).toContain('3');
    expect(text(fixture)).toContain('No data is available for these filters.');
  });

  it('shows page-level retryable error when the summary fails', async () => {
    const stub = new ExamsApiStub();
    stub.summaryError = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load your exam analytics. Try again.");
    expect(api.calls.map((call) => call.op)).toEqual(['summary']);
  });

  it('shows section error with section retry reloading only that section', async () => {
    const stub = new ExamsApiStub();
    stub.categoryError = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(text(fixture)).toContain("We couldn't load category analytics. Try again.");
    expect(byTestId(fixture, 'analytics-total-attempts')).not.toBeNull();

    stub.categoryError = undefined;
    const before = api.calls.length;
    const retry = (fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="analytics-category-retry"]',
    ) as HTMLButtonElement | null;
    retry?.click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.calls.slice(before)).toEqual([{ op: 'by-category', filters: {}, page: 1 }]);
  });

  it('links Back to exams and exposes no raw ids, review, package, or band content', async () => {
    const { fixture } = await setup();
    const body = text(fixture);
    const back = byTestId(fixture, 'analytics-back') as HTMLAnchorElement | null;

    expect(back?.textContent).toContain('Back to exams');
    expect(back?.getAttribute('href')).toBe('/exams');
    expect(body).not.toMatch(UUID_PATTERN);
    expect(body).not.toMatch(/Your answer|Correct answer|Explanation|Recommended|topic guidance/i);
    expect(body).not.toMatch(/Excellent|Good|Weak|Strong|Mastered/i);
  });
});

describe('ExamAnalytics route', () => {
  it('registers static /exams/analytics before the dynamic :examId route', () => {
    const paths = routes.map((entry) => entry.path);

    expect(paths).toContain('exams/analytics');
    expect(paths.indexOf('exams/analytics')).toBeLessThan(paths.indexOf('exams/:examId'));
  });

  it('mounts /exams/analytics with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/analytics');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_ANALYTICS' });
  });
});
