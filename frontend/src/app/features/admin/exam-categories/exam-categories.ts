import { afterNextRender, Component, computed, ElementRef, inject, Injector, OnInit, signal, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import type { AdminExamCategoryDto } from '../../../core/api/generated/models/admin-exam-category-dto';
import type { CountryListItemDto } from '../../../core/api/generated/models/country-list-item-dto';
import type { PaginatedResultOfAdminExamCategoryDto } from '../../../core/api/generated/models/paginated-result-of-admin-exam-category-dto';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { NpSelectControl, NpTextInputControl, NpTextareaControl } from '../../../shared/ui/form-controls';
import type { NpSelectOption } from '../../../shared/ui/form-controls';
import { NpEmptyState } from '../../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';
import { NpPagination } from '../../../shared/ui/pagination';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../../shared/ui/form-validation';

type CategoryForm = FormGroup<{
  countryId: FormControl<string>;
  name: FormControl<string>;
  slug: FormControl<string>;
  description: FormControl<string>;
  displayOrder: FormControl<number>;
}>;
type ConfirmAction = 'archive' | 'delete';
const PAGE_SIZE = 20;
const FIELD_LABELS = Object.freeze({
  CountryId: 'Country', Name: 'Name', Slug: 'Slug',
  Description: 'Description', DisplayOrder: 'Display order',
});
const CONTROL_IDS = Object.freeze({
  CountryId: 'category-country', Name: 'category-name', Slug: 'category-slug',
  Description: 'category-description', DisplayOrder: 'category-display-order',
});

@Component({
  selector: 'np-exam-categories',
  imports: [MatButtonModule, ReactiveFormsModule, NpSelectControl, NpTextInputControl,
    NpTextareaControl, NpEmptyState, LoadingErrorRetry, NpPagination, NpFormValidationSummary],
  templateUrl: './exam-categories.html',
  styleUrl: './exam-categories.scss',
})
export class ExamCategoriesScreen implements OnInit {
  private readonly api = inject(AdminExamCategoriesApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly user = inject(CurrentUserStore);
  private readonly injector = inject(Injector);
  private readonly confirmHeading = viewChild<ElementRef<HTMLHeadingElement>>('confirmHeading');
  private readonly formHeading = viewChild<ElementRef<HTMLHeadingElement>>('formHeading');
  private readonly pageHeading = viewChild.required<ElementRef<HTMLHeadingElement>>('pageHeading');
  private actionTrigger: HTMLElement | undefined;

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly page = signal<PaginatedResultOfAdminExamCategoryDto | undefined>(undefined);
  protected readonly countries = signal<readonly CountryListItemDto[]>([]);
  protected readonly pageNumber = signal(1);
  protected readonly countryFilter = signal('');
  protected readonly activeFilter = signal('all');
  protected readonly editing = signal<AdminExamCategoryDto | undefined>(undefined);
  protected readonly formOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly actionPending = signal(false);
  protected readonly confirmation = signal<{ category: AdminExamCategoryDto; action: ConfirmAction } | undefined>(undefined);
  protected readonly message = signal('');
  protected readonly errorMessage = signal('');
  private readonly validationError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected get validationSummary() {
    return toFormValidationSummary(this.validationError(), {
      fieldLabels: FIELD_LABELS, controlIds: CONTROL_IDS,
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'Exam category could not be saved.',
    });
  }

  protected fieldError(field: keyof typeof FIELD_LABELS): string {
    return toFieldErrorText(this.validationError()?.errors?.[field]);
  }

  protected readonly canCreate = computed(() => this.can('Exams.Create'));
  protected readonly canEdit = computed(() => this.can('Exams.Edit'));
  protected readonly canDelete = computed(() => this.can('Exams.Delete'));

  protected readonly form: CategoryForm = new FormGroup({
    countryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    slug: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(160)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(1000)] }),
    displayOrder: new FormControl(0, { nonNullable: true, validators: [Validators.required] }),
  });

  protected get countryOptions(): readonly NpSelectOption[] {
    return this.countries().map((country) => ({ value: country.id, label: country.name }));
  }

  ngOnInit(): void { void this.load(); }

  protected async retry(): Promise<void> { await this.load(); }

  protected async changePage(page: number): Promise<void> {
    this.pageNumber.set(page);
    await this.load();
  }

  protected async filterCountry(id: string): Promise<void> {
    this.countryFilter.set(id);
    this.pageNumber.set(1);
    await this.load();
  }

  protected async filterActive(value: string): Promise<void> {
    this.activeFilter.set(value);
    this.pageNumber.set(1);
    await this.load();
  }

  protected beginCreate(): void {
    if (!this.canCreate()) return;
    this.editing.set(undefined);
    this.form.reset({ countryId: '', name: '', slug: '', description: '', displayOrder: 0 });
    this.errorMessage.set('');
    this.validationError.set(undefined);
    this.formOpen.set(true);
    afterNextRender(() => this.formHeading()?.nativeElement.focus(), { injector: this.injector });
  }

  protected async beginEdit(category: AdminExamCategoryDto): Promise<void> {
    if (!this.canEdit()) return;
    this.errorMessage.set('');
    this.validationError.set(undefined);
    try {
      const current = await firstValueFrom(this.api.get(category.id));
      this.editing.set(current);
      this.form.reset({
        countryId: current.countryId, name: current.name, slug: current.slug,
        description: current.description ?? '', displayOrder: current.displayOrder,
      });
      this.formOpen.set(true);
      afterNextRender(() => this.formHeading()?.nativeElement.focus(), { injector: this.injector });
    } catch {
      this.errorMessage.set('This exam category is not available to edit. Refresh the list.');
    }
  }

  protected closeForm(): void { if (!this.saving()) this.formOpen.set(false); }

  protected updateCountry(id: string): void {
    if (!this.editing()) this.form.controls.countryId.setValue(id);
    this.validationError.set(undefined);
  }
  protected updateName(value: string): void { this.form.controls.name.setValue(value); this.validationError.set(undefined); }
  protected updateSlug(value: string): void { this.form.controls.slug.setValue(value); this.validationError.set(undefined); }
  protected updateDescription(value: string): void { this.form.controls.description.setValue(value); this.validationError.set(undefined); }
  protected updateDisplayOrder(value: string): void { this.form.controls.displayOrder.setValue(Number(value)); this.validationError.set(undefined); }

  protected async save(): Promise<void> {
    if (this.saving() || !this.formOpen()) return;
    const selected = this.editing();
    if (selected ? !this.canEdit() : !this.canCreate()) return;
    const fields = this.form.getRawValue();
    if (this.form.invalid || fields.name.trim() === '' || fields.slug.trim() === '' ||
      !Number.isInteger(fields.displayOrder) || !Number.isFinite(fields.displayOrder)) {
      this.form.markAllAsTouched();
      this.errorMessage.set('Complete the required fields before saving.');
      return;
    }
    this.saving.set(true);
    this.errorMessage.set('');
    this.validationError.set(undefined);
    this.message.set('');
    const body = {
      countryId: fields.countryId, name: fields.name.trim(), slug: fields.slug.trim(),
      description: fields.description.trim() || null, displayOrder: fields.displayOrder,
    };
    try {
      if (selected) {
        await firstValueFrom(this.api.update(selected.id, {
          name: body.name, slug: body.slug,
          description: body.description, displayOrder: body.displayOrder,
        }));
      }
      else await firstValueFrom(this.api.create(body));
      this.formOpen.set(false);
      await this.load();
      this.message.set(selected ? 'Exam category updated.' : 'Exam category created.');
    } catch (error: unknown) {
      const body = typeof error === 'object' && error !== null && 'error' in error
        ? (error as { error?: unknown }).error : error;
      const normalized = normalizeProblemDetails(body);
      if (normalized.kind === 'validation') {
        const safeErrors: Record<string, readonly string[]> = {};
        for (const field of Object.keys(FIELD_LABELS) as (keyof typeof FIELD_LABELS)[]) {
          if (normalized.errors?.[field]?.length || normalized.errors?.[`Request.${field}`]?.length) {
            safeErrors[field] = [`Review ${FIELD_LABELS[field].toLowerCase()}.`];
          }
        }
        this.validationError.set({ ...normalized, errors: safeErrors, detail: '', title: '' });
      } else {
        this.errorMessage.set(this.statusOf(error) === 409
          ? 'This exam category conflicts with current data. Review the fields and try again.'
          : 'Exam category could not be saved. Try again.');
      }
    } finally {
      this.saving.set(false);
    }
  }

  protected requestAction(category: AdminExamCategoryDto, action: ConfirmAction): void {
    if (this.actionPending() || (action === 'delete' ? !this.canDelete() : !this.canEdit())) return;
    const active = this.pageHeading().nativeElement.ownerDocument.activeElement;
    this.actionTrigger = active instanceof HTMLElement ? active : undefined;
    this.confirmation.set({ category, action });
    this.errorMessage.set('');
    afterNextRender(() => this.confirmHeading()?.nativeElement.focus(), { injector: this.injector });
  }

  protected keepCategory(): void {
    this.confirmation.set(undefined);
    afterNextRender(() => {
      if (this.actionTrigger?.isConnected) this.actionTrigger.focus();
      else this.pageHeading().nativeElement.focus();
      this.actionTrigger = undefined;
    }, { injector: this.injector });
  }

  protected async confirmAction(): Promise<void> {
    const choice = this.confirmation();
    if (!choice || this.actionPending() || (choice.action === 'delete' ? !this.canDelete() : !this.canEdit())) return;
    this.actionPending.set(true);
    this.errorMessage.set('');
    try {
      if (choice.action === 'delete') await firstValueFrom(this.api.delete(choice.category.id));
      else await firstValueFrom(this.api.archive(choice.category.id));
      this.confirmation.set(undefined);
      await this.load();
      this.message.set(choice.action === 'delete' ? 'Exam category deleted.' : 'Exam category archived.');
    } catch {
      this.confirmation.set(undefined);
      this.errorMessage.set(choice.action === 'delete'
        ? "This exam category couldn't be deleted. Refresh the list before trying again."
        : "This exam category couldn't be archived. Refresh the list before trying again.");
      await this.load();
    } finally {
      this.actionPending.set(false);
      afterNextRender(() => this.pageHeading().nativeElement.focus(), { injector: this.injector });
    }
  }

  protected async restore(category: AdminExamCategoryDto): Promise<void> {
    if (!this.canEdit() || this.actionPending()) return;
    this.actionPending.set(true);
    this.errorMessage.set('');
    try {
      await firstValueFrom(this.api.restore(category.id));
      await this.load();
      this.message.set('Exam category restored.');
    } catch {
      this.errorMessage.set("This exam category couldn't be restored. Refresh the list before trying again.");
      await this.load();
    } finally { this.actionPending.set(false); }
  }

  private can(permission: string): boolean {
    return this.user.status() === 'ready' &&
      this.user.currentUser()?.roles.includes('Admin') === true &&
      this.user.currentUser()?.permissions.includes(permission) === true;
  }

  private statusOf(error: unknown): number | undefined {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const value = (error as { status?: unknown }).status;
      return typeof value === 'number' ? value : undefined;
    }
    return undefined;
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.page.set(undefined);
    try {
      const params = {
        page: this.pageNumber(), pageSize: PAGE_SIZE,
        ...(this.countryFilter() ? { countryId: this.countryFilter() } : {}),
        ...(this.activeFilter() !== 'all' ? { isActive: this.activeFilter() === 'active' } : {}),
      };
      const [countries, result] = await Promise.all([
        firstValueFrom(this.countriesApi.list()),
        firstValueFrom(this.api.list(params)),
      ]);
      this.countries.set(countries);
      this.page.set(result);
      this.pageNumber.set(result.page);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      const normalized = normalizeProblemDetails(error);
      this.state.set({ kind: 'error', error: {
        ...normalized, title: "We couldn't load exam categories. Try again.", detail: '',
      }, canRetry: true });
    }
  }
}
