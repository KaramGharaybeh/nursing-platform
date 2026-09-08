import type { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_PATH_PREFIX, normalizeApiOrigin } from '../api/api-config';
import { ApiConfiguration } from '../api/generated/api-configuration';
import { TokenStorage } from './token-storage';

interface AnonymousRoute {
  readonly method: string;
  readonly path: string;
}

const ANONYMOUS_AUTH_ROUTES: readonly AnonymousRoute[] = [
  { method: 'POST', path: '/api/v1/auth/login' },
  { method: 'POST', path: '/api/v1/auth/refresh' },
  { method: 'POST', path: '/api/v1/auth/verify-email' },
  { method: 'POST', path: '/api/v1/auth/forgot-password' },
  { method: 'POST', path: '/api/v1/auth/reset-password' },
];

const OFFERS_LIST_PATH = '/api/v1/preparation-packages/offers';

function stripQueryAndFragment(value: string): string {
  const queryIndex = value.indexOf('?');
  const fragmentIndex = value.indexOf('#');
  let end = value.length;

  if (queryIndex !== -1) {
    end = Math.min(end, queryIndex);
  }

  if (fragmentIndex !== -1) {
    end = Math.min(end, fragmentIndex);
  }

  return value.slice(0, end);
}

function normalizePath(value: string): string {
  if (value.length > 1 && value.endsWith('/')) {
    return value.slice(0, -1);
  }

  return value;
}

function isAnonymousAuthRoute(method: string, path: string): boolean {
  return ANONYMOUS_AUTH_ROUTES.some(
    (route) => route.method === method && route.path === path,
  );
}

function isAnonymousOffersRoute(method: string, path: string): boolean {
  if (method !== 'GET') {
    return false;
  }

  if (path === OFFERS_LIST_PATH) {
    return true;
  }

  if (!path.startsWith(`${OFFERS_LIST_PATH}/`)) {
    return false;
  }

  const slug = path.slice(OFFERS_LIST_PATH.length + 1);

  return slug !== '' && !slug.includes('/');
}

function isAnonymousRequest(method: string, path: string): boolean {
  return isAnonymousAuthRoute(method, path) || isAnonymousOffersRoute(method, path);
}

function isApiPath(path: string): boolean {
  return path === API_PATH_PREFIX || path.startsWith(`${API_PATH_PREFIX}/`);
}

function relativeApiPath(url: string): string | undefined {
  if (!url.startsWith('/') || url.startsWith('//')) {
    return undefined;
  }

  const path = normalizePath(stripQueryAndFragment(url));

  return isApiPath(path) ? path : undefined;
}

function absoluteApiPath(url: string, configuredOrigin: string): string | undefined {
  if (configuredOrigin === '') {
    return undefined;
  }

  let parsed: URL;

  try {
    parsed = new URL(url);
  } catch {
    return undefined;
  }

  if (parsed.origin !== configuredOrigin) {
    return undefined;
  }

  const path = normalizePath(parsed.pathname);

  return isApiPath(path) ? path : undefined;
}

export const bearerInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.headers.has('Authorization')) {
    return next(req);
  }

  const tokenStorage = inject(TokenStorage);
  const accessToken = tokenStorage.getAccessToken();

  if (!accessToken) {
    return next(req);
  }

  const apiConfiguration = inject(ApiConfiguration);
  const configuredOrigin = normalizeApiOrigin(apiConfiguration.rootUrl);
  const isAbsolute = /^https?:\/\//i.test(req.url);
  const apiPath = isAbsolute
    ? absoluteApiPath(req.url, configuredOrigin)
    : relativeApiPath(req.url);

  if (apiPath === undefined) {
    return next(req);
  }

  if (isAnonymousRequest(req.method.toUpperCase(), apiPath)) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } }));
};
