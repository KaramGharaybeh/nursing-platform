import { Injectable, computed, signal } from '@angular/core';

export type FoundationLocale = 'en' | 'ar';
export type FoundationDirection = 'ltr' | 'rtl';

export const DEFAULT_LOCALE: FoundationLocale = 'en';
export const DEFAULT_DIRECTION: FoundationDirection = 'ltr';

export const LOCALE_DIRECTION: Record<FoundationLocale, FoundationDirection> = {
  en: 'ltr',
  ar: 'rtl',
};

export const LOCALE_STORAGE_KEY = 'np-locale';

export const LOCALE_FONT_STACK: Record<FoundationLocale, string> = {
  en: 'system-ui, sans-serif',
  ar: "'Noto Sans Arabic', 'Noto Sans', system-ui, sans-serif",
};

@Injectable({
  providedIn: 'root',
})
export class LocaleDirectionService {
  private readonly localeSignal = signal<FoundationLocale>(DEFAULT_LOCALE);

  readonly locale = this.localeSignal.asReadonly();
  readonly direction = computed<FoundationDirection>(() => LOCALE_DIRECTION[this.localeSignal()]);
  readonly fontStack = computed<string>(() => LOCALE_FONT_STACK[this.localeSignal()]);

  constructor() {
    this.localeSignal.set(this.readStoredLocale());
    this.syncDocument();
  }

  setLocale(locale: FoundationLocale): void {
    this.localeSignal.set(locale);
    this.storeLocale(locale);
    this.syncDocument();
  }

  private readStoredLocale(): FoundationLocale {
    try {
      return localStorage.getItem(LOCALE_STORAGE_KEY) === 'ar' ? 'ar' : DEFAULT_LOCALE;
    } catch {
      return DEFAULT_LOCALE;
    }
  }

  private storeLocale(locale: FoundationLocale): void {
    try {
      localStorage.setItem(LOCALE_STORAGE_KEY, locale);
    } catch {
      // Storage unavailable (e.g. private mode): locale applies to this session only.
    }
  }

  private syncDocument(): void {
    const locale = this.localeSignal();
    document.documentElement.lang = locale;
    document.documentElement.dir = LOCALE_DIRECTION[locale];
  }
}
