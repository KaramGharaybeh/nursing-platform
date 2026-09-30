import { describe, expect, it } from 'vitest';
import type { CanonicalRouteId } from '../routing/canonical-routes';
import {
  getActivePrimaryNavigationId,
  getMountedConcreteRouteIds,
  getPrimaryNavigationItems,
} from './primary-navigation';

const mountedRouteIds = [
  'ACCOUNT_OVERVIEW',
  'ADMIN_ENTRY',
  'ADMIN_USERS',
  'ADMIN_USER_DETAIL',
  'NURSE_PROFILE_OVERVIEW',
  'NURSE_PROFILE_PERSONAL_INFORMATION',
  'NURSE_PROFILE_EXPERIENCE',
  'NURSE_PROFILE_EDUCATION',
  'NURSE_PROFILE_CERTIFICATES',
  'NURSE_PROFILE_SKILLS',
  'NURSE_PROFILE_LANGUAGES',
  'NURSE_PROFILE_CV',
  'NURSE_CONTACT_REQUESTS',
  'PREPARATION_PACKAGES_ENTITLEMENTS',
  'PREPARATION_PACKAGES_ENTITLEMENT_DETAIL',
  'PREPARATION_PACKAGES_PRACTICE',
  'PREPARATION_PACKAGES_REPORT',
  'EXAMS_CATALOG',
  'EXAMS_ANALYTICS',
  'EXAMS_HISTORY',
  'EXAMS_DETAIL',
  'EXAMS_INSTRUCTIONS',
  'EXAMS_SESSION',
  'EXAMS_RESULT',
  'EXAMS_REVIEW',
  'COMMERCE_PRODUCTS',
  'COMMERCE_PRODUCT_DETAIL',
] satisfies CanonicalRouteId[];

describe('primary navigation inventory', () => {
  it('returns current mounted Nurse primary families only', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds,
      user: { status: 'ready', roles: ['Nurse'], permissions: [] },
    });

    expect(items.map((item) => [item.routeId, item.label, item.path])).toEqual([
      ['EXAMS_CATALOG', 'Exams', '/exams'],
      ['PREPARATION_PACKAGES_ENTITLEMENTS', 'Preparation Packages', '/nurse/preparation-packages'],
      ['COMMERCE_PRODUCTS', 'Products', '/commerce/products'],
      ['NURSE_PROFILE_OVERVIEW', 'Profile', '/nurse/profile'],
    ]);
  });

  it('returns no Employer primary destinations while Employer routes are unmounted', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds,
      user: { status: 'ready', roles: ['Employer'], permissions: [] },
    });

    expect(items).toEqual([]);
  });

  it('returns only mounted Admin primary destinations allowed by current permissions', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds,
      user: { status: 'ready', roles: ['Admin'], permissions: ['Users.View'] },
    });

    expect(items.map((item) => [item.routeId, item.label, item.path])).toEqual([
      ['ADMIN_ENTRY', 'Admin', '/admin'],
      ['ADMIN_USERS', 'Users', '/admin/users'],
    ]);
  });

  it('filters Admin permission-protected destinations when permission is missing', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds,
      user: { status: 'ready', roles: ['Admin'], permissions: [] },
    });

    expect(items.map((item) => item.routeId)).toEqual(['ADMIN_ENTRY']);
  });

  it('excludes unmounted canonical routes and unresolved parameterized destinations', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds: [...mountedRouteIds, 'ADMIN_EXAMS', 'ADMIN_EXAM_QUESTIONS'],
      user: { status: 'ready', roles: ['Admin'], permissions: ['Users.View', 'Exams.View', 'Questions.View'] },
    });

    expect(items.map((item) => item.routeId)).toEqual(['ADMIN_ENTRY', 'ADMIN_USERS']);
    expect(items.every((item) => !item.path.includes(':'))).toBe(true);
  });

  it('excludes contextual detail routes from global primary items', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds,
      user: { status: 'ready', roles: ['Nurse'], permissions: [] },
    });

    expect(items.map((item) => item.routeId)).not.toContain('EXAMS_DETAIL');
    expect(items.map((item) => item.routeId)).not.toContain('COMMERCE_PRODUCT_DETAIL');
    expect(items.map((item) => item.routeId)).not.toContain('PREPARATION_PACKAGES_ENTITLEMENT_DETAIL');
  });

  it('resolves active primary family for contextual child routes', () => {
    const items = getPrimaryNavigationItems({
      mountedRouteIds,
      user: { status: 'ready', roles: ['Nurse'], permissions: [] },
    });

    expect(getActivePrimaryNavigationId('EXAMS_REVIEW', items)).toBe('EXAMS_CATALOG');
    expect(getActivePrimaryNavigationId('PREPARATION_PACKAGES_REPORT', items)).toBe('PREPARATION_PACKAGES_ENTITLEMENTS');
    expect(getActivePrimaryNavigationId('COMMERCE_PRODUCT_DETAIL', items)).toBe('COMMERCE_PRODUCTS');
    expect(getActivePrimaryNavigationId('NURSE_PROFILE_CV', items)).toBe('NURSE_PROFILE_OVERVIEW');
  });

  it('derives mounted concrete route ids from Angular route config', () => {
    const ids = getMountedConcreteRouteIds([
      { path: 'admin', data: { routeId: 'ADMIN_ENTRY' } },
      { path: 'admin/users/:userId', data: { routeId: 'ADMIN_USER_DETAIL' } },
      { path: 'nurse', redirectTo: 'nurse/profile', pathMatch: 'full' },
    ]);

    expect(ids).toEqual(['ADMIN_ENTRY']);
  });
});
