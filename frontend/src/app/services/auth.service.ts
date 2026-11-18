import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthResult {
  token: string;
  user: { id: number; name: string; email: string };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  get token(): string | null {
    const token = localStorage.getItem('jwt_token');
    console.log('[AuthService] token getter called → token exists:', !!token);
    return token;
  }

  get user(): { id: number; name: string; email: string } | null {
    const raw = localStorage.getItem('auth_user');
    const user = raw ? JSON.parse(raw) : null;
    console.log('[AuthService] user getter called → user:', user);
    return user;
  }

  get isLoggedIn(): boolean {
    const loggedIn = !!this.token;
    console.log('[AuthService] isLoggedIn getter called → isLoggedIn:', loggedIn);
    return loggedIn;
  }

  login(email: string, password: string): Observable<AuthResult> {
    console.log('[AuthService] login() called with email:', email);
    return this.http.post<AuthResult>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(res => {
        console.log('[AuthService] login() response received ✓', { userId: res.user.id, userName: res.user.name, tokenLength: res.token.length });
        this.setSession(res);
      })
    );
  }

  register(name: string, email: string, password: string): Observable<AuthResult> {
    console.log('[AuthService] register() called with name:', name, 'email:', email);
    return this.http.post<AuthResult>(`${this.apiUrl}/register`, { name, email, password }).pipe(
      tap(res => {
        console.log('[AuthService] register() response received ✓', { userId: res.user.id, userName: res.user.name, tokenLength: res.token.length });
        this.setSession(res);
      })
    );
  }

  logout(): void {
    console.log('[AuthService] logout() called');
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('auth_user');
    console.log('[AuthService] logout() complete ✓ - token and user cleared');
  }

  private setSession(result: AuthResult): void {
    console.log('[AuthService] setSession() called → storing token and user');
    localStorage.setItem('jwt_token', result.token);
    localStorage.setItem('auth_user', JSON.stringify(result.user));
    console.log('[AuthService] setSession() complete ✓ - session stored in localStorage');
  }
}
