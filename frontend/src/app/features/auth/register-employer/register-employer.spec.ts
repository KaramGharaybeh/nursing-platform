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
import { RegisterEmployer } from './register-employer';
import { RegisterEmployerApi } from './register-employer-api';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class RegisterEmployerApiStub {
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
  const registerEmployerApi = new RegisterEmployerApiStub();

  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [RegisterEmployer],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter(routes),
      { provide: RegisterEmployerApi, useValue: registerEmployerApi },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(RegisterEmployer);
  fixture.detectChanges();

  return {
    fixture,
    component: fixture.componentInstance,
    registerEmployerApi,
    router: TestBed.inject(Router),
    httpMock: TestBed.inject(HttpTestingController),
  };
}

function textContent(fixture: { nativeElement: HTMLElement }): string {
  return fixture.nativeElement.textContent ?? '';
}

function emailInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-employer-email');
  if (element === null) {
    throw new Error('Missing employer registration email input');
  }
  return element;
}

function passwordInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-employer-password');
  if (element === null) {
    throw new Error('Missing employer registration password input');
  }
  return element;
}

function firstNameInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-employer-first-name');
  if (element === null) {
    throw new Error('Missing employer registration first-name input');
  }
  return element;
}

function lastNameInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-register-employer-last-name');
  if (element === null) {
    throw new Error('Missing employer registration last-name input');
  }
  return element;
}

function submitButton(fixture: { nativeElement: HTMLElement }): HTMLButtonElement {
  const button = fixture.nativeElement.querySelector<HTMLButtonElement>('button[type="submit"]');
  if (button === null) {
    throw new Error('Missing employer registration submit button');
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
  email: 'employer@example.com',
  password: 'NewPass1x',
  firstName: 'Amal',
  lastName: 'Haddad',
};

describe('AUTH-004 Employer Registration', () => {
  afterEach(() => {
    try {
      TestBed.inject(HttpTestingController).verify();
    } catch {
      // Source-only route/scope tests do not need HTTP testing providers.
    }
    TestBed.resetTestingModule();
  });

  it('renders the approved four-field employer registration form with an accessible submit action', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Create an employer account');
    expect(emailInput(fixture).type).toBe('email');
    expect(passwordInput(fixture).type).toBe('password');
    expect(firstNameInput(fixture).type).toBe('text');
    expect(lastNameInput(fixture).type).toBe('text');
    expect(submitButton(fixture).textContent).toContain('Create account');
    expect(submitButton(fixture).disabled).toBe(false);
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(4);
  });

  it('renders only backend-authorized required validation and guards submission', async () => {
    const { fixture, registerEmployerApi } = await setup();

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' must not be empty.");
    expect(textContent(fixture)).toContain("'Password' must not be empty.");
    expect(textContent(fixture)).toContain("'First Name' must not be empty.");
    expect(textContent(fixture)).toContain("'Last Name' must not be empty.");
    expect(emailInput(fixture).getAttribute('aria-invalid')).toBe('true');
    expect(registerEmployerApi.calls).toEqual([]);
  });

  it('renders backend-authorized email format and password complexity validation without calling the backend', async () => {
    const { fixture, registerEmployerApi } = await setup();

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
    expect(registerEmployerApi.calls).toEqual([]);
  });

  it('renders backend-authorized password uppercase and digit requirements without calling the backend', async () => {
    const { fixture: uppercaseFixture, registerEmployerApi: uppercaseApi } = await setup();

    await enterRegistration(uppercaseFixture, {
      email: 'employer@example.com',
      password: 'longenough1',
      firstName: 'Amal',
      lastName: 'Haddad',
    });
    submitButton(uppercaseFixture).click();
    uppercaseFixture.detectChanges();

    expect(textContent(uppercaseFixture)).toContain('Check the highlighted fields');
    expect(textContent(uppercaseFixture)).toContain('Password must contain at least one uppercase letter.');
    expect(uppercaseApi.calls).toEqual([]);

    const { fixture: digitFixture, registerEmployerApi: digitApi } = await setup();

    await enterRegistration(digitFixture, {
      email: 'employer@example.com',
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
    const { fixture, registerEmployerApi, router } = await setup();
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterRegistration(fixture, VALID_REQUEST);
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(registerEmployerApi.calls).toEqual([VALID_REQUEST]);
    expect(navigateSpy).toHaveBeenCalledWith(canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST'));
  });

  it('prevents duplicate submission while the approved request is in flight', async () => {
    const { fixture, registerEmployerApi } = await setup();

    await enterRegistration(fixture, VALID_REQUEST);
    submitButton(fixture).click();
    submitButton(fixture).click();
    fixture.detectChanges();

    expect(submitButton(fixture).disabled).toBe(true);
    expect(registerEmployerApi.calls).toEqual([VALID_REQUEST]);
  });

  it('maps backend validation failures to the form validation summary without navigating', async () => {
    const { fixture, registerEmployerApi, router } = await setup();
    registerEmployerApi.nextError = {
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

    expect(registerEmployerApi.calls).toEqual([VALID_REQUEST]);
    expect(textContent(fixture)).toContain("'Email' is not a valid email address.");
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('keeps registration failures inside Employer Registration without session or current-user behavior', async () => {
    const { fixture, registerEmployerApi, router } = await setup();
    registerEmployerApi.nextError = {
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

  it('keeps AUTH_REGISTER_EMPLOYER public by lazily activating only the real employer-registration route', async () => {
    const employerRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_REGISTER_EMPLOYER').slice(1),
    );

    await expect(employerRoute?.loadComponent?.()).resolves.toBe(RegisterEmployer);
    expect(employerRoute?.component).toBeUndefined();
    expect(employerRoute?.canActivate).toBeUndefined();
    expect(employerRoute?.canMatch).toBeUndefined();
    expect(employerRoute?.redirectTo).toBeUndefined();
    expect(employerRoute?.data).toBeUndefined();
  });

  it('keeps the Employer Registration screen free of auth session, token storage, and current-user behavior', () => {
    const source = readTextFile('src/app/features/auth/register-employer/register-employer.ts');
    const template = readTextFile('src/app/features/auth/register-employer/register-employer.html');
    const combined = `${source}\n${template}`.toLowerCase();

    expect(source).toContain('RegisterEmployerApi');
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
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(4);
  });

  it('exposes required autocomplete semantics on email, new-password, given-name, and family-name fields', async () => {
    const { fixture } = await setup();

    expect(emailInput(fixture).getAttribute('autocomplete')).toBe('email');
    expect(passwordInput(fixture).getAttribute('autocomplete')).toBe('new-password');
    expect(firstNameInput(fixture).getAttribute('autocomplete')).toBe('given-name');
    expect(lastNameInput(fixture).getAttribute('autocomplete')).toBe('family-name');
  });
});
