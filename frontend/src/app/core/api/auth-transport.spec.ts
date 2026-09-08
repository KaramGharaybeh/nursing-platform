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
import { provideApiConfig } from './api-config';
import { AuthTransport } from './auth-transport';
import type { AuthResult } from './generated/models/auth-result';
import type { LoginCommand } from './generated/models/login-command';
import type { RotateRefreshTokenCommand } from './generated/models/rotate-refresh-token-command';
import type { UserDetailDto } from './generated/models/user-detail-dto';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function authResultFixture(): AuthResult {
  return {
    accessToken: 'access-token-1',
    expiresAt: '2026-09-08T10:00:00Z',
    refreshToken: 'refresh-token-1',
  };
}

function userDetailFixture(): UserDetailDto {
  return {
    createdAt: '2026-09-08T10:00:00Z',
    email: 'nurse@example.com',
    emailVerified: true,
    firstName: 'Nurse',
    id: 'user-1',
    isActive: true,
    lastName: 'Example',
    permissions: ['Exams.View'],
    roles: ['Nurse'],
  };
}

describe('auth-transport', () => {
  let transport: AuthTransport;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    transport = TestBed.inject(AuthTransport);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('delegates login to generated POST /api/v1/auth/login with the LoginCommand body', () => {
    const command: LoginCommand = { email: 'nurse@example.com', password: 'secret-password' };
    const expected = authResultFixture();
    let actual: AuthResult | undefined;

    transport.login(command).subscribe((result) => {
      actual = result;
    });

    const request = httpMock.expectOne('/api/v1/auth/login');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(command);
    expect(request.request.headers.has('Authorization')).toBe(false);

    request.flush(expected);

    expect(actual).toEqual(expected);
  });

  it('delegates refresh to generated POST /api/v1/auth/refresh with the RotateRefreshTokenCommand body', () => {
    const command: RotateRefreshTokenCommand = { refreshToken: 'refresh-token-1' };
    const expected = authResultFixture();
    let actual: AuthResult | undefined;

    transport.refresh(command).subscribe((result) => {
      actual = result;
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(command);
    expect(request.request.headers.has('Authorization')).toBe(false);

    request.flush(expected);

    expect(actual).toEqual(expected);
  });

  it('delegates current-user to generated GET /api/v1/me and returns the typed UserDetailDto', () => {
    const expected = userDetailFixture();
    let actual: UserDetailDto | undefined;

    transport.getCurrentUser().subscribe((result) => {
      actual = result;
    });

    const request = httpMock.expectOne('/api/v1/me');

    expect(request.request.method).toBe('GET');
    expect(request.request.headers.has('Authorization')).toBe(false);

    request.flush(expected);

    expect(actual).toEqual(expected);
  });

  it('propagates login errors unchanged without interpreting them', () => {
    const command: LoginCommand = { email: 'nurse@example.com', password: 'wrong-password' };
    let status: number | undefined;
    let errorBody: unknown;

    transport.login(command).subscribe({
      next: () => {
        throw new Error('Expected login to fail.');
      },
      error: (error: { status?: number; error?: unknown }) => {
        status = error.status;
        errorBody = error.error;
      },
    });

    const request = httpMock.expectOne('/api/v1/auth/login');

    request.flush(
      { title: 'Unauthorized', status: 401, detail: 'Invalid credentials.' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(status).toBe(401);
    expect(errorBody).toEqual({ title: 'Unauthorized', status: 401, detail: 'Invalid credentials.' });
  });

  it('propagates refresh errors unchanged without interpreting them', () => {
    const command: RotateRefreshTokenCommand = { refreshToken: 'stale-refresh-token' };
    let status: number | undefined;

    transport.refresh(command).subscribe({
      next: () => {
        throw new Error('Expected refresh to fail.');
      },
      error: (error: { status?: number }) => {
        status = error.status;
      },
    });

    const request = httpMock.expectOne('/api/v1/auth/refresh');

    request.flush(
      { title: 'Unauthorized', status: 401, detail: 'Invalid refresh token.' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(status).toBe(401);
  });

  it('propagates current-user errors unchanged without interpreting them', () => {
    let status: number | undefined;

    transport.getCurrentUser().subscribe({
      next: () => {
        throw new Error('Expected current-user to fail.');
      },
      error: (error: { status?: number }) => {
        status = error.status;
      },
    });

    const request = httpMock.expectOne('/api/v1/me');

    request.flush(
      { title: 'Unauthorized', status: 401, detail: 'Missing access token.' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(status).toBe(401);
  });

  it('delegates through the generated typed functions with the configured ApiConfiguration root URL', () => {
    const source = readTextFile('src/app/core/api/auth-transport.ts');

    expect(source).toContain('generated/fn/nursing-platform-web-api/login');
    expect(source).toContain('generated/fn/nursing-platform-web-api/refresh-token');
    expect(source).toContain('generated/fn/nursing-platform-web-api/get-current-user');
    expect(source).toContain('ApiConfiguration');
    expect(source).toContain('rootUrl');
  });

  it('keeps the transport free of token storage, header injection, and session behavior', () => {
    const source = readTextFile('src/app/core/api/auth-transport.ts').toLowerCase();

    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('authorization');
    expect(source).not.toContain('bearer');
    expect(source).not.toContain('interceptor');
    expect(source).not.toContain('bootstrap');
    expect(source).not.toContain('logout');
    expect(source).not.toContain('router');
    expect(source).not.toContain('guard');
  });
});
