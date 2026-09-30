import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { updateCurrentUserProfile } from './generated/fn/nursing-platform-web-api/update-current-user-profile';
import type { UpdateCurrentUserProfileRequest } from './generated/models/update-current-user-profile-request';
import type { UpdateCurrentUserProfileResponse } from './generated/models/update-current-user-profile-response';

@Injectable({ providedIn: 'root' })
export class ProfileApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  updateCurrentUserProfile(
    request: UpdateCurrentUserProfileRequest,
  ): Observable<UpdateCurrentUserProfileResponse> {
    return updateCurrentUserProfile(this.http, this.config.rootUrl, { body: request }).pipe(
      map((response) => response.body),
    );
  }
}
