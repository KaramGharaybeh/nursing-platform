import { Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
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
import { RegisterNurseApi } from './register-nurse-api';

type RegisterNurseForm = FormGroup<{
  email: FormControl<string>;
  password: FormControl<string>;
  firstName: FormControl<string>;
  lastName: FormControl<string>;
}>;

const FIELD_LABELS = Object.freeze({
  Email: 'Email address',
  Password: 'Password',
  FirstName: 'First name',
  LastName: 'Last name',
});

const CONTROL_IDS = Object.freeze({
  Email: 'auth-register-nurse-email',
  Password: 'auth-register-nurse-password',
  FirstName: 'auth-register-nurse-first-name',
  LastName: 'auth-register-nurse-last-name',
});

@Component({
  selector: 'np-register-nurse',
  imports: [
    MatButtonModule,
    NpFormValidationSummary,
    NpTextInputControl,
    ReactiveFormsModule,
  ],
  templateUrl: './register-nurse.html',
  styleUrl: './register-nurse.scss',
})
export class RegisterNurse {
  private readonly registerNurseApi = inject(RegisterNurseApi);
  private readonly router = inject(Router);

  protected readonly verifyEmailPath = canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST');

  protected readonly form: RegisterNurseForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
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
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  protected readonly isSubmitting = signal(false);
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

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

  protected get passwordValue(): string {
    return this.form.controls.password.value;
  }

  protected get firstNameValue(): string {
    return this.form.controls.firstName.value;
  }

  protected get lastNameValue(): string {
    return this.form.controls.lastName.value;
  }

  protected get emailError(): string {
    return this.fieldError('Email');
  }

  protected get passwordError(): string {
    return this.fieldError('Password');
  }

  protected get firstNameError(): string {
    return this.fieldError('FirstName');
  }

  protected get lastNameError(): string {
    return this.fieldError('LastName');
  }

  protected updateEmail(value: string): void {
    this.form.controls.email.setValue(value);
    this.clearFeedback();
  }

  protected updatePassword(value: string): void {
    this.form.controls.password.setValue(value);
    this.clearFeedback();
  }

  protected updateFirstName(value: string): void {
    this.form.controls.firstName.setValue(value);
    this.clearFeedback();
  }

  protected updateLastName(value: string): void {
    this.form.controls.lastName.setValue(value);
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
        this.registerNurseApi.register({
          email: this.emailValue,
          password: this.passwordValue,
          firstName: this.firstNameValue,
          lastName: this.lastNameValue,
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
    const passwordControl = this.form.controls.password;
    const firstNameControl = this.form.controls.firstName;
    const lastNameControl = this.form.controls.lastName;

    if (emailControl.hasError('required')) {
      errors['Email'] = ["'Email' must not be empty."];
    } else if (emailControl.hasError('email')) {
      errors['Email'] = ["'Email' is not a valid email address."];
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

    if (firstNameControl.hasError('required')) {
      errors['FirstName'] = ["'First Name' must not be empty."];
    }

    if (lastNameControl.hasError('required')) {
      errors['LastName'] = ["'Last Name' must not be empty."];
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

  private fieldError(field: 'Email' | 'Password' | 'FirstName' | 'LastName'): string {
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
}
