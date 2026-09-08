import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideApiConfig } from './core/api/api-config';
import { provideAuthSessionBootstrap } from './core/auth/auth-session-bootstrap';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideApiConfig(),
    provideAuthSessionBootstrap()
  ]
};
