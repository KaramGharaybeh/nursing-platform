import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NurseProfileDto } from '../../../../core/api/generated/models/nurse-profile-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../../core/routing/canonical-routes';
import { Router } from '@angular/router';
import { LocalizationService } from '../../../../core/i18n/localization.service';
import { NpCheckboxControl, NpSelectControl, NpTextInputControl, NpTextareaControl } from '../../../../shared/ui/form-controls';
import type { NpSelectOption } from '../../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../../../shared/ui/form-validation';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';

const HEADLINE_MAX = 160;
const PROFESSIONAL_SUMMARY_MAX = 2000;
const LICENSE_NUMBER_MAX = 100;
const YEARS_MIN = 0;
const YEARS_MAX = 80;

type PersonalInformationForm = FormGroup<{
  headline: FormControl<string>;
  professionalSummary: FormControl<string>;
  licenseNumber: FormControl<string>;
  licenseCountryId: FormControl<string>;
  currentCountryId: FormControl<string>;
  yearsOfExperience: FormControl<number>;
  isAvailableForRecruitment: FormControl<boolean>;
}>;

const FIELD_LABEL_KEYS = Object.freeze({
  Headline: 'profile.headlineLabel',
  ProfessionalSummary: 'profile.summaryLabel',
  LicenseNumber: 'profile.licenseNumberLabel',
  YearsOfExperience: 'profile.yearsOfExperience',
} as const);

const CONTROL_IDS = Object.freeze({
  Headline: 'nurse-personal-information-headline',
  ProfessionalSummary: 'nurse-personal-information-professional-summary',
  LicenseNumber: 'nurse-personal-information-license-number',
  LicenseCountry: 'nurse-personal-information-license-country',
  CurrentCountry: 'nurse-personal-information-current-country',
  YearsOfExperience: 'nurse-personal-information-years-of-experience',
});

@Component({
  selector: 'np-nurse-personal-information',
  imports: [
    LoadingErrorRetry,
    MatButtonModule,
    NpCheckboxControl,
    NpFormValidationSummary,
    NpSelectControl,
    NpTextInputControl,
    NpTextareaControl,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './nurse-personal-information.html',
  styleUrl: './nurse-personal-information.scss',
})
export class NursePersonalInformation implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly router = inject(Router);
  protected readonly i18n = inject(LocalizationService);

  protected readonly overviewPath = canonicalRoutePath('NURSE_PROFILE_OVERVIEW');

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly countries = signal<readonly CountryListItemDto[]>([]);
  protected readonly isSubmitting = signal(false);
  private readonly submitted = signal(false);
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);
  private readonly isCreateMode = signal(true);

  protected readonly form: PersonalInformationForm = new FormGroup({
    headline: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(HEADLINE_MAX)] }),
    professionalSummary: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(PROFESSIONAL_SUMMARY_MAX)],
    }),
    licenseNumber: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(LICENSE_NUMBER_MAX)] }),
    licenseCountryId: new FormControl('', { nonNullable: true }),
    currentCountryId: new FormControl('', { nonNullable: true }),
    yearsOfExperience: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(YEARS_MIN), Validators.max(YEARS_MAX)],
    }),
    isAvailableForRecruitment: new FormControl(false, { nonNullable: true }),
  });

  protected get title(): string {
    return this.isCreateMode() ? this.i18n.t('profile.addPersonalInfo') : this.i18n.t('profile.editPersonalInfo');
  }

  protected get countryOptions(): readonly NpSelectOption[] {
    return this.countries().map((country) => ({ value: country.id, label: country.name }));
  }

  protected get headlineValue(): string {
    return this.form.controls.headline.value;
  }

  protected get professionalSummaryValue(): string {
    return this.form.controls.professionalSummary.value;
  }

  protected get licenseNumberValue(): string {
    return this.form.controls.licenseNumber.value;
  }

  protected get licenseCountryValue(): string {
    return this.form.controls.licenseCountryId.value;
  }

  protected get currentCountryValue(): string {
    return this.form.controls.currentCountryId.value;
  }

  protected get yearsOfExperienceValue(): string {
    return this.form.controls.yearsOfExperience.value.toString();
  }

  protected get availabilityValue(): boolean {
    return this.form.controls.isAvailableForRecruitment.value;
  }

  protected get headlineError(): string {
    return this.fieldError('Headline');
  }

  protected get professionalSummaryError(): string {
    return this.fieldError('ProfessionalSummary');
  }

  protected get licenseNumberError(): string {
    return this.fieldError('LicenseNumber');
  }

  protected get yearsOfExperienceError(): string {
    return this.fieldError('YearsOfExperience');
  }

  protected get validationSummary() {
    return toFormValidationSummary(this.normalizedError(), {
      fieldLabels: {
        Headline: this.i18n.t(FIELD_LABEL_KEYS.Headline),
        ProfessionalSummary: this.i18n.t(FIELD_LABEL_KEYS.ProfessionalSummary),
        LicenseNumber: this.i18n.t(FIELD_LABEL_KEYS.LicenseNumber),
        YearsOfExperience: this.i18n.t(FIELD_LABEL_KEYS.YearsOfExperience),
      },
      controlIds: CONTROL_IDS,
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('pi.formFallback'),
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'pi.formFallback');
  }

  ngOnInit(): void {
    void this.load();
  }

  protected updateHeadline(value: string): void {
    this.form.controls.headline.setValue(value);
    this.clearFeedback();
  }

  protected updateProfessionalSummary(value: string): void {
    this.form.controls.professionalSummary.setValue(value);
    this.clearFeedback();
  }

  protected updateLicenseNumber(value: string): void {
    this.form.controls.licenseNumber.setValue(value);
    this.clearFeedback();
  }

  protected updateLicenseCountry(value: string): void {
    this.form.controls.licenseCountryId.setValue(value);
    this.clearFeedback();
  }

  protected updateCurrentCountry(value: string): void {
    this.form.controls.currentCountryId.setValue(value);
    this.clearFeedback();
  }

  protected updateYearsOfExperience(value: string): void {
    const parsed = Number.parseInt(value, 10);
    this.form.controls.yearsOfExperience.setValue(Number.isNaN(parsed) ? 0 : parsed);
    this.clearFeedback();
  }

  protected updateAvailability(checked: boolean): void {
    this.form.controls.isAvailableForRecruitment.setValue(checked);
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
      await firstValueFrom(this.api.upsertProfile({
        headline: this.emptyToNull(this.headlineValue),
        professionalSummary: this.emptyToNull(this.professionalSummaryValue),
        licenseNumber: this.emptyToNull(this.licenseNumberValue),
        licenseCountryId: this.emptyToNull(this.licenseCountryValue),
        currentCountryId: this.emptyToNull(this.currentCountryValue),
        yearsOfExperience: this.form.controls.yearsOfExperience.value,
        isAvailableForRecruitment: this.availabilityValue,
      }));
      await this.router.navigateByUrl(this.overviewPath);
    } catch (error: unknown) {
      this.normalizedError.set(
        this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), FIELD_LABEL_KEYS),
      );
      this.form.enable({ emitEvent: false });
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const [countries, profileResult] = await Promise.all([
        this.loadCountries(),
        this.loadProfile(),
      ]);

      this.countries.set(countries);
      this.isCreateMode.set(profileResult === undefined);

      if (profileResult !== undefined) {
        this.form.patchValue({
          headline: profileResult.headline ?? '',
          professionalSummary: profileResult.professionalSummary ?? '',
          licenseNumber: profileResult.licenseNumber ?? '',
          licenseCountryId: profileResult.licenseCountryId ?? '',
          currentCountryId: profileResult.currentCountryId ?? '',
          yearsOfExperience: profileResult.yearsOfExperience,
          isAvailableForRecruitment: profileResult.isAvailableForRecruitment,
        });
      }

      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
    }
  }

  private async loadCountries(): Promise<readonly CountryListItemDto[]> {
    try {
      return await firstValueFrom(this.countriesApi.list());
    } catch {
      return [];
    }
  }

  private async loadProfile(): Promise<NurseProfileDto | undefined> {
    try {
      return await firstValueFrom(this.api.getProfile());
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        return undefined;
      }
      throw error;
    }
  }

  private clientValidationFailure(): NormalizedProblemDetails | undefined {
    const errors: Record<string, readonly string[]> = {};
    const headline = this.form.controls.headline;
    const professionalSummary = this.form.controls.professionalSummary;
    const licenseNumber = this.form.controls.licenseNumber;
    const years = this.form.controls.yearsOfExperience;

    if (headline.hasError('maxlength')) {
      errors['Headline'] = [this.i18n.t('pi.headlineMax')];
    }
    if (professionalSummary.hasError('maxlength')) {
      errors['ProfessionalSummary'] = [this.i18n.t('pi.summaryMax')];
    }
    if (licenseNumber.hasError('maxlength')) {
      errors['LicenseNumber'] = [this.i18n.t('pi.licenseMax')];
    }
    if (years.hasError('required')) {
      errors['YearsOfExperience'] = [this.i18n.t('pi.yearsRequired')];
    } else if (years.hasError('min') || years.hasError('max')) {
      errors['YearsOfExperience'] = [this.i18n.t('pi.yearsRange')];
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

  private emptyToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === '' ? null : value.trim();
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 404;
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
