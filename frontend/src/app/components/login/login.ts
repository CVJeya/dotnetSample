import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h1>Login</h1>

      @if (error()) {
        <div class="alert alert-error">{{ error() }}</div>
      }

      <form (ngSubmit)="login()" #loginForm="ngForm" class="auth-form">
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" name="email" type="email" [(ngModel)]="email" required placeholder="Enter email" />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input id="password" name="password" type="password" [(ngModel)]="password" required placeholder="Enter password" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="submitting() || loginForm.invalid">
            {{ submitting() ? 'Logging in...' : 'Login' }}
          </button>
        </div>
        <p class="auth-link">Don't have an account? <a routerLink="/register">Register</a></p>
      </form>
    </div>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly email = signal('');
  readonly password = signal('');
  readonly error = signal<string | null>(null);
  readonly submitting = signal(false);

  login(): void {
    this.submitting.set(true);
    this.error.set(null);

    this.auth.login(this.email(), this.password()).subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => {
        this.error.set('Invalid email or password');
        this.submitting.set(false);
      }
    });
  }
}
