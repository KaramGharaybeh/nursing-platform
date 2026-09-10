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
import { AUTH_SESSION_STORAGE } from './token-storage';
import type { TokenStorageBackend } from './token-storage';
import { AuthSessionBootstrap } from './auth-session-bootstrap';
import type { AuthResult } from '../api/generated/models/auth-result';
import { authenticatedRouteGuard } from './authenticated-route.guard';
import { RETURN_URL_QUERY_KEY, isSafeReturnUrl } from '../routing/safe-return';

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

function refreshSuccessFixture(): AuthResult {
  return {
    accessToken: 'access-token-new',
    expiresAt: '2026-09-08T11:00:00Z',
    refreshToken: 'refresh-token-new',
  };
}

function setup(backingStore = new MemoryStorageBackend()): {
  bootstrap: AuthSessionBootstrap;
  httpMock: HttpTestingController;
  router: Router;
  backingStore: MemoryStorageBackend;
} {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideApiConfig(),
      provideRouter([]),
      { provide: AUTH_SESSION_STORAGE, useValue: backingStore },
    ],
  });

  return {
    bootstrap: TestBed.inject(AuthSessionBootstrap),
    httpMock: TestBed.inject(HttpTestingController),
    router: TestBed.inject(Router),
    backingStore,
  };
}

function resolveAuthenticated(
  bootstrap: AuthSessionBootstrap,
  httpMock: HttpTestingController,
): void {
  bootstrap.bootstrap().subscribe();
  httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());
}

function resolveAnonymous(
  bootstrap: AuthSessionBootstrap,
  httpMock: HttpTestingController,
): void {
  bootstrap.bootstrap().subscribe();
  httpMock.expectNone('/api/v1/auth/refresh');
  httpMock.expectNone('/api/v1/me');
}

function callGuard(routeId: string | undefined, url: string): Observable<boolean | UrlTree> {
  const route = {
    data: routeId === undefined ? {} : { routeId },
  } as unknown as ActivatedRouteSnapshot;
  const state = { url } as unknown as RouterStateSnapshot;

  return TestBed.runInInjectionContext(() => authenticatedRouteGuard(route, state));
}

describe('authenticated-route.guard', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('allows an authenticated user on an AUTHENTICATED route', async () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock } = setup(backingStore);
    resolveAuthenticated(bootstrap, httpMock);

    const result = await firstValueFrom(callGuard('NURSE_ENTRY', '/nurse'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('redirects an anonymous user on an AUTHENTICATED route to sign-in with returnUrl', async () => {
    const { bootstrap, httpMock } = setup();
    resolveAnonymous(bootstrap, httpMock);

    const result = await firstValueFrom(callGuard('NURSE_ENTRY', '/nurse'));

    httpMock.verify();
    expect(result).toBeInstanceOf(UrlTree);
    const tree = result as UrlTree;
    expect(tree.queryParamMap.get(RETURN_URL_QUERY_KEY)).toBe('/nurse');
    expect(tree.toString()).toBe('/auth/sign-in?returnUrl=%2Fnurse');
  });

  it('preserves path, query, and fragment in the returnUrl value', async () => {
    const { bootstrap, httpMock } = setup();
    resolveAnonymous(bootstrap, httpMock);

    const result = await firstValueFrom(
      callGuard('NURSE_PROFILE_EXPERIENCE', '/nurse/profile?tab=experience#skills'),
    );

    httpMock.verify();
    const tree = result as UrlTree;
    expect(tree.queryParamMap.get(RETURN_URL_QUERY_KEY)).toBe(
      '/nurse/profile?tab=experience#skills',
    );
    expect(tree.toString()).toBe(
      '/auth/sign-in?returnUrl=%2Fnurse%2Fprofile%3Ftab%3Dexperience%23skills',
    );
  });

  it('produces a returnUrl that passes the safe-return helper', async () => {
    const { bootstrap, httpMock } = setup();
    resolveAnonymous(bootstrap, httpMock);

    const result = await firstValueFrom(
      callGuard('EMPLOYER_REQUEST_DETAIL', '/employer/requests/request-1?page=2'),
    );

    httpMock.verify();
    const returnUrl = (result as UrlTree).queryParamMap.get(RETURN_URL_QUERY_KEY);
    expect(returnUrl).not.toBeNull();
    expect(isSafeReturnUrl(returnUrl ?? '')).toBe(true);
  });

  it('waits while initializing without premature redirect or duplicate bootstrap', async () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock } = setup(backingStore);

    expect(bootstrap.state()).toBe('initializing');

    const result$ = callGuard('NURSE_ENTRY', '/nurse');
    const promise = firstValueFrom(result$);

    let resolved = false;
    promise.then(() => {
      resolved = true;
    });

    await Promise.resolve();
    await Promise.resolve();
    expect(resolved).toBe(false);

    resolveAuthenticated(bootstrap, httpMock);

    const result = await promise;
    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows an anonymous user on a PUBLIC route without waiting for session resolution', async () => {
    const { bootstrap, httpMock } = setup();
    resolveAnonymous(bootstrap, httpMock);

    const result = await firstValueFrom(callGuard('AUTH_SIGN_IN', '/auth/sign-in'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows an authenticated user on a PUBLIC route without redirecting away', async () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock } = setup(backingStore);
    resolveAuthenticated(bootstrap, httpMock);

    const result = await firstValueFrom(
      callGuard('PREPARATION_PACKAGES_OFFER_DETAIL', '/preparation-packages/offer-slug'),
    );

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('allows an anonymous user on the ENTRY route without redirect', async () => {
    const { bootstrap, httpMock } = setup();
    resolveAnonymous(bootstrap, httpMock);

    const result = await firstValueFrom(callGuard('ROOT_ENTRY', '/'));

    httpMock.verify();
    expect(result).toBe(true);
  });

  it('treats a route without a canonical routeId as AUTHENTICATED (fail-closed)', async () => {
    const { bootstrap, httpMock } = setup();
    resolveAnonymous(bootstrap, httpMock);

    const result = await firstValueFrom(callGuard(undefined, '/unknown-route'));

    httpMock.verify();
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).queryParamMap.get(RETURN_URL_QUERY_KEY)).toBe('/unknown-route');
  });

  it('keeps the guard free of current-user, storage, permission, and role behavior', () => {
    const source = readTextFile('src/app/core/auth/authenticated-route.guard.ts');
    const lowered = source.toLowerCase();

    expect(lowered).not.toContain('currentuser');
    expect(lowered).not.toContain('current-user');
    expect(lowered).not.toContain('tokenstorage');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('permission');
    expect(lowered).not.toContain('role');
    expect(lowered).not.toContain('navigate(');
    expect(lowered).not.toContain('navigatebyurl');
  });
});