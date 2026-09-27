import { AfterViewInit, Component, ElementRef, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { AuthTextField } from '../auth-text-field/auth-text-field';
import { NpLanguageSwitcher } from '../../../shared/ui/language-switcher';
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

const FIELD_LABEL_KEYS = Object.freeze({
  Email: 'auth.emailLabel',
  NewPassword: 'reset.newPasswordLabel',
} as const);

const CONTROL_IDS = Object.freeze({
  Email: 'auth-reset-password-email',
  NewPassword: 'auth-reset-password-new-password',
});

@Component({
  selector: 'np-reset-password',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    AuthTextField,
    NpLanguageSwitcher,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
})
export class ResetPassword implements AfterViewInit {
  private readonly resetPasswordApi = inject(ResetPasswordApi);
  private readonly host = inject(ElementRef);
  protected readonly i18n = inject(LocalizationService);
  private readonly resetToken: string =
    (inject(ActivatedRoute).snapshot.queryParamMap.get('token') ?? '').trim();

  protected readonly forgotPasswordPath = canonicalRoutePath('AUTH_FORGOT_PASSWORD');
  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly signUpPath = canonicalRoutePath('AUTH_SIGN_UP');

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

  ngAfterViewInit(): void {
    this.applyAutocompleteSemantics();
  }

  protected readonly isTokenMissing = computed(() => this.resetToken === '');

  protected readonly tokenMissingMessage = computed(() =>
    this.isTokenMissing() ? this.i18n.t('reset.missing') : '',
  );

  protected readonly validationSummary = computed(() =>
    toFormValidationSummary(this.normalizedError(), {
      fieldLabels: {
        Email: this.i18n.t(FIELD_LABEL_KEYS.Email),
        NewPassword: this.i18n.t(FIELD_LABEL_KEYS.NewPassword),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('reset.formFallback'),
    }),
  );

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'reset.formFallback');
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
      this.successMessage.set(this.i18n.t('reset.success'));
    } catch (error: unknown) {
      this.normalizedError.set(
        this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), FIELD_LABEL_KEYS),
      );
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
      errors['Email'] = [this.i18n.t('auth.emailEmpty')];
    } else if (emailControl.hasError('email')) {
      errors['Email'] = [this.i18n.t('auth.emailInvalid')];
    }

    if (passwordControl.hasError('required')) {
      errors['NewPassword'] = [this.i18n.t('reset.newPasswordEmpty')];
    } else if (passwordControl.hasError('minlength')) {
      errors['NewPassword'] = [this.i18n.t('reset.newPasswordMin')];
    } else if (passwordControl.hasError('pattern')) {
      const value = this.newPasswordValue;
      errors['NewPassword'] = !/[A-Z]/.test(value)
        ? [this.i18n.t('auth.passwordUpper')]
        : [this.i18n.t('auth.passwordDigit')];
    }

    return {
      kind: 'validation',
      type: '',
      title: this.i18n.t('auth.validationFailed'),
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

  private applyAutocompleteSemantics(): void {
    const root = this.host.nativeElement as HTMLElement;
    root.querySelector(`#${CONTROL_IDS.Email}`)?.setAttribute('autocomplete', 'email');
    root.querySelector(`#${CONTROL_IDS.NewPassword}`)?.setAttribute('autocomplete', 'new-password');
  }
}
