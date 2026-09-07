import { Injectable, computed, signal } from '@angular/core';

export type FoundationLocale = 'en' | 'ar';
export type FoundationDirection = 'ltr' | 'rtl';

export const DEFAULT_LOCALE: FoundationLocale = 'en';
export const DEFAULT_DIRECTION: FoundationDirection = 'ltr';

export const LOCALE_DIRECTION: Record<FoundationLocale, FoundationDirection> = {
  en: 'ltr',
  ar: 'rtl',
};

export const LOCALE_FONT_STACK: Record<FoundationLocale, string> = {
  en: 'system-ui, sans-serif',
  ar: 'system-ui, sans-serif',
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
    this.syncDocument();
  }

  setLocale(locale: FoundationLocale): void {
    this.localeSignal.set(locale);
    this.syncDocument();
  }

  private syncDocument(): void {
    const locale = this.localeSignal();
    document.documentElement.lang = locale;
    document.documentElement.dir = LOCALE_DIRECTION[locale];
  }
}
