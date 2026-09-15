import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CurrentUserStore } from './current-user-store';
import type { CurrentUser } from './current-user';
import { profileCompletionGuard } from './profile-completion.guard';
import { canonicalRoutePath } from '../routing/canonical-routes';

const COMPLETE_USER: CurrentUser = {
  createdAt: '2026-09-14T00:00:00Z',
  email: 'user@example.com',
  emailVerified: true,
  firstName: 'Profile',
  id: 'user-1',
  isActive: true,
  isProfileComplete: true,
  lastLoginAt: undefined,
  lastName: 'Complete',
  permissions: [],
  roles: ['Nurse'],
  username: 'profileuser',
};

class CurrentUserStoreStub {
  statusValue: 'idle' | 'loading' | 'ready' | 'anonymous' | 'unavailable' = 'ready';
  userValue: CurrentUser | undefined = COMPLETE_USER;
  status() { return this.statusValue; }
  currentUser() { return this.userValue; }
}

function setup(store = new CurrentUserStoreStub()) {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [provideRouter([]), { provide: CurrentUserStore, useValue: store }],
  });
  return { store, router: TestBed.inject(Router) };
}

function call(routeId: string, url: string) {
  const route = { data: { routeId } } as unknown as ActivatedRouteSnapshot;
  const state = { url } as unknown as RouterStateSnapshot;
  return TestBed.runInInjectionContext(() => profileCompletionGuard(route, state));
}

describe('profileCompletionGuard', () => {
  it('redirects authenticated users with incomplete profile to onboarding', async () => {
    setup(Object.assign(new CurrentUserStoreStub(), {
      userValue: { ...COMPLETE_USER, isProfileComplete: false, firstName: '', lastName: '' },
    }));

    const result = await firstValueFrom(call('ACCOUNT_OVERVIEW', '/account'));

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toContain(canonicalRoutePath('ONBOARDING_PROFILE'));
    expect((result as UrlTree).toString()).toContain('returnUrl=%2Faccount');
  });

  it('allows incomplete users to remain on onboarding without a redirect loop', async () => {
    setup(Object.assign(new CurrentUserStoreStub(), {
      userValue: { ...COMPLETE_USER, isProfileComplete: false, firstName: '', lastName: '' },
    }));

    await expect(firstValueFrom(call('ONBOARDING_PROFILE', '/onboarding/profile'))).resolves.toBe(true);
  });

  it('allows completed users to proceed to requested authenticated routes', async () => {
    setup();

    await expect(firstValueFrom(call('ACCOUNT_OVERVIEW', '/account'))).resolves.toBe(true);
  });

  it('leaves anonymous behavior to the existing authenticated route guard', async () => {
    setup(Object.assign(new CurrentUserStoreStub(), { statusValue: 'anonymous', userValue: undefined }));

    await expect(firstValueFrom(call('ACCOUNT_OVERVIEW', '/account'))).resolves.toBe(true);
  });
});
