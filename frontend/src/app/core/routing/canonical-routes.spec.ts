// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import {
  buildAdminExamDetailPath,
  buildAdminExamQuestionsPath,
  buildAdminExamVersionsPath,
  buildAdminUserDetailPath,
  buildCanonicalRoutePath,
  buildCommerceOrderDetailPath,
  buildCommercePaymentFailurePath,
  buildCommercePaymentSuccessPath,
  buildCommerceProductDetailPath,
  buildEmployerRequestDetailPath,
  buildExamsDetailPath,
  buildExamsInstructionsPath,
  buildExamsResultPath,
  buildExamsReviewPath,
  buildExamsSessionPath,
  buildPreparationPackageEntitlementDetailPath,
  buildPreparationPackageOfferDetailPath,
  buildPreparationPackagePracticePath,
  buildPreparationPackageReportPath,
  CANONICAL_ROUTES,
  canonicalRoutePath,
} from './canonical-routes';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function stripCellWrapping(value: string): string {
  return value.trim().replace(/^`+|`+$/g, '').trim();
}

function parseApprovedCanonicalFromRegistry(): Map<string, string> {
  const text = readTextFile('../docs/frontend/design/inventory/page-registry.md');
  const parsed = new Map<string, string>();
  for (const line of text.split('\n')) {
    if (!line.trimStart().startsWith('| `')) {
      continue;
    }
    const cells = line.split('|');
    if (cells.length < 10) {
      continue;
    }
    const routeId = stripCellWrapping(cells[1]);
    const approvedPath = stripCellWrapping(cells[4]);
    const status = stripCellWrapping(cells[8]);
    if (!status.includes('APPROVED_CANONICAL')) {
      continue;
    }
    if (approvedPath === '' || approvedPath === '—') {
      continue;
    }
    parsed.set(routeId, approvedPath);
  }
  return parsed;
}

const EXPECTED_APPROVED_CANONICAL: Readonly<Record<string, string>> = {
  ROOT_ENTRY: '/',
  AUTH_SIGN_IN: '/auth/sign-in',
  AUTH_ROLE_SELECTION: '/auth/role-selection',
  AUTH_REGISTER_NURSE: '/auth/register/nurse',
  AUTH_REGISTER_EMPLOYER: '/auth/register/employer',
  AUTH_VERIFY_EMAIL_REQUEST: '/auth/verify-email',
  AUTH_VERIFY_EMAIL_CONFIRM: '/auth/verify-email/confirm',
  AUTH_FORGOT_PASSWORD: '/auth/forgot-password',
  AUTH_RESET_PASSWORD: '/auth/reset-password',
  SYSTEM_SESSION_EXPIRED: '/session-expired',
  SYSTEM_ACCESS_DENIED: '/access-denied',
  ACCOUNT_OVERVIEW: '/account',
  NURSE_ENTRY: '/nurse',
  NURSE_PROFILE_OVERVIEW: '/nurse/profile',
  NURSE_PROFILE_PERSONAL_INFORMATION: '/nurse/profile/personal-information',
  NURSE_PROFILE_EXPERIENCE: '/nurse/profile/experience',
  NURSE_PROFILE_EDUCATION: '/nurse/profile/education',
  NURSE_PROFILE_CERTIFICATES: '/nurse/profile/certificates',
  NURSE_PROFILE_SKILLS: '/nurse/profile/skills',
  NURSE_PROFILE_LANGUAGES: '/nurse/profile/languages',
  NURSE_PROFILE_CV: '/nurse/profile/cv',
  NURSE_CONTACT_REQUESTS: '/nurse/contact-requests',
  EMPLOYER_HOME: '/employer',
  EMPLOYER_CANDIDATES: '/employer/candidates',
  EMPLOYER_REQUESTS: '/employer/requests',
  EMPLOYER_REQUEST_DETAIL: '/employer/requests/:requestId',
  EXAMS_CATALOG: '/exams',
  EXAMS_DETAIL: '/exams/:examId',
  EXAMS_INSTRUCTIONS: '/exams/:examId/instructions',
  EXAMS_SESSION: '/exams/:examId/sessions/:sessionId',
  EXAMS_RESULT: '/exams/:examId/sessions/:sessionId/result',
  EXAMS_ANALYTICS: '/exams/analytics',
  EXAMS_REVIEW: '/exams/:examId/sessions/:sessionId/review',
  EXAMS_HISTORY: '/exams/history',
  PREPARATION_PACKAGES_OFFERS: '/preparation-packages',
  PREPARATION_PACKAGES_OFFER_DETAIL: '/preparation-packages/:offerSlug',
  PREPARATION_PACKAGES_ENTITLEMENTS: '/nurse/preparation-packages',
  PREPARATION_PACKAGES_ENTITLEMENT_DETAIL: '/nurse/preparation-packages/:entitlementId',
  PREPARATION_PACKAGES_PRACTICE: '/nurse/preparation-packages/:entitlementId/practice',
  PREPARATION_PACKAGES_REPORT: '/nurse/preparation-packages/reports/:sessionId',
  COMMERCE_PRODUCTS: '/commerce/products',
  COMMERCE_PRODUCT_DETAIL: '/commerce/products/:productId',
  COMMERCE_CHECKOUT: '/checkout',
  COMMERCE_PAYMENT_SUCCESS: '/checkout/orders/:orderId/success',
  COMMERCE_PAYMENT_FAILURE: '/checkout/orders/:orderId/failure',
  COMMERCE_ORDERS: '/commerce/orders',
  COMMERCE_ORDER_DETAIL: '/commerce/orders/:orderId',
  ADMIN_ENTRY: '/admin',
  ADMIN_USERS: '/admin/users',
  ADMIN_USER_DETAIL: '/admin/users/:userId',
  ADMIN_REFERENCE_DATA: '/admin/reference-data',
  ADMIN_EXAMS: '/admin/exams',
  ADMIN_EXAM_DETAIL: '/admin/exams/:examId',
  ADMIN_EXAM_VERSIONS: '/admin/exams/:examId/versions',
  ADMIN_EXAM_QUESTIONS: '/admin/exams/:examId/questions',
  ADMIN_PAYMENT_PRODUCTS: '/admin/payment-products',
  ADMIN_PREPARATION_PACKAGE_TOPICS: '/admin/preparation-packages/topics',
  ADMIN_PREPARATION_PACKAGE_PROFILES: '/admin/preparation-packages/profiles',
  ADMIN_PREPARATION_PACKAGE_MATERIALS: '/admin/preparation-packages/materials',
  ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS:
    '/admin/preparation-packages/practice-collections',
  ADMIN_PREPARATION_PACKAGE_DEFINITIONS: '/admin/preparation-packages/definitions',
  ADMIN_PREPARATION_PACKAGE_OFFERS: '/admin/preparation-packages/offers',
};

const EXCLUDED_ROUTE_IDS = [
  'AUTH_RESET_PASSWORD_SUCCESS',
  'SYSTEM_NOT_FOUND',
  'SYSTEM_UNEXPECTED_ERROR',
  'ACCOUNT_DETAILS',
  'PREPARATION_PACKAGES_EXAM_START',
  'COMMERCE_PAYMENT_PROCESSING',
];

const FORBIDDEN_PATHS = [
  '/auth/reset-password/success',
  '/not-found',
  '/error',
  '/account/details',
  '/nurse/preparation-packages/:entitlementId/exam',
  '/checkout/orders/:orderId/processing',
];

const APPROVED_PARAMETERS = new Set([
  'requestId',
  'examId',
  'sessionId',
  'offerSlug',
  'entitlementId',
  'productId',
  'orderId',
  'userId',
]);

describe('canonical-routes', () => {
  it('matches the approved exact-path rows in page-registry.md mechanically', () => {
    const parsed = parseApprovedCanonicalFromRegistry();

    expect(parsed.size).toBe(62);
    expect(Object.fromEntries(parsed)).toEqual(EXPECTED_APPROVED_CANONICAL);
    expect({ ...CANONICAL_ROUTES }).toEqual(EXPECTED_APPROVED_CANONICAL);
  });

  it('contains exactly 62 entries with unique IDs and unique templates', () => {
    const ids = Object.keys(CANONICAL_ROUTES);
    const templates = Object.values(CANONICAL_ROUTES);

    expect(ids).toHaveLength(62);
    expect(new Set(ids).size).toBe(62);
    expect(new Set(templates).size).toBe(62);
  });

  it('exposes every template through the typed accessor', () => {
    for (const [id, template] of Object.entries(EXPECTED_APPROVED_CANONICAL)) {
      expect(canonicalRoutePath(id as keyof typeof CANONICAL_ROUTES)).toBe(template);
    }
  });

  it('keeps NURSE_ENTRY and ADMIN_ENTRY without inventing home dashboards', () => {
    expect(CANONICAL_ROUTES['NURSE_ENTRY' as keyof typeof CANONICAL_ROUTES]).toBe('/nurse');
    expect(CANONICAL_ROUTES['ADMIN_ENTRY' as keyof typeof CANONICAL_ROUTES]).toBe('/admin');

    const ids = Object.keys(CANONICAL_ROUTES);
    expect(ids).not.toContain('NURSE_HOME');
    expect(ids).not.toContain('ADMIN_HOME');
  });

  it('applies path hygiene to every template', () => {
    for (const [id, template] of Object.entries(CANONICAL_ROUTES)) {
      expect(template.startsWith('/')).toBe(true);
      if (template !== '/') {
        expect(template.endsWith('/')).toBe(false);
      }
      expect(template).not.toContain('//');
      expect(template).not.toContain('/api/v1');

      const segments = template.split('/').filter((segment) => segment !== '');
      for (const segment of segments) {
        if (segment.startsWith(':')) {
          continue;
        }
        expect(segment).toMatch(/^[a-z0-9]+(-[a-z0-9]+)*$/);
        expect(segment).not.toBe('en');
        expect(segment).not.toBe('ar');
      }

      expect(template).not.toMatch(/[A-Z]+-\d+/);
      expect(template).not.toContain(id);
      expect(template).not.toMatch(/\/:id(?![A-Za-z])/);
    }
  });

  it('uses only approved semantic dynamic parameters', () => {
    for (const template of Object.values(CANONICAL_ROUTES)) {
      for (const match of template.matchAll(/:([A-Za-z]+)/g)) {
        expect(APPROVED_PARAMETERS.has(match[1])).toBe(true);
      }
    }
  });

  it('excludes non-routable, blocked, and deferred destinations', () => {
    const ids = Object.keys(CANONICAL_ROUTES);
    const templates = Object.values(CANONICAL_ROUTES);

    for (const excludedId of EXCLUDED_ROUTE_IDS) {
      expect(ids).not.toContain(excludedId);
    }
    for (const forbiddenPath of FORBIDDEN_PATHS) {
      expect(templates).not.toContain(forbiddenPath);
    }
  });

  it('builds every dynamic route deterministically with segment encoding', () => {
    expect(buildEmployerRequestDetailPath('request-1')).toBe('/employer/requests/request-1');
    expect(buildExamsDetailPath('exam-1')).toBe('/exams/exam-1');
    expect(buildExamsInstructionsPath('exam-1')).toBe('/exams/exam-1/instructions');
    expect(buildExamsSessionPath('exam-1', 'session-2')).toBe('/exams/exam-1/sessions/session-2');
    expect(buildExamsResultPath('exam-1', 'session-2')).toBe(
      '/exams/exam-1/sessions/session-2/result',
    );
    expect(buildExamsReviewPath('exam-1', 'session-2')).toBe(
      '/exams/exam-1/sessions/session-2/review',
    );
    expect(buildPreparationPackageOfferDetailPath('offer-slug')).toBe(
      '/preparation-packages/offer-slug',
    );
    expect(buildPreparationPackageEntitlementDetailPath('entitlement-1')).toBe(
      '/nurse/preparation-packages/entitlement-1',
    );
    expect(buildPreparationPackagePracticePath('entitlement-1')).toBe(
      '/nurse/preparation-packages/entitlement-1/practice',
    );
    expect(buildPreparationPackageReportPath('session-9')).toBe(
      '/nurse/preparation-packages/reports/session-9',
    );
    expect(buildCommerceProductDetailPath('product-1')).toBe('/commerce/products/product-1');
    expect(buildCommercePaymentSuccessPath('order-1')).toBe('/checkout/orders/order-1/success');
    expect(buildCommercePaymentFailurePath('order-1')).toBe('/checkout/orders/order-1/failure');
    expect(buildCommerceOrderDetailPath('order-1')).toBe('/commerce/orders/order-1');
    expect(buildAdminUserDetailPath('user-1')).toBe('/admin/users/user-1');
    expect(buildAdminExamDetailPath('exam-1')).toBe('/admin/exams/exam-1');
    expect(buildAdminExamVersionsPath('exam-1')).toBe('/admin/exams/exam-1/versions');
    expect(buildAdminExamQuestionsPath('exam-1')).toBe('/admin/exams/exam-1/questions');

    expect(buildExamsSessionPath('exam-1', 'session-2')).toBe(
      buildExamsSessionPath('exam-1', 'session-2'),
    );
    expect(buildExamsDetailPath('a b/c')).toBe('/exams/a%20b%2Fc');
    expect(buildCommerceOrderDetailPath('order 1/2')).toBe('/commerce/orders/order%201%2F2');
  });

  it('preserves multiple-parameter order for session routes', () => {
    expect(buildExamsSessionPath('exam-a', 'session-b')).toBe('/exams/exam-a/sessions/session-b');
    expect(buildExamsSessionPath('session-b', 'exam-a')).toBe('/exams/session-b/sessions/exam-a');
    expect(buildExamsResultPath('exam-a', 'session-b')).not.toBe(
      buildExamsResultPath('session-b', 'exam-a'),
    );
  });

  it('supports the generic builder for every dynamic template', () => {
    expect(buildCanonicalRoutePath('EXAMS_DETAIL', { examId: 'exam-1' })).toBe('/exams/exam-1');
    expect(
      buildCanonicalRoutePath('EXAMS_SESSION', { examId: 'exam-1', sessionId: 'session-2' }),
    ).toBe('/exams/exam-1/sessions/session-2');
    expect(buildCanonicalRoutePath('EXAMS_CATALOG')).toBe('/exams');
    expect(buildCanonicalRoutePath('ROOT_ENTRY')).toBe('/');

    expect(() => buildCanonicalRoutePath('EXAMS_DETAIL')).toThrow();
    expect(() => buildCanonicalRoutePath('EXAMS_DETAIL', {})).toThrow();
  });

  it('keeps templates immutable when builders run', () => {
    const before = { ...CANONICAL_ROUTES };

    buildExamsSessionPath('exam-1', 'session-2');
    buildCanonicalRoutePath('EXAMS_RESULT', { examId: 'exam-1', sessionId: 'session-2' });

    expect({ ...CANONICAL_ROUTES }).toEqual(before);
    expect(Object.isFrozen(CANONICAL_ROUTES)).toBe(true);
  });

  it('activates only the approved public Auth routes while leaving downstream route work untouched', () => {
    const appRoutes = readTextFile('src/app/app.routes.ts');

    expect(appRoutes).toContain("path: 'auth/sign-in'");
    expect(appRoutes).toContain("path: 'auth/forgot-password'");
    expect(appRoutes).toContain('loadComponent');
    expect(appRoutes).toContain("./features/auth/sign-in/sign-in");
    expect(appRoutes).toContain("./features/auth/forgot-password/forgot-password");
    expect(appRoutes).not.toContain('auth/register');
    expect(appRoutes).not.toContain('auth/reset-password');
    expect(appRoutes).not.toContain('canActivate');
    expect(appRoutes).not.toContain('redirectTo');
  });

  it('keeps the registry free of guard, redirect, navigation, and UI behavior', () => {
    const source = readTextFile('src/app/core/routing/canonical-routes.ts').toLowerCase();

    expect(source).not.toContain('@angular');
    expect(source).not.toContain('guard');
    expect(source).not.toContain('canactivate');
    expect(source).not.toContain('canmatch');
    expect(source).not.toContain('redirect');
    expect(source).not.toContain('navigate');
    expect(source).not.toContain('router');
    expect(source).not.toContain('permission');
    expect(source).not.toContain('breadcrumb');
    expect(source).not.toContain('menu');
    expect(source).not.toContain('snackbar');
    expect(source).not.toContain('httpclient');
  });
});
