import { Routes } from '@angular/router';
import { authenticatedRouteGuard } from './core/auth/authenticated-route.guard';
import { profileCompletionGuard } from './core/auth/profile-completion.guard';
import { routePermissionGuard } from './core/routing/route-permission.guard';

export const routes: Routes = [
  {
    path: 'auth/sign-in',
    loadComponent: () => import('./features/auth/sign-in/sign-in').then((m) => m.SignIn),
  },
  {
    path: 'auth/sign-up',
    loadComponent: () => import('./features/auth/sign-up/sign-up').then((m) => m.SignUp),
  },
  {
    path: 'auth/role-selection',
    redirectTo: 'auth/sign-up',
    pathMatch: 'full',
  },
  {
    path: 'auth/register/nurse',
    redirectTo: 'auth/sign-up',
    pathMatch: 'full',
  },
  {
    path: 'auth/register/employer',
    redirectTo: 'auth/sign-up',
    pathMatch: 'full',
  },
  {
    path: 'auth/verify-email',
    loadComponent: () => import('./features/auth/check-email/check-email').then((m) => m.CheckEmail),
  },
  {
    path: 'auth/verify-email/confirm',
    loadComponent: () => import('./features/auth/verify-email/verify-email').then((m) => m.VerifyEmail),
  },
  {
    path: 'auth/forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
  },
  {
    path: 'auth/reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password').then((m) => m.ResetPassword),
  },
  {
    path: 'session-expired',
    loadComponent: () => import('./features/auth/session-expired/session-expired').then((m) => m.SessionExpired),
  },
  {
    path: 'access-denied',
    loadComponent: () => import('./features/auth/access-denied/access-denied').then((m) => m.AccessDenied),
  },
  {
    path: 'account',
    loadComponent: () => import('./features/account/account').then((m) => m.Account),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'ACCOUNT_OVERVIEW' },
  },
  {
    path: 'onboarding/profile',
    loadComponent: () => import('./features/onboarding/profile/profile-onboarding').then((m) => m.ProfileOnboarding),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'ONBOARDING_PROFILE' },
  },
  {
    path: 'admin/users',
    loadComponent: () => import('./features/admin/users/admin-users').then((m) => m.AdminUsers),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'ADMIN_USERS' },
  },
  {
    path: 'admin/users/:userId',
    loadComponent: () => import('./features/admin/users/admin-user-detail').then((m) => m.AdminUserDetail),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'ADMIN_USER_DETAIL' },
  },
  {
    path: 'nurse',
    redirectTo: 'nurse/profile',
    pathMatch: 'full',
  },
  {
    path: 'nurse/profile',
    loadComponent: () => import('./features/nurse/profile/overview/nurse-profile-overview').then((m) => m.NurseProfileOverview),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_OVERVIEW' },
  },
  {
    path: 'nurse/profile/personal-information',
    loadComponent: () => import('./features/nurse/profile/personal-information/nurse-personal-information').then((m) => m.NursePersonalInformation),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_PERSONAL_INFORMATION' },
  },
  {
    path: 'nurse/profile/experience',
    loadComponent: () => import('./features/nurse/profile/experience/nurse-experience').then((m) => m.NurseExperience),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_EXPERIENCE' },
  },
  {
    path: 'nurse/profile/education',
    loadComponent: () => import('./features/nurse/profile/education/nurse-education').then((m) => m.NurseEducation),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_EDUCATION' },
  },
  {
    path: 'nurse/profile/certificates',
    loadComponent: () => import('./features/nurse/profile/certificates/nurse-certificates').then((m) => m.NurseCertificates),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_CERTIFICATES' },
  },
  {
    path: 'nurse/profile/skills',
    loadComponent: () => import('./features/nurse/profile/skills/nurse-skills').then((m) => m.NurseSkills),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_SKILLS' },
  },
  {
    path: 'nurse/profile/languages',
    loadComponent: () => import('./features/nurse/profile/languages/nurse-languages').then((m) => m.NurseLanguages),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_LANGUAGES' },
  },
  {
    path: 'nurse/profile/cv',
    loadComponent: () => import('./features/nurse/profile/cv/nurse-cv').then((m) => m.NurseCv),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_PROFILE_CV' },
  },
  {
    path: 'nurse/contact-requests',
    loadComponent: () => import('./features/nurse/contact-requests/nurse-contact-requests').then((m) => m.NurseContactRequests),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'NURSE_CONTACT_REQUESTS' },
  },
  {
    path: 'nurse/preparation-packages',
    loadComponent: () => import('./features/nurse/preparation-packages/nurse-entitlements-list').then((m) => m.NurseEntitlementsList),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'PREPARATION_PACKAGES_ENTITLEMENTS' },
  },
  {
    path: 'nurse/preparation-packages/:entitlementId',
    loadComponent: () => import('./features/nurse/preparation-packages/nurse-entitlement-detail').then((m) => m.NurseEntitlementDetail),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'PREPARATION_PACKAGES_ENTITLEMENT_DETAIL' },
  },
  {
    path: 'nurse/preparation-packages/:entitlementId/practice',
    loadComponent: () => import('./features/nurse/preparation-packages/practice').then((m) => m.Practice),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'PREPARATION_PACKAGES_PRACTICE' },
  },
  {
    path: 'nurse/preparation-packages/reports/:sessionId',
    loadComponent: () => import('./features/nurse/preparation-packages/package-report').then((m) => m.PackageReport),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'PREPARATION_PACKAGES_REPORT' },
  },
  {
    path: 'preparation-packages',
    loadComponent: () => import('./features/preparation-packages/offers-list').then((m) => m.OffersList),
    data: { routeId: 'PREPARATION_PACKAGES_OFFERS' },
  },
  {
    path: 'preparation-packages/:offerSlug',
    loadComponent: () => import('./features/preparation-packages/offer-detail').then((m) => m.OfferDetail),
    data: { routeId: 'PREPARATION_PACKAGES_OFFER_DETAIL' },
  },
  {
    path: 'exams',
    loadComponent: () => import('./features/exams/exams-list').then((m) => m.ExamsList),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'EXAMS_CATALOG' },
  },
  {
    path: 'exams/:examId',
    loadComponent: () => import('./features/exams/exam-detail').then((m) => m.ExamDetail),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'EXAMS_DETAIL' },
  },
  {
    path: 'exams/:examId/instructions',
    loadComponent: () => import('./features/exams/exam-instructions').then((m) => m.ExamInstructions),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'EXAMS_INSTRUCTIONS' },
  },
  {
    path: 'exams/:examId/sessions/:sessionId',
    loadComponent: () => import('./features/exams/exam-session').then((m) => m.ExamSessionScreen),
    canActivate: [authenticatedRouteGuard, profileCompletionGuard, routePermissionGuard],
    data: { routeId: 'EXAMS_SESSION' },
  },
];
