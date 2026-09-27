import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import type { AdminExamCategoryDto } from '../../../core/api/generated/models/admin-exam-category-dto';
import type { AdminExamDto } from '../../../core/api/generated/models/admin-exam-dto';
import type { CountryListItemDto } from '../../../core/api/generated/models/country-list-item-dto';
import type { CreateAdminExamRequest } from '../../../core/api/generated/models/create-admin-exam-request';
import { NpCheckboxControl, NpSelectControl, NpTextInputControl, NpTextareaControl } from '../../../shared/ui/form-controls';
import type { NpSelectOption } from '../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../../shared/ui/form-validation';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import type { TranslationKey } from '../../../core/i18n/translations';
import { LocalizationService } from '../../../core/i18n/localization.service';

const MAX_DURATION = 480;
type ExamFormField = 'CountryId' | 'Title' | 'Slug' | 'DurationMinutes' |
  'PassingScorePercentage' | 'Description' | 'Instructions';
const CONTROL_IDS = Object.freeze({
  CountryId: 'exam-country', Title: 'exam-title', Slug: 'exam-slug',
  DurationMinutes: 'exam-duration', PassingScorePercentage: 'exam-score',
  Description: 'exam-description', Instructions: 'exam-instructions',
});

@Component({
  selector: 'np-admin-exam-form',
  imports: [MatButtonModule, ReactiveFormsModule, NpCheckboxControl, NpSelectControl,
    NpTextInputControl, NpTextareaControl, NpFormValidationSummary],
  templateUrl: './admin-exam-form.html',
  styleUrl: './admin-exam-form.scss',
})
export class AdminExamForm {
  readonly exam = input<AdminExamDto | undefined>(undefined);
  readonly countries = input<readonly CountryListItemDto[]>([]);
  readonly categories = input<readonly AdminExamCategoryDto[]>([]);
  readonly pending = input(false);
  readonly serverError = input<NormalizedProblemDetails | undefined>(undefined);
  readonly submitted = output<CreateAdminExamRequest>();
  readonly cancelled = output<void>();
  protected readonly i18n = inject(LocalizationService);

  private readonly localError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly form = new FormGroup({
    countryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    examCategoryId: new FormControl('', { nonNullable: true }),
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    slug: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(160)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    instructions: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(4000)] }),
    durationMinutes: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(1), Validators.max(MAX_DURATION)] }),
    passingScorePercentage: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100)] }),
    isFree: new FormControl(true, { nonNullable: true }),
  });

  constructor() {
    effect(() => {
      const exam = this.exam();
      if (!exam) return;
      this.form.patchValue({
        countryId: exam.countryId, examCategoryId: exam.examCategoryId ?? '', title: exam.title,
        slug: exam.slug, description: exam.description ?? '', instructions: exam.instructions ?? '',
        durationMinutes: exam.durationMinutes, passingScorePercentage: exam.passingScorePercentage,
        isFree: exam.isFree,
      });
    });
  }

  protected get countryOptions(): readonly NpSelectOption[] {
    return this.countries().map((item) => ({ value: item.id, label: item.name }));
  }

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected fieldLabels(): Record<ExamFormField, string> {
    return {
      CountryId: this.i18n.t('adm.country'),
      Title: this.i18n.t('adm.labelTitle'),
      Slug: this.i18n.t('adm.labelSlug'),
      DurationMinutes: this.i18n.t('adm.labelDurationShort'),
      PassingScorePercentage: this.i18n.t('adm.labelScoreShort'),
      Description: this.i18n.t('adm.labelDescription'),
      Instructions: this.i18n.t('adm.instructions'),
    };
  }

  protected get categoryOptions(): readonly NpSelectOption[] {
    return [
      { value: '', label: this.i18n.t('adm.noCategory') },
      ...this.categories().filter((item) => item.countryId === this.form.controls.countryId.value &&
        (item.isActive || item.id === this.form.controls.examCategoryId.value))
        .map((item) => ({ value: item.id, label: item.name })),
    ];
  }

  protected get validationSummary() {
    return toFormValidationSummary(this.localError() ?? this.serverError(), {
      fieldLabels: this.fieldLabels(), controlIds: CONTROL_IDS, summaryTitle: this.i18n.t('adm.checkFields'),
      formErrorFallback: this.i18n.t('adm.examSaveFallback'),
    });
  }

  protected fieldError(field: ExamFormField): string {
    return toFieldErrorText((this.localError() ?? this.serverError())?.errors?.[field]);
  }

  protected updateCountry(value: string): void {
    this.form.controls.countryId.setValue(value);
    this.form.controls.examCategoryId.setValue('');
    this.localError.set(undefined);
  }
  protected updateCategory(value: string): void { this.form.controls.examCategoryId.setValue(value); this.localError.set(undefined); }
  protected updateTitle(value: string): void { this.form.controls.title.setValue(value); this.localError.set(undefined); }
  protected updateSlug(value: string): void { this.form.controls.slug.setValue(value); this.localError.set(undefined); }
  protected updateDescription(value: string): void { this.form.controls.description.setValue(value); this.localError.set(undefined); }
  protected updateInstructions(value: string): void { this.form.controls.instructions.setValue(value); this.localError.set(undefined); }
  protected updateDuration(value: string): void { this.form.controls.durationMinutes.setValue(Number(value)); this.localError.set(undefined); }
  protected updateScore(value: string): void { this.form.controls.passingScorePercentage.setValue(Number(value)); this.localError.set(undefined); }
  protected updateFree(value: boolean): void { this.form.controls.isFree.setValue(value); this.localError.set(undefined); }

  protected submit(): void {
    if (this.pending()) return;
    const value = this.form.getRawValue();
    const errors: Record<string, readonly string[]> = {};
    if (!value.countryId) errors['CountryId'] = [this.i18n.t('adm.formChooseCountry')];
    if (!value.title.trim() || value.title.length > 200) errors['Title'] = [this.i18n.t('adm.formTitle')];
    if (!value.slug.trim() || value.slug.length > 160) errors['Slug'] = [this.i18n.t('adm.formSlug')];
    if (value.description.length > 2000) errors['Description'] = [this.i18n.t('adm.formDescription')];
    if (value.instructions.length > 4000) errors['Instructions'] = [this.i18n.t('adm.formInstructions')];
    if (!Number.isInteger(value.durationMinutes) || value.durationMinutes < 1 || value.durationMinutes > MAX_DURATION)
      errors['DurationMinutes'] = [this.i18n.t('adm.formDuration')];
    if (!Number.isFinite(value.passingScorePercentage) || value.passingScorePercentage < 0 || value.passingScorePercentage > 100)
      errors['PassingScorePercentage'] = [this.i18n.t('adm.formScore')];
    if (Object.keys(errors).length > 0) {
      this.form.markAllAsTouched();
      this.localError.set({ kind: 'validation', type: '', title: '', status: 400, detail: '', traceId: '', errors });
      return;
    }
    this.localError.set(undefined);
    this.submitted.emit({
      countryId: value.countryId, examCategoryId: value.examCategoryId || null,
      title: value.title.trim(), slug: value.slug.trim(),
      description: value.description.trim() || null, instructions: value.instructions.trim() || null,
      durationMinutes: value.durationMinutes, passingScorePercentage: value.passingScorePercentage,
      isFree: value.isFree,
    });
  }
}
