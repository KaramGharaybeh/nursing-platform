import { TestBed } from '@angular/core/testing';
import {
  AUTH_SESSION_STORAGE,
  TokenStorage,
  type TokenStorageBackend,
} from './token-storage';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';

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

function configureTokenStorage(backingStore = new MemoryStorageBackend()): {
  storage: TokenStorage;
  backingStore: MemoryStorageBackend;
} {
  TestBed.configureTestingModule({
    providers: [{ provide: AUTH_SESSION_STORAGE, useValue: backingStore }],
  });

  return {
    storage: TestBed.inject(TokenStorage),
    backingStore,
  };
}

describe('token-storage', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
  });

  it('starts empty when no refresh token is present in session storage', () => {
    const { storage } = configureTokenStorage();

    expect(storage.getAccessToken()).toBeUndefined();
    expect(storage.getAccessTokenExpiresAt()).toBeUndefined();
    expect(storage.getRefreshToken()).toBeUndefined();
    expect(storage.getTokenState()).toEqual({
      accessToken: undefined,
      accessTokenExpiresAt: undefined,
      refreshToken: undefined,
    });
  });

  it('starts with only the refresh token when session storage already contains one', () => {
    const backingStore = new MemoryStorageBackend();
    backingStore.setItem('np.auth.refreshToken', 'refresh-after-reload');

    const { storage } = configureTokenStorage(backingStore);

    expect(storage.getAccessToken()).toBeUndefined();
    expect(storage.getAccessTokenExpiresAt()).toBeUndefined();
    expect(storage.getRefreshToken()).toBe('refresh-after-reload');
  });

  it('stores access token and expiry in memory while persisting only the refresh token', () => {
    const { storage, backingStore } = configureTokenStorage();

    storage.setTokenMaterial({
      accessToken: 'access-token-one',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-one',
    });

    expect(storage.getAccessToken()).toBe('access-token-one');
    expect(storage.getAccessTokenExpiresAt()).toBe('2026-09-08T10:00:00Z');
    expect(storage.getRefreshToken()).toBe('refresh-token-one');
    expect([...backingStore.values.entries()]).toEqual([
      ['np.auth.refreshToken', 'refresh-token-one'],
    ]);
  });

  it('clears memory token material and the persisted refresh token', () => {
    const { storage, backingStore } = configureTokenStorage();

    storage.setTokenMaterial({
      accessToken: 'access-token-one',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-one',
    });

    storage.clear();

    expect(storage.getAccessToken()).toBeUndefined();
    expect(storage.getAccessTokenExpiresAt()).toBeUndefined();
    expect(storage.getRefreshToken()).toBeUndefined();
    expect(backingStore.values.size).toBe(0);
  });

  it('restores only the refresh token for a new instance sharing the same session storage', () => {
    const backingStore = new MemoryStorageBackend();
    const first = configureTokenStorage(backingStore).storage;

    first.setTokenMaterial({
      accessToken: 'access-token-one',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-one',
    });

    TestBed.resetTestingModule();
    const second = configureTokenStorage(backingStore).storage;

    expect(second.getAccessToken()).toBeUndefined();
    expect(second.getAccessTokenExpiresAt()).toBeUndefined();
    expect(second.getRefreshToken()).toBe('refresh-token-one');
  });

  it('safely handles a missing session storage backend', () => {
    TestBed.configureTestingModule({
      providers: [{ provide: AUTH_SESSION_STORAGE, useValue: null }],
    });

    const storage = TestBed.inject(TokenStorage);

    expect(storage.getRefreshToken()).toBeUndefined();

    storage.setTokenMaterial({
      accessToken: 'access-token-one',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-one',
    });

    expect(storage.getAccessToken()).toBe('access-token-one');
    expect(storage.getAccessTokenExpiresAt()).toBe('2026-09-08T10:00:00Z');
    expect(storage.getRefreshToken()).toBeUndefined();

    storage.clear();

    expect(storage.getAccessToken()).toBeUndefined();
    expect(storage.getAccessTokenExpiresAt()).toBeUndefined();
  });

  it('replaces only approved token material', () => {
    const { storage, backingStore } = configureTokenStorage();

    storage.setTokenMaterial({
      accessToken: 'access-token-one',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-one',
    });
    storage.setTokenMaterial({
      accessToken: 'access-token-two',
      accessTokenExpiresAt: '2026-09-08T11:00:00Z',
      refreshToken: 'refresh-token-two',
    });

    expect(storage.getTokenState()).toEqual({
      accessToken: 'access-token-two',
      accessTokenExpiresAt: '2026-09-08T11:00:00Z',
      refreshToken: 'refresh-token-two',
    });
    expect([...backingStore.values.entries()]).toEqual([
      ['np.auth.refreshToken', 'refresh-token-two'],
    ]);
  });

  it('does not persist AuthResult or unrelated DTO data', () => {
    const { storage, backingStore } = configureTokenStorage();

    storage.setTokenMaterial({
      accessToken: 'access-token-one',
      accessTokenExpiresAt: '2026-09-08T10:00:00Z',
      refreshToken: 'refresh-token-one',
    });

    expect([...backingStore.values.keys()]).toEqual(['np.auth.refreshToken']);
    expect([...backingStore.values.values()]).not.toContain('access-token-one');
    expect([...backingStore.values.values()]).not.toContain('2026-09-08T10:00:00Z');
    expect(Object.keys(storage.getTokenState()).sort()).toEqual([
      'accessToken',
      'accessTokenExpiresAt',
      'refreshToken',
    ]);
  });

  it('keeps token storage free of forbidden integrations and token disclosure paths', () => {
    const source = readTextFile('src/app/core/auth/token-storage.ts').toLowerCase();

    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('authtransport');
    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('authorization');
    expect(source).not.toContain('bearer');
    expect(source).not.toContain('interceptor');
    expect(source).not.toContain('bootstrap');
    expect(source).not.toContain('logout');
    expect(source).not.toContain('router');
    expect(source).not.toContain('guard');
    expect(source).not.toContain('console.');
    expect(source).not.toContain('throw new');
    expect(source).not.toContain('url');
    expect(source).not.toContain('analytics');
    expect(source).not.toContain('authresult');
    expect(source).not.toContain('problemdetails');
    expect(source).not.toContain('userdetail');
    expect(source).not.toContain('roles');
    expect(source).not.toContain('permissions');
  });
});
