import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideApiConfig } from './core/api/api-config';
import { provideCurrentUserHydration } from './core/auth/current-user-store';
import { bearerInterceptor } from './core/auth/bearer-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([bearerInterceptor])),
    provideApiConfig(),
    provideCurrentUserHydration()
  ]
};
