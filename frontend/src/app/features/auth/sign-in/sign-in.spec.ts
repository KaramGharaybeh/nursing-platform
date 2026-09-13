import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { Observable, of } from 'rxjs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { AuthTransport } from '../../../core/api/auth-transport';
import type { AuthResult } from '../../../core/api/generated/models/auth-result';
import type { LoginCommand } from '../../../core/api/generated/models/login-command';
import { AuthSessionBootstrap } from '../../../core/auth/auth-session-bootstrap';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { RETURN_URL_QUERY_KEY } from '../../../core/routing/safe-return';
import { routes } from '../../../app.routes';
import { SignIn } from './sign-in';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function authResultFixture(): AuthResult {
  return {
    accessToken: 'access-token-1',
    expiresAt: '2026-09-08T12:00:00Z',
    refreshToken: 'refresh-token-1',
  };
}

class AuthTransportStub {
  readonly loginCalls: LoginCommand[] = [];
  nextResult: AuthResult | undefined = authResultFixture();
  nextError: unknown;

  login(command: LoginCommand) {
    this.loginCalls.push(command);
    if (this.nextError !== undefined) {
      return new Observable<never>((subscriber) => subscriber.error(this.nextError));
    }
    return of(this.nextResult ?? authResultFixture());
  }
}

class AuthSessionBootstrapStub {
  readonly established: AuthResult[] = [];
  readonly state = () => 'anonymous' as const;

  establishAuthenticatedSession(result: AuthResult) {
    this.established.push(result);
    return 'authenticated' as const;
  }
}

class CurrentUserStoreStub {
  hydrateCalls = 0;
  nextHydration: 'ready' | 'anonymous' | 'unavailable' = 'ready';

  hydrate() {
    this.hydrateCalls += 1;
    return of(this.nextHydration);
  }
}

async function setup(returnUrl?: string) {
  const authTransport = new AuthTransportStub();
  const authSession = new AuthSessionBootstrapStub();
  const currentUser = new CurrentUserStoreStub();

  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [SignIn],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter(routes),
      { provide: AuthTransport, useValue: authTransport },
      { provide: AuthSessionBootstrap, useValue: authSession },
      { provide: CurrentUserStore, useValue: currentUser },
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: {
            queryParamMap: {
              get: (key: string) => (key === RETURN_URL_QUERY_KEY ? (returnUrl ?? null) : null),
            },
          },
        },
      },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(SignIn);
  fixture.detectChanges();

  return {
    fixture,
    component: fixture.componentInstance,
    router: TestBed.inject(Router),
    httpMock: TestBed.inject(HttpTestingController),
    authTransport,
    authSession,
    currentUser,
  };
}

function textContent(fixture: { nativeElement: HTMLElement }): string {
  return fixture.nativeElement.textContent ?? '';
}

function input(fixture: { nativeElement: HTMLElement }, selector: string): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>(selector);
  if (element === null) {
    throw new Error(`Missing input ${selector}`);
  }
  return element;
}

function submitButton(fixture: { nativeElement: HTMLElement }): HTMLButtonElement {
  const button = fixture.nativeElement.querySelector<HTMLButtonElement>('button[type="submit"]');
  if (button === null) {
    throw new Error('Missing submit button');
  }
  return button;
}

async function enterCredentials(fixture: { nativeElement: HTMLElement }, email: string, password: string) {
  const emailInput = input(fixture, '#auth-sign-in-email');
  const passwordInput = input(fixture, '#auth-sign-in-password');
  emailInput.value = email;
  emailInput.dispatchEvent(new Event('input'));
  passwordInput.value = password;
  passwordInput.dispatchEvent(new Event('input'));
}

describe('AUTH-001 Sign In', () => {
  afterEach(() => {
    try {
      TestBed.inject(HttpTestingController).verify();
    } catch {
      // Some source-only route/scope tests do not configure HTTP testing providers.
    }
    TestBed.resetTestingModule();
  });

  it('renders the approved Sign In form fields and accessible submit action', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Sign in');
    expect(textContent(fixture)).toContain('Email address');
    expect(textContent(fixture)).toContain('Password');
    expect(input(fixture, '#auth-sign-in-email').type).toBe('email');
    expect(input(fixture, '#auth-sign-in-password').type).toBe('password');
    expect(submitButton(fixture).textContent).toContain('Sign in');
    expect(submitButton(fixture).disabled).toBe(false);
  });

  it('renders backend-authorized required validation messages and guards submission', async () => {
    const { fixture, authTransport } = await setup();

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' must not be empty.");
    expect(textContent(fixture)).toContain("'Password' must not be empty.");
    expect(input(fixture, '#auth-sign-in-email').getAttribute('aria-invalid')).toBe('true');
    expect(input(fixture, '#auth-sign-in-password').getAttribute('aria-invalid')).toBe('true');
    expect(authTransport.loginCalls).toEqual([]);
  });

  it('sends the exact LoginCommand and establishes session before hydrating current user and navigating to valid returnUrl', async () => {
    const { fixture, router, authTransport, authSession, currentUser } = await setup('/nurse/profile?tab=overview#summary');
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    const result = authResultFixture();
    authTransport.nextResult = result;

    await enterCredentials(fixture, 'nurse@example.com', 'secret-password');
    submitButton(fixture).click();
    await fixture.whenStable();

    expect(authTransport.loginCalls).toEqual([{ email: 'nurse@example.com', password: 'secret-password' }]);
    expect(authSession.established).toEqual([result]);
    expect(currentUser.hydrateCalls).toBe(1);
    expect(navigateSpy).toHaveBeenCalledWith('/nurse/profile?tab=overview#summary');
  });

  it('falls back to canonical ACCOUNT_OVERVIEW when returnUrl is missing or rejected and never uses role-home destinations', async () => {
    const { fixture, router } = await setup('//example.com/phishing');
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterCredentials(fixture, 'admin@example.com', 'secret-password');
    submitButton(fixture).click();
    await fixture.whenStable();

    expect(navigateSpy).toHaveBeenCalledWith(canonicalRoutePath('ACCOUNT_OVERVIEW'));
    expect(navigateSpy).not.toHaveBeenCalledWith('/admin');
    expect(navigateSpy).not.toHaveBeenCalledWith('/nurse');
    expect(navigateSpy).not.toHaveBeenCalledWith('/employer');
    expect(navigateSpy).not.toHaveBeenCalledWith('/auth/role-selection');
  });

  it('preserves authenticated session when current-user hydration becomes unavailable', async () => {
    const { fixture, router, authSession, currentUser } = await setup();
    currentUser.nextHydration = 'unavailable';
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterCredentials(fixture, 'nurse@example.com', 'secret-password');
    submitButton(fixture).click();
    await fixture.whenStable();

    expect(authSession.established.length).toBe(1);
    expect(currentUser.hydrateCalls).toBe(1);
    expect(navigateSpy).toHaveBeenCalledWith(canonicalRoutePath('ACCOUNT_OVERVIEW'));
  });

  it('keeps failed login inside Sign In without establishing session or hydrating current user', async () => {
    const { fixture, authTransport, authSession, currentUser, router } = await setup('/account');
    authTransport.nextError = {
      status: 401,
      error: { title: 'Unauthorized', status: 401, detail: 'Invalid credentials.' },
    };
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterCredentials(fixture, 'nurse@example.com', 'wrong-password');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Invalid credentials.');
    expect(authSession.established).toEqual([]);
    expect(currentUser.hydrateCalls).toBe(0);
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('keeps AUTH_SIGN_IN public by lazily activating the real sign-in route without anonymous-only redirect behavior', async () => {
    const signInRoute = routes.find((route) => route.path === canonicalRoutePath('AUTH_SIGN_IN').slice(1));

    await expect(signInRoute?.loadComponent?.()).resolves.toBe(SignIn);
    expect(signInRoute?.component).toBeUndefined();
    expect(signInRoute?.canActivate).toBeUndefined();
    expect(signInRoute?.canMatch).toBeUndefined();
    expect(signInRoute?.redirectTo).toBeUndefined();
    expect(routes.some((route) => route.path?.includes('reset-password'))).toBe(true);
  });

  it('keeps the Sign In screen free of direct token storage, browser storage, JWT parsing, and role redirects', () => {
    const source = readTextFile('src/app/features/auth/sign-in/sign-in.ts');
    const template = readTextFile('src/app/features/auth/sign-in/sign-in.html');
    const combined = `${source}\n${template}`.toLowerCase();

    expect(source).toContain('AuthTransport');
    expect(source).toContain('AuthSessionBootstrap');
    expect(source).toContain('CurrentUserStore');
    expect(source).toContain('isSafeReturnUrl');
    expect(source).toContain('canonicalRoutePath');
    expect(combined).not.toContain('tokenstorage');
    expect(combined).not.toContain('sessionstorage');
    expect(combined).not.toContain('localstorage');
    expect(combined).not.toContain('globalthis');
    expect(combined).not.toContain('jwt');
    expect(combined).not.toContain('/admin');
    expect(combined).not.toContain('/nurse');
    expect(combined).not.toContain('/employer');
    expect(combined).not.toContain('role-selection');
  });
});
