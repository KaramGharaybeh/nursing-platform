import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { SignUpApi } from '../../../core/api/sign-up-api';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { SignUp } from './sign-up';

class SignUpApiStub {
  readonly calls: PublicRegisterRequest[] = [];
  nextError: unknown;

  signUp(request: PublicRegisterRequest) {
    this.calls.push(request);
    if (this.nextError !== undefined) {
      return throwError(() => this.nextError);
    }
    return of(undefined);
  }
}

async function setup() {
  const api = new SignUpApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [SignUp],
    providers: [provideRouter([]), { provide: SignUpApi, useValue: api }],
  }).compileComponents();
  const fixture = TestBed.createComponent(SignUp);
  fixture.detectChanges();
  return { fixture, api, router: TestBed.inject(Router) };
}

function input(fixture: { nativeElement: HTMLElement }, selector: string): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>(selector);
  if (element === null) {
    throw new Error(`Missing input ${selector}`);
  }
  return element;
}

async function enter(fixture: { nativeElement: HTMLElement }, values: PublicRegisterRequest & { confirmPassword: string }) {
  for (const [selector, value] of [
    ['#auth-sign-up-email', values.email],
    ['#auth-sign-up-username', values.username],
    ['#auth-sign-up-password', values.password],
    ['#auth-sign-up-confirm-password', values.confirmPassword],
  ] as const) {
    const element = input(fixture, selector);
    element.value = value;
    element.dispatchEvent(new Event('input'));
  }
}

describe('Unified Sign Up', () => {
  it('renders exactly email, username, password, and confirm password fields with no role selector', async () => {
    const { fixture } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Sign up');
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(4);
    expect(text).toContain('Email address');
    expect(text).toContain('Username');
    expect(text).toContain('Password');
    expect(text).toContain('Confirm password');
    expect(text).not.toContain('Nurse');
    expect(text).not.toContain('Employer');
    expect(text).not.toContain('Expert');
    expect(text).not.toContain('Admin');
  });

  it('sets registration-safe autocomplete attributes', async () => {
    const { fixture } = await setup();

    expect(input(fixture, '#auth-sign-up-email').getAttribute('autocomplete')).toBe('email');
    expect(input(fixture, '#auth-sign-up-username').getAttribute('autocomplete')).toBe('username');
    expect(input(fixture, '#auth-sign-up-password').getAttribute('autocomplete')).toBe('new-password');
    expect(input(fixture, '#auth-sign-up-confirm-password').getAttribute('autocomplete')).toBe('new-password');
  });

  it('rejects confirm-password mismatch before API submission', async () => {
    const { fixture, api } = await setup();

    await enter(fixture, {
      email: 'new@example.com',
      username: 'newnurse',
      password: 'Password1',
      confirmPassword: 'Different1',
    });
    (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(api.calls).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('Confirm password must match password.');
  });

  it('submits only email, username, and password then navigates to Check Email', async () => {
    const { fixture, api, router } = await setup();
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enter(fixture, {
      email: 'new@example.com',
      username: 'newnurse',
      password: 'Password1',
      confirmPassword: 'Password1',
    });
    (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();

    expect(api.calls).toEqual([{ email: 'new@example.com', username: 'newnurse', password: 'Password1' }]);
    expect(Object.keys(api.calls[0] ?? {})).toEqual(['email', 'username', 'password']);
    expect(navigateSpy).toHaveBeenCalledWith(canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST'));
  });
});
