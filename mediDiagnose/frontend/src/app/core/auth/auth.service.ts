import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import {
  AuthSession,
  PatientProfile,
  PatientProfileInput,
  PatientRegistration,
} from '../models/api.models';

const SESSION_KEY = 'medi-diagnose.auth';
const PENDING_PROFILE_KEY = 'medi-diagnose.pending-profile';

export interface RegistrationResult {
  profileComplete: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentSession = signal<AuthSession | null>(this.readSession());
  readonly session = this.currentSession.asReadonly();
  readonly isAuthenticated = computed(() => this.currentSession() !== null);

  constructor(private readonly http: HttpClient) {}

  registerPatient(registration: PatientRegistration): Observable<RegistrationResult> {
    return this.http
      .post<AuthSession>('/api/auth/register', {
        name: registration.name,
        email: registration.email,
        password: registration.password,
      })
      .pipe(
        tap((session) => this.setSession(session)),
        switchMap(() => {
          const profile: PatientProfileInput = {
            name: registration.name,
            email: registration.email,
            phoneNumber: registration.phoneNumber,
          };

          return this.http.put<PatientProfile>('/api/patients/me', profile).pipe(
            tap(() => this.clearPendingProfile()),
            map(() => ({ profileComplete: true })),
            catchError(() => {
              this.savePendingProfile(profile);
              return of({ profileComplete: false });
            }),
          );
        }),
      );
  }

  login(credentials: { email: string; password: string }): Observable<AuthSession> {
    return this.http.post<AuthSession>('/api/auth/login', credentials).pipe(
      tap((session) => this.setSession(session)),
    );
  }

  retryProfileCompletion(): Observable<boolean> {
    const profile = this.readPendingProfile();
    if (!profile) {
      return of(true);
    }

    return this.http.put<PatientProfile>('/api/patients/me', profile).pipe(
      tap(() => this.clearPendingProfile()),
      map(() => true),
    );
  }

  getAccessToken(): string | null {
    return this.currentSession()?.accessToken ?? null;
  }

  hasPendingProfile(): boolean {
    return this.readPendingProfile() !== null;
  }

  logout(): void {
    sessionStorage.removeItem(SESSION_KEY);
    this.clearPendingProfile();
    this.currentSession.set(null);
  }

  private setSession(session: AuthSession): void {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
    this.currentSession.set(session);
  }

  private readSession(): AuthSession | null {
    const saved = sessionStorage.getItem(SESSION_KEY);
    if (!saved) {
      return null;
    }

    try {
      const session: unknown = JSON.parse(saved);
      if (this.isAuthSession(session)) {
        return session;
      }
    } catch (error) {
      if (!(error instanceof SyntaxError)) {
        throw error;
      }
    }

    sessionStorage.removeItem(SESSION_KEY);
    return null;
  }

  private isAuthSession(value: unknown): value is AuthSession {
    if (typeof value !== 'object' || value === null) {
      return false;
    }
    const session = value as Record<string, unknown>;
    return (
      typeof session['accessToken'] === 'string' &&
      typeof session['userId'] === 'string' &&
      (session['role'] === 'Patient' ||
        session['role'] === 'Doctor' ||
        session['role'] === 'Admin')
    );
  }

  private savePendingProfile(profile: PatientProfileInput): void {
    sessionStorage.setItem(PENDING_PROFILE_KEY, JSON.stringify(profile));
  }

  private readPendingProfile(): PatientProfileInput | null {
    const saved = sessionStorage.getItem(PENDING_PROFILE_KEY);
    if (!saved) {
      return null;
    }

    try {
      const profile: unknown = JSON.parse(saved);
      if (this.isPatientProfileInput(profile)) {
        return profile;
      }
    } catch (error) {
      if (!(error instanceof SyntaxError)) {
        throw error;
      }
    }

    sessionStorage.removeItem(PENDING_PROFILE_KEY);
    return null;
  }

  private isPatientProfileInput(value: unknown): value is PatientProfileInput {
    if (typeof value !== 'object' || value === null) {
      return false;
    }
    const profile = value as Record<string, unknown>;
    return (
      typeof profile['name'] === 'string' &&
      typeof profile['email'] === 'string' &&
      typeof profile['phoneNumber'] === 'string'
    );
  }

  private clearPendingProfile(): void {
    sessionStorage.removeItem(PENDING_PROFILE_KEY);
  }
}
