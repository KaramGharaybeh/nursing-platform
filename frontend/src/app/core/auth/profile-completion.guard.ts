import { inject } from '@angular/core';
import { Router } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { filter, map, of, take, timer } from 'rxjs';
import type { Observable } from 'rxjs';
import { CurrentUserStore } from './current-user-store';
import { canonicalRoutePath } from '../routing/canonical-routes';
import { RETURN_URL_QUERY_KEY, isSafeReturnUrl } from '../routing/safe-return';

export function profileCompletionGuard(
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
): Observable<boolean | UrlTree> {
  const store = inject(CurrentUserStore);
  const router = inject(Router);
  const isOnboarding = route.data['routeId'] === 'ONBOARDING_PROFILE';

  const resolve = (): boolean | UrlTree => resolveProfileState(store, router, state.url, isOnboarding);
  const status = store.status();
  if (status === 'ready' || status === 'anonymous' || status === 'unavailable') {
    return of(resolve());
  }

  return timer(0, 50).pipe(
    map(() => store.status()),
    filter((current) => current !== 'idle' && current !== 'loading'),
    take(1),
    map(() => resolve()),
  );
}

function resolveProfileState(
  store: CurrentUserStore,
  router: Router,
  requestedUrl: string,
  isOnboarding: boolean,
): boolean | UrlTree {
  if (store.status() !== 'ready') {
    return true;
  }
  const user = store.currentUser();
  if (user === undefined) {
    return true;
  }
  if (!user.isProfileComplete) {
    return isOnboarding
      ? true
      : router.createUrlTree([canonicalRoutePath('ONBOARDING_PROFILE')], {
        queryParams: { [RETURN_URL_QUERY_KEY]: requestedUrl },
      });
  }
  if (!isOnboarding) {
    return true;
  }
  const returnUrl = routeSafeReturnUrl(router);
  return router.createUrlTree([returnUrl]);
}

function routeSafeReturnUrl(router: Router): string {
  const tree = router.parseUrl(router.url);
  const value = tree.queryParams[RETURN_URL_QUERY_KEY];
  if (typeof value === 'string' && isSafeReturnUrl(value) && value !== canonicalRoutePath('ONBOARDING_PROFILE')) {
    return value;
  }
  return canonicalRoutePath('ACCOUNT_OVERVIEW');
}
