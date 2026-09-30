import { inject, Injectable } from '@angular/core';
import { AuthSessionBootstrap } from './auth-session-bootstrap';
import { CurrentUserStore } from './current-user-store';
import { RefreshCoordinator } from './refresh-coordinator';
import { TokenStorage } from './token-storage';

@Injectable({
  providedIn: 'root',
})
export class LocalLogout {
  private readonly refreshCoordinator = inject(RefreshCoordinator);
  private readonly tokenStorage = inject(TokenStorage);
  private readonly authSessionBootstrap = inject(AuthSessionBootstrap);
  private readonly currentUserStore = inject(CurrentUserStore);

  logout(): void {
    this.refreshCoordinator.invalidate();
    this.tokenStorage.clear();
    this.authSessionBootstrap.resolveAnonymous();
    this.currentUserStore.resolveAnonymous();
  }
}
