import { Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ProfileApi } from '../../../core/api/profile-api';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { RETURN_URL_QUERY_KEY, isSafeReturnUrl } from '../../../core/routing/safe-return';
import { NpTextInputControl } from '../../../shared/ui/form-controls';
import {
  NpFormValidationSummary,
  toFieldErrorText,
  toFormValidationSummary,
} from '../../../shared/ui/form-validation';

type ProfileForm = FormGroup<{
  firstName: FormControl<string>;
  lastName: FormControl<string>;
}>;

const FIELD_LABELS = Object.freeze({ FirstName: 'First name', LastName: 'Last name' });
const CONTROL_IDS = Object.freeze({
  FirstName: 'onboarding-profile-first-name',
  LastName: 'onboarding-profile-last-name',
});

@Component({
  selector: 'np-profile-onboarding',
  imports: [MatButtonModule, NpFormValidationSummary, NpTextInputControl, ReactiveFormsModule],
  templateUrl: './profile-onboarding.html',
  styleUrl: './profile-onboarding.scss',
})
export class ProfileOnboarding {
  private readonly profileApi = inject(ProfileApi);
  private readonly currentUserStore = inject(CurrentUserStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly form: ProfileForm = new FormGroup({
    firstName: new FormControl(this.currentUserStore.currentUser()?.firstName ?? '', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    lastName: new FormControl(this.currentUserStore.currentUser()?.lastName ?? '', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
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
      formErrorFallback: 'Profile could not be saved.',
    },
  ));

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return error.detail.trim() !== '' ? error.detail : 'Profile could not be saved.';
  });

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

  protected updateFirstName(value: string): void {
    this.form.controls.firstName.setValue(value);
    this.normalizedError.set(undefined);
  }

  protected updateLastName(value: string): void {
    this.form.controls.lastName.setValue(value);
    this.normalizedError.set(undefined);
  }

  protected async submit(): Promise<void> {
    this.submitted.set(true);
    this.normalizedError.set(undefined);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.normalizedError.set(this.validationFailure());
      return;
    }

    this.isSubmitting.set(true);
    this.form.disable({ emitEvent: false });
    try {
      await firstValueFrom(this.profileApi.updateCurrentUserProfile({
        firstName: this.firstNameValue,
        lastName: this.lastNameValue,
      }));
      await firstValueFrom(this.currentUserStore.hydrate());
      await this.router.navigateByUrl(this.destination());
    } catch (error: unknown) {
      this.normalizedError.set(normalizeProblemDetails(this.errorBody(error)));
      this.form.enable({ emitEvent: false });
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private destination(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get(RETURN_URL_QUERY_KEY);
    if (returnUrl !== null && isSafeReturnUrl(returnUrl) && returnUrl !== canonicalRoutePath('ONBOARDING_PROFILE')) {
      return returnUrl;
    }
    return canonicalRoutePath('ACCOUNT_OVERVIEW');
  }

  private validationFailure(): NormalizedProblemDetails {
    const errors: Record<string, readonly string[]> = {};
    if (this.form.controls.firstName.hasError('required')) {
      errors['FirstName'] = ["'First name' must not be empty."];
    }
    if (this.form.controls.lastName.hasError('required')) {
      errors['LastName'] = ["'Last name' must not be empty."];
    }
    return { kind: 'validation', type: '', title: 'Validation failed', status: 400, detail: '', traceId: '', errors };
  }

  private fieldError(field: keyof typeof FIELD_LABELS): string {
    if (!this.submitted()) {
      return '';
    }
    return toFieldErrorText(this.normalizedError()?.errors?.[field]);
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
