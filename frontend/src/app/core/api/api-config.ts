import { provideApiConfiguration } from './generated/api-configuration';

export const API_PATH_PREFIX = '/api/v1';

export const DEFAULT_API_ORIGIN = '';

export interface PublicApiConfig {
  readonly apiOrigin?: string;
}

export function normalizeApiOrigin(origin?: string): string {
  if (origin === undefined || origin === null) {
    return '';
  }
  const trimmed = origin.trim();
  if (trimmed === '') {
    return '';
  }
  return trimmed.replace(/\/+$/, '');
}

export function resolveApiRootUrl(config?: PublicApiConfig): string {
  return normalizeApiOrigin(config?.apiOrigin);
}

export function buildApiPath(relativePath: string, config?: PublicApiConfig): string {
  const path = relativePath.startsWith('/') ? relativePath : `/${relativePath}`;
  return `${resolveApiRootUrl(config)}${API_PATH_PREFIX}${path}`;
}

export function provideApiConfig(config?: PublicApiConfig) {
  return provideApiConfiguration(resolveApiRootUrl(config));
}
