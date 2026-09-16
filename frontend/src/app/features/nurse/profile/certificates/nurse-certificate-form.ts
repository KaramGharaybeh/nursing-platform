import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import {
  NpDateControl,
  NpTextInputControl,
} from '../../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFieldErrorText, toFormValidationSummary } from '../../../../shared/ui/form-validation';

const NAME_MAX = 200;
const ISSUING_ORGANIZATION_MAX = 200;
const CREDENTIAL_ID_MAX = 200;
const CREDENTIAL_URL_MAX = 500;

export interface NurseCertificateFormValue {
  readonly name: string;
  readonly issuingOrganization: string;
  readonly issueDate: string | null;
  readonly expirationDate: string | null;
  readonly credentialId: string | null;
  readonly credentialUrl: string | null;
}

type CertificateForm = FormGroup<{
  name: FormControl<string>;
  issuingOrganization: FormControl<string>;
  issueDate: FormControl<string>;
  expirationDate: FormControl<string>;
  credentialId: FormControl<string>;
  credentialUrl: FormControl<string>;
}>;

const FIELD_LABELS = Object.freeze({
  Name: 'Certificate name',
  IssuingOrganization: 'Issuing organization',
  IssueDate: 'Issue date',
  ExpirationDate: 'Expiration date',
  CredentialId: 'Credential ID',
  CredentialUrl: 'Credential URL',
});

const CONTROL_IDS = Object.freeze({
  Name: 'nurse-certificate-name',
  IssuingOrganization: 'nurse-certificate-issuing-organization',
  IssueDate: 'nurse-certificate-issue-date',
  ExpirationDate: 'nurse-certificate-expiration-date',
  CredentialId: 'nurse-certificate-credential-id',
  CredentialUrl: 'nurse-certificate-credential-url',
});

export function isAbsoluteHttpUrl(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === 'http:' || url.protocol === 'https:';
  } catch {
    return false;
  }
}

@Component({
  selector: 'np-nurse-certificate-form',
  imports: [
    MatButtonModule,
    NpDateControl,
    NpFormValidationSummary,
    NpTextInputControl,
    ReactiveFormsModule,
  ],
  templateUrl: './nurse-certificate-form.html',
  styleUrl: './nurse-certificate-form.scss',
})
export class NurseCertificateForm implements OnInit {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() initial: NurseCertificateFormValue | undefined = undefined;
  @Input() isSubmitting = false;
  @Input() backendError: NormalizedProblemDetails | undefined = undefined;

  @Output() readonly save = new EventEmitter<NurseCertificateFormValue>();
  @Output() readonly cancelled = new EventEmitter<void>();

  protected readonly form: CertificateForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(NAME_MAX)],
    }),
    issuingOrganization: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(ISSUING_ORGANIZATION_MAX)],
    }),
    issueDate: new FormControl('', { nonNullable: true }),
    expirationDate: new FormControl('', { nonNullable: true }),
    credentialId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(CREDENTIAL_ID_MAX)],
    }),
    credentialUrl: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(CREDENTIAL_URL_MAX)],
    }),
  });

  protected readonly controlIds = CONTROL_IDS;
  private readonly submittedState = { submitted: false };

  protected get submitLabel(): string {
    return this.mode === 'edit' ? 'Save changes' : 'Save certificate';
  }

  protected get nameValue(): string {
    return this.form.controls.name.value;
  }

  protected get issuingOrganizationValue(): string {
    return this.form.controls.issuingOrganization.value;
  }

  protected get issueDateValue(): string {
    return this.form.controls.issueDate.value;
  }

  protected get expirationDateValue(): string {
    return this.form.controls.expirationDate.value;
  }

  protected get credentialIdValue(): string {
    return this.form.controls.credentialId.value;
  }

  protected get credentialUrlValue(): string {
    return this.form.controls.credentialUrl.value;
  }

  protected get nameError(): string {
    return this.fieldError('Name');
  }

  protected get issuingOrganizationError(): string {
    return this.fieldError('IssuingOrganization');
  }

  protected get issueDateError(): string {
    return this.fieldError('IssueDate');
  }

  protected get expirationDateError(): string {
    return this.fieldError('ExpirationDate');
  }

  protected get credentialIdError(): string {
    return this.fieldError('CredentialId');
  }

  protected get credentialUrlError(): string {
    return this.fieldError('CredentialUrl');
  }

  protected get validationSummary() {
    return toFormValidationSummary(this.normalizedError(), {
      fieldLabels: FIELD_LABELS,
      controlIds: CONTROL_IDS,
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'Your certificate could not be saved.',
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return error.detail.trim() !== '' ? error.detail : 'Your certificate could not be saved.';
  }

  ngOnInit(): void {
    if (this.initial !== undefined) {
      this.form.patchValue({
        name: this.initial.name,
        issuingOrganization: this.initial.issuingOrganization,
        issueDate: this.initial.issueDate ?? '',
        expirationDate: this.initial.expirationDate ?? '',
        credentialId: this.initial.credentialId ?? '',
        credentialUrl: this.initial.credentialUrl ?? '',
      });
    }
  }

  protected updateName(value: string): void {
    this.form.controls.name.setValue(value);
    this.dismissBackendError();
  }

  protected updateIssuingOrganization(value: string): void {
    this.form.controls.issuingOrganization.setValue(value);
    this.dismissBackendError();
  }

  protected updateIssueDate(value: string): void {
    this.form.controls.issueDate.setValue(value);
    this.dismissBackendError();
  }

  protected updateExpirationDate(value: string): void {
    this.form.controls.expirationDate.setValue(value);
    this.dismissBackendError();
  }

  protected updateCredentialId(value: string): void {
    this.form.controls.credentialId.setValue(value);
    this.dismissBackendError();
  }

  protected updateCredentialUrl(value: string): void {
    this.form.controls.credentialUrl.setValue(value);
    this.dismissBackendError();
  }

  protected async submit(): Promise<void> {
    this.submittedState.submitted = true;

    const validationFailure = this.clientValidationFailure();
    if (validationFailure !== undefined) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit({
      name: this.nameValue.trim(),
      issuingOrganization: this.issuingOrganizationValue.trim(),
      issueDate: this.emptyToNull(this.issueDateValue),
      expirationDate: this.emptyToNull(this.expirationDateValue),
      credentialId: this.emptyToNull(this.credentialIdValue),
      credentialUrl: this.emptyToNull(this.credentialUrlValue),
    });
  }

  protected cancel(): void {
    this.cancelled.emit();
  }

  private normalizedError(): NormalizedProblemDetails | undefined {
    const clientFailure = this.submittedState.submitted ? this.clientValidationFailure() : undefined;
    return clientFailure ?? this.backendError;
  }

  private clientValidationFailure(): NormalizedProblemDetails | undefined {
    const errors: Record<string, readonly string[]> = {};
    const name = this.form.controls.name;
    const issuingOrganization = this.form.controls.issuingOrganization;
    const issueDate = this.form.controls.issueDate;
    const expirationDate = this.form.controls.expirationDate;
    const credentialId = this.form.controls.credentialId;
    const credentialUrl = this.form.controls.credentialUrl;

    if (name.hasError('required')) {
      errors['Name'] = ['Certificate name is required.'];
    } else if (name.hasError('maxlength')) {
      errors['Name'] = ['Certificate name must be at most 200 characters.'];
    }
    if (issuingOrganization.hasError('required')) {
      errors['IssuingOrganization'] = ['Issuing organization is required.'];
    } else if (issuingOrganization.hasError('maxlength')) {
      errors['IssuingOrganization'] = ['Issuing organization must be at most 200 characters.'];
    }
    if (credentialId.hasError('maxlength')) {
      errors['CredentialId'] = ['Credential ID must be at most 200 characters.'];
    }
    if (credentialUrl.hasError('maxlength')) {
      errors['CredentialUrl'] = ['Credential URL must be at most 500 characters.'];
    } else {
      const urlValue = credentialUrl.value.trim();
      if (urlValue !== '' && !isAbsoluteHttpUrl(urlValue)) {
        errors['CredentialUrl'] = ['Credential URL must be an absolute http or https URL.'];
      }
    }
    const issueValue = issueDate.value.trim();
    const expirationValue = expirationDate.value.trim();
    if (issueValue !== '' && expirationValue !== '' && expirationValue < issueValue) {
      errors['ExpirationDate'] = ['Expiration date must be on or after the issue date.'];
    }

    if (Object.keys(errors).length === 0) {
      return undefined;
    }
    return { kind: 'validation', type: '', title: 'Validation failed', status: 400, detail: '', traceId: '', errors };
  }

  private fieldError(field: keyof typeof FIELD_LABELS): string {
    if (!this.submittedState.submitted) {
      return '';
    }
    return toFieldErrorText(this.normalizedError()?.errors?.[field]);
  }

  private dismissBackendError(): void {
    this.backendError = undefined;
  }

  private emptyToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === '' ? null : trimmed;
  }
}
