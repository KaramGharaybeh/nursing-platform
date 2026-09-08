import { inject, Injectable } from '@angular/core';
import { defer, Observable, throwError } from 'rxjs';
import { catchError, finalize, shareReplay, tap } from 'rxjs/operators';
import { AuthTransport } from '../api/auth-transport';
import type { AuthResult } from '../api/generated/models/auth-result';
import { TokenStorage } from './token-storage';

@Injectable({
  providedIn: 'root',
})
export class RefreshCoordinator {
  private readonly authTransport = inject(AuthTransport);
  private readonly tokenStorage = inject(TokenStorage);
  private inFlightRefresh$: Observable<AuthResult> | undefined;

  refresh(): Observable<AuthResult> {
    this.inFlightRefresh$ ??= this.createRefreshRequest();

    return this.inFlightRefresh$;
  }

  private createRefreshRequest(): Observable<AuthResult> {
    return defer(() => {
      const refreshToken = this.tokenStorage.getRefreshToken();

      if (!refreshToken) {
        return throwError(() => new Error('Refresh token is not available.'));
      }

      return this.authTransport.refresh({ refreshToken });
    }).pipe(
      tap((result) => {
        this.tokenStorage.setTokenMaterial({
          accessToken: result.accessToken,
          accessTokenExpiresAt: result.expiresAt,
          refreshToken: result.refreshToken,
        });
      }),
      catchError((error: unknown) => {
        this.tokenStorage.clear();

        return throwError(() => error);
      }),
      finalize(() => {
        this.inFlightRefresh$ = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
  }
}
