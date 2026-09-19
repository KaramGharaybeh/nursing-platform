import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CountriesApi } from '../../core/api/countries-api';
import { ExamsApi } from '../../core/api/exams-api';
import type {
  ExamAnalyticsByCategoryItem,
  ExamAnalyticsByExamItem,
  ExamAnalyticsFilters,
  ExamAnalyticsPage,
  ExamAnalyticsSummary,
  ExamAnalyticsTrendPoint,
} from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { canonicalRoutePath } from '../../core/routing/canonical-routes';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { NpDateControl, NpSelectControl } from '../../shared/ui/form-controls';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { NpPagination } from '../../shared/ui/pagination';

type SectionState = 'loading' | 'ready' | 'error';

interface DraftFilters {
  readonly from: string;
  readonly to: string;
  readonly countryId: string;
  readonly categoryId: string;
}

const NO_FILTER = '';
const EMPTY_FILTERS: DraftFilters = { from: '', to: '', countryId: '', categoryId: '' };
const GENERIC_RETRY_COPY = "We couldn't load your exam analytics. Try again.";

@Component({
  selector: 'np-exam-analytics',
  imports: [
    DatePipe,
    LoadingErrorRetry,
    NpDateControl,
    NpEmptyState,
    NpPagination,
    NpSelectControl,
    RouterLink,
  ],
  templateUrl: './exam-analytics.html',
  styleUrl: './exam-analytics.scss',
})
export class ExamAnalyticsScreen implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly summary = signal<ExamAnalyticsSummary | undefined>(undefined);
  protected readonly examPage = signal<ExamAnalyticsPage<ExamAnalyticsByExamItem> | undefined>(
    undefined,
  );
  protected readonly examSection = signal<SectionState>('loading');
  protected readonly categoryPage = signal<
    ExamAnalyticsPage<ExamAnalyticsByCategoryItem> | undefined
  >(undefined);
  protected readonly categorySection = signal<SectionState>('loading');
  protected readonly trends = signal<ExamAnalyticsTrendPoint[] | undefined>(undefined);
  protected readonly trendsSection = signal<SectionState>('loading');
  protected readonly draft = signal<DraftFilters>(EMPTY_FILTERS);
  protected readonly applied = signal<DraftFilters>(EMPTY_FILTERS);
  protected readonly dateError = signal<string | null>(null);
  protected readonly countries = signal<{ id: string; name: string }[]>([]);

  protected readonly backPath = canonicalRoutePath('EXAMS_CATALOG');

  ngOnInit(): void {
    void this.initialize();
  }

  protected async retry(): Promise<void> {
    await this.loadAll();
  }

  protected setDraftFilters(filters: Partial<DraftFilters>): void {
    this.draft.set({ ...this.draft(), ...filters });
  }

  protected applyFilters(): void {
    const draft = this.draft();
    if (draft.from !== '' && draft.to !== '' && draft.from > draft.to) {
      this.dateError.set('From date must be on or before To date.');
      return;
    }
    this.dateError.set(null);
    this.applied.set({ ...draft });
    void this.router.navigate([], { queryParams: toQueryParams(draft) });
    void this.loadAll();
  }

  protected clearFilters(): void {
    this.draft.set(EMPTY_FILTERS);
    this.applied.set(EMPTY_FILTERS);
    this.dateError.set(null);
    void this.router.navigate([], { queryParams: {} });
    void this.loadAll();
  }

  protected async loadExamPage(page: number): Promise<void> {
    await this.loadExamSection(page);
  }

  protected async loadCategoryPage(page: number): Promise<void> {
    await this.loadCategorySection(page);
  }

  protected async retryExamSection(): Promise<void> {
    await this.loadExamSection(this.examPage()?.page ?? 1);
  }

  protected async retryCategorySection(): Promise<void> {
    await this.loadCategorySection(this.categoryPage()?.page ?? 1);
  }

  protected async retryTrends(): Promise<void> {
    await this.loadTrends();
  }

  protected countryOptions(): { value: string; label: string }[] {
    return [
      { value: NO_FILTER, label: 'All countries' },
      ...this.countries().map((country) => ({ value: country.id, label: country.name })),
    ];
  }

  protected categoryOptions(): { value: string; label: string }[] {
    const seen = new Map<string, string>();
    for (const row of this.categoryPage()?.items ?? []) {
      if (row.categoryId !== '' && row.categoryName !== null && !seen.has(row.categoryId)) {
        seen.set(row.categoryId, row.categoryName);
      }
    }
    return [
      { value: NO_FILTER, label: 'All categories' },
      ...[...seen].map(([value, label]) => ({ value, label })),
    ];
  }

  protected metric(value: number | null): string {
    return value === null ? 'Not available' : `${value}%`;
  }

  private async initialize(): Promise<void> {
    try {
      const loaded = await firstValueFrom(this.countriesApi.list());
      this.countries.set(loaded.map((country) => ({ id: country.id, name: country.name })));
    } catch {
      this.countries.set([]);
    }
    const query = this.route.snapshot.queryParamMap;
    const initial = normalizeFilters({
      from: query.get('from'),
      to: query.get('to'),
      countryId: query.get('countryId'),
      categoryId: query.get('categoryId'),
    });
    this.draft.set(initial);
    this.applied.set(initial);
    await this.loadAll();
  }

  private async loadAll(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.dateError.set(null);
    try {
      const loaded = await firstValueFrom(
        this.api.getExamAnalyticsSummary(toApiFilters(this.applied())),
      );
      this.summary.set(loaded);
      if (loaded.attemptCount === 0) {
        this.state.set({ kind: 'ready' });
        return;
      }
      await Promise.all([
        this.loadExamSection(1),
        this.loadCategorySection(1),
        this.loadTrends(),
      ]);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.retryableError(error), canRetry: true });
    }
  }

  private async loadExamSection(page: number): Promise<void> {
    this.examSection.set('loading');
    try {
      const result = await firstValueFrom(
        this.api.listExamAnalyticsByExam(toApiFilters(this.applied()), page),
      );
      this.examPage.set(result);
      this.examSection.set('ready');
    } catch {
      this.examSection.set('error');
    }
  }

  private async loadCategorySection(page: number): Promise<void> {
    this.categorySection.set('loading');
    try {
      const result = await firstValueFrom(
        this.api.listExamAnalyticsByCategory(toApiFilters(this.applied()), page),
      );
      this.categoryPage.set(result);
      this.categorySection.set('ready');
    } catch {
      this.categorySection.set('error');
    }
  }

  private async loadTrends(): Promise<void> {
    this.trendsSection.set('loading');
    try {
      const result = await firstValueFrom(
        this.api.listExamAnalyticsTrends(toApiFilters(this.applied())),
      );
      this.trends.set(result);
      this.trendsSection.set('ready');
    } catch {
      this.trendsSection.set('error');
    }
  }

  private retryableError(error: unknown): NormalizedProblemDetails {
    const normalized =
      typeof error === 'object' && error !== null && 'error' in error
        ? normalizeProblemDetails((error as { error?: unknown }).error)
        : normalizeProblemDetails(error);
    return { ...normalized, title: GENERIC_RETRY_COPY, detail: '' };
  }
}

function normalizeFilters(raw: {
  from: string | null;
  to: string | null;
  countryId: string | null;
  categoryId: string | null;
}): DraftFilters {
  const clean = (value: string | null): string => (value === null ? '' : value);
  return {
    from: clean(raw.from),
    to: clean(raw.to),
    countryId: clean(raw.countryId),
    categoryId: clean(raw.categoryId),
  };
}

function toApiFilters(filters: DraftFilters): ExamAnalyticsFilters {
  return {
    ...(filters.from === '' ? {} : { from: filters.from }),
    ...(filters.to === '' ? {} : { to: filters.to }),
    ...(filters.countryId === '' ? {} : { countryId: filters.countryId }),
    ...(filters.categoryId === '' ? {} : { categoryId: filters.categoryId }),
  };
}

function toQueryParams(filters: DraftFilters): Record<string, string> {
  return {
    ...(filters.from === '' ? {} : { from: filters.from }),
    ...(filters.to === '' ? {} : { to: filters.to }),
    ...(filters.countryId === '' ? {} : { countryId: filters.countryId }),
    ...(filters.categoryId === '' ? {} : { categoryId: filters.categoryId }),
  };
}
