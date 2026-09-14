// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { routes } from '../../../app.routes';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { RoleSelection } from './role-selection';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

async function createComponent(): Promise<ComponentFixture<RoleSelection>> {
  await TestBed.configureTestingModule({
    imports: [RoleSelection],
    providers: [provideRouter([])],
  }).compileComponents();

  const fixture = TestBed.createComponent(RoleSelection);
  fixture.detectChanges();
  return fixture;
}

describe('AUTH-002 Role Selection', () => {
  it('renders the approved public pre-registration choices', async () => {
    const fixture = await createComponent();
    const text = fixture.nativeElement.textContent as string;

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Choose how to register');
    expect(text).toContain('Nurse');
    expect(text).toContain('Employer');
    expect(text).toContain('Create a nurse account');
    expect(text).toContain('Create an employer account');
  });

  it('navigates Nurse choice to the canonical Nurse registration route', async () => {
    const fixture = await createComponent();
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    const button = fixture.nativeElement.querySelector('[data-testid="auth-role-selection-nurse"]') as HTMLButtonElement;
    button.click();

    expect(navigateByUrl).toHaveBeenCalledWith(canonicalRoutePath('AUTH_REGISTER_NURSE'));
  });

  it('navigates Employer choice to the canonical Employer registration route', async () => {
    const fixture = await createComponent();
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    const button = fixture.nativeElement.querySelector('[data-testid="auth-role-selection-employer"]') as HTMLButtonElement;
    button.click();

    expect(navigateByUrl).toHaveBeenCalledWith(canonicalRoutePath('AUTH_REGISTER_EMPLOYER'));
  });

  it('activates the canonical PUBLIC /auth/role-selection route without guards or redirects', async () => {
    const roleSelectionRoute = routes.find((route) => route.path === canonicalRoutePath('AUTH_ROLE_SELECTION').slice(1));

    await expect(roleSelectionRoute?.loadComponent?.()).resolves.toBe(RoleSelection);
    expect(roleSelectionRoute?.canActivate).toBeUndefined();
    expect(roleSelectionRoute?.canMatch).toBeUndefined();
    expect(roleSelectionRoute?.redirectTo).toBeUndefined();
    expect(roleSelectionRoute?.data).toBeUndefined();
  });

  it('does not reference backend, auth session, current user, token, or logout behavior', () => {
    const component = readTextFile('src/app/features/auth/role-selection/role-selection.ts');
    const template = readTextFile('src/app/features/auth/role-selection/role-selection.html');
    const source = `${component}\n${template}`.toLowerCase();

    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('authtransport');
    expect(source).not.toContain('authsessionbootstrap');
    expect(source).not.toContain('currentuserstore');
    expect(source).not.toContain('tokenstorage');
    expect(source).not.toContain('refreshcoordinator');
    expect(source).not.toContain('locallogout');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('localstorage');
  });

  it('offers Already have an account Sign in onward navigation', async () => {
    const fixture = await createComponent();

    const signInLink = fixture.nativeElement.querySelector(
      `a[href="${canonicalRoutePath('AUTH_SIGN_IN')}"]`,
    ) as HTMLAnchorElement | null;

    expect(signInLink).not.toBeNull();
    expect(signInLink?.textContent).toContain('Sign in');
    expect(fixture.nativeElement.textContent).toContain('Already have an account?');
  });
});
