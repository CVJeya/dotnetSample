import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DoctorService } from '../../core/api/doctor.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { appointmentTimezoneOffset } from '../../core/api/appointment-timezone';
import {
  AppointmentSlot,
  DoctorProfile,
} from '../../core/models/api.models';

@Component({
  selector: 'app-doctor-availability',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './doctor-availability.component.html',
  styleUrl: './doctor-availability.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorAvailabilityComponent {
  private readonly doctorService = inject(DoctorService);
  private readonly doctorId = inject(ActivatedRoute).snapshot.paramMap.get('doctorId');

  protected readonly doctor = signal<DoctorProfile | null>(null);
  protected readonly slots = signal<AppointmentSlot[]>([]);
  protected readonly loadingDoctor = signal(true);
  protected readonly loadingSlots = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly slotsError = signal<string | null>(null);
  protected readonly timezoneOffset = appointmentTimezoneOffset;
  protected dateValue = localDateToday();
  protected shiftValue: 'Morning' | 'Evening' = 'Morning';

  constructor() {
    if (!this.doctorId) {
      this.error.set('A doctor was not selected.');
      this.loadingDoctor.set(false);
      return;
    }

    this.doctorService.getDoctor(this.doctorId).subscribe({
      next: (doctor) => {
        this.doctor.set(doctor);
        this.loadingDoctor.set(false);
        this.findSlots();
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'This doctor profile could not be loaded.'));
        this.loadingDoctor.set(false);
      },
    });
  }

  protected findSlots(): void {
    if (!this.doctorId || !this.dateValue) {
      this.slotsError.set('Choose a date to view availability.');
      return;
    }

    this.loadingSlots.set(true);
    this.slotsError.set(null);
    this.slots.set([]);
    this.doctorService.getAvailability(this.doctorId, this.dateValue, this.shiftValue).subscribe({
      next: (slots) => {
        this.slots.set(slots);
        this.loadingSlots.set(false);
      },
      error: (error: unknown) => {
        this.slotsError.set(
          apiErrorMessage(error, 'Availability could not be loaded. Please try again.'),
        );
        this.loadingSlots.set(false);
      },
    });
  }
}

function localDateToday(): string {
  const date = new Date();
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60_000);
  return localDate.toISOString().slice(0, 10);
}
