// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { CANONICAL_ROUTE_IDS } from './canonical-routes';
import type { CanonicalRouteId } from './canonical-routes';
import {
  ENTRY_ROUTE_ID,
  PUBLIC_ROUTE_IDS,
  ROUTE_CLASSIFICATION_COUNTS,
  classifyRoute,
  isAuthenticatedRoute,
  isPublicRoute,
} from './route-classification';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function stripCellWrapping(value: string): string {
  return value.trim().replace(/^`+|`+$/g, '').trim();
}

function parsePublicRouteIdsFromMatrix(): string[] {
  const text = readTextFile('../docs/frontend/design/inventory/route-permission-matrix.md');
  const section = text.split('## 3. Exact PUBLIC routes')[1]?.split('## 4. ENTRY route')[0] ?? '';
  const ids: string[] = [];
  for (const line of section.split('\n')) {
    if (!line.trimStart().startsWith('| `')) {
      continue;
    }
    const cells = line.split('|');
    if (cells.length < 3) {
      continue;
    }
    ids.push(stripCellWrapping(cells[1]));
  }
  return ids;
}

const EXPECTED_PUBLIC_ROUTE_IDS: readonly CanonicalRouteId[] = [
  'AUTH_SIGN_IN',
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
];

describe('route-classification', () => {
  it('matches the approved PUBLIC route IDs in route-permission-matrix.md mechanically', () => {
    const approved = parsePublicRouteIdsFromMatrix();

    expect(approved).toHaveLength(12);
    expect([...PUBLIC_ROUTE_IDS]).toEqual(approved);
  });

  it('exposes the locked classification counts for the 62-route registry', () => {
    expect(CANONICAL_ROUTE_IDS).toHaveLength(62);
    expect(ROUTE_CLASSIFICATION_COUNTS).toEqual({
      total: 62,
      public: 12,
      entry: 1,
      authenticated: 49,
    });
    expect(
      ROUTE_CLASSIFICATION_COUNTS.public +
        ROUTE_CLASSIFICATION_COUNTS.entry +
        ROUTE_CLASSIFICATION_COUNTS.authenticated,
    ).toBe(62);
  });

  it('classifies every approved PUBLIC route as PUBLIC and never AUTHENTICATED', () => {
    for (const id of EXPECTED_PUBLIC_ROUTE_IDS) {
      expect(classifyRoute(id)).toBe('PUBLIC');
      expect(isPublicRoute(id)).toBe(true);
      expect(isAuthenticatedRoute(id)).toBe(false);
    }
  });

  it('classifies ROOT_ENTRY as the single ENTRY route', () => {
    expect(ENTRY_ROUTE_ID).toBe('ROOT_ENTRY');
    expect(classifyRoute('ROOT_ENTRY')).toBe('ENTRY');
    expect(isPublicRoute('ROOT_ENTRY')).toBe(false);
    expect(isAuthenticatedRoute('ROOT_ENTRY')).toBe(false);
  });

  it('derives exactly 49 AUTHENTICATED routes without a manual exhaustive list', () => {
    const authenticated = CANONICAL_ROUTE_IDS.filter((id) => isAuthenticatedRoute(id));

    expect(authenticated).toHaveLength(49);
    expect(authenticated).toContain('NURSE_ENTRY');
    expect(authenticated).toContain('EMPLOYER_HOME');
    expect(authenticated).toContain('ADMIN_ENTRY');
    expect(authenticated).toContain('ACCOUNT_OVERVIEW');
    expect(authenticated).toContain('EXAMS_CATALOG');
    expect(authenticated).toContain('COMMERCE_PRODUCTS');
  });

  it('assigns every canonical route exactly one classification with no gaps or duplicates', () => {
    const classified = new Set<CanonicalRouteId>();

    for (const id of CANONICAL_ROUTE_IDS) {
      const classification = classifyRoute(id);
      expect(['PUBLIC', 'ENTRY', 'AUTHENTICATED']).toContain(classification);
      classified.add(id);
    }

    expect(classified.size).toBe(62);
    expect(CANONICAL_ROUTE_IDS.length).toBe(62);
  });

  it('keeps the classification counts consistent with the derived registry sets', () => {
    const publicCount = CANONICAL_ROUTE_IDS.filter((id) => isPublicRoute(id)).length;
    const entryCount = CANONICAL_ROUTE_IDS.filter((id) => classifyRoute(id) === 'ENTRY').length;
    const authenticatedCount = CANONICAL_ROUTE_IDS.filter((id) => isAuthenticatedRoute(id)).length;

    expect(publicCount).toBe(12);
    expect(entryCount).toBe(1);
    expect(authenticatedCount).toBe(49);
    expect(publicCount + entryCount + authenticatedCount).toBe(62);
  });

  it('keeps the classification module free of Angular, router, guard, and navigation behavior', () => {
    const source = readTextFile('src/app/core/routing/route-classification.ts');
    const lowered = source.toLowerCase();

    expect(lowered).not.toContain('@angular');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('navigate');
    expect(lowered).not.toContain('redirect');
    expect(lowered).not.toContain('permission');
  });
});