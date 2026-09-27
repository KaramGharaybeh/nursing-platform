import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { NpLanguageSwitcher } from '../../../shared/ui/language-switcher';

@Component({
  selector: 'np-session-expired',
  imports: [RouterLink, NpLanguageSwitcher],
  templateUrl: './session-expired.html',
  styleUrl: './session-expired.scss',
})
export class SessionExpired {
  protected readonly i18n = inject(LocalizationService);
  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly signUpPath = canonicalRoutePath('AUTH_SIGN_UP');
}
