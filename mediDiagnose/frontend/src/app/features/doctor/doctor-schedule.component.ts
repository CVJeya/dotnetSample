import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { StaffService } from '../../core/api/staff.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { DoctorShift } from '../../core/models/api.models';

type ShiftName = DoctorShift['shift'];

type ShiftForm = FormGroup<{
  dayOfWeek: FormControl<number>;
  shift: FormControl<ShiftName>;
  startTime: FormControl<string>;
  endTime: FormControl<string>;
  appointmentDurationMinutes: FormControl<number>;
}>;

const DAYS_OF_WEEK = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
];

@Component({
  selector: 'app-doctor-schedule',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './doctor-schedule.component.html',
  styleUrl: './doctor-schedule.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorScheduleComponent {
  private readonly staff = inject(StaffService);
  protected readonly days = DAYS_OF_WEEK;
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly form = new FormGroup({
    shifts: new FormArray<ShiftForm>([]),
  });

  protected get shifts(): FormArray<ShiftForm> {
    return this.form.controls.shifts;
  }

  constructor() {
    this.loadSchedule();
  }

  protected loadSchedule(): void {
    this.loading.set(true);
    this.error.set(null);
    this.staff.getMySchedule().subscribe({
      next: (shifts) => {
        this.shifts.clear();
        shifts.forEach((shift) => this.shifts.push(this.createShiftForm(shift)));
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Your schedule could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  protected addShift(): void {
    this.shifts.push(
      this.createShiftForm({
        dayOfWeek: 1,
        shift: 'Morning',
        startTime: '09:00',
        endTime: '12:00',
        appointmentDurationMinutes: 15,
      }),
    );
  }

  protected removeShift(index: number): void {
    this.shifts.removeAt(index);
  }

  protected saveSchedule(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const shifts = this.shifts.getRawValue();
    const uniqueShifts = new Set(
      shifts.map((shift) => `${shift.dayOfWeek}:${shift.shift}`),
    );
    if (uniqueShifts.size !== shifts.length) {
      this.error.set('Each day can have only one Morning and one Evening shift.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);
    this.staff.saveMySchedule(shifts.map(toApiShift)).subscribe({
      next: () => {
        this.notice.set('Your schedule has been saved. Existing appointments remain unchanged.');
        this.saving.set(false);
        this.loadSchedule();
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Your schedule could not be saved.'));
        this.saving.set(false);
      },
    });
  }

  private createShiftForm(shift: DoctorShift): ShiftForm {
    return new FormGroup({
      dayOfWeek: new FormControl(shift.dayOfWeek, {
        nonNullable: true,
        validators: [Validators.min(0), Validators.max(6)],
      }),
      shift: new FormControl(shift.shift, { nonNullable: true }),
      startTime: new FormControl(normalizeTime(shift.startTime), {
        nonNullable: true,
        validators: [Validators.required],
      }),
      endTime: new FormControl(normalizeTime(shift.endTime), {
        nonNullable: true,
        validators: [Validators.required],
      }),
      appointmentDurationMinutes: new FormControl(
        shift.appointmentDurationMinutes,
        {
          nonNullable: true,
          validators: [Validators.required, Validators.min(1), Validators.max(1440)],
        },
      ),
    });
  }
}

function normalizeTime(time: string): string {
  return time.slice(0, 5);
}

function toApiShift(shift: DoctorShift): DoctorShift {
  return {
    ...shift,
    startTime: `${shift.startTime}:00`,
    endTime: `${shift.endTime}:00`,
  };
}
