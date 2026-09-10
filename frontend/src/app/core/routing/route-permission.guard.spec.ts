import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import type { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Router, UrlTree } from '@angular/router';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { firstValueFrom } from 'rxjs';
import type { Observable } from 'rxjs';
import { provideApiConfig } from '../api/api-config';
import type { UserDetailDto } from '../api/generated/models/user-detail-dto';
import { AUTH_SESSION_STORAGE } from '../auth/token-storage';
import type { TokenStorageBackend } from '../auth/token-storage';
import { CurrentUserStore } from '../auth/current-user-store';
import { canonicalRoutePath } from './canonical-routes';
import { routePermissionGuard } from './route-permission.guard';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class MemoryStorageBackend implements TokenStorageBackend {
  readonly values = new Map<string, string>();

  getItem(key: string): string | null {
    return this.values.get(key) ?? null;
  }

  setItem(key: string, value: string): void {
    this.values.set(key, value);
  }

  removeItem(key: string): void {
    this.values.delete(key);
  }
}

function userDetailFixture(roles: string[], permissions: string[]): UserDetailDto {
  return {
    createdAt: '2026-09-08T10:00:00Z',
    email: 'user@example.com',
    emailVerified: true,
    firstName: 'Test',
    id: 'user-1',
    isActive: true,
    lastLoginAt: '2026-09-08T09:00:00Z',
    lastName: 'User',
    permissions,
    roles,
  };
}

function setup(): { store: CurrentUserStore; httpMock: HttpTestingController; router: Router } {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideApiConfig(),
      provideRouter([]),
      { provide: AUTH_SESSION_STORAGE, useValue: new MemoryStorageBackend() },
    ],
  });

  return {
    store: TestBed.inject(CurrentUserStore),
    httpMock: TestBed.inject(HttpTestingController),
    router: TestBed.inject(Router),
  };
}

function hydrateReady(
  store: CurrentUserStore,
  httpMock: HttpTestingController,
  roles: string[],
  permissions: string[],
): void {
  store.hydrate().subscribe();
  const request = httpMock.expectOne('/api/v1/me');
  request.flush(userDetailFixture(roles, permissions));
}

function callGuard(routeId: string | undefined, url: string): Observable<boolean | UrlTree> {
  const route = {
    data: routeId === undefined ? {} : { routeId },
  } as unknown as ActivatedRouteSnapshot;
  const state = { url } as unknown as RouterStateSnapshot;

  return TestBed.runInInjectionContext(() => routePermissionGuard(route, state));
}

describe('route-permission.guard', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('allows any ready user for AUTHENTICATED_ONLY', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, [], []);

    const result = await firstValueFrom(callGuard('ACCOUNT_OVERVIEW', '/account'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows ROLE only when the ready user holds the accepted role', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, ['Nurse'], []);

    const allowed = await firstValueFrom(callGuard('NURSE_ENTRY', '/nurse'));
    httpMock.verify();
    expect(allowed).toBe(true);
  });

  it('redirects a ready user failing ROLE to canonical access-denied without returnUrl', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, ['Employer'], []);

    const result = await firstValueFrom(callGuard('NURSE_ENTRY', '/nurse'));

    httpMock.verify();
    expect(result).toBeInstanceOf(UrlTree);
    const tree = result as UrlTree;
    expect(tree.toString()).toBe(canonicalRoutePath('SYSTEM_ACCESS_DENIED'));
    expect(tree.queryParamMap.has('returnUrl')).toBe(false);
  });

  it('requires both role and permission without admin bypass', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, ['Admin'], []);

    const denied = await firstValueFrom(callGuard('ADMIN_USERS', '/admin/users'));

    httpMock.verify();
    expect(denied).toBeInstanceOf(UrlTree);
    expect((denied as UrlTree).toString()).toBe(canonicalRoutePath('SYSTEM_ACCESS_DENIED'));
  });

  it('allows ROLE_AND_PERMISSION when both role and permission pass', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, ['Admin'], ['Users.View']);

    const result = await firstValueFrom(callGuard('ADMIN_USERS', '/admin/users'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('applies multi-role union semantics with no role precedence', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, ['Admin', 'Nurse'], []);

    const nurseAllowed = await firstValueFrom(callGuard('NURSE_ENTRY', '/nurse'));
    const employerDenied = await firstValueFrom(callGuard('EMPLOYER_HOME', '/employer'));

    httpMock.verify();
    expect(nurseAllowed).toBe(true);
    expect(employerDenied).toBeInstanceOf(UrlTree);
  });

  it('waits while idle then evaluates once ready', async () => {
    const { store, httpMock } = setup();
    expect(store.status()).toBe('idle');

    const result$ = callGuard('NURSE_ENTRY', '/nurse');
    const promise = firstValueFrom(result$);

    let resolved = false;
    promise.then(() => {
      resolved = true;
    });

    await Promise.resolve();
    await Promise.resolve();
    expect(resolved).toBe(false);

    hydrateReady(store, httpMock, ['Nurse'], []);

    const result = await promise;
    httpMock.verify();
    expect(result).toBe(true);
  });

  it('waits while loading without treating it as denied', async () => {
    const { store, httpMock } = setup();
    store.hydrate().subscribe();
    expect(store.status()).toBe('loading');

    const result$ = callGuard('ADMIN_USERS', '/admin/users');
    const promise = firstValueFrom(result$);

    let resolved = false;
    promise.then(() => {
      resolved = true;
    });

    await Promise.resolve();
    await Promise.resolve();
    expect(resolved).toBe(false);

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(userDetailFixture(['Admin'], ['Users.View']));

    const result = await promise;
    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows anonymous without sign-in redirect or access-denied redirect', async () => {
    const { store, httpMock } = setup();
    store.resolveAnonymous();
    expect(store.status()).toBe('anonymous');

    const result = await firstValueFrom(callGuard('NURSE_ENTRY', '/nurse'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows unavailable without deny or redirect', async () => {
    const { store, httpMock } = setup();
    store.hydrate().subscribe();
    const request = httpMock.expectOne('/api/v1/me');
    request.error(new ProgressEvent('error'));
    expect(store.status()).toBe('unavailable');

    const result = await firstValueFrom(callGuard('ADMIN_USERS', '/admin/users'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows PUBLIC and ENTRY routes without requiring a policy', async () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, [], []);

    const publicResult = await firstValueFrom(callGuard('AUTH_SIGN_IN', '/auth/sign-in'));
    const entryResult = await firstValueFrom(callGuard('ROOT_ENTRY', '/'));

    httpMock.verify();
    expect(publicResult).toBe(true);
    expect(entryResult).toBe(true);
  });

  it('treats missing policy for an AUTHENTICATED route as an explicit failure', () => {
    const { store, httpMock } = setup();
    hydrateReady(store, httpMock, ['Nurse'], []);

    expect(() => callGuard('UNKNOWN_ROUTE', '/unknown')).toThrow();
    expect(() => callGuard(undefined, '/unknown')).toThrow();

    httpMock.verify();
  });

  it('uses the canonical access-denied destination without duplicating its path', () => {
    const source = readTextFile('src/app/core/routing/route-permission.guard.ts');

    expect(source).toContain('SYSTEM_ACCESS_DENIED');
    expect(source).toContain('canonicalRoutePath');
    expect(source).not.toContain('/access-denied');
    expect(source).not.toContain('returnUrl');
  });

  it('consumes CurrentUserStore only without backend, storage, or session flows', () => {
    const source = readTextFile('src/app/core/routing/route-permission.guard.ts');
    const lowered = source.toLowerCase();

    expect(source).toContain('CurrentUserStore');
    expect(lowered).not.toContain('/me');
    expect(lowered).not.toContain('authtransport');
    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('tokenstorage');
    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('jwt');
    expect(lowered).not.toContain('startsWith');
    expect(lowered).not.toContain('navigate(');
    expect(lowered).not.toContain('navigatebyurl');
  });

  it('holds no navigation, menu, sidebar, breadcrumb, or link visibility behavior', () => {
    const source = readTextFile('src/app/core/routing/route-permission.guard.ts');
    const lowered = source.toLowerCase();

    expect(lowered).not.toContain('menu');
    expect(lowered).not.toContain('sidebar');
    expect(lowered).not.toContain('breadcrumb');
    expect(lowered).not.toContain('403');
  });
});
