import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CurrentUserStore } from '../../../../core/auth/current-user-store';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseCertificateDto } from '../../../../core/api/generated/models/nurse-certificate-dto';
import type { NurseCvDocumentDto } from '../../../../core/api/generated/models/nurse-cv-document-dto';
import type { NurseEducationDto } from '../../../../core/api/generated/models/nurse-education-dto';
import type { NurseExperienceDto } from '../../../../core/api/generated/models/nurse-experience-dto';
import type { NurseLanguageDto } from '../../../../core/api/generated/models/nurse-language-dto';
import type { NurseProfileDto } from '../../../../core/api/generated/models/nurse-profile-dto';
import type { NurseSkillDto } from '../../../../core/api/generated/models/nurse-skill-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';

const MAX_CERTIFICATE_PREVIEWS = 2;

@Component({
  selector: 'np-nurse-profile-overview',
  imports: [DatePipe, LoadingErrorRetry],
  templateUrl: './nurse-profile-overview.html',
  styleUrl: './nurse-profile-overview.scss',
})
export class NurseProfileOverview implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly currentUserStore = inject(CurrentUserStore);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly profile = signal<NurseProfileDto | undefined>(undefined);
  protected readonly experiences = signal<readonly NurseExperienceDto[]>([]);
  protected readonly education = signal<readonly NurseEducationDto[]>([]);
  protected readonly certificates = signal<readonly NurseCertificateDto[]>([]);
  protected readonly skills = signal<readonly NurseSkillDto[]>([]);
  protected readonly languages = signal<readonly NurseLanguageDto[]>([]);
  protected readonly cv = signal<NurseCvDocumentDto | undefined>(undefined);

  protected readonly certificatePreviews = MAX_CERTIFICATE_PREVIEWS;

  protected get displayName(): string {
    const user = this.currentUserStore.currentUser();
    const name = `${user?.firstName ?? ''} ${user?.lastName ?? ''}`.trim();
    return name !== '' ? name : (user?.username ?? '');
  }

  protected get availabilityLabel(): string {
    return this.profile()?.isAvailableForRecruitment === true
      ? 'Available for recruitment'
      : 'Not available for recruitment';
  }

  protected get latestExperience(): NurseExperienceDto | undefined {
    return this.experiences()[0];
  }

  protected get remainingExperienceCount(): number {
    return Math.max(this.experiences().length - 1, 0);
  }

  protected get latestEducation(): NurseEducationDto | undefined {
    return this.education()[0];
  }

  protected get remainingEducationCount(): number {
    return Math.max(this.education().length - 1, 0);
  }

  protected get certificatePreviewItems(): readonly NurseCertificateDto[] {
    return this.certificates().slice(0, MAX_CERTIFICATE_PREVIEWS);
  }

  protected get remainingCertificateCount(): number {
    return Math.max(this.certificates().length - MAX_CERTIFICATE_PREVIEWS, 0);
  }

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected cvFileSizeLabel(sizeInBytes: number): string {
    if (sizeInBytes >= 1024 * 1024) {
      return `${(sizeInBytes / (1024 * 1024)).toFixed(1)} MB`;
    }
    return `${Math.max(Math.round(sizeInBytes / 1024), 1)} KB`;
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const profile = await firstValueFrom(this.api.getProfile());
      this.profile.set(profile);
      await this.loadSections();
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.profile.set(undefined);
        this.state.set({ kind: 'ready' });
        return;
      }
      this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
    }
  }

  private async loadSections(): Promise<void> {
    const [experiences, education, certificates, skills, languages, cv] = await Promise.all([
      this.loadSection(() => firstValueFrom(this.api.listExperiences())),
      this.loadSection(() => firstValueFrom(this.api.listEducation())),
      this.loadSection(() => firstValueFrom(this.api.listCertificates())),
      this.loadSection(() => firstValueFrom(this.api.listSkills())),
      this.loadSection(() => firstValueFrom(this.api.listLanguages())),
      this.loadSection(() => firstValueFrom(this.api.getCv())),
    ]);

    this.experiences.set(experiences ?? []);
    this.education.set(education ?? []);
    this.certificates.set(certificates ?? []);
    this.skills.set(skills ?? []);
    this.languages.set(languages ?? []);
    this.cv.set(cv);
  }

  private async loadSection<T>(load: () => Promise<T>): Promise<T | undefined> {
    try {
      return await load();
    } catch {
      return undefined;
    }
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 404;
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
