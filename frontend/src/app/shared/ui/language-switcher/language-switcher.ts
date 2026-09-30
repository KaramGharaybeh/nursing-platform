import { Component, inject } from '@angular/core';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { LocaleDirectionService, type FoundationLocale } from '../../../core/locale/locale-direction.service';

@Component({
  selector: 'np-language-switcher',
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.scss',
})
export class NpLanguageSwitcher {
  protected readonly i18n = inject(LocalizationService);
  private readonly localeDirection = inject(LocaleDirectionService);

  protected readonly options: readonly FoundationLocale[] = ['en', 'ar'];

  protected isActive(locale: FoundationLocale): boolean {
    return this.localeDirection.locale() === locale;
  }

  protected labelFor(locale: FoundationLocale): string {
    return locale === 'ar' ? this.i18n.t('switcher.arabic') : this.i18n.t('switcher.english');
  }

  protected switchLocale(locale: FoundationLocale): void {
    this.localeDirection.setLocale(locale);
  }
}
