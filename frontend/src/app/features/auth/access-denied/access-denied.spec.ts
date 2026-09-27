// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { routes } from '../../../app.routes';
import { AccessDenied } from './access-denied';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

describe('AUTH-011 Access Denied', () => {
  it('uses a centered safe terminal card and non-operational home appearance', async () => {
    await TestBed.configureTestingModule({ imports: [AccessDenied], providers: [provideRouter(routes)] }).compileComponents();
    const fixture = TestBed.createComponent(AccessDenied);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('.np-access-denied-public-header')).not.toBeNull();
    expect(root.querySelector('.np-access-denied-context')).toBeNull();
    expect(root.querySelector('.np-access-denied-public-header a[href="/preparation-packages"]')).not.toBeNull();
    expect(root.querySelector('.np-access-denied-public-header a[href="/auth/sign-up"]')).not.toBeNull();
    expect(root.querySelector('.np-access-denied-icon svg[aria-hidden="true"]')).not.toBeNull();
    expect(root.querySelector('.np-access-denied-home')?.textContent).toContain('Go to home');
    expect(root.querySelector('.np-access-denied-home a')).toBeNull();
    const signInVisual = root.querySelector('.np-access-denied-sign-in');
    expect(signInVisual?.textContent).toContain('Sign in');
    expect(signInVisual?.querySelector('a, button')).toBeNull();
  });
  it('renders the authorized terminal screen identity with an account recovery action', async () => {
    await TestBed.configureTestingModule({
      imports: [AccessDenied],
      providers: [provideRouter(routes)],
    }).compileComponents();

    const fixture = TestBed.createComponent(AccessDenied);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.np-access-denied-product')?.textContent).toContain(
      'Nursing Platform',
    );
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Access denied');
    const accountLink = fixture.nativeElement.querySelector(
      `a[href="${canonicalRoutePath('ACCOUNT_OVERVIEW')}"]`,
    ) as HTMLAnchorElement | null;
    expect(accountLink).not.toBeNull();
    expect(accountLink?.textContent).toContain('Go to account');
    expect(fixture.nativeElement.querySelector('button')).toBeNull();
  });

  it('does not reference auth/session/token/current-user behavior', () => {
    const component = readTextFile('src/app/features/auth/access-denied/access-denied.ts');
    const template = readTextFile('src/app/features/auth/access-denied/access-denied.html');
    const source = `${component}\n${template}`.toLowerCase();

    expect(source).not.toContain('authsessionbootstrap');
    expect(source).not.toContain('currentuserstore');
    expect(source).not.toContain('tokenstorage');
    expect(source).not.toContain('locallogout');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('httpclient');
  });

  it('activates the canonical PUBLIC /access-denied route without guards or redirects', async () => {
    const accessDeniedRoute = routes.find((route) => route.path === 'access-denied');

    await expect(accessDeniedRoute?.loadComponent?.()).resolves.toBe(AccessDenied);
    expect(accessDeniedRoute?.canActivate).toBeUndefined();
    expect(accessDeniedRoute?.redirectTo).toBeUndefined();
  });
});
