import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthenticatedAppShell } from './authenticated-app-shell';
import { LocalLogout } from '../auth/local-logout';

describe('AuthenticatedAppShell', () => {
  const logoutSpy = { logout: vi.fn() };

  beforeEach(async () => {
    logoutSpy.logout.mockReset();
    await TestBed.configureTestingModule({
      imports: [AuthenticatedAppShell],
      providers: [
        provideRouter([{ path: 'auth/sign-in', component: AuthenticatedAppShell }]),
        { provide: LocalLogout, useValue: logoutSpy },
      ],
    }).compileComponents();
  });

  it('renders brand, primary navigation, account, and sign out without DPF controls or identity details', () => {
    const fixture = TestBed.createComponent(AuthenticatedAppShell);
    fixture.componentRef.setInput('navigationItems', [
      { routeId: 'EXAMS_CATALOG', label: 'Exams', path: '/exams', active: true },
      { routeId: 'NURSE_PROFILE_OVERVIEW', label: 'Profile', path: '/nurse/profile', active: false },
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="app-shell-brand"]')?.textContent).toContain('Nursing Platform');
    expect(compiled.querySelector('[data-testid="app-shell-primary-nav"]')?.textContent).toContain('Exams');
    expect(compiled.querySelector('[data-testid="app-shell-account"]')?.textContent).toContain('Account');
    expect(compiled.querySelector('[data-testid="app-shell-sign-out"]')?.textContent).toContain('Sign out');
    expect(compiled.querySelector('[data-testid="app-shell-notifications"]')).toBeNull();
    expect(compiled.querySelector('[data-testid="app-shell-help"]')).toBeNull();
    expect(compiled.textContent).not.toContain('RN');
    expect(compiled.textContent).not.toContain('verified');
  });

  it('marks only the active primary item with aria-current', () => {
    const fixture = TestBed.createComponent(AuthenticatedAppShell);
    fixture.componentRef.setInput('navigationItems', [
      { routeId: 'EXAMS_CATALOG', label: 'Exams', path: '/exams', active: true },
      { routeId: 'COMMERCE_PRODUCTS', label: 'Products', path: '/commerce/products', active: false },
    ]);
    fixture.detectChanges();

    const active = (fixture.nativeElement as HTMLElement).querySelector('[aria-current="page"]');
    expect(active?.textContent).toContain('Exams');
    expect((fixture.nativeElement as HTMLElement).querySelectorAll('[aria-current="page"]').length).toBe(1);
  });

  it('opens mobile navigation, closes on Escape, and returns focus to the trigger', () => {
    const fixture = TestBed.createComponent(AuthenticatedAppShell);
    fixture.componentRef.setInput('navigationItems', [
      { routeId: 'EXAMS_CATALOG', label: 'Exams', path: '/exams', active: false },
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector<HTMLButtonElement>('[data-testid="app-shell-menu-trigger"]');
    trigger?.focus();
    trigger?.click();
    fixture.detectChanges();
    expect(compiled.querySelector('[data-testid="app-shell-mobile-panel"]')).not.toBeNull();

    compiled.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    fixture.detectChanges();
    expect(compiled.querySelector('[data-testid="app-shell-mobile-panel"]')).toBeNull();
    expect(document.activeElement).toBe(trigger);
  });

  it('closes mobile navigation on route change', async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [AuthenticatedAppShell],
      providers: [
        provideRouter([{ path: 'x', component: AuthenticatedAppShell }]),
        { provide: LocalLogout, useValue: logoutSpy },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AuthenticatedAppShell);
    fixture.componentRef.setInput('navigationItems', [
      { routeId: 'EXAMS_CATALOG', label: 'Exams', path: '/exams', active: false },
    ]);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    compiled.querySelector<HTMLButtonElement>('[data-testid="app-shell-menu-trigger"]')?.click();
    fixture.detectChanges();
    expect(compiled.querySelector('[data-testid="app-shell-mobile-panel"]')).not.toBeNull();

    await TestBed.inject(Router).navigateByUrl('/x');
    await fixture.whenStable();
    fixture.detectChanges();
    expect(compiled.querySelector('[data-testid="app-shell-mobile-panel"]')).toBeNull();
  });

  it('signs out through LocalLogout and navigates to sign in', async () => {
    const fixture = TestBed.createComponent(AuthenticatedAppShell);
    fixture.componentRef.setInput('navigationItems', []);
    fixture.detectChanges();

    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLButtonElement>('[data-testid="app-shell-sign-out"]')
      ?.click();
    await fixture.whenStable();

    expect(logoutSpy.logout).toHaveBeenCalledTimes(1);
    expect(TestBed.inject(Router).url).toBe('/auth/sign-in');
  });
});
