import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
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
import { AUTH_SESSION_STORAGE, TokenStorage } from './token-storage';
import type { TokenStorageBackend } from './token-storage';
import { bearerInterceptor } from './bearer-interceptor';

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

const ACCESS_TOKEN = 'access-token-live';
const ACCESS_EXPIRY = '2026-09-08T11:00:00Z';
const REFRESH_TOKEN = 'refresh-token-live';

function setup(options?: { apiOrigin?: string; withToken?: boolean }): {
  http: HttpClient;
  httpMock: HttpTestingController;
} {
  const backingStore = new MemoryStorageBackend();
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(withInterceptors([bearerInterceptor])),
      provideHttpClientTesting(),
      options?.apiOrigin === undefined
        ? provideApiConfig()
        : provideApiConfig({ apiOrigin: options.apiOrigin }),
      { provide: AUTH_SESSION_STORAGE, useValue: backingStore },
    ],
  });

  if (options?.withToken !== false) {
    const tokens = TestBed.inject(TokenStorage);
    tokens.setTokenMaterial({
      accessToken: ACCESS_TOKEN,
      accessTokenExpiresAt: ACCESS_EXPIRY,
      refreshToken: REFRESH_TOKEN,
    });
  }

  return {
    http: TestBed.inject(HttpClient),
    httpMock: TestBed.inject(HttpTestingController),
  };
}

describe('bearer-interceptor', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('attaches Bearer for a relative protected API request when a token exists', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.get('/api/v1/exams').subscribe();
    const request = httpMock.expectOne('/api/v1/exams');
    httpMock.verify();

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${ACCESS_TOKEN}`);
  });

  it('attaches Bearer for an absolute URL targeting the configured API origin', () => {
    const { http, httpMock } = setup({
      apiOrigin: 'http://localhost:5167',
      withToken: true,
    });

    http.get('http://localhost:5167/api/v1/me').subscribe();
    const request = httpMock.expectOne('http://localhost:5167/api/v1/me');
    httpMock.verify();

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${ACCESS_TOKEN}`);
  });

  it('leaves an arbitrary external absolute URL containing /api/v1 untouched', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.get('https://cdn.example.com/api/v1/me').subscribe();
    const request = httpMock.expectOne('https://cdn.example.com/api/v1/me');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('leaves a protected API request unchanged when no access token exists', () => {
    const { http, httpMock } = setup({ withToken: false });

    http.get('/api/v1/me').subscribe();
    const request = httpMock.expectOne('/api/v1/me');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous POST /api/v1/auth/login', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.post('/api/v1/auth/login', { email: 'nurse@example.com' }).subscribe();
    const request = httpMock.expectOne('/api/v1/auth/login');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous POST /api/v1/auth/refresh', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.post('/api/v1/auth/refresh', { refreshToken: REFRESH_TOKEN }).subscribe();
    const request = httpMock.expectOne('/api/v1/auth/refresh');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous POST /api/v1/auth/verify-email', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.post('/api/v1/auth/verify-email', { token: 'verify-token' }).subscribe();
    const request = httpMock.expectOne('/api/v1/auth/verify-email');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous POST /api/v1/auth/forgot-password', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.post('/api/v1/auth/forgot-password', { email: 'nurse@example.com' }).subscribe();
    const request = httpMock.expectOne('/api/v1/auth/forgot-password');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous POST /api/v1/auth/reset-password', () => {
    const { http, httpMock } = setup({ withToken: true });

    http
      .post('/api/v1/auth/reset-password', { token: 'reset-token' })
      .subscribe();
    const request = httpMock.expectOne('/api/v1/auth/reset-password');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous GET /api/v1/preparation-packages/offers', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.get('/api/v1/preparation-packages/offers').subscribe();
    const request = httpMock.expectOne('/api/v1/preparation-packages/offers');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('does not attach Bearer for anonymous GET /api/v1/preparation-packages/offers/{slug}', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.get('/api/v1/preparation-packages/offers/some-package').subscribe();
    const request = httpMock.expectOne(
      '/api/v1/preparation-packages/offers/some-package',
    );
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('attaches Bearer for POST /api/v1/auth/register when a token exists', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.post('/api/v1/auth/register', { email: 'nurse@example.com' }).subscribe();
    const request = httpMock.expectOne('/api/v1/auth/register');
    httpMock.verify();

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${ACCESS_TOKEN}`);
  });

  it('attaches Bearer for POST /api/v1/auth/send-verification-email when a token exists', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.post('/api/v1/auth/send-verification-email', {}).subscribe();
    const request = httpMock.expectOne('/api/v1/auth/send-verification-email');
    httpMock.verify();

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${ACCESS_TOKEN}`);
  });

  it('attaches Bearer for GET /api/v1/me when a token exists', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.get('/api/v1/me').subscribe();
    const request = httpMock.expectOne('/api/v1/me');
    httpMock.verify();

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${ACCESS_TOKEN}`);
  });

  it('preserves an existing Authorization header exactly', () => {
    const { http, httpMock } = setup({ withToken: true });

    http
      .get('/api/v1/me', { headers: { Authorization: 'Custom existing-scheme' } })
      .subscribe();
    const request = httpMock.expectOne('/api/v1/me');
    httpMock.verify();

    expect(request.request.headers.get('Authorization')).toBe('Custom existing-scheme');
  });

  it('leaves non-API requests untouched when a token exists', () => {
    const { http, httpMock } = setup({ withToken: true });

    http.get('/assets/config.json').subscribe();
    const request = httpMock.expectOne('/assets/config.json');
    httpMock.verify();

    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('keeps the interceptor free of direct browser storage access', () => {
    const lowered = readTextFile('src/app/core/auth/bearer-interceptor.ts').toLowerCase();

    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('globalthis');
  });

  it('keeps the interceptor free of refresh coordination, replay, logout, and error handling', () => {
    const source = readTextFile('src/app/core/auth/bearer-interceptor.ts');
    const lowered = source.toLowerCase();

    expect(source).toContain('TokenStorage');
    expect(source).toContain('Authorization');
    expect(source).toContain('Bearer');
    expect(lowered).not.toContain('refreshcoordinator');
    expect(lowered).not.toContain('authtransport');
    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('logout');
    expect(lowered).not.toContain('retry');
    expect(lowered).not.toContain('replay');
    expect(lowered).not.toContain('backoff');
    expect(lowered).not.toContain('httperrorresponse');
    expect(lowered).not.toContain('catcherror');
    expect(lowered).not.toContain('401');
    expect(lowered).not.toContain('console.');
  });

  it('registers the bearer interceptor through the application HTTP provider', () => {
    const source = readTextFile('src/app/app.config.ts');

    expect(source).toContain('bearerInterceptor');
    expect(source).toContain('withInterceptors');
    expect(source).toContain('provideHttpClient');
  });
});
