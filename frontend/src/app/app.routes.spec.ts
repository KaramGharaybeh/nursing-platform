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

describe('app.routes legacy registration redirects', () => {
  it('redirects /auth/role-selection to /auth/sign-up', () => {
    const route = routes.find(
      (entry) => entry.path === canonicalRoutePath('AUTH_ROLE_SELECTION').slice(1),
    );

    expect(route).toBeDefined();
    expect(route?.redirectTo).toBe('auth/sign-up');
    expect(route?.pathMatch).toBe('full');
    expect(route?.loadComponent).toBeUndefined();
  });

  it('redirects /auth/register/nurse to /auth/sign-up', () => {
    const route = routes.find(
      (entry) => entry.path === canonicalRoutePath('AUTH_REGISTER_NURSE').slice(1),
    );

    expect(route).toBeDefined();
    expect(route?.redirectTo).toBe('auth/sign-up');
    expect(route?.pathMatch).toBe('full');
    expect(route?.loadComponent).toBeUndefined();
  });

  it('redirects /auth/register/employer to /auth/sign-up', () => {
    const route = routes.find(
      (entry) => entry.path === canonicalRoutePath('AUTH_REGISTER_EMPLOYER').slice(1),
    );

    expect(route).toBeDefined();
    expect(route?.redirectTo).toBe('auth/sign-up');
    expect(route?.pathMatch).toBe('full');
    expect(route?.loadComponent).toBeUndefined();
  });
});
