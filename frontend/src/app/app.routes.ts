import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth/sign-in',
    loadComponent: () => import('./features/auth/sign-in/sign-in').then((m) => m.SignIn),
  },
];
