import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { NpLiveRegion } from '../../shared/ui/announcement';
import { ExamsApi } from '../../core/api/exams-api';
import type { CountryOption, ExamCatalogPage } from '../../core/api/exams-api';
import { NpSelectControl } from '../../shared/ui/form-controls';
import { canonicalRoutePath } from '../../core/routing/canonical-routes';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { NpPagination, resolveListState, type NpListState } from '../../shared/ui/pagination';
import { ExamCard } from './exam-card';

const PAGE_SIZE = 20;
const NO_FILTER = '';

@Component({
  selector: 'np-exams-list',
  imports: [LoadingErrorRetry, NpEmptyState, NpLiveRegion, NpPagination, NpSelectControl, ExamCard, RouterLink],
  templateUrl: './exams-list.html',
  styleUrl: './exams-list.scss',
})
export class ExamsList implements OnInit {
  private readonly api = inject(ExamsApi);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly result = signal<ExamCatalogPage | undefined>(undefined);
  protected readonly page = signal(1);
  protected readonly countryId = signal(NO_FILTER);
  protected readonly categoryId = signal(NO_FILTER);
  protected readonly countries = signal<CountryOption[]>([]);

  protected readonly pageSize = PAGE_SIZE;

  protected readonly analyticsPath = canonicalRoutePath('EXAMS_ANALYTICS');

  ngOnInit(): void {
    void this.initialize();
  }

  protected async retry(): Promise<void> {
    await this.load(this.page());
  }

  protected async loadPage(page: number): Promise<void> {
    await this.load(page);
  }

  protected async onCountryChange(countryId: string): Promise<void> {
    this.countryId.set(countryId);
    await this.load(1);
  }

  protected async onCategoryChange(categoryId: string): Promise<void> {
    this.categoryId.set(categoryId);
    await this.load(1);
  }

  protected countryOptions(): { value: string; label: string }[] {
    return [
      { value: NO_FILTER, label: 'All countries' },
      ...this.countries().map((country) => ({ value: country.id, label: country.name })),
    ];
  }

  protected categoryOptions(): { value: string; label: string }[] {
    const seen = new Map<string, string>();
    for (const item of this.result()?.items ?? []) {
      if (item.categoryId !== null && !seen.has(item.categoryId)) {
        seen.set(item.categoryId, item.categoryName ?? item.categoryId);
      }
    }
    return [
      { value: NO_FILTER, label: 'All categories' },
      ...[...seen].map(([value, label]) => ({ value, label })),
    ];
  }

  protected listState(): NpListState {
    const hasItems = (this.result()?.items.length ?? 0) > 0;
    return resolveListState(hasItems, this.hasActiveFilters());
  }

  protected hasActiveFilters(): boolean {
    return this.countryId() !== NO_FILTER || this.categoryId() !== NO_FILTER;
  }

  private async initialize(): Promise<void> {
    try {
      this.countries.set(await firstValueFrom(this.api.listCountries()));
    } catch {
      this.countries.set([]);
    }
    await this.load(1);
  }

  private async load(page: number): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const result = await firstValueFrom(
        this.api.listExams({
          page,
          pageSize: PAGE_SIZE,
          countryId: this.selectedFilter(this.countryId()),
          categoryId: this.selectedFilter(this.categoryId()),
        }),
      );
      this.result.set(result);
      this.page.set(result.page);
      if (result.items.length === 0 && result.totalCount > 0 && result.page !== result.totalPages) {
        await this.load(Math.max(1, result.totalPages));
        return;
      }
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
    }
  }

  private selectedFilter(value: string): string | undefined {
    return value === NO_FILTER ? undefined : value;
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
