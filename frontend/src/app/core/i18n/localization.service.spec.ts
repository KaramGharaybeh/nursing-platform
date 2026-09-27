import { TestBed } from '@angular/core/testing';
import { LocaleDirectionService } from '../locale/locale-direction.service';
import { LocalizationService } from './localization.service';
import { TRANSLATIONS, type TranslationKey } from './translations';

describe('LocalizationService', () => {
  afterEach(() => {
    localStorage.removeItem('np-locale');
    TestBed.resetTestingModule();
  });

  function setup() {
    TestBed.configureTestingModule({});
    return {
      locale: TestBed.inject(LocaleDirectionService),
      i18n: TestBed.inject(LocalizationService),
    };
  }

  it('resolves English copy by default', () => {
    const { i18n } = setup();

    expect(i18n.t('signin.title')).toBe('Sign in');
    expect(i18n.t('auth.emailEmpty')).toBe("'Email' must not be empty.");
  });

  it('resolves Arabic copy after switching locale without touching routes', () => {
    const { locale, i18n } = setup();

    locale.setLocale('ar');

    expect(i18n.t('signin.title')).toBe('تسجيل الدخول');
    expect(i18n.t('auth.emailEmpty')).toBe('يجب إدخال البريد الإلكتروني.');
    expect(i18n.t('switcher.arabic')).toBe('العربية');
    expect(document.documentElement.lang).toBe('ar');
    expect(document.documentElement.dir).toBe('rtl');
  });

  it('restores English copy when switching back', () => {
    const { locale, i18n } = setup();

    locale.setLocale('ar');
    locale.setLocale('en');

    expect(i18n.t('signin.title')).toBe('Sign in');
    expect(document.documentElement.lang).toBe('en');
    expect(document.documentElement.dir).toBe('ltr');
  });

  it('keeps every Arabic key aligned with the English key set', () => {
    const englishKeys = Object.keys(TRANSLATIONS['en']).sort();
    const arabicKeys = Object.keys(TRANSLATIONS['ar']).sort();

    expect(arabicKeys).toEqual(englishKeys);
    for (const key of englishKeys as TranslationKey[]) {
      expect(TRANSLATIONS['ar'][key].trim()).not.toBe('');
    }
  });

  it('passes backend error copy through in the default locale', () => {
    const { i18n } = setup();

    expect(i18n.isDefaultLocale()).toBe(true);
    expect(i18n.backendErrorCopy('Backend exploded.', 'common.genericError')).toBe('Backend exploded.');
    expect(i18n.backendErrorCopy('', 'common.genericError')).toBe('Something went wrong');
    expect(i18n.backendErrorCopy('   ', 'common.genericError')).toBe('Something went wrong');
  });

  it('replaces backend error copy with the localized fallback in Arabic', () => {
    const { locale, i18n } = setup();
    locale.setLocale('ar');

    expect(i18n.isDefaultLocale()).toBe(false);
    expect(i18n.backendErrorCopy('Backend exploded.', 'common.genericError')).toBe('حدث خطأ ما');
    expect(i18n.backendErrorCopy('', 'common.genericError')).toBe('حدث خطأ ما');
  });

  it('passes backend validation errors through untouched in the default locale', () => {
    const { i18n } = setup();
    const backend = {
      kind: 'validation', type: '', title: 'One or more errors', status: 400,
      detail: 'Bad payload', traceId: '',
      errors: { Email: ['Raw backend email message.'], UnknownField: ['Raw unknown.'] },
    } as const;

    expect(i18n.safeBackendError(backend, { Email: 'auth.emailLabel' as TranslationKey })).toBe(backend);
  });

  it('maps recognized backend fields to localized copy and drops unknown fields in Arabic', () => {
    const { locale, i18n } = setup();
    locale.setLocale('ar');
    const backend = {
      kind: 'generic', type: 'about:blank', title: 'Backend title', status: 500,
      detail: 'Backend detail with internals', traceId: 'trace-1', errors: undefined,
    } as const;

    const sanitized = i18n.safeBackendError(
      { ...backend, kind: 'validation', errors: { Email: ['Raw.'], UnknownField: ['Raw.'] } },
      { Email: 'auth.emailLabel' as TranslationKey },
    );

    expect(sanitized.title).toBe('');
    expect(sanitized.detail).toBe('');
    expect(sanitized.errors?.['Email']).toEqual(['راجع البريد الإلكتروني.']);
    expect(sanitized.errors?.['UnknownField']).toBeUndefined();
  });

  it('clears backend title and detail for generic failures in Arabic', () => {
    const { locale, i18n } = setup();
    locale.setLocale('ar');

    const sanitized = i18n.safeBackendError(
      {
        kind: 'generic', type: 'about:blank', title: 'Backend title', status: 500,
        detail: 'Backend detail', traceId: '', errors: undefined,
      },
      {},
    );

    expect(sanitized.title).toBe('');
    expect(sanitized.detail).toBe('');
  });
});
