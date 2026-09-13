import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from '../../../core/api/generated/api-configuration';
import { publicRegisterEmployer } from '../../../core/api/generated/fn/nursing-platform-web-api/public-register-employer';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';

@Injectable({
  providedIn: 'root',
})
export class RegisterEmployerApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  register(request: PublicRegisterRequest): Observable<void> {
    return publicRegisterEmployer(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
