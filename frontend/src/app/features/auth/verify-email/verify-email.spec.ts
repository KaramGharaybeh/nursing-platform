import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { Observable, of } from 'rxjs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import type { VerifyEmailRequest } from '../../../core/api/generated/models/verify-email-request';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { routes } from '../../../app.routes';
import { VerifyEmail } from './verify-email';
import { VerifyEmailApi } from './verify-email-api';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class VerifyEmailApiStub {
  readonly calls: VerifyEmailRequest[] = [];
  nextError: unknown;

  verifyEmail(request: VerifyEmailRequest) {
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
  const verifyEmailApi = new VerifyEmailApiStub();

  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [VerifyEmail],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter(routes),
      { provide: VerifyEmailApi, useValue: verifyEmailApi },
      { provide: ActivatedRoute, useValue: activatedRouteWithToken(token) },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(VerifyEmail);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();

  return {
    fixture,
    component: fixture.componentInstance,
    verifyEmailApi,
    router: TestBed.inject(Router),
    httpMock: TestBed.inject(HttpTestingController),
  };
}

function textContent(fixture: { nativeElement: HTMLElement }): string {
  return fixture.nativeElement.textContent ?? '';
}

describe('AUTH-006 Verify Email', () => {
  it('uses the public header and one centered status card without decorative marketing context', async () => {
    const { fixture } = await setup(null);
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('.np-verify-email-public-header')?.textContent).toContain('Nursing Platform');
    expect(root.querySelector('h1')?.textContent).toContain('Verify email');
    expect(root.querySelector('.np-verify-email-context')).toBeNull();
    expect(root.querySelector('.np-verify-email-public-header a[href="/preparation-packages"]')).not.toBeNull();
    expect(root.querySelector('.np-verify-email-public-header a[href="/auth/sign-up"]')).not.toBeNull();
  });
  afterEach(() => {
    try {
      TestBed.inject(HttpTestingController).verify();
    } catch {
      // Source-only route/scope tests do not need HTTP testing providers.
    }
    TestBed.resetTestingModule();
  });

  it('renders the approved verify-email identity without exposing the query token', async () => {
    const { fixture, verifyEmailApi } = await setup('opaque-token');

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Verify email');
    expect(textContent(fixture)).toContain('Email verified successfully.');
    expect(fixture.nativeElement.querySelector('[role="status"]')?.textContent).toContain(
      'Email verified successfully.',
    );
    expect(textContent(fixture)).not.toContain('opaque-token');
    expect(verifyEmailApi.calls).toEqual([{ token: 'opaque-token' }]);
  });

  it('blocks verification when the token query parameter is missing and makes no backend call', async () => {
    const { fixture, verifyEmailApi } = await setup(null);

    expect(textContent(fixture)).toContain('verification link is invalid or missing');
    expect(textContent(fixture)).not.toContain('Email verified successfully.');
    expect(verifyEmailApi.calls).toEqual([]);
  });

  it('blocks verification when the token query parameter is empty or whitespace and makes no backend call', async () => {
    const { fixture, verifyEmailApi } = await setup('   ');

    expect(textContent(fixture)).toContain('verification link is invalid or missing');
    expect(verifyEmailApi.calls).toEqual([]);
  });

  it('calls the verify-email adapter exactly once with the opaque query token', async () => {
    const { verifyEmailApi } = await setup('opaque-token');

    expect(verifyEmailApi.calls).toEqual([{ token: 'opaque-token' }]);
    expect(verifyEmailApi.calls).toHaveLength(1);
  });

  it('shows a safe error without exposing the token when verification fails', async () => {
    TestBed.resetTestingModule();
    const verifyEmailApi = new VerifyEmailApiStub();
    verifyEmailApi.nextError = {
      status: 409,
      error: {
        title: 'Conflict',
        status: 409,
        detail: 'Verification token has expired.',
      },
    };

    await TestBed.configureTestingModule({
      imports: [VerifyEmail],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter(routes),
        { provide: VerifyEmailApi, useValue: verifyEmailApi },
        { provide: ActivatedRoute, useValue: activatedRouteWithToken('opaque-token') },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(VerifyEmail);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(textContent(fixture)).toContain('Verification token has expired.');
    expect(textContent(fixture)).not.toContain('opaque-token');
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
    expect(verifyEmailApi.calls).toEqual([{ token: 'opaque-token' }]);
  });

  it('keeps AUTH_VERIFY_EMAIL_CONFIRM public and distinct from the check-email notice route', async () => {
    const confirmRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_VERIFY_EMAIL_CONFIRM').slice(1),
    );
    const checkEmailRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST').slice(1),
    );

    await expect(confirmRoute?.loadComponent?.()).resolves.toBe(VerifyEmail);
    expect(confirmRoute?.component).toBeUndefined();
    expect(confirmRoute?.canActivate).toBeUndefined();
    expect(confirmRoute?.canMatch).toBeUndefined();
    expect(confirmRoute?.redirectTo).toBeUndefined();
    expect(confirmRoute?.data).toBeUndefined();
    expect(checkEmailRoute).toBeDefined();
    expect(checkEmailRoute).not.toBe(confirmRoute);
  });

  it('keeps the Verify Email screen free of AUTH-005, session, token-handling, and resend behavior', () => {
    const source = readTextFile('src/app/features/auth/verify-email/verify-email.ts');
    const template = readTextFile('src/app/features/auth/verify-email/verify-email.html');
    const apiSource = readTextFile('src/app/features/auth/verify-email/verify-email-api.ts');
    const combined = `${source}\n${template}\n${apiSource}`.toLowerCase();

    expect(source).toContain('VerifyEmailApi');
    expect(combined).not.toContain('sendverificationemail');
    expect(combined).not.toContain('send-verification');
    expect(combined).not.toContain('resend');
    expect(combined).not.toContain('authsessionbootstrap');
    expect(combined).not.toContain('currentuserstore');
    expect(combined).not.toContain('tokenstorage');
    expect(combined).not.toContain('sessionstorage');
    expect(combined).not.toContain('localstorage');
    expect(combined).not.toContain('navigate');
  });

  it('renders a Continue to sign in link on success', async () => {
    const { fixture } = await setup('opaque-token');

    const signInLink = fixture.nativeElement.querySelector(
      `.np-verify-email-recovery a[href="${canonicalRoutePath('AUTH_SIGN_IN')}"]`,
    ) as HTMLAnchorElement | null;

    expect(signInLink).not.toBeNull();
    expect(signInLink?.textContent).toContain('Continue to sign in');
  });

  it('renders a Back to sign in link when token is missing', async () => {
    const { fixture } = await setup(null);

    const signInLink = fixture.nativeElement.querySelector(
      `.np-verify-email-recovery a[href="${canonicalRoutePath('AUTH_SIGN_IN')}"]`,
    ) as HTMLAnchorElement | null;

    expect(signInLink).not.toBeNull();
    expect(signInLink?.textContent).toContain('Back to sign in');
  });
});
