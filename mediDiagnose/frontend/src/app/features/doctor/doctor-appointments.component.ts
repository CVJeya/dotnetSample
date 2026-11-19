import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StaffService } from '../../core/api/staff.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { appointmentTimezoneOffset } from '../../core/api/appointment-timezone';
import { StaffAppointment } from '../../core/models/api.models';

@Component({
  selector: 'app-doctor-appointments',
  imports: [DatePipe, RouterLink],
  templateUrl: './doctor-appointments.component.html',
  styleUrl: './doctor-appointments.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorAppointmentsComponent {
  private readonly staff = inject(StaffService);
  protected readonly appointments = signal<StaffAppointment[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly timezoneOffset = appointmentTimezoneOffset;

  constructor() {
    this.loadAppointments();
  }

  protected loadAppointments(): void {
    this.loading.set(true);
    this.error.set(null);
    this.staff.getMyAppointments().subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(
          apiErrorMessage(error, 'Your appointments could not be loaded. Please try again.'),
        );
        this.loading.set(false);
      },
    });
  }
}
