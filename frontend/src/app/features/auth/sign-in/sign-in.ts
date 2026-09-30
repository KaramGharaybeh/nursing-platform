import { AfterViewInit, Component, ElementRef, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthTransport } from '../../../core/api/auth-transport';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { AuthSessionBootstrap } from '../../../core/auth/auth-session-bootstrap';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { RETURN_URL_QUERY_KEY, isSafeReturnUrl } from '../../../core/routing/safe-return';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { AuthTextField } from '../auth-text-field/auth-text-field';
import { NpLanguageSwitcher } from '../../../shared/ui/language-switcher';
import {
  NpFormValidationSummary,
  toFieldErrorText,
  toFormValidationSummary,
} from '../../../shared/ui/form-validation';

type SignInForm = FormGroup<{
  email: FormControl<string>;
  password: FormControl<string>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  Email: 'auth.emailLabel',
  Password: 'auth.passwordLabel',
} as const);

const CONTROL_IDS = Object.freeze({
  Email: 'auth-sign-in-email',
  Password: 'auth-sign-in-password',
});

const EMAIL_VERIFICATION_REQUIRED_CODE = 'email_verification_required';

@Component({
  selector: 'np-sign-in',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    AuthTextField,
    NpLanguageSwitcher,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './sign-in.html',
  styleUrl: './sign-in.scss',
})
export class SignIn implements AfterViewInit {
  private readonly authTransport = inject(AuthTransport);
  private readonly authSession = inject(AuthSessionBootstrap);
  private readonly currentUserStore = inject(CurrentUserStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef);
  protected readonly i18n = inject(LocalizationService);

  protected readonly forgotPasswordPath = canonicalRoutePath('AUTH_FORGOT_PASSWORD');
  protected readonly signUpPath = canonicalRoutePath('AUTH_SIGN_UP');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');

  protected readonly form: SignInForm = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly isSubmitting = signal(false);
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly validationSummary = computed(() => toFormValidationSummary(
    this.normalizedError(),
    {
      fieldLabels: {
        Email: this.i18n.t(FIELD_LABEL_KEYS.Email),
        Password: this.i18n.t(FIELD_LABEL_KEYS.Password),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('auth.formFallback'),
    },
  ));

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    if (this.isEmailVerificationRequired(error)) {
      return this.i18n.t('signin.verifyRequired');
    }
    return this.i18n.backendErrorCopy(error.detail, 'auth.formFallback');
  });

  protected get emailValue(): string {
    return this.form.controls.email.value;
  }

  protected get passwordValue(): string {
    return this.form.controls.password.value;
  }

  protected get emailError(): string {
    return this.fieldError('Email');
  }

  protected get passwordError(): string {
    return this.fieldError('Password');
  }

  ngAfterViewInit(): void {
    this.applyAutocompleteSemantics();
  }

  protected updateEmail(value: string): void {
    this.form.controls.email.setValue(value);
    this.clearServerError();
  }

  protected updatePassword(value: string): void {
    this.form.controls.password.setValue(value);
    this.clearServerError();
  }

  protected async submit(): Promise<void> {
    this.submitted.set(true);
    this.normalizedError.set(undefined);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.normalizedError.set(this.requiredValidation());
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });

    try {
      const result = await firstValueFrom(this.authTransport.login({
        email: this.emailValue,
        password: this.passwordValue,
      }));
      this.authSession.establishAuthenticatedSession(result);
      await firstValueFrom(this.currentUserStore.hydrate());
      await this.router.navigateByUrl(this.postLoginDestination());
    } catch (error: unknown) {
      this.normalizedError.set(
        this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), FIELD_LABEL_KEYS),
      );
      this.form.enable({ emitEvent: false });
      this.isSubmitting.set(false);
    }
  }

  private postLoginDestination(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get(RETURN_URL_QUERY_KEY);
    if (returnUrl !== null && isSafeReturnUrl(returnUrl)) {
      return returnUrl;
    }
    return canonicalRoutePath('ACCOUNT_OVERVIEW');
  }

  private fieldError(field: 'Email' | 'Password'): string {
    if (!this.submitted()) {
      return '';
    }
    return toFieldErrorText(this.normalizedError()?.errors?.[field]);
  }

  private requiredValidation(): NormalizedProblemDetails {
    return {
      kind: 'validation',
      type: '',
      title: this.i18n.t('auth.validationFailed'),
      status: 400,
      detail: '',
      traceId: '',
      errors: {
        Email: [this.i18n.t('auth.emailEmpty')],
        Password: [this.i18n.t('auth.passwordEmpty')],
      },
    };
  }

  private clearServerError(): void {
    this.normalizedError.set(undefined);
  }

  private isEmailVerificationRequired(error: NormalizedProblemDetails): boolean {
    return error.kind === 'coded'
      && error.status === 403
      && error.code === EMAIL_VERIFICATION_REQUIRED_CODE;
  }

  private applyAutocompleteSemantics(): void {
    const root = this.host.nativeElement as HTMLElement;
    root.querySelector(`#${CONTROL_IDS.Email}`)?.setAttribute('autocomplete', 'email');
    root.querySelector(`#${CONTROL_IDS.Password}`)?.setAttribute('autocomplete', 'current-password');
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
