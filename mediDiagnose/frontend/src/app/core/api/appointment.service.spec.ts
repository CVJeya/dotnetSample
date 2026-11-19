import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { AppointmentService } from './appointment.service';

describe('AppointmentService', () => {
  let service: AppointmentService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AppointmentService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AppointmentService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('submits the selected offset-bearing slot as a patient appointment request', async () => {
    const startsAt = '2026-12-01T09:30:00+05:30';
    const result = firstValueFrom(service.requestAppointment('doctor-1', startsAt));

    const request = http.expectOne('/api/appointments');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ doctorId: 'doctor-1', startsAt });
    request.flush({
      id: 'appointment-1',
      doctorId: 'doctor-1',
      patientId: 'patient-1',
      startsAt,
      endsAt: '2026-12-01T09:45:00+05:30',
      shift: 'Morning',
      status: 'Pending',
    });

    await expect(result).resolves.toMatchObject({
      id: 'appointment-1',
      startsAt,
      status: 'Pending',
    });
  });
});
