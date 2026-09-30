import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../../shared/ui/announcement';
import { LanguagesApi } from '../../../../core/api/languages-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { LanguageListItemDto } from '../../../../core/api/generated/models/language-list-item-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { NpSelectControl, type NpSelectOption } from '../../../../shared/ui/form-controls';
import { NpFormValidationSummary, toFormValidationSummary } from '../../../../shared/ui/form-validation';
import { LocalizationService } from '../../../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';

const MAX_LANGUAGES = 20;

const PROFICIENCIES: readonly string[] = ['Beginner', 'Intermediate', 'Advanced', 'Fluent', 'Native'];

export interface LanguageRow {
  readonly languageId: string;
  readonly proficiency: string;
}

@Component({
  selector: 'np-nurse-languages',
  imports: [LoadingErrorRetry, MatButtonModule, NpFormValidationSummary, NpLiveRegion, NpSelectControl],
  templateUrl: './nurse-languages.html',
  styleUrl: './nurse-languages.scss',
})
export class NurseLanguages implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly languagesApi = inject(LanguagesApi);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly catalog = signal<readonly LanguageListItemDto[]>([]);
  protected readonly savedRows = signal<readonly LanguageRow[]>([]);
  protected readonly rows = signal<readonly LanguageRow[]>([]);
  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly notice = signal('');
  protected readonly submitted = signal(false);

  protected readonly maxLanguages = MAX_LANGUAGES;
  protected get proficiencyOptions(): readonly NpSelectOption[] {
    const labels: Record<string, string> = {
      Beginner: this.i18n.t('lang.beginner'),
      Intermediate: this.i18n.t('lang.intermediate'),
      Advanced: this.i18n.t('lang.advanced'),
      Fluent: this.i18n.t('lang.fluent'),
      Native: this.i18n.t('lang.native'),
    };
    return PROFICIENCIES.map((level) => ({ value: level, label: labels[level] ?? level }));
  }

  protected get languageOptions(): readonly NpSelectOption[] {
    return this.catalog().map((language) => ({ value: language.id, label: language.name }));
  }

  protected get validationSummary() {
    return toFormValidationSummary(this.normalizedError(), {
      summaryTitle: this.i18n.t('auth.checkFields'),
      formErrorFallback: this.i18n.t('lang.formFallback'),
    });
  }

  protected get backendFailureMessage(): string {
    const error = this.normalizedError();
    if (error === undefined || error.kind === 'validation') {
      return '';
    }
    return this.i18n.backendErrorCopy(error.detail, 'lang.formFallback');
  }

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected rowLanguageLabel(index: number): string {
    return this.i18n.tp('lang.rowLanguage', { index: index + 1 });
  }

  protected rowProficiencyLabel(index: number): string {
    const language = this.catalog().find((entry) => entry.id === this.rows()[index]?.languageId);
    return language === undefined
      ? this.i18n.tp('lang.rowProficiency', { index: index + 1 })
      : this.i18n.tp('lang.rowProficiencyFor', { name: language.name });
  }

  protected addRow(): void {
    if (this.rows().length >= MAX_LANGUAGES) {
      return;
    }
    this.rows.set([...this.rows(), { languageId: '', proficiency: '' }]);
  }

  protected removeRow(index: number): void {
    this.rows.set(this.rows().filter((_, position) => position !== index));
  }

  protected updateRowLanguage(index: number, value: string): void {
    this.rows.set(this.rows().map((row, position) => (position === index ? { ...row, languageId: value } : row)));
  }

  protected updateRowProficiency(index: number, value: string): void {
    this.rows.set(this.rows().map((row, position) => (position === index ? { ...row, proficiency: value } : row)));
  }

  protected cancel(): void {
    this.rows.set(this.savedRows());
    this.saveError.set(undefined);
    this.submitted.set(false);
  }

  protected rowError(index: number): string {
    if (!this.submitted()) {
      return '';
    }
    const row = this.rows()[index];
    if (row === undefined) {
      return '';
    }
    if (row.languageId === '') {
      return this.i18n.t('lang.selectLanguageError');
    }
    if (row.proficiency === '') {
      return this.i18n.t('lang.selectProficiencyError');
    }
    const duplicate = this.rows().some(
      (other, position) => position !== index && other.languageId !== '' && other.languageId === row.languageId,
    );
    if (duplicate) {
      return this.i18n.t('lang.duplicateError');
    }
    return '';
  }

  protected async save(): Promise<void> {
    this.submitted.set(true);
    this.saveError.set(undefined);

    if (this.rows().some((_, index) => this.rowError(index) !== '')) {
      return;
    }

    this.isSaving.set(true);
    try {
      const saved = await firstValueFrom(
        this.api.updateLanguages({
          languages: this.rows().map((row) => ({ languageId: row.languageId, proficiency: row.proficiency })),
        }),
      );
      const next = saved.map((entry) => ({ languageId: entry.languageId, proficiency: entry.proficiency }));
      this.savedRows.set(next);
      this.rows.set(next);
      this.submitted.set(false);
      this.announcer.announce(this.i18n.t('lang.saved'));
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notice.set(this.i18n.t('lang.profileGone'));
        this.announcer.announce(this.i18n.t('lang.profileGoneAnnounce'));
      } else if (this.isConflict(error)) {
        await this.refreshCatalog();
        this.saveError.set({
          kind: 'generic',
          type: '',
          title: this.i18n.t('lang.conflictTitle'),
          status: 409,
          detail: this.i18n.t('lang.conflictDetail'),
          traceId: '',
        });
      } else {
        this.saveError.set(
          this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), {}),
        );
      }
    } finally {
      this.isSaving.set(false);
    }
  }

  private normalizedError(): NormalizedProblemDetails | undefined {
    if (this.submitted() && this.rows().some((_, index) => this.rowError(index) !== '')) {
      const errors: Record<string, readonly string[]> = {};
      this.rows().forEach((_, index) => {
        const message = this.rowError(index);
        if (message !== '') {
          errors[`Row${index + 1}`] = [message];
        }
      });
      return { kind: 'validation', type: '', title: this.i18n.t('auth.validationFailed'), status: 400, detail: '', traceId: '', errors };
    }
    return this.saveError();
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const [catalog, saved] = await Promise.all([
        firstValueFrom(this.languagesApi.list()),
        firstValueFrom(this.api.listLanguages()),
      ]);
      this.catalog.set(catalog);
      const next = saved.map((entry) => ({ languageId: entry.languageId, proficiency: entry.proficiency }));
      this.savedRows.set(next);
      this.rows.set(next);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
    }
  }

  private async refreshCatalog(): Promise<void> {
    try {
      this.catalog.set(await firstValueFrom(this.languagesApi.list()));
    } catch {
      // Keep the last-known catalog; the form-level message already explains the conflict.
    }
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 404;
  }

  private isConflict(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 409;
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
