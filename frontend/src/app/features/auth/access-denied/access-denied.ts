import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { NpLanguageSwitcher } from '../../../shared/ui/language-switcher';

@Component({
  selector: 'np-access-denied',
  imports: [RouterLink, NpLanguageSwitcher],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.scss',
})
export class AccessDenied {
  protected readonly i18n = inject(LocalizationService);
  protected readonly accountPath = canonicalRoutePath('ACCOUNT_OVERVIEW');
  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly signUpPath = canonicalRoutePath('AUTH_SIGN_UP');
}
