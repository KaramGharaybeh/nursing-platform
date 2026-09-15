import { CANONICAL_ROUTE_IDS } from './canonical-routes';
import type { CanonicalRouteId } from './canonical-routes';

export type RouteAccessClassification = 'PUBLIC' | 'ENTRY' | 'AUTHENTICATED';

export const ENTRY_ROUTE_ID: CanonicalRouteId = 'ROOT_ENTRY';

export const PUBLIC_ROUTE_IDS: readonly CanonicalRouteId[] = Object.freeze([
  'AUTH_SIGN_IN',
  'AUTH_SIGN_UP',
  'AUTH_ROLE_SELECTION',
  'AUTH_REGISTER_NURSE',
  'AUTH_REGISTER_EMPLOYER',
  'AUTH_VERIFY_EMAIL_REQUEST',
  'AUTH_VERIFY_EMAIL_CONFIRM',
  'AUTH_FORGOT_PASSWORD',
  'AUTH_RESET_PASSWORD',
  'SYSTEM_SESSION_EXPIRED',
  'SYSTEM_ACCESS_DENIED',
  'PREPARATION_PACKAGES_OFFERS',
  'PREPARATION_PACKAGES_OFFER_DETAIL',
]);

export function classifyRoute(id: CanonicalRouteId): RouteAccessClassification {
  if (id === ENTRY_ROUTE_ID) {
    return 'ENTRY';
  }
  if (PUBLIC_ROUTE_IDS.includes(id)) {
    return 'PUBLIC';
  }
  return 'AUTHENTICATED';
}

export function isPublicRoute(id: CanonicalRouteId): boolean {
  return classifyRoute(id) === 'PUBLIC';
}

export function isAuthenticatedRoute(id: CanonicalRouteId): boolean {
  return classifyRoute(id) === 'AUTHENTICATED';
}

export const ROUTE_CLASSIFICATION_COUNTS: Readonly<{
  total: number;
  public: number;
  entry: number;
  authenticated: number;
}> = Object.freeze({
  total: CANONICAL_ROUTE_IDS.length,
  public: PUBLIC_ROUTE_IDS.length,
  entry: 1,
  authenticated: CANONICAL_ROUTE_IDS.length - PUBLIC_ROUTE_IDS.length - 1,
});
