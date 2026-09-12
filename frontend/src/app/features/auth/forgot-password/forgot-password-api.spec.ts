import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideApiConfig } from '../../../core/api/api-config';
import { ForgotPasswordApi } from './forgot-password-api';

describe('ForgotPasswordApi', () => {
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    TestBed.resetTestingModule();
  });

  it('posts the exact ForgotPasswordRequest to the generated forgot-password endpoint', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        ForgotPasswordApi,
      ],
    });
    const api = TestBed.inject(ForgotPasswordApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.requestPasswordReset({ email: 'nurse@example.com' }).subscribe((result) => {
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/forgot-password');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ email: 'nurse@example.com' });
    request.flush(null);
  });
});
