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
];
