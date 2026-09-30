import type { CanonicalRouteId } from './canonical-routes';
import { evaluateRoutePermission, getRoutePermissionPolicy } from './route-permission-policy';

export type NavigationUserState =
  | { readonly status: 'idle' }
  | { readonly status: 'loading' }
  | { readonly status: 'anonymous' }
  | { readonly status: 'unavailable' }
  | {
      readonly status: 'ready';
      readonly roles: readonly string[];
      readonly permissions: readonly string[];
    };

export type NavigationEligibilityKind = 'eligible' | 'ineligible' | 'unresolved' | 'unsupported';

export interface NavigationEligibility {
  readonly routeId: CanonicalRouteId;
  readonly kind: NavigationEligibilityKind;
}

export function getNavigationEligibility(
  routeId: CanonicalRouteId,
  user: NavigationUserState,
): NavigationEligibility {
  if (user.status !== 'ready') {
    return { routeId, kind: 'unresolved' };
  }
  let policy;
  try {
    policy = getRoutePermissionPolicy(routeId);
  } catch {
    return { routeId, kind: 'unsupported' };
  }
  const allowed = evaluateRoutePermission(policy, {
    roles: user.roles,
    permissions: user.permissions,
  });
  return { routeId, kind: allowed ? 'eligible' : 'ineligible' };
}

export interface ResolvedNavigationCandidates {
  readonly status: 'resolved';
  readonly eligible: readonly CanonicalRouteId[];
}

export interface UnresolvedNavigationCandidates {
  readonly status: 'unresolved';
  readonly candidates: readonly CanonicalRouteId[];
}

export type FilteredNavigationCandidates =
  | ResolvedNavigationCandidates
  | UnresolvedNavigationCandidates;

export function filterEligibleNavigationCandidates(
  candidates: readonly CanonicalRouteId[],
  user: NavigationUserState,
): FilteredNavigationCandidates {
  if (user.status !== 'ready') {
    return { status: 'unresolved', candidates: [...candidates] };
  }
  const eligible: CanonicalRouteId[] = [];
  for (const routeId of candidates) {
    if (getNavigationEligibility(routeId, user).kind === 'eligible') {
      eligible.push(routeId);
    }
  }
  return { status: 'resolved', eligible };
}
