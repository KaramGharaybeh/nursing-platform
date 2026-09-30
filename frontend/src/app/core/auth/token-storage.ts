import { inject, Injectable, InjectionToken } from '@angular/core';

const REFRESH_TOKEN_STORAGE_KEY = 'np.auth.refreshToken';

export interface TokenStorageBackend {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
}

export interface TokenMaterial {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
}

export interface TokenState {
  accessToken: string | undefined;
  accessTokenExpiresAt: string | undefined;
  refreshToken: string | undefined;
}

export const AUTH_SESSION_STORAGE = new InjectionToken<TokenStorageBackend | null>(
  'AUTH_SESSION_STORAGE',
  {
    providedIn: 'root',
    factory: () => globalThis.sessionStorage ?? null,
  },
);

@Injectable({
  providedIn: 'root',
})
export class TokenStorage {
  private readonly storage = inject(AUTH_SESSION_STORAGE);
  private accessToken: string | undefined;
  private accessTokenExpiresAt: string | undefined;

  getTokenState(): TokenState {
    return {
      accessToken: this.accessToken,
      accessTokenExpiresAt: this.accessTokenExpiresAt,
      refreshToken: this.getRefreshToken(),
    };
  }

  getAccessToken(): string | undefined {
    return this.accessToken;
  }

  getAccessTokenExpiresAt(): string | undefined {
    return this.accessTokenExpiresAt;
  }

  getRefreshToken(): string | undefined {
    return this.storage?.getItem(REFRESH_TOKEN_STORAGE_KEY) ?? undefined;
  }

  setTokenMaterial(tokenMaterial: TokenMaterial): void {
    this.accessToken = tokenMaterial.accessToken;
    this.accessTokenExpiresAt = tokenMaterial.accessTokenExpiresAt;
    this.storage?.setItem(REFRESH_TOKEN_STORAGE_KEY, tokenMaterial.refreshToken);
  }

  clear(): void {
    this.accessToken = undefined;
    this.accessTokenExpiresAt = undefined;
    this.storage?.removeItem(REFRESH_TOKEN_STORAGE_KEY);
  }
}
