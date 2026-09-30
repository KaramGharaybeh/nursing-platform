import type { CanonicalRouteId } from './canonical-routes';

export type RoutePermissionPolicyKind = 'AUTHENTICATED_ONLY' | 'ROLE' | 'ROLE_AND_PERMISSION';

export interface AuthenticatedOnlyRoutePolicy {
  readonly kind: 'AUTHENTICATED_ONLY';
  readonly routeId: CanonicalRouteId;
}

export interface RoleRoutePolicy {
  readonly kind: 'ROLE';
  readonly routeId: CanonicalRouteId;
  readonly acceptedRoles: readonly string[];
}

export interface RoleAndPermissionRoutePolicy {
  readonly kind: 'ROLE_AND_PERMISSION';
  readonly routeId: CanonicalRouteId;
  readonly acceptedRoles: readonly string[];
  readonly requiredPermission: string;
}

export type RoutePermissionPolicy =
  | AuthenticatedOnlyRoutePolicy
  | RoleRoutePolicy
  | RoleAndPermissionRoutePolicy;

export interface ReadyRouteUser {
  readonly roles: readonly string[];
  readonly permissions: readonly string[];
}

export const ROUTE_PERMISSION_POLICIES: readonly RoutePermissionPolicy[] = Object.freeze([
  { kind: 'AUTHENTICATED_ONLY', routeId: 'ONBOARDING_PROFILE' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'ACCOUNT_OVERVIEW' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_CATALOG' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_DETAIL' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_INSTRUCTIONS' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_SESSION' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_RESULT' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_ANALYTICS' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_REVIEW' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'EXAMS_HISTORY' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'COMMERCE_PRODUCTS' },
  { kind: 'AUTHENTICATED_ONLY', routeId: 'COMMERCE_PRODUCT_DETAIL' },
  { kind: 'ROLE', routeId: 'NURSE_ENTRY', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_OVERVIEW', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_PERSONAL_INFORMATION', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_EXPERIENCE', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_EDUCATION', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_CERTIFICATES', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_SKILLS', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_LANGUAGES', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_PROFILE_CV', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'NURSE_CONTACT_REQUESTS', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'PREPARATION_PACKAGES_ENTITLEMENTS', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'PREPARATION_PACKAGES_ENTITLEMENT_DETAIL', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'PREPARATION_PACKAGES_PRACTICE', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'PREPARATION_PACKAGES_REPORT', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'COMMERCE_CHECKOUT', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'COMMERCE_PAYMENT_SUCCESS', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'COMMERCE_PAYMENT_FAILURE', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'COMMERCE_ORDERS', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'COMMERCE_ORDER_DETAIL', acceptedRoles: ['Nurse'] },
  { kind: 'ROLE', routeId: 'EMPLOYER_HOME', acceptedRoles: ['Employer'] },
  { kind: 'ROLE', routeId: 'EMPLOYER_CANDIDATES', acceptedRoles: ['Employer'] },
  { kind: 'ROLE', routeId: 'EMPLOYER_REQUESTS', acceptedRoles: ['Employer'] },
  { kind: 'ROLE', routeId: 'EMPLOYER_REQUEST_DETAIL', acceptedRoles: ['Employer'] },
  { kind: 'ROLE', routeId: 'ADMIN_ENTRY', acceptedRoles: ['Admin'] },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_USERS', acceptedRoles: ['Admin'], requiredPermission: 'Users.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_USER_DETAIL', acceptedRoles: ['Admin'], requiredPermission: 'Users.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_REFERENCE_DATA', acceptedRoles: ['Admin'], requiredPermission: 'Exams.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_EXAMS', acceptedRoles: ['Admin'], requiredPermission: 'Exams.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_EXAM_DETAIL', acceptedRoles: ['Admin'], requiredPermission: 'Exams.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_EXAM_VERSIONS', acceptedRoles: ['Admin'], requiredPermission: 'Exams.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_EXAM_QUESTIONS', acceptedRoles: ['Admin'], requiredPermission: 'Questions.View' },
  { kind: 'ROLE_AND_PERMISSION', routeId: 'ADMIN_PAYMENT_PRODUCTS', acceptedRoles: ['Admin'], requiredPermission: 'Exams.View' },
  {
    kind: 'ROLE_AND_PERMISSION',
    routeId: 'ADMIN_PREPARATION_PACKAGE_TOPICS',
    acceptedRoles: ['Admin'],
    requiredPermission: 'ReportingTopics.Manage',
  },
  {
    kind: 'ROLE_AND_PERMISSION',
    routeId: 'ADMIN_PREPARATION_PACKAGE_PROFILES',
    acceptedRoles: ['Admin'],
    requiredPermission: 'ReportingProfiles.Manage',
  },
  {
    kind: 'ROLE_AND_PERMISSION',
    routeId: 'ADMIN_PREPARATION_PACKAGE_MATERIALS',
    acceptedRoles: ['Admin'],
    requiredPermission: 'StudyMaterials.Manage',
  },
  {
    kind: 'ROLE_AND_PERMISSION',
    routeId: 'ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS',
    acceptedRoles: ['Admin'],
    requiredPermission: 'PracticeCollections.Manage',
  },
  {
    kind: 'ROLE_AND_PERMISSION',
    routeId: 'ADMIN_PREPARATION_PACKAGE_DEFINITIONS',
    acceptedRoles: ['Admin'],
    requiredPermission: 'PreparationPackages.View',
  },
  {
    kind: 'ROLE_AND_PERMISSION',
    routeId: 'ADMIN_PREPARATION_PACKAGE_OFFERS',
    acceptedRoles: ['Admin'],
    requiredPermission: 'PreparationPackageOffers.Manage',
  },
]);

const ROUTE_PERMISSION_POLICY_BY_ID: ReadonlyMap<CanonicalRouteId, RoutePermissionPolicy> = new Map(
  ROUTE_PERMISSION_POLICIES.map((entry) => [entry.routeId, entry]),
);

export function getRoutePermissionPolicy(routeId: CanonicalRouteId): RoutePermissionPolicy {
  const policy = ROUTE_PERMISSION_POLICY_BY_ID.get(routeId);
  if (policy === undefined) {
    throw new Error(`Missing route permission policy for canonical route "${routeId}".`);
  }
  return policy;
}

function hasAcceptedRole(policy: RoleRoutePolicy | RoleAndPermissionRoutePolicy, user: ReadyRouteUser): boolean {
  return policy.acceptedRoles.some((accepted) => user.roles.includes(accepted));
}

export function evaluateRoutePermission(policy: RoutePermissionPolicy, user: ReadyRouteUser): boolean {
  if (policy.kind === 'AUTHENTICATED_ONLY') {
    return true;
  }
  if (policy.kind === 'ROLE') {
    return hasAcceptedRole(policy, user);
  }
  return hasAcceptedRole(policy, user) && user.permissions.includes(policy.requiredPermission);
}
