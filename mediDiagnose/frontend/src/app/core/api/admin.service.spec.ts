import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { AdminService } from './admin.service';

describe('AdminService', () => {
  let service: AdminService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AdminService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AdminService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('confirms an appointment decision without sending an unused request body', async () => {
    const result = firstValueFrom(service.approveAppointment('appointment-1'));
    const request = http.expectOne('/api/admin/appointments/appointment-1/approve');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush({
      id: 'appointment-1',
      doctorId: 'doctor-1',
      patientId: 'patient-1',
      startsAt: '2026-12-01T09:30:00+05:30',
      endsAt: '2026-12-01T09:45:00+05:30',
      shift: 'Morning',
      status: 'Approved',
    });

    await expect(result).resolves.toMatchObject({ status: 'Approved' });
  });

  it('declines an appointment without sending an unused request body', async () => {
    const result = firstValueFrom(service.declineAppointment('appointment-1'));
    const request = http.expectOne('/api/admin/appointments/appointment-1/decline');

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush({
      id: 'appointment-1',
      doctorId: 'doctor-1',
      patientId: 'patient-1',
      startsAt: '2026-12-01T09:30:00+05:30',
      endsAt: '2026-12-01T09:45:00+05:30',
      shift: 'Morning',
      status: 'Declined',
    });

    await expect(result).resolves.toMatchObject({ status: 'Declined' });
  });
});
