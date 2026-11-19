import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('creates a patient profile after registration and allows retry when profile creation fails', async () => {
    const registration = {
      name: 'Avery Patient',
      email: 'avery@example.test',
      password: 'secure-password',
      phoneNumber: '555-0199',
    };
    const result = firstValueFrom(service.registerPatient(registration));

    const authRequest = http.expectOne('/api/auth/register');
    expect(authRequest.request.method).toBe('POST');
    expect(authRequest.request.body).toEqual({
      name: registration.name,
      email: registration.email,
      password: registration.password,
    });
    authRequest.flush({
      accessToken: 'patient-token',
      userId: 'patient-1',
      role: 'Patient',
    });

    const profileRequest = http.expectOne('/api/patients/me');
    expect(profileRequest.request.method).toBe('PUT');
    expect(profileRequest.request.body).toEqual({
      name: registration.name,
      email: registration.email,
      phoneNumber: registration.phoneNumber,
    });
    profileRequest.flush(
      { message: 'Temporary failure' },
      { status: 503, statusText: 'Unavailable' },
    );

    await expect(result).resolves.toEqual({ profileComplete: false });
    expect(sessionStorage.getItem('medi-diagnose.auth')).toContain('patient-token');

    const retry = firstValueFrom(service.retryProfileCompletion());
    const retryRequest = http.expectOne('/api/patients/me');
    expect(retryRequest.request.body).toEqual({
      name: registration.name,
      email: registration.email,
      phoneNumber: registration.phoneNumber,
    });
    retryRequest.flush({
      userId: 'patient-1',
      ...registration,
      isActive: true,
    });

    await expect(retry).resolves.toBe(true);
    expect(sessionStorage.getItem('medi-diagnose.pending-profile')).toBeNull();
  });

  it('stores the returned session after a successful login', async () => {
    const result = firstValueFrom(
      service.login({ email: 'avery@example.test', password: 'secure-password' }),
    );

    const request = http.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    request.flush({
      accessToken: 'login-token',
      userId: 'patient-1',
      role: 'Patient',
    });

    await expect(result).resolves.toEqual({
      accessToken: 'login-token',
      userId: 'patient-1',
      role: 'Patient',
    });
    expect(sessionStorage.getItem('medi-diagnose.auth')).toContain('login-token');
  });
});
