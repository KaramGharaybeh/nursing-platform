import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from '../../../core/api/generated/api-configuration';
import { forgotPassword } from '../../../core/api/generated/fn/nursing-platform-web-api/forgot-password';
import type { ForgotPasswordRequest } from '../../../core/api/generated/models/forgot-password-request';

@Injectable({
  providedIn: 'root',
})
export class ForgotPasswordApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  requestPasswordReset(request: ForgotPasswordRequest): Observable<void> {
    return forgotPassword(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
