import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay, withHttpTransferCacheOptions } from '@angular/platform-browser';
import { authInterceptor } from './core/auth-interceptor';
import { AuthService } from './core/auth.service';
import { TokenService } from './core/token.service';
import { ApiService } from './core/api.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
    provideClientHydration(
      withEventReplay(),
      withHttpTransferCacheOptions({
        // Only cache GET requests; let POST/PUT/DELETE always run in the browser
        filter: (req) => req.method === 'GET'
      })
    ),
    AuthService,
    TokenService,
    ApiService
  ]
};
