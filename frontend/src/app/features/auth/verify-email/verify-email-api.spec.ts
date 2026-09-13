import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideApiConfig } from '../../../core/api/api-config';
import { VerifyEmailApi } from './verify-email-api';

describe('VerifyEmailApi', () => {
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    TestBed.resetTestingModule();
  });

  it('posts the exact VerifyEmailRequest to the generated verify-email endpoint', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig(), VerifyEmailApi],
    });
    const api = TestBed.inject(VerifyEmailApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.verifyEmail({ token: 'opaque-token' }).subscribe((result) => {
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/verify-email');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ token: 'opaque-token' });
    request.flush(null);
  });
});
