import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { apiErrorMessage } from '../../core/api/api-error-message';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.error.set(null);
    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: (session) => {
        this.submitting.set(false);
        const requestedUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        const fallbackUrl = session.role === 'Patient' ? '/appointments' : '/staff';
        const destination =
          requestedUrl?.startsWith('/') && !requestedUrl.startsWith('//')
            ? requestedUrl
            : fallbackUrl;
        void this.router.navigateByUrl(destination);
      },
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'Sign in failed. Check your details and try again.'));
        this.submitting.set(false);
      },
    });
  }
}
