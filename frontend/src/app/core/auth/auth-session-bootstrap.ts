import { Injectable, inject, provideAppInitializer, signal } from '@angular/core';
import type { Signal } from '@angular/core';
import type { Observable } from 'rxjs';
import { catchError, firstValueFrom, map, of } from 'rxjs';
import { AuthTransport } from '../api/auth-transport';
import type { AuthResult } from '../api/generated/models/auth-result';
import { TokenStorage } from './token-storage';

export type TokenSessionStatus = 'initializing' | 'authenticated' | 'anonymous';

@Injectable({
  providedIn: 'root',
})
export class AuthSessionBootstrap {
  private readonly transport = inject(AuthTransport);
  private readonly tokens = inject(TokenStorage);
  private readonly statusSignal = signal<TokenSessionStatus>('initializing');

  readonly state: Signal<TokenSessionStatus> = this.statusSignal.asReadonly();

  resolveAnonymous(): void {
    this.statusSignal.set('anonymous');
  }

  establishAuthenticatedSession(result: AuthResult): TokenSessionStatus {
    this.tokens.setTokenMaterial({
      accessToken: result.accessToken,
      accessTokenExpiresAt: result.expiresAt,
      refreshToken: result.refreshToken,
    });
    this.statusSignal.set('authenticated');
    return 'authenticated';
  }

  bootstrap(): Observable<TokenSessionStatus> {
    const refreshToken = this.tokens.getRefreshToken();

    if (!refreshToken) {
      this.statusSignal.set('anonymous');
      return of<TokenSessionStatus>('anonymous');
    }

    return this.transport.refresh({ refreshToken }).pipe(
      map((result): TokenSessionStatus => {
        return this.establishAuthenticatedSession(result);
      }),
      catchError(() => {
        this.tokens.clear();
        this.statusSignal.set('anonymous');
        return of<TokenSessionStatus>('anonymous');
      }),
    );
  }
}

export function provideAuthSessionBootstrap() {
  return provideAppInitializer(() => firstValueFrom(
    inject(AuthSessionBootstrap).bootstrap(),
  ));
}
