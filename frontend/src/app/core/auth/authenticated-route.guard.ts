import { inject } from '@angular/core';
import { Router } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, filter, map, of, take, timer } from 'rxjs';
import { AuthSessionBootstrap } from './auth-session-bootstrap';
import type { TokenSessionStatus } from './auth-session-bootstrap';
import { canonicalRoutePath } from '../routing/canonical-routes';
import type { CanonicalRouteId } from '../routing/canonical-routes';
import { isAuthenticatedRoute } from '../routing/route-classification';
import { RETURN_URL_QUERY_KEY } from '../routing/safe-return';

export function authenticatedRouteGuard(
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
): Observable<boolean | UrlTree> {
  const bootstrap = inject(AuthSessionBootstrap);
  const router = inject(Router);

  const routeId = route.data['routeId'];
  if (typeof routeId === 'string' && !isAuthenticatedRoute(routeId as CanonicalRouteId)) {
    return of(true);
  }

  const sessionStatus = bootstrap.state();
  if (sessionStatus === 'authenticated') {
    return of(true);
  }
  if (sessionStatus === 'anonymous') {
    return of(createSignInUrlTree(router, state.url));
  }

  return waitForSessionResolution(bootstrap).pipe(
    map((status) => (status === 'authenticated' ? true : createSignInUrlTree(router, state.url))),
  );
}

function waitForSessionResolution(bootstrap: AuthSessionBootstrap): Observable<TokenSessionStatus> {
  return timer(0, 50).pipe(
    map(() => bootstrap.state()),
    filter((status): status is 'authenticated' | 'anonymous' => status !== 'initializing'),
    take(1),
  );
}

function createSignInUrlTree(router: Router, requestedUrl: string): UrlTree {
  return router.createUrlTree([canonicalRoutePath('AUTH_SIGN_IN')], {
    queryParams: { [RETURN_URL_QUERY_KEY]: requestedUrl },
  });
}