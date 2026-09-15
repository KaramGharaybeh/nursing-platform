import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideApiConfig } from '../../../core/api/api-config';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';
import { RegisterEmployerApi } from './register-employer-api';

const VALID_REQUEST: PublicRegisterRequest = {
  email: 'employer@example.com',
  password: 'NewPass1x',
  username: 'employer',
};

describe('RegisterEmployerApi', () => {
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    TestBed.resetTestingModule();
  });

  it('posts the exact PublicRegisterRequest to the unified public sign-up endpoint', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        RegisterEmployerApi,
      ],
    });
    const api = TestBed.inject(RegisterEmployerApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.register({ ...VALID_REQUEST }).subscribe((result) => {
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/sign-up');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ ...VALID_REQUEST });
    request.flush(null);
  });

  it('submits exactly the three public fields with no forbidden role, accountType, token, or session fields', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        RegisterEmployerApi,
      ],
    });
    const api = TestBed.inject(RegisterEmployerApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.register({ ...VALID_REQUEST }).subscribe();

    const request = httpMock.expectOne('/api/v1/auth/sign-up');
    const body = request.request.body as Record<string, unknown>;
    expect(Object.keys(body).sort()).toEqual(['email', 'password', 'username']);
    expect(body).not.toHaveProperty('roleIds');
    expect(body).not.toHaveProperty('roles');
    expect(body).not.toHaveProperty('firstName');
    expect(body).not.toHaveProperty('lastName');
    expect(body).not.toHaveProperty('accountType');
    expect(body).not.toHaveProperty('actorType');
    expect(body).not.toHaveProperty('token');
    expect(body).not.toHaveProperty('refreshToken');
    expect(body).not.toHaveProperty('accessToken');
    request.flush(null);
  });

  it('returns void for the accepted response', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        RegisterEmployerApi,
      ],
    });
    const api = TestBed.inject(RegisterEmployerApi);
    const httpMock = TestBed.inject(HttpTestingController);

    let emitted = false;
    api.register({ ...VALID_REQUEST }).subscribe((result) => {
      emitted = true;
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/sign-up');
    request.flush(null, { status: 202, statusText: 'Accepted' });
    expect(emitted).toBe(true);
  });
});
