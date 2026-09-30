import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { NpLiveRegion } from '../../shared/ui/announcement';
import { LocalizationService } from '../../core/i18n/localization.service';
import { ExamsApi } from '../../core/api/exams-api';
import type { CountryOption, ExamCatalogPage } from '../../core/api/exams-api';
import { NurseProfileApi } from '../../core/api/nurse-profile-api';
import { canonicalRoutePath } from '../../core/routing/canonical-routes';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { resolveListState, type NpListState } from '../../shared/ui/pagination';
import { ExamCard } from './exam-card';

const PAGE_SIZE = 20;
const NO_FILTER = '';

@Component({
  selector: 'np-exams-list',
  imports: [LoadingErrorRetry, NpEmptyState, NpLiveRegion, ExamCard, RouterLink],
  templateUrl: './exams-list.html',
  styleUrl: './exams-list.scss',
})
export class ExamsList implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly profiles = inject(NurseProfileApi);
  protected readonly i18n = inject(LocalizationService);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly profileMissing = signal(false);
  protected readonly result = signal<ExamCatalogPage | undefined>(undefined);
  protected readonly page = signal(1);
  protected readonly countryId = signal(NO_FILTER);
  protected readonly categoryId = signal(NO_FILTER);
  protected readonly draftCountryId = signal(NO_FILTER);
  protected readonly draftCategoryId = signal(NO_FILTER);
  protected readonly countries = signal<CountryOption[]>([]);
  protected readonly knownCategories = signal<ReadonlyMap<string, string>>(new Map());

  protected readonly pageSize = PAGE_SIZE;

  protected readonly analyticsPath = canonicalRoutePath('EXAMS_ANALYTICS');

  protected readonly historyPath = canonicalRoutePath('EXAMS_HISTORY');
  protected readonly profilePath = canonicalRoutePath('NURSE_PROFILE_OVERVIEW');
  protected readonly pageNumbers = computed(() => {
    const total = this.result()?.totalPages ?? 0;
    const first = Math.min(Math.max(1, this.page() - 1), Math.max(1, total - 2));
    return Array.from({ length: Math.min(total, 3) }, (_, index) => first + index);
  });

  protected readonly showingFrom = computed(() => (this.result()?.items.length ?? 0) === 0 ? 0 :
    (this.page() - 1) * (this.result()?.pageSize ?? PAGE_SIZE) + 1);
  protected readonly showingTo = computed(() => Math.min(
    this.result()?.totalCount ?? 0,
    (this.page() - 1) * (this.result()?.pageSize ?? PAGE_SIZE) + (this.result()?.items.length ?? 0),
  ));

  ngOnInit(): void {
    void this.initialize();
  }

  protected async retry(): Promise<void> {
    await this.initialize();
  }

  protected async loadPage(page: number): Promise<void> {
    await this.load(page);
  }

  protected async onCountryChange(countryId: string): Promise<void> {
    this.draftCountryId.set(countryId);
  }

  protected async onCategoryChange(categoryId: string): Promise<void> {
    this.draftCategoryId.set(categoryId);
  }

  protected async applyFilters(): Promise<void> {
    this.countryId.set(this.draftCountryId());
    this.categoryId.set(this.draftCategoryId());
    await this.load(1);
  }

  protected async clearFilters(): Promise<void> {
    this.draftCountryId.set(NO_FILTER);
    this.draftCategoryId.set(NO_FILTER);
    this.countryId.set(NO_FILTER);
    this.categoryId.set(NO_FILTER);
    await this.load(1);
  }

  protected countryOptions(): { value: string; label: string }[] {
    return [
      { value: NO_FILTER, label: this.i18n.t('exams.allCountries') },
      ...this.countries().map((country) => ({ value: country.id, label: country.name })),
    ];
  }

  protected categoryOptions(): { value: string; label: string }[] {
    return [
      { value: NO_FILTER, label: this.i18n.t('exams.allCategories') },
      ...[...this.knownCategories()].map(([value, label]) => ({ value, label })),
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
    this.state.set({ kind: 'loading' });
    this.profileMissing.set(false);
    try {
      await firstValueFrom(this.profiles.getProfile());
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.profileMissing.set(true);
        this.state.set({ kind: 'ready' });
        return;
      }
      this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
      return;
    }
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
      this.knownCategories.update((previous) => {
        const next = new Map(previous);
        for (const item of result.items) {
          if (item.categoryId && item.categoryName?.trim()) {
            next.set(item.categoryId, item.categoryName);
          }
        }
        return next;
      });
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

  private isNotFound(error: unknown): boolean {
    return (
      typeof error === 'object' &&
      error !== null &&
      'status' in error &&
      (error as { status?: unknown }).status === 404
    );
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
