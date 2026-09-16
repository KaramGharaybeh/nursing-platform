import { MatButtonModule } from '@angular/material/button';
import { Component, OnInit, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../../shared/ui/announcement';
import { TwoStepConfirmation } from '../../../../shared/ui/confirmation';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NurseEducationDto } from '../../../../core/api/generated/models/nurse-education-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';
import { NurseEducationForm, type NurseEducationFormValue } from './nurse-education-form';

const DESCRIPTION_PREVIEW_LIMIT = 140;

type EducationView = 'list' | 'create' | 'edit';

@Component({
  selector: 'np-nurse-education',
  imports: [LoadingErrorRetry, MatButtonModule, NpLiveRegion, NurseEducationForm],
  templateUrl: './nurse-education.html',
  styleUrl: './nurse-education.scss',
})
export class NurseEducation implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly announcer = inject(Announcer);
  private readonly deleteConfirmation = new TwoStepConfirmation();

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly records = signal<readonly NurseEducationDto[]>([]);
  protected readonly countries = signal<readonly CountryListItemDto[]>([]);
  protected readonly view = signal<EducationView>('list');
  protected readonly editing = signal<NurseEducationDto | undefined>(undefined);
  protected readonly isSubmitting = signal(false);
  protected readonly formError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly deleteTargetId = signal<string | undefined>(undefined);
  protected readonly deletingId = signal<string | undefined>(undefined);
  protected readonly deleteError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly notice = signal('');
  protected readonly expandedDescriptions = signal<readonly string[]>([]);

  protected get formTitle(): string {
    return this.view() === 'edit' ? 'Edit education' : 'Add education';
  }

  protected get formInitial(): NurseEducationFormValue | undefined {
    const record = this.editing();
    if (record === undefined) {
      return undefined;
    }
    return {
      institutionName: record.institutionName,
      degree: record.degree,
      fieldOfStudy: record.fieldOfStudy ?? null,
      countryId: record.countryId ?? null,
      startDate: record.startDate ?? null,
      endDate: record.endDate ?? null,
      description: record.description ?? null,
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

  protected startEdit(record: NurseEducationDto): void {
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

  protected async submitCreate(value: NurseEducationFormValue): Promise<void> {
    this.isSubmitting.set(true);
    this.formError.set(undefined);
    try {
      await firstValueFrom(
        this.api.createEducation({
          institutionName: value.institutionName,
          degree: value.degree,
          fieldOfStudy: value.fieldOfStudy,
          countryId: value.countryId,
          startDate: value.startDate,
          endDate: value.endDate,
          description: value.description,
        }),
      );
      await this.reloadRecords();
      this.editing.set(undefined);
      this.view.set('list');
      this.announcer.announce('Education saved.');
    } catch (error: unknown) {
      this.formError.set(normalizeProblemDetails(this.errorBody(error)));
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected async submitEdit(value: NurseEducationFormValue): Promise<void> {
    const record = this.editing();
    if (record === undefined) {
      return;
    }
    this.isSubmitting.set(true);
    this.formError.set(undefined);
    try {
      await firstValueFrom(
        this.api.updateEducation(record.id, {
          institutionName: value.institutionName,
          degree: value.degree,
          fieldOfStudy: value.fieldOfStudy,
          countryId: value.countryId,
          startDate: value.startDate,
          endDate: value.endDate,
          description: value.description,
        }),
      );
      await this.reloadRecords();
      this.editing.set(undefined);
      this.view.set('list');
      this.announcer.announce('Education saved.');
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.editing.set(undefined);
        this.view.set('list');
        this.notice.set('This education record no longer exists. The list has been refreshed.');
        this.announcer.announce('This education record no longer exists.');
        await this.reloadRecords();
      } else {
        this.formError.set(normalizeProblemDetails(this.errorBody(error)));
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected requestDelete(record: NurseEducationDto): void {
    this.deleteConfirmation.request();
    this.deleteTargetId.set(record.id);
    this.deleteError.set(undefined);
    this.announcer.announce(`Confirm deletion of ${record.degree} at ${record.institutionName}?`);
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
      await firstValueFrom(this.api.deleteEducation(id));
      await this.reloadRecords();
      this.announcer.announce('Education deleted.');
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        await this.reloadRecords();
        this.notice.set('That education record had already been removed. The list has been refreshed.');
        this.announcer.announce('Education was already removed.');
      } else {
        this.deleteTargetId.set(id);
        this.deleteError.set(normalizeProblemDetails(this.errorBody(error)));
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
   * HD-E1 date line: start+end → "start → end"; start only → "start → Present";
   * end only → "end"; neither → ''. Uses the TZ-safe calendar-date formatting
   * pattern verified in Nurse Experience so DateOnly values never shift.
   */
  protected dateLine(record: NurseEducationDto): string {
    const start = this.formatDate(record.startDate);
    const end = this.formatDate(record.endDate);
    if (start !== '' && end !== '') {
      return `${start} → ${end}`;
    }
    if (start !== '') {
      return `${start} → Present`;
    }
    return end;
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

  protected isDescriptionExpanded(id: string): boolean {
    return this.expandedDescriptions().includes(id);
  }

  protected needsDescriptionToggle(description: string | null | undefined): boolean {
    return (description ?? '').length > DESCRIPTION_PREVIEW_LIMIT;
  }

  protected toggleDescription(id: string): void {
    const expanded = this.expandedDescriptions();
    this.expandedDescriptions.set(
      expanded.includes(id) ? expanded.filter((entry) => entry !== id) : [...expanded, id],
    );
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const [countries, records] = await Promise.all([
        this.loadCountries(),
        firstValueFrom(this.api.listEducation()),
      ]);
      this.countries.set(countries);
      this.records.set(records);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
    }
  }

  private async reloadRecords(): Promise<void> {
    this.records.set(await firstValueFrom(this.api.listEducation()));
  }

  private async loadCountries(): Promise<readonly CountryListItemDto[]> {
    try {
      return await firstValueFrom(this.countriesApi.list());
    } catch {
      return [];
    }
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
