import { AfterViewInit, Component, ElementRef, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
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
import { ForgotPasswordApi } from './forgot-password-api';

type ForgotPasswordForm = FormGroup<{
  email: FormControl<string>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  Email: 'auth.emailLabel',
} as const);

const CONTROL_IDS = Object.freeze({
  Email: 'auth-forgot-password-email',
});

const EMAIL_VALIDATION_ERROR_KEY = 'auth.emailInvalid' as const;

@Component({
  selector: 'np-forgot-password',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    AuthTextField,
    NpLanguageSwitcher,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword implements AfterViewInit {
  private readonly forgotPasswordApi = inject(ForgotPasswordApi);
  private readonly host = inject(ElementRef);
  protected readonly i18n = inject(LocalizationService);

  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly signUpPath = canonicalRoutePath('AUTH_SIGN_UP');

  protected readonly form: ForgotPasswordForm = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
  });

  protected readonly isSubmitting = signal(false);
  protected readonly successMessage = signal('');
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  ngAfterViewInit(): void {
    this.applyAutocompleteSemantics();
  }

  protected readonly validationSummary = computed(() => toFormValidationSummary(
    this.normalizedError(),
    {
      fieldLabels: {
        Email: this.i18n.t(FIELD_LABEL_KEYS.Email),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('forgot.formFallback'),
    },
  ));

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'forgot.formFallback');
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
      this.normalizedError.set(this.emailValue.trim() === '' ? this.requiredValidation() : this.emailValidation());
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });

    try {
      await firstValueFrom(this.forgotPasswordApi.requestPasswordReset({ email: this.emailValue }));
      this.successMessage.set(this.i18n.t('forgot.success'));
    } catch (error: unknown) {
      this.normalizedError.set(
        this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), FIELD_LABEL_KEYS),
      );
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

  private singleFieldValidation(messageKey: 'auth.emailEmpty' | 'auth.emailInvalid'): NormalizedProblemDetails {
    return {
      kind: 'validation',
      type: '',
      title: this.i18n.t('auth.validationFailed'),
      status: 400,
      detail: '',
      traceId: '',
      errors: { Email: [this.i18n.t(messageKey)] },
    };
  }

  private requiredValidation(): NormalizedProblemDetails {
    return this.singleFieldValidation('auth.emailEmpty');
  }

  private emailValidation(): NormalizedProblemDetails {
    return this.singleFieldValidation(EMAIL_VALIDATION_ERROR_KEY);
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
  }
}
