import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { StaffService } from '../../core/api/staff.service';
import { DoctorAppointmentsComponent } from './doctor-appointments.component';

describe('DoctorAppointmentsComponent', () => {
  let fixture: ComponentFixture<DoctorAppointmentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorAppointmentsComponent],
      providers: [
        provideRouter([]),
        {
          provide: StaffService,
          useValue: {
            getMyAppointments: () =>
              of([
                {
                  id: 'appointment-1',
                  doctorId: 'doctor-1',
                  patientId: 'patient-1',
                  startsAt: '2026-12-01T09:30:00+05:30',
                  endsAt: '2026-12-01T09:45:00+05:30',
                  shift: 'Morning',
                  status: 'Approved',
                  patientName: 'Avery Patient',
                  patientEmail: 'avery@example.test',
                  patientPhoneNumber: '555-0199',
                },
              ]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorAppointmentsComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('shows only the doctor appointments returned by the staff API', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('My appointments');
    expect(element.textContent).toContain('Avery Patient');
    expect(element.textContent).toContain('avery@example.test');
    expect(element.textContent).toContain('Approved');
  });
});
