import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  Router,
  RouterOutlet,
} from '@angular/router';
import { AuthSessionBootstrap } from './core/auth/auth-session-bootstrap';
import { CurrentUserStore } from './core/auth/current-user-store';
import { LocalizationService } from './core/i18n/localization.service';
import { LocaleDirectionService } from './core/locale/locale-direction.service';
import { canonicalRoutePath } from './core/routing/canonical-routes';
import { PUBLIC_ROUTE_IDS } from './core/routing/route-classification';
import { AuthenticatedAppShell } from './core/shell/authenticated-app-shell';
import {
  getMountedConcreteRouteIds,
  getPrimaryNavigationItems,
  getRouteIdForUrl,
} from './core/shell/primary-navigation';
import { LoadingErrorRetry } from './shared/ui/loading-error-retry';
import { routes } from './app.routes';

@Component({
  selector: 'np-root',
  imports: [RouterOutlet, LoadingErrorRetry, AuthenticatedAppShell],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly localeDirection = inject(LocaleDirectionService);
  protected readonly i18n = inject(LocalizationService);
  private readonly router = inject(Router);
  private readonly authSession = inject(AuthSessionBootstrap);
  private readonly currentUserStore = inject(CurrentUserStore);
  private readonly mountedRouteIds = getMountedConcreteRouteIds(routes);

  protected readonly navigating = signal(false);
  protected readonly currentUrl = signal(this.router.url);
  protected readonly showAuthenticatedShell = computed(() => {
    return this.authSession.state() === 'authenticated' && !isPublicUrl(this.currentUrl());
  });
  protected readonly primaryNavigationItems = computed(() => {
    const status = this.currentUserStore.status();
    return getPrimaryNavigationItems({
      mountedRouteIds: this.mountedRouteIds,
      currentRouteId: getRouteIdForUrl(this.currentUrl()),
      user: status === 'ready'
        ? {
          status: 'ready',
          roles: this.currentUserStore.currentUser()?.roles ?? [],
          permissions: this.currentUserStore.currentUser()?.permissions ?? [],
        }
        : { status },
    });
  });

  constructor() {
    const subscription = this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.navigating.set(true);
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.navigating.set(false);
      }
      if (event instanceof NavigationEnd) {
        this.currentUrl.set(event.urlAfterRedirects);
      }
    });
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
  }
}

function isPublicUrl(url: string): boolean {
  const path = url.split(/[?#]/, 1)[0] || '/';
  return PUBLIC_ROUTE_IDS.some((routeId) => matchesCanonicalPath(path, canonicalRoutePath(routeId)));
}

function matchesCanonicalPath(path: string, template: string): boolean {
  const pathParts = path.split('/').filter(Boolean);
  const templateParts = template.split('/').filter(Boolean);
  if (pathParts.length !== templateParts.length) {
    return false;
  }
  return templateParts.every((part, index) => part.startsWith(':') || part === pathParts[index]);
}
