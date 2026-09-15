import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of } from 'rxjs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { Router } from '@angular/router';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { routes } from '../../../app.routes';
import { RegisterNurse } from './register-nurse';
import { RegisterNurseApi } from './register-nurse-api';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class RegisterNurseApiStub {
  readonly calls: PublicRegisterRequest[] = [];
  nextError: unknown;

  register(request: PublicRegisterRequest) {
    this.calls.push(request);
    if (this.nextError !== undefined) {
      return new Observable<never>((subscriber) => subscriber.error(this.nextError));
    }
    return of(undefined);
  }
}

async function setup() {
  const registerNurseApi = new RegisterNurseApiStub();

  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [RegisterNurse],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter(routes),
      { provide: RegisterNurseApi, useValue: registerNurseApi },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(RegisterNurse);
  fixture.detectChanges();

  return {
    fixture,
    component: fixture.componentInstance,
    registerNurseApi,
    router: TestBed.inject(Router),
    httpMock: TestBed.inject(HttpTestingController),
  };
}

function textContent(fixture: { nativeElement: HTMLElement }): string {
  return fixture.nativeElement.textContent ?? '';
}

function emailInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-nurse-email');
  if (element === null) {
    throw new Error('Missing nurse registration email input');
  }
  return element;
}

function usernameInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-nurse-username');
  if (element === null) {
    throw new Error('Missing nurse registration username input');
  }
  return element;
}

function passwordInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-nurse-password');
  if (element === null) {
    throw new Error('Missing nurse registration password input');
  }
  return element;
}

function submitButton(fixture: { nativeElement: HTMLElement }): HTMLButtonElement {
  const button = fixture.nativeElement.querySelector<HTMLButtonElement>('button[type="submit"]');
  if (button === null) {
    throw new Error('Missing nurse registration submit button');
  }
  return button;
}

async function enterRegistration(
  fixture: { nativeElement: HTMLElement },
  request: PublicRegisterRequest,
) {
  const email = emailInput(fixture);
  email.value = request.email;
  email.dispatchEvent(new Event('input'));
  const username = usernameInput(fixture);
  username.value = request.username;
  username.dispatchEvent(new Event('input'));
  const password = passwordInput(fixture);
  password.value = request.password;
  password.dispatchEvent(new Event('input'));
}

const VALID_REQUEST: PublicRegisterRequest = {
  email: 'nurse@example.com',
  username: 'nurse01',
  password: 'NewPass1x',
};

describe('AUTH-003 Nurse Registration', () => {
  afterEach(() => {
    try {
      TestBed.inject(HttpTestingController).verify();
    } catch {
      // Source-only route/scope tests do not need HTTP testing providers.
    }
    TestBed.resetTestingModule();
  });

  it('renders the nurse registration form with email, username, password, and an accessible submit action', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Create a nurse account');
    expect(emailInput(fixture).type).toBe('email');
    expect(usernameInput(fixture).type).toBe('text');
    expect(passwordInput(fixture).type).toBe('password');
    expect(submitButton(fixture).textContent).toContain('Create account');
    expect(submitButton(fixture).disabled).toBe(false);
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(3);
  });

  it('renders only backend-authorized required validation and guards submission', async () => {
    const { fixture, registerNurseApi } = await setup();

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' must not be empty.");
    expect(textContent(fixture)).toContain("'Username' must not be empty.");
    expect(textContent(fixture)).toContain("'Password' must not be empty.");
    expect(emailInput(fixture).getAttribute('aria-invalid')).toBe('true');
    expect(registerNurseApi.calls).toEqual([]);
  });

  it('renders backend-authorized email format and password complexity validation without calling the backend', async () => {
    const { fixture, registerNurseApi } = await setup();

    await enterRegistration(fixture, {
      email: 'not-an-email',
      username: 'validuser',
      password: 'short',
    });
    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' is not a valid email address.");
    expect(textContent(fixture)).toContain("'Password' must be at least 8 characters.");
    expect(registerNurseApi.calls).toEqual([]);
  });

  it('renders backend-authorized password uppercase and digit requirements without calling the backend', async () => {
    const { fixture: uppercaseFixture, registerNurseApi: uppercaseApi } = await setup();

    await enterRegistration(uppercaseFixture, {
      email: 'nurse@example.com',
      username: 'nurse01',
      password: 'longenough1',
    });
    submitButton(uppercaseFixture).click();
    uppercaseFixture.detectChanges();

    expect(textContent(uppercaseFixture)).toContain('Check the highlighted fields');
    expect(textContent(uppercaseFixture)).toContain('Password must contain at least one uppercase letter.');
    expect(uppercaseApi.calls).toEqual([]);

    const { fixture: digitFixture, registerNurseApi: digitApi } = await setup();

    await enterRegistration(digitFixture, {
      email: 'nurse@example.com',
      username: 'nurse02',
      password: 'Longenoughx',
    });
    submitButton(digitFixture).click();
    digitFixture.detectChanges();

    expect(textContent(digitFixture)).toContain('Check the highlighted fields');
    expect(textContent(digitFixture)).toContain('Password must contain at least one digit.');
    expect(digitApi.calls).toEqual([]);
  });

  it('submits the exact PublicRegisterRequest and navigates to the canonical verify-email route on accepted 202', async () => {
    const { fixture, registerNurseApi, router } = await setup();
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterRegistration(fixture, VALID_REQUEST);
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(registerNurseApi.calls).toEqual([VALID_REQUEST]);
    expect(navigateSpy).toHaveBeenCalledWith(canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST'));
  });

  it('prevents duplicate submission while the approved request is in flight', async () => {
    const { fixture, registerNurseApi } = await setup();

    await enterRegistration(fixture, VALID_REQUEST);
    submitButton(fixture).click();
    submitButton(fixture).click();
    fixture.detectChanges();

    expect(submitButton(fixture).disabled).toBe(true);
    expect(registerNurseApi.calls).toEqual([VALID_REQUEST]);
  });

  it('maps backend validation failures to the form validation summary without navigating', async () => {
    const { fixture, registerNurseApi, router } = await setup();
    registerNurseApi.nextError = {
      status: 400,
      error: {
        title: 'Validation failed',
        status: 400,
        errors: { Email: ["'Email' is not a valid email address."] },
      },
    };
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterRegistration(fixture, VALID_REQUEST);
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(registerNurseApi.calls).toEqual([VALID_REQUEST]);
    expect(textContent(fixture)).toContain("'Email' is not a valid email address.");
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('keeps registration failures inside Nurse Registration without session or current-user behavior', async () => {
    const { fixture, registerNurseApi, router } = await setup();
    registerNurseApi.nextError = {
      status: 500,
      error: {
        title: 'Unexpected error',
        status: 500,
        detail: 'The registration could not be completed.',
      },
    };
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterRegistration(fixture, VALID_REQUEST);
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('The registration could not be completed.');
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('redirects AUTH_REGISTER_NURSE to unified Sign Up instead of loading actor-specific registration', () => {
    const nurseRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_REGISTER_NURSE').slice(1),
    );

    expect(nurseRoute?.redirectTo).toBe('auth/sign-up');
    expect(nurseRoute?.pathMatch).toBe('full');
    expect(nurseRoute?.loadComponent).toBeUndefined();
    expect(nurseRoute?.component).toBeUndefined();
    expect(nurseRoute?.canActivate).toBeUndefined();
    expect(nurseRoute?.canMatch).toBeUndefined();
    expect(nurseRoute?.data).toBeUndefined();
  });

  it('keeps the Nurse Registration screen free of auth session, token storage, and current-user behavior', () => {
    const source = readTextFile('src/app/features/auth/register-nurse/register-nurse.ts');
    const template = readTextFile('src/app/features/auth/register-nurse/register-nurse.html');
    const combined = `${source}\n${template}`.toLowerCase();

    expect(source).toContain('RegisterNurseApi');
    expect(combined).not.toContain('authsessionbootstrap');
    expect(combined).not.toContain('currentuserstore');
    expect(combined).not.toContain('tokenstorage');
    expect(combined).not.toContain('sessionstorage');
    expect(combined).not.toContain('localstorage');
    expect(combined).not.toContain('authtransport');
    expect(combined).not.toContain('locallogout');
  });

  it('renders no company or organization fields', async () => {
    const { fixture } = await setup();
    const combined = `${fixture.nativeElement.innerHTML}`.toLowerCase();

    expect(combined).not.toContain('company');
    expect(combined).not.toContain('organization');
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(3);
  });

  it('exposes required autocomplete semantics on email, username, and new-password fields', async () => {
    const { fixture } = await setup();

    expect(emailInput(fixture).getAttribute('autocomplete')).toBe('email');
    expect(usernameInput(fixture).getAttribute('autocomplete')).toBe('username');
    expect(passwordInput(fixture).getAttribute('autocomplete')).toBe('new-password');
  });
});
