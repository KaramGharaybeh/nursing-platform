import { Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { NpTextInputControl } from '../../../shared/ui/form-controls';
import {
  NpFormValidationSummary,
  toFieldErrorText,
  toFormValidationSummary,
} from '../../../shared/ui/form-validation';
import { ResetPasswordApi } from './reset-password-api';

type ResetPasswordForm = FormGroup<{
  email: FormControl<string>;
  newPassword: FormControl<string>;
}>;

const FIELD_LABELS = Object.freeze({
  Email: 'Email address',
  NewPassword: 'New password',
});

const CONTROL_IDS = Object.freeze({
  Email: 'auth-reset-password-email',
  NewPassword: 'auth-reset-password-new-password',
});

const MISSING_TOKEN_MESSAGE =
  'This reset link is invalid or missing. Request a new reset link to continue.';

const SUCCESS_MESSAGE = 'Password has been reset successfully.';

@Component({
  selector: 'np-reset-password',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    NpTextInputControl,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
})
export class ResetPassword {
  private readonly resetPasswordApi = inject(ResetPasswordApi);
  private readonly resetToken: string =
    (inject(ActivatedRoute).snapshot.queryParamMap.get('token') ?? '').trim();

  protected readonly forgotPasswordPath = canonicalRoutePath('AUTH_FORGOT_PASSWORD');

  protected readonly form: ResetPasswordForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    newPassword: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/[A-Z]/),
        Validators.pattern(/[0-9]/),
      ],
    }),
  });

  protected readonly isSubmitting = signal(false);
  protected readonly successMessage = signal('');
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly isTokenMissing = computed(() => this.resetToken === '');

  protected readonly tokenMissingMessage = computed(() =>
    this.isTokenMissing() ? MISSING_TOKEN_MESSAGE : '',
  );

  protected readonly validationSummary = computed(() =>
    toFormValidationSummary(this.normalizedError(), {
      fieldLabels: FIELD_LABELS,
      controlIds: CONTROL_IDS,
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'The password could not be reset.',
    }),
  );

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return error.detail.trim() !== '' ? error.detail : 'The password could not be reset.';
  });

  protected get emailValue(): string {
    return this.form.controls.email.value;
  }

  protected get newPasswordValue(): string {
    return this.form.controls.newPassword.value;
  }

  protected get emailError(): string {
    return this.fieldError('Email');
  }

  protected get newPasswordError(): string {
    return this.fieldError('NewPassword');
  }

  protected updateEmail(value: string): void {
    this.form.controls.email.setValue(value);
    this.clearFeedback();
  }

  protected updateNewPassword(value: string): void {
    this.form.controls.newPassword.setValue(value);
    this.clearFeedback();
  }

  protected async submit(): Promise<void> {
    if (this.isSubmitting() || this.isTokenMissing()) {
      return;
    }

    this.submitted.set(true);
    this.normalizedError.set(undefined);
    this.successMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.normalizedError.set(this.describeClientValidationFailure());
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });

    try {
      await firstValueFrom(
        this.resetPasswordApi.resetPassword({
          email: this.emailValue,
          token: this.resetToken,
          newPassword: this.newPasswordValue,
        }),
      );
      this.successMessage.set(SUCCESS_MESSAGE);
    } catch (error: unknown) {
      this.normalizedError.set(normalizeProblemDetails(this.errorBody(error)));
      this.form.enable({ emitEvent: false });
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private describeClientValidationFailure(): NormalizedProblemDetails {
    const errors: Record<string, readonly string[]> = {};
    const emailControl = this.form.controls.email;
    const passwordControl = this.form.controls.newPassword;

    if (emailControl.hasError('required')) {
      errors['Email'] = ["'Email' must not be empty."];
    } else if (emailControl.hasError('email')) {
      errors['Email'] = ["'Email' is not a valid email address."];
    }

    if (passwordControl.hasError('required')) {
      errors['NewPassword'] = ["'New Password' must not be empty."];
    } else if (passwordControl.hasError('minlength')) {
      errors['NewPassword'] = ["'New Password' must be at least 8 characters."];
    } else if (passwordControl.hasError('pattern')) {
      const value = this.newPasswordValue;
      errors['NewPassword'] = !/[A-Z]/.test(value)
        ? ['Password must contain at least one uppercase letter.']
        : ['Password must contain at least one digit.'];
    }

    return {
      kind: 'validation',
      type: '',
      title: 'Validation failed',
      status: 400,
      detail: '',
      traceId: '',
      errors,
    };
  }

  private fieldError(field: 'Email' | 'NewPassword'): string {
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
