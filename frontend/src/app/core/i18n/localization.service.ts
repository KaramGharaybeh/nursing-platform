import { Injectable, inject } from '@angular/core';
import { LocaleDirectionService } from '../locale/locale-direction.service';
import type { NormalizedProblemDetails } from '../api/problem-details';
import { TRANSLATIONS, type TranslationKey } from './translations';

/**
 * The single frontend translation mechanism. Reads the active locale from
 * `LocaleDirectionService` (no parallel locale state) and resolves keys from
 * the centralized `TRANSLATIONS` resources. English is the default and the
 * fallback for any key missing in the active language. Backend-provided text
 * is never passed through this service.
 */
@Injectable({
  providedIn: 'root',
})
export class LocalizationService {
  private readonly localeDirection = inject(LocaleDirectionService);

  t(key: TranslationKey): string {
    const locale = this.localeDirection.locale();
    return TRANSLATIONS[locale][key] ?? TRANSLATIONS['en'][key];
  }

  tp(key: TranslationKey, params: Record<string, string | number>): string {
    let text = this.t(key);
    for (const [name, value] of Object.entries(params)) {
      text = text.replace(`{${name}}`, String(value));
    }
    return text;
  }

  locale(): string {
    return this.localeDirection.locale();
  }

  isDefaultLocale(): boolean {
    return this.locale() === 'en';
  }

  /**
   * Resolves user-facing copy for a backend failure carrying free-text
   * (`title`/`detail`). Backend text is shown only in the default locale;
   * any other locale receives the localized generic fallback so untranslated
   * backend English is never rendered as UI.
   */
  backendErrorCopy(backendText: string, fallbackKey: TranslationKey): string {
    if (backendText.trim() !== '' && this.isDefaultLocale()) {
      return backendText;
    }
    return this.t(fallbackKey);
  }

  /**
   * Sanitizes a backend `NormalizedProblemDetails` for display. In the
   * default locale the value passes through unchanged. In any other locale,
   * `title`/`detail` are cleared (callers render their localized fallback)
   * and validation field errors are restricted to recognized fields, each
   * mapped to localized per-field copy; unrecognized fields are dropped so
   * raw backend identifiers never reach the UI.
   */
  safeBackendError(
    error: NormalizedProblemDetails,
    fieldLabelKeys: Readonly<Record<string, TranslationKey>>,
  ): NormalizedProblemDetails {
    if (this.isDefaultLocale()) {
      return error;
    }
    if (error.kind !== 'validation') {
      return { ...error, title: '', detail: '' };
    }
    const errors: Record<string, readonly string[]> = {};
    for (const [field, labelKey] of Object.entries(fieldLabelKeys)) {
      const backendMessages = error.errors?.[field] ?? error.errors?.[`Request.${field}`];
      if (backendMessages !== undefined && backendMessages.length > 0) {
        errors[field] = [this.tp('common.reviewField', { field: this.t(labelKey).toLowerCase() })];
      }
    }
    return { ...error, title: '', detail: '', errors };
  }
}
