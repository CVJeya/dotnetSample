import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { apiErrorMessage } from '../../core/api/api-error-message';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = new FormGroup({
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
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8), Validators.maxLength(100)],
    }),
  });
  protected readonly submitting = signal(false);
  protected readonly retrying = signal(false);
  protected readonly profileIncomplete = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.profileIncomplete.set(
      this.auth.isAuthenticated() && this.auth.hasPendingProfile(),
    );
  }

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.error.set(null);
    this.profileIncomplete.set(false);
    this.submitting.set(true);
    this.auth.registerPatient(this.form.getRawValue()).subscribe({
      next: (result) => {
        this.submitting.set(false);
        if (result.profileComplete) {
          void this.router.navigate(['/appointments']);
        } else {
          this.profileIncomplete.set(true);
        }
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Your account could not be created. Please try again.'));
        this.submitting.set(false);
      },
    });
  }

  protected retryProfileCompletion(): void {
    if (this.retrying()) {
      return;
    }

    this.retrying.set(true);
    this.error.set(null);
    this.auth.retryProfileCompletion().subscribe({
      next: (complete) => {
        this.retrying.set(false);
        if (complete) {
          void this.router.navigate(['/appointments']);
        }
      },
      error: (error: unknown) => {
        this.error.set(
          apiErrorMessage(
            error,
            'Your profile could not be saved yet. Please check your connection and retry.',
          ),
        );
        this.retrying.set(false);
      },
    });
  }
}
