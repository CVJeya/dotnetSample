import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { map, switchMap } from 'rxjs';
import { AdminService } from '../../core/api/admin.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { appointmentTimezoneOffset } from '../../core/api/appointment-timezone';
import { AppointmentStatus, StaffAppointment } from '../../core/models/api.models';

interface AppointmentQueueItem extends StaffAppointment {
  doctorName: string;
}

@Component({
  selector: 'app-appointment-queue',
  imports: [DatePipe],
  templateUrl: './appointment-queue.component.html',
  styleUrl: './appointment-queue.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentQueueComponent {
  private readonly admin = inject(AdminService);
  protected readonly appointments = signal<AppointmentQueueItem[]>([]);
  protected readonly status = signal<AppointmentStatus>('Pending');
  protected readonly loading = signal(true);
  protected readonly decidingId = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly timezoneOffset = appointmentTimezoneOffset;

  constructor() {
    this.loadAppointments();
  }

  protected changeStatus(event: Event): void {
    if (!(event.target instanceof HTMLSelectElement)) {
      return;
    }

    const value = event.target.value;
    if (value === 'Pending' || value === 'Approved' || value === 'Declined') {
      this.status.set(value);
      this.loadAppointments();
    }
  }

  protected loadAppointments(): void {
    this.loading.set(true);
    this.error.set(null);
    this.admin
      .getAppointments(this.status())
      .pipe(
        switchMap((appointments) =>
          this.admin.getDoctorProfiles().pipe(
            map((doctors) =>
              appointments.map((appointment) => ({
                ...appointment,
                doctorName:
                  doctors.find((doctor) => doctor.id === appointment.doctorId)?.name ??
                  'Doctor profile unavailable',
              })),
            ),
          ),
        ),
      )
      .subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(
          apiErrorMessage(error, 'Appointment requests could not be loaded. Please try again.'),
        );
        this.loading.set(false);
      },
      });
  }

  protected decide(
    appointment: AppointmentQueueItem,
    decision: 'approve' | 'decline',
  ): void {
    if (appointment.status !== 'Pending' || this.decidingId()) {
      return;
    }

    this.decidingId.set(appointment.id);
    this.error.set(null);
    this.notice.set(null);
    const request =
      decision === 'approve'
        ? this.admin.approveAppointment(appointment.id)
        : this.admin.declineAppointment(appointment.id);

    request.subscribe({
      next: (updated) => {
        this.appointments.update((items) =>
          items.map((item) =>
            item.id === updated.id ? { ...item, status: updated.status } : item,
          ),
        );
        this.notice.set(`Appointment ${updated.status.toLowerCase()}.`);
        this.decidingId.set(null);
      },
      error: (error: unknown) => {
        this.error.set(
          apiErrorMessage(error, 'The decision was not saved. Refresh the queue and try again.'),
        );
        this.decidingId.set(null);
      },
    });
  }
}
