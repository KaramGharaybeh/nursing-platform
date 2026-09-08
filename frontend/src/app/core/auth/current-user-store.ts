import { inject, Injectable, provideAppInitializer, signal } from '@angular/core';
import type { Signal } from '@angular/core';
import type { Observable } from 'rxjs';
import { catchError, firstValueFrom, map, of } from 'rxjs';
import { AuthTransport } from '../api/auth-transport';
import { AuthSessionBootstrap } from './auth-session-bootstrap';
import { TokenStorage } from './token-storage';
import { adaptUserDetailToCurrentUser } from './current-user';
import type { CurrentUser, CurrentUserStatus } from './current-user';

function failureStatus(error: unknown): number | undefined {
  const status = (error as { status?: unknown }).status;
  return typeof status === 'number' ? status : undefined;
}

@Injectable({
  providedIn: 'root',
})
export class CurrentUserStore {
  private readonly transport = inject(AuthTransport);
  private readonly tokens = inject(TokenStorage);
  private readonly statusSignal = signal<CurrentUserStatus>('idle');
  private readonly userSignal = signal<CurrentUser | undefined>(undefined);

  readonly status: Signal<CurrentUserStatus> = this.statusSignal.asReadonly();
  readonly currentUser: Signal<CurrentUser | undefined> = this.userSignal.asReadonly();

  hydrate(): Observable<CurrentUserStatus> {
    this.userSignal.set(undefined);
    this.statusSignal.set('loading');

    return this.transport.getCurrentUser().pipe(
      map((dto): CurrentUserStatus => {
        this.userSignal.set(adaptUserDetailToCurrentUser(dto));
        this.statusSignal.set('ready');
        return 'ready';
      }),
      catchError((error: unknown) => {
        if (failureStatus(error) === 401) {
          this.tokens.clear();
          this.userSignal.set(undefined);
          this.statusSignal.set('anonymous');
          return of<CurrentUserStatus>('anonymous');
        }

        this.userSignal.set(undefined);
        this.statusSignal.set('unavailable');
        return of<CurrentUserStatus>('unavailable');
      }),
    );
  }

  resolveAnonymous(): void {
    this.userSignal.set(undefined);
    this.statusSignal.set('anonymous');
  }
}

export function provideCurrentUserHydration() {
  return provideAppInitializer(() => {
    const bootstrap = inject(AuthSessionBootstrap);
    const store = inject(CurrentUserStore);

    return firstValueFrom(bootstrap.bootstrap()).then((status) => {
      if (status === 'authenticated') {
        return firstValueFrom(store.hydrate());
      }

      store.resolveAnonymous();
      return undefined;
    });
  });
}
