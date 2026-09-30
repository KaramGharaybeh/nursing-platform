import type { Route, Routes } from '@angular/router';
import { canonicalRoutePath } from '../routing/canonical-routes';
import type { CanonicalRouteId } from '../routing/canonical-routes';
import { filterEligibleNavigationCandidates } from '../routing/navigation-permission-policy';
import type { NavigationUserState } from '../routing/navigation-permission-policy';

export interface PrimaryNavigationItem {
  readonly routeId: CanonicalRouteId;
  readonly label: string;
  readonly path: string;
  readonly active: boolean;
}

interface PrimaryNavigationCandidate {
  readonly routeId: CanonicalRouteId;
  readonly label: string;
  readonly acceptedRoles: readonly string[];
  readonly activeFamily: readonly CanonicalRouteId[];
}

const PRIMARY_NAVIGATION_CANDIDATES: readonly PrimaryNavigationCandidate[] = Object.freeze([
  {
    routeId: 'EXAMS_CATALOG',
    label: 'Exams',
    acceptedRoles: ['Nurse'],
    activeFamily: [
      'EXAMS_CATALOG',
      'EXAMS_DETAIL',
      'EXAMS_INSTRUCTIONS',
      'EXAMS_SESSION',
      'EXAMS_RESULT',
      'EXAMS_ANALYTICS',
      'EXAMS_REVIEW',
      'EXAMS_HISTORY',
    ],
  },
  {
    routeId: 'PREPARATION_PACKAGES_ENTITLEMENTS',
    label: 'Preparation Packages',
    acceptedRoles: ['Nurse'],
    activeFamily: [
      'PREPARATION_PACKAGES_ENTITLEMENTS',
      'PREPARATION_PACKAGES_ENTITLEMENT_DETAIL',
      'PREPARATION_PACKAGES_PRACTICE',
      'PREPARATION_PACKAGES_REPORT',
    ],
  },
  {
    routeId: 'COMMERCE_PRODUCTS',
    label: 'Products',
    acceptedRoles: ['Nurse'],
    activeFamily: ['COMMERCE_PRODUCTS', 'COMMERCE_PRODUCT_DETAIL'],
  },
  {
    routeId: 'NURSE_PROFILE_OVERVIEW',
    label: 'Profile',
    acceptedRoles: ['Nurse'],
    activeFamily: [
      'NURSE_PROFILE_OVERVIEW',
      'NURSE_PROFILE_PERSONAL_INFORMATION',
      'NURSE_PROFILE_EXPERIENCE',
      'NURSE_PROFILE_EDUCATION',
      'NURSE_PROFILE_CERTIFICATES',
      'NURSE_PROFILE_SKILLS',
      'NURSE_PROFILE_LANGUAGES',
      'NURSE_PROFILE_CV',
      'NURSE_CONTACT_REQUESTS',
    ],
  },
  {
    routeId: 'EMPLOYER_HOME',
    label: 'Employer',
    acceptedRoles: ['Employer'],
    activeFamily: ['EMPLOYER_HOME'],
  },
  {
    routeId: 'EMPLOYER_CANDIDATES',
    label: 'Candidates',
    acceptedRoles: ['Employer'],
    activeFamily: ['EMPLOYER_CANDIDATES'],
  },
  {
    routeId: 'EMPLOYER_REQUESTS',
    label: 'Requests',
    acceptedRoles: ['Employer'],
    activeFamily: ['EMPLOYER_REQUESTS', 'EMPLOYER_REQUEST_DETAIL'],
  },
  {
    routeId: 'ADMIN_ENTRY',
    label: 'Admin',
    acceptedRoles: ['Admin'],
    activeFamily: ['ADMIN_ENTRY'],
  },
  {
    routeId: 'ADMIN_USERS',
    label: 'Users',
    acceptedRoles: ['Admin'],
    activeFamily: ['ADMIN_USERS', 'ADMIN_USER_DETAIL'],
  },
]);

export interface PrimaryNavigationOptions {
  readonly mountedRouteIds: readonly CanonicalRouteId[];
  readonly user: NavigationUserState;
  readonly currentRouteId?: CanonicalRouteId;
}

export function getPrimaryNavigationItems(options: PrimaryNavigationOptions): readonly PrimaryNavigationItem[] {
  const mountedRouteIds = new Set(options.mountedRouteIds);
  const roleEligibleCandidates = options.user.status === 'ready'
    ? PRIMARY_NAVIGATION_CANDIDATES.filter((candidate) => {
      return candidate.acceptedRoles.some((role) => options.user.status === 'ready' && options.user.roles.includes(role));
    })
    : [];
  const concreteCandidates = roleEligibleCandidates.filter((candidate) => {
    return mountedRouteIds.has(candidate.routeId) && !canonicalRoutePath(candidate.routeId).includes(':');
  });
  const filtered = filterEligibleNavigationCandidates(
    concreteCandidates.map((candidate) => candidate.routeId),
    options.user,
  );
  if (filtered.status !== 'resolved') {
    return [];
  }
  const eligible = new Set(filtered.eligible);
  return concreteCandidates
    .filter((candidate) => eligible.has(candidate.routeId))
    .map((candidate) => ({
      routeId: candidate.routeId,
      label: candidate.label,
      path: canonicalRoutePath(candidate.routeId),
      active: getActivePrimaryNavigationId(options.currentRouteId, concreteCandidates) === candidate.routeId,
    }));
}

export function getActivePrimaryNavigationId(
  currentRouteId: CanonicalRouteId | undefined,
  items: readonly Pick<PrimaryNavigationItem | PrimaryNavigationCandidate, 'routeId'>[],
): CanonicalRouteId | undefined {
  if (currentRouteId === undefined) {
    return undefined;
  }
  const itemIds = new Set(items.map((item) => item.routeId));
  for (const candidate of PRIMARY_NAVIGATION_CANDIDATES) {
    if (itemIds.has(candidate.routeId) && candidate.activeFamily.includes(currentRouteId)) {
      return candidate.routeId;
    }
  }
  return undefined;
}

export function getMountedConcreteRouteIds(routes: Routes): readonly CanonicalRouteId[] {
  const ids: CanonicalRouteId[] = [];
  for (const route of routes) {
    collectMountedConcreteRouteIds(route, ids);
  }
  return ids;
}

export function getRouteIdForUrl(path: string): CanonicalRouteId | undefined {
  const normalizedPath = path.split(/[?#]/, 1)[0] || '/';
  for (const candidate of PRIMARY_NAVIGATION_CANDIDATES.flatMap((candidate) => candidate.activeFamily)) {
    if (matchesCanonicalPath(normalizedPath, canonicalRoutePath(candidate))) {
      return candidate;
    }
  }
  return undefined;
}

function collectMountedConcreteRouteIds(route: Route, ids: CanonicalRouteId[]): void {
  const dataRouteId = route.data?.['routeId'];
  if (
    typeof dataRouteId === 'string' &&
    route.redirectTo === undefined &&
    typeof route.path === 'string' &&
    !route.path.includes(':')
  ) {
    ids.push(dataRouteId as CanonicalRouteId);
  }
  for (const child of route.children ?? []) {
    collectMountedConcreteRouteIds(child, ids);
  }
}

function matchesCanonicalPath(path: string, template: string): boolean {
  const pathParts = path.split('/').filter(Boolean);
  const templateParts = template.split('/').filter(Boolean);
  if (pathParts.length !== templateParts.length) {
    return false;
  }
  return templateParts.every((part, index) => part.startsWith(':') || part === pathParts[index]);
}
