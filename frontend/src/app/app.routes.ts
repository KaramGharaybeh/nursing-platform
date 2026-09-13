import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth/sign-in',
    loadComponent: () => import('./features/auth/sign-in/sign-in').then((m) => m.SignIn),
  },
  {
    path: 'auth/role-selection',
    loadComponent: () => import('./features/auth/role-selection/role-selection').then((m) => m.RoleSelection),
  },
  {
    path: 'auth/register/nurse',
    loadComponent: () => import('./features/auth/register-nurse/register-nurse').then((m) => m.RegisterNurse),
  },
  {
    path: 'auth/register/employer',
    loadComponent: () => import('./features/auth/register-employer/register-employer').then((m) => m.RegisterEmployer),
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
];
