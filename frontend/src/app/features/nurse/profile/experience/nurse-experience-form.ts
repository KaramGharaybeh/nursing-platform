import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import {
  NpCheckboxControl,
  NpDateControl,
  NpSelectControl,
  NpTextInputControl,
  NpTextareaControl,
  type NpSelectOption,
} from '../../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../../../shared/ui/form-validation';
import { LocalizationService } from '../../../../core/i18n/localization.service';

const FACILITY_NAME_MAX = 200;
const JOB_TITLE_MAX = 200;
const DESCRIPTION_MAX = 2000;

export interface NurseExperienceFormValue {
  readonly facilityName: string;
  readonly jobTitle: string;
  readonly countryId: string | null;
  readonly startDate: string;
  readonly endDate: string | null;
  readonly isCurrent: boolean;
  readonly description: string | null;
}

type ExperienceForm = FormGroup<{
  facilityName: FormControl<string>;
  jobTitle: FormControl<string>;
  countryId: FormControl<string>;
  startDate: FormControl<string>;
  endDate: FormControl<string>;
  isCurrent: FormControl<boolean>;
  description: FormControl<string>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  FacilityName: 'expForm.facilityName',
  JobTitle: 'expForm.jobTitle',
  StartDate: 'expForm.startDate',
  EndDate: 'expForm.endDate',
  Description: 'expForm.description',
} as const);

const CONTROL_IDS = Object.freeze({
  FacilityName: 'nurse-experience-facility-name',
  JobTitle: 'nurse-experience-job-title',
  Country: 'nurse-experience-country',
  StartDate: 'nurse-experience-start-date',
  EndDate: 'nurse-experience-end-date',
  Current: 'nurse-experience-is-current',
  Description: 'nurse-experience-description',
});

@Component({
  selector: 'np-nurse-experience-form',
  imports: [
    MatButtonModule,
    NpCheckboxControl,
    NpDateControl,
    NpFormValidationSummary,
    NpSelectControl,
    NpTextInputControl,
    NpTextareaControl,
    ReactiveFormsModule,
  ],
  templateUrl: './nurse-experience-form.html',
  styleUrl: './nurse-experience-form.scss',
})
export class NurseExperienceForm implements OnInit {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() initial: NurseExperienceFormValue | undefined = undefined;
  @Input() countries: readonly CountryListItemDto[] = [];
  @Input() isSubmitting = false;
  @Input() backendError: NormalizedProblemDetails | undefined = undefined;

  @Output() readonly save = new EventEmitter<NurseExperienceFormValue>();
  @Output() readonly cancelled = new EventEmitter<void>();

  protected readonly form: ExperienceForm = new FormGroup({
    facilityName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(FACILITY_NAME_MAX)],
    }),
    jobTitle: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(JOB_TITLE_MAX)],
    }),
    countryId: new FormControl('', { nonNullable: true }),
    startDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    endDate: new FormControl('', { nonNullable: true }),
    isCurrent: new FormControl(false, { nonNullable: true }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(DESCRIPTION_MAX)],
    }),
  });

  protected readonly controlIds = CONTROL_IDS;
  protected readonly i18n = inject(LocalizationService);
  private readonly submittedState = { submitted: false };
  private retainedEndDate = '';

  protected get submitLabel(): string {
    return this.mode === 'edit' ? this.i18n.t('expForm.submitSave') : this.i18n.t('expForm.submitCreate');
  }

  protected get countryOptions(): readonly NpSelectOption[] {
    return this.countries.map((country) => ({ value: country.id, label: country.name }));
  }

  protected get facilityNameValue(): string {
    return this.form.controls.facilityName.value;
  }

  protected get jobTitleValue(): string {
    return this.form.controls.jobTitle.value;
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

  protected get isCurrentValue(): boolean {
    return this.form.controls.isCurrent.value;
  }

  protected get descriptionValue(): string {
    return this.form.controls.description.value;
  }

  protected get facilityNameError(): string {
    return this.fieldError('FacilityName');
  }

  protected get jobTitleError(): string {
    return this.fieldError('JobTitle');
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
        FacilityName: this.i18n.t(FIELD_LABEL_KEYS.FacilityName),
        JobTitle: this.i18n.t(FIELD_LABEL_KEYS.JobTitle),
        StartDate: this.i18n.t(FIELD_LABEL_KEYS.StartDate),
        EndDate: this.i18n.t(FIELD_LABEL_KEYS.EndDate),
        Description: this.i18n.t(FIELD_LABEL_KEYS.Description),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('expForm.formFallback'),
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'expForm.formFallback');
  }

  ngOnInit(): void {
    if (this.initial !== undefined) {
      this.form.patchValue({
        facilityName: this.initial.facilityName,
        jobTitle: this.initial.jobTitle,
        countryId: this.initial.countryId ?? '',
        startDate: this.initial.startDate,
        endDate: this.initial.endDate ?? '',
        isCurrent: this.initial.isCurrent,
        description: this.initial.description ?? '',
      });
      if (this.initial.isCurrent) {
        this.form.controls.endDate.disable({ emitEvent: false });
      }
    }
  }

  protected updateFacilityName(value: string): void {
    this.form.controls.facilityName.setValue(value);
    this.dismissBackendError();
  }

  protected updateJobTitle(value: string): void {
    this.form.controls.jobTitle.setValue(value);
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

  protected updateIsCurrent(checked: boolean): void {
    this.form.controls.isCurrent.setValue(checked);
    if (checked) {
      this.retainedEndDate = this.form.controls.endDate.value;
      this.form.controls.endDate.setValue('');
      this.form.controls.endDate.disable({ emitEvent: false });
    } else {
      this.form.controls.endDate.enable({ emitEvent: false });
      if (this.retainedEndDate !== '' && this.form.controls.endDate.value === '') {
        this.form.controls.endDate.setValue(this.retainedEndDate);
      }
    }
  }

  protected async submit(): Promise<void> {
    this.submittedState.submitted = true;

    const validationFailure = this.clientValidationFailure();
    if (validationFailure !== undefined) {
      this.form.markAllAsTouched();
      return;
    }

    const isCurrent = this.form.controls.isCurrent.value;
    this.save.emit({
      facilityName: this.facilityNameValue.trim(),
      jobTitle: this.jobTitleValue.trim(),
      countryId: this.emptyToNull(this.countryValue),
      startDate: this.startDateValue,
      endDate: isCurrent ? null : this.emptyToNull(this.endDateValue),
      isCurrent,
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
    const facilityName = this.form.controls.facilityName;
    const jobTitle = this.form.controls.jobTitle;
    const startDate = this.form.controls.startDate;
    const endDate = this.form.controls.endDate;
    const description = this.form.controls.description;
    const isCurrent = this.form.controls.isCurrent.value;

    if (facilityName.hasError('required')) {
      errors['FacilityName'] = [this.i18n.t('expForm.facilityRequired')];
    } else if (facilityName.hasError('maxlength')) {
      errors['FacilityName'] = [this.i18n.t('expForm.facilityMax')];
    }
    if (jobTitle.hasError('required')) {
      errors['JobTitle'] = [this.i18n.t('expForm.jobRequired')];
    } else if (jobTitle.hasError('maxlength')) {
      errors['JobTitle'] = [this.i18n.t('expForm.jobMax')];
    }
    if (startDate.hasError('required')) {
      errors['StartDate'] = [this.i18n.t('expForm.startRequired')];
    }
    if (description.hasError('maxlength')) {
      errors['Description'] = [this.i18n.t('expForm.descMax')];
    }
    const endValue = endDate.value.trim();
    if (endValue !== '' && startDate.value.trim() !== '' && endValue < startDate.value.trim()) {
      errors['EndDate'] = [this.i18n.t('expForm.endAfterStart')];
    }
    if (isCurrent && endValue !== '') {
      errors['EndDate'] = [this.i18n.t('expForm.endCurrentRole')];
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
