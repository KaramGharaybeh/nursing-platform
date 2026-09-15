import { AfterViewInit, Component, ElementRef, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
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
import { RegisterEmployerApi } from './register-employer-api';

type RegisterEmployerForm = FormGroup<{
  email: FormControl<string>;
  username: FormControl<string>;
  password: FormControl<string>;
}>;

const FIELD_LABELS = Object.freeze({
  Email: 'Email address',
  Username: 'Username',
  Password: 'Password',
});

const CONTROL_IDS = Object.freeze({
  Email: 'auth-register-employer-email',
  Username: 'auth-register-employer-username',
  Password: 'auth-register-employer-password',
});

@Component({
  selector: 'np-register-employer',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    NpTextInputControl,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './register-employer.html',
  styleUrl: './register-employer.scss',
})
export class RegisterEmployer implements AfterViewInit {
  private readonly registerEmployerApi = inject(RegisterEmployerApi);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef);

  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly signUpPath = '/auth/sign-up';
  protected readonly verifyEmailPath = canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST');

  protected readonly form: RegisterEmployerForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    username: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    password: new FormControl('', {
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
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  ngAfterViewInit(): void {
    this.applyAutocompleteSemantics();
  }

  protected readonly validationSummary = computed(() =>
    toFormValidationSummary(this.normalizedError(), {
      fieldLabels: FIELD_LABELS,
      controlIds: CONTROL_IDS,
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'The registration could not be completed.',
    }),
  );

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return error.detail.trim() !== '' ? error.detail : 'The registration could not be completed.';
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

  protected get emailError(): string {
    return this.fieldError('Email');
  }

  protected get usernameError(): string {
    return this.fieldError('Username');
  }

  protected get passwordError(): string {
    return this.fieldError('Password');
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

  protected async submit(): Promise<void> {
    if (this.isSubmitting()) {
      return;
    }

    this.submitted.set(true);
    this.normalizedError.set(undefined);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.normalizedError.set(this.describeClientValidationFailure());
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });

    try {
      await firstValueFrom(
        this.registerEmployerApi.register({
          email: this.emailValue,
          username: this.usernameValue,
          password: this.passwordValue,
        }),
      );
      await this.router.navigateByUrl(this.verifyEmailPath);
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
    const usernameControl = this.form.controls.username;
    const passwordControl = this.form.controls.password;

    if (emailControl.hasError('required')) {
      errors['Email'] = ["'Email' must not be empty."];
    } else if (emailControl.hasError('email')) {
      errors['Email'] = ["'Email' is not a valid email address."];
    }

    if (usernameControl.hasError('required')) {
      errors['Username'] = ["'Username' must not be empty."];
    }

    if (passwordControl.hasError('required')) {
      errors['Password'] = ["'Password' must not be empty."];
    } else if (passwordControl.hasError('minlength')) {
      errors['Password'] = ["'Password' must be at least 8 characters."];
    } else if (passwordControl.hasError('pattern')) {
      const value = this.passwordValue;
      errors['Password'] = !/[A-Z]/.test(value)
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

  private fieldError(field: 'Email' | 'Username' | 'Password'): string {
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
  }
}
