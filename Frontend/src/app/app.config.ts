import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideAppInitializer, inject, LOCALE_ID } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { registerLocaleData } from '@angular/common';
import localeEs from '@angular/common/locales/es';

import { routes } from './app.routes';
import { environment } from '../environments/environment';
import { provideApiConfiguration } from './api/api-configuration';
import { credentialsInterceptor } from './core/interceptors/credentials.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { AuthService } from './core/services/auth.service';

registerLocaleData(localeEs);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([credentialsInterceptor, errorInterceptor])),
    provideApiConfiguration(environment.apiUrl),
    provideAppInitializer(() => inject(AuthService).checkSession()),
    { provide: LOCALE_ID, useValue: 'es' },
  ],
};
