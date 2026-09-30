import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CurrentUserStore } from '../../core/auth/current-user-store';
import type { CurrentUser } from '../../core/auth/current-user';
import { ProfileApi } from '../../core/api/profile-api';
import { Account } from './account';

function currentUser(overrides: Partial<CurrentUser> = {}): CurrentUser {
  return {
    id: 'user-1',
    email: 'nurse@example.com',
    username: 'nurseuser',
    firstName: 'Nadia',
    lastName: 'Nursefield',
    isProfileComplete: true,
    isActive: true,
    emailVerified: true,
    roles: ['Nurse'],
    permissions: [],
    createdAt: '2026-09-01T10:00:00Z',
    lastLoginAt: undefined,
    ...overrides,
  };
}

class CurrentUserStoreStub {
  private user: CurrentUser | undefined = currentUser();
  hydrated = 0;

  currentUser() {
    return this.user;
  }

  setUser(user: CurrentUser | undefined) {
    this.user = user;
  }

  hydrate() {
    this.hydrated += 1;
    return of('ready' as const);
  }
}

class ProfileApiStub {
  updated: { body: unknown }[] = [];
  updateError: unknown = undefined;

  updateCurrentUserProfile(body: unknown) {
    this.updated.push({ body });
    if (this.updateError !== undefined) {
      return throwError(() => this.updateError);
    }
    return of({ username: 'nurseuser', firstName: 'Nora', lastName: 'Nursing', isProfileComplete: true });
  }
}

async function setup(options?: {
  store?: CurrentUserStoreStub;
  api?: ProfileApiStub;
}): Promise<{ fixture: ComponentFixture<Account>; store: CurrentUserStoreStub; api: ProfileApiStub }> {
  const store = options?.store ?? new CurrentUserStoreStub();
  const api = options?.api ?? new ProfileApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [Account],
    providers: [
      provideRouter([]),
      { provide: CurrentUserStore, useValue: store },
      { provide: ProfileApi, useValue: api },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(Account);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, store, api };
}

function text(fixture: ComponentFixture<Account>): string {
  return fixture.nativeElement.textContent as string;
}

async function settle(fixture: ComponentFixture<Account>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('Account', () => {
  it('renders identity facts without internal account metadata', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('Nadia Nursefield');
    expect(content).toContain('nurseuser');
    expect(content).toContain('nurse@example.com');
    expect(content).toContain('Verified');
    expect(content).not.toContain('user-1');
    expect(content).not.toContain('Profile complete');
    expect(content).not.toContain('Exams.View');
    expect(content).not.toContain('Last login');
  });

  it('renders Not verified factually when email is unverified', async () => {
    const store = new CurrentUserStoreStub();
    store.setUser(currentUser({ emailVerified: false }));
    const { fixture } = await setup({ store });

    expect(text(fixture)).toContain('Not verified');
    expect(fixture.nativeElement.querySelector('[data-testid="resend-verification"]')).toBeNull();
  });

  it('enters same-route edit state with prefilled names and no unapproved editors', async () => {
    const { fixture } = await setup();

    fixture.nativeElement.querySelector('[data-testid="edit-personal-details"]')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const content = text(fixture);
    expect(fixture.nativeElement.querySelector('np-personal-details-form')).not.toBeNull();
    expect(content).not.toContain('Username');
    expect(content).not.toContain('Password');
    expect(fixture.nativeElement.querySelector('input')?.value ?? '').not.toBe('');
  });

  it('cancels without requests and restores current values', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      startEdit(): void;
      cancelEdit(): void;
    };

    component.startEdit();
    fixture.detectChanges();
    component.cancelEdit();
    fixture.detectChanges();

    expect(api.updated.length).toBe(0);
    expect(text(fixture)).toContain('Nadia Nursefield');
  });

  it('saves the exact update body once and synchronizes the store', async () => {
    const { fixture, api, store } = await setup();
    const component = fixture.componentInstance as unknown as {
      startEdit(): void;
      submitEdit(value: { firstName: string; lastName: string }): Promise<void>;
    };

    component.startEdit();
    await component.submitEdit({ firstName: 'Nora', lastName: 'Nursing' });
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].body).toEqual({ firstName: 'Nora', lastName: 'Nursing' });
    expect(store.hydrated).toBe(1);
    expect(text(fixture)).toContain('Personal details updated.');
  });

  it('prevents duplicate submission while a save is in flight', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      startEdit(): void;
      submitEdit(value: { firstName: string; lastName: string }): Promise<void>;
    };

    component.startEdit();
    const first = component.submitEdit({ firstName: 'Nora', lastName: 'Nursing' });
    const second = component.submitEdit({ firstName: 'Nora', lastName: 'Nursing' });
    await Promise.all([first, second]);
    await settle(fixture);

    expect(api.updated.length).toBe(1);
  });

  it('keeps backend failures local without losing the loaded page', async () => {
    const api = new ProfileApiStub();
    api.updateError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup({ api });
    const component = fixture.componentInstance as unknown as {
      startEdit(): void;
      cancelEdit(): void;
      submitEdit(value: { firstName: string; lastName: string }): Promise<void>;
    };

    component.startEdit();
    await component.submitEdit({ firstName: 'Nora', lastName: 'Nursing' });
    await settle(fixture);

    expect(fixture.nativeElement.querySelector('np-personal-details-form')).not.toBeNull();
    expect(text(fixture)).toContain('could not be saved');
    component.cancelEdit();
    fixture.detectChanges();
    expect(text(fixture)).toContain('Nadia Nursefield');
  });

  it('exposes no Contact Requests navigation', async () => {
    const { fixture } = await setup();
    const hrefs = Array.from(fixture.nativeElement.querySelectorAll('a') as NodeListOf<HTMLAnchorElement>).map(
      (anchor) => anchor.getAttribute('href'),
    );

    expect(hrefs.some((href) => (href ?? '').includes('contact-requests'))).toBe(false);
  });

  it('renders the shared live region for save feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});
