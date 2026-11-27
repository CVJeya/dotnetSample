import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { StaffService } from '../../core/api/staff.service';
import { DoctorScheduleComponent } from './doctor-schedule.component';

describe('DoctorScheduleComponent', () => {
  let fixture: ComponentFixture<DoctorScheduleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorScheduleComponent],
      providers: [
        provideRouter([]),
        {
          provide: StaffService,
          useValue: {
            getMySchedule: () =>
              of([
                {
                  dayOfWeek: 1,
                  shift: 'Morning',
                  startTime: '09:00:00',
                  endTime: '12:00:00',
                  appointmentDurationMinutes: 15,
                },
              ]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorScheduleComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('shows editable morning and evening schedule controls', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('My schedule');
    expect(element.textContent).toContain('Morning');
    expect(element.textContent).toContain('Evening');
    expect(element.querySelector('input[type="time"]')).toBeTruthy();
    expect(element.querySelector('button[type="submit"]')?.textContent).toContain('Save schedule');
  });
});
