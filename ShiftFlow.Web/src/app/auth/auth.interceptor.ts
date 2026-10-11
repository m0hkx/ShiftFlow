import { HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

import { Auth } from './auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(Auth);
    const router = inject(Router);

    // Only our API, and not the auth endpoints themselves
    if (!req.url.startsWith('/api/') || req.url.startsWith('/api/auth/')) {
        return next(req);
    }

    const withToken = (request: HttpRequest<unknown>, token: string | null) =>
        token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

    return next(withToken(req, auth.getToken())).pipe(
        catchError(error => {
            if (error.status !== 401) return throwError(() => error);

            // The access token expired: get a new one with the cookie, then retry once
            return auth.refresh().pipe(
                catchError(refreshError => {
                    auth.clearSession();
                    router.navigate(['/login']);
                    return throwError(() => refreshError);
                }),
                switchMap(res => next(withToken(req, res.token)))
            );
        })
    );
};
