import type { CanonicalRouteId } from './canonical-routes';
import {
  filterEligibleNavigationCandidates,
  getNavigationEligibility,
} from './navigation-permission-policy';
import type { NavigationUserState } from './navigation-permission-policy';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function readyUser(roles: readonly string[], permissions: readonly string[]): NavigationUserState {
  return { status: 'ready', roles, permissions };
}

describe('navigation-permission-policy', () => {
  it('treats READY plus AUTHENTICATED_ONLY as eligible for presentation', () => {
    const result = getNavigationEligibility('ACCOUNT_OVERVIEW', readyUser([], []));

    expect(result).toEqual({ routeId: 'ACCOUNT_OVERVIEW', kind: 'eligible' });
  });

  it('treats READY plus matching Nurse role as eligible', () => {
    const result = getNavigationEligibility('NURSE_ENTRY', readyUser(['Nurse'], []));

    expect(result).toEqual({ routeId: 'NURSE_ENTRY', kind: 'eligible' });
  });

  it('treats READY plus missing Nurse role as ineligible for presentation only', () => {
    const result = getNavigationEligibility('NURSE_ENTRY', readyUser(['Employer'], []));

    expect(result).toEqual({ routeId: 'NURSE_ENTRY', kind: 'ineligible' });
  });

  it('treats READY plus matching Employer role as eligible', () => {
    const result = getNavigationEligibility('EMPLOYER_HOME', readyUser(['Employer'], []));

    expect(result).toEqual({ routeId: 'EMPLOYER_HOME', kind: 'eligible' });
  });

  it('treats READY plus Admin role and required permission as eligible', () => {
    const result = getNavigationEligibility('ADMIN_USERS', readyUser(['Admin'], ['Users.View']));

    expect(result).toEqual({ routeId: 'ADMIN_USERS', kind: 'eligible' });
  });

  it('treats READY plus Admin role without the required permission as ineligible', () => {
    const result = getNavigationEligibility('ADMIN_USERS', readyUser(['Admin'], []));

    expect(result).toEqual({ routeId: 'ADMIN_USERS', kind: 'ineligible' });
  });

  it('denies Admin with an unrelated permission without admin bypass', () => {
    const result = getNavigationEligibility('ADMIN_USERS', readyUser(['Admin'], ['Exams.View']));

    expect(result).toEqual({ routeId: 'ADMIN_USERS', kind: 'ineligible' });
  });

  it('inherits multi-role union semantics with no role precedence', () => {
    const nurseAllowed = getNavigationEligibility(
      'NURSE_ENTRY',
      readyUser(['Admin', 'Nurse'], []),
    );
    const employerDenied = getNavigationEligibility(
      'EMPLOYER_HOME',
      readyUser(['Admin', 'Nurse'], []),
    );

    expect(nurseAllowed.kind).toBe('eligible');
    expect(employerDenied.kind).toBe('ineligible');
  });

  it('preserves caller input order when filtering candidates', () => {
    const candidates: readonly CanonicalRouteId[] = [
      'EMPLOYER_HOME',
      'ACCOUNT_OVERVIEW',
      'NURSE_ENTRY',
    ];

    const result = filterEligibleNavigationCandidates(candidates, readyUser(['Nurse'], []));

    expect(result).toEqual({ status: 'resolved', eligible: ['ACCOUNT_OVERVIEW', 'NURSE_ENTRY'] });
  });

  it('returns only eligible supplied route IDs when filtering', () => {
    const candidates: readonly CanonicalRouteId[] = [
      'NURSE_ENTRY',
      'EMPLOYER_HOME',
      'ACCOUNT_OVERVIEW',
    ];

    const result = filterEligibleNavigationCandidates(candidates, readyUser(['Nurse'], []));

    expect(result.status).toBe('resolved');
    if (result.status !== 'resolved') {
      throw new Error('Expected resolved navigation candidates.');
    }
    expect(result.eligible).toContain('NURSE_ENTRY');
    expect(result.eligible).toContain('ACCOUNT_OVERVIEW');
    expect(result.eligible).not.toContain('EMPLOYER_HOME');
  });

  it('introduces no new route IDs when filtering', () => {
    const candidates: readonly CanonicalRouteId[] = [
      'NURSE_ENTRY',
      'EMPLOYER_HOME',
      'ACCOUNT_OVERVIEW',
    ];

    const result = filterEligibleNavigationCandidates(candidates, readyUser(['Nurse'], []));

    expect(result.status).toBe('resolved');
    if (result.status !== 'resolved') {
      throw new Error('Expected resolved navigation candidates.');
    }
    for (const routeId of result.eligible) {
      expect(candidates).toContain(routeId);
    }
    expect(result.eligible.length).toBeLessThanOrEqual(candidates.length);
  });

  it('preserves duplicate candidate entries when the route is eligible', () => {
    const candidates: readonly CanonicalRouteId[] = [
      'NURSE_ENTRY',
      'ACCOUNT_OVERVIEW',
      'NURSE_ENTRY',
    ];

    const result = filterEligibleNavigationCandidates(candidates, readyUser(['Nurse'], []));

    expect(result).toEqual({
      status: 'resolved',
      eligible: ['NURSE_ENTRY', 'ACCOUNT_OVERVIEW', 'NURSE_ENTRY'],
    });
  });

  it('documents deterministic duplicate behavior by dropping ineligible duplicates', () => {
    const candidates: readonly CanonicalRouteId[] = ['EMPLOYER_HOME', 'EMPLOYER_HOME'];

    const result = filterEligibleNavigationCandidates(candidates, readyUser(['Nurse'], []));

    expect(result).toEqual({ status: 'resolved', eligible: [] });
  });

  it('represents idle as unresolved rather than denied', () => {
    const result = getNavigationEligibility('NURSE_ENTRY', { status: 'idle' });

    expect(result).toEqual({ routeId: 'NURSE_ENTRY', kind: 'unresolved' });
    expect(result.kind).not.toBe('ineligible');
  });

  it('represents loading as unresolved rather than denied', () => {
    const result = getNavigationEligibility('ADMIN_USERS', { status: 'loading' });

    expect(result).toEqual({ routeId: 'ADMIN_USERS', kind: 'unresolved' });
    expect(result.kind).not.toBe('ineligible');
  });

  it('represents unavailable as unresolved pass-through rather than denied', () => {
    const result = getNavigationEligibility('ADMIN_USERS', { status: 'unavailable' });

    expect(result).toEqual({ routeId: 'ADMIN_USERS', kind: 'unresolved' });
    expect(result.kind).not.toBe('ineligible');
  });

  it('represents anonymous as non-ready without sign-in or access-denied behavior', () => {
    const result = getNavigationEligibility('NURSE_ENTRY', { status: 'anonymous' });

    expect(result).toEqual({ routeId: 'NURSE_ENTRY', kind: 'unresolved' });
    expect(result.kind).not.toBe('ineligible');
  });

  it('represents non-ready candidate filtering as unresolved and preserves caller candidates', () => {
    const candidates: readonly CanonicalRouteId[] = ['NURSE_ENTRY', 'ACCOUNT_OVERVIEW'];
    const states: readonly NavigationUserState[] = [
      { status: 'idle' },
      { status: 'loading' },
      { status: 'unavailable' },
      { status: 'anonymous' },
    ];

    for (const user of states) {
      const result = filterEligibleNavigationCandidates(candidates, user);

      expect(result.status).toBe('unresolved');
      expect(result.status).not.toBe('resolved');
      if (result.status !== 'unresolved') {
        throw new Error('Expected unresolved navigation candidates.');
      }
      expect(result.candidates).toEqual([...candidates]);
    }
    expect(getNavigationEligibility('NURSE_ENTRY', { status: 'idle' }).kind).not.toBe(
      'ineligible',
    );
  });

  it('returns explicit unsupported for PUBLIC routes with no presentation inference', () => {
    const signIn = getNavigationEligibility('AUTH_SIGN_IN', readyUser(['Nurse'], []));
    const offers = getNavigationEligibility(
      'PREPARATION_PACKAGES_OFFERS',
      readyUser(['Nurse'], []),
    );

    expect(signIn).toEqual({ routeId: 'AUTH_SIGN_IN', kind: 'unsupported' });
    expect(offers).toEqual({ routeId: 'PREPARATION_PACKAGES_OFFERS', kind: 'unsupported' });
    expect(signIn.kind).not.toBe('eligible');
    expect(offers.kind).not.toBe('eligible');
  });

  it('returns explicit unsupported for entry and unknown routes with no path inference', () => {
    const entry = getNavigationEligibility('ROOT_ENTRY', readyUser(['Nurse'], []));
    const unknown = getNavigationEligibility(
      'UNKNOWN_ROUTE' as unknown as CanonicalRouteId,
      readyUser(['Nurse'], []),
    );

    expect(entry).toEqual({ routeId: 'ROOT_ENTRY', kind: 'unsupported' });
    expect(unknown.kind).toBe('unsupported');
    expect(entry.kind).not.toBe('eligible');
    expect(entry.kind).not.toBe('ineligible');
  });

  it('reuses the T-FE-031 policy registry and evaluator without duplicating logic', () => {
    const source = readTextFile('src/app/core/routing/navigation-permission-policy.ts');

    expect(source).toContain('getRoutePermissionPolicy');
    expect(source).toContain('evaluateRoutePermission');
  });

  it('keeps the navigation module pure without inference, routing, or UI behavior', () => {
    const source = readTextFile('src/app/core/routing/navigation-permission-policy.ts');
    const lowered = source.toLowerCase();

    expect(source).not.toContain("startsWith('/admin')");
    expect(source).not.toContain("startsWith('/nurse')");
    expect(source).not.toContain("startsWith('/employer')");
    expect(source).not.toContain('/access-denied');
    expect(source).not.toContain('returnUrl');
    expect(source).not.toContain('/auth/sign-in');
    expect(lowered).not.toContain('@angular');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('navigate');
    expect(lowered).not.toContain('redirect');
    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('authtransport');
    expect(lowered).not.toContain('tokenstorage');
    expect(lowered).not.toContain('currentuserstore');
    expect(lowered).not.toContain('sessionstorage');
    expect(lowered).not.toContain('localstorage');
    expect(lowered).not.toContain('jwt');
    expect(lowered).not.toContain('/me');
    expect(lowered).not.toContain('menu');
    expect(lowered).not.toContain('sidebar');
    expect(lowered).not.toContain('breadcrumb');
  });

  it('leaves app.routes.ts empty and unchanged', () => {
    const appRoutes = readTextFile('src/app/app.routes.ts');

    expect(appRoutes).toContain('Routes = []');
  });
});
