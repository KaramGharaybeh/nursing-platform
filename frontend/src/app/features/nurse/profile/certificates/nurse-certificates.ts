import { MatButtonModule } from '@angular/material/button';
import { Component, OnInit, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../../shared/ui/announcement';
import { LocalizationService } from '../../../../core/i18n/localization.service';
import { TwoStepConfirmation } from '../../../../shared/ui/confirmation';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseCertificateDto } from '../../../../core/api/generated/models/nurse-certificate-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';
import { NurseCertificateForm, type NurseCertificateFormValue } from './nurse-certificate-form';

type CertificatesView = 'list' | 'create' | 'edit';

@Component({
  selector: 'np-nurse-certificates',
  imports: [LoadingErrorRetry, MatButtonModule, NpLiveRegion, NurseCertificateForm],
  templateUrl: './nurse-certificates.html',
  styleUrl: './nurse-certificates.scss',
})
export class NurseCertificates implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);
  private readonly deleteConfirmation = new TwoStepConfirmation();

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly records = signal<readonly NurseCertificateDto[]>([]);
  protected readonly view = signal<CertificatesView>('list');
  protected readonly editing = signal<NurseCertificateDto | undefined>(undefined);
  protected readonly isSubmitting = signal(false);
  protected readonly formError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly deleteTargetId = signal<string | undefined>(undefined);
  protected readonly deletingId = signal<string | undefined>(undefined);
  protected readonly deleteError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly notice = signal('');

  protected get formTitle(): string {
    return this.view() === 'edit' ? this.i18n.t('cert.formTitleEdit') : this.i18n.t('cert.add');
  }

  protected get formInitial(): NurseCertificateFormValue | undefined {
    const record = this.editing();
    if (record === undefined) {
      return undefined;
    }
    return {
      name: record.name,
      issuingOrganization: record.issuingOrganization,
      issueDate: record.issueDate ?? null,
      expirationDate: record.expirationDate ?? null,
      credentialId: record.credentialId ?? null,
      credentialUrl: record.credentialUrl ?? null,
    };
  }

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected startCreate(): void {
    this.editing.set(undefined);
    this.formError.set(undefined);
    this.notice.set('');
    this.view.set('create');
  }

  protected startEdit(record: NurseCertificateDto): void {
    this.editing.set(record);
    this.formError.set(undefined);
    this.notice.set('');
    this.view.set('edit');
  }

  protected cancelForm(): void {
    this.editing.set(undefined);
    this.formError.set(undefined);
    this.view.set('list');
  }

  protected async submitCreate(value: NurseCertificateFormValue): Promise<void> {
    this.isSubmitting.set(true);
    this.formError.set(undefined);
    try {
      await firstValueFrom(
        this.api.createCertificate({
          name: value.name,
          issuingOrganization: value.issuingOrganization,
          issueDate: value.issueDate,
          expirationDate: value.expirationDate,
          credentialId: value.credentialId,
          credentialUrl: value.credentialUrl,
        }),
      );
      await this.reloadRecords();
      this.editing.set(undefined);
      this.view.set('list');
      this.announcer.announce(this.i18n.t('cert.saved'));
    } catch (error: unknown) {
      this.formError.set(normalizeProblemDetails(this.errorBody(error)));
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected async submitEdit(value: NurseCertificateFormValue): Promise<void> {
    const record = this.editing();
    if (record === undefined) {
      return;
    }
    this.isSubmitting.set(true);
    this.formError.set(undefined);
    try {
      await firstValueFrom(
        this.api.updateCertificate(record.id, {
          name: value.name,
          issuingOrganization: value.issuingOrganization,
          issueDate: value.issueDate,
          expirationDate: value.expirationDate,
          credentialId: value.credentialId,
          credentialUrl: value.credentialUrl,
        }),
      );
      await this.reloadRecords();
      this.editing.set(undefined);
      this.view.set('list');
      this.announcer.announce(this.i18n.t('cert.saved'));
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.editing.set(undefined);
        this.view.set('list');
        this.notice.set(this.i18n.t('cert.goneNotice'));
        this.announcer.announce(this.i18n.t('cert.goneAnnounce'));
        await this.reloadRecords();
      } else {
        this.formError.set(normalizeProblemDetails(this.errorBody(error)));
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected requestDelete(record: NurseCertificateDto): void {
    this.deleteConfirmation.request();
    this.deleteTargetId.set(record.id);
    this.deleteError.set(undefined);
    this.announcer.announce(this.i18n.tp('cert.confirmDelete', { name: record.name, issuer: record.issuingOrganization }));
  }

  protected cancelDelete(): void {
    this.deleteConfirmation.cancel();
    this.deleteTargetId.set(undefined);
    this.deleteError.set(undefined);
  }

  protected async confirmDelete(): Promise<void> {
    const id = this.deleteTargetId();
    if (id === undefined || !this.deleteConfirmation.confirm()) {
      return;
    }
    this.deleteTargetId.set(undefined);
    this.deletingId.set(id);
    this.deleteError.set(undefined);
    try {
      await firstValueFrom(this.api.deleteCertificate(id));
      await this.reloadRecords();
      this.announcer.announce(this.i18n.t('cert.deleted'));
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        await this.reloadRecords();
        this.notice.set(this.i18n.t('cert.alreadyRemoved'));
        this.announcer.announce(this.i18n.t('cert.alreadyRemovedAnnounce'));
      } else {
        this.deleteTargetId.set(id);
        this.deleteError.set(
          this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), {}),
        );
      }
    } finally {
      this.deletingId.set(undefined);
    }
  }

  protected isConfirmingDelete(id: string): boolean {
    return this.deleteTargetId() === id;
  }

  protected isDeleting(id: string): boolean {
    return this.deletingId() === id;
  }

  protected deleteErrorFor(id: string): NormalizedProblemDetails | undefined {
    return this.deleteTargetId() === id ? this.deleteError() : undefined;
  }

  /**
   * Date line without status semantics: issue+expiration → "issue → expiration";
   * issue only → issue; expiration only → expiration; neither → '' (line omitted).
   * Uses the TZ-safe calendar-date formatting pattern verified in Nurse
   * Experience/Education so DateOnly values never shift.
   */
  protected dateLine(record: NurseCertificateDto): string {
    const issue = this.formatDate(record.issueDate);
    const expiration = this.formatDate(record.expirationDate);
    if (issue !== '' && expiration !== '') {
      return `${issue} → ${expiration}`;
    }
    if (issue !== '') {
      return issue;
    }
    return expiration;
  }

  protected formatDate(iso: string | null | undefined): string {
    if (iso === null || iso === undefined) {
      return '';
    }
    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso.trim());
    if (match === null) {
      return iso;
    }
    const date = new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(date);
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      this.records.set(await firstValueFrom(this.api.listCertificates()));
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
    }
  }

  private async reloadRecords(): Promise<void> {
    this.records.set(await firstValueFrom(this.api.listCertificates()));
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
