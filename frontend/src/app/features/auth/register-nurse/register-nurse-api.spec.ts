import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideApiConfig } from '../../../core/api/api-config';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';
import { RegisterNurseApi } from './register-nurse-api';

const VALID_REQUEST: PublicRegisterRequest = {
  email: 'nurse@example.com',
  password: 'NewPass1x',
  firstName: 'Amal',
  lastName: 'Haddad',
};

describe('RegisterNurseApi', () => {
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    TestBed.resetTestingModule();
  });

  it('posts the exact PublicRegisterRequest to the generated public-register-nurse endpoint', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        RegisterNurseApi,
      ],
    });
    const api = TestBed.inject(RegisterNurseApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.register({ ...VALID_REQUEST }).subscribe((result) => {
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/register/nurse');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ ...VALID_REQUEST });
    request.flush(null);
  });

  it('submits exactly the four public fields with no forbidden role, company, token, or session fields', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        RegisterNurseApi,
      ],
    });
    const api = TestBed.inject(RegisterNurseApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.register({ ...VALID_REQUEST }).subscribe();

    const request = httpMock.expectOne('/api/v1/auth/register/nurse');
    const body = request.request.body as Record<string, unknown>;
    expect(Object.keys(body).sort()).toEqual(['email', 'firstName', 'lastName', 'password']);
    expect(body).not.toHaveProperty('roleIds');
    expect(body).not.toHaveProperty('roles');
    expect(body).not.toHaveProperty('company');
    expect(body).not.toHaveProperty('organization');
    expect(body).not.toHaveProperty('token');
    expect(body).not.toHaveProperty('refreshToken');
    expect(body).not.toHaveProperty('accessToken');
    request.flush(null);
  });

  it('returns void for the accepted 202 response', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        RegisterNurseApi,
      ],
    });
    const api = TestBed.inject(RegisterNurseApi);
    const httpMock = TestBed.inject(HttpTestingController);

    let emitted = false;
    api.register({ ...VALID_REQUEST }).subscribe((result) => {
      emitted = true;
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/register/nurse');
    request.flush(null, { status: 202, statusText: 'Accepted' });
    expect(emitted).toBe(true);
  });
});
