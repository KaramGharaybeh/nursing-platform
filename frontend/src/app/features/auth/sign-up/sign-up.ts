import { AfterViewInit, Component, ElementRef, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { SignUpApi } from '../../../core/api/sign-up-api';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { NpTextInputControl } from '../../../shared/ui/form-controls';
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

const FIELD_LABELS = Object.freeze({
  Email: 'Email address',
  Username: 'Username',
  Password: 'Password',
  ConfirmPassword: 'Confirm password',
});

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
    NpTextInputControl,
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
      fieldLabels: FIELD_LABELS,
      controlIds: CONTROL_IDS,
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'The account could not be created.',
    },
  ));

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return error.detail.trim() !== '' ? error.detail : 'The account could not be created.';
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
      this.normalizedError.set(normalizeProblemDetails(this.errorBody(error)));
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
      errors['Email'] = ["'Email' must not be empty."];
    } else if (email.hasError('email')) {
      errors['Email'] = ["'Email' is not a valid email address."];
    }
    if (username.hasError('required')) {
      errors['Username'] = ["'Username' must not be empty."];
    }
    if (password.hasError('required')) {
      errors['Password'] = ["'Password' must not be empty."];
    } else if (password.hasError('minlength')) {
      errors['Password'] = ["'Password' must be at least 8 characters."];
    } else if (password.hasError('pattern')) {
      errors['Password'] = !/[A-Z]/.test(this.passwordValue)
        ? ['Password must contain at least one uppercase letter.']
        : ['Password must contain at least one digit.'];
    }
    if (confirm.hasError('required')) {
      errors['ConfirmPassword'] = ["'Confirm password' must not be empty."];
    } else if (this.confirmPasswordValue !== this.passwordValue) {
      errors['ConfirmPassword'] = ['Confirm password must match password.'];
    }

    if (Object.keys(errors).length === 0) {
      return undefined;
    }
    return { kind: 'validation', type: '', title: 'Validation failed', status: 400, detail: '', traceId: '', errors };
  }

  private fieldError(field: keyof typeof FIELD_LABELS): string {
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
