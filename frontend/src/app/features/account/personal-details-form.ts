import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { LocalizationService } from '../../core/i18n/localization.service';
import { NpTextInputControl } from '../../shared/ui/form-controls';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../shared/ui/form-validation';

const NAME_MAX = 100;

export interface PersonalDetailsFormValue {
  readonly firstName: string;
  readonly lastName: string;
}

type DetailsForm = FormGroup<{
  firstName: FormControl<string>;
  lastName: FormControl<string>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  FirstName: 'details.firstName',
  LastName: 'details.lastName',
} as const);

const CONTROL_IDS = Object.freeze({
  FirstName: 'account-personal-details-first-name',
  LastName: 'account-personal-details-last-name',
});

@Component({
  selector: 'np-personal-details-form',
  imports: [MatButtonModule, NpFormValidationSummary, NpTextInputControl, ReactiveFormsModule],
  templateUrl: './personal-details-form.html',
  styleUrl: './personal-details-form.scss',
})
export class PersonalDetailsForm implements OnInit {
  @Input() initial: PersonalDetailsFormValue | undefined = undefined;
  @Input() isSubmitting = false;
  @Input() backendError: NormalizedProblemDetails | undefined = undefined;

  @Output() readonly save = new EventEmitter<PersonalDetailsFormValue>();
  @Output() readonly cancelled = new EventEmitter<void>();
  protected readonly i18n = inject(LocalizationService);

  protected readonly form: DetailsForm = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(NAME_MAX)],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(NAME_MAX)],
    }),
  });

  protected readonly controlIds = CONTROL_IDS;
  private readonly submittedState = { submitted: false };

  protected get firstNameValue(): string {
    return this.form.controls.firstName.value;
  }

  protected get lastNameValue(): string {
    return this.form.controls.lastName.value;
  }

  protected get firstNameError(): string {
    return this.fieldError('FirstName');
  }

  protected get lastNameError(): string {
    return this.fieldError('LastName');
  }

  protected get validationSummary() {
    return toFormValidationSummary(this.normalizedError(), {
      fieldLabels: {
        FirstName: this.i18n.t(FIELD_LABEL_KEYS.FirstName),
        LastName: this.i18n.t(FIELD_LABEL_KEYS.LastName),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('details.formFallback'),
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'details.formFallback');
  }

  ngOnInit(): void {
    if (this.initial !== undefined) {
      this.form.patchValue({
        firstName: this.initial.firstName,
        lastName: this.initial.lastName,
      });
    }
  }

  protected updateFirstName(value: string): void {
    this.form.controls.firstName.setValue(value);
    this.dismissBackendError();
  }

  protected updateLastName(value: string): void {
    this.form.controls.lastName.setValue(value);
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
      firstName: this.firstNameValue.trim(),
      lastName: this.lastNameValue.trim(),
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
    const firstName = this.form.controls.firstName;
    const lastName = this.form.controls.lastName;

    if (firstName.value.trim() === '') {
      errors['FirstName'] = [this.i18n.t('details.firstRequired')];
    } else if (firstName.hasError('maxlength')) {
      errors['FirstName'] = [this.i18n.t('details.firstMax')];
    }
    if (lastName.value.trim() === '') {
      errors['LastName'] = [this.i18n.t('details.lastRequired')];
    } else if (lastName.hasError('maxlength')) {
      errors['LastName'] = [this.i18n.t('details.lastMax')];
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
}
