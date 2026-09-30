import { routes } from '../../../app.routes';
import { authenticatedRouteGuard } from '../../../core/auth/authenticated-route.guard';
import { profileCompletionGuard } from '../../../core/auth/profile-completion.guard';
import { routePermissionGuard } from '../../../core/routing/route-permission.guard';
import { AdminEntry } from './admin-entry';

describe('AdminEntry route', () => {
  it('registers /admin with existing authenticated profile and permission guards', async () => {
    const route = routes.find((entry) => entry.path === 'admin');

    expect(route).toBeDefined();
    expect(route?.canActivate).toEqual([
      authenticatedRouteGuard,
      profileCompletionGuard,
      routePermissionGuard,
    ]);
    expect(route?.data).toEqual({ routeId: 'ADMIN_ENTRY' });
    expect(await route?.loadComponent?.()).toBe(AdminEntry);
  });
});
