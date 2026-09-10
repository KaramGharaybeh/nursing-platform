import { CANONICAL_ROUTES } from './canonical-routes';
import type { CanonicalRouteId } from './canonical-routes';
import { CANONICAL_ROUTE_IDS } from './canonical-routes';
import { isAuthenticatedRoute } from './route-classification';
import {
  ROUTE_PERMISSION_POLICIES,
  evaluateRoutePermission,
  getRoutePermissionPolicy,
} from './route-permission-policy';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function stripCellWrapping(value: string): string {
  return value.trim().replace(/^`+|`+$/g, '').trim();
}

interface ApprovedMatrixRow {
  routeId: string;
  path: string;
  policy: string;
  acceptedRoles: string[];
  requiredPermission: string;
}

function parseApprovedPolicyMatrix(): ApprovedMatrixRow[] {
  const text = readTextFile('../docs/frontend/design/inventory/route-permission-matrix.md');
  const section =
    text.split('### 15.8 Explicit 49-route policy matrix')[1]?.split('### 15.9')[0] ?? '';
  const rows: ApprovedMatrixRow[] = [];
  for (const line of section.split('\n')) {
    if (!line.trimStart().startsWith('| `')) {
      continue;
    }
    const cells = line.split('|');
    if (cells.length < 8) {
      continue;
    }
    const routeId = stripCellWrapping(cells[1]);
    const path = stripCellWrapping(cells[2]);
    const policy = stripCellWrapping(cells[3]);
    const rolesRaw = stripCellWrapping(cells[4]);
    const permissionRaw = stripCellWrapping(cells[5]);
    const acceptedRoles =
      rolesRaw === '—' || rolesRaw === ''
        ? []
        : rolesRaw
            .split(',')
            .map((entry) => stripCellWrapping(entry))
            .filter((entry) => entry !== '');
    rows.push({
      routeId,
      path,
      policy,
      acceptedRoles,
      requiredPermission: permissionRaw === '—' ? '' : permissionRaw,
    });
  }
  return rows;
}

function readyUser(roles: string[], permissions: string[]): {
  roles: readonly string[];
  permissions: readonly string[];
} {
  return { roles, permissions };
}

describe('route-permission-policy', () => {
  it('matches the approved 49-route matrix mechanically with no gaps or extras', () => {
    const approved = parseApprovedPolicyMatrix();

    expect(approved).toHaveLength(49);
    expect(ROUTE_PERMISSION_POLICIES).toHaveLength(49);

    const registryById = new Map(ROUTE_PERMISSION_POLICIES.map((entry) => [entry.routeId, entry]));
    expect(registryById.size).toBe(49);

    for (const row of approved) {
      const policy = registryById.get(row.routeId as CanonicalRouteId);
      expect(policy).toBeDefined();
      expect(policy?.kind).toBe(row.policy);
      expect(CANONICAL_ROUTES[row.routeId as CanonicalRouteId]).toBe(row.path);
      if (row.policy === 'AUTHENTICATED_ONLY') {
        expect(policy).toEqual({ kind: 'AUTHENTICATED_ONLY', routeId: row.routeId });
      }
      if (row.policy === 'ROLE') {
        expect(policy).toEqual({
          kind: 'ROLE',
          routeId: row.routeId,
          acceptedRoles: row.acceptedRoles,
        });
      }
      if (row.policy === 'ROLE_AND_PERMISSION') {
        expect(policy).toEqual({
          kind: 'ROLE_AND_PERMISSION',
          routeId: row.routeId,
          acceptedRoles: row.acceptedRoles,
          requiredPermission: row.requiredPermission,
        });
      }
    }

    const approvedIds = new Set(approved.map((row) => row.routeId));
    for (const entry of ROUTE_PERMISSION_POLICIES) {
      expect(approvedIds.has(entry.routeId)).toBe(true);
    }
  });

  it('proves the locked matrix counts with zero duplicates, missing, or extras', () => {
    const authenticatedRoutes = CANONICAL_ROUTE_IDS.filter((id) => isAuthenticatedRoute(id));
    const registryIds = ROUTE_PERMISSION_POLICIES.map((entry) => entry.routeId);

    const authenticatedOnly = ROUTE_PERMISSION_POLICIES.filter(
      (entry) => entry.kind === 'AUTHENTICATED_ONLY',
    );
    const nurseRole = ROUTE_PERMISSION_POLICIES.filter(
      (entry) => entry.kind === 'ROLE' && entry.acceptedRoles.includes('Nurse'),
    );
    const employerRole = ROUTE_PERMISSION_POLICIES.filter(
      (entry) => entry.kind === 'ROLE' && entry.acceptedRoles.includes('Employer'),
    );
    const adminEntryRole = ROUTE_PERMISSION_POLICIES.filter(
      (entry) => entry.kind === 'ROLE' && entry.acceptedRoles.includes('Admin'),
    );
    const adminRoleAndPermission = ROUTE_PERMISSION_POLICIES.filter(
      (entry) => entry.kind === 'ROLE_AND_PERMISSION',
    );

    expect(authenticatedRoutes).toHaveLength(49);
    expect(ROUTE_PERMISSION_POLICIES).toHaveLength(49);
    expect(authenticatedOnly).toHaveLength(11);
    expect(nurseRole).toHaveLength(19);
    expect(employerRole).toHaveLength(4);
    expect(adminEntryRole).toHaveLength(1);
    expect(adminRoleAndPermission).toHaveLength(14);
    expect(
      authenticatedOnly.length +
        nurseRole.length +
        employerRole.length +
        adminEntryRole.length +
        adminRoleAndPermission.length,
    ).toBe(49);

    expect(new Set(registryIds).size).toBe(49);

    const registryIdSet = new Set(registryIds);
    const missing = authenticatedRoutes.filter((id) => !registryIdSet.has(id));
    const extra = registryIds.filter((id) => !authenticatedRoutes.includes(id));
    expect(missing).toEqual([]);
    expect(extra).toEqual([]);

    const publicWithPolicy = registryIds.filter((id) => !isAuthenticatedRoute(id));
    expect(publicWithPolicy).toEqual([]);
    expect(registryIdSet.has('ROOT_ENTRY' as CanonicalRouteId)).toBe(false);
  });

  it('allows any ready user for AUTHENTICATED_ONLY regardless of roles or permissions', () => {
    const policy = getRoutePermissionPolicy('ACCOUNT_OVERVIEW');

    expect(policy.kind).toBe('AUTHENTICATED_ONLY');
    expect(evaluateRoutePermission(policy, readyUser([], []))).toBe(true);
    expect(evaluateRoutePermission(policy, readyUser(['Nurse'], []))).toBe(true);
    expect(evaluateRoutePermission(policy, readyUser(['Employer'], ['Users.View']))).toBe(true);
    expect(evaluateRoutePermission(policy, readyUser(['Admin'], ['Exams.View']))).toBe(true);
  });

  it('allows ROLE only when the ready user holds an accepted role', () => {
    const nursePolicy = getRoutePermissionPolicy('NURSE_ENTRY');

    expect(evaluateRoutePermission(nursePolicy, readyUser(['Nurse'], []))).toBe(true);
    expect(evaluateRoutePermission(nursePolicy, readyUser(['Employer'], []))).toBe(false);
    expect(evaluateRoutePermission(nursePolicy, readyUser([], []))).toBe(false);
    expect(evaluateRoutePermission(nursePolicy, readyUser(['Admin'], ['Users.View']))).toBe(false);
  });

  it('applies multi-role union semantics with no role precedence', () => {
    const nursePolicy = getRoutePermissionPolicy('NURSE_ENTRY');
    const employerPolicy = getRoutePermissionPolicy('EMPLOYER_HOME');

    expect(evaluateRoutePermission(nursePolicy, readyUser(['Admin', 'Nurse'], []))).toBe(true);
    expect(evaluateRoutePermission(employerPolicy, readyUser(['Admin', 'Nurse'], []))).toBe(false);
    expect(evaluateRoutePermission(employerPolicy, readyUser(['Employer', 'Nurse'], []))).toBe(
      true,
    );
  });

  it('requires both role and permission for ROLE_AND_PERMISSION without admin bypass', () => {
    const usersPolicy = getRoutePermissionPolicy('ADMIN_USERS');

    expect(
      evaluateRoutePermission(usersPolicy, readyUser(['Admin'], ['Users.View'])),
    ).toBe(true);
    expect(evaluateRoutePermission(usersPolicy, readyUser(['Admin'], []))).toBe(false);
    expect(evaluateRoutePermission(usersPolicy, readyUser(['Admin'], ['Exams.View']))).toBe(false);
    expect(evaluateRoutePermission(usersPolicy, readyUser(['Nurse'], ['Users.View']))).toBe(false);
    expect(evaluateRoutePermission(usersPolicy, readyUser([], ['Users.View']))).toBe(false);
  });

  it('treats missing policy lookup as an explicit configuration failure', () => {
    expect(() => getRoutePermissionPolicy('AUTH_SIGN_IN')).toThrow();
    expect(() => getRoutePermissionPolicy('ROOT_ENTRY')).toThrow();
    expect(() =>
      getRoutePermissionPolicy('UNKNOWN_ROUTE' as unknown as CanonicalRouteId),
    ).toThrow();
  });

  it('holds no PUBLIC route policy and no ROOT_ENTRY policy', () => {
    const registryIds = new Set(ROUTE_PERMISSION_POLICIES.map((entry) => entry.routeId));

    for (const id of CANONICAL_ROUTE_IDS) {
      if (!isAuthenticatedRoute(id)) {
        expect(registryIds.has(id)).toBe(false);
      }
    }
    expect(registryIds.has('PREPARATION_PACKAGES_OFFERS' as CanonicalRouteId)).toBe(false);
    expect(registryIds.has('SYSTEM_ACCESS_DENIED' as CanonicalRouteId)).toBe(false);
  });

  it('leaves app.routes.ts empty and canonical classification unchanged', () => {
    const appRoutes = readTextFile('src/app/app.routes.ts');

    expect(appRoutes).toContain('Routes = []');

    const authenticated = CANONICAL_ROUTE_IDS.filter((id) => isAuthenticatedRoute(id));
    expect(authenticated).toHaveLength(49);
    expect(CANONICAL_ROUTE_IDS).toHaveLength(62);
  });

  it('keeps the policy module pure without prefix inference or navigation behavior', () => {
    const source = readTextFile('src/app/core/routing/route-permission-policy.ts');
    const lowered = source.toLowerCase();

    expect(source).not.toContain("startsWith('/admin')");
    expect(source).not.toContain("startsWith('/nurse')");
    expect(source).not.toContain("startsWith('/employer')");
    expect(source).not.toContain('/access-denied');
    expect(source).not.toContain('returnUrl');
    expect(lowered).not.toContain('@angular');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('httpclient');
    expect(lowered).not.toContain('authtransport');
    expect(lowered).not.toContain('tokenstorage');
    expect(lowered).not.toContain('currentuserstore');
    expect(lowered).not.toContain('menu');
    expect(lowered).not.toContain('sidebar');
    expect(lowered).not.toContain('breadcrumb');
  });
});
