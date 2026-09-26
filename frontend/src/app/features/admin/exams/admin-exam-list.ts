import { afterNextRender, Component, ElementRef, inject, Injector, OnInit, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AdminExamsApi } from '../../../core/api/admin-exams-api';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import type { AdminExamDto } from '../../../core/api/generated/models/admin-exam-dto';
import type { AdminExamCategoryDto } from '../../../core/api/generated/models/admin-exam-category-dto';
import type { CountryListItemDto } from '../../../core/api/generated/models/country-list-item-dto';
import type { PaginatedResultOfAdminExamDto } from '../../../core/api/generated/models/paginated-result-of-admin-exam-dto';
import type { CreateAdminExamRequest } from '../../../core/api/generated/models/create-admin-exam-request';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { buildAdminExamDetailPath } from '../../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';
import { NpEmptyState } from '../../../shared/ui/empty-state';
import { NpPagination } from '../../../shared/ui/pagination';
import { AdminExamForm } from './admin-exam-form';
import { adminExamStatusLabel } from './admin-exam-status';
import { safeAdminExamValidation } from './admin-exam-validation';

@Component({
  selector: 'np-admin-exam-list',
  imports: [RouterLink, LoadingErrorRetry, NpEmptyState, NpPagination, AdminExamForm],
  templateUrl: './admin-exam-list.html',
  styleUrl: './admin-exam-list.scss',
})
export class AdminExamList implements OnInit {
  private readonly api = inject(AdminExamsApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly categoriesApi = inject(AdminExamCategoriesApi);
  private readonly user = inject(CurrentUserStore);
  private readonly injector = inject(Injector);
  private readonly formHeading = viewChild<ElementRef<HTMLHeadingElement>>('formHeading');
  private readonly createTrigger = viewChild<ElementRef<HTMLButtonElement>>('createTrigger');

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly page = signal<PaginatedResultOfAdminExamDto | undefined>(undefined);
  protected readonly countries = signal<readonly CountryListItemDto[]>([]);
  protected readonly categories = signal<readonly AdminExamCategoryDto[]>([]);
  private readonly lookupsReady = signal(false);
  protected readonly pageNumber = signal(1);
  protected readonly countryFilter = signal('');
  protected readonly categoryFilter = signal('');
  protected readonly statusFilter = signal('all');
  protected readonly freeFilter = signal('all');
  protected readonly formOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly message = signal('');
  protected readonly saveError = signal('');
  protected readonly validationError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected get canCreate(): boolean {
    return this.user.status() === 'ready' && this.user.currentUser()?.roles.includes('Admin') === true &&
      this.user.currentUser()?.permissions.includes('Exams.Create') === true;
  }

  protected get filteredCategories(): readonly AdminExamCategoryDto[] {
    return this.categories().filter((item) => !this.countryFilter() || item.countryId === this.countryFilter());
  }

  ngOnInit(): void { void this.initialize(); }

  protected detailPath(id: string): string { return buildAdminExamDetailPath(id); }
  protected status(exam: AdminExamDto): string { return adminExamStatusLabel(exam.status); }
  protected async retry(): Promise<void> {
    if (this.lookupsReady()) await this.load();
    else await this.initialize();
  }
  protected async changePage(value: number): Promise<void> { this.pageNumber.set(value); await this.load(); }
  protected async filterCountry(value: string): Promise<void> {
    this.countryFilter.set(value);
    this.categoryFilter.set('');
    this.pageNumber.set(1);
    await this.load();
  }
  protected async filterCategory(value: string): Promise<void> {
    this.categoryFilter.set(value);
    this.pageNumber.set(1);
    await this.load();
  }
  protected async filterStatus(value: string): Promise<void> {
    this.statusFilter.set(value);
    this.pageNumber.set(1);
    await this.load();
  }
  protected async filterFree(value: string): Promise<void> {
    this.freeFilter.set(value);
    this.pageNumber.set(1);
    await this.load();
  }
  protected openForm(): void {
    if (!this.canCreate) return;
    this.validationError.set(undefined);
    this.formOpen.set(true);
    afterNextRender(() => this.formHeading()?.nativeElement.focus(), { injector: this.injector });
  }
  protected closeForm(): void {
    if (this.saving()) return;
    this.formOpen.set(false);
    afterNextRender(() => this.createTrigger()?.nativeElement.focus(), { injector: this.injector });
  }

  protected async create(body: CreateAdminExamRequest): Promise<void> {
    if (!this.canCreate || this.saving() || !this.formOpen()) return;
    this.saving.set(true);
    this.saveError.set('');
    this.validationError.set(undefined);
    try {
      await firstValueFrom(this.api.create(body));
      this.formOpen.set(false);
      await this.load();
      this.message.set('Exam created.');
    } catch (error: unknown) {
      const validation = safeAdminExamValidation(error);
      if (validation) this.validationError.set(validation);
      else this.saveError.set('Exam could not be created. Review the fields and try again.');
    } finally { this.saving.set(false); }
  }

  private async initialize(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.lookupsReady.set(false);
    try {
      const countries = await firstValueFrom(this.countriesApi.list());
      this.countries.set(countries);
      const categories: AdminExamCategoryDto[] = [];
      let page = 1;
      let totalPages: number;
      do {
        const result = await firstValueFrom(this.categoriesApi.list({ page, pageSize: 100 }));
        categories.push(...result.items);
        totalPages = result.totalPages;
        page++;
      } while (page <= totalPages);
      this.categories.set(categories);
      this.lookupsReady.set(true);
      await this.load();
    } catch (error: unknown) {
      this.fail(error);
    }
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.page.set(undefined);
    try {
      const result = await firstValueFrom(this.api.list({
        page: this.pageNumber(),
        ...(this.countryFilter() ? { countryId: this.countryFilter() } : {}),
        ...(this.categoryFilter() ? { categoryId: this.categoryFilter() } : {}),
        ...(this.statusFilter() !== 'all' ? { status: Number(this.statusFilter()) } : {}),
        ...(this.freeFilter() !== 'all' ? { isFree: this.freeFilter() === 'free' } : {}),
      }));
      this.page.set(result);
      this.pageNumber.set(result.page);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) { this.fail(error); }
  }

  private fail(error: unknown): void {
    const detail = typeof error === 'object' && error !== null && 'error' in error
      ? (error as { error?: unknown }).error : error;
    const normalized = normalizeProblemDetails(detail);
    this.state.set({ kind: 'error', error: {
      ...normalized, title: "We couldn't load admin exams. Try again.", detail: '',
    }, canRetry: true });
  }
}
