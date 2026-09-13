import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from '../../../core/api/generated/api-configuration';
import { verifyEmail } from '../../../core/api/generated/fn/nursing-platform-web-api/verify-email';
import type { VerifyEmailRequest } from '../../../core/api/generated/models/verify-email-request';

@Injectable({
  providedIn: 'root',
})
export class VerifyEmailApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  verifyEmail(request: VerifyEmailRequest): Observable<void> {
    return verifyEmail(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
