import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { AdminService } from '../../core/api/admin.service';
import { AppointmentQueueComponent } from './appointment-queue.component';

describe('AppointmentQueueComponent', () => {
  let fixture: ComponentFixture<AppointmentQueueComponent>;
  const pendingAppointment = {
    id: 'appointment-1',
    doctorId: 'doctor-1',
    patientId: 'patient-1',
    startsAt: '2026-12-01T09:30:00+05:30',
    endsAt: '2026-12-01T09:45:00+05:30',
    shift: 'Morning',
    status: 'Pending',
    patientName: 'Avery Patient',
    patientEmail: 'avery@example.test',
    patientPhoneNumber: '555-0199',
  };
  const approveAppointment = vi.fn(() =>
    of({ ...pendingAppointment, status: 'Approved' }),
  );

  beforeEach(async () => {
    approveAppointment.mockClear();
    await TestBed.configureTestingModule({
      imports: [AppointmentQueueComponent],
      providers: [
        provideRouter([]),
        {
          provide: AdminService,
          useValue: {
            getAppointments: () => of([pendingAppointment]),
            getDoctorProfiles: () =>
              of([
                {
                  id: 'doctor-1',
                  name: 'Dr. Morgan Lee',
                  specialty: 'Cardiology',
                  biography: '',
                },
              ]),
            approveAppointment,
            declineAppointment: vi.fn(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentQueueComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('shows pending appointment details and confirms decisions through the API', async () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Appointment requests');
    expect(element.textContent).toContain('Avery Patient');
    expect(element.textContent).toContain('Dr. Morgan Lee');
    expect(element.textContent).toContain('Pending');

    const approveButton = element.querySelector(
      'button[aria-label="Approve appointment"]',
    );
    expect(approveButton).toBeTruthy();
    approveButton?.dispatchEvent(new Event('click'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(approveAppointment).toHaveBeenCalledWith('appointment-1');
    expect(element.textContent).toContain('Approved');
  });
});
