import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from '../../../core/api/generated/api-configuration';
import { publicRegisterNurse } from '../../../core/api/generated/fn/nursing-platform-web-api/public-register-nurse';
import type { PublicRegisterRequest } from '../../../core/api/generated/models/public-register-request';

@Injectable({
  providedIn: 'root',
})
export class RegisterNurseApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  register(request: PublicRegisterRequest): Observable<void> {
    return publicRegisterNurse(this.http, this.config.rootUrl, { body: request }).pipe(
      map(() => undefined),
    );
  }
}
