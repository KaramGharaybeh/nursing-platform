import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { routes } from './app.routes';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { App } from './app';
import { AuthSessionBootstrap } from './core/auth/auth-session-bootstrap';
import { CurrentUserStore } from './core/auth/current-user-store';
import type { CurrentUser } from './core/auth/current-user';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const appDir = join(nodeGlobal.process.cwd(), 'src', 'app');
const authState = signal<'initializing' | 'authenticated' | 'anonymous'>('anonymous');
const currentUserStatus = signal<'idle' | 'loading' | 'ready' | 'anonymous' | 'unavailable'>('anonymous');
const currentUser = signal<CurrentUser | undefined>(undefined);

function readAppFile(name: string): string {
  return readFileSync(join(appDir, name), 'utf8');
}

describe('App shell frame', () => {
  beforeEach(async () => {
    authState.set('anonymous');
    currentUserStatus.set('anonymous');
    currentUser.set(undefined);
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        { provide: AuthSessionBootstrap, useValue: { state: authState.asReadonly() } },
        { provide: CurrentUserStore, useValue: { status: currentUserStatus.asReadonly(), currentUser: currentUser.asReadonly() } },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('renders exactly one main landmark', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelectorAll('main').length).toBe(1);
  });

  it('exposes the main content target with stable id and focusability', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const main = compiled.querySelector('main');
    expect(main?.getAttribute('id')).toBe('main-content');
    expect(main?.getAttribute('tabindex')).toBe('-1');
  });

  it('renders the router outlet inside the main landmark', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const main = compiled.querySelector('main#main-content');
    expect(main).not.toBeNull();
    expect(main?.querySelector('router-outlet')).not.toBeNull();
  });

  it('removes Angular scaffold placeholder content and links', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.angular-logo')).toBeNull();
    expect(compiled.querySelector('.pill-group')).toBeNull();
    expect(compiled.querySelector('.social-links')).toBeNull();
    expect(compiled.textContent).not.toContain('Hello, nursing-platform-frontend');
    expect(compiled.textContent).not.toContain('Congratulations');
  });

  it('does not show authenticated navigation for anonymous or public context', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="app-shell-header"]')).toBeNull();
    expect(compiled.querySelector('[data-testid="app-shell-primary-nav"]')).toBeNull();
  });

  it('shows authenticated shell navigation for authenticated application context', async () => {
    TestBed.resetTestingModule();
    authState.set('authenticated');
    currentUserStatus.set('ready');
    currentUser.set({
      id: 'user-1',
      email: 'nurse@example.test',
      username: 'nurse-user',
      firstName: 'Nurse',
      lastName: 'Example',
      isActive: true,
      emailVerified: true,
      isProfileComplete: true,
      roles: ['Nurse'],
      permissions: [],
      createdAt: '2026-01-01T00:00:00Z',
      lastLoginAt: undefined,
    });
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([{ path: 'exams', component: App }]),
        { provide: AuthSessionBootstrap, useValue: { state: authState.asReadonly() } },
        { provide: CurrentUserStore, useValue: { status: currentUserStatus.asReadonly(), currentUser: currentUser.asReadonly() } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/exams');
    await fixture.whenStable();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="app-shell-header"]')).not.toBeNull();
    expect(compiled.querySelector('[data-testid="app-shell-primary-nav"]')?.textContent).toContain('Exams');
  });

  it('does not show authenticated shell for authenticated users on public routes', async () => {
    TestBed.resetTestingModule();
    authState.set('authenticated');
    currentUserStatus.set('ready');
    currentUser.set({
      id: 'user-1',
      email: 'nurse@example.test',
      username: 'nurse-user',
      firstName: 'Nurse',
      lastName: 'Example',
      isActive: true,
      emailVerified: true,
      isProfileComplete: true,
      roles: ['Nurse'],
      permissions: [],
      createdAt: '2026-01-01T00:00:00Z',
      lastLoginAt: undefined,
    });
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([{ path: 'auth/sign-in', component: App }]),
        { provide: AuthSessionBootstrap, useValue: { state: authState.asReadonly() } },
        { provide: CurrentUserStore, useValue: { status: currentUserStatus.asReadonly(), currentUser: currentUser.asReadonly() } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/auth/sign-in');
    await fixture.whenStable();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="app-shell-header"]')).toBeNull();
    expect(compiled.querySelector('[data-testid="app-shell-primary-nav"]')).toBeNull();
  });

  it('keeps component separation with external template and style metadata', () => {
    const source = readAppFile('app.ts');
    expect(source).toMatch(/templateUrl\s*:\s*['"]\.\/app\.html['"]/);
    expect(source).toMatch(/styleUrl\s*:\s*['"]\.\/app\.scss['"]/);
    expect(source).not.toMatch(/^\s*template\s*:/m);
    expect(source).not.toMatch(/^\s*styles\s*:/m);
  });

  it('keeps the shell template free of embedded style blocks', () => {
    const template = readAppFile('app.html');
    expect(template.toLowerCase()).not.toContain('<style');
  });

  it('styles the shell with logical properties and approved tokens only', () => {
    const css = readAppFile('app.scss');
    expect(css).toMatch(/min-block-size/);
    expect(css).toMatch(/padding-inline\s*:\s*var\(\s*--np-page-gutter\s*\)/);
    expect(css).toMatch(/var\(\s*--np-color-brand-2\s*\)/);
    expect(css).not.toMatch(/#[0-9a-fA-F]{3,8}/);
    expect(css.toLowerCase()).not.toContain('oklch');
    for (const banned of ['margin-left', 'margin-right', 'padding-left', 'padding-right']) {
      expect(css).not.toContain(banned);
    }
    expect(css).not.toMatch(/^\s*left\s*:/m);
    expect(css).not.toMatch(/^\s*right\s*:/m);
    expect(css).not.toMatch(/float\s*:\s*(left|right)/);
    expect(css).not.toMatch(/text-align\s*:\s*(left|right)/);
  });
});

describe('App route loading state (T-FE-028)', () => {
  it('announces route loading while navigation is in progress, then hides it', async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([{ path: 'x', component: App }]),
        { provide: AuthSessionBootstrap, useValue: { state: authState.asReadonly() } },
        { provide: CurrentUserStore, useValue: { status: currentUserStatus.asReadonly(), currentUser: currentUser.asReadonly() } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    const navigation = router.navigateByUrl('/x');
    fixture.detectChanges();
    const loading = (fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="route-loading"]',
    );
    expect(loading).not.toBeNull();
    expect(loading?.querySelector('[role="status"]')).not.toBeNull();
    expect(loading?.textContent).toContain('Loading page');
    await navigation;
    await fixture.whenStable();
    fixture.detectChanges();
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('[data-testid="route-loading"]'),
    ).toBeNull();
  });

  it('hides route loading when navigation fails without adding a loading route', async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        { provide: AuthSessionBootstrap, useValue: { state: authState.asReadonly() } },
        { provide: CurrentUserStore, useValue: { status: currentUserStatus.asReadonly(), currentUser: currentUser.asReadonly() } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/does-not-exist').catch(() => undefined);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="route-loading"]')).toBeNull();
    expect(routes.every((entry) => !String(entry.path ?? '').includes('loading'))).toBe(true);
  });
});
