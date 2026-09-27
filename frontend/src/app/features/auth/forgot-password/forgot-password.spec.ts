import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of } from 'rxjs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { provideHttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import type { ForgotPasswordRequest } from '../../../core/api/generated/models/forgot-password-request';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { routes } from '../../../app.routes';
import { ForgotPassword } from './forgot-password';
import { ForgotPasswordApi } from './forgot-password-api';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class ForgotPasswordApiStub {
  readonly calls: ForgotPasswordRequest[] = [];
  nextError: unknown;

  requestPasswordReset(request: ForgotPasswordRequest) {
    this.calls.push(request);
    if (this.nextError !== undefined) {
      return new Observable<never>((subscriber) => subscriber.error(this.nextError));
    }
    return of(undefined);
  }
}

async function setup() {
  const forgotPasswordApi = new ForgotPasswordApiStub();

  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ForgotPassword],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter(routes),
      { provide: ForgotPasswordApi, useValue: forgotPasswordApi },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(ForgotPassword);
  fixture.detectChanges();

  return {
    fixture,
    component: fixture.componentInstance,
    forgotPasswordApi,
    router: TestBed.inject(Router),
    httpMock: TestBed.inject(HttpTestingController),
  };
}

function textContent(fixture: { nativeElement: HTMLElement }): string {
  return fixture.nativeElement.textContent ?? '';
}

function emailInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-forgot-password-email');
  if (element === null) {
    throw new Error('Missing forgot password email input');
  }
  return element;
}

function submitButton(fixture: { nativeElement: HTMLElement }): HTMLButtonElement {
  const button = fixture.nativeElement.querySelector<HTMLButtonElement>('button[type="submit"]');
  if (button === null) {
    throw new Error('Missing forgot password submit button');
  }
  return button;
}

async function enterEmail(fixture: { nativeElement: HTMLElement }, email: string) {
  const input = emailInput(fixture);
  input.value = email;
  input.dispatchEvent(new Event('input'));
}

describe('AUTH-007 Forgot Password', () => {
  it('renders the email field with the approved persistent label and native input', async () => {
    const { fixture } = await setup();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('label[for="auth-forgot-password-email"] + input#auth-forgot-password-email')).not.toBeNull();
    expect(root.querySelector('.np-forgot-password-form mat-form-field')).toBeNull();
  });
  it('renders a public header and single centered recovery card', async () => {
    const { fixture } = await setup();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('.np-forgot-password-public-header')?.textContent).toContain('Nursing Platform');
    expect(root.querySelector('.np-forgot-password-card h1')?.textContent).toContain('Forgot password');
    expect(root.querySelector('.np-forgot-password-context')).toBeNull();
    expect(root.querySelector('.np-forgot-password-public-header a[href="/preparation-packages"]')).not.toBeNull();
    expect(root.querySelector('.np-forgot-password-public-header a[href="/auth/sign-up"]')).not.toBeNull();
  });
  afterEach(() => {
    try {
      TestBed.inject(HttpTestingController).verify();
    } catch {
      // Source-only route/scope tests do not need HTTP testing providers.
    }
    TestBed.resetTestingModule();
  });

  it('renders the approved forgot-password email-only form and accessible submit action', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Forgot password');
    expect(textContent(fixture)).toContain('Email address');
    expect(emailInput(fixture).type).toBe('email');
    expect(submitButton(fixture).textContent).toContain('Send reset link');
    expect(submitButton(fixture).disabled).toBe(false);
  });

  it('renders only backend-authorized email required validation and guards submission', async () => {
    const { fixture, forgotPasswordApi } = await setup();

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' must not be empty.");
    expect(emailInput(fixture).getAttribute('aria-invalid')).toBe('true');
    expect(forgotPasswordApi.calls).toEqual([]);
  });

  it('sends the exact ForgotPasswordRequest and shows the generic no-enumeration success state without navigating', async () => {
    const { fixture, forgotPasswordApi, router } = await setup();
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterEmail(fixture, 'nurse@example.com');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(forgotPasswordApi.calls).toEqual([{ email: 'nurse@example.com' }]);
    expect(textContent(fixture)).toContain('If the email exists, a password reset link has been sent.');
    expect(textContent(fixture).toLowerCase()).not.toContain('account found');
    expect(textContent(fixture).toLowerCase()).not.toContain('email delivered');
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('prevents duplicate submission while the approved request is in flight', async () => {
    const { fixture, forgotPasswordApi } = await setup();

    await enterEmail(fixture, 'nurse@example.com');
    submitButton(fixture).click();
    submitButton(fixture).click();
    fixture.detectChanges();

    expect(submitButton(fixture).disabled).toBe(true);
    expect(forgotPasswordApi.calls).toEqual([{ email: 'nurse@example.com' }]);
  });

  it('keeps backend failures inside Forgot Password without token or current-user behavior', async () => {
    const { fixture, forgotPasswordApi, router } = await setup();
    forgotPasswordApi.nextError = {
      status: 400,
      error: {
        title: 'Validation failed',
        status: 400,
        errors: { Email: ["'Email' is not a valid email address."] },
      },
    };
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterEmail(fixture, 'not-an-email');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain("'Email' is not a valid email address.");
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('offers Back to sign in navigation on the canonical sign-in route', async () => {
    const { fixture } = await setup();

    const signInLink = fixture.nativeElement.querySelector(
      `.np-forgot-password-back a[href="${canonicalRoutePath('AUTH_SIGN_IN')}"]`,
    ) as HTMLAnchorElement | null;

    expect(signInLink).not.toBeNull();
    expect(signInLink?.textContent).toContain('Back to sign in');
  });

  it('exposes required autocomplete semantics on the email field', async () => {
    const { fixture } = await setup();

    expect(emailInput(fixture).getAttribute('autocomplete')).toBe('email');
  });

  it('keeps AUTH_FORGOT_PASSWORD public by lazily activating only the real forgot-password route', async () => {
    const forgotPasswordRoute = routes.find((route) => route.path === canonicalRoutePath('AUTH_FORGOT_PASSWORD').slice(1));

    await expect(forgotPasswordRoute?.loadComponent?.()).resolves.toBe(ForgotPassword);
    expect(forgotPasswordRoute?.component).toBeUndefined();
    expect(forgotPasswordRoute?.canActivate).toBeUndefined();
    expect(routes.some((route) => route.path === canonicalRoutePath('AUTH_RESET_PASSWORD').slice(1))).toBe(true);
  });

  it('keeps the Forgot Password screen free of auth session, token storage, and reset-password behavior', () => {
    const source = readTextFile('src/app/features/auth/forgot-password/forgot-password.ts');
    const template = readTextFile('src/app/features/auth/forgot-password/forgot-password.html');
    const combined = `${source}\n${template}`.toLowerCase();

    expect(source).toContain('ForgotPasswordApi');
    expect(combined).not.toContain('authsessionbootstrap');
    expect(combined).not.toContain('currentuserstore');
    expect(combined).not.toContain('tokenstorage');
    expect(combined).not.toContain('sessionstorage');
    expect(combined).not.toContain('localstorage');
    expect(combined).not.toContain('reset-password');
  });
});
