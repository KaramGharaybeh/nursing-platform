import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from '../../../core/api/generated/api-configuration';
import { publicSignUp } from '../../../core/api/generated/fn/nursing-platform-web-api/public-sign-up';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';

/**
 * @deprecated Legacy employer-specific registration adapter.
 * Use the unified sign-up flow at /auth/sign-up instead.
 * This adapter now delegates to the unified publicSignUp endpoint.
 */
@Injectable({
  providedIn: 'root',
})
export class RegisterEmployerApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  register(request: PublicRegisterRequest): Observable<void> {
    return publicSignUp(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
