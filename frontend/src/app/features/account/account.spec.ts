// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { TestBed } from '@angular/core/testing';
import { routes } from '../../app.routes';
import { Account } from './account';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

describe('ACC-001 Account overview shell', () => {
  it('renders only the authorized destination shell identity without invented copy or actions', async () => {
    await TestBed.configureTestingModule({ imports: [Account] }).compileComponents();

    const fixture = TestBed.createComponent(Account);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.np-account-product')?.textContent).toContain(
      'Nursing Platform',
    );
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Account overview');
    expect(fixture.nativeElement.querySelector('a')).toBeNull();
    expect(fixture.nativeElement.querySelector('button')).toBeNull();
    expect(fixture.nativeElement.querySelector('[routerLink]')).toBeNull();
  });

  it('does not reference auth/session/token/current-user/API behavior', () => {
    const component = readTextFile('src/app/features/account/account.ts');
    const template = readTextFile('src/app/features/account/account.html');
    const source = `${component}\n${template}`.toLowerCase();

    expect(source).not.toContain('authsessionbootstrap');
    expect(source).not.toContain('currentuserstore');
    expect(source).not.toContain('tokenstorage');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('routerlink');
    expect(source).not.toContain('navigate');
  });

  it('activates the canonical /account route through the lazy loader', async () => {
    const accountRoute = routes.find((route) => route.path === 'account');

    await expect(accountRoute?.loadComponent?.()).resolves.toBe(Account);
    expect(accountRoute?.component).toBeUndefined();
  });
});