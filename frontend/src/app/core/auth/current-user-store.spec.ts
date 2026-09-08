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
import type { UserDetailDto } from '../api/generated/models/user-detail-dto';
import type { AuthResult } from '../api/generated/models/auth-result';
import { AUTH_SESSION_STORAGE, TokenStorage } from './token-storage';
import type { TokenStorageBackend } from './token-storage';
import { AuthSessionBootstrap } from './auth-session-bootstrap';
import { CurrentUserStore, provideCurrentUserHydration } from './current-user-store';
import { adaptUserDetailToCurrentUser } from './current-user';
import type { CurrentUser } from './current-user';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

class MemoryStorageBackend implements TokenStorageBackend {
  readonly values = new Map<string, string>();
  removedKeys: string[] = [];
  writtenKeys: string[] = [];

  getItem(key: string): string | null {
    return this.values.get(key) ?? null;
  }

  setItem(key: string, value: string): void {
    this.values.set(key, value);
    this.writtenKeys.push(key);
  }

  removeItem(key: string): void {
    this.values.delete(key);
    this.removedKeys.push(key);
  }
}

function userDetailFixture(): UserDetailDto {
  return {
    createdAt: '2026-09-08T10:00:00Z',
    email: 'nurse@example.com',
    emailVerified: true,
    firstName: 'Nurse',
    id: 'user-1',
    isActive: true,
    lastLoginAt: '2026-09-08T09:00:00Z',
    lastName: 'Example',
    permissions: ['Exams.View', 'Users.Create'],
    roles: ['Admin', 'Nurse'],
  };
}

function refreshSuccessFixture(): AuthResult {
  return {
    accessToken: 'access-token-new',
    expiresAt: '2026-09-08T11:00:00Z',
    refreshToken: 'refresh-token-new',
  };
}

function setup(backingStore = new MemoryStorageBackend()): {
  store: CurrentUserStore;
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
    store: TestBed.inject(CurrentUserStore),
    httpMock: TestBed.inject(HttpTestingController),
    tokens: TestBed.inject(TokenStorage),
    backingStore,
  };
}

function seedAuthenticatedTokens(tokens: TokenStorage): void {
  tokens.setTokenMaterial({
    accessToken: 'access-token-valid',
    accessTokenExpiresAt: '2026-09-08T11:00:00Z',
    refreshToken: 'refresh-token-valid',
  });
}

describe('current-user-store', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('starts in the deterministic idle state with no exposed current user', () => {
    const { store, httpMock } = setup();

    expect(store.status()).toBe('idle');
    expect(store.currentUser()).toBeUndefined();

    httpMock.verify();
  });

  it('exposes the approved current-user states only', () => {
    const { store, httpMock } = setup();

    expect(store.status()).toBe('idle');

    store.resolveAnonymous();
    expect(store.status()).toBe('anonymous');

    httpMock.verify();
  });

  it('resolveAnonymous clears the exposed user without calling /me', () => {
    const { store, httpMock } = setup();
    let actual: string | undefined;

    store.hydrate().subscribe((status) => {
      actual = status;
    });

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(userDetailFixture());
    expect(store.status()).toBe('ready');
    expect(store.currentUser()).toBeDefined();

    store.resolveAnonymous();

    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(actual).toBe('ready');
    expect(store.status()).toBe('anonymous');
    expect(store.currentUser()).toBeUndefined();
  });

  it('hydrate calls GET /api/v1/me through AuthTransport without manual Authorization behavior', () => {
    const { store, httpMock } = setup();
    let actual: CurrentUser | undefined;

    store.hydrate().subscribe(() => {
      actual = store.currentUser();
    });

    expect(store.status()).toBe('loading');

    const request = httpMock.expectOne('/api/v1/me');

    expect(request.request.method).toBe('GET');
    expect(request.request.headers.has('Authorization')).toBe(false);

    request.flush(userDetailFixture());
    httpMock.verify();

    expect(store.status()).toBe('ready');
    expect(actual).toBeDefined();
  });

  it('hydrate maps UserDetailDto to the frontend-owned CurrentUser and resolves ready', () => {
    const { store, httpMock } = setup();

    store.hydrate().subscribe();

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(userDetailFixture());
    httpMock.verify();

    expect(store.status()).toBe('ready');

    const user = store.currentUser();
    expect(user).toEqual({
      id: 'user-1',
      email: 'nurse@example.com',
      firstName: 'Nurse',
      lastName: 'Example',
      isActive: true,
      emailVerified: true,
      roles: ['Admin', 'Nurse'],
      permissions: ['Exams.View', 'Users.Create'],
      createdAt: '2026-09-08T10:00:00Z',
      lastLoginAt: '2026-09-08T09:00:00Z',
    });
  });

  it('hydrate exposes an immutable CurrentUser', () => {
    const { store, httpMock } = setup();

    store.hydrate().subscribe();

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(userDetailFixture());
    httpMock.verify();

    const user = store.currentUser();
    expect(user).toBeDefined();
    expect(Object.isFrozen(user)).toBe(true);
    expect(Object.isFrozen(user?.roles)).toBe(true);
    expect(Object.isFrozen(user?.permissions)).toBe(true);
  });

  it('hydrate maps a missing lastLoginAt to undefined', () => {
    const { store, httpMock } = setup();
    const dto = userDetailFixture();
    delete dto.lastLoginAt;

    store.hydrate().subscribe();

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(dto);
    httpMock.verify();

    expect(store.status()).toBe('ready');
    expect(store.currentUser()?.lastLoginAt).toBeUndefined();
  });

  it('hydrate clears stale exposed CurrentUser when a new hydration attempt starts', () => {
    const { store, httpMock } = setup();

    store.hydrate().subscribe();

    const firstRequest = httpMock.expectOne('/api/v1/me');
    firstRequest.flush(userDetailFixture());
    expect(store.status()).toBe('ready');
    expect(store.currentUser()?.id).toBe('user-1');

    store.hydrate().subscribe();

    expect(store.status()).toBe('loading');
    expect(store.currentUser()).toBeUndefined();

    const secondRequest = httpMock.expectOne('/api/v1/me');
    secondRequest.flush(userDetailFixture());
    httpMock.verify();

    expect(store.status()).toBe('ready');
    expect(store.currentUser()?.id).toBe('user-1');
  });

  it('hydrate does not mutate the generated UserDetailDto', () => {
    const { store, httpMock } = setup();
    const dto = userDetailFixture();
    const snapshot = JSON.stringify(dto);
    Object.freeze(dto);
    Object.freeze(dto.roles);
    Object.freeze(dto.permissions);

    store.hydrate().subscribe();

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(dto);
    httpMock.verify();

    expect(store.status()).toBe('ready');
    expect(JSON.stringify(dto)).toBe(snapshot);
    expect(dto.roles).toEqual(['Admin', 'Nurse']);
    expect(dto.permissions).toEqual(['Exams.View', 'Users.Create']);
  });

  it('hydrate preserves backend-projected roles and permissions exactly', () => {
    const { store, httpMock } = setup();
    const dto = userDetailFixture();
    dto.roles = ['Nurse', 'Admin', 'Nurse'];
    dto.permissions = ['Users.Create', 'Exams.View'];

    store.hydrate().subscribe();

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(dto);
    httpMock.verify();

    expect(store.currentUser()?.roles).toEqual(['Nurse', 'Admin', 'Nurse']);
    expect(store.currentUser()?.permissions).toEqual(['Users.Create', 'Exams.View']);
  });

  it('hydrate never writes CurrentUser data to browser storage', () => {
    const { store, httpMock, backingStore } = setup();

    store.hydrate().subscribe();

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(userDetailFixture());
    httpMock.verify();

    expect(store.status()).toBe('ready');
    expect(backingStore.writtenKeys).toEqual([]);
    expect(backingStore.values.size).toBe(0);
  });

  it('/me 401 clears token state via TokenStorage, clears the user, and resolves anonymous', () => {
    const { store, httpMock, tokens, backingStore } = setup();
    seedAuthenticatedTokens(tokens);
    backingStore.writtenKeys = [];
    let actual: string | undefined;

    store.hydrate().subscribe((status) => {
      actual = status;
    });

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(
      { title: 'Unauthorized', status: 401, detail: 'Missing access token.' },
      { status: 401, statusText: 'Unauthorized' },
    );
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(actual).toBe('anonymous');
    expect(store.status()).toBe('anonymous');
    expect(store.currentUser()).toBeUndefined();
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(backingStore.values.size).toBe(0);
  });

  it('network failure preserves token state and resolves unavailable', () => {
    const { store, httpMock, tokens, backingStore } = setup();
    seedAuthenticatedTokens(tokens);
    let actual: string | undefined;

    store.hydrate().subscribe((status) => {
      actual = status;
    });

    const request = httpMock.expectOne('/api/v1/me');
    request.error(new ProgressEvent('error'));
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(actual).toBe('unavailable');
    expect(store.status()).toBe('unavailable');
    expect(store.currentUser()).toBeUndefined();
    expect(tokens.getTokenState()).toEqual({
      accessToken: 'access-token-valid',
      accessTokenExpiresAt: '2026-09-08T11:00:00Z',
      refreshToken: 'refresh-token-valid',
    });
    expect(backingStore.values.get('np.auth.refreshToken')).toBe('refresh-token-valid');
  });

  it('5xx failure preserves token state and resolves unavailable', () => {
    const { store, httpMock, tokens } = setup();
    seedAuthenticatedTokens(tokens);
    let actual: string | undefined;

    store.hydrate().subscribe((status) => {
      actual = status;
    });

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(
      { title: 'Service Unavailable', status: 503, detail: 'Try again later.' },
      { status: 503, statusText: 'Service Unavailable' },
    );
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(actual).toBe('unavailable');
    expect(store.status()).toBe('unavailable');
    expect(store.currentUser()).toBeUndefined();
    expect(tokens.getAccessToken()).toBe('access-token-valid');
    expect(tokens.getRefreshToken()).toBe('refresh-token-valid');
  });

  it('unexpected 403 does not clear token state and resolves unavailable', () => {
    const { store, httpMock, tokens } = setup();
    seedAuthenticatedTokens(tokens);
    let actual: string | undefined;

    store.hydrate().subscribe((status) => {
      actual = status;
    });

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(
      { title: 'Forbidden', status: 403, detail: 'Forbidden.' },
      { status: 403, statusText: 'Forbidden' },
    );
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(actual).toBe('unavailable');
    expect(store.status()).toBe('unavailable');
    expect(store.currentUser()).toBeUndefined();
    expect(tokens.getAccessToken()).toBe('access-token-valid');
    expect(tokens.getRefreshToken()).toBe('refresh-token-valid');
  });

  it('unexpected 404 does not clear token state and resolves unavailable', () => {
    const { store, httpMock, tokens } = setup();
    seedAuthenticatedTokens(tokens);
    let actual: string | undefined;

    store.hydrate().subscribe((status) => {
      actual = status;
    });

    const request = httpMock.expectOne('/api/v1/me');
    request.flush(
      { title: 'Not Found', status: 404, detail: 'Not found.' },
      { status: 404, statusText: 'Not Found' },
    );
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(actual).toBe('unavailable');
    expect(store.status()).toBe('unavailable');
    expect(store.currentUser()).toBeUndefined();
    expect(tokens.getAccessToken()).toBe('access-token-valid');
    expect(tokens.getRefreshToken()).toBe('refresh-token-valid');
  });

  it('hydrate performs no automatic retry; a later explicit hydrate may retry', () => {
    const { store, httpMock, tokens } = setup();
    seedAuthenticatedTokens(tokens);

    store.hydrate().subscribe();

    const first = httpMock.expectOne('/api/v1/me');
    first.error(new ProgressEvent('error'));
    httpMock.expectNone('/api/v1/me');
    expect(store.status()).toBe('unavailable');

    store.hydrate().subscribe();

    const second = httpMock.expectOne('/api/v1/me');
    second.flush(userDetailFixture());
    httpMock.verify();

    expect(store.status()).toBe('ready');
  });

  it('registers startup hydration after bootstrap: anonymous bootstrap resolves anonymous without /me', async () => {
    const backingStore = new MemoryStorageBackend();
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        { provide: AUTH_SESSION_STORAGE, useValue: backingStore },
        provideCurrentUserHydration(),
      ],
    });

    const bootstrap = TestBed.inject(AuthSessionBootstrap);
    const store = TestBed.inject(CurrentUserStore);
    const httpMock = TestBed.inject(HttpTestingController);
    const initStatus = TestBed.inject(ApplicationInitStatus);

    (initStatus as unknown as { runInitializers(): void }).runInitializers();
    await initStatus.donePromise;

    expect(bootstrap.state()).toBe('anonymous');
    expect(store.status()).toBe('anonymous');
    expect(store.currentUser()).toBeUndefined();
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.expectNone('/api/v1/me');
    httpMock.verify();
  });

  it('registers startup hydration after bootstrap: authenticated bootstrap triggers exactly one /me hydration', async () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-token-stored');
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        { provide: AUTH_SESSION_STORAGE, useValue: backingStore },
        provideCurrentUserHydration(),
      ],
    });

    const bootstrap = TestBed.inject(AuthSessionBootstrap);
    const store = TestBed.inject(CurrentUserStore);
    const httpMock = TestBed.inject(HttpTestingController);
    const initStatus = TestBed.inject(ApplicationInitStatus);

    (initStatus as unknown as { runInitializers(): void }).runInitializers();

    const refreshRequest = httpMock.expectOne('/api/v1/auth/refresh');
    expect(refreshRequest.request.body).toEqual({ refreshToken: 'refresh-token-stored' });
    refreshRequest.flush(refreshSuccessFixture());

    await new Promise((resolve) => setTimeout(resolve, 0));

    const meRequest = httpMock.expectOne('/api/v1/me');
    expect(meRequest.request.method).toBe('GET');
    expect(meRequest.request.headers.has('Authorization')).toBe(false);
    meRequest.flush(userDetailFixture());

    await initStatus.donePromise;

    httpMock.expectNone('/api/v1/me');
    httpMock.verify();

    expect(bootstrap.state()).toBe('authenticated');
    expect(store.status()).toBe('ready');
    expect(store.currentUser()?.id).toBe('user-1');
  });

  it('adapts UserDetailDto without leaking the generated type as session state', () => {
    const user: CurrentUser = adaptUserDetailToCurrentUser(userDetailFixture());

    expect(user.id).toBe('user-1');
    expect(user.email).toBe('nurse@example.com');
    expect(user.firstName).toBe('Nurse');
    expect(user.lastName).toBe('Example');
    expect(user.isActive).toBe(true);
    expect(user.emailVerified).toBe(true);
    expect(user.roles).toEqual(['Admin', 'Nurse']);
    expect(user.permissions).toEqual(['Exams.View', 'Users.Create']);
    expect(user.createdAt).toBe('2026-09-08T10:00:00Z');
    expect(user.lastLoginAt).toBe('2026-09-08T09:00:00Z');
    expect(Object.isFrozen(user)).toBe(true);
  });

  it('keeps current-user hydration free of refresh, bearer, storage, guard, and UI behavior', () => {
    const storeSource = readTextFile('src/app/core/auth/current-user-store.ts');
    const modelSource = readTextFile('src/app/core/auth/current-user.ts');
    const storeLowered = storeSource.toLowerCase();
    const modelLowered = modelSource.toLowerCase();

    expect(storeSource).toContain('AuthTransport');
    expect(storeSource).toContain('getCurrentUser');
    expect(storeSource).toContain('TokenStorage');
    expect(storeSource).toContain('AuthSessionBootstrap');
    expect(storeSource).toContain('provideAppInitializer');
    expect(storeSource).toContain('adaptUserDetailToCurrentUser');
    expect(modelSource).toContain('UserDetailDto');
    expect(modelSource).toContain('adaptDto');
    expect(modelSource).toContain('normalizeNullable');

    for (const source of [storeLowered, modelLowered]) {
      expect(source).not.toContain('httpclient');
      expect(source).not.toContain('authorization');
      expect(source).not.toContain('bearer');
      expect(source).not.toContain('refresh');
      expect(source).not.toContain('interceptor');
      expect(source).not.toContain('sessionstorage');
      expect(source).not.toContain('localstorage');
      expect(source).not.toContain('globalthis');
      expect(source).not.toContain('router');
      expect(source).not.toContain('navigate');
      expect(source).not.toContain('guard');
      expect(source).not.toContain('logout');
      expect(source).not.toContain('snackbar');
      expect(source).not.toContain('toast');
      expect(source).not.toContain('retry');
      expect(source).not.toContain('backoff');
    }

    expect(storeLowered).not.toContain('/me');
    expect(modelLowered).not.toContain('/me');
  });
});
