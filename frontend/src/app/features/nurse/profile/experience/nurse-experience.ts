import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { Component, OnInit, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../../shared/ui/announcement';
import { TwoStepConfirmation } from '../../../../shared/ui/confirmation';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NurseExperienceDto } from '../../../../core/api/generated/models/nurse-experience-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';
import { NurseExperienceForm, type NurseExperienceFormValue } from './nurse-experience-form';

const DESCRIPTION_PREVIEW_LIMIT = 140;

type ExperienceView = 'list' | 'create' | 'edit';

@Component({
  selector: 'np-nurse-experience',
  imports: [DatePipe, LoadingErrorRetry, MatButtonModule, NpLiveRegion, NurseExperienceForm],
  templateUrl: './nurse-experience.html',
  styleUrl: './nurse-experience.scss',
})
export class NurseExperience implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly countriesApi = inject(CountriesApi);
  private readonly announcer = inject(Announcer);
  private readonly deleteConfirmation = new TwoStepConfirmation();

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly experiences = signal<readonly NurseExperienceDto[]>([]);
  protected readonly countries = signal<readonly CountryListItemDto[]>([]);
  protected readonly view = signal<ExperienceView>('list');
  protected readonly editing = signal<NurseExperienceDto | undefined>(undefined);
  protected readonly isSubmitting = signal(false);
  protected readonly formError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly deleteTargetId = signal<string | undefined>(undefined);
  protected readonly deletingId = signal<string | undefined>(undefined);
  protected readonly deleteError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly notice = signal('');
  protected readonly expandedDescriptions = signal<readonly string[]>([]);

  protected get formTitle(): string {
    return this.view() === 'edit' ? 'Edit experience' : 'Add experience';
  }

  protected get formInitial(): NurseExperienceFormValue | undefined {
    const record = this.editing();
    if (record === undefined) {
      return undefined;
    }
    return {
      facilityName: record.facilityName,
      jobTitle: record.jobTitle,
      countryId: record.countryId ?? null,
      startDate: record.startDate,
      endDate: record.endDate ?? null,
      isCurrent: record.isCurrent,
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

  protected startEdit(record: NurseExperienceDto): void {
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

  protected async submitCreate(value: NurseExperienceFormValue): Promise<void> {
    this.isSubmitting.set(true);
    this.formError.set(undefined);
    try {
      await firstValueFrom(
        this.api.createExperience({
          facilityName: value.facilityName,
          jobTitle: value.jobTitle,
          countryId: value.countryId,
          startDate: value.startDate,
          endDate: value.endDate,
          isCurrent: value.isCurrent,
          description: value.description,
        }),
      );
      await this.reloadExperiences();
      this.editing.set(undefined);
      this.view.set('list');
      this.announcer.announce('Experience saved.');
    } catch (error: unknown) {
      this.formError.set(normalizeProblemDetails(this.errorBody(error)));
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected async submitEdit(value: NurseExperienceFormValue): Promise<void> {
    const record = this.editing();
    if (record === undefined) {
      return;
    }
    this.isSubmitting.set(true);
    this.formError.set(undefined);
    try {
      await firstValueFrom(
        this.api.updateExperience(record.id, {
          facilityName: value.facilityName,
          jobTitle: value.jobTitle,
          countryId: value.countryId,
          startDate: value.startDate,
          endDate: value.endDate,
          isCurrent: value.isCurrent,
          description: value.description,
        }),
      );
      await this.reloadExperiences();
      this.editing.set(undefined);
      this.view.set('list');
      this.announcer.announce('Experience saved.');
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.editing.set(undefined);
        this.view.set('list');
        this.notice.set('This position no longer exists. The list has been refreshed.');
        this.announcer.announce('This position no longer exists.');
        await this.reloadExperiences();
      } else {
        this.formError.set(normalizeProblemDetails(this.errorBody(error)));
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected requestDelete(record: NurseExperienceDto): void {
    this.deleteConfirmation.request();
    this.deleteTargetId.set(record.id);
    this.deleteError.set(undefined);
    this.announcer.announce(`Confirm deletion of ${record.jobTitle} at ${record.facilityName}?`);
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
      await firstValueFrom(this.api.deleteExperience(id));
      await this.reloadExperiences();
      this.announcer.announce('Experience deleted.');
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        await this.reloadExperiences();
        this.notice.set('That position had already been removed. The list has been refreshed.');
        this.announcer.announce('Experience was already removed.');
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

  protected isDescriptionExpanded(id: string): boolean {
    return this.expandedDescriptions().includes(id);
  }

  protected needsDescriptionToggle(description: string | null): boolean {
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
      const [countries, experiences] = await Promise.all([
        this.loadCountries(),
        firstValueFrom(this.api.listExperiences()),
      ]);
      this.countries.set(countries);
      this.experiences.set(experiences);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
    }
  }

  private async reloadExperiences(): Promise<void> {
    this.experiences.set(await firstValueFrom(this.api.listExperiences()));
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
