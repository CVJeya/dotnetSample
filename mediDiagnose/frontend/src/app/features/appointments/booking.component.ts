import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { AppointmentService } from '../../core/api/appointment.service';
import { DoctorService } from '../../core/api/doctor.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { appointmentTimezoneOffset } from '../../core/api/appointment-timezone';
import { Appointment, DoctorProfile } from '../../core/models/api.models';

@Component({
  selector: 'app-booking',
  imports: [DatePipe, RouterLink],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly appointments = inject(AppointmentService);
  private readonly doctorService = inject(DoctorService);
  protected readonly doctorId = this.route.snapshot.paramMap.get('doctorId');
  protected readonly startsAt = this.route.snapshot.queryParamMap.get('startsAt');
  protected readonly doctor = signal<DoctorProfile | null>(null);
  protected readonly appointment = signal<Appointment | null>(null);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly timezoneOffset = appointmentTimezoneOffset;

  constructor() {
    if (!this.doctorId || !this.startsAt) {
      this.error.set('The appointment time is missing. Return to the doctor’s schedule and choose a time.');
      this.loading.set(false);
      return;
    }

    this.doctorService.getDoctor(this.doctorId).subscribe({
      next: (doctor) => {
        this.doctor.set(doctor);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'The doctor’s profile could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  protected requestAppointment(): void {
    if (!this.doctorId || !this.startsAt || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.appointments.requestAppointment(this.doctorId, this.startsAt).subscribe({
      next: (appointment) => {
        this.appointment.set(appointment);
        this.submitting.set(false);
      },
      error: (error: unknown) => {
        this.error.set(
          apiErrorMessage(
            error,
            'This time may no longer be available. Check the schedule and choose another.',
          ),
        );
        this.submitting.set(false);
      },
    });
  }
}
