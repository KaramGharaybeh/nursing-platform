import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideApiConfig } from '../../../core/api/api-config';
import { ResetPasswordApi } from './reset-password-api';

describe('ResetPasswordApi', () => {
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    TestBed.resetTestingModule();
  });

  it('posts the exact ResetPasswordRequest to the generated reset-password endpoint', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfig(),
        ResetPasswordApi,
      ],
    });
    const api = TestBed.inject(ResetPasswordApi);
    const httpMock = TestBed.inject(HttpTestingController);

    api.resetPassword({ email: 'nurse@example.com', token: 'raw-token', newPassword: 'NewPass1x' }).subscribe((result) => {
      expect(result).toBeUndefined();
    });

    const request = httpMock.expectOne('/api/v1/auth/reset-password');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'nurse@example.com',
      token: 'raw-token',
      newPassword: 'NewPass1x',
    });
    request.flush(null);
  });
});
