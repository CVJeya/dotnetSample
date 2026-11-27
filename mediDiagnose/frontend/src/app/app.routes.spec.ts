import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from './app.routes';

describe('staff route access', () => {
  beforeEach(() => {
    sessionStorage.setItem(
      'medi-diagnose.auth',
      JSON.stringify({
        accessToken: 'patient-token',
        userId: 'patient-1',
        role: 'Patient',
      }),
    );
  });

  afterEach(() => sessionStorage.clear());

  it('blocks patients from admin and doctor workspaces', async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideRouter(routes)],
    });

    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/admin/appointments');
    expect(harness.routeNativeElement?.textContent ?? '').toContain(
      'isn’t available for your account',
    );

    const router = TestBed.inject(Router);
    await router.navigateByUrl('/doctor/schedule');
    expect(router.url).toBe('/access-denied');
  });
});
