// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { routes } from '../../../app.routes';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { CheckEmail } from './check-email';
import { VerifyEmail } from '../verify-email/verify-email';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

async function createComponent(): Promise<ComponentFixture<CheckEmail>> {
  await TestBed.configureTestingModule({
    imports: [CheckEmail],
    providers: [provideRouter([])],
  }).compileComponents();

  const fixture = TestBed.createComponent(CheckEmail);
  fixture.detectChanges();
  return fixture;
}

describe('AUTH-005 Check Email', () => {
  it('uses a public header and a centered verification card without decorative context', async () => {
    const fixture = await createComponent();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('.np-check-email-public-header')?.textContent).toContain('Nursing Platform');
    expect(root.querySelector('.np-check-email-card h1')?.textContent).toContain('Check your email');
    expect(root.querySelector('.np-check-email-context')).toBeNull();
    expect(root.querySelector('.np-check-email-public-header a[href="/preparation-packages"]')).not.toBeNull();
    expect(root.querySelector('.np-check-email-public-header a[href="/auth/sign-up"]')).not.toBeNull();
  });
  it('renders the public check-email informational state without claiming delivery certainty', async () => {
    const fixture = await createComponent();
    const text = (fixture.nativeElement.textContent as string) ?? '';

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Check your email');
    expect(text).toContain('If registration was successful');
    expect(text).toContain('verification email has been sent');
    expect(text).toContain('check your inbox');
  });

  it('renders a Go to sign in link on the canonical sign-in route', async () => {
    const fixture = await createComponent();

    const signInLink = fixture.nativeElement.querySelector(
      `.np-check-email-sign-in a[href="${canonicalRoutePath('AUTH_SIGN_IN')}"]`,
    ) as HTMLAnchorElement | null;

    expect(signInLink).not.toBeNull();
    expect(signInLink?.textContent).toContain('Go to sign in');
  });

  it('renders no resend, countdown, or delivery guarantee behavior', async () => {
    const fixture = await createComponent();

    expect(fixture.nativeElement.querySelector('.np-check-email-card button')).toBeNull();
  });

  it('activates the canonical PUBLIC /auth/verify-email route without guards or redirects', async () => {
    const checkEmailRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_VERIFY_EMAIL_REQUEST').slice(1),
    );

    await expect(checkEmailRoute?.loadComponent?.()).resolves.toBe(CheckEmail);
    expect(checkEmailRoute?.component).toBeUndefined();
    expect(checkEmailRoute?.canActivate).toBeUndefined();
    expect(checkEmailRoute?.canMatch).toBeUndefined();
    expect(checkEmailRoute?.redirectTo).toBeUndefined();
    expect(checkEmailRoute?.data).toBeUndefined();
  });

  it('preserves the AUTH_VERIFY_EMAIL_CONFIRM route without change', async () => {
    const confirmRoute = routes.find(
      (route) => route.path === canonicalRoutePath('AUTH_VERIFY_EMAIL_CONFIRM').slice(1),
    );

    await expect(confirmRoute?.loadComponent?.()).resolves.toBe(VerifyEmail);
    expect(confirmRoute?.component).toBeUndefined();
    expect(confirmRoute?.canActivate).toBeUndefined();
  });

  it('does not reference backend, resend, token, session, current user, or countdown behavior', () => {
    const component = readTextFile('src/app/features/auth/check-email/check-email.ts');
    const template = readTextFile('src/app/features/auth/check-email/check-email.html');
    const source = `${component}\n${template}`.toLowerCase();

    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('authtransport');
    expect(source).not.toContain('verifyemailapi');
    expect(source).not.toContain('sendverificationemail');
    expect(source).not.toContain('send-verification');
    expect(source).not.toContain('resend');
    expect(source).not.toContain('countdown');
    expect(source).not.toContain('cooldown');
    expect(source).not.toContain('authsessionbootstrap');
    expect(source).not.toContain('currentuserstore');
    expect(source).not.toContain('tokenstorage');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('localstorage');
  });
});
