import { inject } from '@angular/core';
import { Router } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { filter, map, of, take, timer } from 'rxjs';
import type { Observable } from 'rxjs';
import { CurrentUserStore } from '../auth/current-user-store';
import { canonicalRoutePath } from './canonical-routes';
import type { CanonicalRouteId } from './canonical-routes';
import { isAuthenticatedRoute } from './route-classification';
import { evaluateRoutePermission, getRoutePermissionPolicy } from './route-permission-policy';

export function routePermissionGuard(
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
): Observable<boolean | UrlTree> {
  void state;
  const store = inject(CurrentUserStore);
  const router = inject(Router);
  const dataValue = route.data['routeId'];
  if (typeof dataValue !== 'string' || dataValue.length === 0) {
    throw new Error('Missing canonical route identity for permission evaluation.');
  }
  const routeId = dataValue as CanonicalRouteId;
  if (!isAuthenticatedRoute(routeId)) {
    return of(true);
  }
  const status = store.status();
  if (status === 'ready') {
    return of(resolveForReady(store, router, routeId));
  }
  if (status === 'anonymous' || status === 'unavailable') {
    return of(true);
  }
  return timer(0, 50).pipe(
    map(() => store.status()),
    filter((current) => current !== 'idle' && current !== 'loading'),
    take(1),
    map((current) => {
      if (current === 'ready') {
        return resolveForReady(store, router, routeId);
      }
      return true as const;
    }),
  );
}

function resolveForReady(
  store: CurrentUserStore,
  router: Router,
  routeId: CanonicalRouteId,
): boolean | UrlTree {
  const policy = getRoutePermissionPolicy(routeId);
  const current = store.currentUser();
  const roles = current?.roles ?? [];
  const permissions = current?.permissions ?? [];
  const allowed = evaluateRoutePermission(policy, { roles, permissions });
  if (allowed) {
    return true;
  }
  return router.createUrlTree([canonicalRoutePath('SYSTEM_ACCESS_DENIED')]);
}
