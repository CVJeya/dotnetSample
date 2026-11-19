import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AppointmentService } from '../../core/api/appointment.service';
import { MyAppointmentsComponent } from './my-appointments.component';

describe('MyAppointmentsComponent', () => {
  let fixture: ComponentFixture<MyAppointmentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyAppointmentsComponent],
      providers: [
        {
          provide: AppointmentService,
          useValue: {
            getMyAppointments: () =>
              of([
                {
                  id: 'pending-1',
                  doctorId: 'doctor-1',
                  patientId: 'patient-1',
                  startsAt: '2026-12-01T09:30:00+05:30',
                  endsAt: '2026-12-01T09:45:00+05:30',
                  shift: 'Morning',
                  status: 'Pending',
                },
                {
                  id: 'approved-1',
                  doctorId: 'doctor-2',
                  patientId: 'patient-1',
                  startsAt: '2026-12-02T10:00:00+05:30',
                  endsAt: '2026-12-02T10:15:00+05:30',
                  shift: 'Morning',
                  status: 'Approved',
                },
                {
                  id: 'declined-1',
                  doctorId: 'doctor-3',
                  patientId: 'patient-1',
                  startsAt: '2026-12-03T11:00:00+05:30',
                  endsAt: '2026-12-03T11:15:00+05:30',
                  shift: 'Morning',
                  status: 'Declined',
                },
              ]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MyAppointmentsComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('renders pending, approved, and declined appointment states', () => {
    const statuses = Array.from(
      fixture.nativeElement.querySelectorAll('[data-status]') as NodeListOf<HTMLElement>,
    ).map((element) => element.textContent?.trim());

    expect(statuses).toEqual(['Pending', 'Approved', 'Declined']);
  });
});
