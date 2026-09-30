import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { publicSignUp } from './generated/fn/nursing-platform-web-api/public-sign-up';
import type { PublicRegisterRequest } from './generated/models/public-register-request';

@Injectable({ providedIn: 'root' })
export class SignUpApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  signUp(request: PublicRegisterRequest): Observable<void> {
    return publicSignUp(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
