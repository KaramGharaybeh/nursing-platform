import { AfterViewInit, Component, ElementRef, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { SignUpApi } from '../../../core/api/sign-up-api';
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

type SignUpForm = FormGroup<{
  email: FormControl<string>;
  username: FormControl<string>;
  password: FormControl<string>;
  confirmPassword: FormControl<string>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  Email: 'auth.emailLabel',
  Username: 'signup.usernameLabel',
  Password: 'auth.passwordLabel',
  ConfirmPassword: 'signup.confirmLabel',
} as const);

const CONTROL_IDS = Object.freeze({
  Email: 'auth-sign-up-email',
  Username: 'auth-sign-up-username',
  Password: 'auth-sign-up-password',
  ConfirmPassword: 'auth-sign-up-confirm-password',
});

@Component({
  selector: 'np-sign-up',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    AuthTextField,
    NpLanguageSwitcher,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './sign-up.html',
  styleUrl: './sign-up.scss',
})
export class SignUp implements AfterViewInit {
  private readonly signUpApi = inject(SignUpApi);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef);
  protected readonly i18n = inject(LocalizationService);

  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly checkEmailPath = canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST');

  protected readonly form: SignUpForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    username: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/[A-Z]/),
        Validators.pattern(/[0-9]/),
      ],
    }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly isSubmitting = signal(false);
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly validationSummary = computed(() => toFormValidationSummary(
    this.normalizedError(),
    {
      fieldLabels: {
        Email: this.i18n.t(FIELD_LABEL_KEYS.Email),
        Username: this.i18n.t(FIELD_LABEL_KEYS.Username),
        Password: this.i18n.t(FIELD_LABEL_KEYS.Password),
        ConfirmPassword: this.i18n.t(FIELD_LABEL_KEYS.ConfirmPassword),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('signup.formFallback'),
    },
  ));

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'signup.formFallback');
  });

  protected get emailValue(): string {
    return this.form.controls.email.value;
  }

  protected get usernameValue(): string {
    return this.form.controls.username.value;
  }

  protected get passwordValue(): string {
    return this.form.controls.password.value;
  }

  protected get confirmPasswordValue(): string {
    return this.form.controls.confirmPassword.value;
  }

  protected get emailError(): string {
    return this.fieldError('Email');
  }

  protected get usernameError(): string {
    return this.fieldError('Username');
  }

  protected get passwordError(): string {
    return this.fieldError('Password');
  }

  protected get confirmPasswordError(): string {
    return this.fieldError('ConfirmPassword');
  }

  ngAfterViewInit(): void {
    this.applyAutocompleteSemantics();
  }

  protected updateEmail(value: string): void {
    this.form.controls.email.setValue(value);
    this.clearFeedback();
  }

  protected updateUsername(value: string): void {
    this.form.controls.username.setValue(value);
    this.clearFeedback();
  }

  protected updatePassword(value: string): void {
    this.form.controls.password.setValue(value);
    this.clearFeedback();
  }

  protected updateConfirmPassword(value: string): void {
    this.form.controls.confirmPassword.setValue(value);
    this.clearFeedback();
  }

  protected async submit(): Promise<void> {
    this.submitted.set(true);
    this.normalizedError.set(undefined);

    const validationFailure = this.clientValidationFailure();
    if (validationFailure !== undefined) {
      this.form.markAllAsTouched();
      this.normalizedError.set(validationFailure);
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });

    try {
      await firstValueFrom(this.signUpApi.signUp({
        email: this.emailValue,
        username: this.usernameValue,
        password: this.passwordValue,
      }));
      await this.router.navigateByUrl(this.checkEmailPath);
    } catch (error: unknown) {
      this.normalizedError.set(
        this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), FIELD_LABEL_KEYS),
      );
      this.form.enable({ emitEvent: false });
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private clientValidationFailure(): NormalizedProblemDetails | undefined {
    const errors: Record<string, readonly string[]> = {};
    const email = this.form.controls.email;
    const username = this.form.controls.username;
    const password = this.form.controls.password;
    const confirm = this.form.controls.confirmPassword;

    if (email.hasError('required')) {
      errors['Email'] = [this.i18n.t('auth.emailEmpty')];
    } else if (email.hasError('email')) {
      errors['Email'] = [this.i18n.t('auth.emailInvalid')];
    }
    if (username.hasError('required')) {
      errors['Username'] = [this.i18n.t('signup.usernameEmpty')];
    }
    if (password.hasError('required')) {
      errors['Password'] = [this.i18n.t('auth.passwordEmpty')];
    } else if (password.hasError('minlength')) {
      errors['Password'] = [this.i18n.t('auth.passwordMin')];
    } else if (password.hasError('pattern')) {
      errors['Password'] = !/[A-Z]/.test(this.passwordValue)
        ? [this.i18n.t('auth.passwordUpper')]
        : [this.i18n.t('auth.passwordDigit')];
    }
    if (confirm.hasError('required')) {
      errors['ConfirmPassword'] = [this.i18n.t('signup.confirmEmpty')];
    } else if (this.confirmPasswordValue !== this.passwordValue) {
      errors['ConfirmPassword'] = [this.i18n.t('signup.confirmMismatch')];
    }

    if (Object.keys(errors).length === 0) {
      return undefined;
    }
    return { kind: 'validation', type: '', title: this.i18n.t('auth.validationFailed'), status: 400, detail: '', traceId: '', errors };
  }

  private fieldError(field: keyof typeof FIELD_LABEL_KEYS): string {
    if (!this.submitted()) {
      return '';
    }
    return toFieldErrorText(this.normalizedError()?.errors?.[field]);
  }

  private clearFeedback(): void {
    this.normalizedError.set(undefined);
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
    root.querySelector(`#${CONTROL_IDS.Username}`)?.setAttribute('autocomplete', 'username');
    root.querySelector(`#${CONTROL_IDS.Password}`)?.setAttribute('autocomplete', 'new-password');
    root.querySelector(`#${CONTROL_IDS.ConfirmPassword}`)?.setAttribute('autocomplete', 'new-password');
  }
}
