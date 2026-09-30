import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { provideApiConfig } from '../api/api-config';
import type { AuthResult } from '../api/generated/models/auth-result';
import type { UserDetailDto } from '../api/generated/models/user-detail-dto';
import { AuthSessionBootstrap } from './auth-session-bootstrap';
import { CurrentUserStore } from './current-user-store';
import { LocalLogout } from './local-logout';
import { RefreshCoordinator } from './refresh-coordinator';
import { AUTH_SESSION_STORAGE, TokenStorage } from './token-storage';
import type { TokenStorageBackend } from './token-storage';

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
    expiresAt: '2026-09-09T12:00:00Z',
    refreshToken: 'refresh-token-new',
  };
}

function userDetailFixture(): UserDetailDto {
  return {
    createdAt: '2026-09-09T10:00:00Z',
    email: 'nurse@example.com',
    emailVerified: true,
    firstName: 'Nurse',
    id: 'user-1',
    isActive: true,
    isProfileComplete: false,
    lastLoginAt: '2026-09-09T09:00:00Z',
    lastName: 'Example',
    permissions: ['Exams.View'],
    roles: ['Nurse'],
    username: 'nurse',
  };
}

function setup(backingStore = new MemoryStorageBackend()): {
  logout: LocalLogout;
  tokens: TokenStorage;
  bootstrap: AuthSessionBootstrap;
  currentUserStore: CurrentUserStore;
  coordinator: RefreshCoordinator;
  httpMock: HttpTestingController;
  backingStore: MemoryStorageBackend;
} {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideApiConfig(),
      { provide: AUTH_SESSION_STORAGE, useValue: backingStore },
    ],
  });

  return {
    logout: TestBed.inject(LocalLogout),
    tokens: TestBed.inject(TokenStorage),
    bootstrap: TestBed.inject(AuthSessionBootstrap),
    currentUserStore: TestBed.inject(CurrentUserStore),
    coordinator: TestBed.inject(RefreshCoordinator),
    httpMock: TestBed.inject(HttpTestingController),
    backingStore,
  };
}

function seedTokenMaterial(tokens: TokenStorage): void {
  tokens.setTokenMaterial({
    accessToken: 'access-token-valid',
    accessTokenExpiresAt: '2026-09-09T11:00:00Z',
    refreshToken: 'refresh-token-valid',
  });
}

describe('LocalLogout', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('clears access token, access-token expiry, and refresh token', () => {
    const { logout, tokens, backingStore, httpMock } = setup();
    seedTokenMaterial(tokens);

    logout.logout();

    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(backingStore.values.has('np.auth.refreshToken')).toBe(false);
    httpMock.verify();
  });

  it('resolves AuthSessionBootstrap and CurrentUserStore to anonymous', () => {
    const { logout, tokens, bootstrap, currentUserStore, httpMock } = setup();
    seedTokenMaterial(tokens);

    bootstrap.bootstrap().subscribe();
    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());

    currentUserStore.hydrate().subscribe();
    httpMock.expectOne('/api/v1/me').flush(userDetailFixture());

    expect(bootstrap.state()).toBe('authenticated');
    expect(currentUserStore.status()).toBe('ready');

    logout.logout();

    expect(bootstrap.state()).toBe('anonymous');
    expect(currentUserStore.status()).toBe('anonymous');
    expect(currentUserStore.currentUser()).toBeUndefined();
    httpMock.verify();
  });

  it('invalidates active refresh before clearing tokens so late refresh success cannot resurrect auth state', () => {
    const { logout, tokens, coordinator, httpMock, bootstrap, currentUserStore } = setup();
    seedTokenMaterial(tokens);
    const refreshResults: AuthResult[] = [];
    const refreshErrors: unknown[] = [];

    coordinator.refresh().subscribe({
      next: (result) => {
        refreshResults.push(result);
      },
      error: (error: unknown) => {
        refreshErrors.push(error);
      },
    });

    logout.logout();

    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());

    expect(refreshResults).toEqual([]);
    expect(refreshErrors.length).toBe(1);
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(bootstrap.state()).toBe('anonymous');
    expect(currentUserStore.status()).toBe('anonymous');
    httpMock.verify();
  });

  it('is idempotent when called repeatedly', () => {
    const { logout, tokens, bootstrap, currentUserStore, httpMock } = setup();
    seedTokenMaterial(tokens);

    logout.logout();
    logout.logout();
    logout.logout();

    expect(tokens.getTokenState()).toEqual({
      accessToken: undefined,
      accessTokenExpiresAt: undefined,
      refreshToken: undefined,
    });
    expect(bootstrap.state()).toBe('anonymous');
    expect(currentUserStore.status()).toBe('anonymous');
    httpMock.expectNone('/api/v1/auth/logout');
    httpMock.expectNone('/api/v1/auth/revoke');
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();
  });

  it('does not perform backend logout, refresh, current-user, navigation, route, or UI behavior', () => {
    const { logout, tokens, httpMock } = setup();
    seedTokenMaterial(tokens);

    logout.logout();

    httpMock.expectNone('/api/v1/auth/logout');
    httpMock.expectNone('/api/v1/auth/revoke');
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();
  });

  it('keeps local logout free of direct storage, navigation, UI, generated, and server revocation behavior', () => {
    const source = readTextFile('src/app/core/auth/local-logout.ts');
    const lowered = source.toLowerCase();

    expect(source).toContain('RefreshCoordinator');
    expect(source).toContain('TokenStorage');
    expect(source).toContain('AuthSessionBootstrap');
    expect(source).toContain('CurrentUserStore');
    expect(source).toContain('invalidate');
    expect(source).toContain('clear');
    expect(source).toContain('resolveAnonymous');

    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('authtransport');
    expect(lowered).not.toContain('authorization');
    expect(lowered).not.toContain('bearer');
    expect(lowered).not.toContain('/me');
    expect(lowered).not.toContain('getcurrentuser');
    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('globalthis');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('navigate');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('component');
    expect(lowered).not.toContain('snackbar');
    expect(lowered).not.toContain('toast');
    expect(lowered).not.toContain('revoke');
    expect(lowered).not.toContain('revocation');
    expect(lowered).not.toContain('server-side');
  });
});
