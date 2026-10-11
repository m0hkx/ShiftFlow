import { HttpClient } from '@angular/common/http';
import { inject, Service, signal, computed } from '@angular/core';
import { Observable, finalize, shareReplay, tap } from 'rxjs';

import type { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from './auth.models';

@Service()
export class Auth {
    private http = inject(HttpClient);

    // The access token lives in memory only. The refresh token is an HttpOnly cookie
    // that the browser sends to /api/auth/* by itself.
    private token = signal<string | null>(null);
    readonly user = signal<CurrentUser | null>(null);
    readonly isLoggedIn = computed(() => this.token() !== null);

    // Shared so several requests failing with 401 at once trigger a single refresh.
    private refreshing$?: Observable<AuthResponse>;

    getToken() {
        return this.token();
    }

    login(request: LoginRequest) {
        return this.http.post<AuthResponse>('/api/auth/login', request).pipe(
            tap(res => this.setSession(res))
        );
    }

    register(request: RegisterRequest) {
        return this.http.post<AuthResponse>('/api/auth/register', request).pipe(
            tap(res => this.setSession(res))
        );
    }

    refresh() {
        this.refreshing$ ??= this.http.post<AuthResponse>('/api/auth/refresh', {}).pipe(
            tap(res => this.setSession(res)),
            finalize(() => this.refreshing$ = undefined),
            shareReplay(1)
        );

        return this.refreshing$;
    }

    logout() {
        return this.http.post('/api/auth/logout', {}).pipe(
            finalize(() => this.clearSession())
        );
    }

    clearSession() {
        this.token.set(null);
        this.user.set(null);
    }

    private setSession(res: AuthResponse) {
        this.token.set(res.token);
        this.user.set({ username: res.username, role: res.role });
    }
}
