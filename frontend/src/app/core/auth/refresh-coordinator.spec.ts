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
import type { AuthResult } from '../api/generated/models/auth-result';
import { AUTH_SESSION_STORAGE, TokenStorage } from './token-storage';
import type { TokenStorageBackend } from './token-storage';
import { RefreshCoordinator } from './refresh-coordinator';

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
  coordinator: RefreshCoordinator;
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
    coordinator: TestBed.inject(RefreshCoordinator),
    httpMock: TestBed.inject(HttpTestingController),
    tokens: TestBed.inject(TokenStorage),
    backingStore,
  };
}

function setupWithRefreshToken(storedRefreshToken: string): {
  coordinator: RefreshCoordinator;
  httpMock: HttpTestingController;
  tokens: TokenStorage;
  backingStore: MemoryStorageBackend;
} {
  const backingStore = new MemoryStorageBackend();
  backingStore.setItem('np.auth.refreshToken', storedRefreshToken);
  return setup(backingStore);
}

describe('refresh-coordinator', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('shares a single backend refresh across concurrent callers', () => {
    const { coordinator, httpMock } = setupWithRefreshToken('refresh-token-stored');
    const firstResults: AuthResult[] = [];
    const secondResults: AuthResult[] = [];

    coordinator.refresh().subscribe((result) => {
      firstResults.push(result);
    });
    coordinator.refresh().subscribe((result) => {
      secondResults.push(result);
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ refreshToken: 'refresh-token-stored' });

    request.flush(refreshSuccessFixture());
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(firstResults).toEqual([refreshSuccessFixture()]);
    expect(secondResults).toEqual([refreshSuccessFixture()]);
  });

  it('updates TokenStorage with returned token material before callers receive success', () => {
    const { coordinator, httpMock, tokens } = setupWithRefreshToken('refresh-token-stored');
    let storageStateAtRelease:
      | { accessToken: string | undefined; refreshToken: string | undefined }
      | undefined;

    coordinator.refresh().subscribe(() => {
      storageStateAtRelease = {
        accessToken: tokens.getAccessToken(),
        refreshToken: tokens.getRefreshToken(),
      };
    });

    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());
    httpMock.verify();

    expect(storageStateAtRelease).toEqual({
      accessToken: 'access-token-new',
      refreshToken: 'refresh-token-new',
    });
    expect(tokens.getTokenState()).toEqual({
      accessToken: 'access-token-new',
      accessTokenExpiresAt: '2026-09-08T11:00:00Z',
      refreshToken: 'refresh-token-new',
    });
  });

  it('errors all concurrent callers and clears token material on backend failure', () => {
    const { coordinator, httpMock, tokens, backingStore } =
      setupWithRefreshToken('refresh-token-stale');
    tokens.setTokenMaterial({
      accessToken: 'access-token-old',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-stale',
    });
    const firstErrors: unknown[] = [];
    const secondErrors: unknown[] = [];
    let firstNextCount = 0;
    let secondNextCount = 0;

    coordinator.refresh().subscribe({
      next: () => {
        firstNextCount += 1;
      },
      error: (error: unknown) => {
        firstErrors.push(error);
      },
    });
    coordinator.refresh().subscribe({
      next: () => {
        secondNextCount += 1;
      },
      error: (error: unknown) => {
        secondErrors.push(error);
      },
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');
    request.flush(
      { title: 'Unauthorized', status: 401, detail: 'Invalid refresh token.' },
      { status: 401, statusText: 'Unauthorized' },
    );
    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(firstNextCount).toBe(0);
    expect(secondNextCount).toBe(0);
    expect(firstErrors.length).toBe(1);
    expect(secondErrors.length).toBe(1);
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(backingStore.values.size).toBe(0);
  });

  it('clears token material and errors callers when the token-storage update fails', () => {
    const { coordinator, httpMock, tokens, backingStore } =
      setupWithRefreshToken('refresh-token-stored');
    tokens.setTokenMaterial = (): void => {
      throw new Error('Simulated storage failure.');
    };
    const results: AuthResult[] = [];
    const errors: unknown[] = [];

    coordinator.refresh().subscribe({
      next: (result) => {
        results.push(result);
      },
      error: (error: unknown) => {
        errors.push(error);
      },
    });

    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());
    httpMock.verify();

    expect(results).toEqual([]);
    expect(errors.length).toBe(1);
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(backingStore.values.size).toBe(0);
  });

  it('clears token material and errors without a backend call when no refresh token exists', () => {
    const { coordinator, httpMock, tokens, backingStore } = setup();
    tokens.setTokenMaterial({
      accessToken: 'access-token-stale',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-stale',
    });
    backingStore.values.delete('np.auth.refreshToken');
    const results: AuthResult[] = [];
    const errors: unknown[] = [];

    coordinator.refresh().subscribe({
      next: (result) => {
        results.push(result);
      },
      error: (error: unknown) => {
        errors.push(error);
      },
    });

    httpMock.expectNone('/api/v1/auth/refresh');
    httpMock.verify();

    expect(results).toEqual([]);
    expect(errors.length).toBe(1);
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBeUndefined();
    expect(backingStore.values.size).toBe(0);
  });

  it('starts a new backend request for a later refresh after success', () => {
    const { coordinator, httpMock } = setupWithRefreshToken('refresh-token-stored');
    const firstResults: AuthResult[] = [];
    const secondResults: AuthResult[] = [];

    coordinator.refresh().subscribe((result) => {
      firstResults.push(result);
    });
    httpMock
      .expectOne('/api/v1/auth/refresh')
      .flush(refreshSuccessFixture());
    httpMock.verify();

    expect(firstResults).toEqual([refreshSuccessFixture()]);

    coordinator.refresh().subscribe((result) => {
      secondResults.push(result);
    });

    const secondRequest = httpMock.expectOne('/api/v1/auth/refresh');

    expect(secondRequest.request.body).toEqual({ refreshToken: 'refresh-token-new' });

    secondRequest.flush({
      accessToken: 'access-token-newer',
      expiresAt: '2026-09-08T12:00:00Z',
      refreshToken: 'refresh-token-newer',
    });
    httpMock.verify();

    expect(secondResults).toEqual([
      {
        accessToken: 'access-token-newer',
        expiresAt: '2026-09-08T12:00:00Z',
        refreshToken: 'refresh-token-newer',
      },
    ]);
  });

  it('starts a new backend request for a later refresh after failure', () => {
    const { coordinator, httpMock, backingStore } =
      setupWithRefreshToken('refresh-token-stale');
    const errors: unknown[] = [];

    coordinator.refresh().subscribe({
      next: () => {
        throw new Error('Expected refresh to fail.');
      },
      error: (error: unknown) => {
        errors.push(error);
      },
    });

    httpMock.expectOne('/api/v1/auth/refresh').flush(
      { title: 'Unauthorized', status: 401, detail: 'Invalid refresh token.' },
      { status: 401, statusText: 'Unauthorized' },
    );
    httpMock.verify();

    expect(errors.length).toBe(1);

    backingStore.setItem('np.auth.refreshToken', 'refresh-token-reseeded');
    const results: AuthResult[] = [];

    coordinator.refresh().subscribe((result) => {
      results.push(result);
    });

    const secondRequest = httpMock.expectOne('/api/v1/auth/refresh');

    expect(secondRequest.request.body).toEqual({ refreshToken: 'refresh-token-reseeded' });

    secondRequest.flush(refreshSuccessFixture());
    httpMock.verify();

    expect(results).toEqual([refreshSuccessFixture()]);
  });

  it('invalidates an in-flight refresh so a late success neither repopulates storage nor reaches waiters', () => {
    const { coordinator, httpMock, tokens, backingStore } =
      setupWithRefreshToken('refresh-token-stored');
    const results: AuthResult[] = [];
    const errors: unknown[] = [];

    coordinator.refresh().subscribe({
      next: (result) => {
        results.push(result);
      },
      error: (error: unknown) => {
        errors.push(error);
      },
    });

    coordinator.invalidate();

    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());
    httpMock.verify();

    expect(results).toEqual([]);
    expect(errors.length).toBe(1);
    expect(tokens.getAccessToken()).toBeUndefined();
    expect(tokens.getAccessTokenExpiresAt()).toBeUndefined();
    expect(tokens.getRefreshToken()).toBe('refresh-token-stored');
    expect(backingStore.values.get('np.auth.refreshToken')).toBe('refresh-token-stored');
  });

  it('resets cleanly after invalidation so a later refresh starts a fresh backend request', () => {
    const { coordinator, httpMock, tokens } = setupWithRefreshToken('refresh-token-stored');
    const firstResults: AuthResult[] = [];
    const firstErrors: unknown[] = [];

    coordinator.refresh().subscribe({
      next: (result) => {
        firstResults.push(result);
      },
      error: (error: unknown) => {
        firstErrors.push(error);
      },
    });

    coordinator.invalidate();

    httpMock.expectOne('/api/v1/auth/refresh').flush(refreshSuccessFixture());
    httpMock.verify();

    expect(firstResults).toEqual([]);
    expect(firstErrors.length).toBe(1);

    const secondResults: AuthResult[] = [];
    coordinator.refresh().subscribe((result) => {
      secondResults.push(result);
    });

    const secondRequest = httpMock.expectOne('/api/v1/auth/refresh');
    expect(secondRequest.request.body).toEqual({ refreshToken: 'refresh-token-stored' });
    secondRequest.flush(refreshSuccessFixture());
    httpMock.verify();

    expect(secondResults).toEqual([refreshSuccessFixture()]);
    expect(tokens.getAccessToken()).toBe('access-token-new');
    expect(tokens.getRefreshToken()).toBe('refresh-token-new');
  });

  it('keeps the coordinator free of forbidden integrations and token disclosure paths', () => {
    const source = readTextFile('src/app/core/auth/refresh-coordinator.ts');
    const lowered = source.toLowerCase();

    expect(source).toContain('TokenStorage');
    expect(source).toContain('AuthTransport');
    expect(source).toContain('.refresh(');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('authorization');
    expect(lowered).not.toContain('bearer');
    expect(lowered).not.toContain('getcurrentuser');
    expect(lowered).not.toContain('/me');
    expect(lowered).not.toContain('globalthis');
    expect(lowered).not.toContain('interceptor');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('logout');
    expect(lowered).not.toContain('retry');
    expect(lowered).not.toContain('backoff');
    expect(lowered).not.toContain('console.');
  });
});
