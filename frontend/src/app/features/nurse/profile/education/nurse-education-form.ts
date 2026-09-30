import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import {
  NpDateControl,
  NpSelectControl,
  NpTextInputControl,
  NpTextareaControl,
  type NpSelectOption,
} from '../../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../../../shared/ui/form-validation';
import { LocalizationService } from '../../../../core/i18n/localization.service';

const INSTITUTION_NAME_MAX = 200;
const DEGREE_MAX = 200;
const FIELD_OF_STUDY_MAX = 200;
const DESCRIPTION_MAX = 2000;

export interface NurseEducationFormValue {
  readonly institutionName: string;
  readonly degree: string;
  readonly fieldOfStudy: string | null;
  readonly countryId: string | null;
  readonly startDate: string | null;
  readonly endDate: string | null;
  readonly description: string | null;
}

type EducationForm = FormGroup<{
  institutionName: FormControl<string>;
  degree: FormControl<string>;
  fieldOfStudy: FormControl<string>;
  countryId: FormControl<string>;
  startDate: FormControl<string>;
  endDate: FormControl<string>;
  description: FormControl<string>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  InstitutionName: 'eduForm.institutionName',
  Degree: 'eduForm.degree',
  FieldOfStudy: 'eduForm.fieldOfStudy',
  StartDate: 'eduForm.startDate',
  EndDate: 'eduForm.endDate',
  Description: 'eduForm.description',
} as const);

const CONTROL_IDS = Object.freeze({
  InstitutionName: 'nurse-education-institution-name',
  Degree: 'nurse-education-degree',
  FieldOfStudy: 'nurse-education-field-of-study',
  Country: 'nurse-education-country',
  StartDate: 'nurse-education-start-date',
  EndDate: 'nurse-education-end-date',
  Description: 'nurse-education-description',
});

@Component({
  selector: 'np-nurse-education-form',
  imports: [
    MatButtonModule,
    NpDateControl,
    NpFormValidationSummary,
    NpSelectControl,
    NpTextInputControl,
    NpTextareaControl,
    ReactiveFormsModule,
  ],
  templateUrl: './nurse-education-form.html',
  styleUrl: './nurse-education-form.scss',
})
export class NurseEducationForm implements OnInit {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() initial: NurseEducationFormValue | undefined = undefined;
  @Input() countries: readonly CountryListItemDto[] = [];
  @Input() isSubmitting = false;
  @Input() backendError: NormalizedProblemDetails | undefined = undefined;

  @Output() readonly save = new EventEmitter<NurseEducationFormValue>();
  @Output() readonly cancelled = new EventEmitter<void>();

  protected readonly form: EducationForm = new FormGroup({
    institutionName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(INSTITUTION_NAME_MAX)],
    }),
    degree: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(DEGREE_MAX)],
    }),
    fieldOfStudy: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(FIELD_OF_STUDY_MAX)],
    }),
    countryId: new FormControl('', { nonNullable: true }),
    startDate: new FormControl('', { nonNullable: true }),
    endDate: new FormControl('', { nonNullable: true }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(DESCRIPTION_MAX)],
    }),
  });

  protected readonly controlIds = CONTROL_IDS;
  protected readonly i18n = inject(LocalizationService);
  private readonly submittedState = { submitted: false };

  protected get submitLabel(): string {
    return this.mode === 'edit' ? this.i18n.t('eduForm.submitSave') : this.i18n.t('eduForm.submitCreate');
  }

  protected get countryOptions(): readonly NpSelectOption[] {
    return this.countries.map((country) => ({ value: country.id, label: country.name }));
  }

  protected get institutionNameValue(): string {
    return this.form.controls.institutionName.value;
  }

  protected get degreeValue(): string {
    return this.form.controls.degree.value;
  }

  protected get fieldOfStudyValue(): string {
    return this.form.controls.fieldOfStudy.value;
  }

  protected get countryValue(): string {
    return this.form.controls.countryId.value;
  }

  protected get startDateValue(): string {
    return this.form.controls.startDate.value;
  }

  protected get endDateValue(): string {
    return this.form.controls.endDate.value;
  }

  protected get descriptionValue(): string {
    return this.form.controls.description.value;
  }

  protected get institutionNameError(): string {
    return this.fieldError('InstitutionName');
  }

  protected get degreeError(): string {
    return this.fieldError('Degree');
  }

  protected get fieldOfStudyError(): string {
    return this.fieldError('FieldOfStudy');
  }

  protected get startDateError(): string {
    return this.fieldError('StartDate');
  }

  protected get endDateError(): string {
    return this.fieldError('EndDate');
  }

  protected get descriptionError(): string {
    return this.fieldError('Description');
  }

  protected get validationSummary() {
    return toFormValidationSummary(this.normalizedError(), {
      fieldLabels: {
        InstitutionName: this.i18n.t(FIELD_LABEL_KEYS.InstitutionName),
        Degree: this.i18n.t(FIELD_LABEL_KEYS.Degree),
        FieldOfStudy: this.i18n.t(FIELD_LABEL_KEYS.FieldOfStudy),
        StartDate: this.i18n.t(FIELD_LABEL_KEYS.StartDate),
        EndDate: this.i18n.t(FIELD_LABEL_KEYS.EndDate),
        Description: this.i18n.t(FIELD_LABEL_KEYS.Description),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('eduForm.formFallback'),
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'eduForm.formFallback');
  }

  ngOnInit(): void {
    if (this.initial !== undefined) {
      this.form.patchValue({
        institutionName: this.initial.institutionName,
        degree: this.initial.degree,
        fieldOfStudy: this.initial.fieldOfStudy ?? '',
        countryId: this.initial.countryId ?? '',
        startDate: this.initial.startDate ?? '',
        endDate: this.initial.endDate ?? '',
        description: this.initial.description ?? '',
      });
    }
  }

  protected updateInstitutionName(value: string): void {
    this.form.controls.institutionName.setValue(value);
    this.dismissBackendError();
  }

  protected updateDegree(value: string): void {
    this.form.controls.degree.setValue(value);
    this.dismissBackendError();
  }

  protected updateFieldOfStudy(value: string): void {
    this.form.controls.fieldOfStudy.setValue(value);
    this.dismissBackendError();
  }

  protected updateCountry(value: string): void {
    this.form.controls.countryId.setValue(value);
    this.dismissBackendError();
  }

  protected updateStartDate(value: string): void {
    this.form.controls.startDate.setValue(value);
    this.dismissBackendError();
  }

  protected updateEndDate(value: string): void {
    this.form.controls.endDate.setValue(value);
    this.dismissBackendError();
  }

  protected updateDescription(value: string): void {
    this.form.controls.description.setValue(value);
    this.dismissBackendError();
  }

  protected async submit(): Promise<void> {
    this.submittedState.submitted = true;

    const validationFailure = this.clientValidationFailure();
    if (validationFailure !== undefined) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit({
      institutionName: this.institutionNameValue.trim(),
      degree: this.degreeValue.trim(),
      fieldOfStudy: this.emptyToNull(this.fieldOfStudyValue),
      countryId: this.emptyToNull(this.countryValue),
      startDate: this.emptyToNull(this.startDateValue),
      endDate: this.emptyToNull(this.endDateValue),
      description: this.emptyToNull(this.descriptionValue),
    });
  }

  protected cancel(): void {
    this.cancelled.emit();
  }

  private normalizedError(): NormalizedProblemDetails | undefined {
    const clientFailure = this.submittedState.submitted ? this.clientValidationFailure() : undefined;
    if (clientFailure !== undefined) {
      return clientFailure;
    }
    return this.safeBackendError(this.backendError);
  }

  private safeBackendError(
    error: NormalizedProblemDetails | undefined,
  ): NormalizedProblemDetails | undefined {
    if (error === undefined) {
      return undefined;
    }
    return this.i18n.safeBackendError(error, FIELD_LABEL_KEYS);
  }

  private clientValidationFailure(): NormalizedProblemDetails | undefined {
    const errors: Record<string, readonly string[]> = {};
    const institutionName = this.form.controls.institutionName;
    const degree = this.form.controls.degree;
    const fieldOfStudy = this.form.controls.fieldOfStudy;
    const startDate = this.form.controls.startDate;
    const endDate = this.form.controls.endDate;
    const description = this.form.controls.description;

    if (institutionName.hasError('required')) {
      errors['InstitutionName'] = [this.i18n.t('eduForm.institutionRequired')];
    } else if (institutionName.hasError('maxlength')) {
      errors['InstitutionName'] = [this.i18n.t('eduForm.institutionMax')];
    }
    if (degree.hasError('required')) {
      errors['Degree'] = [this.i18n.t('eduForm.degreeRequired')];
    } else if (degree.hasError('maxlength')) {
      errors['Degree'] = [this.i18n.t('eduForm.degreeMax')];
    }
    if (fieldOfStudy.hasError('maxlength')) {
      errors['FieldOfStudy'] = [this.i18n.t('eduForm.fieldMax')];
    }
    if (description.hasError('maxlength')) {
      errors['Description'] = [this.i18n.t('eduForm.descMax')];
    }
    const startValue = startDate.value.trim();
    const endValue = endDate.value.trim();
    if (startValue !== '' && endValue !== '' && endValue < startValue) {
      errors['EndDate'] = [this.i18n.t('eduForm.endAfterStart')];
    }

    if (Object.keys(errors).length === 0) {
      return undefined;
    }
    return { kind: 'validation', type: '', title: this.i18n.t('auth.validationFailed'), status: 400, detail: '', traceId: '', errors };
  }

  private fieldError(field: keyof typeof FIELD_LABEL_KEYS): string {
    if (!this.submittedState.submitted) {
      return '';
    }
    return toFieldErrorText(this.normalizedError()?.errors?.[field]);
  }

  private dismissBackendError(): void {
    this.backendError = undefined;
  }

  private emptyToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === '' ? null : trimmed;
  }
}
