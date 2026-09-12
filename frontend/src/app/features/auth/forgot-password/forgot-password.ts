import { Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { NpTextInputControl } from '../../../shared/ui/form-controls';
import {
  NpFormValidationSummary,
  toFieldErrorText,
  toFormValidationSummary,
} from '../../../shared/ui/form-validation';
import { ForgotPasswordApi } from './forgot-password-api';

type ForgotPasswordForm = FormGroup<{
  email: FormControl<string>;
}>;

const FIELD_LABELS = Object.freeze({
  Email: 'Email address',
});

const CONTROL_IDS = Object.freeze({
  Email: 'auth-forgot-password-email',
});

const SUCCESS_MESSAGE = 'If the email exists, a password reset link has been sent.';

const REQUIRED_VALIDATION: NormalizedProblemDetails = Object.freeze({
  kind: 'validation',
  type: '',
  title: 'Validation failed',
  status: 400,
  detail: '',
  traceId: '',
  errors: Object.freeze({
    Email: Object.freeze(["'Email' must not be empty."]),
  }),
});

const EMAIL_VALIDATION: NormalizedProblemDetails = Object.freeze({
  kind: 'validation',
  type: '',
  title: 'Validation failed',
  status: 400,
  detail: '',
  traceId: '',
  errors: Object.freeze({
    Email: Object.freeze(["'Email' is not a valid email address."]),
  }),
});

@Component({
  selector: 'np-forgot-password',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    NpTextInputControl,
    ReactiveFormsModule,
  ],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
  private readonly forgotPasswordApi = inject(ForgotPasswordApi);

  protected readonly form: ForgotPasswordForm = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
  });

  protected readonly isSubmitting = signal(false);
  protected readonly successMessage = signal('');
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly validationSummary = computed(() => toFormValidationSummary(
    this.normalizedError(),
    {
      fieldLabels: FIELD_LABELS,
      controlIds: CONTROL_IDS,
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'The password reset request could not be submitted.',
    },
  ));

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return error.detail.trim() !== '' ? error.detail : 'The password reset request could not be submitted.';
  });

  protected get emailValue(): string {
    return this.form.controls.email.value;
  }

  protected get emailError(): string {
    return this.fieldError('Email');
  }

  protected updateEmail(value: string): void {
    this.form.controls.email.setValue(value);
    this.clearFeedback();
  }

  protected async submit(): Promise<void> {
    if (this.isSubmitting()) {
      return;
    }

    this.submitted.set(true);
    this.normalizedError.set(undefined);
    this.successMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.normalizedError.set(this.emailValue.trim() === '' ? REQUIRED_VALIDATION : EMAIL_VALIDATION);
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });

    try {
      await firstValueFrom(this.forgotPasswordApi.requestPasswordReset({ email: this.emailValue }));
      this.successMessage.set(SUCCESS_MESSAGE);
    } catch (error: unknown) {
      this.normalizedError.set(normalizeProblemDetails(this.errorBody(error)));
      this.form.enable({ emitEvent: false });
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private fieldError(field: 'Email'): string {
    if (!this.submitted()) {
      return '';
    }
    return toFieldErrorText(this.normalizedError()?.errors?.[field]);
  }

  private clearFeedback(): void {
    this.normalizedError.set(undefined);
    this.successMessage.set('');
  }

  private errorBody(error: unknown): unknown {
    if (typeof error !== 'object' || error === null) {
      return error;
    }
    if ('error' in error) {
      return (error as { error?: unknown }).error;
    }
    return error;
  }
}
