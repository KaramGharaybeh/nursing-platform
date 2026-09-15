import { routes } from '../../../app.routes';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';

describe('AUTH-002 legacy Role Selection route', () => {
  it('redirects /auth/role-selection to unified Sign Up instead of loading actor selection', () => {
    const roleSelectionRoute = routes.find((route) => route.path === canonicalRoutePath('AUTH_ROLE_SELECTION').slice(1));

    expect(roleSelectionRoute?.redirectTo).toBe('auth/sign-up');
    expect(roleSelectionRoute?.pathMatch).toBe('full');
    expect(roleSelectionRoute?.loadComponent).toBeUndefined();
    expect(roleSelectionRoute?.component).toBeUndefined();
    expect(roleSelectionRoute?.canActivate).toBeUndefined();
    expect(roleSelectionRoute?.canMatch).toBeUndefined();
    expect(roleSelectionRoute?.data).toBeUndefined();
  });
});
