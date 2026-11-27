import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../core/api/appointment.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { appointmentTimezoneOffset } from '../../core/api/appointment-timezone';
import { Appointment } from '../../core/models/api.models';

@Component({
  selector: 'app-my-appointments',
  imports: [DatePipe, RouterLink],
  templateUrl: './my-appointments.component.html',
  styleUrl: './my-appointments.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyAppointmentsComponent {
  private readonly appointmentService = inject(AppointmentService);
  protected readonly appointments = signal<Appointment[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly timezoneOffset = appointmentTimezoneOffset;

  constructor() {
    this.loadAppointments();
  }

  protected loadAppointments(): void {
    this.loading.set(true);
    this.error.set(null);
    this.appointmentService.getMyAppointments().subscribe({
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

  protected statusLabel(status: Appointment['status']): string {
    return status;
  }
}
