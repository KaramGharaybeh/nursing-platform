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

function passwordInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-nurse-password');
  if (element === null) {
    throw new Error('Missing nurse registration password input');
  }
  return element;
}

function firstNameInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-nurse-first-name');
  if (element === null) {
    throw new Error('Missing nurse registration first-name input');
  }
  return element;
}

function lastNameInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-nurse-last-name');
  if (element === null) {
    throw new Error('Missing nurse registration last-name input');
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
  const password = passwordInput(fixture);
  password.value = request.password;
  password.dispatchEvent(new Event('input'));
  const firstName = firstNameInput(fixture);
  firstName.value = request.firstName;
  firstName.dispatchEvent(new Event('input'));
  const lastName = lastNameInput(fixture);
  lastName.value = request.lastName;
  lastName.dispatchEvent(new Event('input'));
}

const VALID_REQUEST: PublicRegisterRequest = {
  email: 'nurse@example.com',
  password: 'NewPass1x',
  firstName: 'Amal',
  lastName: 'Haddad',
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

  it('renders the approved four-field nurse registration form with an accessible submit action', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Create a nurse account');
    expect(emailInput(fixture).type).toBe('email');
    expect(passwordInput(fixture).type).toBe('password');
    expect(firstNameInput(fixture).type).toBe('text');
    expect(lastNameInput(fixture).type).toBe('text');
    expect(submitButton(fixture).textContent).toContain('Create account');
    expect(submitButton(fixture).disabled).toBe(false);
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(4);
  });

  it('renders only backend-authorized required validation and guards submission', async () => {
    const { fixture, registerNurseApi } = await setup();

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' must not be empty.");
    expect(textContent(fixture)).toContain("'Password' must not be empty.");
    expect(textContent(fixture)).toContain("'First Name' must not be empty.");
    expect(textContent(fixture)).toContain("'Last Name' must not be empty.");
    expect(emailInput(fixture).getAttribute('aria-invalid')).toBe('true');
    expect(registerNurseApi.calls).toEqual([]);
  });

  it('renders backend-authorized email format and password complexity validation without calling the backend', async () => {
    const { fixture, registerNurseApi } = await setup();

    await enterRegistration(fixture, {
      email: 'not-an-email',
      password: 'short',
      firstName: 'Amal',
      lastName: 'Haddad',
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
      password: 'longenough1',
      firstName: 'Amal',
      lastName: 'Haddad',
    });
    submitButton(uppercaseFixture).click();
    uppercaseFixture.detectChanges();

    expect(textContent(uppercaseFixture)).toContain('Check the highlighted fields');
    expect(textContent(uppercaseFixture)).toContain('Password must contain at least one uppercase letter.');
    expect(uppercaseApi.calls).toEqual([]);

    const { fixture: digitFixture, registerNurseApi: digitApi } = await setup();

    await enterRegistration(digitFixture, {
      email: 'nurse@example.com',
      password: 'Longenoughx',
      firstName: 'Amal',
      lastName: 'Haddad',
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

  it('keeps AUTH_REGISTER_NURSE public by lazily activating only the real nurse-registration route', async () => {
    const nurseRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_REGISTER_NURSE').slice(1),
    );

    await expect(nurseRoute?.loadComponent?.()).resolves.toBe(RegisterNurse);
    expect(nurseRoute?.component).toBeUndefined();
    expect(nurseRoute?.canActivate).toBeUndefined();
    expect(nurseRoute?.canMatch).toBeUndefined();
    expect(nurseRoute?.redirectTo).toBeUndefined();
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
});
