import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProfileApi } from '../../../core/api/profile-api';
import type { UpdateCurrentUserProfileRequest } from '../../../core/api/generated/models/update-current-user-profile-request';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { RETURN_URL_QUERY_KEY } from '../../../core/routing/safe-return';
import { ProfileOnboarding } from './profile-onboarding';

class ProfileApiStub {
  readonly calls: UpdateCurrentUserProfileRequest[] = [];
  nextError: unknown;

  updateCurrentUserProfile(request: UpdateCurrentUserProfileRequest) {
    this.calls.push(request);
    if (this.nextError !== undefined) {
      return throwError(() => this.nextError);
    }
    return of({ firstName: request.firstName, isProfileComplete: true, lastName: request.lastName });
  }
}

class CurrentUserStoreStub {
  hydrateCalls = 0;
  currentUser() {
    return { firstName: '', lastName: '' };
  }
  hydrate() {
    this.hydrateCalls += 1;
    return of('ready');
  }
}

async function setup(returnUrl = '/account') {
  const api = new ProfileApiStub();
  const store = new CurrentUserStoreStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ProfileOnboarding],
    providers: [
      provideRouter([]),
      { provide: ProfileApi, useValue: api },
      { provide: CurrentUserStore, useValue: store },
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: {
            queryParamMap: {
              get: (key: string) => (key === RETURN_URL_QUERY_KEY ? returnUrl : null),
            },
          },
        },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ProfileOnboarding);
  fixture.detectChanges();
  return { fixture, api, store, router: TestBed.inject(Router) };
}

function input(fixture: { nativeElement: HTMLElement }, selector: string): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>(selector);
  if (element === null) {
    throw new Error(`Missing input ${selector}`);
  }
  return element;
}

describe('ProfileOnboarding', () => {
  it('renders first and last name fields without a role selector', async () => {
    const { fixture } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Complete your profile');
    expect(text).toContain('First name');
    expect(text).toContain('Last name');
    expect(text).not.toContain('Nurse');
    expect(text).not.toContain('Employer');
  });

  it('validates required profile fields before saving', async () => {
    const { fixture, api } = await setup();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(api.calls).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain("'First name' must not be empty.");
    expect(fixture.nativeElement.textContent).toContain("'Last name' must not be empty.");
  });

  it('saves through /me profile API, rehydrates /me, and continues to safe destination', async () => {
    const { fixture, api, store, router } = await setup('/account');
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    input(fixture, '#onboarding-profile-first-name').value = 'Nora';
    input(fixture, '#onboarding-profile-first-name').dispatchEvent(new Event('input'));
    input(fixture, '#onboarding-profile-last-name').value = 'Nurse';
    input(fixture, '#onboarding-profile-last-name').dispatchEvent(new Event('input'));

    (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();

    expect(api.calls).toEqual([{ firstName: 'Nora', lastName: 'Nurse' }]);
    expect(store.hydrateCalls).toBe(1);
    expect(navigateSpy).toHaveBeenCalledWith(canonicalRoutePath('ACCOUNT_OVERVIEW'));
  });

  it('shows backend save failures without marking profile complete locally', async () => {
    const { fixture, api, store } = await setup();
    api.nextError = { status: 500, error: { title: 'Error', status: 500, detail: 'Profile failed.' } };
    input(fixture, '#onboarding-profile-first-name').value = 'Nora';
    input(fixture, '#onboarding-profile-first-name').dispatchEvent(new Event('input'));
    input(fixture, '#onboarding-profile-last-name').value = 'Nurse';
    input(fixture, '#onboarding-profile-last-name').dispatchEvent(new Event('input'));

    (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(store.hydrateCalls).toBe(0);
    expect(fixture.nativeElement.textContent).toContain('Profile failed.');
  });
});
