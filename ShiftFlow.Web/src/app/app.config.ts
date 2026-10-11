import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { catchError, firstValueFrom, of } from 'rxjs';

import { routes } from './app.routes';
import { Auth } from './auth/auth';
import { authInterceptor } from './auth/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    // After a page reload the in-memory token is gone, so try to get a new one
    // with the refresh cookie before the first route is checked by the guard.
    provideAppInitializer(() => {
      const auth = inject(Auth);

      return firstValueFrom(auth.refresh().pipe(catchError(() => of(null))));
    }),
  ],
};
