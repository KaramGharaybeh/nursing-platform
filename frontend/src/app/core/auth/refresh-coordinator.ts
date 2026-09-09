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
  private refreshGeneration = 0;

  refresh(): Observable<AuthResult> {
    this.inFlightRefresh$ ??= this.createRefreshRequest(this.refreshGeneration);

    return this.inFlightRefresh$;
  }

  invalidate(): void {
    this.refreshGeneration += 1;
    this.inFlightRefresh$ = undefined;
  }

  private createRefreshRequest(generation: number): Observable<AuthResult> {
    const request$: Observable<AuthResult> = defer(() => {
      const refreshToken = this.tokenStorage.getRefreshToken();

      if (!refreshToken) {
        return throwError(() => new Error('Refresh token is not available.'));
      }

      return this.authTransport.refresh({ refreshToken });
    }).pipe(
      tap((result) => {
        if (generation !== this.refreshGeneration) {
          throw new Error('Refresh was invalidated.');
        }
        this.tokenStorage.setTokenMaterial({
          accessToken: result.accessToken,
          accessTokenExpiresAt: result.expiresAt,
          refreshToken: result.refreshToken,
        });
      }),
      catchError((error: unknown) => {
        if (generation !== this.refreshGeneration) {
          return throwError(() => error);
        }
        this.tokenStorage.clear();

        return throwError(() => error);
      }),
      finalize(() => {
        if (this.inFlightRefresh$ === request$) {
          this.inFlightRefresh$ = undefined;
        }
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    return request$;
  }
}
