import { Component, DestroyRef, ElementRef, ViewChild, inject, input, signal } from '@angular/core';
import { CdkTrapFocus } from '@angular/cdk/a11y';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { LocalLogout } from '../auth/local-logout';
import { LocalizationService } from '../i18n/localization.service';
import type { TranslationKey } from '../i18n/translations';
import { canonicalRoutePath } from '../routing/canonical-routes';
import type { CanonicalRouteId } from '../routing/canonical-routes';
import type { PrimaryNavigationItem } from './primary-navigation';
import { NpLanguageSwitcher } from '../../shared/ui/language-switcher';

const NAVIGATION_LABEL_KEYS: Record<string, TranslationKey> = {
  EXAMS_CATALOG: 'shell.navExams',
  PREPARATION_PACKAGES_ENTITLEMENTS: 'shell.navPreparationPackages',
  COMMERCE_PRODUCTS: 'shell.navProducts',
  NURSE_PROFILE_OVERVIEW: 'shell.navProfile',
  EMPLOYER_HOME: 'shell.navEmployer',
  EMPLOYER_CANDIDATES: 'shell.navCandidates',
  EMPLOYER_REQUESTS: 'shell.navRequests',
  ADMIN_ENTRY: 'shell.navAdmin',
  ADMIN_USERS: 'shell.navUsers',
};

@Component({
  selector: 'np-authenticated-app-shell',
  imports: [RouterLink, CdkTrapFocus, NpLanguageSwitcher],
  templateUrl: './authenticated-app-shell.html',
  styleUrl: './authenticated-app-shell.scss',
  host: {
    '(keydown.escape)': 'closeMobileNavigation()',
  },
})
export class AuthenticatedAppShell {
  private readonly logoutService = inject(LocalLogout);
  private readonly router = inject(Router);
  protected readonly i18n = inject(LocalizationService);

  readonly navigationItems = input<readonly PrimaryNavigationItem[]>([]);
  protected readonly mobileNavigationOpen = signal(false);
  protected readonly accountPath = canonicalRoutePath('ACCOUNT_OVERVIEW');

  @ViewChild('menuTrigger') private readonly menuTrigger?: ElementRef<HTMLButtonElement>;
  @ViewChild('mobileClose') private readonly mobileClose?: ElementRef<HTMLButtonElement>;

  constructor() {
    const subscription = this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.closeMobileNavigation(false));
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
  }

  protected openMobileNavigation(): void {
    this.mobileNavigationOpen.set(true);
    queueMicrotask(() => this.mobileClose?.nativeElement.focus());
  }

  protected closeMobileNavigation(restoreFocus = true): void {
    if (!this.mobileNavigationOpen()) {
      return;
    }
    this.mobileNavigationOpen.set(false);
    if (restoreFocus) {
      queueMicrotask(() => this.menuTrigger?.nativeElement.focus());
    }
  }

  protected signOut(): void {
    this.logoutService.logout();
    void this.router.navigateByUrl(canonicalRoutePath('AUTH_SIGN_IN'));
  }

  protected navigationLabel(routeId: CanonicalRouteId, fallback: string): string {
    const key = NAVIGATION_LABEL_KEYS[routeId];
    return key === undefined ? fallback : this.i18n.t(key);
  }
}
