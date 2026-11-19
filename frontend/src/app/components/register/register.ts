import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h1>Register</h1>

      @if (error()) {
        <div class="alert alert-error">{{ error() }}</div>
      }

      <form (ngSubmit)="register()" #regForm="ngForm" class="auth-form">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" name="name" [(ngModel)]="name" required placeholder="Enter name" />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" name="email" type="email" [(ngModel)]="email" required email placeholder="Enter email" />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" name="password" type="password" [(ngModel)]="password" required minlength="8" placeholder="Min 8 characters" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="submitting() || regForm.invalid">
            {{ submitting() ? 'Registering...' : 'Register' }}
          </button>
        </div>
        <p class="auth-link">Already have an account? <a routerLink="/login">Login</a></p>
      </form>
    </div>
  `
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly name = signal('');
  readonly email = signal('');
  readonly password = signal('');
  readonly error = signal<string | null>(null);
  readonly submitting = signal(false);

  register(): void {
    this.submitting.set(true);
    this.error.set(null);

    this.auth.register(this.name(), this.email(), this.password()).subscribe({
      next: () => this.router.navigate(['/products']),
      error: (err) => {
        // Supports both { message } and ASP.NET validation { errors: { Field: [msg] } }
        const body = err?.error;
        const firstValidation = body?.errors ? (Object.values(body.errors)[0] as string[])?.[0] : null;
        this.error.set(body?.message || firstValidation || 'Registration failed');
        this.submitting.set(false);
      }
    });
  }
}
