import { DatePipe } from '@angular/common';
import { afterNextRender, Component, ElementRef, inject, Injector, OnInit, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AdminExamsApi } from '../../../core/api/admin-exams-api';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import type { AdminExamDto } from '../../../core/api/generated/models/admin-exam-dto';
import type { AdminExamCategoryDto } from '../../../core/api/generated/models/admin-exam-category-dto';
import type { CountryListItemDto } from '../../../core/api/generated/models/country-list-item-dto';
import type { UpdateAdminExamRequest } from '../../../core/api/generated/models/update-admin-exam-request';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';
import { AdminExamForm } from './admin-exam-form';
import { adminExamStatusLabel } from './admin-exam-status';
import { safeAdminExamValidation } from './admin-exam-validation';
import type { TranslationKey } from '../../../core/i18n/translations';
import { LocalizationService } from '../../../core/i18n/localization.service';

type DestructiveAction = 'archive' | 'delete';

@Component({
  selector: 'np-admin-exam-detail',
  imports: [DatePipe, RouterLink, LoadingErrorRetry, AdminExamForm],
  templateUrl: './admin-exam-detail.html',
  styleUrl: './admin-exam-detail.scss',
})
export class AdminExamDetail implements OnInit {
  private readonly api = inject(AdminExamsApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly categoriesApi = inject(AdminExamCategoriesApi);
  private readonly user = inject(CurrentUserStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);
  protected readonly i18n = inject(LocalizationService);
  private readonly confirmHeading = viewChild<ElementRef<HTMLHeadingElement>>('confirmHeading');
  private readonly pageHeading = viewChild<ElementRef<HTMLHeadingElement>>('pageHeading');
  private readonly archiveTrigger = viewChild<ElementRef<HTMLButtonElement>>('archiveTrigger');
  private readonly deleteTrigger = viewChild<ElementRef<HTMLButtonElement>>('deleteTrigger');
  private actionToRestore: DestructiveAction | undefined;

  protected readonly backPath = canonicalRoutePath('ADMIN_EXAMS');
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly exam = signal<AdminExamDto | undefined>(undefined);
  protected readonly unavailable = signal(false);
  protected readonly countries = signal<readonly CountryListItemDto[]>([]);
  protected readonly categories = signal<readonly AdminExamCategoryDto[]>([]);
  protected readonly editing = signal(false);
  protected readonly busy = signal(false);
  protected readonly confirming = signal<DestructiveAction | undefined>(undefined);
  protected readonly message = signal('');
  protected readonly actionError = signal('');
  protected readonly validationError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected get canEdit(): boolean { return this.can('Exams.Edit'); }
  protected get canDelete(): boolean { return this.can('Exams.Delete'); }

  ngOnInit(): void { void this.load(); }

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected tp(key: TranslationKey, params: Record<string, string | number>): string {
    return this.i18n.tp(key, params);
  }

  protected async retry(): Promise<void> { await this.load(); }
  protected status(value: string): string {
    return adminExamStatusLabel(value, {
      draft: this.i18n.t('adm.statusDraft'),
      published: this.i18n.t('adm.statusPublished'),
      archived: this.i18n.t('adm.statusArchived'),
      unavailable: this.i18n.t('adm.statusUnavailable'),
    });
  }

  private validationLabels(): Record<string, string> {
    return {
      CountryId: this.i18n.t('adm.flCountry'),
      ExamCategoryId: this.i18n.t('adm.flCategory'),
      Title: this.i18n.t('adm.flTitle'),
      Slug: this.i18n.t('adm.flSlug'),
      Description: this.i18n.t('adm.flDescription'),
      Instructions: this.i18n.t('adm.flInstructions'),
      DurationMinutes: this.i18n.t('adm.flDuration'),
      PassingScorePercentage: this.i18n.t('adm.flScore'),
      IsFree: this.i18n.t('adm.flPriceType'),
    };
  }

  private reviewField(label: string): string {
    return this.i18n.tp('adm.reviewField', { field: label.toLowerCase() });
  }

  protected async beginEdit(): Promise<void> {
    if (!this.canEdit || this.exam()?.status === 'Archived' || !this.exam()) return;
    this.actionError.set('');
    this.validationError.set(undefined);
    try {
      this.countries.set(await firstValueFrom(this.countriesApi.list()));
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
      this.editing.set(true);
    } catch {
      this.actionError.set(this.i18n.t('adm.examEditUnavailable'));
    }
  }

  protected closeEdit(): void { if (!this.busy()) this.editing.set(false); }

  protected async save(body: UpdateAdminExamRequest): Promise<void> {
    if (!this.canEdit || !this.editing() || this.busy() || !this.exam() || this.exam()?.status === 'Archived') return;
    this.busy.set(true);
    this.actionError.set('');
    this.validationError.set(undefined);
    try {
      await firstValueFrom(this.api.update(this.examId(), body));
      this.editing.set(false);
      await this.load();
      this.message.set(this.i18n.t('adm.examUpdated'));
    } catch (error: unknown) {
      const validation = safeAdminExamValidation(error, this.validationLabels(), (label) => this.reviewField(label));
      if (validation) this.validationError.set(validation);
      else this.actionError.set(this.statusOf(error) === 409
        ? this.i18n.t('adm.examConflict')
        : this.i18n.t('adm.examSaveFailed'));
    } finally { this.busy.set(false); }
  }

  protected requestAction(action: DestructiveAction): void {
    const current = this.exam();
    if (!current || this.busy() || (action === 'delete' ? !this.canDelete || current.status !== 'Draft'
      : !this.canEdit || current.status === 'Archived')) return;
    this.actionToRestore = action;
    this.actionError.set('');
    this.confirming.set(action);
    afterNextRender(() => this.confirmHeading()?.nativeElement.focus(), { injector: this.injector });
  }

  protected keepExam(): void {
    this.confirming.set(undefined);
    afterNextRender(() => {
      const trigger = this.actionToRestore === 'archive' ? this.archiveTrigger() : this.deleteTrigger();
      if (trigger) trigger.nativeElement.focus();
      else this.pageHeading()?.nativeElement.focus();
      this.actionToRestore = undefined;
    }, { injector: this.injector });
  }

  protected async confirmAction(): Promise<void> {
    const action = this.confirming();
    const current = this.exam();
    if (!action || !current || this.busy() || (action === 'delete' ? !this.canDelete || current.status !== 'Draft'
      : !this.canEdit || current.status === 'Archived')) return;
    this.busy.set(true);
    this.actionError.set('');
    try {
      if (action === 'delete') {
        await firstValueFrom(this.api.delete(current.id));
        await this.router.navigateByUrl(this.backPath);
      } else {
        await firstValueFrom(this.api.archive(current.id));
        this.confirming.set(undefined);
        await this.load();
        this.message.set(this.i18n.t('adm.examArchived'));
        afterNextRender(() => this.pageHeading()?.nativeElement.focus(), { injector: this.injector });
      }
    } catch {
      this.confirming.set(undefined);
      this.actionError.set(action === 'delete'
        ? this.i18n.t('adm.examDeleteFailed')
        : this.i18n.t('adm.examArchiveFailed'));
      await this.load();
    } finally { this.busy.set(false); }
  }

  private can(permission: string): boolean {
    return this.user.status() === 'ready' && this.user.currentUser()?.roles.includes('Admin') === true &&
      this.user.currentUser()?.permissions.includes(permission) === true;
  }

  private examId(): string { return this.route.snapshot.paramMap.get('examId') ?? ''; }
  private statusOf(error: unknown): number | undefined {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const value = (error as { status?: unknown }).status;
      return typeof value === 'number' ? value : undefined;
    }
    return undefined;
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.exam.set(undefined);
    this.unavailable.set(false);
    try {
      this.exam.set(await firstValueFrom(this.api.get(this.examId())));
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.statusOf(error) === 404) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        const normalized = normalizeProblemDetails(
          typeof error === 'object' && error !== null && 'error' in error
            ? (error as { error?: unknown }).error : error,
        );
        this.state.set({ kind: 'error', error: {
          ...normalized, title: this.i18n.t('adm.examLoadError'), detail: '',
        }, canRetry: true });
      }
    }
  }
}
