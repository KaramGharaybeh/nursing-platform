import { routes } from './app.routes';
import { authenticatedRouteGuard } from './core/auth/authenticated-route.guard';
import { profileCompletionGuard } from './core/auth/profile-completion.guard';
import { canonicalRoutePath } from './core/routing/canonical-routes';
import { routePermissionGuard } from './core/routing/route-permission.guard';

describe('app.routes ACCOUNT_OVERVIEW', () => {
  it('mounts /account as a lazy AUTHENTICATED_ONLY route guarded by routePermissionGuard', () => {
    const accountRoute = routes.find(
      (route) => route.path === canonicalRoutePath('ACCOUNT_OVERVIEW').slice(1),
    );

    expect(accountRoute).toBeDefined();
    expect(accountRoute?.component).toBeUndefined();
    expect(typeof accountRoute?.loadComponent).toBe('function');
    expect(accountRoute?.canActivate).toEqual([authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard]);
    expect(accountRoute?.data).toEqual({ routeId: 'ACCOUNT_OVERVIEW' });
    expect(accountRoute?.redirectTo).toBeUndefined();
  });
});
