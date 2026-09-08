import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getCurrentUser } from './generated/fn/nursing-platform-web-api/get-current-user';
import { login } from './generated/fn/nursing-platform-web-api/login';
import { refreshToken } from './generated/fn/nursing-platform-web-api/refresh-token';
import type { AuthResult } from './generated/models/auth-result';
import type { LoginCommand } from './generated/models/login-command';
import type { RotateRefreshTokenCommand } from './generated/models/rotate-refresh-token-command';
import type { UserDetailDto } from './generated/models/user-detail-dto';

@Injectable({
  providedIn: 'root',
})
export class AuthTransport {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  login(command: LoginCommand): Observable<AuthResult> {
    return login(this.http, this.config.rootUrl, { body: command }).pipe(
      map((response) => response.body),
    );
  }

  refresh(command: RotateRefreshTokenCommand): Observable<AuthResult> {
    return refreshToken(this.http, this.config.rootUrl, { body: command }).pipe(
      map((response) => response.body),
    );
  }

  getCurrentUser(): Observable<UserDetailDto> {
    return getCurrentUser(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }
}
