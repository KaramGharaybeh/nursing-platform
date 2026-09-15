const CANONICAL_ROUTE_TABLE = {
  ROOT_ENTRY: '/',
  AUTH_SIGN_IN: '/auth/sign-in',
  AUTH_SIGN_UP: '/auth/sign-up',
  AUTH_ROLE_SELECTION: '/auth/role-selection',
  AUTH_REGISTER_NURSE: '/auth/register/nurse',
  AUTH_REGISTER_EMPLOYER: '/auth/register/employer',
  AUTH_VERIFY_EMAIL_REQUEST: '/auth/verify-email',
  AUTH_VERIFY_EMAIL_CONFIRM: '/auth/verify-email/confirm',
  AUTH_FORGOT_PASSWORD: '/auth/forgot-password',
  AUTH_RESET_PASSWORD: '/auth/reset-password',
  SYSTEM_SESSION_EXPIRED: '/session-expired',
  SYSTEM_ACCESS_DENIED: '/access-denied',
  ONBOARDING_PROFILE: '/onboarding/profile',
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
} as const;

export const CANONICAL_ROUTES: Readonly<Record<keyof typeof CANONICAL_ROUTE_TABLE, string>> =
  Object.freeze({ ...CANONICAL_ROUTE_TABLE });

export type CanonicalRouteId = keyof typeof CANONICAL_ROUTES;

export const CANONICAL_ROUTE_IDS: readonly CanonicalRouteId[] = Object.freeze(
  Object.keys(CANONICAL_ROUTES) as CanonicalRouteId[],
);

export function canonicalRoutePath(id: CanonicalRouteId): string {
  return CANONICAL_ROUTES[id];
}

export function buildCanonicalRoutePath(
  id: CanonicalRouteId,
  params?: Readonly<Record<string, string>>,
): string {
  const template = CANONICAL_ROUTES[id];
  return template.replace(/:([A-Za-z]+)/g, (match, name: string) => {
    const value = params?.[name];
    if (value === undefined) {
      throw new Error(`Missing route parameter "${name}" for canonical route "${id}".`);
    }
    return encodeURIComponent(value);
  });
}

export function buildEmployerRequestDetailPath(requestId: string): string {
  return buildCanonicalRoutePath('EMPLOYER_REQUEST_DETAIL', { requestId });
}

export function buildExamsDetailPath(examId: string): string {
  return buildCanonicalRoutePath('EXAMS_DETAIL', { examId });
}

export function buildExamsInstructionsPath(examId: string): string {
  return buildCanonicalRoutePath('EXAMS_INSTRUCTIONS', { examId });
}

export function buildExamsSessionPath(examId: string, sessionId: string): string {
  return buildCanonicalRoutePath('EXAMS_SESSION', { examId, sessionId });
}

export function buildExamsResultPath(examId: string, sessionId: string): string {
  return buildCanonicalRoutePath('EXAMS_RESULT', { examId, sessionId });
}

export function buildExamsReviewPath(examId: string, sessionId: string): string {
  return buildCanonicalRoutePath('EXAMS_REVIEW', { examId, sessionId });
}

export function buildPreparationPackageOfferDetailPath(offerSlug: string): string {
  return buildCanonicalRoutePath('PREPARATION_PACKAGES_OFFER_DETAIL', { offerSlug });
}

export function buildPreparationPackageEntitlementDetailPath(entitlementId: string): string {
  return buildCanonicalRoutePath('PREPARATION_PACKAGES_ENTITLEMENT_DETAIL', { entitlementId });
}

export function buildPreparationPackagePracticePath(entitlementId: string): string {
  return buildCanonicalRoutePath('PREPARATION_PACKAGES_PRACTICE', { entitlementId });
}

export function buildPreparationPackageReportPath(sessionId: string): string {
  return buildCanonicalRoutePath('PREPARATION_PACKAGES_REPORT', { sessionId });
}

export function buildCommerceProductDetailPath(productId: string): string {
  return buildCanonicalRoutePath('COMMERCE_PRODUCT_DETAIL', { productId });
}

export function buildCommercePaymentSuccessPath(orderId: string): string {
  return buildCanonicalRoutePath('COMMERCE_PAYMENT_SUCCESS', { orderId });
}

export function buildCommercePaymentFailurePath(orderId: string): string {
  return buildCanonicalRoutePath('COMMERCE_PAYMENT_FAILURE', { orderId });
}

export function buildCommerceOrderDetailPath(orderId: string): string {
  return buildCanonicalRoutePath('COMMERCE_ORDER_DETAIL', { orderId });
}

export function buildAdminUserDetailPath(userId: string): string {
  return buildCanonicalRoutePath('ADMIN_USER_DETAIL', { userId });
}

export function buildAdminExamDetailPath(examId: string): string {
  return buildCanonicalRoutePath('ADMIN_EXAM_DETAIL', { examId });
}

export function buildAdminExamVersionsPath(examId: string): string {
  return buildCanonicalRoutePath('ADMIN_EXAM_VERSIONS', { examId });
}

export function buildAdminExamQuestionsPath(examId: string): string {
  return buildCanonicalRoutePath('ADMIN_EXAM_QUESTIONS', { examId });
}
