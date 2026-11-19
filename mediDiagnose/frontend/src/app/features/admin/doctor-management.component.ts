import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AdminService, CreateDoctorProfileInput } from '../../core/api/admin.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { DoctorProfile } from '../../core/models/api.models';

@Component({
  selector: 'app-doctor-management',
  imports: [ReactiveFormsModule],
  templateUrl: './doctor-management.component.html',
  styleUrl: './doctor-management.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorManagementComponent {
  private readonly admin = inject(AdminService);
  protected readonly doctors = signal<DoctorProfile[]>([]);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly editingDoctorId = signal<string | null>(null);
  protected readonly pendingProfile = signal<CreateDoctorProfileInput | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected readonly createForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(200)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8), Validators.maxLength(100)],
    }),
    specialty: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    biography: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)],
    }),
  });

  protected readonly editForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    specialty: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    biography: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)],
    }),
  });

  constructor() {
    this.loadDoctors();
  }

  protected loadDoctors(): void {
    this.loading.set(true);
    this.admin.getDoctorProfiles().subscribe({
      next: (doctors) => {
        this.doctors.set(doctors);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Doctor profiles could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  protected createDoctor(): void {
    if (this.createForm.invalid || this.submitting()) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.error.set(null);
    this.notice.set(null);
    this.submitting.set(true);
    const form = this.createForm.getRawValue();
    this.admin
      .createDoctorAccount({
        name: form.name,
        email: form.email,
        password: form.password,
      })
      .subscribe({
        next: (account) => {
          this.createForm.controls.password.setValue('');
          this.createForm.controls.password.markAsPristine();
          this.createForm.controls.password.markAsUntouched();
          const profile: CreateDoctorProfileInput = {
            userId: account.userId,
            name: form.name,
            specialty: form.specialty,
            biography: form.biography,
          };
          this.pendingProfile.set(profile);
          this.saveDoctorProfile(profile);
        },
        error: (error: unknown) => {
          this.error.set(apiErrorMessage(error, 'The doctor account could not be created.'));
          this.submitting.set(false);
        },
      });
  }

  protected retryDoctorProfile(): void {
    const profile = this.pendingProfile();
    if (profile && !this.submitting()) {
      this.error.set(null);
      this.submitting.set(true);
      this.saveDoctorProfile(profile);
    }
  }

  protected beginEdit(doctor: DoctorProfile): void {
    this.editingDoctorId.set(doctor.id);
    this.editForm.reset({
      name: doctor.name,
      specialty: doctor.specialty,
      biography: doctor.biography,
    });
  }

  protected saveEdit(doctorId: string): void {
    if (this.editForm.invalid || this.submitting()) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.admin
      .updateDoctorProfile(doctorId, {
        ...this.editForm.getRawValue(),
        isActive: true,
      })
      .subscribe({
        next: (updated) => {
          this.doctors.update((doctors) =>
            doctors.map((doctor) => (doctor.id === doctorId ? updated : doctor)),
          );
          this.editingDoctorId.set(null);
          this.notice.set('Doctor profile updated.');
          this.submitting.set(false);
        },
        error: (error: unknown) => {
          this.error.set(apiErrorMessage(error, 'The doctor profile could not be updated.'));
          this.submitting.set(false);
        },
      });
  }

  protected deactivateDoctor(doctor: DoctorProfile): void {
    if (this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.admin.deactivateDoctor(doctor.id).subscribe({
      next: () => {
        this.doctors.update((doctors) => doctors.filter((item) => item.id !== doctor.id));
        this.notice.set(`${doctor.name} was deactivated. Appointment history is preserved.`);
        this.submitting.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'The doctor profile could not be deactivated.'));
        this.submitting.set(false);
      },
    });
  }

  private saveDoctorProfile(profile: CreateDoctorProfileInput): void {
    this.admin.createDoctorProfile(profile).subscribe({
      next: () => {
        this.pendingProfile.set(null);
        this.createForm.reset({
          name: '',
          email: '',
          password: '',
          specialty: '',
          biography: '',
        });
        this.notice.set('Doctor account and profile created.');
        this.submitting.set(false);
        this.loadDoctors();
      },
      error: (error: unknown) => {
        this.error.set(
          apiErrorMessage(
            error,
            'The account exists, but its directory profile was not saved. Retry profile creation; do not create another account.',
          ),
        );
        this.submitting.set(false);
      },
    });
  }
}
