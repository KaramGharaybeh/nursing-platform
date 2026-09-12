// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { TestBed } from '@angular/core/testing';
import { routes } from '../../../app.routes';
import { SessionExpired } from './session-expired';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

describe('AUTH-010 Session Expired', () => {
  it('renders only the authorized terminal screen identity without invented copy or actions', async () => {
    await TestBed.configureTestingModule({ imports: [SessionExpired] }).compileComponents();

    const fixture = TestBed.createComponent(SessionExpired);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.np-session-expired-product')?.textContent).toContain(
      'Nursing Platform',
    );
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Session expired');
    expect(fixture.nativeElement.querySelector('a')).toBeNull();
    expect(fixture.nativeElement.querySelector('button')).toBeNull();
    expect(fixture.nativeElement.querySelector('[routerLink]')).toBeNull();
  });

  it('does not reference auth/session/token/current-user behavior', () => {
    const component = readTextFile('src/app/features/auth/session-expired/session-expired.ts');
    const template = readTextFile('src/app/features/auth/session-expired/session-expired.html');
    const source = `${component}\n${template}`.toLowerCase();

    expect(source).not.toContain('authsessionbootstrap');
    expect(source).not.toContain('currentuserstore');
    expect(source).not.toContain('tokenstorage');
    expect(source).not.toContain('locallogout');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('httpclient');
  });

  it('activates the canonical PUBLIC /session-expired route without guards or redirects', async () => {
    const sessionExpiredRoute = routes.find((route) => route.path === 'session-expired');

    await expect(sessionExpiredRoute?.loadComponent?.()).resolves.toBe(SessionExpired);
    expect(sessionExpiredRoute?.canActivate).toBeUndefined();
    expect(sessionExpiredRoute?.redirectTo).toBeUndefined();
  });
});
