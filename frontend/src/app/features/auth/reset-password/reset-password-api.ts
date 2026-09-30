import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from '../../../core/api/generated/api-configuration';
import { resetPassword } from '../../../core/api/generated/fn/nursing-platform-web-api/reset-password';
import type { ResetPasswordRequest } from '../../../core/api/generated/models/reset-password-request';

@Injectable({
  providedIn: 'root',
})
export class ResetPasswordApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return resetPassword(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
