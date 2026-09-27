import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { Observable, of } from 'rxjs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import type { ResetPasswordRequest } from '../../../core/api/generated/models/reset-password-request';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { routes } from '../../../app.routes';
import { ResetPassword } from './reset-password';
import { ResetPasswordApi } from './reset-password-api';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class ResetPasswordApiStub {
  readonly calls: ResetPasswordRequest[] = [];
  nextError: unknown;

  resetPassword(request: ResetPasswordRequest) {
    this.calls.push(request);
    if (this.nextError !== undefined) {
      return new Observable<never>((subscriber) => subscriber.error(this.nextError));
    }
    return of(undefined);
  }
}

function activatedRouteWithToken(token: string | null) {
  return {
    snapshot: {
      queryParamMap: {
        get: (key: string) => (key === 'token' ? token : null),
      },
    },
  };
}

async function setup(token: string | null) {
  const resetPasswordApi = new ResetPasswordApiStub();

  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ResetPassword],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter(routes),
      { provide: ResetPasswordApi, useValue: resetPasswordApi },
      { provide: ActivatedRoute, useValue: activatedRouteWithToken(token) },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(ResetPassword);
  fixture.detectChanges();

  return {
    fixture,
    component: fixture.componentInstance,
    resetPasswordApi,
    router: TestBed.inject(Router),
    httpMock: TestBed.inject(HttpTestingController),
  };
}

function textContent(fixture: { nativeElement: HTMLElement }): string {
  return fixture.nativeElement.textContent ?? '';
}

function emailInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-reset-password-email');
  if (element === null) {
    throw new Error('Missing reset password email input');
  }
  return element;
}

function passwordInput(fixture: { nativeElement: HTMLElement }): HTMLInputElement {
  const element = fixture.nativeElement.querySelector<HTMLInputElement>('#auth-reset-password-new-password');
  if (element === null) {
    throw new Error('Missing reset password new-password input');
  }
  return element;
}

function submitButton(fixture: { nativeElement: HTMLElement }): HTMLButtonElement {
  const button = fixture.nativeElement.querySelector<HTMLButtonElement>('button[type="submit"]');
  if (button === null) {
    throw new Error('Missing reset password submit button');
  }
  return button;
}

async function enterCredentials(fixture: { nativeElement: HTMLElement }, email: string, password: string) {
  const emailElement = emailInput(fixture);
  emailElement.value = email;
  emailElement.dispatchEvent(new Event('input'));
  const passwordElement = passwordInput(fixture);
  passwordElement.value = password;
  passwordElement.dispatchEvent(new Event('input'));
}

describe('AUTH-008 Reset Password', () => {
  it('uses the public auth header and single centered reset card without marketing decoration', async () => {
    const { fixture } = await setup(null);
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('.np-reset-password-public-header')?.textContent).toContain('Nursing Platform');
    expect(root.querySelector('.np-reset-password-card h1')?.textContent).toContain('Reset password');
    expect(root.querySelector('.np-reset-password-context')).toBeNull();
    expect(root.querySelector('.np-reset-password-public-header a[href="/preparation-packages"]')).not.toBeNull();
    expect(root.querySelector('.np-reset-password-public-header a[href="/auth/sign-up"]')).not.toBeNull();
  });
  afterEach(() => {
    try {
      TestBed.inject(HttpTestingController).verify();
    } catch {
      // Source-only route/scope tests do not need HTTP testing providers.
    }
    TestBed.resetTestingModule();
  });

  it('renders the approved email and new-password only form with accessible submit action', async () => {
    const { fixture } = await setup('opaque-token');

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Reset password');
    expect(emailInput(fixture).type).toBe('email');
    expect(passwordInput(fixture).type).toBe('password');
    expect(submitButton(fixture).textContent).toContain('Reset password');
    expect(submitButton(fixture).disabled).toBe(false);
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(2);
  });

  it('blocks submission when the token query parameter is missing and makes no backend call', async () => {
    const { fixture, resetPasswordApi } = await setup(null);

    expect(textContent(fixture)).toContain('reset link is invalid or missing');
    expect(submitButton(fixture).disabled).toBe(true);

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(resetPasswordApi.calls).toEqual([]);
  });

  it('blocks submission when the token query parameter is empty or whitespace and makes no backend call', async () => {
    const { fixture, resetPasswordApi } = await setup('   ');

    expect(textContent(fixture)).toContain('reset link is invalid or missing');
    expect(submitButton(fixture).disabled).toBe(true);
    expect(resetPasswordApi.calls).toEqual([]);
  });

  it('renders only backend-authorized required validation and guards submission', async () => {
    const { fixture, resetPasswordApi } = await setup('opaque-token');

    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(textContent(fixture)).toContain("'Email' must not be empty.");
    expect(textContent(fixture)).toContain("'New Password' must not be empty.");
    expect(emailInput(fixture).getAttribute('aria-invalid')).toBe('true');
    expect(passwordInput(fixture).getAttribute('aria-invalid')).toBe('true');
    expect(resetPasswordApi.calls).toEqual([]);
  });

  it('renders backend-authorized new-password complexity validation without calling the backend', async () => {
    const { fixture, resetPasswordApi } = await setup('opaque-token');

    await enterCredentials(fixture, 'nurse@example.com', 'short');
    submitButton(fixture).click();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Check the highlighted fields');
    expect(resetPasswordApi.calls).toEqual([]);
  });

  it('sends the exact ResetPasswordRequest with the opaque query token without navigating to a success route', async () => {
    const { fixture, resetPasswordApi, router } = await setup('opaque-token');
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterCredentials(fixture, 'nurse@example.com', 'NewPass1x');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(resetPasswordApi.calls).toEqual([
      { email: 'nurse@example.com', token: 'opaque-token', newPassword: 'NewPass1x' },
    ]);
    expect(navigateSpy).not.toHaveBeenCalled();
    expect(routes.some((route) => route.path === 'auth/reset-password/success')).toBe(false);
  });

  it('renders the non-routable AUTH-009 reset-success state after the reset succeeds', async () => {
    const { fixture, resetPasswordApi, router } = await setup('opaque-token');
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterCredentials(fixture, 'nurse@example.com', 'NewPass1x');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(resetPasswordApi.calls).toEqual([
      { email: 'nurse@example.com', token: 'opaque-token', newPassword: 'NewPass1x' },
    ]);
    expect(textContent(fixture)).toContain('Password has been reset successfully.');
    expect(fixture.nativeElement.querySelector('[role="status"]')?.textContent).toContain(
      'Password has been reset successfully.',
    );
    expect(navigateSpy).not.toHaveBeenCalled();
    expect(routes.some((route) => route.path === 'auth/reset-password/success')).toBe(false);
  });

  it('renders Continue to sign in on the reset-success state', async () => {
    const { fixture } = await setup('opaque-token');

    await enterCredentials(fixture, 'nurse@example.com', 'NewPass1x');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    const signInLink = fixture.nativeElement.querySelector(
      `.np-reset-password-sign-in a[href="${canonicalRoutePath('AUTH_SIGN_IN')}"]`,
    ) as HTMLAnchorElement | null;

    expect(signInLink).not.toBeNull();
    expect(signInLink?.textContent).toContain('Continue to sign in');
  });

  it('exposes required autocomplete semantics on reset credentials', async () => {
    const { fixture } = await setup('opaque-token');

    expect(emailInput(fixture).getAttribute('autocomplete')).toBe('email');
    expect(passwordInput(fixture).getAttribute('autocomplete')).toBe('new-password');
  });

  it('announces backend reset failures exactly once', async () => {
    const { fixture, resetPasswordApi } = await setup('opaque-token');
    resetPasswordApi.nextError = {
      status: 409,
      error: {
        title: 'Conflict',
        status: 409,
        detail: 'Password reset token has expired.',
      },
    };

    await enterCredentials(fixture, 'nurse@example.com', 'NewPass1x');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    const alerts = Array.from(
      fixture.nativeElement.querySelectorAll('[role="alert"]'),
    ) as HTMLElement[];
    expect(alerts).toHaveLength(1);
    expect(alerts[0]?.textContent).toContain('Password reset token has expired.');
  });

  it('prevents duplicate submission while the approved request is in flight', async () => {
    const { fixture, resetPasswordApi } = await setup('opaque-token');

    await enterCredentials(fixture, 'nurse@example.com', 'NewPass1x');
    submitButton(fixture).click();
    submitButton(fixture).click();
    fixture.detectChanges();

    expect(submitButton(fixture).disabled).toBe(true);
    expect(resetPasswordApi.calls).toEqual([
      { email: 'nurse@example.com', token: 'opaque-token', newPassword: 'NewPass1x' },
    ]);
  });

  it('keeps backend failures inside Reset Password without navigating or touching AUTH-009', async () => {
    const { fixture, resetPasswordApi, router } = await setup('opaque-token');
    resetPasswordApi.nextError = {
      status: 409,
      error: {
        title: 'Conflict',
        status: 409,
        detail: 'Password reset token has expired.',
      },
    };
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await enterCredentials(fixture, 'nurse@example.com', 'NewPass1x');
    submitButton(fixture).click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Password reset token has expired.');
    expect(navigateSpy).not.toHaveBeenCalled();
    expect(textContent(fixture).toLowerCase()).not.toContain('reset-success');
  });

  it('keeps AUTH_RESET_PASSWORD public by lazily activating only the real reset-password route', async () => {
    const resetPasswordRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_RESET_PASSWORD').slice(1),
    );

    await expect(resetPasswordRoute?.loadComponent?.()).resolves.toBe(ResetPassword);
    expect(resetPasswordRoute?.component).toBeUndefined();
    expect(resetPasswordRoute?.canActivate).toBeUndefined();
    expect(routes.some((route) => route.path === 'auth/reset-password/success')).toBe(false);
  });

  it('keeps the Reset Password screen free of auth session, token storage, and AUTH-009 behavior', () => {
    const source = readTextFile('src/app/features/auth/reset-password/reset-password.ts');
    const template = readTextFile('src/app/features/auth/reset-password/reset-password.html');
    const combined = `${source}\n${template}`.toLowerCase();

    expect(source).toContain('ResetPasswordApi');
    expect(combined).not.toContain('authsessionbootstrap');
    expect(combined).not.toContain('currentuserstore');
    expect(combined).not.toContain('tokenstorage');
    expect(combined).not.toContain('sessionstorage');
    expect(combined).not.toContain('localstorage');
    expect(combined).not.toContain('reset-success');
    expect(combined).not.toContain('navigate');
  });
});
