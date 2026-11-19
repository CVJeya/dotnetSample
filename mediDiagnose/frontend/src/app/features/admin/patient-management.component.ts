import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AdminService } from '../../core/api/admin.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { AdminPatientProfile } from '../../core/models/api.models';

@Component({
  selector: 'app-patient-management',
  imports: [ReactiveFormsModule],
  templateUrl: './patient-management.component.html',
  styleUrl: './patient-management.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientManagementComponent {
  private readonly admin = inject(AdminService);
  protected readonly patients = signal<AdminPatientProfile[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly editingPatientId = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected readonly editForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(200)],
    }),
    phoneNumber: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(30)],
    }),
  });

  constructor() {
    this.loadPatients();
  }

  protected loadPatients(): void {
    this.loading.set(true);
    this.error.set(null);
    this.admin.getPatients().subscribe({
      next: (patients) => {
        this.patients.set(patients);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Patient profiles could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  protected beginEdit(patient: AdminPatientProfile): void {
    this.editingPatientId.set(patient.userId);
    this.editForm.reset({
      name: patient.name,
      email: patient.email,
      phoneNumber: patient.phoneNumber ?? '',
    });
  }

  protected saveEdit(patient: AdminPatientProfile): void {
    if (this.editForm.invalid || this.saving()) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.admin
      .updatePatient(patient.userId, {
        ...this.editForm.getRawValue(),
        isActive: true,
      })
      .subscribe({
        next: (updated) => {
          this.patients.update((patients) =>
            patients.map((item) => (item.userId === updated.userId ? updated : item)),
          );
          this.editingPatientId.set(null);
          this.notice.set('Patient profile updated.');
          this.saving.set(false);
        },
        error: (error: unknown) => {
          this.error.set(apiErrorMessage(error, 'The patient profile could not be updated.'));
          this.saving.set(false);
        },
      });
  }

  protected deactivatePatient(patient: AdminPatientProfile): void {
    if (this.saving()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.admin.deactivatePatient(patient.userId).subscribe({
      next: () => {
        this.patients.update((patients) =>
          patients.filter((item) => item.userId !== patient.userId),
        );
        this.notice.set(
          `${patient.name} was deactivated. Appointment history is preserved.`,
        );
        this.saving.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'The patient profile could not be deactivated.'));
        this.saving.set(false);
      },
    });
  }
}
