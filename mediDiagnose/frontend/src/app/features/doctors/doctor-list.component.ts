import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DoctorService } from '../../core/api/doctor.service';
import { apiErrorMessage } from '../../core/api/api-error-message';
import { DoctorProfile } from '../../core/models/api.models';

@Component({
  selector: 'app-doctor-list',
  imports: [RouterLink],
  templateUrl: './doctor-list.component.html',
  styleUrl: './doctor-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DoctorListComponent {
  private readonly doctorService = inject(DoctorService);
  protected readonly doctors = signal<DoctorProfile[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly searchTerm = signal('');
  protected readonly filteredDoctors = computed(() => {
    const term = this.searchTerm().trim().toLocaleLowerCase();
    if (!term) {
      return this.doctors();
    }

    return this.doctors().filter(
      (doctor) =>
        doctor.name.toLocaleLowerCase().includes(term) ||
        doctor.specialty.toLocaleLowerCase().includes(term),
    );
  });

  constructor() {
    this.loadDoctors();
  }

  protected loadDoctors(): void {
    this.loading.set(true);
    this.error.set(null);
    this.doctorService.getDoctors().subscribe({
      next: (doctors) => {
        this.doctors.set(doctors);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Doctors could not be loaded. Please try again.'));
        this.loading.set(false);
      },
    });
  }

  protected updateSearch(event: Event): void {
    if (event.target instanceof HTMLInputElement) {
      this.searchTerm.set(event.target.value);
    }
  }
}
