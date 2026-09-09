import { ApplicationInitStatus } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { provideApiConfig } from '../api/api-config';
import { AUTH_SESSION_STORAGE, TokenStorage } from './token-storage';
import type { TokenStorageBackend } from './token-storage';
import {
  AuthSessionBootstrap,
  provideAuthSessionBootstrap,
} from './auth-session-bootstrap';
import type { AuthResult } from '../api/generated/models/auth-result';

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
  tokens: TokenStorage;
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
    bootstrap: TestBed.inject(AuthSessionBootstrap),
    httpMock: TestBed.inject(HttpTestingController),
    tokens: TestBed.inject(TokenStorage),
    backingStore,
  };
}

describe('auth-session-bootstrap', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('starts in the unknown initializing state before bootstrap completes', () => {
    const { bootstrap, httpMock } = setup();

    expect(bootstrap.state()).toBe('initializing');

    httpMock.verify();
  });

  it('resolves anonymous without network calls when no refresh token is stored', () => {
    const { bootstrap, httpMock } = setup();
    let actual: string | undefined;

    bootstrap.bootstrap().subscribe((state) => {
      actual = state;
    });

    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(actual).toBe('anonymous');
    expect(bootstrap.state()).toBe('anonymous');
  });

  it('registers bootstrap as an awaited Angular application initializer', async () => {
    const backingStore = new MemoryStorageBackend();
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        { provide: AUTH_SESSION_STORAGE, useValue: backingStore },
        provideAuthSessionBootstrap(),
      ],
    });

    const bootstrap = TestBed.inject(AuthSessionBootstrap);
    const httpMock = TestBed.inject(HttpTestingController);
    const initStatus = TestBed.inject(ApplicationInitStatus);

    (initStatus as unknown as { runInitializers(): void }).runInitializers();
    await initStatus.donePromise;

    expect(bootstrap.state()).toBe('anonymous');
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();
  });

  it('performs exactly one startup refresh attempt with the stored refresh token', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock } = setup(backingStore);
    let actual: string | undefined;

    bootstrap.bootstrap().subscribe((state) => {
      actual = state;
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ refreshToken: 'refresh-token-stored' });

    request.flush(refreshSuccessFixture());

    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(actual).toBe('authenticated');
    expect(bootstrap.state()).toBe('authenticated');
  });

  it('persists refreshed token material through TokenStorage on success', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock, tokens } = setup(backingStore);
    let actual: string | undefined;

    bootstrap.bootstrap().subscribe((state) => {
      actual = state;
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');
    request.flush(refreshSuccessFixture());
    httpMock.verify();

    expect(actual).toBe('authenticated');
    expect(tokens.getAccessToken()).toBe('access-token-new');
    expect(tokens.getAccessTokenExpiresAt()).toBe('2026-09-08T11:00:00Z');
    expect(tokens.getRefreshToken()).toBe('refresh-token-new');
    expect(tokens.getTokenState()).toEqual({
      accessToken: 'access-token-new',
      accessTokenExpiresAt: '2026-09-08T11:00:00Z',
      refreshToken: 'refresh-token-new',
    });
  });

  it('clears token material and resolves anonymous on invalid refresh', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-invalid');
    const { bootstrap, httpMock, tokens, backingStore: store } = setup(backingStore);
    let actual: string | undefined;

    bootstrap.bootstrap().subscribe((state) => {
      actual = state;
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    request.flush(
      { title: 'Unauthorized', status: 401, detail: 'Invalid refresh token.' },
      { status: 401, statusText: 'Unauthorized' },
    );
    httpMock.verify();

    expect(actual).toBe('anonymous');
    expect(bootstrap.state()).toBe('anonymous');
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(store.values.size).toBe(0);
  });

  it('clears token material and resolves anonymous on transient refresh failure', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock, tokens, backingStore: store } = setup(backingStore);
    let actual: string | undefined;

    bootstrap.bootstrap().subscribe((state) => {
      actual = state;
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    request.flush(
      { title: 'Service Unavailable', status: 503, detail: 'Try again later.' },
      { status: 503, statusText: 'Service Unavailable' },
    );
    httpMock.verify();

    expect(actual).toBe('anonymous');
    expect(bootstrap.state()).toBe('anonymous');
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(store.values.size).toBe(0);
  });

  it('clears token material and resolves anonymous on network refresh failure', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock, tokens, backingStore: store } = setup(backingStore);
    let actual: string | undefined;

    bootstrap.bootstrap().subscribe((state) => {
      actual = state;
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    request.error(new ProgressEvent('error'));
    httpMock.verify();

    expect(actual).toBe('anonymous');
    expect(bootstrap.state()).toBe('anonymous');
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(store.values.size).toBe(0);
  });

  it('never calls the current-user endpoint during bootstrap', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock } = setup(backingStore);

    bootstrap.bootstrap().subscribe();

    const request = httpMock.expectOne('/api/v1/auth/refresh');
    request.flush(refreshSuccessFixture());

    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(bootstrap.state()).toBe('authenticated');
  });

  it('resolveAnonymous places bootstrap session state in anonymous without network calls', () => {
    const { bootstrap, httpMock, backingStore } = setup();

    expect(bootstrap.state()).toBe('initializing');

    bootstrap.resolveAnonymous();

    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(bootstrap.state()).toBe('anonymous');
    expect(backingStore.values.size).toBe(0);
  });

  it('resolveAnonymous resets an authenticated bootstrap to anonymous without further network calls', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    const { bootstrap, httpMock, backingStore: store } = setup(backingStore);

    bootstrap.bootstrap().subscribe();

    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());
    httpMock.verify();
    expect(bootstrap.state()).toBe('authenticated');

    bootstrap.resolveAnonymous();

    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(bootstrap.state()).toBe('anonymous');
    expect(store.values.get('np.auth.refreshToken')).toBe('refresh-token-new');
  });

  it('keeps bootstrap free of current-user, header, storage, and refresh-coordination behavior', () => {
    const source = readTextFile('src/app/core/auth/auth-session-bootstrap.ts');
    const lowered = source.toLowerCase();

    expect(source).toContain('TokenStorage');
    expect(source).toContain('AuthTransport');
    expect(source).toContain('.refresh(');
    expect(source).toContain('provideAppInitializer');
    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('authorization');
    expect(lowered).not.toContain('bearer');
    expect(lowered).not.toContain('getcurrentuser');
    expect(lowered).not.toContain('/me');
    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('globalthis');
    expect(lowered).not.toContain('interceptor');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('logout');
    expect(lowered).not.toContain('sharereplay');
    expect(lowered).not.toContain('shareplay');
    expect(lowered).not.toContain('singleflight');
    expect(lowered).not.toContain('single-flight');
    expect(lowered).not.toContain('replay');
    expect(lowered).not.toContain('queue');
  });
});
