import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../../shared/ui/announcement';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { NpTextInputControl } from '../../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFormValidationSummary } from '../../../../shared/ui/form-validation';
import { LocalizationService } from '../../../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';

const MAX_SKILLS = 50;
const MAX_SKILL_LENGTH = 100;

export function normalizeSkillName(value: string): string {
  return value.split(/\s+/).filter((part) => part !== '').join(' ');
}

export function normalizeSkillForComparison(value: string): string {
  return normalizeSkillName(value).toUpperCase();
}

@Component({
  selector: 'np-nurse-skills',
  imports: [FormsModule, LoadingErrorRetry, MatButtonModule, NpFormValidationSummary, NpLiveRegion, NpTextInputControl],
  templateUrl: './nurse-skills.html',
  styleUrl: './nurse-skills.scss',
})
export class NurseSkills implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly savedSkills = signal<readonly string[]>([]);
  protected readonly draftSkills = signal<readonly string[]>([]);
  protected readonly skillInput = signal('');
  protected readonly addError = signal('');
  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly notice = signal('');

  protected readonly maxSkills = MAX_SKILLS;

  protected get validationSummary() {
    return toFormValidationSummary(this.saveError(), {
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('skills.formFallback'),
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.saveError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'skills.formFallback');
  }

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected updateInput(value: string): void {
    this.skillInput.set(value);
    this.addError.set('');
  }

  protected addSkill(): void {
    const normalized = normalizeSkillName(this.skillInput());
    if (normalized === '') {
      this.addError.set(this.i18n.t('skills.enterName'));
      return;
    }
    if (normalized.length > MAX_SKILL_LENGTH) {
      this.addError.set(this.i18n.t('skills.nameMax'));
      return;
    }
    const comparison = normalizeSkillForComparison(normalized);
    if (this.draftSkills().some((skill) => normalizeSkillForComparison(skill) === comparison)) {
      this.addError.set(this.i18n.tp('skills.duplicate', { skill: normalized }));
      return;
    }
    if (this.draftSkills().length >= MAX_SKILLS) {
      this.addError.set(this.i18n.tp('skills.capShort', { max: MAX_SKILLS }));
      return;
    }
    this.draftSkills.set([...this.draftSkills(), normalized]);
    this.skillInput.set('');
    this.addError.set('');
  }

  protected removeSkill(name: string): void {
    this.draftSkills.set(this.draftSkills().filter((skill) => skill !== name));
  }

  protected cancel(): void {
    this.draftSkills.set(this.savedSkills());
    this.skillInput.set('');
    this.addError.set('');
    this.saveError.set(undefined);
  }

  protected async save(): Promise<void> {
    this.isSaving.set(true);
    this.saveError.set(undefined);
    try {
      const saved = await firstValueFrom(this.api.updateSkills({ skills: [...this.draftSkills()] }));
      const names = saved.map((skill) => skill.name);
      this.savedSkills.set(names);
      this.draftSkills.set(names);
      this.announcer.announce(this.i18n.t('skills.saved'));
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notice.set(this.i18n.t('skills.profileGone'));
        this.announcer.announce(this.i18n.t('skills.profileGoneAnnounce'));
      } else {
        this.saveError.set(
          this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), {}),
        );
      }
    } finally {
      this.isSaving.set(false);
    }
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const skills = await firstValueFrom(this.api.listSkills());
      const names = skills.map((skill) => skill.name);
      this.savedSkills.set(names);
      this.draftSkills.set(names);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
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
